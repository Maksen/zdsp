@echo off
rem The available verbosity levels are q[uiet], m[inimal], n[ormal], d[etailed], and diag[nostic]
set verbosity=%2
set buildfile=..\..\build\deploy.proj
set configuration=%1
%WINDIR%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe %buildfile% /verbosity:%verbosity% /l:FileLogger,Microsoft.Build.Engine;logfile=log\LoadbalancingBuild.log;verbosity=%verbosity%;performancesummary /property:Configuration="%configuration%" /t:BuildLoadbalancing
