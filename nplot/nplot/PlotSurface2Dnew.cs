// ******** experimental ******** 
/*
using System;
using System.Xml;
using System.Data;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Drawing;

namespace NPlot
{

	class PlotSurface2Dnew
	{

		private ArrayList axisLines_;
		private ArrayList axisDefinitions_;
		private ArrayList drawables_;
		private ArrayList xAxisPositions_;
		private ArrayList yAxisPositions_;

		private System.Collections.Hashtable axes_;

		/// <summary>
		/// PlotSurface2D styles supplied with the library.
		/// </summary>
		public enum PlotSurfaceStyle
		{
			Standard,
			CrossedAxes,
			OppositeCloned
		}

		private class AxisDefinition
		{
			public string Name = "";
			public AxisLine axisLine = null;
			public int Min = 0;
			public int Max = 100;
		}

		private class AxisLine
		{
			public enum OrientationType
			{
				Horizontal,
				Vertical
			}

			public string Name = "";
			public OrientationType Orientation = OrientationType.Horizontal;
			public float Position = 0;
		}


		/// <summary>
		/// Constructor
		/// </summary>
		public PlotSurface2Dnew()
		{
			Init();
		}


		private AxisDefinition parseAxis( XmlNode node )
		{
			// get Parameters 
			AxisDefinition d = new AxisDefinition();

			for (int i=0; i<node.Attributes.Count; ++i)
			{
				XmlAttribute a = node.Attributes[i];
				switch (a.Name.ToLower())
				{
					case "name":
						d.Name = a.Value;
						break;

					case "axisline":
						bool found = false;
						for (int j=0; j<axisLines_.Count; ++j)
						{
							if (((AxisLine)axisLines_[j]).Name == a.Value)
							{
								d.axisLine = (AxisLine)axisLines_[j];
								found = true;
							}
						}
						if (!found)
						{
							ErrorHandler.Instance.CriticalError( "AxisLine not found" );
						}
						break;

					case "physicalmin":
						try
						{
							d.Min = Convert.ToInt32( a.Value );
						}
						catch
						{
							ErrorHandler.Instance.CriticalError( "min value not numeric" );
						}
						if (d.Min< 0 || d.Min > 100)
						{
							ErrorHandler.Instance.CriticalError( "min value must be between 0 and 100" );
						}
						break;

					case "physicalmax":
						try
						{
							d.Max = Convert.ToInt32( a.Value );
						}
						catch
						{
							ErrorHandler.Instance.CriticalError( "max value not numeric" );
						}
						if (d.Max < 0 || d.Max > 100)
						{
							ErrorHandler.Instance.CriticalError( "max value must be between 0 and 100" );
						}
						break;

					default:
						ErrorHandler.Instance.CriticalError( "unknown attribute: " + a.Name );
						break;

				}
			}

			return d;
		}


		private AxisLine parseAxisLine( XmlNode node )
		{
		
			AxisLine l = new AxisLine();

			for (int i=0; i<node.Attributes.Count; ++i)
			{
				XmlAttribute a = node.Attributes[i];
				switch (a.Name.ToLower())
				{
					case "name":
						l.Name = a.Value;
						break;

					case "orientation":
						if (a.Value.ToLower() == "horizontal")
							l.Orientation = AxisLine.OrientationType.Horizontal;
						else if (a.Value.ToLower() == "vertical")
							l.Orientation = AxisLine.OrientationType.Vertical;
						else
						{
							ErrorHandler.Instance.CriticalError( "Unexpected orientation :" + a.Value.ToString() );
						}	
						break;

					case "position":
						try
						{
							l.Position = Convert.ToInt32( a.Value );
						}
						catch
						{
							ErrorHandler.Instance.CriticalError( "position not numeric" );
						}
						if (l.Position < 0 || l.Position > 100)
						{
							ErrorHandler.Instance.CriticalError( "axis position must be between 0 and 100" );
						}
						break;
									
					default:
						ErrorHandler.Instance.CriticalError( "unknown axis line attribute: " + a.Name );
						break;
				}
			}
					
			return l;
		}


		public void SetDefinition( )
		{

			Assembly ass = Assembly.GetExecutingAssembly();
			System.IO.Stream xmlStream = 
				ass.GetManifestResourceStream( "NPlot.PlotSurfaceDefinitions.OppositeCloned.xml" );

			// load xml

			XmlDocument xmlDoc = null;

			try
			{
				xmlDoc = new XmlDocument();
				xmlDoc.Load( xmlStream );
			}
			catch
			{
				ErrorHandler.Instance.CriticalError( "PlotSurface2D definition file malformed" );
			}

			XmlElement el = xmlDoc.DocumentElement;
			
			// check we're a PlotSurface2D.
			if (el.Name.ToLower() != "plotsurface2d")
			{
				ErrorHandler.Instance.CriticalError( "Root element must be PlotSurface2D" );
				return;
			}

			// enforce only one axis set for now.
			if (el.ChildNodes.Count != 1)
			{
				ErrorHandler.Instance.CriticalError( "need one and only one axis set." );
				return;
			}

			// loop around all AxisSets.
			foreach (XmlNode n in el.ChildNodes)
			{
				if (n.Name.ToLower() != "axisset")
				{
					ErrorHandler.Instance.CriticalError( "Expected AxisSet node" );
					return;
				}

				// loop around all nodes in this axis set.
				foreach (XmlNode n2 in n.ChildNodes)
				{
					switch (n2.Name.ToLower())
					{
						case "axisline":
						{
							axisLines_.Add( parseAxisLine(n2) ); 
							break;
						}
						case "axis":
						{
							axisDefinitions_.Add( parseAxis(n2) );
							break;
						}
						default:
						{
							ErrorHandler.Instance.CriticalError( "Unexpected node type encountered: " + n2.Name );
							return;
						}
					}
				} // end loop around each node in axis set

				// add all axes to class
				for (int i=0; i<axisDefinitions_.Count; ++i)
				{
					//axes_.Add( ((AxisDefinition)axisDefinitions_[i]).Name, null );
				}

			} // end loop around axis sets.
			
		}


		/// <summary>
		/// The distance in pixels to leave between of the edge of the bounding rectangle
		/// supplied to the Draw method, and the markings that make up the plot.
		/// </summary>
		public int Padding
		{
			get
			{
				return padding_;
			}
			set
			{
				padding_ = value;
			}
		}
		private int padding_;


		private void Init()
		{
			axes_ = null;

			drawables_ = new ArrayList();
			xAxisPositions_ = new ArrayList();
			yAxisPositions_ = new ArrayList();
			axisLines_ = new ArrayList();
			axisDefinitions_ = new ArrayList();

			padding_ = 10;
		}


		/// <summary>
		/// Draw the the PlotSurface2D and all contents [axes, drawables, and legend] on the 
		/// supplied graphics surface.
		/// </summary>
		/// <param name="g">The graphics surface on which to draw.</param>
		/// <param name="bounds">A bounding box on this surface that denotes the area on the
		/// surface to confine drawing to.</param>
		public void Draw( Graphics g, Rectangle bounds )
		{
			Rectangle realBounds = new Rectangle( 
				bounds.X + padding_ / 2, 
				bounds.Y + padding_ / 2,
				bounds.Width - padding_,
				bounds.Height - padding_
			);

			// create initial set of physical axes. These will
			// be moved later to ensure everything is drawn ok.
			ArrayList physicalAxes = new ArrayList();

			for (int i=0; i<axisDefinitions_.Count; ++i)
			{
				AxisDefinition d = (AxisDefinition)axisDefinitions_[i];
				
				Point physicalMin = new Point(0,0);
				Point physicalMax = new Point(0,0);

				if (d.axisLine.Orientation == AxisLine.OrientationType.Horizontal)
				{
					int yPos = (int)(realBounds.Top + ((float)d.axisLine.Position/100.0 * realBounds.Height));

					physicalMin.X = realBounds.Left;
					physicalMin.Y = yPos;
					physicalMax.X = realBounds.Right;
					physicalMax.Y = yPos;
				}
				else
				{
					int xPos = (int)(realBounds.Left + ((float)d.axisLine.Position/100.0 * realBounds.Width));

					physicalMin.X = xPos;
					physicalMin.Y = realBounds.Bottom;
					physicalMax.X = xPos;
					physicalMax.Y = realBounds.Top;
				}

				physicalAxes.Add( new PhysicalAxis( new LinearAxis(0.0,1.0), physicalMin, physicalMax ) );
			}

			for (int i=0; i<physicalAxes.Count; ++i)
			{
				Rectangle axisBounds;
				((PhysicalAxis)physicalAxes[i]).Draw( g, out axisBounds );
			}


		}


		private void UpdateAxes()
		{
			// make sure drawable lists exist.
			if (drawables_.Count==0 || xAxisPositions_.Count==0 || yAxisPositions_.Count==0)
			{
				ErrorHandler.Instance.ContinuingError( "UpdateAxes called from function other than Add." );
			}

			int last = drawables_.Count - 1;
			
			if ( last != xAxisPositions_.Count-1 || last != yAxisPositions_.Count-1 )
			{
				ErrorHandler.Instance.CriticalError( "plots and axis position arrays our of sync" );
			}


			// make sure axes exit.
			AxisDefinition xAxisDefinition = null;
			AxisDefinition yAxisDefinition = null;
			for (int i=0; i<this.axisDefinitions_.Count; ++i)
			{
				AxisDefinition def = (AxisDefinition)axisDefinitions_[i];
				if (def.Name == (string)xAxisPositions_[last])
					xAxisDefinition = def;

				if (def.Name == (string)yAxisPositions_[last])
					yAxisDefinition = def;	
			}

			if (xAxisDefinition == null || yAxisDefinition == null)
			{
				ErrorHandler.Instance.CriticalError( "Axis does not exist." );
			}

			IPlot p = (IPlot)drawables_[last];

			// update x axis.
			if (axes_[xAxisDefinition.Name] == null)
			{
				this.axes_[xAxisDefinition.Name] = p.SuggestXAxis();
			}
			else
			{
				((Axis)this.axes_[xAxisDefinition.Name]).LUB( p.SuggestXAxis() );
			}

			// update y axis. 
			if (axes_[yAxisDefinition.Name] == null)
			{
				this.axes_[yAxisDefinition.Name] = p.SuggestYAxis();
			}
			else
			{
				((Axis)this.axes_[yAxisDefinition.Name]).LUB( p.SuggestYAxis() );
			}

		}


		/// <summary>
		/// Adds a drawable object to the plot surface against the specified axes. If
		/// the object is an IPlot, the PlotSurface2D axes will also be updated.
		/// </summary>
		/// <param name="p">the IDrawable object to add to the plot surface</param>
		/// <param name="xAxis">the x-axis to add the plot against.</param>
		/// <param name="yAxis">the y-axis to add the plot against.</param>
		public void Add( IDrawable p, string xAxis, string yAxis )
		{
			drawables_.Add( p );
			xAxisPositions_.Add( xAxis );
			yAxisPositions_.Add( yAxis );

			if ( p is IPlot )
			{
				this.UpdateAxes();
			}
		}

	}

}
*/