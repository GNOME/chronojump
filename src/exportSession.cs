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
 * Xavier de Blas: 
 * http://www.xdeblas.com, http://www.deporteyciencia.com (parleblas)
 */

using System;
using System.Data;
using System.IO; 	//TextWriter
using System.Xml;	//XmlTextWriter
using Gtk;		//FileSelection widget
using System.Collections; //ArrayList
using Mono.Unix;

public class ExportSession
{
	protected string [] myPersons;
	protected string [] myJumps;
	protected string [] myJumpsRj;
	protected string [] myRuns;
	protected string [] myRunsInterval;
	protected string [] myReactionTimes;
	protected string [] myPulses;
	protected Session mySession;
	protected TextWriter writer;
	protected static Gtk.Window app1;
	//protected static Gnome.AppBar myAppbar;
	protected static Gtk.Statusbar myAppbar;
	protected string fileName;
	
	protected int prefsDigitsNumber;
	protected bool heightPreferred;
	protected bool weightStatsPercent;

	public ExportSession() {
	}

	//public ExportSession(Session mySession, Gtk.Window app1, Gnome.AppBar mainAppbar) 
	public ExportSession(Session mySession, Gtk.Window app1, Gtk.Statusbar mainAppbar) 
	{
		this.mySession = mySession;
		myAppbar = mainAppbar;
		
		checkFile("none");
	}

	protected void checkFile (string formatFile)
	{
		string exportString = "";
		if(formatFile == "report") {
			exportString = Catalog.GetString ("Save report as...");
		} else {
			exportString = Catalog.GetString ("Export session in format " + formatFile);
		}

			
		FileSelection fs = new FileSelection (exportString);
		fs.SelectMultiple = false;

		//from: http://www.gnomebangalore.org/?q=node/view/467
		if ( (Gtk.ResponseType) fs.Run () != Gtk.ResponseType.Ok) {
			Console.WriteLine("cancelled");
			//report does not currently send the appBar reference
			if(formatFile != "report") {
				myAppbar.Push ( 1, Catalog.GetString ("Cancelled") );
			}
			fs.Hide ();
			return ;
		}

		fileName = fs.Filename;
		fs.Hide ();

		if(formatFile == "report") {
			//add ".html" if needed, remember that on windows should be .htm
			fileName = addHtmlIfNeeded(fileName);
		} else {
			//add ".csv" if needed
			fileName = addCsvIfNeeded(fileName);
		}

		try {
			if (File.Exists(fileName)) {
				Console.WriteLine("File {0} exists with attributes {1}, created at {2}", 
						fileName, File.GetAttributes(fileName), File.GetCreationTime(fileName));
				Console.WriteLine("Overwrite...");
				ConfirmWindow confirmWin = ConfirmWindow.Show(Catalog.GetString("Are you sure you want to overwrite file: "), fileName);
				confirmWin.Button_accept.Clicked += new EventHandler(on_overwrite_file_accepted);
			} else {
				writer = File.CreateText(fileName);
				getData();
				printData();
				closeWriter();
			
				string myString = string.Format(Catalog.GetString("Saved to {0}"), fileName);
				new DialogMessage(myString, false); //false: is info
			}
		} 
		catch {
			string myString = string.Format(Catalog.GetString("Cannot export to file {0} "), fileName);
			new DialogMessage(myString, true); //true: is warning
		}
		return;
	}

	private void on_overwrite_file_accepted(object o, EventArgs args)
	{
		writer = File.CreateText(fileName);
		getData();
		printData();
		closeWriter();
				
		string myString = string.Format(Catalog.GetString("Saved to {0}"), fileName);
		new DialogMessage(myString, false);
	}
		
	private string addHtmlIfNeeded(string myFile)
	{
		int posOfDot = myFile.LastIndexOf('.');
		if (posOfDot == -1) {
			if(Util.IsWindows())
				myFile += ".htm";
			else
				myFile += ".html";
		}
		return myFile;
	}
	
	private string addCsvIfNeeded(string myFile)
	{
		int posOfDot = myFile.LastIndexOf('.');
		if (posOfDot == -1) 
			myFile += ".csv";
		
		return myFile;
	}
	
	protected virtual void getData() 
	{
		myPersons = SqlitePersonSession.SelectCurrentSession(mySession.UniqueID, false, false); //not onlyIDAndName, not reversed
		myJumps= SqliteJump.SelectNormalJumps(mySession.UniqueID, -1, "");
		myJumpsRj = SqliteJump.SelectRjJumps(mySession.UniqueID, -1, "");
		myRuns= SqliteRun.SelectAllNormalRuns(mySession.UniqueID);
		myRunsInterval = SqliteRun.SelectAllIntervalRuns(mySession.UniqueID);
		myReactionTimes = SqliteReactionTime.SelectAllReactionTimes(mySession.UniqueID);
		myPulses = SqlitePulse.SelectAllPulses(mySession.UniqueID);
	}

	protected virtual void printTitles(string title) {
		writer.WriteLine("");
		writer.WriteLine("**** " + title + " ****");
	}

	protected virtual void printData ()
	{
		printTitles(Catalog.GetString("Session"));
		printSessionInfo();
		
		printTitles(Catalog.GetString("Persons"));
		printJumpers();
		
		printTitles(Catalog.GetString("Simple jumps"));
		printJumps();
		
		printTitles(Catalog.GetString("Reactive jumps") + 
				" (" + Catalog.GetString("with subjumps") + ")");
		printJumpsRj(true);

		printTitles(Catalog.GetString("Simple runs"));
		printRuns();

		printTitles(Catalog.GetString("interval runs") + 
				" (" + Catalog.GetString("with tracks") + ")");
		printRunsInterval(true);

		printTitles(Catalog.GetString("Reaction times"));
		printReactionTimes();
		
		printTitles(Catalog.GetString("Pulses"));
		printPulses();

		printFooter();
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
					mySession.Place + ":" + mySession.DateShort + ":" + mySession.Comments );
		writeData(myData);
		writeData("VERTICAL-SPACE");
	}

	protected virtual void printJumpers()
	{
		ArrayList myData = new ArrayList(1);
		myData.Add ( "\n" + Catalog.GetString ("ID") + ":" + Catalog.GetString ("Name") + ":" +
				Catalog.GetString ("Sex") + ":" + Catalog.GetString ("Date of Birth") + ":" +
				Catalog.GetString ("Height") + ":" + Catalog.GetString("Weight") + ":" +
				Catalog.GetString ("Description"));
		foreach (string jumperString in myPersons) {
			string [] myStr = jumperString.Split(new char[] {':'});
			
			myData.Add(
					myStr[0] + ":" + myStr[1] + ":" + 	//person.id, person.name 
					myStr[2] + ":" + myStr[3] + ":" + //sex, dateborn
					myStr[4] + ":" + myStr[5] + ":" + //height, weight
					myStr[6]  //desc
				  );
		}
		writeData(myData);
		writeData("VERTICAL-SPACE");
	}

	protected void printJumps()
	{
		int dec=prefsDigitsNumber; //decimals
		
		string weightName = Catalog.GetString("Weight");
		if(weightStatsPercent)
			weightName += " %";
		else
			weightName += " Kg";

		if(myJumps.Length > 0) {
			ArrayList myData = new ArrayList(1);
			myData.Add( "\n" + 
					Catalog.GetString("Jumper name") + ":" +
					Catalog.GetString("jump ID") + ":" + 
					Catalog.GetString("Type") + ":" + 
					Catalog.GetString("TC") + ":" + 
					Catalog.GetString("TF") + ":" + 
					Catalog.GetString("Fall") + ":" + 
					weightName + ":" + 
					Catalog.GetString("Height") + ":" +
					Catalog.GetString("Initial Speed") + ":" +
					Catalog.GetString("Description") );

			foreach (string jumpString in myJumps) {
				string [] myStr = jumpString.Split(new char[] {':'});
						
				string myWeight = "";
				if(weightStatsPercent)
					myWeight = myStr[8];
				else
					myWeight = Util.WeightFromPercentToKg(
							Convert.ToDouble(myStr[8]), 
							SqlitePersonSession.SelectPersonWeight(
								Convert.ToInt32(myStr[2]),
								Convert.ToInt32(myStr[3])
							)).ToString();

				myData.Add (	
						myStr[0] + ":" +  myStr[1] + ":" +  	//person.name, jump.uniqueID
						//myStr[2] + ":" +  myStr[3] + ":" +  	//jump.personID, jump.sessionID
						myStr[4] + ":" +  Util.TrimDecimals(myStr[6], dec) + ":" + 	//jump.type, jump.tc
						Util.TrimDecimals(myStr[5], dec) + ":" +  myStr[7] + ":" + 	//jump.tv, jump.fall
						//myStr[8] + ":" + 		//jump.weight,
						Util.TrimDecimals(myWeight, dec) + ":" +
						Util.TrimDecimals(Util.GetHeightInCentimeters(myStr[5]), dec) + ":" +  
						Util.TrimDecimals(Util.GetInitialSpeed(myStr[5], true), dec) + ":" +  //true: m/s
						myStr[9]		//jump.description
					   );
			}
			writeData(myData);
			writeData("VERTICAL-SPACE");
		}
	}

	protected void printJumpsRj(bool showSubjumps)
	{
		int dec=prefsDigitsNumber; //decimals

		ArrayList myData = new ArrayList(1);
		bool isFirstHeader = true;
		
		foreach (string jump in myJumpsRj) {
			
			if(showSubjumps) {
				myData = new ArrayList(1);
			}

			string weightName = Catalog.GetString("Weight");
			if(weightStatsPercent)
				weightName += " %";
			else
				weightName += " Kg";

			//if show subjumps show this every time, else show only one
			if(isFirstHeader || showSubjumps) {
				myData.Add( "\n" + 
						Catalog.GetString("Jumper name") + ":" + 
						Catalog.GetString("jump ID") + ":" + 
						Catalog.GetString("jump Type") + ":" + 
						Catalog.GetString("TC Max") + ":" + 
						Catalog.GetString("TF Max") + ":" + 
						Catalog.GetString("Max Height") + ":" +
						Catalog.GetString("Max Initial Speed") + ":" +
						Catalog.GetString("TC AVG") + ":" + 
						Catalog.GetString("TF AVG") + ":" + 
						Catalog.GetString("AVG Height") + ":" +
						Catalog.GetString("AVG Initial Speed") + ":" +
						Catalog.GetString("Fall") + ":" + 
						weightName + ":" + 
						Catalog.GetString("Jumps") + ":" + 
						Catalog.GetString("Time") + ":" + 
						Catalog.GetString("Limited") + ":" + 
						Catalog.GetString("Description" )
					  );
				isFirstHeader = false;
			}
		
			string [] myStr = jump.Split(new char[] {':'});
			
			string myWeight = "";
			if(weightStatsPercent)
				myWeight = myStr[8];
			else
				myWeight = Util.WeightFromPercentToKg(
						Convert.ToDouble(myStr[8]), 
						SqlitePersonSession.SelectPersonWeight(
							Convert.ToInt32(myStr[2]),
							Convert.ToInt32(myStr[3])
							)
						).ToString();
			myData.Add ( 
					myStr[0] + ":" +  myStr[1] + ":" +  	//person.name, jumpRj.uniqueID
					//myStr[2] + ":" +  myStr[3] + ":" +  	//jumpRj.personID, jumpRj.sessionID
					myStr[4] + ":" +  		//jumpRj.type 
					Util.TrimDecimals(myStr[6], dec) + ":" +  		//jumpRj.tcMax 
					Util.TrimDecimals(myStr[5], dec) + ":" + 		//jumpRj.tvMax
					Util.TrimDecimals(Util.GetHeightInCentimeters(myStr[5]), dec) + ":" +  	//Max height
					Util.TrimDecimals(Util.GetInitialSpeed(myStr[5], true), dec) + ":" +  	//Max initial speed (true:m/s)
					Util.TrimDecimals(myStr[11], dec) + ":" +  		//jumpRj.tcAvg
					Util.TrimDecimals(myStr[10], dec) + ":" + 		//jumpRj.tvAvg
					Util.TrimDecimals(Util.GetHeightInCentimeters(myStr[10]), dec) + ":" +  //Avg height
					Util.TrimDecimals(Util.GetInitialSpeed(myStr[10], true), dec) + ":" +  	//Avg Initial speed (true:m/s)
					myStr[7] + ":" + 	 	//jumpRj.Fall
					//myStr[8] + ":" +  myStr[14] + ":" + 	//jumpRj.Weight, jumpRj.Jumps
					Util.TrimDecimals(myWeight,dec) + ":" +  myStr[14] + ":" + 	//jumpRj.Weight, jumpRj.Jumps
					Util.TrimDecimals(myStr[15], dec) + ":" +  Util.GetLimitedRounded(myStr[16],dec) + ":" + 	//jumpRj.Time, jumpRj.Limited
					myStr[9]		//jumpRj.Description
					);
			
			if(showSubjumps) {
				writeData(myData);
			
				myData = new ArrayList(1);
				//print tvString and tcString
				string [] tvString = myStr[12].Split(new char[] {'='});
				string [] tcString = myStr[13].Split(new char[] {'='});
				int count = 0;
				myData.Add( " " + ":" + Catalog.GetString("TC") + 
						":" + Catalog.GetString("TF"));

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
				
				foreach(string myTv in tvString) {
					myData.Add((count+1).ToString() + ":" + 
							Util.TrimDecimals(tcString[count], dec) + ":" + 
							Util.TrimDecimals(myTv, dec));
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
	
	protected void printRuns()
	{
		int dec=prefsDigitsNumber; //decimals
		
		if(myRuns.Length > 0) {
			ArrayList myData = new ArrayList(1);
			myData.Add( "\n" + 
					Catalog.GetString("Runner name") + ":" +
					Catalog.GetString("run ID") + ":" + 
					Catalog.GetString("Type") + ":" + 
					Catalog.GetString("Distance") + ":" + 
					Catalog.GetString("Time") + ":" + 
					Catalog.GetString("Speed") + ":" + 
					Catalog.GetString("Description") );

			foreach (string runString in myRuns) {
				string [] myStr = runString.Split(new char[] {':'});

				myData.Add (
						myStr[0] + ":" +  myStr[1] + ":" +  	//person.name, run.uniqueID
						myStr[4] + ":" +  myStr[5] + ":" + 	//run.type, run.distance
						Util.TrimDecimals(myStr[6], dec) + ":" +  	//run.time
						Util.TrimDecimals(Util.GetSpeed(myStr[5], myStr[6], true), dec) + ":" + //speed in m/s (true)
						myStr[7]		//run.description
					   );
			}
			writeData(myData);
			writeData("VERTICAL-SPACE");
		}
	}
	
	protected void printRunsInterval(bool showSubruns)
	{
		int dec=prefsDigitsNumber; //decimals

		ArrayList myData = new ArrayList(1);
		bool isFirstHeader = true;
		
		foreach (string runString in myRunsInterval) {

			if(showSubruns) {
				myData = new ArrayList(1);
			}

			//if show subruns show this every time, else show only one
			if(isFirstHeader || showSubruns) {
				myData.Add( "\n" + 
						Catalog.GetString("Runner name") + ":" +
						Catalog.GetString("run ID") + ":" + 
						Catalog.GetString("Type") + ":" + 
						Catalog.GetString("Distance total") + ":" + 
						Catalog.GetString("Time total") + ":" +
						Catalog.GetString("Average speed") + ":" +
						Catalog.GetString("Distance interval") + ":" + 
						Catalog.GetString("Tracks") + ":" + 
						Catalog.GetString("Limited") + ":" +
						Catalog.GetString("Description") );
				isFirstHeader = false;
			}

			string [] myStr = runString.Split(new char[] {':'});
			myData.Add (
					myStr[0] + ":" +  myStr[1] + ":" +  	//person.name, run.uniqueID
					myStr[4] + ":" +  Util.TrimDecimals(myStr[5], dec) + ":" + 	//run.type, run.distancetotal
					Util.TrimDecimals(myStr[6], dec) + ":" +  		//run.timetotal
					Util.TrimDecimals(Util.GetSpeed(myStr[5], myStr[6], true), dec) + ":" + 	//speed AVG in m/s(true)
					myStr[7] + ":" + 	 	//run.distanceInterval
					myStr[9] + ":" +  Util.GetLimitedRounded(myStr[11], dec) + ":" + 	//tracks, limited
					myStr[10]		//description
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
					myData.Add((count++).ToString() + ":" + 
							Util.TrimDecimals(Util.GetSpeed(myStr[7], myTime, true), dec) + ":" + //true for: m/s
							Util.TrimDecimals(myTime, dec)
						  );
				}
				writeData(myData);
				writeData("VERTICAL-SPACE");
			}
		}
		//if not showSubruns write data at last for not having every row as TH
		if(! showSubruns) {
			writeData(myData);
		}
	}
	
	protected void printReactionTimes()
	{
		int dec=prefsDigitsNumber; //decimals
		
		if(myReactionTimes.Length > 0) {
			ArrayList myData = new ArrayList(1);
			myData.Add(  
					Catalog.GetString("Person") + ":" +
					Catalog.GetString("Reaction time ID") + ":" + 
					Catalog.GetString("Time") + ":" + 
					Catalog.GetString("Description") );

			foreach (string rtString in myReactionTimes) {
				string [] myStr = rtString.Split(new char[] {':'});

				myData.Add (	
						myStr[0] + ":" +  myStr[1] + ":" +  	//person.name, event.uniqueID
						//myStr[2] + ":" +  myStr[3] + ":" +  	//jump.personID, jump.sessionID
						//myStr[4] + ":" +  //type
						Util.TrimDecimals(myStr[5], dec) + ":" + 	//time
						myStr[6]		//description
					   );
			}
			writeData(myData);
			writeData("VERTICAL-SPACE");
		}
	}

	//protected void printPulses(bool showSubpulses) 
	//no need of bool because all the info is in the sub values
	protected void printPulses()
	{
		int dec=prefsDigitsNumber; //decimals
		
		ArrayList myData = new ArrayList(1);
		bool isFirstHeader = true;
		
		foreach (string pulseString in myPulses) {

			myData = new ArrayList(1);

			myData.Add( "\n" + 
					Catalog.GetString("Person") + ":" +
					Catalog.GetString("Pulse ID") + ":" + 
					Catalog.GetString("Type") + ":" + 
					//Catalog.GetString("Time") + ":" +
					Catalog.GetString("Description") );

			string [] myStr = pulseString.Split(new char[] {':'});
			myData.Add (
					myStr[0] + ":" +  myStr[1] + ":" +  	//person.name, pulse.uniqueID
					myStr[4] + ":" +  		 	//type
					myStr[8]		//description
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
	
	protected virtual void printFooter()
	{
	}
	
	protected void closeWriter ()
	{
		((IDisposable)writer).Dispose();
	}
	
	public int PrefsDigitsNumber {
		set { prefsDigitsNumber = value; }
	}
	
	public bool HeightPreferred {
		set { heightPreferred = value; }
	}
	
	public bool WeightStatsPercent {
		set { weightStatsPercent = value; }
	}

	~ExportSession() {}
}


public class ExportSessionCSV : ExportSession 
{
	
	public ExportSessionCSV(Session mySession, Gtk.Window app1, Gtk.Statusbar mainAppbar) 
	{
		this.mySession = mySession;
		myAppbar = mainAppbar;
		checkFile("CSV");
	}

	protected override void writeData (ArrayList exportData) {
		for(int i=0; i < exportData.Count ; i++) {
			//if the locale of this user shows the decimal point as ',', show it as '.' for not confusing with the comma separator
			//exportData[i] = exportData[i].ToString().Replace(",", ".");

			//correctly separate the rows with no problems with decimals
			//1 delete the ';'
			exportData[i] = exportData[i].ToString().Replace(";", " ");
			//2 put '; ' as separator
			exportData[i] = exportData[i].ToString().Replace(":", "; ");

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
		Console.WriteLine( "Correctly exported" );
		myAppbar.Push ( 1, Catalog.GetString ("Exported to file: ") + fileName );
	}
	
	~ExportSessionCSV() {}
}

public class ExportSessionXML : ExportSession 
{
	private XmlTextWriter xr;
		
	public ExportSessionXML(Session mySession, Gtk.Window app1, Gtk.Statusbar mainAppbar) 
	//public ExportXML(Session mySession, Gtk.Window app1) 
	{
		this.mySession = mySession;
		//this.app1 = app1;
		myAppbar = mainAppbar;
		
		//xr = new XmlTextWriter(fileExport, null);
		//xr.Formatting = Formatting.Indented;
		//xr.Indentation = 4;
		
		checkFile("XML");
	}
	

	//public void printData(string [] myJumps)
	//{
	//	Console.WriteLine("print data export XML");
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

		Console.WriteLine("Saved as: {0}", FilePersons);
		*/
	//}

	protected override void printFooter()
	{
		//xr.Flush();
		//xr.Close();
	}
	
	~ExportSessionXML() {}
}
