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
 * Copyright (C) 2018 Xavier de Blas
 */

using System;
using System.Text;


//Version 31 format with 3 values and possible a commit num like:
//1.8.0-70-kfjsdfjsl
public class Version31
{
	public int Major;
	public int Medium;
	public int Minor;
	public int Commit;
	private string versionStr;

	public Version31 (string versionStr)
	{
		this.versionStr = versionStr;

		Major = -1;
		Medium = -1;
		Minor = -1;
		Commit = -1;

		parseVersionString();
	}

	public override string ToString()
	{
		return string.Format("Major: {0}, Medium: {1}, Minor: {2}, Commit: {3}",
				Major, Medium, Minor, Commit);
	}

	private void parseVersionString()
	{
		string [] vFull = versionStr.Split(new char[] {'-'});
		if(vFull.Length > 1)
		{
			Commit = assignIfIsNumber(vFull[1]);
			parseVersionNumbers(vFull[0]);
		} else
			parseVersionNumbers(versionStr);
	}

	private void parseVersionNumbers(string versionNumStr)
	{
		string [] vFull = versionNumStr.Split(new char[] {'.'});
		if(vFull.Length == 3)
		{
			Major = assignIfIsNumber(vFull[0]);
			Medium = assignIfIsNumber(vFull[1]);
			Minor = assignIfIsNumber(vFull[2]);
		}
	}

	private int assignIfIsNumber(string str)
	{
		if(Util.IsNumber(str, false))
			return Convert.ToInt32(str);

		return -1;
	}

}

public class VersionCompare
{
	public enum ResultType { EQUAL, LOWER, GREATER }
	public ResultType Result;

	public VersionCompare(Version31 ver1, Version31 ver2)
	{
		Result = comparer(ver1, ver2);
	}

	private	ResultType comparer(Version31 ver1, Version31 ver2)
	{
		ResultType rType;

		rType = compare2(ver1.Major, ver2.Major);
		if(rType != ResultType.EQUAL)
			return rType;

		rType = compare2(ver1.Medium, ver2.Medium);
		if(rType != ResultType.EQUAL)
			return rType;

		rType = compare2(ver1.Minor, ver2.Minor);
		if(rType != ResultType.EQUAL)
			return rType;

		rType = compare2(ver1.Commit, ver2.Commit);
		if(rType != ResultType.EQUAL)
			return rType;

		return ResultType.EQUAL;
	}

	private ResultType compare2(int val1, int val2)
	{
		if(val1 > val2)
			return ResultType.GREATER;
		else if(val1 < val2)
			return ResultType.LOWER;
		else
			return ResultType.EQUAL;
	}

	public string ResultStr {
		get
		{
			if(Result == ResultType.EQUAL)
				//return Constants.GetTranslated(Constants.SoftwareUpdated);
				return Constants.SoftwareUpdatedStr();
			else if(Result == ResultType.LOWER)
				return Constants.SoftwareNeedUpdateStr();
			else
				return Constants.SoftwareNewerThanPublisedStr();
		}
	}
}

public class VersionCompareTests
{
	public VersionCompareTests()
	{
		LogB.Information("Starting version compare tests:");

		versionCompareTest("1.5.0", 	"1.5.1", 		VersionCompare.ResultType.LOWER);
		versionCompareTest("1.5.0", 	"1.5.21", 		VersionCompare.ResultType.LOWER);
		versionCompareTest("1.5.11", 	"1.5.21", 		VersionCompare.ResultType.LOWER);
		versionCompareTest("1.5.11", 	"1.5.2", 		VersionCompare.ResultType.GREATER);
		versionCompareTest("1.5.11", 	"1.5.11-20-hell", 	VersionCompare.ResultType.LOWER);
		versionCompareTest("1.5.12", 	"1.5.11-20-hell", 	VersionCompare.ResultType.GREATER);
		versionCompareTest("2.0.0", 	"1.0.0", 		VersionCompare.ResultType.GREATER);
		versionCompareTest("1.5.3-20-a","1.5.3-20-bbc", 	VersionCompare.ResultType.EQUAL);
		versionCompareTest("hello", 	"good morning", 	VersionCompare.ResultType.EQUAL);
	}

	private bool versionCompareTest(string v1Str, string v2Str, VersionCompare.ResultType rTypeExpected)
	{
		Version31 v1 = new Version31(v1Str);
		Version31 v2 = new Version31(v2Str);
		VersionCompare.ResultType rTypeFound = new VersionCompare(v1, v2).Result;

		if(rTypeFound == rTypeExpected)
		{
			LogB.Information(string.Format("Success compare of {0} with {1}", v1Str, v2Str));
			return true;
		}

		LogB.Information(string.Format("Failed compare of {0} with {1}, expecting: {2}, found: {3}",
					v1Str, v2Str, rTypeExpected.ToString(), rTypeFound.ToString() ));
		return false;
	}
}
