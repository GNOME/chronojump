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
 * Copyright (C) 2015 Carles Pina & Xavier de Blas
 */

using System;
using System.Net;
using System.Web;
using System.IO;
using System.Json;
using Mono.Unix;

public class JsonPost
{
	public string ResultMessage;

	public JsonPost()
	{
		ResultMessage = "";
	}

	public bool PostCrashLog() 
	{
		string filePath = UtilAll.GetLogFileOld();
		if(! File.Exists(filePath)) {
			this.ResultMessage = Catalog.GetString("Could not send file.\nIt does not exist.");
			return false;
		}

		// Create a request using a URL that can receive a post. 
		WebRequest request = WebRequest.Create ("http://api.chronojump.org:8080/backtrace");

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
			this.ResultMessage = Catalog.GetString("Could not send file.\nYou are not connected to the Internet\nor (http://api.chronojump.org:8080) server is down.");
			return false;
		}

		// Write the data to the request stream.
		dataStream.Write (byteArray, 0, byteArray.Length);

		// Close the Stream object.
		dataStream.Close ();

		// Get the response.
		WebResponse response = request.GetResponse ();

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

	~JsonPost() {}
}
