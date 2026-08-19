namespace CampusActivityManagement.Models;

/// <summary>
/// 活动报名记录。
/// 当前页面主要通过活动详情中的报名接口操作，保留该 DTO 用于后续展示报名名单或扩展接口。
/// </summary>
public class EnrollDto
{
    /// <summary>报名记录主键。</summary>
    public int Id { get; set; }

    /// <summary>报名用户的用户名。</summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>关联的活动 ID。</summary>
    public int ActivityId { get; set; }

    /// <summary>报名时间，由后端按字符串格式返回。</summary>
    public string? EnrollTime { get; set; }
}
