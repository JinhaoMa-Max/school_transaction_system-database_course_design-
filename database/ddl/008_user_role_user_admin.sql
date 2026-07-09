SET SQLBLANKLINES ON
WHENEVER SQLERROR EXIT SQL.SQLCODE;

/*
  Migrate app_user.role from buyer/seller/admin to user/admin.
  Fresh installs are covered by 001_create_tables*.sql; this script updates
  an existing schema and data set.
*/

ALTER TABLE app_user DROP CONSTRAINT ck_app_user_role;

UPDATE app_user
SET role = 'user'
WHERE role IN ('buyer', 'seller');

ALTER TABLE app_user MODIFY role DEFAULT 'user';

ALTER TABLE app_user ADD CONSTRAINT ck_app_user_role
CHECK (role IN ('user', 'admin'));

CREATE OR REPLACE VIEW v_seller_stats AS
SELECT
    u.user_id,
    u.nickname,
    u.avatar,
    u.credit_score,
    sa.college,
    sa.auth_status,
    COUNT(DISTINCT g.goods_id) AS active_goods,
    COUNT(DISTINCT CASE WHEN o.order_status = 'completed' THEN o.order_id END) AS completed_orders,
    ROUND(
        COUNT(DISTINCT CASE WHEN r.rating >= 4 THEN r.review_id END) * 100.0
        / NULLIF(COUNT(DISTINCT r.review_id), 0),
        1
    ) AS positive_rate,
    COUNT(DISTINCT r.review_id) AS review_count
FROM app_user u
LEFT JOIN student_auth sa ON u.user_id = sa.user_id
LEFT JOIN goods g ON u.user_id = g.seller_id AND g.goods_status IN ('approved', 'sold')
LEFT JOIN trade_order o ON u.user_id = o.seller_id
LEFT JOIN review r ON u.user_id = r.reviewed_user_id
WHERE u.role = 'user'
GROUP BY u.user_id, u.nickname, u.avatar, u.credit_score, sa.college, sa.auth_status;

COMMIT;