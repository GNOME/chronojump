/***************************************************************************/
/* test-saltos.c (c) Juan Gonzalez.  Febrero 2005                          */
/*-------------------------------------------------------------------------*/
/* Pruebas de medidion de saltos. Se mide el tiempo de vuelo de los saltos */
/* realizados, mostrandose los resultados por la pantalla                  */
/*-------------------------------------------------------------------------*/
/* Licencia GPL                                                            */
/***************************************************************************/
/*-------------------------------------------------------------------------
 $Id: test-saltos.c,v 1.1 2005/02/07 11:14:54 obijuan Exp $
 $Revision: 1.1 $
 $Source: /cvsroot/chronojump/chronopic/test/test-saltos.c,v $
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
    "Forma de uso:   test-saltos dispositivo_serie [opciones]\n");
  printf ("\n");
  printf ("Dispositivo serie: \n");
  printf ("   -Dispositivo donde esta conectado ChronoPic. Ej. /dev/ttyUSB0\n");
  printf ("\n");
  printf ("Opciones:\n");
  printf ("    -h : Mostrar esta ayuda\n");
  printf ("\n");
  printf ("Ejemplo: test-saltos /dev/ttyUSB0\n");
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
	printf ("test-saltos. (c) Juan Gonzalez. Febrero 2005. Licencia GPL\n");
	printf ("Medicion del tiempo de vuelo de los saltos\n");
  printf ("\n");
}

/*----------------*/
/*     MAIN       */
/*----------------*/
int main (int argc, char **argv)
{
  char cad[80];
  int serial_fd;
  int ok;
  double t;
  int estado_plataforma;
  int estado_automata=0;
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
  
  //-- Obtener el estado inicial
  ok=chronopic_estado(serial_fd, &estado_plataforma);
  
  //-- Timeout: no hay conexion
  if (ok==0) {
    printf ("ChonoPIC no conectado\n");
    chronopic_close(serial_fd);
    return 1;
  }
  
  //-- Ha habido un error
  if (ok==-1) {
    printf ("Error en la comunicacion con ChronoPIC\n");
    chronopic_close(serial_fd);
    return 1;
  }
  
  //-- Establecer estado inicial del automata
  if (estado_plataforma==1) estado_automata=1;
  else {
    
    printf ("SUBA A LA PLATAFORMA PARA REALIZAR EL SALTO\n");
    
    //-- Esperar a que llegue una trama con estado de la plataforma
    //-- igual a 1, para esperar a que se el usuario se suba
    ok = chronopic_get_trama_cambio(serial_fd,&t,&estado_plataforma);
    while (ok==0 || (ok==1 && estado_plataforma==0)) {
      ok = chronopic_get_trama_cambio(serial_fd,&t,&estado_plataforma);
    }
    
    //-- Aqui el estado de la plataforma es 1, hay alguien subido
    estado_automata=1;
  }
  
    
  printf ("\n");
  printf ("Puede saltar cuando quiera\n");
  printf ("Pulse control-c para finalizar la sesion\n");
  printf ("-----------------------------------------\n");
  
  while(1) {
    //-- Esperar a que llegue una trama
    do {
      n = chronopic_get_trama_cambio(serial_fd,&t,&estado_plataforma);
    } while(n==0);
    
    //-- Error en trama recibida: terminar
    if (n==-1) return 0;
    
    switch(estado_automata) {
      
      case 0: //-- Estado OFF. Usuario en el aire
        
        //-- Si ha aterrizado
        if (estado_plataforma==1) {
          
          //-- Pasar al estado ON
          estado_automata=1;
          
          //-- Registrar tiempo de vuelo
          toff=t;
          
          //-- Imprimir informacion
          sprintf (cad,"Tiempo: %8.1f ms\n",toff);
          printf (cad);
        }
      case 1:  //-- Estado ON. Usuario estaba de la plataforma
        
        //-- Si ahora esta en el aire...
        if (estado_plataforma==0) {
          
          //-- Pasar al estado OFF
          estado_automata=0;
        }
    }
    
  }
    
  printf ("\n");
  return 0;
}
