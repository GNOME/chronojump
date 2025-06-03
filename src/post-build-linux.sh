#!/bin/sh
set -x

cd $(dirname "$0")
rm -rf ../package/linux/deps/share/chronojump/encoder/
rm -rf ../package/linux/deps/share/chronojump/r-scripts/
rm -rf "$1/bin/"
rm -rf "$1/share/"
#mkdir -p "$1/bin/"
#cp -r $(find ../package/linux/deps/ -mindepth 1 -maxdepth 1 -type d) "$1/"
cp -r ../package/linux/deps/bin "$1"
cp -r ../package/linux/deps/share "$1"
cp -r "./angle/" "$1/bin/angle/"
cp -r "./chronojump-importer/" "$1/bin/chronojump-importer/"
mkdir -p "$1/share/chronojump/images/"
cp -r "../win32/chronojump_icon.ico" "$1/share/chronojump/images/chronojump_icon.ico"
cp -r "../encoder/" "$1/share/chronojump/encoder/"
cp -r "../r-scripts/" "$1/share/chronojump/r-scripts/"
rm -rf "$1/bin/chronojump-importer/Makefile.in"
rm -rf "$1/bin/chronojump-importer/Makefile.am"
rm -rf "$1/bin/chronojump-importer/Makefile"
rm -rf "$1/bin/encoder/Makefile.in"
rm -rf "$1/bin/encoder/Makefile.am"
rm -rf "$1/bin/encoder/Makefile"
rm -rf "$1/share/doc/chronojump/Makefile.in"
rm -rf "$1/share/doc/chronojump/Makefile.am"
rm -rf "$1/share/doc/chronojump/Makefile"
cp "../images/bad.wav" "$1/share/chronojump/images/bad.wav"
cp "../images/ok.wav" "$1/share/chronojump/images/ok.wav"
cp "../images/start.wav" "$1/share/chronojump/images/start.wav"
mkdir -p "$1/share/chronojump/images/sounds/beepTests"
cp ../images/sounds/beepTests/*.mp3 "$1/share/chronojump/images/sounds/beepTests/"
dos2unix ble-runner-linux.sh
chmod +x ble-runner-linux.sh

#Get OS Name
UNAME_ALL=$(echo $(uname -a) | tr [:upper:] [:lower:])
OS=""
echo "$UNAME_ALL" | grep "debian"
if [ "$?" = "0" ]; then  
    OS="debian"
else
    exit 0
fi

#Get Arch Name
ARCH="$(uname -m)"
if [ "$ARCH" = "x86_64" ]; then
    ARCH="x64"
elif [ "$ARCH" = "armv7l" ]; then
    ARCH="arm"
elif [ "$ARCH" = "aarch64" ]; then
    ARCH="arm64"
else
    ARCH=""
fi

#Get Major Version Number
. /etc/os-release || exit 0
VERSION="$(echo $VERSION_ID | sed 's/\..*//')"

#Copy if existing
if [ -e "../package/linux/refs/runtimes/$OS.$VERSION-$ARCH/native/SQLite.Interop.dll" ]; then
    cp "../package/linux/refs/System.Data.SQLite.dll" "$1/System.Data.SQLite.dll"
    cp "../package/linux/refs/System.Data.SQLite.dll.config" "$1/System.Data.SQLite.dll.config"
    cp "../package/linux/refs/runtimes/$OS.$VERSION-$ARCH/native/SQLite.Interop.dll" "$1/SQLite.Interop.dll"
    mkdir -p "$1/runtimes/linux-$ARCH/native"
    cp "../package/linux/refs/runtimes/$OS.$VERSION-$ARCH/native/SQLite.Interop.dll" "$1/runtimes/linux-$ARCH/native/SQLite.Interop.dll"
fi