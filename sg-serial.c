/***********************************************/
/* sg-serial.c        Juan Gonzalez Gomez.     */
/*---------------------------------------------*/
/* Comunicaciones serie para clientes StarGate */
/* Licencia GPL                                */
/***********************************************/
/*------------------------------------------------------------------------
 $Id$
 $Revision$
 $Source$
--------------------------------------------------------------------------*/

#include <termios.h>
#include <unistd.h>
#include <fcntl.h>
#include <string.h>
#include "sg-serial.h"

/********************************************/
/* Enviar una cadena por el puerto serie    */
/********************************************/
void sg_serial_enviar(int serial_fd, char *cadena, int tam)
{
  
  write(serial_fd, cadena,tam); 
 
}


/***********************************************/
/* Devolver el estado de la manta              */
/***********************************************/
int sg_serial_estado(int serial_fd, int *estado) 
{
  unsigned char trama[10];
  char respuesta[10];
  fd_set fds;
  struct timeval timeout;
  int status;
  int ret;
  int n;
  
  // Construir la trama del servicio de estado
  trama[0]='E';

  //-- Enviarla al StarGate
  sg_serial_enviar(serial_fd, trama,1);

  //-- Esperar la respuesta
  FD_ZERO(&fds);
  FD_SET (serial_fd, &fds);

  timeout.tv_sec = 0;  //-- Establecer el TIMEOUT
  timeout.tv_usec = 100000;

  ret=select (FD_SETSIZE,&fds, NULL, NULL,&timeout);
  
  switch(ret) {
    case 1 : //--Datos listos 
      n=read (serial_fd, respuesta, 10);
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

/*******************************************/
/* Vaciar el buffer                        */
/*******************************************/
void sg_serial_flush(int serial_fd)
{
  /* Vaciar los buffers de entrada y salida */
  tcflush(serial_fd,TCIOFLUSH);
}

/************************************************/
/* Leer una trama completa                      */
/************************************************/
int sg_serial_read(int serial_fd, int *t0, int *t1, int *t2, int *t3)
{
  fd_set fds;
  ssize_t n;
  struct timeval timeout;
  int ret;
  unsigned char trama[5];
  
  //-- Comprobar si hay datos esperando en el puerto serie
  FD_ZERO(&fds);
  FD_SET (serial_fd, &fds);

  timeout.tv_sec = 0;  //-- Establecer el TIMEOUT
  timeout.tv_usec = 100000;  //-- 100ms

  ret=select (FD_SETSIZE,&fds, NULL, NULL,&timeout);
  
  switch(ret) {
    case 1 : //--Datos listos. Leer una trama
      n=read (serial_fd, trama, 4);
      *t0=trama[0];
      *t1=trama[1];
      *t2=trama[2];
      *t3=trama[3];
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

/********************************************************************/
/* Abrir el puerto serie                                            */
/*------------------------------------------------------------------*/
/* ENTRADA:                                                         */
/*   disp: cadena con el dispositivo serie                          */
/*                                                                  */
/* DEVUELVE:                                                        */
/*   -El descriptor del puerto serie ó -1 si ha ocurrido un error   */
/********************************************************************/
int sg_serial_open(char *disp)
{
  struct termios newtermios;
  int fd;

  fd = open(disp,O_RDWR | O_NOCTTY); /* Abrir puerto serie */

  /* Modificar los atributos */
  newtermios.c_cflag= CBAUD | CS8 | CLOCAL | CREAD;
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
/* Cerrar el puerto serie                                           */
/*------------------------------------------------------------------*/
/* ENTRADA:                                                         */
/*   fd: Descriptor del fichero                                     */
/********************************************************************/
void sg_serial_close(int fd)
{
  close(fd);
}
