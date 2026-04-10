namespace Coingcola.模型
{
    /// <summary>
    /// 常见修复动作项。
    /// 用于“电脑优化 > 常见修复”页面展示与执行。
    /// </summary>
    public class 修复动作项
    {
        /// <summary>
        /// 唯一标识。
        /// </summary>
        public string Id { get; set; } = "";

        /// <summary>
        /// 动作名称。
        /// </summary>
        public string 名称 { get; set; } = "";

        /// <summary>
        /// 分类。
        /// 例如：资源管理器 / 网络 / 清理 / 系统
        /// </summary>
        public string 分类 { get; set; } = "";

        /// <summary>
        /// 简介说明。
        /// </summary>
        public string 说明 { get; set; } = "";

        /// <summary>
        /// 风险级别。
        /// 例如：低风险 / 需确认 / 管理员权限
        /// </summary>
        public string 风险级别 { get; set; } = "";

        /// <summary>
        /// 生效说明。
        /// 例如：执行后立即生效 / 可能短暂闪烁
        /// </summary>
        public string 生效说明 { get; set; } = "";

        /// <summary>
        /// 按钮文案。
        /// </summary>
        public string 按钮文本 { get; set; } = "立即执行";
    }
}