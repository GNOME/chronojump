/**********************************************/
/* serial.cs        Juan Gonzalez Gomez.    */
/*--------------------------------------------*/
/* Licencia GPL                               */
/**********************************************/
/*------------------------------------------------------------------------
 $Id$
 $Revision$
 $Source$
--------------------------------------------------------------------------*/

using System.Runtime.InteropServices;

public class Serial {

  //-------------- Importado del modulo serial---------------------
   [DllImport("libserial.so",EntryPoint="sg_serial_open")] 
   public static extern int Open(string disp);
   
   [DllImport("libserial.so",EntryPoint="sg_serial_close")] 
   public static extern int Close(int fd);
   
   [DllImport("libserial.so",EntryPoint="sg_serial_read")] 
   public static extern int Read(int serial_fd,out int trama0, out int trama1,
                                 out int trama2, out int trama3);
   
   [DllImport("libserial.so",EntryPoint="sg_serial_flush")] 
   public static extern int Flush(int fd);
   
   [DllImport("libserial.so",EntryPoint="sg_serial_estado")] 
   public static extern int Estado(int fd, out int estado);
}
