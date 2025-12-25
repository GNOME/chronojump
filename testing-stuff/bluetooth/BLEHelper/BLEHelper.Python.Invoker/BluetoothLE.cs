using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

/// <summary>
/// Read data from BLE (Bluetooth Low Energy) devices using Python scripts.
/// </summary>
public static class BluetoothLE
{
    /// <summary>
    /// DataChangedEventArgs is used to pass data when a characteristic changes.
    /// </summary>
    public class DataChangedEventArgs : EventArgs
    {
        /// <summary>
        /// The UUID of the service that has changed.
        /// </summary>
        public string ServiceUUID { get; set; }
        /// <summary>
        /// The UUID of the characteristic that has changed.
        /// </summary>
        public string CharacteristicUUID { get; set; }
        /// <summary>
        /// The value of the characteristic that has changed, decoded as a UTF-8 string or a space-separated HEX string.
        /// </summary>
        public string Value { get; set; }

        public DataChangedEventArgs(string serviceUUID, string characteristicUUID, string value)
        {
            ServiceUUID = serviceUUID;
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
    private static readonly Regex regexData = new Regex(@"^\[Data Changed\] Service=([\dA-Za-z\-]+), Characteristic=([\dA-Za-z\-]+), Value=(.+)$");

    /// <summary>
    /// CancellationTokenSource is used to cancel the operation of reading data from BLE devices.
    /// </summary>
    private static CancellationTokenSource cts;

    /// <summary>
    /// pythonPath is the path to the Python.
    /// </summary>
    private static readonly string pythonPath;

    /// <summary>
    /// pythonScriptPath is the path to the Python script that will be executed.
    /// </summary>
    private static readonly string pythonScriptPath;

    /// <summary>
    /// processStartInfo is used to configure the process that runs the Python script.
    /// </summary>
    private static readonly ProcessStartInfo processStartInfo;

    /// <summary>
    /// pythonProcess is the process that runs the Python script.
    /// </summary>
    private static Process pythonProcess;

    /// <summary>
    /// Set pythonFilePath and processStartInfo based on the operating system.
    /// </summary>
    static BluetoothLE()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            pythonPath = "python";
            pythonScriptPath = "../../../../BLEHelper.Python/ble-runner-win.bat";
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            pythonPath = "python3";
            pythonScriptPath = "../../../../BLEHelper.Python/bluepy-ble-runner-linux.sh";
        }
        else
        {
            pythonPath = "python3";
            pythonScriptPath = "../../../../BLEHelper.Python/ble-runner-mac.sh";
        }

        processStartInfo = new ProcessStartInfo
        {
            FileName = pythonScriptPath,
            Arguments = "",
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = false,
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
                processStartInfo.Arguments = pythonPath;
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
                            Console.WriteLine($"[BluetoothLE] {e.Data}");
                        }
                    };
                    pythonProcess.OutputDataReceived += (sender, e) =>
                    {
                        if (!string.IsNullOrEmpty(e.Data))
                        {
                            if (!e.Data.StartsWith("[Data Changed] "))
                            {
                                Console.WriteLine($"[BluetoothLE] {e.Data}");
                            }
                            var m = regexData.Match(e.Data);
                            if (m.Success)
                            {
                                OnDataChanged?.Invoke(null, new DataChangedEventArgs(
                                    m.Groups[1].Value,
                                    m.Groups[2].Value,
                                    m.Groups[3].Value));
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
                Console.WriteLine("[BluetoothLE] Failed to Start.\n{0}", ex.ToString());
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