#!/usr/bin/env python
# -*- coding: iso-8859-15 -*-

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
  Modulo Cliente para comunicarse con el PIC_BOOTLOADER y descargar       
  ficheros .hex en la tarejta Skypic

  Ademas se incorporan metodos para que el usuario pueda acceder 
  a los diferentes servicios del Bootloader por si quiere implementarse
  su propio programa de descarga o quiere hacer modificaciones a bajo 
  nivel en el bootloader
"""

import serial 
import time
import IntelHex
import sys


#############
# CONSTANTS #
#############

#--------------------------------------
#- Configuracion del puerto serie 
#--------------------------------------
#-- Timeout por defecto para el acceso al puerto serie
SERIAL_TIMEOUT = 0.2

#--- Velocidad de transmision para la comunicacion con el PIC Bootloader
BAUDIOS = 38400

#------------------------------------------------------------------
#-  IDENTIFICACION DE LOS COMANDOS DEL PROTOCOLO DEL BOOTLOADER  
#------------------------------------------------------------------
CMD_WRITE    = '\xE3'  #-- Escritura de un bloque
CMD_DATA_OK  = '\xE7'  #-- Datos enviados correctamente
CMD_OK       = '\xE4'  #-- Operacion ejecutada
CMD_IDENT    = '\xEA'  #-- Comando de identificacion del bootloader
CMD_IDACK    = '\xEB'  #-- Bootloader identificado
CMD_SEND_DONE= '\xED'  #-- Comando de ejecucion

#------------------------------------------------------------------------
#-- Constantes usadas con el metodo download para indicar
#-- lo que va ocurriendo con la descarga 
#------------------------------------------------------------------------
WRITING_START=1      #-- Comienzo de la escritura
WRITING_INC=2        #-- Escritura de una palabra
WRITING_END=3        #-- Fin de la escritura

IDENT_START=4        #-- Comienzo de la identificacion del bootloader
IDENT_NACK=5         #-- No se ha recibido respuesta

#-- Timeout por defecto, en segundos, que se espera a detectar el 
#-- Bootloader
DEFAULT_TIMEOUT = 10


#----------------------------------------
#- Clase para la gestion de los errores
#----------------------------------------
class IrisError (Exception):
  """
  Excepciones producidas en el modulo Pic16_Bootloader
  """
  pass


def default_logCallback(msg):
  """
    Funcion de "log" por defecto. 
    Simplemente se imprimen los mensajes
  """
  sys.stdout.write(msg)
  sys.stdout.flush()


#-----------------------------------------------------------------------------
def default_stateCallback(op,inc,total):
  """
  Funcion de estado por defecto
  Se imprime informacion en la consola
  La funcion debe devolver TRUE si todo esta OK y se quiere continuar
  con el proceso. FALSE en caso contrario. La descarga se aborta
 Los parametros recibidos son:
   -op: Tipo de operacion. Indica la fase de la descarga que se ha iniciado
   -inc: 
      -En el estado IDENT_NACK indica el numero reitentos hasta el momento
      -En el estado WRITING_INC indica el numero de bloques enviados
   -total:
      -En el estado IDENT_NACK indica el tiempo total transcurrido desde
       el comienzo de la identificacion (en segundos)
      -En el estado WRITING_INC indica el numero total de bloques del
       del programa a transmitir

  El usuario puede crear su propia funcion de estado para actualizar el
  interfaz de su aplicacion como quiera. Esta funcion es un ejemplo para
  una interfaz de consola
  """


  #--------------------------------------------------
  #- Comienzo de la identificacion del Bootloader 
  #--------------------------------------------------
  if op==IDENT_START:
    print "Esperando Bootloader"
    return True
    
  #-------------------------------------------------------------------------
  #-- Timeout en la identificacion. 
  #-- Cuando el tiempo transcurrido supera el timeout en la identificacion
  #-- se aborta devolviendose False
  #-------------------------------------------------------------------------  
  elif op==IDENT_NACK:
    sys.stdout.write('.')
    sys.stdout.flush()
    if total<=DEFAULT_TIMEOUT:
      return True
    else :
      return False  
  
  #-----------------------------------------------------------------------
  #-- Comienzo de la descarga
  #-- Se imprime una barra de status en ASCII formada por '.' y se lleva
  #-- el cursor a la izquierda (imprimiendo el caracter '\b'
  #----------------------------------------------------------------------
  elif op==WRITING_START:
    sys.stdout.write("\nDescargando:\n")
    cad="".join(["." for i in range(total)])
    back="".join(["\b" for i in range(total)])
    
    #-- Imprimir la "barra de estado" con '.'. Un '.' por cada bloque
    sys.stdout.write(cad)
    
    #-- Llevar el cursos a la izquierda
    sys.stdout.write(back)
    sys.stdout.flush()
    
    return True
  
  #----------------------------------------------------------------------
  #-- Se ha grabado un bloque. Se actualiza la "barra de estado ascii"  
  #----------------------------------------------------------------------
  elif op==WRITING_INC:  
    sys.stdout.write("*")
    sys.stdout.flush()
    return True
    
  #----------------------------------------
  #- Fin de la descarga
  #----------------------------------------
  elif op==WRITING_END: 
    print " OK"
    return True
    

#----------------------------------------------------------------------------
#--                           CLASE  PRINCIPAL    
#----------------------------------------------------------------------------
class Iris:
  """
  Clase prinipal del modulo Pic16_Bootloader. Se utiliza para comunicarse
  con el Bootloader y descargar programas en la Skypic
  """

  #---------------------
  #- Destructor 
  #---------------------
  def __del__(self):
  
    #-- Cerrar el pueto serie
    if self.__serial:
      #print "Debug: cerrando puerto serie: %s" % (self.__serial.portstr)
      self.__serial.close()


  def __init__ (self, serialName, logCallback = default_logCallback):
  #-------------------------------------------------------------------------
    """
  Constructor
  ENTRADAS:
    serialName: Dispositivo serie
    logCallback: Funcion de retrollamada para el "log"    
    """
  #--------------------------------------------------------------------------  
    
    self.__serial = None
    self.__log = logCallback
    
    #-- Abrir puerto serie
    try:
      self.__serial = serial.Serial(serialName, BAUDIOS)
    except serial.SerialException:
      raise IrisError,'Error al abrir puerto serie %s.' % serialName

    if self.__log:
      self.__log ('Serial port %s opened.\n' % self.__serial.portstr)

    #-- Configurar timeout
    #-- He detectado que en Linux al configurar el timeout se modifica
    #-- el estado del DTR. No en todos los casos (depende del driver
    #-- del conversor USB-serie usado). El problema siempre esta en que
    #-- los valores del DTR no estan estandarizados y cada driver los 
    #-- maneja a su propia manera.
    #-- La solucion que se esta utilizando es la de configurar el 
    #-- timeout al principio
    self.__serial.timeout = SERIAL_TIMEOUT
  
    #-- Vaciar los buffers del puerto serie
    self.__serial.flushInput()
    self.__serial.flushOutput()

  
  def close(self):
  #-----------------------------
    """
    Cerrar el puerto serie
    """
  #-----------------------------
    if self.__serial!=None:
      self.__serial.close()
      
 
  
  def sendDone (self):
  #--------------------------------------------------------------------
    """
    Enviar comando SENDDONE para que arranque el programa cargado
    """
  #--------------------------------------------------------------------  
  
    #-- Enviar el comando
    self.__serial.write (CMD_SEND_DONE)
    
    #-- Esperar la respuesta
    ch = self.__serial.read (1)
    if ch != CMD_OK:
      raise IrisError, "Error en Done"
      
    if self.__log:
      self.__log ('Ejecutando programa\n')  
      

  def skypicReset (self):
  #----------------------------------------------------------------
    """
    Hacer reset de la Skypic. Solo funcionara si el jumper JP4
    esta colocado en la posicion DTR
    """
  #----------------------------------------------------------------
  
    #-- Desactivar la senal DTR durante 0.5 segundos
    self.__serial.setDTR (0)
    
    #-- Esto es para depurar
    #if self.__log:
    #  self.__log("%s: DTR OFF\n" % self.__serial.portstr)
    time.sleep (0.5)
    
    #-- Volver a activarla. Reset hecho
    self.__serial.setDTR (1)
    
    #-- Esto es para depurar
    #if self.__log:
    #  self.__log("%s: DTR ON\n" % self.__serial.portstr)
    
    if self.__log:
      self.__log ('Reset Skypic\n')
    

  
  def identBootloader (self, timeoutCallback=default_stateCallback):
  #-----------------------------------------------------------------------
    """
   Identificar el Bootloader
   Devuelve: 
      -TRUE si se ha detectado
      -FALSE si ha transcurrido el timeout y no se ha detectado
       Esto puede ocurrir bien porque no haya comunicacion con el bootloader
       o bien porque no se haya pulsado el boton de reset de la skypic
   ENTRADAS:
      -timeoutCallback : Funcion de estado. Se invoca al comienzo de la 
         identificacion y si no se ha podio encontrar el Bootloader
    """
  #-----------------------------------------------------------------------
     
    #-- Inicializacion de la funcion de callback
    if timeoutCallback:
      timeoutCallback(IDENT_START,0,0)    

    # Timeout or bad reply
    nack=0;
    while True:

      #-- Enviar comando de identificacion
      self.__serial.write (CMD_IDENT)

      #-- Esperar la respuesta
      id = self.__serial.read (1)

      #-- Condicion de deteccion del bootloader
      if len (id) == 1 and id == CMD_IDACK:
        if self.__log:
          self.__log ('Bootloader OK\n')
        return True
          
      nack+=1    
      #-- Invocar la funcion de callback
      if timeoutCallback:
        ret = timeoutCallback(IDENT_NACK,nack,float(nack)*SERIAL_TIMEOUT) 
        if ret==False:
          #-- Bootloder NO detectado
          if self.__log:
            self.__log ('TIMEOUT\n')
          
          raise IrisError,'Bootloader No detectado'
    

  def writeData(self,block):
  #--------------------------------------------------
    """
    Escribir un bloque a traves del booloader
    El primer elemento del bloque es la direccion
    """
  #--------------------------------------------------
    
    #-- Obtener la direccion de comienzo del bloque
    addr=block[0]
    
    #-- Obtener el bloque en bytes
    #-- Se almacena en data. Primero el byte alto y luego el bajo
    data=[]
    for i in block[1:]:
      data.append(i>>8 & 0xFF)  #-- Andir byte alto
      data.append(i&0xFF)       #-- Anadir byte bajo
      
    #-- Calcular el Checksum  
    chk = sum(data) & 0xFF
      
    #-- Tamano del bloque en bytes
    tam = len (data)

    #------------------------------------
    #-- Comenzar la escritura del bloque 
    #------------------------------------
    #-- Enviar comando
    self.__serial.write (CMD_WRITE)
    
    #-- Enviar direccion de comienzo del bloque
    self.__serial.write ('%c%c' % (chr (addr >> 8 & 0xFF),
                                   chr (addr & 0xFF)))
                                   
    #-- Enviar tamano
    self.__serial.write (chr (tam))
    
    #-- Enviar checksum
    self.__serial.write (chr (chk))
    
    #-- Enviar los datos
    for d in data:
      self.__serial.write (chr(d))
    
    #-----------------------------
    #-- Comprobar las respuestas 
    #-----------------------------
    
    # --- Datos correctos?
    ch = self.__serial.read (1)
    if ch != CMD_DATA_OK:
      raise IrisError, 'Data error.'

    # --- Escritura ok?
    ch = self.__serial.read (1)
    if ch != CMD_OK:
      raise IrisError, 'Write error.'
    
  
  def download (self, program, stateCallback=default_stateCallback):
  #---------------------------------------------------------------------------
    """
    Descargar un programa a traves del bootloader
    Para cada fase de la descarga se invoca la funcion de retrollamda
    stateCallback
    """
  #---------------------------------------------------------------------------
  
    #-- Hacer un reset
    self.skypicReset()
    
    #-- Identificar el bootloader
    #-- Se invoca a la funcion de estado
    self.identBootloader(timeoutCallback=stateCallback)
    
    #-- Obtener Tamano del programa en bloques
    tam = len(program)
    
    #-- Invocar la funcion de estado para indicar el comienzo
    #-- de la descarga
    if stateCallback:
      ok=stateCallback(WRITING_START, 0, tam);
      if not ok:
        raise IrisError, "Abortado"
      
    #-- Escribir los bloques
    count=1;
    for block in program:
      self.writeData(block)
      
      #-- Invocar funcion de estado para indicar que se ha descargado
      #-- un bloque
      if stateCallback:
        ok=stateCallback(WRITING_INC,count,tam); 
        if not ok:
          raise IrisError, "Abortado"        
      
      #-- Incrementar contador de numero de bloques descargados
      count=count + 1
      
    #-- Invocar la funcion de estado para indicar que se ha terminado
    #-- la descarga, siempre que no haya sido abortada    
    if stateCallback:
      ok=stateCallback(WRITING_END,0,tam);  
      if not ok:
          raise IrisError, "Abortado" 
      
    #-- Ejecutar el programa  
    self.sendDone ()
