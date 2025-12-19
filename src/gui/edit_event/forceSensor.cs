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

public class EditForceSensorWindow : EditEventWindow
{
	static EditForceSensorWindow EditForceSensorWindowBox;
	Constants.Modes mode;

	//for inheritance
	protected EditForceSensorWindow () {
	}

	public EditForceSensorWindow (Gtk.Window parent)
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

	static public EditForceSensorWindow Show (Gtk.Window parent, Event myEvent, Constants.Modes mode)
	{
		if (EditForceSensorWindowBox == null) {
			EditForceSensorWindowBox = new EditForceSensorWindow (parent);
		}

		EditForceSensorWindowBox.mode = mode;
		EditForceSensorWindowBox.colorize();
		EditForceSensorWindowBox.initializeValues();
		EditForceSensorWindowBox.fillDialog (myEvent);
		EditForceSensorWindowBox.edit_event.Show ();

		return EditForceSensorWindowBox;
	}
	
	protected override void initializeSpecific ()
	{
		typeOfTest = Constants.TestTypes.FORCESENSOR;

		showType = true;
		showDescription = true;
	}

	protected override void fillDialogSpecific (Event myEvent)
	{
		ForceSensor fs = (ForceSensor) myEvent;

		image_forceSensor_capture_standard.Pixbuf = Chronojump.MyPixbuf.Get(null, Util.GetImagePath(false) + "signal_standard.png");
		image_forceSensor_capture_absolute.Pixbuf = Chronojump.MyPixbuf.Get(null, Util.GetImagePath(false) + "signal_absolute.png");
		image_forceSensor_capture_inverted.Pixbuf = Chronojump.MyPixbuf.Get(null, Util.GetImagePath(false) + "signal_inverted.png");
		image_exercise_filter.Pixbuf = Chronojump.MyPixbuf.Get(null, Util.GetImagePath(false) + "filter_off.png");

		box_exercise_filter.Visible = true;
		createLateralityIcons ();

		switch (fs.CaptureOption)
		{
			case ForceSensor.CaptureOptions.NORMAL:
				radio_forceSensor_capture_standard.Active = true;
				break;
			case ForceSensor.CaptureOptions.ABS:
				radio_forceSensor_capture_absolute.Active = true;
				break;
			case ForceSensor.CaptureOptions.INVERTED:
				radio_forceSensor_capture_inverted.Active = true;
				break;
		}

		switch (fs.Laterality)
		{
			case "Both":
				radio_laterality_both.Active = true;
				break;
			case "Left":
				radio_laterality_left.Active = true;
				break;
			case "Right":
				radio_laterality_right.Active = true;
				break;
		}

		label_forceSensor_capture.Visible = true;
		box_forceSensor_capture.Visible = true;
		label_laterality.Visible = true;
		box_laterality.Visible = true;

		label_date_value.Text = fs.DateTimePublic;
	}

	protected override void on_exercise_filter_changed (object o, EventArgs args)
	{
		// note if this is used on more modes, then we need to take care about: combo_exercise_has_signal
		// in order to deactivate this signal first, and reactivate later

		string exName = UtilGtk.ComboGetActive (combo_eventType);
		UtilGtk.ComboDelAll (combo_eventType);
		UtilGtk.ComboUpdate (combo_eventType, findTypesDo (), "");
		combo_eventType.Active = UtilGtk.ComboMakeActive (combo_eventType, exName);
		showExerciseFilterIfNeeded ();
	}

	private void showExerciseFilterIfNeeded ()
	{
		if (entry_exercise_filter.Text.ToString () == "")
			image_exercise_filter.Pixbuf = Chronojump.MyPixbuf.Get(null, Util.GetImagePath(false) + "filter_off.png");
		else
			image_exercise_filter.Pixbuf = Chronojump.MyPixbuf.Get(null, Util.GetImagePath(false) + "filter_on.png");
	}

	protected override string [] findTypes (Event myEvent)
	{
		return findTypesDo (); // no need myEvent
	}
	private string [] findTypesDo ()
	{
		List<ForceSensorExercise> fsex_l;

		LogB.Information ("current_mode: " + mode.ToString ());
		if (mode == Constants.Modes.FORCESENSORISOMETRIC)
			fsex_l = SqliteForceSensorExercise.Select (false, -1, 0,
					true, 		// onlyNames (but returns a full ForceSensorExercise)
					entry_exercise_filter.Text.ToString (),
					Sqlite.Orders_by.NAME_ASC);
		else // (mode == Constants.Modes.FORCESENSORELASTIC)
			fsex_l = SqliteForceSensorExercise.Select (false, -1, 1,
					true, 		// onlyNames (but returns a full ForceSensorExercise)
					entry_exercise_filter.Text.ToString (),
					Sqlite.Orders_by.NAME_ASC);

		// get the exercise names and convert to string []
		return ForceSensorExercise.ListToString (fsex_l);
	}

	protected override void updateSQL (int eventID, int personID, string description)
	{
		// get object before update
		ForceSensor fs = SqliteForceSensor.SelectData (eventID, false, false);
		// set person
		fs.PersonID = personID;

		// set exercise
		int fsExIdNew = Sqlite.ExistsAndGetUniqueID (false,
				Constants.ForceSensorExerciseTable,
				UtilGtk.ComboGetActive(combo_eventType));

		fs.ExerciseID = fsExIdNew;

		// set captureOption
		if (radio_forceSensor_capture_standard.Active)
			fs.CaptureOption = ForceSensor.CaptureOptions.NORMAL;
		else if (radio_forceSensor_capture_absolute.Active)
			fs.CaptureOption = ForceSensor.CaptureOptions.ABS;
		else if (radio_forceSensor_capture_inverted.Active)
			fs.CaptureOption = ForceSensor.CaptureOptions.INVERTED;

		// set laterality
		if (radio_laterality_both.Active)
			fs.Laterality = "Both";
		else if (radio_laterality_left.Active)
			fs.Laterality = "Left";
		else if (radio_laterality_right.Active)
			fs.Laterality = "Right";

		// set description
		fs.Description = description;

		// update
		fs.UpdateSQL (false);
	}

	protected override void on_button_cancel_clicked (object o, EventArgs args)
	{
		EditForceSensorWindowBox.edit_event.Hide();
		EditForceSensorWindowBox = null;
	}
	
	protected override void on_delete_event (object o, DeleteEventArgs args)
	{
		EditForceSensorWindowBox.edit_event.Hide();
		EditForceSensorWindowBox = null;
	}
	
	protected override void hideWindow() {
		EditForceSensorWindowBox.edit_event.Hide();
		EditForceSensorWindowBox = null;
	}
}

public partial class ChronoJumpWindow
{
	private void on_edit_selected_forceSensor_clicked (object o, EventArgs args)
	{
		//notebooks_change(2); see "notebooks_change sqlite problem"
		LogB.Information("Edit selected forceSensor");
		//1.- check that there's a line selected
		//2.- check that this line is a forceSensor and not a person (check also if it's not a individual RJ, the pass the parent RJ)
		int selectedID = treeViewResultsSession.EventSelectedID;
		if (selectedID < 0)
			return;

		//3.- obtain the data of the selected forceSensor
		ForceSensor forceSensor = SqliteForceSensor.SelectData (selectedID, true, false);
		eventOldPerson = forceSensor.PersonID;

		//4.- edit this test
		editForceSensorWin = EditForceSensorWindow.Show (app1, forceSensor, current_mode);
		editForceSensorWin.Fake_button_finished.Clicked += new EventHandler (on_edit_selected_forceSensor_finished);
	}

	private void on_edit_selected_forceSensor_finished (object o, EventArgs args)
	{
		LogB.Information("edit selected forceSensor finished");

		ForceSensor forceSensor = SqliteForceSensor.SelectData (treeViewResultsSession.EventSelectedID, true, false);

		LogB.Information ("currentForceSensor:\n" + currentForceSensor.ToString ());
		LogB.Information ("forceSensor:\n" + forceSensor.ToString ());
		// need to recalculate if changed any variable related to maxForceRaw, maxAvgForce1s
		if (eventOldPerson != forceSensor.PersonID || 	// useful if exercise has projected mass
				forceSensor.ExerciseID != currentForceSensor.ExerciseID ||
				forceSensor.CaptureOption != currentForceSensor.CaptureOption)
		{
			currentForceSensor = forceSensor;

			// this will also: pre_fillTreeView_resultsSession & selectResultsSessionId
			force_sensor_recalculate (
					forceSensor.CaptureOption,
					forceSensor.Laterality);
			return;
		}

		treeViewResultsSession.Update (forceSensor);
	}
}
