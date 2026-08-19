using CampusActivityApi.Common;
using CampusActivityApi.Models;
using CampusActivityApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CampusActivityApi.Controllers;

[ApiController]
[Route("api/[controller]")]
/// <summary>
/// 活动申请、审核、发布展示和留言管理接口。
/// </summary>
public class ActivityController : ControllerBase
{
    private readonly ActivityService _actService;

    /// <summary>
    /// 创建活动控制器。
    /// </summary>
    /// <param name="actService">活动业务服务。</param>
    public ActivityController(ActivityService actService)
    {
        _actService = actService;
    }


    /// <summary>
    /// 提交活动申请，保存后进入待审核状态。
    /// </summary>
    /// <param name="activity">前端提交的活动基本信息和时间配置。</param>
    /// <returns>统一业务响应，返回申请提交结果。</returns>
    [HttpPost("ApplyActivity")]
    public IActionResult Apply(MyActivity activity)
    {
        var res = _actService.ApplyActivity(activity);
        return Ok(res.Contains("成功")
            ? ResultModel<string>.Success(res)
            : ResultModel<string>.Fail(res));
    }

    /// <summary>
    /// 查询管理员可审核的活动申请。
    /// </summary>
    /// <returns>待审核或已驳回的活动申请列表。</returns>
    [Authorize]  //要求授权
    [HttpGet("GetActivityApplies")]
    public IActionResult GetApplies()
    {
        // 从Token解析当前登录用户的角色
        var currentRole = User.Claims.FirstOrDefault(c => c.Type == "Role")?.Value;
        if (!int.TryParse(currentRole, out int Therolenum))
        {
            return Ok(ResultModel<string>.Fail("权限异常"));
        }


        // 权限判断
        if (Therolenum != 2 && Therolenum != 3)
        {
            return Ok(ResultModel<string>.Fail("权限不足！"));
        }

        return Ok(_actService.GetAllApplies());
    }

    /// <summary>
    /// 管理员审核活动申请。
    /// </summary>
    /// <param name="id">待处理活动申请 ID。</param>
    /// <param name="isPass">是否通过审核。</param>
    /// <param name="rejectReason">驳回时填写的理由。</param>
    /// <returns>审核处理结果。</returns>
    [Authorize]  //要求授权
    [HttpPost("CheckActivityApplies")]
    public IActionResult Check(int id, bool isPass, string? rejectReason)
    {
        // 从Token解析当前登录用户的角色
        var currentRole = User.Claims.FirstOrDefault(c => c.Type == "Role")?.Value;
        if (!int.TryParse(currentRole, out int Therolenum))
        {
            return Ok(ResultModel<string>.Fail("权限异常"));
        }


        // 权限判断
        if (Therolenum != 2&&Therolenum != 3)
        {
            return Ok(ResultModel<string>.Fail("权限不足！"));
        }

        return Ok(_actService.CheckActivity(id, isPass, rejectReason));
    }



    /// <summary>
    /// 当前登录用户取消自己的未发布活动申请。
    /// </summary>
    /// <param name="applyId">需要取消的申请 ID。</param>
    /// <returns>取消处理结果。</returns>
    [Authorize]
    [HttpPost("CancelActivityApply")]
    public IActionResult CancelApply(int applyId)
    {
        var userName = User.Identity?.Name;
        if (string.IsNullOrEmpty(userName))
            return Ok(ResultModel<string>.Fail("用户未登录"));

        return Ok(_actService.CancelActivityApply(applyId, userName));
      
    }

    /// <summary>
    /// 下架已发布活动。
    /// </summary>
    /// <param name="activityId">需要下架的活动 ID。</param>
    /// <returns>下架处理结果。</returns>
    [Authorize]
    [HttpPost("TakeDownActivity")]
    public IActionResult TakeDownActivity(int activityId)
    {
        var userName = User.Identity?.Name;
        if (string.IsNullOrEmpty(userName))
            return Ok(ResultModel<string>.Fail("用户未登录"));

        var roleStr = User.Claims.FirstOrDefault(c => c.Type == "Role")?.Value;
        int.TryParse(roleStr, out int role);

        return Ok(_actService.TakeDownActivity(activityId, userName, role));
   
    }

    /// <summary>
    /// 获取审核通过并可对外展示的活动列表。
    /// </summary>
    /// <returns>已发布活动列表。</returns>
    [HttpGet("GetActivitysList")]
    public IActionResult GetActivitysList()
    {
        return Ok(_actService.GetPassedActivities());
    }

    /// <summary>
    /// 为指定活动发表留言。
    /// </summary>
    /// <param name="activityId">留言所属活动 ID。</param>
    /// <param name="content">留言正文。</param>
    /// <returns>留言发布结果。</returns>
    [Authorize]
    [HttpPost("AddMessage")]
    public IActionResult AddMessage(int activityId, string content)
    {
        var userName = User.Identity?.Name;
        if (string.IsNullOrEmpty(userName))
            return Ok(ResultModel<string>.Fail("用户未登录"));

        var msg = _actService.AddMessage(activityId, userName, content);
        if (msg.Contains("成功"))
            return Ok(ResultModel<string>.Success(msg));
        return Ok(ResultModel<string>.Fail(msg));
    }

    /// <summary>
    /// 删除指定活动留言。
    /// </summary>
    /// <param name="activityId">留言所属活动 ID。</param>
    /// <param name="messageId">需要删除的留言 ID。</param>
    /// <returns>留言删除结果。</returns>
    [Authorize]
    [HttpPost("DeleteMessage")]
    public IActionResult DeleteMessage(int activityId, int messageId)
    {
        var userName = User.Identity?.Name;
        if (string.IsNullOrEmpty(userName))
            return Ok(ResultModel<string>.Fail("用户未登录"));

        int.TryParse(User.Claims.FirstOrDefault(c => c.Type == "Role")?.Value, out int role);
        var res = _actService.DeleteMessage(activityId, messageId, userName, role);
        return Ok(res.Contains("成功") ? ResultModel<string>.Success(res) : ResultModel<string>.Fail(res));
    }

    /// <summary>
    /// 获取指定活动的留言列表。
    /// </summary>
    /// <param name="activityId">活动 ID。</param>
    /// <returns>按创建时间倒序排列的留言列表。</returns>
    [HttpGet("GetMessagesByActivity")]
    public IActionResult GetMessages(int activityId)
    {
        var list = _actService.GetMessagesByActivity(activityId);
        return Ok(ResultModel<List<Message>>.Success(list));
    }




}
