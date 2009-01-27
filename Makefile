# **************************************************************************
# Makefile.
# --------------------------------------------------------------------------
# Licencia GPL. Juan Gonzalez Gomez, Xavier de Blas Foix
# --------------------------------------------------------------------------
#
#***************************************************************************


#---- C Compilator
CC = gcc
CFLAGS = -Wall

#-- C# Compilator
MCS = gmcs

BUILD_DIR = build/data

#-- WSDL Client Compilator
WSDL = chronojump_server/compile_wsdl.sh

#-------- Names of programs to build


CHRONOJUMP = chronojump

CHRONOJUMP_MINI = chronojump_mini
#CHRONOJUMP_MINI_VALIDATE = chronojump_mini_validate

CHRONOJUMP_SERVER = chronojump_server


#--------Dependences of CHRONOJUMP


CHRONOJUMP_DEP_GUI = src/gui/chronojump.cs src/gui/confirm.cs src/gui/error.cs src/gui/eventExecute.cs src/gui/eventGraphConfigure.cs src/gui/event.cs src/gui/jump.cs src/gui/jumpType.cs src/gui/run.cs src/gui/runType.cs src/gui/reactionTime.cs src/gui/pulse.cs src/gui/person.cs src/gui/preferences.cs src/gui/session.cs src/gui/stats.cs src/gui/report.cs src/gui/about.cs src/gui/helpPorts.cs src/gui/dialogMessage.cs src/gui/dialogCalendar.cs src/gui/dialogImageTest.cs src/gui/language.cs src/gui/repetitiveConditions.cs src/gui/chronopicConnection.cs src/gui/convertWeight.cs src/gui/genericWindow.cs src/gui/splash.cs src/gui/server.cs

CHRONOJUMP_DEP_STATS = src/statType.cs src/stats/main.cs src/stats/global.cs src/stats/sjCmjAbk.cs src/stats/sjCmjAbkPlus.cs src/stats/djIndex.cs src/stats/djQ.cs src/stats/rjIndex.cs src/stats/rjPotencyBosco.cs src/stats/rjEvolution.cs src/stats/ieIub.cs src/stats/fv.cs src/stats/potency.cs src/stats/rjAVGSD.cs

CHRONOJUMP_DEP_GRAPHS = src/stats/graphs/graphData.cs src/stats/graphs/graphSerie.cs src/stats/graphs/global.cs src/stats/graphs/sjCmjAbk.cs src/stats/graphs/sjCmjAbkPlus.cs src/stats/graphs/djIndex.cs src/stats/graphs/djQ.cs src/stats/graphs/rjIndex.cs src/stats/graphs/rjPotencyBosco.cs src/stats/graphs/rjEvolution.cs src/stats/graphs/ieIub.cs src/stats/graphs/fv.cs src/stats/graphs/potency.cs src/stats/graphs/rjAVGSD.cs

CHRONOJUMP_DEP_SQLITE = src/sqlite/main.cs src/sqlite/preferences.cs src/sqlite/person.cs src/sqlite/session.cs src/sqlite/jump.cs src/sqlite/jumpRj.cs src/sqlite/jumpType.cs src/sqlite/run.cs src/sqlite/runInterval.cs src/sqlite/runType.cs src/sqlite/personSession.cs src/sqlite/stat.cs src/sqlite/pulse.cs src/sqlite/pulseType.cs src/sqlite/reactionTime.cs src/sqlite/event.cs src/sqlite/sport.cs src/sqlite/speciallity.cs src/sqlite/country.cs src/sqlite/server.cs

CHRONOJUMP_DEP_EXECUTE = src/execute/event.cs src/execute/jump.cs src/execute/run.cs src/execute/pulse.cs src/execute/reactionTime.cs

CHRONOJUMP_DEP_SERVER = chronojump_server/ChronojumpServer.cs

CHRONOJUMP_DEP = src/chronojump.cs src/person.cs src/event.cs src/eventType.cs src/jump.cs src/jumpType.cs src/run.cs src/runType.cs src/pulse.cs src/pulseType.cs src/reactionTime.cs src/reactionTimeType.cs src/session.cs src/exportSession.cs src/treeViewEvent.cs src/treeViewPerson.cs src/treeViewJump.cs src/treeViewRun.cs src/treeViewPulse.cs src/treeViewReactionTime.cs src/util.cs src/utilGtk.cs src/constants.cs src/report.cs src/updateProgressBar.cs src/prepareEventGraphObjects.cs src/sport.cs src/log.cs src/serverPing.cs src/serverEvaluator.cs $(CHRONOJUMP_DEP_GUI) $(CHRONOJUMP_DEP_STATS) $(CHRONOJUMP_DEP_GRAPHS) $(CHRONOJUMP_DEP_SQLITE) $(CHRONOJUMP_DEP_REPORT) $(CHRONOJUMP_DEP_EXECUTE) $(CHRONOJUMP_DEP_SERVER)

RESOURCES_GLADE = -resource:glade/chronojump.glade,chronojump.glade

RESOURCES_IMAGES = -resource:images/mini/no_image.png,mini/no_image.png \
		-resource:images/agility_505.png,agility_505.png 		-resource:images/mini/agility_505.png,mini/agility_505.png \
		-resource:images/agility_20yard.png,agility_20yard.png		-resource:images/mini/agility_20yard.png,mini/agility_20yard.png \
		-resource:images/agility_hexagon.png,agility_hexagon.png 	-resource:images/mini/agility_hexagon.png,mini/agility_hexagon.png \
		-resource:images/agility_illinois.png,agility_illinois.png 	-resource:images/mini/agility_illinois.png,mini/agility_illinois.png \
		-resource:images/agility_shuttle.png,agility_shuttle.png 	-resource:images/mini/agility_shuttle.png,mini/agility_shuttle.png \
		-resource:images/agility_zigzag.png,agility_zigzag.png 		-resource:images/mini/agility_zigzag.png,mini/agility_zigzag.png \
		-resource:images/jump_free.png,jump_free.png 			-resource:images/mini/jump_free.png,mini/jump_free.png \
		-resource:images/jump_sj.png,jump_sj.png 			-resource:images/mini/jump_sj.png,mini/jump_sj.png \
		-resource:images/jump_sj_l.png,jump_sj_l.png 			-resource:images/mini/jump_sj_l.png,mini/jump_sj_l.png \
		-resource:images/jump_cmj.png,jump_cmj.png 			-resource:images/mini/jump_cmj.png,mini/jump_cmj.png \
		-resource:images/jump_cmj_l.png,jump_cmj_l.png 			-resource:images/mini/jump_cmj_l.png,mini/jump_cmj_l.png \
		-resource:images/jump_abk.png,jump_abk.png 			-resource:images/mini/jump_abk.png,mini/jump_abk.png \
		-resource:images/jump_abk_l.png,jump_abk_l.png 			-resource:images/mini/jump_abk_l.png,mini/jump_abk_l.png \
		-resource:images/jump_dj.png,jump_dj.png 			-resource:images/mini/jump_dj.png,mini/jump_dj.png \
		-resource:images/jump_rocket.png,jump_rocket.png 		-resource:images/mini/jump_rocket.png,mini/jump_rocket.png \
		-resource:images/jump_rj.png,jump_rj.png 			-resource:images/mini/jump_rj.png,mini/jump_rj.png \
		-resource:images/jump_rj_in.png,jump_rj_in.png 			-resource:images/mini/jump_rj_in.png,mini/jump_rj_in.png \
		-resource:images/run_simple.png,run_simple.png 			-resource:images/mini/run_simple.png,mini/run_simple.png \
		-resource:images/run_interval.png,run_interval.png 		-resource:images/mini/run_interval.png,mini/run_interval.png \
		-resource:images/pulse_free.png,pulse_free.png 			-resource:images/mini/pulse_free.png,mini/pulse_free.png \
		-resource:images/pulse_custom.png,pulse_custom.png		-resource:images/mini/pulse_custom.png,mini/pulse_custom.png \
		-resource:images/mtgug.png,mtgug.png				-resource:images/mini/mtgug.png,mini/mtgug.png \
		-resource:images/stock_bell.png,stock_bell.png \
		-resource:images/stock_bell_green.png,stock_bell_green.png \
		-resource:images/stock_bell_red.png,stock_bell_red.png \
		-resource:images/audio-volume-high.png,audio-volume-high.png \
		-resource:images/audio-volume-muted.png,audio-volume-muted.png \
		-resource:images/gpm-statistics.png,gpm-statistics.png \
		-resource:images/stock_task-assigned.png,stock_task-assigned.png \
		-resource:images/preferences-system.png,preferences-system.png \
		-resource:images/stock_delete.png,stock_delete.png \
		-resource:images/chronojump_icon.png,chronojump_icon.png \
		-resource:images/chronojump_icon_graph.png,chronojump_icon_graph.png \
		-resource:images/gtk-zoom-fit.png,gtk-zoom-fit.png \
		-resource:images/gtk-zoom-in.png,gtk-zoom-in.png \
		-resource:images/gtk-zoom-in-with-text.png,gtk-zoom-in-with-text.png \
		-resource:images/chronojump_320.png,chronojump_320.png \
		
#-resource:images/gtk-zoom-out.png,gtk-zoom-out.png \

#logo is included as assemblie and as a file (with create_release.sh and installjammer)
#report_web_style.css only as a file (there were problems when copying into file at report with stream)
RESOURCES_REPORT = -resource:images/chronojump_logo.png,chronojump_logo.png \
		#-resource:images/report_web_style.css,report_web_style.css \


CHRONOJUMP_LIB =  -pkg:gtk-sharp-2.0 -pkg:glade-sharp-2.0 -r:System.Data -r:Mono.Data.Sqlite -r:System.Web.Services

NPLOT_LIBS = build/data/linux_dlls

#--------Dependences of CHRONOJUMP_MINI

CHRONOJUMP_MINI_DEP = src/chronojump_mini.cs src/chronopic.cs src/util.cs src/log.cs src/constants.cs 
#CHRONOJUMP_MINI_VALIDATE_DEP = src/chronojump_mini_validate.cs src/chronopic.cs src/util.cs src/constants.cs 

#--------Dependences of CHRONOJUMP_SERVER

CHRONOJUMP_SERVER_DEP = chronojump_server/chronojumpServerCSharp.cs src/sqlite/*.cs src/util.cs src/person.cs src/event.cs src/jump.cs src/run.cs src/pulse.cs src/reactionTime.cs src/session.cs src/eventType.cs src/jumpType.cs src/runType.cs src/pulseType.cs src/sport.cs src/constants.cs src/log.cs src/serverPing.cs src/serverEvaluator.cs


#--------Makefiles

#chronojump and chronojump_mini (default if used 'make')
all: $(CHRONOJUMP).prg $(CHRONOJUMP_MINI).prg
server: $(CHRONOJUMP_SERVER)
#all: $(CHRONOJUMP).prg $(CHRONOJUMP_MINI).prg $(CHRONOJUMP_MINI_VALIDATE).prg

#chronojump_server: use 'make server'
server: $(CHRONOJUMP_SERVER).dll


#-------------------------------
# Compile CHRONOJUMP (C#)
#-------------------------------

$(CHRONOJUMP).prg: $(NPLOT_LIBS)/NPlot.dll $(NPLOT_LIBS)/NPlot.Gtk.dll $(CHRONOJUMP_DEP) src/chronopic.cs glade/chronojump.glade
	cp version.txt $(BUILD_DIR)
	./compile_po_files.sh #update translations
	$(MCS) -debug $(CHRONOJUMP_DEP) $(RESOURCES_GLADE) $(RESOURCES_IMAGES) $(RESOURCES_REPORT) -unsafe src/chronopic.cs -r:$(NPLOT_LIBS)/NPlot.dll -r:$(NPLOT_LIBS)/NPlot.Gtk.dll -r:System.Drawing -r:Mono.Posix $(CHRONOJUMP_LIB) -nowarn:169 -out:$(BUILD_DIR)/$(CHRONOJUMP).prg 
   
    
#------------------------------------
# Compile CHRONOJUMP_MINI (C#)
#------------------------------------

$(CHRONOJUMP_MINI).prg: $(CHRONOJUMP_MINI_DEP)
	 $(MCS) $(CHRONOJUMP_MINI_DEP) -r:Mono.Posix -out:$(BUILD_DIR)/$(CHRONOJUMP_MINI).prg 

#$(CHRONOJUMP_MINI_VALIDATE).prg: $(CHRONOJUMP_MINI_VALIDATE_DEP)
#	 $(MCS) $(CHRONOJUMP_MINI_VALIDATE_DEP) -r:Mono.Posix -out:$(BUILD_DIR)/$(CHRONOJUMP_MINI_VALIDATE).prg 
   

#------------------------------------
# Compile server webservice & WSDL
#------------------------------------

$(CHRONOJUMP_SERVER).dll: $(CHRONOJUMP_SERVER_DEP) chronojump_server/chronojumpServer.asmx
	$(MCS) -t:library -out:chronojump_server/bin/chronojumpServer.dll -r:System.Data -r:Mono.Data.Sqlite -r:System.Web.Services -r:Mono.Posix $(CHRONOJUMP_SERVER_DEP)
#currently deactivated WSDL compilation: (seems it doesn't work because when there's no network, there's no localhost)
#$(WSDL)


#--------------------------
#  GENERIC rules
#--------------------------
.c.o:		
		$(CC) $(CFLAGS) -c $<

clean::
	  rm -f $(BUILD_DIR)/$(CHRONOJUMP).prg $(BUILD_DIR)/$(CHRONOJUMP_MINI).prg
