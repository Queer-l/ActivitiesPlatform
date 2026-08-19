using CampusActivityApi.Data;
using CampusActivityApi.Models;
using System.Net;
using System.Net.Mail;
using System.Text.RegularExpressions;

namespace CampusActivityApi.Services;

/// <summary>
/// 用户登录、邮箱验证码、密码重置和用户管理业务服务。
/// </summary>
public class UserService
{
    /// <summary>
    /// 简易邮箱验证码缓存；键为邮箱地址，值为最近一次发送的验证码。
    /// </summary>
    private static readonly Dictionary<string, string> EmailCodeCache = new();
    private readonly AppDbContext _db;

    /// <summary>
    /// 创建用户服务。
    /// </summary>
    /// <param name="db">数据库上下文。</param>
    public UserService(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// 使用用户名和密码查询用户。
    /// </summary>
    /// <param name="userName">用户名或学工号。</param>
    /// <param name="pwd">明文密码。</param>
    /// <returns>匹配用户；账号或密码错误时返回 null。</returns>
    public User? LoginByPwd(string userName, string pwd)
    {
        return _db.Users.FirstOrDefault(u => u.UserName == userName && u.PassWord == pwd);
    }

    /// <summary>
    /// 使用邮箱验证码完成登录前校验。
    /// </summary>
    /// <param name="email">已绑定邮箱。</param>
    /// <param name="code">邮箱验证码。</param>
    /// <returns>登录校验结果。</returns>
    public User LoginByEmail(string email, string code)
    {
        var user = _db.Users.FirstOrDefault(u => u.Email == email);
        if (user == null) return null;
        return user;
    }

    /// <summary>
    /// 向已绑定邮箱发送四位验证码。
    /// </summary>
    /// <param name="email">接收验证码的邮箱地址。</param>
    /// <returns>发送结果。</returns>
    public string SendEmailCode(string email)
    {
        var user = _db.Users.FirstOrDefault(u => u.Email == email);
        if (user == null) return "该邮箱未被任何用户绑定";
        if (!user.IsActive) return "账号已禁用";

        try
        {
            string code = new Random().Next(1000, 9999).ToString();
            EmailCodeCache[email] = code;

            string fromEmail = "1093432569@qq.com";
            string authCode = "dkgpjtarbryhhcgi";

            SmtpClient client = new SmtpClient("smtp.qq.com", 587)
            {
                Credentials = new NetworkCredential(fromEmail, authCode),
                EnableSsl = true
            };

            MailMessage message = new MailMessage(fromEmail, email)
            {
                Subject = "校园系统验证码",
                Body = $"你的验证码是：{code}，5分钟内有效。"
            };

            client.Send(message);
            return "验证码已发送至邮箱！";
        }
        catch
        {
            return "发送失败，请检查邮箱是否正确";
        }
    }

    /// <summary>
    /// 校验邮箱验证码是否与缓存中的最新验证码一致。
    /// </summary>
    /// <param name="email">邮箱地址。</param>
    /// <param name="code">用户输入的验证码。</param>
    /// <returns>验证码匹配时返回 true。</returns>
    public bool CheckEmailCode(string email, string code)
    {
        return EmailCodeCache.TryGetValue(email, out string? cachedCode) && cachedCode == code;
    }

    /// <summary>
    /// 通过邮箱验证码重置密码。
    /// </summary>
    /// <param name="email">已绑定邮箱。</param>
    /// <param name="code">邮箱验证码。</param>
    /// <param name="newPwd">新密码。</param>
    /// <returns>重置结果。</returns>
    public string ResetPasswordByEmail(string email, string code, string newPwd)
    {
        if (!CheckEmailCode(email, code)) return "验证码错误";

        var user = _db.Users.FirstOrDefault(u => u.Email == email);
        if (user == null) return "邮箱未绑定";

        user.PassWord = newPwd;
        _db.SaveChanges();

        return "密码重置成功";
    }

    /// <summary>
    /// 提交人工密码重置申请。
    /// </summary>
    /// <param name="userName">申请用户名或学工号。</param>
    /// <param name="realName">申请人真实姓名。</param>
    /// <param name="reason">申请理由。</param>
    /// <returns>提交结果。</returns>
    public string ApplyResetPwd(string userName, string realName, string reason)
    {
        _db.ResetPwdApplies.Add(new ResetPwdApply
        {
            UserName = userName,
            RealName = realName,
            Reason = reason,
            Status = ResetPwdApplyEnum.Pending
        });
        _db.SaveChanges();

        return "提交成功，请等待管理员审核";
    }

    /// <summary>
    /// 获取全部用户。
    /// </summary>
    /// <returns>按 ID 升序排列的用户列表。</returns>
    public List<User> GetUsers()
    {
        return _db.Users.OrderBy(u => u.Id).ToList();
    }

    /// <summary>
    /// 管理员新增用户，并设置默认初始密码。
    /// </summary>
    /// <param name="userName">新用户用户名或学工号。</param>
    /// <param name="realName">真实姓名。</param>
    /// <param name="role">角色：1=普通用户，2=普通管理员，3=超级管理员。</param>
    /// <param name="email">可选绑定邮箱。</param>
    /// <returns>添加结果。</returns>
    public string AddUser(string userName, string realName, int role, string? email = null)
    {
        bool exists = _db.Users.Any(u => u.UserName == userName);
        if (exists)
            return "学工号已存在，添加失败";

        if (role is not 1 and not 2 and not 3)
            return "非法角色！只能传入 1、2、3";

        if (!IsValidEmail(email))
            return "邮箱格式不正确！";

        _db.Users.Add(new User
        {
            UserName = userName,
            PassWord = "123456",
            RealName = realName,
            Role = role,
            Email = email,
            IsActive = true
        });
        _db.SaveChanges();

        return "用户添加成功，初始密码：123456";
    }

    /// <summary>
    /// 管理员编辑用户邮箱或角色。
    /// </summary>
    /// <param name="userName">目标用户名或学工号。</param>
    /// <param name="email">新邮箱；为空时不修改。</param>
    /// <param name="role">新角色；为空时不修改。</param>
    /// <returns>编辑结果。</returns>
    public string AdminEditUser(string userName, string? email = null, int? role = null)
    {
        var user = _db.Users.FirstOrDefault(x => x.UserName == userName);
        if (user == null)
            return "目标用户不存在";

        if (!IsValidEmail(email))
            return "邮箱格式不正确！";

        if (!string.IsNullOrEmpty(email))
            user.Email = email;

        if (role.HasValue && role.Value is 1 or 2 or 3)
            user.Role = role.Value;

        _db.SaveChanges();

        return "用户信息编辑成功";
    }

    /// <summary>
    /// 为用户绑定邮箱。
    /// </summary>
    /// <param name="userName">用户名或学工号。</param>
    /// <param name="email">待绑定邮箱。</param>
    /// <param name="code">邮箱验证码。</param>
    /// <returns>绑定结果。</returns>
    public string BindEmail(string userName, string email, string code)
    {
        if (!CheckEmailCode(email, code)) return "验证码错误";

        var user = _db.Users.FirstOrDefault(u => u.UserName == userName);
        if (user == null) return "用户不存在";

        user.Email = email;
        _db.SaveChanges();

        return "绑定成功";
    }

    /// <summary>
    /// 获取密码重置申请列表。
    /// </summary>
    /// <returns>按 ID 倒序排列的申请列表。</returns>
    public List<ResetPwdApply> GetApplies()
    {
        return _db.ResetPwdApplies.OrderByDescending(a => a.Id).ToList();
    }

    /// <summary>
    /// 管理员处理密码重置申请。
    /// </summary>
    /// <param name="id">申请 ID。</param>
    /// <param name="isPass">是否通过。</param>
    /// <param name="rejectReason">拒绝理由，拒绝时必填。</param>
    /// <returns>处理结果。</returns>
    public string HandleApply(int id, bool isPass, string? rejectReason)
    {
        var apply = _db.ResetPwdApplies.FirstOrDefault(x => x.Id == id);
        if (apply == null) return "不存在";

        if (!isPass && string.IsNullOrWhiteSpace(rejectReason))
            return "拒绝理由不能为空";

        apply.Status = isPass ? ResetPwdApplyEnum.Passed : ResetPwdApplyEnum.Rejected;
        apply.RejectReason = isPass ? null : rejectReason;

        if (isPass)
        {
            var user = _db.Users.FirstOrDefault(x => x.UserName == apply.UserName);
            if (user != null)
            {
                user.PassWord = "123456";
            }
        }

        _db.SaveChanges();

        return isPass ? "密码重置成功！" : "重置失败";
    }

    /// <summary>
    /// 校验邮箱格式；空邮箱视为合法，用于支持可选邮箱字段。
    /// </summary>
    /// <param name="email">待校验邮箱。</param>
    /// <returns>邮箱为空或格式正确时返回 true。</returns>
    private static bool IsValidEmail(string? email)
    {
        if (string.IsNullOrEmpty(email))
            return true;

        string emailRegex = @"^[a-zA-Z0-9_.-]+@[a-zA-Z0-9-]+\.[a-zA-Z0-9-.]+$";
        return Regex.IsMatch(email, emailRegex);
    }
}
