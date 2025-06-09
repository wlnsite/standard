using SqlSugar;
namespace Dto
{
    /// <summary>
    /// 交易记录
    /// </summary>
    public class Contract
    {
        /// <summary>
        /// 业务系统退款流水号
        /// </summary>
        public string id { get; set; }
        /// <summary>
        /// 购买方Id
        /// </summary>
        public int buyer { get; set; }
        /// <summary>
        /// 购买方Id
        /// </summary>
        public string buyer_name { get; set; }
        /// <summary>
        /// 供应商Id
        /// </summary>
        public int supplier { get; set; }
        /// <summary>
        /// 供应商Id
        /// </summary>
        public string supplier_name { get; set; }
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

    }
}