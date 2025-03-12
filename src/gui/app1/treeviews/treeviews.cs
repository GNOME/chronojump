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
	/* ---------------------------------------------------------
	 * ----------------  TREEVIEW (generic) --------------------
	 *  --------------------------------------------------------
	 */

	private void expandOrMinimizeTreeView(TreeViewEvent tvEvent, TreeView tv)
	{
		if(tvEvent.ExpandState == TreeViewEvent.ExpandStates.MINIMIZED) 
			tv.CollapseAll();
		else if (tvEvent.ExpandState == TreeViewEvent.ExpandStates.OPTIMAL) {
			tv.CollapseAll();
			tvEvent.ExpandOptimal();
		} else   //MAXIMIZED
			tv.ExpandAll();

		//Log.WriteLine("IS " + tvEvent.ExpandState);
	}

	private void on_treeview_results_session_button_release_event (object o, ButtonReleaseEventArgs args)
	{
		Gdk.EventButton e = args.Event;
		//Gtk.TreeView myTv = (Gtk.TreeView) o;
		if (e.Button != 3 || treeViewResultsSession.EventSelectedID <= 0)
			return;

		if (current_mode == Constants.Modes.JUMPSSIMPLE)
		{
			Jump myJump = SqliteJump.SelectJumpData (treeViewResultsSession.EventSelectedID, false );
			treeviewJumpsContextMenu (myJump);
		}
		else if (current_mode == Constants.Modes.JUMPSREACTIVE)
		{
			JumpRj myJump = SqliteJumpRj.SelectJumpData ("jumpRj", treeViewResultsSession.EventSelectedID, false, false);
			treeviewJumpsRjContextMenu (myJump);
		}
		else if (current_mode == Constants.Modes.RUNSSIMPLE)
		{
			Run myRun = SqliteRun.SelectRunData (treeViewResultsSession.EventSelectedID, false);
			treeviewRunsContextMenu (myRun);
		}
		else if (current_mode == Constants.Modes.RUNSINTERVALLIC)
		{
			RunInterval myRun = SqliteRunInterval.SelectRunData (Constants.RunIntervalTable, treeViewResultsSession.EventSelectedID, false, false);
			treeviewRunsIntervalContextMenu (myRun);
		}
		else if (current_mode == Constants.Modes.WILIGHT)
		{
			Wilight wilight = SqliteWilight.SelectData (treeViewResultsSession.EventSelectedID, false);
			treeviewWilightContextMenu (wilight);
		}
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

		if(fillTests)
			pre_fillTreeView_resultsSession (true);
		else
			treeview_results_session_storeReset();

		//close SQL opened in all this process
		Sqlite.Close(); // ------------------------------
	}

	private void on_treeview_test_simple_cursor_changed (object o, EventArgs args)
	{
		sensitiveLastTestButtons(false);

		// don't select if it's a person
		// is for not confusing with the person treeviews that controls who does the test
		if (treeViewResultsSession.EventSelectedID == 0) {
			treeViewResultsSession.Unselect();
			showHideActionEventButtons(false); //hide
		} else {
			showHideActionEventButtons(true); //show
			updateGraphResultsSessionByMode (); //to show the selected bar
		}
	}

	private int currentPersonOrAll ()
	{
		if (! radio_contacts_results_personAll.Active && currentPerson != null)
			return currentPerson.UniqueID;
		else
			return -1;
	}

}

