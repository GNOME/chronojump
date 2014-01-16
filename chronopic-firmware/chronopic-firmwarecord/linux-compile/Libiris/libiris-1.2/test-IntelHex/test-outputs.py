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
#- Pruebas para comprobar las funciones que exportan el .hex a 
#- otros formatos
#------------------------------------------------------------------


#-- Analizar los parametros pasados
try:
  file = sys.argv [1]
except IndexError:
  #-- Por defecto se toma el fichero ledp.hex
  file = "ledp.hex"

#-- Imprimir nombre del fichero 
print 'Fichero: "%s"' % file


#--- Abrir fichero. No se manejan los errores
fd = open (file)

#-----------------------
#-- Realizar el parseo
#-----------------------

#-- Crear elemento de la clase hexreader
try:
  hr = libIris.IntelHex.HexReader (fd)
except libIris.IntelHex.ReaderError,msg:
  print "Error: %s" % msg
  sys.exit(-1)  

  
  
#-----------------------------------------------
#- MOSTRAR CON DIFERENTES FORMATOS DE SALIDA 
#-----------------------------------------------

#-- Vista 1: Como una tabla direccion - contenido
print "---------------------- Vista 1 ---------------------------"
vista1 = hr.outputTable()
print vista1

#-- Vista 2: Direccion - bloque
print "---------------------- Vista 2 ---------------------------"
vista2 = hr.outputBlocks()
print vista2

#-- Vista 3: Direccion - bloque, con bloques de tamano 16 palabras
print "---------------------- Vista 3 ---------------------------"
vista3 = hr.outputBlocks16()
print vista3

#-- Vista 4: Programa en python
print "---------------------- Vista 4 ---------------------------"
vista4 = hr.outputPython()
print vista4
