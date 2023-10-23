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
 * Copyright (C) 2023   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
using Gtk;
using GLib; //for Value
//using System.Text; //StringBuilder
using System.Collections; //ArrayList

//here using app1sae_ , "sae" means session add edit
//this file has been moved from his old window to be part of app1 on Chronojump 2.0

public partial class ChronoJumpWindow
{
	public void on_button_import_from_csv (object o, EventArgs args)
	{
		app1s_notebook.CurrentPage = app1s_PAGE_IMPORT_FROM_CSV;

		on_app1s_import_data_type_toggled (o, args);
	}

	private void on_app1s_import_data_type_toggled (object o, EventArgs args)
	{
		TextBuffer tb1 = new TextBuffer (new TextTagTable());

		string str = "EXPECTED FORMAT:</b>";
		str += "\n- 1st ROW should be headers (will be discarded)";
		str += "\n- 1st COLUMN should be person names like in Chronojump. All should exist in session";

		if (app1s_import_jumps_simple.Active)
		{
			str += "\n- 2nd COLUMN should be jump simple type (should exist in Chronojump)";
			str += "\n- 3rd COLUMN should be jump height in seconds (will accept , or . as decimal).";
		} else if (app1s_import_jumps_multiple.Active) {
			str += "\n\nSorry, Not available yet!";
		} else if (app1s_import_runs_simple.Active) {
			str += "\n\nSorry, Not available yet!";
		} else if (app1s_import_runs_intervallic.Active) {
			str += "\n\nSorry, Not available yet!";
		}

                tb1.Text = str;
		app1s_textview_import_from_csv_format.Buffer = tb1;

		app1s_button_import_csv_select_and_import.Sensitive = app1s_import_jumps_simple.Active;
	}

	private void on_app1s_button_import_csv_select_and_import_clicked (object o, EventArgs args)
	{
		//TODO
	}

	private void on_app1s_button_import_from_csv_close_clicked (object o,EventArgs args)
	{
		app1s_notebook.CurrentPage = app1s_PAGE_MODES;
	}
}


