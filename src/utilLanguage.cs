
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
 *  Copyright (C) 2004-2017   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
using System.IO;
using System.Collections.Generic;
using System.Globalization;


public class UtilLanguage 
{
	//from Longomatch ;)
	//(C) Andoni Morales Alastruey
	public static List<CultureInfo> Languages {
		get { 
			List<CultureInfo> langs;
			string filename, localesDir;

			langs = new List<CultureInfo>();
			filename = String.Format ("{0}.mo", "chronojump");
			localesDir = System.IO.Path.Combine(Util.GetPrefixDir(),"share/locale");

			langs.Add (new CultureInfo ("en"));

			if (!Directory.Exists (localesDir))
				return langs;

			foreach (string dirpath in Directory.EnumerateDirectories (localesDir)) {
				if (File.Exists (Path.Combine (dirpath, "LC_MESSAGES", filename))) {
					try {
						string localeName = Path.GetFileName(dirpath).Replace("_", "-");
						//pt_BR will be pt-BR. This is needed to have them on the list at gui/preferences

						langs.Add(new CultureInfo (localeName));
					} catch (Exception ex) {
						LogB.Warning (ex.ToString());
					}
				} 
			}

			langs.Sort((p1, p2) => string.Compare(p1.NativeName, p2.NativeName, false));
			foreach(CultureInfo myLang in langs)
				LogB.Debug(myLang.NativeName);
			
			return langs;
		}
	}
}
