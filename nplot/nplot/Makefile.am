EXTRA_DIST = StrongName.snk

ASSEMBLY_COMPILER_COMMAND = $(GMCS)
ASSEMBLY_COMPILER_FLAGS =  -debug+
ASSEMBLY = bin/NPlot.dll
ASSEMBLY_MDB = $(ASSEMBLY).mdb
COMPILE_TARGET = library
BUILD_DIR = bin/

RESGEN=resgen2

SERVER=server
	
#all: $(ASSEMBLY) $(SERVER)
all: $(ASSEMBLY)


FILES = \
	AdapterUtils.cs			\
	NPlotException.cs 		\
	ArrowItem.cs			\
	AssemblyInfo.cs			\
	AxesConstraint.cs		\
	Axis.cs				\
	BasePlot.cs			\
	BaseSequenceLinePlot.cs		\
	BaseSequencePlot.cs		\
	Bitmap.PlotSurface2D.cs		\
	CandlePlot.cs			\
	DateTimeAxis.cs			\
	ErrorHandler.cs			\
	FilledRegion.cs			\
	FontScaler.cs			\
	Grid.cs				\
	HistogramPlot.cs		\
	HorizontalLine.cs		\
	IDrawable.cs			\
	IGradient.cs			\
	ImagePlot.cs			\
	IMeshPlot.cs			\
	IPlot.cs			\
	IPlotSurface2D.cs		\
	ITransform2D.cs			\
	ISequencePlot.cs		\
	LabelAxis.cs			\
	LabelPointPlot.cs		\
	LegendBase.cs			\
	Legend.cs			\
	LinearAxis.cs			\
	LinearGradient.cs		\
	LinePlot.cs			\
	LogAxis.cs			\
	Marker.cs			\
	MarkerItem.cs			\
	PageAlignedPhysicalAxis.cs	\
	PhysicalAxis.cs			\
	PiAxis.cs			\
	PiePlot.cs			\
	PlotSurface2D.cs		\
	PlotSurface3D.cs		\
	PointD.cs			\
	PointPlot.cs			\
	RectangleBrushes.cs		\
	RectangleD.cs			\
	SequenceAdapter.cs		\
	StartStep.cs			\
	StepPlot.cs			\
	Transform2D.cs			\
	Utils.cs			\
	VerticalLine.cs			\
	Web.Design.PlotSurface2D.cs	\
	Web.PlotSurface2D.cs
	


REFERENCES =  \
	System.Web \
	System.Design \
	System.Drawing \
	System.Data
	
DLL_REFERENCES = 

CLEANFILES = $(PROGRAMFILES) 

include $(top_srcdir)/Makefile.include

$(eval $(call emit_resgen_targets))
$(build_xamlg_list): %.xaml.g.cs: %.xaml
	xamlg '$<'

$(ASSEMBLY) $(ASSEMBLY_MDB): $(build_sources) $(build_resources) $(build_datafiles) $(DLL_REFERENCES) $(PROJECT_REFERENCES) $(build_xamlg_list) $(build_satellite_assembly_list)
	mkdir -p $(shell dirname $(ASSEMBLY))
	$(ASSEMBLY_COMPILER_COMMAND) $(ASSEMBLY_COMPILER_FLAGS) -out:$(ASSEMBLY) -target:$(COMPILE_TARGET) $(build_sources_embed) $(build_resources_embed) $(build_references_ref)




