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
 *  Copyright (C) 2004-2022   Xavier de Blas <xaviblas@gmail.com>
 */

using System;
using System.Data;
using Mono.Unix;

public class JumpType : EventType
{
	protected bool startIn; //in the special case of Dj where falling height is calculated using a previous jump, startIn is false (but starts in)
	protected bool hasWeight;
	protected bool isRepetitive;
	protected bool jumpsLimited;
	protected double fixedValue;	//0 no fixed value //why this is a double and not an int?
	protected bool unlimited;


	public JumpType() {
		type = Types.JUMP;
	}

	
	public override bool FindIfIsPredefined() {
		string [] predefinedTests = {
			"Free", "SJ", "SJl",
			"CMJ", "CMJl", "slCMJleft", "slCMJright",
			"ABK", "ABKl", "DJa", "DJna",
			"Rocket", "TakeOff", "TakeOffWeight",
			"RJ(j)", "RJ(t)", "RJ(unlimited)",
			"RJ(hexagon)", "triple jump"
		};

		foreach(string search in predefinedTests)
			if(this.name == search)
				return true;

		return false;
	}
	

	//predefined values
	public JumpType(string name) {
		type = Types.JUMP;
		this.name = name;
		
		//we cannot obtain values like has Weight
		
		unlimited = false;	//default value
		imageFileName = "";
		
		//if this changes, sqlite/jumpType.cs initialize tables should change
		if(name == "Free" || name == "SJ" || name == "CMJ" || 
				name == "slCMJleft" || name == "slCMJright" || 
				name == "ABK" || name == "Rocket") {
			startIn 	= true;
			hasWeight 	= false;
			isRepetitive 	= false;
			jumpsLimited 	= false;
			fixedValue 	= 0;
			if(name == "Free") {
				imageFileName = "jump_free.png";
				description	= Catalog.GetString("Free Jump");
				longDescription	= Catalog.GetString("Simple jump with no special technique");
			} 
			else if (name == "SJ") {
				imageFileName = "jump_sj.png";
				description	= Catalog.GetString("Squat Jump");
				longDescription  = "A squat jump is where the jumper starts from a hips and knee flexion (90º), and he extends the knees and hips to jump vertically up off the ground. There is no countermovement to the floor.\n" +
					"Hands are on the hips";
			} 
			else if (name == "CMJ") {
				imageFileName = "jump_cmj.png";
				description	= Catalog.GetString("CounterMovement Jump");
				longDescription	= 
					"A countermovement jump is where the jumper starts from an upright standing position, makes a preliminary downward movement by flexing at the knees and hips, then immediately extends the knees and hips again to jump vertically up off the ground. Such a movement makes use of the ‘stretch-shorten cycle’, where the muscles are ‘pre-stretched’ before shortening in the desired direction." + "\n\n" +
					"http://people.brunel.ac.uk/~spstnpl/" + "BiomechanicsAthletics/VerticalJumping.htm" + "\n" +
					"Hands are on the hips";

			} 
			else if (name == "slCMJleft" || name == "slCMJright") {
				description	= Catalog.GetString("Single-leg CounterMovement Jump");
				longDescription	= "";
			} 
			else if (name == "ABK") {
				description	= Catalog.GetString("Abalakov Jump");
				imageFileName = "jump_abk.png";
			} else if (name == "Rocket") {
				description	= Catalog.GetString("Rocket Jump");
				imageFileName = "jump_rocket.png";
			}
		} else if(name == "SJl" || name == "CMJl" || name == "ABKl") {
			startIn 	= true;
			hasWeight 	= true;
			isRepetitive 	= false;
			jumpsLimited 	= false;
			fixedValue 	= 0;
			if(name == "SJl") {
				description	= Catalog.GetString("Squat Jump with extra weight");
				imageFileName = "jump_sj_l.png";
			} else if(name == "CMJl") {
				description	= Catalog.GetString("CounterMovement Jump with extra weight");
				imageFileName = "jump_cmj_l.png";
			} else if(name == "ABKl") {
				description	= Catalog.GetString("Abalakov Jump with extra weight");
				imageFileName = "jump_abk_l.png";
			}
		} else if(name == Constants.TakeOffName) { //special case, will record only TC
			startIn 	= false;
			hasWeight 	= false;
			isRepetitive 	= false; //for repetitive
			jumpsLimited 	= false; //for repetitive
			fixedValue 	= 0; //for repetitive
			description	= Catalog.GetString("Take off");
			imageFileName = "take_off.png"; 
		} else if(name == Constants.TakeOffWeightName) { //special case, will record only TC
			startIn 	= false;
			hasWeight 	= true;
			isRepetitive 	= false; //for repetitive
			jumpsLimited 	= false; //for repetitive
			fixedValue 	= 0; //for repetitive
			description	= Catalog.GetString("Take off with weight");
			imageFileName = "";
		} else if(name == "DJ") { //not used for end user, but used in software to initially define jump
			startIn 	= false;
			hasWeight 	= false;
			isRepetitive 	= false;
			jumpsLimited 	= false;
			fixedValue 	= 0;
			description	= Catalog.GetString("DJ Jump");
			imageFileName = "jump_dj.png";
		} else if(name == "DJa") { //DJ using arms
			startIn 	= false;
			hasWeight 	= false;
			isRepetitive 	= false;
			jumpsLimited 	= false;
			fixedValue 	= 0;
			description	= Catalog.GetString("DJ Jump using arms");
			imageFileName = "jump_dj_a.png";
		} else if(name == "DJna") { //DJ Not using arms
			startIn 	= false;
			hasWeight 	= false;
			isRepetitive 	= false;
			jumpsLimited 	= false;
			fixedValue 	= 0;
			description	= Catalog.GetString("DJ Jump without using arms");
			imageFileName = "jump_dj.png";
		} else if(name == "RJ(j)") {
			startIn 	= false;
			hasWeight 	= false;
			isRepetitive 	= true;
			jumpsLimited 	= true;
			fixedValue 	= 0;
			imageFileName = "jump_rj.png";
			description	= Catalog.GetString("Reactive Jump limited by Jumps");
			longDescription	= "";
		} else if(name == "RJ(t)") {
			startIn 	= false;
			hasWeight 	= false;
			isRepetitive 	= true;
			jumpsLimited 	= false;
			fixedValue 	= 0;
			description	= Catalog.GetString("Reactive Jump limited by Time");
			imageFileName = "jump_rj.png";
		} else if(name == "RJ(unlimited)") {
			startIn 	= true;
			hasWeight 	= false;
			isRepetitive 	= true;
			jumpsLimited 	= true;	//will finish in a concrete jump, not in a concrete second
			fixedValue 	= -1;	//don't ask for limit of jumps or seconds
			unlimited 	= true;
			description	= Catalog.GetString("Reactive Jump unlimited (until finish button is clicked)");
			imageFileName = "jump_rj_in.png";
		} else if(name == Constants.RunAnalysisName) { //like a Rj(unlimited). but starting out
			//Josep Ma Padullés test
			//TODO: check if this code is used, because that test is a multiChronopic now
			startIn 	= false;
			hasWeight 	= false;
			isRepetitive 	= true;
			jumpsLimited 	= true;	//will finish in a concrete jump, not in a concrete second
			fixedValue 	= -1;	//don't ask for limit of jumps or seconds
			unlimited 	= true;
			description	= Catalog.GetString("Run between two photocells recording contact and flight times in contact platform/s.") + 
				" " + Catalog.GetString("Until finish button is clicked.");
			imageFileName = "jump_rj_in.png";
		} else if(name == "RJ(hexagon)") {
			startIn 	= true;
			hasWeight 	= false;
			isRepetitive 	= true;
			jumpsLimited 	= true;	//will finish in a concrete jump, not in a concrete second
			fixedValue 	= 18;	//don't ask for limit of jumps or seconds
			unlimited 	= false;
			description	= Catalog.GetString("Reactive Jump on a hexagon until three full revolutions are done");
			imageFileName = "jump_rj_hexagon.png";
		} else if(name == "triple jump") {
			startIn 	= false;
			hasWeight 	= false;
			isRepetitive 	= true;
			jumpsLimited 	= true;
			fixedValue 	= 3;
			description	= Catalog.GetString("Triple jump");
			imageFileName = "jump_rj.png";
		}

		isPredefined = FindIfIsPredefined();
	}
	
	
	public JumpType(string name, bool startIn, bool hasWeight, 
			bool isRepetitive, bool jumpsLimited, double fixedValue, bool unlimited, string description, string imageFileName)
	{
		type = Types.JUMP;
		this.name 	= name;
		this.startIn 	= startIn;
		this.hasWeight 	= hasWeight;
		this.isRepetitive = isRepetitive;
		this.jumpsLimited = jumpsLimited;
		this.fixedValue = fixedValue;
		this.unlimited = unlimited;
		this.description = description;
		this.imageFileName = imageFileName;

		//we can obtain values like has Weight and has Fall
		isPredefined = FindIfIsPredefined();
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
				if(isRepetitive)
					return SqliteJumpType.HasWeight("jumpRjType", name);
				else
					return SqliteJumpType.HasWeight("jumpType", name);
			}
		}
		set { hasWeight = value; }
	}
	
	public bool HasFall (bool compujump)
	{
		if(name == Constants.TakeOffName || name == Constants.TakeOffWeightName)
			return false;

		if(isPredefined && ! compujump) {
			return ! startIn;
		} else {
			if(isRepetitive)
				return SqliteJumpType.HasFall("jumpRjType", name);
			else
				return SqliteJumpType.HasFall("jumpType", name);
		}
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
		get {
			if(isPredefined) {
				return unlimited;
			} else {
				if(! isRepetitive)
					return false;
				else
					return SqliteJumpType.IsUnlimited(name);
			}
		}
	}
}

