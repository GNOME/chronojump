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
using Gtk;
using Glade;
using System.Text; //StringBuilder
using System.Collections; //ArrayList

using System.Threading;
using Mono.Unix;

//--------------------------------------------------------
//---------------- EDIT RUN WIDGET -----------------------
//--------------------------------------------------------

public class EditRunWindow 
{
	[Widget] Gtk.Window edit_run;
	[Widget] Gtk.Button button_accept;
	[Widget] Gtk.Label label_header;
	[Widget] Gtk.Label label_run_id_value;
	[Widget] Gtk.Entry entry_distance;
	[Widget] Gtk.Entry entry_time;
	[Widget] Gtk.Label label_speed_value;
	
	[Widget] Gtk.Box hbox_combo_runType;
	[Widget] Gtk.Combo combo_runType;
	[Widget] Gtk.Box hbox_combo_runner;
	[Widget] Gtk.Combo combo_runners;
	
	[Widget] Gtk.TextView textview_description;
	
	[Widget] Gtk.Label label_limited_name;
	[Widget] Gtk.Label label_limited_value;

	static EditRunWindow EditRunWindowBox;
	Gtk.Window parent;
	int pDN;
	bool metersSecondsPreferred;
	string entryDistance; //contains a entry that is a Number. If changed the entry as is not a number, recuperate this
	string entryTime; 

	string type;

	EditRunWindow (Gtk.Window parent) {
		Glade.XML gladeXML;
		try {
			gladeXML = Glade.XML.FromAssembly ("chronojump.glade", "edit_run", null);
		} catch {
			gladeXML = Glade.XML.FromAssembly ("chronojump.glade.chronojump.glade", "edit_run", null);
		}

		gladeXML.Autoconnect(this);
		this.parent = parent;
		
		System.Globalization.NumberFormatInfo localeInfo = new System.Globalization.NumberFormatInfo();
		localeInfo = System.Globalization.NumberFormatInfo.CurrentInfo;
		label_header.Text = string.Format(Catalog.GetString("Use this window to edit a run.\n(decimal separator: '{0}')"), localeInfo.NumberDecimalSeparator);
	}
	
	static public EditRunWindow Show (Gtk.Window parent, Run myRun, int pDN, bool metersSecondsPreferred)
	{
		if (EditRunWindowBox == null) {
			EditRunWindowBox = new EditRunWindow (parent);
		}
		
		EditRunWindowBox.pDN = pDN;
		EditRunWindowBox.metersSecondsPreferred = metersSecondsPreferred;
		
		EditRunWindowBox.edit_run.Show ();

		EditRunWindowBox.fillDialog (myRun);


		return EditRunWindowBox;
	}
	
	private void fillDialog (Run myRun)
	{
		label_run_id_value.Text = myRun.UniqueID.ToString();

		entryDistance = myRun.Distance.ToString();
		entry_distance.Text = Util.TrimDecimals(entryDistance, pDN);
		//if the jumptype hasnot a predefined distance, make the widget sensitive
		RunType myRunType = new RunType (myRun.Type);
		if(myRunType.Distance == 0) {
			entry_distance.Sensitive = true;
		} else {
			entry_distance.Sensitive = false;
		}
			
		entryTime = myRun.Time.ToString();
		entry_time.Text = Util.TrimDecimals(entryTime, pDN);
		label_speed_value.Text = Util.TrimDecimals(myRun.Speed.ToString(), pDN);

		TextBuffer tb = new TextBuffer (new TextTagTable());
		tb.SetText(myRun.Description);
		textview_description.Buffer = tb;

		combo_runType = new Combo ();
		string [] runTypes = SqliteRunType.SelectRunTypes("", true); //don't show allRunsName row, only select name
		combo_runType.PopdownStrings = runTypes;
		foreach (string runType in runTypes) {
			if (runType == myRun.Type) {
				combo_runType.Entry.Text = runType;
			}
		}
		combo_runType.Entry.Changed += new EventHandler (on_combo_runType_changed);
		hbox_combo_runType.PackStart(combo_runType, true, true, 0);
		hbox_combo_runType.ShowAll();
		
		string [] runners = SqlitePersonSession.SelectCurrentSession(myRun.SessionID, false); //not reversed
		combo_runners = new Combo();
		combo_runners.PopdownStrings = runners;
		foreach (string runner in runners) {
			Console.WriteLine("runner: {0}, name: {1}", runner, myRun.PersonID + ":" + myRun.RunnerName);
			if (runner == myRun.PersonID + ":" + myRun.RunnerName) {
				combo_runners.Entry.Text = runner;
			}
		}
		
		hbox_combo_runner.PackStart(combo_runners, true, true, 0);
		hbox_combo_runner.ShowAll();
	
		label_limited_name.Hide();
		label_limited_value.Hide();
		
	
	}
		
	private void on_entry_time_changed (object o, EventArgs args) {
		if(Util.IsNumber(entry_time.Text.ToString())){
			entryTime = entry_time.Text.ToString();
			label_speed_value.Text = Util.TrimDecimals(
					Util.GetSpeed (entryDistance, entryTime, metersSecondsPreferred) , pDN);
		} else {
			entry_time.Text = "";
			entry_time.Text = entryTime;
		}
	}
	
	private void on_entry_distance_changed (object o, EventArgs args) {
		if(Util.IsNumber(entry_distance.Text.ToString())){
			entryDistance = entry_distance.Text.ToString();
			label_speed_value.Text = Util.TrimDecimals(
					Util.GetSpeed (entryDistance, entryTime, metersSecondsPreferred) , pDN);
		} else {
			entry_distance.Text = "";
			entry_distance.Text = entryDistance;
		}
	}
		
		
	private void on_combo_runType_changed (object o, EventArgs args) {
		//if the distance of the new runType is fixed, put this distance
		//if not conserve the old
		RunType myRunType = new RunType (combo_runType.Entry.Text);
		if(myRunType.Distance != 0) {
			entryDistance = myRunType.Distance.ToString();
			entry_distance.Text = "";
			entry_distance.Text = Util.TrimDecimals(entryDistance, pDN);
			entry_distance.Sensitive = false;
		} else {
			entry_distance.Sensitive = true;
		}
		
		label_speed_value.Text = Util.TrimDecimals(
				Util.GetSpeed (entryDistance, entryTime, metersSecondsPreferred) , pDN);
	}
		
	void on_button_cancel_clicked (object o, EventArgs args)
	{
		EditRunWindowBox.edit_run.Hide();
		EditRunWindowBox = null;
	}
	
	void on_edit_run_delete_event (object o, DeleteEventArgs args)
	{
		EditRunWindowBox.edit_run.Hide();
		EditRunWindowBox = null;
	}
	
	void on_button_accept_clicked (object o, EventArgs args)
	{
		int runID = Convert.ToInt32 ( label_run_id_value.Text );
		string myRunner = combo_runners.Entry.Text;
		string [] myRunnerFull = myRunner.Split(new char[] {':'});
		
		string myDesc = textview_description.Buffer.Text;
	
		SqliteRun.Update(runID, combo_runType.Entry.Text, entryDistance, entryTime, Convert.ToInt32 (myRunnerFull[0]), myDesc);

		EditRunWindowBox.edit_run.Hide();
		EditRunWindowBox = null;
	}

	public Button Button_accept 
	{
		set { button_accept = value;	}
		get { return button_accept;	}
	}

}

public class EditRunIntervalWindow 
{
	[Widget] Gtk.Window edit_run;
	[Widget] Gtk.Button button_accept;
	[Widget] Gtk.Label label_header;
	[Widget] Gtk.Label label_run_id_value;
	[Widget] Gtk.Entry entry_distance;
	[Widget] Gtk.Label label_time_name;
	[Widget] Gtk.Entry entry_time;
	[Widget] Gtk.Label label_speed_value;
	[Widget] Gtk.Label label_limited_value;
	[Widget] Gtk.Box hbox_combo_runType;
	[Widget] Gtk.Box hbox_combo_runner;
	[Widget] Gtk.Combo combo_runners;
	[Widget] Gtk.TextView textview_description;

	static EditRunIntervalWindow EditRunIntervalWindowBox;
	Gtk.Window parent;
	int pDN;
	bool metersSecondsPreferred;
	string type;

	EditRunIntervalWindow (Gtk.Window parent) {
		Glade.XML gladeXML;
		try {
			gladeXML = Glade.XML.FromAssembly ("chronojump.glade", "edit_run", null);
		} catch {
			gladeXML = Glade.XML.FromAssembly ("chronojump.glade.chronojump.glade", "edit_run", null);
		}

		gladeXML.Autoconnect(this);
		this.parent = parent;
		
		System.Globalization.NumberFormatInfo localeInfo = new System.Globalization.NumberFormatInfo();
		localeInfo = System.Globalization.NumberFormatInfo.CurrentInfo;
		label_header.Text = string.Format(Catalog.GetString("Use this window to edit a intervallic run.\n(decimal separator: '{0}')"), localeInfo.NumberDecimalSeparator);
	}
	
	static public EditRunIntervalWindow Show (Gtk.Window parent, RunInterval myRun, int pDN, bool metersSecondsPreferred)
	{
		Console.WriteLine(myRun);
		if (EditRunIntervalWindowBox == null) {
			EditRunIntervalWindowBox = new EditRunIntervalWindow (parent);
		}
		
		EditRunIntervalWindowBox.pDN = pDN;
		EditRunIntervalWindowBox.metersSecondsPreferred = metersSecondsPreferred;
		
		EditRunIntervalWindowBox.edit_run.Show ();

		EditRunIntervalWindowBox.fillDialog (myRun);


		return EditRunIntervalWindowBox;
	}
	
	private void fillDialog (RunInterval myRun)
	{
		label_run_id_value.Text = myRun.UniqueID.ToString();
		entry_distance.Text = myRun.DistanceInterval.ToString() + 
			"x" + myRun.Limited;
		entry_distance.Sensitive = false;

		label_time_name.Text = Catalog.GetString("Total Time");
		entry_time.Text = myRun.TimeTotal.ToString();
		//don't allow to change totaltime in rjedit
		entry_time.Sensitive = false; 
		
		label_speed_value.Text = Util.TrimDecimals( 
				Util.GetSpeed(
					myRun.DistanceTotal.ToString(),
					myRun.TimeTotal.ToString(), 
					metersSecondsPreferred), pDN);

		
		label_limited_value.Text = myRun.Limited;

		this.type = myRun.Type;

		TextBuffer tb = new TextBuffer (new TextTagTable());
		tb.SetText(myRun.Description);
		textview_description.Buffer = tb;

		string [] runners = SqlitePersonSession.SelectCurrentSession(myRun.SessionID, false); //not reversed
		combo_runners = new Combo();
		combo_runners.PopdownStrings = runners;
		foreach (string runner in runners) {
			if (runner == myRun.PersonID + ":" + myRun.RunnerName) {
				combo_runners.Entry.Text = runner;
			}
		}
		
		hbox_combo_runner.PackStart(combo_runners, true, true, 0);
		hbox_combo_runner.ShowAll();
		
		Gtk.Label label_runType = new Label();
		label_runType.Text = myRun.Type;
		hbox_combo_runType.PackStart(label_runType, false, false, 0);
		hbox_combo_runType.ShowAll();
	}
	
	private void on_entry_time_changed (object o, EventArgs args) {
		//do nothing, this is never called in reactive jumps
	}
		
	private void on_entry_distance_changed (object o, EventArgs args) {
		//do nothing, this is never called in reactive jumps
	}
		
	void on_button_cancel_clicked (object o, EventArgs args)
	{
		EditRunIntervalWindowBox.edit_run.Hide();
		EditRunIntervalWindowBox = null;
	}
	
	void on_edit_run_delete_event (object o, DeleteEventArgs args)
	{
		EditRunIntervalWindowBox.edit_run.Hide();
		EditRunIntervalWindowBox = null;
	}
	
	void on_button_accept_clicked (object o, EventArgs args)
	{
		int runID = Convert.ToInt32 ( label_run_id_value.Text );
		string myRunner = combo_runners.Entry.Text;
		string [] myRunnerFull = myRunner.Split(new char[] {':'});
		
		string myDesc = textview_description.Buffer.Text;
	
		SqliteRun.IntervalUpdate(runID, Convert.ToInt32 (myRunnerFull[0]), myDesc);

		EditRunIntervalWindowBox.edit_run.Hide();
		EditRunIntervalWindowBox = null;
	}

	public Button Button_accept 
	{
		set { button_accept = value;	}
		get { return button_accept;	}
	}

}

//--------------------------------------------------------
//---------------- Repair runInterval WIDGET -------------
//--------------------------------------------------------

public class RepairRunIntervalWindow 
{
	[Widget] Gtk.Window repair_sub_event;
	[Widget] Gtk.Label label_header;
	[Widget] Gtk.Label label_totaltime_value;
	[Widget] Gtk.TreeView treeview_subevents;
	private TreeStore store;
	[Widget] Gtk.Button button_accept;
	[Widget] Gtk.Button button_add_before;
	[Widget] Gtk.Button button_add_after;
	[Widget] Gtk.Button button_delete;
	[Widget] Gtk.TextView textview1;

	static RepairRunIntervalWindow RepairRunIntervalWindowBox;
	Gtk.Window parent;

	RunType runType;
	RunInterval runInterval; //used on button_accept
	

	RepairRunIntervalWindow (Gtk.Window parent, RunInterval myRun, int pDN) {
		Glade.XML gladeXML;
		try {
			gladeXML = Glade.XML.FromAssembly ("chronojump.glade", "repair_sub_event", null);
		} catch {
			gladeXML = Glade.XML.FromAssembly ("chronojump.glade.chronojump.glade", "repair_sub_event", null);
		}

		gladeXML.Autoconnect(this);
		this.parent = parent;
		this.runInterval = myRun;
	
		repair_sub_event.Title = Catalog.GetString("Repair intervallic run");
		
		System.Globalization.NumberFormatInfo localeInfo = new System.Globalization.NumberFormatInfo();
		localeInfo = System.Globalization.NumberFormatInfo.CurrentInfo;
		label_header.Text = string.Format(Catalog.GetString("Use this window to repair a intervallic run.\nDouble clic any cell to edit it (decimal separator: '{0}')"), localeInfo.NumberDecimalSeparator);
	
		
		runType = SqliteRunType.SelectAndReturnRunIntervalType(myRun.Type);
		
		TextBuffer tb = new TextBuffer (new TextTagTable());
		tb.SetText(createTextForTextView(runType));
		textview1.Buffer = tb;
		
		createTreeView(treeview_subevents);
		//count, time
		store = new TreeStore(typeof (string), typeof (string));
		treeview_subevents.Model = store;
		fillTreeView (treeview_subevents, store, myRun, pDN);
	
		button_add_before.Sensitive = false;
		button_add_after.Sensitive = false;
		button_delete.Sensitive = false;
		
		label_totaltime_value.Text = getTotalTime().ToString() + " " + Catalog.GetString("seconds");
	}
	
	static public RepairRunIntervalWindow Show (Gtk.Window parent, RunInterval myRun, int pDN)
	{
		//Console.WriteLine(myRun);
		if (RepairRunIntervalWindowBox == null) {
			RepairRunIntervalWindowBox = new RepairRunIntervalWindow (parent, myRun, pDN);
		}
		
		RepairRunIntervalWindowBox.repair_sub_event.Show ();

		return RepairRunIntervalWindowBox;
	}
	
	private string createTextForTextView (RunType myRunType) {
		string runTypeString = string.Format(Catalog.GetString(
					"RunType: {0}."), myRunType.Name);

		string fixedString = "";
		if(myRunType.FixedValue > 0) {
			if(myRunType.TracksLimited) {
				//if it's a run type runsLimited with a fixed value, then don't allow the creation of more runs
				fixedString = string.Format(Catalog.GetString("\nThis run type is fixed to {0} runs, you cannot add more."), myRunType.FixedValue);
			} else {
				//if it's a run type timeLimited with a fixed value, then complain when the total time is higher
				fixedString = string.Format(Catalog.GetString("\nThis run type is fixed to {0} seconds, totaltime cannot be greater."), myRunType.FixedValue);
			}
		}
		return runTypeString + fixedString;
	}

	
	private void createTreeView (Gtk.TreeView myTreeView) {
		myTreeView.HeadersVisible=true;
		int count = 0;

		myTreeView.AppendColumn ( Catalog.GetString ("Count"), new CellRendererText(), "text", count++);
		//myTreeView.AppendColumn ( Catalog.GetString ("Time"), new CellRendererText(), "text", count++);

		Gtk.TreeViewColumn timeColumn = new Gtk.TreeViewColumn ();
		timeColumn.Title = Catalog.GetString("TF");
		Gtk.CellRendererText timeCell = new Gtk.CellRendererText ();
		timeCell.Editable = true;
		timeCell.Edited += timeCellEdited;
		timeColumn.PackStart (timeCell, true);
		timeColumn.AddAttribute(timeCell, "text", count ++);
		myTreeView.AppendColumn ( timeColumn );
	}
	
	private void timeCellEdited (object o, Gtk.EditedArgs args)
	{
		Gtk.TreeIter iter;
		store.GetIter (out iter, new Gtk.TreePath (args.Path));
		if(Util.IsNumber(args.NewText)) {
			//if it's limited by fixed value of seconds
			//and new seconds are bigger than allowed, return
			if(runType.FixedValue > 0 && ! runType.TracksLimited &&
					getTotalTime() //current total time in treeview
					- Convert.ToDouble((string) treeview_subevents.Model.GetValue(iter,1)) //-old cell
					+ Convert.ToDouble(args.NewText) //+new cell
					> runType.FixedValue) {	//bigger than allowed
				return;
			} else {
				store.SetValue(iter, 1, args.NewText);

				//update the totaltime label
				label_totaltime_value.Text = getTotalTime().ToString() + " " + Catalog.GetString("seconds");
			}
		}
		
		//if is not number or if it was -1, the old data will remain
	}

	private double getTotalTime() {
		TreeIter myIter;
		double totalTime = 0;
		bool iterOk = store.GetIterFirst (out myIter);
		if(iterOk) {
			do {
				double myTime = Convert.ToDouble((string) treeview_subevents.Model.GetValue(myIter, 1));
				totalTime += myTime;
			} while (store.IterNext (ref myIter));
		}
		return totalTime;
	}
	
	private void fillTreeView (Gtk.TreeView tv, TreeStore store, RunInterval myRun, int pDN)
	{
		if(myRun.IntervalTimesString.Length > 0) {
			string [] timeArray = myRun.IntervalTimesString.Split(new char[] {'='});

			int count = 0;
			foreach (string myTime in timeArray) {
				store.AppendValues ( (count+1).ToString(), Util.TrimDecimals(myTime, pDN) );
				count ++;
			}
		}
	}

	void on_treeview_cursor_changed (object o, EventArgs args) {
		TreeView tv = (TreeView) o;
		TreeModel model;
		TreeIter iter;
		
		if (tv.Selection.GetSelected (out model, out iter)) {
			button_add_before.Sensitive = true;
			button_add_after.Sensitive = true;
			button_delete.Sensitive = true;

			//don't allow to add a row before or after 
			//if the runtype is fixed to n runs and we reached n
			if(runType.FixedValue > 0 && runType.TracksLimited) {
				int lastRow = 0;
				do {
					lastRow = Convert.ToInt32 ((string) model.GetValue (iter, 0));
				} while (store.IterNext (ref iter));

				//don't allow if max rows reached
				if(lastRow == runType.FixedValue) {
					button_add_before.Sensitive = false;
					button_add_after.Sensitive = false;
				}
			}
		}
	}

	void on_button_add_before_clicked (object o, EventArgs args) {
		TreeModel model; 
		TreeIter iter; 
		if (treeview_subevents.Selection.GetSelected (out model, out iter)) {
			int position = Convert.ToInt32( (string) model.GetValue (iter, 0) ) -1; //count starts at '0'
			store.Insert(out iter, position);
			store.SetValue(iter, 1, "0");
			putRowNumbers(store);
		}
	}
	
	void on_button_add_after_clicked (object o, EventArgs args) {
		TreeModel model; 
		TreeIter iter; 
		if (treeview_subevents.Selection.GetSelected (out model, out iter)) {
			int position = Convert.ToInt32( (string) model.GetValue (iter, 0) ); //count starts at '0'
			store.Insert(out iter, position);
			store.SetValue(iter, 1, "0");
			putRowNumbers(store);
		}
	}
	
	private void putRowNumbers(TreeStore myStore) {
		TreeIter myIter;
		bool iterOk = myStore.GetIterFirst (out myIter);
		if(iterOk) {
			int count = 1;
			do {
				store.SetValue(myIter, 0, (count++).ToString());
			} while (myStore.IterNext (ref myIter));
		}
	}
		
	void on_button_delete_clicked (object o, EventArgs args) {
		TreeModel model; 
		TreeIter iter; 
		if (treeview_subevents.Selection.GetSelected (out model, out iter)) {
			store.Remove(ref iter);
			putRowNumbers(store);
		
			label_totaltime_value.Text = getTotalTime().ToString() + " " + Catalog.GetString("seconds");

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
		
		bool iterOk = store.GetIterFirst (out myIter);
		if(iterOk) {
			string equal= ""; //first iteration should not appear '='
			do {
				timeString = timeString + equal + (string) treeview_subevents.Model.GetValue (myIter, 1);
				equal = "=";
			} while (store.IterNext (ref myIter));
		}
			
		//calculate other variables needed for runInterval creation
		
		int runs = Util.GetNumberOfJumps(timeString, false); //don't need a GetNumberOfRuns, this works
		string limitString = "";
	
		if(runType.FixedValue > 0) {
			//if this runType has a fixed value of runs or time, limitstring has not changed
			if(runType.TracksLimited) {
				limitString = runType.FixedValue.ToString() + "R";
			} else {
				limitString = runType.FixedValue.ToString() + "T";
			}
		} else {
			//else limitstring should be calculated
			if(runType.TracksLimited) {
				limitString = runs.ToString() + "R";
			} else {
				limitString = Util.GetTotalTime(timeString) + "T";
			}
		}

		//save it deleting the old first for having the same uniqueID
		SqliteRun.Delete("runInterval", runInterval.UniqueID.ToString());
		SqliteRun.InsertInterval("runInterval", runInterval.UniqueID.ToString(), 
				runInterval.PersonID, runInterval.SessionID, 
				runInterval.Type, 
				runs * runInterval.DistanceInterval,	//distanceTotal
				Util.GetTotalTime(timeString), //timeTotal
				runInterval.DistanceInterval,		//distanceInterval
				timeString, runs, 
				runInterval.Description,
				limitString
				);

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

}

//--------------------------------------------------------
//---------------- run extra WIDGET --------------------
//--------------------------------------------------------

public class RunExtraWindow 
{
	[Widget] Gtk.Label label_limit;
	[Widget] Gtk.SpinButton spinbutton_limit;
	[Widget] Gtk.Label label_limit_units;
	[Widget] Gtk.Label label_distance;
	[Widget] Gtk.SpinButton distance_limit;
	[Widget] Gtk.Label label_meters;
	[Widget] Gtk.Window run_extra;
	[Widget] Gtk.SpinButton spinbutton_distance;
	[Widget] Gtk.Button button_accept;

	static int distance = 100;
	static int limited = 10;
	static bool tracksLimited;
	
	static RunExtraWindow RunExtraWindowBox;
	Gtk.Window parent;

	RunExtraWindow (Gtk.Window parent) {
		Glade.XML gladeXML;
		try {
			gladeXML = Glade.XML.FromAssembly ("chronojump.glade", "run_extra", null);
		} catch {
			gladeXML = Glade.XML.FromAssembly ("chronojump.glade.chronojump.glade", "run_extra", null);
		}

		gladeXML.Autoconnect(this);
		this.parent = parent;
	}
	
	static public RunExtraWindow Show (Gtk.Window parent, RunType myRunType) 
	{
		if (RunExtraWindowBox == null) {
			RunExtraWindowBox = new RunExtraWindow (parent);
		}
		
		if(myRunType.HasIntervals && ! myRunType.Unlimited) {
			string tracksName = Catalog.GetString("tracks");
			string secondsName = Catalog.GetString("seconds");
			if(myRunType.TracksLimited) {
				tracksLimited = true;
				RunExtraWindowBox.label_limit_units.Text = tracksName;
			} else {
				tracksLimited = false;
				RunExtraWindowBox.label_limit_units.Text = secondsName;
			}
			if(myRunType.FixedValue > 0) {
				RunExtraWindowBox.spinbutton_limit.Sensitive = false;
				RunExtraWindowBox.spinbutton_limit.Value = myRunType.FixedValue;
			}
		} else {
			hideIntervalData();	
		}

		if(myRunType.Distance > 0) {
			hideDistanceData();	
		}
		
		RunExtraWindowBox.spinbutton_distance.Value = distance;
		RunExtraWindowBox.spinbutton_limit.Value = limited;
		
		RunExtraWindowBox.run_extra.Show ();

		return RunExtraWindowBox;
	}
	
	static void hideIntervalData () {
		RunExtraWindowBox.label_limit.Sensitive = false;
		RunExtraWindowBox.spinbutton_limit.Sensitive = false;
		RunExtraWindowBox.label_limit_units.Sensitive = false;
	}
	
	static void hideDistanceData () {
		RunExtraWindowBox.label_distance.Sensitive = false;
		RunExtraWindowBox.spinbutton_distance.Sensitive = false;
		RunExtraWindowBox.label_meters.Sensitive = false;
	}
	
	void on_button_cancel_clicked (object o, EventArgs args)
	{
		RunExtraWindowBox.run_extra.Hide();
		RunExtraWindowBox = null;
	}
	
	void on_run_extra_delete_event (object o, DeleteEventArgs args)
	{
		RunExtraWindowBox.run_extra.Hide();
		RunExtraWindowBox = null;
	}
	
	void on_button_accept_clicked (object o, EventArgs args)
	{
		distance = (int) spinbutton_distance.Value;
		limited = (int) spinbutton_limit.Value;
		
		RunExtraWindowBox.run_extra.Hide();
		RunExtraWindowBox = null;
	}

	public Button Button_accept 
	{
		set { button_accept = value;	}
		get { return button_accept;	}
	}
	
	public int Distance 
	{
		get { return distance;	}
	}
	
	public int Limited 
	{
		get { return limited;	}
	}
}


//--------------------------------------------------------
//---------------- runs_more widget ----------------------
//--------------------------------------------------------

public class RunsMoreWindow 
{
	[Widget] Gtk.Window jumps_runs_more;
	
	private TreeStore store;
	[Widget] Gtk.TreeView treeview_more;
	[Widget] Gtk.Button button_accept;

	static RunsMoreWindow RunsMoreWindowBox;
	Gtk.Window parent;
	
	private string selectedRunType;
	private double selectedDistance;
	
	RunsMoreWindow (Gtk.Window parent) {
		Glade.XML gladeXML;
		try {
			gladeXML = Glade.XML.FromAssembly ("chronojump.glade", "jumps_runs_more", null);
		} catch {
			gladeXML = Glade.XML.FromAssembly ("chronojump.glade.chronojump.glade", "jumps_runs_more", null);
		}

		gladeXML.Autoconnect(this);
		this.parent = parent;
		
		createTreeView(treeview_more);
		//name, distance, description
		store = new TreeStore(typeof (string), typeof (string), typeof (string));
		treeview_more.Model = store;
		fillTreeView(treeview_more,store);

		button_accept.Sensitive = false;
	}
	
	static public RunsMoreWindow Show (Gtk.Window parent)
	{
		if (RunsMoreWindowBox == null) {
			RunsMoreWindowBox = new RunsMoreWindow (parent);
		}
		RunsMoreWindowBox.jumps_runs_more.Show ();
		
		return RunsMoreWindowBox;
	}
	
	private void createTreeView (Gtk.TreeView tv) {
		tv.HeadersVisible=true;
		int count = 0;
		
		tv.AppendColumn ( Catalog.GetString ("Name"), new CellRendererText(), "text", count++);
		tv.AppendColumn ( Catalog.GetString ("Distance"), new CellRendererText(), "text", count++);
		tv.AppendColumn ( Catalog.GetString ("Description"), new CellRendererText(), "text", count++);
	}
	
	private void fillTreeView (Gtk.TreeView tv, TreeStore store) 
	{
		//select data without inserting an "all jumps", and not obtain only name of jump
		string [] myRunTypes = SqliteRunType.SelectRunTypes("", false);
		foreach (string myType in myRunTypes) {
			string [] myStringFull = myType.Split(new char[] {':'});
			if(myStringFull[2] == "0") {
				myStringFull[2] = Catalog.GetString("Not defined");
			}

			store.AppendValues (
					//myStringFull[0], //don't display the uniqueID
					myStringFull[1],	//name 
					myStringFull[2], 	//distance
					myStringFull[3]		//description
					);
		}	
	}

	//puts a value in private member selected
	private void on_treeview_changed (object o, EventArgs args)
	{
		TreeView tv = (TreeView) o;
		TreeModel model;
		TreeIter iter;
		selectedRunType = "-1";
		selectedDistance = 0;

		// you get the iter and the model if something is selected
		if (tv.Selection.GetSelected (out model, out iter)) {
			selectedRunType = (string) model.GetValue (iter, 0);
			if( (string) model.GetValue (iter, 1) == Catalog.GetString("Not defined") ) {
				selectedDistance = 0;
			} else {
				selectedDistance = Convert.ToDouble( (string) model.GetValue (iter, 1) );
			}
			
			button_accept.Sensitive = true;
		}
	}
	
	void on_row_double_clicked (object o, Gtk.RowActivatedArgs args)
	{
		TreeView tv = (TreeView) o;
		TreeModel model;
		TreeIter iter;

		if (tv.Selection.GetSelected (out model, out iter)) {
			//put selection in selected
			selectedRunType = (string) model.GetValue (iter, 0);
			if( (string) model.GetValue (iter, 1) == Catalog.GetString("Not defined") ) {
				selectedDistance = 0;
			} else {
				selectedDistance = Convert.ToDouble( (string) model.GetValue (iter, 1) );
			}

			//activate on_button_accept_clicked()
			button_accept.Activate();
		}
	}
	
	void on_button_cancel_clicked (object o, EventArgs args)
	{
		RunsMoreWindowBox.jumps_runs_more.Hide();
		RunsMoreWindowBox = null;
	}
	
	void on_jumps_runs_more_delete_event (object o, DeleteEventArgs args)
	{
		RunsMoreWindowBox.jumps_runs_more.Hide();
		RunsMoreWindowBox = null;
	}
	
	void on_button_accept_clicked (object o, EventArgs args)
	{
		RunsMoreWindowBox.jumps_runs_more.Hide();
	}
	
	//when a run is done using runsMoreWindow, the accept doesn't destroy this instance, because 
	//later we need data from it.
	//This is used for destroying, then if a new run type is added, it will be shown at first time clicking "more" button
	public void Destroy() {		
		RunsMoreWindowBox = null;
	}


	public Button Button_accept 
	{
		set {
			button_accept = value;	
		}
		get {
			return button_accept;
		}
	}
	
	public string SelectedRunType 
	{
		set {
			selectedRunType = value;	
		}
		get {
			return selectedRunType;
		}
	}
	
	public double SelectedDistance 
	{
		get {
			return selectedDistance;
		}
	}
	
}

//--------------------------------------------------------
//---------------- runs_interval_more widget ------------------
//--------------------------------------------------------

public class RunsIntervalMoreWindow 
{
	[Widget] Gtk.Window jumps_runs_more;
	
	private TreeStore store;
	[Widget] Gtk.TreeView treeview_more;
	[Widget] Gtk.Button button_accept;

	static RunsIntervalMoreWindow RunsIntervalMoreWindowBox;
	Gtk.Window parent;

	private string selectedRunType;
	private double selectedDistance;
	private bool selectedTracksLimited;
	private int selectedLimitedValue;
	private bool selectedUnlimited;
	
	RunsIntervalMoreWindow (Gtk.Window parent) {
		//the glade window is the same as jumps_more
		Glade.XML gladeXML;
		try {
			gladeXML = Glade.XML.FromAssembly ("chronojump.glade", "jumps_runs_more", null);
		} catch {
			gladeXML = Glade.XML.FromAssembly ("chronojump.glade.chronojump.glade", "jumps_runs_more", null);
		}

		gladeXML.Autoconnect(this);
		this.parent = parent;
		
		createTreeView(treeview_more);
		//name, distance, limited by tracks or seconds, limit value, description
		store = new TreeStore(typeof (string), typeof (string), typeof(string),
				typeof (string), typeof (string) );
		treeview_more.Model = store;
		fillTreeView(treeview_more,store);
			
		button_accept.Sensitive = false;
	}
	
	static public RunsIntervalMoreWindow Show (Gtk.Window parent)
	{
		if (RunsIntervalMoreWindowBox == null) {
			RunsIntervalMoreWindowBox = new RunsIntervalMoreWindow (parent);
		}
		RunsIntervalMoreWindowBox.jumps_runs_more.Show ();
		
		return RunsIntervalMoreWindowBox;
	}
	
	private void createTreeView (Gtk.TreeView tv) {
		tv.HeadersVisible=true;
		int count = 0;

		tv.AppendColumn ( Catalog.GetString ("Name"), new CellRendererText(), "text", count++);
		tv.AppendColumn ( Catalog.GetString ("Distance"), new CellRendererText(), "text", count++);
		tv.AppendColumn ( Catalog.GetString ("Limited by"), new CellRendererText(), "text", count++);
		tv.AppendColumn ( Catalog.GetString ("Limited value"), new CellRendererText(), "text", count++);
		tv.AppendColumn ( Catalog.GetString ("Description"), new CellRendererText(), "text", count++);
	}
	
	private void fillTreeView (Gtk.TreeView tv, TreeStore store) 
	{
		//select data without inserting an "all jumps", and not obtain only name of jump
		string [] myTypes = SqliteRunType.SelectRunIntervalTypes("", false);
		foreach (string myType in myTypes) {
			string [] myStringFull = myType.Split(new char[] {':'});
			
			//limited
			string myLimiter = "";
			string myLimiterValue = "";
			
			//check if it's unlimited
			if(myStringFull[5] == "1") {
				myLimiter= Catalog.GetString("Unlimited");
				myLimiterValue = "-";
			} else {
				myLimiter = Catalog.GetString("Tracks");
				if(myStringFull[3] == "0") {
					myLimiter = Catalog.GetString("Seconds");
				}
				myLimiterValue = "?";
				if(Convert.ToDouble(myStringFull[4]) > 0) {
					myLimiterValue = myStringFull[4];
				}
			}

			store.AppendValues (
					//myStringFull[0], //don't display de uniqueID
					myStringFull[1],	//name 
					myStringFull[2],	//distance
					myLimiter,		//tracks or seconds or "unlimited"
					myLimiterValue,		//? or exact value (or '-' in unlimited)
					myStringFull[6]		//description
					);
		}	
	}

	//puts a value in private member selected
	private void on_treeview_changed (object o, EventArgs args)
	{
		TreeView tv = (TreeView) o;
		TreeModel model;
		TreeIter iter;
		selectedRunType = "-1";
		selectedDistance = -1;
		selectedTracksLimited = false;
		selectedLimitedValue = 0;
		selectedUnlimited = false; //true if it's an unlimited run

		// you get the iter and the model if something is selected
		if (tv.Selection.GetSelected (out model, out iter)) {
			selectedRunType = (string) model.GetValue (iter, 0);
			selectedDistance = Convert.ToDouble( (string) model.GetValue (iter, 1) );

			if( (string) model.GetValue (iter, 2) == Catalog.GetString("Unlimited") ) {
				selectedUnlimited = true;
			} 

			if( (string) model.GetValue (iter, 2) == Catalog.GetString("Tracks") ) {
				selectedTracksLimited = true;
			}

			if( (string) model.GetValue (iter, 3) == "?" || (string) model.GetValue (iter, 3) == "-" ) {
				selectedLimitedValue = 0;
			} else {
				selectedLimitedValue = Convert.ToInt32( (string) model.GetValue (iter, 3) );
			}

			button_accept.Sensitive = true;
		}
	}
	
	void on_row_double_clicked (object o, Gtk.RowActivatedArgs args)
	{
		TreeView tv = (TreeView) o;
		TreeModel model;
		TreeIter iter;

		if (tv.Selection.GetSelected (out model, out iter)) {
			selectedRunType = (string) model.GetValue (iter, 0);
			selectedDistance = Convert.ToDouble( (string) model.GetValue (iter, 1) );

			if( (string) model.GetValue (iter, 2) == Catalog.GetString("Unlimited") ) {
				selectedUnlimited = true;
			} 
			
			if( (string) model.GetValue (iter, 2) == Catalog.GetString("Tracks") ) {
				selectedTracksLimited = true;
			}

			if( (string) model.GetValue (iter, 3) == "?" || (string) model.GetValue (iter, 3) == "-" ) {
				selectedLimitedValue = 0;
			} else {
				selectedLimitedValue = Convert.ToInt32( (string) model.GetValue (iter, 3) );
			}
			
			//activate on_button_accept_clicked()
			button_accept.Activate();
		}
	}
	
	
	void on_button_cancel_clicked (object o, EventArgs args)
	{
		RunsIntervalMoreWindowBox.jumps_runs_more.Hide();
		RunsIntervalMoreWindowBox = null;
	}
	
	void on_jumps_runs_more_delete_event (object o, DeleteEventArgs args)
	{
		RunsIntervalMoreWindowBox.jumps_runs_more.Hide();
		RunsIntervalMoreWindowBox = null;
	}
	
	void on_button_accept_clicked (object o, EventArgs args)
	{
		RunsIntervalMoreWindowBox.jumps_runs_more.Hide();
	}
	
	//when a runInterval is done using runsIntervalMoreWindow, the accept doesn't destroy this instance, because 
	//later we need data from it.
	//This is used for destroying, then if a new runInterval type is added, it will be shown at first time clicking "more" button
	public void Destroy() {		
		RunsIntervalMoreWindowBox = null;
	}

	public Button Button_accept 
	{
		set {
			button_accept = value;	
		}
		get {
			return button_accept;
		}
	}
	
	public string SelectedRunType 
	{
		get { return selectedRunType; }
	}

	public double SelectedDistance 
	{
		get { return selectedDistance; }
	}
	
	public bool SelectedTracksLimited 
	{
		get { return selectedTracksLimited; }
	}
	
	public int SelectedLimitedValue 
	{ 
		get { return selectedLimitedValue; }
	}
	
	public bool SelectedUnlimited 
	{
		get { return selectedUnlimited; }
	}
	
}
