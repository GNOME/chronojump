/***********************************************************/
/* test-saltos.mono    Juan Gonzalez Gomez. Febrero 2005   */
/*---------------------------------------------------------*/
/* Medir el tiempo de vuelo de los saltos                  */
/* Licencia GPL                                            */
/***********************************************************/
/*------------------------------------------------------------------------
 $Id$
 $Revision$
 $Source$
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
    Chronopic cp = new Chronopic("/dev/ttyS0");
    
    
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
