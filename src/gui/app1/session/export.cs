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

//uses some code on gui/app1/session/backup.cs
public partial class ChronoJumpWindow
{
	private Thread app1s_threadExport;

	private void on_button_session_export_pre_clicked (object o, EventArgs args)
	{
		if(! app1s_getDatabaseFile())
		{
			new DialogMessage(Constants.MessageTypes.WARNING, Catalog.GetString("Error. Cannot find database."));
			return;
		}

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

	private void on_app1s_button_export_start_clicked (object o, EventArgs args)
	{
		try {
			bool exists = false;
			if(Directory.Exists(app1s_fileCopy))
			{
				LogB.Information(string.Format("Directory {0} exists, created at {1}",
							app1s_fileCopy, Directory.GetCreationTime(app1s_fileCopy)));
				exists = true;
			}

			if(exists) {
				LogB.Information("Overwrite...");
				ConfirmWindow confirmWin = ConfirmWindow.Show(Catalog.GetString("Are you sure you want to overwrite: "), "", app1s_fileCopy);
				confirmWin.Button_accept.Clicked += new EventHandler(app1s_export_on_overwrite_file_accepted);
			} else {
				app1s_uc = new UtilCopy(currentSession.UniqueID);
				app1s_threadExport = new Thread(new ThreadStart(app1s_copyRecursive));
				GLib.Idle.Add (new GLib.IdleHandler (app1s_ExportPulseGTK));

//				app1s_backup_doing_sensitive_start_end(true);

				LogB.ThreadStart();
				app1s_threadExport.Start();
			}
		}
		catch {
			string myString = string.Format(Catalog.GetString("Cannot copy to {0} "), app1s_fileCopy);
			new DialogMessage(Constants.MessageTypes.WARNING, myString);
		}
	}

	private bool app1s_ExportPulseGTK ()
	{
		if ( ! app1s_threadExport.IsAlive ) {
		/*
			LogB.ThreadEnding();
			app1s_ExportPulseEnd();

			LogB.ThreadEnded();
			*/
			return false;
		}

		/*
		app1s_pulsebarExportActivity.Pulse();
		app1s_pulsebarExportDirs.Fraction = UtilAll.DivideSafeFraction(app1s_uc.ExportMainDirsCount, 6);
		//6 for: database, encoder, forceSensor, logs, multimedia, raceAnalyzer

		app1s_pulsebarExportDirs.Text = app1s_uc.LastMainDir;
		app1s_pulsebarExportSecondDirs.Fraction =
			UtilAll.DivideSafeFraction(app1s_uc.ExportSecondDirsCount, app1s_uc.ExportSecondDirsLength);
		app1s_pulsebarExportSecondDirs.Text = app1s_uc.LastSecondDir;

		Thread.Sleep (30);
		//LogB.Debug(app1s_threadExport.ThreadState.ToString());
		*/
		return true;
	}

	private void app1s_export_on_overwrite_file_accepted(object o, EventArgs args)
	{
		try {
			Directory.Delete(app1s_fileCopy, true);
			app1s_uc = new UtilCopy(currentSession.UniqueID);
			app1s_threadExport = new Thread(new ThreadStart(app1s_copyRecursive));
			GLib.Idle.Add (new GLib.IdleHandler (app1s_ExportPulseGTK));

//			app1s_backup_doing_sensitive_start_end(true);

			LogB.ThreadStart();
			app1s_threadExport.Start();
		} catch {
			new DialogMessage(Constants.MessageTypes.WARNING,
				string.Format(Catalog.GetString("Cannot copy to {0} "), app1s_fileCopy));
		}
	}

	static long app1s_exportElapsedMs;
	private void app1s_export()
	{
		app1s_exportElapsedMs = 0;
		Stopwatch sw = new Stopwatch();
		sw.Start();

		app1s_uc.CopyFilesRecursively(new DirectoryInfo(Util.GetParentDir(false)), new DirectoryInfo(app1s_fileCopy), 0);

		//TODO: now need to open database and delete unwanted sessions, and all stuff related to those sessions

		sw.Stop();

		app1s_exportElapsedMs = sw.ElapsedMilliseconds;
	}

	private void on_app1s_button_export_cancel_or_close_clicked (object o, EventArgs args)
	{
		app1s_notebook.CurrentPage = app1s_PAGE_MODES;
	}
}
