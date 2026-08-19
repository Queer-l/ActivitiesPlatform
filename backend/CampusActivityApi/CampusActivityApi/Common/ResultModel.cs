namespace CampusActivityApi.Common;

/// <summary>
/// API 统一响应包装模型。
/// </summary>
/// <typeparam name="T">响应数据的实际类型。</typeparam>
/// <remarks>
/// 控制器返回该结构后，前端可统一读取 Code、Msg 和 Data 判断请求结果。
/// </remarks>
/// <summary>
/// Standard response envelope returned by application endpoints.
/// </summary>
/// <typeparam name="T">Payload type stored in <see cref="Data"/>.</typeparam>
/// <remarks>
/// The project keeps HTTP status codes mostly at 200 and places business status in
/// <see cref="Code"/>. Frontend code should use Code/Msg/Data instead of relying only
/// on the transport status.
/// </remarks>
public class ResultModel<T>
{
    /// <summary>
    /// 业务状态码；200 表示成功，400 表示业务失败。
    /// </summary>
    /// <summary>
    /// Business status code. Current convention: 200 = success, 400 = failure.
    /// </summary>
    public int Code { get; set; }

    /// <summary>
    /// 面向前端展示的结果消息。
    /// </summary>
    /// <summary>
    /// Human-readable operation message for direct frontend display.
    /// </summary>
    public string Msg { get; set; } = string.Empty;

    /// <summary>
    /// 接口实际返回的数据；失败时通常为空。
    /// </summary>
    /// <summary>
    /// Optional response payload. It is null/default for failed responses.
    /// </summary>
    public T? Data { get; set; }

    /// <summary>
    /// 构造成功响应。
    /// </summary>
    /// <param name="data">需要返回给前端的数据。</param>
    /// <param name="msg">成功提示文本。</param>
    /// <returns>统一格式的成功响应。</returns>
    /// <summary>
    /// Creates a successful business response.
    /// </summary>
    /// <param name="data">Payload returned to the frontend.</param>
    /// <param name="msg">Optional success message.</param>
    public static ResultModel<T> Success(T data, string msg = "操作成功")
    {
        return new ResultModel<T>
        {
            Code = 200,
            Msg = msg,
            Data = data
        };
    }

    /// <summary>
    /// 构造失败响应。
    /// </summary>
    /// <param name="msg">失败原因或业务提示。</param>
    /// <returns>统一格式的失败响应。</returns>
    /// <summary>
    /// Creates a failed business response without a payload.
    /// </summary>
    /// <param name="msg">Failure reason shown to the frontend.</param>
    public static ResultModel<T> Fail(string msg = "操作失败")
    {
        return new ResultModel<T>
        {
            Code = 400,
            Msg = msg,
            Data = default
        };
    }
}
