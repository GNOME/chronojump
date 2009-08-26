// ******** experimental ******** 

/*
using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;
using System.IO;

namespace NPlot.Windows
{

	/// <summary>
	/// Experimental
	/// </summary>
	public class PlotSurface2Dnew : System.Windows.Forms.UserControl, IPlotSurface2Dnew
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		/// <summary>
		/// Constructor
		/// </summary>
		public PlotSurface2Dnew()
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();

			// double buffer, and update when resize.
			base.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
			base.SetStyle(ControlStyles.DoubleBuffer, true);
			base.SetStyle(ControlStyles.UserPaint, true);
			base.ResizeRedraw = true;
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if( components != null )
					components.Dispose();
			}
			base.Dispose( disposing );
		}

		#region Component Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			components = new System.ComponentModel.Container();
		}
		#endregion

		private NPlot.PlotSurface2Dnew Inner = new NPlot.PlotSurface2Dnew();

		/// <summary>
		/// Experimental
		/// </summary>
		public void SetDefinition( )
		{
			Inner.SetDefinition( );
		}

		/// <summary>
		/// the paint event callback.
		/// </summary>
		/// <param name="pe">paint event arguments - used to get graphics surface to draw on.</param>
		protected override void OnPaint( PaintEventArgs pe )
		{
			Graphics g = pe.Graphics;
			
			Rectangle border = new Rectangle( 0, 0, this.Width, this.Height );
			
			if ( g == null ) 
			{
				ErrorHandler.Instance.CriticalError( "graphics context null" );
			}
						
			if ( border == Rectangle.Empty )
			{
				ErrorHandler.Instance.CriticalError( "Control has zero extent" );
				return;
			}

			this.Draw( g, border );

			base.OnPaint(pe);
		}


		/// <summary>
		/// Draws the plot surface on the supplied graphics surface [not the control surface].
		/// </summary>
		/// <param name="g">The graphics surface on which to draw</param>
		/// <param name="bounds">A bounding box on this surface that denotes the area on the
		/// surface to confine drawing to.</param>
		public void Draw( Graphics g, Rectangle bounds )
		{
			try 
			{
				Inner.Draw( g, bounds );
			} 
			catch (Exception ex) 
			{
				System.Diagnostics.Debugger.Log( 1, "", "Exception drawing plot:" + ex + "\r\n" );
			}
		}


		/// <summary>
		/// Padding of this width will be left between what is drawn and the control border.
		/// </summary>
		[
		Category("PlotSurface2D"),
		Description("Padding of this width will be left between what is drawn and the control border."),
		Browsable(true)
		]
		public int Padding
		{
			get
			{
				return Inner.Padding;
			}
			set
			{
				Inner.Padding = value;
			}
		}


		/// <summary>
		/// Adds a drawable object to the plot surface against the specified axes. If
		/// the object is an IPlot, the PlotSurface2D axes will also be updated.
		/// </summary>
		/// <param name="p">the IDrawable object to add to the plot surface</param>
		/// <param name="xAxis">the x-axis to add the plot against.</param>
		/// <param name="yAxis">the y-axis to add the plot against.</param>am>
		public void Add( IDrawable p, string xAxis, string yAxis )
		{
			this.Inner.Add( p, xAxis, yAxis );
		}

	}
}
*/
