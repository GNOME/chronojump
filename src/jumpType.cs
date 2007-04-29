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

public class JumpType : EventType
{
	protected bool startIn;
	protected bool hasWeight;
	protected bool isRepetitive;
	protected bool jumpsLimited;
	protected double fixedValue;	//0 no fixed value
	//protected string description;
	//protected bool isPredefined;
	protected bool unlimited;


	public JumpType() {
		type = Types.JUMP;
	}
	
	//predefined values
	public JumpType(string name) {
		type = Types.JUMP;
		this.name = name;
		
		//we cannot obtain values like has Weight
		this.isPredefined = false;
		
		unlimited = false;	//default value
		
		//if this changes, sqlite/jumpType.cs initialize tables should change
		if(name == "Free" || name == "SJ" || name == "CMJ" || name == "ABK" || name == "Rocket") {
			startIn 	= true;
			hasWeight 	= false;
			isRepetitive 	= false;
			jumpsLimited 	= false;
			fixedValue 	= 0;
			isPredefined	= true;
		} else if(name == "SJ+") {
			startIn 	= true;
			hasWeight 	= true;
			isRepetitive 	= false;
			jumpsLimited 	= false;
			fixedValue 	= 0;
			isPredefined	= true;
		} else if(name == "DJ") {
			startIn 	= false;
			hasWeight 	= false;
			isRepetitive 	= false;
			jumpsLimited 	= false;
			fixedValue 	= 0;
			isPredefined	= true;
		} else if(name == "RJ(j)") {
			startIn 	= false;
			hasWeight 	= false;
			isRepetitive 	= true;
			jumpsLimited 	= true;
			fixedValue 	= 0;
			isPredefined	= true;
		} else if(name == "RJ(t)") {
			startIn 	= false;
			hasWeight 	= false;
			isRepetitive 	= true;
			jumpsLimited 	= false;
			fixedValue 	= 0;
			isPredefined	= true;
		} else if(name == "RJ(unlimited)") {
			startIn 	= true;
			hasWeight 	= false;
			isRepetitive 	= true;
			jumpsLimited 	= true;	//will finish in a concrete jump, not in a concrete second
			fixedValue 	= -1;	//don't ask for limit of jumps or seconds
			isPredefined	= true;
			unlimited 	= true;
		} else if(name == "triple jump") {
			startIn 	= false;
			hasWeight 	= false;
			isRepetitive 	= true;
			jumpsLimited 	= true;
			fixedValue 	= 3;
			isPredefined	= true;
		}
	}
	
	
	public JumpType(string name, bool startIn, bool hasWeight, 
			bool isRepetitive, bool jumpsLimited, double fixedValue, bool unlimited)
	{
		type = Types.JUMP;
		this.name 	= name;
		this.startIn 	= startIn;
		this.hasWeight 	= hasWeight;
		this.isRepetitive = isRepetitive;
		this.jumpsLimited = jumpsLimited;
		this.fixedValue = fixedValue;
		this.unlimited = unlimited;

		//we can obtain values like has Weight
		this.isPredefined	= true;
	}

	public bool StartIn
	{
		get { return startIn; }
		set { startIn = value; }
	}
	
	public bool HasWeight
	{
		get { 
			if(isPredefined) {
				return hasWeight; 
			} else {
				return SqliteJumpType.HasWeight(name);
			}
		}
		set { hasWeight = value; }
	}
	
	public bool IsRepetitive
	{
		get { return isRepetitive; }
		set { isRepetitive = value; }
	}
	
	public bool JumpsLimited
	{
		get { return jumpsLimited; }
		set { jumpsLimited = value; }
	}
	
	public double FixedValue
	{
		get { return fixedValue; }
		set { fixedValue = value; }
	}
	
	public bool Unlimited
	{
		get { return unlimited; }
	}
}

