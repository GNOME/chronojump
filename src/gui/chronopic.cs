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
 * Copyright (C) 2004-2022   Xavier de Blas <xaviblas@gmail.com>
 */


using System;
using Gtk;
using Gdk;
//using Glade;
using System.IO.Ports;
using Mono.Unix;
using System.Threading;
using System.IO; //"File" things
using System.Collections; //ArrayList
using System.Collections.Generic; //List

//gui stuff for the connection of one or two chronopics
public partial class ChronoJumpWindow
{
	//multi chronopic stuff
	//List<ChronopicRegisterPort> crpMultiList;

	ChronopicRegisterPort crpDoing;
	//bool connectAnother;
	int connectNum;
	enum connectingSequenceEnum { START, CONNECTINGREAL, FIRMWAREIFNEEDED, END }
	private static connectingSequenceEnum connectingSequence;

	private void chronopicConnectionSequenceInit (ChronopicRegisterPort crpDoing)
	{
		// 2.2.2
		this.crpDoing = crpDoing;
		//store a boolean in order to read info faster
		cp2016.StoredCanCaptureContacts = true;
		connectNum = 1;

		/* before 2.2.2
		connectNum = 1;
		if(numCPs == 1)
		{
			connectAnother = false;
			crpDoing = chronopicRegister.ConnectedOfType(ChronopicRegisterPort.Types.CONTACTS);
			//store a boolean in order to read info faster
			cp2016.StoredCanCaptureContacts = true;
		}
		else
		{ //2
			connectAnother = true;
			//will get two crps or null
			crpMultiList = chronopicRegister.GetTwoContactsConnected();
			//store a boolean in order to read info faster
			if(crpMultiList == null)
				return;

			crpDoing = crpMultiList[0];

			cp2016.StoredCanCaptureContacts = (crpMultiList.Count == 2);
		}
		*/
		connectingSequence = connectingSequenceEnum.START;
		chronopicConnectionSequenceDo();

	}

	private void chronopicConnectionSequenceDo()
	{
		//0 print sequence
		LogB.Information("SEQUENCE: " + connectingSequence.ToString());

		//1 check if need to end sequence or go to second chronopic
		if (connectingSequence == connectingSequenceEnum.END)
		{
			/*
			if(connectAnother)
			{
				System.Threading.Thread.Sleep(250);
				crpDoing = crpMultiList[1];
				connectingSequence = connectingSequenceEnum.START;
				connectAnother = false;
				connectNum = 2;
				chronopicConnectionSequenceDo();
				return;
			} else {
			*/
				on_button_execute_test_acceptedPre_start_camera(ChronoJumpWindow.WebcamStartedTestStart.CHRONOPIC);
				return;
			//}
		}

		//2 update sequence
		if(connectingSequence == connectingSequenceEnum.START)
		{
			if(cp2016.IsLastConnectedReal(crpDoing, connectNum))
				connectingSequence = connectingSequenceEnum.FIRMWAREIFNEEDED;
			else
				connectingSequence = connectingSequenceEnum.CONNECTINGREAL;

			chronopicConnectionSequenceDo();
		}
		else if (connectingSequence == connectingSequenceEnum.CONNECTINGREAL)
		{
			callConnectContactsReal(crpDoing, connectNum);
			//this opens a thread and when end goes to chronopicConnectionSequenceDo again
		}
		else if (connectingSequence == connectingSequenceEnum.FIRMWAREIFNEEDED)
		{
			changeMultitestFirmwareIfNeeded(connectNum);
			//this will call chronopicConnectionSequenceDo if success
		}
	}

	private void callConnectContactsReal(ChronopicRegisterPort crp, int numCP)
	{
		cp2016.FakeButtonContactsRealDone.Clicked +=
			new EventHandler(on_connection_contacts_real_done);

		string message = Catalog.GetString("Please, touch the platform or click Chronopic TEST button.");
		if(current_mode == Constants.Modes.RUNSSIMPLE ||
				current_mode == Constants.Modes.RUNSINTERVALLIC)
			message = Catalog.GetString("Please, cut photocell barrier or click Chronopic TEST button.");

		cp2016.ConnectContactsReal(app1, crp, numCP, message);
	}

	private void on_connection_contacts_real_done (object o, EventArgs args)
	{
		cp2016.FakeButtonContactsRealDone.Clicked -=
			new EventHandler(on_connection_contacts_real_done);

		if(cp2016.SuccededConnectContactsRealThread)
		{
			LogB.Information("Success at Connecting real! (main GUI)");

			//manage show threshold stuff
			threshold.ChronopicFirmwareReconnected(1); 	//t_stored_on_chronopic will be 50, and later firmware will be changed. TODO: may this work for Chronopic 2
			label_threshold.Text = //Catalog.GetString("Threshold") + " " +
				threshold.GetLabel() + " ms";
			if(threshold.GetT == 50)
				button_threshold.TooltipText = label_threshold.Text + " (" + Catalog.GetString("Applied") + ")";

			UtilGtk.PrintLabelWithTooltip(event_execute_label_message,
					Catalog.GetString("Connected to Chronopic"));

			//2.2.2 At end of connection change firmware (because connection, change firmware, execute is on the same first button_press)
			connectingSequence = connectingSequenceEnum.FIRMWAREIFNEEDED;
			chronopicConnectionSequenceDo();
		} else
			LogB.Warning("Failure at Connecting real! (main GUI)");
	}

	private void changeMultitestFirmwareIfNeeded(int cpCount)
	{
		//change multitest stuff
		threshold.UpdateAtDatabaseIfNeeded(current_mode);
		if(threshold.ShouldUpdateChronopicFirmware(cpCount))
		{
			bool ok = cp2016.ChangeMultitestFirmwarePre(threshold.GetT, cpCount);
			if(ok) {
				threshold.ChronopicFirmwareUpdated(cpCount);
				button_threshold.TooltipText = label_threshold.Text + " (" + Catalog.GetString("Applied") + ")";
			} else
				button_threshold.TooltipText = label_threshold.Text + " (" + Catalog.GetString("Failed") + ")";
		}

		connectingSequence = connectingSequenceEnum.END;
		chronopicConnectionSequenceDo();
	}

}
