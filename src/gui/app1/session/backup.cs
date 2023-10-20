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
 * Copyright (C) 2020-2023   Xavier de Blas <xaviblas@gmail.com>
 */

using System;
using System.IO;
using Gtk;
using System.Collections.Generic;
using System.Diagnostics;  //Stopwatch
using System.Threading;
using Mono.Unix;

public partial class ChronoJumpWindow
{
	//string app1s_fileDB;
	string app1s_parentCopy; //is the dir selected by app1s_fc, before adding chronojump_datetime,
				//nice to store in SQL and reuse in next backups
	string app1s_fileCopy; //contains chronojump_datetime (just name of the file without the .7z)
	string app1s_tmpCopy; //contains chronojump_datetime (full path at tmp without the .7z). copy here (tmp) and then compress on person selected folder.
	string app1s_fullPathCopy; //contains chronojump_datetime (full path with the .7z)

	Gtk.FileChooserDialog app1s_fc;
	static UtilCopy app1s_uc;
	private Thread app1s_threadBackup;

	private enum notebook_session_backup_pages { BACKUP_DO, SCHEDULED_QUESTIONS, SCHEDULED_FEEDBACK, DELETE_OLD }

	//if entered from more, cancel_close should go to more again
	//but, if entered directly by scheduled, go to app1
	private bool backup_cancel_close_show_more_notebook;

	//user clicks on backup
	private void on_button_db_backup_pre_clicked (object o, EventArgs args)
	{
		backup_cancel_close_show_more_notebook = true; //coming from more, so should go to more
		label_backup_why.Visible = true; //user clicked on backup from more window. Show this label
		db_backup_pre_2 ();
	}

	private void db_backup_pre_2 ()
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

		notebook_session_backup.Page = Convert.ToInt32(notebook_session_backup_pages.BACKUP_DO);
		app1s_label_backup_destination.Text = "";
		app1s_label_backup_progress.Text = "";
		app1s_button_backup_select.Sensitive = true;
		app1s_button_backup_start.Sensitive = false;
		app1s_button_delete_old_incomplete.Visible = false;
		app1s_button_backup_cancel_close.Sensitive = true;
		image_app1s_button_backup_cancel_close.Pixbuf =
				new Gdk.Pixbuf (null, Util.GetImagePath(false) + "image_cancel.png");
		app1s_label_backup_cancel_close.Text = Catalog.GetString("Cancel");

		app1s_notebook.CurrentPage = app1s_PAGE_BACKUP;

		app1s_check_backup_include_config.Visible = File.Exists (Util.GetConfigFileName());

		showBackupEstimatedSize ();
	}

	private void on_app1s_check_backup_include_logs_clicked (object o, EventArgs args)
	{
		showBackupEstimatedSize ();
	}

	private void showBackupEstimatedSize ()
	{
		int sizeInKB = Util.GetFullDataSize (app1s_check_backup_include_logs.Active, false);
		app1s_label_backup_estimated_size.Text = string.Format(Catalog.GetString("Estimated size: {0} MB (uncompressed)."),
				UtilAll.DivideSafe(sizeInKB, 1000));
	}

	private bool shouldAskBackupScheduled ()
	{
		// 1) if next days is -1 (never ask again), do not show widget
		if(preferences.backupScheduledNextDays < 0)
			return false;

		// 2) if never scheduled, need to show widget
		if(preferences.backupScheduledCreatedDate == DateTime.MinValue)
			return true;

		// 3) if backup has to be done, show widget
		// <0 means "This instance is earlier than value.". So if created date + nextDays is < than current date, need to do backup
		if(preferences.backupScheduledCreatedDate.AddDays(preferences.backupScheduledNextDays).CompareTo(DateTime.Now) <= 0)
			return true;

		return false;
	}

	//scheduled backup on start
	private void backupScheduledAsk ()
	{
		menus_sensitive_import_not_danger(false);
		app1s_notebook_sup_entered_from = notebook_sup.CurrentPage;
                notebook_sup.CurrentPage = Convert.ToInt32(notebook_sup_pages.SESSION);

		notebook_session_backup.Page = Convert.ToInt32(notebook_session_backup_pages.SCHEDULED_QUESTIONS);

		app1s_notebook.CurrentPage = app1s_PAGE_BACKUP;

		app1s_button_backup_cancel_close.Visible = false;
		//TODO: remember to show again after pressing any of the buttons
	}

	private void on_app1s_button_backup_scheduled_now_clicked (object o, EventArgs args)
	{
		app1s_button_backup_cancel_close.Visible = true;
		label_backup_why.Visible = false; //do not show again label_backup_why

		db_backup_pre_2 ();
	}

	private void on_app1s_button_backup_scheduled_remind_clicked (object o, EventArgs args)
	{
		/*
		   There is a remind_next_time and not a remind_tomorrow, because tomorrow will be days = 1.
		   But, next day after the backup will continue being days = 1, bothering the user again (everyday).
		   Using next_time (meaning: next Chronojump start), nothing is changed on sqlite,
		   message will appear next time, days will be always 30 by default or 60 or 90 if last time selected one of those.
		 */

		int days = 0;
		if (o == (object) app1s_button_backup_scheduled_remind_next_time)
			days = 0;
		else if (o == (object) app1s_button_backup_scheduled_remind_30d)
			days = 30;
		else if (o == (object) app1s_button_backup_scheduled_remind_60d)
			days = 60;
		else if (o == (object) app1s_button_backup_scheduled_remind_90d)
			days = 90;

		string message = Catalog.GetString("You will be prompted for a backup the next time you start Chronojump.");
		if(days > 0)
			message = string.Format(Catalog.GetString("You will be prompted for a backup in {0} days."), days);

		app1_backup_remind_or_never_do (days, message);
	}

	private void on_app1s_button_backup_scheduled_never_clicked (object o, EventArgs args)
	{
		app1_backup_remind_or_never_do (-1, Catalog.GetString("You will no longer be bothered with scheduled backups, but you can continue backing up by clicking on") +
				" " + Catalog.GetString("Session") + " / " + Catalog.GetString("More") + ".");
	}

	private void app1_backup_remind_or_never_do (int days, string message)
	{
		if(days != 0) //aplly changes on never (-1) and on 30/60/90
		{
			// 1) Sqlite changes
			Sqlite.Open(); // ---->

			SqlitePreferences.Update(SqlitePreferences.BackupScheduledCreatedDateStr, UtilDate.ToSql(DateTime.Now), true);
			SqlitePreferences.Update(SqlitePreferences.BackupScheduledNextDaysStr, days.ToString(), true);

			Sqlite.Close(); // <----
		}

		// 2) gui changes
		label_backup_why.Visible = false;
		notebook_session_backup.Page = Convert.ToInt32(notebook_session_backup_pages.SCHEDULED_FEEDBACK);
		app1s_label_remind_feedback.Text = message;

		backup_cancel_close_show_more_notebook = false; //clicking on close should not show session more
		image_app1s_button_backup_cancel_close.Pixbuf =
				new Gdk.Pixbuf (null, Util.GetImagePath(false) + "image_close.png");
		app1s_label_backup_cancel_close.Text = Catalog.GetString("Close");
		app1s_button_backup_cancel_close.Visible = true;
	}

	private bool app1s_getDatabaseFile ()
	{
		string databaseURL = Util.GetDatabaseDir() +
			System.IO.Path.DirectorySeparatorChar  + "chronojump.db";
		string databaseTempURL = Util.GetDatabaseTempDir() +
			System.IO.Path.DirectorySeparatorChar  + "chronojump.db";

		System.IO.FileInfo file1 = new System.IO.FileInfo(databaseURL);
		System.IO.FileInfo file2 = new System.IO.FileInfo(databaseTempURL);
		//app1s_fileDB = "";

		long length1 = 0;
		if(file1.Exists)
			length1 = file1.Length;
		long length2 = 0;
		if(file2.Exists)
			length2 = file2.Length;

		if(length1 == 0 && length2 == 0)
			return false;
		/*
		else if(length1 > length2)
			app1s_fileDB = databaseURL;
		else
			app1s_fileDB = databaseTempURL;
			*/

		return true;
	}

	private void on_app1s_button_backup_select_clicked (object o, EventArgs args)
	{
		app1s_fc = new Gtk.FileChooserDialog(Catalog.GetString("Copy database to:"),
				app1,
				FileChooserAction.SelectFolder,
				Catalog.GetString("Cancel"),ResponseType.Cancel,
				Catalog.GetString("Select"),ResponseType.Accept
				);

		app1s_fc.SetCurrentFolder(preferences.lastBackupDir);

		if (app1s_fc.Run() == (int)ResponseType.Accept)
		{
			app1s_parentCopy = app1s_fc.Filename;
			app1s_fileCopy = "chronojump_" + UtilDate.ToFile();
			app1s_tmpCopy = Path.Combine (Path.GetTempPath (), app1s_fileCopy);
			app1s_fullPathCopy = Path.Combine (app1s_fc.Filename, app1s_fileCopy + ".7z");

			app1s_label_backup_destination.Text = app1s_fullPathCopy;

			app1s_button_backup_start.Sensitive = true;
		}

		app1s_fc.Hide ();

		//Don't forget to call Destroy() or the FileChooserDialog window won't get closed.
		app1s_fc.Destroy();
	}

	private void on_app1s_button_backup_start_clicked (object o, EventArgs args)
	{
		try {
			bool exists = false;
			//if(check_backup_multimedia_and_encoder.Active) {
			if( File.Exists (app1s_fullPathCopy)) {
				LogB.Information (string.Format ("File {0} exists, created at {1}",
							app1s_fullPathCopy, File.GetCreationTime (app1s_fullPathCopy)));
				exists = true;
			}
			/*} else {
			  if (File.Exists(app1s_fileCopy)) {
			  LogB.Information(string.Format("File {0} exists with attributes {1}, created at {2}",
			  app1s_fileCopy, File.GetAttributes(app1s_fileCopy), File.GetCreationTime(app1s_fileCopy)));
			  exists = true;
			  }
			  }
			  */

			if(exists) {
				LogB.Information("Overwrite...");
				ConfirmWindow confirmWin = ConfirmWindow.Show(Catalog.GetString("Are you sure you want to overwrite: "),
						"", app1s_fullPathCopy);
				confirmWin.Button_accept.Clicked += new EventHandler(app1s_backup_on_overwrite_file_accepted);
			} else {
				//if multimedia_and_encoder, then copy the folder. If not checked, then copy only the db file
				//if(check_backup_multimedia_and_encoder.Active) {
				app1s_uc = new UtilCopy (-1,   //allSessions
						app1s_check_backup_include_logs.Active,
						app1s_check_backup_include_config.Active,
						true);

				app1s_threadBackup = new Thread(new ThreadStart(app1s_copyRecursive));
				GLib.Idle.Add (new GLib.IdleHandler (app1s_BackupPulseGTK));

				app1s_backup_doing_sensitive_start_end(true);

				LogB.ThreadStart();
				app1s_threadBackup.Start();
				/*} else {
				  File.Copy(app1s_fileDB, app1s_fileCopy);

				  label_backup.Text = string.Format(Catalog.GetString("Copied to {0}"), app1s_fileCopy);
				  }
				  */
			}
		}
		catch {
			string myString = string.Format(Catalog.GetString("Cannot copy to {0} "), app1s_fullPathCopy);
			new DialogMessage(Constants.MessageTypes.WARNING, myString);
		}
	}

	private bool app1s_BackupPulseGTK ()
	{
		if ( ! app1s_threadBackup.IsAlive ) {
			LogB.ThreadEnding();
			app1s_BackupPulseEnd();

			LogB.ThreadEnded();

			if (! app1s_copyRecursiveSuccess || ! app1s_copyCompressSuccess)
				return false;

			//update Sqlite backup vars (preferences not neede because will not be used until next boot)
			Sqlite.Open(); // ---->

			DateTime nowDT = DateTime.Now;
			SqlitePreferences.Update(SqlitePreferences.LastBackupDirStr, app1s_parentCopy, true);
			SqlitePreferences.Update(SqlitePreferences.LastBackupDatetimeStr, UtilDate.ToSql(nowDT), true);
			SqlitePreferences.Update(SqlitePreferences.BackupScheduledCreatedDateStr, UtilDate.ToSql(nowDT), true);

			Sqlite.Close(); // <----

			return false;
		}

		if (app1s_copyRecursiveSuccess)
		{
			int minutesNeeded = Convert.ToInt32 (Math.Ceiling (
						UtilAll.DivideSafe (
							12 * UtilAll.DivideSafe (app1s_copyRecursiveElapsedMs, 1000),
							60)
						));

			app1s_label_backup_progress.Text = Catalog.GetString("Compressing …") + "\n" +
				string.Format (Catalog.GetPluralString (
							"Please, wait approximately 1 minute.",
							"Please, wait approximately {0} minutes.",
							minutesNeeded), minutesNeeded);

			app1s_hbox_backup_doing.Visible = false;
		}

		app1s_pulsebarBackupActivity.Pulse();
		app1s_pulsebarBackupDirs.Fraction = UtilAll.DivideSafeFraction(app1s_uc.BackupMainDirsCount, 6);
		//6 for: database, encoder, forceSensor, logs, multimedia, raceAnalyzer

		app1s_pulsebarBackupDirs.Text = app1s_uc.LastMainDir;
		app1s_pulsebarBackupSecondDirs.Fraction =
			UtilAll.DivideSafeFraction(app1s_uc.BackupSecondDirsCount, app1s_uc.BackupSecondDirsLength);
		app1s_pulsebarBackupSecondDirs.Text = app1s_uc.LastSecondDir;

		Thread.Sleep (30);
		//LogB.Debug(app1s_threadBackup.ThreadState.ToString());
		return true;
	}

	private void app1s_BackupPulseEnd()
	{
		app1s_pulsebarBackupActivity.Fraction = 1;
		app1s_pulsebarBackupDirs.Fraction = 1;
		app1s_pulsebarBackupSecondDirs.Fraction = 1;
		app1s_backup_doing_sensitive_start_end(false);
		//app1s_fc.Hide ();

		string str = "";
		if (app1s_copyRecursiveSuccess)
		{
			str = string.Format(Catalog.GetString("Copied in {0} ms."),
					Math.Round (UtilAll.DivideSafe (app1s_copyRecursiveElapsedMs, 1000), 1));
			if (app1s_copyCompressSuccess)
				str += " " + string.Format(Catalog.GetString("Compressed in {0} s."),
						Math.Round (UtilAll.DivideSafe (app1s_copyCompressElapsedMs, 1000), 1));
			else
				str += " " + Catalog.GetString("Failed at compress.");
		} else
			str = Catalog.GetString("Copy failed, maybe the disk is full.");

		app1s_label_backup_progress.Text = str;

		// show button to delete old backups (if exists)
		if(Directory.Exists (Util.GetBackupDirOld()))
		{
			int filesBackup, sizeInKBBackup;
			Util.GetBackupsSize (out filesBackup, out sizeInKBBackup);
			if(filesBackup > 0)
				app1s_button_delete_old_incomplete.Visible = true;
		}
	}

	private void app1s_backup_doing_sensitive_start_end(bool start)
	{
		if(start)
			app1s_label_backup_progress.Text = Catalog.GetString("Please, wait.");

		app1s_pulsebarBackupActivity.Visible = start;
		app1s_hbox_backup_doing.Visible = start;

		app1s_button_backup_select.Sensitive = ! start;
		app1s_button_backup_start.Sensitive = false;

		app1s_check_backup_include_logs.Sensitive = ! start;
		app1s_check_backup_include_config.Sensitive = ! start;

		if(start) {
			app1s_button_backup_cancel_close.Sensitive = false; //or make cancel sensitive while process?
		} else {
			app1s_button_backup_cancel_close.Sensitive = true;
			image_app1s_button_backup_cancel_close.Pixbuf =
				new Gdk.Pixbuf (null, Util.GetImagePath(false) + "image_close.png");
			app1s_label_backup_cancel_close.Text = Catalog.GetString("Close");
		}
	}

	private void app1s_backup_on_overwrite_file_accepted(object o, EventArgs args)
	{
		try {
			//if multimedia_and_encoder, then copy the folder. If not checked, then copy only the db file
			//if(check_backup_multimedia_and_encoder.Active) {
				Directory.Delete(app1s_fileCopy, true);
				app1s_uc = new UtilCopy (-1,   //allSessions
						app1s_check_backup_include_logs.Active,
						app1s_check_backup_include_config.Active,
						true);

				app1s_threadBackup = new Thread(new ThreadStart(app1s_copyRecursive));
				GLib.Idle.Add (new GLib.IdleHandler (app1s_BackupPulseGTK));

				app1s_backup_doing_sensitive_start_end(true);

				LogB.ThreadStart();
				app1s_threadBackup.Start();
			/* } else {
				File.Delete(app1s_fileCopy);
				File.Copy(app1s_fileDB, app1s_fileCopy);

				app1s_fc.Hide ();
				app1s_label_backup_progress.Text =
					string.Format(Catalog.GetString("Copied to {0}"), app1s_fileCopy);
			} */
		} catch {
			new DialogMessage(Constants.MessageTypes.WARNING,
				string.Format(Catalog.GetString("Cannot copy to {0} "), app1s_fileCopy));
		}
	}

	private void on_app1s_button_delete_old_incomplete_clicked (object o, EventArgs args)
	{
		label_backup_why.Visible = false; //do not show again label_backup_why
		image_app1s_button_backup_cancel_close.Pixbuf =
				new Gdk.Pixbuf (null, Util.GetImagePath(false) + "image_cancel.png");
		app1s_label_backup_cancel_close.Text = Catalog.GetString("Cancel");

		notebook_session_backup.Page = Convert.ToInt32(notebook_session_backup_pages.DELETE_OLD);
	}
	private void on_app1s_button_old_backups_delete_do_clicked (object o, EventArgs args)
	{
		Directory.Delete(Util.GetBackupDirOld(), true);

		app1s_button_old_backups_delete_do.Sensitive = false;
		app1s_label_old_backups_delete_done.Visible = true;
		image_app1s_button_backup_cancel_close.Pixbuf =
				new Gdk.Pixbuf (null, Util.GetImagePath(false) + "image_close.png");
		app1s_label_backup_cancel_close.Text = Catalog.GetString("Close");
	}

	static long app1s_copyRecursiveElapsedMs;
	static long app1s_copyCompressElapsedMs;

	static bool app1s_copyRecursiveSuccess;
	static bool app1s_copyCompressSuccess;
	//static int app1s_backupMainDirsDone;
	private void app1s_copyRecursive()
	{
		app1s_copyRecursiveCopy (Util.GetLocalDataDir(false), app1s_tmpCopy);

		System.Threading.Thread.Sleep (250);
		if (! app1s_copyRecursiveSuccess)
			return;

		app1s_copyRecursiveCompress ();
		//do not need to delete tmp folder
	}

	private void app1s_copyRecursiveCopy (string origin, string destination)
	{
		app1s_copyRecursiveElapsedMs = 0;
		Stopwatch sw = new Stopwatch();
		sw.Start();

		LogB.Information (string.Format ("Going to copy from {0} to {1}", origin, destination));
		app1s_copyRecursiveSuccess = app1s_uc.CopyFilesRecursively(
				new DirectoryInfo (origin),
				new DirectoryInfo (destination), 0);

		sw.Stop();
		app1s_copyRecursiveElapsedMs = sw.ElapsedMilliseconds;
	}

	private void app1s_copyRecursiveCompress ()
	{
		Stopwatch sw = new Stopwatch();
		sw.Start();

		List<string> parameters = new List<string>();
		parameters.Add ("a");
		parameters.Add (app1s_fullPathCopy);

		// option 1 add the folder with the files (better to have a dir that can be uncompressed in order to be opened from importer)
		parameters.Add (app1s_tmpCopy);
		// option 2 without the parent folder (cleaner, but do not found how to import)
		//parameters.Add (app1s_tmpCopy + Path.DirectorySeparatorChar + "*");

		string executable = ExecuteProcess.Get7zExecutable (operatingSystem);

		//not at background
		ExecuteProcess.Result execute_result = ExecuteProcess.run (executable, parameters, false, false);
		app1s_copyCompressSuccess = execute_result.success;


		/*
		 * unsuccessful tests on get progress of 7zip
		 *
		//at background
		Process process = new Process ();
		app1s_copyCompressSuccess = ExecuteProcess.RunAtBackground (
				ref process, executable, parameters, false, false, false, true, false);

		if (! app1s_copyCompressSuccess)
		{
			sw.Stop();
			process = null;
			return;
		} else {
			//process.WaitForExit ();
			using (var sr = new StreamReader (process.StandardOutput))
			{
				while (sr.Peek() >= 0)
				{
					LogB.Information (sr.ReadLine());
				}
			}
			process.BeginOutputReadLine();
			while (! process.StandardOutput.EndOfStream)
			{
				var line = process.StandardOutput.ReadLine();
				LogB.Information (line);
				LogB.Information (process.StandardOutput.ReadToEnd().TrimEnd ('\n'));
			}
			//StreamReader sr = process.StandardOutput;

			process.WaitForExit();
		}


		//using (Process process = new Process())
		Process process = new Process();
		{
			app1s_copyCompressSuccess = ExecuteProcess.RunAtBackground (
					ref process, executable, parameters, false, false, false, true, false);

			// Synchronously read the standard output of the spawned process.
			StreamReader reader = process.StandardOutput;
			string output = reader.ReadToEnd();

			// Write the redirected output to this application's window.
			LogB.Information(output);

			process.WaitForExit();
		}
		*/

		sw.Stop();
		app1s_copyCompressElapsedMs = sw.ElapsedMilliseconds;
	}

	/*
	 * Temporarily disabled
	 *
	//from Longomatch
	//https://raw.githubusercontent.com/ylatuya/longomatch/master/LongoMatch.DB/CouchbaseStorage.cs
	private bool backup(string path)
	{
		try {
			string storageName = path + Path.DirectorySeparatorChar + "chronojump_backup-" + DateTime.UtcNow.ToString() + ".tar.gz";
			using (FileStream fs = new FileStream (outputFilename, FileMode.Create, FileAccess.Write, FileShare.None)) {
				using (Stream gzipStream = new GZipOutputStream (fs)) {
					using (TarArchive tarArchive = TarArchive.CreateOutputTarArchive (gzipStream)) {
						//foreach (string n in new string[] {"", "-wal", "-shm"}) {
						//	TarEntry tarEntry = TarEntry.CreateEntryFromFile (
						//			Path.Combine (Config.DBDir, storageName + ".cblite" + n));
						//	tarArchive.WriteEntry (tarEntry, true);
						//}
						//AddDirectoryFilesToTar (tarArchive, Path.Combine (Config.DBDir, storageName + " attachments"), true);
						AddDirectoryFilesToTar (tarArchive, Util.GetLocalDataDir(false), true);
					}
				}
			}
			//LastBackup = DateTime.UtcNow;
		} catch (Exception ex) {
			LogB.Error (ex);
			return false;
		}
		return true;
	}


	//from Longomatch
	//https://raw.githubusercontent.com/ylatuya/longomatch/master/LongoMatch.DB/CouchbaseStorage.cs
	void AddDirectoryFilesToTar (TarArchive tarArchive, string sourceDirectory, bool recurse)
	{
		// Recursively add sub-folders
		if (recurse) {
			string[] directories = Directory.GetDirectories (sourceDirectory);
			foreach (string directory in directories)
				AddDirectoryFilesToTar (tarArchive, directory, recurse);
		}

		// Add files
		string[] filenames = Directory.GetFiles (sourceDirectory);
		foreach (string filename in filenames) {
			TarEntry tarEntry = TarEntry.CreateEntryFromFile (filename);
			tarArchive.WriteEntry (tarEntry, true);
		}
	}
	*/

	private void on_app1s_button_backup_cancel_or_close_clicked (object o, EventArgs args)
	{
		if(backup_cancel_close_show_more_notebook)
			app1s_notebook.CurrentPage = app1s_PAGE_MODES;
		else {
			menus_sensitive_import_not_danger(true);
			notebook_supSetOldPage();
			if (notebook_sup.CurrentPage == Convert.ToInt32(notebook_sup_pages.START))
				new ChronojumpLogo (notebook_chronojump_logo, drawingarea_chronojump_logo, false);//preferences.logoAnimatedShow);
		}
	}

	/*
	 * -----------------------------------------------------------------------------
	 * copy to cloud, copies to a dir that will be synced on cloud
	 * and readed on another computer (just to view, analysis with openExternalDB.
	 * Works like backup but overwriting and not compressing (and with fewer widgets)
	 * -----------------------------------------------------------------------------
	 */
	private string copyToCloudButtonLabel = "";
	private bool exitChronojumpAfterCopyToCloud;
	private bool exitChronojumpAfterCopyToCloudStarted; //to avoid doing it again if person double click on delete event

	private void on_app1s_button_copyToCloud_clicked (object o, EventArgs args)
	{
		exitChronojumpAfterCopyToCloud = false;

		copyToCloud_start ();
	}
	private void on_copyToCloud_when_exit ()
	{
		LogB.Information ("Copying to cloud before exiting");
		exitChronojumpAfterCopyToCloud = true;
		exitChronojumpAfterCopyToCloudStarted = true;

		copyToCloud_start ();
	}

	private void copyToCloud_start ()
	{
		LogB.Information ("Copy to Cloud, Going to copy to: " + configChronojump.CopyToCloudFullPath);
		copyToCloudButtonLabel = app1s_button_copyToCloud.Label;

		try {
			app1s_uc = new UtilCopy (-1, false, false, false); //all sessions, no logs, no config, no other DBs

			app1s_button_copyToCloud.Label = "Copying …";
			app1s_button_copyToCloud.Sensitive = false;
			app1s_progressbar_copyToCloud_dirs.Fraction = 0;
			app1s_progressbar_copyToCloud_subDirs.Fraction = 0;

			app1s_threadBackup = new Thread (new ThreadStart (app1s_copyToCloudDo));
			GLib.Idle.Add (new GLib.IdleHandler (app1s_CopyToCloudPulseGTK));

			//app1s_backup_doing_sensitive_start_end(true);

			LogB.ThreadStart();
			app1s_threadBackup.Start();
		}
		catch {
			string myString = string.Format (Catalog.GetString("Cannot copy to {0} "),
					configChronojump.CopyToCloudFullPath);
			new DialogMessage(Constants.MessageTypes.WARNING, myString);
		}
	}

	private void app1s_copyToCloudDo ()
	{
		// 1 delete and create dir
		if (Directory.Exists (configChronojump.CopyToCloudFullPath))
			Directory.Delete (configChronojump.CopyToCloudFullPath, true);

		System.Threading.Thread.Sleep (100); //to ensure dir is deleted

		Directory.CreateDirectory (configChronojump.CopyToCloudFullPath);
		System.Threading.Thread.Sleep (1000); //to ensure dir is created

		// 2 do the copy
		app1s_copyRecursiveCopy (Util.GetLocalDataDir(false), configChronojump.CopyToCloudFullPath);
	}

	private bool app1s_CopyToCloudPulseGTK ()
	{
		if ( ! app1s_threadBackup.IsAlive )
		{
			LogB.ThreadEnding();
			app1s_CopyToCloudPulseEnd();

			LogB.ThreadEnded();

			if (! app1s_copyRecursiveSuccess)
				return false;

			if (exitChronojumpAfterCopyToCloud)
				on_quit2_activate ();

			return false;
		}

		if (exitChronojumpAfterCopyToCloud)
			app1s_button_copyToCloud.Label = "Copying & exit …";
		else
			app1s_button_copyToCloud.Label = "Copying …";

		app1s_progressbar_copyToCloud_dirs.Fraction = UtilAll.DivideSafeFraction (app1s_uc.BackupMainDirsCount, 6);
		app1s_progressbar_copyToCloud_subDirs.Fraction =
			UtilAll.DivideSafeFraction(app1s_uc.BackupSecondDirsCount, app1s_uc.BackupSecondDirsLength);
		//6 for: database, encoder, forceSensor, logs, multimedia, raceAnalyzer

		Thread.Sleep (30);
		//LogB.Debug(app1s_threadBackup.ThreadState.ToString());
		return true;
	}
	private void app1s_CopyToCloudPulseEnd ()
	{
		app1s_button_copyToCloud.Label = "Done!";
		app1s_progressbar_copyToCloud_dirs.Fraction = 1;
		app1s_progressbar_copyToCloud_subDirs.Fraction = 1;

		GLib.Timeout.Add (2000, new GLib.TimeoutHandler (app1s_CopyToCloudPulseEnd2));
	}
	private bool app1s_CopyToCloudPulseEnd2 ()
	{
		//restore the "Copy to cloud", and make button sensitive
		app1s_button_copyToCloud.Label = copyToCloudButtonLabel;
		app1s_button_copyToCloud.Sensitive = true;

		app1s_progressbar_copyToCloud_dirs.Fraction = 0;
		app1s_progressbar_copyToCloud_subDirs.Fraction = 0;

		return false;
	}

	/*
	 * -----------------------------------------------------------------------------
	 * copy from cloud, copies to tmp before opening to not have cloud sync problems with the DB that is being updated on other machine
	 * Works like backup but overwriting and not compressing (and with fewer widgets)
	 * -----------------------------------------------------------------------------
	 */
	private void copyFromCloudToTemp_pre ()
	{
		LogB.Information ("Copy from Cloud, Going to copy to: " + Util.GetCloudReadTempDir ());

		try {
			app1s_uc = new UtilCopy (-1, false, false, false); //all sessions, no logs, no config, no other DBs

			app1s_progressbar_copyFromCloud_dirs.Fraction = 0;
			app1s_progressbar_copyFromCloud_subDirs.Fraction = 0;

			app1s_threadBackup = new Thread (new ThreadStart (app1s_copyFromCloudDo));
			GLib.Idle.Add (new GLib.IdleHandler (app1s_CopyFromCloudPulseGTK));

			//app1s_backup_doing_sensitive_start_end(true);

			LogB.ThreadStart();
			app1s_threadBackup.Start();
		}
		catch {
			string myString = string.Format (Catalog.GetString("Cannot copy to {0} "),
					Util.GetCloudReadTempDir ());
			new DialogMessage(Constants.MessageTypes.WARNING, myString);
		}
	}

	private void app1s_copyFromCloudDo ()
	{
		LogB.Information ("app1s_copyFromCloudDo 0");
		// 1 delete and create dir
		if (Directory.Exists (Util.GetCloudReadTempDir ()))
		{
			LogB.Information ("app1s_copyFromCloudDo 1");
			Directory.Delete (Util.GetCloudReadTempDir (), true);
		}

		System.Threading.Thread.Sleep (100); //to ensure dir is deleted

			LogB.Information ("app1s_copyFromCloudDo 2");
		Directory.CreateDirectory (Util.GetCloudReadTempDir ());
		System.Threading.Thread.Sleep (1000); //to ensure dir is created

			LogB.Information ("app1s_copyFromCloudDo 3");
		// 2 do the copy
		app1s_copyRecursiveCopy (configChronojump.LastDBFullPath, Util.GetCloudReadTempDir ());
			LogB.Information ("app1s_copyFromCloudDo 4");
	}

	private bool app1s_CopyFromCloudPulseGTK ()
	{
		if ( ! app1s_threadBackup.IsAlive )
		{
			LogB.ThreadEnding();
			app1s_CopyFromCloudPulseEnd();

			LogB.ThreadEnded();

			if (! app1s_copyRecursiveSuccess)
				return false;

			return false;
		}

		app1s_progressbar_copyFromCloud_dirs.Fraction = UtilAll.DivideSafeFraction (app1s_uc.BackupMainDirsCount, 6);
		app1s_progressbar_copyFromCloud_subDirs.Fraction =
			UtilAll.DivideSafeFraction(app1s_uc.BackupSecondDirsCount, app1s_uc.BackupSecondDirsLength);
		//6 for: database, encoder, forceSensor, logs, multimedia, raceAnalyzer

		Thread.Sleep (30);
		//LogB.Debug(app1s_threadBackup.ThreadState.ToString());
		return true;
	}
	private void app1s_CopyFromCloudPulseEnd ()
	{
		app1s_progressbar_copyFromCloud_dirs.Fraction = 1;
		app1s_progressbar_copyFromCloud_subDirs.Fraction = 1;

		GLib.Timeout.Add (2000, new GLib.TimeoutHandler (app1s_CopyFromCloudPulseEnd2));

		// copy to cloud called at start Chronojump ...
		if (databaseCloudCopyToTempModeAtBoot)
		{
			if (configChronojump.ReadFromCloudMainPath != "" || configChronojump.CanOpenExternalDB)
				databaseChange ();

			configDo ();
			ChronojumpWindowCont ();
		}
		else //... or at click on change database
		{
			configChronojump.LastDBFullPath = Util.GetCloudReadTempDir ();
			databaseChange ();
		}
	}
	private bool app1s_CopyFromCloudPulseEnd2 ()
	{
		app1s_progressbar_copyFromCloud_dirs.Fraction = 0;
		app1s_progressbar_copyFromCloud_subDirs.Fraction = 0;

		return false;
	}
}
