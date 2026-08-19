namespace CampusActivityApi.Models
{
    /// <summary>
    /// 活动状态（固定5种，不会出现其他状态）
    /// </summary>
    public enum ActivityStatusEnum
    {
        /// <summary>
        /// 预热中：报名时间未到
        /// </summary>
        预热中,

        /// <summary>
        /// 报名中：在报名时间区间内
        /// </summary>
        报名中,

        /// <summary>
        /// 待开始：报名已结束，活动未开始
        /// </summary>
        待开始,

        /// <summary>
        /// 进行中：活动时间内
        /// </summary>
        进行中,

        /// <summary>
        /// 已结束：活动已结束
        /// </summary>
        已结束
    }
}