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
	private bool connected; //know if server is connected. Do it when there's a change on RFID (pulse)
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

		/*
		string updateStr = "";
		if(currentVersion != lastVersionPublished)
			updateStr = "\n\n" + Catalog.GetString("Update software at ") + "www.chronojump.org";
			*/

		string str =
			Catalog.GetString("Installed version is: ") + currentVersion + "\n" + 
			Catalog.GetString("Last version published: ") + lastVersionPublished;
			// + updateStr;
			//TODO: add updateStr again when resolved that a experimental 1.7.0-xxx is more advanced than a stable 1.7.0

		if(currentVersion == lastVersionPublished)
			str += "\n\n" + Catalog.GetString("Your software is updated!");
		else
			str += "\n\n" + Catalog.GetString("Update software at ") + "www.chronojump.org";

		this.ResultMessage = str;
		
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
		connected = false;
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

		connected = true;
		return person;

	}

	public double LastPersonByRFIDHeight = 0;
	public double LastPersonByRFIDWeight = 0;
	public string LastPersonByRFIDImageURL = "";
	public bool LastPersonWasInserted = false;
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

		Person personTemp = SqlitePerson.Select(false, id);
		/*
		 * if personTemp == -1, need to insert this person
		 * LastPersonWasInserted will be used:
		 * 	to insert person at person.cs
		 * 	to know if (it's new person or RFID changed) at gui/networks.cs
		 */
		LastPersonWasInserted = (personTemp.UniqueID == -1);

		return new Person(LastPersonWasInserted, id, player, rfid);
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
		connected = false;

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

		connected = true;

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

	//get pending tasks on other stations
	public List<StationCount> GetOtherStationsWithPendingTasks(int personID, int stationID)
	{
		// Create a request using a URL that can receive a post.
		WebRequest request = WebRequest.Create (serverUrl + "/getOtherStationsWithPendingTasks");

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
			return new List<StationCount>();
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
			return new List<StationCount>();
		}

		string responseFromServer;
		using (var sr = new StreamReader(response.GetResponseStream()))
		{
			responseFromServer = sr.ReadToEnd();
		}

		LogB.Information("GetOtherStationsWithPendingTasks: " + responseFromServer);

		if(responseFromServer == "" || responseFromServer == "[]")
		{
			LogB.Information(" Empty ");
			return new List<StationCount>();
		}

		//return taskDeserializeFromServer (responseFromServer);
		List<StationCount> stations = new List<StationCount>();
		JsonValue jsonStations = JsonValue.Parse (responseFromServer);

		foreach (JsonValue jsonStation in jsonStations)
		{
			string stationName = jsonStation ["stationName"];
			int tasksCount = jsonStation ["tasksCount"];
			stations.Add(new StationCount(stationName, tasksCount));
		}
		return stations;
	}

	public List<EncoderExercise> GetStationExercises(int stationId)
	{
		List<EncoderExercise> ex_list = new List<EncoderExercise>();

		// Create a request using a URL that can receive a post.
		WebRequest request = WebRequest.Create (serverUrl + "/getStationExercises");

		// Set the Method property of the request to POST.
		request.Method = "POST";

		// Set the ContentType property of the WebRequest.
		request.ContentType = "application/json; Charset=UTF-8"; //but this is not enough, see this line:

		// Creates the json object
		JsonObject json = new JsonObject();
		json.Add("stationId", stationId);

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
			return ex_list;
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
			return ex_list;
		}

		string responseFromServer;
		using (var sr = new StreamReader(response.GetResponseStream()))
		{
			responseFromServer = sr.ReadToEnd();
		}

		LogB.Information("GetStationExercises: " + responseFromServer);

		if(responseFromServer == "")
			LogB.Information(" Empty "); //never happens
		else if(responseFromServer == "[]")
			LogB.Information(" Empty2 "); //when rfid is not on server
		else {
			ex_list = stationExercisesDeserialize(responseFromServer);
		}

		return ex_list;
	}
	private List<EncoderExercise> stationExercisesDeserialize(string str)
	{
		List<EncoderExercise> ex_list = new List<EncoderExercise>();

		JsonValue jsonStationExercises = JsonValue.Parse (str);

		foreach (JsonValue jsonSE in jsonStationExercises)
		{
			Int32 id = jsonSE ["id"];
			string name = jsonSE ["name"];
			Int32 stationId = jsonSE ["stationId"];
			int percentBodyMassDisplaced = jsonSE ["percentBodyMassDisplaced"];

			ex_list.Add(new EncoderExercise(id, name, percentBodyMassDisplaced,
					"", "", 0)); //ressitance, description, speed1RM
		}
		return ex_list;
	}


	/*
	 * Unused, now using the above methods
	 *
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
	*/

	public bool UploadSprintData (UploadSprintDataObject o)
	{
		LogB.Information("calling upload sprint");
		// Create a request using a URL that can receive a post.
		WebRequest request = WebRequest.Create (serverUrl + "/uploadSprintData");


		/*
		LogB.Information("UploadSprintData doubles:");
		foreach(double d in splitTimesL)
			LogB.Information(d.ToString());
			*/

		// Set the Method property of the request to POST.
		request.Method = "POST";

		// Set the ContentType property of the WebRequest.
		request.ContentType = "application/json; Charset=UTF-8"; //but this is not enough, see this line:
		//exerciseName = Util.RemoveAccents(exerciseName);

		// Creates the json object
		JsonObject json = new JsonObject();

		json.Add("personId", o.personId);
		json.Add("distances", o.sprintPositions);
		json.Add("t1", o.splitTimesL[1]);

		//splitTimesL starts with a 0 that is not passed
		if(o.splitTimesL.Count >= 3)
			json.Add("t2", o.splitTimesL[2] - o.splitTimesL[1]); //return lap (partial time) and not split (accumulated time)
		else
			json.Add("t2", "");

		if(o.splitTimesL.Count >= 4)
			json.Add("t3", o.splitTimesL[3] - o.splitTimesL[2]);
		else
			json.Add("t3", "");

		if(o.splitTimesL.Count >= 5)
			json.Add("t4", o.splitTimesL[4] - o.splitTimesL[3]);
		else
			json.Add("t4", "");

		json.Add("k", o.k);
		json.Add("vmax", o.vmax);
		json.Add("amax", o.amax);
		json.Add("fmax", o.fmax);
		json.Add("pmax", o.pmax);

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

		this.ResultMessage = "Sprint data sent.";
		return true;
	}


	/*
	public bool UploadEncoderData()
	{
		return UploadEncoderData(1, 1, "40.2", "lateral", "8100.5", 8);
	}
	*/
	public bool UploadEncoderData(UploadEncoderDataFullObject o)
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

		json.Add("personId", o.personId);
		json.Add("stationId", o.stationId);
		//json.Add("exerciseName", exerciseName);
		json.Add("exerciseId", o.exerciseId);
		json.Add("laterality", o.laterality);
		json.Add("resistance", o.resistance);
		json.Add("repetitions", o.uo.repetitions);

		json.Add("numBySpeed", o.uo.numBySpeed);
		json.Add("lossBySpeed", o.uo.lossBySpeed);
		json.Add("rangeBySpeed", o.uo.rangeBySpeed);
		json.Add("vmeanBySpeed", o.uo.vmeanBySpeed);
		json.Add("vmaxBySpeed", o.uo.vmaxBySpeed);
		json.Add("pmeanBySpeed", o.uo.pmeanBySpeed);
		json.Add("pmaxBySpeed", o.uo.pmaxBySpeed);

		json.Add("numByPower", o.uo.numByPower);
		json.Add("lossByPower", o.uo.lossByPower);
		json.Add("rangeByPower", o.uo.rangeByPower);
		json.Add("vmeanByPower", o.uo.vmeanByPower);
		json.Add("vmaxByPower", o.uo.vmaxByPower);
		json.Add("pmeanByPower", o.uo.pmeanByPower);
		json.Add("pmaxByPower", o.uo.pmaxByPower);

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

	public bool Connected {
		get { return connected; }
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

public class UploadSprintDataObject
{
	public int uniqueId; //used for SQL load and delete
	public int personId;
	public string sprintPositions;
	public List<double> splitTimesL;
	public double k;
	public double vmax;
	public double amax;
	public double fmax;
	public double pmax;

	public UploadSprintDataObject (int uniqueId, int personId, string sprintPositions, List<double> splitTimesL,
			double k, double vmax, double amax, double fmax, double pmax)
	{
		this.uniqueId = uniqueId;
		this.personId = personId;
		this.sprintPositions = sprintPositions;
		this.splitTimesL = splitTimesL;
		this.k = k;
		this.vmax = vmax;
		this.amax = amax;
		this.fmax = fmax;
		this.pmax = pmax;
	}

	public string ToSQLInsertString ()
	{
		return
			"NULL, " +
			personId.ToString() + ", " +
			"\"" + sprintPositions + "\", " +
			"\"" + splitTimesLToString() + "\", " +
			Util.ConvertToPoint(k) + ", " +
			Util.ConvertToPoint(vmax) + ", " +
			Util.ConvertToPoint(amax) + ", " +
			Util.ConvertToPoint(fmax) + ", " +
			Util.ConvertToPoint(pmax) + ")";
	}

	public static List<double> SplitTimesStringToList(string sqlSelectSplitTimes)
	{
		List<double> l = new List<double>();
		if(sqlSelectSplitTimes == null || sqlSelectSplitTimes == "")
			return l;

		string [] myStringFull = sqlSelectSplitTimes.Split(new char[] {';'});
		foreach (string time in myStringFull)
			l.Add(Convert.ToDouble(Util.ChangeDecimalSeparator(time)));

		return l;
	}

	private string splitTimesLToString()
	{
		string str = "";
		string sep = "";
		foreach(double d in splitTimesL)
		{
			str += sep + Util.ConvertToPoint(d);
			sep = ";";
		}

		return str;
	}
}

public class UploadEncoderDataFullObject
{
	public int uniqueId; //used for SQL load and delete
	public int personId;
	public int stationId;
	public int exerciseId;
	public string laterality;
	public string resistance;
	public UploadEncoderDataObject uo;

	public UploadEncoderDataFullObject(int uniqueId, int personId, int stationId, int exerciseId,
			string laterality, string resistance, UploadEncoderDataObject uo)
	{
		this.uniqueId = uniqueId;
		this.personId = personId;
		this.stationId = stationId;
		this.exerciseId = exerciseId;
		this.laterality = laterality;
		this.resistance = resistance;
		this.uo = uo;
	}

	public string ToSQLInsertString ()
	{
		return
			"NULL, " +
			personId.ToString() + ", " +
			stationId.ToString() + ", " +
			exerciseId.ToString() + ", " +
			"\"" + laterality + "\", " +
			"\"" + resistance + "\", " +
			uo.repetitions.ToString() + ", " +
			uo.numBySpeed.ToString() + ", " +
			uo.lossBySpeed.ToString() + ", " +
			"\"" + uo.rangeBySpeed.ToString() + "\", " +
			"\"" + uo.vmeanBySpeed.ToString() + "\"," +
			"\"" + uo.vmaxBySpeed.ToString() + "\"," +
			"\"" + uo.pmeanBySpeed.ToString() + "\"," +
			"\"" + uo.pmaxBySpeed.ToString() + "\"," +
			uo.numByPower.ToString() + ", " +
			uo.lossByPower.ToString() + ", " +
			"\"" + uo.rangeByPower.ToString() + "\", " +
			"\"" + uo.vmeanByPower.ToString() + "\"," +
			"\"" + uo.vmaxByPower.ToString() + "\"," +
			"\"" + uo.pmeanByPower.ToString() + "\"," +
			"\"" + uo.pmaxByPower.ToString() + "\")";
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

	//constructor called after capture
	public UploadEncoderDataObject(ArrayList curves)
	{
		repetitions = curves.Count; //TODO: on ecc-con divide by 2

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

	//constructor called on SQL load
	public UploadEncoderDataObject(int repetitions,
			int numBySpeed, int lossBySpeed, string rangeBySpeed,
			string vmeanBySpeed, string vmaxBySpeed, string pmeanBySpeed, string pmaxBySpeed,
			int numByPower, int lossByPower, string rangeByPower,
			string vmeanByPower, string vmaxByPower, string pmeanByPower, string pmaxByPower)
	{
		this.repetitions = repetitions;
		this.numBySpeed = numBySpeed;
		this.lossBySpeed = lossBySpeed;
		this.rangeBySpeed = rangeBySpeed;
		this.vmeanBySpeed = vmeanBySpeed;
		this.vmaxBySpeed = vmaxBySpeed;
		this.pmeanBySpeed = pmeanBySpeed;
		this.pmaxBySpeed = pmaxBySpeed;
		this.numByPower = numByPower;
		this.lossByPower = lossByPower;
		this.rangeByPower = rangeByPower;
		this.vmeanByPower = vmeanByPower;
		this.vmaxByPower = vmaxByPower;
		this.pmeanByPower = pmeanByPower;
		this.pmaxByPower = pmaxByPower;
	}

	//TODO: on ecc-con should count [ecc-count] reps
	//this calculation should be the same than the client gui
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
	//TODO: on ecc-con should count [ecc-count] reps
	//this calculation should be the same than the client gui
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
		return Convert.ToInt32(Util.DivideSafe(100.0 * (highest - lowest), highest));
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

public class StationCount
{
	private string stationName;
	private int tasksCount;

	public StationCount()
	{
	}

	public StationCount(string name, int count)
	{
		stationName = name;
		tasksCount = count;
	}

	public override string ToString()
	{
		return stationName + " (" + tasksCount.ToString() + ")";
	}
}
