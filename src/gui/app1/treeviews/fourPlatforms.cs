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
//using Glade;
using System.Collections; //ArrayList
using System.Collections.Generic; //List

public partial class ChronoJumpWindow 
{
	private void fillTreeView_fourPlatforms (string filter)
	{
		fillTreeView_fourPlatforms (filter, false);
	}
	private void fillTreeView_fourPlatforms (string filter, bool dbconOpened)
	{
		LogB.Information ("fillTreeView_fourPlatforms start");
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

		string [] myValues = SqliteFourPlatforms.SelectSA (dbconOpened,
				currentSession.UniqueID, currentPersonOrAll (),
				//"",
				Sqlite.Orders_by.DEFAULT, 0);
		if (current_mode == Constants.Modes.OTHER)
		{
			LogB.Information ("calling treeViewResultsSession.Fill");
			treeViewResultsSession.Fill (myValues, filter,
					Util.GetVideosOfSessionAndMode (currentSession.UniqueID, Constants.TestTypes.FOURPLATFORMS));
		}

		//if show just one person, have it expanded (optimal)
		if (! radio_contacts_results_personAll.Active && currentPerson != null)
		{
			treeview_results_session.CollapseAll ();
			((TreeViewEvent) treeViewResultsSession).ExpandOptimal();
		} else
			expandOrMinimizeTreeView((TreeViewEvent) treeViewResultsSession, treeview_results_session);
	}
	
	private void on_treeview_fourPlatforms_cursor_changed (object o, EventArgs args)
	{
		sensitiveLastTestButtons(false);

		// don't select if it's a person, 
		// is for not confusing with the person treeviews that controls who runs
		if (treeViewResultsSession.EventSelectedID == 0) {
			treeViewResultsSession.Unselect();
			showHideActionEventButtons(false);
			return;
		}

		if (treeViewResultsSession.EventSelectedID == -1)
			treeViewResultsSession.SelectHeaderLine();

		showHideActionEventButtons(true);

		//graph the run on realtime cairo graph. Using selectedRunInterval to avoid SQL select continuously
		if(selectedFourPlatforms == null || //selectedFourPlatformsType == null ||
				selectedFourPlatforms.UniqueID != treeViewResultsSession.EventSelectedID)
		{
			selectedFourPlatforms = SqliteFourPlatforms.SelectData (treeViewResultsSession.EventSelectedID, false);
			//selectedRunIntervalType = SqliteRunIntervalType.SelectAndReturnRunIntervalType(selectedRunInterval.Type, false); //TODO: with fourPlatforms (when there are types)

			//LogB.Information("selectedFourPlatforms: " + selectedFourPlatforms.ToString());
		}

		/*
		 * TODO:
		updateGraphFourPlatforms (); //to show the selected bar //TODO

		blankRunIntervalRealtimeCaptureGraph ();
		PrepareRunIntervalRealtimeCaptureGraph (
				selectedRunInterval.IntervalTimesString,
				selectedRunInterval.DistanceInterval,
				selectedRunIntervalType.DistancesString,
				selectedRunInterval.Photocell_l,
				selectedRunInterval.Type, selectedRunInterval.Description, feedbackRunsI); //Description is personName
		drawingarea_results_realtime.QueueDraw ();
		*/
	}
}
