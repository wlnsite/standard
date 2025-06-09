using SqlSugar;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Wlniao;

namespace Models
{
    /// <summary>
    /// 机构证书密钥
    /// </summary>
    [SugarTable(SqlContext.Perfix + "cert_key")]
    [SugarIndex("index_owner", nameof(owner), OrderByType.Asc)]
    public class CertKey
    {
        /// <summary>
        /// 证书序号
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, Length = 50)]
        public string sn { get; set; }

        /// <summary>
        /// 记录状态 0：未启用 1：已启用 -1：已删除
        /// </summary>
        public int state { get; set; }

        /// <summary>
        /// 所属机构
        /// </summary>
        public int owner { get; set; }

        /// <summary>
        /// 证书备注
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = true)]
        public string memo { get; set; }

        /// <summary>
        /// 定向使用的合约列表
        /// </summary>
        [SugarColumn(Length = 80, IsNullable = true)]
        public string only_to { get; set; }

        /// <summary>
        /// 应用端公钥【数据验签】
        /// </summary>
        [SugarColumn(Length = 130, IsNullable = true)]
        public string public_key { get; set; }

        /// <summary>
        /// 应用端私钥【用户提供、不使用】
        /// </summary>
        [SugarColumn(Length = 70, IsNullable = true)]
        public string private_key { get; set; }

        /// <summary>
        /// 密钥录入时间
        /// </summary>
        public long time_create { get; set; }

        /// <summary>
        /// 密钥到期时间
        /// </summary>
        public long time_expire { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sn"></param>
        /// <param name="db"></param>
        /// <returns></returns>
        public static Dco.CacheCertKey GetCache(string sn, SqlContext db = null)
        {
            var dto = Wlniao.Cache.Get<Dco.CacheCertKey>("certkey_" + sn);
            if (dto == null || dto.owner <= 0)
            {
                if (db == null)
                {
                    db = new SqlContext();
                }
                var row = db.Queryable<Models.CertKey>().Where(o => o.sn == sn).First();
                if (row != null)
                {
                    dto = new Dco.CacheCertKey { state = row.state, owner = row.owner, only_to = row.only_to, time_expire = row.time_expire };
                    var server_key = db.Queryable<Models.OwnerSysInfo>().Where(o => o.id == dto.owner).Select(o => o.private_key).First();
                    if (!string.IsNullOrEmpty(server_key))
                    {
                        dto.server_key = Wlniao.Crypto.Helper.Decode(server_key);
                    }
                    if (!string.IsNullOrEmpty(row.public_key))
                    {
                        dto.public_key = Wlniao.Crypto.Helper.Decode(row.public_key);
                    }
                    if (!string.IsNullOrEmpty(row.private_key))
                    {
                        dto.private_key = Wlniao.Crypto.Helper.Decode(row.private_key);
                    }
                    Wlniao.Cache.Set<Dco.CacheCertKey>("certkey_" + sn, dto, 86400);
                }
            }
            return dto;
        }
    }
}