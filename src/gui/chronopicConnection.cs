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
using GLib; //for Value
using System.Text; //StringBuilder
using System.Collections; //ArrayList
using Mono.Unix;


public class ChronopicConnection
{
	[Widget] Gtk.Window chronopic_connection;
	[Widget] Gtk.ProgressBar progressbar1;
	[Widget] Gtk.Label label_feedback;
	[Widget] Gtk.Button button_cancel;
	[Widget] Gtk.Button button_close;

	Gtk.Window parent;
	
	static ChronopicConnection ChronopicConnectionBox;
	
	public ChronopicConnection (Gtk.Window parent)
	{
		Glade.XML gladeXML;
		gladeXML = Glade.XML.FromAssembly (Util.GetGladePath() + "chronojump.glade", "chronopic_connection", null);
		gladeXML.Autoconnect(this);
		this.parent = parent;
	}

	static public ChronopicConnection Show (Gtk.Window parent)
	{
		if (ChronopicConnectionBox == null) {
			ChronopicConnectionBox = new ChronopicConnection(parent);
		}
		ChronopicConnectionBox.chronopic_connection.Show ();

		ChronopicConnectionBox.initialize ();
		
		return ChronopicConnectionBox;
	}

	private void initialize() {
		button_cancel.Sensitive = true;
		button_close.Sensitive = false;
		LabelFeedBackReset();
	}

	public void LabelFeedBackReset() {
		label_feedback.Text = "";
	}

	public void Pulse() {
		progressbar1.Pulse();
	}

	public void Connected(string message) {
		Console.WriteLine("CONNECTED!!");
		label_feedback.Text = message;
		button_cancel.Sensitive = false;
		button_close.Sensitive = true;
	}

	public void Disconnected(string message) {
		Console.WriteLine("DISCONNECTED!!");
		label_feedback.Text = message;
		button_cancel.Sensitive = false;
		button_close.Sensitive = true;
	}

	private void on_button_cancel_clicked (object o, EventArgs args)
	{
		//ChronopicConnectionBox.chronopic_connection.Hide();
		//ChronopicConnectionBox = null;
	}
	
	private void on_button_close_clicked (object o, EventArgs args)
	{
		ChronopicConnectionBox.chronopic_connection.Hide();
		//ChronopicConnectionBox = null;
	}

	private void on_delete_event (object o, DeleteEventArgs args)
	{
		//button_cancel.Click();

		ChronopicConnectionBox.chronopic_connection.Hide();
		ChronopicConnectionBox = null;
	}
	
	public Button Button_cancel 
	{
		set { button_cancel = value; }
		get { return button_cancel; }
	}

	public Button Button_close 
	{
		set { button_close = value; }
		get { return button_close; }
	}

	~ChronopicConnection() {}
	
}
