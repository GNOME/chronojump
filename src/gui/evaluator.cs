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
using Gdk;
//using Glade;
using GLib; //for Value
using Mono.Unix;


public class EvaluatorWindow
{
	// at glade ---->
	Gtk.Window evaluator_window;
	Gtk.Entry entry_name;
	Gtk.Entry entry_email;
	Gtk.Label label_date;
	Gtk.Box hbox_combo_continents;
	Gtk.Box hbox_combo_countries;
	Gtk.Label label_confiable;
	Gtk.TextView textview_comments;

	//chronometer tab
	Gtk.RadioButton radio_cp_undef;
	Gtk.RadioButton radio_cp1;
	Gtk.RadioButton radio_cp2;
	Gtk.RadioButton radio_cp3;
	Gtk.RadioButton radio_cp_other;
	Gtk.Image image_cp1;
	Gtk.Image image_cp2;
	Gtk.Image image_cp3;
	Gtk.Image image_zoom_cp1;
	Gtk.Image image_zoom_cp2;
	Gtk.Image image_zoom_cp3;
	Gtk.Entry entry_cp_other;
	Gtk.Button button_zoom_cp1;
	Gtk.Button button_zoom_cp2;
	Gtk.Button button_zoom_cp3;
	
	//devices tab
	Gtk.RadioButton radio_device_undef;
	Gtk.RadioButton radio_contact_steel;
	Gtk.RadioButton radio_contact_modular;
	Gtk.RadioButton radio_infrared;
	Gtk.RadioButton radio_device_other;
	Gtk.Entry entry_device_other;
	Gtk.Image image_contact_steel;
	Gtk.Image image_contact_modular;
	Gtk.Image image_infrared;
	Gtk.Image image_zoom_contact_steel;
	Gtk.Image image_zoom_contact_modular;
	Gtk.Image image_zoom_infrared;
	Gtk.Button button_zoom_contact_steel;
	Gtk.Button button_zoom_contact_modular;
	Gtk.Button button_zoom_infrared;

	Gtk.Button button_accept;
	// <---- at glade
	
	Gtk.ComboBoxText combo_continents;
	Gtk.ComboBoxText combo_countries;

	string [] continents;
	string [] continentsTranslated;
	string [] countries;
	string [] countriesTranslated;

	DialogCalendar myDialogCalendar;
	System.DateTime dateTime;

	ServerEvaluator eval;
	ServerEvaluator evalBefore;
	bool changed;
	bool creating; //true if no record found before. False if updating
	
	//allows to upload data (from gui/chronojump.cs) after has been inserted in sql
	public Gtk.Button fakeButtonAccept;

	static EvaluatorWindow EvaluatorWindowBox;
	
	public EvaluatorWindow (ServerEvaluator eval)
	{
		//Setup (text, table, uniqueID);
		/*
		Glade.XML gladeXML;
		gladeXML = Glade.XML.FromAssembly (Util.GetGladePath() + "evaluator_window.glade", "evaluator_window", "chronojump");
		gladeXML.Autoconnect(this);
		*/
		Gtk.Builder builder = new Gtk.Builder (null, Util.GetGladePath () + "evaluator_window.glade", null);
		connectWidgets (builder);
		builder.Autoconnect (this);
		
		//put an icon to window
		UtilGtk.IconWindow(evaluator_window);
		
		fakeButtonAccept = new Gtk.Button();

		this.eval = eval;

		if(eval.UniqueID == -1)
			creating = true;
		
		//copy to see if there are changes
		evalBefore = new ServerEvaluator(eval);
		
		createComboContinents();
		createComboCountries();
		
		putNonStandardIcons();	
		

		entry_cp_other.Sensitive = false;
	}

	static public EvaluatorWindow Show (ServerEvaluator eval)
	{
		if (EvaluatorWindowBox == null) {
			EvaluatorWindowBox = new EvaluatorWindow(eval);
		}
		EvaluatorWindowBox.evaluator_window.Show ();
		
		EvaluatorWindowBox.fillDialog ();
		
		
		return EvaluatorWindowBox;
	}
		
	private void putNonStandardIcons() {
		Pixbuf pixbuf;
		
		pixbuf = new Pixbuf (null, Util.GetImagePath(true) + Constants.FileNameChronopic1);
		image_cp1.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(true) + Constants.FileNameChronopic2);
		image_cp2.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(true) + Constants.FileNameChronopic3);
		image_cp3.Pixbuf = pixbuf;
					
		pixbuf = new Pixbuf (null, Util.GetImagePath(true) + Constants.FileNameContactPlatformSteel);
		image_contact_steel.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(true) + Constants.FileNameContactPlatformModular);
		image_contact_modular.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(true) + Constants.FileNameInfrared);
		image_infrared.Pixbuf = pixbuf;
					
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + Constants.FileNameZoomInIcon);
		image_zoom_cp1.Pixbuf = pixbuf;
		image_zoom_cp2.Pixbuf = pixbuf;
		image_zoom_cp3.Pixbuf = pixbuf;
		image_zoom_contact_steel.Pixbuf = pixbuf;
		image_zoom_contact_modular.Pixbuf = pixbuf;
		image_zoom_infrared.Pixbuf = pixbuf;
		
		cp_zoom_buttons_unsensitive();
		device_zoom_buttons_unsensitive();
	}
	
	private void createComboContinents() {
		combo_continents = new ComboBoxText ();
		continents = Constants.ContinentsStr();

		//create continentsTranslated, only with translated stuff
		continentsTranslated = new String[Constants.ContinentsStr().Length];
		int i = 0;
		foreach(string continent in continents) 
			continentsTranslated[i++] = Util.FetchName(continent);

		UtilGtk.ComboUpdate(combo_continents, continentsTranslated, "");
		combo_continents.Active = UtilGtk.ComboMakeActive(continentsTranslated, 
				Catalog.GetString(Constants.ContinentUndefined));

		combo_continents.Changed += new EventHandler (on_combo_continents_changed);

		hbox_combo_continents.PackStart(combo_continents, true, true, 0);
		hbox_combo_continents.ShowAll();
		combo_continents.Sensitive = true;
	}

	private void createComboCountries() {
		combo_countries = new ComboBoxText ();

		countries = new String[1];
		//record countries with id:english name:translatedName
		countries [0] = Constants.CountryUndefinedID + ":" + Constants.CountryUndefined + ":" + Catalog.GetString(Constants.CountryUndefined);

		string [] myCountries = new String[1];
		myCountries [0] = Catalog.GetString(Constants.CountryUndefined);
		UtilGtk.ComboUpdate(combo_countries, myCountries, "");
		combo_countries.Active = UtilGtk.ComboMakeActive(myCountries, 
				Catalog.GetString(Constants.CountryUndefined));

		
		combo_countries.Changed += new EventHandler (on_combo_countries_changed);

		hbox_combo_countries.PackStart(combo_countries, true, true, 0);
		hbox_combo_countries.ShowAll();
		combo_countries.Sensitive = false;
	}

	private void on_combo_continents_changed(object o, EventArgs args) {
		if(UtilGtk.ComboGetActive(combo_continents) == Catalog.GetString(Constants.ContinentUndefined)) {
			countries [0] = Constants.CountryUndefinedID + ":" + Constants.CountryUndefined + ":" + Catalog.GetString(Constants.CountryUndefined);
			countriesTranslated = new String[1];
			countriesTranslated [0] = Catalog.GetString(Constants.CountryUndefined);
			combo_countries.Sensitive = false;
		}
		else {
			//get the active continent
			string continentEnglish = Util.FindOnArray(':', 1, 0, UtilGtk.ComboGetActive(combo_continents), continents); 
			countries = SqliteCountry.SelectCountriesOfAContinent(continentEnglish, true); //put undefined first

			//create countries translated, only with translated stuff
			countriesTranslated = new String[countries.Length];
			int i = 0;
			foreach(string row in countries) {
				string [] myStrFull = row.Split(new char[] {':'});
				countriesTranslated[i++] = myStrFull[2];
			}
		}
		//sort array (except first row)
		System.Array.Sort(countriesTranslated, 1, countriesTranslated.Length-1);

		UtilGtk.ComboUpdate(combo_countries, countriesTranslated, "");
		combo_countries.Active = UtilGtk.ComboMakeActive(countriesTranslated, 
				Catalog.GetString(Constants.CountryUndefined));

		combo_countries.Sensitive = true;

		on_entries_required_changed(new object(), new EventArgs());
	}

	private void on_combo_countries_changed(object o, EventArgs args) {
		on_entries_required_changed(new object(), new EventArgs());
	}

	private void cp_zoom_buttons_unsensitive() {
		button_zoom_cp1.Sensitive = false;
		button_zoom_cp2.Sensitive = false;
		button_zoom_cp3.Sensitive = false;
	}

	private void on_radio_cp_toggled(object o, EventArgs args) {
		cp_zoom_buttons_unsensitive();
		
		if(radio_cp_other.Active)
			entry_cp_other.Sensitive = true;
		else {
			entry_cp_other.Sensitive = false;
			if(radio_cp1.Active) 
				button_zoom_cp1.Sensitive = true;
			else if(radio_cp2.Active) 
				button_zoom_cp2.Sensitive = true;
			else if(radio_cp3.Active) 
				button_zoom_cp3.Sensitive = true;
		}
		on_entries_required_changed(new object(), new EventArgs());
	}
	
	private void device_zoom_buttons_unsensitive() {
		button_zoom_contact_steel.Sensitive = false;
		button_zoom_contact_modular.Sensitive = false;
		button_zoom_infrared.Sensitive = false;
	}

	private void on_radio_device_toggled(object o, EventArgs args) {
		device_zoom_buttons_unsensitive();
		
		if(radio_device_other.Active)
			entry_device_other.Sensitive = true;
		else {
			entry_device_other.Sensitive = false;
			if(radio_contact_steel.Active) 
				button_zoom_contact_steel.Sensitive = true;
			else if(radio_contact_modular.Active) 
				button_zoom_contact_modular.Sensitive = true;
			else if(radio_infrared.Active) 
				button_zoom_infrared.Sensitive = true;
		}
		on_entries_required_changed(new object(), new EventArgs());
	}
	
	
	private void on_entries_required_changed(object o, EventArgs args) {
		if(
				entry_name.Text.Length > 0 &&
				entry_email.Text.Length > 0 &&
				label_date.Text.Length >0  && 
				label_date.Text != Catalog.GetString(Constants.UndefinedDefault) &&
				UtilGtk.ComboGetActive(combo_countries) != Catalog.GetString(Constants.CountryUndefined) &&
				! radio_cp_undef.Active &&
				! (radio_cp_other.Active && entry_cp_other.Text.Length == 0) &&
				! radio_device_undef.Active &&
				! (radio_device_other.Active && entry_device_other.Text.Length == 0)
		  )
			button_accept.Sensitive = true;
		else
			button_accept.Sensitive = false;

	}

	private void on_button_confiable_clicked(object o, EventArgs args) {
		new DialogMessage(Constants.MessageTypes.INFO, 
				"Currently we are creating confiable parameters.\n" + 
				"In nearly future maybe your data can be confiable");
	}
	
	private void on_button_cp1_zoom_clicked(object o, EventArgs args) {
		new DialogImageTest("Chronopic 1", Util.GetImagePath(false) + Constants.FileNameChronopic1, DialogImageTest.ArchiveType.FILE);
	}
	private void on_button_cp2_zoom_clicked(object o, EventArgs args) {
		new DialogImageTest("Chronopic 2", Util.GetImagePath(false) + Constants.FileNameChronopic2, DialogImageTest.ArchiveType.FILE);
	}
	private void on_button_cp3_zoom_clicked(object o, EventArgs args) {
		new DialogImageTest("Chronopic 3", Util.GetImagePath(false) + Constants.FileNameChronopic3, DialogImageTest.ArchiveType.FILE);
	}
	private void on_button_contact_steel_zoom_clicked(object o, EventArgs args) {
		new DialogImageTest("Contact platform (tempered steel)", Util.GetImagePath(false) + Constants.FileNameContactPlatformSteel, DialogImageTest.ArchiveType.FILE);
	}
	private void on_button_contact_modular_zoom_clicked(object o, EventArgs args) {
		new DialogImageTest("Contact platform (modular circuit board)", Util.GetImagePath(false) + Constants.FileNameContactPlatformModular, DialogImageTest.ArchiveType.FILE);
	}
	private void on_button_infrared_zoom_clicked(object o, EventArgs args) {
		new DialogImageTest("Infrared", Util.GetImagePath(false) + Constants.FileNameInfrared, DialogImageTest.ArchiveType.FILE);
	}

	void on_button_change_date_clicked (object o, EventArgs args)
	{
		System.DateTime dt = dateTime;
		if(dt == System.DateTime.MinValue)
			dt = System.DateTime.Now;
		myDialogCalendar = new DialogCalendar(Catalog.GetString("Select of Birth"), dt);
		myDialogCalendar.FakeButtonDateChanged.Clicked += new EventHandler(on_calendar_changed);
	}

	void on_calendar_changed (object obj, EventArgs args)
	{
		dateTime = myDialogCalendar.MyDateTime;
		label_date.Text = dateTime.ToLongDateString();
		on_entries_required_changed(new object(), new EventArgs());
	}

	private void fillDialog ()
	{
		entry_name.Text = eval.Name;
		entry_email.Text = eval.Email;

		Console.Write(creating.ToString());
		if(creating)
			label_date.Text = Catalog.GetString(Constants.UndefinedDefault);
		else {
			dateTime = eval.DateBorn;
			if(dateTime == System.DateTime.MinValue)
				label_date.Text = Catalog.GetString(Constants.UndefinedDefault);
			else
				label_date.Text = dateTime.ToLongDateString();
		}

		if(! creating) {		
			//country stuff
			if(eval.CountryID != Constants.CountryUndefinedID) {
				string [] countryString = SqliteCountry.Select(eval.CountryID);
				combo_continents.Active = UtilGtk.ComboMakeActive(continentsTranslated, 
						Catalog.GetString(countryString[3]));
				combo_countries.Active = UtilGtk.ComboMakeActive(countriesTranslated, 
						Catalog.GetString(countryString[1]));
			}

			label_confiable.Text = eval.Confiable.ToString();

			TextBuffer tb = new TextBuffer (new TextTagTable());
			tb.Text = eval.Comments;
			textview_comments.Buffer = tb;

			switch(eval.Chronometer) {
				case "": 
				case Constants.UndefinedDefault: 
					radio_cp_undef.Active = true;
					break;
				case Constants.ChronometerCp1: 
					radio_cp1.Active = true;
					break;
				case Constants.ChronometerCp2: 
					radio_cp2.Active = true;
					break;
				case Constants.ChronometerCp3: 
					radio_cp3.Active = true;
					break;
				default:
					radio_cp_other.Active = true;
					entry_cp_other.Text = eval.Chronometer;
					break;
			}

			switch(eval.Device) {
				case "": 
				case Constants.UndefinedDefault: 
					radio_device_undef.Active = true;
					break;
				case Constants.DeviceContactSteel: 
					radio_contact_steel.Active = true;
					break;
				case Constants.DeviceContactCircuit: 
					radio_contact_modular.Active = true;
					break;
				case Constants.DeviceInfrared: 
					radio_infrared.Active = true;
					break;
				default:
					radio_device_other.Active = true;
					entry_device_other.Text = eval.Device;
					break;
			}
		}

		//show or hide button_accept
		on_entries_required_changed(new object(), new EventArgs());
	}
		
	
	protected void on_button_cancel_clicked (object o, EventArgs args)
	{
		EvaluatorWindowBox.evaluator_window.Hide();
		EvaluatorWindowBox = null;
	}
	
	protected void on_delete_event (object o, DeleteEventArgs args)
	{
		EvaluatorWindowBox.evaluator_window.Hide();
		EvaluatorWindowBox = null;
	}

	protected void on_button_accept_clicked (object o, EventArgs args)
	{
		eval.Name = entry_name.Text.ToString();
		eval.Email = entry_email.Text.ToString();
		eval.DateBorn = dateTime;
		eval.CountryID = Convert.ToInt32(
				Util.FindOnArray(':', 2, 0, UtilGtk.ComboGetActive(combo_countries), countries));

		eval.Comments = textview_comments.Buffer.Text;

		if(radio_cp_undef.Active)
			eval.Chronometer = Constants.UndefinedDefault;
		else if(radio_cp1.Active)
			eval.Chronometer = Constants.ChronometerCp1;
		else if(radio_cp2.Active)
			eval.Chronometer = Constants.ChronometerCp2;
		else if(radio_cp3.Active)
			eval.Chronometer = Constants.ChronometerCp3;
		else
			eval.Chronometer = entry_cp_other.Text.ToString();


		if(radio_device_undef.Active)
			eval.Device = Constants.UndefinedDefault;
		else if(radio_contact_steel.Active)
			eval.Device = Constants.DeviceContactSteel;
		else if(radio_contact_modular.Active)
			eval.Device = Constants.DeviceContactCircuit;
		else if(radio_infrared.Active)
			eval.Device = Constants.DeviceInfrared;
		else
			eval.Device = entry_device_other.Text.ToString();


		changed = false;
		if(creating) {
			eval.InsertAtDB(false);
			changed = true;
		} else {
			//1st see if there are changes
			if(eval.Equals(evalBefore)) {
				//nothing changed
				//
				//new DialogMessage(Constants.MessageTypes.INFO, "nothing changed.\n"); 
			} else {
				//changed
				eval.Update(false);
				changed = true;
			}
		}

		fakeButtonAccept.Click();

		EvaluatorWindowBox.evaluator_window.Hide();
		EvaluatorWindowBox = null;
	}

	public Button FakeButtonAccept 
	{
		set { fakeButtonAccept = value; }
		get { return fakeButtonAccept; }
	}

	public bool Changed 
	{
		set { changed = value; }
		get { return changed; }
	}

	private void connectWidgets (Gtk.Builder builder)
	{
		evaluator_window = (Gtk.Window) builder.GetObject ("evaluator_window");
		entry_name = (Gtk.Entry) builder.GetObject ("entry_name");
		entry_email = (Gtk.Entry) builder.GetObject ("entry_email");
		label_date = (Gtk.Label) builder.GetObject ("label_date");
		hbox_combo_continents = (Gtk.Box) builder.GetObject ("hbox_combo_continents");
		hbox_combo_countries = (Gtk.Box) builder.GetObject ("hbox_combo_countries");
		label_confiable = (Gtk.Label) builder.GetObject ("label_confiable");
		textview_comments = (Gtk.TextView) builder.GetObject ("textview_comments");

		//chronometer tab
		radio_cp_undef = (Gtk.RadioButton) builder.GetObject ("radio_cp_undef");
		radio_cp1 = (Gtk.RadioButton) builder.GetObject ("radio_cp1");
		radio_cp2 = (Gtk.RadioButton) builder.GetObject ("radio_cp2");
		radio_cp3 = (Gtk.RadioButton) builder.GetObject ("radio_cp3");
		radio_cp_other = (Gtk.RadioButton) builder.GetObject ("radio_cp_other");
		image_cp1 = (Gtk.Image) builder.GetObject ("image_cp1");
		image_cp2 = (Gtk.Image) builder.GetObject ("image_cp2");
		image_cp3 = (Gtk.Image) builder.GetObject ("image_cp3");
		image_zoom_cp1 = (Gtk.Image) builder.GetObject ("image_zoom_cp1");
		image_zoom_cp2 = (Gtk.Image) builder.GetObject ("image_zoom_cp2");
		image_zoom_cp3 = (Gtk.Image) builder.GetObject ("image_zoom_cp3");
		entry_cp_other = (Gtk.Entry) builder.GetObject ("entry_cp_other");
		button_zoom_cp1 = (Gtk.Button) builder.GetObject ("button_zoom_cp1");
		button_zoom_cp2 = (Gtk.Button) builder.GetObject ("button_zoom_cp2");
		button_zoom_cp3 = (Gtk.Button) builder.GetObject ("button_zoom_cp3");

		//devices tab
		radio_device_undef = (Gtk.RadioButton) builder.GetObject ("radio_device_undef");
		radio_contact_steel = (Gtk.RadioButton) builder.GetObject ("radio_contact_steel");
		radio_contact_modular = (Gtk.RadioButton) builder.GetObject ("radio_contact_modular");
		radio_infrared = (Gtk.RadioButton) builder.GetObject ("radio_infrared");
		radio_device_other = (Gtk.RadioButton) builder.GetObject ("radio_device_other");
		entry_device_other = (Gtk.Entry) builder.GetObject ("entry_device_other");
		image_contact_steel = (Gtk.Image) builder.GetObject ("image_contact_steel");
		image_contact_modular = (Gtk.Image) builder.GetObject ("image_contact_modular");
		image_infrared = (Gtk.Image) builder.GetObject ("image_infrared");
		image_zoom_contact_steel = (Gtk.Image) builder.GetObject ("image_zoom_contact_steel");
		image_zoom_contact_modular = (Gtk.Image) builder.GetObject ("image_zoom_contact_modular");
		image_zoom_infrared = (Gtk.Image) builder.GetObject ("image_zoom_infrared");
		button_zoom_contact_steel = (Gtk.Button) builder.GetObject ("button_zoom_contact_steel");
		button_zoom_contact_modular = (Gtk.Button) builder.GetObject ("button_zoom_contact_modular");
		button_zoom_infrared = (Gtk.Button) builder.GetObject ("button_zoom_infrared");

		button_accept = (Gtk.Button) builder.GetObject ("button_accept");
	}

	~EvaluatorWindow() {}
	
}

