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
using System.Collections.Generic; //List<T>
using System.Threading;

public partial class ChronoJumpWindow 
{
	public static int TestNum;
	public static bool TestsActive = false;
	
	//to repeat some actions
	private static int bucleCount;
		
	private List<cjTestTypes> sequence;
	
	public enum cjTestTypes { 
		MODE_POWERINERTIAL, 
		SESSION_LOAD,
		PERSON_SELECT,
		ENCODER_SIGNAL_LOAD,
		ENCODER_ECC_CON_INVERT,
		ENCODER_RECALCULATE,
		ENCODER_SET_SAVE_REPS,
		ENCODER_SET_SAVE_REPS_BUCLE
	}

	private List<cjTestTypes> sequenceEncoder1()
	{
		return new List<cjTestTypes> { 
			cjTestTypes.MODE_POWERINERTIAL, 
				cjTestTypes.SESSION_LOAD,
				cjTestTypes.PERSON_SELECT,
				cjTestTypes.ENCODER_SIGNAL_LOAD,
				cjTestTypes.ENCODER_ECC_CON_INVERT,
				cjTestTypes.ENCODER_RECALCULATE,
				cjTestTypes.ENCODER_ECC_CON_INVERT,
				cjTestTypes.ENCODER_RECALCULATE,
				cjTestTypes.ENCODER_ECC_CON_INVERT,
				cjTestTypes.ENCODER_RECALCULATE,
				cjTestTypes.ENCODER_ECC_CON_INVERT,
				cjTestTypes.ENCODER_RECALCULATE,
				/*
				cjTestTypes.ENCODER_SET_SAVE_REPS,
				cjTestTypes.ENCODER_SET_SAVE_REPS,
				cjTestTypes.ENCODER_SET_SAVE_REPS,
				cjTestTypes.ENCODER_SET_SAVE_REPS,
				*/
				cjTestTypes.ENCODER_SET_SAVE_REPS_BUCLE
		};
	}
	
	private void chronojumpWindowTestsStart() 
	{
		TestsActive = true;
		TestNum = 0;
		bucleCount = 2;
		sequence = sequenceEncoder1();

		chronojumpWindowTestsDo();
	}
	private void chronojumpWindowTestsNext() 
	{
		if(TestsActive) 
		{
			TestNum ++;
			if(TestNum < sequence.Count)
				chronojumpWindowTestsDo();
			else {
				bucleCount --;
				if(bucleCount > 0) {
					TestNum = 0;
					chronojumpWindowTestsDo();
				}
			}
		}
	}
	
	private void chronojumpWindowTestsDo() 
	{
		LogB.Information("TestNum: " + TestNum.ToString());
		
		//if process is very fast (no threads, no GUI problems) just call next from this method
		bool callNext = false;

		switch(sequence[TestNum]) {
			case cjTestTypes.MODE_POWERINERTIAL:
				chronojumpWindowTestsMode(Constants.Menuitem_modes.POWERINERTIAL);
				break;
			case cjTestTypes.SESSION_LOAD:
				chronojumpWindowTestsLoadSession(); //this also selects first person
				break;
			case cjTestTypes.PERSON_SELECT:
				chronojumpWindowTestsSelectPerson(1); //select 2nd person (Giles) //TODO: change this
				callNext = true;
				break;
			case cjTestTypes.ENCODER_SIGNAL_LOAD:
				chronojumpWindowTestsEncoderLoadSignal();
				break;
			case cjTestTypes.ENCODER_ECC_CON_INVERT:
				chronojumpWindowTestsEncoderEccConInvert();
				callNext = true;
				break;
			case cjTestTypes.ENCODER_RECALCULATE:
				chronojumpWindowTestsEncoderRecalculate();
				break;
			case cjTestTypes.ENCODER_SET_SAVE_REPS:
				chronojumpWindowTestsEncoderSetSaveReps();
				callNext = true;
				break;
			case cjTestTypes.ENCODER_SET_SAVE_REPS_BUCLE:
				chronojumpWindowTestsEncoderSetSaveRepsBucle();
				break;
				/*
				   case 6:
				   LogB.Information("ALL TESTS DONE");
				   break;
				*/
		}

		if(callNext)		
			chronojumpWindowTestsNext();
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

		on_open_activate(new Object(), new EventArgs());
		sessionLoadWin.SelectRow(0);
		sessionLoadWin.Button_accept.Click();
		
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
		LogB.TestStart("chronojumpWindowTestsLoadSignal");

		ArrayList data = encoderLoadSignalData(); //selects signals of this person, this session, this encoderGI
		EncoderSQL es = (EncoderSQL) data[0]; //gets first

		on_encoder_load_signal_clicked (Convert.ToInt32(es.uniqueID)); //opens load window with first selected

		genericWin.Button_accept.Click(); //this will call accepted

		LogB.TestEnd("chronojumpWindowTestsLoadSignal");
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


	//saves all best none 4top randomly
	private void chronojumpWindowTestsEncoderSetSaveReps()
	{
		LogB.TestStart("chronojumpWindowTestsEncoderSetSaveReps");

		//Constants.EncoderAutoSaveCurve easc;
		double d = rand.NextDouble(); //0 - 1
		if(d < .25)
			//easc = Constants.EncoderAutoSaveCurve.ALL;
			button_encoder_capture_curves_all.Click();
		else if(d < .5)
			//easc = Constants.EncoderAutoSaveCurve.BEST;
			button_encoder_capture_curves_best.Click();
		else if(d < .75)
			//easc = Constants.EncoderAutoSaveCurve.NONE;
			button_encoder_capture_curves_none.Click();
		else
			//easc = Constants.EncoderAutoSaveCurve.FROM4TOPENULTIMATE;
			button_encoder_capture_curves_4top.Click();

		//encoderCaptureSaveCurvesAllNoneBest(easc, encoderCaptureOptionsWin.GetMainVariable());
		
		LogB.TestEnd("chronojumpWindowTestsEncoderSetSaveReps: " + d.ToString());
	}
	
	//saves all best none 4top randomly
	private void chronojumpWindowTestsEncoderSetSaveRepsBucle()
	{
		LogB.TestStart("chronojumpWindowTestsEncoderSetSaveReps");

		saveRepsLastI = -1;
		saveRepsBucleDoing = true;
		saveRepsBucleCount = 25;

		//make interval bigger if you cannot see GUI updating
		GLib.Timeout.Add(500, new GLib.TimeoutHandler(chronojumpWindowTestsEncoderSetSaveRepsBucleDo));
		
		LogB.TestEnd("chronojumpWindowTestsEncoderSetSaveReps");
	}
	bool saveRepsBucleDoing;
	int saveRepsBucleCount;
	int saveRepsLastI;
	private bool chronojumpWindowTestsEncoderSetSaveRepsBucleDo()
	{
		if(! saveRepsBucleDoing)
			return false;
		
		//LogB.Information(" saveReps(" + saveRepsBucleCount.ToString() + ") ");

		//get random value but different than last time
		int i;
		do {
			i = rand.Next(1,5); //1-4
		} while(i == saveRepsLastI);

		saveRepsLastI = i;
		
		//LogB.Information(" [i=" + i.ToString() + "] ");
		
		Constants.EncoderAutoSaveCurve easc;
		if(i == 1)
			//easc = Constants.EncoderAutoSaveCurve.ALL;
			button_encoder_capture_curves_all.Click();
		else if(i == 2)
			//easc = Constants.EncoderAutoSaveCurve.BEST;
			button_encoder_capture_curves_best.Click();
		else if(i == 3)
			//easc = Constants.EncoderAutoSaveCurve.NONE;
			button_encoder_capture_curves_none.Click();
		else
			//easc = Constants.EncoderAutoSaveCurve.FROM4TOPENULTIMATE;
			button_encoder_capture_curves_4top.Click();

		//encoderCaptureSaveCurvesAllNoneBest(easc, encoderCaptureOptionsWin.GetMainVariable());
			
		saveRepsBucleCount --;
		if(saveRepsBucleCount > 0)
			return true;
		else {
			saveRepsBucleDoing = false;
			chronojumpWindowTestsNext();
			return false;
		}
	}

}
