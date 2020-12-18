
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
 *  Copyright (C) 2004-2020   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
using Gtk;
using Cairo;

public abstract class CairoGeneric
{
	/*
	   need to dispose because Cairo does not clean ok on win and mac:
	   Don’t forget to manually dispose the Context and the target Surface at the end of the expose event. Automatic garbage collecting is not yet 100% working in Cairo.
		https://www.mono-project.com/docs/tools+libraries/libraries/Mono.Cairo/
		eg. on Linux can do the writeCoordinatesOfMouseClick() without disposing, but on win and mac does not work, so dispose always.
	 */

	protected void endGraphDisposing(Cairo.Context g)
	{
		g.GetTarget().Dispose ();
		g.Dispose ();
	}

	//0 - 255
	protected Cairo.Color colorFromRGB(int red, int green, int blue)
	{
		return new Cairo.Color(red/256.0, green/256.0, blue/256.0);
	}
	protected Cairo.Color colorFromGdk(Gdk.Color color)
	{
		return new Cairo.Color(color.Red/65536.0, color.Green/65536.0, color.Blue/65536.0);
	}

	protected enum alignTypes { LEFT, CENTER, RIGHT }
	protected void printText (int x, int y, int height, int textHeight, string text, Cairo.Context g, alignTypes align)
	{
		int moveToLeft = 0;
		if(align == alignTypes.CENTER || align == alignTypes.RIGHT)
		{
			Cairo.TextExtents te;
			te = g.TextExtents(text);

			if(align == alignTypes.CENTER)
				moveToLeft = Convert.ToInt32(te.Width/2);
			else
				moveToLeft = Convert.ToInt32(te.Width);
		}

		g.MoveTo( x - moveToLeft, ((y+y+height)/2) + textHeight/2 );
		g.ShowText(text);
	}
}
