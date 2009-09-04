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
using System;
using System.Collections;
using Gtk;
using GLib;

public class Test
{

  private static Chronopic cp;
  private static Queue Cola;
  private static byte estado_plataforma;
  
  //-- Automata de estado para la recepcion de tramas
  private static int estado = 0;

  public static void Main()
  {
    Application.Init();
    
    //-- Crear el interfaz
    Window w = new Window("Prueba");
    Button b1 = new Button("Test 1");
    Button b2 = new Button("Test 2");
    HBox h= new HBox();
    w.DeleteEvent += new DeleteEventHandler(Window_Delete);
    b1.Clicked += new EventHandler(Button_Clicked);
    b2.Clicked += new EventHandler(Button2_Clicked);
    
    h.PackStart(b1,false,false,0);
    h.PackStart(b2,false,false,0);
    
    w.Add(h);
    w.SetDefaultSize(100,100);
    w.ShowAll();
    
    //-- Inicializar chronopic
    cp = new Chronopic("/dev/ttyUSB0");
    
    //-- Inicializar otras cosas
    Cola = new Queue();
    
    //-- Inicializar temporizador
    GLib.Timeout.Add(100, new GLib.TimeoutHandler(Time_Out));
   
    Application.Run();
  } 

  static bool Time_Out()
  {
    byte[] buffer;
    int status;
    buffer = new byte[20];
    status=cp.Read(buffer,1,100);
    
    //-- Meter elementos nuevos en la cola, si los hay
    if (status>0) {
      for (int i=0; i<status; i++) {
        Cola.Enqueue(buffer[i]);
      }
    }  
    
    //-- Procesar la cola
    if (Cola.Count==0) return true;
    Console.WriteLine("Cola: {0}",Cola.Count);
    
    byte b = (byte)Cola.Dequeue();
    
    switch(estado) {
      case 0: //-- Estado esperando trama
        if (b==(char)'X') {
          //Console.WriteLine("TRAMA CAMBIO");
          
          //-- Leer estado de la plataforma
          if (Cola.Count==0) {
            estado = 1;
            return true;
          }
          
          estado_plataforma = (byte)Cola.Dequeue();
          Console.WriteLine("Cambio0: {0}",estado_plataforma);
          
        }  
        if (b==(char)'E') {
          if (Cola.Count==0) {
            estado=2;
          }
          else {
            b = (byte)Cola.Dequeue();
            Console.WriteLine("ESTADO0: {0}",b);
            estado=0;
          }  
        }  
        break;
        
      case 1:  //-- Procesar trama de Cambio
        estado_plataforma = b;
        Console.WriteLine("Cambio1: {0}",estado_plataforma);
        break;
        
      case 2:  //-- Procesar trama de Estado
        Console.WriteLine("ESTADO2: {0}",b);
        estado=0;
        break;
        
      default:
        break;
    }
   
    
    
    return true;
  }

  static void Window_Delete(object o, DeleteEventArgs args)
  {
    Application.Quit();
    args.RetVal=true;
  }
  
  static void Button_Clicked(object o, EventArgs args)
  {
    byte[] buffer;
    int status;
    buffer = new byte[10];
     status=cp.Read(buffer,5,100000);
    Console.WriteLine("Status: {0}",status);
    if (status>0) {
      for (int i=0; i<status; i++) { 
       Console.WriteLine("Buffer[{0}]: {1}",i,buffer[i]);
      }
    }  
  }
  
  static void Button2_Clicked(object o, EventArgs args)
  {
    cp.Solicitar_estado();
  }
  
}
