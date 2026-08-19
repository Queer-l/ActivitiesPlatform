namespace CampusActivityManagement.Models;

/// <summary>
/// 密码重置审核申请。
/// 管理员审核页会根据 Status 决定是否展示通过/驳回操作。
/// </summary>
public class PwdResetApplyDto
{
    /// <summary>申请主键。</summary>
    public int Id { get; set; }

    /// <summary>申请重置密码的用户名。</summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>申请人真实姓名，用于管理员核对身份。</summary>
    public string RealName { get; set; } = string.Empty;

    /// <summary>用户提交的重置原因。</summary>
    public string Reason { get; set; } = string.Empty;

    /// <summary>审核状态：0 待审核，1 已通过，2 已驳回。</summary>
    public int Status { get; set; }

    /// <summary>驳回时填写的原因。</summary>
    public string? RejectReason { get; set; }
}
