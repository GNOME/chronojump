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
 *  Copyright (C) 2004-2025   Xavier de Blas <xaviblas@gmail.com>
 */

using System;
using Mono.Unix;

public class EncoderSQL : Event
{
	public int exerciseID;
	public string eccon;
	public string laterality;
	public string extraWeight;
	public string signalOrCurve;
	public string filename;
	public string url;	//URL of data of signals and curves. Stored in DB as relative. Used in software as absolute. See SqliteEncoder
	public int time;
	public int minHeight;
	public string status;	//active or inactive curves
	public string videoURL;	//URL of video of signals. Stored in DB as relative. Used in software as absolute. See SqliteEncoder
	
	//encoderConfiguration conversions
	//in signals and curves, need to do conversions (invert, inertiaMomentum, diameter)
	public EncoderConfiguration encoderConfiguration;
//	public int inertiaMomentum; //kg*cm^2
//	public double diameter;
	
	public string meanPower; // future1
	public string meanSpeed; // future2
	public string meanForce; // future3
	public Preferences.EncoderRepetitionCriteria repCriteria;

	public string exerciseName;
	
	public string ecconLong;
	
	public EncoderSQL ()
	{
	}

	public EncoderSQL (int uniqueID, int personID, int sessionID, int exerciseID,
			string eccon, string laterality, string extraWeight, string signalOrCurve, 
			string filename, string url, int time, int minHeight, 
			string description, string status, string videoURL, 
			EncoderConfiguration encoderConfiguration,
			string future1, string future2, string future3, 
			Preferences.EncoderRepetitionCriteria repCriteria,
			string exerciseName
			)
	{
		this.uniqueID = uniqueID;
		this.personID = personID;
		this.sessionID = sessionID;
		this.exerciseID = exerciseID;
		this.eccon = eccon;
		this.laterality = laterality;
		this.extraWeight = extraWeight;
		this.signalOrCurve = signalOrCurve;
		this.filename = filename;
		this.url = url;
		this.time = time;
		this.minHeight = minHeight;
		this.description = description;
		this.status = status;
		this.videoURL = videoURL;
		this.encoderConfiguration = encoderConfiguration;
		this.meanPower = future1;	//on curves: meanPower
		this.meanSpeed = future2; //on curves: meanSpeed
		this.meanForce = future3; //on curves: meanForce
		this.repCriteria = repCriteria;
		this.exerciseName = exerciseName;

		ecconLong = EcconLong(eccon);
	}

	// constructor for TreeViewEncoder.getObjectFromString ()
	public EncoderSQL (int uniqueID, string laterality, string extraWeight, string eccon, string description, string exerciseName)
	{
		this.uniqueID = uniqueID;
		this.laterality = laterality;
		this.extraWeight = extraWeight;
		this.eccon = eccon;
		this.description = description;
		this.exerciseName = exerciseName;
	}

	public static List<Event> EncoderSQLListToEventList (List<EncoderSQL> list)
	{
		List<Event> events = new List<Event>();
		foreach (EncoderSQL eSQL in list)
		{
			Event ev = (Event) eSQL;
			ev.Type = eSQL.exerciseName; //exerciseName name it Type to be used on findLongestWordCairo
			events.Add (ev);
		}

		return events;
	}

	public static string EcconLong (string ecconChars)
	{
		if(ecconChars == "c")
			return Catalog.GetString("Concentric");
		else if(ecconChars == "ec" || ecconChars == "ecS")
			return Catalog.GetString("Eccentric-concentric");
		else if(ecconChars == "ce" || ecconChars == "ceS")
			return Catalog.GetString("Concentric-eccentric");
		else
			return "";
	}

	//used on encoder table
	public enum Eccons { ALL, ecS, ceS, c } 

	//for new code on other parts, use static method: UtilDate.GetDatetimePrint (DateTime dt)
	public string GetDatetimeStr (bool pretty)
	{
		//LogB.Information ("GetDatetimeStr filename: " + filename);
		int pointPos = filename.LastIndexOf('.');
		int dateLength = 19; //YYYY-MM-DD_hh-mm-ss
		string date = "";

		//if file has been stored incorrectly (without datetime), just avoid crashing here
		try {
			date = filename.Substring(pointPos - dateLength, dateLength);
		} catch {
			date = UtilDate.ToFile (DateTime.Now);
		}

		if(pretty) {
			string [] dateParts = date.Split(new char[] {'_'});
			date = dateParts[0] + " " + dateParts[1].Replace('-',':');
		}
		return date;
	}

	public string GetDateStr ()
	{
		int pointPos = filename.LastIndexOf('.');
		int dateLength = 19; //YYYY-MM-DD_hh-mm-ss
		string date = filename.Substring(pointPos - dateLength, dateLength);
		string [] dateParts = date.Split(new char[] {'_'});
		return dateParts[0];
	}

	public string GetFullURL(bool convertPathToR) {
		string str = url + Path.DirectorySeparatorChar + filename;
		/*	
			in Linux is separated by '/'
			in windows is separated by '\'
			but R needs always '/', then do the conversion
		 */
		if(convertPathToR && UtilAll.IsWindows())
			str = str.Replace("\\","/");

		return str;
	}

	//showMeanPower is used in curves, but not in signal
	public string [] ToStringArray (int count, bool checkboxes, bool video, bool encoderConfigPretty, bool showMeanPSF)
	{
		int all = 9;
		if(checkboxes)
			all ++;
		if(video)
			all++;
		if(showMeanPSF)
			all += 3;


		string [] str = new String [all];
		int i=0;
		str[i++] = uniqueID.ToString ();
	
		if(checkboxes)
			str[i++] = "";	//checkboxes
	
		str[i++] = count.ToString();
		str[i++] = Catalog.GetString(exerciseName);
		str[i++] = Catalog.GetString(laterality);
		str[i++] = extraWeight;
		
		if(showMeanPSF)
		{
			str[i++] = meanPower;

			//as recording meanSpeed and meanForce is new on 2.0, show a blank cell instead of a 0
			if(meanSpeed == "0")
				str[i++] = "";
			else
				str[i++] = meanSpeed;

			if(meanForce == "0")
				str[i++] = "";
			else
				str[i++] = meanForce;
		}

		if(encoderConfigPretty)
			str[i++] = encoderConfiguration.ToStringPretty();
		else
			str[i++] = encoderConfiguration.code.ToString();
		
		str[i++] = ecconLong;
		str[i++] = GetDatetimeStr (true);
		
		if(video) {
			if(videoURL != "")
				str[i++] = Catalog.GetString("Yes");
			else
				str[i++] = Catalog.GetString("No");
		}

		str[i++] = description;
		return str;
	}

	public override string ToString () 	 //debug
	{
		return string.Format (
				"uniqueID: {0},  personID: {1},  sessionID: {2},  exerciseID: {3},  eccon: {4}, " +
				"laterality: {5},  extraWeight: {6},  signalOrCurve: {7},  filename: {8}, " +
				"url: {9},  time: {10},  minHeight: {11},  description: {12}, " +
				"status: {13},  videoURL: {14},  encoderConfiguration: {15},  future1: {16}, " +
				"future2: {17},  future3: {18},   repCriteria: {19},  exerciseName: {20}",
				uniqueID, personID, sessionID, exerciseID, eccon, laterality, extraWeight, signalOrCurve, filename,
				url, time, minHeight, description, status, videoURL, encoderConfiguration, meanPower, meanSpeed, meanForce,  repCriteria, exerciseName);
	}

	//uniqueID:name
	public EncoderSQL ChangePerson(string newIDAndName) {
		int newPersonID = Util.FetchID(newIDAndName);
		string newPersonName = Util.FetchName(newIDAndName);
		string newFilename = filename;

		personID = newPersonID;

		/*
		 * this can fail because person name can have an "-"
		string [] filenameParts = filename.Split(new char[] {'-'});
		filenameParts[0] = newPersonID.ToString();
		filenameParts[1] = newPersonName;
		//the rest will be the same: curveID, timestamp, extension 
		filename = Util.StringArrayToString(filenameParts, "-");
		*/


		/*
		 * filename curve has personID-name-uniqueID-fulldate.txt
		 * filename signal as personID-name-fulldate.txt
		 * in both cases name can have '-' (fuck)
		 * eg: filename curve:
		 * 163-personname-840-2013-04-05_14-11-11.txt
		 * filename signal
		 * 163-personname-2013-04-05_14-03-45.txt
		 *
		 * then,
		 * on curve:
		 * last 23 letters are date and ".txt",
		 * write newPersonID-newPersonName-uniqueID-last23letters
		 * 
		 * on signal:
		 * last 23 letters are date and ".txt",
		 * write newPersonID-newPersonName-last23letters
		 */

		if(signalOrCurve == "curve") 
			newFilename = newPersonID + "-" + newPersonName + "-" + uniqueID + "-" + GetDatetimeStr (false) + ".txt";
		else 
			newFilename = newPersonID + "-" + newPersonName + "-" + GetDatetimeStr (false) + ".txt";

		bool success = false;
		success = Util.FileMove(url, filename, newFilename);
		if(success)
			filename = newFilename;

		//will update SqliteEncoder
		return (this);
	}


	/* 
	 * translations stuff
	 * used to store in english and show translated in GUI
	 */
		
	private string [] lateralityOptionsEnglish = { "RL", "R", "L" }; //attention: if this changes, change it also in gui/encoder.cs createEncoderCombos()
	public string LateralityToEnglish() 
	{
		int count = 0;
		foreach(string option in lateralityOptionsEnglish) {
			if(Catalog.GetString(option) == laterality)
				return lateralityOptionsEnglish[count];
			count ++;
		}
		//default return first value
		return lateralityOptionsEnglish[0];
	}

	//used in NUnit
	public string Filename
	{
		set { filename = value; }
	}

	public double meanPowerD
	{
		get {
			if (meanPower == "")
				return 0;
			return Convert.ToDouble (meanPower);
		}
	}

	public double meanSpeedD
	{
		get {
			if (meanSpeed == "")
				return 0;
			return Convert.ToDouble (meanSpeed);
		}
	}

	public double meanForceD
	{
		get {
			if (meanForce == "")
				return 0;
			return Convert.ToDouble (meanForce);
		}
	}

}
