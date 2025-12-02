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
 *  Copyright (C) 2025   Xavier de Blas <xaviblas@gmail.com>
 */

using System;
using System.Data;
using Gtk;
using System.Collections; //ArrayList
using Mono.Unix;


public class TreeViewEncoder : TreeViewEvent
{
	bool gravitatory; //on gravitatory show extraWeight, on inertial hide it
	private string repHtab = "   "; // horizontal spaces on reps data
	Constants.EncoderVariablesCapture encoderCaptureMainVariable;

	public TreeViewEncoder (Gtk.TreeView treeview, int newPrefsDigitsNumber,
			bool gravitatory, ExpandStates expandState, Constants.EncoderVariablesCapture encoderCaptureMainVariable)
	{
		this.treeview = treeview;
		this.pDN = newPrefsDigitsNumber;
		this.gravitatory = gravitatory;
		this.expandState = expandState;
		this.encoderCaptureMainVariable = encoderCaptureMainVariable;

		treeviewHasTwoLevels = true;
		dataLineNamePosition = 0; //position of name in the data to be printed
		dataLineTypePosition = 4; //position of type in the data to be printed
		allEventsName = Constants.AllTestsNameStr();

		boldableColumns_l = new List<int> { 1, 2, 3, 4, 5, 6, 7 };
		if (gravitatory)
		{
			idColumn = 8; //column where the uniqueID of event will be (and will be hidden)
			descriptionColumn = 6;

			columnsString = new string[] {
				"Exercise" + "\n" + repHtab + "Repetitions:",
					lateralityName + "\n" + repHtab + Constants.RangeAbsolute + " (cm)",
					weightExtraName + "\n" + repHtab + Catalog.GetString ("Mean speed") + " (m/s)",
					//Catalog.GetString ("Encoder configuration"),
					Catalog.GetString ("Contraction") + "\n" + repHtab + Catalog.GetString ("Max speed") + " (m/s)",
					datetimeName + "\n" + repHtab + Catalog.GetString ("Mean power") + " (W)",
					videoName + "\n" + repHtab + Catalog.GetString ("Peak power") + " (W)",
					descriptionName + "\n" + repHtab + Catalog.GetString ("Mean force") + " (N)",
					"\n" + repHtab + Catalog.GetString ("Max force") + " (N)"
						// "UNIQUEID" //just for debug
			};
		} else {
			idColumn = 10; //column where the uniqueID of event will be (and will be hidden)
			descriptionColumn = 9;

			columnsString = new string[] {
				"Exercise" + "\n" + repHtab + "Repetitions:",
					lateralityName + "\n" + repHtab + Constants.RangeAbsolute + " (cm)",
					Catalog.GetString ("Contraction") + "\n" + repHtab + Catalog.GetString ("Mean speed") + " (m/s)",
					//Catalog.GetString ("Encoder configuration"),
					"Diameter (cm)" + "\n" + repHtab + Catalog.GetString ("Max speed") + " (m/s)",
					"Weights" + "\n" + repHtab + Catalog.GetString ("Mean power") + " (W)",
					"Inertia M. (kg*cm^2)" + "\n" + repHtab + Catalog.GetString ("Peak power") + " (W)",
					"Equivalent mass (kg)" + "\n" + repHtab + Catalog.GetString ("Mean force") + " (N)",
					datetimeName + "\n" + repHtab + Catalog.GetString ("Max force") + " (N)",
					videoName + "\n",
					descriptionName + "\n"
					// "UNIQUEID" //just for debug
			};
		}

		store = getStore (columnsString.Length +1); //+1 because, eventID is not show in last col
		treeview.Model = store;
		prepareHeaders(columnsString);
	}

	protected override void prepareHeaders (string [] columnsString)
	{
		treeview.HeadersVisible=true;
		int i=0;
		foreach (string myCol in columnsString)
		{
			if (i == 0) // to show person name in bold if is currentPerson
			{
				Gtk.TreeViewColumn personNameColumn = new Gtk.TreeViewColumn ();
				CellRendererText personNameCell = new CellRendererText();
				personNameColumn.Title = myCol;
				personNameColumn.PackStart (personNameCell, true);
				personNameColumn.SetCellDataFunc (personNameCell, new Gtk.TreeCellDataFunc (RenderPersonName));
				treeview.AppendColumn (personNameColumn);
				i ++;
			} else {
				Gtk.TreeViewColumn col = new Gtk.TreeViewColumn ();
				CellRendererText cell = new CellRendererText();
				col.Title = myCol;
				col.PackStart (cell, true);
				col.SetCellDataFunc (cell, new Gtk.TreeCellDataFunc (RenderEncoderCols));
				treeview.AppendColumn (col);
				i ++;
			}
		}
	}

	private void RenderEncoderCols (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.ITreeModel model, Gtk.TreeIter iter)
	{
		if(! (cell is CellRendererText))
			return;

		// get the colID to use just this RenderInertialCols for all cols
		int colID = 0;
		for (int i = 0; i < columnsString.Length; i ++)
			if (column.Title == columnsString[i])
				colID = i;

		string text = (string) model.GetValue (iter, colID);
		int id = -1;
		if (model.GetValue (iter, idColumn) != null)
			if (Util.IsNumber ((string) (model.GetValue (iter, idColumn)), false))
				id = Convert.ToInt32 ( (string) model.GetValue (iter, idColumn));

		if (id == MarkNonSelectRowSubEvent)
		{
			if (text.StartsWith (boldMark))
			{
				text = Util.RemoveSubstring (text, boldMark);
				if (shouldRenderBoldable (column.Title))
				{
					(cell as Gtk.CellRendererText).Markup = "<span weight=\"600\">" + repHtab + text + "</span>";
				}
				else
					(cell as Gtk.CellRendererText).Markup = "<span foreground=\"#666666\">" + repHtab + text + "</span>";
			} else
				(cell as Gtk.CellRendererText).Markup = "<span foreground=\"#666666\">" + repHtab + text + "</span>";
		} else
			(cell as Gtk.CellRendererText).Text = text;
	}

	protected override bool shouldRenderBoldable (string columnTitle)
	{
		if (encoderCaptureMainVariable == Constants.EncoderVariablesCapture.RangeAbsolute &&
				columnTitle.Contains (Constants.RangeAbsolute))
			return true;
		if (encoderCaptureMainVariable == Constants.EncoderVariablesCapture.MeanSpeed &&
				columnTitle.Contains (Catalog.GetString ("Mean speed")))
			return true;
		if (encoderCaptureMainVariable == Constants.EncoderVariablesCapture.MaxSpeed &&
				columnTitle.Contains (Catalog.GetString ("Max speed")))
			return true;
		if (encoderCaptureMainVariable == Constants.EncoderVariablesCapture.MeanPower &&
				columnTitle.Contains (Catalog.GetString ("Mean power")))
			return true;
		if (encoderCaptureMainVariable == Constants.EncoderVariablesCapture.PeakPower &&
				columnTitle.Contains (Catalog.GetString ("Peak power")))
			return true;
		if (encoderCaptureMainVariable == Constants.EncoderVariablesCapture.MeanForce &&
				columnTitle.Contains (Catalog.GetString ("Mean force")))
			return true;
		if (encoderCaptureMainVariable == Constants.EncoderVariablesCapture.MaxForce &&
				columnTitle.Contains (Catalog.GetString ("Peak force")))
			return true;

		return false;
	}

	protected override void resultsInBarsRowChangedTwoLevels ()
	{
		TreeIter iter = new TreeIter();
		if(! treeview.Model.GetIterFirst (out iter))
			return;

		do {
			TreeIter iterDeep = new TreeIter ();
			treeview.Model.IterChildren (out iterDeep, iter);
			do {
				TreeIter iterDeep2 = new TreeIter ();
				treeview.Model.IterChildren (out iterDeep2, iterDeep);
				do {
					foreach (int j in boldableColumns_l)
						if (treeview.Model.GetValue (iterDeep2, j) != null &&
								((string) treeview.Model.GetValue (iterDeep2, j)).StartsWith (boldMark))
						{
							TreePath path = store.GetPath (iterDeep2);
							//LogB.Information ("EmitRowChanged: " + path.ToString ());
							treeview.Model.EmitRowChanged (path, iterDeep2);
						}
				} while (treeview.Model.IterNext (ref iterDeep2));
			} while (treeview.Model.IterNext (ref iterDeep));
		} while (treeview.Model.IterNext (ref iter));
	}

	public override void FillEncoder (List<List<EncoderSQL>> eSQL_ll, string filterExercise, List<string> videos_l)
	{
		LogB.Information ("called Fill Encoder");
		this.videos_l = videos_l;

		TreeIter iter = new TreeIter();
		TreeIter iterDeep = new TreeIter(); //only used by two levels treeviews
		int tempPersonID = -1; //one value that's not possible

		foreach (List<EncoderSQL> eSQL_l in eSQL_ll)
		{
			EncoderSQL eSQL0 = (EncoderSQL) eSQL_l[0]; //to have code a bit clearer
			//LogB.Information (eSQL0.ToString ());

			//show always the names of persons ...
			if (tempPersonID != eSQL0.PersonID)
			{
				iter = store.AppendValues (createPersonRow (eSQL0.PersonID, eSQL0.PersonNameGet));
				tempPersonID = eSQL0.PersonID;
			}

			//... but if we selected one type of test of this mode and this it's not the type, don't show
			if (filterExercise == allEventsName || filterExercise == Catalog.GetString (eSQL0.ExerciseName))
			{
				//getLineToStoreFromString is overriden in two level treeviews
				iterDeep = store.AppendValues (iter, getLineToStore (eSQL0));
				if (treeviewHasTwoLevels)
				{
					//addStatisticInfo (iterDeep, myEvent);
					for (int i = 1; i < eSQL_l.Count; i ++)
						store.AppendValues (iterDeep, getSubLineToStore (eSQL_l[i], i));
				}
			}
		}
	}

	// no need to be of selected set can be of any set
	public override void UpdateReps (List<List<EncoderSQL>> eSQL_ll)
	{
		LogB.Information ("treeview_encoder UpdateReps start");
		if (eSQL_ll.Count == 0)
			return;

		// get the treeiter of the set
		int setID = eSQL_ll[0][0].UniqueID;

		TreeIter iter = new TreeIter ();
		if (! getEvent (setID, out iter))
			return;

		// delete the repetitions (children: level2)
		TreeIter iterDeep = new TreeIter ();
		if (treeview.Model.IterHasChild (iter))
		{
			treeview.Model.IterChildren (out iterDeep, iter);
			do {
				//this will activate: on_treeview_results_session_cursor_changed (but we have blocked it with treeview_results_session_cursor_changed_block
				store.Remove (ref iterDeep); 	//delete iter (repetition)
			} while (store.IterIsValid (iterDeep));
		}

		// to not select next set. Return the selection to the desired set.
		SelectEvent (setID, false);

		// add the reps (if the exist)
		if (eSQL_ll[0].Count <= 1) // note 1st is a set. do this to check if it has reps
			return;

		if (! getEvent (setID, out iter))
			return;

		for (int i = 1; i < eSQL_ll[0].Count; i ++)
			store.AppendValues (iter, getSubLineToStore (eSQL_ll[0][i], i));

		// unfold the reps again
		treeview.ExpandToPath (treeview.Model.GetPath(iter));
		LogB.Information ("treeview_encoder UpdateReps end");
	}

	//TODO: check there is no crash with recent changes
	public override void AddEncoder (int personID, string pName, List<List<EncoderSQL>> eSQL_ll, string videoStr)
	{
		if (eSQL_ll.Count == 0)
			return;

		List<EncoderSQL> eSQL_l = eSQL_ll[0];
		if (eSQL_l.Count == 0)
			return;

		EncoderSQL eSQL0 = (EncoderSQL) eSQL_l[0]; //to have code a bit clearer

		TreeIter iter = new TreeIter();
		TreeIter iterDeep = new TreeIter(); //only used by two levels treeviews
		bool modelNotEmpty = treeview.Model.GetIterFirst ( out iter ) ;
		string iterPersonString;
		bool found = false;

		//on Add blank videos_l and if video the just add this one
		videos_l = new List<string> ();
		if (videoStr != "")
			videos_l.Add (videoStr);

		if(modelNotEmpty) {
			do {
				iterPersonString = ( treeview.Model.GetValue (iter, 0) ).ToString();
				if(iterPersonString == pName) {
					found = true;

					//expand the person
					treeview.ExpandToPath( treeview.Model.GetPath(iter) );

					//getLineToStore is overriden in two level treeviews
					iterDeep = store.AppendValues (iter, getLineToStore (eSQL0));

					//select the test
					treeview.Selection.SelectIter(iterDeep);

					TreePath path = store.GetPath (iterDeep);
					treeview.ScrollToCell (path, null, true, 0, 0);

					if(treeviewHasTwoLevels)
					{
						//addStatisticInfo (iterDeep, myEvent);
						for (int i = 1; i < eSQL_l.Count; i ++)
							store.AppendValues (iterDeep, getSubLineToStore (eSQL_l[i], i));
					}
				}
			} while (treeview.Model.IterNext (ref iter));
		}

		//if the person has not done this kind of event in this session, it's name doesn't appear in the treeview
		//create the name, and write the event
		if(! found)
		{
			iter = store.AppendValues (createPersonRow (personID, pName));
			iterDeep = store.AppendValues (iter, getLineToStore (eSQL0));

			//scroll treeview if needed
			TreePath path = store.GetPath (iterDeep);
			treeview.ScrollToCell (path, null, true, 0, 0);

			if(treeviewHasTwoLevels)
			{
				//addStatisticInfo (iterDeep, myEvent);
				for (int i = 1; i < eSQL_l.Count; i ++)
					store.AppendValues (iterDeep, getSubLineToStore (eSQL_l[i], i));
			}

			//expand the person
			treeview.ExpandToPath( treeview.Model.GetPath(iter) );

			//select the test
			treeview.Selection.SelectIter(iterDeep);
		}
	}


	/*
	 * unused as we used FillEncoder instead of Event.Fill
	protected override System.Object getObjectFromString (string [] strA)
	{
		return new EncoderSQL (
				Convert.ToInt32 (strA[1]), 	//uniqueID
				strA[6],	//laterality
				strA[7],	//extraWeight
				strA[5], 	//eccon
				strA[13], 	//description
				strA[20]  	//exerciseName
				);
	}
	*/

	protected override string [] getLineToStore (System.Object myObject)
	{
		EncoderSQL eSQL = (EncoderSQL) myObject;

		string [] myData = new String [getColsNum()];
		int count = 0;

		myData[count++] = eSQL.ExerciseName;
		myData[count++] = Catalog.GetString (eSQL.Laterality);
		if (gravitatory)
			myData[count++] = Util.TrimDecimals (eSQL.extraWeight, 2);
		myData[count++] = eSQL.ecconLong;

		if (! gravitatory)
		{
			myData[count++] = Util.TrimDecimals (eSQL.encoderConfiguration.d, 2);
			myData[count++] = eSQL.encoderConfiguration.extraWeightN.ToString ();
			myData[count++] = Util.TrimDecimals (UtilEncoder.CalculeInertiaTotal (eSQL.encoderConfiguration), 2);
			myData[count++] = Util.TrimDecimals (UtilEncoder.CalculateEquivalentMass (eSQL.encoderConfiguration), 2);
		}

		myData[count++] = eSQL.GetDatetimeStr (true);

		if (UtilList.StartsWithInListString (videos_l, string.Format ("{0}-{1}", Constants.TestTypes.ENCODER, eSQL.UniqueID)))
			myData[count++] = Catalog.GetString ("Yes");
		else
			myData[count++] = Catalog.GetString ("No");

		myData[count++] = eSQL.Description;
		if (gravitatory)
			myData[count++] = ""; //empty column (on set)
		myData[count++] = eSQL.UniqueID.ToString ();

		return myData;
	}

	protected override string [] getSubLineToStore (System.Object myObject, int i)
	{
		EncoderSQL eSQL = (EncoderSQL) myObject;

		string [] myData = new String [getColsNum()];
		int count = 0;

		myData[count++] = ""; // i.ToString ()  better not show a number now as it gets confused with the number of repetition on current set table.
		myData[count++] = boldMark + Util.TrimDecimals (UtilAll.DivideSafe (eSQL.rangeAbs, 10), 2); // mm -> cm
		myData[count++] = boldMark + Util.TrimDecimals (eSQL.meanSpeed, 2);
		myData[count++] = boldMark + Util.TrimDecimals (eSQL.maxSpeed, 2);
		myData[count++] = boldMark + Util.TrimDecimals (eSQL.meanPower, 2);
		myData[count++] = boldMark + Util.TrimDecimals (eSQL.maxPower, 2);
		myData[count++] = boldMark + Util.TrimDecimals (eSQL.meanForce, 2);
		myData[count++] = boldMark + Util.TrimDecimals (eSQL.maxForce, 2);
		if (! gravitatory)
		{
			myData[count++] = ""; //videoName;
			myData[count++] = ""; //eSQL.Description;
		}
		myData[count++] = MarkNonSelectRowSubEvent.ToString ();

		return myData;
	}

	public Constants.EncoderVariablesCapture EncoderCaptureMainVariable {
		set { encoderCaptureMainVariable = value; }
	}
}
