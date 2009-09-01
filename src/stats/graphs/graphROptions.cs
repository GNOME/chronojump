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
 * Xavier de Blas: 
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
	public int Width;
	public int Height;
	public string Legend;
	
	public GraphROptions(string Type, string VarX, string VarY, string Palette, bool Transposed, 
			int Width, int Height, string Legend) {
		this.Type = Type;
		this.VarX = VarX;
		this.VarY = VarY;
		this.Palette = Palette;
		this.Transposed = Transposed;
		this.Width = Width;
		this.Height = Height;
		this.Legend = Legend;
	}
	public GraphROptions() {
	}
}	
