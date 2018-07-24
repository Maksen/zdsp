echo %1 > D:\zealot\piliq\piliclient\Assets\Resources\version.txt
xcopy D:\zealot\piliq\piliclient\Assets\Resources\version.txt PiliServer /S /Y
xcopy Assets\Resources\version.txt D:\zealot\piliq\photonserver\src-server\Loadbalancing\version.txt /S /Y