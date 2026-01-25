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
using System.Data;
using Gtk;
using System.Collections.Generic; //List<T>

public class UpdateProgressBar {
	public bool IsEvent;
	public bool PercentageMode;
	public double ValueToShow;

	public UpdateProgressBar() {
	}

	public UpdateProgressBar(bool isEvent, bool percentageMode, double valueToShow) {
		this.IsEvent = isEvent;
		this.PercentageMode = percentageMode;
		this.ValueToShow = valueToShow;
	}

	~UpdateProgressBar() {}
}

//start window buttons
public class MovingStartButton
{
	public bool Moving;

	private double pos;
	private double speed;
	private int end;
	public enum Dirs { R, L }
	private Dirs dir;


	public MovingStartButton(int start, int end, Dirs dir)
	{
		pos = start;
		this.end = end;
		this.dir = dir;
		Moving = true;
	}
	
	public bool Next()
	{
		if(dir == Dirs.R) {
			if( pos >= end )
				Moving = false;
			else {
				speed = Math.Ceiling(Math.Abs(end-pos)/25.0);
				pos += speed;
			}
		} else {
			if( pos <= end )
				Moving = false;
			else {
				speed = Math.Ceiling(Math.Abs(end-pos)/25.0);
				pos -= speed;
			}
		}

		//LogB.Information("pos: " + pos + "; speed: " + speed);
		return true;
	}

	public int Pos {
		get { return Convert.ToInt32(pos); }
	}
	public int Speed {
		get { return Convert.ToInt32(speed); }
	}
}

//to store the rectangle size of every encoder or forceSensor capture repetition
//in order to be saved or not on clicking screen
//note every rep will be c or ec
public class RepetitionMouseLimits
{
//	TODO: make all the sample stuff inherited

	protected List<PointInRectangle> list;
	protected int current;

	public RepetitionMouseLimits()
	{
		list = new List<PointInRectangle>();
		current = 0;
	}

	public void Add (double startX, double startY, double endX, double endY)
	{
		PointInRectangle p = new PointInRectangle (current ++, startX, startY, endX, endY);
		list.Add(p);
		//LogB.Information("Mouse added: " + p.ToString());
	}

	//used on CairoBars because bars go from right to left, so we force the pos here
	public void AddInPos (int pos, double startX, double startY, double endX, double endY)
	{
		PointInRectangle p = new PointInRectangle (pos, startX, startY, endX, endY);
		list.Add(p);
		//LogB.Information("Mouse added: " + p.ToString());
	}

	public int FindBarInPixel (double px, double py)
	{
		foreach (PointInRectangle pir in list)
			if (px >= pir.StartX && px <= pir.EndX)
			{
				if (pir.StartY < 0 && pir.EndY < 0) //forceSensor does not have Y, so both are -1, only check X
					return pir.Id;
				else if (py >= pir.StartY && py <= pir.EndY) //encoder has Y, need to check it. Note also on BarPoints.POINTS the y is relevant
					return pir.Id;
			}

		return -1;
	}

	/*
	public double GetStartOfARep(int rep)
	{
		return ((PointInRectangle) list[rep]).Start;
	}
	public double GetEndOfARep(int rep)
	{
		return ((PointInRectangle) list[rep]).End;
	}
	*/

	//to debug
	public int Count ()
	{
		return list.Count;
	}
}
//used on graphs/cairo/forceSensor.cs CairoGraphForceSensorAI
public class RepetitionMouseLimitsWithSamples : RepetitionMouseLimits
{
	private List<int> sampleStart_l;
	private List<int> sampleEnd_l;

	public RepetitionMouseLimitsWithSamples ()
	{
		list = new List<PointInRectangle>();
		current = 0;

		sampleStart_l = new List<int>();
		sampleEnd_l = new List<int>();
	}

	public void AddSamples (int sampleStart, int sampleEnd)
	{
		sampleStart_l.Add (sampleStart);
		sampleEnd_l.Add (sampleEnd);
	}

	public int GetSampleStartOfARep (int rep)
	{
		return (sampleStart_l[rep]);
	}
	public int GetSampleEndOfARep (int rep)
	{
		return (sampleEnd_l[rep]);
	}
}

public class Blink
{
	private DateTime timeStart;

	public enum StatusEnum { NOTSTARTED, RUNNING, ENDED };
	public StatusEnum Status;

	//constructor
	public Blink ()
	{
		Status = StatusEnum.NOTSTARTED;
	}

	public void Start ()
	{
		timeStart = DateTime.Now;
		Status = StatusEnum.RUNNING;
	}

	public void End ()
	{
		Status = StatusEnum.ENDED;
	}

	//to show somthing like the red icon of capturing (blinking)
	public bool IsOn
	{
		get {
			TimeSpan ts = DateTime.Now.Subtract (timeStart);
			return (Util.IsEven (Convert.ToInt32 (ts.TotalSeconds)));
		}
	}
}

public class BlinkImage : Blink
{
	public Gtk.Image imageOff;
	public Gtk.Image imageOn;

	//constructor
	//TODO: assign color (tare, calibrate, detect stiffness: blue, capture: red)
	public BlinkImage (Gtk.Image imageOff, Gtk.Image imageOn)
	{
		Status = StatusEnum.NOTSTARTED;

		this.imageOff = imageOff;
		this.imageOn = imageOn;
	}

	public Gtk.Image ImageOff {
		get { return imageOff; }
	}
	public Gtk.Image ImageOn {
		get { return imageOn; }
	}
}
