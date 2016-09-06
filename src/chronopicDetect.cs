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
 * Copyright (C) 2004-2016   Xavier de Blas <xaviblas@gmail.com> 
 */


using System;
using System.Threading;
using System.IO.Ports;
using Gtk;

public class ChronopicDetect
{
	Thread thread;
	
	Gtk.ProgressBar progressbar;
	Gtk.Button button_cancel;
	Gtk.Button button_info;
	
	private static bool cancel;
	private static bool needToChangeProgressbarText;
	private SerialPort sp;
	private Config.AutodetectPortEnum configAutoDetect;

	public bool Detecting; //used to block closing chronojump window if true
	public string Detected; //readed from chronojump window
	private ChronopicInit chronopicInit;
	private bool connectedNormalChronopic;
	
	public Gtk.Button FakeButtonDone;
	
	public ChronopicDetect (SerialPort sp, Gtk.ProgressBar progressbar, Gtk.Button button_cancel, Gtk.Button button_info,
			Config.AutodetectPortEnum configAutoDetect )
	{
		this.sp = sp;
		this.progressbar = progressbar;
		this.button_cancel = button_cancel;
		this.button_info = button_info;
		this.configAutoDetect = configAutoDetect;

		button_cancel.Clicked += new EventHandler(on_button_cancel_clicked);
		button_info.Clicked += new EventHandler(on_button_info_clicked);

		FakeButtonDone = new Gtk.Button();
		Detecting = false;
	}
	
	public void Detect(string mode)
	{
		//set variables	
		cancel = false;
		Detected = "";
		Detecting = true;
		connectedNormalChronopic = false;
		
		progressbar.Text = Constants.ChronopicDetecting;
		needToChangeProgressbarText = false;


		if(mode == "ENCODER") {
			LogB.Information("Detecting encoder... ");
			thread = new Thread(new ThreadStart(detectEncoder));
		} else {
			LogB.Information("Detecting normal Chronopic... ");
			thread = new Thread(new ThreadStart(detectNormal));
		}
		
		GLib.Idle.Add (new GLib.IdleHandler (PulseGTK));

		LogB.ThreadStart();
		thread.Start(); 
	}

	private void detectEncoder()
	{
		//simulateDriverProblem(); //uncomment to check cancel, info buttons behaviour

		ChronopicAutoDetect cad = 
			new ChronopicAutoDetect(ChronopicAutoDetect.ChronopicType.ENCODER, configAutoDetect);

		Detected = cad.Detected;
	}
	
	private void detectNormal()
	{
		//simulateDriverProblem(); //uncomment to check cancel, info buttons behaviour

		ChronopicAutoDetect cad = 
			new ChronopicAutoDetect(ChronopicAutoDetect.ChronopicType.NORMAL, configAutoDetect);

		Detected = cad.Detected;
		
		if(Detected != null && Detected != "") 
		{
			needToChangeProgressbarText = true;
			connectNormal(Detected);
		}
		LogB.Debug("detectNormal ended");
	}

	private static Chronopic cpDoing;
	private static Chronopic.Plataforma platformState;	//on (in platform), off (jumping), or unknow
	
	private void connectNormal(string myPort)
	{
		LogB.Debug("connectNormal start");
		
		chronopicInit = new ChronopicInit();
		
		string message = "";
		bool success = false;
		connectedNormalChronopic = chronopicInit.Do(1, out cpDoing, out sp, platformState, myPort, out message, out success);
		LogB.Information(message);
		LogB.Debug("connectNormal end");
	}


	private void simulateDriverProblem() 
	{
		//testing a fault in drivers
		int count = 0;
		bool crash = true;
		while(crash) {
			count ++;
			if(count >= 40000) {
				LogB.Debug(" at simulateDriverProblem\n ");
				count = 0;
			}
		}
	}
	
	private bool PulseGTK ()
	{
		if(cancel || ! thread.IsAlive) {
			LogB.ThreadEnding();

			if(cancel)
				thread.Abort();

			LogB.Information("Connected = " + connectedNormalChronopic.ToString());
			
			FakeButtonDone.Click();	//send signal to gui/chronojump.cs to read Detected
			Detecting = false;
			
			LogB.ThreadEnded();
			return false;
		}

		progressbar.Pulse();
		
		if(needToChangeProgressbarText) {
			progressbar.Text = Constants.ChronopicNeedTouch;
			needToChangeProgressbarText = false;
		}
		
		Thread.Sleep (50);
		LogB.Debug(thread.ThreadState.ToString());
		return true;
	}
	
	private void on_button_cancel_clicked (object o, EventArgs args)
	{
		button_cancel.Clicked -= new EventHandler(on_button_cancel_clicked);

		Detected = "Cancelled";
		cancel = true;
	}
	
	private void on_button_info_clicked (object o, EventArgs args)
	{
		string str = Constants.FindDriverNeed;
		
		if(UtilAll.IsWindows())
			str += "\n\n" + Constants.FindDriverWindows;
		else	
			str += "\n\n" + Constants.FindDriverOthers;

		new DialogMessage(Constants.MessageTypes.INFO, str);
	} 
	
	//will be sent to chronopicWin
	public Chronopic getCP() {
		return cpDoing;
	}


	~ChronopicDetect() {}
}

public class ChronopicAutoDetect
{
	public enum ChronopicType { UNDETECTED, NORMAL, ENCODER }
	private ChronopicType searched;

	public string Detected; // portname if detected, if not will be ""
	private Config.AutodetectPortEnum configAutoDetect;

	public ChronopicAutoDetect(ChronopicType type, Config.AutodetectPortEnum configAutoDetect)
	{
		/*
		 * Try to detect a normal 4MHz Chronopic on a 20MHz encoder fails
		 * but encoder can be used normally
		 * In the other hand, try to detect an encoder on a 4MHz Chronopic fails
		 * but encoder cannot be used until 'reset' or disconnect cable (and can be problems with Chronojump GUI)
		 *
		 * So the solution is:
		 * if we are searching encoder, on every port first check if 4MHz connection can be stablished, if it's Found, then normal Chronopic is found
		 * if is not Found, then search for the encoder.
		 *
		 * The only problem is in normal Chronopics with old firmware (without the 'J' read/write)
		 * they will not work after trying to be recognised as an encoder, until reset or disconnect cable
		 *
		 */
		this.searched = type;
		this.configAutoDetect = configAutoDetect;
		
		if(configAutoDetect == Config.AutodetectPortEnum.INACTIVE) {
			return;
			Detected = "";
		}

		//no matter if we are searching for 4MHz or 20MHz (encoder)
		//first see if 4MHz is connected
		ChronopicAuto caNormal = new ChronopicAutoCheck();
		caNormal.IsEncoder = false;    //for the bauds.

		autoDetect(caNormal);
	}

	private void autoDetect(ChronopicAuto caNormal)
	{
		LogB.Information("starting port detection");

		string [] usbSerial = ChronopicPorts.GetPorts();

		bool first = true;
		foreach(string port in usbSerial) 
		{
			if(configAutoDetect == Config.AutodetectPortEnum.DISCARDFIRST && first) {
				first = false;
				LogB.Warning("Discarded port = ", port);
				continue;
			}

			SerialPort sp = new SerialPort(port);
			
			LogB.Information("searching normal Chronopic at port: ", port);
			string readed = caNormal.Read(sp);

			if(caNormal.Found == ChronopicType.NORMAL) //We found a normal Chronopic
			{
				if(searched == ChronopicType.NORMAL) //normal Chronopic is what we are searching
				{
					Detected = port;
					return;
				} else {
					/*
					 * else: 
					 * means that we are searching for an encoder chronopic and found a normal
					 * so don't try to search for an encoder on that port, because 115200 bauds will saturate it
					 */
					LogB.Information("our goal is to search encoder but found normal Chronopic at port: ", port);
				}
			} else if(searched == ChronopicType.ENCODER) 
			{
				/*
				 * we are searching an encoder
				 * if we arrived here, we know is not a normal chronopic
				 * then we can search safely for an encoder here
				 */
				ChronopicAuto caEncoder = new ChronopicAutoCheckEncoder();
				caEncoder.IsEncoder = true;    //for the bauds.
			
				LogB.Information("searching encoder Chronopic at port: ", port);
				readed = caEncoder.Read(sp);
				if(caEncoder.Found == ChronopicType.ENCODER) 
				{
					Detected = port;
					return;
				}
			}
		}
		Detected = "";
	}
}

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
	public ChronopicAutoDetect.ChronopicType Found;

	private bool make(SerialPort sp) 
	{
		this.sp = sp;

		if (sp == null) 
			return false;
	
		LogB.Information("opening port... ");
		try {	
			if (sp != null) 
				if (sp.IsOpen)
					sp.Close(); //close to ensure no bytes are comming

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

		close(sp);
		
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
		
		close(sp);
		
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
		Found = ChronopicAutoDetect.ChronopicType.UNDETECTED;
		sp.Write("J");
		IsChronopicAuto = ( (char) sp.ReadByte() == 'J');
		if (IsChronopicAuto) 
		{
			sp.Write("V");
			int major = (char) sp.ReadByte() - '0'; 
			sp.ReadByte(); 		//.
			int minor = (char) sp.ReadByte() - '0'; 

			Found = ChronopicAutoDetect.ChronopicType.NORMAL;
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
		
		Found = ChronopicAutoDetect.ChronopicType.UNDETECTED;
	
		char myByte;
		for(int i = 0; i < 100; i ++) //try 100 times (usually works on Linux 3-5 try, Mac 8-10, Windows don't work < 20... trying bigger numbers)
		{
			LogB.Debug("writting ...");
	
			sp.Write("J");
			
			LogB.Debug("reading ...");

			myByte = (char) sp.ReadByte();
			
			LogB.Debug("readed");
			if(myByte != null && myByte.ToString() != "")
				LogB.Information(myByte.ToString());
			
			if(myByte == 'J') {
				LogB.Information("Encoder found!");

				Found = ChronopicAutoDetect.ChronopicType.ENCODER;
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
			else if(CharToSend == "y") //red green
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

