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
 * Copyright (C) 2025   Xavier de Blas <xaviblas@gmail.com>
 */

using System;
using Gtk;


//note this file is almost the same than EditForceSensorWindow and most the other sensors, so can be refactorized
public class EditRunEncoderWindow : EditEventWindow
{
	static EditRunEncoderWindow EditRunEncoderWindowBox;

	//for inheritance
	protected EditRunEncoderWindow () {
	}

	public EditRunEncoderWindow (Gtk.Window parent)
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
	}

	static public EditRunEncoderWindow Show (Gtk.Window parent, Event myEvent)
	{
		if (EditRunEncoderWindowBox == null) {
			EditRunEncoderWindowBox = new EditRunEncoderWindow (parent);
		}

		EditRunEncoderWindowBox.colorize();
		EditRunEncoderWindowBox.initializeValues();
		EditRunEncoderWindowBox.fillDialog (myEvent);
		EditRunEncoderWindowBox.edit_event.Show ();

		return EditRunEncoderWindowBox;
	}
	
	protected override void initializeSpecific ()
	{
		typeOfTest = Constants.TestTypes.RACEANALYZER;
		showType = true;
		showDescription = true;
	}

	protected override void fillDialogSpecific (Event myEvent)
	{
		// 1. get the object
		RunEncoder re = (RunEncoder) myEvent;

		// 2. set widgets visibility
		label_race_analyzer_distance.Visible = true;
		spin_race_analyzer_distance.Visible = true;
		label_race_analyzer_distance_units.Visible = true;
		label_race_analyzer_angle.Visible = true;
		spin_race_analyzer_angle.Visible = true;
		label_race_analyzer_angle_units.Visible = true;
		label_race_analyzer_temperature.Visible = true;
		spin_race_analyzer_temperature.Visible = true;
		label_race_analyzer_temperature_units.Visible = true;

		// 2. set widgets value
		spin_race_analyzer_distance.Value = Convert.ToInt32 (re.Distance);
		spin_race_analyzer_angle.Value = Convert.ToInt32 (re.Angle);
		spin_race_analyzer_temperature.Value = Convert.ToInt32 (re.Temperature);

		label_date_value.Text = re.DateTimePublic;
	}

	protected override string [] findTypes (Event myEvent)
	{
		return RunEncoderExercise.ListToString (SqliteRunEncoderExercise.Select (false, -1));
	}
	
	protected override void updateSQL (int eventID, int personID, string description)
	{
		// get object before update
		RunEncoder re = SqliteRunEncoder.SelectData (eventID, false, false);
		// set person
		re.PersonID = personID;

		// set exercise
		int reExIdNew = Sqlite.ExistsAndGetUniqueID (false,
				Constants.RunEncoderExerciseTable,
				UtilGtk.ComboGetActive (combo_eventType));

		re.ExerciseID = reExIdNew;

		// set distance, angle, temperature
		re.Distance = Convert.ToInt32 (spin_race_analyzer_distance.Value);
		re.Angle = Convert.ToInt32 (spin_race_analyzer_angle.Value);
		re.Temperature = Convert.ToInt32 (spin_race_analyzer_temperature.Value);

		// set description
		re.Description = description;

		// update
		re.UpdateSQL (false);
	}

	protected override void on_button_cancel_clicked (object o, EventArgs args)
	{
		EditRunEncoderWindowBox.edit_event.Hide();
		EditRunEncoderWindowBox = null;
	}
	
	protected override void on_delete_event (object o, DeleteEventArgs args)
	{
		EditRunEncoderWindowBox.edit_event.Hide();
		EditRunEncoderWindowBox = null;
	}
	
	protected override void hideWindow() {
		EditRunEncoderWindowBox.edit_event.Hide();
		EditRunEncoderWindowBox = null;
	}
}

public partial class ChronoJumpWindow
{
	private void on_edit_selected_runEncoder_clicked (object o, EventArgs args)
	{
		//notebooks_change(2); see "notebooks_change sqlite problem"
		LogB.Information("Edit selected runEncoder");
		//1.- check that there's a line selected
		//2.- check that this line is a runEncoder and not a person (check also if it's not a individual RJ, the pass the parent RJ)
		int selectedID = treeViewResultsSession.EventSelectedID;
		if (selectedID < 0)
			return;

		//3.- obtain the data of the selected runEncoder
		RunEncoder runEncoder = SqliteRunEncoder.SelectData (selectedID, true, false);
		eventOldPerson = runEncoder.PersonID;

		//4.- edit this test
		editRunEncoderWin = EditRunEncoderWindow.Show (app1, runEncoder);
		editRunEncoderWin.Fake_button_finished.Clicked += new EventHandler (on_edit_selected_runEncoder_finished);
	}
	private void on_edit_selected_runEncoder_finished (object o, EventArgs args)
	{
		LogB.Information("edit selected runEncoder finished");
		RunEncoder runEncoder = SqliteRunEncoder.SelectData (treeViewResultsSession.EventSelectedID, true, false);

		if (
				runEncoder.PersonID != currentRunEncoder.PersonID ||
				runEncoder.ExerciseID != currentRunEncoder.ExerciseID ||
				runEncoder.Distance != currentRunEncoder.Distance ||
				runEncoder.Angle != currentRunEncoder.Angle ||
				runEncoder.Temperature != currentRunEncoder.Temperature)
		{
			currentRunEncoder = runEncoder;

			// this will also: pre_fillTreeView_resultsSession & selectResultsSessionId
			run_encoder_recalculate ();
			return;
		}

		/*
		//if person changed, fill treeview again, if not, only update it's line
		if (eventOldPerson == runEncoder.PersonID)
		{
			runEncoder.ExerciseName = SqliteTests.SelectExerciseNameInOtherTable (false, runEncoder.ExerciseID, Constants.RunEncoderExerciseTable);
			treeViewResultsSession.Update (runEncoder);
		}  else
			pre_fillTreeView_resultsSession ();
		*/
		treeViewResultsSession.Update (runEncoder);

		//updateGraphRunEncoderBars ();
	}

}
