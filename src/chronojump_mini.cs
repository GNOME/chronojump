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
using System.IO; 	//File && TextWriter

using Mono.Unix;

class Test {

	//-- Estado del automata
	enum Automata {
		ON,
		OFF
	}

	/**********************/
	/* PROGRAMA PRINCIPAL */
	/**********************/
	public static void Main(string[] args)
	{
		Chronopic.Plataforma estado_plataforma;
		Automata estado_automata;
		double timestamp;
		double toff;
		double ton;
		bool ok;
		string portName = "";
		string fileName = "";
		TextWriter writer;
		string defaultFileName = "output.txt"; //always output to a file, but if not specified, output here and rewrite it every chronojump_mini execution
	       

		System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("es-ES");
		System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("es-ES");
		
		//connect with catalog.cs for using gettext translation
		Catalog.Init ("chronojump", "./locale");

		//parameters passing only on linux
		if(! Util.IsWindows()) {
			if(args.Length > 2) 
				printSyntaxAndQuit();

			for( int i = 0; i != args.Length; ++i ) {
				Console.WriteLine("param[{0}]: {1}", i, args[i]);
				if(args[i].StartsWith("PORT="))
					portName = args[i].Substring(5);
				else if (args[i].StartsWith("FILE="))
					fileName = args[i].Substring(5);
				else
					printSyntaxAndQuit();
			}
		}
		
		//output file stuff
		fileName = manageFileName(fileName, defaultFileName);
		writer = File.CreateText(fileName);
				

		//detection of ports
		string messageInfo;
		//string messageDetected ="";

		if(Util.IsWindows()) {
			messageInfo = Constants.PortNamesWindows;
			/*
			messageDetected = Catalog.GetString("Detected ports:") + "\n";

			string jumpLine = "";
			foreach (string s in SerialPort.GetPortNames()) {
				messageDetected += jumpLine + s;
				jumpLine = "\n";
			}
			*/
		} else {
			messageInfo = Constants.PortNamesLinux;

			//messageDetected = string.Format(Catalog.GetString("Auto-Detection currently disabled on GNU/Linux"));
		}
			
		messageInfo += string.Format("\n" + Catalog.GetString("More information on Chronojump manual"));

		//messageDetected = string.Format(Catalog.GetString("Auto-Detection currently disabled"));

		Console.WriteLine("---------------------------");
		Console.WriteLine(messageInfo);
		Console.WriteLine("---------------------------");
		//Console.WriteLine(messageDetected);
		Console.WriteLine("---------------------------\n");

		if(portName == "") {
			if( ! Util.IsWindows()) {
				printPortsLinux();
			}
			Console.WriteLine(Catalog.GetString("Print the port name where chronopic is connected:"));
			portName=Console.ReadLine();
		}

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

		double count = 1.0;
						
		Console.WriteLine("\tcount\tTC(ms)\tTF(ms)");
		writer.WriteLine("count;TC(ms);TF(ms)");
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
						Console.WriteLine("{0:f1}",toff);
						writer.WriteLine("{0:f1}",toff);
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
						Console.Write(count + "\t{0:f1}\t",ton);
						writer.Write(count + ";{0:f1};",ton);
					}
					break;
			}
				
			writer.Flush();

			count += .5;
		}

	}

	static void printSyntaxAndQuit() {
		Console.WriteLine(Catalog.GetString("Invalid args. Use:"));

		if(Util.IsWindows()) {
			Console.WriteLine("chronojump_mini.bat [PORT=portName>] [FILE=outputFile]");
			Console.WriteLine(Catalog.GetString("Examples:"));
			Console.WriteLine("chronojump_mini.bat");
			Console.WriteLine("chronojump_mini.bat PORT=COM1");
			Console.WriteLine("chronojump_mini.bat FILE=myFile.csv");
			Console.WriteLine("chronojump_mini.bat PORT=COM1 FILE=myFile.csv");
		} else {
			Console.WriteLine("./chronojump_mini.sh [PORT=portName>] [-FILE=outputFile]");
			Console.WriteLine(Catalog.GetString("Examples:"));
			Console.WriteLine("./chronojump_mini.sh");
			Console.WriteLine("./chronojump_mini.sh PORT=/dev/ttyS0");
			Console.WriteLine("./chronojump_mini.sh FILE=myFile.csv");
			Console.WriteLine("./chronojump_mini.sh PORT=/dev/ttyUSB0 FILE=myFile.csv");
		}
			
		Environment.Exit(1);
	}

	static string manageFileName(string fileName, string defaultFileName) {
		bool fileOk = false;
		do {
			if(fileName == "") 
				fileName = getFileName();

			//user don't want to print to a file
			if(fileName == "")
				fileOk = true;
			else {
				if (File.Exists(fileName)) {
					bool overwrite = askOverwrite(fileName);
					if(overwrite) 
						fileOk = true; //overwrite file, is ok
					else {
						fileOk = false; //no overwrite, ! ok
						fileName = ""; //to be asked for fileName again
					}
				} else
					fileOk = true; //file don't exist, is ok
			}
		} while(! fileOk);

		if(fileName == "") 
			fileName = defaultFileName;

		return fileName;
	}

	static string getFileName() {
		string fileName = "";
		Console.WriteLine(Catalog.GetString("Do you want to output data to a file?") + " [y/n]");
		string option=Console.ReadLine();
		if(option == "Y" || option == "y") {
			Console.WriteLine(Catalog.GetString("If you want to open it with an Spreadsheet like Gnumeric, OpenOffice or MS Office, we recomend to use .csv extension.\neg: 'test.csv'"));
			Console.WriteLine(string.Format(Catalog.GetString("File will be available at directory: {0}"), Path.GetFullPath(".." + Path.DirectorySeparatorChar + "data")));
			Console.WriteLine(Catalog.GetString("Please, write filename:"));
			fileName=Console.ReadLine();
		}
		//if 'n' then "" will be returned

		return fileName;
	}

	static bool askOverwrite(string fileName) {
		Console.WriteLine(string.Format(Catalog.GetString("File {0} exists with attributes {1}, created at {2}"), 
					fileName, File.GetAttributes(fileName), File.GetCreationTime(fileName)));
		Console.WriteLine(string.Format(Catalog.GetString("Are you sure you want to overwrite file: {0}"), fileName) + " [y/n]");
		string option=Console.ReadLine();
		if(option == "Y" || option == "y") 
			return true;
		else 
			return false;
	}

	static void printPortsLinux() {
		string [] usbSerial = Directory.GetFiles("/dev/", "ttyUSB*");
		if(usbSerial.Length > 0) {
			Console.WriteLine(string.Format("\n" + Catalog.GetString("Found {0} USB-serial ports."), usbSerial.Length));
			foreach(string myPort in usbSerial)
				Console.WriteLine(myPort);
		} else {
			Console.WriteLine(Catalog.GetString("Not found any USB-serial ports. Is Chronopic connected?"));
			string [] serial = Directory.GetFiles("/dev/", "ttyS*");
			Console.WriteLine(string.Format(Catalog.GetString("Found {0} Serial ports."), serial.Length));
			foreach(string myPort in serial)
				Console.WriteLine(myPort);
		}
	}

}
