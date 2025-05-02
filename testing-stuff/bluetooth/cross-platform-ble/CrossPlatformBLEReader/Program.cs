using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Plugin.BLE;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;

namespace CrossPlatformBLEReader
{
    class Program
    {
        // Replace these with your BLE device's service and characteristic UUIDs
        private static readonly Guid ServiceUuid = Guid.Parse("0000180d-0000-1000-8000-00805f9b34fb"); // Example: Heart Rate Service
        private static readonly Guid CharacteristicUuid = Guid.Parse("00002a37-0000-1000-8000-00805f9b34fb"); // Example: Heart Rate Measurement

        private static IBluetoothLE _ble;
        private static IAdapter _adapter;
        private static IDevice _connectedDevice;
        private static ICharacteristic _characteristic;
        private static CancellationTokenSource _cancellationTokenSource;

        static async Task Main(string[] args)
        {
            Console.WriteLine("Cross-Platform BLE Device Reader - Starting...");

            try
            {
                _ble = CrossBluetoothLE.Current;
                _adapter = CrossBluetoothLE.Current.Adapter;
                _cancellationTokenSource = new CancellationTokenSource();

                Console.WriteLine($"Bluetooth State: {_ble.State}");
                _ble.StateChanged += OnBluetoothStateChanged;

                // Scan for devices
                Console.WriteLine("\nScanning for BLE devices...");
                _adapter.DeviceDiscovered += OnDeviceDiscovered;
                _adapter.ScanTimeoutElapsed += OnScanTimeoutElapsed;
                _adapter.ScanMode = ScanMode.LowLatency;

                var scanFilter = new ScanFilterOptions
                {
                    ServiceUuids = { ServiceUuid } // Optional: filter by service UUID
                };

                await _adapter.StartScanningForDevicesAsync(scanFilter, _cancellationTokenSource.Token);

                Console.WriteLine("Press any key to stop scanning...");
                Console.ReadKey();

                await _adapter.StopScanningForDevicesAsync();

                if (!_adapter.DiscoveredDevices.Any())
                {
                    Console.WriteLine("No BLE devices found. Please ensure your device is powered on and in range.");
                    return;
                }

                // Select the first device for simplicity (in a real app, let the user choose)
                var device = _adapter.DiscoveredDevices.FirstOrDefault(d => d.Name?.Contains("YourDeviceName") == true);
                if (device == null)
                {
                    Console.WriteLine("No matching device found.");
                    return;
                }

                Console.WriteLine($"\nSelected device: {device.Name ?? "Unknown"} ({device.Id})");

                // Connect to device
                var connectParameters = new ConnectParameters(
                    autoConnect: false,
                    forceBleTransport: false);

                Console.WriteLine("Connecting to device...");
                _connectedDevice = await _adapter.ConnectToDeviceAsync(device, connectParameters, _cancellationTokenSource.Token);

                if (!_connectedDevice.IsConnected)
                {
                    Console.WriteLine("Failed to connect to device.");
                    return;
                }

                Console.WriteLine($"Connected to {_connectedDevice.Name ?? "Unknown"}");

                // Get service
                Console.WriteLine($"Looking for service {ServiceUuid}...");
                var service = await _connectedDevice.GetServiceAsync(ServiceUuid, _cancellationTokenSource.Token);
                if (service == null)
                {
                    Console.WriteLine($"Service {ServiceUuid} not found.");
                    return;
                }

                Console.WriteLine($"Found service: {service.Id}");

                // Get characteristic
                Console.WriteLine($"Looking for characteristic {CharacteristicUuid}...");
                _characteristic = await service.GetCharacteristicAsync(CharacteristicUuid, _cancellationTokenSource.Token);
                if (_characteristic == null)
                {
                    Console.WriteLine($"Characteristic {CharacteristicUuid} not found.");
                    return;
                }

                Console.WriteLine($"Found characteristic: {_characteristic.Id}");

                // Subscribe to notifications
                _characteristic.ValueUpdated += OnCharacteristicValueUpdated;
                await _characteristic.StartUpdatesAsync(_cancellationTokenSource.Token);

                Console.WriteLine("Subscribed to notifications. Press any key to exit...");
                Console.ReadKey();

                // Clean up
                await _characteristic.StopUpdatesAsync(_cancellationTokenSource.Token);
                await _adapter.DisconnectDeviceAsync(_connectedDevice);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            finally
            {
                _cancellationTokenSource?.Cancel();
                _cancellationTokenSource?.Dispose();
                Console.WriteLine("Application exited.");
            }
        }

        private static void OnDeviceDiscovered(object sender, DeviceEventArgs e)
        {
            Console.WriteLine($"Discovered device: {e.Device.Name ?? "Unknown"} ({e.Device.Id}) - RSSI: {e.Device.Rssi}");
        }

        private static void OnScanTimeoutElapsed(object sender, EventArgs e)
        {
            Console.WriteLine("Scan timeout elapsed.");
        }

        private static void OnBluetoothStateChanged(object sender, BluetoothStateChangedArgs e)
        {
            Console.WriteLine($"Bluetooth state changed to {e.NewState}");
        }

        private static void OnCharacteristicValueUpdated(object sender, CharacteristicUpdatedEventArgs e)
        {
            var bytes = e.Characteristic.Value;
            if (bytes == null || bytes.Length == 0)
                return;

            Console.WriteLine($"Received data: {BitConverter.ToString(bytes)}");

            // Example for heart rate measurement:
            // byte flags = bytes[0];
            // if ((flags & 0x01) != 0) // HR is 16-bit
            // {
            //     ushort heartRate = (ushort)(bytes[1] + (bytes[2] << 8));
            //     Console.WriteLine($"Heart Rate: {heartRate} bpm");
            // }
            // else // HR is 8-bit
            // {
            //     byte heartRate = bytes[1];
            //     Console.WriteLine($"Heart Rate: {heartRate} bpm");
            // }
        }
    }
}
