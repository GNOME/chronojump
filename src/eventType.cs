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

public class EventType 
{
	public enum Types {
		JUMP, RUN, PULSE, REACTIONTIME, MULTICHRONOPIC, FORCESENSOR
	}

	protected Types type; //jump, run, reactionTime, pulse
	
	protected int uniqueID;
	protected string name;
	protected bool isPredefined;
	protected string imageFileName;
	protected string description;
	protected string longDescription; //info for "test image and description" window

	public EventType() {
		longDescription = ""; //needed initalization because is not defined in lots of events
	}

	public EventType(string name) {
		longDescription = ""; //needed initalization because is not defined in lots of events
	}

	public override string ToString() {
		return type + ", " + name + ", " + isPredefined + ", " + description;
	}

	public Types Type
	{
		get { return type; }
	}
	
	public virtual bool FindIfIsPredefined() {
		string [] predefinedTests = {
		};

		foreach(string search in predefinedTests)
			if(this.name == search)
				return true;

		return false;
	}
	
	/*
	 * defined on webservice
	 */
	public string Name
	{
		get { return name; }
		set { name = value; }
	}
	
	public string Description
	{
		get { return description; }
		set { description = value; }
	}

	public bool IsPredefined
	{
		get { return isPredefined; }
		set { isPredefined = value; }
	}
	
	public string ImageFileName
	{
		get { return imageFileName; }
		set { imageFileName = value; }
	}
	
	public string LongDescription
	{
		get { return longDescription; }
	}
	
	public bool HasLongDescription
	{
		get {
			if(longDescription != "")
				return true;
			else
				return false;
		}
	}

}

