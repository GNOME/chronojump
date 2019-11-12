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
 *  Copyright (C) 2019   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
using System.Collections.Generic; //List<T>

public abstract class ForceSensorDynamics
{
	public bool CalculedElasticPSAP; //Position Speed Accel Power, boolena to know if we have to gather these lists

	protected List<int> time_micros_l;
	protected List<double> force_l;
	protected ForceSensor.CaptureOptions fsco;
	protected ForceSensorExercise fse;
	protected double stiffness;
	protected double totalMass;

	protected List<double> time_l;

	protected void initialize(List<double> force_l, 
			ForceSensor.CaptureOptions fsco, ForceSensorExercise fse,
			double personMass, double stiffness)
	{
		this.force_l = force_l;
		this.fsco = fsco;
		this.fse = fse;
		this.stiffness = stiffness;

		totalMass = 0;
		if(fse.PercentBodyWeight > 0 && personMass > 0)
			totalMass = fse.PercentBodyWeight * personMass / 100.0;

		CalculedElasticPSAP = false;
	}

	protected double calculeForceWithCaptureOptions(double force)
	{
		if(fsco == ForceSensor.CaptureOptions.ABS)
			return Math.Abs(force);
		if(fsco == ForceSensor.CaptureOptions.INVERTED)
			return -1 * force;

		return force;
	}
	protected List<double> calculeForceWithCaptureOptionsFullSet()
	{
		for(int i = 0 ; i < force_l.Count; i ++)
			force_l[i] = calculeForceWithCaptureOptions(force_l[i]);

		return force_l;
	}

	public List<double> GetForces()
	{
		return force_l;
	}

	//this 4 are only implemented on elastic
	public virtual List<double> GetPositions()
	{
		return new List<double>();
	}

	public virtual List<double> GetSpeeds()
	{
		return new List<double>();
	}

	public virtual List<double> GetAccels()
	{
		return new List<double>();
	}

	public virtual List<double> GetPowers()
	{
		return new List<double>();
	}
}
	
public class ForceSensorDynamicsNotElastic : ForceSensorDynamics
{
	List<double> position_l;
	List<double> speed_l;
	List<double> accel_l;
	List<double> power_l;

	public ForceSensorDynamicsNotElastic (List<int> time_micros_l, List<double> force_l, 
			ForceSensor.CaptureOptions fsco, ForceSensorExercise fse,
			double personMass, double stiffness)
	{
		initialize(force_l, fsco, fse, personMass, stiffness);

		if(! fse.ForceResultant)
		{
			calculeForceWithCaptureOptionsFullSet();
			return;
		}

		calcule();
	}

	//forces are updated, so do not Add to the list
	private void calcule()
	{
		double accel = 0;

		for (int i = 0 ; i < force_l.Count; i ++)
		{
			double force = Math.Sqrt(
					Math.Pow(Math.Cos(fse.AngleDefault * Math.PI / 180.0) * (force_l[i] + totalMass * accel), 2) +                  //Horizontal
					Math.Pow(Math.Sin(fse.AngleDefault * Math.PI / 180.0) * (force_l[i] + totalMass * accel) + totalMass * 9.81, 2) //Vertical
					);
			force_l[i] = calculeForceWithCaptureOptions(force);
		}
	}
}

public class ForceSensorDynamicsElastic : ForceSensorDynamics
{
	List<double> position_l;
	List<double> speed_l;
	List<double> accel_l;
	List<double> power_l;

	public ForceSensorDynamicsElastic (List<int> time_micros_l, List<double> force_l, 
			ForceSensor.CaptureOptions fsco, ForceSensorExercise fse,
			double personMass, double stiffness)
	{
		if(! fse.ForceResultant)
		{
			calculeForceWithCaptureOptionsFullSet();
			return;
		}

		initialize(force_l, fsco, fse, personMass, stiffness);
		convertTimeToSeconds(time_micros_l);
		calcule();
		CalculedElasticPSAP = true;
	}

	//time comes in microseconds
	private void convertTimeToSeconds(List<int> time_micros_l)
	{
		time_l = new List<double>();
		for (int i = 0 ; i < time_micros_l.Count; i ++)
		{
			time_l.Add(time_micros_l[i] / 1000000.0);
			//LogB.Information(string.Format("i: {0}, time_micros_l[i]: {1}, time_l: {2}", i, time_micros_l[i], time_l[i]));
		}
	}

	private void calcule()
	{
		//TODO: check minimum length of forces

		position_l = new List<double>();
		speed_l = new List<double>();
		accel_l = new List<double>();
		power_l = new List<double>();

		calculePositions();
		calculeSpeeds();
		calculeAccels();
		calculeForces();
		calculePowers();
	}
		
	private void calculePositions()
	{
		for (int i = 0 ; i < force_l.Count; i ++)
			position_l.Add(force_l[i] / stiffness);
	}

	private void calculeSpeeds()
	{
		for (int i = 0 ; i < time_l.Count; i ++)
		{
			int pre = i - 1;
			int post = i + 1;

			if(i == 0)
				pre = 0;
			else if(i == time_l.Count -1)
				post = i;

			speed_l.Add( (position_l[post] - position_l[pre]) / (time_l[post] - time_l[pre]) );
		}
	}

	private void calculeAccels()
	{
		for (int i = 0 ; i < speed_l.Count; i ++)
		{
			int pre = i - 1;
			int post = i + 1;

			if(i == 0)
				pre = 0;
			else if(i == speed_l.Count -1)
				post = i;

			accel_l.Add( (speed_l[post] - speed_l[pre]) / (time_l[post] - time_l[pre]) );
			//LogB.Information(string.Format("i: {0}, accel_l[i]: {1}", i, accel_l[i]));
		}
	}

	//forces are updated, so do not Add to the list
	private void calculeForces()
	{
		for (int i = 0 ; i < force_l.Count; i ++)
		{
			//LogB.Information(string.Format("i: {0}, force_l[i]: {1}, force_l.Count: {2}", i, force_l[i], force_l.Count));
			double force = Math.Sqrt(
					Math.Pow(Math.Cos(fse.AngleDefault * Math.PI / 180.0) * (force_l[i] + totalMass * accel_l[i]), 2) +                  //Horizontal
					Math.Pow(Math.Sin(fse.AngleDefault * Math.PI / 180.0) * (force_l[i] + totalMass * accel_l[i]) + totalMass * 9.81, 2) //Vertical
					);
			force_l[i] = calculeForceWithCaptureOptions(force);
			//LogB.Information(string.Format("i: {0}, force_l[i]: {1}", i, force_l[i]));
		}
	}
	
	private void calculePowers()
	{	
		for (int i = 0 ; i < force_l.Count; i ++)
		{
			power_l.Add(
					speed_l[i] * (force_l[i] + totalMass * accel_l[i]) + //Power associated to the acceleration of the mass
					speed_l[i] * (Math.Sin(fse.AngleDefault * Math.PI / 180.0) * totalMass * 9.81) //Power associated to the gravitatory field
				   );
		}
	}

	public override List<double> GetPositions()
	{
		return position_l;
	}

	public override List<double> GetSpeeds()
	{
		return speed_l;
	}

	public override List<double> GetAccels()
	{
		return accel_l;
	}

	public override List<double> GetPowers()
	{
		return power_l;
	}
}
