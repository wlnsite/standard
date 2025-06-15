using SqlSugar;
using System.ComponentModel.DataAnnotations;

namespace Dto
{
    /// <summary>
    /// 收款商户号
    /// </summary>
    public class Owner
    {
        /// <summary>
        /// 主体号
        /// </summary>
        public int id { get; set; }
        /// <summary>
        /// 默认角色 buyer/supplier
        /// </summary>
        public string role { get; set; }
        /// <summary>
        /// 商户名称
        /// </summary>
        public string name { get; set; }
        /// <summary>
        /// 统一信用代码/身份证号
        /// </summary>
        public string certificate { get; set; }
        /// <summary>
        /// 证件照片
        /// </summary>
        public string cert_picture { get; set; }
        /// <summary>
        /// 生成时间
        /// </summary>
        public long time_create { get; set; }
    }
}
