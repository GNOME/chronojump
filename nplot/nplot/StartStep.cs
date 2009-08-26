/*
NPlot - A charting library for .NET

StartStep.cs
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

namespace NPlot
{

	/// <summary>
	/// Encapsulates a Start and Step value. This is useful for specifying a regularly spaced set of 
	/// abscissa values.
	/// </summary>
	public class StartStep
	{

		private double start_;
		private double step_;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="start">the first value of the set of points specified by this object.</param>
		/// <param name="step">the step that specifies the separation between successive points.</param>
		public StartStep( double start, double step )
		{
			this.Start = start;
			this.Step = step;
		}


		/// <summary>
		/// The first value of the set of points specified by this object.
		/// </summary>
		public double Start
		{
			get
			{
				return start_;
			}
			set
			{
				this.start_ = value;
			}
		}


		/// <summary>
		/// The step that specifies the separation between successive points.
		/// </summary>
		public double Step
		{
			get
			{
				return this.step_;
			}
			set
			{
				this.step_ = value;
			}
		}

	}
}
