cd %~dp0
%~d0
xcopy /s /e /y /d "..\butterworth\Data\" "%1butterworth\Data\"          
xcopy /s /e /y /d "..\butterworth\Sample\bin\%2\" "%1butterworth\"
xcopy /s /e /y /d "..\encoder\" "%1encoder\"       
xcopy /s /e /y /d "..\glade\" "%1glade\" 
xcopy /s /e /y /d "..\images\" "%1images\"    
xcopy /s /e /y /d "..\po\" "%1po\" /exclude:post-build-exclude.txt
xcopy /s /e /y /d "..\win32\chronojump_icon.ico" "%1share\chronojump\images\chronojump_icon.ico"
xcopy /s /e /y /d "..\win32\logchronojump.bat" "%1logchronojump.bat"
xcopy /s /e /y /d "..\win32\deps\" "%1" /exclude:post-build-exclude.txt
xcopy /s /e /y /d "..\win32\xbuild_files\" "%1xbuild_files\" 
xcopy /s /e /y /d "..\manual\" "%1share\doc\chronojump\"
xcopy /s /e /y /d "..\win32\gtk3\" "%1" 
xcopy /s /e /y /d "..\build\data\locale\" "%1share\locale\" 