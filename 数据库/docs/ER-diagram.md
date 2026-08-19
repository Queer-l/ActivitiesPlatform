# 校园活动管理系统 - 数据库E-R图

## 实体关系图

```mermaid
erDiagram
    Roles ||--o{ Users : "1:N 拥有"
    Users ||--o{ Activities : "1:N 创建"
    Users ||--o{ Registrations : "1:N 报名"
    Users ||--o{ CheckIns : "1:N 签到"
    Users ||--o{ Blacklist : "1:N 被拉黑"
    Users ||--o{ Blacklist : "1:N 操作拉黑"
    Activities ||--o{ Registrations : "1:N 包含"
    Activities ||--o{ CheckIns : "1:N 包含"
    Registrations ||--o{ CheckIns : "1:1 对应"

    Roles {
        int RoleId PK "角色ID，主键，自增"
        nvarchar RoleName "角色名称（学生/管理员）"
        nvarchar Description "角色描述"
        datetime CreatedAt "创建时间"
    }

    Users {
        int UserId PK "用户ID，主键，自增"
        nvarchar Username "用户名，唯一"
        nvarchar PasswordHash "密码哈希值"
        nvarchar RealName "真实姓名"
        nvarchar Email "邮箱，唯一"
        nvarchar Phone "手机号"
        int RoleId FK "角色ID，外键→Roles"
        bit IsActive "是否激活"
        datetime CreatedAt "注册时间"
    }

    Activities {
        int ActivityId PK "活动ID，主键，自增"
        nvarchar Title "活动标题"
        nvarchar Description "活动描述"
        nvarchar CoverImagePath "封面图路径"
        nvarchar Category "活动类别（学术/文体/社团）"
        nvarchar Location "活动地点"
        int MaxParticipants "最大参与人数"
        datetime RegistrationDeadline "报名截止时间"
        datetime SignInStartTime "签到开始时间"
        datetime SignInEndTime "签到结束时间"
        int Status "状态（0=未开始 1=进行中 2=已结束）"
        int CreatorUserId FK "创建者ID，外键→Users"
        datetime CreatedAt "创建时间"
        datetime UpdatedAt "更新时间"
    }

    Registrations {
        int RegistrationId PK "报名ID，主键，自增"
        int UserId FK "用户ID，外键→Users"
        int ActivityId FK "活动ID，外键→Activities"
        datetime RegistrationTime "报名时间"
        int Status "状态（0=已报名 1=已取消）"
    }

    CheckIns {
        int CheckInId PK "签到ID，主键，自增"
        int RegistrationId FK "报名ID，外键→Registrations"
        int UserId FK "用户ID，外键→Users"
        int ActivityId FK "活动ID，外键→Activities"
        datetime CheckInTime "签到时间"
        int CheckInType "签到方式（0=扫码签到 1=手动补签）"
        nvarchar Remark "备注"
    }

    Blacklist {
        int BlacklistId PK "黑名单ID，主键，自增"
        int UserId FK "用户ID，外键→Users"
        int ActivityId FK "活动ID，外键→Activities（可空：全局拉黑）"
        nvarchar Reason "拉黑原因"
        datetime BlockedAt "拉黑时间"
        int BlockedByUserId FK "操作人ID，外键→Users"
        bit IsActive "是否生效"
    }
```

## 关系说明

| 父表 | 子表 | 关系 | 说明 |
|------|------|------|------|
| Roles | Users | 1:N | 一个角色下有多个用户 |
| Users | Activities | 1:N | 一个管理员可创建多个活动 |
| Users | Registrations | 1:N | 一个用户可报名多个活动 |
| Users | CheckIns | 1:N | 一个用户可多次签到 |
| Users | Blacklist | 1:N | 一个用户可被拉黑多次 |
| Activities | Registrations | 1:N | 一个活动可被多人报名 |
| Activities | CheckIns | 1:N | 一个活动关联多条签到记录 |
| Registrations | CheckIns | 1:1 | 一条报名记录对应一次签到 |

## 设计要点

1. **报名唯一约束**：Users + Activities 联合唯一索引，防止一人重复报名同一活动。
2. **签到关联报名**：CheckIn 表通过 RegistrationId 关联报名记录，并通过 Users + Activities 联合唯一约束防止同一活动重复签到。
3. **黑名单灵活性**：ActivityId 可为 NULL，支持全局拉黑（所有活动）或针对特定活动拉黑。
4. **状态字段**：Activities.Status 由 SQL Server 定时作业或后端服务根据时间自动更新。
5. **密码安全**：PasswordHash 存储 BCrypt/SHA256 哈希值，不存储明文密码。
