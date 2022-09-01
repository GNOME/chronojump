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
 * Copyright (C) 2016-2017 Carles Pina
 * Copyright (C) 2016-2022 Xavier de Blas
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
	protected bool django;
	protected string exercisesFailedStr = Catalog.GetString("Could not get station exercises.");

	//for inheritance
	public JsonCompujump ()
	{
	}

	public JsonCompujump(bool django)
	{
		this.django = django;

		ResultMessage = "";
	}

	public Person GetPersonByRFID(string rfid)
	{
		connected = false;
		Person person = new Person(-1);

		// Create a request using a URL that can receive a post.
		if (! createWebRequest(requestType.AUTHENTICATED, "/api/v1/client/getPlayerByRFID"))
			return person;

		// Set the Method property of the request to POST.
		request.Method = "POST";

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

		connected = true;

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

		return person;
	}

	public double LastPersonByRFIDHeight = 0;
	public double LastPersonByRFIDWeight = 0;
	public string LastPersonByRFIDImageURL = "";
	public bool LastPersonJustInserted = false;
	private Person personDeserialize(string strPerson)
	{
		JsonValue jsonPerson = JsonValue.Parse(strPerson);

		Int32 id = jsonPerson ["user_id"];
		string player = jsonPerson ["name"];
		double weight = jsonPerson ["weight"];
		double height = jsonPerson ["height"];
		string rfid = jsonPerson ["rfid"];
		string image = jsonPerson ["image_thumbnail"];

		LastPersonByRFIDHeight = height;
		LastPersonByRFIDWeight = weight;
		LastPersonByRFIDImageURL = image;

		Person personTemp = SqlitePerson.Select(false, id);
		/*
		 * if personTemp == -1, need to insert this person
		 * LastPersonJustInserted will be used:
		 * 	to insert person at person.cs
		 * 	to know if (it's new person or RFID changed) at gui/networks.cs
		 */
		LastPersonJustInserted = (personTemp.UniqueID == -1);

		return new Person(LastPersonJustInserted, id, player, rfid, image);
	}


	//Retrieve images from server
	private string getImagesUrl()
	{
		//Networks v3
		return serverUrl;
		/*
		int posOfLastColon = serverUrl.LastIndexOf(':');
		if(django) //Networks v2
			return serverUrl.Substring(0, posOfLastColon) + ":8000/media/";
		else //Networks v1
			return serverUrl.Substring(0, posOfLastColon) + ":5000/static/images/photos/";
		*/
	}

	//imageHalfUrl is "jugadors/*.jpg"
	public bool DownloadImage(string imageHalfUrl, int personID)
	{
		try {
			using (WebClient client = new WebClient())
			{
				LogB.Information ("Compujump DownloadImage!!");
				LogB.Information (getImagesUrl() + imageHalfUrl);
				LogB.Information (Path.Combine(Path.GetTempPath(), personID.ToString()));
				client.DownloadFile(new Uri(getImagesUrl() + imageHalfUrl),
						Path.Combine(Path.GetTempPath(), personID.ToString()));
			}
		} catch {
			LogB.Warning("Compujump DownloadImage catched");
			return false;
		}

		return true;
	}

	public string GetTasksResponse (int personID, int stationID)
	{
		connected = false;

		// Create a request using a URL that can receive a post.
		if (! createWebRequest(requestType.AUTHENTICATED, "/api/v1/client/getTasks"))
			return "";

		// Set the Method property of the request to POST.
		request.Method = "POST";

		// Creates the json object
		JsonObject json = new JsonObject();
		json.Add("player_id", personID.ToString());

		// Converts it to a String
		String js = json.ToString();

		LogB.Debug("GetTasks params: " + js + "\n");
		// Writes the json object into the request dataStream
		Stream dataStream;
		if(! getWebRequestStream (request, out dataStream, "Cannot get tasks."))
			return "";

		dataStream.Write (Encoding.UTF8.GetBytes(js), 0, js.Length);
		dataStream.Close ();

		HttpWebResponse response;
		if(! getHttpWebResponse (request, out response, "Cannot get tasks."))
			return "";

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
			return "";
		}

		return responseFromServer;
	}

	public bool UpdateTask(int taskId, int done)
	{
		LogB.Information("At UpdateTask");
		// Create a request using a URL that can receive a post.
		if (! createWebRequest(requestType.AUTHENTICATED, "/api/v1/client/updateTask"))
			return false;

		// Set the Method property of the request to POST.
		request.Method = "POST";

		// Creates the json object
		JsonObject json = new JsonObject();
		json.Add("task_id", taskId);
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
		if (! createWebRequest(requestType.AUTHENTICATED, "/api/v1/client/getOtherStationsWithPendingTasks"))
			return new List<StationCount>();

		// Set the Method property of the request to POST.
		request.Method = "POST";

		// Creates the json object
		JsonObject json = new JsonObject();
		json.Add("player_id", personID.ToString());

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
			string stationName = jsonStation ["station_name"];
			int tasksCount = jsonStation ["tasks_count"];
			stations.Add(new StationCount(stationName, tasksCount));
		}
		return stations;
	}

	public List<SelectJumpTypes> JumpSimpleExercises_l;
	public List<SelectJumpRjTypes> JumpRjExercises_l;

	public virtual void GetJumpStationExercises (int stationId)
	{
	}
	public virtual List<EncoderExercise> GetEncoderStationExercises (int stationId, Constants.EncoderGI stationType)
	{
		return new List<EncoderExercise> ();
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
		json.Add("exercise_id", exerciseId);

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
		Int32 stationId = jsonEx ["station_id"];
		int percentBodyMassDisplaced = jsonEx ["percent_body_mass_displaced"];

		return new EncoderExercise(id, name, percentBodyMassDisplaced,
				"", "", 0); //ressitance, description, speed1RM
	}
	*/

	public bool UploadJumpData (UploadJumpDataObject o, Constants.Modes m)
	{
		LogB.Information("calling jump upload, mode = " + m.ToString());
		// Create a request using a URL that can receive a post.
		if (! createWebRequest(requestType.AUTHENTICATED, "/api/v1/client/uploadJump"))
			return false;

		// Set the Method property of the request to POST.
		request.Method = "POST";

		// Creates the json object
		// debug reading on server /var/log/chronojump/debug.log

		JsonObject json = new JsonObject();
		json.Add("station_id", o.stationId);
		json.Add("exercise_id", o.ExerciseIdStr);
		json.Add("comment", "");
		json.Add("extra_weight", o.WeightInKg);
		if (m == Constants.Modes.JUMPSSIMPLE)
		{
			json.Add("player_id", o.jump.PersonID);
			json.Add("multiple", 0);
			json.Add("contact_time", Util.ConvertToPoint (o.jump.Tc));
			json.Add("flight_time", Util.ConvertToPoint (o.jump.Tv));
			json.Add("fall", Util.ConvertToPoint (o.jump.Fall));
		} else // if (m == Constants.Modes.JUMPSREACTIVE)
		{
			json.Add("player_id", o.jumpRj.PersonID);
			json.Add("multiple", 1);
			json.Add("contact_time", Util.ConvertToPoint (o.jumpRj.TcAvg));
			json.Add("flight_time", Util.ConvertToPoint (o.jumpRj.TvAvg));
			json.Add("fall", Util.ConvertToPoint (o.jumpRj.Fall));
		}
		json.Add("power", o.PowerStr);
		json.Add("stiffness", o.StiffnessStr);
		json.Add("initial_speed", o.InitialSpeedStr);

		// Converts it to a String
		String js = json.ToString();
		LogB.Information("json UploadJumpData: ", js);

		// Writes the json object into the request dataStream
		Stream dataStream;
		if(! getWebRequestStream (request, out dataStream, "Could not upload jump data."))
			return false;

		dataStream.Write (Encoding.UTF8.GetBytes(js), 0, js.Length);
		dataStream.Close ();

		// Get the response.
		WebResponse response;
		if(! getWebResponse (request, out response, "Could not upload jump data."))
			return false;

		// Display the status (will be 202, CREATED)
		Console.WriteLine (((HttpWebResponse)response).StatusDescription);

		// Clean up the streams.
		dataStream.Close ();
		response.Close ();

		this.ResultMessage = "Jump data sent.";
		return true;
	}

	public bool UploadSprintData (UploadSprintDataObject o)
	{
		LogB.Information("calling upload sprint");
		// Create a request using a URL that can receive a post.
		if (! createWebRequest(requestType.AUTHENTICATED, "/api/v1/client/uploadSprint"))
			return false;

		/*
		LogB.Information("UploadSprintData doubles:");
		foreach(double d in splitTimesL)
			LogB.Information(d.ToString());
			*/

		// Set the Method property of the request to POST.
		request.Method = "POST";

		// Creates the json object
		JsonObject json = new JsonObject();
		json.Add("player_id", o.personId);
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

		//at django upload this as strings with '.' as decimal separator
		if(django)
		{
			json.Add("k", Util.ConvertToPoint(o.k));
			json.Add("vmax", Util.ConvertToPoint(o.vmax));
			json.Add("amax", Util.ConvertToPoint(o.amax));
			json.Add("fmax", Util.ConvertToPoint(o.fmax));
			json.Add("pmax", Util.ConvertToPoint(o.pmax));
		} else {
			json.Add("k", o.k);
			json.Add("vmax", o.vmax);
			json.Add("amax", o.amax);
			json.Add("fmax", o.fmax);
			json.Add("pmax", o.pmax);
		}

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
		if (! createWebRequest(requestType.AUTHENTICATED, "/api/v1/client/uploadEncoder"))
			return false;

		// Set the Method property of the request to POST.
		request.Method = "POST";

		// Creates the json object
		JsonObject json = new JsonObject();
		json.Add("player_id", o.personId);
		json.Add("station_id", o.stationId);
		//json.Add("exercise_name", Util.RemoveAccents(exerciseName));
		json.Add("exercise_id", o.exerciseId);
		json.Add("laterality", o.laterality);
		json.Add("resistance", o.resistance);
		json.Add("repetitions", o.uo.repetitions);

		json.Add("num_by_speed", o.uo.numBySpeed);
		json.Add("loss_by_speed", o.uo.lossBySpeed);
		json.Add("range_by_speed", o.uo.rangeBySpeed);
		json.Add("vmean_by_speed", o.uo.vmeanBySpeed);
		json.Add("vmax_by_speed", o.uo.vmaxBySpeed);
		json.Add("pmean_by_speed", o.uo.pmeanBySpeed);
		json.Add("pmax_by_speed", o.uo.pmaxBySpeed);

		json.Add("num_by_power", o.uo.numByPower);
		json.Add("loss_by_power", o.uo.lossByPower);
		json.Add("range_by_power", o.uo.rangeByPower);
		json.Add("vmean_by_power", o.uo.vmeanByPower);
		json.Add("vmax_by_power", o.uo.vmaxByPower);
		json.Add("pmean_by_power", o.uo.pmeanByPower);
		json.Add("pmax_by_power", o.uo.pmaxByPower);

		// Converts it to a String
		String js = json.ToString();
		LogB.Information("json UploadEncoderData: ", js);

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

	public bool UploadForceSensorData(UploadForceSensorDataFullObject o)
	{
		// Create a request using a URL that can receive a post.
		if (! createWebRequest(requestType.AUTHENTICATED, "/api/v1/client/uploadForce"))
			return false;

		// Set the Method property of the request to POST.
		request.Method = "POST";

		// Creates the json object
		JsonObject json = new JsonObject();
		json.Add("player_id", o.personId);
		json.Add("station_id", o.stationId);
		//json.Add("exercise_name", Util.RemoveAccents(exerciseName));
		json.Add("exercise_id", o.exerciseId);
		json.Add("laterality", o.laterality);
		json.Add("resistance", o.resistance);


		json.Add("variability", o.uo.variability);
		json.Add("timeTotal", o.uo.timeTotal);
		json.Add("impulse", o.uo.impulse);
		json.Add("workJ", o.uo.workJ);
		json.Add("repetitions", o.uo.repetitions);
		json.Add("numRep", o.uo.numRep);
		json.Add("repCriteria", o.uo.repCriteria);
		json.Add("time", o.uo.time);
		json.Add("range", o.uo.range);
		json.Add("fmaxRaw", o.uo.fmaxRaw);
		json.Add("rfdmeanRaw", o.uo.rfdmeanRaw);
		json.Add("rfdmaxRaw", o.uo.rfdmaxRaw);
		json.Add("fmaxModel", o.uo.fmaxModel);
		json.Add("rfdmaxModel", o.uo.rfdmaxModel);
		json.Add("vmean", o.uo.vmean);
		json.Add("vmax", o.uo.vmax);
		json.Add("amean", o.uo.amean);
		json.Add("amax", o.uo.amax);
		json.Add("pmean", o.uo.pmean);
		json.Add("pmax", o.uo.pmax);

		// Converts it to a String
		String js = json.ToString();
		LogB.Information("json UploadForceSensorData: ", js);

		// Writes the json object into the request dataStream
		Stream dataStream;
		if(! getWebRequestStream (request, out dataStream, Catalog.GetString("Could not upload force sensor data.")))
			return false;

		dataStream.Write (Encoding.UTF8.GetBytes(js), 0, js.Length);
		dataStream.Close ();

		// Get the response.
		WebResponse response;
		if(! getWebResponse (request, out response, Catalog.GetString("Could not upload force sensor data.")))
			return false;

		// Display the status (will be 202, CREATED)
		Console.WriteLine (((HttpWebResponse)response).StatusDescription);

		// Clean up the streams.
		dataStream.Close ();
		response.Close ();

		this.ResultMessage = "Force sensor data sent.";
		return true;
	}

	~JsonCompujump() {}
}
