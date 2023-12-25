cd %~dp0
%~d0
xcopy /s /e /y /d "..\butterworth\Data\" "%1butterworth\Data\"          
xcopy /s /e /y /d "..\butterworth\Sample\bin\%2\" "%1butterworth\"
xcopy /s /e /y /d "..\angle\" "%1bin\angle\"   
xcopy /s /e /y /d "..\chronojump-importer\" "%1bin\chronojump-importer\"    
xcopy /s /e /y /d "..\encoder\" "%1bin\encoder\"    
xcopy /s /e /y /d "..\po\" "%1po\" /exclude:post-build-exclude.txt
echo F | xcopy /s /e /y /d "..\win32\chronojump_icon.ico" "%1share\chronojump\images\chronojump_icon.ico"
xcopy /s /e /y /d "..\win32\deps\" "%1" /exclude:post-build-exclude.txt
xcopy /s /e /y /d "..\win32\xbuild_files\" "%1xbuild_files\" 
xcopy /s /e /y /d "..\manual\" "%1share\doc\chronojump\" /exclude:post-build-exclude.txt
echo F | xcopy /s /e /y /d "..\win32\gtk3-x64\libglib-2.0-0.dll" "%1libglib-2.0-0.dll" 
echo F | xcopy /s /e /y /d "..\win32\gtk3-x64\libintl-8.dll" "%1libintl-8.dll" 