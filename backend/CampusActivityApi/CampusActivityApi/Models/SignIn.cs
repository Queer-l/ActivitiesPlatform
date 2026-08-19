namespace CampusActivityApi.Models;

/// <summary>
/// 活动签到记录实体。
/// </summary>
/// <remarks>
/// 一条记录表示一个用户在指定活动中完成了一次签到。
/// </remarks>
public class SignIn
{
    /// <summary>
    /// 签到记录主键，自增 ID。
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 签到用户的用户名或学工号。
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// 所属活动 ID。
    /// </summary>
    public int ActivityId { get; set; }

    /// <summary>
    /// 签到完成时间，默认取服务器当前时间。
    /// </summary>
    public DateTime SignInTime { get; set; } = DateTime.Now;
}
