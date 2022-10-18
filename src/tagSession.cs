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
		string [] str = new String [5];
		str[0] = uniqueID.ToString();
		str[1] = ""; 	//checkbox
		str[2] = name;
		//str[3] = color;
		//str[4] = comments;

		return str;
	}

	public static List<object> ListSelectTypesOnSQL ()
	{
		List<object> list = new List<object>();

		foreach(TagSession ts in SqliteTagSession.Select(false, -1))
		{
			if(list.Count == 0)
				list.Add(new SelectTypes(0, "------", "------"));

			list.Add(new SelectTypes(ts.UniqueID, ts.Name, ts.Name)); //no translation on tags
		}

		return list;
	}

	public static bool CheckIfTagNameExists(bool dbconOpened, string name)
	{
		foreach(TagSession ts in SqliteTagSession.Select(dbconOpened, -1))
			if(ts.Name == name)
				return true;

		return false;
	}

	public static string GetActiveTagNamesOfThisSession(int sessionID)
	{
		string str = "";
		List<TagSession> tagSession_l = SqliteSessionTagSession.SelectTagsOfASession(false, sessionID);
		string sep = "";

		foreach(TagSession tagSession in tagSession_l)
		{
			str += sep + tagSession.Name;
			sep = ", ";
		}
		return str;
	}

	public int UniqueID
	{
		get { return uniqueID; }
	}
	public string Name
	{
		get { return name; }
		set { name = value; }
	}
}

//manages the tagSession and its session
public class SessionTagSession
{
	private int sessionID;
	private TagSession ts;

	public SessionTagSession (int sessionID, TagSession ts)
	{
		this.sessionID = sessionID;
		this.ts = ts;
	}

	//methods for managing list of TagSesssion or SessionTagSession

	public static List<TagSession> FindTagSessionsOfSession (int sessionID, List<SessionTagSession> tagsAndSessions_list)
	{
		List<TagSession> tags_list = new List<TagSession>();
		foreach(SessionTagSession sts in tagsAndSessions_list)
			if(sts.SessionID == sessionID)
				tags_list.Add(sts.Ts);

		return tags_list;
	}

	public static string PrintTagNamesOfSession (List<TagSession> tags_list)
	{
		string str = "";
		string sep = "";
		foreach(TagSession ts in tags_list)
		{
			str += sep + ts.Name;
			sep = ", ";
		}

		return str;
	}

	public int SessionID
	{
		get { return sessionID; }
	}
	public TagSession Ts
	{
		get { return ts; }
	}
}
