/*
 * This file is part of ChronoJump
 *
 * Chronojump is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or   
 *    (at your option) any later version.
 *    
 * Chronojump is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the 
 *    GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 *  along with this program; if not, write to the Free Software
 *   Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 *
 * Copyright (C) 2021   Xavier de Blas <xaviblas@gmail.com>
 */

using System;
using Gdk;  //pixbuf
using Glade;
using System.IO;
using System.Collections.Generic; //List<T>
using System.Threading;

public partial class ChronoJumpWindow
{
	[Widget] Gtk.Notebook notebook_news;
	[Widget] Gtk.ProgressBar progressbar_news_get;
	[Widget] Gtk.Image image_news_blue;
	[Widget] Gtk.Image image_news_yellow;
	[Widget] Gtk.Label label_news_frame;
	[Widget] Gtk.Alignment alignment_news;
	[Widget] Gtk.HBox hbox_news_newer_older;
	[Widget] Gtk.Button button_news_newer;
	[Widget] Gtk.Button button_news_older;
	[Widget] Gtk.Label label_news_current_all;
	[Widget] Gtk.RadioButton radio_news_language_english;
	[Widget] Gtk.RadioButton radio_news_language_spanish;
	[Widget] Gtk.Label label_news_title;
	[Widget] Gtk.Label label_news_description_and_link;
	[Widget] Gtk.Label label_news_open_error;
        [Widget] Gtk.Image image_news;
        [Widget] Gtk.Image image_news_close;
	[Widget] Gtk.Alignment alignment_above_news; //to align vertically with sidebar
	[Widget] Gtk.HBox hbox_news_languages; //to align vertically with sidebar

	Pixbuf image_news_pixbuf;
	private int currentNewsPos;
	private bool newsDownloadCancel;


	private void newsGetThreadPrepare()
	{
		// 1) select the news locally
		newsAtDB_l = SqliteNews.Select(false, -1, 10);

		// 2) check if any of the images is missing (because or images server is down or any other reason)
		bool allImagesSaved = true;
		foreach(News news in newsAtDB_l)
			if(! news.ImageSavedOnDisc)
			{
				allImagesSaved = false;
				break;
			}


		// 3) prepare the GUI
		alignment_news.Show(); // is hidden at beginning to allow being well shown when filled
		menus_and_mode_sensitive(false);
		app1s_notebook_sup_entered_from = notebook_sup.CurrentPage;
		notebook_sup.CurrentPage = Convert.ToInt32(notebook_sup_pages.NEWS);

		// 4) get the news on the server and/or display them
		if(
				(
				 preferences.serverNewsDatetime != null &&
				 preferences.serverNewsDatetime != "" &&
				 preferences.serverNewsDatetime != preferences.clientNewsDatetime
				 ) || allImagesSaved == false )
		{
			newsDownloadCancel = false;
			LogB.Information("newsGet thread will start");
			pingThread = new Thread (new ThreadStart (newsGet));
			GLib.Idle.Add (new GLib.IdleHandler (pulseNewsGetGTK));
			pingThread.Start();
		} else {
			// 4b) or display old news
			newsDisplay();
		}
	}

	//No GTK here
	private void newsGet()
	{
		newsAtServer_l = jsPing.GetNews(newsAtDB_l); //send the local list to know if images have to be re-downloaded on a version update
	}
	private bool pulseNewsGetGTK ()
	{
		if(! pingThread.IsAlive || newsDownloadCancel)
		{
			if(newsDownloadCancel)
			{
				menus_and_mode_sensitive(true);
				notebook_sup.CurrentPage = app1s_notebook_sup_entered_from;

				LogB.Information("pulseNewsGetGTK ending here");
				LogB.ThreadEnded();

				return false;
			}

			if(newsAtServer_l != null)
			{
				// 1) update clientNewsDatetime
				preferences.clientNewsDatetime = preferences.serverNewsDatetime;
				SqlitePreferences.Update(SqlitePreferences.ClientNewsDatetime, preferences.clientNewsDatetime, false);

				// 2) insert/update on SQL if needed
				News.InsertOrUpdateIfNeeded (newsAtDB_l, newsAtServer_l);
			}

			// 3) end this pulse
			LogB.Information("pulseNewsGetGTK ending here");
			LogB.ThreadEnded();

			if(newsAtServer_l != null)
				newsDisplay();

			return false;
		}

		notebook_news.Page = 0;
		progressbar_news_get.Pulse();
		Thread.Sleep (100);
		//Log.Write(" (pulseNewsGetGTK:" + thread.ThreadState.ToString() + ") ");
		return true;
	}

	private void on_button_news_cancel_clicked (object o, EventArgs args)
	{
		newsDownloadCancel = true;

		if(pingThread.IsAlive)
			pingThread.Abort();
	}

	private void newsDisplay()
	{
		// 1) select
		newsAtDB_l = SqliteNews.Select(false, -1, 10);

		/*
		//debug stuff
		foreach(News news in newsAtDB_l)
			LogB.Information(news.ToString());
			*/

		// 2) fill the widgets
		news_setup_gui(0); //setup radios: language and arrows
		news_fill_gui(true); //fill the widget

		// 3) show the news tab
		notebook_news.Page = 1;

		//align with sidebar
		if(preferences.personWinHide)
			alignment_above_news.TopPadding = (uint) (alignment_menu_tiny.TopPadding
					- hbox_news_languages.SizeRequest().Height
					-1); //alignment_news_languages.BottomPadding
		else
			alignment_above_news.TopPadding = (uint) (alignment_session_persons.TopPadding
					+ hbox_above_frame_session.SizeRequest().Height
					- hbox_news_languages.SizeRequest().Height
					-1); //alignment_news_languages.BottomPadding
	}

	private void news_setup_gui(int currentPos)
	{
		currentNewsPos = currentPos;

		if(preferences.newsLanguageEs)
			radio_news_language_spanish.Active = true;
		else
			radio_news_language_english.Active = true;

		news_setNewerOlderStuff();
	}


	// ----------------- newer, older management ---------->
	/*
	 * code 1 is the first news in the server (the older)
	 * but newsAtDB_l is a SELECT ORDER BY code DESC
	 * so if there are three news, currentNewsPos == 0 should be the newer (that should be code 3, if there have not been removals on server)
	 */
	private void news_setNewerOlderStuff()
	{
		//do not show newer/older controls if there is one product
		if(newsAtDB_l.Count < 2)
			return;

		hbox_news_newer_older.Visible = true;
		newsNewerOlderUpdateWidgets();
	}

	private void newsNewerOlderUpdateWidgets()
	{
		label_news_current_all.Text = string.Format("{0} / {1}", currentNewsPos +1, newsAtDB_l.Count);

		button_news_newer.Sensitive = currentNewsPos > 0;
		button_news_older.Sensitive = currentNewsPos +1 < newsAtDB_l.Count;
	}

	private void on_button_news_newer_clicked (object o, EventArgs args)
	{
		//just a precaution
		if(currentNewsPos <= 0)
			return;

		currentNewsPos --;
		newsNewerOlderUpdateWidgets();
		news_fill_gui (true);
	}
	private void on_button_news_older_clicked (object o, EventArgs args)
	{
		//just a precaution
		if(currentNewsPos +1 >= newsAtDB_l.Count)
			return;

		currentNewsPos ++;
		newsNewerOlderUpdateWidgets();
		news_fill_gui (true);
	}

	// <----------------- end of newer, older management ----------


	private void news_fill_gui (bool textAndVideo)
	{
		//LogB.Information(string.Format("newsAtDB_l: {0}, newsAtDB_l.Count: {1}, currentNewsPos: {2}",
		//			newsAtDB_l, newsAtDB_l.Count, currentNewsPos));
		//just a precaution
		if(newsAtDB_l == null || currentNewsPos >= newsAtDB_l.Count)
			return;

		News news = newsAtDB_l[currentNewsPos];

		news_setLabels(news, preferences.newsLanguageEs);

		if(textAndVideo)
			news_loadImage(news);

		//hide the error opening web (if it is visible)
		label_news_open_error.Visible = false;
	}

	private void news_setLabels(News news, bool langEs)
	{
		label_news_title.Text = "<b>" + news.GetTitle(langEs) + "</b>";
		label_news_title.UseMarkup = true;

		label_news_description_and_link.Visible = false;
		label_news_description_and_link.Text = news.GetDescription(langEs) + "\n\n" + news.GetLink(langEs);
		label_news_description_and_link.Visible = true;
	}

	private void news_loadImage(News news)
	{
		LogB.Information("news_loadImage: " + news.ToString());
		//TODO: share method for getting extension
		string extension = "";
		if(Util.IsJpeg(news.LinkServerImage))
			extension = ".jpg";
		else if (Util.IsPng(news.LinkServerImage))
			extension = ".png";

		string filename = Path.Combine(News.GetNewsDir(), news.Code.ToString() + extension);
		LogB.Information("news image filename: " + filename);
		if(File.Exists(filename))
		{
			LogB.Information("exists");
			//Pixbuf pixbuf = new Pixbuf (filename);
			//image_news.Pixbuf = pixbuf;
			image_news.Pixbuf = UtilGtk.OpenPixbufSafe(filename, image_news.Pixbuf);
			LogB.Information("opened");
		}
	}

	private void on_radio_news_language_english_toggled (object o, EventArgs args)
	{
		newsLanguageRadioChanged(false);
	}
	private void on_radio_news_language_spanish_toggled (object o, EventArgs args)
	{
		newsLanguageRadioChanged(true);
	}
	private void newsLanguageRadioChanged (bool langEs)
	{
		// 1) update preferences.newsLanguageEs and also SQL
		Sqlite.Open();
		preferences.newsLanguageEs = Preferences.PreferencesChange(
				SqlitePreferences.NewsLanguageEs, preferences.newsLanguageEs,
				langEs);
		Sqlite.Close();

		// 2) rewrite the labels
		news_fill_gui (false);
	}

	private void on_button_new_open_browser_clicked (object o, EventArgs args)
	{
		//just a precaution
		if(newsAtDB_l == null || currentNewsPos < 0 || currentNewsPos >= newsAtDB_l.Count)
			return;

		string link = newsAtDB_l[currentNewsPos].GetLink(preferences.newsLanguageEs);

		LogB.Information("Opening browser (r mac install) to: " + link);
		label_news_open_error.Visible = ! Util.OpenURL (link);
	}
}
