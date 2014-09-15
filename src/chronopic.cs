/*
 * Copyright (C) 2005  Juan Gonzalez Gomez
 * Copyright (C) 2014  Xavier de Blas <xaviblas@gmail.com>
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
Console.Write("0");
		//-- Comprobar si puerto serie ya estaba abierto
		if (sp != null)
			if (sp.IsOpen)
				sp.Close();

Console.Write("1");
		//-- Abrir puerto serie
		sp.Open();

Console.Write("2");
		//-- Configurar timeout por defecto
		//de momento no en windows (hasta que no encontremos por qué falla)
		//OperatingSystem os = Environment.OSVersion;
		//not used, now there's no .NET this was .NET related
		//on mono timeouts work on windows and linux
		//if( ! os.Platform.ToString().ToUpper().StartsWith("WIN"))
			sp.ReadTimeout = DefaultTimeout;

Console.Write("3");
		//-- Guardar el puerto serie
		this.sp = sp;

Console.Write("4");
//		//-- Vaciar buffer
//		//done in a separate method
//		this.flush();
	}

	//-- Destructor
	~Chronopic()
	{
		//-- Cerrar puerto serie
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
		Console.WriteLine("---------------------------");
		Console.WriteLine("ReadVarAutoStart");

		if (sp == null)
			sp.Open(); 
		
		Console.WriteLine("ReadVarAutoOpened");

		try {
			sp.Write("J");
			Console.WriteLine("Port scanning (should return 'J'): " + (char) sp.ReadByte());
		} catch {
			this.error=ErrorType.Timeout;
			Console.WriteLine("Timeout. This is not Chronopic-Automatic-Firmware");
			return;
		}

		
		sp.Write("V");
		Console.WriteLine("Version: " + 
				(char) sp.ReadByte() +
				(char) sp.ReadByte() +
				(char) sp.ReadByte() 
			       	);

		int debounce = 0;

		sp.Write("a");
		debounce = ( sp.ReadByte() - '0' ) * 10;
		Console.WriteLine("\nCurrent debounce time: " + debounce.ToString());

		Console.WriteLine("Changing to 10 ms ... ");
		sp.Write("b\x01");

		sp.Write("a");
		debounce = ( sp.ReadByte() - '0' ) * 10;
		Console.WriteLine("Current debounce time: " + debounce.ToString());

		Console.WriteLine("Changing to 50 ms ... ");
		sp.Write("b\x05");

		sp.Write("a");
		debounce = ( sp.ReadByte() - '0' ) * 10;
		Console.WriteLine("Current debounce time: " + debounce.ToString());

		Console.WriteLine("---------------------------");

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
			
Console.Write(" o1 ");
		if (sp != null)
			if (sp.IsOpen) 
				sp.Close();
Console.Write(" o2 ");
		
		try {
			sp.Open();
		} catch {
			status=false;
			plataforma = Plataforma.UNKNOW;
			this.error=ErrorType.Timeout;
			return status;
		}
Console.Write(" o3 ");

//Console.Write("h");
		//-- Enviar la trama por el puerto serie
		sp.Write(trama,0,1);
//Console.Write("i");

		//-- Esperar a que llegue la respuesta
		//-- Se espera hasta que en el buffer se tengan el numero de bytes
		//-- esperados para la trama. (para esta trama 2). Si hay un 
		//-- timeout se aborta
		count=0;
		do {
//Console.Write("j");
			n = sp.Read(respuesta,count,2-count);
			count+=n;
		} while (count<2 && n!=-1);

//Console.Write("k");
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

//Console.Write("l");
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
					count+=n;
					success = true;
				} catch {}
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
				Log.Write(" spReaded ");
			} catch {
				Log.Write(" catchedTimeOut ");
			}

		} while(! success && ! AbortFlush);
		if(AbortFlush) {
			Log.WriteLine("Abort flush");
		}
	}

	public void Flush() {
		flush();
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


//methods specific of the Automatic firmware
//for "automatic" firmware 1.1: debounce can change, get version, port scanning
public abstract class ChronopicAuto 
{
	protected SerialPort sp;
	protected int sendNum;
	protected internal abstract string Communicate();
	private string str;

	private bool make(SerialPort sp) 
	{
		this.sp = sp;

		if (sp == null) 
			return false;
		
		if (sp != null) 
			if (sp.IsOpen)
				sp.Close(); //close to ensure no bytes are comming

		sp.Open();

		str = "";
		return true;
	}

	//'template method'
	public string Read(SerialPort sp) 
	{
		if ( ! make(sp) )
			return "Error sp == null";
		
		try {
			str = Communicate();
		} catch {
			//this.error=ErrorType.Timeout;
			Console.WriteLine("Error or Timeout. This is not Chronopic-Automatic-Firmware");
			str = "Error";
		}
		
		return str;
	}
	
	//'template method'
	public string Write(SerialPort sp, int num) 
	{
		if ( ! make(sp) )
			return "Error sp == null";
		
		sendNum = num;
		
		try {
			str = Communicate();
		} catch {
			//this.error=ErrorType.Timeout;
			Console.WriteLine("Error or Timeout. This is not Chronopic-Automatic-Firmware");
			str = "Error";
		}
		
		return str;
	}
}

public class ChronopicAutoCheck : ChronopicAuto
{
	protected internal override string Communicate() 
	{
		sp.Write("J");
		bool isChronopicAuto = ( (char) sp.ReadByte() == 'J');
		if (isChronopicAuto) {
			sp.Write("V");
			int major = (char) sp.ReadByte() - '0'; 
			sp.ReadByte(); 		//.
			int minor = (char) sp.ReadByte() - '0'; 
			return "Yes! v" + major.ToString() + "." + minor.ToString();
		}
		return "Please update it\nwith Chronopic-firmwarecord";
	}
}

public class ChronopicAutoCheckDebounce : ChronopicAuto
{
	protected internal override string Communicate() 
	{
		sp.Write("a");
		int debounce = ( sp.ReadByte() - '0' ) * 10;
		return debounce.ToString() + " ms";
	}
}

public class ChronopicAutoChangeDebounce : ChronopicAuto
{
	protected internal override string Communicate() 
	{
		int debounce = sendNum / 10;		//50 -> 5
		
		//byte[] bytesToSend = new byte[2] { 0x62, 0x05 }; //b, 05 //this works
		byte[] bytesToSend = new byte[2] { 0x62, BitConverter.GetBytes(debounce)[0] }; //b, 05
		sp.Write(bytesToSend,0,2);
		
		return "Changed to " + sendNum.ToString() + " ms";
	}
}


