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
 * Xavier de Blas: 
 * http://www.xdeblas.com, http://www.deporteyciencia.com (parleblas)
 */

using System;
using Gtk;
using Glade;
using System.Text; //StringBuilder
using System.Collections; //ArrayList

using System.Threading;
using Mono.Unix;


//--------------------------------------------------------
//---------------- event_more widget ---------------------
//--------------------------------------------------------

public class EventMoreWindow 
{
	//[Widget] Gtk.Window jumps_runs_more;
	
	protected TreeStore store;
	[Widget] protected Gtk.TreeView treeview_more;
	[Widget] protected Gtk.Button button_accept;

	//static JumpsMoreWindow JumpsMoreWindowBox;
	protected Gtk.Window parent;
	
	protected string selectedEventType;
	protected string selectedEventName;
	//protected bool selectedStartIn;
	//protected bool selectedExtraWeight;
	protected string selectedDescription;
	public Gtk.Button button_selected;
	
	public EventMoreWindow () {
		//for inheritance issues
	}

	EventMoreWindow (Gtk.Window parent) {
/*
		Glade.XML gladeXML;
		try {
			gladeXML = Glade.XML.FromAssembly ("chronojump.glade", "jumps_runs_more", null);
		} catch {
			gladeXML = Glade.XML.FromAssembly ("chronojump.glade.chronojump.glade", "jumps_runs_more", null);
		}

		gladeXML.Autoconnect(this);
		this.parent = parent;
*/

		//name, startIn, weight, description
		store = new TreeStore(typeof (string), typeof (string), typeof (string), typeof (string));

		initializeThings();
	}

	protected void initializeThings() {
		button_selected = new Gtk.Button();
		
		createTreeView(treeview_more);

		treeview_more.Model = store;
		fillTreeView(treeview_more,store);

		button_accept.Sensitive = false;
		 
		treeview_more.Selection.Changed += OnSelectionEntry;
	}
	
	protected void OnSelectionEntry (object o, EventArgs args)
	{
		TreeModel model;
		TreeIter iter;

		if (((TreeSelection)o).GetSelected(out model, out iter))
		{
			selectedEventName = (string) model.GetValue (iter, 0);
			button_selected.Click();
		}
	}
	
	protected virtual void createTreeView (Gtk.TreeView tv) {
	}
	
	protected virtual void fillTreeView (Gtk.TreeView tv, TreeStore store) 
	{
	}

	//puts a value in private member selected
	protected virtual void on_treeview_changed (object o, EventArgs args)
	{
	}
	
	protected virtual void on_row_double_clicked (object o, Gtk.RowActivatedArgs args)
	{
	}
	
	//fired when something is selected for drawing on imageTest
	public Button Button_selected
	{
		get { return button_selected; }
	}

	public Button Button_accept 
	{
		set {
			button_accept = value;	
		}
		get {
			return button_accept;
		}
	}
	
	public string SelectedEventName
	{
		set {
			selectedEventName = value;	
		}
		get {
			return selectedEventName;
		}
	}
	
	public string SelectedDescription {
		get { return selectedDescription; }
	}
}
