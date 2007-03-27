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
 * Xavier de Blas: 
 * http://www.xdeblas.com, http://www.deporteyciencia.com (parleblas)
 */

using System;
using Mono.Unix;

public class Constants
{
	//the strings created by Catalog cannot be const
	
	//formulas
	public static string DjIndexFormula = "Dj Index ((tv-tc)/tc *100)";
	public static string QIndexFormula = "Q index (tv/tc)";
	public const string FvIndexFormula = "F/V sj+(100%)/sj *100";
	public const string IeIndexFormula = "IE (cmj-sj)/sj *100";
	public const string IubIndexFormula = "IUB (abk-cmj)/cmj *100";
	public static string CmjPlusPotencyFormula = Catalog.GetString("Potency") + 
		" (Pc + Pb) * 9.81 * sqrt(2 * 9,81 * h)";
	
	//strings
	public static string AllJumpsName = Catalog.GetString("All jumps");
	public static string AllRunsName = Catalog.GetString("All runs");
	public static string AllPulsesName = Catalog.GetString("All pulses");
	
	//30 colors defined
	public static string [] Colors = {
		"Blue", "Coral", "Cyan", "Gray", "Green", "Pink", "Salmon", "Yellow",
		"DarkBlue", "DarkCyan", "DarkGoldenrod", "DarkGray", 
		"DarkGreen", "DarkMagenta", "DarkSalmon",
		"LightBlue", "LightCoral", "LightCyan", "LightGoldenrodYellow", 
		"LightGray", "LightGreen", "LightPink", "LightSalmon", "LightYellow", 
		"MediumBlue", "MediumOrchid", "MediumPurple", "MediumTurquoise", "MediumVioletRed", "YellowGreen" 
	};
	
	//for windows, on linux it takes language ok from the locale
	public static string LanguageDefault = "en-GB:English (United Kingdom)";
	public static string [] Languages = {
		"ca-ES:Catalan", 
		"zh-CN:Chinese", 
		LanguageDefault, 
		//"dz-BT:Dzongkha", Sorry, Dzongkha does not work on Windows
		"fi-FI:Finnish", 
		"fr-FR:French", 
		"pt-BR:Portuguese (Brazil)", 
		"es-ES:Spanish (Spain)", 
		"sv-SE:Swedish", 
		"vi-VN:Vietnamese", 
	};
	//TODO: add:
	//ar (when there ara not fuzzy lines)
	//see in both langs how to write as xx_XX
	

	public static string PortNamesWindows = 
		string.Format(Catalog.GetString("Typical serial and USB-serial ports on Windows:") + 
				"\n\tCOM1\n\tCOM2\n\n" + 
				Catalog.GetString("Also, these are possible:") + 
				"\n\tCOM3 ... COM8");
	
	public static string PortNamesLinux = 
		string.Format(Catalog.GetString("Typical serial serial ports on GNU/Linux:") + 
				"\n\t/dev/ttyS0\n\t/dev/ttyS1\n\n" +
				Catalog.GetString("Typical USB-serial ports on GNU/Linux:") +
				"\n\t/dev/ttyUSB0\n\t/dev/ttyUSB1");

//	public static System.Media.SystemSound SoundCanStart = System.Media.SystemSounds.Question; 
//	public static System.Media.SystemSounds SoundGood = System.Media.SystemSounds.Exclamation; 
//	public static System.Media.SystemSounds SoundBad = System.Media.SystemSounds.Beep; 
	public enum SoundTypes {
		CAN_START, GOOD, BAD
	}
	//public soundTypes SoundType;
}
