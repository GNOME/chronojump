using Linux.Bluetooth;
using Linux.Bluetooth.Extensions;
using System.Collections.Concurrent;

namespace BLEHelper.Linux
{
    internal class Program
    {
        static Adapter? adapter;
        static volatile int devicesCount;
        static ConcurrentDictionary<string, Device> devices = new();

        static void Main(string[] args)
        {
            _ = StartAsync();

            Console.Read();

            _ = StopAsync();
        }

        static async Task StartAsync()
        {
            if (null != adapter)
            {
                return;
            }

            var adapters = await BlueZManager.GetAdaptersAsync();
            if (null == adapters || adapters.Count == 0)
            {
                Console.WriteLine($"Adapter: None, Status: Disconnected");
                return;
            }

            adapter = adapters[0];
            Console.WriteLine($"Adapter: {await adapter.GetNameAsync()}, Status: Connected");

            adapter.PoweredOn += Adapter_PoweredOn;
            adapter.PoweredOff += Adapter_PoweredOff;
            adapter.DeviceFound += Adapter_DeviceFound;
            await adapter.StartDiscoveryAsync();
        }

        private static async Task Adapter_PoweredOff(Adapter sender, BlueZEventArgs eventArgs)
        {
            devicesCount = 0;
            devices.Clear();
        }

        private static async Task Adapter_PoweredOn(Adapter sender, BlueZEventArgs eventArgs)
        {
            if (null == adapter)
            {
                return;
            }
            await adapter.StopDiscoveryAsync();
        }

        private static async Task Adapter_DeviceFound(Adapter sender, DeviceFoundEventArgs eventArgs)
        {
            if (!eventArgs.IsStateChange)
            {
                return;
            }

            var deviceAddr = await sender.GetAddressAsync();
            if (devices.ContainsKey(deviceAddr))
            {
                return;
            }
            var deviceName = await sender.GetNameAsync();
            if (!deviceName.Contains("MI"))
            {
                return;
            }

            eventArgs.Device.Connected += Device_Connected;
            eventArgs.Device.Disconnected += Device_Disconnected;
            eventArgs.Device.ServicesResolved += Device_ServicesResolved;
            _ = eventArgs.Device.ConnectAsync();
        }

        private static async Task Device_ServicesResolved(Device sender, BlueZEventArgs eventArgs)
        {
            var services = await sender.GetServicesAsync();
            foreach (var service in services)
            {
                var characteristics = await service.GetCharacteristicsAsync();
                foreach (var characteristic in characteristics)
                {
                    var _characteristic = characteristic as GattCharacteristic;
                    if (null == _characteristic)
                    {
                        continue;
                    }
                    try
                    {
                        Console.WriteLine($"Device: {await sender.GetNameAsync()}, Status: ServicesResolved, Service: {service.GetUUIDAsync()}, Characteristic: {await _characteristic.GetUUIDAsync()}, Value: {await _characteristic.GetValueAsync()}");
                        _characteristic.Value += Characteristic_Value;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
        }

        private static async Task Characteristic_Value(GattCharacteristic sender, GattCharacteristicValueEventArgs eventArgs)
        {
            var service = await sender.GetServiceAsync();
            var deivce = await service.GetDeviceAsync();
            Console.WriteLine($"Device: {await deivce.GetNameAsync()}, Status: DataChanged, Service: {service.GetUUIDAsync()}, Characteristic: {await sender.GetUUIDAsync()}, Value: {eventArgs.Value}");
        }

        private static async Task Device_Disconnected(Device sender, BlueZEventArgs eventArgs)
        {
            sender.Connected -= Device_Connected;
            sender.Disconnected -= Device_Disconnected;
            sender.ServicesResolved -= Device_ServicesResolved;

            var services = await sender.GetServicesAsync();
            foreach (var service in services)
            {
                var characteristics = await service.GetCharacteristicsAsync();
                foreach (var characteristic in characteristics)
                {
                    var _characteristic = characteristic as GattCharacteristic;
                    if (null == _characteristic)
                    {
                        continue;
                    }
                    try
                    {
                        _characteristic.Value -= Characteristic_Value;
                    }
                    catch { }
                }
            }

            var deviceAddr = await sender.GetAddressAsync();
            devices.TryRemove(deviceAddr, out Device device);
            Interlocked.Decrement(ref devicesCount);
            Console.WriteLine($"Device: {await sender.GetNameAsync()}, Status: Disconnected");
        }

        private static async Task Device_Connected(Device sender, BlueZEventArgs eventArgs)
        {
            var deviceAddr = await sender.GetAddressAsync();
            if (devices.ContainsKey(deviceAddr))
            {
                return;
            }

            devices.TryAdd(deviceAddr, sender);
            Interlocked.Increment(ref devicesCount);
            Console.WriteLine($"Device: {await sender.GetNameAsync()}, Status: Connected");
        }

        static async Task StopAsync()
        {
            if (null == adapter)
            {
                return;
            }

            await adapter.StopDiscoveryAsync();
            adapter.PoweredOn -= Adapter_PoweredOn;
            adapter.PoweredOff -= Adapter_PoweredOff;
            adapter.DeviceFound -= Adapter_DeviceFound;
            foreach (var deviceAddr in devices.Keys)
            {
                await devices[deviceAddr].DisconnectAsync();
            }
            adapter = null;
        }
    }
}