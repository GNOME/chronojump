#!/bin/sh

#Xavier de Blas, www.xdeblas.com
#this scripts allows to run chronojump from gui. It exports the LD_LIBRARY_PATH from ~/.bashrc if needed

#cd to this sh dir
CHRONOJUMP_SH_HOME=`dirname $0`
cd $CHRONOJUMP_SH_HOME
echo $CHRONOJUMP_SH_HOME

echo "---------------"
echo "pre1: chmod +x convert_database scripts (as precaution)"
chmod +x ../data/utils/linux/convert_database.sh
chmod +x ../data/utils/linux/sqlite3-3.5.0.bin
chmod +x ../data/utils/linux/sqlite-2.8.17.bin

echo "---------------"
echo "pre2: copy nplot dlls to data dir"
cp ../data/linux_dlls/NPlot.dll ../data
cp ../data/linux_dlls/NPlot.dll.config ../data
cp ../data/linux_dlls/NPlot.Gtk.dll ../data
cp ../data/linux_dlls/NPlot.Gtk.dll.config ../data

echo "---------------"
echo "1st: Show Current config variables, compare them with ~/.bashrc"

echo "PATH: " $PATH
echo "MANPATH: " $MANPATH
echo "PKG_CONFIG_PATH: " $PKG_CONFIG_PATH
echo "LD_LIBRARY_PATH: " $LD_LIBRARY_PATH

echo "---------------"
echo "2nd: export LD_LIBRARY_PATH"

#if not defined LD_LIBRARY_PATH
if [ -n "$LD_LIBRARY_PATH" ]; then
	echo "LD_LIBRARY_PATH already defined"
else
	echo "LD_LIBRARY_PATH undefined. Doing it now..."
	library_path_cutted=`cat ~/.bashrc | grep LD_LIBRARY_PATH | cut -d '=' -f2 | cut -d ':' -f1 |cut -d '"' -f2`
	#other version:
	#library_path_cutted=`cat ~/.bashrc | grep LD_LIBRARY_PATH | grep mono | cut -d '=' -f2 | cut -d '"' -f2`

	export LD_LIBRARY_PATH=$library_path_cutted
	echo $LD_LIBRARY_PATH
fi

#read a #useful for terminal not being closed

#call Chronojump
echo "---------------"
echo "3d: call Chronojump"
cd ../data
mono chronojump_mini.prg $@

