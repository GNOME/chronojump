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
using Gtk;
using Gdk;
using Glade;
using System.Collections;
using System.Threading;
using Mono.Unix;


public partial class ChronoJumpWindow 
{
	[Widget] Gtk.SpinButton spin_encoder_extra_weight;
	[Widget] Gtk.SpinButton spin_encoder_smooth;
	
	[Widget] Gtk.CheckButton checkbutton_encoder_propulsive;

	[Widget] Gtk.Button button_encoder_capture;
	[Widget] Gtk.Button button_encoder_bells;
	[Widget] Gtk.Button button_encoder_capture_cancel;
	[Widget] Gtk.Button button_encoder_recalculate;
	[Widget] Gtk.Button button_encoder_load_signal;
	[Widget] Gtk.Viewport viewport_image_encoder_capture;
	[Widget] Gtk.Image image_encoder_bell;
	[Widget] Gtk.SpinButton spin_encoder_capture_time;
	[Widget] Gtk.SpinButton spin_encoder_capture_min_height;
	[Widget] Gtk.Image image_encoder_capture;
	[Widget] Gtk.TreeView treeview_encoder_curves;
	[Widget] Gtk.ProgressBar encoder_pulsebar_capture;
	[Widget] Gtk.Entry entry_encoder_signal_comment;
	[Widget] Gtk.Entry entry_encoder_curve_comment;
	[Widget] Gtk.Button button_encoder_delete_curve;
	[Widget] Gtk.Button button_encoder_save_curve;
	[Widget] Gtk.Button button_encoder_save_all_curves;
	[Widget] Gtk.Button button_encoder_export_all_curves;
	[Widget] Gtk.Button button_encoder_update_signal;
	[Widget] Gtk.Button button_encoder_delete_signal;
	
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
	[Widget] Gtk.Box hbox_encoder_user_curves_num;
	[Widget] Gtk.Label label_encoder_user_curves_active_num;
	[Widget] Gtk.Label label_encoder_user_curves_all_num;
	[Widget] Gtk.Button button_encoder_analyze_data_show_user_curves;
	[Widget] Gtk.RadioButton radiobutton_encoder_analyze_powerbars;
	[Widget] Gtk.RadioButton radiobutton_encoder_analyze_single;
	[Widget] Gtk.RadioButton radiobutton_encoder_analyze_side;
	//[Widget] Gtk.RadioButton radiobutton_encoder_analyze_superpose;
	[Widget] Gtk.Box hbox_encoder_analyze_eccon;
	[Widget] Gtk.RadioButton radiobutton_encoder_eccon_both;
	[Widget] Gtk.RadioButton radiobutton_encoder_eccon_together;
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

	ArrayList encoderCurves;
        Gtk.ListStore encoderListStore;

	Thread encoderThread;

	int image_encoder_width;
	int image_encoder_height;

	private string encoderAnalysis="powerBars";
	private string ecconLast;
	private string encoderTimeStamp;
	private string encoderSignalUniqueID;
	
	//difference between CAPTURE and RECALCULATE_OR_LOAD is: CAPTURE does a autosave at end
	enum encoderModes { CAPTURE, RECALCULATE_OR_LOAD, ANALYZE } 
	enum encoderSensEnum { 
		NOSESSION, NOPERSON, YESPERSON, PROCESSING, DONENOSIGNAL, DONEYESSIGNAL, SELECTEDCURVE }
	encoderSensEnum encoderSensEnumStored; //tracks how was sensitive before PROCESSING
 
	private static bool encoderProcessCancel;

	
	//TODO: auto close capturing window

	//TODO: Put person name in graph (at title,with small separation, or inside graph at topright) (if we click on another person on treeview person, we need to know wich person was last generated graph). Put also exercise name and weight 
	//TODO: laterality have to be shown on treeviews: signal and curve. also check that is correct in database
	//TODO: the load (Kg) in graphs has to account the exercice body percent and the extra

	//TODO: put chronopic detection in a generic place. Done But:
	//TODO: solve the problem of connecting two different chronopics
	//
	//TODO:put zoom,unzoom (at side of delete curve)  in capture curves (for every curve)
	//TODO: treeview on analyze (doing in separated window)
	
	//to analyze: user has to select: session, athlete, exercise, 
	//TODO: single curve, and side, checkbox to show1 param, 2 or three
	//TODO: powerbars with checkbox to show1 param, 2 or three
	//TODO: on capture (quasi-realtime), show powerbars or curves or both
	//
	//TODO: if a signal is loaded, exercise has to be updated on combo. (use exerciseID in database)
	//
	//TODO: do the graphical capturing with pygame
	//
	//TODO: allow gui/generic.cs to select rows on treeview to be deleted
	//
	//TODO: calling to R should give feedback during the process
	//
	//TODO: fix problem that on saving maybe dirs are not created
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
		
		encoderListStore = new Gtk.ListStore (typeof (EncoderCurve));

		//the glade cursor_changed does not work on mono 1.2.5 windows
		treeview_encoder_curves.CursorChanged += on_treeview_encoder_curves_cursor_changed; 
		createEncoderCombos();
	}

	//TODO: garantir path windows	
	void on_button_encoder_capture_clicked (object o, EventArgs args) 
	{
		if(chronopicWin.GetEncoderPort() == Util.GetDefaultPort()) {
			new DialogMessage(Constants.MessageTypes.WARNING, 
					Catalog.GetString("Chronopic port is not configured."));
			UtilGtk.ChronopicColors(viewport_chronopics, label_chronopics, label_connected_chronopics, false);
			return;
		}

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
				findMass(true),
				Util.ConvertToPoint((double) spin_encoder_smooth.Value), //R decimal: '.'
				findEccon(true),					//force ecS (ecc-conc separated)
				heightHigherCondition, heightLowerCondition,
				meanSpeedHigherCondition, meanSpeedLowerCondition,
				maxSpeedHigherCondition, maxSpeedLowerCondition,
				powerHigherCondition, powerLowerCondition,
				peakPowerHigherCondition, peakPowerLowerCondition,
				repetitiveConditionsWin.EncoderMainVariable
				); 

		EncoderStruct es = new EncoderStruct(
				"",					//no data input
				"",					//no graph ouptut
				Util.GetEncoderDataTempFileName(), "", ep);				

		//title to sen to python software has to be without spaces
		Util.RunEncoderCapture( 
				Util.ChangeSpaceForUnderscore(currentPerson.Name) + "----" + 
				Util.ChangeSpaceForUnderscore(exerciseNameShown) + "----(" + findMass(true) + "Kg)",
				es, chronopicWin.GetEncoderPort());

		encoderTimeStamp = UtilDate.ToFile(DateTime.Now);
		encoderSignalUniqueID = "-1"; //mark to know that there's no ID for this until it's saved on database

		encoderThreadStart(encoderModes.CAPTURE);
	}
		
	void on_button_encoder_cancel_clicked (object o, EventArgs args) 
	{
		encoderProcessCancel = true;
	}

	void on_button_encoder_recalculate_clicked (object o, EventArgs args) 
	{
		if (File.Exists(Util.GetEncoderDataTempFileName()))
			encoderThreadStart(encoderModes.RECALCULATE_OR_LOAD);
		else
			encoder_pulsebar_capture.Text = Catalog.GetString("Missing data.");
	}
	
	private void encoderUpdateTreeView()
	{
		string contents = Util.ReadFile(Util.GetEncoderCurvesTempFileName(), false);
		if (contents == null || contents == "") {
			encoderButtonsSensitive(encoderSensEnum.DONENOSIGNAL);
		} else {
			treeviewEncoderRemoveColumns();
			int curvesNum = createTreeViewEncoder(contents);
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
	
	private void treeviewEncoderRemoveColumns() {
		Gtk.TreeViewColumn [] myColumns = treeview_encoder_curves.Columns;
		foreach (Gtk.TreeViewColumn column in myColumns) 
			treeview_encoder_curves.RemoveColumn (column);

		//blank the encoderListStore
		encoderListStore = new Gtk.ListStore (typeof (EncoderCurve));
	}


	//this is called by non gtk thread. Don't do gtk stuff here
	//I suppose reading gtk is ok, changing will be the problem
	private void encoderCreateCurvesGraphR() 
	{
		string analysisOptions = "-";
		if(checkbutton_encoder_propulsive.Active)
			analysisOptions = "p";

		EncoderParams ep = new EncoderParams(
				(int) spin_encoder_capture_min_height.Value, 
				Convert.ToInt32(
					Util.FindOnArray(':', 2, 3, UtilGtk.ComboGetActive(combo_encoder_exercise), 
					encoderExercisesTranslationAndBodyPWeight) ),	//ex.percentBodyWeight 
				findMass(true),
				findEccon(true),					//force ecS (ecc-conc separated)
				"curves",
				analysisOptions,
				Util.ConvertToPoint((double) spin_encoder_smooth.Value), //R decimal: '.'
			       	0, 			//curve is not used here
				image_encoder_width, image_encoder_height); 

		EncoderStruct es = new EncoderStruct(
				Util.GetEncoderDataTempFileName(), 
				Util.GetEncoderGraphTempFileName(),
				Util.GetEncoderCurvesTempFileName(), 
				Util.GetEncoderStatusTempFileName(),
				ep);
		
		Util.RunEncoderGraph(
				Util.ChangeSpaceForUnderscore(currentPerson.Name) + "-" + 
				Util.ChangeSpaceForUnderscore(UtilGtk.ComboGetActive(combo_encoder_exercise)) + 
				"-(" + findMass(true) + "Kg)",
				es);

		//store this to show 1,2,3,4,... or 1e,1c,2e,2c,... in RenderN
		//if is not stored, it can change when changed eccon radiobutton on cursor is in treeview
		ecconLast = findEccon(false);
	}
	
	void on_button_encoder_analyze_data_show_user_curves_clicked (object o, EventArgs args) 
	{
		ArrayList data = SqliteEncoder.Select(false, -1, currentPerson.UniqueID, currentSession.UniqueID, "curve");

		ArrayList dataPrint = new ArrayList();
		string [] checkboxes = new string[data.Count]; //to store active or inactive status of curves
		int count = 0;
		foreach(EncoderSQL es in data) {
			checkboxes[count++] = es.future1;
			Log.WriteLine(checkboxes[count-1]);
			dataPrint.Add(es.ToStringArray(count));
		}
	
		string [] columnsString = {
			Catalog.GetString("ID"),
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
		
		//0 is the widgget to show; 1 is the editable; 2 id default value
		a1.Add(Constants.GenericWindowShow.COMBOALLNONESELECTED); a1.Add(true); a1.Add("ALL");
		bigArray.Add(a1);
		
		a2.Add(Constants.GenericWindowShow.TREEVIEW); a2.Add(true); a2.Add("");
		bigArray.Add(a2);
		
		genericWin = GenericWindow.Show(false,	//don't show now
				string.Format(Catalog.GetString("Saved curves of athlete {0} on this session."), 
					currentPerson.Name), bigArray);

		genericWin.SetTreeview(columnsString, true, dataPrint);
		genericWin.MarkActiveCurves(checkboxes);
		genericWin.ShowButtonCancel(false);
		genericWin.SetButtonAcceptSensitive(true);
		//manage selected, unselected curves
		genericWin.Button_accept.Clicked += new EventHandler(on_encoder_show_curves_done);

		//used when we don't need to read data, 
		//and we want to ensure next window will be created at needed size
		//genericWin.DestroyOnAccept=true;
		//here is comented because we are going to read the checkboxes

		genericWin.SetButtonAcceptLabel(Catalog.GetString("Close"));
		genericWin.ShowNow();
	}
	
	protected void on_encoder_show_curves_done (object o, EventArgs args)
	{
		genericWin.Button_accept.Clicked -= new EventHandler(on_encoder_show_curves_done);

		//get selected/deselected rows
		string [] checkboxes = genericWin.GetCheckboxesStatus();
		Log.WriteLine(Util.StringArrayToString(checkboxes,";"));

		ArrayList data = SqliteEncoder.Select(false, -1, currentPerson.UniqueID, currentSession.UniqueID, "curve");

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

		string [] activeCurvesList = getActiveCurvesList(checkboxes, activeCurvesNum);
		UtilGtk.ComboUpdate(combo_encoder_analyze_curve_num_combo, activeCurvesList, "");
		combo_encoder_analyze_curve_num_combo.Active = 
			UtilGtk.ComboMakeActive(combo_encoder_analyze_curve_num_combo, activeCurvesList[0]);

		genericWin.HideAndNull();
		
		encoderButtonsSensitive(encoderSensEnumStored);
	}

		
	void on_button_encoder_load_signal_clicked (object o, EventArgs args) 
	{
		ArrayList data = SqliteEncoder.Select(false, -1, currentPerson.UniqueID, currentSession.UniqueID, "signal");

		ArrayList dataPrint = new ArrayList();
		int count = 1;
		foreach(EncoderSQL es in data) 
			dataPrint.Add(es.ToStringArray(count++));
		
		string [] columnsString = {
			Catalog.GetString("ID"),
			Catalog.GetString("Signal"),
			Catalog.GetString("Exercise"),
			Catalog.GetString("Contraction"),
			Catalog.GetString("Extra weight"),
			Catalog.GetString("Date"),
			Catalog.GetString("Comment")
		};

		genericWin = GenericWindow.Show(
				string.Format(Catalog.GetString("Select signal of athlete {0} on this session."), 
					currentPerson.Name), Constants.GenericWindowShow.TREEVIEW);

		genericWin.SetTreeview(columnsString, false, dataPrint);
		genericWin.SetButtonAcceptLabel(Catalog.GetString("Load"));
		genericWin.SetButtonAcceptSensitive(false);
		genericWin.Button_accept.Clicked += new EventHandler(on_encoder_load_signal_accepted);
	}
	
	protected void on_encoder_load_signal_accepted (object o, EventArgs args)
	{
		genericWin.Button_accept.Clicked -= new EventHandler(on_encoder_load_signal_accepted);

		int uniqueID = genericWin.TreeviewSelectedRowID();
		
		genericWin.HideAndNull();

		ArrayList data = SqliteEncoder.Select(false, uniqueID, 
				currentPerson.UniqueID, currentSession.UniqueID, "signal");

		foreach(EncoderSQL es in data) {	//it will run only one time
			Util.CopyEncoderDataToTemp(es.url, es.filename);
			combo_encoder_exercise.Active = UtilGtk.ComboMakeActive(combo_encoder_exercise, es.exerciseName);
			combo_encoder_eccon.Active = UtilGtk.ComboMakeActive(combo_encoder_eccon, es.ecconLong);
			combo_encoder_laterality.Active = UtilGtk.ComboMakeActive(combo_encoder_laterality, es.laterality);
			spin_encoder_extra_weight.Value = Convert.ToInt32(es.extraWeight);

			spin_encoder_capture_min_height.Value = es.minHeight;
			spin_encoder_smooth.Value = es.smooth;
			encoderTimeStamp = es.GetDate(false); 
			encoderSignalUniqueID = es.uniqueID;
		}
	
		//force a recalculate
		on_button_encoder_recalculate_clicked (o, args);
		
		radiobutton_encoder_analyze_data_current_signal.Active = true;
		
		encoderButtonsSensitive(encoderSensEnumStored);
	}
	
	void on_button_encoder_export_all_curves_clicked (object o, EventArgs args) 
	{
		checkFile();
	}
	
	void on_button_encoder_export_all_curves_file_selected (string selectedFileName) 
	{
		string analysisOptions = "-";
		if(checkbutton_encoder_propulsive.Active)
			analysisOptions = "p";

		EncoderParams ep = new EncoderParams(
				(int) spin_encoder_capture_min_height.Value, 
				Convert.ToInt32(
					Util.FindOnArray(':', 2, 3, UtilGtk.ComboGetActive(combo_encoder_exercise), 
						encoderExercisesTranslationAndBodyPWeight) ),
				findMass(true),
				findEccon(false),		//do not force ecS (ecc-conc separated)
				"exportCSV",
				analysisOptions,
				Util.ConvertToPoint((double) spin_encoder_smooth.Value), //R decimal: '.'
				Convert.ToInt32(UtilGtk.ComboGetActive(combo_encoder_analyze_curve_num_combo)),
				image_encoder_width,
				image_encoder_height); 

		string dataFileName = Util.GetEncoderDataTempFileName();

		EncoderStruct encoderStruct = new EncoderStruct(
				dataFileName, 
				Util.GetEncoderGraphTempFileName(),
				selectedFileName, "NULL", ep);

		Util.RunEncoderGraph(
				Util.ChangeSpaceForUnderscore(currentPerson.Name) + "-" + 
				Util.ChangeSpaceForUnderscore(UtilGtk.ComboGetActive(combo_encoder_exercise)) + 
					"-(" + findMass(true) + "Kg)",
				encoderStruct);

		//encoder_pulsebar_capture.Text = string.Format(Catalog.GetString(
		//			"Exported to {0}."), Util.GetEncoderExportTempFileName());
	}

	string exportFileName;	
	protected void checkFile ()
	{
		string exportString = Catalog.GetString ("Export session in format CSV");
		
		Gtk.FileChooserDialog fc=
			new Gtk.FileChooserDialog(exportString,
					app1,
					FileChooserAction.Save,
					Catalog.GetString("Cancel"),ResponseType.Cancel,
					Catalog.GetString("Export"),ResponseType.Accept
					);

		if (fc.Run() == (int)ResponseType.Accept) 
		{
			exportFileName = fc.Filename;
			//add ".csv" if needed
			exportFileName = Util.AddCsvIfNeeded(exportFileName);
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
					confirmWin.Button_accept.Clicked += new EventHandler(on_overwrite_file_accepted);
				} else {
					on_button_encoder_export_all_curves_file_selected (exportFileName);

					string myString = string.Format(Catalog.GetString("Exported to {0}"), 
							exportFileName) + Constants.SpreadsheetString;
					new DialogMessage(Constants.MessageTypes.INFO, myString);
				}
			} 
			catch {
				string myString = string.Format(
						Catalog.GetString("Cannot export to file {0} "), exportFileName);
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
	private void on_overwrite_file_accepted(object o, EventArgs args)
	{
		on_button_encoder_export_all_curves_file_selected (exportFileName);

		string myString = string.Format(Catalog.GetString("Exported to {0}"), 
				exportFileName) + Constants.SpreadsheetString;
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
				false, Convert.ToInt32(encoderSignalUniqueID), 0, 0, "")[0];
		//remove the file
		bool deletedOk = Util.FileDelete(eSQL.GetFullURL());
		if(deletedOk) {
			Sqlite.Delete(Constants.EncoderTable, Convert.ToInt32(encoderSignalUniqueID));
			encoderSignalUniqueID = "-1";
			treeviewEncoderRemoveColumns();
			encoderButtonsSensitive(encoderSensEnum.DONENOSIGNAL);
			encoder_pulsebar_capture.Text = Catalog.GetString("Signal deleted");
		}
	}

	void on_button_encoder_delete_curve_clicked (object o, EventArgs args) 
	{
		int selectedID = treeviewEncoderCurvesEventSelectedID();
		EncoderCurve curve = treeviewEncoderCurvesGetCurve(selectedID, true);

		//some start at ,5 because of the spline filtering
		int curveStart = Convert.ToInt32(decimal.Truncate(Convert.ToDecimal(curve.Start)));

		int duration;
		if( (ecconLast == "c" && selectedID == encoderCurves.Count) ||
				(ecconLast != "c" && selectedID+1 == encoderCurves.Count) )
			duration = -1; //until the end
		else {
			EncoderCurve curveNext = treeviewEncoderCurvesGetCurve(selectedID+1, false);
			if(ecconLast != "c")
				curveNext = treeviewEncoderCurvesGetCurve(selectedID+2, false);

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
				int selectedID = treeviewEncoderCurvesEventSelectedID();
				encoder_pulsebar_capture.Text = encoderSaveSignalOrCurve("curve", selectedID);
			} else if(button == button_encoder_save_all_curves) 
				for(int i=1; i <= UtilGtk.CountRows(encoderListStore); i++)
					encoder_pulsebar_capture.Text = encoderSaveSignalOrCurve("allCurves", i);

			ArrayList data = SqliteEncoder.Select(false, -1, 
					currentPerson.UniqueID, currentSession.UniqueID, "curve");
			int activeCurvesNum = getActiveCurvesNum(data);
			label_encoder_user_curves_active_num.Text = activeCurvesNum.ToString();
			label_encoder_user_curves_all_num.Text = data.Count.ToString();
			updateComboEncoderAnalyzeCurveNum(data, activeCurvesNum);	
		}
	}

	private int getActiveCurvesNum(ArrayList curvesArray) {
		int countActiveCurves = 0;
		foreach(EncoderSQL es in curvesArray) 
			if(es.future1 == "active")
				countActiveCurves ++;
		
		return countActiveCurves;
	}
	
	private string [] getActiveCurvesList(string [] checkboxes, int activeCurvesNum) {
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
		string [] activeCurvesList = getActiveCurvesList(checkboxes, activeCurvesNum);
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
			decimal curveNum = (decimal) treeviewEncoderCurvesEventSelectedID(); //on c and ec: 1,2,3,4,...
			if(ecconLast != "c")
				curveNum = decimal.Truncate((curveNum +1) /2); //1,1,2,2,...
			feedback = string.Format(Catalog.GetString("Curve {0} saved"), curveNum);
		} else if(mode == "allCurves") {
			signalOrCurve = "curve";
			feedback = Catalog.GetString("All curves saved");
		} else 	{	//mode == "signal"
			signalOrCurve = "signal";
		
			//check if data is ok (maybe encoder was not connected, then don't save this signal)
			EncoderCurve curve = treeviewEncoderCurvesGetCurve(1, false);
			if(curve.N == null)
				return "";
		}
		
		string desc = "";
		if(mode == "curve" || mode == "allCurves") {
			EncoderCurve curve = treeviewEncoderCurvesGetCurve(selectedID,true);

			//some start at ,5 because of the spline filtering
			int curveStart = Convert.ToInt32(decimal.Truncate(Convert.ToDecimal(curve.Start)));

			int duration = Convert.ToInt32(decimal.Truncate(Convert.ToDecimal(curve.Duration)));
			if(ecconLast != "c") {
				EncoderCurve curveNext = treeviewEncoderCurvesGetCurve(selectedID+1,false);
				duration += Convert.ToInt32(decimal.Truncate(Convert.ToDecimal(curveNext.Duration)));
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
		if(mode == "signal")
			myID = encoderSignalUniqueID;

		EncoderSQL eSQL = new EncoderSQL(
				myID, 
				currentPerson.UniqueID, currentSession.UniqueID, 
				Convert.ToInt32(
					Util.FindOnArray(':', 2, 0, UtilGtk.ComboGetActive(combo_encoder_exercise), 
					encoderExercisesTranslationAndBodyPWeight) ),	//exerciseID
				findEccon(true), 	//force ecS (ecc-conc separated)
				UtilGtk.ComboGetActive(combo_encoder_laterality),
				findMass(false),	//when save on sql, do not include person weight
				signalOrCurve,
				fileSaved,		//to know date do: select substr(name,-23,19) from encoder;
				path,			//url
				(int) spin_encoder_capture_time.Value, 
				(int) spin_encoder_capture_min_height.Value, 
				(double) spin_encoder_smooth.Value,
				desc,
				"","","",
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
			ArrayList data = SqliteEncoder.Select(false, -1, 
					currentPerson.UniqueID, currentSession.UniqueID, "curve");
			if(data.Count == 0)
				return;
			//TODO: in the future plot a "no curves" message,
			//or beter done allow to analyze if there's no curves
		}
	
		encoderThreadStart(encoderModes.ANALYZE);
	}
	
	//this is called by non gtk thread. Don't do gtk stuff here
	//I suppose reading gtk is ok, changing will be the problem
	private void analyze () 
	{
		EncoderParams ep = new EncoderParams();
		string dataFileName = "";
		
		string analysisOptions = "-";
		if(checkbutton_encoder_propulsive.Active)
			analysisOptions = "p";

		//use this send because we change it to send it to R
		//but we don't want to change encoderAnalysis because we want to know again if == "cross" 
		string sendAnalysis = encoderAnalysis;

		if(sendAnalysis == "cross") {
			string crossName = Util.FindOnArray(':',1,0,UtilGtk.ComboGetActive(combo_encoder_analyze_cross),
						encoderAnalyzeCrossTranslation);

			if(crossName == "1RM Prediction") {
				sendAnalysis = "1RMBadillo2010";
				analysisOptions = "p";
			} else {
				//convert: "Force / Speed" in: "cross.Force.Speed.mean"
				string [] crossNameFull = crossName.Split(new char[] {' '});
				sendAnalysis += "." + crossNameFull[0] + "." + crossNameFull[2]; //[1]=="/"
				if(radiobutton_encoder_analyze_mean.Active)
					sendAnalysis += ".mean";
				else
					sendAnalysis += ".max";
			}
		}
			
		if(radiobutton_encoder_analyze_data_user_curves.Active) {
			string myEccon = "ec";
			if(! radiobutton_encoder_eccon_together.Active)
				myEccon = "ecS";
			int myCurveNum = -1;
			if(sendAnalysis == "single")
				myCurveNum = Convert.ToInt32(UtilGtk.ComboGetActive(combo_encoder_analyze_curve_num_combo));

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
					"-1",
					myCurveNum,
					image_encoder_width, 
					image_encoder_height); 
			
			dataFileName = Util.GetEncoderGraphInputMulti();

			//create dataFileName
			double bodyMass = Convert.ToDouble(currentPersonSession.Weight);
			ArrayList data = SqliteEncoder.Select(false, -1, 
					currentPerson.UniqueID, currentSession.UniqueID, "curve");

			TextWriter writer = File.CreateText(dataFileName);
			writer.WriteLine("status,exerciseName,mass,smoothingOne,dateTime,fullURL,eccon");
		
			Sqlite.Open();	
			ArrayList eeArray = 
					SqliteEncoder.SelectEncoderExercises(true, -1, false);
			Sqlite.Close();	
			EncoderExercise ex = new EncoderExercise();

			foreach(EncoderSQL eSQL in data) {
				foreach(EncoderExercise eeSearch in eeArray)
					if(eSQL.exerciseID == eeSearch.uniqueID)
						ex = eeSearch;

				double mass = Convert.ToDouble(eSQL.extraWeight); //TODO: future problem if this has '%'
				//EncoderExercise ex = (EncoderExercise) 
				//	SqliteEncoder.SelectEncoderExercises(true, eSQL.exerciseID, false)[0];
				mass += bodyMass * ex.percentBodyWeight / 100.0;

				writer.WriteLine(eSQL.future1 + "," + ex.name + "," + 
						Util.ConvertToPoint(mass).ToString() + "," + 
						Util.ConvertToPoint(eSQL.smooth) + "," + eSQL.GetDate(true) + "," + 
						eSQL.GetFullURL() + "," +
						eSQL.eccon	//this is the eccon of every curve
						);
			}
			writer.Flush();
			((IDisposable)writer).Dispose();
			//Sqlite.Close();	
		} else {
			ep = new EncoderParams(
					(int) spin_encoder_capture_min_height.Value, 
					Convert.ToInt32(
						Util.FindOnArray(':', 2, 3, 
							UtilGtk.ComboGetActive(combo_encoder_exercise), 
							encoderExercisesTranslationAndBodyPWeight) ),
					findMass(true),
					findEccon(false),		//do not force ecS (ecc-conc separated)
					sendAnalysis,
					analysisOptions,
					Util.ConvertToPoint((double) spin_encoder_smooth.Value), //R decimal: '.'
					Convert.ToInt32(UtilGtk.ComboGetActive(combo_encoder_analyze_curve_num_combo)),
					image_encoder_width,
					image_encoder_height); 
			
			dataFileName = Util.GetEncoderDataTempFileName();
		}

		EncoderStruct encoderStruct = new EncoderStruct(
				dataFileName, 
				Util.GetEncoderGraphTempFileName(),
				"NULL", //no data ouptut
				Util.GetEncoderStatusTempFileName(),
				ep);

		//show mass in title except if it's curves because then can be different mass
		//string massString = "-(" + findMass(true) + "Kg)";
		//if(radiobutton_encoder_analyze_data_user_curves.Active)
		//	massString = "";

		Util.RunEncoderGraph(
				Util.ChangeSpaceForUnderscore(currentPerson.Name) + "-" + 
				Util.ChangeSpaceForUnderscore(UtilGtk.ComboGetActive(combo_encoder_exercise)), encoderStruct);
	}
	
	private void on_radiobutton_encoder_analyze_data_current_signal_toggled (object obj, EventArgs args) {
		int rows = UtilGtk.CountRows(encoderListStore);

		//button_encoder_analyze.Sensitive = encoderTimeStamp != null;

		bool analyze_sensitive = (rows > 0);
		if(analyze_sensitive && radiobutton_encoder_analyze_side.Active) {
			analyze_sensitive = curvesNumOkToSideCompare();
			label_encoder_analyze_side_max.Visible = ! analyze_sensitive;
		}
		button_encoder_analyze.Sensitive = analyze_sensitive;
		
		button_encoder_analyze_data_show_user_curves.Sensitive = false;
		hbox_encoder_user_curves_num.Sensitive = false;

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
	}
	private void on_radiobutton_encoder_analyze_data_user_curves_toggled (object obj, EventArgs args) {
		if(currentPerson != null) {
			ArrayList data = SqliteEncoder.Select(false, -1, 
					currentPerson.UniqueID, currentSession.UniqueID, "curve");
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
		

		button_encoder_analyze_data_show_user_curves.Sensitive = currentPerson != null;
		hbox_encoder_user_curves_num.Sensitive = currentPerson != null;
	}


	//show curve_num only on simple and superpose
	private void on_radiobutton_encoder_analyze_single_toggled (object obj, EventArgs args) {
		hbox_encoder_analyze_curve_num.Visible=true;
		hbox_combo_encoder_analyze_curve_num_combo.Visible = true;
		hbox_combo_encoder_analyze_cross.Visible=false;
		hbox_encoder_analyze_mean_or_max.Visible=false;
		encoderAnalysis="single";
		//together, mandatory
		hbox_encoder_analyze_eccon.Visible=false;
		radiobutton_encoder_eccon_together.Active = true;
		label_encoder_analyze_side_max.Visible = false;

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
		hbox_encoder_analyze_eccon.Visible=false;
		radiobutton_encoder_eccon_together.Active = true;
		
		encoderButtonsSensitive(encoderSensEnumStored);
	}
	*/
	private void on_radiobutton_encoder_analyze_side_toggled (object obj, EventArgs args) {
		hbox_encoder_analyze_curve_num.Visible=false;
		hbox_combo_encoder_analyze_curve_num_combo.Visible = false;
		hbox_combo_encoder_analyze_cross.Visible=false;
		hbox_encoder_analyze_mean_or_max.Visible=false;
		encoderAnalysis="side";
		
		//together, mandatory
		hbox_encoder_analyze_eccon.Visible=false;
		radiobutton_encoder_eccon_together.Active = true;

		encoderButtonsSensitive(encoderSensEnumStored);
	}
	private void on_radiobutton_encoder_analyze_powerbars_toggled (object obj, EventArgs args) {
		hbox_encoder_analyze_curve_num.Visible=false;
		hbox_combo_encoder_analyze_curve_num_combo.Visible = false;
		hbox_combo_encoder_analyze_cross.Visible=false;
		hbox_encoder_analyze_mean_or_max.Visible=false;
		encoderAnalysis="powerBars";
		
		hbox_encoder_analyze_eccon.Visible=true;
		label_encoder_analyze_side_max.Visible = false;

		encoderButtonsSensitive(encoderSensEnumStored);
	}
	
	private void on_radiobutton_encoder_analyze_cross_toggled (object obj, EventArgs args) {
		hbox_encoder_analyze_curve_num.Visible=false;
		hbox_combo_encoder_analyze_curve_num_combo.Visible = false;
		hbox_combo_encoder_analyze_cross.Visible=true;
		hbox_encoder_analyze_mean_or_max.Visible=true;
		encoderAnalysis="cross";
		
		hbox_encoder_analyze_eccon.Visible=false;
		label_encoder_analyze_side_max.Visible = false;

		encoderButtonsSensitive(encoderSensEnumStored);
	}
	

	private bool curvesNumOkToSideCompare() {
		if(radiobutton_encoder_analyze_data_current_signal.Active &&
			UtilGtk.CountRows(encoderListStore) <= 12)
			return true;
		else if(radiobutton_encoder_analyze_data_user_curves.Active &&
				Convert.ToInt32(label_encoder_user_curves_active_num.Text) <= 12)
			return true;

		return false;
	}

	private string findMass(bool includePerson) {
		double mass = spin_encoder_extra_weight.Value;
		if(includePerson) {
			//TODO: maybe better have a currentEncoderExercise global variable
			int exPBodyWeight = Convert.ToInt32(
					Util.FindOnArray(':', 2, 3, UtilGtk.ComboGetActive(combo_encoder_exercise), 
					encoderExercisesTranslationAndBodyPWeight) );
			mass += currentPersonSession.Weight * exPBodyWeight / 100.0;
		}

		return Util.ConvertToPoint(mass); //R decimal: '.'
	}

	//TODO: check all this	
	private string findEccon(bool forceEcconSeparated) {	
		if(Util.FindOnArray(':',1,0,UtilGtk.ComboGetActive(combo_encoder_eccon),
					encoderEcconTranslation) == "Concentric") 
			return "c";
		else {
			if(forceEcconSeparated || ! radiobutton_encoder_eccon_together.Active)
				return "ecS";
			else 
				return "ec";
		}
	}
	
	/* encoder exercise stuff */
	
	
	string [] encoderExercisesTranslationAndBodyPWeight;
	string [] encoderEcconTranslation;
	string [] encoderLateralityTranslation;
	string [] encoderAnalyzeCrossTranslation;

	protected void createEncoderCombos() {
		//create combo exercises
		combo_encoder_exercise = ComboBox.NewText ();
		ArrayList encoderExercises = SqliteEncoder.SelectEncoderExercises(false, -1, false);
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
		combo_encoder_exercise.Active = UtilGtk.ComboMakeActive(combo_encoder_exercise, 
				Catalog.GetString(((EncoderExercise) encoderExercises[0]).name));
		
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
		
		//create combo analyze cross (variables)
		string [] comboAnalyzeCrossOptions = { 
			"Speed / Load", "Force / Load", "Power / Load", "Speed,Power / Load", "Force / Speed", "Power / Speed", "1RM Prediction"};
		string [] comboAnalyzeCrossOptionsTranslated = { 
			Catalog.GetString("Speed / Load"), Catalog.GetString("Force / Load"), 
			Catalog.GetString("Power / Load"), Catalog.GetString("Speed,Power / Load"), 
			Catalog.GetString("Force / Speed"), Catalog.GetString("Power / Speed") , 
			Catalog.GetString("1RM Prediction")
		};
		encoderAnalyzeCrossTranslation = new String [comboAnalyzeCrossOptions.Length];
		for(int j=0; j < 7 ; j++)
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
		/*
		if(Util.FindOnArray(':',1,0,UtilGtk.ComboGetActive(combo_encoder_eccon),
					encoderEcconTranslation) == "Concentric") {
			hbox_encoder_analyze_eccon.Sensitive=false;
		} else if(radiobutton_encoder_analyze_powerbars.Active) {
			hbox_encoder_analyze_eccon.Sensitive=true;
		}
		*/
	}

	void on_combo_encoder_analyze_cross_changed (object o, EventArgs args)
	{
		if(Util.FindOnArray(':',1,0,UtilGtk.ComboGetActive(combo_encoder_analyze_cross),
					encoderAnalyzeCrossTranslation) == "1RM Prediction") {
			radiobutton_encoder_analyze_mean.Active = true;
			radiobutton_encoder_analyze_max.Sensitive = false;
		} else
			radiobutton_encoder_analyze_max.Sensitive = true;
	}

	void on_button_encoder_exercise_info_clicked (object o, EventArgs args) 
	{
		int exerciseID = Convert.ToInt32(
				Util.FindOnArray(':', 2, 0, UtilGtk.ComboGetActive(combo_encoder_exercise), 
				encoderExercisesTranslationAndBodyPWeight) );	//exerciseID
		EncoderExercise ex = (EncoderExercise) SqliteEncoder.SelectEncoderExercises(false,exerciseID,false)[0];

		ArrayList bigArray = new ArrayList();

		ArrayList a1 = new ArrayList();
		ArrayList a2 = new ArrayList();
		ArrayList a3 = new ArrayList();
		ArrayList a4 = new ArrayList();

		//0 is the widgget to show; 1 is the editable; 2 id default value
		a1.Add(Constants.GenericWindowShow.ENTRY); a1.Add(false); a1.Add(ex.name);
		bigArray.Add(a1);

		a2.Add(Constants.GenericWindowShow.SPININT); a2.Add(false); a2.Add("");
		bigArray.Add(a2);
		
		a3.Add(Constants.GenericWindowShow.ENTRY2); a3.Add(false); a3.Add(ex.ressistance);
		bigArray.Add(a3);
		
		a4.Add(Constants.GenericWindowShow.ENTRY3); a4.Add(false); a4.Add(ex.description);
		bigArray.Add(a4);
		
		genericWin = GenericWindow.Show(false, Catalog.GetString("Encoder exercise name:"), bigArray);
		genericWin.LabelSpinInt = Catalog.GetString("Displaced body weight") + " (%)";
		genericWin.SetSpinRange(ex.percentBodyWeight, ex.percentBodyWeight); //done this because IsEditable does not affect the cursors
		genericWin.LabelEntry2 = Catalog.GetString("Resistance");
		genericWin.LabelEntry3 = Catalog.GetString("Description");
		genericWin.ShowButtonCancel(false);
		genericWin.SetButtonAcceptLabel(Catalog.GetString("Close"));
		genericWin.ShowNow();
	}

	void on_button_encoder_exercise_add_clicked (object o, EventArgs args) 
	{
		ArrayList bigArray = new ArrayList();

		ArrayList a1 = new ArrayList();
		ArrayList a2 = new ArrayList();
		ArrayList a3 = new ArrayList();
		ArrayList a4 = new ArrayList();

		//0 is the widgget to show; 1 is the editable; 2 id default value
		a1.Add(Constants.GenericWindowShow.ENTRY); a1.Add(true); a1.Add("");
		bigArray.Add(a1);

		a2.Add(Constants.GenericWindowShow.SPININT); a2.Add(true); a2.Add("");
		bigArray.Add(a2);
		
		a3.Add(Constants.GenericWindowShow.ENTRY2); a3.Add(true); a3.Add("");
		bigArray.Add(a3);
		
		a4.Add(Constants.GenericWindowShow.ENTRY3); a4.Add(true); a4.Add("");
		bigArray.Add(a4);
		
		genericWin = GenericWindow.Show(false,	//don't show now
				Catalog.GetString("Write the name of the encoder exercise:"), bigArray);
		genericWin.LabelSpinInt = Catalog.GetString("Displaced body weight") + " (%)";
		genericWin.SetSpinRange(0, 100);
		genericWin.LabelEntry2 = Catalog.GetString("Ressitance");
		genericWin.LabelEntry3 = Catalog.GetString("Description");
		genericWin.SetButtonAcceptLabel(Catalog.GetString("Add"));
		
		genericWin.HideOnAccept = false;

		genericWin.Button_accept.Clicked += new EventHandler(on_button_encoder_exercise_add_accepted);
		genericWin.ShowNow();
	}
	
	void on_button_encoder_exercise_add_accepted (object o, EventArgs args) 
	{
		string name = Util.RemoveTildeAndColonAndDot(genericWin.EntrySelected);

		Log.WriteLine("Trying to insert: " + name);
		if(name == "")
			genericWin.SetLabelError(Catalog.GetString("Error: Missing name of exercise."));
		else if (Sqlite.Exists(Constants.EncoderExerciseTable, name))
			genericWin.SetLabelError(string.Format(Catalog.GetString(
							"Error: An exercise named '{0}' already exists."), name));
		else {
			SqliteEncoder.InsertExercise(false, name, genericWin.SpinIntSelected, 
					genericWin.Entry2Selected, genericWin.Entry3Selected);

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

			genericWin.Button_accept.Clicked -= new EventHandler(on_button_encoder_exercise_add_accepted);
			genericWin.HideAndNull();
			Log.WriteLine("done");
		}
	}


	/* TreeView stuff */	

	//returns curves num
	private int createTreeViewEncoder(string contents) {
		string [] columnsString = {
			Catalog.GetString("Curve") + "\n",
			Catalog.GetString("Start") + "\n (s)",
			Catalog.GetString("Duration") + "\n (s)",
			Catalog.GetString("Range") + "\n (cm)",
			Catalog.GetString("MeanSpeed") + "\n (m/s)",
			Catalog.GetString("MaxSpeed") + "\n (m/s)",
			Catalog.GetString("MeanPower") + "\n (W)",
			Catalog.GetString("PeakPower") + "\n (W)",
			Catalog.GetString("PeakPowerTime") + "\n (s)",
			Catalog.GetString("PeakPower/PPT") + "\n (W/s)"
		};

		encoderCurves = new ArrayList ();

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

				encoderCurves.Add (new EncoderCurve (cells[0], cells[1], cells[2], 
							cells[3], cells[4], cells[5], cells[6], 
							cells[7], cells[8], cells[9]));

			} while(true);
		}

		encoderListStore = new Gtk.ListStore (typeof (EncoderCurve));
		foreach (EncoderCurve curve in encoderCurves) {
			encoderListStore.AppendValues (curve);
		}

		treeview_encoder_curves.Model = encoderListStore;

		if(ecconLast == "c")
			treeview_encoder_curves.Selection.Mode = SelectionMode.Single;
		else
			treeview_encoder_curves.Selection.Mode = SelectionMode.Multiple;
				
		treeview_encoder_curves.HeadersVisible=true;

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
					aColumn.SetCellDataFunc (aCell, new Gtk.TreeCellDataFunc (RenderMeanPower));
					break;
				case 7:
					aColumn.SetCellDataFunc (aCell, new Gtk.TreeCellDataFunc (RenderPeakPower));
					break;
				case 8:
					aColumn.SetCellDataFunc (aCell, new Gtk.TreeCellDataFunc (RenderPeakPowerT));
					break;
				case 9:
					aColumn.SetCellDataFunc (aCell, new Gtk.TreeCellDataFunc (RenderPP_PPT));
					break;
			}
			

			treeview_encoder_curves.AppendColumn (aColumn);
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
			bool isEven = (Convert.ToInt32(curve.N) % 2 == 0); //check if it's even (in spanish "par")
			if(isEven)
				phase = "c";
				
			(cell as Gtk.CellRendererText).Text = 
				decimal.Truncate((Convert.ToInt32(curve.N) +1) /2).ToString() + phase;
		}
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
			String.Format(UtilGtk.TVNumPrint(curve.PP_PPT,6,1),curve.PP_PPT);
	}

	/* end of rendering cols */
	
	
	private string [] fixDecimals(string [] cells) {
		for(int i=1; i <= 3; i++)
			cells[i] = Util.TrimDecimals(Convert.ToDouble(Util.ChangeDecimalSeparator(cells[i])),1);
		for(int i=4; i <= 8; i++)
			cells[i] = Util.TrimDecimals(Convert.ToDouble(Util.ChangeDecimalSeparator(cells[i])),3);
		cells[9] = Util.TrimDecimals(Convert.ToDouble(Util.ChangeDecimalSeparator(cells[9])),1); //pp/ppt
		return cells;
	}
	
	//the bool is for ecc-concentric
	//there two rows are selected
	//if user clicks on 2n row, and bool is true, first row is the returned curve
	private EncoderCurve treeviewEncoderCurvesGetCurve(int row, bool onEccConTakeFirst) 
	{
		if(onEccConTakeFirst && ecconLast != "c") {
			bool isEven = (row % 2 == 0); //check if it's even (in spanish "par")
			if(isEven)
				row --;
		}

		TreeIter iter = new TreeIter();
		bool iterOk = encoderListStore.GetIterFirst(out iter);
		if(iterOk) {
			int count=1;
			do {
				if(count==row) 
					return (EncoderCurve) treeview_encoder_curves.Model.GetValue (iter, 0);
				count ++;
			} while (encoderListStore.IterNext (ref iter));
		}
		EncoderCurve curve = new EncoderCurve();
		return curve;
	}

	private int treeviewEncoderCurvesEventSelectedID() {
		TreeIter iter = new TreeIter();
		TreeModel myModel = treeview_encoder_curves.Model;
		
		if(ecconLast == "c") {
			if (treeview_encoder_curves.Selection.GetSelected (out myModel, out iter)) 
				return Convert.ToInt32(((EncoderCurve) (treeview_encoder_curves.Model.GetValue (iter, 0))).N); //this return an int, also in ec
		} else {
			int selectedLength = treeview_encoder_curves.Selection.GetSelectedRows().Length;
			if(selectedLength == 1 || selectedLength == 2) { 
				TreePath path = treeview_encoder_curves.Selection.GetSelectedRows()[0];
				myModel.GetIter(out iter, path);
				return Convert.ToInt32(((EncoderCurve) (treeview_encoder_curves.Model.GetValue (iter, 0))).N);
			}
		}
		return 0;
	}
	
	private void on_treeview_encoder_curves_cursor_changed (object o, EventArgs args) 
	{
		int lineNum = treeviewEncoderCurvesEventSelectedID();
		encoderButtonsSensitive(encoderSensEnum.DONENOSIGNAL);
		
		//on ecc-con select both lines
		if(ecconLast == "c") {
			if (lineNum > 0)
				encoderButtonsSensitive(encoderSensEnum.SELECTEDCURVE);
		} else {
			TreeIter iter = new TreeIter();

			treeview_encoder_curves.CursorChanged -= on_treeview_encoder_curves_cursor_changed; 

			if (treeview_encoder_curves.Selection.GetSelectedRows().Length == 1) 
			{
				treeview_encoder_curves.Selection.UnselectAll();

				//on even, select also previous row
				//on odd, select also next row
				treeview_encoder_curves.Model.GetIterFirst ( out iter ) ;
				bool isEven = (lineNum % 2 == 0); //check if it's even (in spanish "par")
				int start = lineNum;
				if(isEven) 
					start = lineNum-1;

				//select 1st row
				for(int i=1; i < start; i++)
					treeview_encoder_curves.Model.IterNext (ref iter);
				treeview_encoder_curves.Selection.SelectIter(iter);

				//select 2nd row
				treeview_encoder_curves.Model.IterNext (ref iter);
				treeview_encoder_curves.Selection.SelectIter(iter);
				
				if (treeview_encoder_curves.Selection.GetSelectedRows().Length == 2) 
					encoderButtonsSensitive(encoderSensEnum.SELECTEDCURVE);
			}
			treeview_encoder_curves.CursorChanged += on_treeview_encoder_curves_cursor_changed; 
		}
	}
	
	/* end of TreeView stuff */	

	/* sensitivity stuff */	
			
	//called when a person changes
	private void encoderPersonChanged() {
		ArrayList data = SqliteEncoder.Select(false, -1, currentPerson.UniqueID, currentSession.UniqueID, "curve");
		
		int activeCurvesNum = getActiveCurvesNum(data);
		label_encoder_user_curves_active_num.Text = activeCurvesNum.ToString();
		
		label_encoder_user_curves_all_num.Text = data.Count.ToString();
	
		if(radiobutton_encoder_analyze_data_current_signal.Active) {
			int rows = UtilGtk.CountRows(encoderListStore);
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
		} else {
			updateComboEncoderAnalyzeCurveNum(data, activeCurvesNum);	
		}
	
		encoderButtonsSensitive(encoderSensEnum.YESPERSON);
		treeviewEncoderRemoveColumns();
		image_encoder_capture.Sensitive = false;
		image_encoder_analyze.Sensitive = false;
	}

	private void encoderButtonsSensitive(encoderSensEnum option) {
		//columns
		//c0 button_encoder_capture, button_encoder_bells
		//c1 button_encoder_recalculate
		//c2 button_encoder_load_signal
		//c3 button_encoder_save_all_curves, button_encoder_export_all_curves,
		//	button_encoder_update_signal, 
		//	button_encoder_delete_signal, entry_encoder_signal_comment,
		//	and images: image_encoder_capture , image_encoder_analyze.Sensitive. Update: both NOT managed here
		//c4 button_encoder_delete_curve , button_encoder_save_curve, entry_encoder_curve_comment
		//c5 button_encoder_analyze
		//c6 button_encoder_analyze_data_show_user_curves
		//c7 button_cancel (on capture and analyze)

		//other dependencies
		//c5 True needs 
		//	(signal && treeviewEncoder has rows) || 
		//	(! radiobutton_encoder_analyze_data_current_signal.Active && user has curves))
		//c6 True needs ! radiobutton_encoder_analyze_data_current_signal.Active

		if(option != encoderSensEnum.PROCESSING)
			encoderSensEnumStored = option;
		
		//columns		 0  1  2  3  4  5  6  7
		int [] noSession = 	{0, 0, 0, 0, 0, 0, 0, 0};
		int [] noPerson = 	{0, 0, 0, 0, 0, 0, 0, 0};
		int [] yesPerson = 	{1, 0, 1, 0, 0, 1, 1, 0};
		int [] processing = 	{0, 0, 0, 0, 0, 0, 0, 1};
		int [] doneNoSignal = 	{1, 0, 1, 0, 0, 1, 1, 0};
		int [] doneYesSignal = 	{1, 1, 1, 1, 0, 1, 1, 0};
		int [] selectedCurve = 	{1, 1, 1, 1, 1, 1, 1, 0};
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
			case encoderSensEnum.PROCESSING:
				table = processing;
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
		button_encoder_bells.Sensitive = Util.IntToBool(table[0]);
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

		bool signal = radiobutton_encoder_analyze_data_current_signal.Active;

		bool analyze_sensitive = 
			(Util.IntToBool(table[5]) && 
			 (signal && UtilGtk.CountRows(encoderListStore) > 0 ||
			  (! signal && Convert.ToInt32(label_encoder_user_curves_all_num.Text) >0)));
		if(analyze_sensitive && radiobutton_encoder_analyze_side.Active) {
			analyze_sensitive = curvesNumOkToSideCompare();
			label_encoder_analyze_side_max.Visible = ! analyze_sensitive;
		} else
			label_encoder_analyze_side_max.Visible = false;
		button_encoder_analyze.Sensitive = analyze_sensitive;

		button_encoder_analyze_data_show_user_curves.Sensitive = 
			(Util.IntToBool(table[6]) && ! radiobutton_encoder_analyze_data_current_signal.Active);
		
		button_encoder_capture_cancel.Sensitive = Util.IntToBool(table[7]);
		button_encoder_analyze_cancel.Sensitive = Util.IntToBool(table[7]);
	}

	/* end of sensitivity stuff */	
			
	
	/* thread stuff */

	private void encoderThreadStart(encoderModes mode) {
		if(mode == encoderModes.CAPTURE || mode == encoderModes.RECALCULATE_OR_LOAD) {
			//image is inside (is smaller than) viewport
			image_encoder_width = UtilGtk.WidgetWidth(viewport_image_encoder_capture)-5; 
			image_encoder_height = UtilGtk.WidgetHeight(viewport_image_encoder_capture)-5;

//			encoder_pulsebar_capture.Text = Catalog.GetString("Please, wait.");
			treeview_encoder_curves.Sensitive = false;
			encoderThread = new Thread(new ThreadStart(encoderCreateCurvesGraphR));
			if(mode == encoderModes.CAPTURE)
				GLib.Idle.Add (new GLib.IdleHandler (pulseGTKEncoderCapture));
			else // mode == encoderModes.RECALCULATE_OR_LOAD
				GLib.Idle.Add (new GLib.IdleHandler (pulseGTKEncoderRecalculateOrLoad));
		} else { //encoderModes.ANALYZE
			//the -3 is because image is inside (is smaller than) viewport
			image_encoder_width = UtilGtk.WidgetWidth(viewport_image_encoder_analyze)-5; 
			image_encoder_height = UtilGtk.WidgetHeight(viewport_image_encoder_analyze)-5;

			encoder_pulsebar_analyze.Text = Catalog.GetString("Please, wait.");

		
			encoderThread = new Thread(new ThreadStart(analyze));
			GLib.Idle.Add (new GLib.IdleHandler (pulseGTKEncoderAnalyze));
		}
		encoderButtonsSensitive(encoderSensEnum.PROCESSING);
		encoderThread.Start(); 
	}
	
	private bool pulseGTKEncoderCapture ()
	{
		if(! encoderThread.IsAlive || encoderProcessCancel) {
			if(encoderProcessCancel){
				Util.CancelRScript = true;
			}

			finishPulsebar(encoderModes.CAPTURE);
			Log.Write("dying");
			return false;
		}
		updatePulsebar(encoderModes.CAPTURE); //activity on pulsebar
		Thread.Sleep (50);
		Log.Write(encoderThread.ThreadState.ToString());
		return true;
	}
	
	private bool pulseGTKEncoderRecalculateOrLoad ()
	{
		if(! encoderThread.IsAlive || encoderProcessCancel) {
			if(encoderProcessCancel){
				Util.CancelRScript = true;
			}

			finishPulsebar(encoderModes.RECALCULATE_OR_LOAD);
			Log.Write("dying");
			return false;
		}
		updatePulsebar(encoderModes.CAPTURE); //activity on pulsebar
		Thread.Sleep (50);
		Log.Write(encoderThread.ThreadState.ToString());
		return true;
	}
	
	private bool pulseGTKEncoderAnalyze ()
	{
		if(! encoderThread.IsAlive || encoderProcessCancel) {
			if(encoderProcessCancel){
				Util.CancelRScript = true;
			}

			finishPulsebar(encoderModes.ANALYZE);
			Log.Write("dying");
			return false;
		}
		updatePulsebar(encoderModes.ANALYZE); //activity on pulsebar
		Thread.Sleep (50);
		Log.Write(encoderThread.ThreadState.ToString());
		return true;
	}
	
	private void updatePulsebar (encoderModes mode) {
		string contents = Catalog.GetString("Please, wait.");
		if(Util.FileExists(Util.GetEncoderStatusTempFileName()))
			contents = Util.ReadFile(Util.GetEncoderStatusTempFileName(), true);

		if(mode == encoderModes.CAPTURE || mode == encoderModes.RECALCULATE_OR_LOAD) {
			encoder_pulsebar_capture.Pulse();
			encoder_pulsebar_capture.Text = contents;
		} else {
			encoder_pulsebar_analyze.Pulse();
			encoder_pulsebar_analyze.Text = contents;
		}
	}
	
	private void finishPulsebar(encoderModes mode) {
		if(mode == encoderModes.CAPTURE || mode == encoderModes.RECALCULATE_OR_LOAD) {
			if(encoderProcessCancel) {
				encoderProcessCancel = false;
				encoderButtonsSensitive(encoderSensEnum.DONEYESSIGNAL);
				encoder_pulsebar_capture.Text = Catalog.GetString("Cancelled");
			} else {
				Pixbuf pixbuf = new Pixbuf (Util.GetEncoderGraphTempFileName()); //from a file
				image_encoder_capture.Pixbuf = pixbuf;
				encoderUpdateTreeView();
				image_encoder_capture.Sensitive = true;
		
				//autosave signal (but not in recalculate or load)
				if(mode == encoderModes.CAPTURE)
					encoder_pulsebar_capture.Text = encoderSaveSignalOrCurve("signal", 0);
				else
					encoder_pulsebar_capture.Text = "";
			}

			encoder_pulsebar_capture.Fraction = 1;

		} else { //ANALYZE
			if(encoderProcessCancel) {
				encoderProcessCancel = false;
				encoder_pulsebar_analyze.Text = Catalog.GetString("Cancelled");
			} else {
				//TODO pensar en si s'ha de fer 1er amb mida petita i despres amb gran (en el zoom),
				//o si es una sola i fa alguna edicio
				Pixbuf pixbuf = new Pixbuf (Util.GetEncoderGraphTempFileName()); //from a file
				image_encoder_analyze.Pixbuf = pixbuf;
				encoder_pulsebar_analyze.Text = "";
			}

			encoder_pulsebar_analyze.Fraction = 1;
			encoderButtonsSensitive(encoderSensEnumStored);
			image_encoder_analyze.Sensitive = true;
		}

		treeview_encoder_curves.Sensitive = true;
		Util.FileDelete(Util.GetEncoderStatusTempFileName());
	}
	
	/* end of thread stuff */
	
}	

