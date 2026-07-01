@echo off
setlocal

set "ROOT_DIR=%~dp0"
set "PORT=8765"
set "URL=http://127.0.0.1:%PORT%/index.html"
set "SERVER_COMMAND="

cd /d "%ROOT_DIR%"

where py >nul 2>nul
if %errorlevel%==0 (
  set "SERVER_COMMAND=py -3 -m http.server %PORT%"
  goto :start_server
)

where python >nul 2>nul
if %errorlevel%==0 (
  set "SERVER_COMMAND=python -m http.server %PORT%"
  goto :start_server
)

echo.
echo Python 3 nao foi encontrado nesta maquina.
echo Instale o Python e tente novamente.
echo.
pause
exit /b 1

:start_server
echo Iniciando Export Planner em %URL%
echo Pasta: %ROOT_DIR%
echo.
echo Nao feche a janela do servidor enquanto estiver usando o planner.
echo.

start "LioConnecta Planner" cmd /k "cd /d ""%ROOT_DIR%"" && %SERVER_COMMAND%"

timeout /t 2 /nobreak >nul
start "" "%URL%"

echo Navegador aberto em %URL%
echo.
echo Para encerrar, feche a janela chamada "LioConnecta Planner".
echo.

endlocal
