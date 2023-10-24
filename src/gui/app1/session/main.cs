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
 * Copyright (C) 2004-2023   Xavier de Blas <xaviblas@gmail.com>
 */

using System;
using System.IO;
using Gtk;
//using Glade;
using Mono.Unix;

//here using app1s_ , "s" means session
//this file has been moved from his old window to be part of app1 on Chronojump 2.0

public partial class ChronoJumpWindow
{
	// at glade ---->
	Gtk.Notebook app1s_notebook;

	//notebook tab 0
	Gtk.Frame frame_session_more_this_session;
	Gtk.Label label_session_more_session_name;
	Gtk.Button button_menu_session_export;
	Gtk.EventBox app1s_eventbox_button_close0;
	Gtk.Image image_session_more_window_blue;
	Gtk.Image image_session_more_window_yellow;

	//notebook tab 1
	Gtk.RadioButton app1s_radio_import_new_session;
	Gtk.RadioButton app1s_radio_import_current_session;
	Gtk.Image app1s_image_open_database;
	Gtk.Label app1s_label_open_database_file;
	Gtk.Button app1s_button_select_file_import_same_database;
	Gtk.EventBox app1s_eventbox_button_cancel1;

	//notebook tab 2
	Gtk.Grid app1s_grid_select;
	Gtk.TreeView app1s_treeview_session_load;
	Gtk.ProgressBar app1s_progressbar_treeview_session_load;
	Gtk.Button app1s_button_edit;
	Gtk.Button app1s_button_delete;
	Gtk.Button app1s_button_load;
	Gtk.Button app1s_button_import;
	Gtk.Image app1s_image_edit;
	Gtk.Image app1s_image_delete;
	Gtk.Image app1s_image_cancel;
	Gtk.Image app1s_image_load;
	Gtk.Image app1s_image_import;
	Gtk.Entry app1s_entry_search_filter;
	Gtk.HBox app1s_hbox_tags;
	Gtk.Button app1s_button_manage_tags;
	Gtk.CheckButton app1s_checkbutton_show_data_persons;
	Gtk.CheckButton app1s_checkbutton_show_data_jumps;
	Gtk.CheckButton app1s_checkbutton_show_data_runs;
	Gtk.CheckButton app1s_checkbutton_show_data_isometric;
	Gtk.CheckButton app1s_checkbutton_show_data_elastic;
	Gtk.CheckButton app1s_checkbutton_show_data_weights;
	Gtk.CheckButton app1s_checkbutton_show_data_inertial;
	Gtk.Viewport app1s_viewport_checkbutton_show_data_jumps;
	Gtk.Viewport app1s_viewport_checkbutton_show_data_runs;
	Gtk.Viewport app1s_viewport_checkbutton_show_data_isometric;
	Gtk.Viewport app1s_viewport_checkbutton_show_data_elastic;
	Gtk.Viewport app1s_viewport_checkbutton_show_data_weights;
	Gtk.Viewport app1s_viewport_checkbutton_show_data_inertial;

	//Gtk.CheckButton app1s_checkbutton_show_data_rt;
	//Gtk.CheckButton app1s_checkbutton_show_data_other;
	Gtk.Image app1s_image_show_data_persons;
	Gtk.Image app1s_image_show_data_jumps;
	Gtk.Image app1s_image_show_data_runs;
	Gtk.Image app1s_image_show_data_isometric;
	Gtk.Image app1s_image_show_data_elastic;
	Gtk.Image app1s_image_show_data_encoder_grav;
	Gtk.Image app1s_image_show_data_encoder_inertial;

	Gtk.Label app1s_file_path_import;
	Gtk.Notebook app1s_notebook_load_button_animation;
	Gtk.HButtonBox app1s_hbuttonbox_page2_import;
	Gtk.EventBox app1s_eventbox_button_edit;
	Gtk.EventBox app1s_eventbox_button_delete;
	Gtk.EventBox app1s_eventbox_button_cancel;
	Gtk.EventBox app1s_eventbox_button_load;
	Gtk.EventBox app1s_eventbox_button_back;
	Gtk.EventBox app1s_eventbox_button_import;
	Gtk.Image image_app1s_button_cancel1;
	Gtk.Image image_app1s_button_back;

	//notebook tab 3
	Gtk.Label app1s_label_import_session_name;
	Gtk.Label app1s_label_import_file;
	Gtk.EventBox app1s_eventbox_button_import_confirm_back;
	Gtk.EventBox app1s_eventbox_button_import_confirm_accept;

	//notebook tab 4
	Gtk.ProgressBar app1s_progressbarImport;
	Gtk.HBox app1s_hbox_import_done_at_new_session;
	Gtk.Label app1s_label_import_done_at_current_session;
	Gtk.ScrolledWindow app1s_scrolledwindow_import_error;
	Gtk.TextView app1s_textview_import_error;
	Gtk.Image app1s_image_import1;
	Gtk.HButtonBox app1s_hbuttonbox_page4;
	Gtk.EventBox app1s_eventbox_button_import_close;
	Gtk.EventBox app1s_eventbox_button_import_again;

	//notebook tab 5
	Gtk.EventBox app1s_eventbox_button_delete_close;
	Gtk.Image image_app1s_button_delete_cancel;
	Gtk.Image image_app1s_button_delete_accept;
	Gtk.Image image_app1s_button_delete_close;

	//notebook tab 6 (add/edit)
	Gtk.Notebook app1sae_notebook_add_edit;
	Gtk.Entry app1sae_entry_name;
	Gtk.Entry app1sae_entry_place;
	Gtk.HBox hbox_session_add;
	Gtk.HBox hbox_session_more_edit;
	Gtk.TextView app1sae_textview_tags;
	Gtk.Label app1sae_label_name;
	Gtk.Label app1sae_label_date;
	Gtk.Image image_session_new_blue;
	Gtk.Image image_session_new_yellow;
	Gtk.Image image_sport_undefined;
	Gtk.Image image_speciallity_undefined;
	Gtk.Image image_level_undefined;
	Gtk.RadioButton app1sae_radiobutton_diff_sports;
	Gtk.RadioButton app1sae_radiobutton_same_sport;
	Gtk.RadioButton app1sae_radiobutton_diff_speciallities;
	Gtk.RadioButton app1sae_radiobutton_same_speciallity;
	Gtk.RadioButton app1sae_radiobutton_diff_levels;
	Gtk.RadioButton app1sae_radiobutton_same_level;
	Gtk.Box app1sae_hbox_sports;
	Gtk.Box app1sae_hbox_combo_sports;
	Gtk.Box app1sae_vbox_speciallity;
	Gtk.Label app1sae_label_speciallity;
	Gtk.Box app1sae_hbox_speciallities;
	Gtk.Box app1sae_hbox_combo_speciallities;
	Gtk.Box app1sae_vbox_level;
	Gtk.Label app1sae_label_level;
	Gtk.Box app1sae_hbox_levels;
	Gtk.Box app1sae_hbox_combo_levels;
	Gtk.TextView app1sae_textview_comments;
	Gtk.Button app1sae_button_accept;
	Gtk.Image image_app1sae_button_cancel;
	Gtk.Image image_app1sae_button_accept;

	//notebook tab 7 (backup)
	Gtk.Notebook notebook_session_backup;
	Gtk.Label label_backup_why;
	Gtk.CheckButton app1s_check_backup_include_logs;
	Gtk.CheckButton app1s_check_backup_include_config;
	Gtk.Label app1s_label_backup_estimated_size;
	Gtk.Button app1s_button_backup_select;
	Gtk.Button app1s_button_backup_start;
	Gtk.Button app1s_button_backup_cancel_close;
	Gtk.Label app1s_label_backup_cancel_close;
	Gtk.EventBox app1s_eventbox_button_backup_cancel_close;
	Gtk.Image image_app1s_button_backup_cancel_close;
	Gtk.Image app1s_image_button_backup_select;
	Gtk.Label app1s_label_backup_destination;
	Gtk.HBox app1s_hbox_backup_doing;
	Gtk.Label app1s_label_backup_progress;
	Gtk.ProgressBar app1s_pulsebarBackupActivity;
	Gtk.ProgressBar app1s_pulsebarBackupDirs;
	Gtk.ProgressBar app1s_pulsebarBackupSecondDirs;
	Gtk.Button app1s_button_delete_old_incomplete;
	Gtk.Button app1s_button_old_backups_delete_do;
	Gtk.Label app1s_label_old_backups_delete_done;
	Gtk.Button app1s_button_backup_scheduled_remind_next_time;
	Gtk.Button app1s_button_backup_scheduled_remind_30d;
	Gtk.Button app1s_button_backup_scheduled_remind_60d;
	Gtk.Button app1s_button_backup_scheduled_remind_90d;
	Gtk.Label app1s_label_remind_feedback;
	Gtk.Alignment app1s_alignment_copyToCloud;
	Gtk.Button app1s_button_copyToCloud;
	Gtk.ProgressBar app1s_progressbar_copyToCloud_dirs;
	Gtk.ProgressBar app1s_progressbar_copyToCloud_subDirs;
	Gtk.Box box_copy_from_cloud_progressbars;
	Gtk.ProgressBar app1s_progressbar_copyFromCloud_dirs;
	Gtk.ProgressBar app1s_progressbar_copyFromCloud_subDirs;

	//notebook tab 8 (export)
	Gtk.Button app1s_button_export_select;
	Gtk.Button app1s_button_export_start;
	Gtk.Button app1s_button_export_cancel;
	Gtk.Button app1s_button_export_close;
	Gtk.EventBox app1s_eventbox_button_export_cancel;
	Gtk.EventBox app1s_eventbox_button_export_close;
	Gtk.Image app1s_image_button_export_select;
	Gtk.Label app1s_label_export_destination;
	Gtk.Label app1s_label_export_progress;
	Gtk.Image image_app1s_button_export_cancel;
	Gtk.Image image_app1s_button_export_close;
	Gtk.ProgressBar app1s_pulsebarExportActivity;

	//notebook tab 9 (view_data_folder)
	Gtk.Label app1s_label_view_data_folder_mode_name;
	Gtk.Label app1s_label_view_data_folder_session;
	Gtk.Button button_view_data_folder_specific;
	Gtk.Label app1s_label_view_data_folder_specific_no_data;
	Gtk.EventBox app1s_eventbox_button_view_data_folder_close;
	Gtk.Image image_app1s_button_view_data_folder_close;

	//notebook tab 10 (import_from_csv)
	Gtk.Notebook notebook_session_import_from_csv;
	Gtk.RadioButton app1s_import_jumps_simple;
	Gtk.RadioButton app1s_import_jumps_multiple;
	Gtk.RadioButton app1s_import_runs_simple;
	Gtk.RadioButton app1s_import_runs_intervallic;
	Gtk.TextView app1s_textview_import_from_csv_format;
	Gtk.Button app1s_button_import_csv_select_and_import;
	Gtk.Label app1s_label_import_csv_result;
	Gtk.Button app1s_button_import_from_csv_view_errors;
	Gtk.TextView app1s_textview_import_from_csv_errors;
	Gtk.Image image_app1s_button_import_from_csv_close;
	Gtk.Image image_app1s_button_import_from_csv_errors_back;
	Gtk.Button app1s_button_import_from_csv_close;

	// <---- at glade

	Gtk.ComboBoxText app1sae_combo_sports;
	Gtk.ComboBoxText app1sae_combo_speciallities;
	Gtk.ComboBoxText app1sae_combo_levels;

	const int app1s_PAGE_MODES = 0;
	const int app1s_PAGE_IMPORT_START = 1;
	const int app1s_PAGE_SELECT_SESSION = 2; //for load session and for import
	public const int app1s_PAGE_IMPORT_CONFIRM = 3;
	public const int app1s_PAGE_IMPORT_RESULT = 4;
	public const int app1s_PAGE_DELETE_CONFIRM = 5;
	const int app1s_PAGE_ADD_EDIT = 6;
	const int app1s_PAGE_BACKUP = 7;
	const int app1s_PAGE_EXPORT = 8;
	const int app1s_PAGE_VIEW_DATA_FOLDER = 9;
	const int app1s_PAGE_IMPORT_FROM_CSV = 10;

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
	}

	private void app1s_eventboxes_paint()
	{
		UtilGtk.EventBoxColorBackgroundActive (app1s_eventbox_button_close0,
				UtilGtk.Colors.YELLOW, UtilGtk.Colors.YELLOW_LIGHT);
		UtilGtk.EventBoxColorBackgroundActive (app1s_eventbox_button_cancel1,
				UtilGtk.Colors.YELLOW, UtilGtk.Colors.YELLOW_LIGHT);
		UtilGtk.EventBoxColorBackgroundActive (app1s_eventbox_button_edit,
				UtilGtk.Colors.YELLOW, UtilGtk.Colors.YELLOW_LIGHT);
		UtilGtk.EventBoxColorBackgroundActive (app1s_eventbox_button_delete,
				UtilGtk.Colors.YELLOW, UtilGtk.Colors.YELLOW_LIGHT);
		UtilGtk.EventBoxColorBackgroundActive (app1s_eventbox_button_cancel,
				UtilGtk.Colors.YELLOW, UtilGtk.Colors.YELLOW_LIGHT);
		UtilGtk.EventBoxColorBackgroundActive (app1s_eventbox_button_load,
				UtilGtk.Colors.YELLOW, UtilGtk.Colors.YELLOW_LIGHT);
		UtilGtk.EventBoxColorBackgroundActive (app1s_eventbox_button_back,
				UtilGtk.Colors.YELLOW, UtilGtk.Colors.YELLOW_LIGHT);
		UtilGtk.EventBoxColorBackgroundActive (app1s_eventbox_button_import_confirm_back,
				UtilGtk.Colors.YELLOW, UtilGtk.Colors.YELLOW_LIGHT);
		UtilGtk.EventBoxColorBackgroundActive (app1s_eventbox_button_import_confirm_accept,
				UtilGtk.Colors.YELLOW, UtilGtk.Colors.YELLOW_LIGHT);
		UtilGtk.EventBoxColorBackgroundActive (app1s_eventbox_button_import,
				UtilGtk.Colors.YELLOW, UtilGtk.Colors.YELLOW_LIGHT);
		UtilGtk.EventBoxColorBackgroundActive (app1s_eventbox_button_import_close,
				UtilGtk.Colors.YELLOW, UtilGtk.Colors.YELLOW_LIGHT);
		UtilGtk.EventBoxColorBackgroundActive (app1s_eventbox_button_import_again,
				UtilGtk.Colors.YELLOW, UtilGtk.Colors.YELLOW_LIGHT);
		UtilGtk.EventBoxColorBackgroundActive (app1s_eventbox_button_delete_close,
				UtilGtk.Colors.YELLOW, UtilGtk.Colors.YELLOW_LIGHT);
		UtilGtk.EventBoxColorBackgroundActive (app1s_eventbox_button_backup_cancel_close,
				UtilGtk.Colors.YELLOW, UtilGtk.Colors.YELLOW_LIGHT);
		UtilGtk.EventBoxColorBackgroundActive (app1s_eventbox_button_export_cancel,
				UtilGtk.Colors.YELLOW, UtilGtk.Colors.YELLOW_LIGHT);
		UtilGtk.EventBoxColorBackgroundActive (app1s_eventbox_button_export_close,
				UtilGtk.Colors.YELLOW, UtilGtk.Colors.YELLOW_LIGHT);
		UtilGtk.EventBoxColorBackgroundActive (app1s_eventbox_button_view_data_folder_close,
				UtilGtk.Colors.YELLOW, UtilGtk.Colors.YELLOW_LIGHT);
	}

	private void app1s_label_session_set_name()
	{
		if(currentSession == null)
			label_session_more_session_name.Text = "";
		else
			label_session_more_session_name.Text = currentSession.Name;
	}

	private void on_button_view_data_folder_clicked (object o, EventArgs args)
	{
		if (
				( current_mode != Constants.Modes.RUNSENCODER && //this 4 modes are the only one who have a separate dir
				current_mode != Constants.Modes.POWERGRAVITATORY &&
				current_mode != Constants.Modes.POWERINERTIAL &&
				! Constants.ModeIsFORCESENSOR (current_mode)) ||
				currentSession == null || currentSession.UniqueID < 0)
		{
			string dir = app1s_getDataFolderGeneric ();
			if(! Util.OpenURL (dir))
				new DialogMessage(Constants.MessageTypes.WARNING,
						Catalog.GetString("Error. Cannot open directory.") + "\n\n" + dir);
		}
		else {
			app1s_label_view_data_folder_mode_name.Text = "<b>" + Constants.ModePrint (current_mode) + "</b>";
			app1s_label_view_data_folder_mode_name.UseMarkup = true;

			app1s_label_view_data_folder_session.Text = string.Format("({0}) <b>{1}</b>",
					currentSession.UniqueID, currentSession.Name);
			app1s_label_view_data_folder_session.UseMarkup = true;

			string dir = app1s_getDataFolderSpecific ();
			LogB.Information("dir: " + dir);
			if (dir == "" || ! Util.DirectoryExists (dir)) {
				button_view_data_folder_specific.Sensitive = false;
				app1s_label_view_data_folder_specific_no_data.Text = "<b>" + Catalog.GetString("No data. Please, perform tests.") + "</b>";
				app1s_label_view_data_folder_specific_no_data.UseMarkup = true;
			} else {
				button_view_data_folder_specific.Sensitive = true;
				app1s_label_view_data_folder_specific_no_data.Text = "";
			}

			app1s_notebook.CurrentPage = app1s_PAGE_VIEW_DATA_FOLDER;
		}
	}

	private void on_button_view_data_folder_generic_clicked (object o, EventArgs args)
	{
		string dir = app1s_getDataFolderGeneric ();
		if(! Util.OpenURL (dir))
			new DialogMessage(Constants.MessageTypes.WARNING,
					Catalog.GetString("Error. Cannot open directory.") + "\n\n" + dir);
	}

	private void on_button_view_data_folder_specific_clicked (object o, EventArgs args)
	{
		string dir = app1s_getDataFolderSpecific ();
		if(! Util.OpenURL (dir))
			new DialogMessage(Constants.MessageTypes.WARNING,
					Catalog.GetString("Error. Cannot open directory.") + "\n\n" + dir);
	}

	private string app1s_getDataFolderGeneric ()
	{
		string databaseURL = Util.GetDatabaseDir() + System.IO.Path.DirectorySeparatorChar  + "chronojump.db";
		string databaseTempURL = Util.GetDatabaseTempDir() + System.IO.Path.DirectorySeparatorChar  + "chronojump.db";

		System.IO.FileInfo file1 = new System.IO.FileInfo(databaseURL); //potser cal una arrobar abans (a windows)
		System.IO.FileInfo file2 = new System.IO.FileInfo(databaseTempURL); //potser cal una arrobar abans (a windows)

		if(! file1.Exists && ! file2.Exists)
			new DialogMessage(Constants.MessageTypes.WARNING, Constants.DatabaseNotFoundStr());

		string dir = "";
		if(file1.Exists)
			dir = Util.GetLocalDataDir (false);
		else if(file2.Exists)
			dir = Util.GetDatabaseTempDir();

		return dir;
	}

	private string app1s_getDataFolderSpecific ()
	{
		//extra checks
		string modeFolder = Constants.ModeFolder (current_mode);
		if (modeFolder == "")
			return "";

		if (currentSession == null || currentSession.UniqueID < 0)
			return "";

		return Path.Combine(app1s_getDataFolderGeneric (), modeFolder, currentSession.UniqueID.ToString()); 
	}

	private void on_app1s_button_view_data_folder_close_clicked (object o, EventArgs args)
	{
		app1s_notebook.CurrentPage = app1s_PAGE_MODES;
	}

	private void connectWidgetsSessionMain (Gtk.Builder builder)
	{
		app1s_notebook = (Gtk.Notebook) builder.GetObject ("app1s_notebook");

		//notebook tab 0
		frame_session_more_this_session = (Gtk.Frame) builder.GetObject ("frame_session_more_this_session");
		label_session_more_session_name = (Gtk.Label) builder.GetObject ("label_session_more_session_name");
		button_menu_session_export = (Gtk.Button) builder.GetObject ("button_menu_session_export");
		app1s_eventbox_button_close0 = (Gtk.EventBox) builder.GetObject ("app1s_eventbox_button_close0");
		image_session_more_window_blue = (Gtk.Image) builder.GetObject ("image_session_more_window_blue");
		image_session_more_window_yellow = (Gtk.Image) builder.GetObject ("image_session_more_window_yellow");

		//notebook tab 1
		app1s_radio_import_new_session = (Gtk.RadioButton) builder.GetObject ("app1s_radio_import_new_session");
		app1s_radio_import_current_session = (Gtk.RadioButton) builder.GetObject ("app1s_radio_import_current_session");
		app1s_image_open_database = (Gtk.Image) builder.GetObject ("app1s_image_open_database");
		app1s_label_open_database_file = (Gtk.Label) builder.GetObject ("app1s_label_open_database_file");
		app1s_button_select_file_import_same_database = (Gtk.Button) builder.GetObject ("app1s_button_select_file_import_same_database");
		app1s_eventbox_button_cancel1 = (Gtk.EventBox) builder.GetObject ("app1s_eventbox_button_cancel1");

		//notebook tab 2
		app1s_grid_select = (Gtk.Grid) builder.GetObject ("app1s_grid_select");
		app1s_treeview_session_load = (Gtk.TreeView) builder.GetObject ("app1s_treeview_session_load");
		app1s_progressbar_treeview_session_load = (Gtk.ProgressBar) builder.GetObject ("app1s_progressbar_treeview_session_load");
		app1s_button_edit = (Gtk.Button) builder.GetObject ("app1s_button_edit");
		app1s_button_delete = (Gtk.Button) builder.GetObject ("app1s_button_delete");
		app1s_button_load = (Gtk.Button) builder.GetObject ("app1s_button_load");
		app1s_button_import = (Gtk.Button) builder.GetObject ("app1s_button_import");
		app1s_image_edit = (Gtk.Image) builder.GetObject ("app1s_image_edit");
		app1s_image_delete = (Gtk.Image) builder.GetObject ("app1s_image_delete");
		app1s_image_cancel = (Gtk.Image) builder.GetObject ("app1s_image_cancel");
		app1s_image_load = (Gtk.Image) builder.GetObject ("app1s_image_load");
		app1s_image_import = (Gtk.Image) builder.GetObject ("app1s_image_import");
		app1s_entry_search_filter = (Gtk.Entry) builder.GetObject ("app1s_entry_search_filter");
		app1s_hbox_tags = (Gtk.HBox) builder.GetObject ("app1s_hbox_tags");
		app1s_button_manage_tags = (Gtk.Button) builder.GetObject ("app1s_button_manage_tags");
		app1s_checkbutton_show_data_persons = (Gtk.CheckButton) builder.GetObject ("app1s_checkbutton_show_data_persons");
		app1s_checkbutton_show_data_jumps = (Gtk.CheckButton) builder.GetObject ("app1s_checkbutton_show_data_jumps");
		app1s_checkbutton_show_data_runs = (Gtk.CheckButton) builder.GetObject ("app1s_checkbutton_show_data_runs");
		app1s_checkbutton_show_data_isometric = (Gtk.CheckButton) builder.GetObject ("app1s_checkbutton_show_data_isometric");
		app1s_checkbutton_show_data_elastic = (Gtk.CheckButton) builder.GetObject ("app1s_checkbutton_show_data_elastic");
		app1s_checkbutton_show_data_weights = (Gtk.CheckButton) builder.GetObject ("app1s_checkbutton_show_data_weights");
		app1s_checkbutton_show_data_inertial = (Gtk.CheckButton) builder.GetObject ("app1s_checkbutton_show_data_inertial");
		app1s_viewport_checkbutton_show_data_jumps = (Gtk.Viewport) builder.GetObject ("app1s_viewport_checkbutton_show_data_jumps");
		app1s_viewport_checkbutton_show_data_runs = (Gtk.Viewport) builder.GetObject ("app1s_viewport_checkbutton_show_data_runs");
		app1s_viewport_checkbutton_show_data_isometric = (Gtk.Viewport) builder.GetObject ("app1s_viewport_checkbutton_show_data_isometric");
		app1s_viewport_checkbutton_show_data_elastic = (Gtk.Viewport) builder.GetObject ("app1s_viewport_checkbutton_show_data_elastic");
		app1s_viewport_checkbutton_show_data_weights = (Gtk.Viewport) builder.GetObject ("app1s_viewport_checkbutton_show_data_weights");
		app1s_viewport_checkbutton_show_data_inertial = (Gtk.Viewport) builder.GetObject ("app1s_viewport_checkbutton_show_data_inertial");

		//app1s_checkbutton_show_data_rt = (Gtk.CheckButton) builder.GetObject ("app1s_checkbutton_show_data_rt");
		//app1s_checkbutton_show_data_other = (Gtk.CheckButton) builder.GetObject ("app1s_checkbutton_show_data_other");
		app1s_image_show_data_persons = (Gtk.Image) builder.GetObject ("app1s_image_show_data_persons");
		app1s_image_show_data_jumps = (Gtk.Image) builder.GetObject ("app1s_image_show_data_jumps");
		app1s_image_show_data_runs = (Gtk.Image) builder.GetObject ("app1s_image_show_data_runs");
		app1s_image_show_data_isometric = (Gtk.Image) builder.GetObject ("app1s_image_show_data_isometric");
		app1s_image_show_data_elastic = (Gtk.Image) builder.GetObject ("app1s_image_show_data_elastic");
		app1s_image_show_data_encoder_grav = (Gtk.Image) builder.GetObject ("app1s_image_show_data_encoder_grav");
		app1s_image_show_data_encoder_inertial = (Gtk.Image) builder.GetObject ("app1s_image_show_data_encoder_inertial");

		app1s_file_path_import = (Gtk.Label) builder.GetObject ("app1s_file_path_import");
		app1s_notebook_load_button_animation = (Gtk.Notebook) builder.GetObject ("app1s_notebook_load_button_animation");
		app1s_hbuttonbox_page2_import = (Gtk.HButtonBox) builder.GetObject ("app1s_hbuttonbox_page2_import");
		app1s_eventbox_button_edit = (Gtk.EventBox) builder.GetObject ("app1s_eventbox_button_edit");
		app1s_eventbox_button_delete = (Gtk.EventBox) builder.GetObject ("app1s_eventbox_button_delete");
		app1s_eventbox_button_cancel = (Gtk.EventBox) builder.GetObject ("app1s_eventbox_button_cancel");
		app1s_eventbox_button_load = (Gtk.EventBox) builder.GetObject ("app1s_eventbox_button_load");
		app1s_eventbox_button_back = (Gtk.EventBox) builder.GetObject ("app1s_eventbox_button_back");
		app1s_eventbox_button_import = (Gtk.EventBox) builder.GetObject ("app1s_eventbox_button_import");
		image_app1s_button_cancel1 = (Gtk.Image) builder.GetObject ("image_app1s_button_cancel1");
		image_app1s_button_back = (Gtk.Image) builder.GetObject ("image_app1s_button_back");

		//notebook tab 3
		app1s_label_import_session_name = (Gtk.Label) builder.GetObject ("app1s_label_import_session_name");
		app1s_label_import_file = (Gtk.Label) builder.GetObject ("app1s_label_import_file");
		app1s_eventbox_button_import_confirm_back = (Gtk.EventBox) builder.GetObject ("app1s_eventbox_button_import_confirm_back");
		app1s_eventbox_button_import_confirm_accept = (Gtk.EventBox) builder.GetObject ("app1s_eventbox_button_import_confirm_accept");

		//notebook tab 4
		app1s_progressbarImport = (Gtk.ProgressBar) builder.GetObject ("app1s_progressbarImport");
		app1s_hbox_import_done_at_new_session = (Gtk.HBox) builder.GetObject ("app1s_hbox_import_done_at_new_session");
		app1s_label_import_done_at_current_session = (Gtk.Label) builder.GetObject ("app1s_label_import_done_at_current_session");
		app1s_scrolledwindow_import_error = (Gtk.ScrolledWindow) builder.GetObject ("app1s_scrolledwindow_import_error");
		app1s_textview_import_error = (Gtk.TextView) builder.GetObject ("app1s_textview_import_error");
		app1s_image_import1 = (Gtk.Image) builder.GetObject ("app1s_image_import1");
		app1s_hbuttonbox_page4 = (Gtk.HButtonBox) builder.GetObject ("app1s_hbuttonbox_page4");
		app1s_eventbox_button_import_close = (Gtk.EventBox) builder.GetObject ("app1s_eventbox_button_import_close");
		app1s_eventbox_button_import_again = (Gtk.EventBox) builder.GetObject ("app1s_eventbox_button_import_again");

		//notebook tab 5
		app1s_eventbox_button_delete_close = (Gtk.EventBox) builder.GetObject ("app1s_eventbox_button_delete_close");
		image_app1s_button_delete_cancel = (Gtk.Image) builder.GetObject ("image_app1s_button_delete_cancel");
		image_app1s_button_delete_accept = (Gtk.Image) builder.GetObject ("image_app1s_button_delete_accept");
		image_app1s_button_delete_close = (Gtk.Image) builder.GetObject ("image_app1s_button_delete_close");

		//notebook tab 6 (add/edit)
		app1sae_notebook_add_edit = (Gtk.Notebook) builder.GetObject ("app1sae_notebook_add_edit");
		app1sae_entry_name = (Gtk.Entry) builder.GetObject ("app1sae_entry_name");
		app1sae_entry_place = (Gtk.Entry) builder.GetObject ("app1sae_entry_place");
		hbox_session_add = (Gtk.HBox) builder.GetObject ("hbox_session_add");
		hbox_session_more_edit = (Gtk.HBox) builder.GetObject ("hbox_session_more_edit");
		app1sae_textview_tags = (Gtk.TextView) builder.GetObject ("app1sae_textview_tags");
		app1sae_label_name = (Gtk.Label) builder.GetObject ("app1sae_label_name");
		app1sae_label_date = (Gtk.Label) builder.GetObject ("app1sae_label_date");
		image_session_new_blue = (Gtk.Image) builder.GetObject ("image_session_new_blue");
		image_session_new_yellow = (Gtk.Image) builder.GetObject ("image_session_new_yellow");
		image_sport_undefined = (Gtk.Image) builder.GetObject ("image_sport_undefined");
		image_speciallity_undefined = (Gtk.Image) builder.GetObject ("image_speciallity_undefined");
		image_level_undefined = (Gtk.Image) builder.GetObject ("image_level_undefined");
		app1sae_radiobutton_diff_sports = (Gtk.RadioButton) builder.GetObject ("app1sae_radiobutton_diff_sports");
		app1sae_radiobutton_same_sport = (Gtk.RadioButton) builder.GetObject ("app1sae_radiobutton_same_sport");
		app1sae_radiobutton_diff_speciallities = (Gtk.RadioButton) builder.GetObject ("app1sae_radiobutton_diff_speciallities");
		app1sae_radiobutton_same_speciallity = (Gtk.RadioButton) builder.GetObject ("app1sae_radiobutton_same_speciallity");
		app1sae_radiobutton_diff_levels = (Gtk.RadioButton) builder.GetObject ("app1sae_radiobutton_diff_levels");
		app1sae_radiobutton_same_level = (Gtk.RadioButton) builder.GetObject ("app1sae_radiobutton_same_level");
		app1sae_hbox_sports = (Gtk.Box) builder.GetObject ("app1sae_hbox_sports");
		app1sae_hbox_combo_sports = (Gtk.Box) builder.GetObject ("app1sae_hbox_combo_sports");
		app1sae_vbox_speciallity = (Gtk.Box) builder.GetObject ("app1sae_vbox_speciallity");
		app1sae_label_speciallity = (Gtk.Label) builder.GetObject ("app1sae_label_speciallity");
		app1sae_hbox_speciallities = (Gtk.Box) builder.GetObject ("app1sae_hbox_speciallities");
		app1sae_hbox_combo_speciallities = (Gtk.Box) builder.GetObject ("app1sae_hbox_combo_speciallities");
		app1sae_vbox_level = (Gtk.Box) builder.GetObject ("app1sae_vbox_level");
		app1sae_label_level = (Gtk.Label) builder.GetObject ("app1sae_label_level");
		app1sae_hbox_levels = (Gtk.Box) builder.GetObject ("app1sae_hbox_levels");
		app1sae_hbox_combo_levels = (Gtk.Box) builder.GetObject ("app1sae_hbox_combo_levels");
		app1sae_textview_comments = (Gtk.TextView) builder.GetObject ("app1sae_textview_comments");
		app1sae_button_accept = (Gtk.Button) builder.GetObject ("app1sae_button_accept");
		image_app1sae_button_cancel = (Gtk.Image) builder.GetObject ("image_app1sae_button_cancel");
		image_app1sae_button_accept = (Gtk.Image) builder.GetObject ("image_app1sae_button_accept");

		//notebook tab 7 (backup)
		notebook_session_backup = (Gtk.Notebook) builder.GetObject ("notebook_session_backup");
		label_backup_why = (Gtk.Label) builder.GetObject ("label_backup_why");
		app1s_check_backup_include_logs = (Gtk.CheckButton) builder.GetObject ("app1s_check_backup_include_logs");
		app1s_check_backup_include_config = (Gtk.CheckButton) builder.GetObject ("app1s_check_backup_include_config");
		app1s_label_backup_estimated_size = (Gtk.Label) builder.GetObject ("app1s_label_backup_estimated_size");
		app1s_button_backup_select = (Gtk.Button) builder.GetObject ("app1s_button_backup_select");
		app1s_button_backup_start = (Gtk.Button) builder.GetObject ("app1s_button_backup_start");
		app1s_button_backup_cancel_close = (Gtk.Button) builder.GetObject ("app1s_button_backup_cancel_close");
		app1s_label_backup_cancel_close = (Gtk.Label) builder.GetObject ("app1s_label_backup_cancel_close");
		app1s_eventbox_button_backup_cancel_close = (Gtk.EventBox) builder.GetObject ("app1s_eventbox_button_backup_cancel_close");
		image_app1s_button_backup_cancel_close = (Gtk.Image) builder.GetObject ("image_app1s_button_backup_cancel_close");
		app1s_image_button_backup_select = (Gtk.Image) builder.GetObject ("app1s_image_button_backup_select");
		app1s_label_backup_destination = (Gtk.Label) builder.GetObject ("app1s_label_backup_destination");
		app1s_hbox_backup_doing = (Gtk.HBox) builder.GetObject ("app1s_hbox_backup_doing");
		app1s_label_backup_progress = (Gtk.Label) builder.GetObject ("app1s_label_backup_progress");
		app1s_pulsebarBackupActivity = (Gtk.ProgressBar) builder.GetObject ("app1s_pulsebarBackupActivity");
		app1s_pulsebarBackupDirs = (Gtk.ProgressBar) builder.GetObject ("app1s_pulsebarBackupDirs");
		app1s_pulsebarBackupSecondDirs = (Gtk.ProgressBar) builder.GetObject ("app1s_pulsebarBackupSecondDirs");
		app1s_button_delete_old_incomplete = (Gtk.Button) builder.GetObject ("app1s_button_delete_old_incomplete");
		app1s_button_old_backups_delete_do = (Gtk.Button) builder.GetObject ("app1s_button_old_backups_delete_do");
		app1s_label_old_backups_delete_done = (Gtk.Label) builder.GetObject ("app1s_label_old_backups_delete_done");
		app1s_button_backup_scheduled_remind_next_time = (Gtk.Button) builder.GetObject ("app1s_button_backup_scheduled_remind_next_time");
		app1s_button_backup_scheduled_remind_30d = (Gtk.Button) builder.GetObject ("app1s_button_backup_scheduled_remind_30d");
		app1s_button_backup_scheduled_remind_60d = (Gtk.Button) builder.GetObject ("app1s_button_backup_scheduled_remind_60d");
		app1s_button_backup_scheduled_remind_90d = (Gtk.Button) builder.GetObject ("app1s_button_backup_scheduled_remind_90d");
		app1s_label_remind_feedback = (Gtk.Label) builder.GetObject ("app1s_label_remind_feedback");
		app1s_alignment_copyToCloud = (Gtk.Alignment) builder.GetObject ("app1s_alignment_copyToCloud");
		app1s_button_copyToCloud = (Gtk.Button) builder.GetObject ("app1s_button_copyToCloud");
		app1s_progressbar_copyToCloud_dirs = (Gtk.ProgressBar) builder.GetObject ("app1s_progressbar_copyToCloud_dirs");
		app1s_progressbar_copyToCloud_subDirs = (Gtk.ProgressBar) builder.GetObject ("app1s_progressbar_copyToCloud_subDirs");
		box_copy_from_cloud_progressbars = (Gtk.Box) builder.GetObject ("box_copy_from_cloud_progressbars");
		app1s_progressbar_copyFromCloud_dirs = (Gtk.ProgressBar) builder.GetObject ("app1s_progressbar_copyFromCloud_dirs");
		app1s_progressbar_copyFromCloud_subDirs = (Gtk.ProgressBar) builder.GetObject ("app1s_progressbar_copyFromCloud_subDirs");

		//notebook tab 8 (export)
		app1s_button_export_select = (Gtk.Button) builder.GetObject ("app1s_button_export_select");
		app1s_button_export_start = (Gtk.Button) builder.GetObject ("app1s_button_export_start");
		app1s_button_export_cancel = (Gtk.Button) builder.GetObject ("app1s_button_export_cancel");
		app1s_button_export_close = (Gtk.Button) builder.GetObject ("app1s_button_export_close");
		app1s_eventbox_button_export_cancel = (Gtk.EventBox) builder.GetObject ("app1s_eventbox_button_export_cancel");
		app1s_eventbox_button_export_close = (Gtk.EventBox) builder.GetObject ("app1s_eventbox_button_export_close");
		app1s_image_button_export_select = (Gtk.Image) builder.GetObject ("app1s_image_button_export_select");
		app1s_label_export_destination = (Gtk.Label) builder.GetObject ("app1s_label_export_destination");
		app1s_label_export_progress = (Gtk.Label) builder.GetObject ("app1s_label_export_progress");
		image_app1s_button_export_cancel = (Gtk.Image) builder.GetObject ("image_app1s_button_export_cancel");
		image_app1s_button_export_close = (Gtk.Image) builder.GetObject ("image_app1s_button_export_close");
		app1s_pulsebarExportActivity = (Gtk.ProgressBar) builder.GetObject ("app1s_pulsebarExportActivity");

		//notebook tab 9 (view_data_folder)
		app1s_label_view_data_folder_mode_name = (Gtk.Label) builder.GetObject ("app1s_label_view_data_folder_mode_name");
		app1s_label_view_data_folder_session = (Gtk.Label) builder.GetObject ("app1s_label_view_data_folder_session");
		button_view_data_folder_specific = (Gtk.Button) builder.GetObject ("button_view_data_folder_specific");
		app1s_label_view_data_folder_specific_no_data = (Gtk.Label) builder.GetObject ("app1s_label_view_data_folder_specific_no_data");
		app1s_eventbox_button_view_data_folder_close = (Gtk.EventBox) builder.GetObject ("app1s_eventbox_button_view_data_folder_close");
		image_app1s_button_view_data_folder_close = (Gtk.Image) builder.GetObject ("image_app1s_button_view_data_folder_close");

		//notebook tab 10 (import from csv)
		notebook_session_import_from_csv = (Gtk.Notebook) builder.GetObject ("notebook_session_import_from_csv");
		app1s_import_jumps_simple = (Gtk.RadioButton) builder.GetObject ("app1s_import_jumps_simple");
		app1s_import_jumps_multiple = (Gtk.RadioButton) builder.GetObject ("app1s_import_jumps_multiple");
		app1s_import_runs_simple = (Gtk.RadioButton) builder.GetObject ("app1s_import_runs_simple");
		app1s_import_runs_intervallic = (Gtk.RadioButton) builder.GetObject ("app1s_import_runs_intervallic");
		app1s_textview_import_from_csv_format = (Gtk.TextView) builder.GetObject ("app1s_textview_import_from_csv_format");
		app1s_button_import_csv_select_and_import = (Gtk.Button) builder.GetObject ("app1s_button_import_csv_select_and_import");
		app1s_label_import_csv_result = (Gtk.Label) builder.GetObject ("app1s_label_import_csv_result");
		app1s_button_import_from_csv_view_errors = (Gtk.Button) builder.GetObject ("app1s_button_import_from_csv_view_errors");
		app1s_textview_import_from_csv_errors = (Gtk.TextView) builder.GetObject ("app1s_textview_import_from_csv_errors");
		image_app1s_button_import_from_csv_close = (Gtk.Image) builder.GetObject ("image_app1s_button_import_from_csv_close");
		image_app1s_button_import_from_csv_errors_back = (Gtk.Image) builder.GetObject ("image_app1s_button_import_from_csv_errors_back");
		app1s_button_import_from_csv_close = (Gtk.Button) builder.GetObject ("app1s_button_import_from_csv_close");
	}
}
