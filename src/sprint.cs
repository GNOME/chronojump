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

public class Sprint
{
	//private List<double> positions;
	//private List<double> splitTimes;
	private string positions;
	private string splitTimes;
	private double mass;
	private double personHeight;
	private double tempC;

	public Sprint(string positions, string splitTimes,
			double mass, double personHeight, double tempC)
	{
		this.positions = positions;
		this.splitTimes = splitTimes;
		this.mass = mass;
		this.personHeight = personHeight;
		this.tempC = tempC;
	}

	/*
	public Sprint(List<double> positions, List<double> splitTimes,
			double mass, double personHeight, double tempC)
	{
		this.positions = positions;
		this.splitTimes = splitTimes;
		this.mass = mass;
		this.personHeight = personHeight;
		this.tempC = tempC;
	}
	*/

	public bool CallR(int graphWidth, int graphHeight)
	{
		string executable = UtilEncoder.RProcessBinURL();
		List<string> parameters = new List<string>();

		//A) photocellsScript
		string photocellsScript = UtilEncoder.GetSprintPhotocellsScript();
		if(UtilAll.IsWindows())
			photocellsScript = photocellsScript.Replace("\\","/");

		parameters.Insert(0, "\"" + photocellsScript + "\"");

		//B) tempPath
		string tempPath = Path.GetTempPath();
		if(UtilAll.IsWindows())
			tempPath = tempPath.Replace("\\","/");

		parameters.Insert(1, "\"" + tempPath + "\"");

		//C) writeOptions
		writeOptionsFile(graphWidth, graphHeight);

		LogB.Information("\nCalling sprint.R ----->");

		//D) call process
		//ExecuteProcess.run (executable, parameters);
		ExecuteProcess.Result execute_result = ExecuteProcess.run (executable, parameters);
		//LogB.Information("Result = " + execute_result.stdout);

		LogB.Information("\n<------ Done calling sprint.R.");
		return execute_result.success;
	}

	private void writeOptionsFile(int graphWidth, int graphHeight)
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
			"#positions\n" + 	positions + "\n" +
			"#splitTimes\n" + 	splitTimes + "\n" +
			"#mass\n" + 		Util.ConvertToPoint(mass) + "\n" +
			"#personHeight\n" + 	Util.ConvertToPoint(personHeight / 100.0) + "\n" + //send it in meters
			"#tempC\n" + 		tempC + "\n" +
			"#os\n" + 		UtilEncoder.OperatingSystemForRGraphs() + "\n" +
			"#graphWidth\n" + 	graphWidth.ToString() + "\n" +
			"#graphHeight\n" + 	graphHeight.ToString() + "\n";

		TextWriter writer = File.CreateText(Path.GetTempPath() + "Roptions.txt");
		writer.Write(scriptOptions);
		writer.Flush();
		writer.Close();
		((IDisposable)writer).Dispose();
	}
}
