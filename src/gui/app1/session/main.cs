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
using System.IO;
using Gtk;
using Glade;
using Mono.Unix;

//here using app1s_ , "s" means session
//this file has been moved from his old window to be part of app1 on Chronojump 2.0

public partial class ChronoJumpWindow
{
	[Widget] Gtk.Notebook app1s_notebook;

	//notebook tab 0
	[Widget] Gtk.Frame frame_session_more_this_session;
	[Widget] Gtk.Label label_session_more_session_name;
	[Widget] Gtk.Button button_menu_session_export;
	[Widget] Gtk.EventBox app1s_eventbox_button_close0;

	//notebook tab 1
	[Widget] Gtk.HBox hbox_session_more;
	[Widget] Gtk.VBox vbox_session_overview;
	[Widget] Gtk.RadioButton app1s_radio_import_new_session;
	[Widget] Gtk.RadioButton app1s_radio_import_current_session;
	[Widget] Gtk.Image app1s_image_open_database;
	[Widget] Gtk.Label app1s_label_open_database_file;
	[Widget] Gtk.Button app1s_button_select_file_import_same_database;
	[Widget] Gtk.EventBox app1s_eventbox_button_cancel1;

	//notebook tab 2
	[Widget] Gtk.TreeView app1s_treeview_session_load;
	[Widget] Gtk.Button app1s_button_edit;
	[Widget] Gtk.Button app1s_button_delete;
	[Widget] Gtk.Button app1s_button_load;
	[Widget] Gtk.Button app1s_button_import;
	[Widget] Gtk.Image app1s_image_edit;
	[Widget] Gtk.Image app1s_image_delete;
	[Widget] Gtk.Image app1s_image_cancel;
	[Widget] Gtk.Image app1s_image_load;
	[Widget] Gtk.Image app1s_image_import;
	[Widget] Gtk.Entry app1s_entry_search_filter;
	[Widget] Gtk.HBox app1s_hbox_manage;
	[Widget] Gtk.Button app1s_button_manage_tags;
	[Widget] Gtk.CheckButton app1s_checkbutton_show_data_persons;
	[Widget] Gtk.CheckButton app1s_checkbutton_show_data_jumps;
	[Widget] Gtk.CheckButton app1s_checkbutton_show_data_runs;
	[Widget] Gtk.CheckButton app1s_checkbutton_show_data_force_sensor;
	[Widget] Gtk.CheckButton app1s_checkbutton_show_data_encoder;
	//[Widget] Gtk.CheckButton app1s_checkbutton_show_data_rt;
	//[Widget] Gtk.CheckButton app1s_checkbutton_show_data_other;
	[Widget] Gtk.Image app1s_image_show_data_persons;
	[Widget] Gtk.Image app1s_image_show_data_jumps;
	[Widget] Gtk.Image app1s_image_show_data_runs;
	[Widget] Gtk.Image app1s_image_show_data_run_encoder;
	[Widget] Gtk.Image app1s_image_show_data_rt;
	[Widget] Gtk.Image app1s_image_show_data_force_sensor;
	[Widget] Gtk.Image app1s_image_show_data_encoder_grav;
	[Widget] Gtk.Image app1s_image_show_data_encoder_inertial;
	[Widget] Gtk.Image app1s_image_show_data_other;

	[Widget] Gtk.Label app1s_file_path_import;
	[Widget] Gtk.Notebook app1s_notebook_load_button_animation;
	[Widget] Gtk.HButtonBox app1s_hbuttonbox_page2_import;
	[Widget] Gtk.EventBox app1s_eventbox_button_edit;
	[Widget] Gtk.EventBox app1s_eventbox_button_delete;
	[Widget] Gtk.EventBox app1s_eventbox_button_cancel;
	[Widget] Gtk.EventBox app1s_eventbox_button_load;
	[Widget] Gtk.EventBox app1s_eventbox_button_back;
	[Widget] Gtk.EventBox app1s_eventbox_button_import;
	[Widget] Gtk.Image image_app1s_button_cancel1;
	[Widget] Gtk.Image image_app1s_button_back;

	//notebook tab 3
	[Widget] Gtk.Label app1s_label_import_session_name;
	[Widget] Gtk.Label app1s_label_import_file;
	[Widget] Gtk.Button app1s_button_import_confirm_accept;
	[Widget] Gtk.EventBox app1s_eventbox_button_import_confirm_back;
	[Widget] Gtk.EventBox app1s_eventbox_button_import_confirm_accept;

	//notebook tab 4
	[Widget] Gtk.ProgressBar app1s_progressbarImport;
	[Widget] Gtk.HBox app1s_hbox_import_done_at_new_session;
	[Widget] Gtk.Label app1s_label_import_done_at_current_session;
	[Widget] Gtk.ScrolledWindow app1s_scrolledwindow_import_error;
	[Widget] Gtk.TextView app1s_textview_import_error;
	[Widget] Gtk.Image app1s_image_import1;
	[Widget] Gtk.HButtonBox app1s_hbuttonbox_page4;
	[Widget] Gtk.EventBox app1s_eventbox_button_import_close;
	[Widget] Gtk.EventBox app1s_eventbox_button_import_again;

	//notebook tab 5
	[Widget] Gtk.EventBox app1s_eventbox_button_delete_close;
	[Widget] Gtk.Image image_app1s_button_delete_cancel;
	[Widget] Gtk.Image image_app1s_button_delete_accept;
	[Widget] Gtk.Image image_app1s_button_delete_close;

	//notebook tab 6 (add/edit)
	[Widget] Gtk.Notebook app1sae_notebook_add_edit;
	[Widget] Gtk.Entry app1sae_entry_name;
	[Widget] Gtk.Entry app1sae_entry_place;
	[Widget] Gtk.HBox hbox_session_add;
	[Widget] Gtk.HBox hbox_session_more_edit;
	[Widget] Gtk.TextView app1sae_textview_tags;
	[Widget] Gtk.Label app1sae_label_name;
	[Widget] Gtk.Label app1sae_label_date;
	[Widget] Gtk.Image image_session_new_blue;
	[Widget] Gtk.Image image_session_new_yellow;
	[Widget] Gtk.Image image_sport_undefined;
	[Widget] Gtk.Image image_speciallity_undefined;
	[Widget] Gtk.Image image_level_undefined;
	[Widget] Gtk.RadioButton app1sae_radiobutton_diff_sports;
	[Widget] Gtk.RadioButton app1sae_radiobutton_same_sport;
	[Widget] Gtk.RadioButton app1sae_radiobutton_diff_speciallities;
	[Widget] Gtk.RadioButton app1sae_radiobutton_same_speciallity;
	[Widget] Gtk.RadioButton app1sae_radiobutton_diff_levels;
	[Widget] Gtk.RadioButton app1sae_radiobutton_same_level;
	[Widget] Gtk.Box app1sae_hbox_sports;
	[Widget] Gtk.Box app1sae_hbox_combo_sports;
	[Widget] Gtk.ComboBox app1sae_combo_sports;
	[Widget] Gtk.Box app1sae_vbox_speciallity;
	[Widget] Gtk.Label app1sae_label_speciallity;
	[Widget] Gtk.Box app1sae_hbox_speciallities;
	[Widget] Gtk.Box app1sae_hbox_combo_speciallities;
	[Widget] Gtk.ComboBox app1sae_combo_speciallities;
	[Widget] Gtk.Box app1sae_vbox_level;
	[Widget] Gtk.Label app1sae_label_level;
	[Widget] Gtk.Box app1sae_hbox_levels;
	[Widget] Gtk.Box app1sae_hbox_combo_levels;
	[Widget] Gtk.ComboBox app1sae_combo_levels;
	[Widget] Gtk.TextView app1sae_textview_comments;
	[Widget] Gtk.Button app1sae_button_accept;
	[Widget] Gtk.Image image_app1sae_button_cancel;
	[Widget] Gtk.Image image_app1sae_button_accept;

	//notebook tab 7 (backup)
	[Widget] Gtk.Notebook notebook_session_backup;
	[Widget] Gtk.Label label_backup_why;
	[Widget] Gtk.Label app1s_label_backup_estimated_size;
	[Widget] Gtk.Button app1s_button_backup_select;
	[Widget] Gtk.Button app1s_button_backup_start;
	[Widget] Gtk.Button app1s_button_backup_cancel_close;
	[Widget] Gtk.Label app1s_label_backup_cancel_close;
	[Widget] Gtk.EventBox app1s_eventbox_button_backup_cancel_close;
	[Widget] Gtk.Image image_app1s_button_backup_cancel_close;
	[Widget] Gtk.Image app1s_image_button_backup_select;
	[Widget] Gtk.Label app1s_label_backup_destination;
	[Widget] Gtk.HBox app1s_hbox_backup_doing;
	[Widget] Gtk.Label app1s_label_backup_progress;
	[Widget] Gtk.ProgressBar app1s_pulsebarBackupActivity;
	[Widget] Gtk.ProgressBar app1s_pulsebarBackupDirs;
	[Widget] Gtk.ProgressBar app1s_pulsebarBackupSecondDirs;
	[Widget] Gtk.Button app1s_button_delete_old_incomplete;
	[Widget] Gtk.Button app1s_button_old_backups_delete_do;
	[Widget] Gtk.Label app1s_label_old_backups_delete_done;
	[Widget] Gtk.Button app1s_button_backup_scheduled_remind_next_time;
	[Widget] Gtk.Button app1s_button_backup_scheduled_remind_30d;
	[Widget] Gtk.Button app1s_button_backup_scheduled_remind_60d;
	[Widget] Gtk.Button app1s_button_backup_scheduled_remind_90d;
	[Widget] Gtk.Label app1s_label_remind_feedback;

	//notebook tab 8 (export)
	[Widget] Gtk.Button app1s_button_export_select;
	[Widget] Gtk.Button app1s_button_export_start;
	[Widget] Gtk.Button app1s_button_export_cancel;
	[Widget] Gtk.Button app1s_button_export_close;
	[Widget] Gtk.EventBox app1s_eventbox_button_export_cancel;
	[Widget] Gtk.EventBox app1s_eventbox_button_export_close;
	[Widget] Gtk.Image app1s_image_button_export_select;
	[Widget] Gtk.Label app1s_label_export_destination;
	[Widget] Gtk.Label app1s_label_export_progress;
	[Widget] Gtk.Image image_app1s_button_export_cancel;
	[Widget] Gtk.Image image_app1s_button_export_close;
	[Widget] Gtk.ProgressBar app1s_pulsebarExportActivity;

	const int app1s_PAGE_MODES = 0;
	const int app1s_PAGE_IMPORT_START = 1;
	const int app1s_PAGE_SELECT_SESSION = 2; //for load session and for import
	public const int app1s_PAGE_IMPORT_CONFIRM = 3;
	public const int app1s_PAGE_IMPORT_RESULT = 4;
	public const int app1s_PAGE_DELETE_CONFIRM = 5;
	const int app1s_PAGE_ADD_EDIT = 6;
	const int app1s_PAGE_BACKUP = 7;
	const int app1s_PAGE_EXPORT = 8;

	private int app1s_notebook_sup_entered_from; //to store from which page we entered (to return at it)

	// ---- notebook page 0 buttons ----
	void app1s_on_button_close0_clicked (object o, EventArgs args)
	{
		menus_sensitive_import_not_danger(true);
		notebook_supSetOldPage();
	}

	private void notebook_supSetOldPage()
	{
		notebook_sup.CurrentPage = app1s_notebook_sup_entered_from;

		//but if it is start page, ensure notebook_mode_selector is 0
		if(notebook_sup.CurrentPage == Convert.ToInt32(notebook_sup_pages.START))
			notebook_mode_selector.CurrentPage = 0;
	}

	private void app1s_eventboxes_paint()
	{
		UtilGtk.EventBoxColorBackgroundActive (app1s_eventbox_button_close0, UtilGtk.YELLOW, UtilGtk.YELLOW_LIGHT);
		UtilGtk.EventBoxColorBackgroundActive (app1s_eventbox_button_cancel1, UtilGtk.YELLOW, UtilGtk.YELLOW_LIGHT);
		UtilGtk.EventBoxColorBackgroundActive (app1s_eventbox_button_edit, UtilGtk.YELLOW, UtilGtk.YELLOW_LIGHT);
		UtilGtk.EventBoxColorBackgroundActive (app1s_eventbox_button_delete, UtilGtk.YELLOW, UtilGtk.YELLOW_LIGHT);
		UtilGtk.EventBoxColorBackgroundActive (app1s_eventbox_button_cancel, UtilGtk.YELLOW, UtilGtk.YELLOW_LIGHT);
		UtilGtk.EventBoxColorBackgroundActive (app1s_eventbox_button_load, UtilGtk.YELLOW, UtilGtk.YELLOW_LIGHT);
		UtilGtk.EventBoxColorBackgroundActive (app1s_eventbox_button_back, UtilGtk.YELLOW, UtilGtk.YELLOW_LIGHT);
		UtilGtk.EventBoxColorBackgroundActive (app1s_eventbox_button_import_confirm_back, UtilGtk.YELLOW, UtilGtk.YELLOW_LIGHT);
		UtilGtk.EventBoxColorBackgroundActive (app1s_eventbox_button_import_confirm_accept, UtilGtk.YELLOW, UtilGtk.YELLOW_LIGHT);
		UtilGtk.EventBoxColorBackgroundActive (app1s_eventbox_button_import, UtilGtk.YELLOW, UtilGtk.YELLOW_LIGHT);
		UtilGtk.EventBoxColorBackgroundActive (app1s_eventbox_button_import_close, UtilGtk.YELLOW, UtilGtk.YELLOW_LIGHT);
		UtilGtk.EventBoxColorBackgroundActive (app1s_eventbox_button_import_again, UtilGtk.YELLOW, UtilGtk.YELLOW_LIGHT);
		UtilGtk.EventBoxColorBackgroundActive (app1s_eventbox_button_delete_close, UtilGtk.YELLOW, UtilGtk.YELLOW_LIGHT);
		UtilGtk.EventBoxColorBackgroundActive (app1s_eventbox_button_backup_cancel_close, UtilGtk.YELLOW, UtilGtk.YELLOW_LIGHT);
		UtilGtk.EventBoxColorBackgroundActive (app1s_eventbox_button_export_cancel, UtilGtk.YELLOW, UtilGtk.YELLOW_LIGHT);
		UtilGtk.EventBoxColorBackgroundActive (app1s_eventbox_button_export_close, UtilGtk.YELLOW, UtilGtk.YELLOW_LIGHT);
	}

	private void app1s_label_session_set_name()
	{
		if(currentSession == null)
			label_session_more_session_name.Text = "";
		else
			label_session_more_session_name.Text = currentSession.Name;
	}

	void on_button_data_folder_open_clicked (object o, EventArgs args)
	{
		string databaseURL = Util.GetDatabaseDir() + System.IO.Path.DirectorySeparatorChar  + "chronojump.db";
		string databaseTempURL = Util.GetDatabaseTempDir() + System.IO.Path.DirectorySeparatorChar  + "chronojump.db";

		System.IO.FileInfo file1 = new System.IO.FileInfo(databaseURL); //potser cal una arrobar abans (a windows)
		System.IO.FileInfo file2 = new System.IO.FileInfo(databaseTempURL); //potser cal una arrobar abans (a windows)

		if(! file1.Exists && ! file2.Exists)
			new DialogMessage(Constants.MessageTypes.WARNING, Constants.DatabaseNotFoundStr());

		string dir = "";
		if(file1.Exists)
			dir = Util.GetParentDir(false);
		else if(file2.Exists)
			dir = Util.GetDatabaseTempDir();

		if(! Util.OpenURL (dir))
			new DialogMessage(Constants.MessageTypes.WARNING,
					Catalog.GetString("Error. Cannot open directory.") + "\n\n" + dir);
	}

}
