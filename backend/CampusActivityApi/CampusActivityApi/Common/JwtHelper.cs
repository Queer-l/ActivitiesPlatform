using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace CampusActivityApi.Common;

/// <summary>
/// JWT 生成工具类。
/// </summary>
/// <remarks>
/// 生成的 Token 会写入用户名和角色，后续控制器通过 User.Identity 和 User.Claims 读取。
/// </remarks>
public static class JwtHelper
{
    /// <summary>
    /// JWT 签名密钥；生产环境应改为安全配置项或密钥管理服务。
    /// </summary>
    private const string SecretKey = "12345678901234567890123456789012";

    /// <summary>
    /// Token 签发方标识。
    /// </summary>
    private const string Issuer = "MyApi";

    /// <summary>
    /// Token 接收方标识。
    /// </summary>
    private const string Audience = "MyClient";

    /// <summary>
    /// 根据用户身份生成 JWT。
    /// </summary>
    /// <param name="username">登录用户的用户名或学工号。</param>
    /// <param name="role">用户角色：1=普通用户，2=普通管理员，3=超级管理员。</param>
    /// <returns>可放入 Authorization Bearer 请求头的 JWT 字符串。</returns>
    public static string GenerateToken(string username, int role)
    {
        // 将用户名与角色写入声明，供授权中间件和业务接口读取。
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, username),
            new Claim("Role", role.ToString())
        };

        // 使用 HMAC SHA256 对 Token 进行签名，防止客户端篡改声明内容。
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SecretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // 课设环境采用 1 小时有效期；如需长期登录，应增加刷新 Token 机制。
        var token = new JwtSecurityToken(
            issuer: Issuer,
            audience: Audience,
            claims: claims,
            expires: DateTime.Now.AddHours(1),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
