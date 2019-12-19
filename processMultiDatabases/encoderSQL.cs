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

//different form chronojump/encoder EncoderSQL
public class EncoderSQL
{
        public string uniqueID;
        public int personID;
        public int sessionID;
        public int exerciseID;
        public string eccon;
        public string laterality;
        public string extraWeight;
        public string signalOrCurve;
        public string filename;
        public string url;      //URL of data of signals and curves. Stored in DB as relative. Used in software as absolute. See SqliteEncoder
        public int time;
        public int minHeight;
        public string description;
        public string status;   //active or inactive curves
        public string videoURL; //URL of video of signals. Stored in DB as relative. Used in software as absolute. See SqliteEncoder

	public string encoderConfiguration;
        public string future1;
        public string future2;
        public string future3;

	public EncoderSQL ()
        {
        }

	public EncoderSQL (string uniqueID, int personID, int sessionID, int exerciseID,
			string eccon, string laterality, string extraWeight, string signalOrCurve,
			string filename, string url, int time, int minHeight,
			string description, string status, string videoURL,
			//EncoderConfiguration encoderConfiguration,
			string encoderConfiguration,
			string future1, string future2, string future3//,
			//string exerciseName
			)
	{
		this.uniqueID = uniqueID;
		this.personID = personID;
		this.sessionID = sessionID;
		this.exerciseID = exerciseID;
		this.eccon = eccon;
		this.laterality = laterality;
		this.extraWeight = extraWeight;
		this.signalOrCurve = signalOrCurve;
		this.filename = filename;
		this.url = url;
		this.time = time;
		this.minHeight = minHeight;
		this.description = description;
		this.status = status;
		this.videoURL = videoURL;
		this.encoderConfiguration = encoderConfiguration;
		this.future1 = future1; //on curves: meanPower. Better use alter table
		this.future2 = future2; //on curves: meanSpeed
		this.future3 = future3; //on curves: meanForce
		//this.exerciseName = exerciseName;
	}

	public override string ToString()
	{
		return string.Format("{0} - {1}", url, filename);
	}
}

