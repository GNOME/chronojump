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

	[Widget] Gtk.Button button_encoder_capture;
	[Widget] Gtk.Button button_encoder_recalculate;
	[Widget] Gtk.Button button_encoder_load_stream;
	[Widget] Gtk.Viewport viewport_image_encoder_capture;
	[Widget] Gtk.Image image_encoder_bell;
	[Widget] Gtk.SpinButton spin_encoder_capture_time;
	[Widget] Gtk.SpinButton spin_encoder_capture_min_height;
	[Widget] Gtk.Image image_encoder_capture;
	[Widget] Gtk.TreeView treeview_encoder_curves;
	[Widget] Gtk.ProgressBar encoder_pulsebar_capture;
	[Widget] Gtk.Label label_encoder_capture_comment;
	[Widget] Gtk.Entry entry_encoder_capture_comment;
	[Widget] Gtk.Button button_encoder_delete_curve;
	[Widget] Gtk.Button button_encoder_save_curve;
	[Widget] Gtk.Button button_encoder_save_all_curves;
	[Widget] Gtk.Button button_encoder_save_stream;
	
	[Widget] Gtk.Box hbox_combo_encoder_exercise;
	[Widget] Gtk.ComboBox combo_encoder_exercise;
	[Widget] Gtk.Box hbox_combo_encoder_eccon;
	[Widget] Gtk.ComboBox combo_encoder_eccon;
	[Widget] Gtk.Box hbox_combo_encoder_laterality;
	[Widget] Gtk.ComboBox combo_encoder_laterality;

	
	[Widget] Gtk.Button button_encoder_analyze;
	[Widget] Gtk.RadioButton radiobutton_encoder_analyze_data_current_stream;
	[Widget] Gtk.RadioButton radiobutton_encoder_analyze_data_user_curves;
	[Widget] Gtk.Button button_encoder_analyze_data_show_user_curves;
	[Widget] Gtk.RadioButton radiobutton_encoder_analyze_powerbars;
	[Widget] Gtk.RadioButton radiobutton_encoder_analyze_single;
	[Widget] Gtk.RadioButton radiobutton_encoder_analyze_side;
	[Widget] Gtk.RadioButton radiobutton_encoder_analyze_superpose;
	[Widget] Gtk.Label label_encoder_analyze_eccon;
	[Widget] Gtk.Box hbox_encoder_analyze_eccon;
	[Widget] Gtk.RadioButton radiobutton_encoder_eccon_both;
	[Widget] Gtk.RadioButton radiobutton_encoder_eccon_together;
	[Widget] Gtk.SpinButton spin_encoder_analyze_curve_num;
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
	private string encoderStreamUniqueID;
	enum encoderModes { CAPTURE, ANALYZE }
	
	//TODO: auto close capturing window

	//TODO: Put person name in graph (at title,with small separation, or inside graph at topright) (if we click on another person on treeview person, we need to know wich person was last generated graph)
	//TODO: when change person: unsensitive: recalculate, capture graph, treeview capture, buttons caputre on bottom, analyze button
	//TODO: when selected user curves, Single curve spinbutton have to grow (done). Also do it if person changes (pending)
	//TODO: laterality have to be shown on treeviews: stream and curve. also check that is correct in database

	//TODO: put chronopic detection in a generic place. Done But:
	//TODO: solve the problem of connecting two different chronopics
	//
	//TODO:put zoom,unzoom (at side of delete curve)  in capture curves (for every curve)
	//TODO: treeview on analyze
	
	//to analyze: user has to select: session, athlete, exercise, 
	//TODO: single curve, and side, checkbox to show1 param, 2 or three
	//TODO: powerbars with checkbox to show1 param, 2 or three
	//TODO: on capture (quasi-realtime), show powerbars or curves or both
	//

	
	private void encoderInitializeStuff() {
		encoder_pulsebar_capture.Fraction = 1;
		encoder_pulsebar_capture.Text = "";
		encoder_pulsebar_analyze.Fraction = 1;
		encoder_pulsebar_analyze.Text = "";
		
		//the glade cursor_changed does not work on mono 1.2.5 windows
		treeview_encoder_curves.CursorChanged += on_treeview_encoder_curves_cursor_changed; 
		sensitiveEncoderRowButtons(false);
		createEncoderCombos();
		sensitiveEncoderGlobalButtons(false);
		spin_encoder_analyze_curve_num.SetRange(1,1);
	}

	//TODO: garantir path windows	
	void on_button_encoder_capture_clicked (object o, EventArgs args) 
	{
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

		//capture data
		EncoderParams ep = new EncoderParams(
				(int) spin_encoder_capture_time.Value, 
				(int) spin_encoder_capture_min_height.Value, 
				Convert.ToInt32(
					Util.FindOnArray(':', 2, 3, UtilGtk.ComboGetActive(combo_encoder_exercise), 
					encoderExercisesTranslationAndBodyPWeight) ),
				findMass(true),
				Util.ConvertToPoint((double) spin_encoder_smooth.Value), //R decimal: '.'
				findEccon(true),					//force ecS (ecc-conc separated)
				heightHigherCondition, heightLowerCondition,
				meanSpeedHigherCondition, meanSpeedLowerCondition,
				maxSpeedHigherCondition, maxSpeedLowerCondition,
				powerHigherCondition, powerLowerCondition,
				peakPowerHigherCondition, peakPowerLowerCondition
				); 

		EncoderStruct es = new EncoderStruct(
				"",					//no data input
				"",					//no graph ouptut
				Util.GetEncoderDataTempFileName(), "", ep);				

		Util.RunPythonEncoder(Constants.EncoderScriptCapture, es, true);

		encoderTimeStamp = UtilDate.ToFile(DateTime.Now);
		encoderStreamUniqueID = "-1"; //mark to know that there's no ID for this until it's saved on database

		encoderThreadStart(encoderModes.CAPTURE);
	}
		
	void on_button_encoder_recalculate_clicked (object o, EventArgs args) 
	{
		if (File.Exists(Util.GetEncoderDataTempFileName()))
			encoderThreadStart(encoderModes.CAPTURE);
		else
			encoder_pulsebar_capture.Text = Catalog.GetString("Missing data.");
	}
	
	private void encoderUpdateTreeView()
	{
		string contents = Util.ReadFile(Util.GetEncoderCurvesTempFileName());
		if (contents == null) {
			//TODO: no data: make some of the gui unsensitive ??
			sensitiveEncoderGlobalButtons(false);
		} else {
			removeColumns();
			int curvesNum = createTreeViewEncoder(contents);
			if(Util.FindOnArray(':',1,0,UtilGtk.ComboGetActive(combo_encoder_eccon),
					encoderEcconTranslation) != "Concentric") 
				curvesNum = curvesNum / 2;
			spin_encoder_analyze_curve_num.SetRange(1,curvesNum);
			sensitiveEncoderGlobalButtons(true);
		}
	}
	
	private void removeColumns() {
		Gtk.TreeViewColumn [] myColumns = treeview_encoder_curves.Columns;
		foreach (Gtk.TreeViewColumn column in myColumns) 
			treeview_encoder_curves.RemoveColumn (column);
	}


	//this is called by non gtk thread. Don't do gtk stuff here
	//I suppose reading gtk is ok, changing will be the problem
	private void encoderCreateCurvesGraphR() 
	{
		EncoderParams ep = new EncoderParams(
				(int) spin_encoder_capture_min_height.Value, 
				Convert.ToInt32(
					Util.FindOnArray(':', 2, 3, UtilGtk.ComboGetActive(combo_encoder_exercise), 
					encoderExercisesTranslationAndBodyPWeight) ),
				findMass(true),
				findEccon(true),					//force ecS (ecc-conc separated)
				"curves",
				Util.ConvertToPoint((double) spin_encoder_smooth.Value), //R decimal: '.'
			       	0, 			//curve is not used here
				image_encoder_width, image_encoder_height); 

		EncoderStruct es = new EncoderStruct(
				Util.GetEncoderDataTempFileName(), 
				Util.GetEncoderGraphTempFileName(),
				Util.GetEncoderCurvesTempFileName(), 
				"NULL", ep);
		
		Util.RunPythonEncoder(Constants.EncoderScriptGraphCall, es,false);

		//store this to show 1,2,3,4,... or 1e,1c,2e,2c,... in RenderN
		//if is not stored, it can change when changed eccon radiobutton on cursor is in treeview
		ecconLast = findEccon(false);
	}
	
	void on_button_encoder_analyze_data_show_user_curves_clicked (object o, EventArgs args) 
	{
		ArrayList data = SqliteEncoder.Select(false, -1, currentPerson.UniqueID, currentSession.UniqueID, "curve");

		ArrayList dataPrint = new ArrayList();
		foreach(EncoderSQL es in data) {
			dataPrint.Add(es.ToStringArray());
		}
		
		string [] columnsString = {
			Catalog.GetString("ID"),
			Catalog.GetString("Exercise"),
			Catalog.GetString("Contraction"),
			Catalog.GetString("Extra weight"),
			Catalog.GetString("Date"),
			Catalog.GetString("Comment")
		};

		genericWin = GenericWindow.Show(
				string.Format(Catalog.GetString("Saved curves of athlete {0} on this session."), 
					currentPerson.Name), Constants.GenericWindowShow.TREEVIEW);

		genericWin.SetTreeview(columnsString, dataPrint);
		genericWin.ShowButtonCancel(false);
		genericWin.SetButtonAcceptSensitive(true);
		genericWin.SetButtonAcceptLabel(Catalog.GetString("Close"));
	}
		
	void on_button_encoder_load_stream_clicked (object o, EventArgs args) 
	{
		ArrayList data = SqliteEncoder.Select(false, -1, currentPerson.UniqueID, currentSession.UniqueID, "stream");

		ArrayList dataPrint = new ArrayList();
		foreach(EncoderSQL es in data) {
			dataPrint.Add(es.ToStringArray());
		}
		
		string [] columnsString = {
			Catalog.GetString("ID"),
			Catalog.GetString("Exercise"),
			Catalog.GetString("Contraction"),
			Catalog.GetString("Extra weight"),
			Catalog.GetString("Date"),
			Catalog.GetString("Comment")
		};

		genericWin = GenericWindow.Show(
				string.Format(Catalog.GetString("Select stream of athlete {0} on this session."), 
					currentPerson.Name), Constants.GenericWindowShow.TREEVIEW);

		genericWin.SetTreeview(columnsString, dataPrint);
		genericWin.SetButtonAcceptLabel(Catalog.GetString("Load"));
		genericWin.SetButtonAcceptSensitive(false);
		genericWin.Button_accept.Clicked += new EventHandler(on_encoder_load_stream_accepted);
	}
	
	protected void on_encoder_load_stream_accepted (object o, EventArgs args)
	{
		genericWin.Button_accept.Clicked -= new EventHandler(on_encoder_load_stream_accepted);
		int uniqueID = genericWin.TreeviewSelectedRowID();

		ArrayList data = SqliteEncoder.Select(false, uniqueID, 
				currentPerson.UniqueID, currentSession.UniqueID, "stream");

		foreach(EncoderSQL es in data) {	//it will run only one time
			Util.CopyEncoderDataToTemp(es.url, es.filename);
			UtilGtk.ComboMakeActive(combo_encoder_exercise, es.exerciseName);
			UtilGtk.ComboMakeActive(combo_encoder_eccon, es.ecconLong);
			UtilGtk.ComboMakeActive(combo_encoder_laterality, es.laterality);
			spin_encoder_extra_weight.Value = Convert.ToInt32(es.extraWeight);

			spin_encoder_capture_min_height.Value = es.minHeight;
			spin_encoder_smooth.Value = es.smooth;
			encoderTimeStamp = es.GetDate(false); 
			encoderStreamUniqueID = es.uniqueID;
		}
	
		//force a recalculate
		on_button_encoder_recalculate_clicked (o, args); 
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
			Util.EncoderDeleteCurve(Util.GetEncoderDataTempFileName(), curveStart, duration);
		}
		//force a recalculate
		on_button_encoder_recalculate_clicked (o, args); 
	}

	void on_button_encoder_save_clicked (object o, EventArgs args) 
	{
		Gtk.Button button = (Gtk.Button) o;
		if(button == button_encoder_save_curve) {
			int selectedID = treeviewEncoderCurvesEventSelectedID();
			encoder_pulsebar_capture.Text = encoderSaveStreamOrCurve("curve", selectedID);
		} else if(button == button_encoder_save_all_curves) 
			for(int i=1; i <= UtilGtk.CountRows(encoderListStore); i++)
				encoder_pulsebar_capture.Text = encoderSaveStreamOrCurve("allCurves", i);
		else 	//(button == button_encoder_save_stream) 
			encoder_pulsebar_capture.Text = encoderSaveStreamOrCurve("stream", 0);

	}

	string encoderSaveStreamOrCurve (string mode, int selectedID) 
	{
		//mode is different than type. 
		//mode can be curve, allCurves or stream
		//type is to print on db at type column: curve or stream + (bar or jump)
		string streamOrCurve = "";
		string feedback = "";
		string fileSaved = "";
		string path = "";

		if(mode == "curve") {
			streamOrCurve = "curve";
			decimal curveNum = (decimal) treeviewEncoderCurvesEventSelectedID(); //on c and ec: 1,2,3,4,...
			if(ecconLast != "c")
				curveNum = decimal.Truncate((curveNum +1) /2); //1,1,2,2,...
			feedback = string.Format(Catalog.GetString("Curve {0} saved"), curveNum);
		} else if(mode == "allCurves") {
			streamOrCurve = "curve";
			feedback = Catalog.GetString("All curves saved");
		} else 	//mode == "stream"
			streamOrCurve = "stream";
		
		string desc = Util.RemoveTildeAndColonAndDot(entry_encoder_capture_comment.Text.ToString());
		//Log.WriteLine(desc);

		if(mode == "curve" || mode == "allCurves") {
			EncoderCurve curve = treeviewEncoderCurvesGetCurve(selectedID,true);

			//some start at ,5 because of the spline filtering
			int curveStart = Convert.ToInt32(decimal.Truncate(Convert.ToDecimal(curve.Start)));

			int duration = Convert.ToInt32(decimal.Truncate(Convert.ToDecimal(curve.Duration)));
			if(ecconLast != "c") {
				EncoderCurve curveNext = treeviewEncoderCurvesGetCurve(selectedID+1,false);
				duration += Convert.ToInt32(decimal.Truncate(Convert.ToDecimal(curveNext.Duration)));
			}

			Log.WriteLine(curveStart + "->" + duration);
			int curveIDMax = Sqlite.Max(Constants.EncoderTable, "uniqueID", false);
			fileSaved = Util.EncoderSaveCurve(Util.GetEncoderDataTempFileName(), curveStart, duration,
					currentSession.UniqueID, currentPerson.UniqueID, 
					currentPerson.Name, encoderTimeStamp, curveIDMax);
			path = Util.GetEncoderSessionDataCurveDir(currentSession.UniqueID);
		} else { //stream
			fileSaved = Util.CopyTempToEncoderData (currentSession.UniqueID, currentPerson.UniqueID, 
					currentPerson.Name, encoderTimeStamp);
			path = Util.GetEncoderSessionDataStreamDir(currentSession.UniqueID);
		}

		string myID = "-1";	
		if(mode == "stream")
			myID = encoderStreamUniqueID;

		EncoderSQL eSQL = new EncoderSQL(
				myID, 
				currentPerson.UniqueID, currentSession.UniqueID, 
				Convert.ToInt32(
					Util.FindOnArray(':', 2, 0, UtilGtk.ComboGetActive(combo_encoder_exercise), 
					encoderExercisesTranslationAndBodyPWeight) ),	//exerciseID
				findEccon(true), 	//force ecS (ecc-conc separated)
				UtilGtk.ComboGetActive(combo_encoder_laterality),
				findMass(false),	//when save on sql, do not include person weight
				streamOrCurve,
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

		
		//if is a stream that we just loaded, then don't insert, do an update
		//we know it because encoderUniqueID is != than "-1" if we loaded something from database
		//on curves, always insert, because it can be done with different smoothing, different params
		if(myID == "-1") {
			myID = SqliteEncoder.Insert(false, eSQL).ToString(); //Adding on SQL
			if(mode == "stream") {
				encoderStreamUniqueID = myID;
				feedback = Catalog.GetString("Stream saved");
			}
		}
		else {
			//only stream is updated
			SqliteEncoder.Update(false, eSQL); //Adding on SQL
			feedback = Catalog.GetString("Stream updated");
		}
		
		return feedback;
	}


	//TODO: garantir path windows	
	private void on_button_encoder_analyze_clicked (object o, EventArgs args) 
	{
		//if userCurves and no data, return
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

		if(radiobutton_encoder_analyze_data_user_curves.Active) {
			string myEccon = "ec";
			if(! radiobutton_encoder_eccon_together.Active)
				myEccon = "ecS";
			int myCurveNum = -1;
			if(encoderAnalysis == "single")
				myCurveNum = (int) spin_encoder_analyze_curve_num.Value;

			//-1 because data will be different on any curve
			ep = new EncoderParams(
					-1, 
					Convert.ToInt32(
						Util.FindOnArray(':', 2, 3, UtilGtk.ComboGetActive(combo_encoder_exercise), 
						encoderExercisesTranslationAndBodyPWeight) ),
					"-1",			//mass
					myEccon,	//this decides if analysis will be together or separated
					encoderAnalysis,
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
			writer.WriteLine("exerciseName,mass,smoothingOne,dateTime,fullURL,eccon");
			foreach(EncoderSQL eSQL in data) {
				double mass = Convert.ToDouble(eSQL.extraWeight); //TODO: future problem if this has '%'
				EncoderExercise ex = (EncoderExercise) SqliteEncoder.SelectEncoderExercises(eSQL.exerciseID)[0];
				mass += bodyMass * ex.percentBodyWeight / 100.0;

				writer.WriteLine(ex.name + "," + mass.ToString() + "," + 
						Util.ConvertToPoint(eSQL.smooth) + "," + eSQL.GetDate(true) + "," + 
						eSQL.url + Path.DirectorySeparatorChar + eSQL.filename + "," +
						eSQL.eccon	//this is the eccon of every curve
						);
			}
			writer.Flush();
			((IDisposable)writer).Dispose();
		} else {
			ep = new EncoderParams(
					(int) spin_encoder_capture_min_height.Value, 
					Convert.ToInt32(
						Util.FindOnArray(':', 2, 3, UtilGtk.ComboGetActive(combo_encoder_exercise), 
						encoderExercisesTranslationAndBodyPWeight) ),
					findMass(true),
					findEccon(false),		//do not force ecS (ecc-conc separated)
					encoderAnalysis,
					Util.ConvertToPoint((double) spin_encoder_smooth.Value), //R decimal: '.'
					(int) spin_encoder_analyze_curve_num.Value, 
					image_encoder_width,
					image_encoder_height); 
			
			dataFileName = Util.GetEncoderDataTempFileName();
		}

		EncoderStruct encoderStruct = new EncoderStruct(
				dataFileName, 
				Util.GetEncoderGraphTempFileName(),
				"NULL", "NULL", ep);		//no data ouptut

		Util.RunPythonEncoder(Constants.EncoderScriptGraphCall, encoderStruct, false);
	}
	
	private void on_radiobutton_encoder_analyze_data_current_stream_toggled (object obj, EventArgs args) {
		button_encoder_analyze.Sensitive = encoderTimeStamp != null;
		button_encoder_analyze_data_show_user_curves.Sensitive = false;

		spin_encoder_analyze_curve_num.SetRange(1, UtilGtk.CountRows(encoderListStore));
	}
	private void on_radiobutton_encoder_analyze_data_user_curves_toggled (object obj, EventArgs args) {
		button_encoder_analyze.Sensitive = currentPerson != null;
		button_encoder_analyze_data_show_user_curves.Sensitive = currentPerson != null;
		
		ArrayList data = SqliteEncoder.Select(false, -1, currentPerson.UniqueID, currentSession.UniqueID, "curve");
		spin_encoder_analyze_curve_num.SetRange(1, data.Count);
	}

	//show curve_num only on simple and superpose
	private void on_radiobutton_encoder_analyze_single_toggled (object obj, EventArgs args) {
		spin_encoder_analyze_curve_num.Sensitive=true;
		encoderAnalysis="single";
		//together, mandatory
		label_encoder_analyze_eccon.Sensitive=false;
		hbox_encoder_analyze_eccon.Sensitive=false;
		radiobutton_encoder_eccon_together.Active = true;
	}

	private void on_radiobutton_encoder_analyze_superpose_toggled (object obj, EventArgs args) {
		spin_encoder_analyze_curve_num.Sensitive=true;
		encoderAnalysis="superpose";
		//together, mandatory
		label_encoder_analyze_eccon.Sensitive=false;
		hbox_encoder_analyze_eccon.Sensitive=false;
		radiobutton_encoder_eccon_together.Active = true;
	}
	private void on_radiobutton_encoder_analyze_side_toggled (object obj, EventArgs args) {
		spin_encoder_analyze_curve_num.Sensitive=false;
		encoderAnalysis="side";
		//together, mandatory
		label_encoder_analyze_eccon.Sensitive=false;
		hbox_encoder_analyze_eccon.Sensitive=false;
		radiobutton_encoder_eccon_together.Active = true;
	}
	private void on_radiobutton_encoder_analyze_powerbars_toggled (object obj, EventArgs args) {
		spin_encoder_analyze_curve_num.Sensitive=false;
		encoderAnalysis="powerBars";
		//can select together or separated
		//if(Util.FindOnArray(':',1,0,UtilGtk.ComboGetActive(combo_encoder_eccon),
		//			encoderEcconTranslation) != "Concentric") {
			label_encoder_analyze_eccon.Sensitive=true;
			hbox_encoder_analyze_eccon.Sensitive=true;
		//}
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

	protected void createEncoderCombos() {
		//create combo exercises
		combo_encoder_exercise = ComboBox.NewText ();
		ArrayList encoderExercises = SqliteEncoder.SelectEncoderExercises(-1);
		encoderExercisesTranslationAndBodyPWeight = new String [encoderExercises.Count];
		string [] exerciseNamesToCombo = new String [encoderExercises.Count];
		int i =0;
		foreach(EncoderExercise ex in encoderExercises) {
			encoderExercisesTranslationAndBodyPWeight[i] = 
				ex.uniqueID + ":" + ex.name + ":" + Catalog.GetString(ex.name) + ":" + ex.percentBodyWeight;
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
		UtilGtk.ComboUpdate(combo_encoder_laterality, comboLateralityOptions, "");
		combo_encoder_laterality.Active = UtilGtk.ComboMakeActive(combo_encoder_laterality, 
				Catalog.GetString(comboLateralityOptions[0]));
		combo_encoder_laterality.Changed += new EventHandler (on_combo_encoder_laterality_changed);
		
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
	}

	void on_combo_encoder_exercise_changed (object o, EventArgs args) 
	{
		//TODO
	}

	void on_combo_encoder_eccon_changed (object o, EventArgs args) 
	{
		/*
		if(Util.FindOnArray(':',1,0,UtilGtk.ComboGetActive(combo_encoder_eccon),
					encoderEcconTranslation) == "Concentric") {
			label_encoder_analyze_eccon.Sensitive=false;
			hbox_encoder_analyze_eccon.Sensitive=false;
		} else if(radiobutton_encoder_analyze_powerbars.Active) {
			label_encoder_analyze_eccon.Sensitive=true;
			hbox_encoder_analyze_eccon.Sensitive=true;
		}
		*/
	}

	void on_combo_encoder_laterality_changed (object o, EventArgs args) 
	{
		//TODO
	}

	void on_button_encoder_exercise_info_clicked (object o, EventArgs args) 
	{
		//TODO
	}

	void on_button_encoder_exercise_add_clicked (object o, EventArgs args) 
	{
		//TODO
	}


	/* TreeView stuff */	

	//returns curves num
	private int createTreeViewEncoder(string contents) {
		string [] columnsString = {
			Catalog.GetString("Curve") + "\n",
			Catalog.GetString("Start") + "\n (s)",
			Catalog.GetString("Duration") + "\n (s)",
			Catalog.GetString("Height") + "\n (cm)",
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
		sensitiveEncoderRowButtons(false);
		
		//on ecc-con select both lines
		if(ecconLast == "c") {
			if (lineNum > 0)
				sensitiveEncoderRowButtons(true);
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
					sensitiveEncoderRowButtons(true);
			}
			treeview_encoder_curves.CursorChanged += on_treeview_encoder_curves_cursor_changed; 
		}
	}

	private void sensitiveEncoderGlobalButtons(bool sensitive) {
		label_encoder_capture_comment.Sensitive = sensitive;
		entry_encoder_capture_comment.Sensitive = sensitive;
		button_encoder_save_stream.Sensitive = sensitive;
		button_encoder_analyze.Sensitive = sensitive;
	}

	private void sensitiveEncoderRowButtons(bool sensitive) {
		button_encoder_delete_curve.Sensitive = sensitive;
		button_encoder_save_curve.Sensitive = sensitive;
	}
	
	/* end of TreeView stuff */	

	/* thread stuff */

	private void encoderThreadStart(encoderModes mode) {
		if(mode == encoderModes.CAPTURE) {
			//image is inside (is smaller than) viewport
			image_encoder_width = UtilGtk.WidgetWidth(viewport_image_encoder_capture)-3; 
			image_encoder_height = UtilGtk.WidgetHeight(viewport_image_encoder_capture)-3;

			encoder_pulsebar_capture.Text = Catalog.GetString("Please, wait.");
			treeview_encoder_curves.Sensitive = false;
			encoderThread = new Thread(new ThreadStart(encoderCreateCurvesGraphR));
			GLib.Idle.Add (new GLib.IdleHandler (pulseGTKEncoderCapture));
		} else {
			//the -3 is because image is inside (is smaller than) viewport
			image_encoder_width = UtilGtk.WidgetWidth(viewport_image_encoder_analyze)-3; 
			image_encoder_height = UtilGtk.WidgetHeight(viewport_image_encoder_analyze)-3;

			encoder_pulsebar_analyze.Text = Catalog.GetString("Please, wait.");

			encoderThread = new Thread(new ThreadStart(analyze));
			GLib.Idle.Add (new GLib.IdleHandler (pulseGTKEncoderAnalyze));
		}
		encoderThread.Start(); 
	}
	
	private bool pulseGTKEncoderCapture ()
	{
		if(! encoderThread.IsAlive) {
			finishPulsebar(encoderModes.CAPTURE);
			sensitiveEncoderRowButtons(false);
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
		if(! encoderThread.IsAlive) {
			finishPulsebar(encoderModes.ANALYZE);
			sensitiveEncoderRowButtons(false);
			Log.Write("dying");
			return false;
		}
		updatePulsebar(encoderModes.ANALYZE); //activity on pulsebar
		Thread.Sleep (50);
		Log.Write(encoderThread.ThreadState.ToString());
		return true;
	}
	
	private void updatePulsebar (encoderModes mode) {
		if(mode == encoderModes.CAPTURE) 
			encoder_pulsebar_capture.Pulse();
		else
			encoder_pulsebar_analyze.Pulse();
	}
	
	private void finishPulsebar(encoderModes mode) {
		if(mode == encoderModes.CAPTURE) {
			encoder_pulsebar_capture.Fraction = 1;
			encoder_pulsebar_capture.Text = "";
			treeview_encoder_curves.Sensitive = true;
			
			Pixbuf pixbuf = new Pixbuf (Util.GetEncoderGraphTempFileName()); //from a file
			image_encoder_capture.Pixbuf = pixbuf;

			encoderUpdateTreeView();
		} else {
			encoder_pulsebar_analyze.Fraction = 1;
			encoder_pulsebar_analyze.Text = "";
			
			//TODO pensar en si s'ha de fer 1er amb mida petita i despres amb gran (en el zoom), o si es una sola i fa alguna edicio
			Pixbuf pixbuf = new Pixbuf (Util.GetEncoderGraphTempFileName()); //from a file
			image_encoder_analyze.Pixbuf = pixbuf;
		}
	}
	
	/* end of thread stuff */
	
}	

