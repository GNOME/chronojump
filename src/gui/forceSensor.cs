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
 * Copyright (C) 2017   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
using System.IO.Ports;
using System.Threading;
using Gtk;
using Glade;
using System.Text; //StringBuilder

public partial class ChronoJumpWindow 
{
	[Widget] Gtk.HBox hbox_combo_force_sensor_ports;
	[Widget] Gtk.ComboBox combo_force_sensor_ports;
	[Widget] Gtk.Label label_force_sensor_value;
	
	CjComboForceSensorPorts comboForceSensorPorts;

	Thread forceThread;
	static bool forceProcessFinish;
	static bool forceProcessCancel;

	private void on_button_force_sensor_ports_reload_clicked(object o, EventArgs args)
	{
		createComboForceSensorPorts(false);
	}

	private void createComboForceSensorPorts(bool create) 
	{
		if(comboForceSensorPorts == null)
			create = true;

		if(create)
		{
			//LogB.Information("CREATE");
			comboForceSensorPorts = new CjComboForceSensorPorts(combo_force_sensor_ports, hbox_combo_force_sensor_ports);
			combo_force_sensor_ports = comboForceSensorPorts.Combo;
			//combo_force_sensor_ports.Changed += new EventHandler (on_combo_force_sensor_ports_changed);
		} else {
			//LogB.Information("NO CREATE");
			comboForceSensorPorts.FillNoTranslate();
			combo_force_sensor_ports = comboForceSensorPorts.Combo;
		}
		combo_force_sensor_ports.Sensitive = true;
	}

	string forceSensorPortName;
	private void forceSensorCapture()
	{
		forceSensorPortName = UtilGtk.ComboGetActive(combo_force_sensor_ports);
		if(forceSensorPortName == null || forceSensorPortName == "")
		{
			new DialogMessage(Constants.MessageTypes.WARNING, "Please, select port");
			return;
		}

		forceProcessFinish = false;
		forceProcessCancel = false;
		
		event_execute_ButtonFinish.Clicked -= new EventHandler(on_finish_clicked);
		event_execute_ButtonFinish.Clicked += new EventHandler(on_finish_clicked);
		
		event_execute_ButtonCancel.Clicked -= new EventHandler(on_cancel_clicked);
		event_execute_ButtonCancel.Clicked += new EventHandler(on_cancel_clicked);

		forceThread = new Thread(new ThreadStart(forceSensorCaptureDo));
		GLib.Idle.Add (new GLib.IdleHandler (pulseGTKForceSensor));
	}

	//non GTK on this method
	private void forceSensorCaptureDo()
	{
		SerialPort port = new SerialPort(forceSensorPortName, 115200);
		port.Open();

		string str;
		while(! forceProcessFinish && ! forceProcessCancel)
		{
			str = port.ReadLine();
			LogB.Information("Readed: " + str);
			label_force_sensor_value.Text = str;
		}

		port.Close();
	}
	
	private bool pulseGTKForceSensor ()
	{
		if(! forceThread.IsAlive || forceProcessFinish || forceProcessCancel)
		{
			LogB.ThreadEnding(); 
			//finishPulsebar(...);
			LogB.ThreadEnded(); 
			return false;
		}
		/*
		updatePulsebar(...); //activity on pulsebar
		update graph or vscale
		*/

		Thread.Sleep (50);
		LogB.Information(" ForceSensor:"+ forceThread.ThreadState.ToString());
		return true;
	}
	
}

