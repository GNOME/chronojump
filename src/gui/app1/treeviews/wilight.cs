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
	Gtk.TreeView treeview_wilight;

	private void createTreeView_wilight (Gtk.TreeView tv) {
		//myTreeViewWilight is a TreeViewWilight instance
		myTreeViewWilight = new TreeViewWilight (tv, preferences.digitsNumber, TreeViewEvent.ExpandStates.MINIMIZED );

		//the glade cursor_changed does not work on mono 1.2.5 windows
		tv.CursorChanged += on_treeview_wilight_cursor_changed;
	}

	private void fillTreeView_wilight (string filter) {
		fillTreeView_wilight (filter, false);
	}
	private void fillTreeView_wilight (string filter, bool dbconOpened)
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

		/*
		List<Wilight> wilight_l = SqliteWilight.Select (dbconOpened,
				currentSession.UniqueID, currentPersonOrAll ()//,
				//"", Sqlite.Orders_by.DEFAULT, 0);
			);
		string [] wilightSA = TreeViewWilight.ListToStringArray (wilight_l);
		*/
		string [] wilightSA = SqliteWilight.SelectSA (dbconOpened,
				currentSession.UniqueID, currentPersonOrAll (),
				//"",
				Sqlite.Orders_by.DEFAULT, 0);

		myTreeViewWilight.Fill (wilightSA,
				//filter,
				//Util.GetVideosOfSessionAndMode (currentSession.UniqueID, Constants.TestTypes.RUN));
				"", new List<string> ());
		if (current_mode == Constants.Modes.WILIGHT)
			treeViewResultsSession.Fill (wilightSA,
					//filter,
					//Util.GetVideosOfSessionAndMode (currentSession.UniqueID, Constants.TestTypes.RUN));
					"", new List<string> ());

		//if show just one person, have it expanded
		if (! radio_contacts_results_personAll.Active && currentPerson != null)
		{
			treeview_wilight.ExpandAll();
			treeview_results_session.ExpandAll();
		} else {
			expandOrMinimizeTreeView((TreeViewEvent) myTreeViewWilight, treeview_wilight);
			expandOrMinimizeTreeView((TreeViewEvent) treeViewResultsSession, treeview_results_session);
		}
	}

	private void on_button_wilight_zoom_clicked (object o, EventArgs args)
	{
		myTreeViewWilight.ZoomChange (image_wilight_zoom);
	}

	private void treeview_wilight_storeReset()
	{
		myTreeViewWilight.RemoveColumns();
		myTreeViewWilight = new TreeViewWilight (treeview_wilight,
				preferences.digitsNumber, myTreeViewWilight.ExpandState);
	}

	private void on_treeview_wilight_cursor_changed (object o, EventArgs args)
	{
		LogB.Information("Cursor changed");

		sensitiveLastTestButtons(false);

		// don't select if it's a person,
		// is for not confusing with the person treeviews that controls who jumps
		if (myTreeViewWilight.EventSelectedID == 0) {
			myTreeViewWilight.Unselect();
			showHideActionEventButtons(false); //hide
		} else {
			showHideActionEventButtons(true); //show
			updateGraphWilightBars (); //to show the selected bar
		}
	}

	private void selectWilight (int id)
	{
		//LogB.Information ("selectWilight: " + id.ToString ());
		myTreeViewWilight.ZoomToTestsIfNeeded ();
		myTreeViewWilight.SelectEvent (id, true); //scroll
		on_treeview_wilight_cursor_changed (new object (), new EventArgs ()); //in order to update the play video button
	}

	private void treeviewWilightContextMenu (Wilight wilight)
	{
		Menu myMenu = new Menu ();
		Gtk.MenuItem myItem;

		myItem = new MenuItem ( Catalog.GetString("Edit selected") + " (" + wilight.PersonName + ")");
		myItem.Activated += on_edit_selected_wilight_clicked;
		myMenu.Attach( myItem, 0, 1, 0, 1 );

		Gtk.SeparatorMenuItem mySep = new SeparatorMenuItem();
		myMenu.Attach( mySep, 0, 1, 1, 2 );

		myItem = new MenuItem ( Catalog.GetString("Delete selected") + " (" + wilight.PersonName + ")");
		myItem.Activated += on_delete_selected_wilight_clicked;
		myMenu.Attach( myItem, 0, 1, 2, 3 );

		myMenu.ShowAll();
		myMenu.Popup();
	}
}
