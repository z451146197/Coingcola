using Coingcola.模型;
using System.Collections.Generic;

namespace Coingcola.服务
{
    /// <summary>
    /// 电脑优化兼容服务。
    /// 当前作为“常用设置”页面的薄适配器，统一转调系统设置服务。
    /// </summary>
    public class 电脑优化服务
    {
        public (bool 成功, string 提示, int 已更改数, bool 需要重启资源管理器) 应用全部推荐设置()
        {
            return _系统设置服务.应用全部推荐设置();
        }

        private readonly 系统设置服务 _系统设置服务 = new();

        public List<系统开关项> 获取让电脑更顺手项列表()
        {
            return _系统设置服务.获取设置项列表();
        }

        public (bool 成功, string 提示, bool 需要重启资源管理器) 应用推荐设置(string id)
        {
            return _系统设置服务.应用推荐设置(id);
        }

        public (bool 成功, string 提示, bool 需要重启资源管理器) 切换设置(string id)
        {
            return _系统设置服务.切换设置(id);
        }

        public (bool 成功, string 提示, int 已更改数, bool 需要重启资源管理器) 批量应用推荐设置()
        {
            return _系统设置服务.应用全部推荐设置();
        }

        public (bool 成功, string 提示) 重启资源管理器()
        {
            return _系统设置服务.重启资源管理器();
        }
    }
}
