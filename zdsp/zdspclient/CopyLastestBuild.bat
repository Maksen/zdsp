@echo off
set "PROJPATH=%cd%"

cd build\%1

FOR /F "delims=" %%i IN ('dir /b /ad-h /t:c /od') DO SET recent=%%i
echo Most recent subfolder: %recent%

xcopy /s /e /i "%recent%" "\\192.168.11.60\zdspshare\build\%1\%recent%"

IF %ERRORLEVEL% NEQ 0 GOTO ProcessError

cd %PROJPATH%
@rem no error here
exit /b 0

:ProcessError
cd %PROJPATH%
@rem process error
exit /b 1






