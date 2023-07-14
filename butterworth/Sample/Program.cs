#region License
/*
  Copyright © 2017 Joan Charmant 
  The MIT license.

  Permission is hereby granted, free of charge, to any person obtaining a copy
  of this software and associated documentation files (the "Software"), to deal
  in the Software without restriction, including without limitation the rights
  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
  copies of the Software, and to permit persons to whom the Software is
  furnished to do so, subject to the following conditions:

  The above copyright notice and this permission notice shall be included in all
  copies or substantial portions of the Software.

  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
  SOFTWARE.
*/
#endregion

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
//using System.Linq; //commented to be able to compile without Linq
using Kinovea.Filtering;

namespace Sample
{
  public class Program
  {
    public static void Main(string[] args)
    {
      // 1. Import data.
      string inputPath = "/home/xavier/informatica/progs_meus/chronojump/butterworth/Data/sample-signal.txt";
      List<float> values = ParseInput(inputPath);
      
      // 2. Convert values to a list of timed points. 
      // In this example, time is implicit in the input and interval between data point is given.
      // Kinovea's code is tailored for 2D values, since we are dealing with a 1D series we'll just set the Y component to 0 for all points.
      // Convert the relative displacements into absolute coordinates on the fly.
      long intervalMilliseconds = 1;
      long time = 0;
      float coord = 0;
      List<TimedPoint> samples = new List<TimedPoint>();
      foreach (float value in values)
      {
        coord += value;
        samples.Add(new TimedPoint(coord, 0, time));
        time += intervalMilliseconds;
      }

      // 3. Filter the data.
      // In Kinovea this also takes a calibration helper to convert positions and times between video space and real world space. 
      // Here we assume the data is already in real world space so the function was slightly simplified.
      double framerate = 1000.0 / intervalMilliseconds;
      FilteredTrajectory traj = new FilteredTrajectory();
      traj.Initialize(samples, framerate, -1);

      // 4. Compute linear kinematics on the data.
      // At this point `traj` contains a list of raw positions, a list of filtered positions at various cutoff frequencies, and the best-guess cutoff frequency.
      // The LinearKinematics class was simplified a lot compared to the Kinovea code to focus on 1D and only compute speed and acceleration.
      // A second pass of filtering is possible, this was for high speed video sources which tend to create extra noisy values at the digitization step, it shouldn't be needed here.
      bool enableSecondPassFiltering = false;
      LinearKinematics linearKinematics = new LinearKinematics();
      TimeSeriesCollection tsc = linearKinematics.BuildKinematics(traj, intervalMilliseconds, enableSecondPassFiltering);

      // 5. Export the result.
      // At this point the speed and acceleration are expressed in ms⁻¹ and ms⁻².
      string outputFilename = string.Format("{0}-output.csv", Path.GetFileNameWithoutExtension(inputPath));
      string outputPath = Path.Combine(Path.GetDirectoryName(inputPath), outputFilename);
      ExportCSV(outputPath, tsc);

      Console.WriteLine("Done. Press any key.");
      Console.ReadKey();
    }

    /// <summary>
    /// Import a time series into a list of floats.
    /// This function is specific to the input file format.
    /// </summary>
    private static List<float> ParseInput(string filename)
    {
      if (!File.Exists(filename))
      {
        Console.WriteLine("Input file not found.");
        return new List<float>();
      }

      string content = File.ReadAllText(filename);
      string[] strValues = content.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

      List<float> values = new List<float>();
      foreach (string strValue in strValues)
      {
        float value;
        bool parsed = float.TryParse(strValue, out value);
        if (parsed)
          values.Add(value);
        else
          Console.WriteLine(string.Format("There was an error while parsing the file. Value not parsed: {0}.", strValue));
      }

      return values;
    }

    /// <summary>
    /// Export the computed kinematics to a CSV file.
    /// </summary>
    private static void ExportCSV(string outputPath, TimeSeriesCollection tsc)
    {
      List<string> csv = new List<string>();
      string separator = CultureInfo.CurrentCulture.TextInfo.ListSeparator;

      // Header.
      List<string> headers = new List<string>() { "Time (ms)", "Raw coordinate (mm)", "Filtered coordinate (mm)", "Velocity (mm/ms)", "Acceleration (mm/ms²)" };
      csv.Add(string.Join(separator, headers.ToArray()));

      // Values.
      for (int i = 0; i < tsc.Length; i++)
      {
        List<double> p = new List<double>();
        p.Add(tsc.Times[i]);
        p.Add(tsc[Kinematics.XRaw][i]);
        p.Add(tsc[Kinematics.X][i]);
        p.Add(tsc[Kinematics.LinearHorizontalVelocity][i]);
        p.Add(tsc[Kinematics.LinearHorizontalAcceleration][i]);

	/* seems that this LinQ line just convert the NaN to ""
	 *
	Console.WriteLine ("before Linq");
	foreach (double pDebug in p)
		Console.WriteLine (pDebug.ToString ());

        string[] values = p.Select(v => double.IsNaN(v) ? "" : v.ToString()).ToArray();  //Linq is just used here in all the Kinovea butterworth code, so just do this non Linq
	Console.WriteLine ("after Linq");
	foreach (string vDebug in values)
		Console.WriteLine (vDebug.ToString ());
	*/

	// without Linq:
	string [] values = new String [p.Count];
	for (int j = 0; j < p.Count; j ++)
	{
		if (double.IsNaN (p[j]))
			values [j] = "";
		else
			values [j] = p[j].ToString ();
	}

        string line = string.Join(separator, values);
        csv.Add(line);
      }
      
      try 
      {
        File.WriteAllLines(outputPath, csv);
      }
      catch(Exception e)
      {
        Console.WriteLine(string.Format("An error occured while saving the file: {0}", e.Message));
      }
    }

    /// <summary>
    /// Sanity check method just to verify that the process works.
    /// Generates a list of relative coordinates with a constant acceleration.
    /// Can be used in replacement of ParseInput().
    /// </summary>
    private static List<float> GenerateValues(int count, float a)
    {
      List<float> values = new List<float>();
      float interval = 1;
      float time = 0;
      float p0 = 0;
      float v0 = 0;
      values.Add(0);

      List<float> coords = new List<float>();
      coords.Add(p0);

      for (int i = 1; i < count; i++)
      {
        time += interval;
        float t = time;
        float p = (v0 * t) + (0.5f * a * t * t);
        coords.Add(p);

        // Convert to relative value to emulate the input file.
        values.Add(p - coords[i - 1]);
      }

      return values;
    }
  }
}
