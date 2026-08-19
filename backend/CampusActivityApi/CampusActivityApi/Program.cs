using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models;
using CampusActivityApi.Models;
using Microsoft.Extensions.Options;
using CampusActivityApi.Data;
using Microsoft.EntityFrameworkCore;
using CampusActivityApi.Services;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Data.SqlClient;



// 创建 WebApplication 构建器，后续所有服务注册和中间件配置都基于该对象完成。
var builder = WebApplication.CreateBuilder(args);

// 清理默认日志提供程序后重新启用控制台和 Debug 输出，便于开发阶段查看接口和 EF 日志。
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// 注册 EF Core 数据库上下文，连接串从 appsettings.json 的 ConnectionStrings:DefaultConnection 读取。
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);


// 注册跨域策略。课设/开发环境允许任意来源、请求头和 HTTP 方法访问后端接口。
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()    // 允许所有前端地址访问（开发环境直接用这个）
              .AllowAnyHeader()   // 允许所有请求头
              .AllowAnyMethod();  // 允许 GET/POST 等所有请求方法
    });
});


// 注册控制器，并关闭 JSON 默认 camelCase 命名策略，保持返回字段与 C# 属性名一致。
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // 强制后端返回大驼峰（和前端C#模型完全匹配）
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
    });

// 注册业务服务为 Scoped 生命周期：每个请求使用独立服务实例和 DbContext。
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<ActivityService>();
builder.Services.AddScoped<EnrollService>();
builder.Services.AddScoped<SignInService>();
// 使用临时数据保护密钥，避免开发环境 DPAPI 密钥无法解密导致启动噪声影响接口调试。
builder.Services.AddDataProtection()
    .UseEphemeralDataProtectionProvider();

// 注册 JWT Bearer 认证，控制器上的 [Authorize] 会使用该配置验证 Authorization 请求头。
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            // 课设环境下关闭 Issuer/Audience 验证，减少本地配置成本。
            ValidateIssuer = false,
            ValidateAudience = false,
            // 验证 Token 是否过期，过期 Token 不允许访问授权接口。
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            // 密钥必须与 JwtHelper.SecretKey 保持一致，否则生成的 Token 无法通过验证。
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("12345678901234567890123456789012"))
        };
    });



// 注册 Swagger/OpenAPI 生成器，便于通过浏览器调试接口。
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    // 在 Swagger 页面提供 Bearer Token 输入框。
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "请输入：Bearer {你的Token}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    // 为所有接口挂载安全要求，Swagger 调试授权接口时会自动携带 Token。
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });

    // 禁用 Message 列表的复杂示例生成，避免 Swagger 生成空导航对象造成误解。
   c.MapType(typeof(List<Message>), () => new Microsoft.OpenApi.Models.OpenApiSchema { Type = "array" });
});

// 构建 WebApplication，完成服务容器初始化。
var app = builder.Build();

// 启动时执行轻量数据库初始化：确保数据库存在、修复已知约束问题、补充默认管理员账号。
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    // EnsureCreated 适合课设/演示环境；正式项目建议改用 EF Core Migration 管理结构变更。
    db.Database.EnsureCreated();
    try
    {
        // 兼容旧库中 CK_Activity_AuditStatus 只允许 1 的错误约束。
        // 正确业务需要允许 0=待审核、1=通过、2=驳回。
        db.Database.ExecuteSqlRaw("""
            IF OBJECT_ID(N'dbo.Activity', N'U') IS NOT NULL
               AND OBJECT_ID(N'dbo.CK_Activity_AuditStatus', N'C') IS NOT NULL
            BEGIN
                ALTER TABLE [dbo].[Activity] DROP CONSTRAINT [CK_Activity_AuditStatus];
            END

            IF OBJECT_ID(N'dbo.Activity', N'U') IS NOT NULL
               AND OBJECT_ID(N'dbo.CK_Activity_AuditStatus', N'C') IS NULL
            BEGIN
                ALTER TABLE [dbo].[Activity]
                ADD CONSTRAINT [CK_Activity_AuditStatus] CHECK ([AuditStatus] IN (0, 1, 2));
            END
            """);
    }
    catch (SqlException ex) when (ex.Number == 1088 || ex.Number == 229 || ex.Number == 3728)
    {
        // 当前应用账号可能没有 ALTER TABLE 权限，记录警告但不阻断后端启动。
        app.Logger.LogWarning(ex, "无法自动修复 Activity.AuditStatus 约束，请使用数据库管理员账号执行数据库修复脚本。");
    }

    // 首次启动时自动创建超级管理员账号，保证系统可进入后台管理。
    if (!db.Users.Any(u => u.UserName == "admin"))
    {
        db.Users.Add(new User
        {
            UserName = "admin",
            PassWord = "123456",
            RealName = "系统管理员",
            Role = 3,
            Email = "1093432569@qq.com",
            IsActive = true
        });
        db.SaveChanges();
    }
}

// 开发环境启用 Swagger 页面，便于本地查看和测试接口。
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 启用路由匹配，为后续 CORS、认证授权和控制器端点分发提供路由信息。
app.UseRouting();

// 应用跨域策略，允许前端开发服务器访问后端接口。
app.UseCors("AllowAll");

// 认证必须放在授权之前：先解析 Token 得到用户身份，再执行权限判断。
app.UseAuthentication();

app.UseAuthorization();
// 映射控制器路由，开放 api/[controller]/[action] 等接口。
app.MapControllers();

// 启动 Web API 服务并开始监听端口。
app.Run();
