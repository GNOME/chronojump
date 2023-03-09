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
 * Copyright (C) 2004-2023   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
using Gtk;
//using Glade;
using System.Text; //StringBuilder

using Mono.Unix;

public partial class ChronoJumpWindow 
{
	Gtk.HBox hbox_animation_lights;
	Gtk.HBox hbox_flicker_lights;
	Gtk.VBox vbox_discriminative_lights;
	Gtk.Label label_animation_lights_interval;
	Gtk.Label label_flicker_lights_cycle;
	Gtk.Label label_flicker_lights_frequency;
	Gtk.RadioButton extra_window_radio_reaction_time;
	Gtk.RadioButton extra_window_radio_reaction_time_animation_lights;
	Gtk.RadioButton extra_window_radio_reaction_time_flicker;
	Gtk.RadioButton extra_window_radio_reaction_time_discriminative;
	Gtk.Label label_reaction_time_device_help;


	private void on_extra_window_reaction_times_test_changed(object o, EventArgs args)
	{
		hbox_animation_lights.Visible = false;
		hbox_flicker_lights.Visible = false;
		vbox_discriminative_lights.Visible = false;

		sensitiveLastTestButtons(false);

		currentReactionTimeType = new ReactionTimeType("reactionTime");
		changeTestImage("","", "reaction_time.png");
		setLabelContactsExerciseSelected(Catalog.GetString("Reaction time"));

		if(extra_window_radio_reaction_time_animation_lights.Active) {
			hbox_animation_lights.Visible = true;
			currentReactionTimeType = new ReactionTimeType("anticipation");
			changeTestImage("","", "reaction_time_discriminative.png");
			setLabelContactsExerciseSelected(Catalog.GetString("Animation lights"));
		}
		else if(extra_window_radio_reaction_time_flicker.Active) {
			hbox_flicker_lights.Visible = true;
			currentReactionTimeType = new ReactionTimeType("flickr");
			changeTestImage("","", "reaction_time_discriminative.png");
			setLabelContactsExerciseSelected(Catalog.GetString("Flicker"));
		}
		else if(extra_window_radio_reaction_time_discriminative.Active) {
			vbox_discriminative_lights.Visible = true;
			currentReactionTimeType = new ReactionTimeType("Discriminative");
			changeTestImage("","", "reaction_time_discriminative.png");
			setLabelContactsExerciseSelected(Catalog.GetString("Discriminative"));
		}

		label_reaction_time_device_help.Visible = (
				extra_window_radio_reaction_time_animation_lights.Active ||
				extra_window_radio_reaction_time_flicker.Active ||
				extra_window_radio_reaction_time_discriminative.Active );

		currentEventType = currentReactionTimeType;
	
		if(currentSession != null) {	
			treeview_reaction_times_storeReset();
			fillTreeView_reaction_times(currentReactionTimeType.Name);
		}
			
		updateGraphReactionTimes();
	}
	private void updateGraphReactionTimes () 
	{
		if(currentPerson == null || currentSession == null)
			return;

		//intializeVariables if not done before
		event_execute_initializeVariables(
			! cp2016.StoredCanCaptureContacts, //is simulated
			currentPerson.UniqueID, 
			currentPerson.Name, 
			Catalog.GetString("Phases"),  	  //name of the different moments
			Constants.ReactionTimeTable, //tableName
			currentReactionTimeType.Name 
			);

		PrepareEventGraphReactionTime eventGraph = new PrepareEventGraphReactionTime(
				1, //unused
			       	currentSession.UniqueID, currentPerson.UniqueID, Constants.ReactionTimeTable, currentReactionTimeType.Name);
		
		if(eventGraph.rtsAtSQL.Length > 0)
			PrepareReactionTimeGraph(eventGraph, false); //don't animate
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

	private void on_button_flicker_lights_start_clicked (object o, EventArgs args) {
		int speed = Convert.ToInt32(spinbutton_flicker_lights_speed.Value);
		ChronopicAuto cs = new ChronopicStartReactionTimeAnimation();
		cs.CharToSend = "f";
		cs.Write(cp2016.SP,speed);

		on_button_execute_test_clicked(o, args);
	}

	private string discriminativeCharToSend;
	private double discriminativeStartTime;
	private Random rnd;

	//private void on_button_discriminative_lights_start_clicked (object o, EventArgs args) 
	private void reaction_time_discriminative_lights_prepare () 
	{
		//TODO: check if nothing activated, Start should be unsensitive

		if(check_reaction_time_disc_buzzer.Active == true) //all this are with buzzer
		{
			if(check_reaction_time_disc_red.Active == true) {
				if(check_reaction_time_disc_yellow.Active == true) {
					if(check_reaction_time_disc_green.Active == true)
						discriminativeCharToSend = "X";		//all lights
					else
						discriminativeCharToSend = "U";		//red + yellow
				} else { //! yellow
					if(check_reaction_time_disc_green.Active == true)
						discriminativeCharToSend = "Y";		//red + green
					else
						discriminativeCharToSend = "R";		//red
				}
			} else {	// ! red
				if(check_reaction_time_disc_yellow.Active == true) {
					if(check_reaction_time_disc_green.Active == true)
						discriminativeCharToSend = "W";		//yellow + green
					else
						discriminativeCharToSend = "S";		//yellow
				} else {	// ! yellow
					if(check_reaction_time_disc_green.Active == true)
						discriminativeCharToSend = "T";			//green
					else
						discriminativeCharToSend = "Z";			//only buzzer
				}
			}
		} else {					//all this are without buzzer
			if(check_reaction_time_disc_red.Active == true) {
				if(check_reaction_time_disc_yellow.Active == true) {
					if(check_reaction_time_disc_green.Active == true)
						discriminativeCharToSend = "x";		//all lights
					else
						discriminativeCharToSend = "u";		//red + yellow
				} else { //! yellow
					if(check_reaction_time_disc_green.Active == true)
						discriminativeCharToSend = "y";		//red + green
					else
						discriminativeCharToSend = "r";		//red
				}
			} else {	// ! red
				if(check_reaction_time_disc_yellow.Active == true) {
					if(check_reaction_time_disc_green.Active == true)
						discriminativeCharToSend = "w";		//yellow + green
					else
						discriminativeCharToSend = "s";		//yellow
				} else // ! yellow
					discriminativeCharToSend = "t";			//green
			}
		}
	
		rnd = new Random();
		double rndDouble = rnd.NextDouble(); //double between 0 and 1
		int range = Convert.ToInt32(spinbutton_discriminative_lights_maximum.Value) - 
			Convert.ToInt32(spinbutton_discriminative_lights_minimum.Value);
		discriminativeStartTime = (rndDouble * range) + Convert.ToInt32(spinbutton_discriminative_lights_minimum.Value);

		LogB.Information("discriminativeStartTime");
		LogB.Information(discriminativeStartTime.ToString());
						
		LogB.Information("discriminativeCharToSend");
		LogB.Information(discriminativeCharToSend);
		
		//on_button_execute_test_clicked(o, args);
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
	
	private void connectWidgetsReactionTime (Gtk.Builder builder)
	{
		hbox_animation_lights = (Gtk.HBox) builder.GetObject ("hbox_animation_lights");
		hbox_flicker_lights = (Gtk.HBox) builder.GetObject ("hbox_flicker_lights");
		vbox_discriminative_lights = (Gtk.VBox) builder.GetObject ("vbox_discriminative_lights");
		label_animation_lights_interval = (Gtk.Label) builder.GetObject ("label_animation_lights_interval");
		label_flicker_lights_cycle = (Gtk.Label) builder.GetObject ("label_flicker_lights_cycle");
		label_flicker_lights_frequency = (Gtk.Label) builder.GetObject ("label_flicker_lights_frequency");
		extra_window_radio_reaction_time = (Gtk.RadioButton) builder.GetObject ("extra_window_radio_reaction_time");
		extra_window_radio_reaction_time_animation_lights = (Gtk.RadioButton) builder.GetObject ("extra_window_radio_reaction_time_animation_lights");
		extra_window_radio_reaction_time_flicker = (Gtk.RadioButton) builder.GetObject ("extra_window_radio_reaction_time_flicker");
		extra_window_radio_reaction_time_discriminative = (Gtk.RadioButton) builder.GetObject ("extra_window_radio_reaction_time_discriminative");
		label_reaction_time_device_help = (Gtk.Label) builder.GetObject ("label_reaction_time_device_help");
	}
}
