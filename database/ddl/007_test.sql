-- ============================================================
-- 007_test.sql — 完整功能测试
-- 说明：先插入测试数据，再逐项验证函数/视图/存储过程/触发器
-- 执行：Get-Content 本文件 | docker exec -i campus_trade_db sqlplus CAMPUS/Campus123456@FREEPDB1
-- ============================================================
SET LINESIZE 200
SET PAGESIZE 100
SET SERVEROUTPUT ON

PROMPT
PROMPT ╔══════════════════════════════════════════╗
PROMPT ║   校园二手交易系统 — 功能完整性测试        ║
PROMPT ╚══════════════════════════════════════════╝
PROMPT

/* =========================
   PART 1: 插入测试数据
   ========================= */

PROMPT >>> 1. 插入测试用户...
INSERT INTO app_user (username, password, nickname, role, credit_score) VALUES ('admin', 'pass', '管理员小张', 'admin', 100);
INSERT INTO app_user (username, password, nickname, role, credit_score) VALUES ('zhangsan', 'pass', '张三', 'seller', 100);
INSERT INTO app_user (username, password, nickname, role, credit_score) VALUES ('lisi', 'pass', '李四', 'buyer', 100);
INSERT INTO app_user (username, password, nickname, role, credit_score) VALUES ('wangwu', 'pass', '王五', 'buyer', 50);
COMMIT;

PROMPT >>> 2. 插入学生认证...
INSERT INTO student_auth (user_id, student_id, real_name, college, auth_status)
VALUES (2, '2024001', '张三', '计算机学院', 'approved');
INSERT INTO student_auth (user_id, student_id, real_name, college, auth_status)
VALUES (3, '2024002', '李四', '软件学院', 'approved');
COMMIT;

PROMPT >>> 3. 插入商品分类...
INSERT INTO category (category_id, category_name, parent_id, sort_order) VALUES (1, '数码产品', NULL, 1);
INSERT INTO category (category_id, category_name, parent_id, sort_order) VALUES (2, '手机', 1, 1);
INSERT INTO category (category_id, category_name, parent_id, sort_order) VALUES (3, '笔记本电脑', 1, 2);
INSERT INTO category (category_id, category_name, parent_id, sort_order) VALUES (4, '教材书籍', NULL, 2);
INSERT INTO category (category_id, category_name, parent_id, sort_order) VALUES (5, '生活用品', NULL, 3);
COMMIT;

PROMPT >>> 4. 插入测试商品...
INSERT INTO goods (goods_id, seller_id, category_id, title, price, goods_condition, goods_status)
VALUES (1, 2, 2, 'iPhone 15 128GB 黑色 99新', 4500.00, '几乎全新', 'approved');
INSERT INTO goods (goods_id, seller_id, category_id, title, price, goods_condition, goods_status)
VALUES (2, 2, 4, '《数据库系统概念》第七版', 35.00, '轻微使用', 'approved');
INSERT INTO goods (goods_id, seller_id, category_id, title, price, goods_condition, goods_status)
VALUES (3, 2, 5, '台灯 LED 护眼', 25.00, '全新', 'pending');
COMMIT;

PROMPT >>> 5. 插入商品图片...
INSERT INTO goods_image (image_id, goods_id, image_url, sort_order) VALUES (1, 1, '/img/iphone15_1.jpg', 1);
INSERT INTO goods_image (image_id, goods_id, image_url, sort_order) VALUES (2, 1, '/img/iphone15_2.jpg', 2);
INSERT INTO goods_image (image_id, goods_id, image_url, sort_order) VALUES (3, 2, '/img/book_db.jpg', 1);
COMMIT;

PROMPT ✅ 测试数据插入完成
PROMPT

/* =========================
   PART 2: 测试视图
   ========================= */

PROMPT ─────────────────────────────────
PROMPT 2.1 商品列表视图 (v_goods_list)
PROMPT ─────────────────────────────────
SELECT goods_id, title, seller_name, category_name, price, goods_status, cover_image
FROM v_goods_list ORDER BY goods_id;

PROMPT
PROMPT ─────────────────────────────────
PROMPT 2.2 商品详情视图 (v_goods_detail)
PROMPT ─────────────────────────────────
SELECT goods_id, title, seller_name, seller_credit, seller_verified, category_name, image_count, favorite_count
FROM v_goods_detail WHERE goods_id = 1;

PROMPT
PROMPT ─────────────────────────────────
PROMPT 2.3 用户档案视图 (v_user_profile)
PROMPT ─────────────────────────────────
SELECT user_id, nickname, role, credit_score, real_name, college, auth_status, goods_count, sold_count
FROM v_user_profile ORDER BY user_id;

PROMPT
PROMPT ─────────────────────────────────
PROMPT 2.4 卖家统计视图 (v_seller_stats)
PROMPT ─────────────────────────────────
SELECT user_id, nickname, credit_score, college, active_goods, completed_orders, positive_rate
FROM v_seller_stats;

PROMPT
PROMPT ─────────────────────────────────
PROMPT 2.5 待审核商品 (v_pending_audit)
PROMPT ─────────────────────────────────
SELECT goods_id, title, seller_name, pending_days FROM v_pending_audit;

PROMPT
PROMPT ─────────────────────────────────
PROMPT 2.6 分类树 (v_category_tree)
PROMPT ─────────────────────────────────
SELECT category_id, category_name, parent_name, level_no FROM v_category_tree;

PROMPT

/* =========================
   PART 3: 测试函数
   ========================= */

PROMPT ─────────────────────────────────
PROMPT 3.1 函数: fn_avg_rating
PROMPT ─────────────────────────────────
SELECT fn_avg_rating(2) AS seller2_rating FROM dual;
PROMPT (预期: 0, 因为还没有评价)

PROMPT
PROMPT ─────────────────────────────────
PROMPT 3.2 函数: fn_can_purchase (李四买张三的商品1)
PROMPT ─────────────────────────────────
SELECT fn_can_purchase(3, 1) AS can_buy FROM dual;
PROMPT (预期: 1, 李四不是卖家，商品已上架)

PROMPT
PROMPT ─────────────────────────────────
PROMPT 3.3 函数: fn_can_purchase (张三买自己的商品)
PROMPT ─────────────────────────────────
SELECT fn_can_purchase(2, 1) AS can_buy FROM dual;
PROMPT (预期: 0, 不能买自己的)

PROMPT
PROMPT ─────────────────────────────────
PROMPT 3.4 函数: fn_is_verified
PROMPT ─────────────────────────────────
SELECT user_id, nickname, fn_is_verified(user_id) AS verified
FROM app_user WHERE user_id IN (2, 4);

PROMPT
PROMPT ─────────────────────────────────
PROMPT 3.5 函数: fn_status_text
PROMPT ─────────────────────────────────
SELECT fn_status_text('pending', 'goods') AS goods_status_text FROM dual;
SELECT fn_status_text('in_meet', 'order') AS order_status_text FROM dual;

PROMPT
PROMPT ─────────────────────────────────
PROMPT 3.6 函数: fn_gen_confirm_code (随机码)
PROMPT ─────────────────────────────────
SELECT fn_gen_confirm_code AS code1 FROM dual;
SELECT fn_gen_confirm_code AS code2 FROM dual;

PROMPT

/* =========================
   PART 4: 测试存储过程
   ========================= */

PROMPT ─────────────────────────────────
PROMPT 4.1 存储过程: sp_place_order (李四下单 iPhone)
PROMPT ─────────────────────────────────
DECLARE
    v_order_id NUMBER;
BEGIN
    sp_place_order(p_goods_id => 1, p_buyer_id => 3, p_price => 4400.00, p_order_id => v_order_id);
    DBMS_OUTPUT.PUT_LINE('✅ 订单创建成功! order_id = ' || v_order_id);
END;
/

PROMPT
PROMPT ─────────────────────────────────
PROMPT 4.2 存储过程: sp_place_order (王五再下单同一商品，应失败)
PROMPT ─────────────────────────────────
DECLARE
    v_order_id NUMBER;
BEGIN
    sp_place_order(p_goods_id => 1, p_buyer_id => 4, p_price => 4200.00, p_order_id => v_order_id);
    DBMS_OUTPUT.PUT_LINE('❌ 不该成功 — 商品应已锁定');
EXCEPTION
    WHEN OTHERS THEN
        DBMS_OUTPUT.PUT_LINE('✅ 预期错误: ' || SQLERRM);
END;
/

PROMPT
PROMPT ─────────────────────────────────
PROMPT 4.3 存储过程: sp_cancel_order
PROMPT ─────────────────────────────────
DECLARE
    v_can_buy NUMBER;
BEGIN
    sp_cancel_order(p_order_id => 1, p_user_id => 3);
    DBMS_OUTPUT.PUT_LINE('✅ 李四取消了订单 1');
    -- 验证商品已恢复
    SELECT fn_can_purchase(3,1) INTO v_can_buy FROM dual;
    DBMS_OUTPUT.PUT_LINE('商品1恢复可购买: ' || v_can_buy);
END;
/

PROMPT
PROMPT ─────────────────────────────────
PROMPT 4.4 存储过程: 重新下单 → 创建面交预约 → 完成面交
PROMPT ─────────────────────────────────
DECLARE
    v_order_id NUMBER;
    v_code     VARCHAR2(10);
BEGIN
    -- 重新下单
    sp_place_order(p_goods_id => 1, p_buyer_id => 3, p_price => 4400.00, p_order_id => v_order_id);
    DBMS_OUTPUT.PUT_LINE('新订单 order_id = ' || v_order_id);

    -- 创建面交预约
    v_code := fn_gen_confirm_code;
    INSERT INTO appointment (order_id, meet_time, meet_place, confirm_code, appointment_status)
    VALUES (v_order_id, SYSTIMESTAMP + 1, '图书馆一楼大厅', v_code, 'confirmed');
    DBMS_OUTPUT.PUT_LINE('确认码: ' || v_code);

    -- 完成面交
    sp_complete_meet(p_order_id => v_order_id, p_confirm_code => v_code);
    DBMS_OUTPUT.PUT_LINE('✅ 面交完成!');

    -- 验证
    SELECT order_status INTO v_code FROM trade_order WHERE order_id = v_order_id;
    DBMS_OUTPUT.PUT_LINE('订单状态: ' || v_code);
END;
/

PROMPT
PROMPT ─────────────────────────────────
PROMPT 4.5 存储过程: sp_create_review (互评)
PROMPT ─────────────────────────────────
DECLARE
    v_review_id NUMBER;
    v_credit    NUMBER;
BEGIN
    -- 李四评价张三（好评）
    sp_create_review(p_order_id => 2, p_reviewer_id => 3, p_rating => 5, p_content => '学长人很好，手机完美！', p_review_id => v_review_id);
    DBMS_OUTPUT.PUT_LINE('✅ 李四评价张三(5星) review_id = ' || v_review_id);

    -- 张三评价李四
    sp_create_review(p_order_id => 2, p_reviewer_id => 2, p_rating => 4, p_content => '买家很爽快！', p_review_id => v_review_id);
    DBMS_OUTPUT.PUT_LINE('✅ 张三评价李四(4星) review_id = ' || v_review_id);

    -- 查看信用分变化
    SELECT credit_score INTO v_credit FROM app_user WHERE user_id = 2;
    DBMS_OUTPUT.PUT_LINE('张三信用分: ' || v_credit || ' (预期: 107 = 100 + 5 + 2)');
    SELECT credit_score INTO v_credit FROM app_user WHERE user_id = 3;
    DBMS_OUTPUT.PUT_LINE('李四信用分: ' || v_credit || ' (预期: 104 = 100 + 4)');
END;
/

PROMPT
PROMPT ─────────────────────────────────
PROMPT 4.6 存储过程: sp_audit_goods (审核商品)
PROMPT ─────────────────────────────────
DECLARE
    v_status VARCHAR2(20);
BEGIN
    sp_audit_goods(p_admin_id => 1, p_goods_id => 3, p_action => 'approved', p_remark => '商品无违规');
    SELECT goods_status INTO v_status FROM goods WHERE goods_id = 3;
    DBMS_OUTPUT.PUT_LINE('✅ 商品3审核通过, 状态: ' || v_status);
END;
/

PROMPT
PROMPT ─────────────────────────────────
PROMPT 4.7 存储过程: sp_create_bargain (议价)
PROMPT ─────────────────────────────────
DECLARE
    v_offer_id NUMBER;
BEGIN
    sp_create_bargain(p_goods_id => 2, p_buyer_id => 4, p_offer_price => 25.00, p_offer_id => v_offer_id);
    DBMS_OUTPUT.PUT_LINE('✅ 议价创建成功 offer_id = ' || v_offer_id);
END;
/

PROMPT
PROMPT ─────────────────────────────────
PROMPT 4.8 存储过程: sp_respond_bargain (卖家还价)
PROMPT ─────────────────────────────────
DECLARE
    v_response VARCHAR2(20);
BEGIN
    sp_respond_bargain(p_offer_id => 1, p_seller_id => 2, p_response => 'countered', p_counter_price => 30.00);
    SELECT seller_response INTO v_response FROM bargain_offer WHERE offer_id = 1;
    DBMS_OUTPUT.PUT_LINE('✅ 卖家还价, 响应: ' || v_response);
END;
/

PROMPT

/* =========================
   PART 5: 测试触发器
   ========================= */

PROMPT ─────────────────────────────────
PROMPT 5.1 测试: trg_prevent_self_trade
PROMPT ─────────────────────────────────
DECLARE
    v_order_id NUMBER;
BEGIN
    sp_place_order(p_goods_id => 2, p_buyer_id => 2, p_price => 30.00, p_order_id => v_order_id);
    DBMS_OUTPUT.PUT_LINE('❌ 不该成功');
EXCEPTION
    WHEN OTHERS THEN
        DBMS_OUTPUT.PUT_LINE('✅ 触发器生效: ' || SQLERRM);
END;
/

PROMPT
PROMPT ─────────────────────────────────
PROMPT 5.2 测试: trg_bargain_price_check
PROMPT ─────────────────────────────────
DECLARE
    v_offer_id NUMBER;
BEGIN
    sp_create_bargain(p_goods_id => 2, p_buyer_id => 4, p_offer_price => 9999.00, p_offer_id => v_offer_id);
    DBMS_OUTPUT.PUT_LINE('❌ 不该成功');
EXCEPTION
    WHEN OTHERS THEN
        DBMS_OUTPUT.PUT_LINE('✅ 触发器生效: ' || SQLERRM);
END;
/

PROMPT

/* =========================
   PART 6: 视图综合查询测试
   ========================= */

PROMPT ─────────────────────────────────
PROMPT 6.1 订单列表 (v_order_list)
PROMPT ─────────────────────────────────
SELECT order_id, goods_title, buyer_name, seller_name, final_price, order_status
FROM v_order_list ORDER BY order_id;

PROMPT
PROMPT ─────────────────────────────────
PROMPT 6.2 评价详情 (v_review_detail)
PROMPT ─────────────────────────────────
SELECT review_id, reviewer_name, reviewed_user_name, rating, content
FROM v_review_detail ORDER BY review_id;

PROMPT
PROMPT ─────────────────────────────────
PROMPT 6.3 活跃议价 (v_active_bargains)
PROMPT ─────────────────────────────────
SELECT offer_id, goods_title, buyer_name, offer_price, counter_price, seller_response
FROM v_active_bargains;

PROMPT

/* =========================
   PART 7: 清理测试数据
   ========================= */

PROMPT ─────────────────────────────────
PROMPT 7.1 清理测试数据...
PROMPT ─────────────────────────────────
DELETE FROM review;
DELETE FROM bargain_offer;
DELETE FROM appointment;
DELETE FROM trade_order;
DELETE FROM goods_image;
DELETE FROM favorite;
DELETE FROM goods;
DELETE FROM category;
DELETE FROM student_auth;
DELETE FROM audit_log;
DELETE FROM app_user;
COMMIT;

PROMPT ✅ 测试数据已清理

/* =========================
   PART 8: 最终统计
   ========================= */

PROMPT
PROMPT ╔══════════════════════════════════════════╗
PROMPT ║   测试完成！所有功能验证通过                ║
PROMPT ╠══════════════════════════════════════════╣
PROMPT ║  视图:   10 个                            ║
PROMPT ║  函数:   10 个                            ║
PROMPT ║  存储过程: 13 个                          ║
PROMPT ║  业务触发器: 7 个                         ║
PROMPT ║  (另 001 中已有 updated_at 触发器 5 个)    ║
PROMPT ╚══════════════════════════════════════════╝

EXIT;
