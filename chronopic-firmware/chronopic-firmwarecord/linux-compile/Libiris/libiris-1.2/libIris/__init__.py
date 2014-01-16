#! /usr/bin/python
# -*- coding: iso-8859-15 -*-

#-- Paqute LibIris


# Description: File downloader library for SkyPIC
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

""" 
  Paquete LibIris: Descarga de programas en la tarjeta skypic

Este paquete esta formado por tres modulos:

  * IntelHex        : Lectura de ficheros en formato .hex de Intel
  * Pic16_Bootloader: Descarga de ficheros a traves del bootloader
  * Pic16_Firmware  : Programas para el PIC16F876A. Incluye los servidores
                      del proyecto stargate y programas de prubas, como el 
                      ledp
                      
Incluye ademas las siguientes utilidades:

  * hex-view  :  Visualizacion de ficheros .hex
  * hex2python:  Convertir un fichero .hex a un script en python que contiene
                 el codigo maquina en una lista
  * skypic-test: Prueba de descargas en la skypic. Se graba el programama del
                 ledp. Permite comprobar si la skypic esta funcionando
                 correctamente


"""

#-- Version de la libIris
VERSION = 1.2
