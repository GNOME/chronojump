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
 * Copyright (C) 2019   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;

public class EncoderGraphROptions
{
	public string inputData;
	public string outputGraph;
	public string outputData1;
	public string encoderRPath;
	public string encoderTempPath;
	public EncoderParams ep;
	public string title;
	public string operatingSystem;
	public string englishWords;
	public string translatedWords;
	public bool debug;
	public bool crossValidate;
	private bool cutByTriggers;
	private string triggerList;
	public bool separateSessionInDays;
	public AnalysisModes analysisMode; //the four analysisModes

	public enum AnalysisModes { CAPTURE, INDIVIDUAL_CURRENT_SET, INDIVIDUAL_CURRENT_SESSION, INDIVIDUAL_ALL_SESSIONS, GROUPAL_CURRENT_SESSION }

	public EncoderGraphROptions(
			string inputData, string outputGraph, string outputData1, 
			string encoderRPath, string encoderTempPath,
			EncoderParams ep,
			string title, string operatingSystem,
			string englishWords, string translatedWords,
			bool debug, bool crossValidate, bool cutByTriggers, string triggerList,
			bool separateSessionInDays, AnalysisModes analysisMode)
	{
		this.inputData = inputData;
		this.outputGraph = outputGraph;
		this.outputData1 = outputData1;
		this.encoderRPath = encoderRPath;
		this.encoderTempPath = encoderTempPath;
		this.ep = ep;
		this.title = title;
		this.operatingSystem = operatingSystem;
		this.englishWords = englishWords;
		this.translatedWords = translatedWords;
		this.debug = debug;
		this.crossValidate = crossValidate;
		this.cutByTriggers = cutByTriggers;
		this.triggerList = triggerList;
		this.separateSessionInDays = separateSessionInDays;
		this.analysisMode = analysisMode;

		//ensure triggerList is not null or blank
		if(triggerList == null || triggerList == "")
			triggerList = "-1";
	}

	public override string ToString() {
		return 
			"#inputdata\n" + 	inputData + "\n" + 
			"#outputgraph\n" + 	outputGraph + "\n" + 
			"#outputdata1\n" + 	outputData1 + "\n" + 
			"#encoderRPath\n" + 	encoderRPath + "\n" + 
			"#encoderTempPath\n" + 	encoderTempPath + "\n" + 
			ep.ToStringROptions() + "\n" + 
			"#title\n" + 		title + "\n" + 
			"#operatingsystem\n" + 	operatingSystem + "\n" +
			"#englishWords\n" + 	englishWords + "\n" +
			"#translatedWords\n" + 	translatedWords + "\n" +
			"#debug\n" +		Util.BoolToRBool(debug) + "\n" +
			"#crossValidate\n" +	Util.BoolToRBool(crossValidate) + "\n" +
			"#cutByTriggers\n" +	Util.BoolToRBool(cutByTriggers) + "\n" +
			"#triggerList\n" +	triggerList + "\n" +
			"#separateSessionInDays\n" +	Util.BoolToRBool(separateSessionInDays) + "\n" +
			"#analysisMode\n" + 	analysisMode.ToString() + "\n";
	}
	

	~EncoderGraphROptions() {}
}

