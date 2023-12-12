cd %~dp0
%~d0
xcopy /s /e /y /d "..\butterworth\Data\" "%1\butterworth\Data\"          
xcopy /s /e /y /d "..\encoder\" "%1\encoder\"       
xcopy /s /e /y /d "..\glade\" "%1\glade\" 
xcopy /s /e /y /d "..\images\" "%1\images\"    
xcopy /s /e /y /d "..\po\" "%1\po\" /exclude:exclude.txt
xcopy /s /e /y /d "..\win32\chronojump_icon.ico" "%1\share\chronojump\images\chronojump_icon.ico"
xcopy /s /e /y /d "..\win32\logchronojump.bat" "%1\logchronojump.bat"
xcopy /s /e /y /d "..\win32\deps\" "%1" /exclude:exclude.txt
xcopy /s /e /y /d "..\win32\xbuild_files\" "%1\xbuild_files\" 
xcopy /s /e /y /d "..\manual\" "%1\share\doc\chronojump\"
xcopy /s /e /y /d "..\win32\gtk3\" "%1\" 
xcopy /s /e /y /d "..\build\data\locale\" "%1\share\locale\" 