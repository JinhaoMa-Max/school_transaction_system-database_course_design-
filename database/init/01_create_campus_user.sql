-- =============================================================
-- 初始化脚本：为 CAMPUS 用户补充权限
-- 注意：CAMPUS 用户已由 gvenzl 镜像的 APP_USER 环境变量自动创建
--       本脚本仅做补充授权（首次启动时自动执行）
-- =============================================================

-- 补充授予表空间配额
ALTER USER CAMPUS QUOTA UNLIMITED ON USERS;

-- 补充授予同义词创建权限（RESOURCE 角色不包含）
GRANT CREATE SYNONYM TO CAMPUS;

-- 补充授予视图创建权限（确保有）
GRANT CREATE VIEW TO CAMPUS;

-- 方便调试
GRANT SELECT ANY TABLE TO CAMPUS;

COMMIT;

-- 验证当前状态
SELECT username, account_status, default_tablespace
FROM dba_users
WHERE username = 'CAMPUS';
