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


public class UpdateProgressBar {
	public bool IsEvent;
	public bool PercentageMode;
	public double ValueToShow;

	public UpdateProgressBar() {
	}

	public UpdateProgressBar(bool isEvent, bool percentageMode, double valueToShow) {
		this.IsEvent = isEvent;
		this.PercentageMode = percentageMode;
		this.ValueToShow = valueToShow;
	}

	~UpdateProgressBar() {}
}

