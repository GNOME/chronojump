# Initializers
MONO_BASE_PATH = 
MONO_ADDINS_PATH =

# Install Paths
DEFAULT_INSTALL_DIR = $(pkglibdir)

# External libraries to link against, generated from configure
LINK_SYSTEM = -r:System
LINK_SYSTEM_CORE = -r:System.Core
LINK_SYSTEM_DATA_DATASETEXTENSTIONS = -r:System.Data.DataSetExtensions
LINK_SYSTEMDATA = -r:System.Data
LINK_SYSTEM_DRAWING = -r:System.Drawing
LINK_SYSTEM_NUMERICS = -r:System.Numerics
LINK_SYSTEM_XML = -r:System.Xml
LINK_SYSTEM_XML_LINQ = -r:System.Xml.Linq
LINK_SYSTEM_WEB_SERVICES = -r:System.Web.Services
LINK_MICROSOFT_CSHARP = -r:Microsoft.CSharp
#LINK_CAIRO = -r:Mono.Cairo #Commented because cairo linking is duplicated "error CS0433: The imported type `Cairo.Context' is defined multiple times"
LINK_MONO_POSIX = -r:Mono.Posix
LINK_MONO_DATA_SQLITE = -r:Mono.Data.Sqlite
LINK_GLIB = $(GLIB_SHARP_30_LIBS)
LINK_GTK = $(GTK_SHARP_30_LIBS)
LINK_JSON = -r:System.Json.dll

REF_DEP_CHRONOJUMP = \
	$(LINK_SYSTEM) \
	$(LINK_SYSTEMDATA) \
	$(LINK_SYSTEM_DRAWING) \
	$(LINK_SYSTEM_WEB_SERVICES) \
	$(LINK_SYSTEM_XML) \
	$(LINK_MONO_POSIX) \
	$(LINK_MONO_DATA_SQLITE) \
	$(LINK_GLIB) \
	$(LINK_GLADE) \
	$(LINK_GTK) \
	$(LINK_JSON) #\
	$(LINK_CAIRO)

REF_DEP_CHRONOJUMP_SERVER = \
	$(LINK_SYSTEM) \
	$(LINK_SYSTEMDATA) \
	$(LINK_SYSTEM_WEB_SERVICES) \
	$(LINK_MONO_POSIX) \
	$(LINK_MONO_DATA_SQLITE)


DIR_BIN = $(top_builddir)/bin

# Cute hack to replace a space with something
colon:= :
empty:=
space:= $(empty) $(empty)

# Build path to allow running uninstalled
RUN_PATH = $(subst $(space),$(colon), $(MONO_BASE_PATH))

