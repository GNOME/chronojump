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

public class EncoderBarsData
{
	public double Start;
	public double Duration;
	public double Range;
	public double MeanSpeed;
	public double MaxSpeed;
	public double MeanForce;
	public double MaxForce;
	public double MeanPower;
	public double PeakPower;
	public double WorkJ;
	public double Impulse;
	
	public EncoderBarsData (double start, double duration, double range,
			double meanSpeed, double maxSpeed, double meanForce, double maxForce,
			double meanPower, double peakPower, double workJ, double impulse)
	{
		this.Start = start;
		this.Duration = duration;
		this.Range = range;
		this.MeanSpeed = meanSpeed;
		this.MaxSpeed  = maxSpeed;
		this.MeanForce = meanForce;
		this.MaxForce  = maxForce;
		this.MeanPower = meanPower;
		this.PeakPower = peakPower;
		this.WorkJ = workJ;
		this.Impulse = impulse;
	}

	public double GetValue (string option)
	{
		if(option == Constants.Start)
			return Start;
		if(option == Constants.Duration)
			return Duration;
		//if(option == Constants.Range)
		//	return Range;
		else if(option == Constants.RangeAbsolute)
			return Math.Abs(Range);
		else if(option == Constants.MeanSpeed)
			return MeanSpeed;
		else if(option == Constants.MaxSpeed)
			return MaxSpeed;
		else if(option == Constants.MeanForce)
			return MeanForce;
		else if(option == Constants.MaxForce)
			return MaxForce;
		else if(option == Constants.MeanPower)
			return MeanPower;
		else if(option == Constants.PeakPower)
			return PeakPower;
		else if(option == Constants.WorkJ)
			return WorkJ;
		else if(option == Constants.Impulse)
			return Impulse;

		return MeanPower;
	}
	
	~EncoderBarsData() {}
}
