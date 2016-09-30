// GstVideoSplitter.cs
//
//  Copyright (C) 2007-2009 Andoni Morales Alastruey
//
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA 02110-1301, USA.
//
//

namespace LongoMatch.Video.Editor {

	using System;
	using System.Collections;
	using System.Runtime.InteropServices;
	using LongoMatch.Video.Common;


	public class GstVideoSplitter : GLib.Object, IVideoEditor, IVideoSplitter {

		[DllImport("libcesarplayer.dll")]
		static extern unsafe IntPtr gst_video_editor_new(out IntPtr err);

		public event ProgressHandler Progress;
		
		public unsafe GstVideoSplitter () : base (IntPtr.Zero)
		{
			if (GetType () != typeof (GstVideoSplitter)) {
				throw new InvalidOperationException ("Can't override this constructor.");
			}
			IntPtr error = IntPtr.Zero;
			Raw = gst_video_editor_new(out error);
			if (error != IntPtr.Zero) throw new GLib.GException (error);
			PercentCompleted += delegate(object o, PercentCompletedArgs args) {
				if (Progress!= null)
					Progress (args.Percent);
			};
		}

		#region Properties
		
		[GLib.Property ("enable-audio")]
		public bool EnableAudio {
			get {
				GLib.Value val = GetProperty ("enable-audio");
				bool ret = (bool) val;
				val.Dispose ();
				return ret;
			}
			set {
				GLib.Value val = new GLib.Value(value);
				SetProperty("enable-audio", val);
				val.Dispose ();
			}
		}
		
		[GLib.Property ("enable-title")]
		public bool EnableTitle {
			get {
				GLib.Value val = GetProperty ("enable-title");
				bool ret = (bool) val;
				val.Dispose ();
				return ret;
			}
			set {
				GLib.Value val = new GLib.Value(value);
				SetProperty("enable-title", val);
				val.Dispose ();
			}
		}
		
		[GLib.Property ("video_bitrate")]
		public int VideoBitrate {
			get {
				GLib.Value val = GetProperty ("video_bitrate");
				int ret = (int) val;
				val.Dispose ();
				return ret;
			}
			set {
				GLib.Value val = new GLib.Value(value);
				SetProperty("video_bitrate", val);
				val.Dispose ();
			}
		}

		[GLib.Property ("audio_bitrate")]
		public int AudioBitrate {
			get {
				GLib.Value val = GetProperty ("audio_bitrate");
				int ret = (int) val;
				val.Dispose ();
				return ret;
			}
			set {
				GLib.Value val = new GLib.Value(value);
				SetProperty("audio_bitrate", val);
				val.Dispose ();
			}
		}
		
		[GLib.Property ("width")]
		public int Width {
			get {
				GLib.Value val = GetProperty ("width");
				int ret = (int) val;
				val.Dispose ();
				return ret;
			}
			set {
				GLib.Value val = new GLib.Value(value);
				SetProperty("width", val);
				val.Dispose ();
			}
		}
		
		[GLib.Property ("height")]
		public int Height {
			get {
				GLib.Value val = GetProperty ("height");
				int ret = (int) val;
				val.Dispose ();
				return ret;
			}
			set {
				GLib.Value val = new GLib.Value(value);
				SetProperty("height", val);
				val.Dispose ();
			}
		}
		
		[GLib.Property ("output_file")]
		public string OutputFile {
			get {
				GLib.Value val = GetProperty ("output_file");
				string ret = (string) val;
				val.Dispose ();
				return ret;
			}
			set {
				GLib.Value val = new GLib.Value(value);				
				SetProperty("output_file", val);
				val.Dispose ();
			}
		}
		
		#endregion

		
		
		#region GSignals
#pragma warning disable 0169
		[GLib.CDeclCallback]
		delegate void ErrorVMDelegate (IntPtr gvc, IntPtr message);

		static ErrorVMDelegate ErrorVMCallback;

		static void error_cb (IntPtr gvc, IntPtr message)
		{
			try {
				GstVideoSplitter gvc_managed = GLib.Object.GetObject (gvc, false) as GstVideoSplitter;
				gvc_managed.OnError (GLib.Marshaller.Utf8PtrToString (message));
			} catch (Exception e) {
				GLib.ExceptionManager.RaiseUnhandledException (e, false);
			}
		}

		private static void OverrideError (GLib.GType gtype)
		{
			if (ErrorVMCallback == null)
				ErrorVMCallback = new ErrorVMDelegate (error_cb);
			OverrideVirtualMethod (gtype, "error", ErrorVMCallback);
		}

		[GLib.DefaultSignalHandler(Type=typeof(LongoMatch.Video.Editor.GstVideoSplitter), ConnectionMethod="OverrideError")]
		protected virtual void OnError (string message)
		{
			GLib.Value ret = GLib.Value.Empty;
			GLib.ValueArray inst_and_params = new GLib.ValueArray (2);
			GLib.Value[] vals = new GLib.Value [2];
			vals [0] = new GLib.Value (this);
			inst_and_params.Append (vals [0]);
			vals [1] = new GLib.Value (message);
			inst_and_params.Append (vals [1]);
			g_signal_chain_from_overridden (inst_and_params.ArrayPtr, ref ret);
			foreach (GLib.Value v in vals)
				v.Dispose ();
		}

		[GLib.Signal("error")]
		public event ErrorHandler Error {
			add {
				GLib.Signal sig = GLib.Signal.Lookup (this, "error", typeof (ErrorArgs));
				sig.AddDelegate (value);
			}
			remove {
				GLib.Signal sig = GLib.Signal.Lookup (this, "error", typeof (ErrorArgs));
				sig.RemoveDelegate (value);
			}
		}
		

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void PercentCompletedVMDelegate (IntPtr gvc, float percent);

		static PercentCompletedVMDelegate PercentCompletedVMCallback;

		static void percentcompleted_cb (IntPtr gvc, float percent)
		{
			try {
				GstVideoSplitter gvc_managed = GLib.Object.GetObject (gvc, false) as GstVideoSplitter;
				gvc_managed.OnPercentCompleted (percent);
			} catch (Exception e) {
				GLib.ExceptionManager.RaiseUnhandledException (e, false);
			}
		}

		private static void OverridePercentCompleted (GLib.GType gtype)
		{
			if (PercentCompletedVMCallback == null)
				PercentCompletedVMCallback = new PercentCompletedVMDelegate (percentcompleted_cb);
			OverrideVirtualMethod (gtype, "percent_completed", PercentCompletedVMCallback);
		}

		[GLib.DefaultSignalHandler(Type=typeof(LongoMatch.Video.Editor.GstVideoSplitter), ConnectionMethod="OverridePercentCompleted")]
		protected virtual void OnPercentCompleted (float percent)
		{
			GLib.Value ret = GLib.Value.Empty;
			GLib.ValueArray inst_and_params = new GLib.ValueArray (2);
			GLib.Value[] vals = new GLib.Value [2];
			vals [0] = new GLib.Value (this);
			inst_and_params.Append (vals [0]);
			vals [1] = new GLib.Value (percent);
			inst_and_params.Append (vals [1]);
			g_signal_chain_from_overridden (inst_and_params.ArrayPtr, ref ret);
			foreach (GLib.Value v in vals)
				v.Dispose ();
		}

		[GLib.Signal("percent_completed")]
		public event PercentCompletedHandler PercentCompleted {
			add {
				GLib.Signal sig = GLib.Signal.Lookup (this, "percent_completed", typeof (PercentCompletedArgs));
				sig.AddDelegate (value);
			}
			remove {
				GLib.Signal sig = GLib.Signal.Lookup (this, "percent_completed", typeof (PercentCompletedArgs));
				sig.RemoveDelegate (value);
			}
		}
#pragma warning restore 0169
		#endregion
		
		#region Public Methods

		[DllImport("libcesarplayer.dll")]
		static extern IntPtr gst_video_editor_get_type();

		public static new GLib.GType GType { 
			get {
				IntPtr raw_ret = gst_video_editor_get_type();
				GLib.GType ret = new GLib.GType(raw_ret);
				return ret;
			}
		}
		
		

		[DllImport("libcesarplayer.dll")]
		static extern void gst_video_editor_clear_segments_list(IntPtr raw);

		public void ClearList() {
			gst_video_editor_clear_segments_list(Handle);
		}
		
		[DllImport("libcesarplayer.dll")]
		static extern void gst_video_editor_add_segment(IntPtr raw, string file_path, long start, long duration, double rate, IntPtr title, bool hasAudio);

		public void AddSegment(string filePath, long start, long duration, double rate, string title, bool hasAudio) {
			if (Environment.OSVersion.Platform == PlatformID.Win32NT)
				filePath="file:///"+filePath;
			gst_video_editor_add_segment(Handle, filePath, start, duration, rate, GLib.Marshaller.StringToPtrGStrdup(title), hasAudio);
		}
		
		
		[DllImport("libcesarplayer.dll")]
		static extern void gst_video_editor_start(IntPtr raw);

		public void Start() {
			gst_video_editor_start(Handle);
		}
		
		[DllImport("libcesarplayer.dll")]
		static extern void gst_video_editor_cancel(IntPtr raw);

		public void Cancel() {
			// The handle might have already been dealocated
			try{
				gst_video_editor_cancel(Handle);
			}catch{
			}
		}
		
		[DllImport("libcesarplayer.dll")]
		static extern void gst_video_editor_set_video_encoder(IntPtr raw, out IntPtr error_ptr, int type);

		public void SetVideoEncoder(out string error, VideoEncoderType codec) {
			IntPtr error_ptr = IntPtr.Zero;
			gst_video_editor_set_video_encoder(Handle,out error_ptr,(int)codec);
			if (error_ptr != IntPtr.Zero)
				error = GLib.Marshaller.Utf8PtrToString(error_ptr);
			else
				error = null;
		}
		
		[DllImport("libcesarplayer.dll")]
		static extern void gst_video_editor_set_audio_encoder(IntPtr raw, out IntPtr error_ptr, int type);

		public void SetAudioEncoder(out string error, AudioEncoderType codec) {
			IntPtr error_ptr = IntPtr.Zero;
			gst_video_editor_set_audio_encoder(Handle,out error_ptr,(int)codec);
			if (error_ptr != IntPtr.Zero)
				error = GLib.Marshaller.Utf8PtrToString(error_ptr);
			else
				error = null;
		}
		
		[DllImport("libcesarplayer.dll")]
		static extern void gst_video_editor_set_video_muxer(IntPtr raw, out IntPtr error_ptr, int type);

		public void SetVideoMuxer(out string error, VideoMuxerType muxer) {
			IntPtr error_ptr = IntPtr.Zero;
			gst_video_editor_set_video_muxer(Handle,out error_ptr,(int)muxer);
			if (error_ptr != IntPtr.Zero)
				error = GLib.Marshaller.Utf8PtrToString(error_ptr);
			else
				error = null;
		}

		[DllImport("libcesarplayer.dll")]
		static extern void gst_video_editor_init_backend(out int argc, IntPtr argv);

		public static int InitBackend(string argv) {
			int argc;
			gst_video_editor_init_backend(out argc, GLib.Marshaller.StringToPtrGStrdup(argv));
			return argc;
		}
		
		
		public void SetSegment (string filePath, long start, long duration, double rate, string title, bool hasAudio){
			ClearList();
			AddSegment(filePath, start, duration, rate, title,hasAudio);
		}
		
		public VideoQuality VideoQuality{
			set{VideoBitrate=(int)value;}
		}
		
		public AudioQuality AudioQuality{
			set{AudioBitrate = (int)value;}
		}
		
		public VideoFormat VideoFormat{
			set{
				if (value == VideoFormat.PORTABLE){
					Height = 240;
					Width = 320;
				}
				else if (value == VideoFormat.VGA){
					Height = 480 ;
					Width = 640;
				}
				else if (value == VideoFormat.TV){
					Height = 576;
					Width = 720;
				}
				else if (value == VideoFormat.HD720p){
					Height = 720;
					Width = 1280;
				}
				else if (value == VideoFormat.HD1080p){
					Height = 1080;
					Width = 1920;
				}
			}
		}
		
		public AudioEncoderType AudioEncoder{
			set{
				string error;
				SetAudioEncoder(out error,value);
				if (error != null)
					throw new Exception(error);
			}
		}
		
		public VideoEncoderType VideoEncoder{
			set{
				string error;
				SetVideoEncoder(out error, value);
				if (error != null)
					throw new Exception(error);
			}
		}
		
		public VideoMuxerType VideoMuxer{
			set{
				string error;
				SetVideoMuxer(out error,value);
				if (error != null)
					throw new Exception(error);
			}
		}
		
		public string TempDir{
			set{;}
		}
		
		#endregion

		
	}
}
