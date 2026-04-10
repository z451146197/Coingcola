using System;
using System.Collections.Generic;

namespace Coingcola.模型
{
    public enum 统一搜索结果类型
    {
        应用 = 1,
        快捷动作 = 2,
        设置 = 3,
        文件 = 4,
        文件夹 = 5,
        网站 = 6
    }

    public enum 统一搜索次动作类型
    {
        打开 = 1,
        打开位置 = 2,
        以管理员身份运行 = 3,
        固定到常用 = 4,
        取消固定 = 5,
        查看详情 = 6
    }

    public sealed class 统一搜索次动作
    {
        public 统一搜索次动作类型 类型 { get; set; }
        public string 文案 { get; set; } = string.Empty;
    }

    public sealed class 统一搜索结果项
    {
        public string Id { get; set; } = string.Empty;
        public string 标题 { get; set; } = string.Empty;
        public string 副标题 { get; set; } = string.Empty;
        public string 图标路径 { get; set; } = string.Empty;
        public string 来源 { get; set; } = string.Empty;
        public string 目标 { get; set; } = string.Empty;
        public string 命中说明 { get; set; } = string.Empty;
        public double 分数 { get; set; }
        public bool 是否置顶 { get; set; }
        public bool 是否最近使用 { get; set; }
        public bool 是否可执行 { get; set; } = true;
        public string 主动作文案 { get; set; } = "打开";
        public 统一搜索结果类型 类型 { get; set; }
        public List<统一搜索次动作> 次动作列表 { get; set; } = new();
    }

    public sealed class 统一搜索结果分组
    {
        public string 标题 { get; set; } = string.Empty;
        public 统一搜索结果类型 类型 { get; set; }
        public List<统一搜索结果项> 结果列表 { get; set; } = new();
    }

    public sealed class 统一搜索响应
    {
        public string 查询词 { get; set; } = string.Empty;
        public string 状态提示 { get; set; } = string.Empty;
        public bool 文件搜索可用 { get; set; }
        public bool 正在使用Everything { get; set; }
        public 统一搜索结果项? 最佳匹配 { get; set; }
        public List<统一搜索结果分组> 分组列表 { get; set; } = new();
    }
}
