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
 *  Copyright (C) 2025   Xavier de Blas <xaviblas@gmail.com>
 */

using System;
using System.Collections.Generic; //List<T>

// this class will do same as encoder/util.R getDisplacement (), getDisplacementInertial (), getInertialDiametersPerMs ()

public class EncoderLikeRGetDisplacement
{
	private int ticksRotaryEncoder = 200; //our rotary axis encoder sends 200 ticks per revolution

	//constructor
	public EncoderLikeRGetDisplacement ()
	{
	}

	// in signals and curves, need to do conversions (invert, diameter)
	public List<double> GetDisplacement (
			bool capturing, EncoderConfiguration.Names econfName,
			List<int> dis_l, double diameter, double diameterExt, int gearedDown)
	{
		List<double> disFixed_l = new List<double> ();

		// no change on this encoder configurations
		if (
				econfName == EncoderConfiguration.Names.LINEAR ||
				econfName == EncoderConfiguration.Names.WEIGHTEDMOVPULLEYLINEARONPERSON1 ||
				econfName == EncoderConfiguration.Names.WEIGHTEDMOVPULLEYLINEARONPERSON1INV ||
				econfName == EncoderConfiguration.Names.WEIGHTEDMOVPULLEYLINEARONPERSON2 ||
				econfName == EncoderConfiguration.Names.WEIGHTEDMOVPULLEYLINEARONPERSON2INV ||
				econfName == EncoderConfiguration.Names.LINEARONPLANE ||
				econfName == EncoderConfiguration.Names.ROTARYFRICTIONSIDE ||
				econfName == EncoderConfiguration.Names.WEIGHTEDMOVPULLEYROTARYFRICTION ||
				econfName == EncoderConfiguration.Names.ROTARYAXISINERTIALMOVPULLEY)
		{
			foreach (int dis in dis_l)
				disFixed_l.Add (Convert.ToDouble (dis));

			return disFixed_l;
		}

		if ( ! capturing && (
					econfName == EncoderConfiguration.Names.LINEARINVERTED ||
					econfName == EncoderConfiguration.Names.WEIGHTEDMOVPULLEYLINEARONPERSON1INV ||
					econfName == EncoderConfiguration.Names.WEIGHTEDMOVPULLEYLINEARONPERSON2INV) )
			// On inverted modes the direction of the displacement is changed
		{
			foreach (int dis in dis_l)
				disFixed_l.Add (-1 * dis);
		}
		else if (econfName == EncoderConfiguration.Names.WEIGHTEDMOVPULLEYONLINEARENCODER)
		{
			// On geared down machines the displacement of the subject is multiplied by gearedDown
			// default is: gearedDown = 2. Future maybe this will be a parameter
			foreach (int dis in dis_l)
				disFixed_l.Add (dis * 2);
		}
		else if (econfName == EncoderConfiguration.Names.LINEARONPLANEWEIGHTDIFFANGLEMOVPULLEY)
		{
			foreach (int dis in dis_l)
				disFixed_l.Add (dis * gearedDown);
		}
		else if (econfName == EncoderConfiguration.Names.ROTARYFRICTIONAXIS)
		{
			// On rotary friction axis the displacement of the subject is proportional to the axis diameter
			// and inversely proportional to the diameter where the encoder is coupled
			foreach (int dis in dis_l)
				disFixed_l.Add (dis * diameter / diameterExt);
		}
		else if (econfName == EncoderConfiguration.Names.ROTARYAXIS ||
				econfName == EncoderConfiguration.Names.WEIGHTEDMOVPULLEYROTARYAXIS)
		{
			//On rotary encoders attached to fixed pulleys next to subjects (see config 1 and 3 in interface),
			//the displacement of the subject is anlge * radius

			//The angle rotated by the pulley is (ticks / ticksRotaryEncoder) * 2 * pi
			//The radium in mm is diameter * 1000 / 2
			foreach (int dis in dis_l)
				disFixed_l.Add (
						(dis / ticksRotaryEncoder) * Math.PI * (diameter * 1000)
						);
		}

		return (disFixed_l);
	}

	/*
	 * This function converts angular information from rotary encoder to linear information like linear encoder
	 * TThis is NOT the displacement of the person because con-ec phases roll in the same direction
	 * This is solved by the function getDisplacementInertialBody
	 */

	public List<double> GetDisplacementInertial (
			List<int> dis_l, EncoderConfiguration.Names econfName,
			//List<double> diameterPerTick_l, double diameterExt, int gearedDown) //diameterPerTick_l seems is not implemented even on R
			double diameter, double diameterExt, int gearedDown)
	{
		LogB.Information ("at getDisplacementInertial");
		List<double> disFixed_l = new List<double> ();

		// scanned displacement is ticks of rotary axis encoder
		// now convert it to mm of body displacement
		if(econfName == EncoderConfiguration.Names.ROTARYAXISINERTIAL ||
				econfName == EncoderConfiguration.Names.ROTARYAXISINERTIALLATERAL ||
				econfName == EncoderConfiguration.Names.ROTARYAXISINERTIALMOVPULLEY ||
				econfName == EncoderConfiguration.Names.ROTARYAXISINERTIALLATERALMOVPULLEY)
		{

			// Number of revolutions that the flywheel rotates every millisecond
			// One revolution every ticksRotaryEncoder ticks
			List<double> revolutionsPerMs_l = new List<double> ();
			foreach (int dis in dis_l)
				revolutionsPerMs_l.Add (dis / ticksRotaryEncoder);

			// The person is gearedDown from the machine point of view
			// If force multiplier is 2 (gearedDown = 0.5) the displacement of the body is
			// half the the displacement at the perimeter of the axis
			// Revolutions * perimeter * gearedDown  and converted cm -> mm
			for (int i = 0; i < dis_l.Count ; i ++)
			{
				//disFixed_l.Add (revolutionsPerMs_l[i] * Math.PI * diameterPerTick_l[i] * 10 * gearedDown);
				disFixed_l.Add (revolutionsPerMs_l[i] * Math.PI * diameter * 10 * gearedDown);
			}

		} else if(econfName == EncoderConfiguration.Names.ROTARYFRICTIONSIDEINERTIAL ||
				econfName == EncoderConfiguration.Names.ROTARYFRICTIONSIDEINERTIALLATERAL ||
				econfName == EncoderConfiguration.Names.ROTARYFRICTIONSIDEINERTIALMOVPULLEY)
		{
			for (int i = 0; i < dis_l.Count ; i ++)
			{
				//disFixed_l.Add (dis_l[i] * diameterPerTick_l[i] * gearedDown / diameterExt); //displacement of the axis
				disFixed_l.Add (dis_l[i] * diameter * gearedDown / diameterExt); //displacement of the axis
			}

		} else if(econfName == EncoderConfiguration.Names.ROTARYFRICTIONAXISINERTIALMOVPULLEY)
		{
			// If force multiplier is 2 (gearedDown = 0.5) the displacement of the body is
			// half the the displacement at the perimeter of the axis
			foreach (int dis in dis_l)
				disFixed_l.Add (dis * gearedDown);
		} else
		{
			foreach (int dis in dis_l)
				disFixed_l.Add (Convert.ToDouble (dis));
		}
		return disFixed_l;
	}
}
