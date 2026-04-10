using System;
using System.IO;
using System.Text.Json;

namespace Coingcola.系统工具
{
    /// <summary>
    /// 本地存储工具。
    /// 
    /// 作用：
    /// 1. 统一管理程序本地数据目录
    /// 2. 负责 JSON 文件的读取与写入
    /// 
    /// 当前阶段我们先把网站导航数据保存到本地 JSON。
    /// 后续再在这个基础上叠加 CloudBase 同步。
    /// </summary>
    public static class 本地存储工具
    {
        /// <summary>
        /// 程序本地数据根目录。
        /// 我们放在当前用户的 AppData\Roaming 下，
        /// 这样不会污染程序安装目录，也更适合以后做多用户隔离。
        /// </summary>
        public static string 获取程序数据目录()
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string 程序目录 = Path.Combine(appData, "Coingcola");

            if (!Directory.Exists(程序目录))
            {
                Directory.CreateDirectory(程序目录);
            }

            return 程序目录;
        }

        /// <summary>
        /// 获取网站导航本地保存文件路径。
        /// </summary>
        public static string 获取网站导航文件路径()
        {
            return Path.Combine(获取程序数据目录(), "website-navs.json");
        }

        /// <summary>
        /// 读取 JSON 文件并反序列化为指定类型。
        /// 如果文件不存在或内容无效，则返回默认值。
        /// </summary>
        public static T 读取Json文件<T>(string 文件路径, T 默认值)
        {
            try
            {
                if (!File.Exists(文件路径))
                {
                    return 默认值;
                }

                string json = File.ReadAllText(文件路径);

                if (string.IsNullOrWhiteSpace(json))
                {
                    return 默认值;
                }

                T? result = JsonSerializer.Deserialize<T>(json, 获取默认序列化选项());

                return result ?? 默认值;
            }
            catch
            {
                // 第一版先兜底返回默认值，避免因为本地文件损坏直接让程序崩掉。
                return 默认值;
            }
        }

        /// <summary>
        /// 将对象写入 JSON 文件。
        /// </summary>
        public static void 写入Json文件<T>(string 文件路径, T 数据)
        {
            string? 目录 = Path.GetDirectoryName(文件路径);

            if (!string.IsNullOrWhiteSpace(目录) && !Directory.Exists(目录))
            {
                Directory.CreateDirectory(目录);
            }

            string json = JsonSerializer.Serialize(数据, 获取默认序列化选项());
            File.WriteAllText(文件路径, json);
        }

        /// <summary>
        /// 默认 JSON 序列化选项。
        /// 
        /// WriteIndented = true：
        /// 便于你直接打开本地文件观察和调试。
        /// </summary>
        private static JsonSerializerOptions 获取默认序列化选项()
        {
            return new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNameCaseInsensitive = true
            };
        }
    }
}