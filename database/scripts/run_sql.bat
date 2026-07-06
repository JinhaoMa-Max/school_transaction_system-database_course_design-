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
echo [3] 创建封装层（003~006: 视图/函数/存储过程/触发器）
echo [4] 插入种子数据（seed/seed_data.sql）
echo [5] 运行完整测试（007_test.sql）
echo [6] 进入 SQL*Plus 交互模式
echo [7] 查看容器日志
echo [q] 退出
echo.

set /p choice="请输入编号并按回车: "

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
echo  正在创建数据库表...
echo ========================================
echo.
docker exec -i campus_trade_db sqlplus CAMPUS/Campus123456@FREEPDB1 < ..\ddl\001_create_tables_docker.sql
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
docker exec -i campus_trade_db sqlplus CAMPUS/Campus123456@FREEPDB1 < ..\ddl\002_verify.sql
echo.
pause
goto menu

:create_objects
cls
echo ========================================
echo  正在创建视图（003_views.sql）...
echo ========================================
docker exec -i campus_trade_db sqlplus CAMPUS/Campus123456@FREEPDB1 < ..\ddl\003_views.sql
echo.
echo ========================================
echo  正在创建函数（004_functions.sql）...
echo ========================================
docker exec -i campus_trade_db sqlplus CAMPUS/Campus123456@FREEPDB1 < ..\ddl\004_functions.sql
echo.
echo ========================================
echo  正在创建存储过程（005_procedures.sql）...
echo ========================================
docker exec -i campus_trade_db sqlplus CAMPUS/Campus123456@FREEPDB1 < ..\ddl\005_procedures.sql
echo.
echo ========================================
echo  正在创建触发器（006_triggers.sql）...
echo ========================================
docker exec -i campus_trade_db sqlplus CAMPUS/Campus123456@FREEPDB1 < ..\ddl\006_triggers.sql
echo.
echo ✅ 封装层全部创建完成！
echo    （10 视图 + 10 函数 + 13 存储过程 + 7 业务触发器）
echo.
pause
goto menu

:seed_data
cls
echo ========================================
echo  正在插入种子数据（测试用）...
echo ========================================
echo.
echo  数据内容：8用户 + 13分类 + 15商品 + 20图片
echo           10收藏 + 5议价 + 6订单 + 5面交
echo           4会话 + 10消息 + 4评价 + 3举报
echo           4审核日志 + 3公告
echo.
docker exec -i campus_trade_db sqlplus CAMPUS/Campus123456@FREEPDB1 < ..\seed\seed_data.sql
echo.
pause
goto menu

:run_test
cls
echo ========================================
echo  正在运行完整功能测试...
echo ========================================
docker exec -i campus_trade_db sqlplus CAMPUS/Campus123456@FREEPDB1 < ..\ddl\007_test.sql
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
