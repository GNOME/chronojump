if not exist "%tmp%\Chronojump-logs" mkdir "%tmp%\Chronojump-logs"
cd "%tmp%\Chronojump-logs\"
if exist log_chronojump.txt (
	if exist log_chronojump_old.txt (
		del log_chronojump_old.txt
	)
	ren log_chronojump.txt log_chronojump_old.txt
)

if exist "%programfiles%\Chronojump\bin" cd "%programfiles%\Chronojump\bin"
if exist "%programfiles(x86)%\Chronojump\bin" cd "%programfiles(x86)%\Chronojump\bin"

Chronojump.exe > "%tmp%\Chronojump-logs\log_chronojump.txt" 2>&1
cd %userprofile%
