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
 *  Copyright (C) 2016-2017   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
using System.Collections.Generic; //List<T>
using Gdk;
using Gtk;
using Mono.Unix;


public class TypePix
{
	public ChronopicRegisterPort.Types Type; //public enum Types { UNKNOWN, CONTACTS, ENCODER, ARDUINO_RFID, ARDUINO_FORCE }
	public Pixbuf Pix;

	public TypePix(ChronopicRegisterPort.Types type, Pixbuf pix)
	{
		Type = type;
		Pix = pix;
	}
}

public static class TypePixList
{
	public static List<TypePix> l;

	//one for each type
	static TypePixList()
	{
		l = new List<TypePix>();

		l.Add(new TypePix(ChronopicRegisterPort.Types.UNKNOWN, new Pixbuf (null, Util.GetImagePath(false) + "board-unknown.png")));
		l.Add(new TypePix(ChronopicRegisterPort.Types.CONTACTS, new Pixbuf (null, Util.GetImagePath(false) + "board-jump-run.png")));
		l.Add(new TypePix(ChronopicRegisterPort.Types.ENCODER, new Pixbuf (null, Util.GetImagePath(false) + "board-encoder.png")));
		l.Add(new TypePix(ChronopicRegisterPort.Types.ARDUINO_RFID, new Pixbuf (null, Util.GetImagePath(false) + "board-arduino-rfid.png")));
		l.Add(new TypePix(ChronopicRegisterPort.Types.ARDUINO_FORCE, new Pixbuf (null, Util.GetImagePath(false) + "board-arduino-force.png")));
	}

	public static Pixbuf GetPix(ChronopicRegisterPort.Types type)
	{
		foreach(TypePix tp in l)
		{
			if(tp.Type == type)
				return tp.Pix;
		}

		return l[0].Pix; //return first value if there are problems
	}

	public static TypePix GetPixPrevNext(ChronopicRegisterPort.Types type, string direction)
	{
		int currentPos = 0;
		int nextPos = 0;
		foreach(TypePix tp in l)
		{
			if(tp.Type == type)
			{
				if(direction == "LEFT")
				{
					nextPos = currentPos -1;
					if(nextPos < 0)
						nextPos = 0;
				} else
				{
					nextPos = currentPos +1;
					if(nextPos > l.Count -1)
						nextPos = l.Count -1;
				}

				return(l[nextPos]);
			}
			currentPos ++;
		}

		return(l[0]); //if there are problems, return UNKNOWN value
	}
}


public class ChronopicRegisterWindow
{
	Gtk.Window chronopic_register_win;
	Gtk.VBox vbox_main;
	Gtk.Label label_macOSX;
	private List<ChronopicRegisterPort> listConnected;

	public Gtk.Button FakeButtonCloseSerialPort;

	public ChronopicRegisterWindow(Gtk.Window app1, List<ChronopicRegisterPort> listAll)
	{
		createWindow(app1);
		UtilGtk.IconWindow(chronopic_register_win); //put an icon to window

		listConnected = new List<ChronopicRegisterPort>();

		//create listConnected with connected chronopics
		int connectedCount = 0;
		int unknownCount = 0;
		foreach(ChronopicRegisterPort crp in listAll)
		{
			if(crp.Port != "")
			{
				listConnected.Add(crp);
				connectedCount ++;

				if(crp.Type == ChronopicRegisterPort.Types.UNKNOWN)
					unknownCount ++;
			}
		}

		createVBoxMain();
		createContent(connectedCount, unknownCount);
		createButtons();

		chronopic_register_win.ShowAll();
	}

	private void createWindow(Gtk.Window app1)
	{
		chronopic_register_win = new Gtk.Window (Catalog.GetString("Chronojump devices"));
		chronopic_register_win.AllowGrow = false;
		chronopic_register_win.Modal = true;
		chronopic_register_win.TransientFor = app1;

		chronopic_register_win.DeleteEvent += on_delete_event;

		/* Sets the border width of the window. */
		chronopic_register_win.BorderWidth= 20;
	}

	private void createVBoxMain()
	{
		vbox_main = new Gtk.VBox(false, 20);
		chronopic_register_win.Add(vbox_main);
	}

	Gtk.Table table_main;

	private List<Gtk.Image> list_images;
	private List<Gtk.Label> list_labels_type;
	private List<Gtk.Button> list_buttons_left;
	private List<Gtk.Button> list_buttons_right;

	private void createTable ()
	{
		int rows = listConnected.Count;

		Gtk.Label label_serial_title = new Gtk.Label("<b>" + Catalog.GetString("Serial Number") + "</b>");
		Gtk.Label label_port_title = new Gtk.Label("<b>" + Catalog.GetString("Port") + "</b>");
		Gtk.Label label_type_title = new Gtk.Label("<b>" + Catalog.GetString("Type") + "</b>");

		label_serial_title.UseMarkup = true;
		label_port_title.UseMarkup = true;
		label_type_title.UseMarkup = true;

		label_serial_title.Show();
		label_port_title.Show();
		label_type_title.Show();

		table_main = new Gtk.Table((uint) rows +1, 3, false); //not homogeneous
		table_main.ColumnSpacing = 8;
		table_main.RowSpacing = 6;

		table_main.Attach (label_serial_title, (uint) 1, (uint) 2, 0, 1);
		table_main.Attach (label_port_title, (uint) 2, (uint) 3, 0, 1);
		table_main.Attach (label_type_title, (uint) 3, (uint) 4, 0, 1);

		list_buttons_left = new List<Gtk.Button>();
		list_images = new List<Gtk.Image>();
		list_labels_type = new List<Gtk.Label>();
		list_buttons_right = new List<Gtk.Button>();

		for (int count=1; count <= rows; count ++)
		{
			string serialStr = listConnected[count -1].SerialNumber;
			Gtk.Label label_serial = new Gtk.Label(serialStr);
			table_main.Attach (label_serial, (uint) 1, (uint) 2, (uint) count, (uint) count +1);
			label_serial.Show();

			Gtk.Label label_port = new Gtk.Label(listConnected[count -1].Port);
			table_main.Attach (label_port, (uint) 2, (uint) 3, (uint) count, (uint) count +1);
			label_port.Show();

			Gtk.HBox hbox_type = new Gtk.HBox(false, 6);
			Button button_left = create_arrow_button(ArrowType.Left, ShadowType.In);
			button_left.Sensitive = (listConnected[count-1].Type != TypePixList.l[0].Type);
			button_left.CanFocus = false;
			button_left.IsFocus = false;
			button_left.Clicked += on_button_left_clicked;
			hbox_type.Add(button_left);

			//create image
			Pixbuf pixbuf = TypePixList.GetPix(listConnected[count-1].Type);
			Gtk.Image image = new Gtk.Image();
			image.Pixbuf = pixbuf;
			hbox_type.Add(image);

			Button button_right = create_arrow_button(ArrowType.Right, ShadowType.In);
			button_right.CanFocus = false;
			button_right.IsFocus = false;
			button_right.Clicked += on_button_right_clicked;
			button_right.Sensitive = (listConnected[count-1].Type != TypePixList.l[TypePixList.l.Count -1].Type);
			hbox_type.Add(button_right);

			Gtk.VBox vbox = new Gtk.VBox(false, 2);
			vbox.Add(hbox_type);
			Gtk.Label label_type = new Gtk.Label(ChronopicRegisterPort.TypePrint(listConnected[count-1].Type));
			vbox.Add(label_type);

			table_main.Attach (vbox, (uint) 3, (uint) 4, (uint) count, (uint) count +1);

			list_buttons_left.Add(button_left);
			list_images.Add(image);
			list_labels_type.Add(label_type);
			list_buttons_right.Add(button_right);
		}
		table_main.Show();
	}

	static Button create_arrow_button(ArrowType arrow_type, ShadowType  shadow_type )
	{
		Button button = new Button ();
		Arrow  arrow = new Arrow (arrow_type, shadow_type);

		button.Add(arrow);

		button.Show();
		arrow.Show();

		return button;
	}

	private void createContent(int connectedCount, int unknownCount)
	{
		//create top hbox
		Gtk.HBox hbox = new Gtk.HBox(false, 12);

		Pixbuf pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_chronopic_connect_big.png");
		//hbox image
		Gtk.Image image = new Gtk.Image();
		image.Pixbuf = pixbuf;
		hbox.Add(image);

		//hbox label
		Gtk.Label label = new Gtk.Label();
		label.Text = writeLabel(connectedCount, unknownCount);
		hbox.Add(label);
		vbox_main.Add(hbox);

		//table
		if(connectedCount > 0)
		{
			createTable();
			Gtk.VBox vboxTV = new Gtk.VBox(false, 10);
			vboxTV.Add(table_main);
			vbox_main.Add(vboxTV);
		}
	}

	private string writeLabel(int connectedCount, int unknownCount)
	{
		if(connectedCount > 0)
		{
			string str = string.Format(Catalog.GetPluralString(
						"Found 1 device.",
						"Found {0} devices.",
						connectedCount),
					connectedCount);

			if(unknownCount > 0)
			{
				str += "\n\n";
				str += string.Format(Catalog.GetPluralString(
							"One device is not configured. Please, configure it.",
							"{0} devices are not configured. Please, configure them.",
							unknownCount),
						unknownCount) + "\n";
			}
			return str;
		}

		return Catalog.GetString("Device not found") + "\n\n" + Catalog.GetString("Connect and reopen this window.");
	}

	private void createButtons()
	{
		label_macOSX = new Gtk.Label();
		label_macOSX.Text = Catalog.GetString("There is a known problem with MacOSX:") + "\n" +
				Catalog.GetString("If Chronopic is disconnected after jumps or runs execution,\nthat port will be blocked until restart of machine.") + "\n\n" +
				Catalog.GetString("We are working on a solution.");
		if( UtilAll.GetOSEnum() == UtilAll.OperatingSystems.MACOSX)
			vbox_main.Add(label_macOSX);

		FakeButtonCloseSerialPort = new Gtk.Button();
		Gtk.Button button_close_serial_port = new Gtk.Button("Close serial port (debug)");
		button_close_serial_port.Clicked += new EventHandler(on_button_close_serial_port_clicked);

		//---- button close start --->
		Gtk.Button button_close = new Gtk.Button("Close Window");
		button_close.CanFocus = true;
		button_close.IsFocus = true;
		button_close.Clicked += new EventHandler(on_button_close_clicked);

		Gtk.AccelGroup ag = new Gtk.AccelGroup (); //button can be called clicking Escape key
		chronopic_register_win.AddAccelGroup (ag);

		button_close.AddAccelerator
			("activate", ag, new Gtk.AccelKey
			 (Gdk.Key.Escape, Gdk.ModifierType.None,
			  Gtk.AccelFlags.Visible));
		//<---- button close end

		//add buttons to containers
		Gtk.HButtonBox hbox = new Gtk.HButtonBox ();
		//hbox.Add(button_close_serial_port);

		hbox.Add(button_close);

		vbox_main.Add(hbox);
	}

	private void on_button_left_clicked(object o, EventArgs args)
	{
		Button buttonClicked = o as Button;
		if (o == null)
			return;

		int count = 0;
		foreach(Gtk.Button button in list_buttons_left)
		{
			if(button == buttonClicked)
			{
				TypePix tp = TypePixList.GetPixPrevNext(listConnected[count].Type, "LEFT");
				listConnected[count].Type = tp.Type;
				list_images[count].Pixbuf = tp.Pix;
				list_labels_type[count].Text = ChronopicRegisterPort.TypePrint(listConnected[count].Type);

				buttons_sensitivity(button, list_buttons_right[count], tp.Type);
				updateSQL(listConnected[count].SerialNumber, tp.Type);
			}
			count ++;
		}
	}
	private void on_button_right_clicked(object o, EventArgs args)
	{
		Button buttonClicked = o as Button;
		if (o == null)
			return;

		int count = 0;
		foreach(Gtk.Button button in list_buttons_right)
		{
			if(button == buttonClicked)
			{
				TypePix tp = TypePixList.GetPixPrevNext(listConnected[count].Type, "RIGHT");
				listConnected[count].Type = tp.Type;
				list_images[count].Pixbuf = tp.Pix;
				list_labels_type[count].Text = ChronopicRegisterPort.TypePrint(listConnected[count].Type);

				buttons_sensitivity(list_buttons_left[count], button, tp.Type);
				updateSQL(listConnected[count].SerialNumber, tp.Type);
			}
			count ++;
		}
	}

	private void buttons_sensitivity(Gtk.Button left, Gtk.Button right, ChronopicRegisterPort.Types type)
	{
		left.Sensitive = (type != TypePixList.l[0].Type);
		right.Sensitive = (type != TypePixList.l[TypePixList.l.Count -1].Type);
		//LogB.Information("count + tplcount " + count + "," + TypePixList.l.Count);
	}

	private void updateSQL(string serialNumber, ChronopicRegisterPort.Types type)
	{
		//store on SQL
		SqliteChronopicRegister.Update(false, new ChronopicRegisterPort(serialNumber, type), type);
	}
	
	private void on_button_close_serial_port_clicked(object o, EventArgs args)
	{
		//try first to see if a sp is opened on a cp but that /ttyusbserial does not exists
		FakeButtonCloseSerialPort.Click();
	}

	private void on_button_close_clicked(object o, EventArgs args)
	{
		chronopic_register_win.Hide();
		chronopic_register_win = null;
	}

	private void on_delete_event (object o, DeleteEventArgs args)
	{
		LogB.Information("calling on_delete_event");

		args.RetVal = true;

		on_button_close_clicked(new object(), new EventArgs());
	}
}

