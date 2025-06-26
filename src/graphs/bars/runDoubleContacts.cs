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
 * Copyright (C) 2004-2025   Xavier de Blas <xaviblas@gmail.com>
 */

using System;
using Gtk;
using System.Collections.Generic; //List
using Mono.Unix;


public class CairoManageRunDoubleContacts
{
	private DrawingArea darea;
	private string fontStr;

	public CairoManageRunDoubleContacts (DrawingArea darea, string fontStr)
	{
		this.darea = darea;
		this.fontStr = fontStr;
	}

	public void Paint (EventExecute currentEventExecute, RunPhaseTimeList runPTL, double timeTotal,
			string intervalTimesString) //"" on runSimple
	{
		if(darea == null || darea.Window == null) //at start program, this can fail
			return;

		// 1) get data
		List<RunPhaseTimeListObject> runPTLInListForPainting = runPTL.InListForPainting();

		double negativePTLTime = getRunSRunINegativePTLTime(runPTLInListForPainting);
		double timeTotalWithExtraPTL = getRunSRunITimeTotalWithExtraPTLTime (timeTotal, runPTLInListForPainting, negativePTLTime);

		LogB.Information(string.Format("timeTotal: {0}, negativePTLTime: {1}, timeTotalWithExtraPTL: {2}",
					timeTotal, negativePTLTime, timeTotalWithExtraPTL));

		// 2) graph
		CairoRunDoubleContacts crdc;
		if(intervalTimesString == "") //runSimple
			crdc = new CairoRunDoubleContacts (darea, fontStr);
		else
			crdc = new CairoRunIntervalDoubleContacts (darea, fontStr, intervalTimesString);

		crdc.GraphDo (runPTLInListForPainting,
				timeTotal, timeTotalWithExtraPTL, negativePTLTime);
	}

	private double getRunSRunINegativePTLTime (List<RunPhaseTimeListObject> runPTLInListForPainting)
	{
		return 0;

		/*
		 * inactive because when user stays lot of time (in contact) before the test, with this code the graph will have almost this contact status
		 * having above return 0; fixed this: https://gitlab.gnome.org/GNOME/chronojump/issues/110
		 *
		if(runPTLInListForPainting.Count > 0)
		{
			//get first TC start value
			RunPhaseTimeListObject rptlfp = (RunPhaseTimeListObject) runPTLInListForPainting[0];
			double firstValue = rptlfp.tcStart;

			if(firstValue < 0)
				return Math.Abs(firstValue);
		}

		return 0;
		*/
	}

	private double getRunSRunITimeTotalWithExtraPTLTime (double timeTotal, List<RunPhaseTimeListObject> runPTLInListForPainting, double negativePTLTime)
	{
		double timeTotalWithExtraPTL = timeTotal;
		if(runPTLInListForPainting.Count > 0)
		{
			//get last TC end value
			RunPhaseTimeListObject rptlfp = (RunPhaseTimeListObject) runPTLInListForPainting[runPTLInListForPainting.Count -1];
			timeTotalWithExtraPTL = rptlfp.tcEnd;
		}

		return timeTotalWithExtraPTL + negativePTLTime;
	}
}
