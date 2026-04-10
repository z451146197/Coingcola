using System.Collections.Generic;

namespace Coingcola.模型
{
    /// <summary>
    /// 一键优化体检页的数据模型。
    /// 当前先做总控台骨架，不直接承载复杂诊断引擎。
    /// </summary>
    public class 优化体检页面数据
    {
        public string 页面标题 { get; set; } = "";
        public string 页面结论 { get; set; } = "";
        public string 副结论 { get; set; } = "";

        public string 设备名称 { get; set; } = "";
        public string 系统名称 { get; set; } = "";
        public string 当前用户 { get; set; } = "";
        public string 权限状态 { get; set; } = "";
        public string Winget状态 { get; set; } = "";

        public List<优化体检摘要卡项> 摘要卡列表 { get; set; } = new();
        public List<优化体检建议项> 建议列表 { get; set; } = new();
        public List<优化体检快捷入口项> 快捷入口列表 { get; set; } = new();
    }

    /// <summary>
    /// 摘要卡项。
    /// </summary>
    public class 优化体检摘要卡项
    {
        public string 标题 { get; set; } = "";
        public string 数值 { get; set; } = "";
        public string 说明 { get; set; } = "";
    }

    /// <summary>
    /// 建议项。
    /// </summary>
    public class 优化体检建议项
    {
        public string 标题 { get; set; } = "";
        public string 说明 { get; set; } = "";
        public string 按钮文本 { get; set; } = "去处理";

        /// <summary>
        /// 格式：一级菜单|二级菜单
        /// </summary>
        public string 跳转标识 { get; set; } = "";
    }

    /// <summary>
    /// 快捷入口项。
    /// </summary>
    public class 优化体检快捷入口项
    {
        public string 名称 { get; set; } = "";
        public string 简介 { get; set; } = "";
        public string 跳转标识 { get; set; } = "";
    }
}