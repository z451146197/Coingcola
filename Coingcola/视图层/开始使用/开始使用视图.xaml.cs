﻿using Coingcola.模型;
using Coingcola.服务;
using Microsoft.Web.WebView2.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace Coingcola.视图层.开始使用
{
    public partial class 开始使用视图 : UserControl
    {
        private readonly 开始使用服务 _开始使用服务 = new();
        private readonly 快捷导航服务 _快捷导航服务 = new();
        private readonly 搜索历史服务 _搜索历史服务 = new();

        private CancellationTokenSource? _首页搜索取消源;
        private bool _已完成首次导航;

        public event Action<string, string>? 请求跳转到功能页;

        public 开始使用视图()
        {
            InitializeComponent();
            Loaded += ForceStretchLayoutOnLoaded;
            Loaded += 开始使用视图_Loaded;
        }

        public void 刷新首页数据() => _ = 推送首页初始化数据Async();

        public void 刷新页面()
        {
            _ = 初始化并导航Async();
        }

        private async void 开始使用视图_Loaded(object sender, RoutedEventArgs e)
        {
            await 初始化并导航Async();
        }

        private async Task 初始化并导航Async()
        {
            string indexPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Resources",
                "Web",
                "Home",
                "index.html");

            if (!File.Exists(indexPath))
            {
                Debug.WriteLine("未找到首页静态资源：" + indexPath);
                return;
            }

            if (Browser.CoreWebView2 == null)
            {
                await Browser.EnsureCoreWebView2Async();
                Browser.DefaultBackgroundColor = System.Drawing.Color.Transparent;

                Browser.CoreWebView2.WebMessageReceived -= CoreWebView2_WebMessageReceived;
                Browser.CoreWebView2.WebMessageReceived += CoreWebView2_WebMessageReceived;

                Browser.NavigationCompleted -= Browser_NavigationCompleted;
                Browser.NavigationCompleted += Browser_NavigationCompleted;

                Browser.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;
                Browser.CoreWebView2.Settings.AreDevToolsEnabled = false;
                Browser.CoreWebView2.Settings.IsZoomControlEnabled = false;
                Browser.CoreWebView2.Settings.IsStatusBarEnabled = false;
            }

            var indexUri = new Uri(indexPath);
            if (Browser.Source == null || !UriEqualsFile(Browser.Source, indexUri))
            {
                _已完成首次导航 = false;
                Browser.Source = indexUri;
                return;
            }

            if (_已完成首次导航)
            {
                await 推送首页初始化数据Async();
            }
        }

        private async void Browser_NavigationCompleted(object? sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            if (!e.IsSuccess)
            {
                Debug.WriteLine("首页加载失败。");
                return;
            }

            _已完成首次导航 = true;
            await 推送首页初始化数据Async();
        }

        private async Task 推送首页初始化数据Async()
        {
            if (!_已完成首次导航 || Browser.CoreWebView2 == null)
            {
                return;
            }

            var payload = new
            {
                navItems = 获取网页导航数据(),
                recentItems = 获取最近使用数据(),
                quickActions = 获取常用入口数据()
            };

            await 调用页面脚本Async("window.coingcolaHome && window.coingcolaHome.receiveHomeData", payload);
        }

        private List<object> 获取网页导航数据()
        {
            return _快捷导航服务
                .获取网站列表()
                
                .Select(x => (object)new
                {
                    id = x.Id,
                    title = string.IsNullOrWhiteSpace(x.名称) ? "未命名网站" : x.名称,
                    url = x.地址,
                    removable = !x.是否固定,
                    group = string.IsNullOrWhiteSpace(x.分组) ? "默认" : x.分组,
                    isPinned = x.是否固定,
                    sort = x.排序,
                    usedAt = x.最近使用时间.HasValue ? x.最近使用时间.Value.ToString("yyyy-MM-dd HH:mm:ss") : string.Empty
                })
                .ToList();
        }

        private List<object> 获取最近使用数据()
        {
            return _开始使用服务
                .获取最近使用()
                .Select(x =>
                {
                    string target = 优先取值(x.跳转目标, string.Empty);
                    string actionType = 识别动作类型(target);
                    (string level1, string level2) = 解析页面目标(target);

                    return (object)new
                    {
                        id = x.Id,
                        title = 优先取值(x.标题, "未命名记录"),
                        subtitle = 优先取值(x.副标题, string.Empty),
                        actionType,
                        target,
                        level1,
                        level2
                    };
                })
                .ToList();
        }

        private static List<object> 获取常用入口数据()
        {
            return new List<object>
            {
                new { id = "quick::optimize", title = "一键优化", iconName = "zap", level1 = "电脑优化", level2 = "一键优化" },
                new { id = "quick::settings", title = "常用设置", iconName = "settings", level1 = "电脑优化", level2 = "常用设置" },
                new { id = "quick::software", title = "软件目录", iconName = "grid", level1 = "软件中心", level2 = "软件目录" },
                new { id = "quick::device", title = "电脑概览", iconName = "device", level1 = "我的电脑", level2 = "电脑概览" },
                new { id = "quick::wps", title = "WPS 快捷键", iconName = "app", level1 = "开始使用", level2 = "WPS快捷键" }
            }.Cast<object>().ToList();
        }

        private async void CoreWebView2_WebMessageReceived(object? sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            if (!TryReadMessageDocument(e, out JsonDocument? doc) || doc == null)
            {
                return;
            }

            using (doc)
            {
                JsonElement root = doc.RootElement;
                string action = ReadString(root, "action");

                try
                {
                    switch (action)
                    {
                        case "homeReady":
                            await 推送首页初始化数据Async();
                            break;

                        case "navigate":
                            执行页面跳转(root);
                            break;

                        case "openQuickAction":
                            if (TryGetNestedObject(root, "item", out JsonElement quickItem))
                            {
                                执行页面跳转(quickItem);
                            }
                            else
                            {
                                执行页面跳转(root);
                            }
                            break;

                        case "openNavItem":
                        case "openUrl":
                            await 执行打开网站Async(root);
                            break;

                        case "deleteNavItem":
                        case "deleteNav":
                            await 执行删除网站Async(root);
                            break;

                        case "saveNavItem":
                            await 执行保存网站Async(root);
                            break;

                        case "saveNavOrder":
                            await 执行保存网站顺序Async(root);
                            break;

                        case "openRecent":
                            await 执行最近使用Async(root);
                            break;

                        case "deleteRecent":
                        case "removeRecent":
                            await 执行删除最近使用Async(root);
                            break;

                        case "clearRecent":
                            _开始使用服务.清空最近使用();
                            await 推送首页初始化数据Async();
                            break;

                        case "search":
                        case "searchHome":
                            await 执行首页搜索Async(优先取值(ReadString(root, "query"), ReadString(root, "keyword")));
                            break;

                        case "searchWeb":
                        case "openSearchWeb":
                            执行网络搜索(ReadString(root, "query"));
                            break;

                        case "executeSearchItem":
                        case "executeUnifiedResult":
                            await 执行搜索结果Async(root);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("首页消息处理失败：" + ex);
                }
            }
        }

        private async Task 执行打开网站Async(JsonElement root)
        {
            string id = ReadString(root, "id");
            string siteId = ReadString(root, "siteId");
            string title = ReadString(root, "title");
            string url = ReadString(root, "url");

            string finalId = 优先取值(id, siteId);
            string finalUrl = 补全网址(url);
            if (string.IsNullOrWhiteSpace(finalUrl))
            {
                return;
            }

            if (!string.IsNullOrWhiteSpace(finalId))
            {
                _快捷导航服务.标记最近使用(finalId);
            }

            记录最近使用(创建网站结果(finalId, title, finalUrl), true);
            打开目标(finalUrl);
            await 推送首页初始化数据Async();
        }

        private async Task 执行删除网站Async(JsonElement root)
        {
            string id = ReadString(root, "id");
            if (string.IsNullOrWhiteSpace(id))
            {
                return;
            }

            _快捷导航服务.删除网站(id);
            await 推送首页初始化数据Async();
        }

        private async Task 执行保存网站Async(JsonElement root)
        {
            string id = ReadString(root, "id");
            string title = ReadString(root, "title").Trim();
            string url = 补全网址(ReadString(root, "url").Trim());
            string group = 优先取值(ReadString(root, "group").Trim(), ReadString(root, "分组").Trim(), "默认");
            bool hasPinned = root.TryGetProperty("isPinned", out _) || root.TryGetProperty("是否固定", out _);
            bool isPinned = ReadBool(root, "isPinned");
            if (!hasPinned)
            {
                isPinned = ReadBool(root, "是否固定");
            }

            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(url))
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(id))
            {
                _快捷导航服务.新增网站(url, title, title, group, isPinned);
            }
            else
            {
                List<导航网址项> sites = _快捷导航服务.获取网站列表();
                导航网址项? target = sites.FirstOrDefault(x => string.Equals(x.Id, id, StringComparison.OrdinalIgnoreCase));
                if (target == null)
                {
                    return;
                }

                target.名称 = title;
                target.地址 = url;
                target.分组 = string.IsNullOrWhiteSpace(group) ? "默认" : group;
                if (hasPinned)
                {
                    target.是否固定 = isPinned;
                }
                if (string.IsNullOrWhiteSpace(target.关键字))
                {
                    target.关键字 = title;
                }

                _快捷导航服务.更新网站(target);
            }

            await 推送首页初始化数据Async();
        }

        private async Task 执行保存网站顺序Async(JsonElement root)
        {
            List<string> ids = ReadIdList(root);
            if (ids.Count == 0)
            {
                return;
            }

            List<导航网址项> current = _快捷导航服务.获取网站列表();
            var map = current.ToDictionary(x => x.Id, x => x, StringComparer.OrdinalIgnoreCase);

            List<导航网址项> ordered = ids
                .Where(id => map.ContainsKey(id))
                .Select(id => map[id])
                .ToList();

            foreach (导航网址项 item in current)
            {
                if (!ordered.Any(x => string.Equals(x.Id, item.Id, StringComparison.OrdinalIgnoreCase)))
                {
                    ordered.Add(item);
                }
            }

            _快捷导航服务.保存网站列表(ordered);
            await 推送首页初始化数据Async();
        }

        private async Task 执行最近使用Async(JsonElement root)
        {
            JsonElement item = root;
            if (TryGetNestedObject(root, "item", out JsonElement nested))
            {
                item = nested;
            }

            string actionType = ReadString(item, "actionType");
            string target = ReadString(item, "target");
            string level1 = ReadString(item, "level1");
            string level2 = ReadString(item, "level2");
            string title = ReadString(item, "title");
            string id = ReadString(item, "id");

            if (string.Equals(actionType, "navigate", StringComparison.OrdinalIgnoreCase) &&
                !string.IsNullOrWhiteSpace(level1) &&
                !string.IsNullOrWhiteSpace(level2))
            {
                记录最近使用(创建页面结果(level1, level2, title), true);
                跳转页面(level1, level2);
                await 推送首页初始化数据Async();
                return;
            }

            if (!string.IsNullOrWhiteSpace(target))
            {
                记录最近使用(创建打开目标结果(id, title, target), true);
                打开目标(看起来像网址(target) ? 补全网址(target) : target);
                await 推送首页初始化数据Async();
            }
        }

        private async Task 执行删除最近使用Async(JsonElement root)
        {
            string id = ReadString(root, "id");
            if (string.IsNullOrWhiteSpace(id) && TryGetNestedObject(root, "item", out JsonElement item))
            {
                id = ReadString(item, "id");
            }

            if (string.IsNullOrWhiteSpace(id))
            {
                return;
            }

            _开始使用服务.删除最近使用(id);
            await 推送首页初始化数据Async();
        }

        private async Task 执行首页搜索Async(string query)
        {
            query = (query ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(query))
            {
                await 调用页面脚本Async("window.coingcolaHome && window.coingcolaHome.receiveSearchResult", new
                {
                    query = string.Empty,
                    bestMatch = (object?)null,
                    groups = Array.Empty<object>()
                });
                return;
            }

            _首页搜索取消源?.Cancel();
            _首页搜索取消源?.Dispose();
            _首页搜索取消源 = new CancellationTokenSource();
            CancellationToken token = _首页搜索取消源.Token;

            await 调用页面脚本Async("window.coingcolaHome && window.coingcolaHome.receiveSearchResult", new
            {
                query,
                bestMatch = (object?)null,
                groups = Array.Empty<object>()
            });

            try
            {
                统一搜索响应 response = await _开始使用服务.搜索统一入口Async(query, token);
                if (token.IsCancellationRequested)
                {
                    return;
                }

                var payload = new
                {
                    query = response.查询词,
                    bestMatch = MapSearchItem(response.最佳匹配),
                    groups = response.分组列表.Select(g => new
                    {
                        title = g.标题,
                        items = g.结果列表.Select(MapSearchItem).ToList()
                    }).ToList()
                };

                await 调用页面脚本Async("window.coingcolaHome && window.coingcolaHome.receiveSearchResult", payload);
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                await 调用页面脚本Async("window.coingcolaHome && window.coingcolaHome.receiveSearchResult", new
                {
                    query,
                    error = ex.Message,
                    bestMatch = (object?)null,
                    groups = Array.Empty<object>()
                });
            }
        }

        private async Task 执行搜索结果Async(JsonElement root)
        {
            string query = ReadString(root, "query");
            bool fromBest = ReadBool(root, "fromBest");

            JsonElement itemRoot = root;
            if (TryGetNestedObject(root, "item", out JsonElement nested))
            {
                itemRoot = nested;
            }

            统一搜索结果项 item = ParseSearchItem(itemRoot);
            if (string.IsNullOrWhiteSpace(item.Id))
            {
                return;
            }

            var result = _开始使用服务.执行主动作(query, item, fromBest);
            if (!string.IsNullOrWhiteSpace(result.一级菜单) && !string.IsNullOrWhiteSpace(result.二级菜单))
            {
                跳转页面(result.一级菜单, result.二级菜单);
            }

            await 推送首页初始化数据Async();
        }

        private void 执行页面跳转(JsonElement element)
        {
            string level1 = ReadString(element, "level1");
            string level2 = ReadString(element, "level2");

            if (!string.IsNullOrWhiteSpace(level1) && !string.IsNullOrWhiteSpace(level2))
            {
                记录最近使用(创建页面结果(level1, level2, level2), true);
                跳转页面(level1, level2);
            }
        }

        private void 执行网络搜索(string query)
        {
            query = (query ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(query))
            {
                return;
            }

            string url = "https://cn.bing.com/search?q=" + Uri.EscapeDataString(query);
            记录最近使用(创建网站结果($"web-search::{query}", $"网络搜索：{query}", url), true);
            打开目标(url);
        }

        private void 记录最近使用(统一搜索结果项 item, bool success)
        {
            try
            {
                _搜索历史服务.记录选择(string.Empty, item, false, success);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("记录最近使用失败：" + ex);
            }
        }

        private void 跳转页面(string level1, string level2)
        {
            if (!string.IsNullOrWhiteSpace(level1) && !string.IsNullOrWhiteSpace(level2))
            {
                请求跳转到功能页?.Invoke(level1, level2);
            }
        }

        private async Task 调用页面脚本Async(string functionCall, object payload)
        {
            if (!_已完成首次导航 || Browser.CoreWebView2 == null)
            {
                return;
            }

            try
            {
                string json = JsonSerializer.Serialize(payload);
                await Browser.CoreWebView2.ExecuteScriptAsync($"{functionCall}({json});");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("调用首页脚本失败：" + ex.Message);
            }
        }

        private static object? MapSearchItem(统一搜索结果项? item)
        {
            if (item == null)
            {
                return null;
            }

            return new
            {
                id = item.Id,
                title = item.标题,
                subtitle = item.副标题,
                hit = item.命中说明,
                target = item.目标,
                resultType = (int)item.类型,
                typeName = item.类型.ToString(),
                tag = item.来源,
                iconName = ToIconName(item)
            };
        }

        private static string ToIconName(统一搜索结果项 item)
        {
            return item.类型 switch
            {
                统一搜索结果类型.应用 => "app",
                统一搜索结果类型.快捷动作 => "zap",
                统一搜索结果类型.设置 => "settings",
                统一搜索结果类型.文件 => "fileText",
                统一搜索结果类型.文件夹 => "folder",
                统一搜索结果类型.网站 => "globe",
                _ => "app"
            };
        }

        private static 统一搜索结果项 ParseSearchItem(JsonElement item)
        {
            int resultType = ReadInt(item, "resultType");
            string typeName = ReadString(item, "typeName");

            return new 统一搜索结果项
            {
                Id = ReadString(item, "id"),
                标题 = ReadString(item, "title"),
                副标题 = ReadString(item, "subtitle"),
                命中说明 = ReadString(item, "hit"),
                目标 = ReadString(item, "target"),
                来源 = ReadString(item, "tag"),
                主动作文案 = "打开",
                类型 = ParseResultType(resultType, typeName, ReadString(item, "target"))
            };
        }

        private static 统一搜索结果类型 ParseResultType(int resultType, string typeName, string target)
        {
            if (Enum.IsDefined(typeof(统一搜索结果类型), resultType) && resultType > 0)
            {
                return (统一搜索结果类型)resultType;
            }

            if (Enum.TryParse(typeName, out 统一搜索结果类型 parsed))
            {
                return parsed;
            }

            if (看起来像网址(target))
            {
                return 统一搜索结果类型.网站;
            }

            return 统一搜索结果类型.应用;
        }

        private static 统一搜索结果项 创建页面结果(string level1, string level2, string? title)
        {
            return new 统一搜索结果项
            {
                Id = $"page::{level1}|{level2}",
                标题 = string.IsNullOrWhiteSpace(title) ? level2 : title.Trim(),
                副标题 = level1,
                来源 = "首页常用入口",
                目标 = $"page::{level1}|{level2}",
                类型 = 统一搜索结果类型.快捷动作
            };
        }

        private static 统一搜索结果项 创建网站结果(string? id, string? title, string url)
        {
            return new 统一搜索结果项
            {
                Id = string.IsNullOrWhiteSpace(id) ? $"site::{url}" : $"site::{id}",
                标题 = 优先取值(title ?? string.Empty, url),
                副标题 = url,
                来源 = "首页网页导航",
                目标 = url,
                类型 = 统一搜索结果类型.网站
            };
        }

        private static 统一搜索结果项 创建打开目标结果(string? id, string? title, string target)
        {
            return new 统一搜索结果项
            {
                Id = string.IsNullOrWhiteSpace(id) ? $"recent::{target}" : id,
                标题 = 优先取值(title ?? string.Empty, target),
                副标题 = target,
                来源 = "最近使用",
                目标 = target,
                类型 = 看起来像网址(target) ? 统一搜索结果类型.网站 : 统一搜索结果类型.文件
            };
        }

        private static string 识别动作类型(string target)
        {
            if (string.IsNullOrWhiteSpace(target))
            {
                return "openTarget";
            }

            if (target.StartsWith("page::", StringComparison.OrdinalIgnoreCase))
            {
                return "navigate";
            }

            return 看起来像网址(target) ? "openUrl" : "openTarget";
        }

        private static (string level1, string level2) 解析页面目标(string target)
        {
            if (string.IsNullOrWhiteSpace(target) || !target.StartsWith("page::", StringComparison.OrdinalIgnoreCase))
            {
                return (string.Empty, string.Empty);
            }

            string raw = target["page::".Length..];
            string[] parts = raw.Split('|', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            return parts.Length >= 2 ? (parts[0], parts[1]) : (string.Empty, string.Empty);
        }

        private static bool TryReadMessageDocument(CoreWebView2WebMessageReceivedEventArgs e, out JsonDocument? doc)
        {
            doc = null;

            string? direct = null;
            try
            {
                direct = e.TryGetWebMessageAsString();
            }
            catch
            {
            }

            if (TryParseDocument(direct, out doc))
            {
                return true;
            }

            try
            {
                string json = e.WebMessageAsJson;
                if (TryParseDocument(json, out doc))
                {
                    return true;
                }
            }
            catch
            {
            }

            return false;
        }

        private static bool TryParseDocument(string? raw, out JsonDocument? doc)
        {
            doc = null;
            if (string.IsNullOrWhiteSpace(raw))
            {
                return false;
            }

            raw = raw.Trim();

            try
            {
                JsonDocument first = JsonDocument.Parse(raw);
                if (first.RootElement.ValueKind == JsonValueKind.String)
                {
                    string? inner = first.RootElement.GetString();
                    first.Dispose();
                    if (!string.IsNullOrWhiteSpace(inner))
                    {
                        doc = JsonDocument.Parse(inner);
                        return true;
                    }

                    return false;
                }

                doc = first;
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static bool TryGetNestedObject(JsonElement root, string name, out JsonElement nested)
        {
            if (root.TryGetProperty(name, out nested) && nested.ValueKind == JsonValueKind.Object)
            {
                return true;
            }

            nested = default;
            return false;
        }

        private static string ReadString(JsonElement root, string propertyName)
        {
            if (!root.TryGetProperty(propertyName, out JsonElement value))
            {
                return string.Empty;
            }

            return value.ValueKind switch
            {
                JsonValueKind.String => value.GetString() ?? string.Empty,
                JsonValueKind.Number => value.ToString(),
                JsonValueKind.True => "true",
                JsonValueKind.False => "false",
                _ => string.Empty
            };
        }

        private static bool ReadBool(JsonElement root, string propertyName)
        {
            if (!root.TryGetProperty(propertyName, out JsonElement value))
            {
                return false;
            }

            if (value.ValueKind == JsonValueKind.True || value.ValueKind == JsonValueKind.False)
            {
                return value.GetBoolean();
            }

            if (value.ValueKind == JsonValueKind.String && bool.TryParse(value.GetString(), out bool result))
            {
                return result;
            }

            return false;
        }

        private static int ReadInt(JsonElement root, string propertyName)
        {
            if (!root.TryGetProperty(propertyName, out JsonElement value))
            {
                return 0;
            }

            if (value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out int result))
            {
                return result;
            }

            if (value.ValueKind == JsonValueKind.String && int.TryParse(value.GetString(), out result))
            {
                return result;
            }

            return 0;
        }

        private static List<string> ReadIdList(JsonElement root)
        {
            var result = new List<string>();

            if (root.TryGetProperty("ids", out JsonElement ids) && ids.ValueKind == JsonValueKind.Array)
            {
                foreach (JsonElement element in ids.EnumerateArray())
                {
                    string id = element.ValueKind == JsonValueKind.String ? (element.GetString() ?? string.Empty) : element.ToString();
                    if (!string.IsNullOrWhiteSpace(id))
                    {
                        result.Add(id.Trim());
                    }
                }
            }

            if (result.Count == 0 &&
                root.TryGetProperty("items", out JsonElement items) &&
                items.ValueKind == JsonValueKind.Array)
            {
                foreach (JsonElement element in items.EnumerateArray())
                {
                    if (element.ValueKind == JsonValueKind.Object)
                    {
                        string id = ReadString(element, "id");
                        if (!string.IsNullOrWhiteSpace(id))
                        {
                            result.Add(id.Trim());
                        }
                    }
                }
            }

            return result;
        }

        private static string 优先取值(params string[] values)
        {
            foreach (string value in values)
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    return value.Trim();
                }
            }

            return string.Empty;
        }

        private static bool 看起来像网址(string value)
        {
            value = (value ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            return value.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
                || value.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
                || value.StartsWith("www.", StringComparison.OrdinalIgnoreCase);
        }

        private static string 补全网址(string value)
        {
            value = (value ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(value))
            {
                return value;
            }

            if (value.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                value.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                return value;
            }

            return "https://" + value;
        }

        private static bool UriEqualsFile(Uri current, Uri expected)
        {
            return string.Equals(current.LocalPath, expected.LocalPath, StringComparison.OrdinalIgnoreCase);
        }

        private static void 打开目标(string target)
        {
            if (string.IsNullOrWhiteSpace(target))
            {
                return;
            }

            Process.Start(new ProcessStartInfo
            {
                FileName = target,
                UseShellExecute = true
            });
        }

        private void ForceStretchLayoutOnLoaded(object? sender, RoutedEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    HorizontalAlignment = HorizontalAlignment.Stretch;
                    VerticalAlignment = VerticalAlignment.Stretch;

                    if (!double.IsNaN(Width))
                    {
                        Width = double.NaN;
                    }

                    if (!double.IsNaN(Height))
                    {
                        Height = double.NaN;
                    }

                    if (Content is FrameworkElement root)
                    {
                        root.HorizontalAlignment = HorizontalAlignment.Stretch;
                        root.VerticalAlignment = VerticalAlignment.Stretch;

                        if (!double.IsNaN(root.Width))
                        {
                            root.Width = double.NaN;
                        }

                        if (!double.IsNaN(root.Height))
                        {
                            root.Height = double.NaN;
                        }

                        if (!double.IsPositiveInfinity(root.MaxWidth))
                        {
                            root.MaxWidth = double.PositiveInfinity;
                        }

                        if (!double.IsPositiveInfinity(root.MaxHeight))
                        {
                            root.MaxHeight = double.PositiveInfinity;
                        }
                    }
                }
                catch
                {
                }
            }), DispatcherPriority.Loaded);
        }
    }
}
