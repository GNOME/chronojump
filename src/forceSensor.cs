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
 *  Copyright (C) 2017   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
using System.IO; 		//for detect OS
using System.Collections.Generic; //List<T>
using Mono.Unix;

public class ForceSensorExercise
{
	private int uniqueID;
	private string name;
	private int percentBodyWeight;
	private string resistance;
	private int angleDefault;
	private string description;

	public ForceSensorExercise()
	{
	}

	public ForceSensorExercise(string name)
	{
		this.name = name;
	}

	public ForceSensorExercise(int uniqueID, string name, int percentBodyWeight, string resistance, int angleDefault, string description)
	{
		this.uniqueID = uniqueID;
		this.name = name;
		this.percentBodyWeight = percentBodyWeight;
		this.resistance = resistance;
		this.angleDefault = angleDefault;
		this.description = description;
	}

	public override string ToString()
	{
		return uniqueID.ToString() + ":" + name + ":" + percentBodyWeight.ToString() + ":" +
			resistance + ":" + angleDefault.ToString() + ":" + description;
	}

	public int UniqueID
	{
		get { return uniqueID; }
	}
	public string Name
	{
		get { return name; }
	}
	public int PercentBodyWeight
	{
		get { return percentBodyWeight; }
	}
	public string Resistance
	{
		get { return resistance; }
	}
	public int AngleDefault
	{
		get { return angleDefault; }
	}
	public string Description
	{
		get { return description; }
	}
}

/*
 * TODO: this class only contains points plot stuff
 * currently all the code relevant to force sensor actions is on gui/forcesensor.cs
 * that code should be here and there only the gui stuff
 */
public class ForceSensorCapturePoints
{
	//ForceCapturePoints stored to be realtime displayed
	public List<Gdk.Point> Points;
	public int NumCaptured;
	public int NumPainted;

	//used to redo all points if change RealWidthG or RealHeightG
	private List<int> times;
	private List<double> forces;

	public int RealWidthG; //width of graph in microseconds (will be upgraded if needed)

	public const int DefaultRealHeightG = 20;
	public const int DefaultRealHeightGNeg = 10;
	public int RealHeightG; //Newtons (will be upgraded if needed)
	public int RealHeightGNeg; //Newtons (negative) (will be upgraded if needed)

	private int widthG;
	private int heightG;
	private int marginLeft = 45; //px
	private int marginRight = 30; //px
	private int marginTop = 30; //px
	private int marginBottom = 30; //px

	//initialize
	public ForceSensorCapturePoints(int widthG, int heightG)
	{
		Points = new List<Gdk.Point>();
		NumCaptured = 0;
		NumPainted = 0; 	//-1 means delete screen
		times = new List<int>();
		forces = new List<double>();

		InitRealWidthHeight();

		this.widthG = widthG;
		this.heightG = heightG;
	}

	public void InitRealWidthHeight()
	{
		RealWidthG = 10000000; //width of graph in microseconds (will be upgraded if needed)
		RealHeightG = DefaultRealHeightG; //Newtons (will be upgraded when needed) (nice to see the +25 -25 marks)
		RealHeightGNeg = DefaultRealHeightGNeg; //Newtons (will be upgraded when needed) (nice to see the +25 -25 marks)
	}

	public void Add(int time, double force)
	{
		times.Add(time);
		forces.Add(force);
		Points.Add(new Gdk.Point(GetTimeInPx(time), GetForceInPx(force)));
	}

	public int GetTimeInPx(int time)
	{
		//without 1.0 calculation is done as int producing very buggy value
		return marginLeft + Convert.ToInt32(1.0 * (widthG -marginLeft -marginRight) * time / RealWidthG);
	}

	public int GetForceInPx(double force)
	{
		/*
		 * simmetrical positive / negative
		return Convert.ToInt32(
				(heightG/2)
				- ( Util.DivideSafe((force * heightG), (1.0 * RealHeightG)) )
				);
				*/
		return Convert.ToInt32(
				heightG
				- Util.DivideSafe(
						(force * (heightG - (marginTop + marginBottom))),
						(1.0 * (RealHeightG + RealHeightGNeg))
						)
				- Util.DivideSafe(
						RealHeightGNeg * (heightG - (marginTop + marginBottom)),
						(1.0 * (RealHeightG + RealHeightGNeg))
						)
				- marginBottom
				);
	}

	private Gdk.Point getLastPoint()
	{
		return Points[Points.Count -1];
	}

	// TODO: do all this in an inherited class
	public int GetLength()
	{
		return times.Count;
	}

	public int GetLastTime()
	{
		return times[times.Count -1];
	}

	public double GetLastForce()
	{
		return forces[forces.Count -1];
	}

	public double GetTimeAtCount(int count)
	{
		return times[count];
	}
	public double GetForceAtCount(int count)
	{
		return forces[count];
	}
	public void GetAverageAndMaxForce(int countA, int countB, out double avg, out double max)
	{
		if(countA == countB) {
			avg = forces[countA];
			max = forces[countA];
			return;
		}

		double sum = 0;
		max = 0;
		for(int i = countA; i <= countB; i ++) {
			sum += forces[i];
			if(forces[i] > max)
				max = forces[i];
		}

		avg = sum / (countB - countA);
	}
	public double GetRFD(int countA, int countB)
	{
		double calc = (forces[countB] - forces[countA]) / (times[countB]/1000000.0 - times[countA]/1000000.0); //microsec to sec
		//LogB.Information(string.Format("GetRFD {0}, {1}, {2}, {3}, {4}, {5}, RFD: {6}",
		// 	countA, countB, forces[countA], forces[countB], times[countA], times[countB], calc));
		return calc;
	}
	public double GetImpulse(int countA, int countB)
	{
		double sum = 0;
		int samples = 0;
		for(int i = countA; i <= countB; i ++)
		{
			sum += forces[i];
			samples ++;
		}

		double elapsedSeconds = times[countB]/1000000.0 - times[countA]/1000000.0;
		return sum * Util.DivideSafe(elapsedSeconds, samples);
	}
	public int MarginLeft
	{
		get { return marginLeft; }
	}
	public int MarginRight
	{
		get { return marginRight; }
	}
	// TODO: end of... do all this in an inherited class


	// this is called while capturing, checks if last captured value is outside the graph
	public bool OutsideGraph()
	{
		Gdk.Point p = getLastPoint();
		//LogB.Information("p.Y: " + p.Y + "; heightG: " +  heightG);
		bool outsideGraph = false;

		if(p.X > widthG)
		{
			RealWidthG *= 2;
			outsideGraph = true;
		}
		if(p.Y < 0)
		{
			RealHeightG *= 2;
			outsideGraph = true;
		}
		else if(p.Y > heightG)
		{
			RealHeightGNeg *= 2;
			outsideGraph = true;
		}
		return outsideGraph;
	}
	// this is called at load signal, checks if last X is outside the graph and max/min force
	public bool OutsideGraph(int lastTime, double maxForce, double minForce)
	{
		if(lastTime + GetTimeInPx(marginLeft) + GetTimeInPx(marginRight) > RealWidthG ||
				GetForceInPx(minForce) > heightG ||
				GetForceInPx(maxForce) < 0)
		{
			RealWidthG = lastTime + GetTimeInPx(marginLeft) + GetTimeInPx(marginRight);

			RealHeightG = Convert.ToInt32(maxForce);
			RealHeightGNeg = Convert.ToInt32(Math.Abs(minForce));

			return true;
		}

		return false;
	}

	public void Redo()
	{
		for(int i=0; i < NumCaptured; i ++)
			Points[i] = new Gdk.Point(GetTimeInPx(times[i]), GetForceInPx(forces[i]));
	}

	public int WidthG
	{
		set { widthG = value; }
	}

	public int HeightG
	{
		set { heightG = value; }
	}
}

public class ForceSensorRFD
{
	//if these names change, change FunctionPrint() below
	public enum Functions { RAW, FITTED } //on SQL is inserted like this
	protected static string function_RAW_name = "RAW";
	protected static string function_FITTED_name = "Fitted";

	//if these names change, change TypePrint() below
	public enum Types { INSTANTANEOUS, AVERAGE, PERCENT_F_MAX, RFD_MAX, IMP_UNTIL_PERCENT_F_MAX, IMP_RANGE } //on SQL is inserted like this
	private static string type_INSTANTANEOUS_name = "Instantaneous";
	private static string type_AVERAGE_name = "Average";
	private static string type_PERCENT_F_MAX_name = "% Force max";
	private static string type_RFD_MAX_name = "RFD max";

	public string code; //RFD1...4 //I: on impulse
	public bool active;
	public Functions function;
	public Types type;
	public int num1;
	public int num2;

	//constructor for inheritance
	public ForceSensorRFD()
	{
	}

	public ForceSensorRFD(string code, bool active, Functions function, Types type, int num1, int num2)
	{
		this.code = code;
		this.active = active;
		this.function = function;
		this.type = type;
		this.num1 = num1;
		this.num2 = num2;
	}

	public bool Changed(ForceSensorRFD newRFD)
	{
		if(
				code == newRFD.code && active == newRFD.active &&
				function == newRFD.function && type == newRFD.type &&
				num1 == newRFD.num1 && num2 == newRFD.num2)
			return false;

		return true;
	}

	public static string [] FunctionsArray(bool translated)
	{
		if(translated)
			return new string [] { Catalog.GetString(function_RAW_name), Catalog.GetString(function_FITTED_name) };
		else
			return new string [] { function_RAW_name, function_FITTED_name };
	}
	public static string [] TypesArray(bool translated)
	{
		if(translated)
			return new string [] {
				Catalog.GetString(type_INSTANTANEOUS_name), Catalog.GetString(type_AVERAGE_name),
				Catalog.GetString(type_PERCENT_F_MAX_name), Catalog.GetString(type_RFD_MAX_name)
			};
		else
			return new string [] {
				type_INSTANTANEOUS_name, type_AVERAGE_name, type_PERCENT_F_MAX_name, type_RFD_MAX_name
			};
	}

	public string FunctionPrint(bool translated)
	{
		if(function == Functions.RAW) {
			if(translated)
				return Catalog.GetString(function_RAW_name);
			else
				return function_RAW_name;
		}

		if(translated)
			return Catalog.GetString(function_FITTED_name);
		else
			return function_FITTED_name;
	}

	public virtual string TypePrint(bool translated)
	{
		if(type == Types.INSTANTANEOUS) {
			if(translated)
				return Catalog.GetString(type_INSTANTANEOUS_name);
			else
				return type_INSTANTANEOUS_name;
		}
		else if(type == Types.AVERAGE) {
			if(translated)
				return Catalog.GetString(type_AVERAGE_name);
			else
				return type_AVERAGE_name;
		}
		else if(type == Types.PERCENT_F_MAX) {
			if(translated)
				return Catalog.GetString(type_PERCENT_F_MAX_name);
			else
				return type_PERCENT_F_MAX_name;
		}
		else { //if(type == Types.RFD_MAX)
			if(translated)
				return Catalog.GetString(type_RFD_MAX_name);
			else
				return type_RFD_MAX_name;
		}
	}

	public string ToSQLInsertString()
	{
		return
			"\"" + code  + "\"" + "," +
			Util.BoolToInt(active).ToString() + "," +
			"\"" + function.ToString() + "\"" + "," +
			"\"" + type.ToString() + "\"" + "," +
			num1.ToString() + "," +
			num2.ToString();
	}

	public string ToR()
	{
		return function.ToString() + ";" + type.ToString() + ";" + num1.ToString() + ";" + num2.ToString();
	}

	public static string Function_RAW_name
	{
		get { return function_RAW_name; }
	}
	public static string Function_FITTED_name
	{
		get { return function_FITTED_name; }
	}

	public static string Type_INSTANTANEOUS_name
	{
		get { return type_INSTANTANEOUS_name; }
	}
	public static string Type_AVERAGE_name
	{
		get { return type_AVERAGE_name; }
	}
	public static string Type_PERCENT_F_MAX_name
	{
		get { return type_PERCENT_F_MAX_name; }
	}
	public static string Type_RFD_MAX_name
	{
		get { return type_RFD_MAX_name; }
	}
}

public class ForceSensorImpulse : ForceSensorRFD
{
	//if these names change, change TypePrint() below
	private static string type_IMP_UNTIL_PERCENT_F_MAX_name = "Until % Force max";
	private static string type_IMP_RANGE_name = "Range";

	public ForceSensorImpulse()
	{
	}

	public ForceSensorImpulse(bool active, Functions function, Types type, int num1, int num2)
	{
		this.code = "I";
		this.active = active;
		this.function = function;
		this.type = type;
		this.num1 = num1;
		this.num2 = num2;
	}

	public bool Changed(ForceSensorImpulse newImpulse)
	{
		if(
				active == newImpulse.active &&
				function == newImpulse.function && type == newImpulse.type &&
				num1 == newImpulse.num1 && num2 == newImpulse.num2)
			return false;

		return true;
	}

	public static string [] TypesArrayImpulse(bool translated)
	{
		if(translated)
			return new string [] {
				Catalog.GetString(type_IMP_UNTIL_PERCENT_F_MAX_name), Catalog.GetString(type_IMP_RANGE_name),
			};
		else
			return new string [] {
				type_IMP_UNTIL_PERCENT_F_MAX_name, type_IMP_RANGE_name
			};
	}

	public override string TypePrint(bool translated)
	{
		if(type == Types.IMP_UNTIL_PERCENT_F_MAX) {
			if(translated)
				return Catalog.GetString(type_IMP_UNTIL_PERCENT_F_MAX_name);
			else
				return type_IMP_UNTIL_PERCENT_F_MAX_name;
		}
		else { // if(type == Types.IMP_RANGE)
			if(translated)
				return Catalog.GetString(type_IMP_RANGE_name);
			else
				return type_IMP_RANGE_name;
		}
	}

	public static string Type_IMP_UNTIL_PERCENT_F_MAX_name
	{
		get { return type_IMP_UNTIL_PERCENT_F_MAX_name; }
	}
	public static string Type_IMP_RANGE_name
	{
		get { return type_IMP_RANGE_name; }
	}
}

public class ForceSensorGraph
{
	List<ForceSensorRFD> rfdList;
	ForceSensorImpulse impulse;
	double averageLength;
	double percentChange;
	bool vlineT0;
	bool vline50fmax_raw;
	bool vline50fmax_fitted;
	bool hline50fmax_raw;
	bool hline50fmax_fitted;
	int testLength;
	string title;

	public ForceSensorGraph(List<ForceSensorRFD> rfdList, ForceSensorImpulse impulse, int testLength, string title)
	{
		this.rfdList = rfdList;
		this.impulse = impulse;
		this.testLength = testLength;
		this.title = title;

		averageLength = 0.1;
		percentChange = 5;
		vlineT0 = false;
		vline50fmax_raw = false;
		vline50fmax_fitted = false;
		hline50fmax_raw = false;
		hline50fmax_fitted = false;
	}

	public bool CallR(int graphWidth, int graphHeight)
	{
		LogB.Information("\nforceSensor CallR ----->");
		writeOptionsFile(graphWidth, graphHeight);
		return ExecuteProcess.CallR(UtilEncoder.GetmifScript());
	}

	private void writeOptionsFile(int graphWidth, int graphHeight)
	{
		string scriptsPath = UtilEncoder.GetSprintPath();
		if(UtilAll.IsWindows())
			scriptsPath = scriptsPath.Replace("\\","/");

		System.Globalization.NumberFormatInfo localeInfo = new System.Globalization.NumberFormatInfo();
		localeInfo = System.Globalization.NumberFormatInfo.CurrentInfo;

		string scriptOptions =
			"#os\n" + 			UtilEncoder.OperatingSystemForRGraphs() + "\n" +
			"#decimalChar\n" + 		localeInfo.NumberDecimalSeparator + "\n" +
			"#graphWidth\n" + 		graphWidth.ToString() + "\n" +
			"#graphHeight\n" + 		graphHeight.ToString() + "\n" +
			"#averageLength\n" + 		Util.ConvertToPoint(averageLength) + "\n" +
			"#percentChange\n" + 		Util.ConvertToPoint(percentChange) + "\n" +
			"#vlineT0\n" + 			Util.BoolToRBool(vlineT0) + "\n" +
			"#vline50fmax.raw\n" + 		Util.BoolToRBool(vline50fmax_raw) + "\n" +
			"#vline50fmax.fitted\n" + 	Util.BoolToRBool(vline50fmax_fitted) + "\n" +
			"#hline50fmax.raw\n" + 		Util.BoolToRBool(hline50fmax_raw) + "\n" +
			"#hline50fmax.fitted\n" + 	Util.BoolToRBool(hline50fmax_fitted) + "\n" +
			"#RFDs";

		foreach(ForceSensorRFD rfd in rfdList)
			if(rfd.active)
				scriptOptions += "\n" + rfd.ToR();
			else
				scriptOptions += "\n-1";

		if(impulse.active)
			scriptOptions += "\n" + impulse.ToR();
		else
			scriptOptions += "\n-1";

		scriptOptions +=
			"\n#testLength\n" + 		testLength.ToString() + "\n" +
			"#title\n" + 			title + "\n" +
			"#scriptsPath\n" + 		UtilEncoder.GetScriptsPath() + "\n";

		TextWriter writer = File.CreateText(Path.GetTempPath() + "Roptions.txt");
		writer.Write(scriptOptions);
		writer.Flush();
		writer.Close();
		((IDisposable)writer).Dispose();
	}

	public static string GetDataDir(int sessionID)
	{
		System.IO.DirectoryInfo folderSession =
			new System.IO.DirectoryInfo(Util.GetForceSensorSessionDir(sessionID));
		System.IO.DirectoryInfo folderGeneric =
			new System.IO.DirectoryInfo(Util.GetForceSensorDir());

		if(folderSession.Exists)
			return Util.GetForceSensorSessionDir(sessionID);
		else if(folderGeneric.Exists)
			return Util.GetForceSensorDir();
		else
			return "";
	}
}

public class ForceSensorAnalyzeInstant
{
	public double ForceAVG;
	public double ForceMAX;

	private ForceSensorCapturePoints fscAIPoints; //Analyze Instant
	private ForceSensorValues forceSensorValues;
	private int graphWidth;
	private int graphHeight;

	public ForceSensorAnalyzeInstant(string file, int graphWidth, int graphHeight)
	{
		this.graphWidth = graphWidth;
		this.graphHeight = graphHeight;

		readFile(file);

		//ensure points fit on display
		if(fscAIPoints.OutsideGraph(forceSensorValues.TimeLast, forceSensorValues.ForceMax, forceSensorValues.ForceMin))
			fscAIPoints.Redo();
	}

	private void readFile(string file)
	{
		fscAIPoints = new ForceSensorCapturePoints(graphWidth, graphHeight);

//TODO: check file exists...
		List<string> contents = Util.ReadFileAsStringList(file);
		bool headersRow = true;

		//initialize
		forceSensorValues = new ForceSensorValues();

		foreach(string str in contents)
		{
			if(headersRow)
				headersRow = false;
			else {
				string [] strFull = str.Split(new char[] {';'});
				if(strFull.Length != 2)
					continue;

				/*
				 * TODO: Make this work with decimals as comma and decimals as point
				 * to fix problems on importing data on different localised computer
				 */

				if(Util.IsNumber(strFull[0], false) && Util.IsNumber(strFull[1], true))
				{
					int time = Convert.ToInt32(strFull[0]);
					double force = Convert.ToDouble(strFull[1]);

					fscAIPoints.Add(time, force);
					fscAIPoints.NumCaptured ++;

					forceSensorValues.TimeLast = time;
					forceSensorValues.ForceLast = force;
					forceSensorValues.SetMaxMinIfNeeded(force, time);
				}
			}
		}
	}

	//When B checkbutton is clicked or window is resized
	public void RedoGraph(int graphWidth, int graphHeight)
	{
		this.graphWidth = graphWidth;
		this.graphHeight = graphHeight;
		fscAIPoints.WidthG = graphWidth;
		fscAIPoints.HeightG = graphHeight;

		fscAIPoints.Redo();
	}

	//gets an instant value
	public double GetTimeMS(int count)
	{
		return fscAIPoints.GetTimeAtCount(count) / 1000.0; //microseconds to milliseconds
	}
	public double GetForceAtCount(int count)
	{
		return fscAIPoints.GetForceAtCount(count);
	}

	public int GetLength()
	{
		LogB.Information("GetLength: " + fscAIPoints.GetLength());
		return fscAIPoints.GetLength();
	}

	public int GetXFromSampleCount(int currentPos, int totalPos)
	{
		LogB.Information(string.Format("currentPos: {0}, totalPos: {1}", currentPos, totalPos));
		//this can be called on expose event before calculating needed parameters
		if(graphWidth == 0)
			return 0;

		int leftMargin = fscAIPoints.MarginLeft;
		int rightMargin = fscAIPoints.MarginRight;

		/*
		 * note samples don't come at same time separation, so this does not work:
		double px = Util.DivideSafe(
				(graphWidth - leftMargin - rightMargin) * currentPos,
				totalPos -1); //-1 ok
				//fscAIPoints.RealWidthG);
		*/
		//get the time of sample
		double currentTime = fscAIPoints.GetTimeAtCount(currentPos);
		double lastTime = fscAIPoints.GetLastTime();

		double px = Util.DivideSafe(
				(graphWidth - leftMargin - rightMargin) * currentTime,
				lastTime);

		// fix margin
		//px = px + plt.x1 * graphWidth;
		px = px + leftMargin;

		return Convert.ToInt32(px);
	}

	public int GetPxAtForce(double f)
	{
		return fscAIPoints.GetForceInPx(f);
	}

	//calculates from a range
	public bool CalculateRangeParams(int countA, int countB)
	{
		//countA will be the lowest and countB the highest to calcule Avg and max correctly no matter if B is before A
		if(countA > countB) {
			int temp = countA;
			countA = countB;
			countB = temp;
		}

		fscAIPoints.GetAverageAndMaxForce(countA, countB, out ForceAVG, out ForceMAX);

		return true;
	}

	public double CalculateRFD(int countA, int countB)
	{
		return fscAIPoints.GetRFD(countA, countB);
	}

	public double CalculateImpulse(int countA, int countB)
	{
		return fscAIPoints.GetImpulse(countA, countB);
	}

	/*
	 * Calculates RFD in a point using previous and next point
	 */
//TODO: fer que es vagi recordant el max en un rang determinat pq no s'hagi de tornar a calcular
	public double LastRFDMax;
	public int LastRFDMaxCount;
	public void CalculateMaxRFDInRange(int countA, int countB)
	{
		double max = 0;
		double current = 0;
		int countRFDMax = countA; //count where maxRFD is found

		for(int i = countA; i < countB; i ++)
		{
			current = fscAIPoints.GetRFD(i-1, i+1);
			if(current > max)
			{
				max = current;
				countRFDMax = i;
			}
		}

		//stored to read them from forceSensorAnalyze manual table and graph
		LastRFDMax = max;
		LastRFDMaxCount = countRFDMax;
	}

	/* this method is not working
	public int CalculateXOfTangentLine(int x0, int y0, double RFD, int y, int height)
	{
	*/
		/*
		 * x0 and y0 are coordinates of RFD point
		 * RFD is the RFD value
		 * x is the returned value for an x value
		 * height is used to transform the y's in order to make following formula work
		 *
		 * y = RFD * x + y0 - x0*RFD
		 * y - y0 + x0*RFD = x*RFD
		 * x = (y - y0 + x0*RFD) / RFD
		 */
	/*
		y0 = height - y0;
		y = height -y;

		return Convert.ToInt32(Util.DivideSafe(y - y0 + x0*RFD, RFD));
	}
	*/

	public void CalculateRFDTangentLine(int countRFDMax, out int lineXStart, out int lineXEnd, out int lineYStart, out int lineYEnd)
	{
		LogB.Information(string.Format("CalculateRFDTangentLine: {0}" , countRFDMax));

		// 1) calculate X and Y of points before and after RFD
		int pointXBefore = GetXFromSampleCount(countRFDMax -1, GetLength());
		int pointXAfter = GetXFromSampleCount(countRFDMax +1, GetLength());
		int pointYBefore = GetPxAtForce(GetForceAtCount(countRFDMax -1));
		int pointYAfter = GetPxAtForce(GetForceAtCount(countRFDMax +1));

		// 2) calculate the slope of the line that could pass across this points
		double slope = Math.Abs( Util.DivideSafe( pointYAfter - pointYBefore,
					(1.0 * (pointXAfter- pointXBefore)) ) );

		// 3) get the RFD point
		int pointXRFD = GetXFromSampleCount(countRFDMax, GetLength());
		int pointYRFD = GetPxAtForce(GetForceAtCount(countRFDMax));

		// 4) calculate line that cross RFD point with calculated slope
		lineXStart = pointXRFD - Convert.ToInt32(Util.DivideSafe(
					(graphHeight - pointYRFD),
					slope));
		lineXEnd = pointXRFD + Convert.ToInt32(Util.DivideSafe(
					(pointYRFD - 0),
					slope));
		lineYStart = graphHeight;
		lineYEnd = 0;
	}

	//TODO: better if all this time, force, rfd params are on this class, so don't need to read labels from main gui
	public void ExportToCSV(int countA, int countB, string selectedFileName, string sepString,
			double timeA, double timeB, double timeDiff,
			double forceA, double forceB, double forceDiff, double forceAvg, double forceMax,
			double rfdA, double rfdB, double rfdDiff, double rfdAvg, double rfdMax
			)
	{
		//this overwrites if needed
		TextWriter writer = File.CreateText(selectedFileName);

		string sep = " ";
		if (sepString == "COMMA")
			sep = ";";
		else
			sep = ",";

		string header =
			"" + sep +
			Catalog.GetString("Time") + sep +
			Catalog.GetString("Force") + sep +
			Catalog.GetString("RFD");

		//write header
		writer.WriteLine(header);

		//write statistics
		writer.WriteLine(
				Catalog.GetString("Difference") + sep +
				Util.DoubleToCSV(timeDiff, sepString) + sep +
				Util.DoubleToCSV(forceDiff, sepString) + sep +
				Util.DoubleToCSV(rfdDiff, sepString) );

		writer.WriteLine(
				Catalog.GetString("Average") + sep +
				"" + sep +
				Util.DoubleToCSV(forceAvg, sepString) + sep +
				Util.DoubleToCSV(rfdAvg, sepString) );

		writer.WriteLine(
				Catalog.GetString("Maximum") + sep +
				"" + sep +
				Util.DoubleToCSV(forceMax, sepString) + sep +
				Util.DoubleToCSV(rfdMax, sepString) );

		//blank line
		writer.WriteLine();

		//write header
		writer.WriteLine(header);

		//write data
		for(int i = countA; i <= countB; i ++)
			writer.WriteLine(
					(i+1).ToString() + sep +
					Util.DoubleToCSV(fscAIPoints.GetTimeAtCount(i), sepString) + sep +
					Util.DoubleToCSV(fscAIPoints.GetForceAtCount(i), sepString) + sep +
					Util.DoubleToCSV(CalculateRFD(i-1, i+1), sepString) );

		writer.Flush();
		writer.Close();
		((IDisposable)writer).Dispose();
	}

	public ForceSensorCapturePoints FscAIPoints
	{
		get { return fscAIPoints; }
	}

}
