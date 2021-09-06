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
using GLib; //for Value
using System.Text; //StringBuilder
using System.Collections.Generic; //List<T>
using System.Collections; //ArrayList
using Mono.Unix;

//here using app1s_ , "s" means session
//this file has been moved from his old window to be part of app1 on Chronojump 2.0

public partial class ChronoJumpWindow
{
	private enum app1s_windowType
	{
		LOAD_SESSION,
		IMPORT_SESSION
	};
	
	private TreeStore app1s_store;
	private string app1s_selected;
	private string app1s_import_file_path;

	[Widget] Gtk.HBox app1s_hbox_frame_load;
	[Widget] Gtk.HBox app1s_hbox_frame_import;
	[Widget] Gtk.Image image_session_load3_blue;
	[Widget] Gtk.Image image_session_import1_blue;
	[Widget] Gtk.Image image_session_load3_yellow;
	[Widget] Gtk.Image image_session_import1_yellow;
	[Widget] Gtk.Label app1s_label_load;
	[Widget] Gtk.Label app1s_label_import;
	[Widget] Gtk.ScrolledWindow scrolledwin_session_load;

	[Widget] Gtk.HBox app1s_hbox_combo_tags;
	[Widget] Gtk.ComboBox app1s_combo_tags;
	CjComboGeneric app1sComboTags;

	/*
	 * when fillTreeView() is called, it executes:
	 * SqliteSessionSwitcher sessionSwitcher = new SqliteSessionSwitcher (databaseType, import_file_path);
	 *
	 * then if we finally import the session (on current session or on new session), there's a reloadSession() call
	 * that makes the connection point to client database (and not the database being imported),
	 * but if we cancel after the fillTreeView()
	 * then Chronojump continues on old db until load session is called,
	 * ensure to do a reloadSession() if cancel buttons are clicked or on delete_event
	 *
	 * before 2.0, all load/import gui was on separate window, then this was used:
	 * fakeButton_cancel_maybeDatabaseSwitched.Click();
	 */

	private app1s_windowType app1s_type;
	private bool sessionLoadWinSignals; //to be able to set radiobuttons at first without recreating the treeview

	private void app1s_initializeGui()
	{
		/*
		 * need these two lines for macOS because there are strange glitches if we do not put them here
		 * eg. if we start Chronojump and try to import without having done this first:
		 * app1s_notebook.CurrentPage = app1s_PAGE_SELECT_SESSION;
		 * then import buttons do not work.
		 * And if we do not do:
		 * app1s_notebook.CurrentPage = app1s_PAGE_IMPORT_START;
		 * before load session, buttons are displaced.
		 * All gets fixed resizing a bit the windows, but using Visible=true or .Show() does not solve the problem.
		 * So we need these (or destroy all the mac computers on the planet).
		 */
		app1s_notebook.CurrentPage = app1s_PAGE_SELECT_SESSION;
		app1s_notebook.CurrentPage = app1s_PAGE_IMPORT_START;

		if (app1s_type == app1s_windowType.LOAD_SESSION) {
			app1s_file_path_import.Visible = false;
			app1s_notebook_load_button_animation.Visible = true;
			app1s_hbuttonbox_page2_import.Visible = false;
			app1s_label_load.Text = "<b>" + Catalog.GetString ("Load session") + "</b>";
			app1s_label_load.UseMarkup = true;
			app1s_hbox_frame_load.Visible = true;
			app1s_hbox_frame_import.Visible = false;
			app1s_notebook.CurrentPage = app1s_PAGE_SELECT_SESSION;
			app1s_notebook_load_button_animation.CurrentPage = 0;
			app1s_hbox_manage.Visible = true;
		} else {
			app1s_file_path_import.Visible = true;
			app1s_notebook_load_button_animation.Visible = false;
			app1s_hbuttonbox_page2_import.Visible = true;
			app1s_label_import.Text = "<b>" + Catalog.GetString ("Import session") + "</b>";
			app1s_label_import.UseMarkup = true;
			app1s_hbox_frame_load.Visible = false;
			app1s_hbox_frame_import.Visible = true;
			app1s_button_select_file_import_same_database.Visible = false; //is shown when user want to import a second session
			app1s_notebook.CurrentPage = app1s_PAGE_IMPORT_START;
			app1s_hbox_manage.Visible = false;
		}

		createComboSessionLoadTags (false); //TODO: care because this is only related to load (not report)
		app1s_entry_search_filter.Text = "";
		app1s_button_manage_tags.Sensitive = (app1s_selected != "-1");

		//radio buttons
		sessionLoadWinSignals = false;
		/*
		app1s_checkbutton_show_data_persons.Active = preferences.sessionLoadDisplay.ShowAthletesInfo;
		app1s_checkbutton_show_data_jump_run.Active = preferences.sessionLoadDisplay.ShowJumpsRaces;
		app1s_checkbutton_show_data_other_tests.Active = preferences.sessionLoadDisplay.ShowOtherTests;
		*/
		app1s_checkbutton_show_data_jumps.Active = (current_mode == Constants.Modes.JUMPSSIMPLE ||
				current_mode == Constants.Modes.JUMPSREACTIVE);
		app1s_checkbutton_show_data_runs.Active = (current_mode == Constants.Modes.RUNSSIMPLE ||
				current_mode == Constants.Modes.RUNSINTERVALLIC ||
				current_mode == Constants.Modes.RUNSENCODER);
		app1s_checkbutton_show_data_force_sensor.Active = (current_mode == Constants.Modes.FORCESENSOR);
		app1s_checkbutton_show_data_encoder.Active = (current_mode == Constants.Modes.POWERGRAVITATORY ||
				current_mode == Constants.Modes.POWERINERTIAL);

		sessionLoadWinSignals = true;

		app1s_createTreeView(app1s_treeview_session_load, app1s_type == app1s_windowType.LOAD_SESSION,
				app1s_checkbutton_show_data_persons.Active,
				app1s_checkbutton_show_data_jumps.Active,
				app1s_checkbutton_show_data_runs.Active,
				app1s_checkbutton_show_data_force_sensor.Active,
				app1s_checkbutton_show_data_encoder.Active);/*,
				app1s_checkbutton_show_data_rt.Active,
				app1s_checkbutton_show_data_other.Active
				);*/

		app1s_store = app1s_getStore(true,
				app1s_checkbutton_show_data_persons.Active,
				app1s_checkbutton_show_data_jumps.Active,
				app1s_checkbutton_show_data_runs.Active,
				app1s_checkbutton_show_data_force_sensor.Active,
				app1s_checkbutton_show_data_encoder.Active);/*,
				app1s_checkbutton_show_data_rt.Active,
				app1s_checkbutton_show_data_other.Active
				);*/

		app1s_treeview_session_load.Model = app1s_store;

		app1s_fillTreeView(app1s_treeview_session_load, app1s_store,
				app1s_checkbutton_show_data_persons.Active,
				app1s_checkbutton_show_data_jumps.Active,
				app1s_checkbutton_show_data_runs.Active,
				app1s_checkbutton_show_data_force_sensor.Active,
				app1s_checkbutton_show_data_encoder.Active);/*,
				app1s_checkbutton_show_data_rt.Active,
				app1s_checkbutton_show_data_other.Active
				);*/

		app1s_store.SetSortColumnId(1, Gtk.SortType.Descending); //date
		app1s_store.ChangeSortColumn();

		app1s_button_load.Sensitive = false;
		app1s_button_import.Sensitive = false;
		app1s_entry_search_filter.CanFocus = true;
		app1s_entry_search_filter.IsFocus = true;

		app1s_treeview_session_load.Selection.Changed += app1s_onSelectionEntry;

		/**
		* Uncomment if we want the session file chooser to be loaded with the dialog.
		* On Linux at least the placement of the Windows can be strange so at the moment
		* I leave it disabled until we discuss (to enable or to delete it).
		if (type == WindowType.IMPORT_SESSION) {
			chooseDatabaseToImport ();
		}
		*/
	}

	private TreeStore app1s_getStore(bool loadOrImport, bool showPersons,
			bool showJumps, bool showRuns, bool showForceSensor, bool showEncoder)//, bool showRT, bool showOther)
	{
		int columns = 6;
		if(loadOrImport)
			columns ++; //on load we have the tags column

		if(showPersons)
			columns += 3;
		if(showJumps)
			columns += 2;
		if(showRuns)
			columns += 3; //includes race analyzer
		if(showForceSensor)
			columns ++;
		if(showEncoder)
			columns += 2;
		/*
		if(showRT)
			columns ++;
		if(showOther)
			columns += 2; //pulses, RT
			*/

		Type [] types = new Type [columns];
		for (int i=0; i < columns; i++) {
			types[i] = typeof (string);
		}

		TreeStore s = new TreeStore(types);

		//s.SetSortFunc (0, UtilGtk.IdColumnCompare); //not needed, it's hidden
		s.SetSortFunc (1, app1s_dateColumnCompare);
		s.ChangeSortColumn();

		return s;
	}

	private static int app1s_dateColumnCompare (TreeModel model, TreeIter iter1, TreeIter iter2)
	{
		var dt1String = (model.GetValue(iter1, 1).ToString());
		var dt2String = (model.GetValue(iter2, 1).ToString());

		DateTime dt1;
		DateTime dt2;

		var converted1 = DateTime.TryParse(dt1String, out dt1);
		var converted2 = DateTime.TryParse(dt2String, out dt2);

		if(converted1 && converted2)
			return DateTime.Compare(dt1, dt2);
		else
			return 0;
	}

	private void sessionLoadWindowShow (app1s_windowType type)
	{
		this.app1s_type = type;
		app1s_initializeGui();
		app1s_recreateTreeView("loaded the dialog");
	}

	private void app1s_createTreeView (Gtk.TreeView tv, bool loadOrImport, bool showPersons,
			bool showJumps, bool showRuns, bool showForceSensor, bool showEncoder)//, bool showRT, bool showOther)
	{
		tv.HeadersVisible=true;
		int count = 0;

		Gtk.TreeViewColumn colID = new Gtk.TreeViewColumn(Catalog.GetString ("Number"), new CellRendererText(), "text", count);
		colID.SortColumnId = count ++;
		colID.SortIndicator = true;
		colID.Visible = false; //hidden
		tv.AppendColumn (colID);

		//tv.AppendColumn ( Catalog.GetString ("Date"), new CellRendererText(), "text", count++);
		Gtk.TreeViewColumn colDate = new Gtk.TreeViewColumn(Catalog.GetString ("Date"), new CellRendererText(), "text", count);
		colDate.SortColumnId = count ++;
		colDate.SortIndicator = true;
		tv.AppendColumn (colDate);

		Gtk.TreeViewColumn colName = new Gtk.TreeViewColumn(Catalog.GetString ("Name"), new CellRendererText(), "text", count);
		colName.SortColumnId = count ++;
		colName.SortIndicator = true;
		tv.AppendColumn (colName);

		if(loadOrImport)
		{
			Gtk.TreeViewColumn colTags = new Gtk.TreeViewColumn(Catalog.GetString ("Tags"), new CellRendererText(), "text", count);
			colTags.SortColumnId = count ++;
			colTags.SortIndicator = true;
			tv.AppendColumn (colTags);
		}

		tv.AppendColumn ( Catalog.GetString ("Place"), new CellRendererText(), "text", count++);
		tv.AppendColumn ( Catalog.GetString ("Persons"), new CellRendererText(), "text", count++);
		if(showPersons) {
			tv.AppendColumn ( Catalog.GetString ("Sport"), new CellRendererText(), "text", count++);
			tv.AppendColumn ( Catalog.GetString ("Specialty"), new CellRendererText(), "text", count++);
			tv.AppendColumn ( Catalog.GetString ("Level"), new CellRendererText(), "text", count++);
		}
		if(showJumps) {
			tv.AppendColumn ( Catalog.GetString ("Jumps simple"), new CellRendererText(), "text", count++);
			tv.AppendColumn ( Catalog.GetString ("Jumps reactive"), new CellRendererText(), "text", count++);
		}
		if(showRuns) {
			tv.AppendColumn ( Catalog.GetString ("Races simple"), new CellRendererText(), "text", count++);
			tv.AppendColumn ( Catalog.GetString ("Races interval"), new CellRendererText(), "text", count++);
			tv.AppendColumn ( Catalog.GetString ("Race analyzer"), new CellRendererText(), "text", count++);
		}
		if(showForceSensor) {
			tv.AppendColumn ( Catalog.GetString ("Force sensor"), new CellRendererText(), "text", count++);
		}
		if(showEncoder) {
			tv.AppendColumn ( Catalog.GetString ("Gravitatory encoder") + "\n" +
					Catalog.GetString("Sets") + " ; " + Catalog.GetString("Repetitions"),
					new CellRendererText(), "text", count++);
			tv.AppendColumn ( Catalog.GetString ("Inertial encoder") + "\n" +
					Catalog.GetString("Sets") + " ; " + Catalog.GetString("Repetitions"),
					new CellRendererText(), "text", count++);
		}
		/*
		if(showRT) {
			tv.AppendColumn ( Catalog.GetString ("Reaction time"), new CellRendererText(), "text", count++);
		}
		if(showOther) {
			tv.AppendColumn ( Catalog.GetString ("Pulses"), new CellRendererText(), "text", count++);
			tv.AppendColumn ( Catalog.GetString ("MultiChronopic"), new CellRendererText(), "text", count++);
		}
		*/
		tv.AppendColumn ( Catalog.GetString ("Comments"), new CellRendererText(), "text", count++);
	}

        private void createComboSessionLoadTags (bool create)
        {
                if(create)
                {
                        app1sComboTags = new CjComboGeneric(app1s_combo_tags, app1s_hbox_combo_tags);
                        app1s_combo_tags = app1sComboTags.Combo;
                        app1s_combo_tags.Changed += new EventHandler (app1s_on_combo_tags_changed);
                } else {
			app1sComboTags.L_types = TagSession.ListSelectTypesOnSQL();
                        app1s_combo_tags.Changed -= new EventHandler (app1s_on_combo_tags_changed);
                        app1sComboTags.Fill();
                        app1s_combo_tags.Changed += new EventHandler (app1s_on_combo_tags_changed);
                        app1s_combo_tags = app1sComboTags.Combo;
                }
                app1s_combo_tags.Sensitive = true;
        }

	private void app1s_on_combo_tags_changed (object o, EventArgs args) {
		app1s_recreateTreeView("changed tag");
	}

	protected void app1s_on_entry_search_filter_changed (object o, EventArgs args) {
		app1s_recreateTreeView("changed search filter");
	}

	private void app1s_chooseDatabaseToImport()
	{
		//TODO: try it with Gtk.FileChooserWidget, then we do not need to pass app1 (parent)
		Gtk.FileChooserDialog filechooser = new Gtk.FileChooserDialog ("Choose Chronojump database to import from",
		                                                               app1, FileChooserAction.Open,
		                                                               "Cancel",ResponseType.Cancel,
		                                                               "Open",ResponseType.Accept);

		FileFilter file_filter = new FileFilter();
		file_filter.AddPattern ("*.db");
		file_filter.Name = "Chronojump database (chronojump.db)";
		filechooser.AddFilter (file_filter);

		if (filechooser.Run () == (int)ResponseType.Accept) {
			app1s_import_file_path = filechooser.Filename;
			//file_path_import.Text = System.IO.Path.GetFileName (import_file_path);
			app1s_file_path_import.Text = app1s_import_file_path;
			app1s_file_path_import.TooltipText = app1s_import_file_path;
			app1s_notebook.CurrentPage = app1s_PAGE_SELECT_SESSION;
		}
		filechooser.Destroy ();
		app1s_recreateTreeView ("file path changed");
	}

	void app1s_on_checkbutton_show_data_toggled (object o, EventArgs args)
	{
		if(! sessionLoadWinSignals)
			return;

		//on import this call will be done t end to affect to our desired database
		if (app1s_type == app1s_windowType.LOAD_SESSION)
			sqlChangeSessionLoadDisplay();

		app1s_recreateTreeView("show_data_toggled " + app1s_checkbutton_show_data_persons.Active.ToString());
	}

	private void sqlChangeSessionLoadDisplay ()
	{
		/*
		preferences.sessionLoadDisplay = new SessionLoadDisplay(
				app1s_checkbutton_show_data_persons.Active,
				app1s_checkbutton_show_data_jumps.Active,
				app1s_checkbutton_show_data_runs.Active,
				app1s_checkbutton_show_data_force_sensor.Active,
				app1s_checkbutton_show_data_encoder.Active,
				app1s_checkbutton_show_data_rt.Active,
				app1s_checkbutton_show_data_other.Active);

		SqlitePreferences.Update (SqlitePreferences.SessionLoadDisplay,
				preferences.sessionLoadDisplay.GetInt.ToString(), false);
				*/
	}

	void app1s_recreateTreeView(string message)
	{
		LogB.Information("Recreate treeview: " + message);

		UtilGtk.RemoveColumns(app1s_treeview_session_load);
		
		app1s_createTreeView(app1s_treeview_session_load,
				app1s_type == app1s_windowType.LOAD_SESSION,
				app1s_checkbutton_show_data_persons.Active,
				app1s_checkbutton_show_data_jumps.Active,
				app1s_checkbutton_show_data_runs.Active,
				app1s_checkbutton_show_data_force_sensor.Active,
				app1s_checkbutton_show_data_encoder.Active);/*,
				app1s_checkbutton_show_data_rt.Active,
				app1s_checkbutton_show_data_other.Active
				);*/
		app1s_store = app1s_getStore(
				true,
				app1s_checkbutton_show_data_persons.Active,
				app1s_checkbutton_show_data_jumps.Active,
				app1s_checkbutton_show_data_runs.Active,
				app1s_checkbutton_show_data_force_sensor.Active,
				app1s_checkbutton_show_data_encoder.Active);/*,
				app1s_checkbutton_show_data_rt.Active,
				app1s_checkbutton_show_data_other.Active
				);*/
		app1s_treeview_session_load.Model = app1s_store;
		app1s_fillTreeView(app1s_treeview_session_load, app1s_store,
				app1s_checkbutton_show_data_persons.Active,
				app1s_checkbutton_show_data_jumps.Active,
				app1s_checkbutton_show_data_runs.Active,
				app1s_checkbutton_show_data_force_sensor.Active,
				app1s_checkbutton_show_data_encoder.Active);/*,
				app1s_checkbutton_show_data_rt.Active,
				app1s_checkbutton_show_data_other.Active
				);*/
		
		app1s_store.SetSortColumnId(1, Gtk.SortType.Descending); //date
		app1s_store.ChangeSortColumn();


		/*
		 * after clicking on checkbuttons, treeview row gets unselected
		 * call onSelectionEntry to see if there's a row selected
		 * and it will sensitive on/off button_accept as needed
		 */
		app1s_onSelectionEntry (app1s_treeview_session_load.Selection, new EventArgs ());
	}

	private void app1s_fillTreeView (Gtk.TreeView tv, TreeStore store, bool showPersons,
			bool showJumps, bool showRuns, bool showForceSensor, bool showEncoder)//, bool showRT, bool showOther)
	{
		string filterName = "";
		if(app1s_entry_search_filter.Text.ToString().Length > 0)
			filterName = app1s_entry_search_filter.Text.ToString();

		SqliteSessionSwitcher.DatabaseType databaseType;
		if (app1s_type == app1s_windowType.LOAD_SESSION) {
			databaseType = SqliteSessionSwitcher.DatabaseType.DEFAULT;
		} else {
			databaseType = SqliteSessionSwitcher.DatabaseType.IMPORT;
		}
		SqliteSessionSwitcher sessionSwitcher = new SqliteSessionSwitcher (databaseType, app1s_import_file_path);
		
		string [] mySessions = sessionSwitcher.SelectAllSessions(filterName); //returns a string of values separated by ':'

		//new 2.0 code
		int columns = 6;
		if(showPersons)
			columns += 3;
		if(showJumps)
			columns += 2;
		if(showRuns)
			columns += 3; //includes race analyzer
		if(showForceSensor)
			columns ++;
		if(showEncoder)
			columns += 2;
		/*
		if(showRT)
			columns ++;
		if(showOther)
			columns += 2; //pulses, RT
			*/

		//tags are not going to be imported right now, so use only on load session
		List<SessionTagSession> tagsOfAllSessions = new List<SessionTagSession>();
		if (app1s_type == app1s_windowType.LOAD_SESSION)
		{
			tagsOfAllSessions = SqliteSessionTagSession.SelectTagsOfAllSessions(false);
			columns ++;
		}

		foreach (string session in mySessions)
		{
			string [] myStringFull = session.Split(new char[] {':'});
		
			//don't show any text at sport, speciallity and level if it's undefined	
			string mySport = "";
			if (myStringFull[4] != Catalog.GetString(Constants.SportUndefined)) 
				mySport = Catalog.GetString(myStringFull[4]);

			string mySpeciallity = ""; //done also because Undefined has "" as name and crashes with gettext
			if (myStringFull[5] != "") 
				mySpeciallity = Catalog.GetString(myStringFull[5]);
			
			string myLevel = "";
			if (myStringFull[6] != Catalog.GetString(Constants.LevelUndefined)) 
				myLevel = Catalog.GetString(myStringFull[6]);

			string [] strings = new string [columns];
			//for (int i=0; i < columns; i++) {
			//	types[i] = typeof (string);
			//}
			int i = 0;
			strings[i ++] = myStringFull[0]; 	//session num
			strings[i ++] = myStringFull[3];	//session date
			strings[i ++] = myStringFull[1]; 	//session name

			//to show tag column
			if (app1s_type == app1s_windowType.LOAD_SESSION)
			{
				List<TagSession> tagSession_list = SessionTagSession.FindTagSessionsOfSession(
						Convert.ToInt32(myStringFull[0]),
						tagsOfAllSessions);
				strings[i ++] = SessionTagSession.PrintTagNamesOfSession(tagSession_list);

				//do not show this session depending on tags
				if(app1sComboTags.GetSelectedId() > 0)
				{
					bool found = false;
					foreach(TagSession ts in tagSession_list)
						if(ts.UniqueID == app1sComboTags.GetSelectedId())
							found = true;

					if(! found)
						continue;
				}
			}

			strings[i ++] = myStringFull[2]; 	//session place
			strings[i ++] = myStringFull[8];	//number of jumpers x session

			if(showPersons) {
				strings[i ++] = mySport;		//personsSport
				strings[i ++] = mySpeciallity;		//personsSpeciallity
				strings[i ++] = myLevel;		//personsLevel
			}
			if(showJumps) {
				strings[i ++] = myStringFull[9];	//number of jumps x session
				strings[i ++] = myStringFull[10];	//number of jumpsRj x session
			}
			if(showRuns) {
				strings[i ++] = myStringFull[11]; 	//number of runs x session
				strings[i ++] = myStringFull[12]; 	//number of runsInterval x session
				strings[i ++] = myStringFull[19]; 	//number of runEncoder
			}
			if(showForceSensor) {
				strings[i ++] = myStringFull[18]; 	//number of forceSensor
			}
			if(showEncoder) {
				strings[i ++] = myStringFull[16]; 	//number of encoder signal x session
				strings[i ++] = myStringFull[17]; 	//number of encoder curve x session
			}
			/*
			if(showRT) {
				strings[i ++] = myStringFull[13]; 	//number of reaction times x session
			}
			if(showOther) {
				strings[i ++] = myStringFull[14]; 	//number of pulses x session
				strings[i ++] = myStringFull[15]; 	//number of multiChronopics x session
			}
			*/
			strings[i ++] = myStringFull[7];		//description of session (comments)

			app1s_store.AppendValues (strings);
		}	

	}
	
	//pass 0 for first row
	public void app1s_SelectRowByPos(int rowNumber)
	{
		TreeIter iter;
		bool iterOk = app1s_store.GetIterFirst(out iter);
		if(iterOk) {
			int count = 0;
			while (count < rowNumber) {
				app1s_store.IterNext(ref iter);
				count ++;
			}
			app1s_treeview_session_load.Selection.SelectIter(iter);
		}
	}

	public bool app1s_SelectRowByID(int searchedID)
	{
		return UtilGtk.TreeviewSelectRowWithID(
				app1s_treeview_session_load, app1s_store, 0, searchedID, false);
	}
	
	private void app1s_onSelectionEntry (object o, EventArgs args)
	{
		TreeModel model;
		TreeIter iter;
		app1s_selected = "-1";

		if (((TreeSelection)o).GetSelected(out model, out iter)) {
			app1s_selected = (string)model.GetValue (iter, 0);
			app1s_button_load.Sensitive = true;
			app1s_button_import.Sensitive = true;
			app1s_button_edit.Sensitive = true;
			app1s_button_delete.Sensitive = true;
		} else {
			app1s_button_load.Sensitive = false;
			app1s_button_import.Sensitive = false;
			app1s_button_edit.Sensitive = false;
			app1s_button_delete.Sensitive = false;
		}

		app1s_button_manage_tags.Sensitive = (app1s_selected != "-1");
	}

	//TODO: do not need to be public ? maybe for import
	public int app1s_CurrentSessionId() {
		TreeModel model;
		TreeIter iter;

		if (app1s_treeview_session_load.Selection.GetSelected (out model, out iter)) {
			string selected = (string)model.GetValue (iter, 0);
			return Convert.ToInt32 (selected.Split (':')[0]);
		}
		return -1;
	}
	private string app1s_CurrentSessionName() {
		TreeModel model;
		TreeIter iter;

		if (app1s_treeview_session_load.Selection.GetSelected (out model, out iter)) {
			return (string)model.GetValue (iter, 2);
		}
		return "";
	}

	//TODO: do not need to be public ? maybe for import
	public string app1s_ImportDatabasePath() {
		return app1s_import_file_path;
	}

	//TODO: do not need to be public ? maybe for import
	public bool app1s_ImportToNewSession() {
		return app1s_radio_import_new_session.Active;
	}

	private void app1s_radio_import_new_current_sensitive ()
	{
		if(currentSession == null)
		{
			app1s_radio_import_new_session.Active = true;
			app1s_radio_import_current_session.Sensitive = false;
		} else
			app1s_radio_import_current_session.Sensitive = true;
	}

	//TODO: do not need to be public ? maybe for import
	public void app1s_LabelImportSessionName (string str)
	{
		app1s_label_import_session_name.Text = "<b>" + str + "</b>";
		app1s_label_import_session_name.UseMarkup = true;
	}
	//TODO: do not need to be public ? maybe for import
	public void app1s_LabelImportFile (string str)
	{
		app1s_label_import_file.Text = "<b>" + str + "</b>";
		app1s_label_import_file.UseMarkup = true;
	}

	//TODO: do not need to be public ? maybe for import
	public void app1s_ImportPulse(string str)
	{
		app1s_progressbarImport.Pulse();
		app1s_progressbarImport.Text = str;
	}
	//TODO: do not need to be public ? maybe for import
	public void app1s_ImportPulseEnd()
	{
		app1s_progressbarImport.Fraction = 1;
		app1s_hbuttonbox_page4.Sensitive = true;
	}

	//TODO: do not need to be public ? maybe for import
	public void app1s_ShowLabelImportedOk()
	{
		if(app1s_radio_import_new_session.Active)
			app1s_hbox_import_done_at_new_session.Visible = true;
		else
			app1s_label_import_done_at_current_session.Visible = true;
	}

	//TODO: do not need to be public ? maybe for import
	public void app1s_ShowImportError(string str)
	{
		app1s_scrolledwindow_import_error.Visible = true;
		app1s_textview_import_error.Buffer.Text = str;
	}

	//TODO: do not need to be public ? maybe for import
	public void app1s_NotebookPage(int i)
	{
		if(i == app1s_PAGE_IMPORT_RESULT)
			app1s_hbuttonbox_page4.Sensitive = false;

		app1s_notebook.CurrentPage = i;
	}

	// ---- notebook page 1 buttons ----
	void app1s_on_button_cancel1_clicked (object o, EventArgs args)
	{
		app1s_notebook.CurrentPage = app1s_PAGE_MODES;

		reloadSession(); //explained at top of the file.

		//to not allow to load a session or create a new session until close session/more
		menus_sensitive_import_not_danger(false);
	}
	protected void app1s_on_button_select_file_import_clicked(object o, EventArgs args) {
		app1s_chooseDatabaseToImport ();
	}
	protected void app1s_on_button_select_file_import_same_database_clicked(object o, EventArgs args) {
		app1s_notebook.CurrentPage = app1s_PAGE_SELECT_SESSION;
	}

	// ---- notebook page 2 (load sesion) buttons ----

	private void on_app1s_button_edit_clicked (object o, EventArgs args)
	{
		/*
		 *
		 * hi ha dos opcions, o fem load de la sessió aquell, fem el edit i tornem després:
		 * o cridem al sessionAddEdit amb opcio de edit des de fora
		 * i allà treiem totes les refs a currentSession, i que usi algo temp

		 */
		Session s = SqliteSession.Select (app1s_selected);

		//just care if for any reason the session cannot be found
		if(s.UniqueID == -1)
			return;

		if(s.Name == Constants.SessionSimulatedName)
			new DialogMessage(Constants.MessageTypes.INFO, Constants.SessionProtectedStr());
		else {
			sessionAddEditUseSession (s);
			sessionAddEditShow (App1saeModes.EDITOTHERSESSION);
		}
	}

	private void on_app1s_button_delete_clicked (object o, EventArgs args)
	{
		Session s = SqliteSession.Select (app1s_selected);

		//just care if for any reason the session cannot be found
		if(s.UniqueID == -1)
			return;

		if(s.Name == Constants.SessionSimulatedName)
			new DialogMessage(Constants.MessageTypes.INFO, Constants.SessionProtectedStr());
		else {
			deleteSessionCalledFromLoad = true;
			tempDeletingSession = s;

			on_app1s_delete_session_confirm_start_do ();
		}
	}

	void app1s_on_button_cancel2_clicked (object o, EventArgs args)
	{
		menus_and_mode_sensitive(true);
		notebook_supSetOldPage();
	}

	void app1s_on_row_double_clicked (object o, Gtk.RowActivatedArgs args)
	{
		TreeView tv = (TreeView) o;
		TreeModel model;
		TreeIter iter;

		LogB.Information("double! type: " + app1s_type.ToString());
		if (tv.Selection.GetSelected (out model, out iter)) {
			//put selection in selected
			app1s_selected = (string) model.GetValue (iter, 0);

			if (app1s_type == app1s_windowType.LOAD_SESSION) {
				//activate on_button_load_clicked()
				app1s_button_load.Activate();
			} else {
				app1s_button_import.Activate();
			}
		}
	}
	
	private void app1s_on_button_load_clicked (object o, EventArgs args)
	{
		if(app1s_selected != "-1")
		{
			app1s_notebook_load_button_animation.CurrentPage = 1;
			GLib.Timeout.Add(1000, new GLib.TimeoutHandler(app1s_on_button_load_clicked_do));
		}
	}
	private bool app1s_on_button_load_clicked_do ()
	{
		currentSession = SqliteSession.Select (app1s_selected);
		on_load_session_accepted();
		notebook_supSetOldPage();
		app1s_notebook_load_button_animation.CurrentPage = 0;

		return false; //do not call this again
	}

	private void on_button_sessions_raspberry_up_clicked (object o, EventArgs args)
	{
		Gtk.Scrollbar sb = (Gtk.Scrollbar) scrolledwin_session_load.VScrollbar;
		sb.Value -= sb.Adjustment.PageIncrement; //or StepIncrement if want small steps
	}
	private void on_button_sessions_raspberry_down_clicked (object o, EventArgs args)
	{
		Gtk.Scrollbar sb = (Gtk.Scrollbar) scrolledwin_session_load.VScrollbar;
		sb.Value += sb.Adjustment.PageIncrement;
	}

	private void on_app1s_button_manage_tags_clicked (object o, EventArgs args)
	{
		if(app1s_selected == "" || app1s_selected == "-1")
			return;

		int sessionID = app1s_CurrentSessionId();
		if(sessionID == -1)
			return;

		tagSessionSelect = new TagSessionSelect();

		tagSessionSelect.PassVariables(false, sessionID, app1s_CurrentSessionName(), preferences.askDeletion);

		tagSessionSelect.FakeButtonDone.Clicked -= new EventHandler(on_select_tags_clicked_done_loadSession);
		tagSessionSelect.FakeButtonDone.Clicked += new EventHandler(on_select_tags_clicked_done_loadSession);

		tagSessionSelect.Do();
		tagSessionSelect.Show();
	}
	private void on_select_tags_clicked_done_loadSession (object o, EventArgs args)
	{
		tagSessionSelect.FakeButtonDone.Clicked -= new EventHandler(on_select_tags_clicked_done_loadSession);

		//update combo tags
		createComboSessionLoadTags (false);

		app1s_recreateTreeView("changed tags on manage");
	}

	// ---- notebook page 3 (import) buttons ----
	void app1s_on_button_back_clicked (object o, EventArgs args)
	{
		app1s_notebook.CurrentPage = app1s_PAGE_IMPORT_START;
	}
	void app1s_on_button_import_clicked (object o, EventArgs args)
	{
		if(app1s_selected != "-1")
			on_load_session_accepted_to_import(o, args);
	}

	// ---- notebook page 4 buttons ----
	private void app1s_on_button_import_confirm_back_clicked(object o, EventArgs args)
	{
		app1s_notebook.CurrentPage = app1s_PAGE_IMPORT_START;
	}
	private void app1s_on_button_import_confirm_accept_clicked(object o, EventArgs args)
	{
		app1s_hbuttonbox_page4.Sensitive = false;
		app1s_notebook.CurrentPage = app1s_PAGE_IMPORT_RESULT;
		importSessionFromDatabasePrepare2 (new object(), new EventArgs());
	}

	//import notebook page 4 buttons
	private void on_app1s_button_import_at_new_done_do_load_clicked (object o, EventArgs args)
	{
		//ID has to be the last one, get the last session
		List<Session> session_l = SqliteSession.SelectAll(false, Sqlite.Orders_by.ID_DESC);
		if(session_l == null && session_l.Count == 0)
			return;

		currentSession = session_l[0];
		on_load_session_accepted();
		notebook_supSetOldPage();
	}

	private void app1s_on_button_import_close_clicked(object o, EventArgs args)
	{
		app1s_notebook.CurrentPage = app1s_PAGE_MODES;
	}

	private void app1s_on_button_import_again_clicked(object o, EventArgs args)
	{
		app1s_hbox_import_done_at_new_session.Visible = false;
		app1s_label_import_done_at_current_session.Visible = false;
		app1s_scrolledwindow_import_error.Visible = false;

		app1s_radio_import_new_current_sensitive();
		app1s_label_open_database_file.Text = Catalog.GetString("Open another database");
		app1s_button_select_file_import_same_database.Visible = true;

		app1s_notebook.CurrentPage = app1s_PAGE_IMPORT_START;
	}

	public Button app1s_Button_load
	{
		set { app1s_button_load = value; }
		get { return app1s_button_load; }
	}
}


/*
   manage if show athletes info, jumps/races or other tests
1: athletes
2: jumps/races
4: other tests

eg 6 will be jumps and races
tested with:
for(int i = 0; i <= 7; i++)
	LogB.Information(new SessionLoadDisplay(i).ToString());
*/
public class SessionLoadDisplay : BooleansInt
{
//	private int selection;

	//constructor when we have the 0-7 value
	public SessionLoadDisplay(int selection)
	{
		this.i = selection;
	}

	//constructo with the 3 booleans
	public SessionLoadDisplay(bool showBit1, bool showBit2, bool showBit3)
	{
		this.i = 0;
		if(showBit1)
			i ++;
		if(showBit2)
			i += 2;
		if(showBit3)
			i += 4;
	}

	public bool ShowOtherTests
	{
		get { return Bit3; }
	}

	public bool ShowJumpsRaces
	{
		get { return Bit2; }
	}

	public bool ShowAthletesInfo
	{
		get { return Bit1; }
	}

	//just to debug
	public override string ToString()
	{
		return string.Format("selected: {0} (AthletesInfo: {1}, JumpsRaces: {2}, Other: {3})",
				i, ShowAthletesInfo, ShowJumpsRaces, ShowOtherTests);
	}
}
