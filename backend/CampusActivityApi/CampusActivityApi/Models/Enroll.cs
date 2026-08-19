namespace CampusActivityApi.Models;

/// <summary>
/// 活动报名记录实体。
/// </summary>
/// <remarks>
/// 一条记录表示一个用户报名了一个活动，数据库层通过 UserName + ActivityId 保证不重复报名。
/// </remarks>
public class Enroll
{
    /// <summary>
    /// 报名记录主键，自增 ID。
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 报名用户的用户名或学工号。
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// 被报名活动的 ID。
    /// </summary>
    public int ActivityId { get; set; }

    /// <summary>
    /// 用户完成报名的时间。
    /// </summary>
    public DateTime EnrollTime { get; set; }
}
