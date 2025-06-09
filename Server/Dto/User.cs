using SqlSugar;
using System.ComponentModel.DataAnnotations;

namespace Dto
{
    /// <summary>
    /// 用户信息数据传输对象
    /// </summary>
    public class User
    {
        /// <summary>
        /// 用户标识
        /// </summary>
        public string sid { get; set; }
        /// <summary>
        /// 员工姓名
        /// </summary>
        public string name { get; set; }
        /// <summary>
        /// 手机号码
        /// </summary>
        public string mobile { get; set; }
        /// <summary>
        /// 生成时间
        /// </summary>
        public long time_create { get; set; }
        /// <summary>
        /// 最近登录时间
        /// </summary>
        public long time_login_last { get; set; }
    }
}
