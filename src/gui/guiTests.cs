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
 * Copyright (C) 2004-2016   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
using System.Collections; //ArrayList
using System.Threading;

public class GuiT 
{
	protected enum StatusTypes { WAITING, RUNNING } //WAITING means waiting or done
	protected StatusTypes status = StatusTypes.WAITING;
	protected string message;

	public GuiT() {
	}
		
	protected void logStart() {
		LogB.TestStart(message);
	}
	protected void logEnd() {
		LogB.TestEnd(message);
	}

	//called from chronojump gui
	public void End(int seconds) 
	{
		LogB.Information("End (sleep start)");
		Thread.Sleep(seconds*1000);

		LogB.Information("End (changing to waiting)");
		status = StatusTypes.WAITING;
		LogB.Information("End (changed to waiting)");
	}

	public void WaitEnd() {
		LogB.Information("at waitEnd");
		while(status == StatusTypes.RUNNING)
			Thread.Sleep(50);
		
		LogB.Information("ended waitEnd");

		logEnd();
	}
}	

public class GuiTLoadSignal : GuiT 
{

	public GuiTLoadSignal() 
	{
		message = "guiTLoadSignal";
		status = StatusTypes.RUNNING;
		logStart();
	}
}	

public partial class ChronoJumpWindow 
{
	public GuiTLoadSignal GuiTLoadSignalObject;

	private void chronojumpWindowTests() 
	{
		chronojumpWindowTestsMode(Constants.Menuitem_modes.POWERINERTIAL);
		
		//using tutorial (or demo) session
		chronojumpWindowTestsLoadSession(); //this also selects first person
		
		chronojumpWindowTestsSelectPerson(1); //select 2nd person (Giles)
		
		chronojumpWindowTestsEncoderLoadSignal();

//		chronojumpWindowTestsWaitS(4);

//		chronojumpWindowTestsEncoderEccConInvert();

//		chronojumpWindowTestsWaitS(1);
		
//		chronojumpWindowTestsEncoderRecalculate();
		
//		chronojumpWindowTestsWaitS(4);

/*
		chronojumpWindowTestsEncoderEccConInvert();
		chronojumpWindowTestsEncoderRecalculate();
*/

		/*
		chronojumpWindowTestsEncoderSave(EncoderAutoSaveCurve.ALL);
		chronojumpWindowTestsEncoderSave(EncoderAutoSaveCurve.NONE);
		chronojumpWindowTestsEncoderSave(EncoderAutoSaveCurve.BEST);
		chronojumpWindowTestsEncoderSave(EncoderAutoSaveCurve.FROM4TOPENULTIMATE);
		*/
	}
		
	private void chronojumpWindowTestsWaitS(int seconds)
	{
		LogB.TestStart("chronojumpWindowTestsWaitS");

		Thread.Sleep(1000 * seconds);
		
		LogB.TestEnd("chronojumpWindowTestsWaitS");
	}

		
	private void chronojumpWindowTestsMode(Constants.Menuitem_modes m) 
	{
		LogB.TestStart("chronojumpWindowTestsMode", m.ToString());

		//disable autodetect
		//configAutodetectPort = Config.AutodetectPortEnum.INACTIVE;

		select_menuitem_mode_toggled(m);
		
		LogB.TestEnd("chronojumpWindowTestsMode");
	}

	private void chronojumpWindowTestsLoadSession() 
	{
		LogB.TestStart("chronojumpWindowTestsLoadSession");

		currentSession = SqliteSession.Select ("1"); //select first session (if is not deleted)
		on_load_session_accepted();
		
		LogB.TestEnd("chronojumpWindowTestsLoadSession");
	}
			
	private void chronojumpWindowTestsSelectPerson(int numInPersonTV) 
	{
		LogB.TestStart("chronojumpWindowTestsSelectPerson");

		selectRowTreeView_persons(treeview_persons, treeview_persons_store, numInPersonTV);
		
		LogB.TestEnd("chronojumpWindowTestsSelectPerson");
	}

	private void chronojumpWindowTestsEncoderLoadSignal()
	{
		GuiTLoadSignalObject = new GuiTLoadSignal();
		//LogB.TestStart("chronojumpWindowTestsLoadSignal");

		ArrayList data = encoderLoadSignalData(); //selects signals of this person, this session, this encoderGI
		EncoderSQL es = (EncoderSQL) data[0]; //gets first

		on_encoder_load_signal_clicked (Convert.ToInt32(es.uniqueID)); //opens load window with first selected

		genericWin.Button_accept.Click(); //this will call accepted

		GuiTLoadSignalObject.WaitEnd();
	}
	
	private void chronojumpWindowTestsEncoderEccConInvert()
	{
		LogB.TestStart("chronojumpWindowTestsEncoderEccConInvert");

		if(combo_encoder_eccon.Active == 0)
			combo_encoder_eccon.Active = 1;
		else
			combo_encoder_eccon.Active = 0;
		
		LogB.TestEnd("chronojumpWindowTestsEncoderEccConInvert");
	}
	
	private void chronojumpWindowTestsEncoderRecalculate()
	{
		LogB.TestStart("chronojumpWindowTestsEncoderRecalculate");

		on_button_encoder_recalculate_clicked (new Object (), new EventArgs ());

		LogB.TestEnd("chronojumpWindowTestsEncoderRecalculate");
	}
}
