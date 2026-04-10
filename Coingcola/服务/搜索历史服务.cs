using Coingcola.模型;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace Coingcola.服务
{
    public sealed class 搜索历史服务
    {
        private readonly string _目录;
        private readonly string _文件路径;
        private readonly object _锁 = new();

        public 搜索历史服务()
        {
            _目录 = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Coingcola");
            _文件路径 = Path.Combine(_目录, "search_usage.json");
        }

        public 搜索行为信息 获取行为信息(string id)
        {
            var 数据 = 读取数据();
            var 记录 = 数据.记录
                .Where(x => string.Equals(x.ResultId, id, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(x => x.SelectedAt)
                .ToList();

            return new 搜索行为信息
            {
                是否置顶 = 数据.置顶Id列表.Contains(id, StringComparer.OrdinalIgnoreCase),
                选择次数 = 记录.Count,
                最近选择时间 = 记录.FirstOrDefault()?.SelectedAt
            };
        }

        public List<最近使用项> 获取最近使用(int maxCount)
        {
            var 数据 = 读取数据();

            return 数据.记录
                .Where(x => x.Success)
                .GroupBy(x => x.ResultId, StringComparer.OrdinalIgnoreCase)
                .Select(x =>
                {
                    var 最新 = x.OrderByDescending(r => r.SelectedAt).First();

                    return new 最近使用项
                    {
                        Id = 最新.ResultId,
                        标题 = 最新.Title,
                        副标题 = 最新.Subtitle,
                        图标路径 = 最新.IconPath,
                        跳转目标 = 最新.Target,
                        最近使用时间 = 最新.SelectedAt
                    };
                })
                .OrderByDescending(x => x.最近使用时间)
                .Take(maxCount)
                .ToList();
        }

        public void 删除最近使用(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return;
            }

            lock (_锁)
            {
                var 数据 = 读取数据();
                数据.记录 = 数据.记录
                    .Where(x => !string.Equals(x.ResultId, id, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                写入数据(数据);
            }
        }

        public void 清空最近使用()
        {
            lock (_锁)
            {
                var 数据 = 读取数据();
                数据.记录 = new List<搜索历史记录>();
                写入数据(数据);
            }
        }

        public void 记录选择(string query, 统一搜索结果项 item, bool 来自最佳匹配, bool success)
        {
            lock (_锁)
            {
                var 数据 = 读取数据();

                数据.记录.Add(new 搜索历史记录
                {
                    Query = query,
                    ResultId = item.Id,
                    ResultType = item.类型.ToString(),
                    Title = item.标题,
                    Subtitle = item.副标题,
                    Source = item.来源,
                    Target = item.目标,
                    IsBestMatch = 来自最佳匹配,
                    Success = success,
                    SelectedAt = DateTime.Now,
                    IconPath = item.图标路径
                });

                while (数据.记录.Count > 2000)
                {
                    数据.记录.RemoveAt(0);
                }

                写入数据(数据);
            }
        }

        public void 设置置顶(string id, bool pinned)
        {
            lock (_锁)
            {
                var 数据 = 读取数据();

                if (pinned)
                {
                    if (!数据.置顶Id列表.Contains(id, StringComparer.OrdinalIgnoreCase))
                    {
                        数据.置顶Id列表.Add(id);
                    }
                }
                else
                {
                    数据.置顶Id列表 = 数据.置顶Id列表
                        .Where(x => !string.Equals(x, id, StringComparison.OrdinalIgnoreCase))
                        .ToList();
                }

                写入数据(数据);
            }
        }

        private 搜索历史数据 读取数据()
        {
            try
            {
                if (!Directory.Exists(_目录))
                {
                    Directory.CreateDirectory(_目录);
                }

                if (!File.Exists(_文件路径))
                {
                    return new 搜索历史数据();
                }

                string json = File.ReadAllText(_文件路径);
                return JsonSerializer.Deserialize<搜索历史数据>(json) ?? new 搜索历史数据();
            }
            catch
            {
                return new 搜索历史数据();
            }
        }

        private void 写入数据(搜索历史数据 data)
        {
            try
            {
                if (!Directory.Exists(_目录))
                {
                    Directory.CreateDirectory(_目录);
                }

                string json = JsonSerializer.Serialize(data, new JsonSerializerOptions
                {
                    WriteIndented = true
                });

                File.WriteAllText(_文件路径, json);
            }
            catch
            {
            }
        }

        private sealed class 搜索历史数据
        {
            public List<搜索历史记录> 记录 { get; set; } = new();
            public List<string> 置顶Id列表 { get; set; } = new();
        }

        private sealed class 搜索历史记录
        {
            public string Query { get; set; } = string.Empty;
            public string ResultId { get; set; } = string.Empty;
            public string ResultType { get; set; } = string.Empty;
            public string Title { get; set; } = string.Empty;
            public string Subtitle { get; set; } = string.Empty;
            public string Source { get; set; } = string.Empty;
            public string Target { get; set; } = string.Empty;
            public string IconPath { get; set; } = string.Empty;
            public bool IsBestMatch { get; set; }
            public bool Success { get; set; }
            public DateTime SelectedAt { get; set; }
        }
    }

    public sealed class 搜索行为信息
    {
        public bool 是否置顶 { get; set; }
        public int 选择次数 { get; set; }
        public DateTime? 最近选择时间 { get; set; }
    }
}
