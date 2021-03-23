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
	protected Gtk.ProgressBar progressbar;
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
	protected static string messageToProgressbar;

	protected void assignParams (
			Gtk.Notebook notebook,
			Gtk.ProgressBar progressbar,
			Gtk.Label labelResult,
			bool includeImages,
			int imageWidth, int imageHeight,
			bool isWindows, int personID, int sessionID,
			char exportDecimalSeparator)

	{
		this.notebook = notebook;
		this.progressbar = progressbar;
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
		progressbar.Fraction = 0;
		messageToProgressbar = "";
		notebook.CurrentPage = 1;
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

			if(cancel)
				labelResult.Text = Catalog.GetString("Cancelled.");
			else if (noData)
				labelResult.Text = Catalog.GetString("Missing data.");
			else if (failedRprocess)
				labelResult.Text = Catalog.GetString("Error doing operation.");
			else if (cannotCopy)
				labelResult.Text = string.Format(Catalog.GetString("Cannot copy to {0} "), exportURL);
			else
				labelResult.Text = string.Format(Catalog.GetString("Exported to {0}"), exportURL);// +
						//Constants.GetSpreadsheetString(CSVExportDecimalSeparator)
						//);

			Button_done.Click();

			return false;
		}

		DirectoryInfo dirInfo = new DirectoryInfo(getTempProgressDir());
		//LogB.Information(string.Format("pulse files: {0}", dirInfo.GetFiles().Length));

		int fileCount = dirInfo.GetFiles().Length;
		if(fileCount == 0) {
			progressbar.Text = messageToProgressbar;
			progressbar.Pulse();
		} else {
			setProgressBarTextAndFraction(fileCount);
		}

		Thread.Sleep (100);
		//Log.Write(" (pulseForceSensorExportGTK:" + thread.ThreadState.ToString() + ") ");
		return true;
	}

	protected abstract void setProgressBarTextAndFraction (int fileCount);
}
