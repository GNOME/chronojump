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
//do not use gtk or gdk because this class is used by the server
//see UtilGtk for eg color definitions

public class Constants
{
	public enum TestTypes { JUMP, JUMP_RJ, RUN, RUN_I, RT, PULSE, MULTICHRONOPIC, ENCODER }
        public static string FileCopyProblem = "Error. Cannot copy file.";
        public static string FileCannotSave = "Error. File cannot be saved.";
        public static string VideoTemp = "chronojump-last-video";
        public enum MultimediaItems {
                PHOTO, PHOTOPNG, VIDEO
        }   

	public const string ExtensionPhoto = ".jpg";
        public const string ExtensionPhotoPng = ".png"; //used for Cairo resized images
        public const string ExtensionVideo = ".mp4";
}
