@echo off
chcp 65001 >nul
title Fatura Aktarim Uygulamasi
:: Proje klasorundan cift tiklayinca uygulamayi acar.
:: EXE, derleme sonrasi su konumda olusur: InvoiceTransferApp.UI\bin\Debug\net10.0-windows\InvoiceTransferApp.UI.exe

set "EXE=%~dp0InvoiceTransferApp.UI\bin\Debug\net10.0-windows\InvoiceTransferApp.UI.exe"
if exist "%EXE%" (
    start "" "%EXE%"
    exit /b 0
)

:: EXE yoksa once derleyip sonra acmayi dene (dotnet run)
echo EXE bulunamadi. Derleniyor...
cd /d "%~dp0InvoiceTransferApp.UI"
dotnet run
if errorlevel 1 pause
