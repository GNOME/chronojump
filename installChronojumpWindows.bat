::Xavier de Blas, xaviblas@gmail.com
::no longer used, because if mono version changes, chronojump.bat and chronojump_mini.bat will not find mono
::now included in chronojump.bat and chronojump_mini.bat

@echo off

::find version

call regread.bat "HKEY_LOCAL_MACHINE\Software\Novell\Mono" "DefaultCLR" > temp.txt

set /p version=<temp.txt>nul

del temp.txt

::find SdkInstallRoot

call regread.bat "HKEY_LOCAL_MACHINE\Software\Novell\Mono\%version%" "SdkInstallRoot" > temp.txt

set /p monoPath=<temp.txt>nul

del temp.txt


::Write chronojump.bat

echo "%monoPath%\bin\mono.exe" "chronojump.prg" > chronojump_execute.bat


::Write chronojump_mini.bat

echo "%monoPath%\bin\mono.exe" "chronojump_mini.prg" > chronojump_mini_execute.bat
