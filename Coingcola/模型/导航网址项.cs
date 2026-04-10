using System;

namespace Coingcola.模型
{
    /// <summary>
    /// 网站导航中的单个站点项。
    ///
    /// 这一层只负责“数据长什么样”，不负责具体业务逻辑。
    /// 后续无论是本地保存、云端同步、界面展示，都会复用这个模型。
    /// </summary>
    public class 导航网址项
    {
        /// <summary>
        /// 主键 ID。
        /// 先使用 Guid 字符串，后续如果要和云端 _id 做映射，也能兼容。
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString("N");

        /// <summary>
        /// 网站名称。
        /// 例如：GitHub、知乎、B站。
        /// </summary>
        public string 名称 { get; set; } = "";

        /// <summary>
        /// 最终打开的网址。
        /// 例如：https://github.com
        /// </summary>
        public string 地址 { get; set; } = "";

        /// <summary>
        /// 用户给这个网站设置的关键字。
        /// 例如：github、知乎、oa、邮箱。
        /// </summary>
        public string 关键字 { get; set; } = "";

        /// <summary>
        /// 分组名称。
        /// 第一版先简单使用字符串。
        /// 例如：默认、工作、学习、娱乐。
        /// </summary>
        public string 分组 { get; set; } = "默认";

        /// <summary>
        /// 排序号。
        /// 数字越小越靠前。
        /// </summary>
        public int 排序 { get; set; }

        /// <summary>
        /// 图标地址或本地缓存路径。
        /// 第一版可以先留空，后续抓 favicon 再补。
        /// </summary>
        public string 图标地址 { get; set; } = "";

        /// <summary>
        /// 是否固定在前面。
        /// 后续如果要做“置顶站点”，可以直接用这个字段。
        /// </summary>
        public bool 是否固定 { get; set; }

        /// <summary>
        /// 创建时间。
        /// </summary>
        public DateTime 创建时间 { get; set; } = DateTime.Now;

        /// <summary>
        /// 最近修改时间。
        /// </summary>
        public DateTime 最近修改时间 { get; set; } = DateTime.Now;

        /// <summary>
        /// 最近使用时间。
        /// 用于后续做“最近访问”或排序优化。
        /// </summary>
        public DateTime? 最近使用时间 { get; set; }
    }
}
