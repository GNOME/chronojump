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

public class JumpType 
{
	protected string name;
	protected bool startIn;
	protected bool hasWeight;
	protected bool isRepetitive;
	protected bool jumpsLimited;
	protected double fixedValue;	//0 no fixed value
	//protected string description;

	//predefined values
	public JumpType(string name) {
		this.name = name;
		//if this changes, sqlite/jumpType.cs initialize tables should change
		if(name == "SJ" || name == "CMJ" || name == "ABK") {
			startIn 	= true;
			hasWeight 	= false;
			isRepetitive 	= false;
			jumpsLimited 	= false;
			fixedValue 	= 0;
		} else if(name == "SJ+") {
			startIn 	= true;
			hasWeight 	= true;
			isRepetitive 	= false;
			jumpsLimited 	= false;
			fixedValue 	= 0;
		} else if(name == "DJ") {
			startIn 	= false;
			hasWeight 	= false;
			isRepetitive 	= false;
			jumpsLimited 	= false;
			fixedValue 	= 0;
		} else if(name == "RJ(j)") {
			startIn 	= false;
			hasWeight 	= false;
			isRepetitive 	= true;
			jumpsLimited 	= true;
			fixedValue 	= 0;
		} else if(name == "RJ(t)") {
			startIn 	= false;
			hasWeight 	= false;
			isRepetitive 	= true;
			jumpsLimited 	= false;
			fixedValue 	= 0;
		} else if(name == "triple jump") {
			startIn 	= false;
			hasWeight 	= false;
			isRepetitive 	= true;
			jumpsLimited 	= true;
			fixedValue 	= 3;
		}
	}
	
	
	public JumpType(string name, bool startIn, bool hasWeight, 
			bool isRepetitive, bool jumpsLimited, double fixedValue)
	{
		this.name 	= name;
		this.startIn 	= startIn;
		this.hasWeight 	= hasWeight;
		this.isRepetitive = isRepetitive;
		this.jumpsLimited = jumpsLimited;
		this.fixedValue = fixedValue;
	}
	
	public string Name
	{
		get { return name; }
	}
	
	public bool StartIn
	{
		get { return startIn; }
	}
	
	public bool HasWeight
	{
		get { return hasWeight; }
	}
	
	public bool IsRepetitive
	{
		get { return isRepetitive; }
	}
	
	public bool JumpsLimited
	{
		get { return jumpsLimited; }
	}
	
	public double FixedValue
	{
		get { return fixedValue; }
	}
}

