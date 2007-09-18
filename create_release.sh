#!/bin/sh

#Xavier de Blas, www.xdeblas.com

args=$1
echo $args

if [ "$1" = "" ]; then
	echo "Usage: $0 x.yy"
	exit
fi

if test -e "releases/$1"; then 
	echo "release exists"
	exit
fi

#create dirs
mkdir releases/$1
mkdir releases/$1/windows
mkdir releases/$1/linux
mkdir releases/$1/manual
mkdir releases/$1/data
mkdir releases/$1/data/utils
mkdir releases/$1/data/utils/windows
mkdir releases/$1/data/utils/linux


#create the executable 
make

#copy files
cp chronojump.bat releases/$1/windows
cp chronojump_mini.bat releases/$1/windows

cp chronojump.sh releases/$1/linux
cp chronojump_mini.sh releases/$1/linux

cp manual/chronojump_manual_es.pdf releases/$1/manual

cp chronojump.prg releases/$1/data
cp chronojump_mini.prg releases/$1/data
cp NPlot.dll releases/$1/data #TODO: there should be different versions of 4 nplots for win and linux
cp NPlot.dll.config releases/$1/data
cp NPlot.Gtk.dll releases/$1/data
cp NPlot.Gtk.dll.config releases/$1/data
cp readreg.bat releases/$1/data
cp -R locale releases/$1/data
cp sqlite3.dll releases/$1/data

cp sqlite_utils/sqlite.exe releases/$1/data/utils/windows
cp sqlite_utils/sqlite3.exe releases/$1/data/utils/windows
cp sqlite_utils/convert_database.bat releases/$1/data/utils/windows
cp sqlite_utils/sqlite3-3.5.0.bin releases/$1/data/utils/linux
cp sqlite_utils/sqlite-2.8.17.bin releases/$1/data/utils/linux
cp sqlite_utils/convert_database.sh releases/$1/data/utils/linux

