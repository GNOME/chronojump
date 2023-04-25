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
 * Copyright (C) 2004-2023   Xavier de Blas <xaviblas@gmail.com>
 */

using System;
using Gtk;
using System.Collections.Generic;

public partial class ChronoJumpWindow
{
	// at glade ---->
	Gtk.Box box_presentation;
	Gtk.Box box_combo_presentation;
	Gtk.Button button_presentation_left;
	Gtk.Button button_presentation_right;
	Gtk.Image image_presentation_left;
	Gtk.Image image_presentation_right;
	// <---- at glade

	Gtk.ComboBoxText combo_presentation;
	PresentationSlideList presentationSL;

	private void connectWidgetsPresentation (Gtk.Builder builder)
	{
		box_presentation = (Gtk.Box) builder.GetObject ("box_presentation");
		box_combo_presentation = (Gtk.Box) builder.GetObject ("box_combo_presentation");
		button_presentation_left = (Gtk.Button) builder.GetObject ("button_presentation_left");
		button_presentation_right = (Gtk.Button) builder.GetObject ("button_presentation_right");
		image_presentation_left = (Gtk.Image) builder.GetObject ("image_presentation_left");
		image_presentation_right = (Gtk.Image) builder.GetObject ("image_presentation_right");
	}

	private void presentationPrepare ()
	{
		presentationSL = new PresentationSlideList ();
		if (! presentationSL.Read ())
			return;

		combo_presentation = UtilGtk.CreateComboBoxText (
				box_combo_presentation,
				presentationSL.GetPrintingStrings (),
				presentationSL.GetPrintingStrings ()[0]);
		combo_presentation.Changed += new EventHandler (on_combo_presentation_changed);

		button_presentation_left.Sensitive = false;
		box_presentation.Visible = true;
	}

	private void on_combo_presentation_changed (object o, EventArgs args)
	{
		button_presentation_left.Sensitive = (combo_presentation.Active > 0);
		button_presentation_right.Sensitive = ! UtilGtk.ComboSelectedIsLast (combo_presentation);

		processPresentationActionsIfNeeded (combo_presentation.Active);
	}

	private void on_button_presentation_left_clicked (object o, EventArgs args)
	{
		bool isFirst;
		combo_presentation = UtilGtk.ComboSelectPrevious (combo_presentation, out isFirst);

		button_presentation_left.Sensitive = ! isFirst;
		button_presentation_right.Sensitive = true;
	}

	private void on_button_presentation_right_clicked (object o, EventArgs args)
	{
		bool isLast;
		combo_presentation = UtilGtk.ComboSelectNext (combo_presentation, out isLast);

		button_presentation_left.Sensitive = true;
		button_presentation_right.Sensitive = ! isLast;
	}

	public void processPresentationActionsIfNeeded (int activePos)
	{
		List<PresentationAction> pa_l = presentationSL.GetActionsOfThisSlide (activePos);
		if (pa_l.Count == 0)
			return;

		foreach (PresentationAction pa in pa_l)
		{
			if (pa.ae == PresentationAction.ActionEnum.Mode && pa.parameter != "")
			{
				changeModeCheckRadios ((Constants.Modes) Enum.Parse (typeof (Constants.Modes), pa.parameter));
			}
			else if (pa.ae == PresentationAction.ActionEnum.LoadSessionByName && pa.parameter != "")
			{
				//do not do using guiTests because threads can cause that possible posterior SelectPersonByName happens when treeview_persons is still not updated
				//chronojumpWindowTestsLoadSessionByName (pa.parameter);

				currentSession = SqliteSession.SelectByName (pa.parameter);
				on_load_session_accepted();
				sensitiveGuiYesSession();
			}
			else if (pa.ae == PresentationAction.ActionEnum.SelectPersonByName && pa.parameter != "")
				chronojumpWindowTestsSelectPersonByName (pa.parameter);
		}
	}
}

//TODO: move this class to src/presentation.cs
public class PresentationSlideList
{
	List<PresentationSlide> list;

	public PresentationSlideList ()
	{
		list = new List<PresentationSlide> ();
	}

	public bool Read()
	{
		List<string> contents_l = Util.ReadFileAsStringList (Util.GetPresentationFileName());
		if (contents_l == null || contents_l.Count == 0)
			return false;

		int countItems = 0;
		int countSubitems = 0;
		foreach (string line in contents_l)
		{
			if (line == null)
				break;
			if (line == "" || line[0] == '#')
				continue;

			string [] parts = line.Split (new string[] {":::"}, StringSplitOptions.None);

			PresentationSlide ps = new PresentationSlide ();
			if (parts.Length > 1)
				for (int i = 1; i < parts.Length; i ++)
				{
					PresentationAction pa = new PresentationAction ();
					if (pa.Assign (parts[i]))
					{
						ps.AddAction (pa);
						LogB.Information ("Added action: " + pa.ToString ());
					} else
						ps.AddError ();
				}

			if (parts[0].StartsWith ("  "))
				countSubitems ++;
			else {
				countSubitems = 0;
				countItems ++;
			}

			ps.AddText (parts[0], countItems, countSubitems);

			list.Add (ps);
		}
		return true;
	}

	public List<string> GetPrintingStrings ()
	{
		List<string> string_l = new List<string> ();
		foreach (PresentationSlide ps in list)
			string_l.Add (ps.text);

		return string_l;
	}

	public List<PresentationAction> GetActionsOfThisSlide (int activePos)
	{
		List<PresentationAction> pa_l = new List<PresentationAction> ();

		PresentationSlide ps = list [activePos];
		if (ps.HasActions ())
			pa_l = ps.GetActions ();

		return pa_l;
	}
}

//TODO: move this class to src/presentation.cs
/*
Example of chronojump_presentation.txt
#Exemple de presentació, # i línies en blanc no es processen
#Si es vol posar accions, cal separar cada lína amb :::
#Cada acció tindrà dos parts separades per :
#Les accions disponibles ara mateix són: Mode, LoadSessionByName, SelectPersonByName
#Els Modes són: JUMPSSIMPLE, JUMPSREACTIVE, RUNSSIMPLE, RUNSINTERVALLIC, RUNSENCODER, POWERGRAVITATORY, POWERINERTIAL, FORCESENSORISOMETRIC, FORCESENSORELASTIC


Aquest és el primer punt
i aquest el segon
Un tercer punt amb 2 accions:::LoadSessionByName:Galga+Trigger:::Mode:FORCESENSORISOMETRIC
Carrega Tutorial i William:::LoadSessionByName:Tutorial:::SelectPersonByName:William
Aquest seria el 5è
  Seria el 5.1
  Seria el 5.2
Seria el sisè amb el Carmelo:::SelectPersonByName:Carmelo:::Mode:POWERGRAVITATORY
  Té un subpunt
Últim punt
 */
public class PresentationSlide
{
	public string text;
	public List<PresentationAction> action_l;
	private bool errorInSomeAction; //note at Read time we cannot find here if the errors is in the parameter (like person does not exists in session)

	public PresentationSlide ()
	{
		action_l = new List<PresentationAction> ();
		errorInSomeAction = false;
	}

	public void AddAction (PresentationAction pa)
	{
		action_l.Add (pa);
	}

	public bool HasActions ()
	{
		return (action_l != null && action_l.Count > 0);
	}

	public List<PresentationAction> GetActions ()
	{
		return action_l;
	}

	//adds also enumeration and maybe action mark
	public void AddText (string text, int countItems, int countSubitems)
	{
		string actionsStr = "";
		if (HasActions ())
			actionsStr = " (*)";

		string actionErrorStr = "";
		if (errorInSomeAction)
		{
			LogB.Information ("hasErrorsInActions");
			actionErrorStr = " (!)";
		}

		if (countSubitems == 0)
			this.text = string.Format ("{0}. {1}{2}{3}", countItems, text, actionsStr, actionErrorStr);
		else
			this.text = string.Format ("  {0}.{1}. {2}{3}{4}",
					countItems, countSubitems, text.Substring(2), //removes 2 first chars
					actionsStr, actionErrorStr);
	}

	public void AddError ()
	{
		errorInSomeAction = true;
	}

	//debug
	public override string ToString ()
	{
		string str = "Text: " + text;
		if (action_l.Count > 0)
			foreach (PresentationAction pa in action_l)
				str += "\n" + pa.ToString ();

		return str;
	}
}

//TODO: move this class to src/presentation.cs
public class PresentationAction
{
	public enum ActionEnum { LoadSessionByName, SelectPersonByName, Mode };

	public ActionEnum ae;
	public string parameter;

	public PresentationAction ()
	{
	}

	public bool Assign (string str)
	{
		string [] parts = str.Split (new char[] {':'});
		if (parts.Length != 2)
			return false;

		if (! actionExists (parts[0]))
			return false;

		ae = (ActionEnum) Enum.Parse (typeof (ActionEnum), parts[0]);
		parameter = parts[1];

		if (ae == ActionEnum.Mode && ! modeExists (parameter))
			return false;

		return true;
	}

	private bool actionExists (string str)
	{
		string [] enums = Enum.GetNames(typeof(ActionEnum));
		foreach (string e in enums)
			if (str == e)
				return true;

		return false;
	}

	private bool modeExists (string str)
	{
		if (str == Constants.Modes.UNDEFINED.ToString ())
			return false;

		string [] enums = Enum.GetNames(typeof(Constants.Modes));
		foreach (string e in enums)
			if (str == e)
				return true;

		return false;
	}

	//debug
	public override string ToString ()
	{
		return string.Format ("Action: {0}, Parameter: {1}", ae, parameter);
	}
}

/* Old webkit code used on my thesis presentation

//using System.IO;
//using System.Threading;
//using Mono.Unix;
//using Gdk;
//using Glade;
//using System.Collections;
//using WebKit;

public partial class ChronoJumpWindow 
{
	//presentation
	[Widget] Gtk.Box vbox_presentation;
	[Widget] Gtk.Image image_presentation_logo;
	[Widget] Gtk.Label label_presentation_current;

	//static WebKit.WebView presentation;

	bool presentationInitialized = false;

	void on_menuitem_presentation_activate (object o, EventArgs args) {
		//vbox_presentation.Visible = ! vbox_presentation.Visible;
	}

 
	private void presentationInit() {
		//button_presentation_restore_screen.Sensitive = false;

		 * needs webKit
		 *
		presentation = new WebKit.WebView();
		scrolledwindow_presentation.Add(presentation);
		
		loadInitialPresentation();

		presentation.ShowAll();
	
		presentationInitialized = true;
	}
	
	void on_button_presentation_screen_clicked (object o, EventArgs args) {
		Gtk.Button button = (Gtk.Button) o;

		vbox_persons.Visible =	( button != button_presentation_fullscreen);
		notebook_sup.ShowTabs =	( button != button_presentation_fullscreen);
		//button_presentation_fullscreen.Sensitive =	( button != button_presentation_fullscreen);
		//button_presentation_restore_screen.Sensitive =	( button == button_presentation_fullscreen);
	}

	void on_button_presentation_reload_clicked (object o, EventArgs args) 
	{
		if(! presentationInitialized)
			presentationInit();
		else
			loadInitialPresentation();
	}

	//TODO: in the future read the divs on the HTML
	int presentation_slide_current = 0;
	int presentation_slide_max = 10;

	void on_button_presentation_previous_clicked (object o, EventArgs args)
	{
		if(! presentationInitialized)
			presentationInit();
		else {
			presentation_slide_current --;
			presentationSlideChange();
		}
	}
	void on_button_presentation_next_clicked (object o, EventArgs args)
	{
		if(! presentationInitialized)
			presentationInit();
		else {
			presentation_slide_current ++;
			presentationSlideChange();
		}
	}
	void presentationSlideChange() 
	{
		if (presentation_slide_current < 0)
			presentation_slide_current = 0;
		else if (presentation_slide_current > presentation_slide_max)
			presentation_slide_current = presentation_slide_max;

		openPresentation();
		updatePresentationLabel();
	}

	void openPresentation() {
		//presentationOpenStatic("file:///home/...html#" + presentation_slide_current.ToString());
		string file = Path.Combine (Util.GetLocalDataDir (false) +
				Path.DirectorySeparatorChar + "Chronojump-Boscosystem.html");
		if(File.Exists(file))
			presentationOpenStatic("file://" + file + "#" + presentation_slide_current.ToString());
	}

	void updatePresentationLabel() {
		label_presentation_current.Text = 
			(presentation_slide_current +1).ToString() + " / " + 
			(presentation_slide_max +1).ToString();
	}
	
	private void loadInitialPresentation()
	{
		LogB.Information("Loading");
		
		presentation_slide_current = 0;
		openPresentation();
		updatePresentationLabel();
		
		LogB.Information("Loaded");
	}

	private static void presentationOpenStatic(string url) {
		 * needs WebKit
		 *
		presentation.Open(url);
	}
}
*/
