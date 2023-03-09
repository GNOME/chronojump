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
 * Copyright (C) 2020   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
using System.IO; 
using Gtk;
using Gdk;
//using Glade;
using System.Collections;
using System.Collections.Generic; //List<T>
using Mono.Unix;

//adapted from src/gui/encoderSelectRepetitions.cs
public class TagSessionSelect
{
	//passed variables
	private bool addSession;
	private int sessionID;
	private string sessionName;
	private bool askDeletion;

	private ArrayList allTags_list; //all available tags
	private ArrayList allTags_listPrint;
	private List<TagSession> tagsActiveThisSession_list;
	private static GenericWindow genericWin;
	private string [] columnsString;
        private ArrayList bigArray;
	private string [] checkboxes;
	private string tagsListStringForAddSession;

	public Gtk.Button FakeButtonDone;

	public void PassVariables(bool addSession, int sessionID, string sessionName, bool askDeletion)
	{
		this.addSession = addSession;
		this.sessionID = sessionID;
		this.sessionName = sessionName;
		this.askDeletion = askDeletion;

		FakeButtonDone = new Gtk.Button();
	}

	public void Do() {
                getData();
                createBigArray();
                nullifyGenericWindow();
                createGenericWindow();
        }

	private void nullifyGenericWindow()
	{
		if(genericWin != null && ! genericWin.GenericWindowBoxIsNull())
			genericWin.HideAndNull();
	}

	//used when click on "Select" button
	public void Show()
	{
		//if user destroyed window (on_delete_event), recreate it again
		if(genericWin.GenericWindowBoxIsNull() || ! createdGenericWinIsOfThisType())
			Do();

		activateCallbacks();
		genericWin.ShowNow();
		genericWin.SetButtonAcceptSensitive(true);
	}


	private void getData()
        {
		allTags_list = SqliteTagSession.Select(false, -1);

		if(addSession)
			tagsActiveThisSession_list = new List<TagSession>();
		else
			tagsActiveThisSession_list = SqliteSessionTagSession.SelectTagsOfASession(false, sessionID);
        }

	private void createBigArray()
	{
		allTags_listPrint = new ArrayList();
		checkboxes = new string[allTags_list.Count]; //to store active or inactive status of tags
		int count = 0;
		foreach(TagSession tagS in allTags_list)
		{
			string str = "inactive";
			foreach(TagSession tagActiveThisSession in tagsActiveThisSession_list)
				if(tagActiveThisSession.UniqueID == tagS.UniqueID)
				{
					str = "active";
					break;
				}

			checkboxes[count++] = str;
			allTags_listPrint.Add(tagS.ToStringArray());
		}

		columnsString = new string[] {
			"ID",
			Catalog.GetString("Active"),
			Catalog.GetString("Name"),
			//Catalog.GetString("Color"),
			//Catalog.GetString("Comments")
		};

		bigArray = new ArrayList();
		ArrayList a1 = new ArrayList();
		ArrayList a2 = new ArrayList();
		ArrayList a3 = new ArrayList();
		ArrayList a4 = new ArrayList();

		//0 is the widgget to show; 1 is the editable; 2 id default value
		a1.Add(Constants.GenericWindowShow.ENTRY2); a1.Add(true); a1.Add("");
		bigArray.Add(a1);

		a2.Add(Constants.GenericWindowShow.BUTTONMIDDLE); a2.Add(true); a2.Add("");
                bigArray.Add(a2);

		a3.Add(Constants.GenericWindowShow.LABELBEFORETEXTVIEWTREEVIEW); a3.Add(true); a3.Add("");
		bigArray.Add(a3);

		a4.Add(Constants.GenericWindowShow.TREEVIEW); a4.Add(true); a4.Add("");
		bigArray.Add(a4);
	}

	private void createGenericWindow()
	{
                genericWin = GenericWindow.Show(Catalog.GetString("Tags"), false,       //don't show now
                                Catalog.GetString("Manage tags"), bigArray);

		genericWin.SetTreeview(columnsString, true, allTags_listPrint, new ArrayList(), GenericWindow.EditActions.EDITDELETE, false);

		genericWin.LabelEntry2 = Catalog.GetString("Create new tag");
		genericWin.SetButtonMiddleLabel(Catalog.GetString("Create"));

		if(sessionName == "")
			genericWin.LabelBeforeTextViewTreeView = Catalog.GetString("Select tags for this session");
		else
			genericWin.LabelBeforeTextViewTreeView = Catalog.GetString("Select tags for session: ") + sessionName;

		genericWin.ShowEditRow(false);
		genericWin.HideEditRowCombo();
		genericWin.SetLabelComment(Catalog.GetString("Change name"));
		genericWin.CommentColumn = 2; //used for the name

		genericWin.ResetComboCheckBoxesOptions();
		genericWin.CreateComboCheckBoxes();
		genericWin.MarkActiveCurves(checkboxes);

		genericWin.Type = GenericWindow.Types.TAGSESSION;
	}

	private bool createdGenericWinIsOfThisType() {
		if(genericWin.Type == GenericWindow.Types.TAGSESSION)
			return true;

		return false;
	}

	private void activateCallbacks() {
		//manage selected, unselected curves
		genericWin.Button_accept.Clicked -= new EventHandler(on_tag_session_win_done);
		genericWin.Button_accept.Clicked += new EventHandler(on_tag_session_win_done);

		genericWin.Button_middle.Clicked += new EventHandler(on_tag_session_win_tag_added);
		genericWin.Button_row_edit.Clicked += new EventHandler(on_tag_session_win_row_edit);
		genericWin.Button_row_edit_apply.Clicked += new EventHandler(on_tag_session_win_row_edit_apply);
		genericWin.Button_row_delete.Clicked += new EventHandler(on_tag_session_win_row_delete_prequestion);
	}

	private void removeCallbacks() {
		genericWin.Button_accept.Clicked -= new EventHandler(on_tag_session_win_done);

		genericWin.Button_middle.Clicked -= new EventHandler(on_tag_session_win_tag_added);
		genericWin.Button_row_edit.Clicked -= new EventHandler(on_tag_session_win_row_edit);
		genericWin.Button_row_edit_apply.Clicked -= new EventHandler(on_tag_session_win_row_edit_apply);
		genericWin.Button_row_delete.Clicked -= new EventHandler(on_tag_session_win_row_delete_prequestion);
	}

	private void on_tag_session_win_done (object o, EventArgs args)
	{
		LogB.Information("on_tag_session_win_done");
		removeCallbacks();

		//get selected/deselected rows
		checkboxes = genericWin.GetColumn(1, false);

		//need to refresh allTags_list because tasks could have been added/deleted
		allTags_list = SqliteTagSession.Select(false, -1);

		if(addSession) {
			int count = 0;
			string sep = "";
			tagsListStringForAddSession = "";
			foreach(TagSession tag in allTags_list)
			{
				//LogB.Information("TagSession: " + tag.ToString() + "; checkboxes[count]: " + checkboxes[count]);
				if(checkboxes[count] == "active")
				{
					tagsListStringForAddSession += sep + tag.Name;
					sep = ", ";
				}
				count ++;
			}
		} else
			SQLUpdateTransaction(sessionID);

		FakeButtonDone.Click();
	}

	/*
	   this is called on edit session just on close genericWin
	   and on add, when session is added (and there is a sessionID)
	   */
	public void SQLUpdateTransaction(int sID)
	{
		LogB.Information("SQLUpdateTransaction sID: " + sID.ToString());

		//update on database the what has been selected/deselected
		//doing it as a transaction: FAST
		SqliteSessionTagSession.UpdateTransaction(sID, allTags_list,
				tagsActiveThisSession_list, checkboxes);
	}

	private void on_tag_session_win_tag_added (object o, EventArgs args)
	{
		string nameNew = genericWin.Entry2Selected;
		if(TagSession.CheckIfTagNameExists(false, nameNew))
		{
			new DialogMessage(Constants.MessageTypes.WARNING,
					string.Format("Error: a tag named: '{0}' already exists.", nameNew));
			return;
		}

		TagSession ts = new TagSession(-1, nameNew, "#000000", "");
		int uniqueID = ts.InsertSQL(false);
		genericWin.Entry2Selected = "";

		//update treeview
		genericWin.Row_add_alphabetical (new string[] { uniqueID.ToString(), "", ts.Name }, 2);
	}

	private void on_tag_session_win_row_edit (object o, EventArgs args)
	{
		genericWin.ShowEditRow(true);
	}
	private void on_tag_session_win_row_edit_apply (object o, EventArgs args)
	{
		LogB.Information("on_tag_session_win_row_row_edit_apply. Opening db:");

		Sqlite.Open();

		//1) select set
		int id = genericWin.TreeviewSelectedUniqueID;
		TagSession ts = (TagSession) SqliteTagSession.Select(true, id)[0];

		//2) if changed comment, update SQL, and update treeview
		//first remove conflictive characters
		string nameNew = Util.RemoveTildeAndColonAndDot(genericWin.EntryEditRow);
		if(nameNew != ts.Name)
		{
			if(TagSession.CheckIfTagNameExists(true, nameNew))
			{
				new DialogMessage(Constants.MessageTypes.WARNING,
						string.Format("Error: a tag named: '{0}' already exists.", nameNew));
				return;
			}

			ts.Name = nameNew;
			Sqlite.Update(true, Constants.TagSessionTable, "name",
					"", ts.Name,
					"uniqueID", ts.UniqueID.ToString());

			//update treeview
			genericWin.on_edit_selected_done_update_treeview();
		}

		genericWin.ShowEditRow(false);
		genericWin.SensitiveEditDeleteIfSelected();

		Sqlite.Close();
	}

	private void on_tag_session_win_row_delete_prequestion (object o, EventArgs args)
	{
		if(askDeletion)
		{
			int id = genericWin.TreeviewSelectedUniqueID;
			TagSession ts = (TagSession) SqliteTagSession.Select(false, id)[0];

			ConfirmWindow confirmWin = ConfirmWindow.Show(Catalog.GetString(
						"Are you sure you want to delete this tag?"), "", ts.Name);
			confirmWin.Button_accept.Clicked += new EventHandler(on_tag_session_win_row_delete);
		} else
			on_tag_session_win_row_delete (o, args);
	}
	private void on_tag_session_win_row_delete (object o, EventArgs args)
	{
		LogB.Information("row delete at tag_session");

		int tagID = genericWin.TreeviewSelectedUniqueID;
		LogB.Information(tagID.ToString());

		SqliteTagSession.Delete(false, tagID);

		genericWin.Delete_row_accepted();
	}

	public string TagsListStringForAddSession
	{
		get { return tagsListStringForAddSession; }
	}
}
