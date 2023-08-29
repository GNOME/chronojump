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
 * Copyright (C) 2021   Xavier de Blas <xaviblas@gmail.com>
 */

using System.IO; 		//for detect OS //TextWriter
using System.Collections; 		//ArrayList
using System.Collections.Generic; //List<T>
using System.Threading;
using Mono.Unix;

public abstract class ExportFiles
{
	public Gtk.Button Button_done;

	//passed variables
	protected Gtk.Notebook notebook;
	protected Gtk.Label labelProgress;
	protected Gtk.ProgressBar progressbar;
	protected Gtk.Label labelDiscarded;
	protected Gtk.Label labelResult;
	protected bool includeImages;
	protected int imageWidth;
	protected int imageHeight;
	protected string exportURL; //folder or .csv depending on includeImages
	protected bool isWindows;
	protected int personID; // -1: all
	protected int sessionID;
	protected char exportDecimalSeparator;

	protected ArrayList personSession_l;

	protected static Thread thread;
	protected static bool cancel;
	protected static bool failedRprocess; //error on .R process
	protected static bool noData;
	protected static bool cannotCopy;
	protected static bool allOk;
	protected static int discarded;
	protected static string messageToProgressbar;

	protected void assignParams (
			Gtk.Notebook notebook,
			Gtk.Label labelProgress, Gtk.ProgressBar progressbar,
			Gtk.Label labelDiscarded,
			Gtk.Label labelResult,
			bool includeImages,
			int imageWidth, int imageHeight,
			bool isWindows, int personID, int sessionID,
			char exportDecimalSeparator)

	{
		this.notebook = notebook;
		this.labelProgress = labelProgress;
		this.progressbar = progressbar;
		this.labelDiscarded = labelDiscarded;
		this.labelResult = labelResult;
		this.includeImages = includeImages;
		this.imageWidth = imageWidth;
		this.imageHeight = imageHeight;
		this.isWindows = isWindows;
		this.personID = personID;
		this.sessionID = sessionID;
		this.exportDecimalSeparator = exportDecimalSeparator;
	}

	protected void prepare(string exportURL)
	{
		this.exportURL = exportURL;

		createOrEmptyDirs();

		cancel = false;
		failedRprocess = false;
		noData = false;
		cannotCopy = false;
		allOk = false;
		labelProgress.Text = "";
		progressbar.Fraction = 0;
		messageToProgressbar = "";
		notebook.CurrentPage = 1;
		discarded = 0;
	}
		
	protected virtual void createOrEmptyDirs()
	{
	}

	protected void createOrEmptyDir(string dir)
	{
		if( ! Directory.Exists(dir))
			Directory.CreateDirectory (dir);
		else {
			DirectoryInfo dirInfo = new DirectoryInfo(dir);
			foreach (FileInfo file in dirInfo.GetFiles())
				file.Delete();
		}
	}

	// public method
	public void Start(string exportURL)
	{
		prepare(exportURL);

		thread = new Thread (new ThreadStart (exportDo));
		GLib.Idle.Add (new GLib.IdleHandler (pulseExportGTK));
		thread.Start();
	}

	protected void exportDo ()
	{
		if(! getData())
		{
			LogB.Information("There's no data");
			noData = true;
			return;
		}

		processSets();
	}

	protected abstract bool getData ();

	protected string getTempSourceFilesDir() {
		return Path.Combine(Path.GetTempPath(), "chronojump_export_source_files");
	}
	protected string getTempProgressDir() {
		return Path.Combine(Path.GetTempPath(), "chronojump_export_progress");
	}

	protected abstract bool processSets ();

	public void Cancel()
	{
		cancel = true;
	}

	protected bool pulseExportGTK ()
	{
		if(! thread.IsAlive || cancel)
		{
			if(cancel)
				LogB.Information("pulseExportGTK cancelled");

			LogB.Information("pulseExportGTK ending here");
			LogB.ThreadEnded();

			progressbar.Fraction = 1;
			notebook.CurrentPage = 0;

			if(discarded > 0)
				labelDiscarded.Text = string.Format(Catalog.GetPluralString(
							"Discarded 1 set for being too short.",
							"Discarded {0} sets for being too short.",
							discarded), discarded);

			if(cancel)
				labelResult.Text = Catalog.GetString("Cancelled.");
			else if (noData)
				labelResult.Text = Catalog.GetString("Missing data.");
			else if (failedRprocess)
				labelResult.Text = Catalog.GetString("Error doing operation.");
			else if (cannotCopy)
				labelResult.Text = string.Format(Catalog.GetString("Cannot copy to {0} "), exportURL);
			else
			{
				labelResult.Text = string.Format(Catalog.GetString("Exported to {0}"), exportURL);// +
						//Constants.GetSpreadsheetString(CSVExportDecimalSeparator)
						//);

				allOk = true;
			}

			Button_done.Click();

			return false;
		}

		DirectoryInfo dirInfo = new DirectoryInfo(getTempProgressDir());
		//LogB.Information(string.Format("pulse files: {0}", dirInfo.GetFiles().Length));

		int fileCount = dirInfo.GetFiles().Length;
		if(fileCount == 0) {
			labelProgress.Text = messageToProgressbar;
			progressbar.Pulse();
		} else {
			setProgressBarTextAndFractionPrepare (fileCount);
		}

		Thread.Sleep (100);
		//Log.Write(" (pulseForceSensorExportGTK:" + thread.ThreadState.ToString() + ") ");
		return true;
	}

	protected bool copyImages(string sourceFolderURL, string exportURL, string destFolderName)
	{
		LogB.Information("going to copy export files with images ...");
		if( ! Directory.Exists(exportURL))
			Directory.CreateDirectory (exportURL);

		try {
			DirectoryInfo sourceDirInfo = new DirectoryInfo(sourceFolderURL);
			string destFolderURL = Path.Combine(exportURL, destFolderName);

			Directory.CreateDirectory (destFolderURL);

			foreach (FileInfo file in sourceDirInfo.GetFiles())
			{
				file.CopyTo(Path.Combine(destFolderURL, file.Name), true);
				//note CopyTo needs a fully qualified name (on my linux machine, a folder works, but not in others)
			}
		} catch {
			return false;
		}

		LogB.Information("done copy export files with images!");
		return true;
	}

	protected abstract void setProgressBarTextAndFractionPrepare (int fileCount);

	//forceSensor inherits different
	protected virtual void setProgressBarTextAndFractionDo (int current, int total)
	{
		labelProgress.Text = string.Format(Catalog.GetString("Exporting {0}/{1}"),
				current, total);
		progressbar.Fraction = UtilAll.DivideSafeFraction(current, total);
	}

	//folder or .csv depending on includeImages
	//this provides a way to open file or folder from the main gui
	public bool AllOk {
		get { return allOk; }
	}
	public string ExportURL {
		get { return exportURL; }
	}
	public int Discarded {
		get { return discarded; }
	}
}
