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
 * Copyright (C) 2017-2021   Xavier de Blas <xaviblas@gmail.com>
 */

using System;
using System.IO;
using Gtk;
using Gdk;
using Glade;
using Mono.Unix;

public partial class ChronoJumpWindow
{
	//--- send log ----

	[Widget] Gtk.Box hbox_send_log;
	[Widget] Gtk.TextView textview_send_log;
	[Widget] Gtk.Image image_send_log_no;
	[Widget] Gtk.Image image_send_log_yes;
	[Widget] Gtk.Entry entry_send_log;
	[Widget] Gtk.RadioButton radio_log_catalan;
	[Widget] Gtk.RadioButton radio_log_spanish;
	[Widget] Gtk.RadioButton radio_log_english;
	[Widget] Gtk.RadioButton radio_log_portuguese;
	[Widget] Gtk.TextView textview_comments;
	[Widget] Gtk.Button button_send_log;
	[Widget] Gtk.Image image_button_send_log;
	[Widget] Gtk.Image image_button_check_last_version;
	[Widget] Gtk.Image image_button_open_chronojump;
	[Widget] Gtk.Image image_button_open_chronojump1;
	[Widget] Gtk.TextView textview_send_log_message;

	//--- send poll ----

	[Widget] Gtk.Label label_social_network_poll_question;
	[Widget] Gtk.VBox vbox_social_network_poll_answers;
	[Widget] Gtk.RadioButton radio_social_network_poll_nsnc;
	[Widget] Gtk.RadioButton radio_social_network_poll_facebook;
	[Widget] Gtk.RadioButton radio_social_network_poll_instagram;
	[Widget] Gtk.RadioButton radio_social_network_poll_tiktok;
	[Widget] Gtk.RadioButton radio_social_network_poll_twitch;
	[Widget] Gtk.RadioButton radio_social_network_poll_twitter;
	[Widget] Gtk.RadioButton radio_social_network_poll_youtube;
	[Widget] Gtk.RadioButton radio_social_network_poll_other;
	[Widget] Gtk.Entry entry_social_network_poll_other;
	[Widget] Gtk.Notebook notebook_social_network_poll_send_result;
	[Widget] Gtk.Button button_social_network_poll_send;
	[Widget] Gtk.Image image_button_social_network_poll_send;
	[Widget] Gtk.Image image_social_network_poll_send_yes;
	[Widget] Gtk.Image image_social_network_poll_send_no;
	[Widget] Gtk.Label label_social_network_poll_message;


	//-----------------
	//--- send log ----
	//-----------------

	string emailStored;
	private void show_send_log(string sendLogMessage, string logLanguage)
	{
		TextBuffer tb = new TextBuffer (new TextTagTable());
		tb.Text = sendLogMessage;
		textview_send_log.Buffer = tb;
		textview_send_log.WrapMode = Gtk.WrapMode.Word; //Wrap words on a TextViewiew! (cannot be done on Glade)

		emailStored = SqlitePreferences.Select("email");
		if(emailStored != null && emailStored != "" && emailStored != "0")
			entry_send_log.Text = emailStored;

		//set language radiobuttons
		if(logLanguage == "Catalan")
			radio_log_catalan.Active = true;
		else if(logLanguage == "Spanish")
			radio_log_spanish.Active = true;
		else if(logLanguage == "Portuguese")
			radio_log_portuguese.Active = true;
		else
			radio_log_english.Active = true;

		image_button_send_log.Pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "send_blue.png");
		image_button_check_last_version.Pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_list.png");
		image_button_open_chronojump.Pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "chronojump_kangaroo_icon_transp.png");

		hbox_send_log.Show();
	}

	private string get_send_log_language()
	{
		if(radio_log_catalan.Active)
			return "Catalan";
		else if(radio_log_spanish.Active)
			return "Spanish";
		else if(radio_log_portuguese.Active)
			return "Portuguese";
		else //default english //if(radio_log.english.Active)
			return "English";
	}

	private void on_button_send_log_clicked (object o, EventArgs args)
	{
		string email = entry_send_log.Text.ToString();
		//email can be validated with Util.IsValidEmail(string)
		//or other methods, but maybe there's no need of complexity now 

		//1st save email on sqlite
		if(email != null && email != "" && email != "0" && email != emailStored)
			SqlitePreferences.Update("email", email, false);

		//2nd add language as comments
		string language = get_send_log_language();
		SqlitePreferences.Update("crashLogLanguage", language, false);
		string comments = "Answer in: " + language + "\n";

		//3rd if there are comments, add them at the beginning of the file
		comments += textview_comments.Buffer.Text;
		
		//4th send Json
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

		TextBuffer tb = new TextBuffer (new TextTagTable());
		tb.Text = js.ResultMessage;
		textview_send_log_message.Buffer = tb;
	}

	private void on_button_check_last_version_clicked (object o, EventArgs args)
	{
		Json js = new Json();
		bool success = js.GetLastVersion(progVersion);

		if(success)
			LogB.Information(js.ResultMessage);
		else
			LogB.Error(js.ResultMessage);

		TextBuffer tb = new TextBuffer (new TextTagTable());
		tb.Text = js.ResultMessage;
		textview_send_log_message.Buffer = tb;
	}


	//-----------------
	//--- send poll ----
	//-----------------

	private void socialNetworkPollInit ()
	{
		//question in bold
		label_social_network_poll_question.Text = "<b>" + label_social_network_poll_question.Text + "</b>";
		label_social_network_poll_question.UseMarkup = true;

		image_button_social_network_poll_send.Pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "send_blue.png");
		image_button_open_chronojump1.Pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "chronojump_kangaroo_icon_transp.png");
	}

	private void on_radio_social_network_poll_toggled (object o, EventArgs args)
	{
		entry_social_network_poll_other.Sensitive = (o == (object) radio_social_network_poll_other);

		button_social_network_poll_send_sensitivity();
	}

	private void on_entry_social_network_poll_other_changed (object o, EventArgs args)
	{
		button_social_network_poll_send_sensitivity();
	}

	private void button_social_network_poll_send_sensitivity()
	{
		button_social_network_poll_send.Sensitive =
			! (radio_social_network_poll_other.Active && entry_social_network_poll_other.Text == "");
	}

	private void on_button_social_network_poll_send_clicked (object o, EventArgs args)
	{
		string socialNetwork = "";
		if(radio_social_network_poll_nsnc.Active)
			socialNetwork = "NSNC";
		else if(radio_social_network_poll_facebook.Active)
			socialNetwork = "Facebook";
		else if(radio_social_network_poll_instagram.Active)
			socialNetwork = "Instagram";
		else if(radio_social_network_poll_tiktok.Active)
			socialNetwork = "TikTok";
		else if(radio_social_network_poll_twitch.Active)
			socialNetwork = "Twitch";
		else if(radio_social_network_poll_twitter.Active)
			socialNetwork = "Twitter";
		else if(radio_social_network_poll_youtube.Active)
			socialNetwork = "Youtube";
		else if(radio_social_network_poll_other.Active)
			socialNetwork = Util.MakeValidSQLAndFileName(entry_social_network_poll_other.Text);

		Json js = new Json();
		bool success = js.SocialNetworkPoll(preferences.machineID, socialNetwork);

		notebook_social_network_poll_send_result.CurrentPage = 1;
		button_social_network_poll_send.Sensitive = false;
		vbox_social_network_poll_answers.Sensitive = false;

		preferences.socialNetwork = socialNetwork; //to manage if it has to be sent after ping
		SqlitePreferences.Update(SqlitePreferences.SocialNetwork, socialNetwork, false);

		if(success)
		{
			image_social_network_poll_send_yes.Show();
			image_social_network_poll_send_no.Hide();
			LogB.Information(js.ResultMessage);

			preferences.socialNetworkDatetime = UtilDate.ToFile(DateTime.Now);
			SqlitePreferences.Update(SqlitePreferences.SocialNetworkDatetime,
					UtilDate.ToFile(DateTime.Now), false);
		}
		/*
		   if not success (no network) but user clicked on nsnc and send,
		   then show green button and do not bother user again,
		   but try to send on next ping that value
		   */
		else if(radio_social_network_poll_nsnc.Active)
		{
			image_social_network_poll_send_yes.Show();
			image_social_network_poll_send_no.Hide();
			LogB.Information(js.ResultMessage);

			preferences.socialNetworkDatetime = "-1";
			SqlitePreferences.Update(SqlitePreferences.SocialNetworkDatetime,
					"-1", false);
		} else {
			image_social_network_poll_send_yes.Hide();
			image_social_network_poll_send_no.Show();
			LogB.Error(js.ResultMessage);

			preferences.socialNetworkDatetime = "-1";
			SqlitePreferences.Update(SqlitePreferences.SocialNetworkDatetime,
					"-1", false);
		}

		label_social_network_poll_message.Text = js.ResultMessage;
	}


	//------------------------------
	//--- send log and send poll----
	//------------------------------

	private void on_button_open_chronojump_clicked(object o, EventArgs args)
	{
		notebook_start.CurrentPage = Convert.ToInt32(notebook_start_pages.PROGRAM);

		if(preferences.loadLastModeAtStart &&
				preferences.lastMode != Constants.Modes.UNDEFINED && ! configChronojump.Compujump)
		{
			// 0) note this code is repeated on gui/app1/chronojump.cs public ChronoJumpWindow()
			// 1) to avoid impossibility to start Chronojump if there's any problem with this mode, first put this to false
			SqlitePreferences.Update(SqlitePreferences.LoadLastModeAtStart, false, false);

			// 2) change mode
			changeModeCheckRadios (preferences.lastMode);

			// 3) put preference to true again
			SqlitePreferences.Update(SqlitePreferences.LoadLastModeAtStart, true, false);
		} else {
			notebook_sup.CurrentPage = Convert.ToInt32(notebook_sup_pages.START);
			new ChronojumpLogo (notebook_chronojump_logo, drawingarea_chronojump_logo, viewport_chronojump_logo, preferences.logoAnimatedShow, preferences.fontType.ToString());
		}
	}
}
