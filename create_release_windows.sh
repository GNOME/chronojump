#!/bin/sh

#Xavier de Blas, www.xdeblas.com

args=$1
echo $args

#usage info
if [ "$1" = "" ]; then
	echo "Usage:\n $0 x.yy\n $0 x.yy.zz.ww\n NOT things like 'svn-...'"
	#echo "Recommended!!!: $0 `cat version.txt`"
	exit
fi

#create release root dir if needed
if ! test -e "releases_windows"; then
	mkdir releases_windows
fi

#create the executable
make

#create the blank db
cd build/data
mono Chronojump.exe createBlankDB
echo "Blank DB created!"

cd ../..

#create specific release dir

release_dir="releases_windows/chronojump-$1"

if test -e $release_dir; then 
	echo "release exists. EXITING! NOTHING COPIED TO releases!!!"
	exit
fi

mkdir $release_dir

#copy files ('L' for copying de symbolic link of chronopic-tests)
cp -rL build/windows_bundle/* $release_dir/.

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
cd releases_windows

#zip
windows_zip_dir="windows_zip_releases"
zip -r $release_subdir.zip $release_subdir -x \*.svn\* -x \*.tar.gz
mv $release_subdir.zip $release_subdir
cp $release_subdir/$release_subdir.zip ../$windows_zip_dir

#echo "Release WINDOWS done!\n- $release_subdir.zip has been copied to $windows_zip_dir , please 'git add' from that dir" > $release_subdir/readme.txt
#cd ..
#echo "\nPlease read $releases/$release_subdir/readme.txt"
echo "done"
