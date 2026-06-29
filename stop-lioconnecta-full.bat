@echo off
setlocal

echo.
echo Encerrando LIOCONNECTA completa...
echo.

taskkill /FI "WINDOWTITLE eq LIOCONNECTA API" /T /F >nul 2>nul
taskkill /FI "WINDOWTITLE eq LIOCONNECTA Frontend" /T /F >nul 2>nul

powershell -NoProfile -ExecutionPolicy Bypass -Command ^
  "$ErrorActionPreference = 'SilentlyContinue'; " ^
  "try { " ^
  "  $targets = Get-CimInstance Win32_Process | Where-Object { " ^
  "    ($_.Name -match 'dotnet|python|py|node') -and ($_.CommandLine -match 'PortalLioConnecta\.Api\.csproj|http\.server 3020|dev-static-server\.js') " ^
  "  }; " ^
  "  if ($targets) { $targets | ForEach-Object { Stop-Process -Id $_.ProcessId -Force -ErrorAction SilentlyContinue } } " ^
  "} catch { }"

echo Processos da LIOCONNECTA finalizados.
echo.

endlocal
