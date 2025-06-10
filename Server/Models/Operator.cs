using SqlSugar;

namespace Models
{
    /// <summary>
    /// 操作人员
    /// </summary>
    [SugarTable(SqlContext.Perfix + "operator")]
    public class Operator
    {
        /// <summary>
        /// 记录标识
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public int id { get; set; }
        /// <summary>
        /// 用户标识
        /// </summary>
        [SugarColumn(Length = 50)]
        public string sid { get; set; }
        /// <summary>
        /// 授权角色 manger/operator
        /// </summary>
        [SugarColumn(Length = 10, IsNullable = true)]
        public string role { get; set; }
        /// <summary>
        /// 用户标识
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public string authority { get; set; }
        /// <summary>
        /// 主体标识
        /// </summary>
        public int owner { get; set; }
        /// <summary>
        /// 记录状态 1：有效 0：禁用 -1:删除
        /// </summary>
        public int state { get; set; }
        /// <summary>
        /// 授权时间
        /// </summary>
        public long time_create { get; set; }
    }
}