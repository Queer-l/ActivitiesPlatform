namespace CampusActivityManagement.Models;

/// <summary>
/// 管理员用户列表使用的用户数据。
/// 字段名称跟后端 DTO 保持一致，减少序列化映射成本。
/// </summary>
public class UserDto
{
    /// <summary>用户主键。</summary>
    public int Id { get; set; }

    /// <summary>登录用户名或学工号。</summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>后端返回的密码字段；页面不应主动展示明文密码。</summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>真实姓名。</summary>
    public string RealName { get; set; } = string.Empty;

    /// <summary>角色编号：1 普通用户，2 管理员，3 超级管理员。</summary>
    public int Role { get; set; }

    /// <summary>绑定邮箱，可为空。</summary>
    public string? Email { get; set; }

    /// <summary>账号是否启用。</summary>
    public bool IsActive { get; set; }
}
