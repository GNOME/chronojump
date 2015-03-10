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
 * Copyright (C) 2004-2015   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
using System.IO;
using Gtk;
using Glade;
//using Gnome;
using GLib; //for Value
using System.Text; //StringBuilder
using System.Collections; //ArrayList
using Mono.Unix;

public class ErrorWindow
{
	[Widget] Gtk.Window error_window;
	[Widget] Gtk.Label label1;
	[Widget] Gtk.Button button_abort; //used only when there are problems because USB has been removed on the middle of the test
	[Widget] Gtk.Button button_accept;
	[Widget] Gtk.Box hbox_send_log;
	[Widget] Gtk.Button button_open_database_folder;
	[Widget] Gtk.Button button_open_docs_folder;
	[Widget] Gtk.Image image_send_log_no;
	[Widget] Gtk.Image image_send_log_yes;
	[Widget] Gtk.Entry entry_send_log;
	[Widget] Gtk.TextView textview_comments;
	[Widget] Gtk.Button button_send_log;
	[Widget] Gtk.Label label_send_log_message;

	string table;
	static ErrorWindow ErrorWindowBox;
	
	public ErrorWindow (string text1)
	{
		LogB.Information("At error window2");
		Glade.XML gladeXML;
		gladeXML = Glade.XML.FromAssembly (Util.GetGladePath() + "chronojump.glade", "error_window", "chronojump");
		gladeXML.Autoconnect(this);
		
		//put an icon to window
		UtilGtk.IconWindow(error_window);
		
		label1.Text = text1;
		label1.UseMarkup = true;
	}

	static public ErrorWindow Show (string text1)
	{
		LogB.Information("At error window");
		if (ErrorWindowBox == null) {
			ErrorWindowBox = new ErrorWindow(text1);
		}
		
		//hidden always excepted when called to be shown (see below)
		ErrorWindowBox.hbox_send_log.Hide();
		ErrorWindowBox.button_open_database_folder.Hide();

		ErrorWindowBox.error_window.Show();
		
		return ErrorWindowBox;
	}
	
	void on_delete_window_event (object o, DeleteEventArgs args)
	{
		/*
		ErrorWindowBox.error_window.Hide();
		ErrorWindowBox = null;
		*/
		
		//need this for continue execution on chronojump startup. See src/chronojump.cs
		button_accept.Click();
	}

	string emailStored;
	public void Show_send_log() 
	{
		emailStored = SqlitePreferences.Select("email");
		if(emailStored != null && emailStored != "" && emailStored != "0")
			entry_send_log.Text = emailStored;
		
		hbox_send_log.Show();
		
		//button_send_log.Label = "Disabled";
		//button_send_log.Sensitive = false;
	}
	private void on_button_send_log_clicked (object o, EventArgs args)
	{
		string email = entry_send_log.Text.ToString();
		//email can be validated with Util.IsValidEmail(string)
		//or other methods, but maybe there's no need of complexity now 

		//1st save email on sqlite
		if(email != null && email != "" && email != "0" && email != emailStored)
			SqlitePreferences.Update("email", email, false);

		//2nd if there are comments, add them at the beginning of the file
		string comments = textview_comments.Buffer.Text;
		
		//2nd send Json
		Json js = new Json();
		bool success = js.PostCrashLog(email, comments);
		
		if(success) {
			button_send_log.Label = Catalog.GetString("Thanks");
			button_send_log.Sensitive = false;

			image_send_log_yes.Show();
			image_send_log_no.Hide();
			LogB.Information(js.ResultMessage);
		} else {
			button_send_log.Label = Catalog.GetString("Try again");
			
			image_send_log_yes.Hide();
			image_send_log_no.Show();
			LogB.Error(js.ResultMessage);
		}

		label_send_log_message.Text = js.ResultMessage;
	}

	public void Show_button_open_database_folder () {
		button_open_database_folder.Show();
	}
	private void on_button_open_database_folder_clicked (object o, EventArgs args)
	{
		string database_url = Util.GetDatabaseDir() + System.IO.Path.DirectorySeparatorChar  + "chronojump.db";
		string database_temp_url = Util.GetDatabaseTempDir() + System.IO.Path.DirectorySeparatorChar  + "chronojump.db";
		
		System.IO.FileInfo file1 = new System.IO.FileInfo(database_url); //potser cal una arrobar abans (a windows)
		System.IO.FileInfo file2 = new System.IO.FileInfo(database_temp_url); //potser cal una arrobar abans (a windows)

		if(file1.Exists)
			System.Diagnostics.Process.Start(Util.GetDatabaseDir()); 
		else if(file2.Exists)
			System.Diagnostics.Process.Start(Util.GetDatabaseTempDir()); 
		else
			new DialogMessage(Constants.MessageTypes.WARNING, Constants.DatabaseNotFound);
	}

	public void Show_button_open_docs_folder () {
		button_open_docs_folder.Show();
	}
	private void on_button_open_docs_folder_clicked (object o, EventArgs args)
	{
		LogB.Information("Opening docs at: " + Path.GetFullPath(Util.GetManualDir())); 
		try {
			System.Diagnostics.Process.Start(Path.GetFullPath(Util.GetManualDir())); 
		} catch {
			new DialogMessage(Constants.MessageTypes.WARNING, 
					"Sorry, folder does not exist." + "\n\n" +
					Path.GetFullPath(Util.GetManualDir())
					);
		}
	}
	
	public void Show_button_abort () {
		button_abort.Show();
	}
	private void on_button_abort_clicked (object o, EventArgs args) {

	}

	public void Button_accept_label (string str) {
		button_accept.Label = str;
	}

	protected void on_button_accept_clicked (object o, EventArgs args)
	{
		ErrorWindowBox.error_window.Hide();
		ErrorWindowBox = null;
	}

	public Button Button_accept {
		get { return button_accept; }
	} 	

	public Button Button_abort {
		get { return button_abort; }
	} 	
	
	public void HideAndNull() {
		ErrorWindowBox.error_window.Hide();
		ErrorWindowBox = null;
	}

	~ErrorWindow() {}
	
}

