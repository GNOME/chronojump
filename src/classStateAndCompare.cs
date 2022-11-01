using System;
using System.Reflection;
using System.Collections;

/*
 * This file is part of ChronoJump
 *
 * ChronoJump is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or   
 * (at your option) any later version.
 *    
 * ChronoJump is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the 
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 *
 * Copyright (C) 2016-2017   Carles Pina i Estany <carles@pina.cat>
 */


// This class reads and writes static class states. Reads all the attributes and keeps them,
// then the static class can be changed but the original attributes can be re-written again
// to go back to the previous state.
class StaticClassState
{
	private Hashtable attributes;
	private Type classType;

	public StaticClassState(Type classType)
	{
		// call new StaticClassState(typeof (class));
		this.classType = classType;
	}

	// Reads all the static attributes of this class and saves them into a dictionary (to be used in the writeAttributes)
	public void readAttributes()
	{
		attributes = new Hashtable ();

		foreach (var field in classType.GetFields(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic))
		{
			string name = field.Name;
			var value = field.GetValue(null); // static classes cannot be instanced, so use null...
			attributes.Add (name, value);
		}
	}

	// Writes the read attributes back into the class. See readAttributes()
	public void writeAttributes()
	{
		writeAttributesDo (this);
	}

	// Writes the attributes from staticClassState parameter into the existing class (defined in the constructor)
	public void writeAttributes(StaticClassState staticClassState)
	{
		writeAttributesDo (staticClassState);
	}

	private void writeAttributesDo(StaticClassState staticClassState)
	{
		foreach (var field in classType.GetFields(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic))
		{
			string name = field.Name;
			field.SetValue (name, attributes [name]);
		}
	}
}
