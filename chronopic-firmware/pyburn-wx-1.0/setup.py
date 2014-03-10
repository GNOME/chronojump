#!/usr/bin/env python
#-*- coding: iso-8859-15 -*-

from distutils.core import setup


files = ["pixmaps/*"]

setup(name         = 'pyburn-wx',
      version      = '1.0',
      description  = 'Grabacion de microcontroladores PICs a bajo nivel',
      author       = 'Juan Gonzalez',
      author_email = 'juan@iearobotics.com',
      url          = 'http://www.iearobotics.com/wiki/index.php?title=Pyburn',
      license      = 'GPL v2 or later',
      packages = ['pyburn_wx'],
      package_data = {'pyburn_wx' : files },
      scripts      = ['pyburn-wx.py',],
      )
