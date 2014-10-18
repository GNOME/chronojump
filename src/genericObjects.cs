using System;
using System.Collections.Generic; //List<T>

public class IDName
{
	private int uniqueID;
	private string name;

	public IDName(int uniqueID, string name) {
		this.uniqueID = uniqueID;
		this.name = name;
	}

	public int UniqueID {
	       get { return uniqueID; }
	}	       
	
	public string Name {
	       get { return name; }
	}
}

public class IDNameList
{
	private List<IDName> l;

	public IDNameList() {
	}
	
	//some string [], divided by ":" have first and second strings: uniqueID and name
	public IDNameList(string [] strArray, char sep) 
	{
		l = new List<IDName>();
		foreach(string strJump in strArray) {
			string [] strFull = strJump.Split(new char[] {sep});
			l.Add(new IDName(Convert.ToInt32(strFull[0]), strFull[1]));
		}
	}

	public int FindID(string name) {
		foreach(IDName i in l)
			if(i.Name == name)
				return i.UniqueID;
		return -1;
	}

	public string FindName(int uniqueID) {
		foreach(IDName i in l)
			if(i.UniqueID == uniqueID)
				return i.Name;
		return "";
	}
}
