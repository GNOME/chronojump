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
 * Copyright (C) 2016   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
using System.Collections.Generic; //List<T>
using Gtk;

public class CjCombo
{
	protected Gtk.ComboBox combo;
	protected Gtk.HBox hbox;

	protected void create() 
	{
		combo = ComboBox.NewText ();
	}
	
	public virtual void Fill()
	{
	}
	
	protected void package() 
	{
		hbox.PackStart(combo, true, true, 0);
		hbox.ShowAll();
		combo.Sensitive = false;
	}
	
	public virtual string GetSelectedNameEnglish()
	{
		return "";
	}
	
	public virtual Gtk.ComboBox SelectById(int id) 
	{
		return combo;
	}
	
	public virtual string GetNameTranslated(string nameEnglish)
	{
		return "";
	}
	
	public virtual void MakeActive(string nameEnglish)
	{
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

public class CjComboSelectJumps : CjCombo
{
	List<SelectJumpTypes> jumpTypes;

	public CjComboSelectJumps(Gtk.ComboBox combo_select_jumps, Gtk.HBox hbox_combo_select_jumps) 
	{
		this.combo = combo_select_jumps;
		this.hbox = hbox_combo_select_jumps;

		create();
		
		Fill();
	
		package();	
	}

	//if we just need to update values, call only this method
	public override void Fill()
	{
		jumpTypes = SqliteJumpType.SelectJumpTypesNew(false, "", "", false); //without alljumpsname, without filter, not only name

		string [] jumpNamesToCombo = new String [jumpTypes.Count];
		int i =0;
		foreach(SelectJumpTypes jumpType in jumpTypes)
			jumpNamesToCombo[i++] = jumpType.NameTranslated;
		
		UtilGtk.ComboUpdate(combo, jumpNamesToCombo, "");
		combo.Active = 0;
	}

	//TODO: refactor this. Move it to parent class	
	public override string GetSelectedNameEnglish()
	{
		string nameTranslatedSelected = UtilGtk.ComboGetActive(combo);
		foreach(SelectJumpTypes jumpType in jumpTypes)
			if(jumpType.NameTranslated == nameTranslatedSelected)
				return jumpType.NameEnglish;

		return "";
	}

	//TODO: refactor this. Move it to parent class	
	public Gtk.ComboBox SelectById(int id)
	{
		int pos = 0;
		foreach(SelectJumpTypes jumpType in jumpTypes) 
		{
			if(jumpType.Id == id) 
			{
				combo.Active = pos;
				break;
			}

			pos ++;
		}

		return combo;
	}

	//TODO: refactor this. Move it to parent class	
	public override string GetNameTranslated(string nameEnglish)
	{
		foreach(SelectJumpTypes jumpType in jumpTypes)
			if(jumpType.NameEnglish == nameEnglish)
				return jumpType.NameTranslated;

		return "";
	}

	//TODO: refactor this. Move it to parent class	
	public override void MakeActive(string nameEnglish)
	{
		foreach(SelectJumpTypes jumpType in jumpTypes)
			if(jumpType.NameTranslated == nameEnglish)
				combo.Active = UtilGtk.ComboMakeActive(combo, jumpType.NameTranslated);
	}
}
