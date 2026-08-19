using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using System.Security.Claims;
using System.Text.Json;
using System.Text;
using CampusActivityManagement.Models;

namespace CampusActivityManagement.Services;

/// <summary>
/// Blazor 授权状态提供器。
/// 它从 localStorage 恢复登录信息，构建 ClaimsPrincipal，并在登录/退出时通知 AuthorizeView 和 [Authorize] 页面刷新状态。
/// </summary>
public class CustomAuthStateProvider : AuthenticationStateProvider
{
    /// <summary>读取/写入浏览器 localStorage 中的 authToken 和 authUser。</summary>
    private readonly IJSRuntime _jsRuntime;

    /// <summary>
    /// 与 ApiService 使用同名 HttpClient。
    /// 授权状态恢复后会把 Bearer Token 写入默认请求头，让后续接口请求自动携带登录态。
    /// </summary>
    private readonly HttpClient _httpClient;

    public CustomAuthStateProvider(IJSRuntime jsRuntime, IHttpClientFactory httpClientFactory)
    {
        _jsRuntime = jsRuntime;
        _httpClient = httpClientFactory.CreateClient("ApiClient");
    }

    /// <summary>
    /// 获取当前登录状态。
    /// 页面刷新时会从 localStorage 读取 Token 和用户信息；缓存异常时降级为未登录，避免授权流程卡住。
    /// </summary>
    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            var token = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", "authToken");
            var userJson = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", "authUser");

            if (string.IsNullOrWhiteSpace(token))
            {
                // 没有 Token 就明确返回匿名身份；AuthorizeView 会据此显示未登录内容。
                return CreateAnonymousState();
            }

            // 刷新页面后内存中的请求头会丢失，这里从 localStorage 恢复到 HttpClient。
            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var user = TryReadCachedUser(userJson);
            if (user == null)
            {
                // 本地用户缓存损坏时不要让授权任务一直异常挂起，清理后回到未登录状态。
                await ClearTokenAsync();
                return CreateAnonymousState();
            }

            var userId = user.EffectiveUserId;
            if (userId <= 0)
            {
                // 某些登录响应没有直接给数字用户 ID，但 Token payload 里可能包含。
                // 报名接口依赖 userId，因此这里尽量从 Token 兜底解析。
                userId = TryReadUserIdFromToken(token);
            }

            // 页面通过 ClaimTypes.Name 显示用户名，通过 ClaimTypes.Role 判断普通用户/管理员权限。
            // EnrollUserId 是报名接口使用的兼容字段：优先数字 ID，缺失时退回用户名。
            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, user.UserName),
                new(ClaimTypes.Role, user.Role.ToString()),
                new("RealName", user.RealName ?? string.Empty),
                new("EnrollUserId", userId > 0 ? userId.ToString() : user.UserName),
                new("Token", token)
            };

            if (userId > 0)
            {
                // 同时写入标准 NameIdentifier 和项目内使用的 UserId，兼容不同页面/服务的读取习惯。
                claims.Add(new Claim(ClaimTypes.NameIdentifier, userId.ToString()));
                claims.Add(new Claim("UserId", userId.ToString()));
            }

            var identity = new ClaimsIdentity(claims, "localStorage");
            return new AuthenticationState(new ClaimsPrincipal(identity));
        }
        catch
        {
            // 任何 JSInterop 或缓存解析异常都降级为未登录，避免页面停在 Authorizing。
            _httpClient.DefaultRequestHeaders.Authorization = null;
            return CreateAnonymousState();
        }
    }

    /// <summary>
    /// 登录成功后保存 Token 和用户信息，并立即通知 Blazor 授权系统用户已登录。
    /// </summary>
    public async Task SetTokenAsync(string token, LoginResult user)
    {
        // Token 和用户信息都保存到 localStorage，保证浏览器刷新后仍能恢复登录状态。
        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "authToken", token);
        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "authUser", JsonSerializer.Serialize(user));

        _httpClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    /// <summary>
    /// 清除本地登录信息，并通知 Blazor 授权系统用户已退出。
    /// </summary>
    public async Task ClearTokenAsync()
    {
        // 退出登录时同时清除 Token 和用户缓存，避免下次刷新又被恢复成已登录。
        await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "authToken");
        await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "authUser");

        _httpClient.DefaultRequestHeaders.Authorization = null;

        NotifyAuthenticationStateChanged(Task.FromResult(CreateAnonymousState()));
    }

    /// <summary>读取当前缓存的 Token，供需要直接检查登录态的页面使用。</summary>
    public async Task<string?> GetTokenAsync()
    {
        return await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", "authToken");
    }

    /// <summary>
    /// 尝试解析 localStorage 中的用户 JSON。
    /// 解析失败返回 null，由调用方负责清理缓存并回到未登录状态。
    /// </summary>
    private static LoginResult? TryReadCachedUser(string? userJson)
    {
        if (string.IsNullOrWhiteSpace(userJson))
        {
            // 有 Token 但没有用户信息时，无法构造完整 ClaimsPrincipal，交由调用方清理登录态。
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<LoginResult>(userJson);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <summary>
    /// 从 JWT payload 中尽量解析用户 ID。
    /// 后端不同版本可能使用 userId、id、nameid 等不同 claim 名称，所以这里逐一兼容。
    /// </summary>
    private static int TryReadUserIdFromToken(string token)
    {
        try
        {
            var parts = token.Split('.');
            if (parts.Length < 2)
            {
                // JWT 至少应包含 header.payload.signature；格式不对则放弃解析。
                return 0;
            }

            // JWT payload 使用 base64url 编码，需要转换成标准 base64 后才能用 Convert 解码。
            var payload = parts[1]
                .Replace('-', '+')
                .Replace('_', '/');
            payload = payload.PadRight(payload.Length + (4 - payload.Length % 4) % 4, '=');

            using var document = JsonDocument.Parse(Encoding.UTF8.GetString(Convert.FromBase64String(payload)));
            var root = document.RootElement;

            // 后端不同版本的 claim 命名不统一，这里按常见字段逐个尝试。
            return TryGetIntClaim(root, "userId")
                ?? TryGetIntClaim(root, "UserId")
                ?? TryGetIntClaim(root, "id")
                ?? TryGetIntClaim(root, "Id")
                ?? TryGetIntClaim(root, ClaimTypes.NameIdentifier)
                ?? TryGetIntClaim(root, "nameid")
                ?? 0;
        }
        catch
        {
            return 0;
        }
    }

    /// <summary>按指定 claim 名称读取整数值，兼容数字和字符串两种 JSON 表示。</summary>
    private static int? TryGetIntClaim(JsonElement root, string propertyName)
    {
        // 不存在该 claim 时返回 null，让调用方继续尝试其它名称。
        if (!root.TryGetProperty(propertyName, out var value))
        {
            return null;
        }

        // 兼容 JSON 数字类型，例如 { "userId": 12 }。
        if (value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out var number))
        {
            return number;
        }

        // 兼容 JSON 字符串类型，例如 { "userId": "12" }。
        return value.ValueKind == JsonValueKind.String && int.TryParse(value.GetString(), out var parsed)
            ? parsed
            : null;
    }

    /// <summary>创建未登录状态。</summary>
    private static AuthenticationState CreateAnonymousState()
    {
        return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
    }
}
