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
 *  Copyright (C) 2004-2024   Xavier de Blas <xaviblas@gmail.com>
 */

using System;
using Mono.Unix;

public class EncoderAnalyzeInstant 
{
	public List<double> displ;
	public List<double> speed;
	public List<double> accel;
	public List<double> force;
	public List<double> power;

	public int graphWidth;
	
	private Rx1y2 usr;
	private Rx1y2 plt;
		
	private double pxPlotArea;
	private double msPlotArea;
	
	//last calculated values on last range of msa and msb
	public double displAverageLast;
	public double displMaxLast;
	public double speedAverageLast;
	public double speedMaxLast;
	public double accelAverageLast;
	public double accelMaxLast;
	public double forceAverageLast;
	public double forceMaxLast;
	public double powerAverageLast;
	public double powerMaxLast;

	public EncoderAnalyzeInstant() {
		displ = new List<double>(); 
		speed = new List<double>(); 
		accel = new List<double>(); 
		force = new List<double>(); 
		power = new List<double>();
		
		graphWidth = 0;
		pxPlotArea = 0;
		msPlotArea = 0;
	}

	//file has a first line with headers
	//2nd.... full data
	public void ReadArrayFile(string filename)
	{
		List<string> lines = Util.ReadFileAsStringList(filename, "");
		if(lines == null)
			return;
		if(lines.Count <= 1) //return if there's only the header
			return;

		bool headerLine = true;
		foreach(string l in lines) {
				if(headerLine) {
					headerLine = false;
					continue;
				}

			string [] lsplit = l.Split(new char[] {','});
			displ.Add(Convert.ToDouble(Util.ChangeDecimalSeparator(lsplit[1])));
			speed.Add(Convert.ToDouble(Util.ChangeDecimalSeparator(lsplit[2])));
			accel.Add(Convert.ToDouble(Util.ChangeDecimalSeparator(lsplit[3])));
			force.Add(Convert.ToDouble(Util.ChangeDecimalSeparator(lsplit[4])));
			power.Add(Convert.ToDouble(Util.ChangeDecimalSeparator(lsplit[5])));
		}
	}
	
	public void ReadGraphParams(string filename)
	{
		List<string> lines = Util.ReadFileAsStringList(filename, "");
		if(lines == null)
			return;
		if(lines.Count < 3)
			return;

		graphWidth = Convert.ToInt32(lines[0]);
		usr = new Rx1y2(lines[1]);
		plt = new Rx1y2(lines[2]);

		// calculate the pixels in plot area
		pxPlotArea = graphWidth * (plt.x2 - plt.x1);

		//calculate the ms in plot area
		msPlotArea = usr.x2 - usr.x1;
	}
	
	//gets an instant value
	public double GetParam(string param, int ms) 
	{
		ms --; //converts from starting at 1 (graph) to starting at 0 (data)

		if(ms > displ.Count)
			return -1;

		else {
			if(param == "displ")
				return displ[ms];
			else if(param == "speed")
				return speed[ms];
			else if(param == "accel")
				return accel[ms];
			else if(param == "force")
				return force[ms];
			else if(param == "power")
				return power[ms];
			else
				return -2;
		}
	}
	
	//calculates from a range
	public bool CalculateRangeParams(int msa, int msb)
	{
		msa --; //converts from starting at 1 (graph) to starting at 0 (data)
		msb --; //converts from starting at 1 (graph) to starting at 0 (data)
		
		//if msb < msa invert them
		if(msb < msa) {
			int temp = msa;
			msa = msb;
			msb = temp;
		}

		if(msa > displ.Count || msb > displ.Count)
			return false;

		getAverageAndMax(displ, msa, msb, out displAverageLast, out displMaxLast);
		getAverageAndMax(speed, msa, msb, out speedAverageLast, out speedMaxLast);
		getAverageAndMax(accel, msa, msb, out accelAverageLast, out accelMaxLast);
		getAverageAndMax(force, msa, msb, out forceAverageLast, out forceMaxLast);
		getAverageAndMax(power, msa, msb, out powerAverageLast, out powerMaxLast);
		
		return true;
	}
	private void getAverageAndMax(List<double> dlist, int ini, int end, out double listAVG, out double listMAX) {
		if(ini == end) {
			listAVG = dlist[ini];
			listMAX = dlist[ini];
			return;
		}

		double sum = 0;
		double max = - 1000000;
		for(int i = ini; i <= end; i ++) {
			sum += dlist[i];
			if(dlist[i] > max)
				max = dlist[i];
		}

		listAVG = sum / (end - ini + 1); //+1 because count starts at 0
		listMAX = max;
	}


	public int GetVerticalLinePosition(int ms) 
	{
		//this can be called on expose event before calculating needed parameters
		if(graphWidth == 0 || pxPlotArea == 0 || msPlotArea == 0)
			return 0;

		// rule of three
		double px = (ms - usr.x1) * pxPlotArea / msPlotArea;

		// fix margin
		px = px + plt.x1 * graphWidth;

		return Convert.ToInt32(px);
	}
	
	public void ExportToCSV(int msa, int msb, string selectedFileName, string sepString) 
	{
		//if msb < msa invert them
		if(msb < msa) {
			int temp = msa;
			msa = msb;
			msb = temp;
		}

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
			Catalog.GetString("Displacement") + sep +
			Catalog.GetString("Speed") + sep +
			Catalog.GetString("Acceleration") + sep +
			Catalog.GetString("Force") + sep +
			Catalog.GetString("Power");
			
		//write header
		writer.WriteLine(header);

		//write statistics
		writer.WriteLine(
				Catalog.GetString("Difference") + sep +
				(msb-msa).ToString() + sep +
				Util.DoubleToCSV( (GetParam("displ",msb) - GetParam("displ",msa)), sepString ) + sep +
				Util.DoubleToCSV( (GetParam("speed",msb) - GetParam("speed",msa)), sepString ) + sep +
				Util.DoubleToCSV( (GetParam("accel",msb) - GetParam("accel",msa)), sepString ) + sep +
				Util.DoubleToCSV( (GetParam("force",msb) - GetParam("force",msa)), sepString ) + sep +
				Util.DoubleToCSV( (GetParam("power",msb) - GetParam("power",msa)), sepString ) );
		
		//done here because GetParam does the same again, and if we put it in the top of this method, it will be done two times
		msa --; //converts from starting at 1 (graph) to starting at 0 (data)
		msb --; //converts from starting at 1 (graph) to starting at 0 (data)
		
		writer.WriteLine(
				Catalog.GetString("Average") + sep +
				"" + sep +
				Util.DoubleToCSV(displAverageLast, sepString) + sep +
				Util.DoubleToCSV(speedAverageLast, sepString) + sep +
				Util.DoubleToCSV(accelAverageLast, sepString) + sep +
				Util.DoubleToCSV(forceAverageLast, sepString) + sep +
				Util.DoubleToCSV(powerAverageLast, sepString) );
		
		writer.WriteLine(
				Catalog.GetString("Maximum") + sep +
				"" + sep +
				Util.DoubleToCSV(displMaxLast, sepString) + sep +
				Util.DoubleToCSV(speedMaxLast, sepString) + sep +
				Util.DoubleToCSV(accelMaxLast, sepString) + sep +
				Util.DoubleToCSV(forceMaxLast, sepString) + sep +
				Util.DoubleToCSV(powerMaxLast, sepString) );

		//blank line
		writer.WriteLine();

		//write header
		writer.WriteLine(header);

		//write data
		for(int i = msa; i <= msb; i ++)
			writer.WriteLine(
					"" + sep +
					(i+1).ToString() + sep +
					Util.DoubleToCSV(displ[i], sepString) + sep +
					Util.DoubleToCSV(speed[i], sepString) + sep +
					Util.DoubleToCSV(accel[i], sepString) + sep +
					Util.DoubleToCSV(force[i], sepString) + sep +
					Util.DoubleToCSV(power[i], sepString) );

		writer.Flush();
		writer.Close();
		((IDisposable)writer).Dispose();
	}

	public void PrintDebug() {
		LogB.Information("Printing speed");
		foreach(double s in speed)
			LogB.Debug(s.ToString());
	}
}
