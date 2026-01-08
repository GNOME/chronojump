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
 * Copyright (C) 2022  Xavier de Blas <xaviblas@gmail.com>
 */

using System;
using System.Data;
using System.Collections.Generic; //List


//decide if use this or inspector
//TODO: clarify what this class does
public class RunPhaseTimeList
{
	public bool SpeedStart;

	private List<PhaseTime> listPhaseTime;
	private Constants.DoubleContact checkDoubleContactMode;
	private int checkTime;

	//if there are double contacts at start, first run phase infos will not be used
	public int FirstRPIs;

	public RunPhaseTimeList(Constants.DoubleContact checkDoubleContactMode, int checkTime)
	{
		this.checkDoubleContactMode = checkDoubleContactMode;
		this.checkTime = checkTime;

		listPhaseTime = new List<PhaseTime>();
		FirstRPIs = 0;
	}
	
	public void AddTC(int photocell, double timestamp)
	{
		listPhaseTime.Add(new PhaseTime(photocell, true, timestamp));
	}

	public void AddTF(int photocell, double timestamp)
	{
		listPhaseTime.Add(new PhaseTime(photocell, false, timestamp));
	}

	public override string ToString()
	{
		string str = "";

		//This is problematic (Collection was modified; enumeration operation may not execute) if other thread is changing it:
		//foreach(PhaseTime pt in listPhaseTime)
		//solution:
		List<PhaseTime> listPhaseTimeShallowCloned = new List<PhaseTime>(listPhaseTime);
		foreach(PhaseTime pt in listPhaseTimeShallowCloned)
			str += pt.ToString();

		return str;
	}

	//to show tc chunks or not on gui/eventExecute.cs
	public bool UseDoubleContacts()
	{
		return (checkDoubleContactMode != Constants.DoubleContact.NONE);
	}

	public List<RunPhaseTimeListObject> InListForPainting()
	{
		List<RunPhaseTimeListObject> list_in = new List<RunPhaseTimeListObject>();
		int currentMS = 0;
		int startInMS = -1;
		int startInPhotocell = -1;
		int currentPhotocell = -1;

		// 1) create a copy of listPhaseTime in order to do foreach without problems with other thread that adds records
		//This is problematic (Collection was modified; enumeration operation may not execute) if other thread is changing it:
		//foreach(PhaseTime pt in listPhaseTime)
		//solution:
		List<PhaseTime> listPhaseTimeShallowCloned = new List<PhaseTime>(listPhaseTime);

		/*
		 * 2) check if we started in because 1st TC has to be counted in the track
		 * but 2nd TC has to be the end of the first track
		 * we need this to synchronize correctly
		 */
//		bool startedIn = false;
		if(listPhaseTimeShallowCloned.Count >= 1)
		{
			PhaseTime ptFirst = (PhaseTime) listPhaseTimeShallowCloned[0];
//		if(ptFirst.IsContact)
//				startedIn = true;
		}

		// 3) add elements to the list
		LogB.Information("InListForPainting foreach:");
		int count = 0;
		double negativeValues = 0; //double contacts times at start
		PhaseTime ptLast = null;

		RunPhaseTimeListObject.Phases currentPhase = RunPhaseTimeListObject.Phases.START;
		RunPhaseTimeListObject rptloToAdd = null;

		foreach(PhaseTime pt in listPhaseTimeShallowCloned)
		{
			LogB.Information(pt.ToString());

			if(FirstRPIs > count)
			{
				negativeValues += pt.Duration/1000.0;
				LogB.Information("InListForPainting negativeValues = " + negativeValues.ToString());
			}

			if(pt.IsContact) {
				startInMS = currentMS;
				startInPhotocell = pt.Photocell;
			}
			else if(startInMS >= 0)
			{
				//see if previous has ended to mark as END or STARTEND
				if(rptloToAdd != null)
				{
					bool thisPhaseEnds = false;
					if(list_in.Count == 0 && ! SpeedStart)
					{
						//on ! speedStart first tc+tf pair, count only tf
						if(startInMS/1000.0 - rptloToAdd.tcEnd > checkTime/1000.0)
							thisPhaseEnds = true;
					}
					else if(startInMS/1000.0 - rptloToAdd.tcStart > checkTime/1000.0)
						thisPhaseEnds = true;

					if(thisPhaseEnds)
					{
						if(rptloToAdd.phase == RunPhaseTimeListObject.Phases.START)
							rptloToAdd.phase = RunPhaseTimeListObject.Phases.STARTANDEND;
						else
							rptloToAdd.phase = RunPhaseTimeListObject.Phases.END;

						currentPhase = RunPhaseTimeListObject.Phases.START;
					} else
						currentPhase = RunPhaseTimeListObject.Phases.MIDDLE;

					list_in.Add(rptloToAdd);
				}

				//this will be added in next iteration of flight (! pt.IsContact)
				rptloToAdd = new RunPhaseTimeListObject(
						currentPhase,
						startInMS/1000.0,
						currentMS/1000.0,
						startInPhotocell,
						pt.Photocell);
			}

			currentMS += Convert.ToInt32(pt.Duration);
			currentPhotocell = pt.Photocell;

			LogB.Information(string.Format("End of iteration: {0}, pt.IsContact: {1}, startInMS: {2}, currentMS: {3}",
						count, pt.IsContact, startInMS, currentMS));

			ptLast = pt;
			count ++;
		}

		//add pending rptl
		if(startInMS/1000.0 - rptloToAdd.tcStart > checkTime/1000.0)
		{
			if(rptloToAdd.phase == RunPhaseTimeListObject.Phases.START)
				rptloToAdd.phase = RunPhaseTimeListObject.Phases.STARTANDEND;
			else
				rptloToAdd.phase = RunPhaseTimeListObject.Phases.END;
		}

		list_in.Add(rptloToAdd);

		//when track ends, last phase is a TC, add it
		if(ptLast != null && ptLast.IsContact)
		{
			RunPhaseTimeListObject rptloLast = new RunPhaseTimeListObject(
						RunPhaseTimeListObject.Phases.STARTANDEND,
						startInMS/1000.0,
						(startInMS + ptLast.Duration)/1000.0,
						startInPhotocell, ptLast.Photocell);

			if(rptloToAdd.phase == RunPhaseTimeListObject.Phases.START ||
					rptloToAdd.phase == RunPhaseTimeListObject.Phases.MIDDLE)
				rptloLast.phase = RunPhaseTimeListObject.Phases.END;

			list_in.Add(rptloLast);
		}

		//manage the negative values
		if(negativeValues > 0)
		{
			LogB.Information("Fixing negative values (double contacts times at start)");
			for (int i = 0; i < list_in.Count; i ++)
			{
				LogB.Information(string.Format("PRE i: {0}, list_in[{0}]: {1}", i, list_in[i]));

				RunPhaseTimeListObject rptlo = (RunPhaseTimeListObject) list_in[i];
				rptlo.tcStart -= negativeValues;
				rptlo.tcEnd -= negativeValues;
				list_in[i] = rptlo;
			}
		}

		return list_in;
	}

	//Debug
	public string InListForPaintingToString()
	{
		string str = "Contact in time list:\n";
		List<RunPhaseTimeListObject> list_in = InListForPainting();
		foreach(RunPhaseTimeListObject rptlo in list_in)
			str += rptlo.ToString() + "\n";

		return str;
	}

	public List<PhaseTime> ListPhaseTime
	{
		get { return listPhaseTime; }
	}

}

//TODO: clarify what this class does
public class RunPhaseTimeListObject
{
	//each contact can be start of a chunk, middle, end or startandend
	//this is important for the drawing in gui/eventExecute.cs
	public enum Phases { START, MIDDLE, END, STARTANDEND }
	public Phases phase;
	public double tcStart;
	public double tcEnd;
	public int photocellStart;
	public int photocellEnd;


	public RunPhaseTimeListObject ()
	{
	}

	public RunPhaseTimeListObject (Phases phase,
			double tcStart, double tcEnd, int photocellStart, int photocellEnd)
	{
		this.phase = phase;
		this.tcStart = tcStart;
		this.tcEnd = tcEnd;
		this.photocellStart = photocellStart;
		this.photocellEnd = photocellEnd;
	}

	public override string ToString()
	{
		return phase.ToString() + ":" +
			Math.Round(tcStart, 3).ToString() + ":" +
			Math.Round(tcEnd, 3).ToString() + ":" +
			photocellStart.ToString() + ":" +
			photocellEnd.ToString();
	}
}
