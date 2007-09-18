#!/bin/sh

#Xavier de Blas, www.xdeblas.com
#this scripts allows to run chronojump from gui. It exports the LD_LIBRARY_PATH from ~/.bashrc if needed

echo "---------------"
echo "pre: chmod +x convert_database.sh (as precaution)"
chmod +x ../data/utils/linux/convert_database.sh

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
mono chronojump.prg 
