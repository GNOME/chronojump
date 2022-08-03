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
using System.IO;
using System.Json;
using System.Text;
using System.Collections;
using System.Collections.Generic; //List<>

public class JsonCompujumpEncoder : JsonCompujump
{
	public JsonCompujumpEncoder (bool django)
	{
		this.django = django;

		ResultMessage = "";
	}

	//stationType can be GRAVITATORY or INERTIAL
	public override List<EncoderExercise> GetEncoderStationExercises (int stationId, Constants.EncoderGI stationType)
	{
		List<EncoderExercise> ex_list = new List<EncoderExercise>();

		// Create a request using a URL that can receive a post.
		if (! createWebRequest(requestType.AUTHENTICATED, "/api/v1/client/getStationExercises"))
			return ex_list;

		// Set the Method property of the request to GET.
		request.Method = "GET";

		HttpWebResponse response;
		if(! getHttpWebResponse (request, out response, exercisesFailedStr))
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
			ex_list = stationExercisesDeserialize(responseFromServer, stationId, stationType);
		}

		return ex_list;
	}

	private List<EncoderExercise> stationExercisesDeserialize (string str, int stationId, Constants.EncoderGI stationType)
	{
		List<EncoderExercise> ex_list = new List<EncoderExercise>();
		JsonValue jsonStationExercises = JsonValue.Parse (str);

		foreach (JsonValue jsonSE in jsonStationExercises)
		{
			/*
			   not needed, as exercises on the json are the related to our station
			   and this code uses machineID, but the list jsonSEStation ["id"] contains logical instead of physical.
			   so no need to check anything
			// 1) discard exercise if is not for this station
			JsonValue jsonSEStations = JsonValue.Parse (jsonSE["stations"].ToString());
			bool exerciseForThisStation = false;
			foreach (JsonValue jsonSEStation in jsonSEStations)
			{
				Int32 stations_id = jsonSEStation ["id"];
				if(stations_id == stationId)
					exerciseForThisStation = true;
			}

			if(! exerciseForThisStation)
				continue;
			*/

			// 2) discard if is not for this station type
			string type = jsonSE ["measurable"];
			Constants.EncoderGI newExEncoderGi = Constants.EncoderGI.GRAVITATORY;
			if(type == "E")
				newExEncoderGi = Constants.EncoderGI.GRAVITATORY;
			else if(type == "I")
				newExEncoderGi = Constants.EncoderGI.INERTIAL;
			else
				continue;

			if(stationType != newExEncoderGi)
				continue;

			// 3) add exercise to the list
			Int32 newExId = jsonSE ["id"];
			string newExName = jsonSE ["name"];

			int newExPercentBodyMassDisplaced = 0;
			if(jsonSE ["measurable_info"]["percent_body_mass_displaced"] != null)
				newExPercentBodyMassDisplaced = jsonSE ["measurable_info"]["percent_body_mass_displaced"];

			double newExSpeedAt1RM = 0;
			if(type == "G" && jsonSE ["measurable_info"]["speed_at_one_rm"] != null)
				newExSpeedAt1RM = Convert.ToDouble(Util.ChangeDecimalSeparator(
							jsonSE ["measurable_info"]["speed_at_one_rm"].ToString() )); //ToString is mandatory

			ex_list.Add(new EncoderExercise(newExId, newExName, newExPercentBodyMassDisplaced,
						"", "", newExSpeedAt1RM, newExEncoderGi));
		}
		return ex_list;
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

	public double  pmeanByPowerAsDouble;

	private ArrayList curves;
	private string eccon;

	//constructor called after capture
	public UploadEncoderDataObject(ArrayList curves, string eccon)
	{
		this.curves = curves;
		this.eccon = eccon;
	}

	//returns false if on discarding, there are no curves
	public bool InertialDiscardFirstN(int inertialDiscardFirstN)
	{
		if(eccon == "c")
		{
			if(curves.Count > inertialDiscardFirstN)
				curves.RemoveRange(0, inertialDiscardFirstN);
			else
				return false;
		} else {
			if(curves.Count > inertialDiscardFirstN *2)
				curves.RemoveRange(0, inertialDiscardFirstN *2);
			else
				return false;
		}

		return true;
	}

	public void Calcule (Preferences.EncoderRepetitionCriteria repCriteria)
	{
		if(eccon == "c")
			calculeObjectCon (curves);
		else
			calculeObjectEccCon (curves, repCriteria);
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

		lossBySpeed = getConLoss(curves, byTypes.SPEED);
		lossByPower = getConLoss(curves, byTypes.POWER);
	}

	private void calculeObjectEccCon (ArrayList curves, Preferences.EncoderRepetitionCriteria repCriteria)
	{
		repetitions = curves.Count / 2;
		EncoderSignal eSignal = new EncoderSignal(curves);

		//this n is the n of the ecc curve
		int nSpeed = eSignal.FindPosOfBestEccCon(0, Constants.MeanSpeed, repCriteria);
		int nPower = eSignal.FindPosOfBestEccCon(0, Constants.MeanPower, repCriteria);

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

		//lossBySpeed = eSignal.GetEccConLoss(Constants.MeanSpeed);
		//lossByPower = eSignal.GetEccConLoss(Constants.MeanPower);
		lossBySpeed = eSignal.GetEccConLossByOnlyConPhase(Constants.MeanSpeed);
		lossByPower = eSignal.GetEccConLossByOnlyConPhase(Constants.MeanPower);
	}

	//constructor called on SQL load SqliteJson.SelectTempEncoder()
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

	private int getConLoss(ArrayList curves, byTypes by)
	{
		double lowest = 100000;
		double highest = 0;
		int highestPos = 0;

		int i=0;
		foreach (EncoderCurve curve in curves)
		{
			double compareTo = curve.MeanSpeedD;
			if(by == byTypes.POWER)
				compareTo = curve.MeanPowerD;

			bool needChangeLowest = false;
			if(compareTo > highest)
			{
				highest = compareTo;
				highestPos = i;
				needChangeLowest = true; 	//min rep has to be after max
			}
			if(needChangeLowest || (compareTo < lowest &&
						((EncoderCurve) curves[i]).GetParameter(Constants.Range) >= .7 * ((EncoderCurve) curves[highestPos]).GetParameter(Constants.Range)
					       ))
				lowest = compareTo;

			//LogB.Information(string.Format("Loss (con) of {0}; i: {1} is: {2}", by.ToString(), i++, Convert.ToInt32(UtilAll.DivideSafe(100.0 * (highest - lowest), highest))));
			i ++;
		}
		return Convert.ToInt32(UtilAll.DivideSafe(100.0 * (highest - lowest), highest));
	}
}
