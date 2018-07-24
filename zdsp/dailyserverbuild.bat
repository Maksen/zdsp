SET srv=piliclient\PiliServer
SET dest=d:\dailyserverbuild\%1
rmdir %dest% 
mkdir %dest%
echo targetbuild %dest%

xcopy %srv%\bin_Win64 %dest%\bin_Win64 /S /Y /I
xcopy %srv%\CounterPublisher %dest%\CounterPublisher /S /Y /I
xcopy %srv%\Lite %dest%\Lite /S /Y /I
xcopy %srv%\LiteLobby %dest%\LiteLobby /S /Y /I
xcopy %srv%\Loadbalancing %dest%\Loadbalancing /S /Y /I
xcopy %srv%\Policy %dest%\Policy /S /Y /I
copy %srv%\version.txt %dest%\version.txt /Y

copy %srv%\Loadbalancing\GameServer1\Repository\gamedata.json backend\GMTools\GMTools\Content\Repository\gamedata.json /Y

REM copy d:\dailyserverbuild\config\master\Photon.LoadBalancing.dll.config %dest%\Loadbalancing\Master\bin\Photon.LoadBalancing.dll.config /Y
REM copy d:\dailyserverbuild\config\gameserver\Photon.LoadBalancing.dll.config %dest%\Loadbalancing\GameServer1\bin\Photon.LoadBalancing.dll.config /Y
REM copy d:\dailyserverbuild\config\gameserver\Photon.LoadBalancing.dll.config %dest%\Loadbalancing\GameServer2\bin\Photon.LoadBalancing.dll.config /Y