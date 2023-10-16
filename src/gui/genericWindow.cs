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
using Gdk;
using Gtk;
//using Glade;
//using Gnome;
using GLib; //for Value
//using System.Text; //StringBuilder
using System.Collections; //ArrayList
using System.Collections.Generic; //List<T>
using Mono.Unix;


public class GenericWindow
{
	// at glade ---->
	Gtk.Window generic_window;
	Gtk.Label label_header;
	Gtk.Box hbox_error;
	Gtk.Label label_error;
	Gtk.Frame frame_data;
	Gtk.Entry entry;
	
	Gtk.Box hbox_spin_int;
	Gtk.Label label_spin_int;
	Gtk.SpinButton spin_int;
	Gtk.Box hbox_spin_int2;
	Gtk.Label label_spin_int2;
	Gtk.SpinButton spin_int2;
	Gtk.Box hbox_spin_int3;
	Gtk.Label label_spin_int3;
	Gtk.SpinButton spin_int3;

	Gtk.SpinButton spin_double;
	Gtk.Box hbox_height_metric;

	Gtk.CheckButton check1;

	Gtk.HButtonBox hbuttonbox_middle;
	Gtk.Button button_middle;

	Gtk.Grid grid_person_session;
	Gtk.Box box_combo_person_select;
	Gtk.Box box_combo_session_select;

	//Edit row
	Gtk.Box hbox_edit_row;
	Gtk.Label hbox_combo_label;
	Gtk.Label label_comment;
	Gtk.Box hbox_combo_edit;
	Gtk.Button hbox_combo_button_apply;
	Gtk.Entry entry_edit_row;
	
	Gtk.Box hbox_all_none_selected;
	Gtk.Box hbox_combo_all_none_selected;
	Gtk.SpinButton spin_feet;
	Gtk.SpinButton spin_inches;
	Gtk.Label label_before_textview_treeview;
	Gtk.ScrolledWindow scrolled_window_textview;
	Gtk.TextView textview;
	Gtk.ScrolledWindow scrolled_window_treeview;
	Gtk.TreeView treeview;
	Gtk.Button button_accept;
	Gtk.Button button_cancel;
	Gtk.Image image_button_accept;
	Gtk.Image image_button_cancel;
	Gtk.Label label_button_accept;
	Gtk.Label label_button_cancel;

	//treeview fake buttons
	Gtk.Button button_row_edit;
	Gtk.Button button_row_play;
	Gtk.Button button_row_delete;

	Gtk.Label label_treeviewload_row;
	Gtk.Button button_treeviewload_row_edit;
	Gtk.Button button_treeviewload_row_delete;
	Gtk.Button button_treeviewload_row_play;
	Gtk.Image image_treeviewload_row_play;
	
	Gtk.Box hbox_entry2;
	Gtk.Label label_entry2;
	Gtk.Entry entry2;
	Gtk.Box hbox_entry3;
	Gtk.Label label_entry3;
	Gtk.Entry entry3;

	Gtk.Box hbox_spin_double2;
	Gtk.Label label_spin_double2;
	Gtk.SpinButton spin_double2;
	// <---- at glade
	
	Gtk.ComboBoxText combo_edit;
	Gtk.ComboBoxText combo_all_none_selected;
	Gtk.ComboBoxText combo_person_select;
	Gtk.ComboBoxText combo_session_select;

	private ArrayList nonSensitiveRows;

	static GenericWindow GenericWindowBox;
	
	private TreeStore store;
	private bool textviewChanging = false;

	public enum EditActions { NONE, EDITDELETE, EDITPLAYDELETE, DELETE }

	//used to read data, see if it's ok, and print an error message.
	//if all is ok, destroy it with HideAndNull()
	public bool HideOnAccept;
	
	//used when we don't need to read data, 
	//and we want to ensure next window will be created at needed size
	public bool DestroyOnAccept;
	public int TreeviewSelectedUniqueID;
	private int videoColumn = 0;
	private int commentColumn;

	public int uniqueID; 			//used on encoder & forceSensor edit exercise

	public enum Types { UNDEFINED, ENCODER_SESSION_LOAD, 
		ENCODER_SEL_REPS_IND_CURRENT_SESS, ENCODER_SEL_REPS_IND_ALL_SESS, ENCODER_SEL_REPS_GROUP_CURRENT_SESS, TAGSESSION };
	//used to decide if a genericWin has to be recreated
	public Types Type;

	public GenericWindow (string title, string textHeader)
	{
		/*
		Glade.XML gladeXML;
		gladeXML = Glade.XML.FromAssembly (Util.GetGladePath() + "generic_window.glade", "generic_window", "chronojump");
		gladeXML.Autoconnect(this);
		*/
		Gtk.Builder builder = new Gtk.Builder (null, Util.GetGladePath () + "generic_window.glade", null);
		connectWidgets (builder);
		builder.Autoconnect (this);
	
		//put an icon to window
		UtilGtk.IconWindow(generic_window);

		//manage window color
		if(! Config.UseSystemColor)
		{
			UtilGtk.WindowColor(generic_window, Config.ColorBackground);
			UtilGtk.ContrastLabelsLabel (Config.ColorBackgroundIsDark, label_header);
			UtilGtk.ContrastLabelsLabel (Config.ColorBackgroundIsDark, label_error);
			UtilGtk.ContrastLabelsLabel (Config.ColorBackgroundIsDark, hbox_combo_label);
			UtilGtk.ContrastLabelsLabel (Config.ColorBackgroundIsDark, label_comment);
			UtilGtk.ContrastLabelsLabel (Config.ColorBackgroundIsDark, label_treeviewload_row);

			UtilGtk.WidgetColor (frame_data, Config.ColorBackgroundShifted);
			UtilGtk.ContrastLabelsFrame (Config.ColorBackgroundShiftedIsDark, frame_data);
		}
		
		generic_window.Resizable = false;
		setTitle(title);
		label_header.Text = textHeader;
		
		HideOnAccept = true;
		DestroyOnAccept = false;
	}

	//for an array of widgets
	static public GenericWindow Show (string title, bool showNow, string textHeader, ArrayList array)
	{
		if (GenericWindowBox == null) {
			GenericWindowBox = new GenericWindow(title, textHeader);
		} else {
			GenericWindowBox.setTitle(title);
			GenericWindowBox.label_header.Text = textHeader;
		}

		GenericWindowBox.Type = Types.UNDEFINED;

		GenericWindowBox.hideWidgets();

		foreach(ArrayList widgetArray in array)
			GenericWindowBox.showWidgetsPowerful(widgetArray);

		Pixbuf pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "video_play.png");
		GenericWindowBox.image_treeviewload_row_play.Pixbuf = pixbuf;

		GenericWindowBox.image_button_cancel.Pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_cancel.png");
		GenericWindowBox.image_button_accept.Pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_done_blue.png");

		if(showNow)
			GenericWindowBox.generic_window.Show ();
		
		return GenericWindowBox;
	}

	//for only one widget
	static public GenericWindow Show (string title, string textHeader, Constants.GenericWindowShow stuff, bool sensitive)
	{
		if (GenericWindowBox == null) {
			GenericWindowBox = new GenericWindow(title, textHeader);
		} else {
			GenericWindowBox.setTitle(title);
			GenericWindowBox.label_header.Text = textHeader;
		}

		GenericWindowBox.Type = Types.UNDEFINED;

		GenericWindowBox.hideWidgets();

		GenericWindowBox.image_button_cancel.Pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_cancel.png");
		GenericWindowBox.image_button_accept.Pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_done_blue.png");

		GenericWindowBox.showWidget(stuff, sensitive);
		GenericWindowBox.generic_window.Show ();
		
		return GenericWindowBox;
	}
	
	public void ShowNow() {
		if(GenericWindowBox == null) {
			LogB.Error("at showNow GenericWindowBox is null!!");
		//	GenericWindowBox = new GenericWindow("");
		}

		GenericWindowBox.generic_window.Show ();
	}

	private void setTitle(string title)
	{
		if(title != "")
			generic_window.Title = "Chronojump - " + title;
	}

	void hideWidgets() {
		hbox_error.Hide();
		entry.Hide();
		hbox_entry2.Hide();
		hbox_entry3.Hide();
		hbox_spin_int.Hide();
		hbox_spin_int2.Hide();
		hbox_spin_int3.Hide();
		spin_double.Hide();
		hbox_spin_double2.Hide();
		hbox_height_metric.Hide();
		check1.Hide();
		hbox_edit_row.Hide();
		label_treeviewload_row.Hide();
		button_treeviewload_row_edit.Hide();
		button_treeviewload_row_play.Hide();
		button_treeviewload_row_delete.Hide();
		hbox_all_none_selected.Hide();
		hbox_combo_all_none_selected.Hide();
		hbuttonbox_middle.Hide();
		grid_person_session.Hide ();
		label_before_textview_treeview.Hide();
		scrolled_window_textview.Hide();
		scrolled_window_treeview.Hide();
	}
	
	void showWidgetsPowerful (ArrayList widgetArray) {
		Constants.GenericWindowShow stuff = (Constants.GenericWindowShow) widgetArray[0];
		bool editable = (bool) widgetArray[1];
		string text = (string) widgetArray[2];
		
		if(stuff == Constants.GenericWindowShow.ENTRY) {
			entry.Show();
			entry.IsEditable = editable;
			entry.Sensitive = editable;
			entry.Text = text;
		}
		else if(stuff == Constants.GenericWindowShow.ENTRY2) {
			hbox_entry2.Show();
			entry2.IsEditable = editable;
			entry2.Sensitive = editable;
			entry2.Text = text;
		}
		else if(stuff == Constants.GenericWindowShow.ENTRY3) {
			hbox_entry3.Show();
			entry3.IsEditable = editable;
			entry3.Sensitive = editable;
			entry3.Text = text;
		}
		else if(stuff == Constants.GenericWindowShow.SPININT) {
			hbox_spin_int.Show();
			spin_int.IsEditable = editable;
			spin_int.Sensitive = editable;
		}
		else if(stuff == Constants.GenericWindowShow.SPINDOUBLE) {
			spin_double.Show();
			spin_double.IsEditable = editable;
		}
		else if(stuff == Constants.GenericWindowShow.HBOXSPINDOUBLE2) {
			hbox_spin_double2.Show();
			spin_double2.IsEditable = editable;
			spin_double2.Sensitive = editable;
		}
		else if(stuff == Constants.GenericWindowShow.SPININT2) {
			hbox_spin_int2.Show();
			spin_int2.IsEditable = editable;
		}
		else if(stuff == Constants.GenericWindowShow.SPININT3) {
			hbox_spin_int3.Show();
			spin_int3.IsEditable = editable;
		}
		else if(stuff == Constants.GenericWindowShow.HEIGHTMETRIC) {
			hbox_height_metric.Show();
		}
		else if(stuff == Constants.GenericWindowShow.CHECK1) {
			check1.Active = (text == "TRUE");
			check1.Show();
		}
		else if(stuff == Constants.GenericWindowShow.COMBO) {
			/*
			hbox_combo_edit.Show();
			combo_edit.Show();
			*/
		}
		else if(stuff == Constants.GenericWindowShow.COMBOALLNONESELECTED) {
			//createComboCheckBoxes();
			//combo_all_none_selected.Active = 
			//	UtilGtk.ComboMakeActive(comboCheckBoxesOptions, Catalog.GetString("Selected"));
			hbox_combo_all_none_selected.Show();
			hbox_all_none_selected.Show();
		}
		else if(stuff == Constants.GenericWindowShow.BUTTONMIDDLE) {
			hbuttonbox_middle.Show();
		}
		else if(stuff == Constants.GenericWindowShow.GRIDPERSONSESSION)
		{
			grid_person_session.Show ();
			UseGridPersonSession = true;
		}
		else if(stuff == Constants.GenericWindowShow.LABELBEFORETEXTVIEWTREEVIEW) {
			label_before_textview_treeview.Show();
		}
		else if(stuff == Constants.GenericWindowShow.TEXTVIEW) {
			scrolled_window_textview.Show();
		}
		else { //if(stuff == Constants.GenericWindowShow.TREEVIEW)
			scrolled_window_treeview.Show();
		}
		
	}

	void showWidget(Constants.GenericWindowShow stuff, bool sensitive)
	{
		if(stuff == Constants.GenericWindowShow.ENTRY)
		{
			entry.Show();
			entry.Sensitive = sensitive;
		}
		else if(stuff == Constants.GenericWindowShow.ENTRY2)
		{
			hbox_entry2.Show();
			hbox_entry2.Sensitive = sensitive;
		}
		else if(stuff == Constants.GenericWindowShow.ENTRY3)
		{
			hbox_entry3.Show();
			hbox_entry3.Sensitive = sensitive;
		}
		else if(stuff == Constants.GenericWindowShow.SPININT)
		{
			hbox_spin_int.Show();
			hbox_spin_int.Sensitive = sensitive;
		}
		else if(stuff == Constants.GenericWindowShow.SPINDOUBLE)
		{
			spin_double.Show();
			spin_double.Sensitive = sensitive;
		}
		else if(stuff == Constants.GenericWindowShow.HBOXSPINDOUBLE2)
		{
			hbox_spin_double2.Show();
			hbox_spin_double2.Sensitive = sensitive;
		}
		else if(stuff == Constants.GenericWindowShow.HEIGHTMETRIC)
		{
			hbox_height_metric.Show();
			hbox_height_metric.Sensitive = sensitive;
		}
		else if(stuff == Constants.GenericWindowShow.CHECK1)
		{
			check1.Show();
			check1.Sensitive = sensitive;
		}
		else if(stuff == Constants.GenericWindowShow.SPININT2)
		{
			hbox_spin_int2.Show();
			hbox_spin_int2.Sensitive = sensitive;
		}
		else if(stuff == Constants.GenericWindowShow.SPININT3)
		{
			hbox_spin_int3.Show();
			hbox_spin_int3.Sensitive = sensitive;
		}
		else if(stuff == Constants.GenericWindowShow.COMBO) {
			//do later, we need to create them first
			/*
			hbox_combo_edit.Show();
			combo_edit.Show();
			*/
		}
		else if(stuff == Constants.GenericWindowShow.BUTTONMIDDLE)
		{
			hbuttonbox_middle.Show();
			hbuttonbox_middle.Sensitive = sensitive;
		}
		else if(stuff == Constants.GenericWindowShow.TEXTVIEW)
		{
			scrolled_window_textview.Show();
			scrolled_window_textview.Sensitive = sensitive;
		}
		else //if(stuff == Constants.GenericWindowShow.TREEVIEW)
		{
			scrolled_window_treeview.Show();
			scrolled_window_treeview.Sensitive = sensitive;
		}
	}

	private void on_entries_changed (object o, EventArgs args)
	{
		Gtk.Entry entry = o as Gtk.Entry;
		if (o == null)
			return;

		entry.Text = Util.MakeValidSQL(entry.Text);
	}

	public void SetSize(int width, int height) {
		generic_window.SetDefaultSize(width, height);
	}

	//this works (but can resize lower than needed)
	public void SetSizeRequest(int width, int height) {
		generic_window.SetSizeRequest(width, height);
	}

	
	public void SetTreeviewSize(int width, int height) {
		treeview.SetSizeRequest(width, height);
	}


	public void SetLabelError(string text) {
		label_error.Text = text;
		hbox_error.Show();
	}
	
	public void SetSpinValue(int num) {
		spin_int.Value = num;
	}
	public void SetSpinRange(int min, int max) {
		spin_int.SetRange(min, max);
	}
	public void SetSpinRange(double min, double max) {
		spin_int.SetRange(min, max);
	}
	
	public void SetSpin2Value(int num) {
		spin_int2.Value = num;
	}
	public void SetSpin2Range(int min, int max) {
		spin_int2.SetRange(min, max);
	}
	
	public void SetSpin3Value(int num) {
		spin_int3.Value = num;
	}
	public void SetSpin3Range(int min, int max) {
		spin_int3.SetRange(min, max);
	}
	
	public void SetSpinDouble2Value(double num) {
		spin_double2.Value = num;
	}
	public void SetSpinDouble2Increments(double min, double max) {
		spin_double2.SetIncrements(min, max);
	}
	public void SetSpinDouble2Range(double min, double max) {
		spin_double2.SetRange(min, max);
	}
	public void SetSpinDouble2Digits(uint digits) {
		spin_double2.Digits = digits;
	}
	
	public void SetComboEditValues(string [] values, string current) {
		combo_edit = new ComboBoxText ();
		UtilGtk.ComboUpdate (combo_edit, values, "");

		//if there hbox already has a combo (window has not been properly destroyed)
		//remove that combo (to not have two combos)
		if (hbox_combo_edit.Children.Length > 0)
			hbox_combo_edit.Remove (combo_edit);

		hbox_combo_edit.PackStart (combo_edit, true, true, 0);
		hbox_combo_edit.ShowAll ();
		combo_edit.Sensitive = true;
			
		combo_edit.Active = UtilGtk.ComboMakeActive (values, current);
	}
	public void SetComboLabel(string l) {
		hbox_combo_label.Text = l;
	}
	public void ShowEditRow(bool show) {
		hbox_edit_row.Visible = show;
	}

	public void SetCheck1Label(string s) {
		check1.Label = s;
	}
	
	private static string [] comboCheckBoxesOptionsDefault = {
		Catalog.GetString("All"),
		Catalog.GetString("None"),
		Catalog.GetString("Invert"),
		Catalog.GetString("Selected"),
	};
	private string [] comboCheckBoxesOptions = comboCheckBoxesOptionsDefault;

	public void ResetComboCheckBoxesOptions() {
		comboCheckBoxesOptions = comboCheckBoxesOptionsDefault;
	}
	
	//this search in first column
	public void AddOptionsToComboCheckBoxesOptions(ArrayList newOptions) {
		comboCheckBoxesOptions = Util.AddArrayString( comboCheckBoxesOptions, 
				Util.ArrayListToString(newOptions) );
	}

	public void CreateComboCheckBoxes() 
	{
		if(hbox_combo_all_none_selected.Children.Length > 0)
			hbox_combo_all_none_selected.Remove(combo_all_none_selected);

		combo_all_none_selected = new ComboBoxText ();
		UtilGtk.ComboUpdate(combo_all_none_selected, comboCheckBoxesOptions, "");
		
		//combo_all_none_selected.DisableActivate ();
		combo_all_none_selected.Changed += new EventHandler (on_combo_all_none_selected_changed);

		hbox_combo_all_none_selected.PackStart(combo_all_none_selected, true, true, 0);
		hbox_combo_all_none_selected.ShowAll();
		combo_all_none_selected.Sensitive = true;
			
		combo_all_none_selected.Active = 
			UtilGtk.ComboMakeActive(comboCheckBoxesOptions, Catalog.GetString("Selected"));
	}
	
	private void on_combo_all_none_selected_changed(object o, EventArgs args) {
		string myText = UtilGtk.ComboGetActive(combo_all_none_selected);
			
		if (myText != "" & myText != Catalog.GetString("Selected")) {
			try {
				markSelected(myText);
			} catch {
				LogB.Warning("Do later!!");
			}
		}
	}
	
	private void markSelected(string selected) {
		Gtk.TreeIter iter;
		bool okIter = store.GetIterFirst(out iter);
		if(okIter) {
			int i=0;
			if(selected == Catalog.GetString("All")) {
				do {
					if(! Util.FoundInArrayList(nonSensitiveRows, i))
						store.SetValue (iter, 1, true);
					i++;
				} while ( store.IterNext(ref iter) );
			} else if(selected == Catalog.GetString("Invert")) {
				bool val;
				do {
					if(! Util.FoundInArrayList(nonSensitiveRows, i)) {
						val = (bool) store.GetValue (iter, 1);
						store.SetValue (iter, 1, !val);
					}
					i++;
				} while ( store.IterNext(ref iter) );
			} else if(selected == Catalog.GetString("None")) {
				do {
					store.SetValue (iter, 1, false);
				} while ( store.IterNext(ref iter) );
			} else {	//encoderExercises
				do {
					if(selected == (string) store.GetValue (iter, 3) &&
							! Util.FoundInArrayList(nonSensitiveRows, i))
						store.SetValue (iter, 1, true);
					else
						store.SetValue (iter, 1, false);
					i++;
				} while ( store.IterNext(ref iter) );
			}
		}
			
		//check if there are rows checked for having sensitive or not in recuperate button
		//buttonRecuperateChangeSensitiveness();
	}
	
	public void SetButtonMiddleLabel(string str) {
		button_middle.Label=str;
	}

	//what is in app1
	public Gtk.Button FakeButtonNeedUpdateTreeView;
	public bool UseGridPersonSession;
	private Person currentPerson;
	private Session currentSession;
	private bool followSignals = false;
	public void SetGridPersonSession (Person currentPerson, Session currentSession)
	{
		UseGridPersonSession = true;
		FakeButtonNeedUpdateTreeView = new Gtk.Button ();
		// assign the variables on app1
		this.currentPerson = currentPerson;
		this.currentSession = currentSession;

		// create the empty combos
		combo_person_select = UtilGtk.CreateComboBoxText (box_combo_person_select, new List<string> (), "");
		combo_person_select.Changed += new EventHandler (on_combo_person_select_changed);

		combo_session_select = UtilGtk.CreateComboBoxText (box_combo_session_select, new List<string> (), "");
		combo_session_select.Changed += new EventHandler (on_combo_session_select_changed);

		// update the combo values
		updateGridPersonChanged ();
		updateGridSessionChanged ();
	}

	private void updateGridPersonChanged ()
	{
		// 1) get person and session from gui
		int personID; string personName;
		getPersonFromGui (out personID, out personName);

		int sessionID; string sessionName;
		getSessionFromGui (out sessionID, out sessionName);

		// 2) select sessions for this person
		List<PersonSession> psCurrentPerson_l = SqlitePersonSession.SelectPersonSessionList (false, personID, -1);
		List<Session> session_l = SqliteSession.SelectAll (false, Sqlite.Orders_by.DEFAULT);

		List<string> sessionStr_l = new List<string> ();
		foreach (PersonSession ps in psCurrentPerson_l)
			foreach (Session s in session_l)
				if (s.UniqueID == ps.SessionID)
					sessionStr_l.Add (string.Format("{0}:{1}", s.UniqueID, s.Name));

		// 3) update the session combo
		followSignals = false;
		UtilGtk.ComboUpdate (combo_session_select, sessionStr_l);
		combo_session_select.Active = UtilGtk.ComboMakeActive (combo_session_select,
				string.Format ("{0}:{1}", sessionID, sessionName));
		followSignals = true;
	}

	private void updateGridSessionChanged ()
	{
		// 1) get person and session from gui
		int personID; string personName;
		getPersonFromGui (out personID, out personName);

		int sessionID; string sessionName;
		getSessionFromGui (out sessionID, out sessionName);

		// 2) select persons for this session
		List<Person> person_l = SqlitePersonSession.SelectCurrentSessionPersonsAsList (false, sessionID);
		List<string> personStr_l = new List<string> ();
		foreach (Person p in person_l)
			personStr_l.Add (string.Format("{0}:{1}", p.UniqueID, p.Name));

		// 3) update the person combo
		followSignals = false;
		UtilGtk.ComboUpdate (combo_person_select, personStr_l);
		combo_person_select.Active = UtilGtk.ComboMakeActive (combo_person_select,
				string.Format("{0}:{1}", personID, personName));
		followSignals = true;
	}

	private void getPersonFromGui (out int personID, out string personName)
	{
		// just if it fails for any reason
		personID = currentPerson.UniqueID;
		personName = currentPerson.Name;

		string str = UtilGtk.ComboGetActive (combo_person_select);
		string [] strFull = str.Split(new char[] {':'});
		if (strFull.Length == 2 && Util.IsNumber (strFull[0], false))
		{
			personID = Convert.ToInt32 (strFull[0]);
			personName = strFull[1];
		}
	}

	public int GetPersonIDFromGui ()
	{
		int personID; string personName;
		getPersonFromGui (out personID, out personName);

		return personID;
	}

	private void getSessionFromGui (out int sessionID, out string sessionName)
	{
		// just if it fails for any reason
		sessionID = currentSession.UniqueID;
		sessionName = currentSession.Name;

		string str = UtilGtk.ComboGetActive (combo_session_select);
		string [] strFull = str.Split(new char[] {':'});
		if (strFull.Length == 2 && Util.IsNumber (strFull[0], false))
		{
			sessionID = Convert.ToInt32 (strFull[0]);
			sessionName = strFull[1];
		}
	}

	public int GetSessionIDFromGui ()
	{
		int sessionID; string sessionName;
		getSessionFromGui (out sessionID, out sessionName);

		return sessionID;
	}

	private void on_combo_person_select_changed (object o, EventArgs args)
	{
		if (followSignals)
		{
			updateGridPersonChanged ();
			FakeButtonNeedUpdateTreeView.Click ();
		}
	}

	private void on_combo_session_select_changed (object o, EventArgs args)
	{
		if (followSignals)
		{
			updateGridSessionChanged ();
			FakeButtonNeedUpdateTreeView.Click ();
		}
	}


	public void SetTextview(string str)
	{
		TextBuffer tb = new TextBuffer (new TextTagTable());
		tb.Text = str;
		textview.Buffer = tb;

		textview.Buffer.Changed += new EventHandler(textviewChanged);
		textviewChanging = false;
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

	bool activateRowAcceptsWindow;
	//data is an ArrayList of strings[], each string [] is a row, each of its strings is a column
	public void SetTreeview(string [] columnsString, bool addCheckbox, 
			ArrayList data, ArrayList myNonSensitiveRows,
			EditActions editAction,
			bool activateRowAcceptsWindow	//this param makes button_accept the window if 'enter' on a row or double click
			) 
	{
		//adjust window to be bigger
		generic_window.Resizable = true;
		scrolled_window_treeview.WidthRequest = 550;
		scrolled_window_treeview.HeightRequest = 250;

		store = getStore(columnsString.Length, addCheckbox); 
		treeview.Model = store;
		prepareHeaders(columnsString, addCheckbox);
		treeview.HeadersClickable = false;

		nonSensitiveRows = myNonSensitiveRows;
	
		LogB.Debug("aaaaaaaaaaaaaaaa1");	
		foreach (string [] line in data) {
			store.AppendValues (line);
			//Log.WriteLine(Util.StringArrayToString(line,"\n"));
		}
		LogB.Debug("aaaaaaaaaaaaaaaa2");	

		this.activateRowAcceptsWindow = activateRowAcceptsWindow;

		if(editAction == EditActions.EDITDELETE)
		{
			label_treeviewload_row.Sensitive = false;
			label_treeviewload_row.Visible = true;
			button_treeviewload_row_edit.Sensitive = false;
			button_treeviewload_row_edit.Visible = true;
			button_treeviewload_row_delete.Sensitive = false;
			button_treeviewload_row_delete.Visible = true;
			button_row_edit = new Gtk.Button();
			button_row_delete = new Gtk.Button();
		} else if(editAction == EditActions.EDITPLAYDELETE)
		{
			label_treeviewload_row.Sensitive = false;
			label_treeviewload_row.Visible = true;
			button_treeviewload_row_edit.Sensitive = false;
			button_treeviewload_row_edit.Visible = true;
			button_treeviewload_row_play.Sensitive = false;
			button_treeviewload_row_play.Visible = true;
			button_treeviewload_row_delete.Sensitive = false;
			button_treeviewload_row_delete.Visible = true;
			button_row_edit = new Gtk.Button();
			button_row_play = new Gtk.Button();
			button_row_delete = new Gtk.Button();
		} else if(editAction == EditActions.DELETE)
		{
			label_treeviewload_row.Sensitive = false;
			label_treeviewload_row.Visible = true;
			button_row_delete = new Gtk.Button();
			button_treeviewload_row_delete.Sensitive = false;
			button_treeviewload_row_delete.Visible = true;
		}

		treeview.CursorChanged -= on_treeview_cursor_changed;
		treeview.CursorChanged += on_treeview_cursor_changed;
	}

	public void SelectRowWithID(int colNum, int id) 
	{
		if(id <= 0)
			return;

		UtilGtk.TreeviewSelectRowWithID(treeview, store, colNum, id, true); //last boolean is 'scroll to row'
	}
	
	public void MarkActiveCurves(string [] checkboxes) 
	{
		int count = 0;
		Gtk.TreeIter iter;
		bool okIter = store.GetIterFirst(out iter);
		if(okIter) {
			do {
				if(checkboxes[count++] == "active")
					store.SetValue (iter, 1, true);
			} while ( store.IterNext(ref iter) );
		}
	}
	
	private TreeStore getStore (int columns, bool addCheckbox)
	{
		//prepares the TreeStore for required columns
		Type [] types = new Type [columns];

		for (int i=0; i < columns; i++) {
			if(addCheckbox && i == 1)
				types[1] = typeof (bool);
			else
				types[i] = typeof (string);
		}
		TreeStore myStore = new TreeStore(types);
		return myStore;
	}
	
	private void prepareHeaders(string [] columnsString, bool addCheckbox) 
	{
		treeviewRemoveColumns();
		treeview.HeadersVisible=true;
		int i=0;
		bool visible = false;
		foreach(string myCol in columnsString) {
			if(addCheckbox && i == 1)
				createCheckboxes(treeview);
			else
				UtilGtk.CreateCols(treeview, store, myCol, i, visible);
//			if(i == 1)	//first columns: ID, is hidden
//				store.SetSortFunc (0, UtilGtk.IdColumnCompare);
			visible = true;
			i++;
		}
	}
	
	private void treeviewRemoveColumns() {
		Gtk.TreeViewColumn [] myColumns = treeview.Columns;
		foreach (Gtk.TreeViewColumn column in myColumns) {
			treeview.RemoveColumn (column);
		}
	}

	private void on_treeview_cursor_changed (object o, EventArgs args) 
	{
		TreeIter iter = new TreeIter();
		ITreeModel myModel = treeview.Model;
		if (treeview.Selection.GetSelected (out myModel, out iter))
		{
			SetButtonAcceptSensitive(true);
			label_treeviewload_row.Sensitive = true;
			button_treeviewload_row_edit.Sensitive = true;
			button_treeviewload_row_delete.Sensitive = true;

			string video = (string) myModel.GetValue (iter, videoColumn);
			button_treeviewload_row_play.Sensitive = (video == Catalog.GetString("Yes"));
		}
		else
		{
			SetButtonAcceptSensitive(false);
			label_treeviewload_row.Sensitive = false;
			button_treeviewload_row_edit.Sensitive = false;
			button_treeviewload_row_delete.Sensitive = false;
			button_treeviewload_row_play.Sensitive = false;
		}

		ShowEditRow(false);
	}

	public void SensitiveEditDeleteIfSelected()
	{
		TreeIter iter = new TreeIter();
		ITreeModel myModel = treeview.Model;
		if (treeview.Selection.GetSelected (out myModel, out iter))
		{
			label_treeviewload_row.Sensitive = false;
			button_treeviewload_row_edit.Sensitive = true;
			button_treeviewload_row_play.Sensitive = true;
			button_treeviewload_row_delete.Sensitive = true;
		}
	}

	//get the selected	
	public int TreeviewSelectedRowID() {
		TreeIter iter = new TreeIter();
		ITreeModel myModel = treeview.Model;
		if (treeview.Selection.GetSelected (out myModel, out iter)) 
			return Convert.ToInt32(treeview.Model.GetValue (iter, 0));
		else
			return 0;
	}

	private void createCheckboxes(TreeView tv) 
	{
		CellRendererToggle crt = new CellRendererToggle();
		crt.Visible = true;
		crt.Activatable = true;
		crt.Active = true;
		crt.Toggled += ItemToggled;

		TreeViewColumn column = new TreeViewColumn ("", crt, "active", 1);
		column.Clickable = true;
		tv.AppendColumn (column);
		
	}
	
	public int GetCell(int uniqueID, int column) 
	{
		//LogB.Information(" GetCell " + uniqueID.ToString() + " " + column.ToString());
		Gtk.TreeIter iter;
		bool okIter = store.GetIterFirst(out iter);
		if(okIter) {
			do {
				//LogB.Information("_0_ " + (string) store.GetValue (iter, 0));
				//LogB.Information("_column_ " + column.ToString() + " " + (string) store.GetValue (iter, column));
				if( ((string) store.GetValue (iter, 0)) == uniqueID.ToString()) {
					return Convert.ToInt32( (string) store.GetValue (iter, column) );
				}
			} while ( store.IterNext(ref iter) );
		}

		return 0;
	}
	public bool GetCheckboxStatus(int uniqueID) {
		Gtk.TreeIter iter;
		bool okIter = store.GetIterFirst(out iter);
		if(okIter) {
			do {
				if( ((string) store.GetValue (iter, 0)) == uniqueID.ToString())
					return (bool) store.GetValue (iter, 1);
			} while ( store.IterNext(ref iter) );
		}
		//if error, return false
		return false;
	}
	//if column == 1 returns checkboxes column. If is 2 returns column 2...
	//Attention: Used on checkboxes treeviews
	public string [] GetColumn(int column, bool onlyActive) 
	{
		//to store active or inactive status of curves
		string [] checkboxes = new string[UtilGtk.CountRows(store)];
		
		int count = 0;
		Gtk.TreeIter iter;
		bool okIter = store.GetIterFirst(out iter);
		if(okIter) {
			do {
				if(column == 1) {
					if((bool) store.GetValue (iter, 1))
						checkboxes[count++] = "active";
					else
						checkboxes[count++] = "inactive";
				}
				else
					if((bool) store.GetValue (iter, 1) || ! onlyActive)
						checkboxes[count++] = ((string) store.GetValue (iter, column));
				
			} while ( store.IterNext(ref iter) );
		}
		if(column == 1)
			return checkboxes;
		else {
			string [] checkboxesWithoutGaps = new string[count];
			for(int i=0; i < count; i ++)
				checkboxesWithoutGaps[i] = checkboxes[i];
			return checkboxesWithoutGaps;
		}
	}

	private void ItemToggled(object o, ToggledArgs args) {
		int column = 1;
		TreeIter iter;
		if (store.GetIter (out iter, new TreePath(args.Path))) 
		{
			//Log.WriteLine(args.Path);
			if(! Util.FoundInArrayList(nonSensitiveRows, 
						Convert.ToInt32(args.Path))) {
				bool val = (bool) store.GetValue (iter, column);
				LogB.Information (string.Format("toggled {0} with value {1}", args.Path, !val));

				store.SetValue (iter, column, !val);

				combo_all_none_selected.Active =
					UtilGtk.ComboMakeActive(
							comboCheckBoxesOptions, Catalog.GetString("Selected"));

				//check if there are rows checked for having sensitive or not
				//buttonRecuperateChangeSensitiveness();
				
				hbox_error.Hide();
			} else {
				label_error.Text = "Cannot select rows without data";
				hbox_error.Show();
			}
		}
	}
	
	void on_row_double_clicked (object o, Gtk.RowActivatedArgs args)
	{
		if(activateRowAcceptsWindow) {
			TreeView tv = (TreeView) o;
			ITreeModel model;
			TreeIter iter;

			if (tv.Selection.GetSelected (out model, out iter)) {
				//activate on_button_accept_clicked()
				button_accept.Activate();
			}
		}
	}

	private void on_edit_selected_clicked (object o, EventArgs args) 
	{
		ITreeModel model;
		TreeIter iter = new TreeIter();
		if(! treeview.Selection.GetSelected (out model, out iter))
			return;

		TreeviewSelectedUniqueID = Convert.ToInt32((string) store.GetValue (iter, 0));

		entry_edit_row.Text = (string) model.GetValue (iter, commentColumn);

		label_treeviewload_row.Sensitive = false;
		button_treeviewload_row_edit.Sensitive = false;
		button_treeviewload_row_play.Sensitive = false;
		button_treeviewload_row_delete.Sensitive = false;

		button_row_edit.Click();
	}
	
	public void on_edit_selected_done_update_treeview() 
	{
		//remove conflictive characters
		entry_edit_row.Text = Util.RemoveTildeAndColonAndDot(entry_edit_row.Text);

		ITreeModel model;
		TreeIter iter = new TreeIter();
		treeview.Selection.GetSelected (out model, out iter);
		store.SetValue (iter, commentColumn, entry_edit_row.Text);
	}

	private void on_play_selected_clicked (object o, EventArgs args)
	{
		ITreeModel model;
		TreeIter iter = new TreeIter();
		if(! treeview.Selection.GetSelected (out model, out iter))
			return;

		TreeviewSelectedUniqueID = Convert.ToInt32((string) store.GetValue (iter, 0));
		button_row_play.Click();
	}

	public void on_hbox_combo_button_cancel_clicked (object o, EventArgs args)
	{
		label_treeviewload_row.Sensitive = true;
		button_treeviewload_row_edit.Sensitive = true;
		button_treeviewload_row_play.Sensitive = true;
		button_treeviewload_row_delete.Sensitive = true;
		hbox_edit_row.Hide();
	}

	//this method is only used when try to delete an encoder/forceSensor exercise,
	//and cannot because there are rows done with this exercise.
	//Just unsensitive some stuff now in order to not be able to change them
	public void DeletingExerciseHideSomeWidgets() {
		hbox_spin_int.Hide();
		hbox_entry2.Hide();
		hbox_entry3.Hide();
		hbox_spin_double2.Hide();
		check1.Hide();

		SetButtonAcceptLabel(Catalog.GetString("Close"));
	}	

	public void RemoveSelectedRow () {
		store = UtilGtk.RemoveRow(treeview, store);
	}

	private void on_delete_selected_clicked (object o, EventArgs args)
	{
		ITreeModel model;
		TreeIter iter = new TreeIter();
		if(! treeview.Selection.GetSelected (out model, out iter))
			return;

		TreeviewSelectedUniqueID = Convert.ToInt32((string) store.GetValue (iter, 0));
		label_treeviewload_row.Sensitive = false;
		button_treeviewload_row_edit.Sensitive = false;
		button_treeviewload_row_play.Sensitive = false;
		button_treeviewload_row_delete.Sensitive = false;

		//activate button to manage on gui/encoder.cs in order to delete from SQL
		button_row_delete.Click();
	}

	//if confirmed deletion, this will be called
	public void Delete_row_accepted() {
		//remove selected row from treeview
		store = UtilGtk.RemoveRow(treeview, store);
	}

	//add row to treeview
	public void Row_add_beginning_or_end (string [] row, bool atBeginning)
	{
		if(atBeginning)
			UtilGtk.TreeviewAddRow(treeview, store, row, 0);
		else
			UtilGtk.TreeviewAddRow(treeview, store, row, -1);
	}
	//columnAlphabetical is the column that is going to be checked
	public void Row_add_alphabetical (string [] row, int columnAlphabetical)
	{
		int rowToInsert = 0;
		Gtk.TreeIter iter;
		bool okIter = store.GetIterFirst(out iter);
		if(okIter) {
			do {
				string word = (string) store.GetValue (iter, columnAlphabetical);
				LogB.Information(string.Format("row[columnAphabetical]: {0}, word: {1}, compare: {2}",
							row[columnAlphabetical], word, row[columnAlphabetical].ToLower().CompareTo(word.ToLower()) > 0));
				if(row[columnAlphabetical].ToLower().CompareTo(word.ToLower()) > 0)
					rowToInsert ++;
				else
					break;
			} while ( store.IterNext(ref iter) );
		}

		LogB.Information("Row to insert: " + rowToInsert.ToString());
		UtilGtk.TreeviewAddRow(treeview, store, row, rowToInsert);
	}
	
	public void ShowTextview() {
		scrolled_window_textview.Show();
	}

	public void ShowTreeview() {
		scrolled_window_treeview.Show();
	}

	public void SetButtonAcceptLabel(string str) {
		label_button_accept.Text = str;
	}
	
	public void SetButtonAcceptSensitive(bool show) {
		button_accept.Sensitive=show;
	}
	
	public void SetButtonCancelLabel(string str) {
		label_button_cancel.Text = str;
	}

	public void ShowButtonCancel(bool show) {
		button_cancel.Visible = show;
	}
	public void ShowButtonAccept(bool show) {
		button_accept.Visible = show;
	}

	private void on_button_cancel_clicked (object o, EventArgs args)
	{
		GenericWindowBox.generic_window.Hide();
		GenericWindowBox = null;
	}

	private void on_delete_event (object o, DeleteEventArgs args)
	{
		LogB.Information("calling on_delete_event on genericWindow");

		args.RetVal = true;
			
		GenericWindowBox.generic_window.Hide();
		GenericWindowBox = null;
	}

	private void on_button_accept_clicked (object o, EventArgs args)
	{
		LogB.Information("called on_button_accept_clicked");

		if(HideOnAccept)
			GenericWindowBox.generic_window.Hide();
		if(DestroyOnAccept)
			GenericWindowBox = null;
	}
	
	//when ! HideOnAccept, use this to close window
	//also is better to call it always tat is closed clicking on accept (after data has been readed)
	public void HideAndNull()
	{
		if (GenericWindowBox == null)
			return;

		//this check is extra safety if there are extra EventHandlers opened with +=
		if(GenericWindowBox.generic_window != null)
			GenericWindowBox.generic_window.Hide();

		GenericWindowBox = null;
	}
	
	public bool GenericWindowBoxIsNull() {
		if(GenericWindowBox == null)
			return true;
		return false;
	}
	
	public Button Button_middle {
		get { return button_middle; }
	}
	
	public Button Button_accept {
		set { button_accept = value; }
		get { return button_accept; }
	}
		
	public Button Button_row_edit {
		set { button_row_edit = value; }
		get { return button_row_edit; }
	}

	public Button Button_row_play {
		get { return button_row_play; }
	}

	public Button Button_row_edit_apply {
		set { hbox_combo_button_apply = value; }
		get { return hbox_combo_button_apply; }
	}

	public int VideoColumn {
	       set { videoColumn = value; }
	}
	public int CommentColumn {
		set { commentColumn = value; }
	}

	public void HideEditRowCombo()
	{
		hbox_combo_edit.Visible = false;
		hbox_combo_label.Visible = false;
	}

	public void SetLabelComment(string l) {
		label_comment.Text = l;
	}

	public Button Button_row_delete {
		set { button_row_delete = value; }
		get { return button_row_delete; }
	}
	
	public string EntrySelected {
		set { entry.Text = value; }
		get { return entry.Text.ToString(); }
	}
	
	public string LabelEntry2 {
		set { label_entry2.Text = value; }
	}
	public string Entry2Selected {
		set { entry2.Text = value; }
		get { return entry2.Text.ToString(); }
	}
	
	public string LabelEntry3 {
		set { label_entry3.Text = value; }
	}
	public string Entry3Selected {
		set { entry3.Text = value; }
		get { return entry3.Text.ToString(); }
	}

	public string LabelBeforeTextViewTreeView {
		set { label_before_textview_treeview.Text = value; }
	}

	public string EntryEditRow {
		get { return entry_edit_row.Text.ToString(); }
	}

	public string LabelSpinInt {
		set { label_spin_int.Text = value; }
	}
	public int SpinIntSelected {
		get { return (int) spin_int.Value; }
	}
	
	public string LabelSpinInt2 {
		set { label_spin_int2.Text = value; }
	}
	public int SpinInt2Selected {
		get { return (int) spin_int2.Value; }
	}
	
	public string LabelSpinInt3 {
		set { label_spin_int3.Text = value; }
	}
	public int SpinInt3Selected {
		get { return (int) spin_int3.Value; }
	}
	
	public double SpinDoubleSelected {
		get { return (double) spin_double.Value; }
	}

	public string LabelSpinDouble2 {
		set { label_spin_double2.Text = value; }
	}
	public double SpinDouble2Selected {
		get { return (double) spin_double2.Value; }
	}
	
	public string TwoSpinSelected {
		get { return ((int) spin_feet.Value).ToString() + ":" + ((int) spin_inches.Value).ToString(); }
	}
	
	public string TextviewSelected {
		get { return Util.RemoveTab(textview.Buffer.Text); }
	}

	public string GetComboEditSelected {
		get { return UtilGtk.ComboGetActive (combo_edit); }
	}

	public bool GetCheck1 {
		get { return check1.Active; }
	}

	private void connectWidgets (Gtk.Builder builder)
	{
		generic_window = (Gtk.Window) builder.GetObject ("generic_window");
		label_header = (Gtk.Label) builder.GetObject ("label_header");
		hbox_error = (Gtk.Box) builder.GetObject ("hbox_error");
		label_error = (Gtk.Label) builder.GetObject ("label_error");
		frame_data = (Gtk.Frame) builder.GetObject ("frame_data");
		entry = (Gtk.Entry) builder.GetObject ("entry");

		hbox_spin_int = (Gtk.Box) builder.GetObject ("hbox_spin_int");
		label_spin_int = (Gtk.Label) builder.GetObject ("label_spin_int");
		spin_int = (Gtk.SpinButton) builder.GetObject ("spin_int");
		hbox_spin_int2 = (Gtk.Box) builder.GetObject ("hbox_spin_int2");
		label_spin_int2 = (Gtk.Label) builder.GetObject ("label_spin_int2");
		spin_int2 = (Gtk.SpinButton) builder.GetObject ("spin_int2");
		hbox_spin_int3 = (Gtk.Box) builder.GetObject ("hbox_spin_int3");
		label_spin_int3 = (Gtk.Label) builder.GetObject ("label_spin_int3");
		spin_int3 = (Gtk.SpinButton) builder.GetObject ("spin_int3");

		spin_double = (Gtk.SpinButton) builder.GetObject ("spin_double");
		hbox_height_metric = (Gtk.Box) builder.GetObject ("hbox_height_metric");

		check1 = (Gtk.CheckButton) builder.GetObject ("check1");

		hbuttonbox_middle = (Gtk.HButtonBox) builder.GetObject ("hbuttonbox_middle");
		button_middle = (Gtk.Button) builder.GetObject ("button_middle");

		grid_person_session = (Gtk.Grid) builder.GetObject ("grid_person_session");
		box_combo_person_select = (Gtk.Box) builder.GetObject ("box_combo_person_select");
		box_combo_session_select = (Gtk.Box) builder.GetObject ("box_combo_session_select");

		//Edit row
		hbox_edit_row = (Gtk.Box) builder.GetObject ("hbox_edit_row");
		hbox_combo_label = (Gtk.Label) builder.GetObject ("hbox_combo_label");
		label_comment = (Gtk.Label) builder.GetObject ("label_comment");
		hbox_combo_edit = (Gtk.Box) builder.GetObject ("hbox_combo_edit");
		hbox_combo_button_apply = (Gtk.Button) builder.GetObject ("hbox_combo_button_apply");
		entry_edit_row = (Gtk.Entry) builder.GetObject ("entry_edit_row");

		hbox_all_none_selected = (Gtk.Box) builder.GetObject ("hbox_all_none_selected");
		hbox_combo_all_none_selected = (Gtk.Box) builder.GetObject ("hbox_combo_all_none_selected");
		spin_feet = (Gtk.SpinButton) builder.GetObject ("spin_feet");
		spin_inches = (Gtk.SpinButton) builder.GetObject ("spin_inches");
		label_before_textview_treeview = (Gtk.Label) builder.GetObject ("label_before_textview_treeview");
		scrolled_window_textview = (Gtk.ScrolledWindow) builder.GetObject ("scrolled_window_textview");
		textview = (Gtk.TextView) builder.GetObject ("textview");
		scrolled_window_treeview = (Gtk.ScrolledWindow) builder.GetObject ("scrolled_window_treeview");
		treeview = (Gtk.TreeView) builder.GetObject ("treeview");
		button_accept = (Gtk.Button) builder.GetObject ("button_accept");
		button_cancel = (Gtk.Button) builder.GetObject ("button_cancel");
		image_button_accept = (Gtk.Image) builder.GetObject ("image_button_accept");
		image_button_cancel = (Gtk.Image) builder.GetObject ("image_button_cancel");
		label_button_accept = (Gtk.Label) builder.GetObject ("label_button_accept");
		label_button_cancel = (Gtk.Label) builder.GetObject ("label_button_cancel");

		//treeview fake buttons
		button_row_edit = (Gtk.Button) builder.GetObject ("button_row_edit");
		button_row_play = (Gtk.Button) builder.GetObject ("button_row_play");
		button_row_delete = (Gtk.Button) builder.GetObject ("button_row_delete");

		label_treeviewload_row = (Gtk.Label) builder.GetObject ("label_treeviewload_row");
		button_treeviewload_row_edit = (Gtk.Button) builder.GetObject ("button_treeviewload_row_edit");
		button_treeviewload_row_delete = (Gtk.Button) builder.GetObject ("button_treeviewload_row_delete");
		button_treeviewload_row_play = (Gtk.Button) builder.GetObject ("button_treeviewload_row_play");
		image_treeviewload_row_play = (Gtk.Image) builder.GetObject ("image_treeviewload_row_play");

		hbox_entry2 = (Gtk.Box) builder.GetObject ("hbox_entry2");
		label_entry2 = (Gtk.Label) builder.GetObject ("label_entry2");
		entry2 = (Gtk.Entry) builder.GetObject ("entry2");
		hbox_entry3 = (Gtk.Box) builder.GetObject ("hbox_entry3");
		label_entry3 = (Gtk.Label) builder.GetObject ("label_entry3");
		entry3 = (Gtk.Entry) builder.GetObject ("entry3");

		hbox_spin_double2 = (Gtk.Box) builder.GetObject ("hbox_spin_double2");
		label_spin_double2 = (Gtk.Label) builder.GetObject ("label_spin_double2");
		spin_double2 = (Gtk.SpinButton) builder.GetObject ("spin_double2");
	}

	~GenericWindow() {}
	
}

