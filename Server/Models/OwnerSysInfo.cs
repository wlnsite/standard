using SqlSugar;
using System.ComponentModel.DataAnnotations;

namespace Models
{
    /// <summary>
    /// 主体系统资料
    /// </summary>
    [SugarTable(SqlContext.Perfix + "owner_sysinfo")]
    public class OwnerSysInfo
    {
        /// <summary>
        /// 主体号
        /// </summary>
        public int id { get; set; }
        /// <summary>
        /// 法定代表人：姓名
        /// </summary>
        [SugarColumn(Length = 30, IsNullable = true)]
        public string legal_name { get; set; }
        /// <summary>
        /// 法定代表人：证件号
        /// </summary>
        [SugarColumn(Length = 30, IsNullable = true)]
        public string legal_idcard { get; set; }
        /// <summary>
        /// 法定代表人：手机号码
        /// </summary>
        [SugarColumn(Length = 30, IsNullable = true)]
        public string legal_mobile { get; set; }
        /// <summary>
        /// 通知服务地址
        /// </summary>
        [SugarColumn(Length = 100, IsNullable = true)]
        public string service_url { get; set; }
        /// <summary>
        /// 平台端私钥【不提供】
        /// </summary>
        [SugarColumn(Length = 70, IsNullable = true)]
        public string private_key { get; set; }
        /// <summary>
        /// 平台端公钥【仅展示】
        /// </summary>
        [SugarColumn(Length = 130, IsNullable = true)]
        public string public_key { get; set; }
        /// <summary>
        /// 生成时间
        /// </summary>
        public long time_create { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="db"></param>
        /// <returns></returns>
        public static Dco.CacheOwner GetCache(int id, SqlContext db = null)
        {
            var dto = Wlniao.Cache.Get<Dco.CacheOwner>("merchant_" + id);
            if (dto == null || dto.id<0)
            {
                if (db == null)
                {
                    db = new SqlContext();
                }
                var row = db.Queryable<Models.Owner>().Where(o => o.id == id).First();
                if (row != null)
                {
                    //dto = new Dco.CacheOwner { id = row.id, name = row.name, owner = row.owner, create_time = row.create_time };
                    //if (!string.IsNullOrEmpty(row.public_key))
                    //{
                    //    dto.server_pub = Wlniao.Crypto.Helper.Decode(row.public_key);
                    //}
                    Wlniao.Cache.Set<Dco.CacheOwner>("merchant_" + id, dto);
                }
            }
            return dto;
        }
    }
}