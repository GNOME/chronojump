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
MCS = gmcs

#-------- Nombres y dependencias de los programas a construir

CHRONOJUMP = chronojump

CHRONOJUMP_DEP_GUI = src/gui/confirm.cs src/gui/error.cs src/gui/eventExecute.cs src/gui/eventGraphConfigure.cs src/gui/event.cs src/gui/jump.cs src/gui/jumpType.cs src/gui/run.cs src/gui/runType.cs src/gui/pulse.cs src/gui/person.cs src/gui/preferences.cs src/gui/session.cs src/gui/stats.cs src/gui/report.cs src/gui/about.cs src/gui/helpPorts.cs src/gui/dialogMessage.cs src/gui/dialogHelp.cs src/gui/dialogCalendar.cs src/gui/dialogImageTest.cs src/gui/language.cs src/gui/repetitiveConditions.cs

CHRONOJUMP_DEP_STATS = src/statType.cs src/stats/main.cs src/stats/global.cs src/stats/sjCmjAbk.cs src/stats/sjCmjAbkPlus.cs src/stats/djIndex.cs src/stats/djQ.cs src/stats/rjIndex.cs src/stats/rjPotencyBosco.cs src/stats/rjEvolution.cs src/stats/ieIub.cs src/stats/fv.cs src/stats/cmjPlusPotency.cs

CHRONOJUMP_DEP_GRAPHS = src/stats/graphs/graphData.cs src/stats/graphs/graphSerie.cs src/stats/graphs/global.cs src/stats/graphs/sjCmjAbk.cs src/stats/graphs/sjCmjAbkPlus.cs src/stats/graphs/djIndex.cs src/stats/graphs/djQ.cs src/stats/graphs/rjIndex.cs src/stats/graphs/rjPotencyBosco.cs src/stats/graphs/rjEvolution.cs src/stats/graphs/ieIub.cs src/stats/graphs/fv.cs src/stats/graphs/cmjPlusPotency.cs

CHRONOJUMP_DEP_SQLITE = src/sqlite/main.cs src/sqlite/preferences.cs src/sqlite/person.cs src/sqlite/session.cs src/sqlite/jump.cs src/sqlite/jumpType.cs src/sqlite/run.cs src/sqlite/runType.cs src/sqlite/personSession.cs src/sqlite/stat.cs src/sqlite/pulse.cs src/sqlite/pulseType.cs src/sqlite/reactionTime.cs src/sqlite/event.cs

CHRONOJUMP_DEP_EXECUTE = src/execute/event.cs src/execute/jump.cs src/execute/run.cs src/execute/pulse.cs src/execute/reactionTime.cs

CHRONOJUMP_DEP_SERVER = chronojump_server/ChronojumpServer.cs

CHRONOJUMP_DEP = src/chronojump.cs src/person.cs src/event.cs src/eventType.cs src/jump.cs src/jumpType.cs src/run.cs src/runType.cs src/pulse.cs src/pulseType.cs src/reactionTime.cs src/reactionTimeType.cs src/session.cs src/exportSession.cs src/treeViewEvent.cs src/treeViewPerson.cs src/treeViewJump.cs src/treeViewRun.cs src/treeViewPulse.cs src/treeViewReactionTime.cs src/util.cs src/constants.cs src/report.cs src/updateProgressBar.cs src/prepareEventGraphObjects.cs src/repetitiveConditions.cs $(CHRONOJUMP_DEP_GUI) $(CHRONOJUMP_DEP_STATS) $(CHRONOJUMP_DEP_GRAPHS) $(CHRONOJUMP_DEP_SQLITE) $(CHRONOJUMP_DEP_REPORT) $(CHRONOJUMP_DEP_EXECUTE) $(CHRONOJUMP_DEP_SERVER)

RESOURCES_GLADE = -resource:glade/chronojump.glade,chronojump.glade
RESOURCES_IMAGES = -resource:images/mini/no_image.png,mini/no_image.png \
		-resource:images/agility_505.png,agility_505.png 		-resource:images/mini/agility_505.png,mini/agility_505.png \
		-resource:images/agility_20yard.png,agility_20yard.png		-resource:images/mini/agility_20yard.png,mini/agility_20yard.png \
		-resource:images/agility_illinois.png,agility_illinois.png 	-resource:images/mini/agility_illinois.png,mini/agility_illinois.png \
		-resource:images/agility_shuttle.png,agility_shuttle.png 	-resource:images/mini/agility_shuttle.png,mini/agility_shuttle.png \
		-resource:images/agility_zigzag.png,agility_zigzag.png 		-resource:images/mini/agility_zigzag.png,mini/agility_zigzag.png \
		-resource:images/jump_free.png,jump_free.png 			-resource:images/mini/jump_free.png,mini/jump_free.png \
		-resource:images/jump_sj.png,jump_sj.png 			-resource:images/mini/jump_sj.png,mini/jump_sj.png \
		-resource:images/jump_sj_l.png,jump_sj_l.png 			-resource:images/mini/jump_sj_l.png,mini/jump_sj_l.png \
		-resource:images/jump_cmj.png,jump_cmj.png 			-resource:images/mini/jump_cmj.png,mini/jump_cmj.png \
		-resource:images/jump_abk.png,jump_abk.png 			-resource:images/mini/jump_abk.png,mini/jump_abk.png \
		-resource:images/jump_dj.png,jump_dj.png 			-resource:images/mini/jump_dj.png,mini/jump_dj.png \
		-resource:images/jump_rocket.png,jump_rocket.png 		-resource:images/mini/jump_rocket.png,mini/jump_rocket.png \
		-resource:images/jump_rj.png,jump_rj.png 			-resource:images/mini/jump_rj.png,mini/jump_rj.png \
		-resource:images/jump_rj_in.png,jump_rj_in.png 			-resource:images/mini/jump_rj_in.png,mini/jump_rj_in.png \
		-resource:images/run_simple.png,run_simple.png 			-resource:images/mini/run_simple.png,mini/run_simple.png \
		-resource:images/run_interval.png,run_interval.png 		-resource:images/mini/run_interval.png,mini/run_interval.png \

CHRONOJUMP_LIB =  -pkg:gtk-sharp-2.0 -pkg:glade-sharp-2.0 -r:System.Data -r:Mono.Data.SqliteClient -r:System.Web.Services
#CHRONOJUMP_LIB =  -pkg:gtk-sharp -pkg:glade-sharp -r:System.Data -r:Mono.Data.SqliteClient -r:System.Web.Services 


#-- Construccion del chronojump_mini que funciona por consola
CHRONOJUMP_MINI = chronojump_mini

CHRONOJUMP_MINI_DEP = src/chronojump_mini.cs chronopic.cs src/util.cs src/constants.cs 

all: $(CHRONOJUMP).exe $(CHRONOJUMP_MINI).exe


#-------------------------------
# Regla para compilar CHRONOJUMP (C#)
#-------------------------------

$(CHRONOJUMP).exe: NPlot.dll NPlot.Gtk.dll $(CHRONOJUMP_DEP) chronopic.cs glade/chronojump.glade
	 $(MCS) -debug $(CHRONOJUMP_DEP) $(RESOURCES_GLADE) $(RESOURCES_IMAGES) -unsafe chronopic.cs -r:NPlot.dll -r:NPlot.Gtk.dll -r:System.Drawing -r:Mono.Posix $(CHRONOJUMP_LIB) -nowarn:169 -out:$(CHRONOJUMP).exe 
#$(CHRONOJUMP).exe: $(CHRONOJUMP_DEP) chronopic.cs glade/chronojump.glade
#	 $(MCS) -debug $(CHRONOJUMP_DEP) $(RESOURCES_GLADE) $(RESOURCES_IMAGES) -unsafe chronopic.cs -r:System.Drawing -r:Mono.Posix $(CHRONOJUMP_LIB) -nowarn:169 -out:$(CHRONOJUMP).exe 
   
    
#------------------------------------
# Regla para compilar CHRONOJUMP_MINI (C#)
#------------------------------------

$(CHRONOJUMP_MINI).exe: $(CHRONOJUMP_MINI_DEP)
	 $(MCS) $(CHRONOJUMP_MINI_DEP) -r:Mono.Posix -out:$(CHRONOJUMP_MINI).exe 
    
#--------------------------
#  REGLAS GENERICAS
#--------------------------
.c.o:		
		$(CC) $(CFLAGS) -c $<

clean::
	  rm -f $(CHRONOJUMP).exe $(CHRONOJUMP_MINI).exe  
