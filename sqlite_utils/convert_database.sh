#!/bin/sh
#don't know how to do the redirection from c#
#do it here

cd ../data/utils/linux
rm tmp.db
cat tmp.txt | ./sqlite3-3.5.0.bin tmp.db
