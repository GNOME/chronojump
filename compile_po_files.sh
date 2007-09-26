#!/bin/bash

#Xavier de Blas, xaviblas@gmail.com
#compile_po_files
#drop this script when automake... is well used


PO_DIR="po"
LOCALE_DIR="build/data/locale"

created=0
updated=0
nothingDone=0

echo "Available PO files:"
for i in `ls $PO_DIR/*.po | grep -v _old`; do 
	export FILE_CUTTED=`echo $i | awk -F. '{print $1}' | awk -F/ '{print $2}'`
	if test -f $LOCALE_DIR/$FILE_CUTTED/LC_MESSAGES/chronojump.mo; then
		if test $i -nt $LOCALE_DIR/$FILE_CUTTED/LC_MESSAGES/chronojump.mo; then
			echo -e "$i \t .mo is old. updating..."
			msgfmt $i -o $LOCALE_DIR/$FILE_CUTTED/LC_MESSAGES/chronojump.mo
			echo -e "\t \t \t DONE\n"
			updated=$((${updated} + 1))
		else
			echo -e "$i \t .mo is up to date"
			nothingDone=$((${nothingDone} + 1))
		fi
	else
		echo -e "$i \t .mo DON'T exists \t creating..."
		msgfmt $i -o $LOCALE_DIR/$FILE_CUTTED/LC_MESSAGES/chronojump.mo
		echo -e "\t \t \t DONE\n"
		created=$((${created} + 1))
	fi
done

echo -e "\nMo files result: \nCreated: \t$created \nUpdated: \t$updated \nWhere ok: \t$nothingDone";
