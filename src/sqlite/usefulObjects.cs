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
 * Copyright (C) 2017-2022   Xavier de Blas <xaviblas@gmail.com>
 */

using System;
using Mono.Unix;
using System.Collections.Generic; //List<T>

//class from 2.0 code that manages 3 lists an SQL stuff (for a combo)

public class LSqlEnTrans
{
	private string name;
	private List<string> l_sql;
	private int sqlDefault;
	private int sqlCurrent;
	private List<string> l_trans;

	public LSqlEnTrans (string name, List<string> l_sql, int sqlDefault, int sqlCurrent, List<string> l_en)
	{
		this.name = name;
		this.l_sql = l_sql;
		this.sqlDefault = sqlDefault;
		this.sqlCurrent = sqlCurrent;

		l_trans = new List<string>();
		foreach(string s in l_en)
			l_trans.Add(Catalog.GetString(s));
	}

	public void SetCurrentFromSQL (string newCurrent)
	{
		for(int i = 0; i < l_sql.Count; i ++)
			if(l_sql[i] == newCurrent)
			{
				sqlCurrent = i;
				return;
			}

		sqlCurrent = 0;
	}

	public void SetCurrentFromComboTranslated (string trString)
	{
		for(int i = 0; i < l_trans.Count; i ++)
			if(l_trans[i] == trString)
			{
				sqlCurrent = i;
				return;
			}

		sqlCurrent = 0;
	}

	private string getSqlDefaultName ()
	{
		return(l_sql[sqlDefault]);
	}
	private string getSqlCurrentName ()
	{
		return(l_sql[sqlCurrent]);
	}

	public string Name
	{
		get { return name; }
	}

	public string SqlDefaultName
	{
		get { return getSqlDefaultName(); }
	}

	public string SqlCurrentName
	{
		get { return getSqlCurrentName(); }
	}

	public int SqlCurrent
	{
		get { return sqlCurrent; }
	}

	public List<string> L_trans
	{
		get { return l_trans; }
	}

	public string TranslatedCurrent
	{
		get { return l_trans[sqlCurrent]; }
	}

}

public class SelectTypes
{
	public int Id;
	public string NameEnglish;
	public string NameTranslated;

	public SelectTypes()
	{
	}

	public SelectTypes(int id, string nameEnglish, string nameTranslated)
	{
		this.Id = id;
		this.NameEnglish = nameEnglish;
		this.NameTranslated = nameTranslated;
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

	public string ToSQLString ()
	{
		return string.Format("{0}:{1}:{2}:{3}",
				NameEnglish, Util.BoolToInt (StartIn), Util.BoolToInt (HasWeight), Description);
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

	public new string ToSQLString () //"new" to override inherited
	{
		return string.Format("{0}:{1}:{2}:{3}:{4}:{5}",
				NameEnglish, Util.BoolToInt (StartIn), Util.BoolToInt (HasWeight),
				Util.BoolToInt (JumpsLimited), Util.ConvertToPoint (FixedValue), Description);
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

public class SelectRunITypes : SelectRunTypes
{
	public bool TracksLimited;
	public int FixedValue;
	public bool Unlimited;
	public string DistancesString;

	public SelectRunITypes(string nameEnglish)
	{
		this.NameEnglish = nameEnglish;
	}

	public SelectRunITypes(int id, string nameEnglish, double distance,
			bool tracksLimited, int fixedValue, bool unlimited,
			string description, string distancesString)
	{
		this.Id = id;
		this.NameEnglish = nameEnglish;
		this.NameTranslated = Catalog.GetString(nameEnglish);
		this.Distance = distance;
		this.TracksLimited = tracksLimited;
		this.FixedValue = fixedValue;
		this.Unlimited = unlimited;
		this.Description = description;
		this.DistancesString = distancesString;
	}

	public static string RunIntervalTypeDistancesString (
			string runTypeEnglishName, List<object> selectRunITypes_l)
	{
		foreach(SelectRunITypes type in selectRunITypes_l)
			if(type.NameEnglish == runTypeEnglishName)
				return(type.DistancesString);

		return "";
	}

	//debug
	public override string ToString ()
	{
		return string.Format ("Id: {0}, NameEnglish: {1}, Distance: {2}, TracksLimited: {3}, " +
				"FixedValue: {4}, Unlimited: {5}, Description: {6}, DistancesString: {7}",
				Id, NameEnglish, Distance, TracksLimited, FixedValue, Unlimited, Description, DistancesString);
	}
}

public class LastJumpSimpleTypeParams
{
	public int uniqueID;
	public string name;
	public bool weightIsPercent;
	public double weightValue;
	public int fallmm;

	public LastJumpSimpleTypeParams(string name)
	{
		this.uniqueID = -1;
		this.name = name;
		this.weightIsPercent = true;
		this.weightValue = -1;  // has no weight
		this.fallmm = -1;	// startIn
	}

	public LastJumpSimpleTypeParams(int uniqueID, string name,
			bool weightIsPercent, double weightValue, int fallmm)
	{
		this.uniqueID = uniqueID;
		this.name = name;
		this.weightIsPercent = weightIsPercent;
		this.weightValue = weightValue;
		this.fallmm = fallmm;
	}

	public string ToSqlString()
	{
		string idString = "NULL";
		if(uniqueID != -1)
			idString = uniqueID.ToString();

		return string.Format("{0}, \"{1}\", {2}, {3}, {4}",
				idString,
				name,
				Util.BoolToInt(weightIsPercent),
				Util.ConvertToPoint(weightValue),
				fallmm);
	}

}

public class LastJumpRjTypeParams
{
	public int uniqueID;
	public string name;
	public int limitedValue; //how many jumps or how many seconds (or none: -1)
	public bool weightIsPercent;
	public double weightValue;
	public int fallmm;

	public LastJumpRjTypeParams(string name)
	{
		this.uniqueID = -1;
		this.name = name;
		this.limitedValue = -1;
		this.weightIsPercent = true;
		this.weightValue = -1;  // has no weight
		this.fallmm = -1;	// startIn
	}

	public LastJumpRjTypeParams(int uniqueID, string name, int limitedValue,
			bool weightIsPercent, double weightValue, int fallmm)
	{
		this.uniqueID = uniqueID;
		this.name = name;
		this.limitedValue = limitedValue;
		this.weightIsPercent = weightIsPercent;
		this.weightValue = weightValue;
		this.fallmm = fallmm;
	}

	public string ToSqlString()
	{
		string idString = "NULL";
		if(uniqueID != -1)
			idString = uniqueID.ToString();

		return string.Format("{0}, \"{1}\", {2}, {3}, {4}, {5}",
				idString,
				name,
				limitedValue,
				Util.BoolToInt(weightIsPercent),
				Util.ConvertToPoint(weightValue),
				fallmm);
	}

}
