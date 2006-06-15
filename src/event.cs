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
using System.Data;
using Mono.Data.SqliteClient;
using System.Text; //StringBuilder

using System.Threading;
using System.IO.Ports;


public class Event 
{
	protected int personID;
	protected string personName;
	protected int sessionID;
	protected int uniqueID;
	protected string type;
	protected string description;

	protected Thread thread;
	//platform state variables
	protected enum States {
		ON,
		OFF
	}
	
	//better as private and don't inherit, don't know why
	//protected Chronopic cp;
	//private Chronopic cp;
	

	
	protected States loggedState;		//log of last state
	protected Gtk.ProgressBar progressBar;
	protected Gtk.Statusbar appbar;
	protected Gtk.Window app;
	protected int pDN;

	//for raise a signal and manage it on chronojump.cs
	protected Gtk.Button fakeButtonFinished;
	
	//for cancelling from chronojump.cs
	protected bool cancel;
	
	
	public Event() {
	}
	
	public virtual void Simulate(Random rand)
	{
	}

	protected virtual Chronopic.Plataforma chronopicInitialValue(Chronopic cp)
	{
		Chronopic.Plataforma platformState  = Chronopic.Plataforma.UNKNOW; //on (in platform), off (jumping), or unknow
		bool ok = false;
		Console.WriteLine("A1");

		do {
			Console.WriteLine("B");
			try {
				ok = cp.Read_platform(out platformState);
			} catch {
				Console.WriteLine("Manage called after finishing constructor, do later");
			}
			Console.WriteLine("C");
		} while (! ok);

		return platformState;
	}
	
	public virtual void Manage(object o, EventArgs args)
	{
	}
	

	protected virtual void waitEvent () {
	}
	
	protected bool PulseGTK ()
	{
		//if (thread.IsAlive) {
			if(progressBar.Fraction == 1 || cancel) {
				Console.Write("dying");

				//event will be raised, and managed in chronojump.cs
				//fakeButtonFinished.Click();
				//Now called on write(), now work in mono1.1.6
				
				return false;
			}
			Thread.Sleep (150);
			Console.Write(thread.ThreadState);
			return true;
		//}
		//return false;
	}

	protected virtual void write() {
	}
	
	//from confirm_window cancel button (thread has not started)
	//this is NOT called when a event has started and user click on "Cancel"
	protected void cancel_event(object o, EventArgs args)
	{
		//event will be raised, and managed in chronojump.cs
		fakeButtonFinished.Click();
		
		cancel = true;
	}
	
	public Gtk.Button FakeButtonFinished
	{
		get {
			return	fakeButtonFinished;
		}
	}

	//called from chronojump.cs for cancelling events
	public bool Cancel
	{
		get { return cancel; }
		set { cancel = value; }
	}
	
	public string Type
	{
		get { return type; }
		set { type = value; }
	}
	
	public string Description
	{
		get { return description; }
		set { description = value; }
	}
	
	public int UniqueID
	{
		get { return uniqueID; }
		set { uniqueID = value; }
	}

	public int SessionID
	{
		get { return sessionID; }
	}

	public int PersonID
	{
		get { return personID; }
	}
		
	public string PersonName
	{
		get { return personName; }
	}
	
	~Event() {}
	   
}
