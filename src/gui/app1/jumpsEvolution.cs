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
 * Copyright (C) 2004-2019   Xavier de Blas <xaviblas@gmail.com> 
 */


using System;
using Gtk;
using Glade;
using Mono.Unix;

public partial class ChronoJumpWindow 
{
	[Widget] Gtk.DrawingArea drawingarea_jumps_evolution;
	[Widget] Gtk.Image image_tab_jumps_evolution;
	[Widget] Gtk.Image image_jumps_evolution_save;
	[Widget] Gtk.HBox hbox_combo_select_jumps_evolution;
	[Widget] Gtk.ComboBox combo_select_jumps_evolution;
	[Widget] Gtk.Button button_jumps_evolution_save_image;

	JumpsEvolution jumpsEvolution;
	CjComboSelectJumps comboSelectJumpsEvolution;

	// combo (start)
	private void createComboSelectJumpsEvolution(bool create)
	{
		if(create)
		{
			comboSelectJumpsEvolution = new CjComboSelectJumps(combo_select_jumps_evolution, hbox_combo_select_jumps_evolution, false);
			combo_select_jumps_evolution = comboSelectJumpsEvolution.Combo;
			combo_select_jumps_evolution.Changed += new EventHandler (on_combo_select_jumps_evolution_changed);
		} else {
			comboSelectJumpsEvolution.Fill();
			combo_select_jumps_evolution = comboSelectJumpsEvolution.Combo;
		}
		combo_select_jumps_evolution.Sensitive = true;
	}
	private void on_combo_select_jumps_evolution_changed(object o, EventArgs args)
	{
		ComboBox combo = o as ComboBox;
		if (o == null)
			return;

		jumpsEvolutionDo(true);
	}
	// combo (end)

	private void jumpsEvolutionDo (bool calculateData)
	{
		if(currentPerson == null || currentSession == null ||
				drawingarea_jumps_evolution == null || drawingarea_jumps_evolution.GdkWindow == null) //it happens at start on click on analyze
		{
			button_jumps_evolution_save_image.Sensitive = false;
			return;
		}

		if(jumpsEvolution == null) {
			jumpsEvolution = new JumpsEvolution();
			calculateData = true;
		}

		string jumpType = UtilGtk.ComboGetActive(combo_select_jumps_evolution);

		if(calculateData)
			jumpsEvolution.Calculate(currentPerson.UniqueID, jumpType);

		if(jumpsEvolution.Point_l.Count == 0)
		{
			//constructor for showing blank screen with a message
			new JumpsEvolutionGraph(drawingarea_jumps_evolution, jumpType);
					//currentPerson.Name, jumpType, currentSession.DateShort);

			button_jumps_evolution_save_image.Sensitive = false;

		} else {
			//regular constructor
			JumpsEvolutionGraph jeg = new JumpsEvolutionGraph(
					jumpsEvolution.Point_l,
					jumpsEvolution.Slope,
					jumpsEvolution.Intercept,
					drawingarea_jumps_evolution,
					currentPerson.Name, jumpType, currentSession.DateShort);
			jeg.Do();

			button_jumps_evolution_save_image.Sensitive = true;
		}
	}
	private void on_drawingarea_jumps_evolution_expose_event (object o, ExposeEventArgs args) 
	{
		jumpsEvolutionDo(false); //do not calculate data
		//data is calculated on switch page (at notebook_capture_analyze) or on change person
	}

	//TODO
	private void on_button_jumps_evolution_save_image_clicked (object o, EventArgs args)
	{
		checkFile(Constants.CheckFileOp.JUMPS_EVOLUTION_SAVE_IMAGE);
	}

	private void on_button_jumps_evolution_save_image_selected (string destination)
	{
		if(drawingarea_jumps_evolution == null)
			return;

		Gdk.Pixbuf pixbuf = Gdk.Pixbuf.FromDrawable(drawingarea_jumps_evolution.GdkWindow, Gdk.Colormap.System,
				0, 0, 0, 0,
				UtilGtk.WidgetWidth(drawingarea_jumps_evolution),
				UtilGtk.WidgetHeight(drawingarea_jumps_evolution) );

		LogB.Information("Saving");
		pixbuf.Save(destination,"png");
	}
	private void on_overwrite_file_jumps_evolution_save_image_accepted (object o, EventArgs args)
	{
		on_button_jumps_evolution_save_image_selected (exportFileName);

		string myString = string.Format(Catalog.GetString("Saved to {0}"), exportFileName);
		new DialogMessage(Constants.MessageTypes.INFO, myString);
	}

}
