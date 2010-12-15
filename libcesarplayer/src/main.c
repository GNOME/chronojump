/* -*- Mode: C; indent-tabs-mode: t; c-basic-offset: 4; tab-width: 4 -*- */
/*
 * main.c
 * Copyright (C) Andoni Morales Alastruey 2008 <ylatuya@gmail.com>
 * 
 * main.c is free software: you can redistribute it and/or modify it
 * under the terms of the GNU General Public License as published by the
 * Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * main.c is distributed in the hope that it will be useful, but
 * WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License along
 * with this program.  If not, see <http://www.gnu.org/licenses/>.
 */




#include <gtk/gtk.h>
#include "gst-video-capturer.h"
#include <stdlib.h>
#include <unistd.h>


static int i = 0;
static gboolean
window_state_event (GtkWidget * widget, GdkEventWindowState * event,
    gpointer gvc)
{
  i++;
  g_print ("%d\n", i);
  if (i == 3) {
    gst_video_capturer_rec (GST_VIDEO_CAPTURER (gvc));

  }
  if (i == 5)
    gst_video_capturer_stop (GST_VIDEO_CAPTURER (gvc));
  return TRUE;
}

GtkWidget *
create_window (GstVideoCapturer * gvc)
{
  GtkWidget *window;




  /* Create a new window */
  window = gtk_window_new (GTK_WINDOW_TOPLEVEL);
  gtk_window_set_title (GTK_WINDOW (window), "Capturer");

  g_signal_connect (G_OBJECT (window), "window-state-event",
      G_CALLBACK (window_state_event), gvc);


  return window;
}


int
main (int argc, char *argv[])
{
  GtkWidget *window;
  GstVideoCapturer *gvc;
  GError *error = NULL;


  gtk_init (&argc, &argv);

  /*Create GstVideoCapturer */
  gst_video_capturer_init_backend (&argc, &argv);
  gvc = gst_video_capturer_new (GVC_USE_TYPE_DEVICE_CAPTURE, &error);
  //gvc = gst_video_capturer_new (GVC_USE_TYPE_VIDEO_TRANSCODE, &error );
  //g_object_set(gvc,"input_file","/home/andoni/Escritorio/RC Polo vs CD Complutense.avi",NULL);
  g_object_set (gvc, "output_file", "/home/andoni/jander.avi", NULL);
  //gvc = gst_video_capturer_new (GVC_USE_TYPE_TEST, &error );

  window = create_window (gvc);

  gtk_container_add (GTK_CONTAINER (window), GTK_WIDGET (gvc));
  gtk_widget_show (GTK_WIDGET (gvc));
  gtk_widget_show (window);




  gtk_main ();

  return 0;
}
