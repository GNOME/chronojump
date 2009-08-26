/*
NPlot - A charting library for .NET

PiePlot.cs
Copyright (C) 2004
Thierry Malo

This library is free software; you can redistribute it and/or
modify it under the terms of the GNU Lesser General Public
License as published by the Free Software Foundation; either
version 2.1 of the License, or (at your option) any later version.

This library is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public
License along with this library; if not, write to the Free Software
Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA

$Id: PiePlot.cs,v 1.3 2004/10/23 07:08:35 mhowlett Exp $

*/

/*
using System;
using System.Drawing;

namespace scpl
{
	/// <summary>
	/// Description résumée de PiePlot.
	/// </summary>
	public class PiePlot : BasePlot, IPlot
	{
		public PiePlot(ISequenceAdapter datas)
		{
			//
			// TODO : ajoutez ici la logique du constructeur
			//
			this.Data=datas;

			_brushes=new Brush[_MaxBrush];

        _brushes[0] = Brushes.DarkBlue;
        _brushes[1] = Brushes.Yellow;
        _brushes[2] = Brushes.Green;
        _brushes[3] = Brushes.Brown;
        _brushes[4] = Brushes.Blue;
        _brushes[5] = Brushes.Red;
        _brushes[6] = Brushes.LightGreen;
        _brushes[7] = Brushes.Salmon;
}

		private ISequenceAdapter data_;
		private double _Total;
		const int _MaxBrush=8;
		private Brush[] _brushes;

		public ISequenceAdapter Data
		{
			get
			{
				return data_;
			}
			set
			{
				data_ = value;

				// calculate the sum of all value (this is related to 360°)
				_Total = 0;
				for ( int i=0; i<data_.Count; ++i )
				{
					_Total += data_[i].Y;
				}
			}
		}

		#region SuggestXAxis
		public virtual Axis SuggestXAxis()
		{
				return data_.SuggestXAxis();
		}
		#endregion

		#region SuggestXAxis
		public virtual Axis SuggestYAxis()
		{
			return data_.SuggestYAxis();
		}
		#endregion

		public virtual void Draw( Graphics g, PhysicalAxis xAxis, PhysicalAxis yAxis )
		{
			int LastAngle=0,ReelAngle;

			float rx=(xAxis.PhysicalMax.X - xAxis.PhysicalMin.X);
			float ry=(yAxis.PhysicalMin.Y - yAxis.PhysicalMax.Y);

			int h=(int) (ry * 0.8);
			int w= (int) (rx * 0.8);

			// This is to keep the pie based on a circle (i.e. inside a square)
			int s=Math.Min(h,w);

			// calculate boundary rectangle coordinate
			int cy=(int) (yAxis.PhysicalMax.Y + (h * 1.2 - s) / 2);
			int cx=(int) (xAxis.PhysicalMin.X + (w * 1.2 - s) / 2);

			for (int i=0; i<this.Data.Count; ++i)
			{
				ReelAngle = (int) (data_[i].Y * 360 / _Total);
				g.FillPie(_brushes[i % _MaxBrush], cx, cy, s, s, LastAngle, ReelAngle);
				LastAngle += ReelAngle;
			}
		}
	}
}
*/