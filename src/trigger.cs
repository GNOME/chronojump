/*
 * Copyright (C) 2017  Xavier de Blas <xaviblas@gmail.com>
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA 02111-1307, USA.
 */

using System;
using System.Collections.Generic; //List<T>

public class Trigger
{
	public enum Modes { ENCODER, FORCESENSOR, RACEANALYZER }

	private int uniqueID;
	private Modes mode;
	private int modeID;
	private int ms;
	private bool inOut;
	private string name;
	private string color;
	private string comments;

	//constructor used on capture
	public Trigger (Modes mode, int ms, bool inOut)
	{
		this.uniqueID = -1; 	//will be assigned on SQL insertion
		this.mode = mode;
		this.modeID = -1; 	//will be assigned on SQL insertion
		this.ms = ms;
		this.inOut = inOut;
		this.name = "";
		this.color = "";
		this.comments = "";
	}

	//constructor used on loading from SQL
	public Trigger (int uniqueID, Modes mode, int modeID, int ms, bool inOut, string name, string color, string comments)
	{
		this.uniqueID = uniqueID;
		this.mode = mode;
		this.modeID = modeID;
		this.ms = ms;
		this.inOut = inOut;
		this.name = name;
		this.color = color;
		this.comments = comments;
	}

	public void Substract(int msToSubstract)
	{
		ms -= msToSubstract;
	}

	public string ToSQLInsertString()
	{
		 string idStr = uniqueID.ToString();
		 if(idStr == "-1")
			 idStr = "NULL";

	 	return 
			idStr + "," +
			"\"" + mode.ToString() + "\"" + "," +
			modeID.ToString() + "," +
			ms.ToString() + "," +
			Util.BoolToInt(inOut).ToString() + "," +
			"\"" + name.ToString() + "\"" + "," +
			"\"" + color.ToString() + "\"" + "," +
			"\"" + comments.ToString() + "\""
			;
	}

	//used on TextView
	public override string ToString()
	{
		return ms.ToString() + ", " + Util.BoolToInOut(inOut).ToString();
	}

	public int UniqueID {
		get { return uniqueID; }
	}

	public int Ms {
		get { return ms; }
	}

	public bool IsNegative {
		get { return ms < 0; }
	}

	public bool InOut {
		get { return inOut; }
	}

	public int ModeID {
		set { modeID = value; }
	}
}

public class TriggerList
{
	public const string TriggersNotFoundString = "-1";
	private List<Trigger> l;

	//constructors
	public TriggerList()
	{
		l = new List<Trigger>();
	}
	//from SqliteTrigger.Select()
	public TriggerList(List<Trigger> l)
	{
		this.l = l;
	}


	public void Add(Trigger trigger)
	{
		l.Add(trigger);
	}

	public void Substract(int msToSubstract)
	{
		//iterate negative to not fail enumeration if an element is substracted
		for(int i = l.Count -1 ; i >= 0; i --)
		{
			l[i].Substract(msToSubstract);

			//triggers cannot be negative
			if(l[i].IsNegative)
				l.RemoveAt(i);
		}
	}

	//just to debug
	public void Print()
	{
		LogB.Information("Printing trigger list");
		foreach(Trigger trigger in l)
			LogB.Information(trigger.ToString());
	}

	//used on TextView
	public override string ToString()
	{
		string s = "";
		string sep = "";
		foreach(Trigger trigger in l)
		{
			s += sep + trigger.ToString();
			sep = "\n";
		}
		return s;
	}

	public string ToRCurvesString()
	{
		if(l.Count == 0)
			return TriggersNotFoundString;

		string s = "";
		string sep = "";
		foreach(Trigger trigger in l)
		{
			if(trigger.InOut) {
				s += sep + trigger.Ms.ToString();
				sep = ";";
			}
		}
		return s;
	}

	public int Count()
	{
		return l.Count;
	}

	public void SQLInsert(int signalID)
	{
		//save triggers to file (if any)
		if(l == null || l.Count == 0)
			return;

		//update triggers with encoderSignalUniqueID
		foreach(Trigger trigger in l)
			trigger.ModeID = signalID;

		LogB.Debug("runEncoderCaptureCsharp SQL inserting triggers");
		SqliteTrigger.InsertList(false, l);
	}


	/*
	 * start of spurious management
	 */
	private int countOn()
	{
		int countOn = 0;
		foreach(Trigger t in l)
			if(t.InOut)
				countOn ++;

		return countOn;
	}

	private int countOff()
	{
		int countOff = 0;
		foreach(Trigger t in l)
			if(! t.InOut)
				countOff ++;

		return countOff;
	}

	//see encoderCapture.MinimumOneTriggersOn()
	public bool MinimumOneOn()
	{
		if(countOn() >= 1)
			return true;

		return false;
	}

	public enum SpuriousType { ON, OFF, BOTH }
	//if ON will return last "on"
	private Trigger last(SpuriousType spt)
	{
		int i = 0;
		int lastPos = 0;
		foreach(Trigger t in l)
		{
			if(
					spt == SpuriousType.BOTH ||
					spt == SpuriousType.ON && t.InOut ||
					spt == SpuriousType.OFF && ! t.InOut)
				lastPos = i;
			i ++;
		}

		return l[lastPos];
	}

	//on runEncoder .ino the management of interruptions/triggers could be better
	//just check if two triggers ON or two OFF arrive to discard the second (newTrigger)
	public bool NewSameTypeThanBefore(Trigger newTrigger)
	{
		//cannot be an old trigger if this is the first Of this type
		if(newTrigger.InOut && countOn() == 0)
			return false;
		else if(! newTrigger.InOut && countOff() == 0)
			return false;

		if(last(SpuriousType.BOTH).InOut == newTrigger.InOut)
			return true;

		return false;
	}

	/*
	 * this newTrigger is an On trigger, compare with last
	 * encoder: spt.ON, 50ms
	 * runEncoder: spt.BOTH, 50ms
	 */
	public bool IsSpurious(Trigger newTrigger, SpuriousType spt, int ms)
	{
		//cannot be spurious if is the first of this type
		if(spt == SpuriousType.ON && countOn() == 0)
			return false;
		else if(spt == SpuriousType.BOTH && (
				newTrigger.InOut && countOn() == 0 ||
				! newTrigger.InOut && countOff() == 0) )
			return false;

		if(spt == SpuriousType.BOTH && (newTrigger.Ms - last(SpuriousType.BOTH).Ms) < ms )
			return true;
		else if(spt == SpuriousType.ON && (newTrigger.Ms - last(SpuriousType.ON).Ms) < ms )
			return true;

		return false;
	}

	/*
	 * used on encoder (where we also use triggers to cut repetitions)
	 * if on forceSensor we use triggers to cut repetitions, will use this too
	 * not used on RunEncoder
	 */
	public void RemoveLastOff()
	{
		l.Remove(last(SpuriousType.OFF));
	}

	/*
	 * end of spurious management
	 */


	/*
	public void Write()
	{
		//save triggers to file (if any)
		if(l == null || l.Count == 0)
			return;

		LogB.Debug("runEncoderCaptureCsharp saving triggers");
		TextWriter writer = File.CreateText(Util.GetEncoderTriggerDateTimeFileName());

		foreach(Trigger trigger in l)
			writer.WriteLine(trigger.ToString());

		writer.Flush();
		writer.Close();
		((IDisposable)writer).Dispose();
	}
	*/
}

