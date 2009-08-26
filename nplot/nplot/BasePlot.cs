/*
NPlot - A charting library for .NET

BasePlot.cs
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
	/// Supplies implementation of basic legend handling properties, and
	/// basic data specifying properties which are used by all plots.
	/// </summary>
	/// <remarks>If C# had multiple inheritance, the heirachy would be different.</remarks>
	public abstract class BasePlot 
	{

		/// <summary>
		/// A label to associate with the plot - used in the legend.
		/// </summary>
		public string Label
		{
			get
			{
				return label_;
			}
			set
			{
				this.label_ = value;
			}
		}
		
		private string label_ = "";


		/// <summary>
		/// Whether or not to include an entry for this plot in the legend if it exists.
		/// </summary>
		public bool ShowInLegend
		{
			get
			{
				return showInLegend_;
			}
			set
			{
				this.showInLegend_ = value;
			}
		}
		private bool showInLegend_ = true;


		/// <summary>
		/// Gets or sets the source containing a list of values used to populate the plot object.
		/// </summary>
		public object DataSource
		{
			get
			{
				return this.dataSource_;
			}
			set
			{
				this.dataSource_ = value;
			}
		}
		private object dataSource_ = null;


		/// <summary>
		/// Gets or sets the specific data member in a multimember data source to get data from.
		/// </summary>
		public string DataMember
		{
			get
			{
				return this.dataMember_;
			}
			set
			{
				this.dataMember_ = value;
			}
		}
		private string dataMember_ = null;



	}
}
