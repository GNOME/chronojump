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
using Gdk; //Pixbuf
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


public class SessionLoadWindow
{
	public enum WindowType
	{
		LOAD_SESSION,
		IMPORT_SESSION
	};
	[Widget] Gtk.Window session_load;
	
	private TreeStore store;
	private string selected;
	private string import_file_path;

	[Widget] Gtk.Notebook notebook_import;

	//notebook import tab 0
	[Widget] Gtk.RadioButton radio_import_new_session;
	[Widget] Gtk.RadioButton radio_import_current_session;
	[Widget] Gtk.Image image_open_database;

	//notebook import tab 1
	[Widget] Gtk.TreeView treeview_session_load;
	[Widget] Gtk.Button button_accept;
	[Widget] Gtk.Button button_import;
	[Widget] Gtk.Image image_import;
	[Widget] Gtk.Entry entry_search_filter;
	[Widget] Gtk.CheckButton checkbutton_show_data_jump_run;
	[Widget] Gtk.CheckButton checkbutton_show_data_other_tests;
	[Widget] Gtk.Label file_path_import;
	[Widget] Gtk.HButtonBox hbuttonbox_page1_load;
	[Widget] Gtk.HButtonBox hbuttonbox_page1_import;

	//notebook import tab 2
	[Widget] Gtk.Label label_import_session_name;
	[Widget] Gtk.Label label_import_file;
	[Widget] Gtk.Button button_import_confirm_accept;

	//notebook import tab 3
	[Widget] Gtk.ProgressBar progressbarImport;
	[Widget] Gtk.Label label_import_done_at_new_session;
	[Widget] Gtk.Label label_import_done_at_current_session;
	[Widget] Gtk.ScrolledWindow scrolledwindow_import_error;
	[Widget] Gtk.TextView textview_import_error;
	[Widget] Gtk.Image image_import1;
	[Widget] Gtk.HButtonBox hbuttonbox_page3;


	static SessionLoadWindow SessionLoadWindowBox;
	
	private Session currentSession;
	private WindowType type;

	const int PAGE_IMPORT_START = 0;
	const int PAGE_SELECT_SESSION = 1; //for load session and for import
	public const int PAGE_IMPORT_CONFIRM = 2;
	public const int PAGE_IMPORT_RESULT = 3;

	SessionLoadWindow (Gtk.Window parent)
	{
		Glade.XML gladeXML;
		gladeXML = Glade.XML.FromAssembly (Util.GetGladePath() + "session_load.glade", "session_load", null);
		gladeXML.Autoconnect(this);
		session_load.Parent = parent;
	}

	private void initializeGui()
	{
		if (type == WindowType.LOAD_SESSION) {
			file_path_import.Visible = false;
			hbuttonbox_page1_load.Visible = true;
			hbuttonbox_page1_import.Visible = false;
			session_load.Title = Catalog.GetString ("Load session");
			notebook_import.CurrentPage = PAGE_SELECT_SESSION;
		} else {
			file_path_import.Visible = true;
			hbuttonbox_page1_load.Visible = false;
			hbuttonbox_page1_import.Visible = true;
			session_load.Title = Catalog.GetString ("Import session");
			notebook_import.CurrentPage = PAGE_IMPORT_START;
		}

		//put an icon to window
		UtilGtk.IconWindow(session_load);

		image_open_database.Pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "folder_open.png");
		image_import.Pixbuf = new Pixbuf (null, Util.GetImagePath(false) + Constants.FileNameImport);
		image_import1.Pixbuf = new Pixbuf (null, Util.GetImagePath(false) + Constants.FileNameImport);

		createTreeView(treeview_session_load, false, false);
		store = getStore(false, false);
		treeview_session_load.Model = store;
		fillTreeView(treeview_session_load, store, false, false);

		store.SetSortColumnId(1, Gtk.SortType.Descending); //date
		store.ChangeSortColumn();

		button_accept.Sensitive = false;
		button_import.Sensitive = false;
		entry_search_filter.CanFocus = true;
		entry_search_filter.IsFocus = true;

		// Leave the state of the Importing Comboboxes as they are by default
		/*radio_import_new_session.Active = true;
		radio_import_current_session.Sensitive = false;
		*/

		treeview_session_load.Selection.Changed += onSelectionEntry;

		/**
		* Uncomment if we want the session file chooser to be loaded with the dialog.
		* On Linux at least the placement of the Windows can be strange so at the moment
		* I leave it disabled until we discuss (to enable or to delete it).
		if (type == WindowType.IMPORT_SESSION) {
			chooseDatabaseToImport ();
		}
		*/
	}

	private TreeStore getStore(bool showContacts, bool showEncoderAndForceSensor) {
		TreeStore s;
		if(showContacts && showEncoderAndForceSensor)
			s = new TreeStore(
				typeof (string), typeof (string), typeof (string), typeof (string),  	//number (hidden), date, name, place
				typeof (string), typeof (string), typeof (string), typeof (string), 	//persons, sport, spllity, level
				typeof (string), typeof (string), typeof (string), typeof(string), 	//jumps s,r, runs s, i,
				typeof (string), typeof (string), typeof (string), 			//rt, pulses, mc
				typeof (string), typeof (string), typeof (string), typeof (string),	//encoder s, c, forceSensor, runEncoder
				typeof (string)								//comments
			       	);
		else if(showContacts && ! showEncoderAndForceSensor)
			s = new TreeStore(
				typeof (string), typeof (string), typeof (string), typeof (string), 	//number (hidden), date, name, place
				typeof (string), typeof (string), typeof (string), typeof (string), 	//persons, sport, spllity, level
				typeof (string), typeof (string), typeof (string), typeof(string), 	//jumps s,r, runs s, i,
				typeof (string), typeof (string), typeof (string), 			//rt, pulses, mc
				typeof (string)								//comments
			       	);
		else if(! showContacts && showEncoderAndForceSensor)
			s = new TreeStore(
				typeof (string), typeof (string), typeof (string), typeof (string), 	//number (hidden), date, name, place
				typeof (string), typeof (string), typeof (string), typeof (string), 	//persons, sport, spllity, level
				typeof (string), typeof (string), typeof (string), typeof (string),	//encoder s, c, forceSensor, runEncoder
				typeof (string)								//comments
			       	);
		else // ! showContacts && ! showEncoderAndForceSensor
			s = new TreeStore(
				typeof (string), typeof (string), typeof (string), typeof (string), 	//number (hidden), date, name, place
				typeof (string), typeof (string), typeof (string), typeof (string), 	//persons, sport, spllity, level
				typeof (string)								//comments
			       	);

		//s.SetSortFunc (0, UtilGtk.IdColumnCompare); //not needed, it's hidden
		s.SetSortFunc (1, dateColumnCompare);
		s.ChangeSortColumn();

		return s;
	}


	private static int dateColumnCompare (TreeModel model, TreeIter iter1, TreeIter iter2)
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
	
	static public SessionLoadWindow Show (Gtk.Window parent, WindowType type)
	{
		if (SessionLoadWindowBox == null) {
			SessionLoadWindowBox = new SessionLoadWindow (parent);
		}

		SessionLoadWindowBox.type = type;
		SessionLoadWindowBox.initializeGui();
		SessionLoadWindowBox.recreateTreeView("loaded the dialog");

		SessionLoadWindowBox.session_load.Show ();
		
		return SessionLoadWindowBox;
	}
	
	private void createTreeView (Gtk.TreeView tv, bool showContacts, bool showOtherTests)
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
		
		tv.AppendColumn ( Catalog.GetString ("Place"), new CellRendererText(), "text", count++);
		tv.AppendColumn ( Catalog.GetString ("Persons"), new CellRendererText(), "text", count++);
		tv.AppendColumn ( Catalog.GetString ("Sport"), new CellRendererText(), "text", count++);
		tv.AppendColumn ( Catalog.GetString ("Specialty"), new CellRendererText(), "text", count++);
		tv.AppendColumn ( Catalog.GetString ("Level"), new CellRendererText(), "text", count++);
		if(showContacts) {
			tv.AppendColumn ( Catalog.GetString ("Jumps simple"), new CellRendererText(), "text", count++);
			tv.AppendColumn ( Catalog.GetString ("Jumps reactive"), new CellRendererText(), "text", count++);
			tv.AppendColumn ( Catalog.GetString ("Races simple"), new CellRendererText(), "text", count++);
			tv.AppendColumn ( Catalog.GetString ("Races interval"), new CellRendererText(), "text", count++);
			tv.AppendColumn ( Catalog.GetString ("Reaction time"), new CellRendererText(), "text", count++);
			tv.AppendColumn ( Catalog.GetString ("Pulses"), new CellRendererText(), "text", count++);
			tv.AppendColumn ( Catalog.GetString ("MultiChronopic"), new CellRendererText(), "text", count++);
		}
		if(showOtherTests) {
			tv.AppendColumn ( Catalog.GetString ("Gravitatory encoder") + "\n" +
					Catalog.GetString("Sets") + " ; " + Catalog.GetString("Repetitions"),
					new CellRendererText(), "text", count++);
			tv.AppendColumn ( Catalog.GetString ("Inertial encoder") + "\n" +
					Catalog.GetString("Sets") + " ; " + Catalog.GetString("Repetitions"),
					new CellRendererText(), "text", count++);
			tv.AppendColumn ( Catalog.GetString ("Force sensor"), new CellRendererText(), "text", count++);
			tv.AppendColumn ( Catalog.GetString ("Race analyzer"), new CellRendererText(), "text", count++);
		}
		tv.AppendColumn ( Catalog.GetString ("Comments"), new CellRendererText(), "text", count++);
	}
	
	protected void on_entry_search_filter_changed (object o, EventArgs args) {
		recreateTreeView("changed search filter");
	}

	private void chooseDatabaseToImport()
	{
		Gtk.FileChooserDialog filechooser = new Gtk.FileChooserDialog ("Choose ChronoJump database to import from",
		                                                               session_load, FileChooserAction.Open,
		                                                               "Cancel",ResponseType.Cancel,
		                                                               "Open",ResponseType.Accept);

		FileFilter file_filter = new FileFilter();
		file_filter.AddPattern ("*.db");
		file_filter.Name = "ChronoJump database";
		filechooser.AddFilter (file_filter);

		if (filechooser.Run () == (int)ResponseType.Accept) {
			import_file_path = filechooser.Filename;
			//file_path_import.Text = System.IO.Path.GetFileName (import_file_path);
			file_path_import.Text = import_file_path;
			file_path_import.TooltipText = import_file_path;
			notebook_import.CurrentPage = PAGE_SELECT_SESSION;
		}
		filechooser.Destroy ();
		recreateTreeView ("file path changed");
	}
	void on_checkbutton_show_data_jump_run_toggled (object o, EventArgs args) {
		recreateTreeView("jump run " + checkbutton_show_data_jump_run.Active.ToString());
	}
	void on_checkbutton_show_data_other_tests_toggled (object o, EventArgs args) {
		recreateTreeView("other tests " + checkbutton_show_data_other_tests.Active.ToString());
	}
	void recreateTreeView(string message)
	{
		LogB.Information("Recreate treeview: " + message);

		UtilGtk.RemoveColumns(treeview_session_load);
		
		createTreeView(treeview_session_load, 
				checkbutton_show_data_jump_run.Active, checkbutton_show_data_other_tests.Active);
		store = getStore(
				checkbutton_show_data_jump_run.Active, checkbutton_show_data_other_tests.Active);
		treeview_session_load.Model = store;
		fillTreeView(treeview_session_load, store,
				checkbutton_show_data_jump_run.Active, checkbutton_show_data_other_tests.Active);
		
		store.SetSortColumnId(1, Gtk.SortType.Descending); //date
		store.ChangeSortColumn();


		/*
		 * after clicking on checkbuttons, treeview row gets unselected
		 * call onSelectionEntry to see if there's a row selected
		 * and it will sensitive on/off button_accept as needed
		 */
		onSelectionEntry (treeview_session_load.Selection, new EventArgs ());
	}

	private void fillTreeView (Gtk.TreeView tv, TreeStore store, bool showContacts, bool showOtherTests)
	{
		string filterName = "";
		if(entry_search_filter.Text.ToString().Length > 0) 
			filterName = entry_search_filter.Text.ToString();

		SqliteSessionSwitcher.DatabaseType databaseType;
		if (type == WindowType.LOAD_SESSION) {
			databaseType = SqliteSessionSwitcher.DatabaseType.DEFAULT;
		} else {
			databaseType = SqliteSessionSwitcher.DatabaseType.SPECIFIC;
		}
		SqliteSessionSwitcher sessionSwitcher = new SqliteSessionSwitcher (databaseType, import_file_path);
		
		string [] mySessions = sessionSwitcher.SelectAllSessions(filterName); //returns a string of values separated by ':'
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

			if(showContacts && showOtherTests)
				store.AppendValues (
						myStringFull[0], 	//session num
						myStringFull[3],	//session date
						myStringFull[1], 	//session name
						myStringFull[2], 	//session place
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
						myStringFull[18], 	//number of forceSensor
						myStringFull[19], 	//number of runEncoder
						myStringFull[7]		//description of session
						);
			else if(showContacts && ! showOtherTests)
				store.AppendValues (
						myStringFull[0], 	//session num
						myStringFull[3],	//session date
						myStringFull[1], 	//session name
						myStringFull[2], 	//session place
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
			else if(! showContacts && showOtherTests)
				store.AppendValues (
						myStringFull[0], 	//session num
						myStringFull[3],	//session date
						myStringFull[1], 	//session name
						myStringFull[2], 	//session place
						myStringFull[8],	//number of jumpers x session
						mySport,		//personsSport
						mySpeciallity,		//personsSpeciallity
						myLevel,		//personsLevel
						myStringFull[16], 	//number of encoder signal x session
						myStringFull[17], 	//number of encoder curve x session
						myStringFull[18], 	//number of forceSensor
						myStringFull[19], 	//number of runEncoder
						myStringFull[7]		//description of session
						);
			else // ! showContacts && ! showOtherTests
				store.AppendValues (
						myStringFull[0], 	//session num
						myStringFull[3],	//session date
						myStringFull[1], 	//session name
						myStringFull[2], 	//session place
						myStringFull[8],	//number of jumpers x session
						mySport,		//personsSport
						mySpeciallity,		//personsSpeciallity
						myLevel,		//personsLevel
						myStringFull[7]		//description of session
						);
		}	

	}
	
	//pass 0 for first row
	public void SelectRowByPos(int rowNumber)
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

	public bool SelectRowByID(int searchedID)
	{
		return UtilGtk.TreeviewSelectRowWithID(
				treeview_session_load, store, 0, searchedID, false);
	}
	
	private void onSelectionEntry (object o, EventArgs args)
	{
		TreeModel model;
		TreeIter iter;
		selected = "-1";

		if (((TreeSelection)o).GetSelected(out model, out iter)) {
			selected = (string)model.GetValue (iter, 0);
			button_accept.Sensitive = true;
			button_import.Sensitive = true;
		} else {
			button_accept.Sensitive = false;
			button_import.Sensitive = false;
		}
	}

	public int CurrentSessionId() {
		TreeModel model;
		TreeIter iter;

		if (treeview_session_load.Selection.GetSelected (out model, out iter)) {
			string selected = (string)model.GetValue (iter, 0);
			return Convert.ToInt32 (selected.Split (':')[0]);
		}
		return -1;
	}

	public string ImportDatabasePath() {
		return import_file_path;
	}

	public bool ImportToNewSession() {
		return radio_import_new_session.Active;
	}

	public void DisableImportToCurrentSession() {
		radio_import_new_session.Active = true;
		radio_import_current_session.Sensitive = false;
	}

	public void LabelImportSessionName (string str)
	{
		label_import_session_name.Text = str;
	}
	public void LabelImportFile (string str)
	{
		label_import_file.Text = str;
	}

	public void Pulse(string str)
	{
		progressbarImport.Pulse();
		progressbarImport.Text = str;
	}
	public void PulseEnd()
	{
		progressbarImport.Fraction = 1;
		hbuttonbox_page3.Sensitive = true;
	}

	public void ShowLabelImportedOk()
	{
		if(radio_import_new_session.Active)
			label_import_done_at_new_session.Visible = true;
		else
			label_import_done_at_current_session.Visible = true;
	}

	public void ShowImportError(string str)
	{
		scrolledwindow_import_error.Visible = true;
		textview_import_error.Buffer.Text = str;
	}

	public void NotebookPage(int i)
	{
		if(i == PAGE_IMPORT_RESULT)
			hbuttonbox_page3.Sensitive = false;

		notebook_import.CurrentPage = i;
	}

	//import notebook page 0 buttons
	void on_button_cancel0_clicked (object o, EventArgs args)
	{
		SessionLoadWindowBox.session_load.Hide();
		SessionLoadWindowBox = null;
	}
	protected void on_select_file_import_clicked(object o, EventArgs args) {
		chooseDatabaseToImport ();
	}

	//import notebook page 1 (load sesion) buttons
	void on_button_cancel1_clicked (object o, EventArgs args)
	{
		SessionLoadWindowBox.session_load.Hide();
		SessionLoadWindowBox = null;
	}

	void on_row_double_clicked (object o, Gtk.RowActivatedArgs args)
	{
		TreeView tv = (TreeView) o;
		TreeModel model;
		TreeIter iter;

		LogB.Information("double! type: " + type.ToString());
		if (tv.Selection.GetSelected (out model, out iter)) {
			//put selection in selected
			selected = (string) model.GetValue (iter, 0);

			if (type == WindowType.LOAD_SESSION) {
				//activate on_button_accept_clicked()
				button_accept.Activate();
			} else {
				button_import.Activate();
			}
		}
	}
	
	void on_button_accept_clicked (object o, EventArgs args)
	{
		if(selected != "-1") {
			currentSession = SqliteSession.Select (selected);
			SessionLoadWindowBox.session_load.Hide();
			SessionLoadWindowBox = null;
		}
	}

	//import notebook page 1 (import) buttons
	void on_button_back_clicked (object o, EventArgs args)
	{
		notebook_import.CurrentPage = PAGE_IMPORT_START;
	}
	void on_button_import_clicked (object o, EventArgs args)
	{
		if(selected != "-1") {
			currentSession = SqliteSession.Select (selected);
		}
	}

	//import notebook page 2 buttons
	private void on_button_import_confirm_back_clicked(object o, EventArgs args)
	{
		notebook_import.CurrentPage = PAGE_IMPORT_START;
	}
	private void on_button_import_confirm_accept_clicked(object o, EventArgs args)
	{
		hbuttonbox_page3.Sensitive = false;
		notebook_import.CurrentPage = PAGE_IMPORT_RESULT;
	}

	//import notebook page 3 buttons
	private void on_button_import_close_clicked(object o, EventArgs args)
	{
		SessionLoadWindowBox.session_load.Hide();
		SessionLoadWindowBox = null;
	}
	private void on_button_import_again_clicked(object o, EventArgs args)
	{
		label_import_done_at_new_session.Visible = false;
		label_import_done_at_current_session.Visible = false;
		scrolledwindow_import_error.Visible = false;

		notebook_import.CurrentPage = PAGE_IMPORT_START;
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
	public Button Button_import
	{
		get { return button_import; }
	}
	public Button Button_import_confirm_accept
	{
		get { return button_import_confirm_accept; }
	}
	
	public Session CurrentSession 
	{
		get { return currentSession; }
	}

}

public class SessionSelectStatsWindow
{
	[Widget] Gtk.Window stats_select_sessions;
	
	private TreeStore store1;
	private TreeStore store2;
	[Widget] Gtk.TreeView treeview1;
	[Widget] Gtk.TreeView treeview2;
	[Widget] Gtk.Button button_accept;

	static SessionSelectStatsWindow SessionSelectStatsWindowBox;
	
	private ArrayList arrayOfSelectedSessions;
	
	SessionSelectStatsWindow (Gtk.Window parent, ArrayList oldSelectedSessions) {
		Glade.XML gladeXML;
		gladeXML = Glade.XML.FromAssembly (Util.GetGladePath() + "stats_select_sessions.glade", "stats_select_sessions", "chronojump");
		gladeXML.Autoconnect(this);
		stats_select_sessions.Parent = parent;
		
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
			SqliteSession.SelectAllSessionsSimple(commentsDisable, sessionIdDisable);

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
