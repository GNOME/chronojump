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
 * Copyright (C) 2025   Yang Dejiu <joeries.young@gmail.com>
 * Copyright (C) 2025-2026   Xavier de Blas <xaviblas@gmail.com>
 */

using System.Diagnostics;
using System.Text.RegularExpressions;

/// <summary>
/// Read data from BLE (Bluetooth Low Energy) devices using Python scripts.
/// </summary>
public static class BluetoothLE
{
    // ---- DatChanged ---->

    /// <summary>
    /// DataChangedEventArgs is used to pass data when a characteristic changes.
    /// </summary>
    public class DataChangedEventArgs : EventArgs
    {
        /// <summary>
        /// The UUID of the characteristic that has changed.
        /// </summary>
        public string CharacteristicUUID { get; set; }
        /// <summary>
        /// The name of the characteristic that has changed.
        /// </summary>
        public string CharacteristicName {
		get {
			if (CharacteristicUUID == "588dc235-7184-4550-9053-0e6a82f37cee")
				return "Light1";
			else if (CharacteristicUUID == "378b5d62-1fd3-4266-bbf7-6fec024d59a9")
				return "Light2";
			else if (CharacteristicUUID == "bde4d6e2-b970-42ff-b498-aeeca541ee07")
				return "Light3";
			else if (CharacteristicUUID == "e7331566-3aec-4a47-b8f1-d6f27850ad87")
				return "Light4";
			else if (CharacteristicUUID == "a2317307-e74a-4efe-b8ae-d615cd3be489")
				return "Battery";
			return "";
		}
	}
        /// <summary>
        /// The value of the characteristic that has changed, decoded as a UTF-8 string or a space-separated HEX string.
        /// </summary>
        public string Value { get; set; }

        public DataChangedEventArgs(string characteristicUUID, string value)
        {
            CharacteristicUUID = characteristicUUID;
            Value = value;
        }
    }

    /// <summary>
    /// Delegate for handling data change events.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public delegate void DataChangedHandler(object sender, DataChangedEventArgs e);

    /// <summary>
    /// Event that is raised when data changes in a BLE characteristic.
    /// </summary>
    public static event DataChangedHandler OnDataChanged;

    /// <summary>
    /// Regex to match the output from the Python script that indicates data has changed.
    /// Format:
    ///     Data Changed: {CharacteristicUUID} = {Value}
    ///     Value is decoded as a UTF-8 string or a space-separated HEX string.
    /// Example:
    ///     Data Changed: 85bc9e6c-9501-4bf4-819e-4f40b5e56372 = force: 83.66
    ///     Data Changed: 85bc9e6c-9501-4bf4-819e-4f40b5e56372 = A0 78 D5 90
    /// </summary>
    private static readonly Regex regexData = new Regex(@"^Data Changed: ([\dA-Za-z\-]+) = (.+)$");

    // <---- DataChanged ----

 
    // ---- Installing ---->
    public class InstallingEventArgs : EventArgs
    {
        public string Value { get; set; }

        public InstallingEventArgs(string value)
        {
            Value = value;
        }
    }
    public delegate void InstallingHandler(object sender, InstallingEventArgs e);
    public static event InstallingHandler OnInstalling;

    private static readonly Regex regexInstalling = new Regex(@"Installing (.*)");
    // <---- Installing ----

 
    // ---- BleakVersion ---->
    public class BleakVersionEventArgs : EventArgs
    {
        public string Value { get; set; }

        public BleakVersionEventArgs(string value)
        {
            Value = value;
        }
    }
    public delegate void BleakVersionHandler(object sender, BleakVersionEventArgs e);
    public static event BleakVersionHandler OnBleakVersion;

    private static readonly Regex regexBleakVersion = new Regex(@"^Version: (.*)");
    // <---- BleakVersion ----

 
    // ---- Scanning ---->
    public delegate void ScanningHandler(object sender);
    public static event ScanningHandler OnScanning;
    private static readonly Regex regexScanning = new Regex(@"scanning");
    // <---- Scanning ----

 
    // ---- DeviceEvent ---->
    
    public class DeviceEventArgs : EventArgs
    {
        public string Action { get; set; } //scanned or connected
        public string Ip { get; set; }
        public string Value { get; set; }

        public DeviceEventArgs(string action, string ip, string value)
        {
            Action = action;
	    Ip = ip;
            Value = value;
        }
    }
    public delegate void DeviceHandler(object sender, DeviceEventArgs e);
    public static event DeviceHandler OnDeviceChanged;
    //private static readonly Regex regexDeviceScanned = new Regex(@"^Device Scanned: (..:..:..:..:..:..:) .*local_name='(.*)', rssi.*$");
    private static readonly Regex regexDeviceScanned = new Regex(@"^Device Scanned: (..:..:..:..:..:..:) .*local_name='(.*)', service_uuids.*rssi.*$");
    private static readonly Regex regexDeviceConnected = new Regex(@"^Device Connected: (..:..:..:..:..:..:) (.*)$");
    
    // <---- DeviceEvent ----



    /// <summary>
    /// CancellationTokenSource is used to cancel the operation of reading data from BLE devices.
    /// </summary>
    private static CancellationTokenSource cts;

    /// <summary>
    /// pythonScriptPath is the path to the Python script that will be executed.
    /// </summary>
    //private static readonly string pythonScriptPath;

    /// <summary>
    /// processStartInfo is used to configure the process that runs the Python script.
    /// </summary>
    //private static readonly ProcessStartInfo processStartInfo;
    private static ProcessStartInfo processStartInfo;

    /// <summary>
    /// pythonProcess is the process that runs the Python script.
    /// </summary>
    private static Process pythonProcess;

    /// <summary>
    /// Set pythonFilePath and processStartInfo based on the operating system.
    /// </summary>
    static BluetoothLE()
    {
    }

    public static string GetScriptURL ()
    {
	    if (Util.operatingSystem == UtilAll.OperatingSystems.WINDOWS)
	    {
		    return "ble-runner-win.bat";
		    //return = @"C:\Users\xavi\chronojump\src\bin\Debug\net7.0\ble-runner-win.bat"; // hardcoded!
	    }
	    else if (Util.operatingSystem == UtilAll.OperatingSystems.MACOSX)
		    return "ble-runner-mac.sh";
	    else
		    return "ble-runner-linux.sh";
    }

    public static void SetProcess (string scriptURL)
    {
	    processStartInfo = new ProcessStartInfo
	    {
		    FileName = scriptURL,
			     Arguments = "",
			     RedirectStandardInput = true,
			     RedirectStandardOutput = true,
			     RedirectStandardError = true,
			     UseShellExecute = false,
			     CreateNoWindow = true,
			     StandardInputEncoding = System.Text.Encoding.UTF8,
			     StandardOutputEncoding = System.Text.Encoding.UTF8,
			     StandardErrorEncoding = System.Text.Encoding.UTF8
	    };
    }

    /// <summary>
    /// Start the Python script to read data from BLE devices.
    /// </summary>
    public static bool Start()
    {
        Stop();

        cts = new CancellationTokenSource();
        var task = System.Threading.Tasks.Task.Run(async () =>
        {
            try
            {
                processStartInfo.Arguments = Preferences.GetPythonExecutable(Preferences.pythonVersionEnum.Python3);
		LogB.Debug ("BluetoothLE Start FileName: " + processStartInfo.FileName);
		LogB.Debug ("BluetoothLE Arguments: " + processStartInfo.Arguments);
                using (pythonProcess = Process.Start(processStartInfo))
                {
                    if (null == pythonProcess)
                    {
                        return;
                    }
                    pythonProcess.EnableRaisingEvents = true;
                    pythonProcess.BeginErrorReadLine();
                    pythonProcess.BeginOutputReadLine();
                    pythonProcess.ErrorDataReceived += (sender, e) =>
                    {
                        if (!string.IsNullOrEmpty(e.Data))
                        {
                            LogB.Warning($"[BluetoothLE] error: {e.Data}");
                        }
                    };
                    pythonProcess.OutputDataReceived += (sender, e) =>
                    {
                        if (!string.IsNullOrEmpty(e.Data))
                        {
                            if (!e.Data.StartsWith("Device Scanned: ") &&
                                !e.Data.StartsWith("Device Connected: ") &&
                                !e.Data.StartsWith("Data Changed: "))
                            {
                                LogB.Information($"[BluetoothLE] output: {e.Data}");
                            }
                            
                            if (e.Data.Contains("Installing"))
			    {
				    LogB.Information ("Installing: "  + e.Data.ToString ());
				    var match = regexInstalling.Match(e.Data);
				    if (match.Success)
					    OnInstalling?.Invoke(null, new InstallingEventArgs(match.Groups[1].Value));
			    }
                            if (e.Data.StartsWith("Version: ")) // Bleak version
			    {
				    LogB.Information ("Bleak Version: "  + e.Data.ToString ());
				    var match = regexBleakVersion.Match(e.Data);
				    if (match.Success)
					    OnBleakVersion?.Invoke(null, new BleakVersionEventArgs(match.Groups[1].Value));
			    }
			    /*
				TODO:
			    //No powered Bluetooth adapters found
                            if (e.Data.Contains("POWERED_OFF") || //TODO other errors
			    {
				    LogB.Information ("Scanning: "  + e.Data.ToString ());
				    var match = regexScanning.Match(e.Data);
				    if (match.Success)
					    OnScanning?.Invoke(null, new ScanningEventArgs(match.Groups[1].Value));
			    }
			    */
                            if (e.Data.Contains("scanning"))
			    {
				    LogB.Information ("Scanning: "  + e.Data.ToString ());
				    var match = regexScanning.Match(e.Data);
				    if (match.Success)
					    OnScanning?.Invoke(null);
			    }
                            if (e.Data.StartsWith("Device Scanned: "))
			    {
				    LogB.Information ("scanned: "  + e.Data.ToString ());
                            	    var match = regexDeviceScanned.Match(e.Data);
				    if (match.Success)
					    OnDeviceChanged?.Invoke(null, new DeviceEventArgs(
								    "Scanned", match.Groups[1].Value, match.Groups[2].Value));		   
			    } if (e.Data.StartsWith("Device Connected: "))
				    {
				    LogB.Information ("Connected: "  + e.Data.ToString ());
                            	    var match = regexDeviceConnected.Match(e.Data);
				    if (match.Success)
					    OnDeviceChanged?.Invoke(null, new DeviceEventArgs(
								    "Connected", match.Groups[1].Value, match.Groups[2].Value));		   
			    }

                            var matchD = regexData.Match(e.Data);
                            if (matchD.Success)
                            {
                                OnDataChanged?.Invoke(null, new DataChangedEventArgs(
                                    matchD.Groups[1].Value,
                                    matchD.Groups[2].Value));
                                
				LogB.Information (string.Format ("[BluetoothLE] Characteristic: {0}, Data: {1}",
						matchD.Groups[1].Value, matchD.Groups[2].Value));
                            }
                        }
                    };
                    await pythonProcess.WaitForExitAsync(cts.Token);
                    pythonProcess.EnableRaisingEvents = false;
                    pythonProcess.CancelOutputRead();
                    pythonProcess.CancelErrorRead();
                }
            }
            catch (Exception ex)
            {
                LogB.Error($"[BluetoothLE] Catched!. Reason: " + ex.ToString());
                throw;
            }
        }, cts.Token);
        return !task.IsFaulted;
    }

    /// <summary>
    /// Stop the Python script that is reading data from BLE devices.
    /// </summary>
    public static void Stop()
    {
        if (null != cts)
        {
            cts.Cancel();
            cts = null;
        }

        if (pythonProcess != null)
        {
            pythonProcess.EnableRaisingEvents = false;
            try
            {
                if (!pythonProcess.HasExited)
                {
                    pythonProcess.CancelOutputRead();
                    pythonProcess.CancelErrorRead();
                    pythonProcess.Kill();
                }
            }
            catch { }
            pythonProcess.Dispose();
            pythonProcess = null;
        }
    }
}
