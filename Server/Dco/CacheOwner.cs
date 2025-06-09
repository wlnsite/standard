using SqlSugar;
using System.ComponentModel.DataAnnotations;

namespace Dco
{
    /// <summary>
    /// 收款商户号
    /// </summary>
    public class CacheOwner
    {
        /// <summary>
        /// 主体号
        /// </summary>
        public int id { get; set; }
        /// <summary>
        /// 机构端通知地址
        /// </summary>
        public string owner_service_url { get; set; }
        /// <summary>
        /// 机构端公钥
        /// </summary>
        public byte[] owner_public_key { get; set; }
        /// <summary>
        /// 平台端私钥
        /// </summary>
        public byte[] platform_private_key { get; set; }
    }
}
