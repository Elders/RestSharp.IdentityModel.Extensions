@echo off

%FAKE% %NYX% "target=clean" -st
%FAKE% %NYX% "target=RestoreNugetPackages" -st

IF NOT [%1]==[] (set RELEASE_NUGETKEY="%1")

SET SUMMARY="RestSharp.IdentityModel.Extensions"
SET DESCRIPTION="RestSharp.IdentityModel.Extensions"

%FAKE% %NYX% appName=RestSharp.IdentityModel.Extensions appSummary=%SUMMARY% appDescription=%DESCRIPTION%  nugetkey=%RELEASE_NUGETKEY%
