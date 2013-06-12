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
 * Copyright (C) 2004-2012   Xavier de Blas <xaviblas@gmail.com> 
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


public partial class ChronoJumpWindow 
{
	[Widget] Gtk.SpinButton spin_encoder_extra_weight;
	[Widget] Gtk.SpinButton spin_encoder_displaced_weight;
	[Widget] Gtk.SpinButton spin_encoder_1RM_percent;
	
	[Widget] Gtk.Button button_encoder_capture;
	[Widget] Gtk.RadioButton radiobutton_encoder_capture_safe;
	[Widget] Gtk.RadioButton radiobutton_encoder_capture_external;
	[Widget] Gtk.CheckButton check_encoder_inverted;
	[Widget] Gtk.Button button_encoder_bells;
	[Widget] Gtk.Button button_encoder_capture_cancel;
	[Widget] Gtk.Button button_encoder_capture_finish;
	[Widget] Gtk.Button button_encoder_recalculate;
	[Widget] Gtk.Button button_encoder_load_signal;
	[Widget] Gtk.Button button_video_play_this_test_encoder;
	[Widget] Gtk.Viewport viewport_image_encoder_capture;
	[Widget] Gtk.Image image_encoder_bell;
	[Widget] Gtk.SpinButton spin_encoder_capture_time;
	[Widget] Gtk.SpinButton spin_encoder_capture_min_height;
	[Widget] Gtk.SpinButton spin_encoder_capture_curves_height_range;
	[Widget] Gtk.Image image_encoder_capture;
	[Widget] Gtk.ProgressBar encoder_pulsebar_capture;
	[Widget] Gtk.Entry entry_encoder_signal_comment;
	[Widget] Gtk.Entry entry_encoder_curve_comment;
	[Widget] Gtk.Button button_encoder_delete_curve;
	[Widget] Gtk.Button button_encoder_save_curve;
	[Widget] Gtk.Button button_encoder_save_all_curves;
	[Widget] Gtk.Button button_encoder_export_all_curves;
	[Widget] Gtk.Button button_encoder_update_signal;
	[Widget] Gtk.Button button_encoder_delete_signal;
	
	[Widget] Gtk.Notebook notebook_encoder_sup;
	[Widget] Gtk.Notebook notebook_encoder_capture;
	[Widget] Gtk.DrawingArea encoder_capture_drawingarea;
	
	[Widget] Gtk.Box hbox_combo_encoder_exercise;
	[Widget] Gtk.ComboBox combo_encoder_exercise;
	[Widget] Gtk.Box hbox_combo_encoder_eccon;
	[Widget] Gtk.ComboBox combo_encoder_eccon;
	[Widget] Gtk.Box hbox_combo_encoder_laterality;
	[Widget] Gtk.ComboBox combo_encoder_laterality;
	[Widget] Gtk.Box hbox_combo_encoder_analyze_cross;
	[Widget] Gtk.ComboBox combo_encoder_analyze_cross;
	
	[Widget] Gtk.Button button_encoder_analyze;
	[Widget] Gtk.Button button_encoder_analyze_cancel;
	[Widget] Gtk.RadioButton radiobutton_encoder_analyze_data_current_signal;
	[Widget] Gtk.RadioButton radiobutton_encoder_analyze_data_user_curves;
	[Widget] Gtk.Box hbox_encoder_user_curves;
	[Widget] Gtk.Label label_encoder_user_curves_active_num;
	[Widget] Gtk.Label label_encoder_user_curves_all_num;
	[Widget] Gtk.Box hbox_encoder_analyze_data_compare;
	[Widget] Gtk.ComboBox combo_encoder_analyze_data_compare;
	[Widget] Gtk.Button button_encoder_analyze_data_compare;
	
	[Widget] Gtk.Button button_encoder_analyze_image_save;
	[Widget] Gtk.Button button_encoder_analyze_table_save;
	[Widget] Gtk.Button button_encoder_analyze_1RM_save;

	[Widget] Gtk.RadioButton radiobutton_encoder_analyze_powerbars;
	[Widget] Gtk.RadioButton radiobutton_encoder_analyze_cross;
	[Widget] Gtk.RadioButton radiobutton_encoder_analyze_single;
	[Widget] Gtk.RadioButton radiobutton_encoder_analyze_side;
	//[Widget] Gtk.RadioButton radiobutton_encoder_analyze_superpose;
	[Widget] Gtk.CheckButton check_encoder_analyze_eccon_together;
	[Widget] Gtk.Box hbox_encoder_analyze_curve_num;
	[Widget] Gtk.Box hbox_combo_encoder_analyze_curve_num_combo;
	[Widget] Gtk.ComboBox combo_encoder_analyze_curve_num_combo;
	[Widget] Gtk.Label label_encoder_analyze_side_max;
	
	[Widget] Gtk.Box hbox_encoder_analyze_mean_or_max;
	[Widget] Gtk.RadioButton radiobutton_encoder_analyze_mean;
	[Widget] Gtk.RadioButton radiobutton_encoder_analyze_max;
	
	[Widget] Gtk.Viewport viewport_image_encoder_analyze;
	[Widget] Gtk.Image image_encoder_analyze;
	[Widget] Gtk.ProgressBar encoder_pulsebar_analyze;
	
	[Widget] Gtk.TreeView treeview_encoder_capture_curves;
	[Widget] Gtk.TreeView treeview_encoder_analyze_curves;

	ArrayList encoderCaptureCurves;
        Gtk.ListStore encoderCaptureListStore;
	Gtk.ListStore encoderAnalyzeListStore;

	Thread encoderThreadCapture;
	Thread encoderThreadR;
	
	Gdk.Pixmap encoder_capture_pixmap = null;

	int image_encoder_width;
	int image_encoder_height;

	private string encoderAnalysis="powerBars";
	private string ecconLast;
	private string encoderTimeStamp;
	private string encoderSignalUniqueID;

	private ArrayList encoderCompareInterperson;	//personID:personName
	private ArrayList encoderCompareIntersession;	//sessionID:sessionDate

	private static int encoderCaptureCountdown;
	private static Gdk.Point [] encoderCapturePoints;		//stored to be realtime displayed
	private static int encoderCapturePointsCaptured;		//stored to be realtime displayed
	private static int encoderCapturePointsPainted;			//stored to be realtime displayed
	private static bool encoderProcessCancel;
	private static bool encoderProcessFinish;

	//smooth preferences on Sqlite since 1.3.7
	bool encoderPropulsive;
	double encoderSmoothEccCon; 
	double encoderSmoothCon;

	bool lastRecalculateWasInverted;

	//CAPTURE is the capture from csharp (not from external python)	
	//difference between CALCULECURVES and RECALCULATE_OR_LOAD is: CALCULECURVES does a autosave at end
	enum encoderModes { CAPTURE, CALCULECURVES, RECALCULATE_OR_LOAD, ANALYZE } 
	enum encoderSensEnum { 
		NOSESSION, NOPERSON, YESPERSON, PROCESSINGCAPTURE, PROCESSINGR, DONENOSIGNAL, DONEYESSIGNAL, SELECTEDCURVE }
	encoderSensEnum encoderSensEnumStored; //tracks how was sensitive before PROCESSINGCAPTURE or PROCESSINGR
	
	//for writing text
	Pango.Layout layout_encoder_capture;
 
	Gdk.GC pen_black_encoder_capture;
	Gdk.GC pen_azul_encoder_capture;

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
	

	
	private void encoderInitializeStuff() {
		encoder_pulsebar_capture.Fraction = 1;
		encoder_pulsebar_capture.Text = "";
		encoder_pulsebar_analyze.Fraction = 1;
		encoder_pulsebar_analyze.Text = "";
		
		encoderCaptureListStore = new Gtk.ListStore (typeof (EncoderCurve));

		//the glade cursor_changed does not work on mono 1.2.5 windows
		treeview_encoder_capture_curves.CursorChanged += on_treeview_encoder_capture_curves_cursor_changed; 
		createEncoderCombos();
	}
	
	void on_button_encoder_capture_clicked (object o, EventArgs args) 
	{
		if(chronopicWin.GetEncoderPort() == Util.GetDefaultPort()) {
			new DialogMessage(Constants.MessageTypes.WARNING, 
					Catalog.GetString("Chronopic port is not configured."));
			UtilGtk.ChronopicColors(viewport_chronopics, label_chronopics, label_connected_chronopics, false);
			return;
		}
		
		string analysisOptions = "";
		if(encoderPropulsive)
			analysisOptions = "p";

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
		//capture data
		EncoderParams ep = new EncoderParams(
				(int) spin_encoder_capture_time.Value, 
				(int) spin_encoder_capture_min_height.Value, 
				Convert.ToInt32(
					Util.FindOnArray(':', 2, 3, exerciseNameShown, 
					encoderExercisesTranslationAndBodyPWeight) ),	//ex.percentBodyWeight 
				Util.ConvertToPoint(findMass(true)),
				Util.ConvertToPoint(encoderSmoothEccCon),		//R decimal: '.'
				Util.ConvertToPoint(encoderSmoothCon),			//R decimal: '.'
				findEccon(true),					//force ecS (ecc-conc separated)
				analysisOptions,
				heightHigherCondition, heightLowerCondition,
				meanSpeedHigherCondition, meanSpeedLowerCondition,
				maxSpeedHigherCondition, maxSpeedLowerCondition,
				powerHigherCondition, powerLowerCondition,
				peakPowerHigherCondition, peakPowerLowerCondition,
				repetitiveConditionsWin.EncoderMainVariable,
				check_encoder_inverted.Active
				); 

		EncoderStruct es = new EncoderStruct(
				"",					//no data input
				"",					//no graph ouptut
				Util.GetEncoderDataTempFileName(), 	//OutputData1
				"", 					//OutputData2
				"", 					//SpecialData
				ep);				
				
		lastRecalculateWasInverted = check_encoder_inverted.Active;

		if (radiobutton_encoder_capture_external.Active) {
			encoderStartVideoRecord();
		
			//wait to ensure label "Rec" has been shown
			//Thread.Sleep(100);	
			//Does not work. Basically it records, but Rec message is not shown because we would need to open a new thread here
			
			//title to sen to python software has to be without spaces
			Util.RunEncoderCapturePython( 
					Util.ChangeSpaceAndMinusForUnderscore(currentPerson.Name) + "----" + 
					Util.ChangeSpaceAndMinusForUnderscore(exerciseNameShown) + "----(" + 
					Util.ConvertToPoint(findMass(true)) + "Kg)",
					es, chronopicWin.GetEncoderPort());
			
			entry_encoder_signal_comment.Text = "";
			
			encoderStopVideoRecord();
			
			calculeCurves();
		}
		else if (radiobutton_encoder_capture_safe.Active) {
			if(notebook_encoder_capture.CurrentPage == 1)
				notebook_encoder_capture.PrevPage();

			Log.WriteLine("AAAAAAAAAAAAAAA");
			encoderThreadStart(encoderModes.CAPTURE);
			
			entry_encoder_signal_comment.Text = "";

			Log.WriteLine("ZZZZZZZZZZZZZZZ");
		}
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
		spin_encoder_displaced_weight.Value = findMass(true);

		//1RM
		ArrayList array1RM = SqliteEncoder.Select1RM(false, currentPerson.UniqueID, currentSession.UniqueID, getExerciseID()); 
		double load1RM = 0;
		if(array1RM.Count > 0)
			load1RM = ((Encoder1RM) array1RM[0]).load1RM; //take only the first in array (will be the last uniqueID)

		spin_encoder_1RM_percent.Value = load1RM;
	}


	void calculeCurves() {
		encoderTimeStamp = UtilDate.ToFile(DateTime.Now);
		encoderSignalUniqueID = "-1"; //mark to know that there's no ID for this until it's saved on database

		encoderThreadStart(encoderModes.CALCULECURVES);
	}
	
	void on_button_encoder_cancel_clicked (object o, EventArgs args) 
	{
		encoderProcessCancel = true;
	}

	void on_button_encoder_capture_finish_clicked (object o, EventArgs args) 
	{
		encoderProcessFinish = true;
	}

	void on_button_encoder_recalculate_clicked (object o, EventArgs args) 
	{
		if (File.Exists(Util.GetEncoderDataTempFileName())) {
			//change sign on signal file if check_encoder_inverted changed
	
			if(lastRecalculateWasInverted != check_encoder_inverted.Active) {
				Util.ChangeSign(Util.GetEncoderDataTempFileName());
				lastRecalculateWasInverted = check_encoder_inverted.Active;
			}
			
			encoderThreadStart(encoderModes.RECALCULATE_OR_LOAD);
		}
		else
			encoder_pulsebar_capture.Text = Catalog.GetString("Missing data.");
	}
	
	private void encoderUpdateTreeViewCapture()
	{
		string contents = Util.ReadFile(Util.GetEncoderCurvesTempFileName(), false);
		if (contents == null || contents == "") {
			encoderButtonsSensitive(encoderSensEnum.DONENOSIGNAL);
		} else {
			treeviewEncoderCaptureRemoveColumns();
			int curvesNum = createTreeViewEncoderCapture(contents);
			if(curvesNum == 0) 
				encoderButtonsSensitive(encoderSensEnum.DONENOSIGNAL);
			else {
				if(Util.FindOnArray(':',1,0,UtilGtk.ComboGetActive(combo_encoder_eccon),
						encoderEcconTranslation) != "Concentric") 
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

	private void treeviewEncoderAnalyzeRemoveColumns() {
		Gtk.TreeViewColumn [] myColumns = treeview_encoder_analyze_curves.Columns;
		foreach (Gtk.TreeViewColumn column in myColumns) 
			treeview_encoder_analyze_curves.RemoveColumn (column);

		//blank the encoderAnalyzeListStore
		encoderAnalyzeListStore = new Gtk.ListStore (typeof (EncoderCurve));
	}


	//this is called by non gtk thread. Don't do gtk stuff here
	//I suppose reading gtk is ok, changing will be the problem
	//called on calculatecurves, recalculate and load
	private void encoderCreateCurvesGraphR() 
	{
		string analysisOptions = "-";
		if(encoderPropulsive)
			analysisOptions = "p";

		EncoderParams ep = new EncoderParams(
				(int) spin_encoder_capture_min_height.Value, 
				Convert.ToInt32(
					Util.FindOnArray(':', 2, 3, UtilGtk.ComboGetActive(combo_encoder_exercise), 
					encoderExercisesTranslationAndBodyPWeight) ),	//ex.percentBodyWeight 
				Util.ConvertToPoint(findMass(true)),
				findEccon(true),					//force ecS (ecc-conc separated)
				"curves",
				analysisOptions,
				Util.ConvertToPoint(encoderSmoothEccCon),		//R decimal: '.'
				Util.ConvertToPoint(encoderSmoothCon),			//R decimal: '.'
			       	0, 			//curve is not used here
				image_encoder_width, image_encoder_height,
				Util.GetDecimalSeparator()
				); 

		EncoderStruct es = new EncoderStruct(
				Util.GetEncoderDataTempFileName(), 
				Util.GetEncoderGraphTempFileName(),
				Util.GetEncoderCurvesTempFileName(), 
				Util.GetEncoderStatusTempFileName(),
				"",	//SpecialData
				ep);
		
		Util.RunEncoderGraph(
				Util.ChangeSpaceAndMinusForUnderscore(currentPerson.Name) + "-" + 
				Util.ChangeSpaceAndMinusForUnderscore(UtilGtk.ComboGetActive(combo_encoder_exercise)) + 
				"-(" + Util.ConvertToPoint(findMass(true)) + "Kg)",
				es);

		//store this to show 1,2,3,4,... or 1e,1c,2e,2c,... in RenderN
		//if is not stored, it can change when changed eccon radiobutton on cursor is in treeview
		ecconLast = findEccon(false);
	}
	
	void on_button_encoder_analyze_data_select_curves_clicked (object o, EventArgs args) 
	{
		ArrayList data = SqliteEncoder.Select(
				false, -1, currentPerson.UniqueID, currentSession.UniqueID, "curve", false);

		ArrayList dataPrint = new ArrayList();
		string [] checkboxes = new string[data.Count]; //to store active or inactive status of curves
		int count = 0;
		foreach(EncoderSQL es in data) {
			checkboxes[count++] = es.future1;
			Log.WriteLine(checkboxes[count-1]);
			dataPrint.Add(es.ToStringArray(count,true,false));
		}
	
		string [] columnsString = {
			Catalog.GetString("ID"),
			Catalog.GetString("Active"),	//checkboxes
			Catalog.GetString("Curve"),
			Catalog.GetString("Exercise"),
			Catalog.GetString("Contraction"),
			Catalog.GetString("Extra weight"),
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
				string.Format(Catalog.GetString("Saved curves of athlete {0} on this session."), 
					currentPerson.Name) + "\n" + 
				Catalog.GetString("Activate the curves you want to use clicking on first column.") + "\n" +
				Catalog.GetString("If you want to edit or delete a row, right click on it.") + "\n",
				bigArray);

		genericWin.SetTreeview(columnsString, true, dataPrint, new ArrayList(), true);
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
		genericWin.SetComboLabel(Catalog.GetString("Change the owner of selected curve") + 
				" (" + Catalog.GetString("code") + ":" + Catalog.GetString("name") + ")");
		genericWin.ShowCombo(false);
		
		genericWin.ShowButtonCancel(false);
		genericWin.SetButtonAcceptSensitive(true);
		genericWin.SetButtonCancelLabel(Catalog.GetString("Close"));
		//manage selected, unselected curves
		genericWin.Button_accept.Clicked += new EventHandler(on_encoder_show_curves_done);
		genericWin.Button_row_edit.Clicked += new EventHandler(on_encoder_show_curves_row_edit);
		genericWin.Button_row_edit_apply.Clicked += new EventHandler(on_encoder_show_curves_row_edit_apply);
		genericWin.Button_row_delete.Clicked += new EventHandler(on_encoder_show_curves_row_delete);

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
				false, -1, currentPerson.UniqueID, currentSession.UniqueID, "curve", false);

		//update on database the curves that have been selected/deselected
		int count = 0;

		Sqlite.Open();
		foreach(EncoderSQL es in data) {
			if(es.future1 != checkboxes[count]) {
				es.future1 = checkboxes[count];
				SqliteEncoder.Update(true, es);
			}
			count ++;
		}
		Sqlite.Close();

		int activeCurvesNum = getActiveCurvesNum(data);
		label_encoder_user_curves_active_num.Text = activeCurvesNum.ToString();

		string [] activeCurvesList = getActiveCheckboxesList(checkboxes, activeCurvesNum);
		UtilGtk.ComboUpdate(combo_encoder_analyze_curve_num_combo, activeCurvesList, "");
		combo_encoder_analyze_curve_num_combo.Active = 
			UtilGtk.ComboMakeActive(combo_encoder_analyze_curve_num_combo, activeCurvesList[0]);

		genericWin.HideAndNull();
		
		encoderButtonsSensitive(encoderSensEnumStored);
	}
	
	protected void on_encoder_show_curves_row_edit (object o, EventArgs args) {
		Log.WriteLine("row edit at show curves");
		Log.WriteLine(genericWin.TreeviewSelectedUniqueID.ToString());
		genericWin.ShowCombo(true);
	}

	protected void on_encoder_show_curves_row_edit_apply (object o, EventArgs args) {
		Log.WriteLine("row edit apply at show curves");
		Log.WriteLine("new person: " + genericWin.GetComboSelected);

		int newPersonID = Util.FetchID(genericWin.GetComboSelected);
		if(newPersonID != currentPerson.UniqueID) {
			int curveID = genericWin.TreeviewSelectedUniqueID;
			EncoderSQL eSQL = (EncoderSQL) SqliteEncoder.Select(false, curveID, 0, 0, "", false)[0];

			eSQL.ChangePerson(genericWin.GetComboSelected);
			genericWin.RemoveSelectedRow();
		}

		genericWin.ShowCombo(false);
	}
	
	protected void on_encoder_show_curves_row_delete (object o, EventArgs args) {
		Log.WriteLine("row delete at show curves");

		int uniqueID = genericWin.TreeviewSelectedUniqueID;
		Log.WriteLine(uniqueID.ToString());

		EncoderSQL eSQL = (EncoderSQL) SqliteEncoder.Select(false, uniqueID, 0, 0, "", false)[0];
		//remove the file
		bool deletedOk = Util.FileDelete(eSQL.GetFullURL(false));	//don't convertPathToR
		if(deletedOk)  {
			Sqlite.Delete(false, Constants.EncoderTable, Convert.ToInt32(uniqueID));
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
						false, -1, p.UniqueID, currentSession.UniqueID, "curve", false); 
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
			Catalog.GetString("Selected\ncurves"),
			Catalog.GetString("All\ncurves")
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

		genericWin.SetTreeview(columnsString, true, data, nonSensitiveRows,false);
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
			Catalog.GetString("Selected\ncurves"),
			Catalog.GetString("All\ncurves")
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
				string.Format(Catalog.GetString("Compare curves of {0} from this session with the following sessions."), 
					currentPerson.Name), bigArray);

		//convert data from array of EncoderPersonCurvesInDB to array of strings []
		ArrayList dataConverted = new ArrayList();
		foreach(EncoderPersonCurvesInDB encPS in data) {
			dataConverted.Add(encPS.ToStringArray());
		}

		genericWin.SetTreeview(columnsString, true, dataConverted, nonSensitiveRows,false);
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
				false, -1, currentPerson.UniqueID, currentSession.UniqueID, "signal", false);

		ArrayList dataPrint = new ArrayList();
		int count = 1;
		foreach(EncoderSQL es in data) 
			dataPrint.Add(es.ToStringArray(count++,false,true));
		
		string [] columnsString = {
			Catalog.GetString("ID"),
			Catalog.GetString("Signal"),
			Catalog.GetString("Exercise"),
			Catalog.GetString("Contraction"),
			Catalog.GetString("Extra weight"),
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
				string.Format(Catalog.GetString("Select signal of athlete {0} on this session."), 
					currentPerson.Name) + "\n" + 
				Catalog.GetString("If you want to edit or delete a row, right click on it."), bigArray);

		genericWin.SetTreeview(columnsString, false, dataPrint, new ArrayList(), true);
	
		//find all persons in current session
		ArrayList personsPre = SqlitePersonSession.SelectCurrentSessionPersons(currentSession.UniqueID);
		string [] persons = new String[personsPre.Count];
		count = 0;
	        foreach	(Person p in personsPre)
			persons[count++] = p.UniqueID.ToString() + ":" + p.Name;
		genericWin.SetComboValues(persons, currentPerson.UniqueID + ":" + currentPerson.Name);
		genericWin.SetComboLabel(Catalog.GetString("Change the owner of selected signal") + 
				" (" + Catalog.GetString("code") + ":" + Catalog.GetString("name") + ")");
		genericWin.ShowCombo(false);
	
		genericWin.ShowButtonCancel(true);
		genericWin.SetButtonAcceptLabel(Catalog.GetString("Load"));
		genericWin.SetButtonCancelLabel(Catalog.GetString("Close"));
		genericWin.SetButtonAcceptSensitive(false);
		genericWin.Button_accept.Clicked += new EventHandler(on_encoder_load_signal_accepted);
		genericWin.Button_row_edit.Clicked += new EventHandler(on_encoder_load_signal_row_edit);
		genericWin.Button_row_edit_apply.Clicked += new EventHandler(on_encoder_load_signal_row_edit_apply);
		genericWin.Button_row_delete.Clicked += new EventHandler(on_encoder_load_signal_row_delete);

		genericWin.ShowNow();
	}
	
	protected void on_encoder_load_signal_accepted (object o, EventArgs args)
	{
		genericWin.Button_accept.Clicked -= new EventHandler(on_encoder_load_signal_accepted);

		int uniqueID = genericWin.TreeviewSelectedRowID();
		
		genericWin.HideAndNull();

		ArrayList data = SqliteEncoder.Select(
				false, uniqueID, currentPerson.UniqueID, currentSession.UniqueID, "signal", false);

		bool success = false;
		foreach(EncoderSQL es in data) {	//it will run only one time
			success = Util.CopyEncoderDataToTemp(es.url, es.filename);
			if(success) {
				combo_encoder_exercise.Active = UtilGtk.ComboMakeActive(combo_encoder_exercise, es.exerciseName);
				combo_encoder_eccon.Active = UtilGtk.ComboMakeActive(combo_encoder_eccon, es.ecconLong);
				combo_encoder_laterality.Active = UtilGtk.ComboMakeActive(combo_encoder_laterality, es.laterality);
				spin_encoder_extra_weight.Value = Convert.ToInt32(es.extraWeight);

				spin_encoder_capture_min_height.Value = es.minHeight;
				entry_encoder_signal_comment.Text = es.description;
				encoderTimeStamp = es.GetDate(false); 
				encoderSignalUniqueID = es.uniqueID;
				button_video_play_this_test_encoder.Sensitive = (es.future2 != "");
				check_encoder_inverted.Active = (es.future3 == "1");
				lastRecalculateWasInverted = check_encoder_inverted.Active;
			}
		}

		if(success) {	
			//force a recalculate
			on_button_encoder_recalculate_clicked (o, args);
		
			radiobutton_encoder_analyze_data_current_signal.Active = true;

			encoderButtonsSensitive(encoderSensEnumStored);
		} else 
			new DialogMessage(Constants.MessageTypes.WARNING, Catalog.GetString("Sorry, file not found"));
	}
	
	protected void on_encoder_load_signal_row_edit (object o, EventArgs args) {
		Log.WriteLine("row edit at load signal");
		Log.WriteLine(genericWin.TreeviewSelectedUniqueID.ToString());
		genericWin.ShowCombo(true);
	}
	
	protected void on_encoder_load_signal_row_edit_apply (object o, EventArgs args) {
		Log.WriteLine("row edit apply at load signal");
		Log.WriteLine("new person: " + genericWin.GetComboSelected);

		int newPersonID = Util.FetchID(genericWin.GetComboSelected);
		if(newPersonID != currentPerson.UniqueID) {
			int curveID = genericWin.TreeviewSelectedUniqueID;
			Log.WriteLine(curveID.ToString());

			EncoderSQL eSQL = (EncoderSQL) SqliteEncoder.Select(false, curveID, 0, 0, "", false)[0];

			eSQL.ChangePerson(genericWin.GetComboSelected);
			genericWin.RemoveSelectedRow();
		}

		genericWin.ShowCombo(false);
	}
	
	protected void on_encoder_load_signal_row_delete (object o, EventArgs args) {
		Log.WriteLine("row delete at load signal");

		int uniqueID = genericWin.TreeviewSelectedUniqueID;
		Log.WriteLine(uniqueID.ToString());

		//if it's current signal use the delete signal from the gui interface that updates gui
		if(uniqueID == Convert.ToInt32(encoderSignalUniqueID))
			on_button_encoder_delete_signal_accepted (o, args);
		else {
			EncoderSQL eSQL = (EncoderSQL) SqliteEncoder.Select(false, uniqueID, 0, 0, "", false)[0];
			//remove the file
			bool deletedOk = Util.FileDelete(eSQL.GetFullURL(false));	//don't convertPathToR
			if(deletedOk)  
				Sqlite.Delete(false, Constants.EncoderTable, Convert.ToInt32(uniqueID));

			//genericWin selected row is deleted, unsensitive the "load" button
			genericWin.SetButtonAcceptSensitive(false);
		}
	}
	
	void on_button_encoder_export_all_curves_clicked (object o, EventArgs args) 
	{
		checkFile(Constants.EncoderCheckFileOp.ANALYZE_EXPORT_ALL_CURVES);
	}
	
	void on_button_encoder_export_all_curves_file_selected (string selectedFileName) 
	{
		string analysisOptions = "-";
		if(encoderPropulsive)
			analysisOptions = "p";

		EncoderParams ep = new EncoderParams(
				(int) spin_encoder_capture_min_height.Value, 
				Convert.ToInt32(
					Util.FindOnArray(':', 2, 3, UtilGtk.ComboGetActive(combo_encoder_exercise), 
						encoderExercisesTranslationAndBodyPWeight) ),
				Util.ConvertToPoint(findMass(true)),
				findEccon(false),		//do not force ecS (ecc-conc separated)
				"exportCSV",
				analysisOptions,
				Util.ConvertToPoint(encoderSmoothEccCon),		//R decimal: '.'
				Util.ConvertToPoint(encoderSmoothCon),			//R decimal: '.'
				Convert.ToInt32(UtilGtk.ComboGetActive(combo_encoder_analyze_curve_num_combo)),
				image_encoder_width,
				image_encoder_height,
				Util.GetDecimalSeparator()
				);

		string dataFileName = Util.GetEncoderDataTempFileName();

		EncoderStruct encoderStruct = new EncoderStruct(
				dataFileName, 
				Util.GetEncoderGraphTempFileName(),
				selectedFileName, 
				Util.GetEncoderStatusTempFileName(),
				"", 		//SpecialData
				ep);

		Util.RunEncoderGraph(
				Util.ChangeSpaceAndMinusForUnderscore(currentPerson.Name) + "-" + 
				Util.ChangeSpaceAndMinusForUnderscore(UtilGtk.ComboGetActive(combo_encoder_exercise)) + 
					"-(" + Util.ConvertToPoint(findMass(true)) + "Kg)",
				encoderStruct);

		//encoder_pulsebar_capture.Text = string.Format(Catalog.GetString(
		//			"Exported to {0}."), Util.GetEncoderExportTempFileName());
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
		ConfirmWindow confirmWin = ConfirmWindow.Show(Catalog.GetString(
					"Are you sure you want to delete this signal?"), "", "");
		confirmWin.Button_accept.Clicked += new EventHandler(on_button_encoder_delete_signal_accepted);
	}	

	void on_button_encoder_delete_signal_accepted (object o, EventArgs args) 
	{
		EncoderSQL eSQL = (EncoderSQL) SqliteEncoder.Select(
				false, Convert.ToInt32(encoderSignalUniqueID), 0, 0, "", false)[0];
		//remove the file
		bool deletedOk = Util.FileDelete(eSQL.GetFullURL(false));	//don't convertPathToR
		if(deletedOk) {
			Sqlite.Delete(false, Constants.EncoderTable, Convert.ToInt32(encoderSignalUniqueID));
			encoderSignalUniqueID = "-1";
			image_encoder_capture.Sensitive = false;
			treeviewEncoderCaptureRemoveColumns();
			encoderButtonsSensitive(encoderSensEnum.DONENOSIGNAL);
			encoder_pulsebar_capture.Text = Catalog.GetString("Signal deleted");
			entry_encoder_signal_comment.Text = "";
		}
	}

	void on_button_encoder_delete_curve_clicked (object o, EventArgs args) 
	{
		int selectedID = treeviewEncoderCaptureCurvesEventSelectedID();
		EncoderCurve curve = treeviewEncoderCaptureCurvesGetCurve(selectedID, true);

		//some start at ,5 because of the spline filtering
		int curveStart = Convert.ToInt32(decimal.Truncate(Convert.ToDecimal(curve.Start)));

		int duration;
		if( (ecconLast == "c" && selectedID == encoderCaptureCurves.Count) ||
				(ecconLast != "c" && selectedID+1 == encoderCaptureCurves.Count) )
			duration = -1; //until the end
		else {
			EncoderCurve curveNext = treeviewEncoderCaptureCurvesGetCurve(selectedID+1, false);
			if(ecconLast != "c")
				curveNext = treeviewEncoderCaptureCurvesGetCurve(selectedID+2, false);

			int curveNextStart = Convert.ToInt32(decimal.Truncate(Convert.ToDecimal(curveNext.Start)));
			duration = curveNextStart - curveStart;
		}

		if(curve.Start != null) {
			//Log.WriteLine(curveStart + "->" + duration);
			Util.EncoderDeleteCurveFromSignal(Util.GetEncoderDataTempFileName(), curveStart, duration);
		}
		//force a recalculate
		on_button_encoder_recalculate_clicked (o, args); 
	}

	void on_button_encoder_save_clicked (object o, EventArgs args) 
	{
		Gtk.Button button = (Gtk.Button) o;

		if(button == button_encoder_update_signal) 
			encoder_pulsebar_capture.Text = encoderSaveSignalOrCurve("signal", 0);
		else {
			if(button == button_encoder_save_curve) {
				int selectedID = treeviewEncoderCaptureCurvesEventSelectedID();
				encoder_pulsebar_capture.Text = encoderSaveSignalOrCurve("curve", selectedID);
			} else if(button == button_encoder_save_all_curves) 
				for(int i=1; i <= UtilGtk.CountRows(encoderCaptureListStore); i++)
					if(ecconLast == "c" || ! Util.IsEven(i)) //use only uneven (spanish: "impar") values
						encoder_pulsebar_capture.Text = encoderSaveSignalOrCurve("allCurves", i);

			updateUserCurvesLabelsAndCombo();
		}
	}

	private int getActiveCurvesNum(ArrayList curvesArray) {
		int countActiveCurves = 0;
		foreach(EncoderSQL es in curvesArray) 
			if(es.future1 == "active")
				countActiveCurves ++;
		
		return countActiveCurves;
	}

	private void updateUserCurvesLabelsAndCombo() {
		ArrayList data = SqliteEncoder.Select(
				false, -1, currentPerson.UniqueID, currentSession.UniqueID, "curve", false);
		int activeCurvesNum = getActiveCurvesNum(data);
		label_encoder_user_curves_active_num.Text = activeCurvesNum.ToString();
		label_encoder_user_curves_all_num.Text = data.Count.ToString();
		updateComboEncoderAnalyzeCurveNum(data, activeCurvesNum);	
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
		foreach(EncoderSQL es in data) {
			checkboxes[count++] = es.future1;
		}
		string [] activeCurvesList = getActiveCheckboxesList(checkboxes, activeCurvesNum);
		UtilGtk.ComboUpdate(combo_encoder_analyze_curve_num_combo, activeCurvesList, "");
		combo_encoder_analyze_curve_num_combo.Active = 
			UtilGtk.ComboMakeActive(combo_encoder_analyze_curve_num_combo, activeCurvesList[0]);
	}


	string encoderSaveSignalOrCurve (string mode, int selectedID) 
	{
		//mode is different than type. 
		//mode can be curve, allCurves or signal
		//type is to print on db at type column: curve or signal + (bar or jump)
		string signalOrCurve = "";
		string feedback = "";
		string fileSaved = "";
		string path = "";

		if(mode == "curve") {
			signalOrCurve = "curve";
			decimal curveNum = (decimal) treeviewEncoderCaptureCurvesEventSelectedID(); //on c and ec: 1,2,3,4,...
			if(ecconLast != "c")
				curveNum = decimal.Truncate((curveNum +1) /2); //1,1,2,2,...
			feedback = string.Format(Catalog.GetString("Curve {0} saved"), curveNum);
		} else if(mode == "allCurves") {
			signalOrCurve = "curve";
			feedback = Catalog.GetString("All curves saved");
		} else 	{	//mode == "signal"
			signalOrCurve = "signal";
		
			//check if data is ok (maybe encoder was not connected, then don't save this signal)
			EncoderCurve curve = treeviewEncoderCaptureCurvesGetCurve(1, false);
			if(curve.N == null)
				return "";
		}
		
		string desc = "";
		if(mode == "curve" || mode == "allCurves") {
			EncoderCurve curve = treeviewEncoderCaptureCurvesGetCurve(selectedID,true);

			//some start at ,5 because of the spline filtering
			int curveStart = Convert.ToInt32(decimal.Truncate(Convert.ToDecimal(curve.Start)));

			int duration = Convert.ToInt32(decimal.Truncate(Convert.ToDecimal(curve.Duration)));
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
			}
		
			desc = Util.RemoveTildeAndColonAndDot(entry_encoder_curve_comment.Text.ToString());

			Log.WriteLine(curveStart + "->" + duration);
			int curveIDMax = Sqlite.Max(Constants.EncoderTable, "uniqueID", false);
			fileSaved = Util.EncoderSaveCurve(Util.GetEncoderDataTempFileName(), curveStart, duration,
					currentSession.UniqueID, currentPerson.UniqueID, 
					currentPerson.Name, encoderTimeStamp, curveIDMax);
			path = Util.GetEncoderSessionDataCurveDir(currentSession.UniqueID);
		} else { //signal
			desc = Util.RemoveTildeAndColonAndDot(entry_encoder_signal_comment.Text.ToString());

			fileSaved = Util.CopyTempToEncoderData (currentSession.UniqueID, currentPerson.UniqueID, 
					currentPerson.Name, encoderTimeStamp);
			path = Util.GetEncoderSessionDataSignalDir(currentSession.UniqueID);
		}

		string myID = "-1";	
		string future3 = ""; //unused on curve	
		if(mode == "signal") {
			myID = encoderSignalUniqueID;
			future3 = Util.BoolToInt(check_encoder_inverted.Active).ToString();
		}

		EncoderSQL eSQL = new EncoderSQL(
				myID, 
				currentPerson.UniqueID, currentSession.UniqueID,
				getExerciseID(),	
				findEccon(true), 	//force ecS (ecc-conc separated)
				UtilGtk.ComboGetActive(combo_encoder_laterality),
				Util.ConvertToPoint(findMass(false)),	//when save on sql, do not include person weight
				signalOrCurve,
				fileSaved,		//to know date do: select substr(name,-23,19) from encoder;
				path,			//url
				(int) spin_encoder_capture_time.Value, 
				(int) spin_encoder_capture_min_height.Value, 
				-1,			//Since 1.3.7 smooth is not stored in curves
				desc,
				"","",
				future3,
				Util.FindOnArray(':', 2, 1, UtilGtk.ComboGetActive(combo_encoder_exercise), 
					encoderExercisesTranslationAndBodyPWeight)	//exerciseName (english)
				);

		
		//if is a signal that we just loaded, then don't insert, do an update
		//we know it because encoderUniqueID is != than "-1" if we loaded something from database
		//on curves, always insert, because it can be done with different smoothing, different params
		if(myID == "-1") {
			myID = SqliteEncoder.Insert(false, eSQL).ToString(); //Adding on SQL
			if(mode == "signal") {
				encoderSignalUniqueID = myID;
				feedback = Catalog.GetString("Signal saved");
			
				button_video_play_this_test_encoder.Sensitive = false;
				//copy video	
				if(videoOn) {
					if(Util.CopyTempVideo(currentSession.UniqueID, 
								Constants.TestTypes.ENCODER, 
								Convert.ToInt32(encoderSignalUniqueID))) {
						eSQL.future2 = Util.GetVideoFileName(currentSession.UniqueID, 
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
			feedback = Catalog.GetString("Signal updated");
		}
		
		return feedback;
	}


	//TODO: garantir path windows	
	private void on_button_encoder_analyze_clicked (object o, EventArgs args) 
	{
		//if userCurves and no data, return
		//TODO: fix this, because curves should be active except in the single curve mode
		if(radiobutton_encoder_analyze_data_user_curves.Active) {
			ArrayList data = SqliteEncoder.Select(
					false, -1, currentPerson.UniqueID, currentSession.UniqueID, "curve", false);
			if(data.Count == 0) {
				new DialogMessage(Constants.MessageTypes.WARNING, 
						Catalog.GetString("Sorry, no curves selected."));
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
						"\n\nUser curves - compare - cross variables" +
						"\n- Speed,Power / Load" +
						"\n- 1RM Bench Press" +
						"\n- 1RM Any exercise"
						);

				return;
			}

		}
	
		encoderThreadStart(encoderModes.ANALYZE);
	}

	//this is called by non gtk thread. Don't do gtk stuff here
	//I suppose reading gtk is ok, changing will be the problem
	private void captureCsharp () 
	{
		string exerciseNameShown = UtilGtk.ComboGetActive(combo_encoder_exercise);
		bool capturedOk = runEncoderCaptureCsharp( 
				Util.ChangeSpaceAndMinusForUnderscore(currentPerson.Name) + "----" + 
				Util.ChangeSpaceAndMinusForUnderscore(exerciseNameShown) + "----(" + 
				Util.ConvertToPoint(findMass(true)) + "Kg)",
				//es, 
				(int) spin_encoder_capture_time.Value, 
				Util.GetEncoderDataTempFileName(),
				chronopicWin.GetEncoderPort());
	
		//wait to ensure capture thread has ended
		Thread.Sleep(500);	

		//will start calcule curves thread
		if(capturedOk)
			calculeCurves();
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

	//private bool runEncoderCaptureCsharp(string title, EncoderStruct es, string port) 
	private bool runEncoderCaptureCsharp(string title, int time, string outputData1, string port) 
	{
		int width=encoder_capture_drawingarea.Allocation.Width;
		int height=encoder_capture_drawingarea.Allocation.Height;
		double realHeight = 1000 * 2 * spin_encoder_capture_curves_height_range.Value;
		
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
		
		int byteReaded;
		int [] bytesReaded = new int[recordingTime];

		int sum = 0;
		string dataString = "";
		string sep = "";
		
		int i =-20; //delete first records because there's encoder bug
		int msCount = 0;
		encoderCapturePoints = new Gdk.Point[recordingTime];
		encoderCapturePointsCaptured = 0;
		encoderCapturePointsPainted = 0;
		do {
			byteReaded = sp.ReadByte();
			if(byteReaded > 128)
				byteReaded = byteReaded - 256;

			//invert sign if inverted is selected
			if(check_encoder_inverted.Active)
				byteReaded *= -1;

			i=i+1;
			if(i >= 0) {
				sum += byteReaded;
				bytesReaded[i] = byteReaded;

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

		for(int j=0; j < i ; j ++) {
			writer.Write(sep + bytesReaded[j]);
			sep = ", ";
		}

		writer.Flush();
		((IDisposable)writer).Dispose();

		return true;
	}

	//this is called by non gtk thread. Don't do gtk stuff here
	//I suppose reading gtk is ok, changing will be the problem
	private void analyze () 
	{
		EncoderParams ep = new EncoderParams();
		string dataFileName = "";
		
		string analysisOptions = "-";
		if(encoderPropulsive)
			analysisOptions = "p";

		//use this send because we change it to send it to R
		//but we don't want to change encoderAnalysis because we want to know again if == "cross" 
		string sendAnalysis = encoderAnalysis;

		if(sendAnalysis == "cross") {
			string crossName = Util.FindOnArray(':',1,0,UtilGtk.ComboGetActive(combo_encoder_analyze_cross),
						encoderAnalyzeCrossTranslation);

			if(crossName == "1RM Bench Press") {
				sendAnalysis = "1RMBadillo2010";
				analysisOptions = "p";
			} else if(crossName == "1RM Any exercise") {
				//get speed1RM
				EncoderExercise ex = (EncoderExercise) SqliteEncoder.SelectEncoderExercises(false,getExerciseID(),false)[0];
				
				sendAnalysis = "1RMAnyExercise;" + Util.ConvertToPoint(ex.speed1RM) + ";" +
					SqlitePreferences.Select("encoder1RMMethod");

				analysisOptions = "p";
			} else {
				//convert: "Force / Speed" in: "cross.Force.Speed.mean"
				string [] crossNameFull = crossName.Split(new char[] {' '});
				sendAnalysis += ";" + crossNameFull[0] + ";" + crossNameFull[2]; //[1]=="/"
				if(radiobutton_encoder_analyze_mean.Active)
					sendAnalysis += ";mean";
				else
					sendAnalysis += ";max";
			}
		}
			
		if(radiobutton_encoder_analyze_data_user_curves.Active) {
			string myEccon = "ec";
			if(! check_encoder_analyze_eccon_together.Active)
				myEccon = "ecS";
			int myCurveNum = -1;
			if(sendAnalysis == "single")
				myCurveNum = Convert.ToInt32(UtilGtk.ComboGetActive(
							combo_encoder_analyze_curve_num_combo));

			//-1 because data will be different on any curve
			ep = new EncoderParams(
					-1, 
					Convert.ToInt32(
						Util.FindOnArray(':', 2, 3, 
							UtilGtk.ComboGetActive(combo_encoder_exercise), 
							encoderExercisesTranslationAndBodyPWeight) ),
					"-1",			//mass
					myEccon,	//this decides if analysis will be together or separated
					sendAnalysis,
					analysisOptions,
					Util.ConvertToPoint(encoderSmoothEccCon),		//R decimal: '.'
					Util.ConvertToPoint(encoderSmoothCon),			//R decimal: '.'
					myCurveNum,
					image_encoder_width, 
					image_encoder_height,
					Util.GetDecimalSeparator()
					);
			
			dataFileName = Util.GetEncoderGraphInputMulti();


			double bodyMass = Convert.ToDouble(currentPersonSession.Weight);

			//select curves for this person
			ArrayList data = new ArrayList();

			//select currentPerson, currentSession curves
			//onlyActive is false to have all the curves
			//this is a need for "single" to select on display correct curve
			data = SqliteEncoder.Select(
				false, -1, currentPerson.UniqueID, currentSession.UniqueID, "curve", false);
			
			//if compare persons, select curves for other persons and add
			if(Util.FindOnArray(':',1,0,UtilGtk.ComboGetActive(combo_encoder_analyze_data_compare),
					encoderDataCompareTranslation) == "Between persons") {
				ArrayList dataPre = new ArrayList();
				for (int i=0 ; i < encoderCompareInterperson.Count ; i ++) {
					dataPre = SqliteEncoder.Select(
						false, -1, 
						Util.FetchID(encoderCompareInterperson[i].ToString()),
						currentSession.UniqueID, 
						"curve", true);
					//this curves are added to data, data included currentPerson, currentSession
					foreach(EncoderSQL es in dataPre) 
						data.Add(es);
				}
			} else if(Util.FindOnArray(':',1,0,UtilGtk.ComboGetActive(combo_encoder_analyze_data_compare),
					encoderDataCompareTranslation) == "Between sessions") {
				ArrayList dataPre = new ArrayList();
				for (int i=0 ; i < encoderCompareIntersession.Count ; i ++) {
					dataPre = SqliteEncoder.Select(
						false, -1,
						currentPerson.UniqueID, 
						Util.FetchID(encoderCompareIntersession[i].ToString()),
						"curve", true);
					//this curves are added to data, data included currentPerson, currentSession
					foreach(EncoderSQL es in dataPre) 
						data.Add(es);
				}
			}


			//create dataFileName
			TextWriter writer = File.CreateText(dataFileName);
			writer.WriteLine("status,seriesName,exerciseName,mass,smoothingOne,dateTime,fullURL,eccon");
		
			Sqlite.Open();	
			ArrayList eeArray = 
					SqliteEncoder.SelectEncoderExercises(true, -1, false);
			Sqlite.Close();	
			EncoderExercise ex = new EncoderExercise();
						
Log.WriteLine("AT ANALYZE");

			int countSeries = 1;
			foreach(EncoderSQL eSQL in data) {
				foreach(EncoderExercise eeSearch in eeArray)
					if(eSQL.exerciseID == eeSearch.uniqueID)
						ex = eeSearch;

				double mass = Convert.ToDouble(eSQL.extraWeight); //TODO: future problem if this has '%'
				//EncoderExercise ex = (EncoderExercise) 
				//	SqliteEncoder.SelectEncoderExercises(true, eSQL.exerciseID, false)[0];
				mass += bodyMass * ex.percentBodyWeight / 100.0;

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

				writer.WriteLine(eSQL.future1 + "," + seriesName + "," + ex.name + "," + 
						Util.ConvertToPoint(mass).ToString() + "," + 
						Util.ConvertToPoint(eSQL.smooth) + "," + eSQL.GetDate(true) + "," + 
						fullURL + "," +	
						eSQL.eccon	//this is the eccon of every curve
						);
				countSeries ++;
			}
			writer.Flush();
			((IDisposable)writer).Dispose();
			//Sqlite.Close();	
		} else {	//current signal
			ep = new EncoderParams(
					(int) spin_encoder_capture_min_height.Value, 
					Convert.ToInt32(
						Util.FindOnArray(':', 2, 3, 
							UtilGtk.ComboGetActive(combo_encoder_exercise), 
							encoderExercisesTranslationAndBodyPWeight) ),
					Util.ConvertToPoint(findMass(true)),
					findEccon(false),		//do not force ecS (ecc-conc separated)
					sendAnalysis,
					analysisOptions,
					Util.ConvertToPoint(encoderSmoothEccCon),		//R decimal: '.'
					Util.ConvertToPoint(encoderSmoothCon),			//R decimal: '.'
					Convert.ToInt32(UtilGtk.ComboGetActive(combo_encoder_analyze_curve_num_combo)),
					image_encoder_width,
					image_encoder_height,
					Util.GetDecimalSeparator()
					);
			
			dataFileName = Util.GetEncoderDataTempFileName();
		}

		EncoderStruct encoderStruct = new EncoderStruct(
				dataFileName, 
				Util.GetEncoderGraphTempFileName(),
				Util.GetEncoderCurvesTempFileName(),	//since 1.3.6 all the analysis write curves table
				Util.GetEncoderStatusTempFileName(),
				Util.GetEncoderSpecialDataTempFileName(),
				ep);

		//show mass in title except if it's curves because then can be different mass
		//string massString = "-(" + Util.ConvertToPoint(findMass(true)) + "Kg)";
		//if(radiobutton_encoder_analyze_data_user_curves.Active)
		//	massString = "";

		string titleStr = Util.ChangeSpaceAndMinusForUnderscore(currentPerson.Name);
		//on signal show encoder exercise, but not in curves because every curve can be of a different exercise
		if( ! radiobutton_encoder_analyze_data_user_curves.Active)
			titleStr += "-" + Util.ChangeSpaceAndMinusForUnderscore(UtilGtk.ComboGetActive(combo_encoder_exercise));

		Util.RunEncoderGraph(titleStr, encoderStruct);
	}
	
	private void on_radiobutton_encoder_analyze_data_current_signal_toggled (object obj, EventArgs args) {
		int rows = UtilGtk.CountRows(encoderCaptureListStore);

		//button_encoder_analyze.Sensitive = encoderTimeStamp != null;

		bool analyze_sensitive = (rows > 0);
		if(analyze_sensitive && radiobutton_encoder_analyze_side.Active) {
			analyze_sensitive = curvesNumOkToSideCompare();
			label_encoder_analyze_side_max.Visible = ! analyze_sensitive;
		}
		button_encoder_analyze.Sensitive = analyze_sensitive;
		
		hbox_encoder_user_curves.Sensitive = false;

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
	private void on_radiobutton_encoder_analyze_data_user_curves_toggled (object obj, EventArgs args) {
		if(currentPerson != null) {
			ArrayList data = SqliteEncoder.Select(
					false, -1, currentPerson.UniqueID, currentSession.UniqueID, "curve", false);
			int activeCurvesNum = getActiveCurvesNum(data);
			updateComboEncoderAnalyzeCurveNum(data, activeCurvesNum);	
		}
		
		bool analyze_sensitive = (currentPerson != null && 
			UtilGtk.ComboGetActive(combo_encoder_analyze_curve_num_combo) != "");
		if(analyze_sensitive && radiobutton_encoder_analyze_side.Active) {
			analyze_sensitive = curvesNumOkToSideCompare();
			label_encoder_analyze_side_max.Visible = ! analyze_sensitive;
		}
		button_encoder_analyze.Sensitive = analyze_sensitive;
		
		hbox_encoder_user_curves.Sensitive = currentPerson != null;
		
		radiobutton_encoder_analyze_powerbars.Sensitive = true;
		radiobutton_encoder_analyze_single.Sensitive = true;
		radiobutton_encoder_analyze_side.Sensitive = true;
	}


	private void on_radiobutton_encoder_analyze_single_toggled (object obj, EventArgs args) {
		hbox_encoder_analyze_curve_num.Visible=true;
		hbox_combo_encoder_analyze_curve_num_combo.Visible = true;
		hbox_combo_encoder_analyze_cross.Visible=false;
		hbox_encoder_analyze_mean_or_max.Visible=false;
		encoderAnalysis="single";
		//together, mandatory
		check_encoder_analyze_eccon_together.Sensitive=false;
		check_encoder_analyze_eccon_together.Active = true;
	
		label_encoder_analyze_side_max.Visible = false;

		//restore 1RM Bench Press sensitiveness
		radiobutton_encoder_analyze_max.Sensitive = true;
		
		encoderButtonsSensitive(encoderSensEnumStored);
	}

	/*
	private void on_radiobutton_encoder_analyze_superpose_toggled (object obj, EventArgs args) {
		hbox_encoder_analyze_curve_num.Visible=true;
		hbox_combo_encoder_analyze_curve_num_combo.Visible = true;
		hbox_combo_encoder_analyze_cross.Visible=false;
		hbox_encoder_analyze_mean_or_max.Visible=false;
		encoderAnalysis="superpose";
		
		//together, mandatory
		check_encoder_analyze_eccon_together.Sensitive=false;
		check_encoder_analyze_eccon_together.Active = true;
		
		//restore 1RM Bench Press sensitiveness
		radiobutton_encoder_analyze_max.Sensitive = true;
		
		on_combo_encoder_analyze_cross_changed (obj, args);
	}
	*/
	private void on_radiobutton_encoder_analyze_side_toggled (object obj, EventArgs args) {
		hbox_encoder_analyze_curve_num.Visible=false;
		hbox_combo_encoder_analyze_curve_num_combo.Visible = false;
		hbox_combo_encoder_analyze_cross.Visible=false;
		hbox_encoder_analyze_mean_or_max.Visible=false;
		encoderAnalysis="side";
		
		//together, mandatory
		check_encoder_analyze_eccon_together.Sensitive=false;
		check_encoder_analyze_eccon_together.Active = true;

		//restore 1RM Bench Press sensitiveness
		radiobutton_encoder_analyze_max.Sensitive = true;
		
		encoderButtonsSensitive(encoderSensEnumStored);
	}
	private void on_radiobutton_encoder_analyze_powerbars_toggled (object obj, EventArgs args) {
		hbox_encoder_analyze_curve_num.Visible=false;
		hbox_combo_encoder_analyze_curve_num_combo.Visible = false;
		hbox_combo_encoder_analyze_cross.Visible=false;
		hbox_encoder_analyze_mean_or_max.Visible=false;
		encoderAnalysis="powerBars";
		
		check_encoder_analyze_eccon_together.Sensitive=true;

		label_encoder_analyze_side_max.Visible = false;

		//restore 1RM Bench Press sensitiveness
		radiobutton_encoder_analyze_max.Sensitive = true;
		
		encoderButtonsSensitive(encoderSensEnumStored);
	}
	
	private void on_radiobutton_encoder_analyze_cross_toggled (object obj, EventArgs args) {
		hbox_encoder_analyze_curve_num.Visible=false;
		hbox_combo_encoder_analyze_curve_num_combo.Visible = false;
		hbox_combo_encoder_analyze_cross.Visible=true;
		hbox_encoder_analyze_mean_or_max.Visible=true;
		encoderAnalysis="cross";
		
		check_encoder_analyze_eccon_together.Sensitive=true;

		label_encoder_analyze_side_max.Visible = false;

		on_combo_encoder_analyze_cross_changed (obj, args);

		encoderButtonsSensitive(encoderSensEnumStored);
	}
	

	private bool curvesNumOkToSideCompare() {
		if(radiobutton_encoder_analyze_data_current_signal.Active &&
			UtilGtk.CountRows(encoderCaptureListStore) <= 12)
			return true;
		else if(radiobutton_encoder_analyze_data_user_curves.Active &&
				Convert.ToInt32(label_encoder_user_curves_active_num.Text) <= 12)
			return true;

		return false;
	}

	private double findMass(bool includePerson) {
		double mass = spin_encoder_extra_weight.Value;
		if(includePerson) {
			//TODO: maybe better have a currentEncoderExercise global variable
			int exPBodyWeight = Convert.ToInt32(
					Util.FindOnArray(':', 2, 3, UtilGtk.ComboGetActive(combo_encoder_exercise), 
					encoderExercisesTranslationAndBodyPWeight) );
			mass += currentPersonSession.Weight * exPBodyWeight / 100.0;
		}

		return mass;
	}

	//TODO: check all this	
	private string findEccon(bool forceEcconSeparated) {	
		if(Util.FindOnArray(':',1,0,UtilGtk.ComboGetActive(combo_encoder_eccon),
					encoderEcconTranslation) == "Concentric") 
			return "c";
		else {
			if(forceEcconSeparated || ! check_encoder_analyze_eccon_together.Active)
				return "ecS";
			else 
				return "ec";
		}
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
		
		//create combo eccon
		string [] comboEcconOptions = { "Concentric", "Eccentric-concentric" };
		string [] comboEcconOptionsTranslated = { 
			Catalog.GetString("Concentric"), Catalog.GetString("Eccentric-concentric") };
		encoderEcconTranslation = new String [comboEcconOptions.Length];
		for(int j=0; j < 2 ; j++)
			encoderEcconTranslation[j] = comboEcconOptions[j] + ":" + comboEcconOptionsTranslated[j];
		combo_encoder_eccon = ComboBox.NewText ();
		UtilGtk.ComboUpdate(combo_encoder_eccon, comboEcconOptionsTranslated, "");
		combo_encoder_eccon.Active = UtilGtk.ComboMakeActive(combo_encoder_eccon, 
				Catalog.GetString(comboEcconOptions[0]));
		combo_encoder_eccon.Changed += new EventHandler (on_combo_encoder_eccon_changed);
		
		//create combo laterality
		string [] comboLateralityOptions = { "Both", "Right", "Left" };
		string [] comboLateralityOptionsTranslated = { 
			Catalog.GetString("Both"), Catalog.GetString("Right"), Catalog.GetString("Left") };
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
			radiobutton_encoder_analyze_mean.Active = true;
			radiobutton_encoder_analyze_max.Sensitive = false;
			check_encoder_analyze_eccon_together.Active = false;
			check_encoder_analyze_eccon_together.Sensitive = false;
		} else {
			radiobutton_encoder_analyze_max.Sensitive = true;
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
			File.Copy(Util.GetEncoderGraphTempFileName(), destination, true);
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

			//wrrite header
			writer.WriteLine(Util.RemoveNewLine(Util.StringArrayToString(
						treeviewEncoderAnalyzeHeaders, ";"), false));
			//write curves rows
			ArrayList array = getTreeViewCurves(encoderAnalyzeListStore);
			foreach (EncoderCurve ec in array)
				writer.WriteLine(ec.ToCSV());
			
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
		string contents = Util.ReadFile(Util.GetEncoderSpecialDataTempFileName(), true);
		string [] load1RMStr = contents.Split(new char[] {';'});
		double load1RM = Convert.ToDouble(Util.ChangeDecimalSeparator(load1RMStr[1]));

		SqliteEncoder.Insert1RM(false, currentPerson.UniqueID, currentSession.UniqueID, 
				getExerciseID(), load1RM);
		
		new DialogMessage(Constants.MessageTypes.INFO, Catalog.GetString("Saved"));
	}


	ArrayList getTreeViewCurves(Gtk.ListStore ls) {
		TreeIter iter = new TreeIter();
		ls.GetIterFirst ( out iter ) ;
		ArrayList array = new ArrayList();
		do {
			EncoderCurve ec = (EncoderCurve) ls.GetValue (iter, 0);
			array.Add(ec);
		} while (ls.IterNext (ref iter));
		return array;
	}


	int getExerciseID () {
		return Convert.ToInt32(
				Util.FindOnArray(':', 2, 0, UtilGtk.ComboGetActive(combo_encoder_exercise), 
				encoderExercisesTranslationAndBodyPWeight) );
	}
	
	void on_button_encoder_exercise_info_clicked (object o, EventArgs args) 
	{
		EncoderExercise ex = (EncoderExercise) SqliteEncoder.SelectEncoderExercises(false,getExerciseID(),false)[0];

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
		else if (adding && Sqlite.Exists(Constants.EncoderExerciseTable, name))
			genericWin.SetLabelError(string.Format(Catalog.GetString(
							"Error: An exercise named '{0}' already exists."), name));
		else {
			if(adding)
				SqliteEncoder.InsertExercise(false, name, genericWin.SpinIntSelected, 
						genericWin.Entry2Selected, genericWin.Entry3Selected,
						Util.ConvertToPoint(genericWin.SpinDouble2Selected));
			else
				SqliteEncoder.UpdateExercise(false, name, genericWin.SpinIntSelected, 
						genericWin.Entry2Selected, genericWin.Entry3Selected,
						Util.ConvertToPoint(genericWin.SpinDouble2Selected));

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


	/* TreeView stuff */	

	//returns curves num
	//capture has single and multiple selection in order to save curves... Analyze only shows data.
	private int createTreeViewEncoderCapture(string contents) {
		string [] columnsString = {
			Catalog.GetString("Curve") + "\n",
			Catalog.GetString("Start") + "\n (s)",
			Catalog.GetString("Duration") + "\n (s)",
			Catalog.GetString("Range") + "\n (cm)",
			Catalog.GetString("MeanSpeed") + "\n (m/s)",
			Catalog.GetString("MaxSpeed") + "\n (m/s)",
			Catalog.GetString("MaxSpeedTime") + "\n (s)",
			Catalog.GetString("MeanPower") + "\n (W)",
			Catalog.GetString("PeakPower") + "\n (W)",
			Catalog.GetString("PeakPowerTime") + "\n (s)",
			Catalog.GetString("PeakPower/PPT") + "\n (W/s)"
		};

		encoderCaptureCurves = new ArrayList ();

		string line;
		int curvesCount = 0;
		using (StringReader reader = new StringReader (contents)) {
			line = reader.ReadLine ();	//headers
			Log.WriteLine(line);
			do {
				line = reader.ReadLine ();
				Log.WriteLine(line);
				if (line == null)
					break;

				curvesCount ++;

				string [] cells = line.Split(new char[] {','});
				cells = fixDecimals(cells);

				encoderCaptureCurves.Add (new EncoderCurve (
							cells[0],	//id 
							//cells[1],	//seriesName
							//cells[2], 	//exerciseName
							//cells[3], 	//mass
							cells[4], cells[5], cells[6], 
							cells[7], cells[8], cells[9], 
							cells[10], cells[11], cells[12],
							cells[13]
							));

			} while(true);
		}

		encoderCaptureListStore = new Gtk.ListStore (typeof (EncoderCurve));
		foreach (EncoderCurve curve in encoderCaptureCurves) 
			encoderCaptureListStore.AppendValues (curve);

		treeview_encoder_capture_curves.Model = encoderCaptureListStore;

		if(ecconLast == "c")
			treeview_encoder_capture_curves.Selection.Mode = SelectionMode.Single;
		else
			treeview_encoder_capture_curves.Selection.Mode = SelectionMode.Multiple;

		treeview_encoder_capture_curves.HeadersVisible=true;

		int i=0;
		foreach(string myCol in columnsString) {
			Gtk.TreeViewColumn aColumn = new Gtk.TreeViewColumn ();
			CellRendererText aCell = new CellRendererText();
			aColumn.Title=myCol;
			aColumn.PackStart (aCell, true);

			//crt1.Foreground = "red";
			//crt1.Background = "blue";
		
			switch(i){	
				case 0:
					aColumn.SetCellDataFunc (aCell, new Gtk.TreeCellDataFunc (RenderN));
					break;
				case 1:
					aColumn.SetCellDataFunc (aCell, new Gtk.TreeCellDataFunc (RenderStart));
					break;
				case 2:
					aColumn.SetCellDataFunc (aCell, new Gtk.TreeCellDataFunc (RenderDuration));
					break;
				case 3:
					aColumn.SetCellDataFunc (aCell, new Gtk.TreeCellDataFunc (RenderHeight));
					break;
				case 4:
					aColumn.SetCellDataFunc (aCell, new Gtk.TreeCellDataFunc (RenderMeanSpeed));
					break;
				case 5:
					aColumn.SetCellDataFunc (aCell, new Gtk.TreeCellDataFunc (RenderMaxSpeed));
					break;
				case 6:
					aColumn.SetCellDataFunc (aCell, new Gtk.TreeCellDataFunc (RenderMaxSpeedT));
					break;
				case 7:
					aColumn.SetCellDataFunc (aCell, new Gtk.TreeCellDataFunc (RenderMeanPower));
					break;
				case 8:
					aColumn.SetCellDataFunc (aCell, new Gtk.TreeCellDataFunc (RenderPeakPower));
					break;
				case 9:
					aColumn.SetCellDataFunc (aCell, new Gtk.TreeCellDataFunc (RenderPeakPowerT));
					break;
				case 10:
					aColumn.SetCellDataFunc (aCell, new Gtk.TreeCellDataFunc (RenderPP_PPT));
					break;
			}
			treeview_encoder_capture_curves.AppendColumn (aColumn);
			i++;
		}
		return curvesCount;
	}

	string [] treeviewEncoderAnalyzeHeaders = {
			Catalog.GetString("Curve") + "\n",
			Catalog.GetString("Series") + "\n",
			Catalog.GetString("Exercise") + "\n",
			Catalog.GetString("Extra weight") + "\n (Kg)",
			Catalog.GetString("Start") + "\n (s)",
			Catalog.GetString("Duration") + "\n (s)",
			Catalog.GetString("Range") + "\n (cm)",
			Catalog.GetString("MeanSpeed") + "\n (m/s)",
			Catalog.GetString("MaxSpeed") + "\n (m/s)",
			Catalog.GetString("MaxSpeedTime") + "\n (s)",
			Catalog.GetString("MeanPower") + "\n (W)",
			Catalog.GetString("PeakPower") + "\n (W)",
			Catalog.GetString("PeakPowerTime") + "\n (s)",
			Catalog.GetString("PeakPower/PPT") + "\n (W/s)"
		};

	private int createTreeViewEncoderAnalyze(string contents) {
		string [] columnsString = treeviewEncoderAnalyzeHeaders;

		ArrayList encoderAnalyzeCurves = new ArrayList ();

		//write exercise and extra weight data
		ArrayList curvesData = new ArrayList();
		string exerciseName = "";
		double mass = 0; 
		if(radiobutton_encoder_analyze_data_user_curves.Active) {
			curvesData = SqliteEncoder.Select(
					false, -1, currentPerson.UniqueID, currentSession.UniqueID, "curve", true);
		} else {	//current signal
			exerciseName = UtilGtk.ComboGetActive(combo_encoder_exercise);
			mass = findMass(false);
		}

		string line;
		int curvesCount = 0;
		using (StringReader reader = new StringReader (contents)) {
			line = reader.ReadLine ();	//headers
			Log.WriteLine(line);
			do {
				line = reader.ReadLine ();
				Log.WriteLine(line);
				if (line == null)
					break;

				curvesCount ++;

				string [] cells = line.Split(new char[] {','});
				cells = fixDecimals(cells);
				
				
				if(radiobutton_encoder_analyze_data_user_curves.Active) {
					/*
					EncoderSQL eSQL = (EncoderSQL) curvesData[curvesCount];
					exerciseName = eSQL.exerciseName;
					mass = eSQL.extraWeight;
					*/
					exerciseName = cells[2];
					mass = Convert.ToDouble(cells[3]);
				}

				encoderAnalyzeCurves.Add (new EncoderCurve (
							cells[0], 
							cells[1],	//seriesName 
							exerciseName, 
							mass,
							cells[4], cells[5], cells[6], 
							cells[7], cells[8], cells[9], 
							cells[10], cells[11], cells[12],
							cells[13]
							));

			} while(true);
		}

		encoderAnalyzeListStore = new Gtk.ListStore (typeof (EncoderCurve));
		foreach (EncoderCurve curve in encoderAnalyzeCurves) 
			encoderAnalyzeListStore.AppendValues (curve);

		treeview_encoder_analyze_curves.Model = encoderAnalyzeListStore;

		treeview_encoder_analyze_curves.Selection.Mode = SelectionMode.None;

		treeview_encoder_analyze_curves.HeadersVisible=true;

		int i=0;
		foreach(string myCol in columnsString) {
			Gtk.TreeViewColumn aColumn = new Gtk.TreeViewColumn ();
			CellRendererText aCell = new CellRendererText();
			aColumn.Title=myCol;
			aColumn.PackStart (aCell, true);

			//crt1.Foreground = "red";
			//crt1.Background = "blue";
		
		
			switch(i){	
				case 0:
					aColumn.SetCellDataFunc (aCell, new Gtk.TreeCellDataFunc (RenderNAnalyze));
					break;
				case 1:
					aColumn.SetCellDataFunc (aCell, new Gtk.TreeCellDataFunc (RenderSeries));
					break;
				case 2:
					aColumn.SetCellDataFunc (aCell, new Gtk.TreeCellDataFunc (RenderExercise));
					break;
				case 3:
					aColumn.SetCellDataFunc (aCell, new Gtk.TreeCellDataFunc (RenderExtraWeight));
					break;
				case 4:
					aColumn.SetCellDataFunc (aCell, new Gtk.TreeCellDataFunc (RenderStart));
					break;
				case 5:
					aColumn.SetCellDataFunc (aCell, new Gtk.TreeCellDataFunc (RenderDuration));
					break;
				case 6:
					aColumn.SetCellDataFunc (aCell, new Gtk.TreeCellDataFunc (RenderHeight));
					break;
				case 7:
					aColumn.SetCellDataFunc (aCell, new Gtk.TreeCellDataFunc (RenderMeanSpeed));
					break;
				case 8:
					aColumn.SetCellDataFunc (aCell, new Gtk.TreeCellDataFunc (RenderMaxSpeed));
					break;
				case 9:
					aColumn.SetCellDataFunc (aCell, new Gtk.TreeCellDataFunc (RenderMaxSpeedT));
					break;
				case 10:
					aColumn.SetCellDataFunc (aCell, new Gtk.TreeCellDataFunc (RenderMeanPower));
					break;
				case 11:
					aColumn.SetCellDataFunc (aCell, new Gtk.TreeCellDataFunc (RenderPeakPower));
					break;
				case 12:
					aColumn.SetCellDataFunc (aCell, new Gtk.TreeCellDataFunc (RenderPeakPowerT));
					break;
				case 13:
					aColumn.SetCellDataFunc (aCell, new Gtk.TreeCellDataFunc (RenderPP_PPT));
					break;
			}
			
			treeview_encoder_analyze_curves.AppendColumn (aColumn);
			i++;
		}
		return curvesCount;
	}

	/* rendering columns */
	private string assignColor(double found, bool higherActive, bool lowerActive, double higherValue, double lowerValue) 
	{
		//more at System.Drawing.Color (Monodoc)
		string colorGood= "ForestGreen"; 
		string colorBad= "red";
		string colorNothing= "";	
		//colorNothing will use default color on system, previous I used black,
		//but if the color of the users theme is not 000000, then it looked too different

		if(higherActive && found >= higherValue)
			return colorGood;
		else if(lowerActive && found <= lowerValue)
			return colorBad;
		else
			return colorNothing;
	}

	private string assignColor(double found, bool higherActive, bool lowerActive, int higherValue, int lowerValue) 
	{
		return assignColor(found, higherActive, lowerActive, 
				Convert.ToDouble(higherValue), Convert.ToDouble(lowerValue));
	}

	private void RenderN (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter)
	{
		EncoderCurve curve = (EncoderCurve) model.GetValue (iter, 0);
		if(ecconLast == "c")
			(cell as Gtk.CellRendererText).Text = 
				String.Format(UtilGtk.TVNumPrint(curve.N,1,0),Convert.ToInt32(curve.N));
		else {
			string phase = "e";
			bool isEven = Util.IsEven(Convert.ToInt32(curve.N));
			if(isEven)
				phase = "c";
				
			(cell as Gtk.CellRendererText).Text = 
				decimal.Truncate((Convert.ToInt32(curve.N) +1) /2).ToString() + phase;
		}
	}
	//from analyze, don't checks ecconLast
	private void RenderNAnalyze (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter)
	{
		EncoderCurve curve = (EncoderCurve) model.GetValue (iter, 0);
		
		if(radiobutton_encoder_analyze_data_current_signal.Active && findEccon(false) == "ecS") 
		{
			string phase = "e";
			bool isEven = Util.IsEven(Convert.ToInt32(curve.N));
			if(isEven)
				phase = "c";

			(cell as Gtk.CellRendererText).Text = 
				decimal.Truncate((Convert.ToInt32(curve.N) +1) /2).ToString() + phase;
		} else
			(cell as Gtk.CellRendererText).Text = curve.N;
	}

	private void RenderSeries (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter)
	{
		EncoderCurve curve = (EncoderCurve) model.GetValue (iter, 0);
		(cell as Gtk.CellRendererText).Text = curve.Series;
	}

	private void RenderExercise (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter)
	{
		EncoderCurve curve = (EncoderCurve) model.GetValue (iter, 0);
		(cell as Gtk.CellRendererText).Text = curve.Exercise;
	}

	private void RenderExtraWeight (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter)
	{
		EncoderCurve curve = (EncoderCurve) model.GetValue (iter, 0);
		(cell as Gtk.CellRendererText).Text = 
			String.Format(UtilGtk.TVNumPrint(curve.ExtraWeight.ToString(),3,0),Convert.ToInt32(curve.ExtraWeight));
	}

	private void RenderStart (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter)
	{
		EncoderCurve curve = (EncoderCurve) model.GetValue (iter, 0);
		double myStart = Convert.ToDouble(curve.Start)/1000; //ms->s
		(cell as Gtk.CellRendererText).Text = 
			String.Format(UtilGtk.TVNumPrint(myStart.ToString(),6,3),myStart); 
	}
	
	private void RenderDuration (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter)
	{
		EncoderCurve curve = (EncoderCurve) model.GetValue (iter, 0);
		double myDuration = Convert.ToDouble(curve.Duration)/1000; //ms->s
		(cell as Gtk.CellRendererText).Text = 
			String.Format(UtilGtk.TVNumPrint(myDuration.ToString(),5,3),myDuration); 
	}
	
	private void RenderHeight (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter)
	{
		EncoderCurve curve = (EncoderCurve) model.GetValue (iter, 0);
		string heightToCm = (Convert.ToDouble(curve.Height)/10).ToString();
		string myColor = assignColor(
				Convert.ToDouble(heightToCm),
				repetitiveConditionsWin.EncoderHeightHigher, 
				repetitiveConditionsWin.EncoderHeightLower, 
				repetitiveConditionsWin.EncoderHeightHigherValue,
				repetitiveConditionsWin.EncoderHeightLowerValue);
		if(myColor != "")
			(cell as Gtk.CellRendererText).Foreground = myColor;
		else
			(cell as Gtk.CellRendererText).Foreground = null;	//will show default color

		(cell as Gtk.CellRendererText).Text = 
			String.Format(UtilGtk.TVNumPrint(heightToCm,5,1),Convert.ToDouble(heightToCm));
	}
	
	private void RenderMeanSpeed (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter)
	{
		EncoderCurve curve = (EncoderCurve) model.GetValue (iter, 0);
		string myColor = assignColor(
				Convert.ToDouble(curve.MeanSpeed),
				repetitiveConditionsWin.EncoderMeanSpeedHigher, 
				repetitiveConditionsWin.EncoderMeanSpeedLower, 
				repetitiveConditionsWin.EncoderMeanSpeedHigherValue,
				repetitiveConditionsWin.EncoderMeanSpeedLowerValue);
		if(myColor != "")
			(cell as Gtk.CellRendererText).Foreground = myColor;
		else
			(cell as Gtk.CellRendererText).Foreground = null;	//will show default color

		//no need of UtilGtk.TVNumPrint, always has 1 digit on left of decimal
		(cell as Gtk.CellRendererText).Text = 
			String.Format("{0,10:0.000}",Convert.ToDouble(curve.MeanSpeed));
	}

	private void RenderMaxSpeed (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter)
	{
		EncoderCurve curve = (EncoderCurve) model.GetValue (iter, 0);
		string myColor = assignColor(
				Convert.ToDouble(curve.MaxSpeed),
				repetitiveConditionsWin.EncoderMaxSpeedHigher, 
				repetitiveConditionsWin.EncoderMaxSpeedLower, 
				repetitiveConditionsWin.EncoderMaxSpeedHigherValue,
				repetitiveConditionsWin.EncoderMaxSpeedLowerValue);
		if(myColor != "")
			(cell as Gtk.CellRendererText).Foreground = myColor;
		else
			(cell as Gtk.CellRendererText).Foreground = null;	//will show default color

		//no need of UtilGtk.TVNumPrint, always has 1 digit on left of decimal
		(cell as Gtk.CellRendererText).Text = 
			String.Format("{0,10:0.000}",Convert.ToDouble(curve.MaxSpeed));
	}
	
	private void RenderMaxSpeedT (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter)
	{
		EncoderCurve curve = (EncoderCurve) model.GetValue (iter, 0);
		double time = Convert.ToDouble(curve.MaxSpeedT)/1000; //ms->s
		(cell as Gtk.CellRendererText).Text = 
			String.Format(UtilGtk.TVNumPrint(time.ToString(),7,3),time);
	}

	private void RenderMeanPower (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter)
	{
		EncoderCurve curve = (EncoderCurve) model.GetValue (iter, 0);
		string myColor = assignColor(
				Convert.ToDouble(curve.MeanPower),
				repetitiveConditionsWin.EncoderPowerHigher, 
				repetitiveConditionsWin.EncoderPowerLower, 
				repetitiveConditionsWin.EncoderPowerHigherValue,
				repetitiveConditionsWin.EncoderPowerLowerValue);
		if(myColor != "")
			(cell as Gtk.CellRendererText).Foreground = myColor;
		else
			(cell as Gtk.CellRendererText).Foreground = null;	//will show default color

		(cell as Gtk.CellRendererText).Text = 
			String.Format(UtilGtk.TVNumPrint(curve.MeanPower,9,3),Convert.ToDouble(curve.MeanPower));
	}

	private void RenderPeakPower (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter)
	{
		EncoderCurve curve = (EncoderCurve) model.GetValue (iter, 0);
		string myColor = assignColor(
				Convert.ToDouble(curve.PeakPower),
				repetitiveConditionsWin.EncoderPeakPowerHigher, 
				repetitiveConditionsWin.EncoderPeakPowerLower, 
				repetitiveConditionsWin.EncoderPeakPowerHigherValue,
				repetitiveConditionsWin.EncoderPeakPowerLowerValue);
		if(myColor != "")
			(cell as Gtk.CellRendererText).Foreground = myColor;
		else
			(cell as Gtk.CellRendererText).Foreground = null;	//will show default color

		(cell as Gtk.CellRendererText).Text = 
			String.Format(UtilGtk.TVNumPrint(curve.PeakPower,9,3),Convert.ToDouble(curve.PeakPower));
	}

	private void RenderPeakPowerT (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter)
	{
		EncoderCurve curve = (EncoderCurve) model.GetValue (iter, 0);
		double myPPT = Convert.ToDouble(curve.PeakPowerT)/1000; //ms->s
		(cell as Gtk.CellRendererText).Text = 
			String.Format(UtilGtk.TVNumPrint(myPPT.ToString(),7,3),myPPT);
	}

	private void RenderPP_PPT (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter)
	{
		EncoderCurve curve = (EncoderCurve) model.GetValue (iter, 0);
		(cell as Gtk.CellRendererText).Text = 
			String.Format(UtilGtk.TVNumPrint(curve.PP_PPT,6,1),Convert.ToDouble(curve.PP_PPT));
	}

	/* end of rendering cols */
	
	
	private string [] fixDecimals(string [] cells) {
		//start, width, height
		for(int i=4; i <= 6; i++)
			cells[i] = Util.TrimDecimals(Convert.ToDouble(Util.ChangeDecimalSeparator(cells[i])),1);
		
		//meanSpeed,maxSpeed,maxSpeedT, meanPower,peakPower,peakPowerT
		for(int i=7; i <= 12; i++)
			cells[i] = Util.TrimDecimals(Convert.ToDouble(Util.ChangeDecimalSeparator(cells[i])),3);
		
		//pp/ppt
		cells[13] = Util.TrimDecimals(Convert.ToDouble(Util.ChangeDecimalSeparator(cells[13])),1); 
		return cells;
	}
	
	//the bool is for ecc-concentric
	//there two rows are selected
	//if user clicks on 2n row, and bool is true, first row is the returned curve
	private EncoderCurve treeviewEncoderCaptureCurvesGetCurve(int row, bool onEccConTakeFirst) 
	{
		if(onEccConTakeFirst && ecconLast != "c") {
			bool isEven = (row % 2 == 0); //check if it's even (in spanish "par")
			if(isEven)
				row --;
		}

		TreeIter iter = new TreeIter();
		bool iterOk = encoderCaptureListStore.GetIterFirst(out iter);
		if(iterOk) {
			int count=1;
			do {
				if(count==row) 
					return (EncoderCurve) treeview_encoder_capture_curves.Model.GetValue (iter, 0);
				count ++;
			} while (encoderCaptureListStore.IterNext (ref iter));
		}
		EncoderCurve curve = new EncoderCurve();
		return curve;
	}

	private int treeviewEncoderCaptureCurvesEventSelectedID() {
		TreeIter iter = new TreeIter();
		TreeModel myModel = treeview_encoder_capture_curves.Model;
		
		if(ecconLast == "c") {
			if (treeview_encoder_capture_curves.Selection.GetSelected (out myModel, out iter)) 
				return Convert.ToInt32(((EncoderCurve) (treeview_encoder_capture_curves.Model.GetValue (iter, 0))).N); //this return an int, also in ec
		} else {
			int selectedLength = treeview_encoder_capture_curves.Selection.GetSelectedRows().Length;
			if(selectedLength == 1 || selectedLength == 2) { 
				TreePath path = treeview_encoder_capture_curves.Selection.GetSelectedRows()[0];
				myModel.GetIter(out iter, path);
				return Convert.ToInt32(((EncoderCurve) (treeview_encoder_capture_curves.Model.GetValue (iter, 0))).N);
			}
		}
		return 0;
	}
	
	private void on_treeview_encoder_capture_curves_cursor_changed (object o, EventArgs args) 
	{
		int lineNum = treeviewEncoderCaptureCurvesEventSelectedID();
		encoderButtonsSensitive(encoderSensEnum.DONENOSIGNAL);
		
		//on ecc-con select both lines
		if(ecconLast == "c") {
			if (lineNum > 0)
				encoderButtonsSensitive(encoderSensEnum.SELECTEDCURVE);
		} else {
			TreeIter iter = new TreeIter();

			treeview_encoder_capture_curves.CursorChanged -= on_treeview_encoder_capture_curves_cursor_changed; 

			if (treeview_encoder_capture_curves.Selection.GetSelectedRows().Length == 1) 
			{
				treeview_encoder_capture_curves.Selection.UnselectAll();

				//on even, select also previous row
				//on odd, select also next row
				treeview_encoder_capture_curves.Model.GetIterFirst ( out iter ) ;
				bool isEven = (lineNum % 2 == 0); //check if it's even (in spanish "par")
				int start = lineNum;
				if(isEven) 
					start = lineNum-1;

				//select 1st row
				for(int i=1; i < start; i++)
					treeview_encoder_capture_curves.Model.IterNext (ref iter);
				treeview_encoder_capture_curves.Selection.SelectIter(iter);

				//select 2nd row
				treeview_encoder_capture_curves.Model.IterNext (ref iter);
				treeview_encoder_capture_curves.Selection.SelectIter(iter);
				
				if (treeview_encoder_capture_curves.Selection.GetSelectedRows().Length == 2) 
					encoderButtonsSensitive(encoderSensEnum.SELECTEDCURVE);
			}
			treeview_encoder_capture_curves.CursorChanged += on_treeview_encoder_capture_curves_cursor_changed; 
		}
	}
	
	/* end of TreeView stuff */	

	/* sensitivity stuff */	
			
	//called when a person changes
	private void encoderPersonChanged() {
		ArrayList data = SqliteEncoder.Select(
				false, -1, currentPerson.UniqueID, currentSession.UniqueID, "curve", false);
		
		int activeCurvesNum = getActiveCurvesNum(data);
		label_encoder_user_curves_active_num.Text = activeCurvesNum.ToString();
		
		label_encoder_user_curves_all_num.Text = data.Count.ToString();
	
		if(radiobutton_encoder_analyze_data_current_signal.Active) {
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
		treeviewEncoderCaptureRemoveColumns();
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
		//c3 button_encoder_save_all_curves, button_encoder_export_all_curves,
		//	button_encoder_update_signal, 
		//	button_encoder_delete_signal, entry_encoder_signal_comment,
		//	and images: image_encoder_capture , image_encoder_analyze.Sensitive. Update: both NOT managed here
		//c4 button_encoder_delete_curve , button_encoder_save_curve, entry_encoder_curve_comment
		//c5 button_encoder_analyze
		//c6 hbox_encoder_user_curves
		//c7 button_encoder_capture_cancel (on capture and analyze)
		//c8 button_encoder_capture_finish (only on capture)

		//other dependencies
		//c5 True needs 
		//	(signal && treeviewEncoder has rows) || 
		//	(! radiobutton_encoder_analyze_data_current_signal.Active && user has curves))
		//c6 True needs radiobutton_encoder_analyze_data_user_curves.Active

		if(option != encoderSensEnum.PROCESSINGCAPTURE && option != encoderSensEnum.PROCESSINGR)
			encoderSensEnumStored = option;
		
		//columns		 	 0  1  2  3  4  5  6  7  8
		int [] noSession = 		{0, 0, 0, 0, 0, 0, 0, 0, 0};
		int [] noPerson = 		{0, 0, 0, 0, 0, 0, 0, 0, 0};
		int [] yesPerson = 		{1, 0, 1, 0, 0, 1, 1, 0, 0};
		int [] processingCapture = 	{0, 0, 0, 0, 0, 0, 0, 1, 1};
		int [] processingR = 		{0, 0, 0, 0, 0, 0, 0, 1, 0};
		int [] doneNoSignal = 		{1, 0, 1, 0, 0, 1, 1, 0, 0};
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
		
		button_encoder_save_all_curves.Sensitive = Util.IntToBool(table[3]);
		button_encoder_export_all_curves.Sensitive = Util.IntToBool(table[3]);
		button_encoder_update_signal.Sensitive = Util.IntToBool(table[3]);
		button_encoder_delete_signal.Sensitive = Util.IntToBool(table[3]);
		entry_encoder_signal_comment.Sensitive = Util.IntToBool(table[3]);
		//image_encoder_capture.Sensitive = Util.IntToBool(table[3]);
		//image_encoder_analyze.Sensitive = Util.IntToBool(table[3]);
		
		button_encoder_delete_curve.Sensitive = Util.IntToBool(table[4]);
		button_encoder_save_curve.Sensitive = Util.IntToBool(table[4]);
		entry_encoder_curve_comment.Sensitive = Util.IntToBool(table[3]);

		bool analyze_sensitive = 
			(
			 Util.IntToBool(table[5]) && 
			 (
			  (radiobutton_encoder_analyze_data_current_signal.Active && 
			   UtilGtk.CountRows(encoderCaptureListStore) > 0) 
			  ||
			  (radiobutton_encoder_analyze_data_user_curves.Active && 
			   Convert.ToInt32(label_encoder_user_curves_all_num.Text) >0)
			  )
			 );
		if(analyze_sensitive && radiobutton_encoder_analyze_side.Active) {
			analyze_sensitive = curvesNumOkToSideCompare();
			label_encoder_analyze_side_max.Visible = ! analyze_sensitive;
		} else
			label_encoder_analyze_side_max.Visible = false;
		button_encoder_analyze.Sensitive = analyze_sensitive;

		hbox_encoder_user_curves.Sensitive = 
			(Util.IntToBool(table[6]) && radiobutton_encoder_analyze_data_user_curves.Active);
		
		button_encoder_capture_cancel.Sensitive = Util.IntToBool(table[7]);
		button_encoder_analyze_cancel.Sensitive = Util.IntToBool(table[7]);
		
		button_encoder_capture_finish.Sensitive = Util.IntToBool(table[8]);
	}

	/* end of sensitivity stuff */	
	
	/* update capture graph */	
	
	private void updateEncoderCaptureGraph() 
	{
		bool refreshAreaOnly = false;

		if(encoderCapturePoints != null) 
		{
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

			encoder_capture_pixmap.DrawPoints(pen_black_encoder_capture, paintPoints);
			
			layout_encoder_capture.SetMarkup(currentPerson.Name + " (" + 
					spin_encoder_extra_weight.Value.ToString() + "Kg)");
			encoder_capture_pixmap.DrawLayout(pen_azul_encoder_capture, 5, 5, layout_encoder_capture);

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


				encoder_capture_drawingarea.QueueDrawArea( 			// -- refresh
						startX,
						//0,
						minY,
						(paintPoints[toDraw-1].X-paintPoints[0].X ) + exposeMargin,
						//encoder_capture_drawingarea.Allocation.Height
						maxY-minY
						);
				Log.WriteLine("minY - maxY " + minY + " - " + maxY);
			} else
				encoder_capture_drawingarea.QueueDraw(); 			// -- refresh

			encoderCapturePointsPainted = encoderCapturePointsCaptured;
		}
	}
	
	int encoder_capture_allocationXOld;
	bool encoder_capture_sizeChanged;
	public void on_encoder_capture_drawingarea_configure_event(object o, ConfigureEventArgs args)
	{
		Gdk.EventConfigure ev = args.Event;
		Gdk.Window window = ev.Window;

		Gdk.Rectangle allocation = encoder_capture_drawingarea.Allocation;
		
		if(encoder_capture_pixmap == null || encoder_capture_sizeChanged || 
				allocation.Width != encoder_capture_allocationXOld) {
			encoder_capture_pixmap = new Gdk.Pixmap (window, allocation.Width, allocation.Height, -1);
		
			UtilGtk.ErasePaint(encoder_capture_drawingarea, encoder_capture_pixmap);
			
			encoder_capture_sizeChanged = false;
		}

		encoder_capture_allocationXOld = allocation.Width;
	}
	
	public void on_encoder_capture_drawingarea_expose_event(object o, ExposeEventArgs args)
	{
		/* in some mono installations, configure_event is not called, but expose_event yes. 
		 * Do here the initialization
		 */
		Log.WriteLine("EXPOSE");
		
		Gdk.Rectangle allocation = encoder_capture_drawingarea.Allocation;
		if(encoder_capture_pixmap == null || encoder_capture_sizeChanged || 
				allocation.Width != encoder_capture_allocationXOld) {
			encoder_capture_pixmap = new Gdk.Pixmap (encoder_capture_drawingarea.GdkWindow, allocation.Width, allocation.Height, -1);
			UtilGtk.ErasePaint(encoder_capture_drawingarea, encoder_capture_pixmap);

			encoder_capture_sizeChanged = false;
		}

		Gdk.Rectangle area = args.Event.Area;

		//sometimes this is called when pait is finished
		//don't let this erase win
		if(encoder_capture_pixmap != null) {
			args.Event.Window.DrawDrawable(encoder_capture_drawingarea.Style.WhiteGC, encoder_capture_pixmap,
				area.X, area.Y,
				area.X, area.Y,
				area.Width, area.Height);
		}
		
		encoder_capture_allocationXOld = allocation.Width;
	}


	/* end of update capture graph */	
	
	/* thread stuff */

	private void encoderThreadStart(encoderModes mode) {
		encoderProcessCancel = false;
		encoderProcessFinish = false;
		if(mode == encoderModes.CAPTURE) {
			//encoder_pulsebar_capture.Text = Catalog.GetString("Please, wait.");
			Log.WriteLine("CCCCCCCCCCCCCCC");
			if( runEncoderCaptureCsharpCheckPort(chronopicWin.GetEncoderPort()) ) {
				UtilGtk.ErasePaint(encoder_capture_drawingarea, encoder_capture_pixmap);
				layout_encoder_capture = new Pango.Layout (encoder_capture_drawingarea.PangoContext);
				layout_encoder_capture.FontDescription = 
					Pango.FontDescription.FromString ("Courier 10");

				pen_black_encoder_capture = new Gdk.GC(encoder_capture_drawingarea.GdkWindow);
				pen_azul_encoder_capture = new Gdk.GC(encoder_capture_drawingarea.GdkWindow);

				Gdk.Colormap colormap = Gdk.Colormap.System;
				colormap.AllocColor (ref UtilGtk.BLACK,true,true);
				colormap.AllocColor (ref UtilGtk.BLUE_PLOTS,true,true);
				
				pen_black_encoder_capture.Foreground = UtilGtk.BLACK;
				pen_azul_encoder_capture.Foreground = UtilGtk.BLUE_PLOTS;

				encoderStartVideoRecord();

				encoderThreadCapture = new Thread(new ThreadStart(captureCsharp));
				GLib.Idle.Add (new GLib.IdleHandler (pulseGTKEncoderCapture));
				Log.WriteLine("DDDDDDDDDDDDDDD");
				encoderButtonsSensitive(encoderSensEnum.PROCESSINGCAPTURE);
				encoderThreadCapture.Start(); 
			} else {
				new DialogMessage(Constants.MessageTypes.WARNING, 
					Catalog.GetString("Chronopic port is not configured."));
				createChronopicWindow(true);
				return;
			}
		} else if(mode == encoderModes.CALCULECURVES || mode == encoderModes.RECALCULATE_OR_LOAD) {
			//image is inside (is smaller than) viewport
			image_encoder_width = UtilGtk.WidgetWidth(viewport_image_encoder_capture)-5; 
			image_encoder_height = UtilGtk.WidgetHeight(viewport_image_encoder_capture)-5;

//			encoder_pulsebar_capture.Text = Catalog.GetString("Please, wait.");
			treeview_encoder_capture_curves.Sensitive = false;
			encoderThreadR = new Thread(new ThreadStart(encoderCreateCurvesGraphR));
			if(mode == encoderModes.CALCULECURVES)
				GLib.Idle.Add (new GLib.IdleHandler (pulseGTKEncoderCalculeCurves));
			else // mode == encoderModes.RECALCULATE_OR_LOAD
				GLib.Idle.Add (new GLib.IdleHandler (pulseGTKEncoderRecalculateOrLoad));
			encoderButtonsSensitive(encoderSensEnum.PROCESSINGR);
			encoderThreadR.Start(); 
		} else { //encoderModes.ANALYZE
			//the -3 is because image is inside (is smaller than) viewport
			image_encoder_width = UtilGtk.WidgetWidth(viewport_image_encoder_analyze)-5; 
			image_encoder_height = UtilGtk.WidgetHeight(viewport_image_encoder_analyze)-5;

			encoder_pulsebar_analyze.Text = Catalog.GetString("Please, wait.");
		
			encoderThreadR = new Thread(new ThreadStart(analyze));
			GLib.Idle.Add (new GLib.IdleHandler (pulseGTKEncoderAnalyze));

			encoderButtonsSensitive(encoderSensEnum.PROCESSINGR);
			treeview_encoder_analyze_curves.Sensitive = false;
			button_encoder_analyze_image_save.Sensitive = false;
			button_encoder_analyze_table_save.Sensitive = false;
			button_encoder_analyze_1RM_save.Sensitive = false;

			encoderThreadR.Start(); 
		}
	}

	//this is the only who was finish	
	private bool pulseGTKEncoderCapture ()
	{
		Log.WriteLine("PPPPPPPPP");
		if(! encoderThreadCapture.IsAlive || encoderProcessCancel || encoderProcessFinish) {
			finishPulsebar(encoderModes.CAPTURE);
			Log.Write("dying");
			return false;
		}
		updatePulsebar(encoderModes.CAPTURE); //activity on pulsebar
		updateEncoderCaptureGraph();

		Thread.Sleep (50);
		Log.Write("C:" + encoderThreadCapture.ThreadState.ToString());
		return true;
	}
	
	private bool pulseGTKEncoderCalculeCurves ()
	{
		if(! encoderThreadR.IsAlive || encoderProcessCancel) {
			if(encoderProcessCancel){
				Util.CancelRScript = true;
			}

			finishPulsebar(encoderModes.CALCULECURVES);
			Log.Write("dying");
			return false;
		}
		updatePulsebar(encoderModes.CALCULECURVES); //activity on pulsebar
		Thread.Sleep (50);
		Log.Write("R:" + encoderThreadR.ThreadState.ToString());
		return true;
	}
	
	private bool pulseGTKEncoderRecalculateOrLoad ()
	{
		if(! encoderThreadR.IsAlive || encoderProcessCancel) {
			if(encoderProcessCancel){
				Util.CancelRScript = true;
			}

			finishPulsebar(encoderModes.RECALCULATE_OR_LOAD);
			Log.Write("dying");
			return false;
		}
		updatePulsebar(encoderModes.CALCULECURVES); //activity on pulsebar
		Thread.Sleep (50);
		Log.Write("R:" + encoderThreadR.ThreadState.ToString());
		return true;
	}
	
	private bool pulseGTKEncoderAnalyze ()
	{
		if(! encoderThreadR.IsAlive || encoderProcessCancel) {
			if(encoderProcessCancel){
				Util.CancelRScript = true;
			}

			finishPulsebar(encoderModes.ANALYZE);
			Log.Write("dying");
			return false;
		}
		updatePulsebar(encoderModes.ANALYZE); //activity on pulsebar
		Thread.Sleep (50);
		Log.Write("R:" + encoderThreadR.ThreadState.ToString());
		return true;
	}
	
	private void updatePulsebar (encoderModes mode) {
		if(mode == encoderModes.CAPTURE) {
			int selectedTime = (int) spin_encoder_capture_time.Value;
			encoder_pulsebar_capture.Fraction = Util.DivideSafeFraction(
					(selectedTime - encoderCaptureCountdown), selectedTime);
			encoder_pulsebar_capture.Text = encoderCaptureCountdown + " s";
			return;
		}

		try {
			string contents = Catalog.GetString("Please, wait.");
			double fraction = -1;
			if(Util.FileExists(Util.GetEncoderStatusTempFileName())) {
				contents = Util.ReadFile(Util.GetEncoderStatusTempFileName(), true);
				//contents is:
				//(1/5) Starting R
				//(5/5) R tasks done

				//-48: ascii 0 char
				if(System.Char.IsDigit(contents[1]) && System.Char.IsDigit(contents[3]))
					fraction = Util.DivideSafeFraction(
							Convert.ToInt32(contents[1]-48), Convert.ToInt32(contents[3]-48) );
			}

			if(mode == encoderModes.CALCULECURVES || mode == encoderModes.RECALCULATE_OR_LOAD) {
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
			//Util.GetEncoderStatusTempFileName() is deleted at the end of the process
			//this can make crash updatePulsebar sometimes
		}
	}
	
	private void finishPulsebar(encoderModes mode) {
		if(
				mode == encoderModes.CAPTURE || 
				mode == encoderModes.CALCULECURVES || 
				mode == encoderModes.RECALCULATE_OR_LOAD )
		{
			Log.WriteLine("ffffffinishPulsebarrrrr");
		
			//stop video		
			if(mode == encoderModes.CAPTURE) 
				encoderStopVideoRecord();
			
			//save video will be later at encoderSaveSignalOrCurve, because there encoderSignalUniqueID will be known
			
			if(encoderProcessCancel) {
				//encoderButtonsSensitive(encoderSensEnum.DONEYESSIGNAL);
				encoderButtonsSensitive(encoderSensEnumStored);
				encoder_pulsebar_capture.Text = Catalog.GetString("Cancelled");
				if(notebook_encoder_capture.CurrentPage == 0 )
					notebook_encoder_capture.NextPage();
				encoder_pulsebar_capture.Fraction = 1;
			}
			else if(mode == encoderModes.CAPTURE && encoderProcessFinish) {
				//encoderButtonsSensitive(encoderSensEnum.DONEYESSIGNAL);
				encoderButtonsSensitive(encoderSensEnumStored);
				encoder_pulsebar_capture.Text = Catalog.GetString("Finished");
			} 
			else if(mode == encoderModes.CALCULECURVES || 
					mode == encoderModes.RECALCULATE_OR_LOAD) {

				if(notebook_encoder_capture.CurrentPage == 0)
					notebook_encoder_capture.NextPage();

				Pixbuf pixbuf = new Pixbuf (Util.GetEncoderGraphTempFileName()); //from a file
				image_encoder_capture.Pixbuf = pixbuf;
				encoderUpdateTreeViewCapture();
				image_encoder_capture.Sensitive = true;
		
				//autosave signal (but not in recalculate or load)
				if(mode == encoderModes.CALCULECURVES)
					encoder_pulsebar_capture.Text = encoderSaveSignalOrCurve("signal", 0);
				else
					encoder_pulsebar_capture.Text = "";
			}

			encoder_pulsebar_capture.Fraction = 1;
			//analyze_image_save only has not to be sensitive now because capture graph will be saved
			image_encoder_analyze.Sensitive = false;
			treeview_encoder_analyze_curves.Sensitive = false;
			button_encoder_analyze_image_save.Sensitive = false;
			button_encoder_analyze_table_save.Sensitive = false;
			button_encoder_analyze_1RM_save.Sensitive = false;

		} else { //ANALYZE
			if(encoderProcessCancel) {
				encoder_pulsebar_analyze.Text = Catalog.GetString("Cancelled");
			} else {
				//TODO pensar en si s'ha de fer 1er amb mida petita i despres amb gran (en el zoom),
				//o si es una sola i fa alguna edicio
				Pixbuf pixbuf = new Pixbuf (Util.GetEncoderGraphTempFileName()); //from a file
				image_encoder_analyze.Pixbuf = pixbuf;
				encoder_pulsebar_analyze.Text = "";
			
				string contents = Util.ReadFile(Util.GetEncoderCurvesTempFileName(), false);
				if (contents != null && contents != "") {
					treeviewEncoderAnalyzeRemoveColumns();
					createTreeViewEncoderAnalyze(contents);
				}
			}

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
		Util.FileDelete(Util.GetEncoderStatusTempFileName());
	}
	
	/* end of thread stuff */
	
	/* video stuff */
	private void encoderStartVideoRecord() {
		Log.WriteLine("Starting video");
		checkbutton_video_encoder.Sensitive = false;
		if(videoOn) {
			capturer.ClickRec();
			label_video_feedback_encoder.Text = "Rec";
		}
		button_video_play_this_test_encoder.Sensitive = false; 
	}

	private void encoderStopVideoRecord() {
		Log.WriteLine("Stopping video");
		checkbutton_video_encoder.Sensitive = true;
		if(videoOn) {
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
