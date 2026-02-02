@echo off
:: Turkce karakter sorununu asmak icin 8.3 kisa yol kullan (chcp yok - klavye/consola dokunmaz)
set "PROJE=C:\Users\Mustafa\Desktop\PROJEC~1\InvoiceTransferApp.UI"
dotnet run --project "%PROJE%\InvoiceTransferApp.UI.csproj"
if errorlevel 1 pause
