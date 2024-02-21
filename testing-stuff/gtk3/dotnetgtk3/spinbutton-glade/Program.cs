using Gdk;
using Gtk;
using static Gtk.Orientation;


public class EncoderConfigurationWindow 
{
	// at glade ---->
	Gtk.Window encoder_configuration;
	Gtk.Box box_combo_d_num1;
	// <----> at glade

    	SpinButton spinner = new SpinButton(1, 10, 1);


	static EncoderConfigurationWindow EncoderConfigurationWindowBox;
	
	
	EncoderConfigurationWindow ()
	{
		/*
		Glade.XML gladeXML;
		gladeXML = Glade.XML.FromAssembly (Util.GetGladePath() + "encoder_configuration.glade", "encoder_configuration", "chronojump");
		gladeXML.Autoconnect(this);
		*/
		//Gtk.Builder builder = new Gtk.Builder (null, Util.GetGladePath () + "encoder_configuration.glade", null);
		Gtk.Builder builder = new Gtk.Builder (null, "encoder_configuration.glade", null);
		connectWidgets (builder);
		builder.Autoconnect (this);
        
		box_combo_d_num1.Add (spinner);
		box_combo_d_num1.ShowAll ();
	}
	
	static public EncoderConfigurationWindow View ()
	{
		if (EncoderConfigurationWindowBox == null) {
			EncoderConfigurationWindowBox = new EncoderConfigurationWindow ();
		}
		
		EncoderConfigurationWindowBox.encoder_configuration.Show ();

		return EncoderConfigurationWindowBox;
	}
	
	void on_button_encoder_capture_inertial_cancel_clicked (object o, EventArgs args) {}
	void on_button_encoder_capture_inertial_do_clicked (object o, EventArgs args) {}
	void on_button_accept_clicked (object o, EventArgs args) {}
	private void on_button_close_clicked (object o, EventArgs args) {}
	private void on_button_previous_clicked (object o, EventArgs args) {}
	private void on_button_next_clicked (object o, EventArgs args) {}
	void on_button_encoder_capture_inertial_accuracy_clicked (object o, EventArgs args) {}
	void on_button_manage_show_clicked (object o, EventArgs args) {}
	void on_button_encoder_capture_inertial_show_clicked (object o, EventArgs args) {}
	void on_button_import_clicked (object o, EventArgs args) {}
	void on_button_export_clicked (object o, EventArgs args) {}
	private void on_button_side_action_clicked (object o, EventArgs args) {}
	private void on_button_edit_clicked (object o, EventArgs args) {}
	private void on_button_add_clicked (object o, EventArgs args) {}
	private void on_button_duplicate_clicked (object o, EventArgs args) {}
	void on_button_delete_clicked (object o, EventArgs args) {}
	private void on_button_cancel_clicked (object o, EventArgs args) {}

	void on_entry_save_name_changed	(object o, EventArgs args) {}
	void on_entry_save_description_changed (object o, EventArgs args) {}
	private void on_combo_d_num_changed (object o, EventArgs args) {}
	
	private void on_radio_encoder_type_linear_toggled (object obj, EventArgs args) {}
	private void on_radio_encoder_type_rotary_friction_toggled (object obj, EventArgs args) {}
	private void on_radio_encoder_type_rotary_axis_toggled (object obj, EventArgs args) {}
	private void on_check_rotary_friction_inertia_on_axis_toggled (object obj, EventArgs args) {}
	
	protected void on_delete_event (object o, DeleteEventArgs args)
	{
		args.RetVal = true;
	}

	private void connectWidgets (Gtk.Builder builder)
	{
		encoder_configuration = (Gtk.Window) builder.GetObject ("encoder_configuration");
		box_combo_d_num1 = (Gtk.Box) builder.GetObject ("box_combo_d_num1");
	}
}



class MyWindow : Gtk.Window {
    string[] nums = { "",
        "one", "two", "three", "four", "five",
        "six", "seven", "eight", "nine", "ten" };

    Scale horiz = new Scale(Horizontal, 1, 10, 1);
    Scale vert = new Scale(Vertical, 1, 10, 1);
    SpinButton spinner = new SpinButton(1, 10, 1);
    Label label = new Label("one");
    EncoderConfigurationWindow encoder_configuration_win;

    public MyWindow() : base("sliders") {
        Box row = new Box(Horizontal, 0);
        row.PackStart(horiz, true, true, 0);
        horiz.ValueChanged += on_horiz_changed;
        row.Add(vert);
        vert.ValueChanged += on_vert_changed;

        Box row2 = new Box(Horizontal, 8);
        row2.Add(spinner);
        spinner.ValueChanged += on_spinner_changed;
        row2.Add(label);

        Box vbox = new Box(Vertical, 0);
        vbox.PackStart(row, true, true, 0);
        vbox.Add(row2);
        Add(vbox);
        vbox.Margin = 8;

	encoder_configuration_win = EncoderConfigurationWindow.View ();

	//encoder_configuration_win.Button_close.Clicked += new EventHandler(on_encoder_configuration_win_closed);
    }

    void update(double val) {
        horiz.Value = vert.Value = spinner.Value = val;
        label.Text = nums[(int) val];
    }

    void on_horiz_changed(object? sender, EventArgs args) {
        update(horiz.Value);
    }

    void on_vert_changed(object? sender, EventArgs args) {
        update(vert.Value);
    }

    void on_spinner_changed(object? sender, EventArgs arg) {
        update(spinner.Value);
    }

    protected override bool OnDeleteEvent(Event e) {
        Application.Quit();
        return true;
    }
}

class Hello {
    static void Main() {
        Application.Init();
        MyWindow w = new MyWindow();
        w.Resize(150, 150);
        w.ShowAll();
        Application.Run();
    }
}
