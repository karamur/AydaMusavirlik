@echo off
chcp 65001 >nul
title AYDA Musavirlik - Kurulum Olusturucu
color 0A

echo.
echo ============================================
echo   AYDA Musavirlik - Kurulum Olusturucu
echo   Allbatros Software Solutions
echo ============================================
echo.

set PROJECT_DIR=%~dp0
set OUTPUT_DIR=C:\ARYAMusavirlik\kurulum

echo Proje: %PROJECT_DIR%
echo Cikti: %OUTPUT_DIR%
echo.

echo [1/6] Kurulum dizinleri olusturuluyor...
mkdir "%OUTPUT_DIR%" 2>nul
mkdir "%OUTPUT_DIR%\Api" 2>nul
mkdir "%OUTPUT_DIR%\Web" 2>nul
mkdir "%OUTPUT_DIR%\Desktop" 2>nul

echo [2/6] API derleniyor...
cd /d "%PROJECT_DIR%AydaMusavirlik.Api"
dotnet publish -c Release -r win-x64 --self-contained true -o "%OUTPUT_DIR%\Api"
if %errorlevel% neq 0 goto :error

echo [3/6] Web derleniyor...
cd /d "%PROJECT_DIR%AydaMusavirlik.Web"
dotnet publish -c Release -r win-x64 --self-contained true -o "%OUTPUT_DIR%\Web"
if %errorlevel% neq 0 goto :error

echo [4/6] Desktop derleniyor...
cd /d "%PROJECT_DIR%AydaMusavirlik.Desktop"
dotnet publish -c Release -r win-x64 --self-contained true -o "%OUTPUT_DIR%\Desktop"
if %errorlevel% neq 0 goto :error

echo [5/6] Dokumanlar kopyalaniyor...
cd /d "%PROJECT_DIR%"
copy /y "README.md" "%OUTPUT_DIR%\" 2>nul
copy /y "docs\KULLANIM-KILAVUZU.md" "%OUTPUT_DIR%\" 2>nul

echo [6/6] Masaustu kisayolu olusturuluyor...
powershell -Command "$ws = New-Object -ComObject WScript.Shell; $s = $ws.CreateShortcut('%USERPROFILE%\Desktop\AYDA Musavirlik.lnk'); $s.TargetPath = '%OUTPUT_DIR%\Baslat-Tum-Uygulamalar.bat'; $s.WorkingDirectory = '%OUTPUT_DIR%'; $s.Description = 'AYDA Musavirlik - Mali Musavirlik Yazilimi'; $s.Save()"

echo.
echo ============================================
echo   KURULUM TAMAMLANDI!
echo ============================================
echo.
echo   Kurulum Dizini: %OUTPUT_DIR%
echo   Masaustu Kisayolu Olusturuldu
echo.
pause
goto :end

:error
echo.
echo HATA: Derleme basarisiz oldu!
echo.
pause
exit /b 1

:end
