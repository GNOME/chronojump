# **************************************************************************
# Fichero makefile.
# --------------------------------------------------------------------------
# Licencia GPL. Juan Gonzalez Gomez
# --------------------------------------------------------------------------
#
#***************************************************************************


# Edited for Debian GNU/Linux.
DESTDIR =

# ¿Dónde poner los binarios en el 'make install'?
BIN  = $(DESTDIR)/usr/local/bin

#---- Compilador
CC = gcc
CFLAGS = -Wall

#-- Compilador de C#
MCS = mcs

#-------- Nombres y dependencias de los programas a construir
NAME1= chronopic-serial-conf
DEP1= $(NAME1).o chronopic.o

NAME2= test-tramas
DEP2= $(NAME2).o chronopic.o termansi.o 

NAME3= test-precision
DEP3= $(NAME3).o chronopic.o

NAME4= test-saltos
DEP4=  $(NAME4).o chronopic.o

NAME5= test-saltos.mono
DEP5 = $(NAME5).cs $(DLL_CHRONOPIC).dll

NAME6= test
DEP6 = $(NAME6).cs $(DLL_CHRONOPIC).dll


LIBCHRONOPIC = libchronopic.so
LIBCHRONOPIC_DEP = chronopic.o 

DLL_CHRONOPIC = chronopic

all: test test-mono

#-- Construccion de los ejemplos de prueba en c
test: $(NAME1) $(NAME2)  $(NAME3) $(NAME4)

#-- Construccion de los ejemplos de prueba en Mono
test-mono: $(NAME5).exe $(NAME6).exe

#--------------------------
#  Reglas
#--------------------------

$(NAME1): $(DEP1)
	    $(CC) -o $(NAME1) $(DEP1) 

clean::
	  rm -f $(NAME1) $(DEP1) 

#---------------------------------------
$(NAME2): $(DEP2)
	    $(CC) -o $(NAME2) $(DEP2) 

clean::
	  rm -f $(NAME2) $(DEP2) 

#---------------------------------------
$(NAME3): $(DEP3)
	    $(CC) -o $(NAME3) $(DEP3) 

clean::
	  rm -f $(NAME3) $(DEP3) 
    
#---------------------------------------
$(NAME4): $(DEP4)
	    $(CC) -o $(NAME4) $(DEP4) 

clean::
	  rm -f $(NAME4) $(DEP4)     


# ---- Generacion de la libreria libchronopic
$(LIBCHRONOPIC):  $(LIBCHRONOPIC_DEP)
	           $(CC) -shared -W1,-soname,$(LIBCHRONOPIC) -o $(LIBCHRONOPIC) \
                   $(LIBCHRONOPIC_DEP)
clean::
	  rm -f $(LIBCHRONOPIC) $(LIBCHRONOPIC_DEP)
    
#----- Crear la DLL
$(DLL_CHRONOPIC).dll: $(LIBCHRONOPIC) $(DLL_CHRONOPIC).cs
	 $(MCS) -unsafe -target:library $(DLL_CHRONOPIC).cs \
          -o $(DLL_CHRONOPIC).dll     
    
clean::
	  rm -f $(DLL_CHRONOPIC).dll 
    
#-------------------------------
# EJEMPLOS DE PRUEBA EN C#
#-------------------------------
$(NAME5).exe: $(DEP5)
	 $(MCS) $(NAME5).cs -unsafe -r $(DLL_CHRONOPIC).dll -o $(NAME5).exe 
   
clean::
	  rm -f $(NAME5).exe       
    

$(NAME6).exe: $(DEP6)
	 $(MCS) $(NAME6).cs -o $(NAME6).exe -r $(DLL_CHRONOPIC).dll -unsafe \
          -pkg:gtk-sharp 
   
clean::
	  rm -f $(NAME6).exe       

#--------------------------
#  REGLAS GENERICAS
#--------------------------
.c.o:		
		$(CC) $(CFLAGS) -c $<
