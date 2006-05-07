/***********************************************************/
/* chronojump-mini    Juan Gonzalez Gomez. Febrero 2005    */
/*---------------------------------------------------------*/
/* Mide tiempo de vuelo y contacto de los saltos           */
/* Muestra los resultados en consola                       */
/* Licencia GPL                                            */
/***********************************************************/
/*------------------------------------------------------------------------
 $Id: 
 $Revision: 
 $Source: 
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
    double ton;
   
    //connect with catalog.cs for using gettext translation
    Catalog.Init ("chronojump", "./locale");
    
    //-- Crear objeto chronopic, para acceder al chronopic
//    Chronopic cp = new Chronopic("/dev/ttyS0");
    //Chronopic cp = new Chronopic("/dev/ttyS1");
    //Chronopic cp = new Chronopic("/dev/ttyUSB0");
    //Chronopic cp = new Chronopic("/dev/ttyS17");
    //Chronopic cp = new Chronopic("/dev/ttyS1");
    
SerialPort sp;
	Chronopic cp;
			sp = new SerialPort("/dev/ttyS0");
			sp.Open();
    
			cp = new Chronopic(sp);

			
    //-- Obtener el estado inicial de la plataforma
    /*
    respuesta=cp.Read_platform(out estado_plataforma);
    switch(respuesta) {
      case Chronopic.Respuesta.Error:
        Console.WriteLine(Catalog.GetString("Error comunicating with Chronopic"));
        return;
      case Chronopic.Respuesta.Timeout:
        Console.WriteLine(Catalog.GetString("Chronopic is offline"));
        return;
      default:
        break;
    }
    */
    
    Console.WriteLine(Catalog.GetString("Platform state: {0}"), estado_plataforma);
    
    //-- Establecer el estado inicial del automata
    if (estado_plataforma==Chronopic.Plataforma.ON) 
      estado_automata=Automata.ON;
    else {
      Console.WriteLine(Catalog.GetString("Go up platform for jumping"));
      
      //-- Esperar a que llegue una trama con el estado de la plataforma
      //-- igual a ON. Esto indica que el usuario se ha subido
      do {
      respuesta = cp.Read_event(out timestamp, out estado_plataforma);
      } while (respuesta!=Chronopic.Respuesta.Ok);
      
      //-- Se han subido a la plataforma
      estado_automata = Automata.ON;
    }
    
    Console.WriteLine("");
    Console.WriteLine(Catalog.GetString("Jump when prepared"));
    Console.WriteLine(Catalog.GetString("Press CTRL-c for ending session"));
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
            Console.WriteLine("TV: {0:f1} ms",toff);
          }
          break;
          
        case Automata.ON: //-- Usuario estaba en la plataforma
        
          //-- Si ahora esta en el aire...
          if (estado_plataforma==Chronopic.Plataforma.OFF) {
            
            //-- Pasar al estado OFF
            estado_automata=Automata.OFF;
            
	    //-- Registrar tiempo de contacto
            ton = timestamp;
            
            //-- Imprimir informacion
            Console.WriteLine("TC: {0:f1} ms",ton);
          }
          break;
      }
      
    }
    
  }

}
