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
using Gdk;
using Glade;
using GLib; //for Value
using Mono.Unix;


public class QueryServerWindow
{
	[Widget] Gtk.Window query_server_window;

	[Widget] Gtk.Box hbox_combo_test_types;
	[Widget] Gtk.Box hbox_combo_tests;
	[Widget] Gtk.Box hbox_combo_variables;
	[Widget] Gtk.Box hbox_combo_sexes;
	[Widget] Gtk.Box hbox_combo_ages1;
	[Widget] Gtk.Box hbox_combo_ages2;
	[Widget] Gtk.Box hbox_combo_continents;
	[Widget] Gtk.Box hbox_combo_countries;
	[Widget] Gtk.Box hbox_combo_sports;
	[Widget] Gtk.Box hbox_combo_speciallities;
	[Widget] Gtk.Box hbox_combo_levels;
	[Widget] Gtk.Box hbox_combo_evaluators;

	[Widget] Gtk.ComboBox combo_test_types;
	[Widget] Gtk.ComboBox combo_tests;
	[Widget] Gtk.ComboBox combo_variables;
	[Widget] Gtk.ComboBox combo_sexes;
	[Widget] Gtk.ComboBox combo_ages1;
	[Widget] Gtk.ComboBox combo_ages2;
	[Widget] Gtk.ComboBox combo_continents;
	[Widget] Gtk.ComboBox combo_countries;
	[Widget] Gtk.ComboBox combo_sports;
	[Widget] Gtk.ComboBox combo_speciallities;
	[Widget] Gtk.ComboBox combo_levels;
	[Widget] Gtk.ComboBox combo_evaluators;

	[Widget] Gtk.TextView textview_query;
	
	[Widget] Gtk.Label label_age_and;
	[Widget] Gtk.SpinButton spin_ages1;
	[Widget] Gtk.SpinButton spin_ages2;
	[Widget] Gtk.Label label_speciallity;
	
	[Widget] Gtk.CheckButton check_show_query;
	[Widget] Gtk.ScrolledWindow scrolledwindow_query;

	[Widget] Gtk.Label label_results_num;
	[Widget] Gtk.Label label_results_avg;
	[Widget] Gtk.Label label_results_num_units;
	[Widget] Gtk.Label label_results_avg_units;
	
	[Widget] Gtk.Image image_test_type;
	[Widget] Gtk.Image image_country;
	
	[Widget] Gtk.Button button_search;

	string [] testTypes = {
		Catalog.GetString(Constants.UndefinedDefault), //needs to be defined
		Catalog.GetString(Constants.JumpSimpleName),
		Catalog.GetString(Constants.JumpReactiveName),
		Catalog.GetString(Constants.RunSimpleName),
		//Catalog.GetString(Constants.RunIntervallicName),
		//Catalog.GetString(Constants.ReactionTimeName), //has no types or is not relevant
		//Catalog.GetString(Constants.PulseName),
		//Catalog.GetString(Constants.MultiChronopicName), //has no types or is not relevant
	};
	
	string [] tests = {
		Catalog.GetString(Constants.UndefinedDefault), //needs to be defined
	};
	
	string [] variables = {
		Catalog.GetString(Constants.UndefinedDefault), //needs to be defined
	};
	
	//this variables have to be without Catalog, because in server can have different locale
	private static string [] variablesJumpSimple = {
		"TV", 
	};
	private static string [] variablesJumpSimpleWithTC = {
		"TV", 
		"TC", 
		Constants.DjIndexFormula,
		Constants.QIndexFormula
	};
	private static string [] variablesJumpReactive = {
		Catalog.GetString("Average Index"), 
		Constants.RJPotencyBoscoFormula,
		//Catalog.GetString("Evolution"), is same as RjIndex but showing every tc, tf
		//Constants.RJAVGSDRjIndexName,
		//Constants.RJAVGSDQIndexName
	};
	private static string [] variablesRunSimple = {
		Catalog.GetString("Time"), 
	};
	

	string [] sexes = {
		Catalog.GetString(Constants.Any), 
		Catalog.GetString(Constants.Males), 
		Catalog.GetString(Constants.Females), 
	};


	//static string equalThan = Constants.EqualThanCode + " " + Catalog.GetString("Equal than");
	static string lowerThan = Constants.LowerThanCode + " " + Catalog.GetString("Lower than");
	//static string higherThan = Constants.HigherThanCode + " " + Catalog.GetString("Higher than");
	//static string lowerOrEqualThan = Constants.LowerOrEqualThanCode + " " + Catalog.GetString("Lower or equal than");
	static string higherOrEqualThan = Constants.HigherOrEqualThanCode + " " + Catalog.GetString("Higher or equal than");
	string [] ages1 = {
		Catalog.GetString(Constants.Any), 
		lowerThan,
		higherOrEqualThan
	};
	string [] ages2Lower = {
		Catalog.GetString(Constants.Any), 
		lowerThan
	};
	string [] ages2Higher = {
		Catalog.GetString(Constants.Any), 
		higherOrEqualThan
	};


	Sport sport;
	string [] sports;
	string [] sportsTranslated;
	string [] speciallities;
	string [] speciallitiesTranslated;
	string [] levels;
	string [] continents;
	string [] continentsTranslated;
	string [] countries;
	string [] countriesTranslated;
	string [] evaluators;
	
	int pDN; //prefsDigitsNumber;
	
	static QueryServerWindow QueryServerWindowBox;
	
	public QueryServerWindow (int newPrefsDigitsNumber, string [] evaluators)
	{
		Glade.XML gladeXML;
		gladeXML = Glade.XML.FromAssembly (Util.GetGladePath() + "query_server_window.glade", "query_server_window", "chronojump");
		gladeXML.Autoconnect(this);
		
		this.pDN = newPrefsDigitsNumber;
		this.evaluators = evaluators;
		
		//put an icon to window
		UtilGtk.IconWindow(query_server_window);
		
		createAllCombos();
	}

	static public QueryServerWindow Show (int newPrefsDigitsNumber, string [] evaluators)
	{
		if (QueryServerWindowBox == null) {
			QueryServerWindowBox = new QueryServerWindow(newPrefsDigitsNumber, evaluators);
		}
		QueryServerWindowBox.query_server_window.Show ();
		
		QueryServerWindowBox.fillDialog ();
		
		return QueryServerWindowBox;
	}
		
	/*
	   combos create
	 */	

	private void createAllCombos() {
		button_search.Sensitive = false;

		createComboTestTypes();
		createComboTests();
		createComboVariables();
		createComboContinents();
		createComboCountries();
		createComboSexes();
		createComboAges1();
		createComboAges2();
		createComboSports();
		createComboSpeciallities(-1);
		createComboLevels();
		createComboEvaluators();
	}
	
	private void createComboTestTypes() {
		combo_test_types = ComboBox.NewText ();
		UtilGtk.ComboUpdate(combo_test_types, testTypes, "");
		combo_test_types.Active = UtilGtk.ComboMakeActive(testTypes, Catalog.GetString(Constants.UndefinedDefault));
		combo_test_types.Changed += new EventHandler (on_combo_test_types_changed);
		UtilGtk.ComboPackShowAndSensitive(hbox_combo_test_types, combo_test_types);
	}

	private void createComboTests() {
		combo_tests = ComboBox.NewText ();
		tests = Util.StringToStringArray(Constants.UndefinedDefault);
		UtilGtk.ComboUpdate(combo_tests, Util.StringToStringArray(Constants.UndefinedDefault), "");
		combo_tests.Active = UtilGtk.ComboMakeActive(tests, Catalog.GetString(Constants.UndefinedDefault));
		combo_tests.Changed += new EventHandler (on_combo_tests_changed);
		UtilGtk.ComboPackShowAndSensitive(hbox_combo_tests, combo_tests);
	}

	private void createComboVariables() {
		combo_variables = ComboBox.NewText ();
		variables = Util.StringToStringArray(Constants.UndefinedDefault);
		UtilGtk.ComboUpdate(combo_variables, variables, "");
		combo_variables.Active = UtilGtk.ComboMakeActive(variables, Catalog.GetString(Constants.UndefinedDefault));
		combo_variables.Changed += new EventHandler (on_combo_other_changed);
		UtilGtk.ComboPackShowAndSensitive(hbox_combo_variables, combo_variables);
	}

	private void createComboContinents() {
		combo_continents = ComboBox.NewText ();
		continents = Constants.Continents;
		//first value has to be any
		continents[0] = Constants.Any + ":" + Catalog.GetString(Constants.Any); 

		//create continentsTranslated, only with translated stuff
		continentsTranslated = new String[Constants.Continents.Length];
		int i = 0;
		foreach(string continent in continents) 
			continentsTranslated[i++] = Util.FetchName(continent);

		UtilGtk.ComboUpdate(combo_continents, continentsTranslated, "");
		combo_continents.Active = UtilGtk.ComboMakeActive(continentsTranslated, 
				Catalog.GetString(Constants.Any));

		combo_continents.Changed += new EventHandler (on_combo_continents_changed);
		UtilGtk.ComboPackShowAndSensitive(hbox_combo_continents, combo_continents);
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

		combo_countries.Changed += new EventHandler (on_combo_other_changed);
		UtilGtk.ComboPackShowAndSensitive(hbox_combo_countries, combo_countries);
	}
	
	private void createComboSexes() {
		combo_sexes = ComboBox.NewText ();
		UtilGtk.ComboUpdate(combo_sexes, sexes, "");
		combo_sexes.Active = UtilGtk.ComboMakeActive(sexes, Catalog.GetString(Constants.Any));
		combo_sexes.Changed += new EventHandler (on_combo_other_changed);
		UtilGtk.ComboPackShowAndSensitive(hbox_combo_sexes, combo_sexes);
	}

	private void createComboAges1() {
		combo_ages1 = ComboBox.NewText ();
		UtilGtk.ComboUpdate(combo_ages1, ages1, "");
		combo_ages1.Active = UtilGtk.ComboMakeActive(ages1, Catalog.GetString(Constants.Any));
		combo_ages1.Changed += new EventHandler (on_combo_ages1_changed);
		UtilGtk.ComboPackShowAndSensitive(hbox_combo_ages1, combo_ages1);
		spin_ages1.Sensitive = false;
		label_age_and.Sensitive = false;
	}

	private void createComboAges2() {
		combo_ages2 = ComboBox.NewText ();
		string [] ages2 = Util.StringToStringArray(Constants.Any);
		UtilGtk.ComboUpdate(combo_ages2, ages2, "");
		combo_ages2.Active = UtilGtk.ComboMakeActive(ages2, Catalog.GetString(Constants.Any));
		combo_ages2.Changed += new EventHandler (on_combo_ages2_changed);
		UtilGtk.ComboPackShowAndSensitive(hbox_combo_ages2, combo_ages2);
		combo_ages2.Sensitive = false;
		spin_ages2.Sensitive = false;
	}

	private void createComboSports() {
		combo_sports = ComboBox.NewText ();
		sports = SqliteSport.SelectAll();
		
		//first value has to be any
		sports[0] = Constants.SportUndefinedID + ":" + //no problem using the undefinedID
			Constants.SportAny + ":" + Catalog.GetString(Constants.SportAny); //is "--Any" to be the first in sort
			
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
				Catalog.GetString(Constants.SportAny));
		
		combo_sports.Changed += new EventHandler (on_combo_sports_changed);
	
		UtilGtk.ComboPackShowAndSensitive(hbox_combo_sports, combo_sports);
	}
	
	private void createComboSpeciallities(int sportID) {
		combo_speciallities = ComboBox.NewText ();
		speciallities = SqliteSpeciallity.SelectAll(true, sportID); //show undefined, filter by sport
		
		//first value has to be any
		speciallities[0] = "-1:" + Constants.Any + ":" + Catalog.GetString(Constants.Any);
		
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
				Catalog.GetString(Constants.Any));

		hbox_combo_speciallities.PackStart(combo_speciallities, true, true, 0);
		hbox_combo_speciallities.ShowAll();
		combo_speciallities.Sensitive = true;
		combo_speciallities.Changed += new EventHandler (on_combo_other_changed);
		UtilGtk.ComboPackShowAndSensitive(hbox_combo_speciallities, combo_speciallities);
	}
	
	private void createComboLevels() {
		combo_levels = ComboBox.NewText ();
		levels = Constants.Levels;
		
		//first value has to be any (but is ok to put the id value of the LevelUndefinedID)
		levels[0] = Constants.LevelUndefinedID.ToString() + ":" + Catalog.GetString(Constants.Any);
		
		UtilGtk.ComboUpdate(combo_levels, levels, "");
		combo_levels.Active = UtilGtk.ComboMakeActive(levels, 
				Constants.LevelUndefinedID.ToString() + ":" + 
				Catalog.GetString(Constants.Any));

		hbox_combo_levels.PackStart(combo_levels, true, true, 0);
		hbox_combo_levels.ShowAll();
		combo_levels.Sensitive = false; //level is shown when sport is not "undefined" and not "none"
		combo_levels.Changed += new EventHandler (on_combo_other_changed);
		UtilGtk.ComboPackShowAndSensitive(hbox_combo_levels, combo_levels);
	}

	private void createComboEvaluators() {
		combo_evaluators = ComboBox.NewText ();

		//first value (any) should be translated
		evaluators[0]=Constants.AnyID.ToString() + ":" + Catalog.GetString(Constants.Any);

		UtilGtk.ComboUpdate(combo_evaluators, evaluators, "");
		combo_evaluators.Active = UtilGtk.ComboMakeActive(combo_evaluators, Catalog.GetString(Constants.Any));
		combo_evaluators.Changed += new EventHandler (on_combo_other_changed);
		UtilGtk.ComboPackShowAndSensitive(hbox_combo_evaluators, combo_evaluators);
	}


	/*
	   combos changed signals
	 */	

	private void on_combo_test_types_changed(object o, EventArgs args) 
	{
		if (UtilGtk.ComboGetActive(combo_test_types) == Catalog.GetString(Constants.UndefinedDefault)) {
			tests = new String[1];
			tests [0] = Catalog.GetString(Constants.UndefinedDefault);
			UtilGtk.ComboUpdate(combo_tests, tests, "");
			UtilGtk.ComboUpdate(combo_variables, tests, "");
		}
		else if (UtilGtk.ComboGetActive(combo_test_types) == Catalog.GetString(Constants.JumpSimpleName))
			UtilGtk.ComboUpdate(combo_tests, 
					SqliteJumpType.SelectJumpTypes(false, "", "", true), "");
		else if (UtilGtk.ComboGetActive(combo_test_types) == Catalog.GetString(Constants.JumpReactiveName))
			UtilGtk.ComboUpdate(combo_tests, 
					SqliteJumpType.SelectJumpRjTypes("", true), "");
		else if (UtilGtk.ComboGetActive(combo_test_types) == Catalog.GetString(Constants.RunSimpleName))
			UtilGtk.ComboUpdate(combo_tests, 
					SqliteRunType.SelectRunTypes("", true), "");

		combo_tests.Active = 0;

		on_entries_required_changed(new object(), new EventArgs());
	}
	
	
	
	private void on_combo_tests_changed(object o, EventArgs args) 
	{
		if (UtilGtk.ComboGetActive(combo_test_types) == Catalog.GetString(Constants.UndefinedDefault)) 
			UtilGtk.ComboUpdate(combo_variables, Util.StringToStringArray(Constants.UndefinedDefault), "");
		else if (UtilGtk.ComboGetActive(combo_test_types) == Catalog.GetString(Constants.JumpSimpleName)) {
			JumpType jt = SqliteJumpType.SelectAndReturnJumpType(UtilGtk.ComboGetActive(combo_tests), false);
			if(jt.StartIn)
				UtilGtk.ComboUpdate(combo_variables, variablesJumpSimple, "");
			else
				UtilGtk.ComboUpdate(combo_variables, variablesJumpSimpleWithTC, "");
		}
		else if (UtilGtk.ComboGetActive(combo_test_types) == Catalog.GetString(Constants.JumpReactiveName)) 
			UtilGtk.ComboUpdate(combo_variables, variablesJumpReactive, "");
		else if (UtilGtk.ComboGetActive(combo_test_types) == Catalog.GetString(Constants.RunSimpleName)) 
			UtilGtk.ComboUpdate(combo_variables, variablesRunSimple, "");
		else
			new DialogMessage(Constants.MessageTypes.WARNING, "Problem on tests");
		
		combo_variables.Active = 0;

		on_entries_required_changed(new object(), new EventArgs());
	}

	
	private void on_combo_continents_changed(object o, EventArgs args) {
		if(UtilGtk.ComboGetActive(combo_continents) == Catalog.GetString(Constants.Any)) {
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
	
	private void on_combo_ages1_changed(object o, EventArgs args) 
	{
		string age1 = UtilGtk.ComboGetActive(combo_ages1);
		string [] ages2;

		//if (age1 == Catalog.GetString(Constants.Any) ||	age1 == equalThan) {
		if (age1 == Catalog.GetString(Constants.Any)) {
			if (age1 == Catalog.GetString(Constants.Any))  //zero values
				spin_ages1.Sensitive = false;
			else
				spin_ages1.Sensitive = true;
			
			//no value 2
			label_age_and.Sensitive = false;
			combo_ages2.Sensitive = false;
			spin_ages2.Sensitive = false;

			ages2 = Util.StringToStringArray(Constants.Any);
		} else {
			spin_ages1.Sensitive = true;
			label_age_and.Sensitive = true;
			combo_ages2.Sensitive = true;
			spin_ages2.Sensitive = true;
			//if (age1 == lowerThan || age1 == lowerOrEqualThan)
			if (age1 == lowerThan)
				ages2 = ages2Higher;
			else
				ages2 = ages2Lower;
		}
	
		UtilGtk.ComboUpdate(combo_ages2, ages2, "");
		combo_ages2.Active = UtilGtk.ComboMakeActive(ages2, Catalog.GetString(Constants.Any));
	
		on_entries_required_changed(new object(), new EventArgs());
	}

	private void on_combo_ages2_changed(object o, EventArgs args) 
	{
		string age2 = UtilGtk.ComboGetActive(combo_ages2);

		if (age2 == Catalog.GetString(Constants.Any)) 
			spin_ages2.Sensitive = false;
		else 
			spin_ages2.Sensitive = true;
	
		on_entries_required_changed(new object(), new EventArgs());
	}
	
	private void on_spin_ages1_changed(object o, EventArgs args) {
		if(spin_ages1.Value > 0)
			on_entries_required_changed(new object(), new EventArgs());
	}
	private void on_spin_ages2_changed(object o, EventArgs args) {
		if(spin_ages2.Value > 0)
			on_entries_required_changed(new object(), new EventArgs());
	}



	private void on_combo_sports_changed(object o, EventArgs args) {
		if (o == null)
			return;

		//LogB.Information("changed");
		try {
			int sportID = Convert.ToInt32(Util.FindOnArray(':', 2, 0, UtilGtk.ComboGetActive(combo_sports), sports));
			sport = SqliteSport.Select(false, sportID);

			if(Catalog.GetString(sport.Name) == Catalog.GetString(Constants.SportAny)) {
				//if sport is undefined, level should be undefined, and unsensitive
				try { 
					combo_levels.Active = UtilGtk.ComboMakeActive(levels, 
							Constants.LevelUndefinedID.ToString() + ":" + 
							Catalog.GetString(Constants.LevelUndefined));
					combo_levels.Sensitive = false;
					combo_speciallities.Active = UtilGtk.ComboMakeActive(speciallitiesTranslated, 
							Catalog.GetString(Constants.SpeciallityUndefined));
					label_speciallity.Hide();
					combo_speciallities.Hide();
				}
				catch { LogB.Warning("do later"); }
			} else if(Catalog.GetString(sport.Name) == Catalog.GetString(Constants.SportNone)) {
				//if sport is none, level should be sedentary and unsensitive
				try { 
					combo_levels.Active = UtilGtk.ComboMakeActive(levels, 
							Constants.LevelSedentaryID.ToString() + ":" + 
							Catalog.GetString(Constants.LevelSedentary));
					combo_levels.Sensitive = false;

					combo_speciallities.Active = UtilGtk.ComboMakeActive(speciallitiesTranslated, 
							Catalog.GetString(Constants.SpeciallityUndefined));

					label_speciallity.Hide();
					combo_speciallities.Hide();
				}
				catch { LogB.Warning("do later"); }
			} else {
				//sport is not undefined and not none

				//if level is "sedentary", then change level to "undefined"
				if(UtilGtk.ComboGetActive(combo_levels) ==
						Constants.LevelSedentaryID.ToString() + ":" + 
					       	Catalog.GetString(Constants.LevelSedentary)) {
					combo_levels.Active = UtilGtk.ComboMakeActive(levels,
							Constants.LevelUndefinedID.ToString() + ":" + 
						       	Catalog.GetString(Constants.LevelUndefined));
				}

				//show level
				combo_levels.Sensitive = true;
		
				if(sport.HasSpeciallities) {
					combo_speciallities.Destroy();
					createComboSpeciallities(sport.UniqueID);
					label_speciallity.Show();
					combo_speciallities.Show();
				} else {
					LogB.Information("hide");
					combo_speciallities.Active = UtilGtk.ComboMakeActive(speciallitiesTranslated,
						       	Catalog.GetString(Constants.SpeciallityUndefined));
					label_speciallity.Hide();
					combo_speciallities.Hide();
				}
			}
		} catch { 
			//LogB.Warning("do later");
		}

		on_entries_required_changed(new object(), new EventArgs());
		LogB.Information(sport.ToString());
	}
	
	private void on_combo_other_changed(object o, EventArgs args) {
		on_entries_required_changed(new object(), new EventArgs());
	}


	void on_entries_required_changed (object o, EventArgs args)
	{
		bool allOk = true;
		
		if ( UtilGtk.ComboGetActive(combo_test_types) != Catalog.GetString(Constants.UndefinedDefault) ) {
			image_test_type.Hide();
			combo_tests.Sensitive = true;
			combo_variables.Sensitive = true;
		} else {
			image_test_type.Show();
			combo_tests.Sensitive = false;
			combo_variables.Sensitive = false;
			allOk = false;
		}

		//a continent cannot be searched without selecting the country
		if ( UtilGtk.ComboGetActive(combo_continents) == Catalog.GetString(Constants.Any) ) {
			combo_countries.Sensitive = false;
			image_country.Hide();
		}
		else { 
			combo_countries.Sensitive = true;
			if ( UtilGtk.ComboGetActive(combo_countries) == Catalog.GetString(Constants.UndefinedDefault) ) {
				image_country.Show();
				allOk = false;
			} else
				image_country.Hide();
		}
		
	
		if(allOk) {
			textViewUpdate(sqlBuildSelect(false));
			button_search.Sensitive = true;
		}
		else {
			textViewUpdate("");
			button_search.Sensitive = false;
		}
	}

	void textViewUpdate (string str) 
	{
		TextBuffer tb = new TextBuffer (new TextTagTable());
		tb.Text = str;
		textview_query.Buffer = tb;
		return;
	}

	string sqlBuildSelect (bool performQuery) 
	{
		string testType = UtilGtk.ComboGetActive(combo_test_types);
		string tableName = "";
		if (testType == Catalog.GetString(Constants.JumpSimpleName))
			tableName = Constants.JumpTable;
		else if (testType == Catalog.GetString(Constants.JumpReactiveName))
			tableName = Constants.JumpRjTable;
		else if (testType == Catalog.GetString(Constants.RunSimpleName))
			tableName = Constants.RunTable;
		else {
			new DialogMessage(Constants.MessageTypes.WARNING, "Problem on sqlBuildSelect");
			return "";
		}
		
		string strVariable = UtilGtk.ComboGetActive(combo_variables);
		if(strVariable == Constants.DjIndexFormula)
			strVariable = Constants.DjIndexFormulaOnly;
		else if(strVariable == Constants.QIndexFormula)
			strVariable = Constants.QIndexFormulaOnly;
		else if(strVariable == Catalog.GetString("Average Index"))
			strVariable = Constants.RjIndexFormulaOnly;
		else if(strVariable == Constants.RJPotencyBoscoFormula)
			strVariable = Constants.RJPotencyBoscoFormulaOnly;
		
		/*
		   as in server maybe Catalog locale is different than in client
		   we cannot pass a localized "Any" hoping that will match server.
		   then if it's any, pass "" (if string) or corresponding undefined ID (if int)
		   */

		int sexID = Constants.AnyID;
		if(UtilGtk.ComboGetActive(combo_sexes) == Catalog.GetString(Constants.Males))
			sexID = Constants.MaleID;
		else if(UtilGtk.ComboGetActive(combo_sexes) == Catalog.GetString(Constants.Females))
			sexID = Constants.FemaleID;

		/*
		   ageInterval can be:
		   "" -> any ages
		   ">=|30" -> higher or equal than 30
		   ">=|30|< |40" -> higher or equal than 30 and lower than 40
		   */
		
		string ageInterval = ""; 
		string age1 = UtilGtk.ComboGetActive(combo_ages1);
		if(age1 != "" && age1 != Catalog.GetString(Constants.Any)) {
			ageInterval = age1.Substring(0,2); //get the code
			ageInterval += ":" + spin_ages1.Value.ToString();

			string age2 = UtilGtk.ComboGetActive(combo_ages2);
			if(age2 != "" && age2 != Catalog.GetString(Constants.Any)) {
				ageInterval += ":" + age2.Substring(0,2); //get the code
				ageInterval += ":" + spin_ages2.Value.ToString();
			}
		}
		
		try {
			string sqlString = Sqlite.SQLBuildQueryString(
					tableName, 
					UtilGtk.ComboGetActive(combo_tests),
					strVariable,
					sexID,
					ageInterval,
					Convert.ToInt32(Util.FindOnArray(':', 2, 0, UtilGtk.ComboGetActive(combo_countries), countries)),
					Convert.ToInt32(Util.FindOnArray(':', 2, 0, UtilGtk.ComboGetActive(combo_sports), sports)),
					Convert.ToInt32(Util.FindOnArray(':', 2, 0, UtilGtk.ComboGetActive(combo_speciallities), speciallities)),
					Util.FetchID(UtilGtk.ComboGetActive(combo_levels)),
					Util.FetchID(UtilGtk.ComboGetActive(combo_evaluators))
					);

			if(performQuery) {
				ChronojumpServer myServer = new ChronojumpServer();
				myServer.ConnectDatabase();
				string result = myServer.Query(
					tableName, 
					UtilGtk.ComboGetActive(combo_tests),
					strVariable,
					sexID,
					ageInterval,
					Convert.ToInt32(Util.FindOnArray(':', 2, 0, UtilGtk.ComboGetActive(combo_countries), countries)),
					Convert.ToInt32(Util.FindOnArray(':', 2, 0, UtilGtk.ComboGetActive(combo_sports), sports)),
					Convert.ToInt32(Util.FindOnArray(':', 2, 0, UtilGtk.ComboGetActive(combo_speciallities), speciallities)),
					Util.FetchID(UtilGtk.ComboGetActive(combo_levels)),
					Util.FetchID(UtilGtk.ComboGetActive(combo_evaluators))
					);
				myServer.DisConnectDatabase();

				string [] resultFull = result.Split(new char[] {':'});
				label_results_num.Text = resultFull[0];

				printUnits(resultFull[0]);
		
				if(resultFull[0] == "0") 
					label_results_avg.Text = "-";
				else 
					label_results_avg.Text = Util.TrimDecimals(
						Util.ChangeDecimalSeparator(resultFull[1]), pDN);
			}

			return sqlString;
		} catch {
			//fix problem on changing continent that updates country and two signals come
			//also on run (maybe because there's no data)
			label_results_num.Text = "0";
			label_results_avg.Text = "-";
			printUnits("0");
			return "";
		}
	}
				
	private void printUnits(string result) {
		string testType = UtilGtk.ComboGetActive(combo_test_types);
		if(testType == Catalog.GetString(Constants.JumpSimpleName) ||
				testType == Catalog.GetString(Constants.JumpReactiveName))
			label_results_num_units.Text = Catalog.GetString("jumps");
		else if (testType == Catalog.GetString(Constants.RunSimpleName))
			label_results_num_units.Text = Catalog.GetString("races");

		string strVariable = UtilGtk.ComboGetActive(combo_variables);
		if(result == "0")
			label_results_avg_units.Text = "";
		else if(strVariable == "TV" || strVariable == "TC" || strVariable == Catalog.GetString("Time")) 
			label_results_avg_units.Text = Catalog.GetString("seconds");
		else if(strVariable == Constants.RJPotencyBoscoFormula)
			label_results_avg_units.Text = Catalog.GetString("watts");
		else if(strVariable == Constants.QIndexFormula)
			label_results_avg_units.Text = "";
		else
			label_results_avg_units.Text = "%";
	}

	private void fillDialog ()
	{
		combo_tests.Sensitive = false;
		combo_variables.Sensitive = false;
		label_speciallity.Hide();
		combo_speciallities.Hide();
		
		on_entries_required_changed(new object(), new EventArgs());
	}

	private void on_check_show_query_toggled(object o, EventArgs args)
	{
		scrolledwindow_query.Visible = check_show_query.Active;
	}


	protected void on_button_search_clicked (object o, EventArgs args)
	{
		sqlBuildSelect(true);
	}
	
	protected void on_button_close_clicked (object o, EventArgs args)
	{
		QueryServerWindowBox.query_server_window.Hide();
		QueryServerWindowBox = null;
	}
	
	protected void on_delete_event (object o, DeleteEventArgs args)
	{
		QueryServerWindowBox.query_server_window.Hide();
		QueryServerWindowBox = null;
	}

	~QueryServerWindow() {}
	
}

