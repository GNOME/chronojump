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
 * Copyright (C) 2004-2023   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
using Gtk;
using Gdk;

public class About
{
	 Gtk.Dialog dialog_about;
	 Gtk.Image image_logo;
	 Gtk.Label dialog_about_label_chronojump;
	 Gtk.Label dialog_about_label_version;
	 Gtk.Notebook notebook;
	 Gtk.Image image_button_close;
	
	 Gtk.Label dialog_about_label_developers_software;
	 Gtk.Label dialog_about_label_developers_networks;
	 Gtk.Label dialog_about_label_developers_chronopic;
	 Gtk.Label dialog_about_label_developers_scientific;

	 Gtk.Label dialog_about_label_documenters;
	 Gtk.Label dialog_about_label_translators;

	public About (string version, string translators)
	{
		/*
		Glade.XML gladeXML;
		gladeXML = Glade.XML.FromAssembly (Util.GetGladePath() + "dialog_about.glade", "dialog_about", "chronojump");
		gladeXML.Autoconnect(this);
		*/

		Gtk.Builder builder = new Gtk.Builder (null, Util.GetGladePath () + "dialog_about.glade", null);
		connectWidgets (builder);
		builder.Autoconnect (this);
	
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

			UtilGtk.WidgetColor (notebook, Config.ColorBackgroundShifted);
			UtilGtk.ContrastLabelsNotebook (Config.ColorBackgroundShiftedIsDark, notebook);
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

	private void connectWidgets (Gtk.Builder builder)
	{
		dialog_about = (Gtk.Dialog) builder.GetObject ("dialog_about");
		image_logo = (Gtk.Image) builder.GetObject ("image_logo");
		dialog_about_label_chronojump = (Gtk.Label) builder.GetObject ("dialog_about_label_chronojump");
		dialog_about_label_version = (Gtk.Label) builder.GetObject ("dialog_about_label_version");
		notebook = (Gtk.Notebook) builder.GetObject ("notebook");
		image_button_close = (Gtk.Image) builder.GetObject ("image_button_close");
		dialog_about_label_developers_software = (Gtk.Label) builder.GetObject ("dialog_about_label_developers_software");
		dialog_about_label_developers_networks = (Gtk.Label) builder.GetObject ("dialog_about_label_developers_networks");
		dialog_about_label_developers_chronopic = (Gtk.Label) builder.GetObject ("dialog_about_label_developers_chronopic");
		dialog_about_label_developers_scientific = (Gtk.Label) builder.GetObject ("dialog_about_label_developers_scientific");
		dialog_about_label_documenters = (Gtk.Label) builder.GetObject ("dialog_about_label_documenters");
		dialog_about_label_translators = (Gtk.Label) builder.GetObject ("dialog_about_label_translators");
	}
}
