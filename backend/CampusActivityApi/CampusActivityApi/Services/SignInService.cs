using CampusActivityApi.Data;
using CampusActivityApi.Models;

namespace CampusActivityApi.Services;

/// <summary>
/// 活动签到码和签到记录业务服务。
/// </summary>
public class SignInService
{
    private readonly AppDbContext _db;

    /// <summary>
    /// 创建签到服务。
    /// </summary>
    /// <param name="db">数据库上下文。</param>
    public SignInService(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// 为活动生成新的十位签到码，并移除该活动旧签到码。
    /// </summary>
    /// <param name="activityId">活动 ID。</param>
    /// <returns>新生成的签到码。</returns>
    public string GenerateSignInCode(int activityId)
    {
        var code = Guid.NewGuid().ToString("N")[..10];
        var oldCodes = _db.ActivitySignInCodes.Where(c => c.ActivityId == activityId);

        _db.ActivitySignInCodes.RemoveRange(oldCodes);
        _db.ActivitySignInCodes.Add(new ActivitySignInCode
        {
            Code = code,
            ActivityId = activityId,
            ExpireTime = DateTime.Now.AddMinutes(10)
        });
        _db.SaveChanges();

        return code;
    }

    /// <summary>
    /// 使用签到码完成签到。
    /// </summary>
    /// <param name="code">签到码。</param>
    /// <param name="userName">当前登录用户名。</param>
    /// <returns>签到业务结果。</returns>
    public string ScanSignIn(string code, string userName)
    {
        var signInCode = _db.ActivitySignInCodes.FirstOrDefault(c => c.Code == code);
        if (signInCode == null)
            return "签到码无效，请刷新二维码";

        if (signInCode.ExpireTime < DateTime.Now)
            return "签到码已过期，请管理员刷新二维码";

        var activity = _db.Activities.FirstOrDefault(a =>
            a.Id == signInCode.ActivityId && a.AuditStatus == AuditStatusEnum.Passed);

        if (activity == null)
            return "活动不存在";

        DateTime now = DateTime.Now;
        if (now < activity.ActivityStartTime || now > activity.ActivityEndTime)
            return "不在活动签到时间范围内，无法签到";

        bool hasEnrolled = _db.Enrolls.Any(e =>
            e.ActivityId == signInCode.ActivityId && e.UserName == userName);

        if (!hasEnrolled)
            return "您未报名该活动，无法签到";

        bool alreadySigned = _db.SignIns.Any(r =>
            r.ActivityId == signInCode.ActivityId && r.UserName == userName);

        if (alreadySigned)
            return "你已完成签到，请勿重复操作";

        _db.SignIns.Add(new SignIn
        {
            ActivityId = signInCode.ActivityId,
            UserName = userName,
            SignInTime = DateTime.Now
        });

        activity.SignInCount++;
        _db.SaveChanges();

        return "签到成功";
    }

    /// <summary>
    /// 查询指定活动的签到记录。
    /// </summary>
    /// <param name="activityId">活动 ID。</param>
    /// <returns>签到记录列表，按签到时间倒序排列。</returns>
    public List<SignIn> GetSignInList(int activityId)
    {
        return _db.SignIns
            .Where(r => r.ActivityId == activityId)
            .OrderByDescending(r => r.SignInTime)
            .ToList();
    }
}
