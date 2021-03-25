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
 *  Copyright (C) 2017-2021   Xavier de Blas <xaviblas@gmail.com>
 */

using System;
using System.IO; 		//for detect OS
using System.Collections.Generic; //List<T>
using Mono.Unix;

//to draw a graph on R
//but also to calculate params to be uploaded on networks
public class SprintRGraph
{
	private string positions;
	private string splitTimes;
	private double mass;
	private double personHeight;
	private string personName;
	private double tempC;
	private char exportDecimalSeparator;
	private bool includeImagesOnExport;

	private string errorMessage;

	//constructor for 1 set
	public SprintRGraph (string positions, string splitTimes,
			double mass, double personHeight, string personName, double tempC)
	{
		this.positions = positions;
		this.splitTimes = splitTimes;
		this.mass = mass;
		this.personHeight = personHeight;
		this.personName = personName;
		this.tempC = tempC;

		this.exportDecimalSeparator = '.';
		this.includeImagesOnExport = false;

		errorMessage = "";
	}

	//constructor for export (many sets of possible different persons)
	public SprintRGraph (List<SprintRGraphExport> sprge_l,
			char exportDecimalSeparator,
			bool includeImagesOnExport)
	{
		//to have Roptions.txt with data on row
		this.positions = "-1";
		this.splitTimes = "-1";
		this.mass = -1;
		this.personHeight = -1;
		this.personName = "-1";
		this.tempC = -1;

		this.exportDecimalSeparator = exportDecimalSeparator;
		this.includeImagesOnExport = includeImagesOnExport;

		writeMultipleFilesCSV(sprge_l);
	}

	public bool CallR(int graphWidth, int graphHeight, bool singleOrMultiple)
	{
		LogB.Information("\nsprint CallR ----->");
		writeOptionsFile(graphWidth, graphHeight,singleOrMultiple);
		return ExecuteProcess.CallR(UtilEncoder.GetSprintPhotocellsScript());
	}

	private void writeOptionsFile(int graphWidth, int graphHeight, bool singleOrMultiple)
	{
		/*
		string scriptOptions =
			"#positions\n" + 	"0;20;40;70" + "\n" +
			"#splitTimes\n" + 	"0;2.73;4.49;6.95" + "\n" +
			"#mass\n" + 		"75" + "\n" +
			"#personHeight\n" + 	"1.65" + "\n" +
			"#tempC\n" + 		"25" + "\n";
			*/
		string scriptsPath = UtilEncoder.GetSprintPath();
		if(UtilAll.IsWindows())
			scriptsPath = scriptsPath.Replace("\\","/");

		string scriptOptions =
			"#scriptsPath\n" + 	scriptsPath + "\n" +
			"#os\n" + 		UtilEncoder.OperatingSystemForRGraphs() + "\n" +
			"#graphWidth\n" + 	graphWidth.ToString() + "\n" +
			"#graphHeight\n" + 	graphHeight.ToString() + "\n" +
			//all the following (until tempC (included)) are unused on multiple
			"#positions\n" + 	positions + "\n" +
			"#splitTimes\n" + 	splitTimes + "\n" +
			"#mass\n" + 		Util.ConvertToPoint(mass) + "\n" +
			"#personHeight\n" + 	Util.ConvertToPoint(personHeight / 100.0) + "\n" + //send it in meters
			"#personName\n" + 	personName + "\n" +
			"#tempC\n" + 		tempC + "\n" +
			"#singleOrMultiple\n" +		Util.BoolToRBool(singleOrMultiple) + "\n" +
			"#decimalCharAtExport\n" +	exportDecimalSeparator + "\n" +
			"#includeImagesOnExport\n" + 	Util.BoolToRBool(includeImagesOnExport) + "\n";

		TextWriter writer = File.CreateText(Path.GetTempPath() + "Roptions.txt");
		writer.Write(scriptOptions);
		writer.Flush();
		writer.Close();
		((IDisposable)writer).Dispose();
	}

	private void writeMultipleFilesCSV(List<SprintRGraphExport> sprge_l)
	{
		LogB.Information("writeMultipleFilesCSV start");
		TextWriter writer = File.CreateText(RunInterval.GetCSVInputMulti());

		//write header
		writer.WriteLine(SprintRGraphExport.PrintCSVHeaderOnExport());

		//write sprge_l for
		foreach(SprintRGraphExport sprge in sprge_l)
			writer.WriteLine(sprge.ToCSVRowOnExport());

		writer.Flush();
		writer.Close();
		((IDisposable)writer).Dispose();
		LogB.Information("writeMultipleFilesCSV end");
	}

	public List<double> GetSplitTimesAsList()
	{
		string [] splitTimesArray = splitTimes.Split(new char[] {';'});

		List<double> splitTimesList = new List<double>();
		foreach(string time in splitTimesArray)
		{
			//in seconds
			double timeD = Convert.ToDouble(Util.ChangeDecimalSeparator(time));

			splitTimesList.Add(timeD);
		}

		return splitTimesList;
	}

	/*
	 * if there are double contacts problems and first contacts are very close,
	 * R algorithm gets very slow and program seems frozen
	 */
	public bool IsDataOk()
	{
		List<double> speedsL = speedAsDoubleL();
		double speedMax = -1;
		int trackMax = 1;
		int count = 1;

		foreach(double speed in speedsL)
		{
			LogB.Information("speed: " + speed.ToString());
			if(speedMax < 0)
				speedMax = speed;
			else if( (2 * speed) < speedMax)
			{
				errorMessage = string.Format(
							Catalog.GetString("Track {0} ({1} m/s) is much faster than track {2} ({3} m/s)."),
							trackMax, Math.Round(speedMax,2), count, Math.Round(speed, 2));
				return false;
			}
			else if(speed > speedMax)
			{
				speedMax = speed;
				trackMax = count;
			}

			count ++;
		}

		errorMessage = "";
		return true;
	}

	private List<double> speedAsDoubleL()
	{
		List<double> speedsL = new List<double>();
		List<double> positionsL = positionsAsDoubleL();
		List<double> splitTimesL = splitTimesAsDoubleL();

		//start with 1 because at 0 is the first contact
		for(int i = 1; i < positionsL.Count ; i ++)
		{
			double speed = 1.0 * (positionsL[i] - positionsL[i-1]) /
				(1.0 * (splitTimesL[i] - splitTimesL[i-1]));
			speedsL.Add(speed);
		}
		return speedsL;
	}

	private List<double> positionsAsDoubleL()
	{
		List<double> l = new List<double>();
		string [] positionsFull = positions.Split(new char[] {';'});
		foreach(string p in positionsFull)
		{
			double d = Convert.ToDouble(Util.ChangeDecimalSeparator(p));
			l.Add(d);
		}
		return l;
	}

	private List<double> splitTimesAsDoubleL()
	{
		List<double> l = new List<double>();
		string [] splitTimesFull = splitTimes.Split(new char[] {';'});
		foreach(string s in splitTimesFull)
		{
			double d = Convert.ToDouble(Util.ChangeDecimalSeparator(s));
			l.Add(d);
		}
		return l;
	}

	public string Positions {
		get { return positions; }
	}

	public string SplitTimes {
		get { return splitTimes; }
	}

	public string ErrorMessage {
		get { return errorMessage; }
	}
}

//this class creates the rows of each set for the csv input multi that is read by R
public class SprintRGraphExport
{
	private string positions;
	private string splitTimes;
	private double mass;
	private double personHeight;
	private string personName;
	private double tempC;

	public SprintRGraphExport(
			string positions, string splitTimes,
			double mass, double personHeight,
			string personName, double tempC)
	{
		//if decimal is comma, will be converted to point for R, and also the ; will be _ to differentiate from other ;
		this.positions = "0_" + Util.ChangeChars(
				Util.ChangeChars(positions, ",", "."), ";", "_");
		//if decimal is comma, will be converted to point for R, and also the ; will be _ to differentiate from other ;
		this.splitTimes = "0_" + Util.ChangeChars(
				Util.ChangeChars(splitTimes, ",", "."), ";", "_");

		this.mass = mass;
		this.personHeight = personHeight;
		this.personName = personName;
		this.tempC = tempC;

	}

	public string ToCSVRowOnExport()
	{
		return positions + ";" +
			splitTimes + ";" +
			Util.ConvertToPoint(mass) + ";" +
			Util.ConvertToPoint(personHeight / 100.0) + ";" + //in meters
			personName + ";" +
			Util.ConvertToPoint(tempC);
	}

	public static string PrintCSVHeaderOnExport()
	{
		return "positions;splitTimes;mass;personHeight;personName;tempC;" +
			"comments";
	}
}
