/*
 * Copyright (C) 2004  Juan Gonzalez Gomez
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA 02111-1307, USA.
 */
/***************************************************************************/
/* chronopic-serial-conf.c                                                 */
/*-------------------------------------------------------------------------*/
/* Configurar el puerto serie para trabajar con chronopic                  */
/* Este programa tiene las siguientes funciones:                           */
/*                                                                         */
/*  1) Configurar el puerto serie para poder acceder desde el PC al        */
/*     cronometro (chronopic). La velocidad se establece a 9600 baudios y  */
/*     se usa la configuracion N81. Al dejarlo asi configurado, es posible */
/*     leer las tramas de cronojump directamente abriendo el dispositivo   */
/*     /dev/ttyUSB0. Resulta muy util para acceder a chronopic desde otro    */
/*     lenguaje de programacion distinto de C, como por ejemplo C#, Python */
/*     Java, etc...                                                        */
/*                                                                         */
/*  2) Tener disponibles las funciones en C, para poderlas utilizar        */
/*     desde otros programas en C, o crear una libreria especifica         */
/*-------------------------------------------------------------------------
 $Id: chronopic-serial-conf.c,v 1.1 2005/02/07 11:14:54 obijuan Exp $
 $Revision: 1.1 $
 $Source: /cvsroot/chronojump/chronopic/test/chronopic-serial-conf.c,v $
---------------------------------------------------------------------------*/

#include <stdio.h>
#include <unistd.h>
#include <string.h>

//-- Las funciones de manejo del puerto serie se encuentran en el 
//-- ficher sg-serial.c
#include "chronopic.h"

//-- Dispositivo serie
char disp_serie[80]; 


/**********************/
/* Imprimir la ayuda  */
/**********************/
void help(void)
{
  printf ("\n");
  printf (
    "Forma de uso:   chronopic-serial-conf dispositivo_serie [opciones]\n");
  printf ("Opciones:\n");
  printf ("    -h : Mostrar esta ayuda\n\n");
  printf ("Ejemplo: chronopic-serial-conf /dev/ttyUSB0\n");
  printf ("\n");
}

/*****************************************************************/
/* Analizar los parametros pasados desde la linea de comandos    */
/*---------------------------------------------------------------*/
/* ENTRADAS: Argumentos de la funcion main()                 	   */
/* DEVUELVE:                                                     */
/*    0 : Error                                                  */
/*    1 : OK                                                     */
/*****************************************************************/
int analizar_parametros(int argc, char* argv[])
{
  int c;

  if (argc<2) {
    printf ("\nERROR: No se ha especificado dispositivo serie\n");
    printf ("Utilice el parametro -h para obtener ayuda\n\n");
    return 0;
  }  
  
  //-- Obtener el dispositivo serie
  strcpy(disp_serie,argv[1]);
  
  //-- Obtener el resto de parametros
  while ((c = getopt(argc, argv, ":h"))!=EOF) {
    switch (c) {
      case 'h':
      default: help();
        return 0;
    }
  }
  return 1;
}

/*****************************/
/* Informacion del programa  */
/*****************************/
void presenta()
{
  printf ("\n");
	printf (
   "Chronopic-serial-conf. (c) Juan Gonzalez. Febrero 2005. Licencia GPL\n");
	printf (
   "Configuracion del puerto serie para trabajar con ChronoPIC\n");
  printf ("Proyecto ChronoJump\n");
	printf ("\n");
}

/*----------------*/
/*     MAIN       */
/*----------------*/
int main (int argc, char **argv)
{
  int serial_fd=0;
  int ok;
  
  presenta();
  
	ok=analizar_parametros(argc,argv);
	if (!ok) return 0;
  
  //-- Abrir el puerto serie. Por defecto se abre con los parametros
  //-- necesario en Chronopic
  serial_fd=chronopic_open(disp_serie);

  if (serial_fd==-1) {
    printf ("\n");
    printf ("Error al abrir puerto serie %s\n",argv[1]);
    perror("ERROR: ");
    return 1;
  }
  
  printf ("\n");
  printf ("-------------------------------------------\n");
  printf (" Puerto serie configurado  para Chronopic  \n");
  printf ("-------------------------------------------\n");
  
  //-- Cerrar puerto serie
  chronopic_close(serial_fd);
  
  return 0;
}
