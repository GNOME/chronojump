ASSEMBLY = CesarPlayer
TARGET = library

LINK = $(REF_DEP_CESARPLAYER)

SOURCES = \
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

RESOURCES = \
	gtk-gui/objects.xml \
	gtk-gui/gui.stetic

DLLCONFIG = CesarPlayer.dll.config

include $(top_srcdir)/build/build.mk

EXTRA_DIST += CesarPlayer.dll.config
