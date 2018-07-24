@echo off
set "PROJPATH=%cd%"
if %1==no (
    GOTO ProcessNoError
)

set "outputpath=%PROJPATH%\AssetBundles\%2"
cd /D C:\Program Files\7-Zip
7z a %outputpath%.zip %outputpath%
if %ERRORLEVEL% NEQ 0 GOTO ProcessError

rem need to change to the staging ip
xcopy %outputpath%.zip \\192.168.11.77\zdspfileserver\zdsppatch\%2
if %ERRORLEVEL% NEQ 0 GOTO ProcessError

:ProcessNoError
cd /D %PROJPATH%
@rem no error here
exit /b 0

:ProcessError
echo Error in zipping
cd /D %PROJPATH%
@rem process error
exit /b 1