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

public class News
{
	private int code; //regular integer
	private int category; //at the moment: 0 software, 1 products
	private int version; //is NOT version of the software, is version of the news, because can be updated with new text or image. New version of the sofware will be simply a new code
	private bool viewed;
	//En and Es because there are two versions of the web site
	private string titleEn;
	private string titleEs;
	private string linkEn;
	private string linkEs;
	private string descriptionEn;
	private string descriptionEs;
	private string linkServerImage; //stored to download again if changed (future)

	/* constructors */

	//have a code -1 contructor
	public News()
	{
		code = -1;
	}

	//constructor
	public News(int code, int category, int version, bool viewed,
			string titleEn, string titleEs, string linkEn, string linkEs,
			string descriptionEn, string descriptionEs, string linkServerImage)
	{
		this.code = code;
		this.category = category;
		this.version = version;
		this.viewed = viewed;
		this.titleEn = titleEn;
		this.titleEs = titleEs;
		this.linkEn = linkEn;
		this.linkEs = linkEs;
		this.descriptionEn = descriptionEn;
		this.descriptionEs = descriptionEs;
		this.linkServerImage = linkServerImage;
	}

	/* public methods */

	public override string ToString()
	{
		return string.Format("{0}:{1}:{2}:{3}:{4}:{5}:{6}:{7}:{8}:{9}:{10}",
				code, category, version, viewed, titleEn, titleEs, linkEn, linkEs, descriptionEn, descriptionEs, linkServerImage);
	}

	public int InsertSQL(bool dbconOpened)
	{
		return SqliteNews.Insert(dbconOpened, toSQLInsertString());
	}

	/* public static methods */

	public static void InsertAndDownloadImageIfNeeded(Json js, List<News> newsAtServer_l)
	{
		List<News> newsAtDB_l = SqliteNews.Select(false, -1);
		foreach(News nAtServer in newsAtServer_l)
		{
			bool found = false;
			foreach(News nAtDB in newsAtDB_l)
				if(nAtServer.Code == nAtDB.Code)
				{
					found = true;
					break;
				}
			if(! found)
			{
				nAtServer.InsertSQL(false);
				js.DownloadNewsImage(nAtServer.LinkServerImage, nAtServer.Code);
			}
		}
	}

	public static string GetNewsDir()
	{
		return Path.Combine(
				Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
				"Chronojump" + Path.DirectorySeparatorChar + "news");
	}
	public static void CreateNewsDirIfNeeded ()
	{
		string dir = GetNewsDir();
		if( ! Directory.Exists(dir)) {
			Directory.CreateDirectory (dir);
			LogB.Information ("created dir:", dir);
		}
	}

	/* private methods */

	private string toSQLInsertString()
	{
		string codeStr = "NULL";
		if(code != -1)
			codeStr = code.ToString();

		return codeStr + ", " + category + ", " + version + ", " +
			Util.BoolToInt(viewed).ToString() + ", \"" +
			titleEn + "\", \"" + titleEs + "\", \"" +
			linkEn + "\", \"" + linkEs + "\", \"" +
			descriptionEn + "\", \"" + descriptionEs + "\", \"" +
			linkServerImage + "\"";
	}

	public int Code
	{
		get { return code; }
	}

	public string LinkServerImage
	{
		get { return linkServerImage; }
	}
}
