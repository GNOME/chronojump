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
  /// <summary>
  /// A list of possible kinematics values. These are used as keys for the time series collections.
  /// </summary>
  public enum Kinematics
  {
    X,
    Y,
    XRaw,
    YRaw,

    LinearDistance,                 // Accumulated distance since the starting point.
    LinearHorizontalDisplacement,   // Straight line displacement from the current point to the starting point, horizontal component.
    LinearVerticalDisplacement,     // Straight line displacement from the current point to the starting point, vertical component.
    LinearSpeed,
    LinearHorizontalVelocity,
    LinearVerticalVelocity,
    LinearAcceleration,             // Instantaneous acceleration in the direction of the velocity vector.
    LinearHorizontalAcceleration,
    LinearVerticalAcceleration,

    AngularPosition,                // Absolute or relative value of the angle at this time.
    AngularDisplacement,            // Displacement with regards to previous point of measure.
    TotalAngularDisplacement,       // Total displacement with regards to first point of measure.

    AngularVelocity,
    TangentialVelocity,

    AngularAcceleration,
    TangentialAcceleration,
    CentripetalAcceleration,
    ResultantLinearAcceleration
  }
}
