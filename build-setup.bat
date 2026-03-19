@echo off
echo ============================================
echo   AYDA Müþavirlik - Kurulum Derleme Script
echo ============================================
echo.

set OUTPUT_DIR=C:\ARYAMusavirlik\kurulum
set PUBLISH_DIR=%OUTPUT_DIR%\publish

echo Kurulum dizini oluþturuluyor...
if not exist "%OUTPUT_DIR%" mkdir "%OUTPUT_DIR%"
if not exist "%PUBLISH_DIR%" mkdir "%PUBLISH_DIR%"

echo.
echo Uygulama derleniyor...
cd /d "%~dp0AydaMusavirlik.Web"
dotnet publish -c Release -r win-x64 --self-contained true -o "%PUBLISH_DIR%"

if %errorlevel% neq 0 (
    echo HATA: Derleme baþarýsýz!
    pause
    exit /b 1
)

echo.
echo README kopyalanýyor...
copy /y "%~dp0README.md" "%OUTPUT_DIR%\README.md"

echo.
echo Kurulum bilgisi oluþturuluyor...
(
echo ============================================
echo   AYDA Müþavirlik v1.0.0
echo   Profesyonel Mali Müþavirlik Yazýlýmý
echo ============================================
echo.
echo KURULUM TALÝMATLARI:
echo.
echo 1. publish klasöründeki tüm dosyalarý 
echo    istediðiniz dizine kopyalayýn.
echo.
echo 2. AydaMusavirlik.exe dosyasýný çalýþtýrýn.
echo.
echo 3. Tarayýcýnýzda http://localhost:5000 
echo    adresine gidin.
echo.
echo TEST KULLANICILARI:
echo - admin / admin ^(Tam Yetki^)
echo - muhasebe / muhasebe123 ^(Muhasebe^)
echo - yonetici / yonetici123 ^(Yönetici^)
echo.
echo ============================================
echo   Copyright 2026 Allbatros
echo ============================================
) > "%OUTPUT_DIR%\KURULUM.txt"

echo.
echo ============================================
echo   DERLEME TAMAMLANDI!
echo ============================================
echo.
echo Kurulum dosyalarý: %OUTPUT_DIR%
echo.
echo Dosyalar:
dir "%OUTPUT_DIR%" /b
echo.
pause
