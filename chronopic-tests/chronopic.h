/*****************************************************/
/* chronopic.h       Juan Gonzalez Gomez.            */
/*---------------------------------------------------*/
/* Funciones Del modulo chronopic                    *//*****************************************************/
/*----------------------------------------------------------------------------
 $Id: chronopic.h,v 1.1 2005/02/07 11:14:54 obijuan Exp $
 $Revision: 1.1 $
 $Source: /cvsroot/chronojump/chronopic/test/chronopic.h,v $
-----------------------------------------------------------------------------*/

#ifndef CHRONOPIC_H
#define CHRONOPIC_H

/*--------------------------*/
/* PROTOTIPOS DEL INTERFAZ  */
/*--------------------------*/

/********************************************************************/
/* Abrir el puerto serie y configurarlo para trabajar con chronopic */
/*------------------------------------------------------------------*/
/* ENTRADA:                                                         */
/*   disp: cadena con el dispositivo serie                          */
/*                                                                  */
/* DEVUELVE:                                                        */
/*   -El descriptor del puerto serie ó -1 si ha ocurrido un error   */
/********************************************************************/
int chronopic_open(char *disp);

/********************************************************************/
/* Cerrar el puerto serie y finalizar sesion con Chronopic          */
/*------------------------------------------------------------------*/
/* ENTRADA:                                                         */
/*   fd: Descriptor del fichero                                     */
/********************************************************************/
void chronopic_close(int fd);

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
/*    0  : Error en la lectura de la trama                             */
/*    1  : Trama leida correctamente                                   */
/***********************************************************************/
int chronopic_get_trama_cambio(int fd, double *t, int *estado);

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
int chronopic_estado(int serial_fd, int *estado);

/*******************************************/
/* Vaciar el buffer serie                  */
/*******************************************/
void chronopic_flush(int serial_fd);

#endif
