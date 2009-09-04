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
/*---------------------------------------------------------*/
/* Medir el tiempo de vuelo de los saltos                  */
/* Licencia GPL                                            */
/***********************************************************/
/*------------------------------------------------------------------------
 $Id: test-saltos.mono.cs,v 1.1 2005/02/07 11:14:54 obijuan Exp $
 $Revision: 1.1 $
 $Source: /cvsroot/chronojump/chronopic/test/test-saltos.mono.cs,v $
--------------------------------------------------------------------------*/

using System;

class Test {
  
  //-- Estado del automata
  enum Automata {
    ON,
    OFF
  }
  
  /**********************/
  /* PROGRAMA PRINCIPAL */
  /**********************/
  public static void Main()
  {
    Chronopic.Plataforma estado_plataforma;
    Chronopic.Respuesta respuesta;
    Automata estado_automata;
    double timestamp;
    double toff;
    
    //-- Crear objeto chronopic, para acceder al chronopic
    Chronopic cp = new Chronopic("/dev/ttyUSB0");
    
    //-- Obtener el estado inicial de la plataforma
    respuesta=cp.Read_platform(out estado_plataforma);
    switch(respuesta) {
      case Chronopic.Respuesta.Error:
        Console.WriteLine("Error en comunicacion con Chronopic");
        return;
      case Chronopic.Respuesta.Timeout:
        Console.WriteLine("Chronopic no responde");
        return;
      default:
        break;
    }
    
    Console.WriteLine("Estado plataforma: {0}",estado_plataforma);
    
    //-- Establecer el estado inicial del automata
    if (estado_plataforma==Chronopic.Plataforma.ON) 
      estado_automata=Automata.ON;
    else {
      Console.WriteLine("Suba a la plataforma para realizar el salto");
      
      //-- Esperar a que llegue una trama con el estado de la plataforma
      //-- igual a ON. Esto indica que el usuario se ha subido
      do {
      respuesta = cp.Read_event(out timestamp, out estado_plataforma);
      } while (respuesta!=Chronopic.Respuesta.Ok);
      
      //-- Se han subido a la plataforma
      estado_automata = Automata.ON;
    }
    
    Console.WriteLine("");
    Console.WriteLine("Puede saltar cuando quiera");
    Console.WriteLine("Pulse control-c para finalizar la sesion");
    Console.WriteLine("-----------------------------------------");
    
    while(true) {
    
      //-- Esperar a que llegue una trama
      do {
        respuesta = cp.Read_event(out timestamp, out estado_plataforma);
      } while (respuesta!=Chronopic.Respuesta.Ok);
      
      
      //-- Segun el estado del automata
      switch(estado_automata) {
      
        case Automata.OFF: //-- Usuario estaba en el aire
        
          //-- Si ha aterrizado
          if (estado_plataforma==Chronopic.Plataforma.ON) {
          
            //-- Pasar al estado ON
            estado_automata=Automata.ON;
            
            //-- Registrar tiempo de vuelo
            toff = timestamp;
            
            //-- Imprimir informacion
            Console.WriteLine("Tiempo: {0:f1} ms",toff);
          }
          break;
          
        case Automata.ON: //-- Usuario estaba en la plataforma
        
          //-- Si ahora esta en el aire...
          if (estado_plataforma==Chronopic.Plataforma.OFF) {
            
            //-- Pasar al estado OFF
            estado_automata=Automata.OFF;
          }
          break;
      }
      
    }
    
  }

}
