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
