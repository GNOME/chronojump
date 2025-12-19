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
 * Copyright (C) 2004-2025   Xavier de Blas <xaviblas@gmail.com>
 */

using System;
using Gtk;
using System.Text; //StringBuilder
using System.Collections.Generic; //List<T>
using Mono.Unix;


public class EditRunWindow : EditEventWindow
{
	static EditRunWindow EditRunWindowBox;
	private int mistakes;

	//for inheritance
	protected EditRunWindow () {
	}

	public EditRunWindow (Gtk.Window parent)
	{
		/*
		Glade.XML gladeXML;
		gladeXML = Glade.XML.FromAssembly (Util.GetGladePath() + "edit_event.glade", "edit_event", null);
		gladeXML.Autoconnect(this);
		*/
		Gtk.Builder builder = new Gtk.Builder (null, Util.GetGladePath () + "edit_event.glade", null);
		connectWidgetsEditEvent (builder);
		builder.Autoconnect (this);

		this.parent = parent;
		
		//put an icon to window
		UtilGtk.IconWindow(edit_event);
	
		eventBigTypeString = Catalog.GetString("race");
	}

	static public EditRunWindow Show (Gtk.Window parent, Event myEvent, int pDN, bool metersSecondsPreferred)
	{
		if (EditRunWindowBox == null) {
			EditRunWindowBox = new EditRunWindow (parent);
		}

		EditRunWindowBox.metersSecondsPreferred = metersSecondsPreferred;
		EditRunWindowBox.pDN = pDN;

		EditRunWindowBox.colorize();

		EditRunWindowBox.initializeValues();
		
		EditRunWindowBox.fillDialog (myEvent);
		
		if(myEvent.Type == "Margaria")
			EditRunWindowBox.entry_description.Sensitive = false;
		if(myEvent.Type == "Gesell-DBT") {
			EditRunWindowBox.showMistakes = true;
			EditRunWindowBox.combo_eventType.Sensitive=false;
			EditRunWindowBox.entry_description.Sensitive = false;
			EditRunWindowBox.mistakes = Convert.ToInt32(myEvent.Description);
			EditRunWindowBox.spin_mistakes.Value = Convert.ToInt32(myEvent.Description);
		}

		EditRunWindowBox.edit_event.Show ();

		return EditRunWindowBox;
	}
	
	protected override void initializeSpecific ()
	{
		typeOfTest = Constants.TestTypes.RUN;
		showType = true;
		showRunStart = true;

		showRunDistance = true;
		distanceCanBeDecimal = true;
		showTime = true;
		showSpeed = true;
		showWeight = false;
		showDescription = true;
		
		if(metersSecondsPreferred)
			label_speed_units.Text = "m/s";
		else
			label_speed_units.Text = "km/h";

		combo_exercise_has_signal = true;
	}

	protected override string [] findTypes(Event myEvent) {
		string [] myTypes = SqliteRunType.SelectRunTypes("", true); //don't show allRunsName row, only select name
		return myTypes;
	}
	
	protected override void fillRunStart(Event myEvent) {
		Run myRun = (Run) myEvent;
		if(myRun.InitialSpeed)
			label_run_start_value.Text = Constants.RunStartInitialSpeedYesStr();
		else
			label_run_start_value.Text = Constants.RunStartInitialSpeedNoStr();
	}

	protected override void fillRunDistance (Event myEvent)
	{
		Run myRun = (Run) myEvent;
		entryDistance = myRun.Distance.ToString();
		entry_distance_value.Text = Util.TrimDecimals(entryDistance, pDN);
		//if the eventtype has not a predefined distance, make the widget sensitive
		RunType myRunType = new RunType (myRun.Type);
		if(myRunType.Distance == 0) {
			entry_distance_value.Sensitive = true;
		} else {
			entry_distance_value.Sensitive = false;
		}
	}
	
	protected override void fillTime(Event myEvent) {
		Run myRun = (Run) myEvent;
		entryTime = myRun.Time.ToString();
		
		//show all the decimals for not triming there in edit window using
		//(and having different values in formulae like GetHeightInCm ...)
		//entry_time_value.Text = Util.TrimDecimals(entryTime, pDN);
		entry_time_value.Text = entryTime;
	}
	
	protected override void fillSpeed(Event myEvent) {
		Run myRun = (Run) myEvent;
		label_speed_value.Text = Util.TrimDecimals(myRun.Speed.ToString(), pDN);
		
		if(metersSecondsPreferred)
			label_speed_units.Text = "m/s";
		else
			label_speed_units.Text = "km/h";
	}

	protected override void on_combo_eventType_changed (object o, EventArgs args)
	{
		//if the distance of the new runType is fixed, put this distance
		//if not conserve the old
		RunType myRunType = new RunType (UtilGtk.ComboGetActive(combo_eventType));
		if(myRunType.Distance != 0) {
			entryDistance = myRunType.Distance.ToString();
			entry_distance_value.Text = "";
			entry_distance_value.Text = Util.TrimDecimals(entryDistance, pDN);
			entry_distance_value.Sensitive = false;
		} else {
			entry_distance_value.Sensitive = true;
		}
		
		label_speed_value.Text = Util.TrimDecimals(
				Util.GetSpeed (entryDistance, entryTime, metersSecondsPreferred) , pDN);
	}
	
	protected override void on_spin_mistakes_changed (object o, EventArgs args) {
		if(Util.IsNumber(spin_mistakes.Value.ToString(), true) && entry_time_value.Text.ToString().Length > 0) {
			double timeWithoutMistakes = Convert.ToDouble(entry_time_value.Text.ToString()) - 2 * mistakes;
			entry_time_value.Text = (timeWithoutMistakes + 2 * spin_mistakes.Value).ToString();
			entryTime = entry_time_value.Text.ToString();
			
			mistakes = Convert.ToInt32(spin_mistakes.Value);
			
			entry_description.Text = mistakes.ToString();
		}
	}
		

	protected override void updateSQL(int eventID, int personID, string description)
	{
		SqliteRun.Update (eventID, UtilGtk.ComboGetActive(combo_eventType),
				Convert.ToDouble (entry_distance_value.Text),
				entryTime, personID, description);
	}

	protected override void on_button_cancel_clicked (object o, EventArgs args)
	{
		EditRunWindowBox.edit_event.Hide();
		EditRunWindowBox = null;
	}
	
	protected override void on_delete_event (object o, DeleteEventArgs args)
	{
		EditRunWindowBox.edit_event.Hide();
		EditRunWindowBox = null;
	}
	
	protected override void hideWindow() {
		EditRunWindowBox.edit_event.Hide();
		EditRunWindowBox = null;
	}
}

public partial class ChronoJumpWindow
{
	private void on_edit_selected_run_clicked (object o, EventArgs args)
	{
		//notebooks_change(2); see "notebooks_change sqlite problem"
		LogB.Information("Edit selected run (simple)");
		//1.- check that there's a line selected
		//2.- check that this line is a jump and not a person (check also if it's not a individual RJ, the pass the parent RJ)
		int selectedID = treeViewResultsSession.EventSelectedID;
		if (selectedID < 0)
			return;

		//3.- obtain the data of the selected run
		Run myRun = SqliteRun.SelectRunData (selectedID, false);
		myRun.MetersSecondsPreferred = preferences.metersSecondsPreferred;
		eventOldPerson = myRun.PersonID;

		//4.- edit this run
		editRunWin = EditRunWindow.Show(app1, myRun, preferences.digitsNumber, preferences.metersSecondsPreferred);
		editRunWin.Fake_button_finished.Clicked += new EventHandler (on_edit_selected_run_finished);
	}
	
	private void on_edit_selected_run_finished (object o, EventArgs args)
	{
		LogB.Information("edit selected run finished");

		Run myRun = SqliteRun.SelectRunData (treeViewResultsSession.EventSelectedID, false );
		
		//if person changed, fill treeview again, if not, only update it's line
		if (eventOldPerson == myRun.PersonID)
			treeViewResultsSession.Update (myRun);
		else
			pre_fillTreeView_resultsSession ();

		updateGraphRunsSimple();

		if(createdStatsWin) 
			stats_win_fillTreeView_stats(false, false);
	}
}
