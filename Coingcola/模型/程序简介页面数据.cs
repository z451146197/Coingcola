using System.Collections.Generic;

namespace Coingcola.模型
{
    /// <summary>
    /// 程序简介页数据。
    /// </summary>
    public class 程序简介页面数据
    {
        public string 产品名称 { get; set; } = "";
        public string 产品定位 { get; set; } = "";
        public string 核心口号 { get; set; } = "";
        public string 页面结论 { get; set; } = "";

        public List<程序简介摘要卡项> 摘要卡列表 { get; set; } = new();
        public List<程序简介分组> 分组列表 { get; set; } = new();
    }

    /// <summary>
    /// 程序简介摘要卡项。
    /// </summary>
    public class 程序简介摘要卡项
    {
        public string 标题 { get; set; } = "";
        public string 数值 { get; set; } = "";
        public string 说明 { get; set; } = "";
    }

    /// <summary>
    /// 程序简介分组。
    /// </summary>
    public class 程序简介分组
    {
        public string 标题 { get; set; } = "";
        public string 简介 { get; set; } = "";
        public List<程序简介项> 项列表 { get; set; } = new();
    }

    /// <summary>
    /// 程序简介项。
    /// </summary>
    public class 程序简介项
    {
        public string 名称 { get; set; } = "";
        public string 值 { get; set; } = "";
        public string 说明 { get; set; } = "";
    }
}