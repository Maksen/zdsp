del /q photonserver\src-server\Loadbalancing\LoadBalancing\levels\*
xcopy zdspclient\Assets\GameData\levels\*.json photonserver\src-server\Loadbalancing\LoadBalancing\levels /S /Y
del /q photonserver\src-server\Loadbalancing\LoadBalancing\navdata\*
xcopy zdspclient\Assets\GameData\navdata\*.bytes photonserver\src-server\Loadbalancing\LoadBalancing\navdata /S /Y
pause