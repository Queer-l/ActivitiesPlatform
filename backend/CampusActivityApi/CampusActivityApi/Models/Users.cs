namespace CampusActivityApi.Models;

/// <summary>
/// 系统用户实体，对应数据库中的用户表。
/// </summary>
/// <remarks>
/// 用户名作为业务登录账号使用；密码属性在 <see cref="Data.AppDbContext"/> 中映射到数据库 Password 列。
/// </remarks>
public class User
{
    /// <summary>
    /// 用户主键，自增 ID。
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 用户名或学工号，业务上要求唯一。
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// 登录密码。
    /// </summary>
    public string PassWord { get; set; } = string.Empty;

    /// <summary>
    /// 用户真实姓名，用于管理端展示和身份核验。
    /// </summary>
    public string RealName { get; set; } = string.Empty;

    /// <summary>
    /// 用户角色：1=普通用户，2=普通管理员，3=超级管理员。
    /// </summary>
    public int Role { get; set; } = 1;

    /// <summary>
    /// 绑定邮箱，可用于验证码登录、找回密码和重置密码。
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// 账号是否启用；禁用后不允许登录。
    /// </summary>
    public bool IsActive { get; set; } = true;
}
