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
	private void fillTreeView_runEncoder (string filter) {
		fillTreeView_runEncoder (filter, false);
	}
	private void fillTreeView_runEncoder (string filterExercise, bool dbconOpened)
	{
		LogB.Information ("At fillTreeViewrunEncoder");
		if (currentSession == null || current_mode != Constants.Modes.RUNSENCODER)
		{
			/*
			 * This happens when the user "Imports a session": Chronojump tries to
			 * update comboboxes, it reaches here because the comboboxes are updated
			 * But if the user didn't have any
			 * open session currentSession variable (see below) is null and it crashed here
			 * (when it did currentSession.UniqueID with currentSession==null)
			 */
			return;
		}

		/*
		List<RunEncoder> runEncoder_l = SqliteRunEncoder.Select (dbconOpened,
				currentSession.UniqueID, currentPersonOrAll ()//,
				//"", Sqlite.Orders_by.DEFAULT, 0);
			);
		string [] runEncoderSA = TreeViewRunEncoder.ListToStringArray (runEncoder_l);
		*/
		SqliteTests sqliteTests = new SqliteRunEncoder ();
		string [] runEncoderSA = sqliteTests.SelectSA (dbconOpened,
				currentSession.UniqueID, currentPersonOrAll (),
				//"",
				true, Constants.RunEncoderExerciseTable,
				Sqlite.Orders_by.DEFAULT, 0);

		sqliteTests.FilterOtherString = ""; //precaution

		LogB.Information ("fillTreeViewrunEncoder calling Fill");
		treeViewResultsSession.Fill (runEncoderSA,
				filterExercise,
				//Util.GetVideosOfSessionAndMode (currentSession.UniqueID, Constants.TestTypes.RUN));
			new List<string> ());

		//if show just one person, have it expanded
		if (! radio_contacts_results_personAll.Active && currentPerson != null)
			treeview_results_session.ExpandAll();
		else
			expandOrMinimizeTreeView((TreeViewEvent) treeViewResultsSession, treeview_results_session);

		// every time fillTreeView is done, update this
		updatePersonTestsN (dbconOpened);
	}
} 
