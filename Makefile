# **************************************************************************
# Fichero makefile.
# --------------------------------------------------------------------------
# Licencia GPL. Juan Gonzalez Gomez, Xavier de Blas Foix
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

CHRONOJUMP_DEP_GUI = src/gui/confirm.cs src/gui/error.cs src/gui/jump.cs src/gui/person.cs src/gui/preferences.cs src/gui/session.cs
CHRONOJUMP_DEP_STATS = src/stats/main.cs src/stats/global.cs src/stats/sjCmjAbk.cs src/stats/sjCmjAbkPlus.cs src/stats/dj.cs src/stats/djIndex.cs src/stats/rjIndex.cs src/stats/rjPotencyAguado.cs src/stats/ieIub.cs
CHRONOJUMP_DEP_GRAPHS = src/stats/graphs/global.cs

CHRONOJUMP_DEP_SQLITE = src/sqlite/main.cs src/sqlite/preferences.cs src/sqlite/person.cs src/sqlite/session.cs src/sqlite/jump.cs src/sqlite/personSession.cs src/sqlite/stat.cs

CHRONOJUMP_DEP = src/chronojump.cs src/person.cs src/jump.cs src/session.cs src/catalog.cs src/exportSession.cs src/treeViewJump.cs $(CHRONOJUMP_DEP_GUI) $(CHRONOJUMP_DEP_STATS) $(CHRONOJUMP_DEP_GRAPHS) $(CHRONOJUMP_DEP_SQLITE)

RESOURCES = -resource:glade/chronojump.glade,chronojump.glade 
CHRONOJUMP_LIB =  -pkg:gtk-sharp -pkg:gnome-sharp -pkg:glade-sharp -r System.Data -r Mono.Data.SqliteClient

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

$(CHRONOJUMP).exe: $(DLL_SERIAL).dll NPlot.dll NPlot.Gtk.dll $(CHRONOJUMP_DEP) 
	 $(MCS) $(CHRONOJUMP_DEP) $(RESOURCES) -unsafe -r $(DLL_SERIAL).dll -r NPlot.dll -r NPlot.Gtk.dll -r System.Drawing $(CHRONOJUMP_LIB) -o $(CHRONOJUMP).exe 
   
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
