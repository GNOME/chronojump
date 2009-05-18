#!/bin/sh

#Xavier de Blas, www.xdeblas.com

args=$1
echo $args

#usage info
if [ "$1" = "" ]; then
	echo "Usage:\n $0 x.yy\n $0 x.yy.zz.ww\n NOT things like 'svn-...'"
	echo "Recommended!!!: $0 `cat version.txt`"
	exit
fi

#create release root dir if needed
if ! test -e "releases_linux"; then
	mkdir releases_linux
fi

#create specific release dir

release_dir="releases_linux/chronojump-$1"

if test -e releases_linux/chronojump-$1.tar.gz; then 
	echo "release tar.gz exists. EXITING! NOTHING COPIED TO releases!!!"
	exit
fi

if test -e $release_dir; then 
	echo "release dir exists. EXITING! NOTHING COPIED TO releases!!!"
	exit
fi

mkdir $release_dir

cp Makefile $release_dir/.
cp version.txt $release_dir/.
cp compile_po_files.sh $release_dir/.

#copy files ('L' for copying de symbolic link of chronopic-tests)
mkdir $release_dir/src
cp -rL src/* $release_dir/src/.

#glade
mkdir $release_dir/glade
cp glade/* $release_dir/glade/.

#images
mkdir $release_dir/images
cp images/* $release_dir/images/.
mkdir $release_dir/images/mini
cp images/mini/* $release_dir/images/mini/.

#sever cs
mkdir $release_dir/chronojump_server
cp chronojump_server/ChronojumpServer.cs $release_dir/chronojump_server/.

#nplot libs
mkdir -p $release_dir/build/data/linux_dlls
cp build/data/linux_dlls/* $release_dir/build/data/linux_dlls/.
cp build/data/linux_dlls/* $release_dir/build/data/.

#locales
mkdir $release_dir/build/data/locale
mkdir $release_dir/po
cp po/* $release_dir/po/.

#copy docs, license & other text stuff (these are not on "build" dir)
mkdir $release_dir/docs
cp manual/chronojump_manual_es.pdf $release_dir/docs/.
cp glossary/chronojump_glossary_for_translators.html $release_dir/docs/.
cp AUTHORS $release_dir/docs/.
cp COPYING $release_dir/docs/.
cp INSTALL $release_dir/docs/.
cp changelog.txt $release_dir/docs/.


#create compressed files

release_subdir="chronojump-$1"
cd releases_linux

#tar.gz
tar --exclude=.svn -czvf $release_subdir.tar.gz $release_subdir
rm -rf $release_subdir

#b) zip
echo "Release done!\n- $release_subdir.tar.gz has to be uploaded to gnome FTP\n-, please 'git add' from that dir"
cd ..
