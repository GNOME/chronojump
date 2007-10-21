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
using Glade;

public class DialogHelp
{
	[Widget] Gtk.Dialog dialog_help;
	[Widget] Gtk.Label label_message;

	public DialogHelp (string message)
	{
		Glade.XML gladeXML;
		gladeXML = Glade.XML.FromAssembly (Util.GetGladePath() + "chronojump.glade", "dialog_help", null);
		gladeXML.Autoconnect(this);
		
		//put an icon to window
		UtilGtk.IconWindow(dialog_help);

		label_message.Text = message; 
	}
				

	public void on_close_button_clicked (object obj, EventArgs args) {
		dialog_help.Destroy ();
	}

	private void on_delete_event (object o, DeleteEventArgs args) {
		dialog_help.Destroy ();
	}
}
