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

#---------------------------------------------------------
#-- Visualizacion de ficheros .HEX 
#---------------------------------------------------------

import sys
sys.path = ['..'] + sys.path
import libIris.IntelHex

FORMATOS = ['f1','f2','f3']

def help():
  print "Forma de uso: hex-view <fichero.hex> [Formato salida]"
  print "-Formato de salida:"
  print "   f1: Tabla con direccion - contenido. Formato por defecto"
  print "   f2: Direccion - bloques contiguos"
  print "   f3: Direccion - bloques de tamano maximo 16 palabras"
  print "-Ejemplo:"
  print "  hex-view ledp.hex f2"


#--------------------------
#-- Analizar argumentos  
#--------------------------
# --- Primer argumento: fichero .hex
try:
  file = sys.argv [1]
except IndexError:
  print "Error. Argumentos incorrectos"
  print ""
  help()
  sys.exit(-1)

#--- Segundo argumento [opcional]: Formato
try:
  formato = sys.argv[2]
except IndexError:
  formato = "f1"

#-- Comprobar si el formato especificado es correcto
if not formato in FORMATOS:
  print "Formato desconocido"
  print ""
  help()
  sys.exit(-1)

#-- Imprimir nombre del fichero 
print '\nFichero: "%s"' % file

#-----------------------
#-- Realizar el parseo
#-----------------------

try:
  hr = libIris.IntelHex.HexReader (file)
except libIris.IntelHex.ReaderError,msg:
  print "Error: %s" % msg
  sys.exit(-1)  
  
print "Tamano : %d palabras" % hr.size()

#--------------------------------------------
# Mostrar el fichero con el formato elegido
#--------------------------------------------
if formato == FORMATOS[0]:
  print ""
  print (hr.outputTable())
  
elif formato == FORMATOS[1]:
  print "Bloques: %d" % hr.blocks()
  print ""
  print hr.outputBlocks()

elif formato == FORMATOS[2]:
  print "Bloques: %d" % hr.blocks16()
  print ""
  print hr.outputBlocks16()
