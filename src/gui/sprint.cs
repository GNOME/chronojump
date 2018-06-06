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
 * Copyright (C) 2017   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
using System.IO;
using Gtk;
using Glade;
using System.Text; //StringBuilder
using System.Collections; //ArrayList
using System.Collections.Generic; //List<T>
using Mono.Unix;


public partial class ChronoJumpWindow
{
	[Widget] Gtk.Button button_sprint;
	[Widget] Gtk.Viewport viewport_sprint;
	[Widget] Gtk.Image image_sprint;
	[Widget] Gtk.Button button_sprint_save_image;

	static Sprint sprint;
	TreeStore storeSprint;

	private void createTreeView_runs_interval_sprint (Gtk.TreeView tv)
	{
		LogB.Information("SPRINT create START");
		UtilGtk.RemoveColumns(tv);
		button_sprint.Sensitive = false;
		image_sprint.Sensitive = false;
		button_sprint_save_image.Sensitive = false;

		tv.HeadersVisible=true;

		int count = 0;
		tv.AppendColumn (Catalog.GetString("Type"), new CellRendererText(), "text", count++);
		tv.AppendColumn ("ID", new CellRendererText(), "text", count++);
		tv.AppendColumn (Catalog.GetString("Distances"), new CellRendererText(), "text", count++);
		tv.AppendColumn (Catalog.GetString("Split times"), new CellRendererText(), "text", count++);
		tv.AppendColumn (Catalog.GetString("Total time"), new CellRendererText(), "text", count++);

		storeSprint = new TreeStore(
				typeof (string), typeof (string), typeof (string),
				typeof (string), typeof (string));
		tv.Model = storeSprint;

		if (currentSession == null || currentPerson == null)
		      return;
		
		tv.Selection.Changed -= onTreeviewSprintSelectionEntry;
		tv.Selection.Changed += onTreeviewSprintSelectionEntry;

		List<object> runITypes = SqliteRunIntervalType.SelectRunIntervalTypesNew("", false);
		string [] runsArray = SqliteRunInterval.SelectRuns(
				false, currentSession.UniqueID, currentPerson.UniqueID, "");

		foreach (string line in runsArray)
		{
			//[] lineSplit has run params
			string [] lineSplit = line.Split(new char[] {':'});

			//get intervalTimes
			string intervalTimesString = lineSplit[8];

			string positions = getSprintPositions(
					Convert.ToDouble(lineSplit[7]), //distanceInterval. == -1 means variable distances
					intervalTimesString,
					runIntervalTypeDistances(lineSplit[4], runITypes) 	//distancesString
					);
			if(positions == "")
				continue;

			string splitTimes = getSplitTimes(intervalTimesString);

			string [] lineParams = { 
				lineSplit[4],
				lineSplit[1],
				positions,
				splitTimes,
				Util.TrimDecimals(lineSplit[6], preferences.digitsNumber)
			};
			storeSprint.AppendValues (lineParams);
		}
		LogB.Information("SPRINT create END");
	}

	public void addTreeView_runs_interval_sprint (RunInterval runI, RunType runIType)
	{
		LogB.Information("SPRINT add START");
		if(storeSprint == null)
		{
			createTreeView_runs_interval_sprint (treeview_runs_interval_sprint);
			return;
		}

		string positions = getSprintPositions(
				runI.DistanceInterval, 		//distanceInterval. == -1 means variable distances
				runI.IntervalTimesString,
				runIType.DistancesString 	//distancesString
				);
		if(positions == "")
			return;

		TreeIter iter = new TreeIter();
		bool iterOk = storeSprint.GetIterFirst(out iter);
		if(! iterOk)
			iter = new TreeIter();

		iter = storeSprint.AppendValues (
				runI.Type,
				runI.UniqueID.ToString(),
				positions,
				getSplitTimes(runI.IntervalTimesString),
				Util.TrimDecimals(runI.TimeTotal, preferences.digitsNumber)
				);

		//scroll treeview if needed
		TreePath path = storeSprint.GetPath (iter);
		treeview_runs_interval_sprint.ScrollToCell (path, null, true, 0, 0);
		LogB.Information("SPRINT add END");
	}

	private string getSprintPositions(double distanceInterval, string intervalTimesString, string distancesString)
	{
		string positions = "";
		string [] intervalTimesSplit = intervalTimesString.Split(new char[] {'='});
		if(! distancesString.Contains("R") ) 	//discard RSA
		{
			string sep = "";
			for(int i=0; i < intervalTimesSplit.Length; i ++)
			{
				positions += sep + Util.GetRunITotalDistance(distanceInterval, distancesString, i+1);
				sep = ";";
			}

			//format positions
			positions = Util.ChangeChars(positions, "-", ";");
		}
		return positions;
	}

	private string getSplitTimes(string intervalTimesString)
	{
		string [] intervalTimesSplit = intervalTimesString.Split(new char[] {'='});

		//manage accumulated time
		double timeAccumulated = 0;
		string splitTimes = "";
		string sep = "";
		foreach(string time in intervalTimesSplit)
		{
			double timeD = Convert.ToDouble(time);
			timeAccumulated += timeD;
			splitTimes += sep + Util.TrimDecimals(timeAccumulated, preferences.digitsNumber);
			sep = ";";
		}

		return splitTimes;
	}

	private string runIntervalTypeDistances(string runTypeEnglishName, List<object> runITypes)
	{
		foreach(SelectRunITypes type in runITypes)
			if(type.NameEnglish == runTypeEnglishName)
				return(type.DistancesString);

		return "";
	}

	private void onTreeviewSprintSelectionEntry (object o, EventArgs args)
	{
		TreeModel model;
		TreeIter iter;

		// you get the iter and the model if something is selected
		if (((TreeSelection)o).GetSelected(out model, out iter))
			button_sprint.Sensitive = true;
	}

	public static bool GetSelectedSprint (Gtk.TreeView tv)
	{
		TreeModel model;
		TreeIter iter;

		if (tv.Selection.GetSelected (out model, out iter))
		{
			 string positions = (string) model.GetValue(iter, 2);
			 positions = Util.ChangeChars(positions, ",", ".");
			 positions = "0;" + positions;

			 string splitTimes = (string) model.GetValue(iter, 3);
			 splitTimes = Util.ChangeChars(splitTimes, ",", ".");
			 splitTimes = "0;" + splitTimes;

			 sprint = new Sprint(
					 positions,
					 splitTimes,
					 currentPersonSession.Weight, //TODO: can be more if extra weight
					 currentPersonSession.Height,
					 25);
			return true;
		}
		return false;
	}


	private void on_button_sprint_clicked (object o, EventArgs args)
	{
		if(! GetSelectedSprint(treeview_runs_interval_sprint))
		{
			new DialogMessage(Constants.MessageTypes.WARNING, Catalog.GetString("Error"));
			return;
		}

		on_button_sprint_do ();
	}

	private bool on_button_sprint_do ()
	{
		button_sprint_save_image.Sensitive = false;
		if(currentPersonSession.Weight == 0)
		{
			new DialogMessage(Constants.MessageTypes.WARNING,
					Catalog.GetString("Error, weight of the person cannot be 0"));
			return false;
		}

		if(currentPersonSession.Height == 0)
		{
			new DialogMessage(Constants.MessageTypes.WARNING,
					Catalog.GetString("Error, height of the person cannot be 0"));
			return false;
		}

		if(! sprint.IsDataOk())
		{
			new DialogMessage(Constants.MessageTypes.WARNING,
					Catalog.GetString("This data does not seem a sprint.") + "\n\n" +
					sprint.ErrorMessage);
			return false;
		}

		Util.FileDelete(UtilEncoder.GetSprintImage());

		image_sprint.Sensitive = false;

		bool success = sprint.CallR(
				viewport_sprint.Allocation.Width -5,
				viewport_sprint.Allocation.Height -5);

		if(! success)
		{
			new DialogMessage(Constants.MessageTypes.WARNING, Catalog.GetString("This data does not seem a sprint."));
			return false;
		}

		while ( ! Util.FileReadable(UtilEncoder.GetSprintImage()));

		image_sprint = UtilGtk.OpenImageSafe(
				UtilEncoder.GetSprintImage(),
				image_sprint);
		image_sprint.Sensitive = true;
		button_sprint_save_image.Sensitive = true;
		return true;
	}

	private void on_button_sprint_save_image_clicked (object o, EventArgs args)
	{
		checkFile(Constants.CheckFileOp.RUNS_SPRINT_SAVE_IMAGE);
	}

	private void on_button_runs_sprint_save_image_selected (string destination)
	{
		try {
			File.Copy(UtilEncoder.GetSprintImage(), destination, true);
		} catch {
			string myString = string.Format(
					Catalog.GetString("Cannot save file {0} "), destination);
			new DialogMessage(Constants.MessageTypes.WARNING, myString);
		}
	}

	private void on_overwrite_file_runs_sprint_save_image_accepted (object o, EventArgs args)
	{
		on_button_runs_sprint_save_image_selected(exportFileName);

		string myString = string.Format(Catalog.GetString("Saved to {0}"), exportFileName);
		new DialogMessage(Constants.MessageTypes.INFO, myString);
	}

}
