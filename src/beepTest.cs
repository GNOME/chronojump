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
 *  Copyright (C) 2024   Xavier de Blas <xaviblas@gmail.com>
 */

using System;
using System.Collections.Generic; //List<T>
using System.Diagnostics;  //Stopwatch

//definition of each stage on the test
public class BeepTestStage
{
	public double speedKmh;
	public double lapSeconds; //duration of each lap
	public int laps; //how many laps have each stage
	public int lapMeters; //distance of each lap

	public BeepTestStage (double speedKmh, double lapSeconds, int laps, int lapMeters)
	{
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
		public int lap;
		public int lapsOfThisStage;
		public double speedKmh;
		public bool resting; //rest or not rest
		public bool resting1stHalf; //1st half of the rest (to show walk to one side or to the other) (only applies if resting)

		public StageLapStatus (int stage, int lap, int lapsOfThisStage, double speedKmh, bool resting, bool resting1stHalf)
		{
			this.stage = stage;
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

	public void CreateList (List<double> stageSpeedKm_l, List <double> lapDurationS_l, List<int> stageLaps_l, List<int> lapDistM_l)
	{
		for (int i = 0; i < lapDurationS_l.Count; i ++)
			bts_l.Add (new BeepTestStage (stageSpeedKm_l[i], lapDurationS_l[i], stageLaps_l[i], lapDistM_l[i]));
	}

	public StageLapStatus GetCurrentStageLapStatus (long currentMs, int restSeconds, out bool shouldFinish)
	{
		shouldFinish = false;
		double sum = 0;
		for (int s = 0; s < bts_l.Count; s ++)
		{
			for (int t = 0; t < bts_l[s].laps; t ++)
			{
				sum += 1000 * bts_l[s].lapSeconds;
				if (restSeconds > 0 && Util.IsEven (t+1))
					sum += 1000 * restSeconds;

				if (currentMs < sum)
					return new StageLapStatus (s, t, bts_l[s].laps, bts_l[s].speedKmh,
							(restSeconds > 0 && Util.IsEven (t+1) && currentMs + 1000 * restSeconds >= sum), // true if we are resting
							(restSeconds > 0 && Util.IsEven (t+1) && currentMs + 1000 * restSeconds/2 < sum) // (if we are resting: true in the 1st part of the rest)
							);
			}
		}

		shouldFinish = true;
		return getLastLapStatus ();
	}

	protected StageLapStatus getLastLapStatus ()
	{
		return new StageLapStatus (
				bts_l.Count -1,
				bts_l[bts_l.Count -1].laps -1,
				bts_l[bts_l.Count -1].laps,
				bts_l[bts_l.Count -1].speedKmh,
				false, false);
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

//tests creation and interaction with Chronojump events
public abstract class BeepTest
{
	protected BeepTestStageManage btsm;
	protected DateTime dateIni;
	protected Stopwatch stopwatch;
	protected bool finished;
	protected bool hasVo2max; //default false
	protected int startedWithMs = 0;
	protected int restSeconds = 0;
	protected int lapMeters = 20; //this works because these tests have all the laps with same meters, if any test has different meters, do not use this

	protected BeepTestStageManage.StageLapStatus previousStageLapStatus; //to beep sound on lap changed
	public enum BeepNowEnum { NO, LAP, STAGE };
	protected BeepNowEnum shouldBeepNow;
	private Gdk.Pixbuf imageLapStatus;

	protected virtual void initialize ()
	{
		btsm = new BeepTestStageManage ();
		btsm.CreateList (stageSpeedKm_l, lapDurationS_l, stageLaps_l, lapDistM_l);

		stopwatch = new Stopwatch ();
		previousStageLapStatus = new BeepTestStageManage.StageLapStatus (-1, -1, -1, -1, false, false);

		finished = false;
	}

	public static string Leger20Name = "Leger 20 m shuttle run";
	public static string Leger15Name = "Leger 15 m shuttle run";
	public static string YYIE1Name = "Yo Yo Intermitent Endurance 1";
	public static string YYIE2Name = "Yo Yo Intermitent Endurance 2";
	public static string YYIR1Name = "Yo Yo Intermitent Recovery 1";
	public static string YYIR2Name = "Yo Yo Intermitent Recovery 2";
	public static string ConstantSpeedName = "Constant speed";
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
			ConstantSpeedName
		};
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
		BeepTestStageManage.StageLapStatus currentStageLapStatus = btsm.GetCurrentStageLapStatus (
				stopwatch.ElapsedMilliseconds + startedWithMs,
				restSeconds, out bool shouldFinish);

		if (shouldFinish)
			finished = true;

		decideIfShouldBeep (currentStageLapStatus);
		imageLapStatus = getImageForLapStatus (currentStageLapStatus);

		previousStageLapStatus = currentStageLapStatus;

		return currentStageLapStatus;
	}

	protected virtual void decideIfShouldBeep (BeepTestStageManage.StageLapStatus currentStageLapStatus)
	{
		shouldBeepNow = BeepNowEnum.NO;
		if (previousStageLapStatus.stage >= 0 && //double beep on stage not at start of the test
				previousStageLapStatus.stage != currentStageLapStatus.stage)
				shouldBeepNow = BeepNowEnum.STAGE;
		else if (previousStageLapStatus.lap != currentStageLapStatus.lap)
			shouldBeepNow = BeepNowEnum.LAP;
	}

	protected virtual Gdk.Pixbuf getImageForLapStatus (BeepTestStageManage.StageLapStatus currentStageLapStatus)
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

	protected virtual List<double> stageSpeedKm_l
	{
		get { return (new List<double> ()); }
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

	public virtual double Vo2max (double maxSpeed)
	{
		return -1;
	}

	public BeepNowEnum ShouldBeepNow
	{
		get { return (shouldBeepNow); }
	}

	public Gdk.Pixbuf ImageLapStatus
	{
		get { return (imageLapStatus); }
	}

	public bool Finished
	{
		get { return (finished); }
	}

	public bool HasVo2max
	{
		get { return (hasVo2max); }
	}
}

public class BeepTestLeger20m : BeepTest
{
	private bool startFirstAt8Kmh;

	public BeepTestLeger20m (int startStage, bool startFirstAt8Kmh)
	{
		this.startFirstAt8Kmh = startFirstAt8Kmh;
		initialize ();
		hasVo2max = true;

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
	public override double Vo2max (double maxSpeed)
	{
		return maxSpeed * 6.55 - 35.8;
	}

}

public class BeepTestLeger15m : BeepTest
{
	private bool startFirstAt8Kmh;

	public BeepTestLeger15m (int startStage, bool startFirstAt8Kmh)
	{
		this.startFirstAt8Kmh = startFirstAt8Kmh;

		lapMeters = 15;

		initialize ();

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
}

/*
 * https://footballscience.net/testing/aerobic-endurance/university-of-montreal-track-test/
 * http://www.alfisport.com/News/Tesis/Pruebas_rendimiento.html
 * https://salutpersonal.wordpress.com/wp-content/uploads/2014/02/test-lc3a9ger-boucher_150214.jpg
 * https://tfontanet.github.io/cinquieme/5-perimetres-aires-epi-vma.pdf
 * https://www.traileur.ch/estimation-vma-test-luc-leger-boucher
 * universitats/blanquerna/assignatures/ergonomia-assignatures/valoracio-funcional-ergonomia/docs/ergonomia/ergonomia-esportiva/calçat-cursa/curs-running-estiu-2016/Apunts/Leger, L & Boucher, R. (1980). An indirect continuous running multistage field test..pdf
 *
public class BeepTestMontreal : BeepTest
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

public class Pacer15m : BeepTest
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

public class Pacer20m : BeepTest
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
public abstract class BeepTestYYI : BeepTest
{
	public BeepTestYYI ()
	{
		restSeconds = 5;
	}

	protected override void decideIfShouldBeep (BeepTestStageManage.StageLapStatus currentStageLapStatus)
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
		restSeconds = 5;
		initialize ();
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

public class BeepTestYYIR2 : BeepTestYYI
{
	public BeepTestYYIR2 ()
	{
		restSeconds = 5;
		initialize ();
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

public class BeepTestConstantSpeed : BeepTest
{
	private double speedKmh;
	private int laps;

	public BeepTestConstantSpeed (int lapMeters, double speedKmh, int laps)
	{
		this.lapMeters = lapMeters;
		this.speedKmh = speedKmh;
		this.laps = laps;

		initialize ();
	}

	// each "stage" is done at speedKmh
	protected override List<double> stageSpeedKm_l
	{
		get {
			List<double> l = new List<double> ();
			for (int i = 0; i < laps; i ++)
				l.Add (speedKmh);
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

	protected override void decideIfShouldBeep (BeepTestStageManage.StageLapStatus currentStageLapStatus)
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
