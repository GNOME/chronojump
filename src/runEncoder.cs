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
 *  Copyright (C) 2018   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
using System.IO; 		//for detect OS
using System.Collections.Generic; //List<T>

public class RunEncoderGraph
{
	private int testLength;
	private double mass;
	private double personHeight;
	private double tempC;

	public RunEncoderGraph(int testLength, double mass, double personHeight, double tempC)
	{
		this.testLength = testLength;
		this.mass = mass;
		this.personHeight = personHeight;
		this.tempC = tempC;
	}

	//moure la majoria d'aixo a ExecuteProcess per reciclar-ho a gui/runEncoder.cs linia 335
	public bool CallR(int graphWidth, int graphHeight)
	{
		string executable = UtilEncoder.RProcessBinURL();
		List<string> parameters = new List<string>();

		//A) script
		string script = UtilEncoder.GetRunEncoderScript();
		if(UtilAll.IsWindows())
			script = script.Replace("\\","/");

		parameters.Insert(0, "\"" + script + "\"");

		//B) tempPath
		string tempPath = Path.GetTempPath();
		if(UtilAll.IsWindows())
			tempPath = tempPath.Replace("\\","/");

		parameters.Insert(1, "\"" + tempPath + "\"");

		//C) writeOptions
		writeOptionsFile(graphWidth, graphHeight);

		LogB.Information("\nCalling R file ----->");

		//D) call process
		//ExecuteProcess.run (executable, parameters);
		ExecuteProcess.Result execute_result = ExecuteProcess.run (executable, parameters);
		//LogB.Information("Result = " + execute_result.stdout);

		LogB.Information("\n<------ Done calling R file.");
		return execute_result.success;
	}

	private void writeOptionsFile(int graphWidth, int graphHeight)
	{
		string scriptsPath = UtilEncoder.GetSprintPath();
		if(UtilAll.IsWindows())
			scriptsPath = scriptsPath.Replace("\\","/");

		System.Globalization.NumberFormatInfo localeInfo = new System.Globalization.NumberFormatInfo();
		localeInfo = System.Globalization.NumberFormatInfo.CurrentInfo;

		string scriptOptions =
			"#scriptsPath\n" + 		UtilEncoder.GetScriptsPath() + "\n" +
			"#filename\n" + 		UtilEncoder.GetRunEncoderCSVFileName() + "\n" +
			"#mass\n" + 			Util.ConvertToPoint(mass) + "\n" +
			"#personHeight\n" + 		Util.ConvertToPoint(personHeight / 100.0) + "\n" + //send it in meters
			"#tempC\n" + 			tempC + "\n" +
			"#testLength\n" + 		testLength.ToString() + "\n" +
			"#os\n" + 			UtilEncoder.OperatingSystemForRGraphs() + "\n" +
			"#graphWidth\n" + 		graphWidth.ToString() + "\n" +
			"#graphHeight\n" + 		graphHeight.ToString();


		TextWriter writer = File.CreateText(Path.GetTempPath() + "Roptions.txt");
		writer.Write(scriptOptions);
		writer.Flush();
		writer.Close();
		((IDisposable)writer).Dispose();
	}
}
