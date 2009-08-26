EXTRA_DIST = NPlot.Gtk.dll.config

ASSEMBLY_COMPILER_COMMAND = $(GMCS)
ASSEMBLY_COMPILER_FLAGS =  -debug+
ASSEMBLY = bin/NPlot.Gtk.dll
ASSEMBLY_MDB = $(ASSEMBLY).mdb
COMPILE_TARGET = library
BUILD_DIR = bin/

RESGEN=resgen2


all: $(ASSEMBLY) $(PROGRAMFILES)


FILES = \
	Gtk.PlotSurface2D.cs 		\
	sysdraw.cs
	


REFERENCES =  \
		System.Web \
		System.Design \
		System.Drawing \
		System.Data \
		$(GTK_SHARP_20_LIBS)
	
	
DLL_REFERENCES = ../nplot/bin/NPlot.dll

PROGRAMFILES = \
	$(NPLOT_GTK_DLL_CONFIG)

CLEANFILES = $(PROGRAMFILES) 

include $(top_srcdir)/Makefile.include

NPLOT_GTK_DLL_CONFIG_SOURCE=./NPlot.Gtk.dll.config
NPLOT_GTK_DLL_CONFIG=$(BUILD_DIR)/NPlot.Gtk.dll.config
$(eval $(call emit-deploy-target,NPLOT_GTK_DLL_CONFIG))

$(eval $(call emit_resgen_targets))
$(build_xamlg_list): %.xaml.g.cs: %.xaml
	xamlg '$<'

$(ASSEMBLY) $(ASSEMBLY_MDB): $(build_sources) $(build_resources) $(build_datafiles) $(DLL_REFERENCES) $(PROJECT_REFERENCES) $(build_xamlg_list) $(build_satellite_assembly_list)
	mkdir -p $(shell dirname $(ASSEMBLY))
	$(ASSEMBLY_COMPILER_COMMAND) $(ASSEMBLY_COMPILER_FLAGS) -out:$(ASSEMBLY) -target:$(COMPILE_TARGET) $(build_sources_embed) $(build_resources_embed) $(build_references_ref)




