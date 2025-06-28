cd %~dp0
%~d0
python -m venv ble
call ble\Scripts\activate
python -m pip show simplepyble
if %errorlevel% neq 0 (
    python -m pip install simplepyble
)
python simple-ble.py