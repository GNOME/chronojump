/*
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA 02111-1307, USA.
 *
 * Copyright (C) 2023  Xavier de Blas <xaviblas@gmail.com>
 */

using System;
using System.Collections.Generic; //List<T>
using System.Diagnostics;  //Stopwatch
using System.IO.Ports;
using System.Threading;
using System.Text.RegularExpressions; //Regex

/* this code was in my stash 14: Tue Oct 10 12:30:48 2023 */
/*
 * this code used btmon to capture.
 * it has never been used
 * use class BluetoothLE
 *
public class Bluetooth
{
       private Process bluetoothCaptureProcess;
       private string tmpLog = "/tmp/hcidump.log";

       public Bluetooth ()
       {
       }


       public void TestInit ()
       {
               //TODO: just capture and redirect stdout, processing to obtain the matches, no need to save to tmpLog
               LogB.Information ("Starting bluetooth capture");
               if (! testCapture ())
               {
                       LogB.Information ("cannot bluetooth capture");
                       return;
               }

               Stopwatch stopwatch = new Stopwatch();
               stopwatch.Start();
               while (stopwatch.Elapsed.TotalSeconds < 5)
                       System.Threading.Thread.Sleep(250);

               stopwatch.Stop();
               LogB.Information ("going to kill");
               bluetoothCaptureProcess.Kill ();

               System.Threading.Thread.Sleep(500);
               testRead ();
       }

       private bool testCapture ()
       {
               //btmon -i hci0 -w /tmp/hcidump.log
               List<string> parameters = new List<string>();
               parameters.Add ("-i");
               parameters.Add ("hci0");
               parameters.Add ("-w");

               parameters.Add (tmpLog);

               //Result result = run ("btmon", parameters, true, true);
               bluetoothCaptureProcess = new Process();
               bool success = ExecuteProcess.RunAtBackground (ref bluetoothCaptureProcess, "btmon", parameters, true, false, true, false, false);

               LogB.Information (string.Format ("btmon capture success = {0}", success));
               return success;
       }

       private void testRead ()
       {
               //btmon -r /tmp/hcidump.log | grep "Data:"
               List<string> parameters = new List<string>();
               parameters.Add ("-r");
               parameters.Add (tmpLog);

               ExecuteProcess.Result result = ExecuteProcess.run ("btmon", parameters, true, true);

               LogB.Information (string.Format ("btmon read success = {0}", result.success));
               //return result.success;

               LogB.Information ("result.stdout");
               //LogB.Information (result.stdout);

               foreach (Match match in Regex.Matches (result.stdout, @"Data: (\d+)"))
                       LogB.Information ("match: " + match.ToString ());
       }
}
*/

