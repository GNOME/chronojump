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
		writer.WriteLine("header");
	}

	protected override void printJumpers()
	{
		writer.WriteLine("jumpers");
	}

	protected override void printJumps()
	{
		writer.WriteLine("jumps");
	}

	protected override void printJumpsRj()
	{
		writer.WriteLine("jumpsRj");
	}
	
	protected override void printRuns()
	{
		writer.WriteLine("runs");
	}
	
	protected override void printRunsInterval()
	{
		writer.WriteLine("runsInterval");
	}
	
	protected override void printFooter()
	{
		writer.WriteLine("footer");
	}
	
	
	public int SessionID {
		set { sessionID = value; }
	}
	
	~Report() {}
	   
}

