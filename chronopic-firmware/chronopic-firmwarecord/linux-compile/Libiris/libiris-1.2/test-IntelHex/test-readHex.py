#!/usr/bin/env python
# -*- coding: iso-8859-15 -*-

# Description: Example of use of libIris
# Copyright (C) 2007 by Rafael Treviño Menéndez
# Author: Rafael Treviño Menéndez <skasi.7@gmail.com>
#         Juan Gonzalez <juan@iearobotics.com>

# This program is free software; you can redistribute it and/or
# modify it under the terms of the GNU Library General Public
# License as published by the Free Software Foundation; either
# version 2 of the License, or (at your option) any later version.

# This program is distributed in the hope that it will be useful,
# but WITHOUT ANY WARRANTY; without even the implied warranty of
# MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
# Library General Public License for more details.

# You should have received a copy of the GNU General Public License
# along with this program; if not, write to the Free Software
# Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.

import sys
sys.path = ['..'] + sys.path
import libIris.IntelHex

#------------------------------------------------------------
#- Ejemplo "hola mundo" de parseo.
#------------------------------------------------------------

#-- Analizar los parametros pasados
try:
  file = sys.argv [1]
except IndexError:
  #-- Por defecto se toma el fichero ledp.hex
  file = "ledp.hex"

#-- Imprimir nombre del fichero 
print 'Fichero: "%s"' % file

#-----------------------
#-- Realizar el parseo
#-----------------------

try:
  hr = libIris.IntelHex.HexReader (file)
except libIris.IntelHex.ReaderError,msg:
  print "Error: %s" % msg
  sys.exit(-1)  


#-- Imprimir la lista en formato 1 (memoria)
print hr.memory()
