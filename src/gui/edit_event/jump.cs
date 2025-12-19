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


public class EditJumpWindow : EditEventWindow
{
	private Gtk.Frame frame_jumps_single_leg;
	private Gtk.Box box_jumps_single_leg;
	private Gtk.RadioButton jumps_radiobutton_single_leg_mode_vertical;
	private Gtk.RadioButton jumps_radiobutton_single_leg_mode_horizontal;
	private Gtk.RadioButton jumps_radiobutton_single_leg_mode_lateral;
	private Gtk.RadioButton jumps_radiobutton_single_leg_dominance_this_limb;
	private Gtk.RadioButton jumps_radiobutton_single_leg_dominance_opposite;
	private Gtk.RadioButton jumps_radiobutton_single_leg_dominance_unknown;
	private Gtk.RadioButton jumps_radiobutton_single_leg_fall_this_limb;
	private Gtk.RadioButton jumps_radiobutton_single_leg_fall_opposite;
	private Gtk.RadioButton jumps_radiobutton_single_leg_fall_both;
	private Gtk.SpinButton jumps_spinbutton_single_leg_distance;
	private Gtk.SpinButton jumps_spinbutton_single_leg_jump_angle;

	static EditJumpWindow EditJumpWindowBox;
	protected double personWeight;
	protected int sessionID; //for know weight specific to this session

	//for inheritance
	protected EditJumpWindow () {
	}

	public EditJumpWindow (Gtk.Window parent)
	{
		/*
		Glade.XML gladeXML;
		gladeXML = Glade.XML.FromAssembly (Util.GetGladePath() + "edit_event.glade", "edit_event", null);
		gladeXML.Autoconnect(this);
		*/
		Gtk.Builder builder = new Gtk.Builder (null, Util.GetGladePath () + "edit_event.glade", null);
		connectWidgetsEditEvent (builder);
		connectWidgetsEditJump (builder);
		builder.Autoconnect (this);

		this.parent = parent;

		//put an icon to window
		UtilGtk.IconWindow(edit_event);
	
		eventBigTypeString = Catalog.GetString("jump");
	}

	static public EditJumpWindow Show (Gtk.Window parent, Event myEvent, bool weightPercentPreferred, int pDN)
	{
		if (EditJumpWindowBox == null) {
			EditJumpWindowBox = new EditJumpWindow (parent);
		}	

		EditJumpWindowBox.weightPercentPreferred = weightPercentPreferred;
		EditJumpWindowBox.personWeight = SqlitePersonSession.SelectAttribute(
				false,
				Convert.ToInt32(myEvent.PersonID),
				Convert.ToInt32(myEvent.SessionID),
				Constants.Weight); 

		EditJumpWindowBox.pDN = pDN;
		
		EditJumpWindowBox.sessionID = myEvent.SessionID;

		EditJumpWindowBox.colorize();
		UtilGtk.WidgetColor (EditJumpWindowBox.box_jumps_single_leg, Config.ColorBackgroundShifted);
		UtilGtk.ContrastLabelsBox (Config.ColorBackgroundShiftedIsDark, EditJumpWindowBox.box_jumps_single_leg);

		EditJumpWindowBox.initializeValues();

		EditJumpWindowBox.fillDialog (myEvent);
		
		if(myEvent.Type == "slCMJleft" || myEvent.Type == "slCMJright")
			EditJumpWindowBox.fillSingleLeg (myEvent.Description);
		
		EditJumpWindowBox.edit_event.Show ();

		return EditJumpWindowBox;
	}
	
	protected override void initializeSpecific ()
	{
		typeOfTest = Constants.TestTypes.JUMP;
		showType = true;

		// jumps
		showJumpTv = true;
		showJumpTc = true;
		showJumpFall = true;

		showWeight = true;
		
		if(weightPercentPreferred)
			label_weight_units.Text = "%";
		else
			label_weight_units.Text = "kg";

		LogB.Information(string.Format("-------------{0}", personWeight));

		combo_exercise_has_signal = true;
	}

	protected override string [] findTypes(Event myEvent) {
		Jump myJump = (Jump) myEvent;
		string [] myTypes;
		if (myJump.TypeHasFall) {
			myTypes = SqliteJumpType.SelectJumpTypes(false, "", "TC", true); //don't show allJumpsName row, TC jumps, only select name
		} else {
			myTypes = SqliteJumpType.SelectJumpTypes(false, "", "nonTC", true); //don't show allJumpsName row, nonTC jumps, only select name
		}
		return myTypes;
	}

	protected override void fillTv(Event myEvent) {
		Jump myJump = (Jump) myEvent;
		entryJumpTv = myJump.Tv.ToString();

		//show all the decimals for not triming there in edit window using
		//(and having different values in formulae like GetHeightInCm ...)
		//entry_jump_tv_value.Text = Util.TrimDecimals(entryJumpTv, pDN);
		entry_jump_tv_value.Text = entryJumpTv;
	
		//hide tv if it's only a takeoff	
		if(myEvent.Type == Constants.TakeOffName || myEvent.Type == Constants.TakeOffWeightName) 
			entry_jump_tv_value.Sensitive = false;
	}

	protected override void fillTc (Event myEvent) {
		//on normal jumps fills Tc and Fall
		Jump myJump = (Jump) myEvent;

		if (myJump.TypeHasFall) {
			entryJumpTc = myJump.Tc.ToString();
			
			//show all the decimals for not triming there in edit window using
			//(and having different values in formulae like GetHeightInCm ...)
			//entry_jump_tc_value.Text = Util.TrimDecimals(entryJumpTc, pDN);
			entry_jump_tc_value.Text = entryJumpTc;
			
			entryJumpFall = myJump.Fall.ToString();
			entry_jump_fall_value.Text = entryJumpFall;
			entry_jump_tc_value.Sensitive = true;
			entry_jump_fall_value.Sensitive = true;
		} else {
			entry_jump_tc_value.Sensitive = false;
			entry_jump_fall_value.Sensitive = false;
		}
	}

	protected override void fillWeight(Event myEvent) {
		Jump myJump = (Jump) myEvent;
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

	/*
	protected override void fillAngle(Event myEvent) {
		Jump myJump = (Jump) myEvent;
		
		//default values are -1.0 or -1 (old int)
		if(myJump.Angle < 0) { 
			entryAngle = "-1,0";
			entry_angle_value.Text = "-";
		} else {
			entryAngle = myJump.Angle.ToString();
			entry_angle_value.Text = entryAngle;
		}
	}
	*/

	//this disallows loops on radio actions	
	private bool toggleRaisesSignal = true;

	private bool slCMJDescriptionIsValid(string description) {
		string [] d = description.Split(new char[] {' '});
		if(d.Length != 5)
			return false;
		if(! Util.IsNumber(d[4], false))
			return false;
		if(d[0] != "Vertical" && d[0] != "Horizontal" && d[0] != "Lateral")
			return false;
		if(d[1] != "This" && d[1] != "Opposite" && d[1] != "Unknown")
			return false;
		if(d[2] != "This" && d[2] != "Opposite" && d[2] != "Both")
			return false;

		return true;
	}
	private string slCMJDescriptionDefault() {
		string descDefault = "Vertical Unknown Both 0 90";
		entry_description.Text = descDefault;
		return descDefault;
	}

	private void fillSingleLeg(string description) {
		frame_jumps_single_leg.Show();
		entry_description.Sensitive = false;
		
		if(! slCMJDescriptionIsValid(description))
			description = slCMJDescriptionDefault();

		string [] d = description.Split(new char[] {' '});
			
		toggleRaisesSignal = false;
		
		switch(d[0]) {
			case "Vertical":
				jumps_radiobutton_single_leg_mode_vertical.Active = true;
				jumps_spinbutton_single_leg_distance.Sensitive = false;
				jumps_spinbutton_single_leg_distance.Value = 0;
				jumps_spinbutton_single_leg_jump_angle.Value = 90;
				break;
			case "Horizontal":
				jumps_radiobutton_single_leg_mode_horizontal.Active = true;
				jumps_spinbutton_single_leg_distance.Sensitive = true;
				jumps_spinbutton_single_leg_distance.Value = Convert.ToInt32(d[3]);
				jumps_spinbutton_single_leg_jump_angle.Value = Convert.ToInt32(d[4]);
				break;
			case "Lateral":
				jumps_radiobutton_single_leg_mode_lateral.Active = true;
				jumps_spinbutton_single_leg_distance.Sensitive = true;
				jumps_spinbutton_single_leg_distance.Value = Convert.ToInt32(d[3]);
				jumps_spinbutton_single_leg_jump_angle.Value = Convert.ToInt32(d[4]);
				break;
		}
		switch(d[1]) {
			case "This": jumps_radiobutton_single_leg_dominance_this_limb.Active = true; break;
			case "Opposite": jumps_radiobutton_single_leg_dominance_opposite.Active = true; break;
			case "Unknown": jumps_radiobutton_single_leg_dominance_unknown.Active = true; break;
		}
		switch(d[2]) {
			case "This": jumps_radiobutton_single_leg_fall_this_limb.Active = true; break;
			case "Opposite": jumps_radiobutton_single_leg_fall_opposite.Active = true; break;
			case "Both": jumps_radiobutton_single_leg_fall_both.Active = true; break;
		}

		toggleRaisesSignal = true;
	}
	
	protected override void on_radio_single_leg_1_toggled(object o, EventArgs args) {
		if(toggleRaisesSignal) {
			string description = entry_description.Text;
			if(! slCMJDescriptionIsValid(description))
				description = slCMJDescriptionDefault();
			string [] d = description.Split(new char[] {' '});

			if(jumps_radiobutton_single_leg_mode_vertical.Active) {
				d[0] = "Vertical";	
				d[3] = "0";	//distance
				d[4] = "90";
			}
			else if(jumps_radiobutton_single_leg_mode_horizontal.Active)
				d[0] = "Horizontal";
			else
				d[0] = "Lateral";
			
			entry_description.Text = 
				d[0] + " " + d[1] + " " + d[2] + " " + d[3] + " " + d[4];
			fillSingleLeg(entry_description.Text);
		}
	}

	protected override void on_radio_single_leg_2_toggled(object o, EventArgs args) {
		if(toggleRaisesSignal) {
			string description = entry_description.Text;
			if(! slCMJDescriptionIsValid(description))
				description = slCMJDescriptionDefault();
			string [] d = description.Split(new char[] {' '});

			if(jumps_radiobutton_single_leg_dominance_this_limb.Active)
				d[1] = "This";	
			else if(jumps_radiobutton_single_leg_dominance_opposite.Active)
				d[1] = "Opposite";
			else
				d[1] = "Unknown"; //default since 1.4.8

			entry_description.Text = 
				d[0] + " " + d[1] + " " + d[2] + " " + d[3] + " " + d[4];
			fillSingleLeg(entry_description.Text);
		}
	}

	protected override void on_radio_single_leg_3_toggled(object o, EventArgs args) {
		if(toggleRaisesSignal) {
			string description = entry_description.Text;
			if(! slCMJDescriptionIsValid(description))
				description = slCMJDescriptionDefault();
			string [] d = description.Split(new char[] {' '});

			if(jumps_radiobutton_single_leg_fall_this_limb.Active)
				d[2] = "This";	
			else if(jumps_radiobutton_single_leg_fall_opposite.Active)
				d[2] = "Opposite";
			else
				d[2] = "Both"; //default since 1.4.8

			entry_description.Text = 
				d[0] + " " + d[1] + " " + d[2] + " " + d[3] + " " + d[4];
			fillSingleLeg(entry_description.Text);
		}
	}

	protected override void on_spin_single_leg_changed(object o, EventArgs args) {
		if(toggleRaisesSignal) {
			string description = entry_description.Text;
			if(! slCMJDescriptionIsValid(description))
				description = slCMJDescriptionDefault();
			string [] d = description.Split(new char[] {' '});

			int distance = Convert.ToInt32(jumps_spinbutton_single_leg_distance.Value);
			d[3] = distance.ToString();
			
			d[4] = Util.CalculateJumpAngle(
					Convert.ToDouble(Util.GetHeightInCentimeters(entryJumpTv)), 
					distance ).ToString();

			entry_description.Text = 
				d[0] + " " + d[1] + " " + d[2] + " " + d[3] + " " + d[4];
			fillSingleLeg(entry_description.Text);
		}
	}

	string weightOldStore = "0";
	protected override void on_combo_eventType_changed (object o, EventArgs args)
	{
		//if the distance of the new runType is fixed, put this distance
		//if not conserve the old
		JumpType myJumpType = new JumpType (UtilGtk.ComboGetActive(combo_eventType));

		if(myJumpType.Name == Constants.TakeOffName || myJumpType.Name == Constants.TakeOffWeightName) {
			entry_jump_tv_value.Text = "0";
			entry_jump_tv_value.Sensitive = false;
		} else 
			entry_jump_tv_value.Sensitive = true;


		if(myJumpType.HasWeight) {
			if(weightOldStore != "0")
				entry_weight_value.Text = weightOldStore;

			entry_weight_value.Sensitive = true;
		} else {
			//store weight in a variable if needed
			if(entry_weight_value.Text != "0")
				weightOldStore = entry_weight_value.Text;

			entry_weight_value.Text = "0";
			entry_weight_value.Sensitive = false;
		}
		
		frame_jumps_single_leg.Visible = (myJumpType.Name == "slCMJleft" || myJumpType.Name == "slCMJright");
		entry_description.Sensitive = (myJumpType.Name != "slCMJleft" && myJumpType.Name != "slCMJright");
		if(myJumpType.Name == "slCMJleft" || myJumpType.Name == "slCMJright") {
			fillSingleLeg(entry_description.Text);
		}
	}


	protected override void on_button_cancel_clicked (object o, EventArgs args)
	{
		EditJumpWindowBox.edit_event.Hide();
		EditJumpWindowBox = null;
	}
	
	protected override void on_delete_event (object o, DeleteEventArgs args)
	{
		EditJumpWindowBox.edit_event.Hide();
		EditJumpWindowBox = null;
	}
	
	protected override void hideWindow() {
		EditJumpWindowBox.edit_event.Hide();
		EditJumpWindowBox = null;
	}
	
	protected override void updateSQL(int eventID, int personID, string description) {
		//only for jump
		double jumpPercentWeightForNewPerson = updateWeight(personID, sessionID);
		
		//SqliteJump.Update(eventID, UtilGtk.ComboGetActive(combo_eventType), entryJumpTv, entryJumpTc, entryJumpFall, personID, jumpPercentWeightForNewPerson, description, Convert.ToDouble(entryAngle));
		SqliteJump.Update(eventID, UtilGtk.ComboGetActive(combo_eventType), entryJumpTv, entryJumpTc, entryJumpFall, personID, jumpPercentWeightForNewPerson, description, -1.0);
	}

	
	protected virtual double updateWeight(int personID, int mySessionID) {
		//only for jumps, jumpsRj
		//update the weight percent of jump if needed
		double jumpPercentWeightForNewPerson = 0;
		if(entryWeight != "0") {
			double oldPersonWeight = personWeight;

			double jumpWeightInKg = 0;
			if(weightPercentPreferred)
				jumpWeightInKg = Util.WeightFromPercentToKg(Convert.ToDouble(entryWeight), oldPersonWeight);
			else
				jumpWeightInKg = Convert.ToDouble(entryWeight);
			
			double newPersonWeight = SqlitePersonSession.SelectAttribute(false, personID, mySessionID, Constants.Weight); 
			//jumpPercentWeightForNewPerson = jumpWeightInKg * 100 / newPersonWeight; 
			jumpPercentWeightForNewPerson = Util.WeightFromKgToPercent(jumpWeightInKg, newPersonWeight); 
			LogB.Information(string.Format("oldPW: {0}, jWinKg {1}, newPW{2}, jWin%NewP{3}",
					oldPersonWeight, jumpWeightInKg, newPersonWeight, jumpPercentWeightForNewPerson));
		}

		return jumpPercentWeightForNewPerson;
	}
	
	private void connectWidgetsEditJump (Gtk.Builder builder)
	{
		frame_jumps_single_leg = (Gtk.Frame) builder.GetObject ("frame_jumps_single_leg");
		box_jumps_single_leg = (Gtk.Box) builder.GetObject ("box_jumps_single_leg");
		jumps_radiobutton_single_leg_mode_vertical = (Gtk.RadioButton) builder.GetObject ("jumps_radiobutton_single_leg_mode_vertical");
		jumps_radiobutton_single_leg_mode_horizontal = (Gtk.RadioButton) builder.GetObject ("jumps_radiobutton_single_leg_mode_horizontal");
		jumps_radiobutton_single_leg_mode_lateral = (Gtk.RadioButton) builder.GetObject ("jumps_radiobutton_single_leg_mode_lateral");
		jumps_radiobutton_single_leg_dominance_this_limb = (Gtk.RadioButton) builder.GetObject ("jumps_radiobutton_single_leg_dominance_this_limb");
		jumps_radiobutton_single_leg_dominance_opposite = (Gtk.RadioButton) builder.GetObject ("jumps_radiobutton_single_leg_dominance_opposite");
		jumps_radiobutton_single_leg_dominance_unknown = (Gtk.RadioButton) builder.GetObject ("jumps_radiobutton_single_leg_dominance_unknown");
		jumps_radiobutton_single_leg_fall_this_limb = (Gtk.RadioButton) builder.GetObject ("jumps_radiobutton_single_leg_fall_this_limb");
		jumps_radiobutton_single_leg_fall_opposite = (Gtk.RadioButton) builder.GetObject ("jumps_radiobutton_single_leg_fall_opposite");
		jumps_radiobutton_single_leg_fall_both = (Gtk.RadioButton) builder.GetObject ("jumps_radiobutton_single_leg_fall_both");
		jumps_spinbutton_single_leg_distance = (Gtk.SpinButton) builder.GetObject ("jumps_spinbutton_single_leg_distance");
		jumps_spinbutton_single_leg_jump_angle = (Gtk.SpinButton) builder.GetObject ("jumps_spinbutton_single_leg_jump_angle");
	}

}


public partial class ChronoJumpWindow
{
	private void on_edit_selected_jump_clicked (object o, EventArgs args)
	{
		//notebooks_change(0); see "notebooks_change sqlite problem"
		LogB.Information("Edit selected jump (simple)");
		//1.- check that there's a line selected
		//2.- check that this line is a jump and not a person (check also if it's not a individual RJ, the pass the parent RJ)
		int selectedID = treeViewResultsSession.EventSelectedID;
		if (selectedID < 0)
			return;

		//3.- obtain the data of the selected jump
		Jump myJump = SqliteJump.SelectJumpData (selectedID, false);
		eventOldPerson = myJump.PersonID;

		//4.- edit this jump
		editJumpWin = EditJumpWindow.Show(app1, myJump, preferences.weightStatsPercent, preferences.digitsNumber);
		editJumpWin.Fake_button_finished.Clicked += new EventHandler (on_edit_selected_jump_finished);
	}
	
	private void on_edit_selected_jump_finished (object o, EventArgs args)
	{
		LogB.Information("edit selected jump finished");
	
		Jump myJump = SqliteJump.SelectJumpData (treeViewResultsSession.EventSelectedID, false );

		//if person changed, fill treeview again, if not, only update it's line
		if (eventOldPerson == myJump.PersonID)
		{
			double personWeight = SqlitePersonSession.SelectAttribute (
					false, myJump.PersonID, currentSession.UniqueID, Constants.Weight);
			treeViewResultsSession.PersonWeight = personWeight;
			treeViewResultsSession.Update (myJump);
		}
		else
			pre_fillTreeView_resultsSession ();

		if(! configChronojump.Exhibition)
			updateGraphJumpsSimple();

		if(createdStatsWin) 
			stats_win_fillTreeView_stats(false, false);
	}
}
