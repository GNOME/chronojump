/**********************************************/
/* chronopic.cs        Juan Gonzalez Gomez.   */
/*--------------------------------------------*/
/* Licencia GPL                               */
/**********************************************/
/*------------------------------------------------------------------------
 $Id$
 $Revision$
 $Source$
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
	Console.WriteLine("1");
	 try {  
	     this.serial_fd=this.Open(disp);
	 } catch{
		 Console.WriteLine("catched!!");
	 }
	
     Console.WriteLine("2");
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
