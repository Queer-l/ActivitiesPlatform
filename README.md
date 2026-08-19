# 校园活动管理平台

一个面向校园活动全流程管理的 Web 平台，涵盖用户认证、活动发布与审核、活动报名、签到、留言及后台用户管理等功能。

## 功能概览

- 用户名或邮箱登录、邮箱绑定与密码重置
- 活动申请、审核、下架和列表查询
- 学生活动报名与取消报名
- 签到码生成、扫码签到和签到记录查询
- 活动留言管理
- 用户管理与密码重置申请审核
- 基于 JWT 的身份认证和角色权限控制
- Swagger API 在线调试

## 技术栈

| 模块 | 技术 |
| --- | --- |
| 前端 | Blazor WebAssembly、.NET 9、Bootstrap |
| 后端 | ASP.NET Core Web API、.NET 8 |
| 数据访问 | Entity Framework Core 8 |
| 数据库 | SQL Server |
| 认证 | JWT Bearer Token |
| 接口文档 | Swagger / OpenAPI |

## 项目结构

```text
ActivitiesPlatform/
├─ frontend/CampusActivityManagement/   # Blazor WebAssembly 前端
├─ backend/CampusActivityApi/
│  └─ CampusActivityApi/                # ASP.NET Core Web API
├─ 数据库/
│  ├─ sql/                              # 建表、索引、种子数据及维护脚本
│  └─ docs/                             # ER 图、数据字典和接口说明
└─ README.md
```

## 环境要求

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)（可同时构建 .NET 8 后端）
- SQL Server 2019 或更高版本
- 可选：SQL Server Management Studio 或 `sqlcmd`

## 快速开始

### 1. 克隆项目

```bash
git clone https://github.com/Queer-l/ActivitiesPlatform.git
cd ActivitiesPlatform
```

### 2. 初始化数据库

在 SSMS 中启用 **Query → SQLCMD Mode**，打开并执行：

```text
数据库/sql/00-run-all.sql
```

也可以在项目根目录使用 Windows 身份认证执行：

```powershell
Set-Location 数据库
sqlcmd -S localhost -E -C -f 65001 -i sql\00-run-all.sql
Set-Location ..
```

该入口脚本会按顺序创建 `CampusActivityDB`、修复约束、创建索引、写入演示数据并执行检查。

### 3. 配置后端数据库连接

编辑 `backend/CampusActivityApi/CampusActivityApi/appsettings.json`，将 `ConnectionStrings:DefaultConnection` 修改为自己的 SQL Server 连接串，例如：

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=CampusActivityDB;Trusted_Connection=True;TrustServerCertificate=True"
  }
}
```

不要将生产数据库密码提交到版本库。

### 4. 启动后端

```powershell
dotnet run --project backend\CampusActivityApi\CampusActivityApi\CampusActivityApi.csproj
```

- API 地址：`http://localhost:5196`
- Swagger：`http://localhost:5196/swagger`

后端首次启动时会确保数据库存在，并在缺少管理员时创建开发账号 `admin / 123456`。如果已经执行种子脚本，则管理员账号为 `admin / Admin@123456`。

### 5. 启动前端

另开一个终端执行：

```powershell
dotnet run --project frontend\CampusActivityManagement\CampusActivityManagement.csproj
```

浏览器访问 `http://localhost:5041`。前端默认请求 `http://localhost:5196/api/`；如后端地址有变化，请修改 `frontend/CampusActivityManagement/wwwroot/appsettings.json`。

## 演示账号

执行数据库种子脚本后可使用以下账号：

| 用户名 | 密码 | 角色 |
| --- | --- | --- |
| `admin` | `Admin@123456` | 超级管理员 |
| `student01` | `Student@123` | 学生 |
| `student02` | `Student@123` | 学生 |

这些账号仅用于本地开发和课程演示，部署前请修改默认密码，并将数据库凭据和 JWT 密钥迁移到环境变量或安全配置服务。

## 常用命令

```powershell
# 构建后端
dotnet build backend\CampusActivityApi\CampusActivityApi\CampusActivityApi.csproj

# 构建前端
dotnet build frontend\CampusActivityManagement\CampusActivityManagement.csproj
```

## 相关文档

- [API 接口说明](数据库/docs/API-specification.md)
- [数据字典](数据库/docs/data-dictionary.md)
- [ER 图](数据库/docs/ER-diagram.md)
- [数据库交付说明](数据库/docs/member-c-deliverables.md)

## 开发说明

- 当前 CORS 策略允许任意来源，仅适用于开发环境。
- 当前示例账号密码以明文形式用于课程演示，不应直接用于生产环境。
- 正式部署时应更换 JWT 签名密钥、限制 CORS 来源、使用 HTTPS，并通过 EF Core Migration 管理数据库结构。

## License

本项目暂未声明开源许可证。
