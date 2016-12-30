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
 * Copyright (C) 2016   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
using Gtk;
using Gdk;
using Glade;
using Mono.Unix;

public class DialogThreshold
{
	[Widget] Gtk.Dialog dialog_threshold;
	[Widget] Gtk.Notebook notebook;
	[Widget] Gtk.TextView textview_about;
	[Widget] Gtk.TextView textview_jumps;
	[Widget] Gtk.TextView textview_races;
	[Widget] Gtk.TextView textview_other;

	public DialogThreshold (Constants.Menuitem_modes m)
	{
		Glade.XML gladeXML;
		gladeXML = Glade.XML.FromAssembly (Util.GetGladePath() + "dialog_threshold.glade", "dialog_threshold", null);
		gladeXML.Autoconnect(this);
		
		//put an icon to window
		UtilGtk.IconWindow(dialog_threshold);

		writeTexts();

		if(m == Constants.Menuitem_modes.JUMPSSIMPLE || m == Constants.Menuitem_modes.JUMPSREACTIVE)
			notebook.CurrentPage = 0;
                else if(m == Constants.Menuitem_modes.RUNSSIMPLE || m == Constants.Menuitem_modes.RUNSINTERVALLIC)
			notebook.CurrentPage = 1;
		else 	//other
			notebook.CurrentPage = 2;
	}

	private void writeTexts()
	{
		TextBuffer tb_about = new TextBuffer (new TextTagTable());
		tb_about.Text =  Catalog.GetString("Spurius signals are common on electronics.") +
			"\n\n" + Catalog.GetString("Threshold refers to the minimum value measurable and is the common way to clean this spurius signals.") +
			"\n"   + Catalog.GetString("Threshold should be a value lower than expected values.") +

			"\n\n" + Catalog.GetString("On database three different thresholds are stored: jumps, races and other tests.") +
			"\n"  +  Catalog.GetString("If you change this values they will be stored once test is executed.") +

			"\n\n" + Catalog.GetString("Usually threshold values should not be changed but this option is useful for special cases.");
		textview_about.Buffer = tb_about;

		TextBuffer tb_jumps = new TextBuffer (new TextTagTable());
		tb_jumps.Text =  Catalog.GetString("Default value: 50 ms.") +
			"\n\n" + Catalog.GetString("On jumps with contact platforms a value of 50 ms (3 cm jump approximately) is enough to solve electronical problems.") +
			"\n\n" + Catalog.GetString("You may change this value if you have a jumper that looses pressure with the platform while going down on the eccentric phase previous to a CMJ or ABK jump.") +
			"\n" +   Catalog.GetString("This jumper should change his technique, but if it's difficult, a solution is to increase threshold.");
		textview_jumps.Buffer = tb_jumps;

		TextBuffer tb_races = new TextBuffer (new TextTagTable());
		tb_races.Text =  Catalog.GetString("Default value: 10 ms.") +
			"\n\n" + Catalog.GetString("On races with photocells a value of 10 ms is the default value.") +
			"\n\n" + Catalog.GetString("As Chronojump manages double contacts on photocells, changing threshold value is not very common.");
		textview_races.Buffer = tb_races;

		TextBuffer tb_other = new TextBuffer (new TextTagTable());
		tb_other.Text =  Catalog.GetString("Default value: 50 ms.") +
			"\n\n" + Catalog.GetString("Depending on the test, user could change values.");
		textview_other.Buffer = tb_other;
	}

	public void on_button_close_clicked (object obj, EventArgs args) {
		dialog_threshold.Destroy ();
	}

	private void on_delete_event (object o, DeleteEventArgs args) {
		dialog_threshold.Destroy ();
	}
}
