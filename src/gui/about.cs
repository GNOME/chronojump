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
 * Copyright (C) 2004-2017   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
using Gtk;
using Gdk;
using Glade;

public class About
{
	[Widget] Gtk.Dialog dialog_about;
	[Widget] Gtk.Image image_logo;
	[Widget] Gtk.Label dialog_about_label_chronojump;
	[Widget] Gtk.Label dialog_about_label_version;
	[Widget] Gtk.Image image_button_close;
	
	[Widget] Gtk.Label dialog_about_label_developers_software;
	[Widget] Gtk.Label dialog_about_label_developers_networks;
	[Widget] Gtk.Label dialog_about_label_developers_chronopic;
	[Widget] Gtk.Label dialog_about_label_developers_scientific;

	[Widget] Gtk.Label dialog_about_label_documenters;
	[Widget] Gtk.Label dialog_about_label_translators;

	public About (string version, string translators)
	{
		Glade.XML gladeXML;
		gladeXML = Glade.XML.FromAssembly (Util.GetGladePath() + "dialog_about.glade", "dialog_about", "chronojump");
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

		//images:
		Pixbuf pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + Constants.FileNameLogo);
		image_logo.Pixbuf = pixbuf;

		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_cancel.png");
		image_button_close.Pixbuf = pixbuf;

		dialog_about_label_version.Text = version; 
		dialog_about_label_translators.Text = translators; 

		//white bg
		//dialog_about.ModifyBg(StateType.Normal, new Gdk.Color(0xff,0xff,0xff));

		//manage window color
		if(! Config.UseSystemColor)
		{
			UtilGtk.DialogColor(dialog_about, Config.ColorBackground);
			UtilGtk.ContrastLabelsLabel(Config.ColorBackgroundIsDark, dialog_about_label_chronojump);
			UtilGtk.ContrastLabelsLabel(Config.ColorBackgroundIsDark, dialog_about_label_version);
		}
		
		//put authors
		textLabel(Constants.Authors(Constants.AuthorsEnum.SOFTWARE), dialog_about_label_developers_software);
		textLabel(Constants.Authors(Constants.AuthorsEnum.NETWORKS), dialog_about_label_developers_networks);
		textLabel(Constants.Authors(Constants.AuthorsEnum.CHRONOPIC), dialog_about_label_developers_chronopic);
		textLabel(Constants.Authors(Constants.AuthorsEnum.SCIENTIFIC), dialog_about_label_developers_scientific);

		//put documenters separated by commas
		string docsString = "";
		string paragraph = "";
		foreach (string doc in Constants.Authors(Constants.AuthorsEnum.DOCUMENTERS)) {
			docsString += paragraph;
			docsString += doc;
			paragraph = "\n";
		}
		dialog_about_label_documenters.Text = docsString;
		dialog_about.Show();
	}

	private void textLabel(string [] text, Gtk.Label label) {
		string str = "";
		foreach (string singleAuthor in text)
			str += singleAuthor;
		label.Text = str;
	}

	public void on_button_close_clicked (object obj, EventArgs args) {
		dialog_about.Destroy ();
	}

	private void on_delete_event (object o, DeleteEventArgs args) {
		dialog_about.Destroy ();
	}
}

