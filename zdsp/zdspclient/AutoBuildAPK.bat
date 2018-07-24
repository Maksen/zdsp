@echo off
set "PROJPATH=%cd%"

set serverversion=%1
set buildnumber=%2
set platform=%3
set servertype=%4
set config=%5
set buildassetbundle=%6
set useassetbundle=%7
set buildplayer=%8
set copygamerepo=%9

SHIFT
SHIFT
SHIFT
set mycard=%7

echo == Initiating system instance variables...
echo. -- Setting the variables...

if %copygamerepo% == yes (
	cd ..
    call copyGameRepo.bat
	cd %PROJPATH%
)

rem Here you need to make some changes to suit your system.
set UNITY=C:\Program Files\Unity\Editor\Unity.exe

@echo -----Start: %time%
Taskkill /IM Unity.exe /F

rem -server version,build version,platform type,server type,debug or release,build or not build asset bundle,include asset bundle in build or not
rem set BUILDARG=-BuildAssetBundle -BuildPlayer -Android -ServerVersion 1.0.0 -BuildNumber 1 -PrivateServer
set BUILDARG=-BuildPlayer %buildplayer% -ServerVersion %serverversion% -BuildNumber %buildnumber% -Platform %platform% -ServerType %servertype% -BuildAssetBundle %buildassetbundle% -mycard %mycard%
set PREPROCESSORARG=-Configuration %config% -UseAssetBundle %useassetbundle% -mycard %mycard%
set "PROJPATH=%cd%"


echo. == Running Unity...

echo. == Setting Preprocessor...
rem Setting preprocessor before build
"%UNITY%" -quit -batchmode -projectPath "%PROJPATH%" -executeMethod ZealotBuild.SetPreprocessor %PREPROCESSORARG%
IF %ERRORLEVEL% NEQ 0 GOTO ProcessError

echo. == Updating Unity Asset...
rem Update AssetDatabase of any changed Unity Asset
"%UNITY%" -quit -batchmode -projectPath "%PROJPATH%" -executeMethod ZealotBuild.RefreshAsset
IF %ERRORLEVEL% NEQ 0 GOTO ProcessError

echo. == Building Unity...
rem Perform a build with AssetBundle and BuildPlayer
"%UNITY%" -quit -batchmode -projectPath "%PROJPATH%" -executeMethod ZealotBuild.PerformBuild %BUILDARG%
IF %ERRORLEVEL% NEQ 0 GOTO ProcessError

@rem no error in building
echo no error
exit /b 0

:ProcessError
@rem process error
echo got error
exit /b 1