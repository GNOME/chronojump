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
//using Gnome;
//using GLib; //for Value
using System.Text; //StringBuilder
using System.Collections; //ArrayList

using System.Threading;

//--------------------------------------------------------
//---------------- EDIT JUMP WIDGET ----------------------
//--------------------------------------------------------

public class EditJumpWindow 
{
	[Widget] Gtk.Window edit_jump;
	[Widget] Gtk.Button button_accept;
	[Widget] Gtk.Label label_header;
	[Widget] Gtk.Label label_jump_id_value;
	[Widget] Gtk.Entry entry_tv_value;
	[Widget] Gtk.Entry entry_tc_value;
	[Widget] Gtk.Entry entry_fall_value;
	[Widget] Gtk.Entry entry_weight_value;
	[Widget] Gtk.Label label_limited_title;
	[Widget] Gtk.Label label_limited_value;
	
	[Widget] Gtk.Box hbox_combo_jumpType;
	[Widget] Gtk.Combo combo_jumpType;
	[Widget] Gtk.Box hbox_combo_jumper;
	[Widget] Gtk.Combo combo_jumpers;
	
	[Widget] Gtk.TextView textview_description;

	static EditJumpWindow EditJumpWindowBox;
	Gtk.Window parent;
	int pDN;
	string type;
	string entryTv; //contains a entry that is a Number. If changed the entry as is not a number, recuperate this
	string entryTc = "0";
	string entryFall = "0"; 
	
	string entryWeight = "0"; //used for record the % for old person if we change it

	int oldPersonID; //used for record the % for old person if we change it

	EditJumpWindow (Gtk.Window parent) {
		Glade.XML gladeXML = Glade.XML.FromAssembly ("chronojump.glade", "edit_jump", null);

		gladeXML.Autoconnect(this);
		this.parent = parent;
		
		System.Globalization.NumberFormatInfo localeInfo = new System.Globalization.NumberFormatInfo();
		localeInfo = System.Globalization.NumberFormatInfo.CurrentInfo;
		label_header.Text = string.Format(Catalog.GetString("Use this window for edit a jump\n(decimal separator: '{0}')"), localeInfo.NumberDecimalSeparator);
	}
	
	static public EditJumpWindow Show (Gtk.Window parent, Jump myJump, int pDN)
	{
		if (EditJumpWindowBox == null) {
			EditJumpWindowBox = new EditJumpWindow (parent);
		}
		
		EditJumpWindowBox.pDN = pDN;
		
		EditJumpWindowBox.edit_jump.Show ();

		EditJumpWindowBox.fillDialog (myJump);

		return EditJumpWindowBox;
}
	
	private void fillDialog (Jump myJump)
	{
		label_jump_id_value.Text = myJump.UniqueID.ToString();

		//inicialize entryTv and assign to the entry_tv_value (same for tc, fall, weight)
		entryTv = myJump.Tv.ToString();
		entry_tv_value.Text = Util.TrimDecimals(entryTv, pDN);
	
		if(myJump.TypeHasWeight) {
			entryWeight = myJump.Weight.ToString();
			entry_weight_value.Text = entryWeight;
			entry_weight_value.Sensitive = true;
		} else {
			entry_weight_value.Sensitive = false;
		}
		
		if (myJump.TypeHasFall) {
			entryTc = myJump.Tc.ToString();
			entry_tc_value.Text = Util.TrimDecimals(entryTc, pDN);
			entryFall = myJump.Fall.ToString();
			entry_fall_value.Text = entryFall;
			entry_tc_value.Sensitive = true;
			entry_fall_value.Sensitive = true;
		} else {
			entry_tc_value.Sensitive = false;
			entry_fall_value.Sensitive = false;
		}
	
		//hide limited value (show in rj class)
		label_limited_title.Hide();
		label_limited_value.Hide();

		TextBuffer tb = new TextBuffer (new TextTagTable());
		tb.SetText(myJump.Description);
		textview_description.Buffer = tb;

		combo_jumpType = new Combo ();
		string [] jumpTypes;
		if (myJump.TypeHasFall) {
			jumpTypes = SqliteJumpType.SelectJumpTypes("", "TC", true); //don't show allJumpsName row, TC jumps, only select name
		} else {
			jumpTypes = SqliteJumpType.SelectJumpTypes("", "nonTC", true); //don't show allJumpsName row, nonTC jumps, only select name
		}
		combo_jumpType.PopdownStrings = jumpTypes;
		foreach (string jumpType in jumpTypes) {
			if (jumpType == myJump.Type) {
				combo_jumpType.Entry.Text = jumpType;
			}
		}
		
		
		string [] jumpers = SqlitePersonSession.SelectCurrentSession(myJump.SessionID, false); //not reversed
		combo_jumpers = new Combo();
		combo_jumpers.PopdownStrings = jumpers;
		foreach (string jumper in jumpers) {
			if (jumper == myJump.PersonID + ":" + myJump.PersonName) {
				combo_jumpers.Entry.Text = jumper;
			}
		}
		
		oldPersonID = myJump.PersonID;
			
		hbox_combo_jumpType.PackStart(combo_jumpType, true, true, 0);
		hbox_combo_jumpType.ShowAll();
		hbox_combo_jumper.PackStart(combo_jumpers, true, true, 0);
		hbox_combo_jumper.ShowAll();
	}
	
	private void on_entry_tv_value_changed (object o, EventArgs args) {
		if(Util.IsNumber(entry_tv_value.Text.ToString())){
			entryTv = entry_tv_value.Text.ToString();
		} else {
			entry_tv_value.Text = "";
			entry_tv_value.Text = entryTv;
		}
	}
		
	private void on_entry_tc_value_changed (object o, EventArgs args) {
		if(Util.IsNumber(entry_tc_value.Text.ToString())){
			entryTc = entry_tc_value.Text.ToString();
		} else {
			entry_tc_value.Text = "";
			entry_tc_value.Text = entryTc;
		}
	}
		
	private void on_entry_fall_value_changed (object o, EventArgs args) {
		if(Util.IsNumber(entry_fall_value.Text.ToString())){
			entryFall = entry_fall_value.Text.ToString();
		} else {
			entry_fall_value.Text = "";
			entry_fall_value.Text = entryFall;
		}
	}
		
	private void on_entry_weight_value_changed (object o, EventArgs args) {
		if(Util.IsNumber(entry_weight_value.Text.ToString())){
			entryWeight = entry_weight_value.Text.ToString();
		} else {
			entry_weight_value.Text = "";
			entry_weight_value.Text = entryWeight;
		}
	}
		
	void on_button_cancel_clicked (object o, EventArgs args)
	{
		EditJumpWindowBox.edit_jump.Hide();
		EditJumpWindowBox = null;
	}
	
	void on_edit_jump_delete_event (object o, EventArgs args)
	{
		EditJumpWindowBox.edit_jump.Hide();
		EditJumpWindowBox = null;
	}
	
	void on_button_accept_clicked (object o, EventArgs args)
	{
		int jumpID = Convert.ToInt32 ( label_jump_id_value.Text );
		string myJumper = combo_jumpers.Entry.Text;
		string [] myJumperFull = myJumper.Split(new char[] {':'});
		
		string myDesc = textview_description.Buffer.Text;
		
		//update the weight percent of jump if needed
		double jumpPercentWeightForNewPerson = 0;
		if(entryWeight != "0") {
			//obtain weight of old person
			double oldPersonWeight = SqlitePerson.SelectJumperWeight(Convert.ToInt32(oldPersonID)); 
			double jumpWeightInKg = oldPersonWeight * Convert.ToDouble(entryWeight) / 100;
			
			double newPersonWeight = SqlitePerson.SelectJumperWeight(Convert.ToInt32(myJumperFull[0])); 
			jumpPercentWeightForNewPerson = jumpWeightInKg * 100 / newPersonWeight; 
			Console.WriteLine("oldPW: {0}, jWinKg {1}, newPW{2}, jWin%NewP{3}",
					oldPersonWeight, jumpWeightInKg, newPersonWeight, jumpPercentWeightForNewPerson);
		}
	
		SqliteJump.Update(jumpID, combo_jumpType.Entry.Text, entryTv, entryTc, entryFall, Convert.ToInt32 (myJumperFull[0]), jumpPercentWeightForNewPerson, myDesc);

		EditJumpWindowBox.edit_jump.Hide();
		EditJumpWindowBox = null;
	}

	public Button Button_accept 
	{
		set { button_accept = value;	}
		get { return button_accept;	}
	}

}

//--------------------------------------------------------
//---------------- edit jumpRJ WIDGET --------------------
//--------------------------------------------------------

public class EditJumpRjWindow 
{
	[Widget] Gtk.Window edit_jump;
	[Widget] Gtk.Button button_accept;
	[Widget] Gtk.Label label_header;
	[Widget] Gtk.Label label_jump_id_value;
	[Widget] Gtk.Label label_tc_title;
	[Widget] Gtk.Label label_tv_title;
	[Widget] Gtk.Entry entry_tc_value;
	[Widget] Gtk.Entry entry_tv_value;
	[Widget] Gtk.Entry entry_fall_value;
	[Widget] Gtk.Entry entry_weight_value;
	[Widget] Gtk.Label label_limited_value;
	[Widget] Gtk.Box hbox_combo_jumpType;
	[Widget] Gtk.Box hbox_combo_jumper;
	[Widget] Gtk.Combo combo_jumpers;
	[Widget] Gtk.TextView textview_description;

	static EditJumpRjWindow EditJumpRjWindowBox;
	Gtk.Window parent;
	int pDN;
	string type;
	string entryFall = "0"; 
	string entryWeight = "0"; //used for record the % for old person if we change it
	int oldPersonID; //used for record the % for old person if we change it

	EditJumpRjWindow (Gtk.Window parent) {
		Glade.XML gladeXML = Glade.XML.FromAssembly ("chronojump.glade", "edit_jump", null);

		gladeXML.Autoconnect(this);
		this.parent = parent;
		
		System.Globalization.NumberFormatInfo localeInfo = new System.Globalization.NumberFormatInfo();
		localeInfo = System.Globalization.NumberFormatInfo.CurrentInfo;
		label_header.Text = string.Format(Catalog.GetString("Use this window for edit a reactive jump\n(decimal separator: '{0}')"), localeInfo.NumberDecimalSeparator);
	}
	
	static public EditJumpRjWindow Show (Gtk.Window parent, JumpRj myJump, int pDN)
	{
		Console.WriteLine(myJump);
		if (EditJumpRjWindowBox == null) {
			EditJumpRjWindowBox = new EditJumpRjWindow (parent);
		}
		
		EditJumpRjWindowBox.pDN = pDN;
		
		EditJumpRjWindowBox.edit_jump.Show ();

		EditJumpRjWindowBox.fillDialog (myJump);


		return EditJumpRjWindowBox;
	}
	
	private void fillDialog (JumpRj myJump)
	{
		label_jump_id_value.Text = myJump.UniqueID.ToString();

		//hide tc and tv data
		label_tc_title.Hide();
		label_tv_title.Hide();
		entry_tc_value.Hide();
		entry_tv_value.Hide();
		
		label_limited_value.Text = myJump.Limited.ToString();
		
		entryFall = myJump.Fall.ToString();
		entry_fall_value.Text = entryFall;
		
		if (Util.HasWeight(SqliteJumpType.SelectJumpRjTypes("", false), myJump.Type)) {
			entryWeight = myJump.Weight.ToString();
			entry_weight_value.Text = entryWeight;
			entry_weight_value.Sensitive = true;
		} else {
			entry_weight_value.Sensitive = false;
		}

		this.type = myJump.Type;

		TextBuffer tb = new TextBuffer (new TextTagTable());
		tb.SetText(myJump.Description);
		textview_description.Buffer = tb;

		string [] jumpers = SqlitePersonSession.SelectCurrentSession(myJump.SessionID, false); //not reversed
		combo_jumpers = new Combo();
		combo_jumpers.PopdownStrings = jumpers;
		foreach (string jumper in jumpers) {
			Console.WriteLine("jumper: {0}, name: {1}", jumper, myJump.PersonID + ":" + myJump.PersonName);
			if (jumper == myJump.PersonID + ":" + myJump.PersonName) {
				combo_jumpers.Entry.Text = jumper;
			}
		}
		hbox_combo_jumper.PackStart(combo_jumpers, true, true, 0);
		hbox_combo_jumper.ShowAll();
		
		Gtk.Label label_jumpType = new Label();
		label_jumpType.Text = myJump.Type;
		hbox_combo_jumpType.PackStart(label_jumpType, false, false, 0);
		hbox_combo_jumpType.ShowAll();
		
		oldPersonID = myJump.PersonID;
	}
	
	//this is never called, created here for compatibility with editjump class
	private void on_entry_tv_value_changed (object o, EventArgs args) {
	}
		
	//this is never called, created here for compatibility with editjump class
	private void on_entry_tc_value_changed (object o, EventArgs args) {
	}
		
	private void on_entry_fall_value_changed (object o, EventArgs args) {
		if(Util.IsNumber(entry_fall_value.Text.ToString())){
			entryFall = entry_fall_value.Text.ToString();
		} else {
			entry_fall_value.Text = "";
			entry_fall_value.Text = entryFall;
		}
	}
		
	private void on_entry_weight_value_changed (object o, EventArgs args) {
		if(Util.IsNumber(entry_weight_value.Text.ToString())){
			entryWeight = entry_weight_value.Text.ToString();
		} else {
			entry_weight_value.Text = "";
			entry_weight_value.Text = entryWeight;
		}
	}
		
	
	void on_button_cancel_clicked (object o, EventArgs args)
	{
		EditJumpRjWindowBox.edit_jump.Hide();
		EditJumpRjWindowBox = null;
	}
	
	void on_edit_jump_delete_event (object o, EventArgs args)
	{
		EditJumpRjWindowBox.edit_jump.Hide();
		EditJumpRjWindowBox = null;
	}
	
	void on_button_accept_clicked (object o, EventArgs args)
	{
		int jumpID = Convert.ToInt32 ( label_jump_id_value.Text );
		string myJumper = combo_jumpers.Entry.Text;
		string [] myJumperFull = myJumper.Split(new char[] {':'});
		
		string myDesc = textview_description.Buffer.Text;

		//update the weight percent of jump if needed
		double jumpPercentWeightForNewPerson = 0;
		if(entryWeight != "0") {
			//obtain weight of old person
			double oldPersonWeight = SqlitePerson.SelectJumperWeight(Convert.ToInt32(oldPersonID)); 
			double jumpWeightInKg = oldPersonWeight * Convert.ToDouble(entryWeight) / 100;
			
			double newPersonWeight = SqlitePerson.SelectJumperWeight(Convert.ToInt32(myJumperFull[0])); 
			jumpPercentWeightForNewPerson = jumpWeightInKg * 100 / newPersonWeight; 
			Console.WriteLine("oldPW: {0}, jWinKg {1}, newPW{2}, jWin%NewP{3}",
					oldPersonWeight, jumpWeightInKg, newPersonWeight, jumpPercentWeightForNewPerson);
		}
	
		SqliteJump.UpdateRj(jumpID, Convert.ToInt32 (myJumperFull[0]), entryFall, jumpPercentWeightForNewPerson, myDesc);


		EditJumpRjWindowBox.edit_jump.Hide();
		EditJumpRjWindowBox = null;
	}

	public Button Button_accept 
	{
		set { button_accept = value;	}
		get { return button_accept;	}
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

	JumpType jumpType;
	JumpRj jumpRj; //used on button_accept
	

	RepairJumpRjWindow (Gtk.Window parent, JumpRj myJump) {
		Glade.XML gladeXML = Glade.XML.FromAssembly ("chronojump.glade", "repair_sub_event", null);

		gladeXML.Autoconnect(this);
		this.parent = parent;
		this.jumpRj = myJump;
	
		repair_sub_event.Title = Catalog.GetString("Repair reactive jump");
		
		System.Globalization.NumberFormatInfo localeInfo = new System.Globalization.NumberFormatInfo();
		localeInfo = System.Globalization.NumberFormatInfo.CurrentInfo;
		label_header.Text = string.Format(Catalog.GetString("Use this window for repair a reactive jump\nDouble clic any cell for editing (decimal separator: '{0}')"), localeInfo.NumberDecimalSeparator);
	
		
		jumpType = SqliteJumpType.SelectAndReturnJumpRjType(myJump.Type);
		
		TextBuffer tb = new TextBuffer (new TextTagTable());
		tb.SetText(createTextForTextView(jumpType));
		textview1.Buffer = tb;
		
		createTreeView(treeview_subevents);
		//count, tc, tv
		store = new TreeStore(typeof (string), typeof (string), typeof(string));
		treeview_subevents.Model = store;
		fillTreeView (treeview_subevents, store, myJump);
	
		button_add_before.Sensitive = false;
		button_add_after.Sensitive = false;
		button_delete.Sensitive = false;
		
		label_totaltime_value.Text = getTotalTime().ToString() + " " + Catalog.GetString("seconds");
	}
	
	static public RepairJumpRjWindow Show (Gtk.Window parent, JumpRj myJump)
	{
		//Console.WriteLine(myJump);
		if (RepairJumpRjWindowBox == null) {
			RepairJumpRjWindowBox = new RepairJumpRjWindow (parent, myJump);
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
				//if it's a jump type jumpsLimited with a fixed value, then don't allow the creation of more jumps, and respect the -1 at last TV if found
				fixedString = string.Format(Catalog.GetString("\nThis jump type is fixed for {0} jumps, you cannot add more."), myJumpType.FixedValue);
			} else {
				//if it's a jump type timeLimited with a fixed value, then complain when the total time is higher
				fixedString = string.Format(Catalog.GetString("\nThis jump type is fixed for {0} seconds, totaltime cannot be greater."), myJumpType.FixedValue);
			}
		}
		return jumpTypeString + startString + fixedString;
	}

	
	private void createTreeView (Gtk.TreeView myTreeView) {
		myTreeView.HeadersVisible=true;
		int count = 0;

		myTreeView.AppendColumn ( Catalog.GetString ("Count"), new CellRendererText(), "text", count++);
		//myTreeView.AppendColumn ( Catalog.GetString ("TC"), new CellRendererText(), "text", count++);
		//myTreeView.AppendColumn ( Catalog.GetString ("TV"), new CellRendererText(), "text", count++);

		Gtk.TreeViewColumn tcColumn = new Gtk.TreeViewColumn ();
		tcColumn.Title = Catalog.GetString("TC");
		Gtk.CellRendererText tcCell = new Gtk.CellRendererText ();
		tcCell.Editable = true;
		tcCell.Edited += tcCellEdited;
		tcColumn.PackStart (tcCell, true);
		tcColumn.AddAttribute(tcCell, "text", count ++);
		myTreeView.AppendColumn ( tcColumn );
		
		Gtk.TreeViewColumn tvColumn = new Gtk.TreeViewColumn ();
		tvColumn.Title = Catalog.GetString("TV");
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
		if(Util.IsNumber(args.NewText) && (string) treeview_subevents.Model.GetValue(iter,1) != "-1") {
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
		if(Util.IsNumber(args.NewText) && (string) treeview_subevents.Model.GetValue(iter,2) != "-1") {
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
		if(iterOk) {
			do {
				double myTc = Convert.ToDouble((string) treeview_subevents.Model.GetValue(myIter, 1));
				double myTv = Convert.ToDouble((string) treeview_subevents.Model.GetValue(myIter, 2));
				if(myTc != -1) totalTime += myTc;
				if(myTv != -1) totalTime += myTv;
			} while (store.IterNext (ref myIter));
		}
		return totalTime;
	}
	
	private void fillTreeView (Gtk.TreeView tv, TreeStore store, JumpRj myJump)
	{
		if(myJump.TcString.Length > 0 && myJump.TvString.Length > 0) {
			string [] tcArray = myJump.TcString.Split(new char[] {'='});
			string [] tvArray = myJump.TvString.Split(new char[] {'='});

			int count = 0;
			foreach (string myTc in tcArray) {
				string myTv;
				if(tvArray.Length >= count)
					myTv = tvArray[count];
				else
					myTv = "";
				
				store.AppendValues ( (count+1).ToString(), myTc, myTv );
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
			store.Insert(out iter, position);
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
			store.Insert(out iter, position);
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
			
		//calculate other variables needed for jumpRj creation
		
		int jumps = Util.GetNumberOfJumps(tvString, false);
		string limitString = "";
	
		if(jumpType.FixedValue > 0) {
			//if this jumpType has a fixed value of jumps or time, limitstring has not changed
			if(jumpType.JumpsLimited) {
				limitString = jumpType.FixedValue.ToString() + "J";
			} else {
				limitString = jumpType.FixedValue.ToString() + "T";
			}
		} else {
			//else limitstring should be calculated
			if(jumpType.JumpsLimited) {
				limitString = jumps.ToString() + "J";
			} else {
				limitString = Util.GetTotalTime(tcString, tvString) + "T";
			}
		}

		//save it deleting the old first for having the same uniqueID
		SqliteJump.Delete("jumpRj", jumpRj.UniqueID.ToString());
		//int uniqueID = SqliteJump.InsertRj(jumpRj.UniqueID.ToString(), jumpRj.PersonID, jumpRj.SessionID, 
		SqliteJump.InsertRj(jumpRj.UniqueID.ToString(), jumpRj.PersonID, jumpRj.SessionID, 
				jumpRj.Type, Util.GetMax(tvString), Util.GetMax(tcString), 
				jumpRj.Fall, jumpRj.Weight, jumpRj.Description,
				Util.GetAverage(tvString), Util.GetAverage(tcString),
				tvString, tcString,
				jumps, Util.GetTotalTime(tcString, tvString), limitString
				);

		//close the window
		RepairJumpRjWindowBox.repair_sub_event.Hide();
		RepairJumpRjWindowBox = null;
	}

	void on_button_cancel_clicked (object o, EventArgs args)
	{
		RepairJumpRjWindowBox.repair_sub_event.Hide();
		RepairJumpRjWindowBox = null;
	}
	
	void on_delete_event (object o, EventArgs args)
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
	static double limited = 3;
	static bool jumpsLimited;
	static int weight = 20;
	static int fall = 20;
	
	static JumpExtraWindow JumpExtraWindowBox;
	Gtk.Window parent;

	JumpExtraWindow (Gtk.Window parent) {
		Glade.XML gladeXML = Glade.XML.FromAssembly ("chronojump.glade", "jump_extra", null);

		gladeXML.Autoconnect(this);
		this.parent = parent;
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
			}
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
	
	void on_jump_extra_delete_event (object o, EventArgs args)
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
		Console.WriteLine("option: {0}", option);
	}
	
	void on_radiobutton_weight_toggled (object o, EventArgs args)
	{
		option = "%";
		Console.WriteLine("option: {0}", option);
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
				return Limited.ToString() + "J";
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

public class JumpsMoreWindow 
{
	[Widget] Gtk.Window jumps_runs_more;
	
	private TreeStore store;
	[Widget] Gtk.TreeView treeview_more;
	[Widget] Gtk.Button button_accept;

	static JumpsMoreWindow JumpsMoreWindowBox;
	Gtk.Window parent;
	
	private string selectedJumpType;
	private bool selectedStartIn;
	private bool selectedExtraWeight;
	
	JumpsMoreWindow (Gtk.Window parent) {
		Glade.XML gladeXML = Glade.XML.FromAssembly ("chronojump.glade", "jumps_runs_more", null);

		gladeXML.Autoconnect(this);
		this.parent = parent;
		
		createTreeView(treeview_more);
		//name, startIn, weight, description
		store = new TreeStore(typeof (string), typeof (string), typeof (string), typeof (string));
		treeview_more.Model = store;
		fillTreeView(treeview_more,store);

		button_accept.Sensitive = false;
	}
	
	static public JumpsMoreWindow Show (Gtk.Window parent)
	{
		if (JumpsMoreWindowBox == null) {
			JumpsMoreWindowBox = new JumpsMoreWindow (parent);
		}
		JumpsMoreWindowBox.jumps_runs_more.Show ();
		
		return JumpsMoreWindowBox;
	}
	
	private void createTreeView (Gtk.TreeView tv) {
		tv.HeadersVisible=true;
		int count = 0;
		
		tv.AppendColumn ( Catalog.GetString ("Name"), new CellRendererText(), "text", count++);
		tv.AppendColumn ( Catalog.GetString ("Start inside"), new CellRendererText(), "text", count++);
		tv.AppendColumn ( Catalog.GetString ("Extra weight"), new CellRendererText(), "text", count++);
		tv.AppendColumn ( Catalog.GetString ("Description"), new CellRendererText(), "text", count++);
	}
	
	private void fillTreeView (Gtk.TreeView tv, TreeStore store) 
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

			store.AppendValues (
					//myStringFull[0], //don't display de uniqueID
					myStringFull[1],	//name 
					myStringFull[2], 	//startIn
					myStringFull[3], 	//weight
					myStringFull[4]		//description
					);
		}	
	}

	//puts a value in private member selected
	private void on_treeview_changed (object o, EventArgs args)
	{
		TreeView tv = (TreeView) o;
		TreeModel model;
		TreeIter iter;
		selectedJumpType = "-1";
		selectedStartIn = false;
		selectedExtraWeight = false;

		// you get the iter and the model if something is selected
		if (tv.Selection.GetSelected (out model, out iter)) {
			selectedJumpType = (string) model.GetValue (iter, 0);
			if( (string) model.GetValue (iter, 1) == Catalog.GetString("Yes") ) {
				selectedStartIn = true;
			}
			if( (string) model.GetValue (iter, 2) == Catalog.GetString("Yes") ) {
				selectedExtraWeight = true;
			}
			button_accept.Sensitive = true;
		}
	}
	
	void on_row_double_clicked (object o, EventArgs args)
	{
		TreeView tv = (TreeView) o;
		TreeModel model;
		TreeIter iter;

		if (tv.Selection.GetSelected (out model, out iter)) {
			//put selection in selected
			selectedJumpType = (string) model.GetValue (iter, 0);
			if( (string) model.GetValue (iter, 1) == Catalog.GetString("Yes") ) {
				selectedStartIn = true;
			}
			if( (string) model.GetValue (iter, 2) == Catalog.GetString("Yes") ) {
				selectedExtraWeight = true;
			}

			//activate on_button_accept_clicked()
			button_accept.Activate();
		}
	}
	
	void on_button_cancel_clicked (object o, EventArgs args)
	{
		JumpsMoreWindowBox.jumps_runs_more.Hide();
		JumpsMoreWindowBox = null;
	}
	
	void on_jumps_runs_more_delete_event (object o, EventArgs args)
	{
		JumpsMoreWindowBox.jumps_runs_more.Hide();
		JumpsMoreWindowBox = null;
	}
	
	void on_button_accept_clicked (object o, EventArgs args)
	{
		JumpsMoreWindowBox.jumps_runs_more.Hide();
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
	
	public string SelectedJumpType 
	{
		set {
			selectedJumpType = value;	
		}
		get {
			return selectedJumpType;
		}
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

public class JumpsRjMoreWindow 
{
	[Widget] Gtk.Window jumps_runs_more;
	
	private TreeStore store;
	[Widget] Gtk.TreeView treeview_more;
	[Widget] Gtk.Button button_accept;

	static JumpsRjMoreWindow JumpsRjMoreWindowBox;
	Gtk.Window parent;
	
	private string selectedJumpType;
	private bool selectedStartIn;
	private bool selectedExtraWeight;
	private bool selectedLimited;
	private double selectedLimitedValue;
	private bool selectedUnlimited;
	
	JumpsRjMoreWindow (Gtk.Window parent) {
		//the glade window is the same as jumps_more
		Glade.XML gladeXML = Glade.XML.FromAssembly ("chronojump.glade", "jumps_runs_more", null);

		gladeXML.Autoconnect(this);
		this.parent = parent;

		//if jumps_runs_more is opened for showing Rj jumpTypes make it wider
		jumps_runs_more.Resize(600,300);
		
		createTreeView(treeview_more);
		//name, limited by, limited value, startIn, weight, description
		store = new TreeStore(typeof (string), typeof (string), typeof(string),
				typeof (string), typeof (string), typeof (string));
		treeview_more.Model = store;
		fillTreeView(treeview_more,store);
			
		button_accept.Sensitive = false;
	}
	
	static public JumpsRjMoreWindow Show (Gtk.Window parent)
	{
		if (JumpsRjMoreWindowBox == null) {
			JumpsRjMoreWindowBox = new JumpsRjMoreWindow (parent);
		}
		JumpsRjMoreWindowBox.jumps_runs_more.Show ();
		
		return JumpsRjMoreWindowBox;
	}
	
	private void createTreeView (Gtk.TreeView tv) {
		tv.HeadersVisible=true;
		int count = 0;

		tv.AppendColumn ( Catalog.GetString ("Name"), new CellRendererText(), "text", count++);
		tv.AppendColumn ( Catalog.GetString ("Limited by"), new CellRendererText(), "text", count++);
		tv.AppendColumn ( Catalog.GetString ("Limited value"), new CellRendererText(), "text", count++);
		tv.AppendColumn ( Catalog.GetString ("Start inside"), new CellRendererText(), "text", count++);
		tv.AppendColumn ( Catalog.GetString ("Extra weight"), new CellRendererText(), "text", count++);
		tv.AppendColumn ( Catalog.GetString ("Description"), new CellRendererText(), "text", count++);
	}
	
	private void fillTreeView (Gtk.TreeView tv, TreeStore store) 
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

			store.AppendValues (
					//myStringFull[0], //don't display de uniqueID
					myStringFull[1],	//name 
					myLimiter,		//jumps or seconds		
					myLimiterValue,		//? or exact value
					myStringFull[2], 	//startIn
					myStringFull[3], 	//weight
					myStringFull[6]		//description
					);
		}	
	}

	//puts a value in private member selected
	//private void on_treeview_jumps_more_changed (object o, EventArgs args)
	private void on_treeview_changed (object o, EventArgs args)
	{
		TreeView tv = (TreeView) o;
		TreeModel model;
		TreeIter iter;
		selectedJumpType = "-1";
		selectedStartIn = false;
		selectedExtraWeight = false;
		selectedLimited = false;
		selectedLimitedValue = 0;
		selectedUnlimited = false; //true if it's an unlimited reactive jump

		// you get the iter and the model if something is selected
		if (tv.Selection.GetSelected (out model, out iter)) {
			selectedJumpType = (string) model.GetValue (iter, 0);
			
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
			button_accept.Sensitive = true;
		}
	}
	
	void on_row_double_clicked (object o, EventArgs args)
	{
		TreeView tv = (TreeView) o;
		TreeModel model;
		TreeIter iter;

		if (tv.Selection.GetSelected (out model, out iter)) {
			selectedJumpType = (string) model.GetValue (iter, 0);
			
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

			//activate on_button_accept_clicked()
			button_accept.Activate();
		}
	}
	
	
	void on_button_cancel_clicked (object o, EventArgs args)
	{
		JumpsRjMoreWindowBox.jumps_runs_more.Hide();
		JumpsRjMoreWindowBox = null;
	}
	
	void on_jumps_runs_more_delete_event (object o, EventArgs args)
	{
		JumpsRjMoreWindowBox.jumps_runs_more.Hide();
		JumpsRjMoreWindowBox = null;
	}
	
	void on_button_accept_clicked (object o, EventArgs args)
	{
		JumpsRjMoreWindowBox.jumps_runs_more.Hide();
	}

	
	public Button Button_accept 
	{
		set { button_accept = value; } 
		get { return button_accept; }
	}
	
	public string SelectedJumpType 
	{
		get { return selectedJumpType; }
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

//--------------------------------------------------------
//---------------- JUMP RJ EXECUTE WIDGET ----------------
//--------------------------------------------------------

public class JumpRjExecuteWindow 
{
	[Widget] Gtk.Window jump_rj_execute;
	
	[Widget] Gtk.Label label_jumper;
	[Widget] Gtk.Label label_jumptype;
	[Widget] Gtk.Label label_progress_name;
	[Widget] Gtk.Label label_extra_name;
	[Widget] Gtk.Label label_extra_value;
	
	[Widget] Gtk.ProgressBar progressbar_tv_current;
	[Widget] Gtk.ProgressBar progressbar_tc_current;
	[Widget] Gtk.ProgressBar progressbar_tv_tc_current_1up;
	[Widget] Gtk.ProgressBar progressbar_tv_tc_current_0;
	[Widget] Gtk.ProgressBar progressbar_tv_avg;
	[Widget] Gtk.ProgressBar progressbar_tc_avg;
	[Widget] Gtk.ProgressBar progressbar_tv_tc_avg_1up;
	[Widget] Gtk.ProgressBar progressbar_tv_tc_avg_0;
	
	[Widget] Gtk.ProgressBar progressbar_execution;
	
	[Widget] Gtk.CheckButton checkbutton_show_tv_tc;
	[Widget] Gtk.Box hbox_tv_tc;
	[Widget] Gtk.Box vbox_tv_tc;
	[Widget] Gtk.Label label_tv_tc;
	
	[Widget] Gtk.Button button_cancel;
	[Widget] Gtk.Button button_finish;

	int pDN;
	double limit;
	bool jumpsLimited;
	static bool showTvTc = false;
	
	static JumpRjExecuteWindow JumpRjExecuteWindowBox;
	Gtk.Window parent;

	JumpRjExecuteWindow (Gtk.Window parent) {
		Glade.XML gladeXML = Glade.XML.FromAssembly ("chronojump.glade", "jump_rj_execute", null);

		gladeXML.Autoconnect(this);
		this.parent = parent;
		
		//in first rj jump in a session, always doesn't show the tv/tc
		//showTvTc = false;
	}
	
	static public JumpRjExecuteWindow Show (Gtk.Window parent, string jumperName, string jumpType, 
			int pDN, double limit, bool jumpsLimited)
	{
		if (JumpRjExecuteWindowBox == null) {
			JumpRjExecuteWindowBox = new JumpRjExecuteWindow (parent); 
		}
		
		JumpRjExecuteWindowBox.initializeVariables (jumperName, jumpType, pDN, limit, jumpsLimited);
		
		JumpRjExecuteWindowBox.jump_rj_execute.Show ();

		return JumpRjExecuteWindowBox;
	}

	void initializeVariables (string jumperName, string jumpType, int pDN, double limit, bool jumpsLimited) 
	{
		this.pDN = pDN;
		this.limit = limit;
		this.jumpsLimited = jumpsLimited;
		this.label_jumper.Text = jumperName;
		this.label_jumptype.Text = jumpType;
		this.label_extra_value.Text = "";
		
		progressbar_tv_current.Fraction = 0;
		progressbar_tc_current.Fraction = 0;
		progressbar_tv_tc_current_1up.Fraction = 0;
		progressbar_tv_tc_current_0.Fraction = 0;
		progressbar_tv_avg.Fraction = 0;
		progressbar_tc_avg.Fraction = 0;
		progressbar_tv_tc_avg_1up.Fraction = 0;
		progressbar_tv_tc_avg_0.Fraction = 0;
		progressbar_execution.Fraction = 0;
		
		progressbar_tv_current.Text = "";
		progressbar_tc_current.Text = "";
		progressbar_tv_tc_current_1up.Text = "";
		progressbar_tv_tc_current_0.Text = "";
		progressbar_tv_avg.Text = "";
		progressbar_tc_avg.Text = "";
		progressbar_tv_tc_avg_1up.Text = "";
		progressbar_tv_tc_avg_0.Text = "";
		progressbar_execution.Text = "";

		button_finish.Sensitive = true;
		button_cancel.Sensitive = true;

		if(showTvTc) {
			tv_tc_show_hide(true);
			checkbutton_show_tv_tc.Active = true;
		} else {
			tv_tc_show_hide(false);
			checkbutton_show_tv_tc.Active = false;
		}
	}
	
	void on_checkbutton_show_tv_tc_clicked (object o, EventArgs args)
	{
		if(checkbutton_show_tv_tc.Active) {
			tv_tc_show_hide(true);
			showTvTc = true;
		} else {
			tv_tc_show_hide(false);
			showTvTc = false;
		}
	}
	
	void tv_tc_show_hide (bool show) {
		if(show) {
			hbox_tv_tc.Show();
			vbox_tv_tc.Show();
			label_tv_tc.Show();
		} else {
			hbox_tv_tc.Hide();
			vbox_tv_tc.Hide();
			label_tv_tc.Hide();
		}
	}

	public void JumpEndedHideButtons() {
		button_finish.Sensitive = false;
		button_cancel.Sensitive = false;
		
		//if it was an unlimited jump, put the jumpsBar at end
		if(limit == -1) {
			progressbar_execution.Fraction = 1.0;
		}
	}
	
	void on_finish_clicked (object o, EventArgs args)
	{
		//event will be raised, and managed in chronojump.cs
	}
	
	void on_button_cancel_clicked (object o, EventArgs args)
	{
		//event will be raised, and managed in chronojump.cs
		JumpEndedHideButtons();
	}
		
	void on_button_close_clicked (object o, EventArgs args)
	{
		JumpRjExecuteWindowBox.jump_rj_execute.Hide();
		JumpRjExecuteWindowBox = null;
	}
	
	void on_jump_rj_execute_delete_event (object o, EventArgs args)
	{
		JumpRjExecuteWindowBox.jump_rj_execute.Hide();
		JumpRjExecuteWindowBox = null;
	}

	public void ProgressbarExecution (double jumps, double time)
	{
		if(limit == -1) {	//unlimited jump (until 'finish' is clicked)
			progressbar_execution.Pulse();
			label_progress_name.Text = "";
			label_extra_name.Text = "";
			label_extra_value.Text = string.Format(Catalog.GetString("{0} jumps; {1} seconds"), jumps.ToString(), 
					Util.TrimDecimals(time.ToString(), pDN));
		} else {
			double myFraction;
			if(jumpsLimited)
				myFraction = jumps / limit;
			else
				myFraction = time / limit;
			
			if(myFraction > 1)
				myFraction = 1;
			else if(myFraction < 0)
				myFraction = 0;
			
			Console.Write("{0}-{1}-{2}",jumps, limit, myFraction);
			progressbar_execution.Fraction = myFraction;
			Console.WriteLine("fraction done");

			if(jumpsLimited) { 
				label_progress_name.Text = Catalog.GetString("Jumps");
				progressbar_execution.Text = jumps.ToString() + " / " + limit.ToString();
				label_extra_name.Text = Catalog.GetString("Time");
				label_extra_value.Text = Util.TrimDecimals(time.ToString(), pDN);
			}
			else {	
				label_progress_name.Text = Catalog.GetString("Time");
				progressbar_execution.Text = Util.TrimDecimals(time.ToString(), pDN) + " / " + limit.ToString();
				label_extra_name.Text = Catalog.GetString("Jumps");
				label_extra_value.Text = jumps.ToString();
			}
		}
	}
	

	public double ProgressbarTvCurrent 
	{
		set { 
			progressbar_tv_current.Text = Util.TrimDecimals(value.ToString(), pDN);
			if(value > 1.0) value = 1.0;
			else if(value < 0) value = 0;
			progressbar_tv_current.Fraction = value;
		}
	}

	public double ProgressbarTcCurrent
	{
		set { 
			progressbar_tc_current.Text = Util.TrimDecimals(value.ToString(), pDN);
			if(value > 1.0) value = 1.0;
			else if(value < 0) value = 0;
			progressbar_tc_current.Fraction = value;
		}
	}

	public double ProgressbarTvTcCurrent
	{
		set { 
			if(value > 1) {
				progressbar_tv_tc_current_1up.Text = Util.TrimDecimals(value.ToString(), pDN);
				if(value > 4.0) value = 4.0;
				else if(value <= 0) value = 0.01; //fix the div by 0 bug
				progressbar_tv_tc_current_1up.Fraction = value/4;
				progressbar_tv_tc_current_0.Fraction = 1;
				progressbar_tv_tc_current_0.Text = "";
			} else {
				progressbar_tv_tc_current_1up.Fraction = 0;
				progressbar_tv_tc_current_1up.Text = "";
				progressbar_tv_tc_current_0.Text = Util.TrimDecimals(value.ToString(), pDN);
				if(value > 1.0) value = 1.0;
				else if(value < 0) value = 0;
				progressbar_tv_tc_current_0.Fraction = value;
			}
		}
	}

	public double ProgressbarTvAvg 
	{
		set { 
			progressbar_tv_avg.Text = Util.TrimDecimals(value.ToString(), pDN);
			if(value > 1.0) value = 1.0;
			else if(value < 0) value = 0;
			progressbar_tv_avg.Fraction = value;
		}
	}

	public double ProgressbarTcAvg
	{
		set { 
			progressbar_tc_avg.Text = Util.TrimDecimals(value.ToString(), pDN);
			if(value > 1.0) value = 1.0;
			else if(value < 0) value = 0;
			progressbar_tc_avg.Fraction = value;
		}
	}

	public double ProgressbarTvTcAvg
	{
		set { 
			if(value > 0) {
				progressbar_tv_tc_avg_1up.Text = Util.TrimDecimals(value.ToString(), pDN);
				if(value > 4.0) value = 4.0;
				else if(value <= 0) value = 0.01; //fix the div by 0 bug
				progressbar_tv_tc_avg_1up.Fraction = value/4;
				progressbar_tv_tc_avg_0.Fraction = 1;
				progressbar_tv_tc_avg_0.Text = "";
			} else {
				progressbar_tv_tc_avg_1up.Fraction = 0;
				progressbar_tv_tc_avg_1up.Text = "";
				progressbar_tv_tc_avg_0.Text = Util.TrimDecimals(value.ToString(), pDN);
				if(value > 1.0) value = 1.0;
				else if(value < 0) value = 0;
				progressbar_tv_tc_avg_0.Fraction = value;
			}
		}
	}

	public Button ButtonCancel 
	{
		get { return button_cancel; }
	}
	
	public Button ButtonFinish 
	{
		get { return button_finish; }
	}
}
