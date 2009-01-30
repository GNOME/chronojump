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
 */

using System;
using Gtk;
using Glade;
using System.Text; //StringBuilder
using System.Collections; //ArrayList

using System.Threading;
using Mono.Unix;


//--------------------------------------------------------
//---------------- EDIT JUMP WIDGET ----------------------
//--------------------------------------------------------

public class EditJumpWindow : EditEventWindow
{
	static EditJumpWindow EditJumpWindowBox;
//	protected bool weightPercentPreferred;
	protected double personWeight;
	protected int sessionID; //for know weight specific to this session

	//for inheritance
	protected EditJumpWindow () {
	}

	public EditJumpWindow (Gtk.Window parent) {
		Glade.XML gladeXML;
		gladeXML = Glade.XML.FromAssembly (Util.GetGladePath() + "chronojump.glade", "edit_event", null);
		gladeXML.Autoconnect(this);
		this.parent =  parent;

		//put an icon to window
		UtilGtk.IconWindow(edit_event);
	
		eventBigTypeString = Catalog.GetString("jump");
	}

	static new public EditJumpWindow Show (Gtk.Window parent, Event myEvent, bool weightPercentPreferred, int pDN)
	{
		if (EditJumpWindowBox == null) {
			EditJumpWindowBox = new EditJumpWindow (parent);
		}	

		EditJumpWindowBox.weightPercentPreferred = weightPercentPreferred;
		EditJumpWindowBox.personWeight = SqlitePersonSession.SelectPersonWeight(
				Convert.ToInt32(myEvent.PersonID),
				Convert.ToInt32(myEvent.SessionID)); 

		EditJumpWindowBox.pDN = pDN;
		
		EditJumpWindowBox.sessionID = myEvent.SessionID;

		EditJumpWindowBox.initializeValues();

		EditJumpWindowBox.fillDialog (myEvent);
		
		EditJumpWindowBox.edit_event.Show ();

		return EditJumpWindowBox;
	}
	
	protected override void initializeValues () {
		showTv = true;
		showTc= true;
		showFall = true;
		showDistance = false;
		showTime = false;
		showSpeed = false;
		showWeight = true;
		showLimited = false;
		showAngle = true;
		
		if(weightPercentPreferred)
			label_weight_title.Text = label_weight_title.Text.ToString() + " %";
		else
			label_weight_title.Text = label_weight_title.Text.ToString() + " Kg";

		Log.WriteLine(string.Format("-------------{0}", personWeight));
	}

	protected override string [] findTypes(Event myEvent) {
		Jump myJump = (Jump) myEvent;
		string [] myTypes;
		if (myJump.TypeHasFall) {
			myTypes = SqliteJumpType.SelectJumpTypes("", "TC", true); //don't show allJumpsName row, TC jumps, only select name
		} else {
			myTypes = SqliteJumpType.SelectJumpTypes("", "nonTC", true); //don't show allJumpsName row, nonTC jumps, only select name
		}
		return myTypes;
	}

	protected override void fillTc (Event myEvent) {
		//on normal jumps fills Tc and Fall
		Jump myJump = (Jump) myEvent;

		if (myJump.TypeHasFall) {
			entryTc = myJump.Tc.ToString();
			
			//show all the decimals for not triming there in edit window using
			//(and having different values in formulae like GetHeightInCm ...)
			//entry_tc_value.Text = Util.TrimDecimals(entryTc, pDN);
			entry_tc_value.Text = entryTc;
			
			entryFall = myJump.Fall.ToString();
			entry_fall_value.Text = entryFall;
			entry_tc_value.Sensitive = true;
			entry_fall_value.Sensitive = true;
		} else {
			entry_tc_value.Sensitive = false;
			entry_fall_value.Sensitive = false;
		}
	}

	protected override void fillWeight(Event myEvent) {
		Jump myJump = (Jump) myEvent;
		if(myJump.TypeHasWeight) {
			if(weightPercentPreferred)
				entryWeight = myJump.Weight.ToString();
			else
				entryWeight = Util.WeightFromPercentToKg(myJump.Weight, personWeight).ToString();

			entry_weight_value.Text = entryWeight;
			entry_weight_value.Sensitive = true;
		} else {
			entry_weight_value.Sensitive = false;
		}
	}
	
	protected override void fillAngle(Event myEvent) {
		Jump myJump = (Jump) myEvent;
		
		//default values are -1.0 or -1 (old int)
		if(myJump.Angle < 0) { 
			entryAngle = "-1,0";
			entry_angle_value.Text = "-";
		} else {
			entryAngle = myJump.Angle.ToString();
			entry_angle_value.Text = entryAngle;
		}
	}

	
	protected override void createSignal() {
		//only for jumps & runs
		combo_eventType.Changed += new EventHandler (on_combo_eventType_changed);
	}

	string weightOldStore = "0";
	private void on_combo_eventType_changed (object o, EventArgs args) {
		//if the distance of the new runType is fixed, put this distance
		//if not conserve the old
		JumpType myJumpType = new JumpType (UtilGtk.ComboGetActive(combo_eventType));
		if(myJumpType.HasWeight) {
			if(weightOldStore != "0")
				entry_weight_value.Text = weightOldStore;

			entry_weight_value.Sensitive = true;
		} else {
			//store weight in a variable if needed
			if(entry_weight_value.Text != "0")
				weightOldStore = entry_weight_value.Text;

			entry_weight_value.Text = "0";
			entry_weight_value.Sensitive = false;
		}
	}


	protected override void on_button_cancel_clicked (object o, EventArgs args)
	{
		EditJumpWindowBox.edit_event.Hide();
		EditJumpWindowBox = null;
	}
	
	protected override void on_delete_event (object o, DeleteEventArgs args)
	{
		EditJumpWindowBox.edit_event.Hide();
		EditJumpWindowBox = null;
	}
	
	protected override void hideWindow() {
		EditJumpWindowBox.edit_event.Hide();
		EditJumpWindowBox = null;
	}
	
	protected override void updateEvent(int eventID, int personID, string description) {
		//only for jump
		double jumpPercentWeightForNewPerson = updateWeight(personID, sessionID);
		
		SqliteJump.Update(eventID, UtilGtk.ComboGetActive(combo_eventType), entryTv, entryTc, entryFall, personID, jumpPercentWeightForNewPerson, description, Convert.ToDouble(entryAngle));
	}

	
	protected virtual double updateWeight(int personID, int mySessionID) {
		//only for jumps, jumpsRj
		//update the weight percent of jump if needed
		double jumpPercentWeightForNewPerson = 0;
		if(entryWeight != "0") {
			double oldPersonWeight = personWeight;

			double jumpWeightInKg = 0;
			if(weightPercentPreferred)
				jumpWeightInKg = Util.WeightFromPercentToKg(Convert.ToDouble(entryWeight), oldPersonWeight);
			else
				jumpWeightInKg = Convert.ToDouble(entryWeight);
			
			double newPersonWeight = SqlitePersonSession.SelectPersonWeight(personID, mySessionID); 
			//jumpPercentWeightForNewPerson = jumpWeightInKg * 100 / newPersonWeight; 
			jumpPercentWeightForNewPerson = Util.WeightFromKgToPercent(jumpWeightInKg, newPersonWeight); 
			Log.WriteLine(string.Format("oldPW: {0}, jWinKg {1}, newPW{2}, jWin%NewP{3}",
					oldPersonWeight, jumpWeightInKg, newPersonWeight, jumpPercentWeightForNewPerson));
		}

		return jumpPercentWeightForNewPerson;
	}
	

}

//--------------------------------------------------------
//---------------- EDIT JUMP RJ WIDGET -------------------
//--------------------------------------------------------

public class EditJumpRjWindow : EditJumpWindow
{
	static EditJumpRjWindow EditJumpRjWindowBox;

	EditJumpRjWindow (Gtk.Window parent) {
		Glade.XML gladeXML;
		gladeXML = Glade.XML.FromAssembly (Util.GetGladePath() + "chronojump.glade", "edit_event", null);
		gladeXML.Autoconnect(this);
		this.parent = parent;
		
		//put an icon to window
		UtilGtk.IconWindow(edit_event);
	
		eventBigTypeString = Catalog.GetString("reactive jump");
	}

	static new public EditJumpRjWindow Show (Gtk.Window parent, Event myEvent, bool weightPercentPreferred, int pDN)
	{
		if (EditJumpRjWindowBox == null) {
			EditJumpRjWindowBox = new EditJumpRjWindow (parent);
		}

		EditJumpRjWindowBox.weightPercentPreferred = weightPercentPreferred;
		EditJumpRjWindowBox.personWeight = SqlitePersonSession.SelectPersonWeight(myEvent.PersonID, myEvent.SessionID); 

		EditJumpRjWindowBox.pDN = pDN;
		
		EditJumpRjWindowBox.sessionID = myEvent.SessionID;

		EditJumpRjWindowBox.initializeValues();

		EditJumpRjWindowBox.fillDialog (myEvent);
		
		EditJumpRjWindowBox.edit_event.Show ();

		return EditJumpRjWindowBox;
	}
	
	protected override void initializeValues () {
		showTv = false;
		showTc = false;
		showFall = true;
		showDistance = false;
		showTime = false;
		showSpeed = false;
		showWeight = true;
		showLimited = true;
		
		if(weightPercentPreferred)
			label_weight_title.Text = label_weight_title.Text.ToString() + " %";
		else
			label_weight_title.Text = label_weight_title.Text.ToString() + " Kg";
	}

	protected override string [] findTypes(Event myEvent) {
		//type cannot change on jumpRj
		combo_eventType.Sensitive=false;

		string [] myTypes;
		myTypes = SqliteJumpType.SelectJumpRjTypes("", true); //don't show allJumpsName row, only select name
		return myTypes;
	}

	protected override void fillWeight(Event myEvent) {
		JumpRj myJump = (JumpRj) myEvent;
		if(myJump.TypeHasWeight) {
			if(weightPercentPreferred)
				entryWeight = myJump.Weight.ToString();
			else
				entryWeight = Util.WeightFromPercentToKg(myJump.Weight, personWeight).ToString();

			entry_weight_value.Text = entryWeight;
			entry_weight_value.Sensitive = true;
		} else {
			entry_weight_value.Sensitive = false;
		}
	}

	protected override void fillFall(Event myEvent) {
		JumpRj myJump = (JumpRj) myEvent;
		entryFall = myJump.Fall.ToString();
		entry_fall_value.Text = entryFall;
	}

	protected override void fillLimited(Event myEvent) {
		JumpRj myJumpRj = (JumpRj) myEvent;
		label_limited_value.Text = Util.GetLimitedRounded(myJumpRj.Limited, pDN);
	}


	protected override void on_button_cancel_clicked (object o, EventArgs args)
	{
		EditJumpRjWindowBox.edit_event.Hide();
		EditJumpRjWindowBox = null;
	}
	
	protected override void on_delete_event (object o, DeleteEventArgs args)
	{
		EditJumpRjWindowBox.edit_event.Hide();
		EditJumpRjWindowBox = null;
	}
	
	protected override void hideWindow() {
		EditJumpRjWindowBox.edit_event.Hide();
		EditJumpRjWindowBox = null;
	}
	
	protected override void updateEvent(int eventID, int personID, string description) {
		//only for jumps
		double jumpPercentWeightForNewPerson = updateWeight(personID, sessionID);
		
		SqliteJumpRj.Update(eventID, personID, entryFall, jumpPercentWeightForNewPerson, description);
	}
}


//--------------------------------------------------------
//---------------- Repair jumpRJ WIDGET ------------------
//--------------------------------------------------------

public class RepairJumpRjWindow 
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

	static RepairJumpRjWindow RepairJumpRjWindowBox;
	Gtk.Window parent;
	//int pDN;

	JumpType jumpType;
	JumpRj jumpRj; //used on button_accept
	

	RepairJumpRjWindow (Gtk.Window parent, JumpRj myJump, int pDN) {
		Glade.XML gladeXML;
		gladeXML = Glade.XML.FromAssembly (Util.GetGladePath() + "chronojump.glade", "repair_sub_event", null);
		gladeXML.Autoconnect(this);
	
		//put an icon to window
		UtilGtk.IconWindow(repair_sub_event);
	
		this.parent = parent;
		this.jumpRj = myJump;

		//this.pDN = pDN;
	
		repair_sub_event.Title = Catalog.GetString("Repair reactive jump");
		
		System.Globalization.NumberFormatInfo localeInfo = new System.Globalization.NumberFormatInfo();
		localeInfo = System.Globalization.NumberFormatInfo.CurrentInfo;
		label_header.Text = string.Format(Catalog.GetString("Use this window to repair this test.\nDouble clic any cell to edit it (decimal separator: '{0}')"), localeInfo.NumberDecimalSeparator);
	
		
		jumpType = SqliteJumpType.SelectAndReturnJumpRjType(myJump.Type);
		
		TextBuffer tb = new TextBuffer (new TextTagTable());
		tb.Text = createTextForTextView(jumpType);
		textview1.Buffer = tb;
		
		createTreeView(treeview_subevents);
		//count, tc, tv
		store = new TreeStore(typeof (string), typeof (string), typeof(string));
		treeview_subevents.Model = store;
		fillTreeView (treeview_subevents, store, myJump, pDN);
	
		button_add_before.Sensitive = false;
		button_add_after.Sensitive = false;
		button_delete.Sensitive = false;
		
		label_totaltime_value.Text = getTotalTime().ToString() + " " + Catalog.GetString("seconds");
		
		treeview_subevents.Selection.Changed += onSelectionEntry;
	}
	
	static public RepairJumpRjWindow Show (Gtk.Window parent, JumpRj myJump, int pDN)
	{
		//Log.WriteLine(myJump);
		if (RepairJumpRjWindowBox == null) {
			RepairJumpRjWindowBox = new RepairJumpRjWindow (parent, myJump, pDN);
		}
		
		RepairJumpRjWindowBox.repair_sub_event.Show ();

		return RepairJumpRjWindowBox;
	}
	
	private string createTextForTextView (JumpType myJumpType) {
		string jumpTypeString = string.Format(Catalog.GetString(
					"JumpType: {0}."), myJumpType.Name);

		//if it's a jump type that starts in, then don't allow first TC be different than -1
		string startString = "";
		if(myJumpType.StartIn) {
			startString = string.Format(Catalog.GetString("\nThis jump type starts inside, the first time should be a flight time."));
		}

		string fixedString = "";
		if(myJumpType.FixedValue > 0) {
			if(myJumpType.JumpsLimited) {
				//if it's a jump type jumpsLimited with a fixed value, then don't allow the creation of more jumps, and respect the -1 at last TF if found
				fixedString = string.Format(Catalog.GetString("\nThis jump type is fixed to {0} jumps, you cannot add more."), myJumpType.FixedValue);
			} else {
				//if it's a jump type timeLimited with a fixed value, then complain when the total time is higher
				fixedString = string.Format(Catalog.GetString("\nThis jump type is fixed to {0} seconds, totaltime cannot be greater."), myJumpType.FixedValue);
			}
		}
		return jumpTypeString + startString + fixedString;
	}

	
	private void createTreeView (Gtk.TreeView myTreeView) {
		myTreeView.HeadersVisible=true;
		int count = 0;

		myTreeView.AppendColumn ( Catalog.GetString ("Count"), new CellRendererText(), "text", count++);

		Gtk.TreeViewColumn tcColumn = new Gtk.TreeViewColumn ();
		tcColumn.Title = Catalog.GetString("TC");
		Gtk.CellRendererText tcCell = new Gtk.CellRendererText ();
		tcCell.Editable = true;
		tcCell.Edited += tcCellEdited;
		tcColumn.PackStart (tcCell, true);
		tcColumn.AddAttribute(tcCell, "text", count ++);
		myTreeView.AppendColumn ( tcColumn );
		
		Gtk.TreeViewColumn tvColumn = new Gtk.TreeViewColumn ();
		tvColumn.Title = Catalog.GetString("TF");
		Gtk.CellRendererText tvCell = new Gtk.CellRendererText ();
		tvCell.Editable = true;
		tvCell.Edited += tvCellEdited;
		tvColumn.PackStart (tvCell, true);
		tvColumn.AddAttribute(tvCell, "text", count ++);
		myTreeView.AppendColumn ( tvColumn );
	}
	
	private void tcCellEdited (object o, Gtk.EditedArgs args)
	{
		Gtk.TreeIter iter;
		store.GetIter (out iter, new Gtk.TreePath (args.Path));
		if(Util.IsNumber(args.NewText, true) && (string) treeview_subevents.Model.GetValue(iter,1) != "-1") {
			//if it's limited by fixed value of seconds
			//and new seconds are bigger than allowed, return
			if(jumpType.FixedValue > 0 && ! jumpType.JumpsLimited &&
					getTotalTime() //current total time in treeview
					- Convert.ToDouble((string) treeview_subevents.Model.GetValue(iter,1)) //-old cell
					+ Convert.ToDouble(args.NewText) //+new cell
					> jumpType.FixedValue) {	//bigger than allowed
				return;
			} else {
				store.SetValue(iter, 1, args.NewText);

				//update the totaltime label
				label_totaltime_value.Text = getTotalTime().ToString() + " " + Catalog.GetString("seconds");
			}
		}
		
		//if is not number or if it was -1, the old data will remain
	}

	private void tvCellEdited (object o, Gtk.EditedArgs args)
	{
		Gtk.TreeIter iter;
		store.GetIter (out iter, new Gtk.TreePath (args.Path));
		if(Util.IsNumber(args.NewText, true) && (string) treeview_subevents.Model.GetValue(iter,2) != "-1") {
			//if it's limited by fixed value of seconds
			//and new seconds are bigger than allowed, return
			if(jumpType.FixedValue > 0 && ! jumpType.JumpsLimited &&
					getTotalTime() //current total time in treeview
					- Convert.ToDouble((string) treeview_subevents.Model.GetValue(iter,2)) //-old cell
					+ Convert.ToDouble(args.NewText) //+new cell
					> jumpType.FixedValue) {	//bigger than allowed
				return;
			} else {
				store.SetValue(iter, 2, args.NewText);
				
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
		string stringTc = "";
		string stringTv = "";
		if(iterOk) {
			do {
				//be cautious because when there's no value (like first tc in a jump that starts in,
				//it's stored as "-1", but it's shown as "-"
				stringTc = (string) treeview_subevents.Model.GetValue(myIter, 1);
				if(stringTc != "-" && stringTc != "-1") 
					totalTime += Convert.ToDouble(stringTc);
				
				stringTv = (string) treeview_subevents.Model.GetValue(myIter, 2);
				if(stringTv != "-" && stringTv != "-1") 
					totalTime += Convert.ToDouble(stringTv);
				
			} while (store.IterNext (ref myIter));
		}
		return totalTime;
	}
	
	private void fillTreeView (Gtk.TreeView tv, TreeStore store, JumpRj myJump, int pDN)
	{
		if(myJump.TcString.Length > 0 && myJump.TvString.Length > 0) {
			string [] tcArray = myJump.TcString.Split(new char[] {'='});
			string [] tvArray = myJump.TvString.Split(new char[] {'='});

			int count = 0;
			foreach (string myTc in tcArray) {
				string myTv;
				if(tvArray.Length >= count)
					myTv = Util.TrimDecimals(tvArray[count], pDN);
				else
					myTv = "";
				
				store.AppendValues ( (count+1).ToString(), Util.TrimDecimals(myTc, pDN), myTv );
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

			//don't allow to add a row before the first if first row has a -1 in 'TC'
			//also don't allow deleting
			if((string) model.GetValue (iter, 1) == "-1") {
				button_add_before.Sensitive = false;
				button_delete.Sensitive = false;
			}

			//don't allow to add a row after the last if it has a -1
			//also don't allow deleting
			//the only -1 in flight time can be in the last row
			if((string) model.GetValue (iter, 2) == "-1") {
				button_add_after.Sensitive = false;
				button_delete.Sensitive = false;
			}
			
			//don't allow to add a row before or after 
			//if the jump type is fixed to n jumps and we reached n
			if(jumpType.FixedValue > 0 && jumpType.JumpsLimited) {
				int lastRow = 0;
				do {
					lastRow = Convert.ToInt32 ((string) model.GetValue (iter, 0));
				} while (store.IterNext (ref iter));

				//don't allow if max rows reached
				if(lastRow == jumpType.FixedValue ||
						( lastRow == jumpType.FixedValue +1 && jumpType.StartIn) ) {
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
			store.SetValue(iter, 2, "0");
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
			store.SetValue(iter, 2, "0");
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
		//foreach all lines... extrac tcString and tvString
		TreeIter myIter;
		string tcString = "";
		string tvString = "";
		
		bool iterOk = store.GetIterFirst (out myIter);
		if(iterOk) {
			string equal= ""; //first iteration should not appear '='
			do {
				tcString = tcString + equal + (string) treeview_subevents.Model.GetValue (myIter, 1);
				tvString = tvString + equal + (string) treeview_subevents.Model.GetValue (myIter, 2);
				equal = "=";
			} while (store.IterNext (ref myIter));
		}
		
		jumpRj.TvString = tvString;	
		jumpRj.TcString = tcString;	
		jumpRj.Jumps = Util.GetNumberOfJumps(tvString, false);
		jumpRj.Time = Util.GetTotalTime(tcString, tvString);

		//calculate other variables needed for jumpRj creation
		
		if(jumpType.FixedValue > 0) {
			//if this jumpType has a fixed value of jumps or time, limitstring has not changed
			if(jumpType.JumpsLimited) {
				jumpRj.Limited = jumpType.FixedValue.ToString() + "J";
			} else {
				jumpRj.Limited = jumpType.FixedValue.ToString() + "T";
			}
		} else {
			//else limitstring should be calculated
			if(jumpType.JumpsLimited) {
				jumpRj.Limited = jumpRj.Jumps.ToString() + "J";
			} else {
				jumpRj.Limited = Util.GetTotalTime(tcString, tvString) + "T";
			}
		}

		//save it deleting the old first for having the same uniqueID
		SqliteJump.Delete("jumpRj", jumpRj.UniqueID.ToString());
		jumpRj.InsertAtDB(false, Constants.JumpRjTable); 
		/*
		SqliteJump.InsertRj("jumpRj", jumpRj.UniqueID.ToString(), jumpRj.PersonID, jumpRj.SessionID, 
				jumpRj.Type, Util.GetMax(tvString), Util.GetMax(tcString), 
				jumpRj.Fall, jumpRj.Weight, jumpRj.Description,
				Util.GetAverage(tvString), Util.GetAverage(tcString),
				tvString, tcString,
				jumps, Util.GetTotalTime(tcString, tvString), limitString
				);
				*/

		//close the window
		RepairJumpRjWindowBox.repair_sub_event.Hide();
		RepairJumpRjWindowBox = null;
	}

	void on_button_cancel_clicked (object o, EventArgs args)
	{
		RepairJumpRjWindowBox.repair_sub_event.Hide();
		RepairJumpRjWindowBox = null;
	}
	
	void on_delete_event (object o, DeleteEventArgs args)
	{
		RepairJumpRjWindowBox.repair_sub_event.Hide();
		RepairJumpRjWindowBox = null;
	}
	
	public Button Button_accept 
	{
		set { button_accept = value;	}
		get { return button_accept;	}
	}

}

//--------------------------------------------------------
//---------------- jump extra WIDGET --------------------
//--------------------------------------------------------

public class JumpExtraWindow 
{
	[Widget] Gtk.Window jump_extra;
	[Widget] Gtk.Label label_limit;
	[Widget] Gtk.SpinButton spinbutton_limit;
	[Widget] Gtk.Label label_limit_units;
	[Widget] Gtk.SpinButton spinbutton_weight;
	[Widget] Gtk.SpinButton spinbutton_fall;
	[Widget] Gtk.Button button_accept;
	[Widget] Gtk.RadioButton radiobutton_kg;
	[Widget] Gtk.RadioButton radiobutton_weight;
	[Widget] Gtk.Label label_weight;
	[Widget] Gtk.Label label_fall;
	[Widget] Gtk.Label label_cm;

	static string option = "Kg";
	static double limited = 10;
	static bool jumpsLimited;
	static int weight = 20;
	static int fall = 20;
	
	static JumpExtraWindow JumpExtraWindowBox;
	Gtk.Window parent;

	JumpExtraWindow (Gtk.Window parent) {
		Glade.XML gladeXML;
		gladeXML = Glade.XML.FromAssembly (Util.GetGladePath() + "chronojump.glade", "jump_extra", null);
		gladeXML.Autoconnect(this);
		this.parent = parent;
		
		//put an icon to window
		UtilGtk.IconWindow(jump_extra);
	}
	
	static public JumpExtraWindow Show (Gtk.Window parent, JumpType myJumpType) 
	{
		if (JumpExtraWindowBox == null) {
			JumpExtraWindowBox = new JumpExtraWindow (parent);
		}
		
		if(myJumpType.IsRepetitive) {
			string jumpsName = Catalog.GetString("jumps");
			string secondsName = Catalog.GetString("seconds");
			if(myJumpType.JumpsLimited) {
				jumpsLimited = true;
				JumpExtraWindowBox.label_limit_units.Text = jumpsName;
			} else {
				jumpsLimited = false;
				JumpExtraWindowBox.label_limit_units.Text = secondsName;
			}
			if(myJumpType.FixedValue > 0) {
				JumpExtraWindowBox.spinbutton_limit.Sensitive = false;
				JumpExtraWindowBox.spinbutton_limit.Value = myJumpType.FixedValue;
			} else
				JumpExtraWindowBox.spinbutton_limit.Value = limited;
		} else {
			hideRepetitiveData();	
		}
		if(! myJumpType.HasWeight) {
			hideWeightData();	
		}
		if(myJumpType.StartIn) {
			hideFallData();	
		}
		
		JumpExtraWindowBox.spinbutton_weight.Value = weight;
		JumpExtraWindowBox.spinbutton_fall.Value = fall;
		if (option == "Kg") {
			JumpExtraWindowBox.radiobutton_kg.Active = true;
		} else {
			JumpExtraWindowBox.radiobutton_weight.Active = true;
		}
		
		JumpExtraWindowBox.jump_extra.Show ();

		return JumpExtraWindowBox;
	}
	
	static void hideRepetitiveData () {
		JumpExtraWindowBox.label_limit.Sensitive = false;
		JumpExtraWindowBox.spinbutton_limit.Sensitive = false;
		JumpExtraWindowBox.label_limit_units.Sensitive = false;
	}
	
	static void hideWeightData () {
		JumpExtraWindowBox.label_weight.Sensitive = false;
		JumpExtraWindowBox.spinbutton_weight.Sensitive = false;
		JumpExtraWindowBox.radiobutton_kg.Sensitive = false;
		JumpExtraWindowBox.radiobutton_weight.Sensitive = false;
	}
	
	static void hideFallData () {
		JumpExtraWindowBox.label_fall.Sensitive = false;
		JumpExtraWindowBox.spinbutton_fall.Sensitive = false;
		JumpExtraWindowBox.label_cm.Sensitive = false;
	}
	
	void on_button_cancel_clicked (object o, EventArgs args)
	{
		JumpExtraWindowBox.jump_extra.Hide();
		JumpExtraWindowBox = null;
	}
	
	void on_jump_extra_delete_event (object o, DeleteEventArgs args)
	{
		JumpExtraWindowBox.jump_extra.Hide();
		JumpExtraWindowBox = null;
	}
	
	void on_button_accept_clicked (object o, EventArgs args)
	{
		limited = (double) spinbutton_limit.Value;
		weight = (int) spinbutton_weight.Value;
		fall = (int) spinbutton_fall.Value;
		
		JumpExtraWindowBox.jump_extra.Hide();
		JumpExtraWindowBox = null;
	}

	void on_radiobutton_kg_toggled (object o, EventArgs args)
	{
		option = "Kg";
		Log.WriteLine(string.Format("option: {0}", option));
	}
	
	void on_radiobutton_weight_toggled (object o, EventArgs args)
	{
		option = "%";
		Log.WriteLine(string.Format("option: {0}", option));
	}

	public Button Button_accept 
	{
		set { button_accept = value;	}
		get { return button_accept;	}
	}

	public string Option 
	{
		get { return option;	}
	}

	public bool JumpsLimited 
	{
		get { return jumpsLimited;	}
	}
	
	public double Limited 
	{
		get { return limited;	}
	}
	
	public string LimitString
	{
		get { 
			if(jumpsLimited) {
				return limited.ToString() + "J";
			} else {
				return Limited.ToString() + "T";
			}
		}
	}
	
	public int Weight 
	{
		get { return weight;	}
	}
	
	public int Fall 
	{
		get { return fall;	}
	}
}


//--------------------------------------------------------
//---------------- jumps_more widget ---------------------
//--------------------------------------------------------

public class JumpsMoreWindow : EventMoreWindow
{
	[Widget] Gtk.Window jumps_runs_more;
	static JumpsMoreWindow JumpsMoreWindowBox;
	private bool selectedStartIn;
	private bool selectedExtraWeight;

	public JumpsMoreWindow (Gtk.Window parent) {
		Glade.XML gladeXML;
		gladeXML = Glade.XML.FromAssembly (Util.GetGladePath() + "chronojump.glade", "jumps_runs_more", null);
		gladeXML.Autoconnect(this);
		this.parent = parent;
		
		//put an icon to window
		UtilGtk.IconWindow(jumps_runs_more);
		
		selectedEventType = EventType.Types.JUMP.ToString();
		
		//name, startIn, weight, description
		store = new TreeStore(typeof (string), typeof (string), typeof (string), typeof (string));

		initializeThings();
	}
	
	static public JumpsMoreWindow Show (Gtk.Window parent)
	{
		if (JumpsMoreWindowBox == null) {
			JumpsMoreWindowBox = new JumpsMoreWindow (parent);
		}
		JumpsMoreWindowBox.jumps_runs_more.Show ();
		
		return JumpsMoreWindowBox;
	}
	
	protected override void createTreeView (Gtk.TreeView tv) {
		tv.HeadersVisible=true;
		int count = 0;
		
		tv.AppendColumn ( Catalog.GetString ("Name"), new CellRendererText(), "text", count++);
		tv.AppendColumn ( Catalog.GetString ("Start inside"), new CellRendererText(), "text", count++);
		tv.AppendColumn ( Catalog.GetString ("Extra weight"), new CellRendererText(), "text", count++);
		tv.AppendColumn ( Catalog.GetString ("Description"), new CellRendererText(), "text", count++);
	}
	
	protected override void fillTreeView (Gtk.TreeView tv, TreeStore store) 
	{
		//select data without inserting an "all jumps", without filter, and not obtain only name of jump
		string [] myJumpTypes = SqliteJumpType.SelectJumpTypes("", "", false);
		foreach (string myType in myJumpTypes) {
			string [] myStringFull = myType.Split(new char[] {':'});
			if(myStringFull[2] == "1") {
				myStringFull[2] = Catalog.GetString("Yes");
			} else {
				myStringFull[2] = Catalog.GetString("No");
			}
			if(myStringFull[3] == "1") {
				myStringFull[3] = Catalog.GetString("Yes");
			} else {
				myStringFull[3] = Catalog.GetString("No");
			}

			JumpType tempType = new JumpType (myStringFull[1]);
			string description  = getDescriptionLocalised(tempType, myStringFull[4]);


			store.AppendValues (
					//myStringFull[0], //don't display de uniqueID
					myStringFull[1],	//name 
					myStringFull[2], 	//startIn
					myStringFull[3], 	//weight
					description
					);
		}	
	}

	protected override void onSelectionEntry (object o, EventArgs args) {
		TreeModel model;
		TreeIter iter;
		selectedEventName = "-1";
		selectedStartIn = false;
		selectedExtraWeight = false;
		selectedDescription = "";

		if (((TreeSelection)o).GetSelected(out model, out iter)) {
			selectedEventName = (string) model.GetValue (iter, 0);
			if( (string) model.GetValue (iter, 1) == Catalog.GetString("Yes") ) {
				selectedStartIn = true;
			}
			if( (string) model.GetValue (iter, 2) == Catalog.GetString("Yes") ) {
				selectedExtraWeight = true;
			}
			selectedDescription = (string) model.GetValue (iter, 3);

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
			if( (string) model.GetValue (iter, 1) == Catalog.GetString("Yes") ) {
				selectedStartIn = true;
			}
			if( (string) model.GetValue (iter, 2) == Catalog.GetString("Yes") ) {
				selectedExtraWeight = true;
			}
			selectedDescription = (string) model.GetValue (iter, 3);

			//activate on_button_accept_clicked()
			button_accept.Activate();
		}
	}
	
	void on_button_cancel_clicked (object o, EventArgs args)
	{
		JumpsMoreWindowBox.jumps_runs_more.Hide();
		JumpsMoreWindowBox = null;
	}
	
	void on_jumps_runs_more_delete_event (object o, DeleteEventArgs args)
	{
		JumpsMoreWindowBox.jumps_runs_more.Hide();
		JumpsMoreWindowBox = null;
	}
	
	void on_button_accept_clicked (object o, EventArgs args)
	{
		JumpsMoreWindowBox.jumps_runs_more.Hide();
	}
	
	//when a jump is done using jumpsMoreWindow, the accept doesn't destroy this instance, because 
	//later we need data from it.
	//This is used for destroying, then if a new jump type is added, it will be shown at first time clicking "more" button
	public void Destroy() {		
		JumpsMoreWindowBox = null;
	}
	
	public bool SelectedStartIn 
	{
		get {
			return selectedStartIn;
		}
	}
	
	public bool SelectedExtraWeight 
	{
		get {
			return selectedExtraWeight;
		}
	}
}

//--------------------------------------------------------
//---------------- jumps_rj_more widget ------------------
//--------------------------------------------------------

public class JumpsRjMoreWindow : EventMoreWindow 
{
	[Widget] Gtk.Window jumps_runs_more;
	static JumpsRjMoreWindow JumpsRjMoreWindowBox;
	
	private bool selectedStartIn;
	private bool selectedExtraWeight;
	private bool selectedLimited;
	private double selectedLimitedValue;
	private bool selectedUnlimited;
	
	public JumpsRjMoreWindow (Gtk.Window parent) {
		//the glade window is the same as jumps_more
		Glade.XML gladeXML;
		gladeXML = Glade.XML.FromAssembly (Util.GetGladePath() + "chronojump.glade", "jumps_runs_more", null);
		gladeXML.Autoconnect(this);
		this.parent = parent;
		
		//put an icon to window
		UtilGtk.IconWindow(jumps_runs_more);

		//if jumps_runs_more is opened to showing Rj jumpTypes make it wider
		jumps_runs_more.Resize(600,300);
		
		selectedEventType = EventType.Types.JUMP.ToString();

		//name, limited by, limited value, startIn, weight, description
		store = new TreeStore(typeof (string), typeof (string), typeof(string),
				typeof (string), typeof (string), typeof (string));
		
		initializeThings();
	}
	
	static public JumpsRjMoreWindow Show (Gtk.Window parent)
	{
		if (JumpsRjMoreWindowBox == null) {
			JumpsRjMoreWindowBox = new JumpsRjMoreWindow (parent);
		}
		JumpsRjMoreWindowBox.jumps_runs_more.Show ();
		
		return JumpsRjMoreWindowBox;
	}
	
	protected override void createTreeView (Gtk.TreeView tv) {
		tv.HeadersVisible=true;
		int count = 0;

		tv.AppendColumn ( Catalog.GetString ("Name"), new CellRendererText(), "text", count++);
		tv.AppendColumn ( Catalog.GetString ("Limited by"), new CellRendererText(), "text", count++);
		tv.AppendColumn ( Catalog.GetString ("Limited value"), new CellRendererText(), "text", count++);
		tv.AppendColumn ( Catalog.GetString ("Start inside"), new CellRendererText(), "text", count++);
		tv.AppendColumn ( Catalog.GetString ("Extra weight"), new CellRendererText(), "text", count++);
		tv.AppendColumn ( Catalog.GetString ("Description"), new CellRendererText(), "text", count++);
	}
	
	protected override void fillTreeView (Gtk.TreeView tv, TreeStore store) 
	{
		//select data without inserting an "all jumps", and not obtain only name of jump
		string [] myJumpTypes = SqliteJumpType.SelectJumpRjTypes("", false);
		foreach (string myType in myJumpTypes) {
			string [] myStringFull = myType.Split(new char[] {':'});
			if(myStringFull[2] == "1") {
				myStringFull[2] = Catalog.GetString("Yes");
			} else {
				myStringFull[2] = Catalog.GetString("No");
			}
			if(myStringFull[3] == "1") {
				myStringFull[3] = Catalog.GetString("Yes");
			} else {
				myStringFull[3] = Catalog.GetString("No");
			}
			//limited
			string myLimiter = "";
			string myLimiterValue = "";
			
			//check if it's unlimited
			if(myStringFull[5] == "-1") { //unlimited mark
				myLimiter= Catalog.GetString("Unlimited");
				myLimiterValue = "-";
			} else {
				myLimiter = Catalog.GetString("Jumps");
				if(myStringFull[4] == "0") {
					myLimiter = Catalog.GetString("Seconds");
				}
				myLimiterValue = "?";
				if(Convert.ToDouble(myStringFull[5]) > 0) {
					myLimiterValue = myStringFull[5];
				}
			}

			JumpType tempType = new JumpType (myStringFull[1]);
			string description  = getDescriptionLocalised(tempType, myStringFull[6]);

			store.AppendValues (
					//myStringFull[0], //don't display de uniqueID
					myStringFull[1],	//name 
					myLimiter,		//jumps or seconds		
					myLimiterValue,		//? or exact value
					myStringFull[2], 	//startIn
					myStringFull[3], 	//weight
					description
					);
		}	
	}

	protected override void onSelectionEntry (object o, EventArgs args)
	{
		TreeModel model;
		TreeIter iter;
		selectedEventName = "-1";
		selectedStartIn = false;
		selectedExtraWeight = false;
		selectedLimited = false;
		selectedLimitedValue = 0;
		selectedUnlimited = false; //true if it's an unlimited reactive jump
		selectedDescription = "";

		if (((TreeSelection)o).GetSelected(out model, out iter)) {
			selectedEventName = (string) model.GetValue (iter, 0);
			
			if( (string) model.GetValue (iter, 1) == Catalog.GetString("Unlimited") ) {
				selectedUnlimited = true;
				selectedLimited = true; //unlimited jumps will be limited by clicking on "finish"
							//and this will be always done by the user when
							//some jumps are done (not time like in runs)
			} 
			
			if( (string) model.GetValue (iter, 1) == Catalog.GetString("Jumps") ) {
				selectedLimited = true;
			}
			
			if( (string) model.GetValue (iter, 2) == "?" || (string) model.GetValue (iter, 2) == "-") {
				selectedLimitedValue = 0;
			} else {
				selectedLimitedValue = Convert.ToDouble( (string) model.GetValue (iter, 2) );
			}

			if( (string) model.GetValue (iter, 3) == Catalog.GetString("Yes") ) {
				selectedStartIn = true;
			}
			if( (string) model.GetValue (iter, 4) == Catalog.GetString("Yes") ) {
				selectedExtraWeight = true;
			}
			selectedDescription = (string) model.GetValue (iter, 5);

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
			
			if( (string) model.GetValue (iter, 1) == Catalog.GetString("Unlimited") ) {
				selectedUnlimited = true;
				selectedLimited = true; //unlimited jumps will be limited by clicking on "finish"
							//and this will be always done by the user when
							//some jumps are done (not time like in runs)
			} 
			
			if( (string) model.GetValue (iter, 1) == Catalog.GetString("Jumps") ) {
				selectedLimited = true;
			}
			
			if( (string) model.GetValue (iter, 2) == "?" || (string) model.GetValue (iter, 2) == "-") {
				selectedLimitedValue = 0;
			} else {
				selectedLimitedValue = Convert.ToDouble( (string) model.GetValue (iter, 2) );
			}

			if( (string) model.GetValue (iter, 3) == Catalog.GetString("Yes") ) {
				selectedStartIn = true;
			}
			if( (string) model.GetValue (iter, 4) == Catalog.GetString("Yes") ) {
				selectedExtraWeight = true;
			}
			selectedDescription = (string) model.GetValue (iter, 5);

			//activate on_button_accept_clicked()
			button_accept.Activate();
		}
	}
	
	
	void on_button_cancel_clicked (object o, EventArgs args)
	{
		JumpsRjMoreWindowBox.jumps_runs_more.Hide();
		JumpsRjMoreWindowBox = null;
	}
	
	void on_jumps_runs_more_delete_event (object o, DeleteEventArgs args)
	{
		JumpsRjMoreWindowBox.jumps_runs_more.Hide();
		JumpsRjMoreWindowBox = null;
	}
	
	void on_button_accept_clicked (object o, EventArgs args)
	{
		JumpsRjMoreWindowBox.jumps_runs_more.Hide();
	}

	//when a jump Rj is done using jumpsRjMoreWindow, the accept doesn't destroy this instance, because 
	//later we need data from it.
	//This is used for destroying, then if a new jump rj type is added, it will be shown at first time clicking "more" button
	public void Destroy() {		
		JumpsRjMoreWindowBox = null;
	}

	public bool SelectedLimited 
	{
		get { return selectedLimited; }
	}
	
	public double SelectedLimitedValue 
	{
		get { return selectedLimitedValue; }
	}
	
	public bool SelectedStartIn 
	{
		get { return selectedStartIn; }
	}
	
	public bool SelectedExtraWeight 
	{
		get { return selectedExtraWeight; }
	}
	
	public bool SelectedUnlimited 
	{
		get { return selectedUnlimited; }
	}
}
