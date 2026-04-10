namespace Coingcola.模型
{
    /// <summary>
    /// 高级设置入口项。
    /// 用于“电脑优化 > 高级设置”页面展示与执行。
    /// </summary>
    public class 高级设置入口项
    {
        /// <summary>
        /// 唯一标识。
        /// </summary>
        public string Id { get; set; } = "";

        /// <summary>
        /// 入口名称。
        /// </summary>
        public string 名称 { get; set; } = "";

        /// <summary>
        /// 分类。
        /// 例如：系统 / 应用 / 硬件 / 工具
        /// </summary>
        public string 分类 { get; set; } = "";

        /// <summary>
        /// 简介说明。
        /// </summary>
        public string 说明 { get; set; } = "";

        /// <summary>
        /// 风险级别。
        /// 例如：低风险 / 需谨慎
        /// </summary>
        public string 风险级别 { get; set; } = "";

        /// <summary>
        /// 生效说明。
        /// </summary>
        public string 生效说明 { get; set; } = "";

        /// <summary>
        /// 按钮文案。
        /// </summary>
        public string 按钮文本 { get; set; } = "打开入口";
    }
}