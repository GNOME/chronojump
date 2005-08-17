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
	protected bool unlimited;

	//predefined values
	public RunType(string name) {
		this.name = name;

		unlimited = false;	//default value
		
		//if this changes, sqlite/runType.cs initialize tables should change
		//
		//no interval
		if(name == "Custom") {
			hasIntervals 	= false; 
			distance 	= 0;
			tracksLimited 	= false;
			fixedValue 	= 0;
		} else if(name == "20m") {
			hasIntervals 	= false; 
			distance 	= 20;
			tracksLimited 	= false;
			fixedValue 	= 0;
		} else if(name == "100m") {
			hasIntervals 	= false; 
			distance 	= 100;
			tracksLimited 	= false;
			fixedValue 	= 0;
		} else if(name == "200m") {
			hasIntervals 	= false; 
			distance 	= 200;
			tracksLimited 	= false;
			fixedValue 	= 0;
		} else if(name == "400m") {
			hasIntervals 	= false; 
			distance 	= 400;
			tracksLimited 	= false;
			fixedValue 	= 0;
		} else if(name == "1000m") {
			hasIntervals 	= false; 
			distance 	= 1000;
			tracksLimited 	= false;
			fixedValue 	= 0;
		} else if(name == "2000m") {
			hasIntervals 	= false; 
			distance 	= 2000;
			tracksLimited 	= false;
			fixedValue 	= 0;
		} //interval
		else if(name == "byLaps") {
			hasIntervals 	= true; 
			distance 	= 0;
			tracksLimited 	= true;
			fixedValue 	= 0;
		} else if(name == "byTime") {
			hasIntervals 	= true; 
			distance 	= 0;
			tracksLimited 	= false;
			fixedValue 	= 0;
		} else if(name == "unlimited") {
			hasIntervals 	= true; 
			distance 	= 0;
			tracksLimited 	= false;	//limited by time
			fixedValue 	= 0;
			unlimited 	= true;
		} else if(name == "20m10times") {
			hasIntervals 	= true; 
			distance 	= 20;
			tracksLimited 	= true;
			fixedValue 	= 10;
		} else if(name == "7m30seconds") {
			hasIntervals 	= true; 
			distance 	= 7;
			tracksLimited 	= false;
			fixedValue 	= 30;
		} else if(name == "20m endurance") {
			hasIntervals 	= true; 
			distance 	= 20;
			tracksLimited 	= false;
			fixedValue 	= 0;
			unlimited 	= true;
		}
	}
	
	
	public RunType(string name, bool hasIntervals, double distance, 
			bool tracksLimited, int fixedValue, bool unlimited)
	{
		this.name 	= name;
		this.hasIntervals 	= hasIntervals;
		this.distance 	= distance;
		this.tracksLimited = tracksLimited;
		this.fixedValue = fixedValue;
		this.unlimited = unlimited;
	}
	
	public string Name
	{
		get { return name; }
	}
	
	public double Distance
	{
		get { return distance; }
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
	
	public bool Unlimited
	{
		get { return unlimited; }
	}
}

