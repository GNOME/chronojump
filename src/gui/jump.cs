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
	
		label_fall_value.Text = "0";
		label_weight_value.Text = "0";
		
		if(myJump.TypeHasWeight) {
			label_weight_value.Text = myJump.Weight.ToString();
		} 
		if (myJump.TypeHasFall) {
			label_fall_value.Text = myJump.Fall.ToString();
		} 

		this.type = myJump.Type;

		TextBuffer tb = new TextBuffer (new TextTagTable());
		tb.SetText(myJump.Description);
		textview_description.Buffer = tb;

		string [] jumpers = SqlitePersonSession.SelectCurrentSession(myJump.SessionID);
		combo_jumpers = new Combo();
		combo_jumpers.PopdownStrings = jumpers;
		foreach (string jumper in jumpers) {
			Console.WriteLine("jumper: {0}, name: {1}", jumper, myJump.PersonID + ": " + myJump.JumperName);
			if (jumper == myJump.PersonID + ": " + myJump.JumperName) {
				combo_jumpers.Entry.Text = jumper;
			}
		}
		
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
	
		SqliteJump.Update(jumpID, Convert.ToInt32 (myJumperFull[0]), myDesc);

		EditJumpWindowBox.edit_jump.Hide();
		EditJumpWindowBox = null;
	}

	public Button Button_accept 
	{
		set { button_accept = value;	}
		get { return button_accept;	}
	}

}

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
	
		label_fall_value.Text = "0";
		label_weight_value.Text = "0";
		label_limited_value.Text = "0";
		
		label_limited_value.Text = myJump.Limited.ToString();
		//future: RJ with weight and/or fall
		//label_fall_value.Text = myJump.Fall.ToString();
		//label_weight_value.Text = myJump.Weight.ToString();

		this.type = myJump.Type;

		TextBuffer tb = new TextBuffer (new TextTagTable());
		tb.SetText(myJump.Description);
		textview_description.Buffer = tb;

		string [] jumpers = SqlitePersonSession.SelectCurrentSession(myJump.SessionID);
		combo_jumpers = new Combo();
		combo_jumpers.PopdownStrings = jumpers;
		foreach (string jumper in jumpers) {
			Console.WriteLine("jumper: {0}, name: {1}", jumper, myJump.PersonID + ": " + myJump.JumperName);
			if (jumper == myJump.PersonID + ": " + myJump.JumperName) {
				combo_jumpers.Entry.Text = jumper;
			}
		}
		
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
	
		SqliteJump.RjUpdate(jumpID, Convert.ToInt32 (myJumperFull[0]), myDesc);

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
	[Widget] Gtk.Window jumps_more;
	
	private TreeStore store;
	[Widget] Gtk.TreeView treeview_jumps_more;
	[Widget] Gtk.Button button_accept;

	static JumpsMoreWindow JumpsMoreWindowBox;
	Gtk.Window parent;
	
	private string selectedJumpType;
	private bool selectedStartIn;
	private bool selectedExtraWeight;
	
	JumpsMoreWindow (Gtk.Window parent) {
		Glade.XML gladeXML = Glade.XML.FromAssembly ("chronojump.glade", "jumps_more", null);

		gladeXML.Autoconnect(this);
		this.parent = parent;
		
		createTreeView(treeview_jumps_more);
		//name, startIn, weight, description
		store = new TreeStore(typeof (string), typeof (string), typeof (string), typeof (string));
		treeview_jumps_more.Model = store;
		fillTreeView(treeview_jumps_more,store);

		button_accept.Sensitive = false;
	}
	
	static public JumpsMoreWindow Show (Gtk.Window parent)
	{
		if (JumpsMoreWindowBox == null) {
			JumpsMoreWindowBox = new JumpsMoreWindow (parent);
		}
		JumpsMoreWindowBox.jumps_more.Show ();
		
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
	private void on_treeview_jumps_more_changed (object o, EventArgs args)
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
	
	void on_button_close_clicked (object o, EventArgs args)
	{
		JumpsMoreWindowBox.jumps_more.Hide();
		JumpsMoreWindowBox = null;
	}
	
	void on_jumps_more_delete_event (object o, EventArgs args)
	{
		JumpsMoreWindowBox.jumps_more.Hide();
		JumpsMoreWindowBox = null;
	}
	
	void on_button_accept_clicked (object o, EventArgs args)
	{
		Console.Write("SELECTED");
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
	[Widget] Gtk.Window jumps_rj_more;
	
	private TreeStore store;
	[Widget] Gtk.TreeView treeview_jumps_rj_more;
	[Widget] Gtk.Button button_accept;

	static JumpsRjMoreWindow JumpsRjMoreWindowBox;
	Gtk.Window parent;
	
	private string selectedJumpType;
	private bool selectedStartIn;
	private bool selectedExtraWeight;
	private bool selectedLimited;
	private double selectedLimitedValue;
	
	JumpsRjMoreWindow (Gtk.Window parent) {
		Glade.XML gladeXML = Glade.XML.FromAssembly ("chronojump.glade", "jumps_rj_more", null);

		gladeXML.Autoconnect(this);
		this.parent = parent;
		
		createTreeView(treeview_jumps_rj_more);
		//name, limited by, limited value, startIn, weight, description
		store = new TreeStore(typeof (string), typeof (string), typeof(string),
				typeof (string), typeof (string), typeof (string));
		treeview_jumps_rj_more.Model = store;
		fillTreeView(treeview_jumps_rj_more,store);
			
		button_accept.Sensitive = false;
	}
	
	static public JumpsRjMoreWindow Show (Gtk.Window parent)
	{
		if (JumpsRjMoreWindowBox == null) {
			JumpsRjMoreWindowBox = new JumpsRjMoreWindow (parent);
		}
		JumpsRjMoreWindowBox.jumps_rj_more.Show ();
		
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
			string myLimiter = Catalog.GetString("Jumps");
			if(myStringFull[4] == "0") {
				myLimiter = Catalog.GetString("Seconds");
			}
			string myLimiterValue = "?";
			if(Convert.ToDouble(myStringFull[5]) > 0) {
				myLimiterValue = myStringFull[5];
			}

			iter = store.AppendValues (
					//myStringFull[0], //don't display de uniqueID
					myStringFull[1],	//name 
					myLimiter,		//jumps or seconds		
					myLimiterValue,		//? or exact value
					myStringFull[2], 	//startIn
					myStringFull[3], 	//weight
					myStringFull[4]		//description
					);
		}	
	}

	//puts a value in private member selected
	private void on_treeview_jumps_rj_more_changed (object o, EventArgs args)
	{
		TreeView tv = (TreeView) o;
		TreeModel model;
		TreeIter iter;
		selectedJumpType = "-1";
		selectedStartIn = false;
		selectedExtraWeight = false;
		selectedLimited = false;
		selectedLimitedValue = 0;

		// you get the iter and the model if something is selected
		if (tv.Selection.GetSelected (out model, out iter)) {
			selectedJumpType = (string) model.GetValue (iter, 0);
			if( (string) model.GetValue (iter, 1) == Catalog.GetString("Jumps") ) {
				selectedLimited = true;
			}
			
			if( (string) model.GetValue (iter, 2) == "?") {
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
			if( (string) model.GetValue (iter, 1) == Catalog.GetString("Jumps") ) {
				selectedLimited = true;
			}
			
			if( (string) model.GetValue (iter, 2) == "?") {
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
	
	
	void on_button_close_clicked (object o, EventArgs args)
	{
		JumpsRjMoreWindowBox.jumps_rj_more.Hide();
		JumpsRjMoreWindowBox = null;
	}
	
	void on_jumps_rj_more_delete_event (object o, EventArgs args)
	{
		JumpsRjMoreWindowBox.jumps_rj_more.Hide();
		JumpsRjMoreWindowBox = null;
	}
	
	void on_button_accept_clicked (object o, EventArgs args)
	{
		Console.Write("Selected");
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
		get {
			return selectedJumpType;
		}
	}
	
	public bool SelectedLimited 
	{
		get {
			return selectedLimited;
		}
	}
	
	public double SelectedLimitedValue 
	{
		get {
			return selectedLimitedValue;
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

