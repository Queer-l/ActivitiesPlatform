# 项目代码结构说明

本文档用于说明校园活动管理前端项目中各个文件夹和主要代码文件负责的功能，方便后续维护、答辩或交接。

## 根目录文件

| 文件 | 功能 |
| --- | --- |
| `CampusActivityManagement.sln` | Visual Studio 解决方案文件，用于组织项目。 |
| `CampusActivityManagement.csproj` | Blazor 项目文件，定义目标框架、依赖包和构建配置。 |
| `Program.cs` | 应用启动入口，负责注册 HttpClient、认证授权服务、业务服务和 Blazor WebAssembly 根组件。 |
| `App.razor` | 应用路由入口，配置页面路由、登录授权路由和默认布局。 |
| `_Imports.razor` | Razor 全局 using 文件，让页面和组件可以直接使用常用命名空间。 |
| `ReadMe.md` | 项目说明文档。 |
| `code.md` | 当前代码结构说明文档。 |
| `frontend.out.log` / `frontend.err.log` | 本地运行前端时产生的输出日志和错误日志。 |

## 文件夹职责

| 文件夹 | 功能 |
| --- | --- |
| `Layout` | 存放页面整体布局和导航菜单，控制应用的公共外壳。 |
| `Models` | 存放前后端传输使用的数据模型 DTO，例如活动、用户、登录结果、留言、签到记录等。 |
| `Pages` | 存放具体页面，按业务模块拆分为活动、认证、管理员、签到等页面。 |
| `Services` | 存放前端服务类，负责调用后端 API、维护登录状态、读写浏览器本地缓存。 |
| `Shared` | 存放可复用 UI 组件，例如错误提示、成功提示、加载遮罩、删除按钮和登录跳转组件。 |
| `wwwroot` | 存放静态资源和前端配置，例如 CSS、Bootstrap、appsettings 配置和示例数据。 |
| `Properties` | 存放项目启动配置，例如本地调试端口和运行环境设置。 |
| `bin` | 编译输出目录，由 .NET 构建生成，不属于手写业务代码。 |
| `obj` | 中间构建目录，由 .NET 构建生成，不属于手写业务代码。 |
| `.vs` | Visual Studio 本地缓存目录，不属于业务代码。 |

## Layout 文件夹

| 文件 | 功能 |
| --- | --- |
| `Layout/MainLayout.razor` | 应用主布局，包含顶部/侧边导航区域和页面主体内容。 |
| `Layout/MainLayout.razor.css` | 主布局对应的 scoped CSS，负责布局样式。 |
| `Layout/NavMenu.razor` | 默认导航菜单组件，若启用可用于集中维护页面入口。 |

## Models 文件夹

| 文件 | 功能 |
| --- | --- |
| `Models/ActivityDto.cs` | 活动数据模型，活动列表、详情、审核列表都会使用，包含标题、内容、状态、人数和时间等字段。 |
| `Models/ActivityApplyRequest.cs` | 活动申请请求模型，提交活动申请时发送给后端。 |
| `Models/ApiResponse.cs` | 后端统一响应包装模型，用于解析 `code`、`msg`、`data` 格式的接口返回。 |
| `Models/EnrollDto.cs` | 报名记录模型，预留给报名名单或后续报名展示接口使用。 |
| `Models/LoginResult.cs` | 登录成功后的用户信息和 Token 相关模型，认证状态恢复依赖它。 |
| `Models/MessageDto.cs` | 活动留言模型，详情页留言列表使用。 |
| `Models/PwdResetApplyDto.cs` | 密码重置申请模型，管理员密码重置审核页面使用。 |
| `Models/SignInRecordDto.cs` | 签到记录模型，签到名单页面使用。 |
| `Models/UserDto.cs` | 用户数据模型，管理员用户管理页面使用。 |

## Pages 文件夹

### Pages/Activity

| 文件 | 功能 |
| --- | --- |
| `ActivityList.razor` | 活动列表页，展示后端返回的可见活动，并提供进入活动详情和申请活动的入口。 |
| `ActivityDetail.razor` | 活动详情页，负责展示活动信息、报名/取消报名、签到、留言和撤销活动等操作。 |
| `ActivityApply.razor` | 活动申请页，填写活动标题、内容、人数和时间等信息，并提交审核。 |
| `MyApplies.razor` | 我的活动页，展示当前用户发布或申请过的活动，并合并后端列表、审核列表和本地缓存状态。 |

### Pages/Auth

| 文件 | 功能 |
| --- | --- |
| `Login.razor` | 账号密码登录页面。 |
| `LoginByEmail.razor` | 邮箱验证码登录页面。 |
| `BindEmail.razor` | 绑定邮箱页面，用于给当前账号绑定邮箱。 |
| `ResetPassword.razor` | 通过邮箱验证码重置密码页面。 |
| `ApplyResetPwd.razor` | 人工密码重置申请页面，用户提交申请后等待管理员审核。 |

### Pages/Admin

| 文件 | 功能 |
| --- | --- |
| `UserManagement.razor` | 管理员用户管理页面，负责查看用户、添加用户和编辑用户信息。 |
| `PwdResetAudit.razor` | 密码重置申请审核页面，管理员处理用户提交的重置申请。 |
| `ActivityAudit.razor` | 活动申请审核页面，管理员通过或驳回活动申请。 |

### Pages/SignIn

| 文件 | 功能 |
| --- | --- |
| `GenerateCode.razor` | 生成签到码页面，活动发布者可生成活动签到码。 |
| `ScanSignIn.razor` | 扫码/输入签到码页面，参与者用签到码完成签到。 |
| `SignInList.razor` | 签到名单页面，展示指定活动的签到记录。 |

### Pages 根页面

| 文件 | 功能 |
| --- | --- |
| `Home.razor` | 首页或默认入口页面。 |

## Services 文件夹

| 文件 | 功能 |
| --- | --- |
| `Services/ApiService.cs` | 前端访问后端 API 的统一入口，负责 Token 恢复、请求发送、响应解析和错误提示转换。 |
| `Services/CustomAuthStateProvider.cs` | Blazor 认证状态提供器，负责从 localStorage 恢复登录状态、构建用户 Claims、登录和退出通知。 |
| `Services/ActivityClientCache.cs` | 我的活动本地缓存服务，用 localStorage 保存活动申请草稿、待审核记录和已撤销状态。 |
| `Services/EnrollmentClientCache.cs` | 报名状态本地缓存服务，用 localStorage 保存当前用户是否已报名某活动。 |

## Shared 文件夹

| 文件 | 功能 |
| --- | --- |
| `Shared/ErrorMessage.razor` | 错误提示组件，用于展示接口错误或表单错误。 |
| `Shared/SuccessMessage.razor` | 成功提示组件，用于展示操作成功反馈。 |
| `Shared/LoadingOverlay.razor` | 加载遮罩组件，用于请求处理中提示用户等待。 |
| `Shared/DeleteButton.razor` | 留言删除按钮组件，封装删除留言逻辑和删除后的回调刷新。 |
| `Shared/RedirectToLogin.razor` | 登录跳转组件，用于未登录访问受保护页面时跳转到登录页。 |

## wwwroot 文件夹

| 文件或文件夹 | 功能 |
| --- | --- |
| `wwwroot/appsettings.json` | 前端配置文件，通常用于配置后端 API 地址等运行参数。 |
| `wwwroot/css/app.css` | 全局样式文件，定义页面、按钮、表格、卡片、提示框等通用样式。 |
| `wwwroot/lib/bootstrap` | Bootstrap 静态样式库，用于基础 UI 样式支持。 |
| `wwwroot/sample-data/weather.json` | Blazor 模板示例数据，当前业务中通常不作为核心功能使用。 |

## Properties 文件夹

| 文件 | 功能 |
| --- | --- |
| `Properties/launchSettings.json` | 本地开发启动配置，定义运行 URL、端口和环境变量。 |

## 主要功能模块对应代码

| 功能模块 | 主要文件 |
| --- | --- |
| 登录与授权 | `Pages/Auth/*`、`Services/CustomAuthStateProvider.cs`、`Models/LoginResult.cs` |
| 用户管理 | `Pages/Admin/UserManagement.razor`、`Models/UserDto.cs`、`Services/ApiService.cs` |
| 密码重置 | `Pages/Auth/ResetPassword.razor`、`Pages/Auth/ApplyResetPwd.razor`、`Pages/Admin/PwdResetAudit.razor`、`Models/PwdResetApplyDto.cs` |
| 活动浏览 | `Pages/Activity/ActivityList.razor`、`Pages/Activity/ActivityDetail.razor`、`Models/ActivityDto.cs` |
| 活动申请与审核 | `Pages/Activity/ActivityApply.razor`、`Pages/Activity/MyApplies.razor`、`Pages/Admin/ActivityAudit.razor`、`Models/ActivityApplyRequest.cs` |
| 报名与取消报名 | `Pages/Activity/ActivityDetail.razor`、`Services/EnrollmentClientCache.cs`、`Services/ApiService.cs` |
| 留言功能 | `Pages/Activity/ActivityDetail.razor`、`Shared/DeleteButton.razor`、`Models/MessageDto.cs` |
| 签到功能 | `Pages/SignIn/*`、`Pages/Activity/ActivityDetail.razor`、`Models/SignInRecordDto.cs` |
| 公共 UI 反馈 | `Shared/ErrorMessage.razor`、`Shared/SuccessMessage.razor`、`Shared/LoadingOverlay.razor` |

