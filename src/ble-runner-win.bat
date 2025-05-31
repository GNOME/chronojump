cd %~dp0
%~d0
REM %1 -m venv ble
REM call ble\Scripts\activate
%1 -m pip show bleak
if %errorlevel% neq 0 (
    %1 -m pip install bleak
)
%1 ble.py