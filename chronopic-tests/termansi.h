/*
 * Copyright (C) 1997  Juan Gonzalez Gomez
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

#define NEGRO    0
#define AZUL     4
#define VERDE    2
#define CYAN     6
#define ROJO     1
#define MAGENTA  5
#define AMARILLO 3
#define BLANCO   7

#define SIMPLE 0
#define DOBLE  1

void configansi(int t);
void dprint(char *cad);
void clrscr();
void locate(char x, char y);
void fondo(char f);
void setcolor(char c);
void color(char f,char b);
void high();
void low();
