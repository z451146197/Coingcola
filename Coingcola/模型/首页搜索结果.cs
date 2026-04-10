namespace Coingcola.模型
{
    /// <summary>
    /// 首页搜索 / 意图入口的结果模型。
    /// </summary>
    public class 首页搜索结果
    {
        /// <summary>
        /// 结果标题。
        /// </summary>
        public string 标题 { get; set; } = "";

        /// <summary>
        /// 结果说明。
        /// </summary>
        public string 说明 { get; set; } = "";

        /// <summary>
        /// 结果类型。
        /// 例如：立即打开 / 页面功能 / 知识速查 / 网络搜索 / 本地文件
        /// </summary>
        public string 结果类型 { get; set; } = "";

        /// <summary>
        /// 主按钮文本。
        /// 例如：立即打开 / 进入页面 / 网络搜索 / 打开文件
        /// </summary>
        public string 主按钮文本 { get; set; } = "";

        /// <summary>
        /// 主动作类型。
        /// 例如：打开网址 / 打开系统对象 / 打开文件 / 进入页面 / 网络搜索
        /// </summary>
        public string 动作类型 { get; set; } = "";

        /// <summary>
        /// 动作目标。
        /// 可传 URL / shell / exe / msc / 文件路径 / 搜索词
        /// </summary>
        public string 目标 { get; set; } = "";

        /// <summary>
        /// 页面跳转时的一级菜单名称。
        /// </summary>
        public string 一级菜单名称 { get; set; } = "";

        /// <summary>
        /// 页面跳转时的二级菜单名称。
        /// </summary>
        public string 二级菜单名称 { get; set; } = "";

        /// <summary>
        /// 是否为高置信、可直接执行结果。
        /// </summary>
        public bool 是否自动执行 { get; set; } = false;

        /// <summary>
        /// 次按钮文本。
        /// 例如：打开位置
        /// </summary>
        public string 次按钮文本 { get; set; } = "";

        /// <summary>
        /// 次按钮动作类型。
        /// 例如：打开所在位置
        /// </summary>
        public string 次动作类型 { get; set; } = "";

        /// <summary>
        /// 次按钮动作目标。
        /// </summary>
        public string 次目标 { get; set; } = "";
    }
}
