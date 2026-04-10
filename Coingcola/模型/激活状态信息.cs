namespace Coingcola.模型
{
    /// <summary>
    /// Windows 激活状态信息。
    /// </summary>
    public class 激活状态信息
    {
        /// <summary>
        /// 是否已成功激活。
        /// </summary>
        public bool 是否已激活 { get; set; }

        /// <summary>
        /// 大标题。
        /// 例如：系统已激活 / 系统未激活 / 当前处于宽限期
        /// </summary>
        public string 状态标题 { get; set; } = "";

        /// <summary>
        /// 状态说明。
        /// </summary>
        public string 状态说明 { get; set; } = "";

        /// <summary>
        /// 状态标签。
        /// 例如：已激活 / 宽限期 / 未激活 / 未知
        /// </summary>
        public string 状态标签 { get; set; } = "";

        /// <summary>
        /// Windows 系统名称。
        /// 例如：Windows 11 Pro
        /// </summary>
        public string 系统名称 { get; set; } = "";

        /// <summary>
        /// 系统版本信息。
        /// 例如：23H2 / Build 22631
        /// </summary>
        public string 版本名称 { get; set; } = "";

        /// <summary>
        /// 授权名称。
        /// 通常来自 SoftwareLicensingProduct.Name
        /// </summary>
        public string 授权名称 { get; set; } = "";

        /// <summary>
        /// 部分产品密钥。
        /// </summary>
        public string 部分产品密钥 { get; set; } = "";
    }
}