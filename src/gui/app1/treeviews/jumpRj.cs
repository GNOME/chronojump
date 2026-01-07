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
 * Copyright (C) 2004-2026   Xavier de Blas <xaviblas@gmail.com>
 */


using System;
using Gtk;
//using Glade;
using System.Collections; //ArrayList
using System.Collections.Generic; //List

public partial class ChronoJumpWindow 
{
	private void fillTreeView_jumps_rj (string filter)
	{
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
		if (current_mode == Constants.Modes.JUMPSREACTIVE)
			treeViewResultsSession.Fill (myJumps, filter,
					Util.GetVideosOfSessionAndMode (currentSession.UniqueID, Constants.TestTypes.JUMP_RJ));

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

	private void on_treeview_jumps_rj_cursor_changed (object o, EventArgs args)
	{
		LogB.Information ("on_treeview_jumps_rj_cursor_changed");
		sensitiveSelectedTestButtons (false);

		// don't select if it's a person, 
		// is for not confusing with the person treeviews that controls who jumps
		if (treeViewResultsSession.EventSelectedID == TreeViewEvent.MarkRowIsPerson)
		{
			sensitiveSelectedTestButtons (false);

			selectedJumpRj = null;
			blankJumpReactiveRealtimeCaptureGraph ();

			return;
		}

		if (treeViewResultsSession.EventSelectedID == TreeViewEvent.MarkNonSelectRowSubEvent)
			treeViewResultsSession.SelectEventHeaderLine();

		sensitiveSelectedTestButtons (true);

		//graph the jump on realtime cairo graph. Using selectedJumpRj to avoid SQL select continuously
		if(selectedJumpRj == null || selectedJumpRj.UniqueID != treeViewResultsSession.EventSelectedID)
			selectedJumpRj = SqliteJumpRj.SelectJumpData("jumpRj", treeViewResultsSession.EventSelectedID, true, false); //true: personNameInComment

		updateGraphJumpsReactive (); //to show the selected bar

		blankJumpReactiveRealtimeCaptureGraph ();
		PrepareJumpReactiveRealtimeCaptureGraph (selectedJumpRj.tvLast, selectedJumpRj.tcLast,
				selectedJumpRj.TvString, selectedJumpRj.TcString,
				selectedJumpRj.Type, selectedJumpRj.Description, //Description is personName
				preferences.volumeOn, preferences.gstreamer, feedbackJumpsRj,
				preferences.heightPreferred);
		drawingarea_results_realtime.QueueDraw ();
	}
}
