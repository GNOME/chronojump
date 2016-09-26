from distutils.core import setup
import py2exe
import os

directory=os.path.dirname(os.path.abspath(__file__))
chronojump_importer_path=os.path.join(directory, "chronojump_importer.py")

setup(console=[chronojump_importer_path])
