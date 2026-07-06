-- ============================================================
-- 006_triggers.sql — 业务触发器（补充 001 中的 updated_at 触发器）
-- 执行：Get-Content 本文件 | docker exec -i campus_trade_db sqlplus CAMPUS/Campus123456@FREEPDB1
-- ============================================================

/* =========================
   1. 订单状态变更日志触发器
   记录每次订单状态的变更时间
   ========================= */
CREATE OR REPLACE TRIGGER trg_order_status_log
BEFORE UPDATE OF order_status ON trade_order
FOR EACH ROW
BEGIN
    -- 如果状态真的变了，记录变更
    IF :OLD.order_status != :NEW.order_status THEN
        DBMS_OUTPUT.PUT_LINE(
            '订单 ' || :OLD.order_id || ' 状态: ' ||
            :OLD.order_status || ' → ' || :NEW.order_status ||
            ' @ ' || TO_CHAR(SYSTIMESTAMP, 'YYYY-MM-DD HH24:MI:SS')
        );
    END IF;
END;
/


/* =========================
   2. 防止自买自卖触发器
   下单时校验 buyer_id != seller_id
   ========================= */
CREATE OR REPLACE TRIGGER trg_prevent_self_trade
BEFORE INSERT ON trade_order
FOR EACH ROW
BEGIN
    IF :NEW.buyer_id = :NEW.seller_id THEN
        RAISE_APPLICATION_ERROR(-20030, '不能购买自己的商品');
    END IF;
END;
/


/* =========================
   3. 议价金额不能超过原价触发器
   ========================= */
CREATE OR REPLACE TRIGGER trg_bargain_price_check
BEFORE INSERT OR UPDATE ON bargain_offer
FOR EACH ROW
DECLARE
    v_original_price NUMBER(10,2);
BEGIN
    SELECT price INTO v_original_price
    FROM goods WHERE goods_id = :NEW.goods_id;

    IF :NEW.offer_price > v_original_price THEN
        RAISE_APPLICATION_ERROR(-20031, '议价金额不能高于商品原价 ' || v_original_price);
    END IF;
END;
/


/* =========================
   4. 商品发布时自动创建默认分类（如果未指定）
   不传分类时默认为"其他"
   ========================= */
CREATE OR REPLACE TRIGGER trg_goods_default_category
BEFORE INSERT ON goods
FOR EACH ROW
DECLARE
    v_default_id NUMBER;
BEGIN
    IF :NEW.category_id IS NULL THEN
        -- 查找或创建"其他"分类
        BEGIN
            SELECT category_id INTO v_default_id
            FROM category WHERE category_name = '其他' AND parent_id IS NULL;
        EXCEPTION
            WHEN NO_DATA_FOUND THEN
                INSERT INTO category (category_name) VALUES ('其他')
                RETURNING category_id INTO v_default_id;
        END;
        :NEW.category_id := v_default_id;
    END IF;
END;
/


/* =========================
   5. 评价后自动更新用户信用分触发器
   ========================= */
CREATE OR REPLACE TRIGGER trg_after_review_credit
AFTER INSERT ON review
FOR EACH ROW
DECLARE
    PRAGMA AUTONOMOUS_TRANSACTION;
    v_new_score NUMBER;
BEGIN
    v_new_score := fn_calc_credit(:NEW.reviewed_user_id);

    UPDATE app_user
    SET credit_score = v_new_score, updated_at = SYSTIMESTAMP
    WHERE user_id = :NEW.reviewed_user_id;

    COMMIT;
END;
/


/* =========================
   6. 举报自动生成触发告警
   当某用户被举报超过 5 次时输出警告
   ========================= */
CREATE OR REPLACE TRIGGER trg_report_threshold_alert
AFTER INSERT ON report
FOR EACH ROW
DECLARE
    v_count NUMBER;
BEGIN
    SELECT COUNT(*) INTO v_count
    FROM report
    WHERE target_user_id = :NEW.target_user_id
      AND report_status IN ('pending', 'processing');

    IF v_count >= 5 THEN
        DBMS_OUTPUT.PUT_LINE(
            '⚠️ 警告：用户 ' || :NEW.target_user_id ||
            ' 已有 ' || v_count || ' 条未处理举报！'
        );
    END IF;
END;
/


/* =========================
   7. 清理过期面交预约触发器
   当订单取消/完成时，对应的 pending 状态预约自动取消
   ========================= */
CREATE OR REPLACE TRIGGER trg_cancel_appointment_on_order
AFTER UPDATE OF order_status ON trade_order
FOR EACH ROW
BEGIN
    IF :NEW.order_status IN ('cancelled', 'completed') THEN
        UPDATE appointment
        SET appointment_status = CASE
            WHEN :NEW.order_status = 'completed' THEN 'completed'
            ELSE 'cancelled'
        END
        WHERE order_id = :NEW.order_id
          AND appointment_status IN ('pending', 'confirmed');
    END IF;
END;
/


COMMIT;
EXIT;
