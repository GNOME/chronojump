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
	[Widget] Gtk.Label label_news_title;
	[Widget] Gtk.Label label_news_description_and_link;
        [Widget] Gtk.Image image_news;

	Pixbuf image_news_pixbuf;

	private void news_fill (List<News> news_l)
	{
		if(news_l.Count == 0)
			return;

		News news = news_l[0];

		label_news_title.Text = "<b>" + news.GetTitle(false) + "</b>";
		label_news_title.UseMarkup = true;
		label_news_description_and_link.Text = news.GetDescription(false) + "\n\n" + news.GetLink(false);

		news_loadImage(news);
		alignment_news.Show(); // is hidden at beginning to allow being well shown when filled
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

}
