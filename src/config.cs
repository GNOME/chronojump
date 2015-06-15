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
 * Copyright (C) 2004-2015   Xavier de Blas <xaviblas@gmail.com> 
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
	public SessionModeEnum SessionMode;
	public string RunScriptOnExit;

	public Config()
	{
		Maximized = false;
		CustomButtons = false;
		UseVideo = true;
		AutodetectPort = AutodetectPortEnum.ACTIVE;
		OnlyEncoder = false;
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
					else if(parts[0] == "SessionMode" && Enum.IsDefined(typeof(SessionModeEnum), parts[1]))
						SessionMode = (SessionModeEnum) 
							Enum.Parse(typeof(SessionModeEnum), parts[1]);
					else if(parts[0] == "RunScriptOnExit" && parts[1] != "")
						RunScriptOnExit = parts[1];
				} while(true);
			}
		}
	}

	public override string ToString() {
		return(
				"Maximized = " + Maximized.ToString() + "\n" +
				"CustomButtons = " + CustomButtons.ToString() + "\n" +
				"UseVideo = " + UseVideo.ToString() + "\n" +
				"AutodetectPort = " + AutodetectPort.ToString() + "\n" +
				"OnlyEncoder = " + OnlyEncoder.ToString() + "\n" +
				"SessionMode = " + SessionMode.ToString() + "\n" +
				"RunScriptOnExit = " + RunScriptOnExit.ToString() + "\n"
		      );
	}

	~Config() {}
}
