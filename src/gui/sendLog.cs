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
 * Copyright (C) 2017   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
using System.IO;
using Gtk;
using Gdk;
using Glade;
using Mono.Unix;

public partial class ChronoJumpWindow
{
	[Widget] Gtk.Box hbox_send_log;
	[Widget] Gtk.Label label_send_log;
	[Widget] Gtk.Image image_send_log_no;
	[Widget] Gtk.Image image_send_log_yes;
	[Widget] Gtk.Entry entry_send_log;
	[Widget] Gtk.TextView textview_comments;
	[Widget] Gtk.Button button_send_log;
	[Widget] Gtk.Label label_send_log_message;

	string emailStored;
	private void show_send_log(string sendLogMessage) 
	{
		label_send_log.Text = sendLogMessage;
		emailStored = SqlitePreferences.Select("email");
		if(emailStored != null && emailStored != "" && emailStored != "0")
			entry_send_log.Text = emailStored;
		
		hbox_send_log.Show();
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

	private void on_button_check_last_version_clicked (object o, EventArgs args)
	{
		Json js = new Json();
		bool success = js.GetLastVersion(progVersion);

		if(success) {
			LogB.Information(js.ResultMessage);
			label_send_log_message.Text = js.ResultMessage;
		}
		else {
			LogB.Error(js.ResultMessage);
			label_send_log_message.Text = js.ResultMessage;
		}
	}

	private void on_button_open_chronojump_clicked(object o, EventArgs args)
	{
		notebook_start.CurrentPage = 0;
	}
}
