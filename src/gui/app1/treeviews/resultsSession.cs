//note this is not valid for encoder until we move all the widgets to contacts

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
 * Copyright (C) 2025   Xavier de Blas <xaviblas@gmail.com>
 */


using System;
using Gtk;
using Mono.Unix;
using System.Collections; //ArrayList

// unified treeview for all modes that will act depending on mode
// instead of traditional treeview for each mode
public partial class ChronoJumpWindow 
{
	// at glade ---->
	Gtk.ScrolledWindow scrolledwindow_treeview_results_session;
	Gtk.Box box_results_session_zoom;
	Gtk.TreeView treeview_results_session;
	// <---- at glade

	private TreeViewEvent treeViewResultsSession;

	private void createTreeView_resultsSession (Gtk.TreeView tv)
	{
		LogB.Information ("createTreeView_resultsSession mode = " + current_mode.ToString ());
		//just to have following code shorter
		int pdn = preferences.digitsNumber;
		TreeViewEvent.ExpandStates minimized = TreeViewEvent.ExpandStates.MINIMIZED;

		if (current_mode == Constants.Modes.JUMPSSIMPLE)
			treeViewResultsSession = new TreeViewJumps (tv, preferences, minimized, radio_resultsSession_jump_heights.Active);
		else if (current_mode == Constants.Modes.JUMPSREACTIVE)
			treeViewResultsSession = new TreeViewJumpsRj (tv, preferences, minimized);
		else if (current_mode == Constants.Modes.RUNSSIMPLE)
			treeViewResultsSession = new TreeViewRuns (tv, pdn, preferences.metersSecondsPreferred, minimized, radio_resultsSession_run_speeds.Active);
		else if (current_mode == Constants.Modes.RUNSINTERVALLIC)
			treeViewResultsSession = new TreeViewRunsInterval (tv, pdn, preferences.metersSecondsPreferred, minimized, radio_resultsSession_run_speeds.Active);
		else if (current_mode == Constants.Modes.RUNSENCODER)
			treeViewResultsSession = new TreeViewRunEncoder (tv, pdn, minimized);
		else if (current_mode == Constants.Modes.BEEPTEST)
			treeViewResultsSession = new TreeViewBeepTest (tv, pdn, minimized );
		else if (Constants.ModeIsFORCESENSOR (current_mode))
			treeViewResultsSession = new TreeViewForceSensor (tv, pdn, minimized );
		else if (Constants.ModeIsENCODER (current_mode))
			treeViewResultsSession = new TreeViewEncoder (tv, pdn,
					current_mode == Constants.Modes.POWERGRAVITATORY, minimized);
		else if (current_mode == Constants.Modes.WILIGHT)
			treeViewResultsSession = new TreeViewWilight (tv, pdn, minimized );
		else if (current_mode == Constants.Modes.OTHER)
			treeViewResultsSession = new TreeViewFourPlatforms (tv, pdn, minimized );
		else
			treeViewResultsSession = new TreeViewJumps (tv, preferences, minimized, radio_resultsSession_jump_heights.Active); //default to fix any temporary crash at start (seems there is a personChanged but still not mode)

		//the glade cursor_changed does not work on mono 1.2.5 windows
		tv.CursorChanged -= on_treeview_results_session_cursor_changed;
		tv.CursorChanged += on_treeview_results_session_cursor_changed;
	}

	private void on_button_results_session_zoom_clicked (object o, EventArgs args)
	{
		treeViewResultsSession.ZoomChange (image_results_session_zoom);
	}

	// if treeviewResults changes person, then store here the value of the results id.
	// Change person (will change treeview and lots of widgets) And then select this id on treeviewResults.
	private int personChangingFromResultsId = -1;

	// encoder finishPulsebar updates treeview and store.Remove calls cursor_changed. Block this.
	private bool treeview_results_session_cursor_changed_block = false;

	// Important! see: diagrams/processes/person_results_changes.dia
	// note on right click, this event is always managed first
	int treeViewResultsSessionEventSelectedIDLast = -1; //to not load same set again (on encoder)
	private void on_treeview_results_session_cursor_changed (object o, EventArgs args)
	{
		LogB.Information ("on_treeview_results_session_cursor_changed");

		if (treeViewResultsSession == null)
			return;

		if (treeview_results_session_cursor_changed_block)
		{
			LogB.Information ("blocked: cursor_changed");
			return;
		}

		if (Constants.ModeIsENCODER (current_mode))
		{
			if (treeViewResultsSession.EventSelectedID == treeViewResultsSessionEventSelectedIDLast)
			{
				LogB.Information ("blocked: encoder tried to select same row, avoid load.");
				return;
			}
			else if (treeViewResultsSession.GetIDOfSelectedSubEvent () == treeViewResultsSessionEventSelectedIDLast)
			{
				LogB.Information ("blocked: encoder tried to select a rep of same set. Selecting header line.");
				treeViewResultsSession.SelectEventHeaderLine();
				return;
			}
		}

		// Check if clicked to another person
		if (currentPerson != null && ! treeviewResultsSessionNoCheckPersonChange)
		{
			int personID = treeViewResultsSession.GetPersonIDOfSelectedRow;
			if (personID != currentPerson.UniqueID)
			{
				// If clicked to another person and to a test, store the test to be selected later
				if (treeViewResultsSession.EventSelectedID >= 0)
					personChangingFromResultsId = treeViewResultsSession.EventSelectedID;
				// but if clicked on a subevent (on two level treeviews), then obtain the 1st level id
				else if (treeViewResultsSession.EventSelectedID == TreeViewEvent.MarkNonSelectRowSubEvent)
					personChangingFromResultsId = treeViewResultsSession.GetIDOfSelectedSubEvent ();
				LogB.Information ("personChangingFromResultsId: " + personChangingFromResultsId.ToString ());

				//1st update the variable
				int personPrevious = currentPerson.UniqueID;
				//treeViewResultsSession.CurrentPersonID = currentPerson.UniqueID;
				treeViewResultsSession.CurrentPersonID = personID;

				pre_fillTreeView_resultsSession_NO = true;

				// select the person
				selectRowTreeView_persons (treeview_persons, myTreeViewPersons.FindRow (personID));
				// now currentPerson, currentPersionSession have been updated

				treeViewResultsSession.PersonEmitRowChanged (personPrevious); // show normal
				treeViewResultsSession.PersonEmitRowChanged (currentPerson.UniqueID); // show in bold

				pre_fillTreeView_resultsSession_NO = false;

				return;
			}
		}

		if (current_mode == Constants.Modes.JUMPSSIMPLE ||
				current_mode == Constants.Modes.RUNSSIMPLE ||
				current_mode == Constants.Modes.BEEPTEST ||
				current_mode == Constants.Modes.WILIGHT)
			on_treeview_test_cursor_changed (false); // no load set
		else if (Constants.ModeIsFORCESENSOR (current_mode) ||
				Constants.ModeIsENCODER (current_mode) ||
				current_mode == Constants.Modes.RUNSENCODER)
		{
			pre_fillTreeView_resultsSession_NO = true; //see comment on gui/app1/chronojumpPersons.cs
			on_treeview_test_cursor_changed (true); // load set
			pre_fillTreeView_resultsSession_NO = false;
		} else {
			// other tests with specific functions (not necessarily all 2 levels, because encoder does not go here)
			if (current_mode == Constants.Modes.JUMPSREACTIVE)
				on_treeview_jumps_rj_cursor_changed (o, args);
			else if (current_mode == Constants.Modes.RUNSINTERVALLIC)
				on_treeview_runs_interval_cursor_changed (o, args);
			else if (current_mode == Constants.Modes.OTHER) 	//FOURPLATFORMS
				on_treeview_fourPlatforms_cursor_changed (o, args);
		}

		treeViewResultsSessionEventSelectedIDLast = treeViewResultsSession.EventSelectedID;
	}

	private void on_treeview_test_cursor_changed (bool loadSet)
	{
		sensitiveLastTestButtons(false);

		// don't select if it's a person
		// is for not confusing with the person treeviews that controls who does the test
		if (treeViewResultsSession.EventSelectedID == TreeViewEvent.MarkRowIsPerson)
		{
			showHideActionEventButtons(false); //hide

			if (Constants.ModeIsFORCESENSOR (current_mode))
				blankForceSensorInterface ();
			//else if (Constants.ModeIsENCODER (current_mode))
			//	blankEncoderInterface ();
			else if (current_mode == Constants.Modes.RUNSENCODER)
				blankRunEncoderInterface ();
		} else {
			if (loadSet)
			{
				LogB.Information (string.Format ("going to load id: {0}, on mode: {1}", treeViewResultsSession.EventSelectedID, current_mode));
				if (Constants.ModeIsFORCESENSOR (current_mode))
					forceSensorLoadSignalAcceptedDo (treeViewResultsSession.EventSelectedID, -1, currentSession.UniqueID, ForceSensor.GetElasticIntFromMode (current_mode), false);
				else if (Constants.ModeIsENCODER (current_mode))
				{
					blankEncoderCurrentSetGraphs ();
					treeviewEncoderCaptureRemoveColumns ();

					if (treeViewResultsSession.EventSelectedID == TreeViewEvent.MarkNonSelectRowSubEvent)
						treeViewResultsSession.SelectEventHeaderLine();

					on_encoder_load_signal_accepted_do (treeViewResultsSession.EventSelectedID);
				}
				else //if current_mode == Constants.Modes.RUNSENCODER)
					runEncoderLoadSetDo (treeViewResultsSession.EventSelectedID, -1, currentSession.UniqueID, false);
			}

			showHideActionEventButtons(true); //show
			updateGraphResultsSessionByMode (); //to show the selected bar
		}

		// done here and on updateGraphJumpsSimple ()
		if (current_mode == Constants.Modes.JUMPSSIMPLE && treeViewResultsSession.EventSelectedID >= 0)
		{
			box_jump_simple_height.Visible = true;
			Jump myJump = SqliteJump.SelectJumpData (treeViewResultsSession.EventSelectedID, false);
			label_jump_simple_height_value.Text = Util.TrimDecimals (myJump.Height, 2) + " cm";
		} else
			box_jump_simple_height.Visible = false;

		// done here and on updateGraphRunsSimple ()
		if (current_mode == Constants.Modes.RUNSSIMPLE && treeViewResultsSession.EventSelectedID >= 0)
		{
			box_run_simple_time.Visible = true;
			Run myRun = SqliteRun.SelectRunData  (treeViewResultsSession.EventSelectedID, false);
			label_run_simple_time_value.Text = Util.TrimDecimals (myRun.Time, 2) + " s";
		} else
			box_run_simple_time.Visible = false;
	}

	private void selectResultsSessionId (int id, bool scroll)
	{
		treeViewResultsSession.ZoomToTestsIfNeeded ();

		// here we welect the event
		treeViewResultsSession.SelectEvent (id, scroll);

		// note this can change the person
		on_treeview_results_session_cursor_changed (new object (), new EventArgs ()); //in order to update the play video button
	}

	private void treeview_results_session_storeReset ()
	{
		if (treeViewResultsSession == null)
			return;

		treeViewResultsSession.RemoveColumns();

		//just to have following code shorter
		int pdn = preferences.digitsNumber;
		TreeViewEvent.ExpandStates expandState = treeViewResultsSession.ExpandState;

		if (current_mode == Constants.Modes.JUMPSSIMPLE)
			treeViewResultsSession = new TreeViewJumps (treeview_results_session, preferences, expandState, radio_resultsSession_jump_heights.Active);
		else if (current_mode == Constants.Modes.JUMPSREACTIVE)
			treeViewResultsSession = new TreeViewJumpsRj (treeview_results_session, preferences, expandState);
		else if (current_mode == Constants.Modes.RUNSSIMPLE)
			treeViewResultsSession = new TreeViewRuns (
					treeview_results_session, pdn, preferences.metersSecondsPreferred, expandState, radio_resultsSession_run_speeds.Active);
		else if (current_mode == Constants.Modes.RUNSINTERVALLIC)
			treeViewResultsSession = new TreeViewRunsInterval (
					treeview_results_session, pdn, preferences.metersSecondsPreferred, expandState, radio_resultsSession_run_speeds.Active);
		else if (current_mode == Constants.Modes.RUNSENCODER)
			treeViewResultsSession = new TreeViewRunEncoder (treeview_results_session, pdn, expandState);
		else if (current_mode == Constants.Modes.BEEPTEST)
			treeViewResultsSession = new TreeViewBeepTest (treeview_results_session, pdn, expandState);
		else if (Constants.ModeIsFORCESENSOR (current_mode))
			treeViewResultsSession = new TreeViewForceSensor (treeview_results_session, pdn, expandState);
		else if (Constants.ModeIsENCODER (current_mode))
			treeViewResultsSession = new TreeViewEncoder (treeview_results_session, pdn,
					current_mode == Constants.Modes.POWERGRAVITATORY, expandState);
		else if (current_mode == Constants.Modes.WILIGHT)
			treeViewResultsSession = new TreeViewWilight (treeview_results_session, pdn, expandState);
		else if (current_mode == Constants.Modes.OTHER)
			treeViewResultsSession = new TreeViewFourPlatforms (treeview_results_session, pdn, expandState);
	}

	private void on_treeview_results_session_button_release_event (object o, ButtonReleaseEventArgs args)
	{
		//LogB.Information ("on_treeview_results_session_button_release_event");
		Gdk.EventButton e = args.Event;
		//LogB.Information ("e.Button" + e.Button.ToString ());
		//LogB.Information ("EventSelectedID: " + treeViewResultsSession.EventSelectedID.ToString ());
		//Gtk.TreeView myTv = (Gtk.TreeView) o;
		if (e.Button != 3 || treeViewResultsSession.EventSelectedID < 0)
			return;

		/*
		 * On encoder right click it was disabled on encoder because when right click is done, first cursor_changed is raised
		 * and then it loads set
		 * then the button_release should be raised, but it is lost while loading the set
		 * A solution could be to not load the set if the user clicks to same row, but this can be inconsistent for the user
		 * note on cursor_changed with the EventArgs we do not know which button has been pressed
		 * easiest solution is to put an edit button for encoder, and use also the delete button for encoder
		 * (also great for tactile screens)
		 *
		 * finally on_treeview_results_session_cursor_changed checks if row is the same
		 * so user can left click on set or rep and the right click on set and edit/delete will shown
		 */

		treeviewResultsContextMenu (
				(current_mode == Constants.Modes.JUMPSREACTIVE || current_mode == Constants.Modes.RUNSINTERVALLIC), //hasRepair
				"");
	}

	private void treeviewResultsContextMenu (bool hasRepair, string label)
	{
		Menu myMenu = new Menu ();
		Gtk.MenuItem myItem;
		uint y = 0;

		myItem = new MenuItem (Catalog.GetString("Edit selected") + label);
		myItem.Activated += on_button_tests_edit_selected_clicked;
		myMenu.Attach( myItem, 0, 1, y, (y++)+1 );

		if (hasRepair)
		{
			myItem = new MenuItem (Catalog.GetString("Repair selected") + label);
			myItem.Activated += on_button_tests_repair_selected_clicked;
			myMenu.Attach( myItem, 0, 1, y, (y++)+1 );

			Gtk.SeparatorMenuItem mySep = new SeparatorMenuItem();
			myMenu.Attach( mySep, 0, 1, y, (y++)+1 );
		}

		myItem = new MenuItem (Catalog.GetString("Delete selected") + label);
		myItem.Activated += on_button_tests_delete_selected_clicked;
		myMenu.Attach( myItem, 0, 1, y, (y++)+1 );

		myMenu.ShowAll();
		myMenu.Popup();
	}

	// Used on open Chronojump and on load session.
	// Because at load session if these are not active, user will think it has lost his data
	private void treeviewResultsSessionAllPersonsAllTests ()
	{
		radio_contacts_graph_allTests.Active = true;
		radio_contacts_results_personAll.Active = true;
	}
}

