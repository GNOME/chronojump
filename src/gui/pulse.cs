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
 * Xavier de Blas: 
 * http://www.xdeblas.com, http://www.deporteyciencia.com (parleblas)
 */

using System;
using Gtk;
using Glade;
using System.Text; //StringBuilder
using System.Collections; //ArrayList

using System.Threading;


//--------------------------------------------------------
//---------------- pulse extra WIDGET --------------------
//--------------------------------------------------------

public class PulseExtraWindow 
{
	[Widget] Gtk.Window pulse_extra;
	[Widget] Gtk.SpinButton spinbutton_pulse_step;
	[Widget] Gtk.SpinButton spinbutton_ppm;
	[Widget] Gtk.SpinButton spinbutton_total_pulses;
	[Widget] Gtk.CheckButton checkbutton_unlimited;
	[Widget] Gtk.HBox hbox_total_pulses;
	[Widget] Gtk.Button button_accept;

	static double pulseStep = 1.000;
	static bool unlimited = true;
	static int totalPulses = 10;
	
	static PulseExtraWindow PulseExtraWindowBox;
	Gtk.Window parent;

	PulseExtraWindow (Gtk.Window parent) {
		Glade.XML gladeXML;
		try {
			gladeXML = Glade.XML.FromAssembly ("chronojump.glade", "pulse_extra", null);
		} catch {
			gladeXML = Glade.XML.FromAssembly ("chronojump.glade.chronojump.glade", "pulse_extra", null);
		}

		gladeXML.Autoconnect(this);
		this.parent = parent;
	}
	
	static public PulseExtraWindow Show (Gtk.Window parent, PulseType myPulseType) 
	{
		if (PulseExtraWindowBox == null) {
			PulseExtraWindowBox = new PulseExtraWindow (parent);
		}
		
		//put default values or values from previous pulse
		PulseExtraWindowBox.spinbutton_pulse_step.Value = pulseStep;
		if(totalPulses == -1)
			totalPulses = 10;
		
		PulseExtraWindowBox.spinbutton_total_pulses.Value = totalPulses;
		
		if(unlimited) 
			PulseExtraWindowBox.checkbutton_unlimited.Active = true;
		
		PulseExtraWindowBox.pulse_extra.Show ();

		return PulseExtraWindowBox;
	}
	
	
	void on_button_cancel_clicked (object o, EventArgs args)
	{
		PulseExtraWindowBox.pulse_extra.Hide();
		PulseExtraWindowBox = null;
	}
	
	void on_delete_event (object o, DeleteEventArgs args)
	{
		PulseExtraWindowBox.pulse_extra.Hide();
		PulseExtraWindowBox = null;
	}
	
	void on_button_accept_clicked (object o, EventArgs args)
	{
		pulseStep = (double) PulseExtraWindowBox.spinbutton_pulse_step.Value;
		Console.WriteLine("pulsestep: {0}", pulseStep);
		if(checkbutton_unlimited.Active) {
			totalPulses = -1;
			unlimited = true;
		}
		else {
			totalPulses = (int) PulseExtraWindowBox.spinbutton_total_pulses.Value;
			unlimited = false;
		}
		
		PulseExtraWindowBox.pulse_extra.Hide();
		PulseExtraWindowBox = null;
	}

	
	void on_checkbutton_unlimited_clicked (object o, EventArgs args)
	{
		if(checkbutton_unlimited.Active) {
			hbox_total_pulses.Hide();
		} else {
			hbox_total_pulses.Show();
		}
	}

	void on_spinbutton_pulse_step_changed (object o, EventArgs args)
	{
		if((double) PulseExtraWindowBox.spinbutton_pulse_step.Value == 0) 
			PulseExtraWindowBox.spinbutton_ppm.Value = 0;
		else 
			PulseExtraWindowBox.spinbutton_ppm.Value = 60 / 
				(double) PulseExtraWindowBox.spinbutton_pulse_step.Value;
	}

	void on_spinbutton_ppm_changed (object o, EventArgs args)
	{
		if((int) PulseExtraWindowBox.spinbutton_ppm.Value == 0)
			PulseExtraWindowBox.spinbutton_pulse_step.Value = 0;
		else
			PulseExtraWindowBox.spinbutton_pulse_step.Value = 60 / 
				(double) PulseExtraWindowBox.spinbutton_ppm.Value;
	}


		
	public Button Button_accept 
	{
		set { button_accept = value;	}
		get { return button_accept;	}
	}

	public double PulseStep
	{
		get { return pulseStep;	}
	}
	
	public int TotalPulses
	{
		get { return totalPulses;	}
	}
	
}


