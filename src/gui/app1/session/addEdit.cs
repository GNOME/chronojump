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
using Gtk;
//using Glade;
using GLib; //for Value
using System.Text; //StringBuilder
using System.Collections; //ArrayList
using Mono.Unix;

//here using app1sae_ , "sae" means session add edit
//this file has been moved from his old window to be part of app1 on Chronojump 2.0

public partial class ChronoJumpWindow
{
	//[Widget] Gtk.Button button_delete; //now disabled on edit window, just do it with special button on session/more
	//[Widget] Gtk.Label app1sae_label_persons_data;
	//[Widget] Gtk.TextView app1sae_textview_persons_data;

	TagSessionSelect tagSessionSelect;
	DialogCalendar app1sae_dialogCalendar;
	System.DateTime app1sae_dateTime;

	Sport app1sae_sport;
	string [] app1sae_sports;
	string [] app1sae_sportsTranslated;
	string [] app1sae_speciallities;
	string [] app1sae_speciallitiesTranslated;
	string [] app1sae_levels;

	//EDITOTHERSESSION is used when a session is edit from load session widgets
	//a session that is not currentSession
	public enum App1saeModes { ADDSESSION, EDITCURRENTSESSION, EDITOTHERSESSION};
	App1saeModes app1sae_mode;
	private Session tempEditingSession;

	bool app1sae_combosCreated = false;
	
	GenericWindow app1sae_genericWin;

	private void sessionAddEditUseSession (Session s)
	{
		tempEditingSession = s;
	}

	private void sessionAddEditShow (App1saeModes mode)
	{
		LogB.Information("sessionAddEditShow, " + mode.ToString());

		app1s_notebook.CurrentPage = app1s_PAGE_ADD_EDIT;
		app1sae_notebook_add_edit.CurrentPage = 0;

		app1sae_mode = mode;

		if(! app1sae_combosCreated)
		{
			app1sae_createComboSports();
			app1sae_createComboSpeciallities(-1);
			app1sae_createComboLevels();
			app1sae_combosCreated = true;
		}
			
		app1sae_label_name.Text = "<b>" + app1sae_label_name.Text + "</b>";
		app1sae_label_name.UseMarkup = true;

		app1sae_radiobutton_diff_sports.Active = true;
		app1sae_radiobutton_diff_speciallities.Active = true;
		app1sae_radiobutton_diff_levels.Active = true;

		app1sae_hbox_sports.Visible = false;
		app1sae_hbox_speciallities.Visible = false;
		app1sae_hbox_levels.Visible = false;
		TextBuffer tbtags = new TextBuffer (new TextTagTable());
		tbtags.Text = "";
		app1sae_textview_tags.Buffer = tbtags;

		if(app1sae_mode == App1saeModes.ADDSESSION) {
			hbox_session_add.Visible = true;
			hbox_session_more_edit.Visible = false;
			app1sae_dateTime = System.DateTime.Today;
			app1sae_entry_name.Text = "";
			app1sae_entry_place.Text = "";
			app1sae_label_date.Text = app1sae_dateTime.ToLongDateString();

			image_sport_undefined.Visible = false;
			image_speciallity_undefined.Visible = false;
			image_level_undefined.Visible = false;

			TextBuffer tb = new TextBuffer (new TextTagTable());
			tb.Text = "";
			app1sae_textview_comments.Buffer = tb;
//			app1sae_textview_persons_data.Buffer = tb;

			app1sae_button_accept.Sensitive = false;
		
		} else {
			//just a precaution
			if(tempEditingSession == null)
				return;

			hbox_session_add.Visible = false;
			hbox_session_more_edit.Visible = true;

			app1sae_dateTime = tempEditingSession.Date;

			app1sae_entry_name.Text = tempEditingSession.Name;
			app1sae_entry_place.Text = tempEditingSession.Place;

			app1sae_label_date.Text = tempEditingSession.DateLong;

			TextBuffer tb = new TextBuffer (new TextTagTable());
			tb.Text = tempEditingSession.Comments;
			app1sae_textview_comments.Buffer = tb;

			tbtags.Text = TagSession.GetActiveTagNamesOfThisSession(tempEditingSession.UniqueID);
			app1sae_textview_tags.Buffer = tbtags;
		}

		//app1sae_labelUpdate();
		app1sae_radios_changed();

		if(app1sae_mode != App1saeModes.ADDSESSION)
			app1sae_editSession_showSportStuffWithLoadedData();
	}

	void app1sae_editSession_showSportStuffWithLoadedData()
	{
		LogB.Information(string.Format("{0}-{1}-{2}", tempEditingSession.PersonsSportID, tempEditingSession.PersonsSpeciallityID, tempEditingSession.PersonsPractice));

		if(tempEditingSession.PersonsSportID != Constants.SportUndefinedID)
		{ 
			app1sae_radiobutton_same_sport.Active = true;
			Sport mySport = SqliteSport.Select(false, tempEditingSession.PersonsSportID);
			app1sae_combo_sports.Active = UtilGtk.ComboMakeActive(app1sae_sportsTranslated, mySport.ToString());
			app1sae_hbox_sports.Visible = true;

			if(app1sae_sport.HasSpeciallities)
			{
				app1sae_combo_speciallities.Destroy();
				app1sae_createComboSpeciallities(mySport.UniqueID);
				app1sae_speciallity_row_show(true);

				if(tempEditingSession.PersonsSpeciallityID != Constants.SpeciallityUndefinedID) {
					app1sae_radiobutton_same_speciallity.Active = true;
					app1sae_combo_speciallities.Active = UtilGtk.ComboMakeActive(app1sae_speciallitiesTranslated,
						       SqliteSpeciallity.Select(false, tempEditingSession.PersonsSpeciallityID));
						       
				} else 
					app1sae_combo_speciallities.Active = 
						UtilGtk.ComboMakeActive(app1sae_speciallitiesTranslated, 
								Catalog.GetString(Constants.SpeciallityUndefined));
			}

			if(tempEditingSession.PersonsSportID != Constants.SportNoneID)
			{
				app1sae_combo_levels.Destroy();
				app1sae_createComboLevels();
				app1sae_level_row_show(true);

				if(tempEditingSession.PersonsPractice != Constants.LevelUndefinedID) {
					app1sae_radiobutton_same_level.Active = true;
					app1sae_combo_levels.Active = UtilGtk.ComboMakeActive(app1sae_levels,
						       tempEditingSession.PersonsPractice + ":" +
						       Util.FindLevelName(tempEditingSession.PersonsPractice));
						       
				} else 
					app1sae_combo_levels.Active = 
						UtilGtk.ComboMakeActive(app1sae_levels, 
								Constants.LevelUndefinedID.ToString() + ":" + 
								Catalog.GetString(Constants.LevelUndefined));
			}

		}
	}

	void app1sae_on_entries_required_changed (object o, EventArgs args)
	{
		app1sae_entry_name.Text = Util.MakeValidSQL(app1sae_entry_name.Text);

		app1sae_sensitiveButtonAccept();
	}

	private void app1sae_on_entry_place_changed (object o, EventArgs args)
	{
		app1sae_entry_place.Text = Util.MakeValidSQL(app1sae_entry_place.Text);
	}

	void app1sae_sensitiveButtonAccept()
	{
		if(app1sae_entry_name.Text.ToString().Length > 0 &&
			( ! (! app1sae_radiobutton_diff_sports.Active && 
			  UtilGtk.ComboGetActive(app1sae_combo_sports) == Catalog.GetString(Constants.SportUndefined)) )  &&
			( ! (app1sae_label_speciallity.Visible && ! app1sae_radiobutton_diff_speciallities.Active && 
			  UtilGtk.ComboGetActive(app1sae_combo_speciallities) == Catalog.GetString(Constants.SpeciallityUndefined)) ) &&
			( ! (app1sae_label_level.Visible && ! app1sae_radiobutton_diff_levels.Active && 
			  Util.FetchID(UtilGtk.ComboGetActive(app1sae_combo_levels)) == Constants.LevelUndefinedID) ) ) {
			app1sae_button_accept.Sensitive = true;
		}
		else {
			app1sae_button_accept.Sensitive = false;
		}
	}
		
	void app1sae_on_radiobutton_sports_toggled (object o, EventArgs args)
	{
		if(app1sae_radiobutton_diff_sports.Active) {
			app1sae_hbox_sports.Hide();
			app1sae_speciallity_row_show(false);
			app1sae_level_row_show(false);
		}
		else {
			app1sae_hbox_sports.Show();
			app1sae_on_combo_sports_changed(o, args);
		}
		//app1sae_labelUpdate();
		app1sae_radios_changed();
		app1sae_sensitiveButtonAccept();
	}

	void app1sae_on_radiobutton_speciallities_toggled (object o, EventArgs args)
	{
		if(app1sae_radiobutton_diff_speciallities.Active)
			app1sae_hbox_speciallities.Hide();
		else
			app1sae_hbox_speciallities.Show();
		//app1sae_labelUpdate();
		app1sae_radios_changed();
		app1sae_sensitiveButtonAccept();
	}
					
	void app1sae_speciallity_row_show(bool show)
	{
		if(show) {
			if(app1sae_radiobutton_diff_speciallities.Active)
				app1sae_hbox_speciallities.Hide();
			else
				app1sae_hbox_speciallities.Show();
			app1sae_label_speciallity.Show();
			app1sae_vbox_speciallity.Show();
		} else {
			app1sae_label_speciallity.Hide();
			app1sae_vbox_speciallity.Hide();
			app1sae_radiobutton_diff_speciallities.Active = true;
		}
		//app1sae_labelUpdate();
		app1sae_radios_changed();
		app1sae_sensitiveButtonAccept();
	}

	void app1sae_on_radiobutton_levels_toggled (object o, EventArgs args)
	{
		if(app1sae_radiobutton_diff_levels.Active)
			app1sae_hbox_levels.Hide();
		else
			app1sae_hbox_levels.Show();
		//app1sae_labelUpdate();
		app1sae_radios_changed();
		app1sae_sensitiveButtonAccept();
	}

	void app1sae_level_row_show(bool show)
	{
		if(show) {
			if(app1sae_radiobutton_diff_levels.Active)
				app1sae_hbox_levels.Hide();
			else
				app1sae_hbox_levels.Show();
			app1sae_label_level.Show();
			app1sae_vbox_level.Show();
		} else {
			app1sae_label_level.Hide();
			app1sae_vbox_level.Hide();
			app1sae_radiobutton_diff_levels.Active = true;
		}
		//app1sae_labelUpdate();
		app1sae_radios_changed();
		app1sae_sensitiveButtonAccept();
	}

	private void app1sae_createComboSports()
	{
		app1sae_combo_sports = new ComboBoxText ();
		app1sae_sports = SqliteSport.SelectAll();
		
		//create sports translated, only with translated stuff
		app1sae_sportsTranslated = new String[app1sae_sports.Length];
		int i = 0;
		foreach(string row in app1sae_sports) {
			string [] myStrFull = row.Split(new char[] {':'});
			app1sae_sportsTranslated[i++] = myStrFull[2];
			}
		
		//sort array (except second row)
		System.Array.Sort(app1sae_sportsTranslated, 2, app1sae_sportsTranslated.Length-2);
		
		UtilGtk.ComboUpdate(app1sae_combo_sports, app1sae_sportsTranslated, "");
		app1sae_combo_sports.Active = UtilGtk.ComboMakeActive(app1sae_sportsTranslated, 
				Catalog.GetString(Constants.SportUndefined));
	
		app1sae_combo_sports.Changed += new EventHandler (app1sae_on_combo_sports_changed);

		app1sae_hbox_combo_sports.PackStart(app1sae_combo_sports, true, true, 0);
		app1sae_hbox_combo_sports.ShowAll();
		app1sae_combo_sports.Sensitive = true;
		
		app1sae_hbox_sports.Hide();
	}
	
	private void app1sae_createComboSpeciallities(int sportID)
	{
		app1sae_combo_speciallities = new ComboBoxText ();
		app1sae_speciallities = SqliteSpeciallity.SelectAll(true, sportID); //show undefined, filter by sport

		//create speciallities translated, only with translated stuff
		app1sae_speciallitiesTranslated = new String[app1sae_speciallities.Length];
		int i = 0;
		foreach(string row in app1sae_speciallities) {
			string [] myStrFull = row.Split(new char[] {':'});
			app1sae_speciallitiesTranslated[i++] = myStrFull[2];
			}
		
		//sort array (except first row)
		System.Array.Sort(app1sae_speciallities, 1, app1sae_speciallities.Length-1);

		UtilGtk.ComboUpdate(app1sae_combo_speciallities, app1sae_speciallitiesTranslated, "");
		app1sae_combo_speciallities.Active = UtilGtk.ComboMakeActive(app1sae_speciallitiesTranslated, 
				Catalog.GetString(Constants.SpeciallityUndefined));

		app1sae_combo_speciallities.Changed += new EventHandler (app1sae_on_combo_speciallities_changed);

		app1sae_hbox_combo_speciallities.PackStart(app1sae_combo_speciallities, true, true, 0);
		app1sae_hbox_speciallities.ShowAll();
		app1sae_combo_speciallities.Sensitive = true;

		app1sae_speciallity_row_show(false);
	}
	
	private void app1sae_createComboLevels()
	{
		app1sae_combo_levels = new ComboBoxText ();
		app1sae_levels = Constants.LevelsStr();
		
		UtilGtk.ComboUpdate(app1sae_combo_levels, app1sae_levels, "");
		app1sae_combo_levels.Active = UtilGtk.ComboMakeActive(app1sae_levels, 
				Constants.LevelUndefinedID.ToString() + ":" + 
				Catalog.GetString(Constants.LevelUndefined));

		app1sae_combo_levels.Changed += new EventHandler (app1sae_on_combo_levels_changed);

		app1sae_hbox_combo_levels.PackStart(app1sae_combo_levels, true, true, 0);
		app1sae_hbox_levels.ShowAll();
		//app1sae_combo_levels.Sensitive = false; //level is shown when sport is not "undefined" and not "none"
		app1sae_combo_levels.Sensitive = true; //level is shown when sport is not "undefined" and not "none"
		
		app1sae_level_row_show(false);
	}
	
	private void app1sae_on_combo_sports_changed(object o, EventArgs args)
	{
		if (o == null)
			return;

		//LogB.Information("changed");
		try {
			//sport = new Sport(UtilGtk.ComboGetActive(combo_sports));
			int sportID = Convert.ToInt32(Util.FindOnArray(':', 2, 0, UtilGtk.ComboGetActive(app1sae_combo_sports), app1sae_sports));
			app1sae_sport = SqliteSport.Select(false, sportID);

			if(Catalog.GetString(app1sae_sport.Name) == Catalog.GetString(Constants.SportUndefined)) {
				//if sport is undefined, level should be undefined, and unsensitive
				try { 
					app1sae_combo_levels.Active = UtilGtk.ComboMakeActive(app1sae_levels, 
							Constants.LevelUndefinedID.ToString() + ":" + 
							Catalog.GetString(Constants.LevelUndefined));
					app1sae_level_row_show(false);
					app1sae_combo_speciallities.Active = UtilGtk.ComboMakeActive(app1sae_speciallitiesTranslated,
							Catalog.GetString(Constants.SpeciallityUndefined));
					app1sae_speciallity_row_show(false);
				}
				catch { LogB.Warning("do later"); }
			} else if(Catalog.GetString(app1sae_sport.Name) == Catalog.GetString(Constants.SportNone)) {
				//if sport is none, level should be sedentary and unsensitive
				try { 
					app1sae_combo_levels.Active = UtilGtk.ComboMakeActive(app1sae_levels, 
							Constants.LevelSedentaryID.ToString() + ":" + 
							Catalog.GetString(Constants.LevelSedentary));
					app1sae_level_row_show(false);

					app1sae_combo_speciallities.Active = UtilGtk.ComboMakeActive(app1sae_speciallitiesTranslated, 
							Catalog.GetString(Constants.SpeciallityUndefined));

					app1sae_speciallity_row_show(false);
				}
				catch { LogB.Warning("do later"); }
			} else {
				//sport is not undefined and not none

				//if level is "sedentary", then change level to "undefined"
				if(UtilGtk.ComboGetActive(app1sae_combo_levels) == 
						Constants.LevelSedentaryID.ToString() + ":" + 
						Catalog.GetString(Constants.LevelSedentary))
					app1sae_combo_levels.Active = UtilGtk.ComboMakeActive(app1sae_levels, 
							Constants.LevelUndefinedID.ToString() + ":" + 
							Catalog.GetString(Constants.LevelUndefined));

				//show level
				app1sae_combo_levels.Sensitive = true;
				app1sae_level_row_show(true);
		
				if(app1sae_sport.HasSpeciallities) {
					app1sae_combo_speciallities.Destroy();
					app1sae_createComboSpeciallities(app1sae_sport.UniqueID);
					app1sae_speciallity_row_show(true);
				} else {
					LogB.Information("hide");
					app1sae_combo_speciallities.Active = UtilGtk.ComboMakeActive(app1sae_speciallitiesTranslated, 
							Catalog.GetString(Constants.SpeciallityUndefined));
					app1sae_speciallity_row_show(false);
				}
			}
		} catch { 
			//LogB.Warning("do later");
		}

		LogB.Information("at on_combo_sports_changed " + app1sae_sport.ToString());
		//labelUpdate();
	}

	private void app1sae_on_combo_speciallities_changed(object o, EventArgs args)
	{
		LogB.Information("changed speciallities");
		//app1sae_labelUpdate();
		app1sae_radios_changed();
		app1sae_sensitiveButtonAccept();
	}

	private void app1sae_on_combo_levels_changed(object o, EventArgs args)
	{
		//string myText = UtilGtk.ComboGetActive(combo_sports);
		LogB.Information("changed levels");
		//level = UtilGtk.ComboGetActive(combo_levels);
				
		//if it's sedentary, put sport to none
		/*
		 * Now undone because sedentary has renamed to "sedentary/Occasional practice"
		if(UtilGtk.ComboGetActive(combo_levels) == "0:" + Catalog.GetString(Constants.LevelSedentary))
			combo_sports.Active = UtilGtk.ComboMakeActive(sports, "2:" + Catalog.GetString(Constants.SportNone));
		*/
		//app1sae_labelUpdate();
		app1sae_radios_changed();
		app1sae_sensitiveButtonAccept();
	}

	private void app1sae_radios_changed ()
	{
		if(! app1sae_radiobutton_diff_sports.Active)
			image_sport_undefined.Visible =
				(Convert.ToInt32(Util.FindOnArray(':', 2, 0, UtilGtk.ComboGetActive(app1sae_combo_sports), app1sae_sports)) == Constants.SportUndefinedID);

		if(app1sae_label_speciallity.Visible && ! app1sae_radiobutton_diff_speciallities.Active)
			image_speciallity_undefined.Visible =
						(UtilGtk.ComboGetActive(app1sae_combo_speciallities) == Catalog.GetString(Constants.SpeciallityUndefined));

		if(app1sae_label_level.Visible && ! app1sae_radiobutton_diff_levels.Active)
			image_level_undefined.Visible =
						(Util.FetchID(UtilGtk.ComboGetActive(app1sae_combo_levels)) == Constants.LevelUndefinedID);
	}

	/*	
	private void app1sae_labelUpdate()
	{
		string sportString = "";
		string speciallityString = "";
		string levelString = "";
		string pleaseDefineItString = " [" + Catalog.GetString("Please, define it") + "].";

		try {
			if(app1sae_radiobutton_diff_sports.Active)
				sportString = Catalog.GetString("People in session practice different sports.");
			else {
				sportString = Catalog.GetString("All people in session practice the same sport:");
				int sportID = Convert.ToInt32(Util.FindOnArray(':', 2, 0, UtilGtk.ComboGetActive(app1sae_combo_sports), app1sae_sports));
				Sport mySport = SqliteSport.Select(false, sportID);

				if(sportID == Constants.SportUndefinedID)
					//sportString += "<tt>" + pleaseDefineItString + "</tt>";
					sportString += pleaseDefineItString;
				else if(sportID == Constants.SportNoneID)
					sportString = Catalog.GetString("Nobody in this session practice sport.");
				else
					//sportString += " <i>" + mySport.Name + "</i>.";
					sportString += " " + mySport.Name + ".";

				if(app1sae_label_speciallity.Visible) {
					if(app1sae_radiobutton_diff_speciallities.Active)
						speciallityString = "\n" + Catalog.GetString("Different specialties.");
					else {
						speciallityString = "\n" + Catalog.GetString("This specialty:");
						if(UtilGtk.ComboGetActive(app1sae_combo_speciallities) == Catalog.GetString(Constants.SpeciallityUndefined))
							//speciallityString += "<tt>" + pleaseDefineItString + "</tt>";
							speciallityString += pleaseDefineItString;
						else
							//speciallityString += " <i>" + UtilGtk.ComboGetActive(app1sae_combo_speciallities) + "</i>.";
							speciallityString += " " + UtilGtk.ComboGetActive(app1sae_combo_speciallities) + ".";
					}
				}
				if(app1sae_label_level.Visible) {
					if(app1sae_radiobutton_diff_levels.Active)
						levelString = "\n" + Catalog.GetString("Different levels.");
					else {
						levelString = "\n" + Catalog.GetString("This level:");
						int levelID =  Util.FetchID(UtilGtk.ComboGetActive(app1sae_combo_levels));
						if(levelID == Constants.LevelUndefinedID)
							//levelString += "<tt>" + pleaseDefineItString + "</tt>";
							levelString += pleaseDefineItString;
						else
							//levelString += " <i>" + Util.FetchName(UtilGtk.ComboGetActive(app1sae_combo_levels)) + "</i>.";
							levelString += " " + Util.FetchName(UtilGtk.ComboGetActive(app1sae_combo_levels)) + ".";
					}
				}
			}
			//app1sae_label_persons_data.Text = sportString + speciallityString + levelString;
			//app1sae_label_persons_data.UseMarkup = true;
			//TextBuffer tb = new TextBuffer (new TextTagTable());
			//tb.Text = sportString + speciallityString + levelString;
			//app1sae_textview_persons_data.Buffer = tb;
		} catch {
			LogB.Warning("Do later");
		}
	}
	*/

	void app1sae_on_button_cancel_clicked (object o, EventArgs args)
	{
		if(app1sae_mode == App1saeModes.ADDSESSION)
		{
			menus_and_mode_sensitive (true); //because we go to main gui, not needed on EDITCURRENTSESSION or EDITOTHERSESSION
			notebook_supSetOldPage();
		}
		else if(app1sae_mode == App1saeModes.EDITCURRENTSESSION)
			app1s_notebook.CurrentPage = app1s_PAGE_MODES;
		else {	//(app1sae_mode == App1saeModes.EDITOTHERSESSION)
			app1s_notebook.CurrentPage = app1s_PAGE_SELECT_SESSION;

			//maybe tags have been created while editing:
			createComboSessionLoadTags (false);
			//and reload the treeview:
			app1s_recreateTreeView("Cancelled an edit session from load session");
		}
	}

	private void on_app1sae_button_select_tags_clicked (object o, EventArgs args)
	{
		//just be cautious, but if we are not editing, this is not needed
		if(app1sae_mode != App1saeModes.ADDSESSION && tempEditingSession == null)
			return;

		tagSessionSelect = new TagSessionSelect();

		if(app1sae_mode == App1saeModes.ADDSESSION)
			tagSessionSelect.PassVariables(true, -1, "", preferences.askDeletion);
		else
			tagSessionSelect.PassVariables(false, tempEditingSession.UniqueID, "", preferences.askDeletion);

		tagSessionSelect.FakeButtonDone.Clicked -= new EventHandler(on_select_tags_clicked_done_addEdit);
		tagSessionSelect.FakeButtonDone.Clicked += new EventHandler(on_select_tags_clicked_done_addEdit);

		tagSessionSelect.Do();
		tagSessionSelect.Show();
	}
	private void on_select_tags_clicked_done_addEdit (object o, EventArgs args)
	{
		tagSessionSelect.FakeButtonDone.Clicked -= new EventHandler(on_select_tags_clicked_done_addEdit);

		TextBuffer tbtags = new TextBuffer (new TextTagTable());
		tbtags.Text = "";
		if(app1sae_mode == App1saeModes.ADDSESSION)
			tbtags.Text = tagSessionSelect.TagsListStringForAddSession;
		else
			tbtags.Text = TagSession.GetActiveTagNamesOfThisSession(tempEditingSession.UniqueID);

		app1sae_textview_tags.Buffer = tbtags;
	}

	void app1sae_on_button_change_date_clicked (object o, EventArgs args)
	{
		app1sae_dialogCalendar = new DialogCalendar(Catalog.GetString("Select session date"), app1sae_dateTime);
		app1sae_dialogCalendar.FakeButtonDateChanged.Clicked += new EventHandler(app1sae_on_calendar_changed);
	}

	void app1sae_on_calendar_changed (object obj, EventArgs args)
	{
		app1sae_dateTime = app1sae_dialogCalendar.MyDateTime;
		app1sae_label_date.Text = app1sae_dateTime.ToLongDateString();
	}

	void app1sae_on_button_sport_add_clicked (object o, EventArgs args)
	{
		LogB.Information("sport add clicked");
		app1sae_genericWin = GenericWindow.Show(Catalog.GetString("Add sport"),
				Catalog.GetString("Add new sport to database"),
				Constants.GenericWindowShow.ENTRY, true);
		app1sae_genericWin.Button_accept.Clicked += new EventHandler(app1sae_on_sport_add_accepted);
	}

	private void app1sae_on_sport_add_accepted (object o, EventArgs args)
	{
		app1sae_genericWin.Button_accept.Clicked -= new EventHandler(app1sae_on_sport_add_accepted);

		string newSportName = app1sae_genericWin.EntrySelected;
		if(Sqlite.Exists(false, Constants.SportTable, newSportName) ||
				newSportName == Catalog.GetString(Constants.SportUndefined) || //let's save problems
				newSportName == Catalog.GetString(Constants.SportNone)		//let's save problems
				)
				new DialogMessage(Constants.MessageTypes.WARNING, string.Format(
							Catalog.GetString("Sorry, this sport '{0}' already exists in database"), 
							newSportName));
		else {
			int myID = SqliteSport.Insert(false, "-1", newSportName, true, //dbconOpened, , userDefined
					false, "");	//hasSpeciallities, graphLink 

			Sport mySport = new Sport(myID, newSportName, true, 
					false, "");	//hasSpeciallities, graphLink 
			app1sae_sports = SqliteSport.SelectAll();
			//create sports translated, only with translated stuff
			app1sae_sportsTranslated = new String[app1sae_sports.Length];
			int i = 0;
			foreach(string row in app1sae_sports) {
				string [] myStrFull = row.Split(new char[] {':'});
				app1sae_sportsTranslated[i++] = myStrFull[2];
				}
		
			//sort array (except second row)
			System.Array.Sort(app1sae_sportsTranslated, 2, app1sae_sportsTranslated.Length-2);

			UtilGtk.ComboUpdate(app1sae_combo_sports, app1sae_sportsTranslated, mySport.ToString());
			app1sae_combo_sports.Active = UtilGtk.ComboMakeActive(app1sae_sportsTranslated, mySport.ToString());
		}
	}
	
	void app1sae_on_button_accept_clicked (object o, EventArgs args)
	{
		//there is a bug here, just print some stuff to find it in next log
		LogB.Information("app1sae_on_button_accept_clicked 0");
		//check if name of session exists (is owned by other session),
		//but all is ok if the name is the same as the old name (editing)
		string name = Util.RemoveTildeAndColon(app1sae_entry_name.Text);
		name = Util.RemoveChar(name, '/');
		LogB.Information("app1sae_on_button_accept_clicked 1");

		bool sessionNameExists = Sqlite.Exists (false, Constants.SessionTable, name);
		if(sessionNameExists && app1sae_mode == App1saeModes.ADDSESSION)
		{
			//if we try to add a new session with same name ...
			LogB.Information("app1sae_on_button_accept_clicked add existing ...");
			string myString = string.Format(Catalog.GetString("Session: '{0}' exists. Please, use another name"), name);
			ErrorWindow.Show(myString);
			LogB.Information("app1sae_on_button_accept_clicked add existing done!");
		}
		else if( sessionNameExists && app1sae_mode != App1saeModes.ADDSESSION && (tempEditingSession == null || name != tempEditingSession.Name) )
		{
			//if we edit a session but we changed name and it matches another existing session ...
			LogB.Information("app1sae_on_button_accept_clicked edit existing not me ...");
			string myString = string.Format(Catalog.GetString("Session: '{0}' exists. Please, use another name"), name);
			ErrorWindow.Show(myString);
			LogB.Information("app1sae_on_button_accept_clicked edit existing not me done!");

		} else {
			LogB.Information("app1sae_on_button_accept_clicked 4");
			int sportID;
			if(app1sae_radiobutton_diff_sports.Active)
				sportID = Constants.SportUndefinedID;
			else {
				sportID = Convert.ToInt32(Util.FindOnArray(':', 2, 0, UtilGtk.ComboGetActive(app1sae_combo_sports), app1sae_sports));
			}
			LogB.Information("app1sae_on_button_accept_clicked 5");

			int speciallityID;
			if(! app1sae_label_speciallity.Visible || app1sae_radiobutton_diff_speciallities.Active)
				speciallityID = Constants.SpeciallityUndefinedID; 
			else
				speciallityID = Convert.ToInt32(Util.FindOnArray(':', 2, 0, UtilGtk.ComboGetActive(app1sae_combo_speciallities), app1sae_speciallities));
			LogB.Information("app1sae_on_button_accept_clicked 6");

			int levelID;
			if(! app1sae_label_level.Visible || app1sae_radiobutton_diff_levels.Active)
				levelID = Constants.LevelUndefinedID;
			else
				levelID = Util.FetchID(UtilGtk.ComboGetActive(app1sae_combo_levels));
			LogB.Information("app1sae_on_button_accept_clicked 7");

			string place = Util.RemoveTildeAndColon(app1sae_entry_place.Text);
			string comments = Util.RemoveTildeAndColon(Util.MakeValidSQL(app1sae_textview_comments.Buffer.Text));
			LogB.Information("app1sae_on_button_accept_clicked 8");

			if(app1sae_mode == App1saeModes.ADDSESSION)
			{
				LogB.Information("app1sae_on_button_accept_clicked 9");
				currentSession = new Session (name, place,
						app1sae_dateTime,
						sportID, speciallityID, levelID,
						comments,
						Constants.ServerUndefinedID);

				LogB.Information("app1sae_on_button_accept_clicked A");
				on_new_session_accepted();
				LogB.Information("app1sae_on_button_accept_clicked B");
				notebook_supSetOldPage();
				LogB.Information("app1sae_on_button_accept_clicked C");

				//tags have not been added yet because there was no sessionID
				if(tagSessionSelect != null)
					tagSessionSelect.SQLUpdateTransaction(currentSession.UniqueID);

				LogB.Information("app1sae_on_button_accept_clicked C2");
			} else
			{
				LogB.Information("app1sae_on_button_accept_clicked D");
				tempEditingSession.Name = name;
				tempEditingSession.Place = place;
				tempEditingSession.Date = app1sae_dateTime;
				tempEditingSession.PersonsSportID = sportID;
				tempEditingSession.PersonsSpeciallityID = speciallityID;
				tempEditingSession.PersonsPractice = levelID;
				tempEditingSession.Comments = comments;

				LogB.Information("app1sae_on_button_accept_clicked E");
				SqliteSession.Update(tempEditingSession.UniqueID, tempEditingSession.Name,
						tempEditingSession.Place, tempEditingSession.Date,
						sportID, speciallityID, levelID,
						tempEditingSession.Comments);
				LogB.Information("app1sae_on_button_accept_clicked F");

				if(app1sae_mode == App1saeModes.EDITCURRENTSESSION)
				{
					currentSession = tempEditingSession;
					on_edit_session_accepted ();

					LogB.Information("app1sae_on_button_accept_clicked G");
					app1s_notebook.CurrentPage = app1s_PAGE_MODES;
				} else {//(app1sae_mode == App1saeModes.EDITOTHERSESSION)
					app1s_notebook.CurrentPage = app1s_PAGE_SELECT_SESSION;

					//maybe tags have been created while editing:
					createComboSessionLoadTags (false);
					//and reload the treeview:
					app1s_recreateTreeView("Accepted an edit session from load session");
				}

				LogB.Information("app1sae_on_button_accept_clicked H");
			}
			LogB.Information("app1sae_on_button_accept_clicked I");
		}
		LogB.Information("app1sae_on_button_accept_clicked J");
	}

}
