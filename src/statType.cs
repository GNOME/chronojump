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
 *  Copyright (C) 2004-2017   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
using System.Data;
using System.Text; //StringBuilder
using System.Collections; //ArrayList
using System.IO; 	//TextWriter
using Mono.Unix;

//the onbly purpose of this class is to pass parameters nicer between statType and stat and graphs constructors
public class StatTypeStruct 
{
	public string StatisticApplyTo;
	public ArrayList SendSelectedSessions;
	public bool Sex_active;
	public int StatsJumpsType;
	public int Limit;
	
	public ArrayList MarkedRows;
	public GraphROptions GRO;
	
	public bool ToReport;
		
	public Preferences preferences;
	
	public StatTypeStruct (string statisticApplyTo, 
			ArrayList sendSelectedSessions, bool sex_active, 
			int statsJumpsType, int limit, 
			ArrayList markedRows, GraphROptions gRO,
			bool toReport, Preferences preferences)
	{
		this.StatisticApplyTo = statisticApplyTo;
		this.SendSelectedSessions =  sendSelectedSessions;
		this.Sex_active = sex_active;
		this.StatsJumpsType = statsJumpsType;
		this.Limit = limit;
		this.MarkedRows = markedRows;
		this.GRO = gRO;
		this.ToReport = toReport;
		this.preferences = preferences;
	}
}

public class StatType {

	string statisticType;
	string statisticSubType;
	string statisticApplyTo;
	Gtk.TreeView treeview_stats;
	int evolution_mark_consecutives;
	
	ArrayList markedRows;

	bool graph;
	bool toReport;
	
	TextWriter writer;
	string fileName;
	int statCount; //optimal to create name of each graph on report: 1: 1.png; 2: 2.png

	//this contains the last store of a non-graph stat, 
	//useful for allow to change treeview_stats after a graph stat is done
	//(the graph stat doesn't generate a store)
	Gtk.TreeStore lastStore; 
	
	Stat myStat; 

	StatTypeStruct myStatTypeStruct;
	
	//used for know when a row is checked in treeview, and change then the combo_selected_rows in gui/stats.cs
	public Gtk.Button fakeButtonRowCheckedUnchecked;

	//used for know when no rows are selected in treeview, 
	//and make the graph, add_to_report buttons not sensitive in gui/stats.cs
	public Gtk.Button fakeButtonRowsSelected;
	public Gtk.Button fakeButtonNoRowsSelected;
	
	//comes from gui/stats.cs (initialization)
	public StatType () {
	}
	
	//comes from gui/stats.cs
	public StatType (string statisticType, string statisticSubType, string statisticApplyTo, Gtk.TreeView treeview_stats,
			ArrayList sendSelectedSessions, bool sex_active, int statsJumpsType, int limit, 
			ArrayList markedRows, int evolution_mark_consecutives, GraphROptions gRO,
			bool graph, bool toReport, Preferences preferences)
	{
		//some of this will disappear when we use myStatTypeStruct in all classes:
		this.statisticType = statisticType;
		this.statisticSubType = statisticSubType;
		this.statisticApplyTo = statisticApplyTo;
		this.treeview_stats = treeview_stats ;

		this.markedRows = markedRows;
		
		this.evolution_mark_consecutives = evolution_mark_consecutives;
		
		this.graph = graph;
		this.toReport = toReport;

		myStatTypeStruct = new StatTypeStruct (
				statisticApplyTo,
				sendSelectedSessions, sex_active, 
				statsJumpsType, limit, 
				markedRows, gRO,
				toReport, preferences);

		myStat = new Stat(); //create an instance of myStat

		fakeButtonRowCheckedUnchecked = new Gtk.Button();
		fakeButtonRowsSelected = new Gtk.Button();
		fakeButtonNoRowsSelected = new Gtk.Button();
	}

	private void on_fake_button_row_checked_clicked (object o, EventArgs args) {
		LogB.Information("fakeButtonRowCheckedUnchecked in statType.cs");
		fakeButtonRowCheckedUnchecked.Click();
	}
	
	private void on_fake_button_rows_selected_clicked (object o, EventArgs args) {
		LogB.Information("fakeButtonRowsSelected in statType.cs");
		fakeButtonRowsSelected.Click();
	}
	
	private void on_fake_button_no_rows_selected_clicked (object o, EventArgs args) {
		LogB.Information("fakeButtonNoRowsSelected in statType.cs");
		//only raise another click if this is not a report. This new click will update things in gui/stats.cs
		//this fakeButtons are not initialized in the coming-from-report-statType-constructor
		if(! toReport)
			fakeButtonNoRowsSelected.Click();
	}
	
	//comes from report.cs
	public StatType (string statisticType, string statisticSubType, string statisticApplyTo,
			ArrayList sendSelectedSessions, bool sex_active, 
			int statsJumpsType, int limit, 
			ArrayList markedRows, int evolution_mark_consecutives, GraphROptions gRO,
			bool graph, bool toReport, Preferences preferences, 
			TextWriter writer, string fileName,
			int statCount 
			)
	{
		this.statisticType = statisticType;
		this.statisticSubType = statisticSubType;
		this.statisticApplyTo = statisticApplyTo;

		this.markedRows = markedRows;
		
		this.evolution_mark_consecutives = evolution_mark_consecutives;
		
		this.graph = graph;
		this.toReport = toReport;

		this.writer = writer;
		this.fileName = fileName;
		this.statCount = statCount;
		
		myStatTypeStruct = new StatTypeStruct (
				statisticApplyTo,
				sendSelectedSessions, sex_active, 
				statsJumpsType, limit, 
				markedRows, gRO,
				toReport, preferences);

		myStat = new Stat(); //create and instance of myStat
	}
	

	public bool ChooseStat ()
	{
		if ( statisticType == Constants.TypeSessionSummaryStr() ) {
			int jumperID = -1; //all jumpers
			string jumperName = ""; //all jumpers
			if(graph) {
				myStat = new GraphGlobal(myStatTypeStruct, jumperID, jumperName);
			} else {
				myStat = new StatGlobal(myStatTypeStruct, treeview_stats, jumperID, jumperName);
			}
		}
		else if (statisticType == Constants.TypeJumperSummaryStr())
		{
			if(statisticApplyTo.Length == 0) {
				LogB.Information("Jumper-ret");
				return false;
			}
			int jumperID = Util.FetchID(statisticApplyTo);
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
		else if(statisticType == Constants.TypeJumpsSimpleStr())
		{
			if(statisticApplyTo.Length == 0) {
				LogB.Information("Simple-ret");
				return false;
			}
			
			if(statisticSubType != Catalog.GetString("No indexes")) 
			{
				string indexType = "";
				if(statisticSubType == Catalog.GetString(Constants.SubtractionBetweenTests))
					indexType = "subtraction";
				else if(statisticSubType == Constants.ChronojumpProfileStr())
					indexType = "ChronojumpProfile";
				else if(statisticSubType == Constants.IeIndexFormula) 
					indexType = "IE";
				else if(statisticSubType == Constants.ArmsUseIndexFormula) 
					indexType = "Arms Use Index";
				else if(statisticSubType == Constants.IRnaIndexFormula) 
					indexType = "IRna";
				else if(statisticSubType == Constants.IRaIndexFormula) 
					indexType = "IRa";
				else if(statisticSubType == Constants.FvIndexFormula) 
					indexType = "F/V";
				else if(
						statisticSubType == Constants.PotencyLewisFormulaShortStr() ||
						statisticSubType == Constants.PotencyHarmanFormulaShortStr() ||
						statisticSubType == Constants.PotencySayersSJFormulaShortStr() ||
						statisticSubType == Constants.PotencySayersCMJFormulaShortStr() ||
						statisticSubType == Constants.PotencyShettyFormulaShortStr() ||
						statisticSubType == Constants.PotencyCanavanFormulaShortStr() ||
						//statisticSubType == Constants.PotencyBahamondeFormula ||
						statisticSubType == Constants.PotencyLaraMaleApplicantsSCFormulaShortStr() ||
						statisticSubType == Constants.PotencyLaraFemaleEliteVoleiFormulaShortStr() ||
						statisticSubType == Constants.PotencyLaraFemaleMediumVoleiFormulaShortStr() ||
						statisticSubType == Constants.PotencyLaraFemaleSCStudentsFormulaShortStr() ||
						statisticSubType == Constants.PotencyLaraFemaleSedentaryFormulaShortStr()
						) {
					indexType = statisticSubType;
				}
			
				if(indexType == "subtraction") {
					if(graph) 
						myStat = new GraphJumpSimpleSubtraction(myStatTypeStruct); 
					else 
						myStat = new StatJumpSimpleSubtraction(myStatTypeStruct, treeview_stats); 
				} else if(indexType == "ChronojumpProfile") {
					if(graph) 
						//myStat = new GraphChronojumpProfile(myStatTypeStruct); 
						LogB.Warning("TODO");
					else 
						myStat = new StatChronojumpProfile(myStatTypeStruct, treeview_stats); 
				} else if(indexType == "IE" || indexType == Constants.ArmsUseIndexName || 
						indexType == "IRna" || indexType == "IRa") {
					if(graph) 
						myStat = new GraphJumpIndexes (myStatTypeStruct, indexType);
					else 
						myStat = new StatJumpIndexes(myStatTypeStruct, treeview_stats, indexType); 
				} else if(indexType == "F/V") {
					if(graph) 
						myStat = new GraphFv (myStatTypeStruct, indexType);
					else 
						myStat = new StatFv(myStatTypeStruct, treeview_stats, indexType); 
				} else {
					//indexType = (Potency sayers or lewis);
					if(graph) 
						myStat = new GraphPotency(myStatTypeStruct, indexType); 
					else 
						myStat = new StatPotency(myStatTypeStruct, treeview_stats, indexType); 
				}
			}
			else {
				JumpType myType = new JumpType(statisticApplyTo);

				//manage all weight jumps and the AllJumpsName (simple)
				if(myType.HasWeight || 
						statisticApplyTo == Constants.AllJumpsNameStr())
				{
					if(graph) 
						myStat = new GraphSjCmjAbkPlus (myStatTypeStruct);
					else 
						myStat = new StatSjCmjAbkPlus (myStatTypeStruct, treeview_stats);
				} else {
					if(graph) 
						myStat = new GraphSjCmjAbk (myStatTypeStruct);
					else 
						myStat = new StatSjCmjAbk (myStatTypeStruct, treeview_stats);
				}
			}
		}
		else if(statisticType == Constants.TypeJumpsSimpleWithTCStr())
		{
			if(statisticApplyTo.Length == 0) {
				LogB.Information("WithTC-ret");
				return false;
			}
			
			if(statisticSubType == Constants.DjIndexFormula)
			{
				if(graph) 
					myStat = new GraphDjIndex (myStatTypeStruct);
							//heightPreferred is not used, check this
				else 
					myStat = new StatDjIndex(myStatTypeStruct, treeview_stats);
							//heightPreferred is not used, check this
				
			} else if(statisticSubType == Constants.QIndexFormula)
			{
				if(graph) 
					myStat = new GraphDjQ (myStatTypeStruct);
							//heightPreferred is not used, check this
				else 
					myStat = new StatDjQ(myStatTypeStruct, treeview_stats);
							//heightPreferred is not used, check this
			
			} else if(statisticSubType == Constants.DjPowerFormula)
			{
				if(graph) 
					myStat = new GraphDjPower (myStatTypeStruct);
							//heightPreferred is not used, check this
				else 
					myStat = new StatDjPower(myStatTypeStruct, treeview_stats);
							//heightPreferred is not used, check this
			}
		}
		else if(statisticType == Constants.TypeJumpsReactiveStr()) {
			if(statisticSubType == Catalog.GetString("Average Index"))
			{
				if(graph) 
					myStat = new GraphRjIndex (myStatTypeStruct);
				else 
					myStat = new StatRjIndex(myStatTypeStruct, treeview_stats);
			}	
			else if(statisticSubType == Constants.RJPotencyBoscoFormula)
			{
				if(graph) 
					myStat = new GraphRjPotencyBosco (myStatTypeStruct);
				else 
					myStat = new StatRjPotencyBosco(myStatTypeStruct, treeview_stats);
			}
			else if(statisticSubType == Catalog.GetString("Evolution"))
			{
				if(graph) 
					myStat = new GraphRjEvolution (myStatTypeStruct, evolution_mark_consecutives);
				else 
					myStat = new StatRjEvolution(myStatTypeStruct, evolution_mark_consecutives, treeview_stats);
			}
			else if(statisticSubType == Constants.RJAVGSDRjIndexName)
			{
				if(graph) 
					myStat = new GraphRjAVGSD(myStatTypeStruct, Constants.RjIndexName);
				else
					myStat = new StatRjAVGSD(myStatTypeStruct, treeview_stats, Constants.RjIndexName);
			}
			else if(statisticSubType == Constants.RJAVGSDQIndexName)
			{
				if(graph) 
					myStat = new GraphRjAVGSD(myStatTypeStruct, Constants.QIndexName);
				else
					myStat = new StatRjAVGSD(myStatTypeStruct, treeview_stats, Constants.QIndexName);
			}
		}
		else if(statisticType == Constants.TypeRunsSimpleStr())
		{
			if(statisticApplyTo.Length == 0) {
				LogB.Information("Simple-ret");
				return false;
			}
		
			if(graph) 
				myStat = new GraphRunSimple (myStatTypeStruct);
			else
				myStat = new StatRunSimple (myStatTypeStruct, treeview_stats);
		}
		else if(statisticType == Constants.TypeRunsIntervallicStr())
		{
			if(statisticApplyTo.Length == 0) {
				LogB.Information("Simple-ret");
				return false;
			}
		
			if(graph) 
				myStat = new GraphRunIntervallic (myStatTypeStruct, evolution_mark_consecutives);
			else
				myStat = new StatRunIntervallic (myStatTypeStruct, 
						evolution_mark_consecutives, treeview_stats);
		}
		
		myStat.FakeButtonRowCheckedUnchecked.Clicked += 
			new EventHandler(on_fake_button_row_checked_clicked);
		myStat.FakeButtonRowsSelected.Clicked += 
			new EventHandler(on_fake_button_rows_selected_clicked);
		myStat.FakeButtonNoRowsSelected.Clicked += 
			new EventHandler(on_fake_button_no_rows_selected_clicked);

		myStat.PrepareData();

		if(toReport) {
			if(graph) {
				bool notEmpty = myStat.CreateGraphR(fileName, false, statCount); //dont' show
				if(notEmpty) { linkImage(fileName); }
			} else {
				writer.WriteLine(myStat.ReportString());
			}
		} else {
			if(graph) {
				//myStat.CreateGraph();
				myStat.CreateGraphR(Constants.FileNameRGraph, true, -1); //show
			}
		}
	
	
		//if we just made a graph, store is not made, 
		//and we cannot change the Male/female visualizations in the combo
		//with this we can assign a store to the graph (we assign the store of the last stat (not graph)
		if(! toReport) {
			if(! graph)
				lastStore = myStat.Store;
			else {
				myStat.Store = lastStore;
				myStat.MarkedRows = markedRows;
			}
		}

		return true;
	}
	
	void linkImage(string fileName) {
		string directoryName = Util.GetReportDirectoryName(fileName);
		
		/*
		string [] pngs = Directory.GetFiles(directoryName, "*.png");
		//if found 3 images, sure will be 1.png, 2.png and 3.png, next will be 4.png
		//there will be always a png with chronojump_logo
		writer.WriteLine("<img src=\"" + Util.GetLastPartOfPath(directoryName) + "/" + (pngs.Length -1).ToString() + ".png\">");
		*/
		writer.WriteLine("<img src=\"" + Util.GetLastPartOfPath(directoryName) + "/" + (statCount+1).ToString() + ".png\">");
	}

	public void MarkSelected(string selected) {
		myStat.MarkSelected(selected);
	}

	public void CreateOrUpdateAVGAndSD() {
		myStat.CreateOrUpdateAVGAndSD();
	}

	
	public string Enunciate {
		get { return myStat.ToString(); }
	}
	

	public ArrayList MarkedRows {
		get { return myStat.MarkedRows; }
	}

	public ArrayList PersonsWithData {
		get { return myStat.PersonsWithData; }
	}

	public Gtk.Button FakeButtonRowCheckedUnchecked {
		get { return  fakeButtonRowCheckedUnchecked; }
	}
	
	public Gtk.Button FakeButtonRowsSelected {
		get { return  fakeButtonRowsSelected; }
	}
	
	public Gtk.Button FakeButtonNoRowsSelected {
		get { return  fakeButtonNoRowsSelected; }
	}

	//if we just made a graph, store is not made, 
	//and we cannot change the Male/female visualizations in the combo
	//with this we can assign a store to the graph (we assign the store of the last stat (not graph)
	public Gtk.TreeStore LastStore {
		get { return lastStore; }
		set { 
			lastStore = value; 
			treeview_stats.Model = lastStore;
		}
	}


	~StatType() {}
}
