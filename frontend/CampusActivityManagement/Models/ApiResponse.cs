namespace CampusActivityManagement.Models;

/// <summary>
/// 后端统一响应包装。
/// 业务接口通常返回 { code, msg, data }，前端会先按这个结构拆包，再把 Data 交给页面使用。
/// </summary>
public class ApiResponse<T>
{
    /// <summary>业务状态码，当前约定 200 表示成功。</summary>
    public int Code { get; set; }

    /// <summary>后端返回的提示文本，失败时会作为异常消息显示给用户。</summary>
    public string Msg { get; set; } = string.Empty;

    /// <summary>实际业务数据；无数据接口通常为 null。</summary>
    public T? Data { get; set; }
}

/// <summary>
/// 非泛型响应类型，用于只关心成功/失败、不需要 data 的接口。
/// </summary>
public class ApiResponse : ApiResponse<object> { }
