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
 *  Copyright (C) 2019   Xavier de Blas <xaviblas@gmail.com>, Jordi Rodeiro <jordirodeiro@gmail.com>
 */

using System;
using System.Collections.Generic; //List

public class JumpsDjOptimalFallingHeight
{
	//constructor
	public JumpsDjOptimalFallingHeight()
	{
	}
	
	public void Calculate (int personID, int sessionID)
	{
		//1 get data
                List<Jump> jump_l = SqliteJump.SelectDJa (personID, sessionID);

		//2 convert to list of Point
		List<Point> point_l = new List<Point>();
                foreach(Jump j in jump_l)
			point_l.Add(new Point(
						j.Fall,
						Util.GetHeightInCentimeters(j.Tv)
						));

		//3 get LeastSquares
		LeastSquares ls = new LeastSquares();
		ls.Calculate(point_l);

		//4 print data
		if(ls.CalculatedCoef)
			LogB.Information(string.Format("coef = {0} {1} {2}",
						ls.Coef[0], ls.Coef[1], ls.Coef[2]));

		if(ls.CalculatedMaxY)
			LogB.Information(string.Format("MaxY = {0}", ls.MaxY));
	}
}
