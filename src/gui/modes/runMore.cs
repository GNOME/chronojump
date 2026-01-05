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
 * Copyright (C) 2004-2026   Xavier de Blas <xaviblas@gmail.com>
 */

using System;
using Gtk;
//using Glade;
using System.Text; //StringBuilder
using System.Collections; //ArrayList
using System.Collections.Generic; //List<T>
using System.Threading;
using Mono.Unix;


public class RunsMoreWindow : EventMoreWindow 
{
	Gtk.Window jumps_runs_more;
	static RunsMoreWindow RunsMoreWindowBox;
	
	private double selectedDistance;
	
	RunsMoreWindow (Gtk.Window parent, bool testOrDelete)
	{
		/*
		Glade.XML gladeXML;
		gladeXML = Glade.XML.FromAssembly (Util.GetGladePath() + "jumps_runs_more.glade", "jumps_runs_more", null);
		gladeXML.Autoconnect(this);
		*/
		Gtk.Builder builder = new Gtk.Builder (null, Util.GetGladePath () + "jumps_runs_more.glade", null);
		connectWidgetsEventMore (builder);
		jumps_runs_more = (Gtk.Window) builder.GetObject ("jumps_runs_more");
		builder.Autoconnect (this);

		this.parent = parent;
		this.testOrDelete = testOrDelete;
		
		if(!testOrDelete)
			jumps_runs_more.Title = Catalog.GetString("Delete test type defined by user");
		
		//put an icon to window
		UtilGtk.IconWindow(jumps_runs_more);

		//manage window color
		if(! Config.UseSystemColor)
			UtilGtk.WindowColor(jumps_runs_more, Config.ColorBackground);

		selectedEventType = EventType.Types.RUN.ToString();
		//name, distance, description
		store = new TreeStore(typeof (string), typeof (string), typeof (string));
		
		initializeThings();
	}
	
	static public RunsMoreWindow Show (Gtk.Window parent, bool testOrDelete)
	{
		if (RunsMoreWindowBox == null) {
			RunsMoreWindowBox = new RunsMoreWindow (parent, testOrDelete);
		}
		RunsMoreWindowBox.jumps_runs_more.Show ();
		
		return RunsMoreWindowBox;
	}
	
	protected override void createTreeView (Gtk.TreeView tv) {
		tv.HeadersVisible=true;
		int count = 0;
		
		tv.AppendColumn ( Catalog.GetString ("Name"), new CellRendererText(), "text", count++);
		tv.AppendColumn ( Catalog.GetString ("Distance"), new CellRendererText(), "text", count++);
		tv.AppendColumn ( Catalog.GetString ("Description"), new CellRendererText(), "text", count++);
	}
	
	protected override void fillTreeView (Gtk.TreeView tv, TreeStore store) 
	{
		//select data without inserting an "all jumps", and not obtain only name of jump
		string [] myRunTypes = SqliteRunType.SelectRunTypes("", false);

		//remove typesTranslated
		typesTranslated = new String [myRunTypes.Length];
		int count = 0;

		foreach (string myType in myRunTypes) {
			string [] myStringFull = myType.Split(new char[] {':'});
			if(myStringFull[2] == "0") {
				myStringFull[2] = Catalog.GetString("Not defined");
			}

			RunType tempType = new RunType (myStringFull[1]);
			string description  = getDescriptionLocalised(tempType, myStringFull[3]);

			//if we are going to execute: show all types
			//if we are going to delete: show user defined types
			if(testOrDelete || ! tempType.IsPredefined)
				store.AppendValues (
						//myStringFull[0], //don't display the uniqueID
						Catalog.GetString(myStringFull[1]),	//name 
						myStringFull[2], 	//distance
						description
						);
			
			//create typesTranslated
			typesTranslated [count++] = myStringFull[1] + ":" + Catalog.GetString(myStringFull[1]);
		}	
	}


	protected override void onSelectionEntry (object o, EventArgs args)
	{
		ITreeModel model;
		TreeIter iter;
		selectedEventName = "-1";
		selectedDistance = 0;
		selectedDescription = "";

		if (((TreeSelection)o).GetSelected(out model, out iter)) {
			string translatedName = (string) model.GetValue (iter, 0);
			//get name in english
			selectedEventName = Util.FindOnArray(':', 1, 0, translatedName, typesTranslated);
			if( (string) model.GetValue (iter, 1) == Catalog.GetString("Not defined") ) {
				selectedDistance = 0;
			} else {
				selectedDistance = Convert.ToDouble( (string) model.GetValue (iter, 1) );
			}
			selectedDescription = (string) model.GetValue (iter, 2);
			
			if(testOrDelete) {
				button_accept.Sensitive = true;
				//update graph image test on main window
				button_selected.Click();
			} else
				button_delete_type.Sensitive = true;
		}
	}
	
	protected override void on_row_double_clicked (object o, Gtk.RowActivatedArgs args)
	{
		//return if we are to delete a test
		if(!testOrDelete)
			return;

		TreeView tv = (TreeView) o;
		ITreeModel model;
		TreeIter iter;

		if (tv.Selection.GetSelected (out model, out iter)) {
			string translatedName = (string) model.GetValue (iter, 0);
			//get name in english
			selectedEventName = Util.FindOnArray(':', 1, 0, translatedName, typesTranslated);
			
			if( (string) model.GetValue (iter, 1) == Catalog.GetString("Not defined") ) {
				selectedDistance = 0;
			} else {
				selectedDistance = Convert.ToDouble( (string) model.GetValue (iter, 1) );
			}
			selectedDescription = (string) model.GetValue (iter, 2);

			//activate on_button_accept_clicked()
			button_accept.Activate();
		}
	}
	
	protected override void deleteTestLine() {
		SqliteRunType.Delete(selectedEventName);
		
		//delete from typesTranslated
		string row = Util.FindOnArray(':',0, -1, selectedEventName, typesTranslated);
		LogB.Information("row " + row);
		typesTranslated = Util.DeleteString(typesTranslated, row);
	}

	protected override string [] findTestTypesInSessions() {
		return SqliteRun.SelectRunsSA (false, -1, -1, selectedEventName,
				Sqlite.Orders_by.DEFAULT, -1);
	}

	void on_button_cancel_clicked (object o, EventArgs args)
	{
		RunsMoreWindowBox.jumps_runs_more.Hide();
		RunsMoreWindowBox = null;
	}
	
	void on_jumps_runs_more_delete_event (object o, DeleteEventArgs args)
	{
		RunsMoreWindowBox.jumps_runs_more.Hide();
		RunsMoreWindowBox = null;
	}
	
	void on_button_accept_clicked (object o, EventArgs args)
	{
		RunsMoreWindowBox.jumps_runs_more.Hide();
	}
	
	//when a run is done using runsMoreWindow, the accept doesn't destroy this instance, because 
	//later we need data from it.
	//This is used for destroying, then if a new run type is added, it will be shown at first time clicking "more" button
	public void Destroy() {		
		RunsMoreWindowBox = null;
	}

	public double SelectedDistance {
		get { return selectedDistance; }
	}
}
