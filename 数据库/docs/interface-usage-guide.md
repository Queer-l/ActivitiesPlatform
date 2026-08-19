# 校园活动管理系统接口使用文档

本文档面向前端 WinForm 成员与后端 Web API 成员，用于统一接口调用方式、字段命名、权限规则和联调口径。

## 1. 基础约定

| 项目 | 说明 |
| --- | --- |
| 接口风格 | HTTP RESTful |
| 数据格式 | JSON |
| 默认地址 | `http://localhost:5000/api` |
| 字符编码 | UTF-8 |
| 时间格式 | `yyyy-MM-ddTHH:mm:ss`，例如 `2026-06-20T08:00:00` |
| 登录方式 | JWT Bearer Token |

除登录、注册、公开活动列表和活动详情外，其余接口建议都携带 Token：

```http
Authorization: Bearer <token>
Content-Type: application/json
```

## 2. 统一响应格式

后端所有接口建议统一返回以下结构，前端只需要判断 `code` 是否为 `200`：

```json
{
  "code": 200,
  "message": "ok",
  "data": {}
}
```

失败示例：

```json
{
  "code": 400,
  "message": "报名截止已过",
  "data": null
}
```

常用状态码约定：

| code | 含义 | 前端处理建议 |
| --- | --- | --- |
| 200 | 成功 | 正常渲染 `data` |
| 400 | 业务校验失败 | 直接弹出 `message` |
| 401 | 未登录或 Token 过期 | 清除本地 Token，跳转登录页 |
| 403 | 无权限 | 提示“当前账号无权限操作” |
| 404 | 资源不存在 | 提示数据不存在或已被删除 |
| 500 | 服务端异常 | 提示系统繁忙，并记录错误信息 |

## 3. 角色与权限

| 角色 | roleId | 权限说明 |
| --- | --- | --- |
| 管理员 | 1 | 活动管理、报名名单、签到管理、统计、用户与黑名单管理 |
| 学生 | 2 | 浏览活动、报名、取消报名、查看个人报名和签到记录 |

前端登录后应保存 `token`、`userId`、`roleId`、`roleName`，根据 `roleId` 控制页面入口。后端仍必须做权限校验，不能只依赖前端隐藏按钮。

## 4. 枚举值

| 字段 | 值 | 含义 |
| --- | --- | --- |
| `activity.status` | 0 | 未开始 |
| `activity.status` | 1 | 进行中 |
| `activity.status` | 2 | 已结束 |
| `registration.status` | 0 | 已报名 |
| `registration.status` | 1 | 已取消 |
| `checkIn.checkInType` | 0 | 扫码签到 |
| `checkIn.checkInType` | 1 | 手动补签 |
| `blacklist.activityId` | `null` | 全局拉黑 |
| `blacklist.activityId` | 数字 | 仅针对指定活动拉黑 |

## 5. 认证模块 `/api/auth`

### 5.1 登录

```http
POST /api/auth/login
```

请求体：

```json
{
  "username": "admin",
  "password": "Admin@123456"
}
```

响应 `data`：

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

联调账号：

| 用户名 | 密码 | 角色 |
| --- | --- | --- |
| `admin` | `Admin@123456` | 管理员 |
| `student01` | `Student@123` | 学生 |
| `student02` | `Student@123` | 学生 |

### 5.2 学生注册

```http
POST /api/auth/register
```

请求体：

```json
{
  "username": "zhangsan",
  "password": "MyPass@123",
  "realName": "张三",
  "email": "zhangsan@campus.edu.cn",
  "phone": "13900001111"
}
```

说明：

注册用户默认角色为学生，`roleId = 2`。

## 6. 活动模块 `/api/activities`

### 6.1 活动列表

```http
GET /api/activities?category=学术&status=0&keyword=创新&page=1&pageSize=10
```

查询参数：

| 参数 | 必填 | 说明 |
| --- | --- | --- |
| `category` | 否 | 活动类别，如 `学术`、`文体`、`社团` |
| `status` | 否 | 活动状态：0 未开始，1 进行中，2 已结束 |
| `keyword` | 否 | 标题或描述关键词 |
| `page` | 否 | 页码，默认 1 |
| `pageSize` | 否 | 每页数量，默认 10 |

响应 `data`：

```json
{
  "total": 25,
  "page": 1,
  "pageSize": 10,
  "list": [
    {
      "activityId": 1,
      "title": "2026年校园科技创新大赛",
      "category": "学术",
      "location": "综合楼A座101报告厅",
      "maxParticipants": 200,
      "registeredCount": 56,
      "registrationDeadline": "2026-06-15T23:59:59",
      "signInStartTime": "2026-06-20T08:00:00",
      "signInEndTime": "2026-06-20T18:00:00",
      "status": 0,
      "coverImagePath": "/uploads/activities/cover-tech-2026.jpg",
      "createdAt": "2026-05-08T10:00:00"
    }
  ]
}
```

### 6.2 活动详情

```http
GET /api/activities/{activityId}
```

响应 `data`：

```json
{
  "activityId": 1,
  "title": "2026年校园科技创新大赛",
  "description": "面向全校学生的科技创新竞赛...",
  "category": "学术",
  "location": "综合楼A座101报告厅",
  "maxParticipants": 200,
  "registeredCount": 56,
  "registrationDeadline": "2026-06-15T23:59:59",
  "signInStartTime": "2026-06-20T08:00:00",
  "signInEndTime": "2026-06-20T18:00:00",
  "status": 0,
  "coverImagePath": "/uploads/activities/cover-tech-2026.jpg",
  "creatorName": "系统管理员",
  "createdAt": "2026-05-08T10:00:00",
  "updatedAt": "2026-05-08T10:00:00"
}
```

### 6.3 创建活动

管理员接口。

```http
POST /api/activities
```

请求体：

```json
{
  "title": "校园科技创新大赛",
  "description": "面向全校学生...",
  "category": "学术",
  "location": "综合楼A座101报告厅",
  "maxParticipants": 200,
  "registrationDeadline": "2026-06-15T23:59:59",
  "signInStartTime": "2026-06-20T08:00:00",
  "signInEndTime": "2026-06-20T18:00:00",
  "coverImagePath": "/uploads/activities/cover-tech-2026.jpg"
}
```

后端校验：

- `title`、`category`、`location`、`registrationDeadline` 必填。
- `maxParticipants >= 0`，其中 0 可表示不限制人数。
- 如果签到开始和结束时间都存在，则 `signInStartTime < signInEndTime`。

### 6.4 更新活动

管理员接口。

```http
PUT /api/activities/{activityId}
```

请求体字段同创建活动。

### 6.5 删除活动

管理员接口。

```http
DELETE /api/activities/{activityId}
```

说明：

删除活动会级联删除该活动的报名和签到记录。前端删除前应二次确认。

## 7. 报名模块 `/api/registrations`

### 7.1 报名活动

学生接口。

```http
POST /api/registrations
```

请求体：

```json
{
  "activityId": 1
}
```

后端需要校验：

- 当前用户不是管理员操作学生报名接口。
- 活动存在且未删除。
- 报名截止时间未过。
- 活动未满员。
- 用户没有对该活动重复报名。
- 用户不在全局黑名单或该活动黑名单中。

常见失败：

```json
{ "code": 400, "message": "您已在黑名单中，无法报名", "data": null }
```

```json
{ "code": 400, "message": "活动已满员", "data": null }
```

```json
{ "code": 400, "message": "报名已截止", "data": null }
```

```json
{ "code": 400, "message": "您已报名过该活动", "data": null }
```

### 7.2 我的报名列表

学生接口。

```http
GET /api/registrations/my?status=0&page=1&pageSize=10
```

响应 `data`：

```json
{
  "total": 5,
  "page": 1,
  "pageSize": 10,
  "list": [
    {
      "registrationId": 10,
      "activityId": 1,
      "title": "2026年校园科技创新大赛",
      "category": "学术",
      "location": "综合楼A座101报告厅",
      "registrationTime": "2026-05-10T14:30:00",
      "status": 0,
      "hasCheckedIn": false
    }
  ]
}
```

### 7.3 取消报名

学生接口。

```http
PUT /api/registrations/{registrationId}/cancel
```

建议后端只允许取消当前登录学生自己的报名记录。

### 7.4 活动报名名单

管理员接口。

```http
GET /api/registrations/activity/{activityId}?page=1&pageSize=20
```

建议响应 `data`：

```json
{
  "total": 120,
  "page": 1,
  "pageSize": 20,
  "list": [
    {
      "registrationId": 1,
      "userId": 3,
      "username": "student01",
      "realName": "测试学生张三",
      "email": "student01@campus.edu.cn",
      "phone": "13900000001",
      "registrationTime": "2026-05-10T14:30:00",
      "status": 0,
      "hasCheckedIn": true,
      "checkInTime": "2026-06-20T09:15:00"
    }
  ]
}
```

## 8. 签到模块 `/api/checkins`

### 8.1 扫码签到

管理员扫码后调用，管理员接口。

```http
POST /api/checkins/scan
```

请求体：

```json
{
  "userId": 3,
  "activityId": 1,
  "qrCode": "activity:1:2026"
}
```

后端需要校验：

- 用户已报名该活动，且报名状态为已报名。
- 当前时间在签到时间范围内。
- 同一用户同一活动未重复签到。
- 二维码内容与活动匹配。

常见失败：

```json
{ "code": 400, "message": "该用户未报名此活动", "data": null }
```

```json
{ "code": 400, "message": "已签到，请勿重复签到", "data": null }
```

```json
{ "code": 400, "message": "不在签到时间范围内", "data": null }
```

### 8.2 手动补签

管理员接口。

```http
POST /api/checkins/manual
```

请求体：

```json
{
  "userId": 3,
  "activityId": 1,
  "remark": "学生迟到，管理员手动补签"
}
```

说明：

手动补签可允许 `registrationId` 为空，但仍需要防止同一用户同一活动重复签到。

### 8.3 我的签到记录

学生接口。

```http
GET /api/checkins/my?page=1&pageSize=10
```

响应 `data`：

```json
{
  "total": 3,
  "page": 1,
  "pageSize": 10,
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

### 8.4 活动签到统计

管理员接口。

```http
GET /api/checkins/stats/{activityId}
```

响应 `data`：

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

## 9. 统计模块 `/api/statistics`

以下均为管理员接口。

### 9.1 总览数据

```http
GET /api/statistics/overview
```

响应 `data`：

```json
{
  "totalActivities": 45,
  "totalRegistrations": 1520,
  "todayActiveUsers": 86,
  "avgCheckInRate": 76.5
}
```

### 9.2 近 30 天报名趋势

```http
GET /api/statistics/registration-trend
```

响应 `data`：

```json
[
  { "date": "2026-05-01", "count": 12 },
  { "date": "2026-05-02", "count": 18 }
]
```

### 9.3 活动类别分布

```http
GET /api/statistics/category-distribution
```

响应 `data`：

```json
[
  { "category": "学术", "count": 15 },
  { "category": "文体", "count": 20 },
  { "category": "社团", "count": 10 }
]
```

## 10. 用户与黑名单模块

### 10.1 用户列表

管理员接口。

```http
GET /api/users?roleId=2&keyword=张三&page=1&pageSize=10
```

建议响应 `data`：

```json
{
  "total": 2,
  "page": 1,
  "pageSize": 10,
  "list": [
    {
      "userId": 3,
      "username": "student01",
      "realName": "测试学生张三",
      "email": "student01@campus.edu.cn",
      "phone": "13900000001",
      "roleId": 2,
      "roleName": "学生",
      "isActive": true,
      "createdAt": "2026-05-08T10:00:00"
    }
  ]
}
```

### 10.2 拉黑用户

管理员接口。

```http
POST /api/blacklist
```

请求体：

```json
{
  "userId": 3,
  "activityId": null,
  "reason": "多次无故缺席活动"
}
```

说明：

`activityId = null` 表示全局拉黑；传入活动 ID 表示只限制该活动。

### 10.3 解除拉黑

管理员接口。

```http
PUT /api/blacklist/{blacklistId}/revoke
```

后端建议将 `IsActive` 改为 `false`，保留历史记录用于审计。

## 11. WinForm 调用示例

### 11.1 通用响应类

```csharp
public class ApiResponse<T>
{
    public int Code { get; set; }
    public string Message { get; set; }
    public T Data { get; set; }
}

public class LoginData
{
    public string Token { get; set; }
    public int UserId { get; set; }
    public string Username { get; set; }
    public string RealName { get; set; }
    public int RoleId { get; set; }
    public string RoleName { get; set; }
}
```

### 11.2 登录并保存 Token

```csharp
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;

var client = new HttpClient();
client.BaseAddress = new Uri("http://localhost:5000/api/");

var body = new
{
    username = "admin",
    password = "Admin@123456"
};

var content = new StringContent(
    JsonConvert.SerializeObject(body),
    Encoding.UTF8,
    "application/json");

var response = await client.PostAsync("auth/login", content);
var json = await response.Content.ReadAsStringAsync();
var result = JsonConvert.DeserializeObject<ApiResponse<LoginData>>(json);

if (result.Code == 200)
{
    client.DefaultRequestHeaders.Authorization =
        new AuthenticationHeaderValue("Bearer", result.Data.Token);
}
else
{
    MessageBox.Show(result.Message);
}
```

### 11.3 查询活动列表

```csharp
var response = await client.GetAsync("activities?category=学术&page=1&pageSize=10");
var json = await response.Content.ReadAsStringAsync();
```

### 11.4 发送 POST 请求

```csharp
var body = new { activityId = 1 };
var content = new StringContent(
    JsonConvert.SerializeObject(body),
    Encoding.UTF8,
    "application/json");

var response = await client.PostAsync("registrations", content);
var json = await response.Content.ReadAsStringAsync();
```

## 12. 前后端联调清单

后端成员：

- 保持接口路径、HTTP 方法和 JSON 字段名与本文档一致。
- 统一返回 `code`、`message`、`data`。
- 所有需要登录的接口校验 JWT。
- 管理员接口校验 `roleId = 1`，学生接口校验当前登录用户身份。
- 分页接口统一返回 `total`、`page`、`pageSize`、`list`。
- 业务失败尽量返回明确 `message`，方便前端直接展示。

前端成员：

- 登录成功后保存 Token，后续请求统一带 `Authorization`。
- 所有接口先判断 `code`，失败时弹出 `message`。
- 根据 `roleId` 控制学生端和管理员端入口。
- 时间字段统一按 ISO 格式解析和显示。
- 删除、取消报名、拉黑、补签等操作前做二次确认。

## 13. 建议开发优先级

| 优先级 | 接口 |
| --- | --- |
| P0 | 登录、注册、活动列表、活动详情、创建活动、报名活动、我的报名、扫码签到、我的签到 |
| P1 | 更新活动、删除活动、取消报名、活动报名名单、手动补签、签到统计、用户列表、拉黑 |
| P2 | 解除拉黑、总览统计、报名趋势、类别分布 |

