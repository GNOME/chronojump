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
 * Copyright (C) 2004-2017   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
using System.Data;
using System.Collections; //ArrayList

//only to easy pass data
//this is new stuff created when moved to R
public class GraphROptions
{
	public string Type;
	public string VarX;
	public string VarY;
	public string Palette;
	public bool Transposed;
	public int LineWidth;
	public int Width;
	public int Height;
	public string Legend;
	public int MarginBottom;
	public int MarginLeft;
	public int MarginTop;
	public int MarginRight;
	public double XAxisFontSize;
	
	public GraphROptions(string Type, string VarX, string VarY, string Palette, bool Transposed, 
			int LineWidth, int Width, int Height, string Legend,
			int MarginBottom, int MarginLeft, int MarginTop, int MarginRight, double XAxisFontSize) {
		this.Type = Type;
		this.VarX = VarX;
		this.VarY = VarY;
		this.Palette = Palette;
		this.Transposed = Transposed;
		this.LineWidth = LineWidth;
		this.Width = Width;
		this.Height = Height;
		this.Legend = Legend;
		this.MarginBottom = MarginBottom;
		this.MarginLeft = MarginLeft;
		this.MarginTop = MarginTop;
		this.MarginRight = MarginRight;
		this.XAxisFontSize = XAxisFontSize;
	}
	public GraphROptions() {
	}
	
	//from report
	public GraphROptions(string str) {
		string [] strFull = str.Split(new char[] {':'});
		this.Type = strFull[0];
		this.VarX = strFull[1];
		this.VarY = strFull[2];
		this.Palette = strFull[3];
		this.Transposed = Util.StringToBool(strFull[4]);
		this.LineWidth = Convert.ToInt32(strFull[5]);
		this.Width = Convert.ToInt32(strFull[6]);
		this.Height = Convert.ToInt32(strFull[7]);
		this.Legend = strFull[8];
		this.MarginBottom = Convert.ToInt32(strFull[9]);
		this.MarginLeft = Convert.ToInt32(strFull[10]);
		this.MarginTop = Convert.ToInt32(strFull[11]);
		this.MarginRight = Convert.ToInt32(strFull[12]);
		this.XAxisFontSize = Convert.ToDouble(strFull[13]);
	}

	public override string ToString() {
		return Type + ":" + VarX + ":" + VarY + ":" + Palette + ":" + Transposed + ":" + LineWidth + ":" + Width + ":" + Height + ":" + Legend + 
			":" + MarginBottom + ":" + MarginLeft + ":" + MarginTop + ":" + MarginRight + ":" + XAxisFontSize;
	}
}	
