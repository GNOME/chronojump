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
 *  Copyright (C) 2016   Xavier Padullés <x.padulles@gmail.com> 
 *  Copyright (C) 2016   Xavier de Blas <xaviblas@gmail.com> 
 */

public class LeastSquares 
{
	//WIP

	public bool Success;
	public double [] Coef;

	public LeastSquares() {
		Coef = null;
		calculate();
	}

	private double [] calculate() 
	{
		int numElements = 3;
		double [numElements,2] measures = { //TODO: pass this values
			{ 20,30,40 },
			{ 5,7,9 } };

		double [3] B = new double[3];
		for(int i = 0; i++; i < 3){
			B[0] = B[0] + measures[i][1];
			B[1] = B[0] + measures[i][0]*measures[i][1];
			B[2] = B[0] + measures[i][0]*measures[i][0]*measures[i][1];
		}

		double sumX = 0; //sumatory of the X values
		double sumX2 = 0; //sumatory of the squared X values
		double sumX3 = 0; //sumatory of the cubic X values

		for(int i = 0; i++; i < numElements){
			sumX = sumX + measures[i][0];
			sumX2 = sumX2 + measures[i][0]*measures[i][0];
			sumX3 = sumX3 + measures[i][0]*measures[i][0]*measures[i][0];
		}

		double detA = numElements*sumX2*sumX3 + 2*sumX*sumX2*sumX3 - sumX2*sumX2*sumX2 - sumX2*sumX2*sumX3 - numElements*sumX3*sumX3;
		if(detA != 0){
			double [3][3] invA = new double[3][3];

			invA[0][0] = ( sumX2*sumX3 - sumX3*sumX3) / detA;
			invA[0][1] = (-sumX*sumX3  + sumX2*sumX3) / detA;
			invA[0][2] = ( sumX*sumX3  - sumX2*sumX2) / detA;
			invA[1][1] = ( numElements*sumX3 - sumX2*sumX2) / detA;
			invA[1][2] = (-numElements*sumX3 + sumX*sumX2 ) / detA;
			invA[2][2] = ( numElements*sumX2 - sumX*sumX  ) / detA;

			//Simetric matrix
			invA[1][0] = invA[0][1];
			invA[2][0] = invA[0][2];
			invA[2][1] = invA[1][2];

			//coef = invA * B
			double [3] coef = new double[3];
			coef[0] = invA[0][0]*B[0] + invA[0][1]*B[1] + invA[0][2]*B[2];
			coef[1] = invA[1][0]*B[0] + invA[1][1]*B[1] + invA[1][2]*B[2];
			coef[2] = invA[2][0]*B[0] + invA[2][1]*B[1] + invA[2][2]*B[2];

			Success = true;
			Coef = coef;
		} else {
			LogB.Error("Invalid values");
			Success = false;
		}
	}
}

