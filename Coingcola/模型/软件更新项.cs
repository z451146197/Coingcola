namespace Coingcola.模型
{
    /// <summary>
    /// 软件更新项。
    /// 用于“软件中心 > 软件更新”页面展示与执行。
    /// </summary>
    public class 软件更新项
    {
        /// <summary>
        /// 唯一标识。
        /// </summary>
        public string Id { get; set; } = "";

        /// <summary>
        /// 软件名称。
        /// </summary>
        public string 名称 { get; set; } = "";

        /// <summary>
        /// 分类。
        /// 例如：浏览器 / 办公 / 通讯 / 工具 / 开发 / 播放器
        /// </summary>
        public string 分类 { get; set; } = "";

        /// <summary>
        /// 更新方式。
        /// 例如：系统更新 / 官网更新
        /// </summary>
        public string 更新方式 { get; set; } = "";

        /// <summary>
        /// 说明文案。
        /// </summary>
        public string 说明 { get; set; } = "";

        /// <summary>
        /// 风险级别。
        /// 例如：低风险 / 需确认
        /// </summary>
        public string 风险级别 { get; set; } = "";

        /// <summary>
        /// 按钮文案。
        /// </summary>
        public string 按钮文本 { get; set; } = "立即更新";

        /// <summary>
        /// winget 标识。
        /// </summary>
        public string WingetId { get; set; } = "";

        /// <summary>
        /// 官网地址。
        /// </summary>
        public string 官网地址 { get; set; } = "";
    }
}