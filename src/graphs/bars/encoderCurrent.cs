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
using Gdk; //for the EventMask, RGBA
using System.Collections; //ArrayList
using System.Collections.Generic; //List
using Mono.Unix;


public class CairoPaintBarsPreEncoderCurrent : CairoPaintBarsPre
{
	// without this, while capturing, if screen is minimized/maximized, or any redraw sounds are played again!
	// This is emptied at capture start
	public static List<int> RepetitionsPlayed_l = new List<int> ();

	private PrepareEventGraphEncoderCurrent pegbe;
	private Preferences preferences;

	//copied from gui/encoderGraphObjects (using ArrayList)
	private ArrayList data; //data is related to mainVariable (barplot)
	private List<double> lineData_l; //related to secondary variable (by default range (mm))
	private List<double> dataStart_l; //used on video (in seconds)
	private List<double> dataDuration_l; //used on video (in seconds)
	private ArrayList dataRangeOfMovement; //ROM, need it to discard last rep for loss. Is not the same as lineData_l because maybe user selected another variable as secondary. only checks con.
	private ArrayList dataWorkJ;
	private ArrayList dataImpulse;
	private CairoBarsArrow cairoBarsArrow;

	private double countValid;
	private double sumValid;
	private double sumSaved;
	private int countSaved;
	private double maxThisSetValidAndCon;
	private double minThisSetValidAndCon;
	//we need the position to draw the loss line and maybe to manage that the min should be after the max (for being real loss)
	private int maxThisSetValidAndConPos;
	private int minThisSetValidAndConPos;
	double workTotal; //can be J or kcal (shown in cal)
	double impulseTotal;

	private List<PointF> barA_l; //data is related to mainVariable (barplot)
	private List<PointF> barB_l; //data is related to mainVariable (barplot)
	private List<Cairo.Color> colorMain_l;
	private List<Cairo.Color> colorSecondary_l;
	private List<string> names_l;
	private List<int> saved_l; //saved repetitions
	private List<CairoBarsArrow> eccOverload_l;

	//used on encoder when !relativeToSet
	private double maxAbsoluteForCalc;
	private double maxThisSetForCalc;
	private double maxThisSetSaved; //cb.MaxIntersession will be the greatest of pegbe.maxPowerSpeedForceIntersession and best saved repetition in this set

	private string units;
	private string titleStr;
	private string lossStr;
	private string workStr;
	private string impulseStr;

	private bool noMassAndNeeded;

	//just blank the screen
	public CairoPaintBarsPreEncoderCurrent (DrawingArea darea, string fontStr)
	{
		blankScreen(darea, fontStr);
	}

	//isLastCaptured: if what we are showing is currentJumpRj then true, if is a selection from treeview and id != currentJumpRj then is false (meaning selected)

	public CairoPaintBarsPreEncoderCurrent (Preferences preferences, DrawingArea darea, string fontStr,
			string personName, string testName, int pDN,
			PrepareEventGraphEncoderCurrent pegbe, double videoTime)
	{
		this.pegbe = pegbe;
		this.videoTime = videoTime;

		NewPreferences (preferences);
		//messageNoStoreCreated = " no criteria ";

		initialize (darea, fontStr, mode, personName, testName, pDN);

		if (noMassAndNeededCheck ())
		{
			noMassAndNeeded = true;
			return;
		}

		//calcule all graph stuff
		fillArraysDiscardingReps ();
		fillVariableListsForGraph ();
		prepareTitle ();
		prepareLossArrow ();
	}

	private bool noMassAndNeededCheck ()
	{
		if (! pegbe.hasInertia && pegbe.massDisplaced < 0.00001 &&
				(pegbe.mainVariable != Constants.Range && pegbe.mainVariable != Constants.RangeAbsolute &&
				 pegbe.mainVariable != Constants.MeanSpeed && pegbe.mainVariable != Constants.MaxSpeed) )
				 return true;

		return false;
	}

	public override void ShowMessage (DrawingArea darea, string fontTypeStr, string message)
	{
		if(darea == null)
			return;

		this.darea = darea;
		cb = new CairoBars1Series (darea, CairoBars.Type.ENCODER, fontTypeStr, message);
	}

	protected override bool storeCreated ()
	{
		return (pegbe != null && pegbe.encoderBarsData_l.Count > 0);
	}

	protected override bool haveDataToPlot()
	{
		return (pegbe != null && pegbe.encoderBarsData_l.Count > 0);
	}

	protected override void paintSpecific()
	{
		if (noMassAndNeeded)
			ShowMessage (darea, preferences.fontTypeToGraph(),
					Catalog.GetString("Main variable:") + " " + Catalog.GetString(pegbe.mainVariable) + "\n\n" +
					Catalog.GetString("The bars are not shown because the displaced mass is 0."));
		else
			paintSpecificDo ();
	}

	//preferences can change
	public void NewPreferences (Preferences preferences)
	{
		this.preferences = preferences;

		pegbe.discardFirstN = preferences.encoderCaptureInertialDiscardFirstN;
		pegbe.showNRepetitions = preferences.encoderCaptureShowNRepetitions;
	}

	private void fillArraysDiscardingReps () //copied from gui/encoderGraphObjects fillDataVariables()
	{
		data = new ArrayList (pegbe.encoderBarsData_l.Count); //data is related to mainVariable (barplot)
		lineData_l = new List<double>(); //lineData_l is related to secondary variable (by default range)
		dataStart_l = new List<double> ();
		dataDuration_l = new List<double> ();
		dataRangeOfMovement = new ArrayList (pegbe.encoderBarsData_l.Count);
		dataWorkJ = new ArrayList (pegbe.encoderBarsData_l.Count);
		dataImpulse = new ArrayList (pegbe.encoderBarsData_l.Count);
		bool lastIsEcc = false;
		int count = 0;

		//discard repetitions according to pegbe.showNRepetitions
		foreach(EncoderBarsData ebd in pegbe.encoderBarsData_l)
		{
			//LogB.Information(string.Format("count: {0}, value: {1}", count, ebd.GetValue(pegbe.mainVariable)));
			//when capture ended, show all repetitions
			if(pegbe.showNRepetitions == -1 || ! pegbe.capturing)
			{
				data.Add(ebd.GetValue(pegbe.mainVariable));
				if(pegbe.secondaryVariable != "")
					lineData_l.Add(ebd.GetValue(pegbe.secondaryVariable));
				dataStart_l.Add (UtilAll.DivideSafe (ebd.GetValue (Constants.Start), 1000));
				dataDuration_l.Add (UtilAll.DivideSafe (ebd.GetValue (Constants.Duration), 1000));
				dataRangeOfMovement.Add(ebd.GetValue(Constants.RangeAbsolute));
				dataWorkJ.Add(ebd.GetValue(Constants.WorkJ));
				dataImpulse.Add(ebd.GetValue(Constants.Impulse));
			}
			else {
				if(pegbe.eccon == "c" && ( pegbe.encoderBarsData_l.Count <= pegbe.showNRepetitions || 	//total repetitions are less than show repetitions threshold ||
						count >= pegbe.encoderBarsData_l.Count - pegbe.showNRepetitions ) ) 	//count is from the last group of reps (reps that have to be shown)
				{
					data.Add(ebd.GetValue(pegbe.mainVariable));
					if(pegbe.secondaryVariable != "")
						lineData_l.Add(ebd.GetValue(pegbe.secondaryVariable));
					dataStart_l.Add (UtilAll.DivideSafe (ebd.GetValue (Constants.Start), 1000));
					dataDuration_l.Add (UtilAll.DivideSafe (ebd.GetValue (Constants.Duration), 1000));
					dataRangeOfMovement.Add(ebd.GetValue(Constants.RangeAbsolute));
					dataWorkJ.Add(ebd.GetValue(Constants.WorkJ));
					dataImpulse.Add(ebd.GetValue(Constants.Impulse));
				}
				else if(pegbe.eccon != "c" && (
						pegbe.encoderBarsData_l.Count <= 2 * pegbe.showNRepetitions ||
						count >= pegbe.encoderBarsData_l.Count - 2 * pegbe.showNRepetitions) )
				{
					if(! Util.IsEven(count +1))  	//if it is "impar"
					{
						LogB.Information("added ecc");
						data.Add(ebd.GetValue(pegbe.mainVariable));
						if(pegbe.secondaryVariable != "")
							lineData_l.Add(ebd.GetValue(pegbe.secondaryVariable));
						dataStart_l.Add (UtilAll.DivideSafe (ebd.GetValue (Constants.Start), 1000));
						dataDuration_l.Add (UtilAll.DivideSafe (ebd.GetValue (Constants.Duration), 1000));
						dataRangeOfMovement.Add(ebd.GetValue(Constants.RangeAbsolute));
						dataWorkJ.Add(ebd.GetValue(Constants.WorkJ));
						dataImpulse.Add(ebd.GetValue(Constants.Impulse));
						lastIsEcc = true;
					} else {  			//it is "par"
						if(lastIsEcc)
						{
							data.Add(ebd.GetValue(pegbe.mainVariable));
							if(pegbe.secondaryVariable != "")
								lineData_l.Add(ebd.GetValue(pegbe.secondaryVariable));
							dataStart_l.Add (UtilAll.DivideSafe (ebd.GetValue (Constants.Start), 1000));
							dataDuration_l.Add (UtilAll.DivideSafe (ebd.GetValue (Constants.Duration), 1000));
							dataRangeOfMovement.Add(ebd.GetValue(Constants.RangeAbsolute));
							dataWorkJ.Add(ebd.GetValue(Constants.WorkJ));
							dataImpulse.Add(ebd.GetValue(Constants.Impulse));
							LogB.Information("added con");
							lastIsEcc = false;
						}
					}
				}
			}
			//LogB.Information("data workJ: " + dataWorkJ[count].ToString());
			count ++;
		}
	}

	private void fillVariableListsForGraph ()
	{
		barA_l = new List<PointF>(); //data is related to mainVariable (barplot)
		barB_l = new List<PointF>(); //data is related to mainVariable (barplot)

		colorMain_l = new List<Cairo.Color>();
		colorSecondary_l = new List<Cairo.Color>();
		names_l = new List<string>();
		saved_l = new List<int>();

		//Gdk colors from (soon deleted) encoderGraphDoPlot()
		RGBA colorPhase = new RGBA ();

		//final color of the bar
		Cairo.Color colorBar = new Cairo.Color();

		int count = 0;

		//Get max min avg values of this set
		double maxThisSetForGraph = -100000;
		maxThisSetForCalc = -100000;
		maxThisSetSaved = -100000;
		double minThisSet = 100000;
		/*
		 * if ! Preferences.EncoderPhasesEnum.BOTH, eg: ECC, we can graph max CON (that maybe is the highest value) , but for calculations we want only the max ECC value, so:
		 * maxThisSetForGraph will be to plot the margins,
		 * maxThisSetForCalc will be to calculate feedback (% of max)
		 */

		//only used for loss. For loss only con phase is used
		maxThisSetValidAndCon = maxThisSetForCalc;
		minThisSetValidAndCon = minThisSet;
		//we need the position to draw the loss line and maybe to manage that the min should be after the max (for being real loss)
		maxThisSetValidAndConPos = 0;
		minThisSetValidAndConPos = 0;

		//know not-discarded phases
		countValid = 0;
		sumValid = 0;
		sumSaved = 0;
		countSaved = 0;
		workTotal = 0; //can be J or kcal (shown in cal)
		impulseTotal = 0;

		foreach(double d in data)
		{
			if(d > maxThisSetForGraph)
				maxThisSetForGraph = d;

			if(pegbe.eccon == "c" ||
					preferences.encoderCaptureFeedbackEccon == Preferences.EncoderPhasesEnum.BOTH ||
					preferences.encoderCaptureFeedbackEccon == Preferences.EncoderPhasesEnum.ECC && ! Util.IsEven(count +1) || //odd (impar)
					preferences.encoderCaptureFeedbackEccon == Preferences.EncoderPhasesEnum.CON && Util.IsEven(count +1) ) //even (par)
			{
				if(d > maxThisSetForCalc)
					maxThisSetForCalc = d;
			}

			if(d < minThisSet)
				minThisSet = d;

			if( pegbe.hasInertia && pegbe.discardFirstN > 0 &&
					  ((pegbe.eccon == "c" && count < pegbe.discardFirstN) || (pegbe.eccon != "c" && count < pegbe.discardFirstN * 2)) )
				LogB.Information("Discarded phase");
			else if(pegbe.eccon == "c" ||
					preferences.encoderCaptureFeedbackEccon == Preferences.EncoderPhasesEnum.BOTH ||
					preferences.encoderCaptureFeedbackEccon == Preferences.EncoderPhasesEnum.ECC && ! Util.IsEven(count +1) || //odd (impar)
					preferences.encoderCaptureFeedbackEccon == Preferences.EncoderPhasesEnum.CON && Util.IsEven(count +1) )	//even (par)
			{
				countValid ++;
				sumValid += d;
				bool needChangeMin = false;

				if(pegbe.eccon == "c" || Util.IsEven(count +1)) //par
				{
					if(d > maxThisSetValidAndCon) {
						maxThisSetValidAndCon = d;
						maxThisSetValidAndConPos = count;

						//min rep has to be after max
						needChangeMin = true;
					}
					if(needChangeMin || (d < minThisSetValidAndCon &&
								Convert.ToDouble(dataRangeOfMovement[count]) >= .7 * Convert.ToDouble(dataRangeOfMovement[maxThisSetValidAndConPos])
								//ROM of this rep cannot be lower than 70% of ROM of best rep (helps to filter when you leave the weight on the bar...)
							    ) ) {
						minThisSetValidAndCon = d;
						minThisSetValidAndConPos = count;
					}
				}
			}

			count ++;
		}

		maxAbsoluteForCalc = maxThisSetForCalc;
		//can be on meanPower, meanSpeed, meanForce
		if(! pegbe.relativeToSet)
		{
			//relative to historical of this person

			/*
			 *
			 * if there's a set captured but without repetitions saved, maxPowerSpeedForceIntersession will be 0
			 * and current set (loaded or captured) will have a power that will be out of the graph
			 * for this reason use maxAbsolute or maxThisSet, whatever is higher
			 *
			 * if ! relativeToSet, then Preferences.EncoderPhasesEnum.BOTH, so maxAbsoluteForCalc == maxAbsoluteForGraph
			 */
			if(pegbe.maxPowerSpeedForceIntersession > maxAbsoluteForCalc)
			{
				maxAbsoluteForCalc = pegbe.maxPowerSpeedForceIntersession;
				//maxAbsoluteForGraph = maxPowerSpeedForceIntersession;
			}
		}

		LogB.Information("maxAbsoluteForCalc = " + maxAbsoluteForCalc.ToString());
		pegbe.feedback.ResetBestSetValue(FeedbackEncoder.BestSetValueEnum.CAPTURE_MAIN_VARIABLE);
		pegbe.feedback.UpdateBestSetValue(
				FeedbackEncoder.BestSetValueEnum.CAPTURE_MAIN_VARIABLE, maxAbsoluteForCalc);


		//to show saved curves on DoPlot
		TreeIter iter;
		bool iterOk = pegbe.encoderCaptureListStore.GetIterFirst(out iter);

		//for eccentricOverload
		eccOverload_l = new List<CairoBarsArrow>();
		double concentricPreValue = -1;

		//discard repetitions according to pegbe.showNRepetitions
		//int countToDraw = pegbe.encoderBarsData_l.Count;
		//foreach(EncoderBarsData ebd in pegbe.encoderBarsData_l)
		//for (int count = 0; count < pegbe.encoderBarsData_l.Count; count ++)
//		int countNames = 0;

		//we used data because this array has only the reps not discarded by showNRepetitions
		for (count = 0; count < data.Count ; count ++)
		{
			double mainVariableValue = Convert.ToDouble(data[count]);

			// 1) get phase (for color)
			Preferences.EncoderPhasesEnum phaseEnum = Preferences.EncoderPhasesEnum.BOTH; // (eccon == "c")
			if (pegbe.eccon == "ec" || pegbe.eccon == "ecS") {
				bool isEven = Util.IsEven(count +1); //TODO: check this (as for is reversed)
				if(isEven)
					phaseEnum = Preferences.EncoderPhasesEnum.CON;
				else
					phaseEnum = Preferences.EncoderPhasesEnum.ECC;
			}

			// 2) manage colors for bars. select pen color for bars and sounds
			string myColor = pegbe.feedback.AssignColorAutomatic(
					FeedbackEncoder.BestSetValueEnum.CAPTURE_MAIN_VARIABLE, mainVariableValue, phaseEnum);

			bool discarded = false;
			if(pegbe.hasInertia) {
				if(pegbe.eccon == "c" && pegbe.discardFirstN > 0 && count < pegbe.discardFirstN)
					discarded = true;
				else if(pegbe.eccon != "c" && pegbe.discardFirstN > 0 && count < pegbe.discardFirstN * 2)
					discarded = true;
			}

			if ( ! discarded && ( myColor == UtilGtk.ColorGood || (pegbe.mainVariableHigher != -1 && mainVariableValue >= pegbe.mainVariableHigher) ) )
			{
				colorPhase = UtilGtk.GetRGBA (UtilGtk.Colors.GREEN_PLOTS);
				//play sound if value is high, volumeOn == true, is last value, capturing
				if (pegbe.volumeOn && count == data.Count -1 && pegbe.capturing && ! UtilList.FoundInListInt (RepetitionsPlayed_l, count))
				{
					Util.PlaySound (Constants.SoundTypes.GOOD, preferences.volumeOn, preferences.gstreamer);
					RepetitionsPlayed_l.Add (count);
				}
			}
			else if ( ! discarded && ( myColor == UtilGtk.ColorBad || (pegbe.mainVariableLower != -1 && mainVariableValue <= pegbe.mainVariableLower) ) )
			{
				colorPhase = UtilGtk.GetRGBA (UtilGtk.Colors.RED_PLOTS);
				//play sound if value is low, volumeOn == true, is last value, capturing
				if (pegbe.volumeOn && count == data.Count -1 && pegbe.capturing && ! UtilList.FoundInListInt (RepetitionsPlayed_l, count))
				{
					Util.PlaySound (Constants.SoundTypes.BAD, pegbe.volumeOn, pegbe.gstreamer);
					RepetitionsPlayed_l.Add (count);
				}
			}
			else if(myColor == UtilGtk.ColorGray)
			{
				/*
				 * on ecS when feedback is only in the opposite phase,
				 * AssignColorAutomatic will return ColorGray
				 * this helps to distinguins the phase that we want
				 */
				colorPhase = UtilGtk.GetRGBA (UtilGtk.Colors.GRAY);
			}
			else
				colorPhase = UtilGtk.GetRGBA (UtilGtk.Colors.BLUE_LIGHT);

			//know if ecc or con to paint with dark or light pen
			if (pegbe.eccon == "ec" || pegbe.eccon == "ecS")
			{
				//bool isEven = Util.IsEven(count +1);

				//on inertial if discardFirstN , they have to be gray
				if( pegbe.hasInertia && pegbe.discardFirstN > 0 &&
						((pegbe.eccon == "c" && count < pegbe.discardFirstN) || (pegbe.eccon != "c" && count < pegbe.discardFirstN * 2)) )
					colorBar = CairoGeneric.colorFromRGBA (UtilGtk.GetRGBA (UtilGtk.Colors.GRAY));
				else {
					colorBar = CairoGeneric.colorFromRGBA (colorPhase);
				}
			} else {
				if( pegbe.hasInertia && pegbe.discardFirstN > 0 &&
						((pegbe.eccon == "c" && count < pegbe.discardFirstN) || (pegbe.eccon != "c" && count < pegbe.discardFirstN * 2)) )
					colorBar = CairoGeneric.colorFromRGBA (UtilGtk.GetRGBA (UtilGtk.Colors.GRAY));
				else
					colorBar = CairoGeneric.colorFromRGBA (colorPhase);
			}

			// 3) add data in barA_l, barB_l, names_l and color lists
			if(pegbe.eccon == "c")
			{
				barA_l.Add(new PointF(count +1, mainVariableValue));
				colorMain_l.Add(colorBar);
				names_l.Add((pegbe.encoderBarsData_l.Count -data.Count +(count+1)).ToString());
			} else
			{
				if(! Util.IsEven(count +1))  	//if it is "impar"
				{
					barA_l.Add(new PointF(UtilAll.DivideSafe(count+1,2), mainVariableValue));
					colorSecondary_l.Add(colorBar);
					names_l.Add((UtilAll.DivideSafe(pegbe.encoderBarsData_l.Count -data.Count +count,2)+1).ToString());
				} else {// "par"
					barB_l.Add(new PointF(UtilAll.DivideSafe(count+1,2), mainVariableValue));
					colorMain_l.Add(colorBar);
				}
			}

			// 4) eccentric overload
			//draw green arrow eccentric overload on inertial only if ecc > con
			if (pegbe.hasInertia && preferences.encoderCaptureInertialEccOverloadMode !=
					Preferences.encoderCaptureEccOverloadModes.NOT_SHOW &&
					(pegbe.eccon == "ec" || pegbe.eccon == "ecS"))
			{
				bool isEven = Util.IsEven(count +1);
				if(isEven)
					concentricPreValue = mainVariableValue;
				else if(concentricPreValue >= 0 && mainVariableValue > concentricPreValue)
					eccOverload_l.Add (new CairoBarsArrow(count-1, concentricPreValue, count, mainVariableValue));
			}

			// 5) create saved list: saved_l and add to sumSaved and countSaved for title generation
			if( iterOk && ((EncoderCurve) pegbe.encoderCaptureListStore.GetValue (iter, 0)).Record )
			{
				if(pegbe.eccon == "c" ||
						preferences.encoderCaptureFeedbackEccon == Preferences.EncoderPhasesEnum.BOTH ||
						preferences.encoderCaptureFeedbackEccon == Preferences.EncoderPhasesEnum.ECC && ! Util.IsEven(count +1) || //odd (impar)
						preferences.encoderCaptureFeedbackEccon == Preferences.EncoderPhasesEnum.CON && Util.IsEven(count +1) ) //even (par)
				{
					sumSaved += mainVariableValue;
					countSaved ++;

					if (mainVariableValue > maxThisSetSaved)
						maxThisSetSaved = mainVariableValue;
				}

				if(pegbe.eccon == "c")
					saved_l.Add(count);
				else if(phaseEnum == Preferences.EncoderPhasesEnum.CON)
					saved_l.Add(Convert.ToInt32(Math.Floor(UtilAll.DivideSafe(count, 2))));
			}

			// 6) work and impulse
			if(dataWorkJ.Count > 0)
			{
				if(preferences.encoderWorkKcal)
					workTotal += Convert.ToDouble(dataWorkJ[count]) * 0.000239006;
				else
					workTotal += Convert.ToDouble(dataWorkJ[count]);
			}

			if(dataImpulse.Count > 0)
				impulseTotal += Convert.ToDouble(dataImpulse[count]);

			iterOk = pegbe.encoderCaptureListStore.IterNext (ref iter);
		}

		//if !c && is "impar" (uneven), add a null to B
		if (pegbe.eccon != "c" && ! Util.IsEven(pegbe.encoderBarsData_l.Count))
		{
			barB_l.Add(null);
			colorMain_l.Add(CairoGeneric.colorFromRGBA (UtilGtk.GetRGBA (UtilGtk.Colors.GRAY))); //this color will not be shown is just to match barB_l with colorMain_l
		}
	}

	private void prepareTitle ()
	{
		units = "";
		int decimals;
		if(pegbe.mainVariable == Constants.MeanSpeed || pegbe.mainVariable == Constants.MaxSpeed) {
			units = "m/s";
			decimals = 2;
		} else if(pegbe.mainVariable == Constants.MeanForce || pegbe.mainVariable == Constants.MaxForce) {
			units = "N";
			decimals = 1;
		}
		else { //powers
			units =  "W";
			decimals = 1;
		}

		//LogB.Information(string.Format("sumValid: {0}, countValid: {1}, div: {2}", sumValid, countValid, sumValid / countValid));
		//LogB.Information(string.Format("sumSaved: {0}, countSaved: {1}, div: {2}", sumSaved, countSaved, sumSaved / countSaved));

		//add avg and avg of saved values
		titleStr = Catalog.GetString (pegbe.mainVariable) + " [X: " +
			Util.TrimDecimals( (sumValid / countValid), decimals) +
			" " + units + "; ";

		if(countSaved > 0)
			titleStr += "X" + Catalog.GetString("saved") + ": " +
				Util.TrimDecimals( (sumSaved / countSaved), decimals) +
				" " + units;

		lossStr = "";

		//do not show lossStr on Preferences.EncoderPhasesEnum.ECC
		if( pegbe.showLoss && (pegbe.eccon == "c" || preferences.encoderCaptureFeedbackEccon != Preferences.EncoderPhasesEnum.ECC) )
		{
			titleStr += "; ";
			lossStr = "Loss: ";
			if(pegbe.eccon != "c")
				lossStr = "Loss (con): "; //on ecc/con use only con for loss calculation

			if(maxThisSetValidAndCon > 0)
			{
				lossStr += Util.TrimDecimals(
						100.0 * (maxThisSetValidAndCon - minThisSetValidAndCon) / maxThisSetValidAndCon, decimals) + "%";
				//LogB.Information(string.Format("Loss at plot: {0}", 100.0 * (maxThisSetValidAndCon - minThisSetValidAndCon) / maxThisSetValidAndCon));
			}
		}

		//work and impulse are in separate string variables because maybe we will select to show one or the other
		//work
		workStr = "]    " + Catalog.GetString("Work") + ": " + Util.TrimDecimals(workTotal, decimals);
		if(preferences.encoderWorkKcal)
			workStr += " kcal";
		else
			workStr += " J";

		//impulse
		impulseStr = "    " + Catalog.GetString("Impulse") + ": " + Util.TrimDecimals(impulseTotal, decimals) + " N*s";
	}

	private void prepareLossArrow ()
	{
		cairoBarsArrow = null;
		if(pegbe.showLoss && (pegbe.eccon == "c" || preferences.encoderCaptureFeedbackEccon != Preferences.EncoderPhasesEnum.ECC) )
		{
			if(maxThisSetValidAndCon > 0 && maxThisSetValidAndConPos < minThisSetValidAndConPos)
				cairoBarsArrow = new CairoBarsArrow(maxThisSetValidAndConPos, maxThisSetValidAndCon,
						minThisSetValidAndConPos, minThisSetValidAndCon);
		}
	}

	private void paintSpecificDo ()
	{
		CairoGeneric.MouseClickable mc = CairoGeneric.MouseClickable.NO;
		if (! pegbe.capturing)
			mc = CairoGeneric.MouseClickable.CLICKL;

		if(pegbe.eccon == "c")
			cb = new CairoBars1Series (darea, CairoBars.Type.ENCODER, mc, false, false);
		else
			cb = new CairoBarsNHSeries (darea, CairoBars.Type.ENCODER, false, mc, false, false);

		//LogB.Information("data_l.Count: " + data_l.Count.ToString());
		//cb.GraphInit(fontStr, true, false); //usePersonGuides, useGroupGuides
		cb.GraphInit(fontStr, false, false); //usePersonGuides, useGroupGuides

		int decs;
		if(pegbe.mainVariable == Constants.MeanSpeed || pegbe.mainVariable == Constants.MaxSpeed)
			decs = 2;
		else if(pegbe.mainVariable == Constants.MeanForce || pegbe.mainVariable == Constants.MaxForce)
			decs = 0;
		else //powers
			decs = 0;
		cb.Decs = decs;

		if(cairoBarsArrow != null)
			cb.PassArrowData (cairoBarsArrow);

		if(lineData_l.Count > 0)
		{
			if (! preferences.encoderCaptureSecondaryVariableYAxisCustom)
				cb.Cbsld = new CairoBarsSecondaryLineData (
						lineData_l,
						-1,
						-1,
						pegbe.secondaryVariable);
			else
				cb.Cbsld = new CairoBarsSecondaryLineData (
						lineData_l,
						preferences.encoderCaptureSecondaryVariableYAxisCustomMax,
						preferences.encoderCaptureSecondaryVariableYAxisCustomMin,
						pegbe.secondaryVariable);
		}

		if(eccOverload_l != null && eccOverload_l.Count > 0)
		{
			cb.EccOverload_l = eccOverload_l;
			if (preferences.encoderCaptureInertialEccOverloadMode ==
					Preferences.encoderCaptureEccOverloadModes.SHOW_LINE_AND_PERCENT)
				cb.EccOverloadWriteValue = true;
		}

		if(saved_l.Count > 0)
			cb.Saved_l = saved_l;

		if(! pegbe.relativeToSet)
		{
			//if (maxThisSetSaved >= pegbe.maxPowerSpeedForceIntersession)
			//{
			//	cb.MaxIntersession = maxThisSetSaved;
				//cb.MaxIntersessionValueStr = "";//Util.TrimDecimals (maxThisSetSaved, decs) + " " + units;
				//cb.MaxIntersessionDate = "";
			//} else {
				cb.MaxIntersession = pegbe.maxPowerSpeedForceIntersession;
				//cb.MaxIntersessionValueStr = Util.TrimDecimals(pegbe.maxPowerSpeedForceIntersession, decs) + " " + units;
				//cb.MaxIntersessionDate = pegbe.maxPowerSpeedForceIntersessionDate;
			//}
			cb.MaxIntersessionEcconCriteria = preferences.GetEncoderRepetitionCriteria (pegbe.hasInertia);
		}

		//this should be passed before PassData1Serie && PassData2Series
		cb.SetEncoderTitle (titleStr, lossStr, workStr, impulseStr);

		if(pegbe.eccon == "c")
			cb.PassData1Serie (barA_l,
					colorMain_l, names_l,
					new List<List<double>> (),
					preferences.encoderCaptureBarplotFontSize, 14, 8, "",
					new List<int> (), new List<int> (), CairoBars.BarsOrPoints.BARS);
		else {
			List<List<PointF>> barsSecondary_ll = new List<List<PointF>>();
			barsSecondary_ll.Add(barA_l);

			cb.PassData2Series (barB_l, barsSecondary_ll, false,
					colorMain_l, colorSecondary_l, names_l,
					"Ecc",// "Con",
					false,
					preferences.encoderCaptureBarplotFontSize, 14, 8, "",
					new List<int> (), new List<int> (), CairoBars.BarsOrPoints.BARS);
		}

		if (videoTime > 0)
		{
			cb.VideoPlayTimeInSeconds = videoTime;
			cb.VideoPlayTimes_l = dataStart_l;

			//TODO: used dataStart_l and dataDuration_l
		}

		passDataForScreenshotIfNeeded ();

		cb.GraphDo();
	}
}

