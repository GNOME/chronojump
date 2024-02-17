#!/bin/sh
cd $(dirname "$0")
cp -r "./angle/" "$1bin/angle/"
cp -r "./chronojump-importer/" "$1bin/chronojump-importer/"
cp -r "../encoder/" "$1bin/encoder/"
cp -r "../po/" "$1po/"
cp -r "../win32/chronojump_icon.ico" "$1share/chronojump/images/chronojump_icon.ico"
cp -r "../package/macos/deps/" "$1"
cp -r "../manual/" "$1share/doc/chronojump/"
rm -rf "$1*.in"
rm -rf "$1.gitignore"
rm -rf "$1*.am"