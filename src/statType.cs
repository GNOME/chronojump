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
using System.Text; //StringBuilder
using System.Collections; //ArrayList
using System.IO; 	//TextWriter

//the onbly purpose of this class is to pass parameters nicer between statType and stat and graphs constructors
public class StatTypeStruct 
{
	public string StatisticApplyTo;
	public ArrayList SendSelectedSessions;
	public int PrefsDigitsNumber;
	public bool Sex_active;
	public int StatsJumpsType;
	public int Limit;
	public bool HeightPreferred;
	public bool WeightStatsPercent; 
	public bool ToReport;
	
	public StatTypeStruct (string statisticApplyTo, 
			ArrayList sendSelectedSessions, int prefsDigitsNumber, bool sex_active, 
			int statsJumpsType, int limit, bool heightPreferred, bool weightStatsPercent, 
			bool toReport)
	{
		this.StatisticApplyTo = statisticApplyTo;
		this.SendSelectedSessions =  sendSelectedSessions;
		this.PrefsDigitsNumber =  prefsDigitsNumber;
		this.Sex_active = sex_active;
		this.StatsJumpsType = statsJumpsType;
		this.Limit = limit;
		this.HeightPreferred = heightPreferred;
		this.WeightStatsPercent = weightStatsPercent;
		this.ToReport = toReport;
	}
}

public class StatType {

	string statisticType;
	string statisticSubType;
	string statisticApplyTo;
	Gtk.TreeView treeview_stats;
	ArrayList sendSelectedSessions;
	int prefsDigitsNumber;
	bool sex_active;
	int statsJumpsType;
	int limit;
	bool heightPreferred;
	bool weightStatsPercent; 
	bool graph;
	bool toReport;
	TextWriter writer;
	string fileName;
	
	Stat myStat; 
	//Report myReport; 

	//if this changes, change also gui/stats.cs
	static string djIndexFormula = Catalog.GetString("Dj Index") + " ((tv-tc)/tc *100)";
	static string qIndexFormula = Catalog.GetString("Q index") + " (tv/tc)";
	static string fvIndexFormula = "F/V sj+(100%)/sj *100";
	static string ieIndexFormula = "IE (cmj-sj)/sj *100";
	static string iubIndexFormula = "IUB (abk-cmj)/cmj *100";
	
	string allJumpsName = Catalog.GetString("All jumps");
	
	StatTypeStruct myStatTypeStruct;
	
	//comes from gui/stats.cs
	public StatType (string statisticType, string statisticSubType, string statisticApplyTo, Gtk.TreeView treeview_stats,
			ArrayList sendSelectedSessions, int prefsDigitsNumber, bool sex_active, 
			int statsJumpsType, int limit, bool heightPreferred, bool weightStatsPercent, 
			bool graph, bool toReport)
	{
		//some of this will disappear when we use myStatTypeStruct in all classes:
		this.statisticType = statisticType;
		this.statisticSubType = statisticSubType;
		this.statisticApplyTo = statisticApplyTo;
		this.treeview_stats = treeview_stats ;
		this.sendSelectedSessions =  sendSelectedSessions;
		this.prefsDigitsNumber =  prefsDigitsNumber;
		this.sex_active = sex_active;
		this.statsJumpsType = statsJumpsType;
		this.limit = limit;
		this.heightPreferred = heightPreferred;
		this.weightStatsPercent = weightStatsPercent;
		this.graph = graph;
		this.toReport = toReport;
	
		myStatTypeStruct = new StatTypeStruct (
				statisticApplyTo,
				sendSelectedSessions, prefsDigitsNumber, sex_active, 
				statsJumpsType, limit, heightPreferred, weightStatsPercent, 
				 toReport);

		myStat = new Stat(); //create and instance of myStat
	}

	//comes from report.cs
	public StatType (string statisticType, string statisticSubType, string statisticApplyTo,
			ArrayList sendSelectedSessions, int prefsDigitsNumber, bool sex_active, 
			int statsJumpsType, int limit, bool heightPreferred, bool weightStatsPercent, 
			bool graph, bool toReport, TextWriter writer, string fileName)
	{
		this.statisticType = statisticType;
		this.statisticSubType = statisticSubType;
		this.statisticApplyTo = statisticApplyTo;
		this.sendSelectedSessions =  sendSelectedSessions;
		this.prefsDigitsNumber =  prefsDigitsNumber;
		this.sex_active = sex_active;
		this.statsJumpsType = statsJumpsType;
		this.limit = limit;
		this.heightPreferred = heightPreferred;
		this.weightStatsPercent = weightStatsPercent;
		this.graph = graph;
		this.toReport = toReport;
		this.writer = writer;
		this.fileName = fileName;
		
		myStatTypeStruct = new StatTypeStruct (
				statisticApplyTo,
				sendSelectedSessions, prefsDigitsNumber, sex_active, 
				statsJumpsType, limit, heightPreferred, weightStatsPercent, 
				 toReport);

		myStat = new Stat(); //create and instance of myStat
	}
	

	//TODO: make all this clearer with CASE selections
	public bool ChooseStat ()
	{
		if ( statisticType == Catalog.GetString("Global") ) {
			int jumperID = -1; //all jumpers
			string jumperName = ""; //all jumpers
			if(graph) {
				myStat = new GraphGlobal(myStatTypeStruct, jumperID, jumperName);
			} else {
				myStat = new StatGlobal(myStatTypeStruct, treeview_stats, jumperID, jumperName);
			}
		}
		else if (statisticType == Catalog.GetString("Jumper"))
		{
			if(statisticApplyTo.Length == 0) {
				Console.WriteLine("Jumper-ret");
				return false;
			}
			int jumperID = Convert.ToInt32(Util.FetchID(statisticApplyTo));
			if(jumperID == -1) {
				return false;
			}
			
			string jumperName = Util.FetchName(statisticApplyTo);
			if(graph) {
				myStat = new GraphGlobal(myStatTypeStruct, jumperID, jumperName);
			}
			else {
				myStat = new StatGlobal(myStatTypeStruct, treeview_stats, 
						jumperID, jumperName);
			}
		}
		else if(statisticType == Catalog.GetString("Simple"))
		{
			if(statisticApplyTo.Length == 0) {
				Console.WriteLine("Simple-ret");
				return false;
			}
			
			if(statisticSubType != Catalog.GetString("No indexes")) 
			{
				string indexType = "";
				if(statisticSubType == ieIndexFormula) {
					indexType = "IE";
				} else if(statisticSubType == iubIndexFormula) {
					indexType = "IUB";
				} else if(statisticSubType == fvIndexFormula) {
					indexType = "F/V";
				}
			
				if(indexType == "IE" || indexType == "IUB") {
					if(graph) {
						myStat = new GraphIeIub (myStatTypeStruct, indexType);
					} else {
						myStat = new StatIeIub(myStatTypeStruct, treeview_stats, indexType); 
					}
				} else {	//F/V
					if(graph) {
						myStat = new GraphFv (myStatTypeStruct, indexType);
					} else {
						myStat = new StatFv(myStatTypeStruct, treeview_stats, indexType); 
					}
				}
			}
			else {
				JumpType myType = new JumpType(statisticApplyTo);

				//manage all weight jumps and the "All jumps" (simple)
				if(myType.HasWeight || 
						statisticApplyTo == allJumpsName) 
				{
					if(graph) {
						myStat = new GraphSjCmjAbkPlus (myStatTypeStruct);
					} else {
						myStat = new StatSjCmjAbkPlus (myStatTypeStruct, treeview_stats);
					}
				} else {
					if(graph) {
						myStat = new GraphSjCmjAbk (myStatTypeStruct);
					} else {
						myStat = new StatSjCmjAbk (myStatTypeStruct, treeview_stats);
					}
				}
			}
		}
		else if(statisticType == Catalog.GetString("With TC"))
		{
			if(statisticApplyTo.Length == 0) {
				Console.WriteLine("WithTC-ret");
				return false;
			}
			
			if(statisticSubType == djIndexFormula)
			{
				if(graph) {
					myStat = new GraphDjIndex (myStatTypeStruct);
							//heightPreferred is not used, check this
				} else {
					myStat = new StatDjIndex(myStatTypeStruct, treeview_stats);
							//heightPreferred is not used, check this
				}
			} else if(statisticSubType == qIndexFormula)
			{
				if(graph) {
					myStat = new GraphDjQ (myStatTypeStruct);
							//heightPreferred is not used, check this
				} else {
					myStat = new StatDjQ(myStatTypeStruct, treeview_stats);
							//heightPreferred is not used, check this
				}
			}
		}
		else if(statisticType == Catalog.GetString("Reactive")) {
			if(statisticSubType == Catalog.GetString("Average Index"))
			{
				if(graph) {
					myStat = new GraphRjIndex (myStatTypeStruct);
				} else {
					myStat = new StatRjIndex(myStatTypeStruct, treeview_stats);
				}
			}	
			else if(statisticSubType == Catalog.GetString("POTENCY (Bosco)"))
			{
				if(graph) {
					myStat = new GraphRjPotencyBosco (myStatTypeStruct);
				} else {
					myStat = new StatRjPotencyBosco(myStatTypeStruct, treeview_stats);
				}
			}
			else if(statisticSubType == Catalog.GetString("Evolution"))
			{
				if(graph) {
					myStat = new GraphRjEvolution (myStatTypeStruct);
				} else {
					myStat = new StatRjEvolution(myStatTypeStruct, treeview_stats);
				}
			}
		}
				
		myStat.PrepareData();

		if(toReport) {
			if(graph) {
				bool notEmpty = myStat.CreateGraph(fileName);
				if(notEmpty) { linkImage(fileName); }
			} else {
				writer.WriteLine(myStat.ToString());
			}
		} else {
			if(graph) {
				myStat.CreateGraph();
			}
		}

		return true;
	}
	
	void linkImage(string fileName) {
		string directoryName = Util.GetReportDirectoryName(fileName);
		
		string [] pngs = Directory.GetFiles(directoryName, "*.png");
		//if found 3 images, sure will be 1.png, 2.png and 3.png, next will be 4.png
		writer.WriteLine("<img src=\"" + directoryName + "/" + pngs.Length.ToString() + ".png\">");
	}
	
	~StatType() {}
}
