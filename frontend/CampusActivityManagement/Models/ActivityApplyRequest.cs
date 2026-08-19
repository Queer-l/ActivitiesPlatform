namespace CampusActivityManagement.Models;

/// <summary>
/// 创建活动申请时提交给后端的请求体。
/// 时间字段沿用后端当前接口约定，使用字符串传输，避免前端和后端时区序列化不一致。
/// </summary>
public class ActivityApplyRequest
{
    /// <summary>活动标题，展示在活动列表、详情页和审核列表中。</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>活动详细说明，支持较长文本。</summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>发起申请的用户名，用于“我的活动”筛选和审核页展示。</summary>
    public string PublisherUserName { get; set; } = string.Empty;

    /// <summary>活动允许报名的最大人数。</summary>
    public int MaxCount { get; set; }

    /// <summary>活动开始时间，格式由后端接口约定。</summary>
    public string ActivityStartTime { get; set; } = string.Empty;

    /// <summary>活动结束时间，必须晚于开始时间。</summary>
    public string ActivityEndTime { get; set; } = string.Empty;

    /// <summary>报名开始时间。</summary>
    public string EnrollStartTime { get; set; } = string.Empty;

    /// <summary>报名结束时间，通常应早于活动开始时间。</summary>
    public string EnrollEndTime { get; set; } = string.Empty;
}
