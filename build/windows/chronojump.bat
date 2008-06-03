::Xavier de Blas, xaviblas@gmail.com

@echo off

::call program or print "install mono" if needed

set /p monoPath=<"..\data\mono_path.txt"
set monoFullPath=%monoPath%\bin\mono.exe
echo Path of Mono: "%monoFullPath%"

if exist "%monoFullPath%" goto ExecuteChronojump

echo "Mono not installed. Download from mono-project.com"
pause
exit


:ExecuteChronojump


::prepare LOG_FILE

set LOG_DIR=..\..\logs
if exist %LOG_DIR% goto logDirExists
mkdir ..\..\logs
:logDirExists

::set time like: 16-34-60,21
for /f "tokens=1,2,3 delims=:. " %%x in ("%time%") do set t=%%x-%%y-%%z
::without milleconds. like: 16-34-60
for /f "tokens=1,2 delims=:, " %%x in ("%t%") do set t_no_ms=%%x

::set date_time english
::set LOG_DATE=%date:~-4,4%-%date:~-7,2%-%date:~-0,2%_%t%

::set date_time spanish
set LOG_DATE=%date:~-0,2%-%date:~-7,2%-%date:~-4,4%_%t_no_ms%
set LOG_FILE=%LOG_DIR%\%LOG_DATE%

echo %LOG_FILE%
::pause


cd ..\data

::call program redirecting to a file the standard output and the error output at end
::on widows there are problems for redirect both things, then put the "-crash" in the name of the error
::on linux there's only one file
::this "-crash" it's checked on src/log.cs and src/chronojump.cs

::"%monoFullPath%" "chronojump.prg" %LOG_DATE% 2>> %LOG_FILE%-crash.txt
::without error redirection for Vista
"%monoFullPath%" "chronojump.prg" %LOG_DATE%
