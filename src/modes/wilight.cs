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
 *  Copyright (C) 2024-2025   Xavier de Blas <xaviblas@gmail.com>
 */

using System;
using System.Collections.Generic; //List<T>
using System.Diagnostics;  //Stopwatch

//wilight test (like jump, run, ...)
public class Wilight : Event
{
	private int exerciseID; //until wilightExercise table is not created, all will be 0
	private string videoURL;
	private int totalMs;
	private string onString; //8;2035;ON=6;2120;ON=...

	/*
	//constructor used after deleting a test
	public Wilight ()
	{
		this.uniqueID = -1;
	}
	*/

	//regular constructor
	public Wilight (int uniqueID, int personID, int sessionID, int exerciseID,
			string dateTime, string videoURL, int totalMs, string onString, string description)
	{
		this.uniqueID = uniqueID;
		this.personID = personID;
		this.sessionID = sessionID;
		this.exerciseID = exerciseID;
		this.dateTime = dateTime;
		this.videoURL = videoURL;
		this.totalMs = totalMs;
		this.onString = onString;
		this.description = description;
	}

	//used to select a wilight SqliteWilight.SelectData
	public Wilight (string [] eventString)
	{
		this.uniqueID = Convert.ToInt32(eventString[0]);
		this.personID = Convert.ToInt32(eventString[1]);
		this.sessionID = Convert.ToInt32(eventString[2]);
		this.exerciseID = Convert.ToInt32(eventString[3]);
		this.dateTime = eventString[4];
		this.videoURL = eventString[5];
		this.totalMs = Convert.ToInt32(eventString[6]);
		this.onString = eventString[7];
		this.description = "";
	}

	public static List<Event> WilightListToEventList (List<Wilight> w_l)
	{
		List<Event> events = new List<Event>();
		foreach (Wilight w in w_l)
			events.Add ((Event) w);

		return events;
	}

	public int InsertSQL (bool dbconOpened)
	{
		SqliteTests sqliteTests = new SqliteWilight ();
		return sqliteTests.Insert (dbconOpened, toSQLInsertString());
	}
	private string toSQLInsertString()
	{
		string uniqueIDStr = "NULL";
		if(uniqueID != -1)
			uniqueIDStr = uniqueID.ToString();

		return
			"(" + uniqueIDStr + ", " + personID + ", " + sessionID + ", " + exerciseID +
			", '" + dateTime + "', '" + videoURL + "', " + totalMs + ", '" + onString + "')";
	}

	public int TotalMs {
		get { return totalMs; }
	}
}


public static class WilightColors
{
	public static int OFF = 0;
	public static int RED = 2;
	public static int GREEN = 4;
	public static int BLUE = 8;
}

public class WilightCommandToTerminals
{
	WilightCommand wc;
	WilightTerminalLayout wtl;

	//constructor
	public WilightCommandToTerminals (WilightCommand wc, WilightTerminalLayout wtl)
	{
		this.wc = wc;
		this.wtl = wtl;
	}

	public List<CairoGraphWilightTerminal> Do ()
	{
		List<CairoGraphWilightTerminal> wt_l = new List<CairoGraphWilightTerminal> ();

		//return if is empty
		if (wc.IsEmpty)
			return wt_l;

		foreach (WilightTerminalPair wtp in wc.Wtp_l)
			wt_l.Add (new CairoGraphWilightTerminal (
						wtp.terminalNum,
						wtp.colorCode,
						wtl.GetCenterByCodeNum (wtp.terminalNum)));

		return wt_l;
	}
}

public class WilightPos
{
	private string codeLetter;
	private int codeNum;
	private PointF center;

	public WilightPos (string codeLetter, int codeNum, PointF center)
	{
		this.codeLetter = codeLetter;
		this.codeNum = codeNum;
		this.center = center;
	}

	public override string ToString ()
	{
		return string.Format ("codeLetter: {0}, codeNum: {1}, center: {2}", codeLetter, codeNum, center);
	}

	public string CodeLetter {
		get { return codeLetter; }
	}
	public int CodeNum {
		get { return codeNum; }
	}
	public PointF Center {
		get { return center; }
	}
}
public class WilightTerminalLayout
{
	private List<WilightPos> wp_l;

	//constructor
	public WilightTerminalLayout ()
	{
	}

	/*
	 * reads a file like (note decimal is point):
	 * A;0;7.5;10
	 * B;1;1;8
	 * C;2;2;8
	 */
	public void ReadFile (string layoutFile)
	{
		wp_l = new List<WilightPos> ();

		foreach (string wpStr in Util.ReadFileAsStringList (layoutFile, "#"))
		{
			if (wpStr == "" || wpStr.Length == 0)
				continue;

			//layout goes separated by . converted to comma if needed
			string s = Util.ChangeDecimalSeparator (wpStr);

			string [] sFull = s.Split (new char[] {';'});
			if (sFull.Length != 4)
				continue;

			if (! (
						Util.IsNumber (sFull[1], false) &&
						Util.IsNumber (sFull[2], true) &&
						Util.IsNumber (sFull[3], true)))
				continue;

			wp_l.Add (new WilightPos (sFull[0], Convert.ToInt32 (sFull[1]),
						new PointF (Convert.ToDouble (sFull[2]), Convert.ToDouble (sFull[3]))
						));
		}
	}

	public PointF GetCenterByCodeNum (int codeNum)
	{
		foreach (WilightPos wp in wp_l)
			if (wp.CodeNum == codeNum)
				return wp.Center;

		return new PointF (0, 0); //just in case
	}

	public int GetCodeNumByCodeLetter (string codeLetter)
	{
		foreach (WilightPos wp in wp_l)
			if (wp.CodeLetter == codeLetter)
				return wp.CodeNum;

		return -1; //not found
	}

	public WilightCommand ColorAll  (int colorCode)
	{
		string str = "";
		foreach (WilightPos wp in wp_l)
			str += string.Format ("{0}:{1};", wp.CodeNum, colorCode);

		return new WilightCommand (str);
	}

	private List<int> getTerminalListCodeNums ()
	{
		List<int> t_l = new List<int> ();
		foreach (WilightPos wp in wp_l)
			t_l.Add (wp.CodeNum);

		return t_l;
	}

	public int GetMinTerminal (bool excludeReferenceTerminal) //exclude the 0
	{
		List<int> t_l = UtilList.SortListInt (getTerminalListCodeNums ());
		if (excludeReferenceTerminal)
			return t_l[t_l.Count -2];
		else
			return t_l[t_l.Count -1];

	}
	public int GetMaxTerminal ()
	{
		List<int> t_l = UtilList.SortListInt (getTerminalListCodeNums ());
		return t_l[0];
	}
	//TODO: do a GetNext for use on applyBlacklist ()
}

/*
 * Note a command do not need explictely to have an expected return value, maybe we just want to animate the lights but have no user input (touch)
 * So to validate a command on creation we just need to check that we have pairs ints separated by : and each pair separated by ;. And also note that it ends with ;
 */

public class WilightCommand
{
	private string commandOriginalStr; //just for debug purposes
	private WilightTerminalLayout wilightTerminalLayout;

	private int level;
	private int timeMs;
	protected Stopwatch stopwatch;

	private List<WilightTerminalPair> wtp_l;

	//constructor
	public WilightCommand ()
	{
		initVariables ();
	}

	//constructor
	public WilightCommand (string str)
	{
		initVariables ();

		// 1. remove the last ; and split each of the commands
		str = str.Substring (0, str.LastIndexOf(';'));
		string [] strFull = str.Split (new char[] {';'});

		foreach (string s in strFull)
		{
			string [] sFull = s.Split (new char[] {':'});
			wtp_l.Add (new WilightTerminalPair (
						Convert.ToInt32 (sFull[0]),
						Convert.ToInt32 (sFull[1])
						));
		}
	}

	//constructor using a commandStr (readed from a file)
	public WilightCommand (
			string commandOriginalStr,
			WilightTerminalLayout wilightTerminalLayout,
			List<int> blacklist_l)
	{
		this.commandOriginalStr = commandOriginalStr;
		this.wilightTerminalLayout = wilightTerminalLayout;

		initVariables ();

		// 1. remove the last ; and split each of the commands
		string commandStr = commandOriginalStr.Substring (0, commandOriginalStr.LastIndexOf(';'));
		string [] commandFull = commandStr.Split (new char[] {';'});

		// 2. assing level, time, and create WilightTerminalPair list
		foreach (string s in commandFull)
		{
			string [] sFull = s.Split (new char[] {':'});
			if (sFull.Length != 2)
				continue;

			if (! Util.IsNumber (sFull[1], false))
				continue;

			if (sFull[0] == "Level") {
				level = Convert.ToInt32 (sFull[1]);
				continue;
			}

			if (sFull[0] == "Time") {
				timeMs = Convert.ToInt32 (sFull[1]);
				continue;
			}

			int terminalNum = wilightTerminalLayout.GetCodeNumByCodeLetter (sFull[0]);
			if (terminalNum < 0)
				continue;

			wtp_l.Add (new WilightTerminalPair (
						terminalNum,
						Convert.ToInt32 (sFull[1])
						));
		}
		applyBlacklist (blacklist_l);
	}

	private void initVariables ()
	{
		level = -1;
		timeMs = -1;
		stopwatch = new Stopwatch ();
		wtp_l = new List<WilightTerminalPair> ();
	}

	private void applyBlacklist (List<int> blacklist_l)
	{
		// 2. get the expected terminals
		List<int> expectedTerminals_l = GetExpectedTerminals ();
		if (expectedTerminals_l.Count == 0)
			return;

		// 3. get the necessary changes
		int minTerminal = wilightTerminalLayout.GetMinTerminal (true);
		int maxTerminal = wilightTerminalLayout.GetMaxTerminal ();
		List<IntInt> changes_l = new List<IntInt> ();
		foreach (int et in expectedTerminals_l)
		{
			// 3.a continue if expected terminal is not blackisted
			if (! UtilList.FoundInListInt (blacklist_l, et))
				continue;

			LogB.Information (string.Format ("\nYES OBJECT blacklist: command: '{0}', expected: {1}, blacklist: {2}",
						this.ToDebugString, et, UtilList.ListIntToSQLString (blacklist_l, " ")));

			// this expected terminal is blacklisted, change to another one
			// 3.b get any terminal that is not blacklistd and has a 0 code (or in the future any other terminal)
			// TODO: fix here as numbers of terminals do not neeed to be correlative, like in TR with 0 is used for platform
			int etOk = et+1;
			if (etOk > maxTerminal)
				etOk = minTerminal; //is not 0 because 0 is the reference terminal (top center)

			bool success = false;
			do {
				if (etOk == et)
					break; //to avoid a hang looping infinitely

				if (! UtilList.FoundInListInt (blacklist_l, etOk) &&
						! UtilList.FoundInListInt (expectedTerminals_l, etOk))
					success = true;
				else {
					// TODO: fix here as numbers of terminals do not neeed to be correlative, like in TR with 0 is used for platform
					etOk ++;
					if (etOk > maxTerminal)
						etOk = minTerminal; //is not 0 because 0 is the reference terminal (top center)
				}
			} while (! success);

			if (success)
				changes_l.Add (new IntInt (et, etOk));
		}
		if (changes_l.Count == 0)
			return;

		foreach (IntInt change in changes_l)
		{
			LogB.Information (string.Format ("blacklist change terminal {0} to terminal {1}", change.a, change.b));
			/*
			 * eg.blacklist of expected terminal 11
			 * to avoid sending two differerent codes to same terminal:
			 * from 0:8;11:9;12:6
			 * to   0:8;11:0;12:9
			 */
			// 1. expected colorCode = 0
			int colorCode = getTerminalColorCode (change.a);
			for (int i = 0; i < wtp_l.Count ; i ++)
				if (wtp_l[i].terminalNum == change.a)
					wtp_l[i].colorCode = 0;

			// 2. assign colorCode to the new terminal
			for (int i = 0; i < wtp_l.Count ; i ++)
				if (wtp_l[i].terminalNum == change.b)
					wtp_l[i].colorCode = colorCode;
		}
		LogB.Information (string.Format ("\nchanged command: {0}", this.ToDebugString));
	}

	public List<int> GetExpectedTerminals ()
	{
		List<int> expected_l = new List<int> ();

		//num of the reference terminal
		int terminalReference = wilightTerminalLayout.GetMinTerminal (false);
		int colorCode = getTerminalColorCode (terminalReference);
		if (colorCode < 0)
			return expected_l;

		foreach (WilightTerminalPair wtp in wtp_l)
			if (wtp.colorCode == colorCode +1)
				expected_l.Add (wtp.terminalNum);

		return expected_l;
	}

	private int getTerminalColorCode (int terminalNum)
	{
		foreach (WilightTerminalPair wtp in wtp_l)
			if (wtp.terminalNum == terminalNum)
				return wtp.colorCode;

		return -1;
	}

	public bool IsEmpty {
		get { return wtp_l.Count == 0; }
	}

	//without the level
	//with the last ;
	public string ToArduinoString {
		get { return wilightTerminalPairListToString (true) + ";"; }
	}

	public string ToDebugString {
		get { return string.Format ("Original command: {0}, Fixed command: {1}, Level: {2}, TerminalPairs: {3}",
				commandOriginalStr,
				ToArduinoString,
				level,
				wilightTerminalPairListToString (false));
		}
	}

	private string wilightTerminalPairListToString (bool toArduino)
	{
		string str = "";
		string sep = "";
		foreach (WilightTerminalPair wtp in wtp_l)
		{
			if (toArduino)
			{
				str += string.Format ("{0}{1}", sep, wtp.ToArduinoString);
				sep = ";";
			}
			else //to debug
				str += string.Format ("\n{0}", wtp.ToDebugString);
		}

		return str;
	}

	public void TimeStart ()
	{
		stopwatch.Start ();
	}
	public bool TimeFinished ()
	{
		return (stopwatch.ElapsedMilliseconds >= timeMs);
	}
	public void TimeStop ()
	{
		stopwatch.Start ();
	}

	public int Level {
		get { return level; }
	}
	public int TimeMs {
		get { return timeMs; }
	}
	public List<WilightTerminalPair> Wtp_l {
		get { return wtp_l; }
	}
}

//terminal num & colorCode
public class WilightTerminalPair
{
	public int terminalNum;
	public int colorCode;

	//constructor
	public WilightTerminalPair (int terminalNum, int colorCode)
	{
		this.terminalNum = terminalNum;
		this.colorCode = colorCode;
	}

	public string ToArduinoString {
		get { return string.Format ("{0}:{1}", terminalNum, colorCode); }
	}

	public string ToDebugString {
		get { return string.Format ("terminalNum: {0}, colorCode: {1}",
				terminalNum, colorCode); }
	}
}
