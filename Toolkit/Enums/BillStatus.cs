namespace Logistic.Enums
{
    public class BillStatus
    {
        /// <summary>
        /// 新下单
        /// </summary>
        public const string NewOrder_CODE = "02";
        /// <summary>
        /// 新下单
        /// </summary>
        public const string NewOrder_TEXT = "已下单";

        /// <summary>
        /// 已揽收
        /// </summary>
        public const string Collect_CODE = "03";
        /// <summary>
        /// 已揽收
        /// </summary>
        public const string Collect_TEXT = "已揽收";

        /// <summary>
        /// 分拣中
        /// </summary>
        public const string Sorting_CODE = "04";
        /// <summary>
        /// 分拣中
        /// </summary>
        public const string Sorting_TEXT = "分拣中";

        /// <summary>
        /// 转运中
        /// </summary>
        public const string Relayed_CODE = "05";
        /// <summary>
        /// 转运中
        /// </summary>
        public const string Relayed_TEXT = "转运中";

        /// <summary>
        /// 运输中
        /// </summary>
        public const string Transit_CODE = "06";
        /// <summary>
        /// 运输中
        /// </summary>
        public const string Transit_TEXT = "运输中";

        /// <summary>
        /// 派件中
        /// </summary>
        public const string Deliver_CODE = "07";
        /// <summary>
        /// 派件中
        /// </summary>
        public const string Deliver_TEXT = "派件中";

        /// <summary>
        /// 待收件
        /// </summary>
        public const string Pending_CODE = "08";
        /// <summary>
        /// 待收件
        /// </summary>
        public const string Pending_TEXT = "待收件";


        /// <summary>
        /// 已签收
        /// </summary>
        public const string Receipt_CODE = "09";
        /// <summary>
        /// 已签收
        /// </summary>
        public const string Receipt_TEXT = "已签收";


        /// <summary>
        /// 丢件破损
        /// </summary>
        public const string LossDge_CODE = "10";
        /// <summary>
        /// 丢件破损
        /// </summary>
        public const string LossDge_TEXT = "丢件破损";


        /// <summary>
        /// 异常签收
        /// </summary>
        public const string Overdue_CODE = "11";
        /// <summary>
        /// 异常签收
        /// </summary>
        public const string Overdue_TEXT = "异常签收";


        /// <summary>
        /// 已退回
        /// </summary>
        public const string Returnd_CODE = "12";
        /// <summary>
        /// 已退回
        /// </summary>
        public const string Returnd_TEXT = "已退回";


        /// <summary>
        /// 问题件
        /// </summary>
        public const string Aborted_CODE = "98";
        /// <summary>
        /// 问题件
        /// </summary>
        public const string Aborted_TEXT = "问题件";


        /// <summary>
        /// 已完结
        /// </summary>
        public const string Finished_CODE = "99";
        /// <summary>
        /// 已完结
        /// </summary>
        public const string Finished_TEXT = "已完结";


        /// <summary>
        /// 未知状态
        /// </summary>
        public const string Unknown_CODE = "00";
        /// <summary>
        /// 未知状态
        /// </summary>
        public const string Unknown_TEXT = "未知状态";




    }
}
