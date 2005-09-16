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

	bool toFile = true;

	public ArrayList StatisticsData;
	
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
		
		//createIniStatisticsData();
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
	}

	/*
	private void createIniStatisticsData ()
	{
		StatisticsData.Add("hello" + ":" + "i" + ":" + "like" + ":" + 
				"this" + ":" + "thing" + ":" + "a lot");
		StatisticsData.Add("hello2" + ":" + "i" + ":" + "like2" + ":" + 
				"this" + ":" + "thing" + ":" + "a lot");
		StatisticsData.Add("hello3" + ":" + "i" + ":" + "like" + ":" + 
				"this" + ":" + "thing" + ":" + "a lot3");
	}
	*/

	/*
	//public method for adding stats to the treeview
	public void Add(string type, string subtype, string applyTo, 
			string sessionString, string showJumps, bool showSex) 
	{
		StatisticsData.Add(type + ":" + subtype + ":" + applyTo + ":" + 
				sessionString + ":" + showJumps + ":" + showSex);
	}
	*/

	public void PrepareFile () {
		checkFile("none");
		}

	protected override void getData() 
	{
		//session stuff?


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
		printHeader();

		//session stuff?
		
		if(ShowCurrentSessionJumpers) {
			printJumpers();
		}
		if(ShowSimpleJumps) {
			printJumps();
		}
		if(ShowReactiveJumps) {
			printJumpsRj();
		}
		if(ShowSimpleRuns) {
			printRuns();
		}
		if (ShowIntervalRuns) {
			printRunsInterval();
		}
		printFooter();
	}

	
	protected override void printHeader()
	{
		writer.WriteLine("<HTML><HEAD><TITLE>Chronojump Report (insert date)</TITLE>\n");
		writer.WriteLine("<meta HTTP-EQUIV=\" Content-Type\" CONTENT=\"text/html; charset=ISO-8859-1\">\n");
		writer.WriteLine("</HEAD>\n<BODY BGCOLOR=\"#ffffff\" TEXT=\"#444444\">\n");
	}

	protected override void printJumpers()
	{
		writer.WriteLine("<h2>jumpers</h2>");
	}

	protected override void printJumps()
	{
		writer.WriteLine("<h2>Simple Jumps</h2>");

		//obtain every report stats one by one
		for(int i=0; i < StatisticsData.Count ; i++) {
			string [] strFull = StatisticsData[i].ToString().Split(new char[] {'\n'});
			
			if( strFull[0] == Catalog.GetString("Simple") ) {
				
				ArrayList sendSelectedSessions = new ArrayList(1);;

				//separate in sessions
				string [] sessionsStrFull = strFull[3].Split(new char[] {':'});
				for (int j=0; j < sessionsStrFull.Length ; j++) {
					Session mySession = SqliteSession.Select(sessionsStrFull[j]);
					sendSelectedSessions.Add(mySession.UniqueID + ":" + mySession.Name + ":" + mySession.Date);
				}

				string applyTo = strFull[2];
				//if( applyTo == Catalog.GetString("All Jumps") ) {
				//	applyTo = "-1";
				//}
				
				/*
				ReportSjCmjAbk myReport = new ReportSjCmjAbk(
						sendSelectedSessions,
						4,			//prefsDigitsNumber
						applyTo,		//applyTo (jumpType)
						false, 			//showSex
						3, 			//statsJumpType
						2,			//limit
						false			//heightPreferred
						);
				*/
				
						
				StatType myStatType = new StatType(
						strFull[0], 	//statisticType
						strFull[1], 	//statisticSubType
						strFull[2], 	//statisticApplyTo
						sendSelectedSessions, 
						4,		//prefsDigitsNumber
						false, 		//checkbutton_stats_sex.Active
						3,		//statsJumpsType
						2, 		//limit
						false, 		//heightPreferred
						false, 		//weightStatsPercent
						false, 		//graph
						toFile,
						writer
						);

				bool allFine = myStatType.ChooseStat();
				
				//writer.WriteLine(myReport.TableString);
			}
		}
	}

	protected override void printJumpsRj()
	{
		writer.WriteLine("<h2>jumpsRj</h2>");
	}
	
	protected override void printRuns()
	{
		writer.WriteLine("<h2>runs</h2>");
	}
	
	protected override void printRunsInterval()
	{
		writer.WriteLine("<h2>runsInterval</h2>");
	}
	
	protected override void printFooter()
	{
		writer.WriteLine("\n</BODY></HTML>");
	}
	
	
	public int SessionID {
		set { sessionID = value; }
	}
	
	~Report() {}
	   
}

