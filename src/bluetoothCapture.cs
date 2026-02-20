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
using System.IO;
using Gdk;
using Gtk;
using System.Text; //StringBuilder
//using System.Collections; //ArrayList
using System.Collections.Generic; //List<T>
using Mono.Unix;
//using System.Globalization; //CultureInfo stuff
//using System.Diagnostics;  //Stopwatch
using System.Text.RegularExpressions; //Regex

// TODO: adapted from gui/preferences
// when this is ok, use this code (even on preferences bluetooth test)

public class BluetoothCapture
{
	// passed GTK widgets
	Gtk.Entry entry_url;
	Gtk.Button button_start;
	Gtk.TextView textview;

	private bool bluetoothReading = false;
	//use the string to not have crash by manipulating the TextBuffer outside the pulse thread
	static string tbBluetoothText = "";
	static bool needToUpdateTextViewBluetooth;
	TextBuffer tbBluetooth = new TextBuffer (new TextTagTable());
	static Thread threadBluetooth;

	// constructor
	public BluetoothCapture (Gtk.Entry entry_url, Gtk.Button button_start, Gtk.TextView textview)
	{
		this.entry_url = entry_url;
		this.button_start = button_start;
		this.textview = textview;
	}

	public void Start ()
	{
		if(! File.Exists (entry_url.Text))
		{
			tbBluetooth.Text = Catalog.GetString ("Error. File not found.");
			textview.Buffer = tbBluetooth;
			bluetoothSensitiveDoing (false);

			return;
		}

		bluetoothSensitiveDoing (true);

		bluetoothReading = true;
		textview.Name = "fontSize9";
		tbBluetoothText = "";
		bluetooth_textview_update ("\nConnecting... ");

		threadBluetooth = new Thread (new ThreadStart (bluetoothDo));
		GLib.Idle.Add (new GLib.IdleHandler (pulseBluetooth));

		LogB.ThreadStart();
		threadBluetooth.Start();
	}

	public void End ()
	{
		if (bluetoothReading)
		{
			bluetooth_stop ();
			bluetoothSensitiveDoing (false);
		}
	}


	private void bluetoothSensitiveDoing (bool doing)
	{
		entry_url.Sensitive = ! doing;
		button_start.Sensitive = ! doing;
		//button_end.Sensitive = doing;
	}

	private void bluetoothDo ()
	{
		//Subscribe to BluetoothLE data changed, device changed events
		BluetoothLE.OnDataChanged -= BluetoothLE_OnDataChanged;
		BluetoothLE.OnDataChanged += BluetoothLE_OnDataChanged;
		BluetoothLE.OnDeviceChanged -= BluetoothLE_OnDeviceChanged;
		BluetoothLE.OnDeviceChanged += BluetoothLE_OnDeviceChanged;

		//Start BluetoothLE service
		BluetoothLE.SetProcess (entry_url.Text);
		BluetoothLE.Start ("SCAN", "CJ");
	}

	// by GTK thread
	private bool pulseBluetooth ()
	{
		if (needToUpdateTextViewBluetooth)
		{
			tbBluetooth.Text = tbBluetoothText;
			textview.Buffer = tbBluetooth;
			UtilGtk.TextViewScrollToEnd (textview);
			needToUpdateTextViewBluetooth = false;
		}
		if (! bluetoothReading)
			return false;

		//LogB.Debug (" pulseBluetooth:" + threadBluetooth.ThreadState.ToString());
		Thread.Sleep (50);
		return true;
	}

	private void bluetooth_stop ()
	{
		//Stop the BluetoothLE service if it was started
		BluetoothLE.Stop();
		bluetoothReading = false;
	}

	private void bluetooth_textview_update (string str)
	{
		tbBluetoothText += str;
		needToUpdateTextViewBluetooth = true;
	}

	/// <summary>
	/// Handles the event triggered when the Bluetooth LE data changes.
	/// </summary>
	/// <remarks>This method processes the updated data received from a Bluetooth LE device.  Use the <see cref="BluetoothLE.DataChangedEventArgs.Value"/> property of <paramref name="e"/>  to access the new data.</remarks>
	/// <param name="sender">The source of the event, typically the Bluetooth LE device.</param>
	/// <param name="e">The event data containing the updated value.</param>
	private void BluetoothLE_OnDataChanged(object sender, BluetoothLE.DataChangedEventArgs e)
	{
		bluetooth_textview_update ($"\n {e.CharacteristicUUID} {e.Value}");
	}
	private void BluetoothLE_OnDeviceChanged(object sender, BluetoothLE.DeviceEventArgs e)
	{
		bluetooth_textview_update ($"\n {e.Action} {e.Ip} {e.Value}");
	}
	
}
