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
using Gtk;
using System.Collections.Generic; //List
using Mono.Unix;


public class CairoPaintBarsPreEncoderSession : CairoPaintBarsPre
{
	private bool showPersonName;
	private Constants.EncoderVariablesCapture encoderCaptureMainVariable;

	public CairoPaintBarsPreEncoderSession (DrawingArea darea, string fontStr, Constants.Modes mode,
			string personName, string testName, int pDN, bool showPersonName, //if personName == "" then is all persons
			int currentPersonID,
			Constants.EncoderVariablesCapture encoderCaptureMainVariable,
			bool drawBars)
	{
		initialize (darea, fontStr, mode, personName, testName, pDN);

		this.title = generateTitle();
		this.showPersonName = showPersonName;
		this.currentPersonID = currentPersonID;
		this.encoderCaptureMainVariable = encoderCaptureMainVariable;
		this.drawBars = drawBars;
	}

	public override void StoreEventGraphEncoderSession (PrepareEventGraphEncoderSession eventGraph)
	{
		this.eventGraphEncoderSessionStored = eventGraph;
	}

	protected override bool storeCreated ()
	{
		return (eventGraphEncoderSessionStored != null);
	}

	protected override bool haveDataToPlot()
	{
		return (eventGraphEncoderSessionStored.rowsAtSQL.Count > 0);
	}

	protected override void paintSpecific()
	{
		cb = new CairoBars1Series (darea, CairoBars.Type.NORMAL, CairoGeneric.MouseClickable.CLICKLR, true, CairoBars.PaintGridEnum.ALL);

		cb.YVariable = Catalog.GetString (Constants.GetEncoderVariablesCapture (encoderCaptureMainVariable));
		cb.YUnits = Constants.GetEncoderVariablesCaptureUnits (encoderCaptureMainVariable);
		cb.XVariable = Catalog.GetString (eventGraphEncoderSessionStored.OrderX.ToString ());

		//cb.GraphInit(fontStr, ! ShowPersonNames, true); //usePersonGuides, useGroupGuides
		cb.GraphInit(fontStr, true, true); //usePersonGuides, useGroupGuides

		List<Event> events = EncoderSQL.EncoderSQLListToEventList (eventGraphEncoderSessionStored.rowsAtSQL);

		List<PointF> point_l = new List<PointF>();
		List<string> names_l = new List<string>();
		List<double> color_l = new List<double>();
		List<bool> personIcon_l = new List<bool>();
		List<int> id_l = new List<int>(); //the uniqueIDs for knowing them on bar selection

		calculateBottomParams (events, true, "", "", false, eventGraphEncoderSessionStored.exerciseAll);

		int countToDraw = eventGraphEncoderSessionStored.rowsAtSQL.Count;
		foreach (EncoderSQL eSQL in eventGraphEncoderSessionStored.rowsAtSQL)
		{
			// 1) Add data
			//point_l.Add(new PointF(countToDraw --, UtilAll.DivideSafe (fp.TotalMs, 1000)));
			point_l.Add (new PointF (countToDraw --, eSQL.GetVariable (encoderCaptureMainVariable)));

			// 2) Add bottom names
			string typeRowString = "";
			if (eventGraphEncoderSessionStored.exerciseAll) //if "all tests" show type
				typeRowString = eSQL.ExerciseName;// + "\n" + string.Format ("{0} kg", eSQL.extraWeight);
			//if (eventGraphEncoderSessionStored.type == "")
			//	typeRowString = jump.Type;

			// show extraWeight, but not on inertial
			string extraWeightStr = "";
			if (mode == Constants.Modes.POWERGRAVITATORY)
				extraWeightStr = string.Format ("{0} kg", Util.TrimDecimals (eSQL.extraWeightD, 2));

			names_l.Add (createTextBelowBar(
						extraWeightStr,
						typeRowString,
						eSQL.Description, //person name
						false, false,
						longestWord.Length, maxRowsForText));

			id_l.Add (eSQL.UniqueID);
			color_l.Add (eSQL.extraWeightD);

			personIcon_l.Add (personName == "" && currentPersonID >= 0 && eSQL.PersonID == currentPersonID);

			//if (eventGraphEncoderSessionStored.selectedID == eSQL.UniqueID)
			//	cb.SelectedPos = eventGraphEncoderSessionStored.rowsAtSQL.Count -countToDraw -1;
			if (UtilList.FoundInListInt (eventGraphEncoderSessionStored.selectedRepID_l, eSQL.UniqueID))
				cb.SelectedPos_l.Add (eventGraphEncoderSessionStored.rowsAtSQL.Count -countToDraw -1);
		}
		cb.Id_l = id_l;
		cb.Color_l = color_l;
		cb.PersonIcon_l = personIcon_l;

		if (eventGraphEncoderSessionStored.HistoricalExStr != "")
		{
			cb.BestPersonExHistoricalD = eventGraphEncoderSessionStored.HistoricalExD;
			cb.BestPersonExHistoricalStr = eventGraphEncoderSessionStored.HistoricalExStr;
			LogB.Information ("HistoricalExStr: " + eventGraphEncoderSessionStored.HistoricalExStr);
		}

		cb.PassBoxplots (eventGraphEncoderSessionStored.BoxplotPerson, eventGraphEncoderSessionStored.BoxplotSession);

		// add the yellow points on boxplot (can be n reps of the set)
		if (eventGraphEncoderSessionStored.selectedEvent_l != null && eventGraphEncoderSessionStored.selectedEvent_l.Count > 0)
			foreach (EncoderSQL eSQL in eventGraphEncoderSessionStored.selectedEvent_l)
				cb.SelectedDouble_l.Add (eSQL.GetVariable (encoderCaptureMainVariable));

		cb.PassData1Serie (point_l,
				new List<Cairo.Color>(), names_l,
				new List<List<double>> (),
				-1, fontHeightForBottomNames, bottomMargin, title,
				new List<int> (), new List<int> (), barsOrPoints);

		passDataForScreenshotIfNeeded ();

		cb.GraphDo();
	}

	// to show historic data even if in this session user has not data on that ex.
	protected override double getHistoricD ()
	{
		return eventGraphEncoderSessionStored.HistoricalExD;
	}
	protected override string getHistoricStr ()
	{
		if (eventGraphEncoderSessionStored.HistoricalExStr == "")
			return "";
		else
			return eventGraphEncoderSessionStored.HistoricalExStr;
	}
}
