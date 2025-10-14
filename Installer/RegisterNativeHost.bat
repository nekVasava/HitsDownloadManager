@echo off
REM Registry script to register HitsDownloadManager native messaging host
SET INSTALL_DIR=%~dp0
SET MANIFEST_PATH=%INSTALL_DIR%native-host-manifest.json
echo Installing HitsDownloadManager Native Messaging Host...
REM Register for Chrome
REG ADD "HKCU\Software\Google\Chrome\NativeMessagingHosts\com.hitsdownloadmanager.native" /ve /t REG_SZ /d "%MANIFEST_PATH%" /f
if %ERRORLEVEL% EQU 0 (
    echo [OK] Registered for Chrome
) else (
    echo [FAIL] Failed to register for Chrome
)
REM Register for Edge
REG ADD "HKCU\Software\Microsoft\Edge\NativeMessagingHosts\com.hitsdownloadmanager.native" /ve /t REG_SZ /d "%MANIFEST_PATH%" /f
if %ERRORLEVEL% EQU 0 (
    echo [OK] Registered for Edge
) else (
    echo [FAIL] Failed to register for Edge
)
REM Register for Brave
REG ADD "HKCU\Software\BraveSoftware\Brave-Browser\NativeMessagingHosts\com.hitsdownloadmanager.native" /ve /t REG_SZ /d "%MANIFEST_PATH%" /f
if %ERRORLEVEL% EQU 0 (
    echo [OK] Registered for Brave
) else (
    echo [FAIL] Failed to register for Brave
)
echo.
echo Native messaging host registration complete!
echo You may need to restart your browser for changes to take effect.
pause
