#region License
/*
  Copyright © 2017 Joan Charmant 
  Copyright (C) 2023 Xavier de Blas <xaviblas@gmail.com>
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
using System.Drawing;

namespace Kinovea.Filtering
{
  /// <summary>
  /// Contains filtered and unfiltered data for one time series of 2D points and the corresponding cutoff frequencies.
  /// </summary>
  public class FilteredTrajectory
  {
    /// <summary>
    /// Number of samples.
    /// </summary>
    public int Length { get; private set; }

    /// <summary>
    /// Whether the trajectory can be filtered, depends on the number of samples.
    /// </summary>
    public bool CanFilter { get; private set; }

    /// <summary>
    /// Time coordinates.
    /// </summary>
    public long[] Times { get; private set; }

    /// <summary>
    /// Raw coordinates.
    /// </summary>
    public double[] RawXs { get; private set; }

    /// <summary>
    /// Raw coordinates.
    /// </summary>
    public double[] RawYs { get; private set; }

    /// <summary>
    /// Filtered coordinates at the currently selected cutoff frequency.
    /// </summary>
    public double[] Xs
    {
      get
      {
	return XCutoffIndex < 0 ? RawXs : FilterResultXs[XCutoffIndex].Data;
      }
    }

    /// <summary>
    /// Filtered coordinates at the currently selected cutoff frequency.
    /// </summary>
    public double[] Ys
    {
      get
      {
        return YCutoffIndex < 0 ? RawYs : FilterResultYs[YCutoffIndex].Data;
      }
    }

    /// <summary>
    /// Filtered X coordinates time series at various cutoff frequencies.
    /// </summary>
    public List<FilteringResult> FilterResultXs { get; set; }

    /// <summary>
    /// Best-guess cutoff frequency index for Xs series.
    /// </summary>
    public int XCutoffIndex { get; set; }

    /// <summary>
    /// Best-guess cutoff frequency for Xs series.
    /// </summary>
    public double XCutoff { get; set; }

    /// <summary>
    /// Filtered time series at various cutoff frequencies.
    /// </summary>
    public List<FilteringResult> FilterResultYs { get; set; }

    /// <summary>
    /// Best-guess cutoff frequency index for Ys series.
    /// </summary>
    public int YCutoffIndex { get; set; }

    //public Circle BestFitCircle { get; private set; }

    /// <summary>
    /// Initialize the data and filter it if possible.
    /// </summary>
    /// <param name="forceFreq">If this is > 0, then this freq is used.</param>
    public void Initialize(List<TimedPoint> samples, double captureFramesPerSecond, double forceFreq)
    {
      this.Length = samples.Count;

      Times = new long[samples.Count];
      RawXs = new double[samples.Count];
      RawYs = new double[samples.Count];

      XCutoffIndex = -1;
      YCutoffIndex = -1;

      // Raw coordinates.
      for (int i = 0; i < samples.Count; i++)
      {
        //PointF point = calibrationHelper.GetPointAtTime(samples[i].Point, samples[i].T);
        PointF point = samples[i].Point;

        RawXs[i] = point.X;
        RawYs[i] = point.Y;
        Times[i] = samples[i].T;
      }

      this.CanFilter = samples.Count > 10;
      if (this.CanFilter)
      {
        //double framerate = calibrationHelper.CaptureFramesPerSecond;
        double framerate = captureFramesPerSecond;

        ButterworthFilter filter = new ButterworthFilter();

        // Filter the results a hundred times at various cutoff frequency and store all data along with the best cutoff frequency.
        int tests = 100;
        int bestCutoffIndexX;
        double bestCutoff;
        FilterResultXs = filter.FilterSamples(RawXs, framerate, tests,
			out bestCutoffIndexX, out bestCutoff, forceFreq);
        XCutoffIndex = bestCutoffIndexX;
	XCutoff = bestCutoff;

	/*
	 * unneded Y (for Chronojump)
        int bestCutoffIndexY;
        FilterResultYs = filter.FilterSamples(RawYs, framerate, tests, out bestCutoffIndexY);
        YCutoffIndex = bestCutoffIndexY;
	*/
      }

      //BestFitCircle = CircleFitter.Fit(this);
    }

    public PointF RawCoordinates(int index)
    {
      return new PointF((float)RawXs[index], (float)RawYs[index]);
    }

    public PointF Coordinates(int index)
    {
      float x = XCutoffIndex < 0 ? (float)RawXs[index] : (float)FilterResultXs[XCutoffIndex].Data[index];
      float y = YCutoffIndex < 0 ? (float)RawYs[index] : (float)FilterResultYs[YCutoffIndex].Data[index];
      return new PointF(x, y);
    }
  }
}
