
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
 *  Copyright (C) 2004-2023   Xavier de Blas <xaviblas@gmail.com>
 */

using System;
using Cairo;
using Gtk;
using Gdk;

public static class CairoUtil
{
	const int textHeight = 12;

	/*
	 * public methods
	 */

	//based on https://stackoverflow.com/q/9899756
	public static void GetScreenshotFromDrawingArea (Gtk.DrawingArea darea, string destination)
	{
		var src_context = Gdk.CairoHelper.Create (darea.Window);
		var src_surface = src_context.GetTarget ();
		var dst_surface = new Cairo.ImageSurface (Cairo.Format.ARGB32, darea.Allocation.Width, darea.Allocation.Height);
		var dst_context = new Cairo.Context (dst_surface);
		dst_context.SetSourceSurface (src_surface, 0, 0);
		dst_context.Paint ();
		dst_surface.WriteToPng (destination);
	}

	public static void GetScreenshotFromVBox (Gtk.VBox vbox, string destination)
	{
		var src_context = Gdk.CairoHelper.Create (vbox.Window);
		var src_surface = src_context.GetTarget ();
		var dst_surface = new Cairo.ImageSurface (Cairo.Format.ARGB32, vbox.Allocation.Width, vbox.Allocation.Height);
		var dst_context = new Cairo.Context (dst_surface);
		dst_context.SetSourceSurface (src_surface, -vbox.Allocation.X, -vbox.Allocation.Y);
		dst_context.Paint ();
		dst_surface.WriteToPng (destination);
	}

	public static void PaintDrawingArea (DrawingArea da, Context g, RGBA color)
	{
		int width = da.Allocation.Width;
		int height = da.Allocation.Height;

		g.SetSourceRGBA (color.Red, color.Green, color.Blue, 1);
		g.Rectangle(0, 0, width, height);
		g.Fill();

		g.GetTarget().Dispose();
		g.Dispose();
	}

	public static void PaintSegment (Cairo.Context g, Cairo.Color color, double x1, double y1, double x2, double y2)
	{
		g.SetSourceColor (color);
		PaintSegment (g, x1, y1, x2, y2);
	}
	public static void PaintSegment (Cairo.Context g, double x1, double y1, double x2, double y2)
	{
		g.MoveTo (x1, y1);
		g.LineTo (x2, y2);
		g.Stroke ();
	}

	public static void PaintVerticalLinesAndRectangle (
			Cairo.Context g, int graphHeight, double xposA, double xposB, bool posBuse, int topRect, int bottomRect)
	{
		paintVerticalLinesAndRectangleDo (g, graphHeight, xposA, xposB, posBuse, topRect, bottomRect);
	}

	public static void PaintVerticalLinesAndRectangle (
			Gtk.DrawingArea darea, double xposA, double xposB, bool posBuse, int topRect, int bottomRect)
	{
		using (Cairo.Context g = Gdk.CairoHelper.Create (darea.Window)) 
		{
			paintVerticalLinesAndRectangleDo (g, darea.Allocation.Height, xposA, xposB, posBuse, topRect, bottomRect);
			g.Stroke();
			g.GetTarget ().Dispose ();
		}
	}
	public static void PaintVerticalLinesAndRectangleOnSurface (
			Gtk.DrawingArea darea, int xposA, int xposB, bool posBuse, int topRect, int bottomRect, Pixbuf pixbuf)
	{
		using (Cairo.Context g = Gdk.CairoHelper.Create (darea.Window)) 
		{
			//add image
			Gdk.CairoHelper.SetSourcePixbuf (g, pixbuf, 0, 0);

			g.Paint();

			paintVerticalLinesAndRectangleDo (g, darea.Allocation.Height, xposA, xposB, posBuse, topRect, bottomRect);
			g.Stroke();
			g.GetTarget ().Dispose ();
		}
	}

	public static void Blank (
		Gtk.DrawingArea darea)
	{
		using (Cairo.Context g = Gdk.CairoHelper.Create (darea.Window))
		{
			g.SetSourceRGBA(1, 1, 1, 1);
			g.Paint();
			g.Stroke();
			g.GetTarget ().Dispose ();
		}
	}

	/*
	 * private methods
	 */

	private static void paintVerticalLinesAndRectangleDo (Cairo.Context g, int height, double xposA, double xposB, bool posBuse, int topRect, int bottomRect)
	{
		//add rectangle
		g.SetSourceRGBA(0.906, 0.745, 0.098, 1); //Chronojump yellow

		paintVerticalLine(g, xposA, height, "A");

		if(posBuse && xposA != xposB)
		{
			paintVerticalLine(g, xposB, height, "B");

			//g.SetSourceRGBA(0.906, 0.745, 0.098, .5); //Chronojump yellow, half transp
			g.SetSourceRGBA(0.9, 0.9, 0.01, .15); //More yellow and very transp

			//create rectangle
			double min = Math.Min(xposA, xposB) +1;
			double max = Math.Max(xposA, xposB) -1;
			if(min < max)
			{
				g.Rectangle(min, topRect , max-min, height - bottomRect);
				g.Fill();
			}
		}
	}

	private static void paintVerticalLine (Cairo.Context g, double x, int height, string letter)
	{
		//vertical line
		g.MoveTo(x, 9);
		g.LineTo(x, height);
		g.Stroke();

		/*
		//show top triangle
		g.MoveTo(x -4, 0);
		g.LineTo(x   , 8);
		g.LineTo(x +4, 0);
		g.LineTo(x -4, 0);
		g.Fill();
		*/
		//show letter
		printText(x, 2, 0, textHeight, letter, g, true);

		/*
		//bottom triangle currently not drawn because bottom space changes and half of triangle is not shown
		g.MoveTo(x -4, drawingarea_encoder_analyze_cairo_pixbuf.Height);
		g.LineTo(x   , drawingarea_encoder_analyze_cairo_pixbuf.Height -8);
		g.LineTo(x +4, drawingarea_encoder_analyze_cairo_pixbuf.Height);
		*/
	}

	private static void printText (double x, int y, int height, int textHeight, string text, Cairo.Context g, bool centered)
	{
		int moveToLeft = 0;
		if(centered)
		{
			Cairo.TextExtents te;
			te = g.TextExtents(text);
			moveToLeft = Convert.ToInt32(te.Width/2);
		}
		g.MoveTo( x - moveToLeft, ((y+y+height)/2) + textHeight/2 );
		g.ShowText(text);
	}
}
