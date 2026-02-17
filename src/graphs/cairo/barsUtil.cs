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
 *  Copyright (C) 2004-2025   Xavier de Blas <xaviblas@gmail.com>
 */

using System;
using System.Collections.Generic; //List
using Gdk;
using Gtk;
using Cairo;


public class BarResult
{
	public Point3F p;
	public bool selected;
	public bool above;
	public Cairo.Color color;

	public BarResult (Point3F p, bool selected, bool above, Cairo.Color color)
	{
		this.p = p;
		this.selected = selected;
		this.above = above;
		this.color = color;
	}
}

public class CairoBarsGuide
{
	public enum GuideEnum { SESSION_MAX, SESSION_AVG, SESSION_MIN, PERSON_MAX_ALL_S, PERSON_MAX_THIS_S, PERSON_AVG_THIS_S, PERSON_MIN_THIS_S }

	private GuideEnum gEnum;
	private double y;
	private int width;
	private Cairo.Color color;
	private char c; //this will be an icon of person or group;
	private double extraRightDist;
	//color, linetype, icon, ...

	public CairoBarsGuide (GuideEnum gEnum, double y, int width, Cairo.Color color, char c, double extraRightDist)
	{
		this.gEnum = gEnum;
		this.y = y;
		this.width = width;
		this.color = color;
		this.c = c;
		this.extraRightDist = extraRightDist;
	}

	public GuideEnum Genum {
		get { return gEnum; }
	}
	public double Y {
		get { return y; }
	}
	public int Width {
		get { return width; }
	}
	public Cairo.Color Color {
		get { return color; }
	}
	public char C {
		get { return c; }
	}
	public double ExtraRightDist {
		get { return extraRightDist; }
	}
}

//manage distances of guides do draw als the person, session indicators
//right now used on jump/run simple
public class CairoBarsGuideManage
{
	private List<CairoBarsGuide> l;

	public CairoBarsGuideManage (bool usePersonGuides, bool useGroupGuides,
			double sessionMAXAtSQL, double sessionAVGAtSQL, double sessionMINAtSQL,
			double personMAXAtSQLAllSessions, double personMAXAtSQL, double personAVGAtSQL, double personMINAtSQL)
	{
		l = new List<CairoBarsGuide> ();
		//int pos = 1;
		//int dist = 8;

		if(useGroupGuides)
		{
			l.Add(new CairoBarsGuide(CairoBarsGuide.GuideEnum.SESSION_MAX, sessionMAXAtSQL,
						2, colorFromRGB(0,0,0), 'G', 12)); //(pos++)*dist));
			l.Add(new CairoBarsGuide(CairoBarsGuide.GuideEnum.SESSION_AVG, sessionAVGAtSQL,
						1, colorFromRGB(0,0,0), 'g', 12)); //(pos++)*dist));
			l.Add(new CairoBarsGuide(CairoBarsGuide.GuideEnum.SESSION_MIN, sessionMINAtSQL,
						1, colorFromRGB(0,0,0), 'g', 12)); //(pos++)*dist));
		}

		if(usePersonGuides)
		{
			//unused
			//l.Add(new CairoBarsGuide(CairoBarsGuide.GuideEnum.PERSON_MAX_ALL_S, personMAXAtSQLAllSessions,
			//			4, colorFromRGB(255,0,255), 'P', 12)); //(pos++)*dist));

			l.Add(new CairoBarsGuide(CairoBarsGuide.GuideEnum.PERSON_MAX_THIS_S, personMAXAtSQL,
						2, colorFromRGB(255,238,102), 'P', 12)); //(pos++)*dist));
			l.Add(new CairoBarsGuide(CairoBarsGuide.GuideEnum.PERSON_AVG_THIS_S, personAVGAtSQL,
						1, colorFromRGB(255,238,102), 'p', 12)); //(pos++)*dist));
			l.Add(new CairoBarsGuide(CairoBarsGuide.GuideEnum.PERSON_MIN_THIS_S, personMINAtSQL,
						2, colorFromRGB(255,238,102), 'P', 12)); //(pos++)*dist));
		}
	}

	protected Cairo.Color colorFromRGB(int red, int green, int blue)
	{
		return new Cairo.Color(red/256.0, green/256.0, blue/256.0);
	}

	public double GetMax ()
	{
		double max = 0;
		foreach(CairoBarsGuide cbg in l)
			if(cbg.Y > max)
				max = cbg.Y;

		return max;
	}

	public double GetTipGroupMax ()
	{
		foreach(CairoBarsGuide cbg in l)
			if(cbg.Genum == CairoBarsGuide.GuideEnum.SESSION_MAX)
				return cbg.Y;

		return 0;
	}
	public double GetTipGroupAvg ()
	{
		foreach(CairoBarsGuide cbg in l)
			if(cbg.Genum == CairoBarsGuide.GuideEnum.SESSION_AVG)
				return cbg.Y;

		return 0;
	}
	public double GetTipGroupMin ()
	{
		foreach(CairoBarsGuide cbg in l)
			if(cbg.Genum == CairoBarsGuide.GuideEnum.SESSION_MIN)
				return cbg.Y;

		return 0;
	}

	public double GetTipPersonMax ()
	{
		foreach(CairoBarsGuide cbg in l)
			if(cbg.Genum == CairoBarsGuide.GuideEnum.PERSON_MAX_THIS_S)
				return cbg.Y;

		return 0;
	}
	public double GetTipPersonAvg ()
	{
		foreach(CairoBarsGuide cbg in l)
			if(cbg.Genum == CairoBarsGuide.GuideEnum.PERSON_AVG_THIS_S)
				return cbg.Y;

		return 0;
	}
	public double GetTipPersonMin ()
	{
		foreach(CairoBarsGuide cbg in l)
			if(cbg.Genum == CairoBarsGuide.GuideEnum.PERSON_MIN_THIS_S)
				return cbg.Y;

		return 0;
	}
	public List<CairoBarsGuide> L {
		get { return l; }
	}
}

// ----
//note x0pos, x1pos are the pos (meaning the bar)
//used also for the eccentric overload
public class CairoBarsArrow
{
	public int x0pos;
	public double y0;
	public int x1pos;
	public double y1;

	public CairoBarsArrow (int x0pos, double y0, int x1pos, double y1)
	{
		this.x0pos = x0pos;
		this.y0 = y0;
		this.x1pos = x1pos;
		this.y1 = y1;
		//LogB.Information(string.Format("x0pos: {0}, x1pos: {1}", x0pos, x1pos));
	}

	public double GetX0Graph (List<double> barsXCenter_l)
	{
		return barsXCenter_l[x0pos];
	}

	public double GetX1Graph (List<double> barsXCenter_l)
	{
		return barsXCenter_l[x1pos];
	}

	public override string ToString()
	{
		return string.Format("x0pos: {0}, y0: {1}, x1pos: {2}, y1: {3}",
				x0pos, y0, x1pos, y1);
	}
}

//related to secondary variable (on encoder default is range)
public class CairoBarsSecondaryLineData
{
	public string units;
	public List<double> data_l;
	public bool left; //false: at right
	public double yMax;
	public double yMin;
	public string magnitude;

	public CairoBarsSecondaryLineData ()
	{
		data_l = new List<double> ();
	}

	public CairoBarsSecondaryLineData (
			List<double> data_l, bool left, double yMax, double yMin, string magnitude)
	{
		this.data_l = data_l;
		this.left = left;

		//manage problems if both values are the same of min > max
		if (yMax == yMin) {
			this.yMax = -1;
			this.yMin = -1;
		} else if (yMax > yMin) {
			this.yMax = yMax;
			this.yMin = yMin;
		} else {
			this.yMin = yMax;
			this.yMax = yMin;
		}

		this.magnitude = magnitude;

		if (magnitude == Constants.MeanSpeed || magnitude == Constants.MaxSpeed)
			this.units = "m/s";
		else if (magnitude == Constants.MeanForce || magnitude == Constants.MaxForce)
			this.units = "N";
		else if (magnitude == Constants.MeanPower || magnitude == Constants.PeakPower)
			this.units = "W";
		else { //(magnitude == Constants.RangeAbsolute)
			this.units = "cm";
			this.magnitude = "ROM"; //to display better on graph
			for (int i = 0; i < data_l.Count; i ++)
				data_l[i] = data_l[i] / 10.0;
		}
	}
}
