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
cp NPlot.dll releases/$1/data
cp NPlot.dll.config releases/$1/data
cp NPlot.Gtk.dll releases/$1/data
cp NPlot.Gtk.dll.config releases/$1/data
cp readreg.bat releases/$1/data
cp -R locale releases/$1/data
cp sqlite3.dll releases/$1/data
#probably remain copy the sqlite sqlite.dll (2.1) for db conversion

