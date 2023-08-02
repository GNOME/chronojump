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
using System.IO;
using System.Collections; //ArrayList
using System.Collections.Generic; //List
using Gtk;
//using Glade;
using Mono.Unix;


public class Report : ExportSession
{
	public bool ShowCurrentSessionData;
	public bool ShowCurrentSessionJumpers;
	public bool ShowSimpleJumps;
	public bool ShowReactiveJumps;
	public bool ShowReactiveJumpsWithSubjumps;
	public bool ShowSimpleRuns;
	public bool ShowIntervalRuns;
	public bool ShowIntervalRunsWithSubruns;
	//public bool ShowReactionTimes;
	//public bool ShowPulses;
	
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
		//ShowReactionTimes = true;
		//ShowPulses = true;

		spreadsheetString = "";
		showDialogMessage = true;
		fakeButtonDone = new Gtk.Button ();

		StatisticsData = new ArrayList(1);
		
		mySession = SqliteSession.Select(sessionID.ToString());
	}


	public void PrepareFile () {
		checkFile("report");
	}

	//when a session changes, remove all stats because in report they refer to previoud session
	public void StatisticsRemove() {
		StatisticsData = new ArrayList(1);
	}

	protected override bool getData()
	{
		//to avoid a crash since jumps_ll on exportSession
		jumps_ll = new List<List<SqliteStruct.IntTypeDoubleDouble>> ();

		//create directory filename_files/
		string directoryName = Util.GetReportDirectoryName(filename);
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
			myPersonsAndPS = SqlitePersonSession.SelectCurrentSessionPersons(sessionID, true);
		}
		
		//Leave SQL opened in all this process
		Sqlite.Open(); // ------------------------------

		bool hasData = false;
		if (ShowSimpleJumps) {
			myJumps = SqliteJump.SelectJumpsSA (true, sessionID, -1, "", "",
					Sqlite.Orders_by.DEFAULT, 0);
			if (myJumps.Length > 0)
				hasData = true;
		}
		if (ShowReactiveJumps) {
			myJumpsRj = SqliteJumpRj.SelectJumpsSA(true, sessionID, -1, "", "");
			if (myJumpsRj.Length > 0)
				hasData = true;
		}
		if (ShowSimpleRuns) {
			myRuns = SqliteRun.SelectRunsSA (true, sessionID, -1, "",
					Sqlite.Orders_by.DEFAULT, -1);
			if (myRuns.Length > 0)
				hasData = true;
		}
		if (ShowIntervalRuns) {
			myRunsInterval = SqliteRunInterval.SelectRunsSA (true, sessionID, -1, "");
			if (myRunsInterval.Length > 0)
				hasData = true;
		}
		/*
		if(ShowReactionTimes) {
			myReactionTimes= SqliteReactionTime.SelectReactionTimes(true, sessionID, -1, "",
					Sqlite.Orders_by.DEFAULT, -1);
		}
		if(ShowPulses) {
			myPulses= SqlitePulse.SelectPulses(true, sessionID, -1);
		}
		*/
		
		Sqlite.Close(); // ------------------------------
		return hasData;
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
			printPersons();
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
			printRuns(Catalog.GetString("Simple races"));
		
		if (ShowIntervalRuns) {
			string myTitle = "";
			if(ShowIntervalRunsWithSubruns) {
				myTitle = Catalog.GetString("interval races") +
						" (" + Catalog.GetString("with laps") + ")";
			} else {
				myTitle = Catalog.GetString("interval races") +
						" (" + Catalog.GetString("without laps") + ")";
			}
			printRunsInterval(ShowIntervalRunsWithSubruns, myTitle);
		}

		/*
		if(ShowReactionTimes) 
			printReactionTimes(Catalog.GetString("Reaction times"));

		if(ShowPulses) 
			printPulses(Catalog.GetString("Pulses"));
		*/

		printStats();
		
		printFooter();
	}

	void copyCssAndLogo() {
		//copy files, and continue if already exists
		try {
			File.Copy(Util.GetCssDir()+ Path.DirectorySeparatorChar + Constants.FileNameCSS, 
					Util.GetReportDirectoryName(filename) + Path.DirectorySeparatorChar + Constants.FileNameCSS );
		} catch {}
		try {

			File.Copy(Util.GetImagesDir() + Path.DirectorySeparatorChar + Constants.FileNameLogo, 
					Util.GetReportDirectoryName(filename) + Path.DirectorySeparatorChar + Constants.FileNameLogo );
		} catch {}
	}
	
	protected void printHtmlHeader()
	{
		writer.WriteLine("<HTML><HEAD><TITLE>Chronojump Report (" + DateTime.Now + ")</TITLE>\n");
		writer.WriteLine("<meta HTTP-EQUIV=\"Content-Type\" CONTENT=\"text/html; charset=UTF-8\">\n");
		writer.WriteLine("<style type=\"text/css\">");
		writer.WriteLine("	@import url(\"" + Util.GetLastPartOfPath(Util.GetReportDirectoryName(filename)) + 
				"/report_web_style.css\"); ");
		writer.WriteLine("</style>");
		writer.WriteLine("</HEAD>\n<BODY BGCOLOR=\"#ffffff\" TEXT=\"#444444\">\n");
		
		writer.WriteLine("<table width=\"100%\" class=\"empty\"><tr><td>\n");
		writer.WriteLine("<img src=\"" +
				Util.GetLastPartOfPath(Util.GetReportDirectoryName(filename)) 
				+ "/" + Constants.FileNameLogo + "\">\n ");
		writer.WriteLine("</td><td width=\"80%\" valign=\"bottom\"><h1>Chronojump report</h1></td></tr>\n");
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
					mySession.Place + ":" + mySession.DateShort + ":" + mySession.Comments );

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

	public ArrayList GetSelectedSessions(string sessionsString) {
		ArrayList sendSelectedSessions = new ArrayList(1);
		string [] sessionsStrFull = sessionsString.Split(new char[] {':'});
		for (int j=0; j < sessionsStrFull.Length ; j++) {
			Session tempSession = SqliteSession.Select(sessionsStrFull[j]);
			sendSelectedSessions.Add(tempSession.UniqueID + ":" + tempSession.Name + ":" + tempSession.DateShort);
		}
		return sendSelectedSessions;
	}
			
	public int GetStatsJumpTypeAndLimit(string str, out int limit) {
		int jumpsType = 0;
		limit = -1;
		string [] strJumpsType = str.ToString().Split(new char[] {'.'});
		if(strJumpsType[0] == Catalog.GetString("All")) 
			jumpsType = 0;
		else if(strJumpsType[0] == Catalog.GetString("Limit")) {
			jumpsType = 1;
			limit = Convert.ToInt32 ( strJumpsType[1] ); 
		} else if(strJumpsType[0] == Catalog.GetString("Jumper's best")) {
			jumpsType = 2;
			limit = Convert.ToInt32 ( strJumpsType[1] ); 
		} else if(strJumpsType[0] == Catalog.GetString("Jumper's average")) 
			jumpsType = 3;
		
		return jumpsType;
	}
			
	public int GetRjEvolutionMarkConsecutives(string strIni, out string strEnd) {
		strEnd = strIni;
		int rj_evolution_mark_consecutives = -1;
		if(strEnd.StartsWith(Catalog.GetString("Evolution."))) {
			string [] strEvo = strEnd.Split(new char[] {'.'});
			strEnd = strEvo[0];
			rj_evolution_mark_consecutives = Convert.ToInt32(strEvo[1]);
		}
		return rj_evolution_mark_consecutives;
	}

	protected void printStats()
	{
		if(StatisticsData.Count > 0)
			writer.WriteLine("<h2>Statistics</h2>");
	
		//obtain every report stats one by one
		for(int statCount=0; statCount < StatisticsData.Count ; statCount++) {
			string [] strFull = StatisticsData[statCount].ToString().Split(new char[] {'\t'});
			
			string myHeaderStat = "";

			//separate in sessions
			ArrayList sendSelectedSessions = GetSelectedSessions(strFull[3]);

			//separate in markedRows
			ArrayList arrayListMarkedRows = Util.StringToArrayList(strFull[6], ':');

			string applyTo = strFull[2];
			myHeaderStat += "<h3> " + strFull[0] + " : " + strFull[1] + " : " + applyTo + "</h3> ";

			bool showSex = Util.StringToBool(strFull[5]);

			int limit;
			int statsJumpsType = GetStatsJumpTypeAndLimit(strFull[4], out limit);

			//obtain marked jumps of rj evolution if needed
			string subType;
			int rj_evolution_mark_consecutives = GetRjEvolutionMarkConsecutives(strFull[1], out subType);

			
			
			myHeaderStat += "\n<p><TABLE cellpadding=2 cellspacing=2><tr><td>\n";
			writer.WriteLine(myHeaderStat);

			StatType myStatType;
			//bool allFine;
			//report of stat

			GraphROptions graphROptions = new GraphROptions(strFull[7]);

			myStatType = new StatType(
					strFull[0], 		//statisticType
					subType, 		//statisticSubType
					strFull[2], 		//statisticApplyTo
					sendSelectedSessions, 
					showSex, 	
					statsJumpsType,
					limit, 	
					arrayListMarkedRows,
					rj_evolution_mark_consecutives,
					graphROptions,
					false, 			//graph
					toReport,
					preferences,
					writer,
					"",
					statCount
					);

			//allFine = myStatType.ChooseStat();
			myStatType.ChooseStat();
			
			string myEnunciate ="<tr><td>" + myStatType.Enunciate + "</td></tr>";

			writer.WriteLine("<br>");

			//report of graph
			myStatType = new StatType(
					strFull[0], 		//statisticType
					subType, 		//statisticSubType
					strFull[2], 		//statisticApplyTo
					sendSelectedSessions, 
					showSex, 	
					statsJumpsType,
					limit, 	
					arrayListMarkedRows,
					rj_evolution_mark_consecutives,
					graphROptions,
					true, 			//graph
					toReport,
					preferences,
					writer,
					filename,		//filename for exporting there
					statCount
					);

			myStatType.ChooseStat();

			//enunciate is prented here and not before 
			//because myStatType of a graph doesn't know the numContinuous value 
			//needed for enunciate in rj evolution statistic
			writer.WriteLine(myEnunciate);
			writer.WriteLine("<tr><td>" + strFull[8] + "</td></tr>"); //comment
			writer.WriteLine("</table>");
		}
	}

	protected override void printFooter()
	{
		writer.WriteLine("\n<div id=\"footer\"><hr align=\"right\" width=\"50%\">");
		writer.WriteLine("\nGenerated on " + DateTime.Now + ", by <a href=\"http://chronojump.org\">Chronojump</a> v." + progversion );
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

