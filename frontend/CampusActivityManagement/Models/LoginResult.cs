namespace CampusActivityManagement.Models;

/// <summary>
/// 登录成功后后端返回的身份信息。
/// 前端会把 Token 保存到 localStorage，并把用户信息转换成 Claims 用于授权判断。
/// </summary>
public class LoginResult
{
    /// <summary>兼容部分接口返回的用户主键字段。</summary>
    public int Id { get; set; }

    /// <summary>后端推荐使用的用户主键字段。</summary>
    public int UserId { get; set; }

    /// <summary>Bearer Token，后续 API 请求会放入 Authorization 请求头。</summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>登录名，也是很多业务接口识别用户的字段。</summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>真实姓名，用于后台管理展示。</summary>
    public string RealName { get; set; } = string.Empty;

    /// <summary>角色编号：1 普通用户，2 管理员，3 超级管理员。</summary>
    public int Role { get; set; }

    /// <summary>
    /// 统一后的用户 ID。后端不同版本可能返回 Id 或 UserId，这里优先使用 UserId。
    /// </summary>
    public int EffectiveUserId => UserId > 0 ? UserId : Id;

    /// <summary>
    /// 报名接口需要字符串形式的 userId；没有数字 ID 时退回用户名，兼容旧后端。
    /// </summary>
    public string EffectiveEnrollUserId => EffectiveUserId > 0 ? EffectiveUserId.ToString() : UserName;
}
