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
	private bool isJump;
	private string eccon;
	private string analysis;
	private string smooth; //to pass always as "." to R
	private int curve;
	private int width;
	private int height;
	
	public EncoderParams(int time, string mass, string smooth, string eccon)
	{
		this.time = time;
		this.mass = mass;
		this.smooth = smooth;
		this.eccon = eccon;
	}
	
	public string ToString1 () 
	{
		return time.ToString() + " " + mass.ToString() + " " + smooth + " " + eccon;
	}
	
	public EncoderParams(int minHeight, bool isJump, string mass, string eccon, 
			string analysis, string smooth, int curve, int width, int height)
	{
		this.minHeight = minHeight;
		this.isJump = isJump;
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
		return minHeight + " " + isJump + " " + mass + " " + eccon + " " + analysis + " " + 
			smooth + " " + curve + " " + width + " " + height;
	}
	
	public string Analysis
	{
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
	public EncoderCurve (string n, string width, string height, string meanSpeed, string maxSpeed, 
			string meanPower, string peakPower, string peakPowerT)
	{
		this.N = n;
		this.Width = width;
		this.Height = height;
		this.MeanSpeed = meanSpeed;
		this.MaxSpeed = maxSpeed;
		this.MeanPower = meanPower;
		this.PeakPower = peakPower;
		this.PeakPowerT = peakPowerT;
	}

	public string N;
	public string Width;
	public string Height;
	public string MeanSpeed;
	public string MaxSpeed;
	public string MeanPower;
	public string PeakPower;
	public string PeakPowerT;
}

