/*
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA 02111-1307, USA.
 *
 * Copyright (C) 2025  Xavier de Blas <xaviblas@gmail.com>
 */

using System;
using System.Threading;
using Gtk;

public class RemotePersonNext
{
	private string file;
	private List<Gtk.Button> bNext_l;
	private List<string> p_l;
	private string importPerson = "";
	public Gtk.Button FakeButtonAdd;

	public RemotePersonNext (string file, List<Gtk.Button> bNext_l)
	{
		this.file = file;
		this.bNext_l = bNext_l;

		foreach (Gtk.Button b in bNext_l)
			b.Clicked += new EventHandler (on_button_clicked);

		p_l = new List<string> ();
		FakeButtonAdd = new Gtk.Button ();
	}

	public void ReadFile ()
	{
		importPerson = "";
		p_l = Util.ReadFileAsStringList (file, "");
	}

	public void AssignButtons ()
	{
		for (int i = 0; i < p_l.Count && i < bNext_l.Count; i ++)
			bNext_l[i].Label = p_l[i];
	}

	private void on_button_clicked (object o, EventArgs args)
	{
		Gtk.Button b = (Gtk.Button) o;

		LogB.Information (string.Format ("Pressed: {0}", b.Label));
		importPerson = b.Label;
		FakeButtonAdd.Click ();
	}

	public string ImportPerson {
		get { return importPerson; }
	}
}
