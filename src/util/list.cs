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
 *  Copyright (C) 2004-2025   Xavier de Blas <xaviblas@gmail.com>
 */

using System;
using System.Collections.Generic; //List<T>

public class UtilList
{
	public static string ListIntToSQLString (List<int> ints, string sep)
	{
		string str = "";
		string sepStr = "";
		foreach(int i in ints)
		{
			str += sepStr + i.ToString();
			sepStr = sep;
		}
		return str;
	}

	public static List<int> SQLStringToListInt (string sqlString, string sep)
	{
		List<int> l = new List<int>();
		string [] strFull = sqlString.Split(sep.ToCharArray());
		foreach(string str in strFull)
			if(Util.IsNumber(str, false))
				l.Add(Convert.ToInt32(str));

		return l;
	}

	public static List<double> SQLStringToListDouble (string sqlString, string sep)
	{
		List<double> l = new List<double>();
		string [] strFull = sqlString.Split(sep.ToCharArray());
		foreach(string str in strFull)
			if(Util.IsNumber(str, true))
				l.Add(Convert.ToDouble(str));

		return l;
	}

	//an int solution could is: ListIntToSQLString ()
	public static string ListDoubleToString (List<double> d_l, int decs, string sep)
	{
		string str = "";
		string sepStr = "";
		foreach (double d in d_l)
		{
			str += sepStr + Util.TrimDecimals (d, decs);
			sepStr = sep;
		}
		return str;
	}

	public static void ListIntToFile (List<int> l, string sep, string filename)
	{
		try {
			TextWriter writer = File.CreateText (filename);
			writer.Write (ListIntToSQLString (l, sep));
			writer.Flush();
			writer.Close();
			((IDisposable)writer).Dispose();
		} catch {
			LogB.Information ("catched at ListIntToFile to file: " + filename);
		}
	}

	public static void ListDoubleToFile (List<double> l, int decs, string sep, string filename)
	{
		try {
			TextWriter writer = File.CreateText (filename);
			writer.Write (ListDoubleToString (l, decs, sep));
			writer.Flush();
			writer.Close();
			((IDisposable)writer).Dispose();
		} catch {
			LogB.Information ("catched at ListDoubleToFile to file: " + filename);
		}
	}

	public static double Sum (List<double> l)
	{
		double sum = 0;
		foreach (double d in l)
			sum += d;

		return sum;
	}

	public static List<int> Cumsum (List<int> original_l)
	{
		List<int> accu_l = new List<int> ();
		int sum = 0;
		foreach (int i in original_l)
		{
			sum += i;
			accu_l.Add (sum);
		}

		return accu_l;
	}
	//to pass a list like: (.5, .7, .2) to: (.5, 1.2, 1.4)
	public static List<double> Cumsum (List<double> original_l)
	{
		List<double> accu_l = new List<double> ();
		double sum = 0;
		foreach (double d in original_l)
		{
			sum += d;
			accu_l.Add (sum);
		}

		return accu_l;
	}
	public static void CumsumTest ()
	{
		List<double> l = new List<double> { 7, 5, 3.2, 4};

		LogB.Information (string.Format ("CumsumTest from: {0} to: {1}",
				UtilList.ListDoubleToString (l, 2, "; "),
				UtilList.ListDoubleToString (Cumsum (l), 2, "; ") ));
	}

	public static string ListStringToString (List<string> l)
	{
		string str = "";
		string sep = "";
		if(l == null)
			return str;

		foreach (string s in l)
		{
			str += sep + s;
			sep = "\n";
		}

		return str;
	}

	public static string ListStringToString (List<string> l, string separator)
	{
		if (l == null || l.Count == 0)
			return "";

		string str = "";
		string sepDo = "";

		foreach (string s in l)
		{
			str += sepDo + s;
			sepDo = separator;
		}

		return str;
	}

	public static List<int> AddToListIntIfNotExist (List<int> l, int i) {
		bool found = FoundInListInt(l, i);
		if(! found)
			l.Add(i);

		return l;
	}

	public static List<double> AddToListDoubleIfNotExist(List<double> l, double d) {
	 	bool found = FoundInListDouble(l, d);
		if(!found)
			l.Add(d);
	
		return l;
	}

	public static bool FoundInListInt(List<int> l, int i) {
		foreach (int i2 in l)
			if(i2 == i)
				return true;

		return false;
	}
	public static bool FoundInListDouble(List<double> l, double d) {
		foreach (double d2 in l)
			if(d2 == d)
				return true;

		return false;
	}
	public static bool FoundInListString (List<string> l, string s)
	{
		foreach (string l2 in l)
			if(l2 == s)
				return true;

		return false;
	}
	public static bool StartsWithInListString (List<string> l, string s)
	{
		foreach (string l2 in l)
			if(l2.StartsWith (s))
				return true;

		return false;
	}

	// https://stackoverflow.com/a/273666
	public static List<T> ListRandomize<T>(List<T> list)
	{
		List<T> randomizedList = new List<T>();
		Random rnd = new Random();
		while (list.Count > 0)
		{
			int index = rnd.Next(0, list.Count); //pick a random item from the master list
			randomizedList.Add(list[index]); //place it at the end of the randomized list
			list.RemoveAt(index);
		}
		return randomizedList;
	}

	public static List<T> ListRandomize1stAndThenSequential<T>(List<T> list)
	{
		List<T> randomizedList = new List<T>();
		Random rnd = new Random();
		int firstOne = rnd.Next(0, list.Count); //pick a random item from the master list

		for (int i = firstOne; i < list.Count; i ++)
			randomizedList.Add (list[i]);

		for (int i = 0; i < firstOne; i ++)
			randomizedList.Add (list[i]);

		return randomizedList;
	}
	public static void ListRandomize1stAndThenSequentialTest ()
	{
		List<string> a_l = new List<string> { "aaa", "bbb", "ccc", "ddd", "eee" };
		for (int i = 0; i < 20; i ++)
		{
			LogB.Information ("UtilList.ListRandomize1stAndThenSequential:");
			foreach (string str in ListRandomize1stAndThenSequential (a_l))
				LogB.Information (str);
		}
	}

	public static List<T> ListGetFirstN<T> (List<T> original_l, int firstN)
	{
		List<T> cutted_l = new List<T>();
		for (int i = 0; i < firstN; i ++)
			cutted_l.Add (original_l[i]);

		return cutted_l;
	}

	public static List<T> ListGetFromToIncluded<T> (List<T> original_l, int iFrom, int iToIncluded)
	{
		List<T> cutted_l = new List<T>();
		for (int i = iFrom; i <= iToIncluded && i < original_l.Count; i ++)
			cutted_l.Add (original_l[i]);

		return cutted_l;
	}

	//sort a list of ints descending until 0
	public static List<int> SortListInt (List<int> unsorted_l)
	{
		if(unsorted_l.Count < 2)
			return unsorted_l;

		List<int> sorted_l = new List<int>();
		for (int i = 0; i < unsorted_l.Count; i ++)
		{
			int highest = 0;
			int highestPos = 0;
			for(int j = 0; j < unsorted_l.Count; j ++)
			{
				int comparingToNum = unsorted_l[j];
				if(comparingToNum > highest)
				{
					highest = comparingToNum;
					highestPos = j;
				}
			}
			sorted_l.Add (unsorted_l[highestPos]); //store highest value on sorted_l
			unsorted_l[highestPos] = 0; //mark as 0 on unsorted_l to not choose it again
		}
		return sorted_l;
	}

	public static void TestSortDoublesListstring()
	{
		List<string> numbers_l = new List<string>() { "3", "99", "135", "45", "75", "17", "88", "5" }; //ints
		//List<string> numbers_l = new List<string>() { "3,5", "99,54", "135,1", "45,5", "75,5", "45,9", "88", "45,3" }; //latin

		List<string> numbersSorted_l = SortDoublesListString(numbers_l);

		LogB.Information("Sorted: ");
		foreach(string s in numbersSorted_l)
			LogB.Information(s);
	}

	//sort a list of doubles descending until 0
	public static List<string> SortDoublesListString (List<string> unsorted_l)
	{
		if(unsorted_l.Count < 2)
			return unsorted_l;

		List<string> sorted_l = new List<string>();
		for(int i = 0; i < unsorted_l.Count; i ++)
		{
			double highest = 0;
			int highestPos = 0;
			for(int j = 0; j < unsorted_l.Count; j ++)
			{
				double comparingToNum = Convert.ToDouble(unsorted_l[j]);
				if(comparingToNum > highest)
				{
					highest = comparingToNum;
					highestPos = j;
				}
			}
			sorted_l.Add(unsorted_l[highestPos]); //store highest value on sorted_l
			unsorted_l[highestPos] = "0"; //mark as 0 on unsorted_l to not choose it again
		}
		return sorted_l;
	}

	public static List<int> IntArrayToListInt (int [] arrayInt)
	{
		List<int> l = new List<int> ();
		foreach (int i in arrayInt)
			l.Add (i);

		return l;
	}

	public static List<int> DoubleArrayToListInt (double [] arrayDouble)
	{
		List<int> l = new List<int> ();
		foreach (double d in arrayDouble)
			l.Add (Convert.ToInt32 (d));
	
		return l;
	}

	public static List<string> StringArrayToListString (string [] arrayString)
	{
		List<string> str_l = new List<string> ();
		foreach (string myStr in arrayString)
			str_l.Add (myStr);
	
		return str_l;
	}
	
	public static string [] ListStringToStringArray (List<string> string_l)
	{
		string [] stringArray = new string[string_l.Count];
		for(int i = 0; i < string_l.Count; i ++)
			stringArray[i] = string_l[i];

		return stringArray;
	}

	public static int FindOnListString (List<string> str_l, string searched)
	{
		for (int i = 0; i < str_l.Count; i ++)
			if(str_l[i] == searched)
				return i;

		return 0;
	}

	public static double GetMax (List<double> values_l)
	{
		double max = 0;
		bool firstValue = true;
		foreach (double d in values_l)
		{
			if (firstValue || d > max)
				max = d;

			firstValue = false;
		}
		return max;
	}
	public static double GetMaxValueAndPos (List<double> values_l, ref int pos)
	{
		double max = 0;
		pos = 0;
		bool firstValue = true;
		for (int i = 0 ; i < values_l.Count ; i ++)
		{
			if (firstValue || values_l[i] > max)
			{
				pos = i;
				max = values_l[pos];
			}

			firstValue = false;
		}
		return max;
	}
	//returns a list of the pos where max value is found
	public static double GetMaxValueAndPos (List<double> values_l, ref List<int> pos)
	{
		// 1. found max
		double max = GetMax (values_l);

		// 2. create a list of pos where pos == max
		pos = new List<int> ();
		for (int i = 0 ; i < values_l.Count ; i ++)
			if (values_l[i] == max)
				pos.Add (i);

		return max;
	}

	public static double GetMin (List<double> values)
	{
		double min = 0;
		bool firstValue = true;
		foreach (double d in values)
		{
			if (firstValue || d < min)
				min = d;

			firstValue = false;
		}
		return min;
	}
	public static double GetMinValueAndPos (List<double> values_l, ref int pos)
	{
		double min = 0;
		pos = 0;
		bool firstValue = true;
		for (int i = 0 ; i < values_l.Count ; i ++)
		{
			if (firstValue || values_l[i] < min)
			{
				pos = i;
				min = values_l[pos];
			}

			firstValue = false;
		}
		return min;
	}
	//returns a list of the pos where min value is found
	public static double GetMinValueAndPos (List<double> values_l, ref List<int> pos)
	{
		// 1. found min
		double min = GetMin (values_l);

		// 2. create a list of pos where pos == min
		pos = new List<int> ();
		for (int i = 0 ; i < values_l.Count ; i ++)
			if (values_l[i] == min)
				pos.Add (i);

		return min;
	}

	public static double GetAverage (List<double> values)
	{
		double sum = 0;
		foreach(double d in values)
			sum += d;

		return UtilAll.DivideSafe(sum, values.Count);
	}
	public static double GetAverage (List<int> values)
	{
		double sum = 0;
		foreach(int i in values)
			sum += i;

		return UtilAll.DivideSafe(sum, values.Count);
	}

	public static double GetLast (List<double> l)
	{
		if (l.Count == 0)
			return 0;

		return l[l.Count -1];
	}
	public static string GetLast (List<string> l)
	{
		if (l.Count == 0)
			return "";

		return l[l.Count -1];
	}
	public static string [] GetLast (List<string []> l)
	{
		if (l.Count == 0)
			return new string [] {};

		return l[l.Count -1];
	}

	//if belowOrEqual is true acts like <= i false acts like <
	public static List<int> GetPosListBelowAValue (List<double> values_l, double compareTo, bool belowOrEqual)
	{
		List<int> pos_l = new List<int> ();
		for (int i = 0 ; i < values_l.Count ; i ++)
		{
			if (belowOrEqual && values_l[i] <= compareTo)
				pos_l.Add (i);
			else if (! belowOrEqual && values_l[i] < compareTo)
				pos_l.Add (i);
		}

		return pos_l;
	}

	public static List<double> ListDoubleToAbs (List<double> dOrig_l)
	{
		List<double> dAbs_l = new List<double> ();
		foreach (double dOrig in dOrig_l)
			dAbs_l.Add (Math.Abs (dOrig));

		return dAbs_l;
	}

	public static List<double> ListReverse (List<double> l)
	{
		List<double> rev_l = new List<double> ();
		for (int i = l.Count -1; i >= 0; i --)
			rev_l.Add (l[i]);

		return rev_l;
	}

	public static List<int> ListReverseSign (List<int> values)
	{
		List<int> reversed_l = new List<int> ();
		foreach(int i in values)
			reversed_l.Add (-1 * i);

		return reversed_l;
	}
	public static List<double> ListReverseSign (List<double> values)
	{
		List<double> reversed_l = new List<double> ();
		foreach(int i in values)
			reversed_l.Add (-1 * i);

		return reversed_l;
	}
}

