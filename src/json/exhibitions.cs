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


public class JsonExhibitions : Json
{
	public JsonExhibitions()
	{
		ResultMessage = "";
	}

	//table created with:
	//CREATE TABLE exhibitionTest(dt TIMESTAMP DEFAULT CURRENT_TIMESTAMP, schoolID INT NOT NULL, groupID INT NOT NULL, personID INT NOT NULL, testType CHAR(10), result DOUBLE);
	public bool UploadExhibitionTest(ExhibitionTest et)
	{
		// Create a request using a URL that can receive a post.
		if (! createWebRequest(requestType.AUTHENTICATED, "/api/v1/client/uploadExhibitionTestData"))
			return false;

		// Set the Method property of the request to POST.
		request.Method = "POST";

		// Set the ContentType property of the WebRequest.
		request.ContentType = "application/json; Charset=UTF-8"; //but this is not enough, see this line:
		//exerciseName = Util.RemoveAccents(exerciseName);

		// Creates the json object
		JsonObject json = new JsonObject();

		json.Add("schoolID", et.schoolID);
		json.Add("groupID", et.groupID);
		json.Add("personID", et.personID);
		json.Add("testType", et.testType.ToString());
		json.Add("result", et.resultToJson);

		// Converts it to a String
		String js = json.ToString();

		// Writes the json object into the request dataStream
		Stream dataStream;
		if(! getWebRequestStream (request, out dataStream, "Could not upload exhibition test data (A)."))
			return false;

		dataStream.Write (Encoding.UTF8.GetBytes(js), 0, js.Length);

		dataStream.Close ();

		// Get the response.
		WebResponse response;
		if(! getWebResponse (request, out response, "Could not upload exhibition test data (B)."))
			return false;

		// Display the status (will be 202, CREATED)
		Console.WriteLine (((HttpWebResponse)response).StatusDescription);

		// Clean up the streams.
		dataStream.Close ();
		response.Close ();

		this.ResultMessage = "Exhibition test data sent.";
		return true;
	}

	~JsonExhibitions() {}
}


//eg. YOMO
public class ExhibitionTest
{
	public int schoolID;
	public int groupID;
	public int personID;
	public enum testTypes { JUMP, RUN, INERTIAL, FORCE_ROPE, FORCE_SHOT };
	public testTypes testType;
	public double result;
	/* result is:
	 * 	on jumps is height
	 * 	on runs is maximum speed ?
	 * 	on pull rope is maximum force
	 * 	on shot is maximum force
	 * 	on inertial is mean power of the maximum repetiton
	 */

	public ExhibitionTest(int schoolID, int groupID, int personID, testTypes testType, double result)
	{
		this.schoolID = schoolID;
		this.groupID = groupID;
		this.personID = personID;
		this.testType = testType;
		this.result = result;
	}

	public string ToSQLTempInsertString()
	{
		return
			schoolID.ToString() + ", " +
			groupID.ToString() + ", " +
			personID.ToString() + ", \"" +
			testType.ToString() + "\", " +
			resultToJson;
	}

	//convert to decimal point and str
	public string resultToJson {
		get { return Util.ConvertToPoint(result); }
	}

	~ExhibitionTest() {}
}
