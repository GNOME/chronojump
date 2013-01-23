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

/* Compile with:
 * gcc -o test-remuxer test-remuxer.c gst-remuxer.c `pkg-config --cflags --libs gstreamer-0.10` -DOSTYPE_LINUX -O0 -g
 */

#include <stdlib.h>
#include <unistd.h>
#include "gst-remuxer.h"

static gboolean
percent_done_cb (GstRemuxer *remuxer, gfloat percent, GMainLoop *loop)
{
  if (percent == 1) {
    g_print("SUCESS!\n");
    g_main_loop_quit (loop);
  } else {
    g_print("----> %f%%", percent);
  }
}

static gboolean
error_cb (GstRemuxer *remuxer, gchar *error, GMainLoop *loop)
{
    g_print("ERROR: %s\n", error);
    g_main_loop_quit (loop);
}

int
main (int argc, char *argv[])
{
  GstRemuxer *remuxer;
  GMainLoop *loop;

  gst_remuxer_init_backend (&argc, &argv);

  if (argc != 3) {
    g_print("Usage: test-remuxer input_file output_file\n");
    return 1;
  }
  remuxer = gst_remuxer_new (argv[1], argv[2], NULL);
  gst_remuxer_start (remuxer);

  loop = g_main_loop_new (NULL, FALSE);

  g_signal_connect (remuxer, "percent_completed",
      G_CALLBACK (percent_done_cb), loop);
  g_signal_connect (remuxer, "error",
      G_CALLBACK (error_cb), loop);

  g_main_loop_run (loop);

  return 0;
}
