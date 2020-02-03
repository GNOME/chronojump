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
 * Copyright (C) 2004-2019   Xavier de Blas <xaviblas@gmail.com> 
 */
using System;
using System.IO; 
using Gtk;
using Gdk;
using Glade;
using System.Collections;
using System.Collections.Generic; //List<T>
using Mono.Unix;


public class EncoderGraphDoPlot
{
	private Gtk.DrawingArea drawingarea;
	private Gdk.Pixmap pixmap;
	private Preferences preferences;


	private string mainVariable;
	private double mainVariableHigher;
	private double mainVariableLower;
	private string secondaryVariable;
	private bool capturing;
	private string eccon;
	private RepetitiveConditionsWindow repetitiveConditionsWin;
	private bool hasInertia;
	private bool playSoundsFromFile;
	private ArrayList data7Variables;
	private Gtk.ListStore encoderCaptureListStore;
	private double maxPowerIntersession;

	private int discardFirstN;
	private int showNRepetitions;
	private int graphWidth;
	private int graphHeight;
	private ArrayList data; //data is related to mainVariable (barplot)
	private ArrayList dataSecondary; //dataSecondary is related to secondary variable (by default range)

	private EncoderBarsLimits encoderBarsLimits;

	Pango.Layout layout_encoder_capture_curves_bars;
        Pango.Layout layout_encoder_capture_curves_bars_text; //e, c
        Pango.Layout layout_encoder_capture_curves_bars_superbig; //PlaySounds wewillrockyou

	Gdk.GC pen_black_encoder_capture;
	Gdk.GC pen_gray;
	Gdk.GC pen_gray_loss_bold;

	Gdk.GC pen_red_encoder_capture;
	Gdk.GC pen_red_dark_encoder_capture;
	Gdk.GC pen_red_light_encoder_capture;
	Gdk.GC pen_green_encoder_capture;
	Gdk.GC pen_green_dark_encoder_capture;
	Gdk.GC pen_green_light_encoder_capture;
	Gdk.GC pen_blue_encoder_capture;
	Gdk.GC pen_blue_dark_encoder_capture;
	Gdk.GC pen_blue_light_encoder_capture;
	Gdk.GC pen_yellow_encoder_capture;
	Gdk.GC pen_colors_foreground_encoder_capture;
	Gdk.GC pen_colors_background_encoder_capture;
	
	Gdk.GC pen_white_encoder_capture;
	Gdk.GC pen_selected_encoder_capture;
	Gdk.Colormap colormap;

	//R rainbow(30)
	string [] colorListFG = {
		"#4C00FF", "#2A00FF", "#0800FF", "#0019FF", "#003CFF",
		"#005DFF", "#0080FF", "#00A2FF", "#00C3FF", "#00E5FF",
		"#00FF4D", "#00FF2B", "#00FF08", "#1AFF00", "#3CFF00",
		"#5DFF00", "#80FF00", "#A2FF00", "#C3FF00", "#E6FF00",
		"#FFFF00", "#FFF514", "#FFEC28", "#FFE53C", "#FFE04F",
		"#FFDC63", "#FFDB77", "#FFDB8B", "#FFDD9F", "#FFE0B3"
	};

	//R c(cm.colors(15), rev(cm.colors(15)))
	string [] colorListBG = {
		"#80FFFF", "#92FFFF", "#A4FFFF", "#B6FFFF", "#C8FFFF", "#DBFFFF",
		"#EDFFFF", "#FFFFFF", "#FFEDFF", "#FFDBFF", "#FFC8FF", "#FFB6FF",
		"#FFA4FF", "#FF92FF", "#FF80FF", "#FF80FF", "#FF92FF", "#FFA4FF",
		"#FFB6FF", "#FFC8FF", "#FFDBFF", "#FFEDFF", "#FFFFFF", "#EDFFFF",
		"#DBFFFF", "#C8FFFF", "#B6FFFF", "#A4FFFF", "#92FFFF", "#80FFFF"
	};


	int colorListPos = 0;

	public EncoderGraphDoPlot (Preferences preferences, Gtk.DrawingArea drawingarea, Gdk.Pixmap pixmap)
	{
		NewGraphicObjects(drawingarea, pixmap);
		NewPreferences(preferences);

		graphPrepared = false;
		GraphPrepare(true);
	}

	public void NewGraphicObjects (Gtk.DrawingArea drawingarea, Gdk.Pixmap pixmap)
	{
		this.drawingarea = drawingarea;
		this.pixmap = pixmap;

		graphWidth = drawingarea.Allocation.Width;
		graphHeight = drawingarea.Allocation.Height;
	}

	//preferences can change	
	public void NewPreferences (Preferences preferences)
	{
		this.preferences = preferences;

		discardFirstN = preferences.encoderCaptureInertialDiscardFirstN;
		showNRepetitions = preferences.encoderCaptureShowNRepetitions;
		
		if(layout_encoder_capture_curves_bars != null)
			layout_encoder_capture_curves_bars.FontDescription =
				Pango.FontDescription.FromString ("Courier " + preferences.encoderCaptureBarplotFontSize.ToString());
	}

	public void Start (
			string mainVariable, double mainVariableHigher, double mainVariableLower,
			string secondaryVariable, bool capturing, string eccon,
			RepetitiveConditionsWindow repetitiveConditionsWin,
			bool hasInertia, bool playSoundsFromFile,
			ArrayList data7Variables, Gtk.ListStore encoderCaptureListStore, double maxPowerIntersession)
	{
		this.mainVariable = mainVariable;
		this.mainVariableHigher = mainVariableHigher;
		this.mainVariableLower = mainVariableLower;
		this.secondaryVariable = secondaryVariable;
		this.discardFirstN = discardFirstN;
		this.capturing = capturing;
		this.eccon = eccon;
		this.repetitiveConditionsWin = repetitiveConditionsWin;
		this.hasInertia = hasInertia;
		this.playSoundsFromFile = playSoundsFromFile;
		this.data7Variables = data7Variables;
        	this.encoderCaptureListStore = encoderCaptureListStore;
		this.maxPowerIntersession = maxPowerIntersession;

		graphWidth = drawingarea.Allocation.Width;
		graphHeight = drawingarea.Allocation.Height;
		encoderBarsLimits = new EncoderBarsLimits();
	
		fillDataVariables();
		plot();
	}

	private void fillDataVariables()
	{
		data = new ArrayList (data7Variables.Count); //data is related to mainVariable (barplot)
		dataSecondary = new ArrayList (data7Variables.Count); //dataSecondary is related to secondary variable (by default range)
		bool lastIsEcc = false;
		int count = 0;

		//discard repetitions according to showNRepetitions
		foreach(EncoderBarsData ebd in data7Variables)
		{
			//LogB.Information(string.Format("count: {0}, value: {1}", count, ebd.GetValue(mainVariable)));
			//when capture ended, show all repetitions
			if(showNRepetitions == -1 || ! capturing)
			{
				data.Add(ebd.GetValue(mainVariable));
				if(secondaryVariable != "")
					dataSecondary.Add(ebd.GetValue(secondaryVariable));
			}
			else {
				if(eccon == "c" && ( data7Variables.Count <= showNRepetitions || 	//total repetitions are less than show repetitions threshold ||
						count >= data7Variables.Count - showNRepetitions ) ) 	//count is from the last group of reps (reps that have to be shown)
				{
					data.Add(ebd.GetValue(mainVariable));
					if(secondaryVariable != "")
						dataSecondary.Add(ebd.GetValue(secondaryVariable));
				}
				else if(eccon != "c" && (
						data7Variables.Count <= 2 * showNRepetitions ||
						count >= data7Variables.Count - 2 * showNRepetitions) )
				{
					if(! Util.IsEven(count +1))  	//if it is "impar"
					{
						LogB.Information("added ecc");
						data.Add(ebd.GetValue(mainVariable));
						if(secondaryVariable != "")
							dataSecondary.Add(ebd.GetValue(secondaryVariable));
						lastIsEcc = true;
					} else {  			//it is "par"
						if(lastIsEcc)
						{
							data.Add(ebd.GetValue(mainVariable));
							if(secondaryVariable != "")
								dataSecondary.Add(ebd.GetValue(secondaryVariable));
							LogB.Information("added con");
							lastIsEcc = false;
						}
					}
				}
			}
			count ++;
		}
	}

	private bool graphPrepared;
	public void GraphPrepare(bool eraseBars)
	{
		if(graphPrepared)
			return;

		if(eraseBars && pixmap != null)
			UtilGtk.ErasePaint(drawingarea, pixmap);

		layout_encoder_capture_curves_bars = new Pango.Layout (drawingarea.PangoContext);
		layout_encoder_capture_curves_bars.FontDescription =
			Pango.FontDescription.FromString ("Courier " + preferences.encoderCaptureBarplotFontSize.ToString());
		
		layout_encoder_capture_curves_bars_text = new Pango.Layout (drawingarea.PangoContext);
		layout_encoder_capture_curves_bars_text.FontDescription = Pango.FontDescription.FromString ("Courier 10");

		layout_encoder_capture_curves_bars_superbig = new Pango.Layout (drawingarea.PangoContext);
		layout_encoder_capture_curves_bars_superbig.FontDescription = Pango.FontDescription.FromString ("Courier 300");

		//defined as drawingarea instead of encoder_capture_signal_drawingarea
		//because the 2nd is null if config.EncoderCaptureShowOnlyBars == TRUE
		pen_black_encoder_capture = new Gdk.GC(drawingarea.GdkWindow);
		pen_gray = new Gdk.GC(drawingarea.GdkWindow);
		pen_gray_loss_bold = new Gdk.GC(drawingarea.GdkWindow);
		pen_red_encoder_capture = new Gdk.GC(drawingarea.GdkWindow);
		pen_red_light_encoder_capture = new Gdk.GC(drawingarea.GdkWindow);
		pen_green_encoder_capture = new Gdk.GC(drawingarea.GdkWindow);
		pen_blue_encoder_capture = new Gdk.GC(drawingarea.GdkWindow);
		pen_white_encoder_capture = new Gdk.GC(drawingarea.GdkWindow);
		pen_selected_encoder_capture = new Gdk.GC(drawingarea.GdkWindow);
		pen_red_light_encoder_capture = new Gdk.GC(drawingarea.GdkWindow);
		pen_red_dark_encoder_capture = new Gdk.GC(drawingarea.GdkWindow);
		pen_green_light_encoder_capture = new Gdk.GC(drawingarea.GdkWindow);
		pen_green_dark_encoder_capture = new Gdk.GC(drawingarea.GdkWindow);
		pen_blue_dark_encoder_capture = new Gdk.GC(drawingarea.GdkWindow);
		pen_blue_light_encoder_capture = new Gdk.GC(drawingarea.GdkWindow);
		pen_yellow_encoder_capture = new Gdk.GC(drawingarea.GdkWindow);
		pen_colors_foreground_encoder_capture = new Gdk.GC(drawingarea.GdkWindow);
		pen_colors_background_encoder_capture = new Gdk.GC(drawingarea.GdkWindow);

		colormap = Gdk.Colormap.System;
		colormap.AllocColor (ref UtilGtk.BLACK,true,true);
		colormap.AllocColor (ref UtilGtk.GRAY,true,true);
		colormap.AllocColor (ref UtilGtk.RED_PLOTS,true,true);
		colormap.AllocColor (ref UtilGtk.RED_DARK,true,true);
		colormap.AllocColor (ref UtilGtk.RED_LIGHT,true,true);
		colormap.AllocColor (ref UtilGtk.GREEN_PLOTS,true,true);
		colormap.AllocColor (ref UtilGtk.GREEN_DARK,true,true);
		colormap.AllocColor (ref UtilGtk.GREEN_LIGHT,true,true);
		colormap.AllocColor (ref UtilGtk.BLUE_PLOTS,true,true);
		colormap.AllocColor (ref UtilGtk.BLUE_DARK,true,true);
		colormap.AllocColor (ref UtilGtk.BLUE_LIGHT,true,true);
		colormap.AllocColor (ref UtilGtk.YELLOW,true,true);
		colormap.AllocColor (ref UtilGtk.WHITE,true,true);
		colormap.AllocColor (ref UtilGtk.SELECTED,true,true);

		pen_black_encoder_capture.Foreground = UtilGtk.BLACK;
		pen_gray.Foreground = UtilGtk.GRAY;
		pen_gray_loss_bold.Foreground = UtilGtk.GRAY;
		pen_red_encoder_capture.Foreground = UtilGtk.RED_PLOTS;
		pen_red_dark_encoder_capture.Foreground = UtilGtk.RED_DARK;
		pen_red_light_encoder_capture.Foreground = UtilGtk.RED_LIGHT;
		pen_green_encoder_capture.Foreground = UtilGtk.GREEN_PLOTS;
		pen_green_dark_encoder_capture.Foreground = UtilGtk.GREEN_DARK;
		pen_green_light_encoder_capture.Foreground = UtilGtk.GREEN_LIGHT;
		pen_blue_encoder_capture.Foreground = UtilGtk.BLUE_PLOTS;
		pen_blue_dark_encoder_capture.Foreground = UtilGtk.BLUE_DARK;
		pen_blue_light_encoder_capture.Foreground = UtilGtk.BLUE_LIGHT;
		pen_yellow_encoder_capture.Foreground = UtilGtk.YELLOW;
		pen_white_encoder_capture.Foreground = UtilGtk.WHITE;
		pen_selected_encoder_capture.Foreground = UtilGtk.SELECTED;

		pen_black_encoder_capture.SetLineAttributes (2, Gdk.LineStyle.Solid, Gdk.CapStyle.NotLast, Gdk.JoinStyle.Miter);
		pen_selected_encoder_capture.SetLineAttributes (2, Gdk.LineStyle.Solid, Gdk.CapStyle.NotLast, Gdk.JoinStyle.Miter);
		pen_gray_loss_bold.SetLineAttributes (5, Gdk.LineStyle.Solid, Gdk.CapStyle.NotLast, Gdk.JoinStyle.Miter);

		graphPrepared = true;
	}

	//if we are capturing, play sounds
	private void plot()
	{
		UtilGtk.ErasePaint(drawingarea, pixmap);

		int count = 0;
		count = 0;

		//Get max min avg values of this set
		double maxThisSet = -100000;
		double minThisSet = 100000;

		//only used for loss. For loss only con phase is used
		double maxThisSetValidAndCon = maxThisSet;
		double minThisSetValidAndCon = minThisSet;
		//we need the position to draw the loss line and maybe to manage that the min should be after the max (for being real loss)
		int maxThisSetValidAndConPos = 0;
		int minThisSetValidAndConPos = 0;

		//know not-discarded phases
		double countValid = 0;
		double sumValid = 0;

		foreach(double d in data)
		{
			if(d > maxThisSet)
				maxThisSet = d;
			if(d < minThisSet)
				minThisSet = d;

			if( hasInertia && discardFirstN > 0 &&
					  ((eccon == "c" && count < discardFirstN) || (eccon != "c" && count < discardFirstN * 2)) )
				LogB.Information("Discarded phase");
			else {
				countValid ++;
				sumValid += d;
				bool needChangeMin = false;

				if(eccon == "c" || Util.IsEven(count +1)) //par
				{
					if(d > maxThisSetValidAndCon) {
						maxThisSetValidAndCon = d;
						maxThisSetValidAndConPos = count;

						//min rep has to be after max
						needChangeMin = true;
					}
					if(needChangeMin || d < minThisSetValidAndCon) {
						minThisSetValidAndCon = d;
						minThisSetValidAndConPos = count;
					}
				}
			}

			count ++;
		}
		if(maxThisSet <= 0)
			return;	

		double maxAbsolute = maxThisSet;
		if(! repetitiveConditionsWin.EncoderRelativeToSet)
		{
			//relative to historical of this person

			/*
			 *
			 * if there's a set captured but without repetitions saved, maxPowerIntersession will be 0
			 * and current set (loaded or captured) will have a power that will be out of the graph
			 * for this reason use maxAbsolute or maxThisSet, whatever is higher
			 */
			if(maxPowerIntersession > maxAbsolute)
				maxAbsolute = maxPowerIntersession;
		}

		//calculate maxAbsoluteSecondary (will be secondary variable)
		double maxAbsoluteSecondary = 0;
		foreach(double d in dataSecondary)
			if(d > maxAbsoluteSecondary)
				maxAbsoluteSecondary = d;

		repetitiveConditionsWin.ResetBestSetValue(RepetitiveConditionsWindow.BestSetValueEnum.CAPTURE_MAIN_VARIABLE);
		repetitiveConditionsWin.UpdateBestSetValue(
				RepetitiveConditionsWindow.BestSetValueEnum.CAPTURE_MAIN_VARIABLE, maxAbsolute);

		int textWidth = 1;
		int textHeight = 1;

		int left_margin = 10;
		int right_margin = 0;
		int top_margin = 20 + 3 * preferences.encoderCaptureBarplotFontSize;
		int bottom_margin = 8 + preferences.encoderCaptureBarplotFontSize;
		//bars will be plotted here
		int graphHeightSafe = graphHeight - (top_margin + bottom_margin);

		//plot bars
		int sep = 20;	//between reps

		if (data.Count >= 10 && data.Count < 20) {
			sep = 10;
			layout_encoder_capture_curves_bars.FontDescription =
				Pango.FontDescription.FromString ("Courier " + (preferences.encoderCaptureBarplotFontSize -2).ToString());
		} else if (data.Count >= 20) {
			sep = 2;
			layout_encoder_capture_curves_bars.FontDescription =
				Pango.FontDescription.FromString ("Courier " + (preferences.encoderCaptureBarplotFontSize -4).ToString());
			left_margin = 2;
		}

		if(playSoundsFromFile)
		{
			sep = 2;
			layout_encoder_capture_curves_bars.FontDescription =
				Pango.FontDescription.FromString ("Courier " + (preferences.encoderCaptureBarplotFontSize -4).ToString());
			left_margin = 2;
		}

		layout_encoder_capture_curves_bars_text.FontDescription =
			Pango.FontDescription.FromString ("Courier " + preferences.encoderCaptureBarplotFontSize.ToString());
		layout_encoder_capture_curves_bars_text.FontDescription.Weight = Pango.Weight.Bold;
		
		Rectangle rect;

		Gdk.GC my_pen_ecc_con_e; //ecc-con eccentric
		Gdk.GC my_pen_ecc_con_c; //ecc-con concentric
		Gdk.GC my_pen_con;	//concentric
		Gdk.GC my_pen;		//pen we are going to use

		int dLeft = 0;
		count = 0;
	
		//to show saved curves on DoPlot	
		TreeIter iter;
		
		//sum saved curves to do avg
		double sumSaved = 0; 
		double countSaved = 0;
		
		//draw line for person max intersession
		if(! repetitiveConditionsWin.EncoderRelativeToSet)
		{
			layout_encoder_capture_curves_bars_text.SetMarkup("Person's best:");
			layout_encoder_capture_curves_bars_text.GetPixelSize(out textWidth, out textHeight);
			pixmap.DrawLayout (pen_yellow_encoder_capture,
						left_margin, top_margin - textHeight,
						layout_encoder_capture_curves_bars_text);

			pixmap.DrawLine(pen_yellow_encoder_capture,
					left_margin, top_margin,
					graphWidth - right_margin, top_margin);

			layout_encoder_capture_curves_bars_text.SetMarkup(Util.TrimDecimals(maxAbsolute, 1) + "W");
			layout_encoder_capture_curves_bars_text.GetPixelSize(out textWidth, out textHeight);
			pixmap.DrawLayout (pen_yellow_encoder_capture,
						graphWidth - (right_margin + textWidth),
						top_margin - textHeight,
						layout_encoder_capture_curves_bars_text);
		}

		if(playSoundsFromFile) //TODO: move this to another function/file
		{
			Gdk.Color col = new Gdk.Color();

			//foreground
			Gdk.Color.Parse(colorListFG[colorListPos], ref col);
			colormap.AllocColor (ref col, true,true);
			pen_colors_foreground_encoder_capture.Foreground = col;

			//background
			Gdk.Color.Parse(colorListBG[colorListPos], ref col);
			colormap.AllocColor (ref col, true,true);
			pen_colors_background_encoder_capture.Foreground = col;

			colorListPos ++;
			if (colorListPos >= colorListFG.Length || colorListPos >= colorListBG.Length)
				colorListPos = 0;

			rect = new Rectangle(0, 0, graphWidth, graphHeight);
			pixmap.DrawRectangle(pen_colors_background_encoder_capture, true, rect);
		}

		int encoderBarsLimitsCount = 0;

		Gdk.Point dSecondaryPreviousPoint = new Gdk.Point(0,0);
		bool iterOk = encoderCaptureListStore.GetIterFirst(out iter);

		List<EncoderBarsResults> encoderBarsResults_l = new List<EncoderBarsResults>();
		int dWidth = 0;
		int dHeight = 0;
		int dBottom = 0;
		int dTop = 0;
		foreach(double dFor in data)
		{
			dWidth = 0;
			dHeight = 0;

			//if values are negative, invert it
			//this happens specially in the speeds in eccentric
			//we use dFor because we cannot change the iteration variable
			double d = dFor;
			if(d < 0)
				d *= -1;

			dHeight = Convert.ToInt32(graphHeightSafe * d / maxAbsolute * 1.0);
			dBottom = graphHeight - bottom_margin;
			dTop = dBottom - dHeight;


			if (data.Count == 1)	//do not fill all the screen with only one bar
				dWidth = Convert.ToInt32((graphWidth - left_margin - right_margin) / 2.0); 
			else
				dWidth = Convert.ToInt32((graphWidth - left_margin - right_margin) / data.Count * 1.0);

			dLeft = left_margin + dWidth * count;
		
			//dWidth = dWidth - sep to have separation between bars
			//but if eccon != "c" then have like this: ec ec ec
			if (eccon == "c") {
				dWidth = dWidth - sep;
			} else {
				double sep_ec_mult = 1.2;
				dWidth = Convert.ToInt32(dWidth - sep * sep_ec_mult);

				if(Util.IsEven(count +1)) //par
					dLeft = Convert.ToInt32(dLeft - sep * sep_ec_mult);
			}
			//just in case there are too much bars
			if(dWidth < 1)
				dWidth = 1;

			//select pen color for bars and sounds
			string myColor = repetitiveConditionsWin.AssignColorAutomatic(
					RepetitiveConditionsWindow.BestSetValueEnum.CAPTURE_MAIN_VARIABLE, d);

			bool discarded = false;
			if(hasInertia) {
				if(eccon == "c" && discardFirstN > 0 && count < discardFirstN)
					discarded = true;
				else if(eccon != "c" && discardFirstN > 0 && count < discardFirstN * 2)
					discarded = true;
			}

			if( ! discarded && ( myColor == UtilGtk.ColorGood || (mainVariableHigher != -1 && d >= mainVariableHigher) ) )
			{
				my_pen_ecc_con_e = pen_green_dark_encoder_capture;
				my_pen_ecc_con_c = pen_green_light_encoder_capture;
				my_pen_con = pen_green_encoder_capture;
				//play sound if value is high, volumeOn == true, is last value, capturing
				if(preferences.volumeOn && count == data.Count -1 && capturing)
					Util.PlaySound(Constants.SoundTypes.GOOD, preferences.volumeOn, preferences.gstreamer);
			}
			else if( ! discarded && ( myColor == UtilGtk.ColorBad || (mainVariableLower != -1 && d <= mainVariableLower) ) )
			{
				my_pen_ecc_con_e = pen_red_dark_encoder_capture;
				my_pen_ecc_con_c = pen_red_light_encoder_capture;
				my_pen_con = pen_red_encoder_capture;
				//play sound if value is low, volumeOn == true, is last value, capturing
				if(preferences.volumeOn && count == data.Count -1 && capturing)
					Util.PlaySound(Constants.SoundTypes.BAD, preferences.volumeOn, preferences.gstreamer);
			}
			else {
				my_pen_ecc_con_e = pen_blue_dark_encoder_capture;
				my_pen_ecc_con_c = pen_blue_light_encoder_capture;
				my_pen_con = pen_blue_encoder_capture;
			}
			
			//know if ecc or con to paint with dark or light pen
			if (eccon == "ec" || eccon == "ecS") {
				bool isEven = Util.IsEven(count +1);
				
				//on inertial if discardFirstN , they have to be gray
				if( hasInertia && discardFirstN > 0 &&
						((eccon == "c" && count < discardFirstN) || (eccon != "c" && count < discardFirstN * 2)) )
					my_pen = pen_gray;
				else {
					if(isEven) //par, concentric
						my_pen = my_pen_ecc_con_c;
					else
						my_pen = my_pen_ecc_con_e;
				}
			} else {
				if( hasInertia && discardFirstN > 0 &&
						((eccon == "c" && count < discardFirstN) || (eccon != "c" && count < discardFirstN * 2)) )
					my_pen = pen_gray;
				else
					my_pen = my_pen_con;
			}

			if(playSoundsFromFile)
				my_pen = pen_colors_foreground_encoder_capture;

			//paint bar:	
			rect = new Rectangle(dLeft, dTop, dWidth, dHeight);
			pixmap.DrawRectangle(my_pen, true, rect);

			encoderBarsLimits.Add(encoderBarsLimitsCount ++, dLeft, dLeft + dWidth); //first rep is 1

			//paint diagonal line to distinguish eccentric-concentric
			//line is painted before the black outline to fix graphical problems
			if (eccon == "ec" || eccon == "ecS") {
				bool isEven = Util.IsEven(count +1);

				if(isEven) {
					//to see if it is ok
					//pixmap.DrawPoint(pen_green_encoder_capture, dLeft +1, dBottom -2);
					//pixmap.DrawPoint(pen_yellow_encoder_capture, dLeft + dWidth -2, dTop +1);
					pixmap.DrawLine(pen_white_encoder_capture,
							dLeft, dBottom -1, dLeft + dWidth -2, dTop +1);
				} else {
					//pixmap.DrawPoint(pen_green_encoder_capture, dLeft +1, dTop +1);
					//pixmap.DrawPoint(pen_yellow_encoder_capture, dLeft + dWidth -2, dBottom -2);
					pixmap.DrawLine(pen_white_encoder_capture,
							dLeft +1, dTop +1, dLeft + dWidth -2, dBottom -2);
				}
			}
			//paint black outline line on bar
			pixmap.DrawRectangle(pen_black_encoder_capture, false, rect);

			//paint secondary variable circle and lines
			//but do not do it if user do not want to show it from repetitiveConditionsWindow
			if(dataSecondary.Count > 0)
			{
				double dSecondary = Convert.ToDouble(dataSecondary[count]);
				int dSecondaryHeight = UtilAll.DivideSafeAndGetInt(graphHeightSafe * dSecondary, maxAbsoluteSecondary * 1.0);
				int dSecondaryTop = dBottom - dSecondaryHeight;
				Gdk.Point dSecondaryCurrentPoint = new Gdk.Point(Convert.ToInt32(dLeft + (dWidth /2)), dSecondaryTop);

				//LogB.Information(string.Format("dSecondaryHeight: {0}; dSecondaryTop: {1}", dSecondaryHeight, dSecondaryTop));

				pixmap.DrawArc(pen_yellow_encoder_capture, true,
						dSecondaryCurrentPoint.X -6, dSecondaryCurrentPoint.Y -6,
						12, 12, 90 * 64, 360 * 64);

				if(dSecondaryPreviousPoint.X != 0 && dSecondaryPreviousPoint.Y != 0)
					pixmap.DrawLine(pen_yellow_encoder_capture,
							dSecondaryPreviousPoint.X, dSecondaryPreviousPoint.Y, dSecondaryCurrentPoint.X, dSecondaryCurrentPoint.Y);

				dSecondaryPreviousPoint = dSecondaryCurrentPoint;
			}


			//store values to write later the (not being overlapped) result
			encoderBarsResults_l.Add( new EncoderBarsResults (
						Convert.ToInt32(dLeft + dWidth/2), 			// x = this - textWidth/2
						dTop - (5 + preferences.encoderCaptureBarplotFontSize), // y
						d ) );
			
			bool curveSaved = false;	
			if( iterOk && ((EncoderCurve) encoderCaptureListStore.GetValue (iter, 0)).Record ) {
				curveSaved = true;
				sumSaved += dFor;
				countSaved ++;
			}
			
			//add text on the bottom
			if (eccon == "c" || Util.IsEven(count +1)) //par
			{
				int startX = Convert.ToInt32(dLeft + dWidth/2);
				string bottomText = (count +1).ToString();
				if(showNRepetitions > 0 && capturing && data7Variables.Count > showNRepetitions)
					bottomText = ( (data7Variables.Count - showNRepetitions) + count +1).ToString();

				if (eccon != "c") {
					startX = dLeft;
					bottomText = ((count +1) / 2).ToString();
				}

				layout_encoder_capture_curves_bars.SetMarkup(bottomText);
				textWidth = 1;
				textHeight = 1;
				layout_encoder_capture_curves_bars.GetPixelSize(out textWidth, out textHeight); 
				int myX = Convert.ToInt32( startX - textWidth/2);
				int myY = Convert.ToInt32(dTop + dHeight + (bottom_margin /2) - textHeight/2);
			
				if(curveSaved) {
					rect = new Rectangle(myX -2, myY -1, textWidth +4, graphHeight - (myY -1) -1);
					pixmap.DrawRectangle(pen_selected_encoder_capture, false, rect);
				}
				
				//write the text
				pixmap.DrawLayout (pen_black_encoder_capture, 
						myX, myY,
						layout_encoder_capture_curves_bars);
			}

			count ++;
			iterOk = encoderCaptureListStore.IterNext (ref iter);
		}
		//end plot bars
	

		//plot title
		string units = "";
		int decimals;
		
		if(mainVariable == Constants.MeanSpeed || mainVariable == Constants.MaxSpeed) {
			units = "m/s";
			decimals = 2;
		} else if(mainVariable == Constants.MeanForce || mainVariable == Constants.MaxForce) {
			units = "N";
			decimals = 1;
		}
		else { //powers
			units =  "W";
			decimals = 1;
		}

		//LogB.Information(string.Format("sumValid: {0}, countValid: {1}, div: {2}", sumValid, countValid, sumValid / countValid));
		//LogB.Information(string.Format("sumSaved: {0}, countSaved: {1}, div: {2}", sumSaved, countSaved, sumSaved / countSaved));

		//add avg and avg of saved values
		string title = mainVariable + " [X = " + 
			Util.TrimDecimals( (sumValid / countValid), decimals) +
			" " + units;

		if(countSaved > 0)
			title += "; X" + Catalog.GetString("saved") + " = " + 
				Util.TrimDecimals( (sumSaved / countSaved), decimals) + 
				" " + units;

		string lossString = "; Loss: ";
		if(eccon != "c")
			lossString = "; Loss (con): "; //on ecc/con use only con for loss calculation

		if(maxThisSetValidAndCon > 0)
		{
			title += lossString + Util.TrimDecimals(
					100.0 * (maxThisSetValidAndCon - minThisSetValidAndCon) / maxThisSetValidAndCon, decimals) + "%";
			LogB.Information(string.Format("Loss at plot: {0}", 100.0 * (maxThisSetValidAndCon - minThisSetValidAndCon) / maxThisSetValidAndCon));

			if(maxThisSetValidAndConPos < minThisSetValidAndConPos)
			{
				/*
				 * at bucle dLeft is calculed using dWidth
				 * 	dLeft = left_margin + dWidth * count;
				 * but then dWidth changes on c and on ec. On c:
				 * 	dWidth = dWidth - sep
				 * so here, to calcule the needed dLeft, use: dWidth + sep
				 */
				int x0 = Convert.ToInt32(left_margin + (dWidth + sep) * maxThisSetValidAndConPos + (dWidth/2));
				int y0 = Convert.ToInt32(dBottom - UtilAll.DivideSafeAndGetInt(graphHeightSafe * maxThisSetValidAndCon, maxAbsolute * 1.0));
				int x1 = Convert.ToInt32(left_margin + (dWidth + sep) * minThisSetValidAndConPos + (dWidth/2));
				int y1 = Convert.ToInt32(dBottom - UtilAll.DivideSafeAndGetInt(graphHeightSafe * minThisSetValidAndCon, maxAbsolute * 1.0));

				pixmap.DrawLine(pen_gray_loss_bold, x0, y0, x1, y1);
				UtilGtk.DrawArrow(pixmap, pen_gray_loss_bold, x1, x0, y1, y0, 20);
			}
		}

		title += "]";

		layout_encoder_capture_curves_bars_text.SetMarkup(title);
		textWidth = 1;
		textHeight = 1;
		layout_encoder_capture_curves_bars_text.GetPixelSize(out textWidth, out textHeight);
		pixmap.DrawLayout (pen_black_encoder_capture, 
				Convert.ToInt32( (graphWidth/2) - textWidth/2), 0, //x, y 
				layout_encoder_capture_curves_bars_text);

		//end plot title	

		//plot the values of the bars
		foreach(EncoderBarsResults ebr in encoderBarsResults_l)
		{
			if(mainVariable == Constants.MeanSpeed || mainVariable == Constants.MaxSpeed)
				layout_encoder_capture_curves_bars.SetMarkup(Util.TrimDecimals(ebr.d,2));
			else //force and powers
				layout_encoder_capture_curves_bars.SetMarkup(Util.TrimDecimals(ebr.d,0));

			textWidth = 1;
			textHeight = 1;
			layout_encoder_capture_curves_bars.GetPixelSize(out textWidth, out textHeight);
			pixmap.DrawLayout (pen_black_encoder_capture, ebr.x - Convert.ToInt32(textWidth/2), ebr.y, layout_encoder_capture_curves_bars);
		}

		//display We Will Rock You words
		if( playSoundsFromFile && (Util.SoundIsPum() || Util.SoundIsPam()) ) //TODO: move this to another function/file
		{
			string titleSound = "PUM";
			if(Util.SoundIsPam())
				titleSound = "PAM";

			layout_encoder_capture_curves_bars_superbig.SetMarkup(titleSound);
			textWidth = 1;
			textHeight = 1;
			layout_encoder_capture_curves_bars_superbig.GetPixelSize(out textWidth, out textHeight);

			//rect = new Rectangle(dLeft, dTop, dWidth, dHeight);
			rect = new Rectangle(
					Convert.ToInt32( (graphWidth/2) - (textWidth/2) -30),
					Convert.ToInt32( (graphHeight/2) - (textHeight/2) ), //textHeight is too high on Courier font 300
					textWidth + 60,
					textHeight);
			pixmap.DrawRectangle(pen_black_encoder_capture, true, rect);

			pixmap.DrawLayout (pen_red_light_encoder_capture,
					Convert.ToInt32( (graphWidth/2) - textWidth/2),
					Convert.ToInt32( (graphHeight/2) - textHeight/2),
					layout_encoder_capture_curves_bars_superbig);
		}

		drawingarea.QueueDraw(); 			// -- refresh
		drawingarea.Visible = true;
	}


	public void Erase()
	{
		UtilGtk.ErasePaint(drawingarea, pixmap);
	}

	public void ShowMessage(string message) 
	{
		Pango.Layout layout_message = new Pango.Layout (drawingarea.PangoContext);
		layout_message.FontDescription = Pango.FontDescription.FromString ("Courier 10");
		
		int graphWidth = drawingarea.Allocation.Width;
		int graphHeight = drawingarea.Allocation.Height;

		layout_message.SetMarkup(message);
		int textWidth = 1;
		int textHeight = 1;
		layout_message.GetPixelSize(out textWidth, out textHeight); 
		
		int xStart = Convert.ToInt32(graphWidth/2 - textWidth/2);
		int yStart = Convert.ToInt32(graphHeight/2 - textHeight/2);

		//draw horizontal line behind (across all graph)
		Rectangle rect = new Rectangle(0, yStart + textHeight -1, graphWidth, 1);
		pixmap.DrawRectangle(pen_yellow_encoder_capture, true, rect);

		//draw rectangle behind text
		rect = new Rectangle(xStart -2, yStart -2, textWidth +2, textHeight +2);
		pixmap.DrawRectangle(pen_yellow_encoder_capture, true, rect);
		
		//write text inside
		pixmap.DrawLayout (pen_black_encoder_capture, xStart, yStart, layout_message);
	}

	public int FindBarInPixel (double pixel)
	{
		if(encoderBarsLimits == null)
			return -1;

		return encoderBarsLimits.FindBarInPixel(pixel);
	}

	public bool GraphPrepared {
		get { return graphPrepared; }
	}
}	

/*
 * to store the result of each bar
 * in order to be drawn at the end
 * for not being overlapped with other info
 */
public class EncoderBarsResults
{
	public int x;
	public int y;
	public double d;

	public EncoderBarsResults(int x, int y, double d)
	{
		this.x = x;
		this.y = y;
		this.d = d;
	}
}

//to store the xStart and xEnd of every encoder capture reptition
//in order to be saved or not on clicking screen
//note every rep will be c or ec
public class EncoderBarsLimits
{
	private List<PointStartEnd> list;

	public EncoderBarsLimits()
	{
		list = new List<PointStartEnd>();
	}

	public void Add (int id, double start, double end)
	{
		PointStartEnd p = new PointStartEnd(id, start, end);
		list.Add(p);
		LogB.Information("Added: " + p.ToString());
	}

	public int FindBarInPixel (double pixel)
	{
		foreach(PointStartEnd p in list)
			if(pixel >= p.Start && pixel <= p.End)
				return p.Id;

		return -1;
	}
}
