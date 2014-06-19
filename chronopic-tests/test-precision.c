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
/* Comprobacion de la precision de Chronopic. Se mide mide el periodo y la */
/* frecuencia de senales cuadradas aplicadas a Chronopic. De esta manera   */
/* se puede comprobar cual es la precision en la medida                    */
/*   Sirve para validar Chronopic                                          */
/*-------------------------------------------------------------------------*/
/*-------------------------------------------------------------------------
 $Id: test-precision.c,v 1.1 2005/02/07 11:14:54 obijuan Exp $
 $Revision: 1.1 $
 $Source: /cvsroot/chronojump/chronopic/test/test-precision.c,v $
---------------------------------------------------------------------------*/
#include <termios.h>
#include <unistd.h> 
#include <stdio.h>
#include <string.h>

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
    "Forma de uso:   test-precision dispositivo_serie [opciones]\n");
  printf ("\n");
  printf ("Dispositivo serie: \n");
  printf ("   -Dispositivo donde esta conectado ChronoPic. Ej. /dev/ttyUSB0\n");
  printf ("\n");
  printf ("Opciones:\n");
  printf ("    -h : Mostrar esta ayuda\n");
  printf ("\n");
  printf ("Ejemplo: test-precision /dev/ttyUSB0\n");
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
	printf ("test-precision. (c) Juan Gonzalez. Febrero 2005. Licencia GPL\n");
	printf ("Medicion de senales cuadradas en ChronoPic [Proyecto ChronoJump]\n");
  printf ("\n");
  printf ("Aplicando senales cuadradas de frecuencia conocida y comparando\n");
  printf ("los resultados con este programa, se puede determinar el error\n");
  printf ("que tiene Chronopic\n");
	printf ("\n");
}

/*----------------*/
/*     MAIN       */
/*----------------*/
int main (int argc, char **argv)
{
  int i;
  int serial_fd;
  int ok;
  double t;
  int estado_plataforma;
  int estado_automata=0;
  double ton=0.0;
  double toff=0.0;
  int n;
  
  presenta();
  
  ok=analizar_parametros(argc,argv);
	if (!ok) return 0;
  
  //-- Abrir el puerto serie. Por defecto se abre con los parametros
  //-- necesarios en Chronopic
  serial_fd=chronopic_open(disp_serie);

  if (serial_fd==-1) {
    printf ("\n");
    printf ("Error al abrir puerto serie %s\n",argv[1]);
    perror("ERROR: ");
    return 1;
  }
  
  printf ("\n");
  printf ("Aplicar una senal cuadrada a la entrada del cronometro\n");
  printf ("Pulse control-c para terminar\n");
  printf ("-------------------------------------------------------\n");
  
  while(1) {
    //-- Esperar a que llegue una trama
    do {
      n = chronopic_get_trama_cambio(serial_fd,&t,&estado_plataforma);
    } while(n==0);
    
    //-- Error en trama recibida: terminar
    if (n==-1) return 0;
    
    switch(estado_automata) {
      case 0: //-- Estado OFF
        if (estado_plataforma==1) {
          estado_automata=1;
          toff=t;
           //-- Sacar informacion sobre el evento ocurrido
           printf ("Periodo: %7.2f ms, Freq: %2.3f Hz. ",ton+toff,
                    1000/(ton+toff));
           //sprintf (cad,"%X %X %X",trama[2],trama[3],trama[4]);
           //for (i=0; i<strlen(cad); i++)
           //  printf ("\b");
           fflush(stdout);
        }
      case 1:  //-- Estado ON
        if (estado_plataforma==0) {
          estado_automata=0;
          ton=t;
        }
    }
    
  }
    
  printf ("\n");
  return 0;
}
