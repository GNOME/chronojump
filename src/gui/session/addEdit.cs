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
 * Copyright (C) 2004-2017   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
using Gtk;
using Glade;
using GLib; //for Value
using System.Text; //StringBuilder
using System.Collections; //ArrayList
using Mono.Unix;


public class SessionAddEditWindow
{
	[Widget] Gtk.Window session_add_edit;
	[Widget] Gtk.Entry entry_name;
	[Widget] Gtk.Entry entry_place;
	
	[Widget] Gtk.Label label_date;
	
	[Widget] Gtk.TextView textview;
	[Widget] Gtk.Button button_accept;
	
	[Widget] Gtk.RadioButton radiobutton_diff_sports;
	[Widget] Gtk.RadioButton radiobutton_same_sport;
	[Widget] Gtk.RadioButton radiobutton_diff_speciallities;
	[Widget] Gtk.RadioButton radiobutton_same_speciallity;
	[Widget] Gtk.RadioButton radiobutton_diff_levels;
	[Widget] Gtk.RadioButton radiobutton_same_level;
	[Widget] Gtk.Box hbox_sports;
	[Widget] Gtk.Box hbox_combo_sports;
	[Widget] Gtk.ComboBox combo_sports;
	[Widget] Gtk.Box vbox_speciallity;
	[Widget] Gtk.Label label_speciallity;
	[Widget] Gtk.Box hbox_combo_speciallities;
	[Widget] Gtk.ComboBox combo_speciallities;
	[Widget] Gtk.Box vbox_level;
	[Widget] Gtk.Label label_level;
	[Widget] Gtk.Box hbox_combo_levels;
	[Widget] Gtk.ComboBox combo_levels;
	
	[Widget] Gtk.Label label_persons_data;

	DialogCalendar myDialogCalendar;
	DateTime dateTime;

	Sport sport;
	string [] sports;
	string [] sportsTranslated;
	string [] speciallities;
	string [] speciallitiesTranslated;
	string [] levels;

	bool addSession;
	private bool textviewChanging = false;
	
	private Session currentSession;
	private Gtk.Button fakeButtonAccept;
	
	GenericWindow genericWin;
	static SessionAddEditWindow SessionAddEditWindowBox;
	
	
	SessionAddEditWindow (Gtk.Window parent, Session currentSession) {
		Glade.XML gladeXML;
		gladeXML = Glade.XML.FromAssembly (Util.GetGladePath() + "session_add_edit.glade", "session_add_edit", null);
		gladeXML.Autoconnect(this);
		session_add_edit.Parent = parent;
		
		//put an icon to window
		UtilGtk.IconWindow(session_add_edit);
	
		this.currentSession = currentSession;
		button_accept.Sensitive = false;
		
		fakeButtonAccept = new Button();

		createComboSports();
		createComboSpeciallities(-1);
		createComboLevels();
		labelUpdate();
			
		if(currentSession.UniqueID == -1)
			addSession = true;
		else 
			addSession = false;
		
		if(addSession) {
			session_add_edit.Title = Catalog.GetString("New Session");
			dateTime = DateTime.Today;
			label_date.Text = dateTime.ToLongDateString();
		} else {
			session_add_edit.Title = Catalog.GetString("Session Edit");

			dateTime = currentSession.Date;

			entry_name.Text = currentSession.Name;
			entry_place.Text = currentSession.Place;

			label_date.Text = currentSession.DateLong;

			TextBuffer tb = new TextBuffer (new TextTagTable());
			tb.Text = currentSession.Comments;
			textview.Buffer = tb;

			//showSportStuffWithLoadedData();
		}

		textview.Buffer.Changed += new EventHandler(textviewChanged);
		textviewChanging = false;
	}
	
	static public SessionAddEditWindow Show (Gtk.Window parent, Session currentSession)
	{
		if (SessionAddEditWindowBox == null) {
			SessionAddEditWindowBox = new SessionAddEditWindow (parent, currentSession);
		}
		SessionAddEditWindowBox.session_add_edit.Show ();

		SessionAddEditWindowBox.fillDialog ();
		
		return SessionAddEditWindowBox;
	}
	
	void showSportStuffWithLoadedData() {
		LogB.Information(string.Format("{0}-{1}-{2}", currentSession.PersonsSportID, currentSession.PersonsSpeciallityID, currentSession.PersonsPractice));

		if(currentSession.PersonsSportID != Constants.SportUndefinedID) { 
			radiobutton_same_sport.Active = true;
			Sport mySport = SqliteSport.Select(false, currentSession.PersonsSportID);
			combo_sports.Active = UtilGtk.ComboMakeActive(sportsTranslated, mySport.ToString());

			if(sport.HasSpeciallities) {
				combo_speciallities.Destroy();
				createComboSpeciallities(mySport.UniqueID);
				speciallity_row_show(true);

				if(currentSession.PersonsSpeciallityID != Constants.SpeciallityUndefinedID) { 
					radiobutton_same_speciallity.Active = true;
					combo_speciallities.Active = UtilGtk.ComboMakeActive(speciallitiesTranslated,
						       SqliteSpeciallity.Select(false, currentSession.PersonsSpeciallityID));
						       
				} else 
					combo_speciallities.Active = 
						UtilGtk.ComboMakeActive(speciallitiesTranslated, 
								Catalog.GetString(Constants.SpeciallityUndefined));
				
			}

			if(currentSession.PersonsSportID != Constants.SportNoneID) { 
				combo_levels.Destroy();
				createComboLevels();
				level_row_show(true);

				if(currentSession.PersonsPractice != Constants.LevelUndefinedID) { 
					radiobutton_same_level.Active = true;
					combo_levels.Active = UtilGtk.ComboMakeActive(levels,
						       currentSession.PersonsPractice + ":" + 
						       Util.FindLevelName(currentSession.PersonsPractice));
						       
				} else 
					combo_levels.Active = 
						UtilGtk.ComboMakeActive(levels, 
								Constants.LevelUndefinedID.ToString() + ":" + 
								Catalog.GetString(Constants.LevelUndefined));
				
			}

		}
	}

	void fillDialog() {
		if(!addSession)
			showSportStuffWithLoadedData();

	}

	void on_entries_required_changed (object o, EventArgs args)
	{
		entry_name.Text = Util.MakeValidSQL(entry_name.Text);

		showHideButtonAccept();
	}

	private void on_entry_place_changed (object o, EventArgs args)
	{
		entry_place.Text = Util.MakeValidSQL(entry_place.Text);
	}

	private void textviewChanged(object o,EventArgs args)
	{
		if(textviewChanging)
			return;

		textviewChanging = true;

		TextBuffer tb = o as TextBuffer;
		if (o == null)
			return;

		tb.Text = Util.MakeValidSQL(tb.Text);
		textviewChanging = false;
	}

	void showHideButtonAccept() {
		if(entry_name.Text.ToString().Length > 0 &&
			( ! (!radiobutton_diff_sports.Active && 
			  UtilGtk.ComboGetActive(combo_sports) == Catalog.GetString(Constants.SportUndefined)) )  &&
			( ! (label_speciallity.Visible && !radiobutton_diff_speciallities.Active && 
			  UtilGtk.ComboGetActive(combo_speciallities) == Catalog.GetString(Constants.SpeciallityUndefined)) ) &&
			( ! (label_level.Visible && !radiobutton_diff_levels.Active && 
			  Util.FetchID(UtilGtk.ComboGetActive(combo_levels)) == Constants.LevelUndefinedID) ) ) {
			button_accept.Sensitive = true;
		}
		else {
			button_accept.Sensitive = false;
		}
	}
		
	void on_radiobutton_sports_toggled (object o, EventArgs args) {
		if(radiobutton_diff_sports.Active) {
			hbox_sports.Hide();
			speciallity_row_show(false);
			level_row_show(false);
		}
		else {
			hbox_sports.Show();
			on_combo_sports_changed(o, args);
		}
		labelUpdate();
		showHideButtonAccept();
	}

	void on_radiobutton_speciallities_toggled (object o, EventArgs args) {
		if(radiobutton_diff_speciallities.Active)
			hbox_combo_speciallities.Hide();
		else
			hbox_combo_speciallities.Show();
		labelUpdate();
		showHideButtonAccept();
	}
					
	void speciallity_row_show(bool show) {
		if(show) {
			if(radiobutton_diff_speciallities.Active)
				hbox_combo_speciallities.Hide();
			else
				hbox_combo_speciallities.Show();
			label_speciallity.Show();
			vbox_speciallity.Show();
		} else {
			label_speciallity.Hide();
			vbox_speciallity.Hide();
			radiobutton_diff_speciallities.Active = true;
		}
		labelUpdate();
		showHideButtonAccept();
	}

	void on_radiobutton_levels_toggled (object o, EventArgs args) {
		if(radiobutton_diff_levels.Active)
			hbox_combo_levels.Hide();
		else
			hbox_combo_levels.Show();
		labelUpdate();
		showHideButtonAccept();
	}

	void level_row_show(bool show) {
		if(show) {
			if(radiobutton_diff_levels.Active)
				hbox_combo_levels.Hide();
			else
				hbox_combo_levels.Show();
			label_level.Show();
			vbox_level.Show();
		} else {
			label_level.Hide();
			vbox_level.Hide();
			radiobutton_diff_levels.Active = true;
		}
		labelUpdate();
		showHideButtonAccept();
	}

	private void createComboSports() {
		combo_sports = ComboBox.NewText ();
		sports = SqliteSport.SelectAll();
		
		//create sports translated, only with translated stuff
		sportsTranslated = new String[sports.Length];
		int i = 0;
		foreach(string row in sports) {
			string [] myStrFull = row.Split(new char[] {':'});
			sportsTranslated[i++] = myStrFull[2];
			}
		
		//sort array (except second row)
		System.Array.Sort(sportsTranslated, 2, sportsTranslated.Length-2);
		
		UtilGtk.ComboUpdate(combo_sports, sportsTranslated, "");
		combo_sports.Active = UtilGtk.ComboMakeActive(sportsTranslated, 
				Catalog.GetString(Constants.SportUndefined));
	
		combo_sports.Changed += new EventHandler (on_combo_sports_changed);

		hbox_combo_sports.PackStart(combo_sports, true, true, 0);
		hbox_combo_sports.ShowAll();
		combo_sports.Sensitive = true;
		
		hbox_sports.Hide();
	}
	
	private void createComboSpeciallities(int sportID) {
		combo_speciallities = ComboBox.NewText ();
		speciallities = SqliteSpeciallity.SelectAll(true, sportID); //show undefined, filter by sport

		//create speciallities translated, only with translated stuff
		speciallitiesTranslated = new String[speciallities.Length];
		int i = 0;
		foreach(string row in speciallities) {
			string [] myStrFull = row.Split(new char[] {':'});
			speciallitiesTranslated[i++] = myStrFull[2];
			}
		
		//sort array (except first row)
		System.Array.Sort(speciallities, 1, speciallities.Length-1);

		UtilGtk.ComboUpdate(combo_speciallities, speciallitiesTranslated, "");
		combo_speciallities.Active = UtilGtk.ComboMakeActive(speciallitiesTranslated, 
				Catalog.GetString(Constants.SpeciallityUndefined));

		combo_speciallities.Changed += new EventHandler (on_combo_speciallities_changed);

		hbox_combo_speciallities.PackStart(combo_speciallities, true, true, 0);
		hbox_combo_speciallities.ShowAll();
		combo_speciallities.Sensitive = true;

		speciallity_row_show(false);
	}
	
	private void createComboLevels() {
		combo_levels = ComboBox.NewText ();
		levels = Constants.LevelsStr();
		
		UtilGtk.ComboUpdate(combo_levels, levels, "");
		combo_levels.Active = UtilGtk.ComboMakeActive(levels, 
				Constants.LevelUndefinedID.ToString() + ":" + 
				Catalog.GetString(Constants.LevelUndefined));

		combo_levels.Changed += new EventHandler (on_combo_levels_changed);

		hbox_combo_levels.PackStart(combo_levels, true, true, 0);
		hbox_combo_levels.ShowAll();
		//combo_levels.Sensitive = false; //level is shown when sport is not "undefined" and not "none"
		combo_levels.Sensitive = true; //level is shown when sport is not "undefined" and not "none"
		
		level_row_show(false);
	}
	
	private void on_combo_sports_changed(object o, EventArgs args) {
		if (o == null)
			return;

		//LogB.Information("changed");
		try {
			//sport = new Sport(UtilGtk.ComboGetActive(combo_sports));
			int sportID = Convert.ToInt32(Util.FindOnArray(':', 2, 0, UtilGtk.ComboGetActive(combo_sports), sports));
			sport = SqliteSport.Select(false, sportID);

			if(Catalog.GetString(sport.Name) == Catalog.GetString(Constants.SportUndefined)) {
				//if sport is undefined, level should be undefined, and unsensitive
				try { 
					combo_levels.Active = UtilGtk.ComboMakeActive(levels, 
							Constants.LevelUndefinedID.ToString() + ":" + 
							Catalog.GetString(Constants.LevelUndefined));
					level_row_show(false);
					combo_speciallities.Active = UtilGtk.ComboMakeActive(speciallitiesTranslated,
							Catalog.GetString(Constants.SpeciallityUndefined));
					speciallity_row_show(false);
				}
				catch { LogB.Warning("do later"); }
			} else if(Catalog.GetString(sport.Name) == Catalog.GetString(Constants.SportNone)) {
				//if sport is none, level should be sedentary and unsensitive
				try { 
					combo_levels.Active = UtilGtk.ComboMakeActive(levels, 
							Constants.LevelSedentaryID.ToString() + ":" + 
							Catalog.GetString(Constants.LevelSedentary));
					level_row_show(false);

					combo_speciallities.Active = UtilGtk.ComboMakeActive(speciallitiesTranslated, 
							Catalog.GetString(Constants.SpeciallityUndefined));

					speciallity_row_show(false);
				}
				catch { LogB.Warning("do later"); }
			} else {
				//sport is not undefined and not none

				//if level is "sedentary", then change level to "undefined"
				if(UtilGtk.ComboGetActive(combo_levels) == 
						Constants.LevelSedentaryID.ToString() + ":" + 
						Catalog.GetString(Constants.LevelSedentary))
					combo_levels.Active = UtilGtk.ComboMakeActive(levels, 
							Constants.LevelUndefinedID.ToString() + ":" + 
							Catalog.GetString(Constants.LevelUndefined));

				//show level
				combo_levels.Sensitive = true;
				level_row_show(true);
		
				if(sport.HasSpeciallities) {
					combo_speciallities.Destroy();
					createComboSpeciallities(sport.UniqueID);
					speciallity_row_show(true);
				} else {
					LogB.Information("hide");
					combo_speciallities.Active = UtilGtk.ComboMakeActive(speciallitiesTranslated, 
							Catalog.GetString(Constants.SpeciallityUndefined));
					speciallity_row_show(false);
				}
			}
		} catch { 
			//LogB.Warning("do later");
		}

		LogB.Information("at on_combo_sports_changed " + sport.ToString());
		//labelUpdate();
	}

	private void on_combo_speciallities_changed(object o, EventArgs args) {
		LogB.Information("changed speciallities");
		labelUpdate();
		showHideButtonAccept();
	}

	private void on_combo_levels_changed(object o, EventArgs args) {
		//string myText = UtilGtk.ComboGetActive(combo_sports);
		LogB.Information("changed levels");
		//level = UtilGtk.ComboGetActive(combo_levels);
				
		//if it's sedentary, put sport to none
		/*
		 * Now undone because sedentary has renamed to "sedentary/Occasional practice"
		if(UtilGtk.ComboGetActive(combo_levels) == "0:" + Catalog.GetString(Constants.LevelSedentary))
			combo_sports.Active = UtilGtk.ComboMakeActive(sports, "2:" + Catalog.GetString(Constants.SportNone));
		*/
		labelUpdate();
		showHideButtonAccept();
	}
	
	private void labelUpdate() {
		string sportString = "";
		string speciallityString = "";
		string levelString = "";
		string pleaseDefineItString = " [" + Catalog.GetString("Please, define it") + "].";

		try {
			if(radiobutton_diff_sports.Active)
				sportString = Catalog.GetString("People in session practice different sports.");
			else {
				sportString = Catalog.GetString("All people in session practice the same sport:");
				int sportID = Convert.ToInt32(Util.FindOnArray(':', 2, 0, UtilGtk.ComboGetActive(combo_sports), sports));
				Sport mySport = SqliteSport.Select(false, sportID);

				if(sportID == Constants.SportUndefinedID)
					sportString += "<tt>" + pleaseDefineItString + "</tt>";
				else if(sportID == Constants.SportNoneID)
					sportString = Catalog.GetString("Nobody in this session practice sport.");
				else
					sportString += " <i>" + mySport.Name + "</i>.";

				if(label_speciallity.Visible) {
					if(radiobutton_diff_speciallities.Active)
						speciallityString = "\n" + Catalog.GetString("Different specialties.");
					else {
						speciallityString = "\n" + Catalog.GetString("This specialty:");
						if(UtilGtk.ComboGetActive(combo_speciallities) == Catalog.GetString(Constants.SpeciallityUndefined))
							speciallityString += "<tt>" + pleaseDefineItString + "</tt>";
						else
							speciallityString += " <i>" + UtilGtk.ComboGetActive(combo_speciallities) + "</i>.";
					}
				}
				if(label_level.Visible) {
					if(radiobutton_diff_levels.Active)
						levelString = "\n" + Catalog.GetString("Different levels.");
					else {
						levelString = "\n" + Catalog.GetString("This level:");
						int levelID =  Util.FetchID(UtilGtk.ComboGetActive(combo_levels));
						if(levelID == Constants.LevelUndefinedID)
							levelString += "<tt>" + pleaseDefineItString + "</tt>";
						else
							levelString += " <i>" + Util.FetchName(UtilGtk.ComboGetActive(combo_levels)) + "</i>.";
					}
				}
			}
			label_persons_data.Text= sportString + speciallityString + levelString;
			label_persons_data.UseMarkup = true;
		} catch {
			LogB.Warning("Do later");
		}
	}

	void on_button_cancel_clicked (object o, EventArgs args)
	{
		SessionAddEditWindowBox.session_add_edit.Hide();
		SessionAddEditWindowBox = null;
	}
	
	void on_session_add_edit_delete_event (object o, DeleteEventArgs args)
	{
		SessionAddEditWindowBox.session_add_edit.Hide();
		SessionAddEditWindowBox = null;
	}

	
	void on_button_change_date_clicked (object o, EventArgs args)
	{
		myDialogCalendar = new DialogCalendar(Catalog.GetString("Select session date"), dateTime);
		myDialogCalendar.FakeButtonDateChanged.Clicked += new EventHandler(on_calendar_changed);
	}

	void on_calendar_changed (object obj, EventArgs args)
	{
		dateTime = myDialogCalendar.MyDateTime;
		label_date.Text = dateTime.ToLongDateString();
	}

	void on_button_sport_add_clicked (object o, EventArgs args)
	{
		LogB.Information("sport add clicked");
		genericWin = GenericWindow.Show(Catalog.GetString("Add sport"),
				Catalog.GetString("Add new sport to database"),
				Constants.GenericWindowShow.ENTRY, true);
		genericWin.Button_accept.Clicked += new EventHandler(on_sport_add_accepted);
	}

	private void on_sport_add_accepted (object o, EventArgs args)
	{
		genericWin.Button_accept.Clicked -= new EventHandler(on_sport_add_accepted);

		string newSportName = genericWin.EntrySelected;
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
			sports = SqliteSport.SelectAll();
			//create sports translated, only with translated stuff
			sportsTranslated = new String[sports.Length];
			int i = 0;
			foreach(string row in sports) {
				string [] myStrFull = row.Split(new char[] {':'});
				sportsTranslated[i++] = myStrFull[2];
				}
		
			//sort array (except second row)
			System.Array.Sort(sportsTranslated, 2, sportsTranslated.Length-2);

			UtilGtk.ComboUpdate(combo_sports, sportsTranslated, mySport.ToString());
			combo_sports.Active = UtilGtk.ComboMakeActive(sportsTranslated, mySport.ToString());
		}
	}
	
	void on_button_accept_clicked (object o, EventArgs args)
	{
		//check if name of session exists (is owned by other session),
		//but all is ok if the name is the same as the old name (editing)
		string name = Util.RemoveTildeAndColon(entry_name.Text);
		name = Util.RemoveChar(name, '/');

		bool sessionNameExists = Sqlite.Exists (false, Constants.SessionTable, name);
		if(sessionNameExists && name != currentSession.Name ) {
			string myString = string.Format(Catalog.GetString("Session: '{0}' exists. Please, use another name"), name);
			ErrorWindow.Show(myString);
		} else {
			int sportID;
			if(radiobutton_diff_sports.Active)
				sportID = Constants.SportUndefinedID;
			else {
				sportID = Convert.ToInt32(Util.FindOnArray(':', 2, 0, UtilGtk.ComboGetActive(combo_sports), sports));
			}

			int speciallityID;
			if(!label_speciallity.Visible || radiobutton_diff_speciallities.Active)
				speciallityID = Constants.SpeciallityUndefinedID; 
			else
				speciallityID = Convert.ToInt32(Util.FindOnArray(':', 2, 0, UtilGtk.ComboGetActive(combo_speciallities), speciallities));

			int levelID;
			if(!label_level.Visible || radiobutton_diff_levels.Active)
				levelID = Constants.LevelUndefinedID;
			else
				levelID = Util.FetchID(UtilGtk.ComboGetActive(combo_levels));

			string place = Util.RemoveTildeAndColon(entry_place.Text);
			string comments = Util.RemoveTildeAndColon(textview.Buffer.Text);

			if(addSession) 
				currentSession = new Session (name, place,
						dateTime,
						sportID, speciallityID, levelID,
						comments,
						Constants.ServerUndefinedID);
			else {
				currentSession.Name = name;
				currentSession.Place = place;
				currentSession.Date = dateTime;
				currentSession.PersonsSportID = sportID;
				currentSession.PersonsSpeciallityID = speciallityID;
				currentSession.PersonsPractice = levelID;
				currentSession.Comments = comments;

				SqliteSession.Update(currentSession.UniqueID, currentSession.Name, 
						currentSession.Place, currentSession.Date, 
						sportID, speciallityID, levelID,
						currentSession.Comments);
			}
			
			FakeButtonAccept.Click();
		}
	}

	public void HideAndNull() {
		if (SessionAddEditWindowBox != null) {
			SessionAddEditWindowBox.session_add_edit.Hide();
			SessionAddEditWindowBox = null;
		}
	}

	public Gtk.Button FakeButtonAccept
	{
		get { return fakeButtonAccept; }
	}
	

	public Session CurrentSession 
	{
		get {
			return currentSession;
		}
	}

}
