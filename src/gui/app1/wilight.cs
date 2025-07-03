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
using Gtk;
using Mono.Unix;

public partial class ChronoJumpWindow 
{
	// at glade ---->
	Gtk.Box box_wilight;
	Gtk.Box box_start_wilight;
	Gtk.Grid grid_wilight_controller_terminals;
	Gtk.Box box_wilight_test_actions2;
	//Gtk.SpinButton spin_wilight_portnum;
	Gtk.Entry entry_wilight_port;
	Gtk.Button button_wilight_test_cancel;
	Gtk.Button button_wilight_test_finish;
	Gtk.SpinButton spin_wilight_test_ping;
	Gtk.CheckButton check_wilight_test_r;
	Gtk.CheckButton check_wilight_test_g;
	Gtk.CheckButton check_wilight_test_b;
	Gtk.CheckButton check_wilight_test_blink;
	Gtk.Entry entry_wilight_blacklist;
	Gtk.CheckButton check_wilight_show_commands;
	Gtk.CheckButton check_wilight_very_verbose;
	Gtk.Box box_wilight_commands;
	Gtk.TextView textview_wilight;
	// <---- at glade

	static Thread threadWilight;
	static WilightCaptureManage wilightCaptureManage;
	static bool wilightProcessCancel;
	//static bool wilightProcessFinish; //not implemented at the moment
	static string wilightMessage;

	//to play sound by pulse thread
	enum wilightSoundEnum { NONE, GOOD, BAD };
	static wilightSoundEnum haveToPlaySound;

	enum wilightActions { DISCOVER, PING, CHANGECOLOR, SPEED, DEMO, SEQUENCE };
	private wilightActions wilightAction;
	DateTime wilightTimeStartCapture;

	//use the string to not have crash by manipulating the TextBuffer outside the pulse thread
	static string tbWilightText = "";
	static bool needToUpdateTextViewWilight;
	static bool needToUpdateGraphWilight;
	TextBuffer tbWilight = new TextBuffer (new TextTagTable());

	CairoGraphWilight cairoGraphWilight;
	WilightTerminalLayout wilightTerminalLayout;
	WilightCommand currentWilightCommand;

	private void wilightApp1Init ()
	{
		tbWilightText = "";
		button_wilight_test_cancel.Sensitive = false;
		button_wilight_test_finish.Sensitive = false;

		box_wilight_commands.Visible = check_wilight_show_commands.Active;
		updateGraphWilightBars();

		textview_wilight.Name = "fontSize9";
		entry_wilight_port.Text = "";

		currentWilightCommand = new WilightCommand ();
		wilightTerminalLayout = new WilightTerminalLayout ();

		if (Util.FileExists (configChronojump.WilightLayoutURL))
			wilightTerminalLayout.ReadFile (configChronojump.WilightLayoutURL);
	}

	/*
	private void blankWilightInterface()
	{
		//currentWilight = new Wilight ();
	}
	*/

	private void on_check_wilight_show_commands_clicked (object o, EventArgs args)
	{
		box_wilight_commands.Visible = check_wilight_show_commands.Active;
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

	private bool wilightFilesExist ()
	{
		List<string> missing_l = Util.FilesExists (
				new List<string> {
				configChronojump.WilightLayoutURL, configChronojump.WilightCommandsURL
				});

		if (missing_l.Count > 0)
		{
			event_execute_label_message.Text = string.Format ("Missing file/s: {0}. Fix it and restart Chronojump",
					UtilList.ListStringToString (missing_l, ", "));
			box_wilight_test_actions2.Sensitive = false;
			return false;
		}

		return true;
	}

	private void on_button_wilight_test_speed_clicked (object o, EventArgs args)
	{
		if (! wilightFilesExist ())
			return;

		wilightAction = wilightActions.SPEED;
		wilightExecute ();
	}

	private void on_button_wilight_test_sequence_clicked (object o, EventArgs args)
	{
		if (! wilightFilesExist ())
			return;

		wilightAction = wilightActions.SEQUENCE;
		wilightExecute ();
	}

	private void on_button_wilight_test_demo_clicked (object o, EventArgs args)
	{
		if (! wilightFilesExist ())
			return;

		wilightAction = wilightActions.DEMO;
		wilightExecute ();
	}

	private void on_button_wilight_test_cancel_clicked (object o, EventArgs args)
	{
		wilightProcessCancel = true;
	}

	private void on_button_wilight_test_finish_clicked (object o, EventArgs args)
	{
		//wilightProcessFinish = true;
	}

	private void wilightExecute ()
	{
		grid_wilight_controller_terminals.Sensitive = false;
		box_wilight_test_actions2.Sensitive = false;
		tbWilightText = "";
		needToUpdateTextViewWilight = false;

		wilightProcessCancel = false;
		button_wilight_test_cancel.Sensitive = true;
		button_wilight_test_finish.Sensitive = true;
		//wilightProcessFinish = false;
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

			wilightMessage = Catalog.GetString ("Device seems disconnected.");
			LogB.Information ("cannot connect");
			return false;
		}

		wichroCapture.Flush (); //to be able to read later

		return true;
	}

	private void wilightTestDo ()
	{
		string commandsFile = configChronojump.WilightCommandsURL;

		/*
		//testing stuff
		LogB.Information ("wilightTest");
		WilightCaptureManage wcm = new WilightCaptureManage (commandsFile);
		while (true)
		{
			if (wcm.Finished) //finished here to have also time to answer to the last command
				return;
			wcm.GetNext ();
		}
		*/

		/*
		if (! wilightManageConnect (
					string.Format ("/dev/ttyUSB{0}", Convert.ToInt32 (spin_wilight_portnum.Value))
					))
			return;
		*/
		if (entry_wilight_port.Text == "" || ! wilightManageConnect (entry_wilight_port.Text))
		{
			Util.PlaySound (Constants.SoundTypes.BAD, preferences.volumeOn, preferences.gstreamer);
			return;
		}

		if (wilightAction == wilightActions.DISCOVER ||
				wilightAction == wilightActions.PING ||
				wilightAction == wilightActions.CHANGECOLOR)
		{
			if (wilightAction == wilightActions.DISCOVER)
				discoverWilight ();
			else if (wilightAction == wilightActions.PING)
				ping (Convert.ToInt32 (spin_wilight_test_ping.Value));
			else if (wilightAction == wilightActions.CHANGECOLOR)
				changeColor (Convert.ToInt32 (spin_wilight_test_ping.Value),
						Util.BoolToInt (check_wilight_test_r.Active),
						Util.BoolToInt (check_wilight_test_g.Active),
						Util.BoolToInt (check_wilight_test_b.Active),
						Util.BoolToInt (check_wilight_test_blink.Active));

			grid_wilight_controller_terminals.Sensitive = true;
			box_wilight_test_actions2.Sensitive = true;

			wichroCapture.Stop(); //Should we do a disconnect here?
			return;
		}

		sendCommandAndUpdateWilightTextview (wilightTerminalLayout.ColorAll (WilightColors.OFF).ToArduinoString);
		currentWilightCommand = wilightTerminalLayout.ColorAll (WilightColors.OFF);
		needToUpdateGraphWilight = true;
		System.Threading.Thread.Sleep (100);

		if (wilightAction == wilightActions.SPEED)
		{
			testSpeed ();
			wichroCapture.Stop(); //Should we do a disconnect here?
		}
		else if (wilightAction == wilightActions.SEQUENCE)
		{
			testSequence (commandsFile, entry_wilight_blacklist.Text, false);
			wichroCapture.Stop(); //Should we do a disconnect here?
		}
		else if (wilightAction == wilightActions.DEMO)
		{
			testSequence (commandsFile, entry_wilight_blacklist.Text, true);
			wichroCapture.Stop(); //Should we do a disconnect here?
		}

		haveToPlaySound = wilightSoundEnum.NONE;

		System.Threading.Thread.Sleep (50);

		sendCommandAndUpdateWilightTextview (wilightTerminalLayout.ColorAll (WilightColors.OFF).ToArduinoString);
		currentWilightCommand = wilightTerminalLayout.ColorAll (WilightColors.OFF);
		needToUpdateGraphWilight = true;
	}

	private void discoverWilight ()
	{
		updateWilightTextview ("\n> local:discover;");
		updateWilightTextview ("\n (this operation can last 30s)");

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
		updateWilightTextview (string.Format ("\n> {0}:512;", terminal));

		bool commandSendOk = wichroCapture.Ping (terminal);

		if (! commandSendOk)
			LogB.Information ("Error on call to ping");
		else {
			LogB.Information ("ping called ok");
			updateWilightTextview ("\n< " + wichroCapture.wilightResponse);
		}
	}

	//just to debug drawingarea
	private void plotSequenceWithoutSending ()
	{
		currentWilightCommand = new WilightCommand ("0:34;1:0;2:8;3:4;4:12;5:2;6:10;7:40;8:36;9:35;10:14;11:46;12:0;");
		drawingarea_results_realtime.QueueDraw ();
	}

	private void changeColor (int terminal, int r, int g, int b, int blink)
	{
		WilightCommand wilightCommand = new WilightCommand (string.Format ("{0}:{1};", terminal,
				r*2 + g*4 + b*8 + blink*32));

		System.Threading.Thread.Sleep (50);
		sendCommandAndUpdateWilightTextview (wilightCommand.ToArduinoString);
		currentWilightCommand = wilightCommand; //note this will overwrite the command
		drawingarea_results_realtime.QueueDraw ();
	}

	//TODO: send only to discovered terminals
	private void testSpeed ()
	{
		List<WilightCommand> colorsAll_l = new List<WilightCommand> ();
		colorsAll_l.Add (wilightTerminalLayout.ColorAll (WilightColors.RED));
		colorsAll_l.Add (wilightTerminalLayout.ColorAll (WilightColors.GREEN));
		colorsAll_l.Add (wilightTerminalLayout.ColorAll (WilightColors.BLUE));
		int sleepTime = 500;
		int sleepTimeMin = 100; //does not work very good at 50
		int countDownAtMaxSpeed = 10; //to execute 10 times at max speed
		bool done = false;

		while (! done)
		{
			foreach (WilightCommand wilightCommand in colorsAll_l)
			{
				sendCommandAndUpdateWilightTextview (wilightCommand.ToArduinoString);
				currentWilightCommand = wilightCommand;
				needToUpdateGraphWilight = true;
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

	private void testSequence (string commandsFile, string blacklistStr, bool isDemo)
	{
		wilightCaptureManage = new WilightCaptureManage (wilightTerminalLayout, commandsFile, blacklistStr, isDemo);
		wilightMessage = wilightCaptureManage.GetProgressStatus ();

		List<int> expectedTerminals_l = new List<int> (); //expected response on this (or them)

		bool firstCommand = true;
		while (true)
		{
			if (wilightCaptureManage.Finished | wilightCaptureManage.Cancel) //finished here to have also time to answer to the last command
			{
				if (wilightCaptureManage.Finished)
					updateWilightTextview (string.Format ("\nTotal time: {0} ms",
								wilightCaptureManage.LastTime));
				if (wilightCaptureManage.Cancel)
					updateWilightTextview ("Cancelled");

				break;
			}

			WilightCommand wilightCommand = wilightCaptureManage.GetNext ();
			wilightMessage = wilightCaptureManage.GetProgressStatus ();
			if (wilightCommand.IsEmpty)
				continue;

			if (firstCommand)
			{
				//0 time on the microcontroller and add command. All in a single sendCommand to be faster.
				sendCommandAndUpdateWilightTextview ("local:reset_time;" + wilightCommand.ToArduinoString);
				firstCommand = false;
			} else
				sendCommandAndUpdateWilightTextview (wilightCommand.ToArduinoString);
			expectedTerminals_l = wilightCommand.GetExpectedTerminals ();
			currentWilightCommand = wilightCommand;
			needToUpdateGraphWilight = true;

			if (wilightCommand.TimeMs > 0)
			{
				wilightCommand.TimeStart ();
				while (! wilightCommand.TimeFinished ())
					System.Threading.Thread.Sleep (20);

				continue;
			}

			bool readedFromExpected = false;
			while (! readedFromExpected)
			{
				if (wilightCaptureManage.Cancel)
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
					if (UtilList.FoundInListInt (expectedTerminals_l, we.photocell) && we.status == Chronopic.Plataforma.ON)
					{
						updateWilightTextview ("\n< " + we.ToString ());

						haveToPlaySound = wilightSoundEnum.GOOD;
						readedFromExpected = true;

						wilightCaptureManage.AddToOnString (we.ToString ());
						wilightCaptureManage.SetLastOnTime (we.timeMs);
						wilightCaptureManage.CommandsCountReceivedAdd ();
						//wilightMessage = wilightCaptureManage.GetProgressStatus ();
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
		//TODO: merge these needTo...
		if (needToUpdateTextViewWilight)
		{
			tbWilight.Text = tbWilightText;
			textview_wilight.Buffer = tbWilight;
			UtilGtk.TextViewScrollToEnd (textview_wilight);
			needToUpdateTextViewWilight = false;
		}
		if (needToUpdateGraphWilight)
		{
			drawingarea_results_realtime.QueueDraw ();
			needToUpdateGraphWilight = false;
		}

		event_execute_label_message.Text = wilightMessage;

		if (haveToPlaySound == wilightSoundEnum.GOOD)
		{
			haveToPlaySound = wilightSoundEnum.NONE;
			Util.PlaySound (Constants.SoundTypes.GOOD, preferences.volumeOn, preferences.gstreamer);
		} else if (haveToPlaySound == wilightSoundEnum.BAD)
		{
			haveToPlaySound = wilightSoundEnum.NONE;
			Util.PlaySound (Constants.SoundTypes.BAD, preferences.volumeOn, preferences.gstreamer);
		}

		if (! threadWilight.IsAlive || wilightProcessCancel)
		{
			if (wilightProcessCancel && wilightCaptureManage != null)
				wilightCaptureManage.Cancel = true;

			if (wilightCaptureManage != null && wilightCaptureManage.Finished)
			{
				if (! wilightCaptureManage.IsDemo) //if is not a demo, save it and update treeview and graph
				{
					int exerciseID = 0;
					if (configChronojump.WilightExerciseID > 0)
						exerciseID = configChronojump.WilightExerciseID;

					LogB.Information ("Finished! create object");
					Wilight w = new Wilight (-1, currentPerson.UniqueID, currentSession.UniqueID, exerciseID,
							UtilDate.ToFile (wilightTimeStartCapture), "", //videoURL
							wilightCaptureManage.LastTime,
							wilightCaptureManage.OnStringAsString, "");
					LogB.Information ("Insert to SQL!");
					w.UniqueID = w.InsertSQL (false);
					LogB.Information ("Inserted!");

					treeViewResultsSession.Add (currentPerson.UniqueID, currentPerson.Name, w, "");
					updateGraphWilightBars();
				}

				event_execute_label_message.Text = "Finished";
			}

			grid_wilight_controller_terminals.Sensitive = true;
			box_wilight_test_actions2.Sensitive = true;
			button_wilight_test_cancel.Sensitive = false;
			button_wilight_test_finish.Sensitive = false;
			return false;
		}

		LogB.Information(" Cur:" + threadWilight.ThreadState.ToString());
		Thread.Sleep (50);
		return true;
	}

	private void sendCommandAndUpdateWilightTextview (string commandStr)
	{
		bool sendCommandFeedback = wichroCapture.WilightSendCommand (commandStr);
		updateWilightTextview ("\n> " + commandStr);
	}
	private void updateWilightTextview (string str)
	{
		tbWilightText += str;
		needToUpdateTextViewWilight = true;
	}

	private void updateGraphWilightBars ()
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
		if (treeViewResultsSession != null && treeViewResultsSession.EventSelectedID >= 0)
			selectedID = treeViewResultsSession.EventSelectedID;

		PrepareEventGraphWilight eventGraph = new PrepareEventGraphWilight(
				1, //unused?
				currentSession.UniqueID,
				currentPerson.UniqueID, radio_contacts_results_personAll.Active,
				-1 * Convert.ToInt32 (spin_resultsSession_limit.Value), //negative: end limit
				//Constants.WilightTable, typeTemp,
				selectedID);

		//if(eventGraph.personMAXAtSQLAllSessions > 0 || eventGraph.runsAtSQL.Count > 0)
		//	PrepareRunSimpleGraph(eventGraph, false); //don't animate

		string personStr = "";
		if(! radio_contacts_results_personAll.Active)
			personStr = currentPerson.Name;

		LogB.Information("drawingarea_results_session == null: ",
			(drawingarea_results_session == null).ToString());

		cairoPaintBarsPre = new CairoPaintBarsWilight (
				drawingarea_results_session, preferences.fontTypeToGraph(), current_mode,
				personStr, typeTemp, preferences.digitsNumber, currentPerson.UniqueID);

		cairoPaintBarsPre.StoreEventGraphWilight (eventGraph);
		//PrepareRunSimpleGraph(cairoPaintBarsPre.eventGraphRunsStored, false); //do not need, draw event will graph it:
		drawingarea_results_session.QueueDraw ();
	}

	private void connectWidgetsWilight (Gtk.Builder builder)
	{
		box_wilight = (Gtk.Box) builder.GetObject ("box_wilight");
		box_start_wilight = (Gtk.Box) builder.GetObject ("box_start_wilight");
		grid_wilight_controller_terminals = (Gtk.Grid) builder.GetObject ("grid_wilight_controller_terminals");
		box_wilight_test_actions2 = (Gtk.Box) builder.GetObject ("box_wilight_test_actions2");
		//spin_wilight_portnum = (Gtk.SpinButton) builder.GetObject ("spin_wilight_portnum");
		entry_wilight_port = (Gtk.Entry) builder.GetObject ("entry_wilight_port");
		button_wilight_test_cancel = (Gtk.Button) builder.GetObject ("button_wilight_test_cancel");
		button_wilight_test_finish = (Gtk.Button) builder.GetObject ("button_wilight_test_finish");
		spin_wilight_test_ping = (Gtk.SpinButton) builder.GetObject ("spin_wilight_test_ping");
		check_wilight_test_r = (Gtk.CheckButton) builder.GetObject ("check_wilight_test_r");
		check_wilight_test_g = (Gtk.CheckButton) builder.GetObject ("check_wilight_test_g");
		check_wilight_test_b = (Gtk.CheckButton) builder.GetObject ("check_wilight_test_b");
		check_wilight_test_blink = (Gtk.CheckButton) builder.GetObject ("check_wilight_test_blink");
		entry_wilight_blacklist = (Gtk.Entry) builder.GetObject ("entry_wilight_blacklist");
		check_wilight_show_commands = (Gtk.CheckButton) builder.GetObject ("check_wilight_show_commands");
		check_wilight_very_verbose = (Gtk.CheckButton) builder.GetObject ("check_wilight_very_verbose");
		box_wilight_commands = (Gtk.Box) builder.GetObject ("box_wilight_commands");
		textview_wilight = (Gtk.TextView) builder.GetObject ("textview_wilight");
	}
}

