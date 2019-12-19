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

public class EncoderParams
{
	//graph.R need both to know displacedMass depending on encoderConfiguration
	//and plot both as entry data in the table of result data
	private string massBody; //to pass always as "." to R.
	private string massExtra; //to pass always as "." to R
	
	private int minHeight;
	private int exercisePercentBodyWeight; //was private bool isJump; (if it's 0 is like "jump")
	private string eccon;
	private string analysis;
	private string analysisVariables;
	private string analysisOptions;		//p: propulsive
	private bool captureCheckFullyExtended;
	private int captureCheckFullyExtendedValue;
					
	//encoderConfiguration conversions
	//in signals and curves, need to do conversions (invert, inertiaMomentum, diameter)
	//private EncoderConfiguration encoderConfiguration;	
	private string encoderConfiguration;	
	
	private string smoothCon; //to pass always as "." to R
	private int curve;
	private int width;
	private int height;
	private string decimalSeparator;	//used in export data from R to csv
	//private bool inverted; //used only in runEncoderCapturePython. In graph.R will be used encoderConfigurationName

	public EncoderParams()
	{
	}

	
	//to graph.R	
	public EncoderParams(int minHeight, int exercisePercentBodyWeight, string massBody, string massExtra, 
			string eccon, string analysis, string analysisVariables, string analysisOptions, 
			bool captureCheckFullyExtended, int captureCheckFullyExtendedValue,
			string encoderConfiguration, //EncoderConfiguration encoderConfiguration,
			string smoothCon, int curve, int width, int height, string decimalSeparator)
	{
		this.minHeight = minHeight;
		this.exercisePercentBodyWeight = exercisePercentBodyWeight;
		this.massBody = massBody;
		this.massExtra = massExtra;
		this.eccon = eccon;
		this.analysis = analysis;
		this.analysisVariables = analysisVariables;
		this.analysisOptions = analysisOptions;
		this.captureCheckFullyExtended = captureCheckFullyExtended;
		this.captureCheckFullyExtendedValue = captureCheckFullyExtendedValue;
		this.encoderConfiguration = encoderConfiguration;
		this.smoothCon = smoothCon;
		this.curve = curve;
		this.width = width;
		this.height = height;
		this.decimalSeparator = decimalSeparator;
	}
	
	public string ToStringROptions () 
	{
		string capFullyExtendedStr = "-1";
		if(captureCheckFullyExtended)
			capFullyExtendedStr = captureCheckFullyExtendedValue.ToString(); 
		
		return 
			"#minHeight\n" + 	minHeight + "\n" + 
			"#exercisePercentBodyWeight\n" + exercisePercentBodyWeight + "\n" + 
			"#massBody\n" + 	massBody + "\n" + 
			"#massExtra\n" + 	massExtra + "\n" + 
			"#eccon\n" + 		eccon + "\n" + 
			"#analysis\n" + 	analysis + "\n" + 
			"#analysisVariables\n" + analysisVariables + "\n" + 
			"#analysisOptions\n" + analysisOptions + "\n" + 
			"#captureCheckFullyExtended\n" + capFullyExtendedStr + "\n" + 
			//encoderConfiguration.ToStringOutput(EncoderConfiguration.Outputs.ROPTIONS) + "\n" +
			"#name\nLINEAR\n" +
			"#str_d\n-1\n" +
			"#str_D\n-1\n" +
			"#anglePush\n-1\n" +
			"#angleWeight\n-1\n" +
			"#inertiaTotal\n-1\n" +
			"#gearedDown\n1\n" +
			"#smoothCon\n" + 	smoothCon + "\n" + 
			"#curve\n" + 		curve + "\n" + 
			"#width\n" + 		width + "\n" + 
			"#height\n" + 		height + "\n" + 
			"#decimalSeparator\n" + decimalSeparator
			;
	}
	
	public string Analysis {
		get { return analysis; }
	}


	~EncoderParams() {}
}

