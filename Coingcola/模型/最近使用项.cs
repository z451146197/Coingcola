namespace Coingcola.模型
{
    public class 最近使用项
    {
        public string Id { get; set; } = "";

        // 旧版字段（兼容老页面）
        public string 名称 { get; set; } = "";
        public string 说明 { get; set; } = "";
        public string 图标文本 { get; set; } = "";
        public string 动作类型 { get; set; } = "";
        public string 目标 { get; set; } = "";
        public string 一级菜单名称 { get; set; } = "";
        public string 二级菜单名称 { get; set; } = "";

        // 新版统一搜索字段（兼容搜索模块）
        public string 标题 { get; set; } = "";
        public string 副标题 { get; set; } = "";
        public string 图标路径 { get; set; } = "";
        public string 跳转目标 { get; set; } = "";
        public System.DateTime 最近使用时间 { get; set; } = System.DateTime.MinValue;
    }
}
