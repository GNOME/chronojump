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
using System.Drawing;

namespace Kinovea.Filtering
{
  /// <summary>
  /// Build time series data for various linear kinematics values.
  /// Note: This class was simplified compared to Kinovea code, to focus on 1D and only compute speed and acceleration.
  /// </summary>
  public class LinearKinematics
  {
    public TimeSeriesCollection BuildKinematics(FilteredTrajectory traj, float intervalMilliseconds, bool enableSecondPassFiltering)
    {
      TimeSeriesCollection tsc = new TimeSeriesCollection(traj.Length);

      if (traj.Length == 0)
        return tsc;

      tsc.AddTimes(traj.Times);
      tsc.AddComponent(Kinematics.XRaw, traj.RawXs);
      tsc.AddComponent(Kinematics.X, traj.Xs);
      
      Func<int, PointF> getCoord;
      if (traj.CanFilter)
        getCoord = traj.Coordinates;
      else
        getCoord = traj.RawCoordinates;

      tsc.InitializeKinematicComponents(new List<Kinematics>(){
                Kinematics.LinearHorizontalVelocity,
                Kinematics.LinearHorizontalAcceleration,
            });

      ComputeVelocities(tsc, getCoord, intervalMilliseconds, enableSecondPassFiltering);
      ComputeAccelerations(tsc, getCoord, intervalMilliseconds, enableSecondPassFiltering);

      return tsc;
    }

    private void ComputeVelocities(TimeSeriesCollection tsc, Func<int, PointF> getCoord, float intervalMilliseconds, bool enableSecondPassFiltering)
    {
      if (tsc.Length <= 2)
      {
        TimeSeriesPadder.Pad(tsc[Kinematics.LinearHorizontalVelocity], 1);
        return;
      }

      for (int i = 1; i < tsc.Length - 1; i++)
      {
        PointF a = getCoord(i - 1);
        PointF b = getCoord(i + 1);
        float t = intervalMilliseconds * 2;
        tsc[Kinematics.LinearHorizontalVelocity][i] = (b.X - a.X) / t;
      }

      TimeSeriesPadder.Pad(tsc[Kinematics.LinearHorizontalVelocity], 1);

      // Second pass: apply extra smoothing to the derivatives.
      // This is only applied for high speed videos where the digitization is very noisy 
      // due to the combination of increased time resolution and decreased spatial resolution.
      if (enableSecondPassFiltering)
      {
        double constantVelocitySpan = 40;
        MovingAverage filter = new MovingAverage();
        double framerate = 1000 / intervalMilliseconds;
        double[] averagedHorizontalVelocity = filter.FilterSamples(tsc[Kinematics.LinearHorizontalVelocity], framerate, constantVelocitySpan, 1);
        
        for (int i = 0; i < tsc.Length; i++)
          tsc[Kinematics.LinearHorizontalVelocity][i] = averagedHorizontalVelocity[i];
      }
    }

    private void ComputeAccelerations(TimeSeriesCollection tsc, Func<int, PointF> getCoord, float intervalMilliseconds, bool enableSecondPassFiltering)
    {
      if (tsc.Length <= 4)
      {
        TimeSeriesPadder.Pad(tsc[Kinematics.LinearHorizontalAcceleration], 2);
        return;
      }

      // First pass: average speed over 2t centered on each data point.
      for (int i = 2; i < tsc.Length - 2; i++)
      {
        float t = intervalMilliseconds * 2;
        double horizontalAcceleration = (tsc[Kinematics.LinearHorizontalVelocity][i + 1] - tsc[Kinematics.LinearHorizontalVelocity][i - 1]) / t;
        tsc[Kinematics.LinearHorizontalAcceleration][i] = horizontalAcceleration;
      }

      TimeSeriesPadder.Pad(tsc[Kinematics.LinearHorizontalAcceleration], 2);

      // Second pass: extra smoothing derivatives.
      // This is only applied for high speed videos where the digitization is very noisy 
      // due to the combination of increased time resolution and decreased spatial resolution.
      if (enableSecondPassFiltering)
      {
        double constantAccelerationSpan = 50;
        MovingAverage filter = new MovingAverage();
        double framerate = 1000 / intervalMilliseconds;
        double[] averagedHorizontalAcceleration = filter.FilterSamples(tsc[Kinematics.LinearHorizontalAcceleration], framerate, constantAccelerationSpan, 2);

        for (int i = 0; i < tsc.Length; i++)
          tsc[Kinematics.LinearHorizontalAcceleration][i] = averagedHorizontalAcceleration[i];
      }
    }
  }
}
