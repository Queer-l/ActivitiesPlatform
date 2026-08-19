using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Components.Authorization;
using CampusActivityManagement;
using CampusActivityManagement.Services;

// Blazor WebAssembly 应用入口：创建宿主、注册根组件和前端依赖服务。
var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// 后端 API 客户端。BaseUrl 来自 wwwroot/appsettings.json，便于切换后端端口或部署地址。
builder.Services.AddHttpClient("ApiClient", client =>
{
    var apiBaseUrl = builder.Configuration["Api:BaseUrl"];
    if (string.IsNullOrWhiteSpace(apiBaseUrl))
    {
        throw new InvalidOperationException("Missing configuration value: Api:BaseUrl");
    }

    client.BaseAddress = new Uri(apiBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
});

// 前端授权、API 封装和本地活动缓存都使用 Scoped 生命周期，符合 Blazor WebAssembly 单用户会话模型。
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();
builder.Services.AddScoped<ApiService>();
builder.Services.AddScoped<ActivityClientCache>();
builder.Services.AddScoped<EnrollmentClientCache>();

await builder.Build().RunAsync();
