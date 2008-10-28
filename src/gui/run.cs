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

public class EditRunWindow : EditEventWindow
{
	static EditRunWindow EditRunWindowBox;

	//for inheritance
	protected EditRunWindow () {
	}

	public EditRunWindow (Gtk.Window parent) {
		Glade.XML gladeXML;
		gladeXML = Glade.XML.FromAssembly (Util.GetGladePath() + "chronojump.glade", "edit_event", null);
		gladeXML.Autoconnect(this);
		this.parent = parent;
		
		//put an icon to window
		UtilGtk.IconWindow(edit_event);
	
		eventBigTypeString = Catalog.GetString("run");
	}

	static new public EditRunWindow Show (Gtk.Window parent, Event myEvent, int pDN, bool metersSecondsPreferred)
	{
		if (EditRunWindowBox == null) {
			EditRunWindowBox = new EditRunWindow (parent);
		}

		EditRunWindowBox.pDN = pDN;
		
		EditRunWindowBox.initializeValues();

		EditRunWindowBox.fillDialog (myEvent);

		EditRunWindowBox.edit_event.Show ();

		return EditRunWindowBox;
	}
	
	protected override void initializeValues () {
		showTv = false;
		showTc= false;
		showFall = false;
		showDistance = true;
		showTime = true;
		showSpeed = true;
		showWeight = false;
		showLimited = false;
	}

	protected override string [] findTypes(Event myEvent) {
		string [] myTypes = SqliteRunType.SelectRunTypes("", true); //don't show allRunsName row, only select name
		return myTypes;
	}
	
	protected override void fillDistance(Event myEvent) {
		Run myRun = (Run) myEvent;
		entryDistance = myRun.Distance.ToString();
		entry_distance_value.Text = Util.TrimDecimals(entryDistance, pDN);
		//if the eventtype has not a predefined distance, make the widget sensitive
		RunType myRunType = new RunType (myRun.Type);
		if(myRunType.Distance == 0) {
			entry_distance_value.Sensitive = true;
		} else {
			entry_distance_value.Sensitive = false;
		}
	}
	
	protected override void fillTime(Event myEvent) {
		Run myRun = (Run) myEvent;
		entryTime = myRun.Time.ToString();
		
		//show all the decimals for not triming there in edit window using
		//(and having different values in formulae like GetHeightInCm ...)
		//entry_time_value.Text = Util.TrimDecimals(entryTime, pDN);
		entry_time_value.Text = entryTime;
	}
	
	protected override void fillSpeed(Event myEvent) {
		Run myRun = (Run) myEvent;
		label_speed_value.Text = Util.TrimDecimals(myRun.Speed.ToString(), pDN);
	}

	protected override void createSignal() {
		//only for jumps & runs
		combo_eventType.Changed += new EventHandler (on_combo_eventType_changed);
	}
	
	private void on_combo_eventType_changed (object o, EventArgs args) {
		//if the distance of the new runType is fixed, put this distance
		//if not conserve the old
		RunType myRunType = new RunType (UtilGtk.ComboGetActive(combo_eventType));
		if(myRunType.Distance != 0) {
			entryDistance = myRunType.Distance.ToString();
			entry_distance_value.Text = "";
			entry_distance_value.Text = Util.TrimDecimals(entryDistance, pDN);
			entry_distance_value.Sensitive = false;
		} else {
			entry_distance_value.Sensitive = true;
		}
		
		label_speed_value.Text = Util.TrimDecimals(
				Util.GetSpeed (entryDistance, entryTime, metersSecondsPreferred) , pDN);
	}

	protected override void updateEvent(int eventID, int personID, string description) {
		SqliteRun.Update(eventID, UtilGtk.ComboGetActive(combo_eventType), entryDistance, entryTime, personID, description);
	}

	protected override void on_button_cancel_clicked (object o, EventArgs args)
	{
		EditRunWindowBox.edit_event.Hide();
		EditRunWindowBox = null;
	}
	
	protected override void on_delete_event (object o, DeleteEventArgs args)
	{
		EditRunWindowBox.edit_event.Hide();
		EditRunWindowBox = null;
	}
	
	protected override void hideWindow() {
		EditRunWindowBox.edit_event.Hide();
		EditRunWindowBox = null;
	}
}
	
//--------------------------------------------------------
//---------------- EDIT RUN INTERVAL WIDGET --------------
//--------------------------------------------------------

public class EditRunIntervalWindow : EditRunWindow
{
	static EditRunIntervalWindow EditRunIntervalWindowBox;

	EditRunIntervalWindow (Gtk.Window parent) {
		Glade.XML gladeXML;
		gladeXML = Glade.XML.FromAssembly (Util.GetGladePath() + "chronojump.glade", "edit_event", null);
		gladeXML.Autoconnect(this);
		this.parent = parent;
		
		//put an icon to window
		UtilGtk.IconWindow(edit_event);
	
		eventBigTypeString = Catalog.GetString("intervallic run");
	}

	static new public EditRunIntervalWindow Show (Gtk.Window parent, Event myEvent, int pDN, bool metersSecondsPreferred)
	{
		if (EditRunIntervalWindowBox == null) {
			EditRunIntervalWindowBox = new EditRunIntervalWindow (parent);
		}

		EditRunIntervalWindowBox.pDN = pDN;
		
		EditRunIntervalWindowBox.initializeValues();

		EditRunIntervalWindowBox.fillDialog (myEvent);

		EditRunIntervalWindowBox.edit_event.Show ();

		return EditRunIntervalWindowBox;
	}
	
	protected override void initializeValues () {
		showTv = false;
		showTc= false;
		showFall = false;
		showDistance = true;
		showTime = true;
		showSpeed = true;
		showWeight = false;
		showLimited = true;
	}

	protected override string [] findTypes(Event myEvent) {
		//type cannot change on run interval
		combo_eventType.Sensitive=false;

		string [] myTypes;
		myTypes = SqliteRunType.SelectRunIntervalTypes("", true); //don't show allRunsName row, only select name
		return myTypes;
	}
	
	protected override void fillDistance(Event myEvent) {
		RunInterval myRun = (RunInterval) myEvent;
		entry_distance_value.Text = myRun.DistanceInterval.ToString() +
			"x" + myRun.Limited;
		entry_distance_value.Sensitive = false;
	}

	protected override void fillTime(Event myEvent) {
		RunInterval myRun = (RunInterval) myEvent;
		label_time_title.Text = Catalog.GetString("Total Time");
		
		//show all the decimals for not triming there in edit window using
		//(and having different values in formulae like GetHeightInCm ...)
		//entry_time_value.Text = Util.TrimDecimals(myRun.TimeTotal.ToString(), pDN);
		entry_time_value.Text = myRun.TimeTotal.ToString();
		
		//don't allow to change totaltime in rjedit
		entry_time_value.Sensitive = false; 
	}
	
	protected override void fillSpeed(Event myEvent) {
		RunInterval myRun = (RunInterval) myEvent;
		label_speed_value.Text = Util.TrimDecimals( 
				Util.GetSpeed(
					myRun.DistanceTotal.ToString(),
					myRun.TimeTotal.ToString(), 
					metersSecondsPreferred), pDN);
	}
	
	protected override void fillLimited(Event myEvent) {
		RunInterval myRun = (RunInterval) myEvent;
		label_limited_value.Text = Util.GetLimitedRounded(myRun.Limited, pDN);
	}


	protected override void updateEvent(int eventID, int personID, string description) {
		SqliteRunInterval.Update(eventID, personID, description);
	}

	protected override void on_button_cancel_clicked (object o, EventArgs args)
	{
		EditRunIntervalWindowBox.edit_event.Hide();
		EditRunIntervalWindowBox = null;
	}
	
	protected override void on_delete_event (object o, DeleteEventArgs args)
	{
		EditRunIntervalWindowBox.edit_event.Hide();
		EditRunIntervalWindowBox = null;
	}
	
	protected override void hideWindow() {
		EditRunIntervalWindowBox.edit_event.Hide();
		EditRunIntervalWindowBox = null;
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
		gladeXML = Glade.XML.FromAssembly (Util.GetGladePath() + "chronojump.glade", "repair_sub_event", null);
		gladeXML.Autoconnect(this);
		this.parent = parent;
		
		//put an icon to window
		UtilGtk.IconWindow(repair_sub_event);

		this.runInterval = myRun;
	
		repair_sub_event.Title = Catalog.GetString("Repair intervallic run");
		
		System.Globalization.NumberFormatInfo localeInfo = new System.Globalization.NumberFormatInfo();
		localeInfo = System.Globalization.NumberFormatInfo.CurrentInfo;
		label_header.Text = string.Format(Catalog.GetString("Use this window to repair this test.\nDouble clic any cell to edit it (decimal separator: '{0}')"), localeInfo.NumberDecimalSeparator);
	
		
		runType = SqliteRunType.SelectAndReturnRunIntervalType(myRun.Type);
		
		TextBuffer tb = new TextBuffer (new TextTagTable());
		tb.Text = createTextForTextView(runType);
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
		
		treeview_subevents.Selection.Changed += onSelectionEntry;
	}
	
	static public RepairRunIntervalWindow Show (Gtk.Window parent, RunInterval myRun, int pDN)
	{
		//Log.WriteLine(myRun);
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

	void onSelectionEntry (object o, EventArgs args) {
		TreeModel model;
		TreeIter iter;
		
		if (((TreeSelection)o).GetSelected(out model, out iter)) {
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
			iter = store.InsertNode(position);
			store.SetValue(iter, 1, "0");
			putRowNumbers(store);
		}
	}
	
	void on_button_add_after_clicked (object o, EventArgs args) {
		TreeModel model; 
		TreeIter iter; 
		if (treeview_subevents.Selection.GetSelected (out model, out iter)) {
			int position = Convert.ToInt32( (string) model.GetValue (iter, 0) ); //count starts at '0'
			iter = store.InsertNode(position);
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
		
		runInterval.Tracks = Util.GetNumberOfJumps(timeString, false); //don't need a GetNumberOfRuns, this works
		runInterval.TimeTotal = Util.GetTotalTime(timeString);
		runInterval.DistanceTotal = runInterval.TimeTotal * runInterval.DistanceInterval;
	
		if(runType.FixedValue > 0) {
			//if this runType has a fixed value of runs or time, limitstring has not changed
			if(runType.TracksLimited) {
				runInterval.Limited = runType.FixedValue.ToString() + "R";
			} else {
				runInterval.Limited = runType.FixedValue.ToString() + "T";
			}
		} else {
			//else limitstring should be calculated
			if(runType.TracksLimited) {
				runInterval.Limited = runInterval.Tracks.ToString() + "R";
			} else {
				runInterval.Limited = runInterval.TimeTotal + "T";
			}
		}

		//save it deleting the old first for having the same uniqueID
		SqliteRun.Delete(Constants.RunIntervalTable, runInterval.UniqueID.ToString());
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
		gladeXML = Glade.XML.FromAssembly (Util.GetGladePath() + "chronojump.glade", "run_extra", null);
		gladeXML.Autoconnect(this);
		this.parent = parent;
		
		//put an icon to window
		UtilGtk.IconWindow(run_extra);
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

public class RunsMoreWindow : EventMoreWindow 
{
	[Widget] Gtk.Window jumps_runs_more;
	static RunsMoreWindow RunsMoreWindowBox;
	
	private double selectedDistance;
	
	RunsMoreWindow (Gtk.Window parent) {
		Glade.XML gladeXML;
		gladeXML = Glade.XML.FromAssembly (Util.GetGladePath() + "chronojump.glade", "jumps_runs_more", null);
		gladeXML.Autoconnect(this);
		this.parent = parent;
		
		//put an icon to window
		UtilGtk.IconWindow(jumps_runs_more);

		selectedEventType = EventType.Types.RUN.ToString();
		//name, distance, description
		store = new TreeStore(typeof (string), typeof (string), typeof (string));
		
		initializeThings();
	}
	
	static public RunsMoreWindow Show (Gtk.Window parent)
	{
		if (RunsMoreWindowBox == null) {
			RunsMoreWindowBox = new RunsMoreWindow (parent);
		}
		RunsMoreWindowBox.jumps_runs_more.Show ();
		
		return RunsMoreWindowBox;
	}
	
	protected override void createTreeView (Gtk.TreeView tv) {
		tv.HeadersVisible=true;
		int count = 0;
		
		tv.AppendColumn ( Catalog.GetString ("Name"), new CellRendererText(), "text", count++);
		tv.AppendColumn ( Catalog.GetString ("Distance"), new CellRendererText(), "text", count++);
		tv.AppendColumn ( Catalog.GetString ("Description"), new CellRendererText(), "text", count++);
	}
	
	protected override void fillTreeView (Gtk.TreeView tv, TreeStore store) 
	{
		//select data without inserting an "all jumps", and not obtain only name of jump
		string [] myRunTypes = SqliteRunType.SelectRunTypes("", false);
		foreach (string myType in myRunTypes) {
			string [] myStringFull = myType.Split(new char[] {':'});
			if(myStringFull[2] == "0") {
				myStringFull[2] = Catalog.GetString("Not defined");
			}

			RunType tempType = new RunType (myStringFull[1]);
			string description  = getDescriptionLocalised(tempType, myStringFull[3]);

			store.AppendValues (
					//myStringFull[0], //don't display the uniqueID
					myStringFull[1],	//name 
					myStringFull[2], 	//distance
					description
					);
		}	
	}

	protected override void onSelectionEntry (object o, EventArgs args)
	{
		TreeModel model;
		TreeIter iter;
		selectedEventName = "-1";
		selectedDistance = 0;
		selectedDescription = "";

		if (((TreeSelection)o).GetSelected(out model, out iter)) {
			selectedEventName = (string) model.GetValue (iter, 0);
			if( (string) model.GetValue (iter, 1) == Catalog.GetString("Not defined") ) {
				selectedDistance = 0;
			} else {
				selectedDistance = Convert.ToDouble( (string) model.GetValue (iter, 1) );
			}
			selectedDescription = (string) model.GetValue (iter, 2);
			
			button_accept.Sensitive = true;
			//update graph image test on main window
			button_selected.Click();
		}
	}
	
	protected override void on_row_double_clicked (object o, Gtk.RowActivatedArgs args)
	{
		TreeView tv = (TreeView) o;
		TreeModel model;
		TreeIter iter;

		if (tv.Selection.GetSelected (out model, out iter)) {
			//put selection in selected
			selectedEventName = (string) model.GetValue (iter, 0);
			if( (string) model.GetValue (iter, 1) == Catalog.GetString("Not defined") ) {
				selectedDistance = 0;
			} else {
				selectedDistance = Convert.ToDouble( (string) model.GetValue (iter, 1) );
			}
			selectedDescription = (string) model.GetValue (iter, 2);

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

	public double SelectedDistance {
		get { return selectedDistance; }
	}
}

//--------------------------------------------------------
//---------------- runs_interval_more widget ------------------
//--------------------------------------------------------

public class RunsIntervalMoreWindow : EventMoreWindow 
{
	[Widget] Gtk.Window jumps_runs_more;
	static RunsIntervalMoreWindow RunsIntervalMoreWindowBox;

	private double selectedDistance;
	private bool selectedTracksLimited;
	private int selectedLimitedValue;
	private bool selectedUnlimited;
	
	RunsIntervalMoreWindow (Gtk.Window parent) {
		//the glade window is the same as jumps_more
		Glade.XML gladeXML;
		gladeXML = Glade.XML.FromAssembly (Util.GetGladePath() + "chronojump.glade", "jumps_runs_more", null);
		gladeXML.Autoconnect(this);
		this.parent = parent;
		
		//put an icon to window
		UtilGtk.IconWindow(jumps_runs_more);
		
		selectedEventType = EventType.Types.RUN.ToString();
		//name, distance, limited by tracks or seconds, limit value, description
		store = new TreeStore(typeof (string), typeof (string), typeof(string),
				typeof (string), typeof (string) );
		
		initializeThings();
	}
	
	static public RunsIntervalMoreWindow Show (Gtk.Window parent)
	{
		if (RunsIntervalMoreWindowBox == null) {
			RunsIntervalMoreWindowBox = new RunsIntervalMoreWindow (parent);
		}
		RunsIntervalMoreWindowBox.jumps_runs_more.Show ();
		
		return RunsIntervalMoreWindowBox;
	}
	
	protected override void createTreeView (Gtk.TreeView tv) {
		tv.HeadersVisible=true;
		int count = 0;

		tv.AppendColumn ( Catalog.GetString ("Name"), new CellRendererText(), "text", count++);
		tv.AppendColumn ( Catalog.GetString ("Distance"), new CellRendererText(), "text", count++);
		tv.AppendColumn ( Catalog.GetString ("Limited by"), new CellRendererText(), "text", count++);
		tv.AppendColumn ( Catalog.GetString ("Limited value"), new CellRendererText(), "text", count++);
		tv.AppendColumn ( Catalog.GetString ("Description"), new CellRendererText(), "text", count++);
	}
	
	protected override void fillTreeView (Gtk.TreeView tv, TreeStore store) 
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

			RunType tempType = new RunType (myStringFull[1]);
			string description  = getDescriptionLocalised(tempType, myStringFull[6]);

			store.AppendValues (
					//myStringFull[0], //don't display de uniqueID
					myStringFull[1],	//name 
					myStringFull[2],	//distance
					myLimiter,		//tracks or seconds or "unlimited"
					myLimiterValue,		//? or exact value (or '-' in unlimited)
					description
					);
		}	
	}

	//puts a value in private member selected
	protected override void onSelectionEntry (object o, EventArgs args)
	{
		TreeModel model;
		TreeIter iter;
		selectedEventName = "-1";
		selectedDistance = -1;
		selectedTracksLimited = false;
		selectedLimitedValue = 0;
		selectedUnlimited = false; //true if it's an unlimited run
		selectedDescription = "";

		if (((TreeSelection)o).GetSelected(out model, out iter)) {
			selectedEventName = (string) model.GetValue (iter, 0);
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
		
			selectedDescription = (string) model.GetValue (iter, 4);

			button_accept.Sensitive = true;
			//update graph image test on main window
			button_selected.Click();
		}
	}
	
	protected override void on_row_double_clicked (object o, Gtk.RowActivatedArgs args)
	{
		TreeView tv = (TreeView) o;
		TreeModel model;
		TreeIter iter;

		if (tv.Selection.GetSelected (out model, out iter)) {
			selectedEventName = (string) model.GetValue (iter, 0);
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
			
			selectedDescription = (string) model.GetValue (iter, 4);
			
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
