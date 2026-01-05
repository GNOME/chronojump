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
 * Copyright (C) 2004-2026   Xavier de Blas <xaviblas@gmail.com>
 */

using System;
using Gtk;
//using Glade;
using System.Text; //StringBuilder
using System.Collections; //ArrayList
using System.Collections.Generic; //List<T>
using System.Threading;
using Mono.Unix;


public class RepairRunIntervalWindow 
{
	Gtk.Window repair_sub_event;
	Gtk.Box hbox_notes_and_totaltime;
	Gtk.Label label_header;
	Gtk.Label label_totaltime_value;
	Gtk.TreeView treeview_subevents;
	Gtk.Button button_accept;
	Gtk.Button button_add_before;
	Gtk.Button button_add_after;
	Gtk.Button button_delete;
	Gtk.TextView textview1;

	private TreeStore store;
	static RepairRunIntervalWindow RepairRunIntervalWindowBox;

	RunType type;
	RunInterval runInterval; //used on button_accept
	private int pDN;

	private int phStartCol = 1;
	private int phEndCol = 2;
	private int laptimeCol = 3;
	private int splittimeCol = 4;

	RepairRunIntervalWindow (Gtk.Window parent, RunInterval myRun, int pDN)
	{
		/*
		Glade.XML gladeXML;
		gladeXML = Glade.XML.FromAssembly (Util.GetGladePath() + "repair_sub_event.glade", "repair_sub_event", null);
		gladeXML.Autoconnect(this);
		*/
		Gtk.Builder builder = new Gtk.Builder (null, Util.GetGladePath () + "repair_sub_event.glade", null);
		connectWidgets (builder);
		builder.Autoconnect (this);

		repair_sub_event.Parent = parent;
		
		//put an icon to window
		UtilGtk.IconWindow(repair_sub_event);

		this.runInterval = myRun;
		this.pDN = pDN;

		repair_sub_event.Title = Catalog.GetString("Repair intervallic race");
		label_header.Text = Constants.GetRepairWindowMessage ();
	
		
		type = SqliteRunIntervalType.SelectAndReturnRunIntervalType(myRun.Type, false);
		
		TextBuffer tb = new TextBuffer (new TextTagTable());
		tb.Text = createTextForTextView(type);
		textview1.Buffer = tb;
		
		createTreeView(treeview_subevents);
		//count, time
		store = new TreeStore(typeof (string), typeof (string), typeof (string), typeof (string), typeof (string));
		treeview_subevents.Model = store;
		fillTreeView (treeview_subevents, store, myRun);
	
		button_add_before.Sensitive = false;
		button_add_after.Sensitive = false;
		button_delete.Sensitive = false;
		
		label_totaltime_value.Text = Util.TrimDecimals (getTotalTime(), pDN) + " " + Catalog.GetString("seconds");
		
		treeview_subevents.Selection.Changed += onSelectionEntry;
	}
	
	static public RepairRunIntervalWindow Show (Gtk.Window parent, RunInterval myRun, int pDN)
	{
		//LogB.Information(myRun);
		if (RepairRunIntervalWindowBox == null) {
			RepairRunIntervalWindowBox = new RepairRunIntervalWindow (parent, myRun, pDN);
		}
		
		//manage window color
		if(! Config.UseSystemColor)
		{
			UtilGtk.WindowColor(RepairRunIntervalWindowBox.repair_sub_event, Config.ColorBackground);
			UtilGtk.ContrastLabelsLabel(Config.ColorBackgroundIsDark, RepairRunIntervalWindowBox.label_header);
			UtilGtk.ContrastLabelsBox(Config.ColorBackgroundIsDark, RepairRunIntervalWindowBox.hbox_notes_and_totaltime);
		}

		RepairRunIntervalWindowBox.repair_sub_event.Show ();

		return RepairRunIntervalWindowBox;
	}
	
	private string createTextForTextView (RunType myRunType) {
		string runTypeString = string.Format(Catalog.GetString(
					"RaceType: {0}."), myRunType.Name);

		string fixedString = "";
		if(myRunType.FixedValue > 0) {
			if(myRunType.TracksLimited) {
				//if it's a run type runsLimited with a fixed value, then don't allow the creation of more runs
				fixedString = "\n" +  string.Format(
						Catalog.GetPluralString(
							"This race type is fixed to one lap.",
							"This race type is fixed to {0} laps.",
							myRunType.FixedValue), 
						myRunType.FixedValue) + " " +
					Catalog.GetString("You cannot add more.");
			} else {
				//if it's a run type timeLimited with a fixed value, then complain when the total time is higher
				fixedString = "\n" + string.Format(
						Catalog.GetPluralString(
							"This race type is fixed to one second.",
							"This race type is fixed to {0} seconds.",
							myRunType.FixedValue),
						myRunType.FixedValue) + " " +
					Catalog.GetString("Totaltime cannot be greater.");
			}
		}
		return runTypeString + fixedString;
	}

	
	private void createTreeView (Gtk.TreeView myTreeView)
	{
		myTreeView.HeadersVisible=true;
		int count = 0;

		myTreeView.AppendColumn ( Catalog.GetString ("Track"), new CellRendererText(), "text", count++);
		myTreeView.AppendColumn ( "WICHRO " + Catalog.GetString ("start"), new CellRendererText(), "text", count++);
		myTreeView.AppendColumn ( "WICHRO " + Catalog.GetString ("end"), new CellRendererText(), "text", count++);

		Gtk.TreeViewColumn laptimeColumn = new Gtk.TreeViewColumn ();
		laptimeColumn.Title = Catalog.GetString("Lap time");
		Gtk.CellRendererText laptimeCell = new Gtk.CellRendererText ();
		laptimeCell.Editable = true;
		laptimeCell.Edited += laptimeCellEdited;
		laptimeColumn.PackStart (laptimeCell, true);
		laptimeColumn.AddAttribute(laptimeCell, "text", count ++);
		myTreeView.AppendColumn ( laptimeColumn );

		myTreeView.AppendColumn ( Catalog.GetString ("Split time"), new CellRendererText(), "text", count++);
	}
	
	private void laptimeCellEdited (object o, Gtk.EditedArgs args)
	{
		Gtk.TreeIter iter;
		store.GetIter (out iter, new Gtk.TreePath (args.Path));
		if(Util.IsNumber(args.NewText, true)) {
			//if it's limited by fixed value of seconds
			//and new seconds are bigger than allowed, return
			if(type.FixedValue > 0 && ! type.TracksLimited &&
					getTotalTime() //current total time in treeview
					- Convert.ToDouble((string) treeview_subevents.Model.GetValue (iter, laptimeCol)) //-old cell
					+ Convert.ToDouble(args.NewText) //+new cell
					> type.FixedValue) {	//bigger than allowed
				return;
			} else {
				store.SetValue (iter, laptimeCol, args.NewText);
				valuesUpdate ();
			}
		}
		
		//if is not number or if it was -1, the old data will remain
	}

	// laptime has been edited (or some row added, deleted)
	// need to change splittime and totaltime
	private void valuesUpdate ()
	{
		TreeIter iter;
		double splitTime = 0; //at end will be totalTime
		bool iterOk = store.GetIterFirst (out iter);
		if (iterOk) {
			do {
				double myTime = Convert.ToDouble((string) treeview_subevents.Model.GetValue (iter, laptimeCol));
				splitTime += myTime;
				store.SetValue (iter, splittimeCol, Util.TrimDecimals (splitTime, pDN));
			} while (store.IterNext (ref iter));
		}
		label_totaltime_value.Text = Util.TrimDecimals (splitTime, pDN) + " " + Catalog.GetString("seconds");
	}

	private double getTotalTime()
	{
		TreeIter myIter;
		double totalTime = 0;
		bool iterOk = store.GetIterFirst (out myIter);
		if(iterOk) {
			do {
				double myTime = Convert.ToDouble((string) treeview_subevents.Model.GetValue (myIter, laptimeCol));
				totalTime += myTime;
			} while (store.IterNext (ref myIter));
		}
		return totalTime;
	}
	
	private void fillTreeView (Gtk.TreeView tv, TreeStore store, RunInterval myRun)
	{
		if(myRun.IntervalTimesString.Length > 0)
		{
			string [] laptimeArray = myRun.IntervalTimesString.Split(new char[] {'='});

			int count = 0;
			double splitTime = 0;
			int phN = 0; // photocellN
			foreach (string laptimeStr in laptimeArray)
			{
				/* debug
				LogB.Information ("is not null: " + (myRun.Photocell_l != null).ToString ());
				if (myRun.Photocell_l != null)
				{
				       LogB.Information ("count: " + myRun.Photocell_l.Count.ToString ());
				       LogB.Information (UtilList.ListIntToSQLString (myRun.Photocell_l, ","));
				}
				*/

				// get the photocell for start/end of every row
				int phStart = 0;
				int phEnd = 0;
				if (myRun.Photocell_l != null && myRun.Photocell_l.Count > phN +1)
				{
					phStart = myRun.Photocell_l[phN ++];
					phEnd = myRun.Photocell_l[phN];
				}

				double lapTime = Convert.ToDouble (Util.CDS (laptimeStr));
				splitTime += lapTime;
				store.AppendValues (
						(count+1).ToString(),
						phStart.ToString (),
						phEnd.ToString (),
						Util.TrimDecimals (lapTime, pDN),
						Util.TrimDecimals (splitTime, pDN)
						);
				count ++;
			}
		}
	}

	void onSelectionEntry (object o, EventArgs args)
	{
		ITreeModel model;
		TreeIter iter;
		
		if (((TreeSelection)o).GetSelected(out model, out iter)) {
			button_add_before.Sensitive = true;
			button_add_after.Sensitive = true;
			button_delete.Sensitive = true;

			//don't allow to add a row before or after 
			//if the runtype is fixed to n runs and we reached n
			if(type.FixedValue > 0 && type.TracksLimited) {
				int lastRow = 0;
				do {
					lastRow = Convert.ToInt32 ((string) model.GetValue (iter, 0));
				} while (store.IterNext (ref iter));

				//don't allow if max rows reached
				if(lastRow == type.FixedValue) {
					button_add_before.Sensitive = false;
					button_add_after.Sensitive = false;
					button_delete.Sensitive = false;
				}
			}
		}
	}

	void on_button_add_before_clicked (object o, EventArgs args)
	{
		ITreeModel model; 
		TreeIter iter; 
		if (treeview_subevents.Selection.GetSelected (out model, out iter)) {
			int position = Convert.ToInt32( (string) model.GetValue (iter, 0) ) -1; //count starts at '0'
			iter = store.InsertNode(position);
			store.SetValue (iter, phStartCol, "-1");
			store.SetValue (iter, phEndCol, "-1");
			store.SetValue (iter, laptimeCol, "0");
			putRowNumbers(store);
		}
	}
	
	void on_button_add_after_clicked (object o, EventArgs args)
	{
		ITreeModel model; 
		TreeIter iter; 
		if (treeview_subevents.Selection.GetSelected (out model, out iter)) {
			int position = Convert.ToInt32( (string) model.GetValue (iter, 0) ); //count starts at '0'
			iter = store.InsertNode(position);
			store.SetValue (iter, phStartCol, "-1");
			store.SetValue (iter, phEndCol, "-1");
			store.SetValue (iter, laptimeCol, "0");
			putRowNumbers(store);
		}
	}
	
	private void putRowNumbers(TreeStore myStore)
	{
		TreeIter myIter;
		bool iterOk = myStore.GetIterFirst (out myIter);
		if(iterOk) {
			int count = 1;
			do {
				store.SetValue(myIter, 0, (count++).ToString());
			} while (myStore.IterNext (ref myIter));
		}
	}
		
	void on_button_delete_clicked (object o, EventArgs args)
	{
		ITreeModel model; 
		TreeIter iter; 
		if (treeview_subevents.Selection.GetSelected (out model, out iter)) {
			store.Remove(ref iter);
			putRowNumbers(store);

			valuesUpdate ();

			button_add_before.Sensitive = false;
			button_add_after.Sensitive = false;
			button_delete.Sensitive = false;
		}
	}
	
	void on_button_accept_clicked (object o, EventArgs args)
	{
		//foreach all lines... extrac intervalTimesString
		TreeIter myIter;
		string timeString = "";

		List<int> photocell_l = new List<int> ();
		bool first = true;

		bool iterOk = store.GetIterFirst (out myIter);
		if(iterOk)
		{
			string equal = ""; //first iteration should not appear '='
			do {
				if (first)
				{
					photocell_l.Add (Convert.ToInt32 (treeview_subevents.Model.GetValue (myIter, phStartCol)));
					first = false;
				}
				photocell_l.Add (Convert.ToInt32 (treeview_subevents.Model.GetValue (myIter, phEndCol)));

				timeString = timeString + equal + (string) treeview_subevents.Model.GetValue (myIter, laptimeCol);
				equal = "=";
			} while (store.IterNext (ref myIter));
		}
			
		//calculate other variables needed for runInterval creation
		
		runInterval.Tracks = Util.GetNumberOfJumps(timeString, false); //don't need a GetNumberOfRuns, this works
		runInterval.TimeTotal = Util.GetTotalTime(timeString);

		//distanceTotal calculation caring if distances are variable
		string distancesString = "";
		if(runInterval.DistanceInterval == -1)
			distancesString = type.DistancesString;

		runInterval.DistanceTotal = Util.GetRunITotalDistance(runInterval.DistanceInterval, distancesString, runInterval.Tracks);
		runInterval.Photocell_l = photocell_l;

		if(timeString != runInterval.IntervalTimesString)
			runInterval.IntervalTimesString = timeString;
	
		if(type.FixedValue > 0) {
			//if this t'Type has a fixed value of runs or time, limitstring has not changed
			if(type.TracksLimited) {
				runInterval.Limited = type.FixedValue.ToString() + "R";
			} else {
				runInterval.Limited = type.FixedValue.ToString() + "T";
			}
		} else {
			//else limitstring should be calculated
			if(type.TracksLimited) {
				runInterval.Limited = runInterval.Tracks.ToString() + "R";
			} else {
				runInterval.Limited = runInterval.TimeTotal + "T";
			}
		}

		//save it deleting the old first for having the same uniqueID
		Sqlite.Delete(false, Constants.RunIntervalTable, runInterval.UniqueID);
		runInterval.InsertAtDB(false, Constants.RunIntervalTable); 
		/*
		SqliteRun.InsertInterval(false, Constants.RunIntervalTable, runInterval.UniqueID.ToString(), 
				runInterval.PersonID, runInterval.SessionID, 
				runInterval.Type, 
				runs * runInterval.DistanceInterval,	//distanceTotal
				Util.GetTotalTime(timeString), //timeTotal
				runInterval.DistanceInterval,		//distanceInterval
				timeString, runs, 
				runInterval.Description,
				limitString
				);
				*/

		//close the window
		RepairRunIntervalWindowBox.repair_sub_event.Hide();
		RepairRunIntervalWindowBox = null;
	}

	void on_button_cancel_clicked (object o, EventArgs args)
	{
		RepairRunIntervalWindowBox.repair_sub_event.Hide();
		RepairRunIntervalWindowBox = null;
	}
	
	void on_delete_event (object o, DeleteEventArgs args)
	{
		RepairRunIntervalWindowBox.repair_sub_event.Hide();
		RepairRunIntervalWindowBox = null;
	}
	
	public Button Button_accept 
	{
		set { button_accept = value;	}
		get { return button_accept;	}
	}

	private void connectWidgets (Gtk.Builder builder)
	{
		repair_sub_event = (Gtk.Window) builder.GetObject ("repair_sub_event");
		hbox_notes_and_totaltime = (Gtk.Box) builder.GetObject ("hbox_notes_and_totaltime");
		label_header = (Gtk.Label) builder.GetObject ("label_header");
		label_totaltime_value = (Gtk.Label) builder.GetObject ("label_totaltime_value");
		treeview_subevents = (Gtk.TreeView) builder.GetObject ("treeview_subevents");
		button_accept = (Gtk.Button) builder.GetObject ("button_accept");
		button_add_before = (Gtk.Button) builder.GetObject ("button_add_before");
		button_add_after = (Gtk.Button) builder.GetObject ("button_add_after");
		button_delete = (Gtk.Button) builder.GetObject ("button_delete");
		textview1 = (Gtk.TextView) builder.GetObject ("textview1");
	}
}
