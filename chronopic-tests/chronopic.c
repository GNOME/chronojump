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
/*---------------------------------------------------------*/
/* Funciones de acceso a chronopic                         */
/* Licencia GPL                                            */
/***********************************************************/
/*------------------------------------------------------------------------
 $Id: chronopic.c,v 1.1 2005/02/07 11:14:54 obijuan Exp $
 $Revision: 1.1 $
 $Source: /cvsroot/chronojump/chronopic/test/chronopic.c,v $
--------------------------------------------------------------------------*/
#include <sys/time.h>
#include <sys/types.h>
#include <stdio.h>
#include <termios.h>
#include <unistd.h>
#include <fcntl.h>
#include <string.h>
#include "chronopic.h"


/*********************************/
/* PROTOTIPOS FUNCIONES PRIVADAS */
/*********************************/
int sg_trama_cambio_read(int serial_fd, unsigned char *trama);

/*-----------------------------------------------------------------------*/
/*   F U N C I O N E S      D E      I N T E R F A Z                     */
/*-----------------------------------------------------------------------*/

/********************************************************************/
/* Abrir el puerto serie y configurarlo para trabajar con chronopic */
/*------------------------------------------------------------------*/
/* ENTRADA:                                                         */
/*   disp: cadena con el dispositivo serie                          */
/*                                                                  */
/* DEVUELVE:                                                        */
/*   -El descriptor del puerto serie ó -1 si ha ocurrido un error   */
/********************************************************************/
int chronopic_open(char *disp)
{
  struct termios newtermios;
  int fd;

  fd = open(disp,O_RDWR | O_NOCTTY); /* Abrir puerto serie */

  /* Modificar los atributos */
  newtermios.c_cflag= CS8 | CLOCAL | CREAD;
  /* CBAUD is a Linux extension to the POSIX Terminal I/O definitions. */
#ifdef CBAUD
  newtermios.c_cflag |= CBAUD;
#endif
  newtermios.c_iflag=IGNPAR;
  newtermios.c_oflag=0;
  newtermios.c_lflag=0;
  newtermios.c_cc[VMIN]=1;
  newtermios.c_cc[VTIME]=0;

  /* Establecer velocidad por defecto */
  cfsetospeed(&newtermios,B9600);
  cfsetispeed(&newtermios,B9600);
  
  /* Vaciar los buffers de entrada y salida */
  if (tcflush(fd,TCIFLUSH)==-1) {
    return -1;
  }

  /* Vaciar buffer de salida */
  if (tcflush(fd,TCOFLUSH)==-1) {
    return -1;
  }

  /* Escribir los nuevos atributos */
  if (tcsetattr(fd,TCSANOW,&newtermios)==-1) {
    return -1;
  }  

  return fd;
}

/********************************************************************/
/* Cerrar el puerto serie y finalizar sesion con Chronopic          */
/*------------------------------------------------------------------*/
/* ENTRADA:                                                         */
/*   fd: Descriptor del fichero                                     */
/********************************************************************/
void chronopic_close(int fd)
{
  close(fd);
}

/***********************************************************************/
/* Leer una trama y devolver el estado de la plataforma y el tiempo    */
/* transcurrido desde la ultima transicion                             */
/*                                                                     */
/*  ENTRADAS:                                                          */
/*    -fd: Descriptor del puerto serie, donde esta conectado ChronoPic */
/*                                                                     */
/*  SALIDAS:                                                           */
/*    -t : Tiempo transcurrido                                         */
/*    -estado: Estado de la plataforma:                                */
/*        * 0 --> OFF. No hay nadie sobre la plataforma                */
/*        * 1 --> ON.  Hay alguien apoyado                             */
/*                                                                     */
/*  DEVUELVE:                                                          */
/*    -1 : Error en la lectura de la trama                             */
/*    0  : Timeout                                                     */
/*    1  : Trama leida correctamente                                   */
/***********************************************************************/
int chronopic_get_trama_cambio(int fd, double *t, int *estado)
{
  int n;
  unsigned char trama[10];
  
  //-- Esperar a que llegue una trama
  n=sg_trama_cambio_read(fd,trama);
  
  //-- Timeout o algun error en recepcion
  if (n!=1) return n;
  
  //printf("%c-%c-%c-%c-%c",trama[0], trama[1], trama[2], trama[3], trama[4]);
  //printf("%d-%d-%d-%d-%d",trama[0], trama[1], trama[2], trama[3], trama[4]);
  printf("%c-%d-%d-%d-%d",trama[0], trama[1], trama[2], trama[3], trama[4]);
  printf("%d", ((trama[2]*65536 + trama[3]*256 + trama[4])*8)/1000);
  
  //-- Analizar si es una trama de cambio
  if (trama[0]!='X') {
    printf ("Error. Trama desconocida\n");
    return -1;
  }

  //-- Obtener el estado
  if (trama[1]!=0 && trama[1]!=1) {
    printf ("Error. Estado no valido\n");
    return -1;
  }
  *estado = (int)(trama[1]&0xff);
  
  //-- Obtener el tiempo en milisegundos
  //-- El Chronopic devuelve 3 bytes, que se obtienen en las posiciones
  //-- 2, 3 y 4 de la trama, siendo trama[2] el de mayor peso
  //-- Esta en unidades de 8 micro-segundos, por lo que hay que multiplicar
  //-- por 8 para obtenerlo en microseungos. Finalmente hay que dividirlo
  //-- entre 1000 para obtenerlo en milisegundos.
  *t = (double)((trama[2]*65536 + trama[3]*256 + trama[4])*8)/1000;

  printf ("%f", *t);
 
  return 1;
}

/***********************************************************************/
/* Devolver el estado de la plataforma                                 */
/*  ENTRADAS:                                                          */
/*    -fd: Descriptor del puerto serie, donde esta conectado ChronoPic */
/*                                                                     */
/*  SALIDAS:                                                           */
/*    -estado: Estado de la plataforma:                                */
/*        * 0 --> OFF. No hay nadie sobre la plataforma                */
/*        * 1 --> ON.  Hay alguien apoyado                             */
/*                                                                     */
/*  DEVUELVE:                                                          */
/*    -1 : Error en la lectura de la trama                             */
/*    0  : Timeout                                                     */
/*    1  : Trama leida correctamente                                   */
/***********************************************************************/
int chronopic_estado(int serial_fd, int *estado) 
{
  unsigned char trama[10];
  char respuesta[10];
  fd_set fds;
  struct timeval timeout;
  int status;
  int ret;

  
  // Construir la trama del servicio de estado
  trama[0]='E';

  //-- Enviarla al StarGate
  write(serial_fd, trama,1); 

  //-- Esperar la respuesta
  FD_ZERO(&fds);
  FD_SET (serial_fd, &fds);

  timeout.tv_sec = 0;  //-- Establecer el TIMEOUT
  timeout.tv_usec = 100000;

  ret=select (FD_SETSIZE,&fds, NULL, NULL,&timeout);
  
  switch(ret) {
    case 1 : //--Datos listos 
      read (serial_fd, respuesta, 10);
      if (respuesta[0]=='E') {
        status=1;
        *estado=(int)respuesta[1];
      }
      else { //-- Recibido algo distinto de lo esperado
        status=-1;
        *estado=0;
      }
      break;
    case 0: //-- Timeout
      tcflush(serial_fd,TCIOFLUSH);
      status=0;
      *estado=0;
      break;
    default: //-- Otro Error
      status=-1;
      *estado=0;
      break;
  }
  return status;
    
}

/*************************************/
/* Solicitar el estado de chronopic  */
/* Solo se envia la trama            */
/*************************************/
void chronopic_solicitar_estado(int serial_fd)
{
  unsigned char trama[10];
  
  // Construir la trama del servicio de estado
  trama[0]='E';

  //-- Enviarla al StarGate
  write(serial_fd, trama,1); 
}


/*******************************************/
/* Vaciar el buffer                        */
/*******************************************/
void chronopic_flush(int serial_fd)
{
  /* Vaciar los buffers de entrada y salida */
  tcflush(serial_fd,TCIOFLUSH);
}

/**************************************/
/* Leer un numero de bytes            */
/**************************************/
int chronopic_read(int serial_fd, unsigned char *trama,
                   int bytes, int tout)
{
  fd_set fds;
  ssize_t n;
  struct timeval timeout;
  int ret;
  
  //-- Comprobar si hay datos esperando en el puerto serie
  FD_ZERO(&fds);
  FD_SET (serial_fd, &fds);

  timeout.tv_sec = 0;  //-- Establecer el TIMEOUT
  timeout.tv_usec = tout;  

  ret=select (FD_SETSIZE,&fds, NULL, NULL,&timeout);
  
  switch(ret) {
    case 1 : //--Datos listos. Leer una trama
      n=read (serial_fd, trama, bytes);
      return n;
      break;
    case 0: //-- Timeout
      trama[0]=0;
      return 0;
      break;
    default: //--Cualquier otro error
      trama[0]=0;
      return -1;
      break;
  }
  
  //-- Aqui nunca llega...
  return 1;
}

/************************************************/
/* Leer una trama de cambio completa            */
/************************************************/
int sg_trama_cambio_read(int serial_fd, unsigned char *trama)
{
  fd_set fds;
  ssize_t n;
  struct timeval timeout;
  int ret;
  
  //-- Comprobar si hay datos esperando en el puerto serie
  FD_ZERO(&fds);
  FD_SET (serial_fd, &fds);

  timeout.tv_sec = 0;  //-- Establecer el TIMEOUT
  timeout.tv_usec = 100000;  //-- 100ms

  ret=select (FD_SETSIZE,&fds, NULL, NULL,&timeout);
  
  switch(ret) {
    case 1 : //--Datos listos. Leer una trama
      n=read (serial_fd, trama, 5);
      if (n!=5) return -1;
      return 1;
      break;
    case 0: //-- Timeout
      trama[0]=0;
      return 0;
      break;
    default: //--Cualquier otro error
      trama[0]=0;
      return -1;
      break;
  }
  
  //-- Aqui nunca llega...
  return 1;
}
