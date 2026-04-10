using Coingcola.模型;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Coingcola.服务
{
    /// <summary>
    /// 常见修复服务。
    /// 
    /// 当前策略：
    /// - 只提供轻量、可解释、低风险的修复动作
    /// - 危险性高、不可逆的动作暂不纳入
    /// - 先做“用户能理解、执行后能感知”的能力
    /// </summary>
    public class 常见修复服务
    {
        /// <summary>
        /// 获取常见修复动作列表。
        /// </summary>
        public List<修复动作项> 获取修复动作列表()
        {
            return new List<修复动作项>
            {
                new 修复动作项
                {
                    Id = "restart_explorer",
                    名称 = "重启资源管理器",
                    分类 = "资源管理器",
                    说明 = "适用于任务栏卡死、桌面不刷新、文件资源管理器异常等常见问题。",
                    风险级别 = "低风险",
                    生效说明 = "执行时桌面和任务栏会短暂闪烁。",
                    按钮文本 = "立即执行"
                },
                new 修复动作项
                {
                    Id = "clear_icon_cache",
                    名称 = "刷新图标缓存",
                    分类 = "资源管理器",
                    说明 = "适用于桌面图标异常、图标空白、图标显示错乱等问题。",
                    风险级别 = "低风险",
                    生效说明 = "执行后会自动重启资源管理器。",
                    按钮文本 = "立即执行"
                },
                new 修复动作项
                {
                    Id = "reset_network_stack",
                    名称 = "重建网络栈",
                    分类 = "网络",
                    说明 = "适用于网络异常、DNS 问题、网络适配器状态错乱等场景。",
                    风险级别 = "管理员权限",
                    生效说明 = "执行后建议重启电脑，以确保完全生效。",
                    按钮文本 = "立即执行"
                },
                new 修复动作项
                {
                    Id = "open_temp_folder",
                    名称 = "打开临时文件目录",
                    分类 = "清理",
                    说明 = "适用于手动清理临时文件、排查安装残留或缓存占用。",
                    风险级别 = "低风险",
                    生效说明 = "只打开目录，不会自动删除文件。",
                    按钮文本 = "打开目录"
                }
            };
        }

        /// <summary>
        /// 执行指定修复动作。
        /// </summary>
        public (bool 成功, string 提示) 执行修复(string id)
        {
            try
            {
                return id switch
                {
                    "restart_explorer" => 重启资源管理器(),
                    "clear_icon_cache" => 刷新图标缓存(),
                    "reset_network_stack" => 重建网络栈(),
                    "open_temp_folder" => 打开临时目录(),
                    _ => (false, "未识别的修复动作。")
                };
            }
            catch (Exception ex)
            {
                return (false, $"执行失败：{ex.Message}");
            }
        }

        /// <summary>
        /// 重启资源管理器。
        /// </summary>
        private (bool 成功, string 提示) 重启资源管理器()
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "taskkill",
                Arguments = "/F /IM explorer.exe",
                UseShellExecute = false,
                CreateNoWindow = true
            })?.WaitForExit();

            System.Threading.Thread.Sleep(500);

            Process.Start(new ProcessStartInfo
            {
                FileName = "explorer.exe",
                UseShellExecute = true
            });

            return (true, "已重启资源管理器。");
        }

        /// <summary>
        /// 刷新图标缓存。
        /// 做法：删除当前用户图标缓存文件后重启资源管理器。
        /// </summary>
        private (bool 成功, string 提示) 刷新图标缓存()
        {
            string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string explorerCacheDir = Path.Combine(localAppData, "Microsoft", "Windows", "Explorer");

            // 先关资源管理器，避免缓存文件占用
            Process.Start(new ProcessStartInfo
            {
                FileName = "taskkill",
                Arguments = "/F /IM explorer.exe",
                UseShellExecute = false,
                CreateNoWindow = true
            })?.WaitForExit();

            if (Directory.Exists(explorerCacheDir))
            {
                foreach (string file in Directory.GetFiles(explorerCacheDir, "iconcache*"))
                {
                    try
                    {
                        File.Delete(file);
                    }
                    catch
                    {
                        // 单个文件删不掉时忽略，尽量继续
                    }
                }
            }

            System.Threading.Thread.Sleep(500);

            Process.Start(new ProcessStartInfo
            {
                FileName = "explorer.exe",
                UseShellExecute = true
            });

            return (true, "已刷新图标缓存，并自动重启资源管理器。");
        }

        /// <summary>
        /// 重建网络栈。
        /// 使用 netsh winsock reset 与 ipconfig /flushdns。
        /// </summary>
        private (bool 成功, string 提示) 重建网络栈()
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = "/c netsh winsock reset && ipconfig /flushdns",
                Verb = "runas",
                UseShellExecute = true,
                CreateNoWindow = false
            });

            return (true, "已发起网络栈重建命令。建议执行后重启电脑。");
        }

        /// <summary>
        /// 打开当前用户临时目录。
        /// </summary>
        private (bool 成功, string 提示) 打开临时目录()
        {
            string tempPath = Path.GetTempPath();

            Process.Start(new ProcessStartInfo
            {
                FileName = tempPath,
                UseShellExecute = true
            });

            return (true, "已打开临时文件目录。");
        }
    }
}