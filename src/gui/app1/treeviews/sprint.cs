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
 * Copyright (C) 2004-2025   Xavier de Blas <xaviblas@gmail.com>
 */


using System;
using Gtk;
//using Glade;
using Mono.Unix;
using System.Collections; //ArrayList
using System.Collections.Generic; //List

public partial class ChronoJumpWindow 
{
	Gtk.TreeView treeview_runs_interval_sprint;
	
	private void createTreeView_runs_interval_sprint (Gtk.TreeView tv)
	{
		LogB.Information("SPRINT create START");
		UtilGtk.RemoveColumns(tv);
		button_sprint.Sensitive = false;
		image_sprint.Sensitive = false;
		button_sprint_save_image.Sensitive = false;
		button_sprint_table_save.Sensitive = false;

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
		string [] runsArray = SqliteRunInterval.SelectRunsSA (
				false, currentSession.UniqueID, currentPerson.UniqueID, "");

		foreach (string line in runsArray)
		{
			//[] lineSplit has run params
			string [] lineSplit = line.Split(new char[] {':'});

			//get intervalTimes
			string intervalTimesString = lineSplit[8];

			string positions = RunInterval.GetSprintPositions(
					Convert.ToDouble(lineSplit[7]), //distanceInterval. == -1 means variable distances
					intervalTimesString,
					SelectRunITypes.RunIntervalTypeDistancesString (lineSplit[4], runITypes) 	//distancesString
					);
			if(positions == "")
				continue;

			string splitTimes = RunInterval.GetSplitTimes(intervalTimesString, preferences.digitsNumber);

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

		string positions = RunInterval.GetSprintPositions(
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
				RunInterval.GetSplitTimes(runI.IntervalTimesString, preferences.digitsNumber),
				Util.TrimDecimals(runI.TimeTotal, preferences.digitsNumber)
				);

		//scroll treeview if needed
		TreePath path = storeSprint.GetPath (iter);
		treeview_runs_interval_sprint.ScrollToCell (path, null, true, 0, 0);
		LogB.Information("SPRINT add END");
	}

	private void onTreeviewSprintSelectionEntry (object o, EventArgs args)
	{
		ITreeModel model;
		TreeIter iter;

		// you get the iter and the model if something is selected
		if (((TreeSelection)o).GetSelected(out model, out iter))
		{
			//only allow sprint calculation when there are three tracks
			string splitTimes = (string) model.GetValue(iter, 3);
			if(splitTimes.Split(new char[] {';'}).Length >= 3)
				button_sprint.Sensitive = true;
			else
				button_sprint.Sensitive = false;
		}
		else
			button_sprint.Sensitive = false;
	}

	public static bool GetSelectedSprint (Gtk.TreeView tv)
	{
		ITreeModel model;
		TreeIter iter;

		if (tv.Selection.GetSelected (out model, out iter))
		{
			 string positions = (string) model.GetValue(iter, 2);
			 positions = Util.ChangeChars(positions, ",", ".");
			 positions = "0;" + positions;

			 string splitTimes = (string) model.GetValue(iter, 3);
			 splitTimes = Util.ChangeChars(splitTimes, ",", ".");
			 splitTimes = "0;" + splitTimes;

			 sprintRGraph = new SprintRGraph (
					 positions,
					 splitTimes,
					 currentPersonSession.Weight, //TODO: can be more if extra weight
					 currentPersonSession.Height,
					 currentPerson.Name,
					 25
					 );
			return true;
		}
		return false;
	}
}
