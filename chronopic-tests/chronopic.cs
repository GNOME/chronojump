/*
 * Copyright (C) 2005  Juan Gonzalez Gomez
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA 02111-1307, USA.
 */
/*------------------------------------------------------------------------
 $Id: chronopic.cs,v 1.1 2005/02/07 11:14:54 obijuan Exp $
 $Revision: 1.1 $
 $Source: /cvsroot/chronojump/chronopic/test/chronopic.cs,v $
--------------------------------------------------------------------------*/
using System;
using System.Runtime.InteropServices;

public class Chronopic {

   //-- Acceso a la libreria chronopic
   const string library = "chronopic";

   //****************************
   //  TIPOS                     
   //****************************
   //-- Respuestas en el acceso a Chronopic
   public enum Respuesta : int
   {
     Ok = 1,
     Error = -1,
     Timeout = 0,
   }
   
   //-- Estado de la plataforma
   public enum Plataforma : int
   {
     ON = 1,
     OFF = 0,
     UNKNOW = -1,
   }

   //******************************
   //  CONSTRUCTORES Y DESTRUCTORES
   //******************************
  
   //-- Constructor
   public Chronopic(string disp)
   {
     this.serial_fd=this.Open(disp);
   }

   //-- Destructor
   ~Chronopic()
   {
     if (this.serial_fd!=-1)
       this.Close();
   }

   

   //***************************************
   //  METODOS PUBLICOS
   //***************************************
   
   //----------------------------
   //-- Cerrar el puerto serie 
   //----------------------------
   public void Close()
   {
     Chronopic.close(this.serial_fd);
     this.serial_fd=-1;
   }
   
   //----------------------------------
   //-- Leer un evento en Chronopic
   //----------------------------------
   public Respuesta Read_event(out double timestamp, 
                               out Plataforma plataforma)
   {
     double t;
     int estado;
     int error;
     Respuesta resp;
     
     //-- Leer trama
     error=read(this.serial_fd,out t, out estado);
     
     //-- Convertir el error al tipo Respuesta
     switch(error) {
       case 0:
         resp = Respuesta.Timeout;
         plataforma = Plataforma.UNKNOW;
         timestamp = 0.0;
         break;
       case 1:
         resp = Respuesta.Ok;
         timestamp = t;
         if (estado==0) 
           plataforma = Plataforma.OFF;
         else
           plataforma = Plataforma.ON;
         break;
       default:
         resp = Respuesta.Error;
         timestamp = 0.0;
         plataforma = Plataforma.UNKNOW;
         break;
     }
     
     return resp;
   }   
   
   //----------------------------------------
   //-- Obtener el estado de la plataforma
   //----------------------------------------
   public Respuesta Read_platform(out Plataforma plataforma)
   {
     int error;
     int estado;
     Respuesta resp;
     
     //-- Enviar trama de estado
     error=Chronopic.estado(this.serial_fd, out estado);
     
     //-- Convertir el error al tipo Respueta
     switch(error) {
       case 0: 
         resp = Respuesta.Timeout;
         plataforma = Plataforma.UNKNOW;
         break;
       case 1:
         resp = Respuesta.Ok;
         if (estado==0)
           plataforma = Plataforma.OFF;
         else
           plataforma = Plataforma.ON;
         break;
       default:
         resp = Respuesta.Error;
         plataforma = Plataforma.UNKNOW;
         break;
     }
     
     //-- Devolver Respuesta
     return resp;
   }
   
   //-- Leer bytes
   public unsafe int Read(byte[] buffer, int bytes, int timeout)
   {
     int error;
     
     fixed(byte *bytepointer = buffer)
     {
       error=read(this.serial_fd,bytepointer,bytes,timeout);
     }
     
     return error;
   }
   
   public void Solicitar_estado()
   {
     solicitar_estado(this.serial_fd);
   }

   //***************************************
   //  METODOS PRIVADOS
   //***************************************
   
   //-- Apertura del puerto serie
   private int Open(string disp)
   {
     return Chronopic.open(disp);
   }
   
   //-------------- Importado del modulo chronopic---------------------
   [DllImport(library,EntryPoint="chronopic_open")] 
   extern static int open(string disp);
   
   [DllImport(library,EntryPoint="chronopic_close")] 
   static extern int close(int fd);
   
   [DllImport(library,EntryPoint="chronopic_get_trama_cambio")] 
   static extern int read(int serial_fd, out double t, out int estado);
   
   [DllImport(library,EntryPoint="chronopic_flush")] 
   static extern int flush(int fd);
   
   [DllImport(library,EntryPoint="chronopic_estado")] 
   static extern int estado(int fd, out int estado);
   
   [DllImport(library,EntryPoint="chronopic_read")]
   static extern unsafe int read(int serial_fd, byte *trama,
                                 int bytes,int timeout);
   [DllImport(library,EntryPoint="chronopic_solicitar_estado")]
   static extern void solicitar_estado(int serial_fd);
                              
   
  //------------------------------
  //   Propiedades privadas
  //------------------------------
  int serial_fd;
}
