//from
//https://www.c-sharpcorner.com/article/transferring-data-via-bluetooth-in-net/
//but it uses legacy (old): dotnet add package 32feet.NET

//fix below code to work with:
//https://www.nuget.org/packages/InTheHand.Net.Bluetooth
//new: dotnet add package InTheHand.Net.Bluetooth --version 4.1.40

using InTheHand.Net;
using InTheHand.Net.Bluetooth;
using InTheHand.Net.Sockets;

class Program
{
    static void Main(string[] args)
    {
        // Discover nearby Bluetooth devices

        BluetoothClient bluetoothClient = new BluetoothClient();
        BluetoothDeviceInfo[] devices = bluetoothClient.DiscoverDevices();

        Console.WriteLine("Discovered Bluetooth Devices:");
        foreach (BluetoothDeviceInfo device in devices)
        {
            Console.WriteLine($"Device Name: {device.DeviceName}");
            Console.WriteLine($"Device Address: {device.DeviceAddress}");
            Console.WriteLine($"Is Connected: {device.Connected}");
            Console.WriteLine();
        }

        //   output Device Address : 8803E9C06F34;
        //  Convert this to 88:03:E9:C0:6F:34

        BluetoothAddress deviceAddress = BluetoothAddress.Parse("88:03:E9:C0:6F:34");
        BluetoothEndPoint endPoint = new BluetoothEndPoint(deviceAddress, BluetoothService.SerialPort);
        BluetoothClient client = new BluetoothClient();

        try
        {
            client.Connect(endPoint);

            if (client.Connected)
            {
                Console.WriteLine("Connected to the Bluetooth device!");
                // Proceed to data transfer
            }
            else
            {
                Console.WriteLine("Connection failed.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error connecting to the device: {ex.Message}");
        }
    }
}
