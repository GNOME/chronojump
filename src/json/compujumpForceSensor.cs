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
 * Copyright (C) 2020 Xavier de Blas
 */

using System;
using System.Text;
using System.Collections;


public class UploadForceSensorDataFullObject
{
	public int uniqueId; //used for SQL load and delete
	public int personId;
	public int stationId;
	public int exerciseId;
	public string laterality;
	public string resistance; //stiffness
	public UploadForceSensorDataObject uo;

	public UploadForceSensorDataFullObject(int uniqueId, int personId, int stationId, int exerciseId,
			string laterality, string resistance, UploadForceSensorDataObject uo)
	{
		this.uniqueId = uniqueId;
		this.personId = personId;
		this.stationId = stationId;
		this.exerciseId = exerciseId;
		this.laterality = laterality;
		this.resistance = resistance;
		this.uo = uo;
	}

	public string ToSQLInsertString ()
	{
		return
			"NULL, " +
			personId.ToString() + ", " +
			stationId.ToString() + ", " +
			exerciseId.ToString() + ", " +
			"\"" + laterality + "\", " +
			"\"" + resistance + "\", " +
			uo.ToString();
	}
}

public class UploadForceSensorDataObject
{
	public string variability;
	public string timeTotal;
	public string impulse;
	public string workJ;
	public int repetitions;
	public int numRep;
	public string repCriteria;
	public string time;  //duration?
	public string range;
	public string fmaxRaw;
	public string rfdmeanRaw;
	public string rfdmaxRaw;
	public string fmaxModel;
	public string rfdmaxModel;
	//only on elastic
	public string vmean;
	public string vmax;
	public string amean;
	public string amax;
	public string pmean;
	public string pmax;

	//constructor called after capture
	public UploadForceSensorDataObject()
	{
	}

	public override string ToString()
	{
		return
			"\"" + variability.ToString() + "\", " +
			"\"" + timeTotal.ToString() + "\"," +
			"\"" + impulse.ToString() + "\"," +
			"\"" + workJ.ToString() + "\"," +
			repetitions.ToString() + "," +
			numRep.ToString() + "," +
			"\"" + repCriteria.ToString() + "\", " +
			"\"" + time.ToString() + "\"," +
			"\"" + range.ToString() + "\"," +
			"\"" + fmaxRaw.ToString() + "\"," +
			"\"" + rfdmeanRaw.ToString() + "\"," +
			"\"" + rfdmaxRaw.ToString() + "\"," +
			"\"" + fmaxModel.ToString() + "\"," +
			"\"" + rfdmaxModel.ToString() + "\"," +
			"\"" + vmean.ToString() + "\"," +
			"\"" + vmax.ToString() + "\"," +
			"\"" + amean.ToString() + "\"," +
			"\"" + amax.ToString() + "\"," +
			"\"" + pmean.ToString() + "\"," +
			"\"" + pmax.ToString() + "\")";
	}
}
