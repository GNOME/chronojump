#!/bin/sh
#don't know how to do the redirection from c#
#do it here

cd ../data/utils/linux
rm tmp.txt
rm tmp.db

#read path from db_path.txt
dbPath=`cat db_path.txt`

#dump sqlite data
./sqlite-2.8.17.bin $dbPath/chronojump.db .dump > tmp.txt

#create sqlite3 data
cat tmp.txt | ./sqlite3-3.5.0.bin tmp.db

