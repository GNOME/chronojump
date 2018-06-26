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
 * Xavier de Blas: 
 */

/*
   this will be on server run every 20' to see of server if xsp2 is up
   (not only if the process is ok, we want to know if it really works)
   print also data to a file to know what's happening with xsp2
   and kill...wait...execute xsp2 if died
   */

using System;
using System.IO; //"File" things
using System.Diagnostics; //Process
using System.Text; //StringBuilder

public class ChronoJumpServerAlive 
{
	public static void Main(string [] args) 
	{
		TextWriter writer;
		writer = File.AppendText("log.txt");
		string result = "";

		try {
			ChronojumpServerAlive myServer = new ChronojumpServerAlive();
			result = myServer.PingAlive(DateParse(DateTime.Now.ToString()));
		}
		catch {
			result = "--- restarting " + DateParse(DateTime.Now.ToString()) + " ---";

			string xsp2ReloadMark = "xsp2_need_to_reload";
			if(!File.Exists(xsp2ReloadMark))
					File.CreateText(xsp2ReloadMark);
		}

		writer.WriteLine(result);
                writer.Flush();
		writer.Close();
	}
	
	private static string DateParse(string myDate) {
		StringBuilder myStringBuilder = new StringBuilder(myDate);
		//for not having problems with the directories:
		myStringBuilder.Replace(" ", "_"); //replace the ' ' (date-hour separator) for '_' 
		myStringBuilder.Replace("/", "-"); //replace the '/' (date separator) for '-' 
		myStringBuilder.Replace(":", "-"); //replace the ':' (hour separator) for '-'
		myStringBuilder.Replace(".", ""); //delete the '.' in a.m.: 13-01-2009_02-05-43_a.m.

		return myStringBuilder.ToString();
	}
}
