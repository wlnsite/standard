using SqlSugar;
namespace Models
{
    /// <summary>
    /// 服务合约
    /// </summary>
    [SugarTable(SqlContext.Perfix + "contract")]
    public class Contract
    {
        /// <summary>
        /// 业务系统退款流水号
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, Length = 16)]
        public string id { get; set; }
        /// <summary>
        /// 购买方Id
        /// </summary>
        public int buyer { get; set; }
        /// <summary>
        /// 供应商Id
        /// </summary>
        public int supplier { get; set; }
        /// <summary>
        /// 合约生成时间
        /// </summary>
        public long time_create { get; set; }
        /// <summary>
        /// 合约到期时间
        /// </summary>
        public long time_expire { get; set; }
        /// <summary>
        /// 购买方审核时间
        /// </summary>
        public long time_verify_buyer { get; set; }
        /// <summary>
        /// 供应商审核时间
        /// </summary>
        public long time_supplier_buyer { get; set; }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="row"></param>
        /// <returns></returns>
        public Dto.Contract ConvertToDto()
        {
            return new Dto.Contract
            {
                id = this.id,
                buyer = this.buyer,
                supplier = this.supplier,
                time_verify_buyer = this.time_verify_buyer,
                time_supplier_buyer = this.time_supplier_buyer,
                time_expire = this.time_expire,
                time_create = this.time_create
            };
        }
    }
}