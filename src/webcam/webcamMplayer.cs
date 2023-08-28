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
 *  Copyright (C) 2018-2023   Xavier de Blas <xaviblas@gmail.com>
 */

using System.Collections.Generic; //List
using System.Diagnostics;
using System;
using System.IO;

public class WebcamMplayer : Webcam
{
	public WebcamMplayer (string videoDevice)
	{
		this.videoDevice = videoDevice;
		Running = false;
	}

	/*
	 * constructor for Play
	 */

	public WebcamMplayer ()
	{
		executable = "mplayer";
	}

	public override Result CapturePrepare (CaptureTypes captureType)
	{
		if(process != null)
			return new Result (false, "");

		string tempFile = Util.GetWebcamPhotoTempFileNamePost();
		Util.FileDelete(tempFile);

		List<string> parameters = new List<string>();
		//-noborder -nosound -tv driver=v4l2:gain=1:width=400:height=400:device=/dev/video0:fps=10:outfmt=rgb16 tv:// -vf screenshot=/tmp/chronojump-last-photo
		//parameters.Insert (0, "-noborder"); //on X11 can be: title "Chronojump"". -noborder makes no accept 's', or 'q'

		int i = 0;
		if(UtilAll.GetOSEnum() == UtilAll.OperatingSystems.LINUX)
		{
			parameters.Insert (i ++, "-title"); //on X11 can be: title "Chronojump"". -noborder makes no accept 's', or 'q'
			if(captureType == CaptureTypes.PHOTO)
				parameters.Insert (i ++, "Chronojump snapshot");
			else //if(captureType == CaptureTypes.VIDEO)
				parameters.Insert (i ++, "Chronojump video record");
		} else
			parameters.Insert (i ++, "-noborder");

		parameters.Insert (i ++, "-nosound");
		parameters.Insert (i ++, "-tv");
		parameters.Insert (i ++, "driver=v4l2:gain=1:width=400:height=400:device=" + videoDevice + ":fps=10:outfmt=rgb16");
		parameters.Insert (i ++, "tv://");
		parameters.Insert (i ++, "-vf");
		parameters.Insert (i ++, "screenshot=" + Util.GetWebcamPhotoTempFileNamePre());

		process = new Process();
		bool success = ExecuteProcess.RunAtBackground (ref process, executable, parameters, true, false, true, true, true); //redirectInput, redirectOutput, redirectError
		if(! success)
		{
			streamWriter = null;
			process = null;
			return new Result (false, "", programMplayerNotInstalled);
		}

		/*
		 * experimental double camera start
		 */
		/*
		   List<string> parametersB = parameters;
		   parametersB[4] = "driver=v4l2:gain=1:width=400:height=400:device=/dev/video1:fps=10:outfmt=rgb16";
		   parametersB[7] = "screenshot=/tmp/b/chronojump-last-photo";
		   Process processB = new Process();
		   ExecuteProcess.RunAtBackground (processB, executable, parametersB, true); //redirectInput
		   */
		/*
		 * experimental double camera end
		 */


		streamWriter = process.StandardInput;
		Running = true;

		return new Result (true, "");
	}

	public override Result PlayPreview ()
	{
		//not implemented
		return new Result (false, "");
	}
	public override Result PlayPreviewNoBackground () //experimental
	{
		//not implemented
		return new Result (false, "");
	}
	public override Result PlayPreviewNoBackgroundWantStdoutAndStderr() //experimental
	{
		//not implemented
		return new Result (false, "");
	}

	public override double FindVideoDuration (string filename)
	{
		//not implemented
		return -1;
	}

	public override int FindVideoFrames (string filename)
	{
		//not implemented
		return -1;
	}

	public override Result PlayFile (string filename)
	{
		if(process != null || filename == "")
			return new Result (false, "");

		executable = "mplayer";
		List<string> parameters = new List<string>();
		parameters.Insert (0, filename);
		//parameters.Insert (0, "-noborder"); //on X11 can be: title "Chronojump"". -noborder makes no accept 's', or 'q'
		if(UtilAll.GetOSEnum() == UtilAll.OperatingSystems.LINUX)
		{
			parameters.Insert (0, "-title"); //on X11 can be: title "Chronojump"". -noborder makes no accept 's', or 'q'
			parameters.Insert (1, "Chronojump video");
		} else
			parameters.Insert (0, "-noborder");


		process = new Process();
		bool success = ExecuteProcess.RunAtBackground (ref process, executable, parameters, false, true, false, true, true);
		if(! success)
		{
			process = null;
			return new Result (false, "", programMplayerNotInstalled);
		}

		Running = true;
		return new Result (true, "");
	}

	public override bool Snapshot()
	{
		if(process == null || streamWriter == null)
			return false;

		bool exitAtFirstSnapshot = true;

		if(! snapshotDo())
			return false;

		if(exitAtFirstSnapshot)
		{
			streamWriter.Flush();
			System.Threading.Thread.Sleep(100);
			RecordingStop ();
		}
		return true;
	}

	public override Result VideoCaptureStart()
	{
		if(process == null || streamWriter == null)
			return new Result (false, "", programMplayerClosed);

		if(! recordStartOrEndDo())
			return new Result (false, "", programMplayerCannotSave);

		return new Result (true, "");
	}

	/* unused on ffmpeg
	public override Result VideoCaptureEnd()
	{
		if(process == null || streamWriter == null)
			return new Result (false, "", programMplayerClosed);

		//System.Threading.Thread.Sleep(2000); //TODO: play with this to see if cut better video at end
		if(! recordStartOrEndDo())
			return new Result (false, "", programMplayerCannotSave);

		return new Result (true, "");
	}
	*/


	public override Result SaveFile (int sessionID, Constants.TestTypes testType, int testID, bool moveTempFiles)
	{
		if(! findIfThereAreImagesToConvert())
			return new Result (false, "", Constants.VideoNothingCapturedStr());

		//Convert video to the name and format expected
		if(! convertImagesToVideo())
			return new Result (false, "", programFfmpegNotInstalled);

		if(! moveTempFiles)
			return new Result (true, "");

		//Copy the video to expected place
		if (! Util.CopyTempVideo(sessionID, testType, testID))
			return new Result (false, "", Constants.FileCopyProblemStr());

		//Delete temp photos and video
		deleteTempFiles();

		return new Result (true, "");
	}

	public override void RecordingStop ()
	{
		try {
			streamWriter.Write('q');
			streamWriter.Flush();
		} catch {
			//maybe Mplayer window has been closed by user
			process = null;
			Running = false;
			return;
		}
		System.Threading.Thread.Sleep(100);

		streamWriter = null;
		process = null;
		Running = false;
	}

	/*
	 * protected methods
	 */

	protected override void deleteTempFiles()
	{
		LogB.Information("Deleting temp files");
		var dir = new DirectoryInfo(Path.GetTempPath());
		foreach(var file in dir.EnumerateFiles(
					Constants.PhotoTemp + "-" + videoDeviceToFilename() + "-" + "*" +
					Util.GetMultimediaExtension(Constants.MultimediaItems.PHOTOPNG)))
			file.Delete();

		LogB.Information("Deleting temp video");
		if(File.Exists(Util.GetVideoTempFileName()))
			File.Delete(Util.GetVideoTempFileName());
	}

	/*
	 * private methods
	 */

	private bool snapshotDo()
	{
		try {
			streamWriter.Write('s');
		} catch {
			//maybe Mplayer window has been closed by user
			return false;
		}
		return true;
	}

	private bool recordStartOrEndDo()
	{
		try {
			streamWriter.Write('S');
		} catch {
			//maybe Mplayer window has been closed by user
			return false;
		}
		return true;
	}

	private bool findIfThereAreImagesToConvert()
	{
		return (File.Exists(Util.GetWebcamPhotoTempFileNamePre() + "0001.png"));
	}

	private bool convertImagesToVideo()
	{
		executable = "ffmpeg";
		List<string> parameters = new List<string>();
		//ffmpeg -framerate 20 -y -i chronojump-last-photo%04d.png output.mp4
		parameters.Insert (0, "-framerate");
		parameters.Insert (1, "20");
		parameters.Insert (2, "-y"); //force overwrite without asking
		parameters.Insert (3, "-i"); //input files
		parameters.Insert (4, Util.GetWebcamPhotoTempFileNamePre() + "%04d.png");
		parameters.Insert (5, Util.GetVideoTempFileName());

		ExecuteProcess.Result execute_result = ExecuteProcess.run (executable, parameters, true, true);
		return execute_result.success;
	}

}
