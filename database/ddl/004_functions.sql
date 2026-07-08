-- ============================================================
-- 004_functions.sql — 函数封装
-- 用途：为 Service 层提供可复用的业务计算和校验函数
-- 执行：Get-Content 本文件 | docker exec -i campus_trade_db sqlplus CAMPUS/Campus123456@FREEPDB1
-- ============================================================

/* =========================
   1. 计算用户平均评分
   入参：user_id
   返回：NUMBER(3,1) 平均分，无评价返回 0
   ========================= */
CREATE OR REPLACE FUNCTION fn_avg_rating(
    p_user_id IN NUMBER
) RETURN NUMBER
IS
    v_avg NUMBER(3,1);
BEGIN
    SELECT ROUND(AVG(rating), 1)
    INTO v_avg
    FROM review
    WHERE reviewed_user_id = p_user_id;

    RETURN NVL(v_avg, 0);
END;
/

-- 测试：SELECT fn_avg_rating(1) FROM dual;


/* =========================
   2. 计算卖家已售商品数
   入参：seller_id
   返回：已完成的订单数
   ========================= */
CREATE OR REPLACE FUNCTION fn_sold_count(
    p_seller_id IN NUMBER
) RETURN NUMBER
IS
    v_count NUMBER;
BEGIN
    SELECT COUNT(*)
    INTO v_count
    FROM trade_order
    WHERE seller_id = p_seller_id
      AND order_status = 'completed';

    RETURN v_count;
END;
/

-- 测试：SELECT fn_sold_count(2) FROM dual;


/* =========================
   3. 检查用户是否可以购买该商品
   规则：
     - 商品状态必须是 'approved'
     - 买家不能是自己的商品
     - 买家不能被封禁
     - 买家余额（信用分）必须 >= 0
   返回：1=可以，0=不可以
   ========================= */
CREATE OR REPLACE FUNCTION fn_can_purchase(
    p_user_id IN NUMBER,
    p_goods_id IN NUMBER
) RETURN NUMBER
IS
    v_seller_id  NUMBER;
    v_status     VARCHAR2(20);
    v_user_status VARCHAR2(20);
    v_credit     NUMBER;
BEGIN
    -- 查商品
    SELECT seller_id, goods_status
    INTO v_seller_id, v_status
    FROM goods
    WHERE goods_id = p_goods_id;

    -- 查用户
    SELECT status, credit_score
    INTO v_user_status, v_credit
    FROM app_user
    WHERE user_id = p_user_id;

    -- 校验
    IF v_status != 'approved' THEN
        RETURN 0;  -- 商品不可购买
    END IF;
    IF v_seller_id = p_user_id THEN
        RETURN 0;  -- 不能买自己的
    END IF;
    IF v_user_status = 'banned' THEN
        RETURN 0;  -- 用户被封禁
    END IF;
    IF v_credit < 0 THEN
        RETURN 0;  -- 信用分异常
    END IF;

    RETURN 1;
EXCEPTION
    WHEN NO_DATA_FOUND THEN
        RETURN 0;  -- 商品或用户不存在
END;
/

-- 测试：SELECT fn_can_purchase(1, 3) AS can_buy FROM dual;


/* =========================
   4. 获取用户未读消息数量
   入参：user_id
   返回：未读消息条数
   ========================= */
CREATE OR REPLACE FUNCTION fn_unread_count(
    p_user_id IN NUMBER
) RETURN NUMBER
IS
    v_count NUMBER;
BEGIN
    SELECT COUNT(*)
    INTO v_count
    FROM chat_message cm
    JOIN chat_session cs ON cm.session_id = cs.session_id
    WHERE cm.is_read = 0
      AND (
          (cs.buyer_id = p_user_id AND cm.sender_id != p_user_id)
          OR
          (cs.seller_id = p_user_id AND cm.sender_id != p_user_id)
      );

    RETURN v_count;
END;
/

-- 测试：SELECT fn_unread_count(1) FROM dual;


/* =========================
   5. 状态码转中文
   用途：让前端/Service 直接拿到可读文本
   ========================= */
CREATE OR REPLACE FUNCTION fn_status_text(
    p_status IN VARCHAR2,
    p_type   IN VARCHAR2 DEFAULT 'goods'
) RETURN VARCHAR2
IS
BEGIN
    IF p_type = 'goods' THEN
        RETURN CASE p_status
            WHEN 'pending'   THEN '待审核'
            WHEN 'approved'  THEN '已上架'
            WHEN 'rejected'  THEN '审核拒绝'
            WHEN 'locked'    THEN '已锁定'
            WHEN 'sold'      THEN '已售出'
            WHEN 'offline'   THEN '已下架'
            ELSE p_status
        END;
    ELSIF p_type = 'order' THEN
        RETURN CASE p_status
            WHEN 'pending_meet' THEN '待面交'
            WHEN 'in_meet'      THEN '面交中'
            WHEN 'completed'    THEN '已完成'
            WHEN 'cancelled'    THEN '已取消'
            ELSE p_status
        END;
    ELSIF p_type = 'auth' THEN
        RETURN CASE p_status
            WHEN 'pending'  THEN '待审核'
            WHEN 'approved' THEN '已认证'
            WHEN 'rejected' THEN '认证拒绝'
            ELSE p_status
        END;
    ELSE
        RETURN p_status;
    END IF;
END;
/

-- 测试：
-- SELECT fn_status_text('pending', 'goods') FROM dual;  → 待审核
-- SELECT fn_status_text('in_meet', 'order') FROM dual;  → 面交中


/* =========================
   6. 检查用户是否已认证
   返回：1=已认证，0=未认证/待审核
   ========================= */
CREATE OR REPLACE FUNCTION fn_is_verified(
    p_user_id IN NUMBER
) RETURN NUMBER
IS
    v_status VARCHAR2(20);
BEGIN
    SELECT auth_status INTO v_status
    FROM student_auth
    WHERE user_id = p_user_id;

    IF v_status = 'approved' THEN
        RETURN 1;
    ELSE
        RETURN 0;
    END IF;
EXCEPTION
    WHEN NO_DATA_FOUND THEN
        RETURN 0;
END;
/

-- 测试：SELECT fn_is_verified(1) FROM dual;


/* =========================
   7. 生成随机确认码（用于面交）
   返回：6位随机数字字符串
   ========================= */
CREATE OR REPLACE FUNCTION fn_gen_confirm_code
RETURN VARCHAR2
IS
    v_code VARCHAR2(10);
BEGIN
    SELECT LPAD(TRUNC(DBMS_RANDOM.VALUE(0, 999999)), 6, '0')
    INTO v_code
    FROM dual;

    RETURN v_code;
END;
/

-- 测试：SELECT fn_gen_confirm_code FROM dual;


/* =========================
   8. 计算信用分
   规则：基础分100 + 好评(+5) - 差评(-10) + 成交(+2)
   入参：user_id
   返回：新信用分
   ========================= */
CREATE OR REPLACE FUNCTION fn_calc_credit(
    p_user_id IN NUMBER
) RETURN NUMBER
IS
    v_base       NUMBER := 100;
    v_good_bonus NUMBER := 0;
    v_bad_penalty NUMBER := 0;
    v_trade_bonus NUMBER := 0;
    v_score      NUMBER;
BEGIN
    -- 好评加分（4-5星）
    SELECT NVL(COUNT(*), 0) * 5
    INTO v_good_bonus
    FROM review
    WHERE reviewed_user_id = p_user_id
      AND rating >= 4;

    -- 差评减分（1-2星）
    SELECT NVL(COUNT(*), 0) * 10
    INTO v_bad_penalty
    FROM review
    WHERE reviewed_user_id = p_user_id
      AND rating <= 2;

    -- 成交加分
    SELECT NVL(COUNT(*), 0) * 2
    INTO v_trade_bonus
    FROM trade_order
    WHERE seller_id = p_user_id
      AND order_status = 'completed';

    v_score := v_base + v_good_bonus - v_bad_penalty + v_trade_bonus;

    -- 不低于 0
    RETURN GREATEST(v_score, 0);
END;
/

-- 测试：SELECT fn_calc_credit(1) FROM dual;


/* =========================
   9. 用户收藏的商品ID列表（管道函数，用于分页）
   返回：用户收藏的商品ID结果集
   ========================= */
CREATE OR REPLACE FUNCTION fn_favorite_goods_ids(
    p_user_id IN NUMBER
) RETURN VARCHAR2
IS
    v_ids VARCHAR2(4000);
BEGIN
    SELECT LISTAGG(goods_id, ',') WITHIN GROUP (ORDER BY created_at DESC)
    INTO v_ids
    FROM favorite
    WHERE user_id = p_user_id;

    RETURN v_ids;
END;
/

-- 测试：SELECT fn_favorite_goods_ids(1) FROM dual;


/* =========================
   10. 商品浏览量自增
   返回：新的浏览量
   ========================= */
CREATE OR REPLACE FUNCTION fn_increment_view(
    p_goods_id IN NUMBER
) RETURN NUMBER
IS

    PRAGMA AUTONOMOUS_TRANSACTION;
    v_new_count NUMBER;
BEGIN
    UPDATE goods
    SET view_count = view_count + 1
    WHERE goods_id = p_goods_id
    RETURNING view_count INTO v_new_count;

    COMMIT;

    RETURN v_new_count;
EXCEPTION
    WHEN NO_DATA_FOUND THEN
        RETURN 0;
END;
/

-- 测试：SELECT fn_increment_view(1) FROM dual;


COMMIT;
EXIT;
