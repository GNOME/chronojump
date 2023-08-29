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
 * Copyright (C) 2023   Xavier de Blas <xaviblas@gmail.com>
 */

using System;
using System.IO; 			//Directory, ...
using System.Collections; 		//ArrayList
using System.Collections.Generic; 	//List<T>
using System.Threading;

public class SprintExport : ExportFiles
{
	private int digitsNumber;

	private List<RunInterval> ri_l;
	private List<object> riTypes_l; //class in sqlite/usefulObjects.cs

	//constructor
	public SprintExport (
			Gtk.Notebook notebook,
			Gtk.Label labelProgress,
			Gtk.ProgressBar progressbar,
			Gtk.Label labelDiscarded,
			Gtk.Label labelResult,
			bool includeImages,
			int imageWidth, int imageHeight,
			bool isWindows,
			int personID,
			int sessionID,
			char exportDecimalSeparator,
			int digitsNumber
			)
	{
		Button_done = new Gtk.Button();

		assignParams(notebook, labelProgress, progressbar, labelDiscarded, labelResult, includeImages,
				imageWidth, imageHeight, isWindows, personID, sessionID, exportDecimalSeparator);

		this.digitsNumber = digitsNumber;
	}

	private string getTempGraphsDir() {
		return Path.Combine(Path.GetTempPath(), "chronojump_sprint_export_graphs");
	}

	protected override void createOrEmptyDirs()
	{
		createOrEmptyDir(getTempProgressDir());
		createOrEmptyDir(getTempGraphsDir());
	}

	protected override bool getData ()
	{
		ri_l = SqliteRunInterval.SelectRuns (false, sessionID, personID, "",
				Sqlite.Orders_by.DEFAULT, 0, false);
		personSession_l = SqlitePersonSession.SelectCurrentSessionPersons(sessionID, true);
		riTypes_l = SqliteRunIntervalType.SelectRunIntervalTypesNew("", false);

		return ri_l.Count > 0;
	}

	//runInterval has not a file of data (like forceSensor or runEncoder)
	//so just manage the positions and splitTimes
	protected override bool processSets ()
	{
		Person p = new Person();
		PersonSession ps = new PersonSession();

		List<SprintRGraphExport> sprge_l = new List<SprintRGraphExport>();

		//int element = -1; //used to sync re_l[element] with triggerListOfLists[element]
		foreach(RunInterval ri in ri_l)
		{
			//element ++;

			// get the person
			bool found = false;
			foreach(PersonAndPS paps in personSession_l)
			{
				if(paps.p.UniqueID == ri.PersonID)
				{
					p = paps.p;
					ps = paps.ps;

					found = true;
					break;
				}
			}
			if(! found)
				continue;

			/*
			// get the type
			found = false;
			RunType riEx = new RunType();
			foreach(RunType riExTemp in riEx_l)
				if(reExTemp.UniqueID == ri.ExerciseID)
				{
					reEx = reExTemp;
					found = true;
					break;
				}
			if(! found)
				continue;
				*/

			// get the positions
			string positions = RunInterval.GetSprintPositions(
					ri.DistanceInterval, //distanceInterval. == -1 means variable distances
					ri.IntervalTimesString,
					SelectRunITypes.RunIntervalTypeDistancesString (ri.Type, riTypes_l) 	// distancesString
					);
			if(positions == "") //RSAs are discarded
				continue;

			//get the splitTimes
			string splitTimes = RunInterval.GetSplitTimes(ri.IntervalTimesString, digitsNumber);

			//discard sprints with less than 3 tracks
			if(splitTimes.Split(new char[] {';'}).Length < 3)
				continue;

			//discard sprints that are not sprint
			SprintRGraph sprintRGraph = new SprintRGraph (
					positions,
					splitTimes,
					ps.Weight, 	//TODO: can be more if extra weight
					ps.Height,
					"",
					25);

			 if(! sprintRGraph.IsDataOk())
				continue;

			// create the export row
			string title = Util.ChangeSpaceAndMinusForUnderscore(p.Name) + "-" +
				Util.ChangeSpaceAndMinusForUnderscore(ri.Type);

			SprintRGraphExport sprge = new SprintRGraphExport (
					positions, splitTimes,
					ps.Weight, //TODO: can be more if extra weight
					ps.Height,
					title,
					25);
			sprge_l.Add(sprge);
		}

		//discarded = ri_l.Count - sprge_l.Count;
		//better use discarded below

		Util.FileDelete(RunInterval.GetCSVResultsURL());

		//no data, maybe because all the tests have just two tracks and cannot be processed as sprint
		if(sprge_l.Count == 0)
		{
			noData = true;
			return false;
		}

		// call the graph
		SprintRGraph s = new SprintRGraph (
				sprge_l,
				exportDecimalSeparator,
				includeImages
				);

		if(! s.CallR(imageWidth, imageHeight, false))
		{
			failedRprocess = true;
			return false;
		}

		LogB.Information("Waiting creation of file... ");
		while ( ! ( Util.FileReadable(RunInterval.GetCSVResultsURL()) || cancel ) )
			;

		//use this discarded because R discards also
		List<string> setsProcessedByR = Util.ReadFileAsStringList(RunInterval.GetCSVResultsURL());
		discarded = ri_l.Count - (setsProcessedByR.Count -1); //-1 for the csv header
		if(setsProcessedByR.Count -1 <= 0)
		{
			noData = true;
			return false;
		}

		if(cancel)
			return false;

		if(includeImages && ! copyImages(getTempGraphsDir(), exportURL,
					"chronojump_races_sprint_export_graphs"))
			return false;

		// copy the CSV
		//if includeImages, exportURL is a dir, so need a filename to have File.Copy on all systems
		if(includeImages)
			File.Copy(RunInterval.GetCSVResultsURL(), Path.Combine(exportURL, RunInterval.GetCSVResultsFileName()), true);
		else
			File.Copy(RunInterval.GetCSVResultsURL(), exportURL, true);

		return true;
	}

	protected override void setProgressBarTextAndFractionPrepare (int fileCount)
	{
		setProgressBarTextAndFractionDo (fileCount, ri_l.Count);
	}
}

