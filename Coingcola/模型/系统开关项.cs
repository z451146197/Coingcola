namespace Coingcola.模型
{
    /// <summary>
    /// 系统开关项。
    /// 
    /// 用于“让电脑更顺手”页面展示和操作。
    /// 当前定位：
    /// - 展示当前状态
    /// - 展示推荐状态
    /// - 提供一键应用推荐和手动切换
    /// </summary>
    public class 系统开关项
    {
        /// <summary>
        /// 唯一标识。
        /// </summary>
        public string Id { get; set; } = "";

        /// <summary>
        /// 功能名称。
        /// </summary>
        public string 名称 { get; set; } = "";

        /// <summary>
        /// 功能说明。
        /// </summary>
        public string 说明 { get; set; } = "";

        /// <summary>
        /// 当前是否开启。
        /// </summary>
        public bool 当前是否开启 { get; set; }

        /// <summary>
        /// 推荐是否开启。
        /// </summary>
        public bool 推荐是否开启 { get; set; }

        /// <summary>
        /// 推荐原因或场景说明。
        /// </summary>
        public string 推荐说明 { get; set; } = "";

        /// <summary>
        /// 生效方式说明。
        /// 例如：需重启资源管理器后生效 / 新窗口生效
        /// </summary>
        public string 生效说明 { get; set; } = "";

        /// <summary>
        /// 当前状态文本。
        /// </summary>
        public string 当前状态文本 => 当前是否开启 ? "已开启" : "已关闭";

        /// <summary>
        /// 推荐状态文本。
        /// </summary>
        public string 推荐状态文本 => 推荐是否开启 ? "推荐开启" : "推荐关闭";

        /// <summary>
        /// 是否已符合推荐。
        /// </summary>
        public bool 是否符合推荐 => 当前是否开启 == 推荐是否开启;

        /// <summary>
        /// 应用推荐按钮文案。
        /// </summary>
        public string 应用推荐按钮文本 => 推荐是否开启 ? "一键设为开启" : "一键设为关闭";

        /// <summary>
        /// 切换按钮文案。
        /// </summary>
        public string 切换按钮文本 => 当前是否开启 ? "立即关闭" : "立即开启";

        /// <summary>
        /// 当前是否允许点击“应用推荐”。
        /// </summary>
        public bool 是否可应用推荐 => !是否符合推荐;
    }
}