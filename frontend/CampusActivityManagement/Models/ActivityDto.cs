namespace CampusActivityManagement.Models;

/// <summary>
/// 活动详情、活动列表、活动审核列表共用的数据模型。
/// 该类型同时承载“审核状态”和“活动运行状态”，因此页面统一读取 DisplayStatusText。
/// </summary>
public class ActivityDto
{
    /// <summary>活动或活动申请的主键。</summary>
    public int Id { get; set; }

    /// <summary>活动标题。</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>活动详情内容。</summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>发布者用户名，用于“我的活动”按当前登录用户过滤。</summary>
    public string PublisherUserName { get; set; } = string.Empty;

    /// <summary>审核状态：0 待审核，1 已通过，2 已驳回。</summary>
    public int AuditStatus { get; set; }

    /// <summary>后端返回的审核状态文本。</summary>
    public string? AuditStatusText { get; set; }

    /// <summary>活动申请被驳回时的原因。</summary>
    public string? RejectReason { get; set; }

    /// <summary>创建时间，用于列表倒序展示。</summary>
    public string? CreateTime { get; set; }

    /// <summary>最大报名人数。</summary>
    public int MaxCount { get; set; }

    /// <summary>当前报名人数。</summary>
    public int CurrentCount { get; set; }

    /// <summary>已签到人数。</summary>
    public int SignInCount { get; set; }

    /// <summary>活动开始时间。</summary>
    public string? ActivityStartTime { get; set; }

    /// <summary>活动结束时间。</summary>
    public string? ActivityEndTime { get; set; }

    /// <summary>报名开始时间。</summary>
    public string? EnrollStartTime { get; set; }

    /// <summary>报名结束时间。</summary>
    public string? EnrollEndTime { get; set; }

    /// <summary>活动运行状态文本，例如预热中、报名中、进行中、已结束。</summary>
    public string? StatusText { get; set; }

    /// <summary>活动留言集合，部分列表接口可能不返回。</summary>
    public List<MessageDto>? Messages { get; set; }

    /// <summary>
    /// 审核状态的统一展示文本。
    /// 对 AuditStatus=2 强制显示“已驳回”，避免后端文本字段残留“待审核”时误导页面。
    /// </summary>
    public string DisplayAuditStatusText => AuditStatus switch
    {
        2 => "已驳回",
        1 => string.IsNullOrWhiteSpace(AuditStatusText) ? "已通过" : AuditStatusText,
        0 => string.IsNullOrWhiteSpace(AuditStatusText) ? "待审核" : AuditStatusText,
        _ => string.IsNullOrWhiteSpace(AuditStatusText) ? "未知" : AuditStatusText
    };

    /// <summary>
    /// 页面最终应显示的状态文本。
    /// 优先处理驳回状态，再使用活动运行状态，最后退回审核状态，解决本地缓存和后端字段不同步的问题。
    /// </summary>
    public string DisplayStatusText
    {
        get
        {
            if (AuditStatus == 2 || ContainsRejectedText(AuditStatusText) || ContainsRejectedText(StatusText))
            {
                return "已驳回";
            }

            if (!string.IsNullOrWhiteSpace(StatusText))
            {
                return StatusText;
            }

            return DisplayAuditStatusText;
        }
    }

    /// <summary>
    /// 判断后端状态文本里是否已经包含驳回语义，用于兼容不同后端版本的文字返回。
    /// </summary>
    private static bool ContainsRejectedText(string? value)
    {
        return !string.IsNullOrWhiteSpace(value)
            && (value.Contains("驳回", StringComparison.Ordinal)
                || value.Contains("拒绝", StringComparison.Ordinal));
    }
}
