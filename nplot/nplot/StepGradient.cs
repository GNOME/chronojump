/*
NPlot - A charting library for .NET

StepGradient.cs
Copyright (C) 2004
Matt Howlett

Redistribution and use of NPlot or parts there-of in source and
binary forms, with or without modification, are permitted provided
that the following conditions are met:

1. Re-distributions in source form must retain at the head of each
   source file the above copyright notice, this list of conditions
   and the following disclaimer.

2. Any product ("the product") that makes use NPlot or parts 
   there-of must either:
  
    (a) allow any user of the product to obtain a complete machine-
        readable copy of the corresponding source code for the 
        product and the version of NPlot used for a charge no more
        than your cost of physically performing source distribution,
	on a medium customarily used for software interchange, or:

    (b) reproduce the following text in the documentation, about 
        box or other materials intended to be read by human users
        of the product that is provided to every human user of the
        product: 
   
              "This product includes software developed as 
              part of the NPlot library project available 
              from: http://www.nplot.com/" 

        The words "This product" may optionally be replace with 
        the actual name of the product.

------------------------------------------------------------------------

THIS SOFTWARE IS PROVIDED BY THE AUTHOR ``AS IS'' AND ANY EXPRESS OR
IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.
IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY DIRECT, INDIRECT,
INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT
NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

*/

using System;
using System.Drawing;

namespace NPlot
{
	/// <summary>
	/// Class for creating a rainbow legend.
	/// </summary>
	public class StepGradient : IGradient
	{

		/// <summary>
		/// Types of step gradient defined.
		/// </summary>
		public enum Type
		{
			/// <summary>
			/// Rainbow gradient type (colors of the rainbow)
			/// </summary>
			Rainbow,

			/// <summary>
			/// RGB gradient type (red, green blud).
			/// </summary>
			RGB
		}

		/// <summary>
		/// Sets the type of step gradient.
		/// </summary>
		public Type StepType
		{
			get
			{
				return stepType_;
			}
			set
			{
				stepType_ = value;
			}
		}
		Type stepType_ = Type.RGB;


		/// <summary>
		/// Default Constructor
		/// </summary>
		public StepGradient()
		{
		}


		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="stepType">type of gradient</param>
		public StepGradient( Type stepType )
		{
			stepType_  = stepType;
		}

		/// <summary>
		/// Gets a color corresponding to a number between 0.0 and 1.0 inclusive. The color will
		/// be a linear interpolation of the min and max colors.
		/// </summary>
		/// <param name="prop">the number to get corresponding color for (between 0.0 and 1.0)</param>
		/// <returns>The color corresponding to the supplied number.</returns>
		public Color GetColor( double prop )
		{
			switch (stepType_)
			{
				case Type.RGB:
				{
					if (prop < 1.0/3.0) return Color.Red;
					if (prop < 2.0/3.0) return Color.Green;
					return Color.Blue;
				}
				case Type.Rainbow:
				{
					if (prop < 0.125) return Color.Red;
					if (prop < 0.25) return Color.Orange;
					if (prop < 0.375) return Color.Yellow;
					if (prop < 0.5) return Color.Green;
					if (prop < 0.625) return Color.Cyan;
					if (prop < 0.75) return Color.Blue;
					if (prop < 0.825) return Color.Purple;
					return Color.Pink;
				}
				default:
				{
					return Color.Black;
				}
			}

		}

	} 
}
