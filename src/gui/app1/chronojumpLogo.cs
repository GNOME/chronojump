/*
 * This file is part of ChronoJump
 *
 * ChronoJump is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or   
 *    (at your option) any later version.
 *    
 * ChronoJump is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the 
 *    GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 *  along with this program; if not, write to the Free Software
 *   Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 *
 * Copyright (C) 2017-2020   Xavier de Blas <xaviblas@gmail.com>
 */

using Gtk;
using Cairo;
using System;
using System.Diagnostics;  //Stopwatch


//code based on puff
//http://zetcode.com/gui/gtksharp/drawingII/

//TODO: move all this to an specific class
//excepte the exposes event: that will construct the thing

public partial class ChronoJumpWindow 
{
	private bool timer_chronojump_logo = true;
	private double alpha_chronojump_logo = 1.0;
	private double size_chronojump_logo = 1.0;
	Stopwatch sw_chronojump_logo_pause_at_end;

	void reset_chronojump_logo()
	{
		timer_chronojump_logo = true;
		alpha_chronojump_logo = 1.0;
		size_chronojump_logo = 1.0;
		sw_chronojump_logo_pause_at_end = new Stopwatch();


		viewport_chronojump_logo.Visible = false;
		drawingarea_chronojump_logo.Visible = true;
	}

	bool OnTimer_chronojump_logo()
	{ 
                if (! timer_chronojump_logo)
		{
			drawingarea_chronojump_logo.Visible = false;
			viewport_chronojump_logo.Visible = true;

			return false;
		}

		drawingarea_chronojump_logo.QueueDraw();
                return true;
        } 

        void on_drawingarea_chronojump_logo_expose_event (object sender, ExposeEventArgs args)
        {
                DrawingArea area = (DrawingArea) sender;
                Cairo.Context cr =  Gdk.CairoHelper.Create(area.GdkWindow);

                int x = area.Allocation.Width / 2;
                int y = area.Allocation.Height / 2;

                cr.SetSourceRGB(.055, .118, .275);
                cr.Paint();

		//cr.SelectFontFace("Courier", FontSlant.Normal, FontWeight.Bold);
                cr.SelectFontFace("Ubuntu", FontSlant.Normal, FontWeight.Bold);

		bool showVersion = false;
		if (size_chronojump_logo <= 80)
			size_chronojump_logo += 0.7;

		if(size_chronojump_logo > 20)
		{
			alpha_chronojump_logo -= 0.01;
			if(alpha_chronojump_logo < 0)
			{
				alpha_chronojump_logo = 0;
				sw_chronojump_logo_pause_at_end.Start();
			}
		}

		if (sw_chronojump_logo_pause_at_end.Elapsed.TotalMilliseconds >= 300)
			timer_chronojump_logo = false;

		chronojumpLogo_showChronojump (cr, x, y);

                ((IDisposable) cr.Target).Dispose();
                ((IDisposable) cr).Dispose();
        }

	private void chronojumpLogo_showChronojump (Cairo.Context cr, int x, int y)
	{

                cr.SetFontSize(size_chronojump_logo);
                cr.SetSourceRGB(1, 1, 1);

		string	message = "CHRONOJUMP   2.0";
                TextExtents extents = cr.TextExtents(message);

                cr.MoveTo(x - extents.Width/2, y + extents.Height/2);
                cr.TextPath(message);
                cr.Clip();
                cr.Stroke();
                cr.PaintWithAlpha(alpha_chronojump_logo);
	}

}
