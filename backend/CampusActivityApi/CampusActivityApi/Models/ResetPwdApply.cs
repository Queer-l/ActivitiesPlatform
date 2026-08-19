namespace CampusActivityApi.Models;

/// <summary>
/// 密码重置申请实体。
/// </summary>
/// <remarks>
/// 用户无法通过邮箱自助重置时，可提交申请，由管理员审核后重置密码。
/// </remarks>
public class ResetPwdApply
{
    /// <summary>
    /// 申请记录主键，自增 ID。
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 申请重置密码的用户名或学工号。
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// 申请人填写的真实姓名，用于管理员核对身份。
    /// </summary>
    public string RealName { get; set; } = string.Empty;

    /// <summary>
    /// 用户提交的申请理由。
    /// </summary>
    public string Reason { get; set; } = string.Empty;

    /// <summary>
    /// 申请审核状态：待审核、通过或拒绝。
    /// </summary>
    public ResetPwdApplyEnum Status { get; set; } = ResetPwdApplyEnum.Pending;

    /// <summary>
    /// 拒绝理由；仅当申请被拒绝时填写，通过或待审核时为空。
    /// </summary>
    public string? RejectReason { get; set; }
}
