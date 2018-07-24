@echo off
set "PROJPATH=%cd%"

SET srv=zdspclient\zdspServer

xcopy photonserver\deploy\bin_tools %srv%\bin_tools /S /Y
xcopy photonserver\deploy\bin_Win64 %srv%\bin_Win64 /S /Y
xcopy photonserver\deploy\CounterPublisher %srv%\CounterPublisher /S /Y
xcopy photonserver\deploy\Lite %srv%\Lite /S /Y
xcopy photonserver\deploy\LiteLobby %srv%\LiteLobby /S /Y
xcopy photonserver\deploy\Loadbalancing %srv%\Loadbalancing /S /Y
xcopy photonserver\deploy\Policy %srv%\Policy /S /Y

call changeserverversion %1 %PROJPATH%\zdspclient\zdspServer\Loadbalancing\Master\bin\Photon.LoadBalancing.dll.config
xcopy %PROJPATH%\Photon.LoadBalancing.dll.config %PROJPATH%\zdspclient\zdspServer\Loadbalancing\Master\bin /Y
del /q %PROJPATH%\Photon.LoadBalancing.dll.config
IF %ERRORLEVEL% NEQ 0 GOTO ProcessError

if %2 == yes (
    call copyGameRepo.bat
)
rem call syncSchema.bat

cd piliclient
call syncGameRepo.bat
call syncLevel.bat
call syncnavdata.bat
REM call copyServerConfig.bat

cd ..

For /f "tokens=2-4 delims=/ " %%a in ('date /t') do (set mydate=%%c-%%a-%%b)
For /f "tokens=1-2 delims=/:" %%a in ("%TIME%") do (set mytime=%%a%%b)

xcopy piliclient\PiliServer dailyserverbuild\%mydate%_%mytime%_%1_%3 /S /Y /I

@rem no error in building
echo no error
cd %PROJPATH%
exit /b 0

:ProcessError
@rem process error
echo got error
cd %PROJPATH%
exit /b 1