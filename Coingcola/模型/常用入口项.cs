namespace Coingcola.模型
{
    /// <summary>
    /// 首页“常用入口”里的单个入口项。
    /// 
    /// 这里的入口项可以统一承载：
    /// 1. 网站
    /// 2. 系统位置
    /// 3. 本地软件
    /// 
    /// 当前第一版先重点支持网站与系统位置。
    /// </summary>
    public class 常用入口项
    {
        /// <summary>
        /// 唯一标识。
        /// </summary>
        public string Id { get; set; } = "";

        /// <summary>
        /// 显示名称。
        /// 例如：GitHub、此电脑、下载。
        /// </summary>
        public string 名称 { get; set; } = "";

        /// <summary>
        /// 辅助说明。
        /// 例如：代码仓库、系统位置、常用网站。
        /// </summary>
        public string 说明 { get; set; } = "";

        /// <summary>
        /// 图标占位文本。
        /// 当前先使用 Emoji / 单字符占位，后续再升级真实图标。
        /// </summary>
        public string 图标文本 { get; set; } = "";

        /// <summary>
        /// 类型：
        /// - 网址
        /// - 系统位置
        /// - 待配置
        /// </summary>
        public string 类型 { get; set; } = "";

        /// <summary>
        /// 实际目标。
        /// 
        /// 当类型是“网址”时，这里放 URL。
        /// 当类型是“系统位置”时，这里放 shell: 或 ms-settings: 或 explorer 参数。
        /// </summary>
        public string 目标 { get; set; } = "";

        /// <summary>
        /// 用于搜索匹配的关键字。
        /// </summary>
        public string 关键字 { get; set; } = "";
    }
}
