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
using Mono.Unix;
using System.Collections; //ArrayList

public partial class ChronoJumpWindow 
{
	Gtk.TreeView treeview_jumps;

	private void createTreeView_jumps (Gtk.TreeView tv)
	{
		//myTreeViewJumps is a TreeViewJumps instance
		myTreeViewJumps = new TreeViewJumps(tv, preferences, TreeViewEvent.ExpandStates.MINIMIZED);

		//the glade cursor_changed does not work on mono 1.2.5 windows
		tv.CursorChanged += on_treeview_jumps_cursor_changed; 
	}

	private void fillTreeView_jumps (string filter) {
		fillTreeView_jumps(filter, false);
	}
	private void fillTreeView_jumps (string filter, bool dbconOpened)
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

		string [] myJumps = SqliteJump.SelectJumpsSA (dbconOpened,
				currentSession.UniqueID, currentPersonOrAll (), "", "", Sqlite.Orders_by.DEFAULT, 0);

		myTreeViewJumps.Fill (myJumps, filter,
				Util.GetVideosOfSessionAndMode (currentSession.UniqueID, Constants.TestTypes.JUMP));
		if (current_mode == Constants.Modes.JUMPSSIMPLE)
			treeViewResultsSession.Fill (myJumps, filter,
					Util.GetVideosOfSessionAndMode (currentSession.UniqueID, Constants.TestTypes.JUMP));

		//if show just one person, have it expanded
		if (! radio_contacts_results_personAll.Active && currentPerson != null)
		{
			treeview_jumps.ExpandAll();
			treeview_results_session.ExpandAll();
		} else {
			expandOrMinimizeTreeView((TreeViewEvent) myTreeViewJumps, treeview_jumps);
			expandOrMinimizeTreeView((TreeViewEvent) treeViewResultsSession, treeview_results_session);
		}
	}

	private void on_button_jumps_zoom_clicked (object o, EventArgs args)
	{
		myTreeViewJumps.ZoomChange (image_jumps_zoom);
	}

	private void treeview_jumps_storeReset() {
		myTreeViewJumps.RemoveColumns();
		
		myTreeViewJumps = new TreeViewJumps(treeview_jumps, preferences, myTreeViewJumps.ExpandState);
	}

	private void on_treeview_jumps_cursor_changed (object o, EventArgs args)
	{
		LogB.Information("Cursor changed");

		sensitiveLastTestButtons(false);

		// don't select if it's a person, 
		// is for not confusing with the person treeviews that controls who jumps
		if (myTreeViewJumps.EventSelectedID == 0) {
			myTreeViewJumps.Unselect();
			showHideActionEventButtons(false); //hide
		} else {
			showHideActionEventButtons(true); //show
			updateGraphJumpsSimple (); //to show the selected bar
		}
	}

	private void selectJumpSimple (int id)
	{
		myTreeViewJumps.ZoomToTestsIfNeeded ();
		myTreeViewJumps.SelectEvent (id, true); //scroll
		on_treeview_jumps_cursor_changed (new object (), new EventArgs ()); //in order to update the play video button
	}

	private void treeviewJumpsContextMenu(Jump myJump)
	{
		Menu myMenu = new Menu ();
		Gtk.MenuItem myItem;

		myItem = new MenuItem ( Catalog.GetString("Edit selected") + " " + myJump.Type + " (" + myJump.PersonName + ")");
		myItem.Activated += on_edit_selected_jump_clicked;
		myMenu.Attach( myItem, 0, 1, 0, 1 );
	
		Gtk.SeparatorMenuItem mySep = new SeparatorMenuItem();
		myMenu.Attach( mySep, 0, 1, 1, 2 );

		myItem = new MenuItem ( Catalog.GetString("Delete selected") + " " + myJump.Type + " (" + myJump.PersonName + ")");
		myItem.Activated += on_delete_selected_jump_clicked;
		myMenu.Attach( myItem, 0, 1, 2, 3 );

		myMenu.ShowAll();
		myMenu.Popup();
	}
}
