/*
 * This file is part of ChronoJump
 *
 * Chronojump is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or   
 *    (at your option) any later version.
 *    
 * Chronojump is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the 
 *    GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 *  along with this program; if not, write to the Free Software
 *   Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 *
 * Copyright (C) 2016   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
using Mono.Unix;

public class SelectTypes
{
	public int Id;
	public string NameEnglish;
	public string NameTranslated;

	public SelectTypes()
	{
	}
}

public class SelectJumpTypes : SelectTypes
{
	public bool StartIn;
	public bool HasWeight;
	public string Description;

	//needed for inheritance
	public SelectJumpTypes()
	{
	}

	public SelectJumpTypes(string nameEnglish)
	{
		this.NameEnglish = nameEnglish;
	}

	public SelectJumpTypes(int id, string nameEnglish, bool startIn, bool hasWeight, string description)
	{
		this.Id = id;
		this.NameEnglish = nameEnglish;
		this.NameTranslated = Catalog.GetString(nameEnglish);
		this.StartIn = startIn;
		this.HasWeight = hasWeight;
		this.Description = description;
	}
}

public class SelectJumpRjTypes : SelectJumpTypes
{
	public bool JumpsLimited;
	public double FixedValue;

	public SelectJumpRjTypes(string nameEnglish)
	{
		this.NameEnglish = nameEnglish;
	}

	public SelectJumpRjTypes(int id, string nameEnglish, bool startIn, bool hasWeight, bool jumpsLimited, double fixedValue, string description)
	{
		this.Id = id;
		this.NameEnglish = nameEnglish;
		this.NameTranslated = Catalog.GetString(nameEnglish);
		this.StartIn = startIn;
		this.HasWeight = hasWeight;
		this.JumpsLimited = jumpsLimited;
		this.FixedValue = fixedValue;
		this.Description = description;
	}
}

public class SelectRunTypes : SelectTypes
{
	public double Distance;
	public string Description;

	//needed for inheritance
	public SelectRunTypes()
	{
	}

	public SelectRunTypes(string nameEnglish)
	{
		this.NameEnglish = nameEnglish;
	}

	public SelectRunTypes(int id, string nameEnglish, double distance, string description)
	{
		this.Id = id;
		this.NameEnglish = nameEnglish;
		this.NameTranslated = Catalog.GetString(nameEnglish);
		this.Distance = distance;
		this.Description = description;
	}
}

