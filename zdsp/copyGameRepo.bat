xcopy \\192.168.11.79\ZDSPRepo\*.json zdspclient\Assets\GameData\GameRepo /S /Y
xcopy zdspclient\Assets\GameData\GameRepo\*.json photonserver\src-server\Loadbalancing\LoadBalancing\Repository /S /Y
xcopy zdspclient\Assets\GameData\GameRepo\*.json "backend//GMTools (WebApp)//GMTools (WebApp)//bin//GMToolPages//GameData" /S /Y
pause