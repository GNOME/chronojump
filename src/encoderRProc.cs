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
 *  Copyright (C) 2004-2015   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
using System.Diagnostics; 	//for detect OS and for Process


public abstract class EncoderRProc 
{
	protected Process p;
	protected ProcessStartInfo pinfo;

	protected bool running;

	protected void StartOrContinue() 
	{
		if(isRunning())
			continueProcess();
		else
			startProcess();
	}

	private bool isRunning() 
	{
		if(p == null)
			return false;

		Process [] pids = Process.GetProcessesByName("Rscript");

		foreach (Process myPid in pids)
			if (myPid.Id == Convert.ToInt32(p.Id))
				return true;
	
		return false;
	}

	protected virtual bool startProcess() {
		return true;
	}
	protected virtual bool continueProcess() {
		return true;
	}
}

public class EncoderRProcCapture : EncoderRProc 
{
	public EncoderRProcCapture() {
	}
}

public class EncoderRProcAnalyze : EncoderRProc 
{
	public EncoderRProcAnalyze() {
	}

	protected override bool startProcess() 
	{
		try {   
			p = new Process();
			p.StartInfo = pinfo;
			p.Start();

			LogB.Information(p.StandardOutput.ReadToEnd());
			LogB.Warning(p.StandardError.ReadToEnd());

			//p.WaitForExit(); //do NOT wait for exit
		} catch {
			return false;
		}
			
		running = true;
		return true;
	}
	
	protected override bool continueProcess() 
	{
		/*
		 * create a file to be file to be readed by the Rscript
		 * or send there STDIN, like:
		 * p.StandardInput.WriteLine();
		 */
		
		return true;
	}

}
