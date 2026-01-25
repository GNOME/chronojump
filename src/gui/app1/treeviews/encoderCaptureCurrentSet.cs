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
 * Copyright (C) 2004-2026   Xavier de Blas <xaviblas@gmail.com>
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
			"RVD" + "\n (m/s^2)",
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
						cells[8], cells[9], cells[10], cells[11], 	//speeds
						cells[12], cells[13], cells[14], cells[15],	//powers
						cells[16], cells[17], cells[18], cells[19], 	//forces
						cells[20], cells[21] 		//work, impulse
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
					aColumn.SetCellDataFunc (aCell, new Gtk.TreeCellDataFunc (RenderRVD));
					break;
				case 8:
					aColumn.SetCellDataFunc (aCell, new Gtk.TreeCellDataFunc (RenderMeanPower));
					break;
				case 9:
					aColumn.SetCellDataFunc (aCell, new Gtk.TreeCellDataFunc (RenderPeakPower));
					break;
				case 10:
					aColumn.SetCellDataFunc (aCell, new Gtk.TreeCellDataFunc (RenderPeakPowerT));
					break;
				case 11:
					aColumn.SetCellDataFunc (aCell, new Gtk.TreeCellDataFunc (RenderPP_PPT)); //RPD
					break;
				case 12:
					aColumn.SetCellDataFunc (aCell, new Gtk.TreeCellDataFunc (RenderMeanForce));
					break;
				case 13:
					aColumn.SetCellDataFunc (aCell, new Gtk.TreeCellDataFunc (RenderMaxForce));
					break;
				case 14:
					aColumn.SetCellDataFunc (aCell, new Gtk.TreeCellDataFunc (RenderMaxForceT));
					break;
				case 15:
					aColumn.SetCellDataFunc (aCell, new Gtk.TreeCellDataFunc (RenderMaxForce_maxForceT)); //RFD
					break;
				case 16:
					aColumn.SetCellDataFunc (aCell, new Gtk.TreeCellDataFunc (RenderWork));
					break;
				case 17:
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

			ArrayList signalCurves = SqliteEncoderSignalCurve.SelectSignalCurve(dbconOpened,
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

			//on ec, ecS need to [un]select second row
			if (ecconLast == "ec" || ecconLast == "ecS") {
				path.Next();
				encoderCaptureListStore.IterNext (ref iter);

				//change value
				((EncoderCurve) encoderCaptureListStore.GetValue (iter, column)).Record = ! val;

				//this makes RenderRecord work on changed row without having to put mouse there
				encoderCaptureListStore.EmitRowChanged(path,iter);
			}
			
			updateUserCurvesLabelsAndCombo(false);

			callPlotCurvesGraphDoPlot();

			// update the treeviewResultsSession without changing again current set widgets
			// see: diagrams/processes/person_results_changes.dia
			SqliteEncoder se = new SqliteEncoder ();
			treeview_results_session_cursor_changed_block = true; //to block cursor_change on store.Remove ()

			treeViewResultsSession.UpdateReps (
					se.SelectSetsAndRepsLList (
						false, currentPerson.UniqueID, currentSession.UniqueID,
						currentEncoderGI, currentEncoderSQLSet.exerciseID, encoderSignalUniqueID)
					);
			treeview_results_session_cursor_changed_block = false;

			// update the signal graph
			encoder_capture_signal_drawingarea_cairo.QueueDraw ();

			//and the session barplot
			updateGraphResultsSessionByMode ();
		}
	}

	//mainVariable used if saveOption == BEST
	void encoderCaptureSaveCurvesAllNoneBest(Constants.EncoderAutoSaveCurve saveOption, string mainVariable)
	{
		int bestRow = 0;
		int numRows = 0;
		List<int> list_bestN = new List<int>();
		int bestN = preferences.encoderAutoSaveCurveBestNValue;

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
					(! curve.Record && ! thisRowDiscarded && saveOption == Constants.EncoderAutoSaveCurve.BESTN && UtilList.FoundInListInt(list_bestN, i)) ||
					(! curve.Record && ! thisRowDiscarded && saveOption == Constants.EncoderAutoSaveCurve.BESTNCONSECUTIVE &&
					 i >= bestRow && ( (ecconLast == "c" && i < bestRow + bestN) || (ecconLast != "c" && i < bestRow + 2*bestN) )) ||
					(! curve.Record && ! thisRowDiscarded && saveOption == Constants.EncoderAutoSaveCurve.FROM4TOPENULTIMATE && fromValidToPenult) ||
					(curve.Record && (thisRowDiscarded || saveOption == Constants.EncoderAutoSaveCurve.BEST && i != bestRow)) ||
					(curve.Record && (thisRowDiscarded || saveOption == Constants.EncoderAutoSaveCurve.BESTN && ! UtilList.FoundInListInt(list_bestN, i))) ||
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
	void encoderCaptureSelectBySavedCurves (int msCentral, bool selectIt)
	{
		TreeIter iter;
		TreeIter iterPre;
		bool iterOk = encoderCaptureListStore.GetIterFirst(out iter);
		while (iterOk)
		{
			TreePath path = encoderCaptureListStore.GetPath(iter);
			EncoderCurve curve = (EncoderCurve) encoderCaptureListStore.GetValue (iter, 0);
			string eccon = findEcconFromCurrentSet (true);

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
}
