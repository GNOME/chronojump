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
using Gtk;
using Cairo;
using System.Collections.Generic; //List

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

	public JumpsProfileIndex(Types type, string jumpHigherName, string jumpLowerName, double higher, double lower, double dja) 
	{
		//colour palette: http://www.colourlovers.com/palette/7991/%28not_so%29_still_life	
		this.type = type;
		switch(type) {
			case Types.FMAX:
				Text = "% F. Maximum  SJ100% / DJa";
				Color = colorFromRGB(90,68,102);
				break;
			case Types.FEXPL:
				Text = "% F. Explosive  (SJ - SJ100%) / Dja";
				Color = colorFromRGB(240,57,43);
				break;
			case Types.CELAST:
				Text = "% Hab. Elastic  (CMJ - SJ) / Dja";
				Color = colorFromRGB(254,176,20);
				break;
			case Types.CARMS:
				Text = "% Hab. Arms  (ABK - CMJ) / Dja";
				Color = colorFromRGB(250,209,7);
				break;
			case Types.FREACT:
				Text = "% F. Reactive-reflex  (DJa - ABK) / Dja";
				Color = colorFromRGB(235,235,207);
				break;
			default:
				Text = "% F. Maximum  SJ100% / DJa";
				Color = colorFromRGB(90,68,102);
				break;
		}
		
		this.jumpHigherName = jumpHigherName;
		this.jumpLowerName = jumpLowerName;
		
		ErrorMessage = "";
		Result = calculate(type, higher, lower, dja);
		
		if(errorCode == ErrorCodes.NEEDJUMP)
			ErrorMessage = "\nNeeds to execute jump/s";
		else if(errorCode == ErrorCodes.NEGATIVE)
			ErrorMessage = "\nBad execution " + jumpLowerName + " is higher than " +  jumpHigherName;
	}

	public double calculate(Types type, double higher, double lower, double dja) 
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

public class JumpsProfileGraph
{
	private JumpsProfileIndex jpi1;
	private JumpsProfileIndex jpi2;
	private JumpsProfileIndex jpi3;
	private JumpsProfileIndex jpi4;
	private JumpsProfileIndex jpi5;

	public JumpsProfileGraph() {
	}

	public void Calculate (int personID, int sessionID)
	{
		List<Double> l = SqliteStat.SelectChronojumpProfile(personID, sessionID);

		double sj = l[0];
		double sjl = l[1];
		double cmj = l[2];
		double abk = l[3];
		double dja = l[4];
		
		jpi1 = new JumpsProfileIndex(JumpsProfileIndex.Types.FMAX, "SJ", "", sjl, 0, dja);
		jpi2 = new JumpsProfileIndex(JumpsProfileIndex.Types.FEXPL, "SJ", "SJl", sj, sjl, dja);
		jpi3 = new JumpsProfileIndex(JumpsProfileIndex.Types.CELAST, "CMJ", "SJ", cmj, sj, dja);
		jpi4 = new JumpsProfileIndex(JumpsProfileIndex.Types.CARMS, "ABK", "CMJ", abk, cmj, dja);
		jpi5 = new JumpsProfileIndex(JumpsProfileIndex.Types.FREACT, "DJa", "ABK", dja, abk, dja);
	}
		
	public void Graph (DrawingArea area) 
	{
		Cairo.Context g = Gdk.CairoHelper.Create (area.GdkWindow);
		
		//clear (white)
		g.SetSourceRGB(1,1,1);
		g.Paint();

		g.SelectFontFace("Helvetica", Cairo.FontSlant.Normal, Cairo.FontWeight.Normal);
		int textHeight = 12;
		g.SetFontSize(textHeight);

		double sum = jpi1.Result + jpi2.Result + jpi3.Result + jpi4.Result + jpi5.Result;
		if(sum == 0)
			return;

		double acc = 0; //accumulated
		
		double percent = 2 * jpi1.Result / sum; //*2 to be in range 0*pi - 2*pi
		plotArc(200, 200, 150, acc, acc + percent, g, jpi1.Color);

		acc += percent;
		percent = 2 * jpi2.Result / sum; //*2 to be in range 0*pi - 2*pi
		plotArc(200, 200, 150, acc, acc + percent, g, jpi2.Color);

		acc += percent;
		percent = 2 * jpi3.Result / sum; //*2 to be in range 0*pi - 2*pi
		plotArc(200, 200, 150, acc, acc + percent, g, jpi3.Color);

		acc += percent;
		percent = 2 * jpi4.Result / sum; //*2 to be in range 0*pi - 2*pi
		plotArc(200, 200, 150, acc, acc + percent, g, jpi4.Color);

		acc += percent;
		percent = 2 * jpi5.Result / sum; //*2 to be in range 0*pi - 2*pi
		plotArc(200, 200, 150, acc, acc + percent, g, jpi5.Color);

		int width = 40;
		int height = 24;
		//R seq(from=50,to=(350-24),length.out=5)
		//[1] 50 119 188 257 326 
		drawRoundedRectangle (400,  50, width, height, 6, g, jpi1.Color);
		drawRoundedRectangle (400, 119, width, height, 6, g, jpi2.Color);
		drawRoundedRectangle (400, 188, width, height, 6, g, jpi3.Color);
		drawRoundedRectangle (400, 257, width, height, 6, g, jpi4.Color);
		drawRoundedRectangle (400, 326, width, height, 6, g, jpi5.Color);
	
		printText(460,  50, height, textHeight, Util.TrimDecimals((100 * jpi1.Result / sum),1) + jpi1.Text, g);
		printText(460, 119, height, textHeight, Util.TrimDecimals((100 * jpi2.Result / sum),1) + jpi2.Text, g);
		printText(460, 188, height, textHeight, Util.TrimDecimals((100 * jpi3.Result / sum),1) + jpi3.Text, g);
		printText(460, 257, height, textHeight, Util.TrimDecimals((100 * jpi4.Result / sum),1) + jpi4.Text, g);
		printText(460, 326, height, textHeight, Util.TrimDecimals((100 * jpi5.Result / sum),1) + jpi5.Text, g);
		
		//print errors (if any)
		g.Color = new Cairo.Color (0.5,0,0);
		
		printText(460,  70, height, textHeight, jpi1.ErrorMessage, g);
		printText(460, 139, height, textHeight, jpi2.ErrorMessage, g);
		printText(460, 208, height, textHeight, jpi3.ErrorMessage, g);
		printText(460, 277, height, textHeight, jpi4.ErrorMessage, g);
		printText(460, 346, height, textHeight, jpi5.ErrorMessage, g);

		g.GetTarget().Dispose ();
		g.Dispose ();
	}
	
	private void plotArc(int centerx, int centery, int radius, double start, double end, 
			Cairo.Context g, Cairo.Color color) 
	{
		//pie chart
		g.MoveTo (centerx,  centery);
		g.Arc(centerx, centery, radius, start * Math.PI, end * Math.PI);
		g.ClosePath();
		g.Color = color;
		g.FillPreserve ();

		g.Color = new Cairo.Color (0,0,0);
		g.LineWidth = 2;
		g.Stroke ();
	}
	
	private void printText(int x, int y, int height, int textHeight, string text, Cairo.Context g) {
		g.MoveTo(x, ((y+y+height)/2) + textHeight/2.0);
		g.ShowText(text);
	}

	//http://www.mono-project.com/docs/tools+libraries/libraries/Mono.Cairo/cookbook/	
	private void drawRoundedRectangle (double x, double y, double width, double height, 
			double radius, Cairo.Context g, Cairo.Color color)
	{
		g.Save ();

		if ((radius > height / 2) || (radius > width / 2))
			radius = min (height / 2, width / 2);

		g.MoveTo (x, y + radius);
		g.Arc (x + radius, y + radius, radius, Math.PI, -Math.PI / 2);
		g.LineTo (x + width - radius, y);
		g.Arc (x + width - radius, y + radius, radius, -Math.PI / 2, 0);
		g.LineTo (x + width, y + height - radius);
		g.Arc (x + width - radius, y + height - radius, radius, 0, Math.PI / 2);
		g.LineTo (x + radius, y + height);
		g.Arc (x + radius, y + height - radius, radius, Math.PI / 2, Math.PI);
		g.ClosePath ();
		g.Restore ();
		
		g.Color = color;
		g.FillPreserve ();
		g.Color = new Cairo.Color (0, 0, 0);
		g.LineWidth = 2;
		g.Stroke ();	
	}

	private double min (params double[] arr)
	{
		int minp = 0;
		for (int i = 1; i < arr.Length; i++)
			if (arr[i] < arr[minp])
				minp = i;

		return arr[minp];
	}

	//save to png with http://www.mono-project.com/docs/tools+libraries/libraries/Mono.Cairo/tutorial/ stroke
	/*
	ImageSurface surface = new ImageSurface(Format.ARGB32, 120, 120);
	Context cr = new Context(surface);
	// Examples are in 1.0 x 1.0 coordinate space
	cr.Scale(120, 120);

	// Drawing code goes here
	cr.LineWidth = 0.1;

	//cr.Color = new Color(0, 0, 0); #deprecated, use this:
	cr.SetSourceRGBA(0, 0, 0, 1);

	cr.Rectangle(0.25, 0.25, 0.5, 0.5);
	cr.Stroke();

	surface.WriteToPng("stroke.png");
	*/

}
