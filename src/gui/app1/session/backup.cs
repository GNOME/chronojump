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
 * Copyright (C) 2020   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
using Gtk;
//using Glade;
//using GLib; //for Value
//using System.Text; //StringBuilder
//using System.Collections; //ArrayList
//using Mono.Unix;

public partial class ChronoJumpWindow
{
	private void on_button_db_backup_clicked (object o, EventArgs args)
	{
		app1s_label_backup_destination.Text = "";
		app1s_button_backup_start.Sensitive = false;
		app1s_button_backup_cancel.Visible = true;
		app1s_button_backup_close.Visible = false;
	
		app1s_notebook.CurrentPage = app1s_PAGE_BACKUP;
	}

	private void on_app1s_button_backup_select_clicked (object o, EventArgs args)
	{
	}

	private void on_app1s_button_backup_start_clicked (object o, EventArgs args)
	{
	}

	private void on_app1s_button_backup_cancel_or_close_clicked (object o, EventArgs args)
	{
		app1s_notebook.CurrentPage = app1s_PAGE_MODES;
	}
}
