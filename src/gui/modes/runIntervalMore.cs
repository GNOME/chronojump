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


public class RunsIntervalMoreWindow : EventMoreWindow 
{
	Gtk.Window jumps_runs_more;

	static RunsIntervalMoreWindow RunsIntervalMoreWindowBox;

	private double selectedDistance;
	private bool selectedTracksLimited;
	private int selectedLimitedValue;
	private bool selectedUnlimited;
	private string selectedDistancesString;
	
	RunsIntervalMoreWindow (Gtk.Window parent, bool testOrDelete)
	{
		/*
		//the glade window is the same as jumps_more
		Glade.XML gladeXML;
		gladeXML = Glade.XML.FromAssembly (Util.GetGladePath() + "jumps_runs_more.glade", "jumps_runs_more", "chronojump");
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
		
		selectedEventType = EventType.Types.RUN.ToString();
		//name, distance, limited by tracks or seconds, limit value, description
		store = new TreeStore(typeof (string), typeof (string), typeof(string),
				typeof (string), typeof (string) );
		
		initializeThings();
	}
	
	static public RunsIntervalMoreWindow Show (Gtk.Window parent, bool testOrDelete)
	{
		if (RunsIntervalMoreWindowBox == null) {
			RunsIntervalMoreWindowBox = new RunsIntervalMoreWindow (parent, testOrDelete);
		}
		RunsIntervalMoreWindowBox.jumps_runs_more.Show ();
		
		return RunsIntervalMoreWindowBox;
	}
	
	protected override void createTreeView (Gtk.TreeView tv) {
		tv.HeadersVisible=true;
		int count = 0;

		tv.AppendColumn ( Catalog.GetString ("Name"), new CellRendererText(), "text", count++);
		tv.AppendColumn ( Catalog.GetString ("Distance"), new CellRendererText(), "text", count++);
		tv.AppendColumn ( Catalog.GetString ("Limited by"), new CellRendererText(), "text", count++);
		tv.AppendColumn ( Catalog.GetString ("Limited value"), new CellRendererText(), "text", count++);
		tv.AppendColumn ( Catalog.GetString ("Description"), new CellRendererText(), "text", count++);
	}
	
	protected override void fillTreeView (Gtk.TreeView tv, TreeStore store) 
	{
		//select data without inserting an "all runs", and not obtain only name of run
		string [] myTypes = SqliteRunIntervalType.SelectRunIntervalTypes("", false);
		
		//remove typesTranslated
		typesTranslated = new String [myTypes.Length];
		int count = 0;

		foreach (string myType in myTypes) {
			string [] myStringFull = myType.Split(new char[] {':'});
			
			string distance = myStringFull[2];
			if(distance == "0") 
				distance = Catalog.GetString("Not defined");
			else if(distance == "-1") 
				distance = myStringFull[7]; //distancesString

			
			//limited
			string myLimiter = "";
			string myLimiterValue = "";
			
			//check if it's unlimited
			if(myStringFull[5] == "1") {
				myLimiter= Catalog.GetString("Unlimited");
				myLimiterValue = "-";
			} else {
				myLimiter = Catalog.GetString("Laps");
				if(myStringFull[3] == "0") {
					myLimiter = Catalog.GetString("Seconds");
				}
				myLimiterValue = "?";
				if(Convert.ToDouble(myStringFull[4]) > 0) {
					myLimiterValue = myStringFull[4];
				}
			}

			RunType tempType = new RunType (myStringFull[1]);
			string description  = getDescriptionLocalised(tempType, myStringFull[6]);

			//if we are going to execute: show all types
			//if we are going to delete: show user defined types
			if(testOrDelete || ! tempType.IsPredefined)
				store.AppendValues (
						//myStringFull[0], //don't display de uniqueID
						Catalog.GetString(myStringFull[1]),	//name 
						distance,		
						myLimiter,		//tracks or seconds or "unlimited"
						myLimiterValue,		//? or exact value (or '-' in unlimited)
						description
						);

			//create typesTranslated
			typesTranslated [count++] = myStringFull[1] + ":" + Catalog.GetString(myStringFull[1]);
		}	
	}

	//puts a value in private member selected
	protected override void onSelectionEntry (object o, EventArgs args)
	{
		ITreeModel model;
		TreeIter iter;
		selectedEventName = "-1";
		selectedDistance = -1;
		selectedTracksLimited = false;
		selectedLimitedValue = 0;
		selectedUnlimited = false; //true if it's an unlimited run
		selectedDescription = "";
		selectedDistancesString = "";

		if (((TreeSelection)o).GetSelected(out model, out iter)) {
			string translatedName = (string) model.GetValue (iter, 0);
			//get name in english
			selectedEventName = Util.FindOnArray(':', 1, 0, translatedName, typesTranslated);

			//selectedDistance = Convert.ToDouble( (string) model.GetValue (iter, 1) );
			/*
			 * manage distances from testtypes that have different distance for each track
			 * they are expressed as: (eg for MTGUG: "1-7-19")
			 * if a '-' exists then distances are variable, else, distance is defined
			 */
			string distance = (string) model.GetValue (iter, 1);
			if(distance == Catalog.GetString("Not defined")) 
				selectedDistance = 0;
			else if(distance.Contains("-")) {
				selectedDistance = -1;
				selectedDistancesString = distance;
			} else 
				selectedDistance = Convert.ToDouble(distance);


			if( (string) model.GetValue (iter, 2) == Catalog.GetString("Unlimited") ) {
				selectedUnlimited = true;
			} 

			if( (string) model.GetValue (iter, 2) == Catalog.GetString("Laps") ) {
				selectedTracksLimited = true;
			}

			if( (string) model.GetValue (iter, 3) == "?" || (string) model.GetValue (iter, 3) == "-" ) {
				selectedLimitedValue = 0;
			} else {
				selectedLimitedValue = Convert.ToInt32( (string) model.GetValue (iter, 3) );
			}
		
			selectedDescription = (string) model.GetValue (iter, 4);

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
			
			//selectedDistance = Convert.ToDouble( (string) model.GetValue (iter, 1) );
			
			string distance = (string) model.GetValue (iter, 1);
			if(distance == Catalog.GetString("Not defined")) 
				selectedDistance = 0;
			else if(distance.Contains("-")) {
				selectedDistance = -1;
				selectedDistancesString = distance;
			} else 
				selectedDistance = Convert.ToDouble(distance);


			if( (string) model.GetValue (iter, 2) == Catalog.GetString("Unlimited") ) {
				selectedUnlimited = true;
			} 
			
			if( (string) model.GetValue (iter, 2) == Catalog.GetString("Laps") ) {
				selectedTracksLimited = true;
			}

			if( (string) model.GetValue (iter, 3) == "?" || (string) model.GetValue (iter, 3) == "-" ) {
				selectedLimitedValue = 0;
			} else {
				selectedLimitedValue = Convert.ToInt32( (string) model.GetValue (iter, 3) );
			}
			
			selectedDescription = (string) model.GetValue (iter, 4);
			
			//activate on_button_accept_clicked()
			button_accept.Activate();
		}
	}
	
	protected override void deleteTestLine() {
		SqliteRunIntervalType.Delete(selectedEventName);
		
		//delete from typesTranslated
		string row = Util.FindOnArray(':',0, -1, selectedEventName, typesTranslated);
		typesTranslated = Util.DeleteString(typesTranslated, row);
	}

	protected override string [] findTestTypesInSessions() {
		return SqliteRunInterval.SelectRunsSA (false, -1, -1, selectedEventName);
	}
	
	
	void on_button_cancel_clicked (object o, EventArgs args)
	{
		RunsIntervalMoreWindowBox.jumps_runs_more.Hide();
		RunsIntervalMoreWindowBox = null;
	}
	
	void on_jumps_runs_more_delete_event (object o, DeleteEventArgs args)
	{
		RunsIntervalMoreWindowBox.jumps_runs_more.Hide();
		RunsIntervalMoreWindowBox = null;
	}
	
	void on_button_accept_clicked (object o, EventArgs args)
	{
		RunsIntervalMoreWindowBox.jumps_runs_more.Hide();
	}
	
	//when a runInterval is done using runsIntervalMoreWindow, the accept doesn't destroy this instance, because 
	//later we need data from it.
	//This is used for destroying, then if a new runInterval type is added, it will be shown at first time clicking "more" button
	public void Destroy() {		
		RunsIntervalMoreWindowBox = null;
	}
	
	public double SelectedDistance {
		get { return selectedDistance; }
	}
	
	public string SelectedDistancesString {
		get { return selectedDistancesString; }
	}
	
	public bool SelectedTracksLimited {
		get { return selectedTracksLimited; }
	}
	
	public int SelectedLimitedValue { 
		get { return selectedLimitedValue; }
	}
	
	public bool SelectedUnlimited {
		get { return selectedUnlimited; }
	}
}
