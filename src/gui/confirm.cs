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


public class ConfirmWindowJump
{
	[Widget] Gtk.Window confirm_window;
	[Widget] Gtk.Label label1;
	[Widget] Gtk.Label label2;
	[Widget] Gtk.Button button_accept;

	Gtk.Window parent;
	
	string table;
	int uniqueID;
	bool isRj;
	static ConfirmWindowJump ConfirmWindowJumpBox;
	
	public ConfirmWindowJump (Gtk.Window parent, string text1, string text2, string table, int uniqueID, bool isRj)
	{
		//Setup (text, table, uniqueID);
		Glade.XML gladeXML = Glade.XML.FromAssembly ("chronojump.glade", "confirm_window", null);

		gladeXML.Autoconnect(this);
		this.parent = parent;
		
		label1.Text = text1;
		label2.Text = text2;
		this.table = table;
		this.uniqueID = uniqueID;
		this.isRj = isRj;
	}

	static public ConfirmWindowJump Show (Gtk.Window parent, string text1, string text2, string table, int uniqueID, bool isRj)
	{
		if (ConfirmWindowJumpBox == null) {
			ConfirmWindowJumpBox = new ConfirmWindowJump(parent, text1, text2, table, uniqueID, isRj);
		}
		ConfirmWindowJumpBox.confirm_window.Show ();
		
		return ConfirmWindowJumpBox;
	}
	
	protected void on_button_cancel_clicked (object o, EventArgs args)
	{
		ConfirmWindowJumpBox.confirm_window.Hide();
		ConfirmWindowJumpBox = null;
	}
	
	protected void on_delete_selected_jump_delete_event (object o, EventArgs args)
	{
		ConfirmWindowJumpBox.confirm_window.Hide();
		ConfirmWindowJumpBox = null;
	}
	

	protected void on_button_accept_clicked (object o, EventArgs args)
	{
		if (table == "jump") {
			if (isRj) {
				SqliteJump.RjDelete(uniqueID.ToString());
			} else {
				SqliteJump.Delete(uniqueID.ToString());
			}
		} else if (table == "person") {
		} else if (table == "session") {
		} else {
			Console.WriteLine ("Error, table: {0}", table);
		}
		
		ConfirmWindowJumpBox.confirm_window.Hide();
		ConfirmWindowJumpBox = null;
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

	~ConfirmWindowJump() {}
	
}


public class ConfirmWindow
{
	[Widget] Gtk.Window confirm_window;
	[Widget] Gtk.Label label1;
	[Widget] Gtk.Label label2;
	[Widget] Gtk.Button button_accept;
	[Widget] Gtk.Button button_cancel;

	Gtk.Window parent;
	
	static ConfirmWindow ConfirmWindowBox;
	
	public ConfirmWindow (Gtk.Window parent, string text1, string text2)
	{
		Glade.XML gladeXML = Glade.XML.FromAssembly ("chronojump.glade", "confirm_window", null);

		gladeXML.Autoconnect(this);
		this.parent = parent;
		
		label1.Text = text1;
		label2.Text = text2;
	}

	static public ConfirmWindow Show (Gtk.Window parent, string text1, string text2)
	{
		if (ConfirmWindowBox == null) {
			ConfirmWindowBox = new ConfirmWindow(parent, text1, text2);
		}
		ConfirmWindowBox.confirm_window.Show ();
		
		return ConfirmWindowBox;
	}
	
	protected void on_button_cancel_clicked (object o, EventArgs args)
	{
		ConfirmWindowBox.confirm_window.Hide();
		ConfirmWindowBox = null;
	}
	
	protected void on_delete_selected_jump_delete_event (object o, EventArgs args)
	{
		ConfirmWindowBox.confirm_window.Hide();
		ConfirmWindowBox = null;
	}
	

	protected void on_button_accept_clicked (object o, EventArgs args)
	{
		ConfirmWindowBox.confirm_window.Hide();
		ConfirmWindowBox = null;
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

	public Button Button_cancel 
	{
		get {
			return button_cancel;
		}
	}

	~ConfirmWindow() {}
	
}

