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


public class JsonCompujump : Json
{
	public JsonCompujump()
	{
		ResultMessage = "";
	}

	public Person GetPersonByRFID(string rfid)
	{
		connected = false;
		Person person = new Person(-1);

		// Create a request using a URL that can receive a post.
		if (! createWebRequest(requestType.GENERIC, "/getPersonByRFID"))
			return person;

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
		if(! getWebRequestStream (request, out dataStream, "Cannot get person by RFID."))
			return person;

		dataStream.Write (Encoding.UTF8.GetBytes(js), 0, js.Length);

		dataStream.Close ();
		
		HttpWebResponse response;
		if(! getHttpWebResponse (request, out response, "Cannot get person by RFID."))
			return person;

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
		if (! createWebRequest(requestType.GENERIC, "/getTasks"))
			return new List<Task>();

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
		if(! getWebRequestStream (request, out dataStream, "Cannot get tasks."))
			return new List<Task>();

		dataStream.Write (Encoding.UTF8.GetBytes(js), 0, js.Length);
		dataStream.Close ();

		HttpWebResponse response;
		if(! getHttpWebResponse (request, out response, "Cannot get tasks."))
			return new List<Task>();

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
		if (! createWebRequest(requestType.GENERIC, "/updateTask"))
			return false;

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
		if(! getWebRequestStream (request, out dataStream, "Cannot update task"))
			return false;

		dataStream.Write (Encoding.UTF8.GetBytes(js), 0, js.Length);

		dataStream.Close ();

		// Get the response.
		WebResponse response;
		if(! getWebResponse (request, out response, "Cannot update task"))
			return false;

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
		if (! createWebRequest(requestType.GENERIC, "/getOtherStationsWithPendingTasks"))
			return new List<StationCount>();

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
		if(! getWebRequestStream (request, out dataStream, Catalog.GetString("Could not get tasks from other sessions.")))
			return new List<StationCount>();

		dataStream.Write (Encoding.UTF8.GetBytes(js), 0, js.Length);
		dataStream.Close ();

		HttpWebResponse response;
		if(! getHttpWebResponse (request, out response, Catalog.GetString("Could not get tasks from other sessions.")))
			return new List<StationCount>();

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
		if (! createWebRequest(requestType.GENERIC, "/getStationExercises"))
			return ex_list;

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
		if(! getWebRequestStream (request, out dataStream, Catalog.GetString("Could not get station exercises.")))
			return ex_list;

		dataStream.Write (Encoding.UTF8.GetBytes(js), 0, js.Length);

		dataStream.Close ();

		HttpWebResponse response;
		if(! getHttpWebResponse (request, out response, Catalog.GetString("Could not get station exercises.")))
			return ex_list;


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
		if (! createWebRequest(requestType.GENERIC, "/uploadSprintData"))
			return false;

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
		if(! getWebRequestStream (request, out dataStream, Catalog.GetString("Could not upload sprint data.")))
			return false;

		dataStream.Write (Encoding.UTF8.GetBytes(js), 0, js.Length);

		dataStream.Close ();

		// Get the response.
		WebResponse response;
		if(! getWebResponse (request, out response, Catalog.GetString("Could not upload sprint data.")))
			return false;


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
		if (! createWebRequest(requestType.GENERIC, "/uploadEncoderData"))
			return false;

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
		if(! getWebRequestStream (request, out dataStream, Catalog.GetString("Could not upload encoder data.")))
			return false;

		dataStream.Write (Encoding.UTF8.GetBytes(js), 0, js.Length);

		dataStream.Close ();

		// Get the response.
		WebResponse response;
		if(! getWebResponse (request, out response, Catalog.GetString("Could not upload encoder data.")))
			return false;

		// Display the status (will be 202, CREATED)
		Console.WriteLine (((HttpWebResponse)response).StatusDescription);

		// Clean up the streams.
		dataStream.Close ();
		response.Close ();

		this.ResultMessage = "Encoder data sent.";
		return true;
	}

	~JsonCompujump() {}
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

	public string eccon; //"c" or "ec"
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

	public double  pmeanByPowerAsDouble;

	//constructor called after capture
	public UploadEncoderDataObject(ArrayList curves, string eccon)
	{
		if(eccon == "c")
			calculeObjectCon (curves);
		else
			calculeObjectEccCon (curves);
	}

	private void calculeObjectCon (ArrayList curves)
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

		pmeanByPowerAsDouble = Convert.ToDouble(curveByPower.MeanPower);

		//add +1 to show to user
		numBySpeed = nSpeed + 1;
		numByPower = nPower + 1;

		lossBySpeed = getLoss(curves, byTypes.SPEED);
		lossByPower = getLoss(curves, byTypes.POWER);
	}

	private void calculeObjectEccCon (ArrayList curves)
	{
		repetitions = curves.Count / 2;
		EncoderSignal eSignal = new EncoderSignal(curves);

		//this n is the n of the ecc curve
		int nSpeed = eSignal.FindPosOfBestEccCon(0, Constants.MeanSpeed);
		int nPower = eSignal.FindPosOfBestEccCon(0, Constants.MeanPower);

		rangeBySpeed = Util.ConvertToPoint( eSignal.GetEccConMax(nSpeed, Constants.Range) );
		rangeByPower = Util.ConvertToPoint( eSignal.GetEccConMax(nPower, Constants.Range) );

		vmeanBySpeed = Util.ConvertToPoint( eSignal.GetEccConMean(nSpeed, Constants.MeanSpeed) );
		vmeanByPower = Util.ConvertToPoint( eSignal.GetEccConMean(nPower, Constants.MeanSpeed) );
		vmaxBySpeed = Util.ConvertToPoint( eSignal.GetEccConMax(nSpeed, Constants.MaxSpeed) );
		vmaxByPower = Util.ConvertToPoint( eSignal.GetEccConMax(nPower, Constants.MaxSpeed) );

		pmeanBySpeed = Util.ConvertToPoint( eSignal.GetEccConMean(nSpeed, Constants.MeanPower) );
		pmeanByPower = Util.ConvertToPoint( eSignal.GetEccConMean(nPower, Constants.MeanPower) );
		pmaxBySpeed = Util.ConvertToPoint( eSignal.GetEccConMax(nSpeed, Constants.PeakPower) );
		pmaxByPower = Util.ConvertToPoint( eSignal.GetEccConMax(nPower, Constants.PeakPower) );

		pmeanByPowerAsDouble = Convert.ToDouble( eSignal.GetEccConMean(nPower, Constants.MeanPower) );

		//add +1 to show to user
		numBySpeed = (nSpeed /2) + 1;
		numByPower = (nPower /2) + 1;

		lossBySpeed = eSignal.GetEccConLoss(Constants.MeanSpeed);
		lossByPower = eSignal.GetEccConLoss(Constants.MeanPower);
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
