using CampusActivityApi.Data;
using CampusActivityApi.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace CampusActivityApi.Services;

/// <summary>
/// 活动申请、审核、下架和留言相关业务服务。
/// </summary>
public class ActivityService
{
    private readonly AppDbContext _db;

    /// <summary>
    /// 创建活动服务。
    /// </summary>
    /// <param name="db">数据库上下文。</param>
    public ActivityService(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// 提交活动申请，并初始化审核状态、报名人数、签到人数和创建时间。
    /// </summary>
    /// <param name="activity">前端提交的活动申请实体。</param>
    /// <returns>申请提交结果；数据库审核状态约束异常会转换为明确业务提示。</returns>
    public string ApplyActivity(MyActivity activity)
    {
        activity.Id = 0;
        activity.AuditStatus = AuditStatusEnum.Pending;
        activity.RejectReason = null;
        activity.CurrentCount = 0;
        activity.SignInCount = 0;
        activity.CreateTime = DateTime.Now;

        _db.Activities.Add(activity);
        try
        {
            _db.SaveChanges();
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException sqlException
                                           && sqlException.Number == 547
                                           && sqlException.Message.Contains("CK_Activity_AuditStatus"))
        {
            return "活动申请提交失败：数据库审核状态约束配置错误，请执行 Database/FixActivityAuditStatusConstraint.sql 修复 CK_Activity_AuditStatus。";
        }

        return "活动申请提交成功，等待管理员审核";
    }

    /// <summary>
    /// 获取所有未通过审核的活动申请。
    /// </summary>
    /// <returns>待审核和已驳回申请，按创建时间倒序排列。</returns>
    public List<MyActivity> GetAllApplies()
    {
        return _db.Activities
            .Where(a => a.AuditStatus != AuditStatusEnum.Passed)
            .OrderByDescending(a => a.CreateTime)
            .ToList();
    }

    /// <summary>
    /// 管理员审核活动申请。
    /// </summary>
    /// <param name="applyId">申请 ID。</param>
    /// <param name="isPass">是否审核通过。</param>
    /// <param name="rejectReason">驳回理由，驳回时必填。</param>
    /// <returns>审核处理结果。</returns>
    public string CheckActivity(int applyId, bool isPass, string? rejectReason)
    {
        var apply = _db.Activities.FirstOrDefault(a => a.Id == applyId);
        if (apply == null) return "申请不存在";

        if (!isPass && string.IsNullOrWhiteSpace(rejectReason))
            return "驳回理由不能为空";

        apply.AuditStatus = isPass ? AuditStatusEnum.Passed : AuditStatusEnum.Rejected;
        apply.RejectReason = isPass ? null : rejectReason;
        _db.SaveChanges();

        return isPass ? "活动审核通过，已发布到活动列表" : "成功驳回申请";
    }

    /// <summary>
    /// 取消当前用户自己的未发布活动申请。
    /// </summary>
    /// <param name="applyId">申请 ID。</param>
    /// <param name="userName">当前登录用户名。</param>
    /// <returns>取消处理结果。</returns>
    public string CancelActivityApply(int applyId, string userName)
    {
        var apply = _db.Activities.FirstOrDefault(a => a.Id == applyId);
        if (apply == null)
            return "申请不存在";

        if (apply.AuditStatus == AuditStatusEnum.Passed)
            return "已发布活动不能取消申请，请使用下架功能";

        if (apply.PublisherUserName != userName)
            return "只能取消自己的申请";

        _db.Activities.Remove(apply);
        _db.SaveChanges();

        return "取消申请成功";
    }

    /// <summary>
    /// 下架已审核通过的活动。
    /// </summary>
    /// <param name="activityId">活动 ID。</param>
    /// <param name="userName">当前登录用户名。</param>
    /// <param name="role">当前登录用户角色。</param>
    /// <returns>下架处理结果。</returns>
    public string TakeDownActivity(int activityId, string userName, int role)
    {
        var activity = _db.Activities.FirstOrDefault(a =>
            a.Id == activityId && a.AuditStatus == AuditStatusEnum.Passed);

        if (activity == null)
            return "活动不存在";

        if (role != 2 && role != 3 && activity.PublisherUserName != userName)
            return "无权限下架";

        _db.Activities.Remove(activity);
        _db.SaveChanges();

        return "活动已下架";
    }

    /// <summary>
    /// 获取已审核通过并可展示的活动列表。
    /// </summary>
    /// <returns>已发布活动列表，按创建时间倒序排列。</returns>
    public List<MyActivity> GetPassedActivities()
    {
        return _db.Activities
            .Where(a => a.AuditStatus == AuditStatusEnum.Passed)
            .OrderByDescending(a => a.CreateTime)
            .ToList();
    }

    /// <summary>
    /// 为已发布活动新增留言。
    /// </summary>
    /// <param name="activityId">活动 ID。</param>
    /// <param name="userName">留言用户。</param>
    /// <param name="content">留言内容。</param>
    /// <returns>留言发布结果。</returns>
    public string AddMessage(int activityId, string userName, string content)
    {
        var activity = _db.Activities.FirstOrDefault(a =>
            a.Id == activityId && a.AuditStatus == AuditStatusEnum.Passed);

        if (activity == null) return "活动不存在";
        if (string.IsNullOrWhiteSpace(content)) return "内容不能为空";

        _db.Messages.Add(new Message
        {
            ActivityId = activity.Id,
            UserName = userName,
            Content = content,
            CreateTime = DateTime.Now
        });
        _db.SaveChanges();

        return "留言成功";
    }

    /// <summary>
    /// 删除活动留言。
    /// </summary>
    /// <param name="activityId">活动 ID。</param>
    /// <param name="messageId">留言 ID。</param>
    /// <param name="userName">当前登录用户名。</param>
    /// <param name="role">当前登录用户角色。</param>
    /// <returns>删除处理结果。</returns>
    public string DeleteMessage(int activityId, int messageId, string userName, int role)
    {
        var activity = _db.Activities.FirstOrDefault(a =>
            a.Id == activityId && a.AuditStatus == AuditStatusEnum.Passed);

        if (activity == null) return "活动不存在";

        var msg = _db.Messages.FirstOrDefault(m => m.ActivityId == activityId && m.Id == messageId);
        if (msg == null) return "留言不存在";

        if (msg.UserName != userName && role != 2 && role != 3)
            return "无权限删除";

        _db.Messages.Remove(msg);
        _db.SaveChanges();

        return "删除成功";
    }

    /// <summary>
    /// 查询指定活动的全部留言。
    /// </summary>
    /// <param name="activityId">活动 ID。</param>
    /// <returns>留言列表，按创建时间倒序排列。</returns>
    public List<Message> GetMessagesByActivity(int activityId)
    {
        return _db.Messages
            .Where(m => m.ActivityId == activityId)
            .OrderByDescending(m => m.CreateTime)
            .ToList();
    }
}
