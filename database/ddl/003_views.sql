-- ============================================================
-- 003_views.sql — 视图封装
-- 用途：为 Repository 层提供预连表查询，避免 Service 层写复杂 JOIN
-- 执行：Get-Content 本文件 | docker exec -i campus_trade_db sqlplus CAMPUS/Campus123456@FREEPDB1
-- ============================================================

/* =========================
   1. 商品列表视图 — 首页/搜索/分类浏览
   ========================= */
CREATE OR REPLACE VIEW v_goods_list AS
SELECT
    g.goods_id,
    g.seller_id,
    u.nickname AS seller_name,
    u.avatar AS seller_avatar,
    g.category_id,
    c.category_name,
    g.title,
    g.price,
    g.goods_condition,
    g.goods_status,
    g.view_count,
    g.created_at,
    g.updated_at,
    -- 第一张图片（用于列表缩略图）
    (SELECT MIN(gi.image_url)
     FROM goods_image gi
     WHERE gi.goods_id = g.goods_id
       AND gi.sort_order = (SELECT MIN(sort_order) FROM goods_image WHERE goods_id = g.goods_id)
    ) AS cover_image
FROM goods g
JOIN app_user u ON g.seller_id = u.user_id
JOIN category c ON g.category_id = c.category_id
WHERE g.goods_status IN ('approved', 'sold');

-- 使用示例：SELECT * FROM v_goods_list WHERE category_id = 1 ORDER BY created_at DESC;


/* =========================
   2. 商品详情视图 — 商品页完整信息
   ========================= */
CREATE OR REPLACE VIEW v_goods_detail AS
SELECT
    g.goods_id,
    g.seller_id,
    u.nickname AS seller_name,
    u.avatar AS seller_avatar,
    u.credit_score AS seller_credit,
    -- 卖家是否已实名
    (SELECT CASE WHEN sa.auth_status = 'approved' THEN 1 ELSE 0 END
     FROM student_auth sa WHERE sa.user_id = u.user_id) AS seller_verified,
    g.category_id,
    c.category_name,
    c.parent_id AS category_parent_id,
    (SELECT pc.category_name FROM category pc WHERE pc.category_id = c.parent_id) AS category_parent_name,
    g.title,
    g.description,
    g.price,
    g.goods_condition,
    g.goods_status,
    g.view_count,
    g.created_at,
    g.updated_at,
    -- 图片数量
    (SELECT COUNT(*) FROM goods_image gi WHERE gi.goods_id = g.goods_id) AS image_count,
    -- 收藏数
    (SELECT COUNT(*) FROM favorite f WHERE f.goods_id = g.goods_id) AS favorite_count
FROM goods g
JOIN app_user u ON g.seller_id = u.user_id
JOIN category c ON g.category_id = c.category_id;

-- 使用示例：SELECT * FROM v_goods_detail WHERE goods_id = 3;


/* =========================
   3. 订单列表视图 — 我的订单/卖出的
   ========================= */
CREATE OR REPLACE VIEW v_order_list AS
SELECT
    o.order_id,
    o.goods_id,
    g.title AS goods_title,
    g.goods_condition,
    o.buyer_id,
    bu.nickname AS buyer_name,
    o.seller_id,
    su.nickname AS seller_name,
    o.final_price,
    o.order_status,
    -- 面交预约信息
    a.meet_time,
    a.meet_place,
    a.confirm_code,
    a.appointment_status,
    -- 是否已评价
    (SELECT COUNT(*) FROM review r WHERE r.order_id = o.order_id AND r.reviewer_id = o.buyer_id) AS buyer_reviewed,
    (SELECT COUNT(*) FROM review r WHERE r.order_id = o.order_id AND r.reviewer_id = o.seller_id) AS seller_reviewed,
    o.created_at,
    o.updated_at
FROM trade_order o
JOIN goods g ON o.goods_id = g.goods_id
JOIN app_user bu ON o.buyer_id = bu.user_id
JOIN app_user su ON o.seller_id = su.user_id
LEFT JOIN appointment a ON o.order_id = a.order_id;

-- 使用示例：
-- 我买的:  SELECT * FROM v_order_list WHERE buyer_id = 1 ORDER BY created_at DESC;
-- 我卖的:  SELECT * FROM v_order_list WHERE seller_id = 2 ORDER BY created_at DESC;


/* =========================
   4. 用户档案视图 — 个人主页
   ========================= */
CREATE OR REPLACE VIEW v_user_profile AS
SELECT
    u.user_id,
    u.username,
    u.nickname,
    u.avatar,
    u.phone,
    u.email,
    u.role,
    u.status,
    u.credit_score,
    -- 实名信息
    sa.student_id,
    sa.real_name,
    sa.college,
    sa.auth_status,
    -- 统计
    (SELECT COUNT(*) FROM goods g WHERE g.seller_id = u.user_id) AS goods_count,
    (SELECT COUNT(*) FROM trade_order o WHERE o.seller_id = u.user_id AND o.order_status = 'completed') AS sold_count,
    (SELECT COUNT(*) FROM trade_order o WHERE o.buyer_id = u.user_id AND o.order_status = 'completed') AS bought_count,
    -- 平均评分
    (SELECT ROUND(AVG(r.rating), 1) FROM review r WHERE r.reviewed_user_id = u.user_id) AS avg_rating,
    -- 评分人数
    (SELECT COUNT(*) FROM review r WHERE r.reviewed_user_id = u.user_id) AS rating_count,
    u.created_at
FROM app_user u
LEFT JOIN student_auth sa ON u.user_id = sa.user_id;

-- 使用示例：SELECT * FROM v_user_profile WHERE user_id = 1;


/* =========================
   5. 卖家统计视图 — 选卖家时参考
   ========================= */
CREATE OR REPLACE VIEW v_seller_stats AS
SELECT
    u.user_id,
    u.nickname,
    u.avatar,
    u.credit_score,
    sa.college,
    sa.auth_status,
    -- 上架商品数
    COUNT(DISTINCT g.goods_id) AS active_goods,
    -- 成交数
    COUNT(DISTINCT CASE WHEN o.order_status = 'completed' THEN o.order_id END) AS completed_orders,
    -- 好评率
    ROUND(
        COUNT(DISTINCT CASE WHEN r.rating >= 4 THEN r.review_id END) * 100.0
        / NULLIF(COUNT(DISTINCT r.review_id), 0),
        1
    ) AS positive_rate,
    -- 平均响应（简化为评价数）
    COUNT(DISTINCT r.review_id) AS review_count
FROM app_user u
LEFT JOIN student_auth sa ON u.user_id = sa.user_id
LEFT JOIN goods g ON u.user_id = g.seller_id AND g.goods_status IN ('approved', 'sold')
LEFT JOIN trade_order o ON u.user_id = o.seller_id
LEFT JOIN review r ON u.user_id = r.reviewed_user_id
WHERE u.role = 'user'
GROUP BY u.user_id, u.nickname, u.avatar, u.credit_score, sa.college, sa.auth_status;

-- 使用示例：SELECT * FROM v_seller_stats WHERE user_id = 2;


/* =========================
   6. 待审核商品视图 — 管理员审查
   ========================= */
CREATE OR REPLACE VIEW v_pending_audit AS
SELECT
    g.goods_id,
    g.seller_id,
    u.nickname AS seller_name,
    u.credit_score AS seller_credit,
    g.category_id,
    c.category_name,
    g.title,
    g.description,
    g.price,
    g.goods_condition,
    g.goods_status,
    g.created_at,
    -- 等待天数
    TRUNC(SYSDATE - g.created_at) AS pending_days
FROM goods g
JOIN app_user u ON g.seller_id = u.user_id
JOIN category c ON g.category_id = c.category_id
WHERE g.goods_status = 'pending'
ORDER BY g.created_at ASC;

-- 使用示例：SELECT * FROM v_pending_audit;


/* =========================
   7. 未读消息视图 — 消息红点
   ========================= */
CREATE OR REPLACE VIEW v_unread_messages AS
SELECT
    cm.message_id,
    cm.session_id,
    cs.goods_id,
    g.title AS goods_title,
    cm.sender_id,
    su.nickname AS sender_name,
    cm.content,
    cm.is_read,
    cm.created_at,
    CASE
        WHEN cs.buyer_id = cm.sender_id THEN cs.seller_id
        ELSE cs.buyer_id
    END AS receiver_id
FROM chat_message cm
JOIN chat_session cs ON cm.session_id = cs.session_id
JOIN goods g ON cs.goods_id = g.goods_id
JOIN app_user su ON cm.sender_id = su.user_id
WHERE cm.is_read = 0;

-- 使用示例：SELECT COUNT(*) AS unread FROM v_unread_messages WHERE receiver_id = 1;


/* =========================
   8. 分类树视图 — 分层显示所有分类
   ========================= */
CREATE OR REPLACE VIEW v_category_tree AS
SELECT
    c1.category_id,
    c1.category_name,
    c1.parent_id,
    NVL(c2.category_name, '—') AS parent_name,
    c1.sort_order,
    -- 层级
    CASE WHEN c1.parent_id IS NULL THEN 1 ELSE 2 END AS level_no,
    -- 该分类下商品数
    (SELECT COUNT(*) FROM goods g WHERE g.category_id = c1.category_id AND g.goods_status != 'offline') AS goods_count
FROM category c1
LEFT JOIN category c2 ON c1.parent_id = c2.category_id
ORDER BY c1.parent_id NULLS FIRST, c1.sort_order;

-- 使用示例：SELECT * FROM v_category_tree;


/* =========================
   9. 评价详情视图 — 带评价者和被评价者信息
   ========================= */
CREATE OR REPLACE VIEW v_review_detail AS
SELECT
    r.review_id,
    r.order_id,
    r.reviewer_id,
    rv.nickname AS reviewer_name,
    r.reviewed_user_id,
    rd.nickname AS reviewed_user_name,
    r.rating,
    r.content,
    r.created_at,
    -- 对应商品
    o.goods_id,
    g.title AS goods_title
FROM review r
JOIN app_user rv ON r.reviewer_id = rv.user_id
JOIN app_user rd ON r.reviewed_user_id = rd.user_id
JOIN trade_order o ON r.order_id = o.order_id
JOIN goods g ON o.goods_id = g.goods_id;

-- 使用示例：SELECT * FROM v_review_detail WHERE reviewed_user_id = 2 ORDER BY created_at DESC;


/* =========================
   10. 活跃议价视图 — 我的议价
   ========================= */
CREATE OR REPLACE VIEW v_active_bargains AS
SELECT
    b.offer_id,
    b.goods_id,
    g.title AS goods_title,
    g.price AS original_price,
    b.buyer_id,
    bu.nickname AS buyer_name,
    b.seller_response,
    b.offer_price,
    b.counter_price,
    b.offer_status,
    b.created_at,
    b.updated_at,
    g.seller_id
FROM bargain_offer b
JOIN goods g ON b.goods_id = g.goods_id
JOIN app_user bu ON b.buyer_id = bu.user_id
WHERE b.offer_status = 'active';

-- 使用示例：
-- 买家视角: SELECT * FROM v_active_bargains WHERE buyer_id = 1;
-- 卖家视角: SELECT * FROM v_active_bargains WHERE seller_id = 2;


COMMIT;
EXIT;
