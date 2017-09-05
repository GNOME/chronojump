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

namespace Kinovea.Filtering
{
  public static class MathHelper
  {
    public const double RadiansToDegrees = 180 / Math.PI;
    public const double DegreesToRadians = Math.PI / 180;
    public const double SQRT2 = 1.4142135623730950488;

    // Secant 
    public static double Sec(double x)
    {
      return 1 / Math.Cos(x);
    }

    // Cosecant
    public static double Cosec(double x)
    {
      return 1 / Math.Sin(x);
    }

    // Cotangent 
    public static double Cotan(double x)
    {
      return 1 / Math.Tan(x);
    }

    // Inverse Sine 
    public static double Arcsin(double x)
    {
      return Math.Atan(x / Math.Sqrt(-x * x + 1));
    }

    // Inverse Cosine 
    public static double Arccos(double x)
    {
      return Math.Atan(-x / Math.Sqrt(-x * x + 1)) + 2 * Math.Atan(1);
    }

    // Inverse Secant 
    public static double Arcsec(double x)
    {
      return 2 * Math.Atan(1) - Math.Atan(Math.Sign(x) / Math.Sqrt(x * x - 1));
    }

    // Inverse Cosecant 
    public static double Arccosec(double x)
    {
      return Math.Atan(Math.Sign(x) / Math.Sqrt(x * x - 1));
    }

    // Inverse Cotangent 
    public static double Arccotan(double x)
    {
      return 2 * Math.Atan(1) - Math.Atan(x);
    }

    // Hyperbolic Sine 
    public static double HSin(double x)
    {
      return (Math.Exp(x) - Math.Exp(-x)) / 2;
    }

    // Hyperbolic Cosine 
    public static double HCos(double x)
    {
      return (Math.Exp(x) + Math.Exp(-x)) / 2;
    }

    // Hyperbolic Tangent 
    public static double HTan(double x)
    {
      return (Math.Exp(x) - Math.Exp(-x)) / (Math.Exp(x) + Math.Exp(-x));
    }

    // Hyperbolic Secant 
    public static double HSec(double x)
    {
      return 2 / (Math.Exp(x) + Math.Exp(-x));
    }

    // Hyperbolic Cosecant 
    public static double HCosec(double x)
    {
      return 2 / (Math.Exp(x) - Math.Exp(-x));
    }

    // Hyperbolic Cotangent 
    public static double HCotan(double x)
    {
      return (Math.Exp(x) + Math.Exp(-x)) / (Math.Exp(x) - Math.Exp(-x));
    }

    // Inverse Hyperbolic Sine 
    public static double HArcsin(double x)
    {
      return Math.Log(x + Math.Sqrt(x * x + 1));
    }

    // Inverse Hyperbolic Cosine 
    public static double HArccos(double x)
    {
      return Math.Log(x + Math.Sqrt(x * x - 1));
    }

    // Inverse Hyperbolic Tangent 
    public static double HArctan(double x)
    {
      return Math.Log((1 + x) / (1 - x)) / 2;
    }

    // Inverse Hyperbolic Secant 
    public static double HArcsec(double x)
    {
      return Math.Log((Math.Sqrt(-x * x + 1) + 1) / x);
    }

    // Inverse Hyperbolic Cosecant 
    public static double HArccosec(double x)
    {
      return Math.Log((Math.Sign(x) * Math.Sqrt(x * x + 1) + 1) / x);
    }

    // Inverse Hyperbolic Cotangent 
    public static double HArccotan(double x)
    {
      return Math.Log((x + 1) / (x - 1)) / 2;
    }

    // Logarithm to base N 
    public static double LogN(double x, double n)
    {
      return Math.Log(x) / Math.Log(n);
    }
  }
}
