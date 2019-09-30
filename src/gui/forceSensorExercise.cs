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
 * Copyright (C) 2019   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
//using Gdk;
using Gtk;
using Glade;
//using Gnome;
using GLib; //for Value
using System.Collections.Generic; //List<T>
//using Mono.Unix;


public class ForceSensorExerciseWindow
{
	[Widget] Gtk.Window force_sensor_exercise;
	[Widget] Gtk.Label label_header;
	/*
	   [Widget] Gtk.Box hbox_error;
	   [Widget] Gtk.Label label_error;
	   */
	[Widget] Gtk.Entry entry_name;

	//each of the rows of the table
	[Widget] Gtk.Label label_force;
	[Widget] Gtk.RadioButton radio_force_sensor;
	[Widget] Gtk.RadioButton radio_force_resultant;	
	[Widget] Gtk.Button button_help_force;

	[Widget] Gtk.Label label_string;
	[Widget] Gtk.RadioButton radio_string_rope;
	[Widget] Gtk.RadioButton radio_string_rubber_band;
	[Widget] Gtk.Button button_help_string;

	[Widget] Gtk.Label label_cdg_displ;
	[Widget] Gtk.RadioButton radio_cdg_displ_yes;
	[Widget] Gtk.RadioButton radio_cdg_displ_no;
	[Widget] Gtk.Button button_help_cdg_displ;

	[Widget] Gtk.Label label_sensor_affected_bw;
	[Widget] Gtk.RadioButton radio_sensor_affected_bw_yes;
	[Widget] Gtk.RadioButton radio_sensor_affected_bw_no;
	[Widget] Gtk.Button button_help_sensor_affected_bw;

	[Widget] Gtk.Label label_tare_before_capture;
	[Widget] Gtk.RadioButton radio_tare_before_capture_yes;
	[Widget] Gtk.RadioButton radio_tare_before_capture_no;
	[Widget] Gtk.Button button_help_tare_before_capture;

	[Widget] Gtk.Label label_bw_added;
	[Widget] Gtk.HBox hbox_bw_added;
	[Widget] Gtk.SpinButton spin_bw_added;
	[Widget] Gtk.Button button_help_bw_added;

	[Widget] Gtk.Label label_angle;
	[Widget] Gtk.HBox hbox_angle;
	[Widget] Gtk.SpinButton spin_angle;
	[Widget] Gtk.Button button_help_angle;

	//lists of widgets to play with visibility
	List<Widget> lw_string;
	List<Widget> lw_cdg_displ;
	List<Widget> lw_sensor_affected_bw;
	List<Widget> lw_tare_before_capture;
	List<Widget> lw_bw_added;
	List<Widget> lw_angle;

	//	[Widget] Gtk.Image image_delete;

	static ForceSensorExerciseWindow ForceSensorExerciseWindowBox;

	/*
	   public int uniqueID; 			//used on encoder & forceSensor edit exercise
	   public string nameUntranslated;		//used on encoder edit exercise
	   */

	public ForceSensorExerciseWindow (string title, string textHeader)
	{
		Glade.XML gladeXML;
		gladeXML = Glade.XML.FromAssembly (Util.GetGladePath() + "force_sensor_exercise.glade", "force_sensor_exercise", "chronojump");
		gladeXML.Autoconnect(this);

		//put an icon to window
		UtilGtk.IconWindow(force_sensor_exercise);

		force_sensor_exercise.Resizable = false;
		setTitle(title);
		label_header.Text = textHeader;

		createListsOfWidgets();

		//HideOnAccept = true;
		//DestroyOnAccept = false;
	}

	static public ForceSensorExerciseWindow Show (string title, string textHeader)
	{
		if (ForceSensorExerciseWindowBox == null) {
			ForceSensorExerciseWindowBox = new ForceSensorExerciseWindow(title, textHeader);
		} else {
			ForceSensorExerciseWindowBox.setTitle(title);
			ForceSensorExerciseWindowBox.label_header.Text = textHeader;
		}

		ForceSensorExerciseWindowBox.hideWidgets();
		ForceSensorExerciseWindowBox.force_sensor_exercise.Show ();

		return ForceSensorExerciseWindowBox;
	}

	private void setTitle(string title)
	{
		if(title != "")
			force_sensor_exercise.Title = "Chronojump - " + title;
	}

	private void createListsOfWidgets()
	{
		lw_string = new List<Widget> {
			label_string, radio_string_rope, radio_string_rubber_band, button_help_string };
		lw_cdg_displ = new List<Widget> {
			label_cdg_displ, radio_cdg_displ_yes, radio_cdg_displ_no, button_help_cdg_displ };
		lw_sensor_affected_bw = new List<Widget> {
			label_sensor_affected_bw, radio_sensor_affected_bw_yes, radio_sensor_affected_bw_no, button_help_sensor_affected_bw };
		lw_tare_before_capture = new List<Widget> {
			label_tare_before_capture, radio_tare_before_capture_yes, radio_tare_before_capture_no, button_help_tare_before_capture };
		lw_bw_added = new List<Widget> {
			label_bw_added, hbox_bw_added, button_help_bw_added }; 
		lw_angle = new List<Widget> {
			label_angle, hbox_angle, button_help_angle }; 
	}

	private void hideWidgets()
	{
		showHideWidget(lw_string, false);
		showHideWidget(lw_cdg_displ, false);
		showHideWidget(lw_sensor_affected_bw, false);
		showHideWidget(lw_tare_before_capture, false);
		showHideWidget(lw_bw_added, false);
		showHideWidget(lw_angle, false);
	}

	private void showHideWidget(List<Widget> lw, bool show)
	{
		foreach(Widget w in lw)
			w.Visible = show;

		if(lw == lw_string && show)
			showHideWidget(lw_cdg_displ, radio_string_rubber_band.Active);
	}

	private void on_radio_force_toggled (object o, EventArgs args)
	{
		if (radio_force_resultant.Active)
			showHideWidget(lw_string, true);
		else {
			showHideWidget(lw_string, false);
			showHideWidget(lw_cdg_displ, false);
			showHideWidget(lw_sensor_affected_bw, false);
			showHideWidget(lw_tare_before_capture, false);
			showHideWidget(lw_bw_added, false);
			showHideWidget(lw_angle, false);
		}
	}

	private void on_radio_string_toggled (object o, EventArgs args)
	{
		if(radio_string_rubber_band.Active)
			showHideWidget(lw_cdg_displ, true);
		else {
			showHideWidget(lw_cdg_displ, false);
			showHideWidget(lw_sensor_affected_bw, false);
			showHideWidget(lw_tare_before_capture, false);
			showHideWidget(lw_bw_added, false);
			showHideWidget(lw_angle, false);
		}
	}

	private void on_entries_changed (object o, EventArgs args)
	{
		Gtk.Entry entry_name = o as Gtk.Entry;
		if (o == null)
			return;

		entry_name.Text = Util.MakeValidSQL(entry_name.Text);
	}

	private void on_delete_event (object o, DeleteEventArgs args)
	{
		LogB.Information("calling on_delete_event");

		args.RetVal = true;

		ForceSensorExerciseWindowBox.force_sensor_exercise.Hide();
		ForceSensorExerciseWindowBox = null;
	}


	~ForceSensorExerciseWindow() {}
}

