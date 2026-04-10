using Coingcola.模型;
using System.Collections.Generic;

namespace Coingcola.服务
{
    /// <summary>
    /// 软件中心兼容服务。
    /// 当前作为“安装常用软件 / 软件目录”页面的薄适配器，统一转调软件管理服务。
    /// </summary>
    public class 软件中心服务
    {
        private readonly 软件管理服务 _软件管理服务 = new();

        public List<软件安装项> 获取软件列表()
        {
            return _软件管理服务.获取软件列表();
        }

        public List<软件安装项> 搜索软件(string 关键字, string 分类)
        {
            return _软件管理服务.搜索软件(关键字, 分类);
        }

        public bool 系统支持Winget()
        {
            return _软件管理服务.系统支持Winget();
        }

        public int 获取Winget支持数()
        {
            return _软件管理服务.获取Winget支持数();
        }

        public int 获取官网安装数()
        {
            return _软件管理服务.获取官网安装数();
        }

        public (bool 成功, string 提示) 执行安装(string id)
        {
            return _软件管理服务.执行安装(id);
        }

        public string 获取环境提示文本()
        {
            return _软件管理服务.获取环境提示文本();
        }

        public string 生成检索状态文本(string 关键字, string 分类, int 结果数量)
        {
            return _软件管理服务.生成检索状态文本(关键字, 分类, 结果数量);
        }
    }
}

