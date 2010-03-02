EXTRA_DIST =


##########chronojumpServer.dll##########

ASSEMBLY_COMPILER_COMMAND = $(GMCS)
ASSEMBLY_COMPILER_FLAGS =  -noconfig -unsafe -codepage:utf8 -warn:4 -optimize+
ASSEMBLY = bin/chronojumpServer.dll
ASSEMBLY_MDB = $(ASSEMBLY).mdb
COMPILE_TARGET = library
BUILD_DIR = bin/

RESGEN=resgen2

SERVER=server
	
#all: $(ASSEMBLY) $(SERVER)
all: $(ASSEMBLY)


FILES = \
	chronojumpServerCSharp.cs\
	BCrypt.cs\
	../src/sqlite/*.cs\
	../src/util.cs\
	../src/utilDate.cs\
	../src/event.cs\
	../src/jump.cs\
	../src/run.cs\
	../src/person.cs\
	../src/personSession.cs\
	../src/pulse.cs\
	../src/reactionTime.cs\
	../src/session.cs\
	../src/eventType.cs\
	../src/jumpType.cs\
	../src/runType.cs\
	../src/pulseType.cs\
	../src/constants.cs\
 	../src/multiChronopic.cs\
	../src/sport.cs\
 	../src/log.cs\
	../src/serverPing.cs\
	../src/serverEvaluator.cs\
	../src/oldCodeNeedToDBConvert/person.cs\
	../src/oldCodeNeedToDBConvert/personSession.cs\
	../src/oldCodeNeedToDBConvert/sqlite/*.cs\
	


REFERENCES =  \
	System\
	System.Data\
	Mono.Data.Sqlite\
	System.Web.Services\
	Mono.Posix
	
DLL_REFERENCES = 

CLEANFILES = $(PROGRAMFILES) 

include $(top_srcdir)/Makefile.include

$(eval $(call emit_resgen_targets))
$(build_xamlg_list): %.xaml.g.cs: %.xaml
	xamlg '$<'

$(ASSEMBLY) $(ASSEMBLY_MDB): $(build_sources) $(build_resources) $(build_datafiles) $(DLL_REFERENCES) $(PROJECT_REFERENCES) $(build_xamlg_list) $(build_satellite_assembly_list)
	mkdir -p $(shell dirname $(ASSEMBLY))
	$(ASSEMBLY_COMPILER_COMMAND) $(ASSEMBLY_COMPILER_FLAGS) -out:$(ASSEMBLY) -target:$(COMPILE_TARGET) $(build_sources_embed) $(build_resources_embed) $(build_references_ref)




