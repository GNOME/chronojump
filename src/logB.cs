//
// Log.cs
//
// Author:
//   Aaron Bockover <abockover@novell.com>
//
// Copyright (C) 2005-2007 Novell, Inc.
//
// Minor tweaks by Xavier de Blas <xaviblas@gmail.com> 2014,2016-2017
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Text;
using System.Collections.Generic;
using System.Threading;

	
public delegate void LogNotifyHandler(LogNotifyArgs args);

public class LogNotifyArgs : EventArgs
{
	private LogEntry entry;

	public LogNotifyArgs(LogEntry entry)
	{
		this.entry = entry;
	}

	public LogEntry Entry {
		get {
			return entry;
		}
	}
}

public enum LogEntryType
{
	Debug,
	Warning,
	Error,
	Information,
	SQL, SQLon, SQLoff, SQLonAlready, //already means: was not opened because it was already opened
	ThreadStart, ThreadEnding, ThreadEnded,
	TestStart, TestEnd
}

public class LogEntry
{
	private LogEntryType type;
	private string message;
	private string details;
	private DateTime timestamp;

	internal LogEntry(LogEntryType type, string message, string details)
	{
		this.type = type;
		this.message = message;
		this.details = details;
		this.timestamp = DateTime.Now;
	}

	public LogEntryType Type {
		get {
			return type;
		}
	}

	public string Message {
		get {
			return message;
		}
	}

	public string Details {
		get {
			return details;
		}
	}

	public DateTime TimeStamp {
		get {
			return timestamp;
		}
	}
}

/*
 * LogB Console.Write is written to a file. see log.Start() at log.cs
 * is done by
 * 	System.Console.SetOut(sw);
 * 	System.Console.SetError(sw);
 * 	sw.AutoFlush = true;
 * and this is not threadsafe.
 * We store a string on this class with the Log that will be pushed to Console -> StreamWriter, only by the main thread
 */
// Based on O'Reilly C# Cookbook p. 1015
public static class LogSync
{
	private static string logPending;
	private static object syncObj = new object();

	//only called one time (at Chronojump Main())
	public static void Initialize()
	{
		logPending = "";
	}

	//called by threads 2 and above
	public static void Add(string str)
	{
		lock(syncObj)
		{
			logPending += str;
		}
	}
	
	//called by thread 1 (GTK)
	public static string ReadAndEmpty()
	{
		lock(syncObj)
		{
			string str = logPending;
			logPending = "";
			return str;
		}
	}
}

//copied from Banshee project
//called LogB because in Chronojump there's already a Log class that will be deprecated soon
public static class LogB
{
	public static event LogNotifyHandler Notify;

	private static Dictionary<uint, DateTime> timers = new Dictionary<uint, DateTime> ();
	private static uint next_timer_id = 1;

	/*
	 * Chronojump can be called with a param printAll (chronojump printAll) and then all threads will be printed
	 * default behaviour is to print only when on GTK thread, and messages from other threads
	 * will be stored and printed by GTK thread before what it needs to print
	 */
	public static bool Mute = false;
	public static bool PrintAllThreads = false;

	private static bool debugging = false;
	public static bool Debugging {
		get {
			return debugging;
		}
		set {
			debugging = value;
		}
	}

	public static void Commit(LogEntryType type, string message, string details, bool showUser)
	{
		if(Mute)
			return;

		if(message == null)
			message = "";

		if(type == LogEntryType.Debug && !Debugging) {
			return;
		}

		if(type != LogEntryType.Information || (type == LogEntryType.Information && !showUser))
		{

			var thread_name = String.Empty;
			bool printNow = false;
			if(Debugging) {
				var thread = Thread.CurrentThread;
				thread_name = thread.ManagedThreadId.ToString();
				if(thread_name == "1") {
					printNow = true;
					thread_name = "1-GTK ";
				} else 
					thread_name += "     ";
			}

			string lineStart = string.Format("[{5}{0} {1:00}:{2:00}:{3:00}.{4:000}]", TypeString(type), DateTime.Now.Hour,
					DateTime.Now.Minute, DateTime.Now.Second, DateTime.Now.Millisecond, thread_name);

			if(PrintAllThreads || printNow)
			{
				/*
				 * only doing this in main thread now.
				 * Less buggy and seems to have finished with ^@ and 92M messages
				 */
				switch(type) {
					case LogEntryType.Error:
						ConsoleCrayon.ForegroundColor = ConsoleColor.Red;
						break;
					case LogEntryType.Warning:
						ConsoleCrayon.ForegroundColor = ConsoleColor.DarkYellow;
						break;
					case LogEntryType.Information:
						ConsoleCrayon.ForegroundColor = ConsoleColor.Green;
						break;
					case LogEntryType.Debug:
						ConsoleCrayon.ForegroundColor = ConsoleColor.Blue;
						break;
					case LogEntryType.SQL:
						ConsoleCrayon.ForegroundColor = ConsoleColor.Cyan;
						break;
					case LogEntryType.SQLon:
						ConsoleCrayon.BackgroundColor = ConsoleColor.DarkCyan;
						ConsoleCrayon.ForegroundColor = ConsoleColor.White;
						break;
					case LogEntryType.SQLoff:
						ConsoleCrayon.BackgroundColor = ConsoleColor.DarkCyan;
						ConsoleCrayon.ForegroundColor = ConsoleColor.Black;
						break;
					case LogEntryType.SQLonAlready:
						ConsoleCrayon.BackgroundColor = ConsoleColor.DarkCyan;
						ConsoleCrayon.ForegroundColor = ConsoleColor.Gray;
						break;
					case LogEntryType.ThreadStart:
						ConsoleCrayon.BackgroundColor = ConsoleColor.Green;
						ConsoleCrayon.ForegroundColor = ConsoleColor.Black;
						break;
					case LogEntryType.ThreadEnding:
						ConsoleCrayon.BackgroundColor = ConsoleColor.Yellow;
						ConsoleCrayon.ForegroundColor = ConsoleColor.Black;
						break;
					case LogEntryType.ThreadEnded:
						ConsoleCrayon.BackgroundColor = ConsoleColor.Red;
						ConsoleCrayon.ForegroundColor = ConsoleColor.Black;
						break;
					case LogEntryType.TestStart:
						ConsoleCrayon.BackgroundColor = ConsoleColor.Blue;
						ConsoleCrayon.ForegroundColor = ConsoleColor.Black;
						break;
					case LogEntryType.TestEnd:
						ConsoleCrayon.BackgroundColor = ConsoleColor.Blue;
						ConsoleCrayon.ForegroundColor = ConsoleColor.Black;
						break;
				}

				Console.Write(lineStart);

				ConsoleCrayon.ResetColor();

				//print messagesOtherThreads
				//do not add to "message" because like this is better to find why crashes
				string messagesOtherThreads = LogSync.ReadAndEmpty();
				if(messagesOtherThreads != null) {
					try {
						Console.Write(messagesOtherThreads);
					} catch (System.IndexOutOfRangeException) {
						Console.Write("CATCHED printing messagesOtherThreads");
					}
				}

				//print messages this thread
				if(details != null)
					message += " - " + details;

				try {
					if(type == LogEntryType.Debug)
						Console.Write(" {0}", message);
					else
						Console.WriteLine(" {0}", message);
				}
				catch (System.IndexOutOfRangeException) {
					Console.Write("CATCHED printing main thread");
				}
			} else {
				LogSync.Add(lineStart + "\n" + message);
			}
		}

		if(showUser) {
			OnNotify(new LogEntry(type, message, details));
		}
	}

	private static string TypeString(LogEntryType type)
	{
		switch(type) {
			case LogEntryType.Debug:
				return "Debug";
			case LogEntryType.Warning:
				return "Warn ";
			case LogEntryType.Error:
				return "Error";
			case LogEntryType.Information:
				return "Info ";
			case LogEntryType.SQL:
				return "SQL ";
			case LogEntryType.SQLon:
				return "SQL ON";
			case LogEntryType.SQLoff:
				return "SQL OFF";
			case LogEntryType.SQLonAlready:
				return "SQL ON ALREADY!!!";
			case LogEntryType.ThreadStart:
				return "Thread Start -------------------------->";
			case LogEntryType.ThreadEnding:
				return " <.......................... Thread Ending";
			case LogEntryType.ThreadEnded:
				return " <-------------------------- Thread Ended";
			case LogEntryType.TestStart:
				return "Test Start t_t_t_t_t_t_t_t_t_t_->";
			case LogEntryType.TestEnd:
				return "Test End <-_t_t_t_t_t_t_t_t_t_t";
		}
		return null;
	}

	private static void OnNotify(LogEntry entry)
	{
		LogNotifyHandler handler = Notify;
		if(handler != null) {
			handler(new LogNotifyArgs(entry));
		}
	}

#region Timer Methods

	public static uint DebugTimerStart(string message)
	{
		return TimerStart(message, false);
	}

	public static uint InformationTimerStart(string message)
	{
		return TimerStart(message, true);
	}

	private static uint TimerStart(string message, bool isInfo)
	{
		if(!Debugging && !isInfo) {
			return 0;
		}

		if(isInfo) {
			Information(message);
		} else {
			Debug(message);
		}

		return TimerStart(isInfo);
	}

	public static uint DebugTimerStart()
	{
		return TimerStart(false);
	}

	public static uint InformationTimerStart()
	{
		return TimerStart(true);
	}

	private static uint TimerStart(bool isInfo)
	{
		if(!Debugging && !isInfo) {
			return 0;
		}

		uint timer_id = next_timer_id++;
		timers.Add(timer_id, DateTime.Now);
		return timer_id;
	}

	public static void DebugTimerPrint(uint id)
	{
		if(!Debugging) {
			return;
		}

		TimerPrint(id, "Operation duration: {0}", false);
	}

	public static void DebugTimerPrint(uint id, string message)
	{
		if(!Debugging) {
			return;
		}

		TimerPrint(id, message, false);
	}

	public static void InformationTimerPrint(uint id)
	{
		TimerPrint(id, "Operation duration: {0}", true);
	}

	public static void InformationTimerPrint(uint id, string message)
	{
		TimerPrint(id, message, true);
	}

	private static void TimerPrint(uint id, string message, bool isInfo)
	{
		if(!Debugging && !isInfo) {
			return;
		}

		DateTime finish = DateTime.Now;

		if(!timers.ContainsKey(id)) {
			return;
		}

		TimeSpan duration = finish - timers[id];
		string d_message;
		if(duration.TotalSeconds < 60) {
			d_message = duration.TotalSeconds.ToString();
		} else {
			d_message = duration.ToString();
		}

		if(isInfo) {
			InformationFormat(message, d_message);
		} else {
			DebugFormat(message, d_message);
		}
	}

#endregion

#region Public Debug Methods

	public static void Debug(string message, string details)
	{
		if(Debugging) {
			Commit(LogEntryType.Debug, message, details, false);
		}
	}

	public static void Debug(string message)
	{
		if(Debugging) {
			Debug(message, null);
		}
	}

	public static void DebugFormat(string format, params object [] args)
	{
		if(Debugging) {
			Debug(String.Format(format, args));
		}
	}

#endregion

#region Public Information Methods

	// to debug any variable
	public static void Information (string varName, object val)
	{
		Information (string.Format ("{0}: {1}", varName, val), null);
	}

	public static void Information(string message)
	{
		Information(message, null);
	}

	public static void Information(string message, string details)
	{
		Information(message, details, false);
	}

	public static void Information(string message, string details, bool showUser)
	{
		Commit(LogEntryType.Information, message, details, showUser);
	}

	public static void Information(string message, bool showUser)
	{
		Information(message, null, showUser);
	}

	public static void InformationFormat(string format, params object [] args)
	{
		Information(String.Format(format, args));
	}

#endregion

#region Public Warning Methods

	public static void Warning(string message)
	{
		Warning(message, null);
	}

	public static void Warning(string message, string details)
	{
		Warning(message, details, false);
	}

	public static void Warning(string message, string details, bool showUser)
	{
		Commit(LogEntryType.Warning, message, details, showUser);
	}

	public static void Warning(string message, bool showUser)
	{
		Warning(message, null, showUser);
	}

	public static void WarningFormat(string format, params object [] args)
	{
		Warning(String.Format(format, args));
	}

#endregion

#region Public Error Methods

	public static void Error(string message)
	{
		Error(message, null);
	}

	public static void Error(string message, string details)
	{
		Error(message, details, false);
	}

	public static void Error(string message, string details, bool showUser)
	{
		Commit(LogEntryType.Error, message, details, showUser);
	}

	public static void Error(string message, bool showUser)
	{
		Error(message, null, showUser);
	}

	public static void ErrorFormat(string format, params object [] args)
	{
		Error(String.Format(format, args));
	}

#endregion

#region Public SQL Methods

	public static void SQL(string message)
	{
		SQL(message, null);
	}

	public static void SQL(string message, string details)
	{
		SQL(message, details, false);
	}

	public static void SQL(string message, string details, bool showUser)
	{
		Commit(LogEntryType.SQL, message, details, showUser);
	}

	public static void SQL(string message, bool showUser)
	{
		SQL(message, null, showUser);
	}

	public static void SQLFormat(string format, params object [] args)
	{
		SQL(String.Format(format, args));
	}
	

	public static void SQLon()
	{
		Commit(LogEntryType.SQLon, null, null, false);
	}

	public static void SQLoff()
	{
		Commit(LogEntryType.SQLoff, null, null, false);
	}

	public static void SQLonAlready()
	{
		Commit(LogEntryType.SQLonAlready, null, null, false);
	}

#endregion

#region Public Thread Methods
	
	public static void ThreadStart()
	{
		Commit(LogEntryType.ThreadStart, null, null, false);
	}

	public static void ThreadEnding()
	{
		Commit(LogEntryType.ThreadEnding, null, null, false);
	}

	public static void ThreadEnded()
	{
		Commit(LogEntryType.ThreadEnded, null, null, false);
	}

#endregion

#region Public Test Methods
	
	public static void TestStart(string test)
	{
		Commit(LogEntryType.TestStart, test, null, false);
	}
	
	public static void TestStart(string test, string details)
	{
		Commit(LogEntryType.TestStart, test, details, false);
	}

	public static void TestEnd(string test)
	{
		Commit(LogEntryType.TestEnd, test, null, false);
	}
	
	public static void TestEnd(string test, string details)
	{
		Commit(LogEntryType.TestEnd, test, details, false);
	}

#endregion

#region Public Exception Methods

	public static void DebugException(Exception e)
	{
		if(Debugging) {
			Exception(e);
		}
	}

	public static void Exception(Exception e)
	{
		Exception(null, e);
	}

	public static void Exception(string message, Exception e)
	{
		Stack<Exception> exception_chain = new Stack<Exception> ();
		StringBuilder builder = new StringBuilder();

		while(e != null) {
			exception_chain.Push(e);
			e = e.InnerException;
		}

		while(exception_chain.Count > 0) {
			e = exception_chain.Pop();
			builder.AppendFormat("{0}: {1} (in `{2}')", e.GetType(), e.Message, e.Source).AppendLine();
			builder.Append(e.StackTrace);
			if(exception_chain.Count > 0) {
				builder.AppendLine();
			}
		}

		// FIXME: We should save these to an actual log file
		LogB.Warning(message ?? "Caught an exception", builder.ToString(), false);
	}

#endregion
}
