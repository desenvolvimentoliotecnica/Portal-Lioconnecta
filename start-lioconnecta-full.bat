@echo off
setlocal

set "ROOT_DIR=%~dp0"
set "FRONT_DIR=%ROOT_DIR%LioConnecta"
set "API_PROJECT=%ROOT_DIR%src\PortalLioConnecta.Api\PortalLioConnecta.Api.csproj"
set "FRONT_PORT=3020"
set "FRONT_URL=http://127.0.0.1:%FRONT_PORT%/"
set "NODE_STATIC_SERVER=%FRONT_DIR%\dev-static-server.js"
set "API_URL=http://localhost:3030"

if not exist "%FRONT_DIR%\index.html" (
  echo.
  echo Frontend da LIOCONNECTA nao encontrado em:
  echo %FRONT_DIR%
  echo.
  pause
  exit /b 1
)

if not exist "%API_PROJECT%" (
  echo.
  echo Projeto da API nao encontrado em:
  echo %API_PROJECT%
  echo.
  pause
  exit /b 1
)

where dotnet >nul 2>nul
if errorlevel 1 (
  echo.
  echo .NET SDK nao foi encontrado.
  echo Instale o .NET 8 SDK e tente novamente.
  echo.
  pause
  exit /b 1
)

set "FRONT_COMMAND="
set "FRONT_MODE="

where node >nul 2>nul
if %errorlevel%==0 (
  set "FRONT_COMMAND=node dev-static-server.js"
  set "FRONT_MODE=node"
)

if not defined FRONT_COMMAND (
  where python >nul 2>nul
  if %errorlevel%==0 (
    set "FRONT_COMMAND=python -m http.server %FRONT_PORT%"
    set "FRONT_MODE=python"
  )
)

if not defined FRONT_COMMAND (
  where py >nul 2>nul
  if %errorlevel%==0 (
    set "FRONT_COMMAND=py -3 -m http.server %FRONT_PORT%"
    set "FRONT_MODE=python-launcher"
  )
)

if not defined FRONT_COMMAND (
  echo.
  echo Nenhum runtime compativel foi encontrado para o frontend.
  echo Instale Python 3 ou Node.js e tente novamente.
  echo.
  pause
  exit /b 1
)

echo.
echo Subindo LIOCONNECTA completa...
echo Frontend: %FRONT_URL%
echo API:      %API_URL%
echo Runtime frontend: %FRONT_MODE%
echo.

start "LIOCONNECTA API" cmd /k "cd /d ""%ROOT_DIR%"" && set ASPNETCORE_ENVIRONMENT=Development && set DOTNET_ENVIRONMENT=Development && dotnet build ""%API_PROJECT%"" && dotnet run --no-build --no-restore --no-launch-profile --project ""%API_PROJECT%"" --urls %API_URL%"
start "LIOCONNECTA Frontend" cmd /k "cd /d ""%FRONT_DIR%"" && %FRONT_COMMAND%"

timeout /t 4 /nobreak >nul
start "" "%FRONT_URL%"

echo Navegador aberto em %FRONT_URL%
echo.
echo Para encerrar:
echo - feche a janela "LIOCONNECTA API"
echo - feche a janela "LIOCONNECTA Frontend"
echo.

endlocal
