using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;

namespace NPlot.Windows
{

	/// <summary>
	/// An all encompasing Windows.Forms PlotSurface. This class allows you
	/// to place a single control in your form layout and draw to any type of 
	/// plot surface (PlotSurface2D, PlotSurface3D etc) on it. As there is only
	///  one type of plot surface currently, this 
	/// class isn't necessary... but more a planned soon. 
	/// Also, the implementation isn't finished.
	/// </summary>
	public class PlotSurface : System.Windows.Forms.UserControl
	{

		/// <summary>
		/// control resize handler.
		/// </summary>
		/// <param name="e">event args.</param>
		protected override void OnResize( EventArgs e )
		{
			this.Refresh();
			base.OnResize(e);
		}


		/// <summary>
		/// control paint handler.
		/// </summary>
		/// <param name="pe">paint event args.</param>
		protected override void OnPaint( PaintEventArgs pe )
		{
			surface_.DoPaint( pe, this.Width, this.Height );
			base.OnPaint(pe);
		}

		
		/// <summary>
		/// Mouse down event handler.
		/// </summary>
		/// <param name="e">mouse event args</param>
		protected override void OnMouseDown(MouseEventArgs e)
		{
			surface_.DoMouseDown(e);
			base.OnMouseDown(e);
		}


		/// <summary>
		/// Mouse Up event handler.
		/// </summary>
		/// <param name="e">mouse event args.</param>
		protected override void OnMouseUp(MouseEventArgs e)
		{
			surface_.DoMouseUp(e, this);
			base.OnMouseUp(e);
		}


		/// <summary>
		/// Mouse Move event handler.
		/// </summary>
		/// <param name="e">mouse event args.</param>
		protected override void OnMouseMove(MouseEventArgs e)
		{
			surface_.DoMouseMove(e, this);
			base.OnMouseMove(e);
		}

		/*
		enum Type 
		{
			PlotSurface2D,
			PlotSurface2Dnew,
			PlotSurface3D,
			PieChart
		}
		*/

		ISurface surface_ = null;
		/// <summary>
		/// Gets the underlying plot surface.
		/// </summary>
		public ISurface Surface
		{
			get
			{
				return surface_;
			}
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		public PlotSurface()
		{
			// double buffer, and update when resize.
			base.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
			base.SetStyle(ControlStyles.DoubleBuffer, true);
			base.SetStyle(ControlStyles.UserPaint, true);
			base.ResizeRedraw = true;

			surface_ = new NPlot.Windows.PlotSurface2D();
		}

	}
}
