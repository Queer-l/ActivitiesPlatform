using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using CampusActivityApi.Models;
using CampusActivityApi.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using CampusActivityApi.Common;
using Microsoft.AspNetCore.Authorization;

namespace CampusActivityApi.Controllers
{
    [Authorize] // 必须登录才能使用签到功能
    [ApiController]
    [Route("api/[controller]")]
    /// <summary>
    /// 活动签到码生成、扫码签到和签到名单查询接口。
    /// </summary>
    public class SignInController : ControllerBase
    {
        private readonly SignInService _signInService;
        private readonly ActivityService _activityService;

        /// <summary>
        /// 创建签到控制器。
        /// </summary>
        /// <param name="signInService">签到业务服务。</param>
        /// <param name="activityService">活动查询服务。</param>
        public SignInController(SignInService signInService, ActivityService activityService)
        {
            _signInService = signInService;
            _activityService = activityService;
        }

        /// <summary>
        /// 活动负责人生成签到码，前端可据此渲染二维码。
        /// </summary>
        /// <param name="activityId">需要生成签到码的活动 ID。</param>
        /// <returns>十位签到码。</returns>
        [HttpPost("GenerateSignInCode")]
        public IActionResult GenerateSignInCode(int activityId)
        {
            var theActivity = _activityService.GetPassedActivities().FirstOrDefault(act => act.Id == activityId);
            if (theActivity == null)
            {
                return Ok(ResultModel<string>.Fail("活动不存在！"));
            }
            // 从当前登录用户的Token中获取用户名
            var userName = User.Identity?.Name;
            if (string.IsNullOrEmpty(userName))
            {
                return Ok(ResultModel<string>.Fail("用户未登录！"));
            }
            if(theActivity.PublisherUserName!=userName)
            {
                return Ok(ResultModel<string>.Fail("只有负责人可以生成该活动的签到二维码！"));
            }
            var code = _signInService.GenerateSignInCode(activityId);
            return Ok(ResultModel<string>.Success(code));
        }

        /// <summary>
        /// 当前登录用户使用签到码完成签到。
        /// </summary>
        /// <param name="code">签到码内容。</param>
        /// <returns>签到业务结果。</returns>
        [HttpPost("ScanSignIn")]
        public IActionResult ScanSignIn(string code)
        {
            // 从当前登录用户的Token中获取用户名
            var userName = User.Identity?.Name;
            if (string.IsNullOrEmpty(userName))
            {
                return Ok(ResultModel<string>.Fail("用户未登录"));
            }

            var resultMsg = _signInService.ScanSignIn(code, userName);

            if (resultMsg == "签到成功")
                return Ok(ResultModel<string>.Success(resultMsg));
            else
                return Ok(ResultModel<string>.Fail(resultMsg));
        }

        /// <summary>
        /// 查询指定活动的签到名单。
        /// </summary>
        /// <param name="activityId">活动 ID。</param>
        /// <returns>签到记录列表。</returns>
        [HttpGet("GetSignInList")]
        public IActionResult GetSignInList(int activityId)
        {
            var list = _signInService.GetSignInList(activityId);
            return Ok(ResultModel<List<SignIn>>.Success(list));
        }
    }

}
