/**********************************************/
/* test-platform.cs    Juan Gonzalez Gomez.   */
/*--------------------------------------------*/
/* Licencia GPL                               */
/**********************************************/
/*------------------------------------------------------------------------
 $Id$
 $Revision$
 $Source$
--------------------------------------------------------------------------*/

using System;

class Test {
  
  /**********************/
  /* PROGRAMA PRINCIPAL */
  /**********************/
  public static void Main()
  {
    int serial_fd=0;
    int ok=0;
    //int t0,t1,t2,t3;
    int estado;
    string trama="0000000";
    
    //-- Abrir el puerto serie
    serial_fd=Serial.Open("/dev/ttyS0");
   
    while(true) {
      //ok=Serial.Read(serial_fd, out t0, out t1, out t2, out t3);
      ok=Serial.Estado(serial_fd, out estado);
      if (ok==1) 
        Console.WriteLine("Estado: {0}", estado);
     }
     
    //-- Cerrar puerto serie
    Serial.Close(serial_fd);
  }

}
