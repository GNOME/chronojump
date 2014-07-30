#!/usr/bin/env python
# chronojump
#
# Copyright (c) 2010, Andoni Morales Alastruey <ylatuya@gmail.com>
#
# This program is free software; you can redistribute it and/or
# modify it under the terms of the GNU Lesser General Public
# License as published by the Free Software Foundation; either
# version 2.1 of the License, or (at your option) any later version.
#
# This program is distributed in the hope that it will be useful,
# but WITHOUT ANY WARRANTY; without even the implied warranty of
# MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
# Lesser General Public License for more details.
#
# You should have received a copy of the GNU Lesser General Public
# License along with this program; if not, write to the
# Free Software Foundation, Inc., 59 Temple Place - Suite 330,
# Boston, MA 02111-1307, USA

from distutils.core import setup
import sys
import shutil
import os
import tempfile

sys.argv = [sys.argv[0], 'py2exe']

sys.stdout = open(os.path.join(tempfile.gettempdir(), 'chronopic-firmwarecord-setup-stdout.log'),"w")
sys.stderr = open(os.path.join(tempfile.gettempdir(), 'chronopic-firmwarecord-setup-stderr.log'),"w")

try:
    import py2exe
except:
    print "Could not import 'py2exe', py2exe is not installed in your system."
    exit(1)

try:
    import wx
except:
    print "Could not import 'wx', wxPython is not installed in your system."
    exit(1)

try:
    import serial
except:
    print "Could not import 'serial', pyserial is not installed in your system."
    exit(1)

shutil.copy ("chronopic-firmwarecord.in", "chronopic-firmwarecord.py")

setup(
	name = 'ChonopicFirmwareRecorder',
        description = 'Chronopic firmware recoder',
        version = '0.0.1',

        windows = [
                     {
                        'script': 'chronopic-firmwarecord.py',
                        #'icon_resources': [(1, "chronojump.ico")],
                     }
                 ],

        options = {
                     'py2exe': {
                       'packages':'chronopic-firmwarecord',
                       'includes': 'wx, serial',
                     }
                  },

            zipfile = None,
        )

shutil.copy ("msvcp71.dll", "dist/msvcp71.dll")
