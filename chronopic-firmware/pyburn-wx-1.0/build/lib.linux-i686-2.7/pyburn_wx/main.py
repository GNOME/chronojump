#!/usr/bin/env python
# -*- coding: iso-8859-15 -*-

# Description: Example of use of libIris
# Copyright (C) 2008 by Juan Gonzalez 
# Author: Juan Gonzalez <juan@iearobotics.com>

# This program is free software; you can redistribute it and/or
# modify it under the terms of the GNU Library General Public
# License as published by the Free Software Foundation; either
# version 2 of the License, or (at your option) any later version.

# This program is distributed in the hope that it will be useful,
# but WITHOUT ANY WARRANTY; without even the implied warranty of
# MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
# Library General Public License for more details.

# You should have received a copy of the GNU General Public License
# along with this program; if not, write to the Free Software
# Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.

import wx
import os
import sys

#-- Modulos de Stargate
import libStargate.Picp

#-- Modulos de LibIris
import libIris.Pic16_Bootloader
import libIris.Pic16_Firmware
import libIris.IntelHex

#-- Bootloader. Obtenido con:
#-- hex2python PIC16_bootloader_1.2.hex

bootloader=[[0x0000, 0x0000, 0x158A, 0x160A, 0x2E88],[0x1E80, 0x158A, 0x160A, 0x2E80, 0x018A, 0x158A, 0x160A, 0x2E83, 0x0000, 0x1E03, 0x2E83, 0x1683, 0x3087, 0x0086, 0x1283, 0x0186, 0x3090, 0x0098, 0x1683, 0x1518, 0x1698, 0x301F, 0x0099, 0x1283, 0x3003, 0x00FA, 0x3031, 0x0090, 0x140C, 0x26F1, 0x3AEA, 0x1D03, 0x2EB4, 0x01FA, 0x2EBB, 0x26F1, 0x00F9, 0x3AE3, 0x1903, 0x2EBD, 0x0879, 0x3AEA, 0x1903, 0x2EBB, 0x0879, 0x3AED, 0x1D03, 0x2EA2, 0x30E4, 0x26EC, 0x3003, 0x00FA, 0x26F1, 0x0190, 0x0198, 0x1683, 0x0198, 0x1283, 0x018C, 0x2E83, 0x30EB, 0x2EEA, 0x26F1, 0x00F6, 0x26F1, 0x00F5, 0x26F1, 0x00F1, 0x00FB, 0x26F1, 0x00F3, 0x01F2, 0x3021, 0x1283, 0x1303, 0x0276, 0x1903, 0x2ED6, 0x0875, 0x3903, 0x00FC, 0x00FD, 0x1003, 0x0DFC, 0x3020, 0x077C, 0x2ED7, 0x3020, 0x0084, 0x26F1, 0x0080, 0x07F2, 0x0A84, 0x0BFB, 0x2ED8, 0x0872, 0x0673, 0x30E8, 0x1D03, 0x2EEA, 0x30E7, 0x26EC, 0x2703, 0x3800, 0x30E4, 0x1903, 0x30E5, 0x26EC, 0x2EA2, 0x0064, 0x1E0C, 0x2EEC, 0x0099, 0x0008, 0x0064, 0x087A, 0x1903, 0x2EFF, 0x1C0C, 0x2EFF, 0x1010, 0x0BFA, 0x2EFB, 0x3400, 0x100C, 0x300B, 0x008F, 0x1410, 0x1E8C, 0x2EF1, 0x081A, 0x0008, 0x3021, 0x0276, 0x1903, 0x2F55, 0x1683, 0x1703, 0x178C, 0x1283, 0x1303, 0x0875, 0x39FC, 0x1703, 0x008D, 0x1303, 0x0876, 0x1703, 0x008F, 0x3020, 0x0084, 0x1303, 0x087D, 0x1903, 0x2F2D, 0x1683, 0x1703, 0x140C, 0x0000, 0x0000, 0x1283, 0x080E, 0x0080, 0x0A84, 0x080C, 0x0080, 0x0A84, 0x0A8D, 0x1303, 0x03FD, 0x03F5, 0x0AF1, 0x0AF1, 0x2F16, 0x0871, 0x00FC, 0x3E20, 0x0084, 0x1003, 0x0CFC, 0x0876, 0x1703, 0x008F, 0x1303, 0x0875, 0x1703, 0x008D, 0x1303, 0x087C, 0x1703, 0x078D, 0x1903, 0x0A8F, 0x1703, 0x080D, 0x3903, 0x1903, 0x2F55, 0x1683, 0x140C, 0x0000, 0x0000, 0x1283, 0x080E, 0x0080, 0x0A84, 0x080C, 0x0080, 0x0A84, 0x0A8D, 0x1303, 0x0AF1, 0x0AF1, 0x2F40, 0x0875, 0x00F7, 0x0876, 0x00F8, 0x01FB, 0x3002, 0x00F4, 0x0871, 0x027B, 0x1803, 0x3401, 0x087B, 0x3E20, 0x0084, 0x3021, 0x0278, 0x1703, 0x1683, 0x1903, 0x2F73, 0x178C, 0x0183, 0x301E, 0x0278, 0x3080, 0x1903, 0x0277, 0x1803, 0x2FBB, 0x2F75, 0x138C, 0x0183, 0x0877, 0x1703, 0x008D, 0x1303, 0x0878, 0x1D03, 0x2F8A, 0x3004, 0x0277, 0x1803, 0x2F8A, 0x1703, 0x1683, 0x1F8C, 0x2F87, 0x1283, 0x3084, 0x078D, 0x0183, 0x301E, 0x2F8B, 0x0878, 0x1703, 0x008F, 0x0800, 0x008E, 0x0A84, 0x0800, 0x008C, 0x1683, 0x150C, 0x3055, 0x008D, 0x30AA, 0x008D, 0x148C, 0x0000, 0x0000, 0x0183, 0x1683, 0x1703, 0x1B8C, 0x2FC3, 0x1283, 0x1303, 0x0064, 0x1E0D, 0x2FA2, 0x120D, 0x1683, 0x1703, 0x110C, 0x140C, 0x0000, 0x0000, 0x1283, 0x0384, 0x0800, 0x060E, 0x1D03, 0x2FB7, 0x0A84, 0x0800, 0x060C, 0x1903, 0x2FBB, 0x0183, 0x0BF4, 0x2F5C, 0x3400, 0x1283, 0x1303, 0x3002, 0x07FB, 0x0AF7, 0x1903, 0x0AF8, 0x2F5A, 0x1283, 0x1303, 0x120D, 0x1683, 0x1703, 0x110C, 0x1283, 0x1703, 0x080D, 0x3903, 0x3C03, 0x1D03, 0x2FBB, 0x3003, 0x028D, 0x1283, 0x1303, 0x3004, 0x00FC, 0x3007, 0x0284, 0x0000, 0x1683, 0x1703, 0x140C, 0x0000, 0x0000, 0x0000, 0x0000, 0x0000, 0x0000, 0x0800, 0x1283, 0x1703, 0x060E, 0x1D03, 0x2FED, 0x0A84, 0x0800, 0x060C, 0x1903, 0x2FF4, 0x1283, 0x1303, 0x30FC, 0x05F7, 0x3006, 0x02FB, 0x2FB7, 0x1283, 0x1703, 0x0A8D, 0x0A84, 0x1283, 0x1303, 0x0BFC, 0x2FD8, 0x2FBB],[0x2000, 0x0002, 0x0001, 0x0000, 0x0000, 0x0000, 0x0000, 0x0000, 0x3F32],]


#-- Estados para el interfaz
GUI_INICIAL    = 0
GUI_OK_SERIAL  = 1
GUI_OK_PICP    = 2
GUI_PROGESS    = 3

#-- Timeout para la realizacion del PING a la Skypic. En mili-segundos
PING_TIMEOUT = 500

#-- Timeout para la deteccion del bootloader (en segundos)
#-- Usado para la descarga del Firmware PICP
TIMEOUT = 800

#-- Valor por defecto de la palabra de configuracion
DEFAULT_CONFIG_WORD = 0x3F3A

#--------------------------------------------------------------------
#-- Obtener la palara de configuracion de un fichero .hex parseado
#-- Esta funcion deberia estar en la libIris
#-- DEVUELVE:
#--  * El valor de palabra de configuracion (si existe)
#--  * -1 si no existe
#--------------------------------------------------------------------
def get_config(mem):
  for dir,(vh,vl) in mem:
    if dir==0x2007:
      return vh*256 + vl
      
  return -1

#-------------------------------------------------------------------
#-- Determinar el directorio desde donde se esta ejecutando pyburn
#-------------------------------------------------------------------
def determine_path ():
    """Borrowed from wxglade.py"""
    try:
        root = __file__
        if os.path.islink (root):
            root = os.path.realpath (root)
        return os.path.dirname (os.path.abspath (root))
    except:
        print "I'm sorry, but something is wrong."
        print "There is no __file__ variable. Please contact the author."
        sys.exit ()


#---------------------------------------------------
#-- Dialogo de About
#-- Generado con wxglade
#---------------------------------------------------
class MyDialog(wx.Dialog):
    def __init__(self, *args, **kwds):
        # begin wxGlade: MyDialog.__init__
        kwds["style"] = wx.DEFAULT_DIALOG_STYLE
        wx.Dialog.__init__(self, *args, **kwds)
        self.text_ctrl_1 = wx.TextCtrl(self, -1, "Pyburn 1.0\nGrabacion de microcontroladores PICs usando la Skypic\nhttp://www.iearobotics.com/wiki/index.php?title=Pyburn\n\n(c) Juan Gonzalez, Sep-2008\nLICENCIA GPL\n\nLibrerias libIris y libStargate:\n(c) Rafael Trevino y Juan Gonzalez\n", style=wx.TE_MULTILINE|wx.TE_READONLY|wx.TE_RICH|wx.TE_AUTO_URL|wx.NO_BORDER)
        self.static_line_5 = wx.StaticLine(self, -1)
        self.button_1 = wx.Button(self, -1, "OK")

        self.__set_properties()
        self.__do_layout()

        self.Bind(wx.EVT_BUTTON, self.about_ok_clicked, self.button_1)
        # end wxGlade

    def __set_properties(self):
        # begin wxGlade: MyDialog.__set_properties
        self.SetTitle("Sobre Pyburn")
        self.text_ctrl_1.SetMinSize((400, 200))
        self.text_ctrl_1.SetBackgroundColour(wx.Colour(246, 244, 241))
        # end wxGlade

    def __do_layout(self):
        # begin wxGlade: MyDialog.__do_layout
        sizer_13 = wx.BoxSizer(wx.VERTICAL)
        sizer_13.Add(self.text_ctrl_1, 0, wx.EXPAND, 0)
        sizer_13.Add(self.static_line_5, 0, wx.EXPAND, 0)
        sizer_13.Add(self.button_1, 0, wx.EXPAND|wx.ADJUST_MINSIZE, 0)
        self.SetAutoLayout(True)
        self.SetSizer(sizer_13)
        sizer_13.Fit(self)
        sizer_13.SetSizeHints(self)
        self.Layout()
        # end wxGlade

    def about_ok_clicked(self, event): # wxGlade: MyDialog.<event_handler>
        #print "OK!"
        self.EndModal(0)

# end of class MyDialog


#---------------------------------------------------
#-- Clase principal, que implementa el interfaz
#-- Creada con wxglade
#---------------------------------------------------
class MyFrame(wx.Frame):
  def __init__(self, app, *args, **kwds):
  
  
      #----------------------------------------------------------------
      #--- Esta parte del codigo se ha generado automaticamente con la
      #-- herramienta wxglade. NO modificar.
      #---------------------------------------------------------------
      # begin wxGlade: MyFrame.__init__
      kwds["style"] = wx.DEFAULT_FRAME_STYLE
      wx.Frame.__init__(self, *args, **kwds)
      self.panel_1 = wx.Panel(self, -1)
      self.notebook_1 = wx.Notebook(self.panel_1, -1, style=0)
      self.notebook_1_pane_2 = wx.Panel(self.notebook_1, -1)
      self.sizer_3_staticbox = wx.StaticBox(self.panel_1, -1, "Skypic grabadora")
      self.notebook_1_pane_1 = wx.Panel(self.notebook_1, -1)
      
      # Menu Bar
      self.frame_1_menubar = wx.MenuBar()
      self.SetMenuBar(self.frame_1_menubar)
      self.Help = wx.Menu()
      self.About = wx.MenuItem(self.Help, wx.NewId(), "Sobre Pyburn", "", wx.ITEM_NORMAL)
      self.Help.AppendItem(self.About)
      self.frame_1_menubar.Append(self.Help, "Ayuda")
      # Menu Bar end
      self.statusbar = self.CreateStatusBar(1, 0)
      self.label_2 = wx.StaticText(self.notebook_1_pane_1, -1, "Puerto serie:")
      self.entry_serial = wx.ComboBox(self.notebook_1_pane_1, -1, choices=[], style=wx.CB_DROPDOWN)
      self.button_abrir_cerrar = wx.Button(self.notebook_1_pane_1, -1, "  Abrir  ", style=wx.BU_EXACTFIT)
      self.static_line_1 = wx.StaticLine(self.notebook_1_pane_1, -1)
      self.label_3 = wx.StaticText(self.notebook_1_pane_1, -1, "Firmware:", style=wx.ALIGN_CENTRE)
      self.button_boot = wx.Button(self.notebook_1_pane_1, -1, "Bootloader", style=wx.BU_EXACTFIT)
      self.button_test = wx.Button(self.notebook_1_pane_1, -1, "Test 1", style=wx.BU_EXACTFIT)
      self.button_test2 = wx.Button(self.notebook_1_pane_1, -1, "Test 2", style=wx.BU_EXACTFIT)
      self.static_line_2 = wx.StaticLine(self.notebook_1_pane_1, -1)
      self.label_4 = wx.StaticText(self.notebook_1_pane_1, -1, "Fichero .hex:")
      self.entry_file = wx.TextCtrl(self.notebook_1_pane_1, -1, "")
      self.button_search = wx.Button(self.notebook_1_pane_1, -1, "Buscar", style=wx.BU_EXACTFIT)
      self.button_grabar = wx.Button(self.notebook_1_pane_1, -1, "Grabar!", style=wx.BU_EXACTFIT)
      self.static_line_3 = wx.StaticLine(self.notebook_1_pane_1, -1)
      self.progressbar = wx.Gauge(self.notebook_1_pane_1, -1, 100)
      self.button_cancel = wx.Button(self.notebook_1_pane_1, -1, "Cancelar", style=wx.BU_EXACTFIT)
      self.label_5 = wx.StaticText(self.notebook_1_pane_2, -1, "Palabra de configuracion:")
      self.entry_config = wx.TextCtrl(self.notebook_1_pane_2, -1, "3F3A")
      self.button_write_config = wx.Button(self.notebook_1_pane_2, -1, "Write", style=wx.BU_EXACTFIT)
      self.button_default = wx.Button(self.notebook_1_pane_2, -1, "Default", style=wx.BU_EXACTFIT)
      self.static_line_4 = wx.StaticLine(self.notebook_1_pane_2, -1)
      self.label_6 = wx.StaticText(self.notebook_1_pane_2, -1, "PIC destino:")
      self.button_check = wx.Button(self.notebook_1_pane_2, -1, "Detectar", style=wx.BU_EXACTFIT)
      self.icono_conexion = wx.StaticBitmap(self.panel_1, -1, wx.NullBitmap)
      self.label_conexion = wx.StaticText(self.panel_1, -1, "SIN CONEXION\n")
      self.button_picp = wx.Button(self.panel_1, -1, "Descargar Firmware")

      self.__set_properties()
      self.__do_layout()

      self.Bind(wx.EVT_MENU, self.EVT_MENU, self.About)
      self.Bind(wx.EVT_BUTTON, self.boton_abrir_cerrar_clicked, self.button_abrir_cerrar)
      self.Bind(wx.EVT_BUTTON, self.boot_clicked, self.button_boot)
      self.Bind(wx.EVT_BUTTON, self.test_clicked, self.button_test)
      self.Bind(wx.EVT_BUTTON, self.test2_clicked, self.button_test2)
      self.Bind(wx.EVT_TEXT, self.file_changed, self.entry_file)
      self.Bind(wx.EVT_BUTTON, self.search_clicked, self.button_search)
      self.Bind(wx.EVT_BUTTON, self.grabar_clicked, self.button_grabar)
      self.Bind(wx.EVT_BUTTON, self.button_cancel_clicked, self.button_cancel)
      self.Bind(wx.EVT_TEXT, self.config_changed, self.entry_config)
      self.Bind(wx.EVT_BUTTON, self.write_config_clicked, self.button_write_config)
      self.Bind(wx.EVT_BUTTON, self.default_clicked, self.button_default)
      self.Bind(wx.EVT_BUTTON, self.check_clicked, self.button_check)
      self.Bind(wx.EVT_BUTTON, self.picp_clicked, self.button_picp)
      # end wxGlade
      
      #-- Guardar la aplicacion
      self.app=app;

  def __set_properties(self):
      # begin wxGlade: MyFrame.__set_properties
      self.SetTitle("Pyburn-wx")
      self.statusbar.SetStatusWidths([-1])
      # statusbar fields
      statusbar_fields = ["Inicio"]
      for i in range(len(statusbar_fields)):
          self.statusbar.SetStatusText(statusbar_fields[i], i)
      self.entry_serial.SetMinSize((250, 30))
      self.entry_serial.SetToolTipString("Seleccionar puerto serie donde esta conectada la Skypic grabadora")
      self.button_abrir_cerrar.SetToolTipString("Abrir/cerrar el puerto serie")
      self.button_boot.SetToolTipString("Grabar el Bootloader en la Skypic destino")
      self.button_test.SetToolTipString("Grabar un programa de test en la skypic destino, que hace parpadear el led")
      self.button_test2.SetToolTipString("Grabar un programa de test en la skypic destino, que hacer parpadear el led")
      self.entry_file.SetMinSize((200, 27))
      self.button_search.SetToolTipString("Seleccionar el fichero .hex a grabar en la Skypic destino")
      self.button_grabar.SetToolTipString("Grabar el fichero .hex en la Skypic destino")
      self.progressbar.SetMinSize((350, 20))
      self.button_write_config.SetToolTipString("Grabar la palabra de configuracion en la Skypic destino")
      self.button_default.SetToolTipString("Establecer el valor por defecto de la palabra de configuracion")
      self.button_check.SetToolTipString("Detectar la skypic destino: el pic y su palabra de configuracion")
      self.icono_conexion.SetToolTipString(u"Indica si la Skypic grabadora está conectada o no. Si no está conectada o el firmware no es el correcto aparecera \"SIN CONEXION\"")
      self.button_picp.SetToolTipString("Descargar el Firmware necesario para que la Skypic conectada al puerto serie se convierta en grabadora")
      # end wxGlade

  def __do_layout(self):
      # begin wxGlade: MyFrame.__do_layout
      sizer_1 = wx.BoxSizer(wx.VERTICAL)
      sizer_2 = wx.BoxSizer(wx.VERTICAL)
      sizer_3 = wx.StaticBoxSizer(self.sizer_3_staticbox, wx.HORIZONTAL)
      sizer_4 = wx.BoxSizer(wx.HORIZONTAL)
      sizer_10 = wx.BoxSizer(wx.VERTICAL)
      sizer_12 = wx.BoxSizer(wx.HORIZONTAL)
      sizer_11 = wx.BoxSizer(wx.HORIZONTAL)
      sizer_5 = wx.BoxSizer(wx.VERTICAL)
      sizer_9 = wx.BoxSizer(wx.HORIZONTAL)
      sizer_8 = wx.BoxSizer(wx.HORIZONTAL)
      sizer_7 = wx.BoxSizer(wx.HORIZONTAL)
      sizer_6 = wx.BoxSizer(wx.HORIZONTAL)
      sizer_6.Add(self.label_2, 0, wx.ALL|wx.ALIGN_CENTER_VERTICAL|wx.ADJUST_MINSIZE, 5)
      sizer_6.Add(self.entry_serial, 0, wx.ALL|wx.ALIGN_CENTER_VERTICAL|wx.ADJUST_MINSIZE, 5)
      sizer_6.Add(self.button_abrir_cerrar, 0, wx.ALL|wx.ALIGN_CENTER_VERTICAL|wx.ADJUST_MINSIZE, 5)
      sizer_5.Add(sizer_6, 1, wx.EXPAND, 0)
      sizer_5.Add(self.static_line_1, 0, wx.EXPAND, 0)
      sizer_7.Add(self.label_3, 0, wx.ALL|wx.ALIGN_CENTER_VERTICAL|wx.ADJUST_MINSIZE, 5)
      sizer_7.Add(self.button_boot, 0, wx.ALL|wx.ALIGN_CENTER_VERTICAL|wx.FIXED_MINSIZE, 7)
      sizer_7.Add((20, 20), 0, wx.ADJUST_MINSIZE, 0)
      sizer_7.Add(self.button_test, 0, wx.TOP|wx.BOTTOM|wx.ALIGN_CENTER_VERTICAL|wx.FIXED_MINSIZE, 0)
      sizer_7.Add(self.button_test2, 0, wx.TOP|wx.BOTTOM|wx.ALIGN_CENTER_VERTICAL|wx.FIXED_MINSIZE, 0)
      sizer_5.Add(sizer_7, 1, wx.EXPAND, 0)
      sizer_5.Add(self.static_line_2, 0, wx.EXPAND, 0)
      sizer_8.Add(self.label_4, 0, wx.ALL|wx.ALIGN_CENTER_VERTICAL|wx.ADJUST_MINSIZE, 4)
      sizer_8.Add(self.entry_file, 0, wx.ALL|wx.ALIGN_CENTER_VERTICAL|wx.ADJUST_MINSIZE, 5)
      sizer_8.Add(self.button_search, 0, wx.ALL|wx.ALIGN_CENTER_VERTICAL|wx.ADJUST_MINSIZE, 4)
      sizer_8.Add(self.button_grabar, 0, wx.ALL|wx.ALIGN_CENTER_VERTICAL|wx.ADJUST_MINSIZE, 4)
      sizer_5.Add(sizer_8, 1, wx.EXPAND, 0)
      sizer_5.Add(self.static_line_3, 0, wx.EXPAND, 0)
      sizer_9.Add(self.progressbar, 0, wx.ALL|wx.ALIGN_CENTER_VERTICAL, 5)
      sizer_9.Add(self.button_cancel, 0, wx.ALL|wx.ALIGN_CENTER_VERTICAL|wx.ADJUST_MINSIZE, 5)
      sizer_5.Add(sizer_9, 1, wx.EXPAND, 0)
      self.notebook_1_pane_1.SetAutoLayout(True)
      self.notebook_1_pane_1.SetSizer(sizer_5)
      sizer_5.Fit(self.notebook_1_pane_1)
      sizer_5.SetSizeHints(self.notebook_1_pane_1)
      sizer_11.Add(self.label_5, 0, wx.ALL|wx.ALIGN_CENTER_VERTICAL|wx.ADJUST_MINSIZE, 5)
      sizer_11.Add(self.entry_config, 0, wx.ALL|wx.ALIGN_CENTER_VERTICAL|wx.ADJUST_MINSIZE, 5)
      sizer_11.Add(self.button_write_config, 0, wx.ALL|wx.ALIGN_CENTER_VERTICAL|wx.ADJUST_MINSIZE, 5)
      sizer_11.Add(self.button_default, 0, wx.ALL|wx.ALIGN_CENTER_VERTICAL|wx.ADJUST_MINSIZE, 0)
      sizer_10.Add(sizer_11, 0, wx.EXPAND, 0)
      sizer_10.Add(self.static_line_4, 0, wx.EXPAND, 0)
      sizer_12.Add(self.label_6, 0, wx.ALL|wx.ALIGN_CENTER_VERTICAL|wx.ADJUST_MINSIZE, 5)
      sizer_12.Add(self.button_check, 0, wx.ALL|wx.ALIGN_CENTER_VERTICAL|wx.ADJUST_MINSIZE, 5)
      sizer_10.Add(sizer_12, 0, wx.EXPAND, 0)
      self.notebook_1_pane_2.SetAutoLayout(True)
      self.notebook_1_pane_2.SetSizer(sizer_10)
      sizer_10.Fit(self.notebook_1_pane_2)
      sizer_10.SetSizeHints(self.notebook_1_pane_2)
      self.notebook_1.AddPage(self.notebook_1_pane_1, "Principal")
      self.notebook_1.AddPage(self.notebook_1_pane_2, "Avanzado")
      sizer_2.Add(self.notebook_1, 1, wx.ALL|wx.EXPAND, 4)
      sizer_4.Add(self.icono_conexion, 0, wx.LEFT|wx.RIGHT|wx.ALIGN_CENTER_VERTICAL, 5)
      sizer_4.Add(self.label_conexion, 0, wx.ALL|wx.ALIGN_CENTER_VERTICAL|wx.SHAPED, 10)
      sizer_4.Add(self.button_picp, 0, wx.RIGHT|wx.ALIGN_CENTER_VERTICAL, 10)
      sizer_3.Add(sizer_4, 1, wx.ALIGN_CENTER_VERTICAL, 0)
      sizer_2.Add(sizer_3, 0, wx.ALL|wx.EXPAND, 4)
      self.panel_1.SetAutoLayout(True)
      self.panel_1.SetSizer(sizer_2)
      sizer_2.Fit(self.panel_1)
      sizer_2.SetSizeHints(self.panel_1)
      sizer_1.Add(self.panel_1, 0, 0, 0)
      self.SetAutoLayout(True)
      self.SetSizer(sizer_1)
      sizer_1.Fit(self)
      sizer_1.SetSizeHints(self)
      self.Layout()
      # end wxGlade
      
      #--------------------------------------------
      # Fin del codigo generado automaticamente
      #--------------------------------------------
      
      #-- Limitar la entrada de texto de la palabra de configuracion 
      #-- a 4 digitos
      self.entry_config.SetMaxLength(4)
      
      #-------------------------------------------
      #- Encontrar la ruta a los iconos .xpm
      #-------------------------------------------
      PIXMAP_name=os.sep+"pixmaps";
      EXEC_dir = determine_path()
      IMAGE_ON_DIR = EXEC_dir  + PIXMAP_name + os.sep + "on.xpm"
      IMAGE_OFF_DIR= EXEC_dir  + PIXMAP_name + os.sep + "off.xpm"
      
      #-- Debug: imprimir los directorios donde buscar los iconos
      #print IMAGE_ON_DIR
      #print IMAGE_OFF_DIR
      
      
      #---------------------------------------
      #-- Variables globales
      #---------------------------------------
      
      #-- Stargate para la programacion
      self.picp=None;
      
      #-- Estado en el que se encuentra la GUI
      self.estado_GUI=None
      
      #--  Crear el temporizador
      self.timer = wx.Timer(self, 999999)
      self.Bind(wx.EVT_TIMER, self.skypic_check)
      
      #-- Cargar las imagenes del estado de la conexion
      self.imagen_on = wx.Bitmap(IMAGE_ON_DIR, wx.BITMAP_TYPE_XPM)
      self.imagen_off = wx.Bitmap(IMAGE_OFF_DIR, wx.BITMAP_TYPE_XPM)
      
      #-- Flag de cancelar
      self.cancelar=False
      
      #-- Palabras grabadas
      self.grabado=0;
      
      #-- Valor de la palabra de configuracion
      self.config_word=DEFAULT_CONFIG_WORD
      
      #-- Palabra de configuracion OK o no
      self.config_ok=True;
      
      #-- Fichero especificado existe o no
      self.file_ok=False
      
      #-- Otros
      self.interfaz_estado(GUI_INICIAL);
  
  #---------------------------------------------------------
  #-- Comprobacion periodica de la conexion con la Skypic  
  #-- La invoca el temporizador
  #---------------------------------------------------------
  def skypic_check(self,event):
    
    if self.picp:
    
      #-- Comprobar si hay conexion con el servidor y si
      #-- es del tipo correcto
      
      es_picp = self.picp.check_server_type()
      
      #--DEBUG: Para ver en la consola si esta activado o no
      #print "PICP: %s" % es_picp
      
      #-- Actualizar el interfaz en funcion de que se haya detectado o no
      if es_picp:
        self.interfaz_estado(GUI_OK_PICP)
      else:
        self.interfaz_estado(GUI_OK_SERIAL)      

  #---------------------------------------
  #-- Menu de About
  #----------------------------------------  
  def EVT_MENU(self, event): # wxGlade: MyFrame.<event_handler>
     d=MyDialog(None,-1)
     d.ShowModal()
     d.Destroy()

#-----------------------------------------------------------------------#
#                                                                       #
#     FUNCIONES DE RETROLLAMADA DE LOS ELEMENTOS DEL INTERFAZ           #
#                                                                       #
#-----------------------------------------------------------------------#
  
  
  #--------------------------------------------
  #- Boton de Abrir/Cerrar el puerto serie
  #--------------------------------------------
  def boton_abrir_cerrar_clicked(self, event):  # wxGlade: MyFrame.<event_handler>
    
    #-- Su funcionamiento depende del estado en el que esta el interfaz
    
    if self.estado_GUI==GUI_INICIAL:
      #-- En estado inicial la accion del boton es ABRIR
      self.open_port()
      
    else:
      #-- En el resto de estados la accion es CERRAR
      
      #-- Cerrarlo! 
      if self.picp:
        self.picp.close()
        
      #-- Actualizar interfaz
      self.interfaz_estado(GUI_INICIAL);

      #-- Actualizar barra de estado
      self.statusbar.SetStatusText("Puerto serie CERRADO",0);
      
      
  #-------------------------------------
  #-- Boton de buscar fichero apretado  
  #-------------------------------------
  def search_clicked(self, event): # wxGlade: MyFrame.<event_handler>
    
    #-- Abrir un dialogo de busqueda de ficheros
    filechooser = wx.FileDialog(self,wildcard = "*.hex;*.HEX")
    
    #-- Esperar a que el usuario seleccione el fichero
    opcion = filechooser.ShowModal()
    
    #-- Segun la opcion...
    if opcion == wx.ID_OK:
    
      #-- Se ha pulsado ok. Obtener el nombre del fichero
      fichero = filechooser.GetPath()
     
      #-- Meter el fichero en el entry "fichero .hex"
      self.entry_file.SetValue(fichero)
      
  #----------------------------------------------------------
  #-- Realizar la grabacion del fichero .hex seleccionado
  #----------------------------------------------------------
  def grabar_clicked(self,widget): # wxGlade: MyFrame.<event_handler>
    
    #-- Obtener el nombre del fichero a grabar
    file = str(self.entry_file.GetValue())
    
    #-- Realizar el parseo del fichero 
    try:
      hr = libIris.IntelHex.HexReader (file)
    except libIris.IntelHex.ReaderError,msg:
      self.statusbar.SetStatusText("%s" % msg)
      return      
     
    #-- Obtener la palabra de configuracion. Usar la establecida por defecto
    #-- si no existe    
    config = get_config(hr.memory())
    if config == -1:
      config = DEFAULT_CONFIG_WORD;
      
    #-- Poner la palabra de configuracion en el entry
    self.entry_config.SetValue("%04X" % config);    
    
    #-- Realizar la grabacion
    self.burn_program(hr.dataBlocks())
 
  #------------------------------------
  #-- Cambio en el nombre del fichero  
  #------------------------------------
  def file_changed(self,widget):  # wxGlade: MyFrame.<event_handler>
  
    #-- Leer nombre del fichero 
    file_name = str(self.entry_file.GetValue())
    
    #-- El estado del interfaz se cambia seguen que 
    #-- el fichero exista o no
    if not os.path.exists(file_name):
      self.file_ok=False
    else:
      self.file_ok=True
    
    #-- Actualizar el estado del interfaz
    if self.file_ok:
      self.button_grabar.Enable()
    else:
      self.button_grabar.Disable()
 

  #------------------------
  #-- Boton de cancelacion
  #------------------------ 
  def button_cancel_clicked(self, event): # wxGlade: MyFrame.<event_handler>
    #-- Poner el flag de cancelar a True y deshabilitar el boton
    self.cancelar=True
    self.button_cancel.Disable() 

  #----------------------------------
  #-- Grabar el programa de test 1
  #----------------------------------
  def test_clicked(self,widget):        # wxGlade: MyFrame.<event_handler>
    self.burn_program(libIris.Pic16_Firmware.ledp1)    
  
  #----------------------------------
  #-- Grabar el programa de test 2
  #----------------------------------
  def test2_clicked(self,widget):  # wxGlade: MyFrame.<event_handler>
    self.burn_program(libIris.Pic16_Firmware.ledp2) 
  
  #-----------------------------
  #-- Grabar el bootloader
  #-----------------------------
  def boot_clicked(self, event): # wxGlade: MyFrame.<event_handler>
    self.burn_program(bootloader)   
  
  
  #------------------------------------------
  #-- Boton Write (Pestana Avanzado)
  #-- Escribir palabra de configuracion 
  #------------------------------------------
  def write_config_clicked(self, event): # wxGlade: MyFrame.<event_handler>
    self.burn_config()
    

  #--------------------------------------------------------------------
  #-- Boton Default (Pestana Avanzado)
  #-- Establecer el valor por defecto de la palabra de configuracion 
  #--------------------------------------------------------------------
  def default_clicked(self, event): # wxGlade: MyFrame.<event_handler>
    self.entry_config.SetValue("%04X" % DEFAULT_CONFIG_WORD);
    
    
  #-----------------------------------------
  #-- Cambio en la palabra de confiuracion  
  #-----------------------------------------
  def config_changed(self, event): # wxGlade: MyFrame.<event_handler>
    #-- Leer el valor del entry
    config_str = self.entry_config.GetValue()
    #print "config_str: %s" % config_str
    
    #-- Comprobar si la cadena actual es valida o no
    try:
      #-- Convertir a un numero hexadecimal
      self.config_word = int (config_str, 16);
      
      #-- Indicar que palabra correcta
      self.config_ok=True;
      
    except ValueError:
    
      #-- Inicar que palabra incorrecta
      self.config_ok=False;
    
    #-- La palabra de configuracion 0000 es peligrosa...
    if self.config_ok:
      if self.config_word==0:
        self.statusbar.SetStatusText("CUIDADO!! Palabra de config. peligrosa")
      else:
        self.statusbar.SetStatusText(" ")
        
    #-- Actualizar estado del interfaz    
    self.interfaz_estado(GUI_OK_PICP,force=True);    
    
  
  #-------------------------------
  #-- Boton de Detectar
  #-- Comprobar el PIC destino
  #-------------------------------
  def check_clicked(self,widget): # wxGlade: MyFrame.<event_handler>
    self.check_pic();
  
  
  #-----------------------------
  #-- Grabar el PICP
  #-----------------------------
  def picp_clicked(self, event): # wxGlade: MyFrame.<event_handler>
    #print "Download picp"
    self.download_program(libIris.Pic16_Firmware.picp) 
  
  
#-----------------------------------------------------------------------#
#                                                                       #
#     FUNCIONES PARA IMPLEMENTAR LAS ACCIONES                           #
#                                                                       #
#-----------------------------------------------------------------------#
  
  #------------------------------------------------------------
  #-- Abrir el puerto serie leyendo el dispositivo del entry
  #-- DEVUELVE:
  #--   -True: Ok
  #--   -False: Error al abrir el puerto serie
  #------------------------------------------------------------
  def open_port(self):
    #-- Primero obtener el nombre del dispositivo serie
    serialName=self.entry_serial.GetValue()
    
    #-- Si habia un "Stargate" abierto, cerrarlo primero
    if self.picp:
      self.picp.close()
    
    #-- Abrir el puerto y crear el stargate
    try:
      self.picp = libStargate.Picp.Open(serialName, logCallback=None)
      self.statusbar.SetStatusText("OK. Puerto serie Abierto",0);
      
      #-- Cambiar el estado de la GUI
      self.interfaz_estado(GUI_OK_SERIAL);
      return True
      
    except libStargate.Main.Error, msg:
      self.picp = None
      
      #-- Si hay error indicarlo en la barra de estado y abortar
      self.statusbar.SetStatusText("%s" % msg, 0)
      
      #-- Cambiar estado del interfaz
      self.interfaz_estado(GUI_INICIAL);
      return False  
  
  
  #-----------------------------------------------------
  #-- Detectar el PIC destino
  #-- DEVUELVE:
  #--   True: Pic detectado
  #--   False: No detectado o error de comunicacion
  #-----------------------------------------------------
  def check_pic(self):
    try:
      id,config = self.picp.readConfig()
    except: 
      self.statusbar.SetStatusText("Error de comunicacion")
      return False;
      
    if id!=0x3FFF and id!=0x00:  
      self.statusbar.SetStatusText("PIC DETECTADO. ID: %04X. Palabra config: %04X"
                                    % (id,config))   
      return True;
    else:
      self.statusbar.SetStatusText("PIC DESTINO NO DETECTADO")   
      return False;
  
  #------------------------------------
  #-- Grabar palabra de configuracion
  #------------------------------------
  def burn_config(self):
  
    #-- Detectar pic destino. Si no esta se aborta
    pic_detected=self.check_pic();
    if (not pic_detected):
      #-- Actualizar barra de status
      self.statusbar.SetStatusText("ERROR: PIC no detectado. Config word no Grabada")
      self.update() 
      return;
  
    #-- Grabar palabra de configuracion
    try:
      self.picp.writeConfig(self.config_word);
    except:
      #-- Actualizar barra de status
      self.statusbar.SetStatusText("ERROR de comunicacion")
      self.update()    
      
    #-- Actualizar barra de status
    self.statusbar.SetStatusText("GRABADA PALABRA DE CONFIGURACION: %04X" 
                                  % self.config_word)      
  
  #-----------------------
  #-- Grabar firmware
  #-----------------------
  def burn_program(self,prog):  
    
    #-- Detectar pic destino. Si no esta se aborta
    pic_detected=self.check_pic();
    if (not pic_detected):
      return;   

    #-- Pasar al interfaz de "PROGRESO"
    self.interfaz_estado(GUI_PROGESS)
    
    #-- Inicialmente no hay cancelacion
    self.cancelar=False;
    
    #-- Grabar el programa
    try:
      ok=self.picp.writeProgram (prog,stateCallback=self.burn_state)
    except libStargate.Picp.Error,msg:
    
      #-- Actualizar barra de status
      self.statusbar.SetStatusText("%s" % msg)
      self.update()
      
      #-- Actualizar interfaz
      self.interfaz_estado(GUI_OK_PICP)
      
      return;
      
    #-- El ok nos indica si se ha grabado bien o no
    if not ok:
    
      #-- Actualizar barra de status
      self.statusbar.SetStatusText("ABORTADO")
      self.update()
      
      #-- Pasar al interfaz de "PICP"
      self.interfaz_estado(GUI_OK_PICP)
      
      return

      
    #-- Grabar palabra de configuracion
    try:
      self.picp.writeConfig(self.config_word);
    except:
      #-- Actualizar barra de status
      self.statusbar.SetStatusText("ERROR de comunicacion")
      self.update()
      return      
      
    #-- Actualizar barra de status
    self.statusbar.SetStatusText("GRABADA PALABRA DE CONFIGURACION: %04X" 
                                  % self.config_word)
    self.update()                              
    
    #-- Pasar al interfaz de "PICP"
    self.interfaz_estado(GUI_OK_PICP)
    

  #---------------------------------------------------------------------------#
  #-- Funcion de estado por defecto
  #-- Se invoca durante la grabacion
  #-- Implementa el comportamiento segun el estado de la grabacion en curso
  #---------------------------------------------------------------------------#
  def burn_state(self,op,inc,total):

    #--------------------------------------------------
    #- Comienzo de la grabacion
    #--------------------------------------------------
    if op==libStargate.Picp.WRITING_START:
    
      self.grabado=0;
    
      #-- Barra de progreso a cero
      self.progressbar.SetValue(0)

      #-- Actualizar barra de status
      self.statusbar.SetStatusText("GRABACION DEL PIC EN PROGRESO...")
      self.update()      
      return True
     
    
    #------------------------------------------------
    #--  Grabacion de una palabra
    #------------------------------------------------    
    elif op==libStargate.Picp.WRITING_INC:
    
      #-- Actualizar contador de bytes grabados
      self.grabado=self.grabado+inc;
    
      #-- Actualizar barra de progreso
      self.progressbar.SetValue(100*self.grabado/total) 
      self.update()
      
      #-- Comprobar si se ha apretado boton de Cancelar
      if self.cancelar:
        return False
        
      return True
      
    #------------------------------------
    #-- Fin de la grabacion
    #------------------------------------    
    elif op==libStargate.Picp.WRITING_END:
    
      #-- Actualizar barra de status
      self.statusbar.SetStatusText("OK...")
      self.update()
      return True
      
    #----------------------------------
    #-- Comienzo de la verificacion
    #----------------------------------    
    elif op==libStargate.Picp.VERIFYING_START:
      
      #-- Actualizar barra de status
      self.statusbar.SetStatusText("Verificacion de la grabacion...")
      
      #-- Barra de progreso a cero
      self.progressbar.SetValue(0) 
      self.update()
      
      #-- Contador de bytes a 0
      self.grabado=0;
      
      return True
      
    #-------------------------------------
    #-- Error de verificacion
    #-------------------------------------    
    elif op==libStargate.Picp.VERIFYING_ERROR:
    
      #-- Actualizar barra de status
      self.statusbar.SetStatusText("ERROR de verificacion")
      self.update()
      return False
      
    #-----------------------------------
    #--  Verificacion de una palabra    
    #------------------------------------
    elif op==libStargate.Picp.VERIFYING_INC:
      #-- Actualizar contador de bytes verificados
      self.grabado=self.grabado+inc;
    
      #-- Actualizar barra de progreso
      self.progressbar.SetValue(100*self.grabado/total) 
      self.update()
      
      #-- Comprobar si se ha apretado boton de Cancelar
      if self.cancelar:
        return False
      
      return True
      
    #--------------------------------------------
    #-- Fin de la verificacion
    #--------------------------------------------    
    elif op==libStargate.Picp.VERIFYING_END:
    
      #-- Actualizar barra de status
      self.statusbar.SetStatusText("VERIFICACION OK")
      self.progressbar.SetValue(100)
      self.update()
      
      return True
   
    #-----------------------
    #-- Evento desconocido  
    #-----------------------    
    else:
      print "DESCONOCIDO: %d" % op
      return False  

  

#-------------------------------------------------------------------------#
#                                                                         #
#   METODOS PARA LA DESCARGA DEL FIRMWARE PICP. INTERFAZ CON LA LIBIRIS   #
#                                                                         #
#-------------------------------------------------------------------------#
  

  #--------------------------------------------------------------
  #- Metodo principal para descargar un programa en la skypic
  #--------------------------------------------------------------
  def download_program(self,prog):
    #-- Poner la barra de progreso a 0
    self.progressbar.SetValue(0)
    self.update()
    
    #-- Desactivar flag de cancelacion
    self.cancelar=False
    
    #--------------------------------------------------------------------
    #-- Crear un objeto IRIS para la descarga de firmware. Se asocia
    #-- al puerto serie abierto
    #--------------------------------------------------------------------
    
    #-- Primero obtener el nombre del dispositivo serie
    serialName=self.entry_serial.GetValue()
    
    #-- NOTA para Windows: Hay que cerrar el puerto serie, ya que sino
    #-- al llamar al IRIS dara un error. En windows si un puerto serie ya
    #-- esta abierto, no se puede abrir ningun otro (ni siquiera el mismo)
    if self.picp:
      self.picp.close()
    
    try:
      self.iris = libIris.Pic16_Bootloader.Iris(serialName,logCallback=None)
    except libIris.Pic16_Bootloader.IrisError,msg:
    
      #-- Si hay error indicarlo en la barra de estado y abortar
      self.statusbar.SetStatusText("%s" % msg,0);
      return
    
    #-- Hay que esperar a que detecte el Bootloader
    self.statusbar.SetStatusText("Pulse Reset en la skypic",0);
    
    #-- Pasar al interfaz de "PROGRESO"
    self.interfaz_estado(GUI_PROGESS)
    self.update();
    
    #-- DESCARGAR EL FIRMWARE PICP!!!
    try:
      self.iris.download(prog,stateCallback=self.state)
    except libIris.Pic16_Bootloader.IrisError,msg:
      
      #-- NOTA para Windows: Idem. Es necesario cerrar el puerto serie
      self.iris.close()
      
      #-- Re-abrir el puerto serie
      self.open_port()
      self.statusbar.SetStatusText("%s" % msg,0);
      return
      
    #-- NOTA para Windows: Idem. Es necesario cerrar el puerto serie
    self.iris.close()    
      
    #-- SERVIDOR PICP cargado
    #-- Re-abrir el puerto serie
    self.open_port()
    
    self.statusbar.SetStatusText("Descarga completada",0);
    
      
  #------------------------------------------------------------------------
  #-- Funcion de retrollamada de libIris. Segun el estado de la descarga
  #-- Se hace una cosa u otra
  #------------------------------------------------------------------------  
  def state(self,op,inc,total):
    
    #-----------------------------
    #-- Comienzo de descarga
    #-----------------------------
    if op==libIris.Pic16_Bootloader.WRITING_START:

      #-- Barra de progreso a cero
      self.progressbar.SetValue(0)

      #-- Actualizar barra de status
      self.statusbar.SetStatusText("Descargando firmware...",0);
      self.update()      
      return True
      
    #------------------------------
    #-- Incremento en la descarga  
    #------------------------------    
    elif op==libIris.Pic16_Bootloader.WRITING_INC:  
      self.progressbar.SetValue(100*inc/total)      
      self.update()
      
      #-- Comprobar si se ha apretado boton de Cancelar
      if self.cancelar:
        return False
        
      return True

    #-------------------------------
    #-- Fin de la descarga
    #-------------------------------
    elif op==libIris.Pic16_Bootloader.WRITING_END: 
      self.progressbar.SetValue(100)
      self.statusbar.SetStatusText("Completado",0);
      self.update()
      return True
      
    #---------------------------------------------------
    #-- Comienzo de la identificacion del bootloader    
    #---------------------------------------------------    
    elif op==libIris.Pic16_Bootloader.IDENT_START:
      #-- Hay que esperar a que detecte el Bootloader
      self.statusbar.SetStatusText("Pulse Reset en la skypic", 0)
      self.update()
      return True
      
      
    #-----------------------------------------------------------------
    #-- Respuesta no recibida del bootloader tras un mini-timeout    
    #-----------------------------------------------------------------
    elif op==libIris.Pic16_Bootloader.IDENT_NACK:
    
      #-- Mientras que el tiempo total acumulado sea menor que el 
      #-- TIMEOUT indicado, continuar esperando
      self.update()
      
      #-- Si apretado boton de cancelar abortar...
      if self.cancelar:
        return False
      
      if total<=TIMEOUT:
       return True
      else :
        return False      

#-----------------------------------------------------------------------------#
#                                                                             #
#              FUNCIONES DE GESTION DEL INTERFAZ                              #
#                                                                             #
#-----------------------------------------------------------------------------#
  
  #------------------------------------------------
  #-- Procesar eventos pendientes del interfaz 
  #------------------------------------------------
  def update(self):
    while (self.app.Pending()):
        self.app.Dispatch();  
  
  #------------------------------------------------------------------------#
  #-- Establecer estado interfaz. Esta funcion determina que partes del   
  #-- interfaz estan activas en cada momento, segun la situacion
  #--
  #-- ESTADO DEL INTERFAZ
  #--
  #--   INICIAL:   Comienzo del programa. Puerto serie NO abierto
  #--   OK_SERIAL: Puerto serie abierto correctamente
  #--   OK_PICP:   Servidor PICP para la grabacion detectado
  #--   PROGRESS:  Barra de progreso actualizandose
  #-----------------------------------------------------------------------#
  def interfaz_estado(self,state,force=False):
    
    #-- Por razones de eficiencia, el interfaz solo se actualiza si
    #-- el nuevo estado es diferente del actual.
    #-- Si el Flag force esta activado se actualiza el interfaz sin
    #-- tener en cuenta esto
    if self.estado_GUI==state and force==False:
      return;
    
    #-- Cambiar al estado indicado
    self.estado_GUI=state;
    
    #-----------------------------
    #-- ESTADO INICIAL
    #-----------------------------
    if state==GUI_INICIAL:
      
      #-- Debug
      #print "GUI Estado: INICIAL"
      
      #--- Activar combo del puerto serie
      self.entry_serial.Enable();
      
      #-- El boton de Abrir es de Abrir
      self.button_abrir_cerrar.SetLabel("  Abrir  ")
      self.button_abrir_cerrar.Enable()
      
      #-- FICHERO .HEX
      self.entry_file.Disable()
      self.button_search.Disable()
      self.button_grabar.Disable()
      
      #-- FIRMWARE
      self.button_boot.Disable()
      self.button_test.Disable()
      self.button_test2.Disable()
      
      #-- STARGATE
      self.button_picp.Disable()
      
      #-- OTROS
      self.button_cancel.Disable()
     
      #-- PESTANA DE "AVANZADO"
      self.button_check.Disable()
      self.entry_config.Disable()
      self.button_write_config.Disable()
      self.button_default.Disable()
      
      #-- El temporizador en este estado debe estar parado
      if self.timer.IsRunning():
        self.timer.Stop()
        
      #-- no Hay conexion con PICP
      self.label_conexion.SetLabel("SIN CONEXION\n")
      self.icono_conexion.SetBitmap(self.imagen_off)
      
    #----------------------------
    #-- ESTADO OK_SERIAL
    #----------------------------    
    elif state==GUI_OK_SERIAL:
    
      #-- Debug
      #print "GUI Estado: GUI_OK_SERIAL"
      
      #-- Desactivar combo del puerto serie
      self.entry_serial.Disable();
      
      #-- El boton de Abrir es ahora de Cerrar
      self.button_abrir_cerrar.SetLabel("Cerrar")
      self.button_abrir_cerrar.Enable()
      
      #-- FICHERO .HEX
      self.entry_file.Disable()
      self.button_search.Disable()
      self.button_grabar.Disable()
      
      #-- FIRMWARE
      self.button_boot.Disable()
      self.button_test.Disable()
      self.button_test2.Disable()
      
      #-- Se permite descargar el servidor PICP
      self.button_picp.Enable()
      
      #-- OTROS
      self.button_cancel.Disable()
      
      #-- PESTANA DE "AVANZADO"
      self.button_check.Disable()
      self.entry_config.Disable()
      self.button_write_config.Disable()
      self.button_default.Disable()
      
      #-- El temporizador se Activa, para comprobar la conexion con 
      #-- la Skypic
      self.timer.Start(PING_TIMEOUT,wx.TIMER_CONTINUOUS)
      
      #-- no Hay conexion con PICP
      self.label_conexion.SetLabel("SIN CONEXION\n")
      self.icono_conexion.SetBitmap(self.imagen_off)
      
    
    #-------------------------
    #-- ESTADO OK_PICP
    #-------------------------    
    elif state==GUI_OK_PICP:
    
      #-- Debug
      #print "GUI Estado: GUI_OK_PICP"  

      #-- Desactivar combo del puerto serie
      self.entry_serial.Disable();
      
      #-- El boton de Abrir es ahora de Cerrar
      self.button_abrir_cerrar.SetLabel("Cerrar")
      self.button_abrir_cerrar.Enable()
      
      #-- Se permite descargar el servidor PICP
      self.button_picp.Disable()
      
      #-- FICHERO .HEX
      self.entry_file.Enable()
      self.button_search.Enable()
      
      #-- Actualizar el estado del interfaz
      if self.file_ok:
        self.button_grabar.Enable()
      else:
        self.button_grabar.Disable()
      
      
      #-- FIRMWARE
      self.button_boot.Enable()
      self.button_test.Enable()
      self.button_test2.Enable()
      
      #-- STARGATE
      self.button_picp.Disable()
      
      #-- OTROS
      self.button_cancel.Disable()
     
      #-- PESTANA DE "AVANZADO"
      self.button_check.Enable()
      self.entry_config.Enable()
      
      #-- Si la palabra de configuracion actual es distinta de la 
      #-- que hay por defecto, se activa el boton de default
      if self.config_word!=DEFAULT_CONFIG_WORD:
        self.button_default.Enable()
      else:
        self.button_default.Disable()  
    
      #-- El boton de write solo se activa si la palabra de 
      #-- config. es correcta.
      if self.config_ok:
        self.button_write_config.Enable()
      else:
        self.button_write_config.Disable()
      
      
      
      #-- El temporizador se Activa, para comprobar la conexion con 
      #-- la Skypic
      self.timer.Start(PING_TIMEOUT,wx.TIMER_CONTINUOUS)

      #-- Hay conexion con PICP: Indicarlo
      self.label_conexion.SetLabel("CONECTADA")
      self.icono_conexion.SetBitmap(self.imagen_on)
      
    #----------------------------
    #-- ESTADO: PROGRESS
    #----------------------------    
    elif state==GUI_PROGESS:
      #-- Debug
      #print "GUI Estado: GUI_PROGESS"
      
      #-- El temporizador en este estado debe estar parado
      if self.timer.IsRunning():
        self.timer.Stop()
        
      #-- Boton de cancelacion activa
      self.button_cancel.Enable() 

      #-- Boton de descarga deshabilitado
      self.button_picp.Disable()
      
      #--- Activar combo del puerto serie
      self.entry_serial.Disable()
      
      #-- Desactivar boton de abrir/cerrar
      self.button_abrir_cerrar.Disable()
      
      #-- FICHERO .HEX
      self.entry_file.Disable()
      self.button_search.Disable()
      self.button_grabar.Disable()
      
      #-- FIRMWARE
      self.button_boot.Disable()
      self.button_test.Disable()
      self.button_test2.Disable()
      
      #-- PESTANA DE "AVANZADO"
      self.button_check.Disable()
      self.entry_config.Disable()
      self.button_write_config.Disable()
      self.button_default.Disable()

    else:
      print "Estado GUI NO implementado"    
      

# end of class MyFrame


#---------------------------------------------------------
#-- Funcion para obtener  la lista de puerto serie
#-- Esto depende de la plataforma en la que se ejecute
#---------------------------------------------------------
def getSerialPorts():

  #-- Windows
  if os.name == 'nt':
    
    #-- Se usan los cinco primeros puertos serie
    return ["","COM1","COM2","COM3","COM4","COM5"]
    
  #-- Linux  
  elif os.name == 'posix':
    return ["","/dev/ttyS0","/dev/ttyS1","/dev/ttyUSB0","/dev/ttyUSB1"]

  else:
    return []


#---------------------------------------------------
#-  Aplicacion principal
#---------------------------------------------------
class MyApp(wx.App):
      
    def OnInit(self):
        self.iris=None
        frame = MyFrame(self,None, -1, "")
        frame.Show(True)
        self.SetTopWindow(frame)
        
        #-- Anadir los nombres de los puertos serie al combobox
        serialports = getSerialPorts()
        for disp in serialports:
          frame.entry_serial.Append(disp) 
            
        return True
        
    def OnExit(self):
        print "Fin..."


#-------------------------------
#-  METODO DE ENTRADA
#-------------------------------
def start():
    app = MyApp(0)
    app.MainLoop()


if __name__ == "__main__":
    start()
