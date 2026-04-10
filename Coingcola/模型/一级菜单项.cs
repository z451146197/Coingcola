using System.Collections.Generic;

namespace Coingcola.模型
{
    /// <summary>
    /// 一级菜单项。
    /// 例如：快捷导航、此电脑、系统设置、常用软件、关于
    /// </summary>
    public class 一级菜单项
    {
        /// <summary>
        /// 一级菜单名称
        /// </summary>
        public string 名称 { get; set; } = "";

        /// <summary>
        /// 一级菜单简介
        /// </summary>
        public string 简介 { get; set; } = "";

        /// <summary>
        /// 当前一级菜单下的功能分区（二级菜单）
        /// </summary>
        public List<二级菜单项> 功能分区 { get; set; } = new();
    }
}