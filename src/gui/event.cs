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
using System.Text; //StringBuilder
using System.Collections; //ArrayList
using System.IO;
using System.Threading;
using Mono.Unix;
using LongoMatch.Gui;
using LongoMatch.Video.Player;


//--------------------------------------------------------
//---------------- EDIT EVENT WIDGET ---------------------
//--------------------------------------------------------

public class EditEventWindow 
{
	[Widget] protected Gtk.Window edit_event;
	protected bool weightPercentPreferred;
	[Widget] protected Gtk.Button button_accept;
	[Widget] protected Gtk.Label label_header;
	[Widget] protected Gtk.Label label_type_title;
	[Widget] protected Gtk.Label label_type_value;
	[Widget] protected Gtk.Label label_run_start_title;
	[Widget] protected Gtk.Label label_run_start_value;
	[Widget] protected Gtk.Label label_event_id_value;
	[Widget] protected Gtk.Label label_tv_title;
	[Widget] protected Gtk.Entry entry_tv_value;
	[Widget] protected Gtk.Label label_tv_units;
	[Widget] protected Gtk.Label label_tc_title;
	[Widget] protected Gtk.Entry entry_tc_value;
	[Widget] protected Gtk.Label label_tc_units;
	[Widget] protected Gtk.Label label_fall_title;
	[Widget] protected Gtk.Entry entry_fall_value;
	[Widget] protected Gtk.Label label_fall_units;
	[Widget] protected Gtk.Label label_distance_title;
	[Widget] protected Gtk.Entry entry_distance_value;
	[Widget] protected Gtk.Label label_distance_units;
	[Widget] protected Gtk.Label label_time_title;
	[Widget] protected Gtk.Entry entry_time_value;
	[Widget] protected Gtk.Label label_time_units;
	[Widget] protected Gtk.Label label_speed_title;
	[Widget] protected Gtk.Label label_speed_value;
	[Widget] protected Gtk.Label label_speed_units;
	[Widget] protected Gtk.Label label_weight_title;
	[Widget] protected Gtk.Entry entry_weight_value;
	[Widget] protected Gtk.Label label_weight_units;
	[Widget] protected Gtk.Label label_limited_title;
	[Widget] protected Gtk.Label label_limited_value;
	[Widget] protected Gtk.Label label_angle_title;
	[Widget] protected Gtk.Entry entry_angle_value;
	[Widget] protected Gtk.Label label_angle_units;
	[Widget] protected Gtk.Label label_simulated;
	
	[Widget] protected Gtk.Box hbox_combo_eventType;
	[Widget] protected Gtk.ComboBox combo_eventType;
	[Widget] protected Gtk.Box hbox_combo_person;
	[Widget] protected Gtk.ComboBox combo_persons;
	
	[Widget] protected Gtk.Label label_mistakes;
	[Widget] protected Gtk.SpinButton spin_mistakes;

	[Widget] protected Gtk.Label label_video_yes;
	[Widget] protected Gtk.Label label_video_no;
	[Widget] protected Gtk.Button button_video_watch;
	[Widget] protected Gtk.Button button_video_url;
	protected string videoFileName = "";
	
	[Widget] protected Gtk.Entry entry_description;
	//[Widget] protected Gtk.TextView textview_description;

	static EditEventWindow EditEventWindowBox;
	protected Gtk.Window parent;
	protected int pDN;
	protected bool metersSecondsPreferred;
	protected string type;
	protected string entryTv; //contains a entry that is a Number. If changed the entry as is not a number, recuperate this
	protected string entryTc = "0";
	protected string entryFall = "0"; 
	protected string entryDistance = "0";
	protected string entryTime = "0";
	protected string entrySpeed = "0";
	protected string entryWeight = "0"; //used to record the % for old person if we change it
	protected string entryAngle = "0";

	protected Constants.TestTypes typeOfTest;
	protected bool showType;
	protected bool showRunStart;
	protected bool showTv;
	protected bool showTc;
	protected bool showFall;
	protected bool showDistance;
	protected bool distanceCanBeDecimal;
	protected bool showTime;
	protected bool showSpeed;
	protected bool showWeight;
	protected bool showLimited;
	protected bool showAngle;
	protected bool showMistakes;

	protected string eventBigTypeString = "a test";
	protected bool headerShowDecimal = true;

	protected int oldPersonID; //used to record the % for old person if we change it

	//for inheritance
	protected EditEventWindow () {
	}

	EditEventWindow (Gtk.Window parent) {
		//Glade.XML gladeXML;
		//gladeXML = Glade.XML.FromAssembly (Util.GetGladePath() + "edit_event.glade", "edit_event", null);
		//gladeXML.Autoconnect(this);
		this.parent = parent;
	}

	static public EditEventWindow Show (Gtk.Window parent, Event myEvent, int pDN)
		//run win have also metersSecondsPreferred
	{
		if (EditEventWindowBox == null) {
			EditEventWindowBox = new EditEventWindow (parent);
		}
	
		EditEventWindowBox.pDN = pDN;
		
		EditEventWindowBox.initializeValues();

		EditEventWindowBox.fillDialog (myEvent);

		EditEventWindowBox.edit_event.Show ();

		return EditEventWindowBox;
	}
	
	protected virtual void initializeValues () {
		typeOfTest = Constants.TestTypes.JUMP;
		showType = true;
		showRunStart = false;
		showTv = true;
		showTc = true;
		showFall = true;
		showDistance = true;
		distanceCanBeDecimal = true;
		showTime = true;
		showSpeed = true;
		showWeight = true;
		showLimited = true;
		showAngle = true;
		showMistakes = false;

		label_simulated.Hide();
	}

	protected void fillDialog (Event myEvent)
	{
		fillWindowTitleAndLabelHeader();

		string id = myEvent.UniqueID.ToString();
		if(myEvent.Simulated == Constants.Simulated) 
			label_simulated.Show();
		
		label_event_id_value.Text = id;
		label_event_id_value.UseMarkup = true;

		if(showTv)
			fillTv(myEvent);
		else { 
			label_tv_title.Hide();
			entry_tv_value.Hide();
			label_tv_units.Hide();
		}

		if(showTc)
			fillTc(myEvent);
		else { 
			label_tc_title.Hide();
			entry_tc_value.Hide();
			label_tc_units.Hide();
		}

		if(showFall)
			fillFall(myEvent);
		else { 
			label_fall_title.Hide();
			entry_fall_value.Hide();
			label_fall_units.Hide();
		}

		if(showDistance)
			fillDistance(myEvent);
		else { 
			label_distance_title.Hide();
			entry_distance_value.Hide();
			label_distance_units.Hide();
		}

		if(showTime)
			fillTime(myEvent);
		else { 
			label_time_title.Hide();
			entry_time_value.Hide();
			label_time_units.Hide();
		}

		if(showSpeed)
			fillSpeed(myEvent);
		else { 
			label_speed_title.Hide();
			label_speed_value.Hide();
			label_speed_units.Hide();
		}

		if(showWeight)
			fillWeight(myEvent);
		else { 
			label_weight_title.Hide();
			entry_weight_value.Hide();
			label_weight_units.Hide();
		}

		if(showLimited)
			fillLimited(myEvent);
		else { 
			label_limited_title.Hide();
			label_limited_value.Hide();
		}

		if(showAngle)
			fillAngle(myEvent);
		else { 
			label_angle_title.Hide();
			entry_angle_value.Hide();
			label_angle_units.Hide();
		}
		
		if(! showMistakes) {
			label_mistakes.Hide();
			spin_mistakes.Hide();
		}


		//also remove new line for old descriptions that used a textview
		string temp = Util.RemoveTildeAndColonAndDot(myEvent.Description);
		entry_description.Text = Util.RemoveNewLine(temp, true);

		createComboEventType(myEvent);
		
		if(! showType) {
			label_type_title.Hide();
			combo_eventType.Hide();
		}
		
		if(showRunStart) 
			fillRunStart(myEvent);
		else {
			label_run_start_title.Hide();
			label_run_start_value.Hide();
		}

		ArrayList persons = SqlitePersonSession.SelectCurrentSessionPersons(
				myEvent.SessionID,
				false); //means: do not returnPersonAndPSlist
		string [] personsStrings = new String[persons.Count];
		int i=0;
		foreach (Person person in persons) 
			personsStrings[i++] = person.IDAndName(":");

		combo_persons = ComboBox.NewText();
		UtilGtk.ComboUpdate(combo_persons, personsStrings, "");
		combo_persons.Active = UtilGtk.ComboMakeActive(personsStrings, myEvent.PersonID + ":" + myEvent.PersonName);
		
		oldPersonID = myEvent.PersonID;
			
		hbox_combo_person.PackStart(combo_persons, true, true, 0);
		hbox_combo_person.ShowAll();

		//show video if available	
		videoFileName = Util.GetVideoFileName(myEvent.SessionID, typeOfTest, myEvent.UniqueID);
		if(File.Exists(videoFileName)) {
			label_video_yes.Visible = true;
			label_video_no.Visible = false;
			button_video_watch.Sensitive = true;
			button_video_url.Sensitive = true;
		} else {
			label_video_yes.Visible = false;
			label_video_no.Visible = true;
			button_video_watch.Sensitive = false;
			button_video_url.Sensitive = false;
		}
	}

	private void on_button_video_watch_clicked (object o, EventArgs args) {
		if(File.Exists(videoFileName)) { 
			LogB.Information("Exists and clicked " + videoFileName);

			PlayerBin player = new PlayerBin();
			player.Open(videoFileName);

			Gtk.Window d = new Gtk.Window(Catalog.GetString("Playing video"));
			d.Add(player);
			d.Modal = true;
			d.SetDefaultSize(500,400);
			d.ShowAll();
			d.DeleteEvent += delegate(object sender, DeleteEventArgs e) {player.Close(); player.Dispose();};
			player.Play(); 
		}
	}

	private void on_button_video_url_clicked (object o, EventArgs args) {
		new DialogMessage(Constants.MessageTypes.INFO, 
				Catalog.GetString("Video available here:") + "\n\n" +
				videoFileName);
	}
	
	protected void fillWindowTitleAndLabelHeader() {
		edit_event.Title = string.Format(Catalog.GetString("Edit {0}"), eventBigTypeString);

		System.Globalization.NumberFormatInfo localeInfo = new System.Globalization.NumberFormatInfo();
		localeInfo = System.Globalization.NumberFormatInfo.CurrentInfo;
		label_header.Text = string.Format(Catalog.GetString("Use this window to edit a {0}."), eventBigTypeString);
		if(headerShowDecimal)
			label_header.Text += string.Format(Catalog.GetString("\n(decimal separator: '{0}')"), localeInfo.NumberDecimalSeparator);
	}

		
	protected void createComboEventType(Event myEvent) 
	{
		combo_eventType = ComboBox.NewText ();
		string [] myTypes = findTypes(myEvent);
		UtilGtk.ComboUpdate(combo_eventType, myTypes, "");
		combo_eventType.Active = UtilGtk.ComboMakeActive(myTypes, myEvent.Type);
		hbox_combo_eventType.PackStart(combo_eventType, true, true, 0);
		hbox_combo_eventType.ShowAll();

		createSignal();
	}

	protected virtual string [] findTypes(Event myEvent) {
		string [] myTypes = new String[0];
		return myTypes;
	}

	protected virtual void createSignal() {
		/*
		 * for jumps to show or hide the kg
		 * for runs to put distance depending on it it's fixed or not
		 */
	}

	protected virtual void fillTv(Event myEvent) {
		Jump myJump = (Jump) myEvent;
		entryTv = myJump.Tv.ToString();

		//show all the decimals for not triming there in edit window using
		//(and having different values in formulae like GetHeightInCm ...)
		//entry_tv_value.Text = Util.TrimDecimals(entryTv, pDN);
		entry_tv_value.Text = entryTv;
	}

	protected virtual void fillTc (Event myEvent) {
	}

	protected virtual void fillFall(Event myEvent) {
	}

	protected virtual void fillRunStart(Event myEvent) {
	}

	protected virtual void fillDistance(Event myEvent) {
		/*
		Run myRun = (Run) myEvent;
		entryDistance = myRun.Distance.ToString();
		entry_distance_value.Text = Util.TrimDecimals(entryDistance, pDN);
		*/
	}

	protected virtual void fillTime(Event myEvent) {
		/*
		Run myRun = (Run) myEvent;
		entryTime = myRun.Time.ToString();
		entry_time_value.Text = Util.TrimDecimals(entryTime, pDN);
		*/
	}
	
	protected virtual void fillSpeed(Event myEvent) {
		/*
		Run myRun = (Run) myEvent;
		label_speed_value.Text = Util.TrimDecimals(myRun.Speed.ToString(), pDN);
		*/
	}

	protected virtual void fillWeight(Event myEvent) {
		/*
		Jump myJump = (Jump) myEvent;
		if(myJump.TypeHasWeight) {
			entryWeight = myJump.Weight.ToString();
			entry_weight_value.Text = entryWeight;
			entry_weight_value.Sensitive = true;
		} else {
			entry_weight_value.Sensitive = false;
		}
		*/
	}

	protected virtual void fillLimited(Event myEvent) {
		/*
		JumpRj myJumpRj = (JumpRj) myEvent;
		label_limited_value.Text = Util.GetLimitedRounded(myJumpRj.Limited, pDN);
		*/
	}

	protected virtual void fillAngle(Event myEvent) {
	}
		
	protected virtual void on_radio_single_leg_1_toggled(object o, EventArgs args) {
	}
	protected virtual void on_radio_single_leg_2_toggled(object o, EventArgs args) {
	}
	protected virtual void on_radio_single_leg_3_toggled(object o, EventArgs args) {
	}
	protected virtual void on_radio_single_leg_4_toggled(object o, EventArgs args) {
	}
	protected virtual void on_spin_single_leg_changed(object o, EventArgs args) {
	}


	private void on_entry_tv_value_changed (object o, EventArgs args) {
		if(Util.IsNumber(entry_tv_value.Text.ToString(), true)){
			entryTv = entry_tv_value.Text.ToString();
			button_accept.Sensitive = true;
		} else {
			button_accept.Sensitive = false;
		}
	}
		
	private void on_entry_tc_value_changed (object o, EventArgs args) {
		if(Util.IsNumber(entry_tc_value.Text.ToString(), true)){
			entryTc = entry_tc_value.Text.ToString();
			button_accept.Sensitive = true;
		} else {
			button_accept.Sensitive = false;
			//entry_tc_value.Text = "";
			//entry_tc_value.Text = entryTc;
		}
	}
		
	private void on_entry_fall_value_changed (object o, EventArgs args) {
		if(Util.IsNumber(entry_fall_value.Text.ToString(), true)){
			entryFall = entry_fall_value.Text.ToString();
			button_accept.Sensitive = true;
		} else {
			button_accept.Sensitive = false;
			//entry_fall_value.Text = "";
			//entry_fall_value.Text = entryFall;
		}
	}
		
	private void on_entry_time_changed (object o, EventArgs args) {
		if(Util.IsNumber(entry_time_value.Text.ToString(), true)){
			entryTime = entry_time_value.Text.ToString();
			label_speed_value.Text = Util.TrimDecimals(
					Util.GetSpeed (entryDistance, entryTime, metersSecondsPreferred) , pDN);
			button_accept.Sensitive = true;
		} else {
			button_accept.Sensitive = false;
			//entry_time_value.Text = "";
			//entry_time_value.Text = entryTime;
		}
	}
	
	private void on_entry_distance_changed (object o, EventArgs args) {
		if(Util.IsNumber(entry_distance_value.Text.ToString(), distanceCanBeDecimal)){
			entryDistance = entry_distance_value.Text.ToString();
			label_speed_value.Text = Util.TrimDecimals(
					Util.GetSpeed (entryDistance, entryTime, metersSecondsPreferred) , pDN);
			button_accept.Sensitive = true;
		} else {
			button_accept.Sensitive = false;
			//entry_distance_value.Text = "";
			//entry_distance_value.Text = entryDistance;
		}
	}
		
	private void on_entry_weight_value_changed (object o, EventArgs args) {
		if(Util.IsNumber(entry_weight_value.Text.ToString(), true)){
			entryWeight = entry_weight_value.Text.ToString();
			button_accept.Sensitive = true;
		} else {
			button_accept.Sensitive = false;
			//entry_weight_value.Text = "";
			//entry_weight_value.Text = entryWeight;
		}
	}
		
	private void on_entry_angle_changed (object o, EventArgs args) {
		string angleString = entry_angle_value.Text.ToString();
		if(Util.IsNumber(angleString, true)) {
			entryAngle = angleString;
			button_accept.Sensitive = true;
		} else if(angleString == "-") {
			entryAngle = "-1,0";
			button_accept.Sensitive = true;
		} else 
			button_accept.Sensitive = false;
	}
	
	protected virtual void on_spin_mistakes_changed (object o, EventArgs args) {
	}
		
		
	private void on_entry_description_changed (object o, EventArgs args) {
		entry_description.Text = Util.RemoveTildeAndColonAndDot(entry_description.Text.ToString());
	}
	
	protected virtual void on_radio_mtgug_1_toggled(object o, EventArgs args) { }
	protected virtual void on_radio_mtgug_2_toggled(object o, EventArgs args) { }
	protected virtual void on_radio_mtgug_3_toggled(object o, EventArgs args) { }
	protected virtual void on_radio_mtgug_4_toggled(object o, EventArgs args) { }
	protected virtual void on_radio_mtgug_5_toggled(object o, EventArgs args) { }
	protected virtual void on_radio_mtgug_6_toggled(object o, EventArgs args) { }
	
	protected virtual void on_button_cancel_clicked (object o, EventArgs args)
	{
		EditEventWindowBox.edit_event.Hide();
		EditEventWindowBox = null;
	}
	
	protected virtual void on_delete_event (object o, DeleteEventArgs args)
	{
		EditEventWindowBox.edit_event.Hide();
		EditEventWindowBox = null;
	}

	protected virtual void hideWindow() {
		EditEventWindowBox.edit_event.Hide();
		EditEventWindowBox = null;
	}

	void on_button_accept_clicked (object o, EventArgs args)
	{
		int eventID = Convert.ToInt32 ( label_event_id_value.Text );
		string myPerson = UtilGtk.ComboGetActive(combo_persons);
		string [] myPersonFull = myPerson.Split(new char[] {':'});
		
		string myDesc = entry_description.Text;
		

		updateEvent(eventID, Convert.ToInt32(myPersonFull[0]), myDesc);

		hideWindow();
	}

	protected virtual void updateEvent(int eventID, int personID, string description) {
	}

	public Button Button_accept 
	{
		set { button_accept = value;	}
		get { return button_accept;	}
	}

	~EditEventWindow() {}
}


//--------------------------------------------------------
//---------------- event_more widget ---------------------
//--------------------------------------------------------

public class EventMoreWindow 
{
	protected TreeStore store;
	[Widget] protected Gtk.TreeView treeview_more;
	[Widget] protected Gtk.Button button_accept;
	[Widget] protected Gtk.Button button_delete_type;
	[Widget] protected Gtk.Button button_cancel;
	[Widget] protected Gtk.Button button_close;
	protected Gtk.Window parent;

	protected string selectedEventType;
	protected string selectedEventName;
	protected string selectedDescription;
	public Gtk.Button button_selected;
	public Gtk.Button button_deleted_test; //just to send a signal
	
	protected bool testOrDelete; //are we going to do a test or to delete a test type (test is true)
	protected string [] typesTranslated;

	public EventMoreWindow () {
	}

	public EventMoreWindow (Gtk.Window parent, bool testOrDelete) {
		/*
		Glade.XML gladeXML;
		gladeXML = Glade.XML.FromAssembly (Util.GetGladePath() + "jumps_runs_more.glade", "jumps_runs_more", "chronojump");
		gladeXML.Autoconnect(this);
		*/

		//name, startIn, weight, description
		store = new TreeStore(typeof (string), typeof (string), typeof (string), typeof (string));

		initializeThings();
	}

	protected void initializeThings() 
	{
		button_selected = new Gtk.Button();
		button_deleted_test = new Gtk.Button();
		
		createTreeView(treeview_more);

		treeview_more.Model = store;
		fillTreeView(treeview_more,store);

		//when executing test: show accept and cancel
		button_accept.Visible = testOrDelete;
		button_cancel.Visible = testOrDelete;
		//when deleting test type: show delete type and close
		button_delete_type.Visible = ! testOrDelete;
		button_close.Visible = ! testOrDelete;

		button_accept.Sensitive = false;
		button_delete_type.Sensitive = false;
		 
		treeview_more.Selection.Changed += onSelectionEntry;
	}


	//if eventType is predefined, it will have a translation on src/evenType or derivated class
	//this is useful if user changed language
	protected string getDescriptionLocalised(EventType myType, string descriptionFromDb) {
	if(myType.IsPredefined)
		return myType.Description;
	else
		return descriptionFromDb;
	}


	protected virtual void createTreeView (Gtk.TreeView tv) {
	}
	
	protected virtual void fillTreeView (Gtk.TreeView tv, TreeStore store) 
	{
	}

	/*
	 * when a row is selected...
	 * -put selected value in selected* variables
	 * -update graph image test on main window
	 */
	protected virtual void onSelectionEntry (object o, EventArgs args)
	{
	}
	
	protected virtual void on_row_double_clicked (object o, Gtk.RowActivatedArgs args)
	{
	}
	
	void on_button_delete_type_clicked (object o, EventArgs args)
	{
		string [] tests = findTestTypesInSessions();

		//this will be much better doing a select distinct(session) instead of using SelectJumps or Runs
		ArrayList sessionValues = new ArrayList();
		foreach(string t in tests) {
			string [] tFull = t.Split(new char[] {':'});
			Util.AddToArrayListIfNotExist(sessionValues, tFull[3]);
		}

		//if exist tell user to edit or delete them
		if(tests.Length > 0) 
			new DialogMessage(Constants.MessageTypes.WARNING, 
					Catalog.GetString("There are tests of that type on database on sessions:") + "\n" +
					Util.ArrayListToSingleString(sessionValues, ", ") + "\n\n" +
					Catalog.GetString("please first edit or delete them."));
		else {
			ConfirmWindow confirmWin = ConfirmWindow.Show(Catalog.GetString("Are you sure you want to delete this test type?"), "", selectedEventName);
			confirmWin.Button_accept.Clicked += new EventHandler(on_button_delete_type_accepted);
		}
	}
	

	protected virtual void deleteTestLine() {
	}
	
	protected void on_button_delete_type_accepted (object o, EventArgs args)
	{
		deleteTestLine();

		button_deleted_test.Click();

		TreeModel model;
		TreeIter iter;
		if (treeview_more.Selection.GetSelected (out model, out iter)) 
			store.Remove(ref iter);

		button_delete_type.Sensitive = false;
	}

	///this should be abstract
	protected virtual string [] findTestTypesInSessions() {
		string [] nothing = new String[0];
		return nothing;
	}
	
	//fired when something is selected for drawing on imageTest
	public Button Button_selected
	{
		get { return button_selected; }
	}

	public Button Button_deleted_test
	{
		get { return button_deleted_test; }
	}

	public Button Button_accept {
		set { button_accept = value; }
		get { return button_accept; }
	}
	
	public Button Button_cancel {
		set { button_cancel = value; }
		get { return button_cancel; }
	}
	
	public string SelectedEventName
	{
		set { selectedEventName = value; }
		get { return selectedEventName; }
	}
	
	public string SelectedDescription {
		get { return selectedDescription; }
	}
}
