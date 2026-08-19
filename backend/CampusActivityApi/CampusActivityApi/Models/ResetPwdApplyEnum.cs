namespace CampusActivityApi.Models;

/// <summary>
/// 密码重置申请审核状态。
/// </summary>
public enum ResetPwdApplyEnum
{
    /// <summary>
    /// 待审核：申请已提交，管理员尚未处理。
    /// </summary>
    Pending = 0,

    /// <summary>
    /// 通过：管理员同意重置密码。
    /// </summary>
    Passed = 1,

    /// <summary>
    /// 拒绝：管理员拒绝重置密码申请。
    /// </summary>
    Rejected = 2
}
