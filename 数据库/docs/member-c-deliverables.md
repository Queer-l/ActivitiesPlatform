# 成员 C 数据库交付说明

## 交付范围

成员 C 负责数据库结构、数据初始化、查询性能、导出、备份恢复与 QA 验证。本目录已整理为可直接执行和答辩展示的数据库交付物。

## 文件清单

| 文件 | 作用 |
|------|------|
| `sql/01-create-tables.sql` | 创建 `CampusActivityDB` 与 6 张核心表，包含主键、外键、唯一约束和检查约束 |
| `sql/02-create-indexes.sql` | 创建高频查询索引，覆盖活动列表、报名名单、签到统计、黑名单校验等场景 |
| `sql/03-seed-data.sql` | 初始化角色、管理员、测试学生、示例活动和联调数据 |
| `sql/04-export-registration-list.sql` | 报名名单导出查询，可供后端、WinForm 或 `sqlcmd` 导出 CSV |
| `sql/05-backup-restore.sql` | 完整备份、备份校验与恢复示例 |
| `sql/06-member-c-qa-checks.sql` | 表、约束、索引、数据量和业务一致性检查 |
| `docs/ER-diagram.md` | E-R 图和关系说明 |
| `docs/data-dictionary.md` | 数据字典 |

## 封面图片存储方案

活动封面不直接存入数据库，数据库只保存相对路径 `Activities.CoverImagePath`，例如：

```text
/uploads/activities/cover-tech-2026.jpg
```

后端负责将图片保存到服务器静态文件目录或对象存储，并把最终可访问路径写入 `CoverImagePath`。这样可以避免数据库膨胀，也方便 WinForm 端直接按接口返回路径加载图片。

## 执行顺序

1. 执行 `sql/01-create-tables.sql`
2. 执行 `sql/02-create-indexes.sql`
3. 执行 `sql/03-seed-data.sql`
4. 执行 `sql/06-member-c-qa-checks.sql` 验证结果

需要导出报名名单时执行 `sql/04-export-registration-list.sql`，需要备份时执行 `sql/05-backup-restore.sql`。

## 默认账号

| 用户名 | 密码 | 角色 |
|--------|------|------|
| `admin` | `Admin@123456` | 管理员 |
| `student01` | `Student@123` | 学生 |
| `student02` | `Student@123` | 学生 |

正式部署前必须修改默认密码，并考虑迁移到 ASP.NET Core Identity 的 PBKDF2 哈希格式。
