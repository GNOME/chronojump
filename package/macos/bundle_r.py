#!/usr/bin/env python3

import argparse
import os
import pathlib
import re
import shutil
import subprocess
from stat import S_IREAD, S_IRGRP, S_IROTH, S_IWUSR

# Match against non-system libraries
OTOOL_LIB_REGEX = re.compile(r"(/Library/Frameworks/R.framework/[^\(^\)]+) \(.*\)")


def run_install_name_tool(src_lib, referenced_path):
    # Make writable by user.
    os.chmod(src_lib, S_IREAD | S_IRGRP | S_IROTH | S_IWUSR)

    # Run install_name_tool to fix up the absolute paths to the library
    # dependencies.
    dep_path_basename = os.path.basename(referenced_path)
    dep_lib_name = os.path.realpath(referenced_path).replace("/Library/Frameworks/R.framework/", "")
    dep_lib = os.path.join("/Applications/Chronojump.app/Contents/Resources/", os.path.basename(args.resource_dir), dep_lib_name)
    #print(f"dep_path_basename={dep_path_basename}")
    #print(f"dep_lib_name={dep_lib_name}")
    #print(f"dep_lib={dep_lib}")
    cmd = ['install_name_tool',
           '-change', referenced_path, dep_lib,
           #'-change', f"@rpath/{dep_path_basename}", dep_lib,
           src_lib]
    subprocess.check_output(cmd)


def refresh_libs(src_lib):
    """
    Use otool -L to collect the library dependencies.
    """
    cmd = ['otool', '-L', src_lib]
    output = subprocess.check_output(cmd).decode('utf-8')
    referenced_paths = re.findall(OTOOL_LIB_REGEX, output)

    for referenced_path in referenced_paths:
        #print(referenced_path)
        run_install_name_tool(src_lib, referenced_path)
            

def traverse_directory(path):
    for root, dirs, files in os.walk(path):
        #print(f"Current path: {root}")
        for dir_name in dirs:
            traverse_directory(os.path.join(root, dir_name))
        for file_name in files:
            if file_name.lower() == f"r" or file_name.lower() == f"rscript" or file_name.lower().endswith(f".dylib") or file_name.lower().endswith(f".a") or file_name.lower().endswith(f".so"):
                #print(f"File: {os.path.join(root, file_name)}")
                refresh_libs(os.path.join(root, file_name))


parser = argparse.ArgumentParser(description='Bundle R binaries.')
parser.add_argument('--resource_dir',
                    type=pathlib.Path,
                    required=True,
                    help='R directory.')
args = parser.parse_args()

traverse_directory(args.resource_dir)
