using System;
using System.IO.Ports;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;  //Stopwatch


class SerialPortReader
{
	private SerialPort _serialPort;
	private static bool exitProgram;

	public void StartReadingSync (string portName, int baudRate, string method)
	{
		try
		{
			_serialPort = new SerialPort(portName, baudRate)
			{
				Parity = Parity.None,
				       DataBits = 8,
				       StopBits = StopBits.One,
				       Handshake = Handshake.None,
				       ReadTimeout = 1000, // Timeout for sync operations
				       WriteTimeout = 1000
			};

			_serialPort.Open();

			Console.WriteLine($"Connected to {portName} at {baudRate} baud");

			ReadDataSync (method);

			// Wait for user input to exit
			//Console.ReadKey();

			// Cancel the reading task
			//_cancellationTokenSource.Cancel();

			// Wait for the reading task to complete
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Error: {ex.Message}");
		}
		finally
		{
			if (_serialPort?.IsOpen == true)
			{
				_serialPort.Close();
			}
			_serialPort?.Dispose();
			Console.WriteLine("Serial port closed");
		}
	}

	private void ReadDataSync (string method)
	{
		var buffer = new byte[1024];
		var stringBuilder = new StringBuilder();

		int totalBytes = 0;
		Stopwatch sw = new Stopwatch();
		sw.Start();

		try
		{
			while (sw.Elapsed.TotalMilliseconds < 3000)
			{
				int bytesRead;
				if (method == "1")
				{
					int byteReaded = _serialPort.ReadByte ();
					byteReaded = convertByte (byteReaded);
					Console.Write (string.Format ("{0} ", byteReaded));
					totalBytes ++;
				}
				else //if (method == "2" || method == "3")
				{
					if (method == "2")
						bytesRead = _serialPort.Read (buffer, 0, buffer.Length); //TODO implement cancellation token if needed
					else //if (method == "3")
						bytesRead = _serialPort.BaseStream.Read (buffer, 0, buffer.Length); //TODO implement cancellation token if needed

					if (bytesRead == 0)
						continue;

					Console.WriteLine (string.Format ("readed {0} bytes",  bytesRead));
					for (int i = 0; i < bytesRead; i ++)
					{
						int bInt = Convert.ToInt32 (buffer[i]);
						bInt = convertByte (bInt);
						Console.Write (string.Format ("{0} ", bInt));
						totalBytes ++;
					}
				}
			}
			Console.WriteLine (string.Format ("\n{0} bytes on {1} ms", totalBytes, sw.Elapsed.TotalMilliseconds));
			exitProgram = true;
		}
		catch (OperationCanceledException)
		{
			// Expected when cancelled
			Console.WriteLine("Reading operation cancelled");
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Error in read operation: {ex.Message}");
		}
		exitProgram = true;
	}

	private int convertByte(int b)
	{
		if(b > 128)
			b = b - 256;

		return b;
	}

	private static void methodSync (string method)
	{
		Console.WriteLine("Serial Port sync Reader Example");

		string portName = "/dev/ttyUSB0";
		int baudRate = 115200;

		var reader  = new SerialPortReader ();
		reader.StartReadingSync (portName, baudRate, method);
	}
	
	static void Main(string[] args)
	{
		if (args.Length == 0 || (args[0] != "1" && args [0] != "2" && args[0] != "3"))
		{
			Console.WriteLine ("Please execute with the option 1, 2, or 3." +
					"\n1 uses serialport.Read a byte each time" +
					"\n2 uses serialport.Read with buffer" +
					"\n3 uses serialPort.BaseStream.Read with buffer" +
					"\neg. dotnet run 1 SerialPortExample.csproj");
			Environment.Exit (0);
		}

		methodSync (args[0]);

		while (! exitProgram)
			;
	}
}
