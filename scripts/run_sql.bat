@echo off
chcp 65001 >nul
title 校园二手交易系统 - 数据库部署脚本

echo ========================================
echo  校园二手交易系统 - 数据库部署
echo ========================================
echo.
echo 请确保 Oracle 容器已启动！
echo docker ps ^| findstr campus_trade_db
echo.

:menu
echo 请选择操作：
echo [1] 创建数据库表（001_create_tables.sql）
echo [2] 验证表结构（002_verify.sql）
echo [3] 初始化测试数据
echo [4] 进入 SQL*Plus 交互模式
echo [5] 查看容器日志
echo [q] 退出
echo.

set /p choice="请输入编号并按回车: "

if "%choice%"=="1" goto create_tables
if "%choice%"=="2" goto verify
if "%choice%"=="3" goto seed_data
if "%choice%"=="4" goto sqlplus
if "%choice%"=="5" goto logs
if "%choice%"=="q" goto end
if "%choice%"=="Q" goto end
goto menu

:create_tables
cls
echo ========================================
echo  正在创建数据库表...
echo ========================================
echo.
echo 使用 Docker 方式执行建表脚本...
echo docker exec -i campus_trade_db sqlplus CAMPUS/Campus123456@FREEPDB1 ^< ..\sql\001_create_tables_docker.sql
docker exec -i campus_trade_db sqlplus CAMPUS/Campus123456@FREEPDB1 < ..\sql\001_create_tables_docker.sql
echo.
if %errorlevel% equ 0 (
    echo ✅ 建表成功！
) else (
    echo ❌ 建表失败，请检查错误信息。
)
echo.
pause
goto menu

:verify
cls
echo ========================================
echo  正在验证数据库结构...
echo ========================================
docker exec -i campus_trade_db sqlplus CAMPUS/Campus123456@FREEPDB1 < ..\sql\002_verify.sql
echo.
pause
goto menu

:seed_data
cls
echo ========================================
echo  TODO: 需要先编写 003_seed_data.sql
echo ========================================
echo.
pause
goto menu

:sqlplus
cls
echo ========================================
echo  进入 SQL*Plus 交互模式
echo  退出请输 EXIT 回车
echo ========================================
echo.
echo docker exec -it campus_trade_db sqlplus CAMPUS/Campus123456@FREEPDB1
docker exec -it campus_trade_db sqlplus CAMPUS/Campus123456@FREEPDB1
goto menu

:logs
cls
docker logs campus_trade_db --tail 50
echo.
pause
goto menu

:end
echo 再见！
pause
