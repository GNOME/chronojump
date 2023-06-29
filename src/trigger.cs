/*
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
 *
 * Copyright (C) 2017-2020  Xavier de Blas <xaviblas@gmail.com>
 *
 */

using System;
using System.Collections.Generic; //List<T>

public class Trigger
{
	public enum Modes { ENCODER, FORCESENSOR, RACEANALYZER }
	//note encoder triggers are in milliseconds (ms) and forcesensor and race analyzer in microseconds (us)

	private int uniqueID;
	private Modes mode;
	private int modeID;
	private int us; //on encoder is milliseconds, on forceSensor and raceAnalyzer is micro seconds
	private bool inOut;
	private string name;
	private string color;
	private string comments;

	//constructor used on capture
	public Trigger (Modes mode, int us, bool inOut)
	{
		this.uniqueID = -1; 	//will be assigned on SQL insertion
		this.mode = mode;
		this.modeID = -1; 	//will be assigned on SQL insertion
		this.us = us;
		this.inOut = inOut;
		this.name = "";
		this.color = "";
		this.comments = "";
	}

	//constructor used on loading from SQL
	public Trigger (int uniqueID, Modes mode, int modeID, int us, bool inOut, string name, string color, string comments)
	{
		this.uniqueID = uniqueID;
		this.mode = mode;
		this.modeID = modeID;
		this.us = us;
		this.inOut = inOut;
		this.name = name;
		this.color = color;
		this.comments = comments;
	}
	//us: on encoder is milliseconds, on forceSensor and raceAnalyzer is micro seconds
	public void Substract(int usToSubstract)
	{
		us -= usToSubstract;
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
			us.ToString() + "," +
			Util.BoolToInt(inOut).ToString() + "," +
			"\"" + name.ToString() + "\"" + "," +
			"\"" + color.ToString() + "\"" + "," +
			"\"" + comments.ToString() + "\""
			;
	}

	//used on TextView
	public override string ToString()
	{
		return us.ToString() + ", " + Util.BoolToInOut(inOut).ToString();
	}

	public int UniqueID {
		get { return uniqueID; }
	}

	public int Us {
		get { return us; }
		set { us = value; }
	}
	public double Ms {
		get {
			if(mode == Modes.ENCODER)
				return(us);
			else
				return UtilAll.DivideSafe(us, 1000.0);
		}
	}

	public bool IsNegative {
		get { return us < 0; }
	}

	public bool InOut {
		get { return inOut; }
	}

	public Modes Mode {
		get { return mode; }
	}

	public int ModeID {
		set { modeID = value; }
	}
}

public class TriggerList
{
	public const string TriggersNotFoundString = "-1";
	public enum Type3 { ON, OFF, BOTH }
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

	public void Substract(int usToSubstract)
	{
		//iterate negative to not fail enumeration if an element is substracted
		for(int i = l.Count -1 ; i >= 0; i --)
		{
			l[i].Substract(usToSubstract);

			//triggers cannot be negative
			if(l[i].IsNegative)
				l.RemoveAt(i);
		}
	}

	//used on forceSensorAnalyzeManualGraphDo
	public List<Trigger> GetList()
	{
		return l;
	}
	public List<Trigger> GetListReversed()
	{
		List<Trigger> rev_l = new List<Trigger> ();
		for (int i = l.Count -1; i >= 0; i --)
			rev_l.Add (l[i]);

		return rev_l;
	}

	public Trigger GetTrigger (int i)
	{
		return l[i];
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

	//to print for R,
	//best is call here with an ON to print a triggersOnList
	//then call here with an OFF
	//note on encoder we only use ON, but on runEncoder, we use ON and then OFF
	//usually sepChar will be ';' and all triggers will be on a row
	//but can be ',' if we pass some ints to a csv
	public string ToRCurvesString(Type3 type3, char sepChar)
	{
		/*
		   cannot do it here because maybe we have triggers on but not off,
		   so if we come by off, l.Count is not a good indicator to pass a -1,
		   so do it at end of this method with if(s == "")
		if(l.Count == 0)
			return TriggersNotFoundString;
		*/

		string s = "";
		string sep = "";
		foreach(Trigger trigger in l)
		{
			if(trigger.InOut && type3 != Type3.OFF)
			{
				s += sep + Util.ConvertToPoint(trigger.Ms);
				sep = sepChar.ToString();
			}
			if(! trigger.InOut && type3 != Type3.ON)
			{
				s += sep + Util.ConvertToPoint(trigger.Ms);
				sep = sepChar.ToString();
			}
		}

		if(s == "")
			return TriggersNotFoundString;
		else
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

	//if ON will return last "on"
	private Trigger last(Type3 type3)
	{
		int i = 0;
		int lastPos = 0;
		foreach(Trigger t in l)
		{
			if(
					type3 == Type3.BOTH ||
					type3 == Type3.ON && t.InOut ||
					type3 == Type3.OFF && ! t.InOut)
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

		if(last(Type3.BOTH).InOut == newTrigger.InOut)
			return true;

		return false;
	}

	/*
	 * this newTrigger is an On trigger, compare with last
	 * encoder: type3.ON, 50ms
	 * runEncoder: type3.BOTH, 50ms

	 * note on encoder it works on ms and the rest in us
	 * on encoder threashold its 50 (ms)
	 * rest of instruments its 50000 (us)
	 */
	public bool IsSpurious(Trigger newTrigger, Type3 type3, int threashold)
	{
		//cannot be spurious if is the first of this type
		if(type3 == Type3.ON && countOn() == 0)
			return false;
		else if(type3 == Type3.BOTH && (
				newTrigger.InOut && countOn() == 0 ||
				! newTrigger.InOut && countOff() == 0) )
			return false;

		if(type3 == Type3.BOTH && (newTrigger.Us - last(Type3.BOTH).Us) < threashold )
			return true;
		else if(type3 == Type3.ON && (newTrigger.Us - last(Type3.ON).Us) < threashold )
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
		l.Remove(last(Type3.OFF));
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

