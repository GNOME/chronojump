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
 *  Copyright (C) 2023-2024   Xavier de Blas <xaviblas@gmail.com>
 */

using System;
using System.Collections.Generic; //List
using Gtk;
using Cairo;

public class CairoGraphForceSensorSignalQuestionnaire : CairoGraphForceSensorSignal
{
	public CairoGraphForceSensorSignalQuestionnaire (DrawingArea area, string title)
	{
		initForceSensor (area, title, true);
	}

	protected override void plotSpecific ()
	{
		questionnairePlot (points_l[points_l.Count -1]);

		drawCircle (calculatePaintX (points_l[points_l.Count -1].X),
				calculatePaintY (points_l[points_l.Count -1].Y), 6, bluePlots, true);
	}

	private void questionnairePlot (PointF lastPoint)
	{
		int textHeightHere = textHeight + 8;
		g.SetFontSize (textHeightHere);
		g.SetSourceRGB(0, 0, 0); //black

		bool enoughQuestions = true;
		QuestionAnswers qa = questionnaire.GetQAByMicros (lastPoint.X, ref enoughQuestions);

		// 1) manage finish
		if (! enoughQuestions || lastPoint.X / 1000000 >= questionnaire.N * questionnaire.QDuration)
		{
			printText ((graphWidth -getMargins (Directions.LR))/2 +getMargins (Directions.L),
					.33 * graphHeight, 0, textHeight +4,
					string.Format ("Finished! {0} points", questionnaire.Points), g, alignTypes.CENTER);
			g.SetFontSize (textHeight);

			return;
		}

		// 2) get bars position
		QRectangleManage rectangleM = new QRectangleManage ();

		double barRange = (graphHeight - topMargin - bottomMargin) /30;
		List <double> y_l = new List<double> ();
		for (int i = 0; i < 6; i ++)
			y_l.Add (topMargin + (i * ((graphHeight -topMargin - bottomMargin)/5)));

		// 3) write the question (ensure it horizontally fits)
		string questionText = string.Format ("({0}/{1}) {2}",
				questionnaire.GetQNumByMicros (lastPoint.X), questionnaire.N, qa.question);

		int textHeightReduced = textHeightHere;
		if (! textFits (questionText, g, graphWidth -leftMargin -rightMargin -70)) //75 for the messages "Force (N)" and "Points: xx" message
			textHeightReduced = findFontThatFits (textHeightHere, questionText, g, graphWidth -leftMargin -rightMargin -75);
		if (textHeightReduced >= 2)
		{
			g.SetFontSize (textHeightReduced);
			printText (graphWidth/2 -leftMargin, topMargin/2, 0, textHeightReduced,
					questionText, g, alignTypes.CENTER);
		}
		g.SetFontSize (textHeightHere);

		// 4) write the answers and manage crashes (only if at more than 15% left
		if (questionnaire.GetAnswerXrel (lastPoint.X) >= .15)
		{
			// (ensure they horizontally fit)
			List<string> answers_l = qa.TopBottom_l;
			double answerX = questionnaire.GetAnswerXrel (lastPoint.X) *
				(graphWidth - leftMargin - rightMargin) + leftMargin;
			for (int i = 0; i < 5; i ++)
			{
				string text = "NSNC";
				if (i > 0)
					text = answers_l[i-1];

				textHeightReduced = textHeightHere;
				if (! textRightAlignedFitsOnLeft (answerX - barRange, text, g, leftMargin))
					textHeightReduced = findFontThatFitsOnLeft (textHeightHere, answerX - barRange, text, g, leftMargin);

				if (textHeightReduced >= 4)
				{
					g.SetFontSize (textHeightReduced);
					printText (answerX - barRange, (y_l[i] + y_l[i+1])/2, 0, textHeight +4,
							text, g, alignTypes.RIGHT);
				}
				g.SetFontSize (textHeightHere);
			}

			// 5) plot horizontal bars
			g.SetSourceRGB(1, 0, 0); //red
			double lineLeftX = questionnaire.GetLineStartXrel (lastPoint.X) *
				(graphWidth - leftMargin - rightMargin) + leftMargin;
			for (int i = 1; i < 5; i ++)
			{
				g.Rectangle (lineLeftX, y_l[i] - barRange/2, answerX - lineLeftX, barRange);
				g.Fill();

				rectangleM.Add (new QRectangle (false, lineLeftX, y_l[i] - barRange/2, answerX - lineLeftX, barRange));
			}

			// 6) plot vertical bars
			List<Cairo.Color> answerColor_l = questionnaire.GetAnswerColor (lastPoint.X, qa);
			for (int i = 1; i < 5; i ++)
			{
				g.SetSourceColor (answerColor_l[i]);
				g.Rectangle (answerX -barRange/2, y_l[i] + barRange/2, barRange/2, y_l[i+1] - y_l[i] - barRange);
				g.Fill();

				if (answerColor_l[i].R == Questionnaire.red.R &&
						answerColor_l[i].G == Questionnaire.red.G &&
						answerColor_l[i].B == Questionnaire.red.B)
					rectangleM.Add (new QRectangle (false, answerX -barRange, y_l[i] + barRange/2, barRange, y_l[i+1] - y_l[i] - barRange));
				else
					rectangleM.Add (new QRectangle (true, answerX -barRange, y_l[i] + barRange/2, barRange, y_l[i+1] - y_l[i] - barRange));
			}

			g.SetSourceRGB(0, 0, 0); //black

			// 7 manage crash and points
			if (rectangleM.IsRed (
						calculatePaintX (lastPoint.X),
						calculatePaintY (lastPoint.Y) ))
			{
				crashedPaintOutRectangle ();
				questionnaire.Points --;
			}
			else if (rectangleM.IsGreen (
						calculatePaintX (lastPoint.X),
						calculatePaintY (lastPoint.Y) ) &&
					//do not assign more than 1 point to each question
					DateTime.Now.Subtract (questionnaire.LastGreenDt).TotalSeconds >= 1
				)
			{
				questionnaire.Points ++;
				questionnaire.LastGreenDt = DateTime.Now;
			}
		}

		g.SetFontSize (textHeightHere);
		printText (graphWidth -rightMargin, topMargin/2, 0, textHeight +4,
				"Points: " + questionnaire.Points.ToString (), g, alignTypes.RIGHT);
		g.SetFontSize (textHeight);
	}


}

public class Questionnaire
{
	public int Points;
	public DateTime LastGreenDt;

	public static Cairo.Color red = new Cairo.Color (1, 0, 0, 1);
	private Cairo.Color green = new Cairo.Color (0, 1, 0, 1);
	private Cairo.Color transp = new Cairo.Color (0, 0, 0, 0);

	private int n; //number of questions (passed from feedback gui)
	private int qDuration = 10; //duration of each question (passed from feedback gui)

	private List<QuestionAnswers> qa_l = new List<QuestionAnswers> () {
		new QuestionAnswers ("Year of 1st Chronojump version", "2004", "2008", "2012", "2016"),
		new QuestionAnswers ("Name of wireless photocells", "WICHRO", "RUN+", "PhotoProto", "TopGear"),
		new QuestionAnswers ("Chronojump products are made in ...", "Barcelona", "Helsinki", "New York", "Munich"),
		new QuestionAnswers ("Which jumps are used to calculate elasticity?", "CMJ / SJ", "CMJ / ABK", "ABK / DJ", "SJ / ABK"),
		new QuestionAnswers ("Name of the microcontroller for jumps and races with photocells?", "Chronopic", "Clock fast", "Arduino", "Double step"),
		new QuestionAnswers ("Race to measure left and right turns?", "3L3R", "Margaria", "505", "100 m hurdles"),
		new QuestionAnswers ("Biggest contact platform", "A1", "A2", "A3", "A4"),
		new QuestionAnswers ("Name of 1st version on 2031", "3.1.0", "2.31", "2.3.1", "23.1"),
		new QuestionAnswers ("Max distance of Race Analyzer", "100 m", "5 m", "10 m", "30 m"),
		new QuestionAnswers ("Sample frequency of our encoder", "1 kHz", "1 Hz", "10 Hz", "100 Hz")
	};
	private List<QuestionAnswers> qaRandom_l;

	public Questionnaire (int n, int qDuration, string filename)
	{
		this.n = n;
		this.qDuration = qDuration;

		// read questions file if needed
		if (filename != null && filename != "")
		{
			List<QuestionAnswers> qaLoading_l = getQuestionsFromFile (filename);
			if (qaLoading_l != null)
				qa_l = qaLoading_l;
		}

		// randomize questions
		qaRandom_l = UtilList.ListRandomize (qa_l);

		// if questions < n -> set n; if questions > n -> cut to n
		if (qaRandom_l.Count < n)
			n = qaRandom_l.Count;
		else if (qaRandom_l.Count > n)
			qaRandom_l = UtilList.ListGetFirstN (qaRandom_l, n);

		Points = 0;
		LastGreenDt = DateTime.Now;
	}

	public bool FileIsOk (string filename)
	{
		return (getQuestionsFromFile (filename) != null);
	}

	private List<QuestionAnswers> getQuestionsFromFile (string filename)
	{
		List<List<string>> rows_ll = UtilCSV.ReadAsListListString (filename, ';');
		if (rows_ll.Count == 0 || rows_ll[0].Count != 5)
		{
			rows_ll = UtilCSV.ReadAsListListString (filename, ',');

			if (rows_ll.Count == 0 || rows_ll[0].Count != 5)
				return null;
		}

		List<QuestionAnswers> list = new List<QuestionAnswers> ();
		foreach (List<string> row_l in rows_ll)
			list.Add (new QuestionAnswers(row_l[0], row_l[1], row_l[2], row_l[3], row_l[4]));

		return list;
	}

	public QuestionAnswers GetQAByMicros (double micros, ref bool success)
	{
		double seconds = micros / 1000000;

		if (seconds > n * qDuration)
			seconds = 0;

		//seconds += .2 * qDuration; //to show the next question just when ball passes the red/green vertical line
		seconds += .1 * qDuration; //.1 has a bit of margin between ending a question and starting the next one

		int q;
		if (seconds < qDuration)
			q = 0;
		else
			q = Convert.ToInt32 (Math.Floor (seconds/qDuration));

		//safety to not return outside of list
		success = true;
		if (q >= qaRandom_l.Count)
		{
			q = 0;
			success = false;
		}

		return qaRandom_l[q];
	}

	//just to track the number of questions
	public int GetQNumByMicros (double micros)
	{
		double seconds = micros / 1000000;

		if (seconds > n * qDuration)
			seconds = 0;

		seconds += .1 * qDuration; //.1 has a bit of margin between ending a question and starting the next one

		if (seconds < qDuration)
			return 0 + 1;
		else
			return Convert.ToInt32(Math.Floor(seconds/qDuration)) + 1;
	}

	public double GetLineStartXrel (double micros)
	{
		double xrel = GetAnswerXrel (micros) -.3;
		if (xrel < 0)
			xrel = 0;

		return xrel;
	}
	public double GetAnswerXrel (double micros)
	{
		double seconds = micros / 1000000;

		if (seconds > n * qDuration)
			seconds = 0;

		if (seconds < qDuration)
			return 1 - seconds/qDuration;
		else
			return 1 - (seconds % qDuration)/qDuration;
	}

	public List<Cairo.Color> GetAnswerColor (double micros, QuestionAnswers qa)
	{
		double xrel = GetAnswerXrel (micros) -.3;
		if (xrel < 0)
			xrel = 0;

		//Have answer color when arrive at 20% left of image (where blue ball is)
		if (xrel > .2)
			return new List<Cairo.Color> { transp, transp, transp, transp, transp };

		List<Cairo.Color> color_l = new List<Cairo.Color> ();
		color_l.Add (transp);
		foreach (string answer in qa.TopBottom_l)
		{
			if (qa.AnswerIsCorrect (answer))
				color_l.Add (green);
			else
				color_l.Add (red);
		}
		return color_l;
	}

	public int N {
		get { return n; }
	}
	public int QDuration {
		get { return qDuration; }
	}
}

public class QuestionAnswers
{
	public List<string> TopBottom_l;
	public int CorrectPos;
	public string question;

	private string aCorrect;

	public QuestionAnswers (string question, string aCorrect, string aBad1, string aBad2, string aBad3)
	{
		this.question = question;
		this.aCorrect = aCorrect;

		TopBottom_l = new List<string> () { aCorrect, aBad1, aBad2, aBad3 };
		TopBottom_l = UtilList.ListRandomize (TopBottom_l);
		CorrectPos = UtilList.FindOnListString (TopBottom_l, aCorrect);
	}

	public bool AnswerIsCorrect (string answer)
	{
		return (answer == aCorrect);
	}
}

public class QRectangleManage
{
	private List<QRectangle> rectangle_l;

	public QRectangleManage ()
	{
		rectangle_l = new List<QRectangle> ();
	}

	public void Add (QRectangle r)
	{
		rectangle_l.Add (r);
	}

	public bool IsRed (double x, double y)
	{
		foreach (QRectangle r in rectangle_l)
			if (! r.good && r.x <= x && r.x2 >= x &&
					r.y <= y && r.y2 >= y)
				return true;

		return false;
	}

	public bool IsGreen (double x, double y)
	{
		foreach (QRectangle r in rectangle_l)
			if (r.good && r.x <= x && r.x2 >= x &&
					r.y <= y && r.y2 >= y)
				return true;

		return false;
	}
}
public class QRectangle
{
	public bool good; //true: green, false: red
	public double x;
	public double y;
	public double x2;
	public double y2;

	public QRectangle (bool good, double x, double y, double width, double height)
	{
		this.good = good;
		this.x = x;
		this.y = y;
		this.x2 = x + width;
		this.y2 = y + height;
	}
}

