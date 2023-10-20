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
 * Copyright (C) 2022   Xavier de Blas <xaviblas@gmail.com>
 */

using System;
using System.IO;
using Gtk;
using System.Collections.Generic;
using System.Diagnostics;  //Stopwatch
using System.Threading;
using Mono.Unix;

//uses some code on gui/app1/session/backup.cs
public partial class ChronoJumpWindow
{
	private Thread app1s_threadExport;
	private static bool cancelExport;

	/*
	   on Windows when exporting and compressing, the chronojump.db is still used
	   so better export on app1s_fileCopyPre, then copy dir to app1s_fileCopy and then compress
	   this will be on a temp file because it also cannot be deleted on export end
	   */
	string app1s_fileCopyPre;

	private void on_button_session_export_pre_clicked (object o, EventArgs args)
	{
		if(! app1s_getDatabaseFile())
		{
			new DialogMessage(Constants.MessageTypes.WARNING, Catalog.GetString("Error. Cannot find database."));
			return;
		}

		if (operatingSystem == UtilAll.OperatingSystems.LINUX && ! ExecuteProcess.InstalledOnLinux ("7z"))
		{
			showLinux7zInstallMessage ();
			return;
		}

		app1s_label_export_destination.Text = "";
		app1s_label_export_progress.Text = "";
		app1s_exportText = "";
		app1s_button_export_select.Sensitive = true;
		app1s_button_export_start.Sensitive = false;
		app1s_button_export_cancel.Visible = false;
		app1s_button_export_close.Visible = true;
	
		app1s_notebook.CurrentPage = app1s_PAGE_EXPORT;
	}

	private void on_app1s_button_export_select_clicked (object o, EventArgs args)
	{
		if(currentSession == null || currentSession.UniqueID == -1) {
			new DialogMessage(Constants.MessageTypes.WARNING, "Cannot edit a missing session");
			return;
		}

		app1s_fc = new Gtk.FileChooserDialog(Catalog.GetString("Export session to:"),
				app1,
				FileChooserAction.SelectFolder,
				Catalog.GetString("Cancel"),ResponseType.Cancel,
				Catalog.GetString("Export"),ResponseType.Accept
				);

		if (app1s_fc.Run() == (int)ResponseType.Accept)
		{
			app1s_fileCopy = app1s_fc.Filename + Path.DirectorySeparatorChar + "chronojump_" + currentSession.Name + "_" + UtilDate.ToFile();
			app1s_fileCopyPre = Path.Combine (Path.GetTempPath (), "chronojumpCopyPre_" + currentSession.Name + "_" + UtilDate.ToFile()); //on temp because on Windows cannot be deleted

			app1s_label_export_destination.Text = app1s_fileCopy;
			if (exportImportCompressed)
				app1s_label_export_destination.Text += ".7z";

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
			//if exists app1s_fileCopyPre overwrite it without asking

			if(exists) {
				LogB.Information("Overwrite...");
				ConfirmWindow confirmWin = ConfirmWindow.Show(Catalog.GetString("Are you sure you want to overwrite: "),
						"", app1s_fileCopy);
				confirmWin.Button_accept.Clicked += new EventHandler(app1s_export_on_overwrite_file_accepted);
			} else {
				//note this is the same than: app1s_export_on_overwrite_file_accepted
				app1s_pulsebarExportActivity.Visible = true;
				app1s_uc = new UtilCopy (currentSession.UniqueID, false, false, false); //1 session, do not include logs, do not include config, no other DBs

				cancelExport = false;
				needToCloseSessionToCompress = false;
				app1s_threadExport = new Thread(new ThreadStart(app1s_export));
				GLib.Idle.Add (new GLib.IdleHandler (app1s_ExportPulseGTK));

				app1s_export_doing_sensitive_start_end(true);

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
		if (exportImportCompressed && needToCloseSessionToCompress)
		{
			LogB.Information("calling reloadSession on compress");
			reloadSession ();
			LogB.Information("called reloadSession on compress");

			needToCloseSessionToCompress = false;
		}

		if ( ! app1s_threadExport.IsAlive )
		{
			LogB.ThreadEnding();
			app1s_ExportPulseEnd();

			/* This is commented to avoid the unreachable code warning. If ever change the exportImportCompressed, uncomment
			if (! exportImportCompressed)
			{
				LogB.Information("calling reloadSession on thread end");
				reloadSession(); //to use default db again
				LogB.Information("called reloadSession on thread end");
			}
			*/

			//but unsensitivize left menu
			vbox_menu_tiny_menu.Sensitive = false;
			alignment_session_persons.Sensitive = false;

			LogB.ThreadEnded();
			return false;
		}

		app1s_pulsebarExportActivity.Pulse();
		app1s_label_export_progress.Text = app1s_exportText;

		Thread.Sleep (30);
		LogB.Debug(app1s_threadExport.ThreadState.ToString());
		return true;
	}

	private void app1s_ExportPulseEnd()
	{
		app1s_pulsebarExportActivity.Fraction = 1;

		if(cancelExport)
			app1s_label_export_progress.Text =
				Catalog.GetString("Cancelled.");
		else
			app1s_label_export_progress.Text =
				string.Format(Catalog.GetString("Exported in {0} s"),
						Math.Round(UtilAll.DivideSafe(app1s_exportElapsedMs, 1000.0), 1));

		app1s_export_doing_sensitive_start_end(false);
	}

	private void app1s_export_doing_sensitive_start_end(bool start)
	{
		if(start)
			app1s_label_export_progress.Text = "";

		app1s_pulsebarExportActivity.Visible = start;

		app1s_button_export_select.Sensitive = ! start;
		app1s_button_export_start.Sensitive = false;

		if(start) {
			app1s_button_export_cancel.Visible = true;
			app1s_button_export_close.Visible = false;
		} else {
			app1s_button_export_cancel.Visible = false;
			app1s_button_export_close.Visible = true;
		}
	}

	private void app1s_export_on_overwrite_file_accepted(object o, EventArgs args)
	{
		try {
			Directory.Delete(app1s_fileCopy, true);
			if(Directory.Exists(app1s_fileCopyPre))
				Directory.Delete(app1s_fileCopyPre, true);

			//note this is the same than: on_app1s_button_export_start_clicked
			app1s_pulsebarExportActivity.Visible = true;
			app1s_uc = new UtilCopy(currentSession.UniqueID, false, false, false);

			cancelExport = false;
			needToCloseSessionToCompress = false;
			app1s_threadExport = new Thread(new ThreadStart(app1s_export));
			GLib.Idle.Add (new GLib.IdleHandler (app1s_ExportPulseGTK));

			app1s_export_doing_sensitive_start_end(true);

			LogB.ThreadStart();
			app1s_threadExport.Start();
		} catch {
			new DialogMessage(Constants.MessageTypes.WARNING,
				string.Format(Catalog.GetString("Cannot copy to {0} "), app1s_fileCopy));
		}
	}

	private const bool exportImportCompressed = true;
	private static bool needToCloseSessionToCompress;
	static string app1s_exportText;
	static long app1s_exportElapsedMs;
	//No GTK here!
	private void app1s_export()
	{
		app1s_exportElapsedMs = 0;
		Stopwatch sw = new Stopwatch();
		sw.Start();

		app1s_exportText = Catalog.GetString("Copying files");

		string exportDir = app1s_fileCopy;
		if (exportImportCompressed)
			exportDir = app1s_fileCopyPre;

		//TODO: copy only needed multimedia (photos), videos are discarded by sessions
		app1s_uc.CopyFilesRecursively (new DirectoryInfo(Util.GetLocalDataDir (false)), new DirectoryInfo (exportDir), 0);

		//TODO: check that db exists and manage sessionSwitcher to go back
		string exportedDB = exportDir + System.IO.Path.DirectorySeparatorChar  +
			 "database" + System.IO.Path.DirectorySeparatorChar + "chronojump.db";
		LogB.Information("exporting to:" + exportedDB);
		if(! File.Exists(exportedDB))
		{
			//TODO: some error message
			return;
		}

		SqliteSessionSwitcher sessionSwitcher = new SqliteSessionSwitcher
			(SqliteSessionSwitcher.DatabaseType.EXPORT, exportedDB);

		List<SessionTestsCount> stc_l = sessionSwitcher.SelectAllSessionsTestsCount (""); //returns a string of values separated by ':'

		int count = 1;
		foreach (SessionTestsCount stc in stc_l)
		{
			//cancelExport breaks here instead doing it on pulse to avoid leaving some Sqlite DataReader opened
			if(cancelExport)
				break;

			app1s_exportText = string.Format("Adjusting new database {0}/{1}", count, stc_l.Count);
			if(stc.sessionParams.ID != currentSession.UniqueID)
			{
				LogB.Information(string.Format("session: {0}, will be deleted", stc.sessionParams.ID));
				sessionSwitcher.DeleteAllStuff(stc.sessionParams.ID.ToString ());
			}
			LogB.Information(string.Format("export session {0}/{1} done!", count, stc_l.Count ));
			count ++;
		}

		if (exportImportCompressed)
		{
			needToCloseSessionToCompress = true;
			while (needToCloseSessionToCompress)
				;
			//sessionSwitcher.Dispose ();
			sessionSwitcher = null;

			//need to copy the dir from app1s_fileCopyPre to app1s_fileCopy, because the exported chronojump.db is still used (not succeeded on closing it on Windows)
			Util.CopyFilesRecursively(app1s_fileCopyPre, app1s_fileCopy);

			//compressing with 7z
			app1s_exportText = string.Format("Compressing …");
			List<string> parameters = new List<string>();
			parameters.Add ("a");

			parameters.Add (app1s_fileCopy + ".7z");

			// option 1 add the folder with the files (better to have a dir that can be uncompressed in order to be opened from importer)
			parameters.Add (app1s_fileCopy);
			// option 2 without the parent folder (cleaner, but do not found how to import)
			//parameters.Add (app1s_fileCopy + Path.DirectorySeparatorChar + "*");

			string executable = ExecuteProcess.Get7zExecutable (operatingSystem);
			ExecuteProcess.Result execute_result = ExecuteProcess.run (executable, parameters, false, false);
			// delete exported folder
			if (execute_result.success)
			{
				Util.DirectoryDelete (app1s_fileCopy);
				Util.DirectoryDelete (app1s_fileCopyPre);
			}
		}

		//finishing
		sw.Stop();
		app1s_exportElapsedMs = sw.ElapsedMilliseconds;
		LogB.Information("ended app1s_export()");
	}

	private void on_app1s_button_export_cancel_clicked (object o, EventArgs args)
	{
		LogB.Information("cancelling export session");
		cancelExport = true;
	}

	private void on_app1s_button_export_close_clicked (object o, EventArgs args)
	{
		LogB.Information("closing export session");
		app1s_notebook.CurrentPage = app1s_PAGE_MODES;
	}
}
