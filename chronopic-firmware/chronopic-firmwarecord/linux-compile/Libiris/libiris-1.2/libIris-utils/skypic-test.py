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

#------------------------------------------------------------------------
#--  Programa de pruebas para hacer un test de descarga en la skypic
#------------------------------------------------------------------------

import sys
sys.path = ['..'] + sys.path
import libIris.Pic16_Bootloader
import libIris.IntelHex
import libIris.Pic16_Firmware

#----------------------------------------------------
#-- Mostrar la ayuda de como se usa la aplicacion
#----------------------------------------------------
def help():
  print """
Uso: skypic-test <port>

 -Port: Nombre del dispositivo serie donde esta conectada la Skypic
 
 Ejemplo:
    
   $ skypic-test.py /dev/ttyUSB0
  """


#-- Mensaje de comienzo
print 'Skypic-test. Prueba rapida de la Skypic. Licencia GPL\n'

#----------------------------
#-- Analizar los parametros
#----------------------------

#-- Leer el fichero o el nombre del firmware a cargar
try:
  disp = sys.argv[1]
except IndexError:
  print "Nombre de programa no indicado"
  help()
  sys.exit(-1)


#-- Abrir puerto serie
try:
  iris = libIris.Pic16_Bootloader.Iris(disp)
except libIris.Pic16_Bootloader.IrisError,msg:
  print msg
  sys.exit(-1)

#-- Obtener firmware a descargar. Se puede utilizar cualquier de 
#-- los que se encuentren en la libreria Pic16_Firmware
program = libIris.Pic16_Firmware.ledp2

#-- Realizar la descarga
try:
  iris.download(program)
except libIris.Pic16_Bootloader.IrisError,msg:
  print "\nError: %s" % msg
  sys.exit(-1)
