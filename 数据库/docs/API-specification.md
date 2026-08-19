# 校园活动管理系统 - API 接口文档

## 基础信息

| 项目 | 说明 |
|------|------|
| 协议 | HTTP RESTful |
| 格式 | JSON |
| 认证 | JWT Bearer Token（Header: `Authorization: Bearer <token>`） |
| 基础URL | `http://localhost:5000/api` |

## 统一响应格式

```json
// 成功
{ "code": 200, "message": "ok", "data": { ... } }

// 失败
{ "code": 400, "message": "报名截止已过", "data": null }

// 认证失败
{ "code": 401, "message": "未登录或token已过期", "data": null }
```

---

## 一、认证模块 `/api/auth`

### 1. 登录

```
POST /api/auth/login
```

**请求体：**
```json
{
    "username": "admin",
    "password": "Admin@123456"
}
```

**响应 data：**
```json
{
    "token": "eyJhbGciOiJIUzI1...",
    "userId": 1,
    "username": "admin",
    "realName": "系统管理员",
    "roleId": 1,
    "roleName": "管理员"
}
```

### 2. 注册（仅限学生）

```
POST /api/auth/register
```

```json
{
    "username": "zhangsan",
    "password": "MyPass@123",
    "realName": "张三",
    "email": "zhangsan@campus.edu.cn",
    "phone": "13900001111"
}
```

---

## 二、活动模块 `/api/activities`

### 3. 活动列表（分页 + 筛选）

```
GET /api/activities?category=学术&status=0&keyword=创新&page=1&pageSize=10
```

**响应 data：**
```json
{
    "total": 25,
    "page": 1,
    "list": [
        {
            "activityId": 1,
            "title": "2026年校园科技创新大赛",
            "category": "学术",
            "location": "综合楼A座301报告厅",
            "maxParticipants": 200,
            "registeredCount": 56,
            "registrationDeadline": "2026-06-15T23:59:59",
            "signInStartTime": "2026-06-20T08:00:00",
            "signInEndTime": "2026-06-20T18:00:00",
            "status": 0,
            "coverImagePath": "/uploads/cover1.jpg",
            "createdAt": "2026-05-08T10:00:00"
        }
    ]
}
```

### 4. 活动详情

```
GET /api/activities/{activityId}
```

```json
{
    "activityId": 1,
    "title": "2026年校园科技创新大赛",
    "description": "面向全校学生...",
    "category": "学术",
    "location": "综合楼A座301报告厅",
    "maxParticipants": 200,
    "registeredCount": 56,
    "registrationDeadline": "2026-06-15T23:59:59",
    "signInStartTime": "2026-06-20T08:00:00",
    "signInEndTime": "2026-06-20T18:00:00",
    "status": 0,
    "coverImagePath": "/uploads/cover1.jpg",
    "creatorName": "系统管理员",
    "createdAt": "2026-05-08T10:00:00"
}
```

### 5. 创建活动（管理员）

```
POST /api/activities
```

```json
{
    "title": "校园科技创新大赛",
    "description": "面向全校学生...",
    "category": "学术",
    "location": "综合楼A座301报告厅",
    "maxParticipants": 200,
    "registrationDeadline": "2026-06-15T23:59:59",
    "signInStartTime": "2026-06-20T08:00:00",
    "signInEndTime": "2026-06-20T18:00:00",
    "coverImageFile": "<base64 或使用 multipart/form-data>"
}
```

### 6. 更新活动（管理员）

```
PUT /api/activities/{activityId}
```

### 7. 删除活动（管理员）

```
DELETE /api/activities/{activityId}
```

---

## 三、报名模块 `/api/registrations`

### 8. 报名活动（学生）

```
POST /api/registrations
```

```json
{
    "activityId": 1
}
```

**错误响应示例：**
```json
{ "code": 400, "message": "您已在黑名单中，无法报名" }
{ "code": 400, "message": "活动已满员" }
{ "code": 400, "message": "报名已截止" }
{ "code": 400, "message": "您已报名过该活动" }
```

### 9. 我的报名列表（学生）

```
GET /api/registrations/my?status=0&page=1&pageSize=10
```

```json
{
    "total": 5,
    "list": [
        {
            "registrationId": 10,
            "activityId": 1,
            "title": "2026年校园科技创新大赛",
            "category": "学术",
            "location": "综合楼A座301报告厅",
            "registrationTime": "2026-05-10T14:30:00",
            "status": 0,
            "hasCheckedIn": false
        }
    ]
}
```

### 10. 取消报名（学生）

```
PUT /api/registrations/{registrationId}/cancel
```

### 11. 某活动的报名名单（管理员）

```
GET /api/registrations/activity/{activityId}?page=1&pageSize=20
```

---

## 四、签到模块 `/api/checkins`

### 12. 扫码签到（管理员扫码后调用）

```
POST /api/checkins/scan
```

```json
{
    "userId": 3,
    "activityId": 1,
    "qrCode": "活动二维码内容"
}
```

**错误响应：**
```json
{ "code": 400, "message": "该用户未报名此活动" }
{ "code": 400, "message": "已签到，请勿重复签到" }
{ "code": 400, "message": "不在签到时间范围内" }
```

### 13. 手动补签（管理员）

```
POST /api/checkins/manual
```

```json
{
    "userId": 3,
    "activityId": 1,
    "remark": "学生迟到，管理员手动补签"
}
```

### 14. 我的签到记录（学生）

```
GET /api/checkins/my?page=1&pageSize=10
```

```json
{
    "total": 3,
    "list": [
        {
            "checkInId": 5,
            "activityId": 1,
            "title": "2026年校园科技创新大赛",
            "checkInTime": "2026-06-20T09:15:00",
            "checkInType": 0,
            "remark": null
        }
    ]
}
```

### 15. 活动签到统计（管理员）

```
GET /api/checkins/stats/{activityId}
```

```json
{
    "activityId": 1,
    "title": "2026年校园科技创新大赛",
    "registeredCount": 120,
    "checkedInCount": 95,
    "checkInRate": 79.17,
    "scanCount": 90,
    "manualCount": 5
}
```

---

## 五、统计模块 `/api/statistics`（管理员）

### 16. 总览数据

```
GET /api/statistics/overview
```

```json
{
    "totalActivities": 45,
    "totalRegistrations": 1520,
    "todayActiveUsers": 86,
    "avgCheckInRate": 76.5
}
```

### 17. 报名趋势（近30天）

```
GET /api/statistics/registration-trend
```

```json
[
    { "date": "2026-05-01", "count": 12 },
    { "date": "2026-05-02", "count": 18 }
]
```

### 18. 活动类别分布

```
GET /api/statistics/category-distribution
```

```json
[
    { "category": "学术", "count": 15 },
    { "category": "文体", "count": 20 },
    { "category": "社团", "count": 10 }
]
```

---

## 六、用户管理 `/api/users`（管理员）

### 19. 用户列表

```
GET /api/users?roleId=2&keyword=张三&page=1&pageSize=10
```

### 20. 拉黑用户

```
POST /api/blacklist
```

```json
{
    "userId": 3,
    "activityId": null,
    "reason": "多次无故缺席活动"
}
```

### 21. 解除拉黑

```
PUT /api/blacklist/{blacklistId}/revoke
```

---

## 前端调用示例（WinForm 用 HttpClient）

```csharp
// 登录
var client = new HttpClient();
var body = new { username = "admin", password = "Admin@123456" };
var json = JsonConvert.SerializeObject(body);
var content = new StringContent(json, Encoding.UTF8, "application/json");

var response = await client.PostAsync("http://localhost:5000/api/auth/login", content);
var result = JsonConvert.DeserializeObject<ApiResponse<LoginData>>(
    await response.Content.ReadAsStringAsync());

// 保存 token，之后所有请求都带上
client.DefaultRequestHeaders.Authorization =
    new AuthenticationHeaderValue("Bearer", result.Data.Token);

// 查活动列表
var actResult = await client.GetAsync(
    "http://localhost:5000/api/activities?category=学术&page=1&pageSize=10");
```

---

## 后端开发清单（成员 A）

| # | 接口 | 方法 | 说明 | 优先级 |
|---|------|------|------|--------|
| 1 | `/api/auth/login` | POST | JWT 登录 | P0 |
| 2 | `/api/auth/register` | POST | 学生注册 | P0 |
| 3 | `/api/activities` | GET | 活动列表 | P0 |
| 4 | `/api/activities/{id}` | GET | 活动详情 | P0 |
| 5 | `/api/activities` | POST | 创建活动 | P0 |
| 6 | `/api/activities/{id}` | PUT | 更新活动 | P1 |
| 7 | `/api/activities/{id}` | DELETE | 删除活动 | P1 |
| 8 | `/api/registrations` | POST | 报名活动 | P0 |
| 9 | `/api/registrations/my` | GET | 我的报名 | P0 |
| 10 | `/api/registrations/{id}/cancel` | PUT | 取消报名 | P1 |
| 11 | `/api/registrations/activity/{id}` | GET | 活动报名名单 | P1 |
| 12 | `/api/checkins/scan` | POST | 扫码签到 | P0 |
| 13 | `/api/checkins/manual` | POST | 手动补签 | P1 |
| 14 | `/api/checkins/my` | GET | 我的签到 | P0 |
| 15 | `/api/checkins/stats/{id}` | GET | 签到统计 | P1 |
| 16 | `/api/statistics/overview` | GET | 总览数据 | P1 |
| 17 | `/api/statistics/registration-trend` | GET | 报名趋势 | P2 |
| 18 | `/api/statistics/category-distribution` | GET | 类别分布 | P2 |
| 19 | `/api/users` | GET | 用户列表 | P1 |
| 20 | `/api/blacklist` | POST | 拉黑用户 | P1 |
| 21 | `/api/blacklist/{id}/revoke` | PUT | 解除拉黑 | P2 |
