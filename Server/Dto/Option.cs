using SqlSugar;

namespace Dto
{
    /// <summary>
    /// 选项数据传输对象
    /// </summary>
    public class Option
    {
        /// <summary>
        /// 选项值
        /// </summary>
        public string value { get; set; }
        /// <summary>
        /// 选项名称
        /// </summary>
        public string label { get; set; }
    }
}
