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
 * Copyright (C) 2004-2016   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
using Gtk;
using Glade;
using GLib; //for Value
using System.Text; //StringBuilder
using System.Collections; //ArrayList
using Mono.Unix;


public class SessionAddEditWindow {
	
	[Widget] Gtk.Window session_add_edit;
	[Widget] Gtk.Entry entry_name;
	[Widget] Gtk.Entry entry_place;
	
	[Widget] Gtk.Label label_date;
	[Widget] Gtk.Button button_change_date;
	
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
	int speciallityID;
	string [] speciallities;
	string [] speciallitiesTranslated;
	String level;
	string [] levels;

	bool addSession;
	
	private Session currentSession;
	private Gtk.Button fakeButtonAccept;
	
	GenericWindow genericWin;
	static SessionAddEditWindow SessionAddEditWindowBox;
	Gtk.Window parent;
	
	
	SessionAddEditWindow (Gtk.Window parent, Session currentSession) {
		Glade.XML gladeXML;
		gladeXML = Glade.XML.FromAssembly (Util.GetGladePath() + "chronojump.glade", "session_add_edit", null);
		gladeXML.Autoconnect(this);
		this.parent = parent;
		
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
			Sport mySport = SqliteGeneral.SqliteSport.Select(false, currentSession.PersonsSportID);
			combo_sports.Active = UtilGtk.ComboMakeActive(sportsTranslated, mySport.ToString());

			if(sport.HasSpeciallities) {
				combo_speciallities.Destroy();
				createComboSpeciallities(mySport.UniqueID);
				speciallity_row_show(true);

				if(currentSession.PersonsSpeciallityID != Constants.SpeciallityUndefinedID) { 
					radiobutton_same_speciallity.Active = true;
					combo_speciallities.Active = UtilGtk.ComboMakeActive(speciallitiesTranslated,
						       SqliteGeneral.SqliteSpeciallity.Select(false, currentSession.PersonsSpeciallityID));
						       
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

	void on_entries_required_changed (object o, EventArgs args) {
		showHideButtonAccept();
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
		sports = SqliteGeneral.SqliteSport.SelectAll();
		
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
		speciallities = SqliteGeneral.SqliteSpeciallity.SelectAll(true, sportID); //show undefined, filter by sport

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
		levels = Constants.Levels;
		
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
			sport = SqliteGeneral.SqliteSport.Select(false, sportID);

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
				Sport mySport = SqliteGeneral.SqliteSport.Select(false, sportID);

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
		genericWin = GenericWindow.Show(Catalog.GetString("Add new sport to database"), Constants.GenericWindowShow.ENTRY);
		genericWin.Button_accept.Clicked += new EventHandler(on_sport_add_accepted);
	}

	private void on_sport_add_accepted (object o, EventArgs args) {
		genericWin.Button_accept.Clicked -= new EventHandler(on_sport_add_accepted);
		string newSportName = genericWin.EntrySelected;
		if(SqliteGeneral.Sqlite.Exists(false, Constants.SportTable, newSportName) ||
				newSportName == Catalog.GetString(Constants.SportUndefined) || //let's save problems
				newSportName == Catalog.GetString(Constants.SportNone)		//let's save problems
				)
				new DialogMessage(Constants.MessageTypes.WARNING, string.Format(
							Catalog.GetString("Sorry, this sport '{0}' already exists in database"), 
							newSportName));
		else {
			int myID = SqliteGeneral.SqliteSport.Insert(false, "-1", newSportName, true, //dbconOpened, , userDefined
					false, "");	//hasSpeciallities, graphLink 

			Sport mySport = new Sport(myID, newSportName, true, 
					false, "");	//hasSpeciallities, graphLink 
			sports = SqliteGeneral.SqliteSport.SelectAll();
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
		name = Util.RemoveChar(name, '"');

		bool sessionNameExists = SqliteGeneral.Sqlite.Exists (false, Constants.SessionTable, name);
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
			place = Util.RemoveChar(place, '"');
			comments = Util.RemoveChar(comments, '"');

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

				SqliteGeneral.SqliteSession.Update(currentSession.UniqueID, currentSession.Name, 
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


public class SessionLoadWindow {
	
	[Widget] Gtk.Window session_load;
	
	private TreeStore store;
	private string selected;
	[Widget] Gtk.TreeView treeview_session_load;
	[Widget] Gtk.Button button_accept;
	[Widget] Gtk.Entry entry_search_filter;
	[Widget] Gtk.CheckButton checkbutton_show_data_jump_run;
	[Widget] Gtk.CheckButton checkbutton_show_data_encoder;

	static SessionLoadWindow SessionLoadWindowBox;
	Gtk.Window parent;
	
	private Session currentSession;
	
	SessionLoadWindow (Gtk.Window parent) {
		Glade.XML gladeXML;
		gladeXML = Glade.XML.FromAssembly (Util.GetGladePath() + "chronojump.glade", "session_load", null);
		gladeXML.Autoconnect(this);
		this.parent = parent;
		
		//put an icon to window
		UtilGtk.IconWindow(session_load);
		
		createTreeView(treeview_session_load, false, false);
		store = getStore(false, false);
		treeview_session_load.Model = store;
		fillTreeView(treeview_session_load, store, false, false);

		button_accept.Sensitive = false;
		entry_search_filter.CanFocus = true;
		entry_search_filter.IsFocus = true;

		treeview_session_load.Selection.Changed += onSelectionEntry;
	}

	private TreeStore getStore(bool showContacts, bool showEncoder) {
		TreeStore s;
		if(showContacts && showEncoder)
			s = new TreeStore(
				typeof (string), typeof (string), typeof (string), typeof (string), //number, name, place, date
				typeof (string), typeof (string), typeof (string), typeof (string), //persons, sport, spllity, level
				typeof (string), typeof (string), typeof (string), typeof(string), //jumps s,r, runs s, i, 
				typeof (string), typeof (string), typeof (string), 	//rt, pulses, mc
				typeof (string), typeof (string), 			//encoder s, c
				typeof (string)						//comments
			       	);
		else if(showContacts && ! showEncoder)
			s = new TreeStore(
				typeof (string), typeof (string), typeof (string), typeof (string), //number, name, place, date
				typeof (string), typeof (string), typeof (string), typeof (string), //persons, sport, spllity, level
				typeof (string), typeof (string), typeof (string), typeof(string), //jumps s,r, runs s, i, 
				typeof (string), typeof (string), typeof (string), 	//rt, pulses, mc
				typeof (string)						//comments
			       	);
		else if(! showContacts && showEncoder)
			s = new TreeStore(
				typeof (string), typeof (string), typeof (string), typeof (string), //number, name, place, date
				typeof (string), typeof (string), typeof (string), typeof (string), //persons, sport, spllity, level
				typeof (string), typeof (string), 			//encoder s, c
				typeof (string)						//comments
			       	);
		else // ! showContacts && ! showEncoder
			s = new TreeStore(
				typeof (string), typeof (string), typeof (string), typeof (string), //number, name, place, date
				typeof (string), typeof (string), typeof (string), typeof (string), //persons, sport, spllity, level
				typeof (string)						//comments
			       	);
		return s;
	}

	
	static public SessionLoadWindow Show (Gtk.Window parent)
	{
		if (SessionLoadWindowBox == null) {
			SessionLoadWindowBox = new SessionLoadWindow (parent);
		}
		SessionLoadWindowBox.session_load.Show ();
		
		return SessionLoadWindowBox;
	}
	
	private void createTreeView (Gtk.TreeView tv, bool showContacts, bool showEncoder) {
		tv.HeadersVisible=true;
		int count = 0;
		
		tv.AppendColumn ( Catalog.GetString ("Number"), new CellRendererText(), "text", count++);
		tv.AppendColumn ( Catalog.GetString ("Name"), new CellRendererText(), "text", count++);
		tv.AppendColumn ( Catalog.GetString ("Place"), new CellRendererText(), "text", count++);
		tv.AppendColumn ( Catalog.GetString ("Date"), new CellRendererText(), "text", count++);
		tv.AppendColumn ( Catalog.GetString ("Persons"), new CellRendererText(), "text", count++);
		tv.AppendColumn ( Catalog.GetString ("Sport"), new CellRendererText(), "text", count++);
		tv.AppendColumn ( Catalog.GetString ("Specialty"), new CellRendererText(), "text", count++);
		tv.AppendColumn ( Catalog.GetString ("Level"), new CellRendererText(), "text", count++);
		if(showContacts) {
			tv.AppendColumn ( Catalog.GetString ("Jumps simple"), new CellRendererText(), "text", count++);
			tv.AppendColumn ( Catalog.GetString ("Jumps reactive"), new CellRendererText(), "text", count++);
			tv.AppendColumn ( Catalog.GetString ("Runs simple"), new CellRendererText(), "text", count++);
			tv.AppendColumn ( Catalog.GetString ("Runs interval"), new CellRendererText(), "text", count++);
			tv.AppendColumn ( Catalog.GetString ("Reaction time"), new CellRendererText(), "text", count++);
			tv.AppendColumn ( Catalog.GetString ("Pulses"), new CellRendererText(), "text", count++);
			tv.AppendColumn ( Catalog.GetString ("MultiChronopic"), new CellRendererText(), "text", count++);
		}
		if(showEncoder) {
			tv.AppendColumn ( Catalog.GetString ("Gravitatory encoder") + "\n" +
					Catalog.GetString("Sets") + " ; " + Catalog.GetString("Repetitions"),
					new CellRendererText(), "text", count++);
			tv.AppendColumn ( Catalog.GetString ("Inertial encoder") + "\n" +
					Catalog.GetString("Sets") + " ; " + Catalog.GetString("Repetitions"),
					new CellRendererText(), "text", count++);
		}
		tv.AppendColumn ( Catalog.GetString ("Comments"), new CellRendererText(), "text", count++);
	}
	
	protected void on_entry_search_filter_changed (object o, EventArgs args) {
		recreateTreeView("changed search filter");
	}
	void on_checkbutton_show_data_jump_run_toggled (object o, EventArgs args) {
		recreateTreeView("jump run " + checkbutton_show_data_jump_run.Active.ToString());
	}
	void on_checkbutton_show_data_encoder_toggled (object o, EventArgs args) {
		recreateTreeView("encoder " + checkbutton_show_data_encoder.Active.ToString());
	}
	void recreateTreeView(string message) {
		LogB.Information(message);

		UtilGtk.RemoveColumns(treeview_session_load);
		
		createTreeView(treeview_session_load, 
				checkbutton_show_data_jump_run.Active, checkbutton_show_data_encoder.Active);
		store = getStore(
				checkbutton_show_data_jump_run.Active, checkbutton_show_data_encoder.Active);
		treeview_session_load.Model = store;
		fillTreeView(treeview_session_load, store,
				checkbutton_show_data_jump_run.Active, checkbutton_show_data_encoder.Active);
		
		/*
		 * after clicking on checkbuttons, treeview row gets unselected
		 * call onSelectionEntry to see if there's a row selected
		 * and it will sensitive on/off button_accept as needed
		 */
		onSelectionEntry (treeview_session_load.Selection, new EventArgs ());
	}

	private void fillTreeView (Gtk.TreeView tv, TreeStore store, bool showContacts, bool showEncoder) 
	{
		string filterName = "";
		if(entry_search_filter.Text.ToString().Length > 0) 
			filterName = entry_search_filter.Text.ToString();
		
		string [] mySessions = SqliteGeneral.SqliteSession.SelectAllSessions(filterName); //returns a string of values separated by ':'
		foreach (string session in mySessions) {
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

			if(showContacts && showEncoder)
				store.AppendValues (myStringFull[0], myStringFull[1], 
						myStringFull[2], 
						myStringFull[3],	//session date
						myStringFull[8],	//number of jumpers x session
						mySport,		//personsSport
						mySpeciallity,		//personsSpeciallity
						myLevel,		//personsLevel
						myStringFull[9],	//number of jumps x session
						myStringFull[10],	//number of jumpsRj x session
						myStringFull[11], 	//number of runs x session
						myStringFull[12], 	//number of runsInterval x session
						myStringFull[13], 	//number of reaction times x session
						myStringFull[14], 	//number of pulses x session
						myStringFull[15], 	//number of multiChronopics x session
						myStringFull[16], 	//number of encoder signal x session
						myStringFull[17], 	//number of encoder curve x session
						myStringFull[7]		//description of session
						);
			else if(showContacts && ! showEncoder)
				store.AppendValues (myStringFull[0], myStringFull[1], 
						myStringFull[2], 
						myStringFull[3],	//session date
						myStringFull[8],	//number of jumpers x session
						mySport,		//personsSport
						mySpeciallity,		//personsSpeciallity
						myLevel,		//personsLevel
						myStringFull[9],	//number of jumps x session
						myStringFull[10],	//number of jumpsRj x session
						myStringFull[11], 	//number of runs x session
						myStringFull[12], 	//number of runsInterval x session
						myStringFull[13], 	//number of reaction times x session
						myStringFull[14], 	//number of pulses x session
						myStringFull[15], 	//number of multiChronopics x session
						myStringFull[7]		//description of session
						);
			else if(! showContacts && showEncoder)
				store.AppendValues (myStringFull[0], myStringFull[1], 
						myStringFull[2], 
						myStringFull[3],	//session date
						myStringFull[8],	//number of jumpers x session
						mySport,		//personsSport
						mySpeciallity,		//personsSpeciallity
						myLevel,		//personsLevel
						myStringFull[16], 	//number of encoder signal x session
						myStringFull[17], 	//number of encoder curve x session
						myStringFull[7]		//description of session
						);
			else // ! showContacts && ! showEncoder
				store.AppendValues (myStringFull[0], myStringFull[1], 
						myStringFull[2], 
						myStringFull[3],	//session date
						myStringFull[8],	//number of jumpers x session
						mySport,		//personsSport
						mySpeciallity,		//personsSpeciallity
						myLevel,		//personsLevel
						myStringFull[7]		//description of session
						);
		}	

	}
	
	//pass 0 for first row
	public void SelectRow(int rowNumber)
	{
		TreeIter iter;
		bool iterOk = store.GetIterFirst(out iter);
		if(iterOk) {
			int count = 0;
			while (count < rowNumber) {
				store.IterNext(ref iter);
				count ++;
			}
			treeview_session_load.Selection.SelectIter(iter);
		}
	}
	
	private void onSelectionEntry (object o, EventArgs args)
	{
		TreeModel model;
		TreeIter iter;
		selected = "-1";

		if (((TreeSelection)o).GetSelected(out model, out iter)) {
			selected = (string)model.GetValue (iter, 0);
			button_accept.Sensitive = true;
		} else
			button_accept.Sensitive = false;
	}
	
	void on_row_double_clicked (object o, Gtk.RowActivatedArgs args)
	{
		TreeView tv = (TreeView) o;
		TreeModel model;
		TreeIter iter;

		if (tv.Selection.GetSelected (out model, out iter)) {
			//put selection in selected
			selected = (string) model.GetValue (iter, 0);

			//activate on_button_accept_clicked()
			button_accept.Activate();
		}
	}
	
	void on_button_accept_clicked (object o, EventArgs args)
	{
		if(selected != "-1") {
			currentSession = SqliteGeneral.SqliteSession.Select (selected);
			SessionLoadWindowBox.session_load.Hide();
			SessionLoadWindowBox = null;
		}
	}
	
	void on_button_cancel_clicked (object o, EventArgs args)
	{
		SessionLoadWindowBox.session_load.Hide();
		SessionLoadWindowBox = null;
	}
	
	void on_session_load_delete_event (object o, DeleteEventArgs args)
	{
		SessionLoadWindowBox.session_load.Hide();
		SessionLoadWindowBox = null;
	}

	public Button Button_accept 
	{
		set { button_accept = value; }
		get { return button_accept; }
	}
	
	public Session CurrentSession 
	{
		get { return currentSession; }
	}

}

public class SessionSelectStatsWindow {
	
	[Widget] Gtk.Window stats_select_sessions;
	
	private TreeStore store1;
	private TreeStore store2;
	[Widget] Gtk.TreeView treeview1;
	[Widget] Gtk.TreeView treeview2;
	[Widget] Gtk.Button button_accept;

	static SessionSelectStatsWindow SessionSelectStatsWindowBox;
	Gtk.Window parent;
	
	private ArrayList arrayOfSelectedSessions;
	
	SessionSelectStatsWindow (Gtk.Window parent, ArrayList oldSelectedSessions) {
		Glade.XML gladeXML;
		gladeXML = Glade.XML.FromAssembly (Util.GetGladePath() + "chronojump.glade", "stats_select_sessions", "chronojump");
		gladeXML.Autoconnect(this);
		this.parent = parent;
		
		//put an icon to window
		UtilGtk.IconWindow(stats_select_sessions);
	
		createTreeView(treeview1);
		store1 = new TreeStore(typeof (string), typeof (string), typeof (string), typeof (string), typeof (string) );
		treeview1.Model = store1;
		fillTreeView(treeview1,store1);
		
		createTreeView(treeview2);
		store2 = new TreeStore(typeof (string), typeof (string), typeof (string), typeof (string), typeof (string) );
		treeview2.Model = store2;
		
		processOldSelectedSessions(treeview1, store1, store2, oldSelectedSessions);
	}
	
	static public SessionSelectStatsWindow Show (Gtk.Window parent, ArrayList oldSelectedSessions)
	{
		if (SessionSelectStatsWindowBox == null) {
			SessionSelectStatsWindowBox = new SessionSelectStatsWindow (parent, oldSelectedSessions);
		}
		SessionSelectStatsWindowBox.stats_select_sessions.Show ();
		
		return SessionSelectStatsWindowBox;
	}
	
	private void createTreeView (Gtk.TreeView tv) {
		tv.HeadersVisible=true;
		int count = 0;
		
		tv.AppendColumn ( Catalog.GetString ("Number"), new CellRendererText(), "text", count++);
		tv.AppendColumn ( Catalog.GetString ("Name"), new CellRendererText(), "text", count++);
		tv.AppendColumn ( Catalog.GetString ("Place"), new CellRendererText(), "text", count++);
		tv.AppendColumn ( Catalog.GetString ("Date"), new CellRendererText(), "text", count++);
		tv.AppendColumn ( Catalog.GetString ("Comments"), new CellRendererText(), "text", count++);
	}
	
	private void fillTreeView (Gtk.TreeView tv, TreeStore store) 
	{
		bool commentsDisable = false;
		int sessionIdDisable = -1; //don't disable any session (-1 as uniqueID is impossible)
		string [] mySessions = 
			SqliteGeneral.SqliteSession.SelectAllSessionsSimple(commentsDisable, sessionIdDisable);

		foreach (string session in mySessions) {
			string [] myStringFull = session.Split(new char[] {':'});

			store.AppendValues (myStringFull[0], myStringFull[1], 
					myStringFull[2], myStringFull[3], 
					myStringFull[4]		//description of session
					);
		}	

	}
	
	//oldSelectedSessions is an ArrayList with three cols (values of the old selectedSessions)
	//now, find iters corresponding to each of this sessions and put in the selected treeview, and delete from the unselected treeview
	private void processOldSelectedSessions (Gtk.TreeView treeview1, TreeStore store1, TreeStore store2, ArrayList oldSelectedSessions) {
		TreeIter iter1 = new TreeIter();
		string [] strIter = {"", "", "", "", ""};
		
		for (int i=0; i < oldSelectedSessions.Count ; i ++) {
			string [] str = oldSelectedSessions[i].ToString().Split(new char[] {':'});
			findRowForIter(treeview1, store1, out iter1, Convert.ToInt32(str[0]));

			for (int j=0; j < 5; j ++) {
				strIter [j] = (string) treeview1.Model.GetValue (iter1, j);
			}

			//print values
			store2.AppendValues (strIter[0], strIter[1], strIter[2], strIter[3], strIter[4]);

			//delete iter1
			store1.Remove(ref iter1);
		}
	}
	
	void on_button_select_clicked (object o, EventArgs args)
	{
		TreeModel model; 
		TreeIter iter1; //iter of the first treeview
		TreeIter iter2; //iter of second treeview. Used for search the row on we are going to insert
		TreeIter iter3; //new iter in first treeview
		int i;
		string [] str = {"", "", "", "", ""};

		if (treeview1.Selection.GetSelected (out model, out iter1)) {
			for (i=0; i < 5; i ++) {
				str[i] = (string) model.GetValue (iter1, i);
			}

			//create iter3
			iter3 = store2.AppendValues (str[0], str[1], str[2], str[3], str[4]);

			//move it where it has to be
			findRowForIter(treeview2, store2, out iter2, Convert.ToInt32(str[0]));
			store2.MoveBefore (iter3, iter2);
		
			//delete iter1
			store1.Remove(ref iter1);
		}
	}
		
	void on_button_unselect_clicked (object o, EventArgs args)
	{
		TreeModel model; 
		TreeIter iter1; //iter of first treeview. Used for search the row on we are going to insert
		TreeIter iter2; //iter of the second treeview
		TreeIter iter3; //new iter in first treeview
		int i;
		string [] str = {"", "", "", "", ""};

		if (treeview2.Selection.GetSelected (out model, out iter2)) {
			for (i=0; i < 5; i ++) {
				str[i] = (string) model.GetValue (iter2, i);
			}

			//create iter3
			iter3 = store1.AppendValues (str[0], str[1], str[2], str[3], str[4]);

			//move it where it has to be
			findRowForIter(treeview1, store1, out iter1, Convert.ToInt32(str[0]));
			store1.MoveBefore (iter3, iter1);
		
			//delete iter2
			store2.Remove(ref iter2);
		}
	}

	void findRowForIter (TreeView myTreeview, TreeStore myStore, out TreeIter myIter, int searchedPosition) 
	{
		int position;
		bool firstLap = true;

		myStore.GetIterFirst (out myIter);
		position = Convert.ToInt32( (string) myTreeview.Model.GetValue (myIter, 0) );

		do {
			if ( ! firstLap) {
				myStore.IterNext (ref myIter);
			}
			position = Convert.ToInt32( (string) myTreeview.Model.GetValue (myIter, 0) );
			firstLap = false;
		} while (position < searchedPosition );
	}
		
	void on_button_all_clicked (object o, EventArgs args)
	{
		//delete existing rows in treeview1
		store1.Clear();
		//also in treeview2 (for not having repeated rows)
		store2.Clear();
		
		//put all the values it treeview2 (from the sql)
		fillTreeView(treeview2,store2);
	}
		
	void on_button_none_clicked (object o, EventArgs args)
	{
		//delete existing rows in treeview2
		store2.Clear();
		//also in treeview1 (for not having repeated rows)
		store1.Clear();
		
		//put all the values it treeview1 (from the sql)
		fillTreeView(treeview1,store1);
	}
		
	void on_button_cancel_clicked (object o, EventArgs args)
	{
		SessionSelectStatsWindowBox.stats_select_sessions.Hide();
		SessionSelectStatsWindowBox = null;
	}
	
	void on_delete_event (object o, DeleteEventArgs args)
	{
		SessionSelectStatsWindowBox.stats_select_sessions.Hide();
		SessionSelectStatsWindowBox = null;
	}
	
	void on_button_accept_clicked (object o, EventArgs args)
	{
		prepareSelected (treeview2, store2);
		SessionSelectStatsWindowBox.stats_select_sessions.Hide();
		SessionSelectStatsWindowBox = null;
	}
	
	void prepareSelected (TreeView myTreeview, TreeStore myStore) 
	{
		TreeIter myIter = new TreeIter ();
		bool iterOk = true;
	
		arrayOfSelectedSessions = new ArrayList (2);

		for (int count=0 ; iterOk; count ++) {
			if (count == 0) {
				iterOk = myStore.GetIterFirst (out myIter);
			}
			else {
				iterOk = myStore.IterNext (ref myIter); 
			}
			
			if (iterOk) {
				arrayOfSelectedSessions.Add ( 
					(string) myTreeview.Model.GetValue (myIter, 0) + ":" +	//id
					(string) myTreeview.Model.GetValue (myIter, 1) + ":" +	//name
					(string) myTreeview.Model.GetValue (myIter, 3) 		//date (forget place)
					);
				LogB.Information(arrayOfSelectedSessions[count].ToString());
			}
		} 
	}
		
	public Button Button_accept 
	{
		set {
			button_accept = value;	
		}
		get {
			return button_accept;
		}
	}
	
	public ArrayList ArrayOfSelectedSessions 
	{
		get {
			if (arrayOfSelectedSessions.Count > 0) {
				return arrayOfSelectedSessions;
			} else {
				arrayOfSelectedSessions.Add("-1");
				return arrayOfSelectedSessions;
			}
		}
	}
	
}
