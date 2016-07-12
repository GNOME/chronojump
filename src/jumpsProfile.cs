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
 *  Copyright (C) 2004-2016   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
using System.Collections.Generic; //List
using Cairo;

public class JumpsProfileIndex
{
	public string name;

	public enum ErrorCodes { NEEDJUMP, NEGATIVE, NONE_OK }
	public ErrorCodes errorCode;
	public string Text;
	public Cairo.Color Color;
	public string ErrorMessage;
	
	private string jumpHigherName;
	private string jumpLowerName;
	
	public enum Types { FMAX, FEXPL, CELAST, CARMS, FREACT }   
	public Types type; 

	public double Result;

	public JumpsProfileIndex (Types type, string jumpHigherName, string jumpLowerName, double higher, double lower, double dja) 
	{
		//colour palette: http://www.colourlovers.com/palette/4286628/Tabor_1
		this.type = type;
		switch(type) {
			case Types.FMAX:
				Text = "% Maximum Force  SJl100% / DJa";
				Color = colorFromRGB(101,86,67);
				break;
			case Types.FEXPL:
				Text = "% Explosive Force  (SJ - SJl100%) / Dja";
				Color = colorFromRGB(209,63,58);
				break;
			case Types.CELAST:
				Text = "% Elastic Hability  (CMJ - SJ) / Dja";
				Color = colorFromRGB(255,152,68);
				break;
			case Types.CARMS:
				Text = "% Arms Hability  (ABK - CMJ) / Dja";
				Color = colorFromRGB(141,237,78);
				break;
			case Types.FREACT:
				Text = "% Reactive-reflex Hability  (DJa - ABK) / Dja";
				Color = colorFromRGB(133,190,199);
				break;
			default:
				Text = "% Maximum Force  SJl100% / DJa";
				Color = colorFromRGB(101,86,67);
				break;
		}
		
		this.jumpHigherName = jumpHigherName;
		this.jumpLowerName = jumpLowerName;
		
		ErrorMessage = "";
		Result = calculateIndex(type, higher, lower, dja);
		
		if(errorCode == ErrorCodes.NEEDJUMP)
			ErrorMessage = "Needs to execute jump/s";
		else if(errorCode == ErrorCodes.NEGATIVE)
			ErrorMessage = "Bad execution " + jumpLowerName + " is higher than " +  jumpHigherName;
	}

	private double calculateIndex (Types type, double higher, double lower, double dja) 
	{
		errorCode = ErrorCodes.NONE_OK;

		if(dja == 0 || higher == 0) {
			errorCode = ErrorCodes.NEEDJUMP;
			return 0;
		}

		if(type == Types.FMAX)	//this index only uses higher
			return higher / dja;

		if(lower == 0) {
			errorCode = ErrorCodes.NEEDJUMP;
			return 0;
		}

		if(lower > higher) {
			errorCode = ErrorCodes.NEGATIVE;
			return 0;
		}

		return (higher - lower) / dja;
	}
	
	private Cairo.Color colorFromRGB(int red, int green, int blue) {
		return new Cairo.Color(red/256.0, green/256.0, blue/256.0);
	}

}

public class JumpsProfile
{
	private JumpsProfileIndex jpi0;
	private JumpsProfileIndex jpi1;
	private JumpsProfileIndex jpi2;
	private JumpsProfileIndex jpi3;
	private JumpsProfileIndex jpi4;

	public JumpsProfile() {
	}

	public void Calculate (int personID, int sessionID)
	{
		List<Double> l = SqliteJump.SelectChronojumpProfile(personID, sessionID);

		double sj  = l[0];
		double sjl = l[1];
		double cmj = l[2];
		double abk = l[3];
		double dja = l[4];
		
		jpi0 = new JumpsProfileIndex(JumpsProfileIndex.Types.FMAX, "SJ", "", sjl, 0, dja);
		jpi1 = new JumpsProfileIndex(JumpsProfileIndex.Types.FEXPL, "SJ", "SJl", sj, sjl, dja);
		jpi2 = new JumpsProfileIndex(JumpsProfileIndex.Types.CELAST, "CMJ", "SJ", cmj, sj, dja);
		jpi3 = new JumpsProfileIndex(JumpsProfileIndex.Types.CARMS, "ABK", "CMJ", abk, cmj, dja);
		jpi4 = new JumpsProfileIndex(JumpsProfileIndex.Types.FREACT, "DJa", "ABK", dja, abk, dja);
	}

	public List<JumpsProfileIndex> GetIndexes()
	{
		List<JumpsProfileIndex> l = new List<JumpsProfileIndex>();
		l.Add(jpi0);
		l.Add(jpi1);
		l.Add(jpi2);
		l.Add(jpi3);
		l.Add(jpi4);
		return l;
	}
}
