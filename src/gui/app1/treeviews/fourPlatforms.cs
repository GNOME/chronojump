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
 * Copyright (C) 2026   Xavier de Blas <xaviblas@gmail.com>
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

		SqliteTests sqliteTests = new SqliteFourPlatforms ();
		string [] myValues = sqliteTests.SelectSA (dbconOpened,
				currentSession.UniqueID, currentPersonOrAll (),
				//"",
				false, "",
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

		// every time fillTreeView is done, update this
		updatePersonTestsN (dbconOpened);
	}

	private void on_treeview_fourPlatforms_cursor_changed (object o, EventArgs args)
	{
		sensitiveSelectedTestButtons (false);

		// don't select if it's a person,
		// is for not confusing with the person treeviews that controls who runs
		if (treeViewResultsSession.EventSelectedID == TreeViewEvent.MarkRowIsPerson)
		{
			sensitiveSelectedTestButtons (false);
			blankFourPlatformsGraphs ();
			return;
		}

		if (treeViewResultsSession.EventSelectedID == TreeViewEvent.MarkNonSelectRowSubEvent)
			treeViewResultsSession.SelectEventHeaderLine();

		sensitiveSelectedTestButtons (true);

		//graph the run on realtime cairo graph. Using currentFourPlatforms to avoid SQL select continuously
		if(currentFourPlatforms == null || //currentFourPlatformsType == null ||
				currentFourPlatforms.UniqueID != treeViewResultsSession.EventSelectedID)
		{
			currentFourPlatforms = SqliteFourPlatforms.SelectData (treeViewResultsSession.EventSelectedID, false);
			//currentRunIntervalType = SqliteRunIntervalType.SelectAndReturnRunIntervalType(currentRunInterval.Type, false); //TODO: with fourPlatforms (when there are types)

			cairoGraphFourPlatformsPoints_ll = currentFourPlatforms.Points_ll;

			//reconstruct the lines
			cairoGraphFourPlatformsStepsBottom_l = new List<PointF> ();
			cairoGraphFourPlatformsStepsTop_l = new List<PointF> ();
			if (currentFourPlatforms.ExerciseID >= 1)
				currentFourPlatforms.GetStepsBottomStepsTop (ref cairoGraphFourPlatformsStepsBottom_l, ref cairoGraphFourPlatformsStepsTop_l);
		}
		 drawingarea_results_realtime.QueueDraw ();

		 updateGraphFourPlatformsBars (); //to show the selected bar
	}
}
