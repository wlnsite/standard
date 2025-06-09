using SqlSugar;
using System.ComponentModel.DataAnnotations;

namespace Dto
{
    /// <summary>
    /// 机构证书数据传输实体
    /// </summary>
    public class CertKey
    {
        /// <summary>
        /// 证书序列号
        /// </summary>
        public string sn { get; set; }
        /// <summary>
        /// 记录状态 0：未启用 1：已启用
        /// </summary>
        public int state { get; set; }
        /// <summary>
        /// 所属机构
        /// </summary>
        public int owner { get; set; }
        /// <summary>
        /// 定向使用的合约列表
        /// </summary>
        public string only_to { get; set; }
        /// <summary>
        /// 机构端公钥
        /// </summary>
        public string public_key { get; set; }
        /// <summary>
        /// 机构端私钥
        /// </summary>
        public string private_key { get; set; }

        /// <summary>
        /// 密钥录入时间
        /// </summary>
        public long time_create { get; set; }
        /// <summary>
        /// 密钥到期时间
        /// </summary>
        public long time_expire { get; set; }
    }
}
