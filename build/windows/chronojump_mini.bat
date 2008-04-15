::Xavier de Blas, xaviblas@gmail.com

@echo off

::copy nplot dlls
copy ..\data\windows_dlls\NPlot.dll ..\data
copy ..\data\windows_dlls\NPlot.dll.config ..\data
copy ..\data\windows_dlls\NPlot.Gtk.dll ..\data
copy ..\data\windows_dlls\NPlot.Gtk.dll.config ..\data

::find version

call ..\data\readreg.bat "HKEY_LOCAL_MACHINE\Software\Novell\Mono" "DefaultCLR" > temp.txt
set /p version=<temp.txt>nul
del temp.txt

::find SdkInstallRoot

call ..\data\readreg.bat "HKEY_LOCAL_MACHINE\Software\Novell\Mono\%version%" "SdkInstallRoot" > temp.txt
set /p monoPath=<temp.txt>nul
del temp.txt

::call program or print "install mono" if needed

set monoFullPath=%monoPath%\bin\mono.exe
echo Path of Mono: "%monoFullPath%"

if exist "%monoFullPath%" goto ExecuteChronojump

echo "Mono not installed. Please, reinstall Chronojump."
pause
exit

:ExecuteChronojump
cd ..\data
"%monoFullPath%" "chronojump_mini.prg"
