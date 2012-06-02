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
 *  Copyright (C) 2004-2012   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
using System.Data;
using System.Text; //StringBuilder

using Mono.Unix;

public class EncoderParams
{
	private int time;
	private string mass; //to pass always as "." to R
	private int minHeight;
	private int exercisePercentBodyWeight; //was private bool isJump; (if it's 0 is like "jump")
	private string eccon;
	private string analysis;
	private string smooth; //to pass always as "." to R
	private int curve;
	private int width;
	private int height;
	private double heightHigherCondition;
	private double heightLowerCondition;
	private double meanSpeedHigherCondition;
	private double meanSpeedLowerCondition;
	private double maxSpeedHigherCondition;
	private double maxSpeedLowerCondition;
	private int powerHigherCondition;
	private int powerLowerCondition;
	private int peakPowerHigherCondition;
	private int peakPowerLowerCondition;

	public EncoderParams()
	{
	}

	public EncoderParams(int time, int minHeight, int exercisePercentBodyWeight, string mass, string smooth, string eccon,
			double heightHigherCondition, double heightLowerCondition, 
			double meanSpeedHigherCondition, double meanSpeedLowerCondition, 
			double maxSpeedHigherCondition, double maxSpeedLowerCondition, 
			int powerHigherCondition, int powerLowerCondition, 
			int peakPowerHigherCondition, int peakPowerLowerCondition)
	{
		this.time = time;
		this.minHeight = minHeight;
		this.exercisePercentBodyWeight = exercisePercentBodyWeight;
		this.mass = mass;
		this.smooth = smooth;
		this.eccon = eccon;
		this.heightHigherCondition = heightHigherCondition;
		this.heightLowerCondition = heightLowerCondition;
		this.meanSpeedHigherCondition = meanSpeedHigherCondition;
		this.meanSpeedLowerCondition = meanSpeedLowerCondition;
		this.maxSpeedHigherCondition = maxSpeedHigherCondition;
		this.maxSpeedLowerCondition = maxSpeedLowerCondition;
		this.powerHigherCondition = powerHigherCondition;
		this.powerLowerCondition = powerLowerCondition;
		this.peakPowerHigherCondition = peakPowerHigherCondition;
		this.peakPowerLowerCondition = peakPowerLowerCondition;
	}
	
	public string ToString1 () 
	{
		return time.ToString() + " " + minHeight.ToString() + " " + exercisePercentBodyWeight.ToString() + 
			" " + mass.ToString() + " " + smooth + " " + eccon +
			" " + heightHigherCondition.ToString() +	" " + heightLowerCondition.ToString() +
			" " + Util.ConvertToPoint(meanSpeedHigherCondition.ToString()) + 	
			" " + Util.ConvertToPoint(meanSpeedLowerCondition.ToString()) +
			" " + Util.ConvertToPoint(maxSpeedHigherCondition.ToString()) + 	
			" " + Util.ConvertToPoint(maxSpeedLowerCondition.ToString()) +
			" " + powerHigherCondition.ToString() + 	" " + powerLowerCondition.ToString() +
			" " + peakPowerHigherCondition.ToString() + 	" " + peakPowerLowerCondition.ToString();
	}
	
	public EncoderParams(int minHeight, int exercisePercentBodyWeight, string mass, string eccon, 
			string analysis, string smooth, int curve, int width, int height)
	{
		this.minHeight = minHeight;
		this.exercisePercentBodyWeight = exercisePercentBodyWeight;
		this.mass = mass;
		this.eccon = eccon;
		this.analysis = analysis;
		this.smooth = smooth;
		this.curve = curve;
		this.width = width;
		this.height = height;
	}
	
	public string ToString2 () 
	{
		return minHeight + " " + exercisePercentBodyWeight + " " + mass + " " + eccon + " " + analysis + " " + 
			smooth + " " + curve + " " + width + " " + height;
	}
	
	public string Analysis {
		get { return analysis; }
	}
	

	~EncoderParams() {}
}

public class EncoderStruct
{
	public EncoderStruct() {
	}

	public string InputData;
	public string OutputGraph;
	public string OutputData1;
	public string OutputData2;
	public EncoderParams Ep;

	public EncoderStruct(string InputData, string OutputGraph, string OutputData1, string OutputData2,
		       EncoderParams Ep)
	{
		this.InputData = InputData;
		this.OutputGraph = OutputGraph;
		this.OutputData1 = OutputData1;
		this.OutputData2 = OutputData2;
		this.Ep = Ep;
	}

	~EncoderStruct() {}
}

//used on TreeView
public class EncoderCurve
{
	public EncoderCurve () {
	}

	public EncoderCurve (string n, string start, string duration, string height, string meanSpeed, string maxSpeed, 
			string meanPower, string peakPower, string peakPowerT, string PP_PPT)
	{
		this.N = n;
		this.Start = start;
		this.Duration = duration;
		this.Height = height;
		this.MeanSpeed = meanSpeed;
		this.MaxSpeed = maxSpeed;
		this.MeanPower = meanPower;
		this.PeakPower = peakPower;
		this.PeakPowerT = peakPowerT;
		this.PP_PPT = PP_PPT;	//PeakPower / PeakPowerTime
	}

	public string N;
	public string Start;
	public string Duration;
	public string Height;
	public string MeanSpeed;
	public string MaxSpeed;
	public string MeanPower;
	public string PeakPower;
	public string PeakPowerT;
	public string PP_PPT;
}

public class EncoderSQL
{
	public string uniqueID;
	public int personID;
	public int sessionID;
	public int exerciseID;
	public string eccon;
	public string laterality;
	public string extraWeight;
	public string signalOrCurve;
	public string filename;
	public string url;
	public int time;
	public int minHeight;
	public double smooth;
	public string description;
	public string future1;
	public string future2;
	public string future3;
	public string exerciseName;
	
	public string ecconLong;
	
	public EncoderSQL ()
	{
	}

	public EncoderSQL (string uniqueID, int personID, int sessionID, int exerciseID, 
			string eccon, string laterality, string extraWeight, string signalOrCurve, 
			string filename, string url, int time, int minHeight, double smooth, 
			string description, string future1, string future2, string future3, 
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
		this.smooth = smooth;
		this.description = description;
		this.future1 = future1;
		this.future2 = future2;
		this.future3 = future3;
		this.exerciseName = exerciseName;

		if(eccon == "c")
			ecconLong = Catalog.GetString("Concentric");
		else
			ecconLong = Catalog.GetString("Eccentric-concentric");
	}
	
	public string GetDate(bool pretty) {
		int pointPos = filename.LastIndexOf('.');
		int dateLength = 19; //YYYY-MM-DD_hh-mm-ss
		string date = filename.Substring(pointPos - dateLength, dateLength);
		if(pretty) {
			string [] dateParts = date.Split(new char[] {'_'});
			date = dateParts[0] + " " + dateParts[1].Replace('-',':');
		}
		return date;
	}

	public string [] ToStringArray () {
		string [] str = new String [6];
		str[0] = uniqueID;
		str[1] = exerciseName;
		str[2] = ecconLong;
		str[3] = extraWeight;
		str[4] = GetDate(true);
		str[5] = description;
		return str;
	}

}

public class EncoderExercise
{
	public EncoderExercise() {
	}

	public int uniqueID;
	public string name;
	public int percentBodyWeight;
	public string ressistance;
	public string description;

	public EncoderExercise(int uniqueID, string name, int percentBodyWeight, string ressistance, string description)
	{
		this.uniqueID = uniqueID;
		this.name = name;
		this.percentBodyWeight = percentBodyWeight;
		this.ressistance = ressistance;
		this.description = description;
	}

	~EncoderExercise() {}
}

