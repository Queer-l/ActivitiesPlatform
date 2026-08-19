namespace CampusActivityApi.Models;

/// <summary>
/// 活动申请审核状态。
/// </summary>
/// <remarks>
/// 数据库 Activity.AuditStatus 必须允许 0、1、2 三个值，分别对应待审核、通过和驳回。
/// </remarks>
public enum AuditStatusEnum
{
    /// <summary>
    /// 待审核：申请已提交，但管理员尚未处理。
    /// </summary>
    Pending = 0,

    /// <summary>
    /// 通过：管理员审核通过，活动可对外展示和报名。
    /// </summary>
    Passed = 1,

    /// <summary>
    /// 已驳回：管理员拒绝该活动申请。
    /// </summary>
    Rejected = 2
}
