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
	
	Stat myStat; 
	//Report myReport; 

	//if this changes, change also gui/stats.cs
	static string djIndexFormula = Catalog.GetString("Dj Index") + " ((tv-tc)/tc *100)";
	static string qIndexFormula = Catalog.GetString("Q index") + " (tv/tc)";
	static string fvIndexFormula = "F/V sj+(100%)/sj *100";
	static string ieIndexFormula = "IE (cmj-sj)/sj *100";
	static string iubIndexFormula = "IUB (abk-cmj)/cmj *100";
	
	string allJumpsName = Catalog.GetString("All jumps");
	
	
	//comes from gui/stats.cs
	public StatType (string statisticType, string statisticSubType, string statisticApplyTo, Gtk.TreeView treeview_stats,
			ArrayList sendSelectedSessions, int prefsDigitsNumber, bool sex_active, 
			int statsJumpsType, int limit, bool heightPreferred, bool weightStatsPercent, 
			bool graph, bool toReport)
	{
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

		myStat = new Stat(); //create and instance of myStat
	}

	//comes from report.cs
	public StatType (string statisticType, string statisticSubType, string statisticApplyTo,
			ArrayList sendSelectedSessions, int prefsDigitsNumber, bool sex_active, 
			int statsJumpsType, int limit, bool heightPreferred, bool weightStatsPercent, 
			bool graph, bool toReport, TextWriter writer)
	{
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
		this.writer = writer;

		myStat = new Stat(); //create and instance of myStat
	}
	

	//TODO: make all this clearer with CASE selections
	public bool ChooseStat ()
	{
		if ( statisticType == Catalog.GetString("Global") ) {
			int jumperID = -1; //all jumpers
			string jumperName = ""; //all jumpers
			if(graph) {
				myStat = new GraphGlobal(
						sendSelectedSessions, 
						jumperID, jumperName, 
						prefsDigitsNumber, sex_active,  
						statsJumpsType, heightPreferred 
						);
				myStat.PrepareData();
				myStat.CreateGraph();
			} else {
				myStat = new StatGlobal(treeview_stats, 
						sendSelectedSessions, 
						jumperID, jumperName, 
						prefsDigitsNumber, sex_active,  
						statsJumpsType, heightPreferred 
						);
				myStat.PrepareData();
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
				myStat = new GraphGlobal(
						sendSelectedSessions, 
						jumperID, jumperName, 
						prefsDigitsNumber, sex_active,  
						statsJumpsType, heightPreferred 
						);
				myStat.PrepareData();
				myStat.CreateGraph();
			}
			else {
				myStat = new StatGlobal(treeview_stats, 
						sendSelectedSessions, 
						jumperID, jumperName, 
						prefsDigitsNumber, sex_active,  
						statsJumpsType, heightPreferred  
						);
				myStat.PrepareData();
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
						myStat = new GraphIeIub ( 
								sendSelectedSessions, 
								indexType,
								prefsDigitsNumber, sex_active, 
								statsJumpsType,
								limit);
						myStat.PrepareData();
						myStat.CreateGraph();
					} else {
						myStat = new StatIeIub(treeview_stats, 
								sendSelectedSessions,
								indexType, 
								prefsDigitsNumber, sex_active, 
								statsJumpsType,
								limit);
						myStat.PrepareData();
					}
				} else {	//F/V
					if(graph) {
						myStat = new GraphFv ( 
								sendSelectedSessions, 
								indexType,
								prefsDigitsNumber, sex_active, 
								statsJumpsType,
								limit);
						myStat.PrepareData();
						myStat.CreateGraph();
					} else {
						myStat = new StatFv(treeview_stats, 
								sendSelectedSessions,
								indexType, 
								prefsDigitsNumber, sex_active, 
								statsJumpsType,
								limit);
						myStat.PrepareData();
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
						myStat = new GraphSjCmjAbkPlus ( 
								sendSelectedSessions, 
								prefsDigitsNumber, statisticApplyTo, 
								sex_active, 
								statsJumpsType,
								limit,
								weightStatsPercent, 
								heightPreferred 
								);
						myStat.PrepareData();
						myStat.CreateGraph();
					} else {
						myStat = new StatSjCmjAbkPlus (treeview_stats, 
								sendSelectedSessions, 
								prefsDigitsNumber, statisticApplyTo, 
								sex_active, 
								statsJumpsType,
								limit,
								weightStatsPercent, 
								heightPreferred
								);
						myStat.PrepareData();
					}
				} else {
					if(toReport) {
						if(graph) {
							/*
							myStat = new GraphSjCmjAbk ( 
									sendSelectedSessions, 
									prefsDigitsNumber, statisticApplyTo, 
									sex_active, 
									statsJumpsType,
									limit,
									heightPreferred
									);
							myStat.PrepareData();
							myStat.CreateGraph();
							*/
						} else {
							myStat = new ReportSjCmjAbk (
									sendSelectedSessions, 
									prefsDigitsNumber, statisticApplyTo, 
									sex_active, 
									statsJumpsType,
									limit,
									heightPreferred
									);
							myStat.PrepareData();
							writer.WriteLine(myStat.ToString());
						}
					} else {
						if(graph) {
							myStat = new GraphSjCmjAbk ( 
									sendSelectedSessions, 
									prefsDigitsNumber, statisticApplyTo, 
									sex_active, 
									statsJumpsType,
									limit,
									heightPreferred
									);
							myStat.PrepareData();
							myStat.CreateGraph();
						} else {
							myStat = new StatSjCmjAbk (treeview_stats, 
									sendSelectedSessions, 
									prefsDigitsNumber, statisticApplyTo, 
									sex_active, 
									statsJumpsType,
									limit,
									heightPreferred
									);
							myStat.PrepareData();
						}
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
					myStat = new GraphDjIndex ( 
							sendSelectedSessions, 
							prefsDigitsNumber, statisticApplyTo, 
							sex_active, 
							statsJumpsType,
							limit//,
							//heightPreferred
							);
					myStat.PrepareData();
					myStat.CreateGraph();
				} else {
					myStat = new StatDjIndex(treeview_stats, 
							sendSelectedSessions, 
							prefsDigitsNumber, statisticApplyTo, 
							sex_active,
							statsJumpsType,
							limit//, 
							//heightPreferred
							);
					myStat.PrepareData();
				}
			} else if(statisticSubType == qIndexFormula)
			{
				if(graph) {
					myStat = new GraphDjQ ( 
							sendSelectedSessions, 
							prefsDigitsNumber, statisticApplyTo, 
							sex_active, 
							statsJumpsType,
							limit//,
							//heightPreferred
							);
					myStat.PrepareData();
					myStat.CreateGraph();
				} else {
					myStat = new StatDjQ(treeview_stats, 
							sendSelectedSessions, 
							prefsDigitsNumber, statisticApplyTo, 
							sex_active,
							statsJumpsType,
							limit//, 
							//heightPreferred
							);
					myStat.PrepareData();
				}
			}
		}
		else if(statisticType == Catalog.GetString("Reactive")) {
			if(statisticSubType == Catalog.GetString("Average Index"))
			{
				if(graph) {
					myStat = new GraphRjIndex ( 
							sendSelectedSessions, 
							prefsDigitsNumber, 
							statisticApplyTo, 
							sex_active, 
							statsJumpsType,
							limit);
					myStat.PrepareData();
					myStat.CreateGraph();
				} else {
					myStat = new StatRjIndex(treeview_stats, 
							sendSelectedSessions, 
							prefsDigitsNumber, 
							statisticApplyTo, 
							sex_active,
							statsJumpsType,
							limit);
					myStat.PrepareData();
				}
			}	
			else if(statisticSubType == Catalog.GetString("POTENCY (Bosco)"))
			{
				if(graph) {
					myStat = new GraphRjPotencyBosco ( 
							sendSelectedSessions, 
							prefsDigitsNumber, 
							statisticApplyTo, 
							sex_active, 
							statsJumpsType,
							limit);
					myStat.PrepareData();
					myStat.CreateGraph();
				} else {
					myStat = new StatRjPotencyBosco(treeview_stats, 
							sendSelectedSessions, 
							prefsDigitsNumber, 
							statisticApplyTo, 
							sex_active, 
							statsJumpsType,
							limit);
					myStat.PrepareData();
				}
			}
			else if(statisticSubType == Catalog.GetString("Evolution"))
			{
				if(graph) {
					myStat = new GraphRjEvolution ( 
							sendSelectedSessions, 
							prefsDigitsNumber, 
							statisticApplyTo, 
							sex_active, 
							statsJumpsType,
							limit);
					myStat.PrepareData();
					myStat.CreateGraph();
				} else {
					myStat = new StatRjEvolution(treeview_stats, 
							sendSelectedSessions, 
							prefsDigitsNumber, 
							statisticApplyTo, 
							sex_active, 
							statsJumpsType,
							limit);
					myStat.PrepareData();
				}
			}
		}

		return true;
	}
	
	~StatType() {}
}

