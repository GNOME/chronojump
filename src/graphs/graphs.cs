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
 *  Copyright (C) 2023 Xavier de Blas <xaviblas@gmail.com>
 */

using System;
using System.Collections.Generic; //List

public abstract class Graphs
{
	protected List<PointF> point_l; //to cairo points graph

	protected double mouseX;
	protected double mouseY;

	public void MouseReset ()
	{
		mouseX = -1;
		mouseY = -1;
	}
	public void MouseSet (double mouseX, double mouseY)
	{
		this.mouseX = mouseX;
		this.mouseY = mouseY;
	}

	public void Calculate ()
	{
	}

	public List<PointF> Point_l
	{
		get { return point_l; }
	}
	public double MouseX
	{
		get { return mouseX; }
	}
	public double MouseY
	{
		get { return mouseY; }
	}
}

	
