-- ============================================================
-- 005_procedures.sql — 存储过程封装
-- 用途：为 Service 层提供复杂业务操作（事务性、多表操作）
-- 执行：Get-Content 本文件 | docker exec -i campus_trade_db sqlplus CAMPUS/Campus123456@FREEPDB1
-- 注意：部分存储过程内部包含 COMMIT，表示一个完整的事务边界
-- ============================================================

/* =========================
   1. 用户注册
   入参：用户名、密码、昵称、手机、邮箱
   出参：新用户ID
   校验：用户名不能重复
   ========================= */
CREATE OR REPLACE PROCEDURE sp_register_user(
    p_username IN VARCHAR2,
    p_password IN VARCHAR2,
    p_nickname IN VARCHAR2 DEFAULT '新用户',
    p_phone    IN VARCHAR2 DEFAULT NULL,
    p_email    IN VARCHAR2 DEFAULT NULL,
    p_user_id  OUT NUMBER
)
IS
BEGIN
    -- 检查用户名是否已存在
    SELECT COUNT(*) INTO p_user_id FROM app_user WHERE username = p_username;
    IF p_user_id > 0 THEN
        RAISE_APPLICATION_ERROR(-20001, '用户名已存在');
    END IF;

    INSERT INTO app_user (username, password, nickname, phone, email)
    VALUES (p_username, p_password, p_nickname, p_phone, p_email)
    RETURNING user_id INTO p_user_id;

    COMMIT;
END;
/


/* =========================
   2. 下订单（核心交易流程）
   入参：商品ID、买家ID、成交价
   校验：调用 fn_can_purchase 检查购买资格
   效果：插入订单 + 商品状态改为 locked + 关闭该商品上所有活跃议价
   ========================= */
CREATE OR REPLACE PROCEDURE sp_place_order(
    p_goods_id IN NUMBER,
    p_buyer_id IN NUMBER,
    p_price    IN NUMBER,
    p_order_id OUT NUMBER
)
IS
    v_seller_id NUMBER;
    v_can_buy   NUMBER;
BEGIN
    -- 校验购买资格
    v_can_buy := fn_can_purchase(p_buyer_id, p_goods_id);
    IF v_can_buy = 0 THEN
        RAISE_APPLICATION_ERROR(-20002, '无法购买该商品：商品已下架、自己不能买自己的商品、或账号异常');
    END IF;

    -- 查出卖家ID
    SELECT seller_id INTO v_seller_id FROM goods WHERE goods_id = p_goods_id;

    -- 插入订单
    INSERT INTO trade_order (goods_id, buyer_id, seller_id, final_price, order_status)
    VALUES (p_goods_id, p_buyer_id, v_seller_id, p_price, 'pending_meet')
    RETURNING order_id INTO p_order_id;

    -- 锁定商品（防止其他人重复下单）
    UPDATE goods SET goods_status = 'locked' WHERE goods_id = p_goods_id;

    -- 关闭该商品上的所有活跃议价
    UPDATE bargain_offer
    SET offer_status = 'closed', updated_at = SYSTIMESTAMP
    WHERE goods_id = p_goods_id AND offer_status = 'active';

    COMMIT;
END;
/


/* =========================
   3. 取消订单
   入参：订单ID、操作用户ID
   校验：只有买卖双方可以取消 + 只有 pending_meet 状态可取消
   效果：订单取消 + 商品恢复为 approved
   ========================= */
CREATE OR REPLACE PROCEDURE sp_cancel_order(
    p_order_id IN NUMBER,
    p_user_id  IN NUMBER
)
IS
    v_order_status VARCHAR2(20);
    v_buyer_id    NUMBER;
    v_seller_id   NUMBER;
    v_goods_id    NUMBER;
BEGIN
    -- 查订单信息
    SELECT order_status, buyer_id, seller_id, goods_id
    INTO v_order_status, v_buyer_id, v_seller_id, v_goods_id
    FROM trade_order
    WHERE order_id = p_order_id;

    -- 权限校验：只有买卖双方可取消
    IF p_user_id NOT IN (v_buyer_id, v_seller_id) THEN
        RAISE_APPLICATION_ERROR(-20003, '无权取消该订单');
    END IF;

    -- 状态校验
    IF v_order_status != 'pending_meet' THEN
        RAISE_APPLICATION_ERROR(-20004, '当前订单状态不允许取消');
    END IF;

    -- 取消订单
    UPDATE trade_order SET order_status = 'cancelled', updated_at = SYSTIMESTAMP
    WHERE order_id = p_order_id;

    -- 恢复商品状态
    UPDATE goods SET goods_status = 'approved', updated_at = SYSTIMESTAMP
    WHERE goods_id = v_goods_id;

    -- 取消关联的面交预约
    UPDATE appointment SET appointment_status = 'cancelled'
    WHERE order_id = p_order_id AND appointment_status IN ('pending', 'confirmed');

    COMMIT;
END;
/


/* =========================
   4. 完成面交（确认交易完成）
   入参：订单ID、确认码
   效果：订单状态改为 completed + 商品状态改为 sold
   ========================= */
CREATE OR REPLACE PROCEDURE sp_complete_meet(
    p_order_id     IN NUMBER,
    p_confirm_code IN VARCHAR2
)
IS
    v_order_status   VARCHAR2(20);
    v_appt_status    VARCHAR2(20);
    v_appt_code      VARCHAR2(10);
    v_goods_id       NUMBER;
BEGIN
    -- 查订单状态
    SELECT order_status, goods_id
    INTO v_order_status, v_goods_id
    FROM trade_order
    WHERE order_id = p_order_id;

    IF v_order_status NOT IN ('pending_meet', 'in_meet') THEN
        RAISE_APPLICATION_ERROR(-20005, '当前订单状态不允许确认完成');
    END IF;

    -- 校验确认码（防止误确认）
    SELECT appointment_status, confirm_code
    INTO v_appt_status, v_appt_code
    FROM appointment
    WHERE order_id = p_order_id;

    IF v_appt_code != p_confirm_code THEN
        RAISE_APPLICATION_ERROR(-20006, '确认码不正确');
    END IF;

    -- 完成订单
    UPDATE trade_order SET order_status = 'completed', updated_at = SYSTIMESTAMP
    WHERE order_id = p_order_id;

    -- 商品标记已售
    UPDATE goods SET goods_status = 'sold', updated_at = SYSTIMESTAMP
    WHERE goods_id = v_goods_id;

    -- 面交预约标记完成
    UPDATE appointment SET appointment_status = 'completed'
    WHERE order_id = p_order_id;

    COMMIT;
END;
/


/* =========================
   5. 更新用户信用分
   入参：user_id
   说明：在评价后调用，或定期批量执行
   ========================= */
CREATE OR REPLACE PROCEDURE sp_update_credit(
    p_user_id IN NUMBER
)
IS
    v_new_score NUMBER;
BEGIN
    v_new_score := fn_calc_credit(p_user_id);

    UPDATE app_user
    SET credit_score = v_new_score, updated_at = SYSTIMESTAMP
    WHERE user_id = p_user_id;

    COMMIT;
END;
/


/* =========================
   6. 创建议价
   入参：商品ID、买家ID、报价
   校验：不能给自己商品议价、商品必须是已上架状态
   ========================= */
CREATE OR REPLACE PROCEDURE sp_create_bargain(
    p_goods_id  IN NUMBER,
    p_buyer_id  IN NUMBER,
    p_offer_price IN NUMBER,
    p_offer_id  OUT NUMBER
)
IS
    v_seller_id   NUMBER;
    v_goods_status VARCHAR2(20);
BEGIN
    -- 查商品
    SELECT seller_id, goods_status
    INTO v_seller_id, v_goods_status
    FROM goods
    WHERE goods_id = p_goods_id;

    -- 校验
    IF v_seller_id = p_buyer_id THEN
        RAISE_APPLICATION_ERROR(-20007, '不能给自己的商品议价');
    END IF;
    IF v_goods_status != 'approved' THEN
        RAISE_APPLICATION_ERROR(-20008, '该商品当前不可议价');
    END IF;

    -- 检查是否已有活跃议价
    SELECT COUNT(*) INTO p_offer_id
    FROM bargain_offer
    WHERE goods_id = p_goods_id AND buyer_id = p_buyer_id AND offer_status = 'active';

    IF p_offer_id > 0 THEN
        RAISE_APPLICATION_ERROR(-20009, '您已有该商品的活跃议价，请等待卖家回复');
    END IF;

    -- 插入议价
    INSERT INTO bargain_offer (goods_id, buyer_id, offer_price)
    VALUES (p_goods_id, p_buyer_id, p_offer_price)
    RETURNING offer_id INTO p_offer_id;

    COMMIT;
END;
/


/* =========================
   7. 卖家回复议价
   入参：议价ID、回复（accepted/rejected/countered）、还价
   ========================= */
CREATE OR REPLACE PROCEDURE sp_respond_bargain(
    p_offer_id   IN NUMBER,
    p_seller_id  IN NUMBER,
    p_response   IN VARCHAR2,
    p_counter_price IN NUMBER DEFAULT NULL
)
IS
    v_goods_id    NUMBER;
    v_actual_seller NUMBER;
BEGIN
    -- 查出该议价对应商品的真正卖家
    SELECT b.goods_id, g.seller_id
    INTO v_goods_id, v_actual_seller
    FROM bargain_offer b
    JOIN goods g ON b.goods_id = g.goods_id
    WHERE b.offer_id = p_offer_id;

    IF v_actual_seller != p_seller_id THEN
        RAISE_APPLICATION_ERROR(-20010, '只有该商品的卖家才能回复议价');
    END IF;

    UPDATE bargain_offer
    SET seller_response = p_response,
        counter_price = CASE WHEN p_response = 'countered' THEN p_counter_price ELSE NULL END,
        offer_status = CASE p_response
            WHEN 'accepted' THEN 'accepted'
            WHEN 'rejected' THEN 'rejected'
            ELSE 'active'
        END,
        updated_at = SYSTIMESTAMP
    WHERE offer_id = p_offer_id;

    COMMIT;
END;
/


/* =========================
   8. 用户评价（交易互评）
   入参：订单ID、评价者ID、评分(1-5)、评价内容
   校验：必须是订单参与方 + 每方只能评价一次
   效果：插入评价 + 更新被评价者信用分
   ========================= */
CREATE OR REPLACE PROCEDURE sp_create_review(
    p_order_id    IN NUMBER,
    p_reviewer_id IN NUMBER,
    p_rating      IN NUMBER,
    p_content     IN CLOB DEFAULT NULL,
    p_review_id   OUT NUMBER
)
IS
    v_buyer_id       NUMBER;
    v_seller_id      NUMBER;
    v_order_status   VARCHAR2(20);
    v_reviewed_user  NUMBER;
    v_existing       NUMBER;
BEGIN
    -- 查订单
    SELECT buyer_id, seller_id, order_status
    INTO v_buyer_id, v_seller_id, v_order_status
    FROM trade_order
    WHERE order_id = p_order_id;

    -- 只有已完成订单才能评价
    IF v_order_status != 'completed' THEN
        RAISE_APPLICATION_ERROR(-20011, '只有已完成的订单才能评价');
    END IF;

    -- 确定被评价者
    IF p_reviewer_id = v_buyer_id THEN
        v_reviewed_user := v_seller_id;
    ELSIF p_reviewer_id = v_seller_id THEN
        v_reviewed_user := v_buyer_id;
    ELSE
        RAISE_APPLICATION_ERROR(-20012, '只有交易参与方才能评价');
    END IF;

    -- 不能重复评价
    SELECT COUNT(*) INTO v_existing
    FROM review
    WHERE order_id = p_order_id AND reviewer_id = p_reviewer_id;

    IF v_existing > 0 THEN
        RAISE_APPLICATION_ERROR(-20013, '您已对该订单进行过评价');
    END IF;

    -- 插入评价
    INSERT INTO review (order_id, reviewer_id, reviewed_user_id, rating, content)
    VALUES (p_order_id, p_reviewer_id, v_reviewed_user, p_rating, p_content)
    RETURNING review_id INTO p_review_id;

    -- 自动更新被评价者信用分
    sp_update_credit(v_reviewed_user);

    COMMIT;
END;
/


/* =========================
   9. 商品审核（管理员操作）
   入参：管理员ID、商品ID、操作（approved/rejected）
   效果：更新商品状态 + 记录审核日志
   ========================= */
CREATE OR REPLACE PROCEDURE sp_audit_goods(
    p_admin_id IN NUMBER,
    p_goods_id IN NUMBER,
    p_action   IN VARCHAR2,
    p_remark   IN CLOB DEFAULT NULL
)
IS
    v_admin_role VARCHAR2(20);
BEGIN
    -- 校验管理员身份
    SELECT role INTO v_admin_role FROM app_user WHERE user_id = p_admin_id;
    IF v_admin_role != 'admin' THEN
        RAISE_APPLICATION_ERROR(-20014, '只有管理员才能审核商品');
    END IF;

    -- 更新商品状态
    UPDATE goods
    SET goods_status = p_action, updated_at = SYSTIMESTAMP
    WHERE goods_id = p_goods_id;

    -- 记录审核日志
    INSERT INTO audit_log (admin_id, audit_type, target_id, action, result, remark)
    VALUES (p_admin_id, 'goods_audit', p_goods_id, p_action, 'success', p_remark);

    COMMIT;
END;
/


/* =========================
   10. 举报处理（管理员操作）
   入参：举报ID、管理员ID、处理结果
   效果：更新举报状态为 resolved/rejected + 记录日志
   ========================= */
CREATE OR REPLACE PROCEDURE sp_handle_report(
    p_report_id IN NUMBER,
    p_admin_id  IN NUMBER,
    p_result    IN VARCHAR2,
    p_remark    IN CLOB DEFAULT NULL
)
IS
    v_admin_role VARCHAR2(20);
BEGIN
    SELECT role INTO v_admin_role FROM app_user WHERE user_id = p_admin_id;
    IF v_admin_role != 'admin' THEN
        RAISE_APPLICATION_ERROR(-20015, '只有管理员才能处理举报');
    END IF;

    UPDATE report
    SET report_status = p_result
    WHERE report_id = p_report_id;

    INSERT INTO audit_log (admin_id, audit_type, target_id, action, result, remark)
    VALUES (p_admin_id, 'report_handle', p_report_id, p_result, 'success', p_remark);

    COMMIT;
END;
/


/* =========================
   11. 封禁/解封用户（管理员操作）
   入参：管理员ID、目标用户ID、操作（ban/unban）
   ========================= */
CREATE OR REPLACE PROCEDURE sp_manage_user_ban(
    p_admin_id  IN NUMBER,
    p_target_id IN NUMBER,
    p_action    IN VARCHAR2,
    p_remark    IN CLOB DEFAULT NULL
)
IS
    v_admin_role VARCHAR2(20);
    v_new_status VARCHAR2(20);
BEGIN
    SELECT role INTO v_admin_role FROM app_user WHERE user_id = p_admin_id;
    IF v_admin_role != 'admin' THEN
        RAISE_APPLICATION_ERROR(-20016, '只有管理员才能执行此操作');
    END IF;

    v_new_status := CASE p_action WHEN 'ban' THEN 'banned' WHEN 'unban' THEN 'normal' ELSE NULL END;
    IF v_new_status IS NULL THEN
        RAISE_APPLICATION_ERROR(-20017, '操作无效，请使用 ban 或 unban');
    END IF;

    UPDATE app_user SET status = v_new_status, updated_at = SYSTIMESTAMP
    WHERE user_id = p_target_id;

    INSERT INTO audit_log (admin_id, audit_type, target_id, action, result, remark)
    VALUES (p_admin_id, 'user_ban', p_target_id, p_action, 'success', p_remark);

    -- 封禁时下架所有商品
    IF p_action = 'ban' THEN
        UPDATE goods SET goods_status = 'offline', updated_at = SYSTIMESTAMP
        WHERE seller_id = p_target_id AND goods_status IN ('pending', 'approved', 'locked');
    END IF;

    COMMIT;
END;
/


/* =========================
   12. 发送消息
   入参：会话ID、发送者ID、消息内容
   效果：插入消息 + 自动创建会话（如果不存在）
   ========================= */
CREATE OR REPLACE PROCEDURE sp_send_message(
    p_session_id  IN NUMBER,
    p_sender_id   IN NUMBER,
    p_content     IN CLOB,
    p_message_id  OUT NUMBER
)
IS
    v_count NUMBER;
BEGIN
    -- 验证会话存在
    SELECT COUNT(*) INTO v_count FROM chat_session WHERE session_id = p_session_id;
    IF v_count = 0 THEN
        RAISE_APPLICATION_ERROR(-20018, '会话不存在');
    END IF;

    INSERT INTO chat_message (session_id, sender_id, content)
    VALUES (p_session_id, p_sender_id, p_content)
    RETURNING message_id INTO p_message_id;

    COMMIT;
END;
/


/* =========================
   13. 创建或获取聊天会话
   入参：商品ID、买家ID、卖家ID
   出参：会话ID（已存在则返回已有）
   ========================= */
CREATE OR REPLACE PROCEDURE sp_get_or_create_session(
    p_goods_id  IN NUMBER,
    p_buyer_id  IN NUMBER,
    p_seller_id IN NUMBER,
    p_session_id OUT NUMBER
)
IS
    v_count NUMBER;
BEGIN
    -- 先查是否已有会话
    SELECT COUNT(*) INTO v_count
    FROM chat_session
    WHERE goods_id = p_goods_id AND buyer_id = p_buyer_id AND seller_id = p_seller_id;

    IF v_count > 0 THEN
        SELECT session_id INTO p_session_id
        FROM chat_session
        WHERE goods_id = p_goods_id AND buyer_id = p_buyer_id AND seller_id = p_seller_id;
    ELSE
        INSERT INTO chat_session (goods_id, buyer_id, seller_id)
        VALUES (p_goods_id, p_buyer_id, p_seller_id)
        RETURNING session_id INTO p_session_id;
        COMMIT;
    END IF;
END;
/


COMMIT;
EXIT;
