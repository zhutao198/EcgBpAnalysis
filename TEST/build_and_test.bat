@echo off
rem ============================================================
rem EcgBpAnalysis CI 统一入口：编译解决方案 + 运行单元测试
rem 用法: TEST\build_and_test.bat
rem 退出码: 0=全部成功, 非0=失败（供 CI 判定）
rem 说明: MSBuild/VSTEST 路径可通过环境变量覆盖
rem ============================================================
setlocal enabledelayedexpansion
chcp 65001 >nul

set "ROOT=%~dp0.."
set "MSBUILD=D:\VSBuildTools2022\MSBuild\Current\Bin\MSBuild.exe"
set "VSTEST=D:\VSBuildTools2022\Common7\IDE\Extensions\TestPlatform\vstest.console.exe"
if defined MSBUILD_PATH set "MSBUILD=%MSBUILD_PATH%"
if defined VSTEST_PATH set "VSTEST=%VSTEST_PATH%"
set "ADAPTER=%ROOT%\packages\MSTest.TestAdapter.2.2.10"

if not exist "%MSBUILD%" (
  echo [ERROR] MSBuild not found: %MSBUILD%
  exit /b 1
)
if not exist "%VSTEST%" (
  echo [ERROR] vstest.console not found: %VSTEST%
  exit /b 1
)

echo.
echo [1/3] Building solution (Release^|Any CPU)...
"%MSBUILD%" "%ROOT%\EcgBpAnalysis.sln" /t:Build /p:Configuration=Release /p:Platform="Any CPU" /m /v:m /nologo
if errorlevel 1 (
  echo [ERROR] Solution build FAILED.
  exit /b 1
)

echo.
echo [2/3] Building unit test project...
"%MSBUILD%" "%ROOT%\TEST\UnitTests\EcgBpAnalysis.UnitTests.csproj" /t:Build /p:Configuration=Release /p:Platform=AnyCPU /v:m /nologo
if errorlevel 1 (
  echo [ERROR] Unit test build FAILED.
  exit /b 1
)

echo.
echo [3/3] Running unit tests...
"%VSTEST%" "%ROOT%\TEST\UnitTests\bin\Release\EcgBpAnalysis.UnitTests.dll" /TestAdapterPath:"%ADAPTER%" /Logger:console
if errorlevel 1 (
  echo [ERROR] Unit tests FAILED.
  exit /b 1
)

echo.
echo [OK] Build and tests PASSED.
exit /b 0
