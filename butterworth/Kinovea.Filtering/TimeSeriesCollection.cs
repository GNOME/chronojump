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

namespace Kinovea.Filtering
{
  /// <summary>
  /// Collects a number of time series for a given type of kinematics.
  /// Both linear and angular kinematics uses this with different entries in the dictionary.
  /// Times and data are kept separately.
  /// </summary>
  public class TimeSeriesCollection
  {
    /// <summary>
    /// Time series.
    /// </summary>
    public double[] this[Kinematics k]
    {
      get
      {
        if (!components.ContainsKey(k))
          throw new InvalidOperationException();

        return components[k];
      }
    }

    /// <summary>
    /// Number of samples.
    /// </summary>
    public int Length { get; private set; }

    /// <summary>
    /// Time coordinates.
    /// </summary>
    public long[] Times { get; private set; }

    private Dictionary<Kinematics, double[]> components = new Dictionary<Kinematics, double[]>();

    public TimeSeriesCollection(int length)
    {
      this.Length = length;
      Times = new long[length];
    }

    public void AddTimes(long[] times)
    {
      this.Times = times;
    }

    public void AddComponent(Kinematics key, double[] series)
    {
      if (!components.ContainsKey(key))
        components.Add(key, new double[Length]);

      components[key] = series;
    }

    public void InitializeKinematicComponents(List<Kinematics> list)
    {
      foreach (Kinematics key in list)
        components.Add(key, new double[Length]);
    }
  }
}
