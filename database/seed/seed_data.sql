-- ============================================================
-- seed_data.sql — 种子数据（测试用）
-- 说明：插入模拟校园二手交易场景的初始数据，覆盖所有 15 张表
-- 执行：Get-Content 本文件 | docker exec -i campus_trade_db sqlplus CAMPUS/Campus123456@FREEPDB1
--
-- 前提：已执行 001~006 全部 DDL，数据库对象就绪
-- 注意：脚本开头会清空所有表（按 FK 依赖顺序），可重复执行
-- ============================================================
SET SQLBLANKLINES ON
SET LINESIZE 200
SET PAGESIZE 100
SET SERVEROUTPUT ON

PROMPT
PROMPT ╔══════════════════════════════════════════╗
PROMPT ║   校园二手交易系统 — 种子数据插入         ║
PROMPT ╚══════════════════════════════════════════╝
PROMPT

/* =========================
   PART 0: 清理旧数据（按 FK 依赖倒序）
   ========================= */
PROMPT >>> 清理旧数据...

DELETE FROM chat_message;
DELETE FROM chat_session;
DELETE FROM review;
DELETE FROM appointment;
DELETE FROM trade_order;
DELETE FROM bargain_offer;
DELETE FROM favorite;
DELETE FROM goods_image;
DELETE FROM goods;
DELETE FROM report;
DELETE FROM audit_log;
DELETE FROM notice;
DELETE FROM student_auth;
DELETE FROM category;
DELETE FROM app_user;
COMMIT;

PROMPT ✅ 旧数据已清理
PROMPT

/* ================================================================
   PART 1: 用户模块 — 8 个用户 + 5 个学生认证
   ================================================================ */

PROMPT >>> 1.1 插入用户...

-- 管理员
INSERT INTO app_user (user_id, username, password, nickname, phone, email, role, credit_score)
VALUES (1, 'admin', 'pass123', '管理员小刘', '13800000001', 'admin@campus.edu.cn', 'admin', 100);

-- 卖家（有不同信用分）
INSERT INTO app_user (user_id, username, password, nickname, phone, email, role, credit_score)
VALUES (2, 'zhangsan', 'pass123', '张三', '13800000002', 'zhangsan@campus.edu.cn', 'seller', 120);
INSERT INTO app_user (user_id, username, password, nickname, phone, email, role, credit_score)
VALUES (3, 'lisi', 'pass123', '李四', '13800000003', 'lisi@campus.edu.cn', 'seller', 95);
INSERT INTO app_user (user_id, username, password, nickname, phone, email, role, credit_score)
VALUES (4, 'wangwu', 'pass123', '王五', '13800000004', 'wangwu@campus.edu.cn', 'seller', 108);

-- 买家
INSERT INTO app_user (user_id, username, password, nickname, phone, email, role, credit_score)
VALUES (5, 'zhaoliu', 'pass123', '赵六', '13800000005', 'zhaoliu@campus.edu.cn', 'buyer', 100);
INSERT INTO app_user (user_id, username, password, nickname, phone, email, role, credit_score)
VALUES (6, 'sunqi', 'pass123', '孙七', '13800000006', 'sunqi@campus.edu.cn', 'buyer', 80);
INSERT INTO app_user (user_id, username, password, nickname, phone, email, role, credit_score)
VALUES (7, 'zhouba', 'pass123', '周八', '13800000007', 'zhouba@campus.edu.cn', 'buyer', 100);

-- 被封禁用户
INSERT INTO app_user (user_id, username, password, nickname, phone, email, role, status, credit_score)
VALUES (8, 'banned_user', 'pass123', '违规用户', '13800000008', 'bad@spam.com', 'seller', 'banned', 10);

COMMIT;
PROMPT ✅ 8 个用户已插入


PROMPT >>> 1.2 插入学生认证...

-- 已认证
INSERT INTO student_auth (auth_id, user_id, student_id, real_name, college, auth_status)
VALUES (1, 2, '2021001', '张三', '计算机科学与技术学院', 'approved');
INSERT INTO student_auth (auth_id, user_id, student_id, real_name, college, auth_status)
VALUES (2, 3, '2021002', '李四', '软件学院', 'approved');
INSERT INTO student_auth (auth_id, user_id, student_id, real_name, college, auth_status)
VALUES (3, 4, '2021003', '王五', '电子信息工程学院', 'approved');
INSERT INTO student_auth (auth_id, user_id, student_id, real_name, college, auth_status)
VALUES (4, 5, '2022001', '赵六', '数学与统计学院', 'approved');

-- 待审核
INSERT INTO student_auth (auth_id, user_id, student_id, real_name, college, auth_status)
VALUES (5, 6, '2022002', '孙七', '外国语学院', 'pending');

COMMIT;
PROMPT ✅ 5 条学生认证已插入 (4已认证 + 1待审核)


/* ================================================================
   PART 2: 商品模块 — 分类 + 15 件商品 + 图片 + 收藏
   ================================================================ */

PROMPT >>> 2.1 插入商品分类（两级树）...

-- 一级分类
INSERT INTO category (category_id, category_name, parent_id, sort_order) VALUES (1,  '数码产品', NULL, 1);
INSERT INTO category (category_id, category_name, parent_id, sort_order) VALUES (2,  '教材书籍', NULL, 2);
INSERT INTO category (category_id, category_name, parent_id, sort_order) VALUES (3,  '生活用品', NULL, 3);
INSERT INTO category (category_id, category_name, parent_id, sort_order) VALUES (4,  '服饰鞋包', NULL, 4);
INSERT INTO category (category_id, category_name, parent_id, sort_order) VALUES (5,  '其他',     NULL, 99);

-- 二级分类
INSERT INTO category (category_id, category_name, parent_id, sort_order) VALUES (11, '手机',       1, 1);
INSERT INTO category (category_id, category_name, parent_id, sort_order) VALUES (12, '笔记本电脑', 1, 2);
INSERT INTO category (category_id, category_name, parent_id, sort_order) VALUES (13, '平板/配件',  1, 3);
INSERT INTO category (category_id, category_name, parent_id, sort_order) VALUES (21, '专业教材',   2, 1);
INSERT INTO category (category_id, category_name, parent_id, sort_order) VALUES (22, '考研考公',   2, 2);
INSERT INTO category (category_id, category_name, parent_id, sort_order) VALUES (23, '文学小说',   2, 3);
INSERT INTO category (category_id, category_name, parent_id, sort_order) VALUES (31, '宿舍日用',   3, 1);
INSERT INTO category (category_id, category_name, parent_id, sort_order) VALUES (32, '运动健身',   3, 2);

COMMIT;
PROMPT ✅ 13 个分类已插入 (4个一级 + 9个二级)


PROMPT >>> 2.2 插入商品（15件，覆盖各种状态）...

-- ======== 已审核商品 (approved) ========

-- 1. 张三的 iPhone
INSERT INTO goods (goods_id, seller_id, category_id, title, description, price, goods_condition, goods_status, view_count)
VALUES (1, 2, 11, 'iPhone 15 128GB 黑色 99新',
        '去年12月购入，无拆无修，一直带壳贴膜使用。因换了16Pro所以出掉。配件齐全，带原装充电线。',
        4200.00, '几乎全新', 'approved', 256);

-- 2. 李四的 MacBook
INSERT INTO goods (goods_id, seller_id, category_id, title, description, price, goods_condition, goods_status, view_count)
VALUES (2, 3, 12, 'MacBook Air M2 8+256 星光色',
        '2024年3月京东购入，还在保。轻度使用，主要写论文看视频。电池循环45次。箱说全。',
        5500.00, '几乎全新', 'approved', 189);

-- 3. 王五的考研书
INSERT INTO goods (goods_id, seller_id, category_id, title, description, price, goods_condition, goods_status, view_count)
VALUES (3, 4, 22, '2025考研数学全套（张宇+李永乐）',
        '张宇高数18讲+线代9讲+概率9讲，李永乐线代辅导讲义。有少量笔记，不影响使用。',
        45.00, '轻微使用', 'approved', 67);

-- 4. 张三的教材
INSERT INTO goods (goods_id, seller_id, category_id, title, description, price, goods_condition, goods_status, view_count)
VALUES (4, 2, 21, '《数据库系统概念》原书第7版',
        '数据库课程教材，几乎全新，只用了一学期。无笔记无划痕。',
        35.00, '几乎全新', 'approved', 42);

-- 5. 李四的台灯
INSERT INTO goods (goods_id, seller_id, category_id, title, description, price, goods_condition, goods_status, view_count)
VALUES (5, 3, 31, '小米LED护眼台灯 1S',
        '宿舍自用一年，功能完好，亮度可调。换了显示器挂灯故出。',
        49.00, '轻微使用', 'approved', 31);

-- 6. 王五的键盘
INSERT INTO goods (goods_id, seller_id, category_id, title, description, price, goods_condition, goods_status, view_count)
VALUES (6, 4, 13, '罗技 MX Keys 无线键盘',
        '办公利器，键程舒适，充一次电用一个月。带优联接收器。',
        199.00, '轻微使用', 'approved', 88);

-- 7. 张三的耳机
INSERT INTO goods (goods_id, seller_id, category_id, title, description, price, goods_condition, goods_status, view_count)
VALUES (7, 2, 13, 'AirPods Pro 2 代  Lightning版',
        '使用半年，成色很好。耳塞已换新，带AppleCare到明年3月。',
        950.00, '几乎全新', 'approved', 134);

-- 8. 李四的篮球
INSERT INTO goods (goods_id, seller_id, category_id, title, description, price, goods_condition, goods_status, view_count)
VALUES (8, 3, 32, '斯伯丁 TF-500 篮球',
        '打了一个学期，表皮略有磨损但气密性良好。适合练球用。',
        60.00, '明显痕迹', 'approved', 25);

-- 9. 王五的小说
INSERT INTO goods (goods_id, seller_id, category_id, title, description, price, goods_condition, goods_status, view_count)
VALUES (9, 4, 23, '《三体》全集（1-3册）',
        '看了一遍，几乎全新。重庆出版社新版。',
        30.00, '几乎全新', 'approved', 56);

-- 10. 张三的椅子
INSERT INTO goods (goods_id, seller_id, category_id, title, description, price, goods_condition, goods_status, view_count)
VALUES (10, 2, 31, '西昊人体工学椅 M18',
        '用了两年，坐垫有些许磨损。调节功能完好，腰部支撑很舒服。毕业出，限自提。',
        299.00, '明显痕迹', 'approved', 112);

-- ======== 待审核商品 (pending) ========
INSERT INTO goods (goods_id, seller_id, category_id, title, description, price, goods_condition, goods_status, view_count)
VALUES (11, 2, 11, '三星 S24 Ultra 256GB 钛灰色',
        '国行刚买三个月，实在用不惯安卓想出掉。全套都在。',
        6200.00, '几乎全新', 'pending', 45);

INSERT INTO goods (goods_id, seller_id, category_id, title, description, price, goods_condition, goods_status, view_count)
VALUES (12, 3, 12, '联想小新 Pro16 2024款',
        '买来用了两周，觉得屏幕太大不方便携带。几乎全新，发票在。',
        4800.00, '几乎全新', 'pending', 78);

-- ======== 已售出商品 (sold) ========
INSERT INTO goods (goods_id, seller_id, category_id, title, description, price, goods_condition, goods_status, view_count)
VALUES (13, 2, 11, '小米14 白色 12+256',
        '已售出。去年底购入，用了半年换新机了。',
        1800.00, '轻微使用', 'sold', 203);

INSERT INTO goods (goods_id, seller_id, category_id, title, description, price, goods_condition, goods_status, view_count)
VALUES (14, 3, 21, '《算法导论》第三版',
        '已售出。经典算法书，英文版。',
        55.00, '轻微使用', 'sold', 91);

-- ======== 已锁定商品 (locked，有人下单但未完成) ========
INSERT INTO goods (goods_id, seller_id, category_id, title, description, price, goods_condition, goods_status, view_count)
VALUES (15, 4, 32, 'Keep 瑜伽垫 加厚10mm',
        '只用过三次，几乎全新。买来就坚持不下去了…',
        40.00, '几乎全新', 'locked', 37);

COMMIT;
PROMPT ✅ 15 件商品已插入 (10 approved + 2 pending + 2 sold + 1 locked)


PROMPT >>> 2.3 插入商品图片...

-- iPhone 15 (goods_id=1): 3 张图
INSERT INTO goods_image (image_id, goods_id, image_url, sort_order) VALUES (1,  1,  '/images/goods/1/iphone15_front.jpg',  1);
INSERT INTO goods_image (image_id, goods_id, image_url, sort_order) VALUES (2,  1,  '/images/goods/1/iphone15_back.jpg',   2);
INSERT INTO goods_image (image_id, goods_id, image_url, sort_order) VALUES (3,  1,  '/images/goods/1/iphone15_side.jpg',   3);

-- MacBook Air (goods_id=2): 2 张
INSERT INTO goods_image (image_id, goods_id, image_url, sort_order) VALUES (4,  2,  '/images/goods/2/macbook_air_1.jpg', 1);
INSERT INTO goods_image (image_id, goods_id, image_url, sort_order) VALUES (5,  2,  '/images/goods/2/macbook_air_2.jpg', 2);

-- 考研书 (goods_id=3): 1 张
INSERT INTO goods_image (image_id, goods_id, image_url, sort_order) VALUES (6,  3,  '/images/goods/3/kaoyan_books.jpg',  1);

-- 数据库教材 (goods_id=4): 1 张
INSERT INTO goods_image (image_id, goods_id, image_url, sort_order) VALUES (7,  4,  '/images/goods/4/db_book.jpg',       1);

-- 台灯 (goods_id=5): 2 张
INSERT INTO goods_image (image_id, goods_id, image_url, sort_order) VALUES (8,  5,  '/images/goods/5/lamp_1.jpg',        1);
INSERT INTO goods_image (image_id, goods_id, image_url, sort_order) VALUES (9,  5,  '/images/goods/5/lamp_2.jpg',        2);

-- 键盘 (goods_id=6): 2 张
INSERT INTO goods_image (image_id, goods_id, image_url, sort_order) VALUES (10, 6,  '/images/goods/6/mx_keys_1.jpg',    1);
INSERT INTO goods_image (image_id, goods_id, image_url, sort_order) VALUES (11, 6,  '/images/goods/6/mx_keys_2.jpg',    2);

-- AirPods (goods_id=7): 2 张
INSERT INTO goods_image (image_id, goods_id, image_url, sort_order) VALUES (12, 7,  '/images/goods/7/airpods_1.jpg',    1);
INSERT INTO goods_image (image_id, goods_id, image_url, sort_order) VALUES (13, 7,  '/images/goods/7/airpods_2.jpg',    2);

-- 篮球 (goods_id=8): 1 张
INSERT INTO goods_image (image_id, goods_id, image_url, sort_order) VALUES (14, 8,  '/images/goods/8/basketball.jpg',    1);

-- 三体 (goods_id=9): 1 张
INSERT INTO goods_image (image_id, goods_id, image_url, sort_order) VALUES (15, 9,  '/images/goods/9/three_body.jpg',    1);

-- 工学椅 (goods_id=10): 3 张
INSERT INTO goods_image (image_id, goods_id, image_url, sort_order) VALUES (16, 10, '/images/goods/10/chair_front.jpg',  1);
INSERT INTO goods_image (image_id, goods_id, image_url, sort_order) VALUES (17, 10, '/images/goods/10/chair_side.jpg',   2);
INSERT INTO goods_image (image_id, goods_id, image_url, sort_order) VALUES (18, 10, '/images/goods/10/chair_detail.jpg', 3);

-- 小米14 (goods_id=13): 1 张
INSERT INTO goods_image (image_id, goods_id, image_url, sort_order) VALUES (19, 13, '/images/goods/13/mi14.jpg',         1);

-- 算法导论 (goods_id=14): 1 张
INSERT INTO goods_image (image_id, goods_id, image_url, sort_order) VALUES (20, 14, '/images/goods/14/algorithms.jpg',   1);

COMMIT;
PROMPT ✅ 20 张商品图片已插入


PROMPT >>> 2.4 插入收藏...

-- 赵六收藏了 iPhone、MacBook、键盘
INSERT INTO favorite (favorite_id, user_id, goods_id) VALUES (1, 5, 1);
INSERT INTO favorite (favorite_id, user_id, goods_id) VALUES (2, 5, 2);
INSERT INTO favorite (favorite_id, user_id, goods_id) VALUES (3, 5, 6);

-- 孙七收藏了 考研书、教材、三体、台灯
INSERT INTO favorite (favorite_id, user_id, goods_id) VALUES (4, 6, 3);
INSERT INTO favorite (favorite_id, user_id, goods_id) VALUES (5, 6, 4);
INSERT INTO favorite (favorite_id, user_id, goods_id) VALUES (6, 6, 9);
INSERT INTO favorite (favorite_id, user_id, goods_id) VALUES (7, 6, 5);

-- 周八收藏了 iPhone、AirPods、椅子
INSERT INTO favorite (favorite_id, user_id, goods_id) VALUES (8, 7, 1);
INSERT INTO favorite (favorite_id, user_id, goods_id) VALUES (9, 7, 7);
INSERT INTO favorite (favorite_id, user_id, goods_id) VALUES (10, 7, 10);

COMMIT;
PROMPT ✅ 10 条收藏已插入


/* ================================================================
   PART 3: 交易模块 — 议价 + 订单 + 面交预约
   ================================================================ */

PROMPT >>> 3.1 插入议价记录...

-- 赵六对 MacBook 出价 5000
INSERT INTO bargain_offer (offer_id, goods_id, buyer_id, offer_price, seller_response, counter_price, offer_status)
VALUES (1, 2, 5, 5000.00, 'countered', 5300.00, 'active');

-- 孙七对 iPhone 出价 3800
INSERT INTO bargain_offer (offer_id, goods_id, buyer_id, offer_price, seller_response, offer_status)
VALUES (2, 1, 6, 3800.00, 'pending', 'active');

-- 周八对 考研书 出价 35
INSERT INTO bargain_offer (offer_id, goods_id, buyer_id, offer_price, seller_response, offer_status)
VALUES (3, 3, 7, 35.00, 'pending', 'active');

-- 赵六对 AirPods 出价 850（已被接受）
INSERT INTO bargain_offer (offer_id, goods_id, buyer_id, offer_price, seller_response, offer_status)
VALUES (4, 7, 5, 850.00, 'accepted', 'accepted');

-- 孙七对 键盘 出价 150（被拒绝）
INSERT INTO bargain_offer (offer_id, goods_id, buyer_id, offer_price, seller_response, offer_status)
VALUES (5, 6, 6, 150.00, 'rejected', 'rejected');

COMMIT;
PROMPT ✅ 5 条议价已插入 (3 active + 1 accepted + 1 rejected)


PROMPT >>> 3.2 插入订单 + 面交预约...

-- ======== 已完成订单 (completed) ========

-- 订单1: 赵六买 小米14（张三）→ 已完成
INSERT INTO trade_order (order_id, goods_id, buyer_id, seller_id, final_price, order_status)
VALUES (1, 13, 5, 2, 1800.00, 'completed');
INSERT INTO appointment (appointment_id, order_id, meet_time, meet_place, confirm_code, appointment_status)
VALUES (1, 1,
        TO_TIMESTAMP('2026-06-15 14:00:00', 'YYYY-MM-DD HH24:MI:SS'),
        '图书馆一楼大厅', '874291', 'completed');

-- 订单2: 孙七买 算法导论（李四）→ 已完成
INSERT INTO trade_order (order_id, goods_id, buyer_id, seller_id, final_price, order_status)
VALUES (2, 14, 6, 3, 50.00, 'completed');
INSERT INTO appointment (appointment_id, order_id, meet_time, meet_place, confirm_code, appointment_status)
VALUES (2, 2,
        TO_TIMESTAMP('2026-06-18 10:30:00', 'YYYY-MM-DD HH24:MI:SS'),
        '二食堂门口', '315608', 'completed');

-- ======== 待面交订单 (pending_meet) ========

-- 订单3: 周八买 工学椅（张三）→ 待面交
INSERT INTO trade_order (order_id, goods_id, buyer_id, seller_id, final_price, order_status)
VALUES (3, 10, 7, 2, 280.00, 'pending_meet');
INSERT INTO appointment (appointment_id, order_id, meet_time, meet_place, confirm_code, appointment_status)
VALUES (3, 3,
        TO_TIMESTAMP('2026-07-08 16:00:00', 'YYYY-MM-DD HH24:MI:SS'),
        '北区宿舍楼下', '529407', 'confirmed');

-- ======== 面交中订单 (in_meet) ========

-- 订单4: 赵六买 台灯（李四）→ 面交中（通过议价接受的）
INSERT INTO trade_order (order_id, goods_id, buyer_id, seller_id, final_price, order_status)
VALUES (4, 5, 5, 3, 40.00, 'in_meet');
INSERT INTO appointment (appointment_id, order_id, meet_time, meet_place, confirm_code, appointment_status)
VALUES (4, 4,
        TO_TIMESTAMP('2026-07-06 15:00:00', 'YYYY-MM-DD HH24:MI:SS'),
        '教学楼A区一楼', '718033', 'confirmed');

-- ======== 已取消订单 (cancelled) ========

-- 订单5: 孙七买 篮球（李四）→ 已取消
INSERT INTO trade_order (order_id, goods_id, buyer_id, seller_id, final_price, order_status)
VALUES (5, 8, 6, 3, 55.00, 'cancelled');
INSERT INTO appointment (appointment_id, order_id, meet_time, meet_place, confirm_code, appointment_status)
VALUES (5, 5,
        TO_TIMESTAMP('2026-07-03 12:00:00', 'YYYY-MM-DD HH24:MI:SS'),
        '体育馆门口', '246813', 'cancelled');

-- ======== 锁定中的订单（对应 goods_id=15 的 locked 商品） ========
INSERT INTO trade_order (order_id, goods_id, buyer_id, seller_id, final_price, order_status)
VALUES (6, 15, 5, 4, 35.00, 'pending_meet');

COMMIT;
PROMPT ✅ 6 个订单 + 5 个面交预约已插入 (2 completed + 2 pending_meet + 1 in_meet + 1 cancelled)


/* ================================================================
   PART 4: 沟通模块 — 会话 + 消息
   ================================================================ */

PROMPT >>> 4.1 插入聊天会话...

INSERT INTO chat_session (session_id, goods_id, buyer_id, seller_id) VALUES (1, 1, 6, 2);   -- 孙七问张三 iPhone
INSERT INTO chat_session (session_id, goods_id, buyer_id, seller_id) VALUES (2, 2, 5, 3);   -- 赵六问李四 MacBook
INSERT INTO chat_session (session_id, goods_id, buyer_id, seller_id) VALUES (3, 7, 5, 2);   -- 赵六问张三 AirPods
INSERT INTO chat_session (session_id, goods_id, buyer_id, seller_id) VALUES (4, 10, 7, 2);  -- 周八问张三 椅子（下单前沟通）

COMMIT;
PROMPT ✅ 4 个会话已插入


PROMPT >>> 4.2 插入聊天消息...

-- 会话1: 孙七 ←→ 张三 关于 iPhone
INSERT INTO chat_message (message_id, session_id, sender_id, content, is_read, created_at)
VALUES (1, 1, 6, '你好，iPhone还在吗？', 1,
        TO_TIMESTAMP('2026-07-05 09:30:00', 'YYYY-MM-DD HH24:MI:SS'));
INSERT INTO chat_message (message_id, session_id, sender_id, content, is_read, created_at)
VALUES (2, 1, 2, '在的，有什么想了解的？', 0,
        TO_TIMESTAMP('2026-07-05 09:35:00', 'YYYY-MM-DD HH24:MI:SS'));
INSERT INTO chat_message (message_id, session_id, sender_id, content, is_read, created_at)
VALUES (3, 1, 6, '电池健康度多少？可以小刀吗？', 0,
        TO_TIMESTAMP('2026-07-05 09:40:00', 'YYYY-MM-DD HH24:MI:SS'));

-- 会话2: 赵六 ←→ 李四 关于 MacBook
INSERT INTO chat_message (message_id, session_id, sender_id, content, is_read, created_at)
VALUES (4, 2, 5, '学长好，MacBook还在保吗？', 1,
        TO_TIMESTAMP('2026-07-04 20:00:00', 'YYYY-MM-DD HH24:MI:SS'));
INSERT INTO chat_message (message_id, session_id, sender_id, content, is_read, created_at)
VALUES (5, 2, 3, '在保的，到明年3月。电池循环才45次。', 1,
        TO_TIMESTAMP('2026-07-04 20:15:00', 'YYYY-MM-DD HH24:MI:SS'));
INSERT INTO chat_message (message_id, session_id, sender_id, content, is_read, created_at)
VALUES (6, 2, 5, '5000可以吗？诚心要', 0,
        TO_TIMESTAMP('2026-07-04 20:20:00', 'YYYY-MM-DD HH24:MI:SS'));

-- 会话3: 赵六 ←→ 张三 关于 AirPods（已成交）
INSERT INTO chat_message (message_id, session_id, sender_id, content, is_read, created_at)
VALUES (7, 3, 5, 'AirPods 850出吗？', 1,
        TO_TIMESTAMP('2026-07-03 14:00:00', 'YYYY-MM-DD HH24:MI:SS'));
INSERT INTO chat_message (message_id, session_id, sender_id, content, is_read, created_at)
VALUES (8, 3, 2, '行，850成交。你什么时候方便？', 1,
        TO_TIMESTAMP('2026-07-03 14:30:00', 'YYYY-MM-DD HH24:MI:SS'));

-- 会话4: 周八 ←→ 张三 关于椅子
INSERT INTO chat_message (message_id, session_id, sender_id, content, is_read, created_at)
VALUES (9, 4, 7, '椅子还在吗？能再便宜点吗？', 1,
        TO_TIMESTAMP('2026-07-05 18:00:00', 'YYYY-MM-DD HH24:MI:SS'));
INSERT INTO chat_message (message_id, session_id, sender_id, content, is_read, created_at)
VALUES (10, 4, 2, '280可以，自提的话再便宜20。', 0,
        TO_TIMESTAMP('2026-07-05 18:10:00', 'YYYY-MM-DD HH24:MI:SS'));

COMMIT;
PROMPT ✅ 10 条聊天消息已插入 (5已读 + 5未读)


/* ================================================================
   PART 5: 评价模块 — 互评
   ================================================================ */

PROMPT >>> 5.1 插入评价（已完成订单的互评）...

-- 订单1 互评: 赵六(买家) ←→ 张三(卖家)  小米14
INSERT INTO review (review_id, order_id, reviewer_id, reviewed_user_id, rating, content)
VALUES (1, 1, 5, 2, 5, '学长人很好，手机成色和描述完全一致，还送了个手机壳！');
INSERT INTO review (review_id, order_id, reviewer_id, reviewed_user_id, rating, content)
VALUES (2, 1, 2, 5, 5, '买家很爽快，验机很专业，交易愉快！');

-- 订单2 互评: 孙七(买家) ←→ 李四(卖家)  算法导论
INSERT INTO review (review_id, order_id, reviewer_id, reviewed_user_id, rating, content)
VALUES (3, 2, 6, 3, 4, '书况不错，就是有一章有几处划线标注，其他都很好。');
INSERT INTO review (review_id, order_id, reviewer_id, reviewed_user_id, rating, content)
VALUES (4, 2, 3, 6, 5, '买家准时赴约，沟通很顺畅！');

COMMIT;
PROMPT ✅ 4 条评价已插入（2对互评）


/* ================================================================
   PART 6: 管理模块 — 举报 + 审核日志 + 公告
   ================================================================ */

PROMPT >>> 6.1 插入举报...

-- 举报商品（虚假描述）
INSERT INTO report (report_id, reporter_id, report_type, target_goods_id, reason, report_status)
VALUES (1, 5, 'goods', 8,
        '商品描述写"轻微使用"但实际表皮磨损很严重，误导买家。',
        'pending');

-- 举报用户（骚扰）
INSERT INTO report (report_id, reporter_id, report_type, target_user_id, reason, report_status)
VALUES (2, 6, 'user', 8,
        '该用户多次在评论区发布广告链接，骚扰其他用户。',
        'processing');

-- 举报订单（已处理）
INSERT INTO report (report_id, reporter_id, report_type, target_order_id, reason, report_status)
VALUES (3, 7, 'order', 5,
        '卖家无故取消订单，不守信用。',
        'resolved');

COMMIT;
PROMPT ✅ 3 条举报已插入 (1 pending + 1 processing + 1 resolved)


PROMPT >>> 6.2 插入审核日志...

INSERT INTO audit_log (log_id, admin_id, audit_type, target_id, action, result, remark)
VALUES (1, 1, 'goods_audit', 13, 'approved', 'success', '商品信息完整，图片清晰，审核通过');
INSERT INTO audit_log (log_id, admin_id, audit_type, target_id, action, result, remark)
VALUES (2, 1, 'goods_audit', 14, 'approved', 'success', '教材类商品，内容合规');
INSERT INTO audit_log (log_id, admin_id, audit_type, target_id, action, result, remark)
VALUES (3, 1, 'user_ban', 8, 'ban', 'success', '多次发布广告骚扰，经核实执行封禁');
INSERT INTO audit_log (log_id, admin_id, audit_type, target_id, action, result, remark)
VALUES (4, 1, 'report_handle', 3, 'resolved', 'success', '经核实卖家确有原因取消，已对卖家进行提醒');

COMMIT;
PROMPT ✅ 4 条审核日志已插入


PROMPT >>> 6.3 插入系统公告...

INSERT INTO notice (notice_id, title, content, notice_type, publisher_id)
VALUES (1, '校园二手交易平台上线公告',
        '欢迎使用校园二手交易平台！本平台仅供校内师生使用。交易请注意安全，建议选择校内公共场所面交。',
        'system', 1);

INSERT INTO notice (notice_id, title, content, notice_type, publisher_id)
VALUES (2, '关于规范商品发布的提醒',
        '请各位同学在发布商品时上传真实图片，如实描述商品状况。虚假描述一经查实将做封号处理。',
        'transaction', 1);

INSERT INTO notice (notice_id, title, content, notice_type, publisher_id)
VALUES (3, '近期违规账号处理公告',
        '用户"违规用户"（学号未认证）因多次发布校外广告被永久封禁。请大家引以为戒，遵守平台规则。',
        'violation', 1);

COMMIT;
PROMPT ✅ 3 条公告已插入

/* ================================================================
   PART 7: 重置 identity 序列
   说明：种子数据显式插入了各表主键。Oracle identity 序列不会自动追踪
         显式插入的最大 ID，因此这里统一重置到 MAX(id)+1，避免后续
         注册、发布、下单等普通 INSERT 撞主键。
   ================================================================ */

PROMPT >>> 7.1 重置 identity 序列...

ALTER TABLE app_user MODIFY user_id GENERATED BY DEFAULT AS IDENTITY (START WITH LIMIT VALUE);
ALTER TABLE student_auth MODIFY auth_id GENERATED BY DEFAULT AS IDENTITY (START WITH LIMIT VALUE);
ALTER TABLE category MODIFY category_id GENERATED BY DEFAULT AS IDENTITY (START WITH LIMIT VALUE);
ALTER TABLE goods MODIFY goods_id GENERATED BY DEFAULT AS IDENTITY (START WITH LIMIT VALUE);
ALTER TABLE goods_image MODIFY image_id GENERATED BY DEFAULT AS IDENTITY (START WITH LIMIT VALUE);
ALTER TABLE favorite MODIFY favorite_id GENERATED BY DEFAULT AS IDENTITY (START WITH LIMIT VALUE);
ALTER TABLE bargain_offer MODIFY offer_id GENERATED BY DEFAULT AS IDENTITY (START WITH LIMIT VALUE);
ALTER TABLE trade_order MODIFY order_id GENERATED BY DEFAULT AS IDENTITY (START WITH LIMIT VALUE);
ALTER TABLE appointment MODIFY appointment_id GENERATED BY DEFAULT AS IDENTITY (START WITH LIMIT VALUE);
ALTER TABLE chat_session MODIFY session_id GENERATED BY DEFAULT AS IDENTITY (START WITH LIMIT VALUE);
ALTER TABLE chat_message MODIFY message_id GENERATED BY DEFAULT AS IDENTITY (START WITH LIMIT VALUE);
ALTER TABLE review MODIFY review_id GENERATED BY DEFAULT AS IDENTITY (START WITH LIMIT VALUE);
ALTER TABLE report MODIFY report_id GENERATED BY DEFAULT AS IDENTITY (START WITH LIMIT VALUE);
ALTER TABLE audit_log MODIFY log_id GENERATED BY DEFAULT AS IDENTITY (START WITH LIMIT VALUE);
ALTER TABLE notice MODIFY notice_id GENERATED BY DEFAULT AS IDENTITY (START WITH LIMIT VALUE);

PROMPT ✅ identity 序列已重置

/* ================================================================
   PART 8: 数据概览统计
   ================================================================ */

PROMPT
PROMPT ╔══════════════════════════════════════════╗
PROMPT ║   种子数据插入完成！数据概览：             ║
PROMPT ╠══════════════════════════════════════════╣

SELECT 'app_user'      AS table_name, COUNT(*) AS cnt FROM app_user      UNION ALL
SELECT 'student_auth'  AS table_name, COUNT(*) AS cnt FROM student_auth  UNION ALL
SELECT 'category'      AS table_name, COUNT(*) AS cnt FROM category      UNION ALL
SELECT 'goods'         AS table_name, COUNT(*) AS cnt FROM goods         UNION ALL
SELECT 'goods_image'   AS table_name, COUNT(*) AS cnt FROM goods_image   UNION ALL
SELECT 'favorite'      AS table_name, COUNT(*) AS cnt FROM favorite      UNION ALL
SELECT 'bargain_offer' AS table_name, COUNT(*) AS cnt FROM bargain_offer UNION ALL
SELECT 'trade_order'   AS table_name, COUNT(*) AS cnt FROM trade_order   UNION ALL
SELECT 'appointment'   AS table_name, COUNT(*) AS cnt FROM appointment   UNION ALL
SELECT 'chat_session'  AS table_name, COUNT(*) AS cnt FROM chat_session  UNION ALL
SELECT 'chat_message'  AS table_name, COUNT(*) AS cnt FROM chat_message  UNION ALL
SELECT 'review'        AS table_name, COUNT(*) AS cnt FROM review        UNION ALL
SELECT 'report'        AS table_name, COUNT(*) AS cnt FROM report        UNION ALL
SELECT 'audit_log'     AS table_name, COUNT(*) AS cnt FROM audit_log     UNION ALL
SELECT 'notice'        AS table_name, COUNT(*) AS cnt FROM notice;

PROMPT
PROMPT ╔══════════════════════════════════════════╗
PROMPT ║  用户信息速览                             ║
PROMPT ╚══════════════════════════════════════════╝
SELECT user_id, nickname, role, status, credit_score
FROM app_user ORDER BY user_id;

PROMPT
PROMPT ╔══════════════════════════════════════════╗
PROMPT ║  商品状态速览                             ║
PROMPT ╚══════════════════════════════════════════╝
SELECT goods_id, title, goods_status, price
FROM goods ORDER BY goods_id;

PROMPT
PROMPT ✅ 种子数据全部就绪！可以用 DBeaver 或 SQL*Plus 开始测试了。

EXIT;

