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
 * Copyright (C) 2004-2012   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
using System.IO; 
using Gtk;
using Gdk;
using Glade;
using System.Collections;


public partial class ChronoJumpWindow 
{
	[Widget] Gtk.SpinButton spin_encoder_bar_limit;
	[Widget] Gtk.SpinButton spin_encoder_jump_limit;
	[Widget] Gtk.SpinButton spin_encoder_smooth;

	[Widget] Gtk.Button button_encoder_capture;
	[Widget] Gtk.Label label_encoder_person_weight;
	[Widget] Gtk.RadioButton radiobutton_encoder_concentric;
	[Widget] Gtk.RadioButton radiobutton_encoder_capture_bar;
	[Widget] Gtk.Viewport viewport_image_encoder_capture;
	[Widget] Gtk.Image image_encoder_bell;
	[Widget] Gtk.SpinButton spin_encoder_capture_time;
	[Widget] Gtk.SpinButton spin_encoder_capture_min_height;
	[Widget] Gtk.Image image_encoder_capture;
	[Widget] Gtk.TreeView treeview_encoder_curves;
	
	[Widget] Gtk.RadioButton radiobutton_encoder_analyze_powerbars;
	[Widget] Gtk.RadioButton radiobutton_encoder_analyze_single;
	[Widget] Gtk.RadioButton radiobutton_encoder_analyze_side;
	[Widget] Gtk.RadioButton radiobutton_encoder_analyze_superpose;
	[Widget] Gtk.Label label_encoder_analyze_curve_num;
	[Widget] Gtk.SpinButton spin_encoder_analyze_curve_num;
	[Widget] Gtk.Viewport viewport_image_encoder_analyze;
	[Widget] Gtk.Image image_encoder_analyze;

	TreeStore encoderStore;

	ArrayList encoderCurves;
        Gtk.ListStore encoderListStore;


	private string encoderAnalysis="powerBars";

//TODO: on capture only show ups if concentric

	//TODO: que el curve no pugui ser mes alt de actual numero de curves, per tant s'ha de retornar algun valor. ha de canviar cada cop que hi ha un capture o recalculate

	//TODO: campanes a l'encoder pq mostri colors i sons en funcio del que passa
	//TODO: in ec, curves and powerBars have to be different on ec than on c
	//
	
	public void on_radiobutton_encoder_capture_bar_toggled (object obj, EventArgs args) {
		spin_encoder_bar_limit.Sensitive = true;
		spin_encoder_jump_limit.Sensitive = false;
	}
	public void on_radiobutton_encoder_capture_jump_toggled (object obj, EventArgs args) {
		spin_encoder_bar_limit.Sensitive = false;
		spin_encoder_jump_limit.Sensitive = true;
	}

	//TODO: garantir path windows	
	void on_button_encoder_capture_clicked (object o, EventArgs args) 
	{
		//TODO: que surti barra de progres de calculando... despres de capturar i boto de cerrar automatico
		//TODO: i mostrar valors des de la gui (potser a zona dreta damunt del zoom)
		

		//capture data
		EncoderParams ep = new EncoderParams(
				(int) spin_encoder_capture_time.Value, 
				(int) spin_encoder_capture_min_height.Value, 
				!radiobutton_encoder_capture_bar.Active,
				findMass(),
				Util.ConvertToPoint((double) spin_encoder_smooth.Value), //R decimal: '.'
				findEccon()
				); 

		EncoderStruct es = new EncoderStruct(
				"",					//no data input
				"",					//no graph ouptut
				Util.GetEncoderDataTempFileName(), "", ep);				

		Util.RunPythonEncoder(Constants.EncoderScriptCapture, es, true);

		makeCurvesGraph();
		updateTreeView();
	}
	
	void on_button_encoder_recalculate_clicked (object o, EventArgs args) 
	{
		makeCurvesGraph();
		updateTreeView();
	}

	private void makeCurvesGraph() 
	{
	      	//show curves graph
		int w = UtilGtk.WidgetWidth(viewport_image_encoder_capture)-2; //image is inside (is smaller than) viewport
		int h = UtilGtk.WidgetHeight(viewport_image_encoder_capture)-2;

		EncoderParams ep = new EncoderParams(
				(int) spin_encoder_capture_min_height.Value, 
				!radiobutton_encoder_capture_bar.Active,
				findMass(),
				findEccon(), "curves",
				Util.ConvertToPoint((double) spin_encoder_smooth.Value), //R decimal: '.'
			       	0, 			//curve is not used here
				w, h); 

		EncoderStruct es = new EncoderStruct(
				Util.GetEncoderDataTempFileName(), 
				Util.GetEncoderGraphTempFileName(),
				Util.GetEncoderCurvesTempFileName(), 
				"NULL", ep);
		
		Util.RunPythonEncoder(Constants.EncoderScriptGraphCall, es,false);

		Pixbuf pixbuf = new Pixbuf (Util.GetEncoderGraphTempFileName()); //from a file
		image_encoder_capture.Pixbuf = pixbuf;
	}


	//TODO: garantir path windows	
	private void on_button_encoder_analyze_clicked (object o, EventArgs args) 
	{
		int w = UtilGtk.WidgetWidth(viewport_image_encoder_analyze)-2; //image is inside (is smaller than) viewport
		int h = UtilGtk.WidgetHeight(viewport_image_encoder_analyze)-2;

		EncoderParams ep = new EncoderParams(
				(int) spin_encoder_capture_min_height.Value, 
				!radiobutton_encoder_capture_bar.Active,
				findMass(),
				findEccon(), encoderAnalysis,
				Util.ConvertToPoint((double) spin_encoder_smooth.Value), //R decimal: '.'
				(int) spin_encoder_analyze_curve_num.Value, w, h);

		EncoderStruct es = new EncoderStruct(
				Util.GetEncoderDataTempFileName(), 
				Util.GetEncoderGraphTempFileName(),
				"NULL", "NULL", ep);		//no data ouptut

		Util.RunPythonEncoder(Constants.EncoderScriptGraphCall, es, false);

		//TODO pensar en si s'ha de fer 1er amb mida petita i despres amb gran (en el zoom), o si es una sola i fa alguna edicio
		Pixbuf pixbuf = new Pixbuf (Util.GetEncoderGraphTempFileName()); //from a file
		image_encoder_analyze.Pixbuf = pixbuf;
	}

//TODO: auto close capturing window

	//show curve_num only on simple and superpose
	public void on_radiobutton_encoder_analyze_single_toggled (object obj, EventArgs args) {
		label_encoder_analyze_curve_num.Sensitive=true;
		spin_encoder_analyze_curve_num.Sensitive=true;
		encoderAnalysis="single";
	}

	public void on_radiobutton_encoder_analyze_superpose_toggled (object obj, EventArgs args) {
		label_encoder_analyze_curve_num.Sensitive=true;
		spin_encoder_analyze_curve_num.Sensitive=true;
		encoderAnalysis="superpose";
	}
	public void on_radiobutton_encoder_analyze_side_toggled (object obj, EventArgs args) {
		label_encoder_analyze_curve_num.Sensitive=false;
		spin_encoder_analyze_curve_num.Sensitive=false;
		encoderAnalysis="side";
	}
	public void on_radiobutton_encoder_analyze_powerbars_toggled (object obj, EventArgs args) {
		label_encoder_analyze_curve_num.Sensitive=false;
		spin_encoder_analyze_curve_num.Sensitive=false;
		encoderAnalysis="powerBars";
	}

	private string findMass() {
		double mass = 0;
		if(radiobutton_encoder_capture_bar.Active)
			mass = spin_encoder_bar_limit.Value;
		else
			mass = Convert.ToDouble(label_encoder_person_weight.Text) + spin_encoder_jump_limit.Value;

		return Util.ConvertToPoint(mass); //R decimal: '.'
	}
	
	private string findEccon() {	
		if(radiobutton_encoder_concentric.Active)
			return "c";
		else
			return "ec";
	}
	

	/* TreeView stuff */	

	public void CreateTreeViewEncoder() {
		string [] columnsString = {"n","Width","Height","MeanSpeed","MaxSpeed",
			"MeanPower","PeakPower","PeakPowerT"};

		string contents = Util.ReadFile(Util.GetEncoderCurvesTempFileName());
		if (contents == null) 
			return;

		encoderCurves = new ArrayList ();

		string line;
		using (StringReader reader = new StringReader (contents)) {
			line = reader.ReadLine ();	//headers
			Log.WriteLine(line);
			do {
				line = reader.ReadLine ();
				Log.WriteLine(line);
				if (line == null)
					break;

				string [] cells = line.Split(new char[] {','});
				cells = fixDecimals(cells);
				//iter = encoderStore.AppendValues(cells);

				encoderCurves.Add (new EncoderCurve (cells[0], cells[1], cells[2], 
							cells[3], cells[4], cells[5], cells[6],cells[7]));

			} while(true);
		}

		encoderListStore = new Gtk.ListStore (typeof (EncoderCurve));
		foreach (EncoderCurve curve in encoderCurves) {
			encoderListStore.AppendValues (curve);
		}

		treeview_encoder_curves.Model = encoderListStore;

		treeview_encoder_curves.HeadersVisible=true;

		int i=0;
		foreach(string myCol in columnsString) {
			Gtk.TreeViewColumn aColumn = new Gtk.TreeViewColumn ();
			CellRendererText aCell = new CellRendererText();
			aColumn.Title=myCol;
			aColumn.PackStart (aCell, true);

			//crt1.Foreground = "red";
			//crt1.Background = "blue";
		
			switch(i){	
				case 0:
					aColumn.SetCellDataFunc (aCell, new Gtk.TreeCellDataFunc (RenderN));
					break;
				case 1:
					aColumn.SetCellDataFunc (aCell, new Gtk.TreeCellDataFunc (RenderWidth));
					break;
				case 2:
					aColumn.SetCellDataFunc (aCell, new Gtk.TreeCellDataFunc (RenderHeight));
					break;
				case 3:
					aColumn.SetCellDataFunc (aCell, new Gtk.TreeCellDataFunc (RenderMeanSpeed));
					break;
				case 4:
					aColumn.SetCellDataFunc (aCell, new Gtk.TreeCellDataFunc (RenderMaxSpeed));
					break;
				case 5:
					aColumn.SetCellDataFunc (aCell, new Gtk.TreeCellDataFunc (RenderMeanPower));
					break;
				case 6:
					aColumn.SetCellDataFunc (aCell, new Gtk.TreeCellDataFunc (RenderPeakPower));
					break;
				case 7:
					aColumn.SetCellDataFunc (aCell, new Gtk.TreeCellDataFunc (RenderPeakPowerT));
					break;
			}
			

			treeview_encoder_curves.AppendColumn (aColumn);
			i++;
		}
	}


	/* rendering columns */

	string colorGood= "ForestGreen"; //more at System.Drawing.Color (Monodoc)
	string colorBad= "red";
	private void RenderN (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter)
	{
		EncoderCurve curve = (EncoderCurve) model.GetValue (iter, 0);
		(cell as Gtk.CellRendererText).Text = curve.N;
	}
	private void RenderWidth (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter)
	{
		EncoderCurve curve = (EncoderCurve) model.GetValue (iter, 0);
		(cell as Gtk.CellRendererText).Text = curve.Width;
	}
	private void RenderHeight (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter)
	{
		EncoderCurve curve = (EncoderCurve) model.GetValue (iter, 0);
		(cell as Gtk.CellRendererText).Text = curve.Height;
	}
	
	private void RenderMeanSpeed (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter)
	{
		EncoderCurve curve = (EncoderCurve) model.GetValue (iter, 0);
		if (Convert.ToDouble(curve.MeanSpeed) >= 2.5) 
			(cell as Gtk.CellRendererText).Foreground = colorGood;
		else 
			(cell as Gtk.CellRendererText).Foreground = colorBad;

		(cell as Gtk.CellRendererText).Text = curve.MeanSpeed;
	}

	private void RenderMaxSpeed (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter)
	{
		EncoderCurve curve = (EncoderCurve) model.GetValue (iter, 0);
		if (Convert.ToDouble(curve.MaxSpeed) >= 2.5) 
			(cell as Gtk.CellRendererText).Foreground = colorGood;
		else 
			(cell as Gtk.CellRendererText).Foreground = colorBad;

		(cell as Gtk.CellRendererText).Text = curve.MaxSpeed;
	}

	private void RenderMeanPower (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter)
	{
		EncoderCurve curve = (EncoderCurve) model.GetValue (iter, 0);
		if (Convert.ToDouble(curve.MeanPower) >= 1700) 
			(cell as Gtk.CellRendererText).Foreground = colorGood;
		else 
			(cell as Gtk.CellRendererText).Foreground = colorBad;

		(cell as Gtk.CellRendererText).Text = curve.MeanPower;
	}

	private void RenderPeakPower (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter)
	{
		EncoderCurve curve = (EncoderCurve) model.GetValue (iter, 0);
		if (Convert.ToDouble(curve.PeakPower) >= 1700) 
			(cell as Gtk.CellRendererText).Foreground = colorGood;
		else 
			(cell as Gtk.CellRendererText).Foreground = colorBad;

		(cell as Gtk.CellRendererText).Text = curve.PeakPower;
	}

	private void RenderPeakPowerT (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter)
	{
		EncoderCurve curve = (EncoderCurve) model.GetValue (iter, 0);
		if (Convert.ToDouble(curve.PeakPowerT) <= 100) 
			(cell as Gtk.CellRendererText).Foreground = colorGood;
		else 
			(cell as Gtk.CellRendererText).Foreground = colorBad;

		(cell as Gtk.CellRendererText).Text = curve.PeakPowerT;
	}

	/* end of rendering cols */


	
	private void removeColumns() {
		Gtk.TreeViewColumn [] myColumns = treeview_encoder_curves.Columns;
		foreach (Gtk.TreeViewColumn column in myColumns) 
			treeview_encoder_curves.RemoveColumn (column);
	}

	private void updateTreeView() {
		removeColumns();
		CreateTreeViewEncoder();
	}

	
	private string [] fixDecimals(string [] cells) {
		for(int i=1; i <= 2; i++)
			cells[i] = Util.TrimDecimals(Convert.ToDouble(Util.ChangeDecimalSeparator(cells[i])),1);
		for(int i=3; i <= 6; i++)
			cells[i] = Util.TrimDecimals(Convert.ToDouble(Util.ChangeDecimalSeparator(cells[i])),3);
		return cells;
	}
	
	
	/* end of TreeView stuff */	

	
}	

