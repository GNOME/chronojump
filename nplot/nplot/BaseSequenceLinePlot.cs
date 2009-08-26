using System;
using System.Drawing;

namespace NPlot
{

	/// <summary>
	/// supplies implementation of basic functionality for plots based on drawing
	/// lines [line, step and histogram].
	/// </summary>
	/// <remarks>If C# had multiple inheritance, the heirachy would be different. The way it is isn't very nice.</remarks>
	public class BaseSequenceLinePlot : BaseSequencePlot, ISequencePlot
	{

		/// <summary>
		/// Draws a representation of this plot in the legend.
		/// </summary>
		/// <param name="g">The graphics surface on which to draw.</param>
		/// <param name="startEnd">A rectangle specifying the bounds of the area in the legend set aside for drawing.</param>
		public virtual void DrawInLegend( Graphics g, Rectangle startEnd )
		{
			g.DrawLine( pen_, startEnd.Left, (startEnd.Top + startEnd.Bottom)/2, 
				startEnd.Right, (startEnd.Top + startEnd.Bottom)/2 );
		}


		/// <summary>
		/// The pen used to draw the plot
		/// </summary>
		public System.Drawing.Pen Pen
		{
			get
			{
				return pen_;
			}
			set
			{
				pen_ = value;
			}
		}
		private System.Drawing.Pen pen_ = new Pen( Color.Black );


		/// <summary>
		/// The color of the pen used to draw lines in this plot.
		/// </summary>
		public System.Drawing.Color Color
		{
			set
			{
				if (pen_ != null)
				{
					pen_.Color = value;
				}
				else
				{
					pen_ = new Pen( value );
				}
			}
			get
			{
				return pen_.Color;
			}
		}


	}

}
