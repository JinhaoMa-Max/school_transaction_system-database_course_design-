# 校园二手交易系统 - ER 图说明

## 整体 ER 图（文字版）

```
┌─────────────────────┐       ┌─────────────────────┐
│      app_user       │       │   student_auth      │
│─────────────────────│       │─────────────────────│
│ PK  user_id         │──1:1──│ PK  auth_id         │
│     username        │       │ FK  user_id (UNIQUE)│
│     password        │       │     student_id      │
│     nickname        │       │     real_name       │
│     role            │       │     college         │
│     status          │       │     auth_status     │
│     credit_score    │       │     auth_time       │
│     created_at      │       │     created_at      │
│     updated_at      │       └─────────────────────┘
└──────┬──────────────┘
       │
       │ 1:N (seller)
       │
┌──────┴──────────────┐       ┌─────────────────────┐
│       goods         │       │    goods_image      │
│─────────────────────│       │─────────────────────│
│ PK  goods_id        │──1:N──│ PK  image_id        │
│ FK  seller_id       │       │ FK  goods_id        │
│ FK  category_id     │       │     image_url       │
│     title           │       │     sort_order      │
│     description     │       └─────────────────────┘
│     price           │
│     goods_condition │       ┌─────────────────────┐
│     goods_status    │       │     favorite        │
│     view_count      │──1:N──│─────────────────────│
│     created_at      │       │ PK  favorite_id     │
│     updated_at      │       │ FK  user_id         │
└──────┬──────────────┘       │ FK  goods_id        │
       │                      │ UNIQUE(user,goods)  │
       │ 1:N (buyer)          └─────────────────────┘
       │
┌──────┴──────────────┐       ┌─────────────────────┐
│   bargain_offer     │       │    trade_order      │
│─────────────────────│       │─────────────────────│
│ PK  offer_id        │       │ PK  order_id        │
│ FK  goods_id        │       │ FK  goods_id        │
│ FK  buyer_id        │       │ FK  buyer_id        │
│     offer_price     │       │ FK  seller_id       │
│     seller_response │       │     final_price     │
│     counter_price   │       │     order_status    │
│     offer_status    │       │     created_at      │
│     created_at      │       │     updated_at      │
│     updated_at      │       └────────┬────────────┘
└─────────────────────┘                │
                                       │ 1:1
                              ┌────────┴────────────┐
                              │    appointment      │
                              │─────────────────────│
                              │ PK  appointment_id  │
                              │ FK  order_id(UNIQUE)│
                              │     meet_time       │
                              │     meet_place      │
                              │     confirm_code    │
                              │     appt_status     │
                              └─────────────────────┘

┌─────────────────────┐       ┌─────────────────────┐
│   chat_session      │       │   chat_message      │
│─────────────────────│──1:N──│─────────────────────│
│ PK  session_id      │       │ PK  message_id      │
│ FK  goods_id        │       │ FK  session_id      │
│ FK  buyer_id        │       │ FK  sender_id       │
│ FK  seller_id       │       │     content (CLOB)  │
│ UNIQUE(goods,buyer, │       │     is_read         │
│        seller)      │       │     created_at      │
└─────────────────────┘       └─────────────────────┘

┌─────────────────────┐       ┌─────────────────────┐
│      review         │       │      report         │
│─────────────────────│       │─────────────────────│
│ PK  review_id       │       │ PK  report_id       │
│ FK  order_id        │       │ FK  reporter_id     │
│ FK  reviewer_id     │       │     report_type     │
│ FK  reviewed_user_id│       │ FK  target_goods_id │
│     rating (1-5)    │       │ FK  target_user_id  │
│     content (CLOB)  │       │ FK  target_order_id │
│ UNIQUE(order,review)│       │     reason (CLOB)   │
└─────────────────────┘       │     report_status   │
                              │     created_at      │
┌─────────────────────┐       └─────────────────────┘
│    audit_log        │
│─────────────────────│       ┌─────────────────────┐
│ PK  log_id          │       │      notice         │
│ FK  admin_id        │       │─────────────────────│
│     audit_type      │       │ PK  notice_id       │
│     target_id       │       │     title           │
│     action          │       │     content (CLOB)  │
│     result          │       │     notice_type     │
│     remark          │       │ FK  publisher_id    │
│     created_at      │       │     created_at      │
└─────────────────────┘       │     updated_at      │
                              └─────────────────────┘
```

## 模块关系总结

```
用户模块 ─────────────────────────────┐
  ├─ app_user (1) ── (1) student_auth │
  ├─ app_user (1) ── (N) goods (seller)│
  └─ app_user (1) ── (N) trade_order   │
                                       │
商品模块 ─────────────────────────────┤
  ├─ category (1) ── (N) goods        │
  ├─ goods (1) ── (N) goods_image     │
  ├─ goods (1) ── (N) favorite        │
  └─ goods (1) ── (N) bargain_offer   │
                                       │  ← 这些就是
交易模块 ─────────────────────────────┤    "闭环"
  ├─ bargain_offer ── trade_order     │     的体现
  ├─ trade_order (1) ── (1) appointment│
  ├─ trade_order (1) ── (N) review    │
  └─ review → credit_score (触发器)   │
                                       │
沟通模块 ─────────────────────────────┤
  ├─ goods ── chat_session            │
  └─ chat_session ── chat_message     │
                                       │
管理模块 ─────────────────────────────┤
  ├─ report → audit_log               │
  └─ notice                           │
                                       │
              ↻ 闭环回路 ↻
  评价 → 信用分更新 → 影响后续交易
```

## 什么是"闭环"—— 数据库层面的体现

这个系统的"闭环"在数据库设计上体现在：

1. **交易闭环**：发布→议价→下单→面交→评价→信用分更新
2. **数据闭环**：`review` 的评价通过触发器影响 `app_user.credit_score`
3. **安全闭环**：`report` 被处理 → `audit_log` 记录 → 违规用户/商品被封禁
4. **沟通闭环**：围绕同一件商品的买卖双方，只能有一个 `chat_session`
