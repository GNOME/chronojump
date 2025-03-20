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
	Gtk.TreeView treeview_results_session;
	// <---- at glade

	private TreeViewEvent treeViewResultsSession;

	private void createTreeView_resultsSession (Gtk.TreeView tv)
	{
		LogB.Information ("createTreeView_resultsSession mode = " + current_mode.ToString ());
		if (current_mode == Constants.Modes.JUMPSSIMPLE)
			treeViewResultsSession = new TreeViewJumps (tv, preferences, TreeViewEvent.ExpandStates.MINIMIZED);
		else if (current_mode == Constants.Modes.JUMPSREACTIVE)
			treeViewResultsSession = new TreeViewJumpsRj (tv, preferences, TreeViewEvent.ExpandStates.MINIMIZED);
		else if (current_mode == Constants.Modes.RUNSSIMPLE)
			treeViewResultsSession = new TreeViewRuns (tv, preferences.digitsNumber, preferences.metersSecondsPreferred, TreeViewEvent.ExpandStates.MINIMIZED );
		else if (current_mode == Constants.Modes.RUNSINTERVALLIC)
			treeViewResultsSession = new TreeViewRunsInterval (tv, preferences.digitsNumber, preferences.metersSecondsPreferred, TreeViewEvent.ExpandStates.MINIMIZED);
		else if (current_mode == Constants.Modes.WILIGHT)
			treeViewResultsSession = new TreeViewWilight (tv, preferences.digitsNumber, TreeViewEvent.ExpandStates.MINIMIZED );
		else if (current_mode == Constants.Modes.OTHER)
			treeViewResultsSession = new TreeViewFourPlatforms (tv, preferences.digitsNumber, TreeViewEvent.ExpandStates.MINIMIZED );
		else
			treeViewResultsSession = new TreeViewJumps (tv, preferences, TreeViewEvent.ExpandStates.MINIMIZED); //default to fix any temporary crash at start (seems there is a personChanged but still not mode)

		//the glade cursor_changed does not work on mono 1.2.5 windows
		tv.CursorChanged -= on_treeview_results_session_cursor_changed;
		tv.CursorChanged += on_treeview_results_session_cursor_changed;
	}

	private void on_button_results_session_zoom_clicked (object o, EventArgs args)
	{
		treeViewResultsSession.ZoomChange (image_results_session_zoom);
	}

	private void on_treeview_results_session_cursor_changed (object o, EventArgs args)
	{
		LogB.Information ("on_treeview_results_session_cursor_changed");
		if (treeViewResultsSession == null)
			return;

		if (current_mode == Constants.Modes.JUMPSSIMPLE ||
				current_mode == Constants.Modes.RUNSSIMPLE ||
				current_mode == Constants.Modes.WILIGHT ||
				current_mode == Constants.Modes.OTHER) 	//FOURPLATFORMS
		{
			on_treeview_test_simple_cursor_changed (o, args);
		}
		else if (current_mode == Constants.Modes.JUMPSREACTIVE)
		{
			on_treeview_jumps_rj_cursor_changed (o, args);
		}
		else if (current_mode == Constants.Modes.RUNSINTERVALLIC)
		{
			on_treeview_runs_interval_cursor_changed (o, args);
		}
	}

	private void selectResultsSessionId (int id)
	{
		treeViewResultsSession.ZoomToTestsIfNeeded ();
		treeViewResultsSession.SelectEvent (id, true); //scroll

		on_treeview_results_session_cursor_changed (new object (), new EventArgs ()); //in order to update the play video button
	}

	private void treeview_results_session_storeReset ()
	{
		if (treeViewResultsSession == null)
			return;

		treeViewResultsSession.RemoveColumns();

		if (current_mode == Constants.Modes.JUMPSSIMPLE)
			treeViewResultsSession = new TreeViewJumps (
					treeview_results_session, preferences, treeViewResultsSession.ExpandState);
		else if (current_mode == Constants.Modes.JUMPSREACTIVE)
			treeViewResultsSession = new TreeViewJumpsRj (
					treeview_results_session, preferences, treeViewResultsSession.ExpandState);
		else if (current_mode == Constants.Modes.RUNSSIMPLE)
			treeViewResultsSession = new TreeViewRuns (
					treeview_results_session, preferences.digitsNumber, preferences.metersSecondsPreferred, treeViewResultsSession.ExpandState);
		else if (current_mode == Constants.Modes.RUNSINTERVALLIC)
			treeViewResultsSession = new TreeViewRunsInterval (
					treeview_results_session, preferences.digitsNumber, preferences.metersSecondsPreferred, treeViewResultsSession.ExpandState);
		else if (current_mode == Constants.Modes.WILIGHT)
			treeViewResultsSession = new TreeViewWilight (
					treeview_results_session, preferences.digitsNumber, treeViewResultsSession.ExpandState);
		else if (current_mode == Constants.Modes.OTHER)
			treeViewResultsSession = new TreeViewFourPlatforms (
					treeview_results_session, preferences.digitsNumber, treeViewResultsSession.ExpandState);
	}

	private void on_treeview_results_session_button_release_event (object o, ButtonReleaseEventArgs args)
	{
		Gdk.EventButton e = args.Event;
		//Gtk.TreeView myTv = (Gtk.TreeView) o;
		if (e.Button != 3 || treeViewResultsSession.EventSelectedID <= 0)
			return;

		Event ev;
		if (current_mode == Constants.Modes.JUMPSSIMPLE)
		{
			ev = SqliteJump.SelectJumpData (
					treeViewResultsSession.EventSelectedID, false );
			treeviewResultsContextMenu (false, " " + ev.Type + " (" + ev.PersonName + ")");
		}
		else if (current_mode == Constants.Modes.JUMPSREACTIVE)
		{
			ev = SqliteJumpRj.SelectJumpData (
					"jumpRj", treeViewResultsSession.EventSelectedID, false, false);
			treeviewResultsContextMenu (true, " " + ev.Type + " (" + ev.PersonName + ")");
		}
		else if (current_mode == Constants.Modes.RUNSSIMPLE)
		{
			ev = SqliteRun.SelectRunData (treeViewResultsSession.EventSelectedID, false);
			treeviewResultsContextMenu (false, " " + ev.Type + " (" + ev.PersonName + ")");
		}
		else if (current_mode == Constants.Modes.RUNSINTERVALLIC)
		{
			ev = SqliteRunInterval.SelectRunData (
					Constants.RunIntervalTable, treeViewResultsSession.EventSelectedID, false, false);
			treeviewResultsContextMenu (true, " " + ev.Type + " (" + ev.PersonName + ")");
		}
		else if (current_mode == Constants.Modes.WILIGHT)
		{
			ev = SqliteWilight.SelectData (
					treeViewResultsSession.EventSelectedID, false);
			treeviewResultsContextMenu (false, " (" + ev.PersonName + ")");
		}
		else if (current_mode == Constants.Modes.OTHER) //FOURPLATFORMS
		{
			/*
			ev = SqliteFourPlatforms.SelectData (
					treeViewResultsSession.EventSelectedID, false);
			treeviewResultsContextMenu (false, " (" + ev.PersonName + ")");
			*/
		}
	}

	private void treeviewResultsContextMenu (bool hasRepair, string label)
	{
		Menu myMenu = new Menu ();
		Gtk.MenuItem myItem;
		uint y = 0;

		myItem = new MenuItem (Catalog.GetString("Edit selected") + label);
		myItem.Activated += on_button_contacts_edit_selected_clicked;
		myMenu.Attach( myItem, 0, 1, y, (y++)+1 );

		if (hasRepair)
		{
			myItem = new MenuItem (Catalog.GetString("Repair selected") + label);
			myItem.Activated += on_button_contacts_repair_selected_clicked;
			myMenu.Attach( myItem, 0, 1, y, (y++)+1 );

			Gtk.SeparatorMenuItem mySep = new SeparatorMenuItem();
			myMenu.Attach( mySep, 0, 1, y, (y++)+1 );
		}

		myItem = new MenuItem (Catalog.GetString("Delete selected") + label);
		myItem.Activated += on_button_contacts_delete_selected_clicked;
		myMenu.Attach( myItem, 0, 1, y, (y++)+1 );

		myMenu.ShowAll();
		myMenu.Popup();
	}

}

