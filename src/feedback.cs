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
 * Copyright (C) 2023   Xavier de Blas <xaviblas@gmail.com>
 */

using System;

public abstract class Feedback
{
	protected Cairo.Color mainGreen;
	protected Cairo.Color secondaryGreen;
	protected Cairo.Color mainRed;
	protected Cairo.Color secondaryRed;
	protected Preferences preferences;

	protected void setBarColors ()
	{
		//green
		mainGreen = CairoGeneric.colorFromRGBA (UtilGtk.GetRGBA (UtilGtk.Colors.GREEN_DARK));
		secondaryGreen = CairoGeneric.colorFromRGBA (UtilGtk.GetRGBA (UtilGtk.Colors.GREEN_PLOTS));
		if (! UtilGtk.ColorIsDark (Config.ColorBackground))
		{
			mainGreen = CairoGeneric.colorFromRGBA (UtilGtk.GetRGBA (UtilGtk.Colors.GREEN_PLOTS));
			secondaryGreen = CairoGeneric.colorFromRGBA (UtilGtk.GetRGBA (UtilGtk.Colors.GREEN_DARK));
		}

		//red
		mainRed = CairoGeneric.colorFromRGBA (UtilGtk.GetRGBA (UtilGtk.Colors.RED_DARK));
		secondaryRed = CairoGeneric.colorFromRGBA (UtilGtk.GetRGBA (UtilGtk.Colors.RED_PLOTS));
		if (! UtilGtk.ColorIsDark (Config.ColorBackground))
		{
			mainRed = CairoGeneric.colorFromRGBA (UtilGtk.GetRGBA (UtilGtk.Colors.RED_PLOTS));
			secondaryRed = CairoGeneric.colorFromRGBA (UtilGtk.GetRGBA (UtilGtk.Colors.RED_DARK));
		}
	}
}

public class FeedbackJumpsRj : Feedback
{
	public FeedbackJumpsRj (Preferences preferences)
	{
		this.preferences = preferences; //TODO: check if this has to be updated also on other public calls
		setBarColors ();
	}

	public bool TvGreen (double tv)
	{
		return (preferences.jumpsRjFeedbackTvGreaterActive && tv >= preferences.jumpsRjFeedbackTvGreater);
	}

	public bool TvRed (double tv)
	{
		return (preferences.jumpsRjFeedbackTvLowerActive && tv <= preferences.jumpsRjFeedbackTvLower);
	}

	public bool TcGreen (double tc)
	{
		return (preferences.jumpsRjFeedbackTcLowerActive && tc <= preferences.jumpsRjFeedbackTcLower);
	}

	public bool TcRed (double tc)
	{
		return (preferences.jumpsRjFeedbackTcGreaterActive && tc >= preferences.jumpsRjFeedbackTcGreater);
	}

	public Cairo.Color AssignColorMain (double tv)
	{
		if (TvGreen (tv))
			return (mainGreen);
		else if (TvRed (tv))
			return (mainRed);
		else
			return (CairoGeneric.colorFromRGBA (Config.ColorBackground));
	}

	public Cairo.Color AssignColorSecondary (double tc)
	{
		if (TcGreen (tc))
			return (secondaryGreen);
		else if (TcRed (tc))
			return (secondaryRed);
		else
			return (CairoGeneric.colorFromRGBA (UtilGtk.GetColorShifted
						(Config.ColorBackground, ! UtilGtk.ColorIsDark (Config.ColorBackground))));
	}

	public bool EmphasizeBestTvTc
	{
		get { return preferences.jumpsRjFeedbackShowBestTvTc; }
	}
	public bool EmphasizeWorstTvTc
	{
		get { return preferences.jumpsRjFeedbackShowWorstTvTc; }
	}
}

public class FeedbackRunsInterval : Feedback
{
	public FeedbackRunsInterval (Preferences preferences)
	{
		this.preferences = preferences; //TODO: check if this has to be updated also on other public calls
		setBarColors ();
	}

	public bool Green (double speed, double time)
	{
		if (preferences.runsIFeedbackSpeedGreaterActive && speed >= preferences.runsIFeedbackSpeedGreater)
			return (true);
		else if (preferences.runsIFeedbackTimeLowerActive && time <= preferences.runsIFeedbackTimeLower)
			return (true);

		return false;
	}

	public bool Red (double speed, double time)
	{
		if (preferences.runsIFeedbackSpeedLowerActive && speed <= preferences.runsIFeedbackSpeedLower)
			return (true);
		else if (preferences.runsIFeedbackTimeGreaterActive && time >= preferences.runsIFeedbackTimeGreater)
			return (true);

		return false;
	}

	public Cairo.Color AssignColorMain (double speed, double time)
	{
		if (Green (speed, time))
			return (mainGreen);
		else if (Red (speed, time))
			return (mainRed);
		else
			return (CairoGeneric.colorFromRGBA (Config.ColorBackground));
	}

	public bool EmphasizeBestSpeed
	{
		get { return preferences.runsIFeedbackShowBestSpeed; }
	}
	public bool EmphasizeWorstSpeed
	{
		get { return preferences.runsIFeedbackShowWorstSpeed; }
	}
	public bool EmphasizeBestTime
	{
		get { return preferences.runsIFeedbackShowBest; }
	}
	public bool EmphasizeWorstTime
	{
		get { return preferences.runsIFeedbackShowWorst; }
	}
}

/* This class manages feedback colors in encoder
   this behaviour was done previously on gui/feedback
   */
public class FeedbackEncoder
{
	public enum BestSetValueEnum { CAPTURE_MAIN_VARIABLE, AUTOMATIC_FEEDBACK}
	private double bestSetValueCaptureMainVariable;
	private double bestSetValueAutomaticFeedback;
	private Preferences preferences;

	public FeedbackEncoder (Preferences preferences)
	{
		this.preferences = preferences; //TODO: check if this has to be updated also on other public calls

		bestSetValueCaptureMainVariable = 0;
	}

	public void ResetBestSetValue (BestSetValueEnum b)
	{
		if(b == BestSetValueEnum.AUTOMATIC_FEEDBACK)
			bestSetValueAutomaticFeedback = 0;
		else	// b == BestSetValueEnum.CAPTURE_MAIN_VARIABLE
			bestSetValueCaptureMainVariable = 0;
	}

	public void UpdateBestSetValue (EncoderCurve curve)
	{
		BestSetValueEnum b = BestSetValueEnum.AUTOMATIC_FEEDBACK;
		string encoderVar = getMainVariable();
		if(preferences.encoderCaptureMainVariableGreaterActive || preferences.encoderCaptureMainVariableLowerActive)
		{
			if(encoderVar == Constants.MeanSpeed)
				UpdateBestSetValue(b, curve.MeanSpeedD);
			else if(encoderVar == Constants.MaxSpeed)
				UpdateBestSetValue(b, curve.MaxSpeedD);
			else if(encoderVar == Constants.MeanPower)
				UpdateBestSetValue(b, curve.MeanPowerD);
			else if(encoderVar == Constants.PeakPower)
				UpdateBestSetValue(b, curve.PeakPowerD);
			else if(encoderVar == Constants.MeanForce)
				UpdateBestSetValue(b, curve.MeanForceD);
			else if(encoderVar == Constants.MaxForce)
				UpdateBestSetValue(b, curve.MaxForceD);
		}
	}
	public void UpdateBestSetValue(BestSetValueEnum b, double d)
	{
		if(b == BestSetValueEnum.AUTOMATIC_FEEDBACK)
		{
			if(d > bestSetValueAutomaticFeedback)
				bestSetValueAutomaticFeedback = d;
		} else
		{ 	// b == BestSetValueEnum.CAPTURE_MAIN_VARIABLE
			if(d > bestSetValueCaptureMainVariable)
				bestSetValueCaptureMainVariable = d;
		}
	}

	//called from gui/encoderTreeviews.cs
	public string AssignColorAutomatic(BestSetValueEnum b, EncoderCurve curve, string variable, Preferences.EncoderPhasesEnum phaseEnum)
	{
		if(getMainVariable() != variable)
			return UtilGtk.ColorNothing;

		double currentValue = curve.GetParameter(variable);

		return AssignColorAutomatic(b, currentValue, phaseEnum);
	}
	//called from previous function, gui/encoder.cs plotCurvesGraphDoPlot
	public string AssignColorAutomatic(BestSetValueEnum b, double currentValue, Preferences.EncoderPhasesEnum phaseEnum)
	{
		//trying same than gui/feedback
		// 1) assign radios
		bool radio_ecc = false;
		bool radio_con = false;
		//bool radio_both = false;
		if(preferences.encoderCaptureMainVariableThisSetOrHistorical || ( 
					preferences.encoderCaptureMainVariable != Constants.EncoderVariablesCapture.MeanPower &&
					preferences.encoderCaptureMainVariable != Constants.EncoderVariablesCapture.MeanSpeed &&
					preferences.encoderCaptureMainVariable != Constants.EncoderVariablesCapture.MeanForce ))
		{
			if(preferences.encoderCaptureFeedbackEccon == Preferences.EncoderPhasesEnum.CON)
				radio_con = true;
			else if(preferences.encoderCaptureFeedbackEccon == Preferences.EncoderPhasesEnum.ECC)
				radio_ecc = true;
			//else
			//	radio_both = true; //unused
		} //else
		//	radio_both = true; ///unused

		// 2) assign radios
		//note on "c" phaseEnum will be BOTH
		if(radio_ecc && phaseEnum == Preferences.EncoderPhasesEnum.CON)
			return UtilGtk.ColorGray;
		else if(radio_con && phaseEnum == Preferences.EncoderPhasesEnum.ECC)
			return UtilGtk.ColorGray;

		if(preferences.encoderCaptureMainVariableGreaterActive && currentValue > getBestSetValue(b) * preferences.encoderCaptureMainVariableGreaterValue / 100)
			return UtilGtk.ColorGood;
		else if (preferences.encoderCaptureMainVariableLowerActive && currentValue < getBestSetValue(b) * preferences.encoderCaptureMainVariableLowerValue/ 100)
			return UtilGtk.ColorBad;

		return UtilGtk.ColorNothing;
	}

	private string getMainVariable ()
	{
		return Constants.GetEncoderVariablesCapture(preferences.encoderCaptureMainVariable);
	}

	private double getBestSetValue (BestSetValueEnum b)
	{
		if(b == BestSetValueEnum.AUTOMATIC_FEEDBACK)
			return bestSetValueAutomaticFeedback;
		else	// b == BestSetValueEnum.CAPTURE_MAIN_VARIABLE
			return bestSetValueCaptureMainVariable;
	}

}

