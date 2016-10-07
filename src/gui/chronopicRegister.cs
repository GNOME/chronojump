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
 *  Copyright (C) 2016   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
using Gtk;

public class ChronopicRegisterWindow
{
	Gtk.Window chronopic_register_win;
	Gtk.Table table;

	public ChronopicRegisterWindow() 
	{
		createWindow();
		//put an icon to window
		UtilGtk.IconWindow(chronopic_register_win);

		createTable();
		chronopic_register_win.ShowAll();
	}


	private void createWindow()
	{
		chronopic_register_win = new Window ("Chronopic register");

		/* Set a handler for delete_event that immediately
		 *                          * exits GTK. */ //TODO: change this
//		chronopic_register_win.DeleteEvent += delete_event;

		/* Sets the border width of the window. */
		chronopic_register_win.BorderWidth= 20;
	}

	private void createTable()
	{
		table = new Table (3, 3, true); //rows, columns

		createCells();

		/* Put the table in the main window */
		chronopic_register_win.Add(table);

		table.Show();
	}

	private void createCells()
	{
		//headers
		Label header1 = new Label("<b>SerialNumber</b>");
		Label header2 = new Label("<b>Type</b>");
		Label header3 = new Label("<b>Port</b>");
		header1.UseMarkup = true;
		header2.UseMarkup = true;
		header3.UseMarkup = true;
		table.Attach(header1, 0, 1, 0, 1); //left, right, top, bottom
		table.Attach(header2, 1, 2, 0, 1);
		table.Attach(header3, 2, 3, 0, 1);
		//TODO: negretes


		Label serialN1 = new Label("aa00");
		table.Attach(serialN1, 0, 1, 1, 2);

		Label serialN2 = new Label("aa01");
		table.Attach(serialN2, 0, 1, 2, 3);

		
		ComboBox combo1 = ComboBox.NewText ();
		foreach(string s in Enum.GetNames(typeof(ChronopicRegisterPort.Types)))
			combo1.AppendText (s);
		combo1.Active = 0;
		table.Attach(combo1, 1, 2, 1, 2);

		ComboBox combo2 = ComboBox.NewText ();
		foreach(string s in Enum.GetNames(typeof(ChronopicRegisterPort.Types)))
			combo2.AppendText (s);
		combo2.Active = 0;
		table.Attach(combo2, 1, 2, 2, 3);


		Label port1 = new Label("COM1");
		table.Attach(port1, 2, 3, 1, 2);

		Label port2 = new Label("COM2");
		table.Attach(port2, 2, 3, 2, 3);
	}
}

