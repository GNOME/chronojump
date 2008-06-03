::Xavier de Blas, xaviblas@gmail.com

@echo off

::call program or print "install mono" if needed

set /p monoPath=<"..\data\mono_path.txt"
set monoFullPath=%monoPath%\bin\mono.exe
echo Path of Mono: "%monoFullPath%"

if exist "%monoFullPath%" goto ExecuteChronojump

echo "Mono not installed. Please, reinstall Chronojump."
pause
exit

:ExecuteChronojump
cd ..\data
"%monoFullPath%" "chronojump_mini.prg"
