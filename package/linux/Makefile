MKFILE_PATH := $(abspath $(lastword $(MAKEFILE_LIST)))
CUR_DIR := $(dir $(MKFILE_PATH))
BASE_DIR = "$(CUR_DIR)../.."
SOURCES_DIR = "$(CUR_DIR)../../src"
PACKAGE_DIR = "$(CUR_DIR)../../package/linux"
USER = $(shell whoami)
PACKAGE_VERSION = $(shell git describe --abbrev=1)
PACKAGE_NAME = "chronojump-$(PACKAGE_VERSION)"
APP_DIR = "$(PACKAGE_DIR)/$(PACKAGE_NAME)"

TARGET = $(TARGET)
ifeq ($(TARGET),arm)
    ARCH = "armhf,armel,armv7l"
else ifeq ($(TARGET),arm64)
    ARCH = "arm64,aarch64"
else
    ARCH = "amd64,riscv64,ppc64el,x86_64,s390x,mips64el"
endif 

tar: install totar
deb: install todeb

install:
	mkdir -p deps/bin/
	cp -r ../../win32/deps/bin/chronojump-importer-python3/ deps/bin/chronojump-importer-python3/
	cp -r ../../win32/deps/bin/FTD2XX_NET_v1.0.14/ deps/bin/FTD2XX_NET_v1.0.14/
	cp -r ../../win32/deps/etc/ deps/etc/
	cp -r ../../win32/deps/fonts/ deps/fonts/
	cp -r ../../win32/deps/FTD2XX_NET_v1.2.0/ deps/FTD2XX_NET_v1.2.0/
	cp -r ../../win32/deps/library/ deps/library/
	cp -r ../../win32/deps/modules/ deps/modules/
	cp ../../images/svg/chronojump_icon.svg deps/chronojump_icon.svg

totar:
	dos2unix ../../src/post-build-linux.sh
	chmod +x ../../src/post-build-linux.sh
	dotnet publish ../../src/Chronojump-linux.sln -p:BuildTranslations=true --configuration Release -r linux-$(TARGET) --self-contained true -o $(APP_DIR)
	sh ../../src/post-build-linux.sh $(APP_DIR)
	tar -czvf $(PACKAGE_NAME)_$(TARGET).tar.gz $(PACKAGE_NAME)/*

todeb:	
	#checkinstall --pkgname="Chronojump" --pkgversion="$(PACKAGE_VERSION)" --maintainer="Xavier de Blas" --pkglicense="GPL2" --requires="libgtk3.0-cil,r-base,ffmpeg,p7zip-full,python3-minimal" --install=no
	#cat > deb/DEBIAN/control <<EOF
	#Package: Chronojump
	#Version: $(PACKAGE_VERSION)
	#License: GPL2
	#Architecture: amd64
	#Maintainer: Xavier de Blas
	#Depends: libgtk3.0-cil,r-base,ffmpeg,p7zip-full,python3-minimal
	#Section: default
	#Priority: optional
	#Description: Chronojump is a free software dedicated to the management of several human movement measurement devices. Chronojump is compatible with Windows, Mac and Linux systems. It is regularly updated and the updates are and will always be free. The most common devices that Chronojump connects to are encoders, force gauges, photocells and contact platforms. Among the variables of interest, power, speed, force and displacement, although these depend on the device and the test executed. For more information, see: https://chronojump.org/chronojump-sensors/Chronojump started as a doctoral thesis related to vertical jumping in 2004, and nowadays it is widely used in the following fields: sport training, physiotherapy, medicine, education and research. Chronojump: Field evaluation with laboratory quality measurement / No more blind training.
	#EOF
	rm -rf deb
	mkdir -p deb/DEBIAN
	#cp control deb/DEBIAN/control
	sed -e s/{ARCH}/$(ARCH)/ control > deb/DEBIAN/control
	mkdir -p deb/usr/local/bin
	mkdir -p deb/usr/share/Chronojump
	ln -s /usr/share/Chronojump/Chronojump deb/usr/local/bin/Chronojump	
	mkdir -p deb/usr/share/applications
	cp Chronojump.desktop deb/usr/share/applications/Chronojump.desktop
	dos2unix ../../src/post-build-linux.sh
	chmod +x ../../src/post-build-linux.sh
	dotnet publish ../../src/Chronojump-linux.sln -p:BuildTranslations=true --configuration Release -r linux-$(TARGET) --self-contained true -o deb/usr/share/Chronojump
	sh ../../src/post-build-linux.sh $(PACKAGE_DIR)/deb/usr/share/Chronojump
	dpkg -b deb $(PACKAGE_NAME)_$(TARGET).deb