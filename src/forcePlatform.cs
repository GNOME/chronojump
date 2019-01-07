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
		LogB.Information("Serial port opened. Send capture message");

		System.Threading.Thread.Sleep(3000); //sleep to let arduino start reading serial event
		sp.WriteLine("start_capture:");
		LogB.Information("'start_capture:' sent");

		string str = "";
		do {
			System.Threading.Thread.Sleep(100); //sleep to let arduino start reading
			try {
				str = sp.ReadLine();
			} catch {
				LogB.Warning("Catched at capture");
				return;
			}
			LogB.Information("init string: " + str);
		}
		while(! str.Contains("Starting capture"));

		writer.WriteLine("Time;S1;S2;S3;S4");
		for(int i = 0; i < 400; i ++)
		{
			if(readRowMark())
				readForceValues();
			else
				LogB.Information("problem reading row mark");

			/*
			int b = sp.ReadByte();
			LogB.Information(b.ToString());
			writer.WriteLine(b.ToString());
			*/
		}
		LogB.Information("capture ended");
		sp.WriteLine("end_capture:");
		LogB.Information("'end_capture:' sent");
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

		//read time, four bytes
		int t0 = sp.ReadByte(); //least significative
		int t1 = sp.ReadByte(); //most significative
		int t2 = sp.ReadByte(); //most significative
		int t3 = sp.ReadByte(); //most significative
		dataRow.Add(Convert.ToInt32(
				Math.Pow(256,3) * t3 +
				Math.Pow(256,2) * t2 +
				Math.Pow(256,1) * t1 +
				Math.Pow(256,0) * t0));

		//read data, four sensors, 2 bytes each
		for(int i = 0; i < 4; i ++)
		{
			int b0 = sp.ReadByte(); //least significative
			int b1 = sp.ReadByte(); //most significative

			int readedNum = 256 * b1 + b0;
			//care for negative values
			if(readedNum > 32768)
				readedNum = -1 * (65536 - readedNum);

			dataRow.Add(readedNum);
		}
			
		printDataRow(dataRow);
	}

	private void printDataRow(List<int> dataRow)
	{
		string row = string.Format("{0};{1};{2};{3};{4}", dataRow[0], dataRow[1], dataRow[2], dataRow[3], dataRow[4]);
		LogB.Information("DataRow: " + row);
		if(saveToFile)
			writer.WriteLine(row);
	}
}
