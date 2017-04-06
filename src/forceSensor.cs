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

public class ForceSensor
{
	double averageLength;
	double percentChange;
	bool vlineT0;
	bool vline50fmax_raw;
	bool vline50fmax_fitted;
	bool hline50fmax_raw;
	bool hline50fmax_fitted;
	bool rfd0_fitted;
	bool rfd100_raw;
	bool rfd0_100_raw;
	bool rfd0_100_fitted;
	bool rfd200_raw;
	bool rfd0_200_raw;
	bool rfd0_200_fitted;
	bool rfd50fmax_raw;
	bool rfd50fmax_fitted;

	public ForceSensor()
	{
		averageLength = 0.1; 
		percentChange = 5;
		vlineT0 = false;
		vline50fmax_raw = false;
		vline50fmax_fitted = false;
		hline50fmax_raw = false;
		hline50fmax_fitted = false;
		rfd0_fitted = false;
		rfd100_raw = false;
		rfd0_100_raw = false;
		rfd0_100_fitted = false;
		rfd200_raw = false;
		rfd0_200_raw = false;
		rfd0_200_fitted = false;
		rfd50fmax_raw = false;
		rfd50fmax_fitted = false;
	}

	public bool CallR(int graphWidth, int graphHeight)
	{
		string executable = UtilEncoder.RProcessBinURL();
		List<string> parameters = new List<string>();

		//A) mifcript
		string mifScript = UtilEncoder.GetmifScript();
		if(UtilAll.IsWindows())
			mifScript = mifScript.Replace("\\","/");

		parameters.Insert(0, "\"" + mifScript + "\"");

		//B) tempPath
		string tempPath = Path.GetTempPath();
		if(UtilAll.IsWindows())
			tempPath = tempPath.Replace("\\","/");

		parameters.Insert(1, "\"" + tempPath + "\"");

		//C) writeOptions
		writeOptionsFile(graphWidth, graphHeight);

		LogB.Information("\nCalling mif R file ----->");

		//D) call process
		//ExecuteProcess.run (executable, parameters);
		ExecuteProcess.Result execute_result = ExecuteProcess.run (executable, parameters);
		//LogB.Information("Result = " + execute_result.stdout);

		LogB.Information("\n<------ Done calling mif R file.");
		return execute_result.success;
	}

	private void writeOptionsFile(int graphWidth, int graphHeight)
	{
		string scriptsPath = UtilEncoder.GetSprintPath();
		if(UtilAll.IsWindows())
			scriptsPath = scriptsPath.Replace("\\","/");

		string scriptOptions =
			"#os\n" + 			UtilEncoder.OperatingSystemForRGraphs() + "\n" +
			"#graphWidth\n" + 		graphWidth.ToString() + "\n" +
			"#graphHeight\n" + 		graphHeight.ToString() + "\n" +
			"#averageLength\n" + 		Util.ConvertToPoint(averageLength) + "\n" +
			"#percentChange\n" + 		Util.ConvertToPoint(percentChange) + "\n" +
			"#vlineT0\n" + 			Util.BoolToRBool(vlineT0) + "\n" +
			"#vline50fmax.raw\n" + 		Util.BoolToRBool(vline50fmax_raw) + "\n" +
			"#vline50fmax.fitted\n" + 	Util.BoolToRBool(vline50fmax_fitted) + "\n" +
			"#hline50fmax.raw\n" + 		Util.BoolToRBool(hline50fmax_raw) + "\n" +
			"#hline50fmax.fitted\n" + 	Util.BoolToRBool(hline50fmax_fitted) + "\n" +
			"#rfd0.fitted\n" + 		Util.BoolToRBool(rfd0_fitted) + "\n" +
			"#rfd100.raw\n" + 		Util.BoolToRBool(rfd100_raw) + "\n" +
			"#rfd0_100.raw\n" + 		Util.BoolToRBool(rfd0_100_raw) + "\n" +
			"#rfd0_100.fitted\n" + 		Util.BoolToRBool(rfd0_100_fitted) + "\n" +
			"#rfd200.raw\n" + 		Util.BoolToRBool(rfd200_raw) + "\n" +
			"#rfd0_200.raw\n" + 		Util.BoolToRBool(rfd0_200_raw) + "\n" +
			"#rfd0_200.fitted\n" + 		Util.BoolToRBool(rfd0_200_fitted) + "\n" +
			"#rfd50fmax.raw\n" + 		Util.BoolToRBool(rfd50fmax_raw) + "\n" +
			"#rfd50fmax.fitted\n" + 	Util.BoolToRBool(rfd50fmax_fitted);

		TextWriter writer = File.CreateText(Path.GetTempPath() + "Roptions.txt");
		writer.Write(scriptOptions);
		writer.Flush();
		writer.Close();
		((IDisposable)writer).Dispose();
	}
}
