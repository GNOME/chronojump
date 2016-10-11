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
 * separate in various classes (done!), files
 * progressBar
 * button to end
 * summary
 */

public class CJTests 
{
	private List<Types> sequence;
	private int sequencePos;
	private int bucleCurrent;
	private int bucle1count;
	private int bucle2count;
	private int bucle1startPos;
	private int bucle2startPos;
	private bool bucle1ended;
	private bool bucle2ended;
	
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

	/*	
	public static List<Types> SequenceEncoder1 = new List<Types> 
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
		//CJTests.Types.ENCODER_SET_SAVE_REPS,
		//CJTests.Types.ENCODER_SET_SAVE_REPS,
		//CJTests.Types.ENCODER_SET_SAVE_REPS,
		//CJTests.Types.ENCODER_SET_SAVE_REPS,
		CJTests.Types.ENCODER_SET_SAVE_REPS_BUCLE
	};
	*/
	
	private List<Types> sequenceEncoder2 = new List<Types> 
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
	
	public CJTests()
	{
		sequence = sequenceEncoder2;
		sequencePos = 0;
		bucleCurrent = 0;
	}

	public bool Next() 
	{
		sequencePos ++;

		if(sequence[sequencePos] == Types.BUCLE_1_ON) 
		{
			bucleCurrent = 1;
			bucle1ended = false;
			bucle1count = 0;
			sequencePos ++;
			bucle1startPos = sequencePos;
		} 
		
		if(sequence[sequencePos] == Types.BUCLE_2_ON) 
		{
			bucleCurrent = 2;
			bucle2ended = false;
			bucle2count = 0;
			sequencePos ++;
			bucle2startPos = sequencePos;
		}
		
		if(sequence[sequencePos] == Types.BUCLE_2_OFF) 
		{
			if(bucle2ended) {
				bucleCurrent --;
				sequencePos ++;
			} else {
				bucle2count ++;
				sequencePos = bucle2startPos;
			}
		} 
		
		if(sequence[sequencePos] == Types.BUCLE_1_OFF) 
		{
			if(bucle1ended) {
				bucleCurrent --;
				sequencePos ++;
			}
			else {
				bucle1count ++;
				sequencePos = bucle1startPos;
			}
		} 
		
		if(sequence[sequencePos] == CJTests.Types.END)
			return false;

		return true;
	}
	
	public Types GetSequencePos()
	{
		return sequence[sequencePos];
	}

	//get the iterating value on current bucle	
	public int GetBucleCount() 
	{
		if(bucleCurrent == 1)
			return bucle1count;
		else if(bucleCurrent == 2)
			return bucle2count;

		return 0;
	}

	public void EndBucleCurrent() 
	{
		if(bucleCurrent == 2)
			bucle2ended = true;
		else if(bucleCurrent == 1)
			bucle1ended = true;
	}

}

public partial class ChronoJumpWindow 
{
	private static bool testsActive = false;
	private CJTests cjTest;
	private int sessionID;
	
	private void chronojumpWindowTestsStart(int sessionID) 
	{
		testsActive = true;
		this.sessionID = sessionID;

		cjTest = new CJTests();

		chronojumpWindowTestsDo();
	}
	
	public void chronojumpWindowTestsNext() 
	{
		if(! testsActive) 
			return;

		if(cjTest.Next())
			chronojumpWindowTestsDo();
		else
			testsActive = false;
	}

	private void chronojumpWindowTestsDo() 
	{
		//if process is very fast (no threads, no GUI problems) just call next from this method
		bool callNext = false;
		
		bool bucleContinues = true;
		int bcount = cjTest.GetBucleCount();

		switch(cjTest.GetSequencePos()) 
		{
			case CJTests.Types.MODE_POWERGRAVITATORY:
				chronojumpWindowTestsMode(Constants.Menuitem_modes.POWERGRAVITATORY);
				break;
			case CJTests.Types.MODE_POWERINERTIAL:
				chronojumpWindowTestsMode(Constants.Menuitem_modes.POWERINERTIAL);
				break;
			case CJTests.Types.SESSION_LOAD:
				chronojumpWindowTestsLoadSession(sessionID); //this also selects first person
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

		if(! bucleContinues)
			cjTest.EndBucleCurrent();

		if(callNext)		
			chronojumpWindowTestsNext();
	}


	/*
	 * TESTS START
	 */

	private void chronojumpWindowTestsMode(Constants.Menuitem_modes m) 
	{
		LogB.TestStart("chronojumpWindowTestsMode", m.ToString());

		//disable autodetect
		//configAutodetectPort = Config.AutodetectPortEnum.INACTIVE;

		select_menuitem_mode_toggled(m);
		
		LogB.TestEnd("chronojumpWindowTestsMode");
	}

	private void chronojumpWindowTestsLoadSession(int sID)
	{
		LogB.TestStart("chronojumpWindowTestsLoadSession");

		on_open_activate(new Object(), new EventArgs());
		sessionLoadWin.SelectRow(sID); //0 is the first
		sessionLoadWin.Button_accept.Click();
		
		LogB.TestEnd("chronojumpWindowTestsLoadSession");
	}
			
	private bool chronojumpWindowTestsSelectPerson(int count)
	{
		LogB.TestStart("chronojumpWindowTestsSelectPerson");

		if(count > myTreeViewPersons.CountRows()) {
			LogB.TestEnd("chronojumpWindowTestsSelectPerson_ended");
			return false;
		}
			
		selectRowTreeView_persons(treeview_persons, treeview_persons_store, count);
		
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
	
	/*
	 * TESTS END
	 */


}
