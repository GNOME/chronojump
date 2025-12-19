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
using Mono.Unix;


public class EditJumpRjWindow : EditJumpWindow
{
	static EditJumpRjWindow EditJumpRjWindowBox;

	EditJumpRjWindow (Gtk.Window parent)
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
	
		eventBigTypeString = Catalog.GetString("reactive jump");
	}

	static new public EditJumpRjWindow Show (Gtk.Window parent, Event myEvent, bool weightPercentPreferred, int pDN)
	{
		if (EditJumpRjWindowBox == null) {
			EditJumpRjWindowBox = new EditJumpRjWindow (parent);
		}

		EditJumpRjWindowBox.weightPercentPreferred = weightPercentPreferred;
		EditJumpRjWindowBox.personWeight = SqlitePersonSession.SelectAttribute(
				false, myEvent.PersonID, myEvent.SessionID, Constants.Weight); 

		EditJumpRjWindowBox.pDN = pDN;
		
		EditJumpRjWindowBox.sessionID = myEvent.SessionID;

		EditJumpRjWindowBox.colorize();

		EditJumpRjWindowBox.initializeValues();

		EditJumpRjWindowBox.fillDialog (myEvent);
		
		EditJumpRjWindowBox.edit_event.Show ();

		return EditJumpRjWindowBox;
	}
	
	protected override void initializeSpecific ()
	{
		typeOfTest = Constants.TestTypes.JUMP_RJ;
		showType = true;

		//jump
		showJumpTv = false;
		showJumpTc = false;
		showJumpFall = true;

		showWeight = true;
		showLimited = true;
		showDescription = true;
		
		if(weightPercentPreferred)
			label_weight_units.Text = "%";
		else
			label_weight_units.Text = "kg";
	}

	protected override string [] findTypes(Event myEvent) {
		//type cannot change on jumpRj
		combo_eventType.Sensitive=false;

		string [] myTypes;
		myTypes = SqliteJumpType.SelectJumpRjTypes("", true); //don't show allJumpsName row, only select name
		return myTypes;
	}

	protected override void fillWeight(Event myEvent) {
		JumpRj myJump = (JumpRj) myEvent;
		if(myJump.TypeHasWeight) {
			if(weightPercentPreferred)
				entryWeight = myJump.WeightPercent.ToString ();
			else
				entryWeight = Util.WeightFromPercentToKg (myJump.WeightPercent, personWeight).ToString ();

			entry_weight_value.Text = entryWeight;
			entry_weight_value.Sensitive = true;
		} else {
			entry_weight_value.Sensitive = false;
		}
	}

	protected override void fillFall(Event myEvent) {
		JumpRj myJump = (JumpRj) myEvent;
		entryJumpFall = myJump.Fall.ToString();
		entry_jump_fall_value.Text = entryJumpFall;
	}

	protected override void fillLimited(Event myEvent) {
		JumpRj myJumpRj = (JumpRj) myEvent;
		label_limited_value.Text = Util.GetLimitedRounded(myJumpRj.Limited, pDN);
	}


	protected override void on_button_cancel_clicked (object o, EventArgs args)
	{
		EditJumpRjWindowBox.edit_event.Hide();
		EditJumpRjWindowBox = null;
	}
	
	protected override void on_delete_event (object o, DeleteEventArgs args)
	{
		EditJumpRjWindowBox.edit_event.Hide();
		EditJumpRjWindowBox = null;
	}
	
	protected override void hideWindow() {
		EditJumpRjWindowBox.edit_event.Hide();
		EditJumpRjWindowBox = null;
	}
	
	protected override void updateSQL(int eventID, int personID, string description) {
		//only for jumps
		double jumpPercentWeightForNewPerson = updateWeight(personID, sessionID);
		
		SqliteJumpRj.Update(eventID, personID, entryJumpFall, jumpPercentWeightForNewPerson, description);
	}
}


public partial class ChronoJumpWindow
{
	private void on_edit_selected_jump_rj_clicked (object o, EventArgs args)
	{
		//notebooks_change(1); see "notebooks_change sqlite problem"
		LogB.Information("Edit selected jump (RJ)");
		//1.- check that there's a line selected
		//2.- check that this line is a jump and not a person (check also if it's not a individual RJ, the pass the parent RJ)
		int selectedID = treeViewResultsSession.EventSelectedID;
		if (selectedID < 0)
			return;

		//3.- obtain the data of the selected jump
		JumpRj myJump = SqliteJumpRj.SelectJumpData ("jumpRj", selectedID, false, false );
		eventOldPerson = myJump.PersonID;

		//4.- edit this jump
		editJumpRjWin = EditJumpRjWindow.Show(app1, myJump, preferences.weightStatsPercent, preferences.digitsNumber);
		editJumpRjWin.Fake_button_finished.Clicked += new EventHandler (on_edit_selected_jump_rj_finished);
	}
	
	private void on_edit_selected_jump_rj_finished (object o, EventArgs args)
	{
		LogB.Information("edit selected jump RJ finished");
	
		JumpRj myJump = SqliteJumpRj.SelectJumpData ("jumpRj", treeViewResultsSession.EventSelectedID, false, false );
		
		//if person changed, fill treeview again, if not, only update it's line
		if(eventOldPerson == myJump.PersonID)
		{
			double personWeight = SqlitePersonSession.SelectAttribute (
					false, myJump.PersonID, currentSession.UniqueID, Constants.Weight);
			treeViewResultsSession.PersonWeight = personWeight;
			treeViewResultsSession.Update(myJump);
		} else
			pre_fillTreeView_resultsSession ();

		updateGraphJumpsReactive();

		if(createdStatsWin) 
			stats_win_fillTreeView_stats(false, false);
	}
}
