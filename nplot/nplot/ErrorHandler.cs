/*
NPlot - A charting library for .NET

ErrorHandler.cs
Copyright (C) 2004
Matt Howlett

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions
are met:

1. Redistributions of source code must retain the above copyright
   notice, this list of conditions and the following disclaimer.
   
2. Redistributions in binary form must reproduce the following text in 
   the documentation and / or other materials provided with the 
   distribution: 
   
   "This product includes software developed as part of 
   the NPlot charting library project available from: 
   http://www.nplot.com/" 

------------------------------------------------------------------------

THIS SOFTWARE IS PROVIDED BY THE AUTHOR ``AS IS'' AND ANY EXPRESS OR
IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.
IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY DIRECT, INDIRECT,
INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT
NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

$Id: ErrorHandler.cs,v 1.2 2004/11/04 12:18:24 mhowlett Exp $

*/

using System;
using System.Diagnostics;


namespace NPlot
{
	/// <summary>
	/// Error class that 
	/// (a) adds source of problem. 
	/// (b) by default throws exception.
	/// (c) default behaviour can be overridden.
	/// </summary>
	public class ErrorHandler
	{

		/// <summary>
		/// The singular ErrorHandler instance 
		/// </summary>
		public static readonly ErrorHandler Instance = new ErrorHandler();

		/// <summary>
		/// Defines the allowed message levels. 
		/// </summary>
		public enum Level
		{
			/// <summary>
			/// Debugging Message. Note it's best not to use the general 
			/// handle method to emit these - use the conditional Debug
			/// function.
			/// </summary>
			Debug = 0,
			/// <summary>
			/// Something went wrong, but code execution can continue.
			/// </summary>
			Continuing = 1,
			/// <summary>
			/// Something wend wrong, and code execution can not continue.
			/// </summary>
			Critical = 2
		}

		private delegate void HandlerDelegate( string message );

		private HandlerDelegate[] handlers_;

		private void HandleError( string message )
		{
			throw new System.Exception( message );
		}

		private void NullHandler( string message )
		{
			// do nothing.
		}

		private ErrorHandler()
		{
			// set up default 
			handlers_ = new HandlerDelegate[3];
			handlers_[0] = new HandlerDelegate(HandleError);
			handlers_[1] = new HandlerDelegate(HandleError);
			handlers_[2] = new HandlerDelegate(HandleError);
		}

		/// <summary>
		/// Handles a debug message.
		/// </summary>
		/// <param name="message">message to handle</param>
		[Conditional("DEBUG")]
		public void DebugError( string message )
		{
			handlers_[(int)Level.Debug]( GetPrepend() + message );
		}


		/// <summary>
		/// Handles a continuing error.
		/// </summary>
		/// <param name="message">message to handle</param>
		public void ContinuingError( string message ) 
		{
			handlers_[(int)Level.Continuing]( GetPrepend() + message );
		}


		/// <summary>
		/// Handles a critical error.
		/// </summary>
		/// <param name="message">message to handle</param>
		public void CriticalError( string message )
		{
			handlers_[(int)Level.Critical]( GetPrepend() + message );
		}


		/// <summary>
		/// Handles a message of the given level.
		/// </summary>
		/// <param name="level">The message level.</param>
		/// <param name="message">The message to handle.</param>
		public void Handle( Level level, string message )
		{
			handlers_[(int)level]( GetPrepend() + message );
		}


		/// <summary>
		/// Get text to prepend to log message to describe its origin. This is the first
		/// place in stack outside Cts.Library.MessageLog.
		/// </summary>
		/// <returns>string to prepend to messages detailing its origin.</returns>
		private string GetPrepend()
		{
			System.Diagnostics.StackTrace st = new StackTrace();
			System.Diagnostics.StackFrame sf = null;
			string prepend = "";
			int i = 0;
			while (i < st.FrameCount)
			{
				sf = st.GetFrame(i);
				if ( sf.GetMethod().ReflectedType != this.GetType() )
				{
					break;
				}
				i += 1;
			}

			sf = st.GetFrame(i);
			prepend = 
				"[" +
				sf.GetMethod().ReflectedType +
				"." +
				sf.GetMethod().Name +
				"] ";
			return prepend;
		}


	}
}
