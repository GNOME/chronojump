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
 * Copyright (C) 2004-2016   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
using System.IO;


public class Config
{
	public enum AutodetectPortEnum { ACTIVE, DISCARDFIRST, INACTIVE }
	public enum SessionModeEnum { STANDARD, UNIQUE }

	public bool Maximized;
	public bool CustomButtons;
	public bool UseVideo;
	public AutodetectPortEnum AutodetectPort;
	public bool OnlyEncoder;
	public bool EncoderCaptureShowOnlyBars;
	public bool EncoderUpdateTreeViewWhileCapturing; //recomended: false. Make it false if EncoderCaptureShowOnlyBars == true
							//because treeview will be in 2n page of notebook
	public bool PersonWinHide;
	public bool EncoderAnalyzeHide;
	public EncoderConfiguration Econf;
	public SessionModeEnum SessionMode;
	public string RunScriptOnExit;

	public Config()
	{
		Maximized = false;
		CustomButtons = false;
		UseVideo = true;
		
		//currently disabled AutodetectPort by default on MACOSX
		if(UtilAll.GetOSEnum() == UtilAll.OperatingSystems.MACOSX)
			AutodetectPort = AutodetectPortEnum.INACTIVE;
		else
			AutodetectPort = AutodetectPortEnum.ACTIVE;

		OnlyEncoder = false;
		EncoderCaptureShowOnlyBars = false;
		EncoderUpdateTreeViewWhileCapturing = true;
		PersonWinHide = false;
		EncoderAnalyzeHide = false;
		Econf = null; 
		SessionMode = SessionModeEnum.STANDARD;
		RunScriptOnExit = "";
	}

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
					else if(parts[0] == "AutodetectPort" && Enum.IsDefined(typeof(AutodetectPortEnum), parts[1]))
						AutodetectPort = (AutodetectPortEnum) 
							Enum.Parse(typeof(AutodetectPortEnum), parts[1]);
					else if(parts[0] == "OnlyEncoder" && Util.StringToBool(parts[1]))
						OnlyEncoder = true;
					else if(parts[0] == "EncoderCaptureShowOnlyBars" && Util.StringToBool(parts[1]))
						EncoderCaptureShowOnlyBars = true;
					else if(parts[0] == "EncoderUpdateTreeViewWhileCapturing" && ! Util.StringToBool(parts[1]))
						EncoderUpdateTreeViewWhileCapturing = false;
					else if(parts[0] == "PersonWinHide" && Util.StringToBool(parts[1]))
						PersonWinHide = true;
					else if(parts[0] == "EncoderAnalyzeHide" && Util.StringToBool(parts[1]))
						EncoderAnalyzeHide = true;
					else if(parts[0] == "EncoderConfiguration")
					{
						string [] ecFull = parts[1].Split(new char[] {':'});
						if(Enum.IsDefined(typeof(Constants.EncoderConfigurationNames), ecFull[0])) 
						{ 
							//create object
							Econf = new EncoderConfiguration(
									(Constants.EncoderConfigurationNames) 
									Enum.Parse(typeof(Constants.EncoderConfigurationNames), ecFull[0]) );
							//assign the rest of params
							Econf.ReadParamsFromSQL(ecFull);
						}
					}
					else if(parts[0] == "SessionMode" && Enum.IsDefined(typeof(SessionModeEnum), parts[1]))
						SessionMode = (SessionModeEnum) 
							Enum.Parse(typeof(SessionModeEnum), parts[1]);
					else if(parts[0] == "RunScriptOnExit" && parts[1] != "")
						RunScriptOnExit = parts[1];
				} while(true);
			}
		}
	}
	
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
		string econfStr = "";
		if(Econf != null)
			econfStr = Econf.ToStringPretty();

		return(
				"Maximized = " + Maximized.ToString() + "\n" +
				"CustomButtons = " + CustomButtons.ToString() + "\n" +
				"UseVideo = " + UseVideo.ToString() + "\n" +
				"AutodetectPort = " + AutodetectPort.ToString() + "\n" +
				"OnlyEncoder = " + OnlyEncoder.ToString() + "\n" +
				"EncoderCaptureShowOnlyBars = " + EncoderCaptureShowOnlyBars.ToString() + "\n" +
				"EncoderUpdateTreeViewWhileCapturing = " + EncoderUpdateTreeViewWhileCapturing.ToString() + "\n" +
				"PersonWinHide = " + PersonWinHide.ToString() + "\n" +
				"EncoderAnalyzeHide = " + EncoderAnalyzeHide.ToString() + "\n" +
				"Econf = " + econfStr + "\n" +
				"SessionMode = " + SessionMode.ToString() + "\n" +
				"RunScriptOnExit = " + RunScriptOnExit.ToString() + "\n"
		      );
	}

	~Config() {}
}
