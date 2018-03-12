/*
 * This file is part of ChronoJump
 *
 * Chronojump is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or   
 *    (at your option) any later version.
 *    
 * Chronojump is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the 
 *    GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 *  along with this program; if not, write to the Free Software
 *   Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 *
 * Copyright (C) 2004-2017   Xavier de Blas <xaviblas@gmail.com> 
 */


using System;
using System.Threading;
using System.IO.Ports;
using Gtk;

//methods specific of the Automatic firmware
//for "automatic" firmware 1.1: debounce can change, get version, port scanning
public abstract class ChronopicAuto 
{
	protected SerialPort sp;
	protected int sendNum;
	public bool IsChronopicAuto;
	protected internal abstract string Communicate();
	private string str;
	public string CharToSend = "";
	public bool IsEncoder = false;

	public enum ChronopicType { UNDETECTED, NORMAL, ENCODER }
	public ChronopicType Found;

	private bool make(SerialPort sp) 
	{
		this.sp = sp;

		if (sp == null) 
			return false;

		if (sp.IsOpen)
		{
			LogB.Information("Port is opened. flushing ... ");
			byte[] buffer = new byte[256];
			for (int count = 0; count < 1; count ++) //flush by reading buffer or timeout 1 time
			{
				try{
					sp.Read(buffer,0,256);
					LogB.Debug(" spReaded ");
				} catch {
					LogB.Warning(" catchedTimeOut ");
				}
				count ++;
			}

			LogB.Information("flushed");
		} else {
			LogB.Information("Port is closed. Opening ... ");
			sp.Open();
		}

		LogB.Information("ready!");

		if(IsEncoder)
			setEncoderBauds();

		str = "";
		return true;
	}

	/*
	 * this method closes the port to "flush" but this doesn't work always
	 * is best to use above method that does an explicit flush
	 *
	private bool makeOld(SerialPort sp)
	{
		this.sp = sp;

		if (sp == null)
			return false;
	
		try {	
			if (sp != null) 
				if (sp.IsOpen) {
					LogB.Information("Port is opened. Closing ... ");
					sp.Close(); //close to ensure no bytes are comming
					LogB.Information("closed");
				}

			LogB.Information("opening port... ");
			sp.Open();
		} catch {
			LogB.Warning("catched!");
			return false;
		}
		LogB.Information("opened");
			
		if(IsEncoder)
			setEncoderBauds();

		str = "";
		return true;
	}
	*/

	private void close(SerialPort sp) {
		LogB.Information("closing port... ");
		sp.Close();
	}	


	//'template method'
	public string Read(SerialPort sp) 
	{
		if ( ! make(sp) )
			return "Error sp == null";
		
		//bool needToFlush = false;
		try {
			str = Communicate();
		} catch {
			//this.error=ErrorType.Timeout;
			LogB.Warning("Error or Timeout. This is not Chronopic-Automatic-Firmware");
			str = "Error / not Multitest firmware";
			
			//needToFlush = true;
		}
		
		/*	
		if(needToFlush)
			flush();
			*/

		//better don't close it
		//close(sp);
		
		return str;
	}
	
	//'template method'
	public string Write(SerialPort sp, int num) 
	{
		if ( ! make(sp) )
			return "Error sp == null";
		
		sendNum = num;
		
		//bool needToFlush = false;
		try {
			str = Communicate();
		} catch {
			//this.error=ErrorType.Timeout;
			LogB.Warning("Error or Timeout. This is not Chronopic-Automatic-Firmware");
			str = "Error / not Multitest firmware";

			//needToFlush = true;
		}
	
		/*	
		if(needToFlush)
			flush();
			*/
		
		//better don't close it
		//close(sp);
		
		return str;
	}

	private void setEncoderBauds()
	{
		sp.BaudRate = 115200; //encoder, 20MHz
		LogB.Information("sp.BaudRate = 115200 bauds");
	}

	/*
	protected void flush() 
	{
		LogB.Information("Flushing");
		
		//-- Esperar un tiempo y vaciar buffer
		Thread.Sleep(500); //ErrorTimeout);
		
		byte[] buffer = new byte[256];
		sp.Read(buffer,0,256); //flush
		
		bool success = false;
		try {
			do{
				sp.Read(buffer,0,256);
				success = true;
				LogB.Debug(" spReaded ");
			} while(! success);
		} catch {
			LogB.Information("Cannot flush");
			return;
		}

		LogB.Information("Flushed");
	}
	*/
}

public class ChronopicAutoCheck : ChronopicAuto
{
	protected internal override string Communicate() 
	{
		Found = ChronopicAuto.ChronopicType.UNDETECTED;
		sp.Write("J");
		IsChronopicAuto = ( (char) sp.ReadByte() == 'J');
		if (IsChronopicAuto) 
		{
			sp.Write("V");
			int major = (char) sp.ReadByte() - '0'; 
			sp.ReadByte(); 		//.
			int minor = (char) sp.ReadByte() - '0'; 

			Found = ChronopicAuto.ChronopicType.NORMAL;
			return "Yes! v" + major.ToString() + "." + minor.ToString();
		}

		return "Please update it\nwith Chronopic-firmwarecord";
	}
}
//only for encoder
public class ChronopicAutoCheckEncoder : ChronopicAuto
{
	protected internal override string Communicate() 
	{
		LogB.Information("Communicate start ...");
		
		Found = ChronopicAuto.ChronopicType.UNDETECTED;
	
		char myByte;
		for(int i = 0; i < 100; i ++) //try 100 times (usually works on Linux 3-5 try, Mac 8-10, Windows don't work < 20... trying bigger numbers)
		{
			LogB.Debug("writting ...");
	
			sp.Write("J");
			
			LogB.Debug("reading ...");

			myByte = (char) sp.ReadByte();
			
			LogB.Debug("readed");
			if(myByte.ToString() != "")
				LogB.Information(myByte.ToString());
			
			if(myByte == 'J') {
				LogB.Information("Encoder found!");

				Found = ChronopicAuto.ChronopicType.ENCODER;
				return "1";
			}
		}
		
		return "0";
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

public class ChronopicStartReactionTime : ChronopicAuto
{
	protected internal override string Communicate() 
	{
		try {
			sp.Write(CharToSend);
			LogB.Information("sending",CharToSend);
		} catch {
			return "ERROR";
		}
		return "SUCCESS";
	}
}

//like above method but sending the waiting time between each light
public class ChronopicStartReactionTimeAnimation : ChronopicAuto
{
	protected internal override string Communicate() 
	{
		try {
			byte b;
			if(CharToSend == "l")
				b = 0x6c;
			else if(CharToSend == "f")
				b = 0x66;
			else if(CharToSend == "r")
				b = 0x72;
			else if(CharToSend == "s")
				b = 0x73;
			else if(CharToSend == "t")
				b = 0x74;
			else if(CharToSend == "T") //green and buzzer
				b = 0x54;
			else if(CharToSend == "y") //red green #TODO: maybe should be 79 https://www.asciitable.com/
				b = 0x76;
			else if(CharToSend == "Z")
				b = 0x5A;
			else
				return "ERROR";

			//values go from 0 to 7
			byte[] bytesToSend = new byte[2] { b, BitConverter.GetBytes(sendNum)[0] }; //eg. l, 05; of f, 05
			sp.Write(bytesToSend,0,2);

		} catch {
			return "ERROR";
		}
		return "SUCCESS";
	}
}

