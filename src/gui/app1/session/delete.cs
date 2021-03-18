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
 * Copyright (C) 2004-2020   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
using Gtk;
using Glade;

public partial class ChronoJumpWindow
{
	[Widget] Gtk.VBox app1s_vbox_delete_question;
	[Widget] Gtk.Label app1s_label_delete_session_confirm_name;
	[Widget] Gtk.HButtonBox app1s_hbuttonbox_delete_accept_cancel;
	[Widget] Gtk.Label app1s_label_delete_cannot;
	[Widget] Gtk.Label app1s_label_delete_done;
	[Widget] Gtk.Button app1s_button_delete_close;

	private bool deleteSessionCalledFromLoad;
	private Session tempDeletingSession;

	//not called from load
	private void on_app1s_delete_session_confirm_start (object o, EventArgs args)
	{
		deleteSessionCalledFromLoad = false;
		tempDeletingSession = currentSession;

		on_app1s_delete_session_confirm_start_do ();
	}

	private void on_app1s_delete_session_confirm_start_do ()
	{
		//first show notebook tab in order to ensure the .Visible = true will work
		app1s_notebook.CurrentPage = app1s_PAGE_DELETE_CONFIRM;

		app1s_vbox_delete_question.Visible = false;
		app1s_label_delete_session_confirm_name.Visible = false;
		app1s_hbuttonbox_delete_accept_cancel.Visible = false;

		app1s_label_delete_cannot.Visible = false;
		app1s_label_delete_done.Visible = false;

		app1s_button_delete_close.Visible = false;

		if(tempDeletingSession.Name == Constants.SessionSimulatedName)
		{
			app1s_label_delete_cannot.Visible = true;
			app1s_button_delete_close.Visible = true;
		}
		else {
			app1s_vbox_delete_question.Visible = true;

			app1s_label_delete_session_confirm_name.Text = "<b>" + tempDeletingSession.Name + "</b>";
			app1s_label_delete_session_confirm_name.UseMarkup = true;
			app1s_label_delete_session_confirm_name.Visible = true;

			app1s_hbuttonbox_delete_accept_cancel.Visible = true;
		}
	}
	
	private void on_app1s_button_delete_accept_clicked (object o, EventArgs args) 
	{
		if(currentSession.UniqueID == tempDeletingSession.UniqueID)
			closeSession ();

		SqliteSession.DeleteAllStuff(tempDeletingSession.UniqueID.ToString());
		
		app1s_vbox_delete_question.Visible = false;
		app1s_label_delete_session_confirm_name.Visible = false;
		app1s_hbuttonbox_delete_accept_cancel.Visible = false;

		app1s_label_delete_done.Visible = true;
		app1s_button_delete_close.Visible = true;
	}

	private void on_app1s_button_delete_cancel_clicked (object o, EventArgs args)
	{
		if(deleteSessionCalledFromLoad)
			app1s_notebook.CurrentPage = app1s_PAGE_SELECT_SESSION;
		else
			app1s_notebook.CurrentPage = app1s_PAGE_MODES;
	}

	private void on_app1s_button_delete_close_clicked (object o, EventArgs args)
	{
		if(deleteSessionCalledFromLoad)
		{
			app1s_notebook.CurrentPage = app1s_PAGE_SELECT_SESSION;

			//and reload the treeview:
			app1s_recreateTreeView("deleted a session coming from load session");
		} else
			app1s_notebook.CurrentPage = app1s_PAGE_MODES;
	}
}
