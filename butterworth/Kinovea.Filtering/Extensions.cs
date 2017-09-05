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
  public static class Extensions
  {
    public static List<double> Subtract(this List<double> a, List<double> b)
    {
      if (a.Count != b.Count)
        throw new ArgumentException("Lists must have the same number of elements");

      List<double> result = new List<double>();
      for (int i = 0; i < a.Count; i++)
        result.Add(a[i] - b[i]);

      return result;
    }

    public static double[] Subtract(this double[] a, double[] b)
    {
      if (a.Length != b.Length)
        throw new ArgumentException("Lists must have the same number of elements");

      double[] result = new double[a.Length];
      for (int i = 0; i < a.Length; i++)
        result[i] = a[i] - b[i];

      return result;
    }

  }
}