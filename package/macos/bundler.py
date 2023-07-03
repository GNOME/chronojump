# cerbero - a multi-platform build system for Open Source software
# Copyright (C) 2013 Andoni Morales Alastruey <ylatuya@gmail.com>
#
# This library is free software; you can redistribute it and/or
# modify it under the terms of the GNU Library General Public
# License as published by the Free Software Foundation; either
# version 2 of the License, or (at your option) any later version.
#
# This library is distributed in the hope that it will be useful,
# but WITHOUT ANY WARRANTY; without even the implied warranty of
# MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
# Library General Public License for more details.
#
# You should have received a copy of the GNU Library General Public
# License along with this library; if not, write to the
# Free Software Foundation, Inc., 59 Temple Place - Suite 330,
# Boston, MA 02111-1307, USA.
import os
import re
import platform
import subprocess
import shutil
from pathlib import Path


def run(cmd):
    return subprocess.run(cmd, capture_output=True).stdout.decode('utf-8').splitlines()


class RecursiveLister():

    def list_file_deps(self, prefix, path):
        raise NotImplemented()

    def find_deps(self, prefix, lib, state={}, ordered=[]):
        if state.get(lib, 'clean') == 'processed':
            return
        if state.get(lib, 'clean') == 'in-progress':
            return
        state[lib] = 'in-progress'
        lib_deps = self.list_file_deps(prefix, lib)
        for libdep in lib_deps:
            self.find_deps(prefix, libdep, state, ordered)
        state[lib] = 'processed'
        ordered.append(lib)
        return ordered

    def list_deps(self, prefix, path):
        return self.find_deps(prefix, os.path.realpath(path), {}, [])


class ObjdumpLister(RecursiveLister):

    def list_file_deps(self, prefix, path):
        env = os.environ.copy()
        env['LC_ALL'] = 'C'
        cmd = ['objdump', '-xw', path]
        files = subprocess.run(cmd, capture_output=True).stdout.decode('utf-8').splitlines()
        prog = re.compile(r"(?i)^.*DLL[^:]*: (\S+\.dll)$")
        files = [prog.sub(r"\1", x) for x in files if prog.match(x) is not None]
        files = [os.path.join(prefix, 'bin', x) for x in files if
                 x.lower().endswith('dll')]
        return [os.path.realpath(x) for x in files if os.path.exists(x)]


class OtoolLister(RecursiveLister):

    def list_file_deps(self, prefix, path):
        cmd = ['otool', '-L', path]
        files = subprocess.run(cmd, capture_output=True).stdout.decode('utf-8').splitlines()[1:]
        # Shared libraries might be relocated, we look for files with the
        # prefix or starting with @rpath
        files = [x.strip().split(' ')[0]
                 for x in files if prefix in x or "@rpath" in x]
        rpaths = self._get_rpaths(path, prefix)
        return [self._replace_rpath(x, prefix, rpaths) for x in files]

    def _get_rpaths(self, path, prefix):
        rpaths = []
        cmd = ['otool', '-L', path]
        lines_iter = iter(subprocess.run(cmd, capture_output=True).stdout.decode('utf-8').splitlines())
        while True:
            line = next(lines_iter, None)
            if line is None:
                break
            elif "cmd LC_RPATH" in line:
                line = next(lines_iter)
                rpath = next(lines_iter).strip().split(' ')[1]
                if len(rpath) == 1:
                    rpath = rpath.replace(".", prefix)
                else:
                    rpath = rpath.replace(
                        "@loader_path", os.path.dirname(path))
                    rpath = rpath.replace(
                        "@executable_path", os.path.dirname(path))
                rpaths.append(rpath)
        return rpaths

    def _replace_rpath(self, path, prefix, rpaths):
        for rpath in rpaths:
            translatedPath = path.replace("@rpath", rpath)
            if os.path.exists(translatedPath):
                return translatedPath
        return path


class LddLister():

    def list_deps(self, prefix,  path):
        cmd =  ['ldd', path]
        files = subprocess.run(cmd, capture_output=True).stdout.decode('utf-8').splitlines()
        return [x.split(' ')[2] for x in files if prefix in x]


class DepsTracker():

    BACKENDS = {
        "Windows": ObjdumpLister,
        "Linux" : LddLister,
        "Darwin" : OtoolLister}

    def __init__(self, platform, prefix):
        self.libs_deps = {}
        self.prefix = prefix
        if self.prefix[:-1] != '/':
            self.prefix += '/'
        self.lister = self.BACKENDS[platform]()

    def list_deps(self, path):
        deps = self.lister.list_deps(self.prefix, path)
        rdeps = []
        for d in deps:
            if os.path.islink(d):
                rdeps.append(os.path.realpath(d))
        return [x.replace(self.prefix, '') for x in deps + rdeps]


INT_CMD = 'install_name_tool'
OTOOL_CMD = 'otool'


class OSXRelocator(object):
    '''
    Wrapper for OS X's install_name_tool and otool commands to help
    relocating shared libraries.

    It parses lib/ /libexec and bin/ directories, changes the prefix path of
    the shared libraries that an object file uses and changes it's library
    ID if the file is a shared library.
    '''

    def __init__(self, root, lib_prefix, recursive, logfile=None):
        self.root = root
        self.lib_prefix = self._fix_path(lib_prefix)
        self.recursive = recursive
        self.use_relative_paths = True
        self.logfile = None

    def relocate_file(self, object_file):
        self.change_libs_path(object_file)

    def change_id(self, object_file, id=None):
        id = id or object_file.replace(self.lib_prefix, '@rpath')
        filename = os.path.basename(object_file)
        if not self._is_mach_o_file(filename):
            return
        cmd = [INT_CMD, '-id', id, object_file]
        subprocess.run(cmd)

    def change_libs_path(self, object_file):
        depth = len(str(object_file.parent).split('/'))
        p_depth = '/..' * depth
        rpaths = ['.']
        rpaths += ['@loader_path' + p_depth, '@executable_path' + p_depth]
        rpaths += ['@loader_path' + '/../lib', '@executable_path' + '/../lib']
        if not self._is_mach_o_file(object_file):
            return
        if depth > 1:
            rpaths += ['@loader_path/..', '@executable_path/..']
        for p in rpaths:
            cmd = [INT_CMD, '-add_rpath', p, object_file]
            subprocess.run(cmd)
        for lib in self.list_shared_libraries(object_file):
            if self.lib_prefix in lib:
                prefix = lib[:lib.find('/lib/')] + '/lib'
                new_lib = lib.replace(prefix, '@rpath')
                cmd = [INT_CMD, '-change', lib, new_lib, object_file]
                subprocess.run(cmd)

    def change_lib_path(self, object_file, old_path, new_path):
        for lib in self.list_shared_libraries(object_file):
            if old_path in lib:
                new_path = lib.replace(old_path, new_path)
                cmd = [INT_CMD, '-change', lib, new_path, object_path]
                subprocess.run(cmd)

    @staticmethod
    def list_shared_libraries(object_file):
        cmd = [OTOOL_CMD, '-L', object_file]
        res = subprocess.run(cmd, capture_output=True).stdout.decode('utf-8').splitlines()
        # We don't use the first line
        libs = res[1:]
        # Remove the first character tabulation
        libs = [x[1:] for x in libs]
        # Remove the version info
        libs = [x.split(' ', 1)[0] for x in libs]
        return libs

    def _fix_path(self, path):
        if path.endswith('/'):
            return path[:-1]
        return path

    def _is_mach_o_file(self, filename):
        return os.path.splitext(filename)[1] in ['.dylib', '.so'] or \
                 subprocess.run(['file', '-bh', filename], capture_output=True).stdout.decode('utf-8').startswith('Mach-O')


class Main(object):

    def run(self):
        import optparse
        usage = "usage: %prog library_path prefix"
        description = 'List all shared libraries dependencies from the prefix '
        parser = optparse.OptionParser(usage=usage, description=description)

        options, args = parser.parse_args()
        if len(args) != 2:
            parser.print_usage()
            exit(1)
        prefix = args[1]

        tracker = DepsTracker(platform.system(), prefix)
        deps = tracker.list_deps(args[0])
        deps = [os.path.join(prefix, x) for x in deps]
        deps.sort()
        print('\n'.join(deps))

        libs = Path('out')
        if not libs.exists():
            libs.mkdir()
        os.chdir(libs)
        relocator = OSXRelocator(prefix, prefix, False)
        for dep in deps:
            dep_no_prefix = dep[dep.find('/lib/') + 1:]
            outPath = Path(dep_no_prefix)
            if not outPath.parent.exists():
                outPath.parent.mkdir()
            shutil.copy(dep, outPath)
            outPath.chmod(0o777)
            relocator.relocate_file(outPath)

        exit(0)


if __name__ == "__main__":
    main = Main()
    main.run()
