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
using System.IO;
using System.IO.Ports;
using System.Threading;
using Gtk;
using Glade;
using System.Text; //StringBuilder

public partial class ChronoJumpWindow 
{
	[Widget] Gtk.HBox hbox_combo_force_sensor_ports;
	[Widget] Gtk.ComboBox combo_force_sensor_ports;
	[Widget] Gtk.Label label_force_sensor_value_max;
	[Widget] Gtk.Label label_force_sensor_value;
	[Widget] Gtk.Label label_force_sensor_value_min;
	[Widget] Gtk.VScale vscale_force_sensor;
	
	CjComboForceSensorPorts comboForceSensorPorts;

	Thread forceThread;
	static bool forceProcessFinish;
	static bool forceProcessCancel;

	/*
	 * forceStatus:
	 * STOP is when is not used
	 * STARTING is while is waiting forceSensor to start capturing
	 * CAPTURING is when data is arriving
	 */
	enum forceStatus { STOP, STARTING, CAPTURING }
	static forceStatus capturingForce = forceStatus.STOP;

	static bool forceCaptureStartMark; 	//Just needed to display "Capturing message"
	static double forceSensorLast; 		//Needed to display value and move vscale

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

		button_execute_test.Sensitive = false;
		event_execute_button_finish.Sensitive = true;
		event_execute_button_cancel.Sensitive = true;
		event_execute_label_message.Text = "Please, wait ...";
		forceCaptureStartMark = false;
		vscale_force_sensor.Value = 0;
		label_force_sensor_value_max.Text = "0";
		label_force_sensor_value.Text = "0";
		label_force_sensor_value_min.Text = "0";
		notebook_capture_graph_table.CurrentPage = 1; //"Show table"

		capturingForce = forceStatus.STARTING;
		forceProcessFinish = false;
		forceProcessCancel = false;
		
		event_execute_ButtonFinish.Clicked -= new EventHandler(on_finish_clicked);
		event_execute_ButtonFinish.Clicked += new EventHandler(on_finish_clicked);
		
		event_execute_ButtonCancel.Clicked -= new EventHandler(on_cancel_clicked);
		event_execute_ButtonCancel.Clicked += new EventHandler(on_cancel_clicked);

		forceThread = new Thread(new ThreadStart(forceSensorCaptureDo));
		GLib.Idle.Add (new GLib.IdleHandler (pulseGTKForceSensor));

		LogB.ThreadStart();
		forceThread.Start();
	}

	//non GTK on this method
	private void forceSensorCaptureDo()
	{
		SerialPort port = new SerialPort(forceSensorPortName, 115200);
		port.Open();
		Thread.Sleep(2500); //sleep to let arduino start reading

		port.WriteLine("Start:-920.80:"); //Imp: note decimal is point
		string str = "";
		do {
			Thread.Sleep(100); //sleep to let arduino start reading
			str = port.ReadLine();
			LogB.Information("init string: " + str);
		}
		while(! str.StartsWith("StartedOk"));

		forceCaptureStartMark = true;
		capturingForce = forceStatus.CAPTURING;

		Util.CreateForceSensorSessionDirIfNeeded (currentSession.UniqueID);
		string fileName = Util.GetForceSensorSessionDir(currentSession.UniqueID) + Path.DirectorySeparatorChar +
			currentPerson.Name + "_" + UtilDate.ToFile(DateTime.Now) + ".csv";

		TextWriter writer = File.CreateText(fileName);
		writer.WriteLine("Time (s);Force(N)");
		str = "";
		while(! forceProcessFinish && ! forceProcessCancel)
		{
			str = port.ReadLine();
			string [] strFull = str.Split(new char[] {';'});
			//needed to store double in user locale
			double time = Convert.ToDouble(Util.ChangeDecimalSeparator(strFull[0]));
			double force = Convert.ToDouble(Util.ChangeDecimalSeparator(strFull[1]));

			writer.WriteLine(time.ToString() + ";" + force.ToString());
			forceSensorLast = force;
		}
		port.WriteLine("Stop");
		writer.Flush();
		writer.Close();
		((IDisposable)writer).Dispose();
		capturingForce = forceStatus.STOP;

		port.Close();

		if(forceProcessCancel)
			Util.FileDelete(fileName);
	}
	
	private bool pulseGTKForceSensor ()
	{
		if(! forceThread.IsAlive || forceProcessFinish || forceProcessCancel)
		{
			LogB.ThreadEnding();

			button_execute_test.Sensitive = true;
			if(forceProcessFinish)
				event_execute_label_message.Text = "Saved.";
			else if(forceProcessCancel)
				event_execute_label_message.Text = "Cancelled.";
			else
				event_execute_label_message.Text = "";

			LogB.ThreadEnded(); 

			return false;
		}

		if(forceCaptureStartMark)
		{
			event_execute_label_message.Text = "Capturing ...";
			forceCaptureStartMark = false;
		}

		if(capturingForce == forceStatus.CAPTURING)
		{
			//A) resize vscale if needed
			int upper = Convert.ToInt32(vscale_force_sensor.Adjustment.Upper);
			int lower = Convert.ToInt32(vscale_force_sensor.Adjustment.Lower);
			bool changed = false;

			if(forceSensorLast > upper)
			{
				upper = Convert.ToInt32(forceSensorLast * 2);
				changed = true;
			}
			if(forceSensorLast < lower)
			{
				lower = Convert.ToInt32(forceSensorLast * 2); //good for negative values
				changed = true;
			}
			if(changed)
				vscale_force_sensor.SetRange(lower, upper);

			//B) change the value
			vscale_force_sensor.Value = forceSensorLast;
			label_force_sensor_value.Text = forceSensorLast.ToString();
			if(forceSensorLast > Convert.ToDouble(label_force_sensor_value_max.Text))
				label_force_sensor_value_max.Text = forceSensorLast.ToString();
			if(forceSensorLast < Convert.ToDouble(label_force_sensor_value_min.Text))
				label_force_sensor_value_min.Text = forceSensorLast.ToString();
		}

		Thread.Sleep (25);
		LogB.Information(" ForceSensor:"+ forceThread.ThreadState.ToString());
		return true;
	}

	private void on_button_force_sensor_data_folder_clicked	(object o, EventArgs args)
	{
		System.IO.DirectoryInfo folderSession =
			new System.IO.DirectoryInfo(Util.GetForceSensorSessionDir(currentSession.UniqueID));
		System.IO.DirectoryInfo folderGeneric =
			new System.IO.DirectoryInfo(Util.GetForceSensorDir());

		if(folderSession.Exists)
			System.Diagnostics.Process.Start(Util.GetForceSensorSessionDir(currentSession.UniqueID));
		else if(folderGeneric.Exists)
			System.Diagnostics.Process.Start(Util.GetForceSensorDir());
		else
			new DialogMessage(Constants.MessageTypes.WARNING, Constants.DirectoryCannotOpen);
	}
}

