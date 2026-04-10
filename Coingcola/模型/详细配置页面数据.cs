using System.Collections.Generic;

namespace Coingcola.模型
{
    /// <summary>
    /// 详细配置页面数据。
    /// </summary>
    public class 详细配置页面数据
    {
        public string 设备名称 { get; set; } = "";
        public string 系统名称 { get; set; } = "";
        public string 系统版本 { get; set; } = "";
        public string 系统类型 { get; set; } = "";
        public string 当前用户 { get; set; } = "";

        public string CPU名称 { get; set; } = "";
        public string CPU核心信息 { get; set; } = "";
        public string 内存总量 { get; set; } = "";
        public string 显卡名称 { get; set; } = "";
        public string 主板信息 { get; set; } = "";
        public string BIOS信息 { get; set; } = "";

        public string 设备厂商 { get; set; } = "";
        public string 设备型号 { get; set; } = "";
        public string 系统盘信息 { get; set; } = "";
        public string 磁盘概览 { get; set; } = "";

        public string 活动网卡 { get; set; } = "";
        public string IP地址 { get; set; } = "";
        public string MAC地址 { get; set; } = "";

        /// <summary>
        /// 显卡列表。用于完整展示多显卡信息。
        /// </summary>
        public List<string> 显卡列表 { get; set; } = new();

        /// <summary>
        /// 磁盘列表。用于分行展示磁盘概览。
        /// </summary>
        public List<string> 磁盘列表 { get; set; } = new();

        public List<详细配置分组> 分组列表 { get; set; } = new();
    }

    /// <summary>
    /// 详细配置分组。
    /// </summary>
    public class 详细配置分组
    {
        public string 标题 { get; set; } = "";
        public string 简介 { get; set; } = "";
        public List<详细配置项> 项列表 { get; set; } = new();
    }

    /// <summary>
    /// 详细配置项。
    /// </summary>
    public class 详细配置项
    {
        public string 名称 { get; set; } = "";
        public string 值 { get; set; } = "";
        public string 说明 { get; set; } = "";
    }
}