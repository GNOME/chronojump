cd %~dp0
%~d0
cd ../butterworth
"C:\Windows\Microsoft.NET\Framework64\v4.0.30319\MSBuild.exe" SampleFiltering.sln /t:Rebuild /p:Configuration=Release
cd ../src
dotnet publish -p:PublishProfile=Properties\PublishProfiles\win-x64.pubxml
cd ../win32
"D:\Program Files (x86)\Inno Setup 6\ISCC.exe" chronojump_innosetup_x64.iss