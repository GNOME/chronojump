/*
NPlot - A charting library for .NET

BaseSequencePlot.cs
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

namespace NPlot
{

	/// <summary>
	/// Adds additional basic functionality to BasePlot that is common to all
	/// plots that implement the ISequencePlot interface.
	/// </summary>
	/// <remarks>If C# had multiple inheritance, the heirachy would be different. The way it is isn't very nice.</remarks>
	public class BaseSequencePlot : BasePlot, ISequencePlot
	{

		/// <summary>
		/// Gets or sets the data, or column name for the ordinate [y] axis.
		/// </summary>
		public object OrdinateData
		{
			get
			{
				return this.ordinateData_;
			}
			set
			{
				this.ordinateData_ = value;
			}
		}
		private object ordinateData_ = null;


		/// <summary>
		/// Gets or sets the data, or column name for the abscissa [x] axis.
		/// </summary>
		public object AbscissaData
		{
			get
			{
				return this.abscissaData_;
			}
			set
			{
				this.abscissaData_ = value;
			}
		}
		private object abscissaData_ = null;


		/// <summary>
		/// Writes text data of the plot object to the supplied string builder. It is 
		/// possible to specify that only data in the specified range be written.
		/// </summary>
		/// <param name="sb">the StringBuilder object to write to.</param>
		/// <param name="region">a region used if onlyInRegion is true.</param>
		/// <param name="onlyInRegion">If true, only data enclosed in the provided region will be written.</param>
		public void WriteData( System.Text.StringBuilder sb, RectangleD region, bool onlyInRegion )
		{
			SequenceAdapter data_ = 
				new SequenceAdapter( this.DataSource, this.DataMember, this.OrdinateData, this.AbscissaData );

			sb.Append( "Label: " );
			sb.Append( this.Label );
			sb.Append( "\r\n" );
			data_.WriteData( sb, region, onlyInRegion );
		}

	}
}
