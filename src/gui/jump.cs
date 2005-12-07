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
using Gnome;
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
	[Widget] Gtk.Label label_jump_id_value;
	[Widget] Gtk.Label label_type_value;
	[Widget] Gtk.Label label_tv_value;
	[Widget] Gtk.Label label_tc_value;
	[Widget] Gtk.Label label_fall_value;
	[Widget] Gtk.Label label_weight_value;
	[Widget] Gtk.Box hbox_combo;
	[Widget] Gtk.Combo combo_jumpers;
	[Widget] Gtk.TextView textview_description;

	static EditJumpWindow EditJumpWindowBox;
	Gtk.Window parent;
	string type;
	double oldJumpWeightPercent = 0; //used for record the % for old person if we change it
	int oldPersonID; //used for record the % for old person if we change it

	EditJumpWindow (Gtk.Window parent) {
		Glade.XML gladeXML = Glade.XML.FromAssembly ("chronojump.glade", "edit_jump", null);

		gladeXML.Autoconnect(this);
		this.parent = parent;
	}
	
	static public EditJumpWindow Show (Gtk.Window parent, Jump myJump)
	{
		//Console.WriteLine(myJump);
		if (EditJumpWindowBox == null) {
			EditJumpWindowBox = new EditJumpWindow (parent);
		}
		
		EditJumpWindowBox.edit_jump.Show ();

		EditJumpWindowBox.fillDialog (myJump);

		return EditJumpWindowBox;
}
	
	private void fillDialog (Jump myJump)
	{
		label_jump_id_value.Text = myJump.UniqueID.ToString();
		label_type_value.Text = myJump.Type;
		label_tv_value.Text = myJump.Tv.ToString();
		label_tc_value.Text = myJump.Tc.ToString();
	
		label_fall_value.Text = myJump.Fall.ToString();
		label_weight_value.Text = myJump.Weight.ToString();
		
		if(myJump.TypeHasWeight) {
			label_weight_value.Text = myJump.Weight.ToString();
			oldJumpWeightPercent = myJump.Weight;
		} 
		if (myJump.TypeHasFall) {
			label_fall_value.Text = myJump.Fall.ToString();
		} 

		TextBuffer tb = new TextBuffer (new TextTagTable());
		tb.SetText(myJump.Description);
		textview_description.Buffer = tb;

		string [] jumpers = SqlitePersonSession.SelectCurrentSession(myJump.SessionID, false); //not reversed
		combo_jumpers = new Combo();
		combo_jumpers.PopdownStrings = jumpers;
		foreach (string jumper in jumpers) {
			Console.WriteLine("jumper: {0}, name: {1}", jumper, myJump.PersonID + ": " + myJump.JumperName);
			if (jumper == myJump.PersonID + ": " + myJump.JumperName) {
				combo_jumpers.Entry.Text = jumper;
			}
		}
		
		oldPersonID = myJump.PersonID;
			
		hbox_combo.PackStart(combo_jumpers, true, true, 0);
		hbox_combo.ShowAll();
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
		if(oldJumpWeightPercent > 0) {
			//obtain weight of old person
			double oldPersonWeight = SqlitePerson.SelectJumperWeight(Convert.ToInt32(oldPersonID)); 
			double jumpWeightInKg = oldPersonWeight * oldJumpWeightPercent / 100;
			
			double newPersonWeight = SqlitePerson.SelectJumperWeight(Convert.ToInt32(myJumperFull[0])); 
			jumpPercentWeightForNewPerson = jumpWeightInKg * 100 / newPersonWeight; 
			Console.WriteLine("oldPW: {0}, jWinKg {1}, newPW{2}, jWin%NewP{3}",
					oldPersonWeight, jumpWeightInKg, newPersonWeight, jumpPercentWeightForNewPerson);
		}
	
		SqliteJump.Update("jump", jumpID, Convert.ToInt32 (myJumperFull[0]), jumpPercentWeightForNewPerson, myDesc);

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
	[Widget] Gtk.Label label_jump_id_value;
	[Widget] Gtk.Label label_type_value;
	[Widget] Gtk.Label label_tv_value;
	[Widget] Gtk.Label label_tc_value;
	[Widget] Gtk.Label label_fall_value;
	[Widget] Gtk.Label label_weight_value;
	[Widget] Gtk.Label label_limited_value;
	[Widget] Gtk.Box hbox_combo;
	[Widget] Gtk.Combo combo_jumpers;
	[Widget] Gtk.TextView textview_description;

	static EditJumpRjWindow EditJumpRjWindowBox;
	Gtk.Window parent;
	string type;
	double oldJumpWeightPercent = 0; //used for record the % for old person if we change it
	int oldPersonID; //used for record the % for old person if we change it

	EditJumpRjWindow (Gtk.Window parent) {
		Glade.XML gladeXML = Glade.XML.FromAssembly ("chronojump.glade", "edit_jump", null);

		gladeXML.Autoconnect(this);
		this.parent = parent;
	}
	
	static public EditJumpRjWindow Show (Gtk.Window parent, JumpRj myJump)
	{
		Console.WriteLine(myJump);
		if (EditJumpRjWindowBox == null) {
			EditJumpRjWindowBox = new EditJumpRjWindow (parent);
		}
		
		EditJumpRjWindowBox.edit_jump.Show ();

		EditJumpRjWindowBox.fillDialog (myJump);


		return EditJumpRjWindowBox;
	}
	
	private void fillDialog (JumpRj myJump)
	{
		label_jump_id_value.Text = myJump.UniqueID.ToString();
		label_type_value.Text = myJump.Type;
		label_tv_value.Text = myJump.Tv.ToString();
		label_tc_value.Text = myJump.Tc.ToString();
	
		label_limited_value.Text = myJump.Limited.ToString();
		label_fall_value.Text = myJump.Fall.ToString();
		label_weight_value.Text = myJump.Weight.ToString();
		oldJumpWeightPercent = myJump.Weight;

		this.type = myJump.Type;

		TextBuffer tb = new TextBuffer (new TextTagTable());
		tb.SetText(myJump.Description);
		textview_description.Buffer = tb;

		string [] jumpers = SqlitePersonSession.SelectCurrentSession(myJump.SessionID, false); //not reversed
		combo_jumpers = new Combo();
		combo_jumpers.PopdownStrings = jumpers;
		foreach (string jumper in jumpers) {
			Console.WriteLine("jumper: {0}, name: {1}", jumper, myJump.PersonID + ": " + myJump.JumperName);
			if (jumper == myJump.PersonID + ": " + myJump.JumperName) {
				combo_jumpers.Entry.Text = jumper;
			}
		}
		
		oldPersonID = myJump.PersonID;
		
		hbox_combo.PackStart(combo_jumpers, true, true, 0);
		hbox_combo.ShowAll();
	
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
		if(oldJumpWeightPercent > 0) {
			//obtain weight of old person
			double oldPersonWeight = SqlitePerson.SelectJumperWeight(Convert.ToInt32(oldPersonID)); 
			double jumpWeightInKg = oldPersonWeight * oldJumpWeightPercent / 100;
			
			double newPersonWeight = SqlitePerson.SelectJumperWeight(Convert.ToInt32(myJumperFull[0])); 
			jumpPercentWeightForNewPerson = jumpWeightInKg * 100 / newPersonWeight; 
			Console.WriteLine("oldPW: {0}, jWinKg {1}, newPW{2}, jWin%NewP{3}",
					oldPersonWeight, jumpWeightInKg, newPersonWeight, jumpPercentWeightForNewPerson);
		}
	
		SqliteJump.Update("jumpRj", jumpID, Convert.ToInt32 (myJumperFull[0]), jumpPercentWeightForNewPerson, myDesc);


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
	

	RepairJumpRjWindow (Gtk.Window parent, JumpRj myJump) {
		Glade.XML gladeXML = Glade.XML.FromAssembly ("chronojump.glade", "repair_sub_event", null);

		gladeXML.Autoconnect(this);
		this.parent = parent;
	
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
		//foreach all lines
		//create a jumpRj calculating all the data for the new changes
		//save it 
		//close the window
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
	
	private void fillTreeView (Gtk.TreeView tv, TreeStore store) {
		TreeIter iter = new TreeIter();

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

			iter = store.AppendValues (
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
	
	private void fillTreeView (Gtk.TreeView tv, TreeStore store) {
		TreeIter iter = new TreeIter();

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

			iter = store.AppendValues (
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

