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
//using System.Diagnostics;  //Stopwatch

public class Wilight : Event
{
	private int exerciseID; //until wilightExercise table is not created, all will be 0
	private string dateTime;
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

	public static List<Event> WilightListToEventList (List<Wilight> ws)
	{
		List<Event> events = new List<Event>();
		foreach(Wilight w in ws)
			events.Add((Event) w);

		return events;
	}

	public int InsertSQL (bool dbconOpened)
	{
		return SqliteWilight.Insert (dbconOpened, toSQLInsertString());
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
	public string DateTime {
		get { return dateTime; }
	}
}


public static class WilightColors
{
	public static int OFF = 0;
	public static int RED = 2;
	public static int GREEN = 4;
	public static int BLUE = 8;
}

//note the command has been validated on validateCommand
public class WilightCommandToTerminals
{
	string commandStr;
	WilightTerminalLayout wtl;

	//constructor
	public WilightCommandToTerminals (string commandStr, WilightTerminalLayout wtl)
	{
		this.commandStr = commandStr;
		this.wtl = wtl;
	}

	public List<CairoGraphWilightTerminal> Do ()
	{
		List<CairoGraphWilightTerminal> wt_l = new List<CairoGraphWilightTerminal> ();

		//return if is empty
		if (commandStr == "")
			return wt_l;

		//return if not ends with ;
		//and also delete
		int lastSemicolon = commandStr.LastIndexOf(';');
		if (lastSemicolon != commandStr.Length -1)
			return wt_l;

		commandStr = commandStr.Substring (0, lastSemicolon);

		string [] commandStrFull = commandStr.Split (new char[] {';'});

		foreach (string terminalStr in commandStrFull)
		{
			string [] tsFull = terminalStr.Split(new char[] {':'});
			int id = Convert.ToInt32 (tsFull[0]);

			wt_l.Add (new CairoGraphWilightTerminal (
						id,
						Convert.ToInt32 (tsFull[1]),
						wtl.GetCenterByCodeNum (id)));
		}

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

			string [] sFull = s.Split(new char[] {';'});
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

		return 0; //just in case
	}

	public string ColorAll  (int colorCode)
	{
		string str = "";
		foreach (WilightPos wp in wp_l)
			str += string.Format ("{0}:{1};", wp.CodeNum, colorCode);

		return str;
	}

	/*
	 * at layout:
	 * #term name;term code;x (cm);y (cm)
	 * A;0;7.5;10
	 * B;1;1;8
	 * C;2;2;8
	 * D;3;3;8
	 *
	 * it will return 3 as there are 4 rows in Count
	 */
	public int GetMaxTerminal ()
	{
		return wp_l.Count -1;
	}
}

public class WilightTest
{
	private List<List<string>> command_ll;

	private int currentLevel;
	private int currentCommand; //in level
	private int commandsCountReceived;
	private Random random;
	private bool isRandom;

	public bool Cancel;
	public bool Finished;
	private List<string> onString_l;
	private int lastTime;
	List<int> blacklist_l;

	//passed params
	private WilightTerminalLayout wilightTerminalLayout;
	private string commandsFile;
	private bool isDemo;


	//constructor
	public WilightTest (WilightTerminalLayout wilightTerminalLayout, string commandsFile, string blacklistStr, bool isDemo)
	{
		this.wilightTerminalLayout = wilightTerminalLayout;
		this.commandsFile = commandsFile;
		this.isDemo = isDemo;

		command_ll = new List<List<string>> ();
		createBlacklist (blacklistStr);

		if (isDemo)
			wilightTestDemoSetVars ();
		else
			wilightTestRealSetVars ();

		commandsCountReceived = 0;

		Cancel = false;
		Finished = false;
		lastTime = 0;
		onString_l = new List<string> ();
	}

	private void createBlacklist (string blacklistStr)
	{
		blacklist_l = new List<int> ();
		if (blacklistStr == "")
			return;

		string [] strFull = blacklistStr.Split (new char[] {','});
		foreach (string s in strFull)
			if (Util.IsNumber (s, false))
				blacklist_l.Add (Convert.ToInt32 (s));

		return;
	}

	//not random
	private void wilightTestDemoSetVars ()
	{
		command_ll = readCommandsFrom (demoSequence);

		currentLevel = 0;
		currentCommand = 0;
		isRandom = false;
	}

	private void wilightTestRealSetVars ()
	{
		//if (commandsFile != "")
		//{
			List<List<string>> comReaded_ll = readCommandsFrom (
					Util.ReadFileAsStringList (commandsFile, "#"));

			// randomize lines in each level
			foreach (List <string> cnr_l in comReaded_ll)
				command_ll.Add (UtilList.ListRandomize1stAndThenSequential (cnr_l));

			// debug
			for (int i = 0; i < comReaded_ll.Count; i ++)
				LogB.Information (string.Format ("(Random) Level: {0} Commands:\n{1}",
							i, UtilList.ListStringToString (command_ll[i], "\n")));
		/*} else {
		 * 	disabled until all the colors get back to their value
			command_ll.Add (level0);
			command_ll.Add (level1);
			command_ll.Add (level2);
			command_ll.Add (level3);
			command_ll.Add (level4);
		}
		*/
	
		random = new Random();

		currentLevel = 0;
		currentCommand = 0;
		isRandom = true;
	}

	/*
	    reads a file like this:
		Level:0;A:8;B:0;C:0;D:0;E:9;F:0;G:0;H:0;I:0;J:0;K:0;L:0;M:0;
		Level:0;A:10;B:0;C:0;D:0;E:0;F:0;G:0;H:0;I:0;J:11;K:0;L:0;M:0;

		Level:1;A:6;B:0;C:64;D:0;E:0;F:32;G:0;H:0;I:7;J:0;K:128;L:0;M:0;
		Level:1;A:4;B:0;C:0;D:5;E:96;F:0;G:0;H:0;I:0;J:96;K:0;L:0;M:32;

	    Note the Levels not need to be ordered, and we can have Level 3 without having Level 2, ...
	    This should work:
		Level:3;A:96;B:97;C:0;D:64;E:0;F:6;G:0;H:0;I:8;J:14;K:0;L:32;M:0;
		Level:3;A:64;B:160;C:8;D:0;E:0;F:0;G:65;H:8;I:14;J:64;K:128;L:0;M:0;
		Level:1;A:8;B:0;C:0;D:160;E:128;F:0;G:0;H:9;I:0;J:0;K:0;L:0;M:32;

	    eg. last one will be converted to (A->0, M->12) depending on WilightTerminalLayout
	    Level:1;0:8;1:0;2:0;3:160;4:128;5:0;6:0;7:9;8:0;9:0;10:0;11:0;12:32;
	*/

	//note to be random this is readed at each new capture
	private List<List<string>> readCommandsFrom (List<string> com_l)
	{
		// 1. read the data (note lines don't need to come in a level order)
		List<List<string>> comReaded_ll = new List<List<string>> ();

		//LogB.Information (UtilList.ListStringToString (com_l, "\n"));
		// 2. parseCommandAndChangeLettersToNums. And add to comReaded_ll
		foreach (string com in com_l)
		{
			// 2.a get the level
			int level = parseCommandGetLevel (com);
			if (level < 0)
				continue;

			// 2.b add the sublists needed for that level
			while (comReaded_ll.Count <= level)
				comReaded_ll.Add (new List<string> ());

			// 2.c parse the command
			string comOk = parseCommandChangeLettersToNums (com);
			if (comOk == "")
				continue;

			if (blacklist_l.Count > 0)
				comOk = parseCommandWithBlacklist (comOk);

			// 2.d add the command to the sublist
			comReaded_ll[level].Add (comOk);
		}

		// 2.e debug
		for (int i = 0; i < comReaded_ll.Count; i ++)
			LogB.Information (string.Format ("(Not random) Level: {0} Commands:\n{1}",
						i, UtilList.ListStringToString (comReaded_ll[i], "\n")));
		return comReaded_ll;
	}

	private int parseCommandGetLevel (string com)
	{
		// 1. check line is ok
		// (5th condition: line needs at least one character more than the first ; to not fail on "add the command to the sublist")
		if (
				com == "" ||
				com.Length == 0 ||
				! com.StartsWith ("Level:") ||
				! com.EndsWith (";") ||
				com.Length <= com.IndexOf (';') -1 )
			return -1;

		// 2. get the level
		string levelStr = com.Substring (
				com.IndexOf (':') +1,
				com.IndexOf (';') -com.IndexOf (':') -1);

		if (! Util.IsNumber (levelStr, false))
			return -1;

		return Convert.ToInt32 (levelStr);
	}

	private string parseCommandChangeLettersToNums (string com)
	{
		// 1. remove the last ; and split each of the commands
		string comFix = com.Substring (0, com.LastIndexOf(';'));
		string [] comFixFull = comFix.Split(new char[] {';'});

		// 2. change the codeLetter for codeNum according to wilightTerminalLayout
		string comOk = "";
		bool isLevel = true;
		foreach (string sAB in comFixFull)
		{
			//discard the "Level:x;"
			if (isLevel)
			{
				isLevel = false;
				continue;
			}

			string [] sABFull = sAB.Split(new char[] {':'});
			if (sABFull.Length != 2)
				continue;

			comOk += string.Format ("{0}:{1};",
					wilightTerminalLayout.GetCodeNumByCodeLetter (sABFull[0]),
					sABFull[1]);
		}

		return comOk;
	}

	private string parseCommandWithBlacklist (string com)
	{
		// 1. remove the last ; and split each of the commands
		string comPre = com.Substring (0, com.LastIndexOf(';'));
		string [] comPreFull = comPre.Split (new char[] {';'});

		// 2. get the expected terminals
		List<int> expectedTerminals_l = GetExpectedTerminals (com);
		if (expectedTerminals_l.Count == 0)
			return com;

		// 3. get the necessary changes
		int maxTerminal = wilightTerminalLayout.GetMaxTerminal ();
		List<IntInt> changes_l = new List<IntInt> ();
		foreach (int et in expectedTerminals_l)
		{
			// 3.a continue if expected terminal is not blackisted
			if (! UtilList.FoundInListInt (blacklist_l, et))
				continue;

			LogB.Information (string.Format ("\nblacklist: command: '{0}', expected: {1}, blacklist: {2}",
						com, et, UtilList.ListIntToSQLString (blacklist_l, " ")));

			// this expected terminal is blacklisted, change to another one
			// 3.b get any terminal that is not blacklistd and has a 0 code (or in the future any other terminal)
			// TODO: fix here as numbers of terminals do not neeed to be correlative, like in TR with 0 is used for platform
			int etOk = et+1;
			if (etOk > maxTerminal)
				etOk = 1; //is not 0 because 0 is the reference terminal (top center)

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
						etOk = 1; //is not 0 because 0 is the reference terminal (top center)
				}
			} while (! success);

			if (success)
				changes_l.Add (new IntInt (et, etOk));
		}
		if (changes_l.Count == 0)
			return com;

		string comOld = com; //just for debug
		foreach (IntInt change in changes_l)
		{
			LogB.Information (string.Format ("blacklist change terminal {0} to terminal {1}", change.a, change.b));
			//to avoid sending two differerent codes to same terminal
			// 1. delete any command of the new terminal
			//TODO: use regex, or better a WilightCommand object
			int posStart = com.IndexOf (string.Format (";{0}:", change.b));
			if (posStart >= 0)
			{
				int posEnd = com.IndexOf (';', posStart+1);
				if (posEnd >= 0)
					com = com.Substring (0, posStart) +
						com.Substring (posEnd);
			}

			// 2. change the old (expected and blaclisted) to a non expected, non blacklisted
			com = Util.ChangeChars (com,
					string.Format (";{0}:", change.a),
					string.Format (";{0}:", change.b)
					);

			// 3. add the a:0 (if not will have the same code as previous command)
			com += string.Format ("{0}:0;", change.a);
		}
		LogB.Information (string.Format ("\n{0}\n{1}\n", comOld, com));

		return com;
	}

	//note if any problem it will return "" and this will be called again until Finished
	public string GetNext ()
	{
		LogB.Information (string.Format ("\nAt Wilight.GetNext, currentLevel: {0}, currentCommand: {1}",
					currentLevel, currentCommand));

		if (currentLevel >= command_ll.Count)
		{
			Finished = true;
			return "";
		}

		if (currentCommand >= command_ll[currentLevel].Count)
		{
			currentLevel ++;
			currentCommand = 0;
			return "";
		}

		//this is the commandStr that is going to be returned
		string commandStr = command_ll[currentLevel][currentCommand];
		currentCommand ++;

		if (! validateCommand (commandStr))
			return "";

		LogB.Information ("\nValidated command: " + commandStr);

		return commandStr;
	}

	public void AddToOnString (string str)
	{
		onString_l.Add (str);
	}

	public void SetLastOnTime (int time)
	{
		lastTime = time;
	}

	public void CommandsCountReceivedAdd ()
	{
		commandsCountReceived ++;
	}
	private int getTotalCommands ()
	{
		int sum = 0;
		foreach (List<string> c_l in command_ll)
			sum += c_l.Count;

		return sum;
	}
	public string GetProgressStatus ()
	{
		return string.Format ("{0} / {1} - Level: {2}", commandsCountReceived, getTotalCommands (), currentLevel);
	}

	//from a command detects wich is the terminal that will be active to be clicked. Can be plural
	public List<int> GetExpectedTerminals (string commandStr)
	{
		//LogB.Information ("commandStr:" + commandStr);
		List<int> expected_l = new List<int> ();

		//remove last semicolon on the command
		int lastSemicolon = commandStr.LastIndexOf(';');
		if (lastSemicolon != commandStr.Length -1)
			return expected_l;

		commandStr = commandStr.Substring (0, lastSemicolon);

		string [] commandStrFull = commandStr.Split(new char[] {';'});
		if (commandStrFull.Length < 2) //must have the data for the terminal 0 and for at least one terminal
			return expected_l;

		//it is the first one, do not need to be named 0
		string [] commandStrTerm0 = commandStrFull[0].Split(new char[] {':'});
		int commandTerm0 = Convert.ToInt32 (commandStrTerm0[1]);

		foreach (string cThisTerm in commandStrFull)
		{
			string [] cThisTermFull = cThisTerm.Split(new char[] {':'});
			if (Convert.ToInt32 (cThisTermFull[1]) == commandTerm0 +1)
				expected_l.Add (Convert.ToInt32 (cThisTermFull[0]));
		}
		return expected_l;
	}

	/*
	 * Note a command do not need explictely to have an expected return value, maybe we just want to animate the lights but have no user input (touch)
	 * So to validate a command on creation we just need to check that we have pairs ints separated by : and each pair separated by ;. And also note that it ends with ;
	 */

	private bool validateCommand (string commandStr)
	{
		//LogB.Information ("validateCommand Start");
		if (commandStr == "")
			return false;

		int lastSemicolon = commandStr.LastIndexOf(';');
		if (lastSemicolon != commandStr.Length -1)
			return false;

		commandStr = commandStr.Substring (0, lastSemicolon);

		string [] strFull = commandStr.Split(new char[] {';'});
		if (strFull.Length < 0)
			return false;

		foreach (string strX in strFull)
		{
			string [] strXFull = strX.Split(new char[] {':'});
			if (strXFull.Length != 2 ||
					! Util.IsNumber (strXFull[0], false) ||
					! Util.IsNumber (strXFull[1], false)
					)
				return false;
		}
		//LogB.Information ("validateCommand exit OK");
		return true;
	}

	/*
	 * disabled until all the colors get back to their value
	//S'encèn 1 llum amb pampallugues
	private List<string> level0
	{
		get {
			return (new List<string> {
					"0:8;1:0;2:0;3:0;4:9;5:0;6:0;7:0;8:0;9:0;10:0;11:0;12:0;",
					"0:10;1:0;2:0;3:0;4:0;5:0;6:0;7:0;8:0;9:11;10:0;11:0;12:0;",
					"0:14;1:0;2:0;3:0;4:0;5:0;6:0;7:0;8:0;9:0;10:0;11:0;12:15;",
					"0:2;1:0;2:0;3:0;4:0;5:0;6:0;7:0;8:3;9:0;10:0;11:0;12:0;",
					"0:6;1:0;2:0;3:0;4:0;5:0;6:0;7:7;8:0;9:0;10:0;11:0;12:0;",
					"0:12;1:0;2:0;3:0;4:0;5:0;6:0;7:0;8:0;9:0;10:13;11:0;12:0;",
					"0:4;1:0;2:0;3:0;4:0;5:5;6:0;7:0;8:0;9:0;10:0;11:0;12:0;"
					});
		}
	}

	//S'encenen 3 llums fixes i un amb pampallugues. Tocar el de pampallugues
	private List<string> level1
	{
		get {
			return (new List<string> {
					"0:6;1:0;2:64;3:0;4:0;5:32;6:0;7:0;8:7;9:0;10:128;11:0;12:0;",
					"0:4;1:0;2:0;3:5;4:96;5:0;6:0;7:0;8:0;9:192;10:0;11:0;12:32;",
					"0:8;1:0;2:0;3:160;4:128;5:0;6:0;7:9;8:0;9:0;10:0;11:0;12:32;",
					"0:2;1:3;2:0;3:64;4:0;5:0;6:0;7:96;8:0;9:0;10:64;11:0;12:0;",
					"0:10;1:0;2:0;192:0;4:0;5:0;6:0;7:192;8:0;9:11;10:0;11:0;12:96;",
					"0:12;1:0;2:0;3:224;4:96;5:0;6:0;7:0;8:0;9:0;10:160;11:13;12:0;",
					"0:14;1:0;2:192;3:64;4:0;5:15;6:0;7:0;8:0;9:0;10:0;11:32;12:0;"
					});
		}
	}

	//S'encenen 3 llums fixes i 3 amb pampallugues. Tocar un de pampallugues
	private List<string> level2
	{
		get {
			return (new List<string> {
					"0:6;1:0;2:0;3:128;4:32;5:7;6:12;7:0;8:32;9:0;10:0;11:2;12:0;",
					"0:2;1:160;2:0;3:3;4:0;5:0;6:64;7:6;8:0;9:12;10:96;11:0;12:0;",
					"0:10;1:64;2:11;3:0;4:0;5:0;6:0;7:2;8:2;9:32;10:0;11:0;12:160;",
					"0:4;1:192;2:0;3:0;4:0;5:32;6:5;7:0;8:0;9:2;10:0;11:128;12:6;",
					"0:14;1:4;2:0;3:160;4:0;5:0;6:96;7:0;8:12;9:15;10:0;11:0;12:64;",
					"0:12;1:13;2:0;3:0;4:224;5:0;6:224;7:0;8:160;9:14;10:10;11:0;12:0;",
					"0:8;1:0;2:10;3:0;4:32;5:224;6:6;7:0;8:0;9:0;10:64;11:0;12:9;"
					});
		}
	}

	//S'encenen 3 llums fixes i 3 amb pampallugues. Tocar un fix
	private List<string> level3
	{
		get {
			return (new List<string> {
					"0:96;1:97;2:0;3:64;4:0;5:6;6:0;7:0;8:8;9:14;10:0;11:32;12:0;",
					"0:224;1:160;2:8;3:0;4:0;5:0;6:225;7:12;8:14;9:64;10:128;11:0;12:0;",
					"0:32;1:160;2:14;3:0;4:0;5:8;6:6;7:192;8:0;9:0;10:33;11:0;12:224;",
					"0:192;1:4;2:6;3:10;4:193;5:10;6:0;7:0;8:224;9:8;10:14;11:0;12:192;",
					"0:64;1:0;2:0;3:12;4:224;5:65;6:0;7:0;8:128;9:0;10:0;11:4;12:10;",
					"0:128;1:192;2:4;3:0;4:0;5:0;6:0;7:0;8:129;9:4;10:64;11:0;12:10;",
					"0:160;1:224;2:0;3:161;4:2;5:0;6:6;7:0;8:12;9:0;10:32;11:0;12:0;"
					});
		}
	}

	//S'encenen 3 llums fixes i 6 amb pampallugues. Tocar un fix
	private List<string> level4
	{
		get {
			return (new List<string> {
					"0:192;1:64;2:0;3:4;4:96;5:2;6:10;7:0;8:193;9:14;10:14;11:0;12:2;",
					"0:64;1:4;2:10;3:12;4:32;5:10;6:14;7:0;8:32;9:14;10:65;11:0;12:0;",
					"0:128;1:12;2:14;3:129;4:4;5:32;6:224;7:12;8:0;9:10;10:12;11:0;12:0;",
					"0:32;1:8;2:2;3:128;4:4;5:96;6:33;7:0;8:4;9:10;10:2;11:0;12:0;",
					"0:224;1:8;2:8;3:8;4:96;5:6;6:12;7:12;8:0;9:128;10:225;11:0;12:0;",
					"0:96;1:2;2:4;3:14;4:12;5:10;6:0;7:8;8:64;9:97;10:224;11:0;12:0;",
					"0:160;1:96;2:6;3:0;4:0;5:0;6:6;7:10;8:128;9:0;10:14;11:161;12:0;",
					"0:128;1:128;2:128;3:128;4:128;5:128;6:128;7:128;8:128;9:128;10:128;11:128;12:128;"
					});
		}
	}
	*/

	//not random. 3 easy, 3 very complex.
	private List<string> demoSequence
	{
		get {
			return (new List<string> {
					"Level:0;A:8;B:0;C:0;D:0;E:0;F:0;G:0;H:9;I:0;J:0;K:0;L:0;M:0;",
					"Level:0;A:4;B:0;C:0;D:0;E:5;F:0;G:0;H:0;I:0;J:0;K:0;L:0;M:0;",
					"Level:0;A:34;B:0;C:35;D:0;E:0;F:0;G:0;H:0;I:0;J:0;K:0;L:0;M:0;",
					"Level:4;A:8;B:4;C:0;D:36;E:12;F:40;G:42;H:0;I:9;J:46;K:46;L:0;M:40;",
					"Level:4;A:4;B:36;C:42;D:42;E:8;F:42;G:46;H:0;I:8;J:46;K:5;L:0;M:0;",
					"Level:4;A:2;B:34;C:46;D:3;E:36;F:8;G:12;H:34;I:0;J:42;K:44;L:0;M:0;"
					});
		}
	}

	public bool IsDemo {
		get { return isDemo; }
	}

	public string OnStringAsString {
		get { return UtilList.ListStringToString (onString_l, "="); }
	}

	public int LastTime {
		get { return lastTime; }
	}
}
