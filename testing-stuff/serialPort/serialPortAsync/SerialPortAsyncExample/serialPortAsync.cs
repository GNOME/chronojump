using System;
using System.IO.Ports;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;  //Stopwatch


class SerialPortAsyncReader
{
	private SerialPort _serialPort;
	private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

	public async Task StartReadingAsync(string portName, int baudRate)
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
			Console.WriteLine("Press any key to exit...");

			var readTask = ReadDataAsync(_cancellationTokenSource.Token);

			Console.ReadKey();

			_cancellationTokenSource.Cancel();

			// Wait for the reading task to complete
			await readTask;
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

	private async Task ReadDataAsync(CancellationToken cancellationToken)
	{
		var buffer = new byte[1024];
		var stringBuilder = new StringBuilder();

		int totalBytes = 0;
		Stopwatch sw = new Stopwatch();
		sw.Start();

		try
		{
			while (!cancellationToken.IsCancellationRequested && sw.Elapsed.TotalMilliseconds < 1000)
			{
				// Read available bytes asynchronously
				int bytesRead = await _serialPort.BaseStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
				totalBytes += bytesRead;

				if (bytesRead == 0)
					continue;

				for (int i = 0; i < bytesRead; i ++)
				{
					int bInt = Convert.ToInt32 (buffer[i]);
					if (bInt > 128)
						bInt = bInt -256;

					Console.Write (string.Format ("{0} ", bInt));
				}
			}
			Console.WriteLine (string.Format ("\n{0} bytes on {1} ms", totalBytes, sw.Elapsed.TotalMilliseconds));
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
	}

	private int convertByte(int b)
	{
		if(b > 128)
			b = b - 256;

		return b;
	}

	static async Task Main(string[] args)
	{
		Console.WriteLine("Serial Port Async Reader Example");

		string portName = "/dev/ttyUSB0";
		int baudRate = 115200;

		var reader = new SerialPortAsyncReader();
		await reader.StartReadingAsync(portName, baudRate);
	}
}
