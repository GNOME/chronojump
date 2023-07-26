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
	private void forceSensorZoomDefaultValues()
	{
		FsAiVars.zoomApplied = false;
		check_force_sensor_ai_zoom.Active = false;
	}

	//private double hscale_fs_ai_a_BeforeZoomTimeMS = 0; //to calculate triggers

	private void on_check_force_sensor_ai_zoom_clicked (object o, EventArgs args)
	{
		AnalyzeInstant sAI = getCorrectAI ();

		if(sAI == null || sAI.GetLength() == 0)
			return;

		if(check_force_sensor_ai_zoom.Active)
		{
			FsAiVars.zoomApplied = true;

			//store hscale a to help return to position on unzoom
			FsAiVars.a_beforeZoom = Convert.ToInt32 (hscale_fs_ai_a.Value);
			FsAiVars.b_beforeZoom = Convert.ToInt32 (hscale_fs_ai_b.Value);
			FsAiVars.c_beforeZoom = Convert.ToInt32 (hscale_fs_ai_c.Value);
			FsAiVars.d_beforeZoom = Convert.ToInt32 (hscale_fs_ai_d.Value);

			if (radio_force_sensor_ai_2sets.Active)
			{
				// zoomed has to be the same range for ab than cd, to show all data in graph. range is related to what is selected in the ratio
				int sampleL;
				int sampleR;

				if (spCairoFE_CD.TimeShifted) //time has shifted (not as samples, is directly time, so need to find sample that matches that time)
				{
					if (radio_force_sensor_ai_ab.Active)
					{
						// ab data is the hscales data
						spCairoFEZoom = new SignalPointsCairoForceElastic (spCairoFE,
								FsAiVars.a_beforeZoom, FsAiVars.b_beforeZoom, true);

						// cd data are samples close in time to ab data
						// 1st check if it overlaps, if it does not overlap and we include it, it would show a bigger graph with empty data
						if (! PointF.ListsTimeOverlap (spCairoFE_CD.Force_l, spCairoFE.Force_l, FsAiVars.a_beforeZoom, FsAiVars.b_beforeZoom))
							spCairoFEZoom_CD = new SignalPointsCairoForceElastic ();
						else {
							sampleL = PointF.FindSampleCloseToTime (
									spCairoFE_CD.Force_l, spCairoFE.Force_l[FsAiVars.a_beforeZoom].X);
							sampleR = PointF.FindSampleCloseToTime (
									spCairoFE_CD.Force_l, spCairoFE.Force_l[FsAiVars.b_beforeZoom].X);
							spCairoFEZoom_CD = new SignalPointsCairoForceElastic (spCairoFE_CD,
									sampleL, sampleR, true);
						}
					} else {
						// cd data is the hscales data
						spCairoFEZoom_CD = new SignalPointsCairoForceElastic (spCairoFE_CD,
								FsAiVars.c_beforeZoom, FsAiVars.d_beforeZoom, true);

						// ab data are samples close in time to cd data
						// 1st check if it overlaps, if it does not overlap and we include it, it would show a bigger graph with empty data
						if (! PointF.ListsTimeOverlap (spCairoFE.Force_l, spCairoFE_CD.Force_l, FsAiVars.c_beforeZoom, FsAiVars.d_beforeZoom))
							spCairoFEZoom = new SignalPointsCairoForceElastic ();
						else {
							sampleL = PointF.FindSampleCloseToTime (
									spCairoFE.Force_l, spCairoFE_CD.Force_l[FsAiVars.c_beforeZoom].X);
							sampleR = PointF.FindSampleCloseToTime (
									spCairoFE.Force_l, spCairoFE_CD.Force_l[FsAiVars.d_beforeZoom].X);
							spCairoFEZoom = new SignalPointsCairoForceElastic (spCairoFE,
									sampleL, sampleR, true);
						}
					}
				} else {
					if (radio_force_sensor_ai_ab.Active)
					{
						sampleL = FsAiVars.a_beforeZoom;
						sampleR = FsAiVars.b_beforeZoom;
					} else {
						sampleL = FsAiVars.c_beforeZoom;
						sampleR = FsAiVars.d_beforeZoom;
					}

					spCairoFEZoom = new SignalPointsCairoForceElastic (spCairoFE,
							sampleL, sampleR, true);
					spCairoFEZoom_CD = new SignalPointsCairoForceElastic (spCairoFE_CD,
							sampleL, sampleR, true);
				}
			} else {
				if (radio_force_sensor_ai_ab.Active)
					spCairoFEZoom = new SignalPointsCairoForceElastic (spCairoFE,
							FsAiVars.a_beforeZoom, FsAiVars.b_beforeZoom, true);
				else
					spCairoFEZoom = new SignalPointsCairoForceElastic (spCairoFE,
							FsAiVars.c_beforeZoom, FsAiVars.d_beforeZoom, true);
			}

			//cairo
			if (Constants.ModeIsFORCESENSOR (current_mode))
			{
				FsAiVars.rep_lZoomAppliedCairo = new List<ForceSensorRepetition> ();
				for (int r = 0; r < sAI.ForceSensorRepetition_l.Count; r ++)
				{
					// don't do like this until delete non-cairo because this changes the sAI.ForceSensorRepetition_l values and non-cairo is not displayed correctly
					//ForceSensorRepetition fsr = sAI.ForceSensorRepetition_l[r];
					// do this:
					ForceSensorRepetition fsr = sAI.ForceSensorRepetition_l[r].Clone();

					fsr.sampleStart -= FsAiVars.a_beforeZoom;
					fsr.sampleEnd -= FsAiVars.a_beforeZoom;

					FsAiVars.rep_lZoomAppliedCairo.Add (fsr);
				}
			}

			forceSensorPrepareGraphAI ();

			image_force_sensor_ai_zoom.Visible = false;
			image_force_sensor_ai_zoom_out.Visible = true;
			radiosForceSensorAiSensitivity (false);
		} else {
			FsAiVars.zoomApplied = false;

			if (radio_force_sensor_ai_ab.Active)
			{
				FsAiVars.a_atZoom = Convert.ToInt32 (hscale_fs_ai_a.Value);
				FsAiVars.b_atZoom = Convert.ToInt32 (hscale_fs_ai_b.Value);
			} else {
				FsAiVars.c_atZoom = Convert.ToInt32 (hscale_fs_ai_c.Value);
				FsAiVars.d_atZoom = Convert.ToInt32 (hscale_fs_ai_d.Value);
			}

			forceSensorPrepareGraphAI ();

			if (radio_force_sensor_ai_ab.Active)
			{
				// set hscales a,b to value before + value at zoom (because user maybe changed it on zoom)
				hscale_fs_ai_a.Value = FsAiVars.a_beforeZoom +
					(FsAiVars.a_atZoom);
				hscale_fs_ai_b.Value = FsAiVars.a_beforeZoom +
					(FsAiVars.b_atZoom);

				// set hscales c,d at same value before zoom
				hscale_fs_ai_c.Value = FsAiVars.c_beforeZoom;
				hscale_fs_ai_d.Value = FsAiVars.d_beforeZoom;
			} else {
				hscale_fs_ai_a.Value = FsAiVars.a_beforeZoom;
				hscale_fs_ai_b.Value = FsAiVars.b_beforeZoom;

				hscale_fs_ai_c.Value = FsAiVars.c_beforeZoom +
					(FsAiVars.c_atZoom);
				hscale_fs_ai_d.Value = FsAiVars.c_beforeZoom +
					(FsAiVars.d_atZoom);
			}

			image_force_sensor_ai_zoom.Visible = true;
			image_force_sensor_ai_zoom_out.Visible = false;
			radiosForceSensorAiSensitivity (true);
		}
	}

	private Gtk.HScale getHScaleABCD (bool left)
	{
		if (radio_force_sensor_ai_ab.Active)
		{
			if (left)
				return hscale_fs_ai_a;
			else
				return hscale_fs_ai_b;
		} else {
			if (left)
				return hscale_fs_ai_c;
			else
				return hscale_fs_ai_d;
		}
	}

	private AnalyzeInstant getCorrectAI () // TODO: rename getCorrectAI
	{
		if (Constants.ModeIsFORCESENSOR (current_mode))
		{
			if (radio_force_sensor_ai_ab.Active)
				return fsAI_AB;
			else
				return fsAI_CD;
		} else { //if (current_mode == Constants.Modes.RUNSENCODER)
			if (radio_force_sensor_ai_ab.Active)
				return raAI_AB;
			else
				return raAI_CD;
		}
	}

	private void on_hscale_fs_ai_value_changed (object o, EventArgs args)
	{
		LogB.Information ("on_hscale_fs_ai_value_changed");
		AnalyzeInstant sAI = getCorrectAI ();

		if(sAI == null || sAI.GetLength() == 0)
			return;

		Gtk.HScale hs = (Gtk.HScale) o;

		if (check_force_sensor_ai_chained_hscales.Active)
		{
			bool isLeft; //A or C
			Gtk.HScale hsRelated ; //if A then B, if D then C
			int last;
			int previousDiffWithRelated;
			if (hs == hscale_fs_ai_a)
			{
				isLeft = true; //A or C
				hsRelated = hscale_fs_ai_b; //if A then B, if D then C
				last = FsAiVars.a_last;
				previousDiffWithRelated = FsAiVars.b_last - FsAiVars.a_last;
			}
			else if (hs == hscale_fs_ai_b)
			{
				isLeft = false;
				hsRelated = hscale_fs_ai_a;
				last = FsAiVars.b_last;
				previousDiffWithRelated = FsAiVars.b_last - FsAiVars.a_last;
			}
			else if (hs == hscale_fs_ai_c)
			{
				isLeft = true;
				hsRelated = hscale_fs_ai_d;
				last = FsAiVars.c_last;
				previousDiffWithRelated = FsAiVars.d_last - FsAiVars.c_last;
			}
			else //if (hs == hscale_fs_ai_d)
			{
				isLeft = false;
				hsRelated = hscale_fs_ai_c;
				last = FsAiVars.d_last;
				previousDiffWithRelated = FsAiVars.d_last - FsAiVars.c_last;
			}

			if (! FsAiVars.hscalesDoNotFollow)
			{
				FsAiVars.hscalesDoNotFollow = true;
				int diffWithPrevious = Convert.ToInt32 (hs.Value) - last;
				if (isLeft && Convert.ToInt32 (hsRelated.Value) + diffWithPrevious >= sAI.GetLength() -1)
				{
					hsRelated.Value = sAI.GetLength () -1;
					hs.Value = hsRelated.Value - previousDiffWithRelated;
				}
				else if (! isLeft && Convert.ToInt32 (hsRelated.Value) + diffWithPrevious <= 0)
				{
					hsRelated.Value = 0;
					hs.Value = hsRelated.Value + previousDiffWithRelated;
				}
				else
					hsRelated.Value += diffWithPrevious;

				FsAiVars.hscalesDoNotFollow = false;
			}

			if (hs == hscale_fs_ai_a)
				FsAiVars.b_last = Convert.ToInt32 (hsRelated.Value);
			else if (hs == hscale_fs_ai_b)
				FsAiVars.a_last = Convert.ToInt32 (hsRelated.Value);
			else if (hs == hscale_fs_ai_c)
				FsAiVars.d_last = Convert.ToInt32 (hsRelated.Value);
			else if (hs == hscale_fs_ai_d)
				FsAiVars.c_last = Convert.ToInt32 (hsRelated.Value);
		}

		hscale_fs_ai_value_changed_do (sAI, hs);
	}

	//can be convinient to call it directly
	private void hscale_fs_ai_value_changed_do (AnalyzeInstant sAI, HScale hs)
	{
		// 1. set some variables to make this function work for the four hscales
		bool isLeft; //A or C
		Gtk.HScale hsRelated ; //if A then B, if D then C
		string hscaleToDebug;
		TreeviewFSAnalyze tvFS;

		if (hs == hscale_fs_ai_a)
		{
			tvFS = tvFS_AB;
			isLeft = true; //A or C
			hsRelated = hscale_fs_ai_b; //if A then B, if D then C
			hscaleToDebug = "--- hscale_a ---";
		}
		else if (hs == hscale_fs_ai_b)
		{
			tvFS = tvFS_AB;
			isLeft = false;
			hsRelated = hscale_fs_ai_a;
			hscaleToDebug = "--- hscale_b ---";
		}
		else if (hs == hscale_fs_ai_c)
		{
			tvFS = tvFS_CD;
			isLeft = true;
			hsRelated = hscale_fs_ai_d;
			hscaleToDebug = "--- hscale_c ---";
		}
		else //if (hs == hscale_fs_ai_d)
		{
			tvFS = tvFS_CD;
			isLeft = false;
			hsRelated = hscale_fs_ai_c;
			hscaleToDebug = "--- hscale_d ---";
		}

		/*
		LogB.Information (string.Format ("on_hscale_fs_ai_value_changed {0} 0", hscaleToDebug));
		LogB.Information ("hscales at start: ");
		LogB.Information ("hscales a: " + hscale_fs_ai_a.Value.ToString ());
		LogB.Information ("hscales b: " + hscale_fs_ai_b.Value.ToString ());
		LogB.Information ("hscales c: " + hscale_fs_ai_c.Value.ToString ());
		LogB.Information ("hscales d: " + hscale_fs_ai_d.Value.ToString ());
		*/

		//LogB.Information (string.Format ("on_hscale_fs_ai_value_changed {0} 1", hscaleToDebug));

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
		LogB.Information (string.Format ("on_hscale_fs_ai_value_changed {0} 2", hscaleToDebug));

		// 3. calculate RFD
		string rfd = "";
		if (Constants.ModeIsFORCESENSOR (current_mode))
			if(count > 0 && count < sAI.GetLength() -1)
				rfd = Math.Round(sAI.CalculateRFD(count -1, count +1), 1).ToString();

		// 4. calculate elastic variables
		if (Constants.ModeIsFORCESENSOR (current_mode))
		{
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

			// 5. treeviews prepare
			tvFS.ResetTreeview ();
			tvFS.PassRow1or2 (isLeft, Math.Round(sAI.GetTimeMS(count), 1).ToString(), sAI.GetForceAtCount (count), rfd);
			//fix a bug where B is moved and not A (so A is empty)
			if (! isLeft && (tvFS.TimeStart == null || tvFS.TimeStart == ""))
				tvFS.PassRow1or2 (true, Math.Round(sAI.GetTimeMS(0), 1).ToString(), sAI.GetForceAtCount (0), rfd);

			if (current_mode == Constants.Modes.FORCESENSORELASTIC)
				tvFS.PassRow1or2Elastic (isLeft, position, speed, accel, power);
		}

		//LogB.Information (string.Format ("on_hscale_fs_ai_value_changed {0} 3", hscaleToDebug));
		// update FsAiVars.a_last, ...
		if (hs == hscale_fs_ai_a)
			FsAiVars.a_last = Convert.ToInt32 (hs.Value);
		else if (hs == hscale_fs_ai_b)
			FsAiVars.b_last = Convert.ToInt32 (hs.Value);
		else if (hs == hscale_fs_ai_c)
			FsAiVars.c_last = Convert.ToInt32 (hs.Value);
		else if (hs == hscale_fs_ai_d)
			FsAiVars.d_last = Convert.ToInt32 (hs.Value);

		//LogB.Information (string.Format ("on_hscale_fs_ai_value_changed {0} 6", hscaleToDebug));
		if (FsAiVars.hscalesDoNotFollow)
		{
			FsAiVars.hscalesDoNotFollow = false;

			if (Constants.ModeIsFORCESENSOR (current_mode))
			{
				tvFS.ResetTreeview (); //To avoid duplicated rows on chained A,B
				tvFS.FillTreeview ();
			}

			return;
		}

		if (Constants.ModeIsFORCESENSOR (current_mode))
		{
			//need to do both to ensure at unzoom params are calculated for AB and CD
			force_sensor_analyze_instant_calculate_params (fsAI_AB, tvFS_AB, true,
					Convert.ToInt32 (hscale_fs_ai_a.Value),
					Convert.ToInt32 (hscale_fs_ai_b.Value));
			force_sensor_analyze_instant_calculate_params (fsAI_CD, tvFS_CD, false,
					Convert.ToInt32 (hscale_fs_ai_c.Value),
					Convert.ToInt32 (hscale_fs_ai_d.Value));

			// 6. treeviews fill
			tvFS.ResetTreeview (); //To avoid duplicated rows on chained A,B
			tvFS.FillTreeview ();
		}

		// 7. hscales manage sensitive
		forceSensorAnalyzeGeneralButtonHscaleZoomSensitiveness();

		// 8. refresh graph
		force_sensor_ai_drawingarea_cairo.QueueDraw(); //will fire ExposeEvent
		LogB.Information (string.Format ("on_hscale_fs_ai_value_changed {0} 8", hscaleToDebug));
	}

	private void on_check_force_sensor_ai_chained_hscales_clicked (object o, EventArgs args)
	{
		image_force_sensor_ai_chained_hscales_link.Visible = check_force_sensor_ai_chained_hscales.Active;
		image_force_sensor_ai_chained_hscales_link_off.Visible = ! check_force_sensor_ai_chained_hscales.Active;
	}

	private void forceSensorAnalyzeGeneralButtonHscaleZoomSensitiveness()
	{
		Gtk.HScale hsLeft = getHScaleABCD (true);
		Gtk.HScale hsRight = getHScaleABCD (false);

		button_hscale_fs_ai_a_first.Sensitive = hsLeft.Value > 0;
		button_hscale_fs_ai_a_pre.Sensitive = hsLeft.Value > 0;
		button_hscale_fs_ai_a_pre_1s.Sensitive = hsLeft.Value > 0;
		button_hscale_fs_ai_b_first.Sensitive = hsRight.Value > 0;
		button_hscale_fs_ai_b_pre.Sensitive = hsRight.Value > 0;
		button_hscale_fs_ai_b_pre_1s.Sensitive = hsRight.Value > 0;

		AnalyzeInstant sAI = getCorrectAI ();
		button_hscale_fs_ai_a_last.Sensitive = (sAI != null && hsLeft.Value < sAI.GetLength() -1);
		button_hscale_fs_ai_a_post_1s.Sensitive = (sAI != null && hsLeft.Value < sAI.GetLength() -1);
		button_hscale_fs_ai_a_post.Sensitive = (sAI != null && hsLeft.Value < sAI.GetLength() -1);
		button_hscale_fs_ai_b_last.Sensitive = (sAI != null && hsRight.Value < sAI.GetLength() -1);
		button_hscale_fs_ai_b_post_1s.Sensitive = (sAI != null && hsRight.Value < sAI.GetLength() -1);
		button_hscale_fs_ai_b_post.Sensitive = (sAI != null && hsRight.Value < sAI.GetLength() -1);

		//diff have to be more than one pixel
		check_force_sensor_ai_zoom.Sensitive = (Math.Abs(hsLeft.Value - hsRight.Value) > 1);
	}

	private void on_button_hscale_fs_ai_a_first_clicked (object o, EventArgs args)
	{
		getHScaleABCD (true).Value = 0;
	}
	private void on_button_hscale_fs_ai_a_pre_clicked (object o, EventArgs args)
	{
		HScale hs = getHScaleABCD (true);
		hs.Value -= 1;
	}
	private void on_button_hscale_fs_ai_a_post_clicked (object o, EventArgs args)
	{
		HScale hs = getHScaleABCD (true);
		hs.Value += 1;
	}
	private void on_button_hscale_fs_ai_a_last_clicked (object o, EventArgs args)
	{
		AnalyzeInstant sAI = getCorrectAI ();
		if(sAI == null || sAI.GetLength() < 2)
			return;

		getHScaleABCD (true).Value = sAI.GetLength() -1;
	}

	private void on_button_hscale_fs_ai_b_first_clicked (object o, EventArgs args)
	{
		getHScaleABCD (false).Value = 0;
	}
	private void on_button_hscale_fs_ai_b_pre_clicked (object o, EventArgs args)
	{
		HScale hs = getHScaleABCD (false);
		hs.Value -= 1;
	}
	private void on_button_hscale_fs_ai_b_post_clicked (object o, EventArgs args)
	{
		HScale hs = getHScaleABCD (false);
		hs.Value += 1;
	}

	private void on_button_hscale_fs_ai_b_last_clicked (object o, EventArgs args)
	{
		AnalyzeInstant sAI = getCorrectAI ();
		if(sAI == null || sAI.GetLength() < 2)
			return;

		getHScaleABCD (false).Value = sAI.GetLength() -1;
	}

	private void on_button_hscale_fs_ai_a_pre_1s_clicked (object o, EventArgs args)
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
				//hscale_fs_ai_a.Value += i - startA; is the sample where condition is done,
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
	private void on_button_hscale_fs_ai_a_post_1s_clicked (object o, EventArgs args)
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
				//hscale_fs_ai_a.Value += i - startA;
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

	private void on_button_hscale_fs_ai_b_pre_1s_clicked (object o, EventArgs args)
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
				//hscale_fs_ai_b.Value += i - startB;
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
	private void on_button_hscale_fs_ai_b_post_1s_clicked (object o, EventArgs args)
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
				//hscale_fs_ai_b.Value += i - startB;
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

}

