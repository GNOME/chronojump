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


public partial class ChronoJumpWindow
{
	[Widget] Gtk.Button button_sprint;

	private void createTreeView_runs_interval_sprint (Gtk.TreeView tv)
	{
		UtilGtk.RemoveColumns(tv);
		button_sprint.Sensitive = false;

		tv.HeadersVisible=true;

		int count = 0;
		//tv.AppendColumn (Catalog.GetString ("Type"), new CellRendererText(), "text", count++);
		//tv.AppendColumn (Catalog.GetString ("ID"), new CellRendererText(), "text", count++);
		//tv.AppendColumn (Catalog.GetString ("Total time"), new CellRendererText(), "text", count++);
		tv.AppendColumn ("Type", new CellRendererText(), "text", count++);
		tv.AppendColumn ("ID", new CellRendererText(), "text", count++);
		tv.AppendColumn ("Total time", new CellRendererText(), "text", count++);

		TreeStore store = new TreeStore(typeof (string), typeof (string), typeof (string));
		tv.Model = store;

		if (currentSession == null || currentPerson == null)
		      return;
		
		tv.Selection.Changed -= onTreeviewSprintSelectionEntry;
		tv.Selection.Changed += onTreeviewSprintSelectionEntry;

		string [] array = SqliteRunInterval.SelectRuns(false, currentSession.UniqueID, currentPerson.UniqueID, "");
		foreach (string line in array)
		{
			string [] lineSplit = line.Split(new char[] {':'});
			string [] lineParams = { 
				lineSplit[4],
				lineSplit[1], 
				Util.TrimDecimals(lineSplit[6], preferences.digitsNumber)
			};
			store.AppendValues (lineParams);
		}
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

	private void on_button_sprint_clicked (object o, EventArgs args)
	{
		//test calling sprint.R file
		new Sprint();
	}
}
