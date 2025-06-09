using SqlSugar;

namespace Models
{
    /// <summary>
    /// 人员账号
    /// </summary>
    [SugarTable(SqlContext.Perfix + "user")]
    [SugarIndex("index_name", nameof(name), OrderByType.Asc)]
    [SugarIndex("index_mobile", nameof(mobile), OrderByType.Asc)]
    public class User
    {
        /// <summary>
        /// 用户标识
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, Length = 50)]
        public string sid { get; set; }
        /// <summary>
        /// 手机号码
        /// </summary>
        [SugarColumn(Length = 11)]
        public string mobile { get; set; }
        /// <summary>
        /// 员工姓名
        /// </summary>
        [SugarColumn(Length = 30, IsNullable = true)]
        public string name { get; set; }
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