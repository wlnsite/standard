using SqlSugar;
using System.ComponentModel.DataAnnotations;

namespace Dco
{
    /// <summary>
    /// 机构证书密钥缓存对象
    /// </summary>
    public class CacheCertKey
    {
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
        /// 平台端私钥
        /// </summary>
        public byte[] server_key { get; set; }
        /// <summary>
        /// 机构端公钥
        /// </summary>
        public byte[] public_key { get; set; }
        /// <summary>
        /// 机构端私钥
        /// </summary>
        public byte[] private_key { get; set; }
        /// <summary>
        /// 密钥到期时间
        /// </summary>
        public long time_expire { get; set; }
    }
}
