namespace Coingcola.模型
{
    /// <summary>
    /// 二级菜单项。
    /// 例如：网站导航、优化体检、常用偏好、装机必备 等
    /// </summary>
    public class 二级菜单项
    {
        /// <summary>
        /// 二级菜单名称
        /// </summary>
        public string 名称 { get; set; } = "";

        /// <summary>
        /// 二级菜单简介
        /// </summary>
        public string 简介 { get; set; } = "";

        /// <summary>
        /// 功能指引文案
        /// </summary>
        public string 功能指引 { get; set; } = "";

        /// <summary>
        /// 当前状态摘要
        /// </summary>
        public string 当前状态 { get; set; } = "";
    }
}