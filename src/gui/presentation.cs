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
using System.IO; 		//File
using Gtk;
using System.Collections.Generic;

public partial class ChronoJumpWindow
{
	// at glade ---->
	Gtk.Box box_presentation;
	Gtk.Viewport viewport_presentation;
	Gtk.Box box_combo_presentation;
	Gtk.Button button_presentation_left;
	Gtk.Button button_presentation_right;
	Gtk.Image image_presentation_left;
	Gtk.Image image_presentation_right;
	Gtk.Label label_presentation_subtitle;
	// <---- at glade

	Gtk.ComboBoxText combo_presentation;
	PresentationSlideList presentationSL;

	private void presentationPrepare ()
	{
		presentationSL = new PresentationSlideList ();
		if (! presentationSL.Read () || presentationSL.Count () == 0)
			return;

		label_presentation_subtitle.Visible = presentationSL.HasSubtitles;

		combo_presentation = UtilGtk.CreateComboBoxText (
				box_combo_presentation,
				presentationSL.GetPrintingStrings (),
				presentationSL.GetPrintingStrings ()[0]);
		combo_presentation.Changed += new EventHandler (on_combo_presentation_changed);

		processPresentationActionsIfNeeded (combo_presentation.Active); //to process an action on 1st slide
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
		label_presentation_subtitle.Text = "";

		List<PresentationAction> pa_l = presentationSL.GetActionsOfThisSlide (activePos);
		if (pa_l.Count == 0)
			return;

		foreach (PresentationAction pa in pa_l)
		{
			if (pa.Ae == PresentationAction.ActionEnum.Sub && pa.Parameter != "")
				label_presentation_subtitle.Text = pa.Parameter;
			else if (pa.Ae == PresentationAction.ActionEnum.Mode && pa.Parameter != "")
				changeModeCheckRadios ((Constants.Modes) Enum.Parse (typeof (Constants.Modes), pa.Parameter));
			else if (pa.Ae == PresentationAction.ActionEnum.LoadSessionByName && pa.Parameter != "")
			{
				//do not do using guiTests because threads can cause that possible posterior SelectPersonByName happens when treeview_persons is still not updated
				//chronojumpWindowTestsLoadSessionByName (pa.Parameter);

				currentSession = SqliteSession.SelectByName (pa.Parameter);
				on_load_session_accepted();
				sensitiveGuiYesSession();
			}
			else if (pa.Ae == PresentationAction.ActionEnum.SelectPersonByName && pa.Parameter != "")
				chronojumpWindowTestsSelectPersonByName (pa.Parameter);
			else if (pa.Ae == PresentationAction.ActionEnum.LoadImage && pa.Parameter != "")
				new DialogImageTest ("", pa.Parameter, DialogImageTest.ArchiveType.FILE, "", 0, 0);
			else if (pa.Ae == PresentationAction.ActionEnum.Color && pa.Parameter != "")
				UtilGtk.ViewportColor (viewport_presentation, UtilGtk.ColorParse (pa.Parameter));
		}
	}

	private void connectWidgetsPresentation (Gtk.Builder builder)
	{
		box_presentation = (Gtk.Box) builder.GetObject ("box_presentation");
		viewport_presentation = (Gtk.Viewport) builder.GetObject ("viewport_presentation");
		box_combo_presentation = (Gtk.Box) builder.GetObject ("box_combo_presentation");
		button_presentation_left = (Gtk.Button) builder.GetObject ("button_presentation_left");
		button_presentation_right = (Gtk.Button) builder.GetObject ("button_presentation_right");
		image_presentation_left = (Gtk.Image) builder.GetObject ("image_presentation_left");
		image_presentation_right = (Gtk.Image) builder.GetObject ("image_presentation_right");
		label_presentation_subtitle = (Gtk.Label) builder.GetObject ("label_presentation_subtitle");
	}
}

//TODO: move this class to src/presentation.cs
public class PresentationSlideList
{
	private List<PresentationSlide> list;
	private bool hasSubtitles; //if any slide has subtitles

	public PresentationSlideList ()
	{
		list = new List<PresentationSlide> ();
		hasSubtitles = false;
	}

	public bool Read()
	{
		List<string> contents_l = Util.ReadFileAsStringList (Util.GetPresentationFileName());
		if (contents_l == null || contents_l.Count == 0)
			return false;

		// 1) find if it has to redirect to another file using:
		// file:filename
		foreach (string line in contents_l)
		{
			if (line == null)
				break;
			if (line == "" || line[0] == '#')
				continue;

			string [] parts = line.Split (new char[] {':'});
			if (parts.Length == 2 && parts[0].ToLower () == "file")
			{
				//open that file if parts[1] has the full path
				if (File.Exists (parts[1]))
					contents_l = Util.ReadFileAsStringList (parts[1]);
				//open that file if parts[1] is just the filename (getting the url of datadir
				else if (File.Exists (Util.GetPresentationFileName(parts[1])))
					contents_l = Util.ReadFileAsStringList (Util.GetPresentationFileName(parts[1]));
			}
		}

		// 2) read the presentation file
		int countItems = 0;
		int countSubitems = 0;
		int countSubSubitems = 0;
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

						if (pa.ActionIsSub ())
						{
							LogB.Information ("Action is sub");
							hasSubtitles = true;
						}
					} else
						ps.AddError ();
				}

			if (parts[0].StartsWith ("    "))
				countSubSubitems ++;
			else if (parts[0].StartsWith ("  "))
			{
				countSubSubitems = 0;
				countSubitems ++;
			} else {
				countSubSubitems = 0;
				countSubitems = 0;
				countItems ++;
			}

			ps.AddText (parts[0], countItems, countSubitems, countSubSubitems);

			list.Add (ps);
		}
		return true;
	}

	public List<string> GetPrintingStrings ()
	{
		List<string> string_l = new List<string> ();
		foreach (PresentationSlide ps in list)
			string_l.Add (ps.Text);

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

	public int Count ()
	{
		return list.Count;
	}

	public bool HasSubtitles {
		get { return hasSubtitles; }
	}
}

//TODO: move this class to src/presentation.cs

/*
presentation.txt can have this syntax
#File:presentation_all.txt
File:presentation_force.txt
*/

/*
Example of presentation.txt or the file that is linked above
#Exemple de presentació, # i línies en blanc no es processen
#Si es vol posar accions, cal separar cada lína amb :::
#Cada acció tindrà dos parts separades per :
#Les accions disponibles ara mateix són: Mode, LoadSessionByName, SelectPersonByName, LoadImage
#Els Modes són: JUMPSSIMPLE, JUMPSREACTIVE, RUNSSIMPLE, RUNSINTERVALLIC, RUNSENCODER, POWERGRAVITATORY, POWERINERTIAL, FORCESENSORISOMETRIC, FORCESENSORELASTIC


Aquest és el primer punt
i aquest el segon que té subtítol:::Sub:El meu subtítol explicatiu.
Un tercer punt amb 2 accions:::LoadSessionByName:Galga+Trigger:::Mode:FORCESENSORISOMETRIC
Carrega Tutorial i William:::LoadSessionByName:Tutorial:::SelectPersonByName:William
Aquest seria el 5è
  Seria el 5.1 que fa loadImage:::LoadImage:/home/xavier/Imatges/tarde.png
  Seria el 5.2:::Sub:Subtítol de la 5.2
    Seria el 5.2.1
Seria el sisè amb el Carmelo:::SelectPersonByName:Carmelo:::Mode:POWERGRAVITATORY
  Té un subpunt
Últim punt
 */
public class PresentationSlide
{
	private string text;
	private List<PresentationAction> action_l;
	private string subtitle;
	private bool errorInSomeAction; //note at Read time we cannot find here if the errors is in the parameter (like person does not exists in session)

	public PresentationSlide ()
	{
		action_l = new List<PresentationAction> ();

		subtitle = "";
		errorInSomeAction = false;
	}

	public void AddAction (PresentationAction pa)
	{
		action_l.Add (pa);
	}

	//to process diferent actions (including subtitle)
	public bool HasActions ()
	{
		return (action_l != null && action_l.Count > 0);
	}

	//to print (*) if there is any action (subtitle does not count)
	public bool HasActionsThatNeedsAsterisk ()
	{
		if (! HasActions ())
			return false;

		foreach (PresentationAction pa in action_l)
			if (pa.Ae != PresentationAction.ActionEnum.Sub && pa.Ae != PresentationAction.ActionEnum.Color)
				return true;

		return false;
	}

	public List<PresentationAction> GetActions ()
	{
		return action_l;
	}

	//adds also enumeration and maybe action mark
	public void AddText (string text, int countItems, int countSubitems, int countSubSubitems)
	{
		string actionsStr = "";
		if (HasActionsThatNeedsAsterisk ())
			actionsStr = " (*)";

		string actionErrorStr = "";
		if (errorInSomeAction)
		{
			LogB.Information ("hasErrorsInActions");
			actionErrorStr = " (!)";
		}

		if (countSubitems == 0)
			this.text = string.Format ("{0}. {1}{2}{3}", countItems, text, actionsStr, actionErrorStr);
		else if (countSubSubitems == 0)
			this.text = string.Format ("    {0}.{1}. {2}{3}{4}",
					countItems, countSubitems, text.Substring(2), //removes 2 first chars
					actionsStr, actionErrorStr);
		else
			this.text = string.Format ("        {0}.{1}.{2}. {3}{4}{5}",
					countItems, countSubitems, countSubSubitems, text.Substring(4), //removes 4 first chars
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

	public string Text
	{
		get { return text; }
	}

	public string Subtitle
	{
		set { subtitle = value; }
		get { return subtitle; }
	}
}

//TODO: move this class to src/presentation.cs
public class PresentationAction
{
	//note Sub and Color are special actions because they just displays the subtitle/change color, but it will not show the (*)
	public enum ActionEnum { Sub, LoadSessionByName, SelectPersonByName, Mode, LoadImage, Color };

	private ActionEnum ae;
	private string parameter;

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

	public bool ActionIsSub ()
	{
		return (ae == ActionEnum.Sub);
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

	public ActionEnum Ae
	{
		get { return ae; }
	}
	public string Parameter
	{
		get { return parameter; }
	}
}
