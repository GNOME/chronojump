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
 * Copyright (C) 2004-2019   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
using Gtk;
using Glade;

//here using app1s_ , "s" means session
//this file has been moved from his old window to be part of app1 on Chronojump 2.0

public partial class ChronoJumpWindow
{
	[Widget] Gtk.Notebook app1s_notebook;

	//notebook tab 0
	//notebook tab 1
	[Widget] Gtk.HBox hbox_session_more;
	[Widget] Gtk.VBox vbox_session_overview;
	[Widget] Gtk.RadioButton app1s_radio_import_new_session;
	[Widget] Gtk.RadioButton app1s_radio_import_current_session;
	[Widget] Gtk.Image app1s_image_open_database;
	[Widget] Gtk.Label app1s_label_open_database_file;
	[Widget] Gtk.Button app1s_button_select_file_import_same_database;

	//notebook tab 2
	[Widget] Gtk.TreeView app1s_treeview_session_load;
	[Widget] Gtk.Button app1s_button_accept;
	[Widget] Gtk.Button app1s_button_import;
	[Widget] Gtk.Image app1s_image_import;
	[Widget] Gtk.Entry app1s_entry_search_filter;
	[Widget] Gtk.CheckButton app1s_checkbutton_show_data_jump_run;
	[Widget] Gtk.CheckButton app1s_checkbutton_show_data_other_tests;
	[Widget] Gtk.Label app1s_file_path_import;
	[Widget] Gtk.Notebook app1s_notebook_load_button_animation;
	[Widget] Gtk.HButtonBox app1s_hbuttonbox_page2_import;
	[Widget] Gtk.VBox app1s_vbox_notebook_load;

	//notebook tab 3
	[Widget] Gtk.Label app1s_label_import_session_name;
	[Widget] Gtk.Label app1s_label_import_file;
	[Widget] Gtk.Button app1s_button_import_confirm_accept;

	//notebook tab 4
	[Widget] Gtk.ProgressBar app1s_progressbarImport;
	[Widget] Gtk.Label app1s_label_import_done_at_new_session;
	[Widget] Gtk.Label app1s_label_import_done_at_current_session;
	[Widget] Gtk.ScrolledWindow app1s_scrolledwindow_import_error;
	[Widget] Gtk.TextView app1s_textview_import_error;
	[Widget] Gtk.Image app1s_image_import1;
	[Widget] Gtk.HButtonBox app1s_hbuttonbox_page4;

	const int app1s_PAGE_MODES = 0;
	const int app1s_PAGE_IMPORT_START = 1;
	const int app1s_PAGE_SELECT_SESSION = 2; //for load session and for import
	public const int app1s_PAGE_IMPORT_CONFIRM = 3;
	public const int app1s_PAGE_IMPORT_RESULT = 4;
	public const int app1s_PAGE_DELETE_CONFIRM = 5;


	// ---- notebook page 0 buttons ----
	void app1s_on_button_close0_clicked (object o, EventArgs args)
	{
		menus_sensitive_import_not_danger(true);
		notebook_supSetOldPage();
	}
}
