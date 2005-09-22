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
using Gnome;


public class Report : ExportSession
{

	private int sessionID;
	public bool ShowCurrentSessionData;
	public bool ShowCurrentSessionJumpers;
	public bool ShowSimpleJumps;
	public bool ShowReactiveJumps;
	public bool ShowSimpleRuns;
	public bool ShowIntervalRuns;
		
	private int prefsDigitsNumber;
	private bool heightPreferred;
	private bool weightStatsPercent;

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

		StatisticsData = new ArrayList(2);
		
		mySession = SqliteSession.Select(sessionID.ToString());
	}
	
	public Report(int sessionID, bool showCurrentSessionData, bool showCurrentSessionJumpers, 
			bool showSimpleJumps, bool showReactiveJumps, bool showSimpleRuns, bool showIntervalRuns, 
			ArrayList statisticsData) 
	{
		this.sessionID = sessionID;
		this.ShowCurrentSessionData = showCurrentSessionData;
		this.ShowCurrentSessionJumpers = showCurrentSessionJumpers;
		this.ShowSimpleJumps = showSimpleJumps;
		this.ShowReactiveJumps = showReactiveJumps;
		this.ShowSimpleRuns = showSimpleRuns;
		this.ShowIntervalRuns = showIntervalRuns;
		this.StatisticsData = statisticsData;
		
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
			myPersons = SqlitePersonSession.SelectCurrentSession(sessionID);
		}
		if(ShowSimpleJumps) {
			myJumps= SqliteJump.SelectAllNormalJumps(sessionID, "ordered_by_time");
		}
		if(ShowReactiveJumps) {
			myJumpsRj = SqliteJump.SelectAllRjJumps(sessionID, "ordered_by_time");
		}
		if(ShowSimpleRuns) {
			myRuns= SqliteRun.SelectAllNormalRuns(sessionID, "ordered_by_time");
		}
		if (ShowIntervalRuns) {
			myRunsInterval = SqliteRun.SelectAllIntervalRuns(sessionID, "ordered_by_time");
		}
	}
	
	
	protected override void printData ()
	{
		copyCssAndLogo();
		
		printHtmlHeader();

		writer.WriteLine("<table class=\"empty\" cellspacing=2 cellpadding=2><tr valign=\"top\"><td>\n");
		if(ShowCurrentSessionData) {
			writer.WriteLine("<h2>Session</h2>");
			printSessionInfo();
		}
		writer.WriteLine("</td><td>\n");
		if(ShowCurrentSessionJumpers) {
			writer.WriteLine("<h2>Persons</h2>");
			printJumpers();
		}
		writer.WriteLine("</td></tr></table>\n");
		if(ShowSimpleJumps) {
			writer.WriteLine("<h2>Simple jumps</h2>");
			printJumps();
		}
		if(ShowReactiveJumps) {
			writer.WriteLine("<h2>Reactive jumps</h2>");
			printJumpsRj();
		}
		if(ShowSimpleRuns) {
			writer.WriteLine("<h2>Simple runs</h2>");
			printRuns();
		}
		if (ShowIntervalRuns) {
			writer.WriteLine("<h2>Interval runs</h2>");
			printRunsInterval();
		}
		
		printStats();
		
		printFooter();
	}

	void copyCssAndLogo() {
		//copy files, and continue if already exists
		try {
			File.Copy(home + "/report_web_style.css" , Util.GetReportDirectoryName(fileName) + "/report_web_style.css" );
		} catch {}
		try {
			File.Copy(home + "/chronojump_logo.png" , Util.GetReportDirectoryName(fileName) + "/chronojump_logo.png");
		} catch {}
	}
	
	protected void printHtmlHeader()
	{
		writer.WriteLine("<HTML><HEAD><TITLE>Chronojump Report (" + DateTime.Now + ")</TITLE>\n");
		writer.WriteLine("<meta HTTP-EQUIV=\" Content-Type\" CONTENT=\"text/html; charset=UTF-8\">\n");
		writer.WriteLine("<style type=\"text/css\">");
		writer.WriteLine("	@import url(" + Util.GetReportDirectoryName(fileName) + 
				"/report_web_style.css); ");
		writer.WriteLine("</style>");
		writer.WriteLine("</HEAD>\n<BODY BGCOLOR=\"#ffffff\" TEXT=\"#444444\">\n");
		
		writer.WriteLine("<table width=\"100%\" class=\"empty\"><tr><td>\n");
		writer.WriteLine("<img src=\"" +
				Util.GetReportDirectoryName(fileName) + "/chronojump_logo.png\">\n ");
		writer.WriteLine("</td><td width=\"80%\"><h1>Chronojump report</h1></td></tr>\n");
		writer.WriteLine("</table>\n");
			
	}


	protected override void printSessionInfo()
	{
		ArrayList myData = new ArrayList(2);
		myData.Add( "\n" + Catalog.GetString ("Name") + ":" + mySession.Name);
		myData.Add(Catalog.GetString ("SessionID") + ":" + mySession.UniqueID);
		myData.Add(Catalog.GetString ("Place") + ":" + mySession.Place);
		myData.Add(Catalog.GetString ("Date") + ":" + mySession.Date);
		myData.Add(Catalog.GetString ("Comments") + ":" + mySession.Comments);
		/*
		myData.Add ( mySession.UniqueID + ":" + mySession.Name + ":" +
					mySession.Place + ":" + mySession.Date + ":" + mySession.Comments );
		*/
		writeData(myData);
		writeData("VERTICAL-SPACE");
	}

	protected override void printJumpers()
	{
		ArrayList myData = new ArrayList(1);
		myData.Add ( "\n" + 
				Catalog.GetString ("ID") + ":" + Catalog.GetString ("Name") + ":" +
				Catalog.GetString ("ID") + ":" + Catalog.GetString ("Name") + ":" +
				Catalog.GetString ("ID") + ":" + Catalog.GetString ("Name") + ":" +
				Catalog.GetString ("ID") + ":" + Catalog.GetString ("Name") + ":" +
				Catalog.GetString ("ID") + ":" + Catalog.GetString ("Name") + ":" +
				Catalog.GetString ("ID") + ":" + Catalog.GetString ("Name")
				);

		string myLine = "";
		int count = 0;
		foreach (string jumperString in myPersons) {
			if(count > 5) {
				count = 0;
				myData.Add(myLine);
				myLine = "";
			}
			string [] myStr = jumperString.Split(new char[] {':'});
			if(count > 0) {
				myLine += ":";
			}
			myLine += myStr[0] + ":" + myStr[1]; 	//person.id, person.name 
			count ++;
		}
		writeData(myData);
		writeData("VERTICAL-SPACE");
	}


	protected override void writeData (ArrayList exportData) {
		writer.WriteLine( "<table cellpadding=2 cellspacing=2>" );
		string iniCell = "<th>";
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
		writer.WriteLine("<h2>Statitistics</h2>");
		
		//obtain every report stats one by one
		for(int i=0; i < StatisticsData.Count ; i++) {
			string [] strFull = StatisticsData[i].ToString().Split(new char[] {'\n'});
			
			string myHeaderStat = "";

			ArrayList sendSelectedSessions = new ArrayList(1);;

			//separate in sessions
			string [] sessionsStrFull = strFull[3].Split(new char[] {':'});
			for (int j=0; j < sessionsStrFull.Length ; j++) {
				Session tempSession = SqliteSession.Select(sessionsStrFull[j]);
				sendSelectedSessions.Add(tempSession.UniqueID + ":" + tempSession.Name + ":" + tempSession.Date);
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
			bool allFine;
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
					rj_evolution_mark_consecutives,
					false, 			//graph
					toReport,
					writer,
					""
					);

			allFine = myStatType.ChooseStat();
			
			string myEnunciate ="<tr><td colspan=\"2\">" + myStatType.Enunciate + "</td></tr>";

			writer.WriteLine("</td><td>");

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
					rj_evolution_mark_consecutives,
					true, 			//graph
					toReport,
					writer,
					fileName		//fileName for exporting there
					);

			allFine = myStatType.ChooseStat();

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
		writer.WriteLine("\nGenerated on " + DateTime.Now + ", by <a href=\"http://chronojump.software-libre.org\">Chronojump</a> v." + progversion );
		writer.WriteLine("\n</BODY></HTML>");
	}
	
	
	public int SessionID {
		set { 
			sessionID = value;
			mySession = SqliteSession.Select(sessionID.ToString());
		}
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
	
	public string Progversion {
		set { progversion = value; }
	}
	
	~Report() {}
	   
}

