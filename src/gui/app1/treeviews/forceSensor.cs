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
	private void fillTreeView_forceSensor (string filter) {
		fillTreeView_forceSensor (filter, false);
	}
	private void fillTreeView_forceSensor (string filterExercise, bool dbconOpened)
	{
		LogB.Information ("At fillTreeView_forceSensor");
		if (currentSession == null || ! Constants.ModeIsFORCESENSOR (current_mode))
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
		List<ForceSensor> forceSensor_l = SqliteForceSensor.Select (dbconOpened,
				currentSession.UniqueID, currentPersonOrAll ()//,
				//"", Sqlite.Orders_by.DEFAULT, 0);
			);
		string [] forceSensorSA = TreeViewForceSensor.ListToStringArray (forceSensor_l);
		*/
		SqliteTests sqliteTests = new SqliteForceSensor ();

		//show isometric or elastic
		if (current_mode == Constants.Modes.FORCESENSORISOMETRIC)
			sqliteTests.FilterOtherString = " AND " + Constants.ForceSensorExerciseTable + ".elastic != 1"; //0 or -1 (both) 
		else if (current_mode == Constants.Modes.FORCESENSORELASTIC)
			sqliteTests.FilterOtherString = " AND " + Constants.ForceSensorExerciseTable + ".elastic != 0"; //1 or -1 (both)

		string [] forceSensorSA = sqliteTests.SelectSA (dbconOpened,
				currentSession.UniqueID, currentPersonOrAll (),
				//"",
				true, Constants.ForceSensorExerciseTable,
				Sqlite.Orders_by.DEFAULT, 0);

		sqliteTests.FilterOtherString = ""; //precaution

		LogB.Information ("fillTreeView_forceSensor calling Fill");
		treeViewResultsSession.Fill (forceSensorSA,
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
