using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace BLEHelper.Python.Invoker
{
    internal class Program
    {
        static void Main(string[] args)
        {
            BluetoothLE.OnDataChanged += BluetoothLE_OnDataChanged; //Subscribe BluetoothLE data changed event anywhere else that needs to read BluetoothLE data.
            BluetoothLE.Start();//Start BluetoothLE before the main window is created or anywhere else that needs BluetoothLE.

            //Console.WriteLine("Press any key to stop BluetoothLE and exit the application...");
            Console.Read();

            BluetoothLE.OnDataChanged -= BluetoothLE_OnDataChanged; //Unsubscribe BluetoothLE data changed event anywhere else that doesn't need to read BluetoothLE data any more.
            BluetoothLE.Stop();//Stop BluetoothLE after the main window is destroyed or anywhere else that doesn't need BluetoothLE any more.
        }

        /// <summary>
        /// BluetoothLE data changed event handler.
        /// </summary>
        /// <param name="sender">Always null</param>
        /// <param name="e">DataChangedEventArgs</param>
        private static void BluetoothLE_OnDataChanged(object sender, BluetoothLE.DataChangedEventArgs e)
        {
            Console.WriteLine(e.ServiceUUID);
            Console.WriteLine(e.CharacteristicUUID);
            Console.WriteLine(e.Value);
        }
    }
}