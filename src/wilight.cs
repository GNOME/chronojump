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

public static class WilightColors
{
	public static string AllOffCommand = "0:0;1:0;2:0;3:0;4:0;5:0;6:0;7:0;8:0;9:0;10:0;11:0;12:0;";
	public static string AllRedCommand = "0:128;1:128;2:128;3:128;4:128;5:128;6:128;7:128;8:128;9:128;10:128;11:128;12:128;";
	public static string AllGreenCommand = "0:64;1:64;2:64;3:64;4:64;5:64;6:64;7:64;8:64;9:64;10:64;11:64;12:64;";
	public static string AllBlueCommand = "0:32;1:32;2:32;3:32;4:32;5:32;6:32;7:32;8:32;9:32;10:32;11:32;12:32;";
}

public class WilightTest
{
	private List<List<string>> command_ll;
	private int currentLevel;
	private int currentCommand; //in level
	private bool started;
	private Stopwatch stopwatch; 
	public bool Cancel;
	public bool Finished;
	public int FinishedMs;

	public WilightTest (string commandsFile)
	{
		command_ll = new List<List<string>> ();

		if (commandsFile != "")
		{
			command_ll.Add (Util.ReadFileAsStringList (commandsFile, "#"));
		} else {
			command_ll.Add (level0);
			command_ll.Add (level1);
			command_ll.Add (level2);
			command_ll.Add (level3);
			command_ll.Add (level4);
		}
	
		currentLevel = 0;
		currentCommand = 0;
		started = false;
		stopwatch = new Stopwatch ();
		Cancel = false;
		Finished = false;
		FinishedMs = 0;
	}

	public string GetNext ()
	{
		bool commandValidated = false;
		string commandStr = "";

		if (! started) {
			stopwatch.Start ();
			started = true;
		}

		do {
			commandStr = command_ll[currentLevel][currentCommand];

			if (currentCommand < command_ll[currentLevel].Count -1)
				currentCommand ++;
			else if (currentLevel < command_ll.Count -1)
			{
				currentLevel ++;
				currentCommand = 0;
			} else
			{
				Finished = true;
				FinishedMs = Convert.ToInt32 (stopwatch.ElapsedMilliseconds);
				stopwatch.Stop ();
			}

			commandValidated = validateCommand (commandStr);
		} while (! (commandValidated || Finished));

		//return "" if last command in list is not validated
		if (! commandValidated)
			return "";

		return commandStr;
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
		LogB.Information ("validateCommand exit OK");
		return true;
	}

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
					"0:160;1:96;2:6;3:0;4:0;5:0;6:6;7:10;8:128;9:0;10:14;11:161;12:0;"
					});
		}
	}
}
