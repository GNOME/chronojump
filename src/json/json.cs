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

	protected bool connected; //know if server is connected. Do it when there's a change on RFID (pulse)
	protected WebRequest request; //generic request (for all methods except ping)
	protected static string serverUrl = "http://api.chronojump.org:8080";
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
		if (! createWebRequest(requestType.GENERIC, "/backtrace/" + UtilAll.ReadVersionFromBuildInfo() + "-" + email))
			return false;

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
		if(! getWebRequestStream (request, out dataStream, Catalog.GetString("Could not send file.")))
			return false;

		// Write the data to the request stream.
		dataStream.Write (byteArray, 0, byteArray.Length);

		// Close the Stream object.
		dataStream.Close ();
        
		// Get the response.
		WebResponse response;
		if(! getWebResponse (request, out response, Catalog.GetString("Could not send file.")))
			return false;


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
		if (! createWebRequest(requestType.GENERIC, "/version"))
			return false;
		
		// Set the Method property of the request to GET.
		request.Method = "GET";
		
		// Set the ContentType property of the WebRequest.
		//request.ContentType = "application/x-www-form-urlencoded";
		
		HttpWebResponse response;
		if(! getHttpWebResponse (request, out response, Catalog.GetString("Could not get last version.")))
			return false;

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

		VersionCompare vCompare = new VersionCompare(
				new Version31(currentVersion),
				new Version31(lastVersionPublished));
		str += "\n" + vCompare.ResultStr;

		this.ResultMessage = str;
		
		//ChronojumpUpdated = (currentVersion == ResultMessage);

		return true;
	}

	/*
	 * ---- news stuff start ---->
	 */

	public bool GetNewsDatetime ()
	{
		// Create a request using a URL that can receive a post.
		if (! createWebRequest(requestType.GENERIC, "/getNewsDatetime"))
			return false;

		// Set the Method property of the request to GET.
		request.Method = "GET";

		// Set the ContentType property of the WebRequest.
		//request.ContentType = "application/x-www-form-urlencoded";

		HttpWebResponse response;
		if(! getHttpWebResponse (request, out response, "Could not get news datetime"))
			return false;

		string responseFromServer;
		using (var sr = new StreamReader(response.GetResponseStream()))
		{
			responseFromServer = sr.ReadToEnd();
		}

		LogB.Information("getNewsServerDatetime:" + responseFromServer);

		//responseFromServer comes as: "2020-01-18_12-13-00", need to remove double quotes because we do not want to insert them on sql

		int startPos = responseFromServer.IndexOf('"') +1;
		int endPos = responseFromServer.LastIndexOf('"') -1;
		if(endPos > 0)
			responseFromServer = responseFromServer.Substring(startPos,endPos);

		this.ResultMessage = responseFromServer;

		return true;
	}

	//get all the news, news class will decide if something have to be inserted or selected
	//called by pingThread at start
	public List<News> GetNews(List<News> newsAtDB_l)
	{
		// Create a request using a URL that can receive a post.
		if (! createWebRequest(requestType.GENERIC, "/getNews"))
			return new List<News>();

		// Set the Method property of the request to GET.
		request.Method = "GET";

		// Set the ContentType property of the WebRequest.
		request.ContentType = "application/json; Charset=UTF-8";

		HttpWebResponse response;
		if(! getHttpWebResponse (request, out response, "Could not get last news."))
			return new List<News>();

		string responseFromServer;
		using (var sr = new StreamReader(response.GetResponseStream()))
		{
			responseFromServer = sr.ReadToEnd();
		}

		this.ResultMessage = responseFromServer;

		return newsDeserialize(responseFromServer, newsAtDB_l);
	}
	private List<News> newsDeserialize(string str, List<News> newsAtDB_l)
	{
		LogB.Information("newsDeserialize:|" + str + "|");

		List<News> news_l = new List<News>();
		JsonValue jsonNewsAll = JsonValue.Parse(str);

		foreach (JsonValue jsonNews in jsonNewsAll)
		{
			// 1) create news from json
			Int32 code = jsonNews ["code"];
			Int32 category = jsonNews ["category"];
			Int32 version = jsonNews ["version"];
			string titleEn = jsonNews ["titleEn"];
			string titleEs = jsonNews ["titleEs"];
			string linkEn = jsonNews ["linkEn"];
			string linkEs = jsonNews ["linkEs"];
			string descriptionEn = jsonNews ["descriptionEn"];
			string descriptionEs = jsonNews ["descriptionEs"];
			string linkServerImage = jsonNews ["linkServerImage"];

			News newsAtJson = new News(code, category, version, false,
						titleEn, titleEs, linkEn, linkEs, descriptionEn, descriptionEs, linkServerImage);
			news_l.Add(newsAtJson);

			// 2) download image
			//if image does not exist, download here (in pingThread)
			//can be a jpeg or a png
			string extension = "";
			if(Util.IsJpeg(linkServerImage))
				extension = ".jpg";
			else if (Util.IsPng(linkServerImage))
				extension = ".png";

			//for windows specially, better download to temp and later copy to desired place,
			//if download directly, it could create a 0 bytes file
			string copyToTemp = Path.Combine(Path.GetTempPath(), code.ToString() + extension);
			string copyToNewsDir = Path.Combine(News.GetNewsDir(), code.ToString() + extension);

			/*
			 * download the image if (1 version has changed OR 2 linkServerImage has changed OR 3 image does not exists locally)
			 * but note is much better to CHANGE ALSO THE VERSION if the linkServerImage has changed
			 * if version does not change, maybe local db url never changes forcing the client to redownload the image every time Chronojump is opened
			 */
			bool needToDownloadImage = false;
			foreach(News newsAtDB in newsAtDB_l)
				if( newsAtJson.Code == newsAtDB.Code &&
						(newsAtJson.Version > newsAtDB.Version || 			// 1
						 newsAtJson.LinkServerImage != newsAtDB.LinkServerImage) 	// 2
						)
					needToDownloadImage = true;

			if(needToDownloadImage ||
					! File.Exists(copyToNewsDir) 							// 3
					)
			{
				if(downloadNewsImage(linkServerImage, copyToTemp))
					File.Copy(copyToTemp, copyToNewsDir, true);
			}
		}

		return news_l;
	}

	private bool downloadNewsImage(string linkServerImage, string copyTo)
	{
		try {
			using (WebClient client = new WebClient())
			{
				LogB.Information (string.Format("News DownloadImage from: {0} to: {1}",
							linkServerImage, copyTo));

				client.DownloadFile(new Uri(linkServerImage), copyTo); //if exists, it overwrites
			}
		} catch {
			LogB.Warning("DownloadImage catched");
			return false;
		}

		return true;
	}

	/*
	 * <---- news stuff end ----
	 */

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

		if (! createWebRequest(requestType.PING, "/ping"))
			return false;

		// Set the Method property of the request to POST.
		requestPing.Method = "POST";

		// Set the ContentType property of the WebRequest.
		requestPing.ContentType = "application/json";

		// Creates the json object
		JsonObject json = new JsonObject();

		if(osVersion.Length > 15)
			osVersion = osVersion.Substring(0,15); //at server mysql max length of this param is 15 (windows returns a longer string)

		json.Add("os_version", osVersion);

		if(cjVersion.Length > 11)
			cjVersion = cjVersion.Substring(0,11); //send only the first 11 chars, but not if version is just 2.0.0, see: buildInfo.cs

		json.Add("cj_version", cjVersion);

		json.Add("machine_id", machineID);

		// Converts it to a String
		String js = json.ToString();

		// Writes the json object into the request dataStream
		Stream dataStream;
		if(! getWebRequestStream (requestPing, out dataStream, "Could not send ping (A)."))
			return false;

		if(requestPingAborting) {
			LogB.Information("Aborted from PingAbort");
			return false;
		}

		dataStream.Write (Encoding.UTF8.GetBytes(js), 0, js.Length);

		dataStream.Close ();

		// Get the response.
		WebResponse response;
		if(! getWebResponse (requestPing, out response, "Could not send ping (B)."))
			return false;

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

	protected enum requestType { GENERIC, PING };

	protected bool createWebRequest(Json.requestType rt, string webService)
	{
		try {
			if(rt == Json.requestType.GENERIC)
				request = WebRequest.Create (serverUrl + webService);
			else //(rt == Json.requestType.PING)
				requestPing = WebRequest.Create (serverUrl + webService);

		} catch {
			this.ResultMessage =
				string.Format(Catalog.GetString("You are not connected to the Internet\nor {0} server is down."),
						serverUrl) + "\n\nMaybe proxy is not configured";
			//System.Net.WebRequest.GetSystemWebProxy System.NullReferenceException: Object reference not set to an instance of an object
			return false;
		}

		return true;
	}

	protected bool getWebRequestStream(WebRequest req, out Stream dataStream, string errMessage)
	{
		dataStream = null;

		try {
			dataStream = req.GetRequestStream ();
		} catch {
			this.ResultMessage = errMessage + "\n" +
				string.Format(Catalog.GetString("You are not connected to the Internet\nor {0} server is down."),
				serverUrl);
			return false;
		}

		return true;
	}

	protected bool getWebResponse(WebRequest req, out WebResponse response, string errMessage)
	{
		response = null;

		try {
			response = req.GetResponse ();
		} catch {
			this.ResultMessage = errMessage + "\n" +
				string.Format(Catalog.GetString("You are not connected to the Internet\nor {0} server is down."),
				serverUrl);
			return false;
		}

		return true;
	}

	protected bool getHttpWebResponse(WebRequest req, out HttpWebResponse response, string errMessage)
	{
		response = null;

		try {
			response = (HttpWebResponse) req.GetResponse ();
		} catch {
			this.ResultMessage = errMessage + "\n" +
				string.Format(Catalog.GetString("You are not connected to the Internet\nor {0} server is down."),
				serverUrl);
			return false;
		}

		return true;
	}

	public bool Connected {
		get { return connected; }
	}

	~Json() {}
}

