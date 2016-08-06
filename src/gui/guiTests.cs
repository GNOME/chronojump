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

/* TODO:
 * separate in various classes, files
 * progressBar
 * button to end
 * summary
 */

public class CJTests 
{
	public enum Types 
	{
		MODE_POWERGRAVITATORY, 
		MODE_POWERINERTIAL, 
		SESSION_LOAD,
		PERSON_SELECT,
		ENCODER_SIGNAL_LOAD,
		ENCODER_ECC_CON_INVERT,
		ENCODER_RECALCULATE,
		ENCODER_SET_SAVE_REPS,
		ENCODER_SET_SAVE_REPS_BUCLE,
		BUCLE_1_ON,
		BUCLE_1_OFF,
		BUCLE_2_ON,
		BUCLE_2_OFF,
		END
	}
	
	public static List<CJTests.Types> SequenceEncoder1 = new List<CJTests.Types> 
	{
		CJTests.Types.MODE_POWERINERTIAL, 
		CJTests.Types.SESSION_LOAD,
		CJTests.Types.PERSON_SELECT,
		CJTests.Types.ENCODER_SIGNAL_LOAD,
		CJTests.Types.ENCODER_ECC_CON_INVERT,
		CJTests.Types.ENCODER_RECALCULATE,
		CJTests.Types.ENCODER_ECC_CON_INVERT,
		CJTests.Types.ENCODER_RECALCULATE,
		CJTests.Types.ENCODER_ECC_CON_INVERT,
		CJTests.Types.ENCODER_RECALCULATE,
		CJTests.Types.ENCODER_ECC_CON_INVERT,
		CJTests.Types.ENCODER_RECALCULATE,
		/*
		   CJTests.Types.ENCODER_SET_SAVE_REPS,
		   CJTests.Types.ENCODER_SET_SAVE_REPS,
		   CJTests.Types.ENCODER_SET_SAVE_REPS,
		   CJTests.Types.ENCODER_SET_SAVE_REPS,
		   */
		CJTests.Types.ENCODER_SET_SAVE_REPS_BUCLE
	};
	
	public static List<CJTests.Types> SequenceEncoder2 = new List<CJTests.Types> 
	{
		CJTests.Types.MODE_POWERGRAVITATORY, 
		CJTests.Types.SESSION_LOAD,
		CJTests.Types.BUCLE_1_ON,
			CJTests.Types.PERSON_SELECT, //bucle1startPos //repeat from here
			CJTests.Types.BUCLE_2_ON,
				CJTests.Types.ENCODER_SIGNAL_LOAD, //bucle2startPos
				CJTests.Types.ENCODER_RECALCULATE,
			CJTests.Types.BUCLE_2_OFF,
		CJTests.Types.BUCLE_1_OFF,
		CJTests.Types.END
	};

}

public partial class ChronoJumpWindow 
{
	private static int sequenceCurrent;
	private static bool testsActive = false;
	
	//to repeat some actions
	private static int bucleCount;
		
	private List<CJTests.Types> sequence;
	
	/*
	private void chronojumpWindowTestsStartOld() 
	{
		testsActive = true;
		sequenceCurrent = 0;
		bucleCount = 2;
		sequence = sequenceEncoder1();
	
		chronojumpWindowTestsDo();
	}
	private void chronojumpWindowTestsNextOld() 
	{
		if(testsActive) 
		{
			sequenceCurrent ++;
			if(sequenceCurrent < sequence.Count)
				chronojumpWindowTestsDo();
			else {
				bucleCount --;
				if(bucleCount > 0) {
					sequenceCurrent = 0;
					chronojumpWindowTestsDo();
				}
			}
		}
	}
	*/

	int bucle1count;
	int bucle2count;
	int bucle1startPos;
	int bucle2startPos;
	bool bucle1ended;
	bool bucle2ended;
	int bucleCurrent;
	private void chronojumpWindowTestsStart() 
	{
		testsActive = true;
		sequence = CJTests.SequenceEncoder2;
		sequenceCurrent = 0;
		bucleCurrent = 0;

		chronojumpWindowTestsDo();
	}

	//TODO: move this and all the control variables to CJTests class	
	private void chronojumpWindowTestsNext() 
	{
		if(! testsActive) 
			return;

		sequenceCurrent ++;

		if(sequence[sequenceCurrent] == CJTests.Types.BUCLE_1_ON) 
		{
			bucleCurrent = 1;
			bucle1ended = false;
			bucle1count = 0;
			sequenceCurrent ++;
			bucle1startPos = sequenceCurrent;
		} 
		
		if(sequence[sequenceCurrent] == CJTests.Types.BUCLE_2_ON) 
		{
			bucleCurrent = 2;
			bucle2ended = false;
			bucle2count = 0;
			sequenceCurrent ++;
			bucle2startPos = sequenceCurrent;
		}
		
		if(sequence[sequenceCurrent] == CJTests.Types.BUCLE_2_OFF) 
		{
			if(bucle2ended) {
				//LogB.Information(" 2 OFF A");
				bucleCurrent --;
				sequenceCurrent ++;
			} else {
				//LogB.Information(" 2 OFF B");
				bucle2count ++;
				sequenceCurrent = bucle2startPos;
			}
		} 
		
		if(sequence[sequenceCurrent] == CJTests.Types.BUCLE_1_OFF) 
		{
			if(bucle1ended) {
				//LogB.Information(" 1 OFF A");
				bucleCurrent --;
				sequenceCurrent ++;
			}
			else {
				//LogB.Information(" 1 OFF B");
				bucle1count ++;
				sequenceCurrent = bucle1startPos;
			}
		} 
		
			
		chronojumpWindowTestsDo();
		
		if(sequence[sequenceCurrent] == CJTests.Types.END)
		{
			testsActive = false;
			return;
		}
	}

	private void chronojumpWindowTestsDo() 
	{
		LogB.Information("sequenceCurrent: " + sequenceCurrent.ToString());
		
		//if process is very fast (no threads, no GUI problems) just call next from this method
		bool callNext = false;

		bool bucleContinues = true;

		int bcount = 0;
		if(bucleCurrent == 1)
			bcount = bucle1count;
		else if(bucleCurrent == 2)
			bcount = bucle2count;

		switch(sequence[sequenceCurrent]) {
			case CJTests.Types.MODE_POWERGRAVITATORY:
				chronojumpWindowTestsMode(Constants.Menuitem_modes.POWERGRAVITATORY);
				break;
			case CJTests.Types.MODE_POWERINERTIAL:
				chronojumpWindowTestsMode(Constants.Menuitem_modes.POWERINERTIAL);
				break;
			case CJTests.Types.SESSION_LOAD:
				chronojumpWindowTestsLoadSession(); //this also selects first person
				break;
			case CJTests.Types.PERSON_SELECT:
				bucleContinues = chronojumpWindowTestsSelectPerson(bcount);
				callNext = true;
				break;
			case CJTests.Types.ENCODER_SIGNAL_LOAD:
				bucleContinues = chronojumpWindowTestsEncoderLoadSignal(bcount);
				if(bucleContinues)
					callNext = true;
				break;
			case CJTests.Types.ENCODER_ECC_CON_INVERT:
				chronojumpWindowTestsEncoderEccConInvert();
				callNext = true;
				break;
			case CJTests.Types.ENCODER_RECALCULATE:
				chronojumpWindowTestsEncoderRecalculate();
				break;
			case CJTests.Types.ENCODER_SET_SAVE_REPS:
				chronojumpWindowTestsEncoderSetSaveReps();
				callNext = true;
				break;
			case CJTests.Types.ENCODER_SET_SAVE_REPS_BUCLE:
				chronojumpWindowTestsEncoderSetSaveRepsBucle();
				break;
		}

		if(! bucleContinues) {
			if(bucleCurrent == 2)
				bucle2ended = true;
			else if(bucleCurrent == 1)
				bucle1ended = true;
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
		sessionLoadWin.SelectRow(5); //0 is the first //TODO: hardcoded!!
		sessionLoadWin.Button_accept.Click();
		
		LogB.TestEnd("chronojumpWindowTestsLoadSession");
	}
			
	private bool chronojumpWindowTestsSelectPerson(int count)
	{
		LogB.TestStart("chronojumpWindowTestsSelectPerson");

		bool personExists = selectRowTreeView_persons(treeview_persons, treeview_persons_store, count);
		if(! personExists) {
			LogB.TestEnd("chronojumpWindowTestsSelectPerson_ended");
			return false;
		}
		
		LogB.TestEnd("chronojumpWindowTestsSelectPerson_continuing");
		return true;
	}

	private bool chronojumpWindowTestsEncoderLoadSignal(int count)
	{
		LogB.TestStart("chronojumpWindowTestsLoadSignal");

		ArrayList data = encoderLoadSignalData(); //selects signals of this person, this session, this encoderGI

		if(count >= data.Count) {
			LogB.TestEnd("chronojumpWindowTestsLoadSignal_ended");
			return false;
		}
		
		EncoderSQL es = (EncoderSQL) data[count]; //first is 0

		on_encoder_load_signal_clicked (Convert.ToInt32(es.uniqueID)); //opens load window with first selected

		genericWin.Button_accept.Click(); //this will call accepted

		LogB.TestEnd("chronojumpWindowTestsLoadSignal_continuing");
		return true;
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
			button_encoder_capture_curves_all.Click();
		else if(i == 2)
			button_encoder_capture_curves_best.Click();
		else if(i == 3)
			button_encoder_capture_curves_none.Click();
		else
			button_encoder_capture_curves_4top.Click();

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
