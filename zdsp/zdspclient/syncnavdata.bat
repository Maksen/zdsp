del /q zdspServer\Loadbalancing\GameServer\navdata\*
del /q zdspServer\Loadbalancing\Master\navdata\*
del /q zdspServer\Loadbalancing\CounterPublisher\navdata\*

xcopy ..\photonserver\src-server\Loadbalancing\LoadBalancing\navdata\*.bytes zdspServer\Loadbalancing\GameServer\navdata /S /Y
xcopy ..\photonserver\src-server\Loadbalancing\LoadBalancing\navdata\*.bytes zdspServer\Loadbalancing\Master\navdata /S /Y
xcopy ..\photonserver\src-server\Loadbalancing\LoadBalancing\navdata\*.bytes zdspServer\CounterPublisher\navdata /S /Y