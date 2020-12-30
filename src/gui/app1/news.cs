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
 * Copyright (C) 2020   Xavier de Blas <xaviblas@gmail.com>
 */

using System;
using Gdk;  //pixbuf
using Glade;
using System.IO;
using System.Collections.Generic; //List<T>

public partial class ChronoJumpWindow
{
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

	Pixbuf image_news_pixbuf;
	private int currentNewsPos;

	private void news_setup(int currentPos)
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
		news_fill (true);
	}
	private void on_button_news_older_clicked (object o, EventArgs args)
	{
		//just a precaution
		if(currentNewsPos +1 >= newsAtDB_l.Count)
			return;

		currentNewsPos ++;
		newsNewerOlderUpdateWidgets();
		news_fill (true);
	}

	// <----------------- end of newer, older management ----------


	private void news_fill (bool textAndVideo)
	{

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
		label_news_description_and_link.Text = news.GetDescription(langEs) + "\n\n" + news.GetLink(langEs);
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
			Pixbuf pixbuf = new Pixbuf (filename);
			image_news.Pixbuf = pixbuf;
			LogB.Information("exists");
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
		news_fill (false);
	}

	private void on_button_new_open_browser_clicked (object o, EventArgs args)
	{
		string link = newsAtDB_l[currentNewsPos].GetLink(preferences.newsLanguageEs);

		LogB.Information("Opening browser (r mac install) to: " + link);
		label_news_open_error.Visible = ! Util.OpenFolder(link);
	}
}
