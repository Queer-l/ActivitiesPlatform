namespace CampusActivityApi.Models;

/// <summary>
/// 校园活动实体。
/// </summary>
/// <remarks>
/// 当前系统将活动申请和已发布活动统一保存在 Activity 表中，通过 <see cref="AuditStatus"/> 区分审核阶段。
/// </remarks>
public class MyActivity
{
    /// <summary>
    /// 活动主键，自增 ID。
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 活动名称。
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// 活动详情正文。
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// 活动发布者用户名或学工号。
    /// </summary>
    public string PublisherUserName { get; set; } = string.Empty;

    /// <summary>
    /// 审核状态：0=待审核，1=通过，2=驳回。
    /// </summary>
    public AuditStatusEnum AuditStatus { get; set; } = AuditStatusEnum.Pending;

    /// <summary>
    /// 驳回理由；仅当审核状态为驳回时填写。
    /// </summary>
    public string? RejectReason { get; set; }

    /// <summary>
    /// 活动留言集合，随活动详情一起返回给前端展示。
    /// </summary>
    public List<Message> Messages { get; set; } = new List<Message>();

    /// <summary>
    /// 活动申请创建时间。
    /// </summary>
    public DateTime CreateTime { get; set; } = DateTime.Now;

    /// <summary>
    /// 活动最大报名人数。
    /// </summary>
    public int MaxCount { get; set; }

    /// <summary>
    /// 当前已报名人数。
    /// </summary>
    public int CurrentCount { get; set; }

    /// <summary>
    /// 当前已签到人数。
    /// </summary>
    public int SignInCount { get; set; }

    /// <summary>
    /// 活动开始时间。
    /// </summary>
    public DateTime ActivityStartTime { get; set; }

    /// <summary>
    /// 活动结束时间。
    /// </summary>
    public DateTime ActivityEndTime { get; set; }

    /// <summary>
    /// 报名开始时间。
    /// </summary>
    public DateTime EnrollStartTime { get; set; }

    /// <summary>
    /// 报名结束时间。
    /// </summary>
    public DateTime EnrollEndTime { get; set; }

    /// <summary>
    /// 面向前端显示的审核状态文本。
    /// </summary>
    public string AuditStatusText
    {
        get
        {
            return AuditStatus switch
            {
                AuditStatusEnum.Pending => "待审核",
                AuditStatusEnum.Passed => "通过",
                AuditStatusEnum.Rejected => "已驳回",
                _ => "未知"
            };
        }
    }

    /// <summary>
    /// 根据报名时间和活动时间实时计算当前活动状态。
    /// </summary>
    /// <returns>活动当前所处的业务状态。</returns>
    public ActivityStatusEnum GetCurrentStatus()
    {
        DateTime now = DateTime.Now;

        if (now < EnrollStartTime)
            return ActivityStatusEnum.预热中;

        if (now >= EnrollStartTime && now <= EnrollEndTime)
            return ActivityStatusEnum.报名中;

        if (now > EnrollEndTime && now < ActivityStartTime)
            return ActivityStatusEnum.待开始;

        if (now >= ActivityStartTime && now <= ActivityEndTime)
            return ActivityStatusEnum.进行中;

        return ActivityStatusEnum.已结束;
    }

    /// <summary>
    /// 面向前端显示的活动状态文本。
    /// </summary>
    public string StatusText => GetCurrentStatus().ToString();
}
