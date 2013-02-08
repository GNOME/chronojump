from distutils.core import setup
import py2exe
import os

#solve pygame py2exe font problem on 32 bits
#http://thadeusb.com/weblog/2009/4/15/pygame_font_and_py2exe
origIsSystemDLL = py2exe.build_exe.isSystemDLL
def isSystemDLL(pathname):
       if os.path.basename(pathname).lower() in ["sdl_ttf.dll"]:
               return 0
       return origIsSystemDLL(pathname)
py2exe.build_exe.isSystemDLL = isSystemDLL

#setup(console=['pyserial_pyper_windows.py'])
setup(console=['../encoder/pyserial_pyper_windows.py'])
