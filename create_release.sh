#!/bin/sh

#Xavier de Blas, www.xdeblas.com

#provant de fer un bundle de chronojump amb mono static:
#xavier@corall:~/informatica/progs_meus/chronojump/chronojump$ mkbundle --static --deps -o chronojump_bundled.exe chronojump.exe
#(el exe del final és el chronojump compilat com a exe, no prg)

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

#create the win executable (only for bundle it)
mcs chronojump_execute_windows.cs
#create the bundled executable
mkbundle -o chronojump.exe --deps chronojump_execute_windows.exe

#copy files
#cp chronojump.exe releases/$1/windows
#cp chronojump_mini.exe releases/$1/windows
cp installChronojumpWindows.bat releases/$1/windows
cp regread.bat releases/$1/windows #probably this file will be on data


cp chronojump.sh releases/$1/linux
cp chronojump_mini.sh releases/$1/linux

cp manual/chronojump_manual_es.pdf releases/$1/manual

cp chronojump.prg releases/$1/data
#remain copy the sqlite dlls for win (sqlite3.dll and sqlite.dll)
cp NPlot.dll releases/$1/data
cp NPlot.dll.config releases/$1/data
cp NPlot.Gtk.dll releases/$1/data
cp NPlot.Gtk.dll.config releases/$1/data
cp -R locale releases/$1/data

