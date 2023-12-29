MKFILE_PATH := $(abspath $(lastword $(MAKEFILE_LIST)))
CUR_DIR := $(dir $(MKFILE_PATH))
BASE_DIR = "$(CUR_DIR)/../../"
SOURCES_DIR = "$(CUR_DIR)/../../src"
#TOOLS_DIR = "$(CUR_DIR)/../../tools"
PACKAGE_DIR = "$(CUR_DIR)/../../package/macos"
APP_DIR = "$(PACKAGE_DIR)/app/Chronojump.app/Contents/"
RESOURCES_DIR = "$(PACKAGE_DIR)/resources"
SIGNING = "$(PACKAGE_DIR)/../signing"
USER = $(shell whoami)
PACKAGE_VERSION = $(shell git describe --abbrev=1)
PACKAGE_NAME = "chronojump-$(PACKAGE_VERSION)-x86_64"
#BUILD_DIR = "$(SOURCES_DIR)/build"
#MONO_DIR = "/Library/Frameworks/Mono.framework/Versions/Current/"
#MONO_PKG_CONFIG = "$(MONO_DIR)/lib/pkgconfig"
#MONO_VERSION = 6.10.0
PATH := $(PATH):/usr/local/Cellar/gettext/0.20.1/bin

all: bundle package


configure:
	cd $(BASE_DIR) && PKG_CONFIG_PATH=/Library/Frameworks/Mono.framework/Versions/Current/lib/pkgconfig sh autogen.sh --prefix=$(APP_DIR)/Home/

chronojump:
	cd $(BASE_DIR) && make

install_chronojump:
	cd $(BASE_DIR) && make install

install_mono:
	# Copy the files that will not be embedded in the bundle
	cp -r $(MONO_DIR)/lib/lib*dylib $(APP_DIR)/Home/lib/
	cp $(MONO_DIR)/lib/*sharpglue*.so $(APP_DIR)/Home/lib/
	codesign --remove-signature $(APP_DIR)/Home/lib/lib*dylib
	python3 $(RESOURCES_DIR)/osxrelocator.py lib/ $(APP_DIR)/Home/ /Library/Frameworks/Mono.framework/Versions/$(MONO_VERSION)/ --recursive

bundle:
	# Generate Info.plist file
	cp $(PACKAGE_DIR)/resources/Info.plist $(APP_DIR)
	sed -e s/{version}/$(PACKAGE_VERSION)/ $(PACKAGE_DIR)/resources/Info.plist > $(APP_DIR)/Info.plist
	# The mono binary is signed, we need to remove the signature to add the rpath's
	# otherwise modifying the binary invalidates the signature
	cp $(MONO_DIR)/bin/mono $(PACKAGE_DIR)
	codesign --remove-signature $(PACKAGE_DIR)/mono
	install_name_tool -add_rpath @executable_path/.. -add_rpath @loader_path $(PACKAGE_DIR)/mono
	mkbundle \
		$(APP_DIR)/Home/lib/chronojump/Chronojump.exe \
		-o $(PACKAGE_DIR)/Chronojump \
		-L $(MONO_DIR)/lib/mono/4.5/ \
		-L $(MONO_DIR)/lib/mono/4.5/Facades \
		-L $(MONO_DIR)/lib/mono/gtk-sharp-2.0 \
		--runtime $(PACKAGE_DIR)/mono \
		--config $(RESOURCES_DIR)/config \
	    --simple \
		--deps
	cp $(PACKAGE_DIR)/Chronojump $(APP_DIR)/Home/bin/


package:
	hdiutil create -volname Chronojump -srcfolder app -ov -format UDZO Chronojump-$(PACKAGE_VERSION).dmg
