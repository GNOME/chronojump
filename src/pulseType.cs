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
 *  Copyright (C) 2004-2017   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
using System.Data;
using Mono.Unix;

public class PulseType : EventType
{
	private double fixedPulse; //-1: not fixed, 0,344: 0,344 seconds between pulses
	private int totalPulsesNum; //-1: not fixed (unlimited), 5: 5 times
	//private string description; //currently unused

	public PulseType() {
		type = Types.PULSE;
	}
	
	//predefined values
	public PulseType(string name) {
		type = Types.PULSE;
		this.name = name;
		
		//if this changes, sqlite/pulseType.cs initialize table should change
		if(name == "Free") {
			fixedPulse = -1;
			totalPulsesNum = -1;
			imageFileName = "pulse_free.png";
			description = Catalog.GetString("Pulse free");
			longDescription = 
				Catalog.GetString("User executes a pulse without a predefined tempo. <i>Difference</i> will show the difference between a pulse and it's preceding pulse.");

		} else if(name == "Custom") {
			fixedPulse = -1;
			totalPulsesNum = -1;
			imageFileName = "pulse_custom.png";
			description = Catalog.GetString("Pulse custom");
			longDescription = 
				Catalog.GetString("User executes a pulse trying to follow a predefined tempo and optionally with a fixed number of pulsations. <i>Difference</i> will show the difference between a a pulse and the predefined pulse.");

		}
	}
	
	public PulseType(string name, double fixedPulse, int totalPulsesNum)
	{
		type = Types.PULSE;
		this.name 	= name;
		this.fixedPulse = fixedPulse;
		this.totalPulsesNum = totalPulsesNum;
	}

	public double FixedPulse
	{
		get { return fixedPulse; }
		set { fixedPulse = value; }
	}
	
	public int TotalPulsesNum
	{
		get { return totalPulsesNum; }
		set { totalPulsesNum = value; }
	}
}
