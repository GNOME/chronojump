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
 * Copyright (C) 2016   Xavier de Blas <xaviblas@gmail.com> 
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

	//only constructed one time
	public Threshold()
	{
		t = 50;
		t_previous_on_this_mode = t;
		t_stored_on_chronopic = t;
	}

	//called when menuitem mode changes
	public bool SelectTresholdForThisMode(Constants.Menuitem_modes m)
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
	public void UpdateAtDatabaseIfNeeded(Constants.Menuitem_modes m)
	{
		if(t != t_previous_on_this_mode)
		{
			SqlitePreferences.Update(nameAtSQL(m), t.ToString(), false);
			t_previous_on_this_mode = t;
		}
	}

	public bool ShouldUpdateChronopicFirmware()
	{
		return t != t_stored_on_chronopic;
	}
	public void ChronopicFirmwareUpdated()
	{
		t_stored_on_chronopic = t;
	}

	public string GetLabel()
	{
		return t.ToString();
	}

	public int SetHScaleValue()
	{
		return Convert.ToInt32(t / 10);
	}

	private string nameAtSQL(Constants.Menuitem_modes m)
	{
		if(m == Constants.Menuitem_modes.JUMPSSIMPLE || m == Constants.Menuitem_modes.JUMPSREACTIVE)
			return "thresholdJumps";
		else if(m == Constants.Menuitem_modes.RUNSSIMPLE || m == Constants.Menuitem_modes.RUNSINTERVALLIC)
			return "thresholdRuns";
		else // (m == Constants.Menuitem_modes.OTHER)
			return "thresholdOther";
	}

	//T is reserved word, so use GetT
	public int GetT
	{
		get { return t; }
	}	
}
