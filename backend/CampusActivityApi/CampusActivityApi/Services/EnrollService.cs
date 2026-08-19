using CampusActivityApi.Data;
using CampusActivityApi.Models;

namespace CampusActivityApi.Services;

/// <summary>
/// 活动报名业务服务。
/// </summary>
public class EnrollService
{
    private readonly AppDbContext _db;

    /// <summary>
    /// 创建报名服务。
    /// </summary>
    /// <param name="db">数据库上下文。</param>
    public EnrollService(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// 为用户报名指定活动。
    /// </summary>
    /// <param name="userName">报名用户的用户名或学工号。</param>
    /// <param name="activityId">活动 ID。</param>
    /// <returns>报名业务结果。</returns>
    public string DoEnroll(string userName, int activityId)
    {
        var activity = _db.Activities.FirstOrDefault(a =>
            a.Id == activityId && a.AuditStatus == AuditStatusEnum.Passed);

        if (activity == null)
            return "活动不存在";

        if (DateTime.Now < activity.EnrollStartTime)
            return "报名未开始";

        if (activity.EnrollEndTime < DateTime.Now)
            return "报名已截止";

        if (activity.CurrentCount >= activity.MaxCount)
            return "活动人数已满";

        bool hasEnroll = _db.Enrolls.Any(e => e.UserName == userName && e.ActivityId == activityId);
        if (hasEnroll)
            return "你已报名该活动，无需重复操作";

        _db.Enrolls.Add(new Enroll
        {
            UserName = userName,
            ActivityId = activityId,
            EnrollTime = DateTime.Now
        });

        activity.CurrentCount++;
        _db.SaveChanges();

        return "报名成功";
    }

    /// <summary>
    /// 取消用户对指定活动的报名，并同步减少活动当前报名人数。
    /// </summary>
    /// <param name="activityId">活动 ID。</param>
    /// <param name="userName">取消报名用户的用户名或学工号。</param>
    /// <returns>取消报名结果。</returns>
    public string CancelEnroll(int activityId, string userName)
    {
        var enroll = _db.Enrolls.FirstOrDefault(e =>
            e.ActivityId == activityId && e.UserName == userName);

        if (enroll == null)
            return "未查询到有效报名记录";

        _db.Enrolls.Remove(enroll);

        var activity = _db.Activities.FirstOrDefault(a => a.Id == activityId);
        if (activity != null && activity.CurrentCount > 0)
        {
            activity.CurrentCount--;
        }

        _db.SaveChanges();

        return "取消报名成功";
    }
}
