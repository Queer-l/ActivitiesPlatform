# 校园活动管理系统 - 数据字典

## 1. Roles（角色表）

| 字段名 | 数据类型 | 允许空 | 默认值 | 说明 |
|--------|----------|--------|--------|------|
| RoleId | INT | NOT NULL | IDENTITY(1,1) | 角色ID，主键，自增 |
| RoleName | NVARCHAR(50) | NOT NULL | - | 角色名称，唯一约束 |
| Description | NVARCHAR(200) | NULL | - | 角色描述 |
| CreatedAt | DATETIME | NOT NULL | GETDATE() | 创建时间 |

**种子数据：**
- (1, 管理员, 系统管理员)
- (2, 学生, 普通学生用户)

## 2. Users（用户表）

| 字段名 | 数据类型 | 允许空 | 默认值 | 说明 |
|--------|----------|--------|--------|------|
| UserId | INT | NOT NULL | IDENTITY(1,1) | 用户ID，主键，自增 |
| Username | NVARCHAR(50) | NOT NULL | - | 用户名，唯一约束 |
| PasswordHash | NVARCHAR(256) | NOT NULL | - | 密码哈希值（SHA256） |
| RealName | NVARCHAR(50) | NOT NULL | - | 真实姓名 |
| Email | NVARCHAR(100) | NULL | - | 邮箱，唯一约束，格式校验 |
| Phone | NVARCHAR(20) | NULL | - | 手机号 |
| RoleId | INT | NOT NULL | 2 | 角色ID，外键→Roles |
| IsActive | BIT | NOT NULL | 1 | 是否激活（1=激活 0=禁用） |
| CreatedAt | DATETIME | NOT NULL | GETDATE() | 注册时间 |

**约束：**
- UNIQUE(Username), UNIQUE(Email)
- CHECK(Email LIKE '%_@_%._%')

## 3. Activities（活动表）

| 字段名 | 数据类型 | 允许空 | 默认值 | 说明 |
|--------|----------|--------|--------|------|
| ActivityId | INT | NOT NULL | IDENTITY(1,1) | 活动ID，主键 |
| Title | NVARCHAR(200) | NOT NULL | - | 活动标题 |
| Description | NVARCHAR(MAX) | NULL | - | 活动描述 |
| CoverImagePath | NVARCHAR(500) | NULL | - | 封面图存储路径 |
| Category | NVARCHAR(50) | NOT NULL | '其他' | 活动类别 |
| Location | NVARCHAR(200) | NOT NULL | - | 活动地点 |
| MaxParticipants | INT | NOT NULL | 0 | 最大参与人数 |
| RegistrationDeadline | DATETIME | NOT NULL | - | 报名截止时间 |
| SignInStartTime | DATETIME | NULL | - | 签到开始时间 |
| SignInEndTime | DATETIME | NULL | - | 签到结束时间 |
| Status | TINYINT | NOT NULL | 0 | 0=未开始 1=进行中 2=已结束 |
| CreatorUserId | INT | NOT NULL | - | 创建者ID，外键→Users |
| CreatedAt | DATETIME | NOT NULL | GETDATE() | 创建时间 |
| UpdatedAt | DATETIME | NOT NULL | GETDATE() | 更新时间 |

**约束：**
- CHECK(MaxParticipants >= 0)
- CHECK(SignInStartTime < SignInEndTime)

## 4. Registrations（报名表）

| 字段名 | 数据类型 | 允许空 | 默认值 | 说明 |
|--------|----------|--------|--------|------|
| RegistrationId | INT | NOT NULL | IDENTITY(1,1) | 报名ID，主键 |
| UserId | INT | NOT NULL | - | 用户ID，外键→Users |
| ActivityId | INT | NOT NULL | - | 活动ID，外键→Activities |
| RegistrationTime | DATETIME | NOT NULL | GETDATE() | 报名时间 |
| Status | TINYINT | NOT NULL | 0 | 0=已报名 1=已取消 |

**约束：**
- UNIQUE(UserId, ActivityId) — 一人一活动只能报名一次
- FK_Registrations_Activities ON DELETE CASCADE

## 5. CheckIns（签到表）

| 字段名 | 数据类型 | 允许空 | 默认值 | 说明 |
|--------|----------|--------|--------|------|
| CheckInId | INT | NOT NULL | IDENTITY(1,1) | 签到ID，主键 |
| RegistrationId | INT | NULL | - | 报名ID，外键→Registrations（补签时可为空） |
| UserId | INT | NOT NULL | - | 用户ID，外键→Users |
| ActivityId | INT | NOT NULL | - | 活动ID，外键→Activities |
| CheckInTime | DATETIME | NOT NULL | GETDATE() | 签到时间 |
| CheckInType | TINYINT | NOT NULL | 0 | 0=扫码签到 1=手动补签 |
| Remark | NVARCHAR(500) | NULL | - | 备注（补签原因等） |

**关联逻辑：**
- RegistrationId 关联报名记录，扫码签到通过该字段确保“先报名后签到”
- 手工补签时 RegistrationId 可为 NULL（管理员权限操作）
- UNIQUE(UserId, ActivityId) 防止同一用户在同一活动重复签到

## 6. Blacklist（黑名单表）

| 字段名 | 数据类型 | 允许空 | 默认值 | 说明 |
|--------|----------|--------|--------|------|
| BlacklistId | INT | NOT NULL | IDENTITY(1,1) | 黑名单ID，主键 |
| UserId | INT | NOT NULL | - | 被拉黑用户ID，外键→Users |
| ActivityId | INT | NULL | - | 活动ID（NULL=全局拉黑），外键→Activities |
| Reason | NVARCHAR(500) | NOT NULL | - | 拉黑原因 |
| BlockedAt | DATETIME | NOT NULL | GETDATE() | 拉黑时间 |
| BlockedByUserId | INT | NOT NULL | - | 操作人ID，外键→Users |
| IsActive | BIT | NOT NULL | 1 | 是否生效（可解除） |
