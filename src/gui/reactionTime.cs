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
 * Copyright (C) 2004-2015   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
using Gtk;
using Glade;
using System.Text; //StringBuilder
using System.Collections; //ArrayList

using System.Threading;
using Mono.Unix;

public partial class ChronoJumpWindow 
{
	[Widget] Gtk.HBox hbox_animation_lights;
	[Widget] Gtk.HBox hbox_flicker_lights;
	[Widget] Gtk.HBox hbox_discriminative_lights;
	
	[Widget] Gtk.Label label_animation_lights_interval;
	[Widget] Gtk.Label label_flicker_lights_cycle;
	[Widget] Gtk.Label label_flicker_lights_frequency;

	[Widget] Gtk.Label label_extra_window_radio_reaction_time;
	[Widget] Gtk.RadioButton extra_window_radio_reaction_time;
	[Widget] Gtk.Label label_extra_window_radio_reaction_time_animation_lights;
	[Widget] Gtk.RadioButton extra_window_radio_reaction_time_animation_lights;
	[Widget] Gtk.Label label_extra_window_radio_reaction_time_flicker;
	[Widget] Gtk.RadioButton extra_window_radio_reaction_time_flicker;
	[Widget] Gtk.Label label_extra_window_radio_reaction_time_discriminative;
	[Widget] Gtk.RadioButton extra_window_radio_reaction_time_discriminative;

	
	private void on_extra_window_reaction_times_test_changed(object o, EventArgs args)
	{
		hbox_animation_lights.Visible = false;
		hbox_flicker_lights.Visible = false;
		hbox_discriminative_lights.Visible = false;

		if(extra_window_radio_reaction_time.Active) {
			currentReactionTimeType = new ReactionTimeType("reactionTime");
		} else {
			if(extra_window_radio_reaction_time_animation_lights.Active)
				hbox_animation_lights.Visible = true;
			else if(extra_window_radio_reaction_time_flicker.Active)
				hbox_flicker_lights.Visible = true;
			else if(extra_window_radio_reaction_time_discriminative.Active)
				hbox_discriminative_lights.Visible = true;

		}

		currentEventType = currentReactionTimeType;
	}

	// ---- animation lights

	private void on_spinbutton_animation_lights_speed_value_changed (object o, EventArgs args) {
		switch(Convert.ToInt32(spinbutton_animation_lights_speed.Value)) {
			case 7:
				label_animation_lights_interval.Text = "2 s";
				break;
			case 6:
				label_animation_lights_interval.Text = "1 s";
				break;
			case 5:
				label_animation_lights_interval.Text = "500 ms";
				break;
			case 4:
				label_animation_lights_interval.Text = "250 ms";
				break;
			case 3:
				label_animation_lights_interval.Text = "125 ms";
				break;
			case 2:
				label_animation_lights_interval.Text = "62.5 ms";
				break;
			case 1:
				label_animation_lights_interval.Text = "31.25 ms";
				break;
			case 0:
				label_animation_lights_interval.Text = "15.625 ms";
				break;
		}
	}
	
	private void on_button_animation_lights_help_clicked (object o, EventArgs args) {
	}

	// ---- flicker
	
	private void on_spinbutton_flicker_lights_speed_value_changed (object o, EventArgs args) {
		switch(Convert.ToInt32(spinbutton_flicker_lights_speed.Value)) {
			case 7:
				label_flicker_lights_cycle.Text = "800 ms";
				label_flicker_lights_frequency.Text = "1.25 Hz";
				break;
			case 6:
				label_flicker_lights_cycle.Text = "400 ms";
				label_flicker_lights_frequency.Text = "2.5 Hz";
				break;
			case 5:
				label_flicker_lights_cycle.Text = "200 ms";
				label_flicker_lights_frequency.Text = "5 Hz";
				break;
			case 4:
				label_flicker_lights_cycle.Text = "100 ms";
				label_flicker_lights_frequency.Text = "10 Hz";
				break;
			case 3:
				label_flicker_lights_cycle.Text = "50 ms";
				label_flicker_lights_frequency.Text = "20 Hz";
				break;
			case 2:
				label_flicker_lights_cycle.Text = "25 ms";
				label_flicker_lights_frequency.Text = "40 Hz";
				break;
			case 1:
				label_flicker_lights_cycle.Text = "12.5 ms";
				label_flicker_lights_frequency.Text = "80 Hz";
				break;
			case 0:
				label_flicker_lights_cycle.Text = "6.25 ms";
				label_flicker_lights_frequency.Text = "160 Hz";
				break;
		}
	}

	private void on_button_flicker_lights_help_clicked (object o, EventArgs args) {
	}


	// ---- discriminative

	private void on_spinbutton_discriminative_lights_minimum_value_changed (object o, EventArgs args) {
		if(spinbutton_discriminative_lights_maximum.Value <= spinbutton_discriminative_lights_minimum.Value)
			spinbutton_discriminative_lights_maximum.Value = spinbutton_discriminative_lights_minimum.Value +1;
	}
	private void on_spinbutton_discriminative_lights_maximum_value_changed (object o, EventArgs args) {
		if(spinbutton_discriminative_lights_minimum.Value >= spinbutton_discriminative_lights_maximum.Value)
			spinbutton_discriminative_lights_minimum.Value = spinbutton_discriminative_lights_maximum.Value -1;
	}

	// ---- start buttons

	private void on_button_animation_lights_start_clicked (object o, EventArgs args) {
		int speed = Convert.ToInt32(spinbutton_animation_lights_speed.Value);
		ChronopicAuto cs = new ChronopicStartReactionTimeAnimation();
		cs.CharToSend = "l";
		cs.Write(chronopicWin.SP,speed);
	}
	
	private void on_button_flicker_lights_start_clicked (object o, EventArgs args) {
		int speed = Convert.ToInt32(spinbutton_flicker_lights_speed.Value);
		ChronopicAuto cs = new ChronopicStartReactionTimeAnimation();
		cs.CharToSend = "f";
		cs.Write(chronopicWin.SP,speed);
	}

	private void on_button_discriminative_lights_start_clicked (object o, EventArgs args) {
		//int speed = Convert.ToInt32(spinbutton_flicker_lights_speed.Value); //TODO
		ChronopicAuto cs = new ChronopicStartReactionTimeAnimation();
		if(radiobutton_reaction_time_disc_lr.Active == true)
			cs.CharToSend = "d";
		else if(radiobutton_reaction_time_disc_ly.Active == true)
			cs.CharToSend = "D";
		else if(radiobutton_reaction_time_disc_lg.Active == true)
			cs.CharToSend = "i";
		else if(radiobutton_reaction_time_disc_bz.Active == true)
			cs.CharToSend = "I";
		cs.Write(chronopicWin.SP,0); //TODO
	}

	//---- unused
	
	/*
	private void on_button_rt_3_on_clicked (object o, EventArgs args) {
		ChronopicAuto cs = new ChronopicStartReactionTime();
		cs.CharToSend = "r";
		cs.Write(chronopicWin.SP,0);
	}
	private void on_button_rt_3_off_clicked (object o, EventArgs args) {
		ChronopicAuto cs = new ChronopicStartReactionTime();
		cs.CharToSend = "R";
		cs.Write(chronopicWin.SP,0);
	}

	private void on_button_rt_6_on_clicked (object o, EventArgs args) {
		ChronopicAuto cs = new ChronopicStartReactionTime();
		cs.CharToSend = "s";
		cs.Write(chronopicWin.SP,0);
	}
	private void on_button_rt_6_off_clicked (object o, EventArgs args) {
		ChronopicAuto cs = new ChronopicStartReactionTime();
		cs.CharToSend = "S";
		cs.Write(chronopicWin.SP,0);
	}

	private void on_button_rt_7_on_clicked (object o, EventArgs args) {
		ChronopicAuto cs = new ChronopicStartReactionTime();
		cs.CharToSend = "t";
		cs.Write(chronopicWin.SP,0);
	}
	private void on_button_rt_7_off_clicked (object o, EventArgs args) {
		ChronopicAuto cs = new ChronopicStartReactionTime();
		cs.CharToSend = "T";
		cs.Write(chronopicWin.SP,0);
	}
	*/
	
}

//--------------------------------------------------------
//---------------- EDIT REACTION TIME WIDGET -------------
//--------------------------------------------------------

public class EditReactionTimeWindow : EditEventWindow
{
	static EditReactionTimeWindow EditReactionTimeWindowBox;

	EditReactionTimeWindow (Gtk.Window parent) {
		Glade.XML gladeXML;
		gladeXML = Glade.XML.FromAssembly (Util.GetGladePath() + "chronojump.glade", "edit_event", "chronojump");
		gladeXML.Autoconnect(this);
		this.parent = parent;
		
		//put an icon to window
		UtilGtk.IconWindow(edit_event);
	
		eventBigTypeString = Catalog.GetString("reaction time");
	}

	static new public EditReactionTimeWindow Show (Gtk.Window parent, Event myEvent, int pDN)
	{
		if (EditReactionTimeWindowBox == null) {
			EditReactionTimeWindowBox = new EditReactionTimeWindow (parent);
		}

		EditReactionTimeWindowBox.pDN = pDN;
		
		EditReactionTimeWindowBox.initializeValues();

		EditReactionTimeWindowBox.fillDialog (myEvent);

		//reaction time has no types
		EditReactionTimeWindowBox.label_type_title.Hide();
		EditReactionTimeWindowBox.hbox_combo_eventType.Hide();

		EditReactionTimeWindowBox.edit_event.Show ();

		return EditReactionTimeWindowBox;
	}
	
	protected override void initializeValues () {
		typeOfTest = Constants.TestTypes.RT;
		headerShowDecimal = false;
		showType = false;
		showRunStart = false;
		showTv = false;
		showTc= false;
		showFall = false;
		showDistance = false;
		showTime = true;
		showSpeed = false;
		showWeight = false;
		showLimited = false;
		showMistakes = false;
	}

	protected override string [] findTypes(Event myEvent) {
		//reaction time has no types
		string [] myTypes = new String[0];
		return myTypes;
	}
	
	protected override void fillTime(Event myEvent) {
		ReactionTime myRT = (ReactionTime) myEvent;
		entryTime = myRT.Time.ToString();
		
		//show all the decimals for not triming there in edit window using
		//(and having different values in formulae like GetHeightInCm ...)
		//entry_time_value.Text = Util.TrimDecimals(entryTime, pDN);
		entry_time_value.Text = entryTime;
	}
	
	protected override void updateEvent(int eventID, int personID, string description) {
		SqliteReactionTime.Update(eventID, "", entryTime, personID, description); //2nd is type
	}

	protected override void on_button_cancel_clicked (object o, EventArgs args)
	{
		EditReactionTimeWindowBox.edit_event.Hide();
		EditReactionTimeWindowBox = null;
	}
	
	protected override void on_delete_event (object o, DeleteEventArgs args)
	{
		EditReactionTimeWindowBox.edit_event.Hide();
		EditReactionTimeWindowBox = null;
	}
	
	protected override void hideWindow() {
		EditReactionTimeWindowBox.edit_event.Hide();
		EditReactionTimeWindowBox = null;
	}

}
