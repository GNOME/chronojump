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
 *  Copyright (C) 2025   Xavier de Blas <xaviblas@gmail.com>
 */

using System;
using System.Collections.Generic; //List

public class QuestionAnswerArithmetic
{
	protected Random rnd;
	protected int level;
	protected string difficultyName;

	// just for inheritance
	public QuestionAnswerArithmetic ()
	{
	}

	public QuestionAnswerArithmetic (int level)
	{
		this.level = level;
		rnd = new Random();
	}

	// just to test this class (without generating random equations)
	public static void Test ()
	{
		LogB.Information ("Test QuestionAnswerArithmetic:");
		QuestionAnswerArithmetic qaa = new QuestionAnswerArithmetic (0); //level is not important here as we generate the qaa below

		List<string []> sa_l = new List<string []>();
		sa_l.Add (new string [] { "2", "+", "7" });
		sa_l.Add (new string [] { "2", "-", "7" });
		sa_l.Add (new string [] { "2", "*", "7" });
		sa_l.Add (new string [] { "1", "+", "5", "-", "2" });
		sa_l.Add (new string [] { "21", "+", "(", "52", "*", "34", ")" });
		sa_l.Add (new string [] { "(", "21", "+", "52", ")", "*", "34", });

		foreach (string [] sa in sa_l)
		{
			LogB.Information (string.Format ("\nExpression is: {0}", Util.StringArrayToString (sa, " ")));
			LogB.Information (string.Format ("final result is: {0}", qaa.calculateStr (sa)));
		}
	}

	public virtual QuestionAnswers GenerateQA ()
	{
		switch (level)
		{
			case 0:
				return generateQAEasy ();
			case 1:
				return generateQAMid ();
			case 2:
				return generateQAHard ();
			default:
				return generateQAEasy ();
		}
	}

	private QuestionAnswers generateQAEasy ()
	{
		difficultyName = "Easy";

		int a = rnd.Next (0, 9);
		string op = generateRandomOperator ();
		int b = rnd.Next (0, 9);

		string [] sa = new string [3] { a.ToString (),  op, b.ToString () };
		int result = calculateStr (sa);

		return new QuestionAnswers (
				"    " + Util.StringArrayToString (sa, " "),
				result.ToString (),
				(result + rnd.Next (1, 9)).ToString (),
				(result - rnd.Next (1, 9)).ToString (),
				(result * rnd.Next (2, 4)).ToString ()
				);
	}

	private QuestionAnswers generateQAMid ()
	{
		difficultyName = "Mid";

		int a, b, c;
		a = rnd.Next (1, 9);
		string op1 = generateRandomOperator ();
		b = rnd.Next (1, 9);
		string op2 = generateRandomOperator ();
		c = rnd.Next (1, 9);

		return generateQAMaybeWithParentheses (a.ToString (), op1, b.ToString (), op2, c.ToString ());
	}

	// hard has the operands 1-99 except if preceeding operator is an *
	private QuestionAnswers generateQAHard ()
	{
		difficultyName = "Hard";

		int a, b, c;
		int max = 99;

		a = rnd.Next (0, max);
		string op1 = generateRandomOperator ();

		if (op1 == "*")
			b = rnd.Next (1, 9);
		else
			b = rnd.Next (1, max);

		string op2 = generateRandomOperator ();

		if (op2 == "*")
			c = rnd.Next (1, 9);
		else
			c = rnd.Next (1, max);

		return generateQAMaybeWithParentheses (a.ToString (), op1, b.ToString (), op2, c.ToString ());
	}

	protected QuestionAnswers generateQAMaybeWithParentheses (string a, string op1, string b, string op2, string c)
	{
		string [] sa;

		if ( (op1 == "*" || op2 == "*") && op1 != op2 ) //we need parentheses
		{
			if (rnd.Next (2) == 0) // 50 % change of having it at start
				sa = new string [7] { "(", a, op1, b, ")", op2, c };
			else
				sa = new string [7] { a, op1, "(", b, op2, c, ")" };
		} else
				sa = new string [5] { a, op1, b, op2, c };

		//LogB.Information ("send: " + Util.StringArrayToString (sa, " "));
		int result = calculateStr (convertGLToIntsIfNeeded (sa));
		//LogB.Information ("result: " + result.ToString ());

		// add anaswers in a list to check we do not have duplicates
		// eg if the correct answer is 4 and one of the fake answers is 4+4=8 and the other is 4*2=8
		List<int> answer_l = new List<int> ();
		answer_l.Add (result);
		int bad1, bad2, bad3;

		do {
			bad1 = result + rnd.Next (1, 9);
		} while (UtilList.FoundInListInt (answer_l, bad1));
		answer_l.Add (bad1);

		do {
			bad2 = result - rnd.Next (1, 9);
		} while (UtilList.FoundInListInt (answer_l, bad2));
		answer_l.Add (bad2);

		do {
			if (result == 0)
				bad3 = result + rnd.Next (1, 9);
			else
				bad3 = result * rnd.Next (2, 6);
		} while (UtilList.FoundInListInt (answer_l, bad3));
		//answer_l.Add (bad3);

		return new QuestionAnswers (
				"    " + Util.StringArrayToString (convertGLToStrIfNeeded (sa), " "),
				result.ToString (),
				bad1.ToString (),
				bad2.ToString (),
				bad3.ToString ()
				);
	}

	protected virtual string [] convertGLToIntsIfNeeded (string [] sa)
	{
		return sa;
	}

	protected virtual string [] convertGLToStrIfNeeded (string [] sa)
	{
		return sa;
	}

	protected string generateRandomOperator ()
	{
		int op = rnd.Next (0, 9); //include 0, exclude 9
		if (op < 4) 		// 40 %
			return "+";
		else if (op < 7) 	// 30 %
			return "-";
		else			// 30 %
			return "*";
	}

	private int calculateStr (string [] sArray)
	{
		int result = 0;
		string op = "";

		for (int i = 0; i < sArray.Length; i ++)
		{
			string s = sArray[i];
			//LogB.Information ("s is: " + s);

			int num;
			if (int.TryParse (s, out num)) 	 // s is an int
			{
				if (op == "")
					result = num;
				else {
					result = calculateOp (result, op, num);
					op = "";
				}
			}
			else {
				if (s == "+" || s == "-" || s == "*")
					op = s;
				else if (s == "(")
				{
					string [] sInsidePar = new string [3] {
						sArray[i+1], sArray[i+2], sArray[i+3]
					};

					//LogB.Information ("start calculateStr insidePar");
					num = calculateStr (sInsidePar); //recursive
					//LogB.Information ("end calculateStr insidePar");

					if (op == "")
						result = num;
					else {
						result = calculateOp (result, op, num);
						op = "";
					}
					//LogB.Information ("result is: " + result.ToString ());
					i += 4;
				}
				else
					throw new Exception("syntax error");
			}
		}
		return result;
	}

	private int calculateOp (int a, string op, int b)
	{
		if (op == "+")
			a += b;
		else if (op == "-")
			a -= b;
		else if (op == "*")
			a *= b;

		return a;
	}

	public string DifficultyName {
		get { return difficultyName; }
	}
}

public class QuestionAnswerArithmeticGodLevel : QuestionAnswerArithmetic
{
	public QuestionAnswerArithmeticGodLevel ()
	{
		rnd = new Random();
	}

	public override QuestionAnswers GenerateQA ()
	{
		return generateQAGodLevel ();
	}

	// trigonometrical methods are in degrees
	enum godLevelOperands {
		COS0, COS90, COS180, COS270,
		SIN0, SIN90, SIN180, SIN270,
		TAN0, TAN45, TAN90,
		FIVECUBE, SEVENCUBE,
		SQRT64, SQRT144, SQRT169,
		EEXP0, ROUND7PI,
		LINEARENCLENGTHM, XAVIPADUCURRENTYEARS,
		CURRENTYEAR, CURRENTMONTH, CURRENTDAYOFMONTH
	};

	// to display to user
	private string godLevelOperandsAsStr (godLevelOperands operand)
	{
		switch (operand) {
			case godLevelOperands.COS0 :
				return ("cos (0º)");
			case godLevelOperands.COS90 :
				return ("cos (90º)");
			case godLevelOperands.COS180 :
				return ("cos (180º)");
			case godLevelOperands.COS270 :
				return ("cos (270º)");
			case godLevelOperands.SIN0 :
				return ("sin (0º)");
			case godLevelOperands.SIN90 :
				return ("sin (90º)");
			case godLevelOperands.SIN180 :
				return ("sin (180º)");
			case godLevelOperands.SIN270 :
				return ("sin (270º)");
			case godLevelOperands.TAN0 :
				return ("tan (0º)");
			case godLevelOperands.TAN45 :
				return ("tan (45º)");
			case godLevelOperands.TAN90 :
				return ("tan (90º)");
			case godLevelOperands.FIVECUBE :
				return ("5^3");
			case godLevelOperands.SEVENCUBE :
				return ("7^3");
			case godLevelOperands.SQRT64 :
				return ("sqrt (64)");
			case godLevelOperands.SQRT144 :
				return ("sqrt (144)");
			case godLevelOperands.SQRT169 :
				return ("sqrt (169)");
			case godLevelOperands.EEXP0 :
				return ("e^0");
			case godLevelOperands.ROUND7PI :
				return ("round (7 * pi)");
			case godLevelOperands.LINEARENCLENGTHM :
				return ("Linear encoder maximum length (in m)");
			case godLevelOperands.XAVIPADUCURRENTYEARS :
				return ("Anys des de que va néixer el Xavi Padu");
			case godLevelOperands.CURRENTYEAR :
				return ("year");
			case godLevelOperands.CURRENTMONTH :
				return ("month");
			case godLevelOperands.CURRENTDAYOFMONTH :
				return ("day of month");
			default:
				return ("cos (0)");
		}
	}

	private int godLevelOperandsAsInt (godLevelOperands operand)
	{
		switch (operand) {
			case godLevelOperands.COS0 :
				return (1);
			case godLevelOperands.COS90 :
				return (0);
			case godLevelOperands.COS180 :
				return (-1);
			case godLevelOperands.COS270 :
				return (0);
			case godLevelOperands.SIN0 :
				return (0);
			case godLevelOperands.SIN90 :
				return (1);
			case godLevelOperands.SIN180 :
				return (0);
			case godLevelOperands.SIN270 :
				return (-1);
			case godLevelOperands.TAN0 :
				return (0);
			case godLevelOperands.TAN45 :
				return (1);
			case godLevelOperands.TAN90 :
				return (0);
			case godLevelOperands.FIVECUBE :
				return (125);
			case godLevelOperands.SEVENCUBE :
				return (343);
			case godLevelOperands.SQRT64 :
				return (8);
			case godLevelOperands.SQRT144 :
				return (12);
			case godLevelOperands.SQRT169 :
				return (13);
			case godLevelOperands.EEXP0 :
				return (1);
			case godLevelOperands.ROUND7PI :
				return (22);
			case godLevelOperands.LINEARENCLENGTHM :
				return (3);
			case godLevelOperands.XAVIPADUCURRENTYEARS :
				return (48);
			case godLevelOperands.CURRENTYEAR :
				return DateTime.Today.Year;
			case godLevelOperands.CURRENTMONTH :
				return DateTime.Today.Month;
			case godLevelOperands.CURRENTDAYOFMONTH :
				return DateTime.Today.Day;
			default :
				return (1);
		}
	}

	// all operands are godLevelOperands
	private QuestionAnswers generateQAGodLevel ()
	{
		difficultyName = "GodLevel";

		string aStr, bStr, cStr;
		int max = Enum.GetNames<godLevelOperands>().Length;

		aStr = Enum.GetNames (typeof (godLevelOperands))[rnd.Next (0, max)];
		string op1 = generateRandomOperator ();
		bStr = Enum.GetNames (typeof (godLevelOperands))[rnd.Next (0, max)];
		string op2 = generateRandomOperator ();
		cStr = Enum.GetNames (typeof (godLevelOperands))[rnd.Next (0, max)];

		return generateQAMaybeWithParentheses (aStr, op1, bStr, op2, cStr);
	}

	// it also checks if some of the operands are not godLevel
	// note we need to create a new string [] becase we do not want to modify original
	protected override string [] convertGLToIntsIfNeeded (string [] sa)
	{
		string [] saConverted = new String [sa.Length];
		for (int i = 0; i < sa.Length; i ++)
		{
			if (Enum.IsDefined (typeof(godLevelOperands), sa[i]))
			{
				saConverted[i] = godLevelOperandsAsInt (
						(godLevelOperands) Enum.Parse (typeof (godLevelOperands), sa[i])
						).ToString ();
			} else
				saConverted[i] = sa[i];
		}
		return saConverted;
	}

	protected override string [] convertGLToStrIfNeeded (string [] sa)
	{
		string [] saConverted = new String [sa.Length];
		for (int i = 0; i < sa.Length; i ++)
		{
			if (Enum.IsDefined (typeof(godLevelOperands), sa[i]))
			{
				saConverted[i] = godLevelOperandsAsStr (
						(godLevelOperands) Enum.Parse (typeof (godLevelOperands), sa[i])
						);
			} else
				saConverted[i] = sa[i];
		}
		return saConverted;
	}
}

/*
 * unused, we prefer the above class QuestionAnswerArithmetic as it is more natural to implement
 *
public class QuestionAnswerArithmeticUnused
{
	delegate decimal BinaryOperation (decimal a, decimal b);

	public QuestionAnswerArithmeticOp1 ()
	{
		LogB.Information ("QuestionAnswerArithmetic");
		//test (new string [] { "2", "+", "7" });
		test (new string [] { "2", "7", "+" });
		test (new string [] { "2", "7", "-" });
		test (new string [] { "7", "2", "-" });
		test (new string [] { "2", "7", "*" });
		test (new string [] { "1", "5", "+", "2", "-" });
	}

	// thanks to: Steve Cooper https://stackoverflow.com/users/6722/steve-cooper
	private void test (string [] strArray)
	{
		var stack = new Stack<decimal>();
		var map = new Dictionary<string, BinaryOperation>();
		map.Add ("+", (a,b) => a+b);
		map.Add ("-", (a,b) => a-b);
		map.Add ("*", (a,b) => a*b);

		foreach(var i in strArray)
		{
			decimal number;
			BinaryOperation op;

			if (decimal.TryParse(i, out number))
			{
				// we've found a number
				stack.Push (number);
			}
			else if (map.TryGetValue(i, out op))
			{
				// we've found a known operator;
				var a = stack.Pop ();
				var b = stack.Pop ();
				var result = op (a,b);
				stack.Push (result);
			}
			else
				throw new Exception("syntax error");
		}
		LogB.Information (string.Format ("Expression was: {0}, result is: {1}",
					Util.StringArrayToString (strArray, " "), stack.Peek()));
	}
}
*/

