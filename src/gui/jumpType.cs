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
using GLib; //for Value
using System.Text; //StringBuilder
using System.Collections; //ArrayList
using Mono.Unix;


//--------------------------------------------------------
//---------------- JUMP TYPE ADD WIDGET ------------------
//--------------------------------------------------------

public class JumpTypeAddWindow 
{
	Gtk.Window jump_type_add;
	Gtk.Frame frame;
	Gtk.Button button_accept;
	Gtk.Entry entry_name;

	Gtk.Label label_header;
	Gtk.Label label_main_options;
	Gtk.Grid grid_main_options;

	Gtk.RadioButton radiobutton_simple;
	Gtk.RadioButton radiobutton_repetitive;
	Gtk.RadioButton radiobutton_unlimited;
	Gtk.VBox vbox_limited;
	Gtk.HBox hbox_fixed;
	Gtk.RadioButton radiobutton_limited_jumps;
	Gtk.CheckButton checkbutton_limited_fixed;
	Gtk.SpinButton spin_fixed_num;

	Gtk.Label label_jump_type_simple;
	Gtk.Label label_jump_type_multiple;
	Gtk.VBox vbox_simple_type;
	Gtk.HBox hbox_multiple_type;

	Gtk.RadioButton radiobutton_simple_startIn_yes;
	Gtk.RadioButton radiobutton_simple_startIn_no;
	Gtk.RadioButton radiobutton_multiple_startIn_yes;
	//Gtk.RadioButton radiobutton_multiple_startIn_no;
	Gtk.RadioButton radiobutton_extra_weight_yes;
	Gtk.RadioButton radiobutton_extra_weight_no;
	Gtk.Label label_drop_jump;
	Gtk.TextView textview_description;

	public Gtk.Button fakeButtonAccept;
	static JumpTypeAddWindow JumpTypeAddWindowBox;

	public bool InsertedSimple;
	private bool descriptionChanging = false;
	private string name;

	JumpTypeAddWindow (Gtk.Window parent, bool simple)
	{
		/*
		Glade.XML gladeXML;
		gladeXML = Glade.XML.FromAssembly (Util.GetGladePath() + "jump_type_add.glade", "jump_type_add", "chronojump");
		gladeXML.Autoconnect(this);
		*/
		Gtk.Builder builder = new Gtk.Builder (null, Util.GetGladePath () + "jump_type_add.glade", null);
		connectWidgets (builder);
		builder.Autoconnect (this);

		jump_type_add.Parent = parent;
		
		fakeButtonAccept = new Gtk.Button();

		//put an icon to window
		UtilGtk.IconWindow(jump_type_add);

		//manage window color
		if(! Config.UseSystemColor) {
			UtilGtk.WindowColor(jump_type_add, Config.ColorBackground);
			UtilGtk.ContrastLabelsLabel(Config.ColorBackgroundIsDark, label_header);

			UtilGtk.WidgetColor (frame, Config.ColorBackgroundShifted);
			UtilGtk.ContrastLabelsFrame (Config.ColorBackgroundShiftedIsDark, frame);
		}
	}
	
	static public JumpTypeAddWindow Show (Gtk.Window parent, bool simple)
	{
		if (JumpTypeAddWindowBox == null) {
			JumpTypeAddWindowBox = new JumpTypeAddWindow (parent, simple);
		}
		
		JumpTypeAddWindowBox.fillDialog (simple);
		JumpTypeAddWindowBox.jump_type_add.Show ();

		return JumpTypeAddWindowBox;
	}
	
	private void fillDialog (bool simple)
	{
		vbox_limited.Sensitive = false;	
		hbox_fixed.Sensitive = false;	
		button_accept.Sensitive = false;
		spin_fixed_num.Sensitive = false;
		radiobutton_extra_weight_no.Active = true;
		label_drop_jump.Sensitive = false;

		//active the desired radio
		if(simple) {
			radiobutton_simple.Active = true;

			label_jump_type_simple.Visible = true;
			vbox_simple_type.Visible = true;
			label_jump_type_multiple.Visible = false;
			hbox_multiple_type.Visible = false;
		} else {
			radiobutton_repetitive.Active = true;

			label_jump_type_simple.Visible = false;
			vbox_simple_type.Visible = false;
			label_jump_type_multiple.Visible = true;
			hbox_multiple_type.Visible = true;
		}

		//don't show the radios
		radiobutton_simple.Visible = false;
		radiobutton_repetitive.Visible = false;

		//if simple don't show nothing
		label_main_options.Visible = ! simple;
		grid_main_options.Visible = ! simple;

		textview_description.Buffer.Changed += new EventHandler(descriptionChanged);
		descriptionChanging = false;
	}

	private void descriptionChanged(object o,EventArgs args)
	{
		if(descriptionChanging)
			return;

		descriptionChanging = true;

		TextBuffer tb = o as TextBuffer;
		if (o == null)
			return;

		tb.Text = Util.MakeValidSQL(tb.Text);
		descriptionChanging = false;
	}

	void on_button_cancel_clicked (object o, EventArgs args)
	{
		JumpTypeAddWindowBox.jump_type_add.Hide();
		JumpTypeAddWindowBox = null;
	}
	
	void on_jump_type_add_delete_event (object o, DeleteEventArgs args)
	{
		JumpTypeAddWindowBox.jump_type_add.Hide();
		JumpTypeAddWindowBox = null;
	}
	
	void on_button_accept_clicked (object o, EventArgs args)
	{
		name = Util.RemoveTildeAndColonAndDot(entry_name.Text);

		//check if this jump type exists, and check it's name is not AllJumpsName
		bool jumpTypeExists = Sqlite.Exists (false, Constants.JumpTypeTable, name);
		if(name == Constants.AllJumpsNameStr())
			jumpTypeExists = true;
		
		if(jumpTypeExists) {
			//string myString =  Catalog.GetString ("Jump type: '") + 
			//	Util.RemoveTildeAndColonAndDot(entry_name.Text) + 
			//	Catalog.GetString ("' exists. Please, use another name");
			
			string myString = string.Format(Catalog.GetString("Jump type: '{0}' exists. Please, use another name"), name);
			
			LogB.Information (myString);
			ErrorWindow.Show(myString);
		} else {
			string myJump = name;
			if(radiobutton_simple.Active) {
				if(radiobutton_simple_startIn_yes.Active)
					myJump = myJump + ":1";
				else
					myJump = myJump + ":0";
			} else {
				if(radiobutton_multiple_startIn_yes.Active)
					myJump = myJump + ":1";
				else
					myJump = myJump + ":0";
			}

			if(radiobutton_extra_weight_yes.Active) {
				myJump = myJump + ":1"; 
			} else {
				myJump = myJump + ":0"; 
			}
			
			if(radiobutton_simple.Active) {
				myJump = myJump + ":" + 
					Util.RemoveTildeAndColon(textview_description.Buffer.Text);
			
				SqliteJumpType.JumpTypeInsert(myJump, false); //false, because dbcon is not opened
				InsertedSimple = true;
			} else {
				
				if(radiobutton_unlimited.Active) {
					//unlimited (but in jumps do like if it's limited by jumps 
					//(explanation in sqlite/jumpType.cs)
					myJump = myJump + ":1:-1"; 
				} else {
					if(radiobutton_limited_jumps.Active) {
						myJump = myJump + ":1"; 
					} else {
						myJump = myJump + ":0";
					}
				
					if(checkbutton_limited_fixed.Active) {
						myJump = myJump + ":" + spin_fixed_num.Value; 
					} else {
						myJump = myJump + ":0"; 
					}
				}
			
				myJump = myJump + ":" + 
					Util.RemoveTildeAndColon(textview_description.Buffer.Text);
				
				SqliteJumpType.JumpRjTypeInsert(myJump, false); //false, because dbcon is not opened
				InsertedSimple = false;
			}
			
			LogB.Information(string.Format("Inserted: {0}", myJump));
		
			fakeButtonAccept.Click();

			JumpTypeAddWindowBox.jump_type_add.Hide();
			JumpTypeAddWindowBox = null;
		}
	}

	void on_radiobutton_simple_toggled (object o, EventArgs args)
	{
		vbox_limited.Sensitive = false;	
		hbox_fixed.Sensitive = false;	
	}
	
	void on_radiobutton_repetitive_toggled (object o, EventArgs args)
	{
		vbox_limited.Sensitive = true;	
		if( ! radiobutton_unlimited.Active) {
			hbox_fixed.Sensitive = true;	
		}
	}
	
	void on_radiobutton_limited_jumps_or_time_toggled (object o, EventArgs args)
	{
		hbox_fixed.Sensitive = true;	
	}
	
	void on_radiobutton_unlimited_toggled (object o, EventArgs args)
	{
		hbox_fixed.Sensitive = false;	
	}
	
	void on_checkbutton_limited_fixed_clicked (object o, EventArgs args)
	{
		if(checkbutton_limited_fixed.Active) {
			spin_fixed_num.Sensitive = true;
		} else {
			spin_fixed_num.Sensitive = false;
		}
	}

	private void on_radiobutton_simple_startIn_yes_toggled (object o, EventArgs args)
	{
		if(radiobutton_simple_startIn_yes.Active)
			label_drop_jump.Sensitive = false;
	}
	private void on_radiobutton_simple_startIn_no_toggled (object o, EventArgs args)
	{
		if(radiobutton_simple_startIn_no.Active)
			label_drop_jump.Sensitive = true;
	}

	void on_entries_required_changed (object o, EventArgs args)
	{
		entry_name.Text = Util.MakeValidSQL(entry_name.Text);

		if(entry_name.Text.ToString().Length > 0)
			button_accept.Sensitive = true;
		else
			button_accept.Sensitive = false;
	}
		
	public Button FakeButtonAccept 
	{
		set { fakeButtonAccept = value; }
		get { return fakeButtonAccept; }
	}

	public string Name
	{
		get { return name; }
	}

	private void connectWidgets (Gtk.Builder builder)
	{
		jump_type_add = (Gtk.Window) builder.GetObject ("jump_type_add");
		frame = (Gtk.Frame) builder.GetObject ("frame");
		button_accept = (Gtk.Button) builder.GetObject ("button_accept");
		entry_name = (Gtk.Entry) builder.GetObject ("entry_name");

		label_header = (Gtk.Label) builder.GetObject ("label_header");
		label_main_options = (Gtk.Label) builder.GetObject ("label_main_options");
		grid_main_options = (Gtk.Grid) builder.GetObject ("grid_main_options");

		radiobutton_simple = (Gtk.RadioButton) builder.GetObject ("radiobutton_simple");
		radiobutton_repetitive = (Gtk.RadioButton) builder.GetObject ("radiobutton_repetitive");
		radiobutton_unlimited = (Gtk.RadioButton) builder.GetObject ("radiobutton_unlimited");
		vbox_limited = (Gtk.VBox) builder.GetObject ("vbox_limited");
		hbox_fixed = (Gtk.HBox) builder.GetObject ("hbox_fixed");
		radiobutton_limited_jumps = (Gtk.RadioButton) builder.GetObject ("radiobutton_limited_jumps");
		checkbutton_limited_fixed = (Gtk.CheckButton) builder.GetObject ("checkbutton_limited_fixed");
		spin_fixed_num = (Gtk.SpinButton) builder.GetObject ("spin_fixed_num");

		label_jump_type_simple = (Gtk.Label) builder.GetObject ("label_jump_type_simple");
		label_jump_type_multiple = (Gtk.Label) builder.GetObject ("label_jump_type_multiple");
		vbox_simple_type = (Gtk.VBox) builder.GetObject ("vbox_simple_type");
		hbox_multiple_type = (Gtk.HBox) builder.GetObject ("hbox_multiple_type");

		radiobutton_simple_startIn_yes = (Gtk.RadioButton) builder.GetObject ("radiobutton_simple_startIn_yes");
		radiobutton_simple_startIn_no = (Gtk.RadioButton) builder.GetObject ("radiobutton_simple_startIn_no");
		radiobutton_multiple_startIn_yes = (Gtk.RadioButton) builder.GetObject ("radiobutton_multiple_startIn_yes");
		//radiobutton_multiple_startIn_no = (Gtk.RadioButton) builder.GetObject ("radiobutton_multiple_startIn_no");
		radiobutton_extra_weight_yes = (Gtk.RadioButton) builder.GetObject ("radiobutton_extra_weight_yes");
		radiobutton_extra_weight_no = (Gtk.RadioButton) builder.GetObject ("radiobutton_extra_weight_no");
		label_drop_jump = (Gtk.Label) builder.GetObject ("label_drop_jump");
		textview_description = (Gtk.TextView) builder.GetObject ("textview_description");
	}
}


