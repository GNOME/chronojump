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

		if (current_mode == Constants.Modes.POWERGRAVITATORY)
		{
			cell.Visible = true;
			return;
		}

		// current_mode == Constants.Modes.POWERINERTIAL
		// on inertial show only crt if has not to be discarded

		string pathString = encoderCaptureListStore.GetPath(iter).ToString ();
                string [] myStrFull = pathString.Split(new char[] {':'});

		int inertialStart = preferences.encoderCaptureInertialDiscardFirstN;
		if (ecconLast != "c")
			inertialStart *= 2;

		cell.Visible = (myStrFull.Length > 0 && Util.IsNumber (myStrFull[0], false) && Convert.ToInt32 (myStrFull[0]) >= inertialStart);
	}

	private void RenderN (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.ITreeModel model, Gtk.TreeIter iter)
	{
		//do this in order to have ecconLast useful for RenderN when capturing
		if(capturingCsharp == encoderCaptureProcess.CAPTURING)
			ecconLast = findEcconFromGui (false);

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
		else if (ecconLast == "ec" || ecconLast == "ecS")
		{
			bool isEven = Util.IsEven(Convert.ToInt32(curve.N));
			
			string phase = "e";
			if(isEven)
				phase = "c";
				
			(cell as Gtk.CellRendererText).Text = 
				decimal.Truncate((Convert.ToInt32(curve.N) +1) /2).ToString() + phase;
		} else 
		{	//(ecconLast == "ce" || ecconLast == "ceS")
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
		
		if(radio_encoder_analyze_individual_current_set.Active && findEcconFromGui (false) == "ecS")
		{
			string phase = "e";
			bool isEven = Util.IsEven(Convert.ToInt32(curve.N));
			if(isEven)
				phase = "c";

			(cell as Gtk.CellRendererText).Text = 
				decimal.Truncate((Convert.ToInt32(curve.N) +1) /2).ToString() + phase;
		}
		else if(radio_encoder_analyze_individual_current_set.Active && findEcconFromGui (false) == "ceS")
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

		string str = String.Format(UtilGtk.TVNumPrint(curve.ExtraWeight.ToString(),3,2),Convert.ToDouble(curve.ExtraWeight));

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
			str = String.Format(UtilGtk.TVNumPrint(curve.DisplacedWeight.ToString(),3,2),Convert.ToDouble(curve.DisplacedWeight));
		
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

	private void RenderRVD (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.ITreeModel model, Gtk.TreeIter iter)
	{
		EncoderCurve curve = (EncoderCurve) model.GetValue (iter, 0);
		string str = String.Format(UtilGtk.TVNumPrint(curve.RVD,6,3), curve.RVD);
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
		if(captureOrAnalyze && cells.Length < 22) 		//from 0 to 21
			return false;
		else if(! captureOrAnalyze && cells.Length < 26) 	//from 0 to 25
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
		
		//meanSpeed,maxSpeed,maxSpeedT,rvd, meanPower,peakPower,peakPowerT
		for(int i=8; i <= 14; i++)
			cells[i] = Util.TrimDecimals(Convert.ToDouble(Util.ChangeDecimalSeparator(cells[i])),3);
		
		//pp/ppt
		int pp_ppt = 15;
		cells[pp_ppt] = Util.TrimDecimals(Convert.ToDouble(Util.ChangeDecimalSeparator(cells[pp_ppt])),1); 

		//meanForce, maxForce, maxForceT
		for(int i=16; i <= 18; i++)
			cells[i] = Util.TrimDecimals(Convert.ToDouble(Util.ChangeDecimalSeparator(cells[i])),3);

		//maxForce_maxForceT
		int maxForce_maxForceT = 19;
		cells[maxForce_maxForceT] = Util.TrimDecimals(Convert.ToDouble(Util.ChangeDecimalSeparator(cells[maxForce_maxForceT])),1);

		LogB.Information("cells20: " + cells[20]);
		LogB.Information("cells21: " + cells[21]);
		//work, impulse
		cells[20] = Util.TrimDecimals(Convert.ToDouble(Util.ChangeDecimalSeparator(cells[20])),3);
		cells[21] = Util.TrimDecimals(Convert.ToDouble(Util.ChangeDecimalSeparator(cells[21])),3);

		//cells[22] laterality

		//capture does not return inerta
		//analyze returns inertia (can be different on "saved curves") comes as Kg*m^2, convert it to Kg*cm^2
		//analyze returns also diameter and equivalentMass (both used on inertial)
		if(! captureOrAnalyze) {
			double inertiaInM = Convert.ToDouble(Util.ChangeDecimalSeparator(cells[23]));
			cells[23] = (Convert.ToInt32(inertiaInM * 10000)).ToString();
			cells[24] = Util.ChangeDecimalSeparator(cells[24]);
			cells[25] = Util.ChangeDecimalSeparator(cells[25]);
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
