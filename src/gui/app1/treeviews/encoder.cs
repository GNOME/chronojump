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
 * Copyright (C) 2025   Xavier de Blas <xaviblas@gmail.com>
 */

using System;
using System.IO; 
using System.IO.Ports;
using Gtk;
using Gdk;
//using Glade;
using System.Collections;
using System.Collections.Generic; //List<T>
//using Mono.Unix;


public partial class ChronoJumpWindow 
{
	private void fillTreeView_encoder (string filter)
	{
		fillTreeView_encoder (filter, false);
	}
	private void fillTreeView_encoder (string filter, bool dbconOpened)
	{
		LogB.Information ("fillTreeView_encoder start");
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

		/*
		SqliteTests sqliteTests = new SqliteEncoder ();

		string [] myValues = sqliteTests.SelectSA (dbconOpened,
				currentSession.UniqueID, currentPersonOrAll (),
				//"",
				true, Constants.EncoderExerciseTable,
				Sqlite.Orders_by.DEFAULT, 0);
		if (Constants.ModeIsENCODER (current_mode))
		{
			LogB.Information ("calling treeViewResultsSession.Fill");
			treeViewResultsSession.Fill (myValues, filter,
					Util.GetVideosOfSessionAndMode (currentSession.UniqueID, Constants.TestTypes.ENCODER));
		}
		*/

		if (! Constants.ModeIsENCODER (current_mode))
			return;

		SqliteEncoder sqliteEncoder = new SqliteEncoder ();
		List<List<EncoderSQL>> eSQL_ll = sqliteEncoder.SelectSetsAndRepsLList (dbconOpened,
				currentPersonOrAll (), currentSession.UniqueID, currentEncoderGI, -1, -1);

		treeViewResultsSession.FillEncoder (eSQL_ll, filter,
				Util.GetVideosOfSessionAndMode (currentSession.UniqueID, Constants.TestTypes.ENCODER));

		//if show just one person, have it expanded (optimal)
		if (! radio_contacts_results_personAll.Active && currentPerson != null)
		{
			treeview_results_session.CollapseAll ();
			((TreeViewEvent) treeViewResultsSession).ExpandOptimal();
		} else
			expandOrMinimizeTreeView((TreeViewEvent) treeViewResultsSession, treeview_results_session);
	}

}
