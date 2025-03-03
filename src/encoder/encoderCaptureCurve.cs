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
 *  Copyright (C) 2004-2024   Xavier de Blas <xaviblas@gmail.com>
 */

using System;
using System.Collections; //ArrayList

public class EncoderCaptureCurve
{
	public bool up;
	public int startFrame;
        public int endFrame;

	public EncoderCaptureCurve(int startFrame, int endFrame)
	{
		this.startFrame = startFrame;
		this.endFrame = endFrame;
	}
	
	public string DirectionAsString() {
		if(up)
			return "UP";
		else
			return "DOWN";
	}

	public override string ToString()
	{
		return "ECC: " + up.ToString() + ";" + startFrame.ToString() + ";" + endFrame.ToString();
	}

	~EncoderCaptureCurve() {}
}

public class EncoderCaptureCurveArray
{
	public ArrayList ecc;	//each of the EncoderCaptureCurve
	public int curvesAccepted; //starts at int 0. How many ecc have been accepted (will be rows in treeview_encoder_capture_curves)
	
	public EncoderCaptureCurveArray() {
		ecc = new ArrayList(0);
		curvesAccepted = 0;
	}

	~EncoderCaptureCurveArray() {}
}
