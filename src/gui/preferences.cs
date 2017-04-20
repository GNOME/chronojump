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
using System.IO;
using Gdk;
using Gtk;
using Glade;
//using Gnome;
//using GLib; //for Value
using System.Text; //StringBuilder
using System.Collections; //ArrayList
using Mono.Unix;
using System.Threading;
using System.Globalization; //CultureInfo stuff

using System.Diagnostics;  //Stopwatch


/*
using ICSharpCode.SharpZipLib.Tar;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib;
*/

public class PreferencesWindow
{
	[Widget] Gtk.Window preferences_win;
	[Widget] Gtk.Notebook notebook;

	//appearance tab
	[Widget] Gtk.CheckButton check_appearance_maximized;
	[Widget] Gtk.CheckButton check_appearance_maximized_undecorated;
	[Widget] Gtk.CheckButton check_appearance_person_win_hide;
	[Widget] Gtk.CheckButton check_appearance_encoder_only_bars;
	[Widget] Gtk.Alignment alignment_undecorated;
	[Widget] Gtk.Alignment alignment_restart;

	//database tab
	[Widget] Gtk.Button button_data_folder_open;

	[Widget] Gtk.CheckButton check_backup_multimedia_and_encoder;
	
	[Widget] Gtk.Button button_db_backup;
	[Widget] Gtk.Box hbox_backup_doing;
	[Widget] Gtk.ProgressBar pulsebar;

	
	//jumps tab	
	[Widget] Gtk.CheckButton checkbutton_power;
	[Widget] Gtk.CheckButton checkbutton_stiffness;
	[Widget] Gtk.CheckButton checkbutton_initial_speed;
	[Widget] Gtk.CheckButton checkbutton_angle;
	[Widget] Gtk.CheckButton checkbutton_show_tv_tc_index;
	[Widget] Gtk.Box hbox_indexes;
	[Widget] Gtk.RadioButton radiobutton_show_q_index;
	[Widget] Gtk.RadioButton radiobutton_show_dj_index;
	[Widget] Gtk.RadioButton radio_elevation_height;
	[Widget] Gtk.RadioButton radio_elevation_tf;
	[Widget] Gtk.RadioButton radio_weight_percent;
	[Widget] Gtk.RadioButton radio_weight_kg;
	[Widget] Gtk.RadioButton radio_use_heights_on_jump_indexes;
	[Widget] Gtk.RadioButton radio_do_not_use_heights_on_jump_indexes;
			
	//runs tab	
	[Widget] Gtk.RadioButton radio_speed_ms;
	[Widget] Gtk.RadioButton radio_speed_km;
	[Widget] Gtk.RadioButton radio_runs_speed_start_arrival; 
	[Widget] Gtk.RadioButton radio_runs_speed_start_leaving; 
	[Widget] Gtk.Image image_races_simple;
	[Widget] Gtk.Image image_races_intervallic;
	[Widget] Gtk.Notebook notebook_races_double_contacts;
	[Widget] Gtk.Box vbox_runs_prevent_double_contact;
	[Widget] Gtk.CheckButton checkbutton_runs_prevent_double_contact;
	[Widget] Gtk.SpinButton spinbutton_runs_prevent_double_contact;
	[Widget] Gtk.RadioButton radio_runs_prevent_double_contact_first;
	[Widget] Gtk.RadioButton radio_runs_prevent_double_contact_average;
	[Widget] Gtk.RadioButton radio_runs_prevent_double_contact_last;
	[Widget] Gtk.Box vbox_runs_i_prevent_double_contact;
	[Widget] Gtk.CheckButton checkbutton_runs_i_prevent_double_contact;
	[Widget] Gtk.SpinButton spinbutton_runs_i_prevent_double_contact;
	[Widget] Gtk.RadioButton radio_runs_i_prevent_double_contact_first;
	[Widget] Gtk.RadioButton radio_runs_i_prevent_double_contact_average;
	[Widget] Gtk.RadioButton radio_runs_i_prevent_double_contact_last;
	
	//encoder capture tab
	[Widget] Gtk.SpinButton spin_encoder_capture_time;
	[Widget] Gtk.SpinButton spin_encoder_capture_inactivity_end_time;
	[Widget] Gtk.Box hbox_combo_main_variable;
	[Widget] Gtk.ComboBox combo_main_variable;
	[Widget] Gtk.SpinButton spin_encoder_capture_min_height_gravitatory;
	[Widget] Gtk.SpinButton spin_encoder_capture_min_height_inertial;
	[Widget] Gtk.CheckButton checkbutton_encoder_capture_fully_extended;
	[Widget] Gtk.HBox hbox_encoder_capture_fully_extended;
	[Widget] Gtk.SpinButton spin_encoder_capture_fully_extended;
	[Widget] Gtk.RadioButton radio_encoder_auto_save_curve_best;
	[Widget] Gtk.RadioButton radio_encoder_auto_save_curve_4top;
	[Widget] Gtk.RadioButton radio_encoder_auto_save_curve_all;
	[Widget] Gtk.RadioButton radio_encoder_auto_save_curve_none;
	[Widget] Gtk.CheckButton check_show_start_and_duration;
	
	//encoder other tab
	[Widget] Gtk.CheckButton checkbutton_encoder_propulsive;
	[Widget] Gtk.SpinButton spin_encoder_smooth_con;
	[Widget] Gtk.Label label_encoder_con;
	[Widget] Gtk.RadioButton radio_encoder_1RM_nonweighted;
	[Widget] Gtk.RadioButton radio_encoder_1RM_weighted;
	[Widget] Gtk.RadioButton radio_encoder_1RM_weighted2;
	[Widget] Gtk.RadioButton radio_encoder_1RM_weighted3;

	//force sensor tab
	[Widget] Gtk.HBox hbox_force_1;
	[Widget] Gtk.HBox hbox_force_2;
	[Widget] Gtk.HBox hbox_force_3;
	[Widget] Gtk.HBox hbox_force_4;
	[Widget] Gtk.CheckButton check_force_1;
	[Widget] Gtk.CheckButton check_force_2;
	[Widget] Gtk.CheckButton check_force_3;
	[Widget] Gtk.CheckButton check_force_4;
	[Widget] Gtk.ComboBox combo_force_1_function;
	[Widget] Gtk.ComboBox combo_force_2_function;
	[Widget] Gtk.ComboBox combo_force_3_function;
	[Widget] Gtk.ComboBox combo_force_4_function;
	[Widget] Gtk.ComboBox combo_force_1_type;
	[Widget] Gtk.ComboBox combo_force_2_type;
	[Widget] Gtk.ComboBox combo_force_3_type;
	[Widget] Gtk.ComboBox combo_force_4_type;
	[Widget] Gtk.HBox hbox_force_1_at_ms;
	[Widget] Gtk.HBox hbox_force_2_at_ms;
	[Widget] Gtk.HBox hbox_force_3_at_ms;
	[Widget] Gtk.HBox hbox_force_4_at_ms;
	[Widget] Gtk.HBox hbox_force_1_at_percent;
	[Widget] Gtk.HBox hbox_force_2_at_percent;
	[Widget] Gtk.HBox hbox_force_3_at_percent;
	[Widget] Gtk.HBox hbox_force_4_at_percent;
	[Widget] Gtk.HBox hbox_force_1_from_to;
	[Widget] Gtk.HBox hbox_force_2_from_to;
	[Widget] Gtk.HBox hbox_force_3_from_to;
	[Widget] Gtk.HBox hbox_force_4_from_to;

	//multimedia tab
	[Widget] Gtk.CheckButton checkbutton_volume;
	[Widget] Gtk.Box hbox_combo_camera;
	[Widget] Gtk.ComboBox combo_camera;

	//language tab
	[Widget] Gtk.Box hbox_combo_language;
	[Widget] Gtk.ComboBox combo_language;
	[Widget] Gtk.RadioButton radio_language_detected;
	[Widget] Gtk.RadioButton radio_language_force;
	[Widget] Gtk.RadioButton radio_graphs_translate;
	[Widget] Gtk.RadioButton radio_graphs_no_translate;
	[Widget] Gtk.Box hbox_need_restart;
		
	//advanced tab
	[Widget] Gtk.ComboBox combo_decimals;
	[Widget] Gtk.CheckButton checkbutton_ask_deletion;
	[Widget] Gtk.RadioButton radio_export_latin;
	[Widget] Gtk.RadioButton radio_export_non_latin;
	[Widget] Gtk.Label label_advanced_feedback;
	[Widget] Gtk.ToggleButton toggle_gc_collect_on_close;
	[Widget] Gtk.ToggleButton toggle_never_close;


	[Widget] Gtk.Button button_accept;
	[Widget] Gtk.Button button_cancel;
	public Gtk.Button FakeButtonImported;
	public Gtk.Button FakeButtonDebugModeStart;
	
	static PreferencesWindow PreferencesWindowBox;
	
	private Preferences preferences; //stored to update SQL if anything changed
	private Thread thread;

	string databaseURL;
	string databaseTempURL;
	
	ListStore langsStore;

	const int JUMPSPAGE = 2;
	const int RUNSPAGE = 3;
	const int ENCODERCAPTUREPAGE = 4;
	const int ENCODEROTHERPAGE = 5;
	const int FORCEPAGE = 6;


	PreferencesWindow () {
		Glade.XML gladeXML;
		gladeXML = Glade.XML.FromAssembly (Util.GetGladePath() + "preferences_win.glade", "preferences_win", "chronojump");
		gladeXML.Autoconnect(this);
		
		//put an icon to window
		UtilGtk.IconWindow(preferences_win);

		//database and log files stuff
		databaseURL = Util.GetDatabaseDir() + System.IO.Path.DirectorySeparatorChar  + "chronojump.db";
		databaseTempURL = Util.GetDatabaseTempDir() + System.IO.Path.DirectorySeparatorChar  + "chronojump.db";
		
		FakeButtonImported = new Gtk.Button();
		FakeButtonDebugModeStart = new Gtk.Button();
	}
	
	static public PreferencesWindow Show (Preferences preferences, Constants.Menuitem_modes menu_mode)
	{
		if (PreferencesWindowBox == null) {
			PreferencesWindowBox = new PreferencesWindow ();
		}

		if(menu_mode !=	Constants.Menuitem_modes.JUMPSSIMPLE && menu_mode != Constants.Menuitem_modes.JUMPSREACTIVE)
			PreferencesWindowBox.notebook.GetNthPage(JUMPSPAGE).Hide();
		if(menu_mode !=	Constants.Menuitem_modes.RUNSSIMPLE && menu_mode != Constants.Menuitem_modes.RUNSINTERVALLIC)
			PreferencesWindowBox.notebook.GetNthPage(RUNSPAGE).Hide();
		if(menu_mode !=	Constants.Menuitem_modes.POWERGRAVITATORY && menu_mode != Constants.Menuitem_modes.POWERINERTIAL) {
			PreferencesWindowBox.notebook.GetNthPage(ENCODERCAPTUREPAGE).Hide();
			PreferencesWindowBox.notebook.GetNthPage(ENCODEROTHERPAGE).Hide();
		}
		if(menu_mode !=	Constants.Menuitem_modes.FORCESENSOR)
			PreferencesWindowBox.notebook.GetNthPage(FORCEPAGE).Hide();

		PreferencesWindowBox.preferences = preferences;

		PreferencesWindowBox.createComboLanguage();

		//appearence tab
		if(preferences.maximized == Preferences.MaximizedTypes.NO)
		{
			PreferencesWindowBox.check_appearance_maximized.Active = false;
			PreferencesWindowBox.alignment_undecorated.Visible = false;
		}
		else {
			PreferencesWindowBox.check_appearance_maximized.Active = true;
			PreferencesWindowBox.alignment_undecorated.Visible = true;
			PreferencesWindowBox.check_appearance_maximized_undecorated.Active =
				(preferences.maximized == Preferences.MaximizedTypes.YESUNDECORATED);
		}

		if(preferences.personWinHide)
			PreferencesWindowBox.check_appearance_person_win_hide.Active = true;
		else
			PreferencesWindowBox.check_appearance_person_win_hide.Active = false;

		if(preferences.encoderCaptureShowOnlyBars)
			PreferencesWindowBox.check_appearance_encoder_only_bars.Active = true;
		else
			PreferencesWindowBox.check_appearance_encoder_only_bars.Active = false;


		//multimedia tab
		if(preferences.volumeOn)  
			PreferencesWindowBox.checkbutton_volume.Active = true; 
		else 
			PreferencesWindowBox.checkbutton_volume.Active = false; 

		PreferencesWindowBox.createComboCamera(UtilVideo.GetVideoDevices(), preferences.videoDeviceNum);
	

		string [] decs = {"1", "2", "3"};
		PreferencesWindowBox.combo_decimals.Active = UtilGtk.ComboMakeActive(
				decs, preferences.digitsNumber.ToString());

		if(preferences.showPower)
			PreferencesWindowBox.checkbutton_power.Active = true; 
		else
			PreferencesWindowBox.checkbutton_power.Active = false; 
		
		if(preferences.showStiffness)
			PreferencesWindowBox.checkbutton_stiffness.Active = true; 
		else
			PreferencesWindowBox.checkbutton_stiffness.Active = false; 
		
		if(preferences.showInitialSpeed)  
			PreferencesWindowBox.checkbutton_initial_speed.Active = true; 
		else 
			PreferencesWindowBox.checkbutton_initial_speed.Active = false; 
		
		if(preferences.showAngle)  
			PreferencesWindowBox.checkbutton_angle.Active = true; 
		else 
			PreferencesWindowBox.checkbutton_angle.Active = false; 
		

		if(preferences.showQIndex || preferences.showDjIndex) { 
			PreferencesWindowBox.checkbutton_show_tv_tc_index.Active = true; 
			if(preferences.showQIndex) {
				PreferencesWindowBox.radiobutton_show_q_index.Active = true; 
				PreferencesWindowBox.radiobutton_show_dj_index.Active = false; 
			} else {
				PreferencesWindowBox.radiobutton_show_q_index.Active = false; 
				PreferencesWindowBox.radiobutton_show_dj_index.Active = true; 
			}
		}
		else {
			PreferencesWindowBox.checkbutton_show_tv_tc_index.Active = false; 
			PreferencesWindowBox.hbox_indexes.Hide();
		}

		if(preferences.askDeletion)  
			PreferencesWindowBox.checkbutton_ask_deletion.Active = true; 
		else 
			PreferencesWindowBox.checkbutton_ask_deletion.Active = false; 
		

		if(preferences.weightStatsPercent)  
			PreferencesWindowBox.radio_weight_percent.Active = true; 
		else 
			PreferencesWindowBox.radio_weight_kg.Active = true; 
		

		if(preferences.heightPreferred)  
			PreferencesWindowBox.radio_elevation_height.Active = true; 
		else 
			PreferencesWindowBox.radio_elevation_tf.Active = true; 
		

		if(preferences.metersSecondsPreferred)  
			PreferencesWindowBox.radio_speed_ms.Active = true; 
		else 
			PreferencesWindowBox.radio_speed_km.Active = true; 


		if(preferences.runSpeedStartArrival)  
			PreferencesWindowBox.radio_runs_speed_start_arrival.Active = true; 
		else 
			PreferencesWindowBox.radio_runs_speed_start_leaving.Active = true; 


		//start of double contacts stuff ----
		Pixbuf pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_run_simple.png");
		PreferencesWindowBox.image_races_simple.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_run_multiple.png");
		PreferencesWindowBox.image_races_intervallic.Pixbuf = pixbuf;

		if(menu_mode ==	Constants.Menuitem_modes.RUNSSIMPLE)
			PreferencesWindowBox.notebook_races_double_contacts.CurrentPage = 0;
		else if(menu_mode == Constants.Menuitem_modes.RUNSINTERVALLIC)
			PreferencesWindowBox.notebook_races_double_contacts.CurrentPage = 1;

		PreferencesWindowBox.checkbutton_runs_prevent_double_contact.Active = 
			(preferences.runDoubleContactsMode != Constants.DoubleContact.NONE);
		PreferencesWindowBox.checkbutton_runs_i_prevent_double_contact.Active = 
			(preferences.runIDoubleContactsMode != Constants.DoubleContact.NONE);

		PreferencesWindowBox.spinbutton_runs_prevent_double_contact.Value = 
			preferences.runDoubleContactsMS;
		PreferencesWindowBox.spinbutton_runs_i_prevent_double_contact.Value = 
			preferences.runIDoubleContactsMS;
			
		if(preferences.runDoubleContactsMode != Constants.DoubleContact.NONE) {
			if(preferences.runDoubleContactsMode == Constants.DoubleContact.FIRST)
				PreferencesWindowBox.radio_runs_prevent_double_contact_first.Active = true;
			else if(preferences.runDoubleContactsMode == Constants.DoubleContact.AVERAGE)
				PreferencesWindowBox.radio_runs_prevent_double_contact_average.Active = true;
			else // Constants.DoubleContact.LAST  DEFAULT
				PreferencesWindowBox.radio_runs_prevent_double_contact_last.Active = true;
		}
		if(preferences.runIDoubleContactsMode != Constants.DoubleContact.NONE) {
			if(preferences.runIDoubleContactsMode == Constants.DoubleContact.FIRST)
				PreferencesWindowBox.radio_runs_i_prevent_double_contact_first.Active = true;
			else if(preferences.runIDoubleContactsMode == Constants.DoubleContact.LAST)
				PreferencesWindowBox.radio_runs_i_prevent_double_contact_last.Active = true;
			else //Constants.DoubleContact.AVERAGE  DEFAULT
				PreferencesWindowBox.radio_runs_i_prevent_double_contact_average.Active = true;
		}
		//---- end of double contacts stuff		


		if(preferences.CSVExportDecimalSeparator == "COMMA")
			PreferencesWindowBox.radio_export_latin.Active = true; 
		else
			PreferencesWindowBox.radio_export_non_latin.Active = true; 

	
		//encoder capture -->
		PreferencesWindowBox.spin_encoder_capture_time.Value = preferences.encoderCaptureTime;
		PreferencesWindowBox.spin_encoder_capture_inactivity_end_time.Value = preferences.encoderCaptureInactivityEndTime;
		
		PreferencesWindowBox.createComboEncoderCaptureMainVariable(
				Constants.GetEncoderVariablesCapture(preferences.encoderCaptureMainVariable));

		PreferencesWindowBox.spin_encoder_capture_min_height_gravitatory.Value = preferences.encoderCaptureMinHeightGravitatory;
		PreferencesWindowBox.spin_encoder_capture_min_height_inertial.Value = preferences.encoderCaptureMinHeightInertial;
		PreferencesWindowBox.checkbutton_encoder_capture_fully_extended.Active = preferences.encoderCaptureCheckFullyExtended;
		PreferencesWindowBox.spin_encoder_capture_fully_extended.Value = preferences.encoderCaptureCheckFullyExtendedValue;
		PreferencesWindowBox.hbox_encoder_capture_fully_extended.Visible = preferences.encoderCaptureCheckFullyExtended;
		
		if(preferences.encoderAutoSaveCurve == Constants.EncoderAutoSaveCurve.BEST)
			PreferencesWindowBox.radio_encoder_auto_save_curve_best.Active = true;
		else if(preferences.encoderAutoSaveCurve == Constants.EncoderAutoSaveCurve.FROM4TOPENULTIMATE)
			PreferencesWindowBox.radio_encoder_auto_save_curve_4top.Active = true;
		else if(preferences.encoderAutoSaveCurve == Constants.EncoderAutoSaveCurve.ALL)
			PreferencesWindowBox.radio_encoder_auto_save_curve_all.Active = true;
		else
			PreferencesWindowBox.radio_encoder_auto_save_curve_none.Active = true;
	
		PreferencesWindowBox.check_show_start_and_duration.Active = preferences.encoderShowStartAndDuration;


		//encoder other -->
		PreferencesWindowBox.checkbutton_encoder_propulsive.Active = preferences.encoderPropulsive;
		
		PreferencesWindowBox.spin_encoder_smooth_con.Value = preferences.encoderSmoothCon;

		if(preferences.encoder1RMMethod == Constants.Encoder1RMMethod.NONWEIGHTED)
			PreferencesWindowBox.radio_encoder_1RM_nonweighted.Active = true;
		else if(preferences.encoder1RMMethod == Constants.Encoder1RMMethod.WEIGHTED)
			PreferencesWindowBox.radio_encoder_1RM_weighted.Active = true;
		else if(preferences.encoder1RMMethod == Constants.Encoder1RMMethod.WEIGHTED2)
			PreferencesWindowBox.radio_encoder_1RM_weighted2.Active = true;
		else //(preferences.encoder1RMMethod == Constants.Encoder1RMMethod.WEIGHTED3)
			PreferencesWindowBox.radio_encoder_1RM_weighted3.Active = true;

		//done here and not in glade to be shown with the decimal point of user language	
		PreferencesWindowBox.label_encoder_con.Text = (0.7).ToString();
	
		//force
		PreferencesWindowBox.initForceSensor();

		//language -->
		if(preferences.language == "")
			PreferencesWindowBox.radio_language_detected.Active = true;
		else
			PreferencesWindowBox.radio_language_force.Active = true;

		if(preferences.RGraphsTranslate)
			PreferencesWindowBox.radio_graphs_translate.Active = true;
		else
			PreferencesWindowBox.radio_graphs_no_translate.Active = true;
			
		//allow signal to be called
		PreferencesWindowBox.hbox_language_signalOn = true;
		
		if(preferences.useHeightsOnJumpIndexes)
			PreferencesWindowBox.radio_use_heights_on_jump_indexes.Active = true;
		else
			PreferencesWindowBox.radio_do_not_use_heights_on_jump_indexes.Active = true;
			

		PreferencesWindowBox.preferences_win.Show ();
		return PreferencesWindowBox;
	}
	
	private void createComboEncoderCaptureMainVariable(string v) {
		combo_main_variable = ComboBox.NewText ();
		string [] values = Constants.EncoderVariablesCaptureList;
		UtilGtk.ComboUpdate(combo_main_variable, values, "");
		combo_main_variable.Active = UtilGtk.ComboMakeActive(combo_main_variable, v.ToString());
		
		hbox_combo_main_variable.PackStart(combo_main_variable, false, false, 0);
		hbox_combo_main_variable.ShowAll();
		combo_main_variable.Sensitive = true;
	}

	
	private void createComboCamera(string [] devices, int current) {
		combo_camera = ComboBox.NewText ();

		if(devices.Length == 0) {
			devices = Util.StringToStringArray(Constants.CameraNotFound);
			current = 0;
		}
		
		UtilGtk.ComboUpdate(combo_camera, devices, "");
		hbox_combo_camera.PackStart(combo_camera, true, true, 0);
		hbox_combo_camera.ShowAll();

		if(current >= devices.Length)
			current = 0;
		
		combo_camera.Active = UtilGtk.ComboMakeActive(devices, devices[current]);
	}
		
	private void on_check_appearance_maximized_toggled (object obj, EventArgs args)
	{
		alignment_undecorated.Visible = check_appearance_maximized.Active;
	}

	private void on_check_appearance_encoder_only_bars_toggled (object obj, EventArgs args) 
	{
		alignment_restart.Visible = ! check_appearance_encoder_only_bars.Active;
	}

	private void on_checkbutton_encoder_capture_fully_extended_toggled(object obj, EventArgs args) {
		hbox_encoder_capture_fully_extended.Visible = checkbutton_encoder_capture_fully_extended.Active;
	}


	// ---- Force sensor stuff

	private void initForceSensor ()
	{
		createForceCombos();

		check_force_visibilities();
	}

	private void check_force_visibilities()
	{
		hbox_force_1.Visible = (check_force_1.Active);
		hbox_force_2.Visible = (check_force_2.Active);
		hbox_force_3.Visible = (check_force_3.Active);
		hbox_force_4.Visible = (check_force_4.Active);
	}

	private void on_check_force_clicked (object o, EventArgs args)
	{
		check_force_visibilities();
	}

	//private static string [] forceTypes = {
	//	"Instantaneous", "Average", "% Force max", "RFD max"
	//};
	private static string [] forceTypesTranslated = {
		Catalog.GetString("Instantaneous"), Catalog.GetString("Average"),
		Catalog.GetString("% Force max"), Catalog.GetString("RFD max")
	};
	private void createForceCombos ()
	{
		combo_force_1_function.Active = 0;
		combo_force_2_function.Active = 0;
		combo_force_3_function.Active = 0;
		combo_force_4_function.Active = 0;

		UtilGtk.ComboUpdate(combo_force_1_type, forceTypesTranslated, "");
		UtilGtk.ComboUpdate(combo_force_2_type, forceTypesTranslated, "");
		UtilGtk.ComboUpdate(combo_force_3_type, forceTypesTranslated, "");
		UtilGtk.ComboUpdate(combo_force_4_type, forceTypesTranslated, "");

		combo_force_1_type.Active = 0;
		combo_force_2_type.Active = 0;
		combo_force_3_type.Active = 0;
		combo_force_4_type.Active = 0;
	}

	private void on_combo_force_type_changed (object o, EventArgs args)
	{
		Gtk.ComboBox combo = o as ComboBox;
		if (o == null)
			return;

		if(combo == combo_force_1_type)
			combo_force_visibility(
					UtilGtk.ComboGetActive(combo_force_1_type),
					hbox_force_1_at_ms,
					hbox_force_1_at_percent,
					hbox_force_1_from_to);
		else if(combo == combo_force_2_type)
			combo_force_visibility(
					UtilGtk.ComboGetActive(combo_force_2_type),
					hbox_force_2_at_ms,
					hbox_force_2_at_percent,
					hbox_force_2_from_to);
		else if(combo == combo_force_3_type)
			combo_force_visibility(
					UtilGtk.ComboGetActive(combo_force_3_type),
					hbox_force_3_at_ms,
					hbox_force_3_at_percent,
					hbox_force_3_from_to);
		else if(combo == combo_force_4_type)
			combo_force_visibility(
					UtilGtk.ComboGetActive(combo_force_4_type),
					hbox_force_4_at_ms,
					hbox_force_4_at_percent,
					hbox_force_4_from_to);
	}

	private void combo_force_visibility (string selected, Gtk.HBox at_ms, Gtk.HBox at_percent, Gtk.HBox from_to)
	{
		//valid for active == "" and active == "RFD max"
		at_ms.Visible = false;
		at_percent.Visible = false;
		from_to.Visible = false;

		//LogB.Information("Selected:" + selected + ";");

		if(selected == Catalog.GetString("Instantaneous"))
		{
			at_ms.Visible = true;
		}
		else if(selected == Catalog.GetString("Average"))
		{
			from_to.Visible = true;
		}
		else if(selected == Catalog.GetString("% Force max"))
		{
			at_percent.Visible = true;
		}
	}

	// ---- end of Force sensor stuff


	// ---- Language stuff
	
	private void createComboLanguage() {
		
		combo_language = ComboBox.NewText ();
		fillLanguages();

		hbox_combo_language.PackStart(combo_language, false, false, 0);
		hbox_combo_language.ShowAll();
	}

	//from Longomatch ;)
	//(C) Andoni Morales Alastruey
	bool hbox_language_signalOn = false;
	void fillLanguages () {
		int index = 0, active = 0;

		langsStore = new ListStore(typeof(string), typeof(CultureInfo));

		foreach (CultureInfo lang in UtilLanguage.Languages) {
			langsStore.AppendValues(lang.NativeName, lang);
			if (preferences.language != "" && lang.Name == preferences.language)
				active = index;
			index ++;
		}
		combo_language.Model = langsStore;
		combo_language.Active = active;
		combo_language.Changed += combo_language_changed;
	}

	private void on_radio_language_toggled (object obj, EventArgs args) {
		hbox_combo_language.Sensitive = radio_language_force.Active;

		if(hbox_language_signalOn)
			hbox_need_restart.Visible = true;
	}
	private void on_radio_translate_toggled (object obj, EventArgs args) {
		if(hbox_language_signalOn)
			hbox_need_restart.Visible = true;
	}
	private	void combo_language_changed (object obj, EventArgs args) {
		if(hbox_language_signalOn)
			hbox_need_restart.Visible = true;
	}


	string getSelectedLanguage()
	{
		TreeIter iter;
		CultureInfo info;

		combo_language.GetActiveIter (out iter);
		info = (CultureInfo) langsStore.GetValue (iter, 1);
		if (info == null) {
			return "";
		} else {
			return info.Name;
		}
	}

	// ---- end of Language stuff

			
	private void on_checkbutton_show_tv_tc_index_clicked (object o, EventArgs args) {
		if(checkbutton_show_tv_tc_index.Active)
			hbox_indexes.Show();
		else
			hbox_indexes.Hide();
	}
	
	private void on_checkbutton_runs_prevent_double_contact_toggled (object o, EventArgs args) {
		vbox_runs_prevent_double_contact.Visible = checkbutton_runs_prevent_double_contact.Active;
	}
	private void on_checkbutton_runs_i_prevent_double_contact_toggled (object o, EventArgs args) {
		vbox_runs_i_prevent_double_contact.Visible = checkbutton_runs_i_prevent_double_contact.Active;
	}

		
	void on_button_cancel_clicked (object o, EventArgs args)
	{
		PreferencesWindowBox.preferences_win.Hide();
		PreferencesWindowBox = null;
	}
	
	void on_preferences_delete_event (object o, DeleteEventArgs args)
	{
		//do not hide/exit if copyiing
		if (thread != null && thread.IsAlive)
			args.RetVal = true;
		else {
			PreferencesWindowBox.preferences_win.Hide();
			PreferencesWindowBox = null;
		}
	}
	
	void on_button_data_folder_open_clicked (object o, EventArgs args)
	{
		System.IO.FileInfo file1 = new System.IO.FileInfo(databaseURL); //potser cal una arrobar abans (a windows)
		System.IO.FileInfo file2 = new System.IO.FileInfo(databaseTempURL); //potser cal una arrobar abans (a windows)

		if(file1.Exists)
			System.Diagnostics.Process.Start(Util.GetParentDir(false)); 
		else if(file2.Exists)
			System.Diagnostics.Process.Start(Util.GetDatabaseTempDir()); 
		else
			new DialogMessage(Constants.MessageTypes.WARNING, Constants.DatabaseNotFound);
	}
	
	void on_button_db_restore_clicked (object o, EventArgs args)
	{
		/*
		 * TODO: problem is database stored is a chronojump.db or a folder (if images and videos were saved).
		 * FileChooserAction only lets you use one type
		 * In the future backup db as tgz or similar
		 */

		/*
		fc = new Gtk.FileChooserDialog(Catalog.GetString("Restore database from:"),
				preferences_win,
				FileChooserAction.SelectFolder,
				Catalog.GetString("Cancel"),ResponseType.Cancel,
				Catalog.GetString("Restore"),ResponseType.Accept
				);

		ConfirmWindow confirmWin = ConfirmWindow.Show(Catalog.GetString("Are you sure you want to restore?"));
		confirmWin.Button_accept.Clicked += new EventHandler(on_overwrite_file_accepted);
		*/
	}

	
	void on_button_logs_folder_open_clicked (object o, EventArgs args)
	{
		string dir = UtilAll.GetLogsDir();
		LogB.Information(dir);
		
		if( ! new System.IO.DirectoryInfo(dir).Exists) {
			try {
				Directory.CreateDirectory (dir);
			} catch {
				new DialogMessage(Constants.MessageTypes.WARNING, 
						Catalog.GetString("Cannot create directory.") + "\n\n" + dir);
				return;
			}
		}
		
		try {
			System.Diagnostics.Process.Start(dir); 
		}
		catch {
			new DialogMessage(Constants.MessageTypes.WARNING, 
					Constants.DirectoryCannotOpen + "\n\n" + dir);
		}
	}
	
	void on_button_tmp_folder_open_clicked (object o, EventArgs args)
	{
		string dir = UtilAll.GetTempDir(); //potser cal una arrobar abans (a windows)
		System.IO.FileInfo fInfo = new System.IO.FileInfo(dir);

		try {
			if(fInfo.Exists)
				System.Diagnostics.Process.Start(dir);
		} catch {
			new DialogMessage(Constants.MessageTypes.WARNING, 
					Constants.DirectoryCannotOpen + "\n\n" + dir);
		}

		LogB.Warning(dir);
	}



	string fileDB;
	string fileCopy;
	Gtk.FileChooserDialog fc;
	void on_button_db_backup_clicked (object o, EventArgs args)
	{
		System.IO.FileInfo file1 = new System.IO.FileInfo(databaseURL); //potser cal una arrobar abans (a windows)
		System.IO.FileInfo file2 = new System.IO.FileInfo(databaseTempURL); //potser cal una arrobar abans (a windows)
		fileDB = "";

		long length1 = 0;
		if(file1.Exists)
			length1 = file1.Length;
		long length2 = 0;
		if(file2.Exists)
			length2 = file2.Length;
		
		if(length1 == 0 && length2 == 0) 
			new DialogMessage(Constants.MessageTypes.WARNING, Catalog.GetString("Error. Cannot find database."));
		else if(length1 > length2)
			fileDB = databaseURL;
		else
			fileDB = databaseTempURL;

		fc = new Gtk.FileChooserDialog(Catalog.GetString("Copy database to:"),
				preferences_win,
				FileChooserAction.SelectFolder,
				Catalog.GetString("Cancel"),ResponseType.Cancel,
				Catalog.GetString("Copy"),ResponseType.Accept
				);

		if (fc.Run() == (int)ResponseType.Accept) 
		{
			//if multimedia_and_encoder, then copy the folder. If not checked, then copy only the db file
			if(check_backup_multimedia_and_encoder.Active)
				fileCopy = fc.Filename + Path.DirectorySeparatorChar + "chronojump";
			else
				fileCopy = fc.Filename + Path.DirectorySeparatorChar + "chronojump_copy.db";

			try {
				fc.Hide ();
			
				bool exists = false;
				if(check_backup_multimedia_and_encoder.Active) {
					if(Directory.Exists(fileCopy)) {
						LogB.Information(string.Format("Directory {0} exists, created at {1}", 
									fileCopy, Directory.GetCreationTime(fileCopy)));
						exists = true;
					}
				} else {
					if (File.Exists(fileCopy)) {
						LogB.Information(string.Format("File {0} exists with attributes {1}, created at {2}", 
									fileCopy, File.GetAttributes(fileCopy), File.GetCreationTime(fileCopy)));
						exists = true;
					}
				}

				if(exists) {
					LogB.Information("Overwrite...");
					ConfirmWindow confirmWin = ConfirmWindow.Show(Catalog.GetString("Are you sure you want to overwrite: "), "", fileCopy);
					confirmWin.Button_accept.Clicked += new EventHandler(on_overwrite_file_accepted);
				} else {
					//if multimedia_and_encoder, then copy the folder. If not checked, then copy only the db file
					if(check_backup_multimedia_and_encoder.Active)
					{
						thread = new Thread(new ThreadStart(copyRecursive));
						GLib.Idle.Add (new GLib.IdleHandler (PulseGTK));
		
						backup_doing_sensitive_start_end(true);	
						
						LogB.ThreadStart(); 
						thread.Start(); 
					} else {
						File.Copy(fileDB, fileCopy);
					
						string myString = string.Format(Catalog.GetString("Copied to {0}"), fileCopy);
						new DialogMessage(Constants.MessageTypes.INFO, myString);
					}
				}
			} 
			catch {
				string myString = string.Format(Catalog.GetString("Cannot copy to {0} "), fileCopy);
				new DialogMessage(Constants.MessageTypes.WARNING, myString);
			}
		}
		else {
			fc.Hide ();
			return ;
		}
		
		//Don't forget to call Destroy() or the FileChooserDialog window won't get closed.
		fc.Destroy();
		
	}
	
	void on_button_import_configuration_clicked (object o, EventArgs args)
	{
		fc = new Gtk.FileChooserDialog(Catalog.GetString("Import configuration file"),
				preferences_win,
				FileChooserAction.Open,
				Catalog.GetString("Cancel"),ResponseType.Cancel,
				Catalog.GetString("Import"),ResponseType.Accept
				);
		
		fc.Filter = new FileFilter();
		//it can handle future archives like: chronojump_config_SOME_VENDOR.txt
		//and it will be copied to chronojump_config.txt
		fc.Filter.AddPattern("chronojump_config*.txt");
	
		bool success = false;	
		if (fc.Run() == (int)ResponseType.Accept) 
		{
			try {
				File.Copy(fc.Filename, UtilAll.GetConfigFileName(), true);
				LogB.Information("Imported configuration");

				//will launch configInit() from gui/chronojump.cs
				FakeButtonImported.Click();

				success = true;
			} catch {
				LogB.Warning("Catched! Configuration cannot be imported");
				new DialogMessage(Constants.MessageTypes.WARNING, Catalog.GetString("Error importing data."));
			}
		}
		//Don't forget to call Destroy() or the FileChooserDialog window won't get closed.
		fc.Destroy();

		if(success)
			new DialogMessage(Constants.MessageTypes.INFO, Catalog.GetString("Successfully imported."));
	}
	
	private void on_overwrite_file_accepted(object o, EventArgs args)
	{
		try {
			//if multimedia_and_encoder, then copy the folder. If not checked, then copy only the db file
			if(check_backup_multimedia_and_encoder.Active) {
				Directory.Delete(fileCopy, true);
				thread = new Thread(new ThreadStart(copyRecursive));
				GLib.Idle.Add (new GLib.IdleHandler (PulseGTK));
		
				backup_doing_sensitive_start_end(true);	
				
				LogB.ThreadStart(); 
				thread.Start(); 
			} else {
				File.Delete(fileCopy);
				File.Copy(fileDB, fileCopy);
						
				fc.Hide ();
				string myString = string.Format(Catalog.GetString("Copied to {0}"), fileCopy);
				new DialogMessage(Constants.MessageTypes.INFO, myString);
			}
		} catch {
			string myString = string.Format(Catalog.GetString("Cannot copy to {0} "), fileCopy);
			new DialogMessage(Constants.MessageTypes.WARNING, myString);
		}
	}

	/*
	 * deprecated since 1.6.0. Use backup method below
	*/
	private void copyRecursive() {
		Util.CopyFilesRecursively(new DirectoryInfo(Util.GetParentDir(false)), new DirectoryInfo(fileCopy));
	}

	/*
	 * Temprarily disabled
	 *
	//from Longomatch
	//https://raw.githubusercontent.com/ylatuya/longomatch/master/LongoMatch.DB/CouchbaseStorage.cs
	private bool backup(string path)
	{
		try {   
			string storageName = path + Path.DirectorySeparatorChar + "chronojump_backup-" + DateTime.UtcNow.ToString() + ".tar.gz";
			using (FileStream fs = new FileStream (outputFilename, FileMode.Create, FileAccess.Write, FileShare.None)) {
				using (Stream gzipStream = new GZipOutputStream (fs)) {
					using (TarArchive tarArchive = TarArchive.CreateOutputTarArchive (gzipStream)) {
						//foreach (string n in new string[] {"", "-wal", "-shm"}) {
						//	TarEntry tarEntry = TarEntry.CreateEntryFromFile (
						//			Path.Combine (Config.DBDir, storageName + ".cblite" + n));
						//	tarArchive.WriteEntry (tarEntry, true);
						//}
						//AddDirectoryFilesToTar (tarArchive, Path.Combine (Config.DBDir, storageName + " attachments"), true);
						AddDirectoryFilesToTar (tarArchive, Util.GetParentDir(false), true);
					}
				}
			}
			//LastBackup = DateTime.UtcNow;
		} catch (Exception ex) {
			LogB.Error (ex);
			return false;
		}
		return true;
	}


	//from Longomatch
	//https://raw.githubusercontent.com/ylatuya/longomatch/master/LongoMatch.DB/CouchbaseStorage.cs
	void AddDirectoryFilesToTar (TarArchive tarArchive, string sourceDirectory, bool recurse)
	{
		// Recursively add sub-folders
		if (recurse) {
			string[] directories = Directory.GetDirectories (sourceDirectory);
			foreach (string directory in directories)
				AddDirectoryFilesToTar (tarArchive, directory, recurse);
		}

		// Add files
		string[] filenames = Directory.GetFiles (sourceDirectory);
		foreach (string filename in filenames) {
			TarEntry tarEntry = TarEntry.CreateEntryFromFile (filename);
			tarArchive.WriteEntry (tarEntry, true);
		}
	}
	*/

	//encoder
	private void on_button_inactivity_help_clicked (object o, EventArgs args)
	{
		new DialogMessage(Constants.MessageTypes.INFO, 
				Catalog.GetString("If a repetition has been found, test will end at selected inactivity seconds.") + "\n\n" +
				Catalog.GetString("If a repetition has not been found, test will end at selected inactivity seconds (x2).") + "\n" +
				Catalog.GetString("This will let the person to have more time to start movement.")
				);
	}

	
	// ---- start SQL stress tests ---->

	private void on_SQL_stress_test_safe_short_clicked (object o, EventArgs args) {
		LogB.Information("start safe short stress test ---->");
		sql_stress_test(1000);
	}
	private void on_SQL_stress_test_safe_long_clicked (object o, EventArgs args) {
		LogB.Information("start safe long stress test ---->");
		sql_stress_test(4000);
	}
	private void on_SQL_stress_test_not_safe_short_clicked (object o, EventArgs args) {
		LogB.Information("start not safe short stress test ---->");
		Sqlite.SafeClose = false;
		sql_stress_test(1000);
		Sqlite.SafeClose = true;
	}
	private void on_SQL_stress_test_not_safe_long_clicked (object o, EventArgs args) {
		LogB.Information("start not safe long stress test ---->");
		Sqlite.SafeClose = false;
		sql_stress_test(4000);
		Sqlite.SafeClose = true;
	}
	private void sql_stress_test (int times) {
		Stopwatch sw = new Stopwatch();

		sw.Start();

		//trying if new way of Sqlite.Close disposing dbcmd fixes problems when multiple open / close connection
		for(int i=0 ; i < times; i++) {
			LogB.Debug (" i=" + i.ToString());
			LogB.Debug(SqlitePreferences.Select("databaseVersion"));
		}
		sw.Stop();

		string message = "Success!" + 
			" Done " + times + " times." + 
			" Elapsed " + sw.ElapsedMilliseconds + " ms";
		LogB.Information(message);
	
		label_advanced_feedback.Text = message;
	}

	// <---- end SQL stress tests ----

	private void on_debug_mode_clicked (object o, EventArgs args) {
		//will be managed from gui/chronojump.cs
		FakeButtonDebugModeStart.Click();
	}
	public void DebugActivated() {
		label_advanced_feedback.Text = "Debug mode on while Chronojump is running.";
	}

	private void on_toggle_gc_collect_on_close_toggled(object o, EventArgs args)
	{
		if(toggle_gc_collect_on_close.Active) {
			Sqlite.GCCollect = true;
			new DialogMessage(Constants.MessageTypes.INFO, "GCCollect: ACTIVE!");
		} else {
			Sqlite.GCCollect = false;
			new DialogMessage(Constants.MessageTypes.INFO, "GCCollect: UNACTIVE! (default)");
		}
	}

	private void on_toggle_never_close_toggled(object o, EventArgs args)
	{
		if(toggle_never_close.Active) {
			Sqlite.NeverCloseDB = true;
			new DialogMessage(Constants.MessageTypes.INFO, "Never close: ACTIVE!");
		} else {
			Sqlite.NeverCloseDB = false;
			new DialogMessage(Constants.MessageTypes.INFO, "Never close: UNACTIVE! (default)");
		}
	}

	private bool PulseGTK ()
	{
		if ( ! thread.IsAlive ) {
			LogB.ThreadEnding();
			endPulse();

			LogB.ThreadEnded();
			return false;
		}
	
		pulsebar.Pulse();
		Thread.Sleep (50);
		//LogB.Debug(thread.ThreadState.ToString());
		return true;
	}

	private void endPulse() {
		pulsebar.Fraction = 1;
		backup_doing_sensitive_start_end(false);
		fc.Hide ();
		string myString = string.Format(Catalog.GetString("Copied to {0}"), fileCopy);
		new DialogMessage(Constants.MessageTypes.INFO, myString);
	}
	
	private void backup_doing_sensitive_start_end(bool start) 
	{
		hbox_backup_doing.Visible = start;
	
		button_db_backup.Sensitive = ! start;
		button_data_folder_open.Sensitive = ! start;
		
		button_cancel.Sensitive = ! start;
		button_accept.Sensitive = ! start;
	}

	//change stuff in Sqlite and in preferences object that will be retrieved by GetPreferences
	void on_button_accept_clicked (object o, EventArgs args)
	{
		Sqlite.Open();


		//appearance tab
		Preferences.MaximizedTypes maximizedTypeFromGUI = get_maximized_from_gui();
		if(preferences.maximized != maximizedTypeFromGUI)
		{
			SqlitePreferences.Update("maximized", maximizedTypeFromGUI.ToString(), true);
			preferences.maximized = maximizedTypeFromGUI;
		}

		if( preferences.personWinHide != PreferencesWindowBox.check_appearance_person_win_hide.Active ) {
			SqlitePreferences.Update("personWinHide", PreferencesWindowBox.check_appearance_person_win_hide.Active.ToString(), true);
			preferences.personWinHide = PreferencesWindowBox.check_appearance_person_win_hide.Active;
		}
		if( preferences.encoderCaptureShowOnlyBars != PreferencesWindowBox.check_appearance_encoder_only_bars.Active ) {
			SqlitePreferences.Update("encoderCaptureShowOnlyBars", PreferencesWindowBox.check_appearance_encoder_only_bars.Active.ToString(), true);
			preferences.encoderCaptureShowOnlyBars = PreferencesWindowBox.check_appearance_encoder_only_bars.Active;
		}
		
		if( preferences.digitsNumber != Convert.ToInt32(UtilGtk.ComboGetActive(combo_decimals)) ) {
			SqlitePreferences.Update("digitsNumber", UtilGtk.ComboGetActive(combo_decimals), true);
			preferences.digitsNumber = Convert.ToInt32(UtilGtk.ComboGetActive(combo_decimals));
		}
		
		if( preferences.showPower != PreferencesWindowBox.checkbutton_power.Active ) {
			SqlitePreferences.Update("showPower", PreferencesWindowBox.checkbutton_power.Active.ToString(), true);
			preferences.showPower = PreferencesWindowBox.checkbutton_power.Active;
		}

		if( preferences.showStiffness != PreferencesWindowBox.checkbutton_stiffness.Active ) {
			SqlitePreferences.Update("showStiffness", PreferencesWindowBox.checkbutton_stiffness.Active.ToString(), true);
			preferences.showStiffness = PreferencesWindowBox.checkbutton_stiffness.Active;
		}
		
		if( preferences.showInitialSpeed != PreferencesWindowBox.checkbutton_initial_speed.Active ) {
			SqlitePreferences.Update("showInitialSpeed", PreferencesWindowBox.checkbutton_initial_speed.Active.ToString(), true);
			preferences.showInitialSpeed = PreferencesWindowBox.checkbutton_initial_speed.Active;
		}

		if( preferences.showAngle != PreferencesWindowBox.checkbutton_angle.Active ) {
			SqlitePreferences.Update("showAngle", PreferencesWindowBox.checkbutton_angle.Active.ToString(), true);
			preferences.showAngle = PreferencesWindowBox.checkbutton_angle.Active;
		}
		
		if(PreferencesWindowBox.checkbutton_show_tv_tc_index.Active) {
			SqlitePreferences.Update("showQIndex", PreferencesWindowBox.radiobutton_show_q_index.Active.ToString(), true);
			SqlitePreferences.Update("showDjIndex", PreferencesWindowBox.radiobutton_show_dj_index.Active.ToString(), true);
			preferences.showQIndex = PreferencesWindowBox.radiobutton_show_q_index.Active;
			preferences.showDjIndex = PreferencesWindowBox.radiobutton_show_dj_index.Active;
		} else {
			SqlitePreferences.Update("showQIndex", "False", true);
			SqlitePreferences.Update("showDjIndex", "False", true);
			preferences.showQIndex = false;
			preferences.showDjIndex = false; 
		}
		
		
		if( preferences.askDeletion != PreferencesWindowBox.checkbutton_ask_deletion.Active ) {
			SqlitePreferences.Update("askDeletion", PreferencesWindowBox.checkbutton_ask_deletion.Active.ToString(), true);
			preferences.askDeletion = PreferencesWindowBox.checkbutton_ask_deletion.Active;
		}

		if( preferences.weightStatsPercent != PreferencesWindowBox.radio_weight_percent.Active ) {
			SqlitePreferences.Update("weightStatsPercent", PreferencesWindowBox.radio_weight_percent.Active.ToString(), true);
			preferences.weightStatsPercent = PreferencesWindowBox.radio_weight_percent.Active;
		}

		if( preferences.heightPreferred != PreferencesWindowBox.radio_elevation_height.Active ) {
			SqlitePreferences.Update("heightPreferred", PreferencesWindowBox.radio_elevation_height.Active.ToString(), true);
			preferences.heightPreferred = PreferencesWindowBox.radio_elevation_height.Active;
		}

		if( preferences.metersSecondsPreferred != PreferencesWindowBox.radio_speed_ms.Active ) {
			SqlitePreferences.Update("metersSecondsPreferred", PreferencesWindowBox.radio_speed_ms.Active.ToString(), true);
			preferences.metersSecondsPreferred = PreferencesWindowBox.radio_speed_ms.Active;
		}
		
		if( preferences.runSpeedStartArrival != PreferencesWindowBox.radio_runs_speed_start_arrival.Active ) {
			SqlitePreferences.Update("runSpeedStartArrival", PreferencesWindowBox.radio_runs_speed_start_arrival.Active.ToString(), true);
			preferences.runSpeedStartArrival = PreferencesWindowBox.radio_runs_speed_start_arrival.Active;
		}
		
		//start of double contacts stuff ----

		//1 simple runs ----
		
		//1.1 was FIRST or AVERAGE or LAST and now will be NONE
		if( (preferences.runDoubleContactsMode != Constants.DoubleContact.NONE) && 
				! PreferencesWindowBox.checkbutton_runs_prevent_double_contact.Active) 
		{
				SqlitePreferences.Update("runDoubleContactsMode", Constants.DoubleContact.NONE.ToString(), true);
				preferences.runDoubleContactsMode = Constants.DoubleContact.NONE;
		}
		else if(PreferencesWindowBox.checkbutton_runs_prevent_double_contact.Active) 
		{
			//1.2 mode has changed between FIRST, AVERAGE or LAST
			if( PreferencesWindowBox.radio_runs_prevent_double_contact_first.Active &&
					(preferences.runDoubleContactsMode != Constants.DoubleContact.FIRST) ) {
				SqlitePreferences.Update("runDoubleContactsMode", Constants.DoubleContact.FIRST.ToString(), true);
				preferences.runDoubleContactsMode = Constants.DoubleContact.FIRST;
			}
			else if( PreferencesWindowBox.radio_runs_prevent_double_contact_average.Active &&
					(preferences.runDoubleContactsMode != Constants.DoubleContact.AVERAGE) ) {
				SqlitePreferences.Update("runDoubleContactsMode", Constants.DoubleContact.AVERAGE.ToString(), true);
				preferences.runDoubleContactsMode = Constants.DoubleContact.AVERAGE;
			}
			else if( PreferencesWindowBox.radio_runs_prevent_double_contact_last.Active &&
					(preferences.runDoubleContactsMode != Constants.DoubleContact.LAST) ) {
				SqlitePreferences.Update("runDoubleContactsMode", Constants.DoubleContact.LAST.ToString(), true);
				preferences.runDoubleContactsMode = Constants.DoubleContact.LAST;
			}

			if(preferences.runDoubleContactsMS != (int) PreferencesWindowBox.spinbutton_runs_prevent_double_contact.Value) {
				SqlitePreferences.Update("runDoubleContactsMS", 
						PreferencesWindowBox.spinbutton_runs_prevent_double_contact.Value.ToString(), true); //saved as string
				preferences.runDoubleContactsMS = (int) spinbutton_runs_prevent_double_contact.Value;
			}
		}

		//2 intervallic runs ----
		
		//2.1 was FIRST or AVERAGE or LAST and now will be NONE
		if( (preferences.runIDoubleContactsMode != Constants.DoubleContact.NONE) && 
				! PreferencesWindowBox.checkbutton_runs_i_prevent_double_contact.Active) 
		{
				SqlitePreferences.Update("runIDoubleContactsMode", Constants.DoubleContact.NONE.ToString(), true);
				preferences.runIDoubleContactsMode = Constants.DoubleContact.NONE;
		}
		else if(PreferencesWindowBox.checkbutton_runs_i_prevent_double_contact.Active) 
		{
			//2.2 mode has changed between FIRST, AVERAGE or LAST
			if( PreferencesWindowBox.radio_runs_i_prevent_double_contact_first.Active &&
					(preferences.runIDoubleContactsMode != Constants.DoubleContact.FIRST) ) {
				SqlitePreferences.Update("runIDoubleContactsMode", Constants.DoubleContact.FIRST.ToString(), true);
				preferences.runIDoubleContactsMode = Constants.DoubleContact.FIRST;
			}
			else if( PreferencesWindowBox.radio_runs_i_prevent_double_contact_average.Active &&
					(preferences.runIDoubleContactsMode != Constants.DoubleContact.AVERAGE) ) {
				SqlitePreferences.Update("runIDoubleContactsMode", Constants.DoubleContact.AVERAGE.ToString(), true);
				preferences.runIDoubleContactsMode = Constants.DoubleContact.AVERAGE;
			}
			else if( PreferencesWindowBox.radio_runs_i_prevent_double_contact_last.Active &&
					(preferences.runIDoubleContactsMode != Constants.DoubleContact.LAST) ) {
				SqlitePreferences.Update("runIDoubleContactsMode", Constants.DoubleContact.LAST.ToString(), true);
				preferences.runIDoubleContactsMode = Constants.DoubleContact.LAST;
			}
			
			if(preferences.runIDoubleContactsMS != (int) PreferencesWindowBox.spinbutton_runs_i_prevent_double_contact.Value) {
				SqlitePreferences.Update("runIDoubleContactsMS", 
						PreferencesWindowBox.spinbutton_runs_i_prevent_double_contact.Value.ToString(), true); //saved as string
				preferences.runIDoubleContactsMS = (int) spinbutton_runs_i_prevent_double_contact.Value;
			}
		}

		//---- end of double contacts stuff
		
		//encoder capture ----
	
		preferences.encoderCaptureTime = preferencesChange(
				"encoderCaptureTime",
				preferences.encoderCaptureTime,
				(int) PreferencesWindowBox.spin_encoder_capture_time.Value);

		preferences.encoderCaptureInactivityEndTime = preferencesChange(
				"encoderCaptureInactivityEndTime",
				preferences.encoderCaptureInactivityEndTime,
				(int) PreferencesWindowBox.spin_encoder_capture_inactivity_end_time.Value);

	
		Constants.EncoderVariablesCapture mainVariable = Constants.SetEncoderVariablesCapture(
				UtilGtk.ComboGetActive(PreferencesWindowBox.combo_main_variable));
		if( preferences.encoderCaptureMainVariable != mainVariable ) {
			SqlitePreferences.Update("encoderCaptureMainVariable", mainVariable.ToString(), true);
			preferences.encoderCaptureMainVariable = mainVariable;
		}
		
		preferences.encoderCaptureMinHeightGravitatory = preferencesChange(
				"encoderCaptureMinHeightGravitatory",
				preferences.encoderCaptureMinHeightGravitatory,
				(int) PreferencesWindowBox.spin_encoder_capture_min_height_gravitatory.Value);
	
		preferences.encoderCaptureMinHeightInertial = preferencesChange(
				"encoderCaptureMinHeightInertial",
				preferences.encoderCaptureMinHeightInertial,
				(int) PreferencesWindowBox.spin_encoder_capture_min_height_inertial.Value);

		preferences.encoderCaptureCheckFullyExtended = preferencesChange(
				"encoderCaptureCheckFullyExtended",
				preferences.encoderCaptureCheckFullyExtended,
				PreferencesWindowBox.checkbutton_encoder_capture_fully_extended.Active);
		
		preferences.encoderCaptureCheckFullyExtendedValue = preferencesChange(
				"encoderCaptureCheckFullyExtendedValue",
				preferences.encoderCaptureCheckFullyExtendedValue,
				(int) PreferencesWindowBox.spin_encoder_capture_fully_extended.Value);

		if(PreferencesWindowBox.radio_encoder_auto_save_curve_best.Active) {
			SqlitePreferences.Update("encoderAutoSaveCurve", Constants.EncoderAutoSaveCurve.BEST.ToString(), true);
			preferences.encoderAutoSaveCurve = Constants.EncoderAutoSaveCurve.BEST;
		}
		else if(PreferencesWindowBox.radio_encoder_auto_save_curve_4top.Active) {
			SqlitePreferences.Update("encoderAutoSaveCurve", Constants.EncoderAutoSaveCurve.FROM4TOPENULTIMATE.ToString(), true);
			preferences.encoderAutoSaveCurve = Constants.EncoderAutoSaveCurve.FROM4TOPENULTIMATE;
		}
		else if(PreferencesWindowBox.radio_encoder_auto_save_curve_all.Active) {
			SqlitePreferences.Update("encoderAutoSaveCurve", Constants.EncoderAutoSaveCurve.ALL.ToString(), true);
			preferences.encoderAutoSaveCurve = Constants.EncoderAutoSaveCurve.ALL;
		}
		else {
			SqlitePreferences.Update("encoderAutoSaveCurve", Constants.EncoderAutoSaveCurve.NONE.ToString(), true);
			preferences.encoderAutoSaveCurve = Constants.EncoderAutoSaveCurve.NONE;
		}

		preferences.encoderShowStartAndDuration = preferencesChange(
				"encoderShowStartAndDuration",
				preferences.encoderShowStartAndDuration,
				PreferencesWindowBox.check_show_start_and_duration.Active);
		

		//---- end of encoder capture
		
		//encoder other ----
		
		preferences.encoderPropulsive = preferencesChange(
				"encoderPropulsive",
				preferences.encoderPropulsive,
				PreferencesWindowBox.checkbutton_encoder_propulsive.Active);
		
		preferences.encoderSmoothCon = preferencesChange(
				"encoderSmoothCon",
				preferences.encoderSmoothCon,
				(double) PreferencesWindowBox.spin_encoder_smooth_con.Value);
		
		Constants.Encoder1RMMethod encoder1RMMethod;
		if(PreferencesWindowBox.radio_encoder_1RM_nonweighted.Active)
			encoder1RMMethod = Constants.Encoder1RMMethod.NONWEIGHTED;
		else if(PreferencesWindowBox.radio_encoder_1RM_weighted.Active)
			encoder1RMMethod = Constants.Encoder1RMMethod.WEIGHTED;
		else if(PreferencesWindowBox.radio_encoder_1RM_weighted2.Active)
			encoder1RMMethod = Constants.Encoder1RMMethod.WEIGHTED2;
		else // (PreferencesWindowBox.radio_encoder_1RM_weighted3.Active)
			encoder1RMMethod = Constants.Encoder1RMMethod.WEIGHTED3;

		SqlitePreferences.Update("encoder1RMMethod", encoder1RMMethod.ToString(), true);
		preferences.encoder1RMMethod = encoder1RMMethod;
		
		//---- end of encoder other
		
		//multimedia ----
		if( preferences.volumeOn != PreferencesWindowBox.checkbutton_volume.Active ) {
			SqlitePreferences.Update("volumeOn", PreferencesWindowBox.checkbutton_volume.Active.ToString(), true);
			preferences.volumeOn = PreferencesWindowBox.checkbutton_volume.Active;
		}

		if( preferences.videoDeviceNum != UtilGtk.ComboGetActivePos(combo_camera) ) {
			SqlitePreferences.Update("videoDevice", UtilGtk.ComboGetActivePos(combo_camera).ToString(), true);
			preferences.videoDeviceNum = UtilGtk.ComboGetActivePos(combo_camera);
		}
		

		if(PreferencesWindowBox.radio_export_latin.Active) {
			SqlitePreferences.Update("CSVExportDecimalSeparator","COMMA", true); 
			preferences.CSVExportDecimalSeparator = "COMMA";
		}
		else {
			SqlitePreferences.Update("CSVExportDecimalSeparator","POINT", true); 
			preferences.CSVExportDecimalSeparator = "POINT";
		}
	
		string selectedLanguage = getSelectedLanguage();

		//if there was a language on SQL but now "detected" is selected, put "" in language on SQL
		if(preferences.language != "" && radio_language_detected.Active) {
			SqlitePreferences.Update("language", "", true);
			preferences.language = "";
		}
		//if force a language, and SQL language is != than selected language, change language on SQL
		else if(radio_language_force.Active && preferences.language != selectedLanguage) {
			SqlitePreferences.Update("language", selectedLanguage, true);
			preferences.language = selectedLanguage;
		}


		if( preferences.RGraphsTranslate != PreferencesWindowBox.radio_graphs_translate.Active ) {
			SqlitePreferences.Update("RGraphsTranslate", 
					PreferencesWindowBox.radio_graphs_translate.Active.ToString(), true);
			preferences.RGraphsTranslate = PreferencesWindowBox.radio_graphs_translate.Active;
		}

		if( preferences.useHeightsOnJumpIndexes != PreferencesWindowBox.radio_use_heights_on_jump_indexes.Active ) {
			SqlitePreferences.Update("useHeightsOnJumpIndexes", 
					PreferencesWindowBox.radio_use_heights_on_jump_indexes.Active.ToString(), true);
			preferences.useHeightsOnJumpIndexes = PreferencesWindowBox.radio_use_heights_on_jump_indexes.Active;
		}


		Sqlite.Close();

		PreferencesWindowBox.preferences_win.Hide();
		PreferencesWindowBox = null;
	}

	private Preferences.MaximizedTypes get_maximized_from_gui()
	{
		if( ! PreferencesWindowBox.check_appearance_maximized.Active )
			return Preferences.MaximizedTypes.NO;

		if( ! PreferencesWindowBox.check_appearance_maximized_undecorated.Active )
			return Preferences.MaximizedTypes.YES;

		return Preferences.MaximizedTypes.YESUNDECORATED;
	}

	private bool preferencesChange(string prefName, bool prefValue, bool bNew) 
	{
		if(prefValue != bNew)
			SqlitePreferences.Update(prefName, bNew.ToString(), true);
		
		return bNew;
	}
	private int preferencesChange(string prefName, int prefValue, int iNew) 
	{
		if(prefValue != iNew)
			SqlitePreferences.Update(prefName, iNew.ToString(), true);
		
		return iNew;
	}
	private double preferencesChange(string prefName, double prefValue, double dNew) 
	{
		if(prefValue != dNew)
			SqlitePreferences.Update(prefName, Util.ConvertToPoint(dNew), true);
		
		return dNew;
	}

	public Button Button_accept 
	{
		set { button_accept = value; }
		get { return button_accept;  }
	}

	public Preferences GetPreferences 
	{
		get { return preferences;  }
	}

}
