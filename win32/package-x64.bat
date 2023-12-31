cd %~dp0
%~d0
REM cd ../butterworth
REM "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\MSBuild.exe" SampleFiltering.sln /t:Rebuild /p:Configuration=Release
cd ../src
dotnet publish -p:PublishProfile=Properties\PublishProfiles\win-x64.pubxml
if %errorlevel% neq 0 (
pause
exit
)
cd ../win32
"D:\Program Files (x86)\Inno Setup 6\ISCC.exe" chronojump_innosetup_x64.iss
pause