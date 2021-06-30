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

public class ChronojumpLogo
{
	private bool timer;
	private double alpha;
	private double size;
	private double xpos;
	private double ypos;
	private Stopwatch stopwatch1;
	private Stopwatch stopwatch2;
	private Stopwatch stopwatch3;

	private Gtk.Notebook notebook;
	private Gtk.DrawingArea drawingarea; 	//contains the animation
	private Gtk.Viewport viewport; 		//contains the logo and version number
	private string font;

	//constructor
	public ChronojumpLogo (Gtk.Notebook notebook, Gtk.DrawingArea drawingarea, Gtk.Viewport viewport, bool showAnimated, string font)
	{
		LogB.Information("Chronojump logo constructor start");

		this.notebook = notebook;
		this.drawingarea = drawingarea;
		this.viewport = viewport;
		this.font = font;

		if(! showAnimated)
		{
			notebook.CurrentPage = 1;
			//viewport.Visible = true;
			//drawingarea.Visible = false;
			return;
		}

		//viewport.Visible = false;
		//drawingarea.Visible = true;
		notebook.CurrentPage = 0;

		timer = true;
		alpha = 1.0;
		size = 1.0;
		xpos = 0;
		ypos = .5;
		stopwatch1 = new Stopwatch();
		stopwatch2 = new Stopwatch();
		stopwatch3 = new Stopwatch();

		GLib.Timeout.Add(12, new GLib.TimeoutHandler(onTimer));
		//GLib.Timeout.Add(30, new GLib.TimeoutHandler(onTimer));
		stopwatch1.Start();

		LogB.Information("Chronojump logo constructor end");
	}

	private bool onTimer()
	{ 
                if (! timer)
		{
			//drawingarea.Visible = false;
			//viewport.Visible = true;
			notebook.CurrentPage = 1;

			return false;
		}

		redraw();
                return true;
        } 

	private void redraw()
	{
                Cairo.Context cr =  Gdk.CairoHelper.Create(drawingarea.GdkWindow);

		double elapsedMs1 = stopwatch1.Elapsed.TotalMilliseconds;

		xpos = UtilAll.DivideSafeFraction(elapsedMs1, 500);
		ypos = Math.Pow(xpos,3);

                int x = Convert.ToInt32(xpos * drawingarea.Allocation.Width / 2);
                int y = Convert.ToInt32(ypos * drawingarea.Allocation.Height / 2);

                cr.SetSourceRGB(.055, .118, .275);
                cr.Paint();

                //cr.SelectFontFace("Ubuntu", FontSlant.Normal, FontWeight.Bold); //need to check if they have this font
                //cr.SelectFontFace(font, FontSlant.Normal, FontWeight.Bold); //Courier is so ugly on logo
		cr.SelectFontFace("Helvetica", FontSlant.Normal, FontWeight.Bold);

		bool showVersion = false;
		if (size <= 80) {
			//size += 0.6;
			size = elapsedMs1 / 20.0;
		}

		if(size > 20)
		{
			//alpha -= 0.01;
			if(! stopwatch2.IsRunning)
				stopwatch2.Start();
			alpha = 1 - stopwatch2.Elapsed.TotalMilliseconds * 0.00083;

			if(alpha < 0)
			{
				alpha = 0;
				stopwatch3.Start();
			}
		}

		if (stopwatch3.Elapsed.TotalMilliseconds >= 300)
		{
			timer = false;
		}

		chronojumpLogo_showChronojump (cr, x, y);

                ((IDisposable) cr.Target).Dispose();
                ((IDisposable) cr).Dispose();
        }

	private void chronojumpLogo_showChronojump (Cairo.Context cr, int x, int y)
	{
                cr.SetFontSize(size);
                cr.SetSourceRGB(1, 1, 1);

		string	message = "CHRONOJUMP   2.1";
                TextExtents extents = cr.TextExtents(message);

                cr.MoveTo(x - extents.Width/2, y + extents.Height/2);
                cr.TextPath(message);
                cr.Clip();
                cr.Stroke();
                cr.PaintWithAlpha(alpha);
	}

}
