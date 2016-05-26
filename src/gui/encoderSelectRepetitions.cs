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
 * Copyright (C) 2004-2016   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
using System.IO; 
using Gtk;
using Gdk;
using Glade;
using System.Collections;
using System.Collections.Generic; //List<T>
using Mono.Unix;
using LongoMatch.Gui;



public class EncoderSelectRepetitions
{
	//passed variables
	
	protected Person currentPerson;
	protected Session currentSession;
	protected Constants.EncoderGI encoderGI;
	protected GenericWindow genericWin;
	protected Gtk.Button button_encoder_analyze;
	protected int exerciseID; //can be -1 (all)
	protected bool askDeletion;
	
	protected int dateColumn;
	//protected int activeRepsColumn;
	protected int allRepsColumn;
	

	//calculated variables here
	
	protected string [] columnsString;
	//protected var columnsString; //www.dotnetperls.com/array  var arr3 = new string[] { "one", "two", "three" };
	protected ArrayList data;
	protected ArrayList bigArray;
	protected string [] checkboxes;
	protected ArrayList nonSensitiveRows;
	
	//public variables accessed mainly from gui/encoder.cs	
	
	public Gtk.Button FakeButtonDeleteCurve;
	public Gtk.Button FakeButtonDone;
	public int DeleteCurveID;
	public enum Types { UNDEFINED, INDIVIDUAL_CURRENT_SESSION, INDIVIDUAL_ALL_SESSIONS, GROUPAL_CURRENT_SESSION }
	public Types Type;
	
	//could be Interperson or Intersession
	//personID:personName
	//sessionID:sessionDate
	public ArrayList EncoderCompareInter;
	public int RepsActive;
	public int RepsAll;
	public List<double> EncoderInterSessionDateOnXWeights;

	
	public EncoderSelectRepetitions() {
		Type = Types.UNDEFINED;
	}

	public void PassVariables(Person currentP, Session currentS, Constants.EncoderGI eGI,
			GenericWindow gw, Gtk.Button button_e_a, int exID, bool askDel) 
	{
		RepsActive = 0;
		RepsAll = 0;
		FakeButtonDone = new Gtk.Button();

		currentPerson = currentP;
		currentSession = currentS;
		encoderGI = eGI;

		genericWin = gw;
		button_encoder_analyze = button_e_a;
		exerciseID = exID; //can be -1 (all)
		askDeletion = askDel;
	}
	
	//public GenericWindow Do() {
	public GenericWindow Do() {
		getData();
		createBigArray();
		createGenericWindow();
		genericWin.Type = GenericWindow.Types.ENCODER_SEL_REPS;

		return genericWin;
	}

	//used when click on "Select" button
	public void Show() {
		RepsActive = 0;
		RepsAll = 0;

		activateCallbacks();
		
		genericWin.ShowNow();
	}

	protected virtual void getData() {
	}
	protected virtual void createBigArray() {
	}
	protected virtual void createGenericWindow() {
	}

	protected virtual void activateCallbacks() {
		//manage selected, unselected curves
		genericWin.Button_accept.Clicked += new EventHandler(on_show_repetitions_done);
	}
	
	protected virtual void on_show_repetitions_done (object o, EventArgs args) {
	}
	
	protected void updateEncoderCompareInterAndReps() 
	{
		EncoderCompareInter = new ArrayList ();
	
		//find RepsActive
		string [] selectedID = genericWin.GetColumn(0,true); //only active
		string [] selectedDate = genericWin.GetColumn(dateColumn,true); //only active
		for (int i=0 ; i < selectedID.Length ; i ++) 
		{
			int id = Convert.ToInt32(selectedID[i]);
			RepsActive += genericWin.GetCell(id, allRepsColumn);
			EncoderCompareInter.Add(id + ":" + selectedDate[i]);
		}
		
		//find RepsAll
		string [] allID = genericWin.GetColumn(0,false); //unchecked (session or person don't need to be selected)
		for (int i=0 ; i < allID.Length ; i ++) 
		{
			int id = Convert.ToInt32(allID[i]);
			RepsAll += genericWin.GetCell(id, allRepsColumn);
		}
	}
	
	public ArrayList GetEncoderInterSessionDateOnXWeightsForCombo() {
		ArrayList a = new ArrayList();
		a.Add(Catalog.GetString("All weights"));
		foreach(double d in EncoderInterSessionDateOnXWeights)
			a.Add(d.ToString());

		return a;
	}
}

public class EncoderSelectRepetitionsIndividualCurrentSession : EncoderSelectRepetitions
{
	ArrayList dataPrint;

	public EncoderSelectRepetitionsIndividualCurrentSession() {
		Type = Types.INDIVIDUAL_CURRENT_SESSION;

		FakeButtonDeleteCurve = new Gtk.Button();
	}

	protected override void getData() 
	{
		data = SqliteEncoder.Select(
				false, -1, currentPerson.UniqueID, currentSession.UniqueID, encoderGI,
				exerciseID, "curve", EncoderSQL.Eccons.ALL, 
				false, true);
	}

	protected override void createBigArray() 
	{
		dataPrint = new ArrayList();
		checkboxes = new string[data.Count]; //to store active or inactive status of curves
		int count = 0;
		foreach(EncoderSQL es in data) {
			checkboxes[count++] = es.status;
			dataPrint.Add(es.ToStringArray(count,true,false,true,true));

			if(es.status == "active")
				RepsActive ++;
			
			RepsAll ++;
		}

		columnsString = new string[] {
			Catalog.GetString("ID"),
			Catalog.GetString("Active"),	//checkboxes
			Catalog.GetString("Repetition"),
			Catalog.GetString("Exercise"),
			"RL",
			Catalog.GetString("Extra weight"),
			Catalog.GetString("Mean Power"),
			Catalog.GetString("Encoder"),
			Catalog.GetString("Contraction"),
			Catalog.GetString("Date"),
			Catalog.GetString("Comment")
		};

		bigArray = new ArrayList();
		ArrayList a1 = new ArrayList();
		ArrayList a2 = new ArrayList();
		ArrayList a3 = new ArrayList();

		//0 is the widgget to show; 1 is the editable; 2 id default value
		a1.Add(Constants.GenericWindowShow.COMBOALLNONESELECTED); a1.Add(true); a1.Add("ALL");
		bigArray.Add(a1);

		a2.Add(Constants.GenericWindowShow.TREEVIEW); a2.Add(true); a2.Add("");
		bigArray.Add(a2);

		a3.Add(Constants.GenericWindowShow.COMBO); a3.Add(true); a3.Add("");
		bigArray.Add(a3);
	}
	
	protected override void createGenericWindow() 
	{
		/*
		 * Disabled because combo exercise is selected before (not on genericWin)
		 *
		add exercises to the combo (only the exercises done, and only unique)
		ArrayList encoderExercisesNames = new ArrayList();
		foreach(EncoderSQL es in data) {
			encoderExercisesNames = Util.AddToArrayListIfNotExist(encoderExercisesNames, Catalog.GetString(es.exerciseName));
		}
		*/
		
		genericWin = GenericWindow.Show(false,	//don't show now
				string.Format(Catalog.GetString("Saved repetitions of athlete {0} on this session."), 
					currentPerson.Name) + "\n" + 
				Catalog.GetString("Activate the repetitions you want to use clicking on first column.") + "\n" +
				Catalog.GetString("If you want to edit or delete a row, right click on it.") + "\n",
				bigArray);

		genericWin.SetTreeview(columnsString, true, dataPrint, new ArrayList(), Constants.ContextMenu.EDITDELETE, false);

		genericWin.ResetComboCheckBoxesOptions();
		//genericWin.AddOptionsToComboCheckBoxesOptions(encoderExercisesNames);
		genericWin.CreateComboCheckBoxes();

		genericWin.MarkActiveCurves(checkboxes);
		
		//find all persons in current session
		ArrayList personsPre = SqlitePersonSession.SelectCurrentSessionPersons(
				currentSession.UniqueID,
				false); //means: do not returnPersonAndPSlist
		
		string [] persons = new String[personsPre.Count];
		int count = 0;
	        foreach	(Person p in personsPre)
			persons[count++] = p.UniqueID.ToString() + ":" + p.Name;
		genericWin.SetComboValues(persons, currentPerson.UniqueID + ":" + currentPerson.Name);
		genericWin.SetComboLabel(Catalog.GetString("Change the owner of selected repetition") + 
				" (" + Catalog.GetString("code") + ":" + Catalog.GetString("name") + ")");
		genericWin.ShowEditRow(false);
		genericWin.CommentColumn = 10;
		
		genericWin.ShowButtonCancel(false);
		genericWin.SetButtonAcceptSensitive(true);
		genericWin.SetButtonCancelLabel(Catalog.GetString("Close"));

		//used when we don't need to read data, 
		//and we want to ensure next window will be created at needed size
		//genericWin.DestroyOnAccept=true;
		//here is comented because we are going to read the checkboxes
	}
	
	protected override void activateCallbacks() {
		//manage selected, unselected curves
		genericWin.Button_accept.Clicked += new EventHandler(on_show_repetitions_done);
		
		genericWin.Button_row_edit.Clicked += new EventHandler(on_show_repetitions_row_edit);
		genericWin.Button_row_edit_apply.Clicked += new EventHandler(on_show_repetitions_row_edit_apply);
		genericWin.Button_row_delete.Clicked += new EventHandler(on_show_repetitions_row_delete_pre);
	}
	
	
	protected override void on_show_repetitions_done (object o, EventArgs args)
	{
		//don't stop calling here in order to arrive when encSelReps.Show() is called and accept is clicked
		genericWin.Button_accept.Clicked -= new EventHandler(on_show_repetitions_done);

		//get selected/deselected rows
		checkboxes = genericWin.GetColumn(1, false);

		ArrayList data = SqliteEncoder.Select(
				false, -1, currentPerson.UniqueID, currentSession.UniqueID, encoderGI,
				exerciseID, "curve", EncoderSQL.Eccons.ALL, 
				false, true);

		//update on database the curves that have been selected/deselected
		//doing it as a transaction: FAST
		RepsActive = SqliteEncoder.UpdateTransaction(data, checkboxes);
		RepsAll = data.Count;

		FakeButtonDone.Click();		
	}


	// --------------- edit curves start ---------------
	
	protected void on_show_repetitions_row_edit (object o, EventArgs args) {
		LogB.Information("row edit at show curves");
		LogB.Information(genericWin.TreeviewSelectedUniqueID.ToString());
		genericWin.ShowEditRow(true);
	}

	protected void on_show_repetitions_row_edit_apply (object o, EventArgs args) {
		LogB.Information("row edit apply at show curves");

		int curveID = genericWin.TreeviewSelectedUniqueID;
		EncoderSQL eSQL = (EncoderSQL) SqliteEncoder.Select(
				false, curveID, 0, 0, encoderGI,
				-1, "", EncoderSQL.Eccons.ALL, 
				false, true)[0];

		//if changed comment, update SQL, and update treeview
		//first remove conflictive characters
		string comment = Util.RemoveTildeAndColonAndDot(genericWin.EntryEditRow);
		if(comment != eSQL.description) {
			eSQL.description = comment;
			SqliteEncoder.Update(false, eSQL);

			//update treeview
			genericWin.on_edit_selected_done_update_treeview();
		}

		//if changed person, proceed
		LogB.Information("new person: " + genericWin.GetComboSelected);
		int newPersonID = Util.FetchID(genericWin.GetComboSelected);
		if(newPersonID != currentPerson.UniqueID) {
			EncoderSQL eSQLChangedPerson = eSQL.ChangePerson(genericWin.GetComboSelected);
			SqliteEncoder.Update(false, eSQLChangedPerson);

			genericWin.RemoveSelectedRow();
		}

		genericWin.ShowEditRow(false);
	}
	
	// --------------- edit curves end ---------------
	
	
	// --------------- delete curves start ---------------
	
	protected void on_show_repetitions_row_delete_pre (object o, EventArgs args) {
		if(askDeletion) {
			ConfirmWindow confirmWin = ConfirmWindow.Show(Catalog.GetString(
						"Are you sure you want to delete this repetition?"), "", "");
			confirmWin.Button_accept.Clicked += new EventHandler(on_show_repetitions_row_delete);
		} else
			on_show_repetitions_row_delete (o, args);
	}
	
	protected void on_show_repetitions_row_delete (object o, EventArgs args) {
		LogB.Information("row delete at show curves");

		int uniqueID = genericWin.TreeviewSelectedUniqueID;
		string status = genericWin.GetCheckboxStatus(uniqueID);

		DeleteCurveID = uniqueID;
		FakeButtonDeleteCurve.Click();

		if(status == "active")
			RepsActive --;
		RepsAll --;

		genericWin.Delete_row_accepted();
		FakeButtonDone.Click();		
	}

	
	// --------------- delete curves end ---------------
	
}


public class EncoderSelectRepetitionsIndividualAllSessions : EncoderSelectRepetitions
{
	private int repsByWeightsColumn;

	public EncoderSelectRepetitionsIndividualAllSessions() {
		Type = Types.INDIVIDUAL_ALL_SESSIONS;
	
		dateColumn = 3;
		//activeRepsColumn = 4;
		allRepsColumn = 4;
		repsByWeightsColumn = allRepsColumn +1;

		if(EncoderCompareInter == null)
			EncoderCompareInter = new ArrayList ();
	}

	protected override void getData() 
	{
		data = SqliteEncoder.SelectCompareIntersession(false, encoderGI, exerciseID, currentPerson.UniqueID); 
	}
	
	protected override void createBigArray() 
	{
		nonSensitiveRows = new ArrayList();
		int i = 0;
		//prepare checkboxes to be marked	
		checkboxes = new string[data.Count]; //to store active or inactive status
		int count = 0;
		foreach(EncoderPersonCurvesInDB encPS in data) 
		{
			bool found = false;
		
			if(encPS.countAll == 0)
				nonSensitiveRows.Add(count);
			else {
				foreach(string s2 in EncoderCompareInter)
					if(Util.FetchID(s2) == encPS.sessionID)
						found = true;

				//if EncoderCompareInter is empty, then add currentSession
				//if is not empty, then don't add it because maybe user doesn't want to compare with this session
				if(EncoderCompareInter.Count == 0 && encPS.sessionID == currentSession.UniqueID)
					found = true;
			}

			if(found) {
				checkboxes[count++] = "active";
			} else
				checkboxes[count++] = "inactive";
			
			i ++;
		}			
			
		columnsString = new string[] {
			Catalog.GetString("ID"),
			"",				//checkboxes
			Catalog.GetString("Session name"),
			Catalog.GetString("Session date"),
			//Catalog.GetString("Selected\nrepetitions"),
			//Catalog.GetString("All\nrepetitions")
			Catalog.GetString("Saved repetitions"),
			"Weights (repetitions * weight)"
		};

		bigArray = new ArrayList();
		ArrayList a1 = new ArrayList();
		ArrayList a2 = new ArrayList();
		
		//0 is the widgget to show; 1 is the editable; 2 id default value
		a1.Add(Constants.GenericWindowShow.COMBOALLNONESELECTED); a1.Add(true); a1.Add("ALL");
		bigArray.Add(a1);
		
		a2.Add(Constants.GenericWindowShow.TREEVIEW); a2.Add(true); a2.Add("");
		bigArray.Add(a2);
	}
	
	protected override void createGenericWindow() 
	{
		genericWin = GenericWindow.Show(false,  //don't show now	//TODO: change message
				string.Format(Catalog.GetString("Compare repetitions between the following sessions"),
					currentPerson.Name), bigArray);

		//convert data from array of EncoderPersonCurvesInDB to array of strings []
		ArrayList dataConverted = new ArrayList();
		foreach(EncoderPersonCurvesInDB encPS in data) {
			dataConverted.Add(encPS.ToStringArray(true));
		}

		genericWin.SetTreeview(columnsString, true, dataConverted, nonSensitiveRows, Constants.ContextMenu.NONE, false);

		genericWin.ResetComboCheckBoxesOptions();
		genericWin.CreateComboCheckBoxes();
		
		genericWin.MarkActiveCurves(checkboxes);
		genericWin.ShowButtonCancel(false);
		genericWin.SetButtonAcceptSensitive(true);
		
		//to have encoderCompareInter without opening the select window
		updateEncoderCompareInterAndReps();
		updateEncoderInterSessionDateOnXWeights();

		//used when we don't need to read data, 
		//and we want to ensure next window will be created at needed size
		//genericWin.DestroyOnAccept=true;
		//here is comented because we are going to read the checkboxes
	}
		
	private void updateEncoderInterSessionDateOnXWeights() 
	{
		EncoderInterSessionDateOnXWeights = new List<double>();
		
		string [] selectedID = genericWin.GetColumn(0,true); //only active
		string [] selectedRepsByWeights = genericWin.GetColumn(repsByWeightsColumn,true); //only active
		for (int i=0 ; i < selectedID.Length ; i ++) 
		{
			string [] repsByWeights = selectedRepsByWeights[i].Split(new char[] {' '});
			//repsByWeights is "3*10", "5*70", "4*120"
			foreach(string repByWeight in repsByWeights) {
				string [] chunks = repByWeight.Split(new char[] {'*'});
				if(Util.IsNumber(chunks[1], true))
					EncoderInterSessionDateOnXWeights = Util.AddToListDoubleIfNotExist(
							EncoderInterSessionDateOnXWeights, Convert.ToDouble(chunks[1]));
			}
		}
					
		EncoderInterSessionDateOnXWeights.Sort();
	}
	
	protected override void on_show_repetitions_done (object o, EventArgs args) 
	{
		//don't stop calling here in order to arrive when encSelReps.Show() is called and accept is clicked
		genericWin.Button_accept.Clicked -= new EventHandler(on_show_repetitions_done);
	
		updateEncoderCompareInterAndReps();
		updateEncoderInterSessionDateOnXWeights();
	
		FakeButtonDone.Click();		
		LogB.Information("done");
	}
}

public class EncoderSelectRepetitionsGroupalCurrentSession : EncoderSelectRepetitions
{
	public EncoderSelectRepetitionsGroupalCurrentSession() {
		Type = Types.GROUPAL_CURRENT_SESSION;
		
		dateColumn = 2;
		//activeRepsColumn = 3;
		allRepsColumn = 3;

		if(EncoderCompareInter == null)
			EncoderCompareInter = new ArrayList ();
	}

	protected override void getData() 
	{
		ArrayList dataPre = SqlitePersonSession.SelectCurrentSessionPersons(currentSession.UniqueID,
				false); //means: do not returnPersonAndPSlist
		data = new ArrayList();
		
		nonSensitiveRows = new ArrayList();
		int j = 0;	//list of added persons
		foreach(Person p in dataPre) {
			ArrayList eSQLarray = SqliteEncoder.Select(
					false, -1, p.UniqueID, currentSession.UniqueID, encoderGI,
					exerciseID, "curve", EncoderSQL.Eccons.ALL, 
					false, true);

			int activeCurves = UtilEncoder.GetActiveCurvesNum(eSQLarray);
			int allCurves = eSQLarray.Count;

			string [] s = { p.UniqueID.ToString(), "", p.Name,
				//activeCurves.ToString(), 
				allCurves.ToString()
			};
			data.Add(s);
			
			if(allCurves == 0)
				nonSensitiveRows.Add(j);

			j ++;
		}
	}
	
	protected override void createBigArray() 
	{
		//prepare checkboxes to be marked	
		checkboxes = new string[data.Count]; //to store active or inactive status
		int i = 0;
		int count = 0;
		foreach(string [] sPersons in data) {
			bool found = false;
			bool nonSensitive = false;
		
			//don't use active reps, use all
			foreach(int nsr in nonSensitiveRows)
				if(nsr == i)
					nonSensitive = true;

			if(nonSensitive == false) {
				foreach(string s2 in EncoderCompareInter)
					if(Util.FetchID(s2).ToString() == sPersons[0])
						found = true;

				//if EncoderCompareInter is empty, then add currentPerson
				//if is not empty, then don't add it because maybe user doesn't want to compare with this person
				if(EncoderCompareInter.Count == 0 && sPersons[0] == currentPerson.UniqueID.ToString())
					found = true;
			}

			if(found) {
				checkboxes[count++] = "active";
			} else
				checkboxes[count++] = "inactive";
				
			i ++;
		}			
			
		columnsString = new string [] {
			Catalog.GetString("ID"),
			"",				//checkboxes
			Catalog.GetString("Person name"),
			//Catalog.GetString("Selected\nrepetitions"),
			//Catalog.GetString("All\nrepetitions")
			Catalog.GetString("Saved repetitions")
		};

		bigArray = new ArrayList();
		ArrayList a1 = new ArrayList();
		ArrayList a2 = new ArrayList();
		
		//0 is the widgget to show; 1 is the editable; 2 id default value
		a1.Add(Constants.GenericWindowShow.COMBOALLNONESELECTED); a1.Add(true); a1.Add("ALL");
		bigArray.Add(a1);
		
		a2.Add(Constants.GenericWindowShow.TREEVIEW); a2.Add(true); a2.Add("");
		bigArray.Add(a2);
	}
	
	protected override void createGenericWindow() 
	{
		genericWin = GenericWindow.Show(false,	//don't show now
				Catalog.GetString("Select persons to compare"), bigArray);

		genericWin.SetTreeview(columnsString, true, data, nonSensitiveRows, Constants.ContextMenu.NONE, false);

		//select this person row
		genericWin.SelectRowWithID(0, currentPerson.UniqueID);

		genericWin.ResetComboCheckBoxesOptions();
		genericWin.CreateComboCheckBoxes();
		
		genericWin.MarkActiveCurves(checkboxes);
		genericWin.ShowButtonCancel(false);
		genericWin.SetButtonAcceptSensitive(true);
		
		//to have encoderCompareInter without opening the select window
		updateEncoderCompareInterAndReps();

		//used when we don't need to read data, 
		//and we want to ensure next window will be created at needed size
		//genericWin.DestroyOnAccept=true;
		//here is comented because we are going to read the checkboxes
	}
	
	protected override void on_show_repetitions_done (object o, EventArgs args) 
	{
		//don't stop calling here in order to arrive when encSelReps.Show() is called and accept is clicked
		genericWin.Button_accept.Clicked -= new EventHandler(on_show_repetitions_done);
		
		updateEncoderCompareInterAndReps();
	
		FakeButtonDone.Click();		
		LogB.Information("done");
	}
	
}
