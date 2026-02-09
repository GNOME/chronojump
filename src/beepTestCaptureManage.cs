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
 *  Copyright (C) 2026   Xavier de Blas <xaviblas@gmail.com>
 */

using System;
using Mono.Unix;
using System.Collections.Generic; //List<T>
using System.Diagnostics;  //Stopwatch

//definition of each stage on the test
public class BeepTestStage
{
	public string stageName;
	public double speedKmh;
	public double lapSeconds; //duration of each lap
	public int laps; //how many laps have each stage
	public int lapMeters; //distance of each lap

	public BeepTestStage (string stageName, double speedKmh, double lapSeconds, int laps, int lapMeters)
	{
		this.stageName = stageName;
		this.speedKmh = speedKmh;
		this.lapSeconds = lapSeconds;
		this.laps = laps;
		this.lapMeters = lapMeters;
	}
}

//manages BeepTestStage list
public class BeepTestStageManage
{
	private List<BeepTestStage> bts_l;

	//to store current status in the run
	public struct StageLapStatus
	{
		public int stage;
		public string stageName; //on YYI names are quite crazy
		public int lap;
		public int lapsOfThisStage;
		public double speedKmh;
		public bool resting; //rest or not rest
		public bool resting1stHalf; //1st half of the rest (to show walk to one side or to the other) (only applies if resting)

		public StageLapStatus (int stage, string stageName, int lap, int lapsOfThisStage, double speedKmh, bool resting, bool resting1stHalf)
		{
			this.stage = stage;
			this.stageName = stageName;
			this.lap = lap;
			this.lapsOfThisStage = lapsOfThisStage;
			this.speedKmh = speedKmh;
			this.resting = resting;
			this.resting1stHalf = resting1stHalf;
		}
	}

	//constructor
	public BeepTestStageManage ()
	{
		bts_l = new List<BeepTestStage> ();
	}

	public void CreateList (List<double> stageSpeedKm_l, List <double> lapDurationS_l, List<string> stageName_l, List<int> stageLaps_l, List<int> lapDistM_l)
	{
		for (int i = 0; i < lapDurationS_l.Count; i ++)
			bts_l.Add (new BeepTestStage (stageName_l[i], stageSpeedKm_l[i], lapDurationS_l[i], stageLaps_l[i], lapDistM_l[i]));
	}

	public StageLapStatus GetCurrentStageLapStatus (long currentMs, int restSeconds, out bool shouldFinish)
	{
		shouldFinish = false;
		double sumRaceMs = 0;
		for (int s = 0; s < bts_l.Count; s ++)
		{
			for (int t = 0; t < bts_l[s].laps; t ++)
			{
				sumRaceMs += 1000 * bts_l[s].lapSeconds;
				if (restSeconds > 0 && Util.IsEven (t+1))
					sumRaceMs += 1000 * restSeconds;

				if (currentMs < sumRaceMs)
					return new StageLapStatus (
							s, bts_l[s].stageName,
							t, bts_l[s].laps, bts_l[s].speedKmh,
							(restSeconds > 0 && Util.IsEven (t+1) && currentMs + 1000 * restSeconds >= sumRaceMs), // true if we are resting
							(restSeconds > 0 && Util.IsEven (t+1) && currentMs + 1000 * restSeconds/2 < sumRaceMs) // (if we are resting: true in the 1st part of the rest)
							);
			}
		}

		shouldFinish = true;
		return getLastLapStatus ();
	}

	protected StageLapStatus getLastLapStatus ()
	{
		return new StageLapStatus (
				bts_l.Count -1, bts_l[bts_l.Count -1].stageName,
				bts_l[bts_l.Count -1].laps -1,
				bts_l[bts_l.Count -1].laps,
				bts_l[bts_l.Count -1].speedKmh,
				false, false);
	}

	/*
	 * Unused!
	 * Note also it need to be fixed because on stages previous to current need to count all laps
	 *
	//Total meters on an stage, lap. Note on YoYo intermitent, it will be different
	public int GetTotalMeters (int stage, int lap)
	{
		int sum = 0;
		for (int s = 0; s <= stage; s ++)
			for (int t = 0; t <= lap; t ++)
				sum += bts_l[s].lapMeters;

		return sum;
	}
	*/

	//Note if an stage has 8 laps, then there are 4 possible "lap" values for the YYIR Vo2max detection
	public int GetTotalMetersByLapPairs (int stage, int lap)
	{
		int sum = 0;
		int lapPairAccumulated;

		for (int s = 0; s <= stage; s ++)
		{
			//at each stage: count all laps; except at current stage: go until lap or lap +1
			int untilLap = bts_l[s].laps;
			if (s == stage)
			{
				//iterate until this lap, but if it's even, need one more (to have the pair)
				untilLap = lap;
				if (Util.IsEven (lap))
					untilLap ++;
			}
			//LogB.Information (string.Format ("stage: {0}, lap: {1}, untilLap: {2}", stage, lap, untilLap));

			lapPairAccumulated = 0;
			for (int t = 0; t <= untilLap; t ++)
			{
				if (Util.IsEven (t))
					lapPairAccumulated = bts_l[s].lapMeters;
				else {
					sum += bts_l[s].lapMeters + lapPairAccumulated;
					lapPairAccumulated = 0;
				}
			}
		}

		return sum;
	}


	//gets at which millisecond starts an stage
	public int GetStageTimeStartInMs (int stage)
	{
		int sum = 0;
		for (int s = 0; s < stage -1; s ++)
			for (int t = 0; t < bts_l[s].laps; t ++)
				sum += Convert.ToInt32 (1000 * bts_l[s].lapSeconds);

		return sum;
	}
}

public class BeepTestWarningManage
{
	private List<Warning> warning_l;

	public struct Warning
	{
		public int personID;
		public string personName;
		public int stage;
		public int lap;

		public Warning (int personID, string personName, int stage, int lap)
		{
			this.personID = personID;
			this.personName = personName;
			this.stage = stage;
			this.lap = lap;
		}
	}

	//constructor
	public BeepTestWarningManage ()
	{
		warning_l = new List<Warning> ();
	}

	public void Add (int personID, string personName, int stage, int lap)
	{
		warning_l.Add (new Warning (personID, personName, stage, lap));
	}

	/* unused
	public string PrintPerson (int personID)
	{
		string str = "";
		foreach (Warning w in warning_l)
			if (w.personID == personID)
				str += string.Format ("\n {0,5} | {1,5}", w.stage +1, w.lap +1);

		if (str != "")
			str = " Stage |  Lap";

		return str;
	}
	*/

	public string PrintAll ()
	{
		string str = "";
		foreach (Warning w in warning_l)
			str += string.Format ("\n {0,5} | {1,5} | {2}", w.stage +1, w.lap +1, w.personName);

		if (str != "")
			str = " Stage |  Lap  | Name" +
				"\n" +
				" ----- | ----- | ---- " +
				str;

		return str;
	}

}

//to store current status in the run
//Attention: bad class name as it could be confused with Run, RunInterval, RunEncoder
public class RunnerStatus
{
	//note Nothing is not used at the moment
	public enum StatusEnum { Nothing, Running, Exempt, Finished }
        //Tr: translated
        public static string GetStatusEnumTr (StatusEnum se)
        {
                return Catalog.GetString (se.ToString ());
        }

	public int personID;
	public string personName;
	public StatusEnum statusEnum;

	public RunnerStatus (int personID, string personName, StatusEnum statusEnum)
	{
		this.personID = personID;
		this.personName = personName;
		this.statusEnum = statusEnum;
	}
}

//List of the runners and their status
public class BeepTestRunners
{
	private List<RunnerStatus> runnerStatus_l;

	//constructor
	public BeepTestRunners (List<Person> person_l)
	{
		runnerStatus_l = new List<RunnerStatus> ();
		foreach (Person p in person_l)
			runnerStatus_l.Add (new RunnerStatus (p.UniqueID, p.Name, RunnerStatus.StatusEnum.Running));
	}

	public bool IsRunning (int personID)
	{
		foreach (RunnerStatus rs in runnerStatus_l)
			if (rs.personID == personID)
				return (rs.statusEnum == RunnerStatus.StatusEnum.Running);

		return false;
	}

	public void Exempt (int personID)
	{
		foreach (RunnerStatus rs in runnerStatus_l)
			if (rs.personID == personID)
				rs.statusEnum = RunnerStatus.StatusEnum.Exempt;
	}

	public void Finished (int personID)
	{
		foreach (RunnerStatus rs in runnerStatus_l)
			if (rs.personID == personID)
				rs.statusEnum = RunnerStatus.StatusEnum.Finished;
	}

	//all that are currently running
	//return the list of the runners that we finish now, to send to treeview_persons
	public List<int> FinishAllRunners ()
	{
		List<int> finishedNow_l = new List<int> ();

		foreach (RunnerStatus rs in runnerStatus_l)
			if (rs.statusEnum == RunnerStatus.StatusEnum.Running)
			{
				rs.statusEnum = RunnerStatus.StatusEnum.Finished;
				finishedNow_l.Add (rs.personID);
			}

		return finishedNow_l;
	}

	public string GetName (int personID)
	{
		foreach (RunnerStatus rs in runnerStatus_l)
			if (rs.personID == personID)
				return rs.personName;

		return "";
	}

	public bool AnyoneRunning {
		get {
			foreach (RunnerStatus rs in runnerStatus_l)
				if (rs.statusEnum == RunnerStatus.StatusEnum.Running)
					return true;

			return false;
		}
	}

}


//tests creation and interaction with Chronojump events
//CM for Capture Manage
public abstract class BeepTestCM
{
	protected BeepTestStageManage btsm;
	protected DateTime dateIni;
	protected Stopwatch stopwatch;
	protected int exerciseID;
	protected bool finished;
	protected bool hasVo2max; //default false
	protected int startedWithMs = 0;
	protected int restSeconds = 0;
	protected int lapMeters = 20; //this works because these tests have all the laps with same meters, if any test has different meters, do not use this

	protected BeepTestStageManage.StageLapStatus currentStageLapStatus;
	protected BeepTestStageManage.StageLapStatus previousStageLapStatus; //to beep sound on lap changed

	public enum BeepNowEnum { NO, LAP, STAGE };
	protected BeepNowEnum shouldBeepNow;

	protected bool hasVoice;
	public enum VoiceNowEnum { NO, MID, STAGE }; //MID is middle of stage
	private DateTime lastBeepDt; //to play voice after a delay (not overlap two audios)

	protected VoiceNowEnum shouldVoiceNow; // -1 mean no
	protected bool has05PercentVoiceThisStage;
	protected bool has50PercentVoiceThisStage;

	private Gdk.Pixbuf imageLapStatus;
	protected BeepTestWarningManage warningManage;

	protected void initialize ()
	{
		btsm = new BeepTestStageManage ();
		btsm.CreateList (stageSpeedKm_l, lapDurationS_l, stageName_l, stageLaps_l, lapDistM_l);

		warningManage = new BeepTestWarningManage ();

		stopwatch = new Stopwatch ();
		previousStageLapStatus = new BeepTestStageManage.StageLapStatus (-1, "-1", -1, -1, -1, false, false);

		finished = false;
	}

	public static string Leger20Name = "Leger 20 m shuttle run";
	public static string Leger15Name = "Leger 15 m shuttle run";
	public static string YYIE1Name = "Yo Yo Intermitent Endurance 1";
	public static string YYIE2Name = "Yo Yo Intermitent Endurance 2";
	public static string YYIR1Name = "Yo Yo Intermitent Recovery 1";
	public static string YYIR2Name = "Yo Yo Intermitent Recovery 2";
	public static string CustomName = "Custom";
	//TODO: have a bool translated like in ForceSensorRFD.FunctionsArray
	public static string [] TypesArray ()
	{
		return new string [] {
			Leger20Name,
			Leger15Name,
			YYIE1Name,
			YYIE2Name,
			YYIR1Name,
			YYIR2Name,
			CustomName
		};
	}

	public static BeepTestCM Factory (string name, int leggerStartAt, bool leggerStart8kmh, int customDistM, double customSpeed, int customTotalLaps, double customIncrementalSpeed)
	{
		if (name == Leger20Name)
			return new BeepTestLeger20m (leggerStartAt, leggerStart8kmh);
		else if (name == Leger15Name)
			return new BeepTestLeger15m (leggerStartAt, leggerStart8kmh);
		else if (name == YYIE1Name)
			return new BeepTestYYIE1 ();
		else if (name == YYIE2Name)
			return new BeepTestYYIE2 ();
		else if (name == YYIR1Name)
			return new BeepTestYYIR1 ();
		else if (name == YYIR2Name)
			return new BeepTestYYIR2 ();
		else //if (name == CustomName)
			return new BeepTestCustom (customDistM, customSpeed, customTotalLaps, customIncrementalSpeed);

	}

	public void Start ()
	{
		dateIni = DateTime.Now;
		stopwatch.Start ();
	}

	public void Finish ()
	{
		finished = true;
	}

	public int GetCurrentSeconds ()
	{
		return Convert.ToInt32 (UtilAll.DivideSafe (stopwatch.ElapsedMilliseconds, 1000));
	}

	public BeepTestStageManage.StageLapStatus GetCurrentStageLapStatus ()
	{
		//update stageLapStatus
		currentStageLapStatus = btsm.GetCurrentStageLapStatus (
				stopwatch.ElapsedMilliseconds + startedWithMs,
				restSeconds, out bool shouldFinish);

		if (shouldFinish)
			finished = true;

		decideIfShouldBeep ();
		if (shouldBeepNow != BeepNowEnum.NO)
			lastBeepDt = DateTime.Now;

		if (hasVoice)
			decideIfShouldVoice ();
		imageLapStatus = getImageForLapStatus ();

		previousStageLapStatus = currentStageLapStatus;

		return currentStageLapStatus;
	}

	protected virtual void decideIfShouldBeep ()
	{
		shouldBeepNow = BeepNowEnum.NO;
		if (previousStageLapStatus.stage >= 0 && //double beep on stage not at start of the test
				previousStageLapStatus.stage != currentStageLapStatus.stage)
		{
				shouldBeepNow = BeepNowEnum.STAGE;
				has05PercentVoiceThisStage = false;
				has50PercentVoiceThisStage = false;
		}
		else if (previousStageLapStatus.lap != currentStageLapStatus.lap)
			shouldBeepNow = BeepNowEnum.LAP;
	}

	protected virtual void decideIfShouldVoice ()
	{
		shouldVoiceNow = VoiceNowEnum.NO;
		if (DateTime.Now.Subtract (lastBeepDt).TotalSeconds < 2)
			return;

		if (! has05PercentVoiceThisStage &&
				getCurrentPercentageOnStage (currentStageLapStatus.stage) > 05 &&
				currentStageLapStatus.stage >= 0)
		{
			shouldVoiceNow = VoiceNowEnum.STAGE;
			has05PercentVoiceThisStage = true;
		}
		else if (! has50PercentVoiceThisStage &&
				getCurrentPercentageOnStage (currentStageLapStatus.stage) > 50)
		{
			shouldVoiceNow = VoiceNowEnum.MID;
			has50PercentVoiceThisStage = true;
		}
	}

	protected virtual Gdk.Pixbuf getImageForLapStatus ()
	{
		if (! Util.IsEven (currentStageLapStatus.lap +1))
			return Chronojump.MyPixbuf.Get(null, Util.GetImagePath(false) + "run_2x.png");
		else {
			if (currentStageLapStatus.resting)
			{
				if (currentStageLapStatus.resting1stHalf)
					return Chronojump.MyPixbuf.Get(null, Util.GetImagePath(false) + "walk_back_48px.png");
				else
					return Chronojump.MyPixbuf.Get(null, Util.GetImagePath(false) + "walk_48px.png");
			} else
				return Chronojump.MyPixbuf.Get(null, Util.GetImagePath(false) + "run_back_2x.png");
		}
	}

	protected int getStageTimeStartInMs (int stage)
	{
		return btsm.GetStageTimeStartInMs (stage);
	}

	protected int getCurrentPercentageOnStage (int currentStage)
	{
		currentStage ++;
		int currentStageStartMs = getStageTimeStartInMs (currentStage);
		int nextStageStartMs = getStageTimeStartInMs (currentStage + 1);
		LogB.Information (string.Format ("currentMs: {0}, currentStageStartMs: {1}, nextStageStartMs: {2}, percent: {3}",
					stopwatch.ElapsedMilliseconds,
					currentStageStartMs,
					nextStageStartMs,
					Convert.ToInt32 (100 * UtilAll.DivideSafe (stopwatch.ElapsedMilliseconds -currentStageStartMs, nextStageStartMs - currentStageStartMs))
					));

		return Convert.ToInt32 (100 * UtilAll.DivideSafe (stopwatch.ElapsedMilliseconds -currentStageStartMs, nextStageStartMs - currentStageStartMs));
	}

	protected virtual List<double> stageSpeedKm_l
	{
		get { return (new List<double> ()); }
	}
	protected virtual List<string> stageName_l
	{
		get {
			List<string> l = new List<string> ();
			for (int i = 0; i < stageSpeedKm_l.Count ; i ++)
				l.Add ((i+1).ToString());
			return l;
		}
	}
	protected virtual List<int> stageLaps_l
	{
		get { return (new List<int> ()); }
	}

	protected List<int> lapDistM_l  //in m
	{
		get {
			List<int> l = new List<int> ();
			for (int i = 0; i < stageSpeedKm_l.Count ; i ++)
				l.Add (lapMeters);
			return l;
		}
	}

	protected List<double> lapDurationS_l
	//protected virtual List<double> lapDurationS_l
	{
		get {
			List<double> stageSec_l = new List<double> ();
			for (int i = 0; i < stageSpeedKm_l.Count; i ++)
				stageSec_l.Add (lapDistM_l[i] / (stageSpeedKm_l[i]/3.6)); // km/h -> m/s

			return stageSec_l;
		}
	}

	//isLastOne allows to play double pip of previous stage sound
	public virtual string GetSoundFileForStage (int stage, bool isLastOne)
	{
		if (stage >= 0 && stage <= 20)
		{
			if (! isLastOne)
				stage += 1;

			return System.IO.Path.Combine (Util.GetSoundsBeepDir(), string.Format ("BEEP{0}.mp3", stage));
		} else
			return Util.GetSound (Constants.SoundTypes.CAN_START);
	}

	//middle is for the .5, 1.5, ...
	public virtual string GetVoiceFile (int stage, bool middle)
	{
		if (stage >= 0 && stage <= 20)
		{
			string middleStr = "";
			if (middle)
				middleStr = ".5";

			return System.IO.Path.Combine (Util.GetSoundsBeepDir(), string.Format ("Voice{0}{1}.mp3", stage, middleStr));
		} else
			return Util.GetSound (Constants.SoundTypes.CAN_START);
	}

	public void WarningAdd (int personID, string personName)
	{
		warningManage.Add (personID, personName, currentStageLapStatus.stage, currentStageLapStatus.lap);
	}

	public string WarningPrintAll ()
	{
		return warningManage.PrintAll ();
	}

	public virtual double Vo2max ()
	{
		return -1;
	}
	public virtual double Vo2max (int stageLast, int lapLast)
	{
		return -1;
	}

	public string GetStageNameOfStage (int stage)
	{
		//LogB.Information (string.Format ("GetStageNameOfStage: {0}", stage));
		if (stage < stageName_l.Count)
		{
			//LogB.Information ("name  is: " + stageName_l[stage]);
			return stageName_l[stage];
		}

		return (stage+1).ToString (); //used on Custom
	}

	public BeepNowEnum ShouldBeepNow
	{
		get { return (shouldBeepNow); }
	}

	public VoiceNowEnum ShouldVoiceNow
	{
		get { return (shouldVoiceNow); }
	}

	public Gdk.Pixbuf ImageLapStatus
	{
		get { return (imageLapStatus); }
	}

	public int ExerciseID
	{
		get { return exerciseID; }
	}

	public bool Finished
	{
		get { return (finished); }
	}

	public bool HasVo2max
	{
		get { return (hasVo2max); }
	}

	public virtual string GetOptions
	{
		get { return ""; }
	}
}

public class BeepTestLeger20m : BeepTestCM
{
	private bool startFirstAt8Kmh;

	public BeepTestLeger20m (int startStage, bool startFirstAt8Kmh)
	{
		this.startFirstAt8Kmh = startFirstAt8Kmh;
		initialize ();
		exerciseID = 0;
		hasVo2max = true;
		hasVoice = true;

		if (startStage > 1)
			startedWithMs = getStageTimeStartInMs (startStage);
	}

	protected override List<double> stageSpeedKm_l
	{
		get {
			double firstSpeed = 8.5;
			if (startFirstAt8Kmh)
				firstSpeed = 8;

			return (new List<double> {
					firstSpeed, 9.0, 9.5, 10.0, 10.5, 11.0, 11.5, 12, 12.5, 13.0,
					13.5, 14.0, 14.5, 15.0, 15.5, 16.0, 16.5, 17.0, 17.5, 18.0, 18.5
					} );
		}
	}

	protected override List<int> stageLaps_l
	{
		get {
			return (new List<int> {
					7,  8,  8,  8,  9, 9, 10, 10, 10, 11,
					11, 12, 12, 13, 13, 13, 14, 14, 15, 15, 15
					} );
		}
	}
	//https://www.ncbi.nlm.nih.gov/pmc/articles/PMC1725157
	public override double Vo2max ()
	{
		return currentStageLapStatus.speedKmh * 6.55 - 35.8;
	}

	public override double Vo2max (int stageLast, int lapLast)
	{
		if (stageLast >= stageSpeedKm_l.Count)
			return -1;

		return stageSpeedKm_l[stageLast] * 6.55 - 35.8;
	}

	public override string GetOptions
	{
		get {
			if (startFirstAt8Kmh)
				return "Speed1stStage=8";
			else
				return "";
		}
	}
}

public class BeepTestLeger15m : BeepTestCM
{
	private bool startFirstAt8Kmh;

	public BeepTestLeger15m (int startStage, bool startFirstAt8Kmh)
	{
		this.startFirstAt8Kmh = startFirstAt8Kmh;

		lapMeters = 15;

		initialize ();
		exerciseID = 1;
		hasVoice = true;

		if (startStage > 1)
			startedWithMs = getStageTimeStartInMs (startStage);
	}

	protected override List<double> stageSpeedKm_l
	{
		get {
			double firstSpeed = 8.5;
			if (startFirstAt8Kmh)
				firstSpeed = 8;

			return (new List<double> {
					firstSpeed, 9.0, 9.5, 10.0, 10.5, 11.0, 11.5, 12, 12.5, 13.0,
					13.5, 14.0, 14.5, 15.0, 15.5, 16.0, 16.5, 17.0, 17.5, 18.0, 18.5
					} );
		}
	}

	protected override List<int> stageLaps_l
	{
		get {
			return (new List<int> {
					9, 10, 11, 11, 12, 12, 13, 13, 14, 14,
					15, 16, 16, 17, 17, 18, 18, 19, 19, 20, 21
					} );
		}
	}

	public override string GetOptions
	{
		get {
			if (startFirstAt8Kmh)
				return "Speed1stStage=8";
			else
				return "";
		}
	}
}

/*
 * https://footballscience.net/testing/aerobic-endurance/university-of-montreal-track-test/
 * http://www.alfisport.com/News/Tesis/Pruebas_rendimiento.html
 * https://salutpersonal.wordpress.com/wp-content/uploads/2014/02/test-lc3a9ger-boucher_150214.jpg
 * https://tfontanet.github.io/cinquieme/5-perimetres-aires-epi-vma.pdf
 * https://www.traileur.ch/estimation-vma-test-luc-leger-boucher
 * universitats/blanquerna/assignatures/ergonomia-assignatures/valoracio-funcional-ergonomia/docs/ergonomia/ergonomia-esportiva/calçat-cursa/curs-running-estiu-2016/Apunts/Leger, L & Boucher, R. (1980). An indirect continuous running multistage field test..pdf
 *
public class BeepTestMontreal : BeepTestCM
{
	public BeepTestMontreal ()
	{
		initialize ();
		hasVo2max = true;
	}

	protected override List<double> stageSpeedKm_l
	{
		get {
			return (new List<double> {
					6, 7.1, //first two walking, the others running
					7.16, 8.48, 9.76, 11, 12.21, 13.39,
					14.54, 15.66, 16.75, 17.83, 18.88,
					19.91, 20.91, 21.91, 22.88
					} );
		}
	}

	protected override List<int> stageLaps_l
	{
		get {
			return (new List<int> {
					} );
		}
	}

	//https://www.ncbi.nlm.nih.gov/pmc/articles/PMC1725157
	public override double Vo2max (double maxSpeed)
	{
		return maxSpeed * 6.55 - 35.8;
	}

}
*/

public class Pacer15m : BeepTestCM
{
	public Pacer15m ()
	{
		initialize ();
	}

	/* TODO: put correct values
	protected override List<double> speedKm_l
	{
		get {
			return (new List<double> {
					8.5, 9.0, 9.5, 10.0, 10.5, 11.0, 11.5, 12, 12.5, 13.0, 13.5,
					14.0, 14.5, 15.0, 15.5, 16.0, 16.5, 17.0, 17.5, 18.0, 18.5
					} );
		}
	}
	*/

	/*
	protected override List<int> stageMs_l
	{
		get {
			return (new List<int> {
					6750, 6000, 5684, 5400, 5143, 4909, 4696, 4500, 4320, 4154, 4000,
					3857, 3724, 3600, 3484, 3375, 3273, 3176, 3086, 3000, 2919
					} );
		}
	}
	*/

	protected override List<int> stageLaps_l
	{
		get {
			return (new List<int> {
					9, 10, 11, 12, 12, 13, 13, 14, 14, 15, 15,
					16, 17, 17, 18, 18, 19, 19, 20, 20, 21
					} );
		}
	}
}

public class Pacer20m : BeepTestCM
{
	public Pacer20m ()
	{
		initialize ();
	}

	/* TODO: put correct values
	protected override List<double> speedKm_l
	{
		get {
			return (new List<double> {
					8.5, 9.0, 9.5, 10.0, 10.5, 11.0, 11.5, 12, 12.5, 13.0, 13.5,
					14.0, 14.5, 15.0, 15.5, 16.0, 16.5, 17.0, 17.5, 18.0, 18.5
					} );
		}
	}
	*/

	/*
	protected override List<int> stageMs_l
	{
		get {
			return (new List<int> {
					9000, 8000, 7579, 7200, 6857, 6545, 6261, 6000, 5760, 5538, 5333,
					5143, 4966, 4800, 4645, 4500, 4364, 4235, 4114, 4000, 3892

					} );
		}
	}
	*/

	protected override List<int> stageLaps_l
	{
		get {
			return (new List<int> {
					7, 8, 8, 9, 9, 10, 10, 11, 11, 11, 12,
					12, 13, 13, 13, 14, 14, 15, 15, 16, 16
					} );
		}
	}
}

//YoYo Intermitent tests
//tables at: https://en.wikipedia.org/wiki/Yo-Yo_intermittent_test
public abstract class BeepTestYYI : BeepTestCM
{
	public BeepTestYYI ()
	{
		//restSeconds = 5;
	}

	protected override void decideIfShouldBeep ()
	{
		shouldBeepNow = BeepNowEnum.NO;

		if (previousStageLapStatus.stage >= 0 && //double beep on stage not at start of the test
				previousStageLapStatus.stage != currentStageLapStatus.stage)
				shouldBeepNow = BeepNowEnum.STAGE;
		else if (previousStageLapStatus.lap != currentStageLapStatus.lap)
			shouldBeepNow = BeepNowEnum.LAP;
		else if (previousStageLapStatus.resting != currentStageLapStatus.resting)
			shouldBeepNow = BeepNowEnum.LAP;
	}

}

public class BeepTestYYIE1 : BeepTestYYI
{
	public BeepTestYYIE1 ()
	{
		exerciseID = 2;
		restSeconds = 5;
		initialize ();
	}

	protected override List<double> stageSpeedKm_l
	{
		get {
			return (new List<double> {
					8.0, 9.0, 10.0, 10.5, 10.75,
					11.0, 11.25, 11.5, 11.75,
					12.0, 12.25, 12.5, 12.75,
					13.0, 13.25, 13.5, 13.75,
					14.0, 14.25, 14.5
					} );
		}
	}

	protected override List<string> stageName_l
	{
		get {
			return (new List<string> {
					"1", "3", "5", "6", "6.5",
					"7", "7.5", "8", "8.5", "9",
					"9.5", "10", "10.5", "11", "11.5",
					"12", "12.5", "13", "13.5", "14"
					} );
		}
	}

	protected override List<int> stageLaps_l
	{
		get {
			return (new List<int> {
					4,  4,  4,  16,  16,
					16, 6, 6, 12,
					12, 12, 12, 12,
					12, 12, 12, 12,
					12, 12, 12
					} );
		}
	}
}

public class BeepTestYYIE2 : BeepTestYYI
{
	public BeepTestYYIE2 ()
	{
		exerciseID = 3;
		restSeconds = 5;
		initialize ();
	}

	protected override List<double> stageSpeedKm_l
	{
		get {
			return (new List<double> {
					13.0, 15.0, 16.0, 16.5, 17.0,
					17.5, 18.0, 18.5, 19.0, 19.5,
					20.0, 20.5, 21.0, 21.5, 22.0,
					} );
		}
	}

	protected override List<string> stageName_l
	{
		get {
			return (new List<string> {
					"8", "10", "12", "13", "13.5",
					"14", "14.5", "15", "15.5", "16",
					"16.5", "17", "17.5", "18", "18.5",
					"19", "19.5", "20", "20.5", "21"
					} );
		}
	}

	protected override List<int> stageLaps_l
	{
		get {
			return (new List<int> {
					2, 2, 4, 6, 8,
					16, 16, 16, 16, 16,
					16, 16, 16, 16, 16,
					} );
		}
	}
}

public class BeepTestYYIR1 : BeepTestYYI
{
	public BeepTestYYIR1 ()
	{
		exerciseID = 4;
		restSeconds = 5;
		initialize ();
		hasVo2max = true;
	}

	protected override List<double> stageSpeedKm_l
	{
		get {
			return (new List<double> {
					10.0, 12.0, 13.0, 13.5, 14.0,
					14.5, 15.0, 15.5, 16.0, 16.5,
					17.0, 17.5, 18.0, 18.5, 19.0
					} );
		}
	}

	protected override List<string> stageName_l
	{
		get {
			return (new List<string> {
					"5", "9", "11", "12", "13",
					"14", "15", "16", "17", "18",
					"19", "20", "21", "22", "23"
					} );
		}
	}

	protected override List<int> stageLaps_l
	{
		get {
			return (new List<int> {
					2, 2, 4, 6, 8,
					16, 16, 16, 16, 16,
					16, 16, 16, 16, 16,
					} );
		}
	}

	//https://footballscience.net/testing/aerobic-endurance/yo-yo-tests/
	public override double Vo2max ()
	{
		LogB.Information (string.Format ("TotalMeters: {0}", btsm.GetTotalMetersByLapPairs (currentStageLapStatus.stage, currentStageLapStatus.lap)));
		LogB.Information (string.Format ("Vo2max: {0}", btsm.GetTotalMetersByLapPairs (currentStageLapStatus.stage, currentStageLapStatus.lap) * 0.0084 + 36.4));
		return btsm.GetTotalMetersByLapPairs (currentStageLapStatus.stage, currentStageLapStatus.lap) * 0.0084 + 36.4;
	}

	public override double Vo2max (int stageLast, int lapLast)
	{
		return btsm.GetTotalMetersByLapPairs (stageLast, lapLast) * 0.0084 + 36.4;
	}
}

public class BeepTestYYIR2 : BeepTestYYI
{
	public BeepTestYYIR2 ()
	{
		exerciseID = 5;
		restSeconds = 5;
		initialize ();
		hasVo2max = true;
	}

	protected override List<double> stageSpeedKm_l
	{
		get {
			return (new List<double> {
					13.0, 15.0, 16.0, 16.5, 17.0,
					17.5, 18.0, 18.5, 19.0, 19.5,
					20.0, 20.5, 21.0, 21.5, 22.0
					} );
		}
	}

	protected override List<string> stageName_l
	{
		get {
			return (new List<string> {
					"11", "15", "17", "18", "19",
					"20", "21", "22", "23", "24",
					"25", "26", "27", "28", "29"
					} );
		}
	}

	protected override List<int> stageLaps_l
	{
		get {
			return (new List<int> {
					2, 2, 4, 6, 8,
					16, 16, 16, 16, 16,
					16, 16, 16, 16, 16,
					} );
		}
	}

	//https://footballscience.net/testing/aerobic-endurance/yo-yo-tests/
	public override double Vo2max ()
	{
		return btsm.GetTotalMetersByLapPairs (currentStageLapStatus.stage, currentStageLapStatus.lap) * 0.0136 + 45.3;
	}
	public override double Vo2max (int stageLast, int lapLast)
	{
		return btsm.GetTotalMetersByLapPairs (stageLast, lapLast) * 0.0136 + 45.3;
	}
}

public class BeepTestCustom : BeepTestCM
{
	private double speedKmh;
	private int laps;
	private double incrementalSpeed;

	public BeepTestCustom (int lapMeters, double speedKmh, int laps, double incrementalSpeed)
	{
		exerciseID = 6;
		this.lapMeters = lapMeters;
		this.speedKmh = speedKmh;
		this.laps = laps;
		this.incrementalSpeed = incrementalSpeed;

		initialize ();
	}

	// each "stage" is done at speedKmh
	protected override List<double> stageSpeedKm_l
	{
		get {
			// note get{} is called multiple times, so don't do this at each iteration:
			// speedKmh += incrementalSpeed;
			// do this:
			double speedHere = speedKmh;

			List<double> l = new List<double> ();
			for (int i = 0; i < laps; i ++)
			{
				//LogB.Information ("speed: " + speedHere);
				l.Add (speedHere);
				speedHere += incrementalSpeed;
			}
			return (l);
		}
	}

	// each "stage" has one lap (stage is same than lap on this class
	protected override List<int> stageLaps_l
	{
		get {
			List<int> l = new List<int> ();
			for (int i = 0; i < laps; i ++)
				l.Add (1);
			return (l);
		}
	}

	protected override void decideIfShouldBeep ()
	{
		shouldBeepNow = BeepNowEnum.NO;
		if (previousStageLapStatus.stage != currentStageLapStatus.stage)
			shouldBeepNow = BeepNowEnum.LAP;
	}

	public override string GetSoundFileForStage (int stage, bool isLastOne)
	{
		return Util.GetSound (Constants.SoundTypes.CAN_START);
	}

}

//TODO: check this:
//https://en.wikipedia.org/wiki/Multi-stage_fitness_test
//https://en.wikipedia.org/wiki/Yo-Yo_intermittent_test

//TODO: add https://www.topendsports.com/testing/tests/yo-yo-endurance.htm  https://www.topendsports.com/testing/yo-yo-endurance-levels.htm
//Castagna 2006
