@echo off
@echo Application will start in 5 seconds...
ping -n 5 127.1>nul
for /f "skip=3 tokens=4" %%i in ('sc query "xmen_edge_main"') do set "zf=%%i" &goto :next2
:next2
if /i "%zf%"=="" (
    cd %~dp0
    start Magneto.exe
) else (
if /i "%zf%"=="RUNNING" ( 
    net stop "xmen_edge_main" 
)
sc start "xmen_edge_main"
)
