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

#------------------------------------------------------------------
#- Pruebas de los diferentes formatos
#- Se parsea un fichero y se muestran en los FORMATOS 1, 2 y 3
#------------------------------------------------------------------

#-- Analizar los parametros pasados
try:
  file = sys.argv [1]
except IndexError:
  #-- Por defecto se toma el fichero ledp.hex
  file = "ledp.hex"

#-- Imprimir nombre del fichero 
print 'Fichero: "%s"\n' % file


#-----------------------
#-- Realizar el parseo
#-----------------------

try:
  hr = libIris.IntelHex.HexReader (file)
except libIris.IntelHex.ReaderError,msg:
  print "Error: %s" % msg
  sys.exit(-1)  


#-----------------------------
#- Realizar las conversiones
#-----------------------------


#-- Imprimir las listas
print "---------- FORMATO 1 ---------------------------------"
print hr.memory()
print ""

print "---------- FORMATO 2 ---------------------------------"
print hr.dataBlocks()
print ""

print "---------- FORMATO 3 ---------------------------------"
print hr.dataBlocks16()
print ""
