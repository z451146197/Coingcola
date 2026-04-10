using Coingcola.模型;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Coingcola.服务
{
    /// <summary>
    /// 软件更新服务。
    /// 
    /// 当前策略：
    /// 1. 系统更新类软件优先走 winget upgrade
    /// 2. 官网更新类软件直接打开官网
    /// 3. 提供“一键发起全部系统软件更新”
    /// 4. 不做复杂版本解析，不阻塞页面
    /// </summary>
    public class 软件更新服务
    {
        private readonly List<软件更新项> _更新列表 = new()
        {
            new 软件更新项
            {
                Id = "chrome",
                名称 = "Google Chrome",
                分类 = "浏览器",
                更新方式 = "系统更新",
                说明 = "支持通过 winget 发起更新。",
                风险级别 = "低风险",
                按钮文本 = "立即更新",
                WingetId = "Google.Chrome",
                官网地址 = "https://www.google.com/chrome/"
            },
            new 软件更新项
            {
                Id = "7zip",
                名称 = "7-Zip",
                分类 = "压缩",
                更新方式 = "系统更新",
                说明 = "支持通过 winget 发起更新。",
                风险级别 = "低风险",
                按钮文本 = "立即更新",
                WingetId = "7zip.7zip",
                官网地址 = "https://www.7-zip.org/"
            },
            new 软件更新项
            {
                Id = "everything",
                名称 = "Everything",
                分类 = "工具",
                更新方式 = "系统更新",
                说明 = "支持通过 winget 发起更新。",
                风险级别 = "低风险",
                按钮文本 = "立即更新",
                WingetId = "voidtools.Everything",
                官网地址 = "https://www.voidtools.com/zh-cn/"
            },
            new 软件更新项
            {
                Id = "vscode",
                名称 = "Visual Studio Code",
                分类 = "开发",
                更新方式 = "系统更新",
                说明 = "支持通过 winget 发起更新。",
                风险级别 = "低风险",
                按钮文本 = "立即更新",
                WingetId = "Microsoft.VisualStudioCode",
                官网地址 = "https://code.visualstudio.com/"
            },
            new 软件更新项
            {
                Id = "wechat",
                名称 = "微信",
                分类 = "通讯",
                更新方式 = "官网更新",
                说明 = "当前通过官网更新。",
                风险级别 = "低风险",
                按钮文本 = "打开官网",
                WingetId = "",
                官网地址 = "https://weixin.qq.com/"
            },
            new 软件更新项
            {
                Id = "qq",
                名称 = "QQ",
                分类 = "通讯",
                更新方式 = "官网更新",
                说明 = "当前通过官网更新。",
                风险级别 = "低风险",
                按钮文本 = "打开官网",
                WingetId = "",
                官网地址 = "https://im.qq.com/"
            },
            new 软件更新项
            {
                Id = "potplayer",
                名称 = "PotPlayer",
                分类 = "播放器",
                更新方式 = "官网更新",
                说明 = "当前通过官网更新。",
                风险级别 = "低风险",
                按钮文本 = "打开官网",
                WingetId = "",
                官网地址 = "https://potplayer.tv/"
            },
            new 软件更新项
            {
                Id = "wps",
                名称 = "WPS Office",
                分类 = "办公",
                更新方式 = "官网更新",
                说明 = "当前通过官网更新。",
                风险级别 = "低风险",
                按钮文本 = "打开官网",
                WingetId = "",
                官网地址 = "https://www.wps.cn/"
            }
        };

        public List<软件更新项> 获取更新列表()
        {
            return _更新列表
                .OrderBy(x => 分类排序值(x.分类))
                .ThenBy(x => x.名称)
                .ToList();
        }

        public List<软件更新项> 搜索更新项(string 关键词, string 分类 = "全部")
        {
            IEnumerable<软件更新项> 查询 = _更新列表;

            if (!string.IsNullOrWhiteSpace(分类) && 分类 != "全部")
            {
                查询 = 查询.Where(x => x.分类 == 分类);
            }

            if (!string.IsNullOrWhiteSpace(关键词))
            {
                string key = 关键词.Trim();

                查询 = 查询.Where(x =>
                    包含文本(x.名称, key) ||
                    包含文本(x.分类, key) ||
                    包含文本(x.更新方式, key) ||
                    包含文本(x.说明, key));
            }

            return 查询
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

        public int 获取系统更新数量()
        {
            return _更新列表.Count(x => x.更新方式 == "系统更新");
        }

        public int 获取官网更新数量()
        {
            return _更新列表.Count(x => x.更新方式 == "官网更新");
        }

        public (bool 成功, string 提示) 执行更新(string id)
        {
            软件更新项? 项 = _更新列表.FirstOrDefault(x => x.Id == id);
            if (项 == null)
            {
                return (false, "未找到对应软件。");
            }

            try
            {
                if (项.更新方式 == "系统更新" && !string.IsNullOrWhiteSpace(项.WingetId))
                {
                    if (!系统支持Winget())
                    {
                        if (!string.IsNullOrWhiteSpace(项.官网地址))
                        {
                            打开官网(项.官网地址);
                            return (true, $"当前系统未检测到 winget，已为你打开 {项.名称} 官网。");
                        }

                        return (false, "当前系统未检测到 winget，且未配置官网地址。");
                    }

                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        Arguments = $"/k winget upgrade --id {项.WingetId} -e --accept-package-agreements --accept-source-agreements",
                        Verb = "runas",
                        UseShellExecute = true
                    });

                    return (true, $"已发起更新：{项.名称}。更新窗口会保留，便于查看结果。");
                }

                if (!string.IsNullOrWhiteSpace(项.官网地址))
                {
                    打开官网(项.官网地址);
                    return (true, $"已打开 {项.名称} 官网。");
                }

                return (false, "当前软件暂未配置更新动作。");
            }
            catch (Exception ex)
            {
                return (false, $"执行失败：{ex.Message}");
            }
        }

        public (bool 成功, string 提示) 执行全部系统更新()
        {
            try
            {
                if (!系统支持Winget())
                {
                    return (false, "当前系统未检测到 winget，无法发起全部系统软件更新。");
                }

                Process.Start(new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = "/k winget upgrade --all --accept-package-agreements --accept-source-agreements",
                    Verb = "runas",
                    UseShellExecute = true
                });

                return (true, "已发起全部系统软件更新。更新窗口会保留，便于查看结果。");
            }
            catch (Exception ex)
            {
                return (false, $"执行失败：{ex.Message}");
            }
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

        private bool 包含文本(string 原文, string 关键词)
        {
            return (原文 ?? string.Empty)
                .Contains(关键词, StringComparison.OrdinalIgnoreCase);
        }
    }
}