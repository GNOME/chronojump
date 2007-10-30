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
using System.Data;
using System.Text; //StringBuilder
using Mono.Unix;

public class Person {

	private int uniqueID;
	private string name;
	private string dateBorn;
	private int height;
	private int weight;
	//int level; // 0,1 o 2
	private string sex; // "M" (male) , "F" (female)
	private string description;

	private int sessionID;
	
	public Person() {
	}

	//suitable when we load a person from the database for being the current Person
	public Person(int uniqueID, string name, string sex, string dateBorn, int height, int weight, string description) 
	{
		this.uniqueID = uniqueID;
		this.sex = sex;
		this.name = name;
		this.dateBorn = dateBorn;
		this.height = height;
		this.weight = weight;
		this.description = description;
	}
	
	//typical constructor
	//public Person(string name, string sex, string dateBorn, string description, int sessionID) 
	public Person(string name, string sex, string dateBorn, int height, int weight, string description, int sessionID) 
	{
		this.name = name;
		this.sex = sex;
		this.dateBorn = dateBorn;
		this.height = height;
		this.weight = weight;
		this.description = description;
		this.sessionID = sessionID;

		name = Util.RemoveTildeAndColon(name);
		description = Util.RemoveTildeAndColon(description);
		
		//insert in the person table
		uniqueID = SqlitePerson.Insert (name, sex, dateBorn, height, weight, description);

		Console.WriteLine(this.ToString());

		//insert in the personSession table (fast way of knowing who was in each session)
		SqlitePersonSession.Insert (uniqueID, sessionID, weight);
	}
	
	public override string ToString()
	{
		return "[uniqueID: " + uniqueID + "]" + name + ", " + ", " + sex + ", " + dateBorn + ", " + description;
	}
	
	public override bool Equals(object evalString)
	{
		return this.ToString() == evalString.ToString();
	}
	
	public override int GetHashCode()
	{
		return this.ToString().GetHashCode();
	}
	
	public string Name
	{
		get
		{
			return name;
		}
		set
		{
			name = value;
		}
	}
	
	public string Sex
	{
		get
		{
			return sex;
		}
		set
		{
			sex = value;
		}
	}
	
	public string DateBorn
	{
		get { return dateBorn; }
		set { dateBorn = value; }
	}
	
	public string DateLong {
		get {
			return Util.DateAsDateTime(dateBorn).ToLongDateString();
		}
	}
	
	public string DateShort {
		get {
			return Util.DateAsDateTime(dateBorn).ToShortDateString();
		}
	}
	
	
	public int Height
	{
		get {
			return height;
		}
	}
	
	public int Weight
	{
		get {
			return weight;
		}
	}
	
	public string Description
	{
		get
		{
			return description;
		}
		set 
		{
			description = value;
		}
	}
	
	public int UniqueID
	{
		get
		{
			return uniqueID;
		}
		set 
		{
			uniqueID = value;
		}
	}
	
	~Person() {}
	   
}

