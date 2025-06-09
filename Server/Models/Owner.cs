using SqlSugar;
using System.ComponentModel.DataAnnotations;

namespace Models
{
    /// <summary>
    /// 主体
    /// </summary>
    [SugarTable(SqlContext.Perfix + "owner")]
    public class Owner
    {
        /// <summary>
        /// 主体号
        /// </summary>
        [SugarColumn(IsPrimaryKey = true)]
        public int id { get; set; }
        /// <summary>
        /// 默认角色
        /// </summary>
        [SugarColumn(Length = 10)]
        public string role { get; set; }
        /// <summary>
        /// 商户名称
        /// </summary>
        [SugarColumn(Length = 50)]
        public string name { get; set; }
        /// <summary>
        /// 统一信用代码/身份证号
        /// </summary>
        [SugarColumn(Length = 25, IsNullable = true)]
        public string certificate { get; set; }
        /// <summary>
        /// 证件照片
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = true)]
        public string cert_picture { get; set; }
        /// <summary>
        /// 生成时间
        /// </summary>
        public long time_create { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="row"></param>
        /// <returns></returns>
        public Dto.Owner ConvertToDto()
        {
            return new Dto.Owner
            {
                id = this.id,
                role = this.role,
                name = this.name,
                certificate = this.certificate,
                cert_picture = this.cert_picture,
                time_create = this.time_create
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static int NewId()
        {
            var maxId = new SqlContext().Queryable<Owner>().Max(o => o.id);
            if (maxId < 270000000)
            {
                maxId = 270000000;
            }
            return maxId;
        }

    }
}