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
 * Xavier de Blas: 
 * http://www.xdeblas.com, http://www.deporteyciencia.com (parleblas)
 */


public abstract class EndingConditions
{
	public EndingConditions() {
	}

	public virtual bool ConditionsOk() {
		return true;
	}
	
	~EndingConditions() {}
}


public class EndingConditionsJumpRj : EndingConditions
{
	//user decided to check this values as conditions
	private bool tvMinCheck;
	private bool tcMaxCheck;
	private bool tvDivTcMinCheck;

	//values of current event
	private double tvMin;
	private double tcMax;
	private double tvDivTcMin;

	//conditions reached if true
	private bool tvMinFound;
	private bool tcMaxFound;
	private bool tvDivTcMinFound;


	public EndingConditionsJumpRj(bool tvMinCheck, bool tcMaxCheck, bool tvDivTcMinCheck, double tvMin, double tcMax, double tvDivTcMin) {
		tvMinFound=false;
		tcMaxFound=false;
		tvDivTcMinFound=false;
	}

	public bool ConditionsOk(double tv, double tc) 
	{
		bool conditionsOk = false;

		if(tvMinCheck && tv < tvMin) {
			tvMinFound = true;
			conditionsOk = true;
		}

		if(tcMaxCheck && tc > tcMax) {
			tcMaxFound = true;
			conditionsOk = true;
		}

		double tvDivTc = 0;
		if(tc > 0)
			tvDivTc = tv/tc;

		if(tvDivTcMinCheck && tvDivTc < tvDivTcMin) {
			tvDivTcMinFound = true;
			conditionsOk = true;
		}

		return conditionsOk;
	}

	public bool TvMinFound
	{
		get { return tvMinFound; }
	}
	
	public bool TcMaxFound
	{
		get { return tcMaxFound; }
	}
	
	public bool TvDivTcMinFound
	{
		get { return tvDivTcMinFound; }
	}
	

	~EndingConditionsJumpRj() {}
}

public class EndingConditionsRunI : EndingConditions
{
	//user decided to check this values as conditions
	private bool timeCheck;
	private bool speedCheck;

	//values of current event
	private double timeMin;
	private double speedMin;

	//conditions reached if true
	private bool timeMinFound;
	private bool speedMinFound;


	public EndingConditionsRunI(bool timeMinCheck, bool speedCheck, double timeMin, double speedMin) {
		timeMinFound=false;
		speedMinFound=false;
	}

	public bool ConditionsOk(double time, double speed) 
	{
		bool conditionsOk = false;

		if(timeCheck && time < timeMin) {
			timeMinFound = true;
			conditionsOk = true;
		}

		if(speedCheck && speed < speedMin) {
			speedMinFound = true;
			conditionsOk = true;
		}

		return conditionsOk;
	}

	public bool TimeMinFound
	{
		get { return timeMinFound; }
	}
	
	public bool SpeedMinFound
	{
		get { return speedMinFound; }
	}
	

	~EndingConditionsRunI() {}
}

