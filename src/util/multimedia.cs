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
using System.Text; //StringBuilder
using System.Collections; //ArrayList
using System.Collections.Generic; //List
using System.Diagnostics; 	//for detect OS
using System.IO; 		//for detect OS
using Cairo;
using Gdk;

//this class tries to be a space for methods that are used in different classes
public class UtilMultimedia
{
	/*
	 * VIDEO
	 */

	public static WebcamDeviceList GetVideoDevices ()
	{
		WebcamFfmpegGetDevices w;

		if(UtilAll.GetOSEnum() == UtilAll.OperatingSystems.LINUX)
			w = new WebcamFfmpegGetDevicesLinux();
		else if(UtilAll.GetOSEnum() == UtilAll.OperatingSystems.WINDOWS)
			w = new WebcamFfmpegGetDevicesWindows();
		else
			w = new WebcamFfmpegGetDevicesMac();

		WebcamDeviceList wd_list = w.GetDevices();

		return wd_list;
	}


	/*
	 * IMAGES
	 */

	public enum ImageTypes { UNKNOWN, PNG, JPEG }
	public static ImageTypes GetImageType(string filename)
	{
		if(filename.ToLower().EndsWith("jpeg") || filename.ToLower().EndsWith("jpg"))
			return ImageTypes.JPEG;
		else if(filename.ToLower().EndsWith("png"))
			return ImageTypes.PNG;

		return ImageTypes.UNKNOWN;
	}

	public static void ResizeImageTests()
	{
		string file1 = "/home/xavier/Imatges/tenerife.png";
		string file2 = "/home/xavier/Imatges/2016-06-07-152244.jpg";
		string file3 = "/home/xavier/Imatges/humor-desmotivaciones-ofender-lasers.jpeg";

		LoadAndResizeImage(file1, "/home/xavier/Imatges/prova1.png", 100, 150);
		LoadAndResizeImage(file2, "/home/xavier/Imatges/prova2.png", 200, 200);
		LoadAndResizeImage(file3, "/home/xavier/Imatges/prova3.png", 400, 350);
	}
	
	public static bool LoadAndResizeImage(string filenameOriginal, string filenameDest, int width, int height)
	{
		/*
		 * to avoid problems on windows with original images on remote hard disk,
		 * 1st copy to temp and then resize
		 */

		string tempfile = System.IO.Path.GetTempFileName();
		ImageSurface imgSurface;

		if(GetImageType(filenameOriginal) == ImageTypes.PNG)
		{
			//imgSurface = LoadPngToCairoImageSurface(filenameOriginal);
			tempfile += ".png";
			File.Copy(filenameOriginal, tempfile, true);
			imgSurface = LoadPngToCairoImageSurface(tempfile);
		}
		else if(GetImageType(filenameOriginal) == ImageTypes.JPEG)
		{
			//imgSurface = LoadJpegToCairoImageSurface(filenameOriginal);
			tempfile += ".jpg";
			File.Copy(filenameOriginal, tempfile, true);
			imgSurface = LoadJpegToCairoImageSurface(tempfile);
		}
		else //(GetImageType(filenameOriginal) == ImageTypes.UNKNOWN)
		{
			return false;
		}

		bool success = ImageSurfaceResize(imgSurface, filenameDest, width, height);
		return success;
	}

	public static ImageSurface LoadJpegToCairoImageSurface(string jpegFilename)
	{
		Gdk.Pixbuf pixbuf = new Pixbuf (jpegFilename); //from a file
		return pixbufToCairoImageSurface(pixbuf);
	}

	// Thanks to: Chris Thomson
	// https://stackoverflow.com/questions/25106063/how-do-i-draw-pixbufs-onto-a-surface-with-cairo-sharp
	private static ImageSurface pixbufToCairoImageSurface(Pixbuf pixbuf)
	{
		ImageSurface imgSurface = new ImageSurface(Format.ARGB32, pixbuf.Width, pixbuf.Height);

		using (Cairo.Context cr = new Cairo.Context(imgSurface)) {
			Gdk.CairoHelper.SetSourcePixbuf (cr, pixbuf, 0, 0);
			cr.Paint ();
			cr.Dispose ();
		}

		return imgSurface;
	}

	public static ImageSurface LoadPngToCairoImageSurface (string pngFilename)
	{
		Cairo.ImageSurface imgSurface = new Cairo.ImageSurface(pngFilename);
                Context cr = new Context(imgSurface);
                cr.SetSource(imgSurface);
                cr.Paint();
		cr.Dispose ();

		return imgSurface;
	}

	//height can be -1 to maintain aspect ratio
	public static bool ImageSurfaceResize(ImageSurface imgSurface, string filename_dest,
			int width, int height)
	{
		//maintain aspect ratio
		if(height == -1)
		{
			double ratioOriginal = imgSurface.Width / (1.0 * imgSurface.Height);
			height = Convert.ToInt32( width / ratioOriginal);
		}

		//return if problems on calculating aspect ratio
		if(width <= 0 || height <= 0)
			return false;

		Surface surfaceResized = scale_surface(
				imgSurface, imgSurface.Width, imgSurface.Height, width, height);

		LogB.Information("ImageFileResize - " + filename_dest);
		try {
			surfaceResized.WriteToPng(filename_dest);
		} catch {
			LogB.Warning("Catched at ImageFileResize");
			return false;
		}

		return true;
	}

        // Thanks to: Owen Taylor
	// https://lists.freedesktop.org/archives/cairo/2006-January/006178.html
	private static Surface scale_surface (Surface old_surface,
			int old_width, int old_height,
			int new_width, int new_height)
	{
		Surface new_surface = old_surface.CreateSimilar(Cairo.Content.ColorAlpha, new_width, new_height);
		Context cr = new Context (new_surface);

		/* Scale *before* setting the source surface (1) */
		cr.Scale ((double)new_width / old_width, (double)new_height / old_height);
		cr.SetSourceSurface (old_surface, 0, 0);

		/* To avoid getting the edge pixels blended with 0 alpha, which would
		 * occur with the default EXTEND_NONE. Use EXTEND_PAD for 1.2 or newer (2)
		 */
		Cairo.Pattern pattern = new SurfacePattern (old_surface);
		pattern.Extend = Cairo.Extend.Reflect;

		/* Replace the destination with the source instead of overlaying */
		cr.Operator = Cairo.Operator.Source;

		/* Do the actual drawing */
		cr.Paint();

		cr.Dispose();

		return new_surface;
	}

}
