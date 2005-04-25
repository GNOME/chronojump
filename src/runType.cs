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
 * http://www.xdeblas.com, http://www.deporteyciencia.com (parleblas)
 */

using System;
using System.Data;

public class RunType 
{
	protected string name;
	protected bool hasIntervals;
	protected double distance;
	protected bool tracksLimited;
	protected int fixedValue;

	//predefined values
	public RunType(string name) {
		this.name = name;
		
		//if this changes, sqlite/runType.cs initialize tables should change
		if(name == "Free") {
			hasIntervals 	= false; //if it's a runInterval
			distance 	= 0;
			tracksLimited 	= false;
			fixedValue 	= 0;
		} else if(name == "20m") {
			hasIntervals 	= false; //if it's a runInterval
			distance 	= 20;
			tracksLimited 	= false;
			fixedValue 	= 0;
		} else if(name == "20m10times") {
			hasIntervals 	= true; //if it's a runInterval
			distance 	= 20;
			tracksLimited 	= true;
			fixedValue 	= 10;
		} else if(name == "7m30seconds") {
			hasIntervals 	= true; //if it's a runInterval
			distance 	= 7;
			tracksLimited 	= false;
			fixedValue 	= 30;
		} else if(name == "20m endurance") {
			hasIntervals 	= true; //if it's a runInterval
			distance 	= 20;
			tracksLimited 	= false;
			fixedValue 	= 0;
		}
	}
	
	
	public RunType(string name, bool hasIntervals, double distance, 
			bool tracksLimited, int fixedValue)
	{
		this.name 	= name;
		this.hasIntervals 	= hasIntervals;
		this.distance 	= distance;
		this.tracksLimited = tracksLimited;
		this.fixedValue = fixedValue;
	}
	
	public string Name
	{
		get { return name; }
	}
	
	
	public bool HasIntervals
	{
		get { return hasIntervals; }
	}
	
	public bool TracksLimited
	{
		get { return tracksLimited; }
	}
	
	public double FixedValue
	{
		get { return fixedValue; }
	}
}

