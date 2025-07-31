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
 * Copyright (C) 2004-2024   Xavier de Blas <xaviblas@gmail.com>
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
			"RVD" + "\n (m/s^2)",
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
			exerciseName = currentEncoderSQLSet.ExerciseName;
			totalMass = findDisplacedMassFromSQL ();

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
							cells[22],	//laterality
							Convert.ToDouble(Util.ChangeDecimalSeparator(cells[4])), 	//extraWeight
							totalMass, 							//displaceWeight
							Convert.ToInt32(cells[23]), 					//inertia M. (inertial)
							Convert.ToDouble(cells[24]), 					//diameter (inertial)
							Convert.ToDouble(cells[25]), 					//equivalent mass (inertial)
							cells[5], cells[6], cells[7], 
							cells[8], cells[9], cells[10], cells[11],			//speeds
							cells[12], cells[13], cells[14], cells[15],			//powers
							cells[16], cells[17], cells[18], cells[19],			//forces
							cells[20], cells[21]
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
					aColumn.SetCellDataFunc (aCell, new Gtk.TreeCellDataFunc (RenderRVD));
					break;
				case 16:
					aColumn.SetCellDataFunc (aCell, new Gtk.TreeCellDataFunc (RenderMeanPower));
					break;
				case 17:
					aColumn.SetCellDataFunc (aCell, new Gtk.TreeCellDataFunc (RenderPeakPower));
					break;
				case 18:
					aColumn.SetCellDataFunc (aCell, new Gtk.TreeCellDataFunc (RenderPeakPowerT));
					break;
				case 19:
					aColumn.SetCellDataFunc (aCell, new Gtk.TreeCellDataFunc (RenderPP_PPT));
					break;
				case 20:
					aColumn.SetCellDataFunc (aCell, new Gtk.TreeCellDataFunc (RenderMeanForce));
					break;
				case 21:
					aColumn.SetCellDataFunc (aCell, new Gtk.TreeCellDataFunc (RenderMaxForce));
					break;
				case 22:
					aColumn.SetCellDataFunc (aCell, new Gtk.TreeCellDataFunc (RenderMaxForceT));
					break;
				case 23:
					aColumn.SetCellDataFunc (aCell, new Gtk.TreeCellDataFunc (RenderMaxForce_maxForceT));
					break;
				case 24:
					aColumn.SetCellDataFunc (aCell, new Gtk.TreeCellDataFunc (RenderWork));
					break;
				case 25:
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
}
