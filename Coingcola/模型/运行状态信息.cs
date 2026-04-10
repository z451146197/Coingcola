using System.Collections.Generic;

namespace Coingcola.模型
{
    /// <summary>
    /// 运行状态信息。
    /// 用于“此电脑 > 运行状态”页面展示。
    /// </summary>
    public class 运行状态信息
    {
        public string 页面结论 { get; set; } = "";
        public string 最近启动时间 { get; set; } = "";
        public string 运行时长 { get; set; } = "";

        public string CPU名称 { get; set; } = "";
        public string CPU当前负载 { get; set; } = "";
        public string CPU核心线程 { get; set; } = "";

        public string 内存总量 { get; set; } = "";
        public string 内存已用 { get; set; } = "";
        public string 内存可用 { get; set; } = "";
        public string 内存使用率 { get; set; } = "";

        public string 系统盘名称 { get; set; } = "";
        public string 系统盘总量 { get; set; } = "";
        public string 系统盘可用 { get; set; } = "";
        public string 系统盘使用率 { get; set; } = "";

        public string 显卡信息 { get; set; } = "";
        public string 系统名称 { get; set; } = "";
        public string 系统版本 { get; set; } = "";

        public List<运行状态分组> 分组列表 { get; set; } = new();
    }

    /// <summary>
    /// 运行状态分组。
    /// </summary>
    public class 运行状态分组
    {
        public string 标题 { get; set; } = "";
        public string 简介 { get; set; } = "";
        public List<运行状态项> 项列表 { get; set; } = new();
    }

    /// <summary>
    /// 运行状态项。
    /// </summary>
    public class 运行状态项
    {
        public string 名称 { get; set; } = "";
        public string 值 { get; set; } = "";
        public string 说明 { get; set; } = "";
    }
}