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
using GLib; //for Value
using System.Text; //StringBuilder
using System.Collections; //ArrayList
using Mono.Unix;


//--------------------------------------------------------
//---------------- RUN TYPE ADD WIDGET -------------------
//--------------------------------------------------------

public class RunTypeAddWindow 
{
	[Widget] Gtk.Window run_type_add;
	[Widget] Gtk.Button button_accept;
	[Widget] Gtk.Entry entry_name;
	[Widget] Gtk.RadioButton radiobutton_simple;
	[Widget] Gtk.RadioButton radiobutton_interval;
	[Widget] Gtk.RadioButton radiobutton_unlimited;
	[Widget] Gtk.VBox vbox_limited;
	[Widget] Gtk.HBox hbox_fixed;
	[Widget] Gtk.RadioButton radiobutton_limited_tracks;
	[Widget] Gtk.CheckButton checkbutton_limited_fixed;
	[Widget] Gtk.CheckButton checkbutton_distance_fixed;
	[Widget] Gtk.SpinButton spin_fixed_num;
	[Widget] Gtk.Label label_distance;
	[Widget] Gtk.SpinButton spin_distance;

	[Widget] Gtk.TextView textview_description;

	static RunTypeAddWindow RunTypeAddWindowBox;
	Gtk.Window parent;
	ErrorWindow errorWin;

	RunTypeAddWindow (Gtk.Window parent) {
		Glade.XML gladeXML;
		try {
			gladeXML = Glade.XML.FromAssembly ("chronojump.glade", "run_type_add", null);
		} catch {
			gladeXML = Glade.XML.FromAssembly ("chronojump.glade.chronojump.glade", "run_type_add", null);
		}

		gladeXML.Autoconnect(this);
		this.parent = parent;
	}
	
	static public RunTypeAddWindow Show (Gtk.Window parent)
	{
		if (RunTypeAddWindowBox == null) {
			RunTypeAddWindowBox = new RunTypeAddWindow (parent);
		}
		
		RunTypeAddWindowBox.run_type_add.Show ();
		RunTypeAddWindowBox.fillDialog ();

		return RunTypeAddWindowBox;
	}
	
	private void fillDialog ()
	{
		vbox_limited.Sensitive = false;	
		hbox_fixed.Sensitive = false;	
		button_accept.Sensitive = false;
		spin_fixed_num.Sensitive = false;
		label_distance.Text = "Distance";
		spin_distance.Sensitive = false;
	}
		
	void on_button_cancel_clicked (object o, EventArgs args)
	{
		RunTypeAddWindowBox.run_type_add.Hide();
		RunTypeAddWindowBox = null;
	}
	
	void on_delete_event (object o, DeleteEventArgs args)
	{
		RunTypeAddWindowBox.run_type_add.Hide();
		RunTypeAddWindowBox = null;
	}
	
	void on_button_accept_clicked (object o, EventArgs args)
	{
		//check if this run type exists, and check it's name is not "All runs"
		bool runTypeExists = SqliteRunType.Exists (Util.RemoveTildeAndColonAndDot(entry_name.Text));
		if(Util.RemoveTildeAndColonAndDot(entry_name.Text) == Catalog.GetString("All runs")) {
			runTypeExists = true;
		}
		
		if(runTypeExists) {
			//string myString =  Catalog.GetString ("Run type: '") + 
			//	Util.RemoveTildeAndColonAndDot(entry_name.Text) + 
			//	Catalog.GetString ("' exists. Please, use another name");
			string myString = string.Format(Catalog.GetString("Run type: '{0}' exists. Please, use another name"), Util.RemoveTildeAndColonAndDot(entry_name.Text) );
			Console.WriteLine (myString);
			errorWin = ErrorWindow.Show(run_type_add, myString);
		} else {
			string myRun = "";
			myRun = Util.RemoveTildeAndColonAndDot(entry_name.Text);
						
			if(checkbutton_distance_fixed.Active) {
				myRun = myRun + ":" + spin_distance.Value.ToString();
			} else {
				myRun = myRun + ":0";
			}
			
			if(radiobutton_simple.Active) {
				myRun = myRun + ":" + 
					Util.RemoveTildeAndColon(textview_description.Buffer.Text);
			
				SqliteRunType.RunTypeInsert(myRun, false); //false, because dbcon is not opened
			} else {
				if(radiobutton_unlimited.Active) {
					//unlimited (but in runs do like if it's limited by seconds
					//(explanation in sqlite/jumpType.cs)
					myRun = myRun + ":0:0:1"; 
				} else {
					if(radiobutton_limited_tracks.Active) {
						myRun = myRun + ":1"; 
					} else {
						myRun = myRun + ":0";
					}
				
					if(checkbutton_limited_fixed.Active) {
						myRun = myRun + ":" + spin_fixed_num.Value; 
					} else {
						myRun = myRun + ":0"; 
					}
						
					//not unlimited run
					myRun = myRun + ":0"; 
				}
			
				myRun = myRun + ":" + 
					Util.RemoveTildeAndColon(textview_description.Buffer.Text);
				
				SqliteRunType.RunIntervalTypeInsert(myRun, false); //false, because dbcon is not opened
			}
			
			Console.WriteLine("Inserted: {0}", myRun);
		}

		RunTypeAddWindowBox.run_type_add.Hide();
		RunTypeAddWindowBox = null;
	}

	void on_radiobutton_simple_toggled (object o, EventArgs args)
	{
		vbox_limited.Sensitive = false;	
		hbox_fixed.Sensitive = false;	
		label_distance.Text = "Distance";
	}
	
	void on_radiobutton_interval_toggled (object o, EventArgs args)
	{
		vbox_limited.Sensitive = true;	
		if( ! radiobutton_unlimited.Active) {
			hbox_fixed.Sensitive = true;	
		}
		label_distance.Text = "Distance\nof a track";
	}
	
	void on_radiobutton_limited_tracks_or_time_toggled (object o, EventArgs args)
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
	
	void on_checkbutton_distance_fixed_clicked (object o, EventArgs args)
	{
		if(checkbutton_distance_fixed.Active) {
			spin_distance.Sensitive = true;
		} else {
			spin_distance.Sensitive = false;
		}
	}
	
	void on_entries_required_changed (object o, EventArgs args)
	{
		if(entry_name.Text.ToString().Length > 0) {
			button_accept.Sensitive = true;
		}
		else {
			button_accept.Sensitive = false;
		}
	}
		

	public Button Button_accept 
	{
		set { button_accept = value;	}
		get { return button_accept;	}
	}

}

