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
using Gtk;
using System.Collections.Generic; //List
using Mono.Unix;


//to prepare data before calling cairo method
public abstract class CairoPaintBarsPre
{
	public bool ShowPersonNames; //to hide desc if not ShowPersonNames (because in ShowPersonNames, desc is name, but we do not want to see a comment there)

	//jump simple
	public PrepareEventGraphJumpSimple eventGraphJumpsStored;
	public bool UseHeights;

	//jump reactive
	public PrepareEventGraphJumpReactive eventGraphJumpsRjStored;

	//run simple
	public PrepareEventGraphRunSimple eventGraphRunsStored;
	public bool RunsTimes; //default: speeds

	//run interval
	public PrepareEventGraphRunInterval eventGraphRunsIntervalStored;
	//runEncoder
	public PrepareEventGraphRunEncoder eventGraphRunEncoderStored;
	//wilight
	public PrepareEventGraphWilight eventGraphWilightStored;
	//wilight
	public PrepareEventGraphFourPlatforms eventGraphFourPlatformsStored;
	//forceSensor
	public PrepareEventGraphForceSensor eventGraphForceSensorStored;
	//encoder
	public PrepareEventGraphEncoderCurrent eventGraphEncoderCurrentStored;
	public PrepareEventGraphEncoderSession eventGraphEncoderSessionStored;

	protected CairoBars cb;
	protected DrawingArea darea;
	protected string fontStr;
	protected Constants.Modes mode;
	protected string personName;
	protected int currentPersonID;
	protected bool drawBars; //false is plot points (Y > 0)
	protected string testName;
	protected string title;
	protected int pDN; //preferences.digitsNumber
	//protected string messageNoStoreCreated;
	protected double videoTime;
	protected string screenshotURL;

	protected void initialize (DrawingArea darea, string fontStr, Constants.Modes mode,
			string personName, string testName, int pDN)
	{
		this.darea = darea;
		this.fontStr = fontStr;
		this.mode = mode;
		this.personName = personName;
		this.testName = testName;
		this.pDN = pDN;
		this.screenshotURL = "";
	}

	// to debug
	public override string ToString ()
	{
		return string.Format(
				"mode: {0}, personName: {1}, testName: {2}, pDN: {3}",
				mode, personName, testName, pDN);
	}

	public bool ModeMatches (Constants.Modes mode)
	{
		LogB.Information(string.Format("ModeMatches. This mode: {0}, checking against: {1}, are equal: {2}",
					this.mode, mode, (this.mode == mode)));

		return (this.mode == mode);
	}

	public virtual void StoreEventGraphJumps (PrepareEventGraphJumpSimple eventGraph)
	{
	}
	public virtual void StoreEventGraphJumpsRj (PrepareEventGraphJumpReactive eventGraph)
	{
	}
	public virtual void StoreEventGraphRuns (PrepareEventGraphRunSimple eventGraph)
	{
	}
	public virtual void StoreEventGraphRunsInterval (PrepareEventGraphRunInterval eventGraph)
	{
	}
	public virtual void StoreEventGraphRunEncoder (PrepareEventGraphRunEncoder eventGraph)
	{
	}
	public virtual void StoreEventGraphWilight (PrepareEventGraphWilight eventGraph)
	{
	}
	public virtual void StoreEventGraphFourPlatforms (PrepareEventGraphFourPlatforms eventGraph)
	{
	}
	public virtual void StoreEventGraphForceSensor (PrepareEventGraphForceSensor eventGraph)
	{
	}
	public virtual void StoreEventGraphEncoderCurrent (PrepareEventGraphEncoderCurrent eventGraph)
	{
	}
	public virtual void StoreEventGraphEncoderSession (PrepareEventGraphEncoderSession eventGraph)
	{
	}

	public virtual void ShowMessage (DrawingArea darea, string fontTypeStr, string message)
	{
	}

	/*
	public void Prepare ()
	{
		if(mode == Constants.Modes.JUMPSSIMPLE)
			PrepareJumpSimpleGraph(eventGraphJumpsStored, false);
		else if(current_mode == Constants.Modes.RUNSSIMPLE)
			PrepareRunSimpleGraph(eventGraphRunsStored, false);
	}
	*/

	//used at start capture on realtime tests (jumpRj, runI)
	protected void blankScreen (DrawingArea darea, string fontStr)
	{
		try {
			new CairoBars1Series (darea, CairoBars.Type.NORMAL, fontStr, "");
		} catch {
			LogB.Information("Saved crash at with cairo paint (blank screen)");
		}
	}

	public void Paint ()
	{
		if(darea == null || darea.Window == null) //at start program, this can fail
			return;

		if(! storeCreated())
		{
			try {
				new CairoBars1Series (darea, CairoBars.Type.NORMAL, fontStr, ""); //messageNoStoreCreated);
			} catch {
				LogB.Information("saved crash at with cairo paint at !storeCreated");
			}
			return;
		}

		if(! haveDataToPlot())
		{
			try {
				if (getHistoricStr () == "")
					new CairoBars1Series (darea, CairoBars.Type.NORMAL, fontStr, testsNotFound());
				else
					new CairoBars1Series (darea, CairoBars.Type.NORMAL, fontStr, testsNotFound(),
							getHistoricD (), getHistoricStr ());
			} catch {
				LogB.Information("saved crash at with cairo paint at !haveDataToPlot");
			}
			return;
		}

		paintSpecific ();
		//darea.QueueDraw (); this makes the memory increase a lot! Just call queue when it is needed!
	}

	protected void passDataForScreenshotIfNeeded ()
	{
		if (cb != null && screenshotURL != null && screenshotURL != "")
			cb.ScreenshotURL = screenshotURL;
	}

	private string testsNotFound ()
	{
		if (Constants.ModeIsENCODER (mode))
			return testsNotFoundEncoder (); // saved repetitions
		else
			return testsNotFoundGeneric ();
	}

	private string testsNotFoundGeneric ()
	{
		if(personName != "")
		{
			if(testName != "")
				return string.Format(Catalog.GetString("{0} has not made any {1} test in this session."),
						personName, testName);
			else
				return string.Format(Catalog.GetString("{0} has not made any test in this session."),
						personName);
		} else {
			if(testName != "")
				return string.Format(Catalog.GetString("There are no {0} tests in this session."),
						testName);
			else
				return Catalog.GetString("No tests in this session.");
		}
	}

	private string testsNotFoundEncoder ()
	{
		if(personName != "")
		{
			if(testName != "")
				return string.Format(Catalog.GetString("{0} has not saved any repetitions in the {1} test of this session."),
						personName, testName);
			else
				return string.Format(Catalog.GetString("{0} has not saved any repetitions in this session."),
						personName);
		} else {
			if(testName != "")
				return string.Format(Catalog.GetString("No {0} test repetitions have been saved in this session."),
						testName);
			else
				return Catalog.GetString("No repetitions have been saved in this session.");
		}
	}

	protected abstract bool storeCreated ();
	protected abstract bool haveDataToPlot ();
	protected abstract void paintSpecific();

	// to show historic data even if in this session user has not data on that ex.
	protected virtual double getHistoricD ()
	{
		return 0;
	}
	protected virtual string getHistoricStr ()
	{
		return "";
	}

	protected string generateTitle ()
	{
		string titleStr = "";
		string sep = "";

		if(personName != "")
		{
			titleStr = personName;
			sep = " - ";
		}

		if(testName != "")
			titleStr += sep + testName;

		return titleStr;
	}


	//TODO: this is repeated on this file, think also if move it to gui/cairo/bars.cs
	protected int calculateMaxRowsForTextCairo (List<Event> events, int longestWordSize,
			bool allJumps, bool thereIsASimulated, bool secondDataRow)
	{
		int maxRows = 0;

		//LogB.Information("calculateMaxRowsForText");
		foreach(Event ev in events)
		{
			//LogB.Information("Event: " + ev.ToString());
			int rows = 0;
			if(allJumps) 			//to write the jump type (1st the jump type because it's only one row)
				rows ++;

			//try to pack small words if they fit in a row using wordsAccu (accumulated)
			string wordsAccu = "";
			string [] words = ev.Description.Split(new char[] {' '});

			foreach(string word in words)
			{
				if(wordsAccu == "")
					wordsAccu = word;
				else if( (wordsAccu + " " + word).Length <= longestWordSize )
					wordsAccu += " " + word;
				else {
					wordsAccu = word;
					rows ++;
				}
			}
			if(wordsAccu != "")
				rows ++;

			//if(ev.Simulated == -1) //to write simulated at bottom
			if(thereIsASimulated) //if a event has two lines but not simulated, it has to reserve a line for other events (maybe of 1 line with simulated)
				rows ++;

			if(secondDataRow)
				rows ++;

			if(rows > maxRows)
				maxRows = rows;
		}
		//LogB.Information("maxRows: " + maxRows.ToString());

		return maxRows;
	}

	protected string longestWord;
	protected int fontHeightForBottomNames;
	protected int maxRowsForText;
	protected int bottomMargin;

	//manage bottom text font/spacing of rows
	protected void calculateBottomParams (List<Event> events, bool allTypes, string addToType, string simulatedLabel, bool thereIsASimulated, bool secondDataRow)
	{
		longestWord = findLongestWordCairo (events, allTypes, addToType, simulatedLabel);
		fontHeightForBottomNames = cb.GetFontForBottomNames (events, longestWord);

		maxRowsForText = calculateMaxRowsForTextCairo (events, longestWord.Length,
				allTypes, thereIsASimulated, secondDataRow);
		bottomMargin = cb.GetBottomMarginForText (maxRowsForText, fontHeightForBottomNames);

		//LogB.Information(string.Format("fontHeightForBottomNames: {0}, bottomMargin: {1}", fontHeightForBottomNames, bottomMargin));
	}

	//TODO: need to add personName here
	protected string findLongestWordCairo (List<Event> events, bool allTypes, string addToType, string simulatedLabel)
	{
		int longestWordSize = 0;
		string longestWord = ""; //debug

		foreach(Event ev in events)
		{
			string [] textArray = ev.Description.Split(new char[] {' '});
			foreach(string text in textArray)
			{
				if(text.Length > longestWordSize)
				{
					longestWordSize = text.Length;
					longestWord = text;
				}
			}

			//note jump type will be in one line
			//TODO: check it in local user language (Catalog)
			if(allTypes && ev.Type != null && ev.Type.Length > longestWordSize)
			{
				longestWordSize = ev.Type.Length + addToType.Length;
				longestWord = ev.Type + addToType;
			}

			if(ev.Simulated == -1 && simulatedLabel.Length > longestWordSize)
			{
				longestWordSize = simulatedLabel.Length;
				longestWord = simulatedLabel;
			}
		}

		//LogB.Information("longestWord: " + longestWord);
		//return longestWordSize;
		return longestWord;
	}

	//person name or test type, or both
	//this can separate name with spaces on rows
	protected string createTextBelowBar(
			string secondResult, 	//time on runSimple
			string jumpType,
			string personName,
			bool thereIsASimulated, bool thisIsSimulated,
			int longestWordSize, int maxRowsForText)
	{
		string str = "";
		string vertSep = "";
		int rows = 0;

		if(secondResult != "")
		{
			str += vertSep + secondResult;
			vertSep = "\n";
			rows ++;
		}

		//if have to print jump type, print it first in one row
		if(jumpType != "")
		{
			str += vertSep + jumpType;
			vertSep = "\n";
			rows ++;
		}

		//method 1
		// 2) separate person name in rows and send it to plotTextBelowBarDoRow()
		//    packing small words if they fit in a row using wordsAccu (accumulated)

		string wordsAccu = "";
		string [] words = personName.Split(new char[] {' '});

		//bool newLineDone = false;
		foreach(string word in words)
		{
			if(wordsAccu == "")
				wordsAccu = word;
			else if( (wordsAccu + " " + word).Length <= longestWordSize )
				wordsAccu += " " + word;
			else {
				str += vertSep + wordsAccu;
				vertSep = "\n";
				//newLineDone = true;
				wordsAccu = word;
				rows ++;
			}
		}
		if(wordsAccu != "")
		{
			str += vertSep + wordsAccu;
			vertSep = "\n";
			rows ++;
		}

		/* method 2, two lines for name
		if(personName != "")
		{
			//separate in two lines
			string [] words = personName.Split(new char[] {' '});
			string firstLine;
			string secondLine;
			string space;
			int minLengthOfMaxRow = 1000;
			int bestCombination = 0;
			for(int i = 1; i < words.Length; i ++)
			{
				firstLine = "";
				space = "";
				for(int j = 0; j < i; j ++)
				{
					firstLine += space + words[j];
					space = " ";
				}

				secondLine = "";
				space = "";
				for(int j = i; j < words.Length; j ++)
				{
					secondLine += space + words[j];
					space = " ";
				}

				LogB.Information(string.Format("i: {0}, firstLine: {1}, length: {2}, secondLine: {3}, length: {4}",
							i, firstLine, firstLine.Length, secondLine, secondLine.Length));

				int maxOfThisCombination = firstLine.Length;
				if(secondLine.Length > maxOfThisCombination)
					maxOfThisCombination = secondLine.Length;

				if(maxOfThisCombination < minLengthOfMaxRow)
				{
					minLengthOfMaxRow = maxOfThisCombination;
					bestCombination = i;
				}
			}

			str += vertSep;
			vertSep = "\n";
			space = "";
			for(int i = 0; i < bestCombination; i ++)
			{
				str += space + words[i];
				space = " ";
			}
			str += vertSep;
			space = "";
			for(int i = bestCombination; i < words.Length; i ++)
			{
				str += space + words[i];
				space = " ";
			}
		}
		*/

		if(thereIsASimulated)
		{
			while(rows +1 < maxRowsForText)
			{
				str += "\n";
				rows ++;
			}

			str += "\n";
			if(thisIsSimulated)
				str += "(" + Catalog.GetString("Simulated") + ")"; //TODO: improve this to ensure it is last row
		} else {
			while(rows < maxRowsForText)
			{
				str += "\n";
				rows ++;
			}
		}

		return str;
	}

	public int FindBarInPixel (double px, double py)
	{
		LogB.Information(string.Format("FindBarInPixel cb == null: {0}, px: {1}, py: {2}", (cb == null), px, py));
		if(cb == null)
			return -1;

		return cb.FindBarInPixel (px, py);
	}
	public int FindBarIdInPixel (double px, double py)
	{
		LogB.Information(string.Format("FindBarIdInPixel cb == null: {0}, px: {1}, py: {2}", (cb == null), px, py));
		if(cb == null)
			return -1;

		return cb.FindBarIdInPixel (px, py);
	}

	public string ScreenshotURL
	{
		set { screenshotURL = value; }
	}

	protected CairoBars.BarsOrPoints barsOrPoints
	{
		get {
			if (drawBars)
				return	CairoBars.BarsOrPoints.BARS;
			else
				return	CairoBars.BarsOrPoints.POINTS;
		}
	}
}
