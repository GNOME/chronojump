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
using GLib; //for Value
using System.Text; //StringBuilder
using System.Collections; //ArrayList


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
	//[Widget] Gtk.Label label_limited_value;
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
		Console.WriteLine(myJump);
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
		//label_limited_value.Text = "0";
		
		if(myJump.Type == "SJ+") {
			label_weight_value.Text = myJump.Weight.ToString();
		} else if (myJump.Type == "DJ") {
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
//---------------- SJ+ WIDGET ----------------------------
//--------------------------------------------------------

public class SjPlusWindow {
	[Widget] Gtk.Window sj_plus;
	[Widget] Gtk.SpinButton spinbutton1;
	[Widget] Gtk.Button button_accept;
	[Widget] Gtk.RadioButton radiobutton_kg;
	[Widget] Gtk.RadioButton radiobutton_weight;

	static string option = "Kg";
	static int weight = 10;
	
	static SjPlusWindow SjPlusWindowBox;
	Gtk.Window parent;

	SjPlusWindow (Gtk.Window parent) {
		Glade.XML gladeXML = Glade.XML.FromAssembly ("chronojump.glade", "sj_plus", null);

		gladeXML.Autoconnect(this);
		this.parent = parent;
		
	}
	
	static public SjPlusWindow Show (Gtk.Window parent)
	{
		if (SjPlusWindowBox == null) {
			SjPlusWindowBox = new SjPlusWindow (parent);
		}
		SjPlusWindowBox.spinbutton1.Value = weight;
		if (option == "Kg") {
			SjPlusWindowBox.radiobutton_kg.Active = true;
		} else {
			SjPlusWindowBox.radiobutton_weight.Active = true;
		}
		
		SjPlusWindowBox.sj_plus.Show ();

		return SjPlusWindowBox;
	}
	
	void on_button_cancel_clicked (object o, EventArgs args)
	{
		SjPlusWindowBox.sj_plus.Hide();
		SjPlusWindowBox = null;
	}
	
	void on_sj_plus_delete_event (object o, EventArgs args)
	{
		SjPlusWindowBox.sj_plus.Hide();
		SjPlusWindowBox = null;
	}
	
	void on_button_accept_clicked (object o, EventArgs args)
	{
		weight = (int) spinbutton1.Value;
		
		Console.WriteLine("button_accept_clicked. Value: {0}{1}", spinbutton1.Value.ToString(), option);

		SjPlusWindowBox.sj_plus.Hide();
		SjPlusWindowBox = null;
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

	public int Weight 
	{
		get { return weight;	}
	}
}

//--------------------------------------------------------
//---------------- DJ FALL WIDGET ------------------------
//--------------------------------------------------------

public class DjFallWindow {
	[Widget] Gtk.Window dj_fall;
	[Widget] Gtk.SpinButton spinbutton_fall;
	[Widget] Gtk.Button button_accept;

	static int fall = 20;
	
	static DjFallWindow DjFallWindowBox;
	Gtk.Window parent;

	DjFallWindow (Gtk.Window parent) {
		Glade.XML gladeXML = Glade.XML.FromAssembly ("chronojump.glade", "dj_fall", null);

		gladeXML.Autoconnect(this);
		this.parent = parent;
		
	}
	
	static public DjFallWindow Show (Gtk.Window parent)
	{
		if (DjFallWindowBox == null) {
			DjFallWindowBox = new DjFallWindow (parent);
		}
		DjFallWindowBox.spinbutton_fall.Value = fall;
		
		DjFallWindowBox.dj_fall.Show ();

		return DjFallWindowBox;
	}
	
	void on_button_cancel_clicked (object o, EventArgs args)
	{
		DjFallWindowBox.dj_fall.Hide();
		DjFallWindowBox = null;
	}
	
	void on_dj_fall_delete_event (object o, EventArgs args)
	{
		DjFallWindowBox.dj_fall.Hide();
		DjFallWindowBox = null;
	}
	
	void on_button_accept_clicked (object o, EventArgs args)
	{
		fall = (int) spinbutton_fall.Value;
		
		Console.WriteLine("button_accept_clicked. Value: {0}", spinbutton_fall.Value.ToString());

		DjFallWindowBox.dj_fall.Hide();
		DjFallWindowBox = null;
	}

	public Button Button_accept 
	{
		set { button_accept = value;	}
		get { return button_accept;	}
	}

	public int Fall 
	{
		get { return fall;	}
	}
}

//--------------------------------------------------------
//---------------- RJ WIDGET (similar to sj+ ) -----------
//--------------------------------------------------------


public class RjWindow {
	
	[Widget] Gtk.Window rj;
	[Widget] Gtk.SpinButton spinbutton_limit;

	[Widget] Gtk.Button button_accept;
	[Widget] Gtk.RadioButton radiobutton_jump;
	[Widget] Gtk.RadioButton radiobutton_time;
	
	static RjWindow RjWindowBox;
	Gtk.Window parent;

	static string option = "J";
	static int limited = 20;
	
	RjWindow (Gtk.Window parent) {
		Glade.XML gladeXML = Glade.XML.FromAssembly ("chronojump.glade", "rj", null);

		gladeXML.Autoconnect(this);
		this.parent = parent;
		
	}
	
	static public RjWindow Show (Gtk.Window parent)
	{
		if (RjWindowBox == null) {
			RjWindowBox = new RjWindow (parent);
		}
		RjWindowBox.spinbutton_limit.Value = limited;
		if (option == "J") {
			RjWindowBox.radiobutton_jump.Active = true;
		} else {
			RjWindowBox.radiobutton_time.Active = true;
		}
		
		RjWindowBox.rj.Show ();

		return RjWindowBox;
	}
	
	void on_button_cancel_clicked (object o, EventArgs args)
	{
		RjWindowBox.rj.Hide();
		RjWindowBox = null;
	}
	
	void on_rj_delete_event (object o, EventArgs args)
	{
		RjWindowBox.rj.Hide();
		RjWindowBox = null;
	}
	
	void on_radiobutton_jump_toggled (object o, EventArgs args)
	{
		option = "J";
		Console.WriteLine("option: {0}", option);
	}
	
	void on_radiobutton_time_toggled (object o, EventArgs args)
	{
		option = "T";
		Console.WriteLine("option: {0}", option);
	}

	void on_button_accept_clicked (object o, EventArgs args)
	{
		limited = (int) spinbutton_limit.Value;
		
		RjWindowBox.rj.Hide();
		RjWindowBox = null;
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

	public string Option 
	{
		get { return option;	}
	}
	
	public int Limited 
	{
		get { return limited; }
	}

}

