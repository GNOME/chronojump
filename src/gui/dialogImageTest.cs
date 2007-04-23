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
 * http://www.xdeblas.com, http://www.deporteyciencia.com (parleblas)
 */

using System;
using Gtk;
using Gdk;
using Glade;

public class DialogImageTest
{
	[Widget] Gtk.Dialog dialog_image_test;
	[Widget] Gtk.Label label_frame_test;
	[Widget] Gtk.Image image_test;
	[Widget] Gtk.Label label_test;

//	public DialogImageTest (string message)
	public DialogImageTest ()
	{
		Glade.XML gladeXML;
		try {
			gladeXML = Glade.XML.FromAssembly ("chronojump.glade", "dialog_image_test", null);
		} catch {
			gladeXML = Glade.XML.FromAssembly ("chronojump.glade.chronojump.glade", "dialog_image_test", null);
		}
		
		gladeXML.Autoconnect(this);
		
		label_frame_test.Text = "<b>Test XYZ</b>"; 
		label_frame_test.UseMarkup = true; 
		label_test.Text = "Explanation ZYZ"; 
                Pixbuf pixbuf = new Pixbuf (null, "agility_505.png");
                image_test.Pixbuf = pixbuf;
	}
				

	public void on_close_button_clicked (object obj, EventArgs args) {
		dialog_image_test.Destroy ();
	}

	private void on_delete_event (object o, DeleteEventArgs args) {
		dialog_image_test.Destroy ();
	}
}
