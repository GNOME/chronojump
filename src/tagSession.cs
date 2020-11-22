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

public class TagSession
{
	private int uniqueID;
	private string name;
	private string color;
	private string comments;

	/* constructors */

	//have a uniqueID -1 contructor, useful when set is deleted
	public TagSession()
	{
		uniqueID = -1;
	}

	//constructor
	public TagSession(int uniqueID, string name, string color, string comments)
	{
		this.uniqueID = uniqueID;
		this.name = name;
		this.color = color;
		this.comments = comments;
	}

	/* methods */

	public int InsertSQL(bool dbconOpened)
	{
		return SqliteTagSession.Insert(dbconOpened, toSQLInsertString());
	}
	private string toSQLInsertString()
	{
		string uniqueIDStr = "NULL";
		if(uniqueID != -1)
			uniqueIDStr = uniqueID.ToString();

		return
			uniqueIDStr + ", \"" + name + "\", \"" + color + "\", \"" + comments + "\"";
	}

	public string [] ToStringArray ()
	{
		string [] str = new String [4];
		str[0] = uniqueID.ToString();
		str[1] = name;
		str[2] = color;
		str[3] = comments;

		return str;
	}
}

