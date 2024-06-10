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
 *  Copyright (C) 2023-2024   Xavier de Blas <xaviblas@gmail.com>
 */

using System;
using System.Collections.Generic; //List
using Gtk;
using Cairo;

public class CairoGraphForceSensorSignalAsteroids : CairoGraphForceSensorSignal
{
	private double lastShot;
	private double lastPointUp; //each s 1 point up
	private int multiplier;

	public CairoGraphForceSensorSignalAsteroids (DrawingArea area, string title, bool horizontal)
	{
		initForceSensor (area, title, horizontal);
		multiplier = 1000000; //forceSensor

		lastShot = 0;
		lastPointUp = 0; //each s 1 point up
	}

	protected override void plotSpecific ()
	{
		if (! capturing)
			return;

		asteroidsPlot (points_l[points_l.Count -1], startAt, multiplier,
				marginAfterInSeconds, points_l, horizontal,
				ref lastShot, ref lastPointUp);
	}
}

public class Asteroids
{
	public int Points;
	public bool Dark;
	public int MaxY;
	public int MinY;
	public int ShotsFrequency;
	public const int PlayerRadius = 6;

	private List<Asteroid> asteroid_l;
	private List<Power> power_l;
	private List<Shot> shot_l;
	private List<AsteroidFloatingPoints> asteroidPoints_l;
	private Random random = new Random();
	private double lastCrash; //to paint ship in red for half second
	private AsteroidsPowerEffectManage powerEffectManage;

	private bool micros;
	private int multiplier;
	private Cairo.Color bluePlots = new Cairo.Color (0, 0, .78, 1);
	private Cairo.Color gray = new Cairo.Color (.5, .5, .5, 1);
	//private Cairo.Color white = new Cairo.Color (1, 1, 1, 1);
	private Cairo.Color yellow = new Cairo.Color (0.906, 0.745, 0.098, 1);
	private Cairo.Color redDark = new Cairo.Color (0.55, 0, 0, 1);
	private Cairo.Color red = new Cairo.Color (.784, 0, 0);

	public Asteroids (int maxY, int minY, bool Dark, int asteroidsFrequency, int shotsFrequency, bool micros, int recordingTime)
	{
		this.Dark = Dark;
		this.MaxY = maxY;
		this.MinY = minY;
		this.ShotsFrequency = shotsFrequency;
		this.micros = micros;

		if (micros)
			multiplier = 1000000;
		else
			multiplier = 1000;

		Points = 0;
		lastCrash = -1; //to not start in red
		asteroid_l = new List<Asteroid> ();
		power_l = new List<Power> ();
		shot_l = new List<Shot> ();
		asteroidPoints_l = new List<AsteroidFloatingPoints> ();
		powerEffectManage = new AsteroidsPowerEffectManage ();

		if (recordingTime < 0)
			recordingTime = 100;

		//create asteroids
		for (int i = 0; i < asteroidsFrequency * recordingTime; i ++)
		{
			int xStart = random.Next (7*multiplier, 100*multiplier);
			int usLife = random.Next (3*multiplier/10, 15*multiplier);

			//shield
			int shield = random.Next (0, 20);
			if (shield <= 10)
				shield = 0;
			else if (shield <= 15)
				shield = 1;
			else if (shield <= 18)
				shield = 2;
			else
				shield = 3;

			asteroid_l.Add (new Asteroid (
						xStart, random.Next (minY, maxY), // y (force)
						usLife, random.Next (minY, maxY), // y (force)
						random.Next (20, 100), // size
						createAsteroidColor (),
						micros,
						shield
						));
		}

		/*
		//debug with just one
		asteroid_l.Add (new Asteroid (10 * multiplier, -50, 5 * multiplier, +50,
					50, createAsteroidColor (), micros, 0));
					*/

		Random rnd = new Random();
		//create powers (aprox 1 each 20 asteroids)
		for (int i = 0; i < asteroidsFrequency * recordingTime /20; i ++)
		{
			int xStart = random.Next (7*multiplier, 100*multiplier);
			int usLife = 8*multiplier;
			int y = random.Next (minY, maxY);

			power_l.Add (new Power (
						xStart, y, // y (force)
						usLife, y, // y (force)
						35, // size (side)
						new Cairo.Color (.5,.5,.5, 1), //unused now, as its an empty circle (with 3 borders)
						micros,
						(Power.TypeEnum) rnd.Next (0, Enum.GetNames (typeof (Power.TypeEnum)).Length) //random power
						));
		}
	}

	public List<Asteroid> GetAllAsteroidsPaintable (double startAtPointX, int marginAfterInSeconds)
	{
		List<Asteroid> aPaintable_l = new List<Asteroid> ();
		foreach (Asteroid a in asteroid_l)
			if (a.NeedToShow (startAtPointX, marginAfterInSeconds))
				aPaintable_l.Add (a);

		return aPaintable_l;
	}

	public void AsteroidCrashedWithPlayerSetTime (double timeNow)
	{
		lastCrash = timeNow;
	}

	public List<Power> GetAllPowersPaintable (double startAtPointX, int marginAfterInSeconds)
	{
		List<Power> pPaintable_l = new List<Power> ();
		foreach (Power a in power_l)
			if (a.NeedToShow (startAtPointX, marginAfterInSeconds))
				pPaintable_l.Add (a);

		return pPaintable_l;
	}

	public void Shot (PointF p, bool unstoppable)
	{
		shot_l.Add (new Shot (p, micros, unstoppable));
	}

	public List<Shot> GetAllShotsPaintable (double timeNow)
	{
		List<Shot> sPaintable_l = new List<Shot> ();
		foreach (Shot s in shot_l)
			if (s.NeedToShow (timeNow))
				sPaintable_l.Add (s);

		return sPaintable_l;
	}

	public void PaintShip (double x, double y, double timeNow, Context g)
	{
		Cairo.Color playerColor = bluePlots;
		if (Dark)
			playerColor = yellow;

		//after a crash show ship half red for .5 seconds
		if (lastCrash > 0 && timeNow - lastCrash < .5*multiplier)
			playerColor = redDark;

		CairoUtil.DrawCircle (g, x-PlayerRadius/2, y-PlayerRadius/2, PlayerRadius, playerColor, true);
	}

	public void PaintShot (Shot s, double sx, double sy, double timeNow, bool horizontal, Context g)
	{
		Cairo.Color color = bluePlots;
		if (s.Unstoppable)
			color = red;
		else {
			if (Dark)
				color = yellow;
			if (s.LifeIsEnding (timeNow))
				color = gray;
		}

		g.Save ();
		g.LineWidth = 2;
		g.SetSourceColor (color);

		if (horizontal) {
			g.MoveTo (sx -3, sy);
			g.LineTo (sx +3, sy);
		} else {
			g.MoveTo (sx, sy -3);
			g.LineTo (sx, sy + 3);
		}

		g.Stroke ();
		g.Restore ();

		//drawCircle (sx, sy, s.Size, color, true);
	}

	public enum ShotCrashedEnum { NOCRASHED, CRASHEDNODESTROY, CRASHEDANDDESTROY }
	public ShotCrashedEnum ShotCrashedWithAsteroid (double sx, double sy, int size,
			List<Asteroid> asteroid_l, List<Point3F> asteroidXYZ_l,
			out int i, out Asteroid asteroid) //the i asteroid
	{
		asteroid = null;

		for (i = 0; i < asteroidXYZ_l.Count; i ++)
		{
			Point3F aXYZ = asteroidXYZ_l[i];
			if (CairoUtil.GetDistance2D (aXYZ.X, aXYZ.Y, sx, sy) < aXYZ.Z + size)
			{
				asteroid_l[i].Shield --;
				if (asteroid_l[i].Shield < 0)
				{
					asteroid_l[i].Alive = false;
					asteroid = asteroid_l[i];
					return ShotCrashedEnum.CRASHEDANDDESTROY;
				} else
					return ShotCrashedEnum.CRASHEDNODESTROY;
			}
		}

		return ShotCrashedEnum.NOCRASHED;
	}

	public void AddAsteroidFloatingPoints (AsteroidFloatingPoints ap)
	{
		asteroidPoints_l.Add (ap);
	}

	public List<AsteroidFloatingPoints> GetAllAsteroidFloatingPointssPaintable ()
	{
		List<AsteroidFloatingPoints> apPaintable_l = new List<AsteroidFloatingPoints> ();
		foreach (AsteroidFloatingPoints ap in asteroidPoints_l)
			if (ap.NeedToShow ())
				apPaintable_l.Add (ap);

		return apPaintable_l;
	}

	private Cairo.Color createAsteroidColor ()
	{
		Cairo.Color color;
		do {
			color = new Cairo.Color (random.NextDouble (), random.NextDouble (), random.NextDouble (),
					(double) random.Next (5,10)/10); //alpha: .5-1
		} while (Dark == CairoUtil.ColorIsDark (color));

		return color;
	}

	public AsteroidsPowerEffectManage PowerEffectManage {
		get { return powerEffectManage; }
	}
}

public abstract class MovingObject
{
	protected int xStart; //time. When screen right is this time it will start
	protected int yStart; //force
	protected int usLife; //time
	protected int yEnd; //force
	protected int size;
	protected Cairo.Color color;
	protected bool alive;
	protected int multiplier;

	protected void initialize (int xStart, int yStart, int usLife, int yEnd, int size, Cairo.Color color,  bool micros)
	{
		this.xStart = xStart;
		this.yStart = yStart;
		this.usLife = usLife;
		this.yEnd = yEnd;
		this.size = size;
		this.color = color;

		if (micros)
			multiplier = 1000000;
		else
			multiplier = 1000;

		this.alive = true;
	}

	public bool NeedToShow (double graphUsStart, int graphSecondsAtRight)
	{
		if (! alive)
			return false;

		int graphUsAtRight = graphSecondsAtRight * multiplier;
		double graphUsTotalAtRight = graphUsStart + graphUsAtRight;

		// the 3000000 is for having a bit of margin to easily consider radius
		// to not have asteroids appear/disappear on sides when center arrives to that limits
		if (xStart - 3*multiplier > graphUsTotalAtRight)
			return false;
		if (xStart + usLife + 3*multiplier < graphUsTotalAtRight)
		{
			//LogB.Information (string.Format (
			//			"xStart: {0}, usLife: {1}, multiplier: {2}, graphUsTotalAtRight: {3}",
			//			xStart, usLife, multiplier, graphUsTotalAtRight));
			return false;
		}

		return true;
	}

	public double GetTimeNowProportion (double graphUsStart, int graphSecondsAtRight)
	{
		int graphUsAtRight = graphSecondsAtRight * multiplier;
		double graphUsTotalAtRight = graphUsStart + graphUsAtRight;

		LogB.Information (string.Format ("GetTimeNowProportion: graphUsStart: {0}, graphSecondsAtRight: {1}, graphUsAtRight: {2}, graphUsTotalAtRight: {3}, total: {4}",
					graphUsStart, graphSecondsAtRight, graphUsAtRight, graphUsTotalAtRight, UtilAll.DivideSafe (graphUsTotalAtRight - xStart, usLife)));

		return UtilAll.DivideSafe (graphUsTotalAtRight - xStart, usLife);
	}

	public double GetYNow (double graphUsStart, int graphSecondsAtRight)
	{
		double lifeProportion = GetTimeNowProportion (graphUsStart, graphSecondsAtRight);
		LogB.Information ("lifeProportion:" + lifeProportion);
		return lifeProportion * (yEnd - yStart) + yStart;
	}

	public abstract bool HasCrashedWithPlayer (double moX, double moY,
			double playerX, double playerY, int playerRadius);

	public void Destroy ()
	{
		alive = false;
	}

	public int Size {
		get { return size; }
	}
	public Cairo.Color Color {
		get { return color; }
	}
	public bool Alive {
		set { alive = value; }
	}
}

public class Asteroid : MovingObject
{
	private int shield; // 0 - 3
	private int pointsOnDestroy; //related to shield initial value

	public Asteroid (int xStart, int yStart, int usLife, int yEnd, int size, Cairo.Color color, bool micros, int shield)
	{
		initialize (xStart, yStart, usLife, yEnd, size, color, micros);

		this.shield = shield;
		this.pointsOnDestroy = 5 + shield * 5;
	}

	//asteroid: circle
	public override bool HasCrashedWithPlayer (double moX, double moY,
			double playerX, double playerY, int playerRadius)
	{
		return (CairoUtil.GetDistance2D (moX, moY, playerX, playerY) < size + playerRadius);
	}

	public override string ToString ()
	{
		return string.Format ("({0},{1}) ({2},{3}) size: {4} color: ({5},{6},{7} {8})",
				xStart, yStart, xStart+usLife, yEnd, size, color.R, color.G, color.B, color.A);
	}

	public int Shield {
		get { return shield; }
		set { shield = value; }
	}

	public int PointsOnDestroy {
		get { return pointsOnDestroy; }
	}
}

//to display the power and manage the collision with it
public class Power : MovingObject
{
	public enum TypeEnum { POINTS100, DOUBLESHOT, //TRIPLESHOT,
		UNSTOPPABLESHOT }
	public TypeEnum powerType;

	//public Cairo.Color ColorBorderExt;
	//public Cairo.Color ColorBorderMid;
	//public Cairo.Color ColorBorderInt;
	private Blink blink;

	public Power (int xStart, int yStart, int usLife, int yEnd, int size, Cairo.Color color, bool micros, TypeEnum powerType)
	{
		initialize (xStart, yStart, usLife, yEnd, size, color, micros);

		//ColorBorderExt = new Cairo.Color (0, 0, 1, 1);
		//ColorBorderMid = new Cairo.Color (0, 1, 0, 1);
		//ColorBorderInt = new Cairo.Color (1, 0, 0, 1);
		this.powerType = powerType;

		blink = new Blink ();
		blink.Start ();
	}

	//power: rectangle
	public override bool HasCrashedWithPlayer (double moX, double moY,
			double playerX, double playerY, int playerRadius)
	{
		return (
				(playerX +playerRadius >= moX -size) &&
				(playerX -playerRadius <= moX +size) &&
				(playerY +playerRadius >= moY -size) &&
				(playerY -playerRadius <= moY +size) );
	}

	public bool ShowText {
		get { return blink.IsOn; }
	}
	public TypeEnum PowerType {
		get { return powerType; }
	}
}

//only 1 effect at a time (or none)
public class AsteroidsPowerEffectManage
{
	private AsteroidsPowerEffect apeCurrent;

	public AsteroidsPowerEffectManage ()
	{
	}

	//but currently just one at a time
	public void NewEffect (AsteroidsPowerEffect ape)
	{
		apeCurrent = ape;
	}

	public bool ShowDoubleShot ()
	{
		return (apeCurrent != null && apeCurrent.Alive && apeCurrent.PowerType == Power.TypeEnum.DOUBLESHOT);
	}

	public bool ShowUnstoppable ()
	{
		return (apeCurrent != null && apeCurrent.Alive && apeCurrent.PowerType == Power.TypeEnum.UNSTOPPABLESHOT);
	}

	public void ShouldEndEffect ()
	{
		if (apeCurrent != null && apeCurrent.Alive)
			apeCurrent.ShouldEndEffect ();
	}
}
public class AsteroidsPowerEffect
{
	private Power.TypeEnum powerType;
	private int lifespanInSeconds;
	private bool alive;
	private DateTime timeStarted;

	public AsteroidsPowerEffect (Power.TypeEnum powerType)
	{
		this.powerType = powerType;
		timeStarted = DateTime.Now;

		if (powerType == Power.TypeEnum.POINTS100)
		{
			alive = false;
			lifespanInSeconds = 0;
		}
		else if (powerType == Power.TypeEnum.DOUBLESHOT ||// powerType == Power.TypeEnum.TRIPLESHOT ||
				powerType == Power.TypeEnum.UNSTOPPABLESHOT)
		{
			alive = true;
			lifespanInSeconds = 5;
		}
	}

	public bool ShouldEndEffect ()
	{
		if (alive && DateTime.Now.Subtract (timeStarted).TotalSeconds > lifespanInSeconds)
		{
			alive = false;
			return true;
		}

		return false;
	}

	public Power.TypeEnum PowerType {
		get { return powerType; }
	}
	public bool Alive {
		get { return alive; }
	}
}

public class Shot
{
	private const int life = 3; //will not arrive to end of screen (and not kill something that still we have not seen)
	private const int speed = 3; //relative to ship //note if this change, life will need to change
	private const int size = 2;
	private int multiplier;

	private int xStart; //time when started
	private int yStart;
	private bool alive;
	private bool unstoppable;

	public Shot (PointF p, bool micros, bool unstoppable)
	{
		this.xStart = Convert.ToInt32 (p.X); //TODO: to the right of the "ship"
		this.yStart = Convert.ToInt32 (p.Y);
		this.alive = true;

		if (micros)
			multiplier = 1000000;
		else
			multiplier = 1000;

		this.unstoppable = unstoppable;
	}

	public bool NeedToShow (double timeNow)
	{
		if (! alive)
			return false;

		if (timeNow - xStart > multiplier * life)
		       return false;

		return true;
	}

	public double GetXNow (double timeNow)
	{
		return xStart + speed * (timeNow - xStart);
	}

	// will be shown on gray
	public bool LifeIsEnding (double timeNow)
	{
		if (timeNow - xStart > multiplier * .75 * life && timeNow - xStart <= multiplier * life)
		       return true;

		return false;
	}

	public int Ystart {
		get { return yStart; }
	}
	public int Size {
		get { return size; }
	}
	public bool Alive {
		set { alive = value; }
	}
	public bool Unstoppable {
		get { return unstoppable; }
	}
}

//to show a +5 on destroying an asteroid
public class AsteroidFloatingPoints
{
	private DateTime timeStart;
	private double xGraph;
	private double yGraph;
	private int points;
	private bool alive;
	private const double life = .75; //seconds

	public AsteroidFloatingPoints (DateTime timeStart, double xGraph, double yGraph, int points)
	{
		this.timeStart = timeStart;
		this.xGraph = xGraph;
		this.yGraph = yGraph;
		this.points = points;
		this.alive = true;
	}

	public bool NeedToShow ()
	{
		if (! alive)
			return false;

		if (DateTime.Now.Subtract (timeStart).TotalSeconds > life)
		       return false;

		return true;
	}

	public double XGraph {
		get { return xGraph; }
	}

	public double YGraph {
		get { return yGraph; }
	}

	public int Points {
		get { return points; }
	}

	public bool Alive {
		get { return alive; }
	}
}
