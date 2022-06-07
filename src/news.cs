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

//software will not be managed here! only products
public class News
{
	private int code; //regular integer
	private int category; //unused at the moment
	private int version; //is NOT version of the software (software is not managed here), is version of each news, because can be updated with new text or image. New version of the sofware will be simply a new code
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

	public void UpdateSQL(bool dbconOpened)
	{
		SqliteNews.Update(dbconOpened, toSQLUpdateString());
	}

	public string GetTitle(bool es)
	{
		if(es)
			return titleEs;
		else
			return titleEn;
	}

	public string GetLink(bool es)
	{
		if(es)
			return linkEs;
		else
			return linkEn;
	}

	public string GetDescription(bool es)
	{
		if(es)
			return descriptionEs;
		else
			return descriptionEn;
	}


	/* public static methods */

	//this method uses SQL, called by main thread
	public static bool InsertOrUpdateIfNeeded(List<News> newsAtDB_l, List<News> newsAtServer_l)
	{
		bool newStuff = false;
		foreach(News nAtServer in newsAtServer_l)
		{
			bool existsLocally = false;
			foreach(News nAtDB in newsAtDB_l)
				if(nAtServer.Code == nAtDB.Code)
				{
					existsLocally = true;
					if(nAtServer.Version > nAtDB.Version)
					{
						nAtServer.UpdateSQL(false);
						newStuff = true;
					}

					break;
				}
			if(! existsLocally)
			{
				nAtServer.InsertSQL(false);
				newStuff = true;
			}
		}
		return newStuff;
	}

	public static string GetNewsDir()
	{
		return Path.Combine(
				Util.GetLocalDataDir (false) + Path.DirectorySeparatorChar + "news");
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

	private string toSQLUpdateString()
	{
		return
			" code = " + code +
			", category = " + category +
			", version = " + version +
			", viewed = \"False\"" +
			", titleEn = \"" + titleEn +
			"\", titleEs = \"" + titleEs +
			"\", linkEn = \"" + linkEn +
			"\", linkEs = \"" + linkEs +
			"\", descriptionEn = \"" + descriptionEn +
			"\", descriptionEs = \"" + descriptionEs +
			"\", linkServerImage = \"" + linkServerImage +
			"\" WHERE code = " + code;
	}

	//check if the image is saved or not, to download it if there where any problems on the server
	public bool ImageSavedOnDisc
	{
		get {
			if(linkServerImage == "")
				return false;

			string extension = "";
			if(Util.IsJpeg(linkServerImage))
				extension = ".jpg";
			else if (Util.IsPng(linkServerImage))
				extension = ".png";

			return (File.Exists(Path.Combine(News.GetNewsDir(), code.ToString() + extension)));
		}
	}

	public int Code
	{
		get { return code; }
	}

	public int Version
	{
		get { return version; }
	}

	public string LinkServerImage
	{
		get { return linkServerImage; }
	}
}
