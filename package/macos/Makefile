MKFILE_PATH := $(abspath $(lastword $(MAKEFILE_LIST)))
CUR_DIR := $(dir $(MKFILE_PATH))
BASE_DIR = "$(CUR_DIR)/../../"
SOURCES_DIR = "$(CUR_DIR)/../../src"
PACKAGE_DIR = "$(CUR_DIR)/../../package/macos"
APP_DIR = "$(PACKAGE_DIR)/app/Chronojump.app/Contents"
#APP_BIN_DIR = "$(APP_DIR)Home/bin/"
RESOURCES_DIR = "$(PACKAGE_DIR)/resources"
SIGNING = "$(PACKAGE_DIR)/../signing"
USER = $(shell whoami)
PACKAGE_VERSION = $(shell git describe --abbrev=1)
PACKAGE_SHORT_VERSION = $(shell git describe | sed 's/-.*//')
#ARCH=$(shell uname -m)
#PACKAGE_NAME = "chronojump-$(PACKAGE_VERSION)"

#all: bundle
dmg: bundle package-dmg
pkg: bundle package-pkg

bundle:
	rm -rf deps/FTD2XX_NET_v1.2.0
	rm -rf deps/fonts
	rm -rf deps/bin/FTD2XX_NET_v1.0.14
	rm -rf app/Chronojump.app
	mkdir -p app/Chronojump.app/Contents/MacOS
	mkdir -p app/Chronojump.app/Contents/Resources

	# Generate Info.plist file
	cp $(PACKAGE_DIR)/resources/Info.plist $(APP_DIR)
	sed -e s/{version}/$(PACKAGE_SHORT_VERSION)/ $(PACKAGE_DIR)/resources/Info.plist > $(APP_DIR)/Info.plist
	cp $(PACKAGE_DIR)/resources/icon.icns $(APP_DIR)/Resources

	mkdir -p deps/bin/x64/Python
	mkdir -p deps/R
	cp -r ../../win32/deps/bin/chronojump-importer-python3/ deps/bin/chronojump-importer-python3/
	#cp -r ../../win32/deps/bin/FTD2XX_NET_v1.0.14/ deps/bin/FTD2XX_NET_v1.0.14/
	cp -r ../../win32/deps/etc/ deps/etc/
	#cp -r ../../win32/deps/fonts/ deps/fonts/
	#cp -r ../../win32/deps/FTD2XX_NET_v1.2.0/ deps/FTD2XX_NET_v1.2.0/
	cp -r ../../win32/deps/library/ deps/library/
	cp -r ../../win32/deps/modules/ deps/modules/
	rm -rf deps/bin/x64/Python/
	cp -r /Library/Frameworks/Python.framework/ deps/bin/x64/Python/
	find deps/bin/x64/Python/ -type f -name "Info.plist" -exec rm {} \;
	#find deps/bin/x64/Python/ -type d -name "*.app" -exec rm -rf {} \;
	#find deps/bin/x64/Python/ -type d -name "*.dSYM" -exec rm -rf {} \;
	rm -rf deps/R/
	cp -r /Library/Frameworks/R.framework/ deps/R/
	rm deps/R/R
	find deps/R/ -type f -name "Info.plist" -exec rm {} \;
	#find deps/R/ -type d -name "*.app" -exec rm -rf {} \;
	#find deps/R/ -type d -name "*.dSYM" -exec rm -rf {} \;
	dos2unix ./build_installer.sh

package-dmg:	
	sh ./build_installer.sh $(PACKAGE_VERSION) dmg

package-pkg:	
	sh ./build_installer.sh $(PACKAGE_VERSION) pkg