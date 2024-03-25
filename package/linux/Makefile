MKFILE_PATH := $(abspath $(lastword $(MAKEFILE_LIST)))
CUR_DIR := $(dir $(MKFILE_PATH))
BASE_DIR = "$(CUR_DIR)/../../"
SOURCES_DIR = "$(CUR_DIR)/../../src"
PACKAGE_DIR = "$(CUR_DIR)/../../package/linux"
USER = $(shell whoami)
PACKAGE_VERSION = $(shell git describe --abbrev=1)
PACKAGE_NAME = "chronojump-$(PACKAGE_VERSION)"
APP_DIR = "$(PACKAGE_DIR)/$(PACKAGE_NAME)/"

all: bundle package

configure:
	cd $(BASE_DIR) && sh autogen.sh --prefix=$(PACKAGE_DIR)/deps
chronojump:
	cd $(BASE_DIR) && make

install_chronojump:
	cd $(BASE_DIR) && make install

bundle:
	dotnet publish ../../src/Chronojump-linux.sln -p:BuildTranslations=true --configuration Release -r linux-x64 --self-contained true -o $(APP_DIR)
	cd ../../src/
	sh post-build-linux.sh $(APP_DIR)
	cd ../package/linux
	mv $(APP_DIR)Chronojump-linux $(APP_DIR)Chronojump
	cp -r ../../win32/deps/bin/chronojump-importer-python3/ deps/bin/chronojump-importer-python3/
	cp -r ../../win32/deps/bin/FTD2XX_NET_v1.0.14/ deps/bin/FTD2XX_NET_v1.0.14/
	cp -r ../../win32/deps/etc/ deps/etc/
	cp -r ../../win32/deps/fonts/ deps/fonts/
	cp -r ../../win32/deps/FTD2XX_NET_v1.2.0/ deps/FTD2XX_NET_v1.2.0/
	cp -r ../../win32/deps/library/ deps/library/
	cp -r ../../win32/deps/modules/ deps/modules/

package:	
        cd $(APP_DIR)
	checkinstall --pkgname="Chronojump" --pkgversion="$(PACKAGE_VERSION)" --maintainer="Xavier de Blas" --pkglicense="GPL2" --requires="libgtk3.0-cil,r-base,ffmpeg,p7zip-full,python3-minimal" --install=no