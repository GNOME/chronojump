
EXTRA_DIST =  chronojump_mini.cs ../images/report_web_style.css


if ENABLE_DEBUG
ASSEMBLY_COMPILER_COMMAND = $(GMCS)
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:4 -nowarn:169,649 -optimize+ -debug -define:DEBUG 
ASSEMBLY = bin/Debug/Chronojump.exe
ASSEMBLY_MDB = $(ASSEMBLY).mdb
COMPILE_TARGET = exe
BUILD_DIR = bin/Debug
CHRONOJUMP_EXE_MDB_SOURCE=bin/Debug/Chronojump.exe.mdb
CHRONOJUMP_MINI_EXE=bin/Debug/Chronojump_Mini.exe
endif

if ENABLE_RELEASE
ASSEMBLY_COMPILER_COMMAND = $(GMCS)
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:4 -nowarn:169,649 -optimize+ 
ASSEMBLY = bin/Release/Chronojump.exe
ASSEMBLY_MDB = 
COMPILE_TARGET = exe
BUILD_DIR = bin/Release
CHRONOJUMP_EXE_MDB_SOURCE=
CHRONOJUMP_MINI_EXE=bin/Release/Chronojump_Mini.exe
endif

AL=al2
SATELLITE_ASSEMBLY_NAME=$(notdir $(basename $(ASSEMBLY))).resources.dll

COMMONAPPLICATIONDATAROOT_IMAGES = \
	../images/chronojump_logo.png

COMMONAPPLICATIONDATA_ROOT = \
	../images/report_web_style.css
 

PROGRAMFILES = \
	$(CHRONOJUMP_MINI_EXE)

LINUX_DESKTOPAPPLICATIONS = \
	$(CHRONOJUMP_DESKTOP) 

COMMONAPPLICATIONDATAROOT_LOGO = \
	$(CHRONOJUMP_LOGO)

BINARIES = \
	$(CHRONOJUMP)\
	$(CHRONOJUMP_MINI)

	 


RESGEN=resgen2
	
all: $(ASSEMBLY) $(COMMONAPPLICATIONDATA_ROOT) $(COMMONAPPLICATIONDATAROOT_IMAGES) $(COMMONAPPLICATIONDATAROOT_LOGO) $(PROGRAMFILES) $(LINUX_DESKTOPAPPLICATIONS) $(BINARIES) 

FILES = \
	gui/chronojump.cs\
	gui/confirm.cs\
	gui/error.cs\
	gui/eventExecute.cs\
 	gui/eventGraphConfigure.cs\
 	gui/event.cs\
 	gui/jump.cs\
 	gui/jumpType.cs\
 	gui/run.cs\
 	gui/runType.cs\
 	gui/reactionTime.cs\
 	gui/pulse.cs\
 	gui/multiChronopic.cs\
 	gui/person.cs\
 	gui/preferences.cs\
 	gui/session.cs\
 	gui/stats.cs\
	gui/report.cs\
	gui/about.cs\
	gui/helpPorts.cs\
	gui/dialogMessage.cs\
 	gui/dialogCalendar.cs\
	gui/dialogImageTest.cs\
 	gui/language.cs\
	gui/repetitiveConditions.cs\
	gui/chronopicConnection.cs\
	gui/convertWeight.cs\
 	gui/genericWindow.cs\
 	gui/splash.cs\
	gui/server.cs\
	gui/dialogServerStats.cs\
 	gui/evaluator.cs\
 	gui/queryServer.cs\
	statType.cs\
	stats/main.cs\
	stats/global.cs\
	stats/sjCmjAbk.cs\
 	stats/sjCmjAbkPlus.cs\
 	stats/djIndex.cs\
 	stats/djQ.cs\
 	stats/rjIndex.cs\
 	stats/rjPotencyBosco.cs\
 	stats/rjEvolution.cs\
 	stats/ieIub.cs\
 	stats/fv.cs\
 	stats/potency.cs\
 	stats/rjAVGSD.cs\
	stats/graphs/graphData.cs\
 	stats/graphs/graphROptions.cs\
 	stats/graphs/graphSerie.cs\
 	stats/graphs/global.cs\
 	stats/graphs/sjCmjAbk.cs\
 	stats/graphs/sjCmjAbkPlus.cs\
 	stats/graphs/djIndex.cs\
 	stats/graphs/djQ.cs\
 	stats/graphs/rjIndex.cs\
 	stats/graphs/rjPotencyBosco.cs\
 	stats/graphs/rjEvolution.cs\
 	stats/graphs/ieIub.cs\
 	stats/graphs/fv.cs\
 	stats/graphs/potency.cs\
	stats/graphs/rjAVGSD.cs\
	sqlite/main.cs\
 	sqlite/preferences.cs\
 	sqlite/person.cs\
 	sqlite/session.cs\
 	sqlite/jump.cs\
 	sqlite/jumpRj.cs\
 	sqlite/jumpType.cs\
 	sqlite/run.cs\
 	sqlite/runInterval.cs\
 	sqlite/runType.cs\
 	sqlite/personSession.cs\
 	sqlite/personSessionNotUpload.cs\
 	sqlite/stat.cs\
 	sqlite/pulse.cs\
 	sqlite/pulseType.cs\
 	sqlite/reactionTime.cs\
 	sqlite/multiChronopic.cs\
 	sqlite/event.cs\
 	sqlite/sport.cs\
 	sqlite/speciallity.cs\
 	sqlite/country.cs\
 	sqlite/server.cs\
	execute/event.cs\
	execute/jump.cs\
	execute/run.cs\
	execute/pulse.cs\
	execute/reactionTime.cs\
	execute/multiChronopic.cs\
	../chronojump_server/ChronojumpServer.cs\
	AssemblyInfo.cs\
	chronojump.cs\
 	person.cs\
 	event.cs\
 	eventType.cs\
 	jump.cs\
 	jumpType.cs\
	run.cs\
 	runType.cs\
 	pulse.cs\
 	pulseType.cs\
 	reactionTime.cs\
 	reactionTimeType.cs\
 	multiChronopic.cs\
	multiChronopicType.cs\
 	session.cs\
 	exportSession.cs\
 	treeViewEvent.cs\
 	treeViewPerson.cs\
 	treeViewJump.cs\
 	treeViewRun.cs\
 	treeViewPulse.cs\
 	treeViewReactionTime.cs\
 	treeViewMultiChronopic.cs\
 	util.cs\
 	utilDate.cs\
	utilGtk.cs\
	constants.cs\
 	report.cs\
 	updateProgressBar.cs\
 	prepareEventGraphObjects.cs\
 	sport.cs\
 	log.cs\
 	serverPing.cs\
 	serverEvaluator.cs\
 	server.cs\
	chronopic.cs

CHRONOJUMP_MINI_FILES=\
	chronojump_mini.cs\
	chronopic.cs\
	util.cs\
	utilDate.cs\
	log.cs\
	constants.cs



#CHRONOJUMP_MINI_VALIDATE_FILES=


DATA_FILES = 

RESOURCES = \
	../glade/chronojump.glade\
	../images/mini/no_image.png,mini/no_image.png \
	../images/agility_505.png,agility_505.png\
	../images/mini/agility_505.png,mini/agility_505.png \
	../images/agility_20yard.png,agility_20yard.png\
	../images/mini/agility_20yard.png,mini/agility_20yard.png \
	../images/agility_illinois.png,agility_illinois.png\
 	../images/mini/agility_illinois.png,mini/agility_illinois.png \
	../images/agility_shuttle.png,agility_shuttle.png\
 	../images/mini/agility_shuttle.png,mini/agility_shuttle.png \
	../images/agility_zigzag.png,agility_zigzag.png\
	../images/mini/agility_zigzag.png,mini/agility_zigzag.png \
	../images/jump_free.png,jump_free.png\
	../images/mini/jump_free.png,mini/jump_free.png \
	../images/jump_sj.png,jump_sj.png\
	../images/mini/jump_sj.png,mini/jump_sj.png \
	../images/jump_sj_l.png,jump_sj_l.png\
	../images/mini/jump_sj_l.png,mini/jump_sj_l.png \
	../images/jump_cmj.png,jump_cmj.png\
	../images/mini/jump_cmj.png,mini/jump_cmj.png \
	../images/jump_cmj_l.png,jump_cmj_l.png\
	../images/mini/jump_cmj_l.png,mini/jump_cmj_l.png \
	../images/jump_abk.png,jump_abk.png\
	../images/mini/jump_abk.png,mini/jump_abk.png \
	../images/jump_abk_l.png,jump_abk_l.png\
	../images/mini/jump_abk_l.png,mini/jump_abk_l.png \
	../images/jump_max.png,jump_max.png\
	../images/mini/jump_max.png,mini/jump_max.png \
	../images/jump_dj.png,jump_dj.png\
	../images/mini/jump_dj.png,mini/jump_dj.png \
	../images/jump_rocket.png,jump_rocket.png \
	../images/mini/jump_rocket.png,mini/jump_rocket.png \
	../images/jump_rj.png,jump_rj.png \
	../images/mini/jump_rj.png,mini/jump_rj.png \
	../images/jump_rj_in.png,jump_rj_in.png \
	../images/mini/jump_rj_in.png,mini/jump_rj_in.png \
	../images/jump_rj_hexagon.png,jump_rj_hexagon.png \
 	../images/mini/jump_rj_hexagon.png,mini/jump_rj_hexagon.png \
	../images/run_simple.png,run_simple.png \
	../images/mini/run_simple.png,mini/run_simple.png \
	../images/run_interval.png,run_interval.png \
	../images/mini/run_interval.png,mini/run_interval.png \
	../images/margaria.png,margaria.png\
	../images/mini/margaria.png,mini/margaria.png \
	../images/gesell_dbt.png,gesell_dbt.png \
	../images/mini/gesell_dbt.png,mini/gesell_dbt.png \
	../images/multiChronopic.png,multiChronopic.png	\
	../images/mini/multiChronopic.png,mini/multiChronopic.png \
	../images/run_analysis.png,run_analysis.png \
	../images/mini/run_analysis.png,mini/run_analysis.png \
	../images/pulse_free.png,pulse_free.png \
	../images/mini/pulse_free.png,mini/pulse_free.png \
	../images/pulse_custom.png,pulse_custom.png \
	../images/mini/pulse_custom.png,mini/pulse_custom.png \
	../images/reaction_time.png,reaction_time.png \
	../images/mini/reaction_time.png,mini/reaction_time.png \
	../images/mtgug.png,mtgug.png	\
	../images/mini/mtgug.png,mini/mtgug.png \
	../images/take_off.png,take_off.png \
	../images/mini/take_off.png,mini/take_off.png \
	../images/chronopic1.jpg,chronopic1.jpg	\
	../images/mini/chronopic1.jpg,mini/chronopic1.jpg \
	../images/chronopic2.jpg,chronopic2.jpg \
	../images/mini/chronopic2.jpg,mini/chronopic2.jpg \
	../images/chronopic3.jpg,chronopic3.jpg	\
	../images/mini/chronopic3.jpg,mini/chronopic3.jpg \
	../images/modular_platform_with_chronopic.jpg,modular_platform_with_chronopic.jpg \
	../images/mini/modular_platform_with_chronopic.jpg,mini/modular_platform_with_chronopic.jpg \
	../images/plataforma_contactos.jpg,plataforma_contactos.jpg\
	../images/mini/plataforma_contactos.jpg,mini/plataforma_contactos.jpg \
	../images/infrared.jpg,infrared.jpg \
	../images/mini/infrared.jpg,mini/infrared.jpg \
	../images/stock_bell.png,stock_bell.png \
	../images/stock_bell_green.png,stock_bell_green.png \
	../images/stock_bell_red.png,stock_bell_red.png \
	../images/audio-volume-high.png,audio-volume-high.png \
	../images/audio-volume-muted.png,audio-volume-muted.png \
	../images/gpm-statistics.png,gpm-statistics.png \
	../images/stock_task-assigned.png,stock_task-assigned.png \
	../images/preferences-system.png,preferences-system.png \
	../images/stock_delete.png,stock_delete.png \
	../images/chronojump_icon.png,chronojump_icon.png \
	../images/chronojump_icon_graph.png,chronojump_icon_graph.png \
	../images/gtk-zoom-fit.png,gtk-zoom-fit.png \
	../images/gtk-zoom-in.png,gtk-zoom-in.png \
	../images/gtk-zoom-in-with-text.png,gtk-zoom-in-with-text.png \
	../images/chronojump_320.png,chronojump_320.png \
	../images/calendar.png,calendar.png \
	../images/chronojump_logo.png,chronojump_logo.png 


EXTRAS = \
	chronojump.in\
	chronojump_mini.in

REFERENCES =  \
	$(GTK_SHARP_20_LIBS) \
	$(GLIB_SHARP_20_LIBS) \
	$(GLADE_SHARP_20_LIBS) \
	System\
	System.Data\
	System.Drawing\
	System.Web.Services\
	System.Xml\
	Mono.Data.Sqlite\
	Mono.Posix



#DLL_REFERENCES =  \
#	../nplot/nplot/bin/NPlot.dll\
#	../nplot/nplot-gtk/bin/NPlot.Gtk.dll


#CLEANFILES = $(COMMONAPPLICATIONDATA_ROOT) $(COMMONAPPLICATIONDATAROOT_IMAGES) $(COMMONAPPLICATIONDATAROOT_LOGO) $(PROGRAMFILES) $(LINUX_DESKTOPAPPLICATIONS) $(BINARIES) 
CLEANFILES = $(PROGRAMFILES) $(LINUX_DESKTOPAPPLICATIONS) $(BINARIES) 


include $(top_srcdir)/Makefile.include


CHRONOJUMP_LOGO_SOURCE=../images/chronojump_icon.png
CHRONOJUMP_LOGO=$(BUILD_DIR)/chronojump.png
CHRONOJUMP_DESKTOP_SOURCE=chronojump.desktop
CHRONOJUMP_DESKTOP = $(BUILD_DIR)/chronojump.desktop
CHRONOJUMP_EXE_MDB = $(BUILD_DIR)/ChronoJump.exe.mdb
CHRONOJUMP=$(BUILD_DIR)/chronojump
CHRONOJUMP_MINI=$(BUILD_DIR)/chronojump_mini

$(eval $(call emit-deploy-wrapper,CHRONOJUMP,chronojump,x))
$(eval $(call emit-deploy-wrapper,CHRONOJUMP_MINI,chronojump_mini,x))
$(eval $(call emit-deploy-target,CHRONOJUMP_DESKTOP))
$(eval $(call emit-deploy-target,CHRONOJUMP_LOGO))


$(eval $(call emit_resgen_targets))
$(build_xamlg_list): %.xaml.g.cs: %.xaml
	xamlg '$<'

$(ASSEMBLY) $(ASSEMBLY_MDB): $(build_sources) $(build_resources) $(build_datafiles) $(DLL_REFERENCES) $(PROJECT_REFERENCES) $(build_xamlg_list) $(build_satellite_assembly_list)
	mkdir -p $(shell dirname $(ASSEMBLY))
	$(ASSEMBLY_COMPILER_COMMAND) $(ASSEMBLY_COMPILER_FLAGS) -out:$(ASSEMBLY) -target:$(COMPILE_TARGET) $(build_sources_embed) $(build_resources_embed) $(build_references_ref)


$(CHRONOJUMP_MINI_EXE):
	$(ASSEMBLY_COMPILER_COMMAND) $(ASSEMBLY_COMPILER_FLAGS) $(CHRONOJUMP_MINI_FILES) -out:$(CHRONOJUMP_MINI_EXE) -target:$(COMPILE_TARGET) -r:Mono.Posix -r:System

