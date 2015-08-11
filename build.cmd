@echo off
pushd %~dp0
setlocal

if exist bin goto build
mkdir bin

:Build

REM Find the most recent 32bit MSBuild.exe on the system. Require v12.0 (installed with VS2013) or later since .NET 4.0
REM is not supported. Also handle x86 operating systems, where %ProgramFiles(x86)% is not defined. Always quote the
REM %MSBuild% value when setting the variable and never quote %MSBuild% references.
set MSBuild="%ProgramFiles(x86)%\MSBuild\14.0\Bin\MSBuild.exe"
if not exist %MSBuild% @set MSBuild="%ProgramFiles(x86)%\MSBuild\12.0\Bin\MSBuild.exe"
if not exist %MSBuild% (
  echo Could not find msbuild.exe. Please run this from a Visual Studio developer prompt
  goto BuildFail
)

REM Start Azure Storage Emulator
set StorageEmulator="%ProgramFiles(x86)%\Microsoft SDKs\Azure\Storage Emulator\AzureStorageEmulator.exe"
if not exist %StorageEmulator% (
  echo Could not find AzureStorageEmulator.exe. Please install Azure SDK from http://azure.microsoft.com/en-us/downloads/
  goto BuildFail
)

%StorageEmulator% start 2> NUL

if "%1" == "" goto BuildDefaults

%MSBuild% WebHooks.msbuild /m /nr:false /t:%* /p:Platform="Any CPU" /p:Desktop=true /v:M /fl /flp:LogFile=bin\msbuild.log;Verbosity=Normal
if %ERRORLEVEL% neq 0 goto BuildFail
goto BuildSuccess

:BuildDefaults
%MSBuild% WebHooks.msbuild /m /nr:false /p:Platform="Any CPU" /p:Desktop=true /v:M /fl /flp:LogFile=bin\msbuild.log;Verbosity=Normal
if %ERRORLEVEL% neq 0 goto BuildFail
goto BuildSuccess

:BuildFail
echo.
echo *** BUILD FAILED ***
goto End

:BuildSuccess
echo.
echo **** BUILD SUCCESSFUL ***
goto end

:End
popd
endlocal
