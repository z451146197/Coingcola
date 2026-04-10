using Coingcola.模型;
using Coingcola.系统工具;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Coingcola.服务
{
    /// <summary>
    /// 快捷导航服务。
    /// 
    /// 当前职责：
    /// 1. 读取网站导航列表
    /// 2. 保存网站导航列表
    /// 3. 新增、更新、删除网站导航项
    /// 4. 根据输入内容识别网址
    /// 5. 直接打开网址
    /// 
    /// 当前先只做“本地版”。
    /// 后续如果接 CloudBase，可以在这个服务里继续扩展同步逻辑。
    /// </summary>
    public class 快捷导航服务
    {
        /// <summary>
        /// 网站导航本地保存文件路径。
        /// </summary>
        private readonly string _网站导航文件路径;

        public 快捷导航服务()
        {
            _网站导航文件路径 = 本地存储工具.获取网站导航文件路径();
        }

        /// <summary>
        /// 获取全部网站列表。
        /// 
        /// 返回前会按：
        /// 1. 是否固定
        /// 2. 排序号
        /// 3. 创建时间
        /// 做一个基础排序。
        /// </summary>
        public List<导航网址项> 获取网站列表()
        {
            List<导航网址项> 列表 =
                本地存储工具.读取Json文件(_网站导航文件路径, new List<导航网址项>());

            return 列表
                .OrderByDescending(x => x.是否固定)
                .ThenBy(x => x.排序)
                .ThenBy(x => x.创建时间)
                .ToList();
        }

        /// <summary>
        /// 保存整个网站列表。
        /// 
        /// 说明：
        /// 第一版我们直接整表写回 JSON。
        /// 对于这种轻量配置数据，这样最简单也最稳。
        /// </summary>
        public void 保存网站列表(List<导航网址项> 列表)
        {
            // 保存前顺便统一修正排序号
            for (int i = 0; i < 列表.Count; i++)
            {
                列表[i].排序 = i + 1;
                列表[i].最近修改时间 = DateTime.Now;
            }

            本地存储工具.写入Json文件(_网站导航文件路径, 列表);
        }

        /// <summary>
        /// 预识别输入内容。
        /// 
        /// 用于界面层在“打开”或“添加”前，
        /// 先看看系统会把这个输入识别成什么。
        /// </summary>
        public 网址识别结果 预识别输入(
            string 用户输入,
            Dictionary<string, string>? 自定义关键字映射 = null)
        {
            return 网址处理工具.识别输入(用户输入, 自定义关键字映射);
        }

        /// <summary>
        /// 根据用户输入，直接打开目标。
        /// 
        /// 行为：
        /// - 识别成网址 -> 打开网址
        /// - 未识别成固定网址 -> 走搜索引擎兜底
        /// </summary>
        public 网址识别结果 打开输入内容(
            string 用户输入,
            Dictionary<string, string>? 自定义关键字映射 = null)
        {
            网址识别结果 结果 = 网址处理工具.识别输入(用户输入, 自定义关键字映射);

            if (!结果.可直接打开 || string.IsNullOrWhiteSpace(结果.最终地址))
            {
                throw new InvalidOperationException("输入内容无法识别为可打开的网址。");
            }

            打开网址(结果.最终地址);

            return 结果;
        }

        /// <summary>
        /// 新增网站导航项。
        /// 
        /// 注意：
        /// 如果识别结果只是搜索引擎兜底结果，
        /// 这里不允许直接保存，避免把“搜索词”误存成网站导航。
        /// </summary>
        public 导航网址项 新增网站(
            string 用户输入,
            string? 自定义名称 = null,
            string? 自定义关键字 = null,
            string 分组 = "默认",
            bool 是否固定 = false,
            Dictionary<string, string>? 自定义关键字映射 = null)
        {
            网址识别结果 识别结果 = 网址处理工具.识别输入(用户输入, 自定义关键字映射);

            if (!识别结果.可保存到导航)
            {
                throw new InvalidOperationException("当前输入未识别为固定网址，不适合直接添加到网站导航。");
            }

            List<导航网址项> 列表 = 获取网站列表();

            导航网址项 新项 = new 导航网址项
            {
                名称 = string.IsNullOrWhiteSpace(自定义名称) ? 识别结果.建议名称 : 自定义名称.Trim(),
                地址 = 识别结果.最终地址,
                关键字 = string.IsNullOrWhiteSpace(自定义关键字) ? 用户输入.Trim() : 自定义关键字.Trim(),
                分组 = string.IsNullOrWhiteSpace(分组) ? "默认" : 分组.Trim(),
                是否固定 = 是否固定,
                排序 = 列表.Count + 1,
                创建时间 = DateTime.Now,
                最近修改时间 = DateTime.Now
            };

            列表.Add(新项);
            保存网站列表(列表);

            return 新项;
        }

        /// <summary>
        /// 更新一个已有网站导航项。
        /// </summary>
        public void 更新网站(导航网址项 更新项)
        {
            List<导航网址项> 列表 = 获取网站列表();

            导航网址项? 旧项 = 列表.FirstOrDefault(x => x.Id == 更新项.Id);
            if (旧项 == null)
            {
                throw new InvalidOperationException("未找到要更新的网站导航项。");
            }

            旧项.名称 = 更新项.名称;
            旧项.地址 = 更新项.地址;
            旧项.关键字 = 更新项.关键字;
            旧项.分组 = 更新项.分组;
            旧项.排序 = 更新项.排序;
            旧项.图标地址 = 更新项.图标地址;
            旧项.是否固定 = 更新项.是否固定;
            旧项.最近修改时间 = DateTime.Now;

            保存网站列表(列表);
        }

        /// <summary>
        /// 删除网站导航项。
        /// </summary>
        public void 删除网站(string 网站Id)
        {
            List<导航网址项> 列表 = 获取网站列表();

            列表 = 列表
                .Where(x => x.Id != 网站Id)
                .ToList();

            保存网站列表(列表);
        }

        /// <summary>
        /// 记录某个网站被使用。
        /// 后续你可以用这个字段做“最近使用”排序。
        /// </summary>
        public void 标记最近使用(string 网站Id)
        {
            List<导航网址项> 列表 = 获取网站列表();

            导航网址项? 目标项 = 列表.FirstOrDefault(x => x.Id == 网站Id);
            if (目标项 == null)
            {
                return;
            }

            目标项.最近使用时间 = DateTime.Now;
            目标项.最近修改时间 = DateTime.Now;

            保存网站列表(列表);
        }

        /// <summary>
        /// 使用系统默认浏览器打开网址。
        /// </summary>
        public void 打开网址(string 地址)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = 地址,
                UseShellExecute = true
            };

            Process.Start(startInfo);
        }
    }
}
