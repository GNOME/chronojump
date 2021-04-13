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
 *  Copyright (C) 2004-2020   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
using System.Text; //StringBuilder
using System.Collections; //ArrayList
using Mono.Unix;

//this class tries to be a space for methods that are used in different classes
public class UtilDate
{
	//comes datetime
	//insert in sql like YYYY-MM-DD (with always all digits)
	//we use Year, Month and Day for not having any problem with locale
	public static string ToSql (DateTime dt)
	{
		return UtilAll.DigitsCreate(dt.Year,4) + "-" + 
			UtilAll.DigitsCreate(dt.Month,2) + "-" + 
			UtilAll.DigitsCreate(dt.Day,2);
	}
	
	//records date & time, useful to backup database without having strange chars on filename
	//used also on SQL when time wants to be stored also
	public static string ToFile ()
	{
		return ToFile(DateTime.Now);
	}
	public static string ToFile (DateTime dt)
	{
		return UtilAll.DigitsCreate(dt.Year,4) + "-" + 
			UtilAll.DigitsCreate(dt.Month,2) + "-" + 
			UtilAll.DigitsCreate(dt.Day,2) + "_" +
			UtilAll.DigitsCreate(dt.Hour,2) + "-" + 
			UtilAll.DigitsCreate(dt.Minute,2) + "-" + 
			UtilAll.DigitsCreate(dt.Second,2);
	}

	//returns YYYY-MM-DD hh:mm:ss
	public static string GetDatetimePrint (DateTime dt)
	{
		return UtilAll.DigitsCreate(dt.Year,4) + "-" +
			UtilAll.DigitsCreate(dt.Month,2) + "-" +
			UtilAll.DigitsCreate(dt.Day,2) + " " +
			UtilAll.DigitsCreate(dt.Hour,2) + ":" +
			UtilAll.DigitsCreate(dt.Minute,2) + ":" +
			UtilAll.DigitsCreate(dt.Second,2);
	}

	//records date & time, useful to backup database without having strange chars on filename
	//used also on SQL when time wants to be stored also
	public static DateTime FromFile (string s)
	{
		string [] allFull = s.Split(new char[] {'_'});

		if(allFull.Length != 2)
			return DateTime.MinValue;

		string [] dateFull = allFull[0].Split(new char[] {'-'});
		string [] timeFull = allFull[1].Split(new char[] {'-'});

		if(dateFull.Length != 3 || timeFull.Length != 3)
			return DateTime.MinValue;

		return new DateTime(
				Convert.ToInt32(dateFull[0]),
				Convert.ToInt32(dateFull[1]),
				Convert.ToInt32(dateFull[2]),
				Convert.ToInt32(timeFull[0]),
				Convert.ToInt32(timeFull[1]),
				Convert.ToInt32(timeFull[2])
				);
	}

	//comes from sql like YYYY-MM-DD (with always all digits)
	//return datetime
	public static DateTime FromSql (string date)
	{
		//LogB.Information("UtilDate.FromSql date: " + date);
		/*
		   on report we do a session select with uniqueID = -1
		   it returns nothing, date has nothing
		   */
		if(date == null || date == "")
			return DateTime.Now; //TODO: ensure this now is year-month-day

		/*
		   maybe date format is before 0.72 (d/m/Y)
		   this is still here and not in a standalone conversion
		   because if someone converts from older database
		   can have problems wih constructors with different date formats
		   */

		DateTime dt; //Datetime (year, month, day) constructor
		if(date.IndexOf('/') == -1) { 
			//not contains '/'
			//new sqlite3 compatible date format sice db 0.72 YYYY-MM-DD
			string [] dateFull = date.Split(new char[] {'-'});
			dt = new DateTime(
					Convert.ToInt32(dateFull[0]), 
					Convert.ToInt32(dateFull[1]), 
					Convert.ToInt32(dateFull[2]));
		} else {
			//contains '/'
			//old D/M/Y format 
			string [] dateFull = date.Split(new char[] {'/'});
			dt = new DateTime(
					Convert.ToInt32(dateFull[2]), 
					Convert.ToInt32(dateFull[1]), 
					Convert.ToInt32(dateFull[0]));
		}
		return dt;
	}

	//lowercase
	public static string GetCurrentYearMonthStr()
	{
		DateTime dt = DateTime.Now;

		return UtilAll.DigitsCreate(dt.Year,4) + "-" + Catalog.GetString(dt.ToString("MMMM").ToLower());
	}

	//get a string of last month
	//lowercase
	public static string GetCurrentYearLastMonthStr()
	{
		DateTime dt = DateTime.Now;
		dt = dt.AddMonths(-1);

		return UtilAll.DigitsCreate(dt.Year,4) + "-" + Catalog.GetString(dt.ToString("MMMM").ToLower());
	}

	public static double DateTimeYearDayAsDouble(DateTime dt)
	{
		return dt.Year + UtilAll.DivideSafe(dt.DayOfYear, 360);
	}

	//month from 0 to 11
	public static string GetMonthName(int month, bool abbreviate)
	{
	        string [] monthNames = { "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December" };
	        string [] monthNamesAbbr = { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sept", "Oct", "Nov", "Dec" };

		if(abbreviate)
			return monthNamesAbbr[month];
		else
			return monthNames[month];
	}
}
