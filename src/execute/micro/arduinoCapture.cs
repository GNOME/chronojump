/*
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA 02111-1307, USA.
 *
 * Copyright (C) 2022-2024  Xavier de Blas <xaviblas@gmail.com>
 */

using System;
using System.Collections.Generic; //List<T>
using System.Diagnostics;  //Stopwatch
using System.IO.Ports;
using System.Threading;

public abstract class ArduinoCapture : MicroComms
{
	protected int readedPos; //position already readed from list

	// public stuff ---->

	public abstract bool CaptureStart();
	public abstract bool CaptureSample();
	public abstract bool Stop();
	public abstract bool CanReadFromList();

	public int ReadedPos
	{
		get { return readedPos; }
	}

	//if want to know how many (to read when have complete samples) use micro.BytesToReadAtLeast (int n)
	public bool BytesToRead ()
	{
		return micro.BytesToRead ();
	}

	public virtual string CaptureEchoLine() //for debugging if the controller returns some echo
	{
		return "";
	}

	public void Flush() //for debugging if the controller returns some echo
	{
		flush ();
	}

	// protected stuff ---->

	protected void initialize ()
	{
		readedPos = 0;
		micro.Response = "";

		emptyList();
	}

	protected bool readLine (out string str)
	{
		str = "";
		try {
			if (micro.BytesToRead ())
			{
				str = micro.ReadLine();
				LogB.Information(string.Format("at readLine BytesToRead>0, readed:|{0}|", str));
			}
		} catch (System.IO.IOException)
		{
			LogB.Information("Catched reading!");
			return false;
		}
		return true;
	}

	protected bool readByte (out int b)
	{
		b = 0;
		try {
			if (micro.BytesToRead ())
			{
				b = micro.ReadByte ();
				//LogB.Information(string.Format("at readLine BytesToRead>0, readed:|{0}|", str));
			}
		} catch (System.IO.IOException)
		{
			LogB.Information("Catched reading!");
			return false;
		}
		return true;
	}

	protected abstract void emptyList();

	public void Disconnect()
	{
		micro.ClosePort ();
	}

	public bool PortOpened
	{
		get { return micro.Opened; }
	}
}

