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
			//patheticPersonDeserialize("[[2, \"(playername)\", 82.0, \"253,20,150,13\"]]");
			person = patheticPersonDeserialize(responseFromServer);
		}

		return person;

	}
	public double LastPersonByRFIDWeight = 0;
	private Person patheticPersonDeserialize(string str)
	{
		LogB.Information("str:|" + str + "|");
		
		string id = str.Substring(2, str.IndexOf(',') -2);
		LogB.Information("id:|" + id + "|");

		str = str.Substring(str.IndexOf('"') +1);
		//LogB.Information("str:|" + str + "|");
		
		string player = str.Substring(0, str.IndexOf('"'));
		LogB.Information("player:|" + player + "|");
		
		str = str.Substring(str.IndexOf(',') +2);
		//LogB.Information("str:|" + str + "|");

		string weight = str.Substring(0, str.IndexOf(','));
		LogB.Information("weight:|" + weight + "|");
		
	
		LastPersonByRFIDWeight = Convert.ToDouble(Util.ChangeDecimalSeparator(weight));
		str = str.Substring(str.IndexOf('"') +1);
		//LogB.Information("str:|" + str + "|");
		
		string rfid = str.Substring(0, str.LastIndexOf('"'));
		LogB.Information("rfid:|" + rfid + "|");

		return new Person(Convert.ToInt32(id), player, rfid);
	}


	public bool UploadEncoderData()
	{
		return UploadEncoderData(1, 1, "lateral", "8100", 8);
	}
	public bool UploadEncoderData(int personId, int machineId, string exerciseName, string meanPowerBestRep, int repsAbove50pBest )
	{
		// Create a request using a URL that can receive a post.
		WebRequest request = WebRequest.Create (serverUrl + "/uploadEncoderData");

		// Set the Method property of the request to POST.
		request.Method = "POST";

		// Set the ContentType property of the WebRequest.
		request.ContentType = "application/json; Charset=UTF-8"; //but this is not enough, see this line:
		exerciseName = Util.RemoveAccents(exerciseName);

		// Creates the json object
		JsonObject json = new JsonObject();
		json.Add("personId", personId);
		json.Add("machineId", machineId);
		json.Add("exerciseName", exerciseName);
		json.Add("meanPowerBestRep", meanPowerBestRep);
		json.Add("repsAbove50pBest", repsAbove50pBest);

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
