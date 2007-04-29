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
using Mono.Unix;

public class RunType : EventType 
{
	protected bool hasIntervals;
	protected double distance;
	protected bool tracksLimited;
	protected int fixedValue;
	protected bool unlimited;

	public RunType() {
		type = Types.RUN;
	}
	
	//predefined values
	public RunType(string name) {
		type = Types.RUN;
		this.name = name;

		this.isPredefined = false;

		unlimited = false;	//default value
		imageFileName = "";
		
		//if this changes, sqlite/runType.cs initialize tables should change
		//
		//no interval
		if(name == "Custom") {
			hasIntervals 	= false; 
			distance 	= 0;
			tracksLimited 	= false;
			fixedValue 	= 0;
			isPredefined	= true;
			description	= Catalog.GetString("variable distance running");
		} else if(name == "20m") {
			hasIntervals 	= false; 
			distance 	= 20;
			tracksLimited 	= false;
			fixedValue 	= 0;
			isPredefined	= true;
			description	= "";
			description	= Catalog.GetString("run 20 meters");
		} else if(name == "100m") {
			hasIntervals 	= false; 
			distance 	= 100;
			tracksLimited 	= false;
			fixedValue 	= 0;
			isPredefined	= true;
			description	= Catalog.GetString("run 100 meters");
		} else if(name == "200m") {
			hasIntervals 	= false; 
			distance 	= 200;
			tracksLimited 	= false;
			fixedValue 	= 0;
			isPredefined	= true;
			description	= Catalog.GetString("run 200 meters");
		} else if(name == "400m") {
			hasIntervals 	= false; 
			distance 	= 400;
			tracksLimited 	= false;
			fixedValue 	= 0;
			isPredefined	= true;
			description	= Catalog.GetString("run 400 meters");
		} else if(name == "1000m") {
			hasIntervals 	= false; 
			distance 	= 1000;
			tracksLimited 	= false;
			fixedValue 	= 0;
			isPredefined	= true;
			description	= Catalog.GetString("run 1000 meters");
		} else if(name == "2000m") {
			hasIntervals 	= false; 
			distance 	= 2000;
			tracksLimited 	= false;
			fixedValue 	= 0;
			isPredefined	= true;
			description	= Catalog.GetString("run 2000 meters");
		} //agility
		else if(name == "Agility-20Yard") {
			hasIntervals 	= false; 
			distance 	= 18.28;
			tracksLimited 	= false;
			fixedValue 	= 0;
			isPredefined	= true;
			description	= Catalog.GetString("20Yard Agility test");
			imageFileName = "agility_20yard.png";
		} else if(name == "Agility-505") {
			hasIntervals 	= false; 
			distance 	= 10;
			tracksLimited 	= false;
			fixedValue 	= 0;
			isPredefined	= true;
			description	= Catalog.GetString("505 Agility test");
			imageFileName = "agility_505.png";
		} else if(name == "Agility-Illinois") {
			hasIntervals 	= false; 
			distance 	= 60;
			tracksLimited 	= false;
			fixedValue 	= 0;
			isPredefined	= true;
			description	= Catalog.GetString("Illinois Agility test");
			imageFileName = "agility_illinois.png";
		} else if(name == "Agility-Shuttle-Run") {
			hasIntervals 	= false; 
			distance 	= 40;
			tracksLimited 	= false;
			fixedValue 	= 0;
			isPredefined	= true;
			description	= Catalog.GetString("Shuttle Run Agility test");
			imageFileName = "agility_shuttle.png";
		} else if(name == "Agility-ZigZag") {
			hasIntervals 	= false; 
			distance 	= 17.6;
			tracksLimited 	= false;
			fixedValue 	= 0;
			isPredefined	= true;
			description	= Catalog.GetString("ZigZag Agility test");
			imageFileName = "agility_zigzag.png";
		} //interval
		else if(name == "byLaps") {
			hasIntervals 	= true; 
			distance 	= 0;
			tracksLimited 	= true;
			fixedValue 	= 0;
			isPredefined	= true;
			description	= Catalog.GetString("Run n laps x distance");
		} else if(name == "byTime") {
			hasIntervals 	= true; 
			distance 	= 0;
			tracksLimited 	= false;
			fixedValue 	= 0;
			isPredefined	= true;
			description	= Catalog.GetString("Make max laps in n seconds");
		} else if(name == "unlimited") {
			hasIntervals 	= true; 
			distance 	= 0;
			tracksLimited 	= false;	//limited by time
			fixedValue 	= 0;
			unlimited 	= true;
			isPredefined	= true;
			description	= Catalog.GetString("Continue running in n distance");
		} else if(name == "20m10times") {
			hasIntervals 	= true; 
			distance 	= 20;
			tracksLimited 	= true;
			fixedValue 	= 10;
			isPredefined	= true;
			description	= Catalog.GetString("Run 10 times a 20m distance");
		} else if(name == "7m30seconds") {
			hasIntervals 	= true; 
			distance 	= 7;
			tracksLimited 	= false;
			fixedValue 	= 30;
			isPredefined	= true;
			description	= Catalog.GetString("Make max laps in 30 seconds");
		} else if(name == "20m endurance") {
			hasIntervals 	= true; 
			distance 	= 20;
			tracksLimited 	= false;
			fixedValue 	= 0;
			unlimited 	= true;
			isPredefined	= true;
			description	= Catalog.GetString("Continue running in 20m distance");
		}
	}
	
	
	public RunType(string name, bool hasIntervals, double distance, 
			bool tracksLimited, int fixedValue, bool unlimited, string description, string imageFileName)
	{
		type = Types.RUN;
		this.name 	= name;
		this.hasIntervals 	= hasIntervals;
		this.distance 	= distance;
		this.tracksLimited = tracksLimited;
		this.fixedValue = fixedValue;
		this.unlimited = unlimited;
		this.description = description;
		this.imageFileName = imageFileName;
		
		this.isPredefined	= true;
	}

	public double Distance
	{
		get { 
			if(isPredefined) {
				return distance; 
			} else {
				return SqliteRunType.Distance(name);
			}
		}
		set { distance = value; }
	}
	
	public bool HasIntervals
	{
		get { return hasIntervals; }
		set { hasIntervals = value; }
	}
	
	public bool TracksLimited
	{
		get { return tracksLimited; }
		set { tracksLimited = value; }
	}
	
	public int FixedValue
	{
		get { return fixedValue; }
		set { fixedValue = value; }
	}
	
	public bool Unlimited
	{
		get { return unlimited; }
		set { unlimited = value; }
	}
}

