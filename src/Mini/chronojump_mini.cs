/*
 * Copyright (C) 2005  Juan Gonzalez Gomez
 * Copyright (C) 2014-2017  Xavier de Blas <xaviblas@gmail.com>
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

/***********************************************************/
/* chronojump-mini    					   */
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

class ChronoJumpMini {

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
		
		//always output to a file, but if not specified, output here and rewrite it every chronojump_mini execution
		string defaultFileName = Path.Combine(getOutputDir(), "output"); 
	       

		System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("es-ES");
		System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("es-ES");
		
		//connect with catalog.cs for using gettext translation
		Catalog.Init ("chronojump", "./locale");

		//parameters passing only on linux
		if(! UtilAll.IsWindows()) {
			if(args.Length > 2) 
				printSyntaxAndQuit();

			for( int i = 0; i != args.Length; ++i ) {
				Console.WriteLine("param[{0}]: {1}", i, args[i]);
				if(args[i].StartsWith("PORT="))
					portName = args[i].Substring(5);
				else if (args[i].StartsWith("FILE=")) {
					fileName = args[i].Substring(5);
					fileName= getOutputDir() + Path.DirectorySeparatorChar + fileName;
				}
				else
					printSyntaxAndQuit();
			}
		}
		
		//detection of ports
		string messageInfo;
		//string messageDetected ="";

		if(UtilAll.IsWindows()) {
			messageInfo = Constants.PortNamesWindowsStr();
			/*
			messageDetected = Catalog.GetString("Detected ports:") + "\n";

			string jumpLine = "";
			foreach (string s in SerialPort.GetPortNames()) {
				messageDetected += jumpLine + s;
				jumpLine = "\n";
			}
			*/
		} else {
			messageInfo = Constants.PortNamesLinuxStr();
		}
			
		messageInfo += string.Format("\n" + Catalog.GetString("More information on Chronojump manual"));

		Console.WriteLine("---------------------------");
		Console.WriteLine(messageInfo);
		Console.WriteLine("---------------------------");

		if(portName == "") {
			if( ! UtilAll.IsWindows()) {
				Console.WriteLine(UtilAll.DetectPortsLinux(false)); //formatting
			}
			Console.WriteLine(Catalog.GetString("Print the port name where chronopic is connected:"));
			portName=Console.ReadLine();
		}

		//output file stuff
		fileName = manageFileName(fileName);
		if(fileName == "") 
			fileName = defaultFileName + "-" + portName.Replace("/","") + ".csv";
		
		writer = File.CreateText(fileName);
		

		Console.WriteLine(Catalog.GetString("Opening port …") + " " +
			       Catalog.GetString("Please touch the platform or click Chronopic TEST button"));
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
		cp.Flush();


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


		//Console.WriteLine("Automatic variables: ");
	        //cp.Read_variables_automatic();


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
						
		Console.WriteLine("  TC(ms) TF(ms)");
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
						Console.Write(count + " {0:f1} ",ton);
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

		if(UtilAll.IsWindows()) {
			Console.WriteLine("chronojump_mini.bat [PORT=portName>] [FILE=outputFile]");
			Console.WriteLine(Catalog.GetString("Examples:"));
			Console.WriteLine("chronojump_mini.bat");
			Console.WriteLine("chronojump_mini.bat PORT=COM1");
			Console.WriteLine("chronojump_mini.bat FILE=myFile.csv");
			Console.WriteLine("chronojump_mini.bat PORT=COM1 FILE=myFile.csv");
		} else {
			Console.WriteLine("./chronojump_mini.sh [PORT=portName>] [FILE=outputFile]");
			Console.WriteLine(Catalog.GetString("Examples:"));
			Console.WriteLine("./chronojump_mini.sh");
			Console.WriteLine("./chronojump_mini.sh PORT=/dev/ttyS0");
			Console.WriteLine("./chronojump_mini.sh FILE=myFile.csv");
			Console.WriteLine("./chronojump_mini.sh PORT=/dev/ttyUSB0 FILE=myFile.csv");
		}
			
		Environment.Exit(1);
	}

	static string manageFileName(string fileName) {
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

		return fileName;
	}

	static string getOutputDir()
	{
		// Chronojump (networks at admin) will be able to change the LocalDataDir, so do not use this
		//return UtilAll.GetApplicationDataDir();
		//use this, it is referred to the original unchanged dir (ok for Mini)
		return Path.Combine (
				Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
				"Chronojump");
	}

	static string getFileName() {
		string fileName = "";
		Console.WriteLine(Catalog.GetString("Do you want to output data to a file?") + " [y/n]");
		string option=Console.ReadLine();

		if(option == "Y" || option == "y") {
			Console.WriteLine(Catalog.GetString("If you want to open it with an Spreadsheet like Gnumeric, OpenOffice or MS Office, we recommend to use .csv extension.\neg: 'test.csv'"));
			Console.WriteLine(string.Format(Catalog.GetString("File will be available at directory: {0}"), getOutputDir()));
			Console.WriteLine(Catalog.GetString("Please, write filename:"));
			fileName=Console.ReadLine();
			fileName= getOutputDir() + Path.DirectorySeparatorChar + fileName;
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

}
