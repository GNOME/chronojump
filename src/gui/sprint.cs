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
using Gtk;
using Glade;
using System.Text; //StringBuilder
using System.Collections; //ArrayList
using System.Collections.Generic; //List<T>


public partial class ChronoJumpWindow
{
	[Widget] Gtk.Button button_sprint;
	static Sprint sprint;

	private void createTreeView_runs_interval_sprint (Gtk.TreeView tv)
	{
		UtilGtk.RemoveColumns(tv);
		button_sprint.Sensitive = false;

		tv.HeadersVisible=true;

		int count = 0;
		tv.AppendColumn ("Type", new CellRendererText(), "text", count++);
		tv.AppendColumn ("ID", new CellRendererText(), "text", count++);
		tv.AppendColumn ("Distances", new CellRendererText(), "text", count++);
		tv.AppendColumn ("Split times", new CellRendererText(), "text", count++);
		tv.AppendColumn ("Total time", new CellRendererText(), "text", count++);

		TreeStore store = new TreeStore(
				typeof (string), typeof (string), typeof (string),
				typeof (string), typeof (string));
		tv.Model = store;

		if (currentSession == null || currentPerson == null)
		      return;
		
		tv.Selection.Changed -= onTreeviewSprintSelectionEntry;
		tv.Selection.Changed += onTreeviewSprintSelectionEntry;

		List<object> runITypes = SqliteRunIntervalType.SelectRunIntervalTypesNew("", false);
		string [] runsArray = SqliteRunInterval.SelectRuns(
				false, currentSession.UniqueID, currentPerson.UniqueID, "");

		foreach (string line in runsArray)
		{
			string [] lineSplit = line.Split(new char[] {':'});
			string positions = runIntervalDistances(lineSplit[4], runITypes);
			if(positions == "")
				continue;

			positions = Util.ChangeChars(positions, "-", ";");

			string intervalTimes = lineSplit[8];
			string [] intervalTimesSplit = intervalTimes.Split(new char[] {'='});

			//accumulated
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

			string [] lineParams = { 
				lineSplit[4],
				lineSplit[1],
				positions,
				splitTimes,
				Util.TrimDecimals(lineSplit[6], preferences.digitsNumber)
			};
			store.AppendValues (lineParams);
		}
	}

	private string runIntervalDistances(string runTypeEnglishName, List<object> runITypes)
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
		if (((TreeSelection)o).GetSelected(out model, out iter)) {
			button_sprint.Sensitive = true;
		}
	}

	public static bool GetSelectedSprint (Gtk.TreeView tv)
	{
		TreeModel model;
		TreeIter iter;

		if (tv.Selection.GetSelected (out model, out iter))
		{
			 string positions = (string) model.GetValue(iter, 2);
			 positions = Util.ChangeChars(positions, ",", ".");
			 //add start TODO: check if needed
			 positions = "0;" + positions;

			 string splitTimes = (string) model.GetValue(iter, 3);
			 splitTimes = Util.ChangeChars(splitTimes, ",", ".");
			 //add start TODO: check if needed
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
			new DialogMessage(Constants.MessageTypes.WARNING, "Error");
			return;
		}

		sprint.CallR();
		new DialogMessage(Constants.MessageTypes.WARNING, "Ok");
	}
}
