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
 * Xavier de Blas: 
 */

using System;
using Gtk;
using Gdk;
using Glade;

public class DialogImageTest
{
	[Widget] Gtk.Dialog dialog_image_test;
	[Widget] Gtk.Image image_test;
	[Widget] Gtk.Label label_name_description;
	[Widget] Gtk.Label label_long_description;
	[Widget] Gtk.ScrolledWindow scrolledwindow28;

	public DialogImageTest (EventType myEventType)
	{
		Glade.XML gladeXML;
		gladeXML = Glade.XML.FromAssembly (Util.GetGladePath() + "chronojump.glade", "dialog_image_test", null);
		gladeXML.Autoconnect(this);
		
		//put an icon to window
		UtilGtk.IconWindow(dialog_image_test);

		label_name_description.Text = "<b>" + myEventType.Name + "</b>" + " - " + myEventType.Description; 
		label_name_description.UseMarkup = true; 

		if(myEventType.LongDescription.Length == 0)
			scrolledwindow28.Hide();
		else {
			label_long_description.Text = myEventType.LongDescription; 
			label_long_description.UseMarkup = true; 
		}

                Pixbuf pixbuf = new Pixbuf (null, Util.GetImagePath(false) + myEventType.ImageFileName);
                image_test.Pixbuf = pixbuf;
	}

	//useful to show only an image	
	//in a future do a DialogImage class (with this). And the inherit to DialogImageTest
	public DialogImageTest (string title, string imagePath)
	{
		Glade.XML gladeXML;
		gladeXML = Glade.XML.FromAssembly (Util.GetGladePath() + "chronojump.glade", "dialog_image_test", null);
		gladeXML.Autoconnect(this);
		
		dialog_image_test.Title = title;

		//put an icon to window
		UtilGtk.IconWindow(dialog_image_test);

		scrolledwindow28.Hide();

                Pixbuf pixbuf = new Pixbuf (imagePath);
                image_test.Pixbuf = pixbuf;
	}
				
	public void on_close_button_clicked (object obj, EventArgs args) {
		dialog_image_test.Destroy ();
	}

	private void on_delete_event (object o, DeleteEventArgs args) {
		dialog_image_test.Destroy ();
	}
}
