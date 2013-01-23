
EXTRA_DIST =  

if ENABLE_DEBUG
ASSEMBLY_COMPILER_COMMAND = $(MCS)
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -unsafe -warn:4 -optimize+ -debug -define:DEBUG
ASSEMBLY = bin/Debug/CesarPlayer.dll
ASSEMBLY_MDB = $(ASSEMBLY).mdb
COMPILE_TARGET = library
PROJECT_REFERENCES = 
BUILD_DIR = bin/Debug
CESARPLAYER_DLL_MDB_SOURCE=bin/Debug/CesarPlayer.dll.mdb
CESARPLAYER_DLL_MDB=$(BUILD_DIR)/CesarPlayer.dll.mdb
CESARPLAYER_DLL_CONFIG_SOURCE = CesarPlayer.dll.config
CESARPLAYER_DLL_CONFIG = $(BUILD_DIR)/CesarPlayer.dll.config

endif

if ENABLE_RELEASE
ASSEMBLY_COMPILER_COMMAND = $(MCS)
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -unsafe -warn:4 -optimize+ 
ASSEMBLY = bin/Release/CesarPlayer.dll
ASSEMBLY_MDB = $(ASSEMBLY).mdb
COMPILE_TARGET = library
PROJECT_REFERENCES = 
BUILD_DIR = bin/Release
CESARPLAYER_DLL_MDB_SOURCE=
CESARPLAYER_DLL_MDB=
CESARPLAYER_DLL_CONFIG_SOURCE = CesarPlayer.dll.config
CESARPLAYER_DLL_CONFIG = $(BUILD_DIR)/CesarPlayer.dll.config


endif

AL=al2
SATELLITE_ASSEMBLY_NAME=$(notdir $(basename $(ASSEMBLY))).resources.dll

PROGRAMFILES = \
	$(CESARPLAYER_DLL_MDB)\
	$(CESARPLAYER_DLL_CONFIG)

RESGEN=resgen2
	
all: $(ASSEMBLY) $(PROGRAMFILES) 

FILES = \
	AssemblyInfo.cs \
	gtk-gui/generated.cs \
	Common/Constants.cs\
	Common/Enum.cs\
	Common/Handlers.cs\
	Player/GstPlayer.cs \
	Player/IPlayer.cs \
	Player/ObjectManager.cs \
	gtk-gui/LongoMatch.Gui.CapturerBin.cs \
	gtk-gui/LongoMatch.Gui.PlayerBin.cs \
	gtk-gui/LongoMatch.Gui.VolumeWindow.cs \
	Gui/CapturerBin.cs \
	Gui/PlayerBin.cs \
	Gui/VolumeWindow.cs \
	MultimediaFactory.cs \
	Utils/IFramesCapturer.cs \
	Utils/FramesCapturer.cs \
	Utils/IMetadataReader.cs \
	Utils/TimeString.cs \
	Capturer/CaptureProperties.cs \
	Capturer/GstCameraCapturer.cs \
	Capturer/FakeCapturer.cs \
	Capturer/ICapturer.cs \
	Capturer/LiveSourceTimer.cs \
	Capturer/ObjectManager.cs \
	Editor/GstVideoSplitter.cs \
	Editor/IVideoEditor.cs \
	Editor/IVideoSplitter.cs \
	Editor/VideoSegment.cs \
	Editor/EditorState.cs \
	Utils/Device.cs \
	Utils/MediaFile.cs \
	Utils/PreviewMediaFile.cs

DATA_FILES = 

RESOURCES = \
	gtk-gui/objects.xml \
	gtk-gui/gui.stetic 

EXTRAS = \
	cesarplayer.key\
	CesarPlayer.dll.config\
	AssemblyInfo.cs.in

REFERENCES =  \
	System \
	Mono.Posix \
	System.Drawing \
	$(GTK_SHARP_20_LIBS) \
	$(GLIB_SHARP_20_LIBS)

DLL_REFERENCES = 

CLEANFILES = $(PROGRAMFILES) 

include $(top_srcdir)/Makefile.include

$(eval $(call emit-deploy-target,CESARPLAYER_DLL_CONFIG))

$(eval $(call emit_resgen_targets))
$(build_xamlg_list): %.xaml.g.cs: %.xaml
	xamlg '$<'

$(ASSEMBLY) $(ASSEMBLY_MDB): $(build_sources) $(build_resources) $(build_datafiles) $(DLL_REFERENCES) $(PROJECT_REFERENCES) $(build_xamlg_list) $(build_satellite_assembly_list)
	mkdir -p $(shell dirname $(ASSEMBLY))
	$(ASSEMBLY_COMPILER_COMMAND) $(ASSEMBLY_COMPILER_FLAGS) -out:$(ASSEMBLY) -target:$(COMPILE_TARGET) $(build_sources_embed) $(build_resources_embed) $(build_references_ref)
