/***********************************************/
/* sg-serial.h       Juan Gonzalez Gomez.     */
/*---------------------------------------------*/
/* Comunicaciones serie para clientes StarGate */
/* Licencia GPL                                */
/***********************************************/
/*----------------------------------------------------------------------------
 $Id$
 $Revision$
 $Source$
-----------------------------------------------------------------------------*/

#ifndef SG_SERIAL_H
#define SG_SERIAL_H


/*--------------------------*/
/* PROTOTIPOS DEL INTERFAZ  */
/*--------------------------*/

/********************************************/
/* Enviar una cadena por el puerto serie    */
/********************************************/
void sg_serial_enviar(int serial_fd, char *cadena, int tam);

int sg_serial_read(int serial_fd, int *t0, int *t1, int *t2, int *t3);

void sg_serial_flush(int serial_fd);

int sg_serial_estado(int serial_fd, int *estado);

/********************************************************************/
/* Abrir el puerto serie                                            */
/*------------------------------------------------------------------*/
/* ENTRADA:                                                         */
/*   disp: cadena con el dispositivo serie                          */
/*                                                                  */
/* DEVUELVE:                                                        */
/*   -El descriptor del puerto serie ó -1 si ha ocurrido un error   */
/********************************************************************/
int sg_serial_open(char *disp);

/********************************************************************/
/* Cerrar el puerto serie                                           */
/*------------------------------------------------------------------*/
/* ENTRADA:                                                         */
/*   fd: Descriptor del fichero                                     */
/********************************************************************/
void sg_serial_close(int fd);


#endif  /* del define SG_SERIAL_H */
