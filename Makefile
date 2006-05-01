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

CHRONOJUMP = chronojump

CHRONOJUMP_DEP_GUI = src/gui/confirm.cs src/gui/error.cs src/gui/jump.cs src/gui/jumpType.cs src/gui/run.cs src/gui/runType.cs src/gui/person.cs src/gui/preferences.cs src/gui/session.cs src/gui/stats.cs src/gui/report.cs
CHRONOJUMP_DEP_STATS = src/statType.cs src/stats/main.cs src/stats/global.cs src/stats/sjCmjAbk.cs src/stats/sjCmjAbkPlus.cs src/stats/djIndex.cs src/stats/djQ.cs src/stats/rjIndex.cs src/stats/rjPotencyBosco.cs src/stats/rjEvolution.cs src/stats/ieIub.cs src/stats/fv.cs src/stats/cmjPlusPotency.cs
CHRONOJUMP_DEP_GRAPHS = src/stats/graphs/graphData.cs src/stats/graphs/graphSerie.cs src/stats/graphs/global.cs src/stats/graphs/sjCmjAbk.cs src/stats/graphs/sjCmjAbkPlus.cs src/stats/graphs/djIndex.cs src/stats/graphs/djQ.cs src/stats/graphs/rjIndex.cs src/stats/graphs/rjPotencyBosco.cs src/stats/graphs/rjEvolution.cs src/stats/graphs/ieIub.cs src/stats/graphs/fv.cs src/stats/graphs/cmjPlusPotency.cs

CHRONOJUMP_DEP_SQLITE = src/sqlite/main.cs src/sqlite/preferences.cs src/sqlite/person.cs src/sqlite/session.cs src/sqlite/jump.cs src/sqlite/jumpType.cs src/sqlite/run.cs src/sqlite/runType.cs src/sqlite/personSession.cs src/sqlite/stat.cs src/sqlite/pulse.cs src/sqlite/pulseType.cs

CHRONOJUMP_DEP = src/chronojump.cs src/person.cs src/jump.cs src/jumpType.cs src/run.cs src/runType.cs src/pulse.cs src/pulseType.cs src/session.cs src/catalog.cs src/exportSession.cs src/treeViewPerson.cs src/treeViewJump.cs src/treeViewRun.cs src/treeViewPulse.cs src/util.cs src/constants.cs src/report.cs $(CHRONOJUMP_DEP_GUI) $(CHRONOJUMP_DEP_STATS) $(CHRONOJUMP_DEP_GRAPHS) $(CHRONOJUMP_DEP_SQLITE) $(CHRONOJUMP_DEP_REPORT)

RESOURCES = -resource:glade/chronojump.glade,chronojump.glade
CHRONOJUMP_LIB =  -pkg:gtk-sharp -pkg:gnome-sharp -pkg:glade-sharp -r:System.Data -r:Mono.Data.SqliteClient

all: $(CHRONOJUMP).exe chronojump-mini 

LIBCHRONOPIC = libchronopic.so
LIBCHRONOPIC_DEP = chronopic.o 

DLL_CHRONOPIC = chronopic


NAME5= chronojump-mini
DEP5 = src/chronojump-mini.cs $(DLL_CHRONOPIC).dll


#-- Construccion del chronojump-mini que funciona por consola
chronojump-mini: chronojump-mini.exe

#--------------------------
#  Reglas
#--------------------------

# ---- Generacion de la libreria libchronopic
$(LIBCHRONOPIC):  $(LIBCHRONOPIC_DEP)
	           $(CC) -shared -W1,-soname,$(LIBCHRONOPIC) -o $(LIBCHRONOPIC) \
                   $(LIBCHRONOPIC_DEP)
clean::
	  #rm -f $(LIBCHRONOPIC) $(LIBCHRONOPIC_DEP)
	  #don't delete the chronopic.o because we cannot make now
	  rm -f $(LIBCHRONOPIC)
    
#----- Crear la DLL
$(DLL_CHRONOPIC).dll: $(LIBCHRONOPIC) $(DLL_CHRONOPIC).cs
	 $(MCS) -unsafe -target:library $(DLL_CHRONOPIC).cs \
          -out:$(DLL_CHRONOPIC).dll     
    
clean::
	  rm -f $(DLL_CHRONOPIC).dll 
    
#-------------------------------
# Regla para compilar CHRONOJUMP (C#)
#-------------------------------

$(CHRONOJUMP).exe: $(DLL_CHRONOPIC).dll NPlot.dll NPlot.Gtk.dll $(CHRONOJUMP_DEP) 
	 $(MCS) $(CHRONOJUMP_DEP) $(RESOURCES) -unsafe -r:$(DLL_CHRONOPIC).dll -r:NPlot.dll -r:NPlot.Gtk.dll -r:System.Drawing $(CHRONOJUMP_LIB) -nowarn:169 -out:$(CHRONOJUMP).exe 
   
clean::
	  rm -f $(CHRONOJUMP).exe  
    
#---------------------------------
#--- Dependencias con ficheros .h
#---------------------------------
sg-serial.o    : sg-serial.h

#-------------------------------
# EJEMPLOS DE PRUEBA EN C#
#-------------------------------
$(NAME5).exe: $(DEP5)
	 $(MCS) src/catalog.cs src/$(NAME5).cs -unsafe -r:$(DLL_CHRONOPIC).dll -o $(NAME5).exe 
   
clean::
	  rm -f $(NAME5).exe       
    
#--------------------------
#  REGLAS GENERICAS
#--------------------------
.c.o:		
		$(CC) $(CFLAGS) -c $<
