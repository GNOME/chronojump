/*
NPlot - A charting library for .NET

Gradient.cs
Copyright (C) 2003
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
	/// Class for creating a linear gradient.
	/// </summary>
	public class LinearGradient : IGradient
	{
		
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="minColor">The color corresponding to 0.0</param>
		/// <param name="maxColor">The color corresponding to 1.0</param>
		public LinearGradient( Color minColor, Color maxColor )
		{
			this.minColor_ = minColor;
			this.maxColor_ = maxColor;
		}


		/// <summary>
		/// The color corresponding to 0.0
		/// </summary>
		public Color MaxColor
		{
			get
			{
				return this.maxColor_;
			}
			set
			{
				this.maxColor_ = value;
			}
		}
		private Color maxColor_;


		/// <summary>
		/// The color corresponding to 1.0
		/// </summary>
		public Color MinColor
		{
			get
			{
				return this.minColor_;
			}
			set
			{
				this.minColor_ = value;
			}
		}
		private Color minColor_;


		/// <summary>
		/// The color corresponding to NaN
		/// </summary>
		public Color VoidColor			         
		{
			get 
			{ 
				return voidColor_; 
			}
			set 
			{ 
				voidColor_ = value; 
			}
		}
        private Color voidColor_ = Color.Black;


		/// <summary>
		/// Gets a color corresponding to a number between 0.0 and 1.0 inclusive. The color will
		/// be a linear interpolation of the min and max colors.
		/// </summary>
		/// <param name="prop">the number to get corresponding color for (between 0.0 and 1.0)</param>
		/// <returns>The color corresponding to the supplied number.</returns>
		public Color GetColor( double prop )
		{
            if (Double.IsNaN(prop))
            {
                return voidColor_;
            }

			if ( prop <= 0.0 )
			{
				return this.MinColor;
			}

			if ( prop >= 1.0 )
			{
				return this.MaxColor;
			}

			byte r = (byte)((int)(this.MinColor.R) + (int)(((double)this.MaxColor.R - (double)this.MinColor.R)*prop));
			byte g = (byte)((int)(this.MinColor.G) + (int)(((double)this.MaxColor.G - (double)this.MinColor.G)*prop));
			byte b = (byte)((int)(this.MinColor.B) + (int)(((double)this.MaxColor.B - (double)this.MinColor.B)*prop));

			return Color.FromArgb(r,g,b);
		}


	}
}
