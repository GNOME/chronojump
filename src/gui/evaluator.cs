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
 * Xavier de Blas: 
 */

using System;
using Gtk;
using Gdk;
using Glade;
//using Gnome;
using GLib; //for Value
using Mono.Unix;


public class EvaluatorWindow
{
	[Widget] Gtk.Window evaluator_window;
	[Widget] Gtk.Entry entry_name;
	[Widget] Gtk.Entry entry_email;
	[Widget] Gtk.Label label_date;
	[Widget] Gtk.Box hbox_combo_continents;
	[Widget] Gtk.Box hbox_combo_countries;
	[Widget] Gtk.ComboBox combo_continents;
	[Widget] Gtk.ComboBox combo_countries;
	[Widget] Gtk.Label label_confiable;
	[Widget] Gtk.TextView textview_comments;

	[Widget] Gtk.RadioButton radio_cp_undef;
	[Widget] Gtk.RadioButton radio_cp1;
	[Widget] Gtk.RadioButton radio_cp2;
	[Widget] Gtk.RadioButton radio_cp3;
	[Widget] Gtk.RadioButton radio_cp_other;

	[Widget] Gtk.Image image_cp1;
	[Widget] Gtk.Image image_cp2;
	[Widget] Gtk.Image image_cp3;
	[Widget] Gtk.Image image_zoom_cp1;
	[Widget] Gtk.Image image_zoom_cp2;
	[Widget] Gtk.Image image_zoom_cp3;
	[Widget] Gtk.Entry entry_cp_other;
	[Widget] Gtk.Button button_zoom_cp1;
	[Widget] Gtk.Button button_zoom_cp2;
	[Widget] Gtk.Button button_zoom_cp3;

	[Widget] Gtk.Button button_accept;
	[Widget] Gtk.Button button_cancel;
	
	string [] continents;
	string [] continentsTranslated;
	string [] countries;
	string [] countriesTranslated;
	string [] chronometers;
	string [] devices;
	string [] devicesTranslated;
	
	DialogCalendar myDialogCalendar;
	DateTime dateTime;

	ServerEvaluator eval;
	static EvaluatorWindow EvaluatorWindowBox;
	
	public EvaluatorWindow (ServerEvaluator eval)
	{
		//Setup (text, table, uniqueID);
		Glade.XML gladeXML;
		gladeXML = Glade.XML.FromAssembly (Util.GetGladePath() + "chronojump.glade", "evaluator_window", null);
		gladeXML.Autoconnect(this);
		
		//put an icon to window
		UtilGtk.IconWindow(evaluator_window);
		
		this.eval = eval;
		
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
					
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + Constants.FileNameZoomInIcon);
		image_zoom_cp1.Pixbuf = pixbuf;
		image_zoom_cp2.Pixbuf = pixbuf;
		image_zoom_cp3.Pixbuf = pixbuf;
		
		cp_zoom_buttons_unsensitive();
	}
	
	private void createComboContinents() {
		combo_continents = ComboBox.NewText ();
		continents = Constants.Continents;

		//create continentsTranslated, only with translated stuff
		continentsTranslated = new String[Constants.Continents.Length];
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
		combo_countries = ComboBox.NewText ();

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
		//Console.WriteLine("Changed");
		
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
		//define country is not needed to accept person
		//on_entries_required_changed(new object(), new EventArgs());
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
	}
	
	
	private void on_entries_required_changed(object o, EventArgs args) {
	}
	private void on_button_confiable_clicked(object o, EventArgs args) {
		Console.WriteLine("Confiable info");
	}
	
	private void on_button_cp1_zoom_clicked(object o, EventArgs args) {
		new DialogImageTest("Chronopic 1", Util.GetImagePath(false) + Constants.FileNameChronopic1);
	}
	private void on_button_cp2_zoom_clicked(object o, EventArgs args) {
		new DialogImageTest("Chronopic 2", Util.GetImagePath(false) + Constants.FileNameChronopic2);
	}
	private void on_button_cp3_zoom_clicked(object o, EventArgs args) {
		new DialogImageTest("Chronopic 3", Util.GetImagePath(false) + Constants.FileNameChronopic3);
	}

	void on_button_change_date_clicked (object o, EventArgs args)
	{
		myDialogCalendar = new DialogCalendar(Catalog.GetString("Select session date"));
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
		int myChronometerID;
//		if(adding) {
			//dateTime = DateTime.Today;
			//now dateTime is undefined until user changes it
			dateTime = DateTime.MinValue;
			label_date.Text = Catalog.GetString("Undefined");
			label_confiable.Text = "Not confiable (default, nothing checked)";

//			myChronometerID = currentEvaluator.ChronometerID;
//		} else {
			/*
			Person myPerson = SqlitePersonSession.PersonSelect(personID, currentSession.UniqueID); 
		
			entry1.Text = myPerson.Name;
			if (myPerson.Sex == "M") {
				radiobutton_man.Active = true;
			} else {
				radiobutton_woman.Active = true;
			}

			dateTime = Util.DateAsDateTime(myPerson.DateBorn);
			if(dateTime == DateTime.MinValue)
				label_date.Text = Catalog.GetString("Undefined");
			else
				label_date.Text = dateTime.ToLongDateString();

			spinbutton_height.Value = myPerson.Height;
			spinbutton_weight.Value = myPerson.Weight;
			weightIni = myPerson.Weight; //store for tracking if changes
		
			mySportID = myPerson.SportID;
			mySpeciallityID = myPerson.SpeciallityID;
			myLevelID = myPerson.Practice;


			TextBuffer tb = new TextBuffer (new TextTagTable());
			tb.Text = myPerson.Description;
			textview2.Buffer = tb;

			//country stuff
			if(myPerson.CountryID != Constants.CountryUndefinedID) {
				string [] countryString = SqliteCountry.Select(myPerson.CountryID);
				combo_continents.Active = UtilGtk.ComboMakeActive(continentsTranslated, 
						Catalog.GetString(countryString[3]));
				combo_countries.Active = UtilGtk.ComboMakeActive(countriesTranslated, 
						Catalog.GetString(countryString[1]));
			}

			serverUniqueID = myPerson.ServerUniqueID;
			*/


//	}
//
			
	//	sport = SqliteSport.Select(mySportID);
	//	combo_sports.Active = UtilGtk.ComboMakeActive(sportsTranslated, sport.ToString());

		
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
		EvaluatorWindowBox.evaluator_window.Hide();
		EvaluatorWindowBox = null;
	}
	
	public Button Button_accept 
	{
		set { button_accept = value; }
		get { return button_accept; }
	}

	~EvaluatorWindow() {}
	
}

