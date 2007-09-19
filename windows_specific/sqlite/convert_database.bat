::don't know how to do the redirection from c#
::do it here

cd ..\data\utils\windows
del tmp.txt
del tmp.db

::read path from db_path.txt
set /p dbPath=<db_path.txt>nul

::dump sqlite data
sqlite %dbPath%\chronojump.db .dump > tmp.txt

::create sqlite3 data
sqlite3 tmp.db < tmp.txt
