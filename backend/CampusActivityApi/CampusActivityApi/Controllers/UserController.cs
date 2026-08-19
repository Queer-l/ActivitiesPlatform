using CampusActivityApi.Common;
using CampusActivityApi.Services;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.Metrics;
using System.Diagnostics;
using System.Threading;
using CampusActivityApi.Models;
using Microsoft.AspNetCore.Authorization;
using System.Text.RegularExpressions;

namespace CampusActivityApi.Controllers;

[ApiController]
[Route("api/[controller]")]
/// <summary>
/// 用户登录、邮箱绑定、密码重置和管理员用户管理接口。
/// </summary>
public class UserController : ControllerBase
{
    private readonly UserService _userService;

    /// <summary>
    /// 创建用户控制器。
    /// </summary>
    /// <param name="userService">用户业务服务。</param>
    public UserController(UserService userService)
    {
        _userService = userService;
    }

    /// <summary>
    /// 使用用户名和密码登录系统。
    /// </summary>
    /// <param name="userName">用户名或学工号。</param>
    /// <param name="pwd">登录密码。</param>
    /// <returns>登录成功时返回 JWT、用户名和角色。</returns>
    [HttpPost("LoginByPwd")]
    public IActionResult LoginByPwd([FromForm] string userName, [FromForm] string pwd)
    {
        var user = _userService.LoginByPwd(userName, pwd);

        if (user == null)
        {
            return Ok(ResultModel<string>.Fail("账号或密码错误"));
        }

        if(!user.IsActive)
        {
            return Ok(ResultModel<string>.Fail("账号已禁用"));
        }

        // 登录成功后生成带角色声明的 Token，供后续接口鉴权使用。
        var token = JwtHelper.GenerateToken(user.UserName, user.Role);

        // 同时返回 Token 和 Role，方便前端保存登录态并按角色跳转页面。
        return Ok(new
        {
            Code = 200,
            Msg = "登录成功",
            Data = new
            {
                Token = token,
                UserName = user.UserName,
                Role = user.Role // 学生/普通管理员/超级管理员
            }
        });

    }



    /// <summary>
    /// 为指定用户绑定邮箱。
    /// </summary>
    /// <param name="userName">用户名或学工号。</param>
    /// <param name="email">待绑定邮箱。</param>
    /// <param name="code">邮箱验证码。</param>
    /// <returns>绑定结果。</returns>
    [HttpPost("BindEmail")]
    public IActionResult BindEmail(string userName, string email, string code)
    {
        var res = _userService.BindEmail(userName, email, code);
        return Ok(ResultModel<string>.Success(res));
    }




    /// <summary>
    /// 向指定邮箱发送验证码。
    /// </summary>
    /// <param name="email">接收验证码的邮箱地址。</param>
    /// <returns>发送结果。</returns>
    [HttpPost("SendEmailCode")]
    public IActionResult SendEmailCode(string email)
    {
        return Ok(ResultModel<string>.Success(_userService.SendEmailCode(email)));
    }


    /// <summary>
    /// 使用邮箱验证码登录。
    /// </summary>
    /// <param name="email">已绑定邮箱。</param>
    /// <param name="code">邮箱验证码。</param>
    /// <returns>登录结果。</returns>
    [HttpPost("LoginByEmail")]
    public IActionResult LoginByEmail(string email, string code)
    {
    var user = _userService.LoginByEmail(email, code);

    if (user == null)
    {
        return Ok(ResultModel<string>.Fail("该邮箱未被任何用户绑定"));
    }

    if (!user.IsActive)
    {
        return Ok(ResultModel<string>.Fail("账号已禁用"));
    }

    //生成带角色的Token
    var token = JwtHelper.GenerateToken(user.UserName, user.Role);
    //返回给前端
    return Ok(new
    {
        Code = 200,
        Msg = "登录成功",
        Data = new
        {
            Token = token,
            UserName = user.UserName,
            Role = user.Role // 学生/普通管理员/超级管理员
        }
    });
    }



    /// <summary>
    /// 通过邮箱验证码重置密码。
    /// </summary>
    /// <param name="email">已绑定邮箱。</param>
    /// <param name="code">邮箱验证码。</param>
    /// <param name="newPwd">新密码。</param>
    /// <returns>密码重置结果。</returns>
    [HttpPost("ResetPasswordByEmail")]
    public IActionResult ResetPasswordByEmail(string email, string code, string newPwd)
    {
        var res = _userService.ResetPasswordByEmail(email, code, newPwd);
        return Ok(ResultModel<string>.Success(res));
    }





    /// <summary>
    /// 提交人工密码重置申请。
    /// </summary>
    /// <param name="userName">申请人用户名或学工号。</param>
    /// <param name="realName">申请人真实姓名。</param>
    /// <param name="reason">申请理由。</param>
    /// <returns>申请提交结果。</returns>
    [HttpPost("ApplyResetPwd")]
    public IActionResult ApplyResetPwd(string userName, string realName, string reason)
    {
        var res = _userService.ApplyResetPwd(userName, realName, reason);
        return Ok(ResultModel<string>.Success(res));
    }






    /// <summary>
    /// 超级管理员添加新用户。
    /// </summary>
    /// <param name="userName">新用户用户名或学工号。</param>
    /// <param name="realName">新用户真实姓名。</param>
    /// <param name="role">新用户角色。</param>
    /// <param name="email">可选绑定邮箱。</param>
    /// <returns>添加用户结果。</returns>
    [Authorize]  //要求授权
    [HttpPost("AddUser")]
    public IActionResult AddUser(string userName, string realName, int role, string? email=null)
    {

        // 从 Token 解析当前登录用户的角色，确保只有超级管理员能添加用户。
        var currentRole = User.Claims.FirstOrDefault(c => c.Type == "Role")?.Value;
        if (!int.TryParse(currentRole, out int Therolenum))
        {
            return Ok(ResultModel<string>.Fail("权限异常"));
        }

     
        // 权限判断
        if (Therolenum!=3)
        {
            return Ok(ResultModel<string>.Fail("只有超级管理员才能添加用户！"));
        }

        // 限制角色值只能为系统约定的 1、2、3。
        if (Therolenum is not 1 and not 2 and not 3)
        {
            return Ok(ResultModel<string>.Fail("非法角色，无法操作"));
        }



        // 邮箱可选；如果填写，则先做基础格式校验再落库。
        if (!string.IsNullOrEmpty(email))
        {
            string emailRegex = @"^[a-zA-Z0-9_.-]+@[a-zA-Z0-9-]+\.[a-zA-Z0-9-.]+$";
            if (!Regex.IsMatch(email, emailRegex))
            {
                return Ok(ResultModel<string>.Fail("邮箱格式不正确！"));
            }
        }

        var res = _userService.AddUser(userName, realName, role,email);
        return Ok(ResultModel<string>.Success(res));
    }




    /// <summary>
    /// 管理员编辑用户邮箱或角色。
    /// </summary>
    /// <param name="userName">目标用户用户名或学工号。</param>
    /// <param name="email">新的邮箱地址；为空表示不修改。</param>
    /// <param name="role">新的角色；为空表示不修改。</param>
    /// <returns>编辑结果。</returns>
    [Authorize]
    [HttpPost("AdminEditUser")]
    public IActionResult AdminEditUser(
        string userName,
        string? email = null,
        int? role = null)
    {
        // 从 Token 解析当前登录用户角色，用于执行管理员权限校验。
        var currentRole = User.Claims.FirstOrDefault(c => c.Type == "Role")?.Value;
        if (!int.TryParse(currentRole, out int Therolenum))
        {
            return Ok(ResultModel<string>.Fail("权限异常"));
        }

        // 只接受系统约定角色值，避免非法角色进入业务逻辑。
        if (Therolenum is not 1 and not 2 and not 3)
        {
            return Ok(ResultModel<string>.Fail("非法角色，无法操作"));
        }

        // 权限判断
        if (Therolenum!=2&&Therolenum!=3)
        {
            return Ok(ResultModel<string>.Fail("只有管理员才能修改！"));
        }
        var msg = _userService.AdminEditUser(userName, email, role);
        return Ok(ResultModel<string>.Success(msg));
    }


    /// <summary>
    /// 获取密码重置申请列表。
    /// </summary>
    /// <returns>密码重置申请集合。</returns>
    [Authorize]
    [HttpGet("GetPwdResetApplies")]
    public IActionResult GetPwdResetApplies() => Ok(_userService.GetApplies());


    /// <summary>
    /// 管理员处理密码重置申请。
    /// </summary>
    /// <param name="id">申请 ID。</param>
    /// <param name="isPass">是否通过。</param>
    /// <param name="rejectReason">拒绝时填写的原因。</param>
    /// <returns>处理结果。</returns>
    [Authorize]
    [HttpPost("HandlePwdResetApply")]
    public IActionResult HandlePwdResetApply(int id, bool isPass, string? rejectReason) => Ok(_userService.HandleApply(id,isPass, rejectReason)); 

    /// <summary>
    /// 获取系统用户列表。
    /// </summary>
    /// <returns>用户集合。</returns>
    [Authorize]
    [HttpGet("GetUsersList")]
    public IActionResult GetUsersList() => Ok(_userService.GetUsers());




}



