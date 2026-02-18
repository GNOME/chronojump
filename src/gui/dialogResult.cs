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
 * Copyright (C) 2025   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
using Gdk;
using Gtk;

public class DialogResult
{
	// at glade ---->
	Gtk.Dialog dialog_result;
	Gtk.Frame frame;
	Gtk.Label label_mode;
	Gtk.Label label_exercise;
	Gtk.Label label_person;
	Gtk.Label label_font_size;
	Gtk.Label label_result;
	Gtk.SpinButton spin_font_size;
	// <---- at glade

	public bool Visible;
	public Gtk.Button FakeButtonUpdateFontSize;
	public Gtk.Button FakeButtonClose;

	private int fontSizeAtGui;

	public DialogResult (int fontSizeAtGui, int width, int height)
	{
		this.fontSizeAtGui = fontSizeAtGui;

		initialize (width, height);
	}

	private void initialize (int width, int height)
	{
		Gtk.Builder builder = new Gtk.Builder (null, Util.GetGladePath () + "dialog_result.glade", null);
		connectWidgets (builder);
		builder.Autoconnect (this);

		//put an icon to window
		UtilGtk.IconWindow (dialog_result);

		label_result.Name = "externalWindow_monospace";

		//manage window color
		if(! Config.UseSystemColor)
		{
			UtilGtk.DialogColor(dialog_result, Config.ColorBackground);

			UtilGtk.ContrastLabelsLabel (Config.ColorBackgroundIsDark, label_mode);
			UtilGtk.ContrastLabelsLabel (Config.ColorBackgroundIsDark, label_exercise);
			UtilGtk.ContrastLabelsLabel (Config.ColorBackgroundIsDark, label_person);
			UtilGtk.ContrastLabelsLabel (Config.ColorBackgroundIsDark, label_font_size);

			UtilGtk.WidgetColor (frame, Config.ColorBackgroundShifted);
			UtilGtk.ContrastLabelsFrame (Config.ColorBackgroundShiftedIsDark, frame);
			UtilGtk.ContrastLabelsLabel (Config.ColorBackgroundShiftedIsDark, label_result);
		}
	
		//with this, user doesn't see a moving/changing creation window
		//if uncommented, then does weird bug in windows not showing dialog as its correct size until window is moves
		//dialog_result.Hide();	

		if (width > 0 && height > 0)
		{
			dialog_result.WidthRequest = width;
			dialog_result.HeightRequest = height;
		}

		spin_font_size.Value = Constants.FontSizeExternalWindow;
		on_spin_font_size_value_changed (new object (), new EventArgs ());
		Visible = true;
		FakeButtonUpdateFontSize = new Gtk.Button();
		FakeButtonClose = new Gtk.Button();

		dialog_result.Show();
	}

	public void SetLabels (string title, string modeName, string testName, Person person, string resultStr)
	{
		if(title != "")
			dialog_result.Title = title;

		label_mode.Text = modeName;
		label_exercise.Text = testName;
		if (person.ClubID != "")
			label_person.Text = string.Format ("{0}:{1}", person.ClubID, person.Name);
		else
			label_person.Text = string.Format ("{0}", person.Name);

		label_result.Text = resultStr;
	}

	public void on_spin_font_size_value_changed (object o, EventArgs args)
	{
		Constants.FontSizeExternalWindow = (int) spin_font_size.Value;

		if (FakeButtonUpdateFontSize != null)
			FakeButtonUpdateFontSize.Click (); //to be called by gtk thread
	}

	public void UpdateLabelResult (string str)
	{
		label_result.Text = str;
	}

	public void on_close_button_clicked (object obj, EventArgs args)
	{
		Visible = false;
		dialog_result.Visible = false;

		FakeButtonClose.Click ();
		//dialog_result.Destroy ();
	}

	private void on_delete_event (object o, DeleteEventArgs args)
	{
		Visible = false;
		dialog_result.Visible = false;

		FakeButtonClose.Click ();
		//dialog_result.Destroy ();

		args.RetVal = true;
	}

	private void connectWidgets (Gtk.Builder builder)
	{
		dialog_result = (Gtk.Dialog) builder.GetObject ("dialog_result");
		frame = (Gtk.Frame) builder.GetObject ("frame");
		label_mode = (Gtk.Label) builder.GetObject ("label_mode");
		label_exercise = (Gtk.Label) builder.GetObject ("label_exercise");
		label_person = (Gtk.Label) builder.GetObject ("label_person");
		label_font_size = (Gtk.Label) builder.GetObject ("label_font_size");
		label_result = (Gtk.Label) builder.GetObject ("label_result");
		spin_font_size = (Gtk.SpinButton) builder.GetObject ("spin_font_size");
	}
}

public partial class ChronoJumpWindow
{
	private void on_button_dialog_result_contacts_clicked (object o, EventArgs args)
	{
		button_dialog_result_contacts.Sensitive = false;
		button_dialog_result_contacts_4p.Sensitive = false;

		dialogResult = new DialogResult (preferences.fontSizeAtGui, 800, 600);
		dialog_result_set_labels ();

		dialogResult.FakeButtonUpdateFontSize.Clicked -= new EventHandler (on_button_dialog_result_update_font_size);
		dialogResult.FakeButtonUpdateFontSize.Clicked += new EventHandler (on_button_dialog_result_update_font_size);

		dialogResult.FakeButtonClose.Clicked -= new EventHandler (on_button_dialog_result_contacts_closed);
		dialogResult.FakeButtonClose.Clicked += new EventHandler (on_button_dialog_result_contacts_closed);
	}

	private void dialog_result_set_labels ()
	{
		// main label
		string resultStr = "";
		if (currentEventExecute != null && current_mode != Constants.Modes.OTHER) // currentEventExecute not for fourPlatforms
			resultStr = currentEventExecute.GetDialogResultString ();

		string exAndUnits = getCurrentTestTypeForThisMode ();
		if (current_mode == Constants.Modes.JUMPSSIMPLE)
			exAndUnits += " (cm)";
		else if (current_mode == Constants.Modes.RUNSSIMPLE)
			exAndUnits += " (m/s)";

		dialogResult.SetLabels ("Chronojump external results", Constants.ModePrint (current_mode),
				exAndUnits, currentPerson, resultStr);
	}

	private void on_button_dialog_result_update_font_size (object o, EventArgs args)
	{
		if (current_mode == Constants.Modes.OTHER)
		{
			if (capturingFourPlatforms == arduinoCaptureStatus.CAPTURING)
			{
				fourPlatformsNeedCallApplyCSSExternalWindow = true;
				return;
			}
		}
		else if (currentEventExecute != null && currentEventExecute.IsThreadRunning ()) // capturing: do it on GTK thread
		{
			currentEventExecute.NeedCallApplyCSSExternalWindow = true;
			return;
		}

		UtilGtk.ApplyCSSExternalWindow ();
	}

	private void on_button_dialog_result_contacts_closed (object o, EventArgs args)
	{
		button_dialog_result_contacts.Sensitive = true;
		button_dialog_result_contacts_4p.Sensitive = true;
	}

}
