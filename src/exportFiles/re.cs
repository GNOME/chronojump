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

using System;
using System.IO; 			//Directory, ...
using System.Collections; 		//ArrayList
using System.Collections.Generic; 	//List<T>
using System.Threading;
using Mono.Unix;

public class RunEncoderExport : ExportFiles
{
	private double startAccel;
	private bool plotRawAccel;
	private bool plotFittedAccel;
	private bool plotRawForce;
	private bool plotFittedForce;
	private bool plotRawPower;
	private bool plotFittedPower;
	private char exportDecimalSeparator;

	private List<RunEncoder> re_l;
	ArrayList personSession_l;
	private ArrayList reEx_l;
	private List<TriggerList> triggerListOfLists;

	//constructor
	public RunEncoderExport (
			Gtk.Notebook notebook,
			Gtk.ProgressBar progressbar,
			Gtk.Label labelResult,
			bool includeImages,
			int imageWidth, int imageHeight,
			bool isWindows,
			int personID,
			int sessionID,
			double startAccel,
			bool plotRawAccel, bool plotFittedAccel,
			bool plotRawForce, bool plotFittedForce,
			bool plotRawPower, bool plotFittedPower,
			char exportDecimalSeparator
			)
	{
		Button_done = new Gtk.Button();

		assignParams(notebook, progressbar, labelResult, includeImages,
				imageWidth, imageHeight, isWindows, personID, sessionID);

		this.startAccel = startAccel;
		this.plotRawAccel = plotRawAccel;
		this.plotFittedAccel = plotFittedAccel;
		this.plotRawForce = plotRawForce;
		this.plotFittedForce = plotFittedForce;
		this.plotRawPower = plotRawPower;
		this.plotFittedPower = plotFittedPower;
		this.exportDecimalSeparator = exportDecimalSeparator;
	}

	private string getTempGraphsDir() {
		return Path.Combine(Path.GetTempPath(), "chronojump_race_analyzer_export_graphs");
	}
	
	protected override void createOrEmptyDirs()
	{
		createOrEmptyDir(getTempProgressDir());
		createOrEmptyDir(getTempGraphsDir());
	}

	protected override bool getData ()
	{
		re_l = SqliteRunEncoder.Select(false, -1, personID, sessionID);
		personSession_l = SqlitePersonSession.SelectCurrentSessionPersons(sessionID, true);
		reEx_l = SqliteRunEncoderExercise.Select (false, -1, false);

		//get all the triggers to not be opening and closing sqlite on processSets
		triggerListOfLists = new List<TriggerList>();
		Sqlite.Open();
		foreach(RunEncoder re in re_l)
		{
			TriggerList triggerListRunEncoder = new TriggerList(
					SqliteTrigger.Select(
						true, Trigger.Modes.RACEANALYZER,
						re.UniqueID)
					);
			triggerListOfLists.Add(triggerListRunEncoder);
		}
		Sqlite.Close();

		return re_l.Count > 0;
	}

	protected override bool processSets ()
	{
		Person p = new Person();
		PersonSession ps = new PersonSession();

		List<RunEncoderGraphExport> rege_l = new List<RunEncoderGraphExport>();

		//int count = 1;
		int element = -1; //used to sync re_l[element] with triggerListOfLists[element]
		foreach(RunEncoder re in re_l)
		{
			element ++;

			// 1) checks
			//check fs is ok
			if(re == null || ! Util.FileExists(re.FullURL))
				continue;

			//check fs has data
			List<string> contents = Util.ReadFileAsStringList(re.FullURL);
			if(contents.Count < 3)
			{
				//new DialogMessage(Constants.MessageTypes.WARNING, Constants.FileEmptyStr());
				//return;
				continue;
			}

			// 2) get the person
			bool found = false;
			foreach(PersonAndPS paps in personSession_l)
			{
				if(paps.p.UniqueID == re.PersonID)
				{
					p = paps.p;
					ps = paps.ps;

					found = true;
					break;
				}
			}
			if(! found)
				continue;

			// 3) get the exercise
			found = false;
			RunEncoderExercise reEx = new RunEncoderExercise();
			foreach(RunEncoderExercise reExTemp in reEx_l)
				if(reExTemp.UniqueID == re.ExerciseID)
				{
					reEx = reExTemp;
					found = true;
					break;
				}
			if(! found)
				continue;

			// 4) create the export row
			string title = Util.ChangeSpaceAndMinusForUnderscore(p.Name) + "-" +
				Util.ChangeSpaceAndMinusForUnderscore(reEx.Name);

			RunEncoderGraphExport rege = new RunEncoderGraphExport (
					re.FullURL,
					ps.Weight, ps.Height,
					re.Device,
					re.Temperature, re.Distance,
					reEx, title, re.DateTimePublic,
					triggerListOfLists[element],
					re.Comments
					);
			rege_l.Add(rege);

		}

		Util.FileDelete(RunEncoder.GetCSVResultsFileName());

		// call the graph

		if(rege_l.Count > 0)
		{
			RunEncoderGraph reg = new RunEncoderGraph(
					startAccel,
					plotRawAccel, plotFittedAccel,
					plotRawForce, plotFittedForce,
					plotRawPower, plotFittedPower,
					rege_l,
					exportDecimalSeparator,
					includeImages
					);

			if(! reg.CallR(imageWidth, imageHeight, false))
				return false;
		}

		LogB.Information("Waiting creation of file... ");
		while ( ! ( Util.FileReadable(RunEncoder.GetCSVResultsFileName()) || cancel ) )
			;

		if(cancel)
			return false;

		if(includeImages)
		{
			LogB.Information("going to copy export files with images ...");
			if( ! Directory.Exists(exportURL))
                                Directory.CreateDirectory (exportURL);

			try{
				string sourceFolder = getTempGraphsDir();
				DirectoryInfo sourceDirInfo = new DirectoryInfo(sourceFolder);

				string destFolder = Path.Combine(exportURL, "chronojump_race_analyzer_export_graphs");
				Directory.CreateDirectory (destFolder);

				foreach (FileInfo file in sourceDirInfo.GetFiles())
					file.CopyTo(destFolder, true);
			} catch {
				return false;
			}

			//LogB.Information("done copy export files with images!");
		}

		// copy the CSV
		File.Copy(RunEncoder.GetCSVResultsFileName(), exportURL, true);

		return true;
	}

	protected override void setProgressBarTextAndFraction (int fileCount)
	{
		progressbar.Text = string.Format(Catalog.GetString("Exporting {0}/{1}"),
				fileCount, re_l.Count);
		progressbar.Fraction = UtilAll.DivideSafeFraction(fileCount, re_l.Count);
	}
}
