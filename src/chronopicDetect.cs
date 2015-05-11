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
 * Copyright (C) 2004-2015   Xavier de Blas <xaviblas@gmail.com> 
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

	public bool Detecting; //used to block closing chronojump window if true
	public string Detected; //readed from chronojump window
	private ChronopicInit chronopicInit;
	private bool connectedNormalChronopic;
	
	public Gtk.Button FakeButtonDone;
	
	public ChronopicDetect (SerialPort sp, Gtk.ProgressBar progressbar, Gtk.Button button_cancel, Gtk.Button button_info)
	{
		this.sp = sp;
		this.progressbar = progressbar;
		this.button_cancel = button_cancel;
		this.button_info = button_info;
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
			new ChronopicAutoDetect(ChronopicAutoDetect.ChronopicType.ENCODER);

		Detected = cad.Detected;
	}
	
	private void detectNormal()
	{
		//simulateDriverProblem(); //uncomment to check cancel, info buttons behaviour

		ChronopicAutoDetect cad = 
			new ChronopicAutoDetect(ChronopicAutoDetect.ChronopicType.NORMAL);

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
