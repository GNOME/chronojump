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
 * Copyright (C) 2020-2021   Xavier de Blas <xaviblas@gmail.com>
 */

using System;
using System.IO;
using Gtk;
using System.Diagnostics;  //Stopwatch
using System.Threading;
using Mono.Unix;

public partial class ChronoJumpWindow
{
	string app1s_fileDB;
	string app1s_fileCopy; //contains chronojump_datetime
	string app1s_parentCopy; //is the dir selected by app1s_fc, before adding chronojump_datetime,
				//nice to store in SQL and reuse in next backups
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

		notebook_session_backup.Page = Convert.ToInt32(notebook_session_backup_pages.BACKUP_DO);
		app1s_label_backup_destination.Text = "";
		app1s_label_backup_progress.Text = "";
		app1s_button_backup_select.Sensitive = true;
		app1s_button_backup_start.Sensitive = false;
		app1s_button_delete_old_incomplete.Visible = false;
		app1s_button_backup_cancel_close.Sensitive = true;
		app1s_label_backup_cancel_close.Text = Catalog.GetString("Cancel");

		app1s_notebook.CurrentPage = app1s_PAGE_BACKUP;

		int sizeInKB = Util.GetFullDataSize (false);
		app1s_label_backup_estimated_size.Text = string.Format(Catalog.GetString("Estimated size: {0} MB."),
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
		app1s_fileDB = "";

		long length1 = 0;
		if(file1.Exists)
			length1 = file1.Length;
		long length2 = 0;
		if(file2.Exists)
			length2 = file2.Length;

		if(length1 == 0 && length2 == 0)
			return false;
		else if(length1 > length2)
			app1s_fileDB = databaseURL;
		else
			app1s_fileDB = databaseTempURL;

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

			//if multimedia_and_encoder, then copy the folder. If not checked, then copy only the db file
			//if(check_backup_multimedia_and_encoder.Active)
				app1s_fileCopy = app1s_fc.Filename + Path.DirectorySeparatorChar + "chronojump_" + UtilDate.ToFile();
			//else
			//	app1s_fileCopy = app1s_fc.Filename + Path.DirectorySeparatorChar + "chronojump_copy.db";

			app1s_label_backup_destination.Text = app1s_fileCopy;

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
			if(Directory.Exists(app1s_fileCopy)) {
				LogB.Information(string.Format("Directory {0} exists, created at {1}",
							app1s_fileCopy, Directory.GetCreationTime(app1s_fileCopy)));
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
						"", app1s_fileCopy);
				confirmWin.Button_accept.Clicked += new EventHandler(app1s_backup_on_overwrite_file_accepted);
			} else {
				//if multimedia_and_encoder, then copy the folder. If not checked, then copy only the db file
				//if(check_backup_multimedia_and_encoder.Active) {
				app1s_uc = new UtilCopy(-1, true); //all sessions, backup
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
			string myString = string.Format(Catalog.GetString("Cannot copy to {0} "), app1s_fileCopy);
			new DialogMessage(Constants.MessageTypes.WARNING, myString);
		}
	}

	private bool app1s_BackupPulseGTK ()
	{
		if ( ! app1s_threadBackup.IsAlive ) {
			LogB.ThreadEnding();
			app1s_BackupPulseEnd();

			LogB.ThreadEnded();

			if (! app1s_copyRecursiveSuccess)
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

		if (app1s_copyRecursiveSuccess)
			app1s_label_backup_progress.Text =
				string.Format(Catalog.GetString("Copied in {0} ms."),
						app1s_copyRecursiveElapsedMs);
		else
			app1s_label_backup_progress.Text =
				Catalog.GetString("Failed, maybe the disk is full.");

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

		if(start) {
			app1s_button_backup_cancel_close.Sensitive = false; //or make cancel sensitive while process?
		} else {
			app1s_button_backup_cancel_close.Sensitive = true;
			app1s_label_backup_cancel_close.Text = Catalog.GetString("Close");
		}
	}

	private void app1s_backup_on_overwrite_file_accepted(object o, EventArgs args)
	{
		try {
			//if multimedia_and_encoder, then copy the folder. If not checked, then copy only the db file
			//if(check_backup_multimedia_and_encoder.Active) {
				Directory.Delete(app1s_fileCopy, true);
				app1s_uc = new UtilCopy(-1, true); //all sessions, backup
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
		app1s_label_backup_cancel_close.Text = Catalog.GetString("Cancel");

		notebook_session_backup.Page = Convert.ToInt32(notebook_session_backup_pages.DELETE_OLD);
	}
	private void on_app1s_button_old_backups_delete_do_clicked (object o, EventArgs args)
	{
		Directory.Delete(Util.GetBackupDirOld(), true);

		app1s_button_old_backups_delete_do.Sensitive = false;
		app1s_label_old_backups_delete_done.Visible = true;
		app1s_label_backup_cancel_close.Text = Catalog.GetString("Close");
	}

	/*
	 * deprecated since 1.6.0. Use backup method below
	*/
	static long app1s_copyRecursiveElapsedMs;
	static bool app1s_copyRecursiveSuccess;
	static int app1s_backupMainDirsDone;
	private void app1s_copyRecursive()
	{
		app1s_copyRecursiveElapsedMs = 0;
		Stopwatch sw = new Stopwatch();
		sw.Start();

		app1s_copyRecursiveSuccess = app1s_uc.CopyFilesRecursively(
				new DirectoryInfo(Util.GetParentDir(false)), new DirectoryInfo(app1s_fileCopy), 0);
		sw.Stop();

		app1s_copyRecursiveElapsedMs = sw.ElapsedMilliseconds;
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
						AddDirectoryFilesToTar (tarArchive, Util.GetParentDir(false), true);
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
		}
	}
}
