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
	string app1s_fileDB;
	string app1s_fileCopy;
	Gtk.FileChooserDialog app1s_fc;
	static UtilCopy app1s_uc;
	private Thread app1s_threadBackup;

	private void on_button_db_backup_pre_clicked (object o, EventArgs args)
	{
		if(! app1s_getDatabaseFile())
		{
			new DialogMessage(Constants.MessageTypes.WARNING, Catalog.GetString("Error. Cannot find database."));
			return;
		}

		app1s_label_backup_destination.Text = "";
		app1s_label_backup_progress.Text = "";
		app1s_button_backup_select.Sensitive = true;
		app1s_button_backup_start.Sensitive = false;
		app1s_button_backup_cancel_close.Sensitive = true;
		app1s_label_backup_cancel_close.Text = Catalog.GetString("Cancel");
	
		app1s_notebook.CurrentPage = app1s_PAGE_BACKUP;
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
				Catalog.GetString("Copy"),ResponseType.Accept
				);

		if (app1s_fc.Run() == (int)ResponseType.Accept)
		{
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
				ConfirmWindow confirmWin = ConfirmWindow.Show(Catalog.GetString("Are you sure you want to overwrite: "), "", app1s_fileCopy);
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
		app1s_label_backup_progress.Text =
			string.Format(Catalog.GetString("Copied in {0} ms"),
					app1s_copyRecursiveElapsedMs);
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

	/*
	 * deprecated since 1.6.0. Use backup method below
	*/
	static long app1s_copyRecursiveElapsedMs;
	static int app1s_backupMainDirsDone;
	private void app1s_copyRecursive()
	{
		app1s_copyRecursiveElapsedMs = 0;
		Stopwatch sw = new Stopwatch();
		sw.Start();

		//Util.CopyFilesRecursively(new DirectoryInfo(Util.GetParentDir(false)), new DirectoryInfo(app1s_fileCopy), out app1s_backupMainDirsDone);
		app1s_uc.CopyFilesRecursively(new DirectoryInfo(Util.GetParentDir(false)), new DirectoryInfo(app1s_fileCopy), 0);
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
		app1s_notebook.CurrentPage = app1s_PAGE_MODES;
	}
}
