/*
 * Copyright (C) 2005  Juan Gonzalez Gomez
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
/*-------------------------------------------------------------------------*/
/* Visualizacion de las tramas enviadas por ChronoPic. Es muy util para    */
/* hacer pruebas y comprobar si el hardware esta funcionando correctamente */
/*-------------------------------------------------------------------------*/
/*-------------------------------------------------------------------------
 $Id: test-tramas.c,v 1.1 2005/02/07 11:14:54 obijuan Exp $
 $Revision: 1.1 $
 $Source: /cvsroot/chronojump/chronopic/test/test-tramas.c,v $
---------------------------------------------------------------------------*/

#include <termios.h>
#include <unistd.h> 
#include <stdio.h>
#include <string.h>

#include "termansi.h"
#include "chronopic.h"

//-- Dispositivo serie
char disp_serie[80]; 
int flag_disp_serie=0;


/**********************/
/* Imprimir la ayuda  */
/**********************/
void help(void)
{
  printf ("\n");
  printf (
    "Forma de uso:   test-tramas [dispositivo_serie] [opciones]\n");
  printf ("\n");
  printf (
  "Si no se especifica dispositivo serie, se toma la entrada estandar\n");
  printf ("\n");
  printf ("Dispositivo serie: \n");
  printf ("   -Dispositivo donde esta conectado ChronoPic. Ej. /dev/ttyUSB0\n");
  printf ("\n");
  printf ("Opciones:\n");
  printf ("    -h : Mostrar esta ayuda\n");
  printf ("\n");
  printf ("Ejemplo1: test-tramas /dev/ttyUSB0\n");
  printf (
  "   -Analizar las tramas de Chronopic, conectado al puerto serie /dev/ttyUSB0\n");
  printf ("\n");
  printf ("Ejemplo2: test-tramas < prueba\n");
  printf (
  "   -Las tramas vienen de la entrada estandar, que se toma del fichero prueba");
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

  //-- Por defecto se toma la entrada estandar
  if (argc==1) {
    flag_disp_serie=0;
    return 1;
  }
  else flag_disp_serie=1;
  
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
	printf ("test-tramas. (c) Juan Gonzalez. Febrero 2005. Licencia GPL\n");
	printf ("Visualizacion de tramas de ChronoPic [Proyecto ChronoJump]\n");
  printf ("Ejecute test-tramas -h para OBTENER AYUDA\n");
	printf ("\n");
}

/*----------------*/
/*     MAIN       */
/*----------------*/
int main (int argc, char **argv)
{
  char cad_estado[4];
  char cad_tiempo[80];
  double t;
  int estado;
  int ok;
  int serial_fd;
  int n;
  
  presenta();
  
  ok=analizar_parametros(argc,argv);
	if (!ok) return 0;
  
  //-- Entrada: CONSOLA
  if (flag_disp_serie==0) {
    //-- configurar la consola
    serial_fd=STDIN_FILENO;
    printf ("Recepcion de Datos: ENTRADA ESTANDAR\n");
  }
  //-- Entrada: puerto serie  
  else {
    
    printf ("Recepcion de datos: PUERTO SERIE %s\n",disp_serie);
    
    //-- Abrir el puerto serie. Por defecto se abre con los parametros
    //-- necesarios en Chronopic
    serial_fd=chronopic_open(disp_serie);

    if (serial_fd==-1) {
      printf ("\n");
      printf ("Error al abrir puerto serie %s\n",argv[1]);
      perror("ERROR: ");
      return 1;
    }
  }
 
  
  printf ("\n");
  printf ("Pulse Control-C para terminar\n");
  printf ("------------------------------------\n");
  
  //-- Bucle infinido. Salir pulsando control-c
  do {
    //-- Esperar a que llegue una trama
    do {
      n = chronopic_get_trama_cambio(serial_fd,&t,&estado);
    } while(n==0);
    
    //-- Error en trama recibida: terminar
    if (n==-1) return 0;
    
    //-- Obtener la cadena de estado
    if (estado==0)
      sprintf(cad_estado,"OFF");
    else
      sprintf(cad_estado,"ON ");
    
    //-- Obtener la cadena de tiempo
    sprintf(cad_tiempo,"%8.1f ms",t);
    
    //-- Sacar informacion sobre el evento ocurrido
    //-- Primero el estado del pulsador
    setcolor(BLANCO);
    low();
    printf ("Nuevo estado: ");
    fflush(stdout);
    
    if (estado==0) setcolor(CYAN);
    else setcolor(AMARILLO);
    high();
    printf ("%s",cad_estado);
    fflush(stdout);
    
    //-- Despues el tiempo
    low();
    printf (" ,Marca de tiempo: ");
    fflush(stdout);
   
    if (estado==0) setcolor(CYAN);
    else setcolor(AMARILLO);
    high();  
    printf ("%s", cad_tiempo);
    fflush(stdout);
    low();
    printf ("\n");
    
  } while (1);
  
  printf ("\n");
  return 0;
}
