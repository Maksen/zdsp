@ECHO OFF
SETLOCAL ENABLEEXTENSIONS

SET copperfieldConfigFullPath="D:\zealot\zdsp\zdsprepo\copperfieldbuild\copperfield.config"
SET copperfieldConfigBackupFullPath="D:\zealot\zdsp\zdsprepo\copperfieldbuild\copperfield.config_publishbackup"
SET copperfieldGeneratorFullPath="D:\zealot\zdsp\zdsprepo\copperfieldbuild\CopperfieldGenerator.exe"

SET kopioSolutionFullPath="D:\zealot\zdsp\zdsprepo\kopio\Kopio.sln"
SET kopioWebCsprojFullPath="D:\zealot\zdsp\zdsprepo\kopio\Kopio.Web\Kopio.Web.csproj"

SET kopioPublishCopperfieldConfigFullPath="D:\zealot\zdsp\zdsprepo\kopiopublish\copperfield.config"
SET kopioPublishDirPath="D:\zealot\zdsp\zdsprepo\kopiopublish"
SET kopioPublishProfileFile="publish.pubxml"

SET signToolDirPath="C:\Program Files (x86)\Windows Kits\8.1\bin\x86"

ECHO ******************************************************
ECHO *             KopioPublish: Initialized              *
ECHO ******************************************************
ECHO.
ECHO Warning! For STAGING/LIVE use ONLY.
ECHO.
ECHO This tool will automate the following in sequence:
ECHO 1. Executes copperfieldgenerator.exe
ECHO 2. Build and Publish kopio.sln into //192.168.11.79/zdsp
ECHO.
ECHO ******************************************************
ECHO *             KopioPublish: Initialized              *
ECHO ******************************************************

ECHO.
SET /P userInput=Proceed (Y/[N])? 
IF /I NOT "%userInput%"=="y" (
	EXIT
)

CLS

ECHO ******************************************************
ECHO *      KopioPublish: Editing Copperfield Config      * 
ECHO ******************************************************
ECHO.
ECHO Backing up %copperfieldConfigFullPath% as %copperfieldConfigBackupFullPath%
ECHO.
COPY /Y %copperfieldConfigFullPath% %copperfieldConfigBackupFullPath%
ECHO.
ECHO Overwriting %copperfieldConfigFullPath% to %copperfieldConfigFullPath%
ECHO.
COPY /Y %kopioPublishCopperfieldConfigFullPath% %copperfieldConfigFullPath%
ECHO.
ECHO ******************************************************
ECHO *   KopioPublish: Editing Copperfield Config Done    * 
ECHO ******************************************************

ECHO.
SET /P userInput=Proceed to execute Copperfield (Y/[N])? 
IF /I NOT "%userInput%"=="y" (
	GOTO :REVERTCONFIG 
)

CLS

ECHO ******************************************************
ECHO *        KopioPublish: Executing Copperfield         *
ECHO ****************************************************** 
ECHO.
ECHO Calling %copperfieldGeneratorFullPath%
ECHO.
CALL %copperfieldGeneratorFullPath%
ECHO.
ECHO ******************************************************
ECHO *     KopioPublish: Executing Copperfield Done       * 
ECHO ******************************************************

ECHO.
SET /P userInput=Proceed to build and publish Kopio (Y/[N])? 
IF /I NOT "%userInput%"=="y" (
	GOTO :REVERTCONFIG 
)

CLS

ECHO ******************************************************
ECHO *       KopioPublish: Build and Publish Kopio        *
ECHO ******************************************************
ECHO.
ECHO Building %kopioSolutionFullPath%;
ECHO.
SET msBuildDir=%WINDIR%\Microsoft.NET\Framework\v4.0.30319
CALL %msBuildDir%\msbuild.exe %kopioSolutionFullPath% /p:Configuration=Release;VisualStudioVersion=14.0;SignToolPath=%signToolDirPath%;DeployOnBuild=true;PublishProfileRootFolder=%kopioPublishDirPath%;PublishProfile=%kopioPublishProfileFile%
ECHO.
ECHO ******************************************************
ECHO *     KopioPublish: Build and Publish Kopio Done     *
ECHO ******************************************************

ECHO.
PAUSE

CLS

:REVERTCONFIG
ECHO ******************************************************
ECHO *     KopioPublish: Reverting Copperfield Config     *
ECHO ******************************************************
ECHO.
ECHO Restoring %copperfieldConfigFullPath% from %copperfieldConfigBackupFullPath%
ECHO.
COPY /Y %copperfieldConfigBackupFullPath% %copperfieldConfigFullPath%
ECHO.
ECHO Deleting backup %copperfieldConfigBackupFullPath%
ECHO.
DEL %copperfieldConfigBackupFullPath%
ECHO.
ECHO ******************************************************
ECHO *  KopioPublish: Reverting Copperfield Config Done   *
ECHO ******************************************************

ECHO.
PAUSE

CLS

ECHO ******************************************************
ECHO *              KopioPublish: Completed               *
ECHO ******************************************************
ECHO.

PAUSE