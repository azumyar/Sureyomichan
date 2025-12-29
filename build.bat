@echo off
PATH=%PATH%;C:\Program Files\dotnet
pushd "%~dp0"

pushd src\cs-sureyomichan\src\Sureyomichan
dotnet clean -c Release
set r=%errorlevel%
if %r% neq 0 goto error

dotnet build -c Release
set r=%errorlevel%
if %r% neq 0 goto error

dotnet publish -c Release
set r=%errorlevel%
if %r% neq 0 goto error
popd

xcopy /e /i src\js-sureyomichan src\cs-sureyomichan\dist\chrome-extensions
xcopy /e /i src\js-tegaki_save src\cs-sureyomichan\dist\tegaki_save


:error
@rem exit /B %r%

pause