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
 * Copyright (C) 2004-2009   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
using Gtk;
using Gdk;
using Glade;

public class About
{
	[Widget] Gtk.Dialog dialog_about;
	[Widget] Gtk.Image image_logo;
	[Widget] Gtk.Label dialog_about_label_version;
	[Widget] Gtk.Label dialog_about_label_developers;
	[Widget] Gtk.Label dialog_about_label_translators;

	public About (string version, string translators)
	{
		Glade.XML gladeXML;
		gladeXML = Glade.XML.FromAssembly (Util.GetGladePath() + "chronojump.glade", "dialog_about", null);
		gladeXML.Autoconnect(this);
	
		
		/*	
		//crash for test purposes
		string [] myCrash = {
			"hello" };
		Console.WriteLine("going to crash now intentionally");
		Console.WriteLine(myCrash[1]);
		*/



		//put an icon to window
		UtilGtk.IconWindow(dialog_about);

		//put logo image
		Pixbuf pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + Constants.FileNameLogo);
		image_logo.Pixbuf = pixbuf;

		dialog_about_label_version.Text = version; 
		dialog_about_label_translators.Text = translators; 
		
		//put authos separated by commas
		string authorsString = "";
		string paragraph = "";
		foreach (string singleAuthor in Constants.Authors) {
			authorsString += paragraph;
			authorsString += singleAuthor;
			paragraph = "\n\n";
		}
		dialog_about_label_developers.Text = authorsString;
	}
				

	public void on_button_close_clicked (object obj, EventArgs args) {
		dialog_about.Destroy ();
	}

	private void on_delete_event (object o, DeleteEventArgs args) {
		dialog_about.Destroy ();
	}
}

