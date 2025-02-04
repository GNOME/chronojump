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
 * Copyright (C) 2024-2025   Xavier de Blas <xaviblas@gmail.com>
 */

using System;
using System.Diagnostics; //Stopwatch
using Gtk;

//TODO: note this dirty code is just for testing
public partial class ChronoJumpWindow 
{
	// at glade ---->
	Gtk.Box box_wilight;
	Gtk.Box box_start_wilight;
	Gtk.Box box_wilight_test_actions;
	Gtk.SpinButton spin_wilight_portnum;
	Gtk.Button button_wilight_test_cancel;
	Gtk.Button button_wilight_test_finish;
	Gtk.SpinButton spin_wilight_test_ping;
	Gtk.Box box_combo_wilight_single_color;
	Gtk.CheckButton check_wilight_very_verbose;
	Gtk.Label label_wilight_test_status;
	Gtk.TextView textview_wilight;
	// <---- at glade

	Gtk.ComboBoxText combo_wilight_single_color;

	static Thread threadWilight;
	static WilightTest wilightTest;
	static bool wilightProcessCancel;
	static bool wilightProcessFinish;

	//to play sound by pulse thread
	enum wilightSoundEnum { NONE, GOOD, BAD };
	static wilightSoundEnum haveToPlaySound;

	enum wilightActions { DISCOVER, PING, CHANGECOLOR, SPEED, SEQUENCE };
	private wilightActions wilightAction;
	DateTime wilightTimeStartCapture;

	private IDNameList wilightColor_l;

	//use the string to not have crash by manipulating the TextBuffer outside the pulse thread
	static string tbWilightText = "";
	static bool needToUpdateTextViewWilight;
	TextBuffer tbWilight = new TextBuffer (new TextTagTable());

	//called only once
	private void wilightApp1Init ()
	{
		tbWilightText = "";
		button_wilight_test_cancel.Sensitive = false;
		button_wilight_test_finish.Sensitive = false;
		updateGraphWilight();

		wilightColor_l = new IDNameList ();
		wilightColor_l.Add (new IDName (0, "Black"));
		wilightColor_l.Add (new IDName (128, "Red"));
		wilightColor_l.Add (new IDName (64, "Green"));
		wilightColor_l.Add (new IDName (32, "Blue"));

		combo_wilight_single_color = new ComboBoxText ();
		UtilGtk.ComboUpdate (combo_wilight_single_color, wilightColor_l.GetNames ());
		combo_wilight_single_color.Active = 0;
		box_combo_wilight_single_color.PackStart (combo_wilight_single_color, true, true, 0);
		box_combo_wilight_single_color.ShowAll ();

		textview_wilight.Name = "fontSize9";
	}

	private void on_button_wilight_test_discover_clicked (object o, EventArgs args)
	{
		wilightAction = wilightActions.DISCOVER;
		wilightExecute ();
	}

	private void on_button_wilight_test_ping_clicked (object o, EventArgs args)
	{
		wilightAction = wilightActions.PING;
		wilightExecute ();
	}

	private void on_button_wilight_test_change_color_clicked (object o, EventArgs args)
	{
		wilightAction = wilightActions.CHANGECOLOR;
		wilightExecute ();
	}

	private void on_button_wilight_test_speed_clicked (object o, EventArgs args)
	{
		wilightAction = wilightActions.SPEED;
		wilightExecute ();
	}

	private void on_button_wilight_test_sequence_clicked (object o, EventArgs args)
	{
		wilightAction = wilightActions.SEQUENCE;
		wilightExecute ();
	}

	private void on_button_wilight_test_cancel_clicked (object o, EventArgs args)
	{
		wilightProcessCancel = true;
	}

	private void on_button_wilight_test_finish_clicked (object o, EventArgs args)
	{
		wilightProcessFinish = true;
	}

	private void wilightExecute ()
	{
		box_wilight_test_actions.Sensitive = false;
		label_wilight_test_status.Text = "Doing";
		tbWilightText = "";
		needToUpdateTextViewWilight = false;

		wilightProcessCancel = false;
		button_wilight_test_cancel.Sensitive = true;
		button_wilight_test_finish.Sensitive = true;
		wilightProcessFinish = false;
		wilightTimeStartCapture = DateTime.Now; //to have an active count of capture time

		threadWilight = new Thread (new ThreadStart (wilightTestDo));
		GLib.Idle.Add (new GLib.IdleHandler (pulseWilight));

		LogB.ThreadStart();
		threadWilight.Start();
	}

	private bool wilightManageConnect (string portName)
	{
		if(wichroCapture != null && wichroCapture.PortOpened)
			wichroCapture.Disconnect();

		if(wichroCapture == null || wichroCapture.PortName != portName)
		{
			wichroCapture = new WichroCapture (portName);
		}

		wichroCapture.Reset ();

		if (! wichroCapture.CaptureStart ())
		{
			//chronopicDisconnected = true;
			wichroCapture.Disconnect ();
			//cancel = true; //problem reading line (capturing)
			Util.PlaySound (Constants.SoundTypes.BAD, preferences.volumeOn, preferences.gstreamer);
			LogB.Information ("cannot connect");
			return false;
		}

		wichroCapture.Flush (); //to be able to read later

		return true;
	}

	private void wilightTestDo ()
	{
		string commandsFile = configChronojump.WilightCommandsURL;
		int commandTimeMs = configChronojump.WilightCommandMs;

		/*
		//testing stuff
		LogB.Information ("wilightTest");
		WilightTest wtz = new WilightTest (commandsFile);
		while (true)
		{
			if (wtz.Finished) //finished here to have also time to answer to the last command
				return;
			wtz.GetNext ();
		}
		*/

		if (! wilightManageConnect (
					string.Format ("/dev/ttyUSB{0}", Convert.ToInt32 (spin_wilight_portnum.Value))
					))
			return;

		if (wilightAction == wilightActions.DISCOVER ||
				wilightAction == wilightActions.PING ||
				wilightAction == wilightActions.CHANGECOLOR)
		{
			if (wilightAction == wilightActions.DISCOVER)
				discover ();
			else if (wilightAction == wilightActions.PING)
				ping (Convert.ToInt32 (spin_wilight_test_ping.Value));
			else if (wilightAction == wilightActions.CHANGECOLOR)
				changeColor (Convert.ToInt32 (spin_wilight_test_ping.Value),
						wilightColor_l.FindID (UtilGtk.ComboGetActive (
								combo_wilight_single_color)));

			box_wilight_test_actions.Sensitive = true;
			label_wilight_test_status.Text = "";

			wichroCapture.Stop(); //Should we do a disconnect here?
			return;
		}

		sendCommandAndUpdateWilightTextview (WilightColors.AllOffCommand);
		System.Threading.Thread.Sleep (1000);

		//0 time on the microcontroller
		sendCommandAndUpdateWilightTextview ("reset_time");

		if (wilightAction == wilightActions.SPEED)
		{
			testSpeed ();
			wichroCapture.Stop(); //Should we do a disconnect here?
		}
		else if (wilightAction == wilightActions.SEQUENCE)
		{
			testSequence (commandsFile);
			wichroCapture.Stop(); //Should we do a disconnect here?
		}

		haveToPlaySound = wilightSoundEnum.NONE;

		System.Threading.Thread.Sleep (1000);
		sendCommandAndUpdateWilightTextview (WilightColors.AllOffCommand);
	}

	private void discover ()
	{
		sendCommandAndUpdateWilightTextview ("local:discover;");

		System.Threading.Thread.Sleep (50);
		bool commandSendOk = wichroCapture.Discover ();

		if (! commandSendOk)
			LogB.Information ("Error on call to discover");
		else {
			LogB.Information ("discover called ok");
			updateWilightTextview ("\n< " + wichroCapture.wilightResponse);
		}
	}

	private void ping (int terminal)
	{
		sendCommandAndUpdateWilightTextview (string.Format ("{0}:512;", terminal));
		System.Threading.Thread.Sleep (50);

		bool commandSendOk = wichroCapture.Ping (terminal);

		if (! commandSendOk)
			LogB.Information ("Error on call to ping");
		else {
			LogB.Information ("ping called ok");
			updateWilightTextview ("\n< " + wichroCapture.wilightResponse);
		}
	}

	private void changeColor (int terminal, int code)
	{
		string command = string.Format ("{0}:{1};", terminal, code);
		System.Threading.Thread.Sleep (50);
		sendCommandAndUpdateWilightTextview (command);
	}

	//TODO: send only to discovered terminals
	private void testSpeed ()
	{
		List<string> colorsAll_l = new List<string> ();
		colorsAll_l.Add (WilightColors.AllRedCommand);
		colorsAll_l.Add (WilightColors.AllGreenCommand);
		colorsAll_l.Add (WilightColors.AllBlueCommand);
		int sleepTime = 500;
		int sleepTimeMin = 100; //does not work very good at 50
		int countDownAtMaxSpeed = 10; //to execute 10 times at max speed
		bool done = false;

		while (! done)
		{
			foreach (string colorAllStr in colorsAll_l)
			{
				sendCommandAndUpdateWilightTextview (colorAllStr);
				System.Threading.Thread.Sleep (sleepTime);
			}
			sleepTime -= 50;
			if (sleepTime < sleepTimeMin)
			{
				sleepTime = sleepTimeMin;
				countDownAtMaxSpeed --;
			}
			//LogB.Information ("sleepTime: " + sleepTime.ToString ());
			if (countDownAtMaxSpeed <= 0)
				done = true;
		}
	}

	private void testSequence (string commandsFile)
	{
		wilightTest = new WilightTest (commandsFile);
		List<int> expectedTerminals_l = new List<int> (); //expected response on this (or them)

		while (true)
		{
			if (wilightTest.Finished | wilightTest.Cancel) //finished here to have also time to answer to the last command
			{
				if (wilightTest.Finished)
					updateWilightTextview (string.Format ("\nTotal time: {0} ms", wilightTest.FinishedMs));
				if (wilightTest.Cancel)
					updateWilightTextview ("Cancelled");

				break;
			}

			string command = wilightTest.GetNext ();
			LogB.Information ("command = " + command);
			if (command == "")
				continue;

			sendCommandAndUpdateWilightTextview (command);
			expectedTerminals_l = wilightTest.GetExpectedTerminals (command);

			bool readedFromExpected = false;
			while (! readedFromExpected)
			{
				if (wilightTest.Cancel)
				{
					updateWilightTextview ("Cancelled");
					break;
				}

				if(! wichroCapture.CaptureSample())
				{
					LogB.Information ("Problem capturing sample");
					haveToPlaySound = wilightSoundEnum.BAD;

					break;
				}

				if(wichroCapture.CanReadFromList ())
				{
					LogB.Information ("Can read");
					WichroEvent we = wichroCapture.WichroCaptureReadNext();
					if (UtilList.FoundInListInt (expectedTerminals_l, we.photocell))
					{
						updateWilightTextview ("\n< " + we.ToString ());

						haveToPlaySound = wilightSoundEnum.GOOD;
						readedFromExpected = true;
					}
					else if (check_wilight_very_verbose.Active)
						updateWilightTextview ("\n< " + we.ToString ());
				}
				System.Threading.Thread.Sleep (20);
			}
		}
	}

	private bool pulseWilight ()
	{
		if (needToUpdateTextViewWilight)
		{
			tbWilight.Text = tbWilightText;
			textview_wilight.Buffer = tbWilight;
			UtilGtk.TextViewScrollToEnd (textview_wilight);
			needToUpdateTextViewWilight = false;
		}

		if (! threadWilight.IsAlive || wilightProcessCancel)
		{
			if (wilightProcessCancel && wilightTest != null)
				wilightTest.Cancel = true;

			if (wilightTest != null && wilightTest.Finished)
			{
				LogB.Information ("Finished! create object");
				Wilight w = new Wilight (-1, currentPerson.UniqueID, currentSession.UniqueID, 0,
						UtilDate.ToFile (wilightTimeStartCapture), "", //videoURL
						wilightTest.FinishedMs);
				LogB.Information ("Insert to SQL!");
				w.InsertSQL (false);
				LogB.Information ("Inserted!");

				myTreeViewWilight.Add (currentPerson.Name, w, "");

				updateGraphWilight();
			}

			box_wilight_test_actions.Sensitive = true;
			button_wilight_test_cancel.Sensitive = false;
			button_wilight_test_finish.Sensitive = false;
			label_wilight_test_status.Text = "Done";
			return false;
		}

		if (haveToPlaySound == wilightSoundEnum.GOOD)
		{
			haveToPlaySound = wilightSoundEnum.NONE;
			Util.PlaySound (Constants.SoundTypes.GOOD, preferences.volumeOn, preferences.gstreamer);
		} else if (haveToPlaySound == wilightSoundEnum.BAD)
		{
			haveToPlaySound = wilightSoundEnum.NONE;
			Util.PlaySound (Constants.SoundTypes.BAD, preferences.volumeOn, preferences.gstreamer);
		}

		LogB.Information(" Cur:" + threadWilight.ThreadState.ToString());
		Thread.Sleep (50);
		return true;
	}

	private void sendCommandAndUpdateWilightTextview (string command)
	{
		bool sendCommandFeedback = wichroCapture.WilightSendCommand (command);
		updateWilightTextview ("\n> " + command);
	}
	private void updateWilightTextview (string str)
	{
		tbWilightText += str;
		needToUpdateTextViewWilight = true;
	}

	private void updateGraphWilight ()
	{
		if(currentPerson == null || currentSession == null)
			return;

		//intializeVariables if not done before
		event_execute_initializeVariables(
			(! cp2016.StoredCanCaptureContacts && ! cp2016.StoredWireless), //is simulated
			currentPerson.UniqueID,
			currentPerson.Name,
			"", //Catalog.GetString("Phases"),  	  //name of the different moments
			Constants.WilightTable, //tableName
			"" //type
			);

		/*
		string typeTemp = currentEventType.Name;
		if(radio_contacts_graph_allTests.Active)
			typeTemp = "";
			*/
		string typeTemp = "";

		int selectedID = -1;
		if (myTreeViewWilight != null && myTreeViewWilight.EventSelectedID > 0)
			selectedID = myTreeViewWilight.EventSelectedID;

		PrepareEventGraphWilight eventGraph = new PrepareEventGraphWilight(
				1, //unused?
				currentSession.UniqueID,
				currentPerson.UniqueID, radio_contacts_results_personAll.Active,
				-1 * Convert.ToInt32 (spin_contacts_graph_last_limit.Value), //negative: end limit
				//Constants.WiightTable, typeTemp,
				selectedID);

		//if(eventGraph.personMAXAtSQLAllSessions > 0 || eventGraph.runsAtSQL.Count > 0)
		//	PrepareRunSimpleGraph(eventGraph, false); //don't animate

		string personStr = "";
		if(! radio_contacts_results_personAll.Active)
			personStr = currentPerson.Name;

		LogB.Information("event_execute_drawingarea_cairo == null: ",
			(event_execute_drawingarea_cairo == null).ToString());

		cairoPaintBarsPre = new CairoPaintBarsWilight (
				event_execute_drawingarea_cairo, preferences.fontTypeToGraph(), current_mode,
				personStr, typeTemp, preferences.digitsNumber);

		cairoPaintBarsPre.StoreEventGraphWilight (eventGraph);
		//PrepareRunSimpleGraph(cairoPaintBarsPre.eventGraphRunsStored, false); //do not need, draw event will graph it:
		event_execute_drawingarea_cairo.QueueDraw ();
	}

	private void connectWidgetsWilight (Gtk.Builder builder)
	{
		box_wilight = (Gtk.Box) builder.GetObject ("box_wilight");
		box_start_wilight = (Gtk.Box) builder.GetObject ("box_start_wilight");
		box_wilight_test_actions = (Gtk.Box) builder.GetObject ("box_wilight_test_actions");
		spin_wilight_portnum = (Gtk.SpinButton) builder.GetObject ("spin_wilight_portnum");
		button_wilight_test_cancel = (Gtk.Button) builder.GetObject ("button_wilight_test_cancel");
		button_wilight_test_finish = (Gtk.Button) builder.GetObject ("button_wilight_test_finish");
		spin_wilight_test_ping = (Gtk.SpinButton) builder.GetObject ("spin_wilight_test_ping");
		box_combo_wilight_single_color = (Gtk.Box) builder.GetObject ("box_combo_wilight_single_color");
		check_wilight_very_verbose = (Gtk.CheckButton) builder.GetObject ("check_wilight_very_verbose");
		label_wilight_test_status = (Gtk.Label) builder.GetObject ("label_wilight_test_status");
		textview_wilight = (Gtk.TextView) builder.GetObject ("textview_wilight");
	}
}

