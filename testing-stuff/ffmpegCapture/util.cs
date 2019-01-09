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
 *  Copyright (C) 2004-2017   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
//using System.Data;
using System.Text; //StringBuilder
using System.Collections; //ArrayList
using System.Collections.Generic; //List<T>
using System.Diagnostics; 	//for detect OS
using System.IO; 		//for detect OS
using System.Globalization; 	//Unicode

//this class tries to be a space for methods that are used in different classes
public class Util
{
	public static string GetManualDir() {
		//we are on:
		//lib/chronojump/ (Unix) or bin/ (win32)
		//we have to go to
		//share/doc/chronojump
		return System.IO.Path.Combine(Util.GetPrefixDir(),
			"share" + Path.DirectorySeparatorChar + "doc" + Path.DirectorySeparatorChar + "chronojump");
	}

	public static string GetPrefixDir(){
		string baseDirectory = System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "..");
		if (! Directory.Exists(Path.Combine(baseDirectory, "lib" + Path.DirectorySeparatorChar + "chronojump"))) {
			baseDirectory = System.IO.Path.Combine(baseDirectory, "..");
		}
		return baseDirectory;
	}

	public static string GetDataDir(){
		return System.IO.Path.Combine(GetPrefixDir(),
			"share" + Path.DirectorySeparatorChar + "chronojump");
	}

	public static string GetImagesDir(){
		return System.IO.Path.Combine(GetDataDir(),"images");
	}

	public static string GetVideoTempFileName() {
		return Path.Combine(
				Path.GetTempPath(), Constants.VideoTemp +
				GetMultimediaExtension(Constants.MultimediaItems.VIDEO));
	}

	public static bool CopyTempVideo(int sessionID, Constants.TestTypes type, int uniqueID) {
		string origin = GetVideoTempFileName();
		string destination = GetVideoFileName(sessionID, type, uniqueID);
		Console.WriteLine("destination is: " + destination);
		if(File.Exists(origin)) {
			CreateVideoSessionDirIfNeeded(sessionID);
			/*
			 * no more move it, just copy it, because maybe is still being recorded
			 try {
			 File.Move(origin, destination);
			 } catch {
			 */
			File.Copy(origin, destination, true); //can be overwritten
			//}
			return true;
		} else
			return false;
	}

	public static void DeleteVideo(int sessionID, Constants.TestTypes type, int uniqueID) {
		string fileName = GetVideoFileName(sessionID, type, uniqueID);
		if(File.Exists(fileName))
			File.Delete(fileName);
	}
        public static string GetMultimediaExtension (string filename)
        {
                if(UtilMultimedia.GetImageType(filename) == UtilMultimedia.ImageTypes.JPEG)
                        return Constants.ExtensionPhoto; 
                if(UtilMultimedia.GetImageType(filename) == UtilMultimedia.ImageTypes.PNG)
                        return Constants.ExtensionPhotoPng; 

                return "";
        }

        public static string GetMultimediaExtension (Constants.MultimediaItems multimediaItem) {
                if(multimediaItem == Constants.MultimediaItems.VIDEO)
                        return Constants.ExtensionVideo;
                else if(multimediaItem == Constants.MultimediaItems.PHOTO)
                        return Constants.ExtensionPhoto;
                else //multimediaItem == Constants.MultimediaItems.PHOTOPNG
                        return Constants.ExtensionPhotoPng;
        }

	        public static string GetVideoFileName (int sessionID, Constants.TestTypes testType, int uniqueID) {
                return GetVideoSessionDir(sessionID) + Path.DirectorySeparatorChar +
                        testType.ToString() + "-" + uniqueID.ToString() +
                        GetMultimediaExtension(Constants.MultimediaItems.VIDEO);
        }

		public static void CreateVideoSessionDirIfNeeded (int sessionID) {
                string sessionDir = GetVideoSessionDir(sessionID);
                if( ! Directory.Exists(sessionDir)) {
                        Directory.CreateDirectory (sessionDir);
                        Console.WriteLine ("created dir:", sessionDir);
                }
        }

        public static string GetVideoSessionDir (int sessionID) {
                return GetVideosDir() + Path.DirectorySeparatorChar + sessionID.ToString();
        }   

        public static string GetVideosDir() {
                return Path.Combine(
                                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                                "Chronojump" + Path.DirectorySeparatorChar + "multimedia" +
                                Path.DirectorySeparatorChar + "videos"); 
        }     	

	public static string GetLastPartOfPath (string fileName) {
		//gets a complete url with report directory path and return only last part of path
		//useful for linking images as relative and not absolute in export to HTML
		//works on win and linux
		int temp1 = fileName.LastIndexOf('\\');
		int temp2 = fileName.LastIndexOf('/');
		int posOfBar = 0;
		if(temp1>temp2)
			posOfBar = temp1;
		else
			posOfBar = temp2;

		string lastPartOfPath = fileName.Substring(posOfBar+1, fileName.Length - posOfBar -1);
		return lastPartOfPath;
	}
}
