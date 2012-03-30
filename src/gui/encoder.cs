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
 * Copyright (C) 2004-2012   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
using Gtk;
using Gdk;
using Glade;

public partial class ChronoJumpWindow 
{
	[Widget] Gtk.SpinButton spin_encoder_bar_limit;
	[Widget] Gtk.SpinButton spin_encoder_jump_limit;
	[Widget] Gtk.SpinButton spin_encoder_analyze_smooth;

	[Widget] Gtk.Button button_encoder_capture;
	[Widget] Gtk.Label label_encoder_person_weight;
	[Widget] Gtk.RadioButton radiobutton_encoder_concentric;
	[Widget] Gtk.RadioButton radiobutton_encoder_capture_bar;
	[Widget] Gtk.Viewport viewport_image_encoder_capture;
	[Widget] Gtk.Image image_encoder_bell;
	[Widget] Gtk.SpinButton spin_encoder_capture_time;
	[Widget] Gtk.SpinButton spin_encoder_capture_min_height;
	[Widget] Gtk.Image image_encoder_capture;
	
	[Widget] Gtk.RadioButton radiobutton_encoder_analyze_powerbars;
	[Widget] Gtk.RadioButton radiobutton_encoder_analyze_single;
	[Widget] Gtk.RadioButton radiobutton_encoder_analyze_side;
	[Widget] Gtk.RadioButton radiobutton_encoder_analyze_superpose;
	[Widget] Gtk.Label label_encoder_analyze_curve_num;
	[Widget] Gtk.SpinButton spin_encoder_analyze_curve_num;
	[Widget] Gtk.Viewport viewport_image_encoder_analyze;
	[Widget] Gtk.Image image_encoder_analyze;


	private string encoderEC="c";	//"c" or "ec"
	private string encoderAnalysis="powerBars";

//TODO: on capture only show ups if concentric

	//TODO: que el curve no pugui ser mes alt de actual numero de curves, per tant s'ha de retornar algun valor. ha de canviar cada cop que hi ha un capture o recalculate

	//TODO: campanes a l'encoder pq mostri colors i sons en funcio del que passa
	//TODO:recording time a main options (entre weights i smoothing)
	//TODO: curves amb par mar mes gran
	
	public void on_radiobutton_encoder_capture_bar_toggled (object obj, EventArgs args) {
		spin_encoder_bar_limit.Sensitive = true;
		spin_encoder_jump_limit.Sensitive = false;
	}
	public void on_radiobutton_encoder_capture_jump_toggled (object obj, EventArgs args) {
		spin_encoder_bar_limit.Sensitive = false;
		spin_encoder_jump_limit.Sensitive = true;
	}

	//TODO: garantir path windows	
	void on_button_encoder_capture_clicked (object o, EventArgs args) 
	{
		//TODO: que surti barra de progres de calculando... despres de capturar i boto de cerrar automatico
		//TODO: i mostrar valors des de la gui (potser a zona dreta damunt del zoom)
		

		//capture data
		EncoderParams ep = new EncoderParams(
				(int) spin_encoder_capture_time.Value, 
				findMass(),
				Util.ConvertToPoint((double) spin_encoder_analyze_smooth.Value)); //R decimal: '.'

		EncoderStruct es = new EncoderStruct(
				"",					//no data input
				"",					//no graph ouptut
				Util.GetEncoderDataTempFileName(), "", ep);				

		Util.RunPythonEncoder(Constants.EncoderScriptCapture, es, true);

		makeCurvesGraph();
	}
	
	void on_button_encoder_recalculate_clicked (object o, EventArgs args) 
	{
		makeCurvesGraph();
	}

	private void makeCurvesGraph() 
	{
		if(radiobutton_encoder_concentric.Active)
			encoderEC = "c";
		else
			encoderEC = "ec";
		
	      	//show curves graph
		int w = UtilGtk.WidgetWidth(viewport_image_encoder_capture)-2; //image is inside (is smaller than) viewport
		int h = UtilGtk.WidgetHeight(viewport_image_encoder_capture)-2;

		EncoderParams ep = new EncoderParams(
				(int) spin_encoder_capture_min_height.Value, 
				false,			//isJump (1st) is not used in "curves"
				findMass(),
				encoderEC, "curves",
				"0", 0, w, h); 		//smoothOne, and curve are not used in "curves"

		EncoderStruct es = new EncoderStruct(
				Util.GetEncoderDataTempFileName(), 
				Util.GetEncoderGraphTempFileName(),
				"NULL", "NULL", ep);		//no data ouptut
		
		Util.RunPythonEncoder(Constants.EncoderScriptGraphCall, es,false);

		Pixbuf pixbuf = new Pixbuf (Util.GetEncoderGraphTempFileName()); //from a file
		image_encoder_capture.Pixbuf = pixbuf;
	}
	
	private string findMass() {
		double mass = 0;
		if(radiobutton_encoder_capture_bar.Active)
			mass = spin_encoder_bar_limit.Value;
		else
			mass = Convert.ToDouble(label_encoder_person_weight.Text) + spin_encoder_jump_limit.Value;

		return Util.ConvertToPoint(mass); //R decimal: '.'
	}
	
	//show curve_num only on simple and superpose
	public void on_radiobutton_encoder_analyze_single_toggled (object obj, EventArgs args) {
		label_encoder_analyze_curve_num.Sensitive=true;
		spin_encoder_analyze_curve_num.Sensitive=true;
		encoderAnalysis="single";
	}

	public void on_radiobutton_encoder_analyze_superpose_toggled (object obj, EventArgs args) {
		label_encoder_analyze_curve_num.Sensitive=true;
		spin_encoder_analyze_curve_num.Sensitive=true;
		encoderAnalysis="superpose";
	}
	public void on_radiobutton_encoder_analyze_side_toggled (object obj, EventArgs args) {
		label_encoder_analyze_curve_num.Sensitive=false;
		spin_encoder_analyze_curve_num.Sensitive=false;
		encoderAnalysis="side";
	}
	public void on_radiobutton_encoder_analyze_powerbars_toggled (object obj, EventArgs args) {
		label_encoder_analyze_curve_num.Sensitive=false;
		spin_encoder_analyze_curve_num.Sensitive=false;
		encoderAnalysis="powerBars";
	}

	//TODO: garantir path windows	
	private void on_button_encoder_analyze_clicked (object o, EventArgs args) 
	{
		double mass = 0;
		if(radiobutton_encoder_capture_bar.Active)
			mass = spin_encoder_bar_limit.Value;
		else
			mass = Convert.ToDouble(label_encoder_person_weight.Text) + spin_encoder_jump_limit.Value;

		if(radiobutton_encoder_concentric.Active)
			encoderEC = "c";
		else
			encoderEC = "ec";

		int w = UtilGtk.WidgetWidth(viewport_image_encoder_analyze)-2; //image is inside (is smaller than) viewport
		int h = UtilGtk.WidgetHeight(viewport_image_encoder_analyze)-2;

		EncoderParams ep = new EncoderParams(
				(int) spin_encoder_capture_min_height.Value, 
				!radiobutton_encoder_capture_bar.Active,
				mass,
				encoderEC, encoderAnalysis,
				Util.ConvertToPoint((double) spin_encoder_analyze_smooth.Value), //R decimal: '.'
				(int) spin_encoder_analyze_curve_num.Value, w, h);

		EncoderStruct es = new EncoderStruct(
				Util.GetEncoderDataTempFileName(), 
				Util.GetEncoderGraphTempFileName(),
				"NULL", "NULL", ep);		//no data ouptut

		Util.RunPythonEncoder(Constants.EncoderScriptGraphCall, es, false);

		//TODO pensar en si s'ha de fer 1er amb mida petita i despres amb gran (en el zoom), o si es una sola i fa alguna edicio
		Pixbuf pixbuf = new Pixbuf (Util.GetEncoderGraphTempFileName()); //from a file
		image_encoder_analyze.Pixbuf = pixbuf;
	}

}	

