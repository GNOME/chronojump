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
 * Copyright (C) 2004-2014   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
using System.IO; 
using System.IO.Ports;
using Gtk;
using Gdk;
using Glade;
using System.Collections;
using System.Threading;
using Mono.Unix;
using System.Linq;
using RDotNet;


public partial class ChronoJumpWindow 
{
	[Widget] Gtk.SpinButton spin_encoder_extra_weight;
	[Widget] Gtk.Label label_encoder_displaced_weight;
	[Widget] Gtk.Label label_encoder_1RM_percent;

	[Widget] Gtk.Label label_encoder_selected;	
	
	//this is Kg*cm^2 because there's limitation of Glade on 3 decimals. 
	//at SQL it's in Kg*cm^2 also because it's stored as int
	//at graph.R is converted to Kg*m^2 ( /10000 )
	//[Widget] Gtk.SpinButton spin_encoder_capture_inertial; 
	
	[Widget] Gtk.Box hbox_encoder_capture_wait;
	[Widget] Gtk.Box hbox_encoder_capture_doing;
	[Widget] Gtk.Button button_encoder_capture;
	[Widget] Gtk.Image image_encoder_bell;
	[Widget] Gtk.Button button_encoder_capture_cancel;
	[Widget] Gtk.Button button_encoder_capture_finish;
	[Widget] Gtk.Button button_encoder_recalculate;
	[Widget] Gtk.Button button_encoder_load_signal;
	[Widget] Gtk.Button button_video_play_this_test_encoder;
	[Widget] Gtk.Viewport viewport_image_encoder_capture;
	[Widget] Gtk.Image image_encoder_capture;
	[Widget] Gtk.Image image_encoder_capture_open;
	[Widget] Gtk.ProgressBar encoder_pulsebar_capture;
	//[Widget] Gtk.Entry entry_encoder_signal_comment;
	//[Widget] Gtk.Entry entry_encoder_curve_comment;
	//[Widget] Gtk.Button button_encoder_save_curve;
	[Widget] Gtk.Button button_encoder_export_all_curves;
	[Widget] Gtk.Label label_encoder_curve_action;
	[Widget] Gtk.Button button_encoder_delete_signal;
	
	[Widget] Gtk.Notebook notebook_encoder_sup;
	[Widget] Gtk.Notebook notebook_encoder_capture;
	
	[Widget] Gtk.Box hbox_combo_encoder_exercise;
	[Widget] Gtk.ComboBox combo_encoder_exercise;
	[Widget] Gtk.Box hbox_combo_encoder_eccon;
	[Widget] Gtk.ComboBox combo_encoder_eccon;
	[Widget] Gtk.Box hbox_combo_encoder_laterality;
	[Widget] Gtk.ComboBox combo_encoder_laterality;
	[Widget] Gtk.Box hbox_encoder_capture_curves_save_all_none;

	[Widget] Gtk.Box hbox_combo_encoder_analyze_cross;
	[Widget] Gtk.ComboBox combo_encoder_analyze_cross;
	
	[Widget] Gtk.Box hbox_encoder_analyze_show_powerbars;
	[Widget] Gtk.CheckButton check_encoder_analyze_show_time_to_peak_power;
	[Widget] Gtk.CheckButton check_encoder_analyze_show_range;

	[Widget] Gtk.Box hbox_encoder_analyze_show_SAFE;
	[Widget] Gtk.CheckButton check_encoder_analyze_show_speed;
	[Widget] Gtk.CheckButton check_encoder_analyze_show_accel;
	[Widget] Gtk.CheckButton check_encoder_analyze_show_force;
	[Widget] Gtk.CheckButton check_encoder_analyze_show_power;
	
	[Widget] Gtk.Button button_encoder_analyze;
	[Widget] Gtk.Box hbox_encoder_analyze_progress;
	[Widget] Gtk.Button button_encoder_analyze_cancel;
	[Widget] Gtk.Box hbox_encoder_user_curves;
	[Widget] Gtk.Label label_encoder_user_curves_active_num;
	[Widget] Gtk.Label label_encoder_user_curves_all_num;
	[Widget] Gtk.Box hbox_encoder_analyze_data_compare;
	[Widget] Gtk.ComboBox combo_encoder_analyze_data_compare;
	[Widget] Gtk.Button button_encoder_analyze_data_compare;
	
	[Widget] Gtk.Button button_encoder_analyze_image_save;
	[Widget] Gtk.Button button_encoder_analyze_table_save;
	[Widget] Gtk.Button button_encoder_analyze_1RM_save;

	[Widget] Gtk.CheckButton check_encoder_analyze_signal_or_curves;
	[Widget] Gtk.Image image_encoder_analyze_current_signal;
	[Widget] Gtk.Image image_encoder_analyze_saved_curves;
	[Widget] Gtk.Label label_encoder_analyze_current_signal;
	[Widget] Gtk.Label label_encoder_analyze_saved_curves;
	
	[Widget] Gtk.RadioButton radiobutton_encoder_analyze_powerbars;
	[Widget] Gtk.RadioButton radiobutton_encoder_analyze_cross;
	[Widget] Gtk.RadioButton radiobutton_encoder_analyze_single;
	[Widget] Gtk.RadioButton radiobutton_encoder_analyze_side;
	[Widget] Gtk.RadioButton radiobutton_encoder_analyze_neuromuscular_profile;
	[Widget] Gtk.Image image_encoder_analyze_powerbars;
	[Widget] Gtk.Image image_encoder_analyze_cross;
	[Widget] Gtk.Image image_encoder_analyze_side;
	[Widget] Gtk.Image image_encoder_analyze_single;
	[Widget] Gtk.Image image_encoder_analyze_nmp;
	
	[Widget] Gtk.Button button_encoder_analyze_help;


	//[Widget] Gtk.RadioButton radiobutton_encoder_analyze_superpose;
	[Widget] Gtk.CheckButton check_encoder_analyze_eccon_together;
	[Widget] Gtk.Image image_encoder_analyze_eccon_together;
	[Widget] Gtk.Image image_encoder_analyze_eccon_separated;
	
	[Widget] Gtk.Image image_encoder_analyze_speed;
	[Widget] Gtk.Image image_encoder_analyze_accel;
	[Widget] Gtk.Image image_encoder_analyze_force;
	[Widget] Gtk.Image image_encoder_analyze_power;
	
	[Widget] Gtk.Image image_encoder_analyze_mean;
	[Widget] Gtk.Image image_encoder_analyze_max;
	[Widget] Gtk.Image image_encoder_analyze_range;
	[Widget] Gtk.Image image_encoder_analyze_time_to_pp;

	[Widget] Gtk.Box hbox_encoder_analyze_curve_num;
	[Widget] Gtk.Box hbox_combo_encoder_analyze_curve_num_combo;
	[Widget] Gtk.ComboBox combo_encoder_analyze_curve_num_combo;
	[Widget] Gtk.Label label_encoder_analyze_side_max;

	[Widget] Gtk.CheckButton check_encoder_analyze_mean_or_max;
	
	[Widget] Gtk.Viewport viewport_image_encoder_analyze;
	[Widget] Gtk.Image image_encoder_analyze;
	[Widget] Gtk.ProgressBar encoder_pulsebar_analyze;
	
	[Widget] Gtk.TreeView treeview_encoder_capture_curves;
	[Widget] Gtk.TreeView treeview_encoder_analyze_curves;
	
	[Widget] Gtk.DrawingArea encoder_capture_signal_drawingarea;
	[Widget] Gtk.DrawingArea encoder_capture_curves_bars_drawingarea;
	Gdk.Pixmap encoder_capture_signal_pixmap = null;
	Gdk.Pixmap encoder_capture_curves_bars_pixmap = null;

	ArrayList encoderCaptureCurves;
        Gtk.ListStore encoderCaptureListStore;
	Gtk.ListStore encoderAnalyzeListStore; //can be EncoderCurves or EncoderNeuromuscularData

	Thread encoderThread;
	

	int image_encoder_width;
	int image_encoder_height;

	private string encoderAnalysis="powerBars";
	private string ecconLast;
	private string encoderTimeStamp;
	private string encoderSignalUniqueID;

	private ArrayList encoderCompareInterperson;	//personID:personName
	private ArrayList encoderCompareIntersession;	//sessionID:sessionDate

	private static double [] encoderReaded;		//data coming from encoder and converted (can be double)
	private static int encoderCaptureCountdown;
	private static Gdk.Point [] encoderCapturePoints;		//stored to be realtime displayed
	private static int encoderCapturePointsCaptured;		//stored to be realtime displayed
	private static int encoderCapturePointsPainted;			//stored to be realtime displayed
	
	//Contains curves captured to be analyzed by R
	private static EncoderCaptureCurveArray ecca;
	private static bool eccaCreated = false;

	private static bool encoderProcessCancel;
	private static bool encoderProcessProblems;
	private static bool encoderProcessFinish;

	EncoderCaptureOptionsWindow encoderCaptureOptionsWin;
	EncoderConfigurationWindow encoder_configuration_win;

	EncoderConfiguration encoderConfigurationCurrent;

	/* 
	 * this contains last EncoderSQL captured, recalculated or loaded
	 * 
	 * before using this, saving a curve used the combo values on the top,
	 * but this combo values can be changed by the user, and the he could click on save curve,
	 * then power values (results of curves on graph.R) can be saved with bad weight, exerciseID, ...
	 *
	 * Now, with lastEncoderSQLSignal, saved curves and export curves will take the weight, exerciseID, ...
	 * last capture, recalculate and load. Better usability
	 */
	EncoderSQL lastEncoderSQLSignal;

	/*
	 * CAPTURE is the capture from csharp (not from external python)
	 * CAPTURE_EXTERNAL is deprecated (from Python)
	 *
	 * difference between:
	 * CURVES: calcule and recalculate, autosaves the signal at end
	 * LOAD curves does snot
	 *
	 * CAPTURE_IM records to get the inertia moment but does not calculate curves in R and not updates the treeview
	 * CURVES_AC (After Capture) is like curves but does not start a new thread (uses same pulse as capture)
	 */
	enum encoderActions { CAPTURE, CAPTURE_EXTERNAL, CURVES, CURVES_AC, LOAD, ANALYZE, CAPTURE_IM, CURVES_IM } 
	
	//STOPPING is used to stop the camera. It has to be called only one time
	enum encoderCaptureProcess { CAPTURING, STOPPING, STOPPED } 
	static encoderCaptureProcess capturingCsharp;	

	/* 
	 *
	 * To understand this class threads amnd GUI, see diagram:
	 * encoder-threads.dia
	 *
	 */

	enum encoderSensEnum { 
		NOSESSION, NOPERSON, YESPERSON, PROCESSINGCAPTURE, PROCESSINGR, DONENOSIGNAL, DONEYESSIGNAL, SELECTEDCURVE }
	encoderSensEnum encoderSensEnumStored; //tracks how was sensitive before PROCESSINGCAPTURE or PROCESSINGR
	
	//for writing text
	Pango.Layout layout_encoder_capture_signal;
	Pango.Layout layout_encoder_capture_curves_bars;
	Pango.Layout layout_encoder_capture_curves_bars_text; //e, c

 
	Gdk.GC pen_black_encoder_capture;
	Gdk.GC pen_azul_encoder_capture;
	Gdk.GC pen_green_encoder_capture;
	Gdk.GC pen_red_encoder_capture;
	Gdk.GC pen_white_encoder_capture;
	Gdk.GC pen_selected_encoder_capture;

	//TODO:put zoom,unzoom (at side of delete curve)  in capture curves (for every curve)
	//TODO: treeview on analyze (doing in separated window)
	//
	//TODO: on session load, show encoder stuff
	//
	//TODO: capture also with webcam an attach it to signal or curve
	//
	//TODO: peak power in eccentric in absolute values
	//
	//TODO: on cross, spline and force speed and power speed should have a spar value higher, like 0.7. On the other hand, the other cross graphs, haveload(mass) in the X lot more discrete, there is good to put 0.5
	//TODO: put also the Load as Load(mass) or viceversa, and put the units on the xlab, ylab
	//TODO: put a save graph and a html report
	


	Constants.Status RInitialized;	
	
	private void encoderInitializeStuff() {
		encoder_pulsebar_capture.Fraction = 1;
		encoder_pulsebar_capture.Text = "";
		encoder_pulsebar_analyze.Fraction = 1;
		encoder_pulsebar_analyze.Text = "";

		//default values	
		encoderConfigurationCurrent = new EncoderConfiguration();
		label_encoder_selected.Text = encoderConfigurationCurrent.code; 
		
		encoderCaptureListStore = new Gtk.ListStore (typeof (EncoderCurve));

		//the glade cursor_changed does not work on mono 1.2.5 windows
		//treeview_encoder_capture_curves.CursorChanged += on_treeview_encoder_capture_curves_cursor_changed;
		//changed, now unselectable because there are the checkboxes

		createEncoderCombos();
		
		//spin_encoder_capture_inertial.Value = Convert.ToDouble(Util.ChangeDecimalSeparator(
		//			SqlitePreferences.Select("inertialmomentum")));
		
		encoderCaptureOptionsWin = EncoderCaptureOptionsWindow.Create(repetitiveConditionsWin);
		encoderCaptureOptionsWin.FakeButtonClose.Clicked += new EventHandler(on_encoder_capture_options_closed);

		captureCurvesBarsData = new ArrayList(0);
		
		RInitialized = Constants.Status.UNSTARTED;
	}

	void on_menuitem_test_rdotnet_activate (object o, EventArgs args) {
		if(RInitialized == Constants.Status.UNSTARTED)
			rengine = UtilEncoder.RunEncoderCaptureCsharpInitializeR(rengine, out RInitialized);

		if(RInitialized == Constants.Status.OK)
			new DialogMessage(Constants.MessageTypes.INFO, "RDotNet OK");
		else
			new DialogMessage(Constants.MessageTypes.WARNING, "RDotNet does not work");
	}
	

	void on_button_encoder_select_clicked (object o, EventArgs args) {
		encoder_configuration_win = EncoderConfigurationWindow.View(encoderConfigurationCurrent);
		encoder_configuration_win.Button_accept.Clicked += new EventHandler(on_encoder_configuration_win_accepted);
		encoder_configuration_win.Button_encoder_capture_inertial_do.Clicked += 
			new EventHandler(on_encoder_configuration_win_capture_inertial_do);
		encoder_configuration_win.Button_encoder_capture_inertial_cancel.Clicked += 
			new EventHandler(on_button_encoder_cancel_clicked);
		//encoder_configuration_win.Button_encoder_capture_inertial_finish.Clicked += 
		//	new EventHandler(on_button_encoder_capture_finish_clicked);
	}

	void on_encoder_configuration_win_accepted (object o, EventArgs args) {
		encoder_configuration_win.Button_accept.Clicked -= new EventHandler(on_encoder_configuration_win_accepted);
		
		encoderConfigurationCurrent = encoder_configuration_win.GetAcceptedValues();
		label_encoder_selected.Text = encoderConfigurationCurrent.code;
	}
	
	void on_encoder_configuration_win_capture_inertial_do (object o, EventArgs args) {
		//need this "-=" in order to do not open the port two times on function:
		//on_button_encoder_capture_calcule_im();
		encoder_configuration_win.Button_encoder_capture_inertial_do.Clicked -= 
			new EventHandler(on_encoder_configuration_win_capture_inertial_do);
		
		on_button_encoder_capture_calcule_im();
	}
		
	void on_button_encoder_capture_options_clicked (object o, EventArgs args) {
		encoderCaptureOptionsWin.View(repetitiveConditionsWin, preferences.volumeOn);
	}
	
	private void on_encoder_capture_options_closed(object o, EventArgs args) {
		Log.WriteLine("closed");
		//update the bars graph because main variable maybe has changed
	}
	
	private void on_button_encoder_bells_clicked(object o, EventArgs args) {
		repetitiveConditionsWin.View(Constants.BellModes.ENCODER, preferences.volumeOn);
	}

	private bool encoderCheckPort()	{
		if(
				chronopicWin.GetEncoderPort() == "" ||
				chronopicWin.GetEncoderPort() == Util.GetDefaultPort()) 
		{
			new DialogMessage(Constants.MessageTypes.WARNING, 
					Catalog.GetString("Chronopic port is not configured."));
			/*
			UtilGtk.ChronopicColors(viewport_chronopic_encoder, 
					label_chronopic_encoder, new Gtk.Label(),
					false);
					*/
			createChronopicWindow(true);
			return false;
		}
		return true;
	}

	void on_button_encoder_capture_clicked (object o, EventArgs args) 
	{
		if(! encoderCheckPort())
			return;

		/*
		 * DEPRECATED
		string analysisOptions = getEncoderAnalysisOptions(true);

		double heightHigherCondition = -1;
		if(repetitiveConditionsWin.EncoderHeightHigher)		
			heightHigherCondition = repetitiveConditionsWin.EncoderHeightHigherValue;
		double heightLowerCondition = -1;
		if(repetitiveConditionsWin.EncoderHeightLower)		
			heightLowerCondition = repetitiveConditionsWin.EncoderHeightLowerValue;
	
		double meanSpeedHigherCondition = -1;
		if(repetitiveConditionsWin.EncoderMeanSpeedHigher)		
			meanSpeedHigherCondition = repetitiveConditionsWin.EncoderMeanSpeedHigherValue;
		double meanSpeedLowerCondition = -1;
		if(repetitiveConditionsWin.EncoderMeanSpeedLower)		
			meanSpeedLowerCondition = repetitiveConditionsWin.EncoderMeanSpeedLowerValue;
	
		double maxSpeedHigherCondition = -1;
		if(repetitiveConditionsWin.EncoderMaxSpeedHigher)		
			maxSpeedHigherCondition = repetitiveConditionsWin.EncoderMaxSpeedHigherValue;
		double maxSpeedLowerCondition = -1;
		if(repetitiveConditionsWin.EncoderMaxSpeedLower)		
			maxSpeedLowerCondition = repetitiveConditionsWin.EncoderMaxSpeedLowerValue;
	
		int powerHigherCondition = -1;
		if(repetitiveConditionsWin.EncoderPowerHigher)		
			powerHigherCondition = repetitiveConditionsWin.EncoderPowerHigherValue;
		int powerLowerCondition = -1;
		if(repetitiveConditionsWin.EncoderPowerLower)		
			powerLowerCondition = repetitiveConditionsWin.EncoderPowerLowerValue;
		
		int peakPowerHigherCondition = -1;
		if(repetitiveConditionsWin.EncoderPeakPowerHigher)		
			peakPowerHigherCondition = repetitiveConditionsWin.EncoderPeakPowerHigherValue;
		int peakPowerLowerCondition = -1;
		if(repetitiveConditionsWin.EncoderPeakPowerLower)		
			peakPowerLowerCondition = repetitiveConditionsWin.EncoderPeakPowerLowerValue;

		string exerciseNameShown = UtilGtk.ComboGetActive(combo_encoder_exercise);

		//capture data (Python)
		EncoderParams ep = new EncoderParams(
				(int) encoderCaptureOptionsWin.spin_encoder_capture_time.Value, 
				(int) encoderCaptureOptionsWin.spin_encoder_capture_min_height.Value, 
				getExercisePercentBodyWeightFromCombo (),
				Util.ConvertToPoint(findMass(Constants.MassType.DISPLACED)),
				Util.ConvertToPoint(encoderSmoothCon),			//R decimal: '.'
				findEccon(true),					//force ecS (ecc-conc separated)
				analysisOptions,
				heightHigherCondition, heightLowerCondition,
				meanSpeedHigherCondition, meanSpeedLowerCondition,
				maxSpeedHigherCondition, maxSpeedLowerCondition,
				powerHigherCondition, powerLowerCondition,
				peakPowerHigherCondition, peakPowerLowerCondition,
				encoderCaptureOptionsWin.GetMainVariable()//,
				//checkbutton_encoder_capture_inverted.Active
				); 

		EncoderStruct es = new EncoderStruct(
				"",					//no data input
				"",					//no graph ouptut
				UtilEncoder.GetEncoderDataTempFileName(), 	//OutputData1
				"", 					//OutputData2
				"", 					//SpecialData
				ep);				
				
		//Update inertia momentum of encoder if needed
		//SqlitePreferences.Update("inertialmomentum", 
		//		Util.ConvertToPoint((double) spin_encoder_capture_inertial.Value), false);

		if (encoderCaptureOptionsWin.radiobutton_encoder_capture_external.Active) {
			encoderStartVideoRecord();
		
			//wait to ensure label "Rec" has been shown
			//Thread.Sleep(100);	
			//Does not work. Basically it records, but Rec message is not shown because we would need to open a new thread here
			
			//title to sen to python software has to be without spaces
			UtilEncoder.RunEncoderCapturePython( 
					Util.ChangeSpaceAndMinusForUnderscore(currentPerson.Name) + "----" + 
					Util.ChangeSpaceAndMinusForUnderscore(exerciseNameShown) + "----(" + 
					Util.ConvertToPoint(findMass(Constants.MassType.DISPLACED)) + "Kg)",
					es, chronopicWin.GetEncoderPort());
			
			//entry_encoder_signal_comment.Text = "";
			
			encoderStopVideoRecord();
			
			encoderCalculeCurves(encoderActions.CAPTURE_EXTERNAL); //deprecated
		}
		else if (encoderCaptureOptionsWin.radiobutton_encoder_capture_safe.Active) {
		*/
			//tis notebook has capture (signal plotting), and curves (shows R graph)	
			if(notebook_encoder_capture.CurrentPage == 1)
				notebook_encoder_capture.PrevPage();

			Log.WriteLine("AAAAAAAAAAAAAAA");
			
			encoderProcessFinish = false;
			encoderThreadStart(encoderActions.CAPTURE);
			
			//entry_encoder_signal_comment.Text = "";

			Log.WriteLine("ZZZZZZZZZZZZZZZ");
		//}
	}
	
	void on_button_encoder_capture_calcule_im () 
	{
		if(! encoderCheckPort())
			return;
	
		encoder_configuration_win.Button_encoder_capture_inertial_do_chronopic_ok();
		
		//tis notebook has capture (signal plotting), and curves (shows R graph)	
		if(notebook_encoder_capture.CurrentPage == 1)
			notebook_encoder_capture.PrevPage();
		
		encoderProcessFinish = false;
		encoderThreadStart(encoderActions.CAPTURE_IM);
	}


	void on_combo_encoder_exercise_changed (object o, EventArgs args) {
		if(UtilGtk.ComboGetActive(combo_encoder_exercise) != "") //needed because encoder_exercise_edit updates this combo and can be without values in the changing process
			encoder_change_displaced_weight_and_1RM ();
	}
	void on_spin_encoder_extra_weight_value_changed (object o, EventArgs args) {
		encoder_change_displaced_weight_and_1RM ();
	}

	void encoder_change_displaced_weight_and_1RM () {
		//displaced weight
		label_encoder_displaced_weight.Text = (findMass(Constants.MassType.DISPLACED)).ToString();

		//1RM
		ArrayList array1RM = SqliteEncoder.Select1RM(
				false, currentPerson.UniqueID, currentSession.UniqueID, getExerciseIDFromCombo(), false); 
		double load1RM = 0;
		if(array1RM.Count > 0)
			load1RM = ((Encoder1RM) array1RM[0]).load1RM; //take only the first in array (will be the last uniqueID)

		if(load1RM == 0 || findMass(Constants.MassType.EXTRA) == 0)
			label_encoder_1RM_percent.Text = "";
		else
			label_encoder_1RM_percent.Text = Util.TrimDecimals(
					(100 * findMass(Constants.MassType.EXTRA) / ( load1RM * 1.0 )).ToString(), 2);
	}

	void on_button_encoder_1RM_win_clicked (object o, EventArgs args) {
		ArrayList array1RM = SqliteEncoder.Select1RM(
				false, currentPerson.UniqueID, currentSession.UniqueID, getExerciseIDFromCombo(), true); 
		
		ArrayList dataPrint = new ArrayList();
		foreach(Encoder1RM e1RM in array1RM) {
			dataPrint.Add(e1RM.ToStringArray2());
		}

		string [] columnsString = {
			"ID",
			Catalog.GetString("Person"),
			Catalog.GetString("Exercise"),
			Catalog.GetString("Load 1RM")
		};

		ArrayList bigArray = new ArrayList();
		ArrayList a1 = new ArrayList();

		//0 is the widgget to show; 1 is the editable; 2 id default value
		a1.Add(Constants.GenericWindowShow.TREEVIEW); a1.Add(true); a1.Add("");
		bigArray.Add(a1);
	
		genericWin = GenericWindow.Show(false,	//don't show now
				string.Format(Catalog.GetString("Saved 1RM values of athlete {0} on this session."), 
					currentPerson.Name) + "\n" + 
				Catalog.GetString("If you want to delete a row, right click on it.") + "\n" + 
				Catalog.GetString("If there is more than one value for an exercise,\nthe used value is the top one."),
				bigArray);

		genericWin.SetTreeview(columnsString, false, dataPrint, new ArrayList(), Constants.ContextMenu.DELETE, false);
	
		//find all persons in current session
		ArrayList personsPre = SqlitePersonSession.SelectCurrentSessionPersons(currentSession.UniqueID);
		string [] persons = new String[personsPre.Count];
		int count = 0;
	        foreach	(Person p in personsPre)
			persons[count++] = p.UniqueID.ToString() + ":" + p.Name;
		
		genericWin.ShowButtonCancel(false);
		genericWin.SetButtonAcceptSensitive(true);
		genericWin.SetButtonCancelLabel(Catalog.GetString("Close"));
		//manage selected, unselected curves
		genericWin.Button_accept.Clicked += new EventHandler(on_spin_encoder_extra_weight_value_changed);
		genericWin.Button_row_delete.Clicked += new EventHandler(on_encoder_1RM_win_row_delete);

		//used when we don't need to read data, 
		//and we want to ensure next window will be created at needed size
		//genericWin.DestroyOnAccept=true;
		//here is comented because we are going to read the checkboxes

		genericWin.ShowNow();
	}

	protected void on_encoder_1RM_win_row_delete (object o, EventArgs args) {
		Log.WriteLine("row delete at encoder 1RM");

		int uniqueID = genericWin.TreeviewSelectedUniqueID;
		Log.WriteLine(uniqueID.ToString());

		Sqlite.Delete(false, Constants.Encoder1RMTable, Convert.ToInt32(uniqueID));
	}
	
	//action can be CURVES_AC (After Capture) (where signal does not exists, need to define it)
	//CAPTURE_EXTERNAL, CURVES, LOAD (signal is defined)
	//CAPTURE_EXTERNAL is not implemented because it's deprecated
	void encoderCalculeCurves(encoderActions action) {
		if(action == encoderActions.CURVES_AC) 
		{
			encoderTimeStamp = UtilDate.ToFile(DateTime.Now);
			encoderSignalUniqueID = "-1"; //mark to know that there's no ID for this until it's saved on database
			encoderThreadStart(action);
		} else {
			//calculate and recalculate saves the curve at end
			//load does not save the curve 
		       if(File.Exists(UtilEncoder.GetEncoderDataTempFileName()))
			       encoderThreadStart(action);
		       else
			       encoder_pulsebar_capture.Text = Catalog.GetString("Missing data.");
		}
	}

	
	void on_button_encoder_cancel_clicked (object o, EventArgs args) 
	{
		encoderProcessCancel = true;
	}

	void on_button_encoder_capture_finish_clicked (object o, EventArgs args) 
	{
		encoderProcessFinish = true;
	}

	void on_button_encoder_recalculate_clicked (object o, EventArgs args) {
		encoderCalculeCurves(encoderActions.CURVES);
	}

	private void encoderUpdateTreeViewCapture(string contents)
	{
		if (contents == null || contents == "") {
			encoderButtonsSensitive(encoderSensEnum.DONENOSIGNAL);
		} else {
			treeviewEncoderCaptureRemoveColumns();
			int curvesNum = createTreeViewEncoderCapture(contents);
			if(curvesNum == 0) 
				encoderButtonsSensitive(encoderSensEnum.DONENOSIGNAL);
			else {
				if(Util.FindOnArray(':',1,0,UtilGtk.ComboGetActive(combo_encoder_eccon),
						encoderEcconTranslation) != Constants.Concentric) 
					curvesNum = curvesNum / 2;
			
				string [] activeCurvesList = new String[curvesNum];
				for(int i=0; i < curvesNum; i++)
					activeCurvesList[i] = (i+1).ToString();
				UtilGtk.ComboUpdate(combo_encoder_analyze_curve_num_combo, activeCurvesList, "");
				combo_encoder_analyze_curve_num_combo.Active = 
					UtilGtk.ComboMakeActive(combo_encoder_analyze_curve_num_combo, activeCurvesList[0]);
				
				encoderButtonsSensitive(encoderSensEnum.DONEYESSIGNAL);
			}
		}
	}
	
	private void treeviewEncoderCaptureRemoveColumns() {
		Gtk.TreeViewColumn [] myColumns = treeview_encoder_capture_curves.Columns;
		foreach (Gtk.TreeViewColumn column in myColumns) 
			treeview_encoder_capture_curves.RemoveColumn (column);

		//blank the encoderCaptureListStore
		encoderCaptureListStore = new Gtk.ListStore (typeof (EncoderCurve));
	}

	private void treeviewEncoderAnalyzeRemoveColumns(bool curveOrNeuromuscular) {
		Gtk.TreeViewColumn [] myColumns = treeview_encoder_analyze_curves.Columns;
		foreach (Gtk.TreeViewColumn column in myColumns) 
			treeview_encoder_analyze_curves.RemoveColumn (column);

		//blank the encoderAnalyzeListStore
		if(curveOrNeuromuscular)
			encoderAnalyzeListStore = new Gtk.ListStore (typeof (EncoderCurve));
		else
			encoderAnalyzeListStore = new Gtk.ListStore (typeof (EncoderNeuromuscularData));
	}


	private string getEncoderAnalysisOptions(bool captureOrAnalyze) {
		string analysisOptions = "-";
		if(preferences.encoderPropulsive)
			analysisOptions = "p";

		return analysisOptions;
	}


	//this is called by non gtk thread. Don't do gtk stuff here
	//I suppose reading gtk is ok, changing will be the problem
	//called on calculatecurves, recalculate and load
	private void encoderDoCurvesGraphR() 
	{
		string analysis = "curves";

		string analysisOptions = getEncoderAnalysisOptions(true);

		//see explanation on the top of this file
		lastEncoderSQLSignal = new EncoderSQL(
				"-1",
				currentPerson.UniqueID,
				currentSession.UniqueID,
				getExerciseIDFromCombo(),	
				findEccon(true), 	//force ecS (ecc-conc separated)
				UtilGtk.ComboGetActive(combo_encoder_laterality),
				Util.ConvertToPoint(findMass(Constants.MassType.EXTRA)), //when save on sql, do not include person weight
				"",	//signalOrCurve,
				"", 	//fileSaved,	//to know date do: select substr(name,-23,19) from encoder;
				"",	//path,			//url
				(int) encoderCaptureOptionsWin.spin_encoder_capture_time.Value, 
				(int) encoderCaptureOptionsWin.spin_encoder_capture_min_height.Value, 
				"", 		//desc,
				"","",		//status, videoURL
				encoderConfigurationCurrent,
				"","","",	//future1, 2, 3
				Util.FindOnArray(':', 2, 1, UtilGtk.ComboGetActive(combo_encoder_exercise), 
					encoderExercisesTranslationAndBodyPWeight)	//exerciseName (english)
				);


		EncoderParams ep = new EncoderParams(
				(int) encoderCaptureOptionsWin.spin_encoder_capture_min_height.Value, 
				getExercisePercentBodyWeightFromCombo (),
				Util.ConvertToPoint(findMass(Constants.MassType.BODY)),
				Util.ConvertToPoint(findMass(Constants.MassType.EXTRA)),
				findEccon(true),					//force ecS (ecc-conc separated)
				analysis,
				"none",				//analysisVariables (not needed in create curves). Cannot be blank
				analysisOptions,
				encoderConfigurationCurrent,
				Util.ConvertToPoint(preferences.encoderSmoothCon),	//R decimal: '.'
			       	0, 			//curve is not used here
				image_encoder_width, image_encoder_height,
				preferences.CSVExportDecimalSeparator 
				);

		EncoderStruct es = new EncoderStruct(
				UtilEncoder.GetEncoderDataTempFileName(), 
				UtilEncoder.GetEncoderGraphTempFileName(),
				UtilEncoder.GetEncoderCurvesTempFileName(), 
				UtilEncoder.GetEncoderStatusTempFileName(),
				"none",	//SpecialData
				ep);
		
		bool result = UtilEncoder.RunEncoderGraphNoRDotNet(
				Util.ChangeSpaceAndMinusForUnderscore(currentPerson.Name) + "-" + 
				Util.ChangeSpaceAndMinusForUnderscore(UtilGtk.ComboGetActive(combo_encoder_exercise)) + 
				"-(" + Util.ConvertToPoint(findMass(Constants.MassType.DISPLACED)) + "Kg)",
				es,
				false,	//do not use neuromuscularProfile script
				preferences.RGraphsTranslate
				); 
				
		if(result)
			//store this to show 1,2,3,4,... or 1e,1c,2e,2c,... in RenderN
			//if is not stored, it can change when changed eccon radiobutton on cursor is in treeview
			ecconLast = findEccon(false);
		else {
			encoderProcessProblems = true;
		}
	}
	

	void on_button_encoder_analyze_data_select_curves_clicked (object o, EventArgs args) 
	{
		ArrayList data = SqliteEncoder.Select(
				false, -1, currentPerson.UniqueID, currentSession.UniqueID, 
				"curve", EncoderSQL.Eccons.ALL, 
				false, true);

		ArrayList dataPrint = new ArrayList();
		string [] checkboxes = new string[data.Count]; //to store active or inactive status of curves
		int count = 0;
		foreach(EncoderSQL es in data) {
			checkboxes[count++] = es.status;
			//Log.WriteLine(checkboxes[count-1]);
			dataPrint.Add(es.ToStringArray(count,true,false,true,true));
		}
	
		string [] columnsString = {
			Catalog.GetString("ID"),
			Catalog.GetString("Active"),	//checkboxes
			Catalog.GetString("Repetition"),
			Catalog.GetString("Exercise"),
			"RL",
			Catalog.GetString("Extra weight"),
			Catalog.GetString("Mean Power"),
			Catalog.GetString("Encoder"),
			Catalog.GetString("Contraction"),
			Catalog.GetString("Date"),
			Catalog.GetString("Comment")
		};

		ArrayList bigArray = new ArrayList();
		ArrayList a1 = new ArrayList();
		ArrayList a2 = new ArrayList();
		ArrayList a3 = new ArrayList();
		
		//0 is the widgget to show; 1 is the editable; 2 id default value
		a1.Add(Constants.GenericWindowShow.COMBOALLNONESELECTED); a1.Add(true); a1.Add("ALL");
		bigArray.Add(a1);
		
		a2.Add(Constants.GenericWindowShow.TREEVIEW); a2.Add(true); a2.Add("");
		bigArray.Add(a2);
	
		a3.Add(Constants.GenericWindowShow.COMBO); a3.Add(true); a3.Add("");
		bigArray.Add(a3);
	
		//add exercises to the combo (only the exercises done, and only unique)
		ArrayList encoderExercisesNames = new ArrayList();
		foreach(EncoderSQL es in data) {
			encoderExercisesNames = Util.AddToArrayListIfNotExist(encoderExercisesNames, es.exerciseName);
		}
		
		genericWin = GenericWindow.Show(false,	//don't show now
				string.Format(Catalog.GetString("Saved repetitions of athlete {0} on this session."), 
					currentPerson.Name) + "\n" + 
				Catalog.GetString("Activate the repetitions you want to use clicking on first column.") + "\n" +
				Catalog.GetString("If you want to edit or delete a row, right click on it.") + "\n",
				bigArray);

		genericWin.SetTreeview(columnsString, true, dataPrint, new ArrayList(), Constants.ContextMenu.EDITDELETE, false);
		genericWin.AddOptionsToComboCheckBoxesOptions(encoderExercisesNames);
		genericWin.CreateComboCheckBoxes();
		genericWin.MarkActiveCurves(checkboxes);
		
		//find all persons in current session
		ArrayList personsPre = SqlitePersonSession.SelectCurrentSessionPersons(currentSession.UniqueID);
		string [] persons = new String[personsPre.Count];
		count = 0;
	        foreach	(Person p in personsPre)
			persons[count++] = p.UniqueID.ToString() + ":" + p.Name;
		genericWin.SetComboValues(persons, currentPerson.UniqueID + ":" + currentPerson.Name);
		genericWin.SetComboLabel(Catalog.GetString("Change the owner of selected repetition") + 
				" (" + Catalog.GetString("code") + ":" + Catalog.GetString("name") + ")");
		genericWin.ShowEditRow(false);
		genericWin.CommentColumn = 10;
		
		genericWin.ShowButtonCancel(false);
		genericWin.SetButtonAcceptSensitive(true);
		genericWin.SetButtonCancelLabel(Catalog.GetString("Close"));
		//manage selected, unselected curves
		genericWin.Button_accept.Clicked += new EventHandler(on_encoder_show_curves_done);
		genericWin.Button_row_edit.Clicked += new EventHandler(on_encoder_show_curves_row_edit);
		genericWin.Button_row_edit_apply.Clicked += new EventHandler(on_encoder_show_curves_row_edit_apply);
		genericWin.Button_row_delete.Clicked += new EventHandler(on_encoder_show_curves_row_delete_pre);

		//used when we don't need to read data, 
		//and we want to ensure next window will be created at needed size
		//genericWin.DestroyOnAccept=true;
		//here is comented because we are going to read the checkboxes

		genericWin.ShowNow();
	}
	
	protected void on_encoder_show_curves_done (object o, EventArgs args)
	{
		genericWin.Button_accept.Clicked -= new EventHandler(on_encoder_show_curves_done);

		//get selected/deselected rows
		string [] checkboxes = genericWin.GetCheckboxesStatus(1, false);

		ArrayList data = SqliteEncoder.Select(
				false, -1, currentPerson.UniqueID, currentSession.UniqueID, 
				"curve", EncoderSQL.Eccons.ALL, 
				false, true);

		//update on database the curves that have been selected/deselected
		int count = 0;
		int countActive = 0;

		Sqlite.Open();
		foreach(EncoderSQL eSQL in data) {
			if(eSQL.status != checkboxes[count]) {
				eSQL.status = checkboxes[count];
				SqliteEncoder.Update(true, eSQL);
			}
			
			count ++;

			if(eSQL.status == "active") 
				countActive ++;
		}
		Sqlite.Close();

		int activeCurvesNum = getActiveCurvesNum(data);
		label_encoder_user_curves_active_num.Text = activeCurvesNum.ToString();

		string [] activeCurvesList = getActiveCheckboxesList(checkboxes, activeCurvesNum);
		UtilGtk.ComboUpdate(combo_encoder_analyze_curve_num_combo, activeCurvesList, "");
		combo_encoder_analyze_curve_num_combo.Active = 
			UtilGtk.ComboMakeActive(combo_encoder_analyze_curve_num_combo, activeCurvesList[0]);

		genericWin.HideAndNull();
		
		//encoderButtonsSensitive(encoderSensEnumStored);
		button_encoder_analyze.Sensitive = (countActive > 0);
	}
	
	protected void on_encoder_show_curves_row_edit (object o, EventArgs args) {
		Log.WriteLine("row edit at show curves");
		Log.WriteLine(genericWin.TreeviewSelectedUniqueID.ToString());
		genericWin.ShowEditRow(true);
	}

	protected void on_encoder_show_curves_row_edit_apply (object o, EventArgs args) {
		Log.WriteLine("row edit apply at show curves");

		int curveID = genericWin.TreeviewSelectedUniqueID;
		EncoderSQL eSQL = (EncoderSQL) SqliteEncoder.Select(
				false, curveID, 0, 0, 
				"", EncoderSQL.Eccons.ALL, 
				false, true)[0];

		//if changed comment, update SQL, and update treeview
		//first remove conflictive characters
		string comment = Util.RemoveTildeAndColonAndDot(genericWin.EntryEditRow);
		if(comment != eSQL.description) {
			eSQL.description = comment;
			SqliteEncoder.Update(false, eSQL);

			//update treeview
			genericWin.on_edit_selected_done_update_treeview();
		}

		//if changed person, proceed
		Log.WriteLine("new person: " + genericWin.GetComboSelected);
		int newPersonID = Util.FetchID(genericWin.GetComboSelected);
		if(newPersonID != currentPerson.UniqueID) {
			EncoderSQL eSQLChangedPerson = eSQL.ChangePerson(genericWin.GetComboSelected);
			SqliteEncoder.Update(false, eSQLChangedPerson);

			genericWin.RemoveSelectedRow();
		}

		genericWin.ShowEditRow(false);
		updateUserCurvesLabelsAndCombo();
	}
	
	protected void on_encoder_show_curves_row_delete_pre (object o, EventArgs args) {
		if(preferences.askDeletion) {
			ConfirmWindow confirmWin = ConfirmWindow.Show(Catalog.GetString(
						"Are you sure you want to delete this repetition?"), "", "");
			confirmWin.Button_accept.Clicked += new EventHandler(on_encoder_show_curves_row_delete);
		} else
			on_encoder_show_curves_row_delete (o, args);
	}
	
	protected void on_encoder_show_curves_row_delete (object o, EventArgs args) {
		Log.WriteLine("row delete at show curves");

		int uniqueID = genericWin.TreeviewSelectedUniqueID;

		delete_encoder_curve(uniqueID);

		genericWin.Delete_row_accepted();
	}

	void delete_encoder_curve(int uniqueID) {
		Log.WriteLine(uniqueID.ToString());

		EncoderSQL eSQL = (EncoderSQL) SqliteEncoder.Select(false, uniqueID, 0, 0, "", EncoderSQL.Eccons.ALL, false, true)[0];
		//remove the file
		bool deletedOk = Util.FileDelete(eSQL.GetFullURL(false));	//don't convertPathToR
		if(deletedOk)  {
			Sqlite.Delete(false, Constants.EncoderTable, Convert.ToInt32(uniqueID));
			
			ArrayList escArray = SqliteEncoder.SelectSignalCurve(false, 
						-1, Convert.ToInt32(uniqueID),	//signal, curve
						-1, -1); 			//msStart, msEnd
			SqliteEncoder.DeleteSignalCurveWithCurveID(false, 
					Convert.ToInt32(eSQL.uniqueID)); //delete by curveID on SignalCurve table
			//if deleted curve is from current signal, uncheck it in encoderCaptureCurves
			if(escArray.Count > 0) {
				EncoderSignalCurve esc = (EncoderSignalCurve) escArray[0];
				if(esc.signalID == Convert.ToInt32(encoderSignalUniqueID))
					encoderCaptureSelectBySavedCurves(esc.msCentral, false);
			}

			updateUserCurvesLabelsAndCombo();
		}
	}
	
	
	void on_button_encoder_analyze_data_compare_clicked (object o, EventArgs args) 
	{
		if(Util.FindOnArray(':',1,0,UtilGtk.ComboGetActive(combo_encoder_analyze_data_compare),
					encoderDataCompareTranslation) == "Between persons")
			encoder_analyze_data_compare_interperson();
		else if(Util.FindOnArray(':',1,0,UtilGtk.ComboGetActive(combo_encoder_analyze_data_compare),
					encoderDataCompareTranslation) == "Between sessions")
			encoder_analyze_data_compare_intersession();
	}

	void encoder_analyze_data_compare_interperson () 
	{
		//find all persons except current person
		ArrayList dataPre = SqlitePersonSession.SelectCurrentSessionPersons(currentSession.UniqueID); 
		ArrayList data = new ArrayList();
		ArrayList nonSensitiveRows = new ArrayList();
		int i = 0;	//list of persons
		int j = 0;	//list of added persons
		foreach(Person p in dataPre) {
			if(p.UniqueID != currentPerson.UniqueID) {
				ArrayList eSQLarray = SqliteEncoder.Select(
						false, -1, p.UniqueID, currentSession.UniqueID, 
						"curve", EncoderSQL.Eccons.ALL, 
						false, true);
				string [] s = { p.UniqueID.ToString(), "", p.Name,
					getActiveCurvesNum(eSQLarray).ToString(), eSQLarray.Count.ToString()
			       	};
				data.Add(s);
				if(getActiveCurvesNum(eSQLarray) == 0)
					nonSensitiveRows.Add(j);
				j++;
			}
			i ++;
		}
	
		//prepare checkboxes to be marked	
		string [] checkboxes = new string[data.Count]; //to store active or inactive status
		int count = 0;
		foreach(string [] sPersons in data) {
			bool found = false;
			foreach(string s2 in encoderCompareInterperson)
				if(Util.FetchID(s2).ToString() == sPersons[0])
					found = true;

			if(found)
				checkboxes[count++] = "active";
			else
				checkboxes[count++] = "inactive";
		}			
			
		string [] columnsString = {
			Catalog.GetString("ID"),
			"",				//checkboxes
			Catalog.GetString("Person name"),
			Catalog.GetString("Selected\nrepetitions"),
			Catalog.GetString("All\nrepetitions")
		};

		ArrayList bigArray = new ArrayList();
		ArrayList a1 = new ArrayList();
		ArrayList a2 = new ArrayList();
		
		//0 is the widgget to show; 1 is the editable; 2 id default value
		a1.Add(Constants.GenericWindowShow.COMBOALLNONESELECTED); a1.Add(true); a1.Add("ALL");
		bigArray.Add(a1);
		
		a2.Add(Constants.GenericWindowShow.TREEVIEW); a2.Add(true); a2.Add("");
		bigArray.Add(a2);
		
		genericWin = GenericWindow.Show(false,	//don't show now
				string.Format(Catalog.GetString("Select persons to compare to {0}."), 
					currentPerson.Name), bigArray);

		genericWin.SetTreeview(columnsString, true, data, nonSensitiveRows, Constants.ContextMenu.NONE, false);
		genericWin.CreateComboCheckBoxes();
		genericWin.MarkActiveCurves(checkboxes);
		genericWin.ShowButtonCancel(false);
		genericWin.SetButtonAcceptSensitive(true);
		//manage selected, unselected curves
		genericWin.Button_accept.Clicked += new EventHandler(
				on_encoder_analyze_data_compare_interperson_done);

		//used when we don't need to read data, 
		//and we want to ensure next window will be created at needed size
		//genericWin.DestroyOnAccept=true;
		//here is comented because we are going to read the checkboxes

		genericWin.ShowNow();
	}

	void on_encoder_analyze_data_compare_interperson_done (object o, EventArgs args) {
		genericWin.Button_accept.Clicked -= new EventHandler(
				on_encoder_analyze_data_compare_interperson_done);
	
		encoderCompareInterperson = new ArrayList ();
		string [] selectedID = genericWin.GetCheckboxesStatus(0,true);
		string [] selectedName = genericWin.GetCheckboxesStatus(2,true);

		for (int i=0 ; i < selectedID.Length ; i ++)
			encoderCompareInterperson.Add(Convert.ToInt32(selectedID[i]) + ":" + selectedName[i]);

		genericWin.HideAndNull();
		
		Log.WriteLine("done");
	}
	
	void encoder_analyze_data_compare_intersession () 
	{
		//select all curves of this person on all sessions
		ArrayList dataPre = SqliteEncoder.SelectCompareIntersession(
				false, currentPerson.UniqueID); 
		
		//..except on current session
		ArrayList data = new ArrayList();
		foreach(EncoderPersonCurvesInDB encPS in dataPre)
			if(encPS.sessionID != currentSession.UniqueID)
				data.Add(encPS);
	
		//prepare unsensitive rows	
		ArrayList nonSensitiveRows = new ArrayList();
		int count = 0;
		foreach(EncoderPersonCurvesInDB encPS in data) {
			if(encPS.countActive == 0)
				nonSensitiveRows.Add(count);
			count ++;
		}
		
		//prepare checkboxes to be marked	
		string [] checkboxes = new string[data.Count]; //to store active or inactive status
		count = 0;
		foreach(EncoderPersonCurvesInDB encPS in data) {
			bool found = false;
			foreach(string s2 in encoderCompareIntersession)
				if(Util.FetchID(s2) == encPS.sessionID)
					found = true;

			if(found)
				checkboxes[count++] = "active";
			else
				checkboxes[count++] = "inactive";
		}			
			
		string [] columnsString = {
			Catalog.GetString("ID"),
			"",				//checkboxes
			Catalog.GetString("Session name"),
			Catalog.GetString("Session date"),
			Catalog.GetString("Selected\nrepetitions"),
			Catalog.GetString("All\nrepetitions")
		};

		ArrayList bigArray = new ArrayList();
		ArrayList a1 = new ArrayList();
		ArrayList a2 = new ArrayList();
		
		//0 is the widgget to show; 1 is the editable; 2 id default value
		a1.Add(Constants.GenericWindowShow.COMBOALLNONESELECTED); a1.Add(true); a1.Add("ALL");
		bigArray.Add(a1);
		
		a2.Add(Constants.GenericWindowShow.TREEVIEW); a2.Add(true); a2.Add("");
		bigArray.Add(a2);
		
		genericWin = GenericWindow.Show(false,	//don't show now
				string.Format(Catalog.GetString("Compare repetitions of {0} from this session with the following sessions."), 
					currentPerson.Name), bigArray);

		//convert data from array of EncoderPersonCurvesInDB to array of strings []
		ArrayList dataConverted = new ArrayList();
		foreach(EncoderPersonCurvesInDB encPS in data) {
			dataConverted.Add(encPS.ToStringArray());
		}

		genericWin.SetTreeview(columnsString, true, dataConverted, nonSensitiveRows, Constants.ContextMenu.NONE, false);
		genericWin.CreateComboCheckBoxes();
		genericWin.MarkActiveCurves(checkboxes);
		genericWin.ShowButtonCancel(false);
		genericWin.SetButtonAcceptSensitive(true);
		//manage selected, unselected curves
		genericWin.Button_accept.Clicked += new EventHandler(
				on_encoder_analyze_data_compare_intersession_done);

		//used when we don't need to read data, 
		//and we want to ensure next window will be created at needed size
		//genericWin.DestroyOnAccept=true;
		//here is comented because we are going to read the checkboxes

		genericWin.ShowNow();
	}

	void on_encoder_analyze_data_compare_intersession_done (object o, EventArgs args) {
		genericWin.Button_accept.Clicked -= new EventHandler(
				on_encoder_analyze_data_compare_intersession_done);
	
		encoderCompareIntersession = new ArrayList ();
		string [] selectedID = genericWin.GetCheckboxesStatus(0,true);
		string [] selectedDate = genericWin.GetCheckboxesStatus(3,true);

		for (int i=0 ; i < selectedID.Length ; i ++)
			encoderCompareIntersession.Add(Convert.ToInt32(selectedID[i]) + ":" + selectedDate[i]);

		genericWin.HideAndNull();
		
		Log.WriteLine("done");
	}
	

	void on_button_encoder_load_signal_clicked (object o, EventArgs args) 
	{
		ArrayList data = SqliteEncoder.Select(
				false, -1, currentPerson.UniqueID, currentSession.UniqueID, 
				"signal", EncoderSQL.Eccons.ALL, 
				false, true);

		ArrayList dataPrint = new ArrayList();
		int count = 1;
		foreach(EncoderSQL es in data) 
			dataPrint.Add(es.ToStringArray(count++,false,true,true,false));
		
		string [] columnsString = {
			Catalog.GetString("ID"),
			Catalog.GetString("Set"),
			Catalog.GetString("Exercise"),
			"RL",
			Catalog.GetString("Extra weight"),
			Catalog.GetString("Encoder"),
			Catalog.GetString("Contraction"),
			Catalog.GetString("Date"),
			Catalog.GetString("Video"),
			Catalog.GetString("Comment")
		};

		ArrayList bigArray = new ArrayList();
		ArrayList a1 = new ArrayList();
		ArrayList a2 = new ArrayList();
		//0 is the widgget to show; 1 is the editable; 2 id default value
		a1.Add(Constants.GenericWindowShow.TREEVIEW); a1.Add(true); a1.Add("");
		bigArray.Add(a1);
	
		a2.Add(Constants.GenericWindowShow.COMBO); a2.Add(true); a2.Add("");
		bigArray.Add(a2);
		
		genericWin = GenericWindow.Show(false,	//don't show now
				string.Format(Catalog.GetString("Select set of athlete {0} on this session."), 
					currentPerson.Name) + "\n" + 
				Catalog.GetString("If you want to edit or delete a row, right click on it."), bigArray);

		genericWin.SetTreeview(columnsString, false, dataPrint, new ArrayList(), Constants.ContextMenu.EDITDELETE, true);
	
		//find all persons in current session
		ArrayList personsPre = SqlitePersonSession.SelectCurrentSessionPersons(currentSession.UniqueID);
		string [] persons = new String[personsPre.Count];
		count = 0;
	        foreach	(Person p in personsPre)
			persons[count++] = p.UniqueID.ToString() + ":" + p.Name;
		genericWin.SetComboValues(persons, currentPerson.UniqueID + ":" + currentPerson.Name);
		genericWin.SetComboLabel(Catalog.GetString("Change the owner of selected set") + 
				" (" + Catalog.GetString("code") + ":" + Catalog.GetString("name") + ")");
		genericWin.ShowEditRow(false);
		genericWin.CommentColumn = 9;
	
		genericWin.ShowButtonCancel(true);
		genericWin.SetButtonAcceptLabel(Catalog.GetString("Load"));
		genericWin.SetButtonCancelLabel(Catalog.GetString("Close"));
		genericWin.SetButtonAcceptSensitive(false);
		genericWin.Button_accept.Clicked += new EventHandler(on_encoder_load_signal_accepted);
		genericWin.Button_row_edit.Clicked += new EventHandler(on_encoder_load_signal_row_edit);
		genericWin.Button_row_edit_apply.Clicked += new EventHandler(on_encoder_load_signal_row_edit_apply);
		genericWin.Button_row_delete.Clicked += new EventHandler(on_encoder_load_signal_row_delete_pre);

		genericWin.ShowNow();
	}
	
	protected void on_encoder_load_signal_accepted (object o, EventArgs args)
	{
		genericWin.Button_accept.Clicked -= new EventHandler(on_encoder_load_signal_accepted);

		int uniqueID = genericWin.TreeviewSelectedRowID();
		
		genericWin.HideAndNull();

		ArrayList data = SqliteEncoder.Select(
				false, uniqueID, currentPerson.UniqueID, currentSession.UniqueID, 
				"signal", EncoderSQL.Eccons.ALL, 
				false, true);

		bool success = false;
		foreach(EncoderSQL eSQL in data) {	//it will run only one time
			success = UtilEncoder.CopyEncoderDataToTemp(eSQL.url, eSQL.filename);
			if(success) {
				string exerciseNameTranslated =Util.FindOnArray(':', 1, 2, eSQL.exerciseName, 
						encoderExercisesTranslationAndBodyPWeight);
				combo_encoder_exercise.Active = UtilGtk.ComboMakeActive(combo_encoder_exercise, exerciseNameTranslated);
				
				combo_encoder_eccon.Active = UtilGtk.ComboMakeActive(combo_encoder_eccon, eSQL.ecconLong);
				combo_encoder_laterality.Active = UtilGtk.ComboMakeActive(combo_encoder_laterality, eSQL.laterality);
				spin_encoder_extra_weight.Value = Convert.ToInt32(eSQL.extraWeight);

				encoderCaptureOptionsWin.spin_encoder_capture_min_height.Value = eSQL.minHeight;
				//entry_encoder_signal_comment.Text = eSQL.description;
				encoderTimeStamp = eSQL.GetDate(false); 
				encoderSignalUniqueID = eSQL.uniqueID;
				button_video_play_this_test_encoder.Sensitive = (eSQL.videoURL != "");

				encoderConfigurationCurrent = eSQL.encoderConfiguration;

				label_encoder_selected.Text = encoderConfigurationCurrent.code; 
			}
		}

		if(success) {	
			//force a recalculate but not save the curve (we are loading)
			encoderCalculeCurves(encoderActions.LOAD);
		
			check_encoder_analyze_signal_or_curves.Active = true;

			encoderButtonsSensitive(encoderSensEnumStored);
		}
	}
	
	protected void on_encoder_load_signal_row_edit (object o, EventArgs args) {
		Log.WriteLine("row edit at load signal");
		Log.WriteLine(genericWin.TreeviewSelectedUniqueID.ToString());
		genericWin.ShowEditRow(true);
	}
	
	protected void on_encoder_load_signal_row_edit_apply (object o, EventArgs args) {
		Log.WriteLine("row edit apply at load signal");
			
		int curveID = genericWin.TreeviewSelectedUniqueID;
		EncoderSQL eSQL = (EncoderSQL) SqliteEncoder.Select(false, curveID, 0, 0, "", EncoderSQL.Eccons.ALL, false, true)[0];
		
		//if changed comment, update SQL, and update treeview
		//first remove conflictive characters
		string comment = Util.RemoveTildeAndColonAndDot(genericWin.EntryEditRow);
		if(comment != eSQL.description) {
			eSQL.description = comment;
			SqliteEncoder.Update(false, eSQL);

			//update treeview
			genericWin.on_edit_selected_done_update_treeview();
		}

		//if changed person, proceed
		Log.WriteLine("new person: " + genericWin.GetComboSelected);
		int newPersonID = Util.FetchID(genericWin.GetComboSelected);
		if(newPersonID != currentPerson.UniqueID) {
			EncoderSQL eSQLChangedPerson = eSQL.ChangePerson(genericWin.GetComboSelected);
			SqliteEncoder.Update(false, eSQLChangedPerson);
			
			genericWin.RemoveSelectedRow();
		}

		genericWin.ShowEditRow(false);
	}
	
	protected void on_encoder_load_signal_row_delete_pre (object o, EventArgs args) {
		if(preferences.askDeletion) {
			ConfirmWindow confirmWin = ConfirmWindow.Show(Catalog.GetString(
						"Are you sure you want to delete this set?"), Catalog.GetString("Saved repetitions related to this set will also be deleted."), "");
			confirmWin.Button_accept.Clicked += new EventHandler(on_encoder_load_signal_row_delete);
		} else
			on_encoder_load_signal_row_delete (o, args);
	}
	
	protected void on_encoder_load_signal_row_delete (object o, EventArgs args) {
		Log.WriteLine("row delete at load signal");

		int signalID = genericWin.TreeviewSelectedUniqueID;
		Log.WriteLine(signalID.ToString());

		//if it's current signal use the delete signal from the gui interface that updates gui
		if(signalID == Convert.ToInt32(encoderSignalUniqueID))
			on_button_encoder_delete_signal_accepted (o, args);
		else {
			EncoderSQL eSQL = (EncoderSQL) SqliteEncoder.Select(
					false, signalID, 0, 0, "signal", EncoderSQL.Eccons.ALL, false, true)[0];
		
			//delete signal and related curves (both from SQL and files)
			encoderSignalDelete(eSQL.GetFullURL(false), signalID);	//don't convertPathToR

			//genericWin selected row is deleted, unsensitive the "load" button
			genericWin.SetButtonAcceptSensitive(false);
		}
		genericWin.Delete_row_accepted();
	}

	void encoderSignalDelete (string signalURL, int signalID) 
	{
		//remove signal file
		bool deletedOk = Util.FileDelete(signalURL);

		//delete signal from encoder table
		Sqlite.Delete(false, Constants.EncoderTable, signalID);

		//find related curves using encoderSignalCurve table
		ArrayList linkedCurves = SqliteEncoder.SelectSignalCurve(
				false, signalID, -1, -1, -1);	//DBopened, signal, curve, msStart, msEnd
		
		//delete related curves: files and records from encoder table
		foreach(EncoderSignalCurve esc in linkedCurves) 
		{
			//select related curves to find URL
			EncoderSQL eSQL = (EncoderSQL) SqliteEncoder.Select(
					false, esc.curveID, -1, -1, "curve", EncoderSQL.Eccons.ALL, false, true)[0];
			
			//delete file
			Util.FileDelete(eSQL.GetFullURL(false));	//don't convertPathToR

			//delete curve from encoder table
			Sqlite.Delete(false, Constants.EncoderTable, esc.curveID);
		}
		
		//delete related records from encoderSignalCurve table
		Sqlite.DeleteSelectingField(false, Constants.EncoderSignalCurveTable, 
				"signalID", signalID.ToString());
	}
	
	void on_button_encoder_export_all_curves_clicked (object o, EventArgs args) 
	{
		checkFile(Constants.EncoderCheckFileOp.ANALYZE_EXPORT_ALL_CURVES);
	}
	
	void on_button_encoder_export_all_curves_file_selected (string selectedFileName) 
	{
		string analysisOptions = getEncoderAnalysisOptions(true);

		string displacedMass = Util.ConvertToPoint( lastEncoderSQLSignal.extraWeight + (
					getExercisePercentBodyWeightFromName(lastEncoderSQLSignal.exerciseName) *
					currentPersonSession.Weight
					) );	
		
		EncoderParams ep = new EncoderParams(
				lastEncoderSQLSignal.minHeight, 
				getExercisePercentBodyWeightFromName (lastEncoderSQLSignal.exerciseName),
				Util.ConvertToPoint(findMass(Constants.MassType.BODY)),
				Util.ConvertToPoint(findMass(Constants.MassType.EXTRA)),
				findEccon(false), //do not force ecS (ecc-conc separated) //not taken from lastEncoderSQLSignal because there is (true)
				"exportCSV",
				"none",						//analysisVariables (not needed in create curves). Cannot be blank
				analysisOptions,
				encoderConfigurationCurrent,
				Util.ConvertToPoint(preferences.encoderSmoothCon),	//R decimal: '.'
				-1,
				image_encoder_width,
				image_encoder_height,
				preferences.CSVExportDecimalSeparator 
				);

		string dataFileName = UtilEncoder.GetEncoderDataTempFileName();

		EncoderStruct encoderStruct = new EncoderStruct(
				dataFileName, 
				UtilEncoder.GetEncoderGraphTempFileName(),
				selectedFileName, 
				UtilEncoder.GetEncoderStatusTempFileName(),
				"none", 		//SpecialData
				ep);

		UtilEncoder.RunEncoderGraphNoRDotNet(
				Util.ChangeSpaceAndMinusForUnderscore(currentPerson.Name) + "-" + 
				Util.ChangeSpaceAndMinusForUnderscore(lastEncoderSQLSignal.exerciseName) + 
					"-(" + displacedMass + "Kg)",
				encoderStruct,
				false, 			//do not use neuromuscularProfile script
				preferences.RGraphsTranslate
				);

		//encoder_pulsebar_capture.Text = string.Format(Catalog.GetString(
		//			"Exported to {0}."), UtilEncoder.GetEncoderExportTempFileName());
	}

	string exportFileName;	
	protected void checkFile (Constants.EncoderCheckFileOp checkFileOp)
	{
		string exportString = ""; 
		if(checkFileOp == Constants.EncoderCheckFileOp.ANALYZE_EXPORT_ALL_CURVES)
			exportString = Catalog.GetString ("Export session in format CSV");
		else if(checkFileOp == Constants.EncoderCheckFileOp.ANALYZE_SAVE_IMAGE)
			exportString = Catalog.GetString ("Save image");
		else if(checkFileOp == Constants.EncoderCheckFileOp.ANALYZE_SAVE_TABLE)
			exportString = Catalog.GetString ("Save table");
		
		string nameString = ""; 
		if(checkFileOp == Constants.EncoderCheckFileOp.ANALYZE_EXPORT_ALL_CURVES)
			nameString = "encoder_export.csv";
		else if(checkFileOp == Constants.EncoderCheckFileOp.ANALYZE_SAVE_IMAGE)
			nameString = "encoder_image.png";
		else if(checkFileOp == Constants.EncoderCheckFileOp.ANALYZE_SAVE_TABLE)
			nameString = "encoder_curves_table.csv";
		
		Gtk.FileChooserDialog fc=
			new Gtk.FileChooserDialog(exportString,
					app1,
					FileChooserAction.Save,
					Catalog.GetString("Cancel"),ResponseType.Cancel,
					Catalog.GetString("Export"),ResponseType.Accept
					);
		fc.CurrentName = nameString;

		if (fc.Run() == (int)ResponseType.Accept) 
		{
			exportFileName = fc.Filename;
			//add ".csv" if needed
			if(checkFileOp == Constants.EncoderCheckFileOp.ANALYZE_EXPORT_ALL_CURVES ||
					checkFileOp == Constants.EncoderCheckFileOp.ANALYZE_SAVE_TABLE)
				exportFileName = Util.AddCsvIfNeeded(exportFileName);
			else
				exportFileName = Util.AddPngIfNeeded(exportFileName);
			try {
				if (File.Exists(exportFileName)) {
					Log.WriteLine(string.Format(
								"File {0} exists with attributes {1}, created at {2}", 
								exportFileName, 
								File.GetAttributes(exportFileName), 
								File.GetCreationTime(exportFileName)));
					Log.WriteLine("Overwrite...");
					ConfirmWindow confirmWin = ConfirmWindow.Show(Catalog.GetString(
								"Are you sure you want to overwrite file: "), "", 
							exportFileName);

					if(checkFileOp == Constants.EncoderCheckFileOp.ANALYZE_EXPORT_ALL_CURVES)
						confirmWin.Button_accept.Clicked += 
							new EventHandler(on_overwrite_file_export_all_curves_accepted);
					else if(checkFileOp == Constants.EncoderCheckFileOp.ANALYZE_SAVE_IMAGE)
						confirmWin.Button_accept.Clicked += 
							new EventHandler(on_overwrite_file_encoder_save_image_accepted);
					else if(checkFileOp == Constants.EncoderCheckFileOp.ANALYZE_SAVE_TABLE)
						confirmWin.Button_accept.Clicked += 
							new EventHandler(on_overwrite_file_encoder_save_table_accepted);

				} else {
					if(checkFileOp == Constants.EncoderCheckFileOp.ANALYZE_EXPORT_ALL_CURVES)
						on_button_encoder_export_all_curves_file_selected (exportFileName);
					else if(checkFileOp == Constants.EncoderCheckFileOp.ANALYZE_SAVE_IMAGE)
						on_button_encoder_save_image_file_selected (exportFileName);
					else if(checkFileOp == Constants.EncoderCheckFileOp.ANALYZE_SAVE_TABLE)
						on_button_encoder_save_table_file_selected (exportFileName);

					string myString = string.Format(Catalog.GetString("Saved to {0}"), 
							exportFileName);
					if(checkFileOp == Constants.EncoderCheckFileOp.ANALYZE_EXPORT_ALL_CURVES)
				       		myString += Constants.SpreadsheetString;
					new DialogMessage(Constants.MessageTypes.INFO, myString);
				}
			} catch {
				string myString = string.Format(
						Catalog.GetString("Cannot save file {0} "), exportFileName);
				new DialogMessage(Constants.MessageTypes.WARNING, myString);
			}
		}
		else {
			Log.WriteLine("cancelled");
			//report does not currently send the appBar reference
			new DialogMessage(Constants.MessageTypes.INFO, Catalog.GetString("Cancelled."));
			fc.Hide ();
			return ;
		}
		
		//Don't forget to call Destroy() or the FileChooserDialog window won't get closed.
		fc.Destroy();
		
		return;
	}
	private void on_overwrite_file_export_all_curves_accepted(object o, EventArgs args)
	{
		on_button_encoder_export_all_curves_file_selected (exportFileName);

		string myString = string.Format(Catalog.GetString("Saved to {0}"), 
				exportFileName) + Constants.SpreadsheetString;
		new DialogMessage(Constants.MessageTypes.INFO, myString);
	}
	private void on_overwrite_file_encoder_save_image_accepted(object o, EventArgs args)
	{
		on_button_encoder_save_image_file_selected (exportFileName);

		string myString = string.Format(Catalog.GetString("Saved to {0}"), exportFileName);
		new DialogMessage(Constants.MessageTypes.INFO, myString);
	}
	private void on_overwrite_file_encoder_save_table_accepted(object o, EventArgs args)
	{
		on_button_encoder_save_table_file_selected (exportFileName);

		string myString = string.Format(Catalog.GetString("Saved to {0}"), exportFileName);
		new DialogMessage(Constants.MessageTypes.INFO, myString);
	}
	
	
	void on_button_encoder_delete_signal_clicked (object o, EventArgs args) 
	{
		if(preferences.askDeletion) {
			ConfirmWindow confirmWin = ConfirmWindow.Show(Catalog.GetString(
						"Are you sure you want to delete this set?"), Catalog.GetString("Saved repetitions related to this set will also be deleted."), "");
			confirmWin.Button_accept.Clicked += new EventHandler(on_button_encoder_delete_signal_accepted);
		} else
			on_button_encoder_delete_signal_accepted (o, args);
	}	

	void on_button_encoder_delete_signal_accepted (object o, EventArgs args) 
	{
		EncoderSQL eSQL = (EncoderSQL) SqliteEncoder.Select(
				false, Convert.ToInt32(encoderSignalUniqueID), 0, 0, "signal", EncoderSQL.Eccons.ALL, false, true)[0];

		//delete signal and related curves (both from SQL and files)
		encoderSignalDelete(eSQL.GetFullURL(false), Convert.ToInt32(encoderSignalUniqueID));

		encoderSignalUniqueID = "-1";
		image_encoder_capture.Sensitive = false;
		treeviewEncoderCaptureRemoveColumns();
		UtilGtk.ErasePaint(encoder_capture_curves_bars_drawingarea, encoder_capture_curves_bars_pixmap);
		encoderButtonsSensitive(encoderSensEnum.DONENOSIGNAL);
		encoder_pulsebar_capture.Text = Catalog.GetString("Set deleted");
		//entry_encoder_signal_comment.Text = "";
	}

	private int getActiveCurvesNum(ArrayList curvesArray) {
		int countActiveCurves = 0;
		foreach(EncoderSQL es in curvesArray) 
			if(es.status == "active")
				countActiveCurves ++;
		
		return countActiveCurves;
	}

	private void updateUserCurvesLabelsAndCombo() {
		ArrayList data = SqliteEncoder.Select(
				false, -1, currentPerson.UniqueID, currentSession.UniqueID, 
				"curve", EncoderSQL.Eccons.ALL, 
				false, true);
		int activeCurvesNum = getActiveCurvesNum(data);
		label_encoder_user_curves_active_num.Text = activeCurvesNum.ToString();
		label_encoder_user_curves_all_num.Text = data.Count.ToString();
		updateComboEncoderAnalyzeCurveNum(data, activeCurvesNum);	
	
		button_encoder_analyze_sensitiveness();
	}
	
	private string [] getActiveCheckboxesList(string [] checkboxes, int activeCurvesNum) {
		if(activeCurvesNum == 0)
			return Util.StringToStringArray("");

		string [] activeCurvesList = new String[activeCurvesNum];
		int i=0;
		int j=0;
		foreach(string cb in checkboxes) {
			if(cb == "active")
				activeCurvesList[j++] = (i+1).ToString();
			i++;
		}
		return activeCurvesList;
	}
	
	private void updateComboEncoderAnalyzeCurveNum (ArrayList data, int activeCurvesNum) {
		string [] checkboxes = new string[data.Count]; //to store active or inactive status of curves
		int count = 0;
		foreach(EncoderSQL eSQL in data) {
			checkboxes[count++] = eSQL.status;
		}
		string [] activeCurvesList = getActiveCheckboxesList(checkboxes, activeCurvesNum);
		UtilGtk.ComboUpdate(combo_encoder_analyze_curve_num_combo, activeCurvesList, "");
		combo_encoder_analyze_curve_num_combo.Active = 
			UtilGtk.ComboMakeActive(combo_encoder_analyze_curve_num_combo, activeCurvesList[0]);
	}


	string encoderSaveSignalOrCurve (string mode, int selectedID) 
	{
		//mode is different than type. 
		//mode can be curve or signal
		//type is to print on db at type column: curve or signal + (bar or jump)
		string signalOrCurve = "";
		string feedback = "";
		string fileSaved = "";
		string path = "";
		
		if(mode == "curve") {
			signalOrCurve = "curve";
			feedback = Catalog.GetString("Saved");
		} else 	{	//mode == "signal"
			signalOrCurve = "signal";
		
			//check if data is ok (maybe encoder was not connected, then don't save this signal)
			EncoderCurve curveExist = treeviewEncoderCaptureCurvesGetCurve(1, false);
			if(curveExist.N == null) 
				return "";
		}
		
		string meanPowerStr = "";
		string desc = "";
		if(mode == "curve") {
			EncoderCurve curve = treeviewEncoderCaptureCurvesGetCurve(selectedID,true);

			//some start at ,5 because of the spline filtering
			int curveStart = Convert.ToInt32(decimal.Truncate(Convert.ToDecimal(curve.Start)));

			int duration = Convert.ToInt32(decimal.Truncate(Convert.ToDecimal(curve.Duration)));

			meanPowerStr = curve.MeanPower;

			if(ecconLast != "c") {
				EncoderCurve curveNext = treeviewEncoderCaptureCurvesGetCurve(selectedID+1,false);
				
				//since isometric phase has been implemented (Chronojump 1.3.6)
				//curveE.start + curveE.duration < curveC.start (because there's isometric between)
				int curveEccEnd = curveStart + duration;
				int curveConStart = Convert.ToInt32(
						decimal.Truncate(Convert.ToDecimal(curveNext.Start)));
				int curveConDuration = Convert.ToInt32(
						decimal.Truncate(Convert.ToDecimal(curveNext.Duration)));
				int isometricDuration = curveConStart - curveEccEnd;

				//duration is duration of ecc + duration of iso + duration of concentric
				duration += (isometricDuration + curveConDuration);
			
				meanPowerStr = curveNext.MeanPower; //take power of concentric phase
			}
			
			/*
			 * at inertial signals, first curve is eccentric (can be to left or right, maybe positive or negative)
			 * graph.R manages correctly this
			 * But, when saved a curve, eg. concentric this can be positive or negative
			 * (depending on the rotating sign of inertial machine at that curve)
			 * if it's concentric, and it's full of -1,-2,... we have to change sign
			 * if it's eccentric-concentric, and in the eccentric phase is positive, then we should change sign of both phases
			 */
			int inertialCheckStart = 0;
			int inertialCheckDuration = 0;
			if(encoderConfigurationCurrent.has_inertia) {
				inertialCheckStart = curveStart;
				if(ecconLast == "c")
					inertialCheckDuration = duration;
				else {
					//see if sign is ok just looking if eccentric phase is negative or not
					inertialCheckDuration = Convert.ToInt32(decimal.Truncate(Convert.ToDecimal(curve.Duration)));
				}
			}
		
			//desc = Util.RemoveTildeAndColonAndDot(entry_encoder_curve_comment.Text.ToString());
			desc = "";

			Log.WriteLine(curveStart + "->" + duration);
			int curveIDMax = Sqlite.Max(Constants.EncoderTable, "uniqueID", false);
			
			//save raw file to hard disk
			fileSaved = UtilEncoder.EncoderSaveCurve(UtilEncoder.GetEncoderDataTempFileName(), 
					curveStart, duration,
					inertialCheckStart, inertialCheckDuration, (ecconLast == "c"), 
					currentSession.UniqueID, currentPerson.UniqueID, 
					currentPerson.Name, encoderTimeStamp, curveIDMax);

			//save it to SQL (encoderSignalCurve table)
			SqliteEncoder.SignalCurveInsert(false, 
					Convert.ToInt32(encoderSignalUniqueID), curveIDMax +1,
					Convert.ToInt32(curveStart + (duration /2)));

			path = UtilEncoder.GetEncoderSessionDataCurveDir(currentSession.UniqueID);
		} else { //signal
			//desc = Util.RemoveTildeAndColonAndDot(entry_encoder_signal_comment.Text.ToString());
			desc = "";

			fileSaved = UtilEncoder.CopyTempToEncoderData (currentSession.UniqueID, currentPerson.UniqueID, 
					currentPerson.Name, encoderTimeStamp);
			
			//there was a problem copying
			if(fileSaved == "")
				return "";

			path = UtilEncoder.GetEncoderSessionDataSignalDir(currentSession.UniqueID);
		}

		string myID = "-1";	
		if(mode == "signal")
			myID = encoderSignalUniqueID;

		//assign values from lastEncoderSQLSignal (last calculate curves or reload), and change new things
		EncoderSQL eSQL = lastEncoderSQLSignal;
		eSQL.uniqueID = myID;
		eSQL.signalOrCurve = signalOrCurve;
		eSQL.filename = fileSaved;
		eSQL.url = path;
		eSQL.description = desc;
		if(mode == "curve") {
			eSQL.status = "active";
			eSQL.future1 = meanPowerStr;
		}

		eSQL.encoderConfiguration = encoderConfigurationCurrent;

		
		//if is a signal that we just loaded, then don't insert, do an update
		//we know it because encoderUniqueID is != than "-1" if we loaded something from database
		//This also saves curves
		if(myID == "-1") {
			myID = SqliteEncoder.Insert(false, eSQL).ToString(); //Adding on SQL
			if(mode == "signal") {
				encoderSignalUniqueID = myID;
				feedback = Catalog.GetString("Set saved");
			
				button_video_play_this_test_encoder.Sensitive = false;
				//copy video	
				if(preferences.videoOn) {
					if(Util.CopyTempVideo(currentSession.UniqueID, 
								Constants.TestTypes.ENCODER, 
								Convert.ToInt32(encoderSignalUniqueID))) {
						eSQL.videoURL = Util.GetVideoFileName(currentSession.UniqueID, 
								Constants.TestTypes.ENCODER, 
								Convert.ToInt32(encoderSignalUniqueID));
						//need assign uniqueID to update and add the URL of video
						eSQL.uniqueID = encoderSignalUniqueID;
						SqliteEncoder.Update(false, eSQL);
						button_video_play_this_test_encoder.Sensitive = true;
					} else {
						new DialogMessage(Constants.MessageTypes.WARNING, 
								Catalog.GetString("Sorry, video cannot be stored."));
					}
				}
			}
		}
		else {
			//only signal is updated
			SqliteEncoder.Update(false, eSQL); //Adding on SQL
			feedback = Catalog.GetString("Set updated");
		}
		
		return feedback;
	}


	//TODO: garantir path windows	
	private void on_button_encoder_analyze_clicked (object o, EventArgs args) 
	{
		button_encoder_analyze.Visible = false;
		hbox_encoder_analyze_progress.Visible = true;

		//if userCurves and no data, return
		//TODO: fix this, because curves should be active except in the single curve mode
		if( ! check_encoder_analyze_signal_or_curves.Active) 	//saved curves
		{
			ArrayList data = SqliteEncoder.Select(
					false, -1, currentPerson.UniqueID, currentSession.UniqueID, 
					"curve", EncoderSQL.Eccons.ALL, 
					false, true);

			if(data.Count == 0) {
				new DialogMessage(Constants.MessageTypes.WARNING, 
						Catalog.GetString("Sorry, no repetitions selected."));
				return;
			}
		
			//check on unsupported graph
			string crossNameTemp = 
				Util.FindOnArray(':',1,0,UtilGtk.ComboGetActive(combo_encoder_analyze_cross),
						encoderAnalyzeCrossTranslation);
			if(Util.FindOnArray(':',1,0,UtilGtk.ComboGetActive(combo_encoder_analyze_data_compare),
						encoderDataCompareTranslation) != "No compare" && 
					encoderAnalysis == "cross" &&
					(
					 crossNameTemp == "Speed,Power / Load" || 
					 crossNameTemp == Catalog.GetString("Speed,Power / Load") ||
					 crossNameTemp == "1RM Bench Press" || 
					 crossNameTemp == Catalog.GetString("1RM Bench Press") ||
					 crossNameTemp == "1RM Any exercise" || 
					 crossNameTemp == Catalog.GetString("1RM Any exercise")
					)) {
				new DialogMessage(Constants.MessageTypes.WARNING, 
						Catalog.GetString("Sorry, this graph is not supported yet.") +
						"\n\nSaved repetitions - compare - cross variables" +
						"\n- Speed,Power / Load" +
						"\n- 1RM Bench Press" +
						"\n- 1RM Any exercise"
						);

				return;
			}

		}
	
		encoderThreadStart(encoderActions.ANALYZE);
	}

	//this is called by non gtk thread. Don't do gtk stuff here
	//I suppose reading gtk is ok, changing will be the problem
	private void encoderDoCaptureCsharp () 
	{
		string exerciseNameShown = UtilGtk.ComboGetActive(combo_encoder_exercise);
		bool capturedOk = runEncoderCaptureCsharp( 
				Util.ChangeSpaceAndMinusForUnderscore(currentPerson.Name) + "----" + 
				Util.ChangeSpaceAndMinusForUnderscore(exerciseNameShown) + "----(" + 
				Util.ConvertToPoint(findMass(Constants.MassType.DISPLACED)) + "Kg)",
				//es, 
				(int) encoderCaptureOptionsWin.spin_encoder_capture_time.Value, 
				UtilEncoder.GetEncoderDataTempFileName(),
				chronopicWin.GetEncoderPort() );

		//wait to ensure capture thread has ended
		Thread.Sleep(500);	
				
		capturingCsharp = encoderCaptureProcess.STOPPING;

		//will start calcule curves thread
		if(capturedOk)
			encoderCalculeCurves(encoderActions.CURVES_AC);
	}
	
	//this is called by non gtk thread. Don't do gtk stuff here
	//don't change properties like setting a Visibility status: Gtk.Widget.set_Visible
	//I suppose reading gtk is ok, changing will be the problem
	private void encoderDoCaptureCsharpIM () 
	{
		bool capturedOk = runEncoderCaptureCsharp("Capturing Inertia Moment", 
				encoder_configuration_win.Spin_im_duration,
				UtilEncoder.GetEncoderDataTempFileName(),
				chronopicWin.GetEncoderPort() );

		//wait to ensure capture thread has ended
		Thread.Sleep(500);	

		if(capturedOk)
			UtilEncoder.RunEncoderCalculeIM(
					encoder_configuration_win.Spin_im_weight,
					encoder_configuration_win.Spin_im_length
					);
	}
	
	private bool runEncoderCaptureCsharpCheckPort(string port) {
		Log.WriteLine("00a 1");
		SerialPort sp = new SerialPort(port);
		Log.WriteLine("00b 1");
		sp.BaudRate = 115200;
		Log.WriteLine("00c 1");
		try {
			sp.Open();
			sp.Close();
		} catch {
			return false;
		}
		Log.WriteLine("00d 1");
		return true;
	}
		
	REngine rengine;


	private bool runEncoderCaptureCsharp(string title, int time, string outputData1, string port) 
	{
		int width=encoder_capture_signal_drawingarea.Allocation.Width;
		int height=encoder_capture_signal_drawingarea.Allocation.Height;
		double realHeight = 1000 * 2 * encoderCaptureOptionsWin.spin_encoder_capture_curves_height_range.Value;
		
		Log.Write(" 00a 2 ");
		SerialPort sp = new SerialPort(port);
		Log.Write(" 00b 2 ");
		sp.BaudRate = 115200;
		Log.Write(" 00c 2 ");
		sp.Open();
		Log.Write(" 00d 2 ");
			
		encoderCaptureCountdown = time;
		//int recordingTime = es.Ep.Time * 1000;
		int recordingTime = time * 1000;
		
		//this is what's readed from encoder, as it's linear (non-inverted, not inertial, ...)
		//it's stored in file like this
		int byteReadedRaw;
		//this it's converted applying encoderConfigurationConversions: inverted, inertial, diameter, gearedDown, ...
		double byteReaded;
		
		//initialize
		int [] encoderReadedRaw = new int[recordingTime]; //stored to file in this method
		encoderReaded = new double[recordingTime];	  //readed from drawing process: updateEncoderCaptureGraphRCalc() 
	
		double sum = 0;
		string dataString = "";
		string sep = "";
		
		int i =-20; //delete first records because there's encoder bug
		int msCount = 0;
		encoderCapturePoints = new Gdk.Point[recordingTime];
		encoderCapturePointsCaptured = 0;
		encoderCapturePointsPainted = 0;
				
		/*
		 * calculate params with R explanation	
		 */

		/*               3
		 *              / \
		 *             /   B
		 *            /     \
		 * --1       /
		 *    \     /
		 *     \   A
		 *      \2/
		 *
		 * Record the signal, when arrive to A, then store the descending phase (1-2) and calculate params (power, ...)
		 * When arrive to B, then store the ascending phase (2-3)
		 */

		int directionChangePeriod = 25; //how long (ms) to recognize as change direction. (from 2 to A in ms)
						//it's in ms and not in cm, because it's easier to calculate
		int directionChangeCount = 0; //counter for this period
		int directionNow = 1;		// +1 or -1
		int directionLastMSecond = 1;	// +1 or -1 (direction on last millisecond)
		int directionCompleted = -1;	// +1 or -1
		int previousFrameChange = 0;
		int lastNonZero = 0;
		bool firstCurve = true;

		//this will be used to stop encoder automatically	
		int consecutiveZeros = -1;		
		int consecutiveZerosMax = (int) encoderCaptureOptionsWin.spin_encoder_capture_inactivity_end_time.Value * 1000;

		//create ecca if needed
		if(! eccaCreated) {
			ecca = new EncoderCaptureCurveArray();
			eccaCreated = true;
		}


		do {
			try {
				byteReadedRaw = sp.ReadByte();
			} catch {
				Log.WriteLine("Maybe encoder cable is disconnected");
				encoderProcessCancel = true;
				break;
			}

			if(byteReadedRaw > 128)
				byteReadedRaw = byteReadedRaw - 256;

			byteReaded = UtilEncoder.GetDisplacement(byteReadedRaw, encoderConfigurationCurrent);

			i=i+1;
			if(i >= 0) {
				
				if(byteReaded == 0)
					consecutiveZeros ++;
				else
					consecutiveZeros = -1;
					       
				//stop if n seconds of inactivity
				//but it has to be moved a little bit first, just to give time to the people
				//if(consecutiveZeros >= consecutiveZerosMax && sum > 0) #Not OK becuase sum maybe is 0: +1,+1,-1,-1
				//if(consecutiveZeros >= consecutiveZerosMax && ecca.ecc.Count > 0) #Not ok because when ecca is created, ecc.Count == 1
				//lastNonZero > 0 means something different than 0 has been readed 
				if(consecutiveZeros >= consecutiveZerosMax && lastNonZero > 0)	
				{
					encoderProcessFinish = true;
					Log.WriteLine("SHOULD FINISH");
				}


				sum += byteReaded;
				encoderReaded[i] = byteReaded;
				encoderReadedRaw[i] = byteReadedRaw;

				encoderCapturePoints[i] = new Gdk.Point(
						Convert.ToInt32(width*i/recordingTime),
						Convert.ToInt32( (height/2) - ( sum * height / realHeight) )
						);
				encoderCapturePointsCaptured = i;

				//this slows the process
				//Do not create a large string
				//At end write the data without creating big string
				/*
				dataString += sep + b.ToString();
				sep = ", ";
				*/
	

				/*
				 * calculate params with R (see explanation above)	
				 */
			
				//if string goes up or down
				if(byteReaded != 0) {
					//store the direction
					directionNow = (int) byteReaded / (int) Math.Abs(byteReaded); //1 (up) or -1 (down)
				}
					
				//if we don't have changed the direction, store the last non-zero that we can find
				if(directionChangeCount == 0 && directionNow == directionLastMSecond) {
					//check which is the last non-zero value
					//this is suitable to find where starts the only-zeros previous to the change
					if(byteReaded != 0)
						lastNonZero = i;
				}

				//if it's different than the last direction, mark the start of change
				if(directionNow != directionLastMSecond) {
					directionLastMSecond = directionNow;
					directionChangeCount = 0;
				} 
				else if(directionNow != directionCompleted) {
					//we are in a different direction than the last completed
					
					//we cannot add byteReaded because then is difficult to come back n frames to know the max point
					//directionChangeCount += byteReaded
					directionChangeCount ++;

					//count >= than change_period
					if(directionChangeCount > directionChangePeriod)
					{
						int startFrame = previousFrameChange - directionChangeCount;	//startFrame
								//at startFrame we do the "-directionChangePeriod" because
								//we want data a little bit earlier, because we want some zeros
								//that will be removed by reduceCurveBySpeed
								//if not done, then the data:
								//0 0 0 0 0 0 0 0 0 1
								//will start at 10th digit (the 1)
								//if done, then at speed will be like this:
								//0 0 0 0.01 0.04 0.06 0.07 0.08 0.09 1
								//and will start at fourth digit
						if(startFrame < 0)
							startFrame = 0;

						EncoderCaptureCurve ecc = new EncoderCaptureCurve(
								! Util.IntToBool(directionNow), //if we go now UP, then record previous DOWN phase
								startFrame,
								(i - directionChangeCount + lastNonZero)/2 	//endFrame
								//to find endFrame, first substract directionChangePeriod from i
								//then find the middle point between that and lastNonZero
								);
						ecca.ecc.Add(ecc);


						previousFrameChange = i - directionChangeCount;

						directionChangeCount = 0;
						directionCompleted = directionNow;
					}
				}


				//this is for visual feedback of remaining time	
				msCount ++;
				if(msCount >= 1000) {
					encoderCaptureCountdown --;
					msCount = 1;
				}
			}
		} while (i < (recordingTime -1) && ! encoderProcessCancel && ! encoderProcessFinish);

		Log.Write(" 00e ");
		sp.Close();
		Log.Write(" 00f ");

		if(encoderProcessCancel)
			return false;
		
		TextWriter writer = File.CreateText(outputData1);

		sep = "";
		for(int j=0; j < i ; j ++) {
			writer.Write(sep + encoderReadedRaw[j]); //store the raw file (before encoderConfigurationConversions)
			sep = ", ";
		}

		writer.Flush();
		((IDisposable)writer).Dispose();

		return true;
	}

	//this is called by non gtk thread. Don't do gtk stuff here
	//I suppose reading gtk is ok, changing will be the problem
	private void encoderDoAnalyze () 
	{
		EncoderParams ep = new EncoderParams();
		string dataFileName = "";
		
		string analysisOptions = getEncoderAnalysisOptions(false);

		//use this send because we change it to send it to R
		//but we don't want to change encoderAnalysis because we want to know again if == "cross" 
		//encoderAnalysis can be "cross" and sendAnalysis be "1RMBadillo1010"
		string sendAnalysis = encoderAnalysis;

		//see doProcess at encoder/graph.R
		string analysisVariables = "none"; //cannot be blank

		string crossName = "";
		if(sendAnalysis == "cross") {
			crossName = Util.FindOnArray(':',1,0,UtilGtk.ComboGetActive(combo_encoder_analyze_cross),
						encoderAnalyzeCrossTranslation);
			
			//(crossName == "1RM Any exercise") done below different for curve and signal
			if(crossName == "1RM Bench Press") {
				sendAnalysis = "1RMBadillo2010";
				analysisOptions = "p";
			} else {
				//convert: "Force / Speed" in: "cross.Force.Speed.mean"
				string [] crossNameFull = crossName.Split(new char[] {' '});
				analysisVariables = crossNameFull[0] + ";" + crossNameFull[2]; //[1]=="/"
				if(check_encoder_analyze_mean_or_max.Active)
					analysisVariables += ";mean";
				else
					analysisVariables += ";max";
			}
		}
		
		if(sendAnalysis == "powerBars" || sendAnalysis == "single" || sendAnalysis == "side")
			analysisVariables = getAnalysisVariables(sendAnalysis);

		if( ! check_encoder_analyze_signal_or_curves.Active) 	//saved curves
		{
			string myEccon = "ec";
			if(! check_encoder_analyze_eccon_together.Active)
				myEccon = "ecS";
			int myCurveNum = -1;
			if(sendAnalysis == "single")
				myCurveNum = Convert.ToInt32(UtilGtk.ComboGetActive(
							combo_encoder_analyze_curve_num_combo));

			
			dataFileName = UtilEncoder.GetEncoderGraphInputMulti();

			//neuromuscularProfile works only with ec, do not use c curves
			EncoderSQL.Eccons ecconSelect = EncoderSQL.Eccons.ALL; 	
			if(encoderAnalysis == "neuromuscularProfile") {
				ecconSelect = EncoderSQL.Eccons.ecS; 	
			}

			//double bodyMass = Convert.ToDouble(currentPersonSession.Weight);

			//select curves for this person
			ArrayList data = new ArrayList();

			//select currentPerson, currentSession curves
			//onlyActive is false to have all the curves
			//this is a need for "single" to select on display correct curve
			data = SqliteEncoder.Select(
				false, -1, currentPerson.UniqueID, currentSession.UniqueID, 
				"curve", ecconSelect, 
				false, true);

			//neuromuscularProfile cannot be inerperson or intersession
			if(encoderAnalysis != "neuromuscularProfile") 
			{	
				//if compare persons, select curves for other persons and add
				if(Util.FindOnArray(':',1,0,UtilGtk.ComboGetActive(combo_encoder_analyze_data_compare),
							encoderDataCompareTranslation) == "Between persons") {
					ArrayList dataPre = new ArrayList();
					for (int i=0 ; i < encoderCompareInterperson.Count ; i ++) {
						dataPre = SqliteEncoder.Select(
								false, -1, 
								Util.FetchID(encoderCompareInterperson[i].ToString()),
								currentSession.UniqueID,
							       	"curve", EncoderSQL.Eccons.ALL, 
								true, true);
						//this curves are added to data, data included currentPerson, currentSession
						foreach(EncoderSQL eSQL in dataPre) 
							data.Add(eSQL);
					}
				} else if(Util.FindOnArray(':',1,0,UtilGtk.ComboGetActive(combo_encoder_analyze_data_compare),
							encoderDataCompareTranslation) == "Between sessions") {
					ArrayList dataPre = new ArrayList();
					for (int i=0 ; i < encoderCompareIntersession.Count ; i ++) {
						dataPre = SqliteEncoder.Select(
								false, -1,
								currentPerson.UniqueID, 
								Util.FetchID(encoderCompareIntersession[i].ToString()),
								"curve", EncoderSQL.Eccons.ALL,
							       	true, true);
						//this curves are added to data, data included currentPerson, currentSession
						foreach(EncoderSQL eSQL in dataPre) 
							data.Add(eSQL);
					}
				}
			}

			//1RM is calculated using curves
			//cannot be curves of different exercises
			//because is 1RM of a person on an exercise
			if(encoderAnalysis == "cross" &&
					(crossName == "1RM Bench Press" || crossName == "1RM Any exercise") )
			{
				int count = 0;
				int exerciseOld = -1;
				foreach(EncoderSQL eSQL in data) {
					if(eSQL.status == "active") {
						if(count > 0 && eSQL.exerciseID != exerciseOld) {
							new DialogMessage(Constants.MessageTypes.WARNING, 
									Catalog.GetString("Sorry, cannot calculate 1RM of different exercises."));
							encoderProcessCancel = true;
							return;	
						}

						exerciseOld = eSQL.exerciseID;
						count ++;
					}
				}
				if(crossName == "1RM Any exercise") {
					//get speed1RM (from exercise of curve on SQL, not from combo)
					EncoderExercise exTemp = (EncoderExercise) SqliteEncoder.SelectEncoderExercises(
						false , exerciseOld, false)[0];
				
					sendAnalysis = "1RMAnyExercise";
				        analysisVariables = Util.ConvertToPoint(exTemp.speed1RM) + ";" +
						SqlitePreferences.Select("encoder1RMMethod");
					analysisOptions = "p";
				}
			}

			//-1 because data will be different on any curve
			ep = new EncoderParams(
					-1, 
					-1, 		//exercisePercentBodyWeight
					"-1",		//massBody
					"-1",		//massExtra
					myEccon,	//this decides if analysis will be together or separated
					sendAnalysis,
					analysisVariables,
					analysisOptions,
					encoderConfigurationCurrent,
					Util.ConvertToPoint(preferences.encoderSmoothCon),	//R decimal: '.'
					myCurveNum,
					image_encoder_width, 
					image_encoder_height,
					preferences.CSVExportDecimalSeparator 
					);


			//create dataFileName
			TextWriter writer = File.CreateText(dataFileName);
			writer.WriteLine("status,seriesName,exerciseName,massBody,massExtra,dateTime,fullURL,eccon,percentBodyWeight," + 
					"econfName, econfd, econfD, econfAnglePush, econfAngleWeight, econfInertia, econfGearedDown");
		
			ArrayList eeArray = SqliteEncoder.SelectEncoderExercises(false, -1, false);
			EncoderExercise ex = new EncoderExercise();
						
			Log.WriteLine("AT ANALYZE");

			int iteratingPerson = -1;
			int iteratingSession = -1;
			double iteratingMassBody = -1;
			int countSeries = 1;

			Sqlite.Open();	
			foreach(EncoderSQL eSQL in data) {
				foreach(EncoderExercise eeSearch in eeArray)
					if(eSQL.exerciseID == eeSearch.uniqueID)
						ex = eeSearch;

				Log.Write(" AT ANALYZE 1.1 ");
				//massBody change if we are comparing different persons or sessions
				if(eSQL.personID != iteratingPerson || eSQL.sessionID != iteratingSession) {
					iteratingMassBody = SqlitePersonSession.SelectAttribute(
							true, eSQL.personID, eSQL.sessionID, Constants.Weight);
				}
				Log.Write(" AT ANALYZE 1.2 ");

				//seriesName
				string seriesName = "";
				if(Util.FindOnArray(':',1,0,
							UtilGtk.ComboGetActive(combo_encoder_analyze_data_compare),
						encoderDataCompareTranslation) == "Between persons") 
				{
					foreach(string str in encoderCompareInterperson)
						if(Util.FetchID(str) == eSQL.personID)
							seriesName = Util.FetchName(str);
				} else if(Util.FindOnArray(':',1,0,
							UtilGtk.ComboGetActive(combo_encoder_analyze_data_compare),
						encoderDataCompareTranslation) == "Between sessions") 
				{
					foreach(string str in encoderCompareIntersession) {
						Log.WriteLine(str);
						if(Util.FetchID(str) == eSQL.sessionID)
							seriesName = Util.FetchName(str);
					}
				}
				if(seriesName == "")
					seriesName = currentPerson.Name;

				/*
				 * to avoid problems on reading files from R and strange character encoding
				 * (this problem happens in Parallels (MacOSX)
				 * copy to temp
				 * and tell the csv file that it's in temp
				 */

				string safeFullURL = Path.Combine(Path.GetTempPath(),
						"chronojump_enc_curve_" + countSeries.ToString() + ".txt");
				string fullURL = safeFullURL; 
				
				try {
					File.Copy(eSQL.GetFullURL(false), safeFullURL, true);
					fullURL = fullURL.Replace("\\","/");	//R needs this separator path: "/" in all systems 
				} catch {
					fullURL = eSQL.GetFullURL(true);	//convertPathToR
				}

				writer.WriteLine(eSQL.status + "," + 
						Util.ChangeChars(seriesName,","," ") + "," + //person name cannot have commas
						ex.name + "," +
						Util.ConvertToPoint(iteratingMassBody).ToString() + "," + 
						Util.ConvertToPoint(Convert.ToDouble(eSQL.extraWeight)) + "," +
						eSQL.GetDate(true) + "," + 
						fullURL + "," +	
						eSQL.eccon + "," + 	//this is the eccon of every curve
						ex.percentBodyWeight.ToString() + "," +
						eSQL.encoderConfiguration.ToString(",",true)
						);
				countSeries ++;
			}
			writer.Flush();
			((IDisposable)writer).Dispose();
			Log.Write(" AT ANALYZE 2 ");
			Sqlite.Close();	

		} else {	//current signal
			if(encoderAnalysis == "cross" && crossName == "1RM Any exercise") {
				//get speed1RM (from combo)
				EncoderExercise ex = (EncoderExercise) SqliteEncoder.SelectEncoderExercises(
						false, getExerciseIDFromCombo(), false)[0];
				
				sendAnalysis = "1RMAnyExercise";
			        analysisVariables = Util.ConvertToPoint(ex.speed1RM) + ";" + 
					SqlitePreferences.Select("encoder1RMMethod");
				analysisOptions = "p";
			}

			ep = new EncoderParams(
					(int) encoderCaptureOptionsWin.spin_encoder_capture_min_height.Value, 
					getExercisePercentBodyWeightFromCombo (),
					Util.ConvertToPoint(findMass(Constants.MassType.BODY)),
					Util.ConvertToPoint(findMass(Constants.MassType.EXTRA)),
					findEccon(false),		//do not force ecS (ecc-conc separated)
					sendAnalysis,
					analysisVariables, 
					analysisOptions,
					encoderConfigurationCurrent,
					Util.ConvertToPoint(preferences.encoderSmoothCon),	//R decimal: '.'
					Convert.ToInt32(UtilGtk.ComboGetActive(combo_encoder_analyze_curve_num_combo)),
					image_encoder_width,
					image_encoder_height,
					preferences.CSVExportDecimalSeparator 
					);
			
			dataFileName = UtilEncoder.GetEncoderDataTempFileName();
		}

		EncoderStruct encoderStruct = new EncoderStruct(
				dataFileName, 
				UtilEncoder.GetEncoderGraphTempFileName(),
				UtilEncoder.GetEncoderAnalyzeTableTempFileName(),
				UtilEncoder.GetEncoderStatusTempFileName(),
				UtilEncoder.GetEncoderSpecialDataTempFileName(),
				ep);

		//show mass in title except if it's curves because then can be different mass
		//string massString = "-(" + Util.ConvertToPoint(findMass(true)) + "Kg)";
		//if( ! check_encoder_analyze_signal_or_curves.Active) 	//saved curves
		//	massString = "";

		string titleStr = Util.ChangeSpaceAndMinusForUnderscore(currentPerson.Name);
	
		if(encoderAnalysis == "neuromuscularProfile")
			titleStr = "Neuromuscular Profile" + "-" + titleStr;
		else {
			//on signal show encoder exercise, but not in curves because every curve can be of a different exercise
			if(check_encoder_analyze_signal_or_curves.Active) 	//current signal
				titleStr += "-" + Util.ChangeSpaceAndMinusForUnderscore(UtilGtk.ComboGetActive(combo_encoder_exercise));
		}

		UtilEncoder.RunEncoderGraphNoRDotNet(titleStr, encoderStruct, 
				encoderAnalysis == "neuromuscularProfile",
				preferences.RGraphsTranslate);
	}

	private void on_check_encoder_analyze_signal_or_curves_toggled (object obj, EventArgs args) {
		bool signal = check_encoder_analyze_signal_or_curves.Active;

		if(signal) {
			int rows = UtilGtk.CountRows(encoderCaptureListStore);

			hbox_encoder_user_curves.Visible = false;

			if(ecconLast != "c")
				rows = rows / 2;

			string [] activeCurvesList;
			if(rows == 0)
				activeCurvesList = Util.StringToStringArray("");
			else {
				activeCurvesList = new String[rows];
				for(int i=0; i < rows; i++)
					activeCurvesList[i] = (i+1).ToString();
			}

			UtilGtk.ComboUpdate(combo_encoder_analyze_curve_num_combo, activeCurvesList, "");
			combo_encoder_analyze_curve_num_combo.Active = 
				UtilGtk.ComboMakeActive(combo_encoder_analyze_curve_num_combo, activeCurvesList[0]);

			radiobutton_encoder_analyze_powerbars.Sensitive = true;
			radiobutton_encoder_analyze_single.Sensitive = true;
			radiobutton_encoder_analyze_side.Sensitive = true;
		} 
		else {
			if(currentPerson != null) {
				ArrayList data = SqliteEncoder.Select(
						false, -1, currentPerson.UniqueID, currentSession.UniqueID, 
						"curve", EncoderSQL.Eccons.ALL,
					       	false, true);
				int activeCurvesNum = getActiveCurvesNum(data);
				updateComboEncoderAnalyzeCurveNum(data, activeCurvesNum);	
			}

			hbox_encoder_user_curves.Visible = currentPerson != null;

			radiobutton_encoder_analyze_powerbars.Sensitive = true;
			radiobutton_encoder_analyze_single.Sensitive = true;
			radiobutton_encoder_analyze_side.Sensitive = true;
		}
			
		button_encoder_analyze_sensitiveness();

		image_encoder_analyze_current_signal.Visible 	= signal;
		label_encoder_analyze_current_signal.Visible	= signal;
		image_encoder_analyze_saved_curves.Visible	= ! signal;
		label_encoder_analyze_saved_curves.Visible	= ! signal;
	}

	private string getAnalysisVariables(string encoderAnalysis) 
	{
		string analysisVariables = "none"; //cannot be blank

		if(encoderAnalysis == "powerBars") {
			if(check_encoder_analyze_show_time_to_peak_power.Active)
				analysisVariables = "TimeToPeakPower";
			else
				analysisVariables = "NoTimeToPeakPower";

			if(check_encoder_analyze_show_range.Active)
				analysisVariables += ";Range";
			else
				analysisVariables += ";NoRange";
		}
		else {  //(encoderAnalysis == "single" || encoderAnalysis == "side")
			if(check_encoder_analyze_show_speed.Active)
				analysisVariables = "Speed";
			else
				analysisVariables = "NoSpeed";

			if(check_encoder_analyze_show_accel.Active)
				analysisVariables += ";Accel";
			else
				analysisVariables += ";NoAccel";

			if(check_encoder_analyze_show_force.Active)
				analysisVariables += ";Force";
			else
				analysisVariables += ";NoForce";

			if(check_encoder_analyze_show_power.Active)
				analysisVariables += ";Power";
			else
				analysisVariables += ";NoPower";
		}
		
		return analysisVariables;
	}


	//encoder analysis modes

	private void on_radiobutton_encoder_analyze_single_toggled (object obj, EventArgs args) {
		hbox_encoder_analyze_curve_num.Visible=true;
		hbox_combo_encoder_analyze_curve_num_combo.Visible = true;
		hbox_combo_encoder_analyze_cross.Visible=false;
		check_encoder_analyze_mean_or_max.Visible=false;
		hbox_encoder_analyze_show_powerbars.Visible=false;
		hbox_encoder_analyze_show_SAFE.Visible=true;
		encoderAnalysis = "single";
		//together, mandatory
		check_encoder_analyze_eccon_together.Sensitive=false;
		check_encoder_analyze_eccon_together.Active = true;
	
		button_encoder_analyze_help.Visible = false;
		label_encoder_analyze_side_max.Visible = false;

		//restore 1RM Bench Press sensitiveness
		check_encoder_analyze_mean_or_max.Sensitive = true;
		
		encoderButtonsSensitive(encoderSensEnumStored);
		button_encoder_analyze_sensitiveness();
	}

	/*
	private void on_radiobutton_encoder_analyze_superpose_toggled (object obj, EventArgs args) {
		hbox_encoder_analyze_curve_num.Visible=true;
		hbox_combo_encoder_analyze_curve_num_combo.Visible = true;
		hbox_combo_encoder_analyze_cross.Visible=false;
		check_encoder_analyze_mean_or_max.Visible=false;
		hbox_encoder_analyze_show_powerbars.Visible=false;
		hbox_encoder_analyze_show_SAFE.Visible=true;
		encoderAnalysis="superpose";
		
		//together, mandatory
		check_encoder_analyze_eccon_together.Sensitive=false;
		check_encoder_analyze_eccon_together.Active = true;
		
		button_encoder_analyze_help.Visible = false;
		
		//restore 1RM Bench Press sensitiveness
		check_encoder_analyze_mean_or_max.Sensitive = true;
		
		on_combo_encoder_analyze_cross_changed (obj, args);
		button_encoder_analyze_sensitiveness();
	}
	*/
	private void on_radiobutton_encoder_analyze_side_toggled (object obj, EventArgs args) {
		hbox_encoder_analyze_curve_num.Visible=false;
		hbox_combo_encoder_analyze_curve_num_combo.Visible = false;
		hbox_combo_encoder_analyze_cross.Visible=false;
		check_encoder_analyze_mean_or_max.Visible=false;
		hbox_encoder_analyze_show_powerbars.Visible=false;
		hbox_encoder_analyze_show_SAFE.Visible=true;
		encoderAnalysis = "side";
		
		//together, mandatory
		check_encoder_analyze_eccon_together.Sensitive=false;
		check_encoder_analyze_eccon_together.Active = true;

		button_encoder_analyze_help.Visible = false;
		
		//restore 1RM Bench Press sensitiveness
		check_encoder_analyze_mean_or_max.Sensitive = true;
		
		encoderButtonsSensitive(encoderSensEnumStored);
		button_encoder_analyze_sensitiveness();
	}
	private void on_radiobutton_encoder_analyze_powerbars_toggled (object obj, EventArgs args) {
		hbox_encoder_analyze_curve_num.Visible=false;
		hbox_combo_encoder_analyze_curve_num_combo.Visible = false;
		hbox_combo_encoder_analyze_cross.Visible=false;
		check_encoder_analyze_mean_or_max.Visible=false;
		hbox_encoder_analyze_show_powerbars.Visible=true;
		hbox_encoder_analyze_show_SAFE.Visible=false;
		encoderAnalysis="powerBars";
		
		check_encoder_analyze_eccon_together.Sensitive=true;

		button_encoder_analyze_help.Visible = false;
		label_encoder_analyze_side_max.Visible = false;

		//restore 1RM Bench Press sensitiveness
		check_encoder_analyze_mean_or_max.Sensitive = true;
		
		encoderButtonsSensitive(encoderSensEnumStored);
		button_encoder_analyze_sensitiveness();
	}
	
	private void on_radiobutton_encoder_analyze_cross_toggled (object obj, EventArgs args) {
		hbox_encoder_analyze_curve_num.Visible=false;
		hbox_combo_encoder_analyze_curve_num_combo.Visible = false;
		hbox_combo_encoder_analyze_cross.Visible=true;
		check_encoder_analyze_mean_or_max.Visible=true;
		hbox_encoder_analyze_show_powerbars.Visible=false;
		hbox_encoder_analyze_show_SAFE.Visible=false;
		encoderAnalysis="cross";
		
		check_encoder_analyze_eccon_together.Sensitive=true;

		button_encoder_analyze_help.Visible = false;
		label_encoder_analyze_side_max.Visible = false;

		on_combo_encoder_analyze_cross_changed (obj, args);

		encoderButtonsSensitive(encoderSensEnumStored);
		button_encoder_analyze_sensitiveness();
	}
	
	private void on_radiobutton_encoder_analyze_neuromuscular_profile_toggled (object obj, EventArgs args) {
		hbox_encoder_analyze_curve_num.Visible=false;
		hbox_combo_encoder_analyze_curve_num_combo.Visible = false;
		hbox_combo_encoder_analyze_cross.Visible=false;
		check_encoder_analyze_mean_or_max.Visible=false;
		hbox_encoder_analyze_show_powerbars.Visible=false;
		hbox_encoder_analyze_show_SAFE.Visible=false;
		encoderAnalysis="neuromuscularProfile";
		
		//separated, mandatory
		check_encoder_analyze_eccon_together.Sensitive=false;
		check_encoder_analyze_eccon_together.Active = false;
	
		button_encoder_analyze_help.Visible = true;
		label_encoder_analyze_side_max.Visible = false;
		check_encoder_analyze_mean_or_max.Sensitive = false;
		
		encoderButtonsSensitive(encoderSensEnumStored);
		button_encoder_analyze_sensitiveness();
	}
	
	//end of encoder analysis modes

	private void on_check_encoder_analyze_eccon_together_toggled (object obj, EventArgs args) {
		image_encoder_analyze_eccon_together.Visible = check_encoder_analyze_eccon_together.Active;
		image_encoder_analyze_eccon_separated.Visible = ! check_encoder_analyze_eccon_together.Active;
	}
	
	private void on_check_encoder_analyze_mean_or_max_toggled (object obj, EventArgs args) {
		image_encoder_analyze_mean.Visible = check_encoder_analyze_mean_or_max.Active;
		image_encoder_analyze_max.Visible = ! check_encoder_analyze_mean_or_max.Active;
	}
	
	
	private void on_button_encoder_analyze_help_clicked (object obj, EventArgs args) {
		//currently only active on neuromuscular profile

		string str = 
			Catalog.GetString("About Neuromuscular Profile") + "\n\n" +
			Catalog.GetString("Load = Average eccentric RFD (Ratio of Force Development)") + " (N/s)\n" +
			Catalog.GetString("Explode = Average relative concentric RFD") + " (N/s/kg)\n" +
			Catalog.GetString("Drive = Average relative concentric Impulse") + " (N*s/kg)\n\n" +
			Catalog.GetString("General trends to try to develop an 'equilibrated' neuromuscular profile (always add individual considerations as previous or actual injuries, sport specificity, muscular chains, etc.).") + "\n" +
			Catalog.GetString("If one of the metrics is under developed (weak) compared with the other two, prescribe exercises that emphasize its development.") + "\n" + 
			Catalog.GetString("If one of the metrics is over developed (extreme) compared with the other two, prescribe exercises to emphasize those, but paying attention to flexibility and relaxation of over working muscles.") + "\n\n" +

			Catalog.GetString("General guidelines to improve the neuromuscular profile:") + "\n" +
			
			Catalog.GetString("Load: Perform exercises that develop maximum force (eccentric, isometric or concentric).") + " " +
			Catalog.GetString("Then switch to exercises that develop eccentric force in short time.") + "\n" +

			Catalog.GetString("Explode: Perform exercises that develop force in short time, and maximum force.") + "\n" + 
			Catalog.GetString("Drive: Perform exercises where force is developed during more time.") + "\n\n" +

			Catalog.GetString("Analysis uses the best three jumps using 'jump height' criterion.") + "\n\n" +
			Catalog.GetString("Lapuente and De Blas. Adapted from Wagner:") + "\nhttp://spartapoint.com/category/spartapoint-101/";
		
		new DialogMessage(Constants.MessageTypes.INFO, str);
	}


	private bool curvesNumOkToSideCompare() {
		if(check_encoder_analyze_signal_or_curves.Active && 	//current signal
				(
					(ecconLast == "c" && UtilGtk.CountRows(encoderCaptureListStore) <= 12) ||
					(ecconLast != "c" && UtilGtk.CountRows(encoderCaptureListStore) <= 24)
				) )
			return true;
		else if( ! check_encoder_analyze_signal_or_curves.Active && 	//saved curves
				Convert.ToInt32(label_encoder_user_curves_active_num.Text) <= 12)
			return true;

		return false;
	}

	/*
	private double findMassFromCombo(bool includePerson) {
		double mass = spin_encoder_extra_weight.Value;
		if(includePerson) {
			//TODO: maybe better have a currentEncoderExercise global variable
			if(currentPersonSession.Weight > 0 && getExercisePercentBodyWeightFromCombo() > 0)
				mass += currentPersonSession.Weight * 
					getExercisePercentBodyWeightFromCombo() / 100.0;
		}

		return mass;
	}
	*/

	//BODY and EXTRA are at EncoderParams and sent to graph.R	
	private double findMass(Constants.MassType massType) {
		if(massType == Constants.MassType.BODY)
			return currentPersonSession.Weight;
		else if(massType == Constants.MassType.EXTRA)
			return spin_encoder_extra_weight.Value;
		else //(massType == Constants.MassType.DISPLACED)
			return spin_encoder_extra_weight.Value + 
				( currentPersonSession.Weight * getExercisePercentBodyWeightFromCombo() ) / 100.0;
	}

	//this is used in 1RM return to substract the weight of the body (if used on exercise)
	private double massWithoutPerson(double massTotal, string exerciseName) {
		int percentBodyWeight = getExercisePercentBodyWeightFromName(exerciseName);
		if(currentPersonSession.Weight == 0 || percentBodyWeight == 0 || percentBodyWeight == -1)
			return massTotal;
		else
			return massTotal - (currentPersonSession.Weight * percentBodyWeight / 100.0);
	}

	private string findEccon(bool forceEcconSeparated) {	
		if(Util.FindOnArray(':',1,0,UtilGtk.ComboGetActive(combo_encoder_eccon),
					encoderEcconTranslation) == Constants.Concentric) 
			return "c";
		else //if(Util.FindOnArray(':',1,0,UtilGtk.ComboGetActive(combo_encoder_eccon),
		//			encoderEcconTranslation) == Constants.EccentricConcentric) 
		{
			if(forceEcconSeparated || ! check_encoder_analyze_eccon_together.Active)
				return "ecS";
			else 
				return "ec";
		}
		/*
		 * unavailable until find while concentric data on concentric is the same than in ecc-con,
		 * but is very different than in con-ecc
		else //Constants.ConcentricEccentric
		{
			if(forceEcconSeparated || ! check_encoder_analyze_eccon_together.Active)
				return "ceS";
			else 
				return "ce";
		}
		*/
	}
	
	/* encoder exercise stuff */
	
	
	string [] encoderExercisesTranslationAndBodyPWeight;
	string [] encoderEcconTranslation;
	string [] encoderLateralityTranslation;
	string [] encoderDataCompareTranslation;
	string [] encoderAnalyzeCrossTranslation;
	
	protected void createEncoderCombos() {
		//create combo exercises
		combo_encoder_exercise = ComboBox.NewText ();
		ArrayList encoderExercises = SqliteEncoder.SelectEncoderExercises(false, -1, false);
		encoderExercisesTranslationAndBodyPWeight = new String [encoderExercises.Count];
		string [] exerciseNamesToCombo = new String [encoderExercises.Count];
		int i =0;
		foreach(EncoderExercise ex in encoderExercises) {
			string nameTranslated = Catalog.GetString(ex.name);
			encoderExercisesTranslationAndBodyPWeight[i] = 
				ex.uniqueID + ":" + ex.name + ":" + nameTranslated + ":" + ex.percentBodyWeight;
			exerciseNamesToCombo[i] = Catalog.GetString(ex.name);
			i++;
		}
		UtilGtk.ComboUpdate(combo_encoder_exercise, exerciseNamesToCombo, "");
		combo_encoder_exercise.Active = UtilGtk.ComboMakeActive(combo_encoder_exercise, 
				Catalog.GetString(((EncoderExercise) encoderExercises[0]).name));
		combo_encoder_exercise.Changed += new EventHandler (on_combo_encoder_exercise_changed);
		
		/* ConcentricEccentric
		 * unavailable until find while concentric data on concentric is the same than in ecc-con,
		 * but is very different than in con-ecc
		 */
		//create combo eccon
		string [] comboEcconOptions = { Constants.Concentric, 
			Constants.EccentricConcentric//, 
			//Constants.ConcentricEccentric 
		};
		string [] comboEcconOptionsTranslated = { 
			Catalog.GetString(Constants.Concentric), 
			Catalog.GetString(Constants.EccentricConcentric)//, 
			//Catalog.GetString(Constants.ConcentricEccentric) 
		};
		encoderEcconTranslation = new String [comboEcconOptions.Length];
		//for(int j=0; j < 3 ; j++)
		for(int j=0; j < 2 ; j++)
			encoderEcconTranslation[j] = comboEcconOptions[j] + ":" + comboEcconOptionsTranslated[j];
		combo_encoder_eccon = ComboBox.NewText ();
		UtilGtk.ComboUpdate(combo_encoder_eccon, comboEcconOptionsTranslated, "");
		combo_encoder_eccon.Active = UtilGtk.ComboMakeActive(combo_encoder_eccon, 
				Catalog.GetString(comboEcconOptions[0]));
		combo_encoder_eccon.Changed += new EventHandler (on_combo_encoder_eccon_changed);
		
		//create combo laterality
		string [] comboLateralityOptions = { "RL", "R", "L" };
		string [] comboLateralityOptionsTranslated = { 
			Catalog.GetString("RL"), Catalog.GetString("R"), Catalog.GetString("L") };
		encoderLateralityTranslation = new String [comboLateralityOptions.Length];
		for(int j=0; j < 3 ; j++)
			encoderLateralityTranslation[j] = 
				comboLateralityOptions[j] + ":" + comboLateralityOptionsTranslated[j];
		combo_encoder_laterality = ComboBox.NewText ();
		UtilGtk.ComboUpdate(combo_encoder_laterality, comboLateralityOptionsTranslated, "");
		combo_encoder_laterality.Active = UtilGtk.ComboMakeActive(combo_encoder_laterality, 
				Catalog.GetString(comboLateralityOptions[0]));


		//create combo analyze data compare (variables)
		string [] comboDataCompareOptions = { 
			"No compare", "Between persons", "Between sessions"};
		string [] comboDataCompareOptionsTranslated = { 
			Catalog.GetString("No compare"),
			Catalog.GetString("Between persons"),
			Catalog.GetString("Between sessions")
		};
		encoderDataCompareTranslation = new String [comboDataCompareOptions.Length];
		for(int j=0; j < 3 ; j++)
			encoderDataCompareTranslation[j] = 
				comboDataCompareOptions[j] + ":" + comboDataCompareOptionsTranslated[j];
		combo_encoder_analyze_data_compare = ComboBox.NewText ();
		UtilGtk.ComboUpdate(combo_encoder_analyze_data_compare, comboDataCompareOptionsTranslated, "");
		combo_encoder_analyze_data_compare.Active = UtilGtk.ComboMakeActive(
				combo_encoder_analyze_data_compare, 
				Catalog.GetString(comboDataCompareOptions[0]));
		combo_encoder_analyze_data_compare.Changed += 
			new EventHandler(on_combo_encoder_analyze_data_compare_changed );

		
		//create combo analyze cross (variables)
		string [] comboAnalyzeCrossOptions = { 
			"Speed / Load", "Force / Load", "Power / Load", "Speed,Power / Load", "Force / Speed", "Power / Speed", 
			"1RM Bench Press", "1RM Any exercise"};
		string [] comboAnalyzeCrossOptionsTranslated = { 
			Catalog.GetString("Speed / Load"), Catalog.GetString("Force / Load"), 
			Catalog.GetString("Power / Load"), Catalog.GetString("Speed,Power / Load"), 
			Catalog.GetString("Force / Speed"), Catalog.GetString("Power / Speed"), 
			Catalog.GetString("1RM Bench Press"), Catalog.GetString("1RM Any exercise")
		}; //if added more, change the int in the 'for' below
		encoderAnalyzeCrossTranslation = new String [comboAnalyzeCrossOptions.Length];
		for(int j=0; j < 8 ; j++)
			encoderAnalyzeCrossTranslation[j] = 
				comboAnalyzeCrossOptions[j] + ":" + comboAnalyzeCrossOptionsTranslated[j];
		combo_encoder_analyze_cross = ComboBox.NewText ();
		UtilGtk.ComboUpdate(combo_encoder_analyze_cross, comboAnalyzeCrossOptionsTranslated, "");
		combo_encoder_analyze_cross.Active = UtilGtk.ComboMakeActive(combo_encoder_analyze_cross, 
				Catalog.GetString(comboAnalyzeCrossOptions[0]));
		combo_encoder_analyze_cross.Changed += new EventHandler (on_combo_encoder_analyze_cross_changed);

		//create combo analyze curve num combo
		//is not an spinbutton because values can be separated: "3,4,7,21"
		combo_encoder_analyze_curve_num_combo = ComboBox.NewText ();
		UtilGtk.ComboUpdate(combo_encoder_analyze_curve_num_combo, Util.StringToStringArray(""), "");
		
		
		//pack combos

		hbox_combo_encoder_exercise.PackStart(combo_encoder_exercise, true, true, 0);
		hbox_combo_encoder_exercise.ShowAll();
		combo_encoder_exercise.Sensitive = true;

		hbox_combo_encoder_eccon.PackStart(combo_encoder_eccon, true, true, 0);
		hbox_combo_encoder_eccon.ShowAll();
		combo_encoder_eccon.Sensitive = true;

		hbox_combo_encoder_laterality.PackStart(combo_encoder_laterality, true, true, 0);
		hbox_combo_encoder_laterality.ShowAll();
		combo_encoder_laterality.Sensitive = true;
		
		hbox_encoder_analyze_data_compare.PackStart(combo_encoder_analyze_data_compare, true, true, 0);
		hbox_encoder_analyze_data_compare.ShowAll();
		combo_encoder_analyze_data_compare.Sensitive = true;
	
		hbox_combo_encoder_analyze_cross.PackStart(combo_encoder_analyze_cross, true, true, 0);
		hbox_combo_encoder_analyze_cross.ShowAll(); 
		combo_encoder_analyze_cross.Sensitive = true;
		hbox_combo_encoder_analyze_cross.Visible = false; //do not show hbox at start
	
		hbox_combo_encoder_analyze_curve_num_combo.PackStart(combo_encoder_analyze_curve_num_combo, true, true, 0);
		hbox_combo_encoder_analyze_curve_num_combo.ShowAll(); 
		combo_encoder_analyze_curve_num_combo.Sensitive = true;
		hbox_combo_encoder_analyze_curve_num_combo.Visible = false; //do not show hbox at start
	}

	void on_combo_encoder_eccon_changed (object o, EventArgs args) 
	{
	}

	void on_button_encoder_capture_curves_all_clicked (object o, EventArgs args) {
		encoderCaptureSaveCurvesAllNoneBest(Constants.EncoderAutoSaveCurve.ALL);
	}
	void on_button_encoder_capture_curves_best_clicked (object o, EventArgs args) {
		encoderCaptureSaveCurvesAllNoneBest(Constants.EncoderAutoSaveCurve.BESTMEANPOWER);
	}
	void on_button_encoder_capture_curves_none_clicked (object o, EventArgs args) {
		encoderCaptureSaveCurvesAllNoneBest(Constants.EncoderAutoSaveCurve.NONE);
	}

	void on_combo_encoder_analyze_data_compare_changed (object o, EventArgs args)
	{
		if(Util.FindOnArray(':',1,0,UtilGtk.ComboGetActive(combo_encoder_analyze_data_compare),
					encoderDataCompareTranslation) == "No compare") {
			radiobutton_encoder_analyze_powerbars.Sensitive = true;
			radiobutton_encoder_analyze_single.Sensitive = true;
			radiobutton_encoder_analyze_side.Sensitive = true;
			button_encoder_analyze_data_compare.Visible = false;
		} else {
			radiobutton_encoder_analyze_powerbars.Sensitive = false;
			radiobutton_encoder_analyze_single.Sensitive = false;
			radiobutton_encoder_analyze_side.Sensitive = false;
			radiobutton_encoder_analyze_cross.Active = true;
			button_encoder_analyze_data_compare.Visible = true;

			//put some data just in case user doesn't click on compare button
			encoderCompareInitialize();
		}
	}

	//put some data just in case user doesn't click on compare button
	private void encoderCompareInitialize() {
		if(encoderCompareInterperson == null)
			encoderCompareInterperson = new ArrayList ();
		if(encoderCompareIntersession == null)
			encoderCompareIntersession = new ArrayList ();
	}

	void on_combo_encoder_analyze_cross_changed (object o, EventArgs args)
	{
		if(Util.FindOnArray(':',1,0,UtilGtk.ComboGetActive(combo_encoder_analyze_cross),
					encoderAnalyzeCrossTranslation) == "1RM Bench Press" ||
				Util.FindOnArray(':',1,0,UtilGtk.ComboGetActive(combo_encoder_analyze_cross),
					encoderAnalyzeCrossTranslation) == "1RM Any exercise" ) {
			check_encoder_analyze_mean_or_max.Active = true;
			check_encoder_analyze_mean_or_max.Sensitive = false;
			check_encoder_analyze_eccon_together.Active = false;
			check_encoder_analyze_eccon_together.Sensitive = false;
		} else {
			check_encoder_analyze_mean_or_max.Sensitive = true;
			check_encoder_analyze_eccon_together.Sensitive = true;
		}
	}
	
	void on_button_encoder_analyze_image_save_clicked (object o, EventArgs args)
	{
		/* file is in:
		 * /tmp/chronojump-last-encoder-graph.png
		 * but if a capture curves has done, file is named the same
		 * make unsensitive the capture image after loading or capturing a new signal
		 * or changing person, loading session, ...
		 */

		checkFile(Constants.EncoderCheckFileOp.ANALYZE_SAVE_IMAGE);
	}
	void on_button_encoder_save_image_file_selected (string destination)
	{
		try {
			File.Copy(UtilEncoder.GetEncoderGraphTempFileName(), destination, true);
		} catch {
			string myString = string.Format(
					Catalog.GetString("Cannot save file {0} "), destination);
			new DialogMessage(Constants.MessageTypes.WARNING, myString);
		}
	}

	void on_button_encoder_analyze_table_save_clicked (object o, EventArgs args)
	{
		/* file is in:
		 * /tmp/chronojump-last-encoder-curves.txt
		 * but if a capture curves has done, file is named the same
		 * make unsensitive the capture table after loading or capturing a new signal
		 * or changing person, loading session, ...
		 * No problem. Is nice to play with seinsitiveness, but the reading will be from treeview and not from file
		 */

		checkFile(Constants.EncoderCheckFileOp.ANALYZE_SAVE_TABLE);
	}

	void on_button_encoder_save_table_file_selected (string destination)
	{
		try {
			//this overwrites if needed
			TextWriter writer = File.CreateText(destination);

			if(lastTreeviewEncoderAnalyzeIsNeuromuscular) {
				//write header
				writer.WriteLine(Util.RemoveNewLine(Util.StringArrayToString(
							treeviewEncoderAnalyzeNeuromuscularHeaders, ";"), false));
				//write curves rows
				ArrayList array = getTreeViewNeuromuscular(encoderAnalyzeListStore);
				foreach (EncoderNeuromuscularData nm in array)
					writer.WriteLine(nm.ToCSV());
			} else {
				//write header
				writer.WriteLine(Util.RemoveNewLine(Util.StringArrayToString(
							treeviewEncoderAnalyzeHeaders, ";"), false));
				//write curves rows
				ArrayList array = getTreeViewCurves(encoderAnalyzeListStore);
				foreach (EncoderCurve ec in array)
					writer.WriteLine(ec.ToCSV());
			}
			
			writer.Flush();
			((IDisposable)writer).Dispose();
		} catch {
			string myString = string.Format(
					Catalog.GetString("Cannot save file {0} "), destination);
			new DialogMessage(Constants.MessageTypes.WARNING, myString);
		}
	}

	void on_button_encoder_analyze_1RM_save_clicked (object o, EventArgs args)
	{
		string contents = Util.ReadFile(UtilEncoder.GetEncoderSpecialDataTempFileName(), true);
		string [] load1RMStr = contents.Split(new char[] {';'});
		double load1RM = Convert.ToDouble(Util.ChangeDecimalSeparator(load1RMStr[1]));

		if(load1RM == -1) {
			new DialogMessage(Constants.MessageTypes.WARNING, Catalog.GetString("Not enough data."));
			return;
		}

		//save it without the body weight
		double load1RMWithoutPerson = massWithoutPerson(load1RM,getExerciseNameFromTable());

		SqliteEncoder.Insert1RM(false, currentPerson.UniqueID, currentSession.UniqueID, 
				getExerciseIDFromTable(), load1RMWithoutPerson);
		
		string myString = Catalog.GetString("Saved.");
		if(load1RM != load1RMWithoutPerson)
			myString = string.Format(Catalog.GetString("1RM found: {0} Kg."), load1RM) + "\n" + 
				string.Format(Catalog.GetString("Displaced body weight in this exercise: {0}%."), 
						getExercisePercentBodyWeightFromTable()) + "\n" +
				string.Format(Catalog.GetString("Saved 1RM without displaced body weight: {0} Kg."), 
						load1RMWithoutPerson);
		
		encoder_change_displaced_weight_and_1RM ();
		new DialogMessage(Constants.MessageTypes.INFO, myString);
	}



	int getExerciseIDFromName (string name) {
		string idFound = Util.FindOnArray(':', 2, 0, name, 
				encoderExercisesTranslationAndBodyPWeight);
		if(Util.IsNumber(idFound, false))
			return Convert.ToInt32(idFound);
		else
			return -1;
	}
	int getExerciseIDFromCombo () {
		return getExerciseIDFromName (UtilGtk.ComboGetActive(combo_encoder_exercise));
	}
	int getExerciseIDFromTable () {
		return getExerciseIDFromName (getExerciseNameFromTable());
	}
	
	string getExerciseNameFromTable () { //from first data row
		ArrayList array = getTreeViewCurves(encoderAnalyzeListStore);
		return ( (EncoderCurve) array[0] ).Exercise;
	}

	int getExercisePercentBodyWeightFromName (string name) {
		string found = Util.FindOnArray(':', 2, 3, name, 
				encoderExercisesTranslationAndBodyPWeight);
		if(Util.IsNumber(found, false))
			return Convert.ToInt32(found);
		else {
			//try untranslated
			found = Util.FindOnArray(':', 1, 3, name, 
					encoderExercisesTranslationAndBodyPWeight);
			if(Util.IsNumber(found, false))
				return Convert.ToInt32(found);
			else
				return -1;
		}
	}
	int getExercisePercentBodyWeightFromCombo () {
		return getExercisePercentBodyWeightFromName (UtilGtk.ComboGetActive(combo_encoder_exercise));
	}
	int getExercisePercentBodyWeightFromTable () { //from first data row
		ArrayList array = getTreeViewCurves(encoderAnalyzeListStore);
		string exerciseName = ( (EncoderCurve) array[0] ).Exercise;
		return getExercisePercentBodyWeightFromName (exerciseName);
	}


	// ---------end of helpful methods -----------


	void on_button_encoder_exercise_info_clicked (object o, EventArgs args) 
	{
		EncoderExercise ex = (EncoderExercise) SqliteEncoder.SelectEncoderExercises(
				false, getExerciseIDFromCombo(), false)[0];

		ArrayList bigArray = new ArrayList();

		ArrayList a1 = new ArrayList();
		ArrayList a2 = new ArrayList();
		ArrayList a3 = new ArrayList();
		ArrayList a4 = new ArrayList();
		ArrayList a5 = new ArrayList();

		//0 is the widgget to show; 1 is the editable; 2 id default value
		a1.Add(Constants.GenericWindowShow.ENTRY); a1.Add(false); a1.Add(ex.name);
		bigArray.Add(a1);

		a2.Add(Constants.GenericWindowShow.SPININT); a2.Add(false); a2.Add("");
		bigArray.Add(a2);
		
		a3.Add(Constants.GenericWindowShow.ENTRY2); a3.Add(false); a3.Add(ex.ressistance);
		bigArray.Add(a3);
		
		a4.Add(Constants.GenericWindowShow.ENTRY3); a4.Add(false); a4.Add(ex.description);
		bigArray.Add(a4);
		
		a5.Add(Constants.GenericWindowShow.HBOXSPINDOUBLE2); a5.Add(true); a5.Add("");	//alowed to change
		bigArray.Add(a5);
		
		
		genericWin = GenericWindow.Show(false, Catalog.GetString("Encoder exercise name:"), bigArray);
		genericWin.LabelSpinInt = Catalog.GetString("Displaced body weight") + " (%)";
		genericWin.SetSpinRange(ex.percentBodyWeight, ex.percentBodyWeight); //done this because IsEditable does not affect the cursors
		genericWin.LabelEntry2 = Catalog.GetString("Resistance");
		genericWin.LabelEntry3 = Catalog.GetString("Description");
		genericWin.LabelSpinDouble2 = Catalog.GetString("Speed at 1RM");
		genericWin.SetSpinDouble2Value(ex.speed1RM);
		genericWin.SetSpinDouble2Increments(0.001,0.1);
		/*
		 * Now this is in encoder configuration
		genericWin.LabelSpinInt2 = Catalog.GetString("Body angle") + " (º)";
		genericWin.SetSpin2Range(ex.bodyAngle,ex.bodyAngle); //done this because IsEditable does not affect the cursors
		genericWin.LabelSpinInt3 = Catalog.GetString("Weight angle") + " (º)";
		genericWin.SetSpin3Range(ex.weightAngle,ex.weightAngle); //done this because IsEditable does not affect the cursors
		*/
		genericWin.ShowButtonCancel(false);
		
		genericWin.Button_accept.Clicked += new EventHandler(on_button_encoder_exercise_info_accepted);
		genericWin.ShowNow();
	}

	void on_button_encoder_exercise_add_clicked (object o, EventArgs args) 
	{
		ArrayList bigArray = new ArrayList();

		ArrayList a1 = new ArrayList();
		ArrayList a2 = new ArrayList();
		ArrayList a3 = new ArrayList();
		ArrayList a4 = new ArrayList();
		ArrayList a5 = new ArrayList();

		//0 is the widgget to show; 1 is the editable; 2 id default value
		a1.Add(Constants.GenericWindowShow.ENTRY); a1.Add(true); a1.Add("");
		bigArray.Add(a1);

		a2.Add(Constants.GenericWindowShow.SPININT); a2.Add(true); a2.Add("");
		bigArray.Add(a2);
		
		a3.Add(Constants.GenericWindowShow.ENTRY2); a3.Add(true); a3.Add("");
		bigArray.Add(a3);
		
		a4.Add(Constants.GenericWindowShow.ENTRY3); a4.Add(true); a4.Add("");
		bigArray.Add(a4);
		
		a5.Add(Constants.GenericWindowShow.HBOXSPINDOUBLE2); a5.Add(true); a5.Add("");
		bigArray.Add(a5);
		
		
		genericWin = GenericWindow.Show(false,	//don't show now
				Catalog.GetString("Write the name of the encoder exercise:"), bigArray);
		genericWin.LabelSpinInt = Catalog.GetString("Displaced body weight") + " (%)";
		genericWin.SetSpinRange(0, 100);
		genericWin.LabelEntry2 = Catalog.GetString("Ressitance");
		genericWin.LabelEntry3 = Catalog.GetString("Description");
		genericWin.LabelSpinDouble2 = Catalog.GetString("Speed at 1RM");
		genericWin.SetSpinDouble2Increments(0.001,0.1);
		/*
		 * Now this is in encoder configuration
		genericWin.LabelSpinInt2 = Catalog.GetString("Body angle") + " (º)";
		genericWin.SetSpin2Range(0,90);
		genericWin.SetSpin2Value(90);
		genericWin.LabelSpinInt3 = Catalog.GetString("Weight angle") + " (º)";
		genericWin.SetSpin3Range(0,90);
		genericWin.SetSpin3Value(90);
		*/
		genericWin.SetButtonAcceptLabel(Catalog.GetString("Add"));
		
		genericWin.HideOnAccept = false;

		genericWin.Button_accept.Clicked += new EventHandler(on_button_encoder_exercise_add_accepted);
		genericWin.ShowNow();
	}
	
	void on_button_encoder_exercise_info_accepted (object o, EventArgs args) {
		encoder_exercise_edit(false);
		genericWin.Button_accept.Clicked -= new EventHandler(on_button_encoder_exercise_info_accepted);
	}
	void on_button_encoder_exercise_add_accepted (object o, EventArgs args) {
		encoder_exercise_edit(true);
		genericWin.Button_accept.Clicked -= new EventHandler(on_button_encoder_exercise_add_accepted);
	}
	
	void encoder_exercise_edit (bool adding) 
	{
		string name = Util.RemoveTildeAndColonAndDot(genericWin.EntrySelected);

		if(adding)
			Log.WriteLine("Trying to insert: " + name);
		else
			Log.WriteLine("Trying to edit: " + name);

		if(name == "")
			genericWin.SetLabelError(Catalog.GetString("Error: Missing name of exercise."));
		else if (adding && Sqlite.Exists(false, Constants.EncoderExerciseTable, name))
			genericWin.SetLabelError(string.Format(Catalog.GetString(
							"Error: An exercise named '{0}' already exists."), name));
		else {
			if(adding)
				SqliteEncoder.InsertExercise(false, name, genericWin.SpinIntSelected, 
						genericWin.Entry2Selected, genericWin.Entry3Selected,
						Util.ConvertToPoint(genericWin.SpinDouble2Selected)
						);
			else
				SqliteEncoder.UpdateExercise(false, name, genericWin.SpinIntSelected, 
						genericWin.Entry2Selected, genericWin.Entry3Selected,
						Util.ConvertToPoint(genericWin.SpinDouble2Selected)
						);

			ArrayList encoderExercises = SqliteEncoder.SelectEncoderExercises(false,-1, false);
			encoderExercisesTranslationAndBodyPWeight = new String [encoderExercises.Count];
			string [] exerciseNamesToCombo = new String [encoderExercises.Count];
			int i =0;
			foreach(EncoderExercise ex in encoderExercises) {
				string nameTranslated = ex.name;
				//do not translated user created exercises
				//this names are in SqliteEncoder.initializeTableEncoderExercise()
				if(ex.name == "Bench press" || ex.name == "Squat" || ex.name == "Jump")
					nameTranslated = Catalog.GetString(ex.name);
				encoderExercisesTranslationAndBodyPWeight[i] = 
					ex.uniqueID + ":" + ex.name + ":" + nameTranslated + ":" + ex.percentBodyWeight;
				exerciseNamesToCombo[i] = Catalog.GetString(ex.name);
				i++;
			}
			UtilGtk.ComboUpdate(combo_encoder_exercise, exerciseNamesToCombo, "");
			combo_encoder_exercise.Active = UtilGtk.ComboMakeActive(combo_encoder_exercise, name);

			genericWin.HideAndNull();
			Log.WriteLine("done");
		}
	}


	/* sensitivity stuff */	
			
	//called when a person changes
	private void encoderPersonChanged() {
		ArrayList data = SqliteEncoder.Select(
				false, -1, currentPerson.UniqueID, currentSession.UniqueID, 
				"curve", EncoderSQL.Eccons.ALL, 
				false, true);
		
		int activeCurvesNum = getActiveCurvesNum(data);
		label_encoder_user_curves_active_num.Text = activeCurvesNum.ToString();
		
		label_encoder_user_curves_all_num.Text = data.Count.ToString();
	
		if(check_encoder_analyze_signal_or_curves.Active)	//current signal
		{
			int rows = UtilGtk.CountRows(encoderCaptureListStore);
			if(ecconLast != "c")
				rows = rows / 2;
			string [] activeCurvesList;
			if(rows == 0)
 				activeCurvesList = Util.StringToStringArray("");
			else {
				activeCurvesList = new String[rows];
				for(int i=0; i < rows; i++)
					activeCurvesList[i] = (i+1).ToString();
			}
	
			UtilGtk.ComboUpdate(combo_encoder_analyze_curve_num_combo, activeCurvesList, "");
			combo_encoder_analyze_curve_num_combo.Active = 
				UtilGtk.ComboMakeActive(combo_encoder_analyze_curve_num_combo, activeCurvesList[0]);
		} else {	//current signal
			updateComboEncoderAnalyzeCurveNum(data, activeCurvesNum);	
		}
	
		encoderButtonsSensitive(encoderSensEnum.YESPERSON);
		button_encoder_analyze_sensitiveness();
		treeviewEncoderCaptureRemoveColumns();
		if(encoder_capture_curves_bars_pixmap != null) 
			UtilGtk.ErasePaint(encoder_capture_curves_bars_drawingarea, encoder_capture_curves_bars_pixmap);
		image_encoder_capture.Sensitive = false;
		image_encoder_analyze.Sensitive = false;
		treeview_encoder_analyze_curves.Sensitive = false;
		button_encoder_analyze_image_save.Sensitive = false;
		button_encoder_analyze_table_save.Sensitive = false;
		button_encoder_analyze_1RM_save.Sensitive = false;

		//put some data just in case user doesn't click on compare button
		encoderCompareInitialize();
		
		encoder_change_displaced_weight_and_1RM ();
	}

	private void encoderButtonsSensitive(encoderSensEnum option) {
		//columns
		//c0 button_encoder_capture
		//c1 button_encoder_recalculate
		//c2 button_encoder_load_signal
		//c3 hbox_encoder_capture_curves_save_all_none, button_encoder_export_all_curves,
		//	button_encoder_delete_signal, entry_encoder_signal_comment,
		//	and images: image_encoder_capture , image_encoder_analyze.Sensitive. Update: both NOT managed here
		//UNUSED c4 button_encoder_save_curve, entry_encoder_curve_comment
		//c5 button_encoder_analyze
		//c6 hbox_encoder_user_curves
		//c7 button_encoder_capture_cancel (on capture and analyze)
		//c8 button_encoder_capture_finish (only on capture)

		//other dependencies
		//c5 True needs 
		//	(signal && treeviewEncoder has rows) || 
		//	(! check_encoder_analyze_signal_or_curves.Active && user has curves))
		//c6 True needs ! check_encoder_analyze_signal_or_curves.Active

		if(option != encoderSensEnum.PROCESSINGCAPTURE && option != encoderSensEnum.PROCESSINGR)
			encoderSensEnumStored = option;
		
		//columns		 	 0  1  2  3  4  5  6  7  8
		int [] noSession = 		{0, 0, 0, 0, 0, 0, 0, 0, 0};
		int [] noPerson = 		{0, 0, 0, 0, 0, 0, 0, 0, 0};
		int [] yesPerson = 		{1, 0, 1, 0, 0, 1, 1, 0, 0};
		int [] processingCapture = 	{0, 0, 0, 0, 0, 0, 1, 1, 1};
		int [] processingR = 		{0, 0, 0, 0, 0, 0, 1, 1, 0};
		int [] doneNoSignal = 		{1, 1, 1, 0, 0, 1, 1, 0, 0};
		int [] doneYesSignal = 		{1, 1, 1, 1, 0, 1, 1, 0, 0};
		int [] selectedCurve = 		{1, 1, 1, 1, 1, 1, 1, 0, 0};
		int [] table = new int[7];

		switch(option) {
			case encoderSensEnum.NOSESSION:
				table = noSession;
				break;
			case encoderSensEnum.NOPERSON:
				table = noPerson;
				break;
			case encoderSensEnum.YESPERSON:
				table = yesPerson;
				break;
			case encoderSensEnum.PROCESSINGCAPTURE:
				table = processingCapture;
				break;
			case encoderSensEnum.PROCESSINGR:
				table = processingR;
				break;
			case encoderSensEnum.DONENOSIGNAL:
				table = doneNoSignal;
				break;
			case encoderSensEnum.DONEYESSIGNAL:
				table = doneYesSignal;
				break;
			case encoderSensEnum.SELECTEDCURVE:
				table = selectedCurve;
				break;
		}

		button_encoder_capture.Sensitive = Util.IntToBool(table[0]);

		button_encoder_recalculate.Sensitive = Util.IntToBool(table[1]);
		
		button_encoder_load_signal.Sensitive = Util.IntToBool(table[2]);
		
		hbox_encoder_capture_curves_save_all_none.Sensitive = Util.IntToBool(table[3]);
		button_encoder_export_all_curves.Sensitive = Util.IntToBool(table[3]);
		button_encoder_delete_signal.Sensitive = Util.IntToBool(table[3]);
		//image_encoder_capture.Sensitive = Util.IntToBool(table[3]);
		//image_encoder_analyze.Sensitive = Util.IntToBool(table[3]);
		
		//button_encoder_save_curve.Sensitive = Util.IntToBool(table[4]);
		//entry_encoder_curve_comment.Sensitive = Util.IntToBool(table[4]);

		bool analyze_sensitive = 
			(
			 Util.IntToBool(table[5]) && 
			 (
			  (check_encoder_analyze_signal_or_curves.Active &&
			   UtilGtk.CountRows(encoderCaptureListStore) > 0) 
			  ||
			  ( ! check_encoder_analyze_signal_or_curves.Active &&
			   Convert.ToInt32(label_encoder_user_curves_all_num.Text) >0)
			  )
			 );
		if(analyze_sensitive && radiobutton_encoder_analyze_side.Active) {
			analyze_sensitive = curvesNumOkToSideCompare();
			label_encoder_analyze_side_max.Visible = ! analyze_sensitive;
		} else
			label_encoder_analyze_side_max.Visible = false;
		button_encoder_analyze.Sensitive = analyze_sensitive;

		hbox_encoder_user_curves.Visible = 
			(Util.IntToBool(table[6]) && ! check_encoder_analyze_signal_or_curves.Active);
		
		button_encoder_capture_cancel.Sensitive = Util.IntToBool(table[7]);
		button_encoder_analyze_cancel.Sensitive = Util.IntToBool(table[7]);
		
		button_encoder_capture_finish.Sensitive = Util.IntToBool(table[8]);
	}
	
	private void button_encoder_analyze_sensitiveness() {
		bool analyze_sensitive = false;
		bool signal = check_encoder_analyze_signal_or_curves.Active;
		if(signal) {
			int rows = UtilGtk.CountRows(encoderCaptureListStore);
			
			//button_encoder_analyze.Sensitive = encoderTimeStamp != null;
			
			analyze_sensitive = (rows > 0);
			if(analyze_sensitive && radiobutton_encoder_analyze_side.Active) {
				analyze_sensitive = curvesNumOkToSideCompare();
				label_encoder_analyze_side_max.Visible = ! analyze_sensitive;
			}
		} else {
			analyze_sensitive = (currentPerson != null && 
					UtilGtk.ComboGetActive(combo_encoder_analyze_curve_num_combo) != "");
			if(analyze_sensitive && radiobutton_encoder_analyze_side.Active) {
				analyze_sensitive = curvesNumOkToSideCompare();
				label_encoder_analyze_side_max.Visible = ! analyze_sensitive;
			}
		}
		button_encoder_analyze.Sensitive = analyze_sensitive;
	}


	/* end of sensitivity stuff */	
	
	/*
	 * update encoder capture graph stuff
	 */

	private void updateEncoderCaptureGraph(bool graphSignal, bool calcCurves, bool plotCurvesBars) 
	{
		if(encoderCapturePoints != null) 
		{
			if(graphSignal)
				updateEncoderCaptureGraphPaint(); 
			if(calcCurves)
				updateEncoderCaptureGraphRCalcPre(plotCurvesBars); 
		}
	}
	
	private void updateEncoderCaptureGraphPaint() 
	{
		bool refreshAreaOnly = false;

		//also can be optimized to do not erase window every time and only add points since last time
		int last = encoderCapturePointsCaptured;
		int toDraw = encoderCapturePointsCaptured - encoderCapturePointsPainted;

		//Log.WriteLine("last - toDraw:" + last + " - " + toDraw);	

		//fixes crash at the end
		if(toDraw == 0)
			return;

		int maxY=-1;
		int minY=10000;
		Gdk.Point [] paintPoints = new Gdk.Point[toDraw];
		for(int j=0, i=encoderCapturePointsPainted +1 ; i <= last ; i ++, j++) 
		{
			paintPoints[j] = encoderCapturePoints[i];

			if(refreshAreaOnly) {
				if(encoderCapturePoints[i].Y > maxY)
					maxY = encoderCapturePoints[i].Y;
				if(encoderCapturePoints[i].Y < minY)
					minY = encoderCapturePoints[i].Y;
			}

		}

		encoder_capture_signal_pixmap.DrawPoints(pen_black_encoder_capture, paintPoints);

		layout_encoder_capture_signal.SetMarkup(currentPerson.Name + " (" + 
				spin_encoder_extra_weight.Value.ToString() + "Kg)");
		encoder_capture_signal_pixmap.DrawLayout(pen_azul_encoder_capture, 5, 5, layout_encoder_capture_signal);

		if(refreshAreaOnly) {
			/*			
						Log.WriteLine("pp X-TD-W: " + 
						paintPoints[0].X.ToString() + " - " + 
						paintPoints[toDraw-1].X.ToString() + " - " + 
						(paintPoints[toDraw-1].X-paintPoints[0].X).ToString());
						*/

			int startX = paintPoints[0].X;
			/*
			 * this helps to ensure that no white points are drawed
			 * caused by this int when encoderCapturePoints are assigned:
			 * Convert.ToInt32(width*i/recordingTime)
			 */
			int exposeMargin = 4;
			if(startX -exposeMargin > 0)
				startX -= exposeMargin;	


			encoder_capture_signal_drawingarea.QueueDrawArea( 			// -- refresh
					startX,
					//0,
					minY,
					(paintPoints[toDraw-1].X-paintPoints[0].X ) + exposeMargin,
					//encoder_capture_signal_drawingarea.Allocation.Height
					maxY-minY
					);
			Log.WriteLine("minY - maxY " + minY + " - " + maxY);
		} else
			encoder_capture_signal_drawingarea.QueueDraw(); 			// -- refresh

		encoderCapturePointsPainted = encoderCapturePointsCaptured;
	}

	string encoderCaptureStringR;
	double massDisplacedEncoder = 0;
	ArrayList captureCurvesBarsData;
	static bool updatingEncoderCaptureGraphRCalc;
	
	private void updateEncoderCaptureGraphRCalcPre(bool plotCurvesBars) 
	{
		Log.WriteLine(" PERFORMING CALCULATIONS A ");
	
		//check if this helps to show bars on slow computers
		if(! updatingEncoderCaptureGraphRCalc) {
			updateEncoderCaptureGraphRCalc(plotCurvesBars);
			updatingEncoderCaptureGraphRCalc = false;
		}
	}
	
	private void updateEncoderCaptureGraphRCalc(bool plotCurvesBars) 
	{
		if(RInitialized == Constants.Status.UNSTARTED || RInitialized == Constants.Status.ERROR)
			return;
		
		if(! eccaCreated)
			return;
		if(ecca.ecc.Count <= ecca.curvesDone) 
			return;
		
		updatingEncoderCaptureGraphRCalc = true;
		Log.WriteLine(" PERFORMING CALCULATIONS B ");

		EncoderCaptureCurve ecc = (EncoderCaptureCurve) ecca.ecc[ecca.curvesDone];
		Log.Write("\n" + ecc.DirectionAsString() + " " + ecc.startFrame.ToString() + " " + ecc.endFrame.ToString());
		
		string eccon = findEccon(true);
			
		Log.Write(" uECGRC0 ");
		
		//if eccon == "c" only up phase
		if( ( ( eccon == "c" && ecc.up ) || eccon != "c" ) &&
				(ecc.endFrame - ecc.startFrame) > 0 ) 
		{
			Log.Write(" uECGRC1 ");
			
			double height = 0;

			double [] curve = new double[ecc.endFrame - ecc.startFrame];
			for(int k=0, j=ecc.startFrame; j < ecc.endFrame ; j ++) {
				height += encoderReaded[j];
				curve[k]=encoderReaded[j];
				k++;
			}
			
			//check height in a fast way first to discard curves soon
			//only process curves with height >= min_height
			height = height / 10; //cm -> mm
			if(height < (int) encoderCaptureOptionsWin.spin_encoder_capture_min_height.Value) {
				ecca.curvesDone ++;
				return;	
			}

			
			Log.Write(" uECGRC2 calling rdotnet ");

			NumericVector curveToR = rengine.CreateNumericVector(curve);
			rengine.SetSymbol("curveToR", curveToR);

			//cannot do smooth.spline with less than 4 values
			if(curveToR.Length <= 4)
				return;
			try {
				rengine.Evaluate("speedCut <- smooth.spline( 1:length(curveToR), curveToR, spar=0.7)");
			} catch {
				return;
			}

			Log.Write(" uECGRC3 ");
			//reduce curve by speed, the same way as graph.R
			rengine.Evaluate("b=extrema(speedCut$y)");

			rengine.Evaluate("speedCut$y=abs(speedCut$y)");
			
			if(ecc.up) { //concentric
				rengine.Evaluate("speedT1 <- min(which(speedCut$y == max(speedCut$y)))");
				rengine.Evaluate("speedT2 <- max(which(speedCut$y == max(speedCut$y)))");
			} else {
				rengine.Evaluate("speedT1 <- min(which(speedCut$y == min(speedCut$y)))");
				rengine.Evaluate("speedT2 <- max(which(speedCut$y == min(speedCut$y)))");
			}
			
			Log.Write(" uECGRC4 ");
			int speedT1 = rengine.GetSymbol("speedT1").AsInteger().First();
			int speedT2 = rengine.GetSymbol("speedT2").AsInteger().First();

			rengine.Evaluate("bcrossLen <- length(b$cross[,2])");
			int bcrossLen = rengine.GetSymbol("bcrossLen").AsInteger().First();

			rengine.Evaluate("bcross <- b$cross[,2]");
			IntegerVector bcross = rengine.GetSymbol("bcross").AsInteger();

			//left adjust
			//find the b$cross at left of max speed

			Log.Write(" uECGRC5 ");

			int x_ini = 0;	
			if(bcrossLen == 0)
				x_ini = 0;
			else if(bcrossLen == 1) {
				if(bcross[0] < speedT1)
					x_ini = bcross[0];
			} else {
				x_ini = bcross[0];	//not 1, we are in C# now
				for(int i=0; i < bcross.Length; i++) {
					if(bcross[i] < speedT1)
						x_ini = bcross[i];	//left adjust
				}
			}
			Log.Write(" uECGRC6 ");

			//rengine.Evaluate("curveToRcumsum = cumsum(curveToR)");

			//TODO: this has to be at right of x_ini
			//rengine.Evaluate("firstFrameAtTop <- min(which(curveToRcumsum == max (curveToRcumsum)))");
			//int x_end = rengine.GetSymbol("firstFrameAtTop").AsInteger().First();


			//right adjust
			//find the b$cross at right of max speed
	
			int x_end = curveToR.Length; //good to declare here
			if(bcrossLen == 0) {
				x_end = curveToR.Length;
			} else if(bcrossLen == 1) {
				if(bcross[0] > speedT2)
					x_end = bcross[0];
			} else {
				for(int i=bcross.Length -1; i >= 0; i--) {
					if(bcross[i] > speedT2)
						x_end = bcross[i];	//right adjust
				}
			}
			
			Log.Write(" uECGRC7 ");

			Log.WriteLine("reducedCurveBySpeed (start, end)");
			Log.WriteLine((ecc.startFrame + x_ini).ToString());
			Log.WriteLine((ecc.startFrame + x_end).ToString());

			//TODO: this is to get info about previous TODO bug
			if(ecc.startFrame + x_end <= ecc.startFrame + x_ini)
				for(int i=x_end; i < x_ini; i ++)
					Log.Write(curveToR[i] + ","); //TODO: provar aixo!!				

			//create a curveToR with only reduced curve
			NumericVector curveToRreduced = rengine.CreateNumericVector(new double[x_end - x_ini]);
			for(int k=0, i=x_ini; i < x_end; i ++)
				curveToRreduced[k++] = curveToR[i]; 				
			rengine.SetSymbol("curveToRreduced", curveToRreduced);

			//2) do speed and accel for curve once reducedCurveBySpeed

			//cannot do smooth.spline with less than 4 values
			if(curveToRreduced.Length <= 4)
				return;
			try {
				rengine.Evaluate("speed <- smooth.spline( 1:length(curveToRreduced), curveToRreduced, spar=0.7)");
			} catch {
				return;
			}

			//height (or range)	
			rengine.Evaluate("curveToRreduced.cumsum <- cumsum(curveToRreduced)");
			rengine.Evaluate("range <- abs(curveToRreduced.cumsum[length(curveToRreduced)]-curveToRreduced.cumsum[1])");
			
			//check height now in a more accurate way than before
			height = rengine.GetSymbol("range").AsNumeric().First();
			height = height / 10; //cm -> mm

			//only process curves with height >= min_height
			if(height < (int) encoderCaptureOptionsWin.spin_encoder_capture_min_height.Value) {
				ecca.curvesDone ++;
				return;	
			}


			//accel and propulsive stuff
			rengine.Evaluate("accel <- predict( speed, deriv=1 )");
			rengine.Evaluate("accel$y <- accel$y * 1000"); //input data is in mm, conversion to m

			//propulsive stuff
			int propulsiveEnd = curveToRreduced.Length;
			rengine.Evaluate("g <- 9.81");
			if(preferences.encoderPropulsive) {
				//check if propulsive phase ends
				Log.WriteLine("accel$y");
				//rengine.Evaluate("print(accel$y)");
				rengine.Evaluate("propulsiveStuffAtRight <- length(which(accel$y <= -g))"); 
				int propulsiveStuffAtRight = rengine.GetSymbol("propulsiveStuffAtRight").AsInteger().First();
				Log.WriteLine(string.Format("propulsiveStuffAtRight: {0}", propulsiveStuffAtRight));
				if(propulsiveStuffAtRight > 0) {
					rengine.Evaluate("propulsiveEnd <- min(which(accel$y <= -g))");
					propulsiveEnd = rengine.GetSymbol("propulsiveEnd").AsInteger().First();

					rengine.Evaluate("curveToRreduced <- curveToRreduced[1:propulsiveEnd]");
					rengine.Evaluate("speed$y <- speed$y[1:propulsiveEnd]");
					rengine.Evaluate("accel$y <- accel$y[1:propulsiveEnd]");
				}
			}
			//end of propulsive stuff


			NumericVector mass = rengine.CreateNumericVector(new double[] { massDisplacedEncoder });
			rengine.SetSymbol("mass", mass);
		

			//if isJump == "True":
			rengine.Evaluate("force <- mass*(accel$y+9.81)");
			//else:
			//rengine.Evaluate("force <- mass*accel$y')
			rengine.Evaluate("power <- force*speed$y");

			if(ecc.up) //concentric
				rengine.Evaluate("meanPower <- mean(power)");
			else
				rengine.Evaluate("meanPower <- mean(abs(power))");

			rengine.Evaluate("peakPower <- max(power)");

			//without the 'min', if there's more than one value it returns a list and this make crash later in
			//this code:  pp_ppt = peakPower / peakPowerT
			rengine.Evaluate("peakPowerT=min(which(power == peakPower))"); 

			//rengine.Evaluate("print(speed$y)");
			
			rengine.Evaluate("meanSpeed = mean(abs(speed$y))");
			double meanSpeed = rengine.GetSymbol("meanSpeed").AsNumeric().First();

			double maxSpeed = 0;
			if(ecc.up) {
				rengine.Evaluate("maxSpeed = max(speed$y)");
				maxSpeed = rengine.GetSymbol("maxSpeed").AsNumeric().First();
				//phase = "   up,"
			}
			else {
				rengine.Evaluate("maxSpeed = min(speed$y)");
				maxSpeed = rengine.GetSymbol("maxSpeed").AsNumeric().First();
				//phase = " down,"
			}
			
			double meanPower = rengine.GetSymbol("meanPower").AsNumeric().First();
			double peakPower = rengine.GetSymbol("peakPower").AsNumeric().First();
			double peakPowerT = rengine.GetSymbol("peakPowerT").AsNumeric().First();

			peakPowerT = peakPowerT / 1000; //ms -> s
			double pp_ppt = peakPower / peakPowerT;

			//plot
			if(plotCurvesBars) {
				string title = "";
				string mainVariable = encoderCaptureOptionsWin.GetMainVariable();
				double mainVariableHigher = encoderCaptureOptionsWin.GetMainVariableHigher(mainVariable);
				double mainVariableLower = encoderCaptureOptionsWin.GetMainVariableLower(mainVariable);
				captureCurvesBarsData.Add(new EncoderBarsData(meanSpeed, maxSpeed, meanPower, peakPower));

				plotCurvesGraphDoPlot(mainVariable, mainVariableHigher, mainVariableLower, captureCurvesBarsData, 
						true);	//capturing
			}


			Log.WriteLine(string.Format(
						"height: {0}\nmeanSpeed: {1}\n, maxSpeed: {2}\n, maxSpeedT: {3}\n" + 
						"meanPower: {4}\npeakPower: {5}\npeakPowerT: {6}", 
						height, meanSpeed, maxSpeed, speedT1, meanPower, peakPower, peakPowerT));
			
			encoderCaptureStringR += string.Format("\n{0},2,a,3,4,{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},7",
					ecca.curvesAccepted +1,
					ecc.startFrame, ecc.endFrame-ecc.startFrame,
					Util.ConvertToPoint(height*10), //cm	
					Util.ConvertToPoint(meanSpeed), Util.ConvertToPoint(maxSpeed), speedT1,
					Util.ConvertToPoint(meanPower), Util.ConvertToPoint(peakPower), 
					Util.ConvertToPoint(peakPowerT*1000), Util.ConvertToPoint(peakPower / peakPowerT) 
					);
		
			treeviewEncoderCaptureRemoveColumns();
			ecca.curvesAccepted = createTreeViewEncoderCapture(encoderCaptureStringR);
		}

		ecca.curvesDone ++;
	}

	//if we are capturing, play sounds
	void plotCurvesGraphDoPlot(string mainVariable, double mainVariableHigher, double mainVariableLower, 
			ArrayList data4Variables, bool capturing) 
	{
		//Log.WriteLine("at plotCurvesGraphDoPlot");
		UtilGtk.ErasePaint(encoder_capture_curves_bars_drawingarea, encoder_capture_curves_bars_pixmap);

		int graphWidth=encoder_capture_curves_bars_drawingarea.Allocation.Width;
		int graphHeight=encoder_capture_curves_bars_drawingarea.Allocation.Height;
	
		ArrayList data = new ArrayList (data4Variables.Count);
		foreach(EncoderBarsData ebd in data4Variables)
			data.Add(ebd.GetValue(mainVariable));


		//search max, min, avg
		double max = -100000;
		double min = 100000;
		double sum = 0;
		
		foreach(double d in data) {
			if(d > max)
				max = d;
			if(d < min)
				min = d;
			sum += d;
		}
		if(max <= 0)
			return;	
		

		int textWidth = 1;
		int textHeight = 1;

		int left_margin = 10;
		int right_margin = 0;
		int top_margin = 35;
		int bottom_margin = 20;
		//bars will be plotted here
		int graphHeightSafe = graphHeight - (top_margin + bottom_margin);
	
			

		//plot bars
		int sep = 20;	//between reps
		if (data.Count >= 10 && data.Count < 20) {
			sep = 10;
			layout_encoder_capture_curves_bars.FontDescription = Pango.FontDescription.FromString ("Courier 9");
		} else	if (data.Count >= 20) {
			sep = 2;
			layout_encoder_capture_curves_bars.FontDescription = Pango.FontDescription.FromString ("Courier 7");
			left_margin = 2;
		}
			
		layout_encoder_capture_curves_bars_text.FontDescription = Pango.FontDescription.FromString ("Courier 14");
		layout_encoder_capture_curves_bars_text.FontDescription.Weight = Pango.Weight.Bold;
		
		string eccon = findEccon(true);

		Rectangle rect;
		Gdk.GC my_pen;
		int dLeft = 0;
		int count = 0;
	
		//to show saved curves on DoPlot	
		TreeIter iter;
		
		//sum saved curves to do avg
		double sumSaved = 0; 
		double countSaved = 0;
		
		bool iterOk = encoderCaptureListStore.GetIterFirst(out iter);
		foreach(double dFor in data) {
			int dWidth = 0;
			int dHeight = 0;

			//if values are negative, invert it
			//this happens specially in the speeds in eccentric
			//we use dFor because we cannot change the iteration variable
			double d = dFor;
			if(d < 0)
				d *= -1;

			dHeight = Convert.ToInt32(graphHeightSafe * d / max * 1.0);
			int dBottom = graphHeight - bottom_margin;
			int dTop = dBottom - dHeight;


			if (data.Count == 1)	//do not fill all the screen with only one bar
				dWidth = Convert.ToInt32((graphWidth - left_margin - right_margin) / 2.0); 
			else
				dWidth = Convert.ToInt32((graphWidth - left_margin - right_margin) / data.Count * 1.0);

			dLeft = left_margin + dWidth * count;
		
			//dWidth = dWidth - sep to have separation between bars
			//but if eccon != "c" then have like this: ec ec ec
			if (eccon == "c") {
				dWidth = dWidth - sep;
			} else {
				double sep_ec_mult = 1.2;
				dWidth = Convert.ToInt32(dWidth - sep * sep_ec_mult);

				if(Util.IsEven(count +1)) //par
					dLeft = Convert.ToInt32(dLeft - sep * sep_ec_mult);
			}
			//just in case there are too much bars
			if(dWidth < 1)
				dWidth = 1;

			
			//select pen color for bars and sounds
			if(mainVariableHigher != -1 && d >= mainVariableHigher) {
				my_pen = pen_green_encoder_capture;
				//play sound if value is high, volumeOn == true, is last value, capturing
				if(preferences.volumeOn && count == data.Count -1 && capturing)
					Util.PlaySound(Constants.SoundTypes.GOOD, preferences.volumeOn);
			}
			else if(mainVariableLower != -1 && d <= mainVariableLower) {
				my_pen = pen_red_encoder_capture;
				//play sound if value is low, volumeOn == true, is last value, capturing
				if(preferences.volumeOn && count == data.Count -1 && capturing)
					Util.PlaySound(Constants.SoundTypes.BAD, preferences.volumeOn);
			}
			else
				my_pen = pen_azul_encoder_capture;

			//paint bar:	
			rect = new Rectangle(dLeft, dTop, dWidth, dHeight);
			encoder_capture_curves_bars_pixmap.DrawRectangle(my_pen, true, rect);
			encoder_capture_curves_bars_pixmap.DrawRectangle(pen_black_encoder_capture, false, rect);


			//write the result	
			if(mainVariable == Constants.MeanSpeed || mainVariable == Constants.MaxSpeed)
				layout_encoder_capture_curves_bars.SetMarkup(Util.TrimDecimals(d,2));
			else //powers
				layout_encoder_capture_curves_bars.SetMarkup(Util.TrimDecimals(d,0));
			
			textWidth = 1;
			textHeight = 1;
			layout_encoder_capture_curves_bars.GetPixelSize(out textWidth, out textHeight); 
			encoder_capture_curves_bars_pixmap.DrawLayout (pen_black_encoder_capture, 
					Convert.ToInt32( (dLeft + dWidth/2) - textWidth/2), dTop - 15, //x, y 
					layout_encoder_capture_curves_bars);
			//end of: write the result

			//paint diagonal line to distinguish eccentric-concentric	
			if (eccon == "ec" || eccon == "ecS") {
				bool isEven = Util.IsEven(count +1);
			
				if(isEven)
					encoder_capture_curves_bars_pixmap.DrawLine(pen_white_encoder_capture, 
							dLeft, dBottom, dLeft + dWidth, dTop);
				else
					encoder_capture_curves_bars_pixmap.DrawLine(pen_white_encoder_capture, 
							dLeft, dTop, dLeft + dWidth, dBottom);
			}
			
			//add text on the bottom
			if (eccon == "c" || Util.IsEven(count +1)) { //par
				int startX = Convert.ToInt32(dLeft + dWidth/2);
				string bottomText = (count +1).ToString();
				if (eccon != "c") {
					startX = dLeft;
					bottomText = ((count +1) / 2).ToString();
				}

				layout_encoder_capture_curves_bars.SetMarkup(bottomText);
				textWidth = 1;
				textHeight = 1;
				layout_encoder_capture_curves_bars.GetPixelSize(out textWidth, out textHeight); 
				int myX = Convert.ToInt32( startX - textWidth/2);
				int myY = Convert.ToInt32(dTop + dHeight + (bottom_margin /2) - textHeight/2);
				
				//plot a rectangle if this curve it is checked (in the near future checked will mean saved)
				if(iterOk)
					if(((EncoderCurve) encoderCaptureListStore.GetValue (iter, 0)).Record) {
						rect = new Rectangle(myX -2, myY -1, textWidth +4, graphHeight - (myY -1) -1);
						encoder_capture_curves_bars_pixmap.DrawRectangle(pen_selected_encoder_capture, false, rect);

						//average of saved values
						sumSaved += dFor;
						countSaved ++;
					}
				
				//write the text
				encoder_capture_curves_bars_pixmap.DrawLayout (pen_black_encoder_capture, 
						myX, myY,
						layout_encoder_capture_curves_bars);
			}



			count ++;
			iterOk = encoderCaptureListStore.IterNext (ref iter);
		}
		//end plot bars
	

		//plot title
		string units = "";
		int decimals;
		
		if(mainVariable == Constants.MeanSpeed || mainVariable == Constants.MaxSpeed) {
			units = "m/s";
			decimals = 2;
		}
		else { //powers
			units =  "W";
			decimals = 1;
		}
		
		//add avg and avg of saved values
		string title = mainVariable + " [X = " + 
			Util.TrimDecimals( (sum / data.Count), decimals) + 
			" " + units;

		if(countSaved > 0)
			title += "; X" + Catalog.GetString("saved") + " = " + 
				Util.TrimDecimals( (sumSaved / countSaved), decimals) + 
				" " + units;
			
		title += "]";

		layout_encoder_capture_curves_bars.SetMarkup(title);
		textWidth = 1;
		textHeight = 1;
		layout_encoder_capture_curves_bars.GetPixelSize(out textWidth, out textHeight); 
		encoder_capture_curves_bars_pixmap.DrawLayout (pen_black_encoder_capture, 
				Convert.ToInt32( (graphWidth/2) - textWidth/2), 0, //x, y 
				layout_encoder_capture_curves_bars);

		//end plot title	
		

		encoder_capture_curves_bars_drawingarea.QueueDraw(); 			// -- refresh
	}

	private void callPlotCurvesGraphDoPlot() {
		if(captureCurvesBarsData.Count > 0) {
			string mainVariable = encoderCaptureOptionsWin.GetMainVariable();
			double mainVariableHigher = encoderCaptureOptionsWin.GetMainVariableHigher(mainVariable);
			double mainVariableLower = encoderCaptureOptionsWin.GetMainVariableLower(mainVariable);
			plotCurvesGraphDoPlot(mainVariable, mainVariableHigher, mainVariableLower, captureCurvesBarsData,
					false);	//not capturing
		} else
			UtilGtk.ErasePaint(encoder_capture_curves_bars_drawingarea, encoder_capture_curves_bars_pixmap);
	}

	int encoder_capture_curves_allocationXOld;
	int encoder_capture_curves_allocationYOld;
	bool encoder_capture_curves_sizeChanged;
	public void on_encoder_capture_curves_bars_drawingarea_configure_event(object o, ConfigureEventArgs args)
	{
		Gdk.EventConfigure ev = args.Event;
		Gdk.Window window = ev.Window;

		Gdk.Rectangle allocation = encoder_capture_curves_bars_drawingarea.Allocation;
		
		if(encoder_capture_curves_bars_pixmap == null || encoder_capture_curves_sizeChanged || 
				allocation.Width != encoder_capture_curves_allocationXOld ||
				allocation.Height != encoder_capture_curves_allocationYOld) 
		{
			encoder_capture_curves_bars_pixmap = new Gdk.Pixmap (window, allocation.Width, allocation.Height, -1);

			callPlotCurvesGraphDoPlot();
			
			encoder_capture_curves_sizeChanged = false;
		}

		encoder_capture_curves_allocationXOld = allocation.Width;
		encoder_capture_curves_allocationYOld = allocation.Height;
	}

	public void on_encoder_capture_curves_bars_drawingarea_expose_event(object o, ExposeEventArgs args)
	{
		/* in some mono installations, configure_event is not called, but expose_event yes. 
		 * Do here the initialization
		 */
		//Log.WriteLine("EXPOSE");
		
		Gdk.Rectangle allocation = encoder_capture_curves_bars_drawingarea.Allocation;
		if(encoder_capture_curves_bars_pixmap == null || encoder_capture_curves_sizeChanged || 
				allocation.Width != encoder_capture_curves_allocationXOld ||
				allocation.Height != encoder_capture_curves_allocationYOld) 
		{
			encoder_capture_curves_bars_pixmap = new Gdk.Pixmap (
					encoder_capture_curves_bars_drawingarea.GdkWindow, allocation.Width, allocation.Height, -1);
			
			callPlotCurvesGraphDoPlot();
			
			encoder_capture_curves_sizeChanged = false;
		}

		Gdk.Rectangle area = args.Event.Area;

		//sometimes this is called when pait is finished
		//don't let this erase win
		if(encoder_capture_curves_bars_pixmap != null) {
			args.Event.Window.DrawDrawable(encoder_capture_curves_bars_drawingarea.Style.WhiteGC, 
					encoder_capture_curves_bars_pixmap,
				area.X, area.Y,
				area.X, area.Y,
				area.Width, area.Height);
		}
		
		encoder_capture_curves_allocationXOld = allocation.Width;
		encoder_capture_curves_allocationYOld = allocation.Height;
	}

	
	int encoder_capture_signal_allocationXOld;
	bool encoder_capture_signal_sizeChanged;
	public void on_encoder_capture_signal_drawingarea_configure_event(object o, ConfigureEventArgs args)
	{
		Gdk.EventConfigure ev = args.Event;
		Gdk.Window window = ev.Window;

		Gdk.Rectangle allocation = encoder_capture_signal_drawingarea.Allocation;
		
		if(encoder_capture_signal_pixmap == null || encoder_capture_signal_sizeChanged || 
				allocation.Width != encoder_capture_signal_allocationXOld) {
			encoder_capture_signal_pixmap = new Gdk.Pixmap (window, allocation.Width, allocation.Height, -1);
		
			UtilGtk.ErasePaint(encoder_capture_signal_drawingarea, encoder_capture_signal_pixmap);
			
			encoder_capture_signal_sizeChanged = false;
		}

		encoder_capture_signal_allocationXOld = allocation.Width;
	}
	
	public void on_encoder_capture_signal_drawingarea_expose_event(object o, ExposeEventArgs args)
	{
		/* in some mono installations, configure_event is not called, but expose_event yes. 
		 * Do here the initialization
		 */
		//Log.WriteLine("EXPOSE");
		
		Gdk.Rectangle allocation = encoder_capture_signal_drawingarea.Allocation;
		if(encoder_capture_signal_pixmap == null || encoder_capture_signal_sizeChanged || 
				allocation.Width != encoder_capture_signal_allocationXOld) {
			encoder_capture_signal_pixmap = new Gdk.Pixmap (encoder_capture_signal_drawingarea.GdkWindow, 
					allocation.Width, allocation.Height, -1);
			UtilGtk.ErasePaint(encoder_capture_signal_drawingarea, encoder_capture_signal_pixmap);

			encoder_capture_signal_sizeChanged = false;
		}

		Gdk.Rectangle area = args.Event.Area;

		//sometimes this is called when pait is finished
		//don't let this erase win
		if(encoder_capture_signal_pixmap != null) {
			args.Event.Window.DrawDrawable(encoder_capture_signal_drawingarea.Style.WhiteGC, encoder_capture_signal_pixmap,
				area.X, area.Y,
				area.X, area.Y,
				area.Width, area.Height);
		}
		
		encoder_capture_signal_allocationXOld = allocation.Width;
	}


	/*
	 * end of update encoder capture graph stuff
	 */
	
	
	/* thread stuff */

	private void encoderThreadStart(encoderActions action) {
		encoderProcessCancel = false;
					
		if(action == encoderActions.CAPTURE || action == encoderActions.CAPTURE_IM) {
			//encoder_pulsebar_capture.Text = Catalog.GetString("Please, wait.");
			Log.WriteLine("CCCCCCCCCCCCCCC");
			if( runEncoderCaptureCsharpCheckPort(chronopicWin.GetEncoderPort()) ) {
				if(action == encoderActions.CAPTURE) {
					if(RInitialized == Constants.Status.UNSTARTED)
						rengine = UtilEncoder.RunEncoderCaptureCsharpInitializeR(rengine, out RInitialized);

					/* 
					 * if error means a problem with RDotNet, not necessarily a problem with R
					 * we can contnue but without realtime data
					 *
					if(RInitialized == Constants.Status.ERROR) {
						new DialogMessage(Constants.MessageTypes.WARNING,
								Catalog.GetString("Sorry. Error doing graph.") +
								"\n" + Catalog.GetString("Maybe R or EMD are not installed.") +
								"\n\nhttp://www.r-project.org/");
						return;
					}
					*/
				}

				prepareEncoderGraphs();
				eccaCreated = false;

				if(action == encoderActions.CAPTURE) {
					encoderStartVideoRecord();

					//remove treeview columns
					treeviewEncoderCaptureRemoveColumns();
					encoderCaptureStringR = 
						",series,exercise,mass,start,width,height," + 
						"meanSpeed,maxSpeed,maxSpeedT,meanPower,peakPower,peakPowerT,pp_ppt,NA,NA,NA";

					capturingCsharp = encoderCaptureProcess.CAPTURING;

					massDisplacedEncoder = UtilEncoder.GetMassByEncoderConfiguration( encoderConfigurationCurrent, 
							findMass(Constants.MassType.BODY), findMass(Constants.MassType.EXTRA),
							getExercisePercentBodyWeightFromCombo() );
				}

				if(action == encoderActions.CAPTURE) {
					captureCurvesBarsData = new ArrayList();
					updatingEncoderCaptureGraphRCalc = false;
					encoderThread = new Thread(new ThreadStart(encoderDoCaptureCsharp));
					GLib.Idle.Add (new GLib.IdleHandler (pulseGTKEncoderCaptureAndCurves));
				}
				else { //action == encoderActions.CAPTURE_IM)
					encoderThread = new Thread(new ThreadStart(encoderDoCaptureCsharpIM));
					GLib.Idle.Add (new GLib.IdleHandler (pulseGTKEncoderCaptureIM));
				}
				
				hbox_encoder_capture_wait.Visible = false;
				hbox_encoder_capture_doing.Visible = true;

				Log.WriteLine("DDDDDDDDDDDDDDD");
				encoderButtonsSensitive(encoderSensEnum.PROCESSINGCAPTURE);
				encoderThread.Start(); 
			} else {
				new DialogMessage(Constants.MessageTypes.WARNING, 
					Catalog.GetString("Chronopic port is not configured."));
			
				createChronopicWindow(true);
				return;
			}
		} else if(
				action == encoderActions.CURVES || 
				action == encoderActions.LOAD ||
				action == encoderActions.CURVES_AC)	//this does not run a pulseGTK
		{
			//image is inside (is smaller than) viewport
			image_encoder_width = UtilGtk.WidgetWidth(viewport_image_encoder_capture)-5; 
			image_encoder_height = UtilGtk.WidgetHeight(viewport_image_encoder_capture)-5;
				
			prepareEncoderGraphs();

			if(action == encoderActions.CURVES_AC) {
				//this does not run a pulseGTK
				encoderDoCurvesGraphR();
				encoderButtonsSensitive(encoderSensEnum.PROCESSINGR);
			} else {
				treeview_encoder_capture_curves.Sensitive = false;
				encoderThread = new Thread(new ThreadStart(encoderDoCurvesGraphR));
				if(action == encoderActions.CURVES)
					GLib.Idle.Add (new GLib.IdleHandler (pulseGTKEncoderCurves));
				else // action == encoderActions.LOAD
					GLib.Idle.Add (new GLib.IdleHandler (pulseGTKEncoderLoad));
				encoderButtonsSensitive(encoderSensEnum.PROCESSINGR);
				encoderThread.Start(); 
			}
		} else { //encoderActions.ANALYZE
			//the -3 is because image is inside (is smaller than) viewport
			image_encoder_width = UtilGtk.WidgetWidth(viewport_image_encoder_analyze)-5; 
			image_encoder_height = UtilGtk.WidgetHeight(viewport_image_encoder_analyze)-5;

			encoder_pulsebar_analyze.Text = Catalog.GetString("Please, wait.");
		
			encoderThread = new Thread(new ThreadStart(encoderDoAnalyze));
			GLib.Idle.Add (new GLib.IdleHandler (pulseGTKEncoderAnalyze));

			encoderButtonsSensitive(encoderSensEnum.PROCESSINGR);
			treeview_encoder_analyze_curves.Sensitive = false;
			button_encoder_analyze_image_save.Sensitive = false;
			button_encoder_analyze_table_save.Sensitive = false;
			button_encoder_analyze_1RM_save.Sensitive = false;

			encoderThread.Start(); 
		}
	}

	void prepareEncoderGraphs() {
		UtilGtk.ErasePaint(encoder_capture_signal_drawingarea, encoder_capture_signal_pixmap);
		UtilGtk.ErasePaint(encoder_capture_curves_bars_drawingarea, encoder_capture_curves_bars_pixmap);

		layout_encoder_capture_signal = new Pango.Layout (encoder_capture_signal_drawingarea.PangoContext);
		layout_encoder_capture_signal.FontDescription = Pango.FontDescription.FromString ("Courier 10");

		layout_encoder_capture_curves_bars = new Pango.Layout (encoder_capture_curves_bars_drawingarea.PangoContext);
		layout_encoder_capture_curves_bars.FontDescription = Pango.FontDescription.FromString ("Courier 10");
		
		layout_encoder_capture_curves_bars_text = new Pango.Layout (encoder_capture_curves_bars_drawingarea.PangoContext);
		layout_encoder_capture_curves_bars_text.FontDescription = Pango.FontDescription.FromString ("Courier 10");

		pen_black_encoder_capture = new Gdk.GC(encoder_capture_signal_drawingarea.GdkWindow);
		pen_azul_encoder_capture = new Gdk.GC(encoder_capture_signal_drawingarea.GdkWindow);
		pen_green_encoder_capture = new Gdk.GC(encoder_capture_signal_drawingarea.GdkWindow);
		pen_red_encoder_capture = new Gdk.GC(encoder_capture_signal_drawingarea.GdkWindow);
		pen_white_encoder_capture = new Gdk.GC(encoder_capture_signal_drawingarea.GdkWindow);
		pen_selected_encoder_capture = new Gdk.GC(encoder_capture_signal_drawingarea.GdkWindow);

		Gdk.Colormap colormap = Gdk.Colormap.System;
		colormap.AllocColor (ref UtilGtk.BLACK,true,true);
		colormap.AllocColor (ref UtilGtk.BLUE_PLOTS,true,true);
		colormap.AllocColor (ref UtilGtk.GREEN_PLOTS,true,true);
		colormap.AllocColor (ref UtilGtk.RED_PLOTS,true,true);
		colormap.AllocColor (ref UtilGtk.WHITE,true,true);
		colormap.AllocColor (ref UtilGtk.SELECTED,true,true);

		pen_black_encoder_capture.Foreground = UtilGtk.BLACK;
		pen_azul_encoder_capture.Foreground = UtilGtk.BLUE_PLOTS;
		pen_green_encoder_capture.Foreground = UtilGtk.GREEN_PLOTS;
		pen_red_encoder_capture.Foreground = UtilGtk.RED_PLOTS;
		pen_white_encoder_capture.Foreground = UtilGtk.WHITE;
		pen_selected_encoder_capture.Foreground = UtilGtk.SELECTED;

		pen_selected_encoder_capture.SetLineAttributes (2, Gdk.LineStyle.Solid, Gdk.CapStyle.NotLast, Gdk.JoinStyle.Miter);
	}

	private bool pulseGTKEncoderCaptureAndCurves ()
	{
		if(! encoderThread.IsAlive || encoderProcessCancel) {
			finishPulsebar(encoderActions.CURVES);
			Log.Write("dying");
			return false;
		}
		if(capturingCsharp == encoderCaptureProcess.CAPTURING) {
			updatePulsebar(encoderActions.CAPTURE); //activity on pulsebar
		
			//calculations with RDotNet are not available yet on inertial	
			if(encoderConfigurationCurrent.has_inertia) {
				UtilGtk.ErasePaint(encoder_capture_curves_bars_drawingarea, encoder_capture_curves_bars_pixmap);
				layout_encoder_capture_curves_bars.SetMarkup("Realtime inertial calculations\nnot available in this version");
				int textWidth = 1;
				int textHeight = 1;
				int graphWidth=encoder_capture_curves_bars_drawingarea.Allocation.Width;
				layout_encoder_capture_curves_bars.GetPixelSize(out textWidth, out textHeight); 
				encoder_capture_curves_bars_pixmap.DrawLayout (pen_black_encoder_capture, 
						Convert.ToInt32( (graphWidth/2) - textWidth/2), 0, //x, y 
						layout_encoder_capture_curves_bars);

				updateEncoderCaptureGraph(true, false, false); //graphSignal, not calcCurves, not plotCurvesBars
			} else
				updateEncoderCaptureGraph(true, true, true); //graphSignal, calcCurves, plotCurvesBars
			
			Log.Write(" Cap:" + encoderThread.ThreadState.ToString());
		} else if(capturingCsharp == encoderCaptureProcess.STOPPING) {
			//stop video		
			encoderStopVideoRecord();
			capturingCsharp = encoderCaptureProcess.STOPPED;
		} else {	//STOPPED	
			//do curves, capturingCsharp has ended
			updatePulsebar(encoderActions.CURVES); //activity on pulsebar
			Log.Write(" Cur:" + encoderThread.ThreadState.ToString());
		}
			
		Thread.Sleep (25);
		return true;
	}
	
	private bool pulseGTKEncoderCaptureIM ()
	{
		if(! encoderThread.IsAlive || encoderProcessCancel) {
			finishPulsebar(encoderActions.CAPTURE_IM);
			Log.Write("dying");
			return false;
		}
		updatePulsebar(encoderActions.CAPTURE_IM); //activity on pulsebar
		updateEncoderCaptureGraph(true, false, false); //graphSignal, not calcCurves, not plotCurvesBars

		Thread.Sleep (25);
		Log.Write(" CapIM:" + encoderThread.ThreadState.ToString());
		return true;
	}
	
	
	private bool pulseGTKEncoderCurves ()
	{
		if(! encoderThread.IsAlive || encoderProcessCancel) {
			if(encoderProcessCancel){
				UtilEncoder.CancelRScript = true;
			}

			finishPulsebar(encoderActions.CURVES);
			Log.Write("dying");
			return false;
		}
		updatePulsebar(encoderActions.CURVES); //activity on pulsebar
		Thread.Sleep (50);
		Log.Write(" Cur:" + encoderThread.ThreadState.ToString());
		return true;
	}
	
	private bool pulseGTKEncoderLoad ()
	{
		if(! encoderThread.IsAlive || encoderProcessCancel) {
			if(encoderProcessCancel){
				UtilEncoder.CancelRScript = true;
			}

			finishPulsebar(encoderActions.LOAD);
			Log.Write("dying");
			return false;
		}
		updatePulsebar(encoderActions.LOAD); //activity on pulsebar
		Thread.Sleep (50);
		Log.Write(" L:" + encoderThread.ThreadState.ToString());
		return true;
	}
	
	private bool pulseGTKEncoderAnalyze ()
	{
		if(! encoderThread.IsAlive || encoderProcessCancel) {
			if(encoderProcessCancel){
				UtilEncoder.CancelRScript = true;
			}

			finishPulsebar(encoderActions.ANALYZE);
			Log.Write("dying");
			return false;
		}
		updatePulsebar(encoderActions.ANALYZE); //activity on pulsebar
		Thread.Sleep (50);
		Log.Write(" A:" + encoderThread.ThreadState.ToString());
		return true;
	}
	
	private void updatePulsebar (encoderActions action) {
		if(action == encoderActions.CAPTURE || action == encoderActions.CAPTURE_IM) {
			int selectedTime = (int) encoderCaptureOptionsWin.spin_encoder_capture_time.Value;
			if(action == encoderActions.CAPTURE_IM)
				selectedTime = encoder_configuration_win.Spin_im_duration;

			encoder_pulsebar_capture.Fraction = Util.DivideSafeFraction(
					(selectedTime - encoderCaptureCountdown), selectedTime);
			encoder_pulsebar_capture.Text = encoderCaptureCountdown + " s";
			return;
		}

		try {
			string contents = Catalog.GetString("Please, wait.");
			double fraction = -1;
			if(Util.FileExists(UtilEncoder.GetEncoderStatusTempFileName())) {
				contents = Util.ReadFile(UtilEncoder.GetEncoderStatusTempFileName(), true);
				//contents is:
				//(1/5) Starting R
				//(5/5) R tasks done

				//-48: ascii 0 char
				if(System.Char.IsDigit(contents[1]) && System.Char.IsDigit(contents[3]))
					fraction = Util.DivideSafeFraction(
							Convert.ToInt32(contents[1]-48), Convert.ToInt32(contents[3]-48) );
			}

			if(action == encoderActions.CURVES || action == encoderActions.LOAD) {
				if(fraction == -1) {
					encoder_pulsebar_capture.Pulse();
					encoder_pulsebar_capture.Text = contents;
				} else {
					encoder_pulsebar_capture.Fraction = fraction;
					encoder_pulsebar_capture.Text = contents.Substring(6);
				}
			} else {
				if(fraction == -1) {
					encoder_pulsebar_analyze.Pulse();
					encoder_pulsebar_analyze.Text = contents;
				} else {
					encoder_pulsebar_analyze.Fraction = fraction;
					encoder_pulsebar_analyze.Text = contents.Substring(6);
				}

			}
		} catch {
			//UtilEncoder.GetEncoderStatusTempFileName() is deleted at the end of the process
			//this can make crash updatePulsebar sometimes
		}
	}
	
	private void finishPulsebar(encoderActions action) {
		if(
				action == encoderActions.CAPTURE || 
				action == encoderActions.CAPTURE_IM || 
				action == encoderActions.CURVES || 
				action == encoderActions.LOAD )
		{
			Log.WriteLine("ffffffinishPulsebarrrrr");
		
			//save video will be later at encoderSaveSignalOrCurve, because there encoderSignalUniqueID will be known
			
			if(encoderProcessCancel || encoderProcessProblems) {
				//tis notebook has capture (signal plotting), and curves (shows R graph)	
				if(notebook_encoder_capture.CurrentPage == 0 )
					notebook_encoder_capture.NextPage();
				encoder_pulsebar_capture.Fraction = 1;
			
				if(encoderProcessProblems) {
					new DialogMessage(Constants.MessageTypes.WARNING, 
							Catalog.GetString("Sorry. Error doing graph.") + 
							"\n" + Catalog.GetString("Maybe R or EMD are not installed.") + 
							"\n\nhttp://www.r-project.org/");
					encoderProcessProblems = false;
				} else {
					if(action == encoderActions.CAPTURE_IM)
						encoder_configuration_win.Button_encoder_capture_inertial_do_ended(0,"Cancelled");
					else
						encoder_pulsebar_capture.Text = Catalog.GetString("Cancelled");
				}
			}
			else if( (action == encoderActions.CAPTURE || action == encoderActions.CAPTURE_IM) 
					&& encoderProcessFinish ) {
				encoder_pulsebar_capture.Text = Catalog.GetString("Finished");
			} 
			else if(action == encoderActions.CURVES || action == encoderActions.LOAD) {
				//tis notebook has capture (signal plotting), and curves (shows R graph)	
				if(notebook_encoder_capture.CurrentPage == 0)
					notebook_encoder_capture.NextPage();

				string contents = Util.ReadFile(UtilEncoder.GetEncoderCurvesTempFileName(), false);
				
				Pixbuf pixbuf = new Pixbuf (UtilEncoder.GetEncoderGraphTempFileName()); //from a file
				image_encoder_capture.Pixbuf = pixbuf;
				encoderUpdateTreeViewCapture(contents); //this updates encoderCaptureCurves
				image_encoder_capture.Sensitive = true;

				//plot curves bars graph
				string mainVariable = encoderCaptureOptionsWin.GetMainVariable();
				double mainVariableHigher = encoderCaptureOptionsWin.GetMainVariableHigher(mainVariable);
				double mainVariableLower = encoderCaptureOptionsWin.GetMainVariableLower(mainVariable);
			
				captureCurvesBarsData = new ArrayList();
				foreach (EncoderCurve curve in encoderCaptureCurves) {
					captureCurvesBarsData.Add(new EncoderBarsData(
								Convert.ToDouble(curve.MeanSpeed), 
								Convert.ToDouble(curve.MaxSpeed), 
								Convert.ToDouble(curve.MeanPower), 
								Convert.ToDouble(curve.PeakPower)
								));
				}


				plotCurvesGraphDoPlot(mainVariable, mainVariableHigher, mainVariableLower, captureCurvesBarsData,
						false);	//not capturing
		
				//autosave signal (but not in load)
				if(action == encoderActions.CURVES) 
				{
					bool needToAutoSaveCurve = false;
					if(
							encoderSignalUniqueID == "-1" &&	//if we just captured
							(preferences.encoderAutoSaveCurve == Constants.EncoderAutoSaveCurve.ALL ||
							preferences.encoderAutoSaveCurve == Constants.EncoderAutoSaveCurve.BESTMEANPOWER) )
						needToAutoSaveCurve = true;

					encoder_pulsebar_capture.Text = encoderSaveSignalOrCurve("signal", 0); //this updates encoderSignalUniqueID

					if(needToAutoSaveCurve)
						encoderCaptureSaveCurvesAllNoneBest(preferences.encoderAutoSaveCurve);

				} else
					encoder_pulsebar_capture.Text = "";
		

				/*
				 * if we captured, but encoderSignalUniqueID has not been changed on encoderSaveSignalOrCurve
				 * because there are no curves (problems detecting, or minimal height so big
				 * then do not continue
				 * because with a value of -1 there will be problems in 
				 * SqliteEncoder.Select(false, Convert.ToInt32(encoderSignalUniqueID), ...)
				 */
				Log.Write(" encoderSignalUniqueID:" + encoderSignalUniqueID);
				if(encoderSignalUniqueID != "-1")
				{

					string eccon = findEccon(true);

					/*
					 * (1) if found curves of this signal
					 * (2) and this curves are with different eccon
					 * (3) delete the curves (encoder table)
					 * (4) and also delete from (encoderSignalCurves table)
					 * (5) update analyze labels and combos
					 */
					bool deletedUserCurves = false;
					EncoderSQL currentSignalSQL = (EncoderSQL) SqliteEncoder.Select(
							false, Convert.ToInt32(encoderSignalUniqueID), 0, 0, 
							"", EncoderSQL.Eccons.ALL, 
							false, true)[0];


					ArrayList data = SqliteEncoder.Select(
							false, -1, currentPerson.UniqueID, currentSession.UniqueID, 
							"curve", EncoderSQL.Eccons.ALL,  
							false, true);
					foreach(EncoderSQL eSQL in data) {
						if(
								currentSignalSQL.GetDate(false) == eSQL.GetDate(false) && 		// (1)
								findEccon(true) != eSQL.eccon) {					// (2)
									Sqlite.Delete(false, Constants.EncoderTable, Convert.ToInt32(eSQL.uniqueID));	// (3)
									SqliteEncoder.DeleteSignalCurveWithCurveID(false, Convert.ToInt32(eSQL.uniqueID)); // (4)
									deletedUserCurves = true;
								}
					}
					if(deletedUserCurves)
						updateUserCurvesLabelsAndCombo();		// (5)


					findAndMarkSavedCurves();
				}
			}

			if(action == encoderActions.CAPTURE_IM && ! encoderProcessCancel && ! encoderProcessProblems) 
			{
				string imResultText = Util.ChangeDecimalSeparator(
						Util.ReadFile(UtilEncoder.GetEncoderSpecialDataTempFileName(), true) );
				Log.WriteLine("imResultText = |" + imResultText + "|");

				if(imResultText == "NA" || imResultText == "")
					encoder_configuration_win.Button_encoder_capture_inertial_do_ended (0, "Error capturing. Maybe need more oscillations.");
				else {
					//script calculates Kg*m^2 -> GUI needs Kg*cm^2
					encoder_configuration_win.Button_encoder_capture_inertial_do_ended (
							Convert.ToDouble(imResultText) * 10000.0, "");
				}

				encoderButtonsSensitive(encoderSensEnum.DONENOSIGNAL);
			} else {
				encoderButtonsSensitive(encoderSensEnumStored);
			}

			encoder_pulsebar_capture.Fraction = 1;
			//analyze_image_save only has not to be sensitive now because capture graph will be saved
			image_encoder_analyze.Sensitive = false;
			treeview_encoder_analyze_curves.Sensitive = false;
			button_encoder_analyze_image_save.Sensitive = false;
			button_encoder_analyze_table_save.Sensitive = false;
			button_encoder_analyze_1RM_save.Sensitive = false;
		
			hbox_encoder_capture_wait.Visible = true;
			hbox_encoder_capture_doing.Visible = false;

		} else { //ANALYZE
			if(encoderProcessCancel) {
				encoder_pulsebar_analyze.Text = Catalog.GetString("Cancelled");
			} else {
				//TODO pensar en si s'ha de fer 1er amb mida petita i despres amb gran (en el zoom),
				//o si es una sola i fa alguna edicio
				Pixbuf pixbuf = new Pixbuf (UtilEncoder.GetEncoderGraphTempFileName()); //from a file
				image_encoder_analyze.Pixbuf = pixbuf;
				encoder_pulsebar_analyze.Text = "";

				string contents = Util.ReadFile(UtilEncoder.GetEncoderAnalyzeTableTempFileName(), false);
				if (contents != null && contents != "") {
					if(radiobutton_encoder_analyze_neuromuscular_profile.Active) {
						treeviewEncoderAnalyzeRemoveColumns(false);	//neuromuscular
						createTreeViewEncoderAnalyzeNeuromuscular(contents);
					} else {
						treeviewEncoderAnalyzeRemoveColumns(true);	//curves
						createTreeViewEncoderAnalyze(contents);
					}
				}
			}
		
			button_encoder_analyze.Visible = true;
			hbox_encoder_analyze_progress.Visible = false;

			encoder_pulsebar_analyze.Fraction = 1;
			encoderButtonsSensitive(encoderSensEnumStored);
			image_encoder_analyze.Sensitive = true;
			treeview_encoder_analyze_curves.Sensitive = true;
			
			button_encoder_analyze_image_save.Sensitive = true;
			button_encoder_analyze_table_save.Sensitive = true;
			
			string crossName = Util.FindOnArray(':',1,0,UtilGtk.ComboGetActive(combo_encoder_analyze_cross),
						encoderAnalyzeCrossTranslation);
			button_encoder_analyze_1RM_save.Sensitive = 
				(crossName == "1RM Bench Press" || crossName == "1RM Any exercise");
		}

		treeview_encoder_capture_curves.Sensitive = true;
		Util.FileDelete(UtilEncoder.GetEncoderStatusTempFileName());
	}

	private void findAndMarkSavedCurves() {
		//find the saved curves
		ArrayList linkedCurves = SqliteEncoder.SelectSignalCurve(false, 
				Convert.ToInt32(encoderSignalUniqueID), //signal
				-1, -1, -1);				//curve, msStart,msEnd
		//Log.WriteLine("SAVED CURVES FOUND");
		//foreach(EncoderSignalCurve esc in linkedCurves)
		//	Log.WriteLine(esc.ToString());

		int curveCount = 0;
		double curveStart = 0;
		double curveEnd = 0;
		foreach (EncoderCurve curve in encoderCaptureCurves) 
		{
			if(findEccon(true) == "c") {
				curveStart = Convert.ToDouble(curve.Start);
				curveEnd = Convert.ToDouble(curve.Start) + Convert.ToDouble(curve.Duration);
			} else { //eccon == "ecS"
				if(Util.IsEven(curveCount)) {
					curveStart = Convert.ToDouble(curve.Start);
					curveCount ++;
					continue;
				} else
					curveEnd = Convert.ToDouble(curve.Start) + Convert.ToDouble(curve.Duration);
			}

			foreach(EncoderSignalCurve esc in linkedCurves) {
				if(curveStart <= esc.msCentral && curveEnd >= esc.msCentral)
				{
					Log.WriteLine(curve.Start + " is saved");
					encoderCaptureSelectBySavedCurves(esc.msCentral, true);
					break;
				}
			}
			curveCount ++;
		}
	}

	
	/* end of thread stuff */
	
	/* video stuff */
	private void encoderStartVideoRecord() {
		Log.WriteLine("Starting video");
		checkbutton_video_encoder.Sensitive = false;
		if(preferences.videoOn) {
			capturer.ClickRec();
			label_video_feedback_encoder.Text = "Rec.";
		}
		button_video_play_this_test_encoder.Sensitive = false; 
	}

	private void encoderStopVideoRecord() {
		Log.WriteLine("Stopping video");
		checkbutton_video_encoder.Sensitive = true;
		if(preferences.videoOn) {
			label_video_feedback_encoder.Text = "";
			capturer.ClickStop();
			videoCapturePrepare(false); //if error, show message
		}
	}


	void on_video_play_this_test_encoder_clicked (object o, EventArgs args) {
		if(! playVideo(Util.GetVideoFileName(currentSession.UniqueID, 
					Constants.TestTypes.ENCODER, Convert.ToInt32(encoderSignalUniqueID))))
			new DialogMessage(Constants.MessageTypes.WARNING, 
					Catalog.GetString("Sorry, file not found"));
	}
	/* end of video stuff */

}	
	

public class EncoderCaptureOptionsWindow {

	[Widget] Gtk.Window encoder_capture_options;
	static EncoderCaptureOptionsWindow EncoderCaptureOptionsWindowBox;
	
	[Widget] public Gtk.RadioButton radiobutton_encoder_capture_safe;
	[Widget] public Gtk.RadioButton radiobutton_encoder_capture_external;
	[Widget] public Gtk.SpinButton spin_encoder_capture_time;
	[Widget] public Gtk.SpinButton spin_encoder_capture_inactivity_end_time;
	[Widget] public Gtk.SpinButton spin_encoder_capture_min_height;
	[Widget] public Gtk.SpinButton spin_encoder_capture_curves_height_range;
	[Widget] Gtk.Box hbox_combo_main_variable;
	[Widget] Gtk.ComboBox combo_main_variable;
	[Widget] public Gtk.CheckButton check_show_start_and_duration;
	[Widget] Gtk.Button button_close;
	
	RepetitiveConditionsWindow repetitiveConditionsWin;
	bool volumeOn;
	
	public Gtk.Button FakeButtonClose;
		
	EncoderCaptureOptionsWindow () { 
		Glade.XML gladeXML;
		gladeXML = Glade.XML.FromAssembly (Util.GetGladePath() + "chronojump.glade", "encoder_capture_options", "chronojump");
		gladeXML.Autoconnect(this);
	
		//don't show until View is called
		encoder_capture_options.Hide ();

		createCombo();

		//put an icon to window
		UtilGtk.IconWindow(encoder_capture_options);
		
		FakeButtonClose = new Gtk.Button();
	}

	
	static public EncoderCaptureOptionsWindow Create (RepetitiveConditionsWindow repetitiveConditionsWin) {
		if (EncoderCaptureOptionsWindowBox == null)
			EncoderCaptureOptionsWindowBox = new EncoderCaptureOptionsWindow ();
		
		EncoderCaptureOptionsWindowBox.repetitiveConditionsWin = repetitiveConditionsWin;
		
		return EncoderCaptureOptionsWindowBox;
	}

	public void View (RepetitiveConditionsWindow repetitiveConditionsWin, bool volumeOn)
	{
		if (EncoderCaptureOptionsWindowBox == null) 
			EncoderCaptureOptionsWindowBox = new EncoderCaptureOptionsWindow ();
		
		EncoderCaptureOptionsWindowBox.repetitiveConditionsWin = repetitiveConditionsWin;
		EncoderCaptureOptionsWindowBox.volumeOn = volumeOn;

		//show window
		EncoderCaptureOptionsWindowBox.encoder_capture_options.Show ();
	}

	private void createCombo() {
		combo_main_variable = ComboBox.NewText ();
		string [] values = { Constants.MeanSpeed, Constants.MaxSpeed, Constants.MeanPower, Constants.PeakPower };
		UtilGtk.ComboUpdate(combo_main_variable, values, "");
		combo_main_variable.Active = UtilGtk.ComboMakeActive(combo_main_variable, "Mean power");
		
		hbox_combo_main_variable.PackStart(combo_main_variable, false, false, 0);
		hbox_combo_main_variable.ShowAll();
		combo_main_variable.Sensitive = true;
	}

	public string GetMainVariable() {
		return UtilGtk.ComboGetActive(combo_main_variable);
	}
	
	public double GetMainVariableHigher(string mainVariable) {
		//if user has not clicked at bells, repetitiveConditionsWin has not ben sent to encoderCaptureOptionsWin
		if(repetitiveConditionsWin != null) {
			if(mainVariable == Constants.MeanSpeed && repetitiveConditionsWin.EncoderMeanSpeedHigher)
				return repetitiveConditionsWin.EncoderMeanSpeedHigherValue;
			else if(mainVariable == Constants.MaxSpeed && repetitiveConditionsWin.EncoderMaxSpeedHigher)
				return repetitiveConditionsWin.EncoderMaxSpeedHigherValue;
			else if(mainVariable == Constants.MeanPower && repetitiveConditionsWin.EncoderPowerHigher)
				return repetitiveConditionsWin.EncoderPowerHigherValue;
			else if(mainVariable == Constants.PeakPower && repetitiveConditionsWin.EncoderPeakPowerHigher)
				return repetitiveConditionsWin.EncoderPeakPowerHigherValue;
		}
			
		return -1;
	}

	public double GetMainVariableLower(string mainVariable) {
		//if user has not clicked at bells, repetitiveConditionsWin has not ben sent to encoderCaptureOptionsWin
		if(repetitiveConditionsWin != null) {
			if(mainVariable == Constants.MeanSpeed && repetitiveConditionsWin.EncoderMeanSpeedLower)
				return repetitiveConditionsWin.EncoderMeanSpeedLowerValue;
			else if(mainVariable == Constants.MaxSpeed && repetitiveConditionsWin.EncoderMaxSpeedLower)
				return repetitiveConditionsWin.EncoderMaxSpeedLowerValue;
			else if(mainVariable == Constants.MeanPower && repetitiveConditionsWin.EncoderPowerLower)
				return repetitiveConditionsWin.EncoderPowerLowerValue;
			else if(mainVariable == Constants.PeakPower && repetitiveConditionsWin.EncoderPeakPowerLower)
				return repetitiveConditionsWin.EncoderPeakPowerLowerValue;
		}
			
		return -1;
	}

	protected virtual void on_button_close_clicked (object o, EventArgs args)
	{
		EncoderCaptureOptionsWindowBox.encoder_capture_options.Hide();
		FakeButtonClose.Click();
		//EncoderCaptureOptionsWindowBox = null;
	}
	
	protected virtual void on_delete_event (object o, DeleteEventArgs args)
	{
		button_close.Click();
		args.RetVal = true;
	}
}
