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
 *  Copyright (C) 2018   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
using System.IO;
using System.IO.Ports;
using System.Collections.Generic; //List<T>

public class ForcePlatform
{
	SerialPort sp;
	bool saveToFile = true;
	TextWriter writer;

	public ForcePlatform()
	{
		LogB.Information("Experimental force platform stuff");
		string filename = Path.Combine(Path.GetTempPath(), "forceSensorReaded.csv");
		if(saveToFile)
			writer = File.CreateText(filename);

		if(openPort())
		{
			capture();
			closePort();
		}

		if(saveToFile)
		{
			writer.Flush();
			writer.Close();
			((IDisposable)writer).Dispose();
			LogB.Information("Saved to: " + filename);
		}
	}

	private bool openPort()
	{
		sp = new SerialPort("/dev/ttyUSB0");
		sp.BaudRate = 115200;
		LogB.Information("sp created");

		try {
			sp.Open();
		} catch {
			LogB.Information("Error: Cannot open port");
			return false;
		}
		return true;
	}

	private void closePort()
	{
		LogB.Information("end reading");
		sp.Close();
	}

	private void capture()
	{	
		LogB.Information("Serial port opened. Start reading");

		for(int i = 0; i < 100; i ++)
		{
			if(readRowMark())
				readForceValues();
			else
				LogB.Information("problem reading row mark");
		}
	}

	private bool readRowMark()
	{
		if(sp.ReadByte() != 255)
			return false;

		LogB.Debug("reading mark... 255,");
		for(int j = 0; j < 3; j ++)
			if(sp.ReadByte() != 255)
				return false;

		return true;
	}

	private void readForceValues()
	{
		LogB.Debug("readed start mark Ok");
		List<int> dataRow = new List<int>();
		for(int i = 0; i < 4; i ++)
		{
			int b0 = sp.ReadByte(); //least significative
			int b1 = sp.ReadByte(); //most significative
			dataRow.Add(256 * b1 + b0);
			//int readed = 256 * sp.ReadByte() + sp.ReadByte();
			//LogB.Information(sp.ReadByte().ToString() + "_" + sp.ReadByte().ToString());
		}
			
		printDataRow(dataRow);
	}

	private void printDataRow(List<int> dataRow)
	{
		string row = string.Format("{0}:{1}:{2}:{3}", dataRow[0], dataRow[1], dataRow[2], dataRow[3]);
		LogB.Information("DataRow: " + row);
		if(saveToFile)
			writer.WriteLine(row);
	}
}
