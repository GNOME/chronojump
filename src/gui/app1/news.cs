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
	[Widget] Gtk.RadioButton radio_news_language_english;
	[Widget] Gtk.RadioButton radio_news_language_spanish;
	[Widget] Gtk.Label label_news_title;
	[Widget] Gtk.Label label_news_description_and_link;
        [Widget] Gtk.Image image_news;

	Pixbuf image_news_pixbuf;

	private void news_fill (bool langEs)
	{
		if(newsAtDB_l.Count == 0)
			return;

		if(langEs)
			radio_news_language_spanish.Active = true;
		else
			radio_news_language_english.Active = true;

		News news = newsAtDB_l[0];

		news_setLabels(news, langEs);
		news_loadImage(news);

		alignment_news.Show(); // is hidden at beginning to allow being well shown when filled
	}

	private void news_setLabels(News news, bool langEs)
	{
		label_news_title.Text = "<b>" + news.GetTitle(langEs) + "</b>";
		label_news_title.UseMarkup = true;
		label_news_description_and_link.Text = news.GetDescription(langEs) + "\n\n" + news.GetLink(langEs);
	}

	private void news_loadImage(News news)
	{
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
		if(newsAtDB_l.Count == 0)
			return;

		News news = newsAtDB_l[0];
		news_setLabels(news, langEs);
	}
}
