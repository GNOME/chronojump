//minimal example to write/read using R and not RDotNet
//compile:
//dmcs -r:/usr/lib/pkgconfig/../../lib/cli/glib-sharp-2.0/glib-sharp.dll -r:/usr/lib/pkgconfig/../../lib/cli/gtk-sharp-2.0/gtk-sharp.dll passToR.cs

//adapted from
//http://stackoverflow.com/questions/26053302/is-there-a-way-to-use-standard-input-output-stream-to-communicate-c-sharp-and-r/26058010#26058010

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Gtk;
using System.Threading;

namespace test1
{
	class Program
	{        
		static Thread thread;
		static Process proc;

		static void Main(string[] args)
		{
			Application.Init();

			Console.WriteLine("aaaa");
			proc = new Process();
			proc.StartInfo = new ProcessStartInfo("Rscript")
			{
				Arguments = "passToR.R",
					  RedirectStandardInput = true,
					  RedirectStandardOutput = true,
					  RedirectStandardError = true,
					  UseShellExecute = false
			};

			proc.Start();

			thread = new Thread(new ThreadStart(writeData));
			GLib.Idle.Add (new GLib.IdleHandler (pulseGTKReadData));
			thread.Start();
			
			Application.Run();
		
			proc.WaitForExit(); 
		}
	
		private static void writeData() {
			Console.WriteLine("writing line 1");
			proc.StandardInput.WriteLine("1, 2, 3, 4, 5, 6, 7");
			
			Thread.Sleep(500);
			
			Console.WriteLine("writing line 2");
			proc.StandardInput.WriteLine("8,9,10");
			
			Thread.Sleep(500);
			
			Console.WriteLine("writing line 3");
			proc.StandardInput.WriteLine("21,22,23,24");
			
			Thread.Sleep(500);
			
			Console.WriteLine("writing line 4 (the exit)");
			proc.StandardInput.WriteLine("Q");
			
			Thread.Sleep(500);
			
			Console.WriteLine("writing lines ended");
			Console.WriteLine("exiting from main thread");
			
			Application.Quit();
		}

		private static bool pulseGTKReadData ()
		{
			if(! thread.IsAlive) {
				Console.WriteLine("Exiting!");
				return false;
			}
			string str = proc.StandardOutput.ReadLine();
			if(str != null && str != "" && str != "\n")
				Console.WriteLine(str);

			Thread.Sleep (25);
//			Console.Write(" ReadData:" + thread.ThreadState.ToString() + " ");
			return true;
		}
	}
}
