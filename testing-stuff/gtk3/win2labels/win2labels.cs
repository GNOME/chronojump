using System;
using Gtk;

public class main
{
	static Win2labels win2labels;

	public static void Main (string[] args)
	{
		Gtk.Application.Init (); //needed

		win2labels = Win2labels.Show ();
		
		Gtk.Application.Run ();
	}
}

public class Win2labels
{
	//defined in glade
	Gtk.Window window;
	Gtk.Label label1;
	Gtk.Label label2;
	Gtk.Box hbox_combo;

	static Win2labels Win2labelsBox;
	Gtk.ComboBoxText combo;
	
	public Win2labels ()
	{
		/* 
		 * A) as file
		Builder builder = new Builder();
		builder.AddFromFile ("win2labels.glade");
		*/

		// B) as resource
		Builder builder = new Builder (null, "win2labels.glade", null);


		window = (Gtk.Window) builder.GetObject("window");

		//can this program work without next two lines?
		label1 = (Gtk.Label) builder.GetObject("label1");
		label2 = (Gtk.Label) builder.GetObject("label2");
		hbox_combo = (Gtk.Box) builder.GetObject("hbox");

		//builder.Autoconnect (window);
		builder.Autoconnect (this);

		label1.Text = "Hello!";
		label2.Text = "Bye!";

		combo = new ComboBoxText ();
		combo.AppendText ("què");
		combo.AppendText ("tal");
		combo.AppendText ("estàs?");
		combo.Active = 0;
		hbox_combo.PackStart (combo, true, true, 0);
		hbox_combo.ShowAll();
	}

	static public Win2labels Show ()
	{
		Win2labelsBox = new Win2labels ();
		Win2labelsBox.window.Show ();

		return Win2labelsBox;
	}

	void on_window_draw (object o, Gtk.DrawnArgs args)
	{
                //Console.WriteLine ("draw");
	}

	void on_window_delete_event (object o, DeleteEventArgs args)
        {
                Console.WriteLine ("closing");
/*
                //do not hide/exit if copyiing
                if (thread != null && thread.IsAlive)
                        args.RetVal = true;
                else {
*/
                        Win2labelsBox.window.Hide();
                        Win2labelsBox = null;
			Application.Quit ();
//              }
        }
}
