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

public class BeepTestStage
{
	public int num;
	public int durationMs; //duration of each stage in ms
	public int tracks; //how many tracks have each stage
	public int distanceMm; //distance of each track (in mm to avoid decimals)

	public BeepTestStage (int durationMs, int tracks, int distanceMm)
	{
		this.durationMs = durationMs;
		this.tracks = tracks;
		this.distanceMm = distanceMm;
	}
}

public class BeepTestStageList
{
	private List<BeepTestStage> bts_l;

	public struct StageTrack
	{
		public int stage;
		public int track;
		public int tracksOfThisStage;
	
		public StageTrack (int stage, int track, int tracksOfThisStage)
		{
			this.stage = stage;
			this.track = track;
			this.tracksOfThisStage = tracksOfThisStage;
		}
	}

	public StageTrack currentStageTrack;

	public BeepTestStageList ()
	{
		bts_l = new List<BeepTestStage> ();
	}

	public void CreateList (List<int> stageMs_l, List<int> stageTracks_l, List<int> stageDistances_l)
	{
		for (int i = 0; i < stageMs_l.Count; i ++)
			bts_l.Add (new BeepTestStage (stageMs_l[i], stageTracks_l[i], stageDistances_l[i]));
	}

	public void GetCurrentStageAndTrack (long currentMs)
	{
		int sum = 0;
		for (int s = 0; s < bts_l.Count; s ++)
		{
			for (int t = 0; t < bts_l[s].tracks; t ++)
			{
				sum += bts_l[s].durationMs;
				if (currentMs < sum)
				{
					currentStageTrack = new StageTrack (s, t, bts_l[s].tracks);
				        return;	
				}
			}
		}
		currentStageTrack = new StageTrack (
				bts_l.Count -1,
				bts_l[bts_l.Count -1].tracks -1,
				bts_l[bts_l.Count -1].tracks);
	}
}


public abstract class BeepTest
{
	protected BeepTestStageList btsl;
	protected DateTime dateIni;
	protected Stopwatch stopwatch;
	protected bool finished;

	private BeepTestStageList.StageTrack previousStageTrack; //to beep sound on track changed
	private bool shouldBeepNow;

	protected virtual void initialize ()
	{
		btsl = new BeepTestStageList ();
		btsl.CreateList (stageMs_l, stageTracks_l, stageDistances_l);

		stopwatch = new Stopwatch ();
		previousStageTrack = new BeepTestStageList.StageTrack (-1, -1, -1);

		finished = false;
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

	public BeepTestStageList.StageTrack GetCurrentStageAndTrack ()
	{
		//update stagetrack
		btsl.GetCurrentStageAndTrack (stopwatch.ElapsedMilliseconds);

		//manage beep variables
		shouldBeepNow = (previousStageTrack.stage != btsl.currentStageTrack.stage ||
				   previousStageTrack.track != btsl.currentStageTrack.track);
		previousStageTrack = btsl.currentStageTrack;

		//return stage track
		return btsl.currentStageTrack;
	}

	protected virtual List<int> stageMs_l
	{
		get { return (new List<int> ()); }
	}
	protected virtual List<int> stageTracks_l
	{
		get { return (new List<int> ()); }
	}
	protected virtual List<int> stageDistances_l
	{
		get { return (new List<int> ()); }
	}
	
	public bool ShouldBeepNow
	{
		get { return (shouldBeepNow); }
	}

	public bool Finished
	{
		get { return (finished); }
	}
}

public class CourseNavette : BeepTest
{
	protected override List<int> stageMs_l
	{
		get {
			return (new List<int> {
					8500, 8000, 7500, 7270, 6850, 6550, 6260, 6000, 5760, 5540,
					5330, 5140, 4960, 4800, 4645, 4500, 4360, 4235, 4115, 5000
					} );
		}
	}

	protected override List<int> stageTracks_l
	{
		get {
			return (new List<int> {
					7,  8,  8,  8,  9, 9, 10, 10, 10, 11,
					11, 12, 12, 13, 13, 13, 14, 14, 15, 15
					} );
		}
	}

	protected override List<int> stageDistances_l  //in mm (to avoid decimals)
	{
		get {
			return (new List<int> {
					20000, 20000, 20000, 20000, 20000, 20000, 20000, 20000, 20000, 20000,
					20000, 20000, 20000, 20000, 20000, 20000, 20000, 20000, 20000, 20000
					} );
		}
	}

	public CourseNavette ()
	{
		initialize ();
	}

	/*
	 * aixo es antic, buscar el millor càlcul
	float calculoVO2max()
	{
		return distanciaTramo * tramosCompletados * 0.0084 + 36.4;
	}
	*/
}

public class Pacer15m : BeepTest
{
	protected override List<int> stageMs_l
	{
		get {
			return (new List<int> {
					6750, 6000, 5684, 5400, 5143, 4909, 4696, 4500, 4320, 4154, 4000,
					3857, 3724, 3600, 3484, 3375, 3273, 3176, 3086, 3000, 2919
					} );
		}
	}

	protected override List<int> stageTracks_l
	{
		get {
			return (new List<int> {
					9, 10, 11, 12, 12, 13, 13, 14, 14, 15, 15,
					16, 17, 17, 18, 18, 19, 19, 20, 20, 21
					} );
		}
	}

	protected override List<int> stageDistances_l  //in mm (to avoid decimals)
	{
		get {
			return (new List<int> {
					15000, 15000, 15000, 15000, 15000, 15000, 15000, 15000, 15000, 15000, 15000,
					15000, 15000, 15000, 15000, 15000, 15000, 15000, 15000, 15000, 15000
					} );
		}
	}

	public Pacer15m ()
	{
		initialize ();
	}
}

public class Pacer20m : BeepTest
{
	protected override List<int> stageMs_l
	{
		get {
			return (new List<int> {
					9000, 8000, 7579, 7200, 6857, 6545, 6261, 6000, 5760, 5538, 5333,
					5143, 4966, 4800, 4645, 4500, 4364, 4235, 4114, 4000, 3892

					} );
		}
	}

	protected override List<int> stageTracks_l
	{
		get {
			return (new List<int> {
					7, 8, 8, 9, 9, 10, 10, 11, 11, 11, 12,
					12, 13, 13, 13, 14, 14, 15, 15, 16, 16
					} );
		}
	}

	protected override List<int> stageDistances_l  //in mm (to avoid decimals)
	{
		get {
			return (new List<int> {
					20000, 20000, 20000, 20000, 20000, 20000, 20000, 20000, 20000, 20000, 20000,
					20000, 20000, 20000, 20000, 20000, 20000, 20000, 20000, 20000, 20000
					} );
		}
	}

	public Pacer20m ()
	{
		initialize ();
	}
}

