namespace Coingcola.模型
{
    /// <summary>
    /// 软件安装项。
    /// 用于“软件中心 > 安装常用软件”页面展示与执行。
    /// </summary>
    public class 软件安装项
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
        /// 例如：浏览器 / 压缩 / 办公 / 通讯 / 开发 / 工具 / 播放器
        /// </summary>
        public string 分类 { get; set; } = "";

        /// <summary>
        /// 简介说明。
        /// </summary>
        public string 说明 { get; set; } = "";

        /// <summary>
        /// 来源类型。
        /// 例如：官网安装 / 系统安装 / 后续接入
        /// </summary>
        public string 来源类型 { get; set; } = "";

        /// <summary>
        /// 动作按钮文案。
        /// </summary>
        public string 按钮文本 { get; set; } = "立即安装";

        /// <summary>
        /// 官网地址。
        /// </summary>
        public string 官网地址 { get; set; } = "";

        /// <summary>
        /// winget 安装标识。
        /// </summary>
        public string WingetId { get; set; } = "";
    }
}