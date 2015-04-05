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
 * Copyright (C) 2004-2015   Xavier de Blas <xaviblas@gmail.com> 
 */


using System;
using Gtk;
using Glade;

public class ChronopicDialogAuto
{
	[Widget] Gtk.Dialog dialog_chronopic_auto;
	[Widget] public Gtk.ProgressBar progressbar;
	[Widget] public Gtk.Button button_cancel;

	public ChronopicDialogAuto ()
	{
		Glade.XML gladeXML;
		gladeXML = Glade.XML.FromAssembly (Util.GetGladePath() + "chronojump.glade", "dialog_chronopic_auto", "chronojump");
		gladeXML.Autoconnect(this);

		//put an icon to window
		UtilGtk.IconWindow(dialog_chronopic_auto);

		LogB.Information("ChronopicDialogAuto");
	}
	
	private void on_button_cancel_clicked (object o, EventArgs args) {
		dialog_chronopic_auto.Destroy ();
	}

	private void on_delete_event (object o, DeleteEventArgs args) {
		button_cancel.Click();
		dialog_chronopic_auto.Destroy ();
	}
}

