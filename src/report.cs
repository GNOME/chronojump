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
using System.IO;
using System.Collections; //ArrayList
using Gtk;
using Glade;
using Mono.Unix;


public class Report : ExportSession
{

	private int sessionID;
	public bool ShowCurrentSessionData;
	public bool ShowCurrentSessionJumpers;
	public bool ShowSimpleJumps;
	public bool ShowReactiveJumps;
	public bool ShowReactiveJumpsWithSubjumps;
	public bool ShowSimpleRuns;
	public bool ShowIntervalRuns;
	public bool ShowIntervalRunsWithSubruns;
	public bool ShowReactionTimes;
	public bool ShowPulses;
	
	bool toReport = true;

	public ArrayList StatisticsData;
	
	public static string home = Environment.GetEnvironmentVariable("HOME")+"/.chronojump";

	private string progversion;
	
	
	
	public Report(int sessionID)
	{
		this.SessionID = sessionID;
		ShowCurrentSessionData = true; 
		ShowCurrentSessionJumpers = true;
		ShowSimpleJumps = true;
		ShowReactiveJumps = true;
		ShowSimpleRuns = true;
		ShowIntervalRuns = true;
		ShowReactionTimes = true;
		ShowPulses = true;
	
		spreadsheetString = "";

		StatisticsData = new ArrayList(2);
		
		mySession = SqliteSession.Select(sessionID.ToString());
	}
	
	public Report(int sessionID, bool showCurrentSessionData, bool showCurrentSessionJumpers, 
			bool showSimpleJumps, bool showReactiveJumps, bool showSimpleRuns, bool showIntervalRuns,
			bool showReactionTimes, bool showPulses, ArrayList statisticsData) 
	{
		this.sessionID = sessionID;
		this.ShowCurrentSessionData = showCurrentSessionData;
		this.ShowCurrentSessionJumpers = showCurrentSessionJumpers;
		this.ShowSimpleJumps = showSimpleJumps;
		this.ShowReactiveJumps = showReactiveJumps;
		this.ShowSimpleRuns = showSimpleRuns;
		this.ShowIntervalRuns = showIntervalRuns;
		this.ShowReactionTimes = showReactionTimes;
		this.ShowPulses = showPulses;
		this.StatisticsData = statisticsData;
		
		spreadsheetString = "";

		mySession = SqliteSession.Select(sessionID.ToString());
	}

	public void PrepareFile () {
		checkFile("report");
	}

	protected override void getData() 
	{
		//create directory fileName_files/
		string directoryName = Util.GetReportDirectoryName(fileName);
		if(!Directory.Exists(directoryName)) {
			Directory.CreateDirectory (directoryName);
		} else {
			//if it exists before, delete all pngs
			string [] pngs = Directory.GetFiles(directoryName, "*.png");
			foreach(string myFile in pngs) {
				File.Delete(myFile);
			}
		}

		if(ShowCurrentSessionJumpers) {
			myPersons = SqlitePersonSession.SelectCurrentSession(sessionID, false, false); //not onlyIDAndName, not reversed
		}
		if(ShowSimpleJumps) {
			myJumps= SqliteJump.SelectJumps(sessionID, -1, "");
		}
		if(ShowReactiveJumps) {
			myJumpsRj = SqliteJumpRj.SelectJumps(sessionID, -1, "");
		}
		if(ShowSimpleRuns) {
			myRuns= SqliteRun.SelectAllRuns(sessionID, -1);
		}
		if (ShowIntervalRuns) {
			myRunsInterval = SqliteRunInterval.SelectAllRuns(sessionID, -1);
		}
		if(ShowReactionTimes) {
			myReactionTimes= SqliteReactionTime.SelectAllReactionTimes(sessionID, -1);
		}
		if(ShowPulses) {
			myPulses= SqlitePulse.SelectAllPulses(sessionID, -1);
		}
	}
	
	protected override void printTitles(string title) {
		writer.WriteLine("<h2>" + title + "</h2>");
	}
	
	protected override void printData ()
	{
		copyCssAndLogo();
		
		printHtmlHeader();

		if(ShowCurrentSessionData) {
			printTitles(Catalog.GetString("Session"));
			printSessionInfo();
		}

		if(ShowCurrentSessionJumpers) {
			printTitles(Catalog.GetString("Persons"));
			printJumpers();
		}

		if(ShowSimpleJumps) 
			printJumps(Catalog.GetString("Simple jumps"));

		if(ShowReactiveJumps) {
			string myTitle = "";
			if(ShowReactiveJumpsWithSubjumps) {
				myTitle = Catalog.GetString("Reactive jumps") + 
						" (" + Catalog.GetString("with subjumps") + ")";
			} else {
				myTitle = Catalog.GetString("Reactive jumps") + 
						" (" + Catalog.GetString("without subjumps") + ")";
			}
			printJumpsRj(ShowReactiveJumpsWithSubjumps, myTitle);
		}
		
		if(ShowSimpleRuns) 
			printRuns(Catalog.GetString("Simple runs"));
		
		if (ShowIntervalRuns) {
			string myTitle = "";
			if(ShowIntervalRunsWithSubruns) {
				myTitle = Catalog.GetString("interval runs") + 
						" (" + Catalog.GetString("with tracks") + ")";
			} else {
				myTitle = Catalog.GetString("interval runs") + 
						" (" + Catalog.GetString("without tracks") + ")";
			}
			printRunsInterval(ShowIntervalRunsWithSubruns, myTitle);
		}
		
		if(ShowReactionTimes) 
			printReactionTimes(Catalog.GetString("Reaction times"));

		if(ShowPulses) 
			printPulses(Catalog.GetString("Pulses"));

		printStats();
		
		printFooter();
	}

	void copyCssAndLogo() {
		//copy files, and continue if already exists
		try {
			File.Copy(Constants.FileNameCSS, 
					Util.GetReportDirectoryName(fileName) + Path.DirectorySeparatorChar + Constants.FileNameCSS );
		} catch {}
		try {

			File.Copy(Constants.FileNameLogo, 
					Util.GetReportDirectoryName(fileName) + Path.DirectorySeparatorChar + Constants.FileNameLogo );
		} catch {}
	}
	
	protected void printHtmlHeader()
	{
		writer.WriteLine("<HTML><HEAD><TITLE>Chronojump Report (" + DateTime.Now + ")</TITLE>\n");
		writer.WriteLine("<meta HTTP-EQUIV=\"Content-Type\" CONTENT=\"text/html; charset=UTF-8\">\n");
		writer.WriteLine("<style type=\"text/css\">");
		writer.WriteLine("	@import url(\"" + Util.GetLastPartOfPath(Util.GetReportDirectoryName(fileName)) + 
				"/report_web_style.css\"); ");
		writer.WriteLine("</style>");
		writer.WriteLine("</HEAD>\n<BODY BGCOLOR=\"#ffffff\" TEXT=\"#444444\">\n");
		
		writer.WriteLine("<table width=\"100%\" class=\"empty\"><tr><td>\n");
		writer.WriteLine("<img src=\"" +
				Util.GetLastPartOfPath(Util.GetReportDirectoryName(fileName)) 
				+ "/" + Constants.FileNameLogo + "\">\n ");
		writer.WriteLine("</td><td width=\"80%\"><h1>Chronojump report</h1></td></tr>\n");
		writer.WriteLine("</table>\n");
			
	}


	protected override void printSessionInfo()
	{
		ArrayList myData = new ArrayList(2);
		
		myData.Add( "\n" + 
				Catalog.GetString ("SessionID") + ":" + Catalog.GetString ("Name") + ":" + 
				Catalog.GetString ("Place") + ":" + Catalog.GetString ("Date") + ":" + 
				Catalog.GetString ("Comments")
			  );
		myData.Add ( mySession.UniqueID + ":" + mySession.Name + ":" +
					mySession.Place + ":" + mySession.Date + ":" + mySession.Comments );

		writeData(myData);
		writeData("VERTICAL-SPACE");
	}

	protected override void printJumpers()
	{
		ArrayList myData = new ArrayList(1);
	
		myData.Add ("\n" + 
				Catalog.GetString ("ID") + ":" + Catalog.GetString ("Name") + ":" +
				Catalog.GetString ("Sex") + ":" + Catalog.GetString ("Date of Birth") + ":" +
				Catalog.GetString ("Height") + ":" + Catalog.GetString("Weight") + ":" +
				Catalog.GetString ("Sport") + ":" + Catalog.GetString("Speciallity") + ":" +
				Catalog.GetString ("Level") + ":" + Catalog.GetString ("Description")
			   );
		
		string myLine = "";
		int count = 0;
	
		foreach (string jumperString in myPersons) {
			string [] myStr = jumperString.Split(new char[] {':'});

			myData.Add(myStr[0] + ":" + myStr[1] + ":" + 	//person.id, person.name 
					myStr[2] + ":" + myStr[3] + ":" + //sex, dateborn
					myStr[4] + ":" + myStr[5] + ":" + //height, weight
					myStr[6] + ":" + myStr[7] + ":" + //sportName, speciallityName
					myStr[8] + ":" + myStr[9] //practiceLevel, desc
				  );

		}
		
		writeData(myData);
		writeData("VERTICAL-SPACE");
	}


	protected override void writeData (ArrayList exportData) {
		writer.WriteLine( "<table cellpadding=2 cellspacing=2>" );
		string iniCell = "<th align=\"left\">";
		string endCell = "</th>";
		for(int i=0; i < exportData.Count ; i++) {
			exportData[i] = exportData[i].ToString().Replace(":", endCell + iniCell);
			writer.WriteLine( "<tr>" + iniCell + exportData[i] + endCell + "</tr>" );

			iniCell = "<td>";
			endCell = "</td>";
		}
		writer.WriteLine( "</table>\n" );
	}

	protected override void writeData (string exportData) 
	{
		if(exportData == "VERTICAL-SPACE") {
			writer.WriteLine( "<br>" );
		}
	}
	
	protected void printStats()
	{
		if(StatisticsData.Count > 0)
			writer.WriteLine("<h2>Statitistics</h2>");
		
		//obtain every report stats one by one
		for(int i=0; i < StatisticsData.Count ; i++) {
			//string [] strFull = StatisticsData[i].ToString().Split(new char[] {'\n'});
			string [] strFull = StatisticsData[i].ToString().Split(new char[] {'\t'});
			
			string myHeaderStat = "";

			//separate in sessions
			ArrayList sendSelectedSessions = new ArrayList(1);
			string [] sessionsStrFull = strFull[3].Split(new char[] {':'});
			for (int j=0; j < sessionsStrFull.Length ; j++) {
				Session tempSession = SqliteSession.Select(sessionsStrFull[j]);
				sendSelectedSessions.Add(tempSession.UniqueID + ":" + tempSession.Name + ":" + tempSession.DateShort);
			}

			//separate in markedRows
			ArrayList arrayListMarkedRows = new ArrayList(1);
			string [] markedStrFull = strFull[6].Split(new char[] {':'});
			for (int j=0; j < markedStrFull.Length ; j++) {
				arrayListMarkedRows.Add(markedStrFull[j]);
			}

			string applyTo = strFull[2];
			myHeaderStat += "<h3> " + strFull[0] + " : " + strFull[1] + " : " + applyTo + "</h3> ";

			bool showSex = false;
			if(strFull[5] == "True") {
				showSex = true;
			}

			int statsJumpsType = 0;
			int limit = -1;
			string [] strJumpsType = strFull[4].ToString().Split(new char[] {'.'});
			if(strJumpsType[0] == Catalog.GetString("All")) {
				statsJumpsType = 0;
			} else if(strJumpsType[0] == Catalog.GetString("Limit")) {
				statsJumpsType = 1;
				limit = Convert.ToInt32 ( strJumpsType[1] ); 
			} else if(strJumpsType[0] == Catalog.GetString("Jumper's best")) {
				statsJumpsType = 2;
				limit = Convert.ToInt32 ( strJumpsType[1] ); 
			} else if(strJumpsType[0] == Catalog.GetString("Jumper's average")) {
				statsJumpsType = 3;
			}

			//obtain marked jumps of rj evolution if needed
			int rj_evolution_mark_consecutives = -1;
			if(strFull[1].StartsWith(Catalog.GetString("Evolution."))) {
				string [] strEvo = strFull[1].Split(new char[] {'.'});
				strFull[1] = strEvo[0];
				rj_evolution_mark_consecutives = Convert.ToInt32(strEvo[1]);
			}
			
			
			myHeaderStat += "\n<p><TABLE cellpadding=2 cellspacing=2><tr><td>\n";
			writer.WriteLine(myHeaderStat);

			StatType myStatType;
			//bool allFine;
			//report of stat


			myStatType = new StatType(
					strFull[0], 		//statisticType
					strFull[1], 		//statisticSubType
					strFull[2], 		//statisticApplyTo
					sendSelectedSessions, 
					prefsDigitsNumber,
					showSex, 	
					statsJumpsType,
					limit, 	
					heightPreferred,
					weightStatsPercent,
					arrayListMarkedRows,
					rj_evolution_mark_consecutives,
					false, 			//graph
					toReport,
					writer,
					""
					);

			//allFine = myStatType.ChooseStat();
			myStatType.ChooseStat();
			
			string myEnunciate ="<tr><td>" + myStatType.Enunciate + "</td></tr>";

			writer.WriteLine("<br>");

			//report of graph
			myStatType = new StatType(
					strFull[0], 		//statisticType
					strFull[1], 		//statisticSubType
					strFull[2], 		//statisticApplyTo
					sendSelectedSessions, 
					prefsDigitsNumber,
					showSex, 	
					statsJumpsType,
					limit, 	
					heightPreferred,
					weightStatsPercent,
					arrayListMarkedRows,
					rj_evolution_mark_consecutives,
					true, 			//graph
					toReport,
					writer,
					fileName		//fileName for exporting there
					);

			//allFine = myStatType.ChooseStat();
			myStatType.ChooseStat();

			//enunciate is prented here and not before 
			//because myStatType of a graph doesn't know the numContinuous value 
			//needed for enunciate in rj evolution statistic
			writer.WriteLine(myEnunciate);
			writer.WriteLine("</table>");
		}
	}

	protected override void printFooter()
	{
		writer.WriteLine("\n<div id=\"footer\"><hr align=\"right\" width=\"50%\">");
		writer.WriteLine("\nGenerated on " + DateTime.Now + ", by <a href=\"http://gnome.org/projects/chronojump\">Chronojump</a> v." + progversion );
		writer.WriteLine("\n</BODY></HTML>");
	}
	
	
	public int SessionID {
		set { 
			sessionID = value;
			mySession = SqliteSession.Select(sessionID.ToString());
		}
	}

	public string Progversion {
		set { progversion = value; }
	}
	
	~Report() {}
	   
}

