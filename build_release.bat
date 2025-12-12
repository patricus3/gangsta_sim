@echo off
echo Building Gangsta Simulator Release Version
echo ========================================

REM Clean previous builds
echo Cleaning previous builds...
dotnet clean -c Release

REM Build release version
echo Building release version...
dotnet build -c Release --no-restore

if %errorlevel% equ 0 (
    echo.
    echo Release build completed successfully!
    echo.
    echo Now building installer...
    call build_installer.bat
) else (
    echo.
    echo Error building release version!
    pause
    exit /b 1
)