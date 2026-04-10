namespace Coingcola.模型
{
    /// <summary>
    /// 首页“快速操作”区块的单个动作项。
    /// 
    /// 这里不是直接执行复杂逻辑，
    /// 而是优先负责把用户带到最短路径。
    /// </summary>
    public class 快速操作项
    {
        /// <summary>
        /// 唯一标识。
        /// </summary>
        public string Id { get; set; } = "";

        /// <summary>
        /// 显示名称。
        /// </summary>
        public string 名称 { get; set; } = "";

        /// <summary>
        /// 简短说明。
        /// </summary>
        public string 说明 { get; set; } = "";

        /// <summary>
        /// 图标占位文本。
        /// </summary>
        public string 图标文本 { get; set; } = "";

        /// <summary>
        /// 跳转目标一级菜单名称。
        /// </summary>
        public string 一级菜单名称 { get; set; } = "";

        /// <summary>
        /// 跳转目标二级菜单名称。
        /// </summary>
        public string 二级菜单名称 { get; set; } = "";
    }
}