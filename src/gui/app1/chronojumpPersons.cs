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
 * Copyright (C) 2018-2020   Xavier de Blas <xaviblas@gmail.com> 
 */

//this file has methods of ChronoJumpWindow related to manage persons

using System;
using Gtk;
using Gdk;
using Glade;
using System.Collections.Generic; //List
using System.IO; //"File" things
using System.Collections; //ArrayList
using Mono.Unix;

public partial class ChronoJumpWindow
{
	[Widget] Gtk.VBox vbox_manage_persons;
	[Widget] Gtk.Arrow arrow_manage_persons_up;
	[Widget] Gtk.Arrow arrow_manage_persons_down;

	private void showPersonsOnTop (bool onTop)
	{
		hbox_top_person.Visible = onTop;
		hbox_top_person_encoder.Visible = onTop;

		if(onTop)
		{
			alignment_viewport_menu_top.Visible = false;
			vbox_menu_tiny.Visible = true;
		} else {
			alignment_viewport_menu_top.Visible = true;
			vbox_menu_tiny.Visible = false;
		}
	}

	private void on_button_manage_persons_clicked (object o, EventArgs args)
	{
		vbox_manage_persons.Visible = ! vbox_manage_persons.Visible;

		arrow_manage_persons_up.Visible = vbox_manage_persons.Visible;
		arrow_manage_persons_down.Visible = ! vbox_manage_persons.Visible;
	}

	private void showPersonPhoto (bool showPhoto)
	{
		hbox_persons_bottom_photo.Visible = showPhoto;
		hbox_persons_bottom_no_photo.Visible = ! showPhoto;
	}

	private void label_person_change()
	{
		label_top_person_name.Text = "<b>" + currentPerson.Name + "</b>";
		label_top_person_name.UseMarkup = true;

		label_top_encoder_person_name.Text = "<b>" + currentPerson.Name + "</b>";
		label_top_encoder_person_name.UseMarkup = true;

		string filenameMini = Util.UserPhotoURL(true, currentPerson.UniqueID);
		if(filenameMini != "")
		{
			Pixbuf pixbuf = new Pixbuf (filenameMini);
			image_current_person.Pixbuf = pixbuf;
			button_image_current_person_zoom.Sensitive = true;
			button_image_current_person_zoom_h.Sensitive = true;
		} else {
			//image_current_person.Pixbuf = null;
			Pixbuf pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_no_photo.png");
			image_current_person.Pixbuf = pixbuf;
			button_image_current_person_zoom.Sensitive = false;
			button_image_current_person_zoom_h.Sensitive = false;
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
	}

	private void on_recuperate_person_accepted (object o, EventArgs args) {
		LogB.Information("at: on_recuperate_person_accepted");
		currentPerson = personRecuperateWin.CurrentPerson;
		currentPersonSession = personRecuperateWin.CurrentPersonSession;
		label_person_change();
		
		myTreeViewPersons.Add(currentPerson.UniqueID.ToString(), currentPerson.Name);

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

	private void person_load_multiple () {
		LogB.Information("recuperate persons from other session");
		personsRecuperateFromOtherSessionWin = PersonsRecuperateFromOtherSessionWindow.Show(app1, currentSession);
		personsRecuperateFromOtherSessionWin.FakeButtonDone.Clicked += new EventHandler(on_recuperate_persons_from_session_accepted);
	}
	
	private void on_recuperate_persons_from_session_accepted (object o, EventArgs args) {
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
				preferences.digitsNumber, checkbutton_video_contacts,
				preferences.videoDevice, preferences.videoDevicePixelFormat, preferences.videoDeviceResolution, preferences.videoDeviceFramerate,
				configChronojump.Compujump, preferences.units == Preferences.UnitsEnum.METRIC, preferences.colorBackground
				);
		//-1 means we are adding a new person
		//if we were modifying it will be it's uniqueID
		
		personAddModifyWin.FakeButtonAccept.Clicked -= new EventHandler(on_person_add_single_accepted);
		personAddModifyWin.FakeButtonAccept.Clicked += new EventHandler(on_person_add_single_accepted);
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
		myTreeViewPersons.Add(currentPerson.UniqueID.ToString(), currentPerson.Name);

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
		personAddMultipleWin = PersonAddMultipleWindow.Show(app1, currentSession, preferences.CSVColumnDelimiter);
		personAddMultipleWin.Button_accept.Clicked -= new EventHandler(on_person_add_multiple_accepted);
		personAddMultipleWin.Button_accept.Clicked += new EventHandler(on_person_add_multiple_accepted);
	}
	
	private void on_person_add_multiple_accepted (object o, EventArgs args)
	{
		personAddMultipleWin.Button_accept.Clicked -= new EventHandler(on_person_add_multiple_accepted);
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

		personAddModifyWin = PersonAddModifyWindow.Show(app1, currentSession, currentPerson, 
				//preferences.digitsNumber, checkbutton_video, configChronojump.UseVideo,
				preferences.digitsNumber, checkbutton_video_contacts,
				preferences.videoDevice, preferences.videoDevicePixelFormat, preferences.videoDeviceResolution, preferences.videoDeviceFramerate,
				configChronojump.Compujump, preferences.units == Preferences.UnitsEnum.METRIC, preferences.colorBackground
				); 
		personAddModifyWin.FakeButtonAccept.Clicked += new EventHandler(on_edit_current_person_accepted);
	}
	
	private void on_edit_current_person_accepted (object o, EventArgs args) {
		if (personAddModifyWin.CurrentPerson != null)
		{
			currentPerson = personAddModifyWin.CurrentPerson;
			currentPersonSession = SqlitePersonSession.Select(currentPerson.UniqueID, currentSession.UniqueID);

			if(personAddModifyWin.Units != preferences.units) {
				preferences.units = personAddModifyWin.Units;
				SqlitePreferences.Update (SqlitePreferences.UnitsStr, personAddModifyWin.Units.ToString(), false);
			}

			label_person_change();
			treeview_persons_storeReset();
			fillTreeView_persons();
			
			int rowToSelect = myTreeViewPersons.FindRow(currentPerson.UniqueID);
			if(rowToSelect != -1) {
				selectRowTreeView_persons(treeview_persons, rowToSelect);
				sensitiveGuiYesPerson();
			}

			on_combo_result_jumps_changed(combo_result_jumps, args);
			on_combo_result_jumps_rj_changed(combo_result_jumps_rj, args);
			on_combo_result_runs_changed(combo_result_runs, args);
			on_combo_result_runs_interval_changed(combo_result_runs_interval, args);
			on_combo_pulses_changed(combo_pulses, args);

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
		personShowAllEventsWin = PersonShowAllEventsWindow.Show(app1, currentSession.UniqueID, currentPerson, true);
	}
	
	
	private void on_delete_current_person_from_session_clicked (object o, EventArgs args) {
		LogB.Information("delete current person from this session");
		ConfirmWindow confirmWin = ConfirmWindow.Show(
				Catalog.GetString("Are you sure you want to delete the current person and all his/her tests (jumps, races, pulses, ...) from this session?\n(His/her personal data and tests in other sessions will remain intact.)"), "",
				Catalog.GetString("Current Person: ") + "<b>" + currentPerson.Name + "</b>");

		confirmWin.Button_accept.Clicked += new EventHandler(on_delete_current_person_from_session_accepted);
	}
	
	private void on_delete_current_person_from_session_accepted (object o, EventArgs args) 
	{
		new DialogMessage(Constants.MessageTypes.INFO, Catalog.GetString("Deleted person and all his/her tests on this session."));
		SqlitePersonSession.DeletePersonFromSessionAndTests(
				currentSession.UniqueID.ToString(), currentPerson.UniqueID.ToString());
		
		resetAllTreeViews(false, true, true); //fill, resetPersons, fillPersons
		bool foundPersons = selectRowTreeView_persons(treeview_persons, 0);
			
		if(createdStatsWin) {
			stats_win_fillTreeView_stats(false, true);
		}
		
		//if there are no persons
		if(! foundPersons) {
			currentPerson = null;
			sensitiveGuiNoPerson ();
			if(createdStatsWin) {
				stats_win_hide();
			}
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

		//if not compujump show person change window
		ArrayList myPersons = SqlitePersonSession.SelectCurrentSessionPersons(
				currentSession.UniqueID, 
				false); //means: do not returnPersonAndPSlist

		personSelectWin = PersonSelectWindow.Show(app1, myPersons, currentPerson);
		personSelectWin.FakeButtonAddPerson.Clicked += new EventHandler(on_button_top_person_add_person);
		personSelectWin.FakeButtonAddPersonMultiple.Clicked += new EventHandler(on_button_top_person_add_person_multiple);
		personSelectWin.FakeButtonLoadPerson.Clicked += new EventHandler(on_button_top_person_load_person);
		personSelectWin.FakeButtonLoadPersonMultiple.Clicked += new EventHandler(on_button_top_person_load_person_multiple);
		personSelectWin.FakeButtonEditPerson.Clicked += new EventHandler(on_button_top_person_edit_person);
		personSelectWin.FakeButtonPersonShowAllEvents.Clicked += new EventHandler(on_button_top_person_show_all_events);
		personSelectWin.FakeButtonDeletePerson.Clicked += new EventHandler(on_button_top_person_delete_person);
		personSelectWin.FakeButtonDone.Clicked += new EventHandler(on_button_top_person_change_done);
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
		personShowAllEventsWin = PersonShowAllEventsWindow.Show(app1, currentSession.UniqueID, currentPerson, false);
		personShowAllEventsWin.FakeButtonDone.Clicked -= new EventHandler(on_person_show_all_persons_event_close);
		personShowAllEventsWin.FakeButtonDone.Clicked += new EventHandler(on_person_show_all_persons_event_close);
	}
	private void on_person_show_all_persons_event_close (object o, EventArgs args)
	{
		personShowAllEventsWin.FakeButtonDone.Clicked -= new EventHandler(on_person_show_all_persons_event_close);

		ArrayList myPersons = SqlitePersonSession.SelectCurrentSessionPersons(
				currentSession.UniqueID,
				false); //means: do not returnPersonAndPSlist
		personSelectWin.Update(myPersons);
	}

	private void on_button_top_person_delete_person(object o, EventArgs args)
	{
		currentPerson = personSelectWin.SelectedPerson;
		personChanged();
		
		//without confirm, because it's already confirmed on PersonSelect
		on_delete_current_person_from_session_accepted (o, args);
				
		ArrayList myPersons = SqlitePersonSession.SelectCurrentSessionPersons(
				currentSession.UniqueID, 
				false); //means: do not returnPersonAndPSlist
		personSelectWin.Update(myPersons, currentPerson);
	}
	private void on_button_top_person_change_done(object o, EventArgs args)
	{
		currentPerson = personSelectWin.SelectedPerson; 
		currentPersonSession = SqlitePersonSession.Select(currentPerson.UniqueID, currentSession.UniqueID);
		label_person_change();

		personChanged();
		myTreeViewPersons.SelectRowByUniqueID(currentPerson.UniqueID);
	}

}
