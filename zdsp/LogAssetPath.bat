@echo off
echo == Initiating system instance variables...
echo. -- Setting the variables...

:: Here you need to make some changes to suit your system.
set UNITY=C:\Program Files\Unity\Editor\Unity.exe
set EXECOPYTOSTAGING=\\BUDDHABUILD\Share\TransferAssetBundle.exe
set OUTPUTFOLDERER=\\jinnong\share

echo Unity will be forced to close. 
echo Data will be lost if unsaved. 
echo Do you want to continue?
choice /c YN /M "choice:"
IF "%ERRORLEVEL%" == "1" goto begin
IF "%ERRORLEVEL%" == "2" goto end

:begin
@echo -----Start: %time%
taskkill /im unity.exe /f

set PROJPATH=D:\zealot\piliq\piliclient

echo. == Running Unity...
echo. == Logging Asset...
::Update AssetDatabase of any changed Unity Asset
"%UNITY%" -quit -batchmode -projectPath %PROJPATH% -executeMethod AssetBundleTools.ListAssetPath

:end
echo. ++ Done.
@echo -----End: %time%
pause