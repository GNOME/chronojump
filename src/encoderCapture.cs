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
 *  Copyright (C) 2004-2023   Xavier de Blas <xaviblas@gmail.com>
 */

using System;
using System.IO;
using System.IO.Ports;
using System.Collections.Generic; //List<T>
using Gtk;

public abstract class EncoderCapture
{
	// ---- public stuff ----
		
	//Contains curves captured to be analyzed by R
	public EncoderCaptureCurveArray Ecca;

	public int Countdown;
	
	//stored to be realtime displayed
	//this is unused if showOnlyBars (configChronojump.EncoderCaptureShowOnlyBars)
	public List<PointF> EncoderCapturePointsCairo;
	public List<PointF> EncoderCapturePointsInertialDiscCairo;
	public int PointsCaptured;
	public int PointsPainted;

	// ---- protected stuff ----
	protected bool cont;
	protected string eccon;

	protected int recordingTime;		//on !cont, capture time is defined previously
	protected int recordedTimeCont;	//on cont, capture time is not defined, and this value has the actual recorded time
	protected int byteReaded;

	protected static List<int> encoderReaded;	//data coming from encoder and converted
	protected static List<int> encoderReadedInertialDisc;	//data coming from encoder and converted

	private int TRIGGER_ON = 84; //'T' from TRIGGER_ON on encoder firmware
	private int TRIGGER_OFF = 116; //'t' from TRIGGER_OFF on encoder firmware

	//private BoolMsList boolMsList;
	private TriggerList triggerList;

	private int lastTriggerOn;
	
	/*
	 * sum: sum on byteReaded, it's the vertical position
	 * sumInertialDisc: on inertial this has the sum of the disc, while sum has the position of the body (always <= 0 (starting position))
	 * on inertial we need both
	 */
	protected int sum;
	protected int sumInertialDisc;

	protected int i;
	protected int msCount;
	protected EncoderCaptureCurve ecc;
		
	protected int directionChangePeriod;
	protected int directionChangeCount;
	protected int directionNow;
	protected int directionLastMSecond;
	protected int directionCompleted;
	protected int previousEnd;
	protected int lastNonZero;

	//this will be used to stop encoder automatically	
	protected int consecutiveZeros;
	protected int consecutiveZerosMax;
	protected bool automaticallyEndByTime;	
	//specific of some subclasses
	protected bool lastDirectionStoredIsUp;
	protected bool capturingFirstPhase;

	protected static SerialPort sp;
	private Random rand;
	public Gtk.Button FakeFinishByTime; //finish without pressing finish button. store fullScreenLastCapture variable
	protected bool finish;
	protected bool capturingInertialBG;
	protected bool encoderConfigurationIsInverted;
	protected bool showOnlyBars; //only false on inertia moment calculation

	//get the moment where we cross 0 first time on inertial calibrated
	//signal will be saved from here
	protected int inertialCalibratedFirstCross0Pos;
	protected bool inertialCalibrated;

	//Only for IMCalc: go and return is a period. Here we count semiperiods
	public int IMCalcOscillations;

	//capture is simulated (a signal file is readed)
	private bool simulated = false;
	//private int [] simulatedInts;
	//private int simulatedCount;

	
	// ---- private stuff ----
	private bool cancel;


	//if cont (continuous mode), then will not end when too much time passed before start
	public bool InitGlobal (int time, int timeEnd,
			bool cont, string eccon, string port,
			bool capturingInertialBG, bool encoderConfigurationIsInverted,
			bool showOnlyBars,
			bool simulated)
	{
		this.cont = cont;
		this.eccon = eccon;
		this.capturingInertialBG = capturingInertialBG;
		this.encoderConfigurationIsInverted = encoderConfigurationIsInverted;
		this.showOnlyBars = showOnlyBars;
		this.simulated = simulated;

		//---- a) open port -----
		if(simulated)
		{
			rand = new Random(40);
			SimulatedReset();
		}
		else if(! simulated && ! capturingInertialBG)
		{
			LogB.Debug("encoderCaptureCsharp start port:", port);
			sp = new SerialPort(port);
			sp.BaudRate = 115200;
			LogB.Information("sp created");

			try {
				sp.Open();
			} catch {
				LogB.Information("Error: Cannot open port");
				return false;
			}
			LogB.Information("sp opened");
		}
		
		//---- b) initialize variables ----
	
		Ecca = new EncoderCaptureCurveArray();

		Countdown = time;
		msCount = 0;	//used for visual feedback of remaining time	

		recordingTime = time * 1000;
		recordedTimeCont = 1; //not 0 to not have divide by zero problems

		encoderReaded = new List<int>();
		encoderReadedInertialDisc = new List<int>();

		if(! showOnlyBars)
		{
			EncoderCapturePointsCairo = new List<PointF>();
			EncoderCapturePointsInertialDiscCairo = new List<PointF>();
			PointsCaptured = 0;
			PointsPainted = 0;
		}

		sum = 0;
		
		i = -20; //delete first records because there's encoder bug
		
		/*
		 * calculate params with R explanation	
		 */

		/*               3
		 *              / \
		 *             /   B
		 *            /     \
		 * --1       /
		 *    \     /
		 *     \   A
		 *      \2/
		 *
		 * Record the signal, when arrive to A, then store the descending phase (1-2) and calculate params (power, ...)
		 * When arrive to B, then store the ascending phase (2-3)
		 */

		/*
		 * TODO:
		 * this has to be related to distance and not to time
		 * but we need to have an accurate distance calculation depending on encoderConfiguration (see encoder R files)
		 * and it will be much better to have ecc and con separately to manage better weightlifting (double phase/or not) exercises
		 */
		directionChangePeriod = 25; //how long (ms) to recognize as change direction. (from 2 to A in ms)
						//it's in ms and not in cm, because it's easier to calculate
		directionChangeCount = 0; //counter for this period
		directionNow = 1;		// +1 or -1
		directionLastMSecond = 1;	// +1 or -1 (direction on last millisecond)
		directionCompleted = -1;	// +1 or -1
		previousEnd = 0;
		lastNonZero = 0;
		
		//this will be used to stop encoder automatically (on !cont mode)
		//or to save this set and wait for the next on cont mode
		consecutiveZeros = -1;		
		consecutiveZerosMax = timeEnd * 1000;
		automaticallyEndByTime = (timeEnd >= 0);

		initSpecific();

		//prepare for receiving triggers from encoder
		triggerList = new TriggerList();
		Util.FileDelete(Util.GetEncoderTriggerFileName());

		cancel = false;
		finish = false;

		return true;
	}

	protected virtual void initSpecific()
	{
	}

	public virtual void InitCalibrated(int angleNow)
	{
		inertialCalibrated = false;
	}

	public bool Capture (string outputData1, EncoderRProcCapture encoderRProcCapture,
			bool compujump, Preferences.TriggerTypes cutByTriggers,
			double restClustersSeconds, bool playSoundsFromFile, bool cairoHorizontal)
	{
		/*
		 * removed at 1.7.0
		if(simulated) {
			bool success = initSimulated();
			if(! success)
				return false;
		}
		*/

		lastTriggerOn = 0;
		inertialCalibratedFirstCross0Pos = 0;
		int lastInertialPhase_i = 0;

		//only for cutByTriggers == Preferences.TriggerTypes.START_AT_FIRST_ON
		bool firstTriggerHappened = false;

		//playSoundsFromFile
		DateTime lastTriggeredSound = DateTime.MinValue;

		if(capturingInertialBG)
		{
			/*
			 * reset capture list. If not done here, list will grow at each set
			 * also this fixes the initial 0s after a set
			 */
			EncoderCaptureInertialBackgroundStatic.Initialize();
		}

		do {
			//1 read data
			try {
				byteReaded = readByte();
			} catch {
				if(! simulated) {
					LogB.Error("Maybe encoder cable is disconnected");
					cancel = true;
				}

				break;
			}

			//2 check if readed data is a trigger
			if(byteReaded == TRIGGER_ON)
			{
				if(playSoundsFromFile)
				{
					TimeSpan ts = DateTime.Now.Subtract(lastTriggeredSound);
					if(ts.TotalMilliseconds > 50)
					{
						Util.NextSongInList();
						lastTriggeredSound = DateTime.Now;
					}

					continue;
				}

				Trigger trigger = new Trigger(Trigger.Modes.ENCODER, i, true);
				if(triggerList.IsSpurious(trigger, TriggerList.Type3.ON, 50))
				{
					triggerList.RemoveLastOff();
					continue;
				}

				//TriggerTypes.START_AT_FIRST_ON starts capture at first trigger. So when this happens, reset capture
				if(cutByTriggers == Preferences.TriggerTypes.START_AT_FIRST_ON && ! firstTriggerHappened)
				{
					LogB.Information("Cleaning on capture");

					startCaptureFromHere();

					firstTriggerHappened = true;
					i = -1; //will be 0 on next loop start
					continue;
				}

				if(cutByTriggers != Preferences.TriggerTypes.NO_TRIGGERS)
				{
					ecc = new EncoderCaptureCurve(lastTriggerOn, i);
					lastTriggerOn = i;

					double [] curve = new double[ecc.endFrame - ecc.startFrame];
					//int mySum = 0;
					for(int k=0, j=ecc.startFrame; j < ecc.endFrame ; j ++) {
						curve[k] = encoderReaded[j];
						k ++;
						//mySum += encoderReaded[j];
					}
					//ecc.up = (mySum >= 0);
					ecc.up = true; //make all concentric for the swimming application
					LogB.Debug("curve stuff" + ecc.startFrame + ":" + ecc.endFrame + ":" + encoderReaded.Count);

					bool success = encoderRProcCapture.SendCurve(
							ecc.startFrame,
							UtilEncoder.CompressData(curve, 25)	//compressed
							);
					if(! success)
						cancel = true;

					Ecca.curvesAccepted ++;
					Ecca.ecc.Add(ecc);
					LogB.Information(ecc.ToString());
				}
				triggerList.Add(trigger);
				continue;
			}
			else if(byteReaded == TRIGGER_OFF)
			{
				if(! playSoundsFromFile)
				{
					Trigger trigger = new Trigger(Trigger.Modes.ENCODER, i, false);
					triggerList.Add(trigger);
				}

				continue;
			}

			//3 if is not trigger: convertByte
			byteReaded = convertByte(byteReaded);
			//LogB.Information(" byte: " + byteReaded);

			if (encoderConfigurationIsInverted)
				byteReaded *= -1;

			i = i+1;
			if(i >= 0) 
			{
				if(cont)
					recordedTimeCont ++;

				if(byteReaded == 0)
				{
					consecutiveZeros ++;

					//clean variables when we are on cont and long time elapsed
					if(cont && Ecca.curvesAccepted == 0 && consecutiveZeros >= consecutiveZerosMax)
					{
						LogB.Information("Cleaning on capture");

						//remove this time on existing trigger records
						triggerList.Substract(consecutiveZeros);

						startCaptureFromHere();

						i = -1; //will be 0 on next loop start
						continue;
					}
				}
				else
					consecutiveZeros = -1;

				//stop if n seconds of inactivity
				//but it has to be moved a little bit first, just to give time to the people
				//if(consecutiveZeros >= consecutiveZerosMax && sum > 0) #Not OK because sum maybe is 0: +1,+1,-1,-1
				//if(consecutiveZeros >= consecutiveZerosMax && ecca.ecc.Count > 0) #Not ok because when ecca is created, ecc.Count == 1
				/*
				 * process ends
				 * (
				 * -> when a curve has been found and then there are n seconds of inactivity, or
				 * -> when not in cont and a curve has not been found and then there are 2*n seconds of inactivity
				 * -> on inertial, if a curve has been found, and now passed double end time since last phase (to end when there is no capture but there is "activity" because cone is slowly rolling
				 * ) and if consecutiveZeros > restClustersSeconds * 1.500
				 *
				 * 1500 is conversion to milliseconds and * 1.5 to have enough time to move after clusters res
				 */
				if(
						automaticallyEndByTime &&
						(
						 (Ecca.curvesAccepted > 0 && consecutiveZeros >= consecutiveZerosMax) ||
						 (! cont && Ecca.curvesAccepted == 0 && consecutiveZeros >= (2* consecutiveZerosMax)) ||
						 (inertialCalibrated && Ecca.curvesAccepted > 0 && i - lastInertialPhase_i >= (2* consecutiveZerosMax))
						) &&
						(restClustersSeconds == 0 || consecutiveZeros > restClustersSeconds * 1500)
				  )
				{
					FakeFinishByTime.Click ();
					finish = true;
					LogB.Information("SHOULD FINISH");
				}


				//on inertialCalibrated set mark where 0 is crossed for the first time
				if(inertialCalibrated && inertialCalibratedFirstCross0Pos == 0)
				{
					if( ( sumInertialDisc <= 0 && sumInertialDisc + byteReaded > 0 ) ||
							( sumInertialDisc >= 0 && sumInertialDisc + byteReaded < 0 ) )
						inertialCalibratedFirstCross0Pos = i;
				}

				sumInertialDisc += byteReaded;

				encoderReadedInertialDisc.Add(byteReaded);

				//sum is the body, sumInertialDisc is the disc
				if(inertialCalibrated)
				{
					int sumOld = sum;
					sum = - Math.Abs(sumInertialDisc);
					byteReaded = sum - sumOld;
				} else
					sum += byteReaded;

				encoderReaded.Add(byteReaded);

				if(! showOnlyBars)
				{
					assignEncoderCapturePoints (cairoHorizontal);
					PointsCaptured = i;
				}

				// ---- prepare to send to R ----

				//if string goes up or down, store the direction
				//direction is only up or down
				if(byteReaded != 0)
					directionNow = (int) byteReaded / (int) Math.Abs(byteReaded); //1 (up) or -1 (down)

				//if we don't have changed the direction, store the last non-zero that we can find
				if(directionChangeCount == 0 && directionNow == directionLastMSecond) {
					//check which is the last non-zero value
					//this is suitable to find where starts the only-zeros previous to the change
					if(byteReaded != 0)
						lastNonZero = i;
				}

				bool sendCurveMaybe = false;

				//if it's different than the last direction, mark the start of change
				if(directionNow != directionLastMSecond) {
					directionLastMSecond = directionNow;
					directionChangeCount = 0;

				} 
				else if(directionNow != directionCompleted) {
					//we are in a different direction than the last completed

					//we cannot add byteReaded because then is difficult to come back n frames to know the max point
					//directionChangeCount += byteReaded
					directionChangeCount ++;

					if(directionChangeCount > directionChangePeriod)	//count >= than change_period
						sendCurveMaybe = true;
				}

				/*
				 * on inertialCalibrated don't send curve until 0 is crossed
				 * this ensures first stored phase will be ecc, that's what the rest of the program is expecting
				 * TODO: maybe this can be problematic with triggers maybe can be desinchronized, just move values
				 */
				if(inertialCalibrated && inertialCalibratedFirstCross0Pos == 0)
					sendCurveMaybe = false;

				//if cutByTriggers, triggers send the curve at the beginning of this method
				if(cutByTriggers != Preferences.TriggerTypes.NO_TRIGGERS)
					sendCurveMaybe = false;

				if(sendCurveMaybe)
				{
					//int startFrame = previousFrameChange - directionChangeCount;	//startFrame
					/*
					 * at startFrame we do the "-directionChangePeriod" because
					 * we want data a little bit earlier, because we want some zeros
					 * that will be removed by reduceCurveBySpeed
					 * if not done, then the data:
					 * 0 0 0 0 0 0 0 0 0 1
					 * will start at 10th digit (the 1)
					 * if done, then at speed will be like this:
					 * 0 0 0 0.01 0.04 0.06 0.07 0.08 0.09 1
					 * and will start at fourth digit
					 */

					//this is better, takes a lot of time before, and then reduceCurveBySpeed will cut it
					//but reduceCurveBySpeed is not implemented on inertial
					//TODO: implement it
					int startFrame = previousEnd;	//startFrame
					LogB.Debug("startFrame",startFrame.ToString());
					if(startFrame < 0)
						startFrame = 0;

					//on inertial start when crossing 0 first time
					if(inertialCalibrated && startFrame < inertialCalibratedFirstCross0Pos)
						startFrame = inertialCalibratedFirstCross0Pos;

					LogB.Information("TTTT - i," + i.ToString() +
						       	"; directionChangeCount: " + 
						       	directionChangeCount.ToString() + 
						       	"; lastNonZero: " +
						       	lastNonZero.ToString() +
							"; final: " + 
							((i - directionChangeCount + lastNonZero)/2).ToString());

					ecc = new EncoderCaptureCurve(
							startFrame,
							(i - directionChangeCount + lastNonZero)/2 	//endFrame
							//to find endFrame, first substract directionChangePeriod from i
							//then find the middle point between that and lastNonZero
							//this means that the end is in central point at displacements == 0
							);

					//since 1.5.0 secundary thread is capturing and sending data to R process
					//while main thread is reading data coming from R and updating GUI

					LogB.Debug("curve stuff" + ecc.startFrame + ":" + ecc.endFrame + ":" + encoderReaded.Count);
					if(ecc.endFrame - ecc.startFrame > 0 ) 
					{
						double [] curve = new double[ecc.endFrame - ecc.startFrame];
						int mySum = 0;
						for(int k=0, j=ecc.startFrame; j < ecc.endFrame ; j ++) {
							curve[k] = encoderReaded[j];
							k ++;
							mySum += encoderReaded[j];
						}
						ecc.up = (mySum >= 0);

						previousEnd = ecc.endFrame;

						//22-may-2015: This is done in R now

						//1) check heightCurve in a fast way first to discard curves soon
						//   only process curves with height >= min_height
						//2) if it's concentric, only take the concentric curves, 
						//   but if it's concentric and inertial: take both.
						//   
						//   When capturing on inertial, we have the first graph
						//   that will be converted to the second.
						//   we need the eccentric phase in order to detect the Ci2

						/*               
						 *             /\
						 *            /  \
						 *           /    \
						 *____      C1     \      ___
						 *    \    /        \    /
						 *     \  /          \  C2
						 *      \/            \/
						 *
						 * C1, C2: two concentric phases
						 */

						/*               
						 *____                    ___
						 *    \    /\      /\    /
						 *     \ Ci1 \   Ci2 \ Ci3
						 *      \/    \  /    \/
						 *             \/
						 *
						 * Ci1, Ci2, Ci3: three concentric phases on inertial
						 *
						 * Since 1.6.1:
						 * on inertial curve is sent when rope is fully extended,
						 * this will allow to see at the moment c or e. Not wait the change of direction to see both
						 */


						//store in a boolean to not call shouldSendCurve() two times because it changes some variables
						bool shouldSendCurveBool = shouldSendCurve();
						if(shouldSendCurveBool)
						{
							//if compujump, wakeup screen if it's off
							//do it on the first repetition because it will not be sleeping on the rest of repetitions
							if(compujump && Ecca.curvesAccepted == 0)
								Networks.WakeUpRaspberryIfNeeded();

							bool success = encoderRProcCapture.SendCurve(
									ecc.startFrame,
									UtilEncoder.CompressData(curve, 25)	//compressed
									);
							if(! success)
								cancel = true;

							Ecca.curvesAccepted ++;
							Ecca.ecc.Add(ecc);
							LogB.Information(ecc.ToString());

							lastDirectionStoredIsUp = ecc.up;
						}

						if(inertialCalibrated)
							lastInertialPhase_i = i;
					}

					//on inertial is different
					markDirectionChanged();
				}

				//this is for visual feedback of remaining time	
				msCount ++;
				if(msCount >= 1000) {
					Countdown --;
					msCount = 1;
				}

			}
		} while ( (cont || i < (recordingTime -1)) && ! cancel && ! finish);
		
		LogB.Debug("runEncoderCaptureCsharp main bucle end");

		//leave some time to capture.R be able to paint data, and to create two Roptions.txt file correctly
		if(simulated)
			System.Threading.Thread.Sleep(2000);
		else if(! capturingInertialBG)
			sp.Close();

		if(cancel)
			return false;

		saveToFile(outputData1);

		LogB.Debug("runEncoderCaptureCsharp ended");

		return true;
	}

	/*
	 * removed at 1.7.0
	private bool initSimulated()
	{
		if(! File.Exists(Util.GetECapSimSignalFileName()))
			return false;

		string filename = Util.ReadFile(Util.GetECapSimSignalFileName(), true);
		simulatedInts = Util.ReadFileAsInts(filename);
		return true;
	}
	*/

	private void startCaptureFromHere()
	{
		consecutiveZeros = -1;
		encoderReadedInertialDisc = new List<int>();
		encoderReaded = new List<int>();

		if(capturingInertialBG)
		{
			//empty EncoderCaptureInertialBackgroundStatic.ListCaptured
			EncoderCaptureInertialBackgroundStatic.Initialize();
		}

		if(! showOnlyBars)
		{
			EncoderCapturePointsCairo = new List<PointF>();
			EncoderCapturePointsInertialDiscCairo = new List<PointF>();
			PointsCaptured = 0;
			PointsPainted = 0;
		}
	}

	private bool simulatedGoingUp = false;
	private int simulatedMaxValue = 400;
	private int simulatedLength;
	private int simulatedMaxLength = 4000; //when signal stops

	public void SimulatedReset()
	{
		simulatedLength = 0;
	}

	private int simulateByte()
	{
		System.Threading.Thread.Sleep(1);

		//return 0's to end if signal needs to end
		simulatedLength ++;
		if(simulatedLength > simulatedMaxLength)
			return 0;

		//get new value
		int simValue;
		if(simulatedGoingUp)
			simValue = rand.Next(0, 4);
		else
			simValue = rand.Next(-4, 0);

		//change direction if needed
		if(simulatedGoingUp && sum > simulatedMaxValue)
			simulatedGoingUp = false;
		else if(! simulatedGoingUp && sum < -1 * simulatedMaxValue)
			simulatedGoingUp = true;

		return simValue;
	}

	protected int readByte()
	{
		/*
		 * removed at 1.7.0
		if(simulated) {
			return simulatedInts[simulatedCount ++];
		} else {
		*/
			if(capturingInertialBG)
				return (int) EncoderCaptureInertialBackgroundStatic.GetNext();
			else {
				if(simulated)
					return simulateByte();
				else
					return sp.ReadByte();
			}
		//}
	}
	protected int convertByte(int b)
	{
		if(simulated) {
			if(b >= 48)
				b -= 48;
			else if(b <= -48)
				b += 48;
		} else {
			if(b > 128)
				b = b - 256;
		}
		return b;
	}


	//on inertial also assigns to EncoderCapturePointsInertialDisc
	protected virtual void assignEncoderCapturePoints (bool cairoHorizontal)
	{
		if (cairoHorizontal)
			EncoderCapturePointsCairo.Add (new PointF (i, sum));
		else
			EncoderCapturePointsCairo.Add (new PointF (sum, i));
	}
				
	/*
	 * TODO: 
	 * que el -25 no sigui un -25 sino que depengui del que l' usuari tingui seleccionat i la config del encoder. caldria posar lo de espai de les encoderConfigs de util.R aqui
	 */


	// on IMCalc we don't need to send data to R and get curves we will call R at the end
	protected virtual bool shouldSendCurve() {
		/*
		 * 2) if it's concentric, only take the concentric curves, 
		 * 3) if it's ecc-con, don't record first curve if first curve is concentric
		 * 4) on ec, ecS don't store two curves in the same direction
		*/

		if( eccon == "c" && ! ecc.up )	//2
			return false;
		if( (eccon == "ec" || eccon == "ecS") && ecc.up && capturingFirstPhase ) { //3
			capturingFirstPhase = false;
			return false;
		}
		if( 
				(eccon == "ec" || eccon == "ecS") && 
				Ecca.curvesAccepted > 0 &&
				lastDirectionStoredIsUp == ecc.up ) //4
			return false;

		return true;
	}
				
	protected virtual void markDirectionChanged() 
	{
		directionChangeCount = 0;
		directionCompleted = directionNow;
	}
	
	protected List<int> trimInitialZeros(List<int> l) 
	{
		int count = 0; //position of the first non-zero value
	
		//1. find when there's the first non-zero	
		foreach(int k in l) {
			if(k != 0)
				break;

			count ++;
		}
		
		//number of allowed milliseconds with zero values at start (will be cut by reduceCurveBySpeed)
		int allowedZeroMSAtStart = 1000;

		if(count > allowedZeroMSAtStart)
		{
			l.RemoveRange(0, count - allowedZeroMSAtStart);
			triggerList.Substract(count - allowedZeroMSAtStart);
		} // else: not enough zeros at start, don't need to trim 

		return l; 
	}
	
	//on inertial recordes encoderReadedInertialDisc	
	protected virtual void saveToFile(string file)
	{
		TextWriter writer = File.CreateText(file);

		encoderReaded = trimInitialZeros(encoderReaded);

		string sep = "";
		foreach(int k in encoderReaded)
		{
			if (encoderConfigurationIsInverted)
				writer.Write (sep + (-1 * k)); //store the raw file (before encoderConfigurationConversions)
			else
				writer.Write(sep + k); //store the raw file (before encoderConfigurationConversions)

			sep = ", ";
		}

		writer.Flush();
		writer.Close();
		((IDisposable)writer).Dispose();
	}

	/*
	 * used to playVideo
	 * we need to read Constants.EncoderDataTemp
	 * and put as eCapture variables
	 * in order to be able to graph it while play
	 */
	public void LoadFromFile (bool inertial, bool cairoHorizontal)
	{
		int [] encData = Util.ReadFileAsInts (UtilEncoder.GetEncoderDataTempFileName ());

		EncoderCapturePointsCairo = new List<PointF>();
		EncoderCapturePointsInertialDiscCairo = new List<PointF>();
		PointsCaptured = encData.Length;
		PointsPainted = 0;

		for (i = 0; i < encData.Length; i ++)
		{
			int b;
			if (encData[i] >= 0)
				b = encData[i] -48; //48 is 0 in ASCII
			else 	// -49 -> 49-48 -> 1 -> -1
				b = -1 * (Math.Abs (encData[i]) - 48);

			if (inertial)
			{
				sumInertialDisc += b;
				//int sumOld = sum;
				sum = - Math.Abs (sumInertialDisc);
				//byteReaded = sum - sumOld;
			} else {
				sum += b;
			}

			assignEncoderCapturePoints (cairoHorizontal);
		}
	}

	public void SaveTriggers(int signalID)
	{
		triggerList.SQLInsert(signalID);
	}

	public TriggerList GetTriggers()
	{
		return triggerList;
	}

	/*
	 * graph.R will findCurvesByTriggers if (length(op$TriggersOn) >= 1)
	 * else will findCurvesNew (like if not capturing by triggers)
	 * We need to know what graph.R will do to show a message to user
	 */
	public bool MinimumOneTriggersOn()
	{
		return triggerList.MinimumOneOn();
	}

	public string Eccon {
		get { return eccon; }
	}

	public int DirectionCompleted {
		get { return directionCompleted; }
	}

	public void Cancel() {
		cancel = true;
	}
	
	public void Finish() {
		finish = true;
	}

	//used on inertialIM
	public int Sum {
		get { return sum; }
	}

}


public class EncoderCaptureGravitatory : EncoderCapture
{
	public EncoderCaptureGravitatory() 
	{
		FakeFinishByTime = new Gtk.Button ();
	}

	protected override void initSpecific()
	{
		//useful to don't send to R the first phase of the set in these situations: 
		//going up in ec, ecs
		capturingFirstPhase = true; 
		
		//just a default value, unused until a curve has been accepted
		lastDirectionStoredIsUp = true;
	}
}


public class EncoderCaptureInertial : EncoderCapture
{
	public EncoderCaptureInertial() 
	{
		FakeFinishByTime = new Gtk.Button ();
	}

	protected override void initSpecific()
	{
	}

	public override void InitCalibrated(int angleNow)
	{
		inertialCalibrated = true;
		sum = angleNow;
		if(sum > 0)
			sum *= -1;

		sumInertialDisc = angleNow;
	}

	protected override void assignEncoderCapturePoints (bool cairoHorizontal)
	{
		if (cairoHorizontal) {
			EncoderCapturePointsCairo.Add (new PointF (i, sum));
			EncoderCapturePointsInertialDiscCairo.Add (new PointF (i, sumInertialDisc));
		} else {
			EncoderCapturePointsCairo.Add (new PointF (sum, i));
			EncoderCapturePointsInertialDiscCairo.Add (new PointF (sumInertialDisc, i));
		}
	}
	
	protected override void markDirectionChanged() 
	{
		directionChangeCount = 0;
		directionCompleted = directionNow;
	}
	
	protected override void saveToFile(string file)
	{
		TextWriter writer = File.CreateText(file);

		//on inertialCalibrated remove from the beginnig to the moment where 0 is crossed
		if(inertialCalibrated && inertialCalibratedFirstCross0Pos != 0 &&
				encoderReadedInertialDisc.Count > inertialCalibratedFirstCross0Pos)
			encoderReadedInertialDisc.RemoveRange(0, inertialCalibratedFirstCross0Pos);
		else
			encoderReadedInertialDisc = trimInitialZeros(encoderReadedInertialDisc);

		LogB.Information("Saving to disk");
		string sep = "";
		foreach(int k in encoderReadedInertialDisc) {
			writer.Write(sep + k); //store the raw file (before encoderConfigurationConversions)
			//LogB.Information(sep + k);
			sep = ", ";
		}

		writer.Flush();
		writer.Close();
		((IDisposable)writer).Dispose();
	}
	
}


public class EncoderCaptureIMCalc : EncoderCapture
{
	public static int InactivityEndTime = 3; //end at 3 segons of inactivity

	public EncoderCaptureIMCalc() 
	{
		FakeFinishByTime = new Gtk.Button (); //just in case
	}

	protected override void initSpecific()
	{
		IMCalcOscillations = 0;
	}
	
	// on IMCalc we don't need to send data to R and get curves we will call R at the end
	protected override bool shouldSendCurve()
	{
		IMCalcOscillations ++;
		return false;
	}
}

/*
public class BoolMsList
{
	private List<BoolMs> l;
	public BoolMsList()
	{
		l = new List<BoolMs>();
	}

	public void Add(bool b, int ms)
	{
		l.Add(new BoolMs(b, ms));
	}

	public void Substract(int msToSubstract)
	{
		foreach(BoolMs boolMs in l)
			boolMs.Substract(msToSubstract);
	}

	//just to debug
	public void Print()
	{
		LogB.Information("Printing BoolMSList");
		foreach(BoolMs boolMs in l)
			LogB.Information(boolMs.ToString());
	}

	public void Write()
	{
		//save triggers to file (if any)
		if(l == null || l.Count == 0)
			return;

		LogB.Debug("runEncoderCaptureCsharp saving triggers");
		TextWriter writer = File.CreateText(Util.GetEncoderTriggerDateTimeFileName());

		foreach(BoolMs boolMs in l)
			writer.WriteLine(boolMs.ToString());

		writer.Flush();
		writer.Close();
		((IDisposable)writer).Dispose();
	}
}

public class BoolMs
{
	private bool b;
	private int ms;

	public BoolMs(bool b, int ms)
	{
		this.b = b;
		this.ms = ms;
	}

	public void Substract(int msToSubstract)
	{
		ms -= msToSubstract;
	}

	public override string ToString()
	{
		return b.ToString() + ": " + ms.ToString();
	}
}
*/
