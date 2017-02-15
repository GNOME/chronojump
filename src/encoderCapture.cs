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
 *  Copyright (C) 2004-2017   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
using System.IO;
using System.IO.Ports;
using System.Collections.Generic; //List<T>

public abstract class EncoderCapture
{
	// ---- public stuff ----
		
	//Contains curves captured to be analyzed by R
	public EncoderCaptureCurveArray Ecca;

	public int Countdown;
	
	//stored to be realtime displayed
	public List<Gdk.Point> EncoderCapturePoints;
	public List<Gdk.Point> EncoderCapturePointsInertialDisc;
	public int EncoderCapturePointsCaptured;
	public int EncoderCapturePointsPainted;

	// ---- protected stuff ----
	protected int widthG;
	protected int heightG;
	protected bool cont;
	protected string eccon;

	protected double realHeightG;
	protected int recordingTime;		//on !cont, capture time is defined previously
	protected int recordedTimeCont;	//on cont, capture time is not defined, and this value has the actual recorded time
	protected int byteReaded;

	protected static List<int> encoderReaded;	//data coming from encoder and converted
	protected static List<int> encoderReadedInertialDisc;	//data coming from encoder and converted
	
	/*
	 * sum: sum ob byteReaded, it's the vertical position
	 * sumInertialDisc: on inertial this has the sum of the disc, while sum has the position of the body (always <= 0 (starting position))
	 * on inertial we need both
	 */
	protected double sum;	
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
		
	//specific of some subclasses
	protected bool inertialShouldCheckStartDirection;
	protected bool inertialCaptureDirectionInverted;
	protected bool lastDirectionStoredIsUp;
	protected bool capturingFirstPhase;

	protected static SerialPort sp;
	protected bool finish;
	protected bool capturingInertialBG;

	//get the moment where we cross 0 first time on inertial calibrated
	//signal will be saved from here
	protected int inertialCalibratedFirstCross0Pos;
	protected bool inertialCalibrated;

	//capture is simulated (a signal file is readed)
	private static bool simulated = false;
	private int [] simulatedInts;
	private int simulatedCount;

	
	// ---- private stuff ----
	private bool cancel;


	//if cont (continuous mode), then will not end when too much time passed before start
	public void InitGlobal (int widthG, int heightG, int time, int timeEnd, bool cont, string eccon, string port, bool capturingInertialBG)
	{
		this.widthG = widthG;
		this.heightG = heightG;
		this.cont = cont;
		this.eccon = eccon;
		this.capturingInertialBG = capturingInertialBG;
		
		//---- a) open port -----
		if(! simulated && ! capturingInertialBG) {
			LogB.Debug("runEncoderCaptureCsharp start port:", port);
			sp = new SerialPort(port);
			sp.BaudRate = 115200;
			LogB.Information("sp created");
			sp.Open();
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
		EncoderCapturePoints = new List<Gdk.Point>();
		EncoderCapturePointsInertialDisc = new List<Gdk.Point>();
		EncoderCapturePointsCaptured = 0;
		EncoderCapturePointsPainted = 0; 	//-1 means delete screen
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
	
		//only can be true on inertial capture subclass
		inertialShouldCheckStartDirection = false;
		inertialCaptureDirectionInverted = false;

		initSpecific();

		cancel = false;
		finish = false;
	}

	protected virtual void initSpecific()
	{
	}

	public virtual void InitCalibrated(int angleNow)
	{
		inertialCalibrated = false;
	}

	public bool Capture(string outputData1, EncoderRProcCapture encoderRProcCapture, bool compujump)
	{
		if(simulated) {
			bool success = initSimulated();
			if(! success)
				return false;
		}

		inertialCalibratedFirstCross0Pos = 0;

		LogB.Information("sum = " + sum.ToString());
		LogB.Information("sumInertialDisc = " + sumInertialDisc.ToString());
		do {
			try {
				byteReaded = readByte();
			} catch {
				if(! simulated) {
					LogB.Error("Maybe encoder cable is disconnected");
					cancel = true;
				}

				break;
			}

			byteReaded = convertByte(byteReaded);
			//LogB.Information(" byte: " + byteReaded);

			i = i+1;
			if(i >= 0) 
			{
				if(cont)
					recordedTimeCont ++;

				if(inertialCaptureDirectionInverted)
					byteReaded *= -1;

				if(byteReaded == 0)
					consecutiveZeros ++;
				else
					consecutiveZeros = -1;

				//stop if n seconds of inactivity
				//but it has to be moved a little bit first, just to give time to the people
				//if(consecutiveZeros >= consecutiveZerosMax && sum > 0) #Not OK because sum maybe is 0: +1,+1,-1,-1
				//if(consecutiveZeros >= consecutiveZerosMax && ecca.ecc.Count > 0) #Not ok because when ecca is created, ecc.Count == 1
				//
				//process ends 
				//when a curve has been found and then there are n seconds of inactivity, or
				//when a curve has not been found and then there are 2*n seconds of inactivity
				if(
						(Ecca.curvesAccepted > 0 && consecutiveZeros >= consecutiveZerosMax) ||
						(! cont && Ecca.curvesAccepted == 0 && consecutiveZeros >= (2* consecutiveZerosMax)) )
				{
					finish = true;
					LogB.Information("SHOULD FINISH");
				}
				

				//on inertialCalibrated set mark where 0 is crossed for the first time
				if(inertialCalibrated && inertialCalibratedFirstCross0Pos == 0)
				{
					if(byteReaded > 0 && sumInertialDisc < 0 && sumInertialDisc + byteReaded >= 0)
						inertialCalibratedFirstCross0Pos = i;
					else if(byteReaded < 0 && sumInertialDisc > 0 && sumInertialDisc + byteReaded <= 0)
						inertialCalibratedFirstCross0Pos = i;
				}

				sumInertialDisc += byteReaded;
				encoderReadedInertialDisc.Add(byteReaded);

				if(inertialChangedConToEcc())
					byteReaded *= -1;

				sum += byteReaded;
				encoderReaded.Add(byteReaded);

				assignEncoderCapturePoints();
				
				EncoderCapturePointsCaptured = i;

				//this only applies to inertial subclass
				if(inertialShouldCheckStartDirection)
					inertialCheckIfInverted();

				encoderCapturePointsAdaptativeDisplay();

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

		LogB.Information("sum = " + sum.ToString());
		LogB.Information("sumInertialDisc = " + sumInertialDisc.ToString());

				} 
				else if(directionNow != directionCompleted) {
					//we are in a different direction than the last completed

					//we cannot add byteReaded because then is difficult to come back n frames to know the max point
					//directionChangeCount += byteReaded
					directionChangeCount ++;

					if(directionChangeCount > directionChangePeriod)	//count >= than change_period
						sendCurveMaybe = true;
				}

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


						if( shouldSendCurve() )
						{
							//if compujump, wakeup screen if it's off
							//do it on the first repetition because it will not be sleeping on the rest of repetitions
							if(compujump && Ecca.curvesAccepted == 0)
								Networks.WakeUpRaspberryIfNeeded();

							encoderRProcCapture.SendCurve(
									UtilEncoder.CompressData(curve, 25)	//compressed
									);

							Ecca.curvesAccepted ++;
							Ecca.ecc.Add(ecc);

							lastDirectionStoredIsUp = ecc.up;
						}
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

	private bool initSimulated()
	{
		if(! File.Exists(UtilAll.GetECapSimSignalFileName()))
			return false;

		string filename = Util.ReadFile(UtilAll.GetECapSimSignalFileName(), true);
		simulatedInts = Util.ReadFileAsInts(filename);
		return true;
	}


	protected int readByte()
	{
		if(simulated) {
			return simulatedInts[simulatedCount ++];
		} else {
			if(capturingInertialBG)
				return EncoderCaptureInertialBackgroundStatic.GetNext();
			else
				return sp.ReadByte();
		}
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
	protected virtual void assignEncoderCapturePoints() 
	{
		int xWidth = recordingTime;
		if(cont)
			xWidth = recordedTimeCont;

		EncoderCapturePoints.Add(new Gdk.Point(
				Convert.ToInt32(widthG * i / xWidth),
				Convert.ToInt32( (heightG/2) - ( sum * heightG / realHeightG) )
				));
	}
				
	//on inertial also uses to EncoderCapturePointsInertialDisc
	protected virtual void encoderCapturePointsAdaptativeDisplay()
	{
		//adaptative displayed height
		//if points go outside the graph, duplicate size of graph
		if(EncoderCapturePoints[i].Y > heightG || EncoderCapturePoints[i].Y < 0) 
		{
			realHeightG *= 2;

			int xWidth = recordingTime;
			if(cont)
				xWidth = recordedTimeCont;

			double sum2=0;
			for(int j=0; j <= i; j ++) {
				sum2 += encoderReaded[j];
				EncoderCapturePoints[j] = new Gdk.Point(
						Convert.ToInt32(widthG * j / xWidth),
						Convert.ToInt32( (heightG/2) - ( sum2 * heightG / realHeightG) )
						);
			}
			EncoderCapturePointsCaptured = i;
			EncoderCapturePointsPainted = -1; //mark meaning screen should be erased
		}
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
			l.RemoveRange(0, count-allowedZeroMSAtStart);
		} // else: not enough zeros at start, don't need to trim 
		
		return l; 
	}
	
	//on inertial recordes encoderReadedInertialDisc	
	protected virtual void saveToFile(string file)
	{
		TextWriter writer = File.CreateText(file);

		encoderReaded = trimInitialZeros(encoderReaded);

		string sep = "";
		foreach(int k in encoderReaded) {
			writer.Write(sep + k); //store the raw file (before encoderConfigurationConversions)
			sep = ", ";
		}

		writer.Flush();
		writer.Close();
		((IDisposable)writer).Dispose();
	}
	
	//this methods only applies to inertial subclass
	protected virtual void inertialCheckIfInverted() {
	}
	protected virtual bool inertialChangedConToEcc() {
		return false;
	}

	public void Cancel() {
		cancel = true;
	}
	
	public void Finish() {
		finish = true;
	}
	
}


public class EncoderCaptureGravitatory : EncoderCapture
{
	public EncoderCaptureGravitatory() 
	{
	}

	protected override void initSpecific()
	{
		realHeightG = 2 * 1000 ; //1 meter up / 1 meter down
		
		//useful to don't send to R the first phase of the set in these situations: 
		//going up in ec, ecs
		capturingFirstPhase = true; 
		
		//just a default value, unused until a curve has been accepted
		lastDirectionStoredIsUp = true;
	}
}


public class EncoderCaptureInertial : EncoderCapture
{
	private bool inertialFirstEccPhaseDone;
		
	public EncoderCaptureInertial() 
	{
	}

	protected override void initSpecific()
	{
		realHeightG = 2 * 5000 ; //5 meters up / 5 meters down

		inertialShouldCheckStartDirection = true;

		inertialFirstEccPhaseDone = false;
	}

	public override void InitCalibrated(int angleNow)
	{
		inertialCalibrated = true;
		sum = angleNow;
		sumInertialDisc = angleNow;

		if(inertialShouldCheckStartDirection)
		{
			inertialCheckIfInverted();
		}
	}

	protected override void inertialCheckIfInverted() 
	{
		/*
		 * 1) on inertial, when we start we should go down (because exercise starts in full extension)
		 * if at the beginning we detect movement as positive means that the encoder is connected backwards or
		 * the disc is in a position that makes the start in that direction
		 * we use the '20' to detect when 'some' movement has been done
		 * Just -1 all the past and future values of this capture
		 * (we use the 'sum > 0' to know that it's going upwards)
		 *
		 * 2) here sum == sumInertialDisc and encoderReaded == encoderReadedInertialDisc
		 * because this variables change later (when sum < -25 )
		 */
		if(Math.Abs(sum) > 20) 
		{
			inertialShouldCheckStartDirection = false;

			if(sum > 0) {
				inertialCaptureDirectionInverted = true;
				byteReaded *= -1;
				directionNow *= -1;
				directionLastMSecond *= -1;
				sum *= -1;
				sumInertialDisc *= -1;
			
				int xWidth = recordingTime;
				if(cont)
					xWidth = recordedTimeCont;

				for(int j=0; j <= i; j ++) {
					encoderReaded[j] *= -1;
					encoderReadedInertialDisc[j] *= -1;
				}
				double sum2=0;
				for(int j=0; j <= i; j ++) {
					sum2 += encoderReaded[j];
					EncoderCapturePoints[j] = new Gdk.Point(
							Convert.ToInt32(widthG * j / xWidth),
							Convert.ToInt32( (heightG/2) - ( sum2 * heightG / realHeightG) )
							);
					//same for InertialDisc. Read comment 2 on the top of this method
					EncoderCapturePointsInertialDisc[j] = new Gdk.Point(
							Convert.ToInt32(widthG * j / xWidth),
							Convert.ToInt32( (heightG/2) - ( sum2 * heightG / realHeightG) )
							);
				}
				EncoderCapturePointsCaptured = i;
				EncoderCapturePointsPainted = -1; //mark meaning screen should be erased
			}
		}
	}
	
	protected override bool inertialChangedConToEcc() 
	{
		if(byteReaded == 0)
			return false;

		if(inertialFirstEccPhaseDone && sumInertialDisc > 0)
			return true;

		return false;
	}
	
	protected override void assignEncoderCapturePoints() 
	{
		int xWidth = recordingTime;
		if(cont)
			xWidth = recordedTimeCont;

		EncoderCapturePoints.Add(new Gdk.Point(
				Convert.ToInt32(widthG * i / xWidth),
				Convert.ToInt32( (heightG/2) - ( sum * heightG / realHeightG) )
				));
		EncoderCapturePointsInertialDisc.Add(new Gdk.Point(
				Convert.ToInt32(widthG * i / xWidth),
				Convert.ToInt32( (heightG/2) - ( sumInertialDisc * heightG / realHeightG) )
				));
	}
	
	protected override void encoderCapturePointsAdaptativeDisplay()
	{
		//adaptative displayed height
		//if points go outside the graph, duplicate size of graph
		if(
				EncoderCapturePoints[i].Y > heightG || EncoderCapturePoints[i].Y < 0 ||
				EncoderCapturePointsInertialDisc[i].Y > heightG || 
				EncoderCapturePointsInertialDisc[i].Y < 0 ) {
			realHeightG *= 2;

			int xWidth = recordingTime;
			if(cont)
				xWidth = recordedTimeCont;

			double sum2 = 0;
			double sum2InertialDisc = 0;
			for(int j=0; j <= i; j ++) {
				sum2 += encoderReaded[j];
				sum2InertialDisc += encoderReadedInertialDisc[j];
				EncoderCapturePoints[j] = new Gdk.Point(
						Convert.ToInt32(widthG * j / xWidth),
						Convert.ToInt32( (heightG/2) - ( sum2 * heightG / realHeightG) )
						);
				EncoderCapturePointsInertialDisc[j] = new Gdk.Point(
						Convert.ToInt32(widthG * j / xWidth),
						Convert.ToInt32( (heightG/2) - ( sum2InertialDisc * heightG / realHeightG) )
						);
			}
			EncoderCapturePointsCaptured = i;
			EncoderCapturePointsPainted = -1; //mark meaning screen should be erased and start painting from the beginning
		}
	}
	

	protected override void markDirectionChanged() 
	{
		//If min height is very low, a micromovement will be used to cut phases incorrectly
		//put a safe value like <25 . is below (more negative)  the 20 used on inertialCheckIfInverted
		if(! inertialFirstEccPhaseDone && sum < 25)
			inertialFirstEccPhaseDone = true;

		directionChangeCount = 0;
		directionCompleted = directionNow;
	}
	
	protected override void saveToFile(string file)
	{
		TextWriter writer = File.CreateText(file);

		//on inertialCalibrated remove from the beginnig to the moment where 0 is crossed
		if(inertialCalibrated && inertialCalibratedFirstCross0Pos != 0)
			encoderReadedInertialDisc.RemoveRange(0, inertialCalibratedFirstCross0Pos);
		else
			encoderReadedInertialDisc = trimInitialZeros(encoderReadedInertialDisc);

		string sep = "";
		foreach(int k in encoderReadedInertialDisc) {
			writer.Write(sep + k); //store the raw file (before encoderConfigurationConversions)
			sep = ", ";
		}

		writer.Flush();
		writer.Close();
		((IDisposable)writer).Dispose();
	}
	
}


public class EncoderCaptureIMCalc : EncoderCapture
{
	public EncoderCaptureIMCalc() 
	{
	}

	protected override void initSpecific()
	{
		realHeightG = 2 * 500 ; //.5 meter up / .5 meter down
	}
	
	// on IMCalc we don't need to send data to R and get curves we will call R at the end
	protected override bool shouldSendCurve() {
		return false;
	}
	
}
