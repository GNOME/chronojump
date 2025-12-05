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
 * Copyright (C) 2018-2025   Xavier de Blas <xaviblas@gmail.com>
 */

//this file has methods of ChronoJumpWindow related to manage persons

using System;
using Gtk;
using Gdk;
//using Glade;
using System.Collections.Generic; //List
using System.IO; //"File" things
using System.Collections; //ArrayList
using Mono.Unix;

public partial class ChronoJumpWindow
{
	Gtk.Alignment alignment_session_persons;
	Gtk.EventBox eventbox_button_person_close;
	Gtk.Image image_person_manage_blue;
	Gtk.Image image_person_manage_yellow;
	Gtk.Notebook notebook_persons_export;
	Gtk.Button button_person_merge;
	Gtk.SearchEntry person_search;

	/* ---------------------------------------------------------
	 * ----------------  TREEVIEW PERSONS ----------------------
	 *  --------------------------------------------------------
	 */

	private void createTreeView_persons (Gtk.TreeView tv) {
		myTreeViewPersons = new TreeViewPersons(
				tv,
				preferences.personClubID,
				current_mode,
				get_configured_rest_time_in_seconds());
		tv.Selection.Changed += onTreeviewPersonsSelectionEntry;
	}

	private void fillTreeView_persons ()
	{
		if (currentSession == null)
		{
			currentPerson = null;
			sensitiveGuiNoPerson ();
			return;
		}

		ArrayList myPersons = SqlitePersonSession.SelectCurrentSessionPersons(
				currentSession.UniqueID, 
				false, //means: do not returnPersonAndPSlist
				person_search.Text);

		if(myPersons.Count > 0) {
			//fill treeview
			myTreeViewPersons.Fill(myPersons, restTime);

			//if filter found nothing previously, current person is null and treeview is unsensitive. Fix it now.
			if (currentPerson == null)
			{
				if (selectRowTreeView_persons (treeview_persons, 0))
					sensitiveGuiYesPerson ();
			}
			else {
				// if currentPerson is not on the treeview, then select the first one on the treeview
				if (myTreeViewPersons.FindRow (currentPerson.UniqueID) >= 0)
					myTreeViewPersons.SelectRowByUniqueID (currentPerson.UniqueID);
				else
					selectRowTreeView_persons (treeview_persons, 0);
			}
		} else {
			currentPerson = null;
			sensitiveGuiNoPerson ();
		}
	}

	private void on_treeview_persons_up (object o, EventArgs args) {
		myTreeViewPersons.SelectPreviousRow(currentPerson.UniqueID);
	}
	
	private void on_treeview_persons_down (object o, EventArgs args) {
		myTreeViewPersons.SelectNextRow(currentPerson.UniqueID);
	}

	private void on_person_search_search_changed (object o, EventArgs args)
	{
		LogB.Information ("searching: " + person_search.Text);
		treeview_persons_storeReset();
		fillTreeView_persons ();
	}

	//return true if selection is done (there's any person)
	//this updates currentPerson & currentPersonSession
	private bool selectRowTreeView_persons(Gtk.TreeView tv, int rowNum)
	{
		LogB.Information("selectRowTreeView_persons rowNum: " + rowNum.ToString ());

		if(! myTreeViewPersons.SelectRow(rowNum))
			return false;
		
		LogB.Information("selectRowTreeView_persons B");
		//the selection of row in treeViewPersons.SelectRow is not a real selection 
		//and unfortunately doesn't raises the on_treeview_persons_cursor_changed ()
		//for this reason we reproduce the method here
		ITreeModel model;
		TreeIter iter;
		if (tv.Selection.GetSelected (out model, out iter)) {
			string selectedID = (string) model.GetValue (iter, 0); //ID, Name
			currentPerson = SqlitePerson.Select(Convert.ToInt32(selectedID));
			currentPersonSession = SqlitePersonSession.Select(Convert.ToInt32(selectedID), currentSession.UniqueID);
			label_person_change();
			TreePath path = model.GetPath (iter);
			tv.ScrollToCell (path, null, true, 0, 0);

			LogB.Information("selectRowTreeView_persons C");
			return true;
		} else {
			return false;
		}
	}

	private void treeview_persons_storeReset()
	{
		myTreeViewPersons.RemoveColumns();
		myTreeViewPersons = new TreeViewPersons (
				treeview_persons,
				preferences.personClubID,
				current_mode,
				get_configured_rest_time_in_seconds());
	}

	// note on start page we also care about current_mode to show correctly the selected mode radios.
	// so in start page mode is UNDEFINED only at start
	// here send UNDEFINED as mode in order to not show n column
	private void treeview_persons_storeReset_start_page ()
	{
		myTreeViewPersons.RemoveColumns();
		myTreeViewPersons = new TreeViewPersons (
				treeview_persons,
				preferences.personClubID,
				Constants.Modes.UNDEFINED,
				get_configured_rest_time_in_seconds());
	}

	// to avoid circular calls
	// if treeview_person changes, treeviewResultsSession changes. This boolean is to not change again treeview_persons
	private bool treeviewResultsSessionNoCheckPersonChange;

	// on_treeview_results_session_cursor_changed on forceSensor & raceAnalyzer makes load the set.
	// In the middle of the load process, combo_force_sensor_exercise.Active changes and this makes redo the treeview making fail the load.
	// With this variable we deactivate treeViewResultsSession.Fill while loading the set
	private bool pre_fillTreeView_resultsSession_NO;

	// this is the cursor_changed of treeview_persons
	// Important! see: diagrams/processes/person_results_changes.dia
	private void onTreeviewPersonsSelectionEntry (object o, EventArgs args)
	{
		ITreeModel model;
		TreeIter iter;

		// you get the iter and the model if something is selected
		if (((TreeSelection)o).GetSelected(out model, out iter))
		{
			string selectedID = (string) model.GetValue (iter, 0); //ID, Name
		
			currentPerson = SqlitePerson.Select(Convert.ToInt32(selectedID));
			currentPersonSession = SqlitePersonSession.Select(Convert.ToInt32(selectedID), currentSession.UniqueID);
			label_person_change();

			personChanged();

			button_persons_up.Sensitive = ! myTreeViewPersons.IsFirst(currentPerson.UniqueID);
			button_persons_down.Sensitive = ! myTreeViewPersons.IsLast(currentPerson.UniqueID);
		}
	}

	private void showPersonsOnTop (bool onTop)
	{
		hbox_top_person.Visible = onTop;

		if(onTop)
		{
			alignment_session_persons.Visible = false;
			vbox_menu_tiny.Visible = true;
		} else {
			alignment_session_persons.Visible = true;
			vbox_menu_tiny.Visible = false;
		}
	}

	private void showPersonPhoto (bool showPhoto)
	{
		hbox_persons_bottom_photo.Visible = showPhoto;
	}

	private void label_person_change()
	{
		label_top_person_name.Text = "<b>" + currentPerson.Name + "</b>";
		label_top_person_name.UseMarkup = true;

		personsPhotoShowIfNeeded ();
	}

	private void personsPhotoShowIfNeeded ()
	{
		if (currentPerson == null)
			return;

		string filenameMini = Util.UserPhotoURL(true, currentPerson.UniqueID);
		if(filenameMini != "" && Util.FileExists(filenameMini))
		{
			Pixbuf pixbuf = Chronojump.MyPixbuf.Get(filenameMini);
			image_current_person.Pixbuf = pixbuf;
		} else {
			//image_current_person.Pixbuf = null;
			Pixbuf pixbuf = Chronojump.MyPixbuf.Get(null, Util.GetImagePath(false) + "image_no_photo.png");
			if(Config.ColorBackgroundIsDark)
				pixbuf = Chronojump.MyPixbuf.Get(null, Util.GetImagePath(false) + "image_no_photo_yellow.png");
			image_current_person.Pixbuf = pixbuf;
		}
	}

	/* ---------------------------------------------------------
	 * ----------------  PERSON RECUPERATE, LOAD, EDIT, DELETE -
	 *  --------------------------------------------------------
	 */
	
	bool person_load_single_called_from_person_select_window;
	private void on_recuperate_person_from_main_gui (object o, EventArgs args)
	{
		person_load_single_called_from_person_select_window = false;
		person_load_single();
	}

	private void person_load_single ()
	{
		LogB.Information("recuperate person");
		personRecuperateWin = PersonRecuperateWindow.Show(app1, currentSession, preferences.digitsNumber);
		personRecuperateWin.FakeButtonDone.Clicked += new EventHandler(on_recuperate_person_accepted);

		if (person_load_single_called_from_person_select_window)
		{
			personRecuperateWin.FakeButtonCancel.Clicked -= new EventHandler (on_button_top_person_clicked);
			personRecuperateWin.FakeButtonCancel.Clicked += new EventHandler (on_button_top_person_clicked);
		}
	}

	private void on_recuperate_person_accepted (object o, EventArgs args) {
		LogB.Information("at: on_recuperate_person_accepted");
		currentPerson = personRecuperateWin.CurrentPerson;
		currentPersonSession = personRecuperateWin.CurrentPersonSession;
		label_person_change();
		
		myTreeViewPersons.Add (currentPerson);

		int rowToSelect = myTreeViewPersons.FindRow(currentPerson.UniqueID);
		if(rowToSelect != -1) {
			selectRowTreeView_persons(treeview_persons, rowToSelect);
			sensitiveGuiYesPerson();
		}

		if(person_load_single_called_from_person_select_window)
		{
			personRecuperateWin.HideAndNull();
			updatePersonSelectWin ();
		}
	}
		
	bool person_load_multiple_called_from_person_select_window;
	private void on_recuperate_persons_from_session_at_main_gui (object o, EventArgs args)
	{
		person_load_multiple_called_from_person_select_window = false;
		person_load_multiple();
	}

	private void person_load_multiple ()
	{
		LogB.Information("recuperate persons from other session");
		personsRecuperateFromOtherSessionWin = PersonsRecuperateFromOtherSessionWindow.Show(app1, currentSession);
		personsRecuperateFromOtherSessionWin.FakeButtonDone.Clicked += new EventHandler(on_recuperate_persons_from_session_accepted);

		if (person_load_multiple_called_from_person_select_window)
		{
			personsRecuperateFromOtherSessionWin.FakeButtonCancel.Clicked -= new EventHandler (on_button_top_person_clicked);
			personsRecuperateFromOtherSessionWin.FakeButtonCancel.Clicked += new EventHandler (on_button_top_person_clicked);
		}
	}
	
	private void on_recuperate_persons_from_session_accepted (object o, EventArgs args)
	{
		currentPerson = personsRecuperateFromOtherSessionWin.CurrentPerson;
		currentPersonSession = personsRecuperateFromOtherSessionWin.CurrentPersonSession;
		label_person_change();

		treeview_persons_storeReset();
		fillTreeView_persons();
		int rowToSelect = myTreeViewPersons.FindRow(currentPerson.UniqueID);
		if(rowToSelect != -1) {
			selectRowTreeView_persons(treeview_persons, rowToSelect);
			sensitiveGuiYesPerson();
		}

		if(person_load_multiple_called_from_person_select_window)
		{
			personsRecuperateFromOtherSessionWin.HideAndNull();
			updatePersonSelectWin ();
		}
	}

	bool person_add_single_called_from_person_select_window;
	private void on_person_add_single_from_main_gui (object o, EventArgs args)
	{
		person_add_single_called_from_person_select_window = false;
		person_add_single();
	}

	private void person_add_single ()
	{
		personAddModifyWin = PersonAddModifyWindow.Show(app1,
				currentSession, new Person(-1), 
				//preferences.digitsNumber, checkbutton_video, configChronojump.UseVideo,
				preferences.digitsNumber,// checkbutton_video_contacts,
				preferences.videoDevice, preferences.videoDevicePixelFormat, preferences.videoDeviceResolution, preferences.videoDeviceFramerate,
				configChronojump.Compujump, preferences.units == Preferences.UnitsEnum.METRIC, preferences.machineID
				);
		//-1 means we are adding a new person
		//if we were modifying it will be it's uniqueID
		
		personAddModifyWin.FakeButtonAccept.Clicked -= new EventHandler(on_person_add_single_accepted);
		personAddModifyWin.FakeButtonAccept.Clicked += new EventHandler(on_person_add_single_accepted);

		if (person_add_single_called_from_person_select_window)
		{
			personAddModifyWin.FakeButtonCancel.Clicked -= new EventHandler (on_button_top_person_clicked);
			personAddModifyWin.FakeButtonCancel.Clicked += new EventHandler (on_button_top_person_clicked);
		}
	}

	/*
	 * note: while adding, if a person name is written,
	 * and this name exists in database but not in current session,
	 * a person load will appear
	 * and if clicked, this will be called, so this will be used also as a loader
	 * TODO: unify most of the code of person add and person load
	 */
	private void on_person_add_single_accepted (object o, EventArgs args)
	{
		personAddModifyWin.FakeButtonAccept.Clicked -= new EventHandler(on_person_add_single_accepted);
		if (personAddModifyWin.CurrentPerson != null)
		{
			currentPerson = personAddModifyWin.CurrentPerson;
			currentPersonSession = SqlitePersonSession.Select(currentPerson.UniqueID, currentSession.UniqueID);

			if(personAddModifyWin.Units != preferences.units) {
				preferences.units = personAddModifyWin.Units;
				SqlitePreferences.Update (SqlitePreferences.UnitsStr, personAddModifyWin.Units.ToString(), false);
			}

			person_added();
		}
	}

	private void person_added ()
	{
		label_person_change();
		myTreeViewPersons.Add (currentPerson);

		//when adding new person, photos cannot be recorded as currentPerson.UniqueID
		//because it was undefined. Copy them now
		if(File.Exists(Util.GetPhotoTempFileName(false)) && File.Exists(Util.GetPhotoTempFileName(true))) {
			try {
				File.Move(Util.GetPhotoTempFileName(false),
						Util.GetPhotoFileName(false, currentPerson.UniqueID));
			} catch {
				File.Copy(Util.GetPhotoTempFileName(false),
						Util.GetPhotoFileName(false, currentPerson.UniqueID), true);
			}
			try {
				File.Move(Util.GetPhotoTempFileName(true),
						Util.GetPhotoFileName(true, currentPerson.UniqueID));
			} catch {
				File.Copy(Util.GetPhotoTempFileName(true),
						Util.GetPhotoFileName(true, currentPerson.UniqueID), true);
			}
		}

		int rowToSelect = myTreeViewPersons.FindRow(currentPerson.UniqueID);
		if(rowToSelect != -1) {
			selectRowTreeView_persons(treeview_persons, rowToSelect);
			sensitiveGuiYesPerson();
			//appbar2.Push( 1, Catalog.GetString("Successfully added") + " " + currentPerson.Name );
		}

		if(person_add_single_called_from_person_select_window)
			updatePersonSelectWin ();
	}

	private void updatePersonSelectWin ()
	{
		ArrayList myPersons = SqlitePersonSession.SelectCurrentSessionPersons(
				currentSession.UniqueID,
				false); //means: do not returnPersonAndPSlist
		personSelectWin.Update(myPersons, currentPerson);
	}

	bool person_add_multiple_called_from_person_select_window;
	//show spinbutton window asking for how many people to create	
	private void on_person_add_multiple_from_main_gui (object o, EventArgs args)
	{
		person_add_multiple_called_from_person_select_window = false;
		person_add_multiple();
	}

	private void person_add_multiple ()
	{
		personAddMultipleWin = PersonAddMultipleWindow.Show(app1, currentSession, preferences.CSVColumnDelimiter, preferences.machineID);
		personAddMultipleWin.FakeButtonDone.Clicked -= new EventHandler(on_person_add_multiple_accepted);
		personAddMultipleWin.FakeButtonDone.Clicked += new EventHandler(on_person_add_multiple_accepted);

		if (person_add_multiple_called_from_person_select_window)
		{
			personAddMultipleWin.FakeButtonCancel.Clicked -= new EventHandler (on_button_top_person_clicked);
			personAddMultipleWin.FakeButtonCancel.Clicked += new EventHandler (on_button_top_person_clicked);
		}
	}
	
	private void on_person_add_multiple_accepted (object o, EventArgs args)
	{
		personAddMultipleWin.FakeButtonDone.Clicked -= new EventHandler(on_person_add_multiple_accepted);
		if (personAddMultipleWin.CurrentPerson != null)
		{
			currentPerson = personAddMultipleWin.CurrentPerson;
			currentPersonSession = SqlitePersonSession.Select(currentPerson.UniqueID, currentSession.UniqueID);
			label_person_change();
			treeview_persons_storeReset();
			fillTreeView_persons();
			int rowToSelect = myTreeViewPersons.FindRow(currentPerson.UniqueID);
			if(rowToSelect != -1) {
				selectRowTreeView_persons(treeview_persons, rowToSelect);
				sensitiveGuiYesPerson();
			
				// string myString = string.Format(
				//		Catalog.GetPluralString(
				//			"Successfully added one person.", 
				//			"Successfully added {0} persons.", 
				//			personAddMultipleWin.PersonsCreatedCount),
				//		personAddMultipleWin.PersonsCreatedCount);
				//appbar2.Push( 1, Catalog.GetString(myString) );
			}

			if(person_add_multiple_called_from_person_select_window)
				updatePersonSelectWin ();
		}
	}
	
	bool person_edit_single_called_from_person_select_window;
	private void on_edit_current_person_clicked_from_main_gui (object o, EventArgs args) {
		person_edit_single_called_from_person_select_window = false;
		person_edit_single();
	}

	private void person_edit_single() {
		LogB.Information("modify person");

		//just caution if any widget is not unsensitive in the future
		if (currentPerson == null)
			return;

		personAddModifyWin = PersonAddModifyWindow.Show(app1, currentSession, currentPerson, 
				//preferences.digitsNumber, checkbutton_video, configChronojump.UseVideo,
				preferences.digitsNumber,// checkbutton_video_contacts,
				preferences.videoDevice, preferences.videoDevicePixelFormat, preferences.videoDeviceResolution, preferences.videoDeviceFramerate,
				configChronojump.Compujump, preferences.units == Preferences.UnitsEnum.METRIC, preferences.machineID
				); 
		personAddModifyWin.FakeButtonAccept.Clicked += new EventHandler(on_edit_current_person_accepted);

		if (person_edit_single_called_from_person_select_window)
		{
			personAddModifyWin.FakeButtonCancel.Clicked -= new EventHandler (on_button_top_person_clicked);
			personAddModifyWin.FakeButtonCancel.Clicked += new EventHandler (on_button_top_person_clicked);
		}
	}
	
	private void on_edit_current_person_accepted (object o, EventArgs args)
	{
		if (personAddModifyWin.CurrentPerson != null)
		{
			currentPerson = personAddModifyWin.CurrentPerson;
			int personID = currentPerson.UniqueID; //store now to solve problems with filter
			currentPersonSession = SqlitePersonSession.Select(currentPerson.UniqueID, currentSession.UniqueID);

			if(personAddModifyWin.Units != preferences.units) {
				preferences.units = personAddModifyWin.Units;
				SqlitePreferences.Update (SqlitePreferences.UnitsStr, personAddModifyWin.Units.ToString(), false);
			}

			label_person_change();
			treeview_persons_storeReset();
			fillTreeView_persons(); //this is what makes currentPerson == null

			int rowToSelect = -1;
			if (currentPerson == null)
			{
				if (person_search.Text != "")
				{
					/*
					 * person has changed name, the filter showed that person
					 * but now no persons fill that filter, so currentPerson is null
					 * better disable the filter
					 */
					person_search.Text = ""; // this will update the treeview and assign first person
					rowToSelect = myTreeViewPersons.FindRow (personID);
				}
			} else
				rowToSelect = myTreeViewPersons.FindRow (currentPerson.UniqueID);

			if(rowToSelect != -1) {
				selectRowTreeView_persons(treeview_persons, rowToSelect);
				sensitiveGuiYesPerson();
			}

			pre_fillTreeView_resultsSession ();

			if(createdStatsWin) {
				stats_win_fillTreeView_stats(false, true);
			}

//			personAddModifyWin.Destroy();
			
			if(person_edit_single_called_from_person_select_window) {
				ArrayList myPersons = SqlitePersonSession.SelectCurrentSessionPersons(
						currentSession.UniqueID, 
						false); //means: do not returnPersonAndPSlist
				personSelectWin.Update(myPersons, currentPerson);
			}
		}
	}


	private void on_show_all_person_events_activate (object o, EventArgs args)
	{
		Person p = new Person (); //uniqueID = -1
		if (currentPerson != null)
			p = currentPerson;

		personShowAllEventsWin = PersonShowAllEventsWindow.Show(app1,
				currentSession.UniqueID, p, true, preferences.colorBackground);

		personShowAllEventsWin.FakeButtonLoadSession.Clicked += new EventHandler (on_show_all_persons_load_session);
	}

	private void on_show_all_persons_load_session (object o, EventArgs args)
	{
		currentSession = SqliteSession.Select (personShowAllEventsWin.SelectedSessionID.ToString ());

		on_load_session_accepted();

		//on loaded session make selected person the same than in showAllPersonsWin combo
		if (personShowAllEventsWin.SelectedPersonID >= 0)
		{
			int rowToSelect = myTreeViewPersons.FindRow (personShowAllEventsWin.SelectedPersonID);
			if(rowToSelect != -1)
				selectRowTreeView_persons (treeview_persons, rowToSelect);
		}

		personShowAllEventsWin.CloseWindowAfterLoadSession ();
	}

	private void on_delete_current_person_from_session_clicked (object o, EventArgs args)
	{
		//just caution if any widget is not unsensitive in the future
		if (currentPerson == null)
			return;

		LogB.Information("delete current person from this session");
		ConfirmWindow confirmWin = ConfirmWindow.Show(
				Catalog.GetString("Are you sure you want to delete the current person and all his/her tests (jumps, races, pulses, …) from this session?\n(His/her personal data and tests in other sessions will remain intact.)"), "",
				Catalog.GetString("Current Person: ") + "<b>" + currentPerson.Name + "</b>");

		confirmWin.Button_accept.Clicked += new EventHandler(on_delete_current_person_from_session_accepted);
	}
	
	private void on_delete_current_person_from_session_accepted (object o, EventArgs args) 
	{
		new DialogMessage(Constants.MessageTypes.INFO, Catalog.GetString("Deleted person and all his/her tests on this session."));
		SqlitePersonSession.DeletePersonFromSessionAndTests(
				currentSession.UniqueID.ToString(), currentPerson.UniqueID.ToString());

		resetAllTreeViews(true, true, true); //fillTests, resetPersons, fillPersons
		bool foundPersons = selectRowTreeView_persons(treeview_persons, 0);

		if(createdStatsWin) {
			stats_win_fillTreeView_stats(false, true);
		}

		//if there are no persons
		if(! foundPersons)
		{
			currentPerson = null;
			sensitiveGuiNoPerson ();
		}
	}

	private void on_button_top_person_clicked (object o, EventArgs args)
	{
		//if compujump show person profile at server
		if(configChronojump.Compujump)
		{
			on_button_person_popup_clicked (o, args);
			return;
		}

		//personSelectWindow is not modal to allow other windows to show on top
		//but not allow to change session while personSelectWindow is active
		vbox_menu_tiny.Sensitive = false;

		//if not compujump show person change window
		ArrayList myPersons = SqlitePersonSession.SelectCurrentSessionPersons(
				currentSession.UniqueID, 
				false); //means: do not returnPersonAndPSlist

		personSelectWin = PersonSelectWindow.Show(app1, myPersons, currentPerson, preferences.colorBackground,
				configChronojump.Raspberry, configChronojump.LowHeight,
				preferences.personSelectWinImages, configChronojump.ReadFromCloudMainPath != "");
		personSelectWin.FakeButtonAddPerson.Clicked -= new EventHandler(on_button_top_person_add_person);
		personSelectWin.FakeButtonAddPerson.Clicked += new EventHandler(on_button_top_person_add_person);

		personSelectWin.FakeButtonAddPersonMultiple.Clicked -= new EventHandler(on_button_top_person_add_person_multiple);
		personSelectWin.FakeButtonAddPersonMultiple.Clicked += new EventHandler(on_button_top_person_add_person_multiple);

		personSelectWin.FakeButtonLoadPerson.Clicked -= new EventHandler(on_button_top_person_load_person);
		personSelectWin.FakeButtonLoadPerson.Clicked += new EventHandler(on_button_top_person_load_person);

		personSelectWin.FakeButtonLoadPersonMultiple.Clicked -= new EventHandler(on_button_top_person_load_person_multiple);
		personSelectWin.FakeButtonLoadPersonMultiple.Clicked += new EventHandler(on_button_top_person_load_person_multiple);

		personSelectWin.FakeButtonEditPerson.Clicked -= new EventHandler(on_button_top_person_edit_person);
		personSelectWin.FakeButtonEditPerson.Clicked += new EventHandler(on_button_top_person_edit_person);

		personSelectWin.FakeButtonPersonShowAllEvents.Clicked -= new EventHandler(on_button_top_person_show_all_events);
		personSelectWin.FakeButtonPersonShowAllEvents.Clicked += new EventHandler(on_button_top_person_show_all_events);

		personSelectWin.FakeButtonPersonMerge.Clicked -= new EventHandler(on_button_top_person_merge);
		personSelectWin.FakeButtonPersonMerge.Clicked += new EventHandler(on_button_top_person_merge);

		personSelectWin.FakeButtonDeletePerson.Clicked -= new EventHandler(on_button_top_person_delete_person);
		personSelectWin.FakeButtonDeletePerson.Clicked += new EventHandler(on_button_top_person_delete_person);

		personSelectWin.FakeButtonShowImages.Clicked -= new EventHandler(on_button_top_person_show_images);
		personSelectWin.FakeButtonShowImages.Clicked += new EventHandler(on_button_top_person_show_images);

		personSelectWin.FakeButtonHideImages.Clicked -= new EventHandler(on_button_top_person_hide_images);
		personSelectWin.FakeButtonHideImages.Clicked += new EventHandler(on_button_top_person_hide_images);

		personSelectWin.FakeButtonDone.Clicked -= new EventHandler(on_button_top_person_change_done); //on window close, and on double click (it also closes the window)
		personSelectWin.FakeButtonDone.Clicked += new EventHandler(on_button_top_person_change_done); //on window close, and on double click (it also closes the window)
	}
	private void on_button_top_person_add_person(object o, EventArgs args)
	{
		person_add_single_called_from_person_select_window = true;
		person_add_single();
	}
	private void on_button_top_person_add_person_multiple(object o, EventArgs args)
	{
		person_add_multiple_called_from_person_select_window = true;
		person_add_multiple();
	}
	private void on_button_top_person_load_person(object o, EventArgs args)
	{
		person_load_single_called_from_person_select_window = true;
		person_load_single();
	}
	private void on_button_top_person_load_person_multiple(object o, EventArgs args)
	{
		person_load_multiple_called_from_person_select_window = true;
		person_load_multiple();
	}
	private void on_button_top_person_edit_person(object o, EventArgs args)
	{
		currentPerson = personSelectWin.SelectedPerson; 
		personChanged();
		
		person_edit_single_called_from_person_select_window = true;
		person_edit_single();
	}

	private void on_button_top_person_show_all_events (object o, EventArgs args)
	{
		personShowAllEventsWin = PersonShowAllEventsWindow.Show(app1,
				currentSession.UniqueID, currentPerson, false, preferences.colorBackground);
		personShowAllEventsWin.FakeButtonDoneCalledFromTop.Clicked -= new EventHandler(on_person_show_all_persons_event_close);
		personShowAllEventsWin.FakeButtonDoneCalledFromTop.Clicked += new EventHandler(on_person_show_all_persons_event_close);
	}
	private void on_person_show_all_persons_event_close (object o, EventArgs args)
	{
		personShowAllEventsWin.FakeButtonDoneCalledFromTop.Clicked -= new EventHandler(on_person_show_all_persons_event_close);

		ArrayList myPersons = SqlitePersonSession.SelectCurrentSessionPersons(
				currentSession.UniqueID,
				false); //means: do not returnPersonAndPSlist
		personSelectWin.Update(myPersons);
	}

	bool person_merge_called_from_person_select_window;
	private void on_button_top_person_merge (object o, EventArgs args)
	{
		LogB.Information ("called on_button_top_person_merge");
		//assign currentPerson, ... but if there is a problem, just reshow the top_person window
		if (! assignPersonFromTopWindow ())
		{
			on_button_top_person_clicked (o, args);
			return;
		}

		person_merge_called_from_person_select_window = true;
		person_merge_do ();
	}

	private void on_button_top_person_delete_person(object o, EventArgs args)
	{
		LogB.Information ("called on_button_top_person_delete_person");
		currentPerson = personSelectWin.SelectedPerson;

		//without confirm, because it's already confirmed on PersonSelect
		on_delete_current_person_from_session_accepted (o, args);

		ArrayList myPersons = SqlitePersonSession.SelectCurrentSessionPersons(
				currentSession.UniqueID, 
				false); //means: do not returnPersonAndPSlist
		personSelectWin.Update(myPersons, currentPerson);

		if (personSelectWin.SelectedPerson == null)
			LogB.Information ("personSelectWin.SelectedPerson == null");
		else
			LogB.Information ("selected person is now: " + personSelectWin.SelectedPerson.ToString ());

		if (currentPerson == null)
			LogB.Information ("currentPerson == null");
		else
			LogB.Information (currentPerson.ToString ());
	}

	private void on_button_top_person_show_images (object o, EventArgs args)
	{
		preferences.personSelectWinImages = Preferences.PreferencesChange (false, SqlitePreferences.PersonSelectWinImages, false, true);
	}
	private void on_button_top_person_hide_images (object o, EventArgs args)
	{
		preferences.personSelectWinImages = Preferences.PreferencesChange(false, SqlitePreferences.PersonSelectWinImages, true, false);
	}

	private void on_button_top_person_change_done(object o, EventArgs args)
	{
		vbox_menu_tiny.Sensitive = true;
		assignPersonFromTopWindow ();
	}

	private bool assignPersonFromTopWindow ()
	{
		if(personSelectWin.SelectedPerson == null)
			return false;

		if(currentPerson.UniqueID == personSelectWin.SelectedPerson.UniqueID)
			return true; //no need to do the rest of the method

		currentPerson = personSelectWin.SelectedPerson; 
		currentPersonSession = SqlitePersonSession.Select(currentPerson.UniqueID, currentSession.UniqueID);
		label_person_change();

		personChanged();
		myTreeViewPersons.SelectRowByUniqueID(currentPerson.UniqueID);
		return true;
	}

	private void on_button_persons_raspberry_left_clicked(object o, EventArgs args)
	{
		hpaned_main.Position -= 10;
	}
	private void on_button_persons_raspberry_right_clicked(object o, EventArgs args)
	{
		hpaned_main.Position += 10;
	}

	private void connectWidgetsPersons (Gtk.Builder builder)
	{
		alignment_session_persons = (Gtk.Alignment) builder.GetObject ("alignment_session_persons");
		eventbox_button_person_close = (Gtk.EventBox) builder.GetObject ("eventbox_button_person_close");
		image_person_manage_blue = (Gtk.Image) builder.GetObject ("image_person_manage_blue");
		image_person_manage_yellow = (Gtk.Image) builder.GetObject ("image_person_manage_yellow");
		notebook_persons_export = (Gtk.Notebook) builder.GetObject ("notebook_persons_export");
		button_person_merge = (Gtk.Button) builder.GetObject ("button_person_merge");
		person_search = (Gtk.SearchEntry) builder.GetObject ("person_search");

		connectWidgetsPersonsExport (builder);
	}
}
