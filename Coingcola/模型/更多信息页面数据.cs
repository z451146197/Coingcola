using System.Collections.Generic;

namespace Coingcola.模型
{
    /// <summary>
    /// 更多信息页面数据。
    /// 用于“此电脑 > 更多信息”页面展示。
    /// </summary>
    public class 更多信息页面数据
    {
        public string 页面结论 { get; set; } = "";
        public string 设备名称 { get; set; } = "";
        public string 当前用户 { get; set; } = "";
        public string 系统名称 { get; set; } = "";
        public string 系统版本 { get; set; } = "";

        public string 活动网卡 { get; set; } = "";
        public string IP地址 { get; set; } = "";
        public string MAC地址 { get; set; } = "";
        public string DNS服务器 { get; set; } = "";
        public string 默认网关 { get; set; } = "";

        public string 固定磁盘数量 { get; set; } = "";
        public string 固定磁盘概览 { get; set; } = "";
        public List<string> 固定磁盘列表 { get; set; } = new();

        public string DotNet版本 { get; set; } = "";
        public string PowerShell版本 { get; set; } = "";
        public string Winget状态 { get; set; } = "";

        public string Windows目录 { get; set; } = "";
        public string 系统目录 { get; set; } = "";
        public string 临时目录 { get; set; } = "";
        public string 当前目录 { get; set; } = "";
        public string 进程架构 { get; set; } = "";

        public List<更多信息分组> 分组列表 { get; set; } = new();
    }

    /// <summary>
    /// 更多信息分组。
    /// </summary>
    public class 更多信息分组
    {
        public string 标题 { get; set; } = "";
        public string 简介 { get; set; } = "";
        public List<更多信息项> 项列表 { get; set; } = new();
    }

    /// <summary>
    /// 更多信息项。
    /// </summary>
    public class 更多信息项
    {
        public string 名称 { get; set; } = "";
        public string 值 { get; set; } = "";
        public string 说明 { get; set; } = "";
    }
}