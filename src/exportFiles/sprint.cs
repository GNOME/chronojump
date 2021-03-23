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
 * Copyright (C) 2021   Xavier de Blas <xaviblas@gmail.com>
 */

using System;
using System.IO; 			//Directory, ...
using System.Collections; 		//ArrayList
using System.Collections.Generic; 	//List<T>
using System.Threading;
//using Mono.Unix;

public class SprintExport : ExportFiles
{
	private List<RunInterval> ri_l;

	//constructor
	public SprintExport (
			Gtk.Notebook notebook,
			Gtk.ProgressBar progressbar,
			Gtk.Label labelResult,
			bool includeImages,
			int imageWidth, int imageHeight,
			bool isWindows,
			int personID,
			int sessionID,
			char exportDecimalSeparator
			)
	{
		Button_done = new Gtk.Button();

		assignParams(notebook, progressbar, labelResult, includeImages,
				imageWidth, imageHeight, isWindows, personID, sessionID);

		this.exportDecimalSeparator = exportDecimalSeparator;
	}

	private string getTempGraphsDir() {
		return Path.Combine(Path.GetTempPath(), "chronojump_sprint_export_graphs");
	}
	
	protected override void createOrEmptyDirs()
	{
		createOrEmptyDir(getTempProgressDir());
		createOrEmptyDir(getTempGraphsDir());
	}

	protected override bool getData ()
	{
		ri_l = SqliteRunInterval.SelectRuns (false, sessionID, personID, "");

	}
}

