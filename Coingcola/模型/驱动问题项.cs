namespace Coingcola.模型
{
    /// <summary>
    /// 驱动问题项。
    /// 用于展示当前系统中存在异常状态的设备。
    /// </summary>
    public class 驱动问题项
    {
        /// <summary>
        /// 设备名称。
        /// </summary>
        public string 设备名称 { get; set; } = "";

        /// <summary>
        /// 设备 ID。
        /// </summary>
        public string 设备Id { get; set; } = "";

        /// <summary>
        /// 配置管理器错误代码。
        /// </summary>
        public int 错误代码 { get; set; }

        /// <summary>
        /// 错误说明。
        /// </summary>
        public string 错误说明 { get; set; } = "";

        /// <summary>
        /// 用于界面展示的错误代码文本。
        /// </summary>
        public string 错误代码文本 => $"错误代码：{错误代码}";
    }
}