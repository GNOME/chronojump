#!/bin/sh

#Xavier de Blas, www.xdeblas.com

args=$1
echo $args

if [ "$1" = "" ]; then
	echo "Usage: $0 x.yy"
	exit
fi

release_dir="releases/chronojump-$1"

if test -e $release_dir; then 
	echo "release exists"
	exit
fi

#compile po files (update translations)
./compile_po_files.sh

#create the executable 
make

#create dirs
mkdir $release_dir
mkdir $release_dir/windows
mkdir $release_dir/linux
mkdir $release_dir/manual
mkdir $release_dir/data
mkdir $release_dir/data/windows_dlls
mkdir $release_dir/data/linux_dlls
mkdir $release_dir/data/utils
mkdir $release_dir/data/utils/windows
mkdir $release_dir/data/utils/linux

#copy files
cp windows_specific/chronojump.bat $release_dir/windows
cp windows_specific/chronojump_mini.bat $release_dir/windows
cp linux_specific/chronojump.sh $release_dir/linux
cp linux_specific/chronojump_mini.sh $release_dir/linux

cp manual/chronojump_manual_es.pdf $release_dir/manual

cp chronojump.prg $release_dir/data
cp chronojump_mini.prg $release_dir/data

cp -R locale $release_dir/data

cp windows_specific/sqlite3.dll $release_dir/data
cp windows_specific/readreg.bat $release_dir/data

#chronojump.bat & chronojump_mini.bat will copy this dlls to data dir
cp windows_specific/NPlot.dll $release_dir/data/windows_dlls
cp windows_specific/NPlot.dll.config $release_dir/data/windows_dlls
cp windows_specific/NPlot.Gtk.dll $release_dir/data/windows_dlls
cp windows_specific/NPlot.Gtk.dll.config $release_dir/data/windows_dlls
cp windows_specific/sqlite3.dll $release_dir/data/windows_dlls

#chronojump.sh & chronojump_mini.sh will copy this dlls to data dir
cp linux_specific/NPlot.dll $release_dir/data/linux_dlls
cp linux_specific/NPlot.dll.config $release_dir/data/linux_dlls
cp linux_specific/NPlot.Gtk.dll $release_dir/data/linux_dlls
cp linux_specific/NPlot.Gtk.dll.config $release_dir/data/linux_dlls

#copy sqlite convert stuff
cp windows_specific/sqlite/sqlite.exe $release_dir/data/utils/windows
cp windows_specific/sqlite/sqlite3.exe $release_dir/data/utils/windows
cp windows_specific/sqlite/convert_database.bat $release_dir/data/utils/windows
cp linux_specific/sqlite/sqlite3-3.5.0.bin $release_dir/data/utils/linux
cp linux_specific/sqlite/sqlite-2.8.17.bin $release_dir/data/utils/linux
cp linux_specific/sqlite/convert_database.sh $release_dir/data/utils/linux

