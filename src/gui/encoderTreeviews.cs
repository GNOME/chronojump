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
 * Copyright (C) 2004-2022   Xavier de Blas <xaviblas@gmail.com>
 */

using System;
using System.IO; 
using System.IO.Ports;
using Gtk;
using Gdk;
//using Glade;
using System.Collections;
using System.Collections.Generic; //List<T>
using Mono.Unix;


public partial class ChronoJumpWindow 
{
	/* TreeView stuff */	

	//returns curves num
	//capture has single and multiple selection in order to save curves... Analyze only shows data.
	//TODO: do not create this for every repetition while we are executing. Just add the new row
	private int createTreeViewEncoderCapture(List<string> contents) 
	{
		int curvesCount = 0;
		bool headers = true;
		if(! encoderUpdateTreeViewWhileCapturing && capturingCsharp == encoderCaptureProcess.CAPTURING) {
			//just count how much curves and return
			foreach(string line in contents)
			{
				if(headers) {
					headers = false;
					continue;
				}
			
				if (line == null)
					break;
			
				curvesCount ++;
			}
			return curvesCount;
		}


		LogB.Debug("At createTreeViewEncoderCapture");

		bool showStartAndDuration = preferences.encoderShowStartAndDuration;

		string workString = "|" + Catalog.GetString("Work") + "|";
		if(preferences.encoderWorkKcal)
			workString += "\n (KCal)";
		else
			workString += "\n (J)";

		string [] columnsString = {
			Catalog.GetString("n") + "\n",
			Catalog.GetString("Start") + "\n (s)",
			Catalog.GetString("Duration") + "\n (s)",
			Catalog.GetString("Distance") + "\n (cm)",
			"v" + "\n (m/s)",
			"vmax" + "\n (m/s)",
			"t->vmax" + "\n (s)",
			"p" + "\n (W)",
			"pmax" + "\n (W)",
			"t->pmax" + "\n (s)",
			"RPD" + "\n (W/s)",
			"F" + "\n (N)",
			"Fmax" + "\n (N)",
			"t->Fmax" + "\n (s)",
			"RFD" + "\n (N/s)",
			workString,
			Catalog.GetString("Impulse") + "\n (N*s)"
		};

		encoderCaptureCurves = new ArrayList ();

		headers = true;
		foreach(string line in contents)
		{
			/*
			 * don't print this because on capture, if 100 repetitions are captured
			 * it will be printing 97 lines, 98, 99, 100 (with a small time)
			 * can be too much for certain computers
			 */
			//LogB.Debug(line);

			if(headers) {
				headers = false;
				continue;
			}

			if (line == null)
				break;

			curvesCount ++;

			string [] cells = line.Split(new char[] {','});

			//check if data is ok
			if(! fixDecimalsWillWork(true, cells))
				return curvesCount;

			cells = fixDecimals(true, cells);
			
			/*
			 * don't print this because on capture, if 100 repetitions are captured
			 * it will be printing 97 lines, 98, 99, 100 (with a small time)
			 * can be too much for certain computers
			 */
			//LogB.Error(Util.StringArrayToString(cells, ":"));

			encoderCaptureCurves.Add (new EncoderCurve (
						false,				//user need to mark to save them
						cells[0],	//id 
						//cells[1],	//seriesName
						//cells[2], 	//exerciseName
						//cells[3], 	//massBody
						//cells[4], 	//massExtra
						cells[5], cells[6], cells[7], 	//start, duration, height 
						cells[8], cells[9], cells[10], 	//meanSpeed, maxSpeed, maxSpeedT
						cells[11], cells[12], cells[13],//meanPower, peakPower, peakPowerT
						cells[14],			//peakPower / peakPowerT
						cells[15], cells[16], cells[17], //meanForce, maxForce maxForceT
						cells[18], 			//meanForce / meanForceT
						cells[19], cells[20] 		//work, impulse
						));

		}
		//if last repetition is eccentric (there's no concentric movement after than that)
		//then delete that curve
		if(encoderCaptureCurves.Count > 0) {
			EncoderCurve curve = (EncoderCurve) encoderCaptureCurves[encoderCaptureCurves.Count -1];
			if(Convert.ToDouble(curve.Height) < 0) //it's 'e'
				encoderCaptureCurves = Util.RemoveLastArrayElement(encoderCaptureCurves);

		}

		encoderCaptureListStore = new Gtk.ListStore (typeof (EncoderCurve));
		
		feedbackWin.ResetBestSetValue(FeedbackWindow.BestSetValueEnum.AUTOMATIC_FEEDBACK);
		bool eccPhase = true;
		foreach (EncoderCurve curve in encoderCaptureCurves)
		{
			encoderCaptureListStore.AppendValues (curve);
				
			if( ecconLast == "c" ||
					preferences.encoderCaptureFeedbackEccon == Preferences.EncoderPhasesEnum.BOTH ||
					preferences.encoderCaptureFeedbackEccon == Preferences.EncoderPhasesEnum.ECC && eccPhase ||
					preferences.encoderCaptureFeedbackEccon == Preferences.EncoderPhasesEnum.CON && ! eccPhase )
				feedbackWin.UpdateBestSetValue(curve);

			eccPhase = ! eccPhase;
		}

		treeview_encoder_capture_curves.Model = encoderCaptureListStore;

		/*
		if(ecconLast == "c")
			treeview_encoder_capture_curves.Selection.Mode = SelectionMode.Single;
		else
			treeview_encoder_capture_curves.Selection.Mode = SelectionMode.Multiple;
			*/
		treeview_encoder_capture_curves.Selection.Mode = SelectionMode.None;

		treeview_encoder_capture_curves.HeadersVisible=true;
		
		
		//create first column (checkbox)	
		CellRendererToggle crt = new CellRendererToggle();
		crt.Visible = true;
		crt.Activatable = true;
		crt.Active = true;
		crt.Toggled += EncoderCaptureItemToggled;
		Gtk.TreeViewColumn column = new Gtk.TreeViewColumn ();

		column.Title = Catalog.GetString("Saved");
		column.PackStart (crt, true);
		column.SetCellDataFunc (crt, new Gtk.TreeCellDataFunc (RenderRecord));
		treeview_encoder_capture_curves.AppendColumn (column);

		int i=0;
		foreach(string myCol in columnsString) {
			Gtk.TreeViewColumn aColumn = new Gtk.TreeViewColumn ();
			CellRendererText aCell = new CellRendererText();
			aColumn.Title=myCol;
			aColumn.PackStart (aCell, true);

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
				case 11:
					aColumn.SetCellDataFunc (aCell, new Gtk.TreeCellDataFunc (RenderMeanForce));
					break;
				case 12:
					aColumn.SetCellDataFunc (aCell, new Gtk.TreeCellDataFunc (RenderMaxForce));
					break;
				case 13:
					aColumn.SetCellDataFunc (aCell, new Gtk.TreeCellDataFunc (RenderMaxForceT));
					break;
				case 14:
					aColumn.SetCellDataFunc (aCell, new Gtk.TreeCellDataFunc (RenderMaxForce_maxForceT));
					break;
				case 15:
					aColumn.SetCellDataFunc (aCell, new Gtk.TreeCellDataFunc (RenderWork));
					break;
				case 16:
					aColumn.SetCellDataFunc (aCell, new Gtk.TreeCellDataFunc (RenderImpulse));
					break;
			}
					
			if( ! ( (i == 1 || i == 2) && ! showStartAndDuration ) )
				treeview_encoder_capture_curves.AppendColumn (aColumn);
			i++;
		}

		UtilGtk.TreeviewScrollToLastRow(treeview_encoder_capture_curves, encoderCaptureListStore, encoderCaptureCurves.Count);

		return curvesCount;
	}
	
	//rowNum starts at zero
	void saveOrDeleteCurveFromCaptureTreeView(bool dbconOpened, int rowNum, EncoderCurve curve, bool save) 
	{
		LogB.Information("saving? " + save.ToString() + "; rownum:" + rowNum.ToString());
		if(save)
			encoderSaveSignalOrCurve(dbconOpened, "curve", rowNum +1);
		else {
			double msStart = Convert.ToDouble(curve.Start);
			double msEnd = -1;
			if(ecconLast == "c")
				msEnd = Convert.ToDouble(curve.Start) + 
					Convert.ToDouble(curve.Duration);
			else {
				EncoderCurve curveNext = 
					treeviewEncoderCaptureCurvesGetCurve(rowNum +2,false);
				msEnd = Convert.ToDouble(curveNext.Start) + 
					Convert.ToDouble(curveNext.Duration);
			}

			ArrayList signalCurves = SqliteEncoder.SelectSignalCurve(dbconOpened,
					Convert.ToInt32(encoderSignalUniqueID), -1, 
					msStart, msEnd);
			foreach(EncoderSignalCurve esc in signalCurves)
				delete_encoder_curve(dbconOpened, esc.curveID);
		}
	}

	private string encoderCaptureItemToggledArgsPath = "";
	void EncoderCaptureItemToggled(object o, ToggledArgs args)
	{
		//cannot toggle item while capturing or recalculating
		if(capturingCsharp == encoderCaptureProcess.CAPTURING ||
				encoderRProcAnalyze.status == EncoderRProc.Status.RUNNING)
			return;

		int inertialStart = 0;
		if( current_mode == Constants.Modes.POWERINERTIAL)
		{
			if(ecconLast == "c")
				inertialStart = preferences.encoderCaptureInertialDiscardFirstN;
			else
				inertialStart = 2 * preferences.encoderCaptureInertialDiscardFirstN;
		}

		string myArgsPath = "";
		if(encoderCaptureItemToggledArgsPath != "")
			myArgsPath = encoderCaptureItemToggledArgsPath;
		else
			myArgsPath = args.Path;

		//LogB.Information("myArgsPath: " + myArgsPath);
		TreeIter iter;
		int column = 0;
		if (encoderCaptureListStore.GetIterFromString (out iter, myArgsPath))
		{
			int rowNum = Convert.ToInt32(myArgsPath); //starts at zero

			//do not allow to click a discarded repetition
			if(rowNum < inertialStart)
				return;
			
			//on "ecS" don't pass the 2nd row, pass always the first
			//then need to move the iter to previous row
			TreePath path = new TreePath(myArgsPath);
			if(ecconLast != "c" && ! Util.IsEven(rowNum)) {
				rowNum --;
				path.Prev();
				//there's no "IterPre", for this reason we use this path method:
				encoderCaptureListStore.GetIter (out iter, path);
			
				/*
				 * caution, note args.Path has not changed; but path, iter and rowNum have decreased
				 * do not use args.Path from now
				 */
			}

			EncoderCurve curve = (EncoderCurve) encoderCaptureListStore.GetValue (iter, column);
			//get previous value
			bool val = curve.Record;

			//change value
			//this changes value, but checkbox will be changed on RenderRecord. Was impossible to do here.
			((EncoderCurve) encoderCaptureListStore.GetValue (iter, column)).Record = ! val;
				
			//this makes RenderRecord work on changed row without having to put mouse there
			encoderCaptureListStore.EmitRowChanged(path,iter);

			saveOrDeleteCurveFromCaptureTreeView(false, rowNum, curve, ! val);

			//maybe changed repetition updates the max, so check it:
			findMaxPowerSpeedForceIntersession();
			
			/* temporarily removed message
			 *
			string message = "";
			if(! val)
				message = Catalog.GetString("Saved");
			else
				message = Catalog.GetString("Removed");
			if(ecconLast ==	"c")
				label_encoder_curve_action.Text = message + " " + (rowNum +1).ToString();
			else
				label_encoder_curve_action.Text = message + " " + (decimal.Truncate((rowNum +1) /2) +1).ToString();
				*/


			//on ec, ecS need to [un]select second row
			if (ecconLast=="ec" || ecconLast =="ecS") {
				path.Next();
				encoderCaptureListStore.IterNext (ref iter);

				//change value
				((EncoderCurve) encoderCaptureListStore.GetValue (iter, column)).Record = ! val;

				//this makes RenderRecord work on changed row without having to put mouse there
				encoderCaptureListStore.EmitRowChanged(path,iter);
			}
			
			updateUserCurvesLabelsAndCombo(false);

			callPlotCurvesGraphDoPlot();
		}
	}

	//mainVariable used if saveOption == BEST
	void encoderCaptureSaveCurvesAllNoneBest(Constants.EncoderAutoSaveCurve saveOption, string mainVariable)
	{
		int bestRow = 0;
		int numRows = 0;
		List<int> list_bestN = new List<int>();
		int bestN = Convert.ToInt32(spin_encoder_capture_curves_best_n.Value);

		int inertialStart = 0;
		if( current_mode == Constants.Modes.POWERINERTIAL)
		{
			if(ecconLast == "c")
				inertialStart = preferences.encoderCaptureInertialDiscardFirstN;
			else
				inertialStart = 2 * preferences.encoderCaptureInertialDiscardFirstN;
		}

		if(saveOption == Constants.EncoderAutoSaveCurve.BEST ||
				saveOption == Constants.EncoderAutoSaveCurve.FROM4TOPENULTIMATE ||
				saveOption == Constants.EncoderAutoSaveCurve.BESTN ||
				saveOption == Constants.EncoderAutoSaveCurve.BESTNCONSECUTIVE)
		{
			if(ecconLast == "c") {
				//get the concentric curves
				EncoderSignal encoderSignal = new EncoderSignal(treeviewEncoderCaptureCurvesGetCurves(AllEccCon.CON));

				if(saveOption == Constants.EncoderAutoSaveCurve.BEST)
					bestRow = encoderSignal.FindPosOfBest(inertialStart, mainVariable);
				else if(saveOption == Constants.EncoderAutoSaveCurve.FROM4TOPENULTIMATE)
					numRows = encoderSignal.CurvesNum();
				else if(saveOption == Constants.EncoderAutoSaveCurve.BESTN)
					list_bestN = encoderSignal.FindPosOfBestN(inertialStart, mainVariable,
							bestN, EncoderSignal.Contraction.C,
							Preferences.EncoderRepetitionCriteria.CON); //but not used
				else if(saveOption == Constants.EncoderAutoSaveCurve.BESTNCONSECUTIVE)
					bestRow = encoderSignal.FindPosOfBestNConsecutive(inertialStart, mainVariable,
							bestN);
			} else {
				//decide if best is by ecc_con average, ecc or con
				Preferences.EncoderRepetitionCriteria repCriteria =
					preferences.GetEncoderRepetitionCriteria (current_mode);

				EncoderSignal encoderSignal = new EncoderSignal(treeviewEncoderCaptureCurvesGetCurves(AllEccCon.ALL));
				if(saveOption == Constants.EncoderAutoSaveCurve.BEST)
					bestRow = encoderSignal.FindPosOfBestEccCon(inertialStart, mainVariable, repCriteria); //will be pos of the ecc
				else if(saveOption == Constants.EncoderAutoSaveCurve.FROM4TOPENULTIMATE)
					numRows = encoderSignal.CurvesNum();
				else if(saveOption == Constants.EncoderAutoSaveCurve.BESTN)
					list_bestN = encoderSignal.FindPosOfBestN(inertialStart, mainVariable,
							bestN, EncoderSignal.Contraction.EC, repCriteria);
				else if(saveOption == Constants.EncoderAutoSaveCurve.BESTNCONSECUTIVE)
					bestRow = encoderSignal.FindPosOfBestNConsecutiveEccCon(inertialStart, mainVariable,
							bestN, repCriteria);
			}
		}

		int i = 0; //on "c" and ! "c": i is every row
		string sep = "";
		string messageRows = "";
		
		TreeIter iter;
		bool iterOk = encoderCaptureListStore.GetIterFirst(out iter);
		if(! iterOk)
			return;

		//need to open Sqlite because if more than 50 curves are saved/deleted, it will crash if open/close connnections all the time
		//TODO: do as a transaction, but code need to be refactored
		Sqlite.Open();

		bool changeTo;
		while(iterOk)
		{
			TreePath path = encoderCaptureListStore.GetPath(iter);
			
			//discard first rows
			bool thisRowDiscarded = false;
			if( current_mode == Constants.Modes.POWERINERTIAL &&
					( (ecconLast == "c" && i < preferences.encoderCaptureInertialDiscardFirstN) ||
					(ecconLast != "c" && i < 2 * preferences.encoderCaptureInertialDiscardFirstN) ) )
			{
				thisRowDiscarded = true;
			}

			bool fromValidToPenult = false;
			if( saveOption == Constants.EncoderAutoSaveCurve.FROM4TOPENULTIMATE &&
					( (ecconLast == "c" && i < numRows -1) ||
					(ecconLast != "c" && i < numRows -2) ) )
				fromValidToPenult = true;
			
			EncoderCurve curve = (EncoderCurve) encoderCaptureListStore.GetValue (iter, 0);
			if(
					(! curve.Record && ! thisRowDiscarded && saveOption == Constants.EncoderAutoSaveCurve.ALL) ||
					(! curve.Record && ! thisRowDiscarded && saveOption == Constants.EncoderAutoSaveCurve.BEST && i == bestRow) ||
					(! curve.Record && ! thisRowDiscarded && saveOption == Constants.EncoderAutoSaveCurve.BESTN && Util.FoundInListInt(list_bestN, i)) ||
					(! curve.Record && ! thisRowDiscarded && saveOption == Constants.EncoderAutoSaveCurve.BESTNCONSECUTIVE &&
					 i >= bestRow && ( (ecconLast == "c" && i < bestRow + bestN) || (ecconLast != "c" && i < bestRow + 2*bestN) )) ||
					(! curve.Record && ! thisRowDiscarded && saveOption == Constants.EncoderAutoSaveCurve.FROM4TOPENULTIMATE && fromValidToPenult) ||
					(curve.Record && (thisRowDiscarded || saveOption == Constants.EncoderAutoSaveCurve.BEST && i != bestRow)) ||
					(curve.Record && (thisRowDiscarded || saveOption == Constants.EncoderAutoSaveCurve.BESTN && ! Util.FoundInListInt(list_bestN, i))) ||
					(curve.Record && (thisRowDiscarded || saveOption == Constants.EncoderAutoSaveCurve.BESTNCONSECUTIVE && //! (i >= bestRow && i < bestRow + bestN))) ||
					! (i >= bestRow && ( (ecconLast == "c" && i < bestRow + bestN) || (ecconLast != "c" && i < bestRow + 2*bestN) )))) ||
					(curve.Record && (thisRowDiscarded || saveOption == Constants.EncoderAutoSaveCurve.NONE)) ||
					(curve.Record && (thisRowDiscarded || saveOption == Constants.EncoderAutoSaveCurve.FROM4TOPENULTIMATE && ! fromValidToPenult)) )
			{ 
				changeTo = ! curve.Record;
				
				//change value
				((EncoderCurve) encoderCaptureListStore.GetValue (iter, 0)).Record = changeTo;

				//this makes RenderRecord work on changed row without having to put mouse there
				encoderCaptureListStore.EmitRowChanged(path,iter);

				//on "ecS" don't pass the 2nd row, pass always the first
				saveOrDeleteCurveFromCaptureTreeView(true, i, curve, changeTo);
				
				if(ecconLast != "c") {
					path.Next();
					encoderCaptureListStore.IterNext (ref iter);
				
					//change value
					((EncoderCurve) encoderCaptureListStore.GetValue (iter, 0)).Record = changeTo;

					//this makes RenderRecord work on changed row without having to put mouse there
					encoderCaptureListStore.EmitRowChanged(path,iter);
				}
					
				messageRows += sep + (i+1).ToString();
				sep = ", ";
			} else {
				//if we don't change rows
				//but is ec
				//the advance now one row (the 'e')
				//and later it will advance the 'c'
				if(ecconLast != "c") {
					encoderCaptureListStore.IterNext (ref iter);
				}
			}

			i ++;
			if(ecconLast != "c")
				i ++;

			iterOk = encoderCaptureListStore.IterNext (ref iter);
		}
		
		Sqlite.Close();

		prepareAnalyzeRepetitions();
			
		callPlotCurvesGraphDoPlot();
	}
	
	//saved curves (when load), or recently deleted curves should modify the encoderCapture treeview
	//used also on bells close
	void encoderCaptureSelectBySavedCurves(int msCentral, bool selectIt) {
		TreeIter iter;
		TreeIter iterPre;
		bool iterOk = encoderCaptureListStore.GetIterFirst(out iter);
		while(iterOk) {
			TreePath path = encoderCaptureListStore.GetPath(iter);
			EncoderCurve curve = (EncoderCurve) encoderCaptureListStore.GetValue (iter, 0);
			
			string eccon = findEccon(true);
			if(eccon == "c") {
				if(Convert.ToDouble(curve.Start) <= msCentral && 
						Convert.ToDouble(curve.Start) + Convert.ToDouble(curve.Duration) >= msCentral) 
				{
					((EncoderCurve) encoderCaptureListStore.GetValue (iter, 0)).Record = selectIt;

					//this makes RenderRecord work on changed row without having to put mouse there
					encoderCaptureListStore.EmitRowChanged(path,iter);
				}
			}
			else { // if(eccon == "ecS")
				iterPre = iter; //to point at the "e" curve
				iterOk = encoderCaptureListStore.IterNext (ref iter);

				//this fixes when there's a 'e' but not a 'c' in last repetition
				if(! iterOk)
					break;

				EncoderCurve curve2 = (EncoderCurve) encoderCaptureListStore.GetValue (iter, 0);

				LogB.Information("msCentral, start, end" + msCentral.ToString() + " " + curve.Start + " " + 
						(Convert.ToDouble(curve2.Start) + Convert.ToDouble(curve2.Duration)).ToString());

				if(Convert.ToDouble(curve.Start) <= msCentral && 
						Convert.ToDouble(curve2.Start) + Convert.ToDouble(curve2.Duration) >= msCentral) 
				{
					((EncoderCurve) encoderCaptureListStore.GetValue (iterPre, 0)).Record = selectIt;
					((EncoderCurve) encoderCaptureListStore.GetValue (iter, 0)).Record = selectIt;

					//this makes RenderRecord work on changed row without having to put mouse there
					encoderCaptureListStore.EmitRowChanged(path,iterPre);
					encoderCaptureListStore.EmitRowChanged(path,iter);
				}
			}

			iterOk = encoderCaptureListStore.IterNext (ref iter);
		}
			
		callPlotCurvesGraphDoPlot();
	}


	//on screen shown on s but export is in ms
	public string [] GetTreeviewEncoderAnalyzeHeaders(bool screenOrCSV, Constants.Modes encoderMode)
	{
		string timeUnits = "(s)";
		string distanceUnits = "(cm)";
		if(! screenOrCSV)
		{
			timeUnits = "(ms)";
			distanceUnits = "(mm)";
		}

		string workString = "|" + Catalog.GetString("Work") + "|";
		if(preferences.encoderWorkKcal)
			workString += "\n (KCal)";
		else
			workString += "\n (J)";

		string [] startArray = {
			Catalog.GetString("Repetition") + "\n",
			Catalog.GetString("Series") + "\n",
			Catalog.GetString("Exercise") + "\n",
			Catalog.GetString("Laterality") + "\n",
			Catalog.GetString("Extra weight") + "\n(Kg)",
			Catalog.GetString("Total weight") + "\n(Kg)"
		};

		string [] inertiaArray = {
			Catalog.GetString("Inertia M.") + "\n(Kg*cm^2)", 	//inertial
			Catalog.GetString("Diameter") + "\n(cm)",		//inertial
			Catalog.GetString("Equivalent mass") + "\n(Kg)"		//inertial
		};

		string [] endArray = {
			Catalog.GetString("Start") + "\n" + timeUnits,
			Catalog.GetString("Duration") + "\n" + timeUnits,
			Catalog.GetString("Distance") + "\n" + distanceUnits,
			"v" + "\n(m/s)",
			"vmax" + "\n(m/s)",
			"t->vmax" + "\n" + timeUnits,
			"p" + "\n(W)",
			"pmax" + "\n(W)",
			"t->pmax" + "\n" + timeUnits,
			"RPD" + "\n(W/s)",
			"F" + "\n(N)",
			"Fmax" + "\n(N)",
			"t->Fmax" + "\n" + timeUnits,
			"RFD" + "\n(N/s)",
			workString,
			Catalog.GetString("Impulse") + "\n (N*s)"
		};

		if(encoderMode == Constants.Modes.POWERGRAVITATORY)
		{
			string [] headers = new string[startArray.Length + endArray.Length];
			startArray.CopyTo(headers, 0);
			endArray.CopyTo(headers, startArray.Length);
			return headers;
		} else {
			string [] headers = new string[startArray.Length + inertiaArray.Length + endArray.Length];
			startArray.CopyTo(headers, 0);
			inertiaArray.CopyTo(headers, startArray.Length);
			endArray.CopyTo(headers, startArray.Length + inertiaArray.Length);
			return headers;
		}
	}

	bool lastTreeviewEncoderAnalyzeIsNeuromuscular = false;

	private int createTreeViewEncoderAnalyze(string contents, Constants.Modes encoderMode)
	{
		//note we pass powerinertial because we want here all columns but only relevant will shown
		//on the other hand, on_button_encoder_save_table_file_selected will need to show the relevant columns
		string [] columnsString = GetTreeviewEncoderAnalyzeHeaders(true, //screen
				Constants.Modes.POWERINERTIAL);

		ArrayList encoderAnalyzeCurves = new ArrayList ();

		//write exercise and extra weight data
		string exerciseName = "";
		double totalMass = 0; 
		if(radio_encoder_analyze_individual_current_set.Active) {	//current set
			exerciseName = UtilGtk.ComboGetActive(combo_encoder_exercise_capture);
			totalMass = findMass(Constants.MassType.DISPLACED);
		} else {						//not current set
			//TODO:
			/*SqliteEncoder.Select(
					false, -1, currentPerson.UniqueID, currentSession.UniqueID, currentEncoderGI,
					-1, "curve", EncoderSQL.Eccons.ALL, 
					true, true);
			*/
		}

		string line;
		int curvesCount = 0;
		using (StringReader reader = new StringReader (contents)) {
			line = reader.ReadLine ();	//headers
			LogB.Information(line);
			do {
				line = reader.ReadLine ();
				LogB.Information(line);
				if (line == null)
					break;

				curvesCount ++;

				string [] cells = line.Split(new char[] {','});

				//check if data is ok
				if(! fixDecimalsWillWork(false, cells))
					return curvesCount;

				cells = fixDecimals(false, cells);
				
				
				if(! radio_encoder_analyze_individual_current_set.Active) {	//not current set
					/*
					 * better don't do this to avoid calling SQL in both treads
					EncoderSQL eSQL = (EncoderSQL) curvesData[curvesCount];
					exerciseName = eSQL.exerciseName;
					totalMass = eSQL.extraWeight;
					*/
					exerciseName = cells[2];
					//cells[3]: massBody
					//cells[4]: massExtra

		
					//don't show the DisplacedWeight on AVG or SD because there can be many exercises 
					//(with different exercisePercentBodyWeight) and persons
					if(cells[0] == "MAX" || cells[0] == "AVG" || cells[0] == "SD")
						totalMass  = -1; //mark to not be shown
					else
						totalMass = Convert.ToDouble(Util.ChangeDecimalSeparator(cells[3])) * 
							getExercisePercentBodyWeightFromName (exerciseName) / 100.0
							+ Convert.ToDouble(Util.ChangeDecimalSeparator(cells[4]));
					
					LogB.Debug("totalMass:" + totalMass.ToString());
				}

				encoderAnalyzeCurves.Add (new EncoderCurve (
							cells[0], 
							cells[1],	//seriesName 
							exerciseName,
							cells[21],	//laterality
							Convert.ToDouble(Util.ChangeDecimalSeparator(cells[4])), 	//extraWeight
							totalMass, 							//displaceWeight
							Convert.ToInt32(cells[22]), 					//inertia M. (inertial)
							Convert.ToDouble(cells[23]), 					//diameter (inertial)
							Convert.ToDouble(cells[24]), 					//equivalent mass (inertial)
							cells[5], cells[6], cells[7], 
							cells[8], cells[9], cells[10], 
							cells[11], cells[12], cells[13],
							cells[14],
							cells[15], cells[16], cells[17], //meanForce, maxForce maxForceT
							cells[18],
							cells[19], cells[20]
							));

			} while(true);
		}

		encoderAnalyzeListStore = new Gtk.ListStore (typeof (EncoderCurve));
	
		feedbackWin.ResetBestSetValue(FeedbackWindow.BestSetValueEnum.AUTOMATIC_FEEDBACK);
		bool eccPhase = true;
		foreach (EncoderCurve curve in encoderAnalyzeCurves)
		{
			encoderAnalyzeListStore.AppendValues (curve);

			if( ecconLast == "c" ||
					preferences.encoderCaptureFeedbackEccon == Preferences.EncoderPhasesEnum.BOTH ||
					preferences.encoderCaptureFeedbackEccon == Preferences.EncoderPhasesEnum.ECC && eccPhase ||
					preferences.encoderCaptureFeedbackEccon == Preferences.EncoderPhasesEnum.CON && ! eccPhase )
				feedbackWin.UpdateBestSetValue(curve);

			eccPhase = ! eccPhase;
		}

		treeview_encoder_analyze_curves.Model = encoderAnalyzeListStore;

		treeview_encoder_analyze_curves.Selection.Mode = SelectionMode.None;

		treeview_encoder_analyze_curves.HeadersVisible=true;

		int i=0;
		foreach(string myCol in columnsString)
		{
			//do not show inertia moment, diameter, equivalent mass on powergravitatory
			if(encoderMode == Constants.Modes.POWERGRAVITATORY && (i == 6 || i == 7 || i == 8))
			{
				i ++;
				continue;
			}

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
					aColumn.SetCellDataFunc (aCell, new Gtk.TreeCellDataFunc (RenderLaterality));
					break;
				case 4:
					aColumn.SetCellDataFunc (aCell, new Gtk.TreeCellDataFunc (RenderExtraWeight));
					break;
				case 5:
					aColumn.SetCellDataFunc (aCell, new Gtk.TreeCellDataFunc (RenderDisplacedWeight));
					break;
				case 6:
					aColumn.SetCellDataFunc (aCell, new Gtk.TreeCellDataFunc (RenderInertia)); 	//inertial
					break;
				case 7:
					aColumn.SetCellDataFunc (aCell, new Gtk.TreeCellDataFunc (RenderDiameter)); 	//inertial
					break;
				case 8:
					aColumn.SetCellDataFunc (aCell, new Gtk.TreeCellDataFunc (RenderEquivalentMass)); 	//inertial
					break;
				case 9:
					aColumn.SetCellDataFunc (aCell, new Gtk.TreeCellDataFunc (RenderStart));
					break;
				case 10:
					aColumn.SetCellDataFunc (aCell, new Gtk.TreeCellDataFunc (RenderDuration));
					break;
				case 11:
					aColumn.SetCellDataFunc (aCell, new Gtk.TreeCellDataFunc (RenderHeight));
					break;
				case 12:
					aColumn.SetCellDataFunc (aCell, new Gtk.TreeCellDataFunc (RenderMeanSpeed));
					break;
				case 13:
					aColumn.SetCellDataFunc (aCell, new Gtk.TreeCellDataFunc (RenderMaxSpeed));
					break;
				case 14:
					aColumn.SetCellDataFunc (aCell, new Gtk.TreeCellDataFunc (RenderMaxSpeedT));
					break;
				case 15:
					aColumn.SetCellDataFunc (aCell, new Gtk.TreeCellDataFunc (RenderMeanPower));
					break;
				case 16:
					aColumn.SetCellDataFunc (aCell, new Gtk.TreeCellDataFunc (RenderPeakPower));
					break;
				case 17:
					aColumn.SetCellDataFunc (aCell, new Gtk.TreeCellDataFunc (RenderPeakPowerT));
					break;
				case 18:
					aColumn.SetCellDataFunc (aCell, new Gtk.TreeCellDataFunc (RenderPP_PPT));
					break;
				case 19:
					aColumn.SetCellDataFunc (aCell, new Gtk.TreeCellDataFunc (RenderMeanForce));
					break;
				case 20:
					aColumn.SetCellDataFunc (aCell, new Gtk.TreeCellDataFunc (RenderMaxForce));
					break;
				case 21:
					aColumn.SetCellDataFunc (aCell, new Gtk.TreeCellDataFunc (RenderMaxForceT));
					break;
				case 22:
					aColumn.SetCellDataFunc (aCell, new Gtk.TreeCellDataFunc (RenderMaxForce_maxForceT));
					break;
				case 23:
					aColumn.SetCellDataFunc (aCell, new Gtk.TreeCellDataFunc (RenderWork));
					break;
				case 24:
					aColumn.SetCellDataFunc (aCell, new Gtk.TreeCellDataFunc (RenderImpulse));
					break;
			}
			
			treeview_encoder_analyze_curves.AppendColumn (aColumn);
			i++;
		}

		lastTreeviewEncoderAnalyzeIsNeuromuscular = false; 

		return curvesCount;
	}

	string [] treeviewEncoderAnalyzeNeuromuscularHeaders = {
		Catalog.GetString ("Person") + "\n",
		Catalog.GetString ("Jump") + "\n",
		Catalog.GetString("Extra weight") + "\n(Kg)",
		"e1 range" + "\n (mm)",
		"e1 t" + "\n (ms)",
		"e1 fmax" + "\n (N)",
		"e1 rfd avg" + "\n (N/s)",
		"e1 i" + "\n (N*s/Kg)",
		"ca range" + "\n (mm)",
		"cl t" + "\n (ms)",
		"cl rfd avg" + "\n (N/s/Kg)",
		"cl i" + "\n (N*s/Kg)",
		"cl f avg" + "\n (N/Kg)",
		"cl vf" + "\n (N)",
		"cl f max" + "\n (N)",
		"cl s avg" + "\n (m/s)",
		"cl s max" + "\n (m/s)",
		"cl p avg" + "\n (W)",
		"cl p max" + "\n (W)"
	};

	private int createTreeViewEncoderAnalyzeNeuromuscular(string contents) {
		string [] columnsString = treeviewEncoderAnalyzeNeuromuscularHeaders;

		ArrayList encoderAnalyzeNm = new ArrayList ();

		string line;
		int curvesCount = 0;
		using (StringReader reader = new StringReader (contents)) {
			line = reader.ReadLine ();	//headers
			LogB.Information(line);
			do {
				line = reader.ReadLine ();
				LogB.Information(line);
				if (line == null)
					break;

				curvesCount ++;

				string [] cells = line.Split(new char[] {','});
				encoderAnalyzeNm.Add (new EncoderNeuromuscularData (cells));

			} while(true);
		}

		encoderAnalyzeListStore = new Gtk.ListStore (typeof (EncoderNeuromuscularData));
		foreach (EncoderNeuromuscularData nm in encoderAnalyzeNm) 
			encoderAnalyzeListStore.AppendValues (nm);

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
//				case 0:
//					aColumn.SetCellDataFunc (aCell, new Gtk.TreeCellDataFunc (Render_code));
//					break;
				case 0:
					aColumn.SetCellDataFunc (aCell, new Gtk.TreeCellDataFunc (Render_person));
					break;
				case 1:
					aColumn.SetCellDataFunc (aCell, new Gtk.TreeCellDataFunc (Render_jump_num));
					break;
				case 2:
					aColumn.SetCellDataFunc (aCell, new Gtk.TreeCellDataFunc (RenderNeuromuscularExtraWeight));
					break;
				case 3:
					aColumn.SetCellDataFunc (aCell, new Gtk.TreeCellDataFunc (Render_e1_range));
					break;
				case 4:
					aColumn.SetCellDataFunc (aCell, new Gtk.TreeCellDataFunc (Render_e1_t));
					break;
				case 5:
					aColumn.SetCellDataFunc (aCell, new Gtk.TreeCellDataFunc (Render_e1_fmax));
					break;
				case 6:
					aColumn.SetCellDataFunc (aCell, new Gtk.TreeCellDataFunc (Render_e1_rfd_avg));
					break;
				case 7:
					aColumn.SetCellDataFunc (aCell, new Gtk.TreeCellDataFunc (Render_e1_i));
					break;
				case 8:
					aColumn.SetCellDataFunc (aCell, new Gtk.TreeCellDataFunc (Render_ca_range));
					break;
				case 9:
					aColumn.SetCellDataFunc (aCell, new Gtk.TreeCellDataFunc (Render_cl_t));
					break;
				case 10:
					aColumn.SetCellDataFunc (aCell, new Gtk.TreeCellDataFunc (Render_cl_rfd_avg));
					break;
				case 11:
					aColumn.SetCellDataFunc (aCell, new Gtk.TreeCellDataFunc (Render_cl_i));
					break;
				case 12:
					aColumn.SetCellDataFunc (aCell, new Gtk.TreeCellDataFunc (Render_cl_f_avg));
					break;
				case 13:
					aColumn.SetCellDataFunc (aCell, new Gtk.TreeCellDataFunc (Render_cl_vf));
					break;
				case 14:
					aColumn.SetCellDataFunc (aCell, new Gtk.TreeCellDataFunc (Render_cl_f_max));
					break;
				case 15:
					aColumn.SetCellDataFunc (aCell, new Gtk.TreeCellDataFunc (Render_cl_s_avg));
					break;
				case 16:
					aColumn.SetCellDataFunc (aCell, new Gtk.TreeCellDataFunc (Render_cl_s_max));
					break;
				case 17:
					aColumn.SetCellDataFunc (aCell, new Gtk.TreeCellDataFunc (Render_cl_p_avg));
					break;
				case 18:
					aColumn.SetCellDataFunc (aCell, new Gtk.TreeCellDataFunc (Render_cl_p_max));
					break;
			}
			
			treeview_encoder_analyze_curves.AppendColumn (aColumn);
			i++;
		}
		
		lastTreeviewEncoderAnalyzeIsNeuromuscular = true; 
		
		return curvesCount;
	}

	private int createTreeViewEncoderAnalyzeEncoder1RM(string contents) {
		//buscar o fer algun metode generic que serveixi per mes coses
		//si pot ser d'una classe diferent a la de ChronojumpWindow
		
		return -1;
	}


	/* start rendering capture and analyze cols */
	

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
	
	private void RenderRecord (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.ITreeModel model, Gtk.TreeIter iter) {
		(cell as Gtk.CellRendererToggle).Active = ((EncoderCurve) model.GetValue (iter, 0)).Record;
	}

	private void RenderN (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.ITreeModel model, Gtk.TreeIter iter)
	{
		//do this in order to have ecconLast useful for RenderN when capturing
		if(capturingCsharp == encoderCaptureProcess.CAPTURING)
			ecconLast = findEccon(false);

		EncoderCurve curve = (EncoderCurve) model.GetValue (iter, 0);
	
		//Check if it's number
		if(! curve.IsNumberN()) {
			(cell as Gtk.CellRendererText).Text = "";
			LogB.Error("Curve is not number at RenderN:" + curve.ToCSV(true, current_mode, "COMMA", preferences.encoderWorkKcal, ""));
			return;
		}
		

		if(ecconLast == "c")
			(cell as Gtk.CellRendererText).Text = 
				String.Format(UtilGtk.TVNumPrint(curve.N,1,0),Convert.ToInt32(curve.N));
		else if (ecconLast=="ec" || ecconLast =="ecS") 
		{
			bool isEven = Util.IsEven(Convert.ToInt32(curve.N));
			
			string phase = "e";
			if(isEven)
				phase = "c";
				
			(cell as Gtk.CellRendererText).Text = 
				decimal.Truncate((Convert.ToInt32(curve.N) +1) /2).ToString() + phase;
		} else 
		{	//(ecconLast=="ce" || ecconLast =="ceS")
			string phase = "c";
			bool isEven = Util.IsEven(Convert.ToInt32(curve.N));
			if(isEven)
				phase = "e";
				
			(cell as Gtk.CellRendererText).Text = 
				decimal.Truncate((Convert.ToInt32(curve.N) +1) /2).ToString() + phase;
		}
	}
	//from analyze, don't checks ecconLast
	private void RenderNAnalyze (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.ITreeModel model, Gtk.TreeIter iter)
	{
		EncoderCurve curve = (EncoderCurve) model.GetValue (iter, 0);

		//Check if it's valid
		if(! curve.IsValidN()) {
			(cell as Gtk.CellRendererText).Text = "";
			LogB.Error("Curve is not valid at RenderNAnalyze:" + curve.ToCSV(false, current_mode, "COMMA", preferences.encoderWorkKcal, ""));
			return;
		}
			
		if(curve.N == "MAX" || curve.N == "AVG" || curve.N == "SD") {
			(cell as Gtk.CellRendererText).Markup = "<b>" + Catalog.GetString(curve.N) + "</b>";
			return;
		}
		else if(curve.IsNumberNandEorC()) { //maybe from R comes and '21c' or '15e'. Just write it
			(cell as Gtk.CellRendererText).Text = curve.N;
			return;
		}
		
		if(radio_encoder_analyze_individual_current_set.Active && findEccon(false) == "ecS") 
		{
			string phase = "e";
			bool isEven = Util.IsEven(Convert.ToInt32(curve.N));
			if(isEven)
				phase = "c";

			(cell as Gtk.CellRendererText).Text = 
				decimal.Truncate((Convert.ToInt32(curve.N) +1) /2).ToString() + phase;
		}
		else if(radio_encoder_analyze_individual_current_set.Active && findEccon(false) == "ceS") 
		{
			string phase = "c";
			bool isEven = Util.IsEven(Convert.ToInt32(curve.N));
			if(isEven)
				phase = "e";

			(cell as Gtk.CellRendererText).Text = 
				decimal.Truncate((Convert.ToInt32(curve.N) +1) /2).ToString() + phase;
		} else
			(cell as Gtk.CellRendererText).Text = curve.N;
	}

	private void RenderSeries (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.ITreeModel model, Gtk.TreeIter iter)
	{
		EncoderCurve curve = (EncoderCurve) model.GetValue (iter, 0);

		if(curve.Series == "NA")
			(cell as Gtk.CellRendererText).Text = "";
		else 
			(cell as Gtk.CellRendererText).Text = curve.Series;
	}

	private void RenderExercise (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.ITreeModel model, Gtk.TreeIter iter)
	{
		EncoderCurve curve = (EncoderCurve) model.GetValue (iter, 0);
		
		if(curve.Exercise == "NA")
			(cell as Gtk.CellRendererText).Text = "";
		else 
			(cell as Gtk.CellRendererText).Text = curve.Exercise;
	}

	private void RenderLaterality (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.ITreeModel model, Gtk.TreeIter iter)
	{
		EncoderCurve curve = (EncoderCurve) model.GetValue (iter, 0);
		
		if(curve.Laterality == "NA")
			(cell as Gtk.CellRendererText).Text = "";
		else 
			(cell as Gtk.CellRendererText).Text = curve.Laterality;
	}

	private void renderBoldIfNeeded(Gtk.CellRenderer cell, EncoderCurve curve, string str)
	{
		if(curve.N == "MAX" || curve.N == "AVG" || curve.N == "SD")
			(cell as Gtk.CellRendererText).Markup = "<b>" + str + "</b>";
		else
			(cell as Gtk.CellRendererText).Text = str;
	}

	private void RenderExtraWeight (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.ITreeModel model, Gtk.TreeIter iter)
	{
		EncoderCurve curve = (EncoderCurve) model.GetValue (iter, 0);

		string str = String.Format(UtilGtk.TVNumPrint(curve.ExtraWeight.ToString(),3,0),Convert.ToInt32(curve.ExtraWeight));

		renderBoldIfNeeded(cell, curve, str);
	}

	private void RenderDisplacedWeight (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.ITreeModel model, Gtk.TreeIter iter)
	{
		EncoderCurve curve = (EncoderCurve) model.GetValue (iter, 0);
		
		string str = "";
			
		//don't show the DisplacedWeight on AVG or SD because there can be many exercises 
		//(with different exercisePercentBodyWeight) and persons
		if(curve.DisplacedWeight == -1)	
			str = "";
		else
		       	str = String.Format(UtilGtk.TVNumPrint(curve.DisplacedWeight.ToString(),3,0),Convert.ToInt32(curve.DisplacedWeight));
		
		renderBoldIfNeeded(cell, curve, str);
	}
	
	private void RenderInertia (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.ITreeModel model, Gtk.TreeIter iter)
	{
		EncoderCurve curve = (EncoderCurve) model.GetValue (iter, 0);

		string str = String.Format(UtilGtk.TVNumPrint(curve.Inertia.ToString(),3,0),Convert.ToInt32(curve.Inertia));

		renderBoldIfNeeded(cell, curve, str);
	}

	private void RenderDiameter (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.ITreeModel model, Gtk.TreeIter iter)
	{
		EncoderCurve curve = (EncoderCurve) model.GetValue (iter, 0);

		string str = String.Format(UtilGtk.TVNumPrint(curve.Diameter.ToString(),4,2),Convert.ToDouble(curve.Diameter));

		renderBoldIfNeeded(cell, curve, str);
	}

	private void RenderEquivalentMass (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.ITreeModel model, Gtk.TreeIter iter)
	{
		EncoderCurve curve = (EncoderCurve) model.GetValue (iter, 0);

		string str = String.Format(UtilGtk.TVNumPrint(curve.EquivalentMass.ToString(),6,2),Convert.ToDouble(curve.EquivalentMass));

		renderBoldIfNeeded(cell, curve, str);
	}

	private void RenderStart (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.ITreeModel model, Gtk.TreeIter iter)
	{
		EncoderCurve curve = (EncoderCurve) model.GetValue (iter, 0);
		double myStart = Convert.ToDouble(curve.Start)/1000; //ms->s
		string str = String.Format(UtilGtk.TVNumPrint(myStart.ToString(),6,3),myStart); 
		renderBoldIfNeeded(cell, curve, str);
	}
	
	private void RenderDuration (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.ITreeModel model, Gtk.TreeIter iter)
	{
		EncoderCurve curve = (EncoderCurve) model.GetValue (iter, 0);
		double myDuration = Convert.ToDouble(curve.Duration)/1000; //ms->s
		string str = String.Format(UtilGtk.TVNumPrint(myDuration.ToString(),5,3),myDuration); 
		renderBoldIfNeeded(cell, curve, str);
	}
	
	private void RenderHeight (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.ITreeModel model, Gtk.TreeIter iter)
	{
		EncoderCurve curve = (EncoderCurve) model.GetValue (iter, 0);
		string heightToCm = (Convert.ToDouble(curve.Height)/10).ToString();

		if(curve.N == "MAX" || curve.N == "AVG" || curve.N == "SD")
			(cell as Gtk.CellRendererText).Foreground = null;	//will show default color
		else {
			string myColor = assignColor(
					Convert.ToDouble(heightToCm),
					feedbackWin.EncoderHeightHigher,
					feedbackWin.EncoderHeightLower,
					feedbackWin.EncoderHeightHigherValue,
					feedbackWin.EncoderHeightLowerValue);
			if(myColor != "")
				(cell as Gtk.CellRendererText).Foreground = myColor;
			else
				(cell as Gtk.CellRendererText).Foreground = null;	//will show default color
		}

		string str = String.Format(UtilGtk.TVNumPrint(heightToCm,5,1),Convert.ToDouble(heightToCm));
		renderBoldIfNeeded(cell, curve, str);
	}
	
	private void RenderMeanSpeed (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.ITreeModel model, Gtk.TreeIter iter)
	{
		EncoderCurve curve = (EncoderCurve) model.GetValue (iter, 0);
		
		if(curve.N == "MAX" || curve.N == "AVG" || curve.N == "SD")
			(cell as Gtk.CellRendererText).Foreground = null;	//will show default color
		else {
			Preferences.EncoderPhasesEnum phaseEnum = getEncoderCurvePhaseEnum(curve);
			string myColor = feedbackWin.AssignColorAutomatic(
					FeedbackWindow.BestSetValueEnum.AUTOMATIC_FEEDBACK,
					curve, Constants.MeanSpeed, phaseEnum);

			if(myColor == "")
				myColor = assignColor(
						curve.MeanSpeedD,
						feedbackWin.EncoderMeanSpeedHigher,
						feedbackWin.EncoderMeanSpeedLower,
						feedbackWin.EncoderMeanSpeedHigherValue,
						feedbackWin.EncoderMeanSpeedLowerValue);
			if(myColor != "")
				(cell as Gtk.CellRendererText).Foreground = myColor;
			else
				(cell as Gtk.CellRendererText).Foreground = null;	//will show default color
		}

		//no need of UtilGtk.TVNumPrint, always has 1 digit on left of decimal
		string str = String.Format("{0,8:0.000}",Convert.ToDouble(curve.MeanSpeed));
		renderBoldIfNeeded(cell, curve, str);
	}

	private void RenderMaxSpeed (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.ITreeModel model, Gtk.TreeIter iter)
	{
		EncoderCurve curve = (EncoderCurve) model.GetValue (iter, 0);

		if(curve.N == "MAX" || curve.N == "AVG" || curve.N == "SD")
			(cell as Gtk.CellRendererText).Foreground = null;	//will show default color
		else {
			Preferences.EncoderPhasesEnum phaseEnum = getEncoderCurvePhaseEnum(curve);
			string myColor = feedbackWin.AssignColorAutomatic(
					FeedbackWindow.BestSetValueEnum.AUTOMATIC_FEEDBACK,
					curve, Constants.MaxSpeed, phaseEnum);

			if(myColor == "")
				myColor = assignColor(
						curve.MaxSpeedD,
						feedbackWin.EncoderMaxSpeedHigher,
						feedbackWin.EncoderMaxSpeedLower,
						feedbackWin.EncoderMaxSpeedHigherValue,
						feedbackWin.EncoderMaxSpeedLowerValue);
			if(myColor != "")
				(cell as Gtk.CellRendererText).Foreground = myColor;
			else
				(cell as Gtk.CellRendererText).Foreground = null;	//will show default color
		}

		//no need of UtilGtk.TVNumPrint, always has 1 digit on left of decimal
		string str = String.Format("{0,8:0.000}",Convert.ToDouble(curve.MaxSpeed));
		renderBoldIfNeeded(cell, curve, str);
	}
	
	private void RenderMaxSpeedT (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.ITreeModel model, Gtk.TreeIter iter)
	{
		EncoderCurve curve = (EncoderCurve) model.GetValue (iter, 0);
		double time = Convert.ToDouble(curve.MaxSpeedT)/1000; //ms->s
		string str = String.Format(UtilGtk.TVNumPrint(time.ToString(),5,3),time);
		renderBoldIfNeeded(cell, curve, str);
	}

	private void RenderMeanPower (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.ITreeModel model, Gtk.TreeIter iter)
	{
		EncoderCurve curve = (EncoderCurve) model.GetValue (iter, 0);
		
		if(curve.N == "MAX" || curve.N == "AVG" || curve.N == "SD")
			(cell as Gtk.CellRendererText).Foreground = null;	//will show default color
		else {
			Preferences.EncoderPhasesEnum phaseEnum = getEncoderCurvePhaseEnum(curve);
			string myColor = feedbackWin.AssignColorAutomatic(
					FeedbackWindow.BestSetValueEnum.AUTOMATIC_FEEDBACK,
					curve, Constants.MeanPower, phaseEnum);

			if(myColor == "")
				myColor = assignColor(
						curve.MeanPowerD,
						feedbackWin.EncoderPowerHigher,
						feedbackWin.EncoderPowerLower,
						feedbackWin.EncoderPowerHigherValue,
						feedbackWin.EncoderPowerLowerValue);
			if(myColor != "")
				(cell as Gtk.CellRendererText).Foreground = myColor;
			else
				(cell as Gtk.CellRendererText).Foreground = null;	//will show default color
		}

		string str = String.Format(UtilGtk.TVNumPrint(curve.MeanPower,7,1),Convert.ToDouble(curve.MeanPower));
		renderBoldIfNeeded(cell, curve, str);
	}

	private void RenderPeakPower (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.ITreeModel model, Gtk.TreeIter iter)
	{
		EncoderCurve curve = (EncoderCurve) model.GetValue (iter, 0);

		if(curve.N == "MAX" || curve.N == "AVG" || curve.N == "SD")
			(cell as Gtk.CellRendererText).Foreground = null;	//will show default color
		else {
			Preferences.EncoderPhasesEnum phaseEnum = getEncoderCurvePhaseEnum(curve);
			string myColor = feedbackWin.AssignColorAutomatic(
					FeedbackWindow.BestSetValueEnum.AUTOMATIC_FEEDBACK,
					curve, Constants.PeakPower, phaseEnum);

			if(myColor == "")
				myColor = assignColor(
						curve.PeakPowerD,
						feedbackWin.EncoderPeakPowerHigher,
						feedbackWin.EncoderPeakPowerLower,
						feedbackWin.EncoderPeakPowerHigherValue,
						feedbackWin.EncoderPeakPowerLowerValue);
			if(myColor != "")
				(cell as Gtk.CellRendererText).Foreground = myColor;
			else
				(cell as Gtk.CellRendererText).Foreground = null;	//will show default color
		}

		string str = String.Format(UtilGtk.TVNumPrint(curve.PeakPower,7,1),Convert.ToDouble(curve.PeakPower));
		renderBoldIfNeeded(cell, curve, str);
	}

	private void RenderPeakPowerT (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.ITreeModel model, Gtk.TreeIter iter)
	{
		EncoderCurve curve = (EncoderCurve) model.GetValue (iter, 0);
		double myPPT = Convert.ToDouble(curve.PeakPowerT)/1000; //ms->s
		string str = String.Format(UtilGtk.TVNumPrint(myPPT.ToString(),5,3),myPPT);
		renderBoldIfNeeded(cell, curve, str);
	}

	private void RenderPP_PPT (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.ITreeModel model, Gtk.TreeIter iter)
	{
		EncoderCurve curve = (EncoderCurve) model.GetValue (iter, 0);
		string str = String.Format(UtilGtk.TVNumPrint(curve.PP_PPT,6,1),Convert.ToDouble(curve.PP_PPT));
		renderBoldIfNeeded(cell, curve, str);
	}
	
	/* end of rendering analyze cols. Following gols are only on capture */

	private void RenderMeanForce (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.ITreeModel model, Gtk.TreeIter iter)
	{
		EncoderCurve curve = (EncoderCurve) model.GetValue (iter, 0);

		if(curve.N == "MAX" || curve.N == "AVG" || curve.N == "SD")
			(cell as Gtk.CellRendererText).Foreground = null;	//will show default color
		else {
			Preferences.EncoderPhasesEnum phaseEnum = getEncoderCurvePhaseEnum(curve);
			string myColor = feedbackWin.AssignColorAutomatic(
					FeedbackWindow.BestSetValueEnum.AUTOMATIC_FEEDBACK,
					curve, Constants.MeanForce, phaseEnum);

			if(myColor == "")
				myColor = assignColor(
						curve.MeanForceD,
						feedbackWin.EncoderMeanForceHigher,
						feedbackWin.EncoderMeanForceLower,
						feedbackWin.EncoderMeanForceHigherValue,
						feedbackWin.EncoderMeanForceLowerValue);
			if(myColor != "")
				(cell as Gtk.CellRendererText).Foreground = myColor;
			else
				(cell as Gtk.CellRendererText).Foreground = null;	//will show default color
		}

		string str = String.Format(UtilGtk.TVNumPrint(curve.MeanForce,7,1),Convert.ToDouble(curve.MeanForce));
		renderBoldIfNeeded(cell, curve, str);
	}

	private void RenderMaxForce (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.ITreeModel model, Gtk.TreeIter iter)
	{
		EncoderCurve curve = (EncoderCurve) model.GetValue (iter, 0);
		
		if(curve.N == "MAX" || curve.N == "AVG" || curve.N == "SD")
			(cell as Gtk.CellRendererText).Foreground = null;	//will show default color
		else {
			Preferences.EncoderPhasesEnum phaseEnum = getEncoderCurvePhaseEnum(curve);
			string myColor = feedbackWin.AssignColorAutomatic(
					FeedbackWindow.BestSetValueEnum.AUTOMATIC_FEEDBACK,
					curve, Constants.MaxForce, phaseEnum);

			if(myColor == "")
				myColor = assignColor(
						curve.MaxForceD,
						feedbackWin.EncoderMaxForceHigher,
						feedbackWin.EncoderMaxForceLower,
						feedbackWin.EncoderMaxForceHigherValue,
						feedbackWin.EncoderMaxForceLowerValue);
			if(myColor != "")
				(cell as Gtk.CellRendererText).Foreground = myColor;
			else
				(cell as Gtk.CellRendererText).Foreground = null;	//will show default color
		}

		string str = String.Format(UtilGtk.TVNumPrint(curve.MaxForce,7,1),Convert.ToDouble(curve.MaxForce));
		renderBoldIfNeeded(cell, curve, str);
	}
	
	private void RenderMaxForceT (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.ITreeModel model, Gtk.TreeIter iter)
	{
		EncoderCurve curve = (EncoderCurve) model.GetValue (iter, 0);
		double time = Convert.ToDouble(curve.MaxForceT)/1000; //ms->s
		string str = String.Format(UtilGtk.TVNumPrint(time.ToString(),5,3),time);
		renderBoldIfNeeded(cell, curve, str);
	}

	private void RenderMaxForce_maxForceT (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.ITreeModel model, Gtk.TreeIter iter)
	{
		EncoderCurve curve = (EncoderCurve) model.GetValue (iter, 0);
		string str = String.Format(UtilGtk.TVNumPrint(curve.MaxForce_MaxForceT,6,1),Convert.ToDouble(curve.MaxForce_MaxForceT));
		renderBoldIfNeeded(cell, curve, str);
	}
	
	private void RenderWork (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.ITreeModel model, Gtk.TreeIter iter)
	{
		EncoderCurve curve = (EncoderCurve) model.GetValue (iter, 0);

		double workValueD = curve.WorkJD;
		int decimals = 1;
		if(preferences.encoderWorkKcal)
		{
			workValueD = curve.WorkKcalD;
			decimals = 3;
		}

		string str = String.Format(UtilGtk.TVNumPrint(workValueD.ToString(),6, decimals), workValueD);
		renderBoldIfNeeded(cell, curve, str);
	}

	private void RenderImpulse (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.ITreeModel model, Gtk.TreeIter iter)
	{
		EncoderCurve curve = (EncoderCurve) model.GetValue (iter, 0);
		string str = String.Format(UtilGtk.TVNumPrint(curve.Impulse,6,3),Convert.ToDouble(curve.Impulse));
		renderBoldIfNeeded(cell, curve, str);
	}

	private Preferences.EncoderPhasesEnum getEncoderCurvePhaseEnum(EncoderCurve curve)
	{
		//LogB.Information("getEncoderCurvePhaseEnum curve: " + curve.ToCSV(false, ";", false, ""));

		//if N contains the e or c, use that
		if(curve.IsNumberNandEorC())
			return curve.GetPhaseEnum();

		if (ecconLast == "ec" || ecconLast == "ecS")
		{
			bool isEven = Util.IsEven(Convert.ToInt32(curve.N));
			if(isEven)
				return Preferences.EncoderPhasesEnum.CON;
			else
				return Preferences.EncoderPhasesEnum.ECC;
		}
		else if (ecconLast == "ce" || ecconLast == "ceS")
		{
			bool isEven = Util.IsEven(Convert.ToInt32(curve.N));
			if(isEven)
				return Preferences.EncoderPhasesEnum.ECC;
			else
				return Preferences.EncoderPhasesEnum.CON;
		}
		else // (ecconLast == "c")
			return Preferences.EncoderPhasesEnum.BOTH;
	}

	/* end of rendering capture and analyze cols */

	/* start rendering neuromuscular cols */

	private void Render_code (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.ITreeModel model, Gtk.TreeIter iter)
	{
		EncoderNeuromuscularData nm = (EncoderNeuromuscularData) model.GetValue (iter, 0);
		(cell as Gtk.CellRendererText).Text = nm.code.ToString();
	}

	private void Render_person (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.ITreeModel model, Gtk.TreeIter iter)
	{
		EncoderNeuromuscularData nm = (EncoderNeuromuscularData) model.GetValue (iter, 0);
		(cell as Gtk.CellRendererText).Text = nm.person.ToString();
	}

	private void Render_jump_num (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.ITreeModel model, Gtk.TreeIter iter)
	{
		EncoderNeuromuscularData nm = (EncoderNeuromuscularData) model.GetValue (iter, 0);
		(cell as Gtk.CellRendererText).Text = nm.jump_num.ToString();
	}

	private void RenderNeuromuscularExtraWeight (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.ITreeModel model, Gtk.TreeIter iter)
	{
		EncoderNeuromuscularData nm = (EncoderNeuromuscularData) model.GetValue (iter, 0);
		(cell as Gtk.CellRendererText).Text = nm.extraWeight.ToString();
	}


	private void Render_e1_range (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.ITreeModel model, Gtk.TreeIter iter)
	{
		EncoderNeuromuscularData nm = (EncoderNeuromuscularData) model.GetValue (iter, 0);
		(cell as Gtk.CellRendererText).Text = nm.e1_range.ToString();
	}

	private void Render_e1_t (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.ITreeModel model, Gtk.TreeIter iter)
	{
		EncoderNeuromuscularData nm = (EncoderNeuromuscularData) model.GetValue (iter, 0);
		(cell as Gtk.CellRendererText).Text = nm.e1_t.ToString();
	}

	private void Render_e1_fmax (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.ITreeModel model, Gtk.TreeIter iter)
	{
		EncoderNeuromuscularData nm = (EncoderNeuromuscularData) model.GetValue (iter, 0);
		(cell as Gtk.CellRendererText).Text = nm.e1_fmax.ToString();
	}

	private void Render_e1_rfd_avg (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.ITreeModel model, Gtk.TreeIter iter)
	{
		EncoderNeuromuscularData nm = (EncoderNeuromuscularData) model.GetValue (iter, 0);
		(cell as Gtk.CellRendererText).Text = nm.e1_rfd_avg.ToString();
	}

	private void Render_e1_i (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.ITreeModel model, Gtk.TreeIter iter)
	{
		EncoderNeuromuscularData nm = (EncoderNeuromuscularData) model.GetValue (iter, 0);
		(cell as Gtk.CellRendererText).Text = nm.e1_i.ToString();
	}

	private void Render_ca_range (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.ITreeModel model, Gtk.TreeIter iter)
	{
		EncoderNeuromuscularData nm = (EncoderNeuromuscularData) model.GetValue (iter, 0);
		(cell as Gtk.CellRendererText).Text = nm.ca_range.ToString();
	}

	private void Render_cl_t (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.ITreeModel model, Gtk.TreeIter iter)
	{
		EncoderNeuromuscularData nm = (EncoderNeuromuscularData) model.GetValue (iter, 0);
		(cell as Gtk.CellRendererText).Text = nm.cl_t.ToString();
	}

	private void Render_cl_rfd_avg (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.ITreeModel model, Gtk.TreeIter iter)
	{
		EncoderNeuromuscularData nm = (EncoderNeuromuscularData) model.GetValue (iter, 0);
		(cell as Gtk.CellRendererText).Text = nm.cl_rfd_avg.ToString();
	}

	private void Render_cl_i (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.ITreeModel model, Gtk.TreeIter iter)
	{
		EncoderNeuromuscularData nm = (EncoderNeuromuscularData) model.GetValue (iter, 0);
		(cell as Gtk.CellRendererText).Text = nm.cl_i.ToString();
	}

	private void Render_cl_f_avg (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.ITreeModel model, Gtk.TreeIter iter)
	{
		EncoderNeuromuscularData nm = (EncoderNeuromuscularData) model.GetValue (iter, 0);
		(cell as Gtk.CellRendererText).Text = nm.cl_f_avg.ToString();
	}

	private void Render_cl_vf (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.ITreeModel model, Gtk.TreeIter iter)
	{
		EncoderNeuromuscularData nm = (EncoderNeuromuscularData) model.GetValue (iter, 0);
		(cell as Gtk.CellRendererText).Text = nm.cl_vf.ToString();
	}

	private void Render_cl_f_max (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.ITreeModel model, Gtk.TreeIter iter)
	{
		EncoderNeuromuscularData nm = (EncoderNeuromuscularData) model.GetValue (iter, 0);
		(cell as Gtk.CellRendererText).Text = nm.cl_f_max.ToString();
	}

	private void Render_cl_s_avg (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.ITreeModel model, Gtk.TreeIter iter)
	{
		EncoderNeuromuscularData nm = (EncoderNeuromuscularData) model.GetValue (iter, 0);
		(cell as Gtk.CellRendererText).Text = nm.cl_s_avg.ToString();
	}

	private void Render_cl_s_max (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.ITreeModel model, Gtk.TreeIter iter)
	{
		EncoderNeuromuscularData nm = (EncoderNeuromuscularData) model.GetValue (iter, 0);
		(cell as Gtk.CellRendererText).Text = nm.cl_s_max.ToString();
	}

	private void Render_cl_p_avg (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.ITreeModel model, Gtk.TreeIter iter)
	{
		EncoderNeuromuscularData nm = (EncoderNeuromuscularData) model.GetValue (iter, 0);
		(cell as Gtk.CellRendererText).Text = nm.cl_p_avg.ToString();
	}

	private void Render_cl_p_max (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.ITreeModel model, Gtk.TreeIter iter)
	{
		EncoderNeuromuscularData nm = (EncoderNeuromuscularData) model.GetValue (iter, 0);
		(cell as Gtk.CellRendererText).Text = nm.cl_p_max.ToString();
	}


	/* end of rendering neuromuscular cols */
	
	//check if there are enought cells, sometimes file is created but data is not completely written
	private bool fixDecimalsWillWork(bool captureOrAnalyze, string [] cells)
	{
		LogB.Information(string.Format("captureOrAnalyze: {0}, cells.Length: {1}", captureOrAnalyze, cells.Length));
		//LogB.Information(string.Format("cellsString: {0}", Util.StringArrayToString(cells, ";")));
		if(captureOrAnalyze && cells.Length < 21) 		//from 0 to 20
			return false;
		else if(! captureOrAnalyze && cells.Length < 25) 	//from 0 to 24
			return false;

		return true;
	}
	//captureOrAnalyze is true on capture, false on analyze
	private string [] fixDecimals(bool captureOrAnalyze, string [] cells) 
	{
		LogB.Information("fixDecimals: ");
		LogB.Information(Util.StringArrayToString(cells, ";"));
		//start, width, height
		for(int i=5; i <= 7; i++)
			cells[i] = Util.TrimDecimals(Convert.ToDouble(Util.ChangeDecimalSeparator(cells[i])),1);
		
		//meanSpeed,maxSpeed,maxSpeedT, meanPower,peakPower,peakPowerT
		for(int i=8; i <= 13; i++)
			cells[i] = Util.TrimDecimals(Convert.ToDouble(Util.ChangeDecimalSeparator(cells[i])),3);
		
		//pp/ppt
		int pp_ppt = 14;
		cells[pp_ppt] = Util.TrimDecimals(Convert.ToDouble(Util.ChangeDecimalSeparator(cells[pp_ppt])),1); 

		//meanForce, maxForce, maxForceT
		for(int i=15; i <= 17; i++)
			cells[i] = Util.TrimDecimals(Convert.ToDouble(Util.ChangeDecimalSeparator(cells[i])),3);

		//maxForce_maxForceT
		int maxForce_maxForceT = 18;
		cells[maxForce_maxForceT] = Util.TrimDecimals(Convert.ToDouble(Util.ChangeDecimalSeparator(cells[maxForce_maxForceT])),1);

		LogB.Information("cells19: " + cells[19]);
		LogB.Information("cells20: " + cells[20]);
		//work, impulse
		cells[19] = Util.TrimDecimals(Convert.ToDouble(Util.ChangeDecimalSeparator(cells[19])),3);
		cells[20] = Util.TrimDecimals(Convert.ToDouble(Util.ChangeDecimalSeparator(cells[20])),3);

		//cells[21] laterality

		//capture does not return inerta
		//analyze returns inertia (can be different on "saved curves") comes as Kg*m^2, convert it to Kg*cm^2
		//analyze returns also diameter and equivalentMass (both used on inertial)
		if(! captureOrAnalyze) {
			double inertiaInM = Convert.ToDouble(Util.ChangeDecimalSeparator(cells[22]));
			cells[22] = (Convert.ToInt32(inertiaInM * 10000)).ToString();
			cells[23] = Util.ChangeDecimalSeparator(cells[23]);
			cells[24] = Util.ChangeDecimalSeparator(cells[24]);
		}

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

	private enum AllEccCon { ALL, ECC, CON }

	private ArrayList treeviewEncoderCaptureCurvesGetCurves(AllEccCon option) 
	{
		TreeIter iter;
		ArrayList curves = new ArrayList();
			
		bool iterOk = encoderCaptureListStore.GetIterFirst(out iter);
		if(! iterOk)
			return curves;

		bool oddRow = true;
		while(iterOk) {
			if(ecconLast != "c" && option == AllEccCon.CON && oddRow) {
				oddRow = ! oddRow;
				iterOk = encoderCaptureListStore.IterNext (ref iter);
				continue;
			}
			if(ecconLast != "c" && option == AllEccCon.ECC && ! oddRow) {
				oddRow = ! oddRow;
				iterOk = encoderCaptureListStore.IterNext (ref iter);
				continue;
			}
				
			EncoderCurve curve = (EncoderCurve) encoderCaptureListStore.GetValue (iter, 0);
			curves.Add(curve);

			oddRow = ! oddRow;
			iterOk = encoderCaptureListStore.IterNext (ref iter);
		}

		return curves;
	}
	
	// ---------helpful methods -----------
	
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

	ArrayList getTreeViewNeuromuscular(Gtk.ListStore ls) {
		TreeIter iter = new TreeIter();
		ls.GetIterFirst ( out iter ) ;
		ArrayList array = new ArrayList();
		do {
			EncoderNeuromuscularData nm = (EncoderNeuromuscularData) ls.GetValue (iter, 0);
			array.Add(nm);
		} while (ls.IterNext (ref iter));
		return array;
	}

	/* end of TreeView stuff */	

}
