# **************************************************************************
# Fichero makefile.
# --------------------------------------------------------------------------
# Licencia GPL. Juan Gonzalez Gomez
# --------------------------------------------------------------------------
#
#***************************************************************************


#---- Compilador de C
CC = gcc
CFLAGS = -Wall

#-- Compilador de C#
MCS = mcs

#-------- Nombres y dependencias de los programas a construir
TEST1= test-platform
LIBSERIAL = libserial.so
LIBSERIAL_DEP = sg-serial.o 
DLL_SERIAL = serial


CHRONOJUMP = chronojump

CHRONOJUMP_DEP = chronojump.cs stat.cs someWindows.cs sqlite.cs person.cs jump.cs session.cs
RESOURCES = -resource:glade/chronojump.glade,chronojump.glade
CHRONOJUMP_LIB =  -pkg:gtk-sharp -pkg:gnome-sharp -pkg:glade-sharp -r System.Data -r Mono.Data.SqliteClient
#CHRONOJUMP_LIB =  -pkg:gtk-sharp -pkg:gnome-sharp -pkg:glade-sharp -r System.Data -r Mono.Data.SqliteClient -r GNU.Gettext

all: $(CHRONOJUMP).exe $(TEST1).exe 

#--------------------------
#  Reglas
#--------------------------

# ---- Generacion de la libreria libserial
$(LIBSERIAL):  $(LIBSERIAL_DEP)
	           $(CC) -shared -W1,-soname,libserial.so -o $(LIBSERIAL) $(LIBSERIAL_DEP)
clean::
	  rm -f $(LIBSERIAL) $(LIBSERIAL_DEP)
    
#----- Crear la libserial.dll
$(DLL_SERIAL).dll: $(LIBSERIAL) $(DLL_SERIAL).cs
	 $(MCS) -target:library $(DLL_SERIAL).cs -o $(DLL_SERIAL).dll     
    
clean::
	  rm -f $(DLL_SERIAL).dll 
    
#----- Crear test plaform
$(TEST1).exe: $(TEST1).cs $(DLL_SERIAL).dll
	 $(MCS) $(TEST1).cs -unsafe -r $(DLL_SERIAL).dll -o $(TEST1).exe 
   
clean::
	  rm -f $(TEST1).exe       

#-------------------------------
# Regla para compilar CHRONOJUMP (C#)
#-------------------------------

$(CHRONOJUMP).exe: $(DLL_SERIAL).dll $(CHRONOJUMP_DEP) 
	 $(MCS) $(CHRONOJUMP_DEP) $(RESOURCES) -unsafe -r $(DLL_SERIAL).dll $(CHRONOJUMP_LIB) -o $(CHRONOJUMP).exe 
   
clean::
	  rm -f $(CHRONOJUMP).exe  
    
#---------------------------------
#--- Dependencias con ficheros .h
#---------------------------------
sg-serial.o    : sg-serial.h

#--------------------------
#  REGLAS GENERICAS
#--------------------------
.c.o:		
		$(CC) $(CFLAGS) -c $<
