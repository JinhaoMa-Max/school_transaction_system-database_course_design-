@echo off
chcp 65001 >nul
title Campus Trade System - DB Deploy

echo ========================================
echo  CampusTrade DB Deployment Script
echo ========================================
echo.
echo Make sure Oracle container is running!
echo docker ps ^| findstr campus_trade_db
echo.

:menu
echo Please select:
echo [1] Create tables (001_create_tables_docker.sql)
echo [2] Verify structure (002_verify.sql)
echo [3] Create objects (003~006 views/funcs/procs/triggers)
echo [4] Insert seed data (seed/seed_data.sql)
echo [5] Run full test (007_test.sql)
echo [6] Open SQL*Plus interactive
echo [7] View container logs
echo [q] Quit
echo.

set /p choice="Enter number: "

if "%choice%"=="1" goto create_tables
if "%choice%"=="2" goto verify
if "%choice%"=="3" goto create_objects
if "%choice%"=="4" goto seed_data
if "%choice%"=="5" goto run_test
if "%choice%"=="6" goto sqlplus
if "%choice%"=="7" goto logs
if "%choice%"=="q" goto end
if "%choice%"=="Q" goto end
goto menu

:create_tables
cls
echo ========================================
echo  Creating tables...
echo ========================================
echo.
docker exec -i campus_trade_db sqlplus CAMPUS/Campus123456@FREEPDB1 < ..\ddl\001_create_tables_docker.sql
echo.
if %errorlevel% equ 0 (
    echo [OK] Tables created!
) else (
    echo [FAIL] Check errors above.
)
echo.
pause
goto menu

:verify
cls
echo ========================================
echo  Verifying table structure...
echo ========================================
docker exec -i campus_trade_db sqlplus CAMPUS/Campus123456@FREEPDB1 < ..\ddl\002_verify.sql
echo.
pause
goto menu

:create_objects
cls
echo ========================================
echo  Creating views (003_views.sql)...
echo ========================================
docker exec -i campus_trade_db sqlplus CAMPUS/Campus123456@FREEPDB1 < ..\ddl\003_views.sql
echo.
echo ========================================
echo  Creating functions (004_functions.sql)...
echo ========================================
docker exec -i campus_trade_db sqlplus CAMPUS/Campus123456@FREEPDB1 < ..\ddl\004_functions.sql
echo.
echo ========================================
echo  Creating procedures (005_procedures.sql)...
echo ========================================
docker exec -i campus_trade_db sqlplus CAMPUS/Campus123456@FREEPDB1 < ..\ddl\005_procedures.sql
echo.
echo ========================================
echo  Creating triggers (006_triggers.sql)...
echo ========================================
docker exec -i campus_trade_db sqlplus CAMPUS/Campus123456@FREEPDB1 < ..\ddl\006_triggers.sql
echo.
echo [OK] All objects created!
echo      (10 views + 10 functions + 13 procedures + 7 triggers)
echo.
pause
goto menu

:seed_data
cls
echo ========================================
echo  Inserting seed data...
echo ========================================
echo.
echo  Contents: 8 users + 13 categories + 15 goods + 20 images
echo           10 favorites + 5 bargains + 6 orders + 5 appointments
echo           4 chats + 10 messages + 4 reviews + 3 reports
echo           4 audit logs + 3 notices
echo.
docker exec -i campus_trade_db sqlplus CAMPUS/Campus123456@FREEPDB1 < ..\seed\seed_data.sql
echo.
pause
goto menu

:run_test
cls
echo ========================================
echo  Running full test suite...
echo ========================================
docker exec -i campus_trade_db sqlplus CAMPUS/Campus123456@FREEPDB1 < ..\ddl\007_test.sql
echo.
pause
goto menu

:sqlplus
cls
echo ========================================
echo  Entering SQL*Plus interactive mode
echo  Type EXIT to quit
echo ========================================
echo.
docker exec -it campus_trade_db sqlplus CAMPUS/Campus123456@FREEPDB1
goto menu

:logs
cls
docker logs campus_trade_db --tail 50
echo.
pause
goto menu

:end
echo Bye!
pause
