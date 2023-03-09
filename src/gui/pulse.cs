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
 * Copyright (C) 2004-2023   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
using Gtk;
//using Glade;
using System.Text; //StringBuilder
using System.Collections; //ArrayList

using Mono.Unix;



//--------------------------------------------------------
//---------------- EDIT PULSE WIDGET ---------------------
//--------------------------------------------------------

public class EditPulseWindow : EditEventWindow
{
	static EditPulseWindow EditPulseWindowBox;

	EditPulseWindow (Gtk.Window parent)
	{
		/*
		Glade.XML gladeXML;
		gladeXML = Glade.XML.FromAssembly (Util.GetGladePath() + "edit_event.glade", "edit_event", null);
		gladeXML.Autoconnect(this);
		*/
		Gtk.Builder builder = new Gtk.Builder (null, Util.GetGladePath () + "edit_event.glade", null);
		connectWidgetsEditEvent (builder);
		builder.Autoconnect (this);

		this.parent = parent;
		
		//put an icon to window
		UtilGtk.IconWindow(edit_event);
	
		eventBigTypeString = Catalog.GetString("pulse");
		headerShowDecimal = false;
	}

	static new public EditPulseWindow Show (Gtk.Window parent, Event myEvent, int pDN)
	{
		if (EditPulseWindowBox == null) {
			EditPulseWindowBox = new EditPulseWindow (parent);
		}

		EditPulseWindowBox.pDN = pDN;
		
		EditPulseWindowBox.initializeValues();

		EditPulseWindowBox.fillDialog (myEvent);

		EditPulseWindowBox.edit_event.Show ();

		return EditPulseWindowBox;
	}
	
	protected override void initializeValues () {
		typeOfTest = Constants.TestTypes.PULSE;
		showType = true;
		showRunStart = false;
		showTv = false;
		showTc= false;
		showFall = false;
		showDistance = false;
		showTime = false;
		showSpeed = false;
		showWeight = false;
		showLimited = false;
		showMistakes = false;
	}

	protected override string [] findTypes(Event myEvent) {
		string [] myTypes = SqlitePulseType.SelectPulseTypes("", true); //don't show allEventName row, only select name

		//on pulses can not change type
		combo_eventType.Sensitive = false;

		return myTypes;
	}
	
	protected override void updateEvent(int eventID, int personID, string description) {
		SqlitePulse.Update(eventID, personID, description);
	}

	protected override void on_button_cancel_clicked (object o, EventArgs args)
	{
		EditPulseWindowBox.edit_event.Hide();
		EditPulseWindowBox = null;
	}
	
	protected override void on_delete_event (object o, DeleteEventArgs args)
	{
		EditPulseWindowBox.edit_event.Hide();
		EditPulseWindowBox = null;
	}
	
	protected override void hideWindow() {
		EditPulseWindowBox.edit_event.Hide();
		EditPulseWindowBox = null;
	}

}


//--------------------------------------------------------
//---------------- Repair pulse WIDGET -------------------
//--------------------------------------------------------

public class RepairPulseWindow 
{
	Gtk.Window repair_sub_event;
	Gtk.HBox hbox_notes_and_totaltime;
	Gtk.Label label_header;
	Gtk.Label label_totaltime_value;
	Gtk.TreeView treeview_subevents;
	Gtk.Button button_accept;
	Gtk.Button button_add_before;
	Gtk.Button button_add_after;
	Gtk.Button button_delete;
	Gtk.TextView textview1;

	private TreeStore store;
	static RepairPulseWindow RepairPulseWindowBox;

	PulseType pulseType;
	Pulse myPulse; //used on button_accept
	

	RepairPulseWindow (Gtk.Window parent, Pulse myPulse, int pDN)
	{
		/*
		Glade.XML gladeXML;
		gladeXML = Glade.XML.FromAssembly (Util.GetGladePath() + "repair_sub_event.glade", "repair_sub_event", "chronojump");
		gladeXML.Autoconnect(this);
		*/
		Gtk.Builder builder = new Gtk.Builder (null, Util.GetGladePath () + "repair_sub_event.glade", null);
		connectWidgets (builder);
		builder.Autoconnect (this);
		
		//put an icon to window
		UtilGtk.IconWindow(repair_sub_event);

		repair_sub_event.Parent = parent;
		this.myPulse = myPulse;
	
		repair_sub_event.Title = Catalog.GetString("Repair pulse");
		label_header.Text = Constants.GetRepairWindowMessage ();
	
		
		pulseType = SqlitePulseType.SelectAndReturnPulseType(myPulse.Type);
		
		TextBuffer tb = new TextBuffer (new TextTagTable());
		tb.Text = createTextForTextView(pulseType);
		textview1.Buffer = tb;
		
		createTreeView(treeview_subevents);
		//count, time
		store = new TreeStore(typeof (string), typeof (string));
		treeview_subevents.Model = store;
		fillTreeView (treeview_subevents, store, myPulse, pDN);
	
		button_add_before.Sensitive = false;
		button_add_after.Sensitive = false;
		button_delete.Sensitive = false;
		
		label_totaltime_value.Text = getTotalTime().ToString() + " " + Catalog.GetString("seconds");
		
		treeview_subevents.Selection.Changed += onSelectionEntry;
	}
	
	static public RepairPulseWindow Show (Gtk.Window parent, Pulse myPulse, int pDN)
	{
		if (RepairPulseWindowBox == null) {
			RepairPulseWindowBox = new RepairPulseWindow (parent, myPulse, pDN);
		}

		//manage window color
		if(! Config.UseSystemColor)
		{
			UtilGtk.WindowColor(RepairPulseWindowBox.repair_sub_event, Config.ColorBackground);
			UtilGtk.ContrastLabelsLabel(Config.ColorBackgroundIsDark, RepairPulseWindowBox.label_header);
			UtilGtk.ContrastLabelsHBox(Config.ColorBackgroundIsDark, RepairPulseWindowBox.hbox_notes_and_totaltime);
		}

		RepairPulseWindowBox.repair_sub_event.Show ();

		return RepairPulseWindowBox;
	}
	
	private string createTextForTextView (PulseType myPulseType) {
		string pulseTypeString = string.Format(Catalog.GetString(
					"PulseType: {0}."), myPulseType.Name);

		string fixedString = "";


		/* 
		 * currently all pulseTypes are non fixed, and it's not possible to create more types (by user), then there are no limitations
		 */

		return pulseTypeString + fixedString;
	}

	
	private void createTreeView (Gtk.TreeView myTreeView) {
		myTreeView.HeadersVisible=true;
		int count = 0;

		myTreeView.AppendColumn ( Catalog.GetString ("Count"), new CellRendererText(), "text", count++);
		//myTreeView.AppendColumn ( Catalog.GetString ("Time"), new CellRendererText(), "text", count++);

		Gtk.TreeViewColumn timeColumn = new Gtk.TreeViewColumn ();
		timeColumn.Title = Catalog.GetString("Time");
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
		if(Util.IsNumber(args.NewText, true)) {
			/* 
			 * currently all pulseTypes are non fixed, and it's not possible to create more types (by user), then there are no limitations
			 */
			
			/*
			//if it's limited by fixed value of seconds
			//and new seconds are bigger than allowed, return
			if(runType.FixedValue > 0 && ! runType.TracksLimited &&
					getTotalTime() //current total time in treeview
					- Convert.ToDouble((string) treeview_subevents.Model.GetValue(iter,1)) //-old cell
					+ Convert.ToDouble(args.NewText) //+new cell
					> runType.FixedValue) {	//bigger than allowed
				return;
			} else {
			*/
				store.SetValue(iter, 1, args.NewText);

				//update the totaltime label
				label_totaltime_value.Text = getTotalTime().ToString() + " " + Catalog.GetString("seconds");
			/*
			}
			*/
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
	
	private void fillTreeView (Gtk.TreeView time, TreeStore store, Pulse myPulse, int pDN)
	{
		if(myPulse.TimesString.Length > 0) {
			string [] timeArray = myPulse.TimesString.Split(new char[] {'='});

			int count = 0;
			foreach (string myTime in timeArray) {
				store.AppendValues ( (count+1).ToString(), Util.TrimDecimals(myTime, pDN) );
				count ++;
			}
		}
	}

	void onSelectionEntry (object o, EventArgs args) {
		ITreeModel model;
		TreeIter iter;
		
		if (((TreeSelection)o).GetSelected(out model, out iter)) {
			button_add_before.Sensitive = true;
			button_add_after.Sensitive = true;
			button_delete.Sensitive = true;

			/* 
			 * currently all pulseTypes are non fixed, and it's not possible to create more types (by user), then there are no limitations
			 */
			/*
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
			*/
		}
	}

	void on_button_add_before_clicked (object o, EventArgs args) {
		ITreeModel model; 
		TreeIter iter; 
		if (treeview_subevents.Selection.GetSelected (out model, out iter)) {
			int position = Convert.ToInt32( (string) model.GetValue (iter, 0) ) -1; //count starts at '0'
			iter = store.InsertNode(position);
			store.SetValue(iter, 1, "0");
			putRowNumbers(store);
		}
	}
	
	void on_button_add_after_clicked (object o, EventArgs args) {
		ITreeModel model; 
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
		ITreeModel model; 
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
			
		//calculate other variables needed for pulse creation
		
		myPulse.TotalPulsesNum = Util.GetNumberOfJumps(timeString, false); //don't need a GetNumberOfRuns, this works
		myPulse.TimesString = timeString;

		//save it deleting the old first for having the same uniqueID
		Sqlite.Delete(false, Constants.PulseTable,myPulse.UniqueID);

		myPulse.InsertAtDB(false, Constants.PulseTable);
		/*
		SqlitePulse.Insert(myPulse.UniqueID.ToString(), 
				myPulse.PersonID, myPulse.SessionID, 
				myPulse.Type, myPulse.FixedPulse, totalPulsesNum, 
				timeString, myPulse.Description
				);
				*/

		//close the window
		RepairPulseWindowBox.repair_sub_event.Hide();
		RepairPulseWindowBox = null;
	}

	void on_button_cancel_clicked (object o, EventArgs args)
	{
		RepairPulseWindowBox.repair_sub_event.Hide();
		RepairPulseWindowBox = null;
	}
	
	void on_delete_event (object o, DeleteEventArgs args)
	{
		RepairPulseWindowBox.repair_sub_event.Hide();
		RepairPulseWindowBox = null;
	}
	
	public Button Button_accept 
	{
		set { button_accept = value;	}
		get { return button_accept;	}
	}

	private void connectWidgets (Gtk.Builder builder)
	{
		repair_sub_event = (Gtk.Window) builder.GetObject ("repair_sub_event");
		hbox_notes_and_totaltime = (Gtk.HBox) builder.GetObject ("hbox_notes_and_totaltime");
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

