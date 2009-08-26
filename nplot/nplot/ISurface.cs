/*
NPlot - A charting library for .NET

ISurface.cs
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
using System.Windows.Forms;

namespace NPlot
{

	/// <summary>
	/// All PlotSurface's implement this interface.
	/// </summary>
	/// <remarks>Some of the parameter lists will change to be made more uniform.</remarks>
	public interface ISurface
	{

		/// <summary>
		/// Provides functionality for drawing the control.
		/// </summary>
		/// <param name="pe">paint event args</param>
		/// <param name="width">width of the control.</param>
		/// <param name="height">height of the control.</param>
		void DoPaint( PaintEventArgs pe, int width, int height );
		
	
		/// <summary>
		/// Provides functionality for handling mouse up events.
		/// </summary>
		/// <param name="e">mouse event args</param>
		/// <param name="ctr">the control</param>
		void DoMouseUp( MouseEventArgs e, System.Windows.Forms.Control ctr );
		
		
		/// <summary>
		/// Provides functionality for handling mouse move events.
		/// </summary>
		/// <param name="e">mouse event args</param>
		/// <param name="ctr">the control</param>
		void DoMouseMove( MouseEventArgs e, System.Windows.Forms.Control ctr );
		
		
		/// <summary>
		/// Provides functionality for handling mouse down events.
		/// </summary>
		/// <param name="e">mouse event args</param>
		void DoMouseDown( MouseEventArgs e );
	
	}

}
