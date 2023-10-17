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
 * Copyright (C) 2023   Xavier de Blas <xaviblas@gmail.com>
 */

using System;
using Gtk;
using Gdk;
using System.Collections.Generic; //List<T>
//

/*
 * code for analyze gui on:
 * - forceSensor
 * -  raceAnalyzer
 */

public partial class ChronoJumpWindow
{
	// at glade ---->
	Gtk.Box box_ai_move_cd_accept;
	Gtk.Box box_ai_move_cd_buttons;

	Gtk.Button button_ai_move_cd_pre_1s;
	Gtk.Button button_ai_move_cd_pre_1ds;
	Gtk.Button button_ai_move_cd_pre_1cs;
	Gtk.Button button_ai_move_cd_post_1cs;
	Gtk.Button button_ai_move_cd_post_1ds;
	Gtk.Button button_ai_move_cd_post_1s;
	Gtk.Button button_ai_move_cd_align_left;
	Gtk.Button button_ai_move_cd_align_center;
	Gtk.Button button_ai_move_cd_align_right;

	Gtk.Box box_ai_ac;
	Gtk.Box box_ai_bd;
	Gtk.Box box_ai_a_buttons;
	Gtk.Box box_ai_b_buttons;

	Gtk.Label label_hscale_ai_a_pre_1s;
	Gtk.Label label_hscale_ai_a_post_1s;
	Gtk.Label label_hscale_ai_b_pre_1s;
	Gtk.Label label_hscale_ai_b_post_1s;

	Gtk.Button button_hscale_ai_a_first;
	Gtk.Button button_hscale_ai_a_pre;
	Gtk.Button button_hscale_ai_a_pre_1s;
	Gtk.Button button_hscale_ai_a_post;
	Gtk.Button button_hscale_ai_a_post_1s;
	Gtk.Button button_hscale_ai_a_last;

	Gtk.Button button_hscale_ai_b_first;
	Gtk.Button button_hscale_ai_b_pre;
	Gtk.Button button_hscale_ai_b_pre_1s;
	Gtk.Button button_hscale_ai_b_post;
	Gtk.Button button_hscale_ai_b_post_1s;
	Gtk.Button button_hscale_ai_b_last;

	Gtk.HScale hscale_ai_a;
	Gtk.HScale hscale_ai_b;
	Gtk.HScale hscale_ai_c;
	Gtk.HScale hscale_ai_d;

	//Gtk.Grid grid_radios_force_sensor_ai;
	Gtk.RadioButton radio_ai_1set;
	Gtk.RadioButton radio_ai_2sets;
	Gtk.Notebook notebook_ai_load;
	Gtk.Viewport viewport_radio_ai_ab;
	Gtk.Viewport viewport_radio_ai_cd;
	Gtk.RadioButton radio_ai_ab;
	Gtk.RadioButton radio_ai_cd;
	Gtk.Box box_ai_cd_buttons;

	Gtk.Viewport viewport_ai_hscales;
	Gtk.CheckButton check_ai_chained_hscales;
	Gtk.CheckButton check_ai_zoom;

	Gtk.Button button_ai_model;
	Gtk.Notebook notebook_ai_model_options;
	Gtk.Button button_ai_model_options_close_and_analyze;
	Gtk.Button button_ai_model_options;
	Gtk.Label label_model_analyze;
	Gtk.Viewport viewport_ai_model_graph;
	Gtk.Image image_ai_model_graph;
	Gtk.Button button_ai_model_save_image;
	Gtk.Notebook notebook_ai_model_graph_table_triggers;
	Gtk.Label label_model_triggers_found;

	Gtk.RadioButton radio_ai_export_individual_current_session;
	Gtk.RadioButton radio_ai_export_individual_all_sessions;
	Gtk.RadioButton radio_ai_export_groupal_current_session;
	Gtk.Label label_ai_export_person;
	Gtk.Label label_ai_export_session;
	Gtk.HBox hbox_ai_export_images;
	Gtk.CheckButton check_ai_export_images;
	Gtk.HBox hbox_ai_export_width_height;
	Gtk.SpinButton spinbutton_ai_export_image_width;
	Gtk.SpinButton spinbutton_ai_export_image_height;
	Gtk.Notebook notebook_ai_export;
	Gtk.Label label_ai_export;
	Gtk.ProgressBar progressbar_ai_export;
	Gtk.Label label_ai_export_result;
	Gtk.Button button_ai_export_result_open;
	// <---- at glade

	public enum AlignTypes { LEFT, CENTER, RIGHT };
	private bool button_ai_model_was_sensitive; //needed this temp variable

	private void blankAIInterface ()
	{
		//cd cannot be selected until currentForceSensor.UniqueID >= 0
		radio_ai_ab.Active = true;
		if (radio_ai_2sets.Active)
			radio_ai_cd.Sensitive = false;

		//put scales to 0,0
		hscale_ai_a.SetRange(0, 0);
		hscale_ai_b.SetRange(0, 0);
		hscale_ai_c.SetRange(0, 0);
		hscale_ai_d.SetRange(0, 0);
		//set them to 0, because if not is set to 1 by a GTK error
		hscale_ai_a.Value = 0;
		hscale_ai_b.Value = 0;
		hscale_ai_c.Value = 0;
		hscale_ai_d.Value = 0;

		AiVars.chainedDiffAtStartAB = 0;
		AiVars.chainedDiffAtStartCD = 0;

		button_ai_model_options_close_and_analyze.Sensitive = false;
	}

	private void signalAnalyzeButtonsVisibility ()
	{
		if (Constants.ModeIsFORCESENSOR (current_mode))
		{
			// depend if set is elastic: box_force_sensor_analyze_magnitudes.Visible = true;
		} else //if (current_mode == Constants.Modes.RUNSENCODER)
		{
			box_force_sensor_analyze_magnitudes.Visible = false;
		}
	}

	private void forceSensorZoomDefaultValues()
	{
		AiVars.zoomApplied = false;
		check_ai_zoom.Active = false;
	}

	//private double hscale_fs_ai_a_BeforeZoomTimeMS = 0; //to calculate triggers

	private void signalPrepareGraphAI ()
	{
		if (Constants.ModeIsFORCESENSOR (current_mode))
			forceSensorPrepareGraphAI ();
		else //if (current_mode == Constants.Modes.RUNSENCODER)
			runEncoderPrepareGraphAI ();
	}

	private void signalPrepareGraphAICont (int lengthAB, int lengthCD, int zoomFrameB, Gtk.HScale hsRight)
	{
		/*
		 * position the hscales on the left to avoid loading a csv
		 * with less data rows than current csv and having scales out of the graph
		//hscale_ai_a.ValuePos = Gtk.PositionType.Left; //doesn't work
		//hscale_ai_b.ValuePos = Gtk.PositionType.Left; //doesn't work
		*/
		hscale_ai_a.Value = 0;
		hscale_ai_b.Value = 0;
		hscale_ai_c.Value = 0;
		hscale_ai_d.Value = 0;

//TODO: això haurà de ser també per raceAnalyzer, per tant tota la funció, creant els raAI_AB i raAI_CD

		//ranges should have max value the number of the lines of csv file minus the header
		//this applies to the four hscales
		hscale_ai_a.SetRange (0, lengthAB -1);
		hscale_ai_b.SetRange (0, lengthAB -1);
		hscale_ai_c.SetRange (0, lengthCD -1);
		hscale_ai_d.SetRange (0, lengthCD -1);
		//set them to 0, because if not is set to 1 by a GTK error
		hscale_ai_a.Value = 0;
		hscale_ai_b.Value = 0;
		hscale_ai_c.Value = 0;
		hscale_ai_d.Value = 0;

		//LogB.Information(string.Format("hscale_ai_time_a,b,ab ranges: 0, {0}", fsAI.GetLength() -1));

		//on zoom put hscale B at the right
		if(zoomFrameB >= 0)
		{
			//if (radio_ai_2sets.Active && radio_ai_cd.Active)
			if (radio_ai_cd.Active)
				hsRight.Value = lengthCD -1;
			else
				hsRight.Value = lengthAB -1;
		}

		//to update values
		LogB.Information ("calling to move hscale from forceSensorPrepareGraphAI ()");
		if (radio_ai_ab.Active)
			on_hscale_ai_value_changed (hscale_ai_a, new EventArgs ());
		else
			on_hscale_ai_value_changed (hscale_ai_c, new EventArgs ());
	}

	private void button_ai_move_cd_pre_set_sensitivity ()
	{
		if (! radio_ai_2sets.Active)
		{
			button_ai_move_cd_pre.Sensitive = false;
			return;
		}
		if (! radio_ai_cd.Active)
		{
			button_ai_move_cd_pre.Sensitive = false;
			return;
		}
		if (Constants.ModeIsFORCESENSOR (current_mode))
			button_ai_move_cd_pre.Sensitive = lastForceSensorFullPath_CD != null && lastForceSensorFullPath_CD != "";
		else //if (current_mode == Constants.Modes.RUNSENCODER)
			button_ai_move_cd_pre.Sensitive = currentRunEncoder_CD != null;
	}

	private void on_button_ai_move_cd_pre_clicked (object o, EventArgs args)
	{
		button_ai_move_cd_pre.Sensitive = false;

		box_ai_ac.Visible = false;
		box_ai_bd.Visible = false;
		box_ai_a_buttons.Visible = false;
		box_ai_b_buttons.Visible = false;
		check_ai_chained_hscales.Visible = false;
		check_ai_zoom.Visible = false;
		button_ai_model.Visible = false;

		box_ai_move_cd_accept.Visible = true;
		box_ai_move_cd_buttons.Visible = true;

		radio_ai_1set.Sensitive = false;
		radio_ai_ab.Sensitive = false;
		box_ai_cd_buttons.Sensitive = false;
	}
	private void on_button_signal_analyze_move_cd_close_clicked (object o, EventArgs args)
	{
		button_ai_move_cd_pre.Sensitive = true;

		box_ai_ac.Visible = true;
		box_ai_bd.Visible = true;
		box_ai_a_buttons.Visible = true;
		box_ai_b_buttons.Visible = true;
		check_ai_chained_hscales.Visible = true;
		check_ai_zoom.Visible = true;
		button_ai_model.Visible = true;

		box_ai_move_cd_accept.Visible = false;
		box_ai_move_cd_buttons.Visible = false;

		radio_ai_1set.Sensitive = true;
		radio_ai_ab.Sensitive = true;
		box_ai_cd_buttons.Sensitive = true;
	}

	private void on_button_ai_move_cd_clicked (object o, EventArgs args)
	{
		if (o == null)
			return;

		if ((Gtk.Button) o == button_ai_move_cd_pre_1s)
			on_button_signal_analyze_move_cd_do (-1);
		else if ((Gtk.Button) o == button_ai_move_cd_pre_1ds)
			on_button_signal_analyze_move_cd_do (-.1);
		else if ((Gtk.Button) o == button_ai_move_cd_pre_1cs)
			on_button_signal_analyze_move_cd_do (-.01);
		else if ((Gtk.Button) o == button_ai_move_cd_post_1cs)
			on_button_signal_analyze_move_cd_do (+.01);
		else if ((Gtk.Button) o == button_ai_move_cd_post_1ds)
			on_button_signal_analyze_move_cd_do (+.1);
		else if ((Gtk.Button) o == button_ai_move_cd_post_1s)
			on_button_signal_analyze_move_cd_do (+1);
		else if ((Gtk.Button) o == button_ai_move_cd_align_left)
			on_button_signal_analyze_move_cd_do_align (AlignTypes.LEFT);
		else if ((Gtk.Button) o == button_ai_move_cd_align_center)
			on_button_signal_analyze_move_cd_do_align (AlignTypes.CENTER);
		else if ((Gtk.Button) o == button_ai_move_cd_align_right)
			on_button_signal_analyze_move_cd_do_align (AlignTypes.RIGHT);
	}

	private void on_button_signal_analyze_move_cd_do (double time)
	{
		if (Constants.ModeIsFORCESENSOR (current_mode))
		{
			if (spCairoFE_CD != null)
			{
				spCairoFE_CD.ShiftMicros (Convert.ToInt32 (time * 1000000)); // s to micros
				ai_drawingarea_cairo.QueueDraw(); //will fire ExposeEvent
			}
		} else { //if (current_mode == Constants.Modes.RUNSENCODER)
			if (cairoGraphRaceAnalyzerPoints_st_CD_l != null)
			{
				cairoGraphRaceAnalyzerPoints_st_CD_l =
					PointF.ShiftX (cairoGraphRaceAnalyzerPoints_st_CD_l, time);
				//cairoGraphRaceAnalyzerPoints_st_CD_l_timeShifted = true; //unused

				ai_drawingarea_cairo.QueueDraw(); //will fire ExposeEvent
			}
		}
	}

	private void on_button_signal_analyze_move_cd_do_align (AlignTypes align)
	{
		if (Constants.ModeIsFORCESENSOR (current_mode) &&
				spCairoFE_CD != null &&
				spCairoFE != null &&
				spCairoFE_CD.Force_l.Count > 0 &&
				spCairoFE.Force_l.Count > 0)
			on_button_signal_analyze_move_cd_do_align_forceSensor (align);
		else if (current_mode == Constants.Modes.RUNSENCODER &&
				cairoGraphRaceAnalyzerPoints_st_CD_l != null &&
				cairoGraphRaceAnalyzerPoints_st_l != null &&
				cairoGraphRaceAnalyzerPoints_st_CD_l.Count > 0 &&
				cairoGraphRaceAnalyzerPoints_st_l.Count > 0)
			on_button_signal_analyze_move_cd_do_align_raceAnalyzer (align);
	}

	private void on_button_signal_analyze_move_cd_do_align_forceSensor (AlignTypes align)
	{
		int xAB = 0;
		int xCD = 0;
		if (align == AlignTypes.LEFT)
		{
			xAB = Convert.ToInt32 (spCairoFE.Force_l[0].X);
			xCD = Convert.ToInt32 (spCairoFE_CD.Force_l[0].X);
		}
		else if (align == AlignTypes.CENTER)
		{
			xAB = Convert.ToInt32 (UtilAll.DivideSafe (spCairoFE.Force_l[0].X + PointF.Last (spCairoFE.Force_l).X, 2));
			xCD = Convert.ToInt32 (UtilAll.DivideSafe (spCairoFE_CD.Force_l[0].X + PointF.Last (spCairoFE_CD.Force_l).X, 2));
		}
		else if (align == AlignTypes.RIGHT)
		{
			xAB = Convert.ToInt32 (PointF.Last (spCairoFE.Force_l).X);
			xCD = Convert.ToInt32 (PointF.Last (spCairoFE_CD.Force_l).X);
		}

		spCairoFE_CD.ShiftMicros (xAB - xCD);
		ai_drawingarea_cairo.QueueDraw(); //will fire ExposeEvent
	}

	private void on_button_signal_analyze_move_cd_do_align_raceAnalyzer (AlignTypes align)
	{
		double xAB = 0;
		double xCD = 0;
		if (align == AlignTypes.LEFT)
		{
			xAB = cairoGraphRaceAnalyzerPoints_st_l[0].X;
			xCD = cairoGraphRaceAnalyzerPoints_st_CD_l[0].X;
		}
		else if (align == AlignTypes.CENTER)
		{
			xAB = UtilAll.DivideSafe (cairoGraphRaceAnalyzerPoints_st_l[0].X +
					PointF.Last (cairoGraphRaceAnalyzerPoints_st_l).X, 2);
			xCD = UtilAll.DivideSafe (cairoGraphRaceAnalyzerPoints_st_CD_l[0].X +
					PointF.Last (cairoGraphRaceAnalyzerPoints_st_CD_l).X, 2);
		}
		else if (align == AlignTypes.RIGHT)
		{
			xAB = PointF.Last (cairoGraphRaceAnalyzerPoints_st_l).X;
			xCD = PointF.Last (cairoGraphRaceAnalyzerPoints_st_CD_l).X;
		}

		cairoGraphRaceAnalyzerPoints_st_CD_l = PointF.ShiftX (cairoGraphRaceAnalyzerPoints_st_CD_l, xAB - xCD);
		//cairoGraphRaceAnalyzerPoints_st_CD_l_timeShifted = true; //unused

		ai_drawingarea_cairo.QueueDraw(); //will fire ExposeEvent
	}

	private void on_check_ai_zoom_clicked (object o, EventArgs args)
	{
		LogB.Information ("on_check_ai_zoom_clicked");
		if (check_ai_zoom.Active)
			check_ai_zoom_zoom ();
		else
			check_ai_zoom_unzoom ();
	}

	private void check_ai_zoom_zoom ()
	{
		AnalyzeInstant sAI = getCorrectAI ();

		if(sAI == null || sAI.GetLength() == 0)
			return;

		AiVars.zoomApplied = true;

		//store hscale a to help return to position on unzoom
		AiVars.a_beforeZoom = Convert.ToInt32 (hscale_ai_a.Value);
		AiVars.b_beforeZoom = Convert.ToInt32 (hscale_ai_b.Value);
		AiVars.c_beforeZoom = Convert.ToInt32 (hscale_ai_c.Value);
		AiVars.d_beforeZoom = Convert.ToInt32 (hscale_ai_d.Value);

		int sampleL;
		int sampleR;

		if (radio_ai_2sets.Active &&
				(Constants.ModeIsFORCESENSOR (current_mode) && spCairoFE_CD != null && spCairoFE_CD.Force_l.Count > 0) ||
				(current_mode == Constants.Modes.RUNSENCODER && cairoGraphRaceAnalyzerPoints_st_CD_l != null && cairoGraphRaceAnalyzerPoints_st_CD_l.Count > 0))

		{
			// zoomed has to be the same range for ab than cd, to show all data in graph. range is related to what is selected in the ratio

			//time has shifted (not as samples, is directly time, so need to find sample that matches that time)
			if (Constants.ModeIsFORCESENSOR (current_mode))// && spCairoFE_CD.TimeShifted)
			{
				if (radio_ai_ab.Active)
				{
					// 1) ab data is the hscales data
					spCairoFEZoom = new SignalPointsCairoForceElastic (spCairoFE, AiVars.a_beforeZoom, AiVars.b_beforeZoom, true);

					// 2) cd data are samples close in time to ab data
					// 1st check if it overlaps, if it does not overlap and we include it, it would show a bigger graph with empty data
					if (! PointF.ListsTimeOverlap (spCairoFE_CD.Force_l, spCairoFE.Force_l, AiVars.a_beforeZoom, AiVars.b_beforeZoom))
						spCairoFEZoom_CD = new SignalPointsCairoForceElastic ();
					else {
						sampleL = PointF.FindSampleCloseToTime (
								spCairoFE_CD.Force_l, spCairoFE.Force_l[AiVars.a_beforeZoom].X);
						sampleR = PointF.FindSampleCloseToTime (
								spCairoFE_CD.Force_l, spCairoFE.Force_l[AiVars.b_beforeZoom].X);
						spCairoFEZoom_CD = new SignalPointsCairoForceElastic (spCairoFE_CD,
								sampleL, sampleR, true);
					}
				} else {
					// 1) cd data is the hscales data
					spCairoFEZoom_CD = new SignalPointsCairoForceElastic (spCairoFE_CD, AiVars.c_beforeZoom, AiVars.d_beforeZoom, true);

					// 2) ab data are samples close in time to cd data
					// 1st check if it overlaps, if it does not overlap and we include it, it would show a bigger graph with empty data
					if (! PointF.ListsTimeOverlap (spCairoFE.Force_l, spCairoFE_CD.Force_l, AiVars.c_beforeZoom, AiVars.d_beforeZoom))
						spCairoFEZoom = new SignalPointsCairoForceElastic ();
					else {
						sampleL = PointF.FindSampleCloseToTime (
								spCairoFE.Force_l, spCairoFE_CD.Force_l[AiVars.c_beforeZoom].X);
						sampleR = PointF.FindSampleCloseToTime (
								spCairoFE.Force_l, spCairoFE_CD.Force_l[AiVars.d_beforeZoom].X);
						spCairoFEZoom = new SignalPointsCairoForceElastic (spCairoFE,
								sampleL, sampleR, true);
					}
				}
			}
			else if (current_mode == Constants.Modes.RUNSENCODER)// && cairoGraphRaceAnalyzerPoints_st_CD_l_timeShifted) //similar to above code, read comments there
			{
				if (radio_ai_ab.Active)
				{
					cairoGraphRaceAnalyzerPoints_st_Zoom_l = PointF.GetSubList (cairoGraphRaceAnalyzerPoints_st_l, AiVars.a_beforeZoom, AiVars.b_beforeZoom);

					if (! PointF.ListsTimeOverlap (cairoGraphRaceAnalyzerPoints_st_CD_l, cairoGraphRaceAnalyzerPoints_st_l, AiVars.a_beforeZoom, AiVars.b_beforeZoom))
						cairoGraphRaceAnalyzerPoints_st_Zoom_CD_l = new List<PointF> ();
					else {
						sampleL = PointF.FindSampleCloseToTime (
								cairoGraphRaceAnalyzerPoints_st_CD_l, cairoGraphRaceAnalyzerPoints_st_l[AiVars.a_beforeZoom].X);
						sampleR = PointF.FindSampleCloseToTime (
								cairoGraphRaceAnalyzerPoints_st_CD_l, cairoGraphRaceAnalyzerPoints_st_l[AiVars.b_beforeZoom].X);
						cairoGraphRaceAnalyzerPoints_st_Zoom_CD_l = PointF.GetSubList (cairoGraphRaceAnalyzerPoints_st_CD_l, sampleL, sampleR);
					}
				} else {
					cairoGraphRaceAnalyzerPoints_st_Zoom_CD_l = PointF.GetSubList (cairoGraphRaceAnalyzerPoints_st_CD_l, AiVars.c_beforeZoom, AiVars.d_beforeZoom);

					if (! PointF.ListsTimeOverlap (cairoGraphRaceAnalyzerPoints_st_l, cairoGraphRaceAnalyzerPoints_st_CD_l, AiVars.c_beforeZoom, AiVars.d_beforeZoom))
						cairoGraphRaceAnalyzerPoints_st_Zoom_l = new List<PointF> ();
					else {
						sampleL = PointF.FindSampleCloseToTime (
								cairoGraphRaceAnalyzerPoints_st_l, cairoGraphRaceAnalyzerPoints_st_CD_l[AiVars.c_beforeZoom].X);
						sampleR = PointF.FindSampleCloseToTime (
								cairoGraphRaceAnalyzerPoints_st_l, cairoGraphRaceAnalyzerPoints_st_CD_l[AiVars.d_beforeZoom].X);
						cairoGraphRaceAnalyzerPoints_st_Zoom_l = PointF.GetSubList (cairoGraphRaceAnalyzerPoints_st_l, sampleL, sampleR);
					}
				}
			} /*else { //this code (! timeShifted) is not used because samples are not at same time (even on forceSensor)
			    if (radio_ai_ab.Active)
			    {
			    sampleL = AiVars.a_beforeZoom;
			    sampleR = AiVars.b_beforeZoom;
			    } else {
			    sampleL = AiVars.c_beforeZoom;
			    sampleR = AiVars.d_beforeZoom;
			    }

			    if (Constants.ModeIsFORCESENSOR (current_mode))
			    {
			    spCairoFEZoom = new SignalPointsCairoForceElastic (spCairoFE,
			    sampleL, sampleR, true);
			    spCairoFEZoom_CD = new SignalPointsCairoForceElastic (spCairoFE_CD,
			    sampleL, sampleR, true);
			    }
			    else //if (current_mode == Constants.Modes.RUNSENCODER)
			    {
			    cairoGraphRaceAnalyzerPoints_st_Zoom_l = PointF.GetSubList (
			    cairoGraphRaceAnalyzerPoints_st_l, sampleL, sampleR);
			    cairoGraphRaceAnalyzerPoints_st_Zoom_CD_l = PointF.GetSubList (
			    cairoGraphRaceAnalyzerPoints_st_CD_l, sampleL, sampleR);
			    }
			    }
			    */
		} else {
			sampleL = AiVars.a_beforeZoom;
			sampleR = AiVars.b_beforeZoom;
			if (! radio_ai_ab.Active)
			{
				sampleL = AiVars.c_beforeZoom;
				sampleR = AiVars.d_beforeZoom;
			}

			if (Constants.ModeIsFORCESENSOR (current_mode))
				spCairoFEZoom = new SignalPointsCairoForceElastic (spCairoFE, sampleL, sampleR, true);
			else //if (current_mode == Constants.Modes.RUNSENCODER)
				cairoGraphRaceAnalyzerPoints_st_Zoom_l = PointF.GetSubList (
						cairoGraphRaceAnalyzerPoints_st_l, sampleL, sampleR);
		}

		//cairo, repetitions
		if (Constants.ModeIsFORCESENSOR (current_mode))
		{
			rep_lZoomAppliedCairo = new List<ForceSensorRepetition> ();
			for (int r = 0; r < sAI.ForceSensorRepetition_l.Count; r ++)
			{
				// don't do like this until delete non-cairo because this changes the sAI.ForceSensorRepetition_l values and non-cairo is not displayed correctly
				//ForceSensorRepetition fsr = sAI.ForceSensorRepetition_l[r];
				// do this:
				ForceSensorRepetition fsr = sAI.ForceSensorRepetition_l[r].Clone();

				fsr.sampleStart -= AiVars.a_beforeZoom;
				fsr.sampleEnd -= AiVars.a_beforeZoom;

				rep_lZoomAppliedCairo.Add (fsr);
			}
		}

		signalPrepareGraphAI ();

		image_force_sensor_ai_zoom.Visible = false;
		image_force_sensor_ai_zoom_out.Visible = true;
		radiosAiSensitivity (false);
	}

	private void check_ai_zoom_unzoom ()
	{
		AiVars.zoomApplied = false;

		if (radio_ai_ab.Active)
		{
			AiVars.a_atZoom = Convert.ToInt32 (hscale_ai_a.Value);
			AiVars.b_atZoom = Convert.ToInt32 (hscale_ai_b.Value);
		} else {
			AiVars.c_atZoom = Convert.ToInt32 (hscale_ai_c.Value);
			AiVars.d_atZoom = Convert.ToInt32 (hscale_ai_d.Value);
		}

		signalPrepareGraphAI ();

		if (radio_ai_ab.Active)
		{
			// set hscales a,b to value before + value at zoom (because user maybe changed it on zoom)
			hscale_ai_a.Value = AiVars.a_beforeZoom +
				(AiVars.a_atZoom);
			hscale_ai_b.Value = AiVars.a_beforeZoom +
				(AiVars.b_atZoom);

			// set hscales c,d at same value before zoom
			hscale_ai_c.Value = AiVars.c_beforeZoom;
			hscale_ai_d.Value = AiVars.d_beforeZoom;
		} else {
			hscale_ai_a.Value = AiVars.a_beforeZoom;
			hscale_ai_b.Value = AiVars.b_beforeZoom;

			hscale_ai_c.Value = AiVars.c_beforeZoom +
				(AiVars.c_atZoom);
			hscale_ai_d.Value = AiVars.c_beforeZoom +
				(AiVars.d_atZoom);
		}

		image_force_sensor_ai_zoom.Visible = true;
		image_force_sensor_ai_zoom_out.Visible = false;
		radiosAiSensitivity (true);
	}

	private void getAiZoomStartEnd (string timeStart, string timeEnd, Gtk.HScale hsLeft, Gtk.HScale hsRight,
			ref int zoomFrameA, ref int zoomFrameB)
	{
		LogB.Information ("getAiZoomStartEnd");
		LogB.Information ("timeStart", timeStart);
		LogB.Information ("timeEnd", timeEnd);
		if (timeStart == null || timeStart == "" || timeEnd == null || timeEnd == "")
			return;

		if (! Util.IsNumber (timeStart, true) || ! Util.IsNumber (timeEnd, true))
			return;

		//invert hscales if needed
		int firstValue = Convert.ToInt32 (hsLeft.Value);
		int secondValue = Convert.ToInt32 (hsRight.Value);
		//LogB.Information(string.Format("firstValue: {0}, secondValue: {1}", firstValue, secondValue));

		//note that is almost impossible in the ui, but just in case...
		if(firstValue > secondValue) {
			int temp = firstValue;
			firstValue = secondValue;
			secondValue = temp;
		}

		//-1 and +1 to have the points at the edges to calcule the RFDs
		//like this works but cannot calculate the RFD of A,B
		zoomFrameA = firstValue;
		zoomFrameB = secondValue;;
		//zoomFrameA = firstValue -1;
		//zoomFrameB = secondValue +1;

		//do not zoom if both are the same, or the diff is just on pixel
		if(Math.Abs(zoomFrameA - zoomFrameB) <= 1)
		{
			zoomFrameA = -1;
			zoomFrameB = -1;
		}
	}

	private Gtk.HScale getHScaleABCD (bool left)
	{
		if (radio_ai_ab.Active)
		{
			if (left)
				return hscale_ai_a;
			else
				return hscale_ai_b;
		} else {
			if (left)
				return hscale_ai_c;
			else
				return hscale_ai_d;
		}
	}

	private AnalyzeInstant getCorrectAI () // TODO: rename getCorrectAI
	{
		if (Constants.ModeIsFORCESENSOR (current_mode))
		{
			if (radio_ai_ab.Active)
				return fsAI_AB;
			else
				return fsAI_CD;
		} else { //if (current_mode == Constants.Modes.RUNSENCODER)
			if (radio_ai_ab.Active)
				return raAI_AB;
			else
				return raAI_CD;
		}
	}

	private void on_hscale_ai_value_changed (object o, EventArgs args)
	{
		LogB.Information ("on_hscale_ai_value_changed");
		AnalyzeInstant sAI = getCorrectAI ();

		if(sAI == null || sAI.GetLength() == 0)
			return;

		Gtk.HScale hs = (Gtk.HScale) o;

		if (check_ai_chained_hscales.Active)
			hscale_ai_value_changed_chained (sAI, hs);

		hscale_ai_value_changed_do (sAI, hs);
	}

	private void hscale_ai_value_changed_chained (AnalyzeInstant sAI, Gtk.HScale hs)
	{
		if (AiVars.hscalesDoNotFollow)
			return;

		LogB.Information ("hscale_ai_value_changed_chained 0");
		bool isLeft; //A or C
		Gtk.HScale hsRelated ; //if A then B, if D then C
		int previousDiffWithRelated;
		double previousDiffTimeWithRelated; //used on race analyzer because the samples are not uniform
		if (hs == hscale_ai_a)
		{
			isLeft = true; //A or C
			hsRelated = hscale_ai_b; //if A then B, if D then C
			previousDiffWithRelated = Convert.ToInt32 (AiVars.chainedDiffAtStartAB);
			previousDiffTimeWithRelated = AiVars.chainedDiffAtStartAB;
		}
		else if (hs == hscale_ai_b)
		{
			isLeft = false;
			hsRelated = hscale_ai_a;
			previousDiffWithRelated = -Convert.ToInt32 (AiVars.chainedDiffAtStartAB);
			previousDiffTimeWithRelated = -AiVars.chainedDiffAtStartAB;
		}
		else if (hs == hscale_ai_c)
		{
			isLeft = true;
			hsRelated = hscale_ai_d;
			previousDiffWithRelated = Convert.ToInt32 (AiVars.chainedDiffAtStartCD);
			previousDiffTimeWithRelated = AiVars.chainedDiffAtStartCD;
		}
		else //if (hs == hscale_ai_d)
		{
			isLeft = false;
			hsRelated = hscale_ai_c;
			previousDiffWithRelated = -Convert.ToInt32 (AiVars.chainedDiffAtStartCD);
			previousDiffTimeWithRelated = -AiVars.chainedDiffAtStartCD;
		}

		AiVars.hscalesDoNotFollow = true;

		if (Constants.ModeIsFORCESENSOR (current_mode))
		{
			if (isLeft && Convert.ToInt32 (hs.Value) + previousDiffWithRelated >= sAI.GetLength() -1)
			{
				hsRelated.Value = sAI.GetLength () -1;
				hs.Value = hsRelated.Value - Math.Abs (previousDiffWithRelated);
			}
			else if (! isLeft && Convert.ToInt32 (hs.Value) + previousDiffWithRelated <= 0)
			{
				hsRelated.Value = 0;
				hs.Value = hsRelated.Value + Math.Abs (previousDiffWithRelated);
			}
			else
				hsRelated.Value = hs.Value + previousDiffWithRelated;
		}
		else { //if (current_mode == Constants.Modes.RUNSENCODER)
			if (isLeft && UtilAll.DivideSafe (sAI.GetTimeMS (Convert.ToInt32(hs.Value)) + previousDiffTimeWithRelated, 1000) > PointF.Last (sAI.P_l).X)
			{
				hsRelated.Value = sAI.GetLength () -1;
				hs.Value = PointF.FindSampleCloseToTime (sAI.P_l,
						UtilAll.DivideSafe (sAI.GetTimeMS (Convert.ToInt32 (hsRelated.Value)) - Math.Abs (previousDiffTimeWithRelated), 1000)); //ms to s
			} else if (! isLeft && UtilAll.DivideSafe (sAI.GetTimeMS (Convert.ToInt32(hs.Value)) + previousDiffTimeWithRelated, 1000) <= 0)
			{
				hsRelated.Value = 0;
				hs.Value = PointF.FindSampleCloseToTime (sAI.P_l,
						UtilAll.DivideSafe (sAI.GetTimeMS (Convert.ToInt32 (hsRelated.Value)) + Math.Abs (previousDiffTimeWithRelated), 1000)); //ms to s
			} else {
				int relatedSampleWillBe = PointF.FindSampleCloseToTime (sAI.P_l,
						UtilAll.DivideSafe (sAI.GetTimeMS (Convert.ToInt32(hs.Value)) + previousDiffTimeWithRelated, 1000)); //ms to s
				hsRelated.Value = relatedSampleWillBe;
			}
		}

		AiVars.hscalesDoNotFollow = false;

		LogB.Information ("hscale_ai_value_changed_chained end");
	}

	//can be convinient to call it directly
	private void hscale_ai_value_changed_do (AnalyzeInstant sAI, HScale hs)
	{
		// 1. set some variables to make this function work for the four hscales
		bool isLeft; //A or C
		Gtk.HScale hsRelated ; //if A then B, if D then C
		string hscaleToDebug;
		TreeviewS2Abstract tvS;

		if (hs == hscale_ai_a)
		{
			isLeft = true; //A or C
			hsRelated = hscale_ai_b; //if A then B, if D then C
			hscaleToDebug = "--- hscale_a ---";
		}
		else if (hs == hscale_ai_b)
		{
			isLeft = false;
			hsRelated = hscale_ai_a;
			hscaleToDebug = "--- hscale_b ---";
		}
		else if (hs == hscale_ai_c)
		{
			isLeft = true;
			hsRelated = hscale_ai_d;
			hscaleToDebug = "--- hscale_c ---";
		}
		else //if (hs == hscale_ai_d)
		{
			isLeft = false;
			hsRelated = hscale_ai_c;
			hscaleToDebug = "--- hscale_d ---";
		}

		if (Constants.ModeIsFORCESENSOR (current_mode))
		{
			if (hs == hscale_ai_a || hs == hscale_ai_b)
				tvS = tvFS_AB;
			else
				tvS = tvFS_CD;
		} else { //if (current_mode == Constants.Modes.RUNSENCODER)
			if (hs == hscale_ai_a || hs == hscale_ai_b)
				tvS = tvRA_AB;
			else
				tvS = tvRA_CD;
		}

		/*
		LogB.Information (string.Format ("on_hscale_ai_value_changed {0} 0", hscaleToDebug));
		LogB.Information ("hscales at start: ");
		LogB.Information ("hscales a: " + hscale_ai_a.Value.ToString ());
		LogB.Information ("hscales b: " + hscale_ai_b.Value.ToString ());
		LogB.Information ("hscales c: " + hscale_ai_c.Value.ToString ());
		LogB.Information ("hscales d: " + hscale_ai_d.Value.ToString ());
		*/

		//LogB.Information (string.Format ("on_hscale_ai_value_changed {0} 1", hscaleToDebug));

		//do not allow A or C to be higher than B or D (fix multiple possible problems)
		if (isLeft && hs.Value > hsRelated.Value)
			hsRelated.Value = hs.Value;
		else if (! isLeft && hs.Value < hsRelated.Value)
			hs.Value = hsRelated.Value;

		// 2. fix possible boundaries problem that could happen when there is really few data
		int count = Convert.ToInt32 (hs.Value);
		int countRelated = Convert.ToInt32 (hsRelated.Value);
		if (
				(count > 0 && count > sAI.GetLength() -1) ||
				(countRelated > 0 && countRelated > sAI.GetLength() -1) )
		{
			LogB.Information (string.Format ("hscale_force_sensor outside of boundaries (isLeft: {0}, count: {1}, countRelated: {2}, sAI.GetLength (): {3})",
						isLeft, count, countRelated, sAI.GetLength ()));
			return;
		}
		LogB.Information (string.Format ("on_hscale_ai_value_changed {0} 2", hscaleToDebug));

		// 3. treeviews prepare
		tvS.ResetTreeview ();
		tvS.PassTime1or2 (isLeft, Math.Round(sAI.GetTimeMS(count), 1).ToString());

		// 4. fill treeviews
		if (Constants.ModeIsFORCESENSOR (current_mode))
		{
			// 4.1. calculate RFD
			string rfd = "";
			if (Constants.ModeIsFORCESENSOR (current_mode))
				if(count > 0 && count < sAI.GetLength() -1)
					rfd = Math.Round(sAI.CalculateRFD(count -1, count +1), 1).ToString();

			// 4.2. calculate elastic variables
			string position = "";
			string speed = "";
			string accel = "";
			string power = "";
			LogB.Information ("at hscales sAI.CalculedElasticPSAP: " + sAI.CalculedElasticPSAP.ToString ());
			if(sAI.CalculedElasticPSAP)
			{
				position = Math.Round (sAI.Position_l[count], 3).ToString();
				speed = Math.Round (sAI.Speed_l[count], 3).ToString();
				accel = Math.Round (sAI.Accel_l[count], 3).ToString();
				power = Math.Round (sAI.Power_l[count], 3).ToString();
			}

			// 4.3 fill treeviews
			tvS.PassForceAndRFD1or2 (isLeft, sAI.GetForceAtCount (count), rfd);
			//fix a bug where B is moved and not A (so A is empty)
			if (! isLeft && (tvS.TimeStart == null || tvS.TimeStart == ""))
			{
				tvS.PassTime1or2 (true, Math.Round(sAI.GetTimeMS(0), 1).ToString());
				tvS.PassForceAndRFD1or2 (true, sAI.GetForceAtCount (0), rfd);
			}

			if (current_mode == Constants.Modes.FORCESENSORELASTIC)
				tvS.PassRow1or2Elastic (isLeft, position, speed, accel, power);
		} else { //if (current_mode == Constants.Modes.RUNSENCODER)
			tvS.PassSpeed1or2 (isLeft, sAI.GetSpeedAtCount (count));

			//GetRaceAnalyzer* functions will know which is minor (count or countRelated)
			tvS.PassSpeedAvg (sAI.GetRaceAnalyzerAvg (count, countRelated));
			tvS.PassSpeedMax (sAI.GetRaceAnalyzerMax (count, countRelated));
		}

		//LogB.Information (string.Format ("on_hscale_ai_value_changed {0} 3", hscaleToDebug));

		if (AiVars.hscalesDoNotFollow)
		{
			AiVars.hscalesDoNotFollow = false;

			if (Constants.ModeIsFORCESENSOR (current_mode))
			{
				tvS.ResetTreeview (); //To avoid duplicated rows on chained A,B
				tvS.FillTreeview ();
			}

			return;
		}

		if (Constants.ModeIsFORCESENSOR (current_mode))
		{
			//need to do both to ensure at unzoom params are calculated for AB and CD
			force_sensor_analyze_instant_calculate_params_for_treeview (fsAI_AB, tvFS_AB, true,
					Convert.ToInt32 (hscale_ai_a.Value),
					Convert.ToInt32 (hscale_ai_b.Value));
			force_sensor_analyze_instant_calculate_params_for_treeview (fsAI_CD, tvFS_CD, false,
					Convert.ToInt32 (hscale_ai_c.Value),
					Convert.ToInt32 (hscale_ai_d.Value));
		} else { //if (current_mode == Constants.Modes.RUNSENCODER)
			race_analyzer_analyze_instant_calculate_params_for_treeview (raAI_AB, tvRA_AB, true,
					Convert.ToInt32 (hscale_ai_a.Value),
					Convert.ToInt32 (hscale_ai_b.Value));
			race_analyzer_analyze_instant_calculate_params_for_treeview (raAI_CD, tvRA_CD, false,
					Convert.ToInt32 (hscale_ai_c.Value),
					Convert.ToInt32 (hscale_ai_d.Value));
		}

		// 6. treeviews fill
		tvS.ResetTreeview (); //To avoid duplicated rows on chained A,B
		tvS.FillTreeview ();

		// 7. hscales manage sensitive
		aiButtonsHscaleZoomSensitiveness();

		// 8. refresh graph
		ai_drawingarea_cairo.QueueDraw(); //will fire ExposeEvent

		LogB.Information (string.Format ("on_hscale_ai_value_changed {0} end", hscaleToDebug));
	}

	private void on_check_ai_chained_hscales_clicked (object o, EventArgs args)
	{
		image_force_sensor_ai_chained_hscales_link.Visible = check_ai_chained_hscales.Active;
		image_force_sensor_ai_chained_hscales_link_off.Visible = ! check_ai_chained_hscales.Active;

		if (Constants.ModeIsFORCESENSOR (current_mode))
		{
			AiVars.chainedDiffAtStartAB = hscale_ai_b.Value - hscale_ai_a.Value;
			AiVars.chainedDiffAtStartCD = hscale_ai_d.Value - hscale_ai_c.Value;
		} else //if (current_mode == Constants.Modes.RUNSENCODER)
		{
			AiVars.chainedDiffAtStartAB = raAI_AB.GetTimeMS (Convert.ToInt32 (hscale_ai_b.Value))
				- raAI_AB.GetTimeMS (Convert.ToInt32 (hscale_ai_a.Value));
			AiVars.chainedDiffAtStartCD = raAI_CD.GetTimeMS (Convert.ToInt32 (hscale_ai_d.Value))
				- raAI_CD.GetTimeMS (Convert.ToInt32 (hscale_ai_c.Value));
		}
	}

	private void aiButtonsHscaleZoomSensitiveness ()
	{
		Gtk.HScale hsLeft = getHScaleABCD (true);
		Gtk.HScale hsRight = getHScaleABCD (false);

		button_hscale_ai_a_first.Sensitive = hsLeft.Value > 0;
		button_hscale_ai_a_pre.Sensitive = hsLeft.Value > 0;
		button_hscale_ai_a_pre_1s.Sensitive = hsLeft.Value > 0;
		button_hscale_ai_b_first.Sensitive = hsRight.Value > 0;
		button_hscale_ai_b_pre.Sensitive = hsRight.Value > 0;
		button_hscale_ai_b_pre_1s.Sensitive = hsRight.Value > 0;

		AnalyzeInstant sAI = getCorrectAI ();
		button_hscale_ai_a_last.Sensitive = (sAI != null && hsLeft.Value < sAI.GetLength() -1);
		button_hscale_ai_a_post_1s.Sensitive = (sAI != null && hsLeft.Value < sAI.GetLength() -1);
		button_hscale_ai_a_post.Sensitive = (sAI != null && hsLeft.Value < sAI.GetLength() -1);
		button_hscale_ai_b_last.Sensitive = (sAI != null && hsRight.Value < sAI.GetLength() -1);
		button_hscale_ai_b_post_1s.Sensitive = (sAI != null && hsRight.Value < sAI.GetLength() -1);
		button_hscale_ai_b_post.Sensitive = (sAI != null && hsRight.Value < sAI.GetLength() -1);

		//if not in zoom, diff have to be more than one pixel
		check_ai_zoom.Sensitive = (AiVars.zoomApplied || Math.Abs(hsLeft.Value - hsRight.Value) > 1);
	}

	private void on_button_hscale_ai_a_first_clicked (object o, EventArgs args)
	{
		getHScaleABCD (true).Value = 0;
	}
	private void on_button_hscale_ai_a_pre_clicked (object o, EventArgs args)
	{
		HScale hs = getHScaleABCD (true);
		hs.Value -= 1;
	}
	private void on_button_hscale_ai_a_post_clicked (object o, EventArgs args)
	{
		HScale hs = getHScaleABCD (true);
		hs.Value += 1;
	}
	private void on_button_hscale_ai_a_last_clicked (object o, EventArgs args)
	{
		AnalyzeInstant sAI = getCorrectAI ();
		if(sAI == null || sAI.GetLength() < 2)
			return;

		getHScaleABCD (true).Value = sAI.GetLength() -1;
	}

	private void on_button_hscale_ai_b_first_clicked (object o, EventArgs args)
	{
		getHScaleABCD (false).Value = 0;
	}
	private void on_button_hscale_ai_b_pre_clicked (object o, EventArgs args)
	{
		HScale hs = getHScaleABCD (false);
		hs.Value -= 1;
	}
	private void on_button_hscale_ai_b_post_clicked (object o, EventArgs args)
	{
		HScale hs = getHScaleABCD (false);
		hs.Value += 1;
	}

	private void on_button_hscale_ai_b_last_clicked (object o, EventArgs args)
	{
		AnalyzeInstant sAI = getCorrectAI ();
		if(sAI == null || sAI.GetLength() < 2)
			return;

		getHScaleABCD (false).Value = sAI.GetLength() -1;
	}

	private void on_button_hscale_ai_a_pre_1s_clicked (object o, EventArgs args)
	{
		AnalyzeInstant sAI = getCorrectAI ();
		if(sAI == null || sAI.GetLength() == 0)
			return;

		Gtk.HScale hs = getHScaleABCD (true);
		int startA = Convert.ToInt32 (hs.Value);
		double startAMs = sAI.GetTimeMS(startA);
		for(int i = startA; i > 0; i --)
		{
			if(startAMs - sAI.GetTimeMS(i) >= preferences.forceSensorAnalyzeABSliderIncrement * 1000)
			{
				//hscale_ai_a.Value += i - startA; is the sample where condition is done,
				//but maybe the sample before that condition is more close to 1s than this
				if(MathUtil.PassedSampleIsCloserToCriteria (
							startAMs - sAI.GetTimeMS(i), startAMs - sAI.GetTimeMS(i+1),
							preferences.forceSensorAnalyzeABSliderIncrement * 1000))
					hs.Value += (i - startA);
				else
					hs.Value += (i+1 - startA);

				return;
			}
		}
	}
	private void on_button_hscale_ai_a_post_1s_clicked (object o, EventArgs args)
	{
		AnalyzeInstant sAI = getCorrectAI ();
		if(sAI == null || sAI.GetLength() == 0)
			return;

		Gtk.HScale hs = getHScaleABCD (true);
		int startA = Convert.ToInt32(hs.Value);
		double startAMs = sAI.GetTimeMS(startA);
		for(int i = startA; i < sAI.GetLength() -1; i ++)
		{
			if(sAI.GetTimeMS(i) - startAMs >= preferences.forceSensorAnalyzeABSliderIncrement * 1000)
			{
				//hscale_ai_a.Value += i - startA;
				if(MathUtil.PassedSampleIsCloserToCriteria (
						sAI.GetTimeMS(i) - startAMs, sAI.GetTimeMS(i-1) - startAMs,
						preferences.forceSensorAnalyzeABSliderIncrement * 1000))
					hs.Value += (i - startA);
				else
					hs.Value += (i-1 - startA);

				return;
			}
		}
	}

	private void on_button_hscale_ai_b_pre_1s_clicked (object o, EventArgs args)
	{
		AnalyzeInstant sAI = getCorrectAI ();
		if(sAI == null || sAI.GetLength() == 0)
			return;

		Gtk.HScale hs = getHScaleABCD (false);
		int startB = Convert.ToInt32(hs.Value);
		double startBMs = sAI.GetTimeMS(startB);
		for(int i = startB; i > 0; i --)
		{
			if(startBMs - sAI.GetTimeMS(i) >= preferences.forceSensorAnalyzeABSliderIncrement * 1000)
			{
				//hscale_ai_b.Value += i - startB;
				if(MathUtil.PassedSampleIsCloserToCriteria (
						startBMs - sAI.GetTimeMS(i), startBMs - sAI.GetTimeMS(i+1),
						preferences.forceSensorAnalyzeABSliderIncrement * 1000))
					hs.Value += (i - startB);
				else
					hs.Value += (i+1 - startB);

				return;
			}
		}
	}
	private void on_button_hscale_ai_b_post_1s_clicked (object o, EventArgs args)
	{
		AnalyzeInstant sAI = getCorrectAI ();
		if(sAI == null || sAI.GetLength() == 0)
			return;

		Gtk.HScale hs = getHScaleABCD (false);
		int startB = Convert.ToInt32(hs.Value);
		double startBMs = sAI.GetTimeMS(startB);
		for(int i = startB; i < sAI.GetLength() -1; i ++)
		{
			if(sAI.GetTimeMS(i) - startBMs >= preferences.forceSensorAnalyzeABSliderIncrement * 1000)
			{
				//hscale_ai_b.Value += i - startB;
				if(MathUtil.PassedSampleIsCloserToCriteria (
						sAI.GetTimeMS(i) - startBMs, sAI.GetTimeMS(i-1) - startBMs,
						preferences.forceSensorAnalyzeABSliderIncrement * 1000))
					hs.Value += (i - startB);
				else
					hs.Value += (i-1 - startB);

				return;
			}
		}
	}

	private void on_radio_ai_sets_toggled (object o, EventArgs args)
	{
		if (o == null || ! ((Gtk.RadioButton) o).Active)
			return;

		if ((Gtk.RadioButton) o == radio_ai_1set)
		{
			notebook_ai_load.Page = 0;
			radio_ai_cd.Sensitive = true;

			// if CD goes beyond AB, convert it to 0
			if (fsAI_AB != null)
			{
				hscale_ai_c.SetRange (0, fsAI_AB.GetLength() -1);
				hscale_ai_d.SetRange (0, fsAI_AB.GetLength() -1);
			}
		}
		else //((Gtk.RadioButton) o == radio_ai_2sets)
		{
			notebook_ai_load.Page = 1;

			button_signal_analyze_load_ab.Sensitive = radio_ai_ab.Active;
			button_signal_analyze_load_cd.Sensitive = radio_ai_cd.Active;
			button_ai_move_cd_pre_set_sensitivity ();

			//do not allow to click on cd if two sets (when there is no ab loaded)
			radio_ai_cd.Sensitive =	(
					(Constants.ModeIsFORCESENSOR (current_mode) &&
					 currentForceSensor != null && currentForceSensor.UniqueID >= 0) ||
					(current_mode == Constants.Modes.RUNSENCODER &&
					 currentRunEncoder != null && currentRunEncoder.UniqueID >= 0)
					);
		}
		signalPrepareGraphAI ();
		ai_drawingarea_cairo.QueueDraw(); //will fire ExposeEvent
	}

	private void on_radio_ai_abcd_toggled (object o, EventArgs args)
	{
		if (o == null || ! ((Gtk.RadioButton) o).Active)
			return;

		if ((Gtk.RadioButton) o == radio_ai_ab)
		{
			box_force_sensor_ai_a.Visible = true;
			box_force_sensor_ai_b.Visible = true;
			box_force_sensor_ai_c.Visible = false;
			box_force_sensor_ai_d.Visible = false;
			label_force_sensor_ai_zoom_abcd.Text = "[A-B]";
			UtilGtk.ViewportColor (viewport_ai_hscales, UtilGtk.Colors.YELLOW_LIGHT);
		}
		else if ((Gtk.RadioButton) o == radio_ai_cd)
		{
			box_force_sensor_ai_a.Visible = false;
			box_force_sensor_ai_b.Visible = false;
			box_force_sensor_ai_c.Visible = true;
			box_force_sensor_ai_d.Visible = true;
			label_force_sensor_ai_zoom_abcd.Text = "[C-D]";
			UtilGtk.ViewportColor (viewport_ai_hscales, UtilGtk.Colors.GREEN_LIGHT);
		}

		button_signal_analyze_load_ab.Sensitive = (radio_ai_2sets.Active && radio_ai_ab.Active);
		button_signal_analyze_load_cd.Sensitive = (radio_ai_2sets.Active && radio_ai_cd.Active);
		button_ai_move_cd_pre_set_sensitivity ();

		aiButtonsHscaleZoomSensitiveness();
		ai_drawingarea_cairo.QueueDraw(); //will fire ExposeEvent
	}

	private void radiosAiSensitivity (bool sensitive)
	{
		radio_ai_1set.Sensitive = sensitive;
		radio_ai_2sets.Sensitive = sensitive;
		radio_ai_ab.Sensitive = sensitive;
		radio_ai_cd.Sensitive = sensitive;
		notebook_ai_load.Sensitive = sensitive;
	}

	private int notebook_ai_top_LastPage;
	private void on_button_ai_model_options_clicked (object o, EventArgs args)
	{
		//store the notebook to return to same place
		notebook_ai_top_LastPage = notebook_ai_top.CurrentPage;
		notebook_ai_top.CurrentPage = Convert.ToInt32(notebook_ai_top_pages.AUTOMATICOPTIONS);

		hbox_ai_export_top_modes.Sensitive = false;
		button_ai_model_options_close_and_analyze.Visible = radio_signal_analyze_current_set.Active;

		if (Constants.ModeIsFORCESENSOR (current_mode))
		{
			notebook_ai_model_options.CurrentPage = 0;
			on_button_force_sensor_ai_model_options_clicked ();
		}
		else //if (current_mode == Constants.Modes.RUNSENCODER)
		{
			notebook_ai_model_options.CurrentPage = 1;
			on_button_run_encoder_ai_model_options_clicked ();
		}
	}

	private void on_button_ai_model_options_close_clicked (object o, EventArgs args)
	{
		notebook_ai_top.CurrentPage = notebook_ai_top_LastPage;
		hbox_ai_export_top_modes.Sensitive = true;

		if (Constants.ModeIsFORCESENSOR (current_mode))
			on_button_force_sensor_ai_model_options_close_clicked ();
		else //if (current_mode == Constants.Modes.RUNSENCODER)
			on_button_run_encoder_ai_model_options_close_clicked ();
	}

	private void on_button_ai_model_options_close_and_analyze_clicked (object o, EventArgs args)
	{
		on_button_ai_model_options_close_clicked (o, args);
		on_button_ai_model_clicked (o, args);
	}

	private void on_button_ai_model_clicked (object o, EventArgs args)
	{
		if (Constants.ModeIsFORCESENSOR (current_mode))
			on_button_force_sensor_analyze_model_clicked (o, args);
		else //if (current_mode == Constants.Modes.RUNSENCODER)
			on_button_run_encoder_analyze_analyze_clicked (o, args);
	}

	private void on_button_ai_model_save_image_clicked (object o, EventArgs args)
	{
		if (Constants.ModeIsFORCESENSOR (current_mode))
			on_button_force_sensor_image_save_model_clicked (o, args);
		else //if (current_mode == Constants.Modes.RUNSENCODER)
			on_button_run_encoder_image_save_model_clicked (o, args);
	}

	private void on_ai_export_not_set_clicked (object o, EventArgs args)
	{
		if (Constants.ModeIsFORCESENSOR (current_mode))
			on_button_force_sensor_export_not_set_clicked (o, args);
		else //if (current_mode == Constants.Modes.RUNSENCODER)
			on_button_run_encoder_export_not_set_clicked (o, args);
	}

	private void on_button_ai_export_cancel_clicked (object o, EventArgs args)
	{
		if (Constants.ModeIsFORCESENSOR (current_mode))
			forceSensorExport.Cancel();
		else //if (current_mode == Constants.Modes.RUNSENCODER)
			runEncoderExport.Cancel();
	}

	private void on_ai_export_result_open_clicked (object o, EventArgs args)
	{
		if (Constants.ModeIsFORCESENSOR (current_mode))
			on_button_force_sensor_export_result_open_clicked (o, args);
		else //if (current_mode == Constants.Modes.RUNSENCODER)
			on_button_run_encoder_export_result_open_clicked (o, args);
	}

	private void connectWidgetsSignalAnalyze (Gtk.Builder builder)
	{
		LogB.Information ("connectWidgetsSignalAnalyze");

		box_ai_move_cd_accept = (Gtk.Box) builder.GetObject ("box_ai_move_cd_accept");
		box_ai_move_cd_buttons = (Gtk.Box) builder.GetObject ("box_ai_move_cd_buttons");

		button_ai_move_cd_pre_1s = (Gtk.Button) builder.GetObject ("button_ai_move_cd_pre_1s");
		button_ai_move_cd_pre_1ds = (Gtk.Button) builder.GetObject ("button_ai_move_cd_pre_1ds");
		button_ai_move_cd_pre_1cs = (Gtk.Button) builder.GetObject ("button_ai_move_cd_pre_1cs");
		button_ai_move_cd_post_1cs = (Gtk.Button) builder.GetObject ("button_ai_move_cd_post_1cs");
		button_ai_move_cd_post_1ds = (Gtk.Button) builder.GetObject ("button_ai_move_cd_post_1ds");
		button_ai_move_cd_post_1s = (Gtk.Button) builder.GetObject ("button_ai_move_cd_post_1s");
		button_ai_move_cd_align_left = (Gtk.Button) builder.GetObject ("button_ai_move_cd_align_left");
		button_ai_move_cd_align_center = (Gtk.Button) builder.GetObject ("button_ai_move_cd_align_center");
		button_ai_move_cd_align_right = (Gtk.Button) builder.GetObject ("button_ai_move_cd_align_right");

		box_ai_ac = (Gtk.Box) builder.GetObject ("box_ai_ac");
		box_ai_bd = (Gtk.Box) builder.GetObject ("box_ai_bd");
		box_ai_a_buttons = (Gtk.Box) builder.GetObject ("box_ai_a_buttons");
		box_ai_b_buttons = (Gtk.Box) builder.GetObject ("box_ai_b_buttons");

		label_hscale_ai_a_pre_1s = (Gtk.Label) builder.GetObject ("label_hscale_ai_a_pre_1s");
		label_hscale_ai_a_post_1s = (Gtk.Label) builder.GetObject ("label_hscale_ai_a_post_1s");
		label_hscale_ai_b_pre_1s = (Gtk.Label) builder.GetObject ("label_hscale_ai_b_pre_1s");
		label_hscale_ai_b_post_1s = (Gtk.Label) builder.GetObject ("label_hscale_ai_b_post_1s");

		button_hscale_ai_a_first = (Gtk.Button) builder.GetObject ("button_hscale_ai_a_first");
		button_hscale_ai_a_pre = (Gtk.Button) builder.GetObject ("button_hscale_ai_a_pre");
		button_hscale_ai_a_pre_1s = (Gtk.Button) builder.GetObject ("button_hscale_ai_a_pre_1s");
		button_hscale_ai_a_post = (Gtk.Button) builder.GetObject ("button_hscale_ai_a_post");
		button_hscale_ai_a_post_1s = (Gtk.Button) builder.GetObject ("button_hscale_ai_a_post_1s");
		button_hscale_ai_a_last = (Gtk.Button) builder.GetObject ("button_hscale_ai_a_last");

		button_hscale_ai_b_first = (Gtk.Button) builder.GetObject ("button_hscale_ai_b_first");
		button_hscale_ai_b_pre = (Gtk.Button) builder.GetObject ("button_hscale_ai_b_pre");
		button_hscale_ai_b_pre_1s = (Gtk.Button) builder.GetObject ("button_hscale_ai_b_pre_1s");
		button_hscale_ai_b_post = (Gtk.Button) builder.GetObject ("button_hscale_ai_b_post");
		button_hscale_ai_b_post_1s = (Gtk.Button) builder.GetObject ("button_hscale_ai_b_post_1s");
		button_hscale_ai_b_last = (Gtk.Button) builder.GetObject ("button_hscale_ai_b_last");

		hscale_ai_a = (Gtk.HScale) builder.GetObject ("hscale_ai_a");
		hscale_ai_b = (Gtk.HScale) builder.GetObject ("hscale_ai_b");
		hscale_ai_c = (Gtk.HScale) builder.GetObject ("hscale_ai_c");
		hscale_ai_d = (Gtk.HScale) builder.GetObject ("hscale_ai_d");

		//grid_radios_ai = (Gtk.Grid) builder.GetObject ("grid_radios_ai");
		radio_ai_1set = (Gtk.RadioButton) builder.GetObject ("radio_ai_1set");
		radio_ai_2sets = (Gtk.RadioButton) builder.GetObject ("radio_ai_2sets");
		notebook_ai_load = (Gtk.Notebook) builder.GetObject ("notebook_ai_load");
		viewport_radio_ai_ab = (Gtk.Viewport) builder.GetObject ("viewport_radio_ai_ab");
		viewport_radio_ai_cd = (Gtk.Viewport) builder.GetObject ("viewport_radio_ai_cd");
		radio_ai_ab = (Gtk.RadioButton) builder.GetObject ("radio_ai_ab");
		radio_ai_cd = (Gtk.RadioButton) builder.GetObject ("radio_ai_cd");
		box_ai_cd_buttons = (Gtk.Box) builder.GetObject ("box_ai_cd_buttons");

		viewport_ai_hscales = (Gtk.Viewport) builder.GetObject ("viewport_ai_hscales");
		check_ai_chained_hscales = (Gtk.CheckButton) builder.GetObject ("check_ai_chained_hscales");
		check_ai_zoom = (Gtk.CheckButton) builder.GetObject ("check_ai_zoom");

		button_ai_model = (Gtk.Button) builder.GetObject ("button_ai_model");
		notebook_ai_model_options = (Gtk.Notebook) builder.GetObject ("notebook_ai_model_options");
		button_ai_model_options_close_and_analyze = (Gtk.Button) builder.GetObject ("button_ai_model_options_close_and_analyze");
		button_ai_model_options = (Gtk.Button) builder.GetObject ("button_ai_model_options");
		label_model_analyze = (Gtk.Label) builder.GetObject ("label_model_analyze");
		viewport_ai_model_graph = (Gtk.Viewport) builder.GetObject ("viewport_ai_model_graph");
		image_ai_model_graph = (Gtk.Image) builder.GetObject ("image_ai_model_graph");
		button_ai_model_save_image = (Gtk.Button) builder.GetObject ("button_ai_model_save_image");
		notebook_ai_model_graph_table_triggers = (Gtk.Notebook) builder.GetObject ("notebook_ai_model_graph_table_triggers");
		label_model_triggers_found = (Gtk.Label) builder.GetObject ("label_model_triggers_found");

		radio_ai_export_individual_current_session = (Gtk.RadioButton) builder.GetObject ("radio_ai_export_individual_current_session");
		radio_ai_export_individual_all_sessions = (Gtk.RadioButton) builder.GetObject ("radio_ai_export_individual_all_sessions");
		radio_ai_export_groupal_current_session = (Gtk.RadioButton) builder.GetObject ("radio_ai_export_groupal_current_session");
		label_ai_export_person = (Gtk.Label) builder.GetObject ("label_ai_export_person");
		label_ai_export_session = (Gtk.Label) builder.GetObject ("label_ai_export_session");
		hbox_ai_export_images = (Gtk.HBox) builder.GetObject ("hbox_ai_export_images");
		hbox_ai_export_width_height = (Gtk.HBox) builder.GetObject ("hbox_ai_export_width_height");
		spinbutton_ai_export_image_width = (Gtk.SpinButton) builder.GetObject ("spinbutton_ai_export_image_width");
		spinbutton_ai_export_image_height = (Gtk.SpinButton) builder.GetObject ("spinbutton_ai_export_image_height");
		check_ai_export_images = (Gtk.CheckButton) builder.GetObject ("check_ai_export_images");
		notebook_ai_export = (Gtk.Notebook) builder.GetObject ("notebook_ai_export");
		label_ai_export = (Gtk.Label) builder.GetObject ("label_ai_export");
		progressbar_ai_export = (Gtk.ProgressBar) builder.GetObject ("progressbar_ai_export");
		label_ai_export_result = (Gtk.Label) builder.GetObject ("label_ai_export_result");
		button_ai_export_result_open = (Gtk.Button) builder.GetObject ("button_ai_export_result_open");
	}
}

/*
 * To have some variables separated on a single class (to make in the future easier to reutilize on RaceAnalyzer)
 * Maybe put more zoom variables here
 */
public static class AiVars
{
	public static bool zoomApplied;
	public static bool hscalesDoNotFollow = false;

	public static int a_beforeZoom = 0;
	public static int a_atZoom = 0;
	public static int b_beforeZoom = 0;
	public static int b_atZoom = 0;
	public static int c_beforeZoom = 0;
	public static int c_atZoom = 0;
	public static int d_beforeZoom = 0;
	public static int d_atZoom = 0;

	// on forceSensor diff is samples, on raceAnalyzer is time as here is lot variable
	// double because raceAnalyzer time is returned (or stored) as double
	public static double chainedDiffAtStartAB;
	public static double chainedDiffAtStartCD;
}

//SignalAbstract
public abstract class TreeviewSAbstract
{
	protected TreeStore store;
	protected Gtk.TreeView tv;
	protected string [] columnsString;

	protected void createTreeview ()
	{
		tv = UtilGtk.RemoveColumns (tv);
		columnsString = setColumnsString ();
		store = UtilGtk.GetStore (columnsString.Length);
		tv.Model = store;
		prepareHeaders (columnsString);
		tv.HeadersClickable = false;
	}

	protected virtual string [] setColumnsString ()
	{
		return new string [] {};
	}

	protected void prepareHeaders(string [] columnsString)
	{
		tv.HeadersVisible = true;
		int i = 0;
		foreach (string myCol in columnsString)
			UtilGtk.CreateCols (tv, store, myCol, i++, true);
	}

	public void ResetTreeview ()
	{
		if (tv != null)
			tv = UtilGtk.RemoveColumns (tv);

		createTreeview ();
	}
}

public abstract class TreeviewS2Abstract : TreeviewSAbstract
{
	//row 1
	protected string timeStart;
	//row 2
	protected string timeEnd;
	//row 3
	protected string timeDiff;

	protected abstract string [] getTreeviewStr ();
	protected abstract string [] fillTreeViewStart (string [] str, int i);
	protected abstract string [] fillTreeViewEnd (string [] str, int i);
	protected abstract string [] fillTreeViewDiff (string [] str, int i);
	protected abstract string [] fillTreeViewAvg (string [] str, int i);
	protected abstract string [] fillTreeViewMax (string [] str, int i);

	public virtual void FillTreeview ()
	{
		string [] str = getTreeviewStr ();
		store.AppendValues (fillTreeViewStart (str, 0));
		store.AppendValues (fillTreeViewEnd (str, 0));
		store.AppendValues (fillTreeViewDiff (str, 0));
		store.AppendValues (fillTreeViewAvg (str, 0));
		store.AppendValues (fillTreeViewMax (str, 0));
	}

	//some are string because it is easier to know if missing data, because doble could be 0.00000001 ...
	public void PassTime1or2 (bool isLeft, string time)
	{
		if (isLeft)
			this.timeStart = time;
		else
			this.timeEnd = time;
	}

	/*
	 * forceSensor specific
	 */
	public virtual void PassForceAndRFD1or2 (bool isLeft, double force, string rfd)
	{
	}

	public virtual void PassRow1or2Elastic (bool isLeft, string position, string speed, string accel, string power)
	{
	}

	public virtual void PassElasticDiffs (string position, string speed, string accel, string power)
	{
	}
	public virtual void PassElasticAvgs (string speed, string accel, string power)
	{
	}
	public virtual void PassElasticMaxs (string speed, string accel, string power)
	{
	}

	/*
	 * raceAnalyzer specific
	 */
	public virtual void PassSpeed1or2 (bool isLeft, double speed)
	{
	}
	public virtual void PassSpeedAvg (double speed)
	{
	}
	public virtual void PassSpeedMax (double speed)
	{
	}

	/*
	 * accessors
	 */
	public string TimeStart {
		get { return timeStart; }
	}
	public string TimeEnd {
		get { return timeEnd; }
	}

	// this accessors help to pass variables once are calculated on force_sensor_analyze_instant_calculate_params
	public string TimeDiff {
		get { return timeDiff; }
		set { timeDiff = value; }
	}
}
