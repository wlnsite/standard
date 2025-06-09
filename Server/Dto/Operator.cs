using SqlSugar;

namespace Dto
{
    /// <summary>
    /// 操作员数据传输对象
    /// </summary>
    public class Operator
    {
        /// <summary>
        /// 用户sid
        /// </summary>
        public string sid { get; set; }
        /// <summary>
        /// 员工角色
        /// </summary>
        public string role { get; set; }
        /// <summary>
        /// 员工姓名
        /// </summary>
        public string name { get; set; }
        /// <summary>
        /// 手机号码
        /// </summary>
        public string mobile { get; set; }
        /// <summary>
        /// 主体标识
        /// </summary>
        public int owner { get; set; }
        /// <summary>
        /// 记录状态 0：未启用 1：已启用
        /// </summary>
        public int state { get; set; }
        /// <summary>
        /// 录入时间
        /// </summary>
        public long time_create { get; set; }
    }
}
