/************************************************************************/
/*  TERMANS.C.  (C) Grupo J&J. Noviembre 1997.                          */
/* -------------------------------------------------------------------- */
/*  Funciones para actuar sobre terminales ansi: cambiar el color,      */
/*  borrar la pantalla...                                               */
/*                                                                      */
/************************************************************************/

#include <stdio.h>
#include <unistd.h>
#include "termansi.h"

int ansi=1;   /* ANSI SI/NO */

void configansi(int t)
/**************************************************************************/
/* Cambiar el estado del ansi. Cuando el parametro de entrada indica 0    */
/* no se utilizan comandos ansi. Cuando t=1 si.                           */
/**************************************************************************/
{
  if (t==0) ansi=0;
  else ansi=1;
}

void dprint(char *cad)
/******************************************************/
/* Imprimir una cadena directamente en la pantalla    */
/******************************************************/
{
  int i=0;
  
  while (cad[i]!=0){
    write(1,&cad[i],1);
    i++;
  }
}

void clrscr()
/************************/
/*  Borrar la pantalla  */
/************************/
{
  if (ansi) {
    dprint ("[2J");     /* Borrar la pantalla                        */
    dprint ("[1;1H");   /* Situar cursor en esquina superior derecha */
  }  
}

void locate(char x, char y)
/**********************************************/
/*  Situar el cursor en la posicion indicada  */
/**********************************************/
{
  char s[20];
  
  if (ansi) {
    sprintf(s,"[%d;%dH",y,x);
    dprint(s);
  }  
}

void fondo(char f)
/**********************************/
/* Cambiar el color del fondo     */
/**********************************/
{
  char s[20];
  
  if (ansi) {
    sprintf(s,"[%dm",f+40);
    dprint (s);
  }  
}

void setcolor(char c)
/**************************************/
/*  Cambiar el color del primer plano */
/**************************************/
{
  char s[20];
  
  if (ansi) {
    sprintf(s,"[%dm",c+30);
    dprint(s);
  }  
}

void color(char f,char b)
/***********************************************/
/*  Cambiar color del primer plano y del fondo */
/***********************************************/
{
  char s[20];

  if (ansi) {  
    sprintf(s,"[%d;%dm",f+30,b+40);
    dprint(s);
  }  
}

void high()
/******************************************************************/
/*  Establecer atributos de alta intensidad en color primer plano */
/******************************************************************/
{
  char s[20];
  
  if (ansi) {
    sprintf (s,"[1m");
    dprint(s);
  }  
}

/********************************/
/*  Poner atributos a 'cero'    */
/********************************/
void low()
{
  char s[20];
  
  if (ansi) {
    sprintf(s,"[0m");
    dprint(s);
  } 
}
