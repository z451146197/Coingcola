using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Coingcola.系统工具
{
    /// <summary>
    /// 网址识别来源类型。
    /// 用来说明当前输入最后是按什么规则识别出来的。
    /// </summary>
    public enum 网址来源类型
    {
        完整网址,
        域名补全,
        内置关键字,
        自定义关键字,
        搜索引擎
    }

    /// <summary>
    /// 输入内容识别后的结果。
    /// 
    /// 为什么需要这个类？
    /// 因为用户输入的可能不是直接网址，而是：
    /// - 完整网址
    /// - 域名
    /// - 关键字
    /// - 普通搜索词
    /// 
    /// 所以后续界面层需要知道：
    /// 1. 最终应该打开什么地址
    /// 2. 这个结果可不可以直接保存为导航项
    /// 3. 是通过什么规则识别出来的
    /// </summary>
    public class 网址识别结果
    {
        /// <summary>
        /// 用户原始输入。
        /// </summary>
        public string 原始输入 { get; set; } = "";

        /// <summary>
        /// 最终识别后可打开的网址。
        /// </summary>
        public string 最终地址 { get; set; } = "";

        /// <summary>
        /// 建议名称。
        /// 如果是关键字命中，通常就是站点名；
        /// 如果是域名补全，则通常是主域名。
        /// </summary>
        public string 建议名称 { get; set; } = "";

        /// <summary>
        /// 当前结果是通过哪种方式识别出来的。
        /// </summary>
        public 网址来源类型 来源类型 { get; set; }

        /// <summary>
        /// 是否可以直接打开。
        /// 第一版里，只要识别完成，都是可以打开的。
        /// </summary>
        public bool 可直接打开 { get; set; } = true;

        /// <summary>
        /// 是否适合直接保存到网站导航。
        /// 
        /// 例如：
        /// - GitHub、知乎这种明确网址，适合保存
        /// - “显卡天梯图”这种搜索词，不建议直接保存
        /// </summary>
        public bool 可保存到导航 { get; set; }

        /// <summary>
        /// 对当前识别结果的说明文本。
        /// 方便界面给用户提示。
        /// </summary>
        public string 说明 { get; set; } = "";
    }

    /// <summary>
    /// 网址处理工具。
    /// 
    /// 负责：
    /// 1. 清洗输入
    /// 2. 判断是否为完整网址
    /// 3. 判断是否为域名
    /// 4. 内置关键字映射
    /// 5. 自定义关键字映射
    /// 6. 未识别时自动拼接搜索引擎链接
    /// </summary>
    public static class 网址处理工具
    {
        /// <summary>
        /// 默认搜索引擎前缀。
        /// 第一版先固定使用 Bing。
        /// 后续可做成可配置项。
        /// </summary>
        private const string 默认搜索引擎前缀 = "https://www.bing.com/search?q=";

        /// <summary>
        /// 内置关键字映射表。
        /// 
        /// 说明：
        /// 这一版我们先内置一批高频站点，
        /// 后续再叠加“用户自定义关键字”。
        /// </summary>
        private static readonly Dictionary<string, (string 名称, string 地址)> 内置关键字映射 =
            new(StringComparer.OrdinalIgnoreCase)
            {
                { "github", ("GitHub", "https://github.com") },
                { "gitee", ("Gitee", "https://gitee.com") },
                { "知乎", ("知乎", "https://www.zhihu.com") },
                { "zhihu", ("知乎", "https://www.zhihu.com") },
                { "b站", ("B站", "https://www.bilibili.com") },
                { "bilibili", ("B站", "https://www.bilibili.com") },
                { "语雀", ("语雀", "https://www.yuque.com") },
                { "yuque", ("语雀", "https://www.yuque.com") },
                { "飞书", ("飞书", "https://www.feishu.cn") },
                { "feishu", ("飞书", "https://www.feishu.cn") },
                { "邮箱", ("邮箱", "https://mail.qq.com") },
                { "qq邮箱", ("QQ邮箱", "https://mail.qq.com") },
                { "微信公众平台", ("微信公众平台", "https://mp.weixin.qq.com") },
                { "公众号", ("微信公众平台", "https://mp.weixin.qq.com") }
            };

        /// <summary>
        /// 核心入口：
        /// 将用户输入识别为一个“可打开的网址结果”。
        /// 
        /// 识别顺序：
        /// 1. 完整网址
        /// 2. 域名补全
        /// 3. 内置关键字
        /// 4. 用户自定义关键字
        /// 5. 搜索引擎兜底
        /// </summary>
        public static 网址识别结果 识别输入(
            string 原始输入,
            Dictionary<string, string>? 自定义关键字映射 = null)
        {
            string 输入 = 清洗输入(原始输入);

            if (string.IsNullOrWhiteSpace(输入))
            {
                return new 网址识别结果
                {
                    原始输入 = 原始输入,
                    最终地址 = "",
                    建议名称 = "",
                    来源类型 = 网址来源类型.搜索引擎,
                    可直接打开 = false,
                    可保存到导航 = false,
                    说明 = "输入为空，无法识别。"
                };
            }

            // 1. 先判断是不是完整网址
            if (是完整网址(输入))
            {
                string 规范地址 = 规范化网址(输入);

                return new 网址识别结果
                {
                    原始输入 = 原始输入,
                    最终地址 = 规范地址,
                    建议名称 = 从网址生成建议名称(规范地址),
                    来源类型 = 网址来源类型.完整网址,
                    可直接打开 = true,
                    可保存到导航 = true,
                    说明 = "已识别为完整网址。"
                };
            }

            // 2. 再判断是不是域名
            if (看起来像域名(输入))
            {
                string 规范地址 = 规范化网址($"https://{输入}");

                return new 网址识别结果
                {
                    原始输入 = 原始输入,
                    最终地址 = 规范地址,
                    建议名称 = 从网址生成建议名称(规范地址),
                    来源类型 = 网址来源类型.域名补全,
                    可直接打开 = true,
                    可保存到导航 = true,
                    说明 = "已按域名自动补全为完整网址。"
                };
            }

            // 3. 内置关键字
            if (内置关键字映射.TryGetValue(输入, out var 内置结果))
            {
                return new 网址识别结果
                {
                    原始输入 = 原始输入,
                    最终地址 = 内置结果.地址,
                    建议名称 = 内置结果.名称,
                    来源类型 = 网址来源类型.内置关键字,
                    可直接打开 = true,
                    可保存到导航 = true,
                    说明 = "已命中内置关键字映射。"
                };
            }

            // 4. 用户自定义关键字
            if (自定义关键字映射 != null &&
                自定义关键字映射.TryGetValue(输入, out string? 自定义地址) &&
                !string.IsNullOrWhiteSpace(自定义地址))
            {
                string 规范地址 = 自定义地址;

                if (!是完整网址(规范地址) && 看起来像域名(规范地址))
                {
                    规范地址 = $"https://{规范地址}";
                }

                规范地址 = 规范化网址(规范地址);

                return new 网址识别结果
                {
                    原始输入 = 原始输入,
                    最终地址 = 规范地址,
                    建议名称 = 输入,
                    来源类型 = 网址来源类型.自定义关键字,
                    可直接打开 = true,
                    可保存到导航 = true,
                    说明 = "已命中用户自定义关键字映射。"
                };
            }

            // 5. 最后走搜索引擎兜底
            string 搜索地址 = 构造搜索引擎地址(输入);

            return new 网址识别结果
            {
                原始输入 = 原始输入,
                最终地址 = 搜索地址,
                建议名称 = 输入,
                来源类型 = 网址来源类型.搜索引擎,
                可直接打开 = true,
                可保存到导航 = false,
                说明 = "未识别为固定网址，已自动转为搜索引擎查询。"
            };
        }

        /// <summary>
        /// 清洗输入：
        /// - 去掉首尾空格
        /// - 替换全角空格
        /// </summary>
        public static string 清洗输入(string? 输入)
        {
            if (string.IsNullOrWhiteSpace(输入))
            {
                return "";
            }

            return 输入.Replace('　', ' ').Trim();
        }

        /// <summary>
        /// 判断是否为完整网址。
        /// 只接受 http / https。
        /// </summary>
        public static bool 是完整网址(string 输入)
        {
            if (!Uri.TryCreate(输入, UriKind.Absolute, out Uri? uri))
            {
                return false;
            }

            return uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps;
        }

        /// <summary>
        /// 判断输入是否“看起来像一个域名”。
        /// 
        /// 例如：
        /// github.com
        /// www.zhihu.com
        /// bilibili.com/video
        /// 
        /// 第一版不做特别严苛的校验，
        /// 只要符合常见域名结构即可。
        /// </summary>
        public static bool 看起来像域名(string 输入)
        {
            if (string.IsNullOrWhiteSpace(输入))
            {
                return false;
            }

            if (输入.Contains(" "))
            {
                return false;
            }

            // 允许带路径，但主机部分至少要像 domain.tld
            return Regex.IsMatch(
                输入,
                @"^[a-zA-Z0-9][-a-zA-Z0-9\.]*\.[a-zA-Z]{2,}([/\?#].*)?$",
                RegexOptions.IgnoreCase);
        }

        /// <summary>
        /// 将网址规范化。
        /// 当前主要做：
        /// - 去除多余空格
        /// - 用 Uri 重新格式化
        /// </summary>
        public static string 规范化网址(string 输入)
        {
            string 清洗后 = 清洗输入(输入);

            if (!Uri.TryCreate(清洗后, UriKind.Absolute, out Uri? uri))
            {
                return 清洗后;
            }

            return uri.ToString();
        }

        /// <summary>
        /// 从网址生成建议名称。
        /// 
        /// 例如：
        /// https://github.com -> github
        /// https://www.zhihu.com -> zhihu
        /// 
        /// 后续如果你想更好看，可以在这里增加站点名映射。
        /// </summary>
        public static string 从网址生成建议名称(string 地址)
        {
            if (!Uri.TryCreate(地址, UriKind.Absolute, out Uri? uri))
            {
                return 地址;
            }

            string host = uri.Host;

            if (host.StartsWith("www.", StringComparison.OrdinalIgnoreCase))
            {
                host = host.Substring(4);
            }

            int firstDotIndex = host.IndexOf('.');
            if (firstDotIndex > 0)
            {
                host = host.Substring(0, firstDotIndex);
            }

            return host;
        }

        /// <summary>
        /// 构造搜索引擎网址。
        /// 未识别为网址时，用这个兜底打开搜索结果。
        /// </summary>
        public static string 构造搜索引擎地址(string 关键词)
        {
            string 编码后关键词 = Uri.EscapeDataString(关键词);
            return $"{默认搜索引擎前缀}{编码后关键词}";
        }
    }
}