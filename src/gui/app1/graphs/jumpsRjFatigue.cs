
/*
 * This file is part of ChronoJump
 *
 * Chronojump is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or   
 *    (at your option) any later version.
 *    
 * Chronojump is distributed in the hope that it will be useful,
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
//using Glade;
using Mono.Unix;
using System.Collections.Generic; //List<T> 

public partial class ChronoJumpWindow 
{
	// at glade ---->
	Gtk.DrawingArea drawingarea_jumps_rj_fatigue;
	Gtk.Button button_jumps_rj_fatigue_save_image;
	Gtk.Image image_jumps_rj_fatigue_save;
	Gtk.Image image_jumps_rj_fatigue_image_save;

	Gtk.HBox hbox_combo_select_jumps_rj_fatigue;
	Gtk.HBox hbox_combo_select_jumps_rj_fatigue_num;
	Gtk.Box box_combo_jumps_rj_fatigue_divide_in;

	Gtk.RadioButton radio_jumps_rj_fatigue_heights;
	Gtk.RadioButton radio_jumps_rj_fatigue_tv_tc;
	Gtk.RadioButton radio_jumps_rj_fatigue_rsi;
	// <---- at glade

	Gtk.ComboBoxText combo_select_jumps_rj_fatigue;
	Gtk.ComboBoxText combo_select_jumps_rj_fatigue_num;
	Gtk.ComboBoxText combo_jumps_rj_fatigue_divide_in;

	JumpsRjFatigue jumpsRjFatigue;
	JumpsRjFatigueGraph jumpsRjFatigueGraph;
	CjComboSelectJumpsRj comboSelectJumpsRjFatigue;
	CjComboGeneric comboSelectJumpsRjFatigueNum; //it has num and date

	// combo comboSelectJumpsRjFatigue (start)
	private void createComboSelectJumpsRjFatigue (bool create)
	{
		if(create)
		{
			comboSelectJumpsRjFatigue = new CjComboSelectJumpsRj(combo_select_jumps_rj_fatigue, hbox_combo_select_jumps_rj_fatigue);
			combo_select_jumps_rj_fatigue = comboSelectJumpsRjFatigue.Combo;
			combo_select_jumps_rj_fatigue.Changed += new EventHandler (on_combo_select_jumps_rj_fatigue_changed);
		} else {
			comboSelectJumpsRjFatigue.Fill();
			combo_select_jumps_rj_fatigue = comboSelectJumpsRjFatigue.Combo;
		}
		combo_select_jumps_rj_fatigue.Sensitive = true;
	}
	private void on_combo_select_jumps_rj_fatigue_changed (object o, EventArgs args)
	{
		//ComboBoxText combo = o as ComboboxText;
		if (o == null)
			return;

		//Update the combo with hidden jumpRj uniqueID, viewed and num, datetime
		createComboSelectJumpsRjFatigueNum (false);
	}
	// combo comboSelectJumpsRjFatigue (end)

	// combo comboSelectJumpsRjFatigueNum (start)
	private void createComboSelectJumpsRjFatigueNum (bool create)
	{
		if(create)
		{
			comboSelectJumpsRjFatigueNum = new CjComboGeneric (
					combo_select_jumps_rj_fatigue_num, hbox_combo_select_jumps_rj_fatigue_num);
			combo_select_jumps_rj_fatigue_num = comboSelectJumpsRjFatigueNum.Combo;
			combo_select_jumps_rj_fatigue_num.Changed += new EventHandler (
					on_combo_select_jumps_rj_fatigue_num_changed);
		} else {
			comboSelectJumpsRjFatigueNum.L_types = jumpsRjFatigueSelectJumpsOfType ();
			comboSelectJumpsRjFatigueNum.Fill();
			combo_select_jumps_rj_fatigue_num = comboSelectJumpsRjFatigueNum.Combo;
		}
		combo_select_jumps_rj_fatigue_num.Sensitive = true;
	}
	private void on_combo_select_jumps_rj_fatigue_num_changed (object o, EventArgs args)
	{
		//ComboBoxText combo = o as ComboboxText;
		if (o == null)
			return;

		jumpsRjFatigueCalculate ();
		drawingarea_jumps_rj_fatigue.QueueDraw ();
	}

	private void createCombo_combo_jumps_rj_fatigue_divide_in ()
	{
		combo_jumps_rj_fatigue_divide_in = UtilGtk.CreateComboBoxText (
				box_combo_jumps_rj_fatigue_divide_in,
				new List<string> { "2", "3", "4" },
				"");

		combo_jumps_rj_fatigue_divide_in.Changed += new EventHandler (on_combo_jumps_rj_fatigue_divide_in_changed);
	}

	// combo comboSelectJumpsRjFatigueNum (end)

	private void on_radio_jumps_rj_fatigue_heights_toggled (object o, EventArgs args)
	{
		if(! radio_jumps_rj_fatigue_heights.Active)
			return;

		jumpsRjFatigueCalculate ();
		drawingarea_jumps_rj_fatigue.QueueDraw ();
	}
	private void on_radio_jumps_rj_fatigue_tv_tc_toggled (object o, EventArgs args)
	{
		if(! radio_jumps_rj_fatigue_tv_tc.Active)
			return;

		jumpsRjFatigueCalculate ();
		drawingarea_jumps_rj_fatigue.QueueDraw ();
	}
	private void on_radio_jumps_rj_fatigue_rsi_toggled (object o, EventArgs args)
	{
		if(! radio_jumps_rj_fatigue_rsi.Active)
			return;

		jumpsRjFatigueCalculate ();
		drawingarea_jumps_rj_fatigue.QueueDraw ();
	}

	private void on_combo_jumps_rj_fatigue_divide_in_changed (object o, EventArgs args)
	{
		drawingarea_jumps_rj_fatigue.QueueDraw ();
	}

	private List<object> jumpsRjFatigueSelectJumpsOfType ()
	{
		List<object> types = new List<object>();
		if(currentPerson == null || currentSession == null ||
				comboSelectJumpsRjFatigue == null && comboSelectJumpsRjFatigue.Count == 0)
			return types;

		List <JumpRj> jrj_l = SqliteJumpRj.SelectJumps (false, currentSession.UniqueID, currentPerson.UniqueID,
				comboSelectJumpsRjFatigue.GetSelectedNameEnglish(), Sqlite.Orders_by.DEFAULT, 0, false);

		int count = 1;
		foreach(JumpRj jrj in jrj_l)
		{
			string name = string.Format("{0} {1}", count++,
					UtilDate.FromFile(jrj.Datetime).ToString());
			types.Add(new SelectTypes(jrj.UniqueID, name, name)); //is not translated
		}

		return types;
	}

	private void jumpsRjFatigueCalculate ()
	{
		if(currentPerson == null || currentSession == null ||
				drawingarea_jumps_rj_fatigue == null || drawingarea_jumps_rj_fatigue.Window == null || //it happens at start on click on analyze
				comboSelectJumpsRjFatigueNum.GetSelectedId() < 0)
			return;

		if(jumpsRjFatigue == null)
			jumpsRjFatigue = new JumpsRjFatigue();

		jumpsRjFatigue.MouseReset ();

		JumpsRjFatigue.Statistic statistic = JumpsRjFatigue.Statistic.HEIGHTS;
		if(radio_jumps_rj_fatigue_tv_tc.Active)
			statistic = JumpsRjFatigue.Statistic.Q;
		else if(radio_jumps_rj_fatigue_rsi.Active)
			statistic = JumpsRjFatigue.Statistic.RSI;

		jumpsRjFatigue.Calculate (comboSelectJumpsRjFatigueNum.GetSelectedId(), statistic);
	}

	//called just by QueueDraw
	private void jumpsRjFatiguePlot ()
	{
		if(currentPerson == null || currentSession == null ||
				drawingarea_jumps_rj_fatigue == null || drawingarea_jumps_rj_fatigue.Window == null || //it happens at start on click on analyze
				comboSelectJumpsRjFatigueNum.GetSelectedId() < 0)
		{
			if(drawingarea_jumps_rj_fatigue != null && drawingarea_jumps_rj_fatigue.Window != null)
				new JumpsRjFatigueGraph(drawingarea_jumps_rj_fatigue, "", preferences.fontType.ToString());

			button_jumps_rj_fatigue_save_image.Sensitive = false;
			return;
		}

		if(jumpsRjFatigue == null)
			jumpsRjFatigueCalculate ();

		string jumpType = comboSelectJumpsRjFatigue.GetSelectedNameEnglish();

		JumpsRjFatigue.Statistic statistic = JumpsRjFatigue.Statistic.HEIGHTS;
		if(radio_jumps_rj_fatigue_tv_tc.Active)
			statistic = JumpsRjFatigue.Statistic.Q;
		else if(radio_jumps_rj_fatigue_rsi.Active)
			statistic = JumpsRjFatigue.Statistic.RSI;

		if(jumpsRjFatigue.Point_l.Count == 0)
		{
			//constructor for showing blank screen with a message
			new JumpsRjFatigueGraph(drawingarea_jumps_rj_fatigue, jumpType, preferences.fontType.ToString());

			button_jumps_rj_fatigue_save_image.Sensitive = false;

		} else {
			string jumpDateStr = currentSession.DateShort;
			string [] strFull = (comboSelectJumpsRjFatigueNum.GetSelectedNameEnglish()).Split(new char[] {' '});
			if(strFull.Length == 3)
				jumpDateStr = strFull[1];

			int divideIn = 2;
			string divideInStr = UtilGtk.ComboGetActive(combo_jumps_rj_fatigue_divide_in);
			if(divideInStr == "2" || divideInStr == "3" || divideInStr == "4")
				divideIn = Convert.ToInt32(divideInStr);

			//regular constructor
			jumpsRjFatigueGraph = new JumpsRjFatigueGraph (
					jumpsRjFatigue.Tc_l,
					jumpsRjFatigue.Tv_l,
					jumpsRjFatigue.Point_l,
					jumpsRjFatigue.Slope,
					jumpsRjFatigue.Intercept,
					drawingarea_jumps_rj_fatigue,
					currentPerson.Name, jumpType,
					jumpDateStr,
					statistic,
					divideIn,
					jumpsRjFatigue.MouseX,
					jumpsRjFatigue.MouseY);
			jumpsRjFatigueGraph.Do(preferences.fontType.ToString());

			button_jumps_rj_fatigue_save_image.Sensitive = true;
		}
	}

	private void on_drawingarea_jumps_rj_fatigue_draw (object o, Gtk.DrawnArgs args)
	{
		//createComboSelectJumpsRjFatigueNum (false);
		jumpsRjFatiguePlot ();
		//data is calculated on switch page (at notebook_capture_analyze) or on change person
	}

	private void on_drawingarea_jumps_rj_fatigue_button_press_event (object o, ButtonPressEventArgs args)
	{
		//if there is no data and nothing to show, nothing to press, and also this is null
		if(jumpsRjFatigueGraph == null)
			return;

		LogB.Information("Button press done!");

		//redo the graph to delete previous rectangles of previous mouse clicks
		if (jumpsRjFatigue != null)
			jumpsRjFatigue.MouseSet (args.Event.X, args.Event.Y);

		drawingarea_jumps_rj_fatigue.QueueDraw ();
	}

	private void on_button_jumps_rj_fatigue_save_image_clicked (object o, EventArgs args)
	{
		checkFile(Constants.CheckFileOp.JUMPS_RJ_FATIGUE_SAVE_IMAGE);
	}

	private void on_button_jumps_rj_fatigue_save_image_selected (string destination)
	{
		if(drawingarea_jumps_rj_fatigue == null)
			return;

		LogB.Information("Saving");
		CairoUtil.GetScreenshotFromDrawingArea (drawingarea_jumps_rj_fatigue, destination);
	}
	private void on_overwrite_file_jumps_rj_fatigue_save_image_accepted (object o, EventArgs args)
	{
		on_button_jumps_rj_fatigue_save_image_selected (exportFileName);

		string myString = string.Format(Catalog.GetString("Saved to {0}"), exportFileName);
		new DialogMessage(Constants.MessageTypes.INFO, myString);
	}

	private void connectWidgetsJumpsRjFatigue (Gtk.Builder builder)
	{
		drawingarea_jumps_rj_fatigue = (Gtk.DrawingArea) builder.GetObject ("drawingarea_jumps_rj_fatigue");
		button_jumps_rj_fatigue_save_image = (Gtk.Button) builder.GetObject ("button_jumps_rj_fatigue_save_image");
		image_jumps_rj_fatigue_save = (Gtk.Image) builder.GetObject ("image_jumps_rj_fatigue_save");
		image_jumps_rj_fatigue_image_save = (Gtk.Image) builder.GetObject ("image_jumps_rj_fatigue_image_save");

		hbox_combo_select_jumps_rj_fatigue = (Gtk.HBox) builder.GetObject ("hbox_combo_select_jumps_rj_fatigue");
		hbox_combo_select_jumps_rj_fatigue_num = (Gtk.HBox) builder.GetObject ("hbox_combo_select_jumps_rj_fatigue_num");
		box_combo_jumps_rj_fatigue_divide_in = (Gtk.Box) builder.GetObject ("box_combo_jumps_rj_fatigue_divide_in");

		radio_jumps_rj_fatigue_heights = (Gtk.RadioButton) builder.GetObject ("radio_jumps_rj_fatigue_heights");
		radio_jumps_rj_fatigue_tv_tc = (Gtk.RadioButton) builder.GetObject ("radio_jumps_rj_fatigue_tv_tc");
		radio_jumps_rj_fatigue_rsi = (Gtk.RadioButton) builder.GetObject ("radio_jumps_rj_fatigue_rsi");
	}
}
