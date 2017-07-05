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
 * Copyright (C) 2016-2017 Carles Pina & Xavier de Blas
 */

using System;
using System.Net;
using System.Web;
using System.IO;
using System.Json;
using System.Text;
using System.Collections;
using System.Collections.Generic; //Dictionary
using Mono.Unix;


public class Json
{
	public string ResultMessage;
	static string serverUrl = "http://api.chronojump.org:8080";
	//string serverUrl = "http://192.168.200.1:8080";

	public static void ChangeServerUrl(string url)
	{
		serverUrl = url;
	}

	public Json()
	{
		ResultMessage = "";
	}

	public bool PostCrashLog(string email, string comments) 
	{
		string filePath = UtilAll.GetLogFileOld();

		if(! File.Exists(filePath)) {
			this.ResultMessage = Catalog.GetString("Could not send file.\nIt does not exist.");
			return false;
		}

		if(comments != null && comments != "")
			Util.InsertTextBeginningOfFile(
					"----------\nUser comments:\n" + comments + "\n----------\n", filePath);

		// Create a request using a URL that can receive a post. 
		WebRequest request = WebRequest.Create (serverUrl + "/backtrace/" + UtilAll.ReadVersionFromBuildInfo() + "-" + email);

		// Set the Method property of the request to POST.
		request.Method = "POST";

		// Create POST data and convert it to a byte array.
		byte[] byteArray = readFile(filePath);

		// Set the ContentType property of the WebRequest.
		request.ContentType = "application/x-www-form-urlencoded";

		// Set the ContentLength property of the WebRequest.
		request.ContentLength = byteArray.Length;

		// Get the request stream.
		Stream dataStream;
		try {
			dataStream = request.GetRequestStream ();
		} catch {
			LogB.Warning("Error sending datastream");
			this.ResultMessage = Catalog.GetString("Could not send file.") + "\n" + 
				string.Format(Catalog.GetString("You are not connected to the Internet\nor {0} server is down."),
						serverUrl);
			return false;
		}

		// Write the data to the request stream.
		dataStream.Write (byteArray, 0, byteArray.Length);

		// Close the Stream object.
		dataStream.Close ();
        
		// Get the response.
		WebResponse response;
		try {
			response = request.GetResponse ();
		} catch {
			LogB.Warning("Error getting response");
			this.ResultMessage = Catalog.GetString("Could not send file.") + "\n" + 
				string.Format(Catalog.GetString("You are not connected to the Internet\nor {0} server is down."),
						serverUrl);
			return false;
		}

		// Display the status.
		LogB.Information(((HttpWebResponse)response).StatusDescription);

		// Get the stream containing content returned by the server.
		dataStream = response.GetResponseStream ();

		// Open the stream using a StreamReader for easy access.
		StreamReader reader = new StreamReader (dataStream);

		// Read the content.
		string responseFromServer = reader.ReadToEnd ();

		// Display the content.
		LogB.Information(responseFromServer);

		// Clean up the streams.
		reader.Close ();
		dataStream.Close ();
		response.Close ();    


		JsonValue result = JsonValue.Parse(responseFromServer);
		string crash_id = result["crash_id"];
		LogB.Information("crash_id: ", crash_id);

		this.ResultMessage = Catalog.GetString("Log sent. Thank you.");
		return true;
	}
	
	private byte[] readFile(string filePath)
	{
		return System.IO.File.ReadAllBytes(filePath); 
	}


	//public bool ChronojumpUpdated = true;
	public bool GetLastVersion(string currentVersion) 
	{
		// Create a request using a URL that can receive a post. 
		WebRequest request = WebRequest.Create (serverUrl + "/version");
		
		// Set the Method property of the request to GET.
		request.Method = "GET";
		
		// Set the ContentType property of the WebRequest.
		//request.ContentType = "application/x-www-form-urlencoded";
		
		HttpWebResponse response;
		try {
			response = (HttpWebResponse) request.GetResponse();
		} catch {
			this.ResultMessage = 
				Catalog.GetString("Could not get last version.") + "\n" +
				string.Format(Catalog.GetString("You are not connected to the Internet\nor {0} server is down."), 
				serverUrl);
			return false;
		}

		string responseFromServer;
		using (var sr = new StreamReader(response.GetResponseStream()))
		{
			responseFromServer = sr.ReadToEnd();
		}

		//this prints:
		// {"stable": "1.4.9"}
		//this.ResultMessage = "Last version published: " + responseFromServer;
		
		string [] strFull = responseFromServer.Split(new char[] {':'});
		int startPos = strFull[1].IndexOf('"') +1;
		int endPos = strFull[1].LastIndexOf('"') -2;

		string lastVersionPublished = strFull[1].Substring(startPos,endPos); //1.4.9
			
		string updateStr = "";
		if(currentVersion != lastVersionPublished)
			updateStr = "\n\n" + Catalog.GetString("Update software at ") + "www.chronojump.org";
			
		this.ResultMessage =		
			Catalog.GetString("Installed version is: ") + currentVersion + "\n" + 
			Catalog.GetString("Last version published: ") + lastVersionPublished +
			updateStr;
		
		//ChronojumpUpdated = (currentVersion == ResultMessage);

		return true;
	}

	/*
	 * if software just started, ping gets stuck by network problems, and user try to exit software,
	 * thread.Abort doesn't kill the thread properly
	 * just kill the webRequest
	 */
	WebRequest requestPing;
	bool requestPingAborting;

	public void PingAbort()
	{
		requestPingAborting = true;
		if(requestPing != null)
			requestPing.Abort(); //cancel an asynchronous request
	}
	public bool Ping(string osVersion, string cjVersion, string machineID) 
	{
		requestPingAborting = false;

		// Create a request using a URL that can receive a post. 
		requestPing = WebRequest.Create (serverUrl + "/ping");

		// Set the Method property of the request to POST.
		requestPing.Method = "POST";

		// Set the ContentType property of the WebRequest.
		requestPing.ContentType = "application/json";

		// Creates the json object
		JsonObject json = new JsonObject();
		json.Add("os_version", osVersion);
		json.Add("cj_version", cjVersion);
		json.Add("machine_id", machineID);

		// Converts it to a String
		String js = json.ToString();

		// Writes the json object into the request dataStream
		Stream dataStream;
		try {
			dataStream = requestPing.GetRequestStream ();
		} catch {
			this.ResultMessage = 
				string.Format(Catalog.GetString("You are not connected to the Internet\nor {0} server is down."), 
				serverUrl);
			return false;
		}
		if(requestPingAborting) {
			LogB.Information("Aborted from PingAbort");
			return false;
		}

		dataStream.Write (Encoding.UTF8.GetBytes(js), 0, js.Length);

		dataStream.Close ();

		// Get the response.
		WebResponse response;
		try {
			response = requestPing.GetResponse ();
		} catch {
			this.ResultMessage = 
				string.Format(Catalog.GetString("You are not connected to the Internet\nor {0} server is down."), 
				serverUrl);
			return false;
		}
		if(requestPingAborting) {
			LogB.Information("Aborted from PingAbort");
			return false;
		}

		// Display the status (will be 201, CREATED)
		Console.WriteLine (((HttpWebResponse)response).StatusDescription);

		// Clean up the streams.
		dataStream.Close ();
		response.Close ();
		
		this.ResultMessage = "Ping sent.";
		return true;
	}

	public Person GetPersonByRFID(string rfid)
	{
		Person person = new Person(-1);

		// Create a request using a URL that can receive a post.
		WebRequest request = WebRequest.Create (serverUrl + "/getPersonByRFID");

		// Set the Method property of the request to POST.
		request.Method = "POST";

		// Set the ContentType property of the WebRequest.
		request.ContentType = "application/json; Charset=UTF-8"; //but this is not enough, see this line:

		// Creates the json object
		JsonObject json = new JsonObject();
		json.Add("rfid", rfid);
		
		// Converts it to a String
		String js = json.ToString();

		// Writes the json object into the request dataStream
		Stream dataStream;
		try {
			dataStream = request.GetRequestStream ();
		} catch {
			this.ResultMessage =
				string.Format(Catalog.GetString("You are not connected to the Internet\nor {0} server is down."),
				serverUrl);
			return person;
		}

		dataStream.Write (Encoding.UTF8.GetBytes(js), 0, js.Length);

		dataStream.Close ();
		
		HttpWebResponse response;
		try {
			response = (HttpWebResponse) request.GetResponse();
		} catch {
			this.ResultMessage = 
				string.Format(Catalog.GetString("You are not connected to the Internet\nor {0} server is down."), 
				serverUrl);
			return person;
		}

		string responseFromServer;
		using (var sr = new StreamReader(response.GetResponseStream()))
		{
			responseFromServer = sr.ReadToEnd();
		}

		LogB.Information("GetPersonByRFID: " + responseFromServer);
		
		if(responseFromServer == "")
			LogB.Information(" Empty "); //never happens
		else if(responseFromServer == "[]")
			LogB.Information(" Empty2 "); //when rfid is not on server
		else {
			//patheticPersonDeserialize("[[2, \"(playername)\", 82.0, \"253,20,150,13\", \"\"]]");
			//patheticPersonDeserialize("[[2, \"(playername)\", 82.0, \"253,20,150,13\", \"jugadors/player.jpg\"]]");
			person = personDeserialize(responseFromServer);
		}

		return person;

	}
	public double LastPersonByRFIDHeight = 0;
	public double LastPersonByRFIDWeight = 0;
	public string LastPersonByRFIDImageURL = "";
	private Person personDeserialize(string strPerson)
	{
		JsonValue jsonPerson = JsonValue.Parse(strPerson);

		Int32 id = jsonPerson ["id"];
		string player = jsonPerson ["name"];
		double weight = jsonPerson ["weight"];
		double height = jsonPerson ["height"];
		string rfid = jsonPerson ["rfid"];
		string image = jsonPerson ["imageName"];

		LastPersonByRFIDHeight = height;
		LastPersonByRFIDWeight = weight;
		LastPersonByRFIDImageURL = image;

		return new Person(id, player, rfid);
	}


	//to retrieve images from flask (:5000)
	private string getImagesUrl()
	{
		int posOfLastColon = serverUrl.LastIndexOf(':');
		return serverUrl.Substring(0, posOfLastColon) + ":5000/static/images/photos/";
	}

	//imageHalfUrl is "jugadors/*.jpg"
	public bool DownloadImage(string imageHalfUrl, int personID)
	{
		try {
			using (WebClient client = new WebClient())
			{
				LogB.Information ("DownloadImage!!");
				LogB.Information (getImagesUrl() + imageHalfUrl);
				LogB.Information (Path.Combine(Path.GetTempPath(), personID.ToString()));
				client.DownloadFile(new Uri(getImagesUrl() + imageHalfUrl),
						Path.Combine(Path.GetTempPath(), personID.ToString()));
			}
		} catch {
			LogB.Warning("DownloadImage catched");
			return false;
		}

		return true;
	}

	public List<Task> GetTasks(int personID, int stationID)
	{
		// Create a request using a URL that can receive a post.
		WebRequest request = WebRequest.Create (serverUrl + "/getTasks");

		// Set the Method property of the request to POST.
		request.Method = "POST";

		// Set the ContentType property of the WebRequest.
		request.ContentType = "application/json; Charset=UTF-8"; //but this is not enough, see this line:

		// Creates the json object
		JsonObject json = new JsonObject();
		json.Add("personId", personID.ToString());
		json.Add("stationId", stationID.ToString());

		// Converts it to a String
		String js = json.ToString();

		// Writes the json object into the request dataStream
		Stream dataStream;
		try {
			dataStream = request.GetRequestStream ();
		} catch {
			this.ResultMessage =
				string.Format(Catalog.GetString("You are not connected to the Internet\nor {0} server is down."),
				serverUrl);
			return new List<Task>();
		}

		dataStream.Write (Encoding.UTF8.GetBytes(js), 0, js.Length);
		dataStream.Close ();

		HttpWebResponse response;
		try {
			response = (HttpWebResponse) request.GetResponse();
		} catch {
			this.ResultMessage =
				string.Format(Catalog.GetString("You are not connected to the Internet\nor {0} server is down."), 
				serverUrl);
			return new List<Task>();
		}

		string responseFromServer;
		using (var sr = new StreamReader(response.GetResponseStream()))
		{
			responseFromServer = sr.ReadToEnd();
		}

		LogB.Information("GetTasks: " + responseFromServer);

		if(responseFromServer == "" || responseFromServer == "[]")
		{
			LogB.Information(" Empty ");
			return new List<Task>();
		}

		return taskDeserializeFromServer (responseFromServer);
	}

	private List<Task> taskDeserializeFromServer(string responseFromServer)
	{
		List<Task> tasks = new List<Task>();

		JsonValue jsonTasks = JsonValue.Parse (responseFromServer);

		foreach (JsonValue jsonTask in jsonTasks) {
			Int32 id = jsonTask ["id"];
			char type = jsonTask ["type"];
			int exerciseId = jsonTask ["exerciseId"];
			string exerciseName = jsonTask ["exerciseName"];

			int personId = jsonTask ["personId"];
			int stationId = jsonTask ["stationId"];
			int sets = jsonTask ["sets"];
			int nreps = jsonTask ["nreps"];
			float load = jsonTask ["load"];
			float speed = jsonTask ["speed"];
			float percentMaxSpeed = jsonTask ["percentMaxSpeed"];
			string laterality = jsonTask ["laterality"];
			string comment = jsonTask ["comment"];
			tasks.Add(new Task(id, personId, stationId, exerciseId, exerciseName,
						sets, nreps, load, speed, percentMaxSpeed,
						laterality, comment));
		}
		return tasks;
	}

	public bool UpdateTask(int taskId, int done)
	{
		LogB.Information("At UpdateTask");
		// Create a request using a URL that can receive a post.
		WebRequest request = WebRequest.Create (serverUrl + "/updateTask");

		// Set the Method property of the request to POST.
		request.Method = "POST";

		// Set the ContentType property of the WebRequest.
		request.ContentType = "application/json; Charset=UTF-8"; //but this is not enough, see this line:

		// Creates the json object
		JsonObject json = new JsonObject();

		json.Add("taskId", taskId);
		json.Add("done", done);

		// Converts it to a String
		String js = json.ToString();

		// Writes the json object into the request dataStream
		Stream dataStream;
		try {
			dataStream = request.GetRequestStream ();
		} catch {
			this.ResultMessage =
				string.Format(Catalog.GetString("You are not connected to the Internet\nor {0} server is down."),
				serverUrl);
			return false;
		}

		dataStream.Write (Encoding.UTF8.GetBytes(js), 0, js.Length);

		dataStream.Close ();

		// Get the response.
		WebResponse response;
		try {
			response = request.GetResponse ();
		} catch {
			this.ResultMessage =
				string.Format(Catalog.GetString("You are not connected to the Internet\nor {0} server is down."),
				serverUrl);
			return false;
		}

		// Display the status (will be 202, CREATED)
		Console.WriteLine (((HttpWebResponse)response).StatusDescription);

		// Clean up the streams.
		dataStream.Close ();
		response.Close ();

		this.ResultMessage = "Update task sent.";
		return true;
	}

	public EncoderExercise GetEncoderExercise(int exerciseId)
	{
		EncoderExercise ex = new EncoderExercise();

		// Create a request using a URL that can receive a post.
		WebRequest request = WebRequest.Create (serverUrl + "/getEncoderExercise");

		// Set the Method property of the request to POST.
		request.Method = "POST";

		// Set the ContentType property of the WebRequest.
		request.ContentType = "application/json; Charset=UTF-8"; //but this is not enough, see this line:

		// Creates the json object
		JsonObject json = new JsonObject();
		json.Add("exerciseId", exerciseId);

		// Converts it to a String
		String js = json.ToString();

		// Writes the json object into the request dataStream
		Stream dataStream;
		try {
			dataStream = request.GetRequestStream ();
		} catch {
			this.ResultMessage =
				string.Format(Catalog.GetString("You are not connected to the Internet\nor {0} server is down."),
				serverUrl);
			return ex;
		}

		dataStream.Write (Encoding.UTF8.GetBytes(js), 0, js.Length);

		dataStream.Close ();

		HttpWebResponse response;
		try {
			response = (HttpWebResponse) request.GetResponse();
		} catch {
			this.ResultMessage =
				string.Format(Catalog.GetString("You are not connected to the Internet\nor {0} server is down."),
				serverUrl);
			return ex;
		}

		string responseFromServer;
		using (var sr = new StreamReader(response.GetResponseStream()))
		{
			responseFromServer = sr.ReadToEnd();
		}

		LogB.Information("GetEncoderExercise: " + responseFromServer);

		if(responseFromServer == "")
			LogB.Information(" Empty "); //never happens
		else if(responseFromServer == "[]")
			LogB.Information(" Empty2 "); //when rfid is not on server
		else {
			ex = encoderExerciseDeserialize(responseFromServer);
		}

		return ex;
	}
	private EncoderExercise encoderExerciseDeserialize(string str)
	{
		JsonValue jsonEx = JsonValue.Parse(str);

		Int32 id = jsonEx ["id"];
		string name = jsonEx ["name"];
		Int32 stationId = jsonEx ["stationId"];
		int percentBodyMassDisplaced = jsonEx ["percentBodyMassDisplaced"];

		return new EncoderExercise(id, name, percentBodyMassDisplaced,
				"", "", 0); //ressitance, description, speed1RM
	}

	public bool UploadSprintData(int personId, string distances, string times)
	{
		LogB.Information("calling upload sprint");
		// Create a request using a URL that can receive a post.
		WebRequest request = WebRequest.Create (serverUrl + "/uploadSprintData");

		// Set the Method property of the request to POST.
		request.Method = "POST";

		// Set the ContentType property of the WebRequest.
		request.ContentType = "application/json; Charset=UTF-8"; //but this is not enough, see this line:
		//exerciseName = Util.RemoveAccents(exerciseName);

		// Creates the json object
		JsonObject json = new JsonObject();

		json.Add("personId", personId);
		json.Add("distances", distances);
		json.Add("times", times);

		// Converts it to a String
		String js = json.ToString();

		// Writes the json object into the request dataStream
		Stream dataStream;
		try {
			dataStream = request.GetRequestStream ();
		} catch {
			this.ResultMessage =
				string.Format(Catalog.GetString("You are not connected to the Internet\nor {0} server is down."),
				serverUrl);
			return false;
		}

		dataStream.Write (Encoding.UTF8.GetBytes(js), 0, js.Length);

		dataStream.Close ();

		// Get the response.
		WebResponse response;
		try {
			response = request.GetResponse ();
		} catch {
			this.ResultMessage =
				string.Format(Catalog.GetString("You are not connected to the Internet\nor {0} server is down."),
				serverUrl);
			return false;
		}

		// Display the status (will be 202, CREATED)
		Console.WriteLine (((HttpWebResponse)response).StatusDescription);

		// Clean up the streams.
		dataStream.Close ();
		response.Close ();

		this.ResultMessage = "Encoder data sent.";
		return true;
	}


	/*
	public bool UploadEncoderData()
	{
		return UploadEncoderData(1, 1, "40.2", "lateral", "8100.5", 8);
	}
	*/
	public bool UploadEncoderData(int personId, int stationId, int exerciseId, string laterality, string resistance, UploadEncoderDataObject uo)
	{
		// Create a request using a URL that can receive a post.
		WebRequest request = WebRequest.Create (serverUrl + "/uploadEncoderData");

		// Set the Method property of the request to POST.
		request.Method = "POST";

		// Set the ContentType property of the WebRequest.
		request.ContentType = "application/json; Charset=UTF-8"; //but this is not enough, see this line:
		//exerciseName = Util.RemoveAccents(exerciseName);

		// Creates the json object
		JsonObject json = new JsonObject();

		json.Add("personId", personId);
		json.Add("stationId", stationId);
		//json.Add("exerciseName", exerciseName);
		json.Add("exerciseId", exerciseId);
		json.Add("laterality", laterality);
		json.Add("resistance", resistance);
		json.Add("repetitions", uo.repetitions);

		json.Add("numBySpeed", uo.numBySpeed);
		json.Add("lossBySpeed", uo.lossBySpeed);
		json.Add("rangeBySpeed", uo.rangeBySpeed);
		json.Add("vmeanBySpeed", uo.vmeanBySpeed);
		json.Add("vmaxBySpeed", uo.vmaxBySpeed);
		json.Add("pmeanBySpeed", uo.pmeanBySpeed);
		json.Add("pmaxBySpeed", uo.pmaxBySpeed);

		json.Add("numByPower", uo.numByPower);
		json.Add("lossByPower", uo.lossByPower);
		json.Add("rangeByPower", uo.rangeByPower);
		json.Add("vmeanByPower", uo.vmeanByPower);
		json.Add("vmaxByPower", uo.vmaxByPower);
		json.Add("pmeanByPower", uo.pmeanByPower);
		json.Add("pmaxByPower", uo.pmaxByPower);

		// Converts it to a String
		String js = json.ToString();

		// Writes the json object into the request dataStream
		Stream dataStream;
		try {
			dataStream = request.GetRequestStream ();
		} catch {
			this.ResultMessage =
				string.Format(Catalog.GetString("You are not connected to the Internet\nor {0} server is down."),
				serverUrl);
			return false;
		}

		dataStream.Write (Encoding.UTF8.GetBytes(js), 0, js.Length);

		dataStream.Close ();

		// Get the response.
		WebResponse response;
		try {
			response = request.GetResponse ();
		} catch {
			this.ResultMessage =
				string.Format(Catalog.GetString("You are not connected to the Internet\nor {0} server is down."),
				serverUrl);
			return false;
		}

		// Display the status (will be 202, CREATED)
		Console.WriteLine (((HttpWebResponse)response).StatusDescription);

		// Clean up the streams.
		dataStream.Close ();
		response.Close ();

		this.ResultMessage = "Encoder data sent.";
		return true;
	}


	~Json() {}
}

class JsonUtils
{
	public static JsonValue valueOrDefault(JsonValue jsonObject, string key, string defaultValue)
	{
		// Returns jsonObject[key] if it exists. If the key doesn't exist returns defaultValue and
		// logs the anomaly into the Chronojump log.
		if (jsonObject.ContainsKey (key)) {
			return jsonObject [key];
		} else {
			LogB.Information ("JsonUtils::valueOrDefault: returning default (" + defaultValue + ") from JSON: " + jsonObject.ToString ());
			return defaultValue;
		}
	}
}

public class UploadEncoderDataObject
{
	private enum byTypes { SPEED, POWER }

	public int repetitions;

	//variables calculated BySpeed (by best mean speed)
	public int numBySpeed;
	public int lossBySpeed;
	public string rangeBySpeed; //strings with . as decimal point
	public string vmeanBySpeed;
	public string vmaxBySpeed;
	public string pmeanBySpeed;
	public string pmaxBySpeed;

	//variables calculated ByPower (by best mean power)
	public int numByPower;
	public int lossByPower;
	public string rangeByPower; //strings with . as decimal point
	public string vmeanByPower;
	public string vmaxByPower;
	public string pmeanByPower;
	public string pmaxByPower;

	public UploadEncoderDataObject(ArrayList curves)
	{
		repetitions = curves.Count;

		int nSpeed = getBestRep(curves, byTypes.SPEED);
		int nPower = getBestRep(curves, byTypes.POWER);

		EncoderCurve curveBySpeed = (EncoderCurve) curves[nSpeed];
		EncoderCurve curveByPower = (EncoderCurve) curves[nPower];

		rangeBySpeed = Util.ConvertToPoint(curveBySpeed.Height);
		rangeByPower = Util.ConvertToPoint(curveByPower.Height);

		vmeanBySpeed = Util.ConvertToPoint(curveBySpeed.MeanSpeed);
		vmeanByPower = Util.ConvertToPoint(curveByPower.MeanSpeed);
		vmaxBySpeed = Util.ConvertToPoint(curveBySpeed.MaxSpeed);
		vmaxByPower = Util.ConvertToPoint(curveByPower.MaxSpeed);

		pmeanBySpeed = Util.ConvertToPoint(curveBySpeed.MeanPower);
		pmeanByPower = Util.ConvertToPoint(curveByPower.MeanPower);
		pmaxBySpeed = Util.ConvertToPoint(curveBySpeed.PeakPower);
		pmaxByPower = Util.ConvertToPoint(curveByPower.PeakPower);

		//add +1 to show to user
		numBySpeed = nSpeed + 1;
		numByPower = nPower + 1;

		lossBySpeed = getLoss(curves, byTypes.SPEED);
		lossByPower = getLoss(curves, byTypes.POWER);
	}

	private int getBestRep(ArrayList curves, byTypes by)
	{
		int curveNum = 0;
		int i = 0;
		double highest = 0;

		foreach (EncoderCurve curve in curves)
		{
			double compareTo = curve.MeanSpeedD;
			if(by == byTypes.POWER)
				compareTo = curve.MeanPowerD;

			if(compareTo > highest)
			{
				highest = compareTo;
				curveNum = i;
			}
			i ++;
		}
		return curveNum;
	}

	private int getLoss(ArrayList curves, byTypes by)
	{
		double lowest = 100000;
		double highest = 0;

		foreach (EncoderCurve curve in curves)
		{
			double compareTo = curve.MeanSpeedD;
			if(by == byTypes.POWER)
				compareTo = curve.MeanPowerD;

			if(compareTo < lowest)
				lowest = compareTo;
			if(compareTo > highest)
				highest = compareTo;
		}
		return Convert.ToInt32(Util.DivideSafeFraction(100.0 * (highest - lowest), highest));
	}
}

public class Task
{
	public int Id;
	public char Type; //initially 'P'arametrized or 'F'ree. Now all are 'P'
	public int PersonId;
	public int StationId;
	public int ExerciseId;
	public string ExerciseName;
	public int Sets;
	public int Nreps;
	public float Load;
	public float Speed;
	public float PercentMaxSpeed;
	public string Laterality;
	public string Comment;

	public Task()
	{
		Id = -1;
		Comment = "";
	}

	public Task(int id, int personId, int stationId, int exerciseId, string exerciseName,
			int sets, int nreps, float load, float speed, float percentMaxSpeed,
			string laterality, string comment)
	{
		Type = 'P'; //parametrized

		Id = id;
		PersonId = personId;
		StationId = stationId;
		ExerciseId = exerciseId;
		ExerciseName = exerciseName;
		Sets = sets;
		Nreps = nreps;
		Load = load;
		Speed = speed;
		PercentMaxSpeed = percentMaxSpeed;
		Laterality = laterality;
		Comment = comment;
	}

	public override string ToString()
	{
		string sep = "";
		string str = "";
		if (Laterality == "R" || Laterality == "L")
		{
			string lateralityStr = Catalog.GetString("Right");
			if (Laterality == "L")
				lateralityStr = Catalog.GetString("Left");

			str += sep + lateralityStr;
			sep = "; ";
		}
		if (Load != -1)
		{
			str += sep + "Càrrega = " + Load.ToString() + " Kg";
			sep = "; ";
		}
		if (Sets != -1)
		{
			str += sep + "Series = " + Sets.ToString();
			sep = "; ";
		}
		if (Nreps != -1)
		{
			str += sep + "Repeticions = " + Nreps.ToString();
			sep = "; ";
		}
		if (Speed != -1)
		{
			str += sep + "Velocitat = " + Speed.ToString() + " m/s";
			sep = "; ";
		}
		if (PercentMaxSpeed != -1)
		{
			str += sep + "Velocitat = " + PercentMaxSpeed.ToString() + " %";
			sep = "; ";
		}
		if (Comment != "")
		{
			str += "\n" + Comment;
		}
		return ExerciseName + ": " + str;
	}
}
