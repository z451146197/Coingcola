using System.Collections.Generic;

namespace Coingcola.模型
{
    /// <summary>
    /// 设备概览信息。
    /// 用于“此电脑 > 看看这台电脑”页面展示。
    /// </summary>
    public class 设备概览信息
    {
        public string 设备名称 { get; set; } = "";
        public string 系统名称 { get; set; } = "";
        public string 系统版本 { get; set; } = "";
        public string 当前用户 { get; set; } = "";
        public string 权限状态 { get; set; } = "";
        public string 运行结论 { get; set; } = "";

        public string CPU名称 { get; set; } = "";
        public string 内存总量 { get; set; } = "";
        public string 显卡名称 { get; set; } = "";
        public string 系统盘信息 { get; set; } = "";
        public string 主机名 { get; set; } = "";
        public string 运行时长 { get; set; } = "";

        public string 主板信息 { get; set; } = "";
        public string BIOS信息 { get; set; } = "";
        public string 系统类型 { get; set; } = "";
        public string 上次启动时间 { get; set; } = "";
        public string 设备厂商 { get; set; } = "";
        public string 设备型号 { get; set; } = "";

        /// <summary>
        /// 显卡列表。用于双显卡/多显卡场景完整展示。
        /// </summary>
        public List<string> 显卡列表 { get; set; } = new();

        /// <summary>
        /// 磁盘列表。用于后续扩展展示。
        /// </summary>
        public List<string> 磁盘列表 { get; set; } = new();
    }
}