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
* Xavier de Blas:
*/


/* this is used for store data of the graph we want to prepare.
 * we cannot call directly because there are problems with threads and calling gtk
 * PulseGTK has to call this graph creation
 */

public class PrepareEventGraphJumpSimple {
	public double tv;
	public double tc;

	public PrepareEventGraphJumpSimple() {
	}

	public PrepareEventGraphJumpSimple(double tv, double tc) {
		this.tv = tv;
		this.tc = tc;
	}

	~PrepareEventGraphJumpSimple() {}
}

public class PrepareEventGraphJumpReactive {
	public double lastTv;
	public double lastTc;
	public string tvString;
	public string tcString;

	public PrepareEventGraphJumpReactive() {
	}

	public PrepareEventGraphJumpReactive(double lastTv, double lastTc, string tvString, string tcString) {
		this.lastTv = lastTv;
		this.lastTc = lastTc;
		this.tvString = tvString;
		this.tcString = tcString;
	}

	~PrepareEventGraphJumpReactive() {}
}

public class PrepareEventGraphRunSimple {
	public double time;
	public double speed;

	public PrepareEventGraphRunSimple() {
	}

	public PrepareEventGraphRunSimple(double time, double speed) {
		this.time = time;
		this.speed = speed;
	}

	~PrepareEventGraphRunSimple() {}
}

public class PrepareEventGraphRunInterval {
	public double distance;
	public double lastTime;
	public string timesString;
	public double distanceTotal; //we pass this because it's dificult to calculate in runs with variable distances
	public string distancesString; //we pass this because it's dificult to calculate in runs with variable distances

	public PrepareEventGraphRunInterval() {
	}

	public PrepareEventGraphRunInterval(double distance, double lastTime, string timesString, double distanceTotal, string distancesString) {
		this.distance = distance;
		this.lastTime = lastTime;
		this.timesString = timesString;
		this.distanceTotal = distanceTotal;
		this.distancesString = distancesString;
	}

	~PrepareEventGraphRunInterval() {}
}

public class PrepareEventGraphPulse {
	public double lastTime;
	public string timesString;

	public PrepareEventGraphPulse() {
	}

	public PrepareEventGraphPulse(double lastTime, string timesString) {
		this.lastTime = lastTime;
		this.timesString = timesString;
	}

	~PrepareEventGraphPulse() {}
}

public class PrepareEventGraphReactionTime {
	public double time;

	public PrepareEventGraphReactionTime() {
	}

	public PrepareEventGraphReactionTime(double time) {
		this.time = time;
	}

	~PrepareEventGraphReactionTime() {}
}

