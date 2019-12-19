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
 * Copyright (C) 2019   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;

public class Constants
{
	public static string EncoderDataTemp = "chronojump-last-encoder-data.txt";
	public static string EncoderScriptCallGraph = "call_graph.R";
	public static string EncoderStatusTempBase = "chronojump-encoder-status-";
	public static string EncoderSpecialDataTemp = "chronojump-special-data.txt"; //variable;result (eg. "1RM;82.78")
	public static string EncoderCurvesTemp = "chronojump-last-encoder-curves.txt";
	//public static string EncoderAnalyzeTableTemp = "chronojump-last-encoder-analyze-table.txt";
	public static string EncoderGraphTemp = "chronojump-last-encoder-graph.png";

	public static string FileNotFoundStr()
        {
                return "Error. File not found.";
        }

	public static string FileCopyProblemStr()
	{
		return "Error. Cannot copy file.";
	}
}

