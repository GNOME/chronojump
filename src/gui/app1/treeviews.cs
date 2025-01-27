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
 * Copyright (C) 2004-2025   Xavier de Blas <xaviblas@gmail.com>
 */


using System;
using Gtk;
//using Glade;
using Mono.Unix;
using System.Collections; //ArrayList
using System.Collections.Generic; //List

public partial class ChronoJumpWindow 
{
	Gtk.TreeView treeview_jumps;
	Gtk.TreeView treeview_jumps_rj;
	Gtk.TreeView treeview_runs;
	Gtk.TreeView treeview_runs_interval;
	Gtk.TreeView treeview_runs_interval_sprint;
	
	
	/* ---------------------------------------------------------
	 * ----------------  TREEVIEW (generic) --------------------
	 *  --------------------------------------------------------
	 */

	private void expandOrMinimizeTreeView(TreeViewEvent tvEvent, TreeView tv) {
		if(tvEvent.ExpandState == TreeViewEvent.ExpandStates.MINIMIZED) 
			tv.CollapseAll();
		else if (tvEvent.ExpandState == TreeViewEvent.ExpandStates.OPTIMAL) {
			tv.CollapseAll();
			tvEvent.ExpandOptimal();
		} else   //MAXIMIZED
			tv.ExpandAll();

		//Log.WriteLine("IS " + tvEvent.ExpandState);
	}

	private void on_treeview_button_release_event (object o, ButtonReleaseEventArgs args)
	{
		Gdk.EventButton e = args.Event;
		Gtk.TreeView myTv = (Gtk.TreeView) o;
		if (e.Button == 3) {
			if(myTv == treeview_persons && currentPerson != null) {
				treeviewPersonsContextMenu(currentPerson);
			} else if(myTv == treeview_jumps) {
				if (myTreeViewJumps.EventSelectedID > 0) {
					Jump myJump = SqliteJump.SelectJumpData( myTreeViewJumps.EventSelectedID, false );
					treeviewJumpsContextMenu(myJump);
				}
			} else if(myTv == treeview_jumps_rj) {
				if (myTreeViewJumpsRj.EventSelectedID > 0) {
					JumpRj myJump = SqliteJumpRj.SelectJumpData( "jumpRj", myTreeViewJumpsRj.EventSelectedID, false, false );
					treeviewJumpsRjContextMenu(myJump);
				}
			} else if(myTv == treeview_runs) {
				if (myTreeViewRuns.EventSelectedID > 0) {
					Run myRun = SqliteRun.SelectRunData( myTreeViewRuns.EventSelectedID, false );
					treeviewRunsContextMenu(myRun);
				}
			} else if(myTv == treeview_runs_interval) {
				if (myTreeViewRunsInterval.EventSelectedID > 0) {
					RunInterval myRun = SqliteRunInterval.SelectRunData( Constants.RunIntervalTable, myTreeViewRunsInterval.EventSelectedID, false, false );
					treeviewRunsIntervalContextMenu(myRun);
				}
			} else
				LogB.Information(myTv.ToString());
		}
	}

	private void treeviewPersonsContextMenu(Person myPerson) {
		Menu myMenu = new Menu ();
		Gtk.MenuItem myItem;

		myItem = new MenuItem ( Catalog.GetString("Edit") + " " + myPerson.Name);
		myItem.Activated += on_edit_current_person_clicked_from_main_gui;
		myMenu.Attach( myItem, 0, 1, 0, 1 );

		myItem = new MenuItem ( Catalog.GetString("Show all tests of") + " " + myPerson.Name);
		myItem.Activated += on_show_all_person_events_activate;
		myMenu.Attach( myItem, 0, 1, 1, 2 );

		Gtk.SeparatorMenuItem mySep = new SeparatorMenuItem();
		myMenu.Attach( mySep, 0, 1, 2, 3 );

		myItem = new MenuItem ( string.Format(Catalog.GetString("Delete {0} from this session"),myPerson.Name));
		myItem.Activated += on_delete_current_person_from_session_clicked;
		myMenu.Attach( myItem, 0, 1, 3, 4 );

		myMenu.ShowAll();
		myMenu.Popup();
	}
		

	private void resetAllTreeViews(bool fillTests, bool resetPersons, bool fillPersons)
	{
		//persons
		if(resetPersons) {
			treeview_persons_storeReset();
			if(fillPersons)
				fillTreeView_persons();
		}

		//Leave SQL opened in all this process
		Sqlite.Open(); // ------------------------------

		if(fillTests) {
			pre_fillTreeView_jumps (true);
			pre_fillTreeView_jumps_rj (true);
			pre_fillTreeView_runs (true);
			pre_fillTreeView_runs_interval (true);
		}
		else {
			treeview_jumps_storeReset();
			treeview_jumps_rj_storeReset();
			treeview_runs_storeReset();
			treeview_runs_interval_storeReset();
		}

		//close SQL opened in all this process
		Sqlite.Close(); // ------------------------------
	}


	/* ---------------------------------------------------------
	 * ----------------  TREEVIEW JUMPS ------------------------
	 *  --------------------------------------------------------
	 */

	private void createTreeView_jumps (Gtk.TreeView tv) {
		//myTreeViewJumps is a TreeViewJumps instance
		myTreeViewJumps = new TreeViewJumps(tv, preferences, TreeViewEvent.ExpandStates.MINIMIZED);

		//the glade cursor_changed does not work on mono 1.2.5 windows
		tv.CursorChanged += on_treeview_jumps_cursor_changed; 
	}

	private void fillTreeView_jumps (string filter) {
		fillTreeView_jumps(filter, false);
	}
	private void fillTreeView_jumps (string filter, bool dbconOpened)
	{
		if (currentSession == null) {
			/*
			 * This happens when the user "Imports a session": Chronojump tries to
			 * update comboboxes, it reaches here because the comboboxes are updated
			 * But if the user didn't have any
			 * open session currentSession variable (see below) is null and it crashed here
			 * (when it did currentSession.UniqueID with currentSession==null)
			 */
			return;
		}

		string [] myJumps = SqliteJump.SelectJumpsSA (dbconOpened,
				currentSession.UniqueID, currentPersonOrAll (), "", "", Sqlite.Orders_by.DEFAULT, 0);

		myTreeViewJumps.Fill (myJumps, filter,
				Util.GetVideosOfSessionAndMode (currentSession.UniqueID, Constants.TestTypes.JUMP));

		//if show just one person, have it expanded
		if (! radio_contacts_results_personAll.Active && currentPerson != null)
			treeview_jumps.ExpandAll();
		else
			expandOrMinimizeTreeView((TreeViewEvent) myTreeViewJumps, treeview_jumps);
	}

	private int currentPersonOrAll ()
	{
		if (! radio_contacts_results_personAll.Active && currentPerson != null)
			return currentPerson.UniqueID;
		else
			return -1;
	}

	private void on_button_jumps_zoom_clicked (object o, EventArgs args) {
		myTreeViewJumps.ExpandState = myTreeViewJumps.ZoomChange(myTreeViewJumps.ExpandState);
		if(myTreeViewJumps.ExpandState == TreeViewEvent.ExpandStates.MINIMIZED) {
			treeview_jumps.CollapseAll();
			image_jumps_zoom.Pixbuf = Chronojump.MyPixbuf.Get(null, Util.GetImagePath(false) + "zoom_in.png");
		} else {
			treeview_jumps.ExpandAll();
			image_jumps_zoom.Pixbuf = Chronojump.MyPixbuf.Get(null, Util.GetImagePath(false) + "zoom_out.png");
		}
	}
	
	private void treeview_jumps_storeReset() {
		myTreeViewJumps.RemoveColumns();
		
		myTreeViewJumps = new TreeViewJumps(treeview_jumps, preferences, myTreeViewJumps.ExpandState);
	}

	// works for jumps/runs (to update some buttons like play video sensitivity)
	private void on_treeview_mode_cursor_changed ()
	{
		if (current_mode == Constants.Modes.JUMPSSIMPLE && myTreeViewJumps != null)
			on_treeview_jumps_cursor_changed (new object (), new EventArgs ());
		else if (current_mode == Constants.Modes.JUMPSREACTIVE && myTreeViewJumpsRj != null)
			on_treeview_jumps_rj_cursor_changed (new object (), new EventArgs ());
		else if (current_mode == Constants.Modes.RUNSSIMPLE && myTreeViewRuns != null)
			on_treeview_runs_cursor_changed (new object (), new EventArgs ());
		else if (current_mode == Constants.Modes.RUNSINTERVALLIC && myTreeViewRunsInterval != null)
			on_treeview_runs_interval_cursor_changed (new object (), new EventArgs ());
	}

	private void on_treeview_jumps_cursor_changed (object o, EventArgs args)
	{
		LogB.Information("Cursor changed");

		sensitiveLastTestButtons(false);

		// don't select if it's a person, 
		// is for not confusing with the person treeviews that controls who jumps
		if (myTreeViewJumps.EventSelectedID == 0) {
			myTreeViewJumps.Unselect();
			showHideActionEventButtons(false); //hide
		} else {
			showHideActionEventButtons(true); //show
			updateGraphJumpsSimple (); //to show the selected bar
		}
	}

	private void treeviewJumpsContextMenu(Jump myJump) {
		Menu myMenu = new Menu ();
		Gtk.MenuItem myItem;

		myItem = new MenuItem ( Catalog.GetString("Edit selected") + " " + myJump.Type + " (" + myJump.PersonName + ")");
		myItem.Activated += on_edit_selected_jump_clicked;
		myMenu.Attach( myItem, 0, 1, 0, 1 );
	
		Gtk.SeparatorMenuItem mySep = new SeparatorMenuItem();
		myMenu.Attach( mySep, 0, 1, 1, 2 );

		myItem = new MenuItem ( Catalog.GetString("Delete selected") + " " + myJump.Type + " (" + myJump.PersonName + ")");
		myItem.Activated += on_delete_selected_jump_clicked;
		myMenu.Attach( myItem, 0, 1, 2, 3 );

		myMenu.ShowAll();
		myMenu.Popup();
	}

	/* ---------------------------------------------------------
	 * ----------------  TREEVIEW JUMPS RJ ---------------------
	 *  --------------------------------------------------------
	 */

	private void createTreeView_jumps_rj (Gtk.TreeView tv) {
		myTreeViewJumpsRj = new TreeViewJumpsRj (tv, preferences, TreeViewEvent.ExpandStates.MINIMIZED);

		//the glade cursor_changed does not work on mono 1.2.5 windows
		tv.CursorChanged += on_treeview_jumps_rj_cursor_changed; 
	}

	private void fillTreeView_jumps_rj (string filter) {
		fillTreeView_jumps_rj (filter, false);
	}
	private void fillTreeView_jumps_rj (string filter, bool dbconOpened)
	{
		if (currentSession == null) {
			/*
			 * This happens when the user "Imports a session": Chronojump tries to
			 * update comboboxes, it reaches here because the comboboxes are updated
			 * But if the user didn't have any
			 * open session currentSession variable (see below) is null and it crashed here
			 * (when it did currentSession.UniqueID with currentSession==null)
			 */
			return;
		}

		string [] myJumps = SqliteJumpRj.SelectJumpsSA (dbconOpened,
				currentSession.UniqueID, currentPersonOrAll (), "", "");
		myTreeViewJumpsRj.Fill (myJumps, filter,
				Util.GetVideosOfSessionAndMode (currentSession.UniqueID, Constants.TestTypes.JUMP_RJ));

		//if show just one person, have it expanded (optimal)
		if (! radio_contacts_results_personAll.Active && currentPerson != null)
		{
			treeview_jumps_rj.CollapseAll ();
			((TreeViewEvent) myTreeViewJumpsRj).ExpandOptimal();
		} else
			expandOrMinimizeTreeView((TreeViewEvent) myTreeViewJumpsRj, treeview_jumps_rj);
	}

	private void on_button_jumps_rj_zoom_clicked (object o, EventArgs args) {
		myTreeViewJumpsRj.ExpandState = myTreeViewJumpsRj.ZoomChange(myTreeViewJumpsRj.ExpandState);
		if(myTreeViewJumpsRj.ExpandState == TreeViewEvent.ExpandStates.MINIMIZED) {
			treeview_jumps_rj.CollapseAll();
			image_jumps_rj_zoom.Pixbuf = Chronojump.MyPixbuf.Get(null, Util.GetImagePath(false) + "zoom_in.png");
		} else if(myTreeViewJumpsRj.ExpandState == TreeViewEvent.ExpandStates.OPTIMAL) {
			treeview_jumps_rj.CollapseAll();
			myTreeViewJumpsRj.ExpandOptimal();
			image_jumps_rj_zoom.Pixbuf = Chronojump.MyPixbuf.Get(null, Util.GetImagePath(false) + "zoom_in.png");
		} else {
			treeview_jumps_rj.ExpandAll();
			image_jumps_rj_zoom.Pixbuf = Chronojump.MyPixbuf.Get(null, Util.GetImagePath(false) + "zoom_out.png");
		}
	}

	private void treeview_jumps_rj_storeReset() {
		myTreeViewJumpsRj.RemoveColumns();
		myTreeViewJumpsRj = new TreeViewJumpsRj (treeview_jumps_rj, preferences, myTreeViewJumpsRj.ExpandState);
	}

	private void on_treeview_jumps_rj_cursor_changed (object o, EventArgs args)
	{
		sensitiveLastTestButtons(false);

		// don't select if it's a person, 
		// is for not confusing with the person treeviews that controls who jumps
		if (myTreeViewJumpsRj.EventSelectedID == 0) {
			myTreeViewJumpsRj.Unselect();
			showHideActionEventButtons(false);
			return;
		}

		if (myTreeViewJumpsRj.EventSelectedID == -1)
			myTreeViewJumpsRj.SelectHeaderLine();

		showHideActionEventButtons(true);

		//graph the jump on realtime cairo graph. Using selectedJumpRj to avoid SQL select continuously
		if(selectedJumpRj == null || selectedJumpRj.UniqueID != myTreeViewJumpsRj.EventSelectedID)
			selectedJumpRj = SqliteJumpRj.SelectJumpData("jumpRj", myTreeViewJumpsRj.EventSelectedID, true, false); //true: personNameInComment

		updateGraphJumpsReactive (); //to show the selected bar

		blankJumpReactiveRealtimeCaptureGraph ();
		PrepareJumpReactiveRealtimeCaptureGraph (selectedJumpRj.tvLast, selectedJumpRj.tcLast,
				selectedJumpRj.TvString, selectedJumpRj.TcString,
				selectedJumpRj.Type, selectedJumpRj.Description, //Description is personName
				preferences.volumeOn, preferences.gstreamer, feedbackJumpsRj,
				preferences.heightPreferred);
		event_execute_drawingarea_realtime_capture_cairo.QueueDraw ();
	}

	private void treeviewJumpsRjContextMenu(JumpRj myJump) {
		Menu myMenu = new Menu ();
		Gtk.MenuItem myItem;

		myItem = new MenuItem ( Catalog.GetString("Edit selected") + " " + myJump.Type + " (" + myJump.PersonName + ")");
		myItem.Activated += on_edit_selected_jump_rj_clicked;
		myMenu.Attach( myItem, 0, 1, 0, 1 );

		myItem = new MenuItem ( Catalog.GetString("Repair selected") + " " + myJump.Type + " (" + myJump.PersonName + ")");
		myItem.Activated += on_repair_selected_jump_rj_clicked;
		myMenu.Attach( myItem, 0, 1, 1, 2 );
		
		Gtk.SeparatorMenuItem mySep = new SeparatorMenuItem();
		myMenu.Attach( mySep, 0, 1, 2, 3 );

		myItem = new MenuItem ( Catalog.GetString("Delete selected") + " " + myJump.Type + " (" + myJump.PersonName + ")");
		myItem.Activated += on_delete_selected_jump_rj_clicked;
		myMenu.Attach( myItem, 0, 1, 3, 4 );

		myMenu.ShowAll();
		myMenu.Popup();
	}

	/* ---------------------------------------------------------
	 * ----------------  TREEVIEW RUNS -------------------------
	 *  --------------------------------------------------------
	 */

	private void createTreeView_runs (Gtk.TreeView tv) {
		//myTreeViewRuns is a TreeViewRuns instance
		myTreeViewRuns = new TreeViewRuns (tv, preferences.digitsNumber, preferences.metersSecondsPreferred, TreeViewEvent.ExpandStates.MINIMIZED );

		//the glade cursor_changed does not work on mono 1.2.5 windows
		tv.CursorChanged += on_treeview_runs_cursor_changed; 
	}

	private void fillTreeView_runs (string filter) {
		fillTreeView_runs (filter, false);
	}
	private void fillTreeView_runs (string filter, bool dbconOpened)
	{
		if (currentSession == null) {
			/*
			 * This happens when the user "Imports a session": Chronojump tries to
			 * update comboboxes, it reaches here because the comboboxes are updated
			 * But if the user didn't have any
			 * open session currentSession variable (see below) is null and it crashed here
			 * (when it did currentSession.UniqueID with currentSession==null)
			 */
			return;
		}

		string [] myRuns = SqliteRun.SelectRunsSA (dbconOpened,
				currentSession.UniqueID, currentPersonOrAll (), "", Sqlite.Orders_by.DEFAULT, 0);

		myTreeViewRuns.Fill(myRuns, filter,
				Util.GetVideosOfSessionAndMode (currentSession.UniqueID, Constants.TestTypes.RUN));

		//if show just one person, have it expanded
		if (! radio_contacts_results_personAll.Active && currentPerson != null)
			treeview_runs.ExpandAll();
		else
			expandOrMinimizeTreeView((TreeViewEvent) myTreeViewRuns, treeview_runs);
	}
	
	private void on_button_runs_zoom_clicked (object o, EventArgs args) {
		myTreeViewRuns.ExpandState = myTreeViewRuns.ZoomChange(myTreeViewRuns.ExpandState);
		if(myTreeViewRuns.ExpandState == TreeViewEvent.ExpandStates.MINIMIZED) {
			treeview_runs.CollapseAll();
			image_runs_zoom.Pixbuf = Chronojump.MyPixbuf.Get(null, Util.GetImagePath(false) + "zoom_in.png");
		} else {
			treeview_runs.ExpandAll();
			image_runs_zoom.Pixbuf = Chronojump.MyPixbuf.Get(null, Util.GetImagePath(false) + "zoom_out.png");
		}
	}
	
	private void treeview_runs_storeReset() {
		myTreeViewRuns.RemoveColumns();
		myTreeViewRuns = new TreeViewRuns(treeview_runs, preferences.digitsNumber, preferences.metersSecondsPreferred, myTreeViewRuns.ExpandState);
	}

	private void on_treeview_runs_cursor_changed (object o, EventArgs args)
	{
		sensitiveLastTestButtons(false);
		button_inspect_last_test_run_simple.Sensitive = false;

		// don't select if it's a person, 
		// is for not confusing with the person treeviews that controls who runs
		if (myTreeViewRuns.EventSelectedID == 0) {
			myTreeViewRuns.Unselect();
			showHideActionEventButtons(false);
		} else {
			showHideActionEventButtons(true);
			updateGraphRunsSimple (); //to show the selected bar
		}
	}

	private void treeviewRunsContextMenu(Run myRun) {
		Menu myMenu = new Menu ();
		Gtk.MenuItem myItem;

		myItem = new MenuItem ( Catalog.GetString("Edit selected") + " " + myRun.Type + " (" + myRun.PersonName + ")");
		myItem.Activated += on_edit_selected_run_clicked;
		myMenu.Attach( myItem, 0, 1, 0, 1 );

		Gtk.SeparatorMenuItem mySep = new SeparatorMenuItem();
		myMenu.Attach( mySep, 0, 1, 1, 2 );

		myItem = new MenuItem ( Catalog.GetString("Delete selected") + " " + myRun.Type + " (" + myRun.PersonName + ")");
		myItem.Activated += on_delete_selected_run_clicked;
		myMenu.Attach( myItem, 0, 1, 2, 3 );

		myMenu.ShowAll();
		myMenu.Popup();
	}

	/* ---------------------------------------------------------
	 * ----------------  TREEVIEW RUN INTERVAL -----------------
	 *  --------------------------------------------------------
	 */

	private void createTreeView_runs_interval (Gtk.TreeView tv) {
		//myTreeViewRunsInterval is a TreeViewRunsInterval instance
		myTreeViewRunsInterval = new TreeViewRunsInterval (tv, preferences.digitsNumber, preferences.metersSecondsPreferred, TreeViewEvent.ExpandStates.MINIMIZED);

		//the glade cursor_changed does not work on mono 1.2.5 windows
		tv.CursorChanged += on_treeview_runs_interval_cursor_changed; 
	}

	private void fillTreeView_runs_interval (string filter) {
		fillTreeView_runs_interval (filter, false);
	}
	private void fillTreeView_runs_interval (string filter, bool dbconOpened)
	{
		if (currentSession == null) {
			/*
			 * This happens when the user "Imports a session": Chronojump tries to
			 * update comboboxes, it reaches here because the comboboxes are updated
			 * But if the user didn't have any
			 * open session currentSession variable (see below) is null and it crashed here
			 * (when it did currentSession.UniqueID with currentSession==null)
			 */
			return;
		}

		string [] myRuns = SqliteRunInterval.SelectRunsSA (dbconOpened,
				currentSession.UniqueID, currentPersonOrAll (), "");
		myTreeViewRunsInterval.Fill(myRuns, filter,
				Util.GetVideosOfSessionAndMode (currentSession.UniqueID, Constants.TestTypes.RUN_I));

		//if show just one person, have it expanded (optimal)
		if (! radio_contacts_results_personAll.Active && currentPerson != null)
		{
			treeview_runs_interval.CollapseAll ();
			((TreeViewEvent) myTreeViewRunsInterval).ExpandOptimal();
		} else
		expandOrMinimizeTreeView((TreeViewEvent) myTreeViewRunsInterval, treeview_runs_interval);
	}
	
	private void on_button_runs_interval_zoom_clicked (object o, EventArgs args) {
		myTreeViewRunsInterval.ExpandState = myTreeViewRunsInterval.ZoomChange(myTreeViewRunsInterval.ExpandState);
		if(myTreeViewRunsInterval.ExpandState == TreeViewEvent.ExpandStates.MINIMIZED) {
			treeview_runs_interval.CollapseAll();
			image_runs_interval_zoom.Pixbuf = Chronojump.MyPixbuf.Get(null, Util.GetImagePath(false) + "zoom_in.png");
		} else if(myTreeViewRunsInterval.ExpandState == TreeViewEvent.ExpandStates.OPTIMAL) {
			treeview_runs_interval.CollapseAll();
			myTreeViewRunsInterval.ExpandOptimal();
			image_runs_interval_zoom.Pixbuf = Chronojump.MyPixbuf.Get(null, Util.GetImagePath(false) + "zoom_in.png");
		} else {
			treeview_runs_interval.ExpandAll();
			image_runs_interval_zoom.Pixbuf = Chronojump.MyPixbuf.Get(null, Util.GetImagePath(false) + "zoom_out.png");
		}
	}

	private void treeview_runs_interval_storeReset() {
		myTreeViewRunsInterval.RemoveColumns();
		myTreeViewRunsInterval = new TreeViewRunsInterval (treeview_runs_interval,  
				preferences.digitsNumber, preferences.metersSecondsPreferred, myTreeViewRunsInterval.ExpandState);
	}

	private void on_treeview_runs_interval_cursor_changed (object o, EventArgs args)
	{
		sensitiveLastTestButtons(false);

		// don't select if it's a person, 
		// is for not confusing with the person treeviews that controls who runs
		if (myTreeViewRunsInterval.EventSelectedID == 0) {
			myTreeViewRunsInterval.Unselect();
			showHideActionEventButtons(false);
			return;
		}

		if (myTreeViewRunsInterval.EventSelectedID == -1)
			myTreeViewRunsInterval.SelectHeaderLine();

		showHideActionEventButtons(true);
		button_inspect_last_test_run_intervallic.Sensitive = false;

		//graph the run on realtime cairo graph. Using selectedRunInterval to avoid SQL select continuously
		if(selectedRunInterval == null || selectedRunIntervalType == null ||
				selectedRunInterval.UniqueID != myTreeViewRunsInterval.EventSelectedID)
		{
			selectedRunInterval = SqliteRunInterval.SelectRunData (Constants.RunIntervalTable, myTreeViewRunsInterval.EventSelectedID, true, false);
			selectedRunIntervalType = SqliteRunIntervalType.SelectAndReturnRunIntervalType(selectedRunInterval.Type, false);

			/*
			LogB.Information("selectedRunInterval: " + selectedRunInterval.ToString());
			LogB.Information("selectedRunIntervalType: " + selectedRunIntervalType.ToString());
			*/
		}

		updateGraphRunsInterval (); //to show the selected bar

		blankRunIntervalRealtimeCaptureGraph ();
		PrepareRunIntervalRealtimeCaptureGraph (
				selectedRunInterval.IntervalTimesString,
				selectedRunInterval.DistanceInterval,
				selectedRunIntervalType.DistancesString,
				selectedRunInterval.Photocell_l,
				selectedRunInterval.Type, selectedRunInterval.Description, feedbackRunsI); //Description is personName
		event_execute_drawingarea_realtime_capture_cairo.QueueDraw ();
	}

	private void treeviewRunsIntervalContextMenu(RunInterval myRun) {
		Menu myMenu = new Menu ();
		Gtk.MenuItem myItem;

		myItem = new MenuItem ( Catalog.GetString("Edit selected") + " " + myRun.Type + " (" + myRun.PersonName + ")");
		myItem.Activated += on_edit_selected_run_interval_clicked;
		myMenu.Attach( myItem, 0, 1, 0, 1 );

		myItem = new MenuItem ( Catalog.GetString("Repair selected") + " " + myRun.Type + " (" + myRun.PersonName + ")");
		myItem.Activated += on_repair_selected_run_interval_clicked;
		myMenu.Attach( myItem, 0, 1, 1, 2 );
		
		Gtk.SeparatorMenuItem mySep = new SeparatorMenuItem();
		myMenu.Attach( mySep, 0, 1, 2, 3 );

		myItem = new MenuItem ( Catalog.GetString("Delete selected") + " " + myRun.Type + " (" + myRun.PersonName + ")");
		myItem.Activated += on_delete_selected_run_interval_clicked;
		myMenu.Attach( myItem, 0, 1, 3, 4 );

		myMenu.ShowAll();
		myMenu.Popup();
	}

}

