# Initializers
MONO_BASE_PATH = 
MONO_ADDINS_PATH =

# Install Paths
DEFAULT_INSTALL_DIR = $(pkglibdir)

# External libraries to link against, generated from configure
LINK_SYSTEM = -r:System
LINK_SYSTEM_CORE = -r:System.Core
LINK_SYSTEM_DATA_DATASETEXTENSTIONS = -r:System.Data.DataSetExtensions
LINK_SYSTEM_DRAWING = -r:System.Drawing
LINK_SYSTEM_NUMERICS = -r:System.Numerics
LINK_SYSTEM_XML = -r:System.Xml
LINK_SYSTEM_XML_LINQ = -r:System.Xml.Linq
LINK_MICROSOFT_CSHARP = -r:Microsoft.CSharp
LINK_CAIRO = -r:Mono.Cairo
LINK_MONO_POSIX = -r:Mono.Posix
LINK_GLIB = $(GLIBSHARP_LIBS)
LINK_GTK = $(GTKSHARP_LIBS)
LINK_GCONF = $(GCONFSHARP_LIBS)
LINK_RDOTNET = -r:$(DIR_BIN)/RDotNet.dll
LINK_RDOTNET_NATIVE = -r:$(DIR_BIN)/RDotNet.NativeLibrary.dll


REF_DEP_RDOTNET_NATIVE = \
	$(LINK_SYSTEM) \
	$(LINK_SYSTEM_CORE) \
	$(LINK_SYSTEM_DATA_DATASETEXTENSTIONS) \
	$(LINK_SYSTEM_XML) \
	$(LINK_SYSTEM_XML_LINQ) \
	$(LINK_MICROSOFT_CSHARP)

REF_DEP_RDOTNET = \
	$(LINK_SYSTEM) \
	$(LINK_SYSTEM_CORE) \
	$(LINK_SYSTEM_DATA) \
	$(LINK_SYSTEM_DATA_DATASETEXTENSTIONS) \
	$(LINK_SYSTEM_NUMERICS) \
	$(LINK_SYSTEM_XML) \
	$(LINK_SYSTEM_XML_LINQ) \
	$(LINK_MICROSOFT_CSHARP) \
	$(LINK_RDOTNET_NATIVE)



DIR_BIN = $(top_builddir)/bin

# Cute hack to replace a space with something
colon:= :
empty:=
space:= $(empty) $(empty)

# Build path to allow running uninstalled
RUN_PATH = $(subst $(space),$(colon), $(MONO_BASE_PATH))

