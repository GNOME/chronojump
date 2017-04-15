/*
 * This file is part of ChronoJump
 *
 * Chronojump is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or   
 *    (at your option) any later version.
 *    
 * Chronojump is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the 
 *    GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 *  along with this program; if not, write to the Free Software
 *   Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 *
 * Copyright (C) 2004-2017   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
using System.IO;


public class Config
{
	public enum SessionModeEnum { STANDARD, UNIQUE, MONTHLY }

	public Preferences.MaximizedTypes Maximized;
	public bool CustomButtons;
	public bool UseVideo;
	public bool OnlyEncoderGravitatory;
	public bool OnlyEncoderInertial;
	public bool EncoderCaptureShowOnlyBars;
	public bool EncoderUpdateTreeViewWhileCapturing; //recomended: false. Make it false if EncoderCaptureShowOnlyBars == true
							//because treeview will be in 2n page of notebook
	public bool PersonWinHide;
	public bool EncoderAnalyzeHide;
	public SessionModeEnum SessionMode;
	public bool Compujump;
	public string RunScriptOnExit;

	public Config()
	{
		Maximized = Preferences.MaximizedTypes.NO;
		CustomButtons = false;
		UseVideo = true;
		
		OnlyEncoderGravitatory = false;
		OnlyEncoderInertial = false;
		EncoderCaptureShowOnlyBars = false;
		EncoderUpdateTreeViewWhileCapturing = true;
		PersonWinHide = false;
		EncoderAnalyzeHide = false;
		SessionMode = SessionModeEnum.STANDARD;
		Compujump = false;
		RunScriptOnExit = "";
	}

	/*
	public void Read()
	{
		string contents = Util.ReadFile(UtilAll.GetConfigFileName(), false);
		if (contents != null && contents != "") 
		{
			string line;
			using (StringReader reader = new StringReader (contents)) {
				do {
					line = reader.ReadLine ();

					if (line == null)
						break;
					if (line == "" || line[0] == '#')
						continue;

					string [] parts = line.Split(new char[] {'='});
					if(parts.Length != 2)
						continue;

					if(parts[0] == "Maximized" && Util.StringToBool(parts[1]))
						Maximized = true;
					else if(parts[0] == "CustomButtons" && Util.StringToBool(parts[1]))
						CustomButtons = true;
					else if(parts[0] == "UseVideo" && ! Util.StringToBool(parts[1]))
						UseVideo = false;
					else if(parts[0] == "OnlyEncoderGravitatory" && Util.StringToBool(parts[1]))
						OnlyEncoderGravitatory = true;
					else if(parts[0] == "OnlyEncoderInertial" && Util.StringToBool(parts[1]))
						OnlyEncoderInertial = true;
					else if(parts[0] == "EncoderCaptureShowOnlyBars" && Util.StringToBool(parts[1]))
						EncoderCaptureShowOnlyBars = true;
					else if(parts[0] == "EncoderUpdateTreeViewWhileCapturing" && ! Util.StringToBool(parts[1]))
						EncoderUpdateTreeViewWhileCapturing = false;
					else if(parts[0] == "PersonWinHide" && Util.StringToBool(parts[1]))
						PersonWinHide = true;
					else if(parts[0] == "EncoderAnalyzeHide" && Util.StringToBool(parts[1]))
						EncoderAnalyzeHide = true;
					else if(parts[0] == "SessionMode" && Enum.IsDefined(typeof(SessionModeEnum), parts[1]))
						SessionMode = (SessionModeEnum) 
							Enum.Parse(typeof(SessionModeEnum), parts[1]);
					else if(parts[0] == "Compujump" && Util.StringToBool(parts[1]))
						Compujump = true;
					else if(parts[0] == "RunScriptOnExit" && parts[1] != "")
						RunScriptOnExit = parts[1];
				} while(true);
			}
		}
	}
	*/
	/*
	public static void UpdateField(string field, string text)
	{
		//adapted from
		//http://stackoverflow.com/a/2401873
				
		string tempfile = Path.GetTempFileName();

		LogB.Information("UpdateField, field: " + field + ", text: " + text);
		
		if(! File.Exists(UtilAll.GetConfigFileName())) {
			try {
				using (var writer = new StreamWriter(tempfile))
				{
					writer.WriteLine(field + "=" + text);
				}
				File.Copy(tempfile, UtilAll.GetConfigFileName(), true);
			} catch {
				LogB.Warning("Cannot write at Config.UpdateField");
			}
		} else {
			try {
				using (var writer = new StreamWriter(tempfile))
					using (var reader = new StreamReader(UtilAll.GetConfigFileName()))
					{
						while (! reader.EndOfStream) {
							string line = reader.ReadLine();
							if (line != "" && line[0] != '#') 
							{
								string [] parts = line.Split(new char[] {'='});
								if(parts.Length == 2 && parts[0] == field)
									line = field + "=" + text;
							}

							writer.WriteLine(line);
						}
					}
				File.Copy(tempfile, UtilAll.GetConfigFileName(), true);
			} catch {
				LogB.Warning("Cannot write at Config.UpdateField");
			}
		}
	}

	public override string ToString() 
	{
		return(
				"Maximized = " + Maximized.ToString() + "\n" +
				"CustomButtons = " + CustomButtons.ToString() + "\n" +
				"UseVideo = " + UseVideo.ToString() + "\n" +
				"OnlyEncoderGravitatory = " + OnlyEncoderGravitatory.ToString() + "\n" +
				"OnlyEncoderInertial = " + OnlyEncoderInertial.ToString() + "\n" +
				"EncoderCaptureShowOnlyBars = " + EncoderCaptureShowOnlyBars.ToString() + "\n" +
				"EncoderUpdateTreeViewWhileCapturing = " + EncoderUpdateTreeViewWhileCapturing.ToString() + "\n" +
				"PersonWinHide = " + PersonWinHide.ToString() + "\n" +
				"EncoderAnalyzeHide = " + EncoderAnalyzeHide.ToString() + "\n" +
				"SessionMode = " + SessionMode.ToString() + "\n" +
				"Compujump = " + Compujump.ToString() + "\n" +
				"RunScriptOnExit = " + RunScriptOnExit.ToString() + "\n"
		      );
	}
	*/

	~Config() {}
}
