namespace CampusActivityManagement.Models;

/// <summary>
/// 活动留言数据。
/// 详情页会按活动 ID 拉取留言，并通过该模型渲染留言作者、内容和创建时间。
/// </summary>
public class MessageDto
{
    /// <summary>留言主键。</summary>
    public int Id { get; set; }

    /// <summary>留言所属活动 ID。</summary>
    public int ActivityId { get; set; }

    /// <summary>留言用户。</summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>留言正文。</summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>留言创建时间。</summary>
    public string? CreateTime { get; set; }
}
