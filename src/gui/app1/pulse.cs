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
	Gtk.RadioButton extra_window_radio_pulses_custom;
	Gtk.RadioButton extra_window_radio_pulses_free;
	Gtk.VBox vbox_extra_window_pulses;
	Gtk.SpinButton extra_window_pulses_spinbutton_pulse_step;
	Gtk.SpinButton extra_window_pulses_spinbutton_ppm;
	Gtk.SpinButton extra_window_pulses_spinbutton_total_pulses;
	Gtk.CheckButton extra_window_pulses_checkbutton_unlimited;
	Gtk.HBox extra_window_pulses_hbox_total_pulses;
	

	double extra_window_pulseStep = 1.000;
	int extra_window_totalPulses = 10;
	
	private void on_extra_window_pulses_test_changed(object o, EventArgs args)
	{
		if(extra_window_radio_pulses_free.Active) currentPulseType = new PulseType("Free");
		else if (extra_window_radio_pulses_custom.Active) currentPulseType = new PulseType("Custom");
		
		extra_window_pulses_initialize(currentPulseType);
	}

	private void extra_window_pulses_initialize(PulseType myPulseType) 
	{
		currentEventType = myPulseType;
		changeTestImage(EventType.Types.PULSE.ToString(), myPulseType.Name, myPulseType.ImageFileName);
		bool hasOptions = false;

		if(myPulseType.Name == "Custom") {
			hasOptions = true;
			extra_window_pulses_spinbutton_pulse_step.Value = extra_window_pulseStep;
			extra_window_pulses_spinbutton_total_pulses.Value = extra_window_totalPulses;
			setLabelContactsExerciseSelected(Catalog.GetString("Custom"));
		} else
			setLabelContactsExerciseSelected(Catalog.GetString("Free"));

		extra_window_pulses_showNoOptions(hasOptions);
	}
	
	private void extra_window_pulses_showNoOptions(bool hasOptions) {
		vbox_extra_window_pulses.Visible = hasOptions;
	}
	

	void on_extra_window_pulses_checkbutton_unlimited_clicked (object o, EventArgs args)
	{
		extra_window_pulses_hbox_total_pulses.Visible = ! extra_window_pulses_checkbutton_unlimited.Active;
	}

	void on_extra_window_pulses_spinbutton_pulse_step_changed (object o, EventArgs args)
	{
		if((double) extra_window_pulses_spinbutton_pulse_step.Value == 0) 
			extra_window_pulses_spinbutton_ppm.Value = 0;
		else 
			extra_window_pulses_spinbutton_ppm.Value = 60 / 
				(double) extra_window_pulses_spinbutton_pulse_step.Value;
	}

	void on_extra_window_pulses_spinbutton_ppm_changed (object o, EventArgs args)
	{
		if((int) extra_window_pulses_spinbutton_ppm.Value == 0)
			extra_window_pulses_spinbutton_pulse_step.Value = 0;
		else
			extra_window_pulses_spinbutton_pulse_step.Value = 60 / 
				(double) extra_window_pulses_spinbutton_ppm.Value;
	}

	private void connectWidgetsPulse (Gtk.Builder builder)
	{
		extra_window_radio_pulses_custom = (Gtk.RadioButton) builder.GetObject ("extra_window_radio_pulses_custom");
		extra_window_radio_pulses_free = (Gtk.RadioButton) builder.GetObject ("extra_window_radio_pulses_free");
		vbox_extra_window_pulses = (Gtk.VBox) builder.GetObject ("vbox_extra_window_pulses");
		extra_window_pulses_spinbutton_pulse_step = (Gtk.SpinButton) builder.GetObject ("extra_window_pulses_spinbutton_pulse_step");
		extra_window_pulses_spinbutton_ppm = (Gtk.SpinButton) builder.GetObject ("extra_window_pulses_spinbutton_ppm");
		extra_window_pulses_spinbutton_total_pulses = (Gtk.SpinButton) builder.GetObject ("extra_window_pulses_spinbutton_total_pulses");
		extra_window_pulses_checkbutton_unlimited = (Gtk.CheckButton) builder.GetObject ("extra_window_pulses_checkbutton_unlimited");
		extra_window_pulses_hbox_total_pulses = (Gtk.HBox) builder.GetObject ("extra_window_pulses_hbox_total_pulses");
	}	
}

