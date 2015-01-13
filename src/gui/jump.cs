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
 * Copyright (C) 2004-2014   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
using Gtk;
using Glade;
using System.Text; //StringBuilder
using System.Collections; //ArrayList

using System.Threading;
using Mono.Unix;


//--------------------------------------------------------
//---------------- EDIT JUMP WIDGET ----------------------
//--------------------------------------------------------

public class EditJumpWindow : EditEventWindow
{
	[Widget] private Gtk.Frame frame_jumps_single_leg;
	[Widget] private Gtk.RadioButton jumps_radiobutton_single_leg_mode_vertical;
	[Widget] private Gtk.RadioButton jumps_radiobutton_single_leg_mode_horizontal;
	[Widget] private Gtk.RadioButton jumps_radiobutton_single_leg_mode_lateral;
	[Widget] private Gtk.RadioButton jumps_radiobutton_single_leg_dominance_this_limb;
	[Widget] private Gtk.RadioButton jumps_radiobutton_single_leg_dominance_opposite;
	[Widget] private Gtk.RadioButton jumps_radiobutton_single_leg_dominance_unknown;
	[Widget] private Gtk.RadioButton jumps_radiobutton_single_leg_fall_this_limb;
	[Widget] private Gtk.RadioButton jumps_radiobutton_single_leg_fall_opposite;
	[Widget] private Gtk.RadioButton jumps_radiobutton_single_leg_fall_both;
	[Widget] private Gtk.SpinButton jumps_spinbutton_single_leg_distance;
	[Widget] private Gtk.SpinButton jumps_spinbutton_single_leg_jump_angle;

	static EditJumpWindow EditJumpWindowBox;
	protected double personWeight;
	protected int sessionID; //for know weight specific to this session

	//for inheritance
	protected EditJumpWindow () {
	}

	public EditJumpWindow (Gtk.Window parent) {
		Glade.XML gladeXML;
		gladeXML = Glade.XML.FromAssembly (Util.GetGladePath() + "chronojump.glade", "edit_event", null);
		gladeXML.Autoconnect(this);
		this.parent =  parent;

		//put an icon to window
		UtilGtk.IconWindow(edit_event);
	
		eventBigTypeString = Catalog.GetString("jump");
	}

	static new public EditJumpWindow Show (Gtk.Window parent, Event myEvent, bool weightPercentPreferred, int pDN)
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

		EditJumpWindowBox.initializeValues();

		EditJumpWindowBox.fillDialog (myEvent);
		
		if(myEvent.Type == "slCMJleft" || myEvent.Type == "slCMJright")
			EditJumpWindowBox.fillSingleLeg (myEvent.Description);
		
		EditJumpWindowBox.edit_event.Show ();

		return EditJumpWindowBox;
	}
	
	protected override void initializeValues () {
		typeOfTest = Constants.TestTypes.JUMP;
		showType = true;
		showRunStart = false;
		showTv = true;
		showTc= true;
		showFall = true;
		showDistance = false;
		showTime = false;
		showSpeed = false;
		showWeight = true;
		showLimited = false;
		showAngle = true;
		showMistakes = false;
		
		if(weightPercentPreferred)
			label_weight_units.Text = "%";
		else
			label_weight_units.Text = "Kg";

		LogB.Information(string.Format("-------------{0}", personWeight));
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
		entryTv = myJump.Tv.ToString();

		//show all the decimals for not triming there in edit window using
		//(and having different values in formulae like GetHeightInCm ...)
		//entry_tv_value.Text = Util.TrimDecimals(entryTv, pDN);
		entry_tv_value.Text = entryTv;
	
		//hide tv if it's only a takeoff	
		if(myEvent.Type == Constants.TakeOffName || myEvent.Type == Constants.TakeOffWeightName) 
			entry_tv_value.Sensitive = false;
	}

	protected override void fillTc (Event myEvent) {
		//on normal jumps fills Tc and Fall
		Jump myJump = (Jump) myEvent;

		if (myJump.TypeHasFall) {
			entryTc = myJump.Tc.ToString();
			
			//show all the decimals for not triming there in edit window using
			//(and having different values in formulae like GetHeightInCm ...)
			//entry_tc_value.Text = Util.TrimDecimals(entryTc, pDN);
			entry_tc_value.Text = entryTc;
			
			entryFall = myJump.Fall.ToString();
			entry_fall_value.Text = entryFall;
			entry_tc_value.Sensitive = true;
			entry_fall_value.Sensitive = true;
		} else {
			entry_tc_value.Sensitive = false;
			entry_fall_value.Sensitive = false;
		}
	}

	protected override void fillWeight(Event myEvent) {
		Jump myJump = (Jump) myEvent;
		if(myJump.TypeHasWeight) {
			if(weightPercentPreferred)
				entryWeight = myJump.Weight.ToString();
			else
				entryWeight = Util.WeightFromPercentToKg(myJump.Weight, personWeight).ToString();

			entry_weight_value.Text = entryWeight;
			entry_weight_value.Sensitive = true;
		} else {
			entry_weight_value.Sensitive = false;
		}
	}
	
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
					Convert.ToDouble(Util.GetHeightInCentimeters(entryTv)), 
					distance ).ToString();

			entry_description.Text = 
				d[0] + " " + d[1] + " " + d[2] + " " + d[3] + " " + d[4];
			fillSingleLeg(entry_description.Text);
		}
	}


	
	protected override void createSignal() {
		//only for jumps & runs
		combo_eventType.Changed += new EventHandler (on_combo_eventType_changed);
	}

	string weightOldStore = "0";
	private void on_combo_eventType_changed (object o, EventArgs args) {
		//if the distance of the new runType is fixed, put this distance
		//if not conserve the old
		JumpType myJumpType = new JumpType (UtilGtk.ComboGetActive(combo_eventType));

		if(myJumpType.Name == Constants.TakeOffName || myJumpType.Name == Constants.TakeOffWeightName) {
			entry_tv_value.Text = "0";
			entry_tv_value.Sensitive = false;
		} else 
			entry_tv_value.Sensitive = true;


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
	
	protected override void updateEvent(int eventID, int personID, string description) {
		//only for jump
		double jumpPercentWeightForNewPerson = updateWeight(personID, sessionID);
		
		SqliteJump.Update(eventID, UtilGtk.ComboGetActive(combo_eventType), entryTv, entryTc, entryFall, personID, jumpPercentWeightForNewPerson, description, Convert.ToDouble(entryAngle));
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
	

}

//--------------------------------------------------------
//---------------- EDIT JUMP RJ WIDGET -------------------
//--------------------------------------------------------

public class EditJumpRjWindow : EditJumpWindow
{
	static EditJumpRjWindow EditJumpRjWindowBox;

	EditJumpRjWindow (Gtk.Window parent) {
		Glade.XML gladeXML;
		gladeXML = Glade.XML.FromAssembly (Util.GetGladePath() + "chronojump.glade", "edit_event", null);
		gladeXML.Autoconnect(this);
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

		EditJumpRjWindowBox.initializeValues();

		EditJumpRjWindowBox.fillDialog (myEvent);
		
		EditJumpRjWindowBox.edit_event.Show ();

		return EditJumpRjWindowBox;
	}
	
	protected override void initializeValues () {
		typeOfTest = Constants.TestTypes.JUMP_RJ;
		showType = true;
		showRunStart = false;
		showTv = false;
		showTc = false;
		showFall = true;
		showDistance = false;
		showTime = false;
		showSpeed = false;
		showWeight = true;
		showLimited = true;
		showMistakes = false;
		
		if(weightPercentPreferred)
			label_weight_units.Text = "%";
		else
			label_weight_units.Text = "Kg";
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
				entryWeight = myJump.Weight.ToString();
			else
				entryWeight = Util.WeightFromPercentToKg(myJump.Weight, personWeight).ToString();

			entry_weight_value.Text = entryWeight;
			entry_weight_value.Sensitive = true;
		} else {
			entry_weight_value.Sensitive = false;
		}
	}

	protected override void fillFall(Event myEvent) {
		JumpRj myJump = (JumpRj) myEvent;
		entryFall = myJump.Fall.ToString();
		entry_fall_value.Text = entryFall;
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
	
	protected override void updateEvent(int eventID, int personID, string description) {
		//only for jumps
		double jumpPercentWeightForNewPerson = updateWeight(personID, sessionID);
		
		SqliteJumpRj.Update(eventID, personID, entryFall, jumpPercentWeightForNewPerson, description);
	}
}


//--------------------------------------------------------
//---------------- Repair jumpRJ WIDGET ------------------
//--------------------------------------------------------

public class RepairJumpRjWindow 
{
	[Widget] Gtk.Window repair_sub_event;
	[Widget] Gtk.Label label_header;
	[Widget] Gtk.Label label_totaltime_value;
	[Widget] Gtk.TreeView treeview_subevents;
	private TreeStore store;
	[Widget] Gtk.Button button_accept;
	[Widget] Gtk.Button button_add_before;
	[Widget] Gtk.Button button_add_after;
	[Widget] Gtk.Button button_delete;
	[Widget] Gtk.TextView textview1;

	static RepairJumpRjWindow RepairJumpRjWindowBox;
	Gtk.Window parent;
	//int pDN;

	JumpType jumpType;
	JumpRj jumpRj; //used on button_accept
	

	RepairJumpRjWindow (Gtk.Window parent, JumpRj myJump, int pDN) {
		Glade.XML gladeXML;
		gladeXML = Glade.XML.FromAssembly (Util.GetGladePath() + "chronojump.glade", "repair_sub_event", null);
		gladeXML.Autoconnect(this);
	
		//put an icon to window
		UtilGtk.IconWindow(repair_sub_event);
	
		this.parent = parent;
		this.jumpRj = myJump;

		//this.pDN = pDN;
	
		repair_sub_event.Title = Catalog.GetString("Repair reactive jump");
		
		System.Globalization.NumberFormatInfo localeInfo = new System.Globalization.NumberFormatInfo();
		localeInfo = System.Globalization.NumberFormatInfo.CurrentInfo;
		label_header.Text = string.Format(Catalog.GetString("Use this window to repair this test.\nDouble clic any cell to edit it (decimal separator: '{0}')"), localeInfo.NumberDecimalSeparator);
	
		
		jumpType = SqliteJumpType.SelectAndReturnJumpRjType(myJump.Type, false);
		
		TextBuffer tb = new TextBuffer (new TextTagTable());
		tb.Text = createTextForTextView(jumpType);
		textview1.Buffer = tb;
		
		createTreeView(treeview_subevents);
		//count, tc, tv
		store = new TreeStore(typeof (string), typeof (string), typeof(string));
		treeview_subevents.Model = store;
		fillTreeView (treeview_subevents, store, myJump, pDN);
	
		button_add_before.Sensitive = false;
		button_add_after.Sensitive = false;
		button_delete.Sensitive = false;
		
		label_totaltime_value.Text = getTotalTime().ToString() + " " + Catalog.GetString("seconds");
		
		treeview_subevents.Selection.Changed += onSelectionEntry;
	}
	
	static public RepairJumpRjWindow Show (Gtk.Window parent, JumpRj myJump, int pDN)
	{
		//LogB.Information(myJump);
		if (RepairJumpRjWindowBox == null) {
			RepairJumpRjWindowBox = new RepairJumpRjWindow (parent, myJump, pDN);
		}
		
		RepairJumpRjWindowBox.repair_sub_event.Show ();

		return RepairJumpRjWindowBox;
	}
	
	private string createTextForTextView (JumpType myJumpType) {
		string jumpTypeString = string.Format(Catalog.GetString(
					"JumpType: {0}."), myJumpType.Name);

		//if it's a jump type that starts in, then don't allow first TC be different than -1
		string startString = "";
		if(myJumpType.StartIn) {
			startString = string.Format(Catalog.GetString("\nThis jump type starts inside, the first time should be a flight time."));
		}

		string fixedString = "";
		if(myJumpType.FixedValue > 0) {
			if(myJumpType.JumpsLimited) {
				//if it's a jump type jumpsLimited with a fixed value, then don't allow the creation of more jumps, and respect the -1 at last TF if found
				fixedString = "\n" + string.Format(
						Catalog.GetPluralString(
							"This jump type is fixed to one jump.",
							"This jump type is fixed to {0} jumps.",
							Convert.ToInt32(myJumpType.FixedValue)),
						myJumpType.FixedValue) +
					Catalog.GetString("You cannot add more.");
			} else {
				//if it's a jump type timeLimited with a fixed value, then complain when the total time is higher
				fixedString = "\n" + string.Format(
						Catalog.GetPluralString(
							"This jump type is fixed to one second.",
							"This jump type is fixed to {0} seconds.",
							Convert.ToInt32(myJumpType.FixedValue)),
						myJumpType.FixedValue) +
					Catalog.GetString("You cannot add more.");
			}
		}
		return jumpTypeString + startString + fixedString;
	}

	
	private void createTreeView (Gtk.TreeView myTreeView) {
		myTreeView.HeadersVisible=true;
		int count = 0;

		myTreeView.AppendColumn ( Catalog.GetString ("Count"), new CellRendererText(), "text", count++);

		Gtk.TreeViewColumn tcColumn = new Gtk.TreeViewColumn ();
		tcColumn.Title = Catalog.GetString("TC");
		Gtk.CellRendererText tcCell = new Gtk.CellRendererText ();
		tcCell.Editable = true;
		tcCell.Edited += tcCellEdited;
		tcColumn.PackStart (tcCell, true);
		tcColumn.AddAttribute(tcCell, "text", count ++);
		myTreeView.AppendColumn ( tcColumn );
		
		Gtk.TreeViewColumn tvColumn = new Gtk.TreeViewColumn ();
		tvColumn.Title = Catalog.GetString("TF");
		Gtk.CellRendererText tvCell = new Gtk.CellRendererText ();
		tvCell.Editable = true;
		tvCell.Edited += tvCellEdited;
		tvColumn.PackStart (tvCell, true);
		tvColumn.AddAttribute(tvCell, "text", count ++);
		myTreeView.AppendColumn ( tvColumn );
	}
	
	private void tcCellEdited (object o, Gtk.EditedArgs args)
	{
		Gtk.TreeIter iter;
		store.GetIter (out iter, new Gtk.TreePath (args.Path));
		if(Util.IsNumber(args.NewText, true) && (string) treeview_subevents.Model.GetValue(iter,1) != "-1") {
			//if it's limited by fixed value of seconds
			//and new seconds are bigger than allowed, return
			if(jumpType.FixedValue > 0 && ! jumpType.JumpsLimited &&
					getTotalTime() //current total time in treeview
					- Convert.ToDouble((string) treeview_subevents.Model.GetValue(iter,1)) //-old cell
					+ Convert.ToDouble(args.NewText) //+new cell
					> jumpType.FixedValue) {	//bigger than allowed
				return;
			} else {
				store.SetValue(iter, 1, args.NewText);

				//update the totaltime label
				label_totaltime_value.Text = getTotalTime().ToString() + " " + Catalog.GetString("seconds");
			}
		}
		
		//if is not number or if it was -1, the old data will remain
	}

	private void tvCellEdited (object o, Gtk.EditedArgs args)
	{
		Gtk.TreeIter iter;
		store.GetIter (out iter, new Gtk.TreePath (args.Path));
		if(Util.IsNumber(args.NewText, true) && (string) treeview_subevents.Model.GetValue(iter,2) != "-1") {
			//if it's limited by fixed value of seconds
			//and new seconds are bigger than allowed, return
			if(jumpType.FixedValue > 0 && ! jumpType.JumpsLimited &&
					getTotalTime() //current total time in treeview
					- Convert.ToDouble((string) treeview_subevents.Model.GetValue(iter,2)) //-old cell
					+ Convert.ToDouble(args.NewText) //+new cell
					> jumpType.FixedValue) {	//bigger than allowed
				return;
			} else {
				store.SetValue(iter, 2, args.NewText);
				
				//update the totaltime label
				label_totaltime_value.Text = getTotalTime().ToString() + " " + Catalog.GetString("seconds");
			}
		}
		//if is not number or if it was -1, the old data will remain
	}

	private double getTotalTime() {
		TreeIter myIter;
		double totalTime = 0;
		bool iterOk = store.GetIterFirst (out myIter);
		string stringTc = "";
		string stringTv = "";
		if(iterOk) {
			do {
				//be cautious because when there's no value (like first tc in a jump that starts in,
				//it's stored as "-1", but it's shown as "-"
				stringTc = (string) treeview_subevents.Model.GetValue(myIter, 1);
				if(stringTc != "-" && stringTc != "-1") 
					totalTime += Convert.ToDouble(stringTc);
				
				stringTv = (string) treeview_subevents.Model.GetValue(myIter, 2);
				if(stringTv != "-" && stringTv != "-1") 
					totalTime += Convert.ToDouble(stringTv);
				
			} while (store.IterNext (ref myIter));
		}
		return totalTime;
	}
	
	private void fillTreeView (Gtk.TreeView tv, TreeStore store, JumpRj myJump, int pDN)
	{
		if(myJump.TcString.Length > 0 && myJump.TvString.Length > 0) {
			string [] tcArray = myJump.TcString.Split(new char[] {'='});
			string [] tvArray = myJump.TvString.Split(new char[] {'='});

			int count = 0;
			foreach (string myTc in tcArray) {
				string myTv;
				if(tvArray.Length >= count)
					myTv = Util.TrimDecimals(tvArray[count], pDN);
				else
					myTv = "";

				store.AppendValues ( (count+1).ToString(), Util.TrimDecimals(myTc, pDN), myTv );

				count ++;
			}
		}
	}

	void onSelectionEntry (object o, EventArgs args) {
		TreeModel model;
		TreeIter iter;
		
		if (((TreeSelection)o).GetSelected(out model, out iter)) {
			button_add_before.Sensitive = true;
			button_add_after.Sensitive = true;
			button_delete.Sensitive = true;

			//don't allow to add a row before the first if first row has a -1 in 'TC'
			//also don't allow deleting
			if((string) model.GetValue (iter, 1) == "-1") {
				button_add_before.Sensitive = false;
				button_delete.Sensitive = false;
			}

			//don't allow to add a row after the last if it has a -1
			//also don't allow deleting
			//the only -1 in flight time can be in the last row
			if((string) model.GetValue (iter, 2) == "-1") {
				button_add_after.Sensitive = false;
				button_delete.Sensitive = false;
			}
			
			//don't allow to add a row before or after 
			//if the jump type is fixed to n jumps and we reached n
			if(jumpType.FixedValue > 0 && jumpType.JumpsLimited) {
				int lastRow = 0;
				do {
					lastRow = Convert.ToInt32 ((string) model.GetValue (iter, 0));
				} while (store.IterNext (ref iter));

				//don't allow if max rows reached
				if(lastRow == jumpType.FixedValue ||
						( lastRow == jumpType.FixedValue +1 && jumpType.StartIn) ) {
					button_add_before.Sensitive = false;
					button_add_after.Sensitive = false;
				}
			}
		}
	}

	void on_button_add_before_clicked (object o, EventArgs args) {
		TreeModel model; 
		TreeIter iter; 
		if (treeview_subevents.Selection.GetSelected (out model, out iter)) {
			int position = Convert.ToInt32( (string) model.GetValue (iter, 0) ) -1; //count starts at '0'
			iter = store.InsertNode(position);
			store.SetValue(iter, 1, "0");
			store.SetValue(iter, 2, "0");
			putRowNumbers(store);
		}
	}
	
	void on_button_add_after_clicked (object o, EventArgs args) {
		TreeModel model; 
		TreeIter iter; 
		if (treeview_subevents.Selection.GetSelected (out model, out iter)) {
			int position = Convert.ToInt32( (string) model.GetValue (iter, 0) ); //count starts at '0'
			iter = store.InsertNode(position);
			store.SetValue(iter, 1, "0");
			store.SetValue(iter, 2, "0");
			putRowNumbers(store);
		}
	}
	
	private void putRowNumbers(TreeStore myStore) {
		TreeIter myIter;
		bool iterOk = myStore.GetIterFirst (out myIter);
		if(iterOk) {
			int count = 1;
			do {
				store.SetValue(myIter, 0, (count++).ToString());
			} while (myStore.IterNext (ref myIter));
		}
	}
		
	void on_button_delete_clicked (object o, EventArgs args) {
		TreeModel model; 
		TreeIter iter; 
		if (treeview_subevents.Selection.GetSelected (out model, out iter)) {
			store.Remove(ref iter);
			putRowNumbers(store);
		
			label_totaltime_value.Text = getTotalTime().ToString() + " " + Catalog.GetString("seconds");

			button_add_before.Sensitive = false;
			button_add_after.Sensitive = false;
			button_delete.Sensitive = false;
		}
	}
	
	void on_button_accept_clicked (object o, EventArgs args)
	{
		//foreach all lines... extrac tcString and tvString
		TreeIter myIter;
		string tcString = "";
		string tvString = "";
		
		bool iterOk = store.GetIterFirst (out myIter);
		if(iterOk) {
			string equal= ""; //first iteration should not appear '='
			do {
				tcString = tcString + equal + (string) treeview_subevents.Model.GetValue (myIter, 1);
				tvString = tvString + equal + (string) treeview_subevents.Model.GetValue (myIter, 2);
				equal = "=";
			} while (store.IterNext (ref myIter));
		}
		
		jumpRj.TvString = tvString;	
		jumpRj.TcString = tcString;	
		jumpRj.Jumps = Util.GetNumberOfJumps(tvString, false);
		jumpRj.Time = Util.GetTotalTime(tcString, tvString);

		//calculate other variables needed for jumpRj creation
		
		if(jumpType.FixedValue > 0) {
			//if this jumpType has a fixed value of jumps or time, limitstring has not changed
			if(jumpType.JumpsLimited) {
				jumpRj.Limited = jumpType.FixedValue.ToString() + "J";
			} else {
				jumpRj.Limited = jumpType.FixedValue.ToString() + "T";
			}
		} else {
			//else limitstring should be calculated
			if(jumpType.JumpsLimited) {
				jumpRj.Limited = jumpRj.Jumps.ToString() + "J";
			} else {
				jumpRj.Limited = Util.GetTotalTime(tcString, tvString) + "T";
			}
		}

		//save it deleting the old first for having the same uniqueID
		Sqlite.Delete(false, Constants.JumpRjTable, jumpRj.UniqueID);
		jumpRj.InsertAtDB(false, Constants.JumpRjTable); 
		/*
		SqliteJump.InsertRj("jumpRj", jumpRj.UniqueID.ToString(), jumpRj.PersonID, jumpRj.SessionID, 
				jumpRj.Type, Util.GetMax(tvString), Util.GetMax(tcString), 
				jumpRj.Fall, jumpRj.Weight, jumpRj.Description,
				Util.GetAverage(tvString), Util.GetAverage(tcString),
				tvString, tcString,
				jumps, Util.GetTotalTime(tcString, tvString), limitString
				);
				*/

		//close the window
		RepairJumpRjWindowBox.repair_sub_event.Hide();
		RepairJumpRjWindowBox = null;
	}

	void on_button_cancel_clicked (object o, EventArgs args)
	{
		RepairJumpRjWindowBox.repair_sub_event.Hide();
		RepairJumpRjWindowBox = null;
	}
	
	void on_delete_event (object o, DeleteEventArgs args)
	{
		RepairJumpRjWindowBox.repair_sub_event.Hide();
		RepairJumpRjWindowBox = null;
	}
	
	public Button Button_accept 
	{
		set { button_accept = value;	}
		get { return button_accept;	}
	}

}

//--------------------------------------------------------
//---------------- jump extra WIDGET --------------------
//---------------- in 0.9.3 included in main gui ---------
//--------------------------------------------------------

partial class ChronoJumpWindow
{
	//options jumps
	[Widget] Gtk.SpinButton extra_window_jumps_spinbutton_weight;
	[Widget] Gtk.Box extra_window_jumps_vbox_fall;
	[Widget] Gtk.RadioButton extra_window_jumps_radio_dj_fall_predefined;
	[Widget] Gtk.RadioButton extra_window_jumps_radio_dj_fall_calculate;
	[Widget] Gtk.Label extra_window_jumps_label_dj_start_inside;
	[Widget] Gtk.Label extra_window_jumps_label_dj_start_outside;
	[Widget] Gtk.SpinButton extra_window_jumps_spinbutton_fall;
	[Widget] Gtk.RadioButton extra_window_jumps_radiobutton_kg;
	[Widget] Gtk.RadioButton extra_window_jumps_radiobutton_weight;
	[Widget] Gtk.Label extra_window_jumps_label_weight;
	[Widget] Gtk.Label extra_window_jumps_label_cm;
	[Widget] Gtk.Label extra_window_jumps_label_dj_arms;
	[Widget] Gtk.CheckButton extra_window_jumps_check_dj_arms;
	[Widget] Gtk.Label extra_window_label_jumps_no_options;

	//slCMJ	
	[Widget] Gtk.Box vbox_extra_window_jumps_single_leg;
	[Widget] Gtk.Box vbox_extra_window_jumps_single_leg_radios;
	[Widget] Gtk.Frame frame_extra_window_jumps_single_leg_input;
	[Widget] Gtk.RadioButton extra_window_jumps_radiobutton_single_leg_mode_vertical;
	[Widget] Gtk.RadioButton extra_window_jumps_radiobutton_single_leg_mode_horizontal;
	[Widget] Gtk.RadioButton extra_window_jumps_radiobutton_single_leg_mode_lateral;
	[Widget] Gtk.RadioButton extra_window_jumps_radiobutton_single_leg_dominance_this_limb;
	[Widget] Gtk.RadioButton extra_window_jumps_radiobutton_single_leg_dominance_opposite;
	[Widget] Gtk.RadioButton extra_window_jumps_radiobutton_single_leg_dominance_unknown;
	[Widget] Gtk.RadioButton extra_window_jumps_radiobutton_single_leg_fall_this_limb;
	[Widget] Gtk.RadioButton extra_window_jumps_radiobutton_single_leg_fall_opposite;
	[Widget] Gtk.RadioButton extra_window_jumps_radiobutton_single_leg_fall_both;
	[Widget] Gtk.SpinButton extra_window_jumps_spin_single_leg_distance;
	[Widget] Gtk.SpinButton extra_window_jumps_spin_single_leg_angle;
	
	//options jumps_rj
	[Widget] Gtk.Label extra_window_jumps_rj_label_limit;
	[Widget] Gtk.SpinButton extra_window_jumps_rj_spinbutton_limit;
	[Widget] Gtk.Label extra_window_jumps_rj_label_limit_units;
	[Widget] Gtk.SpinButton extra_window_jumps_rj_spinbutton_weight;
	[Widget] Gtk.SpinButton extra_window_jumps_rj_spinbutton_fall;
	[Widget] Gtk.RadioButton extra_window_jumps_rj_radiobutton_kg;
	[Widget] Gtk.RadioButton extra_window_jumps_rj_radiobutton_weight;
	[Widget] Gtk.Label extra_window_jumps_rj_label_weight;
	[Widget] Gtk.Label extra_window_jumps_rj_label_fall;
	[Widget] Gtk.Label extra_window_jumps_rj_label_cm;
	[Widget] Gtk.Label extra_window_label_jumps_rj_no_options;
	[Widget] Gtk.CheckButton checkbutton_allow_finish_rj_after_time;

	
	//for RunAnalysis
	//but will be used and recorded with "fall"
	//static double distance;

	//jumps
	string extra_window_jumps_option = "%";
	//double extra_window_jumps_weight = 20;
	double extra_window_jumps_fall = 20;
	bool extra_window_jumps_arms = false;
	
	//jumps_rj
	double extra_window_jumps_rj_limited = 10;
	bool extra_window_jumps_rj_jumpsLimited;
	string extra_window_jumps_rj_option = "%";
	double extra_window_jumps_rj_weight = 20;
	double extra_window_jumps_rj_fall = 20;
	
	private JumpType previousJumpType; //used on More to turnback if cancel or delete event is pressed
	private JumpType previousJumpRjType; //used on More to turnback if cancel or delete event is pressed
	
	
	//creates and if is not predefined, checks database to gather all the data
	//simple == true  for normal jumps, and false for reactive
	private JumpType createJumpType(string name, bool simple) {
		JumpType t = new JumpType(name);
		
		if(! t.IsPredefined) {
			if(simple) {
				t = SqliteJumpType.SelectAndReturnJumpType(name, false);
				t.ImageFileName = SqliteEvent.GraphLinkSelectFileName(Constants.JumpTable, name);
			} else {
				t = SqliteJumpType.SelectAndReturnJumpRjType(name, false);
				t.ImageFileName = SqliteEvent.GraphLinkSelectFileName(Constants.JumpRjTable, name);
			}
		}
		return t;
	}


	private void on_extra_window_jumps_radio_dj_fall_calculate_toggled (object o, EventArgs args) {
		extra_window_jumps_label_dj_start_inside.Visible = true;
		extra_window_jumps_label_dj_start_outside.Visible = false;
	}
	private void on_extra_window_jumps_radio_dj_fall_predefined_toggled (object o, EventArgs args) {
		extra_window_jumps_label_dj_start_inside.Visible = false;
		extra_window_jumps_label_dj_start_outside.Visible = true;
	}
	
	private void on_extra_window_jumps_test_changed(object o, EventArgs args)
	{
		string jumpEnglishName = Util.FindOnArray(':',2,1, UtilGtk.ComboGetActive(combo_select_jumps), selectJumpsString);
		currentJumpType = createJumpType(jumpEnglishName, true);
	
		extra_window_jumps_initialize(currentJumpType);
	}
	
	private void on_extra_window_jumps_more(object o, EventArgs args)
	{
		previousJumpType = currentJumpType;

		jumpsMoreWin = JumpsMoreWindow.Show(app1, true);
		jumpsMoreWin.Button_accept.Clicked += new EventHandler(on_more_jumps_accepted);
		jumpsMoreWin.Button_cancel.Clicked += new EventHandler(on_more_jumps_cancelled);
		jumpsMoreWin.Button_selected.Clicked += new EventHandler(on_more_jumps_update_test);
	}
	
	private void on_extra_window_jumps_rj_test_changed(object o, EventArgs args)
	{
		string jumpEnglishName = Util.FindOnArray(':',2,1, UtilGtk.ComboGetActive(combo_select_jumps_rj), selectJumpsRjString);
		currentJumpRjType = createJumpType(jumpEnglishName, false);

		extra_window_jumps_rj_initialize(currentJumpRjType);
	}

	private void on_extra_window_jumps_rj_more(object o, EventArgs args) 
	{
		previousJumpRjType = currentJumpRjType;

		jumpsRjMoreWin = JumpsRjMoreWindow.Show(app1, true);
		jumpsRjMoreWin.Button_accept.Clicked += new EventHandler(on_more_jumps_rj_accepted);
		jumpsRjMoreWin.Button_cancel.Clicked += new EventHandler(on_more_jumps_rj_cancelled);
		jumpsRjMoreWin.Button_selected.Clicked += new EventHandler(on_more_jumps_rj_update_test);
	}


	private void extra_window_jumps_initialize(JumpType myJumpType) 
	{
		currentEventType = myJumpType;
		changeTestImage(EventType.Types.JUMP.ToString(), myJumpType.Name, myJumpType.ImageFileName);
		bool hasOptions = false;
	
		if(myJumpType.HasWeight) {
			hasOptions = true;
			extra_window_showWeightData(myJumpType, true);	
		} else 
			extra_window_showWeightData(myJumpType, false);	

		if(myJumpType.HasFall) {
			hasOptions = true;
			extra_window_showFallData(myJumpType, true);
		} else
			extra_window_showFallData(myJumpType, false);	
		
		if(myJumpType.Name == "DJa" || myJumpType.Name == "DJna") { 
			//on DJa and DJna (coming from More jumps) need to show technique data but not change
			if(myJumpType.Name == "DJa")
				extra_window_jumps_check_dj_arms.Active = true;
			else //myJumpType.Name == "DJna"
				extra_window_jumps_check_dj_arms.Active = false;

			hasOptions = true;
			extra_window_showTechniqueArmsData(true, false); //visible, sensitive
		} else if(myJumpType.Name == "DJ") { 
			//user has pressed DJ button
			hasOptions = true;
			extra_window_jumps_check_dj_arms.Active = extra_window_jumps_arms;

			on_extra_window_jumps_check_dj_arms_clicked(new object(), new EventArgs());
			extra_window_showTechniqueArmsData(true, true); //visible, sensitive
		} else 
			extra_window_showTechniqueArmsData(false, false); //visible, sensitive
		
		extra_window_jumps_spinbutton_weight.Value = 100;
		extra_window_jumps_spinbutton_fall.Value = extra_window_jumps_fall;
		if (extra_window_jumps_option == "Kg") {
			extra_window_jumps_radiobutton_kg.Active = true;
		} else {
			extra_window_jumps_radiobutton_weight.Active = true;
		}

		extra_window_showSingleLegStuff(myJumpType.Name == "slCMJleft" || myJumpType.Name == "slCMJright");
		if(myJumpType.Name == "slCMJleft" || myJumpType.Name == "slCMJright") {
			hasOptions = true;
			frame_extra_window_jumps_single_leg_input.Sensitive = false;
			extra_window_jumps_spin_single_leg_distance.Value = 0;
			extra_window_jumps_spin_single_leg_angle.Value = 90;
		}

		extra_window_jumps_showNoOptions(myJumpType, hasOptions);
	}
	
	private void extra_window_jumps_rj_initialize(JumpType myJumpType) 
	{
		currentEventType = myJumpType;
		changeTestImage(EventType.Types.JUMP.ToString(), myJumpType.Name, myJumpType.ImageFileName);
		bool hasOptions = false;
		checkbutton_allow_finish_rj_after_time.Visible = false;
	
		if(myJumpType.FixedValue >= 0) {
			hasOptions = true;
			string jumpsName = Catalog.GetString("jumps");
			string secondsName = Catalog.GetString("seconds");
			if(myJumpType.JumpsLimited) {
				extra_window_jumps_rj_jumpsLimited = true;
				extra_window_jumps_rj_label_limit_units.Text = jumpsName;
			} else {
				extra_window_jumps_rj_jumpsLimited = false;
				extra_window_jumps_rj_label_limit_units.Text = secondsName;
				checkbutton_allow_finish_rj_after_time.Visible = true;
			}
			if(myJumpType.FixedValue > 0) {
				extra_window_jumps_rj_spinbutton_limit.Sensitive = false;
				extra_window_jumps_rj_spinbutton_limit.Value = myJumpType.FixedValue;
			} else {
				extra_window_jumps_rj_spinbutton_limit.Sensitive = true;
				extra_window_jumps_rj_spinbutton_limit.Value = extra_window_jumps_rj_limited;
			}
			extra_window_showLimitData (true);
		} else  //unlimited
			extra_window_showLimitData (false);

		if(myJumpType.HasWeight) {
			hasOptions = true;
			extra_window_showWeightData(myJumpType, true);	
		} else 
			extra_window_showWeightData(myJumpType, false);	

		if(myJumpType.HasFall || myJumpType.Name == Constants.RunAnalysisName) {
			extra_window_showFallData(myJumpType, true);	
			hasOptions = true;
		} else
			extra_window_showFallData(myJumpType, false);
		
		extra_window_jumps_rj_spinbutton_weight.Value = extra_window_jumps_rj_weight;
		extra_window_jumps_rj_spinbutton_fall.Value = extra_window_jumps_rj_fall;
		if (extra_window_jumps_rj_option == "Kg") {
			extra_window_jumps_rj_radiobutton_kg.Active = true;
		} else {
			extra_window_jumps_rj_radiobutton_weight.Active = true;
		}

		extra_window_jumps_showNoOptions(myJumpType, hasOptions);
	}

	private void on_extra_window_jumps_check_dj_arms_clicked(object o, EventArgs args)
	{
		JumpType j = new JumpType();
		if(extra_window_jumps_check_dj_arms.Active) 
			j = new JumpType("DJa");
		else
			j = new JumpType("DJna");

		changeTestImage(EventType.Types.JUMP.ToString(), j.Name, j.ImageFileName);
	}

	private void on_extra_window_checkbutton_allow_finish_rj_after_time_toggled(object o, EventArgs args)
	{
		SqlitePreferences.Update("allowFinishRjAfterTime", checkbutton_allow_finish_rj_after_time.Active.ToString(), false);
	}

	private void on_more_jumps_update_test (object o, EventArgs args) {
		currentEventType = new JumpType(jumpsMoreWin.SelectedEventName);
		string jumpTranslatedName = Util.FindOnArray(':',1,2, jumpsMoreWin.SelectedEventName, selectJumpsString);
		
		combo_select_jumps.Active = UtilGtk.ComboMakeActive(combo_select_jumps, jumpTranslatedName);	
	}
	
	private void on_more_jumps_rj_update_test (object o, EventArgs args) {
		currentEventType = new JumpType(jumpsRjMoreWin.SelectedEventName);
		string jumpTranslatedName = Util.FindOnArray(':',1,2, jumpsRjMoreWin.SelectedEventName, selectJumpsRjString);
		
		combo_select_jumps_rj.Active = UtilGtk.ComboMakeActive(combo_select_jumps_rj, jumpTranslatedName);	
	}
	
	//used from the dialogue "jumps more"
	private void on_more_jumps_accepted (object o, EventArgs args) 
	{
		jumpsMoreWin.Button_accept.Clicked -= new EventHandler(on_more_jumps_accepted);
		
		currentJumpType = createJumpType(jumpsMoreWin.SelectedEventName, true);

		extra_window_jumps_initialize(currentJumpType);
		
		//destroy the win for not having updating problems if a new jump type is created
		//jumpsMoreWin = null; //don't work
		jumpsMoreWin.Destroy(); //works ;)
	}
	
	//used from the dialogue "jumps rj more"
	private void on_more_jumps_rj_accepted (object o, EventArgs args) 
	{
		jumpsRjMoreWin.Button_accept.Clicked -= new EventHandler(on_more_jumps_rj_accepted);

		currentJumpRjType = createJumpType(jumpsRjMoreWin.SelectedEventName, false);
		
		extra_window_jumps_rj_initialize(currentJumpRjType);
	
		//destroy the win for not having updating problems if a new jump type is created
		jumpsRjMoreWin.Destroy();
	}

	//if it's cancelled (or deleted event) select desired toolbar button
	private void on_more_jumps_cancelled (object o, EventArgs args) 
	{
		currentJumpType = previousJumpType;
		extra_window_jumps_initialize(currentJumpType);
	}
	
	private void on_more_jumps_rj_cancelled (object o, EventArgs args) 
	{
		currentJumpRjType = previousJumpRjType;
		extra_window_jumps_rj_initialize(currentJumpRjType);
	}
	
	private void extra_window_showWeightData (JumpType myJumpType, bool show) {
		if(myJumpType.IsRepetitive) {
			extra_window_jumps_rj_label_weight.Visible = show;
			extra_window_jumps_rj_spinbutton_weight.Visible = show;
			extra_window_jumps_rj_radiobutton_kg.Visible = show;
			extra_window_jumps_rj_radiobutton_weight.Visible = show;
		} else {
			extra_window_jumps_label_weight.Visible = show;
			extra_window_jumps_spinbutton_weight.Visible = show;
			extra_window_jumps_radiobutton_kg.Visible = show;
			extra_window_jumps_radiobutton_weight.Visible = show;
		}
	}
	
	private void extra_window_showTechniqueArmsData (bool show, bool sensitive) {
		extra_window_jumps_label_dj_arms.Visible = show;
		extra_window_jumps_check_dj_arms.Visible = show;
		
		extra_window_jumps_label_dj_arms.Sensitive = sensitive;
		extra_window_jumps_check_dj_arms.Sensitive = sensitive;
	}
	
	private void extra_window_showFallData (JumpType myJumpType, bool show) {
		if(myJumpType.IsRepetitive) {
			extra_window_jumps_rj_label_fall.Visible = show;
			extra_window_jumps_rj_spinbutton_fall.Visible = show;
			extra_window_jumps_rj_label_cm.Visible = show;
		
			//only on simple jumps	
			extra_window_jumps_vbox_fall.Visible = false;
		} else 
			extra_window_jumps_vbox_fall.Visible = show;
	}
	
	private void extra_window_showLimitData (bool show) {
		extra_window_jumps_rj_label_limit.Visible = show;
		extra_window_jumps_rj_spinbutton_limit.Visible = show;
		extra_window_jumps_rj_label_limit_units.Visible = show;
	}
	
	private void extra_window_showSingleLegStuff(bool show) {
		vbox_extra_window_jumps_single_leg.Visible = show;
	}
			
	private void extra_window_jumps_showNoOptions(JumpType myJumpType, bool hasOptions) {
		if(myJumpType.IsRepetitive) 
			extra_window_label_jumps_rj_no_options.Visible = ! hasOptions;
		else 
			extra_window_label_jumps_no_options.Visible = ! hasOptions;
	}



	private void on_extra_window_jumps_radiobutton_kg_toggled (object o, EventArgs args)
	{
		extra_window_jumps_option = "Kg";
	}
	
	private void on_extra_window_jumps_radiobutton_weight_toggled (object o, EventArgs args)
	{
		extra_window_jumps_option = "%";
	}
	
	private void on_extra_window_jumps_rj_radiobutton_kg_toggled (object o, EventArgs args)
	{
		extra_window_jumps_rj_option = "Kg";
	}
	
	private void on_extra_window_jumps_rj_radiobutton_weight_toggled (object o, EventArgs args)
	{
		extra_window_jumps_rj_option = "%";
	}
	
	
	private string limitString()
	{
		if(extra_window_jumps_rj_jumpsLimited) 
			return extra_window_jumps_rj_limited.ToString() + "J";
		else 
			return extra_window_jumps_rj_limited.ToString() + "T";
	}
	
	//do not translate this
	private string slCMJString()
	{
		string str = "";
		if(extra_window_jumps_radiobutton_single_leg_mode_vertical.Active) str = "Vertical";
		else if(extra_window_jumps_radiobutton_single_leg_mode_horizontal.Active) str = "Horizontal";
		else str = "Lateral";
		
		if(extra_window_jumps_radiobutton_single_leg_dominance_this_limb.Active) str += " This";
		else if(extra_window_jumps_radiobutton_single_leg_dominance_opposite.Active) str += " Opposite";
		else str += " Unknown"; //default since 1.4.8

		if(extra_window_jumps_radiobutton_single_leg_fall_this_limb.Active) str += " This";
		else if(extra_window_jumps_radiobutton_single_leg_fall_opposite.Active) str += " Opposite";
		else str += " Both"; //default since 1.4.8

		return str;
	}

	private void on_spin_single_leg_changed(object o, EventArgs args) {
		int distance = Convert.ToInt32(extra_window_jumps_spin_single_leg_distance.Value);
		extra_window_jumps_spin_single_leg_angle.Value =
			Util.CalculateJumpAngle(
					Convert.ToDouble(Util.GetHeightInCentimeters(currentJump.Tv.ToString())), 
					distance );
	}

	private void on_extra_window_jumps_button_single_leg_apply_clicked (object o, EventArgs args) {
		string description = slCMJString();
		int distance = Convert.ToInt32(extra_window_jumps_spin_single_leg_distance.Value);
		int angle = Convert.ToInt32(extra_window_jumps_spin_single_leg_angle.Value);
		currentJump.Description = description + 
			" " + distance.ToString() +
			" " + angle.ToString();
		
		SqliteJump.UpdateDescription(Constants.JumpTable, 
			currentJump.UniqueID, currentJump.Description);
		
		myTreeViewJumps.Update(currentJump);
		
		//sensitive slCMJ options 
		vbox_extra_window_jumps_single_leg_radios.Sensitive = true;
	}

}


//--------------------------------------------------------
//---------------- jumps_more widget ---------------------
//--------------------------------------------------------

public class JumpsMoreWindow : EventMoreWindow
{
	[Widget] Gtk.Window jumps_runs_more;
	static JumpsMoreWindow JumpsMoreWindowBox;
	private bool selectedStartIn;
	private bool selectedExtraWeight;

	public JumpsMoreWindow (Gtk.Window parent, bool testOrDelete) {
		Glade.XML gladeXML;
		gladeXML = Glade.XML.FromAssembly (Util.GetGladePath() + "chronojump.glade", "jumps_runs_more", null);
		gladeXML.Autoconnect(this);
		this.parent = parent;
		this.testOrDelete = testOrDelete;
		
		if(!testOrDelete)
			jumps_runs_more.Title = Catalog.GetString("Delete test type defined by user");
		
		//put an icon to window
		UtilGtk.IconWindow(jumps_runs_more);
		
		selectedEventType = EventType.Types.JUMP.ToString();
		
		//name, startIn, weight, description
		store = new TreeStore(typeof (string), typeof (string), typeof (string), typeof (string));

		initializeThings();
	}
	
	static public JumpsMoreWindow Show (Gtk.Window parent, bool testOrDelete)
	{
		if (JumpsMoreWindowBox == null) {
			JumpsMoreWindowBox = new JumpsMoreWindow (parent, testOrDelete);
		}
		JumpsMoreWindowBox.jumps_runs_more.Show ();
		
		return JumpsMoreWindowBox;
	}
	
	protected override void createTreeView (Gtk.TreeView tv) {
		tv.HeadersVisible=true;
		int count = 0;
		
		tv.AppendColumn ( Catalog.GetString ("Name"), new CellRendererText(), "text", count++);
		tv.AppendColumn ( Catalog.GetString ("Start inside"), new CellRendererText(), "text", count++);
		tv.AppendColumn ( Catalog.GetString ("Extra weight"), new CellRendererText(), "text", count++);
		tv.AppendColumn ( Catalog.GetString ("Description"), new CellRendererText(), "text", count++);
	}
	
	protected override void fillTreeView (Gtk.TreeView tv, TreeStore store) 
	{
		//select data without inserting an "all jumps", without filter, and not obtain only name of jump
		string [] myJumpTypes = SqliteJumpType.SelectJumpTypes(false, "", "", false);

		//remove typesTranslated
		typesTranslated = new String [myJumpTypes.Length];
		int count = 0;

		foreach (string myType in myJumpTypes) {
			string [] myStringFull = myType.Split(new char[] {':'});
			if(myStringFull[2] == "1") {
				myStringFull[2] = Catalog.GetString("Yes");
			} else {
				myStringFull[2] = Catalog.GetString("No");
			}
			if(myStringFull[3] == "1") {
				myStringFull[3] = Catalog.GetString("Yes");
			} else {
				myStringFull[3] = Catalog.GetString("No");
			}

			JumpType tempType = new JumpType (myStringFull[1]);
			string description  = getDescriptionLocalised(tempType, myStringFull[4]);

			//if we are going to execute: show all types
			//if we are going to delete: show user defined types
			if(testOrDelete || ! tempType.IsPredefined)
				store.AppendValues (
						//myStringFull[0], //don't display de uniqueID
						Catalog.GetString(myStringFull[1]),	//name (translated)
						myStringFull[2], 	//startIn
						myStringFull[3], 	//weight
						description
						);

			//create typesTranslated
			typesTranslated [count++] = myStringFull[1] + ":" + Catalog.GetString(myStringFull[1]);
		}	
	}

	protected override void onSelectionEntry (object o, EventArgs args) {
		TreeModel model;
		TreeIter iter;
		selectedEventName = "-1";
		selectedStartIn = false;
		selectedExtraWeight = false;
		selectedDescription = "";

		if (((TreeSelection)o).GetSelected(out model, out iter)) {
			string translatedName = (string) model.GetValue (iter, 0);
			//get name in english
			selectedEventName = Util.FindOnArray(':', 1, 0, translatedName, typesTranslated);
			
			if( (string) model.GetValue (iter, 1) == Catalog.GetString("Yes") ) {
				selectedStartIn = true;
			}
			if( (string) model.GetValue (iter, 2) == Catalog.GetString("Yes") ) {
				selectedExtraWeight = true;
			}
			selectedDescription = (string) model.GetValue (iter, 3);

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
		TreeModel model;
		TreeIter iter;

		if (tv.Selection.GetSelected (out model, out iter)) {
			string translatedName = (string) model.GetValue (iter, 0);
			//get name in english
			selectedEventName = Util.FindOnArray(':', 1, 0, translatedName, typesTranslated);
			
			if( (string) model.GetValue (iter, 1) == Catalog.GetString("Yes") ) {
				selectedStartIn = true;
			}
			if( (string) model.GetValue (iter, 2) == Catalog.GetString("Yes") ) {
				selectedExtraWeight = true;
			}
			selectedDescription = (string) model.GetValue (iter, 3);

			//activate on_button_accept_clicked()
			button_accept.Activate();
		}
	}
	
	protected override void deleteTestLine() {
		SqliteJumpType.Delete(Constants.JumpTypeTable, selectedEventName, false);
		
		//delete from typesTranslated
		string row = Util.FindOnArray(':',0, -1, selectedEventName, typesTranslated);
		LogB.Information("row " + row);
		typesTranslated = Util.DeleteString(typesTranslated, row);
	}

	protected override string [] findTestTypesInSessions() {
		return SqliteJump.SelectJumps(false, -1, -1, "", selectedEventName); 
	}
	
	void on_button_cancel_clicked (object o, EventArgs args)
	{
		JumpsMoreWindowBox.jumps_runs_more.Hide();
		JumpsMoreWindowBox = null;
	}
	
	void on_jumps_runs_more_delete_event (object o, DeleteEventArgs args)
	{
		//raise signal
		button_cancel.Click();

		//JumpsMoreWindowBox.jumps_runs_more.Hide();
		//JumpsMoreWindowBox = null;
	}
	
	void on_button_accept_clicked (object o, EventArgs args)
	{
		JumpsMoreWindowBox.jumps_runs_more.Hide();
	}
	
	//when a jump is done using jumpsMoreWindow, the accept doesn't destroy this instance, because 
	//later we need data from it.
	//This is used for destroying, then if a new jump type is added, it will be shown at first time clicking "more" button
	public void Destroy() {		
		JumpsMoreWindowBox = null;
	}
	
	public bool SelectedStartIn 
	{
		get {
			return selectedStartIn;
		}
	}
	
	public bool SelectedExtraWeight 
	{
		get {
			return selectedExtraWeight;
		}
	}
}

//--------------------------------------------------------
//---------------- jumps_rj_more widget ------------------
//--------------------------------------------------------

public class JumpsRjMoreWindow : EventMoreWindow 
{
	[Widget] Gtk.Window jumps_runs_more;
	static JumpsRjMoreWindow JumpsRjMoreWindowBox;
	
	private bool selectedStartIn;
	private bool selectedExtraWeight;
	private bool selectedLimited;
	private double selectedLimitedValue;
	private bool selectedUnlimited;
	
	public JumpsRjMoreWindow (Gtk.Window parent, bool testOrDelete) {
		//the glade window is the same as jumps_more
		Glade.XML gladeXML;
		gladeXML = Glade.XML.FromAssembly (Util.GetGladePath() + "chronojump.glade", "jumps_runs_more", "chronojump");
		gladeXML.Autoconnect(this);
		this.parent = parent;
		this.testOrDelete = testOrDelete;
		
		if(!testOrDelete)
			jumps_runs_more.Title = Catalog.GetString("Delete test type defined by user");
		
		//put an icon to window
		UtilGtk.IconWindow(jumps_runs_more);

		//if jumps_runs_more is opened to showing Rj jumpTypes make it wider
		jumps_runs_more.Resize(600,300);
		
		selectedEventType = EventType.Types.JUMP.ToString();

		//name, limited by, limited value, startIn, weight, description
		store = new TreeStore(typeof (string), typeof (string), typeof(string),
				typeof (string), typeof (string), typeof (string));
		
		initializeThings();
	}
	
	static public JumpsRjMoreWindow Show (Gtk.Window parent, bool testOrDelete)
	{
		if (JumpsRjMoreWindowBox == null) {
			JumpsRjMoreWindowBox = new JumpsRjMoreWindow (parent, testOrDelete);
		}
		JumpsRjMoreWindowBox.jumps_runs_more.Show ();
		
		return JumpsRjMoreWindowBox;
	}
	
	protected override void createTreeView (Gtk.TreeView tv) {
		tv.HeadersVisible=true;
		int count = 0;

		tv.AppendColumn ( Catalog.GetString ("Name"), new CellRendererText(), "text", count++);
		tv.AppendColumn ( Catalog.GetString ("Limited by"), new CellRendererText(), "text", count++);
		tv.AppendColumn ( Catalog.GetString ("Limited value"), new CellRendererText(), "text", count++);
		tv.AppendColumn ( Catalog.GetString ("Start inside"), new CellRendererText(), "text", count++);
		tv.AppendColumn ( Catalog.GetString ("Extra weight"), new CellRendererText(), "text", count++);
		tv.AppendColumn ( Catalog.GetString ("Description"), new CellRendererText(), "text", count++);
	}

	protected override void fillTreeView (Gtk.TreeView tv, TreeStore store) 
	{
		//select data without inserting an "all jumps", and not obtain only name of jump
		string [] myJumpTypes = SqliteJumpType.SelectJumpRjTypes("", false);

		//remove typesTranslated
		typesTranslated = new String [myJumpTypes.Length];
		int count = 0;

		foreach (string myType in myJumpTypes) {
			string [] myStringFull = myType.Split(new char[] {':'});
			if(myStringFull[2] == "1") {
				myStringFull[2] = Catalog.GetString("Yes");
			} else {
				myStringFull[2] = Catalog.GetString("No");
			}
			if(myStringFull[3] == "1") {
				myStringFull[3] = Catalog.GetString("Yes");
			} else {
				myStringFull[3] = Catalog.GetString("No");
			}
			//limited
			string myLimiter = "";
			string myLimiterValue = "";
			
			//check if it's unlimited
			if(myStringFull[5] == "-1") { //unlimited mark
				myLimiter= Catalog.GetString("Unlimited");
				myLimiterValue = "-";
			} else {
				myLimiter = Catalog.GetString("Jumps");
				if(myStringFull[4] == "0") {
					myLimiter = Catalog.GetString("Seconds");
				}
				myLimiterValue = "?";
				if(Convert.ToDouble(myStringFull[5]) > 0) {
					myLimiterValue = myStringFull[5];
				}
			}

			JumpType tempType = new JumpType (myStringFull[1]);
			string description  = getDescriptionLocalised(tempType, myStringFull[6]);

			//if we are going to execute: show all types
			//if we are going to delete: show user defined types
			if(testOrDelete || ! tempType.IsPredefined)
				store.AppendValues (
						//myStringFull[0], //don't display de uniqueID
						Catalog.GetString(myStringFull[1]),	//name (translated)
						myLimiter,		//jumps or seconds		
						myLimiterValue,		//? or exact value
						myStringFull[2], 	//startIn
						myStringFull[3], 	//weight
						description
						);

			//create typesTranslated
			typesTranslated [count++] = myStringFull[1] + ":" + Catalog.GetString(myStringFull[1]);
		}	
	}

	protected override void onSelectionEntry (object o, EventArgs args)
	{
		TreeModel model;
		TreeIter iter;
		selectedEventName = "-1";
		selectedStartIn = false;
		selectedExtraWeight = false;
		selectedLimited = false;
		selectedLimitedValue = 0;
		selectedUnlimited = false; //true if it's an unlimited reactive jump
		selectedDescription = "";

		if (((TreeSelection)o).GetSelected(out model, out iter)) {
			string translatedName = (string) model.GetValue (iter, 0);
			//get name in english
			selectedEventName = Util.FindOnArray(':', 1, 0, translatedName, typesTranslated);
			
			if( (string) model.GetValue (iter, 1) == Catalog.GetString("Unlimited") ) {
				selectedUnlimited = true;
				selectedLimited = true; //unlimited jumps will be limited by clicking on "finish"
							//and this will be always done by the user when
							//some jumps are done (not time like in runs)
			} 
			
			if( (string) model.GetValue (iter, 1) == Catalog.GetString("Jumps") ) {
				selectedLimited = true;
			}
			
			if( (string) model.GetValue (iter, 2) == "?")
				selectedLimitedValue = 0;
			else if( (string) model.GetValue (iter, 2) == "-") 
				selectedLimitedValue = -1.0;
			else 
				selectedLimitedValue = Convert.ToDouble( (string) model.GetValue (iter, 2) );

			if( (string) model.GetValue (iter, 3) == Catalog.GetString("Yes") ) {
				selectedStartIn = true;
			}
			if( (string) model.GetValue (iter, 4) == Catalog.GetString("Yes") ) {
				selectedExtraWeight = true;
			}
			selectedDescription = (string) model.GetValue (iter, 5);

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
		TreeModel model;
		TreeIter iter;

		if (tv.Selection.GetSelected (out model, out iter)) {
			string translatedName = (string) model.GetValue (iter, 0);
			//get name in english
			selectedEventName = Util.FindOnArray(':', 1, 0, translatedName, typesTranslated);
			
			if( (string) model.GetValue (iter, 1) == Catalog.GetString("Unlimited") ) {
				selectedUnlimited = true;
				selectedLimited = true; //unlimited jumps will be limited by clicking on "finish"
							//and this will be always done by the user when
							//some jumps are done (not time like in runs)
			} 
			
			if( (string) model.GetValue (iter, 1) == Catalog.GetString("Jumps") ) {
				selectedLimited = true;
			}
			
			if( (string) model.GetValue (iter, 2) == "?")
				selectedLimitedValue = 0;
			else if( (string) model.GetValue (iter, 2) == "-") 
				selectedLimitedValue = -1.0;
			else 
				selectedLimitedValue = Convert.ToDouble( (string) model.GetValue (iter, 2) );

			if( (string) model.GetValue (iter, 3) == Catalog.GetString("Yes") ) {
				selectedStartIn = true;
			}
			if( (string) model.GetValue (iter, 4) == Catalog.GetString("Yes") ) {
				selectedExtraWeight = true;
			}
			selectedDescription = (string) model.GetValue (iter, 5);

			//activate on_button_accept_clicked()
			button_accept.Activate();
		}
	}
	
	protected override void deleteTestLine() {
		SqliteJumpType.Delete(Constants.JumpRjTypeTable, selectedEventName, false);
		
		//delete from typesTranslated
		string row = Util.FindOnArray(':',0, -1, selectedEventName, typesTranslated);
		typesTranslated = Util.DeleteString(typesTranslated, row);
	}
	
	protected override string [] findTestTypesInSessions() {
		return SqliteJumpRj.SelectJumps(-1, -1, "", selectedEventName); 
	}
	
	void on_button_cancel_clicked (object o, EventArgs args)
	{
		JumpsRjMoreWindowBox.jumps_runs_more.Hide();
		JumpsRjMoreWindowBox = null;
	}
	
	void on_jumps_runs_more_delete_event (object o, DeleteEventArgs args)
	{
		//raise signal
		button_cancel.Click();

		//JumpsRjMoreWindowBox.jumps_runs_more.Hide();
		//JumpsRjMoreWindowBox = null;
	}
	
	void on_button_accept_clicked (object o, EventArgs args)
	{
		JumpsRjMoreWindowBox.jumps_runs_more.Hide();
	}

	//when a jump Rj is done using jumpsRjMoreWindow, the accept doesn't destroy this instance, because 
	//later we need data from it.
	//This is used for destroying, then if a new jump rj type is added, it will be shown at first time clicking "more" button
	public void Destroy() {		
		JumpsRjMoreWindowBox = null;
	}

	public bool SelectedLimited 
	{
		get { return selectedLimited; }
	}
	
	public double SelectedLimitedValue 
	{
		get { return selectedLimitedValue; }
	}
	
	public bool SelectedStartIn 
	{
		get { return selectedStartIn; }
	}
	
	public bool SelectedExtraWeight 
	{
		get { return selectedExtraWeight; }
	}
	
	public bool SelectedUnlimited 
	{
		get { return selectedUnlimited; }
	}
}
