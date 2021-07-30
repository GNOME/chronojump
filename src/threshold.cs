/*
 * This file is part of ChronoJump
 *
 * ChronoJump is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or   
 * (at your option) any later version.
 *    
 * ChronoJump is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the 
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 *
 * Copyright (C) 2016-2017   Xavier de Blas <xaviblas@gmail.com> 
 */

/*
 * hscale goes from 1 to 10
 * threshold and label from 10 to 100
 * this is because there's a problem with hscale from 10 to 100 with increments of 10
 */
using System;

public class Threshold
{
	private int t;
	private int t_previous_on_this_mode; //on execute test, to decide if threshold have to be changed on SQL
	private int t_stored_on_chronopic; //to know if has to be changed on chronopic or not
	private int t_stored_on_chronopic2; //related to second chronopic (on multichronopic)

	//only constructed one time
	public Threshold()
	{
		t = 50;
		t_previous_on_this_mode = t;
		t_stored_on_chronopic = t;
		t_stored_on_chronopic2 = t;
	}

	//called when menuitem mode changes
	public bool SelectTresholdForThisMode(Constants.Modes m)
	{
		//declare it with default value to solve potential SQL problems
		string newThreshold = "5";

		//check current mode. Power modes doesn't use threshold
		newThreshold = SqlitePreferences.Select(nameAtSQL(m), false);

		if(Util.IsNumber(newThreshold, false))
		{
			t = Convert.ToInt32(newThreshold);
			t_previous_on_this_mode = t;
			return true;
		}
		return false;
	}

	//called when threshold changes by user using the GUI	
	public void UpdateFromGUI(int newThreshold)
	{
		t = newThreshold;
		/*
		 * t_previous_on_this_mode && t_stored_on_chronopic
		 * are not updated now to don't bother SQL and Chronopic
		 * they are updated on test execution (if needed)
		 */
	}

	//called on test execution
	public void UpdateAtDatabaseIfNeeded(Constants.Modes m)
	{
		if(t != t_previous_on_this_mode)
		{
			SqlitePreferences.Update(nameAtSQL(m), t.ToString(), false);
			t_previous_on_this_mode = t;
		}
	}

	//numCP 1: first chronopic; 2: second (on multichronopic)
	public bool ShouldUpdateChronopicFirmware(int numCP)
	{
		if(numCP == 1)
			return t != t_stored_on_chronopic;
		else //2
			return t != t_stored_on_chronopic2;
	}
	//numCP 1: first chronopic; 2: second (on multichronopic)
	public void ChronopicFirmwareUpdated(int numCP)
	{
		if(numCP == 1)
			t_stored_on_chronopic = t;
		else //2
			t_stored_on_chronopic2 = t;
	}

	public void ChronopicFirmwareReconnected(int numCP)
	{
		/*
		 * if Chronopic has been disconnected or is connected for the first time,
		 * make t_stored_on_chronopic = 50 (default value)
		 * and if we are on runs or with a different threshold,
		 * chronpic threshold will be updated
		 */
		if(numCP == 1)
			t_stored_on_chronopic = 50;
		else //2
			t_stored_on_chronopic2 = 50;
	}

	public string GetLabel()
	{
		return t.ToString();
	}

	public int SetHScaleValue()
	{
		return Convert.ToInt32(t / 10);
	}

	private string nameAtSQL(Constants.Modes m)
	{
		if(m == Constants.Modes.JUMPSSIMPLE || m == Constants.Modes.JUMPSREACTIVE)
			return "thresholdJumps";
		else if(m == Constants.Modes.RUNSSIMPLE || m == Constants.Modes.RUNSINTERVALLIC)
			return "thresholdRuns";
		else // (m == Constants.Modes.OTHER)
			return "thresholdOther";
	}

	//T is reserved word, so use GetT
	public int GetT
	{
		get { return t; }
	}	
}
