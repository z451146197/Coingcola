using Coingcola.模型;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Coingcola.服务
{
    /// <summary>
    /// 软件管理服务。
    /// 负责软件目录、搜索、安装入口与系统安装能力判断。
    /// </summary>
    public class 软件管理服务
    {
        private static readonly List<软件安装项> _软件列表 = new()
        {
            new 软件安装项
            {
                Id = "chrome",
                名称 = "Google Chrome",
                分类 = "浏览器",
                说明 = "Google 浏览器。",
                来源类型 = "系统安装",
                按钮文本 = "立即安装",
                官网地址 = "https://www.google.com/chrome/",
                WingetId = "Google.Chrome"
            },
            new 软件安装项
            {
                Id = "edge",
                名称 = "Microsoft Edge",
                分类 = "浏览器",
                说明 = "微软浏览器。",
                来源类型 = "系统安装",
                按钮文本 = "立即安装",
                官网地址 = "https://www.microsoft.com/edge",
                WingetId = "Microsoft.Edge"
            },
            new 软件安装项
            {
                Id = "wps",
                名称 = "WPS Office",
                分类 = "办公",
                说明 = "高频办公套件。",
                来源类型 = "官网安装",
                按钮文本 = "打开官网",
                官网地址 = "https://www.wps.cn/",
                WingetId = ""
            },
            new 软件安装项
            {
                Id = "wechat",
                名称 = "微信",
                分类 = "通讯",
                说明 = "常用通讯工具。",
                来源类型 = "系统安装",
                按钮文本 = "立即安装",
                官网地址 = "https://weixin.qq.com/",
                WingetId = "Tencent.WeChat"
            },
            new 软件安装项
            {
                Id = "qq",
                名称 = "QQ",
                分类 = "通讯",
                说明 = "常用即时通讯工具。",
                来源类型 = "官网安装",
                按钮文本 = "打开官网",
                官网地址 = "https://im.qq.com/",
                WingetId = ""
            },
            new 软件安装项
            {
                Id = "7zip",
                名称 = "7-Zip",
                分类 = "压缩",
                说明 = "轻量压缩解压工具。",
                来源类型 = "系统安装",
                按钮文本 = "立即安装",
                官网地址 = "https://www.7-zip.org/",
                WingetId = "7zip.7zip"
            },
            new 软件安装项
            {
                Id = "vscode",
                名称 = "Visual Studio Code",
                分类 = "开发",
                说明 = "轻量代码编辑器。",
                来源类型 = "系统安装",
                按钮文本 = "立即安装",
                官网地址 = "https://code.visualstudio.com/",
                WingetId = "Microsoft.VisualStudioCode"
            },
            new 软件安装项
            {
                Id = "git",
                名称 = "Git",
                分类 = "开发",
                说明 = "版本管理工具。",
                来源类型 = "系统安装",
                按钮文本 = "立即安装",
                官网地址 = "https://git-scm.com/",
                WingetId = "Git.Git"
            },
            new 软件安装项
            {
                Id = "everything",
                名称 = "Everything",
                分类 = "工具",
                说明 = "本地文件极速检索工具。",
                来源类型 = "官网安装",
                按钮文本 = "打开官网",
                官网地址 = "https://www.voidtools.com/zh-cn/",
                WingetId = ""
            },
            new 软件安装项
            {
                Id = "potplayer",
                名称 = "PotPlayer",
                分类 = "播放器",
                说明 = "高频本地视频播放器。",
                来源类型 = "官网安装",
                按钮文本 = "打开官网",
                官网地址 = "https://potplayer.tv/",
                WingetId = ""
            }
        };

        public List<软件安装项> 获取软件列表()
        {
            return _软件列表
                .Select(复制软件项)
                .OrderBy(x => 分类排序值(x.分类))
                .ThenBy(x => x.名称)
                .ToList();
        }

        public List<软件安装项> 搜索软件(string 关键字, string 分类)
        {
            IEnumerable<软件安装项> query = 获取软件列表();

            if (!string.IsNullOrWhiteSpace(分类) && 分类 != "全部")
            {
                query = query.Where(x => string.Equals(x.分类, 分类, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(关键字))
            {
                string key = 规范化文本(关键字);
                query = query.Where(x =>
                    匹配文本(x.名称, key) ||
                    匹配文本(x.分类, key) ||
                    匹配文本(x.说明, key) ||
                    匹配文本(x.来源类型, key));
            }

            return query
                .OrderBy(x => 分类排序值(x.分类))
                .ThenBy(x => x.名称)
                .ToList();
        }

        public bool 系统支持Winget()
        {
            try
            {
                using var process = Process.Start(new ProcessStartInfo
                {
                    FileName = "where",
                    Arguments = "winget",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                });

                if (process == null)
                {
                    return false;
                }

                process.WaitForExit(2000);
                return process.ExitCode == 0;
            }
            catch
            {
                return false;
            }
        }

        public int 获取Winget支持数()
        {
            return _软件列表.Count(x =>
                x.来源类型 == "系统安装" &&
                !string.IsNullOrWhiteSpace(x.WingetId));
        }

        public int 获取官网安装数()
        {
            return _软件列表.Count(x => x.来源类型 == "官网安装");
        }

        public (bool 成功, string 提示) 执行安装(string id)
        {
            软件安装项? item = _软件列表.FirstOrDefault(x => x.Id == id);
            if (item == null)
            {
                return (false, "未找到要执行的软件项。");
            }

            try
            {
                if (item.来源类型 == "系统安装" && !string.IsNullOrWhiteSpace(item.WingetId))
                {
                    if (!系统支持Winget())
                    {
                        if (!string.IsNullOrWhiteSpace(item.官网地址))
                        {
                            打开官网(item.官网地址);
                            return (true, $"当前系统未检测到 winget，已为你打开 {item.名称} 官网。");
                        }

                        return (false, "当前系统未检测到 winget，且未配置官网地址。");
                    }

                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        Arguments = $"/k winget install --id {item.WingetId} -e --accept-package-agreements --accept-source-agreements",
                        Verb = "runas",
                        UseShellExecute = true
                    });

                    return (true, $"已发起安装：{item.名称}。安装窗口会保留，便于查看结果。");
                }

                if (!string.IsNullOrWhiteSpace(item.官网地址))
                {
                    打开官网(item.官网地址);
                    return (true, $"已打开 {item.名称} 官网。");
                }

                return (false, "当前软件暂未配置安装动作。");
            }
            catch (Exception ex)
            {
                return (false, $"执行失败：{ex.Message}");
            }
        }

        public string 获取环境提示文本()
        {
            return 系统支持Winget()
                ? "当前系统已检测到 winget，可直接安装部分软件。安装窗口会保留，便于查看结果。"
                : "当前系统未检测到 winget，系统安装类软件会自动降级为官网安装。";
        }

        public string 生成检索状态文本(string 关键字, string 分类, int 结果数量)
        {
            bool 有关键词 = !string.IsNullOrWhiteSpace(关键字);
            bool 有分类 = !string.IsNullOrWhiteSpace(分类) && 分类 != "全部";

            if (!有关键词 && !有分类)
            {
                return $"当前展示全部软件目录，共 {结果数量} 项。";
            }

            if (有关键词 && 有分类)
            {
                return $"当前分类：{分类}；搜索词：{关键字}；命中 {结果数量} 项。";
            }

            if (有分类)
            {
                return $"当前分类：{分类}；命中 {结果数量} 项。";
            }

            return $"当前搜索词：{关键字}；命中 {结果数量} 项。";
        }

        private 软件安装项 复制软件项(软件安装项 x)
        {
            return new 软件安装项
            {
                Id = x.Id,
                名称 = x.名称,
                分类 = x.分类,
                说明 = x.说明,
                来源类型 = x.来源类型,
                按钮文本 = x.按钮文本,
                官网地址 = x.官网地址,
                WingetId = x.WingetId
            };
        }

        private void 打开官网(string 地址)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = 地址,
                UseShellExecute = true
            });
        }

        private int 分类排序值(string 分类)
        {
            return 分类 switch
            {
                "浏览器" => 1,
                "办公" => 2,
                "通讯" => 3,
                "工具" => 4,
                "压缩" => 5,
                "开发" => 6,
                "播放器" => 7,
                _ => 99
            };
        }

        private bool 匹配文本(string 原文, string 关键词)
        {
            return 规范化文本(原文).Contains(关键词, StringComparison.OrdinalIgnoreCase);
        }

        private string 规范化文本(string 原文)
        {
            return new string((原文 ?? string.Empty)
                .Where(c => !char.IsWhiteSpace(c))
                .ToArray())
                .ToLowerInvariant();
        }
    }
}

