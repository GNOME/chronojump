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
 *  Copyright (C) 2004-2024   Xavier de Blas <xaviblas@gmail.com>
 */

using System;

//used on TreeView
public class EncoderNeuromuscularData
{
	public string code;
	public string person;
	public string jump_num;  //can be "AVG"
	public double extraWeight;
	public double e1_range; //double on AVG row
	public double e1_t; 	//double on AVG row
	public double e1_fmax;
	public double e1_rfd_avg;
	public double e1_i;

	public double ca_range; //double on AVG row
	public double cl_t;     //double on AVG row
	public double cl_rfd_avg;
	public double cl_i;

	public double cl_f_avg;
	public double cl_vf;
	public double cl_f_max;

	public double cl_s_avg;
	public double cl_s_max;
	public double cl_p_avg;
	public double cl_p_max;

	public EncoderNeuromuscularData () {
	}

	//used on TreeView analyze
	public EncoderNeuromuscularData (
			string code, string person, string jump_num, double extraWeight,
			double e1_range, double e1_t, double e1_fmax, double e1_rfd_avg, double e1_i,
			double ca_range, double cl_t, double cl_rfd_avg, double cl_i,
			double cl_f_avg, double cl_vf, double cl_f_max, 
			double cl_s_avg, double cl_s_max, double cl_p_avg, double cl_p_max
			)
	{
		this.code = code;
		this.person = person;
		this.jump_num = jump_num;
		this.extraWeight = extraWeight;
		this.e1_range = e1_range; 
		this.e1_t = e1_t;
		this.e1_fmax = e1_fmax;
		this.e1_rfd_avg = e1_rfd_avg;
		this.e1_i = e1_i;
		this.ca_range = ca_range;
		this.cl_t = cl_t;
		this.cl_rfd_avg = cl_rfd_avg;
		this.cl_i = cl_i;
		this.cl_f_avg = cl_f_avg;
		this.cl_vf = cl_vf;
		this.cl_f_max = cl_f_max;
		this.cl_s_avg = cl_s_avg;
		this.cl_s_max = cl_s_max;
		this.cl_p_avg = cl_p_avg;
		this.cl_p_max = cl_p_max;
	}

	//reading contents file from graph.R
	public EncoderNeuromuscularData (string [] cells)
	{
		//cells [0-2] are not converted because are strings
		for(int i = 3 ; i < cells.Length ;  i ++)
			cells[i] = Util.TrimDecimals(Convert.ToDouble(Util.ChangeDecimalSeparator(cells[i])),3);
	
		this.code 	= cells[0];
		this.person 	= cells[1];
		this.jump_num 	= cells[2];
		this.extraWeight = Convert.ToDouble(cells[3]);
		this.e1_range 	= Convert.ToDouble(cells[4]);
		this.e1_t 	= Convert.ToDouble(cells[5]);
		this.e1_fmax 	= Convert.ToDouble(cells[6]);
		this.e1_rfd_avg	= Convert.ToDouble(cells[7]);
		this.e1_i	= Convert.ToDouble(cells[8]);
		this.ca_range	= Convert.ToDouble(cells[9]);
		this.cl_t 	= Convert.ToDouble(cells[10]);
		this.cl_rfd_avg = Convert.ToDouble(cells[11]);
		this.cl_i 	= Convert.ToDouble(cells[12]);
		this.cl_f_avg 	= Convert.ToDouble(cells[13]);
		this.cl_vf 	= Convert.ToDouble(cells[14]);
		this.cl_f_max 	= Convert.ToDouble(cells[15]);
		this.cl_s_avg 	= Convert.ToDouble(cells[16]);
		this.cl_s_max 	= Convert.ToDouble(cells[17]);
		this.cl_p_avg 	= Convert.ToDouble(cells[18]);
		this.cl_p_max 	= Convert.ToDouble(cells[19]);
	}

	public string ToCSV (string decimalSeparator) {
		//latin:	2,3 ; 2,5
		//non-latin:	2.3 , 2.5

		string sep = ":::";
		string str = 
			person + sep + jump_num + sep + extraWeight.ToString () + sep +
			e1_range.ToString() + sep +
			e1_t.ToString() + sep + e1_fmax.ToString() + sep + 
			e1_rfd_avg.ToString() + sep + e1_i.ToString() + sep + 
			ca_range.ToString() + sep + cl_t.ToString() + sep + 
			cl_rfd_avg.ToString() + sep + cl_i.ToString() + sep + 
			cl_f_avg.ToString() + sep + cl_vf.ToString() + sep + cl_f_max.ToString() + sep + 
			cl_s_avg.ToString() + sep + cl_s_max.ToString() + sep + 
			cl_p_avg.ToString() + sep + cl_p_max.ToString();

		if(decimalSeparator == "COMMA")
			str = Util.ConvertToComma(str);
		else
			str = Util.ConvertToPoint(str);
			
		if(decimalSeparator == "COMMA")
			return Util.ChangeChars(str, ":::", ";");
		else
			return Util.ChangeChars(str, ":::", ",");
	}
}
