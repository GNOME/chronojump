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
	private bool bluetoothReading = false;
	//use the string to not have crash by manipulating the TextBuffer outside the pulse thread
	static string tbBluetoothText = "";
	static bool needToUpdateTextViewBluetooth;
	TextBuffer tbBluetooth = new TextBuffer (new TextTagTable());
	static Thread threadBluetooth;

	private static bool bluetoothHandlersAssigned; // to not have double feedback if opened again
	BluetoothDataList bd_l;
	BluetoothMessageList bm_l;

	// constructor
	// called at Chronojump start and only once
	public BluetoothCapture ()
	{
		bluetoothReading = false;

		if (! bluetoothHandlersAssigned)
		{
			BluetoothLE.OnInstalling += BluetoothLE_OnInstalling;
			BluetoothLE.OnBleakVersion += BluetoothLE_OnBleakVersion;
			BluetoothLE.OnScanning += BluetoothLE_OnScanning;
			BluetoothLE.OnDataChanged += BluetoothLE_OnDataChanged;
			BluetoothLE.OnDeviceChanged += BluetoothLE_OnDeviceChanged;
			BluetoothLE.OnError += BluetoothLE_OnError;
			bluetoothHandlersAssigned = true;
		}
	}

	public bool Start (string scriptURL, string mode, string val)
	{
		bluetoothReading = false;

		/*
		 * This can fail if used a managed file
		if(! File.Exists (entry_url.Text))
		{
			//tbBluetooth.Text = Catalog.GetString ("Error. File not found.");
			//textview.Buffer = tbBluetooth;

			return false;
		}
		*/

		bd_l = new BluetoothDataList ();
		bm_l = new BluetoothMessageList ();

		bluetoothReading = true;
		bluetoothDo (scriptURL, mode, val);

		return true;
	}

	public void Stop ()
	{
		if (bluetoothReading)
			bluetooth_stop ();
	}

	private void bluetoothDo (string scriptURL, string mode, string val)
	{
		//Start BluetoothLE service
		BluetoothLE.SetProcess (scriptURL);
		//BluetoothLE.Start ("CONNECT", "CP4");
		BluetoothLE.Start (mode, val);
	}

	private void bluetooth_stop ()
	{
		//Stop the BluetoothLE service if it was started
		BluetoothLE.Stop();
		bluetoothReading = false;
	}

	/// <summary>
	/// Handles the event triggered when the Bluetooth LE data changes.
	/// check above: bluetoothHandlersAssigned
	/// </summary>
	private void BluetoothLE_OnInstalling(object sender, BluetoothLE.InstallingEventArgs e)
	{
		bm_l.Add ($"\nInstalling: {e.Value}");
	}
	private void BluetoothLE_OnBleakVersion(object sender, BluetoothLE.BleakVersionEventArgs e)
	{
		bm_l.Add ($"\nBleak version: {e.Value}");
	}
	private void BluetoothLE_OnScanning(object sender)
	{
		bm_l.Add ($"\nStart scanning ...");
	}
	private void BluetoothLE_OnDataChanged(object sender, BluetoothLE.DataChangedEventArgs e)
	{
		if (e.CharacteristicName == BluetoothLE.BatteryName)
			bm_l.Add ($"\nBattery: {e.Value}");
		else
			bd_l.Add (new BluetoothData (e.CharacteristicName, e.Value));
	}
	private void BluetoothLE_OnDeviceChanged(object sender, BluetoothLE.DeviceEventArgs e)
	{
		bm_l.Add ($"\n{e.Action} {e.Ip} {e.Value}");
	}
	private void BluetoothLE_OnError(object sender, BluetoothLE.ErrorEventArgs e)
	{
		bm_l.Add ($"\n{e.Action} {e.Value}");
	}

	public bool BluetoothReading {
		get { return bluetoothReading; }
	}

	public BluetoothDataList Bd_l {
		get { return bd_l; }
	}
	public BluetoothMessageList Bm_l {
		get { return bm_l; }
	}
}

// this will have inheritance for all modes
public class BluetoothDataList
{
	List<BluetoothData> list;
	private int readedPos; //position already readed from list

	public BluetoothDataList ()
	{
		readedPos = 0; //note when nothing is readed (at start) is 0 (not -1)
		list = new List<BluetoothData>();
	}

	public void Add (BluetoothData bd)
	{
		list.Add (bd);
	}

	public bool CanReadFromList ()
	{
		// this produces crashes as an element can be assigned to the list and still be null because creation has not finished
		//return (list.Count > readedPos);

		// solution
		int count = list.Count;
		return (count > readedPos && list[count -1] != null);
	}

	public BluetoothData ReadNext ()
	{
		//try {
			return list[readedPos++];
		//} catch {
		//	return new BluetoothData ("-1", "-1");
		//}
	}

	// debug
	public override string ToString ()
	{
		string str = "";
		foreach (BluetoothData bd in list)
			str += "\n" + bd.ToString ();

		return str;
	}

	public int ReadedPos
	{
		get { return readedPos; }
	}
}

public class BluetoothData
{
	string charName; // charactaristic
	string val;

	public BluetoothData (string charName, string val)
	{
		this.charName = charName;
		this.val = val;
	}

	public FourPlatformsEvent ToFourPlatformsEvent ()
	{
		return new FourPlatformsEvent (this);
	}

	public override string ToString ()
	{
		return string.Format ("{0} {1}", charName, val);
	}
	
	public string CharName { get { return charName; } }
	public string Val { get { return val; } }
}

public class BluetoothMessageList
{
	//List<BluetoothMessage> list; //in the future maybe separate by the different messages
	List<string> list;
	private int readedPos; //position already readed from list

	public BluetoothMessageList ()
	{
		readedPos = 0; //note when nothing is readed (at start) is 0 (not -1)
		//list = new List<BluetoothMessage>();
		list = new List<string>();
	}

	//public void Add (BluetoothMessage bd)
	public void Add (string str)
	{
		//list.Add (bd);
		list.Add (str);
	}

	public bool CanReadFromList ()
	{
		return (list.Count > readedPos);
	}

	//public BluetoothMessage ReadNext ()
	public string ReadNext ()
	{
		try {
			return list[readedPos++];
		} catch {
			return "";
		}
	}

	// debug
	public override string ToString ()
	{
		string str = "";
		//foreach (BluetoothMessage bd in list)
		//	str += "\n" + bd.ToString ();
		foreach (string s in list)
			str += "\n" + s;

		return str;
	}

	public int ReadedPos
	{
		get { return readedPos; }
	}
}
