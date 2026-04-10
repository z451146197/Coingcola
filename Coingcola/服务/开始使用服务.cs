﻿﻿﻿using Coingcola.模型;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Coingcola.服务
{
    public sealed class 开始使用服务
    {
        private readonly Everything搜索服务 _everything搜索服务 = new();
        private readonly 搜索历史服务 _搜索历史服务 = new();

        private List<应用定义>? _应用缓存;
        private DateTime _应用缓存时间 = DateTime.MinValue;
        private readonly object _应用缓存锁 = new();

        public string 获取文件搜索状态文本()
        {
            return _everything搜索服务.获取状态提示();
        }

        public void 预热搜索引擎()
        {
            _everything搜索服务.尝试启动();
            _ = 获取应用列表();
        }

        public void 强制刷新搜索引擎()
        {
            _everything搜索服务.尝试启动();
            lock (_应用缓存锁)
            {
                _应用缓存 = null;
                _应用缓存时间 = DateTime.MinValue;
            }
        }

        public List<最近使用项> 获取最近使用()
        {
            return _搜索历史服务.获取最近使用(6);
        }

        public void 删除最近使用(string id)
        {
            _搜索历史服务.删除最近使用(id);
        }

        public void 清空最近使用()
        {
            _搜索历史服务.清空最近使用();
        }

        public List<统一搜索结果项> 获取空状态常用入口()
        {
            return new List<统一搜索结果项>
            {
                创建页面动作("page::我的电脑|电脑概览", "电脑概览", "快速看设备总览和基础状态。", "我的电脑|电脑概览"),
                创建页面动作("page::电脑优化|常用设置", "常用设置", "集中处理高频系统习惯项。", "电脑优化|常用设置"),
                创建页面动作("page::软件中心|软件目录", "软件目录", "按分类查看常用软件入口。", "软件中心|软件目录"),
                创建页面动作("page::开始使用|首页", "网页导航管理", "在首页直接维护高频网站入口。", "开始使用|首页")
            };
        }

        public async Task<统一搜索响应> 搜索统一入口Async(string query, CancellationToken cancellationToken)
        {
            query = (query ?? string.Empty).Trim();

            var 响应 = new 统一搜索响应
            {
                查询词 = query,
                状态提示 = _everything搜索服务.获取状态提示(),
                文件搜索可用 = _everything搜索服务.可以查询(),
                正在使用Everything = _everything搜索服务.可以唤起Everything()
            };

            if (string.IsNullOrWhiteSpace(query))
            {
                return 响应;
            }

            bool 文件意图 = 是否文件意图(query);

            var 应用结果 = 搜索应用(query);
            var 动作结果 = 搜索快捷动作(query);
            var 设置结果 = 搜索设置(query);
            var 网站结果 = 搜索网站(query);

            List<统一搜索结果项> 文件结果 = new();

            if (_everything搜索服务.可以查询())
            {
                文件结果 = await _everything搜索服务.查询文件与文件夹Async(query, cancellationToken);

                if (文件结果.Count == 0 && _everything搜索服务.可以唤起Everything())
                {
                    文件结果.Add(new 统一搜索结果项
                    {
                        Id = $"everything-search::{query}",
                        标题 = $"用 Everything 搜文件：{query}",
                        副标题 = "Everything 已接入，但当前未直接返回结果。可继续唤起 Everything 搜索。",
                        来源 = "Everything",
                        目标 = query,
                        命中说明 = "文件搜索兜底入口",
                        主动作文案 = "Everything 搜索",
                        类型 = 统一搜索结果类型.文件,
                        次动作列表 = new List<统一搜索次动作>
                        {
                            new() { 类型 = 统一搜索次动作类型.查看详情, 文案 = "查看详情" }
                        }
                    });
                }
            }
            else if (_everything搜索服务.可以唤起Everything())
            {
                文件结果.Add(new 统一搜索结果项
                {
                    Id = $"everything-search::{query}",
                    标题 = $"用 Everything 搜文件：{query}",
                    副标题 = "文件搜索引擎已就绪，当前以唤起 Everything 搜索作为兜底动作。",
                    来源 = "Everything",
                    目标 = query,
                    命中说明 = "文件搜索入口",
                    主动作文案 = "Everything 搜索",
                    类型 = 统一搜索结果类型.文件,
                    次动作列表 = new List<统一搜索次动作>
                    {
                        new() { 类型 = 统一搜索次动作类型.查看详情, 文案 = "查看详情" }
                    }
                });
            }

            if (!文件意图)
            {
                var 桥接应用结果 = 从Everything结果提取应用候选(文件结果, query);
                if (桥接应用结果.Count > 0)
                {
                    应用结果.AddRange(桥接应用结果);

                    var 桥接目标集合 = new HashSet<string>(
                        桥接应用结果.Select(x => x.目标 ?? string.Empty),
                        StringComparer.OrdinalIgnoreCase);

                    文件结果 = 文件结果
                        .Where(x => string.IsNullOrWhiteSpace(x.目标) || !桥接目标集合.Contains(x.目标))
                        .ToList();
                }
            }

            应用结果 = 排序结果(应用结果, query, 文件意图, 统一搜索结果类型.应用);
            动作结果 = 排序结果(动作结果, query, 文件意图, 统一搜索结果类型.快捷动作);
            设置结果 = 排序结果(设置结果, query, 文件意图, 统一搜索结果类型.设置);
            文件结果 = 排序结果(文件结果, query, 文件意图, 统一搜索结果类型.文件)
                .OrderByDescending(x => x.分数)
                .ToList();
            var 文件夹结果 = 文件结果.Where(x => x.类型 == 统一搜索结果类型.文件夹).ToList();
            文件结果 = 文件结果.Where(x => x.类型 == 统一搜索结果类型.文件).ToList();
            网站结果 = 排序结果(网站结果, query, 文件意图, 统一搜索结果类型.网站);

            if (判断核心产品词查询(query))
            {
                bool 已有更高优先级候选 = 应用结果.Count > 0 || 网站结果.Count > 0;

                if (已有更高优先级候选)
                {
                    文件结果 = 文件结果.Where(x => !是否Everything兜底入口(x)).ToList();
                    文件夹结果 = 文件夹结果.Where(x => !是否Everything兜底入口(x)).ToList();
                }

                文件结果 = 文件结果.Where(x => !是否应过滤的产品噪声文件(x, query)).ToList();
                文件夹结果 = 文件夹结果.Where(x => !是否应过滤的产品噪声文件(x, query)).ToList();
            }

            响应.分组列表 = new List<统一搜索结果分组>();

            添加分组(响应.分组列表, "应用", 统一搜索结果类型.应用, 应用结果, 4);
            添加分组(响应.分组列表, "快捷动作", 统一搜索结果类型.快捷动作, 动作结果, 4);
            添加分组(响应.分组列表, "设置", 统一搜索结果类型.设置, 设置结果, 4);
            添加分组(响应.分组列表, "文件", 统一搜索结果类型.文件, 文件结果, 4);
            添加分组(响应.分组列表, "文件夹", 统一搜索结果类型.文件夹, 文件夹结果, 4);
            添加分组(响应.分组列表, "网站", 统一搜索结果类型.网站, 网站结果, 4);

            var 全部结果 = 响应.分组列表
                .SelectMany(x => x.结果列表)
                .OrderByDescending(x => x.分数)
                .ToList();

            响应.最佳匹配 = 生成最佳匹配(全部结果, 文件意图);

            if (响应.最佳匹配 == null && _everything搜索服务.可以唤起Everything())
            {
                响应.最佳匹配 = new 统一搜索结果项
                {
                    Id = $"everything-fallback::{query}",
                    标题 = $"在 Everything 中搜索：{query}",
                    副标题 = "当前未生成更高优先级结果，使用 Everything 继续文件搜索。",
                    来源 = "Everything",
                    目标 = query,
                    命中说明 = "兜底文件搜索",
                    主动作文案 = "Everything 搜索",
                    类型 = 统一搜索结果类型.文件,
                    分数 = 10
                };
            }

            return 响应;
        }

        public (bool 成功, string 提示, string 一级菜单, string 二级菜单) 执行主动作(
            string query,
            统一搜索结果项 item,
            bool 来自最佳匹配)
        {
            return 执行动作(query, item, null, 来自最佳匹配);
        }

        public (bool 成功, string 提示, string 一级菜单, string 二级菜单) 执行次动作(
            string query,
            统一搜索结果项 item,
            统一搜索次动作类型 actionType,
            bool 来自最佳匹配)
        {
            return 执行动作(query, item, actionType, 来自最佳匹配);
        }

        private (bool 成功, string 提示, string 一级菜单, string 二级菜单) 执行动作(
            string query,
            统一搜索结果项 item,
            统一搜索次动作类型? overrideAction,
            bool 来自最佳匹配)
        {
            try
            {
                string target = item.目标 ?? string.Empty;

                if (overrideAction == 统一搜索次动作类型.固定到常用)
                {
                    _搜索历史服务.设置置顶(item.Id, true);
                    return (true, "已固定到常用。", string.Empty, string.Empty);
                }

                if (overrideAction == 统一搜索次动作类型.取消固定)
                {
                    _搜索历史服务.设置置顶(item.Id, false);
                    return (true, "已取消固定。", string.Empty, string.Empty);
                }

                if (overrideAction == 统一搜索次动作类型.查看详情)
                {
                    string detail = $"{item.标题}\n{item.副标题}\n来源：{item.来源}\n目标：{item.目标}\n命中：{item.命中说明}";
                    return (true, detail, string.Empty, string.Empty);
                }

                if (overrideAction == 统一搜索次动作类型.打开位置)
                {
                    打开位置(target);
                    _搜索历史服务.记录选择(query, item, 来自最佳匹配, true);
                    return (true, "已打开位置。", string.Empty, string.Empty);
                }

                if (overrideAction == 统一搜索次动作类型.以管理员身份运行)
                {
                    以管理员身份运行(target);
                    _搜索历史服务.记录选择(query, item, 来自最佳匹配, true);
                    return (true, "已尝试以管理员身份运行。", string.Empty, string.Empty);
                }

                if (target.StartsWith("page::", StringComparison.OrdinalIgnoreCase))
                {
                    string raw = target["page::".Length..];
                    string[] parts = raw.Split('|');
                    if (parts.Length == 2)
                    {
                        _搜索历史服务.记录选择(query, item, 来自最佳匹配, true);
                        return (true, string.Empty, parts[0], parts[1]);
                    }
                }

                if (target.StartsWith("command::", StringComparison.OrdinalIgnoreCase))
                {
                    string command = target["command::".Length..];
                    打开命令(command);
                    _搜索历史服务.记录选择(query, item, 来自最佳匹配, true);
                    return (true, "已打开系统入口。", string.Empty, string.Empty);
                }

                if (target.StartsWith("ms-settings:", StringComparison.OrdinalIgnoreCase))
                {
                    打开进程(target);
                    _搜索历史服务.记录选择(query, item, 来自最佳匹配, true);
                    return (true, "已打开系统设置。", string.Empty, string.Empty);
                }

                if (target.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                    target.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                {
                    打开进程(target);
                    _搜索历史服务.记录选择(query, item, 来自最佳匹配, true);
                    return (true, "已打开网站。", string.Empty, string.Empty);
                }

                if (item.来源 == "Everything" && !string.IsNullOrWhiteSpace(target) && !File.Exists(target) && !Directory.Exists(target))
                {
                    _everything搜索服务.在Everything中继续搜索(target);
                    _搜索历史服务.记录选择(query, item, 来自最佳匹配, true);
                    return (true, "已在 Everything 中继续搜索。", string.Empty, string.Empty);
                }

                if (Directory.Exists(target))
                {
                    打开进程(target);
                    _搜索历史服务.记录选择(query, item, 来自最佳匹配, true);
                    return (true, "已打开文件夹。", string.Empty, string.Empty);
                }

                if (File.Exists(target))
                {
                    打开进程(target);
                    _搜索历史服务.记录选择(query, item, 来自最佳匹配, true);
                    return (true, "已打开文件。", string.Empty, string.Empty);
                }

                _搜索历史服务.记录选择(query, item, 来自最佳匹配, false);
                return (false, "目标不可用。", string.Empty, string.Empty);
            }
            catch (Exception ex)
            {
                _搜索历史服务.记录选择(query, item, 来自最佳匹配, false);
                return (false, ex.Message, string.Empty, string.Empty);
            }
        }

        private List<统一搜索结果项> 搜索应用(string query)
        {
            bool 核心产品词查询 = 判断核心产品词查询(query);

            return 获取应用列表()
                .Where(x => 命中(x.Name, x.Alias, query))
                .Where(x => !是否应过滤的噪声应用(x.Name, query))
                .Where(x => !核心产品词查询 || 是否核心产品主应用候选(query, x.Name))
                .Select(x => new 统一搜索结果项
                {
                    Id = $"app::{x.ShortcutPath}",
                    标题 = x.Name,
                    副标题 = $"应用 · {Path.GetDirectoryName(x.ShortcutPath) ?? string.Empty}",
                    图标路径 = string.Empty,
                    来源 = "App",
                    目标 = x.ShortcutPath,
                    命中说明 = 获取命中说明(x.Name, x.Alias, query),
                    主动作文案 = "打开",
                    类型 = 统一搜索结果类型.应用,
                    次动作列表 = new List<统一搜索次动作>
                    {
                        new() { 类型 = 统一搜索次动作类型.以管理员身份运行, 文案 = "管理员运行" },
                        new() { 类型 = 统一搜索次动作类型.打开位置, 文案 = "打开位置" },
                        new() { 类型 = 统一搜索次动作类型.固定到常用, 文案 = "固定到常用" },
                        new() { 类型 = 统一搜索次动作类型.查看详情, 文案 = "查看详情" }
                    }
                })
                .ToList();
        }

        private bool 是否应过滤的噪声应用(string name, string query)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return false;
            }

            string lower = name.ToLowerInvariant();
            bool 查询包含噪声词 = 查询包含任何(query, "卸载", "修复", "更新", "输入法", "helper", "updater", "plugin", "组件");

            if (查询包含噪声词)
            {
                return false;
            }

            if (lower.Contains("卸载") || lower.Contains("uninstall") || lower.Contains("remove"))
            {
                return true;
            }

            if (lower.Contains("输入法") || lower.Contains("ime"))
            {
                return true;
            }

            if (lower.Contains("更新") || lower.Contains("updater") || lower.Contains("helper") || lower.Contains("repair"))
            {
                return true;
            }

            return false;
        }

        private bool 是否核心产品主应用候选(string query, string name)
        {
            if (!判断核心产品词查询(query))
            {
                return true;
            }

            string q = 规范化(query);
            string n = 规范化(name);

            if (string.IsNullOrWhiteSpace(n))
            {
                return false;
            }

            if (结果包含任何(n, "卸载", "uninstall", "remove", "修复", "repair", "helper", "updater", "更新器"))
            {
                return false;
            }

            if ((q == "微信" || q == "wechat" || q == "weixin") &&
                (n.Contains("微信", StringComparison.OrdinalIgnoreCase) ||
                 n.Contains("wechat", StringComparison.OrdinalIgnoreCase) ||
                 n.Contains("weixin", StringComparison.OrdinalIgnoreCase)))
            {
                return !n.Contains("输入法", StringComparison.OrdinalIgnoreCase);
            }

            if ((q == "飞书" || q == "feishu" || q == "lark") &&
                (n.Contains("飞书", StringComparison.OrdinalIgnoreCase) ||
                 n.Contains("feishu", StringComparison.OrdinalIgnoreCase) ||
                 n.Contains("lark", StringComparison.OrdinalIgnoreCase)))
            {
                return true;
            }

            if ((q == "钉钉" || q == "dingtalk") &&
                (n.Contains("钉钉", StringComparison.OrdinalIgnoreCase) ||
                 n.Contains("dingtalk", StringComparison.OrdinalIgnoreCase)))
            {
                return true;
            }

            if (q == "edge")
            {
                return n.Contains("edge", StringComparison.OrdinalIgnoreCase)
                    && !结果包含任何(n, "edgecore", "edgeupdate", "webview", "helper", "update");
            }

            if (q == "chrome")
            {
                return n.Contains("chrome", StringComparison.OrdinalIgnoreCase)
                    || n.Contains("谷歌", StringComparison.OrdinalIgnoreCase);
            }

            if (q == "qq")
            {
                return n == "qq" || n.Contains("腾讯qq", StringComparison.OrdinalIgnoreCase);
            }

            if (q == "wps")
            {
                return n.Contains("wps", StringComparison.OrdinalIgnoreCase)
                    || n.Contains("金山", StringComparison.OrdinalIgnoreCase);
            }

            return true;
        }

        private bool 是否应过滤的产品噪声文件(统一搜索结果项 item, string query)
        {
            if (item == null || !判断核心产品词查询(query))
            {
                return false;
            }

            string title = 规范化(item.标题);
            string target = 规范化(item.目标);
            string subtitle = 规范化(item.副标题);

            if (是否Everything兜底入口(item))
            {
                return true;
            }

            if (结果包含任何(title, "edgecore", "edgeupdate", "update", "updater", "helper", "temp", "cache", "service") ||
                结果包含任何(target, "edgecore", "edgeupdate", "update", "updater", "helper", "temp", "cache", "service") ||
                结果包含任何(subtitle, "edgecore", "edgeupdate", "update", "updater", "helper", "temp", "cache", "service"))
            {
                return true;
            }

            if ((query.Contains("微信", StringComparison.OrdinalIgnoreCase) ||
                 query.Contains("wechat", StringComparison.OrdinalIgnoreCase) ||
                 query.Contains("weixin", StringComparison.OrdinalIgnoreCase)) &&
                (title.Contains("输入法", StringComparison.OrdinalIgnoreCase) || target.Contains("输入法", StringComparison.OrdinalIgnoreCase)))
            {
                return true;
            }

            return false;
        }

        private bool 是否Everything兜底入口(统一搜索结果项 item)
        {
            if (item == null)
            {
                return false;
            }

            string title = item.标题 ?? string.Empty;

            return (item.Id?.StartsWith("everything-search::", StringComparison.OrdinalIgnoreCase) ?? false)
                || (item.Id?.StartsWith("everything-fallback::", StringComparison.OrdinalIgnoreCase) ?? false)
                || title.StartsWith("用 Everything 搜文件：", StringComparison.OrdinalIgnoreCase)
                || title.StartsWith("在 Everything 中搜索：", StringComparison.OrdinalIgnoreCase);
        }

        private List<统一搜索结果项> 从Everything结果提取应用候选(List<统一搜索结果项> 文件结果, string query)
        {
            bool 核心产品词查询 = 判断核心产品词查询(query);

            return 文件结果
                .Where(是否Everything应用候选)
                .Select(x =>
                {
                    string name = Path.GetFileNameWithoutExtension(x.标题);
                    if (string.IsNullOrWhiteSpace(name))
                    {
                        name = Path.GetFileNameWithoutExtension(x.目标 ?? string.Empty);
                    }

                    if (string.IsNullOrWhiteSpace(name))
                    {
                        name = x.标题;
                    }

                    return new 统一搜索结果项
                    {
                        Id = $"app-bridge::{x.目标}",
                        标题 = name,
                        副标题 = $"应用 · {Path.GetDirectoryName(x.目标 ?? string.Empty) ?? string.Empty}",
                        图标路径 = string.Empty,
                        来源 = "AppBridge",
                        目标 = x.目标,
                        命中说明 = 获取命中说明(name, 构建应用别名(name), query),
                        主动作文案 = "打开",
                        类型 = 统一搜索结果类型.应用,
                        次动作列表 = new List<统一搜索次动作>
                        {
                            new() { 类型 = 统一搜索次动作类型.以管理员身份运行, 文案 = "管理员运行" },
                            new() { 类型 = 统一搜索次动作类型.打开位置, 文案 = "打开位置" },
                            new() { 类型 = 统一搜索次动作类型.固定到常用, 文案 = "固定到常用" },
                            new() { 类型 = 统一搜索次动作类型.查看详情, 文案 = "查看详情" }
                        }
                    };
                })
                .Where(x => !是否应过滤的噪声应用(x.标题, query))
                .Where(x => !核心产品词查询 || 是否核心产品主应用候选(query, x.标题))
                .GroupBy(x => x.目标 ?? string.Empty, StringComparer.OrdinalIgnoreCase)
                .Select(x => x.First())
                .ToList();
        }

        private bool 是否Everything应用候选(统一搜索结果项 item)
        {
            if (!string.Equals(item.来源, "Everything", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            string path = item.目标 ?? string.Empty;
            if (string.IsNullOrWhiteSpace(path))
            {
                return false;
            }

            bool 是可启动文件 =
                path.EndsWith(".lnk", StringComparison.OrdinalIgnoreCase) ||
                path.EndsWith(".exe", StringComparison.OrdinalIgnoreCase) ||
                path.EndsWith(".appref-ms", StringComparison.OrdinalIgnoreCase);

            if (!是可启动文件)
            {
                return false;
            }

            return path.Contains(@"\Start Menu", StringComparison.OrdinalIgnoreCase)
                || path.Contains(@"\Programs", StringComparison.OrdinalIgnoreCase)
                || path.Contains(@"\Desktop", StringComparison.OrdinalIgnoreCase)
                || path.Contains(@"\Public\Desktop", StringComparison.OrdinalIgnoreCase);
        }

        private string 构建应用别名(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return string.Empty;
            }

            var tokens = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                name,
                name.Replace(" - 快捷方式", string.Empty, StringComparison.OrdinalIgnoreCase),
                name.Replace("快捷方式", string.Empty, StringComparison.OrdinalIgnoreCase).Trim()
            };

            void Add(string text)
            {
                if (!string.IsNullOrWhiteSpace(text))
                {
                    tokens.Add(text.Trim());
                }
            }

            string lower = name.ToLowerInvariant();
            bool 是卸载项 = lower.Contains("卸载") || lower.Contains("uninstall") || lower.Contains("remove");
            bool 是输入法 = name.Contains("输入法", StringComparison.OrdinalIgnoreCase) || lower.Contains("ime");

            if (lower.Contains("wechat") || lower.Contains("weixin") || (name.Contains("微信", StringComparison.OrdinalIgnoreCase) && !是输入法 && !是卸载项))
            {
                Add("微信 wechat weixin 腾讯微信");
            }

            if (name.Contains("微信输入法", StringComparison.OrdinalIgnoreCase))
            {
                Add("微信输入法 输入法");
            }

            if (lower.Contains("wecom") || name.Contains("企业微信", StringComparison.OrdinalIgnoreCase))
            {
                Add("企业微信 wecom wxwork");
            }

            if ((lower.Contains("feishu") || lower.Contains("lark") || name.Contains("飞书", StringComparison.OrdinalIgnoreCase)) && !是卸载项)
            {
                Add("飞书 feishu lark");
            }

            if ((lower.Contains("dingtalk") || name.Contains("钉钉", StringComparison.OrdinalIgnoreCase)) && !是卸载项)
            {
                Add("钉钉 dingtalk");
            }

            if (lower.Contains("qq") || name.Contains("qq", StringComparison.OrdinalIgnoreCase))
            {
                Add("QQ 腾讯QQ");
            }

            if (lower.Contains("chrome") || name.Contains("谷歌", StringComparison.OrdinalIgnoreCase))
            {
                Add("Chrome Google Chrome 谷歌浏览器");
            }

            if (lower.Contains("edge") || name.Contains("edge", StringComparison.OrdinalIgnoreCase))
            {
                Add("Edge Microsoft Edge 浏览器");
            }

            if (lower.Contains("wps") || name.Contains("wps", StringComparison.OrdinalIgnoreCase))
            {
                Add("WPS 金山办公");
            }

            if (是输入法)
            {
                Add("输入法");
            }

            return string.Join(' ', tokens.Where(x => !string.IsNullOrWhiteSpace(x)).Distinct(StringComparer.OrdinalIgnoreCase));
        }

        private List<统一搜索结果项> 搜索快捷动作(string query)
        {
            return 获取快捷动作定义()
                .Where(x => 命中(x.标题, x.别名, query))
                .Select(x =>
                {
                    var item = 创建页面动作(x.Id, x.标题, x.说明, x.跳转目标);
                    item.命中说明 = 获取命中说明(x.标题, x.别名, query);
                    return item;
                })
                .ToList();
        }

        private 统一搜索结果项 创建页面动作(string id, string 标题, string 说明, string 跳转目标)
        {
            return new 统一搜索结果项
            {
                Id = id,
                标题 = 标题,
                副标题 = $"快捷动作 · {说明}",
                图标路径 = string.Empty,
                来源 = "PageAction",
                目标 = $"page::{跳转目标}",
                命中说明 = "动作匹配",
                主动作文案 = "进入",
                类型 = 统一搜索结果类型.快捷动作,
                次动作列表 = new List<统一搜索次动作>
                {
                    new() { 类型 = 统一搜索次动作类型.固定到常用, 文案 = "固定到常用" },
                    new() { 类型 = 统一搜索次动作类型.查看详情, 文案 = "查看详情" }
                }
            };
        }

                private List<统一搜索结果项> 搜索设置(string query)
        {
            return 获取设置定义()
                .Where(x => 命中(x.标题, x.别名, query))
                .Select(x =>
                {
                    bool 是系统入口 = x.Uri.StartsWith("command::", StringComparison.OrdinalIgnoreCase);

                    return new 统一搜索结果项
                    {
                        Id = $"setting::{x.Uri}",
                        标题 = x.标题,
                        副标题 = 是系统入口 ? $"系统入口 · {x.说明}" : $"设置 · {x.说明}",
                        来源 = 是系统入口 ? "SystemEntry" : "Setting",
                        目标 = x.Uri,
                        命中说明 = 获取命中说明(x.标题, x.别名, query),
                        主动作文案 = 是系统入口 ? "打开" : "打开设置",
                        类型 = 统一搜索结果类型.设置,
                        次动作列表 = new List<统一搜索次动作>
                        {
                            new() { 类型 = 统一搜索次动作类型.固定到常用, 文案 = "固定到常用" },
                            new() { 类型 = 统一搜索次动作类型.查看详情, 文案 = "查看详情" }
                        }
                    };
                })
                .ToList();
        }
        private List<统一搜索结果项> 搜索网站(string query)
        {
            return 获取网站定义()
                .Where(x => 网站是否命中(x, query))
                .Select(x => new 统一搜索结果项
                {
                    Id = $"web::{x.Url}",
                    标题 = x.标题,
                    副标题 = $"网站 · {x.说明}",
                    来源 = "Website",
                    目标 = x.Url,
                    命中说明 = 获取命中说明(x.标题, x.别名, query),
                    主动作文案 = "打开网站",
                    类型 = 统一搜索结果类型.网站,
                    次动作列表 = new List<统一搜索次动作>
                    {
                        new() { 类型 = 统一搜索次动作类型.固定到常用, 文案 = "固定到常用" },
                        new() { 类型 = 统一搜索次动作类型.查看详情, 文案 = "查看详情" }
                    }
                })
                .ToList();
        }

        private List<统一搜索结果项> 排序结果(
List<统一搜索结果项> items,
string query,
bool 文件意图,
统一搜索结果类型 type)
{
    string 规范化查询 = 规范化(query);
    bool 网站意图 = 判断网站意图(规范化查询);
    bool 系统入口意图 = 判断系统入口意图(规范化查询);
    bool 超短查询 = 规范化查询.Length <= 1;
    bool 短查询 = 规范化查询.Length <= 2;

    foreach (var item in items)
    {
        var 行为信息 = _搜索历史服务.获取行为信息(item.Id);
        item.是否置顶 = 行为信息.是否置顶;
        item.是否最近使用 = 行为信息.最近选择时间.HasValue &&
            (DateTime.Now - 行为信息.最近选择时间.Value).TotalDays <= 7;

        item.分数 = 获取类型基础分(type, 文件意图);
        item.分数 += 获取匹配分(item.标题, query, item.命中说明);
        item.分数 += 获取意图调整分(item, 规范化查询, 文件意图, type, 网站意图, 系统入口意图, 超短查询, 短查询);
        item.分数 += 行为信息.是否置顶 ? 80 : 0;
        item.分数 += Math.Min(40, 行为信息.选择次数 * 6);

        if (行为信息.最近选择时间.HasValue)
        {
            double days = (DateTime.Now - 行为信息.最近选择时间.Value).TotalDays;
            item.分数 += Math.Max(0, 30 - days * 4);
        }
    }

    return items
        .OrderByDescending(x => x.分数)
        .ThenBy(x => 获取最佳匹配次序(x, 文件意图))
        .ToList();
}


private double 获取意图调整分(
    统一搜索结果项 item,
    string query,
    bool 文件意图,
    统一搜索结果类型 type,
    bool 网站意图,
    bool 系统入口意图,
    bool 超短查询,
    bool 短查询)
{
    string 标题 = 规范化(item.标题);
    string 副标题 = 规范化(item.副标题);

    bool 来自Everything = string.Equals(item.来源, "Everything", StringComparison.OrdinalIgnoreCase);
    bool 来自网站 = type == 统一搜索结果类型.网站 ||
        string.Equals(item.来源, "Website", StringComparison.OrdinalIgnoreCase);
    bool 是系统入口 = string.Equals(item.来源, "SystemEntry", StringComparison.OrdinalIgnoreCase);
    bool 是快捷方式 = 标题.EndsWith(".lnk", StringComparison.OrdinalIgnoreCase);
    bool 是Recent = 副标题.Contains(@"\recent", StringComparison.OrdinalIgnoreCase);
    bool 是图片文件 =
        标题.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
        标题.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
        标题.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase) ||
        标题.EndsWith(".webp", StringComparison.OrdinalIgnoreCase) ||
        标题.EndsWith(".gif", StringComparison.OrdinalIgnoreCase) ||
        标题.EndsWith(".bmp", StringComparison.OrdinalIgnoreCase);

    bool 精确匹配 = 标题 == query;
    bool 前缀匹配 = 标题.StartsWith(query, StringComparison.OrdinalIgnoreCase);
    bool 别名匹配 = !string.IsNullOrWhiteSpace(item.命中说明) &&
        item.命中说明.Contains("别名", StringComparison.OrdinalIgnoreCase);
    bool 是Everything兜底入口 =
        (item.Id?.StartsWith("everything-search::", StringComparison.OrdinalIgnoreCase) ?? false) ||
        (item.Id?.StartsWith("everything-fallback::", StringComparison.OrdinalIgnoreCase) ?? false) ||
        标题.StartsWith("用 everything 搜文件：", StringComparison.OrdinalIgnoreCase) ||
        标题.StartsWith("在 everything 中搜索：", StringComparison.OrdinalIgnoreCase);

    bool 查询包含噪声词 = 查询包含任何(query, "卸载", "修复", "更新", "输入法", "helper", "updater", "plugin", "组件");
    bool 结果是噪声应用 = 结果包含任何(标题, "卸载", "修复", "更新", "更新器", "updater", "helper", "插件", "组件")
        || (标题.Contains("输入法", StringComparison.OrdinalIgnoreCase) && !query.Contains("输入法", StringComparison.OrdinalIgnoreCase));
    bool 产品查询 = 查询包含任何(query, "微信", "wechat", "weixin", "飞书", "feishu", "lark", "钉钉", "dingtalk", "qq", "edge", "chrome", "wps");
    bool 安装目录噪声 = type == 统一搜索结果类型.文件夹 && 结果包含任何(标题, "edgecore", "edgeupdate", "update", "updater", "helper", "temp", "cache", "service");

    double score = 0;

    if (!文件意图)
    {
        if (是系统入口)
        {
            score += 220;
        }

        if (系统入口意图 && type == 统一搜索结果类型.设置)
        {
            score += 120;
        }

        if (type == 统一搜索结果类型.应用 && 精确匹配)
        {
            score += 180;
        }

        if (type == 统一搜索结果类型.应用 && 前缀匹配)
        {
            score += 60;
        }

        if (type == 统一搜索结果类型.应用 && 别名匹配)
        {
            score += 60;
        }

        if (产品查询 && type == 统一搜索结果类型.应用 && (精确匹配 || 前缀匹配 || 别名匹配))
        {
            score += 140;
        }

        if (string.Equals(item.来源, "AppBridge", StringComparison.OrdinalIgnoreCase))
        {
            score += 130;
        }

        if (产品查询 && string.Equals(item.来源, "AppBridge", StringComparison.OrdinalIgnoreCase))
        {
            score += 60;
        }

        if (type == 统一搜索结果类型.设置 && 精确匹配)
        {
            score += 170;
        }

        if (type == 统一搜索结果类型.快捷动作 && 精确匹配)
        {
            score += 120;
        }

        if (type == 统一搜索结果类型.快捷动作 && 前缀匹配)
        {
            score += 40;
        }

        if (来自网站 && !网站意图)
        {
            score -= 140;
        }

        if (产品查询 && 来自网站 && 标题 == query)
        {
            score += 320;
        }

        if (来自网站 && 标题 == query && query.Length <= 4)
        {
            score += 220;
        }

        if (来自Everything)
        {
            score -= 160;
        }

        if (是Everything兜底入口)
        {
            score -= 产品查询 ? 420 : 260;
        }

        if (是快捷方式)
        {
            score -= 180;
        }

        if (来自Everything && (
            副标题.Contains(@"\start menu", StringComparison.OrdinalIgnoreCase) ||
            副标题.Contains(@"\desktop", StringComparison.OrdinalIgnoreCase)))
        {
            score -= 120;
        }

        if (产品查询 && 来自Everything && type == 统一搜索结果类型.文件夹)
        {
            score -= 160;
        }

        if (产品查询 && 安装目录噪声)
        {
            score -= 220;
        }

        if (是Recent)
        {
            score -= 140;
        }

        if (是图片文件)
        {
            score -= 120;
        }

        if (短查询 && 来自Everything)
        {
            score -= 80;
        }

        if (短查询 && 来自网站)
        {
            score -= 120;
        }

        if (超短查询 && (type == 统一搜索结果类型.文件 || type == 统一搜索结果类型.文件夹))
        {
            score -= 120;
        }

        if (!精确匹配 && 前缀匹配 && 来自网站 && !网站意图)
        {
            score -= 40;
        }

        if (!查询包含噪声词 && 结果是噪声应用 && type == 统一搜索结果类型.应用)
        {
            score -= 产品查询 ? 900 : 420;
        }

        if (!查询包含噪声词 && 结果是噪声应用 && string.Equals(item.来源, "AppBridge", StringComparison.OrdinalIgnoreCase))
        {
            score -= 产品查询 ? 360 : 180;
        }
    }
    else
    {
        if (type == 统一搜索结果类型.文件 || type == 统一搜索结果类型.文件夹)
        {
            score += 60;
        }

        if (来自Everything)
        {
            score += 30;
        }

        if (type == 统一搜索结果类型.网站)
        {
            score -= 180;
        }

        if (type == 统一搜索结果类型.快捷动作)
        {
            score -= 100;
        }
    }

    return score;
}

private bool 网站是否命中(网站定义 site, string query)
{
    string q = 规范化(query);
    if (string.IsNullOrWhiteSpace(q))
    {
        return false;
    }

    string 标题 = 规范化(site.标题);
    string 别名 = 规范化(site.别名);

    if (site.标题 == "微信公众平台")
    {
        return q.Contains("公众号", StringComparison.OrdinalIgnoreCase)
            || q.Contains("公众平台", StringComparison.OrdinalIgnoreCase)
            || q.Contains("mp.weixin", StringComparison.OrdinalIgnoreCase)
            || q.Contains("后台", StringComparison.OrdinalIgnoreCase);
    }

    return 标题.Contains(q, StringComparison.OrdinalIgnoreCase)
        || (!string.IsNullOrWhiteSpace(别名) && 别名.Contains(q, StringComparison.OrdinalIgnoreCase));
}

private bool 查询包含任何(string query, params string[] keywords)
{
    if (string.IsNullOrWhiteSpace(query) || keywords == null)
    {
        return false;
    }

    return keywords.Any(x => !string.IsNullOrWhiteSpace(x) && query.Contains(x, StringComparison.OrdinalIgnoreCase));
}

private bool 判断核心产品词查询(string query)
{
    if (string.IsNullOrWhiteSpace(query))
    {
        return false;
    }

    return string.Equals(query, "微信", StringComparison.OrdinalIgnoreCase)
        || string.Equals(query, "wechat", StringComparison.OrdinalIgnoreCase)
        || string.Equals(query, "weixin", StringComparison.OrdinalIgnoreCase)
        || string.Equals(query, "飞书", StringComparison.OrdinalIgnoreCase)
        || string.Equals(query, "feishu", StringComparison.OrdinalIgnoreCase)
        || string.Equals(query, "lark", StringComparison.OrdinalIgnoreCase)
        || string.Equals(query, "钉钉", StringComparison.OrdinalIgnoreCase)
        || string.Equals(query, "dingtalk", StringComparison.OrdinalIgnoreCase)
        || string.Equals(query, "edge", StringComparison.OrdinalIgnoreCase)
        || string.Equals(query, "chrome", StringComparison.OrdinalIgnoreCase)
        || string.Equals(query, "qq", StringComparison.OrdinalIgnoreCase)
        || string.Equals(query, "wps", StringComparison.OrdinalIgnoreCase);
}

private bool 结果包含任何(string text, params string[] keywords)
{
    if (string.IsNullOrWhiteSpace(text) || keywords == null)
    {
        return false;
    }

    return keywords.Any(x => !string.IsNullOrWhiteSpace(x) && text.Contains(x, StringComparison.OrdinalIgnoreCase));
}

private bool 判断网站意图(string query)
{
    if (string.IsNullOrWhiteSpace(query))
    {
        return false;
    }

    return query.Contains("官网", StringComparison.OrdinalIgnoreCase)
        || query.Contains("网站", StringComparison.OrdinalIgnoreCase)
        || query.Contains("平台", StringComparison.OrdinalIgnoreCase)
        || query.Contains("后台", StringComparison.OrdinalIgnoreCase)
        || query.Contains("入口", StringComparison.OrdinalIgnoreCase)
        || query.Contains("登录", StringComparison.OrdinalIgnoreCase)
        || query.Contains("公众号", StringComparison.OrdinalIgnoreCase)
        || query.Contains("openai", StringComparison.OrdinalIgnoreCase)
        || query.Contains("github", StringComparison.OrdinalIgnoreCase)
        || query.Contains("feishu", StringComparison.OrdinalIgnoreCase)
        || query.Contains("dingtalk", StringComparison.OrdinalIgnoreCase)
        || query.Contains("飞书", StringComparison.OrdinalIgnoreCase)
        || query.Contains("钉钉", StringComparison.OrdinalIgnoreCase)
        || query.Contains("wechat", StringComparison.OrdinalIgnoreCase);
}

private bool 判断系统入口意图(string query)
{
    if (string.IsNullOrWhiteSpace(query))
    {
        return false;
    }

    return query.Contains("控制面板", StringComparison.OrdinalIgnoreCase)
        || query.Contains("注册表", StringComparison.OrdinalIgnoreCase)
        || query.Contains("服务", StringComparison.OrdinalIgnoreCase)
        || query.Contains("设备管理", StringComparison.OrdinalIgnoreCase)
        || query.Contains("磁盘管理", StringComparison.OrdinalIgnoreCase)
        || query.Contains("任务管理", StringComparison.OrdinalIgnoreCase)
        || query.Contains("命令提示符", StringComparison.OrdinalIgnoreCase)
        || query.Contains("powershell", StringComparison.OrdinalIgnoreCase)
        || query.Contains("系统配置", StringComparison.OrdinalIgnoreCase)
        || query.Contains("终端", StringComparison.OrdinalIgnoreCase);
}

private int 获取最佳匹配次序(统一搜索结果项 item, bool 文件意图)
{
    if (!文件意图 && string.Equals(item.来源, "SystemEntry", StringComparison.OrdinalIgnoreCase))
    {
        return -10;
    }

    if (!文件意图 && string.Equals(item.来源, "AppBridge", StringComparison.OrdinalIgnoreCase))
    {
        return -5;
    }

    if (文件意图)
    {
        return item.类型 switch
        {
            统一搜索结果类型.文件 => 0,
            统一搜索结果类型.文件夹 => 1,
            统一搜索结果类型.应用 => 2,
            统一搜索结果类型.设置 => 3,
            统一搜索结果类型.快捷动作 => 4,
            统一搜索结果类型.网站 => 5,
            _ => 9
        };
    }

    return item.类型 switch
    {
        统一搜索结果类型.应用 => 0,
        统一搜索结果类型.快捷动作 => 1,
        统一搜索结果类型.设置 => 2,
        统一搜索结果类型.网站 => 3,
        统一搜索结果类型.文件夹 => 4,
        统一搜索结果类型.文件 => 5,
        _ => 9
    };
}

private 统一搜索结果项? 生成最佳匹配(List<统一搜索结果项> all, bool 文件意图)
{
    if (all.Count == 0)
    {
        return null;
    }

    return all
        .OrderByDescending(x => x.分数)
        .ThenBy(x => 获取最佳匹配次序(x, 文件意图))
        .FirstOrDefault();
}

private double 获取类型基础分(统一搜索结果类型 type, bool 文件意图)
        {
            if (文件意图)
            {
                return type switch
                {
                    统一搜索结果类型.文件 => 980,
                    统一搜索结果类型.文件夹 => 920,
                    统一搜索结果类型.应用 => 900,
                    统一搜索结果类型.快捷动作 => 780,
                    统一搜索结果类型.设置 => 720,
                    统一搜索结果类型.网站 => 650,
                    _ => 0
                };
            }

            return type switch
            {
                统一搜索结果类型.应用 => 1000,
                统一搜索结果类型.快捷动作 => 900,
                统一搜索结果类型.设置 => 850,
                统一搜索结果类型.文件 => 760,
                统一搜索结果类型.文件夹 => 720,
                统一搜索结果类型.网站 => 650,
                _ => 0
            };
        }

        private double 获取匹配分(string title, string query, string matchReason)
        {
            string t = 规范化(title);
            string q = 规范化(query);
            string 基础名 = 规范化(Path.GetFileNameWithoutExtension(title ?? string.Empty));

            if (t == q)
            {
                return 100;
            }

            if (!string.IsNullOrWhiteSpace(基础名) && 基础名 == q)
            {
                return 96;
            }

            if (t.StartsWith(q, StringComparison.OrdinalIgnoreCase))
            {
                double score = 85;

                if (!string.IsNullOrWhiteSpace(基础名) && 基础名.StartsWith(q, StringComparison.OrdinalIgnoreCase))
                {
                    int 额外长度 = Math.Max(0, 基础名.Length - q.Length);
                    score -= Math.Min(18, 额外长度 * 3);
                }

                return score;
            }

            if (!string.IsNullOrWhiteSpace(matchReason) && matchReason.Contains("别名"))
            {
                return 75;
            }

            if (t.Contains(q, StringComparison.OrdinalIgnoreCase))
            {
                return 45;
            }

            return 10;
        }


        private static void 添加分组(
            List<统一搜索结果分组> groups,
            string 标题,
            统一搜索结果类型 类型,
            List<统一搜索结果项> items,
            int maxCount)
        {
            _ = maxCount;

            if (groups == null || items == null || items.Count == 0)
            {
                return;
            }

            groups.Add(new 统一搜索结果分组
            {
                标题 = 标题,
                类型 = 类型,
                结果列表 = items.ToList()
            });
        }

        private bool 命中(string title, string alias, string query)
        {
            string q = 规范化(query);
            if (string.IsNullOrWhiteSpace(q))
            {
                return false;
            }

            string t = 规范化(title);
            string a = 规范化(alias);

            return t.Contains(q, StringComparison.OrdinalIgnoreCase)
                   || (!string.IsNullOrWhiteSpace(a) && a.Contains(q, StringComparison.OrdinalIgnoreCase));
        }

        private string 获取命中说明(string title, string alias, string query)
        {
            string q = 规范化(query);
            string t = 规范化(title);
            string a = 规范化(alias);

            if (t == q) return "精确匹配";
            if (t.StartsWith(q, StringComparison.OrdinalIgnoreCase)) return "前缀匹配";
            if (!string.IsNullOrWhiteSpace(a) && a.Contains(q, StringComparison.OrdinalIgnoreCase)) return "别名匹配";
            return "包含匹配";
        }

        private bool 是否文件意图(string query)
        {
            string q = query ?? string.Empty;
            return q.Contains('.') || q.Contains('\\') || q.Contains('/') || q.Contains(':')
                   || q.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase)
                   || q.EndsWith(".docx", StringComparison.OrdinalIgnoreCase)
                   || q.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase)
                   || q.EndsWith(".pptx", StringComparison.OrdinalIgnoreCase)
                   || q.EndsWith(".ps1", StringComparison.OrdinalIgnoreCase);
        }

        private string 规范化(string value)
        {
            return (value ?? string.Empty).Trim().ToLowerInvariant();
        }

        private void 打开位置(string target)
        {
            if (string.IsNullOrWhiteSpace(target))
            {
                throw new InvalidOperationException("目标为空，无法打开位置。");
            }

            if (File.Exists(target))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "explorer.exe",
                    Arguments = $"/select,\"{target}\"",
                    UseShellExecute = true
                });
                return;
            }

            if (Directory.Exists(target))
            {
                打开进程(target);
                return;
            }

            throw new FileNotFoundException("未找到目标位置。");
        }

        private void 以管理员身份运行(string target)
        {
            if (string.IsNullOrWhiteSpace(target) || !File.Exists(target))
            {
                throw new FileNotFoundException("未找到可执行目标。");
            }

            Process.Start(new ProcessStartInfo
            {
                FileName = target,
                Verb = "runas",
                UseShellExecute = true
            });
        }

        private void 打开命令(string command)
        {
            if (string.IsNullOrWhiteSpace(command))
            {
                throw new InvalidOperationException("命令为空，无法执行。");
            }

            Process.Start(new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = "/c start \"\" " + command,
                UseShellExecute = false,
                CreateNoWindow = true
            });
        }

        private void 打开进程(string target)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = target,
                UseShellExecute = true
            });
        }

        private List<应用定义> 获取应用列表()
        {
            lock (_应用缓存锁)
            {
                if (_应用缓存 != null && DateTime.Now - _应用缓存时间 < TimeSpan.FromMinutes(20))
                {
                    return _应用缓存;
                }

                var list = new List<应用定义>();

                foreach (string root in new[]
                {
                    Environment.GetFolderPath(Environment.SpecialFolder.StartMenu),
                    Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu),
                    Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory),
                    Environment.GetFolderPath(Environment.SpecialFolder.CommonDesktopDirectory)
                })
                {
                    if (string.IsNullOrWhiteSpace(root) || !Directory.Exists(root))
                    {
                        continue;
                    }

                    try
                    {
                        foreach (string file in Directory.EnumerateFiles(root, "*.lnk", SearchOption.AllDirectories))
                        {
                            string name = Path.GetFileNameWithoutExtension(file);
                            if (string.IsNullOrWhiteSpace(name))
                            {
                                continue;
                            }

                            list.Add(new 应用定义
                            {
                                Name = name,
                                Alias = 构建应用别名(name),
                                ShortcutPath = file
                            });
                        }
                    }
                    catch
                    {
                    }
                }

                _应用缓存 = list
                    .GroupBy(x => x.ShortcutPath, StringComparer.OrdinalIgnoreCase)
                    .Select(x => x.First())
                    .OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
                    .ToList();
                _应用缓存时间 = DateTime.Now;

                return _应用缓存;
            }
        }

        private List<快捷动作定义> 获取快捷动作定义()
        {
            return new List<快捷动作定义>
            {
                new() { Id = "page::我的电脑|电脑概览", 标题 = "电脑概览", 别名 = "看看这台电脑 我的电脑 电脑", 说明 = "查看设备概览。", 跳转目标 = "我的电脑|电脑概览" },
                new() { Id = "page::我的电脑|详细配置", 标题 = "详细配置", 别名 = "配置 硬件 详细", 说明 = "查看硬件与系统配置。", 跳转目标 = "我的电脑|详细配置" },
                new() { Id = "page::电脑优化|常用设置", 标题 = "常用设置", 别名 = "让电脑更顺手 优化 设置", 说明 = "处理高频系统习惯项。", 跳转目标 = "电脑优化|常用设置" },
                new() { Id = "page::电脑优化|一键优化", 标题 = "一键优化", 别名 = "优化 体检", 说明 = "查看优化建议。", 跳转目标 = "电脑优化|一键优化" },
                new() { Id = "page::软件中心|安装常用软件", 标题 = "安装常用软件", 别名 = "安装 软件 常用软件", 说明 = "安装高频软件。", 跳转目标 = "软件中心|安装常用软件" },
                new() { Id = "page::软件中心|软件目录", 标题 = "软件目录", 别名 = "软件目录 软件列表", 说明 = "查看全部软件目录。", 跳转目标 = "软件中心|软件目录" },
                new() { Id = "page::开始使用|首页", 标题 = "网页导航管理", 别名 = "网站 网站导航 常用网站 首页", 说明 = "在首页直接管理高频网站。", 跳转目标 = "开始使用|首页" },
                new() { Id = "page::开始使用|WPS快捷键", 标题 = "WPS快捷键", 别名 = "wps 快捷键 文档 办公", 说明 = "查看 WPS 常用快捷键。", 跳转目标 = "开始使用|WPS快捷键" }
            };
        }

                private List<设置定义> 获取设置定义()
        {
            return new List<设置定义>
            {
                new() { 标题 = "控制面板", 别名 = "控制面板 control panel 传统控制面板", Uri = "command::control.exe", 说明 = "打开传统控制面板。" },
                new() { 标题 = "注册表编辑器", 别名 = "注册表 regedit 注册表编辑器", Uri = "command::regedit.exe", 说明 = "打开注册表编辑器。" },
                new() { 标题 = "服务", 别名 = "服务 services 系统服务", Uri = "command::services.msc", 说明 = "管理 Windows 服务。" },
                new() { 标题 = "设备管理器", 别名 = "设备管理器 device manager 驱动", Uri = "command::devmgmt.msc", 说明 = "管理硬件设备和驱动。" },
                new() { 标题 = "磁盘管理", 别名 = "磁盘管理 磁盘 分区 disk management", Uri = "command::diskmgmt.msc", 说明 = "查看磁盘与分区。" },
                new() { 标题 = "任务管理器", 别名 = "任务管理器 task manager 进程", Uri = "command::taskmgr.exe", 说明 = "查看进程与性能。" },
                new() { 标题 = "命令提示符", 别名 = "命令提示符 cmd 终端", Uri = "command::cmd.exe", 说明 = "打开命令提示符。" },
                new() { 标题 = "PowerShell", 别名 = "powershell 终端 shell", Uri = "command::powershell.exe", 说明 = "打开 PowerShell。" },
                new() { 标题 = "系统配置", 别名 = "系统配置 msconfig 启动项", Uri = "command::msconfig.exe", 说明 = "打开系统配置工具。" },
                new() { 标题 = "蓝牙设置", 别名 = "蓝牙 bluetooth", Uri = "ms-settings:bluetooth", 说明 = "管理蓝牙设备。" },
                new() { 标题 = "存储设置", 别名 = "存储 磁盘 空间", Uri = "ms-settings:storagesense", 说明 = "查看存储与清理。" },
                new() { 标题 = "默认应用", 别名 = "默认应用 关联 程序", Uri = "ms-settings:defaultapps", 说明 = "设置默认应用。" },
                new() { 标题 = "显示设置", 别名 = "显示 分辨率 屏幕", Uri = "ms-settings:display", 说明 = "调整显示参数。" },
                new() { 标题 = "声音设置", 别名 = "声音 音量 音频", Uri = "ms-settings:sound", 说明 = "调整系统声音。" },
                new() { 标题 = "网络设置", 别名 = "网络 wifi 以太网", Uri = "ms-settings:network", 说明 = "查看网络状态。" }
            };
        }
        private List<网站定义> 获取网站定义()
        {
            return new List<网站定义>
            {
                new() { 标题 = "GitHub", 别名 = "github 代码 仓库", Url = "https://github.com", 说明 = "代码托管平台。" },
                new() { 标题 = "微信", 别名 = "微信 wechat weixin 腾讯微信 下载", Url = "https://weixin.qq.com", 说明 = "微信官网。" },
                new() { 标题 = "微信公众平台", 别名 = "公众号 公众平台 mp.weixin 后台", Url = "https://mp.weixin.qq.com", 说明 = "微信公众平台后台。" },
                new() { 标题 = "飞书", 别名 = "飞书 feishu lark", Url = "https://www.feishu.cn", 说明 = "飞书官网。" },
                new() { 标题 = "钉钉", 别名 = "钉钉 dingtalk", Url = "https://www.dingtalk.com", 说明 = "钉钉官网。" },
                new() { 标题 = "Bing", 别名 = "搜索 bing", Url = "https://www.bing.com", 说明 = "Bing 搜索。" }
            };
        }

        private sealed class 应用定义
        {
            public string Name { get; set; } = string.Empty;
            public string Alias { get; set; } = string.Empty;
            public string ShortcutPath { get; set; } = string.Empty;
        }

        private sealed class 快捷动作定义
        {
            public string Id { get; set; } = string.Empty;
            public string 标题 { get; set; } = string.Empty;
            public string 别名 { get; set; } = string.Empty;
            public string 说明 { get; set; } = string.Empty;
            public string 跳转目标 { get; set; } = string.Empty;
        }

        private sealed class 设置定义
        {
            public string 标题 { get; set; } = string.Empty;
            public string 别名 { get; set; } = string.Empty;
            public string Uri { get; set; } = string.Empty;
            public string 说明 { get; set; } = string.Empty;
        }

        private sealed class 网站定义
        {
            public string 标题 { get; set; } = string.Empty;
            public string 别名 { get; set; } = string.Empty;
            public string Url { get; set; } = string.Empty;
            public string 说明 { get; set; } = string.Empty;
        }
    }
}
