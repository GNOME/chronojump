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
 *  Copyright (C) 2004-2022   Xavier de Blas <xaviblas@gmail.com>
 */

using System;
using System.Collections.Generic; //List
using Cairo;
using Mono.Unix;

public class JumpsProfileIndex
{
	public string name;

	public enum ErrorCodes { NEEDJUMP, NEGATIVE, DJATOOLOW, NONE_OK }
	public ErrorCodes errorCode;
	public string Text;
	public Cairo.Color Color;
	public string ErrorMessage;
	
	public enum Types { FMAX, FEXPL, CELAST, CARMS, FREACT }   
	public Types type; 

	public double Result;

	public JumpsProfileIndex (Types type, string jumpHigherName, string jumpLowerName, double higher, double lower, double max)
	{
		//colour palette: http://www.colourlovers.com/palette/4286628/Tabor_1
		this.type = type;
		switch(type) {
			case Types.FMAX:
				Text = "% Maximum Force  SJl100% / max";
				Color = colorFromRGB(101,86,67);
				break;
			case Types.FEXPL:
				Text = "% Explosive Force  (SJ - SJl100%) / max";
				Color = colorFromRGB(209,63,58);
				break;
			case Types.CELAST:
				Text = "% Elastic  (CMJ - SJ) / max";
				Color = colorFromRGB(255,152,68);
				break;
			case Types.CARMS:
				Text = "% Arms  (ABK - CMJ) / max";
				Color = colorFromRGB(141,237,78);
				break;
			case Types.FREACT:
				Text = "% Reactive-reflex  (DJa - ABK) / max";
				Color = colorFromRGB(133,190,199);
				break;
			default:
				Text = "% Maximum Force  SJl100% / max";
				Color = colorFromRGB(101,86,67);
				break;
		}
		
		ErrorMessage = "";
		Result = calculateIndex(type, higher, lower, max);

		LogB.Information("errorCode: " + errorCode.ToString());
		if(errorCode == ErrorCodes.NEEDJUMP)
			ErrorMessage = Catalog.GetString("Need to execute jump/s"); //TODO: write which jumps
		else if(errorCode == ErrorCodes.NEGATIVE)
			ErrorMessage = string.Format(Catalog.GetString("Negative index: {0} is higher than {1}"), jumpLowerName, jumpHigherName);
		//else if(errorCode == ErrorCodes.DJATOOLOW)
		//	ErrorMessage = string.Format(Catalog.GetString("Jump {0} too low"), "Dja");
	}

	private double calculateIndex (Types type, double higher, double lower, double max)
	{
		errorCode = ErrorCodes.NONE_OK;

		if(max == 0 || higher == 0) {
			errorCode = ErrorCodes.NEEDJUMP;
			return 0;
		}

		if(type == Types.FMAX)	//this index only uses higher
			return higher / max;

		if(lower == 0) {
			errorCode = ErrorCodes.NEEDJUMP;
			return 0;
		}

		if(lower > higher) {
			errorCode = ErrorCodes.NEGATIVE;
			//return 0;
		}

		return (higher - lower) / max;
	}
	
	private Cairo.Color colorFromRGB(int red, int green, int blue) {
		return new Cairo.Color(red/256.0, green/256.0, blue/256.0);
	}

}

public class JumpsProfile
{
	public enum YesNo { YES, NO }
	public List<YesNo> JumpsDone;
	public bool AllJumpsDone;
	public string ErrorSJl;

	private JumpsProfileIndex jpi0;
	private JumpsProfileIndex jpi1;
	private JumpsProfileIndex jpi2;
	private JumpsProfileIndex jpi3;
	private JumpsProfileIndex jpi4;

	//sjl calc related
	private enum SJLEnum { SJL100EXISTS, SJL100CALCNEEDTWOSJLDIFFW, SJL100CALCNEEDSJ,
		SJL100CALCPROBLEMS, SJL100CALCNEGATIVE, SJL100CALCOK }
	private SJLEnum sjlEnum;
	private double initialSpeedAt100PercentWeight;

	public JumpsProfile() {
	}

	public void Calculate (int personID, int sessionID)
	{
		List<Double> l = SqliteJump.SelectChronojumpProfile(personID, sessionID);

		double sj  = l[0];
		double sjl = l[1];
		double cmj = l[2];
		double abk = l[3];
		double dja = l[4];

		ErrorSJl = "";
		if (sjl > 0)
			sjlEnum = SJLEnum.SJL100EXISTS;
		else {
			initialSpeedAt100PercentWeight = 0;
			sjlEnum = calculateSjl100 (personID, sessionID, sj);

			if (sjlEnum == SJLEnum.SJL100CALCOK)
				sjl = UtilAll.DivideSafe (2 * initialSpeedAt100PercentWeight, 9.81);
		}
		LogB.Information ("sjlEnum = " + sjlEnum.ToString ());
		
		double maxJump = MathUtil.GetMax (new List<double> { sj, sjl, cmj, abk, dja});

		jpi0 = new JumpsProfileIndex(JumpsProfileIndex.Types.FMAX, "SJ", "", sjl, 0, maxJump);
		jpi1 = new JumpsProfileIndex(JumpsProfileIndex.Types.FEXPL, "SJ", "SJl", sj, sjl, maxJump);
		jpi2 = new JumpsProfileIndex(JumpsProfileIndex.Types.CELAST, "CMJ", "SJ", cmj, sj, maxJump);
		jpi3 = new JumpsProfileIndex(JumpsProfileIndex.Types.CARMS, "ABK", "CMJ", abk, cmj, maxJump);
		jpi4 = new JumpsProfileIndex(JumpsProfileIndex.Types.FREACT, "DJa", "ABK", dja, abk, maxJump);

		fillListJumpsDone(sj, sjl, cmj, abk, dja);
	}

	private SJLEnum calculateSjl100 (int personID, int sessionID, double sj)
	{
		initialSpeedAt100PercentWeight = 0;

		// 1) get data from Sqlite
		List<Jump> sjSjl_l = SqliteJump.SelectJumpsWeightFVProfile (personID, sessionID, false);

		// 2) check if we have two different weight SJL
		List<double> differentWeights_l = new List<double> ();
		foreach (Jump j in sjSjl_l)
			if (j.Type == "SJl")
				differentWeights_l = Util.AddToListDoubleIfNotExist (differentWeights_l, j.WeightPercent);

		if (differentWeights_l.Count < 2)
			return SJLEnum.SJL100CALCNEEDTWOSJLDIFFW; //With this we can show the red ball

		// 3) check there is SJ
		if (sj == 0)
			return SJLEnum.SJL100CALCNEEDSJ;

		// 4) prepare the points
		List<PointF> point_l = new List<PointF>();
		foreach(Jump j in sjSjl_l)
			point_l.Add (new PointF (j.WeightPercent, Jump.GetInitialSpeed (j.Tv, true) ));

		LeastSquaresParabole lsp = new LeastSquaresParabole ();
		lsp.Calculate (point_l);

		// 5) check parabole is ok
		LogB.Information ("Parabole: " + lsp.ParaboleType);
		if (! lsp.CalculatedCoef || lsp.ParaboleType != LeastSquaresParabole.ParaboleTypes.CONVEX)
			return SJLEnum.SJL100CALCPROBLEMS;

		LogB.Information ("coefs: " + Util.DoubleArrayToString (lsp.Coef, 4, "; "));

		LogB.Information ("Parabole: " + lsp.ParaboleType);
		// 6) calculate initial speed
		initialSpeedAt100PercentWeight = lsp.CalculateYAtSomeX (100);
		LogB.Information (string.Format ("calculateYAtSomeX = {0}", initialSpeedAt100PercentWeight));

		if (initialSpeedAt100PercentWeight <= 0)
		{
			initialSpeedAt100PercentWeight = 0;
			return SJLEnum.SJL100CALCNEGATIVE;
		}

		return SJLEnum.SJL100CALCOK;
	}

	public List<JumpsProfileIndex> GetIndexes()
	{
		List<JumpsProfileIndex> l = new List<JumpsProfileIndex>();
		l.Add(jpi0);
		l.Add(jpi1);
		l.Add(jpi2);
		l.Add(jpi3);
		l.Add(jpi4);
		return l;
	}

	private void fillListJumpsDone(double sj, double sjl, double cmj, double abk, double dja)
	{
		AllJumpsDone = true;
		JumpsDone = new List<YesNo>();
		JumpsDone.Add(fillJump(sj));

		LogB.Information ("at fillListJumpsDone sjl: " + sjl.ToString ());
		if (sjl > 0)
			JumpsDone.Add(fillJump(sjl));
		else if (sjlEnum == SJLEnum.SJL100CALCNEEDSJ) //mark sjl 100 exists but not all jumps done (it would be also false because previously there is no sj)
		{
			JumpsDone.Add (YesNo.YES);
			AllJumpsDone = false;
		} else {
			//any calc problem like sjl negative or parabole problems
			JumpsDone.Add (YesNo.NO);
			AllJumpsDone = false;

			if (sjlEnum == SJLEnum.SJL100CALCNEGATIVE)
				ErrorSJl = Catalog.GetString ("Cannot predict SJl at 100% body weight. Jump height would be negative.");
			else
				ErrorSJl = Catalog.GetString ("Problems calculating SJl at 100% body weight.");
		}
		JumpsDone.Add(fillJump(cmj));
		JumpsDone.Add(fillJump(abk));
		JumpsDone.Add(fillJump(dja));
	}

	private YesNo fillJump(double j)
	{
		if(j > 0)
			return YesNo.YES;

		AllJumpsDone = false;
		return YesNo.NO;
	}
}
