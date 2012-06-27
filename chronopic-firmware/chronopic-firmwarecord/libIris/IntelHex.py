#!/usr/bin/env python
# -*- coding: iso-8859-15 -*-

# Description: File downloader library for SkyPIC
# Copyright (C) 2007 by Rafael Treviño Menéndez
# Author: Rafael Treviño Menéndez <skasi.7@gmail.com>
#         Juan Gonzalez Gomez <juan@iearobotics.com>

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




#------------------------------------------------------------------------------
"""
 Libreria para el analisis de ficheros .hex que esten en formato Hex de 
 Intel. Se incluyen ademas funciones para realizar conversiones entre
 diferentes formas de representacion de los programas en codigo maquina
 
 FORMATOS DE DATOS. En esta libreria se usan tres tipos de formatos 
  para representar la informacion que hay en un fichero .HEX

 -FORMATO 1: Memoria (mem). Se asocia cada direccion con su palabra.
    Es una lista de tuplas (direccion,palabra). La palabra se representa
    a su vez mediante una otra tupla con su byte alto y su byte bajo
    El formato lo podemos expresar asi: 
    mem = [tupla1, tupla2....] = [(dir1, pal1), (dir2,pal2), ...] =
        = [ (dir1, (dato1h,dato1l)), (dir2,(dato2h,dato2l)), ... ]
    Este es el formato mas generico

 -FORMATO 2: Lista de bloques. Los contenidos que estan en posiciones de
    memoria consecutivas se agrupan en bloques. La representacion es una
    lista de bloques:
    programa = [bloque1, bloque2.....] Cada uno de estos bloques es a su 
    vez una lista de PALABRAS (no bytes). La primera palabra es la direccion
   de comienzo del bloque
    bloque1 = [direccion, palabra1, palabra2,.....]

 -FORMATO 3: Lista de bloques de 16 palabras. Es el mismo que el formato 2
    pero ahora los bloques son como maximo de 16 palabras

 USO DE LOS FORMATOS

 -FORMATO 1: Es el mas generico. Contiene tuplas con las direcciones y 
             las palabras almacenadas. 
 -FORMATO 2: Bloques contiguos de palabras. Su principal utilidad es para
             almacenar programas que se grabaran en el PIC. Los bloques
             pueden ser de cualquier tamano
 -FORMATO 3: Bloques contiguos de como maximo 16 palabras. Es igual que el 
             formato 2 pero con la limitacion de tamano de bloques. Su 
             principal utilidad es para la carga de programas con el 
             Bootloader. Por cada bloque de 16 palabras se envia un 
             checksum para comprobar que el envio es correcto
 
"""
#------------------------------------------------------------------------------


class ReaderError (Exception):
#----------------------------------------------------------------------------
  """
  Excepciones producidas en el modulo IntelHex
  """
#----------------------------------------------------------------------------  
  pass




class HexReader:
#----------------------------------------------------------------------------
  """
  Clase principal para la lectura de ficheros .HEX y realizar conversiones
  """
#----------------------------------------------------------------------------

  
  def __init__ (self, file):
  #----------------------------------------------------------------------------
    """
    Inicializacion de la clase. Se le pasa el fichero que se quiere parsear
    Bien se puede pasar el descriptor o el nombre del fichero
    """
  #----------------------------------------------------------------------------
  
    #-- Comprobar si lo que se pasa es una cadena con el nombre del 
    #-- fichero. En ese caso se abre el fichero para obtener su descriptor
    if isinstance (file, str): 
    
      #-- Abrir el fichero o devolver una excepcion si hay error
      try:
        fd = open (file)
      except IOError,msg:
        raise ReaderError, msg
        
      #-- Indicar que se especifico el fichero por su nombre  
      fileName=True;  
        
    else:
      fd = file; #-- El argumento se toma directamente como un descriptor
      fileName=False;
      
    #-- Realizar el parseo y obtener el contenido en formato 2
    self.__memory = readHex(fd)
    
    #-- En el caso de que se haya especificado el fichero mediante un
    #-- nombre se cierra el fichero. En caso contrario se deja abierto
    if fileName:
      fd.close()
    
  
  def memory (self):
  #-------------------------------------------------------------
    """
    Devolver el fichero .HEX en formato 1, como una memoria  
    """
  #-------------------------------------------------------------
    return self.__memory


  def dataBlocks(self):
  #----------------------------------------------------------------------  
    """
    Devolver el fichero .HEX como una lista de bloques en formato 2
    """
  #----------------------------------------------------------------------
    return memToBlocks(self.__memory)  

  
  
  def dataBlocks16(self): 
  #----------------------------------------------------------------------
    """
    Devolver el fichero .Hex como una lista de bloques en formato 3
    """
  #----------------------------------------------------------------------
    return memToBlocks16(self.__memory) 

  
  
  def size(self):
  #-------------------------------------------------------------------------
    """
    Devolver el tamano en palabras. No se distingue entre codigo y datos
    """
  #-------------------------------------------------------------------------
    return len(self.__memory)

  
  def blocks(self):
  #------------------------------------------------------------------
    """
    Devolver el numero de bloques 
    """
  #------------------------------------------------------------------
    blocks = memToBlocks(self.__memory)
    return len(blocks)
    
  
  def blocks16(self):
  #------------------------------------------------------------------
    """
    Devolver el numero de bloques con tamano maximo de 16 palabras
    """
  #------------------------------------------------------------------
    blocks = memToBlocks16(self.__memory)
    return len(blocks)  

  
  
  def outputPython(self,name="prog"):
  #-------------------------------------------------------------------
    """
    Devuelve una cadena de Salida en formato python con el codigo
    maquina del fichero .hex
    """
  #-------------------------------------------------------------------
    
    #-- Convertir a bloques y luego a cadena python
    blocks = memToBlocks(self.__memory)
    return blocksToStrPython(blocks,name)
    
  
  def outputPython16(self,name="prog"):
  #-------------------------------------------------------------------
    """
    Devuelve una cadena de Salida en formato python con el codigo
    maquina del fichero .hex
    La lista esta formada por bloques con tamano menor o igual a 16
    """
  #-------------------------------------------------------------------
    
    #-- Convertir a bloques y luego a cadena python
    blocks = memToBlocks16(self.__memory)
    return blocksToStrPython(blocks,name)  
  
  
  def outputTable(self):
  #------------------------------------------------------------
    """
    Salida como una tabla Direccion - Contenido
    """
  #------------------------------------------------------------  
    return memToStrTable(self.__memory)
   
  
  def outputBlocks(self):
  #----------------------------------------------------------
    """
    Salida como bloques en formato 2. Direccion - Bloque
    """
  #----------------------------------------------------------
    blocks = memToBlocks(self.__memory)
    return blocksToStr(blocks)
    
  
  def outputBlocks16(self):
  #----------------------------------------------------------
    """
    Salida como bloques en formato 3. Direccion - Bloque
    """
  #----------------------------------------------------------
    blocks = memToBlocks16(self.__memory)
    return blocksToStr(blocks)




#------------------------------------------------------------------------------
#           FUNCIONES ESTATICAS QUE SE PUEDEN INVOCAR DIRECTAMENTE
#------------------------------------------------------------------------------



def readHex (fd):
#--------------------------------------------------------------
  """
  Funciona para analizar ficheros .HEX.
  ENTRADAS: Descriptor del fichero
  DEVUELVE: Una lista en el FORMATO 1 (memoria)
  """
#--------------------------------------------------------------

  #-- Leer las lineas del fichero .hex
  lines = fd.readlines ()
  fd.close ()

  # Inicializar la lista de salida
  mem = []

  #-- Recorrer todas las lineas del fichero
  for line in lines:
  
    #-- FORMAT .HEX
    #-- CAMPO 1. (1 byte) Comienzo de linea. Caracter ':'
    if line [0] != ':':
      raise ReaderError, 'Error en formato HEX: Comienzo de linea incorrecto'

    #-- CAMPO 2. (2 bytes) Numero de bytes de los datos
    count = int (line [1:3], 16)
    
    #-- CAMPO 3. (2 bytes) Direccion de comienzo de los datos
    addr = int (line [3:7], 16) / 2
    
    #-- CAMPO 4. (1 byte). Tipo de comando (registro)
    rectype = int (line [7:9], 16)

    #-- El registro de tipo 1 indica que es el final del fichero
    #-- Si es asi se termina y se devuelve el contenido leido
    if rectype == 1:
      return mem

    #-- Si es un registro mayor a 1 se ignora
    #-- Los registro de tipo 4 no tengo muy claro para que son
    #-- Creo que indican cual es la direccion de comienzo del
    #-- programa
    #-- Los registros normales son los de tipo 0 (datos)
    if rectype > 1:
      continue

    #-- Inicializar Checksum
    chk = count + (addr * 2 & 0xFF) + (addr >> 7) + rectype

    #-- CAMPO 5: Datos. Una cadena de "count" bytes. Se deben interpretar
    #-- como palabras. El primer byte es el bajo y el segundo el alto
    for loop in xrange (0, count / 2):
      #-- Crear la tupla con el (byte alto, byte bajo)
      data = (int (line [11 + 4 * loop: 13 + 4 * loop], 16),
              int (line [9 + 4 * loop: 11 + 4 * loop], 16))
      
      #-- Actualizar checksum      
      chk += data [0] + data [1]        
              
      #-- En el pic las palabras son de 14 bits por lo que el byte alto
      #-- NUNCA puede ser mayor de 0x3F        
      if data [0] > 0x3F:
        raise ReaderError, 'Error en formato HEX: Palabra incorrecta'
        
      #-- Anadir la tupla con la direccion y los datos  
      mem.append ((addr, data))
      
      #-- Incrementar la direccion
      addr += 1
      
    #-- CAMPO 6: Checksum del fichero
    checksum = int (line [9 + count * 2: 11 + count * 2], 16)
    chk = (0x100 - chk & 0xFF) & 0xFF  

    #-- Comprobación del checksum. Ver si el checksum del fichero es igual
    #-- al calculado.
    if chk != checksum:
      raise ReaderError, 'Error en formato HEX: Fallo en checksum'

  raise ReaderError, 'Error en formato HEX: Final erroneo'
  


def memToBlocks(mem):
#---------------------------------------------------------------------- 
  """
  Conversion del FORMATO 1 (memoria) al FORMATO 2: lista de bloques
  contiguos
  ENTRADA: Lista en formato 1 (memoria)
  DEVUELVE: Lista en FORMATO 2
  """
#----------------------------------------------------------------------

  #-- obtener una copia local de la memoria para no borrar la original
  data = [] + mem

  #-- Obtener la primera tupla
  address, (d0, d1) = data [0]
  del data [0]
  a = address

  #-- Inicializar programa. Un programa es una lista de bloques contiguos
  #-- de palabras
  program = []
  
  #-- Comenzar el primer bloque. Situar el primer elemento
  block = [a, (d0 * 0x100 + d1)]

  #-- Repetir para cada palabra del fichero .hex
  while len (data):
  
    #-- Obtener la siguiente palabra y su direccion
    address, (d0, d1) = data [0]
    del data [0]
    
    #-- Si la palabra esta a continuacion de la anterior
    if address== a + 1: 
      #-- Anadir palabra al bloque
      block.append (d0 * 0x100 + d1)
      a = address
    else:  
      #-- La palabra NO es contigua
      #-- Hay dos casos:
      #-- 1) Que este en el mismo subbloque de 8. En ese caso se considera
      #-- que forman parte del mismo bloque. Los "gaps" se rellenan con ceros
      #-- 2) Que esten en diferentes subbloques. Eso significa que 
      #--    pertenecen a bloques separados.
      if address/8 == (a+1)/8:  #-- Caso 1. Mismo subbloque
        block.extend ((address - (a + 1)) * [0])
        block.append (d0 * 0x100 + d1)
        a = address
      else:   #-- Caso 2: Distinto Bloque
        #-- Anadir el bloque actual al programa
        #-- Pero SOLO si es un bloque de codigo. Es decir, si su direccion
        #-- de inicio esta por debajo de 0x2000. A partir de esa direccion
        #-- lo que se tiene es la configuracion
        program.append (block)
        
        #-- Crear el bloque nuevo. Meter el primer elemento
        a = address
        block = [a, (d0 * 0x100 + d1)]
      
  #-- Falta por añadir al programa el ultimo bloque leido 
  program.append (block)
    
  return program



def blocksToBlocks16(prog1):
#--------------------------------------------------------------------------
  """
  CONVERSION DEL FORMATO 2  al FORMATO 3: Lista de bloques de
  datos de tamano maximo de 16 palabras
  ENTRADA: Lista en formato 2
  DEVUELVE: Lista de bloques en FORMATO 3
  """
#---------------------------------------------------------------------------
  #-- Programa de salida: lista de bloques de tamano 16 palabras
  prog2 = []
  
  #---- Recorrer todos los bloques y trocearlos en bloques de 16 palabras
  for block in prog1:

    #-- Si el bloque tiene un tamano menor o igual a 16 no hace
    #-- falta trocearlo
    if len(block)<=16:
      prog2.append(block)
    else:
      #-- Bloque tiene tamano mayor a 16. Hay que trocear.
      
      #-- Guardar la direccion de inicio
      addr = block[0]
      del block[0]
      
      #-- Calcular el numero de subbloques de 16 palabras que hay 
      nblock = len(block)/16;
     
      #-- Obtener los subbloques completos
      for i in range(nblock):
        nuevo_bloque = [addr] + block[0:16]
        
        #-- Anadir subbloque
        prog2.append(nuevo_bloque)
        addr+=16;
        del block[0:16]
        
      
      #--- El ultimo bloque esta formados por los "restos"
      if (len(block)!=0):
        nuevo_bloque = [addr] + block
        prog2.append(nuevo_bloque)   

  return prog2



def memToBlocks16(mem): 
#--------------------------------------------------------------------------
  """
  CONVERSION DEL FORMATO 1 (memoria) al FORMATO 3: Lista de bloques de
  datos de tamano maximo de 16 palabras
  ENTRADA: Lista en formato 2
  DEVUELVE: Lista de bloques en FORMATO 3
  """
#---------------------------------------------------------------------------
 
  #-- Primero agrupar en bloques contiguos
  prog1 = memToBlocks(mem)
  
  #-- "trocear" en bloques de 16
  return blocksToBlocks16(prog1)




#------------------------------------------------------------------------------#
#            FUNCIONES DE CONVERSION A CADENAS DE CARACTERES                   #
#------------------------------------------------------------------------------#


def blocksToStrPython(program,name="prog"):
#-------------------------------------------------------------------
  """
  Convertir a una cadena en formato de lista de Python
  Se crea una cadena con syntaxis python con el codigo maquina
  El parametro name es el nombre del programa en el codigo python
  ENTRADAS:
     -blocks:  LIsta de bloques en FORMATO 2 o 3
     -name: Cadena a asignar como nombre a la lista de salida
  DEVUELVE: Una cadena con la lista, en formato PYTHON
  """
#-------------------------------------------------------------------

  #-- Comienzo de la cadena
  prog_str = "%s=[" % (name)
  
  #-- Recorrer todos los bloques y pasarlos a un string
  for block in program:
    cad=["0x%04X" % (palabra) for palabra in block]
    prog_str = prog_str + "[" + ", ".join(cad) + "],"
    
  #-- Final de la cadena  
  prog_str+="]"  
  
  #-- Devolver la cadena
  return prog_str
  
  #-- Esta es la version compacta. Hace lo mismo que todo lo anterior
  #-- return '%s = [%s]' % (name, ', '.join (['[%s]' % (', '.join (["0x%04X" %
  #            palabra for palabra in block])) for block in self.__program]))

 

def memToStrTable(mem):
#--------------------------------------------------------------------------
  """
  Convertir una memoria (formato 1) en una cadena en forma de tabla. 
  Cada una de las filas contiene la direccion y su contenido
  ENTRADAS: mem: Memoria en FORMATO 1
  """
#--------------------------------------------------------------------------

  #-- Volcar la memoria
  tabla=  "Dir: Contenido\n"
  tabla+= "---- ---------\n"
  for addres,palabra in mem:
    tabla+= "%04X: %04X\n" % (addres, palabra[0]*0x100 + palabra[1])
  return tabla
  


def blocksToStr(data):
#----------------------------------------------------------------
  """
  Convertir a una lista en formato 2 o 3 en una cadena
  ENTRADAS: data: datos en FORMATO 2 o 3
  """
#----------------------------------------------------------------
  salida=""
  for block in data:
    salida+= "Direccion: %04X\n" % (block[0])
    cad=["%04X" % (palabra) for palabra in block[1:]]
    salida= salida + " ".join(cad)
    salida+="\n\n"
  return salida

  #-- Esta es la version compacta (sustituye al resto)
  #-- return ''.join (['Direccion: %04X\n%s\n\n' % (block [0], ' '.join  
  #(["%04X" % (palabra) for palabra in block[1:]])) for block in self.__program])
