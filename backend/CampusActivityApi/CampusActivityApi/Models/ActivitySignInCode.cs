namespace CampusActivityApi.Models
{

    /// <summary>
    /// 活动签到码
    /// </summary>
    public class ActivitySignInCode
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;       // 随机签到码（二维码内容）
        public int ActivityId { get; set; }   // 所属活动ID
        public DateTime ExpireTime { get; set; } // 过期时间
    }


}
