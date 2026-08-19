namespace CampusActivityManagement.Models;

/// <summary>
/// 活动签到记录。
/// 活动发布者查看签到名单时使用该模型渲染用户和签到时间。
/// </summary>
public class SignInRecordDto
{
    /// <summary>签到记录主键。</summary>
    public int Id { get; set; }

    /// <summary>签到用户。</summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>关联活动 ID。</summary>
    public int ActivityId { get; set; }

    /// <summary>签到时间。</summary>
    public string? SignInTime { get; set; }
}
