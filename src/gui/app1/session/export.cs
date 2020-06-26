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
 * Copyright (C) 2020   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
using System.IO;
using Gtk;
using System.Diagnostics;  //Stopwatch
using System.Threading;
using Mono.Unix;

public partial class ChronoJumpWindow
{
	private void on_button_session_export_pre_clicked (object o, EventArgs args)
	{
		/*
		if(! app1s_getDatabaseFile())
		{
			new DialogMessage(Constants.MessageTypes.WARNING, Catalog.GetString("Error. Cannot find database."));
			return;
		}
		*/

		app1s_label_export_destination.Text = "";
		app1s_label_export_progress.Text = "";
		app1s_button_export_select.Sensitive = true;
		app1s_button_export_start.Sensitive = false;
		app1s_button_export_cancel_close.Sensitive = true;
		app1s_label_export_cancel_close.Text = Catalog.GetString("Cancel");
	
		app1s_notebook.CurrentPage = app1s_PAGE_EXPORT;
	}

	private void on_app1s_button_export_select_clicked (object o, EventArgs args)
	{
		app1s_fc = new Gtk.FileChooserDialog(Catalog.GetString("Export session to:"),
				app1,
				FileChooserAction.SelectFolder,
				Catalog.GetString("Cancel"),ResponseType.Cancel,
				Catalog.GetString("Export"),ResponseType.Accept
				);

		if (app1s_fc.Run() == (int)ResponseType.Accept)
		{
			app1s_fileCopy = app1s_fc.Filename + Path.DirectorySeparatorChar + "chronojump_" + currentSession.Name + "_" + UtilDate.ToFile();

			app1s_label_export_destination.Text = app1s_fileCopy;

			app1s_button_export_start.Sensitive = true;
		}

		app1s_fc.Hide ();

		//Don't forget to call Destroy() or the FileChooserDialog window won't get closed.
		app1s_fc.Destroy();
	}


	private void on_app1s_button_export_cancel_or_close_clicked (object o, EventArgs args)
	{
		app1s_notebook.CurrentPage = app1s_PAGE_MODES;
	}
}
