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
 *  Copyright (C) 2004-2023   Xavier de Blas <xaviblas@gmail.com>
 */

using System;
using System.Data;
using System.IO; 	//TextWriter
using System.Xml;	//XmlTextWriter
using Gtk;		//FileSelection widget
using System.Collections; //ArrayList
using System.Collections.Generic; //List
using Mono.Unix;

public abstract class ExportSession
{
	public enum DoneEnumType { CANCEL, NODATA, CANNOTCOPY, SUCCESS };
	public DoneEnumType DoneEnum;

	protected Gtk.Button fakeButtonDone;
	protected string filename;

	protected ArrayList myPersonsAndPS;
	List<string> jumpTypes_l;
	protected List<List<SqliteStruct.IntTypeDoubleDouble>> jumps_ll;
	protected string [] myJumps;
	protected string [] myJumpsRj;
	protected string [] myRuns;
	protected string [] myRunsInterval;
	/*
	protected string [] myReactionTimes;
	protected string [] myPulses;
	protected string [] myMCs;
	*/
	protected Session mySession;
	protected TextWriter writer;
	protected static Gtk.Window app1;
	
	public Preferences preferences;
					
	protected string spreadsheetString;

	protected string modeForFilename;
	protected int personID; // -1 for all
	protected string personName; // just to name file if personID >= 0int
	protected int sessionID; // -1 for all
	protected bool jumpsSimple;
	protected bool jumpsSimpleMeanMaxTables;
	protected bool jumpsReactive;
	protected bool runsSimple;
	protected bool runsIntervallic;

	protected bool showDialogMessage; //on report

	protected void checkFile (string formatFile)
	{
		string exportString = "";
		if(formatFile == "report") {
			exportString = Catalog.GetString ("Save report as …");
		} else {
			exportString = Catalog.GetString ("Export session in format " + formatFile);
		}

		
		Gtk.FileChooserDialog fc=
			new Gtk.FileChooserDialog(exportString,
					app1,
					FileChooserAction.Save,
					Catalog.GetString("Cancel"),ResponseType.Cancel,
					Catalog.GetString("Export"),ResponseType.Accept
					);
	
		//set default name
		string nameString = "";
		if (sessionID >= 0)
			nameString += mySession.Name + "_";
		if (personID >= 0)
			nameString += personName + "_";

		nameString += modeForFilename;

		if (sessionID >= 0)
			nameString += "_" + mySession.DateShortAsSQL;

		if(formatFile == "report") {
			if(UtilAll.IsWindows())
				nameString += ".htm";
			else
				nameString += ".html";
		} else
			nameString += ".csv";

		fc.CurrentName = nameString;

		if (fc.Run() == (int)ResponseType.Accept) 
		{
			filename = fc.Filename;
			if(formatFile == "report") {
				//add ".html" if needed, remember that on windows should be .htm
				filename = addHtmlIfNeeded(filename);
			} else {
				//add ".csv" if needed
				filename = Util.AddCsvIfNeeded(filename);
			}

			if (File.Exists(filename)) {
				LogB.Warning(string.Format("File {0} exists with attributes {1}, created at {2}",
							filename, File.GetAttributes(filename), File.GetCreationTime(filename)));
				LogB.Information("Overwrite …");
				ConfirmWindow confirmWin = ConfirmWindow.Show(Catalog.GetString("Are you sure you want to overwrite file: "),
						"", filename);
				confirmWin.Button_accept.Clicked += new EventHandler(on_overwrite_file_accepted);
			} else {
				writeFile();
			}
		}
		else {
			LogB.Information("cancelled");
			//report does not currently send the appBar reference
			//if(formatFile != "report") {
			//	new DialogMessage(Constants.MessageTypes.INFO, Catalog.GetString("Cancelled."));
			//}
			fc.Hide ();
			DoneEnum = DoneEnumType.CANCEL;
			fakeButtonDone.Click ();
			return;
		}
		
		//Don't forget to call Destroy() or the FileChooserDialog window won't get closed.
		fc.Destroy();
	}

	private void on_overwrite_file_accepted(object o, EventArgs args)
	{
		writeFile();
	}

	private void writeFile()
	{
		if (! getData())
		{
			DoneEnum = DoneEnumType.NODATA;
			fakeButtonDone.Click ();
			return;
		}

		// 1) create temp file to not have problems with a previously opened html on windows browser
		string tempfile = Path.GetTempFileName();
		try {
			writer = File.CreateText(tempfile);
		} catch {
			LogB.Information("Couldn't create tempfile: " + tempfile);
			if (showDialogMessage)
				new DialogMessage( Constants.MessageTypes.WARNING,
						string.Format(Catalog.GetString("Cannot export to file {0} "), tempfile) );

			DoneEnum = DoneEnumType.CANNOTCOPY;
			fakeButtonDone.Click ();
			return;
		}

		printData();
		closeWriter();

		// 2) copy temp file to user destination
		try {
			File.Copy(tempfile, filename, true); //can be overwritten
		} catch {
			LogB.Information("Couldn't copy to: " + filename);
			if (showDialogMessage)
				new DialogMessage( Constants.MessageTypes.WARNING,
						string.Format(Catalog.GetString("Cannot export to file {0} "), filename) );

			DoneEnum = DoneEnumType.CANNOTCOPY;
			fakeButtonDone.Click ();
			return;
		}

		// 3) show message
		if (showDialogMessage)
			new DialogMessage(Constants.MessageTypes.INFO,
					string.Format(Catalog.GetString("Saved to {0}"), filename) + spreadsheetString);

		DoneEnum = DoneEnumType.SUCCESS;
		fakeButtonDone.Click ();
	}
		
	private string addHtmlIfNeeded(string myFile)
	{
		int posOfDot = myFile.LastIndexOf('.');
		if (posOfDot == -1) {
			if(UtilAll.IsWindows())
				myFile += ".htm";
			else
				myFile += ".html";
		}
		return myFile;
	}
	
	protected virtual bool getData()
	{
		myPersonsAndPS = SqlitePersonSession.SelectCurrentSessionPersons(mySession.UniqueID, true);

		//Leave SQL opened in all this process
		Sqlite.Open(); // ------------------------------

		jumps_ll = new List<List<SqliteStruct.IntTypeDoubleDouble>> ();

		if (jumpsSimpleMeanMaxTables)
		{
			jumpTypes_l = SqliteJump.SelectJumpsTypeInSession (true, sessionID, personID);
			foreach (string type in jumpTypes_l)
				jumps_ll.Add (SqliteJump.SelectJumpsToCSVExport (true, sessionID, personID, type));
		}

		bool hasData = false;
		if (jumpsSimple) {
			myJumps = SqliteJump.SelectJumpsSA (true, sessionID, personID, "", "",
					Sqlite.Orders_by.DEFAULT, 0);
			if (myJumps.Length > 0)
				hasData = true;
		}

		if (jumpsReactive) {
			myJumpsRj = SqliteJumpRj.SelectJumpsSA (true, sessionID, personID, "", "");
			if (myJumpsRj.Length > 0)
				hasData = true;
		}

		if(runsSimple) {
			myRuns = SqliteRun.SelectRunsSA (true, sessionID, personID, "",
					Sqlite.Orders_by.DEFAULT, 0);
			if (myRuns.Length > 0)
				hasData = true;
		}

		if (runsIntervallic) {
			myRunsInterval = SqliteRunInterval.SelectRunsSA (true, sessionID, personID, "");
			if (myRunsInterval.Length > 0)
				hasData = true;
		}

		/*
		myReactionTimes = SqliteReactionTime.SelectReactionTimes(true, mySession.UniqueID, -1, "",
				Sqlite.Orders_by.DEFAULT, -1);

		myPulses = SqlitePulse.SelectPulses(true, mySession.UniqueID, -1);
		myMCs = SqliteMultiChronopic.SelectTests(true, mySession.UniqueID, -1);
		*/
		
		Sqlite.Close(); // ------------------------------
		return hasData;
	}

	protected virtual void printTitles (string title)
	{
		writer.WriteLine ("");
		writer.WriteLine ("+ " + title.ToUpper ());
	}

	protected virtual void printSubTitles (string subtitle)
	{
		writer.WriteLine ("");
		writer.WriteLine ("- " + subtitle.ToUpper ());
	}

	protected virtual void printData ()
	{
		/*
		printTitles(Catalog.GetString("Session"));
		printSessionInfo();
		
		printTitles(Catalog.GetString("Persons"));
		printPersons();
		*/
		
		if (jumpsSimple)
			printJumps (Catalog.GetString("Simple jumps"));
		
		if (jumpsReactive)
			printJumpsRj (true, Catalog.GetString("Reactive jumps") +
					" (" + Catalog.GetString("with subjumps") + ")");

		if (runsSimple)
			printRuns (Catalog.GetString("Simple races"));

		if (runsIntervallic)
			printRunsInterval (true, Catalog.GetString("interval races") +
					" (" + Catalog.GetString("with laps") + ")");

		/*
		printReactionTimes(Catalog.GetString("Reaction times"));
		
		printPulses(Catalog.GetString("Pulses"));
		
		printMCs(Catalog.GetString("MultiChronopic"));

		printFooter();
		*/
	}

	protected virtual void writeData (ArrayList exportData) {
	}

	protected virtual void writeData (string exportData) {
	}

	
	protected virtual void printSessionInfo()
	{
		ArrayList myData = new ArrayList(2);
		myData.Add( "\n" + 
				Catalog.GetString ("SessionID") + ":" +
				Catalog.GetString ("Name") + ":" +
				Catalog.GetString ("Place") + ":" + 
				Catalog.GetString ("Date") + ":" + 
				Catalog.GetString ("Comments") );
		myData.Add ( mySession.UniqueID + ":" + mySession.Name + ":" +
					mySession.Place + ":" + mySession.DateShort + ":" + 
					Util.RemoveNewLine(mySession.Comments, true) );
		writeData(myData);
		writeData("VERTICAL-SPACE");
	}

	protected void printPersons()
	{
		//PERSON STUFF
		ArrayList myData = new ArrayList(1);
		myData.Add ( "\n" + Catalog.GetString ("ID") + ":" + Catalog.GetString ("Name") + ":" +
				Catalog.GetString ("Sex") + ":" + Catalog.GetString ("Date of Birth") + ":" +
				Catalog.GetString ("Description") + ":" + 
				Catalog.GetString ("Height") + ":" + Catalog.GetString("Weight") + ":" +
				Catalog.GetString ("Sport") + ":" + Catalog.GetString("Specialty") + ":" +
				Catalog.GetString ("Level") + ":" + Catalog.GetString ("Comments")
			   );

		Sqlite.Open();	
		foreach (PersonAndPS paps in myPersonsAndPS) {
			string sportName = (SqliteSport.Select(true, paps.ps.SportID)).Name;
			string speciallityName = SqliteSpeciallity.Select(true, paps.ps.SpeciallityID);
			
			myData.Add(
					paps.p.UniqueID.ToString() + ":" + paps.p.Name + ":" +
					paps.p.Sex + ":" + paps.p.DateBorn.ToShortDateString() + ":" +
					Util.RemoveNewLine(paps.p.Description, true) + ":" +
					paps.ps.Height + ":" + paps.ps.Weight + ":" + 
					sportName + ":" + speciallityName + ":" +
					Util.FindLevelName(paps.ps.Practice) + ":" +
					Util.RemoveNewLine(paps.ps.Comments, true)
				  );
		}
		Sqlite.Close();	
		
		writeData(myData);
		writeData("VERTICAL-SPACE");
	}
	
	protected string getPower(double tc, double tf, double personWeight, double extraWeightInKg, double fall) 
	{
		int dec = preferences.digitsNumber; //decimals
		if(tf > 0) {	
			if(tc > 0) 		//dj
				return Util.TrimDecimals (Jump.GetDjPower(tc, tf, (personWeight + extraWeightInKg), fall).ToString(), dec);
			else 			//it's a simple jump without tc
				return Util.TrimDecimals (Jump.GetPower(tf, personWeight, extraWeightInKg).ToString(), dec);
		}
		return "-";
	}

	protected void printJumps(string title)
	{
		int dec = preferences.digitsNumber; //decimals
		
		string weightName = Catalog.GetString("Weight");
		if(preferences.weightStatsPercent)
			weightName += " %";
		else
			weightName += " Kg";

		if(myJumps.Length > 0 || jumps_ll.Count > 0)
			printTitles(title);

		if (jumps_ll.Count > 0)
		{
			printJumpsAvgMaxTable (true, dec);
			printJumpsAvgMaxTable (false, dec);
		}

		if(myJumps.Length > 0)
			printJumpsFullData (dec, weightName);
	}

	private void printJumpsAvgMaxTable (bool avgOrMax, int dec)
	{
		if (avgOrMax)
			printSubTitles (Catalog.GetString ("Height averages"));
		else
			printSubTitles (Catalog.GetString ("Height maximums"));

		ArrayList myData = new ArrayList(1);
		myData.Add (
				Catalog.GetString ("Person ID") + ":" +
				Catalog.GetString ("Person name") + ":" +
				Util.ListStringToString (jumpTypes_l, ":")
			  );

		foreach (PersonAndPS paps in myPersonsAndPS)
		{
			//on -1 show all persons, but on selected personID only show that
			if (personID >= 0 && personID != paps.p.UniqueID)
				continue;

			string str = paps.p.UniqueID + ":" + paps.p.Name + ":";

			foreach (List<SqliteStruct.IntTypeDoubleDouble> jumps_l in jumps_ll)
			{
				SqliteStruct.IntTypeDoubleDouble idd =
					SqliteStruct.IntTypeDoubleDouble.FindRowFromPersonID (
							jumps_l, paps.p.UniqueID);

				if (idd.personID < 0)
					str += " :";
				else
				{
					if (avgOrMax)
						str += string.Format ("{0}:", Util.TrimDecimals (idd.avg, dec));
					else
						str += string.Format ("{0}:", Util.TrimDecimals (idd.max, dec));
				}
			}
			myData.Add (str);
		}

		writeData(myData);
		writeData("VERTICAL-SPACE");
	}

	private void printJumpsFullData (int dec, string weightName)
	{
		ArrayList myData = new ArrayList(1);
		myData.Add( "\n" + 
				Catalog.GetString("Person ID") + ":" +
				Catalog.GetString("Person name") + ":" +
				Catalog.GetString("jump ID") + ":" + 
				Catalog.GetString("Type") + ":" + 
				Catalog.GetString("TC") + ":" + 
				Catalog.GetString("TF") + ":" + 
				Catalog.GetString("Fall") + ":" + 
				weightName + ":" + 
				Catalog.GetString("Height") + ":" +
				Catalog.GetString("Power") + ":" +
				Catalog.GetString("Stiffness") + ":" +
				Catalog.GetString("Initial Speed") + ":" +
				"RSI" + ":" +
				Catalog.GetString("Datetime") + ":" +
				Catalog.GetString("Description") + ":" +
				//Catalog.GetString("Angle") + ":" +
				Catalog.GetString("Simulated") 
			  );


		foreach (string jumpString in myJumps) {
			string [] myStr = jumpString.Split(new char[] {':'});


			//find weight of person and extra weight
			int papsPosition = PersonAndPSUtil.Find(myPersonsAndPS, 
					Convert.ToInt32(myStr[2])); //personID

			if(papsPosition == -1) {
				LogB.Error("PersonsAndPSUtil don't found person:", myStr[2]);
				return;
			}

			double personWeight = ((PersonAndPS) myPersonsAndPS[papsPosition]).ps.Weight;
			double extraWeightInKg = Util.WeightFromPercentToKg(
					Convert.ToDouble(myStr[8]), 
					personWeight);

			string extraWeightPrint = "";
			if(preferences.weightStatsPercent)
				extraWeightPrint = myStr[8];
			else
				extraWeightPrint = extraWeightInKg.ToString();

			//end of find weight of person and extra weight

			double fall = Convert.ToDouble(myStr[7]);
			double tc = Convert.ToDouble(myStr[6]);
			double tf = Convert.ToDouble(myStr[5]);

			myData.Add (	
					myStr[2] + ":" +  myStr[0] + ":" +  	//person.UniqueID, person.Name
					myStr[1] + ":" +  			//jump.uniqueID
					myStr[4] + ":" +  Util.TrimDecimals(myStr[6], dec) + ":" + 	//jump.type, jump.tc
					Util.TrimDecimals(myStr[5], dec) + ":" +  Util.TrimDecimals(myStr[7], dec) + ":" + 	//jump.tv, jump.fall
					Util.TrimDecimals(extraWeightPrint, dec) + ":" +
					Util.TrimDecimals(Util.GetHeightInCentimeters(myStr[5]), dec) + ":" +  
					Util.TrimDecimals(getPower(tc, tf, personWeight, extraWeightInKg, fall), dec) + ":" +
					Util.TrimDecimals(Util.GetStiffness(personWeight, extraWeightInKg, tf, tc), dec) + ":" +
					Util.TrimDecimals(Jump.GetInitialSpeed(myStr[5], preferences.metersSecondsPreferred), dec) + ":" +  //true: m/s
					Util.TrimDecimals(UtilAll.DivideSafe(Util.GetHeightInMeters(tf), tc), dec) + ":" +
					myStr[12] + ":" +	//jump.datetime
					Util.RemoveNewLine(myStr[9], true) + ":" +	//jump.description
					//Util.TrimDecimals(myStr[10],dec) + ":" +	//jump.angle
					Util.SimulatedTestNoYes(Convert.ToInt32(myStr[11]))		//jump.simulated

				   );
		}

		writeData(myData);
		writeData("VERTICAL-SPACE");
	}

	protected void printJumpsRj(bool showSubjumps, string title)
	{
		int dec=preferences.digitsNumber; //decimals

		ArrayList myData = new ArrayList(1);
		bool isFirstHeader = true;
		
		if(myJumpsRj.Length > 0) 
			printTitles(title); 

		foreach (string jump in myJumpsRj) {
			
			if(showSubjumps) {
				myData = new ArrayList(1);
			}

			string weightName = Catalog.GetString("Weight");
			if(preferences.weightStatsPercent)
				weightName += " %";
			else
				weightName += " Kg";

			//TODO: add power and stiffness

			//if show subjumps show this every time, else show only one
			if(isFirstHeader || showSubjumps) {
				myData.Add( "\n" + 
						Catalog.GetString("Person ID") + ":" +
						Catalog.GetString("Person name") + ":" + 
						Catalog.GetString("jump ID") + ":" + 
						Catalog.GetString("jump Type") + ":" + 
						Catalog.GetString("TC Max") + ":" + 
						Catalog.GetString("TF Max") + ":" + 
						Catalog.GetString("Max Height") + ":" +
						Catalog.GetString("Max Initial Speed") + ":" +
						"Max RSI" + ":" +
						Catalog.GetString("TC AVG") + ":" + 
						Catalog.GetString("TF AVG") + ":" + 
						Catalog.GetString("AVG Height") + ":" +
						Catalog.GetString("AVG Initial Speed") + ":" +
						"AVG RSI" + ":" +
						Catalog.GetString("Fall") + ":" + 
						weightName + ":" + 
						Catalog.GetString("Jumps") + ":" + 
						Catalog.GetString("Time") + ":" + 
						Catalog.GetString("Limited") + ":" + 
						Catalog.GetString("Datetime") + ":" +
						Catalog.GetString("Description") + ":" +
						//Catalog.GetString("Angles") + ":" +
						Catalog.GetString("Simulated") 
					  );
				isFirstHeader = false;
			}
		
			string [] myStr = jump.Split(new char[] {':'});

			
			//find weight of person and extra weight
			int papsPosition = PersonAndPSUtil.Find(myPersonsAndPS, 
					Convert.ToInt32(myStr[2])); //personID

			if(papsPosition == -1) {
				LogB.Error("PersonsAndPSUtil don't found person:", myStr[2]);
				return;
			}

			double personWeight = ((PersonAndPS) myPersonsAndPS[papsPosition]).ps.Weight;
			double extraWeightInKg = Util.WeightFromPercentToKg(
					Convert.ToDouble(myStr[8]), 
					personWeight);

			string extraWeightPrint = "";
			if(preferences.weightStatsPercent)
				extraWeightPrint = myStr[8];
			else
				extraWeightPrint = extraWeightInKg.ToString();
			//end of find weight of person and extra weight

			//used for subjumps and for MAX/AVG RSI calculation
			string [] tvString = myStr[12].Split(new char[] {'='});
			string [] tcString = myStr[13].Split(new char[] {'='});

			// calculate RSI stuff ---->
			double maxRSI = 0;
			double sumRSI = 0;
			double avgRSI = 0;
			if(tvString.Length > 0 && tvString.Length == tcString.Length) //just a precaution
			{
				for(int i = 0; i < tvString.Length; i ++)
				{
					double rsi = Jump.CalculateRSI(Convert.ToDouble(tvString[i]), Convert.ToDouble(tcString[i]));
					if(rsi > maxRSI)
						maxRSI = rsi;

					sumRSI += rsi;
				}
				avgRSI = UtilAll.DivideSafe(sumRSI, tvString.Length);
			}
			// < ---- end of calculate RSI stuff

			
			myData.Add ( 
					myStr[2] + ":" +    			//jumpRj.personID
					myStr[0] + ":" +  myStr[1] + ":" +  	//person.name, jumpRj.uniqueID
					//myStr[2] + ":" +  myStr[3] + ":" +  	//jumpRj.personID, jumpRj.sessionID
					myStr[4] + ":" +  		//jumpRj.type 
					Util.TrimDecimals(myStr[6], dec) + ":" +  		//jumpRj.tcMax 
					Util.TrimDecimals(myStr[5], dec) + ":" + 		//jumpRj.tvMax
					Util.TrimDecimals(Util.GetHeightInCentimeters(myStr[5]), dec) + ":" +  	//Max height
					Util.TrimDecimals(Jump.GetInitialSpeed(
							myStr[5], preferences.metersSecondsPreferred), dec) + ":" +  	//Max initial speed (true:m/s)
					Util.TrimDecimals(maxRSI, dec) + ":" +
					Util.TrimDecimals(myStr[11], dec) + ":" +  		//jumpRj.tcAvg
					Util.TrimDecimals(myStr[10], dec) + ":" + 		//jumpRj.tvAvg
					Util.TrimDecimals(Util.GetHeightInCentimeters(myStr[10]), dec) + ":" +  //Avg height
					Util.TrimDecimals(Jump.GetInitialSpeed(
							myStr[10], preferences.metersSecondsPreferred), dec) + ":" +  	//Avg Initial speed (true:m/s)
					Util.TrimDecimals(avgRSI, dec) + ":" +
					myStr[7] + ":" + 	 	//jumpRj.Fall
					//myStr[8] + ":" +  myStr[14] + ":" + 	//jumpRj.Weight, jumpRj.Jumps
					Util.TrimDecimals(extraWeightPrint,dec) + ":" +  myStr[14] + ":" + 	//jumpRj.Weight, jumpRj.Jumps
					Util.TrimDecimals(myStr[15], dec) + ":" +  Util.GetLimitedRounded(myStr[16],dec) + ":" + 	//jumpRj.Time, jumpRj.Limited
					myStr[19] + ":" +	//jumpRj.datetime
					Util.RemoveNewLine(myStr[9], true) + ":" + 	//jumpRj.Description
					//myStr[17] + ":" + 	//jumpRj.Angle
					Util.SimulatedTestNoYes(Convert.ToInt32(myStr[18]))		//simulated
					);
			
			if(showSubjumps) 
			{
				writeData(myData);
			
				myData = new ArrayList(1);
				int count = 0;
				myData.Add( " " + ":" + Catalog.GetString("TC") + 
						":" + Catalog.GetString("TF") + 
						":" + Catalog.GetString("Height") + 
						":" + "RSI" +
						":" + Catalog.GetString("Power") + 
						":" + Catalog.GetString("Stiffness") 
						);

				//print Total, AVG, SD
				myData.Add(Catalog.GetString("Total") + ":" +
						Util.TrimDecimals(Util.GetTotalTime(myStr[13]).ToString(), dec) + ":" +
						Util.TrimDecimals(Util.GetTotalTime(myStr[12]).ToString(), dec));
				myData.Add(Catalog.GetString("AVG") + ":" +
						Util.TrimDecimals(Util.GetAverage(myStr[13]).ToString(), dec) + ":" +
						Util.TrimDecimals(Util.GetAverage(myStr[12]).ToString(), dec));
				myData.Add(Catalog.GetString("SD") + ":" + 
						Util.TrimDecimals(Util.CalculateSD(
								Util.ChangeEqualForColon(myStr[13]),
								Util.GetTotalTime(myStr[13]),
								Util.GetNumberOfJumps(myStr[13], false)).ToString(),
							dec) + ":" + 
						Util.TrimDecimals(Util.CalculateSD(
								Util.ChangeEqualForColon(myStr[12]),
								Util.GetTotalTime(myStr[12]),
								Util.GetNumberOfJumps(myStr[12], false)).ToString(),
							dec));
				
				foreach(string myTv in tvString) 
				{
					double tc = Convert.ToDouble(tcString[count]);
					double tv = Convert.ToDouble(myTv);
					
					//on first jump use fall from RJ option
					//on next jumps calculate from previous TV
					double fall;
					if(count == 0)
						fall = Convert.ToDouble(myStr[7]); //jumpRj.Fall
					else
						fall = Convert.ToDouble(Util.GetHeightInCentimeters(tvString[count -1].ToString()));

					myData.Add((count+1).ToString() + ":" + 
							Util.TrimDecimals(tc, dec) + ":" + 
							Util.TrimDecimals(tv, dec) + ":" +
							Util.TrimDecimals(Util.GetHeightInCentimeters(tv.ToString()), dec) + ":" +
							Util.TrimDecimals(Jump.CalculateRSI(tv, tc), dec) + ":" +
							Util.TrimDecimals(getPower(tc, tv, personWeight, extraWeightInKg, fall), dec) + ":" +
							Util.TrimDecimals(Util.GetStiffness(personWeight, extraWeightInKg, tv, tc), dec)
						  );
					count ++;
				}
				writeData(myData);
				writeData("VERTICAL-SPACE");
			}
		}

		//if not showSubjumps write data at last for not having every row as TH
		if(! showSubjumps) {
			writeData(myData);
		}
	}
	
	protected void printRuns(string title)
	{
		int dec=preferences.digitsNumber; //decimals
		
		if(myRuns.Length > 0) {
			printTitles(title); 

			ArrayList myData = new ArrayList(1);
			myData.Add( "\n" + 
					Catalog.GetString("Person ID") + ":" +
					Catalog.GetString("Person name") + ":" +
					Catalog.GetString("Race ID") + ":" +
					Catalog.GetString("Type") + ":" + 
					Catalog.GetString("Distance") + ":" + 
					Catalog.GetString("Time") + ":" + 
					Catalog.GetString("Speed") + ":" + 
					Catalog.GetString("Datetime") + ":" +
					Catalog.GetString("Description") + ":" +
					Catalog.GetString("Simulated") + ":" +
					Catalog.GetString("Initial Speed") );

			foreach (string runString in myRuns) {
				string [] myStr = runString.Split(new char[] {':'});
				string speed = "";
				if(myStr[4] != "Margaria")
					speed = Util.TrimDecimals(Util.GetSpeed(myStr[5], myStr[6], true), dec);//speed in m/s (true)

				myData.Add (
						myStr[2] + ":" +    			//personID
						myStr[0] + ":" +  myStr[1] + ":" +  	//person.name, run.uniqueID
						myStr[4] + ":" +  myStr[5] + ":" + 	//run.type, run.distance
						Util.TrimDecimals(myStr[6], dec) + ":" +  	//run.time
						speed + ":" + 				//speed in m/s (true)
						myStr[10] + ":" +			//datetime
						Util.RemoveNewLine(myStr[7], true) + ":" + 	//description
						Util.SimulatedTestNoYes(Convert.ToInt32(myStr[8])) + ":" + //simulated
						Util.NoYes(Util.StringToBool(myStr[9]))	//initialSpeed
					   );
			}
			writeData(myData);
			writeData("VERTICAL-SPACE");
		}
	}
	
	protected void printRunsInterval(bool showSubruns, string title)
	{
		int dec=preferences.digitsNumber; //decimals

		ArrayList myData = new ArrayList(1);
		bool isFirstHeader = true;
		
		if(myRunsInterval.Length > 0)
			printTitles(title); 

		Sqlite.Open();	
		foreach (string runString in myRunsInterval) 
		{
			if(showSubruns) {
				myData = new ArrayList(1);
			}

			//if show subruns show this every time, else show only one
			if(isFirstHeader || showSubruns) {
				myData.Add( "\n" + 
						Catalog.GetString("Person ID") + ":" +
						Catalog.GetString("Person name") + ":" +
						Catalog.GetString("run ID") + ":" + 
						Catalog.GetString("Type") + ":" + 
						Catalog.GetString("Distance total") + ":" + 
						Catalog.GetString("Time total") + ":" +
						Catalog.GetString("Average speed") + ":" +
						Catalog.GetString("Distance interval") + ":" + 
						Catalog.GetString("Laps") + ":" +
						Catalog.GetString("Limited") + ":" +
						Catalog.GetString("Datetime") + ":" +
						Catalog.GetString("Description") + ":" +
						Catalog.GetString("Simulated") + ":" +
						Catalog.GetString("Initial Speed") );
				isFirstHeader = false;
			}

			string [] myStr = runString.Split(new char[] {':'});

			RunType myRunType = new RunType();
			string myRunTypeString = myStr[4];
			string myRunDistanceInterval = myStr[7];
			if(myRunDistanceInterval == "-1" || myRunDistanceInterval == "-1.0") {
				myRunType = SqliteRunIntervalType.SelectAndReturnRunIntervalType(
						myRunTypeString, true);
			}
			myData.Add (
					myStr[2] + ":" +    			//personID
					myStr[0] + ":" +  myStr[1] + ":" +  	//person.name, run.uniqueID
					myRunTypeString + ":" +  Util.TrimDecimals(myStr[5], dec) + ":" + 	//run.type, run.distancetotal
					Util.TrimDecimals(myStr[6], dec) + ":" +  		//run.timetotal
					Util.TrimDecimals(Util.GetSpeed(myStr[5], myStr[6], true), dec) + ":" + 	//speed AVG in m/s(true)
					myStr[7] + ":" + 	 	//run.distanceInterval
					myStr[9] + ":" +  Util.GetLimitedRounded(myStr[11], dec) + ":" + 	//tracks, limited
					myStr[14] + ":" +				//datetime
					Util.RemoveNewLine(myStr[10], true) + ":" + 	//description
					Util.SimulatedTestNoYes(Convert.ToInt32(myStr[12])) + ":" +	//simulated
					Util.NoYes(Util.StringToBool(myStr[13]))	//initialSpeed
				   );
			
			if(showSubruns) {
				writeData(myData);

				myData = new ArrayList(1);
				//print intervalTimesString
				string [] timeString = myStr[8].Split(new char[] {'='});
				myData.Add( " " + ":" + 
						Catalog.GetString ("Interval speed") + ":" + 
						Catalog.GetString("interval times") );
				
				//print Total, AVG, SD
				myData.Add(Catalog.GetString("Total") + ":" +
						" " + ":" +
						Util.TrimDecimals(Util.GetTotalTime(myStr[8]).ToString(), dec));
				myData.Add(Catalog.GetString("AVG") + ":" +
						Util.TrimDecimals(Util.GetSpeed(
								myStr[5], myStr[6], true), dec) + ":" +
						Util.TrimDecimals(Util.GetAverage(myStr[8]).ToString(), dec));
				myData.Add(Catalog.GetString("SD") + ":" + 
						" " + ":" +
						Util.TrimDecimals(Util.CalculateSD(
								Util.ChangeEqualForColon(myStr[8]),
								Util.GetTotalTime(myStr[8]),
								Util.GetNumberOfJumps(myStr[8], false)).ToString(),
							dec));
				
				int count = 1;
				foreach(string myTime in timeString) {
					string myDistance = myStr[7];
					string myShowDistance = "";
					if(myRunDistanceInterval == "-1" || myRunDistanceInterval == "-1.0") {
						myDistance = Util.GetRunIVariableDistancesStringRow(
								myRunType.DistancesString, count -1).ToString();
						myShowDistance = " (" + myDistance.ToString() + "m)";
					}

					myData.Add((count++).ToString() + myShowDistance + ":" + 
							Util.TrimDecimals(Util.GetSpeed(myDistance, myTime, true), dec) + ":" + //true for: m/s
							Util.TrimDecimals(myTime, dec)
						  );
				}
				writeData(myData);
				writeData("VERTICAL-SPACE");
			}
		}
		Sqlite.Close();	

		//if not showSubruns write data at last for not having every row as TH
		if(! showSubruns) {
			writeData(myData);
		}
	}
	
	/*
	protected void printReactionTimes(string title)
	{
		int dec=preferences.digitsNumber; //decimals
		
		if(myReactionTimes.Length > 0) {
			printTitles(title); 

			ArrayList myData = new ArrayList(1);
			myData.Add(  
					Catalog.GetString("Person ID") + ":" +
					Catalog.GetString("Person name") + ":" +
					Catalog.GetString("Reaction time ID") + ":" + 
					Catalog.GetString("Type") + ":" + 
					Catalog.GetString("Time") + ":" + 
					Catalog.GetString("Description") + ":" +
					Catalog.GetString("Simulated") );

			foreach (string rtString in myReactionTimes) {
				string [] myStr = rtString.Split(new char[] {':'});

				myData.Add (	
						myStr[2] + ":" +    			//personID
						myStr[0] + ":" +  myStr[1] + ":" +  	//person.name, event.uniqueID
						//myStr[2] + ":" +  myStr[3] + ":" +  	//jump.personID, jump.sessionID
						myStr[4] + ":" +  //type
						Util.TrimDecimals(myStr[5], dec) + ":" + 	//time
						Util.RemoveNewLine(myStr[6], true) + ":" + 
						Util.SimulatedTestNoYes(Convert.ToInt32(myStr[7]))	//description, simulated
					   );
			}
			writeData(myData);
			writeData("VERTICAL-SPACE");
		}
	}

	//protected void printPulses(bool showSubpulses) 
	//no need of bool because all the info is in the sub values
	protected void printPulses(string title)
	{
		int dec=preferences.digitsNumber; //decimals
		
		ArrayList myData = new ArrayList(1);
		
		if(myPulses.Length > 0) 
			printTitles(title); 
		
		foreach (string pulseString in myPulses) {

			myData = new ArrayList(1);

			myData.Add( "\n" + 
					Catalog.GetString("Person ID") + ":" +
					Catalog.GetString("Person name") + ":" +
					Catalog.GetString("Pulse ID") + ":" + 
					Catalog.GetString("Type") + ":" + 
					//Catalog.GetString("Time") + ":" +
					Catalog.GetString("Description") + ":" +
					Catalog.GetString("Simulated") );

			string [] myStr = pulseString.Split(new char[] {':'});
			myData.Add (
					myStr[2] + ":" +    			//personID
					myStr[0] + ":" +  myStr[1] + ":" +  	//person.name, pulse.uniqueID
					myStr[4] + ":" +  		 	//type
					Util.RemoveNewLine(myStr[8], true) + ":" + 
					Util.SimulatedTestNoYes(Convert.ToInt32(myStr[9]))		//description, simulated
				   );
			
			writeData(myData);

			myData = new ArrayList(1);
			//print intervalTimesString
			string [] timeString = myStr[7].Split(new char[] {'='});
			myData.Add( " " + ":" + 
					Catalog.GetString ("Time") );

			//print Total, AVG, SD
			myData.Add(Catalog.GetString("Total") + ":" +
					Util.TrimDecimals(Util.GetTotalTime(myStr[7]).ToString(), dec));
			myData.Add(Catalog.GetString("AVG") + ":" +
					Util.TrimDecimals(Util.GetAverage(myStr[7]).ToString(), dec));
			myData.Add(Catalog.GetString("SD") + ":" + 
					Util.TrimDecimals(Util.CalculateSD(
							Util.ChangeEqualForColon(myStr[7]),
							Util.GetTotalTime(myStr[7]),
							Util.GetNumberOfJumps(myStr[7], false)).ToString(),
						dec));
				
			int count = 1;
			foreach(string myTime in timeString) {
				myData.Add((count++).ToString() + ":" + 
						Util.TrimDecimals(myTime, dec)
					  );
			}
			writeData(myData);
			writeData("VERTICAL-SPACE");
		}
	}
	
	protected void printMCs(string title)
	{
		int dec=preferences.digitsNumber; //decimals
		
		ArrayList myData = new ArrayList(1);
		
		if(myMCs.Length > 0) 
			printTitles(title); 
		
		foreach (string testString in myMCs) {
			myData = new ArrayList(1);

			myData.Add( "\n" + 
					Catalog.GetString("Person ID") + ":" +
					Catalog.GetString("Person name") + ":" +
					Catalog.GetString("MC ID") + ":" + 
					Catalog.GetString("Type") + ":" + 
					Catalog.GetString("Description") + ":" +
					Catalog.GetString("Simulated") );

			string [] myStr = testString.Split(new char[] {':'});
			MultiChronopic mc = new MultiChronopic();
			mc.UniqueID = Convert.ToInt32(myStr[1].ToString()); 
			mc.PersonID = Convert.ToInt32(myStr[2].ToString()); 
			mc.Type = myStr[4].ToString(); 
			mc.Cp1StartedIn = Convert.ToInt32(myStr[5].ToString()); 
			mc.Cp2StartedIn = Convert.ToInt32(myStr[6].ToString()); 
			mc.Cp3StartedIn = Convert.ToInt32(myStr[7].ToString()); 
			mc.Cp4StartedIn = Convert.ToInt32(myStr[8].ToString()); 
			mc.Cp1InStr = myStr[9].ToString(); 
			mc.Cp1OutStr = myStr[10].ToString(); 
			mc.Cp2InStr = myStr[11].ToString(); 
			mc.Cp2OutStr = myStr[12].ToString(); 
			mc.Cp3InStr = myStr[13].ToString(); 
			mc.Cp3OutStr = myStr[14].ToString(); 
			mc.Cp4InStr = myStr[15].ToString(); 
			mc.Cp4OutStr = myStr[16].ToString(); 
			mc.Vars = myStr[17].ToString(); 
			mc.Description = myStr[18].ToString(); 
			mc.Simulated = Convert.ToInt32(myStr[19].ToString()); 

			string typeExtra = mc.GetCPsString();
			if(mc.Type == Constants.RunAnalysisName)
				typeExtra = mc.Vars + " cm.";

			myData.Add (
					mc.PersonID + ":" +    			
					myStr[0] + ":" +  mc.UniqueID + ":" +  	//person.name, mc.uniqueID
					mc.Type + " " + typeExtra  + ":" +  		 	
					Util.RemoveNewLine(mc.Description, true) + ":" + 
					Util.SimulatedTestNoYes(Convert.ToInt32(mc.Simulated.ToString()))
				   );
			
			writeData(myData);

			myData = new ArrayList(1);
		
			string cols4 = ": : : :";
			myData.Add( mc.DeleteCols(
						" " + ":" + 
						Catalog.GetString ("Time") + ":" +
						Catalog.GetString ("State") + cols4 +
						Catalog.GetString ("Change") + cols4 +
						Catalog.GetString ("IN-IN") + cols4 + 
						Catalog.GetString ("OUT-OUT") + cols4
						, mc.CPs(), false)
				  );

			string titleStr = "CP1:CP2:CP3:CP4:";
			myData.Add( mc.DeleteCols(
						" " + ":" + 
						" " + ":" +
						titleStr + 
						titleStr + 
						titleStr + 
						titleStr
						, mc.CPs(), false)
				  );

			string [] averages = mc.Statistics(true, dec); //first boolean is averageOrSD
			int count = 0;
			myData.Add( mc.DeleteCols(
						Catalog.GetString("AVG") + ": : " + cols4 + " " + cols4 + 
						Util.RemoveZeroOrMinus(Util.TrimDecimals( averages[count++], dec )) + ":" +
						Util.RemoveZeroOrMinus(Util.TrimDecimals( averages[count++], dec )) + ":" +
						Util.RemoveZeroOrMinus(Util.TrimDecimals( averages[count++], dec )) + ":" +
						Util.RemoveZeroOrMinus(Util.TrimDecimals( averages[count++], dec )) + ":" +
						Util.RemoveZeroOrMinus(Util.TrimDecimals( averages[count++], dec )) + ":" +
						Util.RemoveZeroOrMinus(Util.TrimDecimals( averages[count++], dec )) + ":" +
						Util.RemoveZeroOrMinus(Util.TrimDecimals( averages[count++], dec )) + ":" +
						Util.RemoveZeroOrMinus(Util.TrimDecimals( averages[count++], dec ))
						, mc.CPs(), false)
				  );

			string [] sds = mc.Statistics(false, dec); //first boolean is averageOrSD
			count = 0;
			myData.Add( mc.DeleteCols(
						Catalog.GetString("SD") + ": : " + cols4 + " " + cols4 + 
						Util.RemoveZeroOrMinus(Util.TrimDecimals( sds[count++], dec )) + ":" +
						Util.RemoveZeroOrMinus(Util.TrimDecimals( sds[count++], dec )) + ":" +
						Util.RemoveZeroOrMinus(Util.TrimDecimals( sds[count++], dec )) + ":" +
						Util.RemoveZeroOrMinus(Util.TrimDecimals( sds[count++], dec )) + ":" +
						Util.RemoveZeroOrMinus(Util.TrimDecimals( sds[count++], dec )) + ":" +
						Util.RemoveZeroOrMinus(Util.TrimDecimals( sds[count++], dec )) + ":" +
						Util.RemoveZeroOrMinus(Util.TrimDecimals( sds[count++], dec )) + ":" +
						Util.RemoveZeroOrMinus(Util.TrimDecimals( sds[count++], dec ))
						, mc.CPs(), false)
				  
					);

			ArrayList array = mc.AsArrayList(dec);
			foreach(string row in array) 
				myData.Add(mc.DeleteCols(row, mc.CPs(), true));
			
			
			writeData(myData);
			writeData("VERTICAL-SPACE");
		}
	}
	*/

	public Gtk.Button FakeButtonDone {
		get { return fakeButtonDone; }
	}

	public string Filename {
		get { return filename; }
	}

	protected virtual void printFooter()
	{
	}
	
	protected void closeWriter ()
	{
		writer.Close();
		((IDisposable)writer).Dispose();
	}

	~ExportSession() {}
}


public class ExportSessionCSV : ExportSession 
{

	//TODO: falta tot lo de individual current session, individual all sessions, groupal current session

	public ExportSessionCSV ()
	{
	}

	public ExportSessionCSV (Session mySession, Gtk.Window app1, Preferences preferences,
			string modeForFilename, int personID, string personName, int sessionID,
			bool jumpsSimple, bool jumpsSimpleMeanMaxTables, bool jumpsReactive,
			bool runsSimple, bool runsIntervallic)
	{
		this.mySession = mySession;
		this.preferences = preferences;
		this.modeForFilename = modeForFilename;
		this.personID = personID;
		this.personName = personName;
		this.sessionID = sessionID;
		this.jumpsSimple = jumpsSimple;
		this.jumpsSimpleMeanMaxTables = jumpsSimpleMeanMaxTables;
		this.jumpsReactive = jumpsReactive;
		this.runsSimple = runsSimple;
		this.runsIntervallic = runsIntervallic;

		//spreadsheetString = Constants.GetSpreadsheetString(preferences.CSVExportDecimalSeparator);
		showDialogMessage = false;
		fakeButtonDone = new Gtk.Button ();
		filename = "";
	}

	public void Do ()
	{
		checkFile ("CSV");
	}

	protected override void writeData (ArrayList exportData) {
		for(int i=0; i < exportData.Count ; i++) 
		{
			//correctly separate the rows with no problems with decimals
			//delete the ';'
			exportData[i] = exportData[i].ToString().Replace(";", " ");

			bool latin = true;
			if(preferences.CSVExportDecimalSeparator != "COMMA")
				latin = false;

			if(latin) {
				//put ';' as separator
				exportData[i] = exportData[i].ToString().Replace(":", ";");
			} else {
				//decimal as "."
				exportData[i] = exportData[i].ToString().Replace(",", ".");
				//put ',' as separator
				exportData[i] = exportData[i].ToString().Replace(":", ",");
			}

			writer.WriteLine( exportData[i] );
		}
	}
	
	protected override void writeData (string exportData) {
		//do nothing
		//if(exportData == "VERTICAL-SPACE") {
		//	writer.WriteLine( "--------------------" );
		//}
	}

	protected override void printFooter()
	{
		LogB.Information( "Correctly exported" );
		/*
		string myString = Catalog.GetString ("Exported to file: ") + filename;
		new DialogMessage(Constants.MessageTypes.INFO, myString);
		*/
	}
	
	~ExportSessionCSV() {}
}

public class ExportSessionXML : ExportSession 
{
	public ExportSessionXML(Session mySession, Gtk.Window app1, Preferences preferences) 
	{
		this.mySession = mySession;
		this.preferences = preferences;
		
		//this.app1 = app1;
		
		//xr = new XmlTextWriter(fileExport, null);
		//xr.Formatting = Formatting.Indented;
		//xr.Indentation = 4;

		showDialogMessage = false;
		checkFile("XML");
	}
	

	//public void printData(string [] myJumps)
	//{
	//	Log.WriteLine("print data export XML");
		/*

		xr.WriteStartDocument();
		xr.WriteComment("Exported File:");

		//for (int i=0; i < persons.NextIndex; i++)
		foreach (string jump in myJumps) {
		{
			xr.WriteStartElement(jump[i]);

			//put all this in session.cs 
			//and person.cs and jump.cs
			//do it as myPerson.ExportXML()

			xr.WriteElementString("Name", persons[i].Name);
			xr.WriteElementString("dateBorn", persons[i].DateBorn);
			//xr.WriteElementString("level", persons[i].Level);
			xr.WriteElementString("description", persons[i].Description);

			xr.WriteEndElement();
		}

		Log.WriteLine("Saved as: {0}", FilePersons);
		*/
	//}

	protected override void printFooter()
	{
		//xr.Flush();
		//xr.Close();
	}
	
	~ExportSessionXML() {}
}
