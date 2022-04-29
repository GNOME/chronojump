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
 *  Copyright (C) 2021   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
using System.Data;
using System.Collections.Generic; //List<T>
using System.Diagnostics;  //Stopwatch

//to debug time needed for some processes
//Sw: Stopwatch
public class ChronoDebugAction
{
	private string action;
	private int elapsedMs;

	//constructor
	public ChronoDebugAction (string action, int elapsedMs)
	{
		this.action = action;
		this.elapsedMs = elapsedMs;
	}

	public override string ToString()
	{
		return string.Format("action: {0}, ms: {1}", action, elapsedMs);
	}

	public int ElapsedMs {
		get { return elapsedMs; }
	}
}

public class ChronoDebug
{
	private string name;
	private List<ChronoDebugAction> list;
	private Stopwatch sw;

	public ChronoDebug (string name)
	{
		this.name = name;

		list = new List<ChronoDebugAction>();
		sw = new Stopwatch();
	}

	public void Add (string action)
	{
		list.Add(new ChronoDebugAction(
					action,
					Convert.ToInt32(sw.Elapsed.TotalMilliseconds)
					));
	}

	public void Start ()
	{
		sw.Start();
		Add("Start");
	}

	public void StopAndPrint ()
	{
		Stop();
		PrintResults();
	}

	public void Stop ()
	{
		sw.Stop();
		Add("Stop");
	}

	public void PrintResults()
	{
		LogB.Information("ChronoDebug for " + name);
		foreach(ChronoDebugAction action in list)
			LogB.Information(action.ToString());
	}

	//Note this only works properly if there are just these two moments
	public int StartToEndInMs ()
	{
		if(list.Count == 0)
			return 0;

		//Start is ms 1
		ChronoDebugAction actionStop = list[list.Count -1];
		return actionStop.ElapsedMs;
	}
}

