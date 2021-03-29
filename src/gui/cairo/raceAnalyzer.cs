
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
 *  Copyright (C) 2021   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
using System.Collections.Generic; //List
using Gtk;
using Cairo;


public class CairoGraphRaceAnalyzer : CairoXY
{
	/*
	//constructor when there are no points
	public CairoGraphRaceAnalyzer (DrawingArea area, string jumpType, string font)//, string title, string jumpType, string date)
	{
		this.area = area;

		initGraph(font);

		g.SetFontSize(16);
		printText(area.Allocation.Width /2, area.Allocation.Height /2, 24, textHeight,
				needToExecuteJumpsStr + " " + jumpType + ".", g, alignTypes.CENTER);

		endGraphDisposing(g);
	}
	*/

	//to avoid to have new data on PassData while the for is working on plotRealPoints
	static bool doing;
	//regular constructor
	public CairoGraphRaceAnalyzer (
//			List<PointF> point_l,
			DrawingArea area, string title,
			string yVariable, string yUnits)
	{
//		this.point_l = point_l;
		this.area = area;
		this.title = title;
		this.colorBackground = colorFromGdk(Config.ColorBackground); //but note if we are using system colors, this will not match

		xVariable = timeStr;
		this.yVariable = yVariable;
		xUnits = "s";
		this.yUnits = yUnits;
		
		doing = false;
	}

	public override bool PassData (List<PointF> point_l)
	{
		if(doing)
			return false;
		else
			doing = true;

		this.point_l = point_l;
		return true;
	}

	public override void Do (string font)
	{
		LogB.Information("at RaceAnalyzerGraph.Do");
		initGraph(font, .9);

		//because point_l is updated while foreach in findPointMaximums() and plotRealPoints()
		//TODO: on realtime do something better in order to just pass the new points and redo the graph just if margins changed
		//this new method will not have problems of changing the point_l list while iterating it
		try {
			findPointMaximums(false);
			//TODO: have a way to pass the x min max if we want to have two graphs with same x
			paintGrid(gridTypes.BOTH, true);
			paintAxis();

			pointsRadius = 1;
			plotRealPoints(false);
		} catch {}

		endGraphDisposing(g);
		doing = false;
	}

	protected override void writeTitle()
	{
	}

}
