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
 * Copyright (C) 2024-2025   Xavier de Blas <xaviblas@gmail.com>
 */

using System;
using Gtk;
using Mono.Unix;

public partial class ChronoJumpWindow 
{
	private void on_delete_selected_test_clicked (object o, EventArgs args)
	{
		//1.- check that there's a line selected
		//2.- check that this line is a test and not a person
		if (treeViewResultsSession.EventSelectedID >= 0) {
			//3.- display confirmwindow of deletion
			if (preferences.askDeletion) {
				ConfirmWindow confirmWin = ConfirmWindow.Show (Catalog.GetString(
							"Are you sure you want to delete this test?"), "", "");
				confirmWin.Button_accept.Clicked += new EventHandler (delete_current_test_accepted);
			} else {
				delete_current_test_accepted (new object(), new EventArgs());
			}
		}
	}

	private void delete_current_test_accepted (object o, EventArgs args)
	{
		int id = treeViewResultsSession.EventSelectedID;
		Sqlite.Delete (false, Constants.ModeTable (current_mode), id);
		treeViewResultsSession.DelEvent (id);
		updatePersonTestsN (false);

		/*
		 * TODO:
		 * - manage selected test and testType
		 * - sensitiveSelectedTestButtons (false);
		 * - delete video
		 * - deleted_last_test_update_widgets
		 */

		updateGraphResultsSessionByMode ();
	}
}
