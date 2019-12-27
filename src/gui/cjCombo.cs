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
 * Copyright (C) 2016-2017   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
using System.Collections.Generic; //List<T>
using Gtk;

public partial class ChronoJumpWindow
{
	int getExerciseIDFromName (string [] comboArrayString, string name, bool comboWithTranslation)
	{
		LogB.Information("getExerciseIDFromName for: " + name);

		string idFound = "";

		if(comboWithTranslation)
		{
			//first try translated
			idFound = Util.FindOnArray(':', 2, 0, name, comboArrayString);
			if(Util.IsNumber(idFound, false))
				return Convert.ToInt32(idFound);
		}

		//second try english (or if comboWithTranslation == false, then this is the only name
		idFound = Util.FindOnArray(':', 1, 0, name, comboArrayString);
		if(Util.IsNumber(idFound, false))
			return Convert.ToInt32(idFound);

		//third, send error value
		return -1;
	}


	int getExerciseIDFromAnyCombo(Gtk.ComboBox combo, string [] comboArrayString, bool comboWithTranslation)
	{
		return getExerciseIDFromName(comboArrayString, UtilGtk.ComboGetActive(combo), comboWithTranslation);
	}
}

public class CjCombo
{
	protected Gtk.ComboBox combo;
	protected Gtk.HBox hbox;

	protected List<object> l_types;


	protected void create() 
	{
		combo = ComboBox.NewText ();
	}

	protected virtual void select()
	{
	}

	//if we just need to update values, call only this method
	public void Fill()
	{
		select();

		string [] namesToCombo = new String [l_types.Count];
		int i =0;
		foreach(SelectTypes type in l_types)
			namesToCombo[i++] = type.NameTranslated;

		UtilGtk.ComboUpdate(combo, namesToCombo, "");
		combo.Active = 0;
	}
	//used when strings are not translatable, like port names
	public void FillNoTranslate()
	{
		select();

		string [] namesToCombo = new String [l_types.Count];
		int i =0;
		foreach(string type in l_types)
			namesToCombo[i++] = type;

		UtilGtk.ComboUpdate(combo, namesToCombo, "");
		combo.Active = 0;
	}
	
	protected void package() 
	{
		hbox.PackStart(combo, true, true, 0);
		hbox.ShowAll();
		combo.Sensitive = false;
	}
	
	public string GetSelectedNameEnglish()
	{
		string nameTranslatedSelected = UtilGtk.ComboGetActive(combo);
		foreach(SelectTypes type in l_types)
			if(type.NameTranslated == nameTranslatedSelected)
				return type.NameEnglish;

		return "";
	}
	
	public Gtk.ComboBox SelectById(int id)
	{
		int pos = 0;
		foreach(SelectTypes type in l_types) 
		{
			if(type.Id == id) 
			{
				combo.Active = pos;
				break;
			}

			pos ++;
		}

		return combo;
	}

	public string GetNameTranslated(string nameEnglish)
	{
		foreach(SelectTypes type in l_types)
			if(type.NameEnglish == nameEnglish)
				return type.NameTranslated;

		return "";
	}

	public void MakeActive(string nameEnglish)
	{
		foreach(SelectTypes type in l_types)
			if(type.NameTranslated == nameEnglish)
				combo.Active = UtilGtk.ComboMakeActive(combo, type.NameTranslated);
	}

	public Gtk.ComboBox DeleteValue(string nameTranslated)
	{
		UtilGtk.ComboDelThisValue(combo, nameTranslated);
		combo.Active = 0;
		return combo;
	}

	public Gtk.ComboBox Combo {
		get { return combo; }
	}
}


//------------ jumps -------------

public class CjComboSelectJumps : CjCombo
{
	private bool onlyFallingJumps;

	public CjComboSelectJumps(Gtk.ComboBox combo_select_jumps, Gtk.HBox hbox_combo_select_jumps, bool onlyFallingJumps)
	{
		this.combo = combo_select_jumps;
		this.hbox = hbox_combo_select_jumps;
		this.onlyFallingJumps = onlyFallingJumps;

		create();
		Fill();
		package();
	}

	protected override void select()
	{
		if(onlyFallingJumps)
			l_types = (List<object>) SqliteJumpType.SelectJumpTypesNew(false, "", "TC", false); //without alljumpsname, not startIn, not only name
		else
			l_types = (List<object>) SqliteJumpType.SelectJumpTypesNew(false, "", "", false); //without alljumpsname, without filter, not only name
	}
}

public class CjComboSelectJumpsRj : CjCombo
{
	public CjComboSelectJumpsRj(Gtk.ComboBox combo_select_jumps_rj, Gtk.HBox hbox_combo_select_jumps_rj)
	{
		this.combo = combo_select_jumps_rj;
		this.hbox = hbox_combo_select_jumps_rj;

		create();
		Fill();
		package();
	}

	protected override void select()
	{
		l_types = (List<object>) SqliteJumpType.SelectJumpRjTypesNew("", false); //without alljumpsname, not only name
	}
}

//------------ runs -------------

public class CjComboSelectRuns : CjCombo
{
	public CjComboSelectRuns(Gtk.ComboBox combo_select_runs, Gtk.HBox hbox_combo_select_runs) 
	{
		this.combo = combo_select_runs;
		this.hbox = hbox_combo_select_runs;

		create();
		Fill();
		package();
	}

	protected override void select()
	{
		l_types = (List<object>) SqliteRunType.SelectRunTypesNew("", false); //without allrunsname, not only name
	}
}

public class CjComboSelectRunsI : CjCombo
{
	public CjComboSelectRunsI(Gtk.ComboBox combo_select_runs_interval, Gtk.HBox hbox_combo_select_runs_interval)
	{
		this.combo = combo_select_runs_interval;
		this.hbox = hbox_combo_select_runs_interval;

		create();
		Fill();
		package();
	}

	protected override void select()
	{
		l_types = (List<object>) SqliteRunIntervalType.SelectRunIntervalTypesNew("", false); //without allrunsname, not only name
	}
}

public class CjComboForceSensorPorts : CjCombo
{
	public CjComboForceSensorPorts(Gtk.ComboBox combo_force_sensor_ports, Gtk.HBox hbox_combo_force_sensor_ports)
	{
		this.combo = combo_force_sensor_ports;
		this.hbox = hbox_combo_force_sensor_ports;

		create();
		FillNoTranslate();
		package();
	}

	protected override void select()
	{
		l_types = new List<object>();
		string [] strArray = ChronopicPorts.GetPorts();
		foreach(string str in strArray)
			l_types.Add(str);
	}
}
