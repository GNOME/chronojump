#!/bin/sh
cd $(dirname "$0")
cp -r "../../package/macos/deps/System.Data.SQLite.dll" "$1/System.Data.SQLite.dll"
cp -r "../../package/macos/deps/System.Data.SQLite.dll.config" "$1/System.Data.SQLite.dll.config"
cp -r "../../package/macos/deps/runtimes" "$1/"