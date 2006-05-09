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
using System.IO.Ports;

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
    Automata estado_automata;
    double timestamp;
    double toff;
    double ton;
    bool ok;
   
    //connect with catalog.cs for using gettext translation
    Catalog.Init ("chronojump", "./locale");
    
    
	Console.WriteLine(Catalog.GetString("On GNU/Linux, typical serial ports are"));
	Console.WriteLine("\t/dev/ttyS0\n\t/dev/ttyS1");
	Console.WriteLine(Catalog.GetString("On GNU/Linux, typical USB-serial ports are"));
	Console.WriteLine("\t/dev/ttyUSB0\n\t/dev/ttyUSB1");
	Console.WriteLine(Catalog.GetString("On Windows, typical serial and USB-serial ports are"));
	Console.WriteLine("\tCOM1\n\tCOM2");
	Console.WriteLine(Catalog.GetString("Also COM3 and COM4 can be used on Windows"));
	Console.WriteLine("-----------------------");
	Console.WriteLine(Catalog.GetString("Print the port name where chronopic is connected:"));

	string portName=Console.ReadLine();

	Console.WriteLine(Catalog.GetString("Opening port... if get hanged, generate events with chronopic or the platform"));
    	//-- Crear puerto serie		
	SerialPort sp;
	sp = new SerialPort(portName);
	
    	//-- Abrir puerto serie. Si ocurre algun error
	//-- Se lanzara una excepcion
	try {
		sp.Open();
    	} catch (Exception e){
		Console.WriteLine(Catalog.GetString("Error opening serial port"));
		Console.WriteLine(e);
		Environment.Exit(1);
	}
    
		
    //-- Crear objeto chronopic, para acceder al chronopic
    Chronopic cp = new Chronopic(sp);

    //-- Obtener el estado inicial de la plataforma
    // this do...while is here because currently there's no timeout on chronopic.cs on windows
    do {
	    ok=cp.Read_platform(out estado_plataforma);
    } while(!ok);
    if (!ok) {
      //-- Si hay error terminar
      Console.WriteLine(string.Format(Catalog.GetString("Error: {0}"),cp.Error));
      System.Environment.Exit(-1);
    }
    Console.WriteLine(string.Format(Catalog.GetString("Platform state: {0}"), estado_plataforma));
    

    //-- Establecer el estado inicial del automata
    if (estado_plataforma==Chronopic.Plataforma.ON) 
      estado_automata=Automata.ON;
    else {
      Console.WriteLine(Catalog.GetString("Go up platform for jumping"));
      
      //-- Esperar a que llegue una trama con el estado de la plataforma
      //-- igual a ON. Esto indica que el usuario se ha subido
      do {
        ok = cp.Read_event(out timestamp, out estado_plataforma);
      } while (!ok);
      
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
        ok = cp.Read_event(out timestamp, out estado_plataforma);
      } while (ok==false);
      
      
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
