# 登录/注册接口改动说明

> 前端模块：登录页 `Login.vue`、注册页 `Register.vue`  
> 目的：注册时绑定学号；登录时支持“用户名或学号”在同一个输入框登录。  
> 说明：本文档用于后端同学调整接口和数据库逻辑。字段命名以当前前端改动为准，如后端已有固定命名，可协商后统一。

---

## 1. 改动概览

本次登录注册模块有两处核心改动：

1. **注册接口新增 `studentId` 字段**  
   用户注册时必须填写学号，后端需要在注册成功后完成“用户账号”和“学号”的绑定。

2. **登录接口由 `username` 改为 `account`**  
   登录页同一个输入框支持输入用户名或学号，因此前端提交字段改为 `account`，后端需要根据 `account` 判断是用户名还是学号，并找到对应用户。

---

## 2. 登录接口改动

### 2.1 接口地址

```http
POST /auth/login
```

如后端实际接口前缀是 `/api/auth/login`，以项目代理配置为准。

### 2.2 请求参数

#### 改动前

```json
{
  "username": "zhangsan",
  "password": "Aa123456!"
}
```

#### 改动后

```json
{
  "account": "zhangsan 或 20230001",
  "password": "Aa123456!"
}
```

### 2.3 字段说明

| 字段 | 类型 | 是否必填 | 说明 |
|---|---|---:|---|
| `account` | string | 是 | 用户名或学号，来自登录页同一个输入框 |
| `password` | string | 是 | 用户密码 |

### 2.4 后端处理逻辑建议

后端登录逻辑需要支持两种查询方式：

```txt
收到 account
  → 先按 users.username 查询用户
  → 如果没查到，再按 student_auth.student_id 查询 user_id
  → 根据 user_id 查询 users 用户信息
  → 校验密码
  → 返回 token 和用户信息
```

伪代码示例：

```ts
const { account, password } = req.body

let user = await userRepository.findByUsername(account)

if (!user) {
  const auth = await studentAuthRepository.findByStudentId(account)
  if (auth) {
    user = await userRepository.findById(auth.userId)
  }
}

if (!user) {
  return loginFailed()
}

const passwordValid = await checkPassword(password, user.password)
if (!passwordValid) {
  return loginFailed()
}

return {
  token,
  user
}
```

### 2.5 登录成功响应

前端仍然期望登录成功后能拿到 token 和用户信息。建议响应结构保持现有项目格式，例如：

```json
{
  "code": 200,
  "message": "登录成功",
  "data": {
    "token": "jwt-token-string",
    "user": {
      "userId": 1,
      "username": "zhangsan",
      "nickname": "张三",
      "avatarUrl": "",
      "phone": "13800138000",
      "email": "zhangsan@example.com",
      "role": "buyer",
      "status": "normal",
      "creditScore": 100,
      "registerTime": "2025-01-01 10:00:00"
    }
  }
}
```

> 注意：`user` 返回结构可以继续不包含 `studentId`。学号仍然可以放在学生认证表或学号绑定表中。

### 2.6 登录失败响应建议

前端会根据错误信息展示提示。建议后端对账号/密码错误返回统一提示，避免泄露账号是否存在：

```json
{
  "code": 401,
  "message": "账号或密码错误"
}
```

不建议返回：

```txt
用户名不存在
学号不存在
```

因为这类提示可能暴露账号是否存在。

---

## 3. 注册接口改动

### 3.1 接口地址

```http
POST /auth/register
```

如后端实际接口前缀是 `/api/auth/register`，以项目代理配置为准。

### 3.2 请求参数

#### 改动前

```json
{
  "username": "zhangsan",
  "password": "Aa123456!",
  "nickname": "张三",
  "phone": "13800138000",
  "email": "zhangsan@example.com"
}
```

#### 改动后

```json
{
  "studentId": "20230001",
  "username": "zhangsan",
  "password": "Aa123456!",
  "nickname": "张三",
  "phone": "13800138000",
  "email": "zhangsan@example.com"
}
```

### 3.3 字段说明

| 字段 | 类型 | 是否必填 | 说明 |
|---|---|---:|---|
| `studentId` | string | 是 | 学号，注册时绑定，用于后续学号登录 |
| `username` | string | 是 | 用户名，建议唯一，且不要允许纯数字，避免和学号冲突 |
| `password` | string | 是 | 密码 |
| `nickname` | string | 否 | 昵称 |
| `phone` | string | 是/按项目约定 | 手机号，目前前端有手机号格式校验 |
| `email` | string | 否 | 邮箱，前端做邮箱格式校验 |

### 3.4 后端处理逻辑建议

注册时后端需要同时处理用户表和学号绑定/学生认证表：

```txt
收到注册请求
  → 校验 username 是否已存在
  → 校验 studentId 是否已绑定
  → 创建 users 用户记录
  → 创建 student_auth / 学号绑定记录
  → 返回注册成功
```

建议注册完成后，创建一条学生认证或学号绑定记录：

```json
{
  "userId": 1,
  "studentId": "20230001",
  "realName": null,
  "college": null,
  "authStatus": "pending 或 unverified"
}
```

如果你们的实名认证逻辑要求用户后续再填写真实姓名和学院，那么注册时可以只绑定 `studentId`，认证状态设置为：

```txt
unverified / pending / 待完善
```

具体枚举值以后端现有设计为准。

---

## 4. 数据库约束建议

为了保证“用户名或学号登录”不产生歧义，建议数据库至少满足这些约束：

### 4.1 `users.username` 唯一

```sql
ALTER TABLE users ADD CONSTRAINT uk_users_username UNIQUE (username);
```

### 4.2 `student_auth.student_id` 唯一

```sql
ALTER TABLE student_auth ADD CONSTRAINT uk_student_auth_student_id UNIQUE (student_id);
```

### 4.3 用户名避免纯数字

因为登录输入框支持用户名或学号，如果用户名允许纯数字，可能出现冲突：

```txt
A 用户 username = 20230001
B 用户 studentId = 20230001
```

建议后端也做一次校验：

```txt
username 不允许为纯数字
```

前端目前也会配合做用户名规则限制。

---

## 5. 前端类型约定

前端建议的类型如下：

```ts
export interface LoginParams {
  account: string
  password: string
}

export interface RegisterParams {
  studentId: string
  username: string
  password: string
  nickname?: string
  phone?: string
  email?: string
}
```

`User` 类型不建议强行加入 `studentId`，除非后端登录返回的用户对象中也包含该字段。

推荐保持：

```txt
User：账号基础信息
StudentAuth：学号、实名信息、学院、认证状态
```

---

## 6. 前端提交的数据示例

### 6.1 用户名登录

```json
{
  "account": "zhangsan",
  "password": "Aa123456!"
}
```

### 6.2 学号登录

```json
{
  "account": "20230001",
  "password": "Aa123456!"
}
```

### 6.3 注册

```json
{
  "studentId": "20230001",
  "username": "zhangsan",
  "password": "Aa123456!",
  "nickname": "张三",
  "phone": "13800138000",
  "email": "zhangsan@example.com"
}
```

---

## 7. 错误信息建议

为了方便前端显示更友好的提示，建议后端返回明确的 `message`。

| 场景 | 建议状态码 | 建议 message |
|---|---:|---|
| 登录账号或密码错误 | 401 | `账号或密码错误` |
| 用户名已存在 | 400 / 409 | `用户名已存在` |
| 学号已绑定 | 400 / 409 | `学号已绑定` |
| 手机号格式错误 | 400 | `手机号格式错误` |
| 邮箱格式错误 | 400 | `邮箱格式错误` |
| 参数缺失 | 400 | `缺少必要参数` |
| 服务器异常 | 500 | `服务器内部错误` |

前端当前会重点识别：

```txt
用户名 + 已存在
学号 + 已存在/已绑定
账号/用户名/学号/密码错误
网络错误/timeout
```

---

## 8. 需要后端确认的问题

请后端同学确认以下几点：

1. 登录接口是否可以把请求字段从 `username` 改为 `account`？
2. 如果暂时不能改字段名，是否接受前端继续传 `username`，但语义上代表“用户名或学号”？
3. 注册接口是否可以新增 `studentId` 字段？
4. 注册时 `studentId` 是直接写入 `student_auth` 表，还是单独建学号绑定表？
5. 注册后学生认证状态应该是 `unverified`、`pending`，还是其他枚举？
6. 登录成功响应中的 `user` 是否需要返回 `studentId`？目前前端不强制需要。

---

## 9. 前后端联调检查清单

联调时请依次测试：

- [ ] 使用用户名可以登录成功
- [ ] 使用学号可以登录成功
- [ ] 密码错误时返回 `账号或密码错误`
- [ ] 注册时必须提交 `studentId`
- [ ] 重复用户名注册时返回 `用户名已存在`
- [ ] 重复学号注册时返回 `学号已绑定` 或 `学号已存在`
- [ ] 注册成功后，数据库中同时有用户记录和学号绑定/认证记录
- [ ] 用户名不能和学号产生登录歧义
- [ ] 前端 `npm run build` 通过

---

## 10. 本次前端影响文件

本次前端主要涉及：

```txt
src/views/Login.vue
src/views/Register.vue
src/api/auth.ts
src/stores/user.ts
src/types/index.ts
```

核心变化：

```txt
Login.vue：username 输入框改为 account，文案改为“用户名 / 学号”
Register.vue：新增 studentId 字段
api/auth.ts：LoginParams 使用 account；RegisterParams 新增 studentId
stores/user.ts：login 方法第一个参数语义由 username 改为 account
types/index.ts：新增或调整 LoginParams、RegisterParams
```
