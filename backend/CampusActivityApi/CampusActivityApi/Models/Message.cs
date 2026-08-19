namespace CampusActivityApi.Models;

/// <summary>
/// 活动留言实体。
/// </summary>
/// <remarks>
/// 留言属于具体活动，用于活动详情页的用户交流区。
/// </remarks>
public class Message
{
    /// <summary>
    /// 留言主键，自增 ID。
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 留言所属活动 ID。
    /// </summary>
    public int ActivityId { get; set; }

    /// <summary>
    /// 留言用户的用户名或学工号。
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// 留言正文。
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// 留言创建时间，默认按中国标准时间生成。
    /// </summary>
    public DateTime CreateTime { get; set; } = TimeZoneInfo.ConvertTimeFromUtc(
        DateTime.UtcNow,
        TimeZoneInfo.FindSystemTimeZoneById("China Standard Time"));
}
