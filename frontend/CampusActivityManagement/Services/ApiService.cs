using System.Net.Http.Json;
using System.Text.Json;
using System.Web;
using Microsoft.JSInterop;
using CampusActivityManagement.Models;

namespace CampusActivityManagement.Services;

/// <summary>
/// 前端访问后端 API 的统一入口。
/// 该服务负责补齐 Token、拼接查询参数、处理统一响应格式，并把网络/业务错误转换成页面可展示的异常消息。
/// </summary>
public class ApiService
{
    /// <summary>统一访问后端的 HttpClient，由 Program.cs 中的 ApiClient 配置 BaseAddress。</summary>
    private readonly HttpClient _httpClient;

    /// <summary>用于从 localStorage 恢复 Token，解决刷新页面后请求头丢失的问题。</summary>
    private readonly IJSRuntime _jsRuntime;

    /// <summary>
    /// API 序列化配置。
    /// 后端字段命名可能保持 PascalCase，也可能返回 camelCase，所以读取时允许大小写不敏感。
    /// </summary>
    private readonly JsonSerializerOptions _jsonOptions;

    public ApiService(IHttpClientFactory httpClientFactory, IJSRuntime jsRuntime)
    {
        _httpClient = httpClientFactory.CreateClient("ApiClient");
        _jsRuntime = jsRuntime;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = null
        };
    }

    /// <summary>
    /// 设置当前请求客户端的 Bearer Token。
    /// 登录成功或从 localStorage 恢复登录状态后调用。
    /// </summary>
    public void SetToken(string token)
    {
        _httpClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
    }

    /// <summary>
    /// 清空请求客户端的授权头，用于退出登录或登录缓存失效。
    /// </summary>
    public void ClearToken()
    {
        _httpClient.DefaultRequestHeaders.Authorization = null;
    }

    /// <summary>
    /// 兼容两类后端响应：统一包装 { code, msg, data } 与直接返回业务对象/数组。
    /// 页面层不需要关心后端具体返回形态，只接收已经拆包后的 Data。
    /// </summary>
    private async Task<T?> DeserializeResponse<T>(string responseContent)
    {
        if (string.IsNullOrWhiteSpace(responseContent))
        {
            // 某些 POST 接口只返回空响应体，调用方使用 object 时不需要业务数据。
            return default;
        }

        if (typeof(T) == typeof(object))
        {
            try
            {
                // 对“只关心成功/失败”的接口，仍然尝试读取统一响应格式中的 code/msg。
                // 这样即使没有 data，也能把业务失败消息抛给页面显示。
                var apiResp = JsonSerializer.Deserialize<ApiResponse>(responseContent, _jsonOptions);
                if (apiResp != null)
                {
                    if (apiResp.Code != 200)
                        throw new InvalidOperationException(apiResp.Msg ?? "操作失败");
                    return default;
                }
            }
            catch (JsonException) { }
            return default;
        }

        var trimmed = responseContent.TrimStart();
        if (!trimmed.StartsWith('{') && !trimmed.StartsWith('['))
        {
            // 非 JSON 短文本通常是后端直接返回的错误提示，转成 InvalidOperationException 给页面展示。
            // 过长文本更可能是 HTML 错误页或代理响应，隐藏具体内容避免污染 UI。
            if (responseContent.Length > 200)
                throw new InvalidOperationException("服务器返回了非预期的响应格式");
            throw new InvalidOperationException(responseContent);
        }

        try
        {
            // 优先按统一包装格式 { code, msg, data } 解析，这是当前大多数后端接口的约定。
            var apiResponse = JsonSerializer.Deserialize<ApiResponse<T>>(responseContent, _jsonOptions);
            if (apiResponse != null)
            {
                if (apiResponse.Code != 200)
                    throw new InvalidOperationException(apiResponse.Msg ?? "操作失败");
                return apiResponse.Data;
            }
        }
        catch (JsonException) { }

        try
        {
            // 少数接口可能直接返回数组、字符串或业务对象；统一包装解析失败后再尝试直接解析。
            return JsonSerializer.Deserialize<T>(responseContent, _jsonOptions);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"响应数据格式错误: {ex.Message}");
        }
    }

    /// <summary>
    /// 发送 API 请求的核心方法。
    /// 这里统一处理 Token 恢复、query string 拼接、JSON/form body 写入、HTTP 错误和业务错误。
    /// </summary>
    private async Task<T?> SendAsync<T>(HttpMethod method, string url,
        Dictionary<string, string?>? queryParams = null,
        object? body = null,
        HttpContent? formContent = null)
    {
        // 每次请求前都尝试补齐授权头，保证刷新页面后首次 API 请求也能携带 Token。
        await EnsureAuthorizationHeaderAsync();

        var uriBuilder = new UriBuilder(new Uri(_httpClient.BaseAddress!, url));
        if (queryParams != null)
        {
            // 使用 HttpUtility 生成 query string，自动处理中文、空格等特殊字符编码。
            var query = HttpUtility.ParseQueryString("");
            foreach (var kvp in queryParams)
            {
                // null/空字符串参数不追加到 URL，避免后端把空值误判为显式传参。
                if (!string.IsNullOrEmpty(kvp.Value))
                    query[kvp.Key] = kvp.Value;
            }
            uriBuilder.Query = query.ToString();
        }

        var request = new HttpRequestMessage(method, uriBuilder.Uri.PathAndQuery);

        if (formContent != null)
        {
            // 登录接口按 Swagger 要求使用 multipart/form-data，因此优先使用调用方传入的 formContent。
            request.Content = formContent;
        }
        else if (body != null)
        {
            // 其它提交类接口默认使用 JSON body。
            request.Content = JsonContent.Create(body, options: _jsonOptions);
        }

        try
        {
            var response = await _httpClient.SendAsync(request);

            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    // 401 单独抛出，页面可以提示重新登录，而不是展示普通网络错误。
                    throw new UnauthorizedAccessException("登录已过期，请重新登录");
                }

                try
                {
                    // 即使 HTTP 状态码不是 2xx，后端也可能返回统一错误格式，优先展示其中的 msg。
                    var errResp = JsonSerializer.Deserialize<ApiResponse>(responseContent, _jsonOptions);
                    if (errResp != null && !string.IsNullOrEmpty(errResp.Msg))
                        throw new InvalidOperationException(errResp.Msg);
                }
                catch (JsonException) { }

                throw new HttpRequestException($"请求失败: {response.StatusCode}, 内容: {responseContent}");
            }

            return await DeserializeResponse<T>(responseContent);
        }
        catch (UnauthorizedAccessException)
        {
            throw;
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (HttpRequestException ex) when (ex.Message.Contains("请求失败"))
        {
            throw;
        }
        catch (HttpRequestException)
        {
            // 连接失败、DNS 错误等底层网络异常统一转换成用户可理解的中文提示。
            throw new InvalidOperationException("网络连接失败，请检查网络");
        }
        catch (TaskCanceledException)
        {
            // HttpClient 超时通常表现为 TaskCanceledException。
            throw new InvalidOperationException("请求超时，请稍后重试");
        }
    }

    /// <summary>GET 请求便捷包装，保持具体接口方法更短更清晰。</summary>
    private async Task<T?> GetAsync<T>(string url, Dictionary<string, string?>? queryParams = null)
        => await SendAsync<T>(HttpMethod.Get, url, queryParams);

    /// <summary>POST 请求便捷包装，支持 query string 和 JSON body。</summary>
    private async Task<T?> PostAsync<T>(string url, Dictionary<string, string?>? queryParams = null, object? body = null)
        => await SendAsync<T>(HttpMethod.Post, url, queryParams, body);

    /// <summary>
    /// 页面刷新后 HttpClient 内存中的 Authorization 会丢失。
    /// 每次请求前从 localStorage 尝试恢复 Token，保证刷新页面后仍能访问需登录接口。
    /// </summary>
    private async Task EnsureAuthorizationHeaderAsync()
    {
        if (_httpClient.DefaultRequestHeaders.Authorization != null)
        {
            return;
        }

        try
        {
            var token = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", "authToken");
            if (!string.IsNullOrWhiteSpace(token))
            {
                SetToken(token);
            }
        }
        catch (JSException)
        {
            // Blazor 启动早期或特殊浏览器环境中 localStorage 可能不可用。
            // 此时继续匿名请求，让后端按权限返回结果。
        }
    }

    // ==================== User Module (11 endpoints) ====================

    /// <summary>账号密码登录，成功后返回 Token 和用户信息。</summary>
    public async Task<LoginResult?> LoginByPwd(string userName, string pwd)
    {
        // Swagger 标注 LoginByPwd 使用 multipart/form-data。
        // 按文档构造表单可以避免后端模型绑定在 JSON/form 两种格式之间出现兼容问题。
        using var formContent = new MultipartFormDataContent
        {
            { new StringContent(userName), "userName" },
            { new StringContent(pwd), "pwd" }
        };

        return await SendAsync<LoginResult>(HttpMethod.Post, "User/LoginByPwd", formContent: formContent);
    }

    /// <summary>邮箱验证码登录。</summary>
    public async Task<LoginResult?> LoginByEmail(string email, string code)
    {
        // 邮箱登录只需要 query 参数，不需要 JSON body。
        return await SendAsync<LoginResult>(HttpMethod.Post, "User/LoginByEmail",
            new Dictionary<string, string?> { ["email"] = email, ["code"] = code });
    }

    /// <summary>发送邮箱验证码，用于邮箱登录、绑定邮箱和邮箱重置密码流程。</summary>
    public async Task SendEmailCode(string email)
    {
        await PostAsync<object>("User/SendEmailCode",
            new Dictionary<string, string?> { ["email"] = email });
    }

    /// <summary>为当前用户绑定邮箱。</summary>
    public async Task BindEmail(string userName, string email, string code)
    {
        await PostAsync<object>("User/BindEmail",
            new Dictionary<string, string?> { ["userName"] = userName, ["email"] = email, ["code"] = code });
    }

    /// <summary>通过邮箱验证码重置密码。</summary>
    public async Task ResetPasswordByEmail(string email, string code, string newPwd)
    {
        await PostAsync<object>("User/ResetPasswordByEmail",
            new Dictionary<string, string?> { ["email"] = email, ["code"] = code, ["newPwd"] = newPwd });
    }

    /// <summary>提交人工密码重置申请，等待管理员审核。</summary>
    public async Task ApplyResetPwd(string userName, string realName, string reason)
    {
        await PostAsync<object>("User/ApplyResetPwd",
            new Dictionary<string, string?> { ["userName"] = userName, ["realName"] = realName, ["reason"] = reason });
    }

    /// <summary>获取用户列表，管理员页面使用。</summary>
    public async Task<List<UserDto>?> GetUsersList()
    {
        return await GetAsync<List<UserDto>>("User/GetUsersList");
    }

    /// <summary>新增用户，仅超级管理员可用。</summary>
    public async Task AddUser(string userName, string realName, int role, string? email)
    {
        await PostAsync<object>("User/AddUser",
            new Dictionary<string, string?> { ["userName"] = userName, ["realName"] = realName, ["role"] = role.ToString(), ["email"] = email });
    }

    /// <summary>管理员编辑用户邮箱或角色。</summary>
    public async Task AdminEditUser(string userName, string? email, int? role)
    {
        var queryParams = new Dictionary<string, string?> { ["userName"] = userName };

        // 后端把 email/role 设计成可选参数；只有页面实际修改了对应字段才追加。
        if (email != null) queryParams["email"] = email;
        if (role.HasValue) queryParams["role"] = role.Value.ToString();
        await PostAsync<object>("User/AdminEditUser", queryParams);
    }

    /// <summary>获取密码重置申请列表。</summary>
    public async Task<List<PwdResetApplyDto>?> GetPwdResetApplies()
    {
        return await GetAsync<List<PwdResetApplyDto>>("User/GetPwdResetApplies");
    }

    /// <summary>处理密码重置申请，isPass=false 时会携带驳回理由。</summary>
    public async Task HandlePwdResetApply(int id, bool isPass, string? rejectReason = null)
    {
        var queryParams = new Dictionary<string, string?> { ["id"] = id.ToString(), ["isPass"] = isPass.ToString().ToLower() };

        // 只有驳回时才需要理由；通过时不传 rejectReason，保持请求参数简洁。
        if (!string.IsNullOrEmpty(rejectReason)) queryParams["rejectReason"] = rejectReason;
        await PostAsync<object>("User/HandlePwdResetApply", queryParams);
    }

    // ==================== Activity Module (9 endpoints) ====================

    /// <summary>提交活动申请，等待管理员审核。</summary>
    public async Task ApplyActivity(ActivityApplyRequest request)
    {
        await SendAsync<object>(HttpMethod.Post, "Activity/ApplyActivity", body: request);
    }

    /// <summary>获取活动申请/审核列表，管理员审核页和“我的活动”状态修正会使用。</summary>
    public async Task<List<ActivityDto>?> GetActivityApplies()
    {
        return await GetAsync<List<ActivityDto>>("Activity/GetActivityApplies");
    }

    /// <summary>审核活动申请，isPass=false 时表示驳回并携带驳回理由。</summary>
    public async Task CheckActivityApplies(int id, bool isPass, string? rejectReason = null)
    {
        var queryParams = new Dictionary<string, string?> { ["id"] = id.ToString(), ["isPass"] = isPass.ToString().ToLower() };

        // 活动审核通过时不携带驳回理由；驳回时页面会传入管理员填写的原因。
        if (!string.IsNullOrEmpty(rejectReason)) queryParams["rejectReason"] = rejectReason;
        await PostAsync<object>("Activity/CheckActivityApplies", queryParams);
    }

    /// <summary>撤销自己提交的活动申请。</summary>
    public async Task CancelActivityApply(int applyId)
    {
        await PostAsync<object>("Activity/CancelActivityApply",
            new Dictionary<string, string?> { ["applyId"] = applyId.ToString() });
    }

    /// <summary>下架/撤销已发布活动。</summary>
    public async Task TakeDownActivity(int activityId)
    {
        await PostAsync<object>("Activity/TakeDownActivity",
            new Dictionary<string, string?> { ["activityId"] = activityId.ToString() });
    }

    /// <summary>获取对普通用户可见的活动列表。</summary>
    public async Task<List<ActivityDto>?> GetActivitysList()
    {
        return await GetAsync<List<ActivityDto>>("Activity/GetActivitysList");
    }

    /// <summary>为活动添加留言。</summary>
    public async Task AddMessage(int activityId, string content)
    {
        await PostAsync<object>("Activity/AddMessage",
            new Dictionary<string, string?> { ["activityId"] = activityId.ToString(), ["content"] = content });
    }

    /// <summary>删除活动留言，后端负责校验作者或管理员权限。</summary>
    public async Task DeleteMessage(int activityId, int messageId)
    {
        await PostAsync<object>("Activity/DeleteMessage",
            new Dictionary<string, string?> { ["activityId"] = activityId.ToString(), ["messageId"] = messageId.ToString() });
    }

    /// <summary>按活动 ID 获取留言列表。</summary>
    public async Task<List<MessageDto>?> GetMessagesByActivity(int activityId)
    {
        return await GetAsync<List<MessageDto>>("Activity/GetMessagesByActivity",
            new Dictionary<string, string?> { ["activityId"] = activityId.ToString() });
    }

    // ==================== Enroll Module (2 endpoints) ====================

    /// <summary>报名活动。userId 按后端接口要求传字符串。</summary>
    public async Task DoEnroll(int activityId, string userId)
    {
        await PostAsync<object>("Enroll/DoEnroll",
            new Dictionary<string, string?> { ["activityId"] = activityId.ToString(), ["userId"] = userId });
    }

    /// <summary>取消当前用户对指定活动的报名。</summary>
    public async Task CancelEnroll(int activityId)
    {
        await PostAsync<object>("Enroll/CancelEnroll",
            new Dictionary<string, string?> { ["activityId"] = activityId.ToString() });
    }

    // ==================== SignIn Module (3 endpoints) ====================

    /// <summary>活动发布者生成签到码。</summary>
    public async Task<string?> GenerateSignInCode(int activityId)
    {
        return await PostAsync<string>("SignIn/GenerateSignInCode",
            new Dictionary<string, string?> { ["activityId"] = activityId.ToString() });
    }

    /// <summary>参与者输入/扫码签到码完成签到。</summary>
    public async Task ScanSignIn(string code)
    {
        await PostAsync<object>("SignIn/ScanSignIn",
            new Dictionary<string, string?> { ["code"] = code });
    }

    /// <summary>获取指定活动的签到名单。</summary>
    public async Task<List<SignInRecordDto>?> GetSignInList(int activityId)
    {
        return await GetAsync<List<SignInRecordDto>>("SignIn/GetSignInList",
            new Dictionary<string, string?> { ["activityId"] = activityId.ToString() });
    }
}
