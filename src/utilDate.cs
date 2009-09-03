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
 *  Copyright (C) 2004-2009   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
using System.Text; //StringBuilder
using System.Collections; //ArrayList

//this class tries to be a space for methods that are used in different classes
public class UtilDate
{
	//comes datetime
	//insert in sql like YYYY-MM-DD (with always all digits)
	//we use Year, Month and Day for not having any problem with locale
	public static string ToSql (DateTime dt)
	{
		return Util.DigitsCreate(dt.Year,4) + "-" + 
			Util.DigitsCreate(dt.Month,2) + "-" + 
			Util.DigitsCreate(dt.Day,2);
	}
	
	//records date & time, useful to backup database without having strange chars on filename
	public static string ToFile (DateTime dt)
	{
		return Util.DigitsCreate(dt.Year,4) + "-" + 
			Util.DigitsCreate(dt.Month,2) + "-" + 
			Util.DigitsCreate(dt.Day,2) + "_" +
			Util.DigitsCreate(dt.Hour,2) + "-" + 
			Util.DigitsCreate(dt.Minute,2) + "-" + 
			Util.DigitsCreate(dt.Second,2);
	}


	//comes from sql like YYYY-MM-DD (with always all digits)
	//return datetime
	public static DateTime FromSql (string date)
	{
		try {
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
		catch {
			/*
			   on report we do a session select with uniqueID = -1
			   it returns nothing, date has nothing
			   */
			return DateTime.Now;
		}
	}

}
