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
	Gtk.TreeView treeview_jumps_rj;


	private void createTreeView_jumps_rj (Gtk.TreeView tv)
	{
		myTreeViewJumpsRj = new TreeViewJumpsRj (tv, preferences, TreeViewEvent.ExpandStates.MINIMIZED);

		//the glade cursor_changed does not work on mono 1.2.5 windows
		tv.CursorChanged += on_treeview_jumps_rj_cursor_changed; 
	}

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
		myTreeViewJumpsRj.Fill (myJumps, filter,
				Util.GetVideosOfSessionAndMode (currentSession.UniqueID, Constants.TestTypes.JUMP_RJ));

		//if show just one person, have it expanded (optimal)
		if (! radio_contacts_results_personAll.Active && currentPerson != null)
		{
			treeview_jumps_rj.CollapseAll ();
			((TreeViewEvent) myTreeViewJumpsRj).ExpandOptimal();
		} else
			expandOrMinimizeTreeView((TreeViewEvent) myTreeViewJumpsRj, treeview_jumps_rj);
	}

	private void on_button_jumps_rj_zoom_clicked (object o, EventArgs args)
	{
		myTreeViewJumpsRj.ExpandState = myTreeViewJumpsRj.ZoomChange(myTreeViewJumpsRj.ExpandState);
		if(myTreeViewJumpsRj.ExpandState == TreeViewEvent.ExpandStates.MINIMIZED) {
			treeview_jumps_rj.CollapseAll();
			image_jumps_rj_zoom.Pixbuf = Chronojump.MyPixbuf.Get(null, Util.GetImagePath(false) + "zoom_in.png");
		} else if(myTreeViewJumpsRj.ExpandState == TreeViewEvent.ExpandStates.OPTIMAL) {
			treeview_jumps_rj.CollapseAll();
			myTreeViewJumpsRj.ExpandOptimal();
			image_jumps_rj_zoom.Pixbuf = Chronojump.MyPixbuf.Get(null, Util.GetImagePath(false) + "zoom_in.png");
		} else {
			treeview_jumps_rj.ExpandAll();
			image_jumps_rj_zoom.Pixbuf = Chronojump.MyPixbuf.Get(null, Util.GetImagePath(false) + "zoom_out.png");
		}
	}

	private void treeview_jumps_rj_storeReset()
	{
		myTreeViewJumpsRj.RemoveColumns();
		myTreeViewJumpsRj = new TreeViewJumpsRj (treeview_jumps_rj, preferences, myTreeViewJumpsRj.ExpandState);
	}

	private void on_treeview_jumps_rj_cursor_changed (object o, EventArgs args)
	{
		sensitiveLastTestButtons(false);

		// don't select if it's a person, 
		// is for not confusing with the person treeviews that controls who jumps
		if (myTreeViewJumpsRj.EventSelectedID == 0) {
			myTreeViewJumpsRj.Unselect();
			showHideActionEventButtons(false);
			return;
		}

		if (myTreeViewJumpsRj.EventSelectedID == -1)
			myTreeViewJumpsRj.SelectHeaderLine();

		showHideActionEventButtons(true);

		//graph the jump on realtime cairo graph. Using selectedJumpRj to avoid SQL select continuously
		if(selectedJumpRj == null || selectedJumpRj.UniqueID != myTreeViewJumpsRj.EventSelectedID)
			selectedJumpRj = SqliteJumpRj.SelectJumpData("jumpRj", myTreeViewJumpsRj.EventSelectedID, true, false); //true: personNameInComment

		updateGraphJumpsReactive (); //to show the selected bar

		blankJumpReactiveRealtimeCaptureGraph ();
		PrepareJumpReactiveRealtimeCaptureGraph (selectedJumpRj.tvLast, selectedJumpRj.tcLast,
				selectedJumpRj.TvString, selectedJumpRj.TcString,
				selectedJumpRj.Type, selectedJumpRj.Description, //Description is personName
				preferences.volumeOn, preferences.gstreamer, feedbackJumpsRj,
				preferences.heightPreferred);
		event_execute_drawingarea_realtime_capture_cairo.QueueDraw ();
	}

	private void treeviewJumpsRjContextMenu(JumpRj myJump)
	{
		Menu myMenu = new Menu ();
		Gtk.MenuItem myItem;

		myItem = new MenuItem ( Catalog.GetString("Edit selected") + " " + myJump.Type + " (" + myJump.PersonName + ")");
		myItem.Activated += on_edit_selected_jump_rj_clicked;
		myMenu.Attach( myItem, 0, 1, 0, 1 );

		myItem = new MenuItem ( Catalog.GetString("Repair selected") + " " + myJump.Type + " (" + myJump.PersonName + ")");
		myItem.Activated += on_repair_selected_jump_rj_clicked;
		myMenu.Attach( myItem, 0, 1, 1, 2 );
		
		Gtk.SeparatorMenuItem mySep = new SeparatorMenuItem();
		myMenu.Attach( mySep, 0, 1, 2, 3 );

		myItem = new MenuItem ( Catalog.GetString("Delete selected") + " " + myJump.Type + " (" + myJump.PersonName + ")");
		myItem.Activated += on_delete_selected_jump_rj_clicked;
		myMenu.Attach( myItem, 0, 1, 3, 4 );

		myMenu.ShowAll();
		myMenu.Popup();
	}
}
