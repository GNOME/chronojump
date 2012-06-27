::Adapted from http://www.robvanderwoude.com/batexamples_r.html#R

@ECHO OFF
:: Check Windows version
IF NOT "%OS%"=="Windows_NT" GOTO Syntax
:: Check command line arguments:
:: 2 required plus 1 optional, "?" or "/?" for help screen
ECHO.%* | FIND "?" >NUL
IF NOT ERRORLEVEL 1 GOTO Syntax
IF     "%~2"=="" GOTO Syntax
IF NOT "%~4"=="" GOTO Syntax
IF NOT "%~3"=="" IF /I NOT "%~3"=="/K" GOTO Syntax
:: Check if either FINDSTR or EGREP is available
SET FINDSTR=
FINDSTR.EXE /? >NUL 2>&1
IF ERRORLEVEL 1 (
	EGREP.EXE --help >NUL 2>&1
	IF ERRORLEVEL 1 GOTO Syntax
	SET FINDSTR=EGREP
)

:: Keep variables local
SETLOCAL

:: Read variables from command line
SET RegSection=%~1
SET RegKey=%~2
SET RegVal=
IF /I "%~3"=="/K" (SET VarName=_%RegKey%) ELSE (SET VarName=RegVal)

:: Delete temporary file if it already exists
IF EXIST "%Temp%.\_readreg.dat" DEL "%Temp%.\_readreg.dat"

:: Store content of registry section in temporary file
START /WAIT REGEDIT.EXE /E "%Temp%.\_readreg.dat" "%~1"

:: Abort with error message if the section wasn't found in the registry
IF NOT EXIST "%Temp%.\_readreg.dat" (
	ECHO.
	ECHO ERROR:  [%RegSection%]  not found
) 1>&2
IF NOT EXIST "%Temp%.\_readreg.dat" (
	ENDLOCAL
	GOTO:EOF
)

:: Use either FINDSTR or EGREP to search requested value in temporary file.
:: Note: TYPE is used here because the temporary file may be in Unicode.
IF "%FINDSTR%"=="EGREP" (
	FOR /F "tokens=1* delims==" %%A IN ('TYPE "%Temp%.\_readreg.dat" 2^>NUL ^| EGREP.EXE -i "^^\"?%~2\"?="') DO (
		SET RegKey=%%~A
		SET RegVal=%%~B
	)
) ELSE (
	FOR /F "tokens=1* delims==" %%A IN ('TYPE "%Temp%.\_readreg.dat" 2^>NUL ^| FINDSTR.EXE /I /B /R /C:"\"%~2\"="') DO (
		SET RegKey=%%~A
		SET RegVal=%%~B
	)
)

:: Format and display the result
IF DEFINED RegVal (SET RegVal=%RegVal:\\=\%) ELSE (SET RegVal= - Undefined -)
::ECHO.
::ECHO [%RegSection%]
::ECHO %RegKey%=%RegVal%
ECHO %RegVal%

:: Delete temporary file
:: IF EXIST "%Temp%.\_readreg.dat" DEL "%Temp%.\_readreg.dat"

:: Pass result in variable to calling environment and end program
ENDLOCAL & SET %VarName%=%RegVal%
GOTO:EOF


:Syntax
ECHO.
ECHO ReadReg.bat,  Version 2.10 for Windows NT 4 and later
ECHO Read a value from the registry and store it in an environment variable
ECHO.
ECHO Usage:    READREG  "section"  "key"  [ /K ]
ECHO.
ECHO Where:             "section"  is the section name, without brackets
ECHO                    "key"      is the key whose value must be read
ECHO                    /K         uses "_key" for environment value name
ECHO.

:: In case we use NT 4 we have multiple notes, otherwise one single note
VER | FIND "Windows NT" >NUL
IF     ERRORLEVEL 1 ECHO Note:     The result is stored in an environment variable %%RegVal%%,
VER | FIND "Windows NT" >NUL
IF NOT ERRORLEVEL 1 ECHO Notes:    The result is stored in an environment variable %%RegVal%%,
ECHO           unless /K switch is used.

:: Message for NT 4 about FINDSTR and EGREP availability
VER | FIND "Windows NT" >NUL
IF ERRORLEVEL 1 GOTO Example
ECHO           This batch file uses either FINDSTR or EGREP to search for the
ECHO           the requested value.
ECHO           FINDSTR is part of the Windows NT 4 Resource Kit, available at
ECHO           http://www.microsoft.com/ntserver/nts/downloads/recommended/ntkit/
ECHO           EGREP is available at http://unxutils.sourceforge.net/ and
ECHO           several other sites.

:Example
ECHO.
ECHO Example:  READREG  "HKEY_CURRENT_USER\Environment"  "path"  /K
ECHO           should store the user part of the PATH variable in _PATH variable
ECHO.
ECHO Written by Rob van der Woude
ECHO http://www.robvanderwoude.com