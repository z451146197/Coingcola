namespace Coingcola.模型
{
    /// <summary>
    /// 首页“状态提醒”区块的摘要项。
    /// </summary>
    public class 状态摘要项
    {
        /// <summary>
        /// 标题。
        /// </summary>
        public string 标题 { get; set; } = "";

        /// <summary>
        /// 内容说明。
        /// </summary>
        public string 内容 { get; set; } = "";

        /// <summary>
        /// 强调文本。
        /// 例如：正常 / 待处理 / 1项可更新
        /// </summary>
        public string 强调文本 { get; set; } = "";
    }
}