@echo off
cd /d "%~dp0"
echo ========================================
echo  CampusTrade DB - Full Deploy
echo ========================================
echo.

echo [Step 1/5] Creating tables...
docker exec -i campus_trade_db sqlplus CAMPUS/Campus123456@FREEPDB1 < ..\ddl\001_create_tables_docker.sql
echo.

echo [Step 2/5] Creating views...
docker exec -i campus_trade_db sqlplus CAMPUS/Campus123456@FREEPDB1 < ..\ddl\003_views.sql
echo.

echo [Step 3/5] Creating functions...
docker exec -i campus_trade_db sqlplus CAMPUS/Campus123456@FREEPDB1 < ..\ddl\004_functions.sql
echo.

echo [Step 4/5] Creating procedures + triggers...
docker exec -i campus_trade_db sqlplus CAMPUS/Campus123456@FREEPDB1 < ..\ddl\005_procedures.sql
docker exec -i campus_trade_db sqlplus CAMPUS/Campus123456@FREEPDB1 < ..\ddl\006_triggers.sql
echo.

echo [Step 5/5] Inserting seed data...
docker exec -i campus_trade_db sqlplus CAMPUS/Campus123456@FREEPDB1 < ..\seed\seed_data.sql
echo.

echo ========================================
echo  Deploy complete!
echo ========================================
pause
