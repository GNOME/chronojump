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
//using Glade;
//using System.Text; //StringBuilder
using Mono.Unix;

//--------------------------------------------------------
//---------------- EDIT WIDGET ---------------------------
//--------------------------------------------------------

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
	
//TODO:
//		eventBigTypeString = Catalog.GetString("race");
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
	
	protected override void initializeValues ()
	{
		typeOfTest = Constants.TestTypes.FORCESENSOR;
		showType = false; //TODO: in the future change this
		showRunStart = false;
		showTv = false;
		showTc = false;
		showFall = false;
		showDistance = false;
		distanceCanBeDecimal = true;
		showTime = false;
		showSpeed = false;
		showWeight = false;
		showLimited = false;
		showMistakes = false;
		showVideo = false;
		showDescription = true;
	}

	protected override string [] findTypes (Event myEvent)
	{
		//TODO
		return new string []{};
	}
	
	private void on_combo_eventType_changed (object o, EventArgs args)
	{
		//TODO:
	}

	protected override void updateEvent (int eventID, int personID, string description)
	{
		SqliteTests st = new SqliteRunEncoder ();
		st.Update (eventID,
				//UtilGtk.ComboGetActive(combo_eventType),
				personID);
		st.UpdateComments (eventID, description);
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
		if (selectedID <= 0)
			return;

		//3.- obtain the data of the selected runEncoder
		RunEncoder runEncoder = SqliteRunEncoder.SelectData (selectedID, false );
		eventOldPerson = runEncoder.PersonID;

		//4.- edit this test
		editRunEncoderWin = EditRunEncoderWindow.Show (app1, runEncoder);
		editRunEncoderWin.Button_accept.Clicked += new EventHandler (on_edit_selected_runEncoder_accepted);
	}
	private void on_edit_selected_runEncoder_accepted (object o, EventArgs args)
	{
		LogB.Information("edit selected runEncoder accepted");
		RunEncoder runEncoder = SqliteRunEncoder.SelectData (treeViewResultsSession.EventSelectedID, false);

		//if person changed, fill treeview again, if not, only update it's line
		if (eventOldPerson == runEncoder.PersonID)
		{
			runEncoder.ExerciseName = SqliteTests.SelectExerciseNameInOtherTable (false, runEncoder.ExerciseID, Constants.RunEncoderExerciseTable);
			treeViewResultsSession.Update (runEncoder);
		}  else
			pre_fillTreeView_resultsSession ();

		//updateGraphRunEncoderBars ();
	}

}
