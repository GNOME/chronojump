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
 *  Copyright (C) 2020   Xavier de Blas <xaviblas@gmail.com>
 */

using System;
using System.IO; 		//for detect OS //TextWriter
using System.Collections.Generic; //List<T>
using Mono.Unix;

public class News
{
	private int uniqueID;
	private int code; //regular integer
	private int category; //at the moment: 0 software, 1 products
	private int version; //is NOT version of the software, is version of the news, because can be updated with new text or image. New version of the sofware will be simply a new code
	private string versionDateTime;
	private bool viewed;
	private string title;
	private string link;
	private string description;

	/* constructors */

	//have a uniqueID -1 contructor, useful when set is deleted
	public News()
	{
		uniqueID = -1;
	}

	//constructor
	public News(int uniqueID, int code, int category, int version, string versionDateTime,
			bool viewed, string title, string link, string description)
	{
		this.uniqueID = uniqueID;
		this.code = code;
		this.category = category;
		this.version = version;
		this.versionDateTime = versionDateTime;
		this.viewed = viewed;
		this.title = title;
		this.link = link;
		this.description = description;
	}

	/* methods */

	public int InsertSQL(bool dbconOpened)
	{
		return SqliteNews.Insert(dbconOpened, toSQLInsertString());
	}
	private string toSQLInsertString()
	{
		string uniqueIDStr = "NULL";
		if(uniqueID != -1)
			uniqueIDStr = uniqueID.ToString();

		return uniqueIDStr + ", " + code + ", " + category + ", " +
			version + ", \"" + versionDateTime + "\", " + Util.BoolToInt(viewed).ToString() + ", \"" +
			       title + "\", \"" + link + "\", \"" + description + "\"";
	}

}
