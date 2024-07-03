MKFILE_PATH := $(abspath $(lastword $(MAKEFILE_LIST)))
CUR_DIR := $(dir $(MKFILE_PATH))
BASE_DIR = "$(CUR_DIR)/../../"
SOURCES_DIR = "$(CUR_DIR)/../../src"
PACKAGE_DIR = "$(CUR_DIR)/../../package/macos"
APP_DIR = "$(PACKAGE_DIR)/app/Chronojump.app/Contents"
APP_BIN_DIR = "$(APP_DIR)Home/bin/"
RESOURCES_DIR = "$(PACKAGE_DIR)/resources"
SIGNING = "$(PACKAGE_DIR)/../signing"
USER = $(shell whoami)
PACKAGE_VERSION = $(shell git describe --abbrev=1)
PACKAGE_NAME = "chronojump-$(PACKAGE_VERSION)"

#all: bundle
x64: bundle package-x64
arm64: bundle package-arm64

bundle:
	# Generate Info.plist file
	cp $(PACKAGE_DIR)/resources/Info.plist $(APP_DIR)
	sed -e s/{version}/$(PACKAGE_VERSION)/ $(PACKAGE_DIR)/resources/Info.plist > $(APP_DIR)/Info.plist

	mkdir -p deps/bin/x64/Python
	mkdir -p deps/R
	cp -r ../../win32/deps/bin/chronojump-importer-python3/ deps/bin/chronojump-importer-python3/
	cp -r ../../win32/deps/bin/FTD2XX_NET_v1.0.14/ deps/bin/FTD2XX_NET_v1.0.14/
	cp -r ../../win32/deps/etc/ deps/etc/
	cp -r ../../win32/deps/fonts/ deps/fonts/
	cp -r ../../win32/deps/FTD2XX_NET_v1.2.0/ deps/FTD2XX_NET_v1.2.0/
	cp -r ../../win32/deps/library/ deps/library/
	cp -r ../../win32/deps/modules/ deps/modules/
	rm -rf deps/bin/x64/Python/
	cp -r /Library/Frameworks/Python.framework/ deps/bin/x64/Python/
	rm deps/bin/x64/Python/Python
	rm -rf deps/R/
	cp -r /Library/Frameworks/R.framework/ deps/R/
	rm deps/R/R
	dos2unix ./build_installer.sh

package-x64:	
	sh ./build_installer.sh $(PACKAGE_NAME) x64

package-arm64:	
	sh ./build_installer.sh $(PACKAGE_NAME) arm64