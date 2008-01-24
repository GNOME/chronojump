#!/bin/sh

#Xavier de Blas, www.xdeblas.com

args=$1
echo $args

#usage info
if [ "$1" = "" ]; then
	echo "Usage: $0 x.yy"
	exit
fi

#create release root dir if needed
if ! test -e "releases"; then
	mkdir releases
fi

#create the executable on "build" dir
make

#create specific release dir

release_dir="releases/chronojump-$1"

if test -e $release_dir; then 
	echo "release exists"
	exit
fi

mkdir $release_dir

#copy files ('L' for copying de symbolic link of chronopic-tests)
cp -rL build/* $release_dir/.

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
cd releases

#a) tar.gz
tar --exclude=.svn -czvf $release_subdir.tar.gz $release_subdir
mv $release_subdir.tar.gz $release_subdir

#b) zip
windows_zip_dir="windows_zip_releases"
zip -r $release_subdir.zip $release_subdir -x \*.svn\* -x \*.tar.gz
mv $release_subdir.zip $release_subdir
cp $release_subdir/$release_subdir.zip ../$windows_zip_dir

echo "Release done!\n- data, docs, linux, windows are for local testing\n- $release_subdir.tar.gz has to be uploaded to gnome FTP\n- $release_subdir.zip has been copied to $windows_zip_dir , please 'svn add' from that dir" > $release_subdir/readme.txt
cd ..
echo "\nPlease read $releases/$release_subdir/readme.txt"
