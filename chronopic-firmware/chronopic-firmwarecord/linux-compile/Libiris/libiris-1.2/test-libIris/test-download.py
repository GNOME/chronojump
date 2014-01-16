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

#---------------------------------------------------------------
#--  Pruebas de la libreria Pic16_Bootloader para descargar un 
#--  fichero .hex en la skypic      
#----------------------------------------------------------------

import sys
sys.path = ['..'] + sys.path
import libIris.Pic16_Bootloader
import libIris.IntelHex


#-- Puerto serie a emplear
DISPOSITIVO_SERIE = "/dev/ttyUSB0"

#-- Fichero a descargar
FICHERO = "ledp1.hex"


#-- Abrir puerto serie
try:
  iris = libIris.Pic16_Bootloader.Iris(DISPOSITIVO_SERIE)
except libIris.Pic16_Bootloader.IrisError,msg:
  print msg
  sys.exit(-1)

#-- Abrir y analizar fichero .hex
try:
  hr = libIris.IntelHex.HexReader (FICHERO)
except libIris.IntelHex.ReaderError,msg:
  print "Error: %s" % msg
  sys.exit(-1)  

#-- Obtener el programa en el formato adecuado
program = hr.dataBlocks16()

#-- Realizar la descarga
try:
  iris.download(program)
except libIris.Pic16_Bootloader.IrisError,msg:
  print "\nError: %s" % msg
  sys.exit(-1)
