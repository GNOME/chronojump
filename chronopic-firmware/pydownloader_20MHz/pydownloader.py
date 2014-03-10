#!/usr/bin/env python
# -*- coding: iso-8859-15 -*-

# Description: Example of use of libIris
# Copyright (C) 2007 by Rafael Treviño Menéndez
# Author: Rafael Treviño Menéndez <skasi.7@gmail.com>

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
# Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA 02110-1301,USA.

import sys
sys.path = ['..'] + sys.path
import libIris.Pic16_Bootloader
import libIris.Pic16_Firmware
import libIris.IntelHex


#-- Diccionario para asignar al firmware una cadena identificativa
Firmware_DICT = {
  'LEDP1'  : libIris.Pic16_Firmware.ledp1,
  'LEDP2'  : libIris.Pic16_Firmware.ledp2,
  'LEDP'   : libIris.Pic16_Firmware.ledp1,
  'MONITOR': libIris.Pic16_Firmware.generic,
  'SERVOS8': libIris.Pic16_Firmware.servos8,
  'PICP'   : libIris.Pic16_Firmware.picp,
  'ECO'    : libIris.Pic16_Firmware.echo,
}

#-- Cambiar el timeout por defecto para esperar a que responda
#-- el bootloader
libIris.Pic16_Bootloader.DEFAULT_TIMEOUT=800;


#----------------------------------------------------
#-- Mostrar la ayuda de como se usa la aplicacion
#----------------------------------------------------
def help():
  print """
Uso: pydownloader <programa> [<port>]

 -Programa: Puede ser un fichero .hex o el nombre del firmware a cargar:
    -ledp1: Programa de prueba 1. Led parpadeante
    -ledp2: Programa de prueba 2. Led parpadeante a mas velocidad
    -monitor: Servidor generico
    -servos8: Servidor para movimiento de hasta 8 servos
    -Picp   : Servidor para la programacion de PICs
    -eco    : Servidor de eco por el puerto serie (pruebas)
 -Port: Nombre del dispositivo serie a utilizar   
 
 Ejemplo:
    
   1) pydownloader test.hex /dev/ttyUSB0
        Descargar el programa test.hex en la skypic
         
   2) pydownloader ledp1 /dev/ttyUSB0
        Descargar el Firmware de pruebas ledp

  """

#-- Mensaje de comienzo
print 'PyDownloader. Descarga de programas en la Skypic. Licencia GPL\n'

#----------------------------
#-- Analizar los parametros
#----------------------------

#-- Leer el fichero o el nombre del firmware a cargar
try:
  file = sys.argv[1]
except IndexError:
  print "Nombre de programa no indicado"
  help()
  sys.exit(-1)
  
  
#-- Opcional: Leer el puerto serie
try:
  serialName = sys.argv [2]
  
except IndexError:
  #-- Establecer un puerto serie por defecto
  serialName = "/dev/ttyUSB0" 
  
#---------------------------------------------------------------------
#- Obtener el programa a descargar. Bien leyendolo del fichero .hex
#- o bien detectando que es un firmware
#- El programa listo para descargar se devuelve en program
#---------------------------------------------------------------------

try:
  #-- Obtener el firmware
  program = Firmware_DICT[file.upper()]
  print "Firmware: %s" % file
  
#-- No es un firmare conocido. Comprobar si es un fichero  
except KeyError:

  #-- Realizar el parseo del fichero 
  try:
    hr = libIris.IntelHex.HexReader (file)
  except libIris.IntelHex.ReaderError,msg:
    print msg
    sys.exit(-1)
  
  #-- Obtener el programa en el formato correcto
  program = hr.dataBlocks16()
  
  print 'Fichero: "%s"' % file
  
#------------------------------------
#-- Abrir puerto serie
#------------------------------------

try:
  #-- Quitar el argumento logCallback=None para mostrar mas informacion 
  #-- de lo que esta ocurriendo
  iris = libIris.Pic16_Bootloader.Iris(serialName,logCallback=None)
  
#-- Error en puerto Serie  
except libIris.Pic16_Bootloader.IrisError,msg:
  print msg
  sys.exit(-1)

#----------------------------------
#-- Realizar la descarga
#----------------------------------

try:
  iris.download(program)
except libIris.Pic16_Bootloader.IrisError,msg:
  print msg
  sys.exit(-1)
