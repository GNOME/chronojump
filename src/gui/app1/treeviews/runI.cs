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
	Gtk.TreeView treeview_runs_interval;

	private void createTreeView_runs_interval (Gtk.TreeView tv)
	{
		//myTreeViewRunsInterval is a TreeViewRunsInterval instance
		myTreeViewRunsInterval = new TreeViewRunsInterval (tv, preferences.digitsNumber, preferences.metersSecondsPreferred, TreeViewEvent.ExpandStates.MINIMIZED);

		//the glade cursor_changed does not work on mono 1.2.5 windows
		tv.CursorChanged += on_treeview_runs_interval_cursor_changed;
	}

	private void fillTreeView_runs_interval (string filter)
	{
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
	
	private void on_button_runs_interval_zoom_clicked (object o, EventArgs args)
	{
		myTreeViewRunsInterval.ZoomChange (image_runs_interval_zoom);
	}

	private void treeview_runs_interval_storeReset()
	{
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
		drawingarea_results_realtime.QueueDraw ();
	}

	private void selectRunIntervallic (int id)
	{
		myTreeViewRunsInterval.ZoomToTestsIfNeeded ();
		myTreeViewRunsInterval.SelectEvent (id, true); //scroll
		on_treeview_runs_interval_cursor_changed (new object (), new EventArgs ()); //in order to update top graph and play video button
	}

	private void treeviewRunsIntervalContextMenu(RunInterval myRun)
	{
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
