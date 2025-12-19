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


public class EditFourPlatformsWindow : EditEventWindow
{
	static EditFourPlatformsWindow EditFourPlatformsWindowBox;

	//for inheritance
	protected EditFourPlatformsWindow () {
	}

	public EditFourPlatformsWindow (Gtk.Window parent)
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

	static public EditFourPlatformsWindow Show (Gtk.Window parent, Event myEvent)
	{
		if (EditFourPlatformsWindowBox == null) {
			EditFourPlatformsWindowBox = new EditFourPlatformsWindow (parent);
		}

		EditFourPlatformsWindowBox.colorize();
		EditFourPlatformsWindowBox.initializeValues();
		EditFourPlatformsWindowBox.fillDialog (myEvent);
		EditFourPlatformsWindowBox.edit_event.Show ();

		return EditFourPlatformsWindowBox;
	}
	
	protected override void initializeSpecific ()
	{
		typeOfTest = Constants.TestTypes.FOURPLATFORMS;
	}

	protected override void fillDialogSpecific (Event myEvent)
	{
		FourPlatforms fp = (FourPlatforms) myEvent;
		label_date_value.Text = fp.DateTimePublic;
	}

	protected override string [] findTypes (Event myEvent)
	{
		//TODO
		return new string []{};
	}
	
	protected override void updateSQL (int eventID, int personID, string description)
	{
		SqliteTests st = new SqliteFourPlatforms ();
		st.UpdateFromEdit (eventID, personID, -1, description);
	}

	protected override void on_button_cancel_clicked (object o, EventArgs args)
	{
		EditFourPlatformsWindowBox.edit_event.Hide();
		EditFourPlatformsWindowBox = null;
	}
	
	protected override void on_delete_event (object o, DeleteEventArgs args)
	{
		EditFourPlatformsWindowBox.edit_event.Hide();
		EditFourPlatformsWindowBox = null;
	}
	
	protected override void hideWindow() {
		EditFourPlatformsWindowBox.edit_event.Hide();
		EditFourPlatformsWindowBox = null;
	}
}

public partial class ChronoJumpWindow
{
	private void on_edit_selected_fourPlatforms_clicked (object o, EventArgs args)
	{
		//notebooks_change(2); see "notebooks_change sqlite problem"
		LogB.Information("Edit selected wilight");
		//1.- check that there's a line selected
		//2.- check that this line is a wilight and not a person (check also if it's not a individual RJ, the pass the parent RJ)
		int selectedID = treeViewResultsSession.EventSelectedID;
		if (selectedID < 0)
			return;

		//3.- obtain the data of the selected test
		FourPlatforms fp = SqliteFourPlatforms.SelectData (selectedID, false );
		eventOldPerson = fp.PersonID;

		//4.- edit this test
		editFourPlatformsWin = EditFourPlatformsWindow.Show (app1, fp);
		editFourPlatformsWin.Fake_button_finished.Clicked += new EventHandler (on_edit_selected_fourPlatforms_finished);
	}

	private void on_edit_selected_fourPlatforms_finished (object o, EventArgs args)
	{
		LogB.Information("edit selected fourPlatforms finished");
		FourPlatforms fourPlatforms = SqliteFourPlatforms.SelectData (treeViewResultsSession.EventSelectedID, false);

		//if person changed, fill treeview again, if not, only update it's line
		if (eventOldPerson == fourPlatforms.PersonID)
			treeViewResultsSession.Update (fourPlatforms);
		else
			pre_fillTreeView_resultsSession ();

		updateGraphFourPlatformsBars ();
	}
}
