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
 *  Copyright (C) 2018-2019   Xavier de Blas <xaviblas@gmail.com>
 */

using System;
using System.IO; 		//for detect OS //TextWriter
using System.Collections.Generic; //List<T>
using Mono.Unix;

public class RunEncoder
{
	public enum Devices { MANUAL, RESISTED } //RESISTED will have two columns on the CSV (encoder, forecSensor)
	public static string DevicesStringMANUAL = "Manual race analyzer";
	public static string DevicesStringRESISTED = "Resisted race analyzer";

	private int uniqueID;
	private int personID;
	private int sessionID;
	private int exerciseID; //until runEncoderExercise table is not created, all will be 0
	private int angle;
	private Devices device;
	private int distance;
	private int temperature;
	private string filename;
	private string url;	//relative
	private string dateTime;
	private string comments;
	private string videoURL;

	private string exerciseName;

	/* constructors */

	//have a uniqueID -1 contructor, useful when set is deleted
	public RunEncoder()
	{
		uniqueID = -1;
	}

	//constructor
	public RunEncoder(int uniqueID, int personID, int sessionID, int exerciseID, Devices device,
			int distance, int temperature, string filename, string url,
			string dateTime, string comments, string videoURL, string exerciseName)
	{
		this.uniqueID = uniqueID;
		this.personID = personID;
		this.sessionID = sessionID;
		this.exerciseID = exerciseID;
		this.device = device;
		this.distance = distance;
		this.temperature = temperature;
		this.filename = filename;
		this.url = url;
		this.dateTime = dateTime;
		this.comments = comments;
		this.videoURL = videoURL;

		this.exerciseName = exerciseName;
	}

	/* methods */

	public int InsertSQL(bool dbconOpened)
	{
		return SqliteRunEncoder.Insert(dbconOpened, toSQLInsertString());
	}
	private string toSQLInsertString()
	{
		string uniqueIDStr = "NULL";
		if(uniqueID != -1)
			uniqueIDStr = uniqueID.ToString();

		return
			"(" + uniqueIDStr + ", " + personID + ", " + sessionID + ", " + exerciseID + ", \"" + device.ToString() + "\", " +
			distance + ", " + temperature + ", \"" + filename + "\", \"" + url + "\", \"" + dateTime + "\", \"" +
			comments + "\", \"" + videoURL + "\")";
	}

	public void UpdateSQL(bool dbconOpened)
	{
		SqliteRunEncoder.Update(dbconOpened, toSQLUpdateString());
	}
	private string toSQLUpdateString()
	{
		return
			" uniqueID = " + uniqueID +
			", personID = " + personID +
			", sessionID = " + sessionID +
			", exerciseID = " + exerciseID +
			", device = \"" + device.ToString() +
			"\", distance = " + distance +
			", temperature = " + temperature +
			", filename = \"" + filename +
			"\", url = \"" + Util.MakeURLrelative(url) +
			"\", dateTime = \"" + dateTime +
			"\", comments = \"" + comments +
			"\", videoURL = \"" + Util.MakeURLrelative(videoURL) +
			"\" WHERE uniqueID = " + uniqueID;
	}

	public void UpdateSQLJustComments()
	{
		SqliteRunEncoder.UpdateComments (false, uniqueID, comments); //SQL not opened
	}

	public string [] ToStringArray (int count)
	{
		int all = 8;
		string [] str = new String [all];
		int i=0;
		str[i++] = uniqueID.ToString();
		str[i++] = count.ToString();
		str[i++] = exerciseName;
		str[i++] = Catalog.GetString(GetDeviceString(device));
		str[i++] = distance.ToString();
		str[i++] = dateTime;

		//str[i++] = videoURL;
		if(videoURL != "")
			str[i++] = Catalog.GetString("Yes");
		else
			str[i++] = Catalog.GetString("No");

		str[i++] = comments;

		return str;
	}

	public static string GetDeviceString(Devices d)
	{
		if(d == Devices.RESISTED)
			return DevicesStringRESISTED;
		else
			return DevicesStringMANUAL;
	}

	public static string GetScript() {
		return System.IO.Path.Combine(UtilEncoder.GetSprintPath(), "sprintEncoder.R");
	}
	public static string GetCSVFileName() {
		return Path.Combine(Path.GetTempPath(), "cj_race_analyzer_data.csv");
	}
	public static string GetTempFileName() {
		return Path.Combine(Path.GetTempPath(), "cj_race_analyzer_graph.png");
	}


	public string FullURL
	{
		get { return Util.GetRunEncoderSessionDir(sessionID) + Path.DirectorySeparatorChar + filename; }
	}
	public string Filename
	{
		get { return filename; }
	}

	public int UniqueID
	{
		get { return uniqueID; }
		set { uniqueID = value; }
	}
	public int ExerciseID
	{
		get { return exerciseID; }
		set { exerciseID = value; }
	}
	public Devices Device
	{
		get { return device; }
		set { device = value; }
	}
	public int Distance
	{
		get { return distance; }
		set { distance = value; }
	}
	public int Temperature
	{
		get { return temperature; }
		set { temperature = value; }
	}
	public string Comments
	{
		get { return comments; }
		set { comments = value; }
	}
	public string ExerciseName
	{
		get { return exerciseName; }
		set { exerciseName = value; }
	}
}

public class RunEncoderExercise
{
	private int uniqueID;
	private string name;
	private string description;

	public RunEncoderExercise()
	{
	}

	public RunEncoderExercise(string name)
	{
		this.name = name;
	}

	public RunEncoderExercise(int uniqueID, string name, string description)
	{
		this.uniqueID = uniqueID;
		this.name = name;
		this.description = description;
	}

	public override string ToString()
	{
		return uniqueID.ToString() + ":" + name + ":" + description + ":";
	}

	public int UniqueID
	{
		get { return uniqueID; }
	}
	public string Name
	{
		get { return name; }
	}
	public string Description
	{
		get { return description; }
	}
}

public class RunEncoderGraph
{
	private int testLength;
	private double mass;
	private double personHeight;
	private double tempC;
	private RunEncoder.Devices device;

	public RunEncoderGraph(int testLength, double mass, double personHeight, double tempC, RunEncoder.Devices device)
	{
		this.testLength = testLength;
		this.mass = mass;
		this.personHeight = personHeight;
		this.tempC = tempC;
		this.device = device;
	}

	public bool CallR(int graphWidth, int graphHeight)
	{
		LogB.Information("\nrunEncoder CallR ----->");
		writeOptionsFile(graphWidth, graphHeight);
		return ExecuteProcess.CallR(RunEncoder.GetScript());
	}

	private void writeOptionsFile(int graphWidth, int graphHeight)
	{
		string scriptsPath = UtilEncoder.GetSprintPath();
		if(UtilAll.IsWindows())
			scriptsPath = scriptsPath.Replace("\\","/");

		System.Globalization.NumberFormatInfo localeInfo = new System.Globalization.NumberFormatInfo();
		localeInfo = System.Globalization.NumberFormatInfo.CurrentInfo;

		string scriptOptions =
			"#scriptsPath\n" + 		UtilEncoder.GetScriptsPath() + "\n" +
			"#filename\n" + 		RunEncoder.GetCSVFileName() + "\n" +
			"#mass\n" + 			Util.ConvertToPoint(mass) + "\n" +
			"#personHeight\n" + 		Util.ConvertToPoint(personHeight / 100.0) + "\n" + //send it in meters
			"#tempC\n" + 			tempC + "\n" +
			"#testLength\n" + 		testLength.ToString() + "\n" +
			"#os\n" + 			UtilEncoder.OperatingSystemForRGraphs() + "\n" +
			"#graphWidth\n" + 		graphWidth.ToString() + "\n" +
			"#graphHeight\n" + 		graphHeight.ToString() + "\n" +
			"#device\n" + 			device.ToString();


		TextWriter writer = File.CreateText(Path.GetTempPath() + "Roptions.txt");
		writer.Write(scriptOptions);
		writer.Flush();
		writer.Close();
		((IDisposable)writer).Dispose();
	}

	public static string GetDataDir(int sessionID)
	{
		System.IO.DirectoryInfo folderSession =
			new System.IO.DirectoryInfo(Util.GetRunEncoderSessionDir(sessionID));
		System.IO.DirectoryInfo folderGeneric =
			new System.IO.DirectoryInfo(Util.GetRunEncoderDir());

		if(folderSession.Exists)
			return Util.GetRunEncoderSessionDir(sessionID);
		else if(folderGeneric.Exists)
			return Util.GetRunEncoderDir();
		else
			return "";
	}
}

public class RunEncoderLoadTryToAssignPersonAndComment
{
	private bool dbconOpened;
	private string filename; //filename comes without extension
	private int currentSessionID; //we get a person if already exists on that session

	public string Comment;

	public RunEncoderLoadTryToAssignPersonAndComment(bool dbconOpened, string filename, int currentSessionID)
	{
		this.dbconOpened = dbconOpened;
		this.filename = filename;
		this.currentSessionID = currentSessionID;

		Comment = "";
	}

	public Person GetPerson()
	{
		string personName = getNameAndComment();
		if(personName == "")
			return new Person(-1);

		Person p = SqlitePerson.SelectByName(dbconOpened, personName);
		if(SqlitePersonSession.PersonSelectExistsInSession(dbconOpened, p.UniqueID, currentSessionID))
			return p;

		return new Person(-1);
	}

	private string getNameAndComment()
	{
		string name = "";

		string [] strFull = filename.Split(new char[] {'_'});

		/*
		 * first filename was: personName_date_hour
		 * but we have lots of files with comments added manually like:
		 * first filename was: personName_date_hour_comment
		 * first filename was: personName_date_hour_comment_long_with_underscores
		 */
		if(strFull.Length >= 3)
			name = strFull[0];

		if(strFull.Length == 4) //with one comment
			Comment = strFull[3];
		else if(strFull.Length > 4) //with comments separated by underscores
		{
			string myComment = "";
			string sep = "";
			for(int i = 3; i <= strFull.Length -3; i ++)
			{
				myComment += sep + strFull[i];
				sep = "_";
			}

			Comment = myComment;
		}

		return name;
	}
}
