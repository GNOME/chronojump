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
 *  Copyright (C) 2004-2023   Xavier de Blas <xaviblas@gmail.com>
 */

using System;
using System.IO; //"File" things

public class EventType 
{
	public enum Types {
		JUMP, RUN, PULSE, REACTIONTIME, MULTICHRONOPIC, FORCESENSOR
	}

	protected Types type; //jump, run, reactionTime, pulse
	
	protected int uniqueID;
	protected string name;
	protected bool isPredefined;
	protected string imageFileName;
	protected string description;
	protected string longDescription; //info for "test image and description" window

	public EventType() {
		longDescription = ""; //needed initalization because is not defined in lots of events
	}

	public EventType(string name) {
		longDescription = ""; //needed initalization because is not defined in lots of events
	}

	public override string ToString() {
		return type + ", " + name + ", " + isPredefined + ", " + description;
	}

	public Types Type
	{
		get { return type; }
	}
	
	public virtual bool FindIfIsPredefined() {
		string [] predefinedTests = {
		};

		foreach(string search in predefinedTests)
			if(this.name == search)
				return true;

		return false;
	}
	
	public int UniqueID
	{
		get { return uniqueID; }
		set { uniqueID = value; }
	}

	/*
	 * defined on webservice
	 */
	public string Name
	{
		get { return name; }
		set { name = value; }
	}
	
	public string Description
	{
		get { return description; }
		set { description = value; }
	}

	public bool IsPredefined
	{
		get { return isPredefined; }
		set { isPredefined = value; }
	}
	
	public string ImageFileName
	{
		get { return imageFileName; }
		set { imageFileName = value; }
	}
	
	public string LongDescription
	{
		get { return longDescription; }
	}
	
	public bool HasLongDescription
	{
		get {
			if(longDescription != "")
				return true;
			else
				return false;
		}
	}

}

public class ExerciseImage
{
	//private Constants.Modes mode;
	private int uniqueID;
	private types type;

	private enum types {
		all, jump, jumpMultiple, run, runInterval, raceAnalyzer, forceSensor, encoder
	}


	// constructor
	public ExerciseImage (Constants.Modes mode, int uniqueID)
	{
		//this.mode = mode;
		this.uniqueID = uniqueID;

		if (mode == Constants.Modes.JUMPSSIMPLE)
			this.type = types.jump;
		else if (mode == Constants.Modes.JUMPSREACTIVE)
			this.type = types.jumpMultiple;
		else if (mode == Constants.Modes.RUNSSIMPLE)
			this.type = types.run;
		else if (mode == Constants.Modes.RUNSINTERVALLIC)
			this.type = types.runInterval;
		else if (mode == Constants.Modes.RUNSENCODER)
			this.type = types.raceAnalyzer;
		else if (Constants.ModeIsFORCESENSOR (mode))
			this.type = types.forceSensor;
		else if (Constants.ModeIsENCODER (mode))
			this.type = types.encoder;
	}

	// public methods

	//images enter as ".jpg" or ".png"
	public string GetUrlIfExists (bool small)
	{
		string url = getUrlJpg (small);
		if (File.Exists (url))
			return url;

		url = getUrlPng (small);
		if (File.Exists (url))
			return url;

		return "";
	}

	public void CopyImageToLocal (string urlOrigin)
	{
		deleteFileIfNeeded ();

		if (UtilMultimedia.GetImageType (urlOrigin) == UtilMultimedia.ImageTypes.JPEG)
		{
			File.Copy (urlOrigin, getUrlJpg (false));
			System.Threading.Thread.Sleep (250);
			UtilMultimedia.LoadAndResizeImage (
					getUrlJpg (false),
					getUrlJpg (true), 150, -1); //-1: maintain aspect ratio
		}
		else if (UtilMultimedia.GetImageType (urlOrigin) == UtilMultimedia.ImageTypes.PNG)
		{
			File.Copy (urlOrigin, getUrlPng (false));
			System.Threading.Thread.Sleep (250);
			UtilMultimedia.LoadAndResizeImage (
					getUrlPng (false),
					getUrlPng (true), 150, -1); //-1: maintain aspect ratio
		}
	}

	//used when delete local exercise
	public void DeleteImage ()
	{
		deleteFileIfNeeded ();
	}

	//TODO: remember to do this on import
	public static void CreateDirsIfNeeded ()
	{
		foreach (types type in Enum.GetValues (typeof (types)))
			foreach (bool small in new [] { false, true })
			{
				string dir = getDir (type, small);
				if( ! Directory.Exists (dir)) {
					Directory.CreateDirectory (dir);
					LogB.Information ("created dir:", dir);
				}
			}
	}

	// private methods

	private static string getDir (types type, bool small)
	{
		string url = Path.Combine(
				Util.GetLocalDataDir (false), "multimedia", "exercises");

		if (type == types.all)
			return url;
		else {
			if (small)
				return Path.Combine (url, type.ToString (), "small");
			else
				return Path.Combine (url, type.ToString ());
		}
	}

	private string getUrlJpg (bool small)
	{
		return Path.Combine (getDir (type, small), uniqueID.ToString () + ".jpg");
	}
	private string getUrlPng (bool small)
	{
		return Path.Combine (getDir (type, small), uniqueID.ToString () + ".png");
	}

	private void deleteFileIfNeeded ()
	{
		if (File.Exists (getUrlJpg (false)))
			File.Delete (getUrlJpg (false));

		if (File.Exists (getUrlJpg (true)))
			File.Delete (getUrlJpg (true));

		if (File.Exists (getUrlPng (false)))
			File.Delete (getUrlPng (false));

		if (File.Exists (getUrlPng (true)))
			File.Delete (getUrlPng (true));
	}

}
