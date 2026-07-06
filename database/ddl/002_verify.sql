-- =============================================================
-- 验证脚本：建表完成后运行，检查所有对象是否创建成功
-- 使用方式：sqlplus CAMPUS/Campus123456@//localhost:1521/FREEPDB1 @002_verify.sql
-- =============================================================

SET LINESIZE 200
SET PAGESIZE 50
SET SERVEROUTPUT ON

PROMPT ========================================
PROMPT 1. 检查所有表是否已创建
PROMPT ========================================

SELECT table_name, tablespace_name, status
FROM user_tables
ORDER BY table_name;

PROMPT
PROMPT ========================================
PROMPT 2. 统计各表记录数（刚建完应该全部为0）
PROMPT ========================================

SELECT 'app_user' AS table_name, COUNT(*) AS row_count FROM app_user
UNION ALL SELECT 'student_auth', COUNT(*) FROM student_auth
UNION ALL SELECT 'category', COUNT(*) FROM category
UNION ALL SELECT 'goods', COUNT(*) FROM goods
UNION ALL SELECT 'goods_image', COUNT(*) FROM goods_image
UNION ALL SELECT 'favorite', COUNT(*) FROM favorite
UNION ALL SELECT 'bargain_offer', COUNT(*) FROM bargain_offer
UNION ALL SELECT 'trade_order', COUNT(*) FROM trade_order
UNION ALL SELECT 'appointment', COUNT(*) FROM appointment
UNION ALL SELECT 'chat_session', COUNT(*) FROM chat_session
UNION ALL SELECT 'chat_message', COUNT(*) FROM chat_message
UNION ALL SELECT 'review', COUNT(*) FROM review
UNION ALL SELECT 'report', COUNT(*) FROM report
UNION ALL SELECT 'audit_log', COUNT(*) FROM audit_log
UNION ALL SELECT 'notice', COUNT(*) FROM notice
ORDER BY table_name;

PROMPT
PROMPT ========================================
PROMPT 3. 检查所有约束（主键、外键、检查约束）
PROMPT ========================================

SELECT constraint_name, constraint_type, table_name, status
FROM user_constraints
ORDER BY table_name, constraint_type;

PROMPT
PROMPT ========================================
PROMPT 4. 检查所有触发器
PROMPT ========================================

SELECT trigger_name, table_name, status, trigger_type, triggering_event
FROM user_triggers
ORDER BY trigger_name;

PROMPT
PROMPT ========================================
PROMPT 5. 检查所有索引
PROMPT ========================================

SELECT index_name, table_name, uniqueness, status
FROM user_indexes
ORDER BY table_name, index_name;

PROMPT
PROMPT ========================================
PROMPT 6. 检查序列（IDENTITY 列自动生成的序列）
PROMPT ========================================

SELECT sequence_name, min_value, max_value, increment_by
FROM user_sequences
ORDER BY sequence_name;

PROMPT
PROMPT ========================================
PROMPT ✅ 验证完成！所有对象应全部显示 VALID / ENABLED
PROMPT ========================================

EXIT;
