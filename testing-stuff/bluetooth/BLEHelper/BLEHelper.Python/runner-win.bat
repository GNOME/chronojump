cd %~dp0
%~d0
REM python -m venv ble
REM call ble\Scripts\activate
python -m pip show bleak
if %errorlevel% neq 0 (
    python -m pip install bleak
)
python ble.py