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


//--------------------------------------------------------
//---------------- RUN TYPE ADD WIDGET -------------------
//--------------------------------------------------------

public class RunTypeAddWindow 
{
	[Widget] Gtk.Window run_type_add;
	[Widget] Gtk.Button button_accept;
	public Gtk.Button fakeButtonAccept;
	[Widget] Gtk.Entry entry_name;
	
	[Widget] Gtk.Label label_header;
	[Widget] Gtk.Label label_main_options;
	[Widget] Gtk.Table table_main_options;

	[Widget] Gtk.RadioButton radiobutton_simple;
	[Widget] Gtk.RadioButton radiobutton_interval;
	[Widget] Gtk.RadioButton radiobutton_unlimited;
	[Widget] Gtk.VBox vbox_limited;
	[Widget] Gtk.HBox hbox_fixed;
	[Widget] Gtk.RadioButton radiobutton_limited_tracks;
	[Widget] Gtk.CheckButton checkbutton_limited_fixed;
	[Widget] Gtk.SpinButton spin_fixed_tracks_or_time;

	[Widget] Gtk.Label label_distance;
	[Widget] Gtk.RadioButton radiobutton_dist_variable;
	[Widget] Gtk.RadioButton radiobutton_dist_fixed;
	[Widget] Gtk.RadioButton radiobutton_dist_different;

	[Widget] Gtk.Alignment alignment_hbox_distance_fixed;
	[Widget] Gtk.SpinButton spin_distance_fixed;

	[Widget] Gtk.Alignment alignment_vbox_distance_variable;
	[Widget] Gtk.ComboBox combo_distance_different_tracks;
	[Widget] Gtk.HBox hbox_distance_variable;
	[Widget] Gtk.Label label_decimal_separator;


	[Widget] Gtk.TextView textview_description;
	
	//10 entries for distance different test definition
	[Widget] Gtk.Entry dd0;
	[Widget] Gtk.Entry dd1;
	[Widget] Gtk.Entry dd2;
	[Widget] Gtk.Entry dd3;
	[Widget] Gtk.Entry dd4;
	[Widget] Gtk.Entry dd5;
	[Widget] Gtk.Entry dd6;
	[Widget] Gtk.Entry dd7;
	[Widget] Gtk.Entry dd8;
	[Widget] Gtk.Entry dd9;

	static RunTypeAddWindow RunTypeAddWindowBox;

	public bool InsertedSimple;
	private bool descriptionChanging = false;
	private string name;

	RunTypeAddWindow (Gtk.Window parent, bool simple) {
		Glade.XML gladeXML;
		gladeXML = Glade.XML.FromAssembly (Util.GetGladePath() + "run_type_add.glade", "run_type_add", "chronojump");
		gladeXML.Autoconnect(this);
		run_type_add.Parent = parent;
		
		fakeButtonAccept = new Gtk.Button();

		//put an icon to window
		UtilGtk.IconWindow(run_type_add);

		//manage window color
		if(! Config.UseSystemColor) {
			UtilGtk.WindowColor(run_type_add, Config.ColorBackground);
			UtilGtk.ContrastLabelsLabel(Config.ColorBackgroundIsDark, label_header);
		}
	}
	
	static public RunTypeAddWindow Show (Gtk.Window parent, bool simple)
	{
		if (RunTypeAddWindowBox == null) {
			RunTypeAddWindowBox = new RunTypeAddWindow (parent, simple);
		}
		
		RunTypeAddWindowBox.fillDialog (simple);
		RunTypeAddWindowBox.run_type_add.Show ();

		return RunTypeAddWindowBox;
	}
	
	private void fillDialog (bool simple)
	{
		//active the desired radio
		if(simple)
			radiobutton_simple.Active = true;
		else
			radiobutton_interval.Active = true;

		//don't show the radios
		radiobutton_simple.Visible = false;
		radiobutton_interval.Visible = false;

		//if simple don't show nothing
		label_main_options.Visible = ! simple;
		table_main_options.Visible = ! simple;

		//hbox_fixed.Sensitive = false;	
		button_accept.Sensitive = false;
		spin_fixed_tracks_or_time.Sensitive = false;
		label_distance.Text = Catalog.GetString("Distance");
		//System.Globalization.NumberFormatInfo localeInfo = new System.Globalization.NumberFormatInfo();
		
		radiobutton_dist_different.Visible = ! simple;
		/*
		   instead of hidden, made unsensitive to have no problems on being inside a viewport
		hbox_distance_fixed.Hide();	
		vbox_distance_variable.Hide();	
		*/
		alignment_hbox_distance_fixed.Sensitive = false;
		alignment_vbox_distance_variable.Sensitive = false;
		if(simple)
			alignment_vbox_distance_variable.Hide();

		dd0 = new Gtk.Entry(); 	dd0.Changed += new EventHandler(on_entries_required_changed);
		dd1 = new Gtk.Entry(); 	dd1.Changed += new EventHandler(on_entries_required_changed);
		dd2 = new Gtk.Entry(); 	dd2.Changed += new EventHandler(on_entries_required_changed);
		dd3 = new Gtk.Entry(); 	dd3.Changed += new EventHandler(on_entries_required_changed);
		dd4 = new Gtk.Entry(); 	dd4.Changed += new EventHandler(on_entries_required_changed);
		dd5 = new Gtk.Entry(); 	dd5.Changed += new EventHandler(on_entries_required_changed);
		dd6 = new Gtk.Entry(); 	dd6.Changed += new EventHandler(on_entries_required_changed);
		dd7 = new Gtk.Entry(); 	dd7.Changed += new EventHandler(on_entries_required_changed);
		dd8 = new Gtk.Entry(); 	dd8.Changed += new EventHandler(on_entries_required_changed);
		dd9 = new Gtk.Entry(); 	dd9.Changed += new EventHandler(on_entries_required_changed);
	
		combo_distance_different_tracks.Active = 0;
		reset_hbox_distance_variable (2);

		System.Globalization.NumberFormatInfo localeInfo = new System.Globalization.NumberFormatInfo();
		localeInfo = System.Globalization.NumberFormatInfo.CurrentInfo;
		label_decimal_separator.Text += string.Format(Catalog.GetString("(decimal separator: '{0}')"), localeInfo.NumberDecimalSeparator);

		textview_description.Buffer.Changed += new EventHandler(descriptionChanged);
		descriptionChanging = false;
	}

	private void descriptionChanged(object o,EventArgs args)
	{
		if(descriptionChanging)
			return;

		descriptionChanging = true;

		TextBuffer tb = o as TextBuffer;
		if (o == null)
			return;

		tb.Text = Util.MakeValidSQL(tb.Text);
		descriptionChanging = false;
	}
		
	void on_button_cancel_clicked (object o, EventArgs args)
	{
		RunTypeAddWindowBox.run_type_add.Hide();
		RunTypeAddWindowBox = null;
	}
	
	void on_delete_event (object o, DeleteEventArgs args)
	{
		RunTypeAddWindowBox.run_type_add.Hide();
		RunTypeAddWindowBox = null;
	}
	
	void on_button_accept_clicked (object o, EventArgs args)
	{
		//ConsoleB.Information(getEntriesString());
		name = Util.RemoveTildeAndColonAndDot(entry_name.Text);

		//check if this run type exists, and check it's name is not AllRunsName
		bool runTypeExists = Sqlite.Exists (false, Constants.RunTypeTable, name);
		if(name == Constants.AllRunsNameStr())
			runTypeExists = true;
		
		if(runTypeExists) {
			string myString = string.Format(Catalog.GetString("Race type: '{0}' exists. Please, use another name"), name);
			LogB.Information (myString);
			ErrorWindow.Show(myString);
		} else {
			RunType type = new RunType();
			type.Name = name;
			type.Description = Util.RemoveTildeAndColon(textview_description.Buffer.Text);
						
			if(radiobutton_dist_variable.Active)
				type.Distance = 0;
			else if(radiobutton_dist_fixed.Active) 
				type.Distance = spin_distance_fixed.Value;
			else {
				//dist_different (only on intervallic)
				type.Distance = -1;
				type.DistancesString = getEntriesString();
			}
			
			if(radiobutton_simple.Active) {
				SqliteRunType.Insert(type, Constants.RunTypeTable, false); //false, because dbcon is not opened
				InsertedSimple = true;
			}
			else {
				if(radiobutton_unlimited.Active) {
					//unlimited (but in runs do like if it's limited by seconds: TracksLimited = false
					//(explanation in sqlite/jumpType.cs)
					type.TracksLimited = false; 
					type.FixedValue = 0; 
					type.Unlimited = true; 
				} else {
					type.TracksLimited = radiobutton_limited_tracks.Active;
				
					if(checkbutton_limited_fixed.Active) 
						type.FixedValue = Convert.ToInt32(spin_fixed_tracks_or_time.Value); 
					else 
						type.FixedValue = 0; 
					
						
					type.Unlimited = false; 
				}
			
				SqliteRunIntervalType.Insert(type, Constants.RunIntervalTypeTable, false); //false, because dbcon is not opened
				InsertedSimple = false;
			}
			
			//LogB.Information(string.Format("Inserted: {0}", type));
			
			fakeButtonAccept.Click();
		
			RunTypeAddWindowBox.run_type_add.Hide();
			RunTypeAddWindowBox = null;
		}
	}

	/* 
	 * when radiobutton is simple
	 * vboxLimited non sensitive
	 * hboxFixed non sensitive
	 * and distance different non sensitive
	 */


	void on_radiobutton_simple_toggled (object o, EventArgs args)
	{
		label_distance.Text = "Distance";
		vbox_limited.Sensitive = false;	
		hbox_fixed.Sensitive = false;	

		if(radiobutton_dist_different.Active)
			radiobutton_dist_variable.Active = true;
		radiobutton_dist_different.Sensitive = false;
		
	}
	
	void on_radiobutton_interval_toggled (object o, EventArgs args)
	{
		label_distance.Text = "Distance\nof each lap";
		vbox_limited.Sensitive = true;	
		if( ! radiobutton_unlimited.Active) {
			hbox_fixed.Sensitive = true;	
		}
		
		radiobutton_dist_different.Sensitive = true;
	}
	
	void on_radiobutton_limited_tracks_or_time_toggled (object o, EventArgs args)
	{
		hbox_fixed.Sensitive = true;	
	}
	
	void on_radiobutton_unlimited_toggled (object o, EventArgs args)
	{
		hbox_fixed.Sensitive = false;	
	}
	
	void on_checkbutton_limited_fixed_clicked (object o, EventArgs args)
	{
		if(checkbutton_limited_fixed.Active) 
			spin_fixed_tracks_or_time.Sensitive = true;
		else 
			spin_fixed_tracks_or_time.Sensitive = false;

	}
	
	void on_radiobutton_dist_variable_toggled (object o, EventArgs args)
	{
		alignment_hbox_distance_fixed.Sensitive = false;
		alignment_vbox_distance_variable.Sensitive = false;
	}
	
	void on_radiobutton_dist_fixed_toggled (object o, EventArgs args)
	{
		alignment_hbox_distance_fixed.Sensitive = true;
		alignment_vbox_distance_variable.Sensitive = false;
	}
	
	void on_radiobutton_dist_different_toggled (object o, EventArgs args)
	{
		alignment_hbox_distance_fixed.Sensitive = false;
		alignment_vbox_distance_variable.Sensitive = true;
		combo_distance_different_tracks.Sensitive = true;
	}
	
	void on_combo_distance_different_tracks_changed (object o, EventArgs args)
	{
		reset_hbox_distance_variable(Convert.ToInt32(UtilGtk.ComboGetActive(combo_distance_different_tracks)));
	}
	
	void reset_hbox_distance_variable (int colsNum) 
	{
		foreach(Gtk.Entry entry in hbox_distance_variable.Children)
			hbox_distance_variable.Remove(entry);

		int wc = 5; //widthChars (width of the entry)
		int ml = 5; //maxLength (max chars to entry)
		for (int i = 0; i < colsNum; i ++) {
			switch(i) {
				case 0: 
					dd0.WidthChars = wc; dd0.MaxLength = ml;
					hbox_distance_variable.PackStart(dd0, false, false, 0);
					break;
				case 1: 
					dd1.WidthChars = wc; dd1.MaxLength = ml;
					hbox_distance_variable.PackStart(dd1, false, false, 0);
					break;
				case 2: 
					dd2.WidthChars = wc; dd2.MaxLength = ml;
					hbox_distance_variable.PackStart(dd2, false, false, 0);
					break;
				case 3: 
					dd3.WidthChars = wc; dd3.MaxLength = ml;
					hbox_distance_variable.PackStart(dd3, false, false, 0);
					break;
				case 4: 
					dd4.WidthChars = wc; dd4.MaxLength = ml;
					hbox_distance_variable.PackStart(dd4, false, false, 0);
					break;
				case 5: 
					dd5.WidthChars = wc; dd5.MaxLength = ml;
					hbox_distance_variable.PackStart(dd5, false, false, 0);
					break;
				case 6: 
					dd6.WidthChars = wc; dd6.MaxLength = ml;
					hbox_distance_variable.PackStart(dd6, false, false, 0);
					break;
				case 7: 
					dd7.WidthChars = wc; dd7.MaxLength = ml;
					hbox_distance_variable.PackStart(dd7, false, false, 0);
					break;
				case 8: 
					dd8.WidthChars = wc; dd8.MaxLength = ml;
					hbox_distance_variable.PackStart(dd8, false, false, 0);
					break;
				case 9: 
					dd9.WidthChars = wc; dd9.MaxLength = ml;
					hbox_distance_variable.PackStart(dd9, false, false, 0);
					break;
			}
		}
		hbox_distance_variable.ShowAll();
	}
	
	private void on_button_help_rsa_clicked (object o, EventArgs args) {
		new DialogMessage(Constants.MessageTypes.HELP, 
				Catalog.GetString("On RSA tests, rest time counts as a \"lap\".") +
				"\n" + 	Catalog.GetString("You should write the time in seconds after a capital 'R' (meaning \"Rest\").") + 
				"\n\n" + 	Catalog.GetString("Eg. Aziz et al. (2000) test repeats 8 times the following sequence:") + 
				"\n\n\t" + Catalog.GetString("Run 40 meters, rest 30 seconds.") + 
				"\n\n" + Catalog.GetString("Will be limited by laps with a fixed value of 16") +
				"\n" + Catalog.GetString("because there are 16 laps:") +
				"\n" + Catalog.GetString("2 different laps: ('Run' and 'rest') x 8 times") +
				"\n\n" + Catalog.GetString("And the 'distance' of each different lap will be:") +
				"\n\n\t40, R30"
				);
	}

	private string getEntriesString () {
		string str = "";
		string separator = "";
		
		string ddString = dd0.Text + "-" + dd1.Text + "-" + dd2.Text + "-" + dd3.Text + "-" + dd4.Text + "-" + 
			dd5.Text + "-" + dd6.Text + "-" + dd7.Text + "-" + dd8.Text + "-" + dd9.Text; 
		string [] s = ddString.Split(new char[] {'-'});

		for(int i=0; i < Convert.ToInt32(UtilGtk.ComboGetActive(combo_distance_different_tracks)); i ++) {
			str += separator + s[i];
			separator = "-";
		}
		return str;
	}
	
	void on_entries_required_changed (object o, EventArgs args)
	{
		entry_name.Text = Util.MakeValidSQL(entry_name.Text);

		if(entry_name.Text.ToString().Length == 0) {
			button_accept.Sensitive = false;
			return;
		}

		if(radiobutton_dist_different.Active) {
			string ddString = getEntriesString();
			string [] ddSplitted = ddString.Split(new char[] {'-'});
			bool distancesAreOk = true;
			foreach (string s in ddSplitted) {
				string s2 = s;

				if( s2.Length == 0) {
					distancesAreOk = false;
					break;
				}

				if( s2.Length > 1 && s2[0] == 'R')
					s2 = s2.Substring(1);

				if( ! Util.IsNumber(s2, true)) {
					distancesAreOk = false;
					break;
				}
			}
			if(! distancesAreOk) {
				button_accept.Sensitive = false;
				return;
			}
		}
			
		button_accept.Sensitive = true;
	}


	public Button FakeButtonAccept 
	{
		set { fakeButtonAccept = value; }
		get { return fakeButtonAccept; }
	}

	public string Name
	{
		get { return name; }
	}
}

