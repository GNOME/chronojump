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


public class WilightTestManage
{
	private List<List<WilightCommand>> wilightCommand_ll;

	private int currentLevel;
	private int currentCommand; //in level
	private int commandsCountReceived;
	private Random random;

	public bool Cancel;
	public bool Finished;
	private List<string> onString_l;
	private int lastTime;
	List<int> blacklist_l;
	private enum randomTypes { NO, BYLEVEL, ALL };
	private randomTypes randomType;

	//passed params
	private WilightTerminalLayout wilightTerminalLayout;
	private string commandsFile;
	private bool isDemo;


	//constructor
	public WilightTestManage (WilightTerminalLayout wilightTerminalLayout, string commandsFile, string blacklistStr, bool isDemo)
	{
		this.wilightTerminalLayout = wilightTerminalLayout;
		this.commandsFile = commandsFile;
		this.isDemo = isDemo;

		wilightCommand_ll = new List<List<WilightCommand>> ();
		createBlacklist (blacklistStr);
		randomType = randomTypes.BYLEVEL;

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
		wilightCommand_ll = readCommandsFrom (demoSequence);

		currentLevel = 0;
		currentCommand = 0;
	}

	private void wilightTestRealSetVars ()
	{
		//if (commandsFile != "")
		//{
			List<List<WilightCommand>> wilightCommandReaded_ll = readCommandsFrom (
					Util.ReadFileAsStringList (commandsFile, "#"));

			//TODO:
			/* if (randomType == randomTypes.ALL) 	// randomize all commands
				foreach (List <WilightCommand> wilightCommandReaded_l in wilightCommandReaded_ll)
					wilightCommand_ll.Add (UtilList.ListRandomize (wilightCommandReaded_l));
			else*/ if (randomType == randomTypes.BYLEVEL) 	// randomize commands in each level
				foreach (List <WilightCommand> wilightCommandReaded_l in wilightCommandReaded_ll)
					wilightCommand_ll.Add (UtilList.ListRandomize1stAndThenSequential (wilightCommandReaded_l));
			else if (randomType == randomTypes.NO) 	// do not randomize
				foreach (List <WilightCommand> wilightCommandReaded_l in wilightCommandReaded_ll)
					wilightCommand_ll.Add (wilightCommandReaded_l);

			// debug
			foreach (List<WilightCommand> wilightCommand_l in wilightCommand_ll)
				foreach (WilightCommand wilightCommand in wilightCommand_l)
				LogB.Information (wilightCommand.ToString ());
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
	private List<List<WilightCommand>> readCommandsFrom (List<string> com_l)
	{
		// 1. read the data (note lines don't need to come in a level order)
		List<List<string>> comReaded_ll = new List<List<string>> ();
		List<List<WilightCommand>> wilightCommandReaded_ll = new List<List<WilightCommand>> ();

		//LogB.Information (UtilList.ListStringToString (com_l, "\n"));
		foreach (string com in com_l)
		{
			/*
			if (com.StartsWith ("Random:All"))
			{
				randomType = randomTypes.ALL;
				continue;
			}
			*/
			if (com.StartsWith ("Random:ByLevel")) //default
			{
				randomType = randomTypes.BYLEVEL;
				continue;
			}
			if (com.StartsWith ("Random:No"))
			{
				randomType = randomTypes.NO;
				continue;
			}

			// create the WilightCommand to know the level
			WilightCommand wc = new WilightCommand (com, wilightTerminalLayout, blacklist_l);

			// add the sublists needed for that level
			while (wilightCommandReaded_ll.Count <= wc.Level)
				wilightCommandReaded_ll.Add (new List<WilightCommand> ());

			// add the command to the sublist
			wilightCommandReaded_ll[wc.Level].Add (wc);
		}

		// 2.e debug
		foreach (List<WilightCommand> wilightCommandReaded_l in wilightCommandReaded_ll)
			foreach (WilightCommand wilightCommandReaded in wilightCommandReaded_l)
				LogB.Information (wilightCommandReaded.ToString ());

		return wilightCommandReaded_ll;
	}

	//note if any problem it will return "" and this will be called again until Finished
	public WilightCommand GetNext ()
	{
		LogB.Information (string.Format ("\nAt Wilight.GetNext, currentLevel: {0}, currentCommand: {1}",
					currentLevel, currentCommand));

		if (currentLevel >= wilightCommand_ll.Count)
		{
			Finished = true;
			return new WilightCommand ();
		}

		if (currentCommand >= wilightCommand_ll[currentLevel].Count)
		{
			currentLevel ++;
			currentCommand = 0;
			return new WilightCommand ();
		}

		//this is the commandStr that is going to be returned
		WilightCommand wilightCommand = wilightCommand_ll[currentLevel][currentCommand];
		currentCommand ++;

		return wilightCommand;
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
		foreach (List<WilightCommand> wc_l in wilightCommand_ll)
			sum += wc_l.Count;

		return sum;
	}
	public string GetProgressStatus ()
	{
		return string.Format ("{0} / {1} - Level: {2}",
				commandsCountReceived, getTotalCommands (), currentLevel);
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

