using CampusActivityApi.Common;
using CampusActivityApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace CampusActivityApi.Controllers;

[ApiController]
[Route("api/[controller]")]
/// <summary>
/// 活动报名与取消报名接口。
/// </summary>
public class EnrollController : ControllerBase
{
    private readonly EnrollService _enrollService;

    /// <summary>
    /// 创建报名控制器。
    /// </summary>
    /// <param name="enrollService">报名业务服务。</param>
    public EnrollController(EnrollService enrollService)
    {
        _enrollService = enrollService;
    }

    /// <summary>
    /// 为用户报名指定活动。
    /// </summary>
    /// <param name="userId">报名用户的用户名或学工号。</param>
    /// <param name="activityId">被报名活动 ID。</param>
    /// <returns>报名业务结果。</returns>
    [HttpPost("DoEnroll")]
    public IActionResult DoEnroll(string userId, int activityId)
    {
        string res = _enrollService.DoEnroll(userId, activityId);
        if (res == "报名成功")
        {
            return Ok(ResultModel<string>.Success(res));
        }
        else
        {
            return Ok(ResultModel<string>.Fail(res));
        }
    }



    /// <summary>
    /// 当前登录用户取消自己的活动报名。
    /// </summary>
    /// <param name="activityId">需要取消报名的活动 ID。</param>
    /// <returns>取消报名结果。</returns>
    [Microsoft.AspNetCore.Authorization.Authorize]
    [HttpPost("CancelEnroll")]
    public IActionResult CancelEnroll(int activityId)
    {
        var userName = User.Identity?.Name;
        if (string.IsNullOrEmpty(userName))
            return Ok(ResultModel<string>.Fail("用户未登录"));

        var res = _enrollService.CancelEnroll(activityId, userName);
        return Ok(ResultModel<string>.Success(res));
    }











}
