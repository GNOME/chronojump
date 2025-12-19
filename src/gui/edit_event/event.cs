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
 * Copyright (C) 2004-2025   Xavier de Blas <xaviblas@gmail.com>
 */

using System;
using Gdk;
using Gtk;
//using Glade;
using System.Text; //StringBuilder
using System.Collections; //ArrayList
using System.Collections.Generic; //List<>
using System.IO;
using System.Threading;
using Mono.Unix;


public class EditEventWindow 
{
	// at glade ---->
	protected Gtk.Window edit_event;
	protected Gtk.Button button_accept;
	protected Gtk.Button fake_button_finished; // gui/app1/chronojump.cs will process AFTER button_accept
	protected Gtk.Label label_header;
	protected Gtk.Frame frame;
	//protected Gtk.Grid grid;
	protected Gtk.Label label_type_title;
	protected Gtk.Label label_type_value;
	protected Gtk.Label label_run_start_title;
	protected Gtk.Label label_run_start_value;
	protected Gtk.Label label_event_id_value;
	protected Gtk.Label label_jump_tv_title;
	protected Gtk.Entry entry_jump_tv_value;
	protected Gtk.Label label_jump_tv_units;
	protected Gtk.Label label_jump_tc_title;
	protected Gtk.Entry entry_jump_tc_value;
	protected Gtk.Label label_jump_tc_units;
	protected Gtk.Label label_jump_fall_title;
	protected Gtk.Entry entry_jump_fall_value;
	protected Gtk.Label label_jump_fall_units;
	protected Gtk.Label label_distance_title;
	protected Gtk.Entry entry_distance_value;
	protected Gtk.Label label_distance_units;
	protected Gtk.Label label_time_title;
	protected Gtk.Entry entry_time_value;
	protected Gtk.Label label_time_units;
	protected Gtk.Label label_speed_title;
	protected Gtk.Label label_speed_value;
	protected Gtk.Label label_speed_units;
	protected Gtk.Label label_weight_title;
	protected Gtk.Entry entry_weight_value;
	protected Gtk.Label label_weight_units;
	protected Gtk.Label label_limited_title;
	protected Gtk.Label label_limited_value;
	//protected Gtk.Label label_angle_title; //kneeAngle
	//protected Gtk.Entry entry_angle_value; //kneeAngle
	//protected Gtk.Label label_angle_units; //kneeAngle
	protected Gtk.Label label_simulated;

	protected Gtk.Box box_exercise_filter;
	protected Gtk.Entry entry_exercise_filter;
	protected Gtk.Image image_exercise_filter;
	protected Gtk.Box hbox_combo_eventType;
	protected Gtk.Box hbox_combo_person;
	
	protected Gtk.Label label_mistakes;
	protected Gtk.SpinButton spin_mistakes;
	protected Gtk.Label label_date;
	protected Gtk.Label label_date_value;

	// force sensor
	protected Gtk.Label label_forceSensor_capture;
	protected Gtk.Box box_forceSensor_capture;
	protected Gtk.Label label_laterality;
	protected Gtk.Box box_laterality;
	protected Gtk.RadioButton radio_forceSensor_capture_standard;
	protected Gtk.RadioButton radio_forceSensor_capture_absolute;
	protected Gtk.RadioButton radio_forceSensor_capture_inverted;
	protected Gtk.Image image_forceSensor_capture_standard;
	protected Gtk.Image image_forceSensor_capture_absolute;
	protected Gtk.Image image_forceSensor_capture_inverted;
	protected Gtk.RadioButton radio_laterality_both;
	protected Gtk.RadioButton radio_laterality_left;
	protected Gtk.RadioButton radio_laterality_right;
	protected Gtk.Image image_laterality_both;
	protected Gtk.Image image_laterality_left;
	protected Gtk.Image image_laterality_right;

	// raceAnalyzer
	// entry_distance_value (and title, units) (3 already declared above)
	protected Gtk.Label label_race_analyzer_distance;
	protected Gtk.SpinButton spin_race_analyzer_distance;
	protected Gtk.Label label_race_analyzer_distance_units;
	protected Gtk.Label label_race_analyzer_angle;
	protected Gtk.SpinButton spin_race_analyzer_angle;
	protected Gtk.Label label_race_analyzer_angle_units;
	protected Gtk.Label label_race_analyzer_temperature;
	protected Gtk.SpinButton spin_race_analyzer_temperature;
	protected Gtk.Label label_race_analyzer_temperature_units;

	// encoder 
	// laterality widgets (already declared on forceSensor)
	protected Gtk.Label label_encoder_exercise;
	protected Gtk.Button button_encoder_select;
	protected Gtk.Image image_encoder_configuration;
	protected Gtk.Box box_encoder_selected;
	protected Gtk.Image image_encoder_selected_type;
	protected Gtk.Label label_encoder_selected;
	protected Gtk.Label label_encoder_eccon_title;
	protected Gtk.Box box_encoder_eccon;
	protected Gtk.RadioButton radio_encoder_eccon_concentric;
	protected Gtk.RadioButton radio_encoder_eccon_eccentric_concentric;
	protected Gtk.Image image_encoder_eccon_concentric;
	protected Gtk.Image image_encoder_eccon_eccentric_concentric;
	protected Gtk.Label label_encoder_ecc_con_alert;
	// encoder mass-inertia
	protected Gtk.Label label_encoder_exercise_mass;
	protected Gtk.Label label_encoder_exercise_inertia;
	protected Gtk.HBox hbox_encoder_exercise_mass;
	protected Gtk.Box box_encoder_exercise_inertia;
	protected Gtk.Image image_extra_mass;
	protected Gtk.SpinButton spin_encoder_extra_weight;
	protected Gtk.Label label_encoder_displaced_weight;
	protected Gtk.HBox hbox_combo_encoder_anchorage;
	protected Gtk.Image image_encoder_inertial_weights;
	protected Gtk.SpinButton spin_encoder_im_weights_n;
	protected Gtk.Label label_encoder_im_total;
	protected Gtk.Label label_encoder_equivalent_mass;

	protected Gtk.Label label_encoder_rep_length;
	protected Gtk.VBox vbox_encoder_rep_length;
	protected Gtk.SpinButton spin_encoder_rep_min_height_gravitatory;
	protected Gtk.SpinButton spin_encoder_rep_min_height_inertial;
	protected Gtk.Label label_encoder_rep_length_units;

	private Gtk.Box hbox_video;
	private Gtk.Label label_video;
	protected Gtk.Label label_video_yes_no;
	protected Gtk.Button button_video_watch;
	protected Gtk.Image image_video_watch;
	protected Gtk.Button button_video_url;
	private Gtk.Label label_description;
	protected Gtk.Entry entry_description;
	//protected Gtk.TextView textview_description;
	// <---- at glade

	protected Gtk.ComboBoxText combo_eventType;
	protected Gtk.ComboBoxText combo_persons;

	protected string videoFileName = "";
	protected bool weightPercentPreferred;

	static EditEventWindow EditEventWindowBox;
	protected Gtk.Window parent;
	protected int pDN;
	protected bool metersSecondsPreferred;
	protected string type;
	protected string entryJumpTv; //contains a entry that is a Number. If changed the entry as is not a number, recuperate this
	protected string entryJumpTc = "0";
	protected string entryJumpFall = "0"; 
	protected string entryDistance = "0";
	protected string entryTime = "0";
	protected string entrySpeed = "0";
	protected string entryWeight = "0"; //used to record the % for old person if we change it
	//protected string entryAngle = "0"; //kneeAngle

	protected Constants.TestTypes typeOfTest;
	protected bool showType;
	protected bool showRunStart;
	protected bool showJumpTv;
	protected bool showJumpTc;
	protected bool showJumpFall;
	protected bool showRunDistance;
	protected bool distanceCanBeDecimal;
	protected bool showTime;
	protected bool showSpeed;
	protected bool showWeight;
	protected bool showLimited;
	//protected bool showAngle; //kneeAngle
	protected bool showVideo;
	protected bool showMistakes;
	protected bool showDescription;

	protected bool combo_person_has_signal;
	protected bool combo_exercise_has_signal;

	protected string eventBigTypeString = "a test";
	protected bool headerShowDecimal = true;

	protected int oldPersonID; //used to record the % for old person if we change it
	private List<Person> person_l;
	private List<PersonSession> personSession_l;

	//to know if changed or not in order to redo the treeview
	//public double distanceAtInit;

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

	protected void colorize ()
	{
		if(! Config.UseSystemColor)
		{
			UtilGtk.WindowColor (edit_event, Config.ColorBackground);
			UtilGtk.ContrastLabelsWidget (Config.ColorBackgroundIsDark, edit_event);

			UtilGtk.WidgetColor (frame, Config.ColorBackgroundShifted);
			UtilGtk.ContrastLabelsFrame (Config.ColorBackgroundShiftedIsDark, frame);
		}
	}

	protected void initializeValues ()
	{
		//create fake_button_finished
	 	fake_button_finished = new Gtk.Button ();

		// default options false for all modes
		typeOfTest = Constants.TestTypes.JUMP; //whatever
		showJumpTv = false;
		showJumpTc = false;
		showJumpFall = false;

		showRunDistance = false;
		distanceCanBeDecimal = true;
		showTime = false;
		showSpeed = false;
		showWeight = false;
		showLimited = false;
		showMistakes = false;
		showVideo = false;
		showDescription = false;

		combo_person_has_signal = false;
		combo_exercise_has_signal = false;

		// specific options (true) for each mode
		initializeSpecific (); //assign the true values of each mode
	}
	protected virtual void initializeSpecific ()
	{
	}

	protected void fillDialog (Event myEvent)
	{
		fillWindowTitleAndLabelHeader();

		image_video_watch.Pixbuf = Chronojump.MyPixbuf.Get(null, Util.GetImagePath(false) + "video_play.png");

		string id = myEvent.UniqueID.ToString();
		if(myEvent.Simulated == Constants.Simulated) 
			label_simulated.Show();
		
		label_event_id_value.Text = id;
		label_event_id_value.UseMarkup = true;

		if(showJumpTv)
			fillTv(myEvent);
		else { 
			label_jump_tv_title.Hide();
			entry_jump_tv_value.Hide();
			label_jump_tv_units.Hide();
		}

		if(showJumpTc)
			fillTc(myEvent);
		else { 
			label_jump_tc_title.Hide();
			entry_jump_tc_value.Hide();
			label_jump_tc_units.Hide();
		}

		if(showJumpFall)
			fillFall(myEvent);
		else { 
			label_jump_fall_title.Hide();
			entry_jump_fall_value.Hide();
			label_jump_fall_units.Hide();
		}

		if (showRunDistance)
		{
			fillRunDistance (myEvent);

			label_distance_title.Visible = true;
			entry_distance_value.Visible = true;
			label_distance_units.Visible = true;
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

		/*
		if(showAngle)
			fillAngle(myEvent);
		else { 
			label_angle_title.Hide();
			entry_angle_value.Hide();
			label_angle_units.Hide();
		}
		*/

		if(! showMistakes) {
			label_mistakes.Hide();
			spin_mistakes.Hide();
		}

		label_date.Visible = true;
		label_date_value.Visible = true;

		if (showDescription) {
			//also remove new line for old descriptions that used a textview
			string temp = Util.RemoveTildeAndColonAndDot(myEvent.Description);
			entry_description.Text = Util.RemoveNewLine(temp, true);
			label_description.Show();
			entry_description.Show();
		} else {
			label_description.Hide();
			entry_description.Hide();
		}

		createComboEventType (myEvent);

		box_exercise_filter.Visible = false;
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

		person_l = SqlitePersonSession.SelectCurrentSessionPersonsAsList (false, myEvent.SessionID);
		createComboPersons (myEvent);
		oldPersonID = myEvent.PersonID;

		// used on encoder to know total weight when body mass is displaced
		personSession_l = SqlitePersonSession.SelectPersonSessionList (false, -1, myEvent.SessionID);

		fillDialogSpecific (myEvent);

		if (showVideo)
		{
			//show video if available	
			videoFileName = Util.GetVideoFileName(myEvent.SessionID, typeOfTest, myEvent.UniqueID);
			if(File.Exists(videoFileName)) {
				label_video_yes_no.Text = Catalog.GetString("Yes");
				button_video_watch.Sensitive = true;
				button_video_url.Sensitive = true;
			} else {
				label_video_yes_no.Text = Catalog.GetString("No");
				button_video_watch.Sensitive = false;
				button_video_url.Sensitive = false;
			}

			label_video.Show ();
			hbox_video.Show ();
		} else {
			label_video.Hide ();
			hbox_video.Hide ();
		}
	}

	protected virtual void fillDialogSpecific (Event myEvent)
	{
	}

	protected void createLateralityIcons ()
	{
		image_laterality_both.Pixbuf = Chronojump.MyPixbuf.Get(null, Util.GetImagePath(false) + "laterality-both.png");
		image_laterality_left.Pixbuf = Chronojump.MyPixbuf.Get(null, Util.GetImagePath(false) + "laterality-left.png");
		image_laterality_right.Pixbuf = Chronojump.MyPixbuf.Get(null, Util.GetImagePath(false) + "laterality-right.png");
	}

	private void on_button_video_watch_clicked (object o, EventArgs args)
	{
		if(File.Exists(videoFileName))
		{
			LogB.Information("Exists and clicked " + videoFileName);

			/*
			 * using mplayer
			 *
			 * Webcam webcam = new WebcamMplayer ();
			 * Webcam.Result result = webcam.PlayFile(videoFileName);
			 */

			//using ffmpeg
			Webcam webcam = new WebcamFfmpeg (Webcam.Action.PLAYFILE, UtilAll.GetOSEnum(), "", "", "", "");
			//Webcam.Result result = webcam.PlayFile (videoFileName);
			webcam.PlayFile (videoFileName);
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

	protected void createComboPersons (Event myEvent)
	{
		string [] personsStrings = new String[person_l.Count];
		int i=0;
		foreach (Person person in person_l)
			personsStrings[i++] = person.Name;

		combo_persons = new ComboBoxText();
		UtilGtk.ComboUpdate (combo_persons, personsStrings, "");
		foreach (Person person in person_l)
			if (person.UniqueID == myEvent.PersonID)
				combo_persons.Active = UtilGtk.ComboMakeActive (personsStrings, person.Name);

		hbox_combo_person.PackStart(combo_persons, true, true, 0);
		hbox_combo_person.ShowAll();
		combo_person_createSignalIfNeeded ();
	}

	protected void createComboEventType (Event myEvent)
	{
		combo_eventType = new ComboBoxText ();
		string [] myTypes = findTypes (myEvent);
		UtilGtk.ComboUpdate (combo_eventType, myTypes, "");
		// LogB.Information ("createComboEventType myTypes: " + Util.StringArrayToString (myTypes, ", "));
		// LogB.Information ("myEvent: " + myEvent.ToString ());
		// LogB.Information ("myEvent.Type: " + myEvent.Type.ToString ());
		combo_eventType.Active = UtilGtk.ComboMakeActive (myTypes, myEvent.Type);
		hbox_combo_eventType.PackStart (combo_eventType, true, true, 0);
		hbox_combo_eventType.ShowAll ();

		combo_EventType_createSignalIfNeeded ();
	}

	private void combo_person_createSignalIfNeeded ()
	{
		if (! combo_person_has_signal)
			return;

		combo_persons.Changed -= new EventHandler (on_combo_persons_changed);
		combo_persons.Changed += new EventHandler (on_combo_persons_changed);
	}
	protected virtual void on_combo_persons_changed (object o, EventArgs args)
	{
		// only implemented in Modes: POWERGRAVITATORY
	}


	protected virtual void on_exercise_filter_changed (object o, EventArgs args)
	{
		// only implemented in Force Sensor modes: ISOMETRIC, ELASTIC
	}

	private void combo_EventType_createSignalIfNeeded ()
	{
		if (! combo_exercise_has_signal)
			return;

		combo_eventType.Changed -= new EventHandler (on_combo_eventType_changed);
		combo_eventType.Changed += new EventHandler (on_combo_eventType_changed);
	}
	protected virtual void on_combo_eventType_changed (object o, EventArgs args)
	{
		// only implemented in Modes: JUMPSSIMPLE, RUNSSIMPLE, POWERGRAVITATORY
	}

	protected virtual void on_spin_encoder_extra_weight_value_changed (object o, EventArgs args)
	{
		// only implemented in Modes: POWERGRAVITATORY
	}
	protected virtual void on_spin_encoder_im_weights_n_value_changed (object o, EventArgs args)
	{
		// only implemented in Modes: POWERINERTIAL
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
		entryJumpTv = myJump.Tv.ToString();

		//show all the decimals for not triming there in edit window using
		//(and having different values in formulae like GetHeightInCm ...)
		//entry_jump_tv_value.Text = Util.TrimDecimals(entryJumpTv, pDN);
		entry_jump_tv_value.Text = entryJumpTv;
	}

	protected virtual void fillTc (Event myEvent) {
	}

	protected virtual void fillFall(Event myEvent) {
	}

	protected virtual void fillRunStart(Event myEvent) {
	}

	protected virtual void fillRunDistance(Event myEvent) {
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

	//protected virtual void fillAngle(Event myEvent) {
	//}
		
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


	private void on_entry_jump_tv_value_changed (object o, EventArgs args) {
		if(Util.IsNumber(entry_jump_tv_value.Text.ToString(), true)){
			entryJumpTv = entry_jump_tv_value.Text.ToString();
			button_accept.Sensitive = true;
		} else {
			button_accept.Sensitive = false;
		}
	}
		
	private void on_entry_jump_tc_value_changed (object o, EventArgs args) {
		if(Util.IsNumber(entry_jump_tc_value.Text.ToString(), true)){
			entryJumpTc = entry_jump_tc_value.Text.ToString();
			button_accept.Sensitive = true;
		} else {
			button_accept.Sensitive = false;
			//entry_jump_tc_value.Text = "";
			//entry_jump_tc_value.Text = entryJumpTc;
		}
	}
		
	private void on_entry_jump_fall_value_changed (object o, EventArgs args) {
		if(Util.IsNumber(entry_jump_fall_value.Text.ToString(), true)){
			entryJumpFall = entry_jump_fall_value.Text.ToString();
			button_accept.Sensitive = true;
		} else {
			button_accept.Sensitive = false;
			//entry_jump_fall_value.Text = "";
			//entry_jump_fall_value.Text = entryJumpFall;
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
	
	protected virtual void on_entry_distance_changed (object o, EventArgs args)
	{
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

	/*
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
	*/

	protected virtual void on_spin_mistakes_changed (object o, EventArgs args) {
	}
		
	protected virtual void on_button_encoder_select_clicked (object o, EventArgs args)
	{
		// defined on encoder
	}
	protected virtual void on_encoder_configuration_win_closed (object o, EventArgs args)
	{
		// defined on encoder
	}
	protected virtual void on_radio_encoder_eccon_concentric_toggled (object o, EventArgs args)
	{
		// defined on encoder
	}
	protected virtual void on_radio_encoder_eccon_eccentric_concentric_toggled (object o, EventArgs args)
	{
		// defined on encoder
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
		int personID = getPersonIDFromCombo ();

		if (personID >= 0)
			updateSQL (Convert.ToInt32 (label_event_id_value.Text),
					personID, entry_description.Text);

		fake_button_finished.Click ();

		hideWindow();
	}

	protected int getPersonIDFromCombo ()
	{
		string personName = UtilGtk.ComboGetActive (combo_persons);
		foreach (Person person in person_l)
			if (person.Name == personName)
				return person.UniqueID;

		return -1;
	}

	protected double getPersonWeight (int personID)
	{
		foreach (PersonSession ps in personSession_l)
			if (ps.PersonID == personID)
				return ps.Weight;

		return 0;
	}

	protected virtual void updateSQL(int eventID, int personID, string description) {
	}

	public Button Button_accept 
	{
		set { button_accept = value;	}
		get { return button_accept;	}
	}
	public Button Fake_button_finished
	{
		get { return fake_button_finished;	}
	}

	/*
	   unused
	public bool DistanceChanged
	{
		get {
			LogB.Information ("entry_distance_value: " + entry_distance_value.Text);
			if (distanceAtInit != 0 && distanceAtInit != Convert.ToDouble(entry_distance_value.Text))
				return true;
			return false;
		}
	}
	*/

	protected void connectWidgetsEditEvent (Gtk.Builder builder)
	{
		edit_event = (Gtk.Window) builder.GetObject ("edit_event");
		button_accept = (Gtk.Button) builder.GetObject ("button_accept");
		label_header = (Gtk.Label) builder.GetObject ("label_header");
		frame = (Gtk.Frame) builder.GetObject ("frame");
		//grid = (Gtk.Grid) builder.GetObject ("grid");
		label_type_title = (Gtk.Label) builder.GetObject ("label_type_title");
		label_type_value = (Gtk.Label) builder.GetObject ("label_type_value");
		label_run_start_title = (Gtk.Label) builder.GetObject ("label_run_start_title");
		label_run_start_value = (Gtk.Label) builder.GetObject ("label_run_start_value");
		label_event_id_value = (Gtk.Label) builder.GetObject ("label_event_id_value");
		label_jump_tv_title = (Gtk.Label) builder.GetObject ("label_jump_tv_title");
		entry_jump_tv_value = (Gtk.Entry) builder.GetObject ("entry_jump_tv_value");
		label_jump_tv_units = (Gtk.Label) builder.GetObject ("label_jump_tv_units");
		label_jump_tc_title = (Gtk.Label) builder.GetObject ("label_jump_tc_title");
		entry_jump_tc_value = (Gtk.Entry) builder.GetObject ("entry_jump_tc_value");
		label_jump_tc_units = (Gtk.Label) builder.GetObject ("label_jump_tc_units");
		label_jump_fall_title = (Gtk.Label) builder.GetObject ("label_jump_fall_title");
		entry_jump_fall_value = (Gtk.Entry) builder.GetObject ("entry_jump_fall_value");
		label_jump_fall_units = (Gtk.Label) builder.GetObject ("label_jump_fall_units");
		label_distance_title = (Gtk.Label) builder.GetObject ("label_distance_title");
		entry_distance_value = (Gtk.Entry) builder.GetObject ("entry_distance_value");
		label_distance_units = (Gtk.Label) builder.GetObject ("label_distance_units");
		label_time_title = (Gtk.Label) builder.GetObject ("label_time_title");
		entry_time_value = (Gtk.Entry) builder.GetObject ("entry_time_value");
		label_time_units = (Gtk.Label) builder.GetObject ("label_time_units");
		label_speed_title = (Gtk.Label) builder.GetObject ("label_speed_title");
		label_speed_value = (Gtk.Label) builder.GetObject ("label_speed_value");
		label_speed_units = (Gtk.Label) builder.GetObject ("label_speed_units");
		label_weight_title = (Gtk.Label) builder.GetObject ("label_weight_title");
		entry_weight_value = (Gtk.Entry) builder.GetObject ("entry_weight_value");
		label_weight_units = (Gtk.Label) builder.GetObject ("label_weight_units");
		label_limited_title = (Gtk.Label) builder.GetObject ("label_limited_title");
		label_limited_value = (Gtk.Label) builder.GetObject ("label_limited_value");
		// label_angle_title = (Gtk.Label) builder.GetObject ("label_angle_title"); //kneeAngle
		// entry_angle_value = (Gtk.Entry) builder.GetObject ("entry_angle_value"); //kneeAngle
		// label_angle_units = (Gtk.Label) builder.GetObject ("label_angle_units"); //kneeAngle
		label_simulated = (Gtk.Label) builder.GetObject ("label_simulated");

		box_exercise_filter = (Gtk.Box) builder.GetObject ("box_exercise_filter");
		entry_exercise_filter = (Gtk.Entry) builder.GetObject ("entry_exercise_filter");
		image_exercise_filter = (Gtk.Image) builder.GetObject ("image_exercise_filter");
		hbox_combo_eventType = (Gtk.Box) builder.GetObject ("hbox_combo_eventType");
		hbox_combo_person = (Gtk.Box) builder.GetObject ("hbox_combo_person");

		label_mistakes = (Gtk.Label) builder.GetObject ("label_mistakes");
		spin_mistakes = (Gtk.SpinButton) builder.GetObject ("spin_mistakes");
		label_date = (Gtk.Label) builder.GetObject ("label_date");
		label_date_value = (Gtk.Label) builder.GetObject ("label_date_value");

		label_forceSensor_capture = (Gtk.Label) builder.GetObject ("label_forceSensor_capture");
		box_forceSensor_capture = (Gtk.Box) builder.GetObject ("box_forceSensor_capture");
		label_laterality = (Gtk.Label) builder.GetObject ("label_laterality");
		box_laterality = (Gtk.Box) builder.GetObject ("box_laterality");
		radio_forceSensor_capture_standard = (Gtk.RadioButton) builder.GetObject ("radio_forceSensor_capture_standard");
		radio_forceSensor_capture_absolute = (Gtk.RadioButton) builder.GetObject ("radio_forceSensor_capture_absolute");
		radio_forceSensor_capture_inverted = (Gtk.RadioButton) builder.GetObject ("radio_forceSensor_capture_inverted");
		image_forceSensor_capture_standard = (Gtk.Image) builder.GetObject ("image_forceSensor_capture_standard");
		image_forceSensor_capture_absolute = (Gtk.Image) builder.GetObject ("image_forceSensor_capture_absolute");
		image_forceSensor_capture_inverted = (Gtk.Image) builder.GetObject ("image_forceSensor_capture_inverted");
		radio_laterality_both = (Gtk.RadioButton) builder.GetObject ("radio_laterality_both");
		radio_laterality_left = (Gtk.RadioButton) builder.GetObject ("radio_laterality_left");
		radio_laterality_right = (Gtk.RadioButton) builder.GetObject ("radio_laterality_right");
		image_laterality_both = (Gtk.Image) builder.GetObject ("image_laterality_both");
		image_laterality_left = (Gtk.Image) builder.GetObject ("image_laterality_left");
		image_laterality_right = (Gtk.Image) builder.GetObject ("image_laterality_right");

		// raceAnalyzer
		label_race_analyzer_distance = (Gtk.Label) builder.GetObject ("label_race_analyzer_distance");
		spin_race_analyzer_distance = (Gtk.SpinButton) builder.GetObject ("spin_race_analyzer_distance");
		label_race_analyzer_distance_units = (Gtk.Label) builder.GetObject ("label_race_analyzer_distance_units");
		label_race_analyzer_angle = (Gtk.Label) builder.GetObject ("label_race_analyzer_angle");
		spin_race_analyzer_angle = (Gtk.SpinButton) builder.GetObject ("spin_race_analyzer_angle");
		label_race_analyzer_angle_units = (Gtk.Label) builder.GetObject ("label_race_analyzer_angle_units");
		label_race_analyzer_temperature = (Gtk.Label) builder.GetObject ("label_race_analyzer_temperature");
		spin_race_analyzer_temperature = (Gtk.SpinButton) builder.GetObject ("spin_race_analyzer_temperature");
		label_race_analyzer_temperature_units = (Gtk.Label) builder.GetObject ("label_race_analyzer_temperature_units");

		// encoder 
		// exercise
		label_encoder_exercise = (Gtk.Label) builder.GetObject ("label_encoder_exercise");
		button_encoder_select = (Gtk.Button) builder.GetObject ("button_encoder_select");
		image_encoder_configuration = (Gtk.Image) builder.GetObject ("image_encoder_configuration");
		box_encoder_selected = (Gtk.Box) builder.GetObject ("box_encoder_selected");
		image_encoder_selected_type = (Gtk.Image) builder.GetObject ("image_encoder_selected_type");
		label_encoder_selected = (Gtk.Label) builder.GetObject ("label_encoder_selected");
		// eccon
		label_encoder_eccon_title = (Gtk.Label) builder.GetObject ("label_encoder_eccon_title");
		box_encoder_eccon = (Gtk.Box) builder.GetObject ("box_encoder_eccon");
		radio_encoder_eccon_concentric = (Gtk.RadioButton) builder.GetObject ("radio_encoder_eccon_concentric");
		radio_encoder_eccon_eccentric_concentric = (Gtk.RadioButton) builder.GetObject ("radio_encoder_eccon_eccentric_concentric");
		image_encoder_eccon_concentric = (Gtk.Image) builder.GetObject ("image_encoder_eccon_concentric");
		image_encoder_eccon_eccentric_concentric = (Gtk.Image) builder.GetObject ("image_encoder_eccon_eccentric_concentric");
		label_encoder_ecc_con_alert = (Gtk.Label) builder.GetObject ("label_encoder_ecc_con_alert");
		label_encoder_rep_length = (Gtk.Label) builder.GetObject ("label_encoder_rep_length");
		vbox_encoder_rep_length = (Gtk.VBox) builder.GetObject ("vbox_encoder_rep_length");
		spin_encoder_rep_min_height_gravitatory = (Gtk.SpinButton) builder.GetObject ("spin_encoder_rep_min_height_gravitatory");
		spin_encoder_rep_min_height_inertial = (Gtk.SpinButton) builder.GetObject ("spin_encoder_rep_min_height_inertial");
		label_encoder_rep_length_units = (Gtk.Label) builder.GetObject ("label_encoder_rep_length_units");
		// encoder mass-inertia
		label_encoder_exercise_mass = (Gtk.Label) builder.GetObject ("label_encoder_exercise_mass");
		label_encoder_exercise_inertia = (Gtk.Label) builder.GetObject ("label_encoder_exercise_inertia");
		hbox_encoder_exercise_mass = (Gtk.HBox) builder.GetObject ("hbox_encoder_exercise_mass");
		box_encoder_exercise_inertia = (Gtk.Box) builder.GetObject ("box_encoder_exercise_inertia");
		image_extra_mass = (Gtk.Image) builder.GetObject ("image_extra_mass");
		spin_encoder_extra_weight = (Gtk.SpinButton) builder.GetObject ("spin_encoder_extra_weight");
		label_encoder_displaced_weight = (Gtk.Label) builder.GetObject ("label_encoder_displaced_weight");
		hbox_combo_encoder_anchorage = (Gtk.HBox) builder.GetObject ("hbox_combo_encoder_anchorage");
		image_encoder_inertial_weights = (Gtk.Image) builder.GetObject ("image_encoder_inertial_weights");
		spin_encoder_im_weights_n = (Gtk.SpinButton) builder.GetObject ("spin_encoder_im_weights_n");
		label_encoder_im_total = (Gtk.Label) builder.GetObject ("label_encoder_im_total");
		label_encoder_equivalent_mass = (Gtk.Label) builder.GetObject ("label_encoder_equivalent_mass");

		hbox_video = (Gtk.Box) builder.GetObject ("hbox_video");
		label_video = (Gtk.Label) builder.GetObject ("label_video");
		label_video_yes_no = (Gtk.Label) builder.GetObject ("label_video_yes_no");
		button_video_watch = (Gtk.Button) builder.GetObject ("button_video_watch");
		image_video_watch = (Gtk.Image) builder.GetObject ("image_video_watch");
		button_video_url = (Gtk.Button) builder.GetObject ("button_video_url");
		label_description = (Gtk.Label) builder.GetObject ("label_description");
		entry_description = (Gtk.Entry) builder.GetObject ("entry_description");
		// textview_description = (Gtk.TextView) builder.GetObject ("textview_description");
	}

	~EditEventWindow() {}
}
