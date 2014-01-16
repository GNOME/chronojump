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

def help():
  print "Forma de uso: hex2python <fichero.hex> [formato bloques] [nombre lista]"
  print "-formato de los bloques:"
  print "  f2: Bloques contiguos"
  print "  f3: Bloques de tamano maximo 16 palabras"
  print "Ejemplo: "
  print "  hex2python ledp.hex f2 ledp"  


#--------------------------
#-- Analizar argumentos  
#--------------------------
# --- Primer argumento: fichero .hex
try:
  file = sys.argv [1]
except IndexError:
  help()
  sys.exit(0)

FORMATOS = ['f2','f3']

#--- Segundo argumento (opcional): Formato de los bloques
try:
  formato = sys.argv[2]
except IndexError:
  formato = "f2"

#--- tercer argumento [opcional]: Nombre de la lista
try:
  prog_name = sys.argv[3]
except IndexError:
  prog_name = "prog"
  
#-- Comprobar si el formato especificado es correcto
if not formato in FORMATOS:
  print "Formato de bloques desconocido"
  print ""
  help()
  sys.exit(-1)  
  
  
#-----------------------
#-- Realizar el parseo
#-----------------------

try:
  hr = libIris.IntelHex.HexReader (file)
except libIris.IntelHex.ReaderError,msg:
  sys.stderr.write("Error: %s" % str(msg))
  sys.stderr.write("\n")
  sys.exit(-1)
  
#------------------------
#-- Generar la salida
#------------------------

#-- Obtener la cadena de salida en formato python
if formato == FORMATOS[0]:
  cad = hr.outputPython(prog_name)
  
else:
  cad = hr.outputPython16(prog_name)


#-- Sacar por la salida estandar
sys.stdout.write("\n")
sys.stdout.write(cad)
sys.stdout.write("\n")
