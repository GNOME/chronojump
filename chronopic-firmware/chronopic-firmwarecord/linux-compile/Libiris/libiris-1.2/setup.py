#!/usr/bin/env python

from distutils.core import setup

setup(name         = 'libIris',
      version      = '1.2',
      description  = 'Libreria para descarga de Firmware en la skypic',
      author       = 'Rafael Trevino, Juan Gonzalez',
      author_email = 'skasi.7@gmail.com>',
      url          = 'http://www.iearobotics.com/wiki/index.php?title=LibIris',
      packages     = ['libIris'],
      license      = 'GPL v2 or later',
      data_files   = [('share/man/man3',['debian/libiris.3'])],
      scripts      = ['libIris-utils/hex-view.py', \
                      'libIris-utils/hex2python.py', \
                      'libIris-utils/skypic-test.py'],
      )
