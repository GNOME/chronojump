/*
 * Copyright (C) 2005  Juan Gonzalez Gomez
 * Copyright (C) 2014-2022  Xavier de Blas <xaviblas@gmail.com>
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
using System.IO.Ports;
using System.Threading;
//using System.Runtime.InteropServices;

using System.Diagnostics; 	//for detect OS
using System.IO; 		//for detect OS
using Mono.Unix;

public class Chronopic {

	//****************************
	//  TIPOS PUBLICOS
	//****************************
	public enum ErrorType
	{
		Ok = 0,        //-- OK. No hay error
		Timeout = 1,   //-- Error por Timeout
		Invalid = 2,   //-- Error por recibir caracter invalido
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

	//-- Fake Constructor
	//only used when there's a problem in detection and must return a Chronopic instance
	//see gui/chronopic.cs chronopicInit
	public Chronopic() {
	}

	//-- Constructor
	public Chronopic(SerialPort sp)
	{
		//-- Comprobar si puerto serie ya estaba abierto
		if (sp != null)
			if (sp.IsOpen)
				sp.Close();

		//-- Abrir puerto serie
		sp.Open();

		//-- Configurar timeout por defecto
		//de momento no en windows (hasta que no encontremos por qué falla)
		//OperatingSystem os = Environment.OSVersion;
		//not used, now there's no .NET this was .NET related
		//on mono timeouts work on windows and linux
		//if( ! os.Platform.ToString().ToUpper().StartsWith("WIN"))
			sp.ReadTimeout = DefaultTimeout;

		//-- Guardar el puerto serie
		this.sp = sp;

//		//-- Vaciar buffer
//		//done in a separate method
//		this.flush();
	}

	//-- Destructor
	~Chronopic()
	{
		//-- Cerrar puerto serie
		if (sp != null)
			if (sp.IsOpen)
				sp.Close();
	}

	//***************************************
	//  METODOS PUBLICOS
	//***************************************

	//--------------------------------------------------
	//-- Leer un evento en Chronopic
	//-- Devuelve:
	//--   * timestamp : Marca de tiempo
	//--   * plataforma: Nuevo estado de la plataforma
	//--------------------------------------------------
	public bool Read_event(out double timestamp, 
			out Plataforma plataforma)
	{
		double t;

		//-- Trama de Evento
		byte[] trama = new byte[5];
		bool ok;

		//-- Esperar a que llegue la trama o que se
		//-- produzca un timeout
		ok = Read_cambio(trama);

		LogB.Warning("after Read_cambio =");
		LogB.Warning(ok.ToString());

		//-- Si hay timeout o errores
		if (ok==false) {
			plataforma = Plataforma.UNKNOW;
			timestamp = 0.0;



			return false;
		}

		//-- Comprobar que el estado transmitido en la trama es correcto
		//-- El estado de la plataforma solo puede tener los valores 0,1
		if (trama[1]!=0 && trama[1]!=1) {
			//-- Trama erronea
			plataforma = Plataforma.UNKNOW;
			timestamp = 0.0;
			return false;
		}

		//-- Actualizar el estado
		if (trama[1]==0)
			plataforma = Plataforma.OFF;
		else 
			plataforma = Plataforma.ON;

		//-- Obtener el tiempo
		t = (double)((trama[2]*65536 + trama[3]*256 + trama[4])*8)/1000;

		timestamp = t;

		return true;
	}   
	
	public void Read_variables_automatic()
	{
		LogB.Information("ReadVarAutoStart");

		if (sp == null)
			sp.Open(); 
		
		LogB.Information("ReadVarAutoOpened");

		try {
			sp.Write("J");
			LogB.Information("Port scanning (should return 'J'): " + (char) sp.ReadByte());
		} catch {
			this.error=ErrorType.Timeout;
			LogB.Information("Timeout. This is not Chronopic-Automatic-Firmware");
			return;
		}

		
		sp.Write("n");
		LogB.Information("Version: " + 
				(char) sp.ReadByte() +
				(char) sp.ReadByte() +
				(char) sp.ReadByte() 
			       	);

		int debounce = 0;

		sp.Write("a");
		debounce = ( sp.ReadByte() - '0' ) * 10;
		LogB.Information("Current debounce time:", debounce.ToString());

		LogB.Information("Changing to 10 ms ... ");
		sp.Write("b\x01");

		sp.Write("a");
		debounce = ( sp.ReadByte() - '0' ) * 10;
		LogB.Information("Current debounce time:", debounce.ToString());

		LogB.Information("Changing to 50 ms ... ");
		sp.Write("b\x05");

		sp.Write("a");
		debounce = ( sp.ReadByte() - '0' ) * 10;
		LogB.Information("Current debounce time: ", debounce.ToString());
	}



	//----------------------------------------
	//-- Obtener el estado de la plataforma
	//----------------------------------------
	public bool Read_platform(out Plataforma plataforma)
	{
		//-- Crear la trama
		byte[] trama = {(byte)Trama.Estado};
		byte[] respuesta = new byte[2];
		int n;
		int count;
		bool status;
			
		if (sp != null)
			if (sp.IsOpen) 
				sp.Close();
		
		try {
			sp.Open();
		} catch {
			status=false;
			plataforma = Plataforma.UNKNOW;
			this.error=ErrorType.Timeout;
			return status;
		}

		//-- Enviar la trama por el puerto serie
		sp.Write(trama,0,1);

		//-- Esperar a que llegue la respuesta
		//-- Se espera hasta que en el buffer se tengan el numero de bytes
		//-- esperados para la trama. (para esta trama 2). Si hay un 
		//-- timeout se aborta
		count=0;
		do {
			n = sp.Read(respuesta,count,2-count);
			count+=n;
		} while (count<2 && n!=-1);

		//-- Comprobar la respuesta recibida
		switch(count) {
			case 2 : //-- Datos listos
				if (respuesta[0]==(byte)Trama.REstado) {
					switch (respuesta[1]) {
						case 0: 
							plataforma = Plataforma.OFF;
							this.error=ErrorType.Ok;
							status=true;
							break;      
						case 1: 
							plataforma = Plataforma.ON;
							this.error=ErrorType.Ok;
							status=true;
							break;      
						default:
							plataforma = Plataforma.UNKNOW;
							this.error=ErrorType.Invalid;
							status=false;
							break;
					}
				}
				else {  //-- Recibida respuesta invalida
					plataforma = Plataforma.UNKNOW;
					this.error=ErrorType.Invalid;
					status=false;

					//-- Esperar un tiempo y vaciar buffer
					Thread.Sleep(ErrorTimeout);
					this.flush();
				}
				break;
			default : //-- Timeout (u otro error desconocido)
				status=false;
				plataforma = Plataforma.UNKNOW;
				this.error=ErrorType.Timeout;
				break;
		}

		return status;
	}

	/****************************/
	/* PROPIEDADES              */
	/****************************/
	public ErrorType Error 
	{
		get {
			return this.error;
		}
	}

	//***************************************
	//  METODOS PRIVADOS
	//***************************************
	//-- Esperar a recibir una trama de cambio de estado
	private bool Read_cambio(byte[] respuesta)
	{
		//-- Crear la trama
		int n=0;
		int count;
		bool status;

		//-- Esperar a que llegue la respuesta
		//-- Se espera hasta que en el buffer se tengan el numero de bytes
		//-- esperados para la trama. (En el caso de id son 4). Si hay un 
		//-- timeout se aborta
		count=0;
		do {
			//try, catch done because mono-1.2.3 throws an exception when there's a timeout
			//http://bugzilla.gnome.org/show_bug.cgi?id=420520
			bool success = false;
			do {
				try {
					n = sp.Read(respuesta,count,5-count);
					LogB.Warning("respuesta = ");
					LogB.Warning(respuesta[count].ToString());
					count+=n;
					success = true;
				} catch {
					//LogB.Warning("catched at Read_cambio");
					//if cancel is clicked, cancellingTest will be true. Stop reading
					//same for finish and finishingTest
					if(cancellingTest || finishingTest)
					{
						//-- Wait a bit and empty buffer
						Thread.Sleep(ErrorTimeout);
						this.flushByTimeOut();
						return false;
					}
				}
			} while (!success);
		} while (count<5 && n!=-1);

		//-- Comprobar la respuesta recibida
		switch(count) {
			case 5 : //-- Datos listos
				if (respuesta[0]==(byte)Trama.Evento) {  //-- Trama de evento
					this.error=ErrorType.Ok;
					status=true;
				}
				else {  //-- Recibida trama invalida
					this.error=ErrorType.Invalid;
					status=false;

					//-- Esperar un tiempo y vaciar buffer
					Thread.Sleep(ErrorTimeout);
					this.flush();
				}
				break;
			default : //-- Timeout (u otro error desconocido)
				status=false;
				this.error=ErrorType.Timeout;
				break;
		}

		return status;
	}

	public bool AbortFlush;	

	//Used by two threads
	private static bool cancellingTest;
	public static void CancelDo()
	{
		cancellingTest = true;
	}
	private static bool finishingTest;
	public static void FinishDo()
	{
		finishingTest = true;
	}

	/*
	   on <= 1.6.2 we could have this problem with static variables:
	   on thread 1 finishingTest is marked as true
	   just at the moment on thread 2 read_event is called and read_cambio is called and finishingTest = false
	   so thread 2 does not end
	   solution is to not define finishingTest on read_cambio
	   define it on the beginning and will affect both chronopics
	   */
	public static void InitCancelAndFinish()
	{
		cancellingTest = false;
		finishingTest = false;
	}

	//-- Vaciar buffer de entrada
	//-- De momento se hace leyendo muchos datos y descartando
	private void flush()
	{
		byte[] buffer = new byte[256];

		//try, catch done because mono-1.2.3 throws an exception when there's a timeout
		//http://bugzilla.gnome.org/show_bug.cgi?id=420520
		bool success = false;
		AbortFlush = false;
		do{
			try{
				sp.Read(buffer,0,256);
				success = true;
				LogB.Debug(" spReaded ");
			} catch {
				LogB.Warning(" catchedTimeOut ");
			}

		} while(! success && ! AbortFlush);
		if(AbortFlush) {
			LogB.Information("Abort flush");
		}
	}

	/*
	 * this will read what's in the serial port until time out
	*/	
	private void flushByTimeOut()
	{
		byte[] buffer = new byte[256];

		//try, catch done because mono-1.2.3 throws an exception when there's a timeout
		//http://bugzilla.gnome.org/show_bug.cgi?id=420520
		bool timeOut = false;
		do{
			try{
				sp.Read(buffer,0,256);
				LogB.Debug(" spReaded ");
			} catch {
				LogB.Warning(" catchedTimeOut ");
				timeOut = true;
			}

		} while(! timeOut);
	}


	public void Flush() {
		flush();
	}
	
	public void FlushByTimeOut() {
		flushByTimeOut();
	}

	/**********************************/
	/* TIPOS PRIVADOS                 */
	/**********************************/
	//-- Identificacion de las tramas
	private enum Trama
	{
		Evento =  'X',  //-- Trama de evento
		       Estado =  'E',  //-- Trama de solicitud de estado
		       REstado = 'E',  //-- Trama de respuesta de estado
	}

	/*********************************************************************/
	/* CONSTANTES PRIVADAS                                               */
	/*********************************************************************/
	private const int DefaultTimeout = 100;  //-- En ms 
	private const int ErrorTimeout   = 500;  //-- En ms

	//------------------------------
	//   Propiedades privadas
	//------------------------------
	//-- Puerto serie donde esta conectada la Chronopic
	private SerialPort sp;
	//-- Ultimo error que se ha producido
	private ErrorType error = ErrorType.Ok;

}


public static class ChronopicPorts
{
	public static string [] GetPorts() {
		if(UtilAll.GetOSEnum() == UtilAll.OperatingSystems.LINUX)
			return UtilAll.AddArrayString (
					Directory.GetFiles ("/dev/", "ttyUSB*"),
					Directory.GetFiles ("/dev/", "ttyACM*"));
		else if(UtilAll.GetOSEnum() == UtilAll.OperatingSystems.MACOSX)
			return Directory.GetFiles("/dev/", "tty.usbserial*");
		else // WINDOWS
			return SerialPort.GetPortNames();
	}
}

public class ChronopicInit 
{
	public bool CancelledByUser;

	public ChronopicInit () 
	{
	}

	//chronopic init should not touch  gtk, for the threads
	public bool Do (int currentCp, out Chronopic myCp, out SerialPort mySp, Chronopic.Plataforma myPS, string myPort, out string returnString, out bool success) 
	{
		LogB.Information("starting connection with chronopic");

		CancelledByUser = false;
		success = true;
		
		LogB.Information("chronopicInit-1");		
		LogB.Information(string.Format("chronopic port: {0}", myPort));
		mySp = new SerialPort(myPort);
		try {
			mySp.Open();
			LogB.Information("chronopicInit-2");		
			//-- Create chronopic object, for accessing chronopic
			myCp = new Chronopic(mySp);
			
			LogB.Information("chronopicInit-2.1");
			if(mySp.BytesToRead > 0)
				myCp.Flush();
			
			//if myCp has been cancelled
			if(myCp.AbortFlush) {
				LogB.Information("chronopicInit-2.2 cancelled");
				success = false;
				myCp = new Chronopic(); //fake constructor
			} else {
				LogB.Information("chronopicInit-3");		
				//on windows, this check make a crash 
				//i think the problem is: as we don't really know the Timeout on Windows (.NET) and this variable is not defined on chronopic.cs
				//the Read_platform comes too much soon (when cp is not totally created), and this makes crash

				//-- Obtener el estado inicial de la plataforma

				/*
				   since 2.2.2 do not make connect having user to press platform.
				   Just directly do the jump or race

				bool ok=false;
				LogB.Information("chronopicInit-4");		
				do {
					LogB.Information("chronopicInit-5");		
					ok = myCp.Read_platform(out myPS);
					LogB.Information("chronopicInit-6");		
				} while(! ok && ! CancelledByUser);
				LogB.Information("chronopicInit-7");		
				if (!ok) {
					//-- Si hay error terminar
					LogB.Error(string.Format("Error: {0}", myCp.Error));
					success = false;
				}
				*/
				success = true;
			}
		} catch {
			LogB.Error("chronopicInit-2.a catched");
			success = false;
			myCp = new Chronopic(); //fake constructor
		}
		
		returnString = "";
		return success;
	}
	
}
