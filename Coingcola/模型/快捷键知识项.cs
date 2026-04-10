namespace Coingcola.模型
{
    /// <summary>
    /// 快捷键知识项。
    /// 
    /// 当前先用于 WPS 快捷键知识库，
    /// 后续也可以扩展到：
    /// - Windows 系统快捷键
    /// - 浏览器快捷键
    /// - Office / VS Code 等操作速查
    /// </summary>
    public class 快捷键知识项
    {
        /// <summary>
        /// 唯一标识。
        /// </summary>
        public string Id { get; set; } = "";

        /// <summary>
        /// 软件名称。
        /// 例如：WPS
        /// </summary>
        public string 软件名称 { get; set; } = "";

        /// <summary>
        /// 分类。
        /// 例如：文字 / 表格 / 演示 / 通用
        /// </summary>
        public string 分类 { get; set; } = "";

        /// <summary>
        /// 功能名称。
        /// 例如：复制 / 保存 / 查找
        /// </summary>
        public string 功能名称 { get; set; } = "";

        /// <summary>
        /// 快捷键。
        /// 例如：Ctrl + C
        /// </summary>
        public string 快捷键 { get; set; } = "";

        /// <summary>
        /// 说明文案。
        /// </summary>
        public string 说明 { get; set; } = "";

        /// <summary>
        /// 检索关键字。
        /// 用于本地知识库搜索匹配。
        /// </summary>
        public string 关键字 { get; set; } = "";
    }
}