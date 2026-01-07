/*
 * This file is part of ChronoJump
 *
 * ChronoJump is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or   
 *    (at your option) any later version.
 *    
 * ChronoJump is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the 
 *    GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 *  along with this program; if not, write to the Free Software
 *   Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 *
 * Copyright (C) 2004-2026   Xavier de Blas <xaviblas@gmail.com>
 */

using System;
using Gtk;
//using Glade;
using System.Text; //StringBuilder
using System.Collections; //ArrayList
using System.Collections.Generic; //List<T>
using System.Threading;
using Mono.Unix;


public partial class ChronoJumpWindow
{
	// ----------------
	// ---- DELETE ----
	// ----------------

	private void on_delete_selected_run_clicked (object o, EventArgs args) {
		//notebooks_change(2); see "notebooks_change sqlite problem"
		LogB.Information("delete this race (simple)");
		
		//1.- check that there's a line selected
		//2.- check that this line is a jump and not a person
		if (treeViewResultsSession.EventSelectedID >= 0) {
			//3.- display confirmwindow of deletion 
			if (preferences.askDeletion) {
				confirmWinJumpRun = ConfirmWindowJumpRun.Show(Catalog.GetString("Do you want to delete this race?"), "");
				confirmWinJumpRun.Button_accept.Clicked += new EventHandler(on_delete_selected_run_accepted);
			} else {
				on_delete_selected_run_accepted(o, args);
			}
		}
	}
		
	
	private void on_delete_selected_run_interval_clicked (object o, EventArgs args) {
		//notebooks_change(3); see "notebooks_change sqlite problem"
		LogB.Information("delete this race interval");
		//1.- check that there's a line selected
		//2.- check that this line is a run and not a person (check also if it's a subrun, pass the parent run)
		if (treeViewResultsSession.EventSelectedID >= 0) {
			//3.- display confirmwindow of deletion 
			if (preferences.askDeletion) {
				confirmWinJumpRun = ConfirmWindowJumpRun.Show(
						Catalog.GetString("Do you want to delete this race?"), "");
				confirmWinJumpRun.Button_accept.Clicked += new EventHandler(on_delete_selected_run_interval_accepted);
			} else {
				on_delete_selected_run_interval_accepted(o, args);
			}
		}
	}

	private void on_delete_selected_run_accepted (object o, EventArgs args)
	{
		LogB.Information("accept delete this race");
		int id = treeViewResultsSession.EventSelectedID;
		
		Sqlite.Delete(false, Constants.RunTable, id);
		
		treeViewResultsSession.DelEvent(id);
		updatePersonTestsN (false);
		selectedRunInterval = null;
		selectedRunIntervalType = null;
		sensitiveSelectedTestButtons (false);
		button_inspect_last_test_run_simple.Sensitive = false;
		
		if(createdStatsWin) {
			stats_win_fillTreeView_stats(false, false);
		}
		Util.DeleteVideo(currentSession.UniqueID, Constants.TestTypes.RUN, id );
		try {
			if(currentRun.UniqueID == id)
				deleted_last_test_update_widgets();
		} catch {
			//there's no currentRun (no one done it now), then it crashed,
			//but don't need to update widgets
		}
		
		updateGraphRunsSimple();
	}

	private void on_delete_selected_run_interval_accepted (object o, EventArgs args)
	{
		LogB.Information("accept delete this race");
		int id = treeViewResultsSession.EventSelectedID;
		
		Sqlite.Delete(false, Constants.RunIntervalTable, id);
		
		treeViewResultsSession.DelEvent(id);
		updatePersonTestsN (false);
		selectedRunInterval = null;
		sensitiveSelectedTestButtons (false);
		button_inspect_last_test_run_intervallic.Sensitive = false;

		if(createdStatsWin) {
			stats_win_fillTreeView_stats(false, false);
		}
		Util.DeleteVideo(currentSession.UniqueID, Constants.TestTypes.RUN_I, id );
		try {
			if(currentRunInterval.UniqueID == id)
				deleted_last_test_update_widgets();
		} catch {
			//there's no currentRunInterval (no one done it now), then it crashed,
			//but don't need to update widgets
		}

		updateGraphRunsInterval();

		//blank also realtime graph
		blankRunIntervalRealtimeCaptureGraph ();
	}
}
