SET srv=zdspclient\zdspServer

xcopy photonserver\deploy\bin_tools %srv%\bin_tools /S /Y
xcopy photonserver\deploy\bin_Win64 %srv%\bin_Win64 /S /Y
xcopy photonserver\deploy\CounterPublisher %srv%\CounterPublisher /S /Y
xcopy photonserver\deploy\Loadbalancing %srv%\Loadbalancing /S /Y
xcopy photonserver\deploy\Plugins %srv%\Plugins /S /Y
xcopy photonserver\deploy\Policy %srv%\Policy /S /Y

rem call syncSchema.bat

cd zdspclient
call syncGameRepo.bat
call syncLevel.bat
call syncnavdata.bat
REM call copyServerConfig.bat

cd ..